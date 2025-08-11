using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace Atavism.UI
{


    public class UIAtavismLootList : UIAtavismWindowBase
    {
        [SerializeField] VisualTreeAsset listElementTemplate;
        private Button lootAllButton;
        private ScrollView grid;
        

        protected override void OnEnable()
        {
            base.OnEnable();
        

        }
        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("LOOT_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CLOSE_LOOT_WINDOW", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("LOOT_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_LOOT_WINDOW", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
        }
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            lootAllButton = uiWindow.Q<Button>("loot-button");
            lootAllButton.clicked += LootAll;
            grid = uiWindow.Q<ScrollView>("loot-items");
#if UNITY_6000_0_OR_NEWER    
                grid.mouseWheelScrollSize = 19;
#endif
            return true;
        }
        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            isRegisteredUI = false;

            return true;
        }

        void Start()
        {
            base.Start();

        }

        UIAtavismLootListEntry NewElement()
        {
       //     Debug.LogError("NewElement");
            UIAtavismLootListEntry newListEntryLogic = new UIAtavismLootListEntry();
            // Instantiate the UXML template for the entry
            var newListEntry = listElementTemplate.Instantiate();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
            grid.Add(newListEntry);
            return newListEntryLogic;
        }
        
        public  void UpdateCell(int index, LootListEntryUXML cell)
        {
            grid.Clear();
            // Assuming Inventory.Instance.Loot and Inventory.Instance.LootCurr are accessible here
            if (index < Inventory.Instance.Loot.Count)
            {
                cell.SetLootEntryDetails(Inventory.Instance.Loot[index]);
            }
            else
            {
                var currencyEntry = Inventory.Instance.LootCurr.ElementAt(index - Inventory.Instance.Loot.Count);
                cell.SetLootEntryDetails(currencyEntry.Key, currencyEntry.Value);
            }
        }

     

        private void Refresh()
        {
            grid.Clear();
            // Assuming Inventory.Instance.Loot and Inventory.Instance.LootCurr are accessible here
            foreach (var item in Inventory.Instance.Loot.Values)
            {
                var element = NewElement();
                element.SetLootEntryDetails(item);
            }

            foreach (var curr in Inventory.Instance.LootCurr.Keys)
            {
                var element = NewElement();
                element.SetLootEntryDetails(curr,Inventory.Instance.LootCurr[curr]);
            }
        }
        public override void Show()
        {
            base.Show();
            // Refresh the list
            Refresh();
        }

        public override void Hide()
        {
            // root.style.display = DisplayStyle.None;
            base.Hide();
            Inventory.Instance.Loot.Clear();
            Inventory.Instance.LootCurr.Clear();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "LOOT_UPDATE" || eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (Inventory.Instance.Loot.Count > 0 || Inventory.Instance.LootCurr.Count > 0)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
            else if (eData.eventType == "CLOSE_LOOT_WINDOW")
            {
                Hide();
            }
        }

        public void LootAll()
        {
            NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootAll");
        }


    }
}