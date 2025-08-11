using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public delegate void OnItemClicked(AtavismInventoryItem item);

    /// <summary>
    /// Handles the display of an item in UGUI, such as setting the texture and the count label. 
    /// This script can only be used on a button.
    /// </summary>
    public class UIAtavismItemSlotDisplay 
    {

        public VisualElement  m_itemIcon;
        public VisualElement m_itemQuality;
        public Label  m_countText;
        public Label  m_itemName;

        OnItemClicked itemClickedFunction;
        
        AtavismInventoryItem item;
        int slotNum;
        bool mouseEntered = false;
        // [SerializeField] bool resetDisableQuality = false;
        // [SerializeField] Color selectedColor = Color.yellow;
        VisualElement m_Root;

        // Use this for initialization
        // void Start()
        // {
        //     AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
        // }
        //
        // private void OnDestroy()
        // {
        //     AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
        // }

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            
            m_itemName = visualElement.Q<Label>("item-name");
            m_itemIcon = visualElement.Q<VisualElement>("icon");
            m_countText = visualElement.Q<Label>("count");
            m_itemQuality = visualElement.Q<VisualElement>("quality");
#if AT_MOBILE   
            m_Root.RegisterCallback<ClickEvent>(  e =>
                {
                     item.ShowUITooltip(m_Root); 
                });
#endif
#if !AT_MOBILE            
            m_Root.RegisterCallback<MouseEnterEvent>(
                e =>
                {
                    MouseEntered = true;
                });
            m_Root.RegisterCallback<MouseLeaveEvent>(
                e =>
                {
                    MouseEntered = false;
                });
#endif 
        }
        
        
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (item != null)
                {
                    // item.icon = AtavismPrefabManager.Instance.GetItemIconByID(item.templateId);
                    m_itemIcon.style.backgroundImage = item.Icon.texture;
        
                    if (this.m_itemIcon != null)
                    {
                        m_itemIcon.visible = true;
                        if (item.Icon != null)
                            m_itemIcon.style.backgroundImage = item.Icon.texture;
                        else
                            m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                    }
        
                
                }
            }
        }
#if AT_MOBILE
        public void OnPointerClick(PointerEventData eventData) //PopuGames
        {
            item.ShowUITooltip(m_Root); //PopuGames
           
        }
#endif
        public void OnPointerEnter(PointerEventData eventData)
        {
#if !AT_MOBILE             
            MouseEntered = true;
#endif            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if !AT_MOBILE             
            MouseEntered = false;
#endif            
        }

        public void ItemClicked()
        {
            if (itemClickedFunction != null)
                itemClickedFunction(item);
#if AT_MOBILE
            item.ShowUITooltip(m_Root);
#endif
        }
        public void Reset()
        {
            this.item = null;
            if (this.m_itemIcon != null)
            {
                this.m_itemIcon.visible = false;
                //    this.itemIcon.sprite = null;
            }
            // }
            if (m_countText != null)
            {
                m_countText.text = "";
            }
         
            if (m_itemName != null)
            {
                m_itemName.text = "";
            }
        
            this.itemClickedFunction = null;
            if (m_itemQuality != null)
            {
                this.m_itemQuality.style.unityBackgroundImageTintColor = Color.white;
              
            }
        

        }

        public void SetItemData(AtavismInventoryItem item, OnItemClicked itemClickedFunction)
        {
            this.item = item;
            if (item == null)
            {
                //   if (this.itemIcon != null)
                //       this.itemIcon.enabled = false;
             
                if (m_itemQuality != null)
                {
                    m_itemQuality.style.unityBackgroundImageTintColor = Color.white;
                }
            }
         
            if (this.m_itemIcon != null)
            {
                if (item != null)
                {
                    if (item.Icon != null)
                        this.m_itemIcon.style.backgroundImage = item.Icon.texture;
                    else
                        this.m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                    //   this.itemIcon.sprite = item.icon;
                    this.m_itemIcon.visible = true;
                }
                else
                {
                    this.m_itemIcon.visible = false;
                }
            }
            else
            {
            }


            if (m_countText != null)
            {
                if (item != null && item.Count > 1)
                    m_countText.text = item.Count.ToString();
                else
                    m_countText.text = "";
            }
           

            if (m_itemName != null && item != null)
            {
#if AT_I2LOC_PRESET
            m_itemName.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name);
#else
                m_itemName.text = item.name;
#endif
            }
            if (m_itemQuality != null && item != null)
            {
                this.m_itemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.Quality);
                // this.m_itemQuality.enabled = true;

            }
            this.itemClickedFunction = itemClickedFunction;
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        // public void Selected(bool select)
        // {
        //     if (select)
        //     { if (GetComponent<Image>() != null)
        //         GetComponent<Image>().color = selectedColor;
        //     }
        //     else
        //     { if (GetComponent<Image>() != null)
        //         GetComponent<Image>().color = Color.white;
        //     }
        // }
        public void Show()
        {
            m_Root.ShowVisualElement();
        }
        public void Hide()
        {
            m_Root.HideVisualElement();
        }
        public bool MouseEntered
        {
            get
            {
                return mouseEntered;
            }
            set
            {
                mouseEntered = value;
                if (mouseEntered && item != null)
                {
                    item.ShowUITooltip(m_Root);
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}