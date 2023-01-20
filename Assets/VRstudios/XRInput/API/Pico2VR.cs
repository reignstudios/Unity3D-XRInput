using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !XRINPUT_DISABLE_PICO2
using Pvr_UnitySDKAPI;
#endif

namespace VRstudios.API
{
	#if !XRINPUT_DISABLE_PICO2
	public class Pico2VR : XRInputAPI
	{
		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			// defaults
            GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);

			// validate required objects
			//if (Pvr_ControllerManager.controllerlink == null) return false;

			// gather input
			//var controller = Pvr_ControllerManager.controllerlink.Controller0;
			//if (controller != null)
			{
				leftSet = true;
                leftSetIndex = controllerCount;
				GatherInputForController(0, ref state_controllers[controllerCount]);
				controllerCount++;
			}

			//controller = Pvr_ControllerManager.controllerlink.Controller1;
			//if (controller != null)
			{
				rightSet = true;
                rightSetIndex = controllerCount;
				GatherInputForController(1, ref state_controllers[controllerCount]);
				controllerCount++;
			}

			// TODO: make simple tracking driver
			//Controller.UPvr_GetControllerPOS // Get controller pos
			//Controller.UPvr_GetControllerQUA // get controller rot

			// hmd velocity
            /*hmdLinearVelocityValid = true;
            hmdLinearVelocity = OVRPlugin.GetNodeVelocity(OVRPlugin.Node.Head, OVRPlugin.Step.Render).FromFlippedZVector3f();

            hmdAngularVelocityValid = true;
            hmdAngularVelocity = OVRPlugin.GetNodeAngularVelocity(OVRPlugin.Node.Head, OVRPlugin.Step.Render).FromFlippedZVector3f();*/

			// finish
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);

			return true;
		}

		private void GatherInputForController(int controllerIndex, ref XRControllerState state_controller)
		{
			var side = controllerIndex == 0 ? XRControllerSide.Left : XRControllerSide.Right;
			state_controller.side = side;
			
			// common buttons
            state_controller.button1.Update(Controller.UPvr_GetKey(controllerIndex, side == XRControllerSide.Right ? Pvr_KeyCode.A : Pvr_KeyCode.X));
            state_controller.button2.Update(Controller.UPvr_GetKey(controllerIndex, side == XRControllerSide.Right ? Pvr_KeyCode.B : Pvr_KeyCode.Y));
            state_controller.buttonJoystick.Update(Controller.UPvr_GetKey(controllerIndex, Pvr_KeyCode.TOUCHPAD));
            state_controller.buttonTrigger.Update(Controller.UPvr_GetKey(controllerIndex, Pvr_KeyCode.TRIGGER));
			bool gripOn = Controller.UPvr_GetKey(controllerIndex, side == XRControllerSide.Right ? Pvr_KeyCode.Right : Pvr_KeyCode.Left);
            state_controller.buttonGrip.Update(gripOn);

            // analogs
            state_controller.trigger.Update(Controller.UPvr_GetControllerTriggerValue(controllerIndex) / 255.0f);
            state_controller.grip.Update(gripOn ? 1 : 0);

            // joysticks
            state_controller.joystick.Update(Controller.UPvr_GetAxis2D(controllerIndex));
			
            // touch
            state_controller.touch1.Update(state_controller.button1.on);
            state_controller.touch2.Update(state_controller.button2.on);
            state_controller.touchTrigger.Update(state_controller.trigger.value >= XRControllerAnalog.tolerance);
            state_controller.touchGrip.Update(state_controller.grip.value >= XRControllerAnalog.tolerance);
            state_controller.touchJoystick.Update(Controller.UPvr_IsTouching(controllerIndex));

			// velocity
            /*state_controller.linearVelocityValid = true;
            state_controller.linearVelocity = OVRInput.GetLocalControllerVelocity(controller);

            state_controller.angularVelocityValid = true;
            state_controller.angularVelocity = -OVRInput.GetLocalControllerAngularVelocity(controller);*/
		}

		public override bool SetRumble(XRControllerRumbleSide controller, float strength, float duration)
		{
			//Controller.UPvr_SetStationRestart
			return base.SetRumble(controller, strength, duration);// TODO
		}
	}
	#else
	/// <summary>
    /// Shim when disabled
    /// </summary>
	public sealed class Pico2VR : XRInputAPI
	{
		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			throw new System.NotImplementedException();
		}
	}
	#endif
}