using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMailAttachmentSlot : UIAtavismDraggableSlot
    {

        AtavismAction action;
        bool mouseEntered = false;
        private VisualElement m_Root;
        // Use this for initialization
        void Start()
        {
            
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement.Q<VisualElement>("slot-container");
            // m_hover = visualElement.Q<VisualElement>("hover-icon");
            // m_itemIcon = visualElement.Query<VisualElement>("slot-icon");
            m_Root.userData = this;
            slotBehaviour = DraggableBehaviour.Temporary;
        }
        public void UpdateAttachmentData(AtavismAction action)
        {
            this.action = action;
            if (action == null || action.actionObject == null)
            {
                if (uiActivatable != null)
                {
                   
                }
            }
            else
            {
                if (uiActivatable == null)
                {
                    if (action.actionType == ActionType.Ability)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                    }
                    else
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                    }
                }
                uiActivatable.SetActivatable(action.actionObject, ActivatableType.Item, this);
            }
        }

        public override void OnMouseEnter(MouseEnterEvent eventData)
        {
#if !AT_MOBILE             
            MouseEntered = true;
#endif            
        }

        public override void OnMouseLeave(MouseLeaveEvent eventData)
        {
#if !AT_MOBILE             
            MouseEntered = false;
#endif            
        }

        public override void OnDrop(DropEvent eventData)
        {
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject; 

            // Reject any references or non item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference || droppedActivatable.Link != null
                || droppedActivatable.ActivatableType != ActivatableType.Item)
            {
                return;
            }

            if (uiActivatable != null && uiActivatable != droppedActivatable)
            {
                // Delete existing child
                // DestroyImmediate(uiActivatable.gameObject);
                if (backLink != null)
                {
                    backLink.SetLink(null);
                }
            }
            else if (uiActivatable == droppedActivatable)
            {
                droppedOnSelf = true;
            }

            // If the source was a temporary slot, clear it
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary)
            {
                droppedActivatable.Source.ClearChildSlot();
                uiActivatable = droppedActivatable;

               
                backLink = droppedActivatable.Source.BackLink;
            }
            else
            {
                // Create a duplicate
                uiActivatable = new UIAtavismActivatable(m_Root);
                //thisUIAtavismActivatable.Startup();
                uiActivatable.m_Root.AddToClassList("activatableContainer");

               
                uiActivatable.SetActivatable(droppedActivatable.ActivatableObject, ActivatableType.Item, this);

                // Set the back link
                backLink = droppedActivatable;
                droppedActivatable.SetLink(uiActivatable);
            }

            droppedActivatable.SetDropTarget(this);

            Mailing.Instance.SetMailItem(slotNum, (AtavismInventoryItem)droppedActivatable.ActivatableObject);
        }

        public override void ClearChildSlot()
        {
            uiActivatable = null;
            Mailing.Instance.SetMailItem(slotNum, null);
        }

        public override void Discarded()
        {
            // Debug.LogError("Discarded");
            if (droppedOnSelf)
            {
                droppedOnSelf = false;
                return;
            }
             if (uiActivatable != null)
                 m_Root.Remove(uiActivatable.m_Root);
                // DestroyImmediate(uiActivatable.gameObject);
            if (backLink != null)
            {
                backLink.SetLink(null);
            }
            backLink = null;
            ClearChildSlot();
        }

        public override void Activate()
        {
            // Do nothing
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
            // if (cor != null)
                // StopCoroutine(cor);
        }

        public bool MouseEntered
        {
            get
            {
                return mouseEntered;
            }
            set
            {
                mouseEntered = value;
                //if (mouseEntered && action != null && action.actionObject != null) {
                if (mouseEntered && uiActivatable != null)
                {
                    uiActivatable.ShowTooltip(m_Root);
                    // cor = StartCoroutine(CheckOver());
                }
                else
                {
                    HideTooltip();
                }
            }
        }

       
    }
}