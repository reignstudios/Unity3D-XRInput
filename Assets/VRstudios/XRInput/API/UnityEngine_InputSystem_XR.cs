using System.Collections;
using System.Collections.Generic;
using Unity.XR.Oculus.Input;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.XR.OpenXR.Features.Interactions.HTCViveControllerProfile;

namespace VRstudios.API
{
	public sealed class UnityEngine_InputSystem_XR : XRInputAPI
	{
		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			throw new KeyNotFoundException();

			/*// defaults
			GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);
			
			// 
			var leftHand = XRControllerWithRumble.leftHand;
			if (leftHand != null)
			{
				//Debug.Log(leftHand.displayName);
				//Debug.Log(leftHand.GetType().ToString());
				
				//var t = leftHand as OculusTouchController;
				var t = leftHand as ViveController;
				//var t = leftHand as XRControllerWithRumble;
				//Debug.Log((leftHand != null).ToString());
				t.SendImpulse(1, 10000);
				if (t.gripPressed.isPressed)
				{
					Debug.Log("yahoo");
				}
			}

			// finish
			return false;*/
		}
	}
}