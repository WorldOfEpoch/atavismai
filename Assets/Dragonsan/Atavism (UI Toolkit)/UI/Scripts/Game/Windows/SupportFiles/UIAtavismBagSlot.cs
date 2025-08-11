using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
namespace Atavism.UI
{
    public class UIAtavismBagSlot : UIAtavismDraggableSlot
    {
        // public VisualElement Icon;
        // public OID ItemGuid = null;
        public string slotName;
        Button button;
        AtavismInventoryItem item;
        bool mouseEntered = false;
        public VisualElement thisBagSlotItem;
        
        UIAtavismBag bagPanel;
        // public Label cooldownLabel;
        Bag bag;
        // public KeyCode activateKey;
        //	float cooldownExpiration = -1;
        // public Material materialGray;
        // public VisualElement hover;
        // internal int slotNum;

        public VisualElement m_Root;
        public VisualElement m_hover;
        public VisualElement m_itemIcon;

        private UIAtavismInventory inventory;
        //UGUIBag bagPanel;
        //	float cooldownExpiration = -1;
        //public Image m_itemIcon;

        public void Startup()
        {
            //m_itemIcon = new Image();
            thisBagSlotItem = AtavismSettings.Instance.UISlotBagItemUXML.Instantiate();
            registerUI();
            m_Root.userData = this;
            slotBehaviour = DraggableBehaviour.Standard;
            if (slotNum ==0 )
            {
                m_Root.RegisterCallback<PointerEnterEvent>(OnMouseEnter);
                m_Root.RegisterCallback<PointerLeaveEvent>(OnMouseLeave);
            }
            //this.RegisterCallback<MouseUpEvent>(onClick);
        }

        protected bool registerUI()
        {

            //Search the root for the SlotContainer Visual Element
            m_Root = thisBagSlotItem.Q<VisualElement>("slot-container");
            m_hover = thisBagSlotItem.Q<VisualElement>("hover-icon");
            m_itemIcon = thisBagSlotItem.Query<VisualElement>("slot-icon");
            return true;
        }

        public virtual void OnMouseEnter(PointerEnterEvent eventData)
        {
#if !AT_MOBILE             
            MouseEntered = true;
#endif            
            // if (eventData.pointerDrag != null)
            // {
            //     UGUIAtavismActivatable dActivatable = eventData.pointerDrag.GetComponent<UGUIAtavismActivatable>();
            //     if (dActivatable != null)
            //         if (dActivatable.Source == this)
            //         {
            //             if (hover != null)
            //             {
            //                 hover.enabled = true;
            //                 hover.color = AtavismSettings.Instance.itemDropColorFalse;
            //             }
            //         }
            //         else
            //         {
            //             if (uiActivatable != null)
            //             {
            //                 if (uiActivatable.hover != null)
            //                 {
            //                     uiActivatable.hover.enabled = true;
            //                     uiActivatable.hover.color = AtavismSettings.Instance.itemDropColorTrue;
            //                 }
            //             }
            //             else
            //             {
            //                 if (hover != null)
            //                 {
            //                     hover.enabled = true;
            //                     hover.color = AtavismSettings.Instance.itemDropColorTrue;
            //                 }
            //             }
            //         }
            // }
        }

        public virtual void OnMouseLeave(PointerLeaveEvent eventData)
        {
            // if (uiActivatable != null)
            // {
            //     if (uiActivatable.hover != null)
            //         uiActivatable.hover.enabled = false;
            // }
            // if (hover != null)
            //     hover.enabled = false;
#if !AT_MOBILE             
            MouseEntered = false;
#endif            
        }
       
     


     

        // Update is called once per frame
        public void Update()
        {
            if (uiActivatable != null)
                uiActivatable.update();
            // if (Input.GetKeyDown(activateKey) && !ClientAPI.UIHasFocus())
            // {
            //     Activate();
            // }
            if (this.bag == null)
            {
                if (m_itemIcon != null)
                    m_itemIcon.SetEnabled(false);

            }

        }

        public void UpdateBagData(Bag bag, UIAtavismBag bagPanel, UIAtavismInventory inventory)
        {
            this.inventory = inventory;
            this.bag = bag;
            this.bagPanel = bagPanel;
            
         //   Debug.LogError("Bag slot first bug? "+slotNum+" "+(bag != null ? bag.itemTemplate:"null"));
            
            if (bag == null)
            {
                if (uiActivatable != null)
                {
                   // uiActivatable.Clear();
                    if (m_itemIcon != null)
                        m_itemIcon.SetEnabled(false);
                    uiActivatable.m_Root.RemoveFromHierarchy();
                    uiActivatable = null;
                }
            }
            else
            {
                if (bag.itemTemplate == null)
                {
                 //   Debug.LogError("Bag slot first bug? "+slotNum+" "+bag.name);
                    // Do nothing, hard coded first bag?
                    if (m_itemIcon != null)
                    {
                        m_itemIcon.SetEnabled(true);
                        m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultBagIcon.texture;
                    }

                }
                else if (uiActivatable == null)
                {
                    if (m_itemIcon != null)
                    {
                        m_itemIcon.SetEnabled(true);
                        if (bag.itemTemplate.Icon != null)
                            m_itemIcon.style.backgroundImage = bag.itemTemplate.Icon.texture;
                        else
                            m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                        // m_itemIcon.sprite = bag.itemTemplate.icon;
                        //m_itemIcon.material = materialGray;
                    }

                    if (AtavismSettings.Instance.inventoryItemPrefab != null)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        //thisUIAtavismActivatable.Startup();
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                    }
                    else
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        //thisUIAtavismActivatable.Startup();
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                    }

                     uiActivatable.SetActivatable(bag.itemTemplate, ActivatableType.Bag, this);
                }
                else
                {
                    uiActivatable.SetActivatable(bag.itemTemplate, ActivatableType.Bag, this);
                }
            }
        }

        public override void OnMouseEnter(MouseEnterEvent eventData)
        {
            MouseEntered = true;
            UIAtavismActivatable dActivatable = DragDropManager.CurrentlyDraggedObject;

            if (dActivatable != null)
            {
                if (dActivatable != null)
                    if (dActivatable.Source == this)
                    {
                        if (hover != null)
                        {
                            hover.SetEnabled(true);
                            //hover.color = AtavismSettings.Instance.itemDropColorFalse;
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
                                //hover.color = AtavismSettings.Instance.itemDropColorTrue;
                            }
                        }
                    }
            }
        }

        public override void OnMouseLeave(MouseLeaveEvent evt)
        {
            if (uiActivatable != null)
            {
                if (uiActivatable.hover != null)
                    uiActivatable.hover.SetEnabled(false);
            }
            if (hover != null)
                hover.SetEnabled(false);
            MouseEntered = false;
        }

        public override void OnDrop(DropEvent eventData)
        {
            if (!allowOverwrite)
                return;
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject;
            if (droppedActivatable == null)
                return;
            // Reject any references, temporaries or non item/bag slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference ||
                droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary || droppedActivatable.Link != null ||
                (droppedActivatable.ActivatableType != ActivatableType.Item && droppedActivatable.ActivatableType != ActivatableType.Bag))
            {
                return;
            }

            if (droppedActivatable.ActivatableType == ActivatableType.Item)
            {
                Inventory.Instance.PlaceItemAsBag((AtavismInventoryItem)droppedActivatable.ActivatableObject, slotNum);
            }
            else if (droppedActivatable.ActivatableType == ActivatableType.Bag)
            {
                Inventory.Instance.MoveBag(droppedActivatable.Source.slotNum, slotNum);
                if (hover != null)
                    hover.SetEnabled(false);
            }
            droppedActivatable.PreventDiscard();
        }

        public override void ClearChildSlot()
        {
            uiActivatable = null;
            bag = null;
        }

        // public void OnClick()
        // {
        //     Activate();
        // }

        new void Activate()
        {
            if (bag == null)
                return;
            bagPanel.gameObject.SetActive(!bagPanel.gameObject.activeSelf);
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
           // if (cor != null)
          //      StopCoroutine(cor);
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
            //        cor = StartCoroutine(CheckOver());
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }



}