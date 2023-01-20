// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;


namespace Pvr_UnitySDKAPI
{
    public enum ControllerVariety
    {
        Controller0,
        Controller1,
    }
}   

public class Pvr_ControllerModuleInit : MonoBehaviour
{
    
    public ControllerVariety Variety;
    public bool IsCustomModel = false;
    [SerializeField]
    private GameObject dot;
    [SerializeField]
    private GameObject rayLine;
    [SerializeField]
    private GameObject controller;
    private int controllerDof = -1;
    private bool moduleState = true;

    void Awake()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
        Pvr_ControllerManager.PvrControllerStateChangedEvent += ControllerStateChanged;
        Pvr_ControllerManager.ChangeMainControllerCallBackEvent += MainControllerIDChanged;

        if(Pvr_ControllerManager.Instance.LengthAdaptiveRay)
        {           
            rayLine = transform.GetComponentInChildren<LineRenderer>(true).gameObject;
#if UNITY_2017_1_OR_NEWER
            rayLine.GetComponent<LineRenderer>().startWidth = 0.003f;
            rayLine.GetComponent<LineRenderer>().endWidth = 0.0015f;
#else
            rayLine.GetComponent<LineRenderer>().SetWidth(0.003f, 0.0015f);
#endif
        }
    }
    void OnDestroy()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
        Pvr_ControllerManager.PvrControllerStateChangedEvent -= ControllerStateChanged;
        Pvr_ControllerManager.ChangeMainControllerCallBackEvent -= MainControllerIDChanged;
    }

    private void ServiceStartSuccess()
    {
        RefreshRay();
    }
    //Controller connection status changes
    private void ControllerStateChanged(string data)
    {
        if (Pvr_ControllerManager.controllerlink.controller0Connected ||
            Pvr_ControllerManager.controllerlink.controller1Connected)
        {
            moduleState = true;
            controller.transform.localScale = Vector3.one;
        }
        RefreshRay();
    }
    //Main Controller ID changes
    private void MainControllerIDChanged(string data)
    {
        RefreshRay();
    }

    private void RefreshRay()
    {
        if (Variety == ControllerVariety.Controller0)
        {
            StartCoroutine(ShowOrHideRay(0));
        }
        if (Variety == ControllerVariety.Controller1)
        {
            StartCoroutine(ShowOrHideRay(1));
        }
    }
    private IEnumerator ShowOrHideRay(int id)
    {
        yield return null;
        yield return null;
        if (moduleState)
        {
            var state = Controller.UPvr_GetMainHandNess() == id && Controller.UPvr_GetControllerState(id) == ControllerState.Connected;
            dot.SetActive(state);
            rayLine.SetActive(state);
        }
    }

    public void ForceHideOrShow(bool state)
    {
        dot.SetActive(state);
        rayLine.SetActive(state);
        controller.transform.localScale = state ? Vector3.one : Vector3.zero;
        moduleState = state;
    }

    public void UpdateRay()
    {
        if (!Pvr_ControllerManager.Instance.LengthAdaptiveRay)
        {
            return;
        }
        bool isupdate = false;
        
        if (Pvr_ControllerManager.controllerlink.controller0Connected || Pvr_ControllerManager.controllerlink.controller1Connected)
        {
            isupdate = true;
        }
        else
        {
            isupdate = false;
        }
        
        if (isupdate && rayLine != null && rayLine.gameObject.activeSelf)
        {
            int type = Controller.UPvr_GetDeviceType();
            if (type == 1)
            {
                rayLine.GetComponent<LineRenderer>().SetPosition(0, transform.TransformPoint(0, 0, 0.058f));
            }
            else
            {
                rayLine.GetComponent<LineRenderer>().SetPosition(0, transform.TransformPoint(0, 0.009f, 0.055f));
            }
            rayLine.GetComponent<LineRenderer>().SetPosition(1, dot.transform.position);
        }
    }
}
