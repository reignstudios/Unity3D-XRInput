// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using UnityEngine.UI;

public class Pvr_VolumePowerBrightness : MonoBehaviour
{
    /************************************      Properties       ***********************************/
    #region Properties
    bool VolEnable = false;
    bool BattEnable = false;

    public Text showResult;
    public Text setVolumnum;
    public Text setBrightnum;

    public string MusicPath;
    #endregion

    void Awake()
    {
        InitBatteryVolClass();
        string gameobjName = this.gameObject.name;
        StartBatteryReceiver(gameobjName);
        StartAudioReceiver(gameobjName);
    }

    void OnDisable()
    {
        if (VolEnable)
        {
            StopAudioReceiver();
        }
        if (BattEnable)
        {
            StopBatteryReceiver();
        }
    }

    /************************************   Public Interfaces   **********************************/
    #region Public Interfaces
    public void GetMaxVolumeNumber()
    {
        int maxVolume = 0;
        maxVolume = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetMaxVolumeNumber();
        showResult.text = "Maximum volume: " + maxVolume.ToString();
    }

    public void GetCurrentVolumeNumber()
    {
        int currVolume = 0;
        currVolume = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetCurrentVolumeNumber();
        showResult.text = "Current volume：" + currVolume.ToString();
    }

    public void VolumeUp()
    {
        bool enable = false;
        enable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_VolumeUp();
        if (!enable)
        {
            PLOG.E("VolumeUp Error");
        }
    }

    public void VolumeDown()
    {
        bool enable = false;
        enable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_VolumeDown();
        if (!enable)
        {
            PLOG.E("VolumeDown Error");
        }
    }

    public void SetVolumeNum()
    {
        bool enable = false;
        System.Random rm = new System.Random();
        int volume = rm.Next(0, 15);
        setVolumnum.text = "Random number：" + volume.ToString();
        enable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetVolumeNum(volume);
        if (!enable)
        {
            PLOG.E("SetVolumeNum Error");
        }
    }

    public void SetBrightness()
    {
        bool enable = false;
        System.Random rm = new System.Random();
        int brightness = rm.Next(0, 255);
        setBrightnum.text = "Random number：" + brightness.ToString();
        enable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetCommonBrightness(brightness);

        if (!enable)
        {
            PLOG.E("SetBrightness Error");
        }
    }

    public void GetCurrentBrightness()
    {
        int lightness = 0;
        lightness = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetCommonBrightness();

        showResult.text = "Current brightness：" + lightness.ToString();
    }

    public bool setAudio(string s)
    {
        PLOG.I(s.ToString());
        // do what you want !
        return true;
    }

    public bool setBattery(string s)
    {
        PLOG.I(s.ToString());
        // do what you want !
        return true;
    }

    #endregion

    /************************************   Private Interfaces  **********************************/
    #region Private Interfaces
    private bool InitBatteryVolClass()
    {
        return Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_InitBatteryVolClass();
    }

    private bool StartBatteryReceiver(string startreceivre)
    {
        BattEnable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StartBatteryReceiver(startreceivre);
        return BattEnable;
    }

    private bool StopBatteryReceiver()
    {
        return Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StopBatteryReceiver();
    }

    private bool StartAudioReceiver(string startreceivre)
    {
        VolEnable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StartAudioReceiver(startreceivre);
        return VolEnable;
    }

    private bool StopAudioReceiver()
    {
        return Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_StopAudioReceiver();
    }

    #endregion
}
