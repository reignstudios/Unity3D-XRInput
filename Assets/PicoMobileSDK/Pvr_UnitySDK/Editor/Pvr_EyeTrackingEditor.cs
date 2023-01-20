// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class Pvr_EyeTrackingEditor : Editor, IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        bool EyeTracking = CheckTrackEyes();
        if(EyeTracking)
        {
            UpdateAndroidManifestXML("enable_eyetracking", "1");
        }
        else
        {
            UpdateAndroidManifestXML("enable_eyetracking", "0");
        }
    }

    public static bool CheckTrackEyes()
    {
        bool EyeTracking = false;
        bool buildCurrentScene = false;
        if (CheckTrackEyes(ref buildCurrentScene))
        {
            EyeTracking = true;
        }
        if (buildCurrentScene)
        {
            if(CheckTrackEyesInCur())
            {
                EyeTracking = true;
            }
        }

        return EyeTracking;
    }

    public static bool CheckTrackEyesInCur()
    {
        bool EyeTracking = false;

        Pvr_UnitySDKEyeManager[] array = GameObject.FindObjectsOfType<Pvr_UnitySDKEyeManager>();
        foreach(Pvr_UnitySDKEyeManager manager in array)
        {
            if(manager.EyeTracking)
            {
                EyeTracking = true;
            }
        }

        return EyeTracking;
    }

    public static bool CheckTrackEyes(ref bool buildCurrentScene)
    {
        bool EyeTracking = false;

        EditorBuildSettingsScene[] scenelist = EditorBuildSettings.scenes;
        string[] allScenes = EditorBuildSettingsScene.GetActiveSceneList(scenelist);
        buildCurrentScene = (allScenes.Length == 0);

        foreach (string scenepath in allScenes)
        {
            if(CheckTrackEyesByScene(scenepath))
            {
                EyeTracking = true;
            }
        }
        return EyeTracking;
    }

    public static bool CheckTrackEyesByScene(string path)
    {
        StreamReader sr = new StreamReader(path, Encoding.Default);
        string line;
        string strValue;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.Contains("EyeTracking"))
            {
                if((strValue = sr.ReadLine()) != null)
                {
                    if(strValue.Contains("1"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;

    }

    public static void UpdateAndroidManifestXML(string attributename, string targetvalue)
    {
        string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
        if (File.Exists(m_sXmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;
            nodeList = root.SelectNodes("/manifest/application/meta-data");
            foreach (XmlElement xe in nodeList)
            {
                if (xe.GetAttribute("android:name") == attributename)
                {
                    xe.SetAttribute("android:value", targetvalue);
                    xmlDoc.Save(m_sXmlPath);
                    return;
                }
            }
        }
    }

}
