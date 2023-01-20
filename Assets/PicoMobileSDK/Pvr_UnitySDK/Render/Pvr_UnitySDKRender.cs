// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using Pvr_UnitySDKAPI;
using UnityEngine.Rendering;

public class Pvr_UnitySDKRender
{
    public Pvr_UnitySDKRender()
    {
        if (!canConnecttoActivity)
        {

            ConnectToAndriod();
            PLOG.I("PvrLog Init Render Ability Success!");
            isInitrenderThread = false;
        }
        Init();
    }

    /************************************    Properties  *************************************/
    #region Properties
#if ANDROID_DEVICE
    public AndroidJavaObject activity;
    public static AndroidJavaClass javaVrActivityClass;
    public static AndroidJavaClass javaSysActivityClass;  
    public static AndroidJavaClass javaserviceClass;
	public static AndroidJavaClass javaVrActivityLongReceiver;
    public static AndroidJavaClass javaVrActivityClientClass;
#endif

    private bool canConnecttoActivity = false;
    public bool CanConnecttoActivity
    {
        get { return canConnecttoActivity; }
        set
        {
            if (value != canConnecttoActivity)
                canConnecttoActivity = value;
        }
    }

    private bool isInitrenderThread = true;
    private string model;
    private Vector2 prefinger1 = new Vector2(0.0f, 0.0f);
    private Vector2 prefinger2 = new Vector2(0.0f, 0.0f);
    #endregion

    /************************************   Public Interfaces **********************************/
    #region       PublicInterfaces

    public void ConnectToAndriod()
    {
#if ANDROID_DEVICE
        try
        {      
            Debug.Log("PvrLog SDK Version :  " + Pvr_UnitySDKAPI.System.UPvr_GetSDKVersion().ToString() + "  Unity Script Version :" +  Pvr_UnitySDKAPI.System.UPvr_GetUnitySDKVersion().ToString());
            UnityEngine.AndroidJavaClass unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
            javaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.VrActivity");
            javaserviceClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.UnityClient");
			javaVrActivityLongReceiver = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.HomeKeyReceiver");
            javaSysActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.SysActivity");
            javaVrActivityClientClass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.PvrClient");
			Pvr_UnitySDKAPI.System.Pvr_SetInitActivity(activity.GetRawObject(), javaVrActivityClass.GetRawClass());
            model = javaVrActivityClass.CallStatic<string>("Pvr_GetBuildModel");

            double[] parameters = new double[5];
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref parameters, javaVrActivityClass, "getDPIParameters", activity);
            int platformType = -1 ;
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.PLATFORM_TYPE;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex,ref platformType);

            string systemfps = "";
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref systemfps, javaserviceClass, "getSysproc", "persist.pvr.debug.appfps");
            if(systemfps != "")
                Pvr_UnitySDKManager.SDK.systemFPS = Convert.ToBoolean(Convert.ToInt16(systemfps));
        
            if (platformType == 0)
            {
                 Pvr_UnitySDKAPI.Render.UPvr_ChangeScreenParameters(model, (int)parameters[0], (int)parameters[1], parameters[2], parameters[3], parameters[4]);				 
				 Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
            {
                bool setMonoPresentation = Pvr_UnitySDKAPI.System.UPvr_SetMonoPresentation();
                Debug.Log("ConnectToAndriod set monoPresentation success ?-------------" + setMonoPresentation.ToString());

                bool isPresentationExisted = Pvr_UnitySDKAPI.System.UPvr_IsPresentationExisted();
                Debug.Log("ConnectToAndriod presentation existed ?-------------" + isPresentationExisted.ToString());
            }
        
        }
        catch (AndroidJavaException e)
        {
            PLOG.E("ConnectToAndriod--catch" + e.Message);
        }
#endif
        canConnecttoActivity = true;
    }

    public void Init()
    {
        if (InitRenderAbility())
        {
            Debug.Log("PvrLog Init Render Ability Success!");
            isInitrenderThread = false;
        }
        else
            Debug.Log("PvrLog Init Render Ability Failed!");
    }

    /************************************  Private Interfaces **********************************/
    #region     Private Interfaces

    private bool InitRenderAbility()
    {
        if (UpdateRenderParaFrame())
        {
            if (CreateEyeBuffer())
            {
                float separation = Pvr_UnitySDKAPI.System.UPvr_GetIPD();
                Pvr_UnitySDKManager.SDK.leftEyeOffset = new Vector3(-separation / 2, 0, 0);
                Pvr_UnitySDKManager.SDK.rightEyeOffset = new Vector3(separation / 2, 0, 0);
                return true;
            }
        }
        return false;
    }

    private bool UpdateRenderParaFrame()
    {
        Pvr_UnitySDKManager.SDK.EyeVFoV = GetEyeVFOV();
        Pvr_UnitySDKManager.SDK.EyeHFoV = GetEyeHFOV();
        Pvr_UnitySDKManager.SDK.EyesAspect = Pvr_UnitySDKManager.SDK.EyeHFoV / Pvr_UnitySDKManager.SDK.EyeVFoV;
        return true;
    }

    private bool CreateEyeBuffer()
    {
        Vector2 resolution = GetEyeBufferResolution();
        Pvr_UnitySDKAPI.System.UPvr_SetSinglePassDepthBufferWidthHeight((int)resolution.x, (int)resolution.y);       
        Pvr_UnitySDKManager.SDK.eyeTextures = new RenderTexture[Pvr_UnitySDKManager.eyeTextureCount];

        // eye buffer
        for (int i = 0; i < Pvr_UnitySDKManager.eyeTextureCount; i++)
        {
            if (null == Pvr_UnitySDKManager.SDK.eyeTextures[i])
            {
                try
                {
                    ConfigureEyeBuffer(i, resolution);
                }
                catch (Exception e)
                {
                    PLOG.E("ConfigureEyeBuffer ERROR " + e.Message);
                    throw;
                }
            }

            if (!Pvr_UnitySDKManager.SDK.eyeTextures[i].IsCreated())
            {
                Pvr_UnitySDKManager.SDK.eyeTextures[i].Create();
                Pvr_UnitySDKManager.SDK.eyeTextureIds[i] = Pvr_UnitySDKManager.SDK.eyeTextures[i].GetNativeTexturePtr().ToInt32();
            }
            Pvr_UnitySDKManager.SDK.eyeTextureIds[i] = Pvr_UnitySDKManager.SDK.eyeTextures[i].GetNativeTexturePtr().ToInt32();
        }
        return true;
    }

    public float GetEyeVFOV()
    {
        float fov = 102;
        try
        {
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.VFOV;
            Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref fov);
            if (fov <= 0)
            {
                fov = 102;
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("GetEyeVFOV ERROR! " + e.Message);
            throw;
        }

        return fov;
    }

    public float GetEyeHFOV()
    {
        float fov = 102;
        try
        {
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.HFOV;
            Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref fov);
            if (fov <= 0)
            {
                fov = 102;
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("GetEyeHFOV ERROR! " + e.Message);
            throw;
        }

        return fov;
    }

    private void ConfigureEyeBuffer(int eyeTextureIndex, Vector2 resolution)
    {
        int x = (int)resolution.x;
        int y = (int)resolution.y;
        Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex] = new RenderTexture(x, y, (int)Pvr_UnitySDKProjectSetting.GetProjectConfig().rtBitDepth, Pvr_UnitySDKProjectSetting.GetProjectConfig().rtFormat);
        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].anisoLevel = 0;
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)Pvr_UnitySDKProjectSetting.GetProjectConfig().rtAntiAlising);
            Debug.Log("MultiPass ConfigureEyeBuffer eyeTextureIndex " + eyeTextureIndex);
        }
        else if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].useMipMap = false;
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].wrapMode = TextureWrapMode.Clamp;
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].filterMode = FilterMode.Bilinear;
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].anisoLevel = 1;
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].dimension = TextureDimension.Tex2DArray;
            Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].volumeDepth = 2;
            Debug.Log("SinglePass ConfigureEyeBuffer eyeTextureIndex " + eyeTextureIndex);
        }

        Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].Create();
        if (Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].IsCreated())
        {
            Pvr_UnitySDKManager.SDK.eyeTextureIds[eyeTextureIndex] = Pvr_UnitySDKManager.SDK.eyeTextures[eyeTextureIndex].GetNativeTexturePtr().ToInt32();
            Debug.Log("eyeTextureIndex : " + eyeTextureIndex.ToString());
        }

    }

    public bool ReCreateEyeBuffer()
    {
        if (!Pvr_UnitySDKProjectSetting.GetProjectConfig().usedefaultRenderTexture)
        {
            for (int i = 0; i < Pvr_UnitySDKManager.eyeTextureCount; i++)
            {
                if (Pvr_UnitySDKManager.SDK.eyeTextures[i] != null)
                {
                    Pvr_UnitySDKManager.SDK.eyeTextures[i].Release();
                }
            }

            Array.Clear(Pvr_UnitySDKManager.SDK.eyeTextures, 0, Pvr_UnitySDKManager.SDK.eyeTextures.Length);

            return CreateEyeBuffer();
        }

        return false;
    }
    #endregion

    public void IssueRenderThread()
    {
        if (canConnecttoActivity && !isInitrenderThread)
        {
            ColorSpace colorSpace = QualitySettings.activeColorSpace;
            if (colorSpace == ColorSpace.Gamma)
            {
                Pvr_UnitySDKAPI.Render.UPvr_SetColorspaceType(0);
            }
            else if (colorSpace == ColorSpace.Linear)
            {
                Pvr_UnitySDKAPI.Render.UPvr_SetColorspaceType(1);
            }

            Pvr_UnitySDKPluginEvent.Issue(RenderEventType.InitRenderThread);
            isInitrenderThread = true;
            if (Pvr_UnitySDKManager.StereoRendering != null)
            {
                Pvr_UnitySDKManager.StereoRendering.OnSDKRenderInited();
            }
            Debug.Log("PvrLog IssueRenderThread end");
        }
        else
        {
            PLOG.I("PvrLog IssueRenderThread  canConnecttoActivity = " + canConnecttoActivity);
        }
    }

    private void AutoAdpatForPico1s()
    {
        Vector2 finger1 = Input.touches[0].position;
        Vector2 finger2 = Input.touches[1].position;
        if (Vector2.Distance(prefinger1, finger1) > 2.0f && Vector2.Distance(prefinger2, finger2) > 2.0f)
        {
            float x = (Input.touches[0].position.x + Input.touches[1].position.x) / Screen.width - 1.0f;
            float y = (Input.touches[0].position.y + Input.touches[1].position.y) / Screen.height - 1.0f;
            Pvr_UnitySDKAPI.Render.UPvr_SetRatio(x, y);
        }
        prefinger1 = finger1;
        prefinger2 = finger2;
    }

    public static Vector2 GetEyeBufferResolution()
    {
        Vector2 eyeBufferResolution;
        int w = 1024;
        int h = 1024;
        if (Pvr_UnitySDKProjectSetting.GetProjectConfig().usedefaultRenderTexture)
        {
            try
            {
                int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.EYE_TEXTURE_RESOLUTION0;
                Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref w);
                enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.EYE_TEXTURE_RESOLUTION1;
                Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref h);
            }
            catch (System.Exception e)
            {
                PLOG.E("GetEyeBufferResolution ERROR! " + e.Message);
                throw;
            }
        }
        else
        {
            w = (int)(Pvr_UnitySDKProjectSetting.GetProjectConfig().customRTSize.x * Pvr_UnitySDKManager.SDK.RtScaleFactor);
            h = (int)(Pvr_UnitySDKProjectSetting.GetProjectConfig().customRTSize.y * Pvr_UnitySDKManager.SDK.RtScaleFactor);
        }

        eyeBufferResolution = new Vector2(w, h);
        Debug.Log("DISFT Customize RenderTexture:" + eyeBufferResolution + ", scaleFactor: " + Pvr_UnitySDKManager.SDK.RtScaleFactor);

        return eyeBufferResolution;
    }

    public bool GetUsePredictedMatrix()
    {
        return true;
    }

    #endregion

}
