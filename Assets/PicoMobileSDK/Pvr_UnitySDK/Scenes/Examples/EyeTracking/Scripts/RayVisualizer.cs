// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayVisualizer : MonoBehaviour {
    private LineRenderer _line;
    private Pvr_UnitySDKAPI.EyeTrackingGazeRay gazeRay;
    
    void Start ()
    {
        _line = gameObject.GetComponent<LineRenderer>();
        _line.startWidth = 0.002f;
        _line.endWidth = 0.002f;
    }
	
    void Update ()
    {
        var t = Pvr_UnitySDKManager.SDK.HeadPose.Matrix;
        Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingGazeRay(ref gazeRay);
        _line.SetPosition(0, t.MultiplyPoint(new Vector3(0,-0.05f,0.2f)));
        _line.SetPosition(1, gazeRay.Origin + gazeRay.Direction * 20);
    }
}