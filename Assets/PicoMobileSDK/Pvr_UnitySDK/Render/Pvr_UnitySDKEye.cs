// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System.Collections;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(Camera))]
public class Pvr_UnitySDKEye : MonoBehaviour
{
    public static List<Pvr_UnitySDKEye> Instances = new List<Pvr_UnitySDKEye>();

    /************************************    Properties  *************************************/
    #region Properties
    public Eye eyeSide;

    public Camera eyecamera { get; private set; }

    #region BoundarySystem
    private int eyeCameraOriginCullingMask;
    private CameraClearFlags eyeCameraOriginClearFlag;
    private Color eyeCameraOriginBackgroundColor;
    private int applicationOriginFrameRate;

    private bool isBoundaryLimitFrameRate;
    private int lastSaveBoundaryState = 0;
    #endregion

    private Pvr_UnitySDKEyeManager controller;
    Matrix4x4 realProj = Matrix4x4.identity;
    private const int bufferSize = 3;
    private int IDIndex;

    private RenderEventType eventType = 0;

    private bool isFadeUsing;
    private float elapsedTime;
    public float fadeTime = 5.0f;
    public Color fadeColor = new Color(0f, 0f, 0f, 1.0f);
    private Material fadeMaterial;
    private Color fadeMaterialColor = new Color(0f, 0f, 0f, 1.0f);
    private static MeshFilter fadeMeshFilter;
    private static MeshRenderer fadeMeshRenderer;
    private bool isFading;
    private static Vector2 FoveationGainValue = Vector2.zero;
    private static float FoveationAreaValue;
    private static float FoveationMinimumValue;
    private int previousId = 0;


    public Pvr_UnitySDKEyeManager Controller
    {
        get
        {
            if (transform.parent == null)
            {
                return null;
            }
            if ((Application.isEditor && !Application.isPlaying) || controller == null)
            {
                return transform.parent.GetComponentInParent<Pvr_UnitySDKEyeManager>();
            }
            return controller;
        }
    }


    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        Instances.Add(this);

        eyecamera = GetComponent<Camera>();
        Pvr_UnitySDKEyeManager.FFRLevelChanged += SetFFRParamByLevel;
        SetFFRParamByLevel();
    }

    void Start()
    {
        Setup(eyeSide);
        SetupUpdate();
        if (eyecamera != null)
        {
            #region BoundarySystem
            // record
            eyeCameraOriginCullingMask = eyecamera.cullingMask;
            eyeCameraOriginClearFlag = eyecamera.clearFlags;
            eyeCameraOriginBackgroundColor = eyecamera.backgroundColor;

            applicationOriginFrameRate = Application.targetFrameRate;
            isBoundaryLimitFrameRate = BoundarySystem.UPvr_GetFrameRateLimit();
            #endregion
        }

    }

    void Update()
    {
        if (Pvr_UnitySDKManager.SDK.trackingmode == 2 || Pvr_UnitySDKManager.SDK.trackingmode == 3)
        {
#if ANDROID_DEVICE
            if (!Pvr_UnitySDKManager.SDK.HmdOnlyrot)
            {
                if (Pvr_UnitySDKManager.SDK.DefaultRange)
                {
                    if (Mathf.Sqrt(Mathf.Pow(Pvr_UnitySDKManager.SDK.HeadPose.Position.x, 2.0f) + Mathf.Pow(Pvr_UnitySDKManager.SDK.HeadPose.Position.z, 2.0f)) >= 0.8f)
                    {
                        isFading = true;
                        fadeMaterialColor = new Color(0f, 0f, 0f,
                            Mathf.Clamp((Mathf.Max(Mathf.Abs(Pvr_UnitySDKManager.SDK.HeadPose.Position.x),
                                             Mathf.Abs(Pvr_UnitySDKManager.SDK.HeadPose.Position.z)) - 0.8f) /
                                        0.16f, 0f, 0.3f));
                        
                    }
                    else
                    {
                        if (isFadeUsing)
                        {
                            if (elapsedTime >= fadeTime)
                            {
                                fadeMaterialColor = new Color(0f, 0f, 0f, 0f);
                                isFading = false;
                            }
                        }
                        else
                        {
                            fadeMaterialColor = new Color(0f, 0f, 0f, 0f);
                            isFading = false;
                        }
                    }
                }
                else
                {
                    if (Mathf.Sqrt(Mathf.Pow(Pvr_UnitySDKManager.SDK.HeadPose.Position.x, 2.0f) + Mathf.Pow(Pvr_UnitySDKManager.SDK.HeadPose.Position.z, 2.0f)) >= Pvr_UnitySDKManager.SDK.CustomRange)
                    {
                        isFading = true;
                        fadeMaterialColor = new Color(0f, 0f, 0f,
                            Mathf.Clamp((Mathf.Max(Mathf.Abs(Pvr_UnitySDKManager.SDK.HeadPose.Position.x),
                                             Mathf.Abs(Pvr_UnitySDKManager.SDK.HeadPose.Position.z)) - Pvr_UnitySDKManager.SDK.CustomRange) /
                                        (Pvr_UnitySDKManager.SDK.CustomRange / 5f), 0f, 0.3f));
                    }
                    else
                    {
                        if (isFadeUsing)
                        {
                            if (elapsedTime >= fadeTime)
                            {
                                fadeMaterialColor = new Color(0f, 0f, 0f, 0f);
                                isFading = false;
                            }
                        }
                        else
                        {
                            fadeMaterialColor = new Color(0f, 0f, 0f, 0f);
                            isFading = false;
                        }
                    }
                }
                SetFadeMeshEnable();
            }
#endif
        }


        // boundary
        if (eyecamera != null && eyecamera.enabled)
        {
            int currentBoundaryState = BoundarySystem.UPvr_GetSeeThroughState();

            if (currentBoundaryState != this.lastSaveBoundaryState)
            {
                if (currentBoundaryState == 2) // close camera render(close camera render) and limit framerate(if needed)
                {
                    // record
                    eyeCameraOriginCullingMask = eyecamera.cullingMask;
                    eyeCameraOriginClearFlag = eyecamera.clearFlags;
                    eyeCameraOriginBackgroundColor = eyecamera.backgroundColor;

                    // close render
                    eyecamera.cullingMask = 0;
                    eyecamera.clearFlags = CameraClearFlags.SolidColor;
                    eyecamera.backgroundColor = Color.black;

                    if (isBoundaryLimitFrameRate)
                    {
                        if (Application.targetFrameRate != 30)
                        {
                            Application.targetFrameRate = 30;
                        }
                    }

                }
                else if (currentBoundaryState == 1) // open camera render, but limit framerate(if needed)
                {
                    if (this.lastSaveBoundaryState == 2)
                    {
                        if (eyecamera.cullingMask == 0)
                        {
                            eyecamera.cullingMask = eyeCameraOriginCullingMask;
                        }
                        if (eyecamera.clearFlags == CameraClearFlags.SolidColor)
                        {
                            eyecamera.clearFlags = eyeCameraOriginClearFlag;
                        }
                        if (eyecamera.backgroundColor == Color.black)
                        {
                            eyecamera.backgroundColor = eyeCameraOriginBackgroundColor;
                        }                       
                    }

                    if (isBoundaryLimitFrameRate)
                    {
                        if (Application.targetFrameRate != 30)
                        {
                            Application.targetFrameRate = 30;
                        }
                    }
                }
                else // open camera render(recover)
                {
                    if ((this.lastSaveBoundaryState == 2 || this.lastSaveBoundaryState == 1))
                    {
                        if (eyecamera.cullingMask == 0)
                        {
                            eyecamera.cullingMask = eyeCameraOriginCullingMask;
                        }
                        if (eyecamera.clearFlags == CameraClearFlags.SolidColor)
                        {
                            eyecamera.clearFlags = eyeCameraOriginClearFlag;
                        }
                        if (eyecamera.backgroundColor == Color.black)
                        {
                            eyecamera.backgroundColor = eyeCameraOriginBackgroundColor;
                        }

                        if (isBoundaryLimitFrameRate)
                        {
                            Application.targetFrameRate = applicationOriginFrameRate;
                        }
                    }
                }

                this.lastSaveBoundaryState = currentBoundaryState;
            }
        }
    }


    void OnEnable()
    {
        isFadeUsing = Pvr_UnitySDKManager.SDK.ScreenFade;
        if (isFadeUsing)
        {
            fadeMaterial = new Material(Shader.Find("Pvr_UnitySDK/Fade"));
            if (fadeMaterial != null)
            {
                PLOG.I("Get fade material success");
            }
            else
            {
                PLOG.I("Get fade material Error");
                isFadeUsing = false;
            }
            bool fade = false;
            if (Pvr_UnitySDKManager.StereoRendering != null && eyeSide == Eye.BothEye)
            {
                fade = true;
            }
            if (Pvr_UnitySDKManager.StereoRendering == null && eyeSide != Eye.BothEye)
            {
                fade = true;
            }
            if (fade)
            {
                StartCoroutine(ScreenFade(1, 0));
            }
        }
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += MyPreRender;
#endif
#if UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            RenderPipelineManager.beginCameraRendering += MyPreRender;
            RenderPipelineManager.endCameraRendering += MyPostRender;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering -= MyPreRender;
#endif
#if UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            RenderPipelineManager.beginCameraRendering -= MyPreRender;
            RenderPipelineManager.endCameraRendering -= MyPostRender;
        }
#endif
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }

    public void MyPreRender(Camera camera)
    {
        if (camera.gameObject != this.gameObject)
            return;
        OnPreRender();
    }

    public void MyPreRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject != this.gameObject)
            return;
        OnPreRender();
    }

    public void MyPostRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject != this.gameObject)
            return;
        OnPostRender();
    }

    public static bool setLevel = false;

    void OnPreRender()
    {
        if (!eyecamera.enabled)
            return;
#if ANDROID_DEVICE
        if (Pvr_UnitySDKManager.StereoRendering != null)
        {
            Pvr_UnitySDKManager.StereoRendering.OnSDKPreRender();
        }
        SetFFRParameter();
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.BeginEye);
#endif
    }

    void OnPostRender()
    {
        if (!eyecamera.enabled)
            return;
        //DrawVignetteLine();

        // eyebuffer
        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(Pvr_UnitySDKManager.SDK.eyeTextureIds[IDIndex]));
        Pvr_UnitySDKPluginEvent.Issue(eventType);

#if ANDROID_DEVICE
        if (Pvr_UnitySDKManager.StereoRendering != null)
        {
            Pvr_UnitySDKManager.StereoRendering.OnSDKPostRender();
        }
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.EndEye);
#endif
    }

#if UNITY_EDITOR
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        ModifyShadePara();
        Graphics.Blit(source, dest, Pvr_UnitySDKManager.SDK.Eyematerial);
    }

    void ModifyShadePara()
    {
        Matrix4x4 proj = Matrix4x4.identity;
        float near = GetComponent<Camera>().nearClipPlane;
        float far = GetComponent<Camera>().farClipPlane;
        float aspectFix = GetComponent<Camera>().rect.height / GetComponent<Camera>().rect.width / 2;

        proj[0, 0] *= aspectFix;
        Vector2 dir = transform.localPosition; // ignore Z
        dir = dir.normalized * 1.0f;
        proj[0, 2] *= Mathf.Abs(dir.x);
        proj[1, 2] *= Mathf.Abs(dir.y); proj[2, 2] = (near + far) / (near - far);
        proj[2, 3] = 2 * near * far / (near - far);

        Vector4 projvec = new Vector4(proj[0, 0], proj[1, 1],
                                    proj[0, 2] - 1, proj[1, 2] - 1) / 2;

        Vector4 unprojvec = new Vector4(realProj[0, 0], realProj[1, 1],
                                        realProj[0, 2] - 1, realProj[1, 2] - 1) / 2;

        float distortionFactor = 0.0241425f;
        Shader.SetGlobalVector("_Projection", projvec);
        Shader.SetGlobalVector("_Unprojection", unprojvec);
        Shader.SetGlobalVector("_Distortion1",
                                new Vector4(Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k1, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k2, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k3, distortionFactor));
        Shader.SetGlobalVector("_Distortion2",
                               new Vector4(Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k4, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k5, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k6));

    }

#endif
    #endregion

    /************************************ Public Interfaces  *********************************/
    #region Public Interfaces

    public void EyeRender()
    {
        SetupUpdate();
        if (Pvr_UnitySDKManager.SDK.eyeTextures[IDIndex] != null)
        {
            Pvr_UnitySDKManager.SDK.eyeTextures[IDIndex].DiscardContents();
            eyecamera.targetTexture = Pvr_UnitySDKManager.SDK.eyeTextures[IDIndex];
        }
    }

    #endregion

    /************************************ Private Interfaces  *********************************/
    #region Private Interfaces
    private void Setup(Eye eyeSide)
    {
        eyecamera = GetComponent<Camera>();
        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            transform.localPosition = Pvr_UnitySDKManager.SDK.EyeOffset(eyeSide);
        }
        else if (eyeSide == Eye.BothEye)
        {
            transform.localPosition = Vector3.zero;
        }
        eyecamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        eyecamera.rect = new Rect(0, 0, 1, 1);
#if UNITY_EDITOR
        eyecamera.rect = Pvr_UnitySDKManager.SDK.EyeRect(eyeSide);
#endif
        //  AW
        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            eventType = (eyeSide == Eye.LeftEye) ?
                        RenderEventType.LeftEyeEndFrame :
                        RenderEventType.RightEyeEndFrame;
        }
        else
        {
            eventType = RenderEventType.BothEyeEndFrame;
        }

    }

    private void SetupUpdate()
    {
#if !UNITY_EDITOR
        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            eyecamera.enabled = !(Pvr_UnitySDKManager.SDK.Monoscopic || Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass);
        }
        else if (eyeSide == Eye.BothEye)
        {
            eyecamera.enabled = Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass;
        }
#endif
        eyecamera.fieldOfView = Pvr_UnitySDKManager.SDK.EyeVFoV;
        eyecamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            IDIndex = Pvr_UnitySDKManager.SDK.currEyeTextureIdx + (int)eyeSide * bufferSize;
        }
        else if (eyeSide == Eye.BothEye)
        {
            IDIndex = Pvr_UnitySDKManager.SDK.currEyeTextureIdx;
        }
    }

    public void RefreshCameraPosition(float ipd)
    {

        Pvr_UnitySDKManager.SDK.leftEyeOffset = new Vector3(-ipd / 2, 0, 0);
        Pvr_UnitySDKManager.SDK.rightEyeOffset = new Vector3(ipd / 2, 0, 0);

        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            transform.localPosition = Pvr_UnitySDKManager.SDK.EyeOffset(eyeSide);
        }
        else if (eyeSide == Eye.BothEye)
        {
            transform.localPosition = Vector3.zero;
        }
    }

    #region  DrawVignetteLine

    private Material mat_Vignette;

    void DrawVignetteLine()
    {
        if (null == mat_Vignette)
        {
            mat_Vignette = new Material(Shader.Find("Diffuse"));//Mobile/
            if (null == mat_Vignette)
            {
                return;
            }
        }
        GL.PushMatrix();
        mat_Vignette.SetPass(0);
        GL.LoadOrtho();
        vignette();
        GL.PopMatrix();
        //screenFade();
    }

    void CreateFadeMesh()
    {
        if (Pvr_UnitySDKEye.fadeMeshFilter != null)
            return;
        
        fadeMeshFilter = gameObject.AddComponent<MeshFilter>();
        fadeMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        var mesh = new Mesh();
        fadeMeshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[4];

        float width = 2f;
        float height = 2f;
        float depth = 1f;

        vertices[0] = new Vector3(-width, -height, depth);
        vertices[1] = new Vector3(width, -height, depth);
        vertices[2] = new Vector3(-width, height, depth);
        vertices[3] = new Vector3(width, height, depth);

        mesh.vertices = vertices;

        int[] tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        mesh.triangles = tri;

        Vector3[] normals = new Vector3[4];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        mesh.normals = normals;

        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        mesh.uv = uv;
    }

    void DestroyFadeMesh()
    {
        if (fadeMeshRenderer != null)
            Destroy(fadeMeshRenderer);

        if (fadeMeshFilter != null)
            Destroy(fadeMeshFilter);
    }

    IEnumerator ScreenFade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0.0f;
        Color color = fadeColor;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / fadeTime));
            isFading = color.a > 0;
            fadeMaterialColor = color;
            SetFadeMeshEnable();            
            
            yield return new WaitForEndOfFrame();
        }
    }

    void SetFadeMeshEnable()
    {
        if (!isFading)
            return;
        if (Pvr_UnitySDKEye.fadeMeshFilter == null)
            CreateFadeMesh();
        if(fadeMaterial == null)
        {
            fadeMaterial = new Material(Shader.Find("Pvr_UnitySDK/Fade"));
        }
        fadeMaterial.color = fadeMaterialColor;
        fadeMaterial.renderQueue = 5000;
        fadeMeshRenderer.material = fadeMaterial;
        fadeMeshRenderer.enabled = isFading;
        if (!isFading)
            DestroyFadeMesh();
    }

    void vignette()
    {
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);
        //top
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.995f, 0.0f);
        GL.Vertex3(0.0f, 0.995f, 0.0f);
        //bottom
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.005f, 0.0f);
        GL.Vertex3(1.0f, 0.005f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);
        //left
        GL.Vertex(new Vector3(0.0f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(0.005f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(0.005f, 0.0f, 0.0f));
        GL.Vertex(new Vector3(0.0f, 0.0f, 0.0f));
        //right
        GL.Vertex(new Vector3(0.995f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(1.0f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(1.0f, 0.0f, 0.0f));
        GL.Vertex(new Vector3(0.995f, 0.0f, 0.0f));
        GL.End();
    }

    #endregion

    #endregion



    private void SetFFRParamByLevel()
    {
        switch (Pvr_UnitySDKEyeManager.Instance.FoveationLevel)
        {
            case EFoveationLevel.None:
                SetFoveatedRenderingParameters(Vector2.zero, 0.0f, 0.0f);
                break;
            case EFoveationLevel.Low:
                SetFoveatedRenderingParameters(new Vector2(2.0f, 2.0f), 0.0f, 0.125f);
                break;
            case EFoveationLevel.Med:
                SetFoveatedRenderingParameters(new Vector2(3.0f, 3.0f), 1.0f, 0.125f);
                break;
            case EFoveationLevel.High:
                SetFoveatedRenderingParameters(new Vector2(4.0f, 4.0f), 2.0f, 0.125f);
                break;
        }
    }

    private void SetFFRParameter()
    {
        if (FoveationGainValue.x <= float.Epsilon && FoveationGainValue.y <= float.Epsilon && FoveationAreaValue <= float.Epsilon && FoveationMinimumValue <= float.Epsilon)
            return;

        Vector3 eyePoint = Vector3.zero;
        if (Pvr_UnitySDKManager.SDK.isEnterVRMode)
        {
            eyePoint = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingPos();
        }
        int eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[IDIndex];
        Pvr_UnitySDKAPI.Render.UPvr_SetFoveationParameters(eyeTextureId, previousId, eyePoint.x, eyePoint.y, FoveationGainValue.x, FoveationGainValue.y, FoveationAreaValue, FoveationMinimumValue);
        previousId = eyeTextureId;
    }

    public static void GetFoveatedRenderingParameters(ref Vector2 ffrGainValue, ref float ffrAreaValue, ref float ffrMinimumValue)
    {
        ffrGainValue = FoveationGainValue;
        ffrAreaValue = FoveationAreaValue;
        ffrMinimumValue = FoveationMinimumValue;
    }

    public static void SetFoveatedRenderingParameters(Vector2 ffrGainValue, float ffrAreaValue, float ffrMinimumValue)
    {
        FoveationGainValue = ffrGainValue;
        FoveationAreaValue = ffrAreaValue;
        FoveationMinimumValue = ffrMinimumValue;
        Debug.Log("SetFoveatedRenderingParameters GainValue= " + FoveationGainValue.ToString("f5") + " AreaValue " + FoveationAreaValue + " MinimumValue " + FoveationMinimumValue);
    }

}