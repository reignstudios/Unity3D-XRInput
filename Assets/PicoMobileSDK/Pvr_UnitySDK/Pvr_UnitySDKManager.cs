// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_UnitySDKManager : MonoBehaviour
{

    /************************************    Properties  *************************************/
    #region Properties
    public static PlatForm platform;
    bool BattEnable = false;

    private static Pvr_UnitySDKManager sdk = null;
    public static Pvr_UnitySDKManager SDK
    {
        get
        {
            if (sdk == null)
            {
                sdk = UnityEngine.Object.FindObjectOfType<Pvr_UnitySDKManager>();
            }
            if (sdk == null)
            {
                var go = new GameObject("Pvr_UnitySDKManager");
                sdk = go.AddComponent<Pvr_UnitySDKManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return sdk;
        }
    }

    // Sensor
    [HideInInspector]
    public static Pvr_UnitySDKSensor pvr_UnitySDKSensor;
    [HideInInspector]
    public Pvr_UnitySDKPose HeadPose;
    [HideInInspector]
    public bool reStartHead = false;
    //render
    [HideInInspector]
    public static Pvr_UnitySDKRender pvr_UnitySDKRender;

    [SerializeField]
    private float eyeVFov = 90.0f;
    [HideInInspector]
    public float EyeVFoV
    {
        get
        {
            return eyeVFov;
        }
        set
        {
            if (value != eyeVFov)
            {
                eyeVFov = value;
            }
        }
    }
    [SerializeField]
    private float eyeHFov = 90.0f;
    [HideInInspector]
    public float EyeHFoV
    {
        get
        {
            return eyeHFov;
        }
        set
        {
            if (value != eyeHFov)
            {
                eyeHFov = value;
            }
        }
    }
    [HideInInspector]
    public float EyesAspect = 1.0f;
    
    [HideInInspector]
    public static int eyeTextureCount = 6;
    [HideInInspector]
    public RenderTexture[] eyeTextures;// = new RenderTexture[eyeTextureCount];
    [HideInInspector]
    public int[] eyeTextureIds;
    [HideInInspector]
    public int currEyeTextureIdx = 0;
    [HideInInspector]
    public int nextEyeTextureIdx = 1;
    [HideInInspector]
    public RenderTexture[] overlayTextures;// = new RenderTexture[eyeTextureCount];
    [HideInInspector]
    public int[] overlayTextureIds;
    [HideInInspector]
    public int overlayCamNum = 0;

    [HideInInspector]
    public int resetRot = 0;
    [HideInInspector]
    public int resetPos = 0;
    [HideInInspector]
    public int posStatus = 0;
    [HideInInspector]
    public bool isPUI;
    [HideInInspector]
    public Vector3 resetBasePos = new Vector3();
    [HideInInspector]
    public Vector3 resetCol0Pos = new Vector3();
    [HideInInspector]
    public Vector3 resetCol1Pos = new Vector3();
    [HideInInspector]
    public int trackingmode = -1;
    [HideInInspector]
    public int systemprop = -1;
    [HideInInspector]
    public bool systemFPS = false;

    [HideInInspector]
    public float[] headData = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
    
    [SerializeField]
    private bool rotfoldout = false;

    public bool Rotfoldout
    {
        get { return rotfoldout; }
        set
        {
            if (value != rotfoldout)
                rotfoldout = value;
        }
    }

    [SerializeField]
    private bool hmdOnlyrot =false;

    public bool HmdOnlyrot
    {
        get { return hmdOnlyrot; }
        set
        {
            if (value != hmdOnlyrot)
                hmdOnlyrot = value;
        }
    }
    [SerializeField]
    private bool controllerOnlyrot = false;

    public bool ControllerOnlyrot
    {
        get { return controllerOnlyrot; }
        set
        {
            if (value != controllerOnlyrot)
                controllerOnlyrot = value;
        }
    }
    /// <summary>
    /// Represents how the SDK is reporting pose data.(EyeLevel for Default)
    /// </summary>
    [SerializeField]
    private TrackingOrigin trackingOrigin = TrackingOrigin.EyeLevel;
    public TrackingOrigin TrackingOrigin
    {
        get
        {
            return this.trackingOrigin;
        }

        set
        {
            if (value != this.trackingOrigin)
            {
                this.trackingOrigin = value;

                Pvr_UnitySDKAPI.Sensor.UPvr_SetTrackingOriginType(value);
            }
        }
    }

    /// <summary>
    /// Reset Tracker OnLoad
    /// </summary>
    public bool ResetTrackerOnLoad = false;

    // Becareful, you must excute this before Pvr_UnitySDKManager script
    public void ChangeDefaultCustomRtSize(int w, int h)
    {
        Pvr_UnitySDKProjectSetting.GetProjectConfig().customRTSize = new Vector2(w, h);
    }

    [SerializeField]
    private float rtScaleFactor = 1;
    public float RtScaleFactor
    {
        get
        {
            return rtScaleFactor;
        }
        set
        {
            if (value != rtScaleFactor)
            {
                rtScaleFactor = value;

                if (pvr_UnitySDKRender != null)
                {
                    pvr_UnitySDKRender.ReCreateEyeBuffer();
                }
            }
        }
    }

    [HideInInspector]
    public int RenderviewNumber = 0;
    public Vector3 EyeOffset(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeOffset : rightEyeOffset;
    }
    [HideInInspector]
    public Vector3 leftEyeOffset;
    [HideInInspector]
    public Vector3 rightEyeOffset;
    public Rect EyeRect(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeRect : rightEyeRect;
    }
    [HideInInspector]
    public Rect leftEyeRect;
    [HideInInspector]
    public Rect rightEyeRect;
    [HideInInspector]
    public Matrix4x4 leftEyeView;
    [HideInInspector]
    public Matrix4x4 rightEyeView;

    // unity editor
    [HideInInspector]
    public Pvr_UnitySDKEditor pvr_UnitySDKEditor;
    [SerializeField]
    private bool vrModeEnabled = true;
    [HideInInspector]
    public bool VRModeEnabled
    {

        get
        {
            return vrModeEnabled;
        }
        set
        {
            if (value != vrModeEnabled)
                vrModeEnabled = value;

        }
    }
    [HideInInspector]
    public Material Eyematerial;
    [HideInInspector]
    public Material Middlematerial;
    [HideInInspector]
    public bool picovrTriggered { get; set; }
    [HideInInspector]
    public bool newPicovrTriggered = false;

    // FPS
    [SerializeField]
    private bool showFPS;
    public bool ShowFPS
    {
        get
        {
            return showFPS;
        }
        set
        {
            if (value != showFPS)
            {
                showFPS = value;
            }
        }
    }
    //6dof recenter
    [SerializeField]
    private bool sixDofPosReset;
    public bool SixDofPosReset
    {
        get
        {
            return sixDofPosReset;
        }
        set
        {
            if (value != sixDofPosReset)
            {
                sixDofPosReset = value;
            }
        }
    }

    //show safe panel
    [SerializeField]
    private bool showSafePanel;
    public bool ShowSafePanel
    {
        get
        {
            return showSafePanel;
        }
        set
        {
            if (value != showSafePanel)
            {
                showSafePanel = value;
            }
        }
    }
    
    //use default range 0.8m
    [SerializeField]
    private bool defaultRange;
    public bool DefaultRange
    {
        get
        {
            return defaultRange;
        }
        set
        {
            if (value != defaultRange)
            {
                defaultRange = value;
            }
        }
    }
    //custom range
    [SerializeField]
    private float customRange = 0.8f;
    public float CustomRange
    {
        get
        {
            return customRange;
        }
        set
        {
            if (value != customRange)
            {
                customRange = value;
            }
        }
    }
    //Moving Ratios
    [SerializeField]
    private float movingRatios;
    public float MovingRatios
    {
        get
        {
            return movingRatios;
        }
        set
        {
            if (value != movingRatios)
            {
                movingRatios = value;
            }
        }
    }
    // screenFade
    [SerializeField]
    private bool screenFade = false;
    public bool ScreenFade
    {
        get
        {
            return screenFade;
        }
        set
        {
            if (value != screenFade)
            {
                screenFade = value;
            }
        }
    }
    //Neck model
    [HideInInspector]
    public Vector3 neckOffset = new Vector3(0, 0.075f, 0.0805f);


    [SerializeField]
    private bool pVRNeck = true;
    public bool PVRNeck
    {
        get { return pVRNeck; }
        set
        {
            if (value != pVRNeck)
                pVRNeck = value;
        }
    }
    [HideInInspector]
    public bool UseCustomNeckPara = false;

    // life
    [HideInInspector]
    public bool onResume = false;
    [HideInInspector]
    public bool isEnterVRMode = false;

    private GameObject safeArea;
    [HideInInspector]
    public GameObject safeToast;
    [HideInInspector]
    public GameObject resetPanel;
    private GameObject safePanel;
    public bool isHasController = false;
    public Pvr_UnitySDKConfigProfile pvr_UnitySDKConfig;

    private GameObject calltoast;
    private GameObject msgtoast;
    private GameObject lowhmdBatterytoast;
    private GameObject lowphoneBatterytoast;
    private GameObject LowPhoneHealthtoast;
    private GameObject LowcontrollerBatterytoast;
    private bool lowControllerpowerstate = false;
    private float controllerpowershowtime = 0f;
    private bool UseToast = true;
    private int iPhoneHMDModeEnabled;

    private GameObject G3LiteTips;
    
    [SerializeField]
    private bool monoscopic = false;

    [HideInInspector]
    public bool Monoscopic
    {
        get { return monoscopic; }
        set
        {
            if (value != monoscopic)
            {
                monoscopic = value;
                // if monoscopick change, reset mono mode
                Pvr_UnitySDKAPI.Render.UPvr_SetMonoMode(monoscopic);
            }
        }
    }

    [SerializeField]
    private bool copyrightprotection = false;

    private bool mIsAndroid7 = false;
    public static Func<bool> eventEnterVRMode;

    private static StereoRenderingPathPico stereoRenderPath = StereoRenderingPathPico.MultiPass;
    public static StereoRenderingPathPico StereoRenderPath
    {
        get
        {
            return stereoRenderPath;
        }
    }
    public static SDKStereoRendering StereoRendering { get; private set; }

    #endregion


    /************************************ Private Interfaces  *********************************/

    private bool SDKManagerInit()
    {
        if (SDKManagerInitConfigProfile())
        {
            mIsAndroid7 = SystemInfo.operatingSystem.Contains("Android OS 7.");
            PLOG.I("Android 7 = " + mIsAndroid7);
#if UNITY_EDITOR
            if (SDKManagerInitEditor())
                return true;
            else
                return false;
#else

            if (SDKManagerInitCoreAbility())

                return true;
            else
                return false;
#endif
        }
        else
            return false;
    }

    private bool SDKManagerInitCoreAbility()
    {
        Pvr_UnitySDKAPI.Sensor.UPvr_SetTrackingOriginType(this.trackingOrigin);
        Pvr_UnitySDKAPI.Render.UPvr_SetMonoMode(this.monoscopic);

        if (pvr_UnitySDKRender == null)
        {
            PLOG.I("pvr_UnitySDKRender  init");
            pvr_UnitySDKRender = new Pvr_UnitySDKRender();
        }
        else
        {
            pvr_UnitySDKRender.Init();
        }

        HeadPose = new Pvr_UnitySDKPose(Vector3.zero, Quaternion.identity);
        if (pvr_UnitySDKSensor == null)
        {
            PLOG.I("pvr_UnitySDKSensor init");
            pvr_UnitySDKSensor = new Pvr_UnitySDKSensor();
        }
        else
        {
            pvr_UnitySDKSensor.InitUnitySDK6DofSensor();
            if (trackingmode == 2 || trackingmode == 3)
            {
                pvr_UnitySDKSensor.InitUnitySDKSensor();
            }

        }
        Pvr_UnitySDKAPI.System.UPvr_StartHomeKeyReceiver(this.gameObject.name);

        return true;
    }

    public void smsReceivedCallback(string msg)
    {
        PLOG.I("PvrLog MSG" + msg);

        var Jdmsg = LitJson.JsonMapper.ToObject(msg);

        string name = "";
        if (msg.Contains("messageSender"))
        {
            name = (string)Jdmsg["messageSender"];
        }

        string number = "";
        if (msg.Contains("messageAdr"))
        {
            number = (string)Jdmsg["messageAdr"];
            if (number.Substring(0, 3) == "+82")
            {
                number = "0" + number.Remove(0, 3);
                number = TransformNumber(number);
            }
            else
            {
                if (number.Substring(0, 1) != "+")
                {
                    number = TransformNumber(number);
                }
            }
        }
        string body = "";
        if (msg.Contains("messageBody"))
        {
            body = (string)Jdmsg["messageBody"];
        }
        //DateTime dt = DateTime.Parse("1970-01-01 00:00:00").AddMilliseconds(Convert.ToInt64((Int64)Jdmsg["messageTime"]));
        //string time = dt.ToString("yyyy-MM-dd HH:mm:ss");
        if (UseToast)
        {
            msgtoast.transform.Find("number").GetComponent<Text>().text = number;
            msgtoast.transform.Find("name").GetComponent<Text>().text = name;
            if (name.Length == 0)
            {
                msgtoast.transform.Find("number").transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                msgtoast.transform.Find("number").transform.localPosition = new Vector3(60, 0, 0);
            }

            StartCoroutine(ToastManager(2, true, 0f));
            StartCoroutine(ToastManager(2, false, 5.0f));
        }
    }

    public void phoneStateCallback(string state)
    {
        PLOG.I("PvrLog phone" + state);

        var Jdstate = LitJson.JsonMapper.ToObject(state);

        string number = "";
        if (state.Contains("phoneNumber"))
        {
            number = (string)Jdstate["phoneNumber"];
            
            if (number.Substring(0, 3) == "+82")
            {
                number = "0" + number.Remove(0, 3);
                number =TransformNumber(number);
            }
            else
            {
                if (number.Substring(0, 1) != "+")
                {
                    number = TransformNumber(number);
                }
            }
        }
        string name = "";
        if (state.Contains("contactName"))
        {
           name = (string)Jdstate["contactName"];
        }
        
        if (UseToast)
        {
            calltoast.transform.Find("number").GetComponent<Text>().text = number;
            calltoast.transform.Find("name").GetComponent<Text>().text = name;
            if (name.Length == 0)
            {
                calltoast.transform.Find("number").transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                calltoast.transform.Find("number").transform.localPosition = new Vector3(60, 0, 0);
            }
            
            StartCoroutine(ToastManager(1, true, 0f));
            StartCoroutine(ToastManager(1, false, 5.0f));
        }
    }

    public void phoneBatteryStateCallback(string state)
    {
        PLOG.I("PvrLog phoneBatteryState" + state);

        var Jdstate = LitJson.JsonMapper.ToObject(state);

        string level = "";
        if (state.Contains("phoneBatteryLevel"))
        {
            level = (string)Jdstate["phoneBatteryLevel"];
        }
        string health = "";
        if (state.Contains("phoneBatteryHealth"))
        {
            health = (string)Jdstate["phoneBatteryHealth"];
        }
        
        if (UseToast)
        {
            if (Convert.ToInt16(level) <= 5)
            {
                if (lowhmdBatterytoast.activeSelf == false)
                {
                    StartCoroutine(ToastManager(4, true, 0f));
                    StartCoroutine(ToastManager(4, false, 3.0f));
                }
                else
                {
                    StartCoroutine(ToastManager(4, true, 5.0f));
                    StartCoroutine(ToastManager(4, false, 8.0f));
                }
                
            }
            if (Convert.ToInt16(health) == 3)
            {
                StartCoroutine(ToastManager(5, true, 0f));
                StartCoroutine(ToastManager(5, false, 5.0f));
            }
        }
    }
    public void hmdLowBatteryCallback(string level)
    {
        PLOG.I("PvrLog hmdLowBatteryCallback" + level);

        if (UseToast)
        {
            if (lowphoneBatterytoast.activeSelf == false)
            {
                StartCoroutine(ToastManager(3, true, 0f));
                StartCoroutine(ToastManager(3, false, 3.0f));
            }
            else
            {
                StartCoroutine(ToastManager(3, true, 5.0f));
                StartCoroutine(ToastManager(3, false, 8.0f));
            }
            
        }
    }
    private string TransformNumber(string number)
    {
        if (number.Length == 11)
        {
            //0xy-1234-1234
            //x = 3,4,5,6
            //y = 1,2,3,4,5

            //01x-1234-1234
            //x=0,1,6,7,8...
            var part1 = number.Substring(0, 3);
            var part2 = number.Substring(3, 4);
            var part3 = number.Substring(7, 4);

            number = part1 + "-" + part2 + "-" + part3;
        }
        else if (number.Length == 10)
        {
            //01x-123-1234
            if (number.Substring(1, 1) == "1")
            {
                var part1 = number.Substring(0, 3);
                var part2 = number.Substring(3, 3);
                var part3 = number.Substring(6, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
            //02-1234-1234
            else
            {
                var part1 = number.Substring(0, 2);
                var part2 = number.Substring(2, 4);
                var part3 = number.Substring(6, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
        }
        //02-123-1234
        else if (number.Length == 9)
        {
            if (number.Substring(1, 1) == "2")
            {
                var part1 = number.Substring(0, 2);
                var part2 = number.Substring(2, 3);
                var part3 = number.Substring(5, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
            else
            {
                number = "+82" + number.Remove(0, 1);
            }
        }
        return number;
    }
    //Head reset is complete
    public void onHmdOrientationReseted()
    {

    }

    private IEnumerator ToastManager(int type,bool state,float time)
    {
        yield return new WaitForSeconds(time);

        switch (type)
        {
            //call toast
            case 1:
                {
                    calltoast.SetActive(state);
                    break;
                }
            //msg toast
            case 2:
                {
                    msgtoast.SetActive(state);
                    break;
                }
            //low hmd battery toast
            case 3:
                {
                    lowhmdBatterytoast.SetActive(state);
                    break;
                }
            //low phone battery toast
            case 4:
                {
                    lowphoneBatterytoast.SetActive(state);
                    break;
                }
            //low phone health toast
            case 5:
                {
                    LowPhoneHealthtoast.SetActive(state);
                    break;
                }
            //low controller battery toast
            case 6:
                {
                    LowcontrollerBatterytoast.SetActive(state);
                    break;
                }
        }

    }

    private void CheckControllerStateForG2(string state)
    {
        if (iPhoneHMDModeEnabled == 1)
        {
            if (Convert.ToBoolean(Convert.ToInt16(state)) && Controller.UPvr_GetControllerPower(0) == 0 && Pvr_ControllerManager.controllerlink.Controller0.Rotation.eulerAngles != Vector3.zero)
            {
                StartCoroutine(ToastManager(6, true, 0f));
                StartCoroutine(ToastManager(6, false, 3.0f));
            }
        }
    }

    //-1:unknown 0:sms 1:call 2:msg 3:lowbat 4:overheat 5:general
    public void notificationCallback(string data)
    {
        LitJson.JsonData jdata = LitJson.JsonMapper.ToObject(data);
        if (G3LiteTips == null)
        {
            G3LiteTips = Instantiate(Resources.Load("Prefabs/G3LiteTips") as GameObject, transform.Find("Head"), false);
        }
        string tmp =  jdata["str"].ToString();
        LitJson.JsonData callbackdata = LitJson.JsonMapper.ToObject(tmp);
        switch ((int)jdata["type"])
        {
            case -1:
                {
                    //unknown
                }
                break;
            case 0:
                {
                    //sms
                    SetProperty(0, callbackdata,"Sms");
                }
                break;
            case 1:
                {
                    //call
                    SetProperty(1, callbackdata, "Call");
                }
                break;
            case 2:
                {
                    //msg
                    SetProperty(2, callbackdata, "Warnning");
                }
                break;
            case 3:
                {
                    //lowbat
                    SetProperty(3, callbackdata, "Warnning");
                }
                break;
            case 4:
                {
                    //overheat
                    SetProperty(4, callbackdata, "Warnning");
                }
                break;
            case 5:
                {
                    //general
                    var image = G3LiteTips.transform.Find("Onlyimage");
                    SetBaseProperty(image, callbackdata["General"], "");
                    SetImageProperty(image, callbackdata["General"], "");
                    image.gameObject.SetActive(true);
                    StartCoroutine(G3TipsManager(image.gameObject, (int)callbackdata["General"]["time"]));
                }
                break;
        }
    }

    private Sprite LoadSprite(Vector2 size, string filepath)
    {
        int t_w = (int)size.x;
        int t_h = (int)size.y;
        var m_tex = new Texture2D(t_w, t_h);
        m_tex.LoadImage(ReadTex(filepath));
        Sprite sp = Sprite.Create(m_tex, new Rect(0, 0, m_tex.width, m_tex.height), new Vector2(0.5f, 0.5f));
        return sp;
    }

    private byte[] ReadTex(string path)
    {
        if (path == "")
        {
            return new byte[0];
        }
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] binary = new byte[fileStream.Length];
        fileStream.Read(binary, 0, (int)fileStream.Length);
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        return binary;
    }

    private void SetProperty(int type, LitJson.JsonData data, string value)
    {
        var trans = G3LiteTips.transform.Find(value);
        SetBaseProperty(trans, data, "");
        SetImageProperty(trans, data, "");
        trans.gameObject.SetActive(true);
        StartCoroutine(G3TipsManager(trans.gameObject, (int)data["time"]));

        var icon = trans.transform.Find("icon");
        SetBaseProperty(icon, data, "icon_");
        SetImageProperty(icon, data, "icon_");

        var title = trans.transform.Find("title");
        SetBaseProperty(title, data, "title_");
        SetTextProperty(title, data, "title_");

        if (type != 1)
        {
            var details = trans.transform.Find("details");
            SetBaseProperty(details, data, "details_");
            SetTextProperty(details, data, "details_");

            var image1 = trans.transform.Find("image1");
            SetBaseProperty(image1, data, "image1_");
            SetImageProperty(image1, data, "image1_");
        }
        if (type == 0 || type == 1)
        {
            var explain = trans.transform.Find("explain");
            SetBaseProperty(explain, data, "explain_");
            SetTextProperty(explain, data, "explain_");

            var source = trans.transform.Find("source");
            SetBaseProperty(source, data, "source_");
            SetTextProperty(source, data, "source_");
        }
        if (type == 0)
        {
            var time = trans.transform.Find("time");
            SetBaseProperty(time, data, "system_time_");
            SetTextProperty(time, data, "system_time_");
        }

        var btn = trans.transform.Find("Button");
        SetBaseProperty(btn, data, "button_");
        SetImageProperty(btn, data, "button_");
        btn.GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine(G3TipsManager(trans.gameObject, 0f)); });

        var btntext = btn.transform.Find("Text");
        SetBaseProperty(btntext, data, "button_text_");
        SetTextProperty(btntext, data, "button_text_");
    }

    private void SetBaseProperty(Transform trans, LitJson.JsonData data, string value)
    {
        string spos = value + "pos";
        string sangles = value + "angles";
        string ssize = value + "size";
        string sscale = value + "scale";
        trans.GetComponent<RectTransform>().anchoredPosition3D =
            new Vector3(JsonToFloat(data[spos][0]), JsonToFloat(data[spos][1]), JsonToFloat(data[spos][2]));
        trans.GetComponent<RectTransform>().eulerAngles =
            new Vector3(JsonToFloat(data[sangles][0]), JsonToFloat(data[sangles][1]), JsonToFloat(data[sangles][2]));
        trans.GetComponent<RectTransform>().sizeDelta =
            new Vector2(JsonToFloat(data[ssize][0]), JsonToFloat(data[ssize][1]));
        trans.GetComponent<RectTransform>().localScale =
            new Vector3(JsonToFloat(data[sscale][0]), JsonToFloat(data[sscale][1]), JsonToFloat(data[sscale][2]));
    }

    private void SetImageProperty(Transform image, LitJson.JsonData data, string value)
    {
        string spath = value + "sprite";
        string scolor = value + "color";
        string ssize = value + "size";
        image.GetComponent<Image>().sprite =
            LoadSprite(new Vector2(JsonToFloat(data[ssize][0]), JsonToFloat(data[ssize][1])), (string)data[spath]);
        image.GetComponent<Image>().color =
            new Color(JsonToFloat(data[scolor][0]), JsonToFloat(data[scolor][1]), JsonToFloat(data[scolor][2]), JsonToFloat(data[scolor][3]));
    }

    private void SetTextProperty(Transform text, LitJson.JsonData data, string value)
    {
        string scolor = value + "color";
        string ssize = value + "font_size";
        string sstyle = value + "font_style";
        string stext = value + "text";
        text.GetComponent<Text>().text = (string)data[stext];
        text.GetComponent<Text>().color =
            new Color(JsonToFloat(data[scolor][0]), JsonToFloat(data[scolor][1]), JsonToFloat(data[scolor][2]), JsonToFloat(data[scolor][3])); ;
        text.GetComponent<Text>().fontSize = (int)data[ssize];
        text.GetComponent<Text>().fontStyle = (FontStyle)(int)data[sstyle];
    }

    private IEnumerator G3TipsManager(GameObject tip, float time)
    {
        yield return new WaitForSeconds(time);
        tip.SetActive(false);
    }

    private float JsonToFloat(LitJson.JsonData data)
    {
        return Convert.ToSingle((string)data);
    }

    private bool SDKManagerInitFPS()
    {
        Transform[] father;
        father = GetComponentsInChildren<Transform>(true);
        GameObject FPS = null;
        foreach (Transform child in father)
        {
            if (child.gameObject.name == "FPS")
            {
                FPS = child.gameObject;
            }
        }
        if (FPS != null)
        {
            if (systemFPS)
            {
                FPS.SetActive(true);
                return true;
            }
            int fps = 0;
#if !UNITY_EDITOR
            int rate = (int)GlobalIntConfigs.iShowFPS;
            Render.UPvr_GetIntConfig(rate, ref fps);
#endif
            if (Convert.ToBoolean(fps))
            {
                FPS.SetActive(true);
                return true;
            }
            if (ShowFPS)
            {
                FPS.SetActive(true);
                return true;
            }
            return false;
        }
        return false;
    }

    private bool SDKManagerInitConfigProfile()
    {
        pvr_UnitySDKConfig = Pvr_UnitySDKConfigProfile.Default;
        return true;
    }

    private bool SDKManagerInitEditor()
    {
        if (pvr_UnitySDKEditor == null)
        {
            HeadPose = new Pvr_UnitySDKPose(Vector3.zero, Quaternion.identity);
            pvr_UnitySDKEditor = this.gameObject.AddComponent<Pvr_UnitySDKEditor>();
        }
        return true;
    }

    private bool SDKManagerInitPara()
    {
        return true;
    }

    public void SDKManagerLongHomeKey()
    {
        //closepanel
        if (resetPanel.activeSelf)
        {
            resetPanel.SetActive(false);
            resetPanel.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = 10001;
        }
        if (pvr_UnitySDKSensor != null)
        {
            if (isHasController)
            {
                if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
                    Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
                {
                    pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(0, 1);
                }
                else
                {
                    pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                }
            }
            else
            {
                pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
            }

        }
    }

    private void setLongHomeKey()
    {
        if (sdk.HmdOnlyrot)
        {
            if (pvr_UnitySDKSensor != null)
            {
                PLOG.I(pvr_UnitySDKSensor.ResetUnitySDKSensor()
                    ? "Long Home Key to Reset Sensor Success!"
                    : "Long Home Key to Reset Sensor Failed!");
            }
        }
        else
        {
            if (trackingmode == 4)
            {
                pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);

            }
            else
            {
                if (safeToast != null)
                {
                    if (safeToast.activeSelf)
                    {
                        if (isHasController && (Controller.UPvr_GetControllerState(0) == ControllerState.Connected || Controller.UPvr_GetControllerState(1) == ControllerState.Connected))
                        {
                            pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(0, 1);
                        }
                        else
                        {
                            pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                        }
                    }
                    else
                    {
                        resetPanel.SetActive(true);
                    }
                }
                else
                {
                    if (trackingmode == 0 || trackingmode == 1)
                    {
                        pvr_UnitySDKSensor.ResetUnitySDKSensor();
                    }
                }
            }
            
        }
    }

    public void verifyAPPCallback(string code)
    {
        Debug.Log("PvrLog verifyAPPCallback" + code);
        //code:0 valid
        //code:other invalid
    }
    /*************************************  Unity API ****************************************/
    #region Unity API

    void Awake()
    {
#if ANDROID_DEVICE
        Debug.Log("DISFT Unity Version:" + Application.unityVersion);
        Debug.Log("DISFT Customize NeckOffset:" + neckOffset);
        Debug.Log("DISFT MSAA :" + Pvr_UnitySDKProjectSetting.GetProjectConfig().rtAntiAlising.ToString());
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            Debug.Log("DISFT LWRP = Enable");
        }
        Debug.Log("DISFT Content Proctect :" + Pvr_UnitySDKProjectSetting.GetProjectConfig().usecontentprotect.ToString());
        int isrot = 0;
        int rot = (int)GlobalIntConfigs.Enable_Activity_Rotation;
        Render.UPvr_GetIntConfig(rot, ref isrot);
        if (isrot == 1)
        {
            Debug.Log("DISFT ScreenOrientation.Portrait = Enable");
            Screen.orientation = ScreenOrientation.Portrait;
        }
        bool supportSinglePass = true;
#if UNITY_2018_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            supportSinglePass = false;
            if(!Pvr_UnitySDKAPI.BoundarySystem.UPvr_EnableLWRP(true))
            {
                Debug.Log("UPvr_EnableLWRP return false");
            }
            Vector2 resolution = Pvr_UnitySDKRender.GetEyeBufferResolution();
            if (!Pvr_UnitySDKAPI.BoundarySystem.UPvr_SetViewportSize((int)resolution.x, (int)resolution.y))
            {
                Debug.Log("UPvr_SetViewportSize return false");
            }            
        }
            
#endif
        if(Pvr_UnitySDKProjectSetting.GetProjectConfig().usesinglepass)
        {
            bool result = false;
            if (supportSinglePass)
            {
                result = Pvr_UnitySDKAPI.System.UPvr_EnableSinglePass(true);
            }
            if (result)
            {
                StereoRendering = new Pvr_UnitySDKSinglePass();
                stereoRenderPath = StereoRenderingPathPico.SinglePass;
                eyeTextureCount = 3;
            }
            Debug.Log("EnableSinglePass supportSinglePass " + supportSinglePass.ToString() + " result " + result);
        }
#endif

#if ANDROID_DEVICE
        var javaVrActivityClass = new AndroidJavaClass("com.psmart.vrlib.VrActivity");
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif
        var controllermanager = FindObjectOfType<Pvr_ControllerManager>();
        isHasController = controllermanager != null;
        PLOG.getConfigTraceLevel();

        int enumindex = (int)GlobalIntConfigs.TRACKING_MODE;
        Render.UPvr_GetIntConfig(enumindex, ref trackingmode);
        //setting of fps
        Application.targetFrameRate = 61;
#if ANDROID_DEVICE
        int ability6dof = 0;
        enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF;
        Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref ability6dof);
        if (ability6dof == 0)
        {
            SDK.HmdOnlyrot = true;
        }
        int fps = -1;
        int rate = (int) GlobalIntConfigs.TARGET_FRAME_RATE;
        Render.UPvr_GetIntConfig(rate, ref fps);
        float ffps = 0.0f;
        int frame = (int) GlobalFloatConfigs.DISPLAY_REFRESH_RATE;
        Render.UPvr_GetFloatConfig(frame, ref ffps);
        Application.targetFrameRate = fps > 0 ? fps : (int)ffps;

        if (!Pvr_UnitySDKProjectSetting.GetProjectConfig().usedefaultfps)
        {
            if (Pvr_UnitySDKProjectSetting.GetProjectConfig().customfps <= ffps)
            {
                Application.targetFrameRate = Pvr_UnitySDKProjectSetting.GetProjectConfig().customfps;
            }
            else
            {
                Application.targetFrameRate = (int)ffps;
            }
        }
        Debug.Log("DISFT Customize FPS :" + Application.targetFrameRate);

#endif

        //setting of neck model 
#if ANDROID_DEVICE
        if (!UseCustomNeckPara)
        {
            float neckx = 0.0f;
            float necky = 0.0f;
            float neckz = 0.0f;
            int modelx = (int) GlobalFloatConfigs.NECK_MODEL_X;
            int modely = (int) GlobalFloatConfigs.NECK_MODEL_Y;
            int modelz = (int) GlobalFloatConfigs.NECK_MODEL_Z;
            Render.UPvr_GetFloatConfig(modelx, ref neckx);
            Render.UPvr_GetFloatConfig(modely, ref necky);
            Render.UPvr_GetFloatConfig(modelz, ref neckz);
            if (neckx != 0.0f || necky != 0.0f || neckz != 0.0f)
            {
                neckOffset = new Vector3(neckx, necky, neckz);
            }
        }
#endif
        Render.UPvr_GetIntConfig((int)GlobalIntConfigs.iPhoneHMDModeEnabled, ref iPhoneHMDModeEnabled);
        if (sdk == null)
        {
            sdk = this;
        }
        if (sdk != this)
        {
            PLOG.E("SDK object should be a singleton.");
            enabled = false;
            return;
        }
        if (SDKManagerInit())
        {
            PLOG.I("SDK Init success.");
        }
        else
        {
            PLOG.E("SDK Init Failed.");
            Application.Quit();
        }

        SDKManagerInitFPS();
        Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForG2;
#if ANDROID_DEVICE
        InitUI();
        RefreshTextByLanguage();
#endif
        if (!SDK.HmdOnlyrot)
        {
            if (Sensor.Pvr_IsHead6dofReset() && ShowSafePanel)
            {
                safePanel.SetActive(true);
            }
        }
    }


    //wait for unity to start rendering
    IEnumerator Start()
    {
#if UNITY_EDITOR
        yield break;
#else
        yield return StartCoroutine(InitRenderThreadRoutine());
#endif
    }

    IEnumerator InitRenderThreadRoutine()
    {
        PLOG.I("InitRenderThreadRoutine begin");
        for (int i = 0; i < 2; ++i)
        {
            yield return null;
        }
        Debug.Log("InitRenderThreadRoutine after a wait");


        if (pvr_UnitySDKRender != null)
        {
            pvr_UnitySDKRender.IssueRenderThread();
        }
        else
        {
            Debug.Log("InitRenderThreadRoutine pvr_UnitySDKRender == null");
        }

        Debug.Log("InitRenderThreadRoutine end");
        yield break;
    }


    void Update()
    {
        if (isHasController && iPhoneHMDModeEnabled == 1)
        {
            if (Controller.UPvr_GetControllerPower(0) == 0 && Pvr_ControllerManager.controllerlink.controller0Connected && Pvr_ControllerManager.controllerlink.Controller0.Rotation.eulerAngles != Vector3.zero)
            {
                if (!lowControllerpowerstate)
                {
                    StartCoroutine(ToastManager(6, true, 0f));
                    StartCoroutine(ToastManager(6, false, 3.0f));
                    lowControllerpowerstate = true;
                }

                controllerpowershowtime += Time.deltaTime;
                if (controllerpowershowtime >= 3600f)
                {
                    lowControllerpowerstate = false;
                    controllerpowershowtime = 0f;
                }
            }
        }
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                newPicovrTriggered = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            newPicovrTriggered = true;
        }

        if (pvr_UnitySDKSensor != null)
        {
            pvr_UnitySDKSensor.SensorUpdate();
        }

        if (trackingmode == 2 || trackingmode == 3) 
        {
#if ANDROID_DEVICE
            if (isHasController && (Controller.UPvr_GetControllerState(0) == ControllerState.Connected || Controller.UPvr_GetControllerState(1) == ControllerState.Connected))
            {
                safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                    Pvr_UnitySDKAPI.System.UPvr_GetLangString("safeToast0") + CustomRange + Pvr_UnitySDKAPI.System.UPvr_GetLangString("safeToast1");

                if (Input.GetKeyDown(KeyCode.JoystickButton0) || Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD))
                {
                    if (safePanel.activeSelf)
                        safePanel.SetActive(false);
                    if (resetPanel.activeSelf)
                    {
                        resetPanel.SetActive(false);
                        pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(0, 1);
                    }
                }
            }
            else
            {
                safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                    Pvr_UnitySDKAPI.System.UPvr_GetLangString("safeToast2") + CustomRange + Pvr_UnitySDKAPI.System.UPvr_GetLangString("safeToast3");
                if (Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    if (safePanel.activeSelf)
                    {
                        safePanel.SetActive(false);
                    }
                    if (resetPanel.activeSelf)
                    {
                        resetPanel.SetActive(false);
                        pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                    }
                }
            }

            if (safeToast.activeSelf)
            {
                safeToast.transform.localPosition = SDK.HeadPose.Position;
                safeToast.transform.localRotation = Quaternion.Euler(0, SDK.HeadPose.Orientation.eulerAngles.y, 0);
            }
            if (resetPanel.activeSelf)
            {
                resetPanel.transform.localPosition = SDK.HeadPose.Position;
                resetPanel.transform.localRotation = Quaternion.Euler(0, SDK.HeadPose.Orientation.eulerAngles.y, 0);
            }
            if (safePanel.activeSelf)
            {
                safePanel.transform.localPosition = SDK.HeadPose.Position;
                safePanel.transform.localRotation = Quaternion.Euler(0, SDK.HeadPose.Orientation.eulerAngles.y, 0);
            }

            if (!SDK.HmdOnlyrot)
            {
                //default 0.8m
                if (DefaultRange)
                {
                    if (isHasController)
                    {
                        if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > 0.56f || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(0).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(0).z, 2.0f)) > 0.8f || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(1).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(1).z, 2.0f)) > 0.8f)
                        {
                            safeArea.transform.localScale.Set(1.6f, 1.6f, 1.6f);
                            safeArea.SetActive(true);
                        }
                        else
                        {
                            safeArea.SetActive(false);
                        }
                    }
                    else
                    {
                        if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > 0.56f)
                        {
                            safeArea.transform.localScale.Set(1.6f, 1.6f, 1.6f);
                            safeArea.SetActive(true);
                        }
                        else
                        {
                            safeArea.SetActive(false);
                        }
                    }

                    if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > 0.8f)
                    {
                        if (!safeToast.activeSelf)
                        {
                            safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = resetPanel.transform.Find("Panel").GetComponent<Canvas>().sortingOrder + 1;
                            safeToast.SetActive(true);
                        }
                    }
                    else
                    {
                        if (safeToast.activeSelf)
                        {
                            safeToast.SetActive(false);
                            safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = 10001;
                        }
                    }
                }
                else
                {
                    if (isHasController)
                    {
                        if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > (0.7f * CustomRange) || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(0).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(0).z, 2.0f)) > CustomRange || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(1).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(1).z, 2.0f)) > CustomRange)
                        {
                            safeArea.transform.localScale.Set(CustomRange / 0.5f, CustomRange / 0.5f, CustomRange / 0.5f);
                            safeArea.SetActive(true);
                        }
                        else
                        {
                            safeArea.SetActive(false);
                        }
                    }
                    else
                    {
                        if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > (0.7f * CustomRange))
                        {
                            safeArea.transform.localScale.Set(CustomRange / 0.5f, CustomRange / 0.5f, CustomRange / 0.5f);
                            safeArea.SetActive(true);
                        }
                        else
                        {
                            safeArea.SetActive(false);
                        }
                    }
                    if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > CustomRange)
                    {
                        if (!safeToast.activeSelf)
                        {
                            safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = resetPanel.transform.Find("Panel").GetComponent<Canvas>().sortingOrder + 1;
                            safeToast.SetActive(true);
                        }
                    }
                    else
                    {
                        if (safeToast.activeSelf)
                        {
                            safeToast.SetActive(false);
                            safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = 10001;
                        }
                    }
                }
            }
#endif
        }

        picovrTriggered = newPicovrTriggered;
        newPicovrTriggered = false;

    }
    void OnDestroy()
    {

        if (sdk == this)
        {
            sdk = null;
        }
        RenderTexture.active = null;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForG2;
    }

    public void OnApplicationQuit()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        /*
               if (pvr_UnitySDKSensor != null)
                 {
                pvr_UnitySDKSensor.StopUnitySDKSensor();
                  }
                try{
                    PLOG.I("OnApplicationQuit  1  -------------------------");
                    Pvr_UnitySDKPluginEvent.Issue( RenderEventType.ShutdownRenderThread );
                }
                catch (Exception e)
                {
                    PLOG.I("ShutdownRenderThread Error" + e.Message);
                }
        */
#endif
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPause()
    {
        Pvr_UnitySDKAPI.System.UPvr_StopHomeKeyReceiver();
        this.LeaveVRMode();
        if (pvr_UnitySDKSensor != null)
        {
            pvr_UnitySDKSensor.StopUnitySDKSensor();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("OnApplicationPause-------------------------" + (pause ? "true" : "false"));
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
        {
            bool state = Pvr_UnitySDKAPI.System.UPvr_GetMainActivityPauseStatus();
            PLOG.I("Current Activity Pause State:" + state);
            pause = state;
        }

        if (pause)
        { 
            onResume = false;
            OnPause();
        }
        else
        {             
            onResume = true;
            GL.InvalidateState();
            StartCoroutine(OnResume());
        }
#endif
    }

    public void EnterVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Resume);
        this.isEnterVRMode = true;
        if (eventEnterVRMode != null)
        {
            eventEnterVRMode();
        }
    }

    public void LeaveVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Pause);
        this.isEnterVRMode = false;
    }

    public void SixDofForceQuit()
    {
        Application.Quit();
    }

    private void InitUI()
    {
        if (trackingmode > 1)
        {
            safeArea = Instantiate(Resources.Load("Prefabs/SafeArea") as GameObject, transform, false);
            safeToast = Instantiate(Resources.Load("Prefabs/SafeToast") as GameObject, transform, false);
        }

        resetPanel = Instantiate(Resources.Load("Prefabs/ResetPanel") as GameObject, transform, false);
        safePanel = Instantiate(Resources.Load("Prefabs/SafePanel") as GameObject, transform, false);

        if (iPhoneHMDModeEnabled == 1)
        {
            var flamingo2Tips = Instantiate(Resources.Load("Prefabs/flamingo2Tips") as GameObject, transform.Find("Head"), false).transform;
            calltoast = flamingo2Tips.Find("Call").gameObject;
            msgtoast = flamingo2Tips.Find("Msg").gameObject;
            lowhmdBatterytoast = flamingo2Tips.Find("LowHmdBattery").gameObject;
            lowphoneBatterytoast = flamingo2Tips.Find("LowPhoneBattery").gameObject;
            LowPhoneHealthtoast = flamingo2Tips.Find("LowPhoneHealth").gameObject;
            LowcontrollerBatterytoast = flamingo2Tips.Find("LowControllerBattery").gameObject;
        }
    }

    private void RefreshTextByLanguage()
    {
        if (safeToast != null)
        {
            safeToast.transform.Find("Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(470, 470);
            safeToast.transform.Find("Panel/title").localPosition = new Vector3(0, 173, 0);
            safeToast.transform.Find("Panel/Image").localPosition = new Vector3(0, -108, 0);
            safeToast.transform.Find("Panel/Text").GetComponent<RectTransform>().sizeDelta = new Vector2(440, 180);
            safeToast.transform.Find("Panel/Text").localPosition = new Vector3(10, 55, 0);
        }

        if (safePanel != null && resetPanel != null)
        {
            safePanel.transform.Find("Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(470, 470);
            safePanel.transform.Find("Panel/toast1").GetComponent<RectTransform>().sizeDelta = new Vector2(425, 200);
            resetPanel.transform.Find("Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(470, 470);
            resetPanel.transform.Find("Panel/toast").GetComponent<RectTransform>().sizeDelta = new Vector2(440, 180);

            resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                Pvr_UnitySDKAPI.System.UPvr_GetLangString("resetPanel0") + CustomRange + Pvr_UnitySDKAPI.System.UPvr_GetLangString("resetPanel1");
            safePanel.transform.Find("Panel/toast1").GetComponent<Text>().text =
                Pvr_UnitySDKAPI.System.UPvr_GetLangString("safePanel0") + CustomRange + Pvr_UnitySDKAPI.System.UPvr_GetLangString("safePanel1");
        }

        if (msgtoast != null)
        {
            msgtoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("msgtoast0");
            msgtoast.transform.Find("string").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("msgtoast1");
            calltoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("calltoast0");
            calltoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("calltoast1");
            lowhmdBatterytoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("lowhmdBatterytoast");
            lowphoneBatterytoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("lowphoneBatterytoast");
            LowPhoneHealthtoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("LowPhoneHealthtoast");
            LowcontrollerBatterytoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("LowcontrollerBatterytoast");
        }
    }
#endregion

    /************************************    IEnumerator  *************************************/
    private IEnumerator OnResume()
    {
        int ability6dof = 0;
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF;
        Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref ability6dof);
        if (ResetTrackerOnLoad && ability6dof == 1)
        {
            Debug.Log("Reset Tracker OnLoad");
            pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
        }

        RefreshTextByLanguage();
        if (pvr_UnitySDKSensor != null)
        {
            pvr_UnitySDKSensor.StartUnitySDKSensor();

            int iEnable6Dof = -1;
#if !UNITY_EDITOR && UNITY_ANDROID
            int iEnable6DofGlobalTracking = (int) GlobalIntConfigs.ENBLE_6DOF_GLOBAL_TRACKING;
            Render.UPvr_GetIntConfig(iEnable6DofGlobalTracking, ref iEnable6Dof);
#endif
            if (iEnable6Dof != 1)
            {
                int sensormode = -1;
#if !UNITY_EDITOR && UNITY_ANDROID
                int isensormode = (int) GlobalIntConfigs.SensorMode;
                Render.UPvr_GetIntConfig(isensormode, ref sensormode);
#endif
                if (sensormode != 8)
                {
                    pvr_UnitySDKSensor.ResetUnitySDKSensor();
                }
            }

            if (!SDK.HmdOnlyrot)
            {
                if (Sensor.Pvr_IsHead6dofReset() && ShowSafePanel)
                {
                    if (trackingmode == 2 || trackingmode == 3)
                    {
                        safePanel.SetActive(true);
                    }
                    
                }
            }

        }

        if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
        {
            bool setMonoPresentation = Pvr_UnitySDKAPI.System.UPvr_SetMonoPresentation();
            PLOG.I("onresume set monoPresentation success ?-------------" + setMonoPresentation.ToString());

            bool isPresentationExisted = Pvr_UnitySDKAPI.System.UPvr_IsPresentationExisted();
            PLOG.I("onresume presentation existed ?-------------" + isPresentationExisted.ToString());
        }

        for (int i = 0; i < Pvr_UnitySDKEyeManager.Instance.Eyes.Length; i++)
        {
            Pvr_UnitySDKEyeManager.Instance.Eyes[i].RefreshCameraPosition(Pvr_UnitySDKAPI.System.UPvr_GetIPD());
        }

        yield return new WaitForSeconds(1.0f);

        this.EnterVRMode();
        Pvr_UnitySDKAPI.System.UPvr_StartHomeKeyReceiver(this.gameObject.name);
        Pvr_UnitySDKEye.setLevel = false;
    }
}
