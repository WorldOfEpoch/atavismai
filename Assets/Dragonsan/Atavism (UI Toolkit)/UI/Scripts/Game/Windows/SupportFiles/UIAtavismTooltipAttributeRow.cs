using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismTooltipAttributeRow 
    {

        public Label LeftText;
        public VisualElement LeftCompare1;
        public VisualElement LeftCompare2;
        public Label LeftTextValue;
        public Label RightText;
        public VisualElement RightCompare1;
        public VisualElement RightCompare2;
        public Label RightTextValue;
        public void SetVisualElement(VisualElement visualElement)
        {
            LeftText = visualElement.Q<Label>("left-attribute");
            LeftCompare1 = visualElement.Q<VisualElement>("left-compare");
            LeftCompare2 = visualElement.Q<VisualElement>("left-compare2");
            LeftTextValue = visualElement.Q<Label>("left-attribute-value");
            RightText = visualElement.Q<Label>("right-attribute");
            RightCompare1 = visualElement.Q<VisualElement>("right-compare");
            RightCompare2 = visualElement.Q<VisualElement>("right-compare2");
            RightTextValue = visualElement.Q<Label>("right-attribute-value");
        }
    }
}