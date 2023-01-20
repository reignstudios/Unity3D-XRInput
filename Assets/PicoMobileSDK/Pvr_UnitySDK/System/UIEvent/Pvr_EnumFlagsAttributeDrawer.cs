// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(Pvr_EnumFlags))]
public class Pvr_EnumFlagsAttributeDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        property.intValue = UnityEditor.EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
    }
}
#endif