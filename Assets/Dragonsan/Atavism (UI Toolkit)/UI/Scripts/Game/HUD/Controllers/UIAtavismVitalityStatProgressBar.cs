using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    public class UIAtavismVitalityStatProgressBar : AbstractProgressBar
    {
        public new class UxmlFactory : UxmlFactory<UIAtavismVitalityStatProgressBar, UxmlTraits> { }
        public new class UxmlTraits : AbstractProgressBar.UxmlTraits
        {
            private UxmlIntAttributeDescription valueAttribute = new UxmlIntAttributeDescription { defaultValue = 0, name = "Value" };
            private UxmlStringAttributeDescription vitalityStatNameAttribute = new UxmlStringAttributeDescription { defaultValue = "health", name = "Vitality-Stat-Name" };
            private UxmlColorAttributeDescription backgroundColorAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(0, 0, 0, 0), name = "Background-Color" };
            private UxmlColorAttributeDescription foregroundColorAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(0, 0, 0, 0), name = "Foreground-Color" };

            private UxmlStringAttributeDescription containerClassAttribute = new UxmlStringAttributeDescription { defaultValue = "VitalityStatSlider-container", name = "Container-Class" };
            private UxmlStringAttributeDescription backgroundClassAttribute = new UxmlStringAttributeDescription { defaultValue = "VitalityStatSlider-background", name = "Background-Class" };
            private UxmlStringAttributeDescription progressBarClassAttribute = new UxmlStringAttributeDescription { defaultValue = "VitalityStatSlider-progressBar", name = "Progress-Bar-Class" };
            private UxmlStringAttributeDescription titleContainerClassAttribute = new UxmlStringAttributeDescription { defaultValue = "VitalityStatSlider-title-container", name = "Title-Container-Class" };
            private UxmlStringAttributeDescription titleClassAttribute = new UxmlStringAttributeDescription { defaultValue = "VitalityStatSlider-title", name = "Title-Class" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIAtavismVitalityStatProgressBar script = (UIAtavismVitalityStatProgressBar)ve;

                script.value = valueAttribute.GetValueFromBag(bag, cc);
                script.vitalityStatName = vitalityStatNameAttribute.GetValueFromBag(bag, cc);
                script.backgroundColor = backgroundColorAttribute.GetValueFromBag(bag, cc);
                script.foregroundColor = foregroundColorAttribute.GetValueFromBag(bag, cc);

                script.containerClass = containerClassAttribute.GetValueFromBag(bag, cc);
                script.backgroundClass = backgroundClassAttribute.GetValueFromBag(bag, cc);
                script.progressBarClass = progressBarClassAttribute.GetValueFromBag(bag, cc);
                script.titleContainerClass = titleContainerClassAttribute.GetValueFromBag(bag, cc);
                script.titleClass = titleClassAttribute.GetValueFromBag(bag, cc);

                script.uiContainer = ve.ElementAt(0);
                script.uiBackground = script.uiContainer.ElementAt(0);
                script.uiProgressBar = script.uiBackground.ElementAt(0);
                script.uiTitleContainer = script.uiBackground.ElementAt(1);
                script.uiTitle = (Label)script.uiTitleContainer.ElementAt(0);

                if (script.uiContainer != null)
                {
                    script.uiContainer.RemoveFromClassList(containerUssClassName);
                    script.uiContainer.AddToClassList(script.containerClass);
                }
                if (script.uiBackground != null)
                    script.uiBackground.AddToClassList(script.backgroundClass);
                if (script.uiProgressBar != null)
                    script.uiProgressBar.AddToClassList(script.progressBarClass);
                if (script.uiTitleContainer != null)
                {
                    script.uiTitleContainer.RemoveFromClassList(titleContainerUssClassName);
                    script.uiTitleContainer.AddToClassList(script.titleContainerClass);
                }
                if (script.uiTitle != null)
                {
                    script.uiTitle.RemoveFromClassList(titleUssClassName);
                    script.uiTitle.AddToClassList(script.titleClass);
                }

                script.UpdateColors();
            }
        }

        private string vitalityStatName { get; set; }
        private Color backgroundColor { get; set; }
        private Color foregroundColor { get; set; }
        private string containerClass { get; set; }
        private string backgroundClass { get; set; }
        private string progressBarClass { get; set; }
        private string titleContainerClass { get; set; }
        private string titleClass { get; set; }

        private VisualElement uiContainer, uiBackground, uiProgressBar, uiTitleContainer;
        private Label uiTitle;
        public override float value { get => base.value; 
            set 
            {
                base.value = value;

                if (uiTitle != null)
                    uiTitle.text = value.ToString() + "/" + highValue.ToString();
                UpdateColors();
            }
        }

        public UIAtavismVitalityStatProgressBar() : base()
        {
            UpdateColors();
        }

        public void UpdateColors()
        {
            if (uiBackground != null)
                if (backgroundColor.a > 0f)
                    uiBackground.style.unityBackgroundImageTintColor = backgroundColor;
            if (uiProgressBar != null)
                if (foregroundColor.a > 0f)
                    uiProgressBar.style.unityBackgroundImageTintColor = foregroundColor;
            if (uiTitle != null)
                uiTitle.text = value.ToString() + "/" + highValue.ToString();

            /*if (uiContainer != null)
            {
                uiContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                uiContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            }

            if (uiBackground != null)
            {
                uiBackground.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                uiBackground.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            }*/
        }
    }
}