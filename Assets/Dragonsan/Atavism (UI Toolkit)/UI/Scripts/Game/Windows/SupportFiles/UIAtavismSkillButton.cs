using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismSkillButton //: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {

        Skill skill;
        UIAtavismSkillsWindow skillsWindow;
        int pos;
        public VisualElement m_icon;
        public VisualElement m_selectIcon;
        public VisualElement m_Root;

        // Use this for initialization
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);

            m_icon = visualElement.Q<VisualElement>("icon");
            m_selectIcon = visualElement.Q<VisualElement>("selected");
        }

        public void SetSkillData(Skill skill, UIAtavismSkillsWindow skillsWindow, int pos, int select)
        {
            this.skill = skill;
            this.skillsWindow = skillsWindow;
            this.pos = pos;
            m_icon.style.backgroundImage = skill.Icon.texture;
            if (m_selectIcon != null)
            {
                if(pos == select)
                    m_selectIcon.ShowVisualElement();
                else 
                    m_selectIcon.HideVisualElement();
            }
        }

        public void OnPointerClick(MouseUpEvent evt)
        {
            if (skillsWindow != null)
            {
                skillsWindow.SelectSkill(pos);
            }
        }

        public void OnPointerEnter(MouseEnterEvent evt)
        {
#if !AT_MOBILE             
            MouseEntered = true;
#endif            
        }

        public void OnPointerExit(MouseLeaveEvent evt)
        {
#if !AT_MOBILE 
            MouseEntered = false;
#endif            
        }

        void ShowTooltip()
        {
            if (skill == null)
            {
                HideTooltip();
                return;
            }
#if AT_I2LOC_PRESET
            if (skill.CurrentLevel==0)
                UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + skill.skillname) );
            else
                UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + skill.skillname) + " (" + skill.CurrentLevel + ")");
#else
            if (skill.CurrentLevel==0)
                UIAtavismTooltip.Instance.SetTitle(skill.skillname);
            else
                UIAtavismTooltip.Instance.SetTitle(skill.skillname + " (" + skill.CurrentLevel + ")");
#endif
            UIAtavismTooltip.Instance.SetIcon(skill.Icon);
            UIAtavismTooltip.Instance.SetType("");
            UIAtavismTooltip.Instance.SetWeight("");
            // UGUITooltip.Instance.HideType(true);
            // UGUITooltip.Instance.HideWeight(true);
            UIAtavismTooltip.Instance.SetDescription("");
            UIAtavismTooltip.Instance.Show(m_Root);
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        public bool MouseEntered
        {
            set
            {
                if (value)
                {
                    ShowTooltip();
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}