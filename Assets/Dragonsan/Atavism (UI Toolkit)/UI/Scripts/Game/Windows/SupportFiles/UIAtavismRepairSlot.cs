using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismRepairSlot : UIAtavismDraggableSlot
    {
        // UGUIRepairWindow repairWindow;
        bool mouseEntered = false;
        public VisualElement thisUIAtavismRepairSlotItem;

        public VisualElement m_Root;
        public VisualElement m_hover;
        public VisualElement m_itemIcon;

        public UIAtavismRepairSlot()
        {
            slotBehaviour = DraggableBehaviour.Temporary;
            // thisUIAtavismRepairSlotItem = AtavismSettings.Instance.UISlotInventoryItemUXML.Instantiate();
            // registerUI();
            // m_Root.userData = this;
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            slotBehaviour = DraggableBehaviour.Temporary;
            m_Root = visualElement.Q<VisualElement>("slot-container");
            // m_countLabel = visualElement.Q<Label>("label");
            m_itemIcon = visualElement.Query<VisualElement>("slot-icon");
            m_hover = visualElement.Q<VisualElement>("hover-icon");
            m_Root.userData = this;
        }
        
        // protected bool registerUI()
        // {
        //
        //     //Search the root for the SlotContainer Visual Element
        //     m_Root = thisUIAtavismRepairSlotItem.Q<VisualElement>("slot-container");
        //     m_hover = thisUIAtavismRepairSlotItem.Q<VisualElement>("hover-icon");
        //     m_itemIcon = thisUIAtavismRepairSlotItem.Query<VisualElement>("slot-icon");
        //     return true;
        // }

   


        public void UpdateRepairSlotData(AtavismInventoryItem component)
        {
            if (component == null)
            {
                if (uiActivatable != null)
                {
                //    uiActivatable.Clear();
                    if (backLink != null)
                    {
                        backLink.SetLink(null);
                    }
                }
            }
            else
            {
                if (uiActivatable == null)
                {

                    uiActivatable = new UIAtavismActivatable(m_Root);
                    uiActivatable.m_Root.AddToClassList("activatableContainer");

                    m_Root.Add(uiActivatable.m_Root);
                }

                uiActivatable.SetActivatable(component, ActivatableType.Item, this);
            }
        }

        public void OnPointerEnter(PointerEnterEvent evt)
        {
            mouseEntered = true;
        }

        public void OnPointerLeave(PointerLeaveEvent evt)
        {
            mouseEntered = false;
        }

        public override void OnDrop(DropEvent evt)
        {
            UIAtavismActivatable droppedActivatable =  DragDropManager.CurrentlyDraggedObject;

            // Reject any references or non item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference || droppedActivatable.Link != null
                || droppedActivatable.ActivatableType != ActivatableType.Item)
            {
                return;
            }
            else
            {
                AtavismInventoryItem droppedItem = (AtavismInventoryItem)droppedActivatable.ActivatableObject;
                if (droppedItem.MaxDurability == 0 || droppedItem.Durability == droppedItem.MaxDurability || !droppedItem.repairable)
                {
                    // dispatch a ui event to tell the rest of the system
                    string[] args = new string[1];
                    args[0] = "That Item cannot be repaired";
                    AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                    droppedActivatable.PreventDiscard();
                    return;
                }
            }
            SetActivatable(droppedActivatable);
        }

        public void SetActivatable(UIAtavismActivatable droppedActivatable)
        {

            if (uiActivatable != null && uiActivatable != droppedActivatable)
            {
                // Delete existing child
                // uiActivatable.Clear();
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
                uiActivatable = droppedActivatable;//Instantiate(droppedActivatable);
                uiActivatable.SetActivatable(droppedActivatable.ActivatableObject, ActivatableType.Item, this);

                // Set the back link
                backLink = droppedActivatable;
                droppedActivatable.SetLink(uiActivatable);
            }

            droppedActivatable.SetDropTarget(this);

            //repairWindow.RepairListUpdated(this);
        }

        public override void ClearChildSlot()
        {
            uiActivatable = null;
            //Crafting.Instance.SetGridItem(slotNum, null);
        }

        public override void Discarded()
        {
            if (droppedOnSelf)
            {
                droppedOnSelf = false;
                return;
            }
            if (uiActivatable != null)
                m_Root.Remove(uiActivatable.m_Root);
        //    uiActivatable.Clear();
            if (backLink != null)
            {
                backLink.SetLink(null);
            }
            backLink = null;
            ClearChildSlot();
        }
        
        public override void Activate()
        {
            Discarded();
            // Do nothing
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
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
                if (mouseEntered && uiActivatable != null)
                {
                    uiActivatable.ShowTooltip(m_Root);
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}