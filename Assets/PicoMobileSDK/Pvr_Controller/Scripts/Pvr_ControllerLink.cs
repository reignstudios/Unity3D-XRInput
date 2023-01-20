// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using Pvr_UnitySDKAPI;
using UnityEngine;

public class Pvr_ControllerLink
{

#if ANDROID_DEVICE
    public AndroidJavaClass javaHummingbirdClass;
    public AndroidJavaClass javaPico2ReceiverClass;
    public AndroidJavaClass javaserviceClass;
    public AndroidJavaClass javavractivityclass;
    public AndroidJavaClass javaCVClass;
    public AndroidJavaObject activity;
#endif
    public string gameobjname = "";
    public bool picoDevice = false;
    public string hummingBirdMac;
    public int hummingBirdRSSI;
    public bool goblinserviceStarted = false;
    public bool neoserviceStarted = false;
    public bool controller0Connected = false;
    public bool controller1Connected = false;
    public int mainHandID = 0;
    public int controllerType = 0;
    public ControllerHand Controller0;
    public ControllerHand Controller1;
    public int platFormType = -1; //0 phone，1 Pico Neo DK，2 Pico Goblin 3 Pico Neo
    public int trackingmode = -1; //0:null,1:3dof,2:cv 3:cv+hb 4:cv2 5:cv2+hb
    public int systemProp = -1;   //0：goblin1 1：goblin1 2:neo 3:goblin2 4:neo2
    public int enablehand6dofbyhead = -1;
    public bool switchHomeKey = true;
    private int iPhoneHMDModeEnabled = 0;

    public Pvr_ControllerLink(string name)
    {
        gameobjname = name;
        hummingBirdMac = "";
        hummingBirdRSSI = 0;
        PLOG.I("PvrLog gameobjectname:" +gameobjname);
        StartHummingBirdService();
        Controller0 = new ControllerHand();
        Controller0.Position = new Vector3(0, Pvr_UnitySDKManager.SDK.HeadPose.Position.y, 0)  + new Vector3(-0.1f, -0.3f, 0.3f);
        Controller1 = new ControllerHand();
        Controller1.Position = new Vector3(0, Pvr_UnitySDKManager.SDK.HeadPose.Position.y, 0) + new Vector3(0.1f, -0.3f, 0.3f);
    }

    private void StartHummingBirdService()
    {
#if ANDROID_DEVICE
        try
        {
            UnityEngine.AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            javaHummingbirdClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.HbClientActivity");
            javaCVClass = new AndroidJavaClass("com.picovr.picovrlib.cvcontrollerclient.ControllerClient");
            javavractivityclass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.VrActivity");
            javaserviceClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.UnityClient");
            Pvr_UnitySDKAPI.System.Pvr_SetInitActivity(activity.GetRawObject(), javaHummingbirdClass.GetRawClass());
            int enumindex = (int)GlobalIntConfigs.PLATFORM_TYPE;
            Render.UPvr_GetIntConfig(enumindex, ref platFormType);
            PLOG.I("PvrLog platform" + platFormType);
            enumindex = (int)GlobalIntConfigs.TRACKING_MODE;
            Render.UPvr_GetIntConfig(enumindex, ref trackingmode);
            PLOG.I("PvrLog trackingmode" + trackingmode);
            systemProp = GetSysproc();
            PLOG.I("PvrLog systemProp" + systemProp);
            enumindex = (int) GlobalIntConfigs.ENBLE_HAND6DOF_BY_HEAD;
            Render.UPvr_GetIntConfig(enumindex, ref enablehand6dofbyhead);
            PLOG.I("PvrLog enablehand6dofbyhead" + enablehand6dofbyhead);
            if (trackingmode == 0 || trackingmode == 1 || (trackingmode == 3 || trackingmode == 5) && (systemProp == 1 || systemProp == 3))
            {
                picoDevice = platFormType != 0;
                javaPico2ReceiverClass = new UnityEngine.AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.HbClientReceiver");
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver", activity, gameobjname);
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setPlatformType", platFormType);
            }
            else
            {
                picoDevice = true;
                SetGameObjectToJar(gameobjname);
            }
            Render.UPvr_GetIntConfig((int)GlobalIntConfigs.iPhoneHMDModeEnabled, ref iPhoneHMDModeEnabled);
            if (iPhoneHMDModeEnabled == 1)
            {
                BindService();
            }
            else
            {
                if (IsServiceExisted())
                {
                    BindService();
                }
            }
        }
        catch (AndroidJavaException e)
        {
            PLOG.E("ConnectToAndriod--catch" + e.Message);
        }
#endif
    }

    public bool IsServiceExisted()
    {
        bool service = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref service, javaserviceClass, "isServiceExisted", activity,trackingmode);
#endif
        PLOG.I("PvrLog ServiceExisted ?" + service);
        return service;
    }

    public void SetGameObjectToJar(string name)
    {
        PLOG.I("PvrLog SetGameObjectToJar " + name);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setGameObjectCallback", name);
#endif
    }

    public void BindService()
    {
        PLOG.I("PvrLog Bind Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaserviceClass, "bindService", activity,trackingmode);
#endif
    }

    public void UnBindService()
    {
        PLOG.I("PvrLog UnBind Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaserviceClass, "unbindService", activity,trackingmode);
#endif
    }

    public void StopLark2Receiver()
    {
        PLOG.I("PvrLog StopLark2Receiver");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopReceiver",activity);
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopOnBootReceiver",activity);
#endif
    }

    public void StartLark2Receiver()
    {
        PLOG.I("PvrLog StartLark2Receiver");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver",activity, gameobjname);
#endif
    }

    public void StopLark2Service()
    {
        PLOG.I("PvrLog StopLark2Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopReceiver", activity); 
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "unbindHbService", activity);
#endif
    }

    public void StartLark2Service()
    {
        PLOG.I("PvrLog StartLark2Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver",activity, gameobjname);
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "bindHbService", activity);
#endif
    }

    public int getHandness()
    {
        int handness = -1;
#if ANDROID_DEVICE
        if (iPhoneHMDModeEnabled == 0)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref handness, javavractivityclass, "getPvrHandness", activity);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref handness, javaHummingbirdClass, "getHbHandednessInSP");
        }
#endif
        PLOG.I("PvrLog HandNess =" + handness);
        return handness;
    }

    public void setHandness(int hand)
    {
#if ANDROID_DEVICE
        if (iPhoneHMDModeEnabled == 1)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setHbHandednessInSP", hand);
        }
#endif
    }

    public void StartScan()
    {
        PLOG.I("PvrLog ScanHBController");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "scanHbDevice", true);
#endif
    }

    public void StopScan()
    {
        PLOG.I("PvrLog StopScanHBController");
        if (iPhoneHMDModeEnabled == 0)
        {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "scanHbDevice", false);
#endif
        }
    }

    public int GetSysproc()
    {
        int prop = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref prop, javaserviceClass, "getSysproc");
#endif
        PLOG.I("PvrLog GetSysproc" + prop);
        return prop;
    }

    public void ResetController(int num)
    {
        PLOG.I("PvrLog ResetController" + num);
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "resetControllerSensorState",num);
        }
        if(goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "resetHbSensorState");
		}
#endif
    }

    public void ConnectBLE()
    {
        PLOG.I("PvrLog ConnectHBController" + hummingBirdMac);
        if (hummingBirdMac != "")
        {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "connectHbController", hummingBirdMac);
#endif
        }
    }

    public void DisConnectBLE()
    {
        PLOG.I("PvrLog DisConnectHBController");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "disconnectHbController");
#endif
    }

    public bool StartUpgrade()
    {
        bool start = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref start, javaHummingbirdClass, "startUpgrade");
#endif
        return start;
    }

    public void setBinPath(string path, bool isasset)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setBinPath",path,isasset);
#endif
    }

    public string GetBLEImageType()
    {
        string type = "";
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type, javaHummingbirdClass, "getBLEImageType");
        }
#endif
        return type;
    }

    public long GetBLEVersion()
    {
        long version = 0L;
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<long>(ref version, javaHummingbirdClass, "getBLEVersion");
        }
#endif
        return version;
    }

    public string GetFileImageType()
    {
        string type = "";
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type, javaHummingbirdClass, "getFileImageType");
        }
#endif
        return type;
    }

    public long GetFileVersion()
    {
        long version = 0L;
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<long>(ref version, javaHummingbirdClass, "getFileVersion");
        }
#endif
        return version;
    }

    public int GetControllerConnectionState(int num)
    {
        int state = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, javaCVClass, "getControllerConnectionState",num);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, javaHummingbirdClass, "getHbConnectionState");
        }
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog GetControllerState:" + num + "state:" + state);
        }
        return state;
    }

    public void RebackToLauncher()
    {
        PLOG.I("PvrLog RebackToLauncher");
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startLauncher");
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "startLauncher");
        }
#endif
    }

    public void TurnUpVolume()
    {
        PLOG.I("PvrLog TurnUpVolume");
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "turnUpVolume", activity);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "turnUpVolume", activity);
        }
#endif
    }

    public void TurnDownVolume()
    {
        PLOG.I("PvrLog TurnDownVolume");
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "turnDownVolume", activity);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "turnDownVolume", activity);
        }
#endif
    }

    public float[] GetHBControllerPoseData()
    {
        var data = new float[4] { 0, 0, 0, 1};
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaHummingbirdClass, "getHBSensorPose");
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog HBControllerData" + data[0] + "," + data[1] + "," + data[2] + "," + data[3]);
        }
        return data;
    }

    private float[] fixedState = new float[7] {0, 0, 0, 1, 0, 0, 0};
    public float[] GetControllerFixedSensorState(int hand)
    {
        if (trackingmode == 2 || trackingmode == 3)
        {
            return fixedState;
        }

        var data = fixedState;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerFixedSensorState", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog GetControllerFixedSensorState " + hand + "Rotation:" + data[0] + "," + data[1] + "," + data[2] + "," + data[3] + "Position:" +
                   data[4] + "," + data[5] + "," + data[6]);
        }
        return data;
    }

    private float[] neoposeData = new float[7] { 0, 0, 0, 1, 0, 0, 0 };
    public float[] GetCvControllerPoseData(int hand)
    {
        var data = neoposeData;
#if ANDROID_DEVICE
        if (enablehand6dofbyhead == 1)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerSensorState", hand,Pvr_UnitySDKManager.SDK.headData);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerSensorState", hand);
        }

#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog CVControllerData :" + data[0] + "," + data[1] + "," + data[2] + "," + data[3] + "," +
                   data[4] + "," + data[5] + "," + data[6]);
        }

        if (float.IsNaN(data[0]) || float.IsNaN(data[1]) || float.IsNaN(data[2]) || float.IsNaN(data[3]))
        {
            data[0] = data[1] = data[2] = 0;
            data[3] = 1;
        }
        if (float.IsNaN(data[4]) || float.IsNaN(data[5]) || float.IsNaN(data[6]))
        {
            data[4] = data[5] = data[6] = 0;
        }
        return data;
    }

    private int[] goblinKeyArray = new int[47];
    //touch.x,touch.y,home,app,touch click,volume up,volume down,trigger,power
    public int[] GetHBControllerKeyData()
    {
        var data = goblinKeyArray;
        for (int i = 0; i < 47; i++)
        {
            data[i] = 0;
        }
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaHummingbirdClass, "getHBKeyEventUnityExt");
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog HBControllerKey" + data[0] + data[1] + data[2] + data[3] + data[4] + "," + data[5] + data[6] + data[7] + data[8] + data[9] + ","
                   + data[10] + data[11] + data[12] + data[13] + data[14] + "," + data[15] + data[16] + data[17] + data[18] + data[19] + ","
                   + data[20] + data[21] + data[22] + data[23] + data[24] + "," + data[25] + data[26] + data[27] + data[28] + data[29] + ","
                   + data[30] + data[31] + data[32] + data[33] + data[34] + "," + data[35] + data[36] + data[37] + data[38] + data[39] + ","
                   + data[40] + data[41] + data[42] + data[43] + data[44] + "," + data[45] + data[46]);
        }
        return data;
    }

    public int GetHBKeyValue()
    {
        int key = -1;
#if ANDROID_DEVICE
     Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref key,javaHummingbirdClass, "getTriggerKeyEvent");
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog GoblinControllerTriggerKey:" + key);
        }
        return key;
    }

    private int[] neoKeyArray = new int[67];
    //touch.x,touch.y,home,app,touch click,volume up,volume down,trigger,power,X（A），Y（B），Left，Right
    public int[] GetCvControllerKeyData(int hand)
    {
        var data = neoKeyArray;
        for (int i = 0; i < 67; i++)
        {
            data[i] = 0;
        }

#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerKeyEventUnityExt", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog CVControllerKey hand:" + hand + "-" + data[0] + data[1] + data[2] + data[3] + data[4] + "," + data[5] + data[6] + data[7] + data[8] + data[9] + ","
                   + data[10] + data[11] + data[12] + data[13] + data[14] + "," + data[15] + data[16] + data[17] + data[18] + data[19] + ","
                   + data[20] + data[21] + data[22] + data[23] + data[24] + "," + data[25] + data[26] + data[27] + data[28] + data[29] + ","
                   + data[30] + data[31] + data[32] + data[33] + data[34] + "," + data[35] + data[36] + data[37] + data[38] + data[39] + ","
                   + data[40] + data[41] + data[42] + data[43] + data[44] + "," + data[45] + data[46] + data[47] + data[48] + data[49] + ","
                   + data[50] + data[51] + data[52] + data[53] + data[54] + "," + data[55] + data[56] + data[57] + data[58] + data[59] + ","
                   + data[60] + data[61] + data[62] + data[63] + data[64] + "," + data[65] + data[66]);
        }
        return data;
    }

    private int[] neotriggerV = new int[9];
    public int GetCVTriggerValue(int hand)
    {
        var data = neotriggerV;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerKeyEvent", hand);
#endif
        return data[7];
    }

    public void AutoConnectHbController(int scanTimeMs)
    {
        PLOG.I("PvrLog AutoConnectHbController" + scanTimeMs);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "autoConnectHbController",scanTimeMs,gameobjname);
#endif
    }

    public void StartControllerThread(int headSensorState, int handSensorState)
    {
        if (BoundarySystem.UPvr_IsBoundaryEnable())
        {
            headSensorState = 1;
            handSensorState = 1;
        }
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startControllerThread",headSensorState,handSensorState);
#endif
        PLOG.I("PvrLog StartControllerThread" + headSensorState + handSensorState);
    }
    public void StopControllerThread(int headSensorState, int handSensorState)
    {
        if (BoundarySystem.UPvr_IsBoundaryEnable())
        {
            headSensorState = 1;
            handSensorState = 1;
        }
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "stopControllerThread",headSensorState,handSensorState);
#endif
        PLOG.I("PvrLog StopControllerThread" + headSensorState + handSensorState);
    }

    public void SetUnityVersionToJar(string version)
    {
        if (trackingmode == 4)
        {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setUnityVersion",version);
#endif
            PLOG.I("PvrLog SetUnityVersionToJar" + version);
        }
    }

    public Vector3 GetVelocity(int num)
    {
        var velocity = new float[3] {0, 0, 0};
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref velocity, javaCVClass, "getControllerLinearVelocity", num);
        }
#endif
        return new Vector3(velocity[0],velocity[1],velocity[2]);
    }

    public Vector3 GetAngularVelocity(int num)
    {
        var angulae = new float[3] { 0, 0, 0 };
        try
        {
#if ANDROID_DEVICE

            if (neoserviceStarted)
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref angulae, javaCVClass, "getControllerAngularVelocity", num);
            }
            if (goblinserviceStarted)
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref angulae, javaHummingbirdClass, "getHbAngularVelocity");
            }
#endif
        }
        catch (Exception e)
        {
            PLOG.I(e.ToString());
        }
        PLOG.D("PvrLog Gyro:" + angulae[0] + angulae[1] + angulae[2]);
        if (!float.IsNaN(angulae[0]) && !float.IsNaN(angulae[1]) && !float.IsNaN(angulae[2]))
        {
            return new Vector3(angulae[0], angulae[1], angulae[2]);
        }
        return new Vector3(0, 0, 0);
    }

    
    public Vector3 GetAcceleration(int num)
    {
        var accel = new float[3] { 0, 0, 0 };
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref accel, javaCVClass, "getControllerAcceleration", num);
        }
        if(goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref accel, javaHummingbirdClass, "getHbAcceleration");
        }

#endif
        PLOG.D("PvrLog Acce:" + accel[0] + accel[1] + accel[2]);
        if (!float.IsNaN(accel[0]) && !float.IsNaN(accel[1]) && !float.IsNaN(accel[2]))
        {
            return new Vector3(accel[0], accel[1], accel[2]);
        }
        return new Vector3(0, 0, 0);
    }

    public string GetConnectedDeviceMac()
    {
        string mac = "";
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref mac, javaHummingbirdClass, "getConnectedDeviceMac");
        }
#endif
        PLOG.I("PvrLog ConnectedDeviceMac:" + mac);
        return mac;
    }
  
    public void VibateController(int hand, int strength)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "vibrateControllerStrength", hand, strength);
        }
#endif
        PLOG.I("PvrLog VibateController:" + hand + strength);
    }

    public void VibrateNeo2Controller(float strength, int time, int hand)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "vibrateCV2ControllerStrength",strength,time, hand);
        }
#endif
        PLOG.I("PvrLog VibrateNeo2Controller:" + strength + time + hand);
    }

    public int GetMainControllerIndex()
    {
        int index = 0;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index, javaCVClass, "getMainControllerIndex");
        }
#endif
        PLOG.I("PvrLog GetMainControllerIndex:" + index);
        return index;
    }

    public void SetMainController(int index)
    {
        PLOG.I("PvrLog SetMainController:" + index);
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setMainController",index); 
        }
#endif
    }
    public void ResetHeadSensorForController()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "resetHeadSensorForController");
        }
#endif
        PLOG.I("PvrLog ResetHeadSensorForController:");
    }

    public void GetDeviceVersion(int deviceType)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getDeviceVersion",deviceType); 
        }
#endif
        PLOG.I("PvrLog GetDeviceVersion:" + deviceType);
    }
 
    public void GetControllerSnCode(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerSnCode",controllerSerialNum); 
        }
#endif
    }
 
    public void SetControllerUnbind(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setControllerUnbind",controllerSerialNum); 
        }
#endif
    }

    public void SetStationRestart()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setStationRestart"); 
        }
#endif
    }

    public void StartStationOtaUpdate()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startStationOtaUpdate"); 
        }
#endif
    }
  
    public void StartControllerOtaUpdate(int mode, int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startControllerOtaUpdate",mode,controllerSerialNum); 
        }
#endif
    }
    
    public void EnterPairMode(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "enterPairMode",controllerSerialNum); 
        }
#endif
    }
   
    public void SetControllerShutdown(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setControllerShutdown",controllerSerialNum); 
        }
#endif
    }
    
    public int GetStationPairState()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getStationPairState"); 
        }
#endif
        PLOG.I("PvrLog StationPairState" + index);
        return index;
    }
   
    public int GetStationOtaUpdateProgress()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getStationOtaUpdateProgress"); 
        }
#endif
        PLOG.I("PvrLog StationOtaUpdateProgress" + index);
        return index;
    }
    
    public int GetControllerOtaUpdateProgress()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getControllerOtaUpdateProgress"); 
        }
#endif
        PLOG.I("PvrLog ControllerOtaUpdateProgress" + index);
        return index;
    }

    public void GetControllerVersionAndSN(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerVersionAndSN",controllerSerialNum); 
        }
#endif
    }
    
    public void GetControllerUniqueID()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerUniqueID"); 
        }
#endif
    }
    
    public void InterruptStationPairMode()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "interruptStationPairMode"); 
        }
#endif
    }

    public int GetControllerAbility(int controllerSerialNum)
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getControllerAbility",controllerSerialNum);
        }
#endif
        PLOG.I("PvrLog ControllerAbility:" + index);
        return index;
    }

    public void SwitchHomeKey(bool state)
    {
        PLOG.I("PvrLog SwitchHomeKey:" + state);
        switchHomeKey = state;
    }

    public void SetBootReconnect()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setBootReconnect");
#endif
    }

    //Acquisition of equipment temperature
    public int GetTemperature()
    {
        int value = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value,javaHummingbirdClass, "getTemperature");
#endif
        PLOG.I("PvrLog Temperature:" + value);
        return value;
    }

    //Get the device type
    public int GetDeviceType()
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaHummingbirdClass, "getDeviceType");
#endif
        PLOG.I("PvrLog DeviceType:" + type);
        return type;
    }

    public string GetHummingBird2SN()
    {
        string type = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type,javaHummingbirdClass, "getHummingBird2SN");
#endif
        PLOG.I("PvrLog HummingBird2SN:" + type);
        return type;
    }

    public string GetControllerVersion()
    {
        string type = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type,javaHummingbirdClass, "getControllerVersion");
#endif
        PLOG.I("PvrLog ControllerVersion:" + type);
        return type;
    }

    public bool IsEnbleTrigger()
    {
        bool state = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref state,javaHummingbirdClass, "isEnbleTrigger");
#endif
        PLOG.I("PvrLog IsEnbleTrigger:" + state);
        return state;
    }

    //deviceType: 0：scan both controller；1：scan left controller；2：scan right controller
    public void StartCV2PairingMode(int devicetype)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startCV2PairingMode",devicetype); 
        }
#endif
    }

    public void StopCV2PairingMode(int devicetype)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "stopCV2PairingMode",devicetype); 
        }
#endif
    }
    public int GetControllerBindingState(int id)
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaCVClass, "getControllerBindingState",id);
#endif
        PLOG.I("PvrLog getControllerBindingState:" + type);
        return type;
    }
    public void setIsEnbleHomeKey(bool state)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setIsEnbleHomeKey",state); 
        }
#endif
        PLOG.I("PvrLog setIsEnbleHomeKey:" + state);
    }

    public int getControllerSensorStatus(int id)
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaCVClass, "getControllerSensorStatus",id);
#endif
        PLOG.I("PvrLog getControllerSensorStatus:" + type);
        return type;
    }

}
