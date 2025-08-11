using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMerchantItemEntry 
    {

        //public Text name;
        public Label m_name;
        [SerializeField] Label m_taught;
        public UIAtavismMerchantItemSlot itemSlot;
        UIAtavismCurrencyDisplay currencyDisplays;
        //MerchantItem merchantItem;
        private VisualElement m_Root;
        // Use this for initialization
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_name = m_Root.Q<Label>("item-name");
            m_taught = m_Root.Q<Label>("thught");
            VisualElement slot  = m_Root.Q<VisualElement>("left-panel");
            itemSlot = new UIAtavismMerchantItemSlot();
            itemSlot.SetVisualElement(slot);
            VisualElement currency  = m_Root.Q<VisualElement>("currency");
            currencyDisplays = new UIAtavismCurrencyDisplay();
            currencyDisplays.SetVisualElement(currency);
            currencyDisplays.ReverseOrder = true;
            m_Root.RegisterCallback<MouseUpEvent>(Activate);
        //    Debug.LogError("UIAtavismMerchantItemEntry ");
            //   parentScript = parent;
        }

        private void Activate(MouseUpEvent evt)
        {
            itemSlot.Activate();
        }

        public void UpdateMerchantItemData(MerchantItem merchantItem, UIAtavismMerchantPanel merchantFrame)
        {
            //	this.merchantItem = merchantItem;
            itemSlot.UpdateMerchantItemData(merchantItem, merchantFrame);

            AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(merchantItem.itemID);
#if AT_I2LOC_PRESET
        if (m_name!=null)m_name.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name);
#else
            if (m_name != null)
                m_name.text = aii.name;
#endif
            currencyDisplays.SetData(merchantItem.purchaseCurrency, merchantItem.cost);
            if (aii.GetEffectPositionsOfTypes("UseAbility").Count > 0)
            {
                if (aii.name.IndexOf("TeachAbility") > -1)
                {
                    int abilityID = int.Parse(aii.itemEffectNames[aii.GetEffectPositionsOfTypes("UseAbility")[0]]);
                    //  AtavismAbility aa = Abilities.Instance.GetAbility(abilityID);
                    AtavismAbility paa = Abilities.Instance.GetPlayerAbility(abilityID);
                    if (paa != null)
                    {
                        if (m_taught != null)
                        {
                            m_taught.visible = true;
                            return;
                        }
                    }
                }
            }

            if (m_taught != null)

                m_taught.visible = false;
        }
        
        public void UpdateShopItemData(ShopItem merchantItem, UIAtavismShopWindow merchantFrame)
        {
            //	this.merchantItem = merchantItem;
            itemSlot.UpdateShopItemData(merchantItem, merchantFrame);

            AtavismInventoryItem aii = merchantItem.item;
       //     Debug.LogError("UpdateShopItemData " + aii.name+" "+merchantItem.purchaseCurrency+"  "+merchantItem.cost);
#if AT_I2LOC_PRESET
        if (m_name!=null)m_name.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + aii.name);
#else
            if (m_name != null)
                m_name.text = aii.name;
#endif
            currencyDisplays.SetData(merchantItem.purchaseCurrency, merchantItem.cost);
            if (aii.GetEffectPositionsOfTypes("UseAbility").Count > 0)
            {
              /*  if (aii.name.IndexOf("TeachAbility") > -1)
                {*/
              string ability = aii.itemEffectNames[aii.GetEffectPositionsOfTypes("UseAbility")[0]];

              if (ability.Length > 0)
              {
                  int abilityID = int.Parse(aii.itemEffectNames[aii.GetEffectPositionsOfTypes("UseAbility")[0]]);
                  //  AtavismAbility aa = Abilities.Instance.GetAbility(abilityID);
                  AtavismAbility paa = Abilities.Instance.GetPlayerAbility(abilityID);
                  if (paa != null)
                  {
                      if (m_taught != null)
                      {
                          m_taught.visible = true;
                          //  return;
                      }
                  }
              }
              // }
            }else if (aii.GetEffectPositionsOfTypes("Blueprint").Count > 0)
            {
                if (CheckRecipe(int.Parse(aii.itemEffectValues[aii.GetEffectPositionsOfTypes("Blueprint")[0]])))
                {
                    if (m_taught != null)
                    {
                        m_taught.visible = true;
                    }
                }
            }
            else
            {
                if (m_taught != null)
                    m_taught.visible = false;
            }
        }
        
        bool CheckRecipe(int recipeId)
        {
            if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetPlayerObject().PropertyExists("recipes"))
            {
                LinkedList<object> recipes_prop = (LinkedList<object>)ClientAPI.GetPlayerObject().GetProperty("recipes");
                foreach (string recipeString in recipes_prop)
                {
                    int recipeID = int.Parse(recipeString);
                    if (recipeID == recipeId)
                        return true;
                }
            }
            return false;
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }
    }
}