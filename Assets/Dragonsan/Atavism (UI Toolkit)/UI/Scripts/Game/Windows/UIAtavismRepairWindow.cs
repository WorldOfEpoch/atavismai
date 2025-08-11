using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Atavism.UI;

namespace Atavism.UI
{
    public class UIAtavismRepairWindow : UIAtavismWindowBase
    {
        public List<UIAtavismRepairSlot> repairSlots= new List<UIAtavismRepairSlot>();
        VisualElement m_SlotContainer;
        public int numberOfSlots;

        private Button m_repairButton;
        private Button m_repairAllButton;
        private UIAtavismCurrencyDisplay currencyDisplays;
        // Use this for initialization

        protected override bool registerUI()
        {
            if(!base.registerUI())
                return false;

            m_SlotContainer = uiWindow.Q<VisualElement>("items");
            m_repairButton = uiWindow.Q<Button>("repair-button");
            m_repairAllButton = uiWindow.Q<Button>("repair-all-button");

            m_repairAllButton.clicked += RepairAll;
            m_repairButton.clicked += Repair;
            VisualElement currency  = uiWindow.Q<VisualElement>("currency");
            currencyDisplays = new UIAtavismCurrencyDisplay();
            currencyDisplays.SetVisualElement(currency);
            currencyDisplays.ReverseOrder = true;
            for (int i = 0; i < numberOfSlots; i++)
            {
                UIAtavismRepairSlot newListEntryLogic = new UIAtavismRepairSlot();
                // Instantiate the UXML template for the entry
                var newListEntry = AtavismSettings.Instance.UISlotInventoryItemUXML.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                repairSlots.Add(newListEntryLogic);
                m_SlotContainer.Add(newListEntry);
                RegisterCallbacksFromInventoryTarget(newListEntryLogic);
                
            }
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("REPAIR_COMPLETE", this);
            AtavismEventSystem.RegisterEvent("REPAIR_START", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected  override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("REPAIR_COMPLETE", this);
            AtavismEventSystem.UnregisterEvent("REPAIR_START", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
        }
        protected void RegisterCallbacksFromInventoryTarget(VisualElement thisobject)
        {

        }
        protected void UnregisterCallbacksFromInventoryTarget(VisualElement thisobject)
        {

        }

        void OnDisable()
        {
            foreach (UIAtavismRepairSlot repairSlot in repairSlots)
            {
                repairSlot.UpdateRepairSlotData(null);
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "REPAIR_COMPLETE")
            {
                // Clear slots 
                foreach (UIAtavismRepairSlot repairSlot in repairSlots)
                {
                    repairSlot.UpdateRepairSlotData(null);
                }
            }
            else if (eData.eventType == "REPAIR_START")
            {
                if (AtavismCursor.Instance != null)
                    AtavismCursor.Instance.SetUIActivatableClickedOverride(PlaceRepirItem);
                Show();
            }
            if (eData.eventType == "CURRENCY_UPDATE"||eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                UpdateCurrencies();
            }
        }
        void UpdateCurrencies()
        {
            currencyDisplays.MainCurrency();
            currencyDisplays.Show();
        }
        // public void RepairListUpdated(UIAtavismRepairSlot slot)
        // {
        //     //TODO: update currency display
        // }

        public void Repair()
        {
            List<AtavismInventoryItem> itemsToRepair = new List<AtavismInventoryItem>();
            foreach (UIAtavismRepairSlot repairSlot in repairSlots)
            {
                if (repairSlot != null && repairSlot.UIActivatable != null)
                {
                    itemsToRepair.Add((AtavismInventoryItem)repairSlot.UIActivatable.ActivatableObject);
                }
            }
            NpcInteraction.Instance.RepairItems(itemsToRepair);
        }

        public void RepairAll()
        {
            NpcInteraction.Instance.RepairAllItems();
        }

        private void PlaceRepirItem(UIAtavismActivatable activatable)
        {
            if (activatable.Link != null)
            {
                return;
            }
            AtavismInventoryItem item = (AtavismInventoryItem)activatable.ActivatableObject;
            if (item != null)
            {
                if (item.MaxDurability > 0 && item.Durability < item.MaxDurability && item.repairable)
                {
                    foreach (UIAtavismRepairSlot repairSlot in repairSlots)
                    {
                        if (repairSlot != null && repairSlot.UIActivatable == null)
                        {
                            repairSlot.SetActivatable(activatable);
                            return;
                        }
                    }
                }
                else
                {
                    activatable.PreventDiscard();

                    string[] args = new string[1];
#if AT_I2LOC_PRESET
                    args[0] = I2.Loc.LocalizationManager.GetTranslation("Wrong Item");
#else
                    args[0] = "Wrong Item";
#endif
                    AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                }
            }
        }

        public override void Show()
        {
            base.Show();
            AtavismUIUtility.BringToFront(gameObject);
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.SetUIActivatableClickedOverride(PlaceRepirItem);
            UpdateCurrencies();
        }

        public override void Hide()
        {
            base.Hide();
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(PlaceRepirItem);

            foreach (UIAtavismRepairSlot repairSlot in repairSlots)
            {
                if (repairSlot != null)
                    repairSlot.Discarded();
            }
        }
     
    }
}
