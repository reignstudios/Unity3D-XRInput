// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public enum ESplashScreenType
{
    UseUnitySplashScreen,
    UsePicoSplashScreen,
    UseDynamicSplashScreen,
}

public enum ESplashTextAlignment
{
    Left,
    Center,
    Right,
}

public enum EHintType
{
    None,
    Success,
    SplashListNull,
    BackGroundNull,
}


[SerializeField]
public class Pvr_LoadingEditor : EditorWindow
{
    public static Pvr_LoadingEditor myWindow;
    static string windowName = "Splash Screen";

    public static string assetPath = "Assets/PicoMobileSDK/Pvr_UnitySDK/SplashScreen/Config/";
    private string texPath;
    private string strTexPrefix;

    private ESplashScreenType enumSplashType;
    [SerializeField]
    private List<Texture2D> SplashImage = new List<Texture2D>();
    private SerializedObject splashImageSerializedObject;
    private SerializedProperty splashImageProperty;

    private Texture2D texInside_bg;

    private bool toggleUseSplashText;
    private bool foldoutLanguageLocalization;
    private string[] strLanText = new string[5];
    private string[] strLanguage = { "values", "values-zh", "values-en", "values-ja", "values-ko", };

    private int childIndentLevel = 1;
    private int grandChildIndentLevel = 2;

    private string labelFontSize = "16";
    private Color colorSplashText = Color.white;
    private string labelTextHeight = "100";
    private bool toggleUseCarousel = true;
    private ESplashTextAlignment enumAlignment;

    private int textwidth = 400;
    Vector2 scrollpos = Vector2.zero;

    [MenuItem("Pvr_UnitySDK" + "/Splash Screen")]
    static void Init()
    {
        myWindow = (Pvr_LoadingEditor)EditorWindow.GetWindow(typeof(Pvr_LoadingEditor), false, windowName, true);
        myWindow.autoRepaintOnSceneChange = true;
        myWindow.Show(true);
    }   


    protected void OnEnable()
    {
        LoadSavedInfo();

        splashImageSerializedObject = new SerializedObject(this);
        splashImageProperty = splashImageSerializedObject.FindProperty("SplashImage");
    }

    void LoadSavedInfo()
    {
        string assetpath = Pvr_LoadingEditor.assetPath + typeof(CLoadingAsset).ToString() + ".asset";
        if (File.Exists(assetpath))
        {            
            CLoadingAsset asset = AssetDatabase.LoadAssetAtPath<CLoadingAsset>(assetpath);
            enumSplashType = (ESplashScreenType)asset.SplashScreenType;
            SplashImage.Clear();
            int size = asset.splashImage.Count;
            for (int i = 0; i < size; i++)
            {
                SplashImage.Add(asset.splashImage[i]);
            }
            texInside_bg = asset.Inside_background;

            //SplashText
            toggleUseSplashText = asset.UseSplashText;
            strLanText[0] = asset.DefaultText;
            strLanText[1] = asset.ChineseText;
            strLanText[2] = asset.EnglishText;
            strLanText[3] = asset.JapaneseText;
            strLanText[4] = asset.KoreanText;

            //other             
            labelFontSize = asset.FontSize;
            colorSplashText = asset.FontColor;
            labelTextHeight = asset.TextHeight;
            toggleUseCarousel = asset.UseCarousel;
            enumAlignment = (ESplashTextAlignment)asset.SplashTextAlignment;
        }
    }
   
    protected void OnGUI()
    {
        CreateSpace(2);                
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("SplashScreenType", GUILayout.Width(130));
        enumSplashType = (ESplashScreenType)EditorGUILayout.EnumPopup(enumSplashType, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        scrollpos = EditorGUILayout.BeginScrollView(scrollpos);
        EditorImage();
        EditorSplashText();
        EditorOtherTextAttribute();
        EditorButton();
        EditorGUILayout.EndScrollView();
        
    }


    void EditorImage()
    {
        if (enumSplashType == ESplashScreenType.UseUnitySplashScreen || enumSplashType == ESplashScreenType.UsePicoSplashScreen)
        {
            return;
        }
        //serialized list begin
        splashImageSerializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(splashImageProperty, true);
        if (EditorGUI.EndChangeCheck())
        {
            splashImageSerializedObject.ApplyModifiedProperties();
        }
        //serialized list end
        texInside_bg = (Texture2D)EditorGUILayout.ObjectField("Inside_background", texInside_bg, typeof(Texture2D), false);
    }

    #region SplashText
    void EditorSplashText()
    {
        if (enumSplashType == ESplashScreenType.UseUnitySplashScreen || enumSplashType == ESplashScreenType.UsePicoSplashScreen)
        {
            return;
        }

        toggleUseSplashText = EditorToggle("Use Splash Text", toggleUseSplashText, GUILayout.Width(145));
        if (!toggleUseSplashText)
        {
            return;
        }

        strLanText[0] = CreateOneTextArea("Default Text", strLanText[0], childIndentLevel, GUILayout.Width(130));

        foldoutLanguageLocalization = EditorGUILayout.Foldout(foldoutLanguageLocalization, "LanguageLocalization");
        if (foldoutLanguageLocalization)
        {
            strLanText[1] = CreateOneTextArea("中文", strLanText[1], grandChildIndentLevel, GUILayout.Width(115));
            strLanText[2] = CreateOneTextArea("English", strLanText[2], grandChildIndentLevel, GUILayout.Width(115));
            strLanText[3] = CreateOneTextArea("日本語", strLanText[3], grandChildIndentLevel, GUILayout.Width(115));
            strLanText[4] = CreateOneTextArea("한글", strLanText[4], grandChildIndentLevel, GUILayout.Width(115));
        }
    }

    string CreateOneTextArea(string strlabelfield, string text, int indent, params GUILayoutOption[] labelfieldoptions)
    {
        EditorGUI.indentLevel = indent;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(strlabelfield, labelfieldoptions);
        text = EditorGUILayout.TextArea(text);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        return text;
    }

    #endregion

    #region OtherTextAttribute

    void EditorOtherTextAttribute()
    {
        if (enumSplashType == ESplashScreenType.UseUnitySplashScreen || enumSplashType == ESplashScreenType.UsePicoSplashScreen)
        {
            return;
        }

        if (!toggleUseSplashText)
        {
            return;
        }

        EditorGUI.indentLevel = childIndentLevel;
        labelFontSize = EditorGUILayout.TextField("FontSize", labelFontSize, GUILayout.Width(textwidth));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("FontColor", GUILayout.Width(130));
        colorSplashText = EditorGUILayout.ColorField(colorSplashText, GUILayout.Width(textwidth*0.71f));
        EditorGUILayout.EndHorizontal();

        labelTextHeight = EditorGUILayout.TextField("TextHeight", labelTextHeight, GUILayout.Width(textwidth));
        GUIStyle fontstyle = new GUIStyle();
        fontstyle.fontSize = 10;
        if(labelTextHeight == "")
        {
            labelTextHeight = "0";
        }
        EditorGUILayout.LabelField(" In pixels," + labelTextHeight + " at bottom", fontstyle);

        toggleUseCarousel = EditorToggle("Use Carousel", toggleUseCarousel, GUILayout.Width(128));
        EditorGUILayout.LabelField(" If the text is too long, enable the option and the text will scrolling display", fontstyle);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Alignment", GUILayout.Width(130));
        enumAlignment = (ESplashTextAlignment) EditorGUILayout.EnumPopup(enumAlignment, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel = 0;
    }

    bool  EditorToggle(string labelfiled, bool toggle, params GUILayoutOption[] toggleoptions)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(labelfiled, toggleoptions);
        toggle = EditorGUILayout.Toggle(toggle);
        EditorGUILayout.EndHorizontal();
        return toggle;
    }

    void EditorButton()
    {
        CreateSpace(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(100));
        if (GUILayout.Button("OK", GUILayout.Width(200)))
        {
            OnConfirmClick();
        }
        EditorGUILayout.EndHorizontal();
        CreateSpace(5);
    }

    void CreateSpace(int num)
    {
        for(int i=0; i<num; i++)
        {
            EditorGUILayout.Space();
        }
    }
        

    #endregion

    void ShowHintInfo(EHintType etype)
    {
        EditorUtility.ClearProgressBar();
        Pvr_2DLoadingConfigureOkEditor.Init(etype);
    }

    void OnConfirmClick()
    {
        SaveAssetDataBase();
        EditorUtility.DisplayProgressBar("Configuring LoadingInfo", "", 0.3f);
        switch(enumSplashType)
        {
            case ESplashScreenType.UseUnitySplashScreen:
                UpdateAndroidManifestXML("platform_logo", "0");
                break;
            case ESplashScreenType.UsePicoSplashScreen:
                UpdateAndroidManifestXML("platform_logo", "1");
                break;
            case ESplashScreenType.UseDynamicSplashScreen:
                UpdateAndroidManifestXML("platform_logo", "2");
                break;
        }
        if (enumSplashType == ESplashScreenType.UseUnitySplashScreen || enumSplashType == ESplashScreenType.UsePicoSplashScreen)
        {
            ShowHintInfo(EHintType.Success);
            return;
        }

        EditorUtility.DisplayProgressBar("Configuring LoadingInfo", "CopyTexture", 0.5f);
        EHintType etype = UpdateTexFile();
        if(etype == EHintType.SplashListNull || etype == EHintType.BackGroundNull)
        {
            ShowHintInfo(etype);
            return;
        }


        //xml
        EditorUtility.DisplayProgressBar("UpdateXml", "AndroidManifest...", 1/(strLanguage.Length + 1));                
        for(int i=0; i< strLanguage.Length; i++)
        {
            EditorUtility.DisplayProgressBar("UpdateXml", strLanguage[i] + "...", i+2 / (strLanguage.Length + 1));
            if (toggleUseSplashText)//LoadingText
            {
                UpdateOtherXml(strLanguage[i], "strings", "string", "loading", strLanText[i]);
            }
            else
            {
                UpdateOtherXml(strLanguage[i], "strings", "string", "loading", "");
            }

            string strcolor = ColorUtility.ToHtmlStringRGBA(colorSplashText);
            string stralpha = strcolor.Substring(6);
            string strrgb = strcolor.Substring(0, 6);
            strcolor = "#" + stralpha + strrgb;


            UpdateOtherXml(strLanguage[i], "color", "color", "custom", strcolor);//font color
        }
        UpdateAndroidManifestXML("loadingtextsize", labelFontSize); //font size

        //UseCarousel
        int useCarousel = toggleUseCarousel == true ? 1 : 0;
        UpdateAndroidManifestXML("loadingmarquee", useCarousel.ToString());

        //Text Height
        if (labelTextHeight != "")
        {
            UpdateAndroidManifestXML("loadingheight", labelTextHeight);
        }

        switch(enumAlignment)//alignment
        {
            case ESplashTextAlignment.Left:
                UpdateAndroidManifestXML("loadingalign", "left");
                break;
            case ESplashTextAlignment.Center:
                UpdateAndroidManifestXML("loadingalign", "mid");
                break;
            case ESplashTextAlignment.Right:
                UpdateAndroidManifestXML("loadingalign", "right");
                break;
        }

        ShowHintInfo(EHintType.Success);
        return;
    }

    private void SaveAssetDataBase()
    {
        CLoadingAsset asset;
        string assetpath = Pvr_LoadingEditor.assetPath + typeof(CLoadingAsset).ToString() + ".asset";
        if (File.Exists(assetpath))
        {
            asset = AssetDatabase.LoadAssetAtPath<CLoadingAsset>(assetpath);
        }
        else
        {
            asset = new CLoadingAsset();
            ScriptableObjectUtility.CreateAsset<CLoadingAsset>(asset, Pvr_LoadingEditor.assetPath);
        }
        asset.SplashScreenType = (int)enumSplashType;
        //Splash Image
        asset.splashImage.Clear();
        int size = splashImageProperty.arraySize;
        for (int i = 0; i < size; i++)
        {
            Texture2D tex = (Texture2D)splashImageProperty.GetArrayElementAtIndex(i).objectReferenceValue;
            asset.splashImage.Add(tex);
        }
        asset.Inside_background = texInside_bg;

        //SplashText
        asset.UseSplashText = toggleUseSplashText;
        asset.DefaultText = strLanText[0];
        asset.ChineseText = strLanText[1];
        asset.EnglishText = strLanText[2];
        asset.JapaneseText = strLanText[3];
        asset.KoreanText = strLanText[4];
        //other
        asset.FontSize = labelFontSize;
        asset.FontColor = colorSplashText;
        asset.TextHeight = labelTextHeight;
        asset.UseCarousel = toggleUseCarousel;
        asset.SplashTextAlignment = (int)enumAlignment;

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();//must Refresh
    }

    private EHintType UpdateTexFile()
    {
        string animationpath = "Assets/Plugins/Android/assets/LoadingRes/img/";
        string bgpath = "Assets/Plugins/Android/assets/LoadingRes/";
        if(!Directory.Exists(animationpath))
        {
            Directory.CreateDirectory(animationpath);
        }
        DeleteOldFile(animationpath, true);
        DeleteOldFile(bgpath);

        int picid = 0;
        for (int i=0; i<SplashImage.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Configuring LoadingInfo", "CopyTexture..." + i + "/" + (SplashImage.Count + 2), i/(SplashImage.Count + 2));
            string assetpath = AssetDatabase.GetAssetPath(SplashImage[i]);  
            if(!File.Exists(assetpath))
            {
                continue; 
            }
            string strindex = picid <= 9 ? ("0" + picid.ToString()) : picid.ToString();
            string tarpngpath = animationpath + "loading_animation_000" + strindex + ".png";
            File.Copy(assetpath, tarpngpath, true);
            picid++;
        }
        if(picid == 0)
        {
            return EHintType.SplashListNull;
        }

        //Texture Inside_background 
        string assetpathinside = AssetDatabase.GetAssetPath(texInside_bg);
        if (texInside_bg != null && File.Exists(assetpathinside))
        {
            EditorUtility.DisplayProgressBar("Configuring LoadingInfo", "CopyTexture..." + (SplashImage.Count + 1) + "/" + (SplashImage.Count + 2), (SplashImage.Count + 1) / (SplashImage.Count + 2));            
            string tarinsidepath = bgpath + "inside_background_img.png";
            File.Copy(assetpathinside, tarinsidepath, true);
        }
        else
        {
            return EHintType.BackGroundNull;
        }

        return EHintType.None;
    }

    private void DeleteOldFile(string directorypath, bool deletemeta = false)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directorypath);
        if (directoryInfo == null)
        {
            Debug.LogError(directorypath + " path is not find");
            return;
        }
        FileInfo[] files = directoryInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension == ".png")
            {
                file.Delete();
            }
            if (deletemeta && file.Extension == ".meta")
            {
                file.Delete();
            }
        }
    }

    private void UpdateAndroidManifestXML(string attributename, string targetvalue)
    {
        string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
        if (File.Exists(m_sXmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;
            nodeList = root.SelectNodes("/manifest/application/meta-data");
            foreach(XmlElement xe in nodeList)
            {
                if(xe.GetAttribute("android:name") == attributename)
                {
                    xe.SetAttribute("android:value", targetvalue);
                    xmlDoc.Save(m_sXmlPath);
                    return;
                }
            }
        }
    }

    private void UpdateOtherXml(string language, string xmlfilename, string parentnode, string attributename, string tarInnerText)
    {
        string prePath = "Assets/Plugins/Android/res/";
        string xmlpath = prePath + language + "/" + xmlfilename + ".xml";
        if (File.Exists(xmlpath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;
            nodeList = root.SelectNodes("/resources/" + parentnode);//"/manifest/application/meta-data"
            foreach (XmlElement xe in nodeList)
            {
                if (xe.GetAttribute("name") == attributename)
                {
                    xe.InnerText = tarInnerText;
                    xmlDoc.Save(xmlpath);
                    return;
                }
            }
            //not found
            XmlElement eleNew = xmlDoc.CreateElement(parentnode);
            eleNew.SetAttribute("name", attributename);
            eleNew.InnerText = tarInnerText;
            root.AppendChild(eleNew);
            xmlDoc.Save(xmlpath);
            return;
        }
    }

}


public static class ScriptableObjectUtility
{
    public static void CreateAsset<T>(T classdata, string path) where T : ScriptableObject
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + typeof(T).ToString() + ".asset");

        AssetDatabase.CreateAsset(classdata, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

public class Pvr_2DLoadingConfigureOkEditor : EditorWindow
{
    static Pvr_2DLoadingConfigureOkEditor myWindow;
    static EHintType hintType;
    GUIContent guiContent = new GUIContent("Error");

    public static void Init(EHintType htype)
    {
        hintType = htype;
        myWindow = (Pvr_2DLoadingConfigureOkEditor)EditorWindow.GetWindow(typeof(Pvr_2DLoadingConfigureOkEditor), true, "Congratulations", true);
        myWindow.Show(true);
        Rect pos;
        if (Pvr_LoadingEditor.myWindow != null)
        {
            Rect frect = Pvr_LoadingEditor.myWindow.position;
            pos = new Rect(frect.x + 300, frect.y + 200, 200, 140);
        }
        else
        {
            pos = new Rect(700, 400, 200, 140);
        }
        myWindow.position = pos;
    }

    protected void OnGUI()
    {
        for (int i = 0; i < 10; i++)
        {
            EditorGUILayout.Space();
        }
        switch (hintType)
        {
            case EHintType.Success:
                EditorGUILayout.LabelField("      Configuration is completed", GUILayout.Width(200));
                break;
            case EHintType.SplashListNull:

                myWindow.titleContent = guiContent;
                EditorGUILayout.LabelField("      Splash Image can`t be null", GUILayout.Width(200));
                break;
            case EHintType.BackGroundNull:
                myWindow.titleContent = guiContent;

                EditorGUILayout.LabelField("Inside_background can`t be null", GUILayout.Width(200));
                break;

        }

    }
}
