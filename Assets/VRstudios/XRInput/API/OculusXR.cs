using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;
using System.Runtime.InteropServices;

namespace VRstudios.API
{
    public sealed class OculusXR : XRInputAPI
    {
        private float rightRumbleTime, leftRumbleTime;

        public override void LateUpdate()
        {
            if (rightRumbleTime > 0)
            {
                rightRumbleTime -= Time.deltaTime;
                if (rightRumbleTime <= 0) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            }

            if (leftRumbleTime > 0)
            {
                leftRumbleTime -= Time.deltaTime;
                if (leftRumbleTime <= 0) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            }
        }

        public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
        {
            // defaults
            GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);

            // gather input
            controllerCount = 0;

            if (GatherInputForController(OVRInput.Controller.RHand, ref state_controllers[controllerCount]))
            {
                rightSet = true;
                rightSetIndex = controllerCount;
                controllerCount++;
            }

            if (GatherInputForController(OVRInput.Controller.LHand, ref state_controllers[controllerCount]))
            {
                leftSet = true;
                leftSetIndex = controllerCount;
                controllerCount++;
            }

            // finish
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);

            return true;
        }

        private bool GatherInputForController(OVRInput.Controller controller, ref XRControllerState state_controller)
        {
            if (OVRInput.IsControllerConnected(controller)) return false;

            if (controller == OVRInput.Controller.RHand)
            {
                // common buttons
                state_controller.button1.Update(OVRInput.Get(OVRInput.RawButton.A, controller));
                state_controller.button2.Update(OVRInput.Get(OVRInput.RawButton.B, controller));
                state_controller.buttonJoystick.Update(OVRInput.Get(OVRInput.RawButton.RThumbstick, controller));
                state_controller.buttonTrigger.Update(OVRInput.Get(OVRInput.RawButton.RIndexTrigger, controller));
                state_controller.buttonGrip.Update(OVRInput.Get(OVRInput.RawButton.RHandTrigger, controller));

                // analogs
                state_controller.trigger.Update(OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger, controller));
                state_controller.grip.Update(OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger, controller));

                // joysticks
                state_controller.joystick.Update(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, controller));
            }
            else if (controller == OVRInput.Controller.LHand)
            {
                // common buttons
                state_controller.button1.Update(OVRInput.Get(OVRInput.RawButton.X, controller));
                state_controller.button2.Update(OVRInput.Get(OVRInput.RawButton.Y, controller));
                state_controller.buttonJoystick.Update(OVRInput.Get(OVRInput.RawButton.LThumbstick, controller));
                state_controller.buttonTrigger.Update(OVRInput.Get(OVRInput.RawButton.LIndexTrigger, controller));
                state_controller.buttonGrip.Update(OVRInput.Get(OVRInput.RawButton.LHandTrigger, controller));

                // special buttons
                state_controller.buttonMenu.Update(OVRInput.Get(OVRInput.RawButton.Start, controller));

                // analogs
                state_controller.trigger.Update(OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger, controller));
                state_controller.grip.Update(OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger, controller));

                // joysticks
                state_controller.joystick.Update(OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, controller));
            }

            // velocity
            state_controller.linearVelocityValid = true;
            state_controller.linearVelocity = OVRInput.GetLocalControllerVelocity(controller);

            state_controller.angularVelocityValid = true;
            state_controller.angularVelocity = -OVRInput.GetLocalControllerAngularVelocity(controller);
            state_controller.angularVelocity = OVRInput.GetLocalControllerRotation(controller) * state_controller.angularVelocity;

            return true;
        }

        public override bool SetRumble(XRControllerRumbleSide controller, float strength, float duration)
        {
            if (controller == XRControllerRumbleSide.Right || controller == XRControllerRumbleSide.Both)
            {
                OVRInput.SetControllerVibration(1, strength, OVRInput.Controller.RTouch);
                rightRumbleTime = duration;
            }

            if (controller == XRControllerRumbleSide.Left || controller == XRControllerRumbleSide.Both)
            {
                OVRInput.SetControllerVibration(1, strength, OVRInput.Controller.LTouch);
                leftRumbleTime = duration;
            }

            return true;
        }
    }
}