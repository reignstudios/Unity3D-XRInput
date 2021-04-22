using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using System.Runtime.InteropServices;
using System.Text;

namespace VRstudios.API
{
	public sealed class OpenVR_New : XRInputAPI
	{
		private const uint controllerStateLength = OpenVR.k_unMaxTrackedDeviceCount;
        private CVRSystem system;
        private int leftHand = -1, rightHand = -1;

        private CVRInput input;
        private ulong viveActionSetHandle, viveAction_Button1;
        private VRActiveActionSet_t[] actionSets;

        // NOTE: capacity must match 'propertyText' for equals to work (in this case 256)
        private StringBuilder propertyText = new StringBuilder(256);
        private StringBuilder propertyText_ViveController = new StringBuilder("vive_controller", 256);
        private StringBuilder propertyText_IndexController = new StringBuilder("knuckles", 256);

        private bool CheckGetActionSetHandle(EVRInputError error, string setName, ref ulong handle)
        {
            if (error != EVRInputError.None)
            {
                handle = 0;
                Debug.Log($"OpenVR action-set get failed for '{setName}': {error}");
                return false;
			}
            return true;
		}

        private bool CheckGetActionHandle(EVRInputError error, string name, ref ulong handle)
        {
            if (error != EVRInputError.None)
            {
                handle = 0;
                Debug.Log($"OpenVR action get failed for '{name}': {error}");
                return false;
			}
            return true;
		}

		public override void Init()
		{
			base.Init();

            // make sure OpenVR is init right away
            EVRInitError e = EVRInitError.None;
            system = OpenVR.Init(ref e);
            Debug.Log("OpenVR version: " + system.GetRuntimeVersion());

            // init OpenVR new input system
            input = OpenVR.Input;
            actionSets = new VRActiveActionSet_t[1];
            if (CheckGetActionSetHandle(input.GetActionSetHandle("vive_controller", ref viveActionSetHandle), "vive_controller", ref viveActionSetHandle))
            {
                CheckGetActionHandle(input.GetActionHandle("/user/hand/left/input/grip", ref viveAction_Button1), "grip", ref viveAction_Button1);
			}
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

		public unsafe override bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
		{
			// defaults
            GatherInputDefaults(out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet);
            leftHand = -1;
            rightHand = -1;

            // validate OpenVR is avaliable
            if (system == null || !system.IsInputAvailable() || input.IsUsingLegacyInput()) return false;

            // gather input
           // var ptr = (VRActiveActionSet_t*)viveActionSetHandle;
            actionSets[0].ulActionSet = viveActionSetHandle;
            var error = input.UpdateActionState(actionSets, (uint)Marshal.SizeOf<VRActiveActionSet_t>());
            if (error != EVRInputError.None) Debug.LogError("UpdateActionState: " + error.ToString());

            /*var digitalDataSize = Marshal.SizeOf<InputDigitalActionData_t>();
            var data = new InputDigitalActionData_t();
            error = input.GetDigitalActionData(viveAction_Button1, ref data, (uint)digitalDataSize, 0);
            if (error != EVRInputError.None && error != EVRInputError.NoData) Debug.LogError("GetDigitalActionData: " + error.ToString());*/

            var digitalDataSize = Marshal.SizeOf<InputAnalogActionData_t>();
            var data = new InputAnalogActionData_t();
            error = input.GetAnalogActionData(viveAction_Button1, ref data, (uint)digitalDataSize, 0);
            if (error != EVRInputError.None && error != EVRInputError.NoData) Debug.LogError("GetAnalogActionData: " + error.ToString());

            // finish
            GatherInputFinish(state_controllers, controllerCount, ref leftSet, ref leftSetIndex, ref rightSet, ref rightSetIndex, ref sideToSet);
            return true;
		}
	}
}