using UnityEngine;
using UnityEngine.SpatialTracking;
using Oculus.Interaction.Input;
using UnityEngine.XR;

namespace VRstudios.Tools
{
    public class XRInputTrackedPoseDriver : TrackedPoseDriver
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation, PoseDataFlags poseFlags)
        {
            if (XRInput.singleton.apiType == XRInputAPIType.OculusXR)
            {
                if (poseSource == TrackedPose.Head)
                {
                    var pose = OVRManager.tracker.GetPose();
                    base.SetLocalTransform(pose.position, pose.orientation, poseFlags);
                }
                else if (poseSource == TrackedPose.RightPose || poseSource == TrackedPose.LeftPose)
                {
                    OVRInput.Controller deviceType;
                    if (poseSource == TrackedPose.RightPose) deviceType = OVRInput.Controller.RHand;
                    else if (poseSource == TrackedPose.LeftPose) deviceType = OVRInput.Controller.LHand;
                    else deviceType = OVRInput.Controller.None;

                    Vector3 pos;
                    if ((poseFlags & PoseDataFlags.Position) != 0) pos = OVRInput.GetLocalControllerPosition(deviceType);
                    else pos = Vector3.zero;

                    Quaternion rot;
                    if ((poseFlags & PoseDataFlags.Rotation) != 0) rot = OVRInput.GetLocalControllerRotation(deviceType);
                    else rot = Quaternion.identity;

                    base.SetLocalTransform(pos, rot, poseFlags);
                }
                else
                {
                    base.SetLocalTransform(newPosition, newRotation, poseFlags);
                }
            }
            else if (XRInput.singleton.apiType == XRInputAPIType.OpenVR)
            {
                base.SetLocalTransform(newPosition, newRotation, poseFlags);// TODO
            }
            else if (XRInput.singleton.apiType == XRInputAPIType.OpenVR_Legacy)
            {
                base.SetLocalTransform(newPosition, newRotation, poseFlags);// TODO
            }
            else
            {
                base.SetLocalTransform(newPosition, newRotation, poseFlags);
            }
        }
    }
}