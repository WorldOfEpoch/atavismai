using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismCreateShopEntity 
    {
        Label m_Name;
        UIAtavismShopItemSlot itemSlot;
        UIAtavismCurrencyDisplay currencyDisplays;
        public UIAtavismShopItemSlot ItemSlot => itemSlot;
        
        private VisualElement m_Root;
        // Start is called before the first frame update
      
        public void SetVisualElement(VisualElement visualElement)
        {
            //Search the root for the SlotContainer Visual Element
            m_Root = visualElement;
            m_Name = m_Root.Q<Label>("item-name");
            VisualElement slot  = m_Root.Q<VisualElement>("left-panel");
            itemSlot = new UIAtavismShopItemSlot(slot);
          
            VisualElement currency  = m_Root.Q<VisualElement>("currency");
            currencyDisplays = new UIAtavismCurrencyDisplay();
            currencyDisplays.SetVisualElement(currency);
            currencyDisplays.ReverseOrder = true;
         
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }
        public void Hide()
        {
            m_Root.HideVisualElement();
        }
        public void AssignShop(UIAtavismCreateShop acs, int pos)
        {
            if (itemSlot != null)
                itemSlot.AssignShop(acs,pos);
        }

        public void ResetSlot()
        {
            if (m_Name != null)
                m_Name.text = "";
            if (itemSlot != null)  itemSlot.Discarded();
            
            if (currencyDisplays != null)  currencyDisplays.Hide();
            // for (int i = 0; i < currencyDisplays.Count; i++)
            // {
            //         currencyDisplays[i].gameObject.SetActive(false);
            // }
        }

        public void updateDisplay(ShopItem merchantItem)
        {
            if (merchantItem == null)
            {
                if (m_Name != null)
                    m_Name.text = "";
                if (itemSlot != null)   itemSlot.ResetSlot();
                if (currencyDisplays != null)   currencyDisplays.Hide();
                // for (int i = 0; i < currencyDisplays.Count; i++)
                //     currencyDisplays[i].gameObject.SetActive(false);
                return;
            }
            AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(merchantItem.itemID);
            itemSlot.UpdateSlotData(aii);
#if AT_I2LOC_PRESET
        if (m_Name!=null)m_Name.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name);
#else

            if (m_Name != null)
                m_Name.text = aii.name;
#endif
            currencyDisplays.SetData(merchantItem.purchaseCurrency, merchantItem.cost);
            currencyDisplays.Show();
            // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(merchantItem.purchaseCurrency, merchantItem.cost);
            // for (int i = 0; i < currencyDisplays.Count; i++)
            // {
            //     if (i < currencyDisplayList.Count)
            //     {
            //         currencyDisplays[i].gameObject.SetActive(true);
            //         currencyDisplays[i].SetCurrencyDisplayData(currencyDisplayList[i]);
            //     }
            //     else
            //     {
            //         currencyDisplays[i].gameObject.SetActive(false);
            //     }
            // }
        }
    }
}