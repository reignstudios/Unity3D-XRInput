using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reign.XR.API
{
    public enum SideToSet
    {
        None,
        Both,
        Left,
        Right
    }

    public abstract class XRInputAPI
    {
        public bool hmdLinearVelocityValid { get; protected set; }
        public Vector3 hmdLinearVelocity { get; protected set; }

        public bool hmdAngularVelocityValid { get; protected set; }
        public Vector3 hmdAngularVelocity { get; protected set; }

        public virtual void Init()
        {
            // do nothing...
		}

        public virtual void Dispose()
        {
            // do nothing...
		}

        public virtual void FixedUpdate()
        {
            // do nothing...
        }

        public virtual void Update()
        {
            // do nothing...
        }

        public virtual void LateUpdate()
        {
            // do nothing...
        }

        protected void GatherInputDefaults(out int state_controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet)
        {
            state_controllerCount = 0;
            leftSet = false;
            leftSetIndex = -1;
            rightSet = false;
            rightSetIndex = -1;
            sideToSet = SideToSet.None;
        }

        public abstract bool GatherInput(XRControllerState[] state_controllers, out int controllerCount, out bool leftSet, out int leftSetIndex, out bool rightSet, out int rightSetIndex, out SideToSet sideToSet);

        protected void GatherInputFinish(XRControllerState[] state_controllers, int controllerCount, ref bool leftSet, ref int leftSetIndex, ref bool rightSet, ref int rightSetIndex, ref SideToSet sideToSet)
        {
            // if left or right not known use controller index as side (also makes sure not to override used index)
            if (!leftSet || !rightSet)
            {
                if (controllerCount == 1)
                {
                    if (!leftSet && !rightSet)
                    {
                        state_controllers[0].side = XRControllerSide.Right;

                        rightSetIndex = 0;
                        rightSet = true;
                        sideToSet = SideToSet.Right;
                    }
                    else
                    {
                        sideToSet = leftSet ? SideToSet.Left : SideToSet.Right;
					}
				}
                else if (controllerCount >= 2)
                {
                    int rightI = 0;
                    int leftI = 1;

                    if (!rightSet && !leftSet)
                    {
                        rightI = 0;
                        leftI = 1;
                    }
                    else if (!rightSet)
                    {
                        leftI = leftSetIndex;
                        rightI = leftI == 0 ? 1 : 0;
                    }
                    else if (!leftSet)
                    {
                        rightI = rightSetIndex;
                        leftI = rightI == 0 ? 1 : 0;
                    }

                    state_controllers[rightI].side = XRControllerSide.Right;
                    state_controllers[leftI].side = XRControllerSide.Left;

                    leftSetIndex = leftI;
                    rightSetIndex = rightI;
                    rightSet = true;
                    leftSet = true;
                    sideToSet = SideToSet.Both;
                }
            }
            else
            {
                sideToSet = SideToSet.Both;
			}
		}

        protected void ResetControllers(XRControllerState[] state_controllers)
        {
            for (int i = 0; i != state_controllers.Length; ++i)
            {
                state_controllers[i].connected = false;
			}
		}

        public virtual bool SetRumble(XRControllerRumbleSide controller, float strength, float duration)
        {
            return false;
        }

        public virtual bool SetHMDRumble(float strength, float duration)
        {
            return false;
        }
    }
}