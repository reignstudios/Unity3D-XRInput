// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using UnityEngine;
using System;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pvr_ControllerManager : MonoBehaviour
{

    /************************************    Properties  *************************************/
    private static Pvr_ControllerManager instance = null;

    public static Pvr_ControllerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindObjectOfType<Pvr_ControllerManager>();
            }
            if (instance == null)
            {
                var go = new GameObject("GameObject");
                instance = go.AddComponent<Pvr_ControllerManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return instance;
        }
    }
    #region Properties
    
    public static Pvr_ControllerLink controllerlink;
    private float cTime = 1.0f;
    private bool stopConnect; 
    public Text toast;
    private bool controllerServicestate;
    private float disConnectTime;
    public bool LengthAdaptiveRay;

    #endregion

    //Service Start Success
    //The service startup is successful and the API interface can be used normally.
    public delegate void PvrServiceStartSuccess();
    public static event PvrServiceStartSuccess PvrServiceStartSuccessEvent;
    //Controller State event
    //1.Goblin controller，"int a"，0：Disconnect 1：Connect
    //2.Neo controller，"int a,int b"，a(0:controller0,1：controller1)，b(0:Disconnect，1：Connect)  
    public delegate void PvrControllerStateChanged(string data);
    public static event PvrControllerStateChanged PvrControllerStateChangedEvent;
    //Master control hand change
    public delegate void ChangeMainControllerCallBack(string index);
    public static event ChangeMainControllerCallBack ChangeMainControllerCallBackEvent;
    //HandNess Changed
    public delegate void ChangeHandNessCallBack(string index);
    public static event ChangeHandNessCallBack ChangeHandNessCallBackEvent;


    //The following is the separation of platform events, suggesting the use of the above events.
    //goblin service bind success
    public delegate void SetHbServiceBindState();
    public static event SetHbServiceBindState SetHbServiceBindStateEvent;
    //neo ControllerThread start-up success
    public delegate void ControllerThreadStartedCallback();
    public static event ControllerThreadStartedCallback ControllerThreadStartedCallbackEvent;
    //neo service Bind success
    public delegate void SetControllerServiceBindState();
    public static event SetControllerServiceBindState SetControllerServiceBindStateEvent;
    //goblin Controller connection status change
    public delegate void ControllerStatusChange(string isconnect);
    public static event ControllerStatusChange ControllerStatusChangeEvent;
    //neo Controller connection status change
    public delegate void SetControllerAbility(string data);
    public static event SetControllerAbility SetControllerAbilityEvent;
    //neo Controller connection status change
    public delegate void SetControllerStateChanged(string data);
    public static event SetControllerStateChanged SetControllerStateChangedEvent;
    //goblin Mac
    public delegate void SetHbControllerMac(string mac);
    public static event SetHbControllerMac SetHbControllerMacEvent;
    //Get the version
    public delegate void ControllerDeviceVersionCallback(string data);
    public static event ControllerDeviceVersionCallback ControllerDeviceVersionCallbackEvent;
    //Acquisition controller SN
    public delegate void ControllerSnCodeCallback(string data);
    public static event ControllerSnCodeCallback ControllerSnCodeCallbackEvent;
    //controller unbundling
    public delegate void ControllerUnbindCallback(string status);
    public static event ControllerUnbindCallback ControllerUnbindCallbackEvent;
    //Station working status.
    public delegate void ControllerStationStatusCallback(string status);
    public static event ControllerStationStatusCallback ControllerStationStatusCallbackEvent;
    //Station is busy.
    public delegate void ControllerStationBusyCallback(string status);
    public static event ControllerStationBusyCallback ControllerStationBusyCallbackEvent;
    //OTA upgrade error
    public delegate void ControllerOtaStartCodeCallback(string data);
    public static event ControllerOtaStartCodeCallback ControllerOtaStartCodeCallbackEvent;
    //controller version and SN 
    public delegate void ControllerDeviceVersionAndSNCallback(string data);
    public static event ControllerDeviceVersionAndSNCallback ControllerDeviceVersionAndSNCallbackEvent;
    //The controller's unique identification code
    public delegate void ControllerUniqueIDCallback(string data);
    public static event ControllerUniqueIDCallback ControllerUniqueIDCallbackEvent;
    //The combined to controller
    public delegate void ControllerCombinedKeyUnbindCallback(string data);
    public static event ControllerCombinedKeyUnbindCallback ControllerCombinedKeyUnbindCallbackEvent;
    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (instance != this)
        {
            PLOG.E("instance object should be a singleton.");
            return;
        }
        if (controllerlink == null)
        {
            controllerlink = new Pvr_ControllerLink(this.gameObject.name);
        }
        else
        {
            BindService();
        }
    }
    // Use this for initialization
    void Start()
    {
#if ANDROID_DEVICE
        if (controllerlink.trackingmode < 2)
        {
            Invoke("CheckControllerService", 10.0f);
        }
#endif
    }
    void Update()
    {
#if UNITY_ANDROID
        if (controllerlink.neoserviceStarted)
        {
            var fixedpose0 = controllerlink.GetControllerFixedSensorState(0);
            if (controllerlink.controller0Connected)
            {
                var pose0 = controllerlink.GetCvControllerPoseData(0);
                controllerlink.Controller0.Rotation.Set(pose0[0], pose0[1], pose0[2], pose0[3]);
                controllerlink.Controller0.Position.Set(pose0[4] / 1000.0f, pose0[5] / 1000.0f, -pose0[6] / 1000.0f);

                var key0 = controllerlink.GetCvControllerKeyData(0);
                Sensor.UPvr_SetReinPosition(fixedpose0[0], fixedpose0[1], fixedpose0[2], fixedpose0[3], fixedpose0[4], fixedpose0[5], fixedpose0[6], 0, true, Convert.ToInt32((Convert.ToString(key0[35]) + "000"), 2));
                TransformData(controllerlink.Controller0, 0, key0);
            }
            else
            {
                Sensor.UPvr_SetReinPosition(fixedpose0[0], fixedpose0[1], fixedpose0[2], fixedpose0[3], fixedpose0[4], fixedpose0[5], fixedpose0[6], 0, false, 0);
            }

            var fixedpose1 = controllerlink.GetControllerFixedSensorState(1);
            if (controllerlink.controller1Connected)
            {
                var pose1 = controllerlink.GetCvControllerPoseData(1);
                controllerlink.Controller1.Rotation.Set(pose1[0], pose1[1], pose1[2], pose1[3]);
                controllerlink.Controller1.Position.Set(pose1[4] / 1000.0f, pose1[5] / 1000.0f, -pose1[6] / 1000.0f);

                var key1 = controllerlink.GetCvControllerKeyData(1);
                Sensor.UPvr_SetReinPosition(fixedpose1[0], fixedpose1[1], fixedpose1[2], fixedpose1[3], fixedpose1[4], fixedpose1[5], fixedpose1[6], 1, true, Convert.ToInt32((Convert.ToString(key1[35]) + "000"), 2));
                TransformData(controllerlink.Controller1, 1, key1);
            }
            else
            {
                Sensor.UPvr_SetReinPosition(fixedpose1[0], fixedpose1[1], fixedpose1[2], fixedpose1[3], fixedpose1[4], fixedpose1[5], fixedpose1[6], 1, false, 0);
            }
        }

        //Goblin controller
        if (controllerlink.goblinserviceStarted && controllerlink.controller0Connected)
        {
            var pose0 = controllerlink.GetHBControllerPoseData();
            controllerlink.Controller0.Rotation.Set(pose0[0], pose0[1], pose0[2], pose0[3]);

            var key0 = controllerlink.GetHBControllerKeyData();
            TransformData(controllerlink.Controller0, 0, key0);
        }

        SetSystemKey();
#endif
    }

    private void OnApplicationPause(bool pause)
    {
        var headdof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
        var handdof = Pvr_UnitySDKManager.SDK.ControllerOnlyrot ? 0 : 1;

        if (pause)
        {
            if (controllerlink.neoserviceStarted)
            {
                controllerlink.SetGameObjectToJar("");
                controllerlink.StopControllerThread(headdof, handdof);
            }
            if (controllerlink.goblinserviceStarted)
            {
                controllerlink.StopLark2Receiver();
            }
        }
        else
        {
            controllerlink.Controller0 = new ControllerHand();
            controllerlink.Controller1 = new ControllerHand();
            if (controllerlink.neoserviceStarted)
            {
                controllerlink.SetGameObjectToJar(controllerlink.gameobjname);
                controllerlink.SetUnityVersionToJar(Pvr_UnitySDKAPI.System.UnitySDKVersion);
                controllerlink.StartControllerThread(headdof, handdof);
            }
            if (controllerlink.goblinserviceStarted)
            {
                controllerlink.StartLark2Receiver();
                controllerlink.controller0Connected = GetControllerConnectionState(0) == 1;
                if (PvrServiceStartSuccessEvent != null)
                    PvrServiceStartSuccessEvent();
            }
        }
    }

    private void OnDestroy()
    {
        controllerlink.UnBindService();
    }
    // Update is called once per frame
    
    void OnApplicationQuit()
    {
        var headdof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
        var handdof = Pvr_UnitySDKManager.SDK.ControllerOnlyrot ? 0 : 1;
      
        if (controllerlink.neoserviceStarted)
        {
            controllerlink.StopControllerThread(headdof, handdof);
        }
            
    }
    
#endregion

    /************************************ Public Interfaces  *********************************/
#region Public Interfaces

   
    public void StopLark2Service()
    {
        if (controllerlink != null)
        {
            controllerlink.StopLark2Service();
        }
    }
  
    public Vector3 GetAngularVelocity(int num)
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAngularVelocity(num);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }
   
    public Vector3 GetAcceleration(int num)
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAcceleration(num);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void BindService()
    {
        if (controllerlink != null)
        {
            controllerlink.BindService();
        }
    }
    public void StartScan()
    {
#if ANDROID_DEVICE
        if (controllerlink != null)
        {
            controllerlink.StartScan();
        }
#endif
    }
    public void StopScan()
    {
        if (controllerlink != null)
        {
            controllerlink.StopScan();
        }
    }
   
    public void ResetController(int num)
    {
        if (controllerlink != null)
        {
            controllerlink.ResetController(num);
        }
    }
    public static int GetControllerConnectionState(int num)
    {
        var sta = controllerlink.GetControllerConnectionState(num);
        return sta;
    }
    public void ConnectBLE()
    {
#if ANDROID_DEVICE
        if (controllerlink != null)
        {
            controllerlink.ConnectBLE();
        }
#endif
    }
    public void DisConnectBLE()
    {
        if (controllerlink != null)
        {
            controllerlink.DisConnectBLE();
        }
    }

    public void SetBinPath(string path, bool isAsset)
    {
        if (controllerlink != null)
        {
            controllerlink.setBinPath(path, isAsset);
        }
    }
    public void StartUpgrade()
    {
        if (controllerlink != null)
        {
            controllerlink.StartUpgrade();
        }
    }
    public static string GetBLEImageType()
    {
        var type = controllerlink.GetBLEImageType();
        return type;
    }
    public static long GetBLEVersion()
    {
        var version = controllerlink.GetBLEVersion();
        return version;
    }
    public static string GetFileImageType()
    {
        var type = controllerlink.GetFileImageType();
        return type;
    }
    public static long GetFileVersion()
    {
        var version = controllerlink.GetFileVersion();
        return version;
    }
    public static void AutoConnectHbController(int scans)
    {
        if (controllerlink != null)
        {
            controllerlink.AutoConnectHbController(scans);
        }
    }
    public static string GetConnectedDeviceMac()
    {
        string mac = "";
        if (controllerlink != null)
        {
            mac = controllerlink.GetConnectedDeviceMac();
        }
        return mac;
    }
    //--------------
    public void setHbControllerMac(string mac)
    {
        PLOG.I("PvrLog HBMacRSSI" + mac);
        controllerlink.hummingBirdMac = mac.Substring(0, 17);
        controllerlink.hummingBirdRSSI = Convert.ToInt16(mac.Remove(0, 18));
        if (SetHbControllerMacEvent != null)
            SetHbControllerMacEvent(mac.Substring(0, 17));

    }
    public int GetControllerRSSI()
    {
        return controllerlink.hummingBirdRSSI;
    }

    public void setHbServiceBindState(string state)
    {
        PLOG.I("PvrLog HBBindCallBack" + state);
        controllerServicestate = true;
        //State: 0- unbound, 1- bound, 2- timed.
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindService", 0.5f);
            controllerlink.goblinserviceStarted = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.goblinserviceStarted = true;
            controllerlink.controller0Connected = GetControllerConnectionState(0) == 1;
            if (SetHbServiceBindStateEvent != null)
            {
                SetHbServiceBindStateEvent();
            }
            if (PvrServiceStartSuccessEvent != null)
            {
                PvrServiceStartSuccessEvent();
            }
        }
    }
    public void setControllerServiceBindState(string state)
    {
        PLOG.I("PvrLog CVBindCallBack" + state);
        //state:0 unbind,1:bind
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindService", 0.5f);
            controllerlink.neoserviceStarted = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.SetUnityVersionToJar(Pvr_UnitySDKAPI.System.UnitySDKVersion);
            controllerlink.neoserviceStarted = true;
            var headdof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
            var handdof = Pvr_UnitySDKManager.SDK.ControllerOnlyrot ? 0 : 1;
            controllerlink.StartControllerThread(headdof, handdof);
            if (SetControllerServiceBindStateEvent != null)
                SetControllerServiceBindStateEvent();

        }

    }
    public void setHbControllerConnectState(string isconnect)
    {
        PLOG.I("PvrLog HBControllerConnect" + isconnect);
        controllerlink.controller0Connected = Convert.ToInt16(isconnect) == 1;
        if (!controllerlink.controller0Connected)
        {
            controllerlink.Controller0 = new ControllerHand();
        }
        else
        {
            ResetController(0);
        }
        //State: 0- disconnect, 1- connected, 2- unknown.
        stopConnect = false;
        if (ControllerStatusChangeEvent != null)
            ControllerStatusChangeEvent(isconnect);
        if (PvrControllerStateChangedEvent != null)
            PvrControllerStateChangedEvent(isconnect);
    }
    
    public void setControllerStateChanged(string state)
    {
        PLOG.I("PvrLog CVControllerStateChanged" + state);
        
        int controller = Convert.ToInt16(state.Substring(0, 1));
        if (controller == 0)
        {
            controllerlink.controller0Connected = Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1)));
            if (!controllerlink.controller0Connected)
            {
                controllerlink.Controller0 = new ControllerHand();
                controllerlink.Controller0.Position = new Vector3(0, Pvr_UnitySDKManager.SDK.HeadPose.Position.y, 0) + new Vector3(-0.1f, -0.3f, 0.3f);
            }
                
        }
        else
        {
            controllerlink.controller1Connected = Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1)));
            if (!controllerlink.controller1Connected)
            {
                controllerlink.Controller1 = new ControllerHand();
                controllerlink.Controller1.Position = new Vector3(0, Pvr_UnitySDKManager.SDK.HeadPose.Position.y, 0) + new Vector3(0.1f, -0.3f, 0.3f);
            }   
        }
        if (Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1))))
        { 
            controllerlink.controllerType = controllerlink.GetDeviceType();
            controllerlink.ResetController(controller);
        }
        controllerlink.mainHandID = Controller.UPvr_GetMainHandNess();
        if (SetControllerStateChangedEvent != null)
            SetControllerStateChangedEvent(state);
        if (PvrControllerStateChangedEvent != null)
            PvrControllerStateChangedEvent(state);

    }
 
    public void setControllerAbility(string data)
    {
        //data format is ID,ability,state.
        //ID 0/1 represents two handles.
        //ability 1/2 1:3dof controller 2. 6dof controller.
        //state 0/1 0: disconnect 1: connection.
        //this callback for setControllerStateChanged extended edition, on the basis of this callback to increase the ability of controller
        PLOG.I("PvrLog setControllerAbility" + data);
        if (SetControllerAbilityEvent != null)
            SetControllerAbilityEvent(data);
    }

    public void controllerThreadStartedCallback()
    {
        PLOG.I("PvrLog ThreadStartSuccess");
        GetCVControllerState();
        if (ControllerThreadStartedCallbackEvent != null)
            ControllerThreadStartedCallbackEvent();
        if (PvrServiceStartSuccessEvent != null)
            PvrServiceStartSuccessEvent();
    }


    public void controllerDeviceVersionCallback(string data)
    {
        PLOG.I("PvrLog VersionCallBack" + data);
        //data format device, deviceVersion
        //device: 0-station 1- controller 0 2- controller 1 deviceVersion: version number.
        if (ControllerDeviceVersionCallbackEvent != null)
            ControllerDeviceVersionCallbackEvent(data);
    }

    public void controllerSnCodeCallback(string data)
    {
        PLOG.I("PvrLog SNCodeCallBack" + data);
        //data formats: controllerSerialNum, controllerSn
        //controllerSerialNum: 0- controller 1 controllerSn: the unique identification of the controller of Sn.
        if (ControllerSnCodeCallbackEvent != null)
            ControllerSnCodeCallbackEvent(data);
    }
   
    public void controllerUnbindCallback(string status)
    {
        PLOG.I("PvrLog ControllerUnBindCallBack" + status);
        // status: 0- all unbind  1- unbind left 2-unbind right 
        if (ControllerUnbindCallbackEvent != null)
            ControllerUnbindCallbackEvent(status);
    }
    
    public void controllerStationStatusCallback(string status)
    {
        PLOG.I("PvrLog StationStatusCallBack" + status);
        //STATION_STATUS{NORMAL = 0, QUERYING = 1, PAIRING = 2, OTA = 3, RESTARTING = 4, CTRLR_UNBINDING = 5, CTRLR_SHUTTING_DOWN = 6};
        if (ControllerStationStatusCallbackEvent != null)
            ControllerStationStatusCallbackEvent(status);
    }
    
    public void controllerStationBusyCallback(string status)
    {
        PLOG.I("PvrLog StationBusyCallBack" + status);
        //STATION_STATUS{NORMAL = 0, QUERYING, PAIRING, OTA, RESTARTING, CTRLR_UNBINDING, CTRLR_SHUTTING_DOWN};
        if (ControllerStationBusyCallbackEvent != null)
            ControllerStationBusyCallbackEvent(status);
    }
   
    public void controllerOTAStartCodeCallback(string data)
    {
        PLOG.I("PvrLog OTAUpdateCallBack" + data);
        //data:deviceType,statusCode
        // deviceType:0-station 1-controller statusCode: 0- upgrade launch success 1- upgrade file not found 2- upgrade file failed to open.
        if (ControllerOtaStartCodeCallbackEvent != null)
            ControllerOtaStartCodeCallbackEvent(data);
    }
 
    public void controllerDeviceVersionAndSNCallback(string data)
    {
        PLOG.I("PvrLog DeviceVersionAndSNCallback" + data);
        //data controllerSerialNum,deviceVersion
        //controllerSerialNum : 0- controller 0 1- controller 1 deviceVersion: version and SN 
        if (ControllerDeviceVersionAndSNCallbackEvent != null)
            ControllerDeviceVersionAndSNCallbackEvent(data);
    }
    
    public void controllerUniqueIDCallback(string data)
    {
        PLOG.I("PvrLog controllerUniqueIDCallback" + data);
        //data controller0ID，controller1ID
        //controller0ID ：ID of controller 0;Controller1ID: ID of controller 1 (if the current controller is not connected, the ID will return to 0)
        if (ControllerUniqueIDCallbackEvent != null)
            ControllerUniqueIDCallbackEvent(data);
    }
    
    public void controllerCombinedKeyUnbindCallback(string controllerSerialNum)
    {
        //controllerSerialNum 0：controller0 1：controller1
        if (ControllerCombinedKeyUnbindCallbackEvent != null)
            ControllerCombinedKeyUnbindCallbackEvent(controllerSerialNum);
    }
    public void setupdateFailed()
    {
        
    }

    public void setupdateSuccess()
    {
        
    }

    public void setupdateProgress(string progress)
    {
        //The upgrade progress 0-100 
    }

    public void setHbAutoConnectState(string state)
    {
        PLOG.I("PvrLog HBAutoConnectState" + state);
        // UNKNOW = 1; the default value
        // NO_DEVICE = 0; No scan to HB controller.
        // ONLY_ONE = 1; Scan only one HB controller.
        // MORE_THAN_ONE = 2; Scan to multiple HB handles.
        // LAST_CONNECTED = 3; Scan the HB controller that was last connected.
        // FACTORY_DEFAULT = 4; Scan the HB controller of the factory binding(temporarily not enabled)
        controllerServicestate = true;
        if (Convert.ToInt16(state) == 0)
        {
            if (GetControllerConnectionState(0) == 0)
            {
                ShowToast(2);
            }
        }
        if (Convert.ToInt16(state) == 2)
        {
            ShowToast(3);
        }
    }

    public void callbackControllerServiceState(string state)
    {
        PLOG.I("PvrLog HBServiceState" + state);
        //state = 0,Non-mobile platform, service is not started.
        //state = 1,The mobile platform, the service is not started, but the system will initiate the service.
        //state = 2,Mobile platform, service apk is not installed, need to install.
        controllerServicestate = true;
        if (Convert.ToInt16(state) == 0)
        {
            ShowToast(0);
        }
        if (Convert.ToInt16(state) == 1)
        {
            BindService();
        }
        if (Convert.ToInt16(state) == 2)
        {
            ShowToast(1);
        }
    }
  
    public void changeMainControllerCallback(string index)
    {
        PLOG.I("PvrLog MainControllerCallBack" + index);

        controllerlink.mainHandID = Convert.ToInt16(index);
        //index = 0/1
        if (ChangeMainControllerCallBackEvent != null)
            ChangeMainControllerCallBackEvent(index);

    }

    public void changeHandnessCallback(string index)
    {
        PLOG.I("PvrLog changeHandnessCallback" + index);
        if (ChangeHandNessCallBackEvent != null)
            ChangeHandNessCallBackEvent(index);
    }

    private void ShowToast(int type)
    {
        if (toast != null)
        {
            switch (type)
            {
                case 0:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip0");
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 1:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip1");
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 2:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip2");
                        AutoConnectHbController(6000);
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 3:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip3");
                        AutoConnectHbController(6000);
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 4:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip4");
                        Invoke("HideToast", 10.0f);
                    }
                    break;
                default:
                    return;
            }
        }
    }

    private void HideToast()
    {
        if (toast != null)
        {
            toast.text = "";
        }
    }

    private void CheckControllerService()
    {
        if (!controllerServicestate)
        {
            ShowToast(4);
        }
    }

    private void GetCVControllerState()
    {
        var state0 = GetControllerConnectionState(0);
        var state1 = GetControllerConnectionState(1);
        PLOG.I("PvrLog CVconnect" + state0 + state1);
        if (state0 == -1 && state1 == -1)
        {
            Invoke("GetCVControllerState", 0.02f);
        }
        if (state0 != -1 && state1 != -1)
        {
            controllerlink.controller0Connected = state0 == 1;
            controllerlink.controller1Connected = state1 == 1;
            if (!controllerlink.controller0Connected && controllerlink.controller1Connected)
            {
                if (Controller.UPvr_GetMainHandNess() == 0)
                {
                    Controller.UPvr_SetMainHandNess(1);
                }
            }

            if (controllerlink.controller0Connected || controllerlink.controller1Connected)
            {
                controllerlink.controllerType = controllerlink.GetDeviceType();
            }

            controllerlink.mainHandID = Controller.UPvr_GetMainHandNess();
        }
    }

    private void SetSystemKey()
    {
        if (controllerlink.switchHomeKey)
        {
            if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME) || Controller.UPvr_GetKeyLongPressed(1, Pvr_KeyCode.HOME))
            {
                if (Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                {
                    Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 0);
                }
                else
                {
                    if (Pvr_UnitySDKManager.SDK.safeToast != null)
                    {
                        if (controllerlink.trackingmode == 4)
                        {
                            Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                        }
                        else
                        {
                            if (Pvr_UnitySDKManager.SDK.safeToast.activeSelf)
                            {
                                Pvr_UnitySDKManager.SDK.safeToast.SetActive(false);
                                Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                            }
                            else
                            {
                                Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 0);
                            }
                            controllerlink.ResetHeadSensorForController();
                        }
                    }
                    else
                    {
                        if (controllerlink.trackingmode == 0 || controllerlink.trackingmode == 1)
                        {
                            Pvr_UnitySDKManager.pvr_UnitySDKSensor.ResetUnitySDKSensor();
                        }
                    }
                }
                if (Pvr_UnitySDKManager.SDK.ControllerOnlyrot || controllerlink.controller0Connected && Controller.UPvr_GetControllerPOS(0).Equals(Vector3.zero) || controllerlink.controller1Connected && Controller.UPvr_GetControllerPOS(1).Equals(Vector3.zero))
                {
                    if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME))
                        ResetController(0);
                    if (Controller.UPvr_GetKeyLongPressed(1, Pvr_KeyCode.HOME))
                        ResetController(1);
                }
            }
        }
        if (controllerlink.picoDevice)
        {
            if (controllerlink.switchHomeKey)
            {
                if (Controller.UPvr_GetKeyClick(0, Pvr_KeyCode.HOME) || Controller.UPvr_GetKeyClick(1, Pvr_KeyCode.HOME) && !stopConnect)
                {
                    controllerlink.RebackToLauncher();
                }
            }
            if (Controller.UPvr_GetKeyClick(0, Pvr_KeyCode.VOLUMEUP) || Controller.UPvr_GetKeyClick(1, Pvr_KeyCode.VOLUMEUP))
            {
                controllerlink.TurnUpVolume();
            }
            if (Controller.UPvr_GetKeyClick(0, Pvr_KeyCode.VOLUMEDOWN) || Controller.UPvr_GetKeyClick(1, Pvr_KeyCode.VOLUMEDOWN))
            {
                controllerlink.TurnDownVolume();
            }
            if (!Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP) && !Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEUP) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEDOWN))
            {
                cTime = 1.0f;
            }
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP) || Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEUP))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnUpVolume();
                }
            }
            if (!Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.HOME) && (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) || Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEDOWN)))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnDownVolume();
                }
            }
        }
        if (controllerlink.goblinserviceStarted)
        {
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME) && Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) && !stopConnect)
            {
                disConnectTime += Time.deltaTime;
                if (disConnectTime > 1.0)
                {
                    DisConnectBLE();
                    controllerlink.hummingBirdMac = "";
                    stopConnect = true;
                    disConnectTime = 0;
                }
            }
        }
    }

    /// <summary>
    /// Data transformation, encapsulating key values as apis
    /// </summary>
    private void TransformData(ControllerHand hand, int handid, int[]data)
    {
        hand.TouchPadPosition.x = data[0];
        hand.TouchPadPosition.y = data[5];

        hand.Home.State =  Convert.ToBoolean(data[10]);
        hand.Home.PressedDown = Convert.ToBoolean(data[11]);
        hand.Home.PressedUp = Convert.ToBoolean(data[12]);
        hand.Home.LongPressed = Convert.ToBoolean(data[13]);
        hand.Home.Click = Convert.ToBoolean(data[14]);

        hand.App.State = Convert.ToBoolean(data[15]);
        hand.App.PressedDown = Convert.ToBoolean(data[16]);
        hand.App.PressedUp = Convert.ToBoolean(data[17]);
        hand.App.LongPressed = Convert.ToBoolean(data[18]);
        hand.App.Click = Convert.ToBoolean(data[19]);

        hand.Touch.State = Convert.ToBoolean(data[20]);
        hand.Touch.PressedDown = Convert.ToBoolean(data[21]);
        hand.Touch.PressedUp = Convert.ToBoolean(data[22]);
        hand.Touch.LongPressed = Convert.ToBoolean(data[23]);
        hand.Touch.Click = Convert.ToBoolean(data[24]);

        hand.VolumeUp.State = Convert.ToBoolean(data[25]);
        hand.VolumeUp.PressedDown = Convert.ToBoolean(data[26]);
        hand.VolumeUp.PressedUp = Convert.ToBoolean(data[27]);
        hand.VolumeUp.LongPressed = Convert.ToBoolean(data[28]);
        hand.VolumeUp.Click = Convert.ToBoolean(data[29]);

        hand.VolumeDown.State = Convert.ToBoolean(data[30]);
        hand.VolumeDown.PressedDown = Convert.ToBoolean(data[31]);
        hand.VolumeDown.PressedUp = Convert.ToBoolean(data[32]);
        hand.VolumeDown.LongPressed = Convert.ToBoolean(data[33]);
        hand.VolumeDown.Click = Convert.ToBoolean(data[34]);

        if (controllerlink.goblinserviceStarted && !controllerlink.neoserviceStarted)
        {
            hand.TriggerNum = controllerlink.GetHBKeyValue();
        }

        if (!controllerlink.goblinserviceStarted && controllerlink.neoserviceStarted)
        {
            hand.TriggerNum = controllerlink.GetCVTriggerValue(handid);
        }
        
        hand.Trigger.State = Convert.ToBoolean(data[35]);
        hand.Trigger.PressedDown = Convert.ToBoolean(data[36]);
        hand.Trigger.PressedUp = Convert.ToBoolean(data[37]);
        hand.Trigger.LongPressed = Convert.ToBoolean(data[38]);
        hand.Trigger.Click = Convert.ToBoolean(data[39]);

        hand.Battery = data[40];

        if (data.Length == 47)
        {
            hand.SwipeDirection = (SwipeDirection)data[45];
            hand.TouchPadClick = (TouchPadClick)data[46];
        }
        else
        {
            if (handid == 0)
            {
                hand.X.State = Convert.ToBoolean(data[45]);
                hand.X.PressedDown = Convert.ToBoolean(data[46]);
                hand.X.PressedUp = Convert.ToBoolean(data[47]);
                hand.X.LongPressed = Convert.ToBoolean(data[48]);
                hand.X.Click = Convert.ToBoolean(data[49]);

                hand.Y.State = Convert.ToBoolean(data[50]);
                hand.Y.PressedDown = Convert.ToBoolean(data[51]);
                hand.Y.PressedUp = Convert.ToBoolean(data[52]);
                hand.Y.LongPressed = Convert.ToBoolean(data[53]);
                hand.Y.Click = Convert.ToBoolean(data[54]);

                hand.Left.State = Convert.ToBoolean(data[60]);
                hand.Left.PressedDown = Convert.ToBoolean(data[61]);
                hand.Left.PressedUp = Convert.ToBoolean(data[62]);
                hand.Left.LongPressed = Convert.ToBoolean(data[63]);
                hand.Left.Click = Convert.ToBoolean(data[64]);
            }

            if (handid == 1)
            {
                hand.A.State = Convert.ToBoolean(data[45]);
                hand.A.PressedDown = Convert.ToBoolean(data[46]);
                hand.A.PressedUp = Convert.ToBoolean(data[47]);
                hand.A.LongPressed = Convert.ToBoolean(data[48]);
                hand.A.Click = Convert.ToBoolean(data[49]);

                hand.B.State = Convert.ToBoolean(data[50]);
                hand.B.PressedDown = Convert.ToBoolean(data[51]);
                hand.B.PressedUp = Convert.ToBoolean(data[52]);
                hand.B.LongPressed = Convert.ToBoolean(data[53]);
                hand.B.Click = Convert.ToBoolean(data[54]);

                hand.Right.State = Convert.ToBoolean(data[55]);
                hand.Right.PressedDown = Convert.ToBoolean(data[56]);
                hand.Right.PressedUp = Convert.ToBoolean(data[57]);
                hand.Right.LongPressed = Convert.ToBoolean(data[58]);
                hand.Right.Click = Convert.ToBoolean(data[59]);
            }

            hand.SwipeDirection = (SwipeDirection)data[65];
            hand.TouchPadClick = (TouchPadClick) data[66];
        }
    }

#endregion

}
