// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;

public class Pvr_UnitySDKEyeManager : MonoBehaviour
{
    private static Pvr_UnitySDKEyeManager instance;
    public static Pvr_UnitySDKEyeManager Instance
    {
        get
        {
            if (instance == null)
            {
                PLOG.E("Pvr_UnitySDKEyeManager instance is not init yet...");
                UnityEngine.Object.FindObjectOfType<Pvr_UnitySDKEyeManager>();
            }
            return instance;
        }
    }

    /************************************    Properties  *************************************/
    #region Properties
    /// <summary>
    /// Eyebuffer Layers
    /// </summary>
    private Pvr_UnitySDKEye[] eyes = null;
    public Pvr_UnitySDKEye[] Eyes
    {
        get
        {
            if (eyes == null)
            {
                eyes = Pvr_UnitySDKEye.Instances.ToArray();
            }
            return eyes;
        }
    }

    /// <summary>
    /// Compositor Layers
    /// </summary>
    private Pvr_UnitySDKEyeOverlay[] overlays = null;
    public Pvr_UnitySDKEyeOverlay[] Overlays
    {
        get
        {
            if (overlays == null)
            {
                overlays = Pvr_UnitySDKEyeOverlay.Instances.ToArray();
            }
            return overlays;
        }
    }
    [HideInInspector]
    public Camera LeftEyeCamera;
    [HideInInspector]
    public Camera RightEyeCamera;
    /// <summary>
    /// Mono Camera(only enable when Monoscopic switch on)
    /// </summary>
	[HideInInspector]
    public Camera MonoEyeCamera;
    [HideInInspector]
    public Camera BothEyeCamera;
    /// <summary>
    /// Mono Eye RTexture ID
    /// </summary>
    private int MonoEyeTextureID = 0;

    // wait for a number of frames, because custom splash screen(2D loading) need display time when first start-up.
    private readonly int WaitSplashScreenFrames = 3;
    private bool isFirstStartup = true;
    private int frameNum = 0;

    /// <summary>
    /// Max Compositor Layers
    /// </summary>
    private int MaxCompositorLayers = 15;

    [SerializeField]
    [HideInInspector]
    public bool FoveatedRendering;
    [SerializeField]
    [HideInInspector]
    private EFoveationLevel foveationLevel = EFoveationLevel.None;
    [HideInInspector]
    public EFoveationLevel FoveationLevel
    {
        get
        {
            return foveationLevel;
        }
        set
        {
            if (value != foveationLevel)
            {
                foveationLevel = value;
                if (Application.isPlaying && FFRLevelChanged != null)
                {
                    FFRLevelChanged();
                }
            }
        }
    }
    public static Action FFRLevelChanged;

    [HideInInspector]
    public Vector2 FoveationGainValue = Vector2.zero;
    [HideInInspector]
    public float FoveationAreaValue = 0.0f;
    [HideInInspector]
    public float FoveationMinimumValue = 0.0f;
    #endregion

    /************************************ Process Interface  *********************************/
    #region  Process Interface
    private void SetCameraEnableEditor()
    {
        MonoEyeCamera.enabled = !Pvr_UnitySDKManager.SDK.VRModeEnabled || Pvr_UnitySDKManager.SDK.Monoscopic;
        for (int i = 0; i < Eyes.Length; i++)
        {
            if (Eyes[i].eyeSide == Eye.LeftEye || Eyes[i].eyeSide == Eye.RightEye)
            {
                Eyes[i].eyecamera.enabled = Pvr_UnitySDKManager.SDK.VRModeEnabled;
            }
            else if (Eyes[i].eyeSide == Eye.BothEye)
            {
                Eyes[i].eyecamera.enabled = false;
            }
        }
    }
    private void SetCamerasEnableByStereoRendering()
    {
        MonoEyeCamera.enabled = Pvr_UnitySDKManager.SDK.Monoscopic && Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.MultiPass;
    }
    private void SetupMonoCamera()
    {
        transform.localPosition = Vector3.zero;
        MonoEyeCamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        MonoEyeCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void SetupUpdate()
    {
        MonoEyeCamera.fieldOfView = Pvr_UnitySDKManager.SDK.EyeVFoV;
        MonoEyeCamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        MonoEyeTextureID = Pvr_UnitySDKManager.SDK.currEyeTextureIdx;
    }

    private void MonoEyeRender()
    {
        SetupUpdate();
        if (Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID] != null)
        {
            Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID].DiscardContents();
            MonoEyeCamera.targetTexture = Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID];
        }
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    private void Awake()
    {
        instance = this;
        if (this.MonoEyeCamera == null)
        {
            this.MonoEyeCamera = this.GetComponent<Camera>();
        }
        if (this.LeftEyeCamera == null)
        {
            this.LeftEyeCamera = this.gameObject.transform.Find("LeftEye").GetComponent<Camera>();
        }
        if (this.RightEyeCamera == null)
        {
            this.RightEyeCamera = this.gameObject.transform.Find("RightEye").GetComponent<Camera>();
        }
        if (this.BothEyeCamera == null)
        {
            this.BothEyeCamera = this.gameObject.transform.Find("BothEye").GetComponent<Camera>();
        }
        if (this.BothEyeCamera != null)
        {
            this.BothEyeCamera.transform.GetComponent<Pvr_UnitySDKEye>().eyeSide = Eye.BothEye;
        }

        Pvr_UnitySDKManager.eventEnterVRMode += SetEyeTrackingMode;
    }

    void OnEnable()
    {
        StartCoroutine("EndOfFrame");
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            Pvr_UnitySDKManager.StereoRendering.InitEye(BothEyeCamera);
        }
        SetCamerasEnableByStereoRendering();
        SetupMonoCamera();

        foreach (var t in Pvr_UnitySDKEyeManager.Instance.Overlays)
        {
            if (t.overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Overlay)
            {
                if (t.overlayShape ==
                    Pvr_UnitySDKEyeOverlay.OverlayShape.Cylinder)
                {
                    Debug.Log("DISFT Cylinder OverLay = Enable");
                }
                if (t.overlayShape ==
                    Pvr_UnitySDKEyeOverlay.OverlayShape.Equirect)
                {
                    Debug.Log("DISFT 360 OverLay= Enable");
                }
                if (t.overlayShape ==
                    Pvr_UnitySDKEyeOverlay.OverlayShape.Quad)
                {
                    Debug.Log("DISFT 2D OverLay= Enable");
                }
            }
            if (t.overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Underlay)
            {
                Debug.Log("DISFT UnderLay= Enable");
            }
        }
#endif

#if UNITY_EDITOR
        SetCameraEnableEditor();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        SetCameraEnableEditor();
#endif

        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            for (int i = 0; i < Eyes.Length; i++)
            {
                if (Eyes[i].isActiveAndEnabled && Eyes[i].eyeSide == Eye.BothEye)
                {
                    Eyes[i].EyeRender();
                }
            }
        }

        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
            {
                // Open Stero Eye Render
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (Eyes[i].isActiveAndEnabled && Eyes[i].eyeSide != Eye.BothEye)
                    {
                        Eyes[i].EyeRender();
                    }
                }
            }
            else
            {
                // Open Mono Eye Render
                MonoEyeRender();
            }
        }
    }

    private void OnPause()
    {
        Pvr_UnitySDKManager.eventEnterVRMode -= SetEyeTrackingMode;
    }

    
    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPostRender()
    {
        long eventdata = Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx]);
        // eyebuffer
        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(eventdata);
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.LeftEyeEndFrame);

        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(eventdata);
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.RightEyeEndFrame);
    } 
#endregion

    /************************************  End Of Per Frame  *************************************/
    // for eyebuffer params
    private int eyeTextureId = 0;
    private RenderEventType eventType = RenderEventType.LeftEyeEndFrame;

    private int overlayLayerDepth = 1;
    private int underlayLayerDepth = 0;
    private bool isHeadLocked = false;
    private int layerFlags = 0;

    IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
#if !UNITY_EDITOR
            if (!Pvr_UnitySDKManager.SDK.isEnterVRMode)
            {
                // Call GL.clear before Enter VRMode to avoid unexpected graph breaking.
                GL.Clear(false, true, Color.black);
            }
#endif
            if (isFirstStartup && frameNum == this.WaitSplashScreenFrames)
            {
                Pvr_UnitySDKAPI.System.UPvr_RemovePlatformLogo();
                if (Pvr_UnitySDKManager.SDK.ResetTrackerOnLoad)
                {
                    Debug.Log("Reset Tracker OnLoad");
                    Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                }

                Pvr_UnitySDKAPI.System.UPvr_StartVRModel();
                isFirstStartup = false;
            }
            else if (isFirstStartup && frameNum < this.WaitSplashScreenFrames)
            {
                PLOG.I("frameNum:" + frameNum);
                frameNum++;
            }

#region Eyebuffer
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (!Eyes[i].isActiveAndEnabled || !Eyes[i].eyecamera.enabled)
                    {
                        continue;
                    }

                    switch (Eyes[i].eyeSide)
                    {
                        case Pvr_UnitySDKAPI.Eye.LeftEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            eventType = RenderEventType.LeftEyeEndFrame;
                            break;
                        case Pvr_UnitySDKAPI.Eye.RightEye:
                            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
                            {
                                eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx + 3];
                            }
                            else
                            {
                                eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            }
                            eventType = RenderEventType.RightEyeEndFrame;
                            break;
                        case Pvr_UnitySDKAPI.Eye.BothEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            eventType = RenderEventType.BothEyeEndFrame;
                            break;
                        default:
                            break;
                    }
                    
                    // eyebuffer
                    Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(eyeTextureId));;
			     	Pvr_UnitySDKPluginEvent.Issue(eventType);

                    Pvr_UnitySDKPluginEvent.Issue(RenderEventType.EndEye);
                }
            }
#endif
#endregion

            // Compositor Layers: if find Overlay then Open Compositor Layers feature
#region Compositor Layers
            int boundaryState = BoundarySystem.UPvr_GetSeeThroughState();
            if (Pvr_UnitySDKEyeOverlay.Instances.Count > 0 && boundaryState != 2)
            {
                overlayLayerDepth = 1;
                underlayLayerDepth = 0;

                Pvr_UnitySDKEyeOverlay.Instances.Sort();
                for (int i = 0; i < Overlays.Length; i++)
                {
                    if (!Overlays[i].isActiveAndEnabled) continue;
                    if (Overlays[i].layerTextures[0] == null && Overlays[i].layerTextures[1] == null && !Overlays[i].isExternalAndroidSurface) continue;
                    if (Overlays[i].layerTransform != null && !Overlays[i].layerTransform.gameObject.activeSelf) continue;
                 
                    layerFlags = 0;

                    if (Overlays[i].overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Quad || Overlays[i].overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Cylinder)
                    {
                        if (Overlays[i].overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Overlay)
                        {
                            isHeadLocked = false;
                            if (Overlays[i].layerTransform != null && Overlays[i].layerTransform.parent == this.transform)
                            {
                                isHeadLocked = true;
                            }

                            // external surface
                            if (Overlays[i].isExternalAndroidSurface)
                            {
                                layerFlags = 1;
                                this.CreateExternalSurface(Overlays[i], overlayLayerDepth);
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, overlayLayerDepth, isHeadLocked, layerFlags, Overlays[i].MVMatrixs[0],
							Overlays[i].ModelScales[0], Overlays[i].ModelRotations[0], Overlays[i].ModelTranslations[0], Overlays[i].CameraRotations[0], Overlays[i].CameraTranslations[0], Overlays[i].GetLayerColorScale(), Overlays[i].GetLayerColorOffset());

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, overlayLayerDepth, isHeadLocked, layerFlags, Overlays[i].MVMatrixs[1],
							Overlays[i].ModelScales[1], Overlays[i].ModelRotations[1], Overlays[i].ModelTranslations[1], Overlays[i].CameraRotations[1], Overlays[i].CameraTranslations[1], Overlays[i].GetLayerColorScale(), Overlays[i].GetLayerColorOffset());

                            overlayLayerDepth++;
                        }
                        else if (Overlays[i].overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Underlay)
                        {
                            // external surface
                            if (Overlays[i].isExternalAndroidSurface)
                            {
                                layerFlags = 1;
                                this.CreateExternalSurface(Overlays[i], underlayLayerDepth);
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, underlayLayerDepth, false, layerFlags, Overlays[i].MVMatrixs[0],
							Overlays[i].ModelScales[0], Overlays[i].ModelRotations[0], Overlays[i].ModelTranslations[0], Overlays[i].CameraRotations[0], Overlays[i].CameraTranslations[0], Overlays[i].GetLayerColorScale(), Overlays[i].GetLayerColorOffset());

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, underlayLayerDepth, false, layerFlags, Overlays[i].MVMatrixs[1],
							Overlays[i].ModelScales[1], Overlays[i].ModelRotations[1], Overlays[i].ModelTranslations[1], Overlays[i].CameraRotations[1], Overlays[i].CameraTranslations[1], Overlays[i].GetLayerColorScale(), Overlays[i].GetLayerColorOffset());

                            underlayLayerDepth++;
                        }
                    }
                    else if (Overlays[i].overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Equirect)
                    {
                        // external surface
                        if (Overlays[i].isExternalAndroidSurface)
                        {
                            layerFlags = 1;
                            this.CreateExternalSurface(Overlays[i], 0);
                        }

                        // 360 Overlay Equirectangular Texture
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.LeftEye, Overlays[i].layerTextureIds[0], (int)Overlays[i].overlayShape, layerFlags, Overlays[i].GetLayerColorScale(), Overlays[i].GetLayerColorOffset());
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.RightEye, Overlays[i].layerTextureIds[1], (int)Overlays[i].overlayShape, layerFlags, Overlays[i].GetLayerColorScale(), Overlays[i].GetLayerColorOffset());
                    }
                }
#endregion
            }

            // Begin TimeWarp
            //Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.TimeWarp, Pvr_UnitySDKManager.SDK.RenderviewNumber);
            Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(0));
            Pvr_UnitySDKPluginEvent.Issue(RenderEventType.TimeWarp);
            Pvr_UnitySDKManager.SDK.currEyeTextureIdx = Pvr_UnitySDKManager.SDK.nextEyeTextureIdx;
            Pvr_UnitySDKManager.SDK.nextEyeTextureIdx = (Pvr_UnitySDKManager.SDK.nextEyeTextureIdx + 1) % 3;
        }
    }

    
    /// <summary>
    /// Create External Surface
    /// </summary>
    /// <param name="overlayInstance"></param>
    /// <param name="layerDepth"></param>
    private void CreateExternalSurface(Pvr_UnitySDKEyeOverlay overlayInstance, int layerDepth)
    {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        if (overlayInstance.externalAndroidSurfaceObject == System.IntPtr.Zero)
        {          
            overlayInstance.externalAndroidSurfaceObject = Pvr_UnitySDKAPI.Render.UPvr_CreateLayerAndroidSurface((int)overlayInstance.overlayType, layerDepth);
            Debug.LogFormat("CreateExternalSurface: Overlay Type:{0}, LayerDepth:{1}, SurfaceObject:{2}", overlayInstance.overlayType, layerDepth, overlayInstance.externalAndroidSurfaceObject);

            if (overlayInstance.externalAndroidSurfaceObject != System.IntPtr.Zero)
            {
                if (overlayInstance.externalAndroidSurfaceObjectCreated != null)
                {
                    overlayInstance.externalAndroidSurfaceObjectCreated();
                }
            }
        }
#endif
    }


#region EyeTrack  
    [HideInInspector]
    public bool EyeTracking = false;
    [HideInInspector]
    public Vector3 eyePoint;
    private EyeTrackingData eyePoseData;

    public bool SetEyeTrackingMode()
    {
        int trackingMode = Pvr_UnitySDKAPI.System.UPvr_GetTrackingMode();
        bool supportEyeTracking = (trackingMode & (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE) != 0;
        bool result = false;

        if (EyeTracking && supportEyeTracking)
        {
            result = Pvr_UnitySDKAPI.System.UPvr_setTrackingMode((int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_POSITION | (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE);
        }
        Debug.Log("SetEyeTrackingMode EyeTracking " + EyeTracking + " supportEyeTracking " + supportEyeTracking + " result " + result);
        return result;
    }

    public Vector3 GetEyeTrackingPos()
    {
        if (!Pvr_UnitySDKEyeManager.Instance.EyeTracking)
            return Vector3.zero;

        bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyePoseData);
        if (!result)
        {
            PLOG.E("UPvr_getEyeTrackingData failed " + result);
            return Vector3.zero;
        }

        EyeDeviceInfo info = GetDeviceInfo();
        Vector3 frustumSize = Vector3.zero;
        frustumSize.x = 0.5f * (info.targetFrustumLeft.right - info.targetFrustumLeft.left);
        frustumSize.y = 0.5f * (info.targetFrustumLeft.top - info.targetFrustumLeft.bottom);
        frustumSize.z = info.targetFrustumLeft.near;

        var combinedDirection = eyePoseData.foveatedGazeDirection;
        float denominator = Vector3.Dot(combinedDirection, Vector3.forward);
        if (denominator > float.Epsilon)
        {
            eyePoint = combinedDirection * (frustumSize.z / denominator);
            eyePoint.x /= frustumSize.x; // [-1..1]
            eyePoint.y /= frustumSize.y; // [-1..1]
        }
        return eyePoint;
    }

    private EyeDeviceInfo GetDeviceInfo()
    {
        float vfov = Pvr_UnitySDKManager.SDK.EyeVFoV;
        float tanhalfvfov = Mathf.Tan(vfov / 2f * Mathf.Deg2Rad);

        float hfov = Pvr_UnitySDKManager.SDK.EyeHFoV;
        float tanhalfhfov = Mathf.Tan(hfov / 2f * Mathf.Deg2Rad);

        EyeDeviceInfo info;
        info.targetFrustumLeft.left = -(LeftEyeCamera.nearClipPlane * tanhalfhfov);
        info.targetFrustumLeft.right = LeftEyeCamera.nearClipPlane * tanhalfhfov;
        info.targetFrustumLeft.top = LeftEyeCamera.nearClipPlane * tanhalfvfov;
        info.targetFrustumLeft.bottom = -(LeftEyeCamera.nearClipPlane * tanhalfvfov);
        info.targetFrustumLeft.near = LeftEyeCamera.nearClipPlane;
        info.targetFrustumLeft.far = LeftEyeCamera.farClipPlane;

        info.targetFrustumRight.left = -(RightEyeCamera.nearClipPlane * tanhalfhfov);
        info.targetFrustumRight.right = RightEyeCamera.nearClipPlane * tanhalfhfov;
        info.targetFrustumRight.top = RightEyeCamera.nearClipPlane * tanhalfvfov;
        info.targetFrustumRight.bottom = -(RightEyeCamera.nearClipPlane * tanhalfvfov);
        info.targetFrustumRight.near = RightEyeCamera.nearClipPlane;
        info.targetFrustumRight.far = RightEyeCamera.farClipPlane;

        return info;
    }
    #endregion
}