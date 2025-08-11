using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    // public class ShopItem
    // {
    //     public int itemID;
    //     public OID itemOID;
    //     public AtavismInventoryItem item;
    //     public int count;
    //     public int sellCount;
    //     public long cost;
    //     public int purchaseCurrency;
    //     public int pos=-1;
    // }

    public class UIAtavismCreateShop : UIAtavismWindowBase
    {
        [SerializeField] private int numberSlots = 10;
        [SerializeField] private VisualTreeAsset itemSlotTemplate;
        
        public List<UIAtavismCreateShopEntity> buyEntries = new List<UIAtavismCreateShopEntity>();
        public List<UIAtavismCreateShopEntity> sellEntries = new List<UIAtavismCreateShopEntity>();
        protected List<ShopItem> sellItems = new List<ShopItem>();
        protected List<ShopItem> buyItems = new List<ShopItem>();
        
        // public List<VisualElement> priceCurrencyIcons = new List<VisualElement>();
        // public List<UITextField> priceCurrencyInput = new List<UITextField>();
        
        UIAtavismCurrencyInputPanel priceCurrencyInputPanel;
        public UITextField priceQuantity;
        public SliderInt priceQuantitySlider;
        public UIAtavismCurrencyDisplay sellSumaryCurrencies;
        public VisualElement buyPanel;
        public VisualElement sellPanel;
        public VisualElement buyItemsList;
        public VisualElement sellItemsList;

        public VisualElement quantityPanel;
        public VisualElement pricePanel;
        public VisualElement totalPricePanel;
        public Label priceItemName;
        public UIAtavismItemSlotDisplay priceItemDisplay;
        public List<RectTransform> quantityObjects = new List<RectTransform>();

       
        bool buyMode = false;
        int slots = 10;
     
        public UITextField messageInput;
        private Button createButton;
        private Button cancelButton;
        
        
        private Button savePriceButton;
        private Button cancelPriceButton;

        [SerializeField] VisualElement panel;
        
        [AtavismSeparator("Menu Settings")]
        // public bool hideNormaleMenuImage = true;
        private UIButtonToggleGroup menu;
        // [SerializeField] List<SkillTypeButton> tabButtons = new List<SkillTypeButton>();
        // [SerializeField] Color buttonMenuSelectedColor = Color.green;
        // [SerializeField] Color buttonMenuNormalColor = Color.white;
        // [SerializeField] Color buttonMenuSelectedTextColor = Color.black;
        // [SerializeField] Color buttonMenuNormalTextColor = Color.black;
        // Use this for initialization
        protected override void Start()
        {
            // Hide();
          
            // for (int i = 0; i < buyEntries.Count; i++)
            // {
            //     if (buyEntries[i] != null)
            //     {
            //
            //         buyEntries[i].AssignShop(this, i);
            //         buyEntries[i].updateDisplay(null);
            //     }
            // }
            // for (int i = 0; i < buyEntries.Count; i++)
            // {
            //     if (sellEntries[i] != null)
            //     {
            //         sellEntries[i].AssignShop(this, i);
            //         sellEntries[i].updateDisplay(null);
            //     }
            // }

           
          
        }

        private void _HandleCreateShop(Dictionary<string, object> props)
        {
            slots =(int) props["slots"];
            buyItems.Clear();
            sellItems.Clear();
            for (int i = 0; i < buyEntries.Count; i++)
            {
                buyEntries[i].updateDisplay(null);
                buyEntries[i].ResetSlot();
            }
            for (int i = 0; i < sellEntries.Count; i++)
            {
                sellEntries[i].updateDisplay(null);
                sellEntries[i].ResetSlot();
            }
            
            
            // List<Currency> currencyDisplayList = Inventory.Instance.GetCurrenciesInGroup(Inventory.Instance.mainCurrencyGroup);
            //
            //
            //
            // for (int i = 0; i < priceCurrencyIcons.Count; i++)
            // {
            //     if (i < currencyDisplayList.Count)
            //     {
            //         if (priceCurrencyIcons[i] != null)
            //         {
            //             if (priceCurrencyIcons[i]!=null)
            //                 priceCurrencyIcons[i].visible = true;
            //             priceCurrencyIcons[i].style.backgroundImage = currencyDisplayList[i].Icon.texture;
            //         }
            //     }
            //     else
            //     {
            //         if (priceCurrencyIcons[i] != null) priceCurrencyIcons[i].visible = false;
            //     }
            // }
            //
            // for (int i = 0; i < priceCurrencyInput.Count; i++)
            // {
            //     if (i < currencyDisplayList.Count)
            //     {
            //         if(priceCurrencyInput[i]!=null )
            //             priceCurrencyInput[i].visible=true;
            //     }
            //     else
            //     {
            //         if (priceCurrencyInput[i] != null)   priceCurrencyInput[i].visible=false;
            //     }
            // }
            
            Show();
          
        }

        void OnDestroy()
        {
          
        }
          protected override  void registerEvents()
        {
            base.registerEvents();
            NetworkAPI.RegisterExtensionMessageHandler("start_player_shop", _HandleCreateShop);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            // showBuy();
        }

        protected override  void unregisterEvents()
        {
            base.unregisterEvents();
            NetworkAPI.RemoveExtensionMessageHandler("start_player_shop", _HandleCreateShop);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
        }

           protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            VisualElement inner_panel = uiWindow.Q<VisualElement>("inner-panel");

            menu = inner_panel.Query<UIButtonToggleGroup>("menu");
            menu.OnItemIndexChanged += TopMenuChange;

            messageInput = inner_panel.Q<UITextField>("message");
            
            createButton = uiWindow.Q<Button>("create-button");
            cancelButton = uiWindow.Q<Button>("cancel-button");
            createButton.clicked += StartShop;
            cancelButton.clicked += CancelShop;

            buyPanel = uiWindow.Q<VisualElement>("buy-panel");
            buyItemsList =  buyPanel.Q<VisualElement>("item-list");
            sellPanel = uiWindow.Q<VisualElement>("sell-panel");
            sellItemsList =  sellPanel.Q<VisualElement>("item-list");
            pricePanel= uiWindow.Q<VisualElement>("price-panel");
            
            buyItemsList.Clear();
            for (int i = 0; i < numberSlots; i++)
            {
                UIAtavismCreateShopEntity  script = new UIAtavismCreateShopEntity();
                // Instantiate the UXML template for the entry
                var newListEntry = itemSlotTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = script;
                // Initialize the controller script
                script.SetVisualElement(newListEntry);
                buyEntries.Add(script);
                buyItemsList.Add(newListEntry);
            }
            sellItemsList.Clear();
            for (int i = 0; i < numberSlots; i++)
            {
                UIAtavismCreateShopEntity  script = new UIAtavismCreateShopEntity();
                // Instantiate the UXML template for the entry
                var newListEntry = itemSlotTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = script;
                // Initialize the controller script
                script.SetVisualElement(newListEntry);
                sellEntries.Add(script);
                sellItemsList.Add(newListEntry);
            }

            quantityPanel = pricePanel.Q<VisualElement>("quantity-panel");
            priceQuantity = pricePanel.Query<UITextField>("quantity-input");
            priceQuantity.RegisterValueChangedCallback(changeQuantity);
            priceQuantitySlider = pricePanel.Query<SliderInt>("quantity-slider");
            priceQuantitySlider.RegisterValueChangedCallback(changeQuantitySlider);
            //Price
            totalPricePanel = pricePanel.Q<VisualElement>("total-price-panel");
            VisualElement currencyInput = pricePanel.Q<VisualElement>("currency-input");
            priceCurrencyInputPanel = new UIAtavismCurrencyInputPanel();
            priceCurrencyInputPanel.SetVisualElement(currencyInput);
            priceCurrencyInputPanel.SetOnChange(checkPrice);
            priceCurrencyInputPanel.SetCurrencyReverseOrder = true;
            priceCurrencyInputPanel.SetCurrencies(Inventory.Instance.GetMainCurrencies());

           
            
            VisualElement currency  = pricePanel.Q<VisualElement>("currency-sum");
            sellSumaryCurrencies = new UIAtavismCurrencyDisplay();
            sellSumaryCurrencies.SetVisualElement(currency);
            sellSumaryCurrencies.ReverseOrder = true;
            // sellSumaryCurrencies.Show();
            
            cancelPriceButton = pricePanel.Q<Button>("cancel-button");
            savePriceButton = pricePanel.Q<Button>("save-button");
            cancelPriceButton.clicked += CancelPrice;
            savePriceButton.clicked += PriceSave;
            priceItemName = pricePanel.Q<Label>("item-name");

            VisualElement slot  = pricePanel.Q<VisualElement>("item-slot");
            priceItemDisplay = new UIAtavismItemSlotDisplay();
            priceItemDisplay.SetVisualElement(slot);
            
            if (pricePanel!=null)
                pricePanel.HideVisualElement();
            for (int i = 0; i < buyEntries.Count; i++)
            {
                if (buyEntries[i] != null)
                {

                    buyEntries[i].AssignShop(this, i);
                    buyEntries[i].updateDisplay(null);
                }
            }
            for (int i = 0; i < buyEntries.Count; i++)
            {
                if (sellEntries[i] != null)
                {
                    sellEntries[i].AssignShop(this, i);
                    sellEntries[i].updateDisplay(null);
                }
            }
            return true;
        }

    
        private void TopMenuChange(int obj)
        {
           // Debug.LogError("TopMenuChange "+obj);
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

        public void showBuy()
        {
            buyPanel.ShowVisualElement();
            sellPanel.HideVisualElement();
            buyMode = true;
          
            int c = 0;
            for (int i = 0; i < buyEntries.Count; i++)
            {
                if (buyEntries[i] != null)
                {
                    if (c < slots - sellItems.Count)
                        buyEntries[i].Show();
                    else
                        buyEntries[i].Hide();
                    c++;
                }
            }
            c = 0;
            for (int i = 0; i < sellEntries.Count; i++)
            {
                if (c < slots - buyItems.Count)
                    sellEntries[i].Show();
                else
                    sellEntries[i].Hide();
                c++;
            }
        }

        public void showSell()
        {
            sellPanel.ShowVisualElement();
            buyPanel.HideVisualElement();
            buyMode = false;
           
            int c = 0;
            for (int i = 0; i < buyEntries.Count; i++)
            {
                if (buyEntries[i] != null)
                {
                    if (c < slots - sellItems.Count)
                        buyEntries[i].Show();
                    else
                        buyEntries[i].Hide();
                    c++;
                }
            }
            c = 0;
            for (int i = 0; i < sellEntries.Count; i++)
            {
                if (c < slots - buyItems.Count)
                    sellEntries[i].Show();
                else
                    sellEntries[i].Hide();
                c++;
            }
        }

        public override void Show()
        {
            base.Show();
            menu.Set( 1);
            showSell();
       

                //    AtavismTrade.Instance.NewTradeStarted();
                AtavismUIUtility.BringToFront(gameObject);
            // Handle currency
            int c = 0;
              for (int i = 0; i < buyEntries.Count; i++)
              {
                if (buyEntries[i] != null)
                {
                    if( c < slots- sellItems.Count)
                        buyEntries[i].Show();
                    else
                        buyEntries[i].Hide();
                    c++;
                }
              }
            c = 0;
            for (int i = 0; i < sellEntries.Count; i++)
              {
                if (c < slots - buyItems.Count)
                    sellEntries[i].Show();
                else
                    sellEntries[i].Hide();
                c++;
            }
            ResetParams();
            if (panel != null)
                panel.ShowVisualElement();
        }

        public override void Hide()
        {
            base.Hide();
            if (panel != null)
                panel.HideVisualElement();

            // Set all referenced items back to non referenced
            /* for (int i = 0; i < buyEntries.Count; i++)
             {
                 myOfferEntries[i].ResetSlot();
             }*/
        }

        void UpdateWindow()
        {
           /* for (int i = 0; i < buyEntries.Count; i++)
            {
                  buyEntries[i].updateDisplay(null);
            }
            for (int i = 0; i < sellEntries.Count; i++)
            {
                sellEntries[i].updateDisplay(null);
            }*/

           
            // If accepted set the colour of the panels

        }

        void ResetParams()
        {
            if (messageInput!=null)
            {
                messageInput.value = "";
            }

            priceCurrencyInputPanel.ClearCurrencyAmounts();
            
       }


        public void OnEvent(AtavismEventData eData)
        {
            
            if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                UpdateWindow();
            }
           
        }

       
        public void StartShop()
        {
            // AtavismTrade.Instance.AcceptTrade();
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("message", messageInput.text);
            int c = 0;
          //  Debug.LogError("Shop StartShop sellItems=" + sellItems.Count + " buyMode=" + buyMode);
            foreach (ShopItem si in sellItems)
            {
                props.Add("sitemOid" + c, si.itemOID);
               // props.Add("itemId" + c, si.itemID);
               // props.Add("itemCount" + c, si.count);
                props.Add("sitemCurr" + c, si.purchaseCurrency);
                props.Add("sitemCost" + c, si.cost);
                c++;
            }
            props.Add("sellNum", c);
         //   Debug.LogError("Shop StartShop buyItems=" + buyItems.Count + " buyMode=" + buyMode);

            c = 0;
            foreach (ShopItem si in buyItems)
            {
                //props.Add("itemOid" + c, si.itemOID);
                props.Add("bitemId" + c, si.itemID);
                props.Add("bitemCount" + c, si.count);
                props.Add("bitemCurr" + c, si.purchaseCurrency);
                props.Add("bitemCost" + c, si.cost);
                c++;
            }
            props.Add("buyNum", c);

            if (sellItems.Count == 0 && buyItems.Count == 0)
            {
                string[] args = new string[1];
                args[0] = "Shop cant be empty";
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                return;
            }


            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.START_SHOP", props);
            Hide();
            for (int i = 0; i < buyEntries.Count; i++)
            {
                buyEntries[i].updateDisplay(null);
            }
            for (int i = 0; i < sellEntries.Count; i++)
            {
                sellEntries[i].updateDisplay(null);
            }

        }

        public void CancelShop()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.CANCEL_SHOP", props);
            Hide();
        }

        public void DropItem(AtavismInventoryItem item, int pos)
        {
          //  Debug.LogError("Shop drop "+item+ " buyMode="+ buyMode);
            selectPos = pos;
            if (item != null)
            {
               // Debug.LogError("Shop drop " + item.ItemId);
               // if (item.StackLimit > 1)
               // {
                    if (buyMode)
                    {
                        ShopItem mi = new ShopItem();
                        int currId = -1;
                        long currAmm = -1;
                        mi.cost = currAmm;
                        mi.purchaseCurrency = currId;
                        mi.itemID = item.templateId;
                        mi.item = item;
                        mi.count = item.Count;
                        mi.pos = selectPos;
                    // sellItems.Add(item.ItemId, mi);
                    selectItem = mi;
                        // foreach (RectTransform rc in quantityObjects)
                        // {
                        //     if (rc != null)
                        //         rc.gameObject.SetActive(true);
                        // }
                        if (totalPricePanel != null)
                            totalPricePanel.visible = true;
                        if(quantityPanel != null)
                            quantityPanel.visible = true;
                    if (priceQuantity!=null)
                    {
                        priceQuantity.value = item.StackLimit.ToString();
                    }
                    if (priceQuantitySlider!=null)
                    {
                        Debug.LogError("SHop Drop " + item.BaseName + " " + item.StackLimit);
                        priceQuantitySlider.highValue = item.StackLimit;
                        priceQuantitySlider.value = item.StackLimit;
                    }
                    if (pricePanel!=null)
                            pricePanel.ShowVisualElement();
                        if (priceItemDisplay!=null)
                        {
                            priceItemDisplay.SetItemData(item, null);
                        }
                        if (priceItemName!=null)
                        {
                            priceItemName.text = item.BaseName;
                        }
                        //                        buyItems.Add(item.ItemId, mi);
                    }
                    else
                    {
                        if (totalPricePanel != null)
                            totalPricePanel.visible = false;
                        if(quantityPanel != null)
                            quantityPanel.visible = false;

                        ShopItem mi = new ShopItem();
                        int currId = -1;
                        long currAmm = -1;
                       // List<Vector2> c = new List<Vector2>();
                       // Inventory.Instance.ConvertCurrenciesToBaseCurrency(c, out currId, out currAmm);
                        mi.cost = currAmm;
                        mi.purchaseCurrency = currId;
                        mi.itemID = item.templateId;
                        mi.item = item;
                        mi.itemOID = item.ItemId;
                        mi.count = item.Count;
                        mi.pos = selectPos;
                       // sellItems.Add(item.ItemId, mi);
                        selectItem = mi;
                        // foreach (RectTransform rc in quantityObjects)
                        // {
                        //     if (rc != null)
                        //         rc.gameObject.SetActive(false);
                        // }
                        if (pricePanel!=null)
                            pricePanel.ShowVisualElement();
                        if (priceItemDisplay != null)
                        {
                            priceItemDisplay.SetItemData(item, null);
                        }
                        if (priceItemName!=null)
                        {
                            priceItemName.text = item.BaseName;
                        }
                    }
                checkPrice();
               // }
            }
        }

        public void checkPrice()
        {
            
            int currencyType = 0;
            long currencyAmount = 0;
            priceCurrencyInputPanel.GetCurrencyAmount(out currencyType, out currencyAmount);
            
           
            if (priceQuantity!=null)
            {
                if (priceQuantity.text.Length == 0)
                    priceQuantity.value = "1";
                // int currencyType = -1;
                // long currencyAmount = -1;
                // Inventory.Instance.ConvertCurrenciesToBaseCurrency(sellprice, out currencyType, out currencyAmount);
                currencyAmount = currencyAmount * int.Parse(priceQuantity.text);
                
                
                sellSumaryCurrencies.SetData(currencyType, currencyAmount);
                sellSumaryCurrencies.Show();
               
            }
        }


        public void changeQuantity(ChangeEvent<string> evt)
        {
         //   Debug.LogError("changeQuantity "+evt.newValue+" "+evt.previousValue);
            if (priceQuantity!=null && priceQuantitySlider!=null)
            {
                int v = int.Parse(priceQuantity.text);
                if (v < 1)
                {
                    v = 1;
                    priceQuantity.SetValueWithoutNotify(v.ToString());
                }
                if (selectItem != null)
                {
                    if (v > selectItem.item.StackLimit)
                    {
                        v = selectItem.item.StackLimit;
                        priceQuantity.SetValueWithoutNotify(v.ToString());
                    }
                }

                priceQuantitySlider.SetValueWithoutNotify(int.Parse(priceQuantity.text));
            }
            checkPrice();
        }
        public void changeQuantitySlider(ChangeEvent<int> evt)
        {
          //  Debug.LogError("changeQuantitySlider "+evt.newValue+" "+evt.previousValue);
            if (priceQuantity!=null && priceQuantitySlider!=null)
                priceQuantity.SetValueWithoutNotify(priceQuantitySlider.value.ToString());
            checkPrice();
        }

        public void CancelPrice()
        {
            if (pricePanel != null)
                pricePanel.HideVisualElement();
            if (buyMode)
            {
                buyEntries[selectPos].updateDisplay(null);
                buyEntries[selectPos].ResetSlot();
            }
            else
            {
                sellEntries[selectPos].updateDisplay(null);
                sellEntries[selectPos].ResetSlot();
            }
            selectItem = null;
            // UpdateWindow();
        }
        public void PriceSave()
        {
            int currencyType = 0;
            long currencyAmount = 0;
            priceCurrencyInputPanel.GetCurrencyAmount(out currencyType, out currencyAmount);
            
        //    Debug.LogError("PriceSave "+currencyType+" "+currencyAmount);
           
            if (buyMode)
            {
              //  Debug.LogError("selectItem >" + selectItem + " buyMode="+ buyMode);
                if (selectItem != null)
                {
                    
                    if (currencyAmount == 0)
                    {
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                args[0] = I2.Loc.LocalizationManager.GetTranslation("Price can't be zero");
#else
                        args[0] = "Price can't be zero";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                        
                        return;
                    }
                    selectItem.cost = currencyAmount;
                    selectItem.purchaseCurrency = currencyType;
                    if (priceQuantity!=null)
                    {
                        if (priceQuantity!=null)
                            if (priceQuantity.text.Length == 0)
                                priceQuantity.value = "1";
                        selectItem.count = int.Parse(priceQuantity.text);
                    }
                    buyEntries[selectPos].updateDisplay(selectItem);
                 /*   if (buyEntries[selectPos])
                        Debug.LogError("sellEntries[selectPos]");
                    if (buyEntries[selectPos].itemSlot)
                        Debug.LogError("sellEntries[selectPos].itemSlot");
                    if (buyEntries[selectPos].itemSlot.UguiActivatable)
                        Debug.LogError("sellEntries[selectPos].itemSlot.UguiActivatable");*/
                    if (buyEntries[selectPos].ItemSlot.UiActivatable.CountText != null)
                        buyEntries[selectPos].ItemSlot.UiActivatable.CountText.text = selectItem.count.ToString();
                    buyItems.Add(selectItem);
                  //  Debug.LogError("added  buyItems=" + buyItems.Count + " buyMode=" + buyMode);

                }
            }
            else
            {
                if (selectItem != null)
                {
                    // int currId = -1;
                    // long currAmm = -1;
                    // Inventory.Instance.ConvertCurrenciesToBaseCurrency(sellprice, out currId, out currAmm);
                    if (currencyAmount == 0)
                    {
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                args[0] = I2.Loc.LocalizationManager.GetTranslation("Price can't be zero");
#else
                        args[0] = "Price can't be zero";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);

                        return;
                    }
                    selectItem.cost = currencyAmount;
                    selectItem.purchaseCurrency = currencyType;
                  //  if(!sellItems.ContainsKey(selectItem.itemOID))

                    sellItems.Add(selectItem);
                    sellEntries[selectPos].updateDisplay(selectItem);
                    if (sellEntries[selectPos].ItemSlot.UiActivatable.CountText != null)
                        sellEntries[selectPos].ItemSlot.UiActivatable.CountText.text = selectItem.item.Count.ToString();

                    //   Debug.LogError("added  sellItems=" + sellItems.Count + " buyMode=" + buyMode);

                }
            }
            if (pricePanel != null)
                pricePanel.HideVisualElement();
            selectItem = null;
            priceCurrencyInputPanel.ClearCurrencyAmounts();
            
            //  UpdateWindow();
        }

        Dictionary<int, long> sellprice = new Dictionary<int, long>();
        ShopItem selectItem = null;
        int selectPos=-1;
        public void ClearSlot(int pos, AtavismInventoryItem item)
        {
            if (item == null)
                return;
            //Debug.LogError("ClearSlot "+ buyItems.Count+" "+sellItems.Count);
            ShopItem select = null;
            if (buyMode)
            {
                foreach (ShopItem si in buyItems)
                {
                  //  Debug.LogError("ClearSlot buy pos=" + si.pos + " " + pos);
                    if (si.pos == pos)
                    {
                        select = si;
                        break;
                    }
                }
             //   Debug.LogError("ClearSlot BUY select=" + select);
                if (select != null)
                    buyItems.Remove(select);
                if (pos >= 0 && buyEntries.Count > pos)
                {
                   // buyEntries[pos].ResetSlot();
                    buyEntries[pos].updateDisplay(null);
                }
            }
            else
            {
                foreach (ShopItem si in sellItems)
                {
               //     Debug.LogError("ClearSlot sell pos=" + si.pos + " " + pos);
                    if (si.pos == pos)
                    {
                        select = si;
                        break;
                    }
                }
              //  Debug.LogError("ClearSlot SELL select=" + select);
                if (select != null)
                    sellItems.Remove(select);
                if (pos >= 0 && sellEntries.Count > pos)
                {
                   // sellEntries[pos].ResetSlot();
                    sellEntries[pos].updateDisplay(null);
                }
            }
           // Debug.LogError("ClearSlot End " + buyItems.Count + " " + sellItems.Count);

        }
    }
}