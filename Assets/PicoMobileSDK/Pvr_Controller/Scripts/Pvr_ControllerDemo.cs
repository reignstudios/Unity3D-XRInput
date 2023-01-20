// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;

public class Pvr_ControllerDemo : MonoBehaviour
{
    public GameObject HeadSetController;
    public GameObject controller0;
    public GameObject controller1;

    public GameObject cube;

    private Ray ray;
    private GameObject currentController;

    private Transform lastHit;
    private Transform currentHit;

    [SerializeField]
    private Material normat;
    [SerializeField]
    private Material gazemat;
    [SerializeField]
    private Material clickmat;
    private bool noClick;
    GameObject referenceObj;
    public float rayDefaultLength = 4;
    private bool isHasController = false;
    private bool headcontrolmode = false;
    private RaycastHit hit;

    void Start()
    {
        ray = new Ray();
        hit = new RaycastHit();
        if (Pvr_UnitySDKManager.SDK.isHasController)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerStateChangedEvent += ControllerStateListener;
            Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForGoblin;
            isHasController = true;
#if UNITY_EDITOR
            HeadSetController.SetActive(false);
            currentController = controller1;
            controller1.transform.Find("dot").gameObject.SetActive(true);
            controller1.transform.Find("ray_alpha").gameObject.SetActive(true);
#endif
        }
        referenceObj = new GameObject("ReferenceObj");
    }

    void OnDestroy()
    {
        if (isHasController)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerStateChangedEvent -= ControllerStateListener;
            Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForGoblin;
        }
    }

    void Update()
    {
        if (HeadSetController.activeSelf)
        {
            HeadSetController.transform.parent.localRotation = Quaternion.Euler(Pvr_UnitySDKManager.SDK.HeadPose.Orientation.eulerAngles.x, Pvr_UnitySDKManager.SDK.HeadPose.Orientation.eulerAngles.y, 0);

            ray.direction = HeadSetController.transform.position - HeadSetController.transform.parent.parent.Find("Head").position;
            ray.origin = HeadSetController.transform.parent.parent.Find("Head").position;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (HeadSetController.name == "SightPointer")
                {
                    HeadSetController.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                }

                currentHit = hit.transform;

                if (currentHit != null && lastHit != null && currentHit != lastHit)
                {
                    if (lastHit.GetComponent<Pvr_UIGraphicRaycaster>() && lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                    {
                        lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = false;
                    }
                }
                if (currentHit != null && lastHit != null && currentHit == lastHit)
                {
                    if (currentHit.GetComponent<Pvr_UIGraphicRaycaster>() && !currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                    {
                        currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = true;
                    }
                }

                if (1 << hit.transform.gameObject.layer == LayerMask.GetMask("Water"))
                {
                    if (!noClick)
                        hit.transform.GetComponent<Renderer>().material = gazemat;
                }
                lastHit = hit.transform;
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, Color.red);
#endif
                if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                {
                    HeadSetController.transform.position = hit.point;
                    HeadSetController.transform.position -= (hit.point - HeadSetController.transform.parent.parent.Find("Head").position).normalized * 0.02f;
                    float scale = 0.008f * hit.distance / 4f;
                    Mathf.Clamp(scale, 0.002f, 0.008f);
                    HeadSetController.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
            else
            {
                if (HeadSetController.name == "SightPointer")
                {
                    HeadSetController.transform.localScale = Vector3.zero;
                }
                if (lastHit != null && 1 << lastHit.transform.gameObject.layer == LayerMask.GetMask("Water"))
                {
                    lastHit.transform.GetComponent<Renderer>().material = normat;
                }
                currentHit = null;
                noClick = false;
                if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                {
                    HeadSetController.transform.position = HeadSetController.transform.parent.parent.Find("Head").position + ray.direction.normalized * (0.5f + rayDefaultLength);
                    HeadSetController.transform.localScale = new Vector3(0.008f, 0.008f, 1);
                }
            }

            if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetMouseButtonDown(0))
            {
                if (lastHit != null && 1 << lastHit.transform.gameObject.layer == LayerMask.GetMask("Water") && currentHit != null)
                {
                    lastHit.transform.GetComponent<Renderer>().material = clickmat;
                    noClick = true;
                }
            }
        }
        else
        {
            if (currentController != null)
            {
                ray.direction = currentController.transform.Find("dot").position - currentController.transform.Find("start").position;
                ray.origin = currentController.transform.Find("start").position;

                if (Physics.Raycast(ray, out hit))
                {
                    currentHit = hit.transform;

                    if (currentHit != null && lastHit != null && currentHit != lastHit)
                    {
                        if (lastHit.GetComponent<Pvr_UIGraphicRaycaster>() && lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                        {
                            lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = false;
                        }
                    }
                    if (currentHit != null && lastHit != null && currentHit == lastHit)
                    {
                        if (currentHit.GetComponent<Pvr_UIGraphicRaycaster>() && !currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                        {
                            currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = true;

                        }
                    }
                    if (1 << hit.transform.gameObject.layer == LayerMask.GetMask("Water"))
                    {
                        if (!noClick)
                            hit.transform.GetComponent<Renderer>().material = gazemat;

                        if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD) || Input.GetMouseButtonDown(0))
                        {
                            referenceObj.transform.position = hit.point;

                            disX = hit.transform.position.x - referenceObj.transform.position.x;
                            disY = hit.transform.position.y - referenceObj.transform.position.y;
                            dragObj = hit.transform;
                        }
                        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKey(1, Pvr_KeyCode.TOUCHPAD) || Input.GetMouseButton(0))
                        {
                            if (hit.transform == dragObj.transform)
                            {
                                referenceObj.transform.position = new Vector3(hit.point.x, hit.point.y, hit.transform.position.z);
                                dragObj.position = new Vector3(referenceObj.transform.position.x + disX, referenceObj.transform.position.y + disY, hit.transform.position.z);
                            }
                        }
                    }
                    lastHit = hit.transform;
#if UNITY_EDITOR
                    Debug.DrawLine(ray.origin, hit.point, Color.red);
#endif
                    currentController.transform.Find("dot").position = hit.point;
                    if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                    {
                        float scale = 0.178f * currentController.transform.Find("dot").localPosition.z / 3.3f;
                        Mathf.Clamp(scale, 0.05f, 0.178f);
                        currentController.transform.Find("dot").localScale = new Vector3(scale, scale, 1);
                    }
                }
                else
                {
                    if (lastHit != null && 1 << lastHit.transform.gameObject.layer == LayerMask.GetMask("Water"))
                    {
                        lastHit.transform.GetComponent<Renderer>().material = normat;
                    }
                    currentHit = null;
                    noClick = false;

                    if(Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                    {
                        currentController.transform.Find("dot").localScale = new Vector3(0.178f, 0.178f, 1);
                        currentController.transform.Find("dot").position = currentController.transform.position + currentController.transform.forward.normalized * (0.5f + rayDefaultLength);
                    }
                }
            }

            if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) ||
                Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD) || Input.GetMouseButtonDown(0))
            {
                if (lastHit != null && 1 << lastHit.transform.gameObject.layer == LayerMask.GetMask("Water") && currentHit != null)
                {
                    lastHit.transform.GetComponent<Renderer>().material = clickmat;
                    noClick = true;
                }
            }
        }
    }
    private Transform dragObj;
    float disX, disY, disZ;

    private void ServiceStartSuccess()
    {
        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        else
        {
            HeadSetController.SetActive(true);
        }
        if (Controller.UPvr_GetMainHandNess() == 0)
        {
            currentController = controller0;
        }
        if (Controller.UPvr_GetMainHandNess() == 1)
        {
            currentController = controller1;
        }
    }

    private void ControllerStateListener(string data)
    {

        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        else
        {
            HeadSetController.SetActive(true);
        }

        if (Controller.UPvr_GetMainHandNess() == 0)
        {
            currentController = controller0;
        }
        if (Controller.UPvr_GetMainHandNess() == 1)
        {
            currentController = controller1;
        }
    }

    private void CheckControllerStateForGoblin(string state)
    {
        HeadSetController.SetActive(!Convert.ToBoolean(Convert.ToInt16(state)));
    }

    public void SwitchControlMode()
    {
#if UNITY_EDITOR
        if (headcontrolmode)
        {
            headcontrolmode = false;
            HeadSetController.SetActive(false);
            controller0.SetActive(true);
            controller1.SetActive(true);
        }
        else
        {
            headcontrolmode = true;
            HeadSetController.SetActive(true);
            controller0.SetActive(false);
            controller1.SetActive(false);
        }
#endif
    }

}
