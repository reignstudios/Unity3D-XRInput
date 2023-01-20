// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class InputManager : MonoBehaviour
{
    public delegate void doEnterEventHandler();

    public static doEnterEventHandler doEnter;
    StringBuilder sb = new StringBuilder();
    ArrayList btnsName = new ArrayList();

    private GameObject enter;

    void Awake()
    {
        InitKeyBoard();
    }

    void Start()
    {
    }

    void Update()
    {

    }

    void InitKeyBoard()
    {
        btnsName.Add("1");
        btnsName.Add("2");
        btnsName.Add("3");
        btnsName.Add("4");
        btnsName.Add("5");
        btnsName.Add("6");
        btnsName.Add("7");
        btnsName.Add("8");
        btnsName.Add("9");
        btnsName.Add("0");
        btnsName.Add("Q");
        btnsName.Add("W");
        btnsName.Add("E");
        btnsName.Add("R");
        btnsName.Add("T");
        btnsName.Add("Y");
        btnsName.Add("U");
        btnsName.Add("I");
        btnsName.Add("O");
        btnsName.Add("P");
        btnsName.Add("A");
        btnsName.Add("S");
        btnsName.Add("D");
        btnsName.Add("F");
        btnsName.Add("G");
        btnsName.Add("H");
        btnsName.Add("J");
        btnsName.Add("K");
        btnsName.Add("L");
        btnsName.Add("Z");
        btnsName.Add("X");
        btnsName.Add("C");
        btnsName.Add("V");
        btnsName.Add("B");
        btnsName.Add("N");
        btnsName.Add("M");
        btnsName.Add("Clear");
        btnsName.Add("Capslock");
        btnsName.Add("Enter");


        foreach (string btnName in btnsName)
        {
            GameObject btnObj = GameObject.Find(btnName);
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(delegate() { OnClick(btnObj); });
        }

        enter = GameObject.Find("Enter");
        enter.SetActive(sb.Length > 0);
    }

    void OnClick(GameObject btnObj)
    {
        if (btnObj.name.Equals("Capslock"))
        {
            if (GameObject.Find("Q").transform.GetChild(0).GetComponent<Text>().text.Equals("Q"))
            {
                DoCapslock(true);
            }
            else
            {
                DoCapslock(false);
            }

            return;
        }

        if (btnObj.name.Equals("Clear"))
        {
            GameObject.Find("CodeText").GetComponent<Text>().text = "";
            ClearBuffer();
            return;
        }

        if (btnObj.name.Equals("Enter"))
        {
            ClearBuffer();
            doEnter();
            return;
        }

        EnterChar(btnObj.transform.GetChild(0).GetComponent<Text>().text);
    }

    void EnterChar(string s)
    {
        sb.Append(s);
        GameObject.Find("CodeText").GetComponent<Text>().text = sb.ToString();
        enter.SetActive(sb.Length > 0);
    }

    void DoCapslock(bool IsUpper)
    {
        if (IsUpper)
        {
            foreach (string btnName in btnsName)
            {
                GameObject btnObj = GameObject.Find(btnName);
                Text btnText = btnObj.transform.GetChild(0).GetComponent<Text>();
                btnText.text = btnText.text.ToLower();
            }
        }
        else
        {
            foreach (string btnName in btnsName)
            {
                GameObject btnObj = GameObject.Find(btnName);
                Text btnText = btnObj.transform.GetChild(0).GetComponent<Text>();
                btnText.text = btnText.text.ToUpper();
            }
        }
    }

    void ClearBuffer()
    {
        sb.Remove(0, sb.Length);
        enter.SetActive(sb.Length > 0);
    }
}