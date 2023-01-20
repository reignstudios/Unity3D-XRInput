// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using UnityEngine;
using UnityEngine.UI;
using Pvr_UnitySDKAPI;
using System;

public class Pvr_Controller : MonoBehaviour
{

    /************************************    Properties  *************************************/
    #region Properties

    public GameObject controller0;
    public GameObject controller1;
    private static UserHandNess handness;
    private bool controller0is3dof = false;
    private bool controller1is3dof = false;

    public enum UserHandNess
    {
        Right,
        Left
    }

    public enum GazeType
    {
        Never,
        DuringMotion,
        Always
    }

    public enum ControllerAxis
    {
        Controller,
        Wrist,
        Elbow,
        Shoulder
    }

    public ControllerAxis Axis;
    public GazeType Gazetype;

    [Range(0.0f, 0.2f)]
    public float ElbowHeight = 0.0f;
    [Range(0.0f, 0.2f)]
    public float ElbowDepth = 0.0f;
    [Range(0.0f, 30.0f)]
    public float PointerTiltAngle = 15.0f;

    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
        Pvr_ControllerManager.SetControllerAbilityEvent += CheckControllerState;
        Pvr_ControllerManager.ChangeMainControllerCallBackEvent += MainControllerChanged;
        Pvr_ControllerManager.ChangeHandNessCallBackEvent += HandnessChanged;

    }
    void Start()
    {
        handness = (UserHandNess)Pvr_ControllerManager.controllerlink.getHandness();

        if ((int)handness == -1)
        {
            handness = UserHandNess.Right;
        }
    }

    void OnDestroy()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
        Pvr_ControllerManager.SetControllerAbilityEvent -= CheckControllerState;
        Pvr_ControllerManager.ChangeMainControllerCallBackEvent -= MainControllerChanged;
        Pvr_ControllerManager.ChangeHandNessCallBackEvent -= HandnessChanged;
    }

    private void MainControllerChanged(string index)
    {
        RefreshHandness();
    }

    private void HandnessChanged(string index)
    {
        RefreshHandness();
    }

    private void ServiceStartSuccess()
    {
        RefreshHandness();
        if (Pvr_ControllerManager.controllerlink.neoserviceStarted)
        {
            if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected)
            {
                controller0is3dof = Controller.UPvr_GetControllerAbility(0) == 1;
            }
            if (Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
            {
                controller1is3dof = Controller.UPvr_GetControllerAbility(1) == 1;
            }
        }
    }

    private void RefreshHandness()
    {
        handness = (UserHandNess)Pvr_ControllerManager.controllerlink.getHandness();
        if (Controller.UPvr_GetMainHandNess() == 1)
        {
            ChangeHandNess();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            if (controller0 != null)
                controller0.transform.localScale = Vector3.zero;
            if (controller1 != null)
                controller1.transform.localScale = Vector3.zero;
        }
        Invoke("ShowController", 0.1f);
    }

    private void ShowController()
    {
        if (controller0 != null)
            controller0.transform.localScale = Vector3.one;
        if (controller1 != null)
            controller1.transform.localScale = Vector3.one;
    }

    private void CheckControllerState(string data)
    {
        var state = Convert.ToBoolean(Convert.ToInt16(data.Substring(4, 1)));
        var id = Convert.ToInt16(data.Substring(0, 1));
        var ability = Convert.ToInt16(data.Substring(2, 1));
        if (state)
        {
            if (id == 0)
            {
                controller0is3dof = ability == 1;
            }
            if (id == 1)
            {
                controller1is3dof = ability == 1;
                RefreshHandness();
            }
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        Quaternion controllerData = new Quaternion();
        controllerData = UpdateSimulatedFrameParams();
        if (controller1 != null)
            controller1.transform.localRotation = controllerData;
#else
        if (Pvr_UnitySDKManager.SDK.ControllerOnlyrot)
        {
            DoUpdateControler0();
            DoUpdateControler1();
            UpdateControlloerRay();
        }
        else
        {
            if (Controller.UPvr_GetPreferenceDevice() == 1)
            {
                DoUpdateControler0();
                DoUpdateControler1();
                UpdateControlloerRay();
            }
            else
            {
                if (controller0 != null)
                {                    
                    if (controller0is3dof)
                    {
                        DoUpdateControler0();
                    }
                    else
                    {
                        controller0.transform.localRotation = Controller.UPvr_GetControllerQUA(0);
                        controller0.transform.localPosition = Controller.UPvr_GetControllerPOS(0);
                    }
                    UpdateControlloerRay(true, false);
                }
                if (controller1 != null)
                {
                    if (controller1is3dof)
                    {
                        DoUpdateControler1();
                    }
                    else
                    {
                        controller1.transform.localRotation = Controller.UPvr_GetControllerQUA(1);
                        controller1.transform.localPosition = Controller.UPvr_GetControllerPOS(1);
                    }
                    UpdateControlloerRay(false, true);
                }
            }
        }
#endif 
    }
    public static void ChangeHandNess()
    {
        handness = handness == UserHandNess.Right ? UserHandNess.Left : UserHandNess.Right;
    }
    #endregion

    private void DoUpdateControler0()
    {
        //controller0
        SetArmParaToSo((int)handness, (int)Gazetype, ElbowHeight, ElbowDepth, PointerTiltAngle);
        CalcArmModelfromSo(0);
        UpdateControllerDataSO(0);
    }

    private void DoUpdateControler1()
    {
        //controller1
        var offhand = handness == UserHandNess.Left ? (int)UserHandNess.Right : (int)UserHandNess.Left;
        SetArmParaToSo(offhand, (int)Gazetype, ElbowHeight, ElbowDepth, PointerTiltAngle);
        CalcArmModelfromSo(1);
        UpdateControllerDataSO(1);
    }

    private void UpdateControlloerRay(bool update0 = true, bool update1 = true)
    {
        if (!Pvr_ControllerManager.Instance.LengthAdaptiveRay)
            return;

        if (update0 && controller0 != null)
        {
            controller0.GetComponent<Pvr_ControllerModuleInit>().UpdateRay();
        }
        if (update1 && controller1 != null)
        {
            controller1.GetComponent<Pvr_ControllerModuleInit>().UpdateRay();
        }
    }

    private float mouseX = 0;
    private float mouseY = 0;
    private float mouseZ = 0;
    private Quaternion UpdateSimulatedFrameParams()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            mouseX += Input.GetAxis("Mouse X") * 5;
            if (mouseX <= -180)
            {
                mouseX += 360;
            }
            else if (mouseX > 180)
            {
                mouseX -= 360;
            }
            mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
            mouseY = Mathf.Clamp(mouseY, -80, 80);
        }
        else if (Input.GetKey(KeyCode.RightShift))
        {
            mouseZ += Input.GetAxis("Mouse X") * 5;
            mouseZ = Mathf.Clamp(mouseZ, -80, 80);
        }

        return Quaternion.Euler(mouseY, mouseX, mouseZ);
    }
    private Vector3[] inputDirection = new Vector3[2];
    private void SetArmParaToSo(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle)
    {
        Pvr_UnitySDKAPI.Controller.UPvr_SetArmModelParameters(hand, gazeType, elbowHeight, elbowDepth, pointerTiltAngle);
    }

    private void CalcArmModelfromSo(int hand)
    {
        float[] Headrot = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
        float[] Handrot = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
        float[] AgeeAngularVelocity = new float[3] { 0.0f, 0.0f, 0.0f };
        Quaternion controllerData = new Quaternion();
        controllerData = Controller.UPvr_GetControllerQUA(hand);
        Vector3 angVelocity = Controller.UPvr_GetAngularVelocity(hand);

        Handrot[0] = controllerData.x;
        Handrot[1] = controllerData.y;
        Handrot[2] = controllerData.z;
        Handrot[3] = controllerData.w;

        AgeeAngularVelocity[0] = angVelocity.x;
        AgeeAngularVelocity[1] = angVelocity.y;
        AgeeAngularVelocity[2] = angVelocity.z;
        if (Gazetype == GazeType.DuringMotion)
        {
            Vector3 gazeDirection = Pvr_UnitySDKManager.SDK.HeadPose.Orientation * Vector3.forward;
            gazeDirection.y = 0.0f;
            gazeDirection.Normalize();
            float angular = angVelocity.magnitude;
            float gazeFilter = Mathf.Clamp((angular - 0.2f) / 45.0f, 0.0f, 0.1f);
            inputDirection[hand] = Vector3.Slerp(inputDirection[hand], gazeDirection, gazeFilter);
            if (Controller.UPvr_GetKeyLongPressed(hand, Pvr_KeyCode.HOME))
            {
                inputDirection[hand] = new Vector3();
            }
            Quaternion gazeRotation = Quaternion.FromToRotation(Vector3.forward, inputDirection[hand]);

            Headrot[0] = gazeRotation.x;
            Headrot[1] = gazeRotation.y;
            Headrot[2] = gazeRotation.z;
            Headrot[3] = gazeRotation.w;

        }
        else
        {
            Headrot[0] = Pvr_UnitySDKManager.SDK.HeadPose.Orientation.x;
            Headrot[1] = Pvr_UnitySDKManager.SDK.HeadPose.Orientation.y;
            Headrot[2] = Pvr_UnitySDKManager.SDK.HeadPose.Orientation.z;
            Headrot[3] = Pvr_UnitySDKManager.SDK.HeadPose.Orientation.w;
        }
        Controller.UPvr_CalcArmModelParameters(Headrot, Handrot, AgeeAngularVelocity);
    }

    public void UpdateControllerDataSO(int hand)
    {
#if ANDROID_DEVICE
        float[] rot = new float[4] { 0, 0, 0, 0 };
        float[] pos = new float[3] { 0, 0, 0 };
        Vector3 finalyPosition;
        Quaternion finalyRotation;
        switch (Axis)
        {
            case ControllerAxis.Controller:
                Controller.UPvr_GetPointerPose(rot, pos);
                pointerPosition = new Vector3(pos[0], pos[1], pos[2]);
                pointerRotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
                finalyPosition = pointerPosition;
                finalyRotation = pointerRotation;
                break;
            case ControllerAxis.Wrist:
                Controller.UPvr_GetWristPose(rot, pos);
                wristPosition = new Vector3(pos[0], pos[1], pos[2]);
                wristRotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
                finalyPosition = wristPosition;
                finalyRotation = wristRotation;
                break;
            case ControllerAxis.Elbow:
                Controller.UPvr_GetElbowPose(rot, pos);
                elbowPosition = new Vector3(pos[0], pos[1], pos[2]);
                elbowRotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);   
                finalyPosition = elbowPosition;
                finalyRotation = elbowRotation;
                break;
            case ControllerAxis.Shoulder:
                Controller.UPvr_GetShoulderPose(rot, pos);
                shoulderPosition = new Vector3(pos[0], pos[1], pos[2]);
                shoulderRotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);  
                finalyPosition = shoulderPosition;
                finalyRotation = shoulderRotation;
                break;
            default:
                throw new System.Exception("Invalid FromJoint.");
        }
        
        if (hand == 0)
        {
            if (controller0 != null)
            {
                if (!Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                {
                    controller0.transform.localPosition = finalyPosition + Pvr_UnitySDKManager.SDK.HeadPose.Position;
                }
                else
                {
                    if (Pvr_UnitySDKManager.SDK.TrackingOrigin == TrackingOrigin.FloorLevel)
                    {
                        controller0.transform.localPosition = finalyPosition + Pvr_UnitySDKManager.SDK.HeadPose.Position;
                    }
                    else
                    {
                        controller0.transform.localPosition = finalyPosition;
                    }
                    
                }
                controller0.transform.localRotation = finalyRotation;
            }
        }
        else
        {
            if (controller1 != null)
            {
                if (!Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                {
                    controller1.transform.localPosition = finalyPosition + Pvr_UnitySDKManager.SDK.HeadPose.Position;
                }
                else
                {
                    if (Pvr_UnitySDKManager.SDK.TrackingOrigin == TrackingOrigin.FloorLevel)
                    {
                        controller1.transform.localPosition = finalyPosition + Pvr_UnitySDKManager.SDK.HeadPose.Position;
                    }
                    else
                    {
                        controller1.transform.localPosition = finalyPosition;
                    }
                }
                controller1.transform.localRotation = finalyRotation;
            }
        }
#endif
    }

    public static Vector3 pointerPosition { get; set; }
    public static Quaternion pointerRotation { get; set; }
    public static Vector3 elbowPosition { get; set; }
    public static Quaternion elbowRotation { get; set; }
    public static Vector3 wristPosition { get; set; }
    public static Quaternion wristRotation { get; set; }
    public static Vector3 shoulderPosition { get; set; }
    public static Quaternion shoulderRotation { get; set; }

}
