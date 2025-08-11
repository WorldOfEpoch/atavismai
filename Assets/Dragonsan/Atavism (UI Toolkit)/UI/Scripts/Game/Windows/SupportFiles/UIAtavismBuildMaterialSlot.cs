using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismBuildMaterialSlot : UIAtavismDraggableSlot
    {

        Button button;
        public Label countText;
        public UIAtavismItemDisplay itemDisplay;
        AtavismInventoryItem component;
        int count = 0;
        bool mouseEntered = false;
        //float cooldownExpiration = -1;
        bool activeItem = false;

        private VisualElement m_Root;
        // Use this for initialization
        public UIAtavismBuildMaterialSlot()
        {
            slotBehaviour = DraggableBehaviour.Temporary;
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", OnEvent);

        }

        ~UIAtavismBuildMaterialSlot()
        {
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", OnEvent);
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            countText = m_Root.Q<Label>("material-count");
            itemDisplay = m_Root.Q<UIAtavismItemDisplay>("item");
            // m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                UpdateBuildingSlotData(component, count);
            }
        }

        public void UpdateBuildingSlotData(AtavismInventoryItem component, int count)
        {
            // Debug.Log("Updating building slot data "+component+" "+count);
                 
            this.component = component;
            this.count = count;
            if (component == null)
            {
                itemDisplay.HideVisualElement();;
                if (countText != null)  countText.text = "";
            }
            else
            {
                itemDisplay.ShowVisualElement();
                itemDisplay.SetItemData(component);
                if (countText != null)
                {
                    if(WorldBuilder.Instance.showInConstructMaterialsFromBackpack)
                        countText.text = Inventory.Instance.GetCountOfItem(component.templateId)+" / "+count;
                    else
                    if (count > 0)
                    {
                        countText.text = count.ToString();
                    }
                    else
                    {
                        countText.text = "";
                    }
                }
                activeItem = false;
                UpdateDisplay();
            }
        }

        public void OnPointerEnter(MouseEnterEvent evt)
        {
            MouseEntered = true;
        }

        public void OnPointerExit(MouseLeaveEvent evt)
        {
            MouseEntered = false;
        }

        public override void OnDrop(DropEvent eventData)
        {
            if (!WorldBuilder.Instance.itemsForUpgradeMustBeInserted)
                return;
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject;
                //eventData.pointerDrag.GetComponent<UGUIAtavismActivatable>();

            // Reject any references or non item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference || droppedActivatable.Link != null
                || droppedActivatable.ActivatableType != ActivatableType.Item)
            {
                return;
            }

            SetActivatable(droppedActivatable);
        }

        public void SetActivatable(UIAtavismActivatable newActivatable)
        {
            AtavismLogger.LogInfoMessage("Setting activatable");
          
            /*if (uguiActivatable != null && uguiActivatable != newActivatable) {
                // Delete existing child
                DestroyImmediate(uguiActivatable.gameObject);
                if (backLink != null) {
                    backLink.SetLink(null);
                }
            } else if (uguiActivatable == newActivatable) {
                droppedOnSelf = true;
            }*/

            // If the source was a temporary slot, clear it
            if (newActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary)
            {
                newActivatable.Source.ClearChildSlot();
                uiActivatable = newActivatable;

                backLink = newActivatable.Source.BackLink;
            }
            else
            {
                // Does the item placed match the given item?
                AtavismInventoryItem newItem = (AtavismInventoryItem)newActivatable.ActivatableObject;
                if (newItem.templateId != component.templateId)
                {
                    newActivatable.PreventDiscard();
                    return;
                }
                // Create a duplicate
                uiActivatable = new UIAtavismActivatable(m_Root);
                //thisUIAtavismActivatable.Startup();
                uiActivatable.m_Root.AddToClassList("activatableContainer");
                
                uiActivatable.SetActivatable(newActivatable.ActivatableObject, ActivatableType.Item, this);

                // Set the back link
                backLink = newActivatable;
                newActivatable.SetLink(uiActivatable);
            }

            newActivatable.SetDropTarget(this);

            WorldBuilder.Instance.AddItemPlacedForUpgrade((AtavismInventoryItem)newActivatable.ActivatableObject);
            activeItem = true;
            UpdateDisplay();
        }

        public override void ClearChildSlot()
        {
         //   Debug.Log("Clearing child slot");
            if (uiActivatable != null)
                WorldBuilder.Instance.RemoveItemPlacedForUpgrade((AtavismInventoryItem)uiActivatable.ActivatableObject);
            uiActivatable = null;
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
            // if (uguiActivatable != null)
            // {
                // DestroyImmediate(uguiActivatable.gameObject);
            // }
            if (backLink != null)
            {
                backLink.SetLink(null);
            }
            backLink = null;
            ClearChildSlot();
        }

        public override void Activate()
        {
            // Unlink item?
            Discarded();
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        void UpdateDisplay()
        {
            // if (activeItem)
            // {
            //     itemDisplay.GetComponent<Image>().color = Color.white;
            // }
            // else
            // {
            //     itemDisplay.GetComponent<Image>().color = Color.gray;
            // }
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }
        
        public void Hide()
        {
            m_Root.HideVisualElement();
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
                if (mouseEntered && component != null && uiActivatable != null)
                {
                    uiActivatable.ShowTooltip(m_Root);
                }
                else if (mouseEntered && component != null && uiActivatable == null)
                {
                    component.ShowUITooltip(m_Root);
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}