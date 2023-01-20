// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class MoveCylinder : MonoBehaviour
{

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    void Start()
    {
        startingPosition = transform.localPosition;
        startingRotation = transform.rotation;
    }


    public void Reset()
    {
        transform.localPosition = startingPosition;
        transform.rotation = startingRotation;
    }
}
