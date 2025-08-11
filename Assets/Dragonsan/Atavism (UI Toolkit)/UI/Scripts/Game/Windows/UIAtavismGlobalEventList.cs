using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismGlobalEventList : UIAtavismWindowBase
        //AtList<UGUIGlobalEventEntry>
    {
        [SerializeField] VisualTreeAsset listElementTemplate;
        [SerializeField] List<UIAtavismGlobalEventEntry> events = new List<UIAtavismGlobalEventEntry>();
        private VisualElement grid;
        
        // Start is called before the first frame update
        protected override void OnEnable()
        {
            base.OnEnable();
            AtavismEventSystem.RegisterEvent("GLOABL_EVENTS_UPDATE", OnEvent);
          
            Refresh();
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            
            grid = uiWindow.Query<VisualElement>("grid");
            
            
            return true;
        }

        private void OnDisable()
        {
            AtavismEventSystem.UnregisterEvent("GLOABL_EVENTS_UPDATE", OnEvent);
        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "GLOABL_EVENTS_UPDATE")
            {

                Refresh();
            }
        }

        private void Refresh()
        {
            grid.Clear();
            events.Clear();
            foreach (var globalEvent in AtavismGlobalEvents.Instance.List)
            {
                UIAtavismGlobalEventEntry  script = new UIAtavismGlobalEventEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = listElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = script;
                // Initialize the controller script
                script.SetVisualElement(newListEntry);
                script.UpdateDisplay(globalEvent);
                grid.Add(newListEntry);
                events.Add(script);
            }
          
        }
    }
}