using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismShopItemSlot : UIAtavismDraggableSlot
    {

        Button button;
        Label nameLabel;
        AtavismInventoryItem item;
        bool mouseEntered = false;
         UIAtavismCreateShop acs;
        int pos = -1;
        
        public VisualElement m_Root;
        public VisualElement m_hover;
        public VisualElement thisSlotItem;
        // Use this for initialization
        public UIAtavismShopItemSlot(VisualElement visualElement)
        {
            m_Root = visualElement;
            registerUI();
            // registerEvents();
            //manipulator = new(m_Root);
            //itemIcon = new Image();
            // thisSlotItem = AtavismSettings.Instance.UISlotInventoryItemUXML.Instantiate();
            
           
           // visualElement.Add(m_Root);
            m_Root.userData = this;
            slotBehaviour = DraggableBehaviour.Temporary;
        }
        
        protected bool registerUI()
        {

            //Search the root for the SlotContainer Visual Element
            // m_Root = m_Root.Q<VisualElement>("slot-container");
            // m_hover = m_Root.Q<VisualElement>("hover-icon");
            m_itemIcon = m_Root.Query<VisualElement>("item-icon");
            return true;
        }
        
        public void AssignShop(UIAtavismCreateShop acs,int pos)
        {
            this.acs = acs;
            this.pos = pos;
        }

        public void UpdateSlotData(AtavismInventoryItem item)
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
                    //UIAtavismActivatable thisUIAtavismActivatable = Instantiate(AtavismSettings.Instance.uiInventoryItemPrefab) as UIAtavismActivatable;


                    m_Root.Add(uiActivatable.m_Root);
                    // if (AtavismSettings.Instance.inventoryItemPrefab != null)
                    //     uguiActivatable = (UGUIAtavismActivatable)Instantiate(AtavismSettings.Instance.inventoryItemPrefab, transform, false);
                    // else
                    //     uguiActivatable = (UGUIAtavismActivatable)Instantiate(Inventory.Instance.uguiAtavismItemPrefab);
                    // //uguiActivatable.transform.SetParent(transform, false);
                    // uguiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    // if (uguiActivatable.GetComponent<RectTransform>().anchorMin == Vector2.zero && uguiActivatable.GetComponent<RectTransform>().anchorMax == Vector2.one)
                    //     uguiActivatable.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
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
                //eventData.pointerDrag.GetComponent<UGUIAtavismActivatable>();
            AtavismInventoryItem item = (AtavismInventoryItem)droppedActivatable.ActivatableObject;
            if (item != null)
            {
                if (item.isBound )
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
                else if ( !item.sellable || !item.auctionHouse)
                {
                    string[] args = new string[1];
#if AT_I2LOC_PRESET
                args[0] = I2.Loc.LocalizationManager.GetTranslation("You can't trade item");
#else
                    args[0] = "You can't trade item";
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
                droppedActivatable.Source.ClearChildSlot(false);
                uiActivatable = droppedActivatable;

                // uiActivatable.transform.SetParent(transform, false);
                // uguiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                // if (uguiActivatable.GetComponent<RectTransform>().anchorMin == Vector2.zero && uguiActivatable.GetComponent<RectTransform>().anchorMax == Vector2.one)
                //     uguiActivatable.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                backLink = droppedActivatable.Source.BackLink;
            }
            else
            {
                uiActivatable = new UIAtavismActivatable(m_Root);
                //thisUIAtavismActivatable.Startup();
                uiActivatable.m_Root.AddToClassList("activatableContainer");
                //UIAtavismActivatable thisUIAtavismActivatable = Instantiate(AtavismSettings.Instance.uiInventoryItemPrefab) as UIAtavismActivatable;

                m_Root.Add(uiActivatable.m_Root);
                // Create a duplicate
                // uiActivatable = Instantiate(droppedActivatable);
                // uiActivatable.transform.SetParent(transform, false);
                // uguiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                // if (uguiActivatable.GetComponent<RectTransform>().anchorMin == Vector2.zero && uguiActivatable.GetComponent<RectTransform>().anchorMax == Vector2.one)
                //     uguiActivatable.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                // uguiActivatable.GetComponent<CanvasGroup>().blocksRaycasts = true;
                uiActivatable.SetActivatable(droppedActivatable.ActivatableObject, ActivatableType.Item, this);

                // Set the back link
                backLink = droppedActivatable;
                droppedActivatable.SetLink(uiActivatable);
            }

            droppedActivatable.SetDropTarget(this);
            this.item = item;
            acs.DropItem((AtavismInventoryItem)droppedActivatable.ActivatableObject,pos);
        }

        public override void ClearChildSlot(bool send)
        {
         //   Debug.LogError("ClearChildSlot");
            this.item = null;
         /*   if (item == null)
                return;*/
            uiActivatable = null;
           
        }

        public override void Discarded()
        {
          //  Debug.LogError("Discarded");
            if (droppedOnSelf)
            {
                droppedOnSelf = false;
                return;
            }
            if (backLink != null)
            {
                backLink.SetLink(null);
            }
            if(uiActivatable!=null)
                m_Root.Remove(uiActivatable.m_Root);
            backLink = null;
            acs.ClearSlot(pos, item);
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
                if (mouseEntered && item != null )
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