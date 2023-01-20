// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Pvr_UnitySDKEyeOverlay))]
public class Pvr_UnitySDKEyeOverlayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        foreach (Pvr_UnitySDKEyeOverlay overlayTarget in targets)
        {
            EditorGUILayout.LabelField("Overlay Display Order", EditorStyles.boldLabel);
            overlayTarget.overlayType = (Pvr_UnitySDKEyeOverlay.OverlayType)EditorGUILayout.EnumPopup("Overlay Type", overlayTarget.overlayType);
            overlayTarget.layerIndex = EditorGUILayout.IntField("Layer Index", overlayTarget.layerIndex);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Overlay Shape", EditorStyles.boldLabel);
            overlayTarget.overlayShape = (Pvr_UnitySDKEyeOverlay.OverlayShape)EditorGUILayout.EnumPopup("Overlay Shape", overlayTarget.overlayShape);
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Overlay Textures", EditorStyles.boldLabel);
            overlayTarget.isExternalAndroidSurface = EditorGUILayout.Toggle("External Surface", overlayTarget.isExternalAndroidSurface);
            var labelControlRect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(labelControlRect.x, labelControlRect.y, labelControlRect.width / 2, labelControlRect.height), new GUIContent("Left Texture", "Texture used for the left eye"));
            EditorGUI.LabelField(new Rect(labelControlRect.x + labelControlRect.width / 2, labelControlRect.y, labelControlRect.width / 2, labelControlRect.height), new GUIContent("Right Texture", "Texture used for the right eye"));

            var textureControlRect = EditorGUILayout.GetControlRect(GUILayout.Height(64));
            overlayTarget.layerTextures[0] = (Texture2D)EditorGUI.ObjectField(new Rect(textureControlRect.x, textureControlRect.y, 64, textureControlRect.height), overlayTarget.layerTextures[0], typeof(Texture2D), false);
            overlayTarget.layerTextures[1] = (Texture2D)EditorGUI.ObjectField(new Rect(textureControlRect.x + textureControlRect.width / 2, textureControlRect.y, 64, textureControlRect.height), overlayTarget.layerTextures[1] != null ? overlayTarget.layerTextures[1] : overlayTarget.layerTextures[0], typeof(Texture2D), false);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Color Scale And Offset", EditorStyles.boldLabel);
            overlayTarget.overrideColorScaleAndOffset = EditorGUILayout.Toggle(new GUIContent("Override Color Scale", "Set color scale and offset of this layer."), overlayTarget.overrideColorScaleAndOffset);
            if (overlayTarget.overrideColorScaleAndOffset)
            {
                Vector4 colorScale = EditorGUILayout.Vector4Field(new GUIContent("Color Scale", "Scale that the color values"), overlayTarget.colorScale);
                Vector4 colorOffset = EditorGUILayout.Vector4Field(new GUIContent("Color Offset", "Offset that the color values"), overlayTarget.colorOffset);
                overlayTarget.SetLayerColorScaleAndOffset(colorScale, colorOffset);
            }
        }

        //DrawDefaultInspector();
        if (GUI.changed)
        {
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
        }
    }
}
