using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismCraftSkillSlot
    {
        Label m_skillName;
        Label m_skillLevel;
        UIProgressBar m_skillFill;
        private VisualElement m_Root;

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            // m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            // m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            // m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);

            m_skillName = visualElement.Q<Label>("name");
            m_skillFill = visualElement.Q<UIProgressBar>("progress");
            m_skillLevel = visualElement.Q<Label>("level");
        }

        public void UpdateDisplay(Skill skill)
        {
            if(Skills.Instance.PlayerSkills.ContainsKey(skill.id))
               skill = Skills.Instance.PlayerSkills[skill.id];
            //   Debug.LogError("skill "+skill.skillname+" E="+skill.exp+" "+skill.expMax+" L="+skill.CurrentLevel + "/" + skill.MaximumLevel);
            //  int level = (int)skill.CurrentLevel / perLevel;
            if (skill.expMax == 0 && skill.exp == 0)
            {
                if (m_skillName != null)
                {
#if AT_I2LOC_PRESET
                    m_skillName.text = I2.Loc.LocalizationManager.GetTranslation(skill.skillname) ;
#else
                    m_skillName.text = skill.skillname;
#endif
                }
            }
            else
            {
                if (m_skillName != null)
                {
#if AT_I2LOC_PRESET
                    m_skillName.text = I2.Loc.LocalizationManager.GetTranslation(skill.skillname) +" "+ (skill.CurrentLevel);
#else
                    m_skillName.text = skill.skillname + " " + (skill.CurrentLevel);
#endif
                }
            }

            if (skill.expMax == 0 && skill.exp == 0)
            {
                if (m_skillLevel != null)
                    m_skillLevel.text = skill.CurrentLevel + "/" + skill.MaximumLevel;
                if (m_skillFill != null)
                {
                    m_skillFill.highValue = skill.MaximumLevel;
                    m_skillFill.lowValue = 0;
                    m_skillFill.value = skill.CurrentLevel;
                }
            }
            else
            {
                if (m_skillLevel != null)
                    m_skillLevel.text = skill.exp + "/" + skill.expMax;
                if (m_skillFill != null)
                {
                    m_skillFill.highValue = skill.expMax;
                    m_skillFill.lowValue = 0;
                    m_skillFill.value = skill.exp;
                }
            }
        }
    }
}