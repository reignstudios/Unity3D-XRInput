# Simple XRInput for Unity3D
* Ensure 'com.valvesoftware.unity.openvr' .tgz file is installed via Unity package manager
* Run file-menu 'Assets/VRstudios/XRInput/Enable for Steam' for Steam / OpenVR input mode
* Run file-menu 'Assets/VRstudios/XRInput/Enable for Generic-Unity-Input' for Oculus PC/Android, Generic, etc mode
* Drop 'Unity3D-XRInput/Assets/VRstudios/XRInput' prefab into scene & thats it.

# How to get Input
* Take a look at 'Unity3D-XRInput/Assets/VRstudios/TestInput.cs' for basic working example
* You can get Input in multiple ways
    * Get full input state of controller: XRInput.ControllerState(...)
    * Get specific input state of controller: XRInput.ButtonTrigger(...), XRInput.Button1(...), etc
    * Get specific input state via callbacks: ButtonTriggerOnEvent, ButtonTriggerDownEvent, Button1DownEvent, etc