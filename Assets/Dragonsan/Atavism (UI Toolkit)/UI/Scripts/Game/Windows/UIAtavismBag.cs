using Atavism.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism.UI
{

    public class UIAtavismBag : MonoBehaviour
    {
        public int bagNum;
        public UIAtavismInventorySlot slotPrefab;
        public float verticalPadding = 50;
        List<UIAtavismInventorySlot> slots = new List<UIAtavismInventorySlot>();
        bool open = false;

        // Use this for initialization
        void Start()
        {
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            UpdateInventory();
        }

        void OnEnable()
        {
            UpdateInventory();
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
        }

        void Update()
        {
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                // Update 
                UpdateInventory();
            }
        }

        public void UpdateInventory()
        {
            Bag bag = Inventory.Instance.Bags[bagNum];

            int numSlots = bag.numSlots;
            if (bag.numSlots != numSlots)
            {
                slots.Clear();
                var tmpbagSlots = new List<UIAtavismInventorySlot>(slots);

                foreach (var item in tmpbagSlots)
                {
                    if (item != null)
                    {
                        slots.Remove(item);
                        item.Clear();
                    }
                }

                for (int i = 0; i < bag.numSlots; i++)
                {
                    UIAtavismInventorySlot slot = new UIAtavismInventorySlot();
                    slot.name = "UIAtavismInventorySlot" + i;
                    slot.bagNum = bagNum;
                    slot.slotNum = i;
                    slots.Add(slot);
                }
            }

            for (int i = 0; i < bag.numSlots; i++)
            {
                if (bag.items.ContainsKey(i))
                {
                    slots[i].UpdateInventoryItemData(bag.items[i], null);
                }
                else
                {
                    slots[i].UpdateInventoryItemData(null, null);
                }
            }
        }

        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public bool Open
        {
            get
            {
                return open;
            }
        }
    }
}