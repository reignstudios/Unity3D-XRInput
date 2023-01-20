// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pvr_UnitySDKAPI
{
    public enum DeviceCommand
    {
        SET_PICO_NEO_HMD_BRIGHTNESS = 12,
        SET_PICO_NEO_HMD_SLEEPDELAY = 13
    }
    public enum BrightnessLevel
    {
        VR_BRIGHTNESS_LEVEL_MIN = 1,
        VR_BRIGHTNESS_LEVEL_MAX = 100,
        VR_BRIGHTNESS_LEVEL_DOWN = 1000,
        VR_BRIGHTNESS_LEVEL_UP = 1001,
        VR_BRIGHTNESS_LEVEL_SCREEN_OFF = -100
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VolumePowerBrightness
    {
        public const string LibFileName = "Pvr_UnitySDK";

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_SetInitActivity(IntPtr activity, IntPtr vrActivityClass);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_IsHmdExist();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetHmdScreenBrightness();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_SetHmdScreenBrightness(int brightness);

#if ANDROID_DEVICE
        public AndroidJavaObject activity;
        public static AndroidJavaClass javaSysActivityClass;         
        private static UnityEngine.AndroidJavaClass batteryjavaVrActivityClass;     
        private static UnityEngine.AndroidJavaClass volumejavaVrActivityClass;
#endif
        #region Public Function

        public static bool UPvr_IsHmdExist()
        {
#if ANDROID_DEVICE
            return Pvr_IsHmdExist();
#endif
            return false;
        }

        public static int UPvr_GetHmdScreenBrightness()
        {
#if ANDROID_DEVICE
            return Pvr_GetHmdScreenBrightness();
#endif
            return 0;
        }

        public static bool UPvr_SetHmdScreenBrightness(int brightness)
        {
#if ANDROID_DEVICE
            return Pvr_SetHmdScreenBrightness(brightness);
#endif
            return false;
        }

        public static bool UPvr_SetCommonBrightness(int brightness)
        {
            bool enable = false;
            if (UPvr_IsHmdExist())
            {
                enable = UPvr_SetHmdScreenBrightness(brightness);
            }
            else
            {
                enable = UPvr_SetBrightness(brightness);
            }
            return enable;
        }

        public static int UPvr_GetCommonBrightness()
        {
            int lightness = 0;
            if (UPvr_IsHmdExist())
            {
                lightness = UPvr_GetHmdScreenBrightness();
            }
            else
            {
                lightness = UPvr_GetCurrentBrightness();
            }
            return lightness;
        }

        public static int[] UPvr_GetScreenBrightnessLevel()
        {
            return Pvr_GetScreenBrightnessLevel();
        }

        public static void UPvr_SetScreenBrightnessLevel(int vrBrightness, int level)
        {
            Pvr_SetScreenBrightnessLevel(vrBrightness, level);
        }

        public static bool UPvr_SetDevicePropForUser(DeviceCommand deviceid, string number)
        {
            return setDevicePropForUser(deviceid, number); ;
        }

        public static string UPvr_GetDevicePropForUser(DeviceCommand deviceid)
        {
            return getDevicePropForUser(deviceid);
        }

        public static bool UPvr_InitBatteryClass()
        {
#if ANDROID_DEVICE
            try
            {
                if (javaSysActivityClass == null)
                {
                    javaSysActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.SysActivity");
                }

                if (javaSysActivityClass != null &&Pvr_UnitySDKManager.pvr_UnitySDKRender.activity != null)
                {
                    if (batteryjavaVrActivityClass ==null)
                    {
                        batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.BatteryReceiver");

                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                PLOG.E("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_InitBatteryVolClass()
        {
#if ANDROID_DEVICE
            try
            {
                javaSysActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.SysActivity");
                if (javaSysActivityClass != null &&Pvr_UnitySDKManager.pvr_UnitySDKRender.activity != null)
                {
                   
                    batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.BatteryReceiver");
                    volumejavaVrActivityClass = new AndroidJavaClass("com.psmart.aosoperation.AudioReceiver");
               
                    Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_InitAudioDevice", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity); 
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                PLOG.E("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_StartBatteryReceiver(string startreceivre)
        {
#if ANDROID_DEVICE
            try
            {
               // string startreceivre = PicoVRManager.SDK.gameObject.name;
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
                return true;
            }
            catch (Exception e)
            {
                PLOG.E("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_StopBatteryReceiver()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StopReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                return true;
            }
            catch (Exception e)
            {
                PLOG.E("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_SetBrightness(int brightness)
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_SetScreen_Brightness", brightness, Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                return true;
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static int UPvr_GetCurrentBrightness()
        {
#if ANDROID_DEVICE
            int currentlight = 0;
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentlight, javaSysActivityClass, "Pvr_GetScreen_Brightness", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
            }
            return currentlight;
#endif
            return 0;
        }

        public static int[] Pvr_GetScreenBrightnessLevel()
        {
            int[] currentlight = { 0 };
#if ANDROID_DEVICE

            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int[]>(ref currentlight, javaSysActivityClass, "getScreenBrightnessLevel");
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
            }
#endif
            return currentlight;
        }

        public static void Pvr_SetScreenBrightnessLevel(int vrBrightness, int level)
        {
#if ANDROID_DEVICE
    
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod( javaSysActivityClass, "setScreenBrightnessLevel",vrBrightness,level);
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
            }
#endif
        }

        public static bool UPvr_StartAudioReceiver(string startreceivre)
        {
#if ANDROID_DEVICE
            try
            {
               // string startreceivre = PicoVRManager.SDK.gameObject.name;
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
                return true;
            }
            catch (Exception e)
            {
                PLOG.E("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_StopAudioReceiver()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StopReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                return true;
            }
            catch (Exception e)
            {
                PLOG.E("startReceiver Error :" + e.ToString());
                return false;
            }

#endif
            return true;
        }

        public static int UPvr_GetMaxVolumeNumber()
        {
#if ANDROID_DEVICE
            int maxvolm = 0;
            try
            {  
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref maxvolm, javaSysActivityClass, "Pvr_GetMaxAudionumber");
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
            }
            return maxvolm;
#endif
            return 0;
        }

        public static int UPvr_GetCurrentVolumeNumber()
        {
#if ANDROID_DEVICE
            int currentvolm = 0;
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentvolm, javaSysActivityClass, "Pvr_GetAudionumber");
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
            }
            return currentvolm;
#endif
            return 0;
        }

        public static bool UPvr_VolumeUp()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_UpAudio");
                return true;
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_VolumeDown()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_DownAudio");
                return true;
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_SetVolumeNum(int volume)
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_ChangeAudio", volume);
                return true;
            }
            catch (Exception e)
            {
                PLOG.E(" Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

        public static bool UPvr_SetAudio(string s)
        {
            return false;
        }

        public static bool UPvr_SetBattery(string s)
        {
            return false;
        }

        #endregion

        #region Pravite Function
        private static string getDevicePropForUser(DeviceCommand deviceid)
        {
            string istrue = "0";
#if ANDROID_DEVICE
              Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref istrue, Pvr_UnitySDKRender.javaVrActivityClass, "getDevicePropForUser", (int)deviceid);
#endif
            return istrue;
        }

        private static bool setDevicePropForUser(DeviceCommand deviceid, string number)
        {
            bool istrue = false;
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, Pvr_UnitySDKRender.javaVrActivityClass, "setDevicePropForUser", (int)deviceid, number);
#endif
            return istrue;
        }
        #endregion
    }
}