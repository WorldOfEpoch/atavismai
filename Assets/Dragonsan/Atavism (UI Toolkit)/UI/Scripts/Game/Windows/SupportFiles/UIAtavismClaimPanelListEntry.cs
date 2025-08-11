using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
//using Button = UnityEngine.UI.Button;

namespace Atavism.UI
{
    public class UIAtavismClaimPanelListEntry
    {
        // public Button buttonPay;
        public Button payButton;
        public Label claimName;
        private VisualElement claimNameRow;
        public Label taxPayedTime;
        private VisualElement taxPaidRow;
        public Label taxInfo;
        private VisualElement taxInfoRow;
        
        
        
        // public UGUIMiniTooltipEvent tooltip;
        private float time = 0;
        private int id = -1;
        private long taxAmount ;
        private long taxInterval;
        private long taxPeriodPay;
        private int taxCurrency;
        // Update is called once per frame
        public void SetVisualElement(VisualElement visualElement)
        {
           // uiRoot = visualElement;
           payButton = visualElement.Q<Button>("pay-button");
           claimName = visualElement.Q<Label>("claim-name");
           claimNameRow = visualElement.Q<VisualElement>("claim-name-row");
           taxPayedTime = visualElement.Q<Label>("tax-paid");
           taxPaidRow = visualElement.Q<VisualElement>("tax-paid-row");
           taxInfoRow = visualElement.Q<VisualElement>("tax-info-row");
           taxInfo = visualElement.Q<Label>("tax-info");
        }
        public void UpdateDisplay(string name, int claimId, float time,long taxAmount,int taxCurrency,long taxInterval,long taxPeriodPay)
        {
            id = claimId;
            this.time = time;
            this.taxAmount = taxAmount;
            this.taxCurrency = taxCurrency;
            this.taxInterval = taxInterval;
            this.taxPeriodPay = taxPeriodPay;
            if (claimName != null)
            {
                if (name.Length == 0)
                    name = "No name";
#if AT_I2LOC_PRESET
                claimName.text = I2.Loc.LocalizationManager.GetTranslation("name");
#else
                claimName.text = name;
#endif         
                
            }
            // if (tooltip != null)
                // tooltip.dectName = name;
         
            long days = 0;
            long hour = 0;
            if (taxInterval > 24)
            {
                days = (long) (taxInterval / 24F);
                hour =  (taxInterval - (days * 24));
            }
            else
            {
                hour = taxInterval;
            }
            string cost = Inventory.Instance.GetCostString(taxCurrency, taxAmount);
            
            if (taxAmount > 0)
            {
                
                long _days = 0;
                long _hour = 0;
                if (taxPeriodPay > 24)
                {
                    _days = (long) (taxPeriodPay / 24F);
                    _hour =  (taxPeriodPay - (_days * 24));
                }
                else
                {
                    _hour = taxPeriodPay;
                }
#if AT_I2LOC_PRESET
                taxInfo.text = cost + " "+I2.Loc.LocalizationManager.GetTranslation("per")+" " + (days > 0 ? days > 1 ? days + " "+I2.Loc.LocalizationManager.GetTranslation("days")+" " : days + " "+
                        I2.Loc.LocalizationManager.GetTranslation("day")+" " : "") + (hour > 0 ? hour + " "+I2.Loc.LocalizationManager.GetTranslation("hour") : "")+
                                  ". "+I2.Loc.LocalizationManager.GetTranslation("Can be paid")+" "+ (_days > 0 ? _days > 1 ? _days + " days " : _days + " day " : "") + (_hour > 0 ? _hour + " hour" : "")+" "+
                                 I2.Loc.LocalizationManager.GetTranslation("before tax expire");
#else
                taxInfo.text = cost + " per " + (days > 0 ? days > 1 ? days + " days " : days + " day " : "") + (hour > 0 ? hour + " hour" : "")+
                               ". Can be paid "+ (_days > 0 ? _days > 1 ? _days + " days " : _days + " day " : "") + (_hour > 0 ? _hour + " hour" : "")+" before tax expires";

#endif
            }
            else
            {
#if AT_I2LOC_PRESET
                taxInfo.text = I2.Loc.LocalizationManager.GetTranslation("No Tax");
#else
                taxInfo.text = "No tax";

#endif
            }

            if (taxAmount <=0)
            {
                if (payButton != null)
                    payButton.HideVisualElement();
            }
            else
            {
                if (payButton != null)
                    payButton.ShowVisualElement();
            }
            // StopAllCoroutines();
            // StartCoroutine(UpdateTimer());
        }

        public IEnumerator UpdateTimer()
        {
            WaitForSeconds delay = new WaitForSeconds(1f);
            while (true)
            {
                if (time > Time.time)
                {
                    float _time = time - Time.time;
                    int days = 0;
                    int hour = 0;
                    int minute = 0;
                    int secound = 0;
                    if (_time > 24 * 3600)
                    {
                        days = (int) (_time / (24F * 3600F));
                    }

                    if (_time - days * 24 * 3600 > 3600)
                        hour = (int) ((_time - days * 24 * 3600) / 3600F);
                    if (_time - days * 24 * 3600 - hour * 3600 > 60)
                        minute = (int) (_time - days * 24 * 3600 - hour * 3600) / 60;
                    secound = (int) (_time - days * 24 * 3600 - hour * 3600 - minute * 60);

#if AT_I2LOC_PRESET
                     taxPayedTime.text = (days > 0 ? days > 1 ? days + " "+I2.Loc.LocalizationManager.GetTranslation("days")+" " : days + " "+
                        I2.Loc.LocalizationManager.GetTranslation("day")+" " : "") + (hour > 0 ? hour + " "+I2.Loc.LocalizationManager.GetTranslation("hour") : "") + (minute > 0 ? minute + " "+I2.Loc.LocalizationManager.GetTranslation("minute") : "");
#else
                    taxPayedTime.text = (days > 0 ? days > 1 ? days + " days " : days + " day " : "") + (hour > 0 ? hour + " h " : "") + (minute > 0 ? minute + " m" : "");

#endif
                    if (taxPayedTime.text.Length == 0)
                    {
                        if(taxPaidRow != null)  taxPaidRow.HideVisualElement();
                    }
                    else
                    {
                        if(taxPaidRow != null) taxPaidRow.ShowVisualElement();
                    }
                    if (time - Time.time > taxPeriodPay * 3600F)
                    {
                      
                            
                        if (payButton!=null)
                        {
                            // payButton.interactable = false;
                            float t = time - Time.time - taxPeriodPay * 3600f;

                            days = 0;
                            hour = 0;
                            minute = 0;
                            secound = 0;
                            if (_time > 24 * 3600)
                            {
                                days = (int) (t / (24F * 3600F));
                            }

                            if (t - days * 24 * 3600 > 3600)
                                hour = (int) ((t - days * 24 * 3600) / 3600F);
                            if (t - days * 24 * 3600 - hour * 3600 > 60)
                                minute = (int) (t - days * 24 * 3600 - hour * 3600) / 60;
                            secound = (int) (t - days * 24 * 3600 - hour * 3600 - minute * 60);

                            payButton.text = "Pay" + (days > 0 ? "(" + days + "days)" : hour > 0 ? "(" + hour + "h)" : minute > 0 ? "(" + minute + "m)" : secound > 0 ? "(" + secound + "s)" : "") + "";
                        }
                    }
                    else
                    {
                        if (payButton != null)
                        {
                            // payButton.interactable = true;
                        }

                        if (payButton != null)
                        {
#if AT_I2LOC_PRESET
                     payButton.text = I2.Loc.LocalizationManager.GetTranslation("PAY");
#else
                            payButton.text = "PAY";
#endif
                        }
                    }
                }
                else
                {
                    if (taxPayedTime != null)
                    {
                        taxPayedTime.text = "";
                        
                    }
                    if(taxPaidRow != null)
                        taxPaidRow.HideVisualElement();
                }

                yield return delay;
            }
        }
        public void Click()
        {
            WorldBuilder.Instance.SendPayTaxForClaim(id, false);
        }

        public void SetData(ClaimListEntry cle)
        {
            UpdateDisplay(cle.name, cle.id, cle.time,cle.taxAmount,cle.taxCurrency,cle.taxInterval,cle.taxPeriodPay);
        }
    }
}

