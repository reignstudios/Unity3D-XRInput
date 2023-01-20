// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;


[InitializeOnLoad]
public class Pvr_UnitySDKQualitySettings
{
    static bool xmlUpdate = false;

    public delegate void Change(int Msaa);
    static public Change MSAAChange;

    [InitializeOnLoadMethod]
    static void UnitySDKQualitySettings()    
    {
		PlayerSettings.Android.blitType = AndroidBlitType.Never;
        Pvr_UnitySDKManagerEditor.HeadDofChangedEvent += UpdateXMLHeadDof;

	    Pvr_UnitySDKManagerEditor.MSAAChange += UpdateXMLMsaa;

        Pvr_UnitySDKManagerEditor.SetContentProtectXml += UpdateXMLContentProtect;

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        if (!xmlUpdate)
        {

#if UNITY_5
         UpdateXML("0", "Assets/Plugins/Android/AndroidManifest.xml");
#else
         UpdateXML("1", "Assets/Plugins/Android/AndroidManifest.xml");
#endif
         xmlUpdate = true;
        }
        SetvSyncCount();  
    }

    private static void UpdateXML(string Value,string m_sXmlPath)
    {
        XNamespace android = "http://schemas.android.com/apk/res/android";
        if (File.Exists(m_sXmlPath))
        {
         
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;             
            nodeList = root.SelectNodes("/manifest/application/meta-data");

            foreach (XmlNode node in nodeList)
            {
               
                if (node.Attributes.GetNamedItem("name", android.NamespaceName) != null)   
                {
                    if (node.Attributes.GetNamedItem("name", android.NamespaceName).Value == "platform_high")
                    {
                        if (node.Attributes.GetNamedItem("value", android.NamespaceName).Value == Value)
                        {
                            PLOG.I("platform_high = " + Value.ToString());
                        }
                        else
                        {
                            node.Attributes.GetNamedItem("value", android.NamespaceName).Value = Value;
                            xmlDoc.Save(m_sXmlPath); 
                        }

                        return;
                    }
                } 
            }
            XmlNode applicationNode = xmlDoc.SelectSingleNode("/manifest/application"); 
            XmlElement xmlEle = xmlDoc.CreateElement("meta-data");  
            Debug.Log(android.NamespaceName.ToString());
            xmlEle.SetAttribute( "name", android.NamespaceName,"platform_high");
            xmlEle.SetAttribute( "value", android.NamespaceName, Value);      
            applicationNode.AppendChild(xmlEle);    
            xmlDoc.Save(m_sXmlPath); 
        }
    }


    static void UpdateXMLHeadDof(string dof)
    {
		XNamespace android = "http://schemas.android.com/apk/res/android";
        string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
        if (File.Exists(m_sXmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;

            nodeList = root.SelectNodes("/manifest/application/meta-data");
            foreach (XmlNode node in nodeList)
            {
                if (node.Attributes.GetNamedItem("name", android.NamespaceName) != null)
                {
                    if (node.Attributes.GetNamedItem("name", android.NamespaceName).Value == "com.pvr.hmd.trackingmode")
                    {
                        if (node.Attributes.GetNamedItem("value", android.NamespaceName).Value == dof)
                        {
                            PLOG.I("com.pvr.hmd.trackingmode = " + dof.ToString());
                        }
                        else
                        {
                            node.Attributes.GetNamedItem("value", android.NamespaceName).Value = dof;

                            xmlDoc.Save(m_sXmlPath);
                        }
                        return;
					}
                }
            }
            XmlNode applicationNode = xmlDoc.SelectSingleNode("/manifest/application");
            XmlElement xmlEle = xmlDoc.CreateElement("meta-data");
            xmlEle.SetAttribute("name", android.NamespaceName, "com.pvr.hmd.trackingmode");
            xmlEle.SetAttribute("value", android.NamespaceName, dof);

            Debug.Log(android.NamespaceName.ToString());
            applicationNode.AppendChild(xmlEle);
            xmlDoc.Save(m_sXmlPath);
        }
    }
    static void UpdateXMLMsaa(int MassValue)
    {
      
        XNamespace android = "http://schemas.android.com/apk/res/android";
        string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
        if (File.Exists(m_sXmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;
            nodeList = root.SelectNodes("/manifest/application/meta-data");     
            foreach (XmlNode node in nodeList)
            {
              
                if (node.Attributes.GetNamedItem("name", android.NamespaceName) != null)
                { 
                    if (node.Attributes.GetNamedItem("name", android.NamespaceName).Value == "MSAA")
                    {  
                        if (node.Attributes.GetNamedItem("value", android.NamespaceName).Value == MassValue.ToString())
                        {
                            PLOG.I("MSAA = " + MassValue.ToString());							
                        }
                        else
                        {
                            node.Attributes.GetNamedItem("value", android.NamespaceName).Value = MassValue.ToString();
                            xmlDoc.Save(m_sXmlPath);
                        }
                        return;
					}
                }
            }
            XmlNode applicationNode = xmlDoc.SelectSingleNode("/manifest/application");
            XmlElement xmlEle = xmlDoc.CreateElement("meta-data");
            Debug.Log(android.NamespaceName.ToString());
            xmlEle.SetAttribute("name", android.NamespaceName, "MSAA");
            xmlEle.SetAttribute("value", android.NamespaceName, MassValue.ToString());
            applicationNode.AppendChild(xmlEle);
            xmlDoc.Save(m_sXmlPath);
        }
    }
    static void SetvSyncCount()
    {
        QualitySettings.vSyncCount = 0;
        int currentLevel = QualitySettings.GetQualityLevel();
        for (int i = currentLevel; i >= 1; i--)
        {
            QualitySettings.DecreaseLevel(true);
            QualitySettings.vSyncCount = 0;
        }
        QualitySettings.SetQualityLevel(currentLevel, true);
        for (int i = currentLevel; i < 10; i++)
        {
            QualitySettings.IncreaseLevel(true);
            QualitySettings.vSyncCount = 0;
        }
    }
    static void UpdateXMLContentProtect(string enable_cpt)
    {
        XNamespace android = "http://schemas.android.com/apk/res/android";
        string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
        if (File.Exists(m_sXmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;
            nodeList = root.SelectNodes("/manifest/application/meta-data");
            foreach (XmlNode node in nodeList)
            {

                if (node.Attributes.GetNamedItem("name", android.NamespaceName) != null)
                {
                    if (node.Attributes.GetNamedItem("name", android.NamespaceName).Value == "enable_cpt")
                    {
                        if (node.Attributes.GetNamedItem("value", android.NamespaceName).Value == enable_cpt)
                        {
                            PLOG.I("enable_cpt = " + enable_cpt);
                        }
                        else
                        {
                            node.Attributes.GetNamedItem("value", android.NamespaceName).Value = enable_cpt;
                            xmlDoc.Save(m_sXmlPath);
                        }
                        return;
                    }
                }
            }
            XmlNode applicationNode = xmlDoc.SelectSingleNode("/manifest/application");
            XmlElement xmlEle = xmlDoc.CreateElement("meta-data");
            Debug.Log(android.NamespaceName.ToString());
            xmlEle.SetAttribute("name", android.NamespaceName, "enable_cpt");
            xmlEle.SetAttribute("value", android.NamespaceName, enable_cpt.ToString());
            applicationNode.AppendChild(xmlEle);
            xmlDoc.Save(m_sXmlPath);
        }
    }
}




[InitializeOnLoad]
public class Pvr_UnitySDKBuildSetting : IActiveBuildTargetChanged
{
    void Start()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        SetvSyncCount();
    }
    public int callbackOrder { get { return 0; } }
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        OnChangePlatform();
    }

    static void OnChangePlatform()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            SetvSyncCount();
        }
    }

    static void SetvSyncCount()
    {
        QualitySettings.vSyncCount = 0;
        int currentLevel = QualitySettings.GetQualityLevel();
        for (int i = currentLevel; i >= 1; i--)
        {
            QualitySettings.DecreaseLevel(true);
            QualitySettings.vSyncCount = 0;
        }
        QualitySettings.SetQualityLevel(currentLevel, true);
        for (int i = currentLevel; i < 10; i++)
        {
            QualitySettings.IncreaseLevel(true);
            QualitySettings.vSyncCount = 0;
        }
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
    }
}


