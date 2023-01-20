// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections.Generic;
using UnityEngine;

public class CLoadingAsset : ScriptableObject
{
    public int SplashScreenType;
    //Splash Image
    public List<Texture2D> splashImage = new List<Texture2D>();
    public Texture2D Inside_background;

    //SplashText
    public bool UseSplashText;
    public string DefaultText;
    public string ChineseText;
    public string EnglishText;
    public string JapaneseText;
    public string KoreanText;
    //other
    public string FontSize;
    public Color FontColor;
    public string TextHeight;
    public bool UseCarousel;
    public int SplashTextAlignment;
}
