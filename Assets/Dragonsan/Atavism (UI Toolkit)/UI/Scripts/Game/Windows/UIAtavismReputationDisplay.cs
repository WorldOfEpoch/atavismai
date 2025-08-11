using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismReputationDisplay 
    {
        private Label m_name;
        private Label m_value;
        private VisualElement m_container;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_container = visualElement;
            m_name = visualElement.Q<Label>("reputation-name");
            m_value = visualElement.Q<Label>("reputation-value");
        }

        public void SetData(QuestRepRewardEntry data )
        {
#if AT_I2LOC_PRESET
                 if(m_name!=null)    m_name.text = I2.Loc.LocalizationManager.GetTranslation(data.name);
#else
            if(m_name!=null) m_name.text = data.name;
#endif
            if (m_value != null)
            {
                m_value.RemoveFromClassList("reputation-positive");
                m_value.RemoveFromClassList("reputation-negative");
                m_value.text = data.count.ToString();
                if (data.count > 0)
                {
                    m_value.AddToClassList("reputation-positive");
                }
                else
                {
                    m_value.AddToClassList("reputation-negative");
                    
                }
            }
        }

        public void Show()
        {
            m_container.ShowVisualElement();
        }

        public void Hide()
        {
            m_container.HideVisualElement();
        }

    }

}