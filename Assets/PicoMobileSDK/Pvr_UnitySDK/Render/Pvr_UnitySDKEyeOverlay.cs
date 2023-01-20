// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using Pvr_UnitySDKAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pvr_UnitySDKEyeOverlay : MonoBehaviour, IComparable<Pvr_UnitySDKEyeOverlay>
{
    public static List<Pvr_UnitySDKEyeOverlay> Instances = new List<Pvr_UnitySDKEyeOverlay>();

    public int layerIndex = 0;
    public OverlayType overlayType = OverlayType.Overlay;
    public OverlayShape overlayShape = OverlayShape.Quad;
    public Transform layerTransform;

    public Texture[] layerTextures = new Texture[2];

    public int[] layerTextureIds = new int[2];
    public Matrix4x4[] MVMatrixs = new Matrix4x4[2];
	public Vector3[] ModelScales = new Vector3[2];
	public Quaternion[] ModelRotations = new Quaternion[2];
	public Vector3[] ModelTranslations = new Vector3[2];
	public Quaternion[] CameraRotations = new Quaternion[2];
	public Vector3[] CameraTranslations = new Vector3[2];
    private Camera[] layerEyeCamera = new Camera[2];

    public bool overrideColorScaleAndOffset = false;
    public Vector4 colorScale = Vector4.one;
    public Vector4 colorOffset = Vector4.zero;

    private Vector4 overlayLayerColorScaleDefault = Vector4.one;
    private Vector4 overlayLayerColorOffsetDefault = Vector4.zero;

    public bool isExternalAndroidSurface = false;
    public IntPtr externalAndroidSurfaceObject = IntPtr.Zero;
    public delegate void ExternalAndroidSurfaceObjectCreated();
    // Will be called after externalAndroidSurfaceObject get created.
    public ExternalAndroidSurfaceObjectCreated externalAndroidSurfaceObjectCreated = null;


    public int CompareTo(Pvr_UnitySDKEyeOverlay other)
    {
        return this.layerIndex.CompareTo(other.layerIndex);
    }

    #region Unity Methods
    private void Awake()
    {
        Instances.Add(this);

        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            this.layerEyeCamera[0] = Pvr_UnitySDKEyeManager.Instance.BothEyeCamera;
            this.layerEyeCamera[1] = Pvr_UnitySDKEyeManager.Instance.BothEyeCamera;
        }
        else
        {
            if (Pvr_UnitySDKManager.SDK.Monoscopic)
            {
                this.layerEyeCamera[0] = Pvr_UnitySDKEyeManager.Instance.MonoEyeCamera;
                this.layerEyeCamera[1] = Pvr_UnitySDKEyeManager.Instance.MonoEyeCamera;
            }
            else
            {
                this.layerEyeCamera[0] = Pvr_UnitySDKEyeManager.Instance.LeftEyeCamera;
                this.layerEyeCamera[1] = Pvr_UnitySDKEyeManager.Instance.RightEyeCamera;
            }
        }

        this.layerTransform = this.GetComponent<Transform>();
#if !UNITY_EDITOR && UNITY_ANDROID 
        if (this.layerTransform != null)
        {
            MeshRenderer render = this.layerTransform.GetComponent<MeshRenderer>();
            if (render != null)
            {
                render.enabled = false;
            }
        }
#endif

        this.InitializeBuffer();
    }

    private void LateUpdate()
    {
        this.UpdateCoords();
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }
    #endregion

    private void InitializeBuffer()
    {
        switch (this.overlayShape)
        {
            case OverlayShape.Quad:
            case OverlayShape.Cylinder:
            case OverlayShape.Equirect:
            //case OverlayShape.CubeMap:
                for (int i = 0; i < this.layerTextureIds.Length; i++)
                {
                    if (this.layerTextures[i] != null)
                    {
                        this.layerTextureIds[i] = this.layerTextures[i].GetNativeTexturePtr().ToInt32();
                    }
                    else
                    {
                        Debug.LogWarning(String.Format("{0} Texture is null!", (Eye)i));
                    }
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Update MV Matrix
    /// </summary>
    private void UpdateCoords()
    {
        if (this.layerTransform == null || !this.layerTransform.gameObject.activeSelf)
        {
            return;
        }

        if (this.layerEyeCamera[0] == null || this.layerEyeCamera[1] == null)
        {
            return;
        }

        //if (this.overlayShape == OverlayShape.Quad || this.overlayShape == OverlayShape.Cylinder)
        {
            // update MV matrix
            for (int i = 0; i < this.MVMatrixs.Length; i++)
            {
                if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
                {
                    Matrix4x4[] unity_StereoWorldToCamera = Pvr_UnitySDKSinglePass.GetStereoWorldToCameraMat();
                    this.MVMatrixs[i] = unity_StereoWorldToCamera[i] * this.layerTransform.localToWorldMatrix;
                }
                else
                {
                    this.MVMatrixs[i] = this.layerEyeCamera[i].worldToCameraMatrix * this.layerTransform.localToWorldMatrix;
                }

            this.ModelScales[i] = this.layerTransform.localScale;
            this.ModelRotations[i] = this.layerTransform.rotation;
            this.ModelTranslations[i] = this.layerTransform.position;
            this.CameraRotations[i] = this.layerEyeCamera[i].transform.rotation;
            this.CameraTranslations[i] = this.layerEyeCamera[i].transform.position;
        }
        }
    }

    #region Public Method
    /// <summary>
    /// Reset Layer Texture
    /// </summary>
    /// <param name="texture"></param>
    public void SetTexture(Texture texture)
    {
        for (int i = 0; i < this.layerTextures.Length; i++)
        {
            this.layerTextures[i] = texture;
        }
        this.InitializeBuffer();
    }

    /// <summary>
    /// Override Color Scale
    /// </summary>
    /// <param name="scale">Scale that the color values</param>
    /// <param name="offset">Offset that the color values</param>
    public void SetLayerColorScaleAndOffset(Vector4 scale, Vector4 offset)
    {
        colorScale = scale;
        colorOffset = offset;
    }

    public Vector4 GetLayerColorScale()
    {
        if (!this.overrideColorScaleAndOffset)
        {
            return this.overlayLayerColorScaleDefault;
        }

        return this.colorScale;
    }

    public Vector4 GetLayerColorOffset()
    {
        if (!this.overrideColorScaleAndOffset)
        {
            return this.overlayLayerColorOffsetDefault;
        }
        return this.colorOffset;
    }
    #endregion

    public enum OverlayShape
    {
        Quad = 0,
        Cylinder = 1,
        Equirect = 2,
        //CubeMap = 3
    }

    public enum OverlayType
    {
        Overlay = 0,
        Underlay = 1
    }
}
