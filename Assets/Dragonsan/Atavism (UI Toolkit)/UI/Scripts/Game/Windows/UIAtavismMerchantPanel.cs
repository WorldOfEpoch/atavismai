using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMerchantPanel : UIAtavismWindowBase
    {
        [SerializeField] private int numberSlots = 10;
        [SerializeField] private VisualTreeAsset itemSlotTemplate;
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

        [SerializeField] VisualElement panel;
        [SerializeField] Label itemName;
        [SerializeField] UIAtavismCurrencyDisplay sumCost;
        MerchantItem merchantItem;
        public UIAtavismInventory inventory;

        // Use this for initialization
        void Start()
        {
           
           
            Hide();
        }

        
          
        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("MERCHANT_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CLOSE_NPC_DIALOGUE", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected  override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("MERCHANT_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_NPC_DIALOGUE", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
          //  Debug.LogError("UIAtavismMerchantPanel registerUI");
   
            VisualElement main_currency  = uiWindow.Q<VisualElement>("currency");
            mainCurrency = new UIAtavismCurrencyDisplay();
            mainCurrency.SetVisualElement(main_currency);
            mainCurrency.ReverseOrder = true;
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

            
            return true;
        }

        public override void Show()
        {
            base.Show();
           // AtavismSettings.Instance.OpenWindow(this);
           if (inventory != null)
           {
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
           }

           currentPage = 0;
            AtavismCursor.Instance.SetUIActivatableClickedOverride(SellItemToMerchant);


            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MERCHANT_UI_OPENED", args);
            // AtavismUIUtility.BringToFront(gameObject);
            UpdateCurrencies();
            uiWindowTitle.text = ClientAPI.GetObjectNode(NpcInteraction.Instance.NpcId.ToLong()).Name;

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
          //  AtavismSettings.Instance.CloseWindow(this);
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(SellItemToMerchant);

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
            UpdateMerchantFrame();
        }

        public void ShowNextPage()
        {
            currentPage++;
            UpdateMerchantFrame();
        }

        void UpdateMerchantFrame()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if ((currentPage * entries.Count) + i < NpcInteraction.Instance.MerchantItems.Count)
                {
                    entries[i].Show();
                    entries[i].UpdateMerchantItemData(NpcInteraction.Instance.GetMerchantItem((currentPage * entries.Count) + i), this);
                }
                else
                {
                    entries[i].Hide();
                }
            }
            if (currentPageText != null)
                if (NpcInteraction.Instance.MerchantItems.Count > entries.Count)
                    currentPageText.text = (currentPage + 1) + " / " + ((NpcInteraction.Instance.MerchantItems.Count % entries.Count) > 0 ? (NpcInteraction.Instance.MerchantItems.Count / entries.Count + 1).ToString() : (NpcInteraction.Instance.MerchantItems.Count / entries.Count).ToString());
                else
                    currentPageText.text = "";

            // Update visibility of prev and next buttons
            if (currentPage > 0)
            {
                prevButton.visible = true;
            }
            else
            {
                prevButton.visible = false;
            }

            if (NpcInteraction.Instance.MerchantItems.Count > (currentPage + 1) * entries.Count)
            {
                nextButton.visible =true;
            }
            else
            {
                nextButton.visible = false;
            }

            if (purchaseCountPanel != null)
            {
                purchaseCountPanel.HideVisualElement();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "MERCHANT_UPDATE")
            {
                if (!showing)
                    Show();
                if (eData.eventArgs[0] != null)
                {
                    if (NpcId != OID.fromString(eData.eventArgs[0]))
                    {
                        NpcId = OID.fromString(eData.eventArgs[0]);
                        currentPage = 0;
                    }
                }
                UpdateMerchantFrame();
            }
            else if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (showing)
                    UpdateMerchantFrame();
            }
            else if (eData.eventType == "CLOSE_NPC_DIALOGUE")
            {
                Hide();
                NpcId = null;
            }else if (eData.eventType == "CURRENCY_UPDATE"||eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                UpdateCurrencies();
            }
        }
      
        void UpdateCurrencies()
        {
            mainCurrency.MainCurrency();
            mainCurrency.Show();
        }
        public bool ShowPurchaseCountPanel(UIAtavismMerchantItemSlot mItemEntry, MerchantItem merchantItem)
        {
            this.merchantItem = merchantItem;
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
                AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(merchantItem.itemID);
                if (itemName != null && aii != null)
                {
#if AT_I2LOC_PRESET
                itemName.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name)) ? aii.name : I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name);
#else
                    itemName.text = aii.name;
#endif
                }
                sumCost.SetData(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
               
                // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
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
            sumCost.SetData(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
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
            if (minusButton != null)
                minusButton.visible = true;
            if (purchaseCountText != null)
            {
                purchaseCountText.value = multiplePurchaseCount.ToString();
            }
            sumCost.SetData(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
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
                if (purchaseCountText.value != "")
                    multiplePurchaseCount = int.Parse(purchaseCountText.value);
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
            sumCost.SetData(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(merchantItem.purchaseCurrency, merchantItem.cost * multiplePurchaseCount);
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
                MerchantItem mItem = new MerchantItem();
                mItem.itemID = merchantItem.itemID;
                mItem.count = int.Parse(purchaseCountText.text);
                NpcInteraction.Instance.PurchaseItemConfirmed(mItem, true);
                merchantItem = null;
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
            merchantItem = null;
        }

        public void SellItemToMerchant(UIAtavismActivatable activatable)
        {
            NpcInteraction.Instance.SellItemToMerchant((AtavismInventoryItem)activatable.ActivatableObject);
        }
    }
}