using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismTooltipResourceRow 
    {

        public VisualElement LeftIcon;
        public Label LeftText;
        public Label LeftTextValue;
        public VisualElement RightIcon;
        public Label RightText;
        public Label RightTextValue;
        public void SetVisualElement(VisualElement visualElement )
        {
            // m_container = visualElement;
            LeftIcon = visualElement.Q<VisualElement>("left-icon");
            LeftText = visualElement.Q<Label>("left-attribute");
            LeftTextValue = visualElement.Q<Label>("left-attribute-value");
            RightIcon = visualElement.Q<VisualElement>("left-icon");
            RightText = visualElement.Q<Label>("right-attribute");
            RightTextValue = visualElement.Q<Label>("right-attribute-value");
        }
    }
}