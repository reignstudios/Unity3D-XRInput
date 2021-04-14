using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRstudios.API
{
	public sealed class InputSystem_Package : XRInputAPI
	{
		public override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			throw new System.NotImplementedException();
		}
	}
}