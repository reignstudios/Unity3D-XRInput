// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;

public class Pvr_TouchVisual : MonoBehaviour {

    [HideInInspector]
    public ControllerDevice currentDevice;
    [HideInInspector]
    public ControllerVariety variety;

    private MeshRenderer touchRenderer;

    void Start () {
        variety = transform.GetComponentInParent<Pvr_ControllerModuleInit>().Variety;
        currentDevice = GetComponentInParent<Pvr_ControllerVisual>().currentDevice;
        touchRenderer = GetComponent<MeshRenderer>();
    }
	
	void Update ()
	{
	    ChangeEffects(variety == ControllerVariety.Controller0 ? 0 : 1);
    }

    private void ChangeEffects(int hand)
    {
        switch (currentDevice)
        {
            case ControllerDevice.Goblin:
            {
                if (Controller.UPvr_IsTouching(0))
                {
                    touchRenderer.enabled = true;
                    gameObject.SetActive(true);
                    transform.localPosition = new Vector3(1.3f - Controller.UPvr_GetTouchPadPosition(hand).y * 0.01f, 0.8f, -1f - Controller.UPvr_GetTouchPadPosition(hand).x * 0.01f);
                }
                else
                {
                    touchRenderer.enabled = false;
                }

            }
                break;
            case ControllerDevice.Neo:
                {
                    if (Controller.UPvr_IsTouching(hand))
                    {
                        touchRenderer.enabled = true;
                        transform.localPosition = new Vector3(1.2f - Controller.UPvr_GetTouchPadPosition(hand).y * 0.01f, 1.3f, -1f - Controller.UPvr_GetTouchPadPosition(hand).x * 0.01f);
                    }
                    else
                    {
                        touchRenderer.enabled = false;
                    }

                }
                break;
            case ControllerDevice.G2:
            {
                if (Controller.UPvr_IsTouching(0))
                {
                    touchRenderer.enabled = true;
                        transform.localPosition = new Vector3(1.3f - Controller.UPvr_GetTouchPadPosition(hand).y * 0.01f, 1.6f, -1.7f - Controller.UPvr_GetTouchPadPosition(hand).x * 0.01f);
                }
                else
                {
                    touchRenderer.enabled = false;
                }

            }
                break;
        }
    }
}
