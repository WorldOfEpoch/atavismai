using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Atavism.UI.Game;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismBankSlot : UIAtavismDraggableSlot
    {

        public int bagNum;
        public Label m_countLabel;
        AtavismInventoryItem item;
        bool mouseEntered = false;
        public VisualElement m_itemIcon;
        public VisualElement m_hover;
        private VisualElement m_Root;
       
        
        public void SetVisualElement(VisualElement visualElement)
        {
            slotBehaviour = DraggableBehaviour.Standard;
            m_Root = visualElement.Q<VisualElement>("slot-container");
            m_countLabel = visualElement.Q<Label>("label");
            m_itemIcon = visualElement.Query<VisualElement>("slot-icon");
            m_hover = visualElement.Q<VisualElement>("hover-icon");
            m_Root.userData = this;
        }

        
        /// <summary>
        /// Creates a UGUIAtavismActivatable object to put in this slot if the item is not null.
        /// </summary>
        /// <param name="item">Item.</param>
        public void UpdateInventoryItemData(AtavismInventoryItem item)
        {
            this.item = item;
            if (item == null)
            {
                if (uiActivatable != null)
                {
                    // Destroy(uguiActivatable.gameObject);
                    uiActivatable.m_Root.RemoveFromHierarchy();
                  //  Destroy(uiActivatable);
                    // m_Root.Remove(uiActivatable.m_Root);
                    uiActivatable = null;
                }
                if (m_itemIcon != null)
                    m_itemIcon.visible = false;
                if (mouseEntered)
                    HideTooltip();
            }
            else
            {
                if (uiActivatable == null)
                {
                    if (m_itemIcon != null)
                    {
                        m_itemIcon.visible = true;
                        if (item.Icon != null)
                            m_itemIcon.style.backgroundImage = item.Icon.texture;
                        else
                            m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
//itemIcon.sprite = item.icon;
                    }
                    uiActivatable = new UIAtavismActivatable(m_Root);
                    //thisUIAtavismActivatable.Startup();
                    uiActivatable.m_Root.AddToClassList("activatableContainer");
                    //UIAtavismActivatable thisUIAtavismActivatable = Instantiate(AtavismSettings.Instance.uiInventoryItemPrefab) as UIAtavismActivatable;


                    m_Root.Add(uiActivatable.m_Root);
                }
                uiActivatable.SetActivatable(item, ActivatableType.Item, this);
            }
        }

        // public override void OnPointerEnter(PointerEventData eventData)
        // {
        //     MouseEntered = true;
        // }
        //
        // public override void OnPointerExit(PointerEventData eventData)
        // {
        //     MouseEntered = false;
        // }

        public override void OnDrop(DropEvent eventData)
        {
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject; 

            // Don't allow reference or temporary slots, or non Item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference ||
                droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary || droppedActivatable.Link != null ||
                (droppedActivatable.ActivatableType != ActivatableType.Item))
            {
                return;
            }

            if (item == null && uiActivatable == null)
            {
                // If this was a drag from a reference, do nothing
                if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference)
                {
                    return;
                }
                this.uiActivatable = droppedActivatable;
                uiActivatable.SetDropTarget(this);
                AtavismInventoryItem newItem = (AtavismInventoryItem)uiActivatable.ActivatableObject;
                Inventory.Instance.PlaceItemInBank(bagNum, slotNum, newItem, newItem.Count, false);
                // if (m_itemIcon != null)
                // {
                //     m_itemIcon.visible = true;
                //     if (newItem.Icon != null)
                //         m_itemIcon.style.backgroundImage = newItem.Icon.texture;
                //     else
                //         m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                //  //   itemIcon.sprite = newItem.icon;
                // }
                if (hover != null)
                    hover.visible = false;
            }
            else
            {
                if (droppedActivatable.Source == this)
                {
                    droppedActivatable.PreventDiscard();
                    return;
                }
                // Check if the source is the same type of item
                if (item != null && droppedActivatable.ActivatableType == ActivatableType.Item)
                {
                    AtavismInventoryItem newItem = (AtavismInventoryItem)droppedActivatable.ActivatableObject;
                    if (item.templateId == newItem.templateId)
                    {
                        Inventory.Instance.PlaceItemInBank(bagNum, slotNum, newItem, newItem.Count, false);
                        droppedActivatable.PreventDiscard();
                        if (hover != null)
                            hover.visible = false;
                    }
                    else
                    {
                        // Send move item with swap
                        Inventory.Instance.PlaceItemInBank(bagNum, slotNum, newItem, newItem.Count, true);
                        if (droppedActivatable.Source.GetType().Equals(typeof(UIAtavismBagSlot)))
                        {
                          /*  UGUIBagSlot dA = (UGUIBagSlot)droppedActivatable.Source;
                            if (dA.itemIcon)
                            {
                                dA.itemIcon.enabled = true;
                                dA.itemIcon.sprite = item.icon;
                            }*/
                        }
                        else if (droppedActivatable.Source.GetType().Equals(typeof(UIAtavismInventorySlot)))
                        {
                       /*     UGUIInventorySlot dA = (UGUIInventorySlot)droppedActivatable.Source;
                            if (dA.itemIcon)
                            {
                                dA.itemIcon.enabled = true;
                                dA.itemIcon.sprite = item.icon;
                            }*/
                        }
                        
                        if (hover != null)
                            hover.visible = false;
                        droppedActivatable.PreventDiscard();
                    }
                }
            }
        }

        public override void ClearChildSlot()
        {
            if (uiActivatable != null)
            {
                // Destroy(uguiActivatable.gameObject);
                uiActivatable.m_Root.RemoveFromHierarchy();
                //  Destroy(uiActivatable);
                // m_Root.Remove(uiActivatable.m_Root);
                uiActivatable = null;
            }
        }

        public override void Discarded()
        {
            if (Inventory.Instance.ItemsOnGround)
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    Inventory.Instance.DropItemOnGround(item);
                    return;
                }
            }
            
#if AT_I2LOC_PRESET
        UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("DeleteItemPopup") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + "?", item, Inventory.Instance.DeleteItemStack);
#else
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Delete " + item.name + "?", item, Inventory.Instance.DeleteItemStack);
#endif
        }

        public override void Activate()
        {
            if (item == null)
                return;
            //if (!AtavismCursor.Instance.HandleUGUIActivatableUseOverride(uguiActivatable)) {
            Inventory.Instance.RetrieveItemFromBank((AtavismInventoryItem)uiActivatable.ActivatableObject);
            //}
        }

        protected override void ShowTooltip()
        {
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