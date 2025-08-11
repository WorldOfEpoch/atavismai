using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public delegate void ItemResponse(AtavismInventoryItem item);

    public class UIAtavismGearSocketSlot : UIAtavismDraggableSlot, IPointerClickHandler
    {

        // Button button;
        AtavismAction action;
        bool mouseEntered = false;
        public VisualElement m_Root;
        
        UIAtavismActivatable thisUIAtavismActivatable;
        public VisualElement thisInventorySlotItem;
        
        
        public UIAtavismGearSocketSlot()
        {
            registerEvents();
            thisInventorySlotItem = AtavismSettings.Instance.UISlotInventoryItemUXML.Instantiate();
            Add(thisInventorySlotItem);
            registerUI();
            m_Root.userData = this;
            slotBehaviour = DraggableBehaviour.Temporary;

        }
        
        protected bool registerUI()
        {

            //Search the root for the SlotContainer Visual Element
            m_Root = thisInventorySlotItem.Q<VisualElement>("slot-container");
            // m_hover = thisInventorySlotItem.Q<VisualElement>("hover-icon");
            m_itemIcon = thisInventorySlotItem.Query<VisualElement>("slot-icon");
            return true;
        }


        public void UpdateAttachmentData(AtavismAction action)
        {
            if (itemResponse != null)
                itemResponse(null);
            Debug.LogWarning("UpdateAttachmentData");

            this.action = action;
            if (action == null || action.actionObject == null)
            {
                if (uiActivatable != null)
                {
                    Debug.LogWarning("UpdateAttachmentData dest/roy");
                    if (uiActivatable != null)
                        m_Root.Remove(uiActivatable.m_Root);
                }
                Debug.LogWarning("UpdateAttachmentDatav 2");

            }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            // Debug.LogWarning("UGUIGearSocketSlot OnPointerClick " + eventData);

        }

//         public override void OnPointerEnter(PointerEventData eventData)
//         {
// #if !AT_MOBILE             
//             MouseEntered = true;
// #endif            
//         }
//
//         public override void OnPointerExit(PointerEventData eventData)
//         {
// #if !AT_MOBILE             
//             MouseEntered = false;
// #endif            
//         }

        public override void OnDrop(DropEvent eventData)
        {
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject; 

            // Reject any references or non item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference || droppedActivatable.Link != null
                || droppedActivatable.ActivatableType != ActivatableType.Item)
            {
                return;
            }

            SetActivatable(droppedActivatable);
        }

        public void SetActivatable(UIAtavismActivatable droppedActivatable)
        {


            if (uiActivatable != null && uiActivatable != droppedActivatable)
            {

                
                // Delete existing child
                m_Root.Remove(uiActivatable.m_Root);
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

                // uiActivatable.transform.SetParent(transform, false);
                // uiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                backLink = droppedActivatable.Source.BackLink;
            }
            else
            {
                AtavismInventoryItem item = (AtavismInventoryItem)droppedActivatable.ActivatableObject;
                if (item != null)
                {
                    if ((item.sockettype.Length > 0 && SocketMode == 0) || (item.itemEffectTypes.Contains("Sockets") && SocketMode == 1))
                    {

                        // Create a duplicate
             
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");

                        m_Root.Add(uiActivatable.m_Root);
                        uiActivatable.SetActivatable(droppedActivatable.ActivatableObject, ActivatableType.Item, this);

                        // Set the back link
                        backLink = droppedActivatable;
                        droppedActivatable.SetLink(uiActivatable);

                        droppedActivatable.SetDropTarget(this);
                        if (itemResponse != null)
                            itemResponse(item);
                        else
                            Debug.LogWarning("itemResponse is null");

                    }
                    else if ((item.itemType.Contains("Armor") || item.itemType.Contains("Weapon")) && SocketMode == 2 && item.EnchantId > 0)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");

                        m_Root.Add(uiActivatable.m_Root);
                        uiActivatable.SetActivatable(droppedActivatable.ActivatableObject, ActivatableType.Item, this);

                        // Set the back link
                        backLink = droppedActivatable;
                        droppedActivatable.SetLink(uiActivatable);

                        droppedActivatable.SetDropTarget(this);
                        if (itemResponse != null)
                            itemResponse(item);
                        else
                            Debug.LogWarning("itemResponse is null");
                    }
                    else
                    {
                        //    droppedActivatable.Source.ClearChildSlot();
                        droppedActivatable.PreventDiscard();
                        /*   droppedActivatable.PreventDiscard();
                           droppedActivatable.transform.SetParent(transform, false);
                           droppedActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                           backLink = droppedActivatable.Source.BackLink;*/
                        //   Debug.LogError("Wrong Item");
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                    args[0] = I2.Loc.LocalizationManager.GetTranslation("Wrong Item");
#else
                        args[0] = "Wrong Item";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                    }


                }

            }


        }
        ItemResponse itemResponse;
        int SocketMode = 0;
        public void SetSocket(ItemResponse itemResponse, int SocketMode)
        {
            this.itemResponse = itemResponse;
            this.SocketMode = SocketMode;
        }

        public override void ClearChildSlot()
        {
            uiActivatable = null;
            //   Mailing.Instance.SetMailItem(slotNum, null);
        }

        public override void Discarded()
        {
            // Debug.LogError("Discarded UGUIGearSocketSlot " + name);

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
            if (itemResponse != null)
                itemResponse(null);
            ClearChildSlot();
        }

        public override void Activate()
        {
            if (uiActivatable != null)
                Discarded();
            // Debug.LogError("UGUIGearSocketSlot Activate");
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