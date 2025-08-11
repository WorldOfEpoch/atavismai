using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismAdminChooseListEntry
    {
        Label m_itemName;
        private VisualElement m_itemIcon;
        private VisualElement m_itemQuality;
        private VisualElement m_iconPanel;

        public void SetVisualElement(VisualElement visualElement)
        {
            m_iconPanel = visualElement.Q<VisualElement>("choose-slot-panel");
            m_itemName = visualElement.Q<Label>("choose-item-name");
            m_itemIcon = visualElement.Q<VisualElement>("choose-item-icon");
            m_itemQuality = visualElement.Q<VisualElement>("choose-quality-icon");
        }

        public void SetData(AdminChooseData data)
        {
            if (data.m_type == AdminChooseType.Weather)
            {
                m_iconPanel.HideVisualElement();
            }
            else
            {
                m_iconPanel.ShowVisualElement();
            }
            m_itemName.text = data.m_Name;
            if (data.m_Image != null)
                m_itemIcon.style.backgroundImage = data.m_Image.texture;
            m_itemQuality.style.unityBackgroundImageTintColor = data.m_Quality;
        }
    }
}