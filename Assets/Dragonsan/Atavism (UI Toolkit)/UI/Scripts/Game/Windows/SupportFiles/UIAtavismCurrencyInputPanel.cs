using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public delegate void ResponseOnChange();
    public class UIAtavismCurrencyInputPanel 
    {

        public List<UITextField> inputFields = new List<UITextField>();
        public List<VisualElement> icons = new List<VisualElement>();
        public bool allowMoreThanPlayersCurrency = false;
        List<int> currencyIDs = new List<int>();
        List<int> currencyAmounts = new List<int>();
        private VisualElement m_Root;
        public bool SetCurrencyReverseOrder = false;
        private ResponseOnChange _responseOnChange;

        public void SetOnChange(ResponseOnChange onChange)
        {
            _responseOnChange = onChange;
        }
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            for (int i = 1; i <= 10; i++)
            {
                UITextField value = m_Root.Q<UITextField>("currency-input-"+i);
                VisualElement icon = m_Root.Q<VisualElement>("currency-icon-"+i);
                if (value != null)
                {
                    value.RegisterValueChangedCallback(CurrencyChange);
                    inputFields.Add(value);
                    icons.Add(icon);
                }
            }
        }

        private void CurrencyChange(ChangeEvent<string> evt)
        {
            if (_responseOnChange!=null)
                _responseOnChange();
        }

        
        /// <summary>
        /// Sets the currencies icons from currency type. Call this function first
        /// </summary>
        /// <param name="currencyType"></param>
        public void SetCurrencies(int currencyType)
        {
           int currencyGroup = Inventory.Instance.GetCurrencyGroup(AtavismAuction.Instance.GetCurrencyType);
           if (currencyGroup > 0)
           {
               SetCurrencies(Inventory.Instance.GetCurrenciesInGroup(currencyGroup));
           }
           else
           {
               List<Currency> currencies = new List<Currency>();
               currencies.Add(Inventory.Instance.GetCurrency(currencyType));
               SetCurrencies(currencies);
           }
         }
        
        
        /// <summary>
        /// Sets the currencies icons. Call this function first
        /// </summary>
        /// <param name="currencyList">Currency list.</param>
        public void SetCurrencies(List<Currency> currencyList)
        {
            if(!SetCurrencyReverseOrder)
                currencyList.Reverse();
            currencyIDs = new List<int>();
            currencyAmounts = new List<int>();
            for (int i = 0; i < currencyList.Count; i++)
            {
                currencyIDs.Add(currencyList[i].id);
                currencyAmounts.Add(0);
                if (i < icons.Count)
                {
                    inputFields[ i ].ShowVisualElement();
                    icons[ i ].ShowVisualElement();
                    icons[ i ].style.backgroundImage = new StyleBackground(currencyList[i].Icon);
                }
            }

            for (int i = currencyList.Count; i < icons.Count; i++)
            {
                inputFields[ i ].HideVisualElement();
                icons[ i ].HideVisualElement();
            }
            ClearCurrencyAmounts();
        }

        /// <summary>
        /// Sets the currency amounts if the input should be filled before the player types anything in.
        /// Should be called after SetCurrencies.
        /// </summary>
        /// <param name="currencyID">Currency I.</param>
        /// <param name="amount">Amount.</param>
        public void SetCurrencyAmounts(int currencyID, long amount)
        {
            foreach (UITextField inputField in inputFields)
            {
                if (inputField != null)
                    inputField.SetValueWithoutNotify("0");
            }
            List<Vector2> convertedCurrencies = Inventory.Instance.GetConvertedCurrencyValues(currencyID, amount);
            foreach (Vector2 currency in convertedCurrencies)
            {
                for (int i = 0; i < currencyIDs.Count; i++)
                {
                    if (currencyIDs[i] == (int)currency.x)
                    {
                        currencyAmounts[i] = (int)currency.y;
                        if (inputFields.Count > i)
                            inputFields[inputFields.Count - i - 1].value = currencyAmounts[i].ToString();
                    }
                }
            }
        }

        public void ClearCurrencyAmounts()
        {
            foreach (UITextField inputField in inputFields)
            {
                if (inputField != null)
                    inputField.SetValueWithoutNotify("0");
            }
  
            for (int i = 0; i < currencyAmounts.Count; i++)
            {
                currencyAmounts[i] = 0;
            }
        }

        // public void SetCurrency1Amount(string amount)
        // {
        //     currencyAmounts[0] = int.Parse(amount);
        // }
        //
        // public void SetCurrency2Amount(string amount)
        // {
        //     currencyAmounts[1] = int.Parse(amount);
        // }
        //
        // public void SetCurrency3Amount(string amount)
        // {
        //     currencyAmounts[2] = int.Parse(amount);
        // }
        //
        // void CheckPlayerHasCurrency()
        // {
        //
        // }

        public void GetCurrencyAmount(out int currencyID, out long currencyAmount)
        {
            for (int i = 0; i < currencyIDs.Count; i++)
            {
                if (inputFields.Count > i)
                    currencyAmounts[i] = inputFields[ i ].value.Length>0?int.Parse(inputFields[ i ].value):0;
            }
            List<Vector2> currencies = new List<Vector2>();
            for (int i = 0; i < currencyIDs.Count; i++)
            {
                currencies.Add(new Vector2(currencyIDs[i], currencyAmounts[i]));
            }
            Inventory.Instance.ConvertCurrenciesToBaseCurrency(currencies, out currencyID, out currencyAmount);
        }
    }
}