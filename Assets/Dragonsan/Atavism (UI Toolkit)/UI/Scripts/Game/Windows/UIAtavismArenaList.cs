using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Xml.Linq;
using Atavism.UI.Game;

namespace Atavism.UI
{
    public class UIAtavismArenaList : UIAtavismWindowBase
    {
        [SerializeField] private VisualTreeAsset  arenaListTemplate;

        public Button joinGroupButton;
        public Button joinSoloButton;
        public Button leaveButton;
        public Label instanceName;
        public Label instanceArenaType;
        public Label instanceTime;
        public Label instanceLevel;
        public Label instanceDesc;
        public Label instanceNameTitle;
        public Label instanceArenaTypeTitle;
        public Label instanceTimeTitle;
        public Label instanceLevelTitle;
        public Label instanceDescTitle;

        private VisualElement infoPanel;
        // public Color normalColorText = Color.black;
        // public Color FailedColorText = Color.red;
        ArenaEntry selectedArena;
        private ListView arenaGrid;
        private List<UIAtavismArenaListEntry> arenaList = new List<UIAtavismArenaListEntry>();


        protected override void OnEnable()
        {
            base.OnEnable();

            Hide();
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
            AtavismEventSystem.RegisterEvent("ARENA_LIST_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("ARENA_LIST_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
            
            base.unregisterEvents();
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
         //   uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("ArenaWindow");

            // tabs = uiWindow.Query<UIButtonToggleGroup>("auction-top-menu");
            // tabs.OnItemIndexChanged += TopMenuChange;
            //  
            // buyPanel = uiWindow.Query<VisualElement>("auction-buy-panel");
            VisualElement innerPanel = uiWindow.Query<VisualElement>("inner-panel");
            arenaGrid = innerPanel.Query<ListView>("arena-list-grid");
#if UNITY_6000_0_OR_NEWER                
            ScrollView scrollView = arenaGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            arenaGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismArenaListEntry newListEntryLogic = new UIAtavismArenaListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = arenaListTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                arenaList.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            arenaGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismArenaListEntry).SetData(Arena.Instance.ArenaEntries[index]);
            };
            arenaGrid.selectionChanged += SelectArena;
         
            // arenaGrid.fixedItemHeight = 65;

            infoPanel = innerPanel.Query<VisualElement>("info-panel");
            infoPanel.visible = false;
            instanceNameTitle = innerPanel.Query<Label>("info-arena-name-label");
            instanceName = innerPanel.Query<Label>("info-arena-name");
            instanceArenaTypeTitle = innerPanel.Query<Label>("info-arena-type-label");
            instanceArenaType = innerPanel.Query<Label>("info-arena-type");
            instanceTimeTitle = innerPanel.Query<Label>("info-time-label");
            instanceTime = innerPanel.Query<Label>("info-time");
            instanceLevelTitle = innerPanel.Query<Label>("info-level-label");
            instanceLevel = innerPanel.Query<Label>("info-level");
            instanceDescTitle = innerPanel.Query<Label>("info-description-label");
            instanceDesc = innerPanel.Query<Label>("info-description");
            
            leaveButton = innerPanel.Query<Button>("leave");
            leaveButton.clicked += LeaveQueue;
            joinSoloButton = innerPanel.Query<Button>("join-solo");
            joinSoloButton.clicked += JoinQueueSolo;
            joinGroupButton = innerPanel.Query<Button>("join-group");
            joinGroupButton.clicked += JoinQueueGroup;
           
            return true;
        }

        private void SelectArena(IEnumerable<object> obj)
        {
            if (obj.Count() == 0)
                return;
         //   Debug.LogError("SelectArena "+String.Join(",",obj)+"|"+  arenaGrid.selectedIndex+"|"+  arenaGrid.selectedItem);
            ArenaEntry arena = (ArenaEntry)obj.First();
          //  Debug.LogError("SelectArena "+arena+" "+(arena != null?arena.ArenaName+" "+arena.ArenaId + " "+arena.index:" "));
            Arena.Instance.ArenaEntrySelected(arena.index);
            SetArenaDetails();
            arenaGrid.Rebuild();
            // foreach (var _arena in arenaList)
            // {
            //     _arena.UpdateDisplay();
            // }
        }

       
        public override void Show()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("playerOid", ClientAPI.GetPlayerOid());
            props.Add("cat", 1);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.getList", props);
            base.Show();

            // ClearAllCells();
            // Refresh();
        }

        public override void Hide()
        {
            base.Hide();
            Arena.Instance.ArenaEntrySelected(0);
        }

      

        // Your other methods and logic

        // public override int NumberOfCells()
        // {
        //     return Arena.Instance.ArenaEntries.Count;
        // }
        //
        // public override void UpdateCell(int index, UIAtavismArenaListEntry cell)
        // {
        //     cell.SetArenaEntryDetails(Arena.Instance.ArenaEntries[index], index, this);
        // }

        /// <summary>
        /// Function run after get message from atavism evant system
        /// </summary>
        /// <param name="eData"></param>
        public void OnEvent(AtavismEventData eData)
        {
       //     Debug.LogError("Arena OnEvent "+eData.eventType);
            if (eData.eventType == "ARENA_LIST_UPDATE" || eData.eventType == "UPDATE_LANGUAGE")
            {
                // Delete the old list
                // ClearAllCells();
                // Refresh();
                // Debug.LogError("Arena OnEvent count  "+Arena.Instance.ArenaEntries.Count+" "+Arena.Instance.GetSelectedArenaEntry());
                if (Arena.Instance.GetSelectedArenaEntry() == null)
                {
                    UpdateList();
                }
                else
                {
                    if (arenaList.Count != Arena.Instance.ArenaEntries.Count)
                    {
                        UpdateList();
                    }
                    else
                    {
                        arenaGrid.Rebuild();
                        // foreach (var _arena in arenaList)
                        // {
                        //   
                        //     
                        //     _arena.UpdateDisplay();
                        // }
                    }
                }
                SetArenaDetails();
            }
        }


        void UpdateList()
        {
            arenaGrid.Clear();
            arenaList.Clear();
            // auctionsList = AtavismAuction.Instance.Auctions.Values.ToList();
            // int i = 1;
            // Debug.LogError("Arena UpdateList "+Arena.Instance.ArenaEntries.Count);
                
            arenaGrid.itemsSource = Arena.Instance.ArenaEntries;
            arenaGrid.Rebuild();
            arenaGrid.selectedIndex = -1;
            arenaGrid.fixedItemHeight = 23;
        }
        
        public void SetArenaDetails()
        {
            
           // Debug.LogError("Arena SetArenaDetails "+Arena.Instance.ArenaEntries.Count);
            
            int level = 1;
            if (ClientAPI.GetPlayerObject() != null)
                if (ClientAPI.GetPlayerObject().PropertyExists("level"))
                {
                    level = (int)ClientAPI.GetPlayerObject().GetProperty("level");
                }

            selectedArena = Arena.Instance.GetSelectedArenaEntry();
            if (selectedArena == null)
            {
                infoPanel.visible = false;
                return;
            }

            infoPanel.visible = true;
            DateTime arenaStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, selectedArena.StartHour, selectedArena.StartMin, 0);
            DateTime arenaEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, selectedArena.EndHour, selectedArena.EndMin, 0);
            DateTime timeNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TimeManager.Instance.Hour, TimeManager.Instance.Minute, 0);
            Color color = Color.white;
            // Color color = normalColorText;
            //
            if (selectedArena.StartHour == 0 && selectedArena.StartMin == 0 && selectedArena.EndHour == 0 && selectedArena.EndMin == 0)
            {
                // color = normalColorText;
                instanceTime.RemoveFromClassList("arena-entry-require-failed");
            }
            else if (timeNow > arenaStart && timeNow < arenaEnd)
            {
                // color = normalColorText;
                instanceTime.RemoveFromClassList("arena-entry-require-failed");
            }
            else
            {
                // color = FailedColorText;
                instanceTime.AddToClassList("arena-entry-require-failed");
            }

            
            // Adjust buttons based on whether the arena is queued or not
            if (selectedArena.ArenaQueued)
            {
                joinGroupButton.HideVisualElement();
                joinSoloButton.HideVisualElement();
                leaveButton.ShowVisualElement();
            }
            else
            {
                bool isInTimeAndLevelRange = (timeNow > arenaStart && timeNow < arenaEnd) && (level <= selectedArena.MaxLeval && level >= selectedArena.ReqLeval);
                bool isAlwaysAvailable = selectedArena.StartHour == 0 && selectedArena.StartMin == 0 && selectedArena.EndHour == 0 && selectedArena.EndMin == 0 && (level <= selectedArena.MaxLeval && level >= selectedArena.ReqLeval);

                if (isAlwaysAvailable || isInTimeAndLevelRange)
                {
                    joinSoloButton.ShowVisualElement();
                    if(selectedArena.teamSize[0] > 1 && AtavismGroup.Instance.Members.Count > 0) 
                        joinGroupButton.ShowVisualElement();
                    else
                    {
                        joinGroupButton.HideVisualElement();
                    }
                }
                else
                {
                    joinSoloButton.HideVisualElement();
                    joinGroupButton.HideVisualElement();
                }
                leaveButton.HideVisualElement();
            }

            // Here, you can set the text for your labels based on localization or not.
            // I will provide a version without localization for clarity.
#if AT_I2LOC_PRESET
            this.instanceNameTitle.text = I2.Loc.LocalizationManager.GetTranslation("Arena/InstanceName")+": ";
            this.instanceName.text = ( string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Arena/" + selectedArena.ArenaName)) ? selectedArena.ArenaName : I2.Loc.LocalizationManager.GetTranslation("Arena/" + selectedArena.ArenaName));
            this.instanceArenaTypeTitle.text = I2.Loc.LocalizationManager.GetTranslation("Arena/InstanceType") + ": ";
            this.instanceArenaType.text =  (selectedArena.ArenaType==1? string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Arena/DEATHMATCH_ARENA")) ? "Deathmatch" : I2.Loc.LocalizationManager.GetTranslation("Arena/DEATHMATCH_ARENA"): string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Arena/CTF_ARENA")) ? "Capture the flag" : I2.Loc.LocalizationManager.GetTranslation("Arena/CTF_ARENA"));
            this.instanceTimeTitle.text = (string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Arena/available")) ? "Available:" : I2.Loc.LocalizationManager.GetTranslation("Arena/available")+":");
            this.instanceTime.text =  "<color=" + ColorTypeConverter.ToRGBHex(color) + ">"+" " +selectedArena.StartHour + ":" + (selectedArena.StartMin < 10 ? "0" + selectedArena.StartMin.ToString() : selectedArena.StartMin.ToString()) + " - " + selectedArena.EndHour + ":" + (selectedArena.EndMin < 10 ? "0" + selectedArena.EndMin.ToString() : selectedArena.EndMin.ToString()) + "</color>";
            this.instanceDesc.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Arena/" + selectedArena.Description)) ? selectedArena.Description : I2.Loc.LocalizationManager.GetTranslation("Arena/" + selectedArena.Description);
            this.instanceLevelTitle.text = "";
            this.instanceLevel.text = "";
#else            
            this.instanceNameTitle.text ="Instance Name:" ;
            this.instanceName.text =selectedArena.ArenaName;
            this.instanceArenaTypeTitle.text = "Instance Type: ";
            this.instanceArenaType.text = (selectedArena.ArenaType == 1 ? "Deathmatch" : "Capture the flag");
            this.instanceTimeTitle.text = "Available: ";
                this.instanceTime.text = selectedArena.StartHour + ":" +
                                         (selectedArena.StartMin < 10
                                             ? "0" + selectedArena.StartMin.ToString()
                                             : selectedArena.StartMin.ToString()) + " - " + selectedArena.EndHour + ":" +
                                         (selectedArena.EndMin < 10
                                             ? "0" + selectedArena.EndMin.ToString()
                                             : selectedArena.EndMin.ToString()) ;
            this.instanceDesc.text = selectedArena.Description;
            this.instanceLevelTitle.text = "";
            this.instanceLevel.text = "";
#endif
        }

           /// <summary>
        /// Function send message to server for leave from queue and refresh list
        /// </summary>
        /// <param name="item"></param>
        /// <param name="accepted"></param>
        void LeaveQueue(object item, bool accepted)
        {
            if (accepted)
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("arenaType", selectedArena.ArenaType);
                props.Add("arenaTemp", selectedArena.ArenaId);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.leaveQueue", props);
                foreach (var _arena in arenaList)
                {
                    _arena.UpdateDisplay();
                }
            }
        }

        public void JoinQueueSolo()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("arenaType", selectedArena.ArenaType);
            props.Add("arenaTemp", selectedArena.ArenaId);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.joinQueue", props);
        }
        public void JoinQueueGroup()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("arenaType", selectedArena.ArenaType);
            props.Add("arenaTemp", selectedArena.ArenaId);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.groupJoinQueue", props);
        }
        /// <summary>
        /// from click Function show confirmation leave queue
        /// </summary>
        public void LeaveQueue()
        {
#if AT_I2LOC_PRESET
        UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want abandon queue") + " " + I2.Loc.LocalizationManager.GetTranslation("Arena/" + selectedArena.ArenaName) + "?", null, LeaveQueue);
#else
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want abandon queue " + selectedArena.ArenaName + "?", null, LeaveQueue);
#endif

        }
    }
}
