using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Atavism
{

    public class UGUICurrency : MonoBehaviour
    {

        public Image image;
        public Text amountText;
        [SerializeField] TextMeshProUGUI TMPAmountText;

        private CurrencyDisplay currencyDisplay;

        private Currency currency;
        // Use this for initialization
        void Start()
        {
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
        }
        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                if (currency != null)
                {
                    if (image != null)
                        image.sprite = currency.Icon;
                }
            }
        }

        public void UpdateCurrency(Currency c)
        {
            if (c != null)
            {
                if (image != null)
                    image.sprite = c.Icon;
                if (amountText != null)
                    amountText.text = c.Current.ToString();
                if (TMPAmountText != null)
                    TMPAmountText.text = c.Current.ToString();
            }
        }

        public void SetCurrencyDisplayData(CurrencyDisplay display)
        {
            currencyDisplay = display;
            if (amountText != null)
                amountText.text = display.amount.ToString();
            if (TMPAmountText != null)
                TMPAmountText.text = display.amount.ToString();
            if (image != null)
                image.sprite = display.icon;
        }
    }
}