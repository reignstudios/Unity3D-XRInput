// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif

using UnityEngine;


public class LoginSDK
{
    public static void Login()
    {
        PicoPaymentSDK.Login();
    }

    public static void GetUserAPI()
    {
        PicoPaymentSDK.GetUserAPI();
    }
}