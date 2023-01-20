// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using AOT;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

/// <summary>
/// Matches the events in the native plugin.
/// </summary>
public enum RenderEventType
{    
    InitRenderThread = 1024,
    Pause,
    Resume ,
    LeftEyeEndFrame,
    RightEyeEndFrame,
    TimeWarp,
    ResetVrModeParms,
    ShutdownRenderThread ,
    BeginEye,
    EndEye,
    BoundaryRenderLeft,
    BoundaryRenderRight,
    BothEyeEndFrame,
}

/// <summary>
/// Communicates with native plugin functions that run on the rendering thread.
/// </summary>
public static class Pvr_UnitySDKPluginEvent
{
    private const UInt32 IS_DATA_FLAG = 0x80000000;
    private const UInt32 DATA_POS_MASK = 0x40000000;
    private const int DATA_POS_SHIFT = 30;
    private const UInt32 EVENT_TYPE_MASK = 0x3EFF0000;
    private const int EVENT_TYPE_SHIFT = 17;
    private const UInt32 PAYLOAD_MASK = 0x0000FFFF;
    private const int PAYLOAD_SHIFT = 16;

    /// <summary>
    /// Immediately issues the given event.
    /// </summary>
    public static void Issue(RenderEventType eventType)
    {
#if ANDROID_DEVICE
        GL.IssuePluginEvent(Pvr_UnitySDKAPI.System.GetRenderEventFunc(), (int)eventType);
#endif
    }

    private static int EncodeType(int eventType)
    {
        return (int)((UInt32)eventType & ~IS_DATA_FLAG); // make sure upper bit is not set
    }

	private static int EncodeData(int eventId, int eventData, int pos)
    {
        UInt32 data = 0;
        data |= IS_DATA_FLAG;
        data |= (((UInt32)pos << DATA_POS_SHIFT) & DATA_POS_MASK);
        data |= (((UInt32)eventId << EVENT_TYPE_SHIFT) & EVENT_TYPE_MASK);
        data |= (((UInt32)eventData >> (pos * PAYLOAD_SHIFT)) & PAYLOAD_MASK);

        return (int)data;
    }

	private static int DecodeData(int eventData)
    {
        //bool hasData   = (((UInt32)eventData & IS_DATA_FLAG) != 0);
        UInt32 pos = (((UInt32)eventData & DATA_POS_MASK) >> DATA_POS_SHIFT);
        //UInt32 eventId = (((UInt32)eventData & EVENT_TYPE_MASK) >> EVENT_TYPE_SHIFT);
        UInt32 payload = (((UInt32)eventData & PAYLOAD_MASK) << (PAYLOAD_SHIFT * (int)pos));

        return (int)payload;
    }

    private delegate void RenderEventDelegate(int eventId);

    [MonoPInvokeCallback(typeof(RenderEventDelegate))]
    private static void SetSinglePassBeforeForwardOpaque(int eventId)
    {
        Pvr_UnitySDKAPI.System.UPvr_SinglePassBeforeForwardOpaque();
    }

    private static RenderEventDelegate SetSinglePassBeforeForwardOpaqueHandle = new RenderEventDelegate(SetSinglePassBeforeForwardOpaque);
    private static System.IntPtr SetSinglePassBeforeForwardOpaquePtr = Marshal.GetFunctionPointerForDelegate(SetSinglePassBeforeForwardOpaqueHandle);

    public static void SetSinglePassBeforeForwardOpaque(CommandBuffer cmd)
    {
        cmd.IssuePluginEvent(SetSinglePassBeforeForwardOpaquePtr, 0);
    }
}
