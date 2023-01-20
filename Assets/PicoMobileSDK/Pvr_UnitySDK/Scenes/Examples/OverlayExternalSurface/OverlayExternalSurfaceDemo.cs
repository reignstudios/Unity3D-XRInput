// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayExternalSurfaceDemo : MonoBehaviour {

    public string movieName;

    public Pvr_UnitySDKEyeOverlay.OverlayType overlayType;
    public Pvr_UnitySDKEyeOverlay.OverlayShape overlayShape;

    private Pvr_UnitySDKEyeOverlay overlay = null;

    private const string TAG = "[ExternalSurface]>>>>>>";


    private void Awake()
    {
        this.overlay = GetComponent<Pvr_UnitySDKEyeOverlay>();
        if (this.overlay == null)
        {
            Debug.LogError(TAG + "Overlay is null!");
            this.overlay = gameObject.AddComponent<Pvr_UnitySDKEyeOverlay>();
        }

        this.overlay.overlayType = overlayType;
        this.overlay.overlayShape = overlayShape;
        this.overlay.isExternalAndroidSurface = true;
    }

    // Use this for initialization
    void Start()
    {
        if (!string.IsNullOrEmpty(movieName))
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            StartPlay(Application.streamingAssetsPath + "/" + movieName, null);
#endif
        }
    }


    void StartPlay(string moviePath, string licenceUrl)
    {
        if (moviePath != string.Empty)
        {
            if (overlay.isExternalAndroidSurface)
            {
                Pvr_UnitySDKEyeOverlay.ExternalAndroidSurfaceObjectCreated surfaceObjectCreatedCallback = () =>
                {
                    Debug.Log(TAG + "SurfaceObject created callback is Invoke().");
                    // TODO:
                    // You need pass externalAndroidSurfaceObject to one android video player for video texture updates.
                    // eg.if you use Android ExoPlayer,you can call exoPlayer.setVideoSurface( surface );
                };

                if (overlay.externalAndroidSurfaceObject == System.IntPtr.Zero)
                {
                    Debug.Log(TAG + "Register surfaceObject crreated callback");
                    overlay.externalAndroidSurfaceObjectCreated = surfaceObjectCreatedCallback;
                }
                else
                {
                    Debug.Log(TAG + "SurfaceObject is already created! Invoke callback");
                    surfaceObjectCreatedCallback.Invoke();
                }
            }
        }
        else
        {
            Debug.LogError(TAG + "Movie path is null!");
        }
    }
}
