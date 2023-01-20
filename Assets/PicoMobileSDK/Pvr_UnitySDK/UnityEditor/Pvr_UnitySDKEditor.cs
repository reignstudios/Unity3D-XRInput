// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

[ExecuteInEditMode]
public class Pvr_UnitySDKEditor : MonoBehaviour
{
    /************************************    Properties  *************************************/
    #region Properties

    private bool vrModeEnabled = true;
    private float mouseX = 0;
    private float mouseY = 0;
    private float mouseZ = 0;
    private float neckModelScale = 0;
    private bool autoUntiltHead = false;
    private static readonly Vector3 neckOffset = new Vector3(0, 0.075f, 0.0805f);

    [HideInInspector]
    public Matrix4x4 headView;

    public Matrix4x4 UndistortedProjection(Pvr_UnitySDKAPI.Eye eye)
    {
        return eye == Pvr_UnitySDKAPI.Eye.LeftEye ? leftEyeUndistortedProj : rightEyeUndistortedProj;
    }
    [HideInInspector]
    public Matrix4x4 leftEyeUndistortedProj;
    [HideInInspector]
    public Matrix4x4 rightEyeUndistortedProj;

    public Matrix4x4 Projection(Pvr_UnitySDKAPI.Eye eye)
    {
        return eye == Pvr_UnitySDKAPI.Eye.LeftEye ? leftEyeProj : rightEyeProj;
    }
    [HideInInspector]
    public Matrix4x4 leftEyeProj;
    [HideInInspector]
    public Matrix4x4 rightEyeProj;

    private const float TOUCH_TIME_LIMIT = 0.2f;
    private float touchStartTime = 0;

    #endregion Properties

    /*************************************  Unity API ****************************************/
    #region Unity API

    void Awake()
    {
        InitEyePara();
        InitEditorSensorPara();
    }

    void Update()
    {
        SimulateInput();
        Pvr_UnitySDKManager.SDK.picovrTriggered = Pvr_UnitySDKManager.SDK.newPicovrTriggered;
        Pvr_UnitySDKManager.SDK.newPicovrTriggered = false;
    }
    #endregion

    /************************************ Public Interface  *********************************/
    #region Public Interface

    public void InitEyePara()
    {
        ComputeEyesFromProfile();
        InitForEye(ref Pvr_UnitySDKManager.SDK.Eyematerial, ref Pvr_UnitySDKManager.SDK.Middlematerial);
        FovAdjust();
    }

    private void InitEditorSensorPara()
    {
        Pvr_UnitySDKManager.SDK.picovrTriggered = Pvr_UnitySDKManager.SDK.newPicovrTriggered;
        Pvr_UnitySDKManager.SDK.newPicovrTriggered = false;
    }

    public static Matrix4x4 MakeProjection(float l, float t, float r, float b, float n, float f)
    {
        Matrix4x4 m = Matrix4x4.zero;
        m[0, 0] = 2 * n / (r - l);
        m[1, 1] = 2 * n / (t - b);
        m[0, 2] = (r + l) / (r - l);
        m[1, 2] = (t + b) / (t - b);
        m[2, 2] = (n + f) / (n - f);
        m[2, 3] = 2 * n * f / (n - f);
        m[3, 2] = -1;
        return m;
    }

    public bool UpdateStatesensor()
    {
        UpdateSimulatedSensor();
        return true;
    }

    public void ComputeEyesFromProfile()
    {
        Vector2 rtorScren = new Vector2(0.110f, 0.062f);
        Pvr_UnitySDKManager.SDK.leftEyeView = Matrix4x4.identity;
        Pvr_UnitySDKManager.SDK.leftEyeView[0, 3] = -Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devLenses.separation / 2;
        float[] rect = Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.GetLeftEyeVisibleTanAngles(rtorScren.x, rtorScren.y);
        leftEyeProj = MakeProjection(rect[0], rect[1], rect[2], rect[3], 1, 1000);
        rect = Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.GetLeftEyeNoLensTanAngles(rtorScren.x, rtorScren.y);
        leftEyeUndistortedProj = MakeProjection(rect[0], rect[1], rect[2], rect[3], 1, 1000);
        Pvr_UnitySDKManager.SDK.leftEyeRect = Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.GetLeftEyeVisibleScreenRect(rect, rtorScren.x, rtorScren.y);
        Pvr_UnitySDKManager.SDK.rightEyeView = Pvr_UnitySDKManager.SDK.leftEyeView;
        Pvr_UnitySDKManager.SDK.rightEyeView[0, 3] *= -1;
        rightEyeProj = leftEyeProj;
        rightEyeProj[0, 2] *= -1;
        rightEyeUndistortedProj = leftEyeUndistortedProj;
        rightEyeUndistortedProj[0, 2] *= -1;
        Rect left = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
        Rect right = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
        Pvr_UnitySDKManager.SDK.leftEyeRect = left;
        Pvr_UnitySDKManager.SDK.rightEyeRect = right;
        Pvr_UnitySDKManager.SDK.leftEyeOffset = new Vector3(-Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devLenses.separation / 2, 0, 0);
        Pvr_UnitySDKManager.SDK.rightEyeOffset = new Vector3(Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devLenses.separation / 2, 0, 0);

    }

    public bool ResetUnitySDKSensor()
    {
        mouseX = mouseY = mouseZ = 0;
        return true;
    }
    #endregion

    /************************************ Private Interfaces  *********************************/
    #region Private Interfaces

    private void SimulateInput()
    {

        if (Input.GetMouseButtonDown(0)
            && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
        {
            EnableVEmodel();
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStartTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - touchStartTime <= TOUCH_TIME_LIMIT)
            {
                Pvr_UnitySDKManager.SDK.newPicovrTriggered = true;
            }
            touchStartTime = 0;
        }
        UpdateSimulatedSensor();

    }

    private void FovAdjust()
    {
        Pvr_UnitySDKManager.SDK.EyeVFoV = 2 * Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devMaxFov.upper;
    }

    private Rect RectAdjust(Rect eyeRect)
    {
        Rect rect = new Rect(0, 0, 0.5f, 1.0f);
        rect.width *= 2 * eyeRect.width;
        rect.x = eyeRect.x + 2 * rect.x * eyeRect.width;
        rect.height *= eyeRect.height;
        rect.y = eyeRect.y + rect.y * eyeRect.height;
        return rect;
    }

    private void UpdateSimulatedSensor()
    {
        bool rolled = false;
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
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
            mouseY = Mathf.Clamp(mouseY, -91, 91);
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            rolled = true;
            mouseZ += Input.GetAxis("Mouse X") * 5;
            mouseZ = Mathf.Clamp(mouseZ, -91, 91);
        }
        if (!rolled && autoUntiltHead)
        {
            mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
        }
        var rot = Quaternion.Euler(mouseY, mouseX, mouseZ);
        var neck = (rot * neckOffset - neckOffset.y * Vector3.up) * neckModelScale;
        Matrix4x4 Matrix1 = Matrix4x4.TRS(neck, rot, Vector3.one);
        Pvr_UnitySDKManager.SDK.HeadPose = new Pvr_UnitySDKPose(Matrix1);

    }

    private void InitForEye(ref Material mat, ref Material mat1)
    {
        Shader shader = Shader.Find("Pvr_UnitySDK/Undistortion");
        Shader shader1 = Shader.Find("Pvr_UnitySDK/FillColor");
        if (shader == null || shader1 == null)
        {
            PLOG.E("Ths Shader Missing ！");
            return;
        }
        mat = new Material(shader);
        mat1 = new Material(shader1);
    }

    private void EnableVEmodel()
    {
        vrModeEnabled = !vrModeEnabled;
        Pvr_UnitySDKManager.SDK.VRModeEnabled = vrModeEnabled;
    }
    #endregion

}
