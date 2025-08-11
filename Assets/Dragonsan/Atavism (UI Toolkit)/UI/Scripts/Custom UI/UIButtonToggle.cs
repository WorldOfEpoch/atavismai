using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIButtonToggle : Button
    {
        public new class UxmlFactory : UxmlFactory<UIButtonToggle, UxmlTraits> { }
        public new class UxmlTraits : Button.UxmlTraits
        {
            private UxmlBoolAttributeDescription isOnAttribute = new UxmlBoolAttributeDescription { defaultValue = false, name = "Is-On" };
            private UxmlColorAttributeDescription backgroundColorNormalAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(0, 0, 0, 0), name = "Background-Color-Normal" };
            private UxmlColorAttributeDescription textColorNormalAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(158/255f, 154/255f, 147/255f, 1), name = "Text-Color-Normal" };
            private UxmlColorAttributeDescription backgroundColorCheckedAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(0, 0, 0, 0), name = "Background-Color-Checked" };
            private UxmlColorAttributeDescription textColorCheckedAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(223 / 255f, 205 / 255f, 177 / 255f, 1), name = "Text-Color-Checked" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                ((UIButtonToggle)ve).IsOn = isOnAttribute.GetValueFromBag(bag, cc);
                ((UIButtonToggle)ve).BackgroundColorNormal = backgroundColorNormalAttribute.GetValueFromBag(bag, cc);
                ((UIButtonToggle)ve).TextColorNormal = textColorNormalAttribute.GetValueFromBag(bag, cc);
                ((UIButtonToggle)ve).BackgroundColorChecked = backgroundColorCheckedAttribute.GetValueFromBag(bag, cc);
                ((UIButtonToggle)ve).TextColorChecked = textColorCheckedAttribute.GetValueFromBag(bag, cc);

                ((UIButtonToggle)ve).updateColors();
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        public Color TextColorNormal { get; set; }
        public Color TextColorChecked { get; set; }
        public Color BackgroundColorNormal { get; set; }
        public Color BackgroundColorChecked { get; set; }

        private bool isOn;
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                isOn = value;

                updateColors();
            }
        }

        public UIButtonToggle()
        {
            this.RegisterCallback<GeometryChangedEvent>(onGeometryChange);

            this.clicked += onValueChanged;

            IsOn = false;
            focusable = false;
        }

        private void onValueChanged()
        {
            IsOn = !isOn;
        }

        private void updateColors()
        {
            if (isOn)
            {
                if (TextColorChecked.a > 0)
                    style.color = TextColorChecked;
                if (BackgroundColorChecked.a > 0)
                    style.backgroundColor = BackgroundColorChecked;
            }
            else
            {
                if (TextColorNormal.a > 0)
                    style.color = TextColorNormal;
                if (BackgroundColorNormal.a > 0)
                    style.backgroundColor = BackgroundColorNormal;
            }
        }

        private void onGeometryChange(GeometryChangedEvent evt)
        {
       
        }
    }
}