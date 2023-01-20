// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using Pvr_UnitySDKAPI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pvr_UnitySDKEyeManager))]
public class Pvr_UnitySDKEyeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.changed = false;

        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        Pvr_UnitySDKEyeManager sdkEyeManager = (Pvr_UnitySDKEyeManager)target;

        sdkEyeManager.EyeTracking = EditorGUILayout.Toggle("Eye Tracking", sdkEyeManager.EyeTracking);
        if(sdkEyeManager.EyeTracking)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("EyeTracking is supported only on the Neo2 Eye");
            EditorGUILayout.EndVertical();
        }

        sdkEyeManager.FoveatedRendering = EditorGUILayout.Toggle("Foveated Rendering", sdkEyeManager.FoveatedRendering);
        if (sdkEyeManager.FoveatedRendering)
        {
            EditorGUI.indentLevel = 1;
            sdkEyeManager.FoveationLevel = (EFoveationLevel)EditorGUILayout.EnumPopup("Foveation Level", sdkEyeManager.FoveationLevel);
            EditorGUI.indentLevel = 0;
        }
        else
        {
            sdkEyeManager.FoveationLevel = EFoveationLevel.None;
        }

        EditorUtility.SetDirty(sdkEyeManager);
        if (GUI.changed)
        {
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager
                .GetActiveScene());
#endif
        }
    }

}
