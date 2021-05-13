using System;
using UnityEngine;

namespace VRstudios
{
    public class TestInput : MonoBehaviour
    {
		private void Start()
		{
			XRInput.InitializedCallback += XRInput_InitializedCallback;
			XRInput.DisposedCallback += XRInput_DisposedCallback;
			XRInput.ControllerConnectedCallback += XRInput_ControllerConnectedCallback;
			XRInput.ControllerDisconnectedMethod += XRInput_ControllerDisconnectedMethod;
		}

        private void XRInput_InitializedCallback(bool success)
        {
            Debug.Log("CALLBACK: XRInput Initilized!");
        }

        private void XRInput_DisposedCallback()
        {
            Debug.Log("CALLBACK: XRInput Disposed!");
        }

        private void XRInput_ControllerConnectedCallback(Guid id, XRControllerSide side, XRInputControllerType type)
        {
            Debug.Log("CALLBACK: XRInput Controller-Connected: " + side.ToString());
        }

        private void XRInput_ControllerDisconnectedMethod(Guid id, XRControllerSide side, XRInputControllerType type)
		{
            Debug.Log("CALLBACK: XRInput Controller-Disconnected: " + side.ToString());
        }

		private void Update()
        {
            // =====================================
            // left
            // =====================================
            var stateL = XRInput.ControllerState(XRController.Left);

            // touch
            PrintButton(stateL.touchTrigger, "Left - TouchTrigger");
            PrintButton(stateL.touchJoystick, "Left - TouchJoystick");
            PrintButton(stateL.touchJoystick2, "Left - TouchJoystick2");
            PrintButton(stateL.touchGrip, "Left - TouchGrip");
            PrintButton(stateL.touchMenu, "Left - TouchMenu");
            PrintButton(stateL.touch1, "Left - Touch1");
            PrintButton(stateL.touch2, "Left - Touch2");
            PrintButton(stateL.touch3, "Left - Touch3");
            PrintButton(stateL.touch4, "Left - Touch4");

            // buttons
            PrintButton(stateL.buttonTrigger, "Left - ButtonTrigger");
            PrintButton(stateL.buttonJoystick, "Left - ButtonJoystick");
            PrintButton(stateL.buttonJoystick2, "Left - ButtonJoystick2");
            PrintButton(stateL.buttonGrip, "Left - ButtonGrip");
            PrintButton(stateL.buttonMenu, "Left - ButtonMenu");
            PrintButton(stateL.button1, "Left - Button1");
            PrintButton(stateL.button2, "Left - Button2");
            PrintButton(stateL.button3, "Left - Button3");
            PrintButton(stateL.button4, "Left - Button4");

            // triggers
            PrintAnalog(stateL.trigger, "Left - Trigger");

            // grips
            PrintAnalog(stateL.grip, "Left - Grip");

            // joysticks
            PrintJoystick(stateL.joystick, "Left - Joystick");
            PrintJoystick(stateL.joystick2, "Left - Joystick2");

            // rumble
            if (stateL.trigger.value != 0) XRInput.SetRumble(XRControllerRumbleSide.Left, stateL.trigger.value);

            // =====================================
            // right
            // =====================================
            var stateR = XRInput.ControllerState(XRController.Right);

            // touch
            PrintButton(stateR.touchTrigger, "Right - TouchTrigger");
            PrintButton(stateR.touchJoystick, "Right - TouchJoystick");
            PrintButton(stateR.touchJoystick2, "Right - TouchJoystick2");
            PrintButton(stateR.touchGrip, "Right - TouchGrip");
            PrintButton(stateR.touchMenu, "Right - TouchMenu");
            PrintButton(stateR.touch1, "Right - Touch1");
            PrintButton(stateR.touch2, "Right - Touch2");
            PrintButton(stateR.touch3, "Right - Touch3");
            PrintButton(stateR.touch4, "Right - Touch4");

            // buttons
            PrintButton(stateR.buttonTrigger, "Right - ButtonTrigger");
            PrintButton(stateR.buttonJoystick, "Right - ButtonJoystick");
            PrintButton(stateR.buttonJoystick2, "Right - ButtonJoystick2");
            PrintButton(stateR.buttonGrip, "Right - ButtonGrip");
            PrintButton(stateR.buttonMenu, "Right - ButtonMenu");
            PrintButton(stateR.button1, "Right - Button1");
            PrintButton(stateR.button2, "Right - Button2");
            PrintButton(stateR.button3, "Right - Button3");
            PrintButton(stateR.button4, "Right - Button4");

            // triggers
            PrintAnalog(stateR.trigger, "Right - Trigger");

            // grips
            PrintAnalog(stateR.grip, "Right - Grip");

            // joysticks
            PrintJoystick(stateR.joystick, "Right - Joystick");
            PrintJoystick(stateR.joystick2, "Right - Joystick2");

            // rumble
            if (stateR.trigger.value != 0) XRInput.SetRumble(XRControllerRumbleSide.Right, stateR.trigger.value);
        }

        void PrintButton(XRControllerButton button, string name)
        {
            if (button.down) Debug.Log(name + " down");
            if (button.up) Debug.Log(name + " up");
        }

        void PrintAnalog(XRControllerAnalog analog, string name)
        {
           if (analog.value >= .1f) Debug.Log(name + " " + analog.value.ToString());
	    }

        void PrintJoystick(XRControllerJoystick joystick, string name)
        {
            if (joystick.value.magnitude >= .1f) Debug.Log($"{name} {joystick.value.x}x{joystick.value.y}");
        }
    }
}