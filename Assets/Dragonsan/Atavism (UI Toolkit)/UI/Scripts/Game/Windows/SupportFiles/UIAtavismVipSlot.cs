using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismVipSlot 
    {
        public Label m_name;
        public Label m_vipA;
        public Label m_vipB;
        private VisualElement m_Root;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_name = visualElement.Q<Label>("vip-name");
            m_vipA = visualElement.Q<Label>("vip-bonus-a");
            m_vipB = visualElement.Q<Label>("vip-bonus-b");
        }
        public void UpdateDetaile(string Name, string vipABonus, string vipBBonus)
        {
            Show();
            if (m_name!=null)
            {
                m_name.text = Name;
            }
            if (m_vipA!=null)
            {
                m_vipA.text = vipABonus;
            }
            if (m_vipB!=null)
            {
                m_vipB.text = vipBBonus;
            }

        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }
        
        public void Hide()
        {
            m_Root.HideVisualElement();
        }
    }
}