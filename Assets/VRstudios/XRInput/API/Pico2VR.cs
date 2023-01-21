using UnityEngine;

#if !XRINPUT_DISABLE_PICO2
using Pvr_UnitySDKAPI;
#endif

namespace VRstudios.API
{
	#if !XRINPUT_DISABLE_PICO2
	public sealed class Pico2VR : XRInputAPI
	{
		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			// defaults
            GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);

			if (Pvr_UnitySDKManager.SDK != null)
			{
				// force hand index
				const int leftHandIndex = 0;
				const int rightHandIndex = 1;
				Controller.UPvr_SetHandNess(Pvr_Controller.UserHandNess.Left);
				Controller.UPvr_SetMainHandNess(leftHandIndex);

				// get hand index
				//int mainControllerIndex = Controller.UPvr_GetMainHandNess();
				//var mainControlerSide = Controller.UPvr_GetHandNess();
				//int leftHandIndex = mainControlerSide == Pvr_Controller.UserHandNess.Left ? mainControllerIndex : (1 - mainControllerIndex);
				//int rightHandIndex = 1 - leftHandIndex;

				// gather input
				if (Controller.UPvr_GetControllerState(leftHandIndex) == ControllerState.Connected)
				{
					leftSet = true;
					leftSetIndex = controllerCount;
					GatherInputForController(leftHandIndex, XRControllerSide.Left, ref state_controllers[controllerCount]);
					controllerCount++;
				}

				if (Controller.UPvr_GetControllerState(rightHandIndex) == ControllerState.Connected)
				{
					rightSet = true;
					rightSetIndex = controllerCount;
					GatherInputForController(rightHandIndex, XRControllerSide.Right, ref state_controllers[controllerCount]);
					controllerCount++;
				}

				// hmd velocity (doesn't seem to be supported in API)
				hmdLinearVelocityValid = false;
				hmdLinearVelocity = Vector3.zero;

				hmdAngularVelocityValid = false;
				hmdAngularVelocity = Vector3.zero;
			}

			// finish
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);

			return true;
		}

		private void GatherInputForController(int controllerIndex, XRControllerSide side, ref XRControllerState state_controller)
		{
			state_controller.side = side;
			state_controller.connected = true;
			
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
            state_controller.linearVelocityValid = true;
            state_controller.linearVelocity = Controller.UPvr_GetVelocity(controllerIndex) / 1000f;
			state_controller.linearVelocity.z = -state_controller.linearVelocity.z;

            state_controller.angularVelocityValid = true;
			state_controller.angularVelocity = -Controller.UPvr_GetAngularVelocity(controllerIndex);
			state_controller.angularVelocity.z = -state_controller.angularVelocity.z;
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