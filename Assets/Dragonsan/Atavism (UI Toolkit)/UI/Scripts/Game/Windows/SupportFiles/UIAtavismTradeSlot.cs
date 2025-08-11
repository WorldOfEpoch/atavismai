using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismTradeSlot : UIAtavismDraggableSlot
    {

        Button button;
        public Label nameLabel;
        public Label cooldownLabel;
        AtavismInventoryItem item;
        bool mouseEntered = false;
        //float cooldownExpiration = -1;
        public VisualElement m_Root;
        // Use this for initialization
        void Start()
        {
            
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root= visualElement.Q<VisualElement>("slot"); 
            nameLabel = visualElement.Q<Label>("name"); 
            slotBehaviour = DraggableBehaviour.Temporary;
        }
        public void UpdateTradeSlotData(AtavismInventoryItem item)
        {
            this.item = item;
            if (item == null)
            {
                if (uiActivatable != null)
                {
                    
                    m_Root.Remove(uiActivatable.m_Root);
                }
                if (nameLabel != null)
                    nameLabel.text = "";
            }
            else
            {
                if (uiActivatable == null)
                {
                    uiActivatable = new UIAtavismActivatable(m_Root);
                    //thisUIAtavismActivatable.Startup();
                    uiActivatable.m_Root.AddToClassList("activatableContainer");
                    //uguiActivatable.transform.SetParent(transform, false);
                 
                }
                uiActivatable.SetActivatable(item, ActivatableType.Item, this);
#if AT_I2LOC_PRESET
            if (nameLabel != null)  nameLabel.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name); ;
#else
                if (nameLabel != null)
                    nameLabel.text = item.name;
#endif
            }
        }

        public override void OnMouseEnter(MouseEnterEvent evt)
        {
            MouseEntered = true;
        }

        public override void OnMouseLeave(MouseLeaveEvent evt)
        {
            MouseEntered = false;
        }

        public override void OnDrop(DropEvent eventData)
        {
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject;
            //eventData.pointerDrag.GetComponent<UGUIAtavismActivatable>();
            AtavismInventoryItem item = (AtavismInventoryItem)droppedActivatable.ActivatableObject;
            if (item != null)
            {
                if (item.isBound)
                {
                    string[] args = new string[1];
#if AT_I2LOC_PRESET
                args[0] = I2.Loc.LocalizationManager.GetTranslation("You can't trade soulbound item");
#else
                    args[0] = "You can't trade soulbound item";
#endif
                    AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                    droppedActivatable.PreventDiscard();
                    return;
                }
            }
            // Reject any references or non item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference || droppedActivatable.Link != null
                || droppedActivatable.ActivatableType != ActivatableType.Item)
            {
                return;
            }

            if (uiActivatable != null && uiActivatable != droppedActivatable)
            {
                // Delete existing child
                uiActivatable.m_Root.RemoveFromHierarchy();
                
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
                droppedActivatable.Source.ClearChildSlot(false);
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

            AtavismTrade.Instance.ItemPlacedInTradeSlot((AtavismInventoryItem)droppedActivatable.ActivatableObject, slotNum, true);
        }

        public override void ClearChildSlot(bool send)
        {
            if (item == null)
                return;
            uiActivatable = null;
            AtavismTrade.Instance.ItemPlacedInTradeSlot(null, slotNum, send);
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
            if (backLink != null)
            {
                backLink.SetLink(null);
            }
            backLink = null;
            ClearChildSlot(true);
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
                if (mouseEntered && item != null)
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