using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_EDITOR
[OpenXRFeature(
    UiName = "OpenXR Clock",
    Desc = "Exposes the OpenXR clock and timestamps from controller pose queries.",
    Company = "Reign",
    Version = "1.0.0",
    FeatureId = "com.reign.openxr.clock",
    BuildTargetGroups = new[]
    {
        BuildTargetGroup.Standalone,
        BuildTargetGroup.Android
    },
    OpenxrExtensionStrings =
        "XR_KHR_win32_convert_performance_counter_time " +
        "XR_KHR_convert_timespec_time"
)]
#endif
public sealed class OpenXRClockFeature : OpenXRFeature
{
    private static OpenXRClockFeature active;
    private static readonly object poseLock = new object();
    private static readonly Dictionary<ulong, PoseSpace> poseSpaces = new Dictionary<ulong, PoseSpace>();
    private static long poseSequence;
    private static int poseCaptureFailed;
    private static ulong leftHandPath, rightHandPath;

    // Keep reverse P/Invoke delegates rooted for the entire native hook lifetime.
    private static GetInstanceProcAddrDelegate nextGetProc;
    private static readonly GetInstanceProcAddrDelegate getProcHook = GetProcHook;
    private static readonly CreateActionSpaceDelegate createSpaceHook = CreateSpaceHook;
    private static readonly LocateSpaceDelegate locateSpaceHook = LocateSpaceHook;
    private static readonly LocateSpacesDelegate locateSpacesHook = LocateSpacesHook;
    private static readonly LocateSpacesDelegate locateSpacesKhrHook = LocateSpacesKhrHook;
    private static readonly DestroySpaceDelegate destroySpaceHook = DestroySpaceHook;
    private static CreateActionSpaceDelegate nextCreateSpace;
    private static LocateSpaceDelegate nextLocateSpace;
    private static LocateSpacesDelegate nextLocateSpaces, nextLocateSpacesKhr;
    private static DestroySpaceDelegate nextDestroySpace;

    private sealed class PoseSpace
    {
        public ulong action, subactionPath;
        public long time, pollTime, sequence;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ActionSpaceCreateInfo
    {
        public int type;
        public IntPtr next;
        public ulong action, subactionPath;
        public Quaternion orientation;
        public Vector3 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SpaceLocation
    {
        public int type;
        public IntPtr next;
        public ulong flags;
        public Quaternion orientation;
        public Vector3 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SpacesLocateInfo
    {
        public int type;
        public IntPtr next;
        public ulong baseSpace;
        public long time;
        public uint spaceCount;
        public IntPtr spaces;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SpaceLocations
    {
        public int type;
        public IntPtr next;
        public uint locationCount;
        public IntPtr locations;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SpaceLocationData
    {
        public ulong flags;
        public Quaternion orientation;
        public Vector3 position;
    }

    private static readonly int locationDataSize = Marshal.SizeOf<SpaceLocationData>();

    private static readonly int locationFlagsOffset =
        (int)Marshal.OffsetOf<SpaceLocation>(nameof(SpaceLocation.flags));

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CreateActionSpaceDelegate(ulong session, IntPtr info, out ulong space);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int LocateSpaceDelegate(ulong space, ulong baseSpace, long time, IntPtr location);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int LocateSpacesDelegate(ulong session, IntPtr info, IntPtr locations);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int DestroySpaceDelegate(ulong space);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int GetInstanceProcAddrDelegate(
        ulong instance,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        out IntPtr function);

    protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
    {
        active = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || (UNITY_ANDROID && !UNITY_EDITOR)
        Volatile.Write(ref clockSource, null);
#endif
        ClearPoseSpaces();
        leftHandPath = rightHandPath = 0;
        nextCreateSpace = null;
        nextLocateSpace = null;
        nextLocateSpaces = nextLocateSpacesKhr = null;
        nextDestroySpace = null;
        if (func == IntPtr.Zero || IntPtr.Size != 8)
            return func;

        nextGetProc = Marshal.GetDelegateForFunctionPointer<GetInstanceProcAddrDelegate>(func);
        return Marshal.GetFunctionPointerForDelegate(getProcHook);
    }

    [MonoPInvokeCallback(typeof(GetInstanceProcAddrDelegate))]
    private static int GetProcHook(ulong xrInstance, string name, out IntPtr function)
    {
        int result = nextGetProc(xrInstance, name, out function);
        if (result < 0 || function == IntPtr.Zero)
            return result;

        switch (name)
        {
            case "xrCreateActionSpace":
                InstallHook(ref function, createSpaceHook, ref nextCreateSpace);
                break;
            case "xrLocateSpace":
                InstallHook(ref function, locateSpaceHook, ref nextLocateSpace);
                break;
            case "xrLocateSpaces":
                InstallHook(ref function, locateSpacesHook, ref nextLocateSpaces);
                break;
            case "xrLocateSpacesKHR":
                InstallHook(ref function, locateSpacesKhrHook, ref nextLocateSpacesKhr);
                break;
            case "xrDestroySpace":
                InstallHook(ref function, destroySpaceHook, ref nextDestroySpace);
                break;
        }
        return result;
    }

    private static void InstallHook<T>(ref IntPtr function, T hook, ref T next) where T : Delegate
    {
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hook);
        if (function == hookPtr)
            return;
        next = Marshal.GetDelegateForFunctionPointer<T>(function);
        function = hookPtr;
    }

    [MonoPInvokeCallback(typeof(CreateActionSpaceDelegate))]
    private static int CreateSpaceHook(ulong session, IntPtr info, out ulong space)
    {
        int result = nextCreateSpace(session, info, out space);
        if (result >= 0 && space != 0 && info != IntPtr.Zero)
        {
            try
            {
                var createInfo = Marshal.PtrToStructure<ActionSpaceCreateInfo>(info);
                lock (poseLock)
                    poseSpaces[space] = new PoseSpace
                    {
                        action = createInfo.action,
                        subactionPath = createInfo.subactionPath
                    };
            }
            catch (Exception)
            {
                // Managed bookkeeping must never throw across a native callback.
                Interlocked.Exchange(ref poseCaptureFailed, 1);
            }
        }
        return result;
    }

    [MonoPInvokeCallback(typeof(LocateSpaceDelegate))]
    private static int LocateSpaceHook(ulong space, ulong baseSpace, long time, IntPtr location)
    {
        long pollTime = ReadPollTime();
        int result = nextLocateSpace(space, baseSpace, time, location);
        try
        {
            // Orientation/position VALID bits; a failed or invalid query invalidates its cached time.
            bool valid = result >= 0 && location != IntPtr.Zero &&
                (Marshal.ReadInt64(location, locationFlagsOffset) & 3) != 0;
            lock (poseLock)
                RecordPoseTime(space, time, pollTime, valid);
        }
        catch (Exception)
        {
            Interlocked.Exchange(ref poseCaptureFailed, 1);
        }
        return result;
    }

    // Called with poseLock held. Native pose queries can run on the render thread.
    private static void RecordPoseTime(ulong space, long time, long pollTime, bool valid)
    {
        if (poseSpaces.TryGetValue(space, out var pose))
        {
            pose.time = valid && time > 0 ? time : 0;
            pose.pollTime = valid ? pollTime : 0;
            pose.sequence = ++poseSequence;
        }
    }

    [MonoPInvokeCallback(typeof(LocateSpacesDelegate))]
    private static int LocateSpacesHook(ulong session, IntPtr info, IntPtr locations)
    {
        long pollTime = ReadPollTime();
        int result = nextLocateSpaces(session, info, locations);
        RecordPoseTimes(info, locations, result, pollTime);
        return result;
    }

    [MonoPInvokeCallback(typeof(LocateSpacesDelegate))]
    private static int LocateSpacesKhrHook(ulong session, IntPtr info, IntPtr locations)
    {
        long pollTime = ReadPollTime();
        int result = nextLocateSpacesKhr(session, info, locations);
        RecordPoseTimes(info, locations, result, pollTime);
        return result;
    }

    private static void RecordPoseTimes(IntPtr info, IntPtr locations, int result, long pollTime)
    {
        try
        {
            if (info == IntPtr.Zero)
                return;
            var input = Marshal.PtrToStructure<SpacesLocateInfo>(info);
            var output = result >= 0 && locations != IntPtr.Zero ?
                Marshal.PtrToStructure<SpaceLocations>(locations) : default;
            if (input.spaces == IntPtr.Zero)
                return;
            lock (poseLock)
            {
                for (int i = 0; i < input.spaceCount; ++i)
                {
                    ulong space = unchecked((ulong)Marshal.ReadInt64(input.spaces, checked(i * 8)));
                    bool valid = result >= 0 && output.locations != IntPtr.Zero &&
                        i < output.locationCount &&
                        (Marshal.ReadInt64(output.locations, checked(i * locationDataSize)) & 3) != 0;
                    RecordPoseTime(space, input.time, pollTime, valid);
                }
            }
        }
        catch (Exception)
        {
            Interlocked.Exchange(ref poseCaptureFailed, 1);
        }
    }

    private static long ReadPollTime()
    {
        try
        {
            return TryGetNowNanoseconds(out long time) ? time : 0;
        }
        catch (Exception)
        {
            // Failure to read the clock must not prevent Unity's native pose query.
            return 0;
        }
    }

    [MonoPInvokeCallback(typeof(DestroySpaceDelegate))]
    private static int DestroySpaceHook(ulong space)
    {
        int result = nextDestroySpace(space);
        if (result >= 0)
        {
            lock (poseLock)
                poseSpaces.Remove(space);
        }
        return result;
    }

    private static void ClearPoseSpaces()
    {
        lock (poseLock)
        {
            poseSpaces.Clear();
            poseSequence = 0;
            Interlocked.Exchange(ref poseCaptureFailed, 0);
        }
    }

    private static void ResetPoseTimes()
    {
        lock (poseLock)
        {
            foreach (var pose in poseSpaces.Values)
                pose.time = pose.pollTime = pose.sequence = 0;
            poseSequence = 0;
        }
    }

    protected override void OnSessionBegin(ulong xrSession) => ResetPoseTimes();
    protected override void OnSessionEnd(ulong xrSession) => ResetPoseTimes();
    protected override void OnSessionDestroy(ulong xrSession) => ClearPoseSpaces();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

    private const string Extension =
        "XR_KHR_win32_convert_performance_counter_time";

    private const string Function =
        "xrConvertWin32PerformanceCounterToTimeKHR";

    [DllImport("kernel32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryPerformanceCounter(out long counter);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int ConvertTimeDelegate(
        ulong instance,
        ref long performanceCounter,
        out long xrTime);

#elif UNITY_ANDROID && !UNITY_EDITOR

    private const string Extension =
        "XR_KHR_convert_timespec_time";

    private const string Function =
        "xrConvertTimespecTimeToTimeKHR";

    // Android ARM64 / x86_64 ABI: both native fields are 64-bit.
    [StructLayout(LayoutKind.Sequential)]
    private struct Timespec
    {
        public long seconds;
        public long nanoseconds;
    }

    private const int CLOCK_MONOTONIC = 1;

    [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
    private static extern int clock_gettime(
        int clockId,
        out Timespec time);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ConvertTimeDelegate(
        ulong instance,
        ref Timespec monotonicTime,
        out long xrTime);

#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || (UNITY_ANDROID && !UNITY_EDITOR)
    // An immutable snapshot lets native/render-thread callbacks read the clock
    // without accessing a Unity ScriptableObject or racing its mutable fields.
    private sealed class ClockSource
    {
        public readonly ulong instance;
        public readonly ConvertTimeDelegate convertTime;

        public ClockSource(ulong instance, ConvertTimeDelegate convertTime)
        {
            this.instance = instance;
            this.convertTime = convertTime;
        }
    }

    private static ClockSource clockSource;
#endif

    protected override bool OnInstanceCreate(ulong xrInstance)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || (UNITY_ANDROID && !UNITY_EDITOR)
        // This example targets Windows x64 and Android ARM64/x86_64.
        if (IntPtr.Size != 8)
            return false;

        if (!OpenXRRuntime.IsExtensionEnabled(Extension))
        {
            Debug.LogError($"OpenXR Clock: {Extension} was not enabled.");
            return false;
        }

        IntPtr resolver = xrGetInstanceProcAddr;
        if (resolver == IntPtr.Zero)
            return false;

        var getProc = Marshal.GetDelegateForFunctionPointer<
            GetInstanceProcAddrDelegate>(resolver);

        int result = getProc(xrInstance, Function, out IntPtr function);

        if (result < 0 || function == IntPtr.Zero)
        {
            Debug.LogError(
                $"OpenXR Clock: resolving {Function} failed: {result}");
            return false;
        }

        var convertTime =
            Marshal.GetDelegateForFunctionPointer<ConvertTimeDelegate>(
                function);

        leftHandPath = StringToPath("/user/hand/left");
        rightHandPath = StringToPath("/user/hand/right");
        active = this;
        Volatile.Write(ref clockSource, new ClockSource(xrInstance, convertTime));
        return true;
#else
        return false;
#endif
    }

    protected override void OnInstanceDestroy(ulong xrInstance)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || (UNITY_ANDROID && !UNITY_EDITOR)
        Volatile.Write(ref clockSource, null);
#endif
        if (active == this)
            active = null;

        ClearPoseSpaces();
        leftHandPath = rightHandPath = 0;
    }

    /// <summary>
    /// Latest intercepted pose-query time for a Unity XR device, in OpenXR nanoseconds.
    /// This is the pose's evaluation/prediction time, not a hardware sensor capture time.
    /// Call on the Unity main thread after XR has initialized. False until a matching
    /// valid query has been observed. poseUsage is a Unity XR feature usage name.
    /// </summary>
    public static bool TryGetPoseTimeNanoseconds(InputDevice device, out long time,
        string poseUsage = "DevicePosition")
        => TryGetQueryTimeNanoseconds(device, false, out time, poseUsage);

    /// <summary>
    /// Actual OpenXR clock time at entry to the latest successful pose query.
    /// This does not include the pose's prediction offset or represent sensor exposure time.
    /// Call on the Unity main thread after XR has initialized.
    /// </summary>
    public static bool TryGetPollTimeNanoseconds(InputDevice device, out long time,
        string poseUsage = "DevicePosition")
        => TryGetQueryTimeNanoseconds(device, true, out time, poseUsage);

    private static bool TryGetQueryTimeNanoseconds(InputDevice device, bool pollTime,
        out long time, string poseUsage)
    {
        time = 0;
        if (active == null || !device.isValid || Volatile.Read(ref poseCaptureFailed) != 0)
            return false;

        ulong action = OpenXRInput.GetActionHandle(device, poseUsage);
        if (action == 0)
            return false;

        var hand = device.characteristics & (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Right);
        ulong path = hand == InputDeviceCharacteristics.Left ? leftHandPath :
            hand == InputDeviceCharacteristics.Right ? rightHandPath : 0;
        if (path == 0)
            return false;

        // Prefer the explicitly handed space. Never substitute another hand's space.
        if (TryGetQueryTimeNanoseconds(action, path, pollTime, out time))
            return true;
        // An observed but currently invalid handed query must not fall back to an old query.
        if (HasPoseSpace(action, path))
            return false;

        // Unity can also create device-specific actions with XR_NULL_PATH. Only use
        // those if the other controller does not resolve to the same action.
        var other = InputDevices.GetDeviceAtXRNode(hand == InputDeviceCharacteristics.Left ?
            XRNode.RightHand : XRNode.LeftHand);
        if (other.isValid && OpenXRInput.GetActionHandle(other, poseUsage) == action)
            return false;
        return TryGetQueryTimeNanoseconds(action, 0, pollTime, out time);
    }

    private static bool HasPoseSpace(ulong action, ulong subactionPath)
    {
        lock (poseLock)
        {
            foreach (var pose in poseSpaces.Values)
                if (pose.action == action && pose.subactionPath == subactionPath)
                    return true;
        }
        return false;
    }

    /// <summary>
    /// Latest pose-query time for an exact native action/subaction pair. XR_NULL_PATH
    /// (zero) matches only unqualified spaces. Uses query order, not the largest
    /// timestamp, since later queries can request earlier prediction times.
    /// </summary>
    public static bool TryGetPoseTimeNanoseconds(ulong action, ulong subactionPath, out long time)
        => TryGetQueryTimeNanoseconds(action, subactionPath, false, out time);

    /// <summary>Actual poll time for an exact native action/subaction pair.</summary>
    public static bool TryGetPollTimeNanoseconds(ulong action, ulong subactionPath, out long time)
        => TryGetQueryTimeNanoseconds(action, subactionPath, true, out time);

    private static bool TryGetQueryTimeNanoseconds(ulong action, ulong subactionPath,
        bool pollTime, out long time)
    {
        time = 0;
        if (action == 0 || Volatile.Read(ref poseCaptureFailed) != 0)
            return false;
        lock (poseLock)
        {
            long sequence = 0;
            foreach (var pose in poseSpaces.Values)
            {
                if (pose.action == action && pose.subactionPath == subactionPath && pose.sequence > sequence)
                {
                    time = pollTime ? pose.pollTime : pose.time;
                    sequence = pose.sequence;
                }
            }
        }
        return time > 0;
    }

    /// <summary>Latest pose evaluation time in the same seconds clock as TryGetNowSeconds.</summary>
    public static bool TryGetPoseTimeSeconds(InputDevice device, out double time,
        string poseUsage = "DevicePosition")
    {
        bool valid = TryGetPoseTimeNanoseconds(device, out long nanoseconds, poseUsage);
        time = nanoseconds * 1e-9;
        return valid;
    }

    /// <summary>Actual poll time in the same seconds clock as TryGetNowSeconds.</summary>
    public static bool TryGetPollTimeSeconds(InputDevice device, out double time,
        string poseUsage = "DevicePosition")
    {
        bool valid = TryGetPollTimeNanoseconds(device, out long nanoseconds, poseUsage);
        time = nanoseconds * 1e-9;
        return valid;
    }

    /// <summary>
    /// Current time in the OpenXR runtime's clock, in nanoseconds.
    /// May be called from native pose-query callbacks on the render thread.
    /// </summary>
    public static bool TryGetNowNanoseconds(out long now)
    {
        now = 0;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || (UNITY_ANDROID && !UNITY_EDITOR)
        var clock = Volatile.Read(ref clockSource);
        if (clock == null)
            return false;
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (!QueryPerformanceCounter(out long counter))
            return false;

        return clock.convertTime(
            clock.instance, ref counter, out now) >= 0;

#elif UNITY_ANDROID && !UNITY_EDITOR
        if (clock_gettime(CLOCK_MONOTONIC, out Timespec timestamp) != 0)
            return false;

        return clock.convertTime(
            clock.instance, ref timestamp, out now) >= 0;
#else
        return false;
#endif
    }

    /// <summary>
    /// Same OpenXR clock origin, expressed in seconds.
    /// </summary>
    public static bool TryGetNowSeconds(out double now)
    {
        if (!TryGetNowNanoseconds(out long nanoseconds))
        {
            now = 0;
            return false;
        }

        now = nanoseconds * 1e-9;
        return true;
    }
}
