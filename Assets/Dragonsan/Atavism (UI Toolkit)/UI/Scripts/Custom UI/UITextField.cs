using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UITextField : TextField
    {
        public new class UxmlFactory : UxmlFactory<UITextField, UxmlTraits> { }
        public new class UxmlTraits : TextField.UxmlTraits
        {
            readonly UxmlStringAttributeDescription placeholderAttribute = new UxmlStringAttributeDescription { name = "Placeholder", defaultValue = "" };
            readonly UxmlStringAttributeDescription placeholderClassStyleAttribute = new UxmlStringAttributeDescription { name = "Placeholder-Class-Style", defaultValue = "UITextField__placeholder" };
            readonly UxmlStringAttributeDescription labelClassStyleAttribute = new UxmlStringAttributeDescription { name = "Label-Class-Style", defaultValue = "" };
            readonly UxmlBoolAttributeDescription removeUnityClassStylesAttribute = new UxmlBoolAttributeDescription { name = "Remove-Unity-Class-Styles", defaultValue = false };
            readonly UxmlBoolAttributeDescription onlyNumbersAttribute = new UxmlBoolAttributeDescription { name = "Only-Numbers", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UITextField script = (UITextField)ve;

                script.placeholder = placeholderAttribute.GetValueFromBag(bag, cc);
                script.placeholderClassStyle = placeholderClassStyleAttribute.GetValueFromBag(bag, cc);
                script.labelClassStyle = labelClassStyleAttribute.GetValueFromBag(bag, cc);
                script.removeUnityClassStyles = removeUnityClassStylesAttribute.GetValueFromBag(bag, cc);
                script.onlyNumbers = onlyNumbersAttribute.GetValueFromBag(bag, cc);

                if (script.uiPlaceholderLabel != null)
                    script.uiPlaceholderLabel.RemoveFromHierarchy();

                if (!string.IsNullOrEmpty(script.placeholder))
                {
                    script.uiPlaceholderLabel = new Label();
                    script.uiPlaceholderLabel.name = "placeholder-label";
                    script.uiPlaceholderLabel.text = script.placeholder;
                    script.uiPlaceholderLabel.pickingMode = PickingMode.Ignore;
                    script.uiPlaceholderLabel.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    script.uiPlaceholderLabel.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    script.uiPlaceholderLabel.AddToClassList(script.placeholderClassStyle);
                    script.uiPlaceholderLabel.RemoveUnityClassStyle();

                    script.uiTextInput = ve.Q<TextInputBase>("unity-text-input");
                    script.uiTextInput.Add(script.uiPlaceholderLabel);
                    script.uiTextInput.RemoveUnityClassStyle();
                    script.uiTextInput.AddToClassList(script.labelClassStyle);
                    script.updateData_placeholder();
                }
                if(script.multiline)
                    script.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
                if(script.removeUnityClassStyles)
                {
                    script.uiTextInput.RemoveFromClassList("unity-base-text-field__input--single-line");
                }
            }
        }

        private TextInputBase uiTextInput;
        public VisualElement TextInputLabel => uiTextInput;
        private Label uiPlaceholderLabel;
        private bool password { get { return isPasswordField; } set { isPasswordField = value; } } // fix Unity bug
        private string placeholder { get; set; }
        private string placeholderClassStyle { get; set; }
        private string labelClassStyle { get; set; }
        private bool removeUnityClassStyles { get; set; }
        private bool isFocused;
        public bool IsFocused => isFocused;
        public bool onlyNumbers { get; set; }
        public override string value
        {
            get => base.value;
            set
            {
                if (onlyNumbers)
                {
                    int numericvalue;
                    bool isNumber = int.TryParse(value, out numericvalue);
                    float floatvalue;
                    bool isFloat = float.TryParse(value, out floatvalue);
                    if (isNumber || isFloat || String.IsNullOrEmpty(value))
                    {
                        base.value = value;
                        
                    }
                    else
                    {
                        base.value = base.value;
                    }
                }
                else
                {

                    base.value = value;
                }
            }
        }

        public UITextField() : base()
        {
            RegisterCallback<FocusInEvent>(onFocusIn);
            RegisterCallback<FocusOutEvent>(onFocusOut);
            RegisterCallback<AttachToPanelEvent>(onAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
        }

        private void OnLeavePanel(DetachFromPanelEvent evt)
        {
          if(isFocused)
            ClientAPI.Instance.UiElementFocused = false;
        }

        private void onAttachToPanel(AttachToPanelEvent evt)
        {
            updateData_placeholder();
        }

        public void Focus()
        {
         //   Debug.LogError("Focus");
            base.Focus();
        }
        
        private void onFocusIn(FocusInEvent evt)
        {
         //   Debug.LogError("Focus in "+name+ " "+(VisualElement)evt.currentTarget+" "+(VisualElement)evt.target+" "+evt.relatedTarget);
            isFocused = true;
            ClientAPI.Instance.UiElementFocused = true;
            updateData_placeholder();

            if (string.IsNullOrEmpty(value))
                SelectAll();
        }

        private void onFocusOut(FocusOutEvent evt)
        {
           // Debug.LogError("Focus out "+name);
            isFocused = false;
            ClientAPI.Instance.UiElementFocused = false;
            updateData_placeholder();
        }

        public void FocusInput()
        {
            uiTextInput.Focus();
        }

        private void updateData_placeholder()
        {
            if (uiPlaceholderLabel != null)
            {
                if (isFocused || !string.IsNullOrEmpty(uiTextInput.text))
                {
                    uiPlaceholderLabel.HideVisualElement();
                }
                else
                {
                    uiPlaceholderLabel.ShowVisualElement();
                }
            }
        }

        public void SetPlaceholder(string placeholder)
        {
            this.placeholder = placeholder;
            uiPlaceholderLabel.text = placeholder;

            updateData_placeholder();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);

            updateData_placeholder();
        }
    }
}