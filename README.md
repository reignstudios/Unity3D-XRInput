# Simple XRInput for Unity3D & whats it for
This API acts as its own easy-to-use agnostic XR-Input layer allowing you to write the bulk of your code against it then easily target multiple platforms (namely Steam, Oculus, WMR, etc) without having to deal with the endless flux of complication or fragmentation between different companies or even Unity's native input API flux. More non-standard Input sources could be added to if needed making it so you don't need to modify Input systems scattered around in your projects down the road.

# How to setup
* <b>NOTE: If you have "Initialize XR on Startup" off & manually create a loader instance, you may need to pass that instance to "XRInput.loaderOverride".</b>
* Drop 'Unity3D-XRInput/Assets/VRstudios/XRInput' prefab into scene & thats it.
* Select Input type and ready to go!

## 'SteamVR' setup
* Ensure 'com.valvesoftware.unity.openvr' .tgz file is installed via Unity package manager for OpenVR input mode
* NOTE: In 'Packages/manifest.json' you can set packages to be relitive paths like so: "file:../com.some-package.tgz"'
* <b>NOTE: SteamVR Plugin must NOT be installed for OpenVR_Legacy Input mode to work!</b>
* <b>NOTE: If SteamVR Plugin is installed follow these steps for OpenVR Input mode to work!</b>
    * Create a new folder: "Assets/StreamingAssets/SteamVR/Original"
    * Move all .json files into "Original" folder
    * Copy all .json files from "Assets/StreamingAssets/OpenVR/" to "Assets/StreamingAssets/SteamVR/"
    * In the "SteamVR" folder, rename "vrstudios_actions.json" to "actions.json"
    * Make sure <b>"Steam Plugin In Use"</b> is checked on in the XRInput script in your scene

## 'Windows Mixed Reality' setup
* Download and open 'MixedRealityFeatureTool'
* Open & follow navigation steps...
    * Click: Start
    * Select root Unity3D project path & click 'Discover Features'
    * Expand 'Platform Support' & check 'Mixed Reality OpenXR Plugin'
    * Click 'Get Features' & close when done
* In Unity's OpenXR settings: Add 'Microsoft Motion Controller Profile' & 'HP Reverb G2 Controller Profile'

## 'HTC VIVE Focus 3' setup
* Setup docs: https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRGettingStart.html
* Configure Unity to get access to Wave OpenXR plugin
    * Open Unity 'Project Settings' & navigate to 'Package Manager'
    * Add to 'Scoped Registeries'
        * Name: 'VIVE'
        * URL: 'https://npm-registry.vive.com'
        * Scope(s): 'com.htc.upm'
    * Open Unity's package manager
    * Clip drop-down & select 'My Registries'
    * Search for 'VIVE Wave XR Plugin' & install
* Rename "WAVE_AndroidManifest" to "AndroidManifest" if using this repo for testing

## 'PicoVR' setup
* Use 'Unity XR Platform SDK' plugin NOT legacy 'PicoVR Unity SDK' plugin
* 'Unity XR Platform SDK' doesn't ship correctly as a '.tar.tgz' package so follow these steps (NOTE: z7ip ideal here)
    * Extract 'PicoXR_Platform_SDK-x.x.x_XXX.zip' in a folder named 'package'
    * Compress 'package' folder into '.tar' file
    * Compress '.tar' into '.tgz' file. (NOTE: If file extension is '.gz' just change it to '.tgz')
    * In Unity3D go to package manager and install from tarball option
    * NOTE: In 'Packages/manifest.json' you can set packages to be relitive paths like so: "file:../com.some-package.tgz"'
    * You should now see 'PicoXR' in Unity's standard XR managment area

# How to get Input
* Take a look at 'Unity3D-XRInput/Assets/VRstudios/TestInput.cs' for basic working example
* You can get Input in multiple ways
    * Get full input state of controller: XRInput.ControllerState(...)
    * Get specific input state of controller: XRInput.ButtonTrigger(...), XRInput.Button1(...), etc
    * Get specific input state via callbacks: ButtonTriggerOnEvent, ButtonTriggerDownEvent, Button1DownEvent, etc

# Android: See Input in Example scene
* Run: adb logcat -s Unity ActivityManager PackageManager dalvikvm DEBUG