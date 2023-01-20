// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using Pvr_UnitySDKAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pvr_TouchPadScroll : MonoBehaviour
{
    ScrollRect scrollRect;
    private bool isHoving = false;
    private int mainHand = 0;
    private SwipeDirection direction;
    private Vector2 currPos;
    private Vector2 tarPos;
    private float lerpSpeed = 10f;
    private Transform tranViewport;


    private float ignoreDis = 3f;
    private Vector2 lastTouchDownPos;
    private Vector2 lastTouchUpPos;
    private bool isTouching = false;
    private bool isClosed = true;

    Vector2 vecOrigin = Vector2.zero;
    Vector2 vecOriginNeo2 = new Vector2(128f,128f);

    private void Awake()
    {
        scrollRect = gameObject.GetComponent<ScrollRect>();
        tranViewport = transform.Find("Viewport");
    }

    void Update ()
    {
        Process();
        UpdateTargetPos();
        UpdatePos();
    }

    bool IsControllerConnect()
    {
        mainHand = Controller.UPvr_GetMainHandNess();
        if (mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected)
        {
            return true;
        }
        if (mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected)
        {
            return true;
        }
        return false;
    }

    void UpdateTargetPos()
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD))
        {
            ResetParameter();
                return;
        }

        if (isHoving)
        {            
            if(Pvr_ControllerManager.controllerlink.GetDeviceType() == 4)
            {
                ComputeNeo2Pos();
            }
            else
            {
                ComputePos();
            }

            if ((currPos - tarPos).sqrMagnitude <= 10)
            {
                isClosed = true;
                return;
            }
        }
    }

    void ComputePos()
    {
        mainHand = Controller.UPvr_GetMainHandNess();
        currPos = scrollRect.content.localPosition;
        Vector2 nowTouchPos = vecOrigin;
        if (mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected)
        {
            nowTouchPos = Controller.UPvr_GetTouchPadPosition(mainHand);
        }
        if (mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected)
        {
            nowTouchPos = Controller.UPvr_GetTouchPadPosition(mainHand);
        }

        if ((nowTouchPos - vecOrigin).sqrMagnitude >= 1)//touching
        {
            if (!isTouching)
            {
                lastTouchDownPos = nowTouchPos;
                isTouching = true;
            }
            lastTouchUpPos.x = nowTouchPos.x;
            float value = Mathf.Abs(lastTouchUpPos.x - lastTouchDownPos.x);
            if (value > ignoreDis)
            {
                Vector2 delta = new Vector2((lastTouchUpPos.x - lastTouchDownPos.x) * 10f, 0);
                lastTouchDownPos.x = lastTouchUpPos.x;
                if (isClosed)
                {
                    tarPos = currPos + delta;
                    isClosed = false;
                }
                else
                {
                    tarPos += delta;
                }
            }
        }
        else//not touching
        {
            lastTouchDownPos = vecOrigin;
            lastTouchUpPos = vecOrigin;
            isTouching = false;

            if (scrollRect.horizontalScrollbar.value >= 0.999 || scrollRect.horizontalScrollbar.value <= 0.0001)
            {
                isClosed = true;
                return;
            }
        }
    }

    void ComputeNeo2Pos()
    {
        mainHand = Controller.UPvr_GetMainHandNess();
        currPos = scrollRect.content.localPosition;
        Vector2 nowTouchPos = vecOriginNeo2;
        if (mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected)
        {
            nowTouchPos = Controller.UPvr_GetTouchPadPosition(mainHand);
        }
        if (mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected)
        {
            nowTouchPos = Controller.UPvr_GetTouchPadPosition(mainHand);
        }
        float temp = nowTouchPos.x;
        nowTouchPos.x = nowTouchPos.y;
        nowTouchPos.y = temp;

        if (VerifyNeo2TouchValid(nowTouchPos) && ((nowTouchPos - vecOriginNeo2).sqrMagnitude >= 1))//touching
        {
            if (!isTouching)
            {
                lastTouchDownPos = nowTouchPos;
                isTouching = true;
            }
            lastTouchUpPos.x = nowTouchPos.x;
            float value = Mathf.Abs(lastTouchUpPos.x - lastTouchDownPos.x);
            if (value > ignoreDis)
            {
                Vector2 delta = new Vector2((lastTouchUpPos.x - lastTouchDownPos.x) * 10f, 0);
                lastTouchDownPos.x = lastTouchUpPos.x;
                if (isClosed)
                {
                    tarPos = currPos + delta;
                    isClosed = false;
                }
                else
                {
                    tarPos += delta;
                }
            }
        }
        else//not touching
        {
            lastTouchDownPos = vecOriginNeo2 ;
            lastTouchUpPos = vecOriginNeo2;
            isTouching = false;

            if (scrollRect.horizontalScrollbar.value >= 0.999 || scrollRect.horizontalScrollbar.value <= 0.0001)
            {
                isClosed = true;
                return;
            }
        }
    }

    bool VerifyNeo2TouchValid(Vector2 nowTouchPos)
    {
        if (nowTouchPos.x > vecOriginNeo2.x && nowTouchPos.x > lastTouchDownPos.x)
        {
            return true;
        }

        if (nowTouchPos.x < vecOriginNeo2.x && nowTouchPos.x < lastTouchDownPos.x)
        {
            return true;
        }
        return false;
    }

    void UpdatePos()
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD))
        {
            ResetParameter();
            return;
        }

        if (isClosed || !IsControllerConnect())
            return;

        if(scrollRect.horizontalScrollbar.value >= 0.9999 && tarPos.x <= currPos.x)
        {
            isClosed = true;
            return;
        }

        if (scrollRect.horizontalScrollbar.value <= 0.0001 && tarPos.x >= currPos.x)
        {
            isClosed = true;
            return;
        }

        currPos.x = Mathf.Lerp(currPos.x, tarPos.x, lerpSpeed * Time.deltaTime);
        currPos.y = Mathf.Lerp(currPos.y, tarPos.y, lerpSpeed * Time.deltaTime);
        scrollRect.content.localPosition = currPos;
    }

    void ResetParameter()
    {
        currPos = scrollRect.content.localPosition; ;
        tarPos = scrollRect.content.localPosition; ;
        isClosed = true;
    }

    void Process()
    {
        for (int i = 0; i < Pvr_InputModule.pointers.Count; i++)
        {
            Pvr_UIPointer pointer = Pvr_InputModule.pointers[i];
            if (pointer.gameObject.activeInHierarchy && pointer.enabled)
            {
                isHoving = IsHovering(pointer);
            }
        }
    }

    private bool IsHovering(Pvr_UIPointer pointer)
    {
        if (!IsControllerConnect())
            return false;
        foreach (var hoveredObject in pointer.pointerEventData.hovered)
        {
            if(FindTree(hoveredObject.transform))
            {
                mainHand = Controller.UPvr_GetMainHandNess();
                if (mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected)
                {
                    return true;
                }
                if (mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool FindTree(Transform tran)
    {
        if(tran == transform || tran == tranViewport)
        {
            return true;
        }
        else
        {
            if(tran.IsChildOf(tranViewport))
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
