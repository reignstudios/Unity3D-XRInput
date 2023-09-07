using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

#if !XRINPUT_DISABLE_OCULUSXR
using Oculus.Interaction.Input;
#endif

namespace Reign.XR.Tools
{
    public class XRInputTrackedPoseDriver : TrackedPoseDriver
    {
        /*protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }*/

        protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation, PoseDataFlags poseFlags)
        {
            if (XRInput.singleton.apiType == XRInputAPIType.OculusXR)
            {
                #if !XRINPUT_DISABLE_OCULUSXR
                if (poseSource == TrackedPose.Center)
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
                #else
                base.SetLocalTransform(newPosition, newRotation, poseFlags);
                #endif
            }
            else if (XRInput.singleton.apiType == XRInputAPIType.OpenVR)
            {
                #if UNITY_STANDALONE && !XRINPUT_DISABLE_STEAMVR
                base.SetLocalTransform(newPosition, newRotation, poseFlags);// TODO: use native OpenVR API directly
                #else
                base.SetLocalTransform(newPosition, newRotation, poseFlags);
                #endif
            }
            else if (XRInput.singleton.apiType == XRInputAPIType.OpenVR_Legacy)
            {
                #if UNITY_STANDALONE && !XRINPUT_DISABLE_STEAMVR
                base.SetLocalTransform(newPosition, newRotation, poseFlags);// TODO: use native OpenVR API directly
                #else
                base.SetLocalTransform(newPosition, newRotation, poseFlags);
                #endif
            }
            else if (XRInput.singleton.apiType == XRInputAPIType.Pico2VR)
            {
                /*#if !XRINPUT_DISABLE_PICO2// NOTE: sadly this functionality isn't called when XR isn't init (which Pico2 doesn't use)
                if (poseSource == TrackedPose.Head)
                {
                    if (Pvr_UnitySDKManager.SDK != null && Pvr_UnitySDKManager.SDK.HeadPose != null)
                    {
                        var headPose = Pvr_UnitySDKManager.SDK.HeadPose;
                        base.SetLocalTransform(headPose.Position, headPose.Orientation, poseFlags);
                    }
                    else
                    {
                        base.SetLocalTransform(newPosition, newRotation, poseFlags);
                    }
                }
                else if (poseSource == TrackedPose.RightPose || poseSource == TrackedPose.LeftPose)
                {
                    int deviceIndex;
                    if (poseSource == TrackedPose.RightPose) deviceIndex = 1;
                    else if (poseSource == TrackedPose.LeftPose) deviceIndex = 0;
                    else deviceIndex = 0;

                    Vector3 pos;
                    if ((poseFlags & PoseDataFlags.Position) != 0) pos = Pvr_UnitySDKAPI.Controller.UPvr_GetControllerPOS(deviceIndex);
                    else pos = Vector3.zero;

                    Quaternion rot;
                    if ((poseFlags & PoseDataFlags.Rotation) != 0) rot = Pvr_UnitySDKAPI.Controller.UPvr_GetControllerQUA(deviceIndex);
                    else rot = Quaternion.identity;

                    base.SetLocalTransform(pos, rot, poseFlags);
                }
                else
                {
                    base.SetLocalTransform(newPosition, newRotation, poseFlags);
                }
                #else*/
                base.SetLocalTransform(newPosition, newRotation, poseFlags);
                //#endif
            }
            else
            {
                base.SetLocalTransform(newPosition, newRotation, poseFlags);
            }
        }
    }
}