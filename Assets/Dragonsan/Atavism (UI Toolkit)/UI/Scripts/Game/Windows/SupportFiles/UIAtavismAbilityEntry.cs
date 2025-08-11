using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismAbilityEntry 
    {
        public int slotNum;
        public Label m_Name;
        public Label m_Description;
        public Label m_LevelReq;
        public UIAtavismAbilitySlot abilitySlot;
        AtavismAbility ability;
        bool mouseEntered = false;

        private VisualElement m_Root;
        private VisualElement m_Slot;
        // Use this for initialization
     
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            // m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);
            
            m_Slot = visualElement.Q<VisualElement>("slot");
            abilitySlot = new UIAtavismAbilitySlot();
            abilitySlot.SetVisualElement(m_Slot);
            
            m_Name = visualElement.Q<Label>("name");
            m_Description = visualElement.Q<Label>("description");
            m_LevelReq = visualElement.Q<Label>("level");
        }


        public void Update()
        {
            if(abilitySlot!=null)
                abilitySlot.Update();
        }
        
        /*public void OnPointerClick(PointerEventData data)
        {
            switch(data.button)
            {
            case PointerEventData.InputButton.Left:
                PickUpOrPlaceItem();
                break;
            case PointerEventData.InputButton.Right:
                Activate();
                break;
            case PointerEventData.InputButton.Middle:
                break;
            }
        }*/

        public void UpdateAbilityData(AtavismAbility ability)
        {
            this.ability = ability;
            abilitySlot.UpdateAbilityData(ability);
            if (ability == null)
            {
                if (m_Name != null)
                    m_Name.text = "";
                if (m_Description != null)
                    m_Description.text = "";
                return;
            }
#if AT_I2LOC_PRESET
      if(m_Description!=null) m_Description.text = I2.Loc.LocalizationManager.GetTranslation("Ability/" + ability.tooltip);
      if(m_Name!=null)   m_Name.text = I2.Loc.LocalizationManager.GetTranslation("Ability/" + ability.name);
#else
            if (m_Name != null)
                m_Name.text = ability.name;
            if (m_Description != null)
                m_Description.text = ability.tooltip;

#endif
            if (m_LevelReq != null)
            {
                m_LevelReq.text = "";
                Skill skill = Skills.Instance.GetSkillOfAbility(ability.id);
                if (skill != null)
                {
                    for (int i = 0; i < skill.abilities.Count; i++)
                    {
                        if (skill.abilities[i] == ability.id)
                        {
                            m_LevelReq.text = skill.abilityLevelReqs[i].ToString();
                            break;
                        }
                    }
                }
            }
            // If the player doesn't know this ability, disable this panel
            if (!Abilities.Instance.PlayerAbilities.Contains(ability))
            {
                m_Root.AddToClassList("ability-disabled");
            }
            else
            {
                m_Root.RemoveFromClassList("ability-disabled");
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
        public bool MouseEntered
        {
            get
            {
                return mouseEntered;
            }
            set
            {
                mouseEntered = value;
                if (mouseEntered && ability != null)
                {
                    ability.ShowUITooltip(m_Root);
                }
                else
                {
                    HideTooltip();
                }
            }
        }
        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

    }
}