using Atavism.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    public class UICustomizerSliderPropertyController : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UICustomizerSliderPropertyController, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription labelClassSheetAttribute = new UxmlStringAttributeDescription() { name = "Label-Class-Sheet", defaultValue = "CustomizerProperty__displayTitle" };
            private UxmlStringAttributeDescription sliderClassSheetAttribute = new UxmlStringAttributeDescription() { name = "Slider-Class-Sheet", defaultValue = "CustomizerProperty__slider" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UICustomizerSliderPropertyController script = (UICustomizerSliderPropertyController)ve;
                script.LabelClassSheet = labelClassSheetAttribute.GetValueFromBag(bag, cc);
                script.SliderClassSheet = sliderClassSheetAttribute.GetValueFromBag(bag, cc);

                script.SetDisplayName("Slider");
                script.UpdateData();
            }
        }

        public string LabelClassSheet { get; private set; }
        public string SliderClassSheet { get; private set; }
        public string DisplayName { get; private set; }
        public string PropertyName { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public int Value { get; private set; }

        private Label uiDisplayTitle;
        private SliderInt uiSlider;
        public Action<int, string> OnValueChanged;

        public UICustomizerSliderPropertyController() : base()
        {
            focusable = false;

            style.height = 60;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
        }

        public void UpdateData()
        {
            createBaseLayout();

            if (uiDisplayTitle != null)
                uiDisplayTitle.text = DisplayName;
            if (uiSlider != null)
            {
                uiSlider.lowValue = Min;
                uiSlider.highValue = Max;
                uiSlider.SetValueWithoutNotify(Value);
            }
        }

        private void createBaseLayout()
        {
            uiDisplayTitle = this.Query<Label>("display-title");
            if (uiDisplayTitle == null)
            {
                uiDisplayTitle = new Label();
                uiDisplayTitle.name = "display-title";
                uiDisplayTitle.style.width = new StyleLength(new Length(33, LengthUnit.Percent));
                uiDisplayTitle.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                uiDisplayTitle.AddToClassList(LabelClassSheet);
                uiDisplayTitle.text = "Display title";
                Add(uiDisplayTitle);
            }

            uiSlider = this.Query<SliderInt>("item-slider");
            if (uiSlider == null)
            {
                uiSlider = new SliderInt();
                uiSlider.name = "item-slider";
                uiSlider.style.width = new StyleLength(new Length(67, LengthUnit.Percent));
                uiSlider.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                uiSlider.style.flexDirection = FlexDirection.Row;
                uiSlider.style.alignItems = Align.Center;
                uiSlider.AddToClassList(SliderClassSheet);
                // uiSlider.showInputField = true;
                uiSlider.RegisterValueChangedCallback(onSliderValueChanged);
                Add(uiSlider);
            }
        }

        public void SetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public void SetPropertyName(string propertyName)
        {
            PropertyName = propertyName;
        }

        public void SetRange(int min, int max)
        {
            this.Min = min;
            this.Max = max;
        }

        public void SetValue(int value, bool notify = true)
        {
            this.Value = value;

            if (uiSlider != null)
                uiSlider.SetValueWithoutNotify(value);

            if (notify)
                if (OnValueChanged != null)
                    OnValueChanged.Invoke(value, PropertyName);
        }

        public void SetRandomValue()
        {
            if (this.IsVisibleElement())
            {
                SetValue(UnityEngine.Random.Range(Min, Max));
            }
        }

        private void onSliderValueChanged(ChangeEvent<int> evt)
        {
            SetValue(evt.newValue);
        }
    }
}