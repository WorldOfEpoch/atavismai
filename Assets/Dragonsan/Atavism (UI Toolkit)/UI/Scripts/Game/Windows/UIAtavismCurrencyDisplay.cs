using System.Collections.Generic;
using Atavism;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismCurrencyDisplay 
    {
        private long currencyValue;
        private int currencyId;
        private List<Label> currencyValues = new List<Label>();
        private List<VisualElement> currencyIcons = new List<VisualElement>();
        private VisualElement m_container;
        public bool ReverseOrder = false;
        private bool mainCurrency = false;
        private bool showAllFromGroup =false;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_container = visualElement;
            for (int i = 1; i <= 5; i++)
            {
                Label value = visualElement.Q<Label>("currency-value"+i);
                VisualElement icon = visualElement.Q<VisualElement>("currency-icon"+i);
                if (value != null)
                {
                    currencyValues.Add(value);
                    currencyIcons.Add(icon);
                }

                
            }

            registerUI();
            // Debug.LogError("UIAtavismCurrencyDisplay currency elements "+currencyValues.Count);
            //   parentScript = parent;
        }

        public void SetData(int currencyId, long currencyValue)
        {
            SetData(currencyId, currencyValue, true);
        }

        public void SetData(int currencyId, long currencyValue ,bool showAllFromGroup )
        {
            Show();
            this.showAllFromGroup = showAllFromGroup;
          //  Debug.LogError("UIAtavismCurrencyDisplay.SetData: "+currencyId+"="+currencyValue);
            mainCurrency = false;
            this.currencyId = currencyId;
            this.currencyValue = currencyValue;
            UpdateCurrencies();
        }
        
        public void MainCurrency( )
        {
            Show();
          //  Debug.LogError("UIAtavismCurrencyDisplay.MainCurrency");
            mainCurrency = true;
            UpdateMainCurrencies();
        }

        protected virtual void OnDisable()
        {
            UnregisterUI();
        }

        // Use this for initialization
        void registerUI()
        {
            AtavismEventSystem.RegisterEvent("CURRENCY_UPDATE", OnEvent);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", OnEvent);
            UpdateCurrencies();
        }

        void UnregisterUI()
        {
             AtavismEventSystem.UnregisterEvent("CURRENCY_UPDATE", OnEvent);
             AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", OnEvent);
        }

        private void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CURRENCY_UPDATE"||eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                if (mainCurrency)
                {
                    UpdateMainCurrencies();
                }
                else
                {
                    UpdateCurrencies();
                }
            }
        }

        void UpdateCurrencies()
        {
            List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(currencyId, currencyValue,showAllFromGroup);
            if(ReverseOrder)
                currencyDisplayList.Reverse();
          //  Debug.LogError("UIAtavismCurrencyDisplay.UpdateCurrencies: currencyDisplayList="+currencyDisplayList.Count);
            for (int i = 0; i < currencyValues.Count; i++)
            {
                if (i < currencyDisplayList.Count)
                {
                    currencyValues[i].text = currencyDisplayList[i].amount.ToString();
                    currencyValues[i].ShowVisualElement();
                    currencyIcons[i].ShowVisualElement();
                    currencyIcons[i].style.backgroundImage = currencyDisplayList[i].icon.texture;
                }
                else
                {
                   // Debug.LogError("UIAtavismCurrencyDisplay.UpdateCurrencies: hide " + i);
                    currencyValues[i].HideVisualElement();
                    currencyIcons[i].HideVisualElement();
                }
            }
        }
        void UpdateMainCurrencies()
        {
            List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateMainCurrencyList();
            if(ReverseOrder)
                currencyDisplayList.Reverse();
          //  Debug.LogError("UIAtavismCurrencyDisplay.UpdateCurrencies: currencyDisplayList="+currencyDisplayList.Count);
            for (int i = 0; i < currencyValues.Count; i++)
            {
                if (i < currencyDisplayList.Count)
                {
                    currencyValues[i].text = currencyDisplayList[i].amount.ToString();
                    currencyValues[i].ShowVisualElement();
                    currencyIcons[i].ShowVisualElement();
                    currencyIcons[i].style.backgroundImage = currencyDisplayList[i].icon.texture;
                }
                else
                {
                 //   Debug.LogError("UIAtavismCurrencyDisplay.UpdateCurrencies: hide " + i);
                    currencyValues[i].HideVisualElement();
                    currencyIcons[i].HideVisualElement();
                }
            }
        }
        public void Show()
        {
            m_container.ShowVisualElement();
        }

        public void Hide()
        {
            m_container.HideVisualElement();
        }

        public void SetData(List<CurrencyDisplay> currencyDisplayList)
        {
            Show();
            if(ReverseOrder)
                currencyDisplayList.Reverse();
            //  Debug.LogError("UIAtavismCurrencyDisplay.UpdateCurrencies: currencyDisplayList="+currencyDisplayList.Count);
            for (int i = 0; i < currencyValues.Count; i++)
            {
                if (i < currencyDisplayList.Count)
                {
                    currencyValues[i].text = currencyDisplayList[i].amount.ToString();
                    currencyValues[i].ShowVisualElement();
                    currencyIcons[i].ShowVisualElement();
                    currencyIcons[i].style.backgroundImage = currencyDisplayList[i].icon.texture;
                }
                else
                {
                    //   Debug.LogError("UIAtavismCurrencyDisplay.UpdateCurrencies: hide " + i);
                    currencyValues[i].HideVisualElement();
                    currencyIcons[i].HideVisualElement();
                }
            }
        }
    }

}