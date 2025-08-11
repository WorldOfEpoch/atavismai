using Atavism;
using UnityEngine.UIElements;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class LootListEntryUXML : VisualElement
{
    public new class UxmlFactory : UxmlFactory<LootListEntryUXML, UxmlTraits> { }

    public Label itemNameLabel;
    public Label countLabel;
    public VisualElement itemQualityElement;
    public Image itemIcon;
    public AtavismInventoryItem item;
    public Currency curr;
    public Image itemQuality;

    public LootListEntryUXML()
    {
        // Load or create your UI hierarchy here, and assign the elements
        itemNameLabel = this.Q<Label>("ItemName"); // Assuming you have a label with name 'ItemName' in your UXML
        countLabel = this.Q<Label>("CountLabel"); // Assuming you have a label with name 'CountLabel' in your UXML
        itemQualityElement = this.Q<VisualElement>("ItemQuality"); // Assuming you have a visual element with name 'ItemQuality' in your UXML
        itemIcon = this.Q<Image>("ItemIcon"); // Assuming you have an image with name 'ItemIcon' in your UXML

        RegisterCallback<PointerEnterEvent>(evt => OnPointerEnter());
        RegisterCallback<PointerLeaveEvent>(evt => OnPointerExit());
        this.AddManipulator(new Clickable(() => LootEntryClicked()));
        RegisterCallback<MouseEnterEvent>(e => ShowTooltip());
        RegisterCallback<MouseLeaveEvent>(e => HideTooltip());

    }

    public void OnPointerEnter()
    {
        MouseEntered = true;
    }

    public void OnPointerExit()
    {
        MouseEntered = false;
    }


    public void LootEntryClicked()
    {
        if (item != null)
            NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootItem " + item.ItemId);

        if (curr != null)
            NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootItem " + curr.id);
    }

    public void SetLootEntryDetails(AtavismInventoryItem item)
    {
        this.item = item;
        this.curr = null;

        itemNameLabel.text = item.name;
        itemIcon.sprite = item.Icon != null ? item.Icon : AtavismSettings.Instance.defaultItemIcon;
        countLabel.text = item.Count > 1 ? item.Count.ToString() : "";

        itemQualityElement.style.backgroundColor = AtavismSettings.Instance.ItemQualityColor(item.Quality);
    }

    // ... inside LootListEntryUXML class

    public void BindData(AtavismInventoryItem item, Currency currency)
    {
        // Reset current data
        this.item = item;
        this.curr = currency;

        // Bind item data if it exists
        if (item != null)
        {
            itemNameLabel.text = item.name;
            itemIcon.sprite = item.Icon != null ? item.Icon : AtavismSettings.Instance.defaultItemIcon;
            countLabel.text = item.Count > 1 ? item.Count.ToString() : "";
            itemQualityElement.style.backgroundColor = AtavismSettings.Instance.ItemQualityColor(item.Quality);
        }
        // Bind currency data if it exists
        else if (currency != null)
        {
            itemNameLabel.text = currency.name;
            itemIcon.sprite = currency.Icon != null ? currency.Icon : AtavismSettings.Instance.defaultItemIcon;
            countLabel.text = currency.conversionAmountReq > 1 ? currency.conversionAmountReq.ToString() : "";
            itemQualityElement.style.backgroundColor = AtavismSettings.Instance.ItemQualityColor(1); // Assuming quality 1 for currency
        }

        // If localization is needed, wrap the text assignments with the localization method call
        // For example:
        // itemNameLabel.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name);
        // 
    }


    public void SetLootEntryDetails(int currencyId, int count)
    {
        this.item = null;
        this.curr = Inventory.Instance.GetCurrency(currencyId);

        itemNameLabel.text = curr.name;
        itemIcon.sprite = curr.Icon != null ? curr.Icon : AtavismSettings.Instance.defaultItemIcon;
        countLabel.text = count > 1 ? count.ToString() : "";

        itemQualityElement.style.backgroundColor = AtavismSettings.Instance.ItemQualityColor(1); // Assuming quality 1 for currency
    }

    private void ShowTooltip()
    {
        // Implement tooltip visibility logic here
    }

    private void HideTooltip()
    {
        // Implement tooltip hiding logic here
    }

    public bool MouseEntered
    {
        set
        {
            if (value)
            {
                if (item != null)
                {
                    // Show tooltip logic here
                }
            }
            else
            {
                HideTooltip();
            }
        }
    }
}
