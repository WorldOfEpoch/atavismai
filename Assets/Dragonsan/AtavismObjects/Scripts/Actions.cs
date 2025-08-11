using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace Atavism
{

    public enum ActionType
    {
        Ability,
        Item,
        None
    }

    public class AtavismAction
    {
        public ActionType actionType;
        public Activatable actionObject;
        public int bar;
        public int slot;

        public void Activate()
        {
            if (actionObject != null)
                actionObject.Activate();
        }

        public void DrawTooltip(float x, float y)
        {
            actionObject.DrawTooltip(x, y);
        }
    }

    public class Actions : MonoBehaviour
    {

        static Actions instance;

        public bool removeEmptyItemsFromActionBar = true;
        public bool removeWeaponItemsFromActionBar = false;
        List<List<AtavismAction>> actions = new List<List<AtavismAction>>();
        int mainActionBar = 0; // Which bar is currently sitting in the main Action Bar UI
        List<Dictionary<string, object>> actionsMsgQueue = new List<Dictionary<string, object>>();

        protected AtavismAction comboAction = null;
        protected bool comboShowCenter = true;
        protected bool comboReplaceSlot = true;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("actions", HandleActionsUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("comboAction", HandleComboUpdate);

            // Listen for the Abilities and Inventory updates as the action bar may need to be updated to match
            AtavismEventSystem.RegisterEvent("ABILITY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
        }

        private Dictionary<int, int> combosHistory = new Dictionary<int, int>();
        private int lastCombo = -1;
        private void HandleComboUpdate(Dictionary<string, object> props)
        {
           // Debug.LogError("HandleComboUpdate Start");
            int comboAbilityId = (int)props["cId"];
            int orgAbilityId = (int)props["aId"];
            
            int time = (int)props["time"];
            bool reset = (bool)props["r"];
            comboShowCenter = (bool)props["scu"];
           bool  comboReplaceSlot = (bool)props["rs"];
          //  Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" HandleComboUpdate comboAbilityId="+comboAbilityId+" orgAbilityId="+orgAbilityId+" time="+time+" reset="+reset); 
            if (comboAbilityId > 0)
            {
               // bool found = false;
               if(comboReplaceSlot)
                if (combosHistory.ContainsKey(comboAbilityId))
                {
                    combosHistory[comboAbilityId] = orgAbilityId;
                }
                else
                {
                    combosHistory.Add(comboAbilityId,orgAbilityId);
                }
                AtavismAction action = new AtavismAction();

                action.actionType = ActionType.Ability;
                if (reset)
                {
                    if (lastCombo == comboAbilityId)
                    {
                        action.actionObject = Abilities.Instance.GetAbility(orgAbilityId);
                        comboAction = null;
                    }
                }
                else
                {
                    action.actionObject = Abilities.Instance.GetAbility(comboAbilityId);
                    comboAction = action;
                    lastCombo = comboAbilityId;
                }
                if(comboReplaceSlot)
                foreach (var bar in actions)
                {
                    for (int i = 0; i < bar.Count; i++)
                    {
                        if (bar[i].actionType == ActionType.Ability)
                        {
                            
                            if (
                                (!reset && ((AtavismAbility)bar[i].actionObject).id == orgAbilityId) 
                                //|| (reset && ((AtavismAbility)bar[i].actionObject).id == comboAbilityId)
                                )
                            {
                              //  Debug.LogError("found orgAbilityId="+orgAbilityId+" change to "+comboAbilityId+" reset="+reset);
                                bar[i] = action;
                               // found = true;
                            }
                        }
                    }
                }

                if (!reset)
                {
                    if(comboReplaceSlot)
                    replaceActionFromHistory(action, comboAbilityId, orgAbilityId);
                }
            }
            else
            {
                comboAction = null;
            }
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("ACTION_UPDATE", event_args);
        //    Debug.LogError("HandleComboUpdate End");
            
        }

        void replaceActionFromHistory(AtavismAction action, int comboAbilityId, int orgAbilityId)
        {
            if (combosHistory.ContainsKey(orgAbilityId))
            {
                int parentAbilityId = combosHistory[orgAbilityId];
             //   bool found = false;
                foreach (var bar in actions)
                {
                    for (int i = 0; i < bar.Count; i++)
                    {
                        if (bar[i].actionType == ActionType.Ability)
                        {

                            if (
                                (((AtavismAbility)bar[i].actionObject).id == parentAbilityId))
                            {
                                //  Debug.LogError("found parentAbilityId=" + parentAbilityId + " change to " + comboAbilityId );
                                bar[i] = action;
                              //  found = true;
                            }
                        }
                    }
                }

                replaceActionFromHistory(action, comboAbilityId, parentAbilityId);
            }
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("ABILITY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
        }

        bool CheckParentCombo(int findAbility, int abilityToCheck)
        {
            while (combosHistory.ContainsKey(abilityToCheck))
            {
                 abilityToCheck = combosHistory[abilityToCheck];
                 if (findAbility == abilityToCheck)
                     return true;
            }

            return false;
        }
        
        private void Update()
        {
         
            
            if (AtavismSettings.Instance == null || Camera.main == null ||
                (AtavismSettings.Instance !=null &&AtavismSettings.Instance.isWindowOpened() )||
                (AtavismCursor.Instance!=null && ((AtavismCursor.Instance.isOverClaimObject() && !AtavismCursor.Instance.isOverClaimObject() )|| (!AtavismCursor.Instance.isOverClaimObject() && !AtavismCursor.Instance.isTargetCanBeAttack()) || AtavismCursor.Instance.IsMouseOverUI())) )
            {
            //    Debug.LogError("Over Object Break actions "+AtavismSettings.Instance.isWindowOpened()+" "+AtavismCursor.Instance.isOverObject()+" "+AtavismCursor.Instance.isOverClaimObject()+" "+AtavismCursor.Instance.isTargetCanBeAttack()+" "+AtavismCursor.Instance.IsMouseOverUI());
                return;
            }
            
            foreach (var entry in AtavismSettings.Instance.GetKeySettings().additionalActions)
            {
                if (Input.GetKeyDown(entry.key) || Input.GetKeyDown(entry.altKey))
                {
                 //   Debug.LogError("Action  mouse down");
                    foreach (var eItem in Inventory.Instance.EquippedItems.Values)
                    {
                        if (eItem.weaponProfile > 0)
                        {
                            WeaponProfileData wpd = AtavismPrefabManager.Instance.GetWeaponProfileByID(eItem.weaponProfile);
                            foreach (var act in wpd.actions)
                            {
                                if (act.action.Equals(entry.name) && eItem.slot.Equals(act.slot))
                                {
                                    if (act.abilityId > 0)
                                    {
                                        //   Debug.LogError("mouse down "+act.abilityId+" "+ComboAction);
                                        bool comboUsed = false;
                                        if (ComboAction != null)
                                        {
                                            AtavismAbility aa = (AtavismAbility)ComboAction.actionObject;


                                            int abId = combosHistory[aa.id];
                                            if (CheckParentCombo(act.abilityId, aa.id))
                                            {
                                                comboUsed = true;
                                                ComboAction.Activate();
                                            }
                                        }

                                        if (!comboUsed)
                                        {
                                            AtavismAbility aa = AtavismPrefabManager.Instance.LoadAbility(act.abilityId);
                                            if (aa != null)
                                            {
                                                // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" Abilities activate "+act.abilityId);
                                                aa.Activate();
                                            }
                                            else
                                            {
                                                //  Debug.LogError("Action  aa == null");
                                            }
                                        }
                                    }
                                    else if(act.zoom)
                                    {
                                     //   Debug.LogError("Action  Activate zoom");
                                        ClientAPI.GetInputController().ZoomActive = true;
                                    }

                                    if (!string.IsNullOrEmpty(act.coordEffect))
                                    {
                                    //    Debug.LogError("Action  Activate "+act.coordEffect);
                                        Dictionary<string, object> props = new Dictionary<string, object>();
                                        props.Add("sourceOID", OID.fromLong(ClientAPI.GetPlayerOid()));
                                        props.Add("targetOID", OID.fromLong(ClientAPI.GetPlayerOid()));
                                        props.Add("ceId",-1L);
                                        CoordinatedEffectSystem.ExecuteCoordinatedEffect(act.coordEffect, props);
                                    }
                                }
                                else
                                {
                                  //  Debug.LogError("Action  slot not ok");
                                }
                            }
                        }
                        else
                        {
                        //    Debug.LogError("Action  no weapon profile");
                        }
                    }
                }else  if (Input.GetKeyUp(entry.key) || Input.GetKeyUp(entry.altKey))
                {
                    // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" key up ");
                    foreach (var eItem in Inventory.Instance.EquippedItems.Values)
                    {
                        if (eItem.weaponProfile > 0)
                        {
                            WeaponProfileData wpd = AtavismPrefabManager.Instance.GetWeaponProfileByID(eItem.weaponProfile);
                            foreach (var act in wpd.actions)
                            {
                                if (act.action.Equals(entry.name) && eItem.slot.Equals(act.slot))
                                {
                                    // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" key up "+act.abilityId+" "+ComboAction);
                                //    Debug.LogError("mouse up "+act.abilityId+" "+ComboAction);
                                if (act.abilityId > 0)
                                {
                                    if (Abilities.Instance.abilityPowerUp > 0)
                                    {
                                       
                                        bool comboUsed = false;
                                        if (ComboAction != null)
                                        {
                                            AtavismAbility aa = (AtavismAbility)ComboAction.actionObject;


                                            int abId = combosHistory[aa.id];
                                            if (CheckParentCombo(act.abilityId, aa.id))
                                            {
                                                comboUsed = true;
                                                ComboAction.Activate();
                                            }
                                        }

                                        if (!comboUsed)

                                            {
                                                AtavismAbility aa = AtavismPrefabManager.Instance.LoadAbility(act.abilityId);
                                                if (aa != null)
                                                {
                                                    AbilityPrefabData apd = AtavismPrefabManager.Instance.GetAbilityPrefab(act.abilityId);
                                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                                        aa.Activate();
                                                }
                                            }

                                    }
                                    else
                                    {
                                       
                                     // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" Release Key no Abilities.Instance.abilityPowerUp " +Abilities.Instance.abilityPowerUp);
                                    }
                                }

                                if(act.zoom)
                                    {
                                     //   Debug.LogError("Action Deactivate zoom");
                                        ClientAPI.GetInputController().ZoomActive = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //   Debug.LogError("Action | no wepoan profile");
                        }
                    }
                }
                else
                {
                    
                }
            }
        }

        void ClientReady()
        {
            //ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("actions", ActionsPropertyHandler);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                foreach (Dictionary<string, object> qProps in actionsMsgQueue)
                {
                   // Debug.LogError("Running Queued actions update message");
                    HandleActionsUpdate(qProps);
                }
                if (!removeEmptyItemsFromActionBar)
                {
                    return;
                }

                for (int i = 0; i < actions.Count; i++)
                {
                    for (int j = 0; j < actions[i].Count; j++)
                    {
                        AtavismAction action = actions[i][j];
                        if (action != null && action.actionObject != null && action.actionType == ActionType.Item)
                        {
                            // verify the item count is still > 0
                            AtavismInventoryItem actionItem = (AtavismInventoryItem)action.actionObject;
                            if ((removeWeaponItemsFromActionBar  && actionItem.itemType.Equals("Weapon")|| !actionItem.itemType.Equals("Weapon")) && Inventory.Instance.GetCountOfItemAdnEquip(actionItem.templateId) < 1)
                            {
                                SetAction(i, j, null, false, 0, 0);
                            }
                        }
                    }
                }
            } else if (eData.eventType == "ABILITY_UPDATE")
            {
                foreach (Dictionary<string, object> qProps in actionsMsgQueue)
                {
                   // Debug.LogError("Running Queued actions update message");
                    HandleActionsUpdate(qProps);
                }
            }
        }

        public void SetAction(int bar, int slot, Activatable action, bool movingSlot, int sourceBar, int sourceSlot)
        {
            string actionString = "";
            if (action is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)action;
                actionString = "a" + ability.id;
            }
            else if (action is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)action;
                actionString = "i" + item.templateId;
            }
            //NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/updateActionBar " + slot + " " + actionString);
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("bar", bar);
            props.Add("slot", slot);
            props.Add("action", actionString);
            props.Add("movingSlot", movingSlot);
            props.Add("sourceBar", sourceBar);
            props.Add("sourceSlot", sourceSlot);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.UPDATE_ACTIONBAR", props);
        }

        void UpdateActions()
        {
            if (ClientAPI.GetPlayerObject() == null || !ClientAPI.GetPlayerObject().PropertyExists("actions"))
                return;
            actions.Clear();

            List<object> actions_prop = (List<object>)ClientAPI.GetPlayerObject().GetProperty("actions");
            AtavismLogger.LogDebugMessage("Got player actions property change: " + actions_prop);
            int pos = 0;
            //	int bar = 0;

            //foreach (List<object> actionList in actions_prop) {
            List<AtavismAction> actionBar = new List<AtavismAction>();
            foreach (string actionString in actions_prop)
            {
                AtavismAction action = new AtavismAction();
                if (actionString.StartsWith("a"))
                {
                    action.actionType = ActionType.Ability;
                    int abilityID = int.Parse(actionString.Substring(1));
                    action.actionObject = GetComponent<Abilities>().GetAbility(abilityID);
                }
                else if (actionString.StartsWith("i"))
                {
                    action.actionType = ActionType.Item;
                    int itemID = int.Parse(actionString.Substring(1));
                    action.actionObject = Inventory.Instance.GetItemByTemplateID(itemID);
                }
                else
                {
                    action.actionType = ActionType.None;
                }
                action.slot = pos;
                //if (actionBars[bar] != null)
                //	actionBars[bar].SendMessage("ActionUpdate", action);
                pos++;
                actionBar.Add(action);
            }
            actions.Add(actionBar);
            //}
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("ACTION_UPDATE", event_args);
        }

        public void HandleActionsUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogInfoMessage("Got Actions Update");
            try
            {
                if (!Abilities.Instance.AbilityLoaded || !Inventory.Instance.InventoryItemLoaded)
                {
                    if(!actionsMsgQueue.Contains(props))
                      actionsMsgQueue.Add(props);
                    return;
                }
                actions.Clear();
                mainActionBar = (int)props["currentBar"];
                int numBars = (int)props["numBars"];
                for (int i = 0; i < numBars; i++)
                {
                    List<AtavismAction> actionBar = new List<AtavismAction>();
                    int barActionCount = (int)props["barActionCount" + i];
                    for (int j = 0; j < barActionCount; j++)
                    {
                        AtavismAction action = new AtavismAction();
                        string actionString = (string)props["bar" + i + "action" + j];
                        if (actionString.StartsWith("a"))
                        {
                            action.actionType = ActionType.Ability;
                            int abilityID = int.Parse(actionString.Substring(1));
                            action.actionObject = Abilities.Instance.GetAbility(abilityID);
                        }
                        else if (actionString.StartsWith("i"))
                        {
                            action.actionType = ActionType.Item;
                            int itemID = int.Parse(actionString.Substring(1));
                            action.actionObject = Inventory.Instance.GetItemByTemplateID(itemID);
                        }
                        else
                        {
                            action.actionType = ActionType.None;
                        }
                        action.slot = j;
                        actionBar.Add(action);
                    }
                    actions.Add(actionBar);
                }

                // dispatch a ui event to tell the rest of the system
                string[] event_args = new string[1];
                AtavismEventSystem.DispatchEvent("ACTION_UPDATE", event_args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Auction.HandleActionsUpdate Exeption " +e.Message+"\n\n"+e.StackTrace);
            }
            AtavismLogger.LogDebugMessage("HandleActionsUpdate End");
        }

        /*public void ActionsPropertyHandler(object sender, ObjectPropertyChangeEventArgs args) {
            if (args.Oid != ClientAPI.GetPlayerOid())
                return;
            UpdateActions();
        }*/

        public void AddActionBar(GameObject actionBar, int slot)
        {
        }

        public static Actions Instance
        {
            get
            {
                return instance;
            }
        }

        public List<List<AtavismAction>> PlayerActions
        {
            get
            {
                return actions;
            }
        }

        public int MainActionBar
        {
            get
            {
                return mainActionBar;
            }
        }

        public AtavismAction ComboAction
        {
            get
            {
                return comboAction;
            }
        }
        public bool ComboReplaceSlot
        {
            get
            {
                return comboReplaceSlot;
            }
        }
        public bool ComboShowCenter
        {
            get
            {
                return comboShowCenter;
            }
        }
    }
}