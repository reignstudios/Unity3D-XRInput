// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public sealed class Pvr_UnitySDKPlatformSetting : ScriptableObject
{
    public enum simulationType
    {
        Null,
        Invalid,
        Valid,
    }

    [SerializeField]
    private bool entitlementchecksimulation;

    public static bool Entitlementchecksimulation
    {
        get { return Instance.entitlementchecksimulation; }
        set
        {
            if (Instance.entitlementchecksimulation != value)
                Instance.entitlementchecksimulation = value;
        }
    }

    public List<string> deviceSN = new List<string>();


    private static Pvr_UnitySDKPlatformSetting instance;
    public static Pvr_UnitySDKPlatformSetting Instance
    {
        get
        {
            if (instance == null)
            {

                instance = Resources.Load<Pvr_UnitySDKPlatformSetting>("PlatformSettings");
#if UNITY_EDITOR
                if (instance == null)
                {
                    instance = CreateInstance<Pvr_UnitySDKPlatformSetting>();
                    UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/PicoMobileSDK/Pvr_UnitySDK/Resources/PlatformSettings.asset");
                }   
#endif
            }
            return instance;
        }

        set { instance = value; }
    }

}
