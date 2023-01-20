// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using UnityEngine.EventSystems;

public struct UIDraggableItemEventArgs
{
    public GameObject target;
}

public delegate void UIDraggableItemEventHandler(object sender, UIDraggableItemEventArgs e);

public class Pvr_UIDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public bool restrictToDropZone = false;

    public bool restrictToOriginalCanvas = false;

    public float moveOffset = 0.1f;

    [HideInInspector]
    public GameObject validDropZone;

    public event UIDraggableItemEventHandler DraggableItemDropped;

    public event UIDraggableItemEventHandler DraggableItemReset;

    protected RectTransform dragTransform;
    protected Vector3 startPosition;
    protected Quaternion startRotation;
    protected GameObject startDropZone;
    protected Transform startParent;
    protected Canvas startCanvas;
    protected CanvasGroup canvasGroup;
    protected Pvr_InputModule currentInputmodule;

    public virtual void OnDraggableItemDropped(UIDraggableItemEventArgs e)
    {
        if (DraggableItemDropped != null)
        {
            DraggableItemDropped(this, e);
        }
    }

    public virtual void OnDraggableItemReset(UIDraggableItemEventArgs e)
    {
        if (DraggableItemReset != null)
        {
            DraggableItemReset(this, e);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startParent = transform.parent;
        startCanvas = GetComponentInParent<Canvas>();
        canvasGroup.blocksRaycasts = false;

        if (restrictToDropZone)
        {
            startDropZone = GetComponentInParent<Pvr_UIDropZone>().gameObject;
            validDropZone = startDropZone;
        }

        SetDragPosition(eventData);
        Pvr_UIPointer pointer = GetPointer();
        if (pointer != null)
        {
            pointer.OnUIPointerElementDragStart(pointer.SetUIPointerEvent(pointer.pointerEventData.pointerPressRaycast, gameObject));
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDragPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        dragTransform = null;
        transform.position += (transform.forward * moveOffset);
        bool validDragEnd = true;
        if (restrictToDropZone)
        {
            if (validDropZone != null && validDropZone != startDropZone)
            {
                transform.SetParent(validDropZone.transform);
            }
            else
            {
                ResetElement();
                validDragEnd = false;
            }
        }

        Canvas destinationCanvas = (eventData.pointerEnter != null ? eventData.pointerEnter.GetComponentInParent<Canvas>() : null);
        if (restrictToOriginalCanvas)
        {
            if (destinationCanvas != null && destinationCanvas != startCanvas)
            {
                ResetElement();
                validDragEnd = false;
            }
        }

        if (destinationCanvas == null)
        {
            //We've been dropped off of a canvas
            ResetElement();
            validDragEnd = false;
        }

        if (validDragEnd)
        {
            Pvr_UIPointer pointer = GetPointer();
            if (pointer != null)
            {
                pointer.OnUIPointerElementDragEnd(pointer.SetUIPointerEvent(pointer.pointerEventData.pointerPressRaycast, gameObject));
            }
            OnDraggableItemDropped(SetEventPayload(validDropZone));
        }

        validDropZone = null;
        startParent = null;
        startCanvas = null;
    }

    protected virtual void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (restrictToDropZone && GetComponentInParent<Pvr_UIDropZone>() == null)
        {
            enabled = false;
        }
        currentInputmodule = FindObjectOfType<Pvr_InputModule>();
    }

    protected virtual Pvr_UIPointer GetPointer()
    {
        foreach (Pvr_UIPointer t in Pvr_InputModule.pointers)
        {
            if (t.gameObject.activeInHierarchy && t)
            {
                return t;
            }
        }
        return null;
    }

    protected virtual void SetDragPosition(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null && eventData.pointerEnter.transform as RectTransform != null)
        {
            dragTransform = eventData.pointerEnter.transform as RectTransform;
        }

        Vector3 pointerPosition;

        if (dragTransform != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(dragTransform, eventData.position, eventData.pressEventCamera, out pointerPosition))
        {
            transform.position = pointerPosition - (transform.forward * moveOffset);
            transform.rotation = dragTransform.rotation;
        }
    }

    protected virtual void ResetElement()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.SetParent(startParent);
        OnDraggableItemReset(SetEventPayload(startParent.gameObject));
    }

    protected virtual UIDraggableItemEventArgs SetEventPayload(GameObject target)
    {
        UIDraggableItemEventArgs e;
        e.target = target;
        return e;
    }

}
