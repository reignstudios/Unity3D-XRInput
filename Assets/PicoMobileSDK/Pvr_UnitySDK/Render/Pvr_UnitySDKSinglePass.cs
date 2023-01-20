// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class Pvr_UnitySDKSinglePass : SDKStereoRendering
{
    private static Camera bothCamera;
    private Matrix4x4[] unity_StereoMatrixP = new Matrix4x4[2];
    private Matrix4x4[] unity_StereoMatrixInvP = new Matrix4x4[2];
    private static Matrix4x4[] unity_StereoWorldToCamera = new Matrix4x4[2];
    private Matrix4x4[] unity_StereoCameraToWorld = new Matrix4x4[2];
    private Matrix4x4[] unity_StereoMatrixVP = new Matrix4x4[2];
    private Vector3[] eyesOffset = new Vector3[2];
    private static Matrix4x4[] eyesOffsetMatrix = new Matrix4x4[2];
    private CommandBuffer commandBufferBeforeForwardOpaque, commandBufferBeforeSkybox, commandBufferAfterSkybox;

    public override void InitEye(Camera eye)
    {
        bothCamera = eye;
        bothCamera.stereoSeparation = 0.0625f;
        bothCamera.stereoConvergence = 0;
        bothCamera.allowHDR = false;
#if UNITY_2017_3_OR_NEWER
        bothCamera.allowDynamicResolution = false;
#endif
        SetEyesPosition();
        SetEyeProjection();
        
        Pvr_UnitySDKSensor.EyeFovChanged += SetEyeProjection;

        Debug.Log("SinglePass Init success! CameraName = " + eye.transform.name + " eye.eyeSide " + eye.GetComponent<Pvr_UnitySDKEye>().eyeSide);
    }

    public override void OnSDKRenderInited()
    {
        OnSDKRenderInited_SinglePass();
    }

    public override void OnSDKPreRender()
    {
        SinglePassPreRender();
    }

    public override void OnSDKPostRender()
    {
        SwitchKeywordAndDeviceView(false);
    }

    public void OnSDKRenderInited_SinglePass()
    {
        Vector4[] unity_StereoScaleOffset = new Vector4[2];
        unity_StereoScaleOffset[0] = new Vector4(1.0f, 1.0f, 0f, 0f);
        unity_StereoScaleOffset[1] = new Vector4(1.0f, 1.0f, 0.5f, 0f);
        Shader.SetGlobalVectorArray("unity_StereoScaleOffset", unity_StereoScaleOffset);
        SetAntiAliasing();
        
        Debug.Log("OnSDKRenderInited_SinglePass");
    }

    private void SetAntiAliasing()
    {
        int antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)Pvr_UnitySDKProjectSetting.GetProjectConfig().rtAntiAlising);
        Pvr_UnitySDKAPI.System.UPvr_SetAntiAliasing(antiAliasing);
        Debug.Log("SetAntiAliasing  antiAliasing = " + antiAliasing);
    }

    public void SetEyesPosition()
    {
        float separation = 0.0625f;
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.IPD;
        if (0 != Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref separation))
        {
            PLOG.E("Cannot get ipd");
            separation = 0.0625f;
        }
        bothCamera.stereoSeparation = separation;
        Vector3 l = new Vector3(-separation / 2.0f, 0f, 0f);
        Vector3 r = new Vector3(separation / 2.0f, 0f, 0f);
        Vector3 center = (l + r) / 2.0f;
        bothCamera.transform.localPosition = center;
        bothCamera.transform.localRotation = Quaternion.identity;

        Vector3 left = l - center;
        Vector3 right = r - center;
        eyesOffset[0] = left;
        eyesOffset[1] = right;
        eyesOffsetMatrix[0] = Matrix4x4.TRS(left, Quaternion.identity, Vector3.one);
        eyesOffsetMatrix[1] = Matrix4x4.TRS(right, Quaternion.identity, Vector3.one);

        Debug.Log("SetEyesPosition left " + left.x + " " + left.y + " " + left.z);
        Debug.Log("SetEyesPosition right " + right.x + " " + right.y + " " + right.z);
    }

    public void SetEyeProjection()
    {
        Matrix4x4 projL, projR;
        projL = GetProjection(bothCamera.nearClipPlane, bothCamera.farClipPlane);
        projR = GetProjection(bothCamera.nearClipPlane, bothCamera.farClipPlane);
        SetStereoProjectionMatrix(projL, projR);

        Debug.Log("mat4 projL \n" + projL.ToString());
        Debug.Log("mat4 projR \n" + projR.ToString());
    }

    private static Matrix4x4 GetProjection(float near, float far)
    {
        if (near < 0.01f)
            near = 0.01f;
        if (far < 0.02f)
            far = 0.02f;

        Matrix4x4 proj = Matrix4x4.identity;

        proj = MakeProjection(near, far);
        return proj;
    }

    public static Matrix4x4 MakeProjection(float n, float f)
    {
        Vector2 resolution = Pvr_UnitySDKRender.GetEyeBufferResolution();
        float fov = Pvr_UnitySDKManager.SDK.EyeVFoV;

        float tanhalffov = Mathf.Tan(fov / 2f * Mathf.Deg2Rad);
        float cothalffov = 1f / tanhalffov;

        Matrix4x4 m = Matrix4x4.zero;

        m[0, 0] = cothalffov / (resolution.x / resolution.y);
        m[1, 1] = cothalffov;
        m[2, 2] = -(f + n) / (f - n);
        m[2, 3] = -2 * f * n / (f - n);
        m[3, 2] = -1;
        return m;
    }

    public void SetStereoProjectionMatrix(Matrix4x4 left, Matrix4x4 right)
    {
        Debug.Log("SinglePass SetStereoProjectionMatrix left \n" + left.ToString());
        Debug.Log("SinglePass SetStereoProjectionMatrix right \n" + right.ToString());
        unity_StereoMatrixP[0] = left;
        unity_StereoMatrixInvP[0] = left.inverse;

        unity_StereoMatrixP[1] = right;
        unity_StereoMatrixInvP[1] = right.inverse;
    }

    private void SwitchKeywordAndDeviceView(bool enable)
    {
        if (enable)
        {
            Shader.EnableKeyword("STEREO_MULTIVIEW_ON");
            Shader.EnableKeyword("UNITY_SINGLE_PASS_STEREO");
            XRSettings.showDeviceView = false;
        }
        else
        {
            XRSettings.showDeviceView = true;            
            Shader.DisableKeyword("UNITY_SINGLE_PASS_STEREO");
            Shader.DisableKeyword("STEREO_MULTIVIEW_ON");
        }
    }

    public static Matrix4x4[] GetStereoWorldToCameraMat()
    {
        Matrix4x4 world2Camera = bothCamera.worldToCameraMatrix;
        unity_StereoWorldToCamera[0] = eyesOffsetMatrix[0].inverse * world2Camera;
        unity_StereoWorldToCamera[1] = eyesOffsetMatrix[1].inverse * world2Camera;

        return unity_StereoWorldToCamera;
    }

    public void SinglePassPreRender()
    {
        SwitchKeywordAndDeviceView(true);        
        Shader.SetGlobalMatrixArray("unity_StereoCameraProjection", unity_StereoMatrixP);
        Shader.SetGlobalMatrixArray("unity_StereoCameraInvProjection", unity_StereoMatrixInvP);
        Shader.SetGlobalMatrixArray("unity_StereoMatrixP", unity_StereoMatrixP);

        Matrix4x4 world2Camera = bothCamera.worldToCameraMatrix;
        Matrix4x4 camera2World = bothCamera.cameraToWorldMatrix;

        unity_StereoCameraToWorld[0] = camera2World * eyesOffsetMatrix[0];
        unity_StereoCameraToWorld[1] = camera2World * eyesOffsetMatrix[1];
        Shader.SetGlobalMatrixArray("unity_StereoCameraToWorld", unity_StereoCameraToWorld);
        unity_StereoWorldToCamera[0] = eyesOffsetMatrix[0].inverse * world2Camera;
        unity_StereoWorldToCamera[1] = eyesOffsetMatrix[1].inverse * world2Camera;
        Shader.SetGlobalMatrixArray("unity_StereoWorldToCamera", unity_StereoWorldToCamera);        

        Vector4[] stereoWorldSpaceCameraPos = {
                bothCamera.transform.position + eyesOffset[0],
                bothCamera.transform.position + eyesOffset[1]
            };
        Shader.SetGlobalVectorArray("unity_StereoWorldSpaceCameraPos", stereoWorldSpaceCameraPos);

        Shader.SetGlobalMatrixArray("unity_StereoMatrixV", unity_StereoWorldToCamera);
        Shader.SetGlobalMatrixArray("unity_StereoMatrixInvV", unity_StereoCameraToWorld);

        unity_StereoMatrixVP[0] = unity_StereoMatrixP[0] * unity_StereoWorldToCamera[0];
        unity_StereoMatrixVP[1] = unity_StereoMatrixP[1] * unity_StereoWorldToCamera[1];
        Shader.SetGlobalMatrixArray("unity_StereoMatrixVP", unity_StereoMatrixVP);

        SetRenderTextureWithDepth();
        SetCommandBuffers();
    }

    void SetCommandBuffers()
    {
        if (commandBufferBeforeForwardOpaque == null)
        {
            commandBufferBeforeForwardOpaque = new CommandBuffer();
            Pvr_UnitySDKPluginEvent.SetSinglePassBeforeForwardOpaque(commandBufferBeforeForwardOpaque);
            commandBufferBeforeForwardOpaque.ClearRenderTarget(true, true, bothCamera.backgroundColor);
            commandBufferBeforeForwardOpaque.name = "SinglePassPrepare";
        }
        bothCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBufferBeforeForwardOpaque);
        bothCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBufferBeforeForwardOpaque);

        if (commandBufferAfterSkybox == null)
            commandBufferAfterSkybox = new CommandBuffer();

        bothCamera.RemoveCommandBuffer(CameraEvent.AfterSkybox, commandBufferAfterSkybox);

        commandBufferAfterSkybox.Clear();
        commandBufferAfterSkybox.SetGlobalMatrixArray("unity_StereoMatrixVP", unity_StereoMatrixVP);
        commandBufferAfterSkybox.name = "SinglePassAfterSkyBox";
        bothCamera.AddCommandBuffer(CameraEvent.AfterSkybox, commandBufferAfterSkybox);

        if (commandBufferBeforeSkybox == null)
            commandBufferBeforeSkybox = new CommandBuffer();

        Matrix4x4 viewMatrix1 = Matrix4x4.LookAt(Vector3.zero, bothCamera.transform.forward, bothCamera.transform.up) * Matrix4x4.Scale(new Vector3(1, 1, -1));
        viewMatrix1 = viewMatrix1.transpose;
        Matrix4x4 proj0 = unity_StereoMatrixP[0];
        Matrix4x4 proj1 = unity_StereoMatrixP[1];
        proj0.m22 = -1.0f;
        proj1.m22 = -1.0f;
        Matrix4x4[] MatrixVPSkybox = new Matrix4x4[2];
        MatrixVPSkybox[0] = proj0 * viewMatrix1;
        MatrixVPSkybox[1] = proj1 * viewMatrix1;

        bothCamera.RemoveCommandBuffer(CameraEvent.BeforeSkybox, commandBufferBeforeSkybox);
        
        commandBufferBeforeSkybox.Clear();
        commandBufferBeforeSkybox.SetGlobalMatrixArray("unity_StereoMatrixVP", MatrixVPSkybox);
        commandBufferBeforeSkybox.name = "SinglePassAfterSkybox";

        bothCamera.AddCommandBuffer(CameraEvent.BeforeSkybox, commandBufferBeforeSkybox);
    }

    private void SetRenderTextureWithDepth()
    {
        int eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
        Pvr_UnitySDKAPI.System.UPvr_SetCurrentRenderTexture((uint)eyeTextureId);
    }
}

public abstract class SDKStereoRendering
{
    public abstract void InitEye(Camera eye);
    public abstract void OnSDKRenderInited();
    public abstract void OnSDKPreRender();
    public abstract void OnSDKPostRender();
}

