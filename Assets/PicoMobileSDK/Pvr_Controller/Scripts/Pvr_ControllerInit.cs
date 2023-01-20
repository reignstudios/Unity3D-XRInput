// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class Pvr_ControllerInit : MonoBehaviour
{
    private ControllerVariety Variety;
    private bool isCustomModel = false;
    [SerializeField]
    private GameObject goblin;
    [SerializeField]
    private GameObject neo;
    [SerializeField]
    private GameObject g2;
    [SerializeField]
    private GameObject neo2;
    [SerializeField]
    private Material standardMat;
    [SerializeField]
    private Material unlitMat;

    private int controllerType = -1;
    [HideInInspector]
    public bool loadModelSuccess = false;
    private string modelFilePath = "/system/media/PvrRes/controller/";
    private int systemOrUnity = 0;

    private JsonData curControllerData;
    private int curControllerNum = 1;
    private string modelName = "";

    void Awake()
    {
#if ANDROID_DEVICE
        int enumindex = (int)GlobalIntConfigs.iCtrlModelLoadingPri;
        Render.UPvr_GetIntConfig(enumindex, ref systemOrUnity);
#endif
        Variety = transform.GetComponentInParent<Pvr_ControllerModuleInit>().Variety;
        isCustomModel = transform.GetComponentInParent<Pvr_ControllerModuleInit>().IsCustomModel;
        if (!isCustomModel)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerAbilityEvent += CheckControllerStateOfAbility;
            Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForGoblin;
        }

#if UNITY_EDITOR
        neo2.SetActive(true);
#endif
    }
    void OnDestroy()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
        Pvr_ControllerManager.SetControllerAbilityEvent -= CheckControllerStateOfAbility;
        Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForGoblin;
    }

    private void OnApplicationPause(bool pause)
    {
#if ANDROID_DEVICE
        if (pause)
        {
            HideController();
        }
#endif
    }

    private void ServiceStartSuccess()
    {
        var type = Controller.UPvr_GetDeviceType();
        if (controllerType != type && type != 0)
        {
            controllerType = type;
        }

        LoadResFromJson();

        if (Pvr_ControllerManager.controllerlink.neoserviceStarted)
        {
            if (Variety == ControllerVariety.Controller0)
            {
                if (Pvr_ControllerManager.controllerlink.controller0Connected)
                {
                    StartCoroutine(RefreshController(0));
                }
                else
                {
                    HideController();
                }
            }
            if (Variety == ControllerVariety.Controller1)
            {
                if (Pvr_ControllerManager.controllerlink.controller1Connected)
                {
                    StartCoroutine(RefreshController(1));
                }
                else
                {
                    HideController();
                }
            }
        }
        if (Pvr_ControllerManager.controllerlink.goblinserviceStarted)
        {
            if (Variety == ControllerVariety.Controller0)
            {
                if (Pvr_ControllerManager.controllerlink.controller0Connected)
                {
                    StartCoroutine(RefreshController(0));
                }
                else
                {
                    HideController();
                }
            }
        }
    }

    private void LoadResFromJson()
    {
        string json = Pvr_UnitySDKAPI.System.UPvr_GetObjectOrArray("config.controller",
            (int)Pvr_UnitySDKAPI.ResUtilsType.TYPR_OBJECTARRAY);
        if (json != null)
        {
            JsonData jdata = JsonMapper.ToObject(json);
            if (controllerType >= 0)
            {
                curControllerData = jdata[controllerType - 1];
                modelFilePath = (string)curControllerData["base_path"];
                modelName = (string)curControllerData["model_name"] + "_sys";
            }
        }
        else
        {
            PLOG.E("PvrLog LoadJsonFromSystem Error");
        }

    }

    private void CheckControllerStateForGoblin(string state)
    {
        var type = Controller.UPvr_GetDeviceType();
        if (Variety == ControllerVariety.Controller0)
        {
            if (Convert.ToInt16(state) == 1)
            {
                if (controllerType != type)
                {
                    DestroyController();
                    controllerType = type;
                }
                StartCoroutine(RefreshController(0));
            }
            else
            {
                HideController();
            }
        }
    }

    private void CheckControllerStateOfAbility(string data)
    {
        var state = Convert.ToBoolean(Convert.ToInt16(data.Substring(4, 1)));
        var id = Convert.ToInt16(data.Substring(0, 1));
        var type = Controller.UPvr_GetDeviceType();
        if (state)
        {
            controllerType = type;
            switch (id)
            {
                case 0:
                    if (Variety == ControllerVariety.Controller0)
                    {
                        StartCoroutine(RefreshController(0));
                    }
                    break;
                case 1:
                    if (Variety == ControllerVariety.Controller1)
                    {
                        StartCoroutine(RefreshController(1));
                    }
                    break;
            }
        }
        else
        {
            switch (id)
            {
                case 0:
                    if (Variety == ControllerVariety.Controller0)
                    {
                        HideController();
                    }
                    break;
                case 1:
                    if (Variety == ControllerVariety.Controller1)
                    {
                        HideController();
                    }
                    break;
            }
        }
    }

    private void HideController()
    {
        foreach (Transform t in transform)
        {
            if (t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(false);
                loadModelSuccess = false;
            }
        }
    }

    private void DestroyController()
    {
        foreach (Transform t in transform)
        {
            if (t.name == modelName)
            {
                Destroy(t.gameObject);
                loadModelSuccess = false;
            }
        }
    }

    private IEnumerator RefreshController(int id)
    {
        yield return null;
        yield return null;

        if (Controller.UPvr_GetControllerState(id) == ControllerState.Connected)
        {
            if (systemOrUnity == 0)
            {
                LoadControllerFromPrefab();
                if (!loadModelSuccess)
                {
                    LoadControllerFromSystem(id);
                }
            }
            else
            {
                var isControllerExist = false;
                foreach (Transform t in transform)
                {
                    if (t.name == modelName)
                    {
                        isControllerExist = true;
                    }
                }
                if (!isControllerExist)
                {
                    LoadControllerFromSystem(id);
                    if (!loadModelSuccess)
                    {
                        LoadControllerFromPrefab();
                    }
                }
                else
                {
                    var currentController = transform.Find(modelName);
                    currentController.gameObject.SetActive(true);
                }
            }
            Pvr_ToolTips.RefreshTips();
            PLOG.I("PvrLog Controller Refresh Success");
        }
    }

    private void LoadControllerFromPrefab()
    {
        switch (controllerType)
        {
            case 1:
                goblin.SetActive(true);
                neo.SetActive(false);
                g2.SetActive(false);
                neo2.SetActive(false);
                loadModelSuccess = true;
                break;
            case 2:
                goblin.SetActive(false);
                neo.SetActive(true);
                g2.SetActive(false);
                neo2.SetActive(false);
                loadModelSuccess = true;
                break;
            case 3:
                goblin.SetActive(false);
                neo.SetActive(false);
                g2.SetActive(true);
                neo2.SetActive(false);
                loadModelSuccess = true;
                break;
            case 4:
                goblin.SetActive(false);
                neo.SetActive(false);
                g2.SetActive(false);
                neo2.SetActive(true);
                loadModelSuccess = true;
                break;
            default:
                goblin.SetActive(false);
                neo.SetActive(false);
                g2.SetActive(false);
                neo2.SetActive(false);
                loadModelSuccess = false;
                break;
        }
    }

    private void LoadControllerFromSystem(int id)
    {
        var syscontrollername = controllerType.ToString() + id.ToString() + ".obj";
        var fullFilePath = modelFilePath + syscontrollername;

        if (!File.Exists(fullFilePath))
        {
            PLOG.E("PvrLog Obj File does not exist==" + fullFilePath);
        }
        else
        {
            GameObject go = new GameObject();
            go.name = modelName;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = Pvr_ObjImporter.Instance.ImportFile(fullFilePath);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            int matID = (int)curControllerData["material_type"];
            meshRenderer.material = matID == 0 ? standardMat : unlitMat;

            loadModelSuccess = true;
            Pvr_ControllerVisual controllerVisual = go.AddComponent<Pvr_ControllerVisual>();
            switch (controllerType)
            {
                case 1:
                    {
                        controllerVisual.currentDevice = ControllerDevice.Goblin;
                    }
                    break;
                case 2:
                    {
                        controllerVisual.currentDevice = ControllerDevice.Neo;
                    }
                    break;
                case 3:
                    {
                        controllerVisual.currentDevice = ControllerDevice.G2;
                    }
                    break;
                case 4:
                    {
                        controllerVisual.currentDevice = ControllerDevice.Neo2;
                    }
                    break;
                default:
                    controllerVisual.currentDevice = ControllerDevice.NewController;
                    break;
            }

            controllerVisual.variety = Variety;
            LoadTextureFromSystem(controllerVisual, controllerType.ToString() + id.ToString());
            go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            go.transform.localScale = new Vector3(-0.01f, 0.01f, 0.01f);
        }
    }


    private void LoadTextureFromSystem(Pvr_ControllerVisual visual, string controllerName)
    {
        string texFormat = (string)curControllerData["tex_format"];

        var texturepath = modelFilePath + controllerName + "_idle." + texFormat;
        visual.m_idle = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_app." + texFormat;
        visual.m_app = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_home." + texFormat;
        visual.m_home = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_touch." + texFormat;
        visual.m_touchpad = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_volume_down." + texFormat;
        visual.m_volDn = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_volume_up." + texFormat;
        visual.m_volUp = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_trigger." + texFormat;
        visual.m_trigger = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_a." + texFormat;
        visual.m_a = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_b." + texFormat;
        visual.m_b = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_x." + texFormat;
        visual.m_x = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_y." + texFormat;
        visual.m_y = LoadOneTexture(texturepath);
        texturepath = modelFilePath + controllerName + "_grip." + texFormat;
        visual.m_grip = LoadOneTexture(texturepath);
    }

    private Texture2D LoadOneTexture(string filepath)
    {
        int t_w = (int)curControllerData["tex_width"];
        int t_h = (int)curControllerData["tex_height"];
        var m_tex = new Texture2D(t_w, t_h);
        m_tex.LoadImage(ReadPNG(filepath));
        return m_tex;
    }

    private byte[] ReadPNG(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] binary = new byte[fileStream.Length];
        fileStream.Read(binary, 0, (int)fileStream.Length);
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        return binary;
    }
}
