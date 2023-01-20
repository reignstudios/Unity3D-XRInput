// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_ControllerPower : MonoBehaviour
{
    [SerializeField]
    private Sprite power1;
    [SerializeField]
    private Sprite power2;
    [SerializeField]
    private Sprite power3;
    [SerializeField]
    private Sprite power4;
    [SerializeField]
    private Sprite power5;

    
    [HideInInspector]
    public ControllerVariety variety;
    [HideInInspector]
    public ControllerDevice currentDevice;

    private Image powerImage;
    private int powerValue;
    
    void Start()
    {
        powerImage = transform.GetComponent<Image>();
        powerValue = -1;
        variety = transform.GetComponentInParent<Pvr_ControllerModuleInit>().Variety;
        currentDevice = transform.GetComponentInParent<Pvr_ControllerVisual>().currentDevice;
    }

    void Update()
    {
        RefreshPower(variety == ControllerVariety.Controller0
            ? 0
            : 1);
    }

    private void RefreshPower(int hand)
    {
        switch (currentDevice)
        {
            case ControllerDevice.Neo2:
                {
                    if (Controller.UPvr_GetControllerPower(hand) > 0 && Controller.UPvr_GetControllerPower(hand) <= 15)
                    {
                        powerImage.sprite = power1;
                        powerImage.color = Color.red;
                    }
                    else if (Controller.UPvr_GetControllerPower(hand) >= 16 &&
                             Controller.UPvr_GetControllerPower(hand) <= 20)
                    {
                        powerImage.sprite = power1;
                        powerImage.color = Color.white;
                    }
                    else if (Controller.UPvr_GetControllerPower(hand) >= 21 &&
                             Controller.UPvr_GetControllerPower(hand) <= 40)
                    {
                        powerImage.sprite = power2;
                        powerImage.color = Color.white;
                    }
                    else if (Controller.UPvr_GetControllerPower(hand) >= 41 &&
                             Controller.UPvr_GetControllerPower(hand) <= 60)
                    {
                        powerImage.sprite = power3;
                        powerImage.color = Color.white;
                    }
                    else if (Controller.UPvr_GetControllerPower(hand) >= 61 &&
                             Controller.UPvr_GetControllerPower(hand) <= 80)
                    {
                        powerImage.sprite = power4;
                        powerImage.color = Color.white;
                    }
                    else if (Controller.UPvr_GetControllerPower(hand) >= 81 &&
                             Controller.UPvr_GetControllerPower(hand) <= 100)
                    {
                        powerImage.sprite = power5;
                        powerImage.color = Color.white;
                    }
                    else
                    {
                        powerImage.sprite = power1;
                        powerImage.color = Color.white;
                    }
                }
                break;
            case ControllerDevice.Neo:
                {
                    if (powerValue == 1)
                    {
                        powerImage.enabled = true;
                    }
                    else
                    {
                        powerImage.enabled = Vector3.Distance(transform.parent.parent.parent.localPosition,
                                                 Pvr_UnitySDKManager.SDK.HeadPose.Position) <= 0.35f;
                    }
                    if (powerValue != Controller.UPvr_GetControllerPower(hand))
                    {
                        switch (Controller.UPvr_GetControllerPower(hand))
                        {
                            case 1:
                                powerImage.sprite = power1;
                                powerImage.color = Color.red;
                                break;
                            case 2:
                                powerImage.sprite = power1;
                                powerImage.color = Color.white;
                                break;
                            case 3:
                                powerImage.sprite = power1;
                                powerImage.color = Color.white;
                                break;
                            case 4:
                                powerImage.sprite = power2;
                                powerImage.color = Color.white;
                                break;
                            case 5:
                                powerImage.sprite = power2;
                                powerImage.color = Color.white;
                                break;
                            case 6:
                                powerImage.sprite = power3;
                                powerImage.color = Color.white;
                                break;
                            case 7:
                                powerImage.sprite = power3;
                                powerImage.color = Color.white;
                                break;
                            case 8:
                                powerImage.sprite = power4;
                                powerImage.color = Color.white;
                                break;
                            case 9:
                                powerImage.sprite = power4;
                                powerImage.color = Color.white;
                                break;
                            case 10:
                                powerImage.sprite = power5;
                                powerImage.color = Color.white;
                                break;
                            default:
                                powerImage.sprite = power1;
                                powerImage.color = Color.white;
                                break;
                        }
                        powerValue = Controller.UPvr_GetControllerPower(hand);
                    }
                }
                break;
            case ControllerDevice.G2:
                {
                    if (Pvr_ControllerManager.controllerlink.controller0Connected)
                    {
                        if (powerValue == 0)
                        {
                            powerImage.enabled = true;
                        }
                        else
                        {
                            powerImage.enabled = Vector3.Distance(transform.parent.parent.parent.localPosition,
                                                     Pvr_UnitySDKManager.SDK.HeadPose.Position) <= 0.35f;
                        }
                        if (powerValue != Controller.UPvr_GetControllerPower(0))
                        {
                            switch (Controller.UPvr_GetControllerPower(0))
                            {
                                case 0:
                                    powerImage.sprite = power1;
                                    powerImage.color = Color.red;
                                    break;
                                case 1:
                                    powerImage.sprite = power2;
                                    powerImage.color = Color.white;
                                    break;
                                case 2:
                                    powerImage.sprite = power3;
                                    powerImage.color = Color.white;
                                    break;
                                case 3:
                                    powerImage.sprite = power4;
                                    powerImage.color = Color.white;
                                    break;
                                case 4:
                                    powerImage.sprite = power5;
                                    powerImage.color = Color.white;
                                    break;
                                default:
                                    powerImage.sprite = power1;
                                    powerImage.color = Color.white;
                                    break;
                            }
                            powerValue = Controller.UPvr_GetControllerPower(0);
                        }
                    }
                }
                break;
        }
    }
}
