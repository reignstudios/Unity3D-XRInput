// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class Resetbutton : MonoBehaviour
{
    public void DemoResetTracking()
    {
        if (Pvr_UnitySDKManager.SDK != null)
        {
            if (Pvr_UnitySDKManager.pvr_UnitySDKSensor != null)
            {
                Pvr_UnitySDKManager.pvr_UnitySDKSensor.ResetUnitySDKSensor();
            }
            else
            {
                Pvr_UnitySDKManager.SDK.pvr_UnitySDKEditor.ResetUnitySDKSensor();
            }
        }
    }
}
