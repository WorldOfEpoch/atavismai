using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismClaimPanel : UIAtavismWindowBase
    {
        [SerializeField] VisualTreeAsset listElementTemplate;
        [SerializeField] List<UIAtavismClaimPanelListEntry> claims = new List<UIAtavismClaimPanelListEntry>();
        // private bool showing = false;
        [SerializeField] ListView grid;


        [SerializeField] bool autoShowHide = false;

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        
        // Start is called before the first frame update
        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("CLAIM_LIST_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("CLAIM_LIST_UPDATE", this);
        }

        
          protected override bool registerUI()
        {
             if (!base.registerUI())
                return false;
            grid = uiWindow.Query<ListView>("list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = grid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            grid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismClaimPanelListEntry newListEntryLogic = new UIAtavismClaimPanelListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = listElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                claims.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            grid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismClaimPanelListEntry);
                    entry.SetData(WorldBuilder.Instance.PlayerClaims[index]);
                    StartCoroutine(entry.UpdateTimer());
            };
            
            Hide();
            return true;
        }
        
        

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CLAIM_LIST_UPDATE")
            {

                if (!showing && autoShowHide)
                    Show();

                UpdateDisplay();
            }
        }

        void UpdateDisplay()
        {
            StopAllCoroutines();
            grid.itemsSource = WorldBuilder.Instance.PlayerClaims;
            grid.Rebuild();
            grid.selectedIndex = -1;
            
           
            if (WorldBuilder.Instance.PlayerClaims.Count == 0 && autoShowHide)
            {
                Hide();

            }
        }

        public override void Show()
        {
            base.Show();
            UpdateDisplay();

        }

        public override void Hide()
        {
            base.Hide();

        }
    }
}