using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismTooltipSocketRow 
    {

        public VisualElement LeftIcon;
        public Label LeftText;
        public VisualElement RightIcon;
        public Label RightText;
        public void SetVisualElement(VisualElement visualElement)
        {
            // m_container = visualElement;
            LeftText = visualElement.Q<Label>("left-attribute");
            LeftIcon = visualElement.Q<VisualElement>("left-icon");
            RightText = visualElement.Q<Label>("right-attribute");
            RightIcon = visualElement.Q<VisualElement>("right-icon");
        }
    }
}