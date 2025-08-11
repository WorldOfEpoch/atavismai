using Atavism.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    public class UICreateCharacterSlot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UICreateCharacterSlot, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription selectClassNameAttribute = new UxmlStringAttributeDescription { defaultValue = "UICreateCharacterSlot__selected", name = "Selected-Class-Name" };
            private UxmlIntAttributeDescription iconSizePercentageAttribute = new UxmlIntAttributeDescription { defaultValue = 78, name = "Icon-Size-Percentage" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UICreateCharacterSlot c = (UICreateCharacterSlot)ve;

                c.SelectedClassName = selectClassNameAttribute.GetValueFromBag(bag, cc);
                c.IconSizePercentage = iconSizePercentageAttribute.GetValueFromBag(bag, cc);

                if (c.icon != null)
                    c.icon.RemoveFromHierarchy();

                c.icon = new VisualElement();
                c.icon.name = "item-icon";
                c.icon.style.width = new StyleLength(new Length(c.IconSizePercentage, LengthUnit.Percent));
                c.icon.style.height = new StyleLength(new Length(c.IconSizePercentage, LengthUnit.Percent));
                c.icon.style.backgroundColor = Color.white;
                c.Add(c.icon);
            }
        }

        private string SelectedClassName { get; set; }
        private int IconSizePercentage { get; set; }
        private bool isSelected;
        public bool IsSelected => isSelected;
        public Action<VisualElement> clicked;

        private VisualElement icon;

        public UICreateCharacterSlot() : base()
        {
            this.RegisterCallback<MouseUpEvent>(onClick);

            focusable = false;
            style.width = 128;
            style.height = 128;
            style.flexShrink = 0f;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
        }

        private void onClick(MouseUpEvent evt)
        {
            if (isSelected)
                Unselect();
            else Select();

            if (clicked != null)
                clicked.Invoke(this);
        }

        public void SetIcon(Sprite sprite)
        {
            icon.style.backgroundImage = new StyleBackground(sprite);
            icon.style.backgroundColor = new StyleColor();
        }

        public void Select()
        {
            if (!isSelected)
            {
                isSelected = true;
                this.AddToClassList(SelectedClassName);
            }
        }

        public void Unselect()
        {
            if (isSelected)
            {
                isSelected = false;
                this.RemoveFromClassList(SelectedClassName);
            }
        }

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