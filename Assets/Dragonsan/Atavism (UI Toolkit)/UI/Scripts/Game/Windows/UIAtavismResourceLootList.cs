using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismResourceLootList : UIAtavismWindowBase //AtList<UGUIResourceLootListEntry>
    {

        [SerializeField] VisualTreeAsset listElementTemplate;
        private ListView grid;
        private Button lootAllButton;


        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
           // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("ResourceLootWindow");
            grid = uiWindow.Query<ListView>("list");
            if (grid != null)
            {
#if UNITY_6000_0_OR_NEWER                    
                ScrollView scrollView = grid.Q<ScrollView>();
                scrollView.mouseWheelScrollSize = 19;
#endif
                grid.makeItem = () =>
                {
                    // Instantiate a controller for the data
                    UIAtavismResourceLootListEntry newListEntryLogic = new UIAtavismResourceLootListEntry();
                    // Instantiate the UXML template for the entry
                    var newListEntry = listElementTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = newListEntryLogic;
                    // Initialize the controller script
                    newListEntryLogic.SetVisualElement(newListEntry);
                    // factions.Add(newListEntryLogic);
                    // Return the root of the instantiated visual tree
                    return newListEntry;
                };
                grid.bindItem = (item, index) =>
                {
                    var entry = (item.userData as UIAtavismResourceLootListEntry);
                    entry.SetResourceLootEntryDetails(Crafting.Instance.ResourceLoot[index]);
                };
            }

            // lootAllButton = uiWindow.Query<Button>("loot-all-button");
            // if (lootAllButton != null)
            //     lootAllButton.clicked += LootAll;
            return true;
        }

        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            isRegisteredUI = false;

            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("RESOURCE_LOOT_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CLOSE_RESOURCE_LOOT_WINDOW", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
        }


        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("RESOURCE_LOOT_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_RESOURCE_LOOT_WINDOW", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
        }

        public override void Show()
        {
            base.Show();
            // AtavismSettings.Instance.OpenWindow(this);
            Refresh();

        }

        private void Refresh()
        {
            grid.itemsSource = Crafting.Instance.ResourceLoot;
            grid.Rebuild();
            grid.selectedIndex = -1;
        }

        public override void Hide()
        {
            base.Hide();
          //  AtavismSettings.Instance.CloseWindow(this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "RESOURCE_LOOT_UPDATE")
            {
                if (Crafting.Instance.ResourceLoot.Count > 0)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
            else if (eData.eventType == "CLOSE_RESOURCE_LOOT_WINDOW")
            {
                Hide();
            }
            else if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (showing)
                {
                    Refresh();
                }
            }
        }

        public void LootAll()
        {
            //NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootAll");
        }
    }
}