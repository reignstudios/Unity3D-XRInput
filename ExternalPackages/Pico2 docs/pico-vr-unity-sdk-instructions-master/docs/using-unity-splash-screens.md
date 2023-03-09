# Working with the Unity splash screen

## Professional Edition

As of v2.7.8 of the Pico SDK it is now possible to customize the starting splash screen, using some predefined options. 

Using the menu item in **Pvr_UnitySDK -> Splash Screen** you can open the Pico splash screen editing interface.

<p align="center">
  <img alt="Splash Screen Menu" width="500px" src="/docs/assets/SplashScreenPVR.png">
</p>

This interface has a drop-down box allowing the user to pick between showing either a Pico, Unity, or Dynamic splash screen. The 'Ok' button will apply any changes the user makes.

Alternatively, if splash screens are disabled under **PlayerSettings -> Splash Image -> Show Splash Screen** developers can implement their own splash screen scripts, or go without a splash screen entirely.

## Personal Edition

### The problem with personal-edition Unity splash screens

Unity provides a system to display a splash screen as the app is launched and loaded for the first time. On Pico devices it is recommended that users of the professional version of Unity disable this functionality, and create their own implementation of a splash screen if necessary. However, this method does not help users of the personal version of Unity or those who are unable to create their own splash screen implementation for their own reasons.

As stated in the [installation guide](/docs/pico-vr-unity-sdk-installation.md) the Pico SDK requires it's apps to run with XR/VR settings disabled, as it provides it's own VR implementation. This has the unfortunate effect of telling Unity to use the screenspace splashscreen, which causes the splash image to be stretched out across both the users eye displays as the app is launched. This effect is unpleasant and confusing on the eyes, and should be resolved using the method beneath.

### Using Unity's VR Splash screen with the Pico SDK.

> Code and technique courtesy of developer cmdr2.

To enable Unity's XR splash screen, while still using a non-xr setting during the apps runtime apply these steps to your project.

#### For Unity 2017+:
1.  In Player Settings, set "XR Supported" checkbox to true, and pick "Mock HMD" in the headset option.
2.  Set a VR Splash Screen image in Player Settings.
3.  In the Start() method of any class, call StartCoroutine(SetEmptyXRDevice()) as early as possible in the game.

        IEnumerator SetEmptyXRDevice() {
            XRSettings.LoadDeviceByName("");
            yield return new WaitForEndOfFrame();
            VRSettings.enabled = false;
        }
