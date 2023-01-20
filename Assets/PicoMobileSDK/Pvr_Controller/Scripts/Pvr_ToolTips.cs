// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_ToolTips : MonoBehaviour
{
    public enum TipBtn
    {
        app,
        touchpad,
        home,
        volup,
        voldown,
        trigger,
        grip
    }
    private ControllerDevice currentDevice;
    private float tipsAlpha;
    public static Pvr_ToolTips tooltips;

    public void ChangeTipsText(TipBtn tip, string key)
    {
        switch (tip)
        {
            case TipBtn.app:
                {
                    transform.Find("apptip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.touchpad:
                {
                    transform.Find("touchtip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.home:
                {
                    transform.Find("hometip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.volup:
                {
                    transform.Find("volup/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.voldown:
                {
                    transform.Find("voldown/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.trigger:
                {
                    transform.Find("triggertip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.grip:
                {
                    transform.Find("grip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
        }
    }

    private void Awake()
    {
        tooltips = transform.GetComponent<Pvr_ToolTips>();
    }

    void Update()
    {
        switch (currentDevice)
        {
            case Pvr_UnitySDKAPI.ControllerDevice.Goblin:
            case Pvr_UnitySDKAPI.ControllerDevice.G2:
                {
                    tipsAlpha = (330 - transform.parent.parent.parent.localRotation.eulerAngles.x) / 45.0f;
                    if (transform.parent.parent.parent.localRotation.eulerAngles.x >= 270 &&
                        transform.parent.parent.parent.localRotation.eulerAngles.x <= 330)
                    {
                        tipsAlpha = Mathf.Max(0.0f, tipsAlpha);
                        tipsAlpha = tipsAlpha > 1.0f ? 1.0f : tipsAlpha;
                    }
                    else
                    {
                        tipsAlpha = 0.0f;
                    }
                    GetComponent<CanvasGroup>().alpha = tipsAlpha;

                }
                break;
            case Pvr_UnitySDKAPI.ControllerDevice.Neo2:
                {
                    tipsAlpha = (330 - transform.parent.parent.parent.localRotation.eulerAngles.x) / 45.0f;
                    if (transform.parent.parent.parent.localRotation.eulerAngles.x >= 270 &&
                        transform.parent.parent.parent.localRotation.eulerAngles.x <= 330)
                    {
                        tipsAlpha = Mathf.Max(0.0f, tipsAlpha);
                        tipsAlpha = tipsAlpha > 1.0f ? 1.0f : tipsAlpha;
                    }
                    else
                    {
                        tipsAlpha = 0.0f;
                    }
                    GetComponent<CanvasGroup>().alpha = tipsAlpha;
                }
                break;
        }

    }

    private void LoadTextFromJson()
    {
        currentDevice = transform.GetComponentInParent<Pvr_ControllerVisual>().currentDevice;
        transform.Find("apptip/btn/Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("apptip");
        transform.Find("touchtip/btn/Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("touchtip");
        transform.Find("hometip/btn/Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("hometip");

        var volup = transform.Find("volup/btn/Text");
        if (volup != null)
            volup.GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("voluptip");
        var voldown = transform.Find("voldown/btn/Text");
        if (voldown != null)
            voldown.GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("voldowntip");
        var trigtip = transform.Find("triggertip/btn/Text");
        if (trigtip != null)
            trigtip.GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("triggertip");
        var griptip = transform.Find("grip/btn/Text");
        if (griptip != null)
            griptip.GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("griptip");
    }

    public static void RefreshTips()
    {
        tooltips.LoadTextFromJson();
    }
}


