# Overview of API functions

> Instuctions relevant for Pico SDK version 2.7.8

Sections:
* Version Info
* Sensor Tracking
* Controller
* Battery, Volume, Brightness Services
* HMD Sensors
* Hardware Devices

## (1) Version Info

#### UPvr_GetUnitySDKVersion
| Name | public static string UPvr_GetUnitySDKVersion () |
| :---: | :----: |
| Functionality | Acquire SDK version number |
| Parameters | None |
| Returns | SDK Version Number |
| Call Method | Pvr_UnitySDKAPI.System.UPvr_GetUnitySDKVersion() |


## (2) Sensor Tracking

#### UPvr_StartSensor
| Name | public static int UPvr_StartSensor(int index) |
| :---: | :----: |
| Functionality | Enable sensor tracking |
| Parameters | Index: 0, main sensor; 1, subsidiary sensor. |
| Returns | 0: Call succeeded; 1: Call failed |
| Call Method | Pvr_UnitySDKAPI.Sensor.UPvr_StartSensor(index) |
>Main sensor is the HMD sensor of Pico Viewer, Pico Neo and Goblin,Goblin2 namely the head tracking sensor.

#### UPvr_ResetSensor
| Name | public static int UPvr_ResetSensor(int index) |
| :---: | :----: |
| Functionality | Reset sensor tracking |
| Parameters | Index: 0, main sensor; 1, subsidiary sensor |
| Returns | 0: Call succeeded; 1: Call failed |
| Call Method | Pvr_UnitySDKAPI.Sensor.UPvr_ResetSensor(index) |

#### UPvr_UPvr_ResetSensorAll
| Name | public static int UPvr_ResetSensorAll (int index) |
| :---: | :----: |
| Functionality | Reset the sensor tracking horizontal vertical direction. |
| Parameters | Index: 0, main sensor; 1, subsidiary sensor |
| Returns | 0: Call succeeded; 1: Call failed |
| Call Method | Pvr_UnitySDKAPI. Sensor. UPvr_ResetSensorAll (index) |
>This function is intended for use with the Pico Viewer device, and should not typically be used or required.

#### Upvr_OptionalResetSensor
| Name | public static int Upvr_OptionalResetSensor(int index, int resetRot, int resetPos) |
| :---: | :----: |
| Functionality | Reset the sensor rotation and position |
| Parameters | Index: 0, main sensor; 1, subsidiary sensor resetRot:rotation resetPos:position 0:do not reset 1:reset |
| Returns | 0:Call succeeded 1:Call failed |
| Call Method | Pvr_UnitySDKAPI. Sensor. UPvr_OptionalResetSensor (0,1,1) |

## (3) Controller

#### StartScan
| Name | public static void StartScan () |
| :---: | :----: |
| Functionality | Search 3dof controller |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_ControllerManager.StartScan() |

#### StopScan
| Name | public static void StopScan () |
| :---: | :----: |
| Functionality | Stop searching 3dof controller |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_ControllerManager.StopScan() |

#### ConnectBLE
| Name | public static void ConnectBLE () |
| :---: | :----: |
| Functionality | Start connecting to 3dof controllers |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_ControllerManager.ConnectBLE () |

#### UPvr_GetControllerPower
| Name | public static int UPvr_GetControllerPower(int hand) |
| :---: | :----: |
| Functionality | Get controller’s battery level |
| Parameters | For Goblin and Goblin2, pass 0; for Pico Neo, pass 0/1 for corresponding controllers. |
| Returns | Goblin and Goblin2 0-4，Pico Neo 1-9 |
| Call Method | Pvr_ControllerAPI.Controller.UPvr_GetControllerPower(int hand) |
> Parameters: (Pico Neo，0/1 are controller indexes, 0 represents the first bound controller and 1 represents the second bound controller, the same below)

#### changeMainControllerCallback
| Name | public static void changeMainControllerCallback(string index) |
| :---: | :----: |
| Functionality | Callback of changing main controller |
| Parameters | 0/1 indicates which one is switched to be the main control controller |
| Returns | None |
| Call Method | Call back when the main controller changes. |
> (The main control controller is a controller that enables beam and participates in the UI interaction and has no specific relationship with the index of the controller, the same below

#### setControllerServiceBindState
| Name | public static void setControllerServiceBindState (string state) |
| :---: | :----: |
| Functionality | Callback of Pico Neo controller binding |
| Parameters | 0 indicates binding failed, 1 indicates binding succeeded |
| Returns | None |
| Call Method | Callback of Pico Neo controller binding |

#### setControllerStateChanged
| Name | public static void setControllerStateChanged (string state) |
| :---: | :----: |
| Functionality | Callback of connection status changes of Pico Neo controllers |
| Parameters | A two character string |
| Returns | None |
| Call Method | Callback of the status changes of Pico Neo controllers |
> Parameters: A two-character string. The first number is the index number of the controller and the second if the connection status of the controller. For example: “0,0” means that controller 0 is disconnected, and “0, 1” means controller 0 is connected.

#### setHbServiceBindState
| Name | public static void setHbServiceBindState (string state) |
| :---: | :----: |
| Functionality | Call back when service bind of 3dof controller changes state |
| Parameters | 0-unbound, 1-bound, 2-unknown |
| Returns | None |
| Call Method | Call back when service bind of 3dof controller changes status |

#### isServiceExisted
| Name | public static bool isServiceExisted () |
| :---: | :----: |
| Functionality | Determine if Pico Controller Service exists |
| Parameters | true, exists; false, does not exist |
| Returns | None |
| Call Method | Use it directly in Pvr_ControllerManager class |

#### UPvr_GetControllerState
| Name | public static ControllerState UPvr_GetControllerState (int hand) |
| :---: | :----: |
| Functionality | Get Pico controller’s connection status |
| Parameters | For Goblin,Goblin2 pass 0; for Pico Neo, pass 0/1 for the corresponding controller |
| Returns | Connection status |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetControllerState(hand) |
> When there is only one controller connected to Pico Neo device, it is recommended to call this function twice, since the index of the connected controller is unsure.

#### ResetController
| Name | public static void ResetController(int num) |
| :---: | :----: |
| Functionality | Reset controller pose |
| Parameters | 0/1. If use only 1 controller or a 3dof controller, pass 0 |
| Returns | None |
| Call Method | Pvr_ControllerManager.ResetController(num) |

#### RebackToLauncher
| Name | public static void RebackToLauncher () |
| :---: | :----: |
| Functionality | Exit and go back to Launcher UI |
| Parameters | None |
| Returns | None |
| Call Method | Use it directly in Pvr_ControllerManager class |

#### StartUpgrade
| Name | public static bool StartUpgrade () |
| :---: | :----: |
| Functionality | Start to upgrade |
| Parameters | None |
| Returns | Success or failure |
| Call Method | Pvr_ControllerManager.StartUpgrade() |

#### GetBLEVersion
| Name | public static long GetBLEVersion () |
| :---: | :----: |
| Functionality | Get the BLE version |
| Parameters | None |
| Returns | The version of BLE |
| Call Method | Pvr_ControllerManager.GetBLEVersion() |

#### setupdateFailed
| Name | public static void setupdateFailed () |
| :---: | :----: |
| Functionality | Call back when upgrade failed |
| Parameters | None |
| Returns | None |
| Call Method | Use it directly in Pvr_ControllerManager class |

#### setupdateSuccess
| Name | public static void setupdateSuccess () |
| :---: | :----: |
| Functionality | Call back when upgrade succeeded |
| Parameters | None |
| Returns | None |
| Call Method | Use it directly in Pvr_ControllerManager class |

#### setupdateProgress
| Name | public static void setupdateProgress (string progress) |
| :---: | :----: |
| Functionality | Call back of upgrade progress |
| Parameters | Progress 0-100 |
| Returns | None |
| Call Method | Use it directly in Pvr_ControllerManager class |

#### UPvr_GetControllerQUA
| Name | public static Quaternion UPvr_GetControllerQUA (int hand) |
| :---: | :----: |
| Functionality | Get controller poses |
| Parameters | 0/1 – for Goblin and Goblin2, pass 0; for Pico Neo, pass the index of the corresponding controllers |
| Returns | Quaternion, controller pose |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetControllerQUA(hand) |

#### UPvr_GetControllerPOS
| Name | public static Vector3 UPvr_GetControllerPOS (int hand) |
| :---: | :----: |
| Functionality | Get controller position |
| Parameters | Applicable for Pico Neo only, please pass the index of the corresponding controller |
| Returns | 3-D vector, controller position |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetControllerPOS(hand) |

#### UPvr_GetMainHandNess
| Name | public static int UPvr_GetMainHandNess() |
| :---: | :----: |
| Functionality | Get the index of current main control controller |
| Parameters | None |
| Returns | 0/1 |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetMainHandNess() |
> (Note: Pico Neo ：Pico Neo exclusive. It can only return the correct value when bind service has succeeded. The SDK will start Bind controller service when the app starts. We recommend developers to check if bind has succeeded using the Bind callback function)

#### UPvr_SetMainHandNess
| Name | public static void UPvr_SetMainHandNess(int hand) |
| :---: | :----: |
| Functionality | Set current main control controller |
| Parameters | 0/1 |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_SetMainHandNess() |

#### UPvr_GetKey
| Name | public static bool UPvr_GetKey (int hand，Pvr_KeyCode key) |
| :---: | :----: |
| Functionality | Check if the key is pressed down |
| Parameters | 0/1，Pvr_KeyCode |
| Returns | Whether it’s pressed or not |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetKey(hand , key) |

#### UPvr_GetKeyDown
| Name | public static bool UPvr_GetKeyDown (int hand，Pvr_KeyCode key) |
| :---: | :----: |
| Functionality | Check if the key is pressed down |
| Parameters | Hand: controller index, key: the specified key |
| Returns | Whether it is pressed down or not |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(hand,key) |

#### UPvr_GetKeyUp
| Name | public static bool UPvr_GetKeyUp(int hand，Pvr_KeyCode key) |
| :---: | :----: |
| Functionality | Check if the key is lifted |
| Parameters | Hand: controller index, key: the specified key |
| Returns | Whether it is lifted or not |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetKeyUp(hand ,key) |

#### UPvr_GetKeyLongPressed
| Name | public static bool UPvr_GetKeyLongPressed (int hand，Pvr_KeyCode key) |
| :---: | :----: |
| Functionality | Check if the key is long pressed |
| Parameters | Hand: controller index, key: the specified key |
| Returns | Whether it is long pressed or not |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetKeyLongPressed(hand,key) |

#### UPvr_IsTouching
| Name | public static bool UPvr_IsTouching (int hand) |
| :---: | :----: |
| Functionality | Check if the touchpad is being touched |
| Parameters | Hand: controller index |
| Returns | Whether it is touched or not |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_IsTouching() |

#### UPvr_GetSwipeDirection
| Name | public static SwipeDirection UPvr_GetSwipeDirection(int hand) |
| :---: | :----: |
| Functionality | Get the status of the swipe pose |
| Parameters | Hand: controller index |
| Returns | SwipeDirection |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetSwipeDirection(hand) |

#### UPvr_GetTouchPadPosition
| Name | public static Vector2 UPvr_GetTouchPadPosition (int hand) |
| :---: | :----: |
| Functionality | Get the touch value of the touchpad |
| Parameters | Controller index, 0/1 |
| Returns | Touchpad’s touch value. Same for Pico Goblin and Pico Neo, see figure5.4 for details |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr_GetTouchPadPosition(hand) |

#### UPvr_GetTouchPadClick
| Name | public static TouchPadClick UPvr_GetTouchPadClick(int hand) |
| :---: | :----: |
| Functionality | Touchpad simulation click function |
| Parameters | Controller index, 0/1 |
| Returns | Imitate the function of the top and bottom left buttons |
| Call Method | Pvr_UnitySDKAPI.Controller.UPvr-GetTouchPadClick (hand) |
> Divide the touchpad into 4 pieces to simulate the up and down and left functions of the game controller

#### AutoConnectHbController
| Name | public static void AutoConnectHbController (int scans) |
| :---: | :----: |
| Functionality | Automatically search and connect to controllers |
| Parameters | Search time (in ms) |
| Returns | None |
| Call Method | Pvr_ControllerManager.AutoConnectHbController(scans) |

#### setHbAutoConnectState
| Name | public static void setHbAutoConnectState (string state) |
| :---: | :----: |
| Functionality | Callback of auto- searching controllers |
| Parameters | None |
| Returns | Connection state (See next table) |
| Call Method | Pvr_ControllerManager.setHbAutoConnectState(state) |

#### Connection states
| UNKNOWN | default |
| :---: | :----: |
| NO_DEVICE | No HBcontroller found |
| ONLY_ONE | Only 1 HB controller found |
| MORE_THAN_ONE | More than 1 controllers found |
| LAST_CONNECTED | The last connected HB controller found |
| FACTORY_DEFAULT | The factory default HB controller found (not enabled yet) |

#### Upvr_GetControllerTriggerValue
| Name | public static int Upvr_GetControllerTriggerValue(int hand) |
| :---: | :----: |
| Functionality | Get the input value of the trigger(available to Goblin2 and Neo Controller ) |
| Parameters | 0/1 |
| Returns | 0~255 |
| Call Method | Pvr_UnitySDKAPI.Controller.Upvr_ControllerTriggerClick（hand) |

#### Upvr_ControllerTriggerClick
| Name | public static bool Upvr_ControllerTriggerClick(int hand) |
| :---: | :----: |
| Functionality | Check if the trigger clicked or not |
| Parameters | 0/1 |
| Returns | true, clicked; false, not clicked |
| Call Method | Pvr_UnitySDKAPI.Controller.Upvr_ControllerTriggerClick（hand） |
> Trigger click is available to 6dof controllers only and a trigger click is determined only when the trigger key is pressed to the bottom and raised once.

#### VibateController
| Name | public void VibateController(int hand, int strength) |
| :---: | :----: |
| Functionality | The vibration interface of Pico Neo Controller |
| Parameters | hand: 0,1 strength:0-255 |
| Returns | None |
| Call Method | Pvr_ControllerManager.controllerlink.VibateController(hand,strength); |

#### UPvr_GetDeviceType
| Name | public static int UPvr_GetDeviceType () |
| :---: | :----: |
| Functionality | Gets the connected controller type |
| Parameters | none |
| Returns | 0:none 1:Goblin 2:Neo 3:Goblin2 |
| Call Method | Pvr_UnitySDKAPI.Controller. UPvr_GetDeviceType (); |
> If the return value is 0, the controller connection state needs to be called at the same time, and the two values cooperate to decide whether to enable head control)

## (4) Battery, Volume, Brightness Services

#### UPvr_InitBatteryVolClass
| Name | public bool UPvr_InitBatteryVolClass() |
| :---: | :----: |
| Functionality | Initialize battery, volume and brightness services |
| Parameters | None |
| Returns | Initialization succeeded or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_InitBatteryVolClass() |
> Please call this interface for initialization before using battery, volume and brightness services

#### UPvr_StartAudioReceiver
| Name | public bool UPvr_StartAudioReceiver () |
| :---: | :----: |
| Functionality | Start audio service |
| Parameters | None |
| Returns | Whether it’s started successfully or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StartAudioReceiver() |

#### UPvr_SetAudio
| Name | public void UPvr_SetAudio(string s) |
| :---: | :----: |
| Functionality | Notify Unity of the value of current volume when the volume changes |
| Parameters | The value of the volume after change |
| Returns | None |
| Call Method | This interface is a low-level Android call and unity doesn’t need to call it. If you need to perform any operations when volume changes, write the related logics in this method. |

#### UPvr_StopAudioReceiver
| Name | public bool UPvr_StopAudioReceiver () |
| :---: | :----: |
| Functionality | Stop volume service |
| Parameters | None |
| Returns | Whether it’s stopped or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StopAudioReceiver() |

#### UPvr_StartBatteryReceiver
| Name | public bool UPvr_StartBatteryReceiver () |
| :---: | :----: |
| Functionality | Start battery service |
| Parameters | None |
| Returns | Whether it’s started or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StartBatteryReceiver() |

#### UPvr_SetBattery
| Name | public void UPvr_SetBattery(string s) |
| :---: | :----: |
| Functionality | Notify Unity of the current battery level when battery level changes |
| Parameters | The battery level after change (in 0.00~1.00) |
| Returns | None |
| Call Method | This interface is a low-level Android call and unity doesn’t need to call it. If you need to perform any operations when battery level changes, write the related logics in this method. |

#### UPvr_StopBatteryReceiver
| Name | public bool UPvr_StopBatteryReceiver () |
| :---: | :----: |
| Functionality | Stop battery service |
| Parameters | None |
| Returns | Whether it’s stopped or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StopBatteryReceiver() |

#### UPvr_GetMaxVolumeNumber
| Name | public int UPvr_GetMaxVolumeNumber () |
| :---: | :----: |
| Functionality | Get the max value of volume |
| Parameters | None |
| Returns | The max value of volume |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetMaxVolumeNumber() |

#### UPvr_GetCurrentVolumeNumber
| Name | public int UPvr_GetCurrentVolumeNumber () |
| :---: | :----: |
| Functionality | Get current volume value |
| Parameters | None |
| Returns | Current volume value (in 0~15) |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetCurrentVolumeNumber() |

#### UPvr_VolumeUp
| Name | public bool UPvr_VolumeUp () |
| :---: | :----: |
| Functionality | Tune up the volume |
| Parameters | None |
| Returns | Whether it succeeded or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_VolumeUp() |

#### UPvr_VolumeDown
| Name | public bool UPvr_VolumeDown () |
| :---: | :----: |
| Functionality | Tune down the volume |
| Parameters | None |
| Returns | Whether it succeeded or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_VolumeDown() |

#### UPvr_SetVolumeNum
| Name | public bool UPvr_SetVolumeNum(int volume) |
| :---: | :----: |
| Functionality | Set volume |
| Parameters | Set the value for the volume (the value of volume is between 0 and 15) |
| Returns | Whether it succeeded or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetVolumeNum(volume) |

#### UPvr_SetBrightness
| Name | public bool UPvr_SetBrightness(int brightness) |
| :---: | :----: |
| Functionality | Set brightness (available to regular mobile phone versions, not applicable for Pico Neo) |
| Parameters | The value of the brightness (the value is between 0 and 255) |
| Returns | Whether it succeeded or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetBrightness(bn) |

#### UPvr_GetScreenBrightnessLevel
| Name | public static int[] UPvr_GetScreenBrightnessLevel() |
| :---: | :----: |
| Functionality | Get current screen brightness level (applicable for Goblin only) |
| Parameters | None |
| Returns | Int array |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetScreenBrightnessLevel() |
> Int array: The first digit is the total brightness level supported, the second is the current brightness level and from the third to the end is the brightness interval values.

#### UPvr_SetScreenBrightnessLevel
| Name | public static void UPvr_SetScreenBrightnessLevel(int vrBrightness, int level) |
| :---: | :----: |
| Functionality | Set screen brightness (applicable for Goblin only) |
| Parameters | vrBrightness：brightness mode; level: brightness value (value of the brightness level) |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetScreenBrightnessLevel(vrBrightness,level) |
> If vrBrightness passes 1, level should pass the brightness level; if vrBrightness passes 0, representing the system default brightness mode, then level can be set to any value in 1~255.

#### UPvr_IsHmdExist
| Name | public bool UPvr_IsHmdExist() |
| :---: | :----: |
| Functionality | Check if it’s an HMD headset |
| Parameters | None |
| Returns | If true, it is HMD; false, not HMD |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_IsHmdExist() |

#### UPvr_GetHmdScreenBrightness
| Name | public int UPvr_GetHmdScreenBrightness() |
| :---: | :----: |
| Functionality | Get current brightness value of current HMD device (applicable for Pico Neo only) |
| Parameters | None |
| Returns | Int, current brightness value (in 0~255) |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetHmdScreenBrightness() |

#### UPvr_SetHmdScreenBrightness
| Name | public bool UPvr_SetHmdScreenBrightness(int brightness) |
| :---: | :----: |
| Functionality | Set the brightness value of HMD device (applicable for PicoNeo only) |
| Parameters | Brightness value (in 0~255) |
| Returns | Whether it’s successfully set or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetHmdScreenBrightness(brightness) |

#### UPvr_GetCommonBrightness
| Name | public int UPvr_GetCommonBrightness() |
| :---: | :----: |
| Functionality | Get the current brightness value of common devices (applicable for Pico Neo and other regular mobile phones) |
| Parameters | None |
| Returns | Current brightness value (in 0~255) |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetCommonBrightness() |

#### UPvr_SetCommonBrightness
| Name | public bool UPvr_SetCommonBrightness(int brightness) |
| :---: | :----: |
| Functionality | Set brightness for common devices (applicable for Pico Neo and other regular mobile phones) |
| Parameters | Current brightness value (in 0~255) |
| Returns | Whether it’s successfully set or not |
| Call Method | Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetCommonBrightness(brightness) |

## (5) HMD Sensors

#### UPvr_InitPsensor
| Name | public static void UPvr_InitPsensor() |
| :---: | :----: |
| Functionality | Initialize distance sensor |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI. Sensor. UPvr_InitPsensor () |

#### UPvr_GetPsensorState
| Name | public static int UPvr_GetPsensorState() |
| :---: | :----: |
| Functionality | Get the state of head-mounted distance sensor |
| Parameters | None |
| Returns | Return 0 when it’s worn on head, return 1 when it’s away |
| Call Method | Pvr_UnitySDKAPI. Sensor. UPvr_GetPsensorState() |
> Note: UPvr_InitPsensor should be called for initialization before getting Psensor’s state

#### UPvr_UnregisterPsensor
| Name | public static void UPvr_UnregisterPsensor () |
| :---: | :----: |
| Functionality | Release distance sensor |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI. Sensor. UPvr_UnregisterPsensor () |

## (6) Hardware Devices

#### UPvr_GetDeviceMode
| Name | public static string UPvr_GetDeviceMode () |
| :---: | :----: |
| Functionality | Get device type |
| Parameters | None |
| Returns | SystemInfo.deviceModel |
| Call Method | Pvr_UnitySDKAPI.System.UPvr_GetDeviceMode () |

#### UPvr_GetDeviceSN
| Name | public static string UPvr_GetDeviceSN () |
| :---: | :----: |
| Functionality | Get device SN number |
| Parameters | None |
| Returns | Get device SN number |
| Call Method | Pvr_UnitySDKAPI.System.UPvr_GetDeviceSN () |

#### UPvr_ShutDown
| Name | public static void UPvr_ShutDown() |
| :---: | :----: |
| Functionality | Shut down |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI.System.UPvr_ShutDown () |

#### UPvr_Reboot
| Name | public static void UPvr_Reboot () |
| :---: | :----: |
| Functionality | Reboot |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI.System.UPvr_Reboot () |

#### UPvr_Sleep
| Name | public static void UPvr_Sleep () |
| :---: | :----: |
| Functionality | Turn off screen |
| Parameters | None |
| Returns | None |
| Call Method | Pvr_UnitySDKAPI. System. UPvr_Sleep () |
