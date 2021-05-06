using UnityEngine;

using Valve.VR;
using System.Runtime.InteropServices;
using System.IO;

namespace VRstudios.API
{
	public sealed class OpenVR_New : XRInputAPI
	{
        private CVRSystem system;
        private bool isInit;
        private int leftHand = -1, rightHand = -1;

        private CVRInput input;
        private ulong viveSource_RightHand, viveSource_LeftHand;
        private ulong viveActionSetHandle;
        private ulong viveAction_TriggerTouch, viveAction_GripTouch, viveAction_MenuTouch, viveAction_Touch1, viveAction_Touch2, viveAction_Joystick1_Touch;
        private ulong viveAction_TriggerButton, viveAction_GripButton, viveAction_MenuButton, viveAction_Button1, viveAction_Button2, viveAction_Touchpad1_Button, viveAction_Joystick1_Button;
        private ulong viveAction_Trigger, viveAction_Touchpad1, viveAction_Joystick1;
        private ulong viveAction_Rumble_RightHand, viveAction_Rumble_LeftHand;
        private VRActiveActionSet_t[] actionSets;

        private bool GetInputSourceHandle(string path, ref ulong handle)
        {
            var e = input.GetInputSourceHandle(path, ref handle);
            if (e != EVRInputError.None)
            {
                Debug.LogError($"Failed: 'GetInputSourceHandle' with path '{path}': {e}");
                return false;
			}
            return true;
		}

        private bool GetActionSetHandle(string path, ref ulong handle)
        {
            var e = input.GetActionSetHandle(path, ref handle);
            if (e != EVRInputError.None)
            {
                Debug.LogError($"Failed: 'GetActionSetHandle' with path '{path}': {e}");
                return false;
			}
            return true;
		}

        private bool GetActionHandle(string path, ref ulong handle)
        {
            var e = input.GetActionHandle(path, ref handle);
            if (e != EVRInputError.None)
            {
                Debug.LogError($"Failed: 'GetActionHandle' with path '{path}': {e}");
                return false;
			}
            return true;
		}

		public override void Init()
		{
			base.Init();

            // make sure OpenVR is init right away
            EVRInitError e = EVRInitError.None;
            system = OpenVR.System;
            if (system == null) system = OpenVR.Init(ref e);
            Debug.Log("OpenVR version: " + system.GetRuntimeVersion());

            // init input system
            input = OpenVR.Input;
            string actionsPath;
            if (XRInput.singleton.steamSDK_InUse) actionsPath = Path.Combine(Application.dataPath, "StreamingAssets", "SteamVR", "actions.json");
            else actionsPath = Path.Combine(Application.dataPath, "StreamingAssets", "OpenVR", "vrstudios_actions.json");
            actionsPath = actionsPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            Debug.Log($"Loading OpenVR Input actions: '{actionsPath}'");
            var error = input.SetActionManifestPath(actionsPath);
            if (error != EVRInputError.None)
            {
                Debug.LogError("Failed: 'SetActionManifestPath': " + error.ToString());
                return;
			}

            // get hands
            GetInputSourceHandle("/user/hand/right", ref viveSource_RightHand);
            GetInputSourceHandle("/user/hand/left", ref viveSource_LeftHand);

            // get action set
            actionSets = new VRActiveActionSet_t[1];
            if (!GetActionSetHandle("/actions/vrstudios", ref viveActionSetHandle)) return;
            actionSets[0].ulActionSet = viveActionSetHandle;

            // get object actions (touch)
            GetActionHandle("/actions/vrstudios/in/triggertouch", ref viveAction_TriggerTouch);
            GetActionHandle("/actions/vrstudios/in/griptouch", ref viveAction_GripTouch);
            GetActionHandle("/actions/vrstudios/in/menutouch", ref viveAction_MenuTouch);
            GetActionHandle("/actions/vrstudios/in/touch1", ref viveAction_Touch1);
            GetActionHandle("/actions/vrstudios/in/touch2", ref viveAction_Touch2);
            GetActionHandle("/actions/vrstudios/in/joystick1_touch", ref viveAction_Joystick1_Touch);

            // get object actions (buttons)
            GetActionHandle("/actions/vrstudios/in/triggerbutton", ref viveAction_TriggerButton);
            GetActionHandle("/actions/vrstudios/in/gripbutton", ref viveAction_GripButton);
            GetActionHandle("/actions/vrstudios/in/menubutton", ref viveAction_MenuButton);
            GetActionHandle("/actions/vrstudios/in/button1", ref viveAction_Button1);
            GetActionHandle("/actions/vrstudios/in/button2", ref viveAction_Button2);
            GetActionHandle("/actions/vrstudios/in/touchpad1_button", ref viveAction_Touchpad1_Button);
            GetActionHandle("/actions/vrstudios/in/joystick1_button", ref viveAction_Joystick1_Button);

            // get object actions (triggers)
            GetActionHandle("/actions/vrstudios/in/trigger", ref viveAction_Trigger);

            // get object actions (touchpads)
            GetActionHandle("/actions/vrstudios/in/touchpad1", ref viveAction_Touchpad1);

            // get object actions (joysticks)
            GetActionHandle("/actions/vrstudios/in/joystick1", ref viveAction_Joystick1);

            // rumble
            GetActionHandle("/actions/vrstudios/out/haptic_right", ref viveAction_Rumble_RightHand);
            GetActionHandle("/actions/vrstudios/out/haptic_left", ref viveAction_Rumble_LeftHand);

            // finish
            isInit = true;
        }

		public override void Dispose()
		{
            isInit = false;
            #if !UNITY_EDITOR
            if (system != null)
            {
                OpenVR.Shutdown();
                system = null;
			}
            #endif
            base.Dispose();
		}

        private bool GetButtonState(ulong handle, ulong hand)
        {
            var dataSize = Marshal.SizeOf<InputDigitalActionData_t>();
            var data = new InputDigitalActionData_t();
            var error = input.GetDigitalActionData(handle, ref data, (uint)dataSize, hand);
            #if UNITY_EDITOR
            if (error != EVRInputError.None && error != EVRInputError.NoData)
            {
                Debug.LogError("GetDigitalActionData: " + error.ToString());
                return false;
			}
            #endif
            return data.bState;
		}

        private Vector3 GetAnalogState(ulong handle, ulong hand)
        {
            var dataSize = Marshal.SizeOf<InputAnalogActionData_t>();
            var data = new InputAnalogActionData_t();
            var error = input.GetAnalogActionData(handle, ref data, (uint)dataSize, hand);
            #if UNITY_EDITOR
            if (error != EVRInputError.None && error != EVRInputError.NoData)
            {
                Debug.LogError("GetAnalogActionData: " + error.ToString());
                return Vector3.zero;
			}
            #endif
            return new Vector3(data.x, data.y, data.z);
		}

		public unsafe override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			// defaults
            GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);

            // reset controllers
            ResetControllers(state_controllers);

            // validate OpenVR is avaliable
            if (!isInit) return false;
            if (system == null || !system.IsInputAvailable() || input.IsUsingLegacyInput()) return false;

            // get controller connection status and side
            for (uint i = 0; i != OpenVR.k_unMaxTrackedDeviceCount; ++i)
            {
                if (!system.IsTrackedDeviceConnected(i)) continue;
                if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller) continue;


                // get controller type
                ETrackedPropertyError e = ETrackedPropertyError.TrackedProp_Success;
                system.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_ControllerType_String, OpenVR_Shared.propertyText, (uint)OpenVR_Shared.propertyText.Capacity, ref e);

                // ignore gamepads
                if (OpenVR_Shared.propertyText.Equals(OpenVR_Shared.propertyText_Gamepad)) continue;

                // get controller
                var controller = state_controllers[controllerCount];
                controller.connected = true;

                if (e == ETrackedPropertyError.TrackedProp_Success)
                {
                    if (OpenVR_Shared.propertyText.Equals(OpenVR_Shared.propertyText_ViveController)) controller.type = XRInputControllerType.HTCVive;
                    else if (OpenVR_Shared.propertyText.Equals(OpenVR_Shared.propertyText_IndexController)) controller.type = XRInputControllerType.ValveIndex;
                    else if (OpenVR_Shared.propertyText.ToString().StartsWith(OpenVR_Shared.propertyText_Oculus.ToString())) controller.type = XRInputControllerType.Oculus;
                    else if (OpenVR_Shared.propertyText.Equals(OpenVR_Shared.propertyText_WMR)) controller.type = XRInputControllerType.WMR;
                    else if (OpenVR_Shared.propertyText.Equals(OpenVR_Shared.propertyText_WMR_G2)) controller.type = XRInputControllerType.WMR_G2;
                    else controller.type = XRInputControllerType.Unknown;
				}

                // update controller side
                var role = system.GetControllerRoleForTrackedDeviceIndex(i);
                switch (role)
                {
                    case ETrackedControllerRole.LeftHand:
                        controller.side = XRControllerSide.Left;
                        leftSet = true;
                        leftSetIndex = controllerCount;
                        leftHand = (int)i;
                        break;

                    case ETrackedControllerRole.RightHand:
                        controller.side = XRControllerSide.Right;
                        rightSet = true;
                        rightSetIndex = controllerCount;
                        rightHand = (int)i;
                        break;

                    default: controller.side = XRControllerSide.Unknown; break;
                }

                state_controllers[controllerCount] = controller;
                ++controllerCount;
			}

            // pre-finish/pre-resolve unknown controller sides
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);

            // update inputs
            var error = input.UpdateActionState(actionSets, (uint)Marshal.SizeOf<VRActiveActionSet_t>());
            if (error != EVRInputError.None) Debug.LogError("UpdateActionState: " + error.ToString());

            // get hands
            var controllerRight = new XRControllerState();
            var controllerLeft = new XRControllerState();
            if (rightSet) controllerRight = state_controllers[rightSetIndex];
            if (leftSet) controllerLeft = state_controllers[leftSetIndex];
            
            // update trigger buttons/touch
            controllerRight.buttonTrigger.Update(GetButtonState(viveAction_TriggerButton, viveSource_RightHand));
            controllerLeft.buttonTrigger.Update(GetButtonState(viveAction_TriggerButton, viveSource_LeftHand));
            controllerRight.touchTrigger.Update(GetButtonState(viveAction_TriggerTouch, viveSource_RightHand));
            controllerLeft.touchTrigger.Update(GetButtonState(viveAction_TriggerTouch, viveSource_LeftHand));

            // update grip buttons/touch
            controllerRight.buttonGrip.Update(GetButtonState(viveAction_GripButton, viveSource_RightHand));
            controllerLeft.buttonGrip.Update(GetButtonState(viveAction_GripButton, viveSource_LeftHand));
            controllerRight.touchGrip.Update(GetButtonState(viveAction_GripTouch, viveSource_RightHand));
            controllerLeft.touchGrip.Update(GetButtonState(viveAction_GripTouch, viveSource_LeftHand));

            // update menu buttons/touch
            controllerRight.buttonMenu.Update(GetButtonState(viveAction_MenuButton, viveSource_RightHand));
            controllerLeft.buttonMenu.Update(GetButtonState(viveAction_MenuButton, viveSource_LeftHand));
            controllerRight.touchMenu.Update(GetButtonState(viveAction_MenuTouch, viveSource_RightHand));
            controllerLeft.touchMenu.Update(GetButtonState(viveAction_MenuTouch, viveSource_LeftHand));

            // update button/touch 1
            controllerRight.button1.Update(GetButtonState(viveAction_Button1, viveSource_RightHand));
            controllerLeft.button1.Update(GetButtonState(viveAction_Button1, viveSource_LeftHand));
            controllerRight.touch1.Update(GetButtonState(viveAction_Touch1, viveSource_RightHand));
            controllerLeft.touch1.Update(GetButtonState(viveAction_Touch1, viveSource_LeftHand));

            // update button/touch 2
            controllerRight.button2.Update(GetButtonState(viveAction_Button2, viveSource_RightHand));
            controllerLeft.button2.Update(GetButtonState(viveAction_Button2, viveSource_LeftHand));
            controllerRight.touch2.Update(GetButtonState(viveAction_Touch2, viveSource_RightHand));
            controllerLeft.touch2.Update(GetButtonState(viveAction_Touch2, viveSource_LeftHand));

            // update grip (just simulate)
            controllerRight.grip.Update(controllerRight.buttonGrip.on ? 1 : 0);
            controllerLeft.grip.Update(controllerLeft.buttonGrip.on ? 1 : 0);

            // update triggers
            controllerRight.trigger.Update(GetAnalogState(viveAction_Trigger, viveSource_RightHand).x);
            controllerLeft.trigger.Update(GetAnalogState(viveAction_Trigger, viveSource_LeftHand).x);

            // update trackpads / touch / button
            if (controllerRight.type == XRInputControllerType.HTCVive)
            {
                controllerRight.joystick.Update(GetAnalogState(viveAction_Touchpad1, viveSource_RightHand));
                controllerRight.buttonJoystick.Update(GetButtonState(viveAction_Touchpad1_Button, viveSource_RightHand));
			}
            else
            {
                controllerRight.joystick2.Update(GetAnalogState(viveAction_Touchpad1, viveSource_RightHand));
                controllerRight.buttonJoystick2.Update(GetButtonState(viveAction_Touchpad1_Button, viveSource_RightHand));
			}

            if (controllerLeft.type == XRInputControllerType.HTCVive)
            {
                controllerLeft.joystick.Update(GetAnalogState(viveAction_Touchpad1, viveSource_LeftHand));
                controllerLeft.buttonJoystick.Update(GetButtonState(viveAction_Touchpad1_Button, viveSource_LeftHand));
			}
            else
            {
                controllerLeft.joystick2.Update(GetAnalogState(viveAction_Touchpad1, viveSource_LeftHand));
                controllerLeft.buttonJoystick2.Update(GetButtonState(viveAction_Touchpad1_Button, viveSource_LeftHand));
			}


            // update joysticks / touch / button
            if (controllerRight.type != XRInputControllerType.HTCVive)
            {
                controllerRight.joystick.Update(GetAnalogState(viveAction_Joystick1, viveSource_RightHand));
                controllerRight.touchJoystick.Update(GetButtonState(viveAction_Joystick1_Touch, viveSource_RightHand));
                controllerRight.buttonJoystick.Update(GetButtonState(viveAction_Joystick1_Button, viveSource_RightHand));
			}
            
            if (controllerLeft.type != XRInputControllerType.HTCVive)
            {
                controllerLeft.joystick.Update(GetAnalogState(viveAction_Joystick1, viveSource_LeftHand));
                controllerLeft.touchJoystick.Update(GetButtonState(viveAction_Joystick1_Touch, viveSource_LeftHand));
                controllerLeft.buttonJoystick.Update(GetButtonState(viveAction_Joystick1_Button, viveSource_LeftHand));
			}

            // copy back hand updates
            if (rightSet) state_controllers[rightSetIndex] = controllerRight;
            if (leftSet) state_controllers[leftSetIndex] = controllerLeft;

            return true;
		}

		public override bool SetRumble(XRControllerRumbleSide controller, float strength, float duration)
		{
            if (strength < 0) strength = 0;
            if (strength > 1) strength = 1;
            if (duration < 0) duration = 0;

            if (leftHand >= 0 && (controller == XRControllerRumbleSide.Left || controller == XRControllerRumbleSide.Both))
            {
                input.TriggerHapticVibrationAction(viveAction_Rumble_LeftHand, 0, duration, 1, strength, 0);
			}
			
            if (rightHand >= 0 && (controller == XRControllerRumbleSide.Right || controller == XRControllerRumbleSide.Both))
            {
                input.TriggerHapticVibrationAction(viveAction_Rumble_RightHand, 0, duration, 1, strength, 0);
			}

			return true;
		}
	}
}