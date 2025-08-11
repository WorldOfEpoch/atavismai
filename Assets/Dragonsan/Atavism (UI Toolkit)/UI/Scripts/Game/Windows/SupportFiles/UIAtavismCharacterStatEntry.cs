using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismCharacterStatEntry
    {
        Label m_statText;
        public Label StatText => m_statText;
        Label m_statValueText;
        public Label StatValueText => m_statValueText;
        private VisualElement m_Root;
        
        public void SetVisualElement(VisualElement root)
        {
            m_Root = root;

            m_statText = root.Q<Label>("stat-label");
            m_statValueText = root.Q<Label>("stat-label-value");
        }
        
        public void UpdateStat(string statName, string statValue )
        {
            string name = statName;
#if AT_I2LOC_PRESET
            if(name.Length>0)
            name = I2.Loc.LocalizationManager.GetTranslation(statName);
            
#endif
            m_statText.text = name;
            m_statText.tooltip = name;
            m_statValueText.text = statValue;
            m_statValueText.tooltip = name;
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
        }

        public void Show()
        {m_Root.ShowVisualElement();
        }
    }
}