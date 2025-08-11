using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;


namespace Atavism.UI
{
    public class UIAtavismCraftingSlot : UIAtavismDraggableSlot
    {

        Button button;
        Label cooldownLabel;
        CraftingComponent component;
        bool mouseEntered = false;
        
        VisualElement itemIcon;
        VisualElement itemQuality;
        Label countText;
        private VisualElement m_Root;

        // Use this for initialization
        void Start()
        {
            slotBehaviour = DraggableBehaviour.Temporary;
        }
        public void SetVisualElement(VisualElement visualElement)
        {
            slotBehaviour = DraggableBehaviour.Temporary;
          //  Debug.LogError("SetVisualElement ");
          m_Root = visualElement.Q<VisualElement>("slot-container");
          m_Root.userData = this;
            // itemNameText = visualElement.Q<Label>("item-name");
            itemIcon = visualElement.Q<VisualElement>("icon");
            countText = visualElement.Q<Label>("count");
            itemQuality = visualElement.Q<VisualElement>("quality");
            // uiRoot.RegisterCallback<ClickEvent>(LootEntryClicked);
#if !AT_MOBILE            
            m_Root.RegisterCallback<MouseEnterEvent>(
                e =>
                {
                    MouseEntered = true;
                });
            m_Root.RegisterCallback<MouseLeaveEvent>(
                e =>
                {
                    MouseEntered = false;
                });
#endif 
         //   Debug.LogError("SetVisualElement End");
        }
        public void UpdateCraftingSlotData(CraftingComponent component)
        {
            // Debug.LogError("UpdateCraftingSlotData "+slotNum+" "+ component);
            
            this.component = component;
            if (component == null)
            {
                if (uiActivatable != null)
                {

                        // Debug.LogWarning("UIAtavismCraftingSlot "+slotNum+" remove uiActivatable "+(uiActivatable.LinkedVisualElement.userData is  UIAtavismCraftingSlot?(uiActivatable.LinkedVisualElement.userData as UIAtavismCraftingSlot).slotNum+"":"" ));

                        uiActivatable.m_Root.RemoveFromHierarchy();
                        uiActivatable = null;
                        ;
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
                uiActivatable.SetActivatable(component.item, ActivatableType.Item, this);
            }
        }
        public void UpdateCraftingBookSlotData(CraftingComponent component)
        {
            this.component = component;
            if (component == null)
            {
                if (itemIcon != null) 
                    itemIcon.HideVisualElement();
                if (itemQuality != null){
                    itemQuality.HideVisualElement();
                    }
                if (countText != null)
                    countText.text = "";
            }
            else
            {
                if (itemIcon != null)
                {
                        itemIcon.ShowVisualElement();
                        itemIcon.style.backgroundImage = component.item.Icon.texture;
                }
                if (countText != null)
                {
                    if (component.count > 1)
                        countText.text = component.count.ToString();
                    else
                        countText.text = "";
                }
                if (itemQuality != null)
                {
                    itemQuality.ShowVisualElement();
                    itemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(component.item.quality);
                }
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
            // Debug.LogError("UIAtavismCraftingSlot.OnDrop() called");
            if (!allowOverwrite)
                return;
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject; 
            if (droppedActivatable == null)
            {
                return;
            }
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

            if (uiActivatable != null && uiActivatable != newActivatable)
            {
                // Delete existing child
                // DestroyImmediate(uguiActivatable.gameObject);
                if (backLink != null)
                {
                    backLink.SetLink(null);
                }
            }
            else if (uiActivatable == newActivatable)
            {
                droppedOnSelf = true;
            }

            // If the source was a temporary slot, clear it
            if (newActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary)
            {
                newActivatable.Source.ClearChildSlot(false);
                    if (uiActivatable == null)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Root.Add(uiActivatable.m_Root);
                    }
                uiActivatable.SetActivatable(newActivatable.ActivatableObject, ActivatableType.Item, this);
                backLink = newActivatable.Source.BackLink;
              //  newActivatable.Source.Discarded();
            }
            else
            {
                // Create a duplicate
             
                uiActivatable = new UIAtavismActivatable(m_Root);
                uiActivatable.m_Root.AddToClassList("activatableContainer");
                m_Root.Add(uiActivatable.m_Root);
              
                uiActivatable.SetActivatable(newActivatable.ActivatableObject, ActivatableType.Item, this);

                // Set the back link
                backLink = newActivatable;
                newActivatable.SetLink(uiActivatable);
            }

            newActivatable.SetDropTarget(this);
            Crafting.Instance.SetGridItem(slotNum, (AtavismInventoryItem)newActivatable.ActivatableObject, true);
            AtavismEventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", new string[]{});
        }

        public override void ClearChildSlot(bool send)
        {
            // if (uiActivatable != null)
            //     uiActivatable.m_Root.RemoveFromHierarchy();
            Crafting.Instance.SetGridItem(slotNum, null, send);
        }

        public override void Discarded()
        {
            if (droppedOnSelf)
            {
                droppedOnSelf = false;
                return;
            }
            // if (uiActivatable != null)
            //     DestroyImmediate(uiActivatable.gameObject);
            if (uiActivatable != null)
            {
                // Debug.LogWarning("UIAtavismCraftingSlot "+slotNum+" remove uiActivatable "+(uiActivatable.m_Root.userData as UIAtavismCraftingSlot).slotNum );
                uiActivatable.m_Root.RemoveFromHierarchy();
                uiActivatable = null;
            }

            if (backLink != null)
            {
                backLink.SetLink(null);
            }
            backLink = null;
            ClearChildSlot(true);
        }

        public override void Activate()
        {
            // Unlink item?
            Discarded();
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
            // if (cor != null)
            //     StopCoroutine(cor);
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
                if (mouseEntered && component != null && component.item != null)
                {
                    if (uiActivatable != null)
                        uiActivatable.ShowTooltip(m_Root);
                    else
                        component.item.ShowUITooltip(m_Root);
                    //  cor = StartCoroutine(CheckOver());
                }
                else
                {
                    HideTooltip();
                }
            }
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