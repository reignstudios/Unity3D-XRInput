// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class HighlightAtGaze : MonoBehaviour
{
    public Color HighlightColor = Color.red;
    public float AnimationTime = 0.1f;

    private Renderer myRenderer;
    private Color originalColor;
    private Color targetColor;
    private Pvr_UnitySDKAPI.EyeTrackingGazeRay gazeRay;


    private void Start()
    {
        myRenderer = GetComponent<Renderer>();
        originalColor = myRenderer.material.color;
        targetColor = originalColor;
    }

    private void Update()
    {
        Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingGazeRay(ref gazeRay);
        Ray ray = new Ray(gazeRay.Origin, gazeRay.Direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == transform.name)
            {
                if(targetColor != HighlightColor)
                    targetColor = HighlightColor;
            }
            else
            {
                if (targetColor != originalColor)
                    targetColor = originalColor;
            }               

        }
        else
        {
            if (targetColor != originalColor)
                targetColor = originalColor;
        }
        
        myRenderer.material.color = Color.Lerp(myRenderer.material.color, targetColor, Time.deltaTime * (1 / AnimationTime));
    }
}