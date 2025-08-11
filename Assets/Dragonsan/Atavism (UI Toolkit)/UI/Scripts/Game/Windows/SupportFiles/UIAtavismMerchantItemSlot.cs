using UnityEngine.EventSystems;
using System.Collections;
using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMerchantItemSlot : UIAtavismDraggableSlot
    {

        UIAtavismMerchantPanel merchantFrame;
        UIAtavismShopWindow playerShop;
        MerchantItem merchantItem;
        ShopItem shopItem;

        bool mouseEntered = false;
        // Color defaultColour = Color.white;
        VisualElement m_Root;
        VisualElement itemIcon;
        private VisualElement itemQualityIcon;

        // Use this for initialization
        public UIAtavismMerchantItemSlot()
        {
            slotBehaviour = DraggableBehaviour.SourceOnly;
        }
        public void SetVisualElement(VisualElement visualElement)
        {

            //Search the root for the SlotContainer Visual Element
            m_Root = visualElement;
             itemQualityIcon = visualElement.Q<VisualElement>("quality-icon");
             m_itemIcon = visualElement.Query<VisualElement>("item-icon");
         
        }

        public void UpdateMerchantItemData(MerchantItem merchantItem, UIAtavismMerchantPanel merchantFrame)
        {
            this.merchantItem = merchantItem;
            this.merchantFrame = merchantFrame;
            this.playerShop = null;
            if (merchantItem == null)
            {
                if (uiActivatable != null)
                {
                    m_Root.Remove(uiActivatable.m_Root);
                    uiActivatable = null;
                    if (m_itemIcon != null)
                        m_itemIcon.SetEnabled(false);
                    if (mouseEntered)
                        mouseEntered = false;
                }
            }
            else
            {
                AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(merchantItem.itemID);
                if (uiActivatable == null)
                {
                    if (AtavismSettings.Instance.UIActivatableUXML != null)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Root.Add(uiActivatable.m_Root);
                    }
                    else
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Root.Add(uiActivatable.m_Root);
                    }
                }

                uiActivatable.SetActivatable(item, ActivatableType.Item, this, false);
                itemQualityIcon.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                if (merchantItem.count > 0)
                {
                    if (uiActivatable.CountText != null)
                        uiActivatable.CountText.text = merchantItem.count.ToString();
                }
                else
                {
                    if (uiActivatable.CountText != null)
                        uiActivatable.CountText.text = "";
                                    }
                // if (merchantItem.count == 0)
                // {
                //     uguiActivatable.GetComponent<Image>().color = Color.gray;
                // }
                // else
                // {
                //     uguiActivatable.GetComponent<Image>().color = defaultColour;
                // }
                
                // Set background Image - HACK to still show item when it is being dragged
                if (uiActivatable.Quality != null)
                {
                    uiActivatable.Quality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                }
                if (itemIcon != null)
                {
                    if (item.Icon != null)
                        itemIcon.style.backgroundImage = item.Icon.texture;
                    else
                        itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                }
            }
        }

        public void UpdateShopItemData(ShopItem shopItem, UIAtavismShopWindow playerShop)
        {
            this.shopItem = shopItem;
            this.playerShop = playerShop;
            this.merchantFrame = null;
            if (shopItem == null)
            {
                if (uiActivatable != null)
                {
                    m_Root.Remove(uiActivatable.m_Root);
                    uiActivatable = null;
                }
            }
            else
            {
                AtavismInventoryItem item = shopItem.item;
                    //Inventory.Instance.GetItemByTemplateID(shopItem.itemID);
                 // if (item.Iicon == null)
                 //      item.icon = Inventory.Instance.GetItemByTemplateID(item.templateId).icon;
                if (uiActivatable == null)
                {
                    // item = Inventory.Instance.GetItemByTemplateID(merchantItem.itemID);
                    if (AtavismSettings.Instance.UIActivatableUXML != null)
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Root.Add(uiActivatable.m_Root);
                    }
                    else
                    {
                        uiActivatable = new UIAtavismActivatable(m_Root);
                        uiActivatable.m_Root.AddToClassList("activatableContainer");
                        m_Root.Add(uiActivatable.m_Root);
                    }
                }

                uiActivatable.SetActivatable(item, ActivatableType.Item, this, false);
                itemQualityIcon.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                if (shopItem.count > 0)
                {
                    if (uiActivatable.CountText != null)
                        uiActivatable.CountText.text = shopItem.count.ToString();
                }
                else
                {
                    if (uiActivatable.CountText != null)
                        uiActivatable.CountText.text = "";
                }
                // if (shopItem.count == 0)
                // {
                //     uiActivatable.GetComponent<Image>().color = Color.gray;
                // }
                // else
                // {
                //     uiActivatable.GetComponent<Image>().color = defaultColour;
                // }
                
                // Set background Image - HACK to still show item when it is being dragged
                if (uiActivatable.Quality != null)
                {
                    uiActivatable.Quality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                }
                if (itemIcon != null)
                {
                    if (item.Icon != null)
                        itemIcon.style.backgroundImage = Inventory.Instance.GetItemByTemplateID(item.templateId).Icon.texture;
                    else
                        itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                }
            }
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
//
//         public override void OnDrop(PointerEventData eventData)
//         {
//             // Do nothing
//         }

        public override void ClearChildSlot()
        {
            // if (m_itemIcon != null)
            //     m_itemIcon.SetEnabled(false);
            // // if(uiActivatable!=null)
            // // m_Root.Remove(uiActivatable.m_Root);
            // uiActivatable = null;
        }

        public override void Discarded()
        {

        }

        public override void Activate()
        {
            if (shopItem != null)
            {
                string costString = "";
                if (shopItem.itemOID == null)
                {
                    int cItem = Inventory.Instance.GetCountOfItem(shopItem.itemID);
                    if (cItem == 0)
                    {
                        return;
                    }
                }
            }

            //if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (playerShop != null && shopItem.itemOID==null && playerShop.ShowPurchaseCountPanel(this, shopItem))
                    return;

                if (merchantFrame!=null && merchantFrame.ShowPurchaseCountPanel(this, merchantItem))
                    return;
            }
            StartPurchase(1);
        }

        public void StartPurchase(int count)
        {
            string confirmationString = "";
            if (merchantItem != null)
            {

                MerchantItem mItem = new MerchantItem();
                mItem.itemID = merchantItem.itemID;
                mItem.count = count;
                AtavismInventoryItem aiItem = Inventory.Instance.GetItemByTemplateID(merchantItem.itemID);
                string costString = Inventory.Instance.GetCostString(merchantItem.purchaseCurrency, merchantItem.cost * mItem.count);
                
                if (mItem.count == 1)
#if AT_I2LOC_PRESET
        confirmationString = I2.Loc.LocalizationManager.GetTranslation("Purchase1Item") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + aiItem.name) + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?";
#else
                    confirmationString = "Purchase " + aiItem.name + " for " + costString + "?";
#endif
                else
#if AT_I2LOC_PRESET
	    confirmationString = I2.Loc.LocalizationManager.GetTranslation("PurchaseXItems") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + aiItem.name) + " (x" + mItem.count + ") " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?";
#else
                    confirmationString = "Purchase " + aiItem.name + " (x" + mItem.count + ") for " + costString + "?";
#endif
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(confirmationString, mItem, NpcInteraction.Instance.PurchaseItemConfirmed);
            }




            if (shopItem != null)
            {
                string costString = "";
                if (shopItem.itemOID == null)
                {
                    int cItem = Inventory.Instance.GetCountOfItem(shopItem.itemID);
                    if (cItem == 0)
                    {

                        return;
                    }
                    shopItem.sellCount = count;
                    costString = Inventory.Instance.GetCostString(shopItem.purchaseCurrency, shopItem.cost * shopItem.sellCount);

                    if (shopItem.sellCount == 1)
                    {
#if AT_I2LOC_PRESET
        confirmationString = I2.Loc.LocalizationManager.GetTranslation("Sell") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + shopItem.item.name) + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?";
#else
                        confirmationString = "Sell " + shopItem.item.name + " for " + costString + "?";
#endif
                    }
                    else
                    {

#if AT_I2LOC_PRESET
	    confirmationString = I2.Loc.LocalizationManager.GetTranslation("Sell") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + shopItem.item.name) + " (x" + shopItem.sellCount + ") " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?";
#else
                        confirmationString = "Sell " + shopItem.item.name + " (x" + shopItem.sellCount + ") for " + costString + "?";
#endif
                    }


                }
                else
                {
                    costString = Inventory.Instance.GetCostString(shopItem.purchaseCurrency, shopItem.cost);

                    if (shopItem.count == 1)
                    {

#if AT_I2LOC_PRESET
        confirmationString = I2.Loc.LocalizationManager.GetTranslation("Purchase1Item") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + shopItem.item.name) + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?";
#else
                        confirmationString = "Purchase " + shopItem.item.name + " for " + costString + "?";
#endif
                    }
                    else
                    {

#if AT_I2LOC_PRESET
	    confirmationString = I2.Loc.LocalizationManager.GetTranslation("PurchaseXItems") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + shopItem.item.name) + " (x" + shopItem.count + ") " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?";
#else
                        confirmationString = "Purchase " + shopItem.item.name + " (x" + shopItem.count + ") for " + costString + "?";
#endif
                    }
                }
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(confirmationString, shopItem, playerShop.PurchaseItemConfirmed);
            }
        }

        protected override void ShowTooltip()
        {
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