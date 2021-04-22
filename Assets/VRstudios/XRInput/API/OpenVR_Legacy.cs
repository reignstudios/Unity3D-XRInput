using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace VRstudios.API
{
#if UNITY_STANDALONE
    public sealed class OpenVR_Legacy : XRInputAPI
    {
        private const uint controllerStateLength = OpenVR.k_unMaxTrackedDeviceCount;
        private CVRSystem system;
        private int leftHand = -1, rightHand = -1;

        // NOTE: capacity must match 'propertyText' for equals to work (in this case 256)
        private StringBuilder propertyText = new StringBuilder(256);
        private StringBuilder propertyText_ViveController = new StringBuilder("vive_controller", 256);
        private StringBuilder propertyText_IndexController = new StringBuilder("knuckles", 256);

		public override void Init()
		{
			base.Init();

            // make sure OpenVR is init right away
            EVRInitError e = EVRInitError.None;
            system = OpenVR.Init(ref e);
            Debug.Log("OpenVR version: " + system.GetRuntimeVersion());
        }

		public override void Dispose()
		{
            if (system != null)
            {
                OpenVR.Shutdown();
                system = null;
			}
            base.Dispose();
		}

		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
        {
            // defaults
            GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);
            leftHand = -1;
            rightHand = -1;

            // validate OpenVR is avaliable
            var system = OpenVR.System;
            if (system == null || !system.IsInputAvailable()) return false;

            // gather input
            for (uint i = 0; i != controllerStateLength; ++i)
            {
                if (!system.IsTrackedDeviceConnected(i)) continue;

                // update controller state
                if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller) continue;
                var state = new VRControllerState_t();
                if (system.GetControllerState(i, ref state, (uint)Marshal.SizeOf<VRControllerState_t>()))
                {
                    var controller = state_controllers[controllerCount];
                    controller.connected = true;

                    // get controller type
                    ETrackedPropertyError e = ETrackedPropertyError.TrackedProp_Success;
                    system.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_ControllerType_String, propertyText, (uint)propertyText.Capacity, ref e);

                    // update button & touch states
                    if (e == ETrackedPropertyError.TrackedProp_Success)
                    {
                        if (propertyText.Equals(propertyText_ViveController))// specialize input for odd Vive button layout
                        {
                            controller.type = XRInputControllerType.HTCVive;

                            // buttons
                            controller.buttonTrigger.Update((state.ulButtonPressed & 8589934592) != 0);
                            controller.buttonGrip.Update((state.ulButtonPressed & 4) != 0);
                            controller.buttonMenu.Update((state.ulButtonPressed & 2) != 0);
                            controller.button1.Update((state.ulButtonPressed & 4294967296) != 0);

                            // touch
                            controller.touchTrigger.Update((state.ulButtonTouched & 8589934592) != 0);
                            controller.touch1.Update((state.ulButtonTouched & 4294967296) != 0);

                            // update joystick states
                            if (state.ulButtonTouched != 0) controller.joystick.Update(new Vector2(state.rAxis0.x, state.rAxis0.y));
                            else controller.joystick.Update(Vector2.zero);
                        }
                        else if (propertyText.Equals(propertyText_IndexController))// specialize input for odd Vive button layout
                        {
                            controller.type = XRInputControllerType.ValveIndex;

                            // buttons
                            controller.buttonJoystick.Update((state.ulButtonTouched & 4294967296) != 0);
                            controller.buttonTrigger.Update((state.ulButtonPressed & 8589934592) != 0);
                            controller.buttonGrip.Update((state.ulButtonPressed & 4) != 0);
                            //controller.button2.Update((state.ulButtonPressed & 4) != 0);// button 2 is the same as grip
                            controller.button1.Update((state.ulButtonPressed & 2) != 0);

                            // touch
                            controller.touchJoystick.Update((state.ulButtonTouched & 4294967296) != 0);
                            controller.touchTrigger.Update((state.ulButtonTouched & 8589934592) != 0);
                            controller.touchGrip.Update((state.ulButtonTouched & 4) != 0);
                            //controller.touch2.Update((state.ulButtonTouched & 4) != 0);// button 2 is the same as grip
                            controller.touch1.Update((state.ulButtonTouched & 2) != 0);

                            // update joystick states
                            controller.joystick.Update(new Vector2(state.rAxis0.x, state.rAxis0.y));
                        }
                    }
                    else// agnostic controller mappings
                    {
                        controller.type = XRInputControllerType.Unknown;

                        // buttons
                        bool triggerButton = (state.ulButtonPressed & 8589934592) != 0;// get normal trigger button state if avaliable
                        if (state.rAxis1.x >= .75f) triggerButton = true;// virtually simulate trigger button in case it doesn't exist
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

                        // update joystick states
                        controller.joystick.Update(new Vector2(state.rAxis0.x, state.rAxis0.y));
                    }

                    // update analog states
                    controller.trigger.Update(state.rAxis1.x);

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
            }

            // finish
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);
            return true;
        }

		public override bool SetRumble(XRControllerRumbleSide controller, float strength, float duration)
		{
            if (leftHand >= 0 && (controller == XRControllerRumbleSide.Left || controller == XRControllerRumbleSide.Both))
            {
                system.TriggerHapticPulse((uint)leftHand, 0, (ushort)(duration * 1000000));
			}
			
            if (rightHand >= 0 && (controller == XRControllerRumbleSide.Right || controller == XRControllerRumbleSide.Both))
            {
                system.TriggerHapticPulse((uint)rightHand, 0, (ushort)(duration * 1000000));
			}

            return true;
		}
	}
#else
    /// <summary>
    /// Shim when Unity not in standalone/PC mode
    /// </summary>
	public sealed class OpenVR_Legacy : XRInputAPI
	{
		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			throw new System.NotImplementedException();
		}
	}
#endif
}