// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Pvr_SDKSetting : EditorWindow
{
    public static Pvr_SDKSetting window;
    public static string assetPath = "Assets/PicoMobileSDK/Pvr_UnitySDK/PicoSDKSetting/Config/";
    GUIContent myTitleContent = new GUIContent();
    static ELanguage elanguage = ELanguage.English;

    const BuildTarget recommended_BuildTarget = BuildTarget.Android;
    const int recommended_vSyncCount = 0;
    const UIOrientation recommended_orientation = UIOrientation.LandscapeLeft;


    public bool toggleBuildTarget = true;
    public bool toggleVSync = true;
    public bool toggleOrientation = true;
    GUIStyle styleApply;

    #region strings
    static string[] strWindowName = { "Pico SDK Setting", "Pico SDK 设置" };
    string strseparate = "______________________________________________________________________________________________________________________________________________";
    string[] strNoticeText = { "Notice: Recommended project settings for Pico SDK", "注意：Pico SDK 推荐项目配置" };
    string[] strBtnChange = { "切换至中文", "Switch to English" };
    string[] strApplied = { "Applied", "已应用" };

    string[] strInformationText = { "Information:", "信息说明" };
    string[] strInfo1Text = { "1 Rotation tag in Androidmainfest: \n " +
                                "  <meta-data android:name=\"pvr.app.type\" android:value=\"vr\" /> \n" +
                                "  <meta-data android:name=\"pvr.display.orientation\" android:value=\"180\" />",
                              "1 Rotation tag in Androidmainfest: \n " +
                                "  <meta-data android:name=\"pvr.app.type\" android:value=\"vr\" /> \n" +
                                "  <meta-data android:name=\"pvr.display.orientation\" android:value=\"180\" />", };
    string[] strInfo2Text = { "2 Player Setting: \n" +
                                "  Default Orientation setting Landscape Left",
                            "2 Player Setting: \n" +
                                "  Default Orientation setting Landscape Left" };
    string[] strInfo3Text = { "3 Get the lastest version of SDK:", "3 获取最新版本的SDK:" };
    string[] strInfoURL = { "http://us-dev.picovr.com/sdk", "http://dev.picovr.com/sdk" };

    string[] strConfigurationText = { "Configuration:", "配置" };
    string[] strConfiguration1Text = { "1 current:             Build Target = {0}\n" +
                                       "   Recommended:  Build Target = {1}\n",
                                        "1 当前:             Build Target = {0}\n" +
                                       "   推荐:             Build Target = {1}\n" };
    string[] strConfiguration2Text = { "2 current:             V Sync Count = {0}\n" +
                                       "   Recommended:  V Sync Count = {1}\n",
                                        "2 当前:             V Sync Count = {0}\n" +
                                       "   推荐:             V Sync Count = {1}\n" };
    string[] strConfiguration3Text = { "3 current:             Orientation = {0}\n" +
                                       "   Recommended:  Orientation = {1}\n",
                                        "3 当前:             Orientation = {0}\n" +
                                       "   推荐:             Orientation = {1}\n" };

    string[] strBtnApply = { "Apply", "应用" };
    string[] strBtnClose = { "Close", "关闭" };

    #endregion


    static Pvr_SDKSetting()
    {
        //EditorApplication.update += Update;
    }

    [MenuItem("Pvr_UnitySDK" + "/Pico SDK Setting")]
    static void Init()
    {
        IsIgnoreWindow();
        ShowSettingWindow();
    }

    static void Update()
    {
        bool allapplied = IsAllApplied();
        bool showWindow = !allapplied;

        if (IsIgnoreWindow())
        {
            showWindow = false;
        }
        if (showWindow)
        {
            ShowSettingWindow();
        }

        EditorApplication.update -= Update;
    }

   public static bool IsIgnoreWindow()
    {
        string path = Pvr_SDKSetting.assetPath + typeof(CPicoSDKSettingAsset).ToString() + ".asset";
        if (File.Exists(path))
        {
            CPicoSDKSettingAsset asset = AssetDatabase.LoadAssetAtPath<CPicoSDKSettingAsset>(path);
            return asset.IgnoreSDKSetting;
        }
        return false;
    }

    static void ShowSettingWindow()
    {
        if (window != null)
            return;
        window = (Pvr_SDKSetting)GetWindow(typeof(Pvr_SDKSetting), true, strWindowName[(int)elanguage], true);
        window.autoRepaintOnSceneChange = true;
        window.minSize = new Vector2(960, 620);
    }

    string GetResourcePath()
    {
        var ms = MonoScript.FromScriptableObject(this);
        var path = AssetDatabase.GetAssetPath(ms);
        path = Path.GetDirectoryName(path);
        return path.Substring(0, path.Length - "Editor".Length) + "Textures/";
    }

    public void OnGUI()
    {
        myTitleContent.text = strWindowName[(int)elanguage];
        if (window != null)
        {
            window.titleContent = myTitleContent;
        }
        ShowNoticeTextandChangeBtn();

        GUIStyle styleSlide = new GUIStyle();
        styleSlide.normal.textColor = Color.white;
        GUILayout.Label(strseparate, styleSlide);

        GUILayout.Label(strInformationText[(int)elanguage]);
        GUILayout.Label(strInfo1Text[(int)elanguage]);
        GUILayout.Label(strInfo2Text[(int)elanguage]);
        GUILayout.Label(strInfo3Text[(int)elanguage]);

        string strURL = strInfoURL[(int)elanguage];
        GUIStyle style = new GUIStyle();
        style.normal.textColor = new Color(0, 122f / 255f, 204f / 255f);
        if (GUILayout.Button("    " + strURL, style, GUILayout.Width(200)))
        {
            Application.OpenURL(strURL);
        }

        GUILayout.Label(strseparate, styleSlide);


        GUILayout.Label(strConfigurationText[(int)elanguage]);


        string strinfo1 = string.Format(strConfiguration1Text[(int)elanguage], EditorUserBuildSettings.activeBuildTarget, recommended_BuildTarget);
        EditorConfigurations(strinfo1, EditorUserBuildSettings.activeBuildTarget == recommended_BuildTarget, ref toggleBuildTarget);

        string strinfo2 = string.Format(strConfiguration2Text[(int)elanguage], QualitySettings.vSyncCount, recommended_vSyncCount);
        EditorConfigurations(strinfo2, QualitySettings.vSyncCount == recommended_vSyncCount, ref toggleVSync);

        string strinfo3 = string.Format(strConfiguration3Text[(int)elanguage], PlayerSettings.defaultInterfaceOrientation, recommended_orientation);
        EditorConfigurations(strinfo3, PlayerSettings.defaultInterfaceOrientation == recommended_orientation, ref toggleOrientation);


        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(200));


        if (IsAllApplied())
        {
            styleApply = new GUIStyle("ObjectPickerBackground");
            styleApply.alignment = TextAnchor.MiddleCenter;
        }
        else
        {
            styleApply = new GUIStyle("LargeButton");
            styleApply.alignment = TextAnchor.MiddleCenter;
        }
        if (GUILayout.Button(strBtnApply[(int)elanguage], styleApply, GUILayout.Width(100), GUILayout.Height(30)))
        {
            EditorApplication.delayCall += OnClickApply;
        }
        styleApply = null;

        EditorGUILayout.LabelField("", GUILayout.Width(200));
        if (GUILayout.Button(strBtnClose[(int)elanguage], GUILayout.Width(100), GUILayout.Height(30)))
        {
            OnClickClose();
        }
        EditorGUILayout.EndHorizontal();


    }

    public  void OnClickApply()
    {
        if (toggleVSync && QualitySettings.vSyncCount != recommended_vSyncCount)
        {
            QualitySettings.vSyncCount = recommended_vSyncCount;
        }

        if (toggleOrientation && PlayerSettings.defaultInterfaceOrientation != recommended_orientation)
        {
            PlayerSettings.defaultInterfaceOrientation = recommended_orientation;
        }

        if (toggleBuildTarget && EditorUserBuildSettings.activeBuildTarget != recommended_BuildTarget)
        {
            Close();
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, recommended_BuildTarget);
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
            ShowSettingWindow();
        }
    }

    void OnClickClose()
    {
        bool allapplied = IsAllApplied();
        if (allapplied)
        {
            Close();
        }
        else
        {
            Pvr_SettingMessageBoxEditor.Init(elanguage);
        }
    }

    public static bool IsAllApplied()
    {
        bool notApplied = (EditorUserBuildSettings.activeBuildTarget != recommended_BuildTarget) ||
                        (QualitySettings.vSyncCount != recommended_vSyncCount) ||
                        (PlayerSettings.defaultInterfaceOrientation != recommended_orientation);

        if (!notApplied)
            return true;
        else
            return false;
    }

    void EditorConfigurations(string strconfiguration, bool torf, ref bool toggle)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(strconfiguration, GUILayout.Width(500));

        GUIStyle styleApplied = new GUIStyle();
        styleApplied.normal.textColor = Color.green;
        if (torf)
        {
            GUILayout.Label(strApplied[(int)elanguage], styleApplied);
        }
        else
        {
            toggle = EditorGUILayout.Toggle(toggle);
        }

        EditorGUILayout.EndHorizontal();
    }

    void ShowLogo()
    {
        var resourcePath = GetResourcePath();
#if !(UNITY_5_0)
        var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "logo.png");
#else
		var logo = Resources.LoadAssetAtPath<Texture2D>(resourcePath + "logo.png");
#endif        
        if (logo)
        {
            var rect = GUILayoutUtility.GetRect(position.width, 150, GUI.skin.box);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
        }
    }

    void ShowNoticeTextandChangeBtn()
    {
        EditorGUILayout.BeginHorizontal();

        GUIStyle styleNoticeText = new GUIStyle();
        styleNoticeText.alignment = TextAnchor.UpperCenter;
        styleNoticeText.fontSize = 20;
        GUILayout.Label(strNoticeText[(int)elanguage], styleNoticeText);

        if (GUILayout.Button(strBtnChange[(int)elanguage], GUILayout.Width(150), GUILayout.Height(30)))
        {
            SwitchLanguage();
        }

        EditorGUILayout.EndHorizontal();
    }

    void SwitchLanguage()
    {
        if (elanguage == ELanguage.Chinese)
            elanguage = ELanguage.English;
        else if (elanguage == ELanguage.English)
            elanguage = ELanguage.Chinese;
    }

}

public enum ELanguage
{
    English,
    Chinese,
}

public class Pvr_SettingMessageBoxEditor : EditorWindow
{
    static Pvr_SettingMessageBoxEditor myWindow;
    static ELanguage elanguage = ELanguage.English;
    static string[] strWindowName = { "Ignore the recommended configuration", "忽略推荐配置" };
    string[] strTipInfo = { "                                   No more prompted \n" +
            "             You can get recommended configuration from  \n" +
            "                            Development documentation.",
             "                               点击\"忽略\"后,不再提示！\n"+
            "                       可从开发者文档中获取推荐配置说明  \n"};

    string[] strBtnIgnore = { "Ignore", "忽略" };
    string[] strBtnCancel = { "Cancel", "取消" };

    public static void Init(ELanguage language)
    {
        elanguage = language;
        myWindow = (Pvr_SettingMessageBoxEditor)GetWindow(typeof(Pvr_SettingMessageBoxEditor), true, strWindowName[(int)language], true);
        myWindow.autoRepaintOnSceneChange = true;
        myWindow.minSize = new Vector2(360, 200);
        myWindow.Show(true);
        Rect pos;
        if (Pvr_SDKSetting.window != null)
        {
            Rect frect = Pvr_SDKSetting.window.position;
            pos = new Rect(frect.x + 300, frect.y + 200, 200, 140);
        }
        else
        {
            pos = new Rect(700, 400, 200, 140);
        }
        myWindow.position = pos;
    }

    void OnGUI()
    {
        for (int i = 0; i < 10; i++)
        {
            EditorGUILayout.Space();
        }
        GUILayout.Label(strTipInfo[(int)elanguage]);

        for (int i = 0; i < 3; i++)
        {
            EditorGUILayout.Space();
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(20));
        if (GUILayout.Button(strBtnIgnore[(int)elanguage], GUILayout.Width(100), GUILayout.Height(30)))
        {
            OnClickIgnore();
        }
        EditorGUILayout.LabelField("", GUILayout.Width(50));
        if (GUILayout.Button(strBtnCancel[(int)elanguage], GUILayout.Width(130), GUILayout.Height(30)))
        {
            OnClickCancel();
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnClickIgnore()
    {
        SaveAssetDataBase();
        Pvr_SDKSetting.window.Close();
        Close();
    }

    private void SaveAssetDataBase()
    {
        CPicoSDKSettingAsset asset;
        string assetpath = Pvr_SDKSetting.assetPath + typeof(CPicoSDKSettingAsset).ToString() + ".asset";
        if (File.Exists(assetpath))
        {
            asset = AssetDatabase.LoadAssetAtPath<CPicoSDKSettingAsset>(assetpath);
        }
        else
        {
            asset = new CPicoSDKSettingAsset();
            ScriptableObjectUtility.CreateAsset<CPicoSDKSettingAsset>(asset, Pvr_SDKSetting.assetPath);
        }
        asset.IgnoreSDKSetting = true;

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();//must Refresh
    }

    void OnClickCancel()
    {
        Close();
    }
}
