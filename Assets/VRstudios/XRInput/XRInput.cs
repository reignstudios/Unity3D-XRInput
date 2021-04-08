using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#if VRSTUDIOS_XRINPUT_OPENVR
using Valve.VR;
using System.Runtime.InteropServices;
using System.Text;
#else
using Unity.XR.Oculus;
#endif

namespace VRstudios
{
    [DefaultExecutionOrder(-99)]
    public sealed class XRInput : MonoBehaviour
    {
        public static XRInput singleton { get; private set; }

        public List<InputDevice> devices { get; private set; } = new List<InputDevice>();
        public List<InputDevice> controllers { get; private set; } = new List<InputDevice>();
        public InputDevice handLeft { get; private set; }
        public InputDevice handRight { get; private set; }

        #if VRSTUDIOS_XRINPUT_OPENVR
        private const uint controllerStateLength = OpenVR.k_unMaxTrackedDeviceCount;
        private StringBuilder propertyText = new StringBuilder(256);
        private StringBuilder propertyText_ViveController = new StringBuilder("vive_controller", 256);// capacity must match 'propertyText' for equals to work
        private StringBuilder propertyText_IndexController = new StringBuilder("knuckles", 256);// capacity must match 'propertyText' for equals to work
        #else
        private const int controllerStateLength = 4;
        private string deviceName_MixedReality = "Spatial Controller";
        #endif
        private XRControllerState[] state_controllers = new XRControllerState[controllerStateLength];
        private uint state_controllerCount;
        private XRControllerState state_controllerLeft, state_controllerRight;
        private XRControllerState state_controllerFirst, state_controllerMerged;

        private void Start()
        {
            // only one can exist in scene at a time
            if (singleton != null)
            {
                Destroy(gameObject);
                return;
		    }
            DontDestroyOnLoad(gameObject);
            singleton = this;
            
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllers);
            devices.AddRange(controllers);

            InputDevices.deviceConnected += InputDevices_deviceConnected;
		    InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
		    InputDevices.deviceConfigChanged += InputDevices_deviceConfigChanged;

            #if VRSTUDIOS_XRINPUT_OPENVR
            EVRInitError e = EVRInitError.None;
            var system = OpenVR.Init(ref e);
            Debug.Log("OpenVR version: " + system.GetRuntimeVersion());
            #endif
        }

	    private void OnDestroy()
	    {
            #if VRSTUDIOS_XRINPUT_OPENVR
            OpenVR.Shutdown();
            #endif

            InputDevices.deviceConnected -= InputDevices_deviceConnected;
            InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
            InputDevices.deviceConfigChanged -= InputDevices_deviceConfigChanged;
        }

	    private void Update()
        {
            state_controllerCount = 0;
            bool leftSet = false, rightSet = false;

            #if VRSTUDIOS_XRINPUT_OPENVR
            var system = OpenVR.System;
            if (system == null || !system.IsInputAvailable()) return;

            for (uint i = 0; i != controllerStateLength; ++i)
            {
                if (!system.IsTrackedDeviceConnected(i)) continue;

                // update controller state
                if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller) continue;
                var state = new VRControllerState_t();
                if (system.GetControllerState(i, ref state, (uint)Marshal.SizeOf<VRControllerState_t>()))
                {
                    var controller = state_controllers[state_controllerCount];
                    controller.connected = true;

                    // get controller type
                    ETrackedPropertyError e = ETrackedPropertyError.TrackedProp_Success;
                    system.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_ControllerType_String, propertyText, (uint)propertyText.Capacity, ref e);

                    // update button & touch states
                    if (e == ETrackedPropertyError.TrackedProp_Success)
                    {
                        if (propertyText.Equals(propertyText_ViveController))// specialize input for odd Vive button layout
                        {
                            // buttons
                            controller.buttonTrigger.Update((state.ulButtonPressed & 8589934592) != 0);
                            controller.buttonGrip.Update((state.ulButtonPressed & 4) != 0);
                            controller.buttonMenu.Update((state.ulButtonPressed & 2) != 0);
                            controller.button1.Update((state.ulButtonPressed & 4294967296) != 0);

                            // touch
                            controller.touchTrigger.Update((state.ulButtonTouched & 8589934592) != 0);
                            controller.touch1.Update((state.ulButtonTouched & 4294967296) != 0);
                        }
                        else if (propertyText.Equals(propertyText_IndexController))// specialize input for odd Vive button layout
                        {
                            Debug.Log(state.ulButtonPressed.ToString());
                            // buttons
                            //controller.buttonTrigger.Update((state.ulButtonPressed & 8589934592) != 0);
                            //controller.buttonGrip.Update((state.ulButtonPressed & 4) != 0);
                            //controller.buttonMenu.Update((state.ulButtonPressed & 2) != 0);
                            //controller.button1.Update((state.ulButtonPressed & 4294967296) != 0);

                            // touch
                            //controller.touchTrigger.Update((state.ulButtonTouched & 8589934592) != 0);
                            //controller.touch1.Update((state.ulButtonTouched & 4294967296) != 0);
                        }
                    }
                    else// normal controller mappings
                    {
                        // buttons
                        bool triggerButton = (state.ulButtonPressed & 8589934592) != 0;// get normal trigger button state if avaliable
                        if (state.rAxis1.x >= .75f) triggerButton = true;// virtually simulate trigger button in case it doesn't exist
                        else triggerButton = false;
                        controller.buttonTrigger.Update(triggerButton);

                        controller.buttonJoystick.Update((state.ulButtonPressed & 4294967296) != 0);
                        controller.buttonGrip.Update((state.ulButtonPressed & 17179869188) != 0);
						controller.button1.Update((state.ulButtonPressed & 128) != 0);
                        controller.button2.Update((state.ulButtonPressed & 2) != 0);

                        // touch
                        controller.touchTrigger.Update((state.ulButtonTouched & 8589934592) != 0);
                        controller.touchJoystick.Update((state.ulButtonTouched & 4294967296) != 0);
                        controller.touchGrip.Update((state.ulButtonTouched & 17179869188) != 0);
                        controller.touch1.Update((state.ulButtonTouched & 128) != 0);
                        controller.touch2.Update((state.ulButtonTouched & 2) != 0);
                    }

                    /*// update analog states
                    controller.trigger.Update(state.rAxis1.x);

                    // update joystick states
                    if (state.ulButtonTouched != 0) controller.joystick.Update(new Vector2(state.rAxis0.x, state.rAxis0.y));
                    else controller.joystick.Update(Vector2.zero);*/

                    // update controller side
                    var role = system.GetControllerRoleForTrackedDeviceIndex(i);
                    switch (role)
                    {
                        case ETrackedControllerRole.LeftHand:
                            controller.side = XRControllerSide.Left;
                            state_controllerLeft = controller;
                            leftSet = true;
                            break;

                        case ETrackedControllerRole.RightHand:
                            controller.side = XRControllerSide.Right;
                            state_controllerRight = controller;
                            rightSet = true;
                            break;

                        default: controller.side = XRControllerSide.Unknown; break;
                    }

                    state_controllers[state_controllerCount] = controller;
                    ++state_controllerCount;
                }
            }
            #else
            foreach (var c in controllers)
            {
                if (!c.isValid || (c.characteristics & InputDeviceCharacteristics.Controller) == 0) continue;

                var controller = state_controllers[state_controllerCount];
                controller.connected = true;
                bool isMixedReality = c.name.StartsWith(deviceName_MixedReality);

                // update buttons states
                bool triggerValueValid = c.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
                bool triggerButton = false;
                if (!c.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButton))
                {
                    if (triggerValueValid)
                    {
                        if (triggerValue >= .75f) triggerButton = true;// virtually simulate trigger button in case it doesn't exist
                        else triggerButton = false;
                    }
                    else
                    {
                        triggerButton = false;
                    }
                }
                controller.buttonTrigger.Update(triggerButton);

                if (isMixedReality)
				{
                    if (c.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out bool joystickButton)) controller.buttonJoystick.Update(joystickButton);
                    else controller.buttonJoystick.Update(false);

                    if (c.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool joystickButton2)) controller.buttonJoystick2.Update(joystickButton2);
                    else controller.buttonJoystick2.Update(false);
				}
                else
				{
                    if (c.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool joystickButton)) controller.buttonJoystick.Update(joystickButton);
                    else controller.buttonJoystick.Update(false);
				}

                if (c.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton)) controller.buttonGrip.Update(gripButton);
                else controller.buttonGrip.Update(false);

                if (c.TryGetFeatureValue(CommonUsages.menuButton, out bool menuButton)) controller.buttonMenu.Update(menuButton);
                else controller.buttonMenu.Update(false);

                if (c.TryGetFeatureValue(CommonUsages.primaryButton, out bool button1)) controller.button1.Update(button1);
                else controller.button1.Update(false);

                if (c.TryGetFeatureValue(CommonUsages.secondaryButton, out bool button2)) controller.button2.Update(button2);
                else controller.button2.Update(false);

                // update analog states
                if (triggerValueValid) controller.trigger.Update(triggerValue);
                else controller.trigger.Update(0);

                // update joystick states
                if (isMixedReality)
				{
                    if (c.TryGetFeatureValue(CommonUsages.secondary2DAxis, out Vector2 joystick)) controller.joystick.Update(joystick);
                    else controller.joystick.Update(Vector2.zero);

                    if (c.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick2)) controller.joystick2.Update(joystick2);
                    else controller.joystick2.Update(Vector2.zero);
				}
                else
				{
                    if (c.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick)) controller.joystick.Update(joystick);
                    else controller.joystick.Update(Vector2.zero);
				}

                // update touch states
                if (isMixedReality)
				{
                    controller.touchJoystick.Update(false);
                    controller.touchJoystick2.Update(controller.joystick2.value.magnitude >= XRControllerJoystick.tolerance);
				}
                else
				{
                    if (c.TryGetFeatureValue(OculusUsages.indexTouch, out bool triggerTouch)) controller.touchTrigger.Update(triggerTouch);
                    else controller.touchTrigger.Update(false);

                    if (c.TryGetFeatureValue(OculusUsages.thumbTouch, out bool joystickTouch)) controller.touchJoystick.Update(joystickTouch);
                    else controller.touchJoystick.Update(false);
				}

                //if (c.TryGetFeatureValue(CommonUsages.gripTouch, out bool gripTouch)) controller.touchGrip.Update(gripTouch);// not supported
                //else controller.touchGrip.Update(false);

                //if (c.TryGetFeatureValue(CommonUsages.menuTouch, out bool menuTouch)) controller.touchMenu.Update(menuTouch);// not supported
                //else controller.touchMenu.Update(false);

                if (c.TryGetFeatureValue(CommonUsages.primaryTouch, out bool touch1)) controller.touch1.Update(touch1);
                else controller.touch1.Update(false);

                if (c.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool touch2)) controller.touch2.Update(touch2);
                else controller.touch2.Update(false);

                // update controller side
                if ((c.characteristics & InputDeviceCharacteristics.Left) != 0)
                {
                    controller.side = XRControllerSide.Left;
                    state_controllerLeft = controller;
                    leftSet = true;
                }
                else if ((c.characteristics & InputDeviceCharacteristics.Right) != 0)
                {
                    controller.side = XRControllerSide.Right;
                    state_controllerRight = controller;
                    rightSet = true;
                }
                else
                {
                    controller.side = XRControllerSide.Unknown;
                }

                state_controllers[state_controllerCount] = controller;
                ++state_controllerCount;
            }
            #endif

            // if left or right not known use controller index as side
            if (!leftSet || !rightSet)
            {
                if (state_controllerCount == 1)
                {
                    if (!leftSet && !rightSet)
                    {
                        state_controllerRight = state_controllers[0];
                        state_controllers[0].side = XRControllerSide.Right;
                        rightSet = true;
                    }
				}
                else if (state_controllerCount >= 2)
                {
                    state_controllerRight = state_controllers[0];
                    state_controllerLeft = state_controllers[1];
                    state_controllers[0].side = XRControllerSide.Right;
                    state_controllers[1].side = XRControllerSide.Left;
                    rightSet = true;
                    leftSet = true;
                }
            }

            // null memory if no state
            if (!leftSet) state_controllerLeft = new XRControllerState();
            if (!rightSet) state_controllerRight = new XRControllerState();

            // buffer special controller states
            if (state_controllerCount != 0) state_controllerFirst = state_controllers[0];
            else state_controllerFirst = new XRControllerState();

            state_controllerMerged = new XRControllerState();
            for (uint i = 0; i != singleton.state_controllerCount; ++i)
            {
                var controllerState = singleton.state_controllers[i];
                if (controllerState.connected) state_controllerMerged.connected = true;

                controllerState.buttonTrigger.Merge(ref state_controllerMerged.buttonTrigger);
                controllerState.buttonJoystick.Merge(ref state_controllerMerged.buttonJoystick);
                controllerState.buttonJoystick2.Merge(ref state_controllerMerged.buttonJoystick2);
                controllerState.buttonGrip.Merge(ref state_controllerMerged.buttonGrip);
                controllerState.buttonMenu.Merge(ref state_controllerMerged.buttonMenu);

                controllerState.button1.Merge(ref state_controllerMerged.button1);
                controllerState.button2.Merge(ref state_controllerMerged.button2);
                controllerState.button3.Merge(ref state_controllerMerged.button3);
                controllerState.button4.Merge(ref state_controllerMerged.button4);

                controllerState.touchTrigger.Merge(ref state_controllerMerged.touchTrigger);
                controllerState.touchJoystick.Merge(ref state_controllerMerged.touchJoystick);
                controllerState.touchJoystick2.Merge(ref state_controllerMerged.touchJoystick2);
                controllerState.touchGrip.Merge(ref state_controllerMerged.touchGrip);
                controllerState.touchMenu.Merge(ref state_controllerMerged.touchMenu);

                controllerState.touch1.Merge(ref state_controllerMerged.touch1);
                controllerState.touch2.Merge(ref state_controllerMerged.touch2);
                controllerState.touch3.Merge(ref state_controllerMerged.touch3);
                controllerState.touch4.Merge(ref state_controllerMerged.touch4);

                controllerState.trigger.Merge(ref state_controllerMerged.trigger);
                controllerState.joystick.Merge(ref state_controllerMerged.joystick);
                controllerState.joystick2.Merge(ref state_controllerMerged.joystick2);
            }

            // fire events
            // <<< buttons
            TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref state_controllerRight.buttonTrigger, XRControllerSide.Right);
            TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref state_controllerLeft.buttonTrigger, XRControllerSide.Left);

            TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref state_controllerRight.buttonJoystick, XRControllerSide.Right);
            TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref state_controllerLeft.buttonJoystick, XRControllerSide.Left);

            TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref state_controllerRight.buttonJoystick2, XRControllerSide.Right);
            TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref state_controllerLeft.buttonJoystick2, XRControllerSide.Left);

            TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref state_controllerRight.buttonGrip, XRControllerSide.Right);
            TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref state_controllerLeft.buttonGrip, XRControllerSide.Left);

            TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref state_controllerRight.buttonMenu, XRControllerSide.Right);
            TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref state_controllerLeft.buttonMenu, XRControllerSide.Left);

            TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref state_controllerRight.button1, XRControllerSide.Right);
            TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref state_controllerLeft.button1, XRControllerSide.Left);

            TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref state_controllerRight.button2, XRControllerSide.Right);
            TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref state_controllerLeft.button2, XRControllerSide.Left);

            TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref state_controllerRight.button3, XRControllerSide.Right);
            TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref state_controllerLeft.button3, XRControllerSide.Left);

            TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref state_controllerRight.button4, XRControllerSide.Right);
            TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref state_controllerLeft.button4, XRControllerSide.Left);

            // <<< touch
            TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref state_controllerRight.touchTrigger, XRControllerSide.Right);
            TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref state_controllerLeft.touchTrigger, XRControllerSide.Left);

            TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref state_controllerRight.touchJoystick, XRControllerSide.Right);
            TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref state_controllerLeft.touchJoystick, XRControllerSide.Left);

            TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref state_controllerRight.touchJoystick2, XRControllerSide.Right);
            TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref state_controllerLeft.touchJoystick2, XRControllerSide.Left);

            TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref state_controllerRight.touchGrip, XRControllerSide.Right);
            TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref state_controllerLeft.touchGrip, XRControllerSide.Left);

            TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref state_controllerRight.touchMenu, XRControllerSide.Right);
            TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref state_controllerLeft.touchMenu, XRControllerSide.Left);

            TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref state_controllerRight.touch1, XRControllerSide.Right);
            TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref state_controllerLeft.touch1, XRControllerSide.Left);

            TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref state_controllerRight.touch2, XRControllerSide.Right);
            TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref state_controllerLeft.touch2, XRControllerSide.Left);

            TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref state_controllerRight.touch3, XRControllerSide.Right);
            TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref state_controllerLeft.touch3, XRControllerSide.Left);

            TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref state_controllerRight.touch4, XRControllerSide.Right);
            TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref state_controllerLeft.touch4, XRControllerSide.Left);

            // <<< analogs
            TestAnalogEvent(TriggerActiveEvent, ref state_controllerRight.trigger, XRControllerSide.Right);
            TestAnalogEvent(TriggerActiveEvent, ref state_controllerLeft.trigger, XRControllerSide.Left);

            TestJoystickEvent(JoystickActiveEvent, ref state_controllerRight.joystick, XRControllerSide.Right);
            TestJoystickEvent(JoystickActiveEvent, ref state_controllerLeft.joystick, XRControllerSide.Left);

            TestJoystickEvent(Joystick2ActiveEvent, ref state_controllerRight.joystick2, XRControllerSide.Right);
            TestJoystickEvent(Joystick2ActiveEvent, ref state_controllerLeft.joystick2, XRControllerSide.Left);
        }

	    private void InputDevices_deviceConnected(InputDevice device)
	    {
            Debug.Log("XR Device connected: " + device.name);
            devices.Add(device);
            UpdateDevice(device, false);
        }

        private void InputDevices_deviceDisconnected(InputDevice device)
        {
            Debug.Log("XR Device disconnected: " + device.name);
            devices.Remove(device);
            UpdateDevice(device, true);
        }

        private void InputDevices_deviceConfigChanged(InputDevice device)
        {
            Debug.Log("XR Device config changed: " + device.name);
            var index = devices.FindIndex(x => x.name == device.name);
            devices[index] = device;
            UpdateDevice(device, false);
        }

        private void UpdateDevice(InputDevice device, bool removingDevice)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                if (controllers.Exists(x => x.name == device.name))
                {
				    if (!removingDevice)
				    {
					    var index = controllers.FindIndex(x => x.name == device.name);
					    controllers[index] = device;
				    }
                    else
                    {
                        controllers.Remove(device);
                    }
                }
                else
                {
                    if (!removingDevice) controllers.Add(device);
                }

                if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left))
                {
                    if (!removingDevice) handLeft = device;
                    else handLeft = new InputDevice();
			    }
                else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
                {
                    if (!removingDevice) handRight = device;
                    else handRight = new InputDevice();
			    }
            }
	    }

		#region Public static interface
        public delegate void ButtonEvent(XRControllerSide side);
        public delegate void AnalogEvent(XRControllerSide side, float value);
        public delegate void JoystickEvent(XRControllerSide side, Vector2 value);

        public static event ButtonEvent ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent;
        public static event ButtonEvent ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent;
        public static event ButtonEvent ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent;
        public static event ButtonEvent ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent;
        public static event ButtonEvent ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent;
        public static event ButtonEvent Button1OnEvent, Button1DownEvent, Button1UpEvent;
        public static event ButtonEvent Button2OnEvent, Button2DownEvent, Button2UpEvent;
        public static event ButtonEvent Button3OnEvent, Button3DownEvent, Button3UpEvent;
        public static event ButtonEvent Button4OnEvent, Button4DownEvent, Button4UpEvent;

        public static event ButtonEvent TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent;
        public static event ButtonEvent TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent;
        public static event ButtonEvent TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent;
        public static event ButtonEvent TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent;
        public static event ButtonEvent TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent;
        public static event ButtonEvent Touch1OnEvent, Touch1DownEvent, Touch1UpEvent;
        public static event ButtonEvent Touch2OnEvent, Touch2DownEvent, Touch2UpEvent;
        public static event ButtonEvent Touch3OnEvent, Touch3DownEvent, Touch3UpEvent;
        public static event ButtonEvent Touch4OnEvent, Touch4DownEvent, Touch4UpEvent;

        public static event AnalogEvent TriggerActiveEvent;
        public static event JoystickEvent JoystickActiveEvent, Joystick2ActiveEvent;

        private static void TestButtonEvent(ButtonEvent onEvent, ButtonEvent downEvent, ButtonEvent upEvent, ref XRControllerButton button, XRControllerSide side)
        {
            if (onEvent != null && button.on) onEvent(side);
            if (downEvent != null && button.down) downEvent(side);
            if (upEvent != null && button.up) upEvent(side);
        }

        private static void TestAnalogEvent(AnalogEvent e, ref XRControllerAnalog analog, XRControllerSide side)
        {
            if (e != null && analog.value != 0) e(side, analog.value);
        }

        private static void TestJoystickEvent(JoystickEvent e, ref XRControllerJoystick joystick, XRControllerSide side)
        {
            if (e != null && joystick.value.magnitude != 0) e(side, joystick.value);
        }

        /// <summary>
        /// Gets full controller state
        /// </summary>
        public static XRControllerState ControllerState(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst;
                case XRController.Left: return singleton.state_controllerLeft;
                case XRController.Right: return singleton.state_controllerRight;
                case XRController.Merged: return singleton.state_controllerMerged;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
	    }

        public static XRControllerButton ButtonTrigger(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonTrigger;
                case XRController.Left: return singleton.state_controllerLeft.buttonTrigger;
                case XRController.Right: return singleton.state_controllerRight.buttonTrigger;
                case XRController.Merged: return singleton.state_controllerMerged.buttonTrigger;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonJoystick(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonJoystick;
                case XRController.Left: return singleton.state_controllerLeft.buttonJoystick;
                case XRController.Right: return singleton.state_controllerRight.buttonJoystick;
                case XRController.Merged: return singleton.state_controllerMerged.buttonJoystick;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonJoystick2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonJoystick2;
                case XRController.Left: return singleton.state_controllerLeft.buttonJoystick2;
                case XRController.Right: return singleton.state_controllerRight.buttonJoystick2;
                case XRController.Merged: return singleton.state_controllerMerged.buttonJoystick2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonGrip(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonGrip;
                case XRController.Left: return singleton.state_controllerLeft.buttonGrip;
                case XRController.Right: return singleton.state_controllerRight.buttonGrip;
                case XRController.Merged: return singleton.state_controllerMerged.buttonGrip;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonMenu(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonMenu;
                case XRController.Left: return singleton.state_controllerLeft.buttonMenu;
                case XRController.Right: return singleton.state_controllerRight.buttonMenu;
                case XRController.Merged: return singleton.state_controllerMerged.buttonMenu;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button1(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button1;
                case XRController.Left: return singleton.state_controllerLeft.button1;
                case XRController.Right: return singleton.state_controllerRight.button1;
                case XRController.Merged: return singleton.state_controllerMerged.button1;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button2;
                case XRController.Left: return singleton.state_controllerLeft.button2;
                case XRController.Right: return singleton.state_controllerRight.button2;
                case XRController.Merged: return singleton.state_controllerMerged.button2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button3(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button3;
                case XRController.Left: return singleton.state_controllerLeft.button3;
                case XRController.Right: return singleton.state_controllerRight.button3;
                case XRController.Merged: return singleton.state_controllerMerged.button3;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button4(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button4;
                case XRController.Left: return singleton.state_controllerLeft.button4;
                case XRController.Right: return singleton.state_controllerRight.button4;
                case XRController.Merged: return singleton.state_controllerMerged.button4;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchTrigger(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchTrigger;
                case XRController.Left: return singleton.state_controllerLeft.touchTrigger;
                case XRController.Right: return singleton.state_controllerRight.touchTrigger;
                case XRController.Merged: return singleton.state_controllerMerged.touchTrigger;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchJoystick(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchJoystick;
                case XRController.Left: return singleton.state_controllerLeft.touchJoystick;
                case XRController.Right: return singleton.state_controllerRight.touchJoystick;
                case XRController.Merged: return singleton.state_controllerMerged.touchJoystick;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchJoystick2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchJoystick2;
                case XRController.Left: return singleton.state_controllerLeft.touchJoystick2;
                case XRController.Right: return singleton.state_controllerRight.touchJoystick2;
                case XRController.Merged: return singleton.state_controllerMerged.touchJoystick2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchGrip(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchGrip;
                case XRController.Left: return singleton.state_controllerLeft.touchGrip;
                case XRController.Right: return singleton.state_controllerRight.touchGrip;
                case XRController.Merged: return singleton.state_controllerMerged.touchGrip;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchMenu(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchMenu;
                case XRController.Left: return singleton.state_controllerLeft.touchMenu;
                case XRController.Right: return singleton.state_controllerRight.touchMenu;
                case XRController.Merged: return singleton.state_controllerMerged.touchMenu;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch1(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch1;
                case XRController.Left: return singleton.state_controllerLeft.touch1;
                case XRController.Right: return singleton.state_controllerRight.touch1;
                case XRController.Merged: return singleton.state_controllerMerged.touch1;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch2;
                case XRController.Left: return singleton.state_controllerLeft.touch2;
                case XRController.Right: return singleton.state_controllerRight.touch2;
                case XRController.Merged: return singleton.state_controllerMerged.touch2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch3(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch3;
                case XRController.Left: return singleton.state_controllerLeft.touch3;
                case XRController.Right: return singleton.state_controllerRight.touch3;
                case XRController.Merged: return singleton.state_controllerMerged.touch3;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch4(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch4;
                case XRController.Left: return singleton.state_controllerLeft.touch4;
                case XRController.Right: return singleton.state_controllerRight.touch4;
                case XRController.Merged: return singleton.state_controllerMerged.touch4;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerAnalog Trigger(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.trigger;
                case XRController.Left: return singleton.state_controllerLeft.trigger;
                case XRController.Right: return singleton.state_controllerRight.trigger;
                case XRController.Merged: return singleton.state_controllerMerged.trigger;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerJoystick Joystick(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.joystick;
                case XRController.Left: return singleton.state_controllerLeft.joystick;
                case XRController.Right: return singleton.state_controllerRight.joystick;
                case XRController.Merged: return singleton.state_controllerMerged.joystick;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerJoystick Joystick2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.joystick2;
                case XRController.Left: return singleton.state_controllerLeft.joystick2;
                case XRController.Right: return singleton.state_controllerRight.joystick2;
                case XRController.Merged: return singleton.state_controllerMerged.joystick2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }
        #endregion
    }

	public enum XRController
    {
        /// <summary>
        /// First controller connected
        /// </summary>
        First,

        /// <summary>
        /// All controller states merged
        /// </summary>
        Merged,

        /// <summary>
        /// Left controller states only
        /// </summary>
        Left,

        /// <summary>
        /// Right controller states only
        /// </summary>
        Right
    }

    public enum XRControllerSide
    {
        Unknown,
        Left,
        Right
	}

    public struct XRControllerState
    {
        public bool connected;
        public XRControllerSide side;
        public XRControllerButton touchTrigger, touchJoystick, touchJoystick2, touchGrip, touchMenu;
        public XRControllerButton touch1, touch2, touch3, touch4;
        public XRControllerButton buttonTrigger, buttonJoystick, buttonJoystick2, buttonGrip, buttonMenu;
        public XRControllerButton button1, button2 ,button3, button4;
        public XRControllerAnalog trigger;
        public XRControllerJoystick joystick, joystick2;
    }

    public struct XRControllerButton
    {
        public bool on, down, up;

        internal void Update(bool on)
        {
            down = false;
            up = false;
            if (this.on != on)
            {
                if (on) down = true;
                else if (!on) up = true;
            }
            this.on = on;
	    }

        internal void Merge(ref XRControllerButton button)
        {
            if (on) button.on = true;
            if (down) button.down = true;
            if (up) button.up = true;
		}
    }

    public struct XRControllerAnalog
    {
        public float value;
        public static float tolerance = 0.2f;

        internal void Update(float value)
        {
            if (value < tolerance) value = 0.0f;
            this.value = value;
	    }

        internal void Merge(ref XRControllerAnalog analog)
        {
            if (value >= tolerance) analog.value = value;
        }
    }

    public struct XRControllerJoystick
    {
        public Vector2 value;
        public static float tolerance = 0.2f;

        internal void Update(Vector2 value)
        {
            if (value.magnitude < tolerance) value = Vector2.zero;
            this.value = value;
        }

        internal void Merge(ref XRControllerJoystick joystick)
        {
            if (value.magnitude >= tolerance) joystick.value = value;
        }
    }
}