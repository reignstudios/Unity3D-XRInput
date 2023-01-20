// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class PLOG : MonoBehaviour
{
    public static int logLevel = 0;
    public static void getConfigTraceLevel()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.LOG_LEVEL;
        Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref logLevel);
#endif
    }

    public static void D(string msg)
    {
        if (logLevel > 2)
            Debug.Log(msg);
    }

    public static void I(string msg)
    {
        if (logLevel > 1)
            Debug.Log(msg);
    }

    public static void W(string msg)
    {
        if (logLevel > 0)
            Debug.LogWarning(msg);
    }

    public static void E(string msg)
    {
        Debug.LogError(msg);
    }
}
