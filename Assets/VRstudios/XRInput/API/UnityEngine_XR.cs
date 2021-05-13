using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.Oculus;

namespace VRstudios.API
{
    public sealed class UnityEngine_XR : XRInputAPI
    {
        private List<InputDevice> controllers;

        /// <summary>
        /// Native UnityXR Left-Hand device handle
        /// </summary>
        public static InputDevice handLeft { get; private set; }

        /// <summary>
        /// Native UnityXR Right-Hand device handle
        /// </summary>
        public static InputDevice handRight { get; private set; }

        public override void Init()
		{
			base.Init();
            controllers = new List<InputDevice>();

            // add existing devices (this is needed to handle OVR bug where left controller may not be added via callbacks)
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllers);
            foreach (var c in controllers) UpdateDevice(c, false);

            // watch for device changes
            InputDevices.deviceConnected += InputDevices_deviceConnected;
            InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
            InputDevices.deviceConfigChanged += InputDevices_deviceConfigChanged;
        }

		public override void Dispose()
		{
            InputDevices.deviceConnected -= InputDevices_deviceConnected;
            InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
            InputDevices.deviceConfigChanged -= InputDevices_deviceConfigChanged;

            base.Dispose();
		}

		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			// defaults
			GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);

            // reset controllers
            ResetControllers(state_controllers);
            
            // gather input
            foreach (var c in controllers)
            {
                if (!c.isValid || (c.characteristics & InputDeviceCharacteristics.Controller) == 0) continue;

                var controller = state_controllers[controllerCount];
                controller.connected = true;

                // set type
                if (c.name.StartsWith("Oculus")) controller.type = XRInputControllerType.Oculus;
                else if (c.name.StartsWith("Spatial Controller")) controller.type = XRInputControllerType.WMR;
                else if (c.name.StartsWith("HP Reverb G2 Controller")) controller.type = XRInputControllerType.WMR_G2;
                else if (c.name.StartsWith("HTC Vive")) controller.type = XRInputControllerType.HTCVive;
                else if (c.name.StartsWith("Index Controller")) controller.type = XRInputControllerType.ValveIndex;
                else controller.type = XRInputControllerType.Unknown;

                bool simulateGripAnalog = controller.type != XRInputControllerType.Oculus && controller.type != XRInputControllerType.WMR_G2;

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

                if (controller.type == XRInputControllerType.WMR)
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

                if (simulateGripAnalog && controller.buttonGrip.on) controller.grip.Update(1);
                else if (c.TryGetFeatureValue(CommonUsages.grip, out float gripValue)) controller.grip.Update(gripValue);
                else controller.grip.Update(0);

                // update joystick states
                if (controller.type == XRInputControllerType.WMR)
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

                    if (c.TryGetFeatureValue(CommonUsages.secondary2DAxis, out Vector2 joystick2)) controller.joystick2.Update(joystick2);
                    else controller.joystick2.Update(Vector2.zero);
                }

                // update touch states
                if (controller.type == XRInputControllerType.WMR)
                {
                    controller.touchJoystick.Update(false);
                    controller.touchJoystick2.Update(controller.joystick2.value.magnitude >= XRControllerJoystick.tolerance);
                }
                else if (controller.type == XRInputControllerType.Oculus)
                {
                    if (c.TryGetFeatureValue(OculusUsages.indexTouch, out bool triggerTouch)) controller.touchTrigger.Update(triggerTouch);
                    else controller.touchTrigger.Update(false);

                    if (c.TryGetFeatureValue(OculusUsages.thumbTouch, out bool joystickTouch)) controller.touchJoystick.Update(joystickTouch);
                    else controller.touchJoystick.Update(false);
                }

                if (c.TryGetFeatureValue(CommonUsages.primaryTouch, out bool touch1)) controller.touch1.Update(touch1);
                else controller.touch1.Update(false);

                if (c.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool touch2)) controller.touch2.Update(touch2);
                else controller.touch2.Update(false);

                // update controller side
                if ((c.characteristics & InputDeviceCharacteristics.Left) != 0)
                {
                    if (leftSet)// if left already set, assume this is right
                    {
                        controller.side = XRControllerSide.Right;
                        rightSet = true;
                        rightSetIndex = controllerCount;
                        handRight = c;
                    }
                    else
                    {
                        controller.side = XRControllerSide.Left;
                        leftSet = true;
                        leftSetIndex = controllerCount;
                        handLeft = c;
                    }
                }
                else if ((c.characteristics & InputDeviceCharacteristics.Right) != 0)
                {
                    if (rightSet)// if right already set, assume this is left
                    {
                        controller.side = XRControllerSide.Left;
                        leftSet = true;
                        leftSetIndex = controllerCount;
                        handLeft = c;
                    }
                    else
                    {
                        controller.side = XRControllerSide.Right;
                        rightSet = true;
                        rightSetIndex = controllerCount;
                        handRight = c;
                    }
                }
                else
                {
                    controller.side = XRControllerSide.Unknown;
                }

                state_controllers[controllerCount] = controller;
                ++controllerCount;
            }

            // finish
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);
            if (sideToSet == SideToSet.Left || sideToSet == SideToSet.Both) handLeft = controllers[leftSetIndex];
            if (sideToSet == SideToSet.Right || sideToSet == SideToSet.Both) handRight = controllers[rightSetIndex];

            return true;
		}

        private bool ControllersMatch(InputDevice device1, InputDevice device2)
        {
            return device1.characteristics == device2.characteristics && device1.serialNumber == device2.serialNumber && device1.name == device2.name;
        }

        private bool FindExistingController(InputDevice device, out int index)
        {
            if (controllers.Exists(x => ControllersMatch(x, device)))
            {
                index = controllers.FindIndex(x => ControllersMatch(x, device));
                return true;
            }

            index = -1;
            return false;
        }

        private bool ReplaceControllerIfExists(InputDevice device)
        {
            if (FindExistingController(device, out int index))
            {
                controllers[index] = device;
                return true;
            }
            return false;
        }

        private void InputDevices_deviceConnected(InputDevice device)
        {
            Debug.Log("XR Device connected: " + device.name);
            if (!ReplaceControllerIfExists(device)) controllers.Add(device);
            UpdateDevice(device, false);
        }

        private void InputDevices_deviceDisconnected(InputDevice device)
        {
            Debug.Log("XR Device disconnected: " + device.name);
            controllers.Remove(device);
            UpdateDevice(device, true);
        }

        private void InputDevices_deviceConfigChanged(InputDevice device)
        {
            Debug.Log("XR Device config changed: " + device.name);
            ReplaceControllerIfExists(device);
            UpdateDevice(device, false);
        }

        private void UpdateDevice(InputDevice device, bool removingDevice)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                if (FindExistingController(device, out int index))
                {
                    if (!removingDevice)
                    {
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
                    if (!removingDevice)
                    {
                        Debug.Log("XR Device Left-Hand configued");
                        handLeft = device;
                    }
                    else
                    {
                        Debug.Log("XR Device Left-Hand removed");
                        handLeft = new InputDevice();
                    }
                }
                else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
                {
                    if (!removingDevice)
                    {
                        Debug.Log("XR Device Right-Hand configued");
                        handLeft = device;
                    }
                    else
                    {
                        Debug.Log("XR Device Right-Hand removed");
                        handLeft = new InputDevice();
                    }
                }
            }
        }

		public override bool SetRumble(XRControllerRumbleSide controller, float strength, float duration)
		{
            if (strength < 0) strength = 0;
            if (strength > 1) strength = 1;
            if (duration < 0) duration = 0;

            if (controller == XRControllerRumbleSide.Left || controller == XRControllerRumbleSide.Both)
            {
                HapticCapabilities capabilities;
                if (handLeft.TryGetHapticCapabilities(out capabilities))
                {
                    if (capabilities.supportsImpulse) return handLeft.SendHapticImpulse(XRInput.singleton.rumbleChannel, strength, duration);
                }
            }

            if (controller == XRControllerRumbleSide.Right || controller == XRControllerRumbleSide.Both)
            {
                HapticCapabilities capabilities;
                if (handRight.TryGetHapticCapabilities(out capabilities))
                {
                    if (capabilities.supportsImpulse) return handRight.SendHapticImpulse(XRInput.singleton.rumbleChannel, strength, duration);
                }
            }

            return false;
        }
    }
}