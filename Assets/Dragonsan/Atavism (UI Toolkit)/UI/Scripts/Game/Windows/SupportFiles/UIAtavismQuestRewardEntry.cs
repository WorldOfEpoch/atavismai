using Atavism;
using Atavism.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismQuestRewardEntry
    {
        private Button m_container;
        private Label m_itemName;
        private VisualElement m_itemIcon;
        private VisualElement m_itemQuality;
        private Label m_itemCount;
        private QuestRewardEntry data;
        OnItemClicked itemClickedFunction;

        public void SetVisualElement(VisualElement visualElement)
        {
            m_container = visualElement.Q<Button>("quest-reward-slot");
            m_itemName = visualElement.Q<Label>("reward-item-name");
            m_itemIcon = visualElement.Q<VisualElement>("slot-item-image");
            m_itemQuality = visualElement.Q<VisualElement>("slot-image");
            m_itemCount = visualElement.Q<Label>("slot-item-count");
            m_container.clicked += ItemClicked;
#if !AT_MOBILE            
            m_container.RegisterCallback<PointerEnterEvent>((e) =>
            {
                if (data != null && data.item != null)
                {
                    data.item.ShowUITooltip(m_container);
                }
            });
            m_container.RegisterCallback<PointerLeaveEvent>((e) =>
            {
                if (data != null && data.item != null)
                {
                    data.item.HideUITooltip();
                }
            });
#endif
        }

        public void Show()
        {
            m_container.ShowVisualElement();
        }

        public void Hide()
        {
            m_container.HideVisualElement();
        }

        public void SetData(QuestRewardEntry data, OnItemClicked itemClickedFunction)
        {
            this.data = data;
            if (m_itemName != null && data.item != null)
            {
#if AT_I2LOC_PRESET
            m_itemName.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + data.item.name);
#else
                m_itemName.text = data.item.name;
#endif
            }

            if (m_itemIcon != null)
            {
                if (data.item != null)
                {
                    if (data.item.Icon != null)
                        m_itemIcon.style.backgroundImage = data.item.Icon.texture;
                    else
                        m_itemIcon.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
                    //   this.itemIcon.sprite = item.icon;
                    m_itemIcon.ShowVisualElement();
                }
                else
                {
                    m_itemIcon.HideVisualElement();
                }
            }

            if (m_itemCount != null)
            {
                if (data.item != null && data.item.Count > 1)
                    m_itemCount.text = data.item.Count.ToString();
                else
                    m_itemCount.text = "";
            }

            if (m_itemQuality != null && data.item != null)
                m_itemQuality.style.unityBackgroundImageTintColor =
                    AtavismSettings.Instance.ItemQualityColor(data.item.Quality);
            this.itemClickedFunction = itemClickedFunction;
            if (itemClickedFunction != null)
                m_container.SetEnabled(true);
            else
                m_container.SetEnabled(false);
        }

        public void SetExpData(int count)
        {
            if (m_itemName != null)
            {
#if AT_I2LOC_PRESET
                m_itemName.text = count.ToString() + " " + I2.Loc.LocalizationManager.GetTranslation("EXP");
#else
                m_itemName.text = count.ToString() + " EXP";
                ;
#endif
            }

            if (m_itemIcon != null)
            {
                m_itemIcon.style.backgroundImage = AtavismSettings.Instance.expIcon.texture;
                m_itemIcon.ShowVisualElement();
            }
            else
            {
                m_itemIcon.HideVisualElement();
            }

            if (m_itemCount != null)
                m_itemCount.text = count.ToString();

            if (m_itemQuality != null)
                m_itemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(0);
            m_container.SetEnabled(false);
        }

        public void Selected(bool selected)
        {
            if (m_container != null)
            {
                if (selected)
                {
                    m_container.AddToClassList("quest-reward-slot-selected");
                }
                else
                {
                    m_container.RemoveFromClassList("quest-reward-slot-selected");
                }
            }
        }
        
        public void ItemClicked()
        {
            if (itemClickedFunction != null && data != null)
                itemClickedFunction(data.item);
#if AT_MOBILE
            data.item.ShowUITooltip(m_container);
#endif
        }
    }
}