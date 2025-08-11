 using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
 using System.Xml;

 namespace Atavism
{

    [Serializable]
    public class SetsSlots
    {
        public String name;
        public Button button;
        public Image selected;
        // public List<UGUICharacterEquipSlot> slots;

    }
    public class UGUICharacterEquipFrame : MonoBehaviour
    {

        public UGUIPanelTitleBar titleBar;
        public List<UGUICharacterEquipSlot> slots;
        public List<SetsSlots> setsSlots;
        public UGUIItemDisplay ammoSlot;
        public UGUICharacterEquipSlot ammoSlot2;
        public bool useSetButtonSelectedTint = true; 
        public Color setButtonSelectedTint = Color.green;
        public Color setButtonNotSelectedTint = Color.white;
        private float clicklimit;


        // Use this for initialization
        void Start()
        {
            AtavismEventSystem.RegisterEvent("EQUIPPED_UPDATE", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SLOTS_UPDATE", this);
            UpdateEquipSlots();

            if (titleBar != null)
            {
                titleBar.SetPanelTitle(ClientAPI.GetPlayerObject().Name);
            }
        }

        void OnEnable()
        {
            UpdateEquipSlots();
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("EQUIPPED_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SLOTS_UPDATE", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "EQUIPPED_UPDATE" || eData.eventType == "ITEM_ICON_UPDATE"|| eData.eventType == "SLOTS_UPDATE")
            {
                // Update 
                UpdateEquipSlots();
          
            }

            if (eData.eventType == "EQUIPPED_UPDATE")
            {
                clicklimit = 0;
            }
        }
        
        public void UpdateEquipSlots()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    AtavismInventoryItem item = GetItemInSlot(slots[i].slotName);
                    slots[i].UpdateEquipItemData(item);
                }
            }

            if (ammoSlot2 != null)
                ammoSlot2.UpdateEquipItemData(Inventory.Instance.EquippedAmmo);
            if (Inventory.Instance.EquippedAmmo != null)
            {
                //	ammoSlot.gameObject.SetActive(true);
                if (ammoSlot2 != null)
                    ammoSlot2.UpdateEquipItemData(Inventory.Instance.EquippedAmmo);
                //ammoSlot.SetItemData(Inventory.Instance.EquippedAmmo, Inventory.Instance.UnequipAmmo);
                //    if (ammoSlot.countText != null)
                //		ammoSlot.countText.text = Inventory.Instance.GetCountOfItem(Inventory.Instance.EquippedAmmo.templateId).ToString();
            }
            else
            {
                //	ammoSlot.gameObject.SetActive(false);
            }
            if(setsSlots!=null)
                foreach (var slot in setsSlots)
                {
                    if (slot != null)
                    {
                        if (Inventory.Instance.sets.Contains(slot.name))
                        {
                            if (slot.button != null && !slot.button.gameObject.activeSelf)
                            {
                                slot.button.gameObject.SetActive(true);
                            }
                            if (slot.button != null){
                                slot.button.onClick.RemoveAllListeners();
                                slot.button.onClick.AddListener(()=> ClickSelectSet(slot.name));
                            }

                            if (slot.name.Equals(Inventory.Instance.GetSetSelected))
                            {
                                if (slot.selected != null)
                                    if (!slot.selected.enabled)
                                        slot.selected.enabled = true;
                                if (useSetButtonSelectedTint)
                                {
                                    if (slot.button != null)
                                    {
                                        slot.button.image.color = setButtonSelectedTint;
                                    }
                                }
                            }
                            else
                            {
                                if (slot.selected != null)
                                    if (slot.selected.enabled)
                                        slot.selected.enabled = false;
                                if (slot.button != null)
                                {
                                    slot.button.image.color = setButtonNotSelectedTint;
                                }
                            }
                        }
                        else
                        {
                            if (slot.button != null && slot.button.gameObject.activeSelf)
                            {
                                slot.button.onClick.RemoveAllListeners();
                                slot.button.gameObject.SetActive(false);
                               
                            }
                            if (slot.selected != null)
                                if (slot.selected.enabled)
                                    slot.selected.enabled = false;
                        }
                    }
                }
        }

        AtavismInventoryItem GetItemInSlot(string slotName)
        {
            foreach (AtavismInventoryItem item in Inventory.Instance.EquippedItems.Values)
            {
                string orgSlotName = Inventory.Instance.GetItemByTemplateID(item.TemplateId).slot;

                if (Inventory.Instance.itemGroupSlots.ContainsKey(slotName))
                {
                    //Debug.LogError(item.ItemId+" found Group "+slotName+" GS:"+Inventory.Instance.itemGroupSlots[slotName]);
                    try
                    {


                        if (Inventory.Instance.itemGroupSlots[orgSlotName].all)
                        {
                            foreach (var s in Inventory.Instance.itemGroupSlots[orgSlotName].slots)
                            {
                                // Debug.LogError(slotName+" item "+item.ItemId+" "+item.slot+" orgSlotName="+orgSlotName+" set? "+(!(item.slot.StartsWith("Set_") && item.slot.ToLower().Contains(orgSlotName.ToLower())))+" Set_? "+item.slot.StartsWith("Set_")+" orgslot in slot? "+item.slot.ToLower().Contains(orgSlotName.ToLower())+" "+s.name.ToLower().Contains(orgSlotName.ToLower()));
                                if (s.name.ToLower() == slotName.ToLower() && !item.slot.StartsWith("Set_"))
                                {
                                    // Debug.LogError(slotName+" return items "+item.ItemId);
                                    return item;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(item.ItemId + "Exception slotName: " + slotName + " orgSlotName: " + orgSlotName+" "+e);
                    }

                    foreach (var s in Inventory.Instance.itemGroupSlots[slotName].slots)
                    {
                        // Debug.LogError(item.ItemId+" "+slotName+"Group Slot "+s.name+" | "+slotName);

                        if (s.name.ToLower() == item.slot.ToLower())
                        {
                            // Debug.LogError(slotName+" return items "+item.ItemId);
                            return item;
                        }
                    }

                }

            }

            return null;
        }

        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            if (gameObject.activeSelf)
                AtavismUIUtility.BringToFront(transform.parent.gameObject);
        }

        public void ClickSelectSet(String set)
        {
            if (clicklimit > Time.time)
                return;
            clicklimit = Time.time + 1f;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("set", set);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.SWITCH_WEAPON", props);
        }
    }
}