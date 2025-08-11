using Atavism.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    public class UICharacterSelectionSlot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UICharacterSelectionSlot, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription selectedClassNameAttribute = new UxmlStringAttributeDescription { defaultValue = "UICharacterSelectionSlot__selected", name = "Selected-Class-Name" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UICharacterSelectionSlot script = (UICharacterSelectionSlot)ve;

                script.SelectedClassName = selectedClassNameAttribute.GetValueFromBag(bag, cc);
            }
        }

        private string SelectedClassName { get; set; }
        private bool isSelected;
        public bool IsSelected => isSelected;

        private VisualElement characterIcon;
        private Label characterName;
        private Label characterClass;
        private Label characterRace;
        private Label characterLevel;

        public Action<VisualElement> clicked;

        public UICharacterSelectionSlot() : base()
        {
            style.flexDirection = FlexDirection.Row;
            style.height = 100;
            style.marginBottom = 4;
            style.flexShrink = 0f;

            this.RegisterCallback<MouseUpEvent>(onClick);
            this.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void onClick(MouseUpEvent evt)
        {
            if (isSelected)
                Unselect();
            else Select();

            if (clicked != null)
                clicked.Invoke(this);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            characterIcon = this.Query<VisualElement>("character-icon");
            characterName = this.Query<Label>("character-name");
            characterRace = this.Query<Label>("character-race");
            characterClass = this.Query<Label>("character-class");
            characterLevel = this.Query<Label>("character-level");
        }

        public void Select()
        {
            isSelected = true;
            this.AddToClassList(SelectedClassName);
        }

        public void Unselect()
        {
            isSelected = false;
            this.RemoveFromClassList(SelectedClassName);
        }

        public void SetCharacterIcon(Sprite icon) => this.characterIcon.style.backgroundImage = new StyleBackground(icon);
        public void SetCharacterName(string characterName) => this.characterName.text = characterName;
        public void SetCharacterRace(string raceName) => this.characterRace.text = raceName;
        public void SetCharacterClass(string className) => this.characterClass.text = className;
        public void SetCharacterLevel(string level) => this.characterLevel.text = level;

        public void Show()
        {
            style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
    }
}