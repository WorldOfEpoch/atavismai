using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

     public class UIAtavismTradeOffer //: MonoBehaviour
    {

        public UIAtavismItemDisplay itemDisplay;
        public Label m_nameLabel;
        private VisualElement m_Root;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_nameLabel = m_Root.Q<Label>("name");
            itemDisplay = m_Root.Q<UIAtavismItemDisplay>("item");
        }
        
        public void UpdateTradeOfferData(AtavismInventoryItem item)
        {
            if (item == null)
            {
                if (m_nameLabel != null)
                    m_nameLabel.text = "";
                itemDisplay.Reset();
            }
            else
            {
                if (m_nameLabel != null)
                    m_nameLabel.text = item.name;
                itemDisplay.SetItemData(item);
            }
        }
    }
}