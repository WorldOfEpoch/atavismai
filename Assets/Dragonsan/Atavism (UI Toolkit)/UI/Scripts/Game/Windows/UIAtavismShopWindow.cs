using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Atavism;

namespace Atavism.UI
{
    
    public class UIAtavismShopWindow : UIAtavismWindowBase
    {
        [SerializeField] private int numberSlots = 10;
        [SerializeField] private VisualTreeAsset itemSlotTemplate;
        private UIButtonToggleGroup menu;
        public List<UIAtavismMerchantItemEntry> entries= new List<UIAtavismMerchantItemEntry>();
        public Button prevButton;
        public Button nextButton;
        public Label currentPageText;
        [SerializeField] UIAtavismCurrencyDisplay mainCurrency;
        public VisualElement purchaseCountPanel;
        public UITextField purchaseCountText;
        public Button minusButton;
        public Button plusButton;
        public VisualElement itemList;
        
        private Button cancelButton;
        private Button purchaseButton;
        
        int multiplePurchaseCount = 1;
        int currentPage = 0;
        OID NpcId;
        bool sellMode = true;
        
        [SerializeField] VisualElement panel;
        [SerializeField] Label itemName;
        [SerializeField] UIAtavismCurrencyDisplay sumCost;

        public UIAtavismInventory inventory;

        
        
        
        // [SerializeField] List<SkillTypeButton> tabButtons = new List<SkillTypeButton>();
        // [SerializeField] Color buttonMenuSelectedColor = Color.green;
        // [SerializeField] Color buttonMenuNormalColor = Color.white;
        // [SerializeField] Color buttonMenuSelectedTextColor = Color.black;
        // [SerializeField] Color buttonMenuNormalTextColor = Color.black;
        // Use this for initialization
        protected override  void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("SHOP_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CLOSE_SHOP", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
            // showBuy();
        }

        protected override  void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("SHOP_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_SHOP", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
        }

           protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
          //  Debug.LogError("UIAtavismShopWindow registerUI");
            VisualElement inner_panel1 = uiWindow.Q<VisualElement>("inner-panel");
            menu = inner_panel1.Query<UIButtonToggleGroup>("menu");
            menu.OnItemIndexChanged += TopMenuChange;
            
         
            prevButton = uiWindow.Q<Button>("prev-button");
            nextButton = uiWindow.Q<Button>("next-button");
            prevButton.clicked += ShowPrevPage;
            nextButton.clicked += ShowNextPage;
            
            currentPageText= uiWindow.Q<Label>("page-label");
            itemList =  uiWindow.Q<VisualElement>("item-list");

           
            purchaseCountPanel =  uiWindow.Q<VisualElement>("count-panel");

            itemName = purchaseCountPanel.Q<Label>("item-name");
            purchaseCountText = purchaseCountPanel.Q<UITextField>("item-count");
            minusButton = purchaseCountPanel.Q<Button>("minus-button");
            plusButton = purchaseCountPanel.Q<Button>("plus-button");
            minusButton.clicked += ReduceMultipleCount;
            plusButton.clicked += IncreaseMultipleCount;

            
            VisualElement currency  = purchaseCountPanel.Q<VisualElement>("currency-sum");
            sumCost = new UIAtavismCurrencyDisplay();
            sumCost.SetVisualElement(currency);
            sumCost.ReverseOrder = true;
            // sumCost  = purchaseCountPanel.Q<VisualElement>("count-panel");
            cancelButton = purchaseCountPanel.Q<Button>("cancel-button");
            purchaseButton = purchaseCountPanel.Q<Button>("purchase-button");
            cancelButton.clicked += CancelPurchase;
            purchaseButton.clicked += PurchaseMultiple;
            entries.Clear();
            itemList.Clear();
            VisualElement main_currency  = uiWindow.Q<VisualElement>("currency");
            mainCurrency = new UIAtavismCurrencyDisplay();
            mainCurrency.SetVisualElement(main_currency);
            mainCurrency.ReverseOrder = true;
            mainCurrency.MainCurrency();
            for (int i = 0; i < numberSlots; i++)
            {
                UIAtavismMerchantItemEntry  script = new UIAtavismMerchantItemEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = itemSlotTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = script;
                // Initialize the controller script
                script.SetVisualElement(newListEntry);
                entries.Add(script);
                itemList.Add(newListEntry);
            }

          //  Debug.LogError("UIAtavismShopWindow registerUI End");
            return true;
        }

        private void TopMenuChange(int obj)
        {
            switch (obj)
            {
                case 0:
                    showBuy();
                    break;
                case 1:
                    showSell();
                    break;
            }
        }


        public override void Show()
        {
            base.Show();
            menu.Set(0,true);
            // AtavismSettings.Instance.OpenWindow(this);
            if (!inventory.IsVisible)
            {
                if (uiWindow.resolvedStyle.width == 0 )
                {
                    uiWindow.RegisterCallback<GeometryChangedEvent>(onGeometryChangedShow);
                }
                else
                {
                    float left = uiWindow.resolvedStyle.left + uiWindow.resolvedStyle.width + 5;
                    float top = uiWindow.resolvedStyle.top;
                    inventory.SetPosition(left, top);
                    inventory.Show();
                }
            }
            currentPage = 0;
    
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MERCHANT_UI_OPENED", args);
            // AtavismUIUtility.BringToFront(gameObject);
        }
        private void onGeometryChangedShow(GeometryChangedEvent evt)
        {
            float left = uiWindow.resolvedStyle.left + uiWindow.resolvedStyle.width + 5;
            float top = uiWindow.resolvedStyle.top;
            inventory.SetPosition(left, top);
            inventory.Show();
            uiWindow.UnregisterCallback<GeometryChangedEvent>(onGeometryChangedShow);
        }
        public override void Hide()
        {
            base.Hide();
         //   AtavismSettings.Instance.CloseWindow(this);
            if (purchaseCountPanel != null)
            {
                purchaseCountPanel.HideVisualElement();
            }
        }

        public void ShowPrevPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
            }
            UpdatePlayerShop();
        }

        public void ShowNextPage()
        {
            currentPage++;
            UpdatePlayerShop();
        }
        public void showBuy()
        {
            sellMode = true;
            // foreach (SkillTypeButton stb in tabButtons)
            // {
            //     if (stb.typeId == 1)
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuSelectedColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuSelectedTextColor;
            //     }
            //     else
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuNormalColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuNormalTextColor;
            //     }
            // }
            UpdatePlayerShop();
        }

        public void showSell()
        {
            sellMode = false;
            // foreach (SkillTypeButton stb in tabButtons)
            // {
            //     if (stb.typeId == 0)
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuSelectedColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuSelectedTextColor;
            //     }
            //     else
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuNormalColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuNormalTextColor;
            //     }
            // }
            UpdatePlayerShop();
        }
        void UpdatePlayerShop()
        {
            mainCurrency.MainCurrency();
            if (sellMode)
            {

                for (int i = 0; i < entries.Count; i++)
                {
                    if ((currentPage * entries.Count) + i < AtavismPlayerShop.Instance.PlayerShopSellItems.Count)
                    {
                        entries[i].Show();
                        entries[i].UpdateShopItemData(AtavismPlayerShop.Instance.GetSellItem((currentPage * entries.Count) + i), this);
                    }
                    else
                    {
                        entries[i].Hide();
                    }
                }
                if (currentPageText != null)
                    if (AtavismPlayerShop.Instance.PlayerShopSellItems.Count > entries.Count)
                        currentPageText.text = (currentPage + 1) + " / " + ((AtavismPlayerShop.Instance.PlayerShopSellItems.Count % entries.Count) > 0 ? (AtavismPlayerShop.Instance.PlayerShopSellItems.Count / entries.Count + 1).ToString() : (AtavismPlayerShop.Instance.PlayerShopSellItems.Count / entries.Count).ToString());
                    else
                        currentPageText.text = "";
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if ((currentPage * entries.Count) + i < AtavismPlayerShop.Instance.PlayerShopBuyItems.Count)
                    {
                        entries[i].Show();
                        entries[i].UpdateShopItemData(AtavismPlayerShop.Instance.GetBuyItem((currentPage * entries.Count) + i), this);
                    }
                    else
                    {
                        entries[i].Hide();
                    }
                }
                if (currentPageText != null)
                    if (AtavismPlayerShop.Instance.PlayerShopBuyItems.Count > entries.Count)
                        currentPageText.text = (currentPage + 1) + " / " + ((AtavismPlayerShop.Instance.PlayerShopBuyItems.Count % entries.Count) > 0 ? (AtavismPlayerShop.Instance.PlayerShopBuyItems.Count / entries.Count + 1).ToString() : (AtavismPlayerShop.Instance.PlayerShopBuyItems.Count / entries.Count).ToString());
                    else
                        currentPageText.text = "";
            }
            // Update visibility of prev and next buttons
            if (currentPage > 0)
            {
                if (prevButton!=null)
                    prevButton.visible = true;
            }
            else
            {
                if (prevButton!=null)
                    prevButton.visible = false;
            }
            if (sellMode)
            {
                if (AtavismPlayerShop.Instance.PlayerShopSellItems.Count > (currentPage + 1) * entries.Count)
                {
                    if (nextButton!=null)
                        nextButton.visible = true;
                }
                else
                {
                    if (nextButton!=null)
                        nextButton.visible = false;
                }
            }
            else
            {
                if (AtavismPlayerShop.Instance.PlayerShopBuyItems.Count > (currentPage + 1) * entries.Count)
                {
                    if (nextButton!=null)
                        nextButton.visible = true;
                }
                else
                {
                    if (nextButton!=null)
                        nextButton.visible = false;
                }
            }
            if (purchaseCountPanel != null)
            {
                purchaseCountPanel.HideVisualElement();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "SHOP_UPDATE")
            {
                if (!showing)
                    Show();
               
                   currentPage = 0;

                if (AtavismPlayerShop.Instance.PlayerShopBuyItems.Count > 0)
                    showSell();
                else
                    showBuy();
                UpdatePlayerShop();
            }
            else if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (showing)
                    UpdatePlayerShop();
            }
            else if (eData.eventType == "CLOSE_SHOP")
            {
                Hide();
                
            }
            if (eData.eventType == "CURRENCY_UPDATE" || eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                UpdateCurrencies();
            }
        }
        void UpdateCurrencies()
        {
            mainCurrency.MainCurrency();
            mainCurrency.Show();
        }
        ShopItem shopItem;

        public bool ShowPurchaseCountPanel(UIAtavismMerchantItemSlot mItemEntry, ShopItem shopItem)
        {
            this.shopItem = shopItem;
            if (purchaseCountPanel != null)
            {
                purchaseCountPanel.ShowVisualElement();
                multiplePurchaseCount = 1;
                if (purchaseCountText != null)
                {
                    purchaseCountText.value = multiplePurchaseCount.ToString();
                }
                if (minusButton != null)
                    minusButton.visible = false;
                //	selectedPurchaseItem = mItemEntry;
                AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(shopItem.itemID);
                
                if (itemName != null && aii != null)
                {
#if AT_I2LOC_PRESET
                itemName.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name)) ? aii.name : I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name);
#else
                    itemName.text = aii.name;
#endif
                }
                sumCost.SetData(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
                // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
                // for (int i = 0; i < sumCost.Length; i++)
                // {
                //     if (i < currencyDisplayList.Count)
                //     {
                //         sumCost[i].gameObject.SetActive(true);
                //         sumCost[i].SetCurrencyDisplayData(currencyDisplayList[currencyDisplayList.Count - i - 1]);
                //     }
                //     else
                //     {
                //         sumCost[i].gameObject.SetActive(false);
                //     }
                // }
                return true;
            }
            return false;
        }

        public void ReduceMultipleCount()
        {
            multiplePurchaseCount--;
            if (multiplePurchaseCount < 2)
            {
                multiplePurchaseCount = 1;
                if (minusButton != null)
                    minusButton.visible = false;
            }
            if (purchaseCountText != null)
            {
                purchaseCountText.value = multiplePurchaseCount.ToString();
            }
            sumCost.SetData(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
            // for (int i = 0; i < sumCost.Length; i++)
            // {
            //     if (i < currencyDisplayList.Count)
            //     {
            //         sumCost[i].gameObject.SetActive(true);
            //         sumCost[i].SetCurrencyDisplayData(currencyDisplayList[currencyDisplayList.Count - i - 1]);
            //     }
            //     else
            //     {
            //         sumCost[i].gameObject.SetActive(false);
            //     }
            // }

        }

        public void IncreaseMultipleCount()
        {
            multiplePurchaseCount++;

            int cItem = Inventory.Instance.GetCountOfItem(shopItem.itemID);
            if (multiplePurchaseCount > cItem)
            {
                multiplePurchaseCount = cItem;
            }

            if (minusButton != null)
                minusButton.visible = true;
            if (purchaseCountText != null)
            {
                purchaseCountText.value = multiplePurchaseCount.ToString();
            }
            sumCost.SetData(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
            // for (int i = 0; i < sumCost.Length; i++)
            // {
            //     if (i < currencyDisplayList.Count)
            //     {
            //         sumCost[i].gameObject.SetActive(true);
            //         sumCost[i].SetCurrencyDisplayData(currencyDisplayList[currencyDisplayList.Count - i - 1]);
            //     }
            //     else
            //     {
            //         sumCost[i].gameObject.SetActive(false);
            //     }
            // }

        }

        public void UpdatePurchaseCount()
        {
            if (purchaseCountText != null)
            {
                if (purchaseCountText.text != "")
                    multiplePurchaseCount = int.Parse(purchaseCountText.text);
                else
                    multiplePurchaseCount = 0;
                if (minusButton != null)
                    if (multiplePurchaseCount > 1)
                    {
                            minusButton.visible = true;
                    }
                    else
                    {
                            minusButton.visible = false;
                    }
            }

            sumCost.SetData(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(shopItem.purchaseCurrency, shopItem.cost * multiplePurchaseCount);
            // for (int i = 0; i < sumCost.Length; i++)
            // {
            //     if (i < currencyDisplayList.Count)
            //     {
            //         sumCost[i].gameObject.SetActive(true);
            //         sumCost[i].SetCurrencyDisplayData(currencyDisplayList[currencyDisplayList.Count - i - 1]);
            //     }
            //     else
            //     {
            //         sumCost[i].gameObject.SetActive(false);
            //     }
            // }
        }

        public void PurchaseMultiple()
        {
            if (purchaseCountText != null)
            {
                //			int count = int.Parse(purchaseCountText.text);
                ShopItem mItem = new ShopItem();
                mItem.itemID = shopItem.itemID;
                mItem.sellCount = int.Parse(purchaseCountText.text);
                PurchaseItemConfirmed(mItem, true);
               shopItem = null;
                if (purchaseCountPanel!=null)
                    purchaseCountPanel.HideVisualElement();
            }
        }

        public void CancelPurchase()
        {
            if (purchaseCountPanel != null)
            {
                if (purchaseCountPanel!=null)
                    purchaseCountPanel.HideVisualElement();
            }
            shopItem = null;
        }

    
        public void PurchaseItemConfirmed(object item, bool response)
        {
         //   Debug.LogError("PurchaseItemConfirmed  ");
            if (!response)
                return;

            ShopItem mItem = (ShopItem)item;
            
       
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("shop", AtavismPlayerShop.Instance.Shop);
            if (mItem.itemOID != null)
            {
                props.Add("ItemId", -1);
                props.Add("ItemOid", mItem.itemOID);
                props.Add("ItemCount",-1);
            }
            else
            {
                props.Add("ItemId", mItem.itemID);
                props.Add("ItemOid", OID.fromLong(0L));
                props.Add("ItemCount", mItem.sellCount);
            }
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.PLAYER_SHOP_BUY", props);
         //   Debug.LogError("PurchaseItemConfirmed  END");
        }
    }
}