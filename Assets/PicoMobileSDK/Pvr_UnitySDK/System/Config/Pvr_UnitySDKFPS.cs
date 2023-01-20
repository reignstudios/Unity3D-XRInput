// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine; 
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Pvr_UnitySDKFPS : MonoBehaviour
{
    public Text fpsText;

    private float fps = 60;

    void Start()
    {
        fps = 60;
    }

    void Update()
    {
        if (fpsText != null)
        {
            fpsText.text = "FPS:" + ShowFps();
        }
    }

    public string ShowFps()
    {
        float interp = Time.deltaTime / (0.5f + Time.deltaTime);
        if (float.IsNaN(interp) || float.IsInfinity(interp))
        {
            interp = 0;
        }
        float currentFPS = 1.0f / Time.deltaTime;
        if (float.IsNaN(currentFPS) || float.IsInfinity(currentFPS))
        {
            currentFPS = 0;
        }
        if (float.IsNaN(fps) || float.IsInfinity(fps))
        {
            fps = 0;
        }
        fps = Mathf.Lerp(fps, currentFPS, interp);

        return (Mathf.RoundToInt(fps) + "fps");
    }
}
