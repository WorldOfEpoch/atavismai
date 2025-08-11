using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismCharacterEquipSlot : UIAtavismDraggableSlot
    {

        public string slotName;
        Button button;
        //public Text countLabel;
        AtavismInventoryItem item;
        bool mouseEntered = false;
        private VisualElement m_Icon;
        private VisualElement m_Root;
        public bool pet = false;
        public int petProfile = -1;
        
        public UIAtavismCharacterEquipSlot()
        {
            slotBehaviour = DraggableBehaviour.Standard;
        }
        public void SetVisualElement(VisualElement val)
        {
    
            m_Root = val;
            m_Icon = val.Q<VisualElement>("slot-icon");


        }
        /// <summary>
        /// Creates a UGUIAtavismActivatable object to put in this slot if the item is not null.
        /// </summary>
        /// <param name="item">Item.</param>
        public void UpdateEquipItemData(AtavismInventoryItem item)
        {
            if (item == null)
            {
                if (uiActivatable != null)
                {
                    uiActivatable.m_Root.RemoveFromHierarchy();
                    uiActivatable = null;
                    if (mouseEntered)
                        HideTooltip();
                }
            }
            else 
            {
                if (this.item != null && ((item.ItemId != null && !item.ItemId.Equals(this.item.ItemId)) || item.ItemId == null))
                {
                    if (uiActivatable != null)
                    {
                        uiActivatable.m_Root.RemoveFromHierarchy();
                        uiActivatable = null;
                    }
                }
                if (uiActivatable == null)
                {
                    if (AtavismSettings.Instance.UIActivatableUXML != null)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Icon);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Icon.Add(uiActivatable.m_Root);
                    }
                    else
                    {
                        uiActivatable = new UIAtavismActivatable(m_Icon);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Icon.Add(uiActivatable.m_Root);
                    }
                }
                uiActivatable.SetActivatable(item, ActivatableType.Item, this);
            }
            this.item = item;
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
            // Apply logic here
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject; 
                //eventData.pointerDrag.GetComponent<UGUIAtavismActivatable>();
            if (droppedActivatable != null)
                if (droppedActivatable.Source == this)
                {
                    droppedActivatable.PreventDiscard();
                    return;
                }

            // Reject any references, temporaries or non item slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference ||
                droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary ||
                droppedActivatable.ActivatableType != ActivatableType.Item)
            {
                return;
            }
            //if (item == null && uguiItem == null) {
            // TODO: perhaps change this to store locally

            //uguiItem.SetDropTarget(this.transform);
            //droppedActivatable.ActivatableObject.Activate();
            if (droppedActivatable.ActivatableObject is AtavismInventoryItem)
            {
                if (pet)
                {
                    Inventory.Instance.EquipItemInPetSlot((AtavismInventoryItem)droppedActivatable.ActivatableObject, slotName, petProfile);
                }
                else
                {
                    Inventory.Instance.EquipItemInSlot((AtavismInventoryItem)droppedActivatable.ActivatableObject, slotName);
                }
            }
            droppedActivatable.PreventDiscard();
            //}
        }

        public override void ClearChildSlot()
        {
            uiActivatable.m_Root.RemoveFromHierarchy();
            uiActivatable = null;
        }

        public override void Activate()
        {
            if (item != null)
                item.Activate();
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
                if (mouseEntered)
                {
                    if (uiActivatable != null)
                    {
                        uiActivatable.ShowTooltip(m_Icon);
                        // cor = StartCoroutine(CheckOver());
                    }
                    else
                    {
                        ShowTooltip();
                    }
                }
                else
                {
                    HideTooltip();
                }
            }
        }
        
        public void Update()
        {
            if (uiActivatable != null)
                uiActivatable.update();

        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
        }
    }
}