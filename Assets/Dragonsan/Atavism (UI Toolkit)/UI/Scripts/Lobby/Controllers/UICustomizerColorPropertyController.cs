using Atavism.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    public class UICustomizerColorPropertyController : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UICustomizerColorPropertyController, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription labelClassSheetAttribute = new UxmlStringAttributeDescription() { name = "Label-Class-Sheet", defaultValue = "CustomizerProperty__displayTitle" };
            private UxmlStringAttributeDescription colorsStringAttribute = new UxmlStringAttributeDescription() { name = "Colors-String", defaultValue = "FFFFFF,B0B0B0,3C3C3C,1F0303,EED48C,EE8E2A,E06018,E5150A,F75DFF,B2098E,1EAFD9,003DFF,49D996,60B72F" };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UICustomizerColorPropertyController script = (UICustomizerColorPropertyController)ve;
                script.LabelClassSheet = labelClassSheetAttribute.GetValueFromBag(bag, cc);
                script.ColorsString = colorsStringAttribute.GetValueFromBag(bag, cc);

                script.SetDisplayName("Color");
                script.UpdateData();
            }
        }

        public string LabelClassSheet { get; private set; }
        public string ColorsString { get; private set; }
        public string DisplayName { get; private set; }
        public string PropertyName { get; private set; }

        private Color selectedColor;
        public Color SelectedColor { get { return selectedColor; } set { selectedColor = value; } }

        private Label uiDisplayTitle;
        private VisualElement uiSelectedColorElement, uiContainer, uiColorsContainer;
        private List<VisualElement> listofColorElements;

        public Action<Color, string> OnSelectedColorChanged;

        public UICustomizerColorPropertyController(): base()
        {
            focusable = false;

            style.height = 60;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            this.RegisterCallback<MouseUpEvent>(onButtonMouseUp);
        }

        private void onButtonMouseUp(MouseUpEvent evt)
        {
            VisualElement e = (VisualElement)evt.target;
            
            if (e == this || !e.name.Contains("color-item"))
                return;

            SetColor(e.resolvedStyle.backgroundColor);
        }

        public void SetColor(Color color, bool notify = true)
        {
            selectedColor = color;
            if (uiSelectedColorElement != null)
                uiSelectedColorElement.style.backgroundColor = selectedColor;

            if (notify)
                if (OnSelectedColorChanged != null)
                    OnSelectedColorChanged.Invoke(selectedColor, PropertyName);
        }

        public void SetRandomColor()
        {
            if (listofColorElements != null)
            {
                int index = UnityEngine.Random.Range(0, listofColorElements.Count);
                SetColor(listofColorElements[index].resolvedStyle.backgroundColor);
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

            uiContainer = this.Query<VisualElement>("container");
            if (uiContainer == null)
            {
                uiContainer = new VisualElement();
                uiContainer.name = "container";
                uiContainer.style.width = new StyleLength(new Length(67, LengthUnit.Percent));
                uiContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                uiContainer.style.flexDirection = FlexDirection.Row;
                uiContainer.style.alignItems = Align.Center;
                Add(uiContainer);
            }

            uiSelectedColorElement = this.Query<VisualElement>("selected-color");
            if (uiSelectedColorElement == null)
            {
                uiSelectedColorElement = new VisualElement();
                uiSelectedColorElement.name = "selected-color";
                uiSelectedColorElement.style.width = 64;
                uiSelectedColorElement.style.height = 32;
                uiSelectedColorElement.style.marginRight = 20;
                uiSelectedColorElement.style.backgroundColor = Color.white;
                uiSelectedColorElement.style.flexShrink = 0f;
                uiContainer.Add(uiSelectedColorElement);
            }

            uiColorsContainer = this.Query<VisualElement>("container-colors");
            if(uiColorsContainer == null)
            {
                uiColorsContainer = new VisualElement();
                uiColorsContainer.name = "container-colors";
                uiColorsContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                uiColorsContainer.style.height = 32;
                uiColorsContainer.style.flexDirection = FlexDirection.Row;
                uiColorsContainer.style.flexShrink = 1f;
                uiColorsContainer.style.alignItems = Align.Center;
                uiContainer.Add(uiColorsContainer);
            }
        }

        private void createColorElements()
        {
            if (listofColorElements == null)
                listofColorElements = new List<VisualElement>();
            else
            {
                for (int n = 0; n < listofColorElements.Count; n++)
                    listofColorElements[n].RemoveFromHierarchy();
                listofColorElements.Clear();
            }

            string[] colorsString = ColorsString.Split(',');
            if (colorsString != null && colorsString.Length > 0)
            {
                Color[] colors = new Color[colorsString.Length];

                for (int n = 0; n < colorsString.Length; n++)
                {
                    ColorUtility.TryParseHtmlString("#" + colorsString[n], out colors[n]);

                    VisualElement e = new VisualElement();
                    e.name = "color-item-" + n.ToString();
                    e.style.minWidth = 16;
                    e.style.minHeight = 16;
                    e.style.backgroundColor = colors[n];

                    listofColorElements.Add(e);
                    uiColorsContainer.Add(e);
                }
            }
        }

        public void UpdateData()
        {
            createBaseLayout();
            createColorElements();

            if (uiDisplayTitle != null)
                uiDisplayTitle.text = DisplayName;
        }

        public void SetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public void SetPropertyName(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Hex color, separated by comma
        /// </summary>
        /// <param name="colors"></param>
        public void SetColorsList(Color[] colors)
        {
            string colorsString = "";

            if (colors != null)
            {
                for (int n = 0; n < colors.Length; n++)
                {
                    if (n == 0)
                        colorsString = ColorUtility.ToHtmlStringRGB(colors[n]);
                    else colorsString += "," + ColorUtility.ToHtmlStringRGB(colors[n]);
                }
            }

            ColorsString = colorsString;
        }

        public void SetColorsList(Color32[] colors)
        {
            string colorsString = "";

            if (colors != null)
            {
                for (int n = 0; n < colors.Length; n++)
                {
                    if (n == 0)
                        colorsString = ColorUtility.ToHtmlStringRGB(colors[n]);
                    else colorsString += "," + ColorUtility.ToHtmlStringRGB(colors[n]);
                }
            }

            ColorsString = colorsString;
        }


        public virtual Color[] GetDefaultColorList()
        {
            Color[] colors = new Color[14];

            ColorUtility.TryParseHtmlString("#FFFFFF", out colors[0]);
            ColorUtility.TryParseHtmlString("#B0B0B0", out colors[1]);
            ColorUtility.TryParseHtmlString("#3C3C3C", out colors[2]);
            ColorUtility.TryParseHtmlString("#1F0303", out colors[3]);
            ColorUtility.TryParseHtmlString("#EED48C", out colors[4]);
            ColorUtility.TryParseHtmlString("#EE8E2A", out colors[5]);
            ColorUtility.TryParseHtmlString("#E06018", out colors[6]);
            ColorUtility.TryParseHtmlString("#E5150A", out colors[7]);
            ColorUtility.TryParseHtmlString("#F75DFF", out colors[8]);
            ColorUtility.TryParseHtmlString("#B2098E", out colors[9]);
            ColorUtility.TryParseHtmlString("#1EAFD9", out colors[10]);
            ColorUtility.TryParseHtmlString("#003DFF", out colors[11]);
            ColorUtility.TryParseHtmlString("#49D996", out colors[12]);
            ColorUtility.TryParseHtmlString("#60B72F", out colors[13]);

            return colors;
        }
    }
}