// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pvr_UnitySDKPlatformSetting))]
public class Pvr_UnitySDKPlatformSettingEditor : Editor {

    private SerializedProperty deviceSNList;

    private void OnEnable()
    {
        deviceSNList = serializedObject.FindProperty("deviceSN");
    }

    [MenuItem("Pvr_UnitySDK" + "/Platform Settings")]
    public static void Edit()
    {
        Selection.activeObject = Pvr_UnitySDKPlatformSetting.Instance;
    }

    public override void OnInspectorGUI()
    {
        var simulationTip = "If true,Development devices will simulate Entitlement Check," +
                            "you should enter a valid device SN codes list." + 
                            "The SN code can be obtain in Settings-General-Device serial number or input  \"adb devices\" in cmd";
        var simulationLabel = new GUIContent("Entitlement Check Simulation [?]", simulationTip);

        Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation = EditorGUILayout.Toggle(simulationLabel, Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation);
        if (Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(deviceSNList, true);
            serializedObject.ApplyModifiedProperties();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(Pvr_UnitySDKPlatformSetting.Instance);
            GUI.changed = false;
        }
    }
}
