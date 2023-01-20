// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;

// To use:
// 1. Add a cross hair in front of your VR camera:
//    - Create 3d->Quad and position it at a default distance (0,0,2)
//    - Assign your crosshair material 
//    - Adjust the transform scale for the desired size
// 2. Add this to a gameobject with a GazeInputModule
//    - Drag on the crosshair onto this script
// 3. Call GazeInputModuleCrosshair.DisplayCrosshair = true 
//    to show the crosshair
[RequireComponent(typeof(Pvr_GazeInputModule))]
public class Pvr_GazeInputModuleCrosshair : MonoBehaviour
{
    // To show/hide the crosshair from scripts use:  GazeInputModuleCrosshair.DisplayCrosshair
    public static bool DisplayCrosshair = true;

    [Tooltip("Crosshair GameObject attached to your VR Camera")]
    public Transform Crosshair;

    private Pvr_GazeInputModule gazeInputModule;
    private Vector3 CrosshairOriginalScale;
    private float CrosshairOriginalDistance;

    void Awake()
    {
        gazeInputModule = GetComponent<Pvr_GazeInputModule>();
    }

    void Start()
    {
        // Crosshair initial size and distance
        CrosshairOriginalScale = Crosshair.localScale;
        CrosshairOriginalDistance = Crosshair.localPosition.z;

        // Initially disable crosshair, we'll manage it during Update()
        Crosshair.gameObject.SetActive(false);
    }

    void Update()
    {
        // Show or hide the crosshair
        Crosshair.gameObject.SetActive(DisplayCrosshair);
        if (DisplayCrosshair)
        {
            // Set the crosshair distance close to where the user is looking
            float distance = CrosshairOriginalDistance;
            if (gazeInputModule.CurrentRaycast.isValid)
            {
                distance = gazeInputModule.CurrentRaycast.distance * 0.8f - 0.5f;
            }
            SetCrossHairAtDistance(distance);
        }
    }

    void SetCrossHairAtDistance(float dist)
    {
        // Move the crosshair forward to the new distance
        Vector3 position = Crosshair.localPosition;
        Crosshair.localPosition = new Vector3(position.x, position.y, dist);
        // But keep the crosshair the same perceptable size no matter the distance
        Crosshair.localScale = CrosshairOriginalScale * dist;
    }
}
