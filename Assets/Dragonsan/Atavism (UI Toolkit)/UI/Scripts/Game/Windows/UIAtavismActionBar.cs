using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismActionBar : MonoBehaviour
    {

        [SerializeField] private UIDocument uiDocument;
        public int barNum = 0;
        public bool mainActionBar = true;
        public KeyCode[] activateKeys;
        public List<UIAtavismActionBarSlot> slots = new List<UIAtavismActionBarSlot>();
        private VisualElement slotGrid;
        [SerializeField] private VisualTreeAsset actionSlotTemplate;
        [SerializeField] private int numberOfSlots;

        private VisualElement m_Root;
    // public UIAtavismActionBarSlot comboSlot;
        // private bool presed = false;
        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if(uiDocument==null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            m_Root = uiDocument.rootVisualElement;
            slotGrid = uiDocument.rootVisualElement.Q<VisualElement>("slot-grid");
            if (slotGrid == null)
            {
                for (int i = 0; i < numberOfSlots; i++)
                {
                    VisualElement v = uiDocument.rootVisualElement.Q<VisualElement>("slot-"+i);
                    if (v != null)
                    {
                        UIAtavismActionBarSlot newListEntryLogic = new UIAtavismActionBarSlot();
                        v.userData = newListEntryLogic;
                        // Initialize the controller script
                        if (activateKeys.Length > i)
                            newListEntryLogic.activateKey = activateKeys[i];
                        newListEntryLogic.slotNum = i;
                        newListEntryLogic.SetVisualElement(v);
                        slots.Add(newListEntryLogic);
                    }
                }
            }
            else
            {
                slotGrid.Clear();
                for (int i = 0; i < numberOfSlots; i++)
                {
                    // Instantiate a controller for the data
                    UIAtavismActionBarSlot newListEntryLogic = new UIAtavismActionBarSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = actionSlotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = newListEntryLogic;
                    // Initialize the controller script
                    if (activateKeys.Length > i)
                        newListEntryLogic.activateKey = activateKeys[i];
                    newListEntryLogic.slotNum = i;
                    newListEntryLogic.SetVisualElement(newListEntry);
                    slots.Add(newListEntryLogic);
                    // Return the root of the instantiated visual tree
                    slotGrid.Add(newListEntry);
                }
            }

            registerEvents();
        }

        // Use this for initialization
        void registerEvents()
        {
            //Actions.Instance.AddActionBar(gameObject, barNum);

            AtavismEventSystem.RegisterEvent("ACTION_UPDATE", this);
            AtavismEventSystem.RegisterEvent("COOLDOWN_UPDATE", this);
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("ABILITY_UPDATE", this);
            UpdateActions();
        }

        ~UIAtavismActionBar()
        {
            AtavismEventSystem.UnregisterEvent("ACTION_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("COOLDOWN_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("ABILITY_UPDATE", this);
        }


        void Update()
        {
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().sprint.key)||Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().sprint.altKey)) && !ClientAPI.UIHasFocus())
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("state", 1);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.SPRINT", props);
            }
            else if((Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().sprint.key )|| Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().sprint.altKey)) && !ClientAPI.UIHasFocus())
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("state", 0);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.SPRINT", props);
            }

            foreach (var slot in slots)
            {
                if(slot!=null)
                slot.Update();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ACTION_UPDATE") 
            {
                UpdateActions();
            }
            else if (eData.eventType == "ABILITY_UPDATE")
            {
                UpdateActions();
            }
            else if (eData.eventType == "COOLDOWN_UPDATE")
            {
                // UpdateActions();
            }
            else if (eData.eventType == "INVENTORY_UPDATE")
            {
                UpdateActions();
            }
        }

        public void UpdateActions()
        {
          //  Debug.LogError("UpdateActions called "+Actions.Instance.PlayerActions.Count);
            List<List<AtavismAction>> actionBars = Actions.Instance.PlayerActions;
            if (actionBars.Count == 0)
            {
                return;
            }
            if (mainActionBar)
            {
                barNum = Actions.Instance.MainActionBar;
            }

            List<AtavismAction> actionBar = new List<AtavismAction>();
            if (actionBars.Count > barNum)
            {
                actionBar = actionBars[barNum];
            }
            for (int i = 0; i < slots.Count; i++)
            {
                // if (slots[i]!=null)
                    if (actionBar.Count > i)
                    {
                        slots[i].UpdateActionData(actionBar[i], barNum);
                    }
                    else
                    {
                        slots[i].UpdateActionData(null, barNum);

                    }
            }

            // if (comboSlot)
            // {
            //     if (mainActionBar & Actions.Instance.ComboAction != null)
            //     {
            //         if (!comboSlot.gameObject.activeSelf)
            //             comboSlot.gameObject.SetActive(true);
            //         comboSlot.UpdateActionData(Actions.Instance.ComboAction,-1);
            //     }
            //     else
            //     {
            //         if (comboSlot.gameObject.activeSelf)
            //         comboSlot.gameObject.SetActive(false);
            //     }
            // }
        }

        // public void Toggle()
        // {
        //     gameObject.SetActive(!gameObject.activeSelf);
        // }
    }
}