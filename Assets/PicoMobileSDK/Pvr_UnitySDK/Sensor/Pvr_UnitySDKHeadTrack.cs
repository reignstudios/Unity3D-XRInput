// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class Pvr_UnitySDKHeadTrack : MonoBehaviour
{
    public bool trackRotation = true;
    public bool trackPosition = true;
    public Transform target;
    private bool updated = false;
    private bool dataClock;
    
    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    void Update()
    {
        updated = false;
        UpdateHead();
    }

    private void UpdateHead()
    {
        if (updated)
        {
            return;
        }
        updated = true;
        if (Pvr_UnitySDKManager.SDK == null)
        {
            return;
        }
        if (trackRotation)
        {
            var rot = Pvr_UnitySDKManager.SDK.HeadPose.Orientation;
            if (target == null)
            {
                transform.localRotation = rot;
            }
            else
            {
                transform.rotation = rot * target.rotation;
            }
        }

        else
        {
            var rot = Pvr_UnitySDKManager.SDK.HeadPose.Orientation;
            if (target == null)
            {
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.rotation = rot * target.rotation;
            }
        }
        if (trackPosition)
        {
            Vector3 pos = Pvr_UnitySDKManager.SDK.HeadPose.Position;
            if (target == null)
            {
                transform.localPosition = pos;
            }
            else
            {
                transform.position = target.position + target.rotation * pos;
            }
        }
    }

}
