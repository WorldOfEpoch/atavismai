using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismTradeWindow : UIAtavismWindowBase
    {

        private Label myName;
        private Label partnerName;
        private List<UIAtavismTradeSlot> myOfferEntries = new List<UIAtavismTradeSlot>();
        // private List<Image> myCurrencyIcons;
        private List<UIAtavismTradeOffer> partnerOfferEntries = new List<UIAtavismTradeOffer>();
        private UIAtavismCurrencyInputPanel myCurrency;
        private UIAtavismCurrencyDisplay partnerOfferedCurrencies;
        private Label myStatus;
        private Label partnerStatus;
        private Button tradeButton;
        private Button cancelButton;
        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("TRADE_START", this);
            AtavismEventSystem.RegisterEvent("TRADE_OFFER_UPDATE", this);
            AtavismEventSystem.RegisterEvent("TRADE_COMPLETE", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("TRADE_START", this);
            AtavismEventSystem.UnregisterEvent("TRADE_OFFER_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("TRADE_COMPLETE", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            
       myName= uiWindow.Query<Label>("my-name");
        partnerName= uiWindow.Query<Label>("partner-name");
        // private List<UIAtavismTradeSlot> myOfferEntries;
        // private List<UIAtavismTradeOffer> partnerOfferEntries;
        VisualElement createCurrency = uiWindow.Q<VisualElement>("currency-input");
        myCurrency = new UIAtavismCurrencyInputPanel();
        myCurrency.SetVisualElement(createCurrency);
        myCurrency.SetOnChange(checkCurrency);
        myCurrency.SetCurrencyReverseOrder = true;
        myCurrency.SetCurrencies(Inventory.Instance.GetMainCurrencies());
      
        
        for (int i = 1; i <=AtavismTrade.Instance.tradeSlotCount; i++)
        {
            VisualElement item  = uiWindow.Q<VisualElement>("my-item-"+i);
            if (item != null)
            {
                // Instantiate a controller for the data
                UIAtavismTradeSlot newListEntryLogic = new UIAtavismTradeSlot();
                // Assign the controller script to the visual element
                item.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(item);
                newListEntryLogic.slotNum = i - 1;
                myOfferEntries.Add(newListEntryLogic);
            }
        }
        
        for (int i = 1; i <= AtavismTrade.Instance.tradeSlotCount; i++)
        {
            VisualElement item  = uiWindow.Q<VisualElement>("partner-item-"+i);
            if (item != null)
            {
                // Instantiate a controller for the data
                UIAtavismTradeOffer newListEntryLogic = new UIAtavismTradeOffer();
                // Assign the controller script to the visual element
                item.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(item);
                partnerOfferEntries.Add(newListEntryLogic);
            }
        }        
        
        
        VisualElement currency  = uiWindow.Q<VisualElement>("currency");
        partnerOfferedCurrencies = new UIAtavismCurrencyDisplay();
        partnerOfferedCurrencies.SetVisualElement(currency);
        partnerOfferedCurrencies.ReverseOrder = true;   

        myStatus= uiWindow.Query<Label>("my-status");
         partnerStatus= uiWindow.Query<Label>("partner-status");

         tradeButton = uiWindow.Q<Button>("trade-button");
         tradeButton.clicked += AcceptTrade;
         cancelButton = uiWindow.Q<Button>("cancel-button");
         cancelButton.clicked += CancelTrade;
            return true;
        }


        public override void Show()
        {

            base.Show();
            if (myName != null)
                myName.text = ClientAPI.GetPlayerObject().Name;
            if (partnerName != null)
                partnerName.text = ClientAPI.GetObjectNode(AtavismTrade.Instance.TradePartnerOid.ToLong()).Name;

            AtavismTrade.Instance.NewTradeStarted();
            // Handle currency
        }

        public override void Hide()
        {
            base.Hide();
          //  AtavismSettings.Instance.CloseWindow(this);

            // Set all referenced items back to non referenced
            for (int i = 0; i < myOfferEntries.Count; i++)
            {
                myOfferEntries[i].ResetSlot();
            }
        }

        void UpdateTradeWindow()
        {
            for (int i = 0; i < myOfferEntries.Count; i++)
            {
                myOfferEntries[i].UpdateTradeSlotData(AtavismTrade.Instance.GetTradeItemInfo(true,i));
            }
            for (int i = 0; i < partnerOfferEntries.Count; i++)
            {
                partnerOfferEntries[i].UpdateTradeOfferData(AtavismTrade.Instance.GetTradeItemInfo(false,i));
            }

            // Handle currency
            // for (int i = 0; i < myCurrencyIcons.Count; i++)
            // {
            //     if (i < Inventory.Instance.GetMainCurrencies().Count)
            //     {
            //         myCurrencyIcons[i].sprite = Inventory.Instance.GetMainCurrency(i).Icon;
            //     }
            // }
            partnerOfferedCurrencies.SetData( AtavismTrade.Instance.TheirCurrencyOffers);
          
            // for (int i = 0; i < partnerOfferedCurrencies.Count; i++)
            // {
            //     if (i < AtavismTrade.Instance.TheirCurrencyOffers.Count)
            //     {
            //         partnerOfferedCurrencies[i].gameObject.SetActive(true);
            //         partnerOfferedCurrencies[i].SetCurrencyDisplayData(AtavismTrade.Instance.TheirCurrencyOffers[i]);
            //     }
            //     else
            //     {
            //         partnerOfferedCurrencies[i].gameObject.SetActive(false);
            //     }
            // }

            // If accepted set the colour of the panels
            if (AtavismTrade.Instance.AcceptedByMe)
            {
#if AT_I2LOC_PRESET
           if (myStatus!=null)  myStatus.text = I2.Loc.LocalizationManager.GetTranslation("Accepted");
#else
                if (myStatus != null)
                    myStatus.text = "Accepted";
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
          if (myStatus!=null)   myStatus.text = I2.Loc.LocalizationManager.GetTranslation("Pending...");
#else
                if (myStatus != null)
                    myStatus.text = "Pending...";
#endif
            }

            if (AtavismTrade.Instance.AcceptedByPartner)
            {
#if AT_I2LOC_PRESET
           if (partnerStatus!=null)  partnerStatus.text = I2.Loc.LocalizationManager.GetTranslation("Accepted");
#else
                if (partnerStatus != null)
                    partnerStatus.text = "Accepted";
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
          if (partnerStatus!=null)   partnerStatus.text = I2.Loc.LocalizationManager.GetTranslation("Pending...");
#else
                if (partnerStatus != null)
                    partnerStatus.text = "Pending...";
#endif
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "TRADE_START")
            {
                if (!showing)
                    Show();
                UpdateTradeWindow();
            }
            else if (eData.eventType == "TRADE_OFFER_UPDATE")
            {
                UpdateTradeWindow();
            }
            else if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                UpdateTradeWindow();
            }else if (eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                UpdateTradeWindow();
            }
            else if (eData.eventType == "TRADE_COMPLETE")
            {
                Hide();
            }
        }

        /// <summary>
        /// Updates the currency amount for the first "main" currency
        /// </summary>
        /// <param name="currencyAmount">Currency amount.</param>
        public void SetCurrency1(string currencyAmount)
        {
            if (currencyAmount == "")
                currencyAmount = "0";
            AtavismTrade.Instance.SetCurrencyAmount(Inventory.Instance.GetMainCurrency(0).id, int.Parse(currencyAmount));
        }

        /// <summary>
        /// Updates the currency amount for the first "main" currency
        /// </summary>
        /// <param name="currencyAmount">Currency amount.</param>
        public void SetCurrency2(string currencyAmount)
        {
            if (currencyAmount == "")
                currencyAmount = "0";
            AtavismTrade.Instance.SetCurrencyAmount(Inventory.Instance.GetMainCurrency(1).id, int.Parse(currencyAmount));
        }

        /// <summary>
        /// Updates the currency amount for the first "main" currency
        /// </summary>
        /// <param name="currencyAmount">Currency amount.</param>
        public void SetCurrency3(string currencyAmount)
        {
            if (currencyAmount == "")
                currencyAmount = "0";
            AtavismTrade.Instance.SetCurrencyAmount(Inventory.Instance.GetMainCurrency(2).id, int.Parse(currencyAmount));
        }

        void checkCurrency()
        {
            int currencyId = 0;
            long cost = 0;
            myCurrency.GetCurrencyAmount(out currencyId, out cost);
            AtavismTrade.Instance.SetCurrencyAmountConvert(currencyId, (int)cost);
            List<Vector2> currencies = new List<Vector2>();
            currencies.Add(new Vector2(currencyId, cost));
            if (Inventory.Instance.DoesPlayerHaveEnoughCurrency(currencies))
            {
                Debug.Log("Player does have enough currency");
                return ;
            }
            Debug.Log("Player does not have enough currency");
            return ;
        }

        public void AcceptTrade()
        {
            AtavismTrade.Instance.AcceptTrade();
        }

        protected override void onWindowCloseButtonClicked()
        {
            CancelTrade();
        }

        public void CancelTrade()
        {
            AtavismTrade.Instance.CancelTrade();
            Hide();
        }
    }
}