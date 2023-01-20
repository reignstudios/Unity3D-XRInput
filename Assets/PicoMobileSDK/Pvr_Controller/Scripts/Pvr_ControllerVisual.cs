// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;


namespace Pvr_UnitySDKAPI
{
    public enum ControllerDevice
    {
        Goblin,
        Neo,
        G2,
        Neo2,
        NewController
    }
}

public class Pvr_ControllerVisual : MonoBehaviour
{    
    public ControllerDevice currentDevice;
    
    public Texture2D m_idle;
    public Texture2D m_app;
    public Texture2D m_home;
    public Texture2D m_touchpad;
    public Texture2D m_volUp;
    public Texture2D m_volDn;
    public Texture2D m_trigger;
    public Texture2D m_a;
    public Texture2D m_b;
    public Texture2D m_x;
    public Texture2D m_y;
    public Texture2D m_grip;
    
    private Renderer controllerRenderMat;

    [HideInInspector]
    public ControllerVariety variety;

    void Awake()
    {
        controllerRenderMat = GetComponent<Renderer>();
    }

    void Start()
    {
        variety = transform.GetComponentInParent<Pvr_ControllerModuleInit>().Variety;
    }

    void Update()
    {
        ChangeKeyEffects(variety == ControllerVariety.Controller0 ? 0 : 1);
    }
   
    private void ChangeKeyEffects(int hand)
    {
        if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.TOUCHPAD))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_touchpad);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_touchpad);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.APP))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_app);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_app);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.HOME))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_home);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_home);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.VOLUMEUP))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_volUp);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_volUp);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.VOLUMEDOWN))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_volDn);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_volDn);
        }
        else if (Controller.UPvr_GetControllerTriggerValue(hand) > 0 || Controller.UPvr_GetKey(hand,Pvr_KeyCode.TRIGGER))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_trigger);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_trigger);
        }
        else if(Controller.UPvr_GetKey(hand, Pvr_KeyCode.X))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_x);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_x);
        }
        else if(Controller.UPvr_GetKey(hand, Pvr_KeyCode.Y))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_y);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_y);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.A))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_a);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_a);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.B))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_b);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_b);
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.Left) || Controller.UPvr_GetKey(hand, Pvr_KeyCode.Right))
        {
            controllerRenderMat.material.SetTexture("_MainTex", m_grip);
            controllerRenderMat.material.SetTexture("_EmissionMap", m_grip);
        }
        else
        {
            if (controllerRenderMat.material.GetTexture("_MainTex") != m_idle)
            {
                controllerRenderMat.material.SetTexture("_MainTex", m_idle);
                controllerRenderMat.material.SetTexture("_EmissionMap", m_idle);
            }
        }
    }
}
