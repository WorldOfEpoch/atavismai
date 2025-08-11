using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIDropdown : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIDropdown, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription emptyAttribute = new UxmlStringAttributeDescription { name = "Empty", defaultValue = "Choose" };
            private UxmlStringAttributeDescription optionsAttribute = new UxmlStringAttributeDescription { name = "Popup-Options", defaultValue = "Option 1,Option 2" };
            private UxmlBoolAttributeDescription generatePopup = new UxmlBoolAttributeDescription { name = "Generate-Popup", defaultValue = true };
            readonly UxmlStringAttributeDescription classStyleAttribute = new UxmlStringAttributeDescription { name = "Class-Style", defaultValue = "UIDropdown" };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIDropdown dropdown = (UIDropdown)ve;
                dropdown.Empty = emptyAttribute.GetValueFromBag(bag, cc);
                dropdown.ClassStyle = classStyleAttribute.GetValueFromBag(bag, cc);
                dropdown.GeneratePopup = generatePopup.GetValueFromBag(bag, cc);
                dropdown.PopupOptions =optionsAttribute.GetValueFromBag(bag, cc);
                dropdown.generateLayout(dropdown.GeneratePopup);
                dropdown.Options(dropdown.PopupOptions);
                dropdown.focusable = true;
            }
        }

        public string ClassStyle { get; set; }
        public string containerUssClassName {
            get { return  ClassStyle + "__container";}
}
        public string labelUssClassName {
            get { return  ClassStyle + "__label";}
}
        public string arrowUssClassName {
            get { return  ClassStyle + "__arrow";}
}

        private string Empty { get; set; }
        private string PopupOptions { get; set; }

        private VisualElement uiContainer, uiArrow;
        private VisualElement uiScroll;
        private UIDropdownPopup uiPopupPanel;
        private Label uiSelectedLabel;
        private bool GeneratePopup { get; set; }
        private bool isPopupOpen { get { if (uiPopupPanel == null) return false; return uiPopupPanel.IsPopupOpen; } }
        public int Index
        {
            get { return uiPopupPanel.Index; }
            set { uiPopupPanel.Select(value); }
        }

        public VisualElement Screen { get; set; }

        public void Initialize(UIDropdownPopup popup)
        {
        //    Debug.LogError("UIDropdown.Initialize");
            this.uiPopupPanel = popup;
        }

        public UIDropdown() : base()
        {
            this.RegisterCallback<AttachToPanelEvent>(onAttachToPanel);
            this.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
            this.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
            RegisterCallback<FocusInEvent>(onFocusIn);
            RegisterCallback<FocusOutEvent>(onFocusOut);
        }

        private void onAttachToPanel(AttachToPanelEvent evt)
        {
        }

        private void onGeometryChanged(GeometryChangedEvent evt)
        {
        }

        public void OnMouseUpEvent(MouseUpEvent evt)
        {
            
            if (!isPopupOpen)
                ShowPopup();
            else HidePopup();
        }
        public void Focus()
        {
            //   Debug.LogError("Focus");
            base.Focus();
        }
        private void onFocusIn(FocusInEvent evt)
        {
        }

        private void onFocusOut(FocusOutEvent evt)
        {
              if(evt.relatedTarget == null || !((VisualElement)evt.relatedTarget).Equals(uiPopupPanel.uiPopupPanel))
             HidePopup();
        }
        private void generateLayout(bool popup)
        {
            if(Screen == null)
            Screen = this;
            AddToClassList(ClassStyle);
          //  Debug.LogError("UIDropdown.generateLayout");
            uiContainer = this.Q<VisualElement>("container");
            if (uiContainer != null)
                uiContainer.RemoveFromHierarchy();
                uiContainer = new VisualElement();
            uiContainer.name = "container";
            uiContainer.pickingMode = PickingMode.Position;
            uiContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            uiContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            uiContainer.style.flexDirection = FlexDirection.Row;
            uiContainer.AddToClassList(containerUssClassName);
            this.Add(uiContainer);

            uiSelectedLabel = this.Q<Label>("selected-label");
            if (uiSelectedLabel == null)
                uiSelectedLabel = new Label();
            uiSelectedLabel.name = "selected-label";
            uiSelectedLabel.style.flexGrow = 1f;
            uiSelectedLabel.style.paddingLeft = 4f;
            uiSelectedLabel.style.paddingRight = 4f;
            uiSelectedLabel.style.paddingTop = 2f;
            uiSelectedLabel.style.paddingBottom = 2f;
            uiSelectedLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            uiSelectedLabel.RemoveUnityClassStyle();
            uiSelectedLabel.AddToClassList(labelUssClassName);
            uiSelectedLabel.pickingMode = PickingMode.Position;
            uiContainer.Add(uiSelectedLabel);

            uiArrow = this.Q<VisualElement>("arrow");
            if (uiArrow == null)
                uiArrow = new VisualElement();
            uiArrow.name = "arrow";
            uiArrow.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            uiArrow.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            uiArrow.style.flexShrink = 0f;
            uiArrow.style.maxWidth = 16f;
            uiArrow.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            uiArrow.pickingMode = PickingMode.Ignore;
            uiArrow.AddToClassList(arrowUssClassName);
            uiContainer.Add(uiArrow);
            if (popup)
            {
                // VisualElement uipopup = new VisualElement();
                // uipopup.name = "popup-panel";
                // this.Add(uipopup);
                //
                // uiScroll = new ScrollView();
                // uiScroll.name = "popup-scroll";
                // uipopup.Add(uiScroll);
                
                
                uiPopupPanel = new UIDropdownPopup();
                uiPopupPanel.ussClassName = ClassStyle;
                Add(uiPopupPanel);
                uiPopupPanel.SetDropdown(this);
                uiPopupPanel.DropdownName = this.name;
                uiPopupPanel.generateLayout();
                
            }

            UpdateData();
        }
        public void Options(List<string> options)
        {
           // Debug.LogError("UIDropdown.Options "+options);
            if (uiPopupPanel != null)
            {
                uiPopupPanel.Options = string.Join(",",options);
                uiPopupPanel.generateItems();
            }
        }
        public void Options(string options)
        {
         //   Debug.LogError("UIDropdown.Options "+options);
            if (uiPopupPanel != null)
            {
                uiPopupPanel.Options = options;
                uiPopupPanel.generateItems();
            }
        }
        public void UpdateData()
        {
            string text = uiPopupPanel != null ? uiPopupPanel.GetSelectedOption() : "";
            if (string.IsNullOrEmpty(text))
                text = Empty;
            uiSelectedLabel.text = text;
        }

        public void ShowPopup()
        {
            if (uiPopupPanel != null)
                uiPopupPanel.ShowPopup(this);
            Focus();
            if (AtavismSettings.Instance != null)
            {
                AtavismSettings.Instance.ShowUIDropdown(this);
            }
        }

        public void HidePopup()
        {
            if (uiPopupPanel != null)
                uiPopupPanel.HidePopup();
        }
    }
}