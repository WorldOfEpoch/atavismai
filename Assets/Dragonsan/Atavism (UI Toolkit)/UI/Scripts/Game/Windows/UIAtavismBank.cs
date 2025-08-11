using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismBank : UIAtavismWindowBase
    {

        public int bagNum = 0;
       [SerializeField] VisualTreeAsset slotTemplate;
        VisualElement m_grid;
        // public float verticalPadding = 50;
        // public KeyCode toggleKey;
        List<UIAtavismBankSlot> slots = new List<UIAtavismBankSlot>();
     protected override void OnEnable()
        {
            base.OnEnable();
           
        }
        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("BANK_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CLOSE_STORAGE_WINDOW", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("BANK_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_STORAGE_WINDOW", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
        }
        // Use this for initialization

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            
            m_grid = uiWindow.Query<VisualElement>("slots");
            
            
            
            
            return true;
        }

        public override void Show()
        {
           base.Show();
       
            AtavismCursor.Instance.SetUIActivatableClickedOverride(PlaceBankItem);
            AtavismUIUtility.BringToFront(gameObject);
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("CLOSE_NPC_DIALOGUE", args);
        }

        public override void Hide()
        {
            base.Hide();
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(PlaceBankItem);
            Inventory.Instance.StorageClosed();
        }

        void Update()
        {
            base.Update();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "BANK_UPDATE")
            {
                bool open = bool.Parse(eData.eventArgs[0]);
                if (!showing && open)
                    Show();
                if (showing)
                    UpdateInventory();
            }
            else if (eData.eventType == "CLOSE_STORAGE_WINDOW")
            {
                if (showing)
                    Hide();
            }
            else if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (showing)
                    UpdateInventory();
            }
        }

        public void UpdateInventory()
        {
            Bag bag = null;
            if (Inventory.Instance.StorageItems.ContainsKey(bagNum))
                bag = Inventory.Instance.StorageItems[bagNum];
            if (bag == null)
            {
                Hide();
                return;
            }

            Debug.Log("Bank slot count: " + bag.numSlots);
            slots.Clear();
            m_grid.Clear();

            for (int i = 0; i < bag.numSlots; i++)
            {
                UIAtavismBankSlot newListEntryLogic = new UIAtavismBankSlot();
                // Instantiate the UXML template for the entry
                var newListEntry = AtavismSettings.Instance.UISlotInventoryItemUXML.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                newListEntryLogic.bagNum = bagNum;
                newListEntryLogic.slotNum = i;
                slots.Add(newListEntryLogic);
                m_grid.Add(newListEntry);
            }

            for (int i = 0; i < bag.numSlots; i++)
            {
                if (bag.items.ContainsKey(i))
                {
                    slots[i].UpdateInventoryItemData(bag.items[i]);
                }
                else
                {
                    slots[i].UpdateInventoryItemData(null);
                }
            }
        }

        public void PlaceBankItem(UIAtavismActivatable activatable)
        {
            if (activatable.Link != null)
                return;
            AtavismInventoryItem item = (AtavismInventoryItem)activatable.ActivatableObject;
            Inventory.Instance.PlaceItemInBank(item, item.Count);
        }
    }
}