using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismKeySettingsEntry
    {
        public string name = "";
        public Label label;
        public Button button;
        public Button altButton;
        public AtavismKeyDefinition def;
        private VisualElement m_Root;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            label = visualElement.Q<Label>("label");
            button = visualElement.Q<Button>("button");
            altButton = visualElement.Q<Button>("alt-button");
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }
        public void Hide()
        {
            m_Root.HideVisualElement();
        }
    }
}