using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismMobCreatorPathEntry 
    {
        private Label label;
        private Button button;
        PatrolPoint pp;
        UIAtavismMobCreator mobCreator;
     
        public void SetVisualElement(VisualElement visualElement)
        {
            // uiRoot = visualElement;
            label = visualElement.Q<Label>("label");
            button = visualElement.Q<Button>("button");
            button.clicked += EntryClicked;
        }
        
        public void SetEntryDetails(string name, PatrolPoint pp, UIAtavismMobCreator mobCreator)
        {
            this.label.text = name;
            this.mobCreator = mobCreator;
            this.pp = pp;

        }
        public void EntryClicked()
        {
            mobCreator.PatrolPathDeletePointClicked(pp);
        }
    }
}