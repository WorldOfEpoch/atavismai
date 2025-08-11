using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismLimitDisplay 
    {
        public Label m_nameText;
        public Label m_valueText;
        private VisualElement m_Root;
        public UIAtavismLimitDisplay(VisualElement visualElement , VisualElement grid)
        {
            m_Root = visualElement;
            m_nameText = visualElement.Q<Label>("limit-name");
            m_valueText = visualElement.Q<Label>("limit-value");
            grid.Add(m_Root);
        }
        
        public void Display(string value, string name)
        {
            if (m_valueText!=null)
            {
                m_valueText.text = value;
            }

            if (m_nameText!=null)
            {
#if AT_I2LOC_PRESET
                m_nameText.text = I2.Loc.LocalizationManager.GetTranslation(name);
#else
                m_nameText.text = name;
#endif
               
            }
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