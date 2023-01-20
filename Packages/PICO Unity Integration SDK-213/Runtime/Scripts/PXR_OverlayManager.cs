﻿/*******************************************************************************
Copyright © 2015-2022 PICO Technology Co., Ltd.All rights reserved.  

NOTICE：All information contained herein is, and remains the property of 
PICO Technology Co., Ltd. The intellectual and technical concepts 
contained hererin are proprietary to PICO Technology Co., Ltd. and may be 
covered by patents, patents in process, and are protected by trade secret or 
copyright law. Dissemination of this information or reproduction of this 
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd. 
*******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.XR.PXR
{
    public class PXR_OverlayManager : MonoBehaviour
    {
        private void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginFrameRendering += BeginRendering;
                RenderPipelineManager.endFrameRendering += EndRendering;
            }
            else
            {
                Camera.onPreRender += OnPreRenderCallBack;
                Camera.onPostRender += OnPostRenderCallBack;
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginFrameRendering -= BeginRendering;
                RenderPipelineManager.endFrameRendering -= EndRendering;
            }
            else
            {
                Camera.onPreRender -= OnPreRenderCallBack;
                Camera.onPostRender -= OnPostRenderCallBack;
            }
#endif
        }

        private void Start()
        {
            // external surface
            if (PXR_OverLay.Instances.Count > 0)
            {
                foreach (var overlay in PXR_OverLay.Instances)
                {
                    if (overlay.isExternalAndroidSurface)
                    {
                        overlay.CreateExternalSurface(overlay);
                    }
                }
            }
        }

        private void BeginRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            foreach (Camera cam in arg2)
            {
                if (cam != null && Camera.main.tag == cam.tag)
                {
                    OnPreRenderCallBack(cam);
                }
            }
        }

        private void EndRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            foreach (Camera cam in arg2)
            {
                if (cam != null && Camera.main.tag == cam.tag)
                {
                    OnPostRenderCallBack(cam);
                }
            }
        }

        private void OnPreRenderCallBack(Camera cam)
        {
            // There is only one XR main camera in the scene.
            if (cam.tag != Camera.main.tag || cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) return;

            //CompositeLayers
            int boundaryState = PXR_Plugin.Boundary.UPxr_GetSeeThroughState();
            if (PXR_OverLay.Instances.Count > 0 && boundaryState != 2)
            {
                foreach (var overlay in PXR_OverLay.Instances)
                {
                    if (!overlay.isActiveAndEnabled) continue;
                    if (overlay.layerTextures[0] == null && overlay.layerTextures[1] == null && !overlay.isExternalAndroidSurface) continue;
                    if (overlay.overlayTransform != null && !overlay.overlayTransform.gameObject.activeSelf) continue;
                    overlay.CreateTexture();
                    PXR_Plugin.Render.UPxr_GetLayerNextImageIndex(overlay.overlayIndex, ref overlay.imageIndex);
                }
            }
        }

        private void OnPostRenderCallBack(Camera cam)
        {
            // There is only one XR main camera in the scene.
            if (cam.tag != Camera.main.tag || cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) return;

            int boundaryState = PXR_Plugin.Boundary.UPxr_GetSeeThroughState();
            if (PXR_OverLay.Instances.Count > 0 && boundaryState != 2)
            {
                PXR_OverLay.Instances.Sort();
                foreach (var compositeLayer in PXR_OverLay.Instances)
                {
                    compositeLayer.UpdateCoords();
                    if (!compositeLayer.isActiveAndEnabled) continue;
                    if (compositeLayer.layerTextures[0] == null && compositeLayer.layerTextures[1] == null && !compositeLayer.isExternalAndroidSurface) continue;
                    if (compositeLayer.overlayTransform != null && !compositeLayer.overlayTransform.gameObject.activeSelf) continue;

                    Vector4 colorScale = compositeLayer.GetLayerColorScale();
                    Vector4 colorBias = compositeLayer.GetLayerColorOffset();
                    bool isHeadLocked = false;
                    if (compositeLayer.overlayTransform != null && compositeLayer.overlayTransform.parent == transform)
                    {
                        isHeadLocked = true;
                    }

                    if (!compositeLayer.isExternalAndroidSurface && !compositeLayer.CopyRT())
                    {
                        return;
                    }

                    if (compositeLayer.overlayShape == PXR_OverLay.OverlayShape.Quad)
                    {
                        PxrLayerQuad2 layerSubmit2 = new PxrLayerQuad2();
                        layerSubmit2.header.layerId = compositeLayer.overlayIndex;
                        layerSubmit2.header.colorScaleX = colorScale.x;
                        layerSubmit2.header.colorScaleY = colorScale.y;
                        layerSubmit2.header.colorScaleZ = colorScale.z;
                        layerSubmit2.header.colorScaleW = colorScale.w;
                        layerSubmit2.header.colorBiasX = colorBias.x;
                        layerSubmit2.header.colorBiasY = colorBias.y;
                        layerSubmit2.header.colorBiasZ = colorBias.z;
                        layerSubmit2.header.colorBiasW = colorBias.w;
                        layerSubmit2.header.compositionDepth = compositeLayer.layerDepth;
                        layerSubmit2.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                        layerSubmit2.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                        layerSubmit2.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                        layerSubmit2.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                        layerSubmit2.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                        layerSubmit2.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                        layerSubmit2.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;
                        layerSubmit2.header.layerShape = PXR_OverLay.OverlayShape.Quad;
                        layerSubmit2.header.useLayerBlend = (UInt32)(compositeLayer.useLayerBlend ? 1 : 0);
                        layerSubmit2.header.layerBlend.srcColor = compositeLayer.srcColor;
                        layerSubmit2.header.layerBlend.dstColor = compositeLayer.dstColor;
                        layerSubmit2.header.layerBlend.srcAlpha = compositeLayer.srcAlpha;
                        layerSubmit2.header.layerBlend.dstAlpha = compositeLayer.dstAlpha;
                        layerSubmit2.header.useImageRect = (UInt32)(compositeLayer.useImageRect ? 1 : 0);
                        layerSubmit2.header.imageRectLeft = compositeLayer.getPxrRectiLeft(true);
                        layerSubmit2.header.imageRectRight = compositeLayer.getPxrRectiLeft(false);

                        layerSubmit2.sizeLeft.x = compositeLayer.modelScales[0].x * Mathf.Min(compositeLayer.dstRectLeft.width, 1 - compositeLayer.dstRectLeft.x);
                        layerSubmit2.sizeLeft.y = compositeLayer.modelScales[0].y * Mathf.Min(compositeLayer.dstRectLeft.height, 1 - compositeLayer.dstRectLeft.y);
                        layerSubmit2.sizeRight.x = compositeLayer.modelScales[0].x * Mathf.Min(compositeLayer.dstRectRight.width, 1 - compositeLayer.dstRectRight.x);
                        layerSubmit2.sizeRight.y = compositeLayer.modelScales[0].y * Mathf.Min(compositeLayer.dstRectRight.height, 1 - compositeLayer.dstRectRight.y);

                        if (isHeadLocked)
                        {
                            layerSubmit2.poseLeft.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.overlayTransform.localPosition.x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.overlayTransform.localPosition.y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.overlayTransform.localPosition.z;

                            layerSubmit2.poseRight.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                            layerSubmit2.poseRight.position.x = compositeLayer.overlayTransform.localPosition.x;
                            layerSubmit2.poseRight.position.y = compositeLayer.overlayTransform.localPosition.y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.overlayTransform.localPosition.z;

                            layerSubmit2.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace |
                                PxrLayerSubmitFlags.PxrLayerFlagHeadLocked);
                        }
                        else
                        {
                            layerSubmit2.poseLeft.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.modelTranslations[0].z;
                            layerSubmit2.poseRight.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseRight.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseRight.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                        }

                        if (compositeLayer.useImageRect)
                        {
                            layerSubmit2.poseLeft.position.x += -0.5f + compositeLayer.dstRectLeft.x + 0.5f * Mathf.Min(compositeLayer.dstRectLeft.width, 1 - compositeLayer.dstRectLeft.x);
                            layerSubmit2.poseLeft.position.y += -0.5f + compositeLayer.dstRectLeft.y + 0.5f * Mathf.Min(compositeLayer.dstRectLeft.height, 1 - compositeLayer.dstRectLeft.y);

                            layerSubmit2.poseRight.position.x += -0.5f + compositeLayer.dstRectRight.x + 0.5f * Mathf.Min(compositeLayer.dstRectRight.width, 1 - compositeLayer.dstRectRight.x);
                            layerSubmit2.poseRight.position.y += -0.5f + compositeLayer.dstRectRight.y + 0.5f * Mathf.Min(compositeLayer.dstRectRight.height, 1 - compositeLayer.dstRectRight.y);
                        }

                        if (PXR_Plugin.Render.UPxr_SubmitLayerQuad2(layerSubmit2))
                        {
                            PxrLayerQuad layerSubmit = new PxrLayerQuad();
                            layerSubmit.header.layerId = compositeLayer.overlayIndex;
                            layerSubmit.header.colorScaleX = colorScale.x;
                            layerSubmit.header.colorScaleY = colorScale.y;
                            layerSubmit.header.colorScaleZ = colorScale.z;
                            layerSubmit.header.colorScaleW = colorScale.w;
                            layerSubmit.header.colorBiasX = colorBias.x;
                            layerSubmit.header.colorBiasY = colorBias.y;
                            layerSubmit.header.colorBiasZ = colorBias.z;
                            layerSubmit.header.colorBiasW = colorBias.w;
                            layerSubmit.header.compositionDepth = compositeLayer.layerDepth;
                            layerSubmit.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                            layerSubmit.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                            layerSubmit.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                            layerSubmit.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                            layerSubmit.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                            layerSubmit.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                            layerSubmit.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;

                            if (isHeadLocked)
                            {
                                layerSubmit.pose.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                                layerSubmit.pose.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                                layerSubmit.pose.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                                layerSubmit.pose.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                                layerSubmit.pose.position.x = compositeLayer.overlayTransform.localPosition.x;
                                layerSubmit.pose.position.y = compositeLayer.overlayTransform.localPosition.y;
                                layerSubmit.pose.position.z = -compositeLayer.overlayTransform.localPosition.z;

                                layerSubmit.header.layerFlags = (UInt32)(
                                    PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace |
                                    PxrLayerSubmitFlags.PxrLayerFlagHeadLocked);
                            }
                            else
                            {
                                layerSubmit.pose.orientation.x = compositeLayer.modelRotations[0].x;
                                layerSubmit.pose.orientation.y = compositeLayer.modelRotations[0].y;
                                layerSubmit.pose.orientation.z = -compositeLayer.modelRotations[0].z;
                                layerSubmit.pose.orientation.w = -compositeLayer.modelRotations[0].w;
                                layerSubmit.pose.position.x = compositeLayer.modelTranslations[0].x;
                                layerSubmit.pose.position.y = compositeLayer.modelTranslations[0].y;
                                layerSubmit.pose.position.z = -compositeLayer.modelTranslations[0].z;

                                layerSubmit.header.layerFlags = (UInt32)(
                                    PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                    PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                            }

                            layerSubmit.width = compositeLayer.modelScales[0].x;
                            layerSubmit.height = compositeLayer.modelScales[0].y;

                            PXR_Plugin.Render.UPxr_SubmitLayerQuad(layerSubmit);
                        }
                    }
                    else if (compositeLayer.overlayShape == PXR_OverLay.OverlayShape.Cylinder)
                    {
                        PxrLayerCylinder2 layerSubmit2 = new PxrLayerCylinder2();
                        layerSubmit2.header.layerId = compositeLayer.overlayIndex;
                        layerSubmit2.header.colorScaleX = colorScale.x;
                        layerSubmit2.header.colorScaleY = colorScale.y;
                        layerSubmit2.header.colorScaleZ = colorScale.z;
                        layerSubmit2.header.colorScaleW = colorScale.w;
                        layerSubmit2.header.colorBiasX = colorBias.x;
                        layerSubmit2.header.colorBiasY = colorBias.y;
                        layerSubmit2.header.colorBiasZ = colorBias.z;
                        layerSubmit2.header.colorBiasW = colorBias.w;
                        layerSubmit2.header.compositionDepth = compositeLayer.layerDepth;
                        layerSubmit2.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                        layerSubmit2.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                        layerSubmit2.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                        layerSubmit2.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                        layerSubmit2.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                        layerSubmit2.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                        layerSubmit2.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;
                        layerSubmit2.header.layerShape = PXR_OverLay.OverlayShape.Cylinder;
                        layerSubmit2.header.useLayerBlend = (UInt32)(compositeLayer.useLayerBlend ? 1 : 0);
                        layerSubmit2.header.layerBlend.srcColor = compositeLayer.srcColor;
                        layerSubmit2.header.layerBlend.dstColor = compositeLayer.dstColor;
                        layerSubmit2.header.layerBlend.srcAlpha = compositeLayer.srcAlpha;
                        layerSubmit2.header.layerBlend.dstAlpha = compositeLayer.dstAlpha;
                        layerSubmit2.header.useImageRect = (UInt32)(compositeLayer.useImageRect ? 1 : 0);
                        layerSubmit2.header.imageRectLeft = compositeLayer.getPxrRectiLeft(true);
                        layerSubmit2.header.imageRectRight = compositeLayer.getPxrRectiLeft(false);

                        if (isHeadLocked)
                        {
                            layerSubmit2.poseLeft.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.overlayTransform.localPosition.x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.overlayTransform.localPosition.y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.overlayTransform.localPosition.z;

                            layerSubmit2.poseRight.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                            layerSubmit2.poseRight.position.x = compositeLayer.overlayTransform.localPosition.x;
                            layerSubmit2.poseRight.position.y = compositeLayer.overlayTransform.localPosition.y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.overlayTransform.localPosition.z;

                            layerSubmit2.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace |
                                PxrLayerSubmitFlags.PxrLayerFlagHeadLocked);
                        }
                        else
                        {
                            layerSubmit2.poseLeft.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.poseRight.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseRight.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseRight.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                        }

                        if (compositeLayer.modelScales[0].z != 0)
                        {
                            layerSubmit2.centralAngleLeft = compositeLayer.modelScales[0].x / compositeLayer.modelScales[0].z;
                            layerSubmit2.centralAngleRight = compositeLayer.modelScales[0].x / compositeLayer.modelScales[0].z;
                        }
                        else
                        {
                            Debug.LogError("PXRLog scale.z is 0");
                        }
                        layerSubmit2.heightLeft = compositeLayer.modelScales[0].y;
                        layerSubmit2.heightRight = compositeLayer.modelScales[0].y;
                        layerSubmit2.radiusLeft = compositeLayer.modelScales[0].z;
                        layerSubmit2.radiusRight = compositeLayer.modelScales[0].z;

                        if (PXR_Plugin.Render.UPxr_SubmitLayerCylinder2(layerSubmit2))
                        {
                            PxrLayerCylinder layerSubmit = new PxrLayerCylinder();
                            layerSubmit.header.layerId = compositeLayer.overlayIndex;
                            layerSubmit.header.colorScaleX = colorScale.x;
                            layerSubmit.header.colorScaleY = colorScale.y;
                            layerSubmit.header.colorScaleZ = colorScale.z;
                            layerSubmit.header.colorScaleW = colorScale.w;
                            layerSubmit.header.colorBiasX = colorBias.x;
                            layerSubmit.header.colorBiasY = colorBias.y;
                            layerSubmit.header.colorBiasZ = colorBias.z;
                            layerSubmit.header.colorBiasW = colorBias.w;
                            layerSubmit.header.compositionDepth = compositeLayer.layerDepth;
                            layerSubmit.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                            layerSubmit.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                            layerSubmit.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                            layerSubmit.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                            layerSubmit.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                            layerSubmit.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                            layerSubmit.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;

                            if (isHeadLocked)
                            {
                                layerSubmit.pose.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                                layerSubmit.pose.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                                layerSubmit.pose.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                                layerSubmit.pose.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                                layerSubmit.pose.position.x = compositeLayer.overlayTransform.localPosition.x;
                                layerSubmit.pose.position.y = compositeLayer.overlayTransform.localPosition.y;
                                layerSubmit.pose.position.z = -compositeLayer.overlayTransform.localPosition.z;

                                layerSubmit.header.layerFlags = (UInt32)(
                                    PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace |
                                    PxrLayerSubmitFlags.PxrLayerFlagHeadLocked);
                            }
                            else
                            {
                                layerSubmit.pose.orientation.x = compositeLayer.modelRotations[0].x;
                                layerSubmit.pose.orientation.y = compositeLayer.modelRotations[0].y;
                                layerSubmit.pose.orientation.z = -compositeLayer.modelRotations[0].z;
                                layerSubmit.pose.orientation.w = -compositeLayer.modelRotations[0].w;
                                layerSubmit.pose.position.x = compositeLayer.modelTranslations[0].x;
                                layerSubmit.pose.position.y = compositeLayer.modelTranslations[0].y;
                                layerSubmit.pose.position.z = -compositeLayer.modelTranslations[0].z;

                                layerSubmit.header.layerFlags = (UInt32)(
                                    PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                    PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                            }

                            if (compositeLayer.modelScales[0].z != 0)
                            {
                                layerSubmit.centralAngle = compositeLayer.modelScales[0].x / compositeLayer.modelScales[0].z;
                            }
                            else
                            {
                                Debug.LogError("PXRLog scale.z is 0");
                            }
                            layerSubmit.height = compositeLayer.modelScales[0].y;
                            layerSubmit.radius = compositeLayer.modelScales[0].z;

                            PXR_Plugin.Render.UPxr_SubmitLayerCylinder(layerSubmit);
                        }
                    }
                    else if (compositeLayer.overlayShape == PXR_OverLay.OverlayShape.Equirect)
                    {
                        PxrLayerEquirect layerSubmit = new PxrLayerEquirect();
                        layerSubmit.header.layerId = compositeLayer.overlayIndex;
                        layerSubmit.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                        layerSubmit.header.colorScaleX = colorScale.x;
                        layerSubmit.header.colorScaleY = colorScale.y;
                        layerSubmit.header.colorScaleZ = colorScale.z;
                        layerSubmit.header.colorScaleW = colorScale.w;
                        layerSubmit.header.colorBiasX = colorBias.x;
                        layerSubmit.header.colorBiasY = colorBias.y;
                        layerSubmit.header.colorBiasZ = colorBias.z;
                        layerSubmit.header.colorBiasW = colorBias.w;
                        layerSubmit.header.compositionDepth = compositeLayer.layerDepth;
                        layerSubmit.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                        layerSubmit.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                        layerSubmit.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                        layerSubmit.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                        layerSubmit.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                        layerSubmit.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                        layerSubmit.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;
                        layerSubmit.header.layerShape = PXR_OverLay.OverlayShape.Equirect;
                        layerSubmit.header.useLayerBlend = (UInt32)(compositeLayer.useLayerBlend ? 1 : 0);
                        layerSubmit.header.layerBlend.srcColor = compositeLayer.srcColor;
                        layerSubmit.header.layerBlend.dstColor = compositeLayer.dstColor;
                        layerSubmit.header.layerBlend.srcAlpha = compositeLayer.srcAlpha;
                        layerSubmit.header.layerBlend.dstAlpha = compositeLayer.dstAlpha;
                        layerSubmit.header.useImageRect = (UInt32)(compositeLayer.useImageRect ? 1 : 0);
                        layerSubmit.header.imageRectLeft = compositeLayer.getPxrRectiLeft(true);
                        layerSubmit.header.imageRectRight = compositeLayer.getPxrRectiLeft(false);

                        layerSubmit.poseLeft.orientation.x = compositeLayer.modelRotations[0].x;
                        layerSubmit.poseLeft.orientation.y = compositeLayer.modelRotations[0].y;
                        layerSubmit.poseLeft.orientation.z = -compositeLayer.modelRotations[0].z;
                        layerSubmit.poseLeft.orientation.w = -compositeLayer.modelRotations[0].w;
                        layerSubmit.poseLeft.position.x = compositeLayer.modelTranslations[0].x;
                        layerSubmit.poseLeft.position.y = compositeLayer.modelTranslations[0].y;
                        layerSubmit.poseLeft.position.z = -compositeLayer.modelTranslations[0].z;

                        layerSubmit.poseRight.orientation.x = compositeLayer.modelRotations[0].x;
                        layerSubmit.poseRight.orientation.y = compositeLayer.modelRotations[0].y;
                        layerSubmit.poseRight.orientation.z = -compositeLayer.modelRotations[0].z;
                        layerSubmit.poseRight.orientation.w = -compositeLayer.modelRotations[0].w;
                        layerSubmit.poseRight.position.x = compositeLayer.modelTranslations[0].x;
                        layerSubmit.poseRight.position.y = compositeLayer.modelTranslations[0].y;
                        layerSubmit.poseRight.position.z = -compositeLayer.modelTranslations[0].z;

                        layerSubmit.radiusLeft = compositeLayer.radius;
                        layerSubmit.radiusRight = compositeLayer.radius;
                        layerSubmit.scaleXLeft = 1 / compositeLayer.dstRectLeft.width;
                        layerSubmit.scaleXRight = 1 / compositeLayer.dstRectRight.width;
                        layerSubmit.scaleYLeft = 1 / compositeLayer.dstRectLeft.height;
                        layerSubmit.scaleYRight = 1 / compositeLayer.dstRectRight.height;
                        layerSubmit.biasXLeft = -compositeLayer.dstRectLeft.x / compositeLayer.dstRectLeft.width;
                        layerSubmit.biasXRight = -compositeLayer.dstRectRight.x / compositeLayer.dstRectRight.width;
                        layerSubmit.biasYLeft = 1 + (compositeLayer.dstRectLeft.y - 1) / compositeLayer.dstRectLeft.height;
                        layerSubmit.biasYRight = 1 + (compositeLayer.dstRectRight.y - 1) / compositeLayer.dstRectRight.height;

                        if (PXR_Plugin.Render.UPxr_SubmitLayerEquirect(layerSubmit))
                        {
                            PxrLayerEquirect2 layerSubmit2 = new PxrLayerEquirect2();
                            layerSubmit2.header.layerId = compositeLayer.overlayIndex;
                            layerSubmit2.header.layerFlags = (UInt32)(
                                    PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                    PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                            layerSubmit2.header.colorScaleX = colorScale.x;
                            layerSubmit2.header.colorScaleY = colorScale.y;
                            layerSubmit2.header.colorScaleZ = colorScale.z;
                            layerSubmit2.header.colorScaleW = colorScale.w;
                            layerSubmit2.header.colorBiasX = colorBias.x;
                            layerSubmit2.header.colorBiasY = colorBias.y;
                            layerSubmit2.header.colorBiasZ = colorBias.z;
                            layerSubmit2.header.colorBiasW = colorBias.w;
                            layerSubmit2.header.compositionDepth = compositeLayer.layerDepth;
                            layerSubmit2.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                            layerSubmit2.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                            layerSubmit2.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                            layerSubmit2.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                            layerSubmit2.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                            layerSubmit2.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                            layerSubmit2.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;
                            layerSubmit2.header.layerShape = (PXR_OverLay.OverlayShape)4;
                            layerSubmit2.header.useLayerBlend = (UInt32)(compositeLayer.useLayerBlend ? 1 : 0);
                            layerSubmit2.header.layerBlend.srcColor = compositeLayer.srcColor;
                            layerSubmit2.header.layerBlend.dstColor = compositeLayer.dstColor;
                            layerSubmit2.header.layerBlend.srcAlpha = compositeLayer.srcAlpha;
                            layerSubmit2.header.layerBlend.dstAlpha = compositeLayer.dstAlpha;
                            layerSubmit2.header.useImageRect = (UInt32)(compositeLayer.useImageRect ? 1 : 0);
                            layerSubmit2.header.imageRectLeft = compositeLayer.getPxrRectiLeft(true);
                            layerSubmit2.header.imageRectRight = compositeLayer.getPxrRectiLeft(false);

                            layerSubmit2.poseLeft.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.poseRight.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseRight.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseRight.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.radiusLeft = compositeLayer.radius;
                            layerSubmit2.radiusRight = compositeLayer.radius;
                            layerSubmit2.centralHorizontalAngleLeft = compositeLayer.dstRectLeft.width * 2 * Mathf.PI;
                            layerSubmit2.centralHorizontalAngleRight = compositeLayer.dstRectRight.width * 2 * Mathf.PI;
                            layerSubmit2.upperVerticalAngleLeft = (compositeLayer.dstRectLeft.height + compositeLayer.dstRectLeft.y - 0.5f) * Mathf.PI;
                            layerSubmit2.upperVerticalAngleRight = (compositeLayer.dstRectRight.height + compositeLayer.dstRectRight.y - 0.5f) * Mathf.PI;
                            layerSubmit2.lowerVerticalAngleLeft = (compositeLayer.dstRectLeft.y - 0.5f) * Mathf.PI;
                            layerSubmit2.lowerVerticalAngleRight = (compositeLayer.dstRectRight.y - 0.5f) * Mathf.PI;

                            PXR_Plugin.Render.UPxr_SubmitLayerEquirect2(layerSubmit2);
                        }
                    }
                    else if (compositeLayer.overlayShape == PXR_OverLay.OverlayShape.Cubemap)
                    {
                        PxrLayerCube2 layerSubmit2 = new PxrLayerCube2();
                        layerSubmit2.header.layerId = compositeLayer.overlayIndex;
                        layerSubmit2.header.colorScaleX = colorScale.x;
                        layerSubmit2.header.colorScaleY = colorScale.y;
                        layerSubmit2.header.colorScaleZ = colorScale.z;
                        layerSubmit2.header.colorScaleW = colorScale.w;
                        layerSubmit2.header.colorBiasX = colorBias.x;
                        layerSubmit2.header.colorBiasY = colorBias.y;
                        layerSubmit2.header.colorBiasZ = colorBias.z;
                        layerSubmit2.header.colorBiasW = colorBias.w;
                        layerSubmit2.header.compositionDepth = compositeLayer.layerDepth;
                        layerSubmit2.header.headPose.orientation.x = compositeLayer.cameraRotations[0].x;
                        layerSubmit2.header.headPose.orientation.y = compositeLayer.cameraRotations[0].y;
                        layerSubmit2.header.headPose.orientation.z = -compositeLayer.cameraRotations[0].z;
                        layerSubmit2.header.headPose.orientation.w = -compositeLayer.cameraRotations[0].w;
                        layerSubmit2.header.headPose.position.x = (compositeLayer.cameraTranslations[0].x + compositeLayer.cameraTranslations[1].x) / 2;
                        layerSubmit2.header.headPose.position.y = (compositeLayer.cameraTranslations[0].y + compositeLayer.cameraTranslations[1].y) / 2;
                        layerSubmit2.header.headPose.position.z = -(compositeLayer.cameraTranslations[0].z + compositeLayer.cameraTranslations[1].z) / 2;
                        layerSubmit2.header.layerShape = PXR_OverLay.OverlayShape.Cubemap;
                        layerSubmit2.header.useLayerBlend = (UInt32)(compositeLayer.useLayerBlend ? 1 : 0);
                        layerSubmit2.header.layerBlend.srcColor = compositeLayer.srcColor;
                        layerSubmit2.header.layerBlend.dstColor = compositeLayer.dstColor;
                        layerSubmit2.header.layerBlend.srcAlpha = compositeLayer.srcAlpha;
                        layerSubmit2.header.layerBlend.dstAlpha = compositeLayer.dstAlpha;

                        if (isHeadLocked)
                        {
                            layerSubmit2.poseLeft.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.overlayTransform.localPosition.x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.overlayTransform.localPosition.y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.overlayTransform.localPosition.z;

                            layerSubmit2.poseRight.orientation.x = compositeLayer.overlayTransform.localRotation.x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.overlayTransform.localRotation.y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.overlayTransform.localRotation.z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.overlayTransform.localRotation.w;
                            layerSubmit2.poseRight.position.x = compositeLayer.overlayTransform.localPosition.x;
                            layerSubmit2.poseRight.position.y = compositeLayer.overlayTransform.localPosition.y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.overlayTransform.localPosition.z;

                            layerSubmit2.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace |
                                PxrLayerSubmitFlags.PxrLayerFlagHeadLocked);
                        }
                        else
                        {
                            layerSubmit2.poseLeft.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseLeft.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseLeft.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseLeft.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseLeft.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseLeft.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseLeft.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.poseRight.orientation.x = compositeLayer.modelRotations[0].x;
                            layerSubmit2.poseRight.orientation.y = compositeLayer.modelRotations[0].y;
                            layerSubmit2.poseRight.orientation.z = -compositeLayer.modelRotations[0].z;
                            layerSubmit2.poseRight.orientation.w = -compositeLayer.modelRotations[0].w;
                            layerSubmit2.poseRight.position.x = compositeLayer.modelTranslations[0].x;
                            layerSubmit2.poseRight.position.y = compositeLayer.modelTranslations[0].y;
                            layerSubmit2.poseRight.position.z = -compositeLayer.modelTranslations[0].z;

                            layerSubmit2.header.layerFlags = (UInt32)(
                                PxrLayerSubmitFlags.PxrLayerFlagUseExternalHeadPose |
                                PxrLayerSubmitFlags.PxrLayerFlagLayerPoseNotInTrackingSpace);
                        }

                        PXR_Plugin.Render.UPxr_SubmitLayerCube2(layerSubmit2);
                    }
                }
            }
        }
    }
}

