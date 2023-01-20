// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;

public class Pvr_ControllerEventsExamples : MonoBehaviour {

	void Start () {

	    if (GetComponent<Pvr_UIPointer>() == null)
	    {
	        return;
	    }

	    GetComponent<Pvr_UIPointer>().UIPointerElementEnter += UIPointerElementEnter;
	    GetComponent<Pvr_UIPointer>().UIPointerElementExit += UIPointerElementExit;
	    GetComponent<Pvr_UIPointer>().UIPointerElementClick += UIPointerElementClick;
	    GetComponent<Pvr_UIPointer>().UIPointerElementDragStart += UIPointerElementDragStart;
	    GetComponent<Pvr_UIPointer>().UIPointerElementDragEnd += UIPointerElementDragEnd;
    }

    private void UIPointerElementEnter(object sender,UIPointerEventArgs e)
    {
        PLOG.I("UI Pointer entered" + e.currentTarget.name);
    }
    private void UIPointerElementExit(object sender, UIPointerEventArgs e)
    {
        PLOG.I("UI Pointer exited" + e.currentTarget.name);
    }
    private void UIPointerElementClick(object sender, UIPointerEventArgs e)
    {
        PLOG.I("UI Pointer clicked" + e.currentTarget.name);
    }
    private void UIPointerElementDragStart(object sender, UIPointerEventArgs e)
    {
        PLOG.I("UI Pointer started dragging" + e.currentTarget.name);
    }
    private void UIPointerElementDragEnd(object sender, UIPointerEventArgs e)
    {
        PLOG.I("UI Pointer stopped dragging" + e.currentTarget.name);
    }
}
