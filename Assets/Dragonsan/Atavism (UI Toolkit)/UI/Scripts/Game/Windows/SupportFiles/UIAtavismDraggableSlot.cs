using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    /*public enum DraggableBehaviour
    {
        Standard, // Can be dragged from and receive drops (i.e Inventory Bags)
        SourceOnly, // Can only be dragged from, will not receive any (i.e Ability Window)
        Reference, // Can receive from all, can only be dragged into other reference slots (i.e Action Bar)
        Temporary // Can receive from all (except reference) and can only be dragged into other temporary slots (i.e Mail attachments, crafting)
    }*/
    public class UIAtavismDraggableSlot : VisualElement
    {

        private string SelectedClassName;// { get; set; }
        private bool isSelected;
        public bool IsSelected => isSelected;
        public VisualElement thisUIAtavismDraggableSlotItem;
        public Image hover;
        public VisualElement thisToolTip;

        public UIAtavismDraggableSlot()
        {
            registerEvents();
        }

        public void registerEvents()
        {
            this.RegisterCallback<DropEvent>(OnDrop);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        public virtual void OnDrop(DropEvent eventData)
        {
            // Debug.LogError("UIAtavismDraggableSlot.OnDrop");
        }

        public virtual void OnMouseEnter(MouseEnterEvent evt)
        {
            // Debug.LogError("UIAtavismDraggableSlot.OnMouseEnter");
            ShowTooltip();
        }

        public virtual void OnMouseLeave(MouseLeaveEvent evt)
        {
            // Debug.LogError("UIAtavismDraggableSlot.OnMouseLeave");
            HideTooltip();
        }



        protected DraggableBehaviour slotBehaviour;// = DraggableBehaviour.Standard;
        protected UIAtavismActivatable uiActivatable;
        public UIAtavismActivatable UiActivatable => uiActivatable;
        public bool allowOverwrite = true;
        public int slotNum;
        public bool ammo = false;
        protected bool droppedOnSelf;// = false; // Has this slot just received a drop that was from itself
        protected UIAtavismActivatable backLink; // Links back to the original
        public VisualElement m_itemIcon;
        AtavismInventoryItem item;
        public int bagNum;

        //protected Coroutine cor = null;
        // Use this for initialization


        void OnDisable()
        {
            ResetSlot();
        }

        public void ResetSlot()
        {
            if (backLink != null)
            {
                backLink.SetLink(null);
            }
        }

        public virtual void ClearChildSlot()
        {
        }
        public virtual void ClearChildSlot(bool send)
        {
        }



        /// <summary>
        /// Called when a player drags the item from the slot and drops it onto nothing
        /// </summary>
        public virtual void Discarded()
        {
        }

        public virtual void Activate()
        {
        }

        public void Awake()
        {
            //this.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }


        protected virtual void ShowTooltip()
        {
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
            //if (cor != null)
              //  StopCoroutine(cor);
        }

        public DraggableBehaviour SlotBehaviour
        {
            get
            {
                return slotBehaviour;
            }
            set
            {
                slotBehaviour = value;
            }
        }

        public UIAtavismActivatable UIActivatable
        {
            get
            {
                return uiActivatable;
            }
            set
            {
                uiActivatable = value;
            }
        }

        public UIAtavismActivatable BackLink
        {
            get
            {
                return backLink;
            }
            set
            {
                backLink = value;
            }


        }

        protected IEnumerator CheckOver()
        {
            bool show = true;
            WaitForSeconds delay = new WaitForSeconds(1.0f);
            while (show)
            {
                Vector2 localMousePosition = this.WorldToLocal(Event.current.mousePosition);
                if (this.ContainsPoint(localMousePosition))
                {
                    // This is a rough translation. In UI Toolkit, there's no direct equivalent to raycasting in UGUI.
                    // Instead, you can check if the pointer is over this element using the above method.
                    // The log messages below can be adjusted accordingly.
                    if (this.ContainsPoint(localMousePosition))
                    {
                        AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver dds is this");
                        show = true;
                    }
                    else
                    {
                        AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver dds not this");
                    }
                }
                else
                {
                    AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver pointer not over this element");
                }
                yield return delay;
            }
            HideTooltip();
        }



        public void CheckOver(UIAtavismDraggableSlot thisslot)
        {
            bool show = true;
            //WaitForSeconds delay = new WaitForSeconds(1.0f);
            while (show)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        pointerId = -1,
                    };
                    pointerData.position = Input.mousePosition;
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);
                    show = false;
                    if (results.Count > 0)
                    {
                        foreach (RaycastResult rr in results)
                        {
                            if (rr.gameObject != null)
                            {
                                UIAtavismDraggableSlot dds = thisslot;
                                if (dds != null)
                                {
                                    if (dds == this)
                                    {
                                        AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver dds is this");
                                        show = true;
                                    }
                                    else
                                        AtavismLogger.LogDebugMessage("GUIDraggableSlot CheckOver dds not this");
                                }
                                else
                                    AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver dds null");
                            }
                            else
                                AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver rr.gameObject null");
                        }
                    }
                    else
                        AtavismLogger.LogDebugMessage("UIAtavismDraggableSlot CheckOver results.Count = 0");
                }
                //yield return delay;
            }
            HideTooltip();
        }

    }
}