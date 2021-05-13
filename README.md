# Simple XRInput for Unity3D & whats it for
This API acts as its own easy-to-use agnostic XR-Input layer allowing you to write the bulk of your code against it then easily target multiple platforms (namely Steam, Oculus, WMR, etc) without having to deal with the endless flux of complication or fragmentation between different companies or even Unity's native input API flux. More non-standard Input sources could be added to if needed (such as Pico) making it so you don't need to modify Input systems scattered around in your projects down the road.

# How to setup
* <b>NOTE: SteamVR SDK must NOT be installed for OpenVR_Legacy Input mode to work!</b>
* <b>NOTE: If SteamVR SDK is installed follow these steps for OpenVR Input mode to work!</b>
    * Create a new folder: "Assets/StreamingAssets/SteamVR/Original"
    * Move all .json files into "Original" folder
    * Copy all .json files from "Assets/StreamingAssets/OpenVR/" to "Assets/StreamingAssets/SteamVR/"
    * In the "SteamVR" folder, rename "vrstudios_actions.json" to "actions.json"
    * Make sure <b>"Steam SDK In Use"</b> is checked on in the XRInput script in your scene
* Ensure 'com.valvesoftware.unity.openvr' .tgz file is installed via Unity package manager for OpenVR input mode
* Drop 'Unity3D-XRInput/Assets/VRstudios/XRInput' prefab into scene & thats it.
* Select Input type and ready to go!

# How to get Input
* Take a look at 'Unity3D-XRInput/Assets/VRstudios/TestInput.cs' for basic working example
* You can get Input in multiple ways
    * Get full input state of controller: XRInput.ControllerState(...)
    * Get specific input state of controller: XRInput.ButtonTrigger(...), XRInput.Button1(...), etc
    * Get specific input state via callbacks: ButtonTriggerOnEvent, ButtonTriggerDownEvent, Button1DownEvent, etc