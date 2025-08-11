using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnhancedDragAndDropManipulator : PointerManipulator
{
    private VisualElement lastDroppable = null;
    private string droppableId = "droppable";

    public EnhancedDragAndDropManipulator(VisualElement target)
    {
        this.target = target;
        root = target;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    private Vector2 targetStartPosition { get; set; }
    private Vector3 pointerStartPosition { get; set; }
    private bool enabled { get; set; }
    private VisualElement root { get; }
    private VisualElement parentsRoot { get; }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        targetStartPosition = target.transform.position;
        pointerStartPosition = evt.position;
        target.CapturePointer(evt.pointerId);
        target.AddToClassList("draggable--dragging");
        enabled = true;
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;

            target.transform.position = new Vector2(
                Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));

            if (CanDrop(evt.position, out var droppable))
            {
                target.AddToClassList("draggable--can-drop");
                droppable.AddToClassList("droppable--can-drop");
                if (lastDroppable != droppable)
                    lastDroppable?.RemoveFromClassList("droppable--can-drop");
                lastDroppable = droppable;
            }
            else
            {
                target.RemoveFromClassList("draggable--can-drop");
                lastDroppable?.RemoveFromClassList("droppable--can-drop");
                lastDroppable = null;
            }
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
            target.RemoveFromClassList("draggable--dragging");
        }
    }


    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        if (enabled)
        {
            VisualElement slotsContainer = parentsRoot.Q<VisualElement>("bag-items");
            UQueryBuilder<VisualElement> allSlots = slotsContainer.Query<VisualElement>(className: "slot-container");
            UQueryBuilder<VisualElement> overlappingSlots = allSlots.Where(OverlapsTarget);
            VisualElement closestOverlappingSlot = FindClosestSlot(overlappingSlots);
            Vector3 closestPos = Vector3.zero;
            if (closestOverlappingSlot != null)
            {
                closestPos = RootSpaceOfSlot(closestOverlappingSlot);
                closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);
            }
            target.transform.position = closestOverlappingSlot != null ? closestPos : targetStartPosition;
            if (closestOverlappingSlot != null)
            {
                EnhancedDragAndDropDropEvent e = EnhancedDragAndDropDropEvent.GetPooled(this.target, closestOverlappingSlot);
                this.target.SendEvent(e);
                //EnhancedDragAndDropDropEvent e = EnhancedDragAndDropDropEvent.GetPooled();
                //  e.target = this.target;
                //e.currentTarget = closestOverlappingSlot;
                //  this.target.SendEvent(e);
            }
            enabled = false;
        }
    }

    private bool OverlapsTarget(VisualElement slot)
    {
        return target.worldBound.Overlaps(slot.worldBound);
    }

    private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
    {
        List<VisualElement> slotsList = slots.ToList();
        float bestDistanceSq = float.MaxValue;
        VisualElement closest = null;
        foreach (VisualElement slot in slotsList)
        {
            Vector3 displacement = RootSpaceOfSlot(slot) - target.transform.position;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }

    private Vector3 RootSpaceOfSlot(VisualElement slot)
    {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return root.WorldToLocal(slotWorldSpace);
    }

    protected bool CanDrop(Vector3 position, out VisualElement droppable)
    {
        droppable = target.panel.Pick(position);
        var element = droppable;
        while (element != null && !element.ClassListContains(droppableId))
            element = element.parent;
        if (element != null)
        {
            droppable = element;
            return true;
        }
        return false;
    }
}

public class EnhancedDragAndDropDropEvent : EventBase<EnhancedDragAndDropDropEvent>
{
    public VisualElement TargetElement { get; private set; }
    public VisualElement DroppableElement { get; private set; }

    public static EnhancedDragAndDropDropEvent GetPooled(VisualElement target, VisualElement droppable)
    {
        EnhancedDragAndDropDropEvent pooled = EventBase<EnhancedDragAndDropDropEvent>.GetPooled();
        pooled.TargetElement = target;
        pooled.DroppableElement = droppable;
        return pooled;
    }
}