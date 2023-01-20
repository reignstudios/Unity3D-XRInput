// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;
using UnityEngine;


[System.Serializable]
public class Pvr_UnitySDKProjectSetting : ScriptableObject
{

    public RenderTextureAntiAliasing rtAntiAlising;
    public RenderTextureDepth rtBitDepth;
    public RenderTextureFormat rtFormat;

    public bool usedefaultRenderTexture;
    public Vector2 customRTSize;

    public bool usedefaultfps;
    public int customfps;
    public bool usesinglepass;
    public bool usecontentprotect;

    public static Pvr_UnitySDKProjectSetting GetProjectConfig()
    {
        Pvr_UnitySDKProjectSetting projectConfig = Resources.Load<Pvr_UnitySDKProjectSetting>("ProjectSetting");
#if UNITY_EDITOR
        if (projectConfig == null)
        {
            projectConfig = CreateInstance<Pvr_UnitySDKProjectSetting>();
            projectConfig.rtAntiAlising = RenderTextureAntiAliasing.X_2;
            projectConfig.rtBitDepth = RenderTextureDepth.BD_24;
            projectConfig.rtFormat = RenderTextureFormat.Default;
            projectConfig.usedefaultRenderTexture = true;
            projectConfig.usedefaultfps = true;
            projectConfig.usesinglepass = false;
            projectConfig.usecontentprotect = false;
            projectConfig.customRTSize = new Vector2(2048, 2048);
            projectConfig.customfps = 61;
            UnityEditor.AssetDatabase.CreateAsset(projectConfig, "Assets/PicoMobileSDK/Pvr_UnitySDK/Resources/ProjectSetting.asset");
        }
#endif
        return projectConfig;
    }

}
