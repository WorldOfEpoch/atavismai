using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismResourceLootListEntry 
    {

        public Label itemNameText;
        public VisualElement itemIcon;
        public Label countText;
        ResourceItem resource;
        public VisualElement itemQuality;
        public VisualElement uiRoot;

        public void SetVisualElement(VisualElement visualElement)
        {
            uiRoot = visualElement;
            
            itemNameText = visualElement.Q<Label>("item-name");
            itemIcon = visualElement.Q<VisualElement>("item-icon");
            countText = visualElement.Q<Label>("count-label");
            itemQuality = visualElement.Q<VisualElement>("item-quality");
            uiRoot.RegisterCallback<ClickEvent>(LootEntryClicked);
#if !AT_MOBILE            
            uiRoot.RegisterCallback<MouseEnterEvent>(
                e =>
                {
                    MouseEntered = true;
                });
            uiRoot.RegisterCallback<MouseLeaveEvent>(
                e =>
                {
                    MouseEntered = false;
                });
#endif 
        }

        
        public void LootEntryClicked(ClickEvent evt)
        {
            Crafting.Instance.LootResource(resource.item);
        }

        public void SetResourceLootEntryDetails(ResourceItem resourceItem)
        {
            this.resource = resourceItem;
            if (itemNameText != null)
            {
#if AT_I2LOC_PRESET
                this.itemNameText.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + resource.item.name);
#else
                this.itemNameText.text = resource.item.name;
#endif
                this.itemNameText.style.color = AtavismSettings.Instance.ItemQualityColor(resource.item.quality);
            }
         
            if (itemQuality != null)
                itemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(resource.item.quality);
            if (resource.item.Icon != null)
                this.itemIcon.style.backgroundImage = resource.item.Icon.texture;
            else
                this.itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;

          //  this.itemIcon.sprite = resource.item.icon;
            if (countText != null)
            {
                if (resource.count > 1)
                {
                    if (countText != null)
                        this.countText.text = resource.count.ToString();
                }
                else
                {
                    if (countText != null)
                        this.countText.text = "";
                }
            }
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        public bool MouseEntered
        {
            set
            {
                if (value)
                {
                    resource.item.ShowUITooltip(uiRoot);
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}