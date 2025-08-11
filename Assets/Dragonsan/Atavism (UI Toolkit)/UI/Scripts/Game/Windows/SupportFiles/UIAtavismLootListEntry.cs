using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismLootListEntry 
    {

        public Label itemNameText;
        public VisualElement itemIcon;
        public Label countText;
        public VisualElement itemQuality;
        public VisualElement uiRoot;

        AtavismInventoryItem item;
        Currency curr;

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
            if(item!=null)
                NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootItem " + item.ItemId);

            if (curr != null)
                NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootItem " + curr.id);
        }

        
        
        public void SetLootEntryDetails(AtavismInventoryItem item)
        {
            this.item = item;
            this.curr = null;
#if AT_I2LOC_PRESET
      if (itemNameText != null)  this.itemNameText.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name);
#else
            if (itemNameText != null)
                this.itemNameText.text = item.name;
#endif
            if (item.Icon != null)
                this.itemIcon.style.backgroundImage = item.Icon.texture;
            else
                this.itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;

            // this.itemIcon.sprite = item.icon;
            if (countText != null)
            {
                if (item.Count > 1)
                {
                    this.countText.text = item.Count.ToString();
                }
                else
                {
                    this.countText.text = "";
                }
            }
        

            if (itemQuality != null)
            {
                this.itemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.Quality);
            }
        }

        public void SetLootEntryDetails(int currencyId, int count)
        {
            this.item = null;
            this.curr = Inventory.Instance.GetCurrency(currencyId);
#if AT_I2LOC_PRESET
      if (itemNameText != null)  this.itemNameText.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + curr.name);
#else
            if (itemNameText != null)
                this.itemNameText.text = curr.name;
#endif
            if (curr.Icon != null)
                this.itemIcon.style.backgroundImage = curr.Icon.texture;
            else
                this.itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;

            // this.itemIcon.sprite = item.icon;
            if (countText != null)
            {
                if (count > 1)
                {
                    this.countText.text = count.ToString();
                }
                else
                {
                    this.countText.text = "";
                }
            }

            if (itemQuality != null)
            {
                this.itemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(1);
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
                    if(item!=null)
                        item.ShowUITooltip(uiRoot);
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}