// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pvr_UICanvas : MonoBehaviour
{
    public bool clickOnPointerCollision = false;

    public float autoActivateWithinDistance = 0f;

    protected BoxCollider canvasBoxCollider;
    protected Rigidbody canvasRigidBody;

    protected Coroutine draggablePanelCreation;
    protected const string CANVAS_DRAGGABLE_PANEL = "UICANVAS_DRAGGABLE_PANEL";

    protected virtual void OnEnable()
    {
        SetupCanvas();
    }

    protected virtual void OnDisable()
    {
        RemoveCanvas();
    }

    protected virtual void OnDestroy()
    {
        RemoveCanvas();
    }

    protected virtual void SetupCanvas()
    {
        var canvas = GetComponent<Canvas>();

        if (!canvas || canvas.renderMode != RenderMode.WorldSpace)
        {
            return;
        }

        var canvasRectTransform = canvas.GetComponent<RectTransform>();
        var canvasSize = canvasRectTransform.sizeDelta;

        var defaultRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
        var customRaycaster = canvas.gameObject.GetComponent<Pvr_UIGraphicRaycaster>();


        if (!customRaycaster)
        {
            customRaycaster = canvas.gameObject.AddComponent<Pvr_UIGraphicRaycaster>();
        }

        if (defaultRaycaster && defaultRaycaster.enabled)
        {
            customRaycaster.ignoreReversedGraphics = defaultRaycaster.ignoreReversedGraphics;
            customRaycaster.blockingObjects = defaultRaycaster.blockingObjects;
            defaultRaycaster.enabled = false;
        }
        if (!canvas.gameObject.GetComponent<BoxCollider>())
        {
            float zSize = 0.1f;
            float zScale = zSize / canvasRectTransform.localScale.z;

            canvasBoxCollider = canvas.gameObject.AddComponent<BoxCollider>();
            canvasBoxCollider.size = new Vector3(canvasSize.x, canvasSize.y, zScale);
            canvasBoxCollider.isTrigger = true;
        }

        if (!canvas.gameObject.GetComponent<Rigidbody>())
        {
            canvasRigidBody = canvas.gameObject.AddComponent<Rigidbody>();
            canvasRigidBody.isKinematic = true;
        }

        draggablePanelCreation = StartCoroutine(CreateDraggablePanel(canvas, canvasSize));
    }

    protected virtual IEnumerator CreateDraggablePanel(Canvas canvas, Vector2 canvasSize)
    {
        if (canvas && !canvas.transform.Find(CANVAS_DRAGGABLE_PANEL))
        {
            yield return null;

            var draggablePanel = new GameObject(CANVAS_DRAGGABLE_PANEL, typeof(RectTransform));
            draggablePanel.AddComponent<LayoutElement>().ignoreLayout = true;
            draggablePanel.AddComponent<Image>().color = Color.clear;
            draggablePanel.AddComponent<EventTrigger>();
            draggablePanel.transform.SetParent(canvas.transform);
            draggablePanel.GetComponent<RectTransform>().sizeDelta = canvasSize;
            draggablePanel.transform.localPosition = Vector3.zero;
            draggablePanel.transform.localRotation = Quaternion.identity;
            draggablePanel.transform.localScale = Vector3.one;
            draggablePanel.transform.SetAsFirstSibling();

        }
    }

    protected virtual void RemoveCanvas()
    {
        var canvas = GetComponent<Canvas>();

        if (!canvas)
        {
            return;
        }

        var defaultRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
        var customRaycaster = canvas.gameObject.GetComponent<Pvr_UIGraphicRaycaster>();
        //if a custom raycaster exists then remove it
        if (customRaycaster)
        {
            Destroy(customRaycaster);
        }

        //If the default raycaster is disabled, then re-enable it
        if (defaultRaycaster && !defaultRaycaster.enabled)
        {
            defaultRaycaster.enabled = true;
        }
        if (canvasBoxCollider)
        {
            Destroy(canvasBoxCollider);
        }

        if (canvasRigidBody)
        {
            Destroy(canvasRigidBody);
        }

        StopCoroutine(draggablePanelCreation);
        var draggablePanel = canvas.transform.Find(CANVAS_DRAGGABLE_PANEL);
        if (draggablePanel)
        {
            Destroy(draggablePanel.gameObject);
        }

    }
}


