using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIProgressBar : ProgressBar
    {
        public new class UxmlFactory : UxmlFactory<UIProgressBar, UxmlTraits>
        {
        }

        public new class UxmlTraits : ProgressBar.UxmlTraits
        {
            private UxmlIntAttributeDescription valueAttribute = new UxmlIntAttributeDescription
                { defaultValue = 0, name = "Value" };

            private UxmlBoolAttributeDescription showLabelClassAttribute = new UxmlBoolAttributeDescription
                { defaultValue = true, name = "Show-Label" };

            private UxmlStringAttributeDescription containerClassAttribute = new UxmlStringAttributeDescription
                { defaultValue = "UIProgressBar__container", name = "Container-Class" };

            private UxmlStringAttributeDescription backgroundClassAttribute = new UxmlStringAttributeDescription
                { defaultValue = "UIProgressBar__background", name = "Background-Class" };

            private UxmlStringAttributeDescription progressBarClassAttribute = new UxmlStringAttributeDescription
                { defaultValue = "UIProgressBar__progressBar", name = "Progress-Bar-Class" };

            private UxmlStringAttributeDescription titleContainerClassAttribute = new UxmlStringAttributeDescription
                { defaultValue = "UIProgressBar-title__container", name = "Title-Container-Class" };

            private UxmlStringAttributeDescription titleClassAttribute = new UxmlStringAttributeDescription
                { defaultValue = "UIProgressBar__title", name = "Title-Class" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIProgressBar script = (UIProgressBar)ve;

                script.value = valueAttribute.GetValueFromBag(bag, cc);
                script.showLabel = showLabelClassAttribute.GetValueFromBag(bag, cc);
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

                script.uiContainer.AddToClassList(script.containerClass);
                script.uiBackground.AddToClassList(script.backgroundClass);
                script.uiProgressBar.AddToClassList(script.progressBarClass);
                script.uiTitleContainer.AddToClassList(script.titleContainerClass);
                script.uiTitle.AddToClassList(script.titleClass);

                script.RemoveUnityClassStyle();
                if (!script.showLabel)
                    script.uiTitle.visible = false;
                else
                    script.uiTitle.visible = true;
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        private bool showLabel { get; set; }
        private string containerClass { get; set; }
        private string backgroundClass { get; set; }
        private string progressBarClass { get; set; }
        private string titleContainerClass { get; set; }
        private string titleClass { get; set; }
        private VisualElement uiContainer, uiBackground, uiProgressBar, uiTitleContainer;
        private Label uiTitle;

        public override float value
        {
            get => base.value;
            set
            {
                base.value = value;

                if (showLabel)
                {
                    if (uiTitle != null)
                    {
                        uiTitle.text = value.ToString() + "/" + highValue.ToString();
                        uiTitle.visible = true;
                    }
                }
                else
                {
                    if (uiTitle != null)
                        uiTitle.visible = false;
                }
            }
        }

        public UIProgressBar()
        {

            RegisterCallback<PointerEnterEvent>((e) =>
            {
                if (tooltip.Length > 0)
                {
                    if (UIAtavismMiniTooltip.Instance != null)
                    {
                        UIAtavismMiniTooltip.Instance.SetDescription(tooltip);
                        UIAtavismMiniTooltip.Instance.Show(this);
                    }
                }
            });
            RegisterCallback<PointerLeaveEvent>((e) => { if(UIAtavismMiniTooltip.Instance!=null) UIAtavismMiniTooltip.Instance.Hide(); });
        }
    }
}