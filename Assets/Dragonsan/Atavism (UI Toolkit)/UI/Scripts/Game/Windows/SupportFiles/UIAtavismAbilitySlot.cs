using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismAbilitySlot : UIAtavismDraggableSlot // Assuming UIAtavismDraggableSlot is the UIToolkit version of UGUIDraggableSlot
    {
        AtavismAbility ability;
        bool mouseEntered = false;
        public VisualElement m_icon; // Texture2D replaces Image
        public VisualElement m_Root;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            // m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);

            m_icon = visualElement.Q<VisualElement>("icon");
            // m_selectIcon = visualElement.Q<VisualElement>("selected");
            slotBehaviour = DraggableBehaviour.SourceOnly;
        }

        
        public override void ClearChildSlot()
        {
            uiActivatable = null;
        }

        public void UpdateAbilityData(AtavismAbility ability)
        {
            this.ability = ability;
            if (ability == null)
            {
                if (uiActivatable != null)
                {
           //         uiActivatable.RemoveFromHierarchy();
                }
            }
            else if (Abilities.Instance.PlayerAbilities.Contains(ability))
            {
                if (uiActivatable == null)
                {
                    uiActivatable = new UIAtavismActivatable(m_Root);
                    uiActivatable.m_Root.AddToClassList("activatableContainer");
                    m_Root.Add(uiActivatable.m_Root);
                }

                uiActivatable.SetActivatable(ability, ActivatableType.Ability, this);

                // Set background Image
                if (m_icon != null)
                {
                    m_icon.SetBackgroundImage(ability.Icon);
                }
            }
            else
            {
                if (uiActivatable != null)
                {
                    uiActivatable.m_Root.RemoveFromHierarchy();
                    uiActivatable = null;
                }

                if (m_icon != null)
                {
                    m_icon.SetBackgroundImage(ability.Icon);
                }
            }
        }

        public void Update()
        {
            if (uiActivatable != null)
                uiActivatable.update();

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
        
        public override void Activate()
        {
          
            if (ability == null) return;
            ability.Activate();
        }

        
        public override void OnDrop(DropEvent eventData)
        {
            Debug.Log("On Drop");
            // Do nothing
        }

        
        protected override void ShowTooltip()
        {
            // Implement UIToolkit tooltip logic here
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
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
                if (mouseEntered && uiActivatable != null)
                {
                  uiActivatable.ShowTooltip(this); // Assuming UIAtavismActivatable has a ShowTooltip method compatible with UIToolkit
                    // Coroutine replacement might be needed here
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}
