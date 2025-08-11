using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
namespace Atavism.UI
{
    public class UIAtavismInventorySlot : UIAtavismDraggableSlot
    {
        public OID ItemGuid = null;
        public string slotName;
        Button button;
        AtavismInventoryItem item;
        bool mouseEntered = false;
     //   string s_StyleSheetPath = "Assets/Atavism (UI Toolkit)/UI/Style Sheets/InventorySlot.uss";
        UIAtavismActivatable thisUIAtavismActivatable;
        public VisualElement thisInventorySlotItem;

        private UIAtavismInventory inventory;
        //public int bagNum;
        // public TextMeshProUGUI TMPCountLabel;
        //public Image itemIcon;
        public VisualTreeAsset m_thisTreeItem;

        public VisualElement m_Root;
        public VisualElement m_hover;
        //public VisualElement m_itemIcon;
        //internal int slotNum;
        /*
        #region UXML
        [Preserve]
        public class UxmlFactory : UxmlFactory<UIAtavismInventorySlot, UxmlTraits> { }
        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        #endregion

        */

        public UIAtavismInventorySlot()
        {
            registerEvents();
            //manipulator = new(m_Root);
            //itemIcon = new Image();
            thisInventorySlotItem = AtavismSettings.Instance.UISlotInventoryItemUXML.Instantiate();
            registerUI();
            m_Root.userData = this;

            // this.RegisterCallback(OnPointerEnter, TrickleDown.TrickleDown);
            //this.thisInventorySlotItem.RegisterCallback<PointerEventData>(OnPointerExit);
            // this.thisInventorySlotItem.RegisterCallback<PointerEventData>(OnDrop);



            //this.RegisterCallback<MouseUpEvent>(onClick);
            slotBehaviour = DraggableBehaviour.Standard;
            AtavismEventSystem.RegisterEvent("ITEM_RELOAD", OnEvent);

        }

        public override void OnMouseEnter(MouseEnterEvent evt)
        {
                UIAtavismActivatable dActivatable = uiActivatable;
                if (dActivatable != null)
                    if (dActivatable.Source == this)
                    {
                        if (hover != null)
                        {
                            hover.SetEnabled(true);
                        uiActivatable.hover.style.backgroundColor = AtavismSettings.Instance.itemDropColorFalse;

                    }
                }
                    else
                    {
                        if (uiActivatable != null)
                        {
                            if (uiActivatable.hover != null)
                            {
                                uiActivatable.hover.SetEnabled(true);

                                uiActivatable.hover.style.backgroundColor = AtavismSettings.Instance.itemDropColorTrue;
                            }
                        }
                        else
                        {
                            if (hover != null)
                            {
                                hover.SetEnabled(true);
                            uiActivatable.hover.style.backgroundColor = AtavismSettings.Instance.itemDropColorTrue;
                        }
                    }
                    }
            
            MouseEntered = true;
        }

        private void onInventoryClick(PointerUpEvent evt)
        {
            //Not the left mouse button
            if (evt.button != 0 )
            {
                return;
            }
            //Clear the image
            m_itemIcon.style.backgroundImage = null;
            //Start the drag
            UIAtavismInventory.StartDrag(evt.position, this);
        }

        protected bool registerUI()
        {

            //Search the root for the SlotContainer Visual Element
            m_Root = thisInventorySlotItem.Q<VisualElement>("slot-container");
            m_hover = thisInventorySlotItem.Q<VisualElement>("hover-icon");
            m_itemIcon = thisInventorySlotItem.Query<VisualElement>("slot-icon");
            return true;
        }


        public void HoldItem(AtavismInventoryItem thisItem)
        {
            item = thisItem;
            m_itemIcon.style.backgroundImage = item.Icon.texture;
            ItemGuid = item.ItemId;
        }
        public void DropItem()
        {
            item = null;
            ItemGuid = null;
            m_itemIcon.style.backgroundImage = null;
        }



        private void OnDisable ()
        {
            //uiActivatable.Clear();
        }


        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("ITEM_RELOAD", OnEvent);
            //uiActivatable.Clear();
        }

        public void OnEvent(AtavismEventData eData)
        {
            AtavismLogger.LogDebugMessage("UIAtavismInventorySlot " + eData.eventType);

            if (eData.eventType == "ITEM_RELOAD")
            {
                if (item != null)
                {
                    item = AtavismPrefabManager.Instance.LoadItem(item);
                    UpdateInventoryItemData(item, inventory);
                }
            }
        }

       public void Update()
        {
            // Debug.LogError("UIAtavismInventorySlot Update");
            if (this.item == null)
            {
                if (m_itemIcon != null)
                    m_itemIcon.SetEnabled(false);
            }

            if (uiActivatable != null)
                uiActivatable.update();
        }

        /// <summary>
        /// Creates a UIAtavismActivatable object to put in this slot if the item is not null.
        /// </summary>
        /// <param name="item">Item.</param>
        public void UpdateInventoryItemData(AtavismInventoryItem item, UIAtavismInventory inventory)
        {
            this.inventory = inventory;
            this.item = item;
            if (item == null)
            {
                if (uiActivatable != null)
                {
                    //uiActivatable.Clear();
                    uiActivatable = null;
                    if (m_itemIcon != null)
                        m_itemIcon.SetEnabled(false);
                    if (mouseEntered)
                        mouseEntered = false;

                }
            }
            else
            {
                //   Debug.LogError("UpdateInventoryItemData: item nit null uiActivatable:"+ uiActivatable);
                if (uiActivatable == null)
                {
                    if (AtavismSettings.Instance.UIActivatableUXML != null)
                    {
                        //    Debug.LogError("UpdateInventoryItemData: item nit null AtavismSettings");
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        //thisUIAtavismActivatable.Startup();
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        //UIAtavismActivatable thisUIAtavismActivatable = Instantiate(AtavismSettings.Instance.uiInventoryItemPrefab) as UIAtavismActivatable;


                        m_Root.Add(uiActivatable.m_Root);
                       // uiActivatable.BringToFront();
                      //  uiActivatable.style.position = Position.Absolute;
                    }
                    else
                    {
                        //    Debug.LogError("UpdateInventoryItemData: item nit null Inentory");
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        //thisUIAtavismActivatable.Startup();
                        uiActivatable.m_Root.AddToClassList("activatableContainer");

                        //UIAtavismActivatable thisUIAtavismActivatable = Instantiate(Inventory.Instance.uiAtavismItemPrefab) as UIAtavismActivatable;
                        //                        thisInventorySlotItem.Add(thisUIAtavismActivatable.m_Root);

                        //uiActivatable = (UIAtavismActivatable)Instantiate(Inventory.Instance.uiAtavismItemPrefab);
                        m_Root.Add(uiActivatable.m_Root);
                        //uiActivatable.BringToFront();
                        //uiActivatable.style.position = Position.Absolute;
                    }
                }
                if (m_itemIcon != null)
                {
                    m_itemIcon.SetEnabled(true);
                    if (item.Icon != null)
                        m_itemIcon.SetBackgroundImage(item.Icon);
                    else
                        m_itemIcon.SetBackgroundImage( AtavismSettings.Instance.defaultItemIcon);
                }
                if(uiActivatable != null)
                    uiActivatable.SetActivatable(item, ActivatableType.Item, this);
               // check this for mouse over
               // if (mouseEntered)
                 //   uiActivatable.ShowTooltip(this);
            }
        }
        
        public override void OnMouseLeave(MouseLeaveEvent evt)
        {
          //  Debug.LogError("UIAtavismInventorySlot.OnMouseLeave");
            if (uiActivatable != null)
            {
                if (uiActivatable.hover != null)
                    uiActivatable.hover.SetEnabled(false);
            }
            if (hover != null)
                hover.SetEnabled(true);
            MouseEntered = false;
        }

        public override void OnDrop(DropEvent eventData1)
        {
         //   Debug.LogError("UIAtavismInventorySlot.OnDrop");
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject;

            // Don't allow reference or temporary slots, or non Item/bag slots
            if (droppedActivatable != null)
                if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference ||
                droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary || droppedActivatable.Link != null ||
                (droppedActivatable.ActivatableType != ActivatableType.Item && droppedActivatable.ActivatableType != ActivatableType.Bag))
                {
                    return;
                }
            if (droppedActivatable != null)
                if (item == null && uiActivatable == null)
                {
                    // If this was a drag from a reference, do nothing
                    if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference)
                    {
                        return;
                    }
                    else if (droppedActivatable.ActivatableType == ActivatableType.Bag)
                    {
                        // If it is a bag, send the place bag message
                        if (droppedActivatable.Source.slotNum == bagNum)
                        {
                            if (hover != null)
                                hover.SetEnabled(true);
                            droppedActivatable.PreventDiscard();
                            return;
                        }

                        Inventory.Instance.PlaceBagAsItem(droppedActivatable.Source.slotNum, bagNum, slotNum);

                        if (m_itemIcon != null)
                        {
                            m_itemIcon.SetEnabled(true); 
                            m_itemIcon = droppedActivatable.m_Icon;
                        }
                        if (hover != null)
                            hover.SetEnabled(true);
                        return;
                    }
                    else if (droppedActivatable.Source is UIAtavismMerchantItemSlot)
                    {
                        droppedActivatable.Source.Activate();
                        return;
                    }
                    if (droppedActivatable.Source.ammo)
                    {
                        droppedActivatable.PreventDiscard();
                        return;
                    }
                    this.uiActivatable = droppedActivatable;
                    uiActivatable.SetDropTarget(this);
                    AtavismInventoryItem newItem = (AtavismInventoryItem)uiActivatable.ActivatableObject;

                    Inventory.Instance.PlaceItemInBag(bagNum, slotNum, newItem, newItem.Count);
                    if (m_itemIcon != null)
                    {
                        m_itemIcon.SetEnabled(true);
                        if (newItem.Icon != null)
                            m_itemIcon.style.backgroundImage = newItem.Icon.texture;
                        else
                         m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture; 
                    //    itemIcon.sprite = newItem.icon;
                    }
                    if (hover != null)
                        hover.SetEnabled(true);

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
                            Inventory.Instance.PlaceItemInBag(bagNum, slotNum, newItem, newItem.Count);
                            droppedActivatable.PreventDiscard();
                            if (hover != null)
                                 hover.SetEnabled(true);
                        }
                        else
                        {
                            // Send move item with swap
                            Inventory.Instance.PlaceItemInBag(bagNum, slotNum, newItem, newItem.Count, true);
                            if (m_itemIcon != null)
                            {
                                m_itemIcon.SetEnabled(true);
                                if (newItem.Icon != null)
                                    m_itemIcon.style.backgroundImage = newItem.Icon.texture;
                                else
                                    m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                                //     itemIcon.sprite = newItem.icon;
                            }
                            UIAtavismInventorySlot dA = (UIAtavismInventorySlot)droppedActivatable.Source;
                            if (dA.m_itemIcon != null)
                            {
                                dA.m_itemIcon.SetEnabled(true);
                                dA.m_itemIcon.style.backgroundImage = item.Icon.texture;
                            }
                            if (hover != null)
                                 hover.SetEnabled(true);

                            droppedActivatable.PreventDiscard();

                        }
                    }
                }
        }
        public override void Activate()
        {
          //  Debug.LogError("Activate");
            if (item == null)
                return;
            if (!AtavismCursor.Instance.HandleUIActivatableUseOverride(uiActivatable))
            {
                item.Activate();
            }
        }

        public override void ClearChildSlot()
        {
            uiActivatable = null;
            if (m_itemIcon != null)
                m_itemIcon.SetEnabled(false);
        }

        public override void Discarded()
        {
            if (item != null)
            {
                if (Inventory.Instance.ItemsOnGround)
                {
                    if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    {
                        Inventory.Instance.DropItemOnGround(item);
                        return;
                    }
                }

                // Debug.LogError("Atavism Inventory slot Discarded");
#if AT_I2LOC_PRESET
        UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("DeleteItemPopup") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + "?", item, Inventory.Instance.DeleteItemStack);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Delete " + item.name + "?", item,
                    Inventory.Instance.DeleteItemStack);

#endif
            }
        }

        public new void Awake()
        {
            base.Awake();
            if (item == null)
                return;
            if (!AtavismCursor.Instance.HandleUIActivatableUseOverride(uiActivatable))
            {
                item.Activate();
            }
        }

        protected override void ShowTooltip()
        {
            if(item!=null)
                item.ShowUITooltip(m_Root);
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
            //if (cor != null)
             //   StopCoroutine(cor);
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
                    ShowTooltip();
                    //cor = 
                       // CheckOver(this);
                }
                else
                {
                    HideTooltip();
                }
            }
        }


    }





}