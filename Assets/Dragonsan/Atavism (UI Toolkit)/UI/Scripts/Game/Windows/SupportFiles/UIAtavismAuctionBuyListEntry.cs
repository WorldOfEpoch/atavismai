using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismAuctionBuyListEntry
    {
        private Label m_itemName;
        private Label m_itemCount;
        private VisualElement m_itemIcon;
        private VisualElement m_itemQuality;
        private UIAtavismCurrencyDisplay m_currecny;
        private VisualElement m_Root;
        private Auction data;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<PointerEnterEvent>((e) =>
            {
                if (data != null && data.item != null)
                {
                    data.item.ShowUITooltip(m_Root);
                }
            });
            m_Root.RegisterCallback<PointerLeaveEvent>((e) =>
            {
                if (data != null && data.item != null)
                {
                    data.item.HideUITooltip();
                }
            });

            m_itemName = visualElement.Q<Label>("item-name");
            VisualElement slot = visualElement.Q<VisualElement>("slot-container");
            if (slot != null)
            {
                m_itemIcon = slot.Q<VisualElement>("icon");
                m_itemQuality = slot.Q<VisualElement>("quality");
                m_itemCount = slot.Q<Label>("count");
            }

            VisualElement curr = visualElement.Q<VisualElement>("currency-panel");
            m_currecny = new UIAtavismCurrencyDisplay();
            m_currecny.SetVisualElement(curr);
            m_currecny.ReverseOrder = true;
        }

        public void SetData(Auction data)
        {
            SetData(data, false);
        }

        public void SetData(Auction data, bool inventory)
        {
            this.data = data;
            if (m_itemName != null)
            {
                m_itemName.text = data.item.name;
                m_itemName.style.color = AtavismSettings.Instance.ItemQualityColor(data.item.quality);
            }

            if (m_itemIcon != null)
            {
                if (data.item.Icon != null)
                    m_itemIcon.style.backgroundImage = data.item.Icon.texture;
            }

            if (m_itemQuality != null)
            {
                m_itemQuality.style.unityBackgroundImageTintColor =
                    AtavismSettings.Instance.ItemQualityColor(data.item.quality);
            }

            // Debug.LogError("UIAtavismAuctionBuyListEntry.SetData "+data.item.name+" "+data.item.quality+" "+data.count+" "+data.item.Count);
            if (m_itemCount != null)
            {

                if (data.count > 0)
                    m_itemCount.text = data.count.ToString();
                else if (data.item.Count > 1)
                    m_itemCount.text = data.item.Count.ToString();
                else
                    m_itemCount.text = "";
            }

            if (m_currecny != null)
            {
                m_currecny.SetData(data.currency, data.buyout);
            }
        }
    }
}