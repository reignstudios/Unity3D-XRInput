// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public enum ConfirmBtn
{
    App = 1,
    TouchPad = 2,
    Trigger = 4,
}

public class Pvr_InputModule : PointerInputModule
{
    public static List<Pvr_UIPointer> pointers = new List<Pvr_UIPointer>();

    [Pvr_EnumFlags]
    public ConfirmBtn confirmBtn = ConfirmBtn.TouchPad;

    public virtual void Initialise()
    {
        pointers.Clear();
    }

    public static void AddPoint(Pvr_UIPointer point)
    {
        if (!pointers.Contains(point))
            pointers.Add(point);
    }

    public static void RemovePoint(Pvr_UIPointer point)
    {
        if (pointers.Contains(point))
            pointers.Remove(point);
    }

    public override void Process()
    {
        if (Pvr_UnitySDKManager.SDK.isHasController)
        {
            if ((int)(confirmBtn & ConfirmBtn.App) == 1)
            {
                Pvr_UIPointer.AppBtnValue = Pvr_UnitySDKAPI.Controller.UPvr_GetKey(Pvr_ControllerManager.controllerlink.mainHandID, Pvr_UnitySDKAPI.Pvr_KeyCode.APP);
            }
            if ((int)(confirmBtn & ConfirmBtn.TouchPad) == 2)
            {
                Pvr_UIPointer.TouchBtnValue = Pvr_UnitySDKAPI.Controller.UPvr_GetKey(Pvr_ControllerManager.controllerlink.mainHandID, Pvr_UnitySDKAPI.Pvr_KeyCode.TOUCHPAD);
            }
            if ((int)(confirmBtn & ConfirmBtn.Trigger) == 4)
            {
                Pvr_UIPointer.TriggerBtnValue = Pvr_UnitySDKAPI.Controller.UPvr_GetKey(Pvr_ControllerManager.controllerlink.mainHandID, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER);
            }
        }

        for (int i = 0; i < pointers.Count; i++)
        {
            Pvr_UIPointer pointer = pointers[i];
            if (pointer.gameObject.activeInHierarchy && pointer.enabled)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                if (pointer.PointerActive())
                {
                    results = CheckRaycasts(pointer);
                }

                //Process events
                Hover(pointer, results);
                Click(pointer, results);
                Drag(pointer, results);
            }
        }
    }

    protected virtual List<RaycastResult> CheckRaycasts(Pvr_UIPointer pointer)
    {
        var raycastResult = new RaycastResult();
        raycastResult.worldPosition = pointer.GetOriginPosition();
        raycastResult.worldNormal = pointer.GetOriginForward();

        pointer.pointerEventData.pointerCurrentRaycast = raycastResult;

        List<RaycastResult> raycasts = new List<RaycastResult>();
        eventSystem.RaycastAll(pointer.pointerEventData, raycasts);
        return raycasts;
    }

    protected virtual bool CheckTransformTree(Transform target, Transform source)
    {
        if (target == null)
        {
            return false;
        }

        if (target.Equals(source))
        {
            return true;
        }

        return CheckTransformTree(target.transform.parent, source);
    }
    protected virtual bool NoValidCollision(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        return (results.Count == 0 || !CheckTransformTree(results[0].gameObject.transform, pointer.pointerEventData.pointerEnter.transform));
    }


    protected virtual bool IsHovering(Pvr_UIPointer pointer)
    {
        foreach (var hoveredObject in pointer.pointerEventData.hovered)
        {
            if (pointer.pointerEventData.pointerEnter && hoveredObject && CheckTransformTree(hoveredObject.transform, pointer.pointerEventData.pointerEnter.transform))
            {
                return true;
            }
        }
        return false;
    }

    protected virtual bool ValidElement(GameObject obj)
    {
        var canvasCheck = obj.GetComponentInParent<Pvr_UICanvas>();
        return (canvasCheck && canvasCheck.enabled ? true : false);
    }

    protected virtual void CheckPointerHoverClick(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        if (pointer.hoverDurationTimer > 0f)
        {
            pointer.hoverDurationTimer -= Time.deltaTime;
        }

        if (pointer.canClickOnHover && pointer.hoverDurationTimer <= 0f)
        {
            pointer.canClickOnHover = false;
            ClickOnDown(pointer, results, true);
        }
    }

    protected virtual void Hover(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        if (pointer.pointerEventData.pointerEnter)
        {
            CheckPointerHoverClick(pointer, results);
            if (!ValidElement(pointer.pointerEventData.pointerEnter))
            {
                pointer.pointerEventData.pointerEnter = null;
                return;
            }

            if (NoValidCollision(pointer, results))
            {
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerEnter, pointer.pointerEventData, ExecuteEvents.pointerExitHandler);
                pointer.pointerEventData.hovered.Remove(pointer.pointerEventData.pointerEnter);
                pointer.pointerEventData.pointerEnter = null;
            }
        }
        else
        {
            foreach (var result in results)
            {
                if (!ValidElement(result.gameObject))
                {
                    continue;
                }

                var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.pointerEnterHandler);
                if (target != null)
                {
                    var selectable = target.GetComponent<Selectable>();
                    if (selectable)
                    {
                        var noNavigation = new Navigation();
                        noNavigation.mode = Navigation.Mode.None;
                        selectable.navigation = noNavigation;
                    }

                    pointer.OnUIPointerElementEnter(pointer.SetUIPointerEvent(result, target, pointer.hoveringElement));
                    pointer.hoveringElement = target;
                    pointer.pointerEventData.pointerCurrentRaycast = result;
                    pointer.pointerEventData.pointerEnter = target;
                    pointer.pointerEventData.hovered.Add(pointer.pointerEventData.pointerEnter);
                    break;
                }
                else
                {
                    if (result.gameObject != pointer.hoveringElement)
                    {
                        pointer.OnUIPointerElementEnter(pointer.SetUIPointerEvent(result, result.gameObject, pointer.hoveringElement));
                    }
                    pointer.hoveringElement = result.gameObject;
                }
            }

            if (pointer.hoveringElement && results.Count == 0)
            {
                pointer.OnUIPointerElementExit(pointer.SetUIPointerEvent(new RaycastResult(), null, pointer.hoveringElement));
                pointer.hoveringElement = null;
            }
        }
    }

    protected virtual void Click(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        switch (pointer.clickMethod)
        {
            case Pvr_UIPointer.ClickMethods.ClickOnButtonUp:
                ClickOnUp(pointer, results);
                break;
            case Pvr_UIPointer.ClickMethods.ClickOnButtonDown:
                ClickOnDown(pointer, results);
                break;
        }
    }
    protected virtual void ClickOnUp(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        pointer.pointerEventData.eligibleForClick = pointer.ValidClick(false);

        if (!AttemptClick(pointer))
        {
            IsEligibleClick(pointer, results);
        }
    }

    protected virtual void ClickOnDown(Pvr_UIPointer pointer, List<RaycastResult> results, bool forceClick = false)
    {
        pointer.pointerEventData.eligibleForClick = (forceClick ? true : pointer.ValidClick(true));

        if (IsEligibleClick(pointer, results))
        {
            pointer.pointerEventData.eligibleForClick = false;
            AttemptClick(pointer);
        }
    }
    protected virtual bool IsEligibleClick(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        if (pointer.pointerEventData.eligibleForClick)
        {
            foreach (var result in results)
            {
                if (!ValidElement(result.gameObject))
                {
                    continue;
                }

                var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.pointerDownHandler);
                if (target != null)
                {
                    pointer.pointerEventData.pressPosition = pointer.pointerEventData.position;
                    pointer.pointerEventData.pointerPressRaycast = result;
                    pointer.pointerEventData.pointerPress = target;
                    return true;
                }
            }
        }

        return false;
    }

    protected virtual bool AttemptClick(Pvr_UIPointer pointer)
    {
        if (pointer.pointerEventData.pointerPress)
        {
            if (!ValidElement(pointer.pointerEventData.pointerPress))
            {
                pointer.pointerEventData.pointerPress = null;
                return true;
            }

            if (pointer.pointerEventData.eligibleForClick)
            {
                if (!IsHovering(pointer))
                {
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerUpHandler);
                    pointer.pointerEventData.pointerPress = null;
                }
            }
            else
            {
                pointer.OnUIPointerElementClick(pointer.SetUIPointerEvent(pointer.pointerEventData.pointerPressRaycast, pointer.pointerEventData.pointerPress));
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerUpHandler);
                pointer.pointerEventData.pointerPress = null;
            }
            return true;
        }
        return false;
    }
    protected virtual void Drag(Pvr_UIPointer pointer, List<RaycastResult> results)
    {
        pointer.pointerEventData.dragging = pointer.IsSelectionButtonPressed() && pointer.pointerEventData.delta != Vector2.zero;

        if (pointer.pointerEventData.pointerDrag)
        {
            if (!ValidElement(pointer.pointerEventData.pointerDrag))
            {
                pointer.pointerEventData.pointerDrag = null;
                return;
            }

            if (pointer.pointerEventData.dragging)
            {
                if (IsHovering(pointer))
                {
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.dragHandler);
                }
            }
            else
            {
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.dragHandler);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.endDragHandler);
                foreach (RaycastResult raycast in results)
                {
                    ExecuteEvents.ExecuteHierarchy(raycast.gameObject, pointer.pointerEventData, ExecuteEvents.dropHandler);
                }
                pointer.pointerEventData.pointerDrag = null;
            }
        }
        else if (pointer.pointerEventData.dragging)
        {
            foreach (var result in results)
            {
                if (!ValidElement(result.gameObject))
                {
                    continue;
                }

                ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.initializePotentialDrag);
                ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.beginDragHandler);
                var target = ExecuteEvents.ExecuteHierarchy(result.gameObject, pointer.pointerEventData, ExecuteEvents.dragHandler);
                if (target != null)
                {
                    pointer.pointerEventData.pointerDrag = target;
                    break;
                }
            }
        }

    }

}
