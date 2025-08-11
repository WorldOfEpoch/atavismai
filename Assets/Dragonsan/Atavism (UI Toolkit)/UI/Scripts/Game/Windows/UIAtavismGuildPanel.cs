using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Atavism.UI.Game;
using UnityEngine.UIElements;

// using TMPro;

namespace Atavism.UI
{

    public class UIAtavismGuildPanel : UIAtavismWindowBase
    {
        [SerializeField] private VisualTreeAsset memberEntryTemplate;
        [SerializeField] private VisualTreeAsset rankEntryTemplate;
        private Label guildMotd;
        private VisualElement guildMotdPanel;

        public VisualElement createPopup;
        public UITextField guildNameField;
        
        public VisualElement invitePopup;
        public UITextField inviteNameField;
        public VisualElement memberPopup;
        private Button promoteButton;
        private Button demoteButton;
        private Button kickButton;
        
        public VisualElement guildSettingsPanel;
        
        
        
        // public Button settingsDropDown;
        // public VisualElement settingsContainer;
        // public List<Button> settingsButtons;
      
        private VisualElement memberList;
        private VisualElement rankPermissionsPanel;
        public Button rankDropDown;
        private VisualElement rankContainer;
        // public List<Button> rankButtons= new List<Button>();
       
        private Toggle guildChatListen;
        private Toggle guildChatSpeak;
        private Toggle inviteMember;
        private Toggle removeMember;
        private Toggle promote;
        private Toggle demote;
        private Toggle setMOTD;
        private Toggle editPublicNote;
        private Toggle claimAdd;
        private Toggle claimEdit;
        private Toggle claimAction;
        private Toggle levelUp;
        private Toggle warehouseAdd;
        private Toggle warehouseGet;

        private VisualElement guildRanksPanel;
        private VisualElement rankList;
        public List<UIAtavismGuildRank> guildRanks = new List<UIAtavismGuildRank>();
        [SerializeField] private int maxNumberOfRanks = 10;
        
        //public KeyCode toggleKey;
        public UIDropdown settingsDropdown;
        public UIDropdown rankModDropdown;
        private List<string> rankModList = new List<string>();
        [SerializeField] Button disbandButton;
        [SerializeField] Button leaveButton;
        [SerializeField] Button addRankButton;
    
        Button guildSettingsCloseButton;
        Button guildSettingsButton;
        Button addPlayerButton;
        Button leaveGuildButton;

        [SerializeField] VisualElement editMOTD;
        [SerializeField] UITextField editMOTDField;
        [SerializeField] Button editMOTDButton;

        private Label level;
        //Resource info
        private UIProgressBar levelProgressSlider;
        private Button resourcesButton;
        //Resource Window
        public VisualElement resourcesWindow;
        public List<UIAtavismItemDisplay> resourceRequired = new List<UIAtavismItemDisplay>();
        public List<UIAtavismItemDisplay> resourceCollected = new List<UIAtavismItemDisplay>();
        private Button levelUpButton;
        //Resource Count Panel
        public VisualElement resourceCountPanel;
        public UIAtavismItemDisplay resourceCountItemDisplay;
        public UITextField resourceCountText;
        public Label resourceItemName;
        private Button resourceAddButton;
        private Button resourceCancelButton;
        public Button minusButton;
        public Button plusButton;
        AtavismInventoryItem resourceItem;
      
        int resourceItemCount = 1;

        public Label memberNumText;

        [SerializeField] private bool changeWindowTitleToGuildName = false;
        // bool showing = false;
        AtavismGuildMember selectedMember = null;
        // bool settingsDropDownOpen = false;
         bool rankDropDownOpen = false;
        int selectedRank = 0;
        // [SerializeField] GameObject panel;
        float interactionDelay;
        bool create=false;
        // Use this for initialization
        protected override void OnEnable()
        {
            base.OnEnable();
           // toggleKey = AtavismSettings.Instance.GetKeySettings().guild;

           

            // for (int i = 0; i < rankButtons.Count; i++)
            // {
            //     rankButtons[i].gameObject.SetActive(true);
            // }
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            //  tabs = uiWindow.Query<UIButtonToggleGroup>("auction-top-menu");
            //  tabs.OnItemIndexChanged += TopMenuChange;
            //  
            memberPopup = uiScreen.Q<VisualElement>("member-popup");
            promoteButton = memberPopup.Q<Button>("promote-button");
            demoteButton = memberPopup.Q<Button>("demote-button");
            kickButton = memberPopup.Q<Button>("kick-button");
            promoteButton.clicked += PromoteMemberClicked;
            demoteButton.clicked += DemoteMemberClicked;
            kickButton.clicked += KickMemberClicked;

            guildMotd = uiWindow.Query<Label>("motd");
            guildMotdPanel = uiWindow.Query<VisualElement>("motd-panel");
            Clickable clickable = new Clickable(() => { });
            guildMotdPanel.AddManipulator(clickable);
            guildMotdPanel.RegisterCallback<MouseUpEvent>(EditMOTDMenuClicked);
            //Resource Info
            level = uiWindow.Query<Label>("guild-level");
            levelProgressSlider = uiWindow.Query<UIProgressBar>("guild-level-progress-bar");
            resourcesButton = uiWindow.Query<Button>("resource-button");
            resourcesButton.clicked += ShowResources;

            memberNumText = uiWindow.Query<Label>("member-num-text");
            memberList = uiWindow.Query<VisualElement>("member-list");


            leaveGuildButton = uiWindow.Query<Button>("leave-guild-button");
            leaveGuildButton.clicked += LeaveClicked;
            guildSettingsButton = uiWindow.Query<Button>("guild-settings-button");
            guildSettingsButton.clicked += ToggleRankPopup;
            addPlayerButton = uiWindow.Query<Button>("add-player-button");
            addPlayerButton.clicked += AddMemberClicked;

            guildSettingsPanel = uiWindow.Query<VisualElement>("guild-settings-panel");
            guildRanksPanel = guildSettingsPanel.Query<VisualElement>("rank-list-panel");
            rankList = guildRanksPanel.Query<VisualElement>("rank-list");

            guildSettingsCloseButton = guildSettingsPanel.Query<Button>("Window-close-button");
            guildSettingsCloseButton.clicked += ToggleRankPopup;
            rankPermissionsPanel = guildSettingsPanel.Query<VisualElement>("rank-permissions-panel");
            guildChatListen = rankPermissionsPanel.Query<Toggle>("guild-chat-listen");
            guildChatListen.RegisterValueChangedCallback(ToggleGuildChatListen);
            guildChatSpeak = rankPermissionsPanel.Query<Toggle>("guild-chat-talk");
            guildChatSpeak.RegisterValueChangedCallback(ToggleGuildChatSpeak);
            inviteMember = rankPermissionsPanel.Query<Toggle>("guild-invite-member");
            inviteMember.RegisterValueChangedCallback(ToggleGuildInvite);
            removeMember = rankPermissionsPanel.Query<Toggle>("guild-remove-member");
            removeMember.RegisterValueChangedCallback(ToggleGuildRemove);
            promote = rankPermissionsPanel.Query<Toggle>("guild-promote");
            promote.RegisterValueChangedCallback(ToggleGuildPromote);
            demote = rankPermissionsPanel.Query<Toggle>("guild-degrade");
            demote.RegisterValueChangedCallback(ToggleGuildDemote);
            setMOTD = rankPermissionsPanel.Query<Toggle>("set-motd");
            setMOTD.RegisterValueChangedCallback(ToggleGuildSetMotd);
            editPublicNote = rankPermissionsPanel.Query<Toggle>("guild-public-note");
            if (editPublicNote != null) editPublicNote.RegisterValueChangedCallback(ToggleGuildEditPublicNote);
            claimAdd = rankPermissionsPanel.Query<Toggle>("claim-add-objects");
            claimAdd.RegisterValueChangedCallback(ToggleGuildAddClaim);
            claimEdit = rankPermissionsPanel.Query<Toggle>("claim-edit-objects");
            claimEdit.RegisterValueChangedCallback(ToggleGuildEditClaim);
            claimAction = rankPermissionsPanel.Query<Toggle>("claim-interaction");
            claimAction.RegisterValueChangedCallback(ToggleGuildActionClaim);
            levelUp = rankPermissionsPanel.Query<Toggle>("level-up");
            levelUp.RegisterValueChangedCallback(ToggleLevelUp);
            warehouseAdd = rankPermissionsPanel.Query<Toggle>("warehouse-add");
            warehouseAdd.RegisterValueChangedCallback(ToggleWarehouseAdd);
            warehouseGet = rankPermissionsPanel.Query<Toggle>("warehouse-get");
            warehouseGet.RegisterValueChangedCallback(ToggleWarehouseGet);
            rankContainer = guildSettingsPanel.Query<VisualElement>("rank-list-panel");
            // rankContainer =  guildSettingsPanel.Query<VisualElement>("rank-list-panel");
            addRankButton = guildSettingsPanel.Query<Button>("add-rank-button");
            addRankButton.clicked += AddRankClick;
            editMOTDButton = guildSettingsPanel.Query<Button>("edit-motd-button");
            editMOTDButton.clicked += EditMOTDMenuClicked;
            disbandButton = guildSettingsPanel.Query<Button>("disband-guild-button");
            disbandButton.clicked += DisbandClicked;
            rankModDropdown = guildSettingsPanel.Query<UIDropdown>("rank-list-dropdown");
            rankModDropdown.RegisterCallback<ChangeEvent<int>>(RankDropdownClicked);
            rankModDropdown.Screen = uiScreen;
            settingsDropdown = guildSettingsPanel.Query<UIDropdown>("settings-type");
            settingsDropdown.RegisterCallback<ChangeEvent<int>>(ShowRankSettings);
            settingsDropdown.Screen = uiScreen;
            settingsDropdown.Index = 1;
            //Rank settings




            //Resource Window
            resourcesWindow = uiWindow.Query<VisualElement>("resource-panel");
            Button resourceWindowClose = resourcesWindow.Q<Button>("Window-close-button");
            resourceWindowClose.clicked += CloseResources;
            for (int i = 0; i < 20; i++)
            {
                UIAtavismItemDisplay item = resourcesWindow.Q<UIAtavismItemDisplay>("resource-item-req-" + i);
                if (item != null)
                    resourceRequired.Add(item);
            }

            for (int i = 0; i < 20; i++)
            {
                UIAtavismItemDisplay item = resourcesWindow.Q<UIAtavismItemDisplay>("resource-item-collected-" + i);
                if (item != null)
                    resourceCollected.Add(item);
            }

            levelUpButton = uiWindow.Query<Button>("level-up-button");
            //Resource Count Panel
            resourceCountPanel = uiWindow.Query<VisualElement>("resource-count-panel");
            resourceCountItemDisplay = uiWindow.Query<UIAtavismItemDisplay>("resource-item");
            resourceCountText = uiWindow.Query<UITextField>("resource-count");
            resourceItemName = uiWindow.Query<Label>("resource-name");
            resourceAddButton = uiWindow.Query<Button>("resource-add-button");
            resourceAddButton.clicked += SendResource;
            resourceCancelButton = uiWindow.Query<Button>("resource-cancel-button");
            resourceCancelButton.clicked += CancelResource;
            minusButton = uiWindow.Query<Button>("resource-count-minus");
            minusButton.clicked += ReduceMultipleCount;
            plusButton = uiWindow.Query<Button>("resource-count-plus");
            plusButton.clicked += IncreaseMultipleCount;
            Hide();
            //  Debug.LogError("UIAtavismClaimPanel registerUI End");
            return true;
        }

        private void EditMOTDMenuClicked()
        {
            EditMOTDMenuClicked(null);
        }


        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("GUILD_UPDATE", this);
            AtavismEventSystem.RegisterEvent("GUILD_RES_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("GUILD_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("GUILD_RES_UPDATE", this);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().guild.key) ||Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().guild.altKey))&& !ClientAPI.UIHasFocus())
            {
                Toggle();
            }

        }

        public override void Show()
        {
           // AtavismSettings.Instance.OpenWindow(this);
            AtavismUIUtility.BringToFront(gameObject);

            
            if (string.IsNullOrEmpty(AtavismGuild.Instance.GuildName))
            {
                AtavismEventSystem.DispatchEvent("GUILD_CREATE", new string[]{});
               
                if (uiWindowTitle != null && changeWindowTitleToGuildName)
                    uiWindowTitle.text = "";
                if (guildMotd != null)
                    guildMotd.text = "";
                create = true;
            }
            else
            {
                if(!showing)
                 base.Show();
                UpdateGuildDetails();

                invitePopup.HideVisualElement();
                memberPopup.HideVisualElement();
                if (!showing)
                {
                    guildSettingsPanel.HideVisualElement();
                    guildRanksPanel.HideVisualElement();
                    rankPermissionsPanel.HideVisualElement();
                    // guildSettingsPanel.HideVisualElement();
                }
                rankDropDownOpen = false;
                // showing = true;
                create = false;
            }
            // if (panel != null)
                // panel.SetActive(true);
            //   AtavismUIUtility.BringToFront(gameObject);
        }

        public void Hide()
        {
            base.Hide();
           // AtavismSettings.Instance.CloseWindow(this);
            // if (createPopup != null)
            // {
            //     createPopup.HideVisualElement();
            // }
            if (guildSettingsPanel != null)
                guildSettingsPanel.HideVisualElement();
            if (guildRanksPanel != null)
                guildRanksPanel.HideVisualElement();
            if (rankPermissionsPanel != null)
                rankPermissionsPanel.HideVisualElement();
            // if (guildSettingsPanel != null)
                // guildSettingsPanel.HideVisualElement();
            if (invitePopup != null)
                invitePopup.HideVisualElement();
            // if (panel != null)
                // panel.SetActive(false);
          
            HideEditMOTDPopup();
            CloseResources();
        }


        public void OnEvent(AtavismEventData eData)
        {
           // Debug.LogError("OnEvent " + eData.eventType);
            if (eData.eventType == "GUILD_UPDATE")
            {
                if (showing || create)
                {
                    UpdateGuildDetails();

                    Show();
                }
            }
            else if (eData.eventType == "GUILD_RES_UPDATE")
            {
                UpdateShowResources();
            }

        }

        public void UpdateGuildDetails()
        {
            int memberRank = -1;
            AtavismGuildMember member = AtavismGuild.Instance.GetGuildMemberByOid(OID.fromLong(ClientAPI.GetPlayerOid()));
            if (member != null)
                memberRank = member.rank;

            createPopup.HideVisualElement();
            if (AtavismGuild.Instance.GuildName == null || AtavismGuild.Instance.GuildName == "")
            {
                Hide();
                return;
            }
            if (uiWindowTitle != null && changeWindowTitleToGuildName)
                uiWindowTitle.text = AtavismGuild.Instance.GuildName;
            if (guildMotd != null)
                guildMotd.text = AtavismGuild.Instance.Motd;

            if (level != null)
                level.text = AtavismGuild.Instance.Level;

            if (levelProgressSlider != null)
            {
                int req = 0;
                foreach (int v in AtavismGuild.Instance.RequiredItems.Values)
                    req += v;
                int count = 0;
                foreach (int v in AtavismGuild.Instance.Items.Values)
                    count += v;

                levelProgressSlider.highValue = req;
                levelProgressSlider.lowValue = 0;
                levelProgressSlider.value = count;
            }


            // Delete the old list
            // ClearAllCells();
            //
            Refresh();


            if (memberNumText != null)
            {
                if (AtavismGuild.Instance.MemberNum > 0)
                    memberNumText.text = AtavismGuild.Instance.Members.Count + "/" + AtavismGuild.Instance.MemberNum;
                else
                    memberNumText.text = AtavismGuild.Instance.Members.Count.ToString();
            }

            // if (rankModDropdown != null)
            // rankModDropdown.ClearOptions();
            //
            
            rankModList.Clear();


            List<string> rankNameList = new List<string>();
            // Reset ranks
            foreach (var _rank in AtavismGuild.Instance.Ranks)
            {
                rankModList.Add(_rank.rankName);
            }
            
            
            // for (int i = 0; i < rankButtons.Count; i++)
            // {
            //     if (i >= AtavismGuild.Instance.Ranks.Count)
            //     {
            //         if (rankButtons[i] != null)
            //             rankButtons[i].HideVisualElement();
            //     }
            //     else
            //     {
            //         if (rankButtons[i] != null)
            //             rankButtons[i].ShowVisualElement();
            //         if (rankButtons[i] != null)
            //             rankButtons[i].text = AtavismGuild.Instance.Ranks[i].rankName;
            //         rankModList.Add(AtavismGuild.Instance.Ranks[i].rankName);
            //     
            //         
            //        
            //         
            //         // if (TMPRankModDropdown != null)
            //         //     TMPRankModDropdown.options.Add(new TMP_Dropdown.OptionData(AtavismGuild.Instance.Ranks[i].rankName));
            //     }
            // }

            if (rankModDropdown != null)
            {
                rankModDropdown.Options(rankModList);
                rankModDropdown.Index = selectedRank;

            }

            // if (rankModDropdown != null)
            // {
            //     //TODO
            //     rankModDropdown.choices = rankModList;
            //     if (rankModList.Count > selectedRank)
            //         rankModDropdown.value = rankModList[selectedRank];
            //     else
            //         rankModDropdown.value = rankModList[0];
            //     
            //     
            //     
            //     List<string> ranks = new List<string>()
            //     for (int i = 0; i < AtavismGuild.Instance.Ranks.Count; i++)
            //     {
            //         // rankModDropdown.options.Add(new Dropdown.OptionData(AtavismGuild.Instance.Ranks[i].rankName));
            //         ranks = AtavismGuild.Instance.Ranks[i].rankName;
            //     }
            //     // if (rankModDropdown.options.Count > selectedRank)
            //         // rankModDropdown.captionText.text = AtavismGuild.Instance.Ranks[selectedRank].rankName;
            //     // else if (rankModDropdown.options.Count > 0)
            //         // rankModDropdown.captionText.text = AtavismGuild.Instance.Ranks[0].rankName;
            //     
            //     // if (rankModDropdown.options.Count > selectedRank)
            //         rankModDropdown.Index = selectedRank;
            //     // else
            //         // rankModDropdown.value = 0;
            //
            // }
            //

            if (memberRank == 0 && selectedRank != 0)
            {
                disbandButton.SetEnabled(true);
                if (leaveButton != null)
                    leaveButton.SetEnabled(false);
                addRankButton.SetEnabled(true);
                levelUp.SetEnabled(true);
                warehouseAdd.SetEnabled(true);
                warehouseGet.SetEnabled(true);
                guildChatListen.SetEnabled(true);
                guildChatSpeak.SetEnabled(true);
                inviteMember.SetEnabled(true);
                removeMember.SetEnabled(true);
                promote.SetEnabled(true);
                demote.SetEnabled(true);
                setMOTD.SetEnabled(true);
                if(editPublicNote!=null)  editPublicNote.SetEnabled(true);
                claimAdd.SetEnabled(true);
                claimEdit.SetEnabled(true);
                claimAction.SetEnabled(true);
                editMOTDButton.SetEnabled(true);

            }
            else
            {
                disbandButton.SetEnabled(false);
                if (leaveButton != null)
                    leaveButton.SetEnabled(true);
                addRankButton.SetEnabled(false);
                levelUp.SetEnabled(false);
                warehouseAdd.SetEnabled(false);
                warehouseGet.SetEnabled(false);
                guildChatListen.SetEnabled(false);
                guildChatSpeak.SetEnabled(false);
                inviteMember.SetEnabled(false);
                removeMember.SetEnabled(false);
                promote.SetEnabled(false);
                demote.SetEnabled(false);
                setMOTD.SetEnabled(false);
                if(editPublicNote!=null) editPublicNote.SetEnabled(false);
                claimAdd.SetEnabled(false);
                claimEdit.SetEnabled(false);
                claimAction.SetEnabled(false);
                editMOTDButton.SetEnabled(false);

            }
            if (memberRank == 0)
            {
                disbandButton.SetEnabled(true);
                if (leaveButton != null)
                    leaveButton.SetEnabled(false);
            }
            AtavismGuildRank rank = AtavismGuild.Instance.Ranks[memberRank];

            if (rank.permissions.Contains(AtavismGuildRankPermission.setmotd))
            {
                editMOTDButton.SetEnabled(true);
            }

            if (guildRanks.Count < AtavismGuild.Instance.Ranks.Count)
            {
                guildRanks.Clear();
                rankList.Clear();
                foreach (var _rank in AtavismGuild.Instance.Ranks)
                {
                    // Instantiate a controller for the data
                    UIAtavismGuildRank newListEntryLogic = new UIAtavismGuildRank();
                    // Instantiate the UXML template for the entry
                    var newListEntry = rankEntryTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = newListEntryLogic;
                    // Initialize the controller script
                    newListEntryLogic.SetVisualElement(newListEntry);
                    // slots.Add(newListEntryLogic);
                    rankList.Add(newListEntry);
                    guildRanks.Add(newListEntryLogic);
                    // Return the root of the instantiated visual tree
                }
            }
            
            for (int i = 0; i < guildRanks.Count; i++)
            {
                if (i >= AtavismGuild.Instance.Ranks.Count)
                {
                    guildRanks[i].Hide();
                }
                else
                {
                    guildRanks[i].Show();
                    if (i == 0)
                    {

                    }
                    guildRanks[i].setRankId = AtavismGuild.Instance.Ranks[i].rankLevel;
                    if (guildRanks[i].Input != null)
                        guildRanks[i].Input.SetValueWithoutNotify(AtavismGuild.Instance.Ranks[i].rankName);
#if AT_I2LOC_PRESET
                 if (guildRanks[i].RankText != null) guildRanks[i].RankText.text = I2.Loc.LocalizationManager.GetTranslation("Rank") + " " + (i + 1) + ":";
#else
                    if (guildRanks[i].RankText != null)
                        guildRanks[i].RankText.text = "Rank " + (i + 1) + ":";
#endif
                    if (memberRank == 0 && i > AtavismGuild.Instance.Ranks.Count - 2 )
                                        {
                                            if (guildRanks[i].DeleteButton != null)
                                                guildRanks[i].DeleteButton.SetEnabled(true);
                                            if (guildRanks[i].Input != null)
                                                guildRanks[i].Input.SetEnabled(true);
                                        }
                                        else if (memberRank == 0 && i > 0 )
                                                                {
                                                                    if (guildRanks[i].DeleteButton != null)
                                                                        guildRanks[i].DeleteButton.SetEnabled(false);
                                                                    if (guildRanks[i].Input != null)
                                                                        guildRanks[i].Input.SetEnabled(true);
                                                                }
                                                                else
                    {
                        if (guildRanks[i].DeleteButton != null)
                            guildRanks[i].DeleteButton.SetEnabled(false);
                        if (guildRanks[i].Input != null)
                            guildRanks[i].Input.SetEnabled(false);
                    }
                
                }
            }
            if (memberRank == 0 && maxNumberOfRanks > AtavismGuild.Instance.Ranks.Count)
                addRankButton.SetEnabled(true);
            else
                addRankButton.SetEnabled(false);

        }

        private void Refresh()
        {
           memberList.Clear();
           foreach (var member in AtavismGuild.Instance.Members)
           {
               // Instantiate a controller for the data
               UIAtavismGuildMemberEntry newListEntryLogic = new UIAtavismGuildMemberEntry();
               // Instantiate the UXML template for the entry
               var newListEntry = memberEntryTemplate.Instantiate();
               // Assign the controller script to the visual element
               newListEntry.userData = newListEntryLogic;
               // Initialize the controller script
               newListEntryLogic.SetVisualElement(newListEntry);
               // slots.Add(newListEntryLogic);
               newListEntryLogic.SetGuildMemberDetails(member, this);
               memberList.Add(newListEntry);
               // Return the root of the instantiated visual tree
           }
            
            
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HideMemberPopup();
        }

        public void CreateGuildClicked()
        {
            if (guildNameField != null)
                if (guildNameField.text != "")
                {
                    AtavismGuild.Instance.CreateGuild(guildNameField.text);
                    createPopup.HideVisualElement();
                    guildNameField.value = "";
                }
                else
                {

                }
        }

        public void AddMemberClicked()
        {
            HideMemberPopup();
            // AtavismUIUtility.BringToFront(invitePopup.gameObject);
            if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetTargetObject().CheckBooleanProperty("combat.userflag"))
            {
                AtavismGuild.Instance.SendGuildCommand("invite", OID.fromLong(ClientAPI.GetTargetOid()), null);
                return;
            }
            else
            {
                invitePopup.ShowVisualElement();
                if (inviteNameField != null)
                {
                    inviteNameField.value = "";
                    // EventSystem.current.SetSelectedGameObject(inviteNameField.gameObject, null);
                }
                else
                {
                    AtavismEventSystem.DispatchEvent("GUILD_ADD_PLAYER", new string[]{});
                }
            }
        }

        public void AddMemberMenuClicked()
        {
            HideMemberPopup();
            if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetTargetObject().CheckBooleanProperty("combat.userflag"))
            {
                AtavismGuild.Instance.SendGuildCommand("invite", OID.fromLong(ClientAPI.GetTargetOid()), null);
                return;
            }
            else
            {
                // AtavismUIUtility.BringToFront(invitePopup.gameObject);
                invitePopup.ShowVisualElement();
                // invitePopup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                if (inviteNameField != null)
                {
                    inviteNameField.value = "";
                    // EventSystem.current.SetSelectedGameObject(inviteNameField.gameObject, null);
                }
            }
        }

        public void AddMemberByNameClicked()
        {
            if (inviteNameField != null)
                if (inviteNameField.text != "")
                {
                    AtavismGuild.Instance.SendGuildCommand("invite", null, inviteNameField.text);
                    invitePopup.HideVisualElement();
                }
        }

        public void ShowResources()
        {
            if (resourcesWindow != null)
            {
                resourcesWindow.ShowVisualElement();
            }
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.SetUIActivatableClickedOverride(PlaceItem);
            UpdateShowResources();

        }

        public void CloseResources()
        {
            if (resourcesWindow != null)
            {
                resourcesWindow.HideVisualElement();
            }
            if (resourceCountPanel != null)
                resourceCountPanel.HideVisualElement();
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(PlaceItem);
        }


        public bool ShowResourceCountPanel(AtavismInventoryItem resourceItem)
        {
            this.resourceItem = resourceItem;
            if (resourceCountItemDisplay != null)
                resourceCountItemDisplay.SetItemData(resourceItem);
            if (resourceCountPanel != null)
            {
                resourceCountPanel.ShowVisualElement();
                resourceItemCount = 1;
                if (resourceCountText != null)
                {
                    resourceCountText.value = resourceItemCount.ToString();
                }
                if (resourceItemName != null && resourceItem != null)
                {
#if AT_I2LOC_PRESET
                    resourceItemName.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Items/" + resourceItem.name)) ? resourceItem.name : I2.Loc.LocalizationManager.GetTranslation("Items/" + resourceItem.name);
#else
                    resourceItemName.text = resourceItem.name;
#endif
                }
                return true;
            }
            return false;
        }

        public void ReduceMultipleCount()
        {
            resourceItemCount--;
            if (resourceItemCount < 2)
            {
                resourceItemCount = 1;
            }
            if (resourceCountText != null)
            {
                resourceCountText.value = resourceItemCount.ToString();
            }
        }

        public void IncreaseMultipleCount()
        {
            resourceItemCount++;
            int count = Inventory.Instance.GetCountOfItem(resourceItem.templateId);
            if (resourceItemCount > count)
                resourceItemCount = count;
            int reqCount = AtavismGuild.Instance.RequiredItems[resourceItem.templateId];
            int itemCount = 0;
            if (AtavismGuild.Instance.Items.ContainsKey(resourceItem.templateId))
                itemCount = AtavismGuild.Instance.Items[resourceItem.templateId];
            if (resourceItemCount > reqCount - itemCount)
                resourceItemCount = reqCount - itemCount;

            if (resourceCountText != null)
            {
                resourceCountText.value = resourceItemCount.ToString();
            }

        }

        public void UpdateResourceCount()
        {
            if (resourceCountText != null)
            {
                if (resourceCountText.value != "")
                    resourceItemCount = int.Parse(resourceCountText.value);
                else
                    resourceItemCount = 0;
            }
        }


        public void UpdateShowResources()
        {
            if (resourceRequired != null)
            {
                int req = 0;
                foreach (int key in AtavismGuild.Instance.RequiredItems.Keys)
                {
                    //   Debug.LogError("Guild Req item " + key + " " + AtavismGuild.Instance.RequiredItems[key]);
                    if (key > 0)
                    {
                        AtavismInventoryItem item = AtavismPrefabManager.Instance.LoadItem(key);
                        item.Count = AtavismGuild.Instance.RequiredItems[key];
                        if (resourceRequired.Count > req)
                        {
                            if (resourceRequired[req] != null)
                            {
                                //resourceRequired[req].Show();
                                resourceRequired[req].SetItemData(item);
                            }
                        }
                        req++;
                    }
                }
                if (resourceRequired.Count > req)
                {
                    for (int i = req; i < resourceRequired.Count; i++)
                    {
                        //  Debug.LogError("Guild Req item reset slot " + i );
                        if (resourceRequired[i] != null)
                        {
                            resourceRequired[i].Reset();
                            // resourceRequired[i].Hide();
                        }
                    }
                }
            }

            if (resourceCollected != null)
            {
                int req = 0;
                foreach (int key in AtavismGuild.Instance.Items.Keys)
                {
                    //  Debug.LogError("Guild item " + key + " " + AtavismGuild.Instance.Items[key]);
                    if (key > 0)
                    {
                        AtavismInventoryItem item = AtavismPrefabManager.Instance.LoadItem(key);
                        item.Count = AtavismGuild.Instance.Items[key];
                        if (resourceCollected.Count > req)
                        {
                            if (resourceCollected[req] != null)
                            {
                                // resourceCollected[req].Show();
                                resourceCollected[req].SetItemData(item);
                            }
                        }
                        req++;
                    }
                }
                if (resourceCollected.Count > req)
                {
                    for (int i = req; i < resourceCollected.Count; i++)
                    {
                        if (resourceCollected[i] != null)
                        {
                            resourceCollected[i].Reset();
                            // resourceCollected[i].Hide();
                        }
                    }
                }
            }
            if (levelUpButton != null)
            {
                bool allresources = false;
                foreach (int key in AtavismGuild.Instance.RequiredItems.Keys)
                {
                    if (AtavismGuild.Instance.Items.ContainsKey(key))
                    {
                        if (AtavismGuild.Instance.RequiredItems[key] == AtavismGuild.Instance.Items[key])
                        {
                            allresources = true;
                        }
                        else
                        {
                            allresources = false;
                            break;
                        }
                    }
                    else
                    {
                        allresources = false;
                        break;
                    }
                }
                if (allresources)
                {
                    int memberRank = -1;
                    AtavismGuildMember member = AtavismGuild.Instance.GetGuildMemberByOid(OID.fromLong(ClientAPI.GetPlayerOid()));
                    if (member != null)
                        memberRank = member.rank;
                    AtavismGuildRank rank = AtavismGuild.Instance.Ranks[memberRank];

                    if (rank.permissions.Contains(AtavismGuildRankPermission.levelUp))
                        levelUpButton.SetEnabled(true);
                    else
                        levelUpButton.SetEnabled(false);
                }
                else
                {
                    levelUpButton.SetEnabled(false);
                }
            }
            if (level != null)
                level.text = AtavismGuild.Instance.Level;
            if (levelProgressSlider != null)
            {
                int req = 0;
                foreach (int v in AtavismGuild.Instance.RequiredItems.Values)
                    req += v;
                int count = 0;
                foreach (int v in AtavismGuild.Instance.Items.Values)
                    count += v;

                levelProgressSlider.highValue = req;
                levelProgressSlider.lowValue = 0;
                levelProgressSlider.value = count;
            }
        }



        // public override int NumberOfCells()
        // {
        //     int numCells = AtavismGuild.Instance.Members.Count;
        //     return numCells;
        // }
        //
        // public override void UpdateCell(int index, UGUIGuildMemberEntry cell)
        // {
        //     cell.SetGuildMemberDetails(AtavismGuild.Instance.Members[index], this);
        // }

        #region Member Popup
        public void ShowMemberPopup(UIAtavismGuildMemberEntry selectedMemberEntry, AtavismGuildMember member)
        {
            // Debug.LogError("ShowMemberPopup");
            selectedMember = member;
            memberPopup.ShowVisualElement();
            
            float width = uiWindow.resolvedStyle.width;
            float height = uiWindow.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            
            memberPopup.style.left = scaledMousePosition.x - uiWindow.resolvedStyle.left; //new Vector2(popupPosition.x, memberEntryTransform.anchoredPosition.y);
            memberPopup.style.top  = canvasHeight - scaledMousePosition.y - uiWindow.resolvedStyle.top; //new Vector2(popupPosition.x, memberEntryTransform.anchoredPosition.y);
        }

        public void HideMemberPopup()
        {
            memberPopup.HideVisualElement();
        }
        public void HideInvitePopup()
        {
            invitePopup.HideVisualElement();
        }

        public void WhisperMemberClicked()
        {
        }

        public void PromoteMemberClicked()
        {
            if (selectedMember.rank == 1)
            {
#if AT_I2LOC_PRESET
       UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want promote") +" "+selectedMember.name+" "+ I2.Loc.LocalizationManager.GetTranslation("to Guild Master") +"?", null, masterGuild);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want promote " + selectedMember.name + " to Guild Master?", null, masterGuild);
#endif
            }
            else
            {
                AtavismGuild.Instance.SendGuildCommand("promote", selectedMember.oid, null);
                HideMemberPopup();
            }
        }

        private void masterGuild(object confirmObject, bool accepted)
        {
            if (accepted)
            {
                AtavismGuild.Instance.SendGuildCommand("promote", selectedMember.oid, null);
                HideMemberPopup();
            }
        }


        public void DemoteMemberClicked()
        {
            AtavismGuild.Instance.SendGuildCommand("demote", selectedMember.oid, null);
            HideMemberPopup();
        }

        public void KickMemberClicked()
        {

#if AT_I2LOC_PRESET
       UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want kick") +" "+selectedMember.name+" "+ I2.Loc.LocalizationManager.GetTranslation("from Guild") +"?", null, kickMember);
#else
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want kick " + selectedMember.name + " from Guild ?", null, kickMember);
#endif
            HideMemberPopup();
        }

        private void kickMember(object confirmObject, bool accepted)
        {
            if (accepted)
            {
                AtavismGuild.Instance.SendGuildCommand("kick", selectedMember.oid, null);

            }
        }


        public void DisbandClicked()
        {
#if AT_I2LOC_PRESET
       UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want disband guild") + "?", null, disbandGuild);
#else
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want disband guild?", null, disbandGuild);
#endif
        }

        private void disbandGuild(object confirmObject, bool accepted)
        {
            if (accepted)
            {
                AtavismGuild.Instance.SendGuildCommand("disband", null, null);
                Hide();
            }
        }
        public void LeaveClicked()
        {
#if AT_I2LOC_PRESET
       UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want leave guild") + "?", null, leaveGuild);
#else
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want leave guild?", null, leaveGuild);
#endif
        }

        private void leaveGuild(object confirmObject, bool accepted)
        {
            if (accepted)
            {
                AtavismGuild.Instance.SendGuildCommand("quit", null, null);
                Hide();
            }
        }
        #endregion Member Popup



        public void SendResource()
        {
            ////    if (accepted)
            // {
            if (resourceItem != null)
                AtavismGuild.Instance.SendGuildResource(resourceItem.templateId, resourceItemCount);
            if (resourceCountPanel != null)
                resourceCountPanel.HideVisualElement();
            // }
        }

        public void CancelResource()
        {
            resourceItem = null;
            resourceItemCount = 0;
            if (resourceCountPanel != null)
                resourceCountPanel.HideVisualElement();

        }

        private void PlaceItem(UIAtavismActivatable activatable)
        {
            //  Debug.LogError("PlaceSocketingItem " + activatable.Link);

            if (activatable.Link != null)
            {
                return;
            }
            AtavismInventoryItem item = (AtavismInventoryItem)activatable.ActivatableObject;
            if (item != null)
            {
                if (AtavismGuild.Instance.RequiredItems.ContainsKey(item.templateId))
                {
                    int reqCount = AtavismGuild.Instance.RequiredItems[item.templateId];
                    int itemCount = 0;
                    if (AtavismGuild.Instance.Items.ContainsKey(item.templateId))
                        itemCount = AtavismGuild.Instance.Items[item.templateId];
                    if (reqCount - itemCount > 0)
                    {
                        ShowResourceCountPanel(item);
                    }
                    else
                    {
                        activatable.PreventDiscard();
                        //     Debug.LogError("Wrong Item");
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                    args[0] = I2.Loc.LocalizationManager.GetTranslation("Items/"+item.name)+" "+I2.Loc.LocalizationManager.GetTranslation("is no longer required");
#else
                        args[0] = item.name+" is no longer required";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                    }

                }
                else
                {
                    activatable.PreventDiscard();

                    //     Debug.LogError("Wrong Item");
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



        #region Rank Popup
        public void ToggleRankPopup()
        {
            if (guildSettingsPanel.style.display == DisplayStyle.Flex)
            {
                guildSettingsPanel.HideVisualElement();
            }
            else
            {
                guildSettingsPanel.ShowVisualElement();
                if (settingsDropdown!=null)
                    settingsDropdown.Index = 1;
                
                // if (rankDropDown!=null)
                //     rankDropDown.gameObject.GetComponentInChildren<Text>().text = AtavismGuild.Instance.Ranks[selectedRank].rankName;
                UpdateRankDisplay();
            }
        }

        void UpdateRankDisplay()
        {
            interactionDelay = Time.time + 0.5f;
            int memberRank = -1;
            AtavismGuildMember member = AtavismGuild.Instance.GetGuildMemberByOid(OID.fromLong(ClientAPI.GetPlayerOid()));
            if (member != null)
                memberRank = member.rank;

            levelUp.value = false;
            warehouseAdd.value = false;
            warehouseGet.value = false;
            guildChatListen.value = false;
            guildChatSpeak.value = false;
            inviteMember.value = false;
            removeMember.value = false;
            promote.value = false;
            demote.value = false;
            setMOTD.value = false;
            if(editPublicNote!=null) editPublicNote.value = false;
            claimAdd.value = false;
            claimEdit.value = false;
            claimAction.value = false;
            claimAction.SetEnabled(false);
//Debug.LogError("Guild Ranks "+AtavismGuild.Instance.Ranks+" selectedRank="+selectedRank);
            AtavismGuildRank rank = AtavismGuild.Instance.Ranks[selectedRank];
         /*   string r = "";
            foreach (AtavismGuildRankPermission a in rank.permissions)
            {
                r += a + " | ";
            }

            Debug.LogError("GUILD rank perm="+r);*/
            if (rank.permissions.Contains(AtavismGuildRankPermission.chat))
            {
                guildChatListen.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.chat))
            {
                guildChatSpeak.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.invite))
            {
                inviteMember.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.kick))
            {
                removeMember.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.promote))
            {
                promote.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.demote))
            {
                demote.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.setmotd))
            {
                setMOTD.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.editPublic))
            {
                if(editPublicNote!=null) editPublicNote.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.claimAdd))
            {
                claimAdd.value = true;
                claimAction.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.claimEdit))
            {
                claimEdit.value = true;
                claimAdd.value = true;
                claimAction.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.claimAction))
            {
                claimAction.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.levelUp))
            {
                levelUp.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.whAdd))
            {
                warehouseAdd.value = true;
            }
            if (rank.permissions.Contains(AtavismGuildRankPermission.whGet))
            {
                warehouseGet.value = true;
            }
            if (claimAdd.value || claimEdit.value)
                claimAction.value = true;
            else
                claimAction.value = false;
            if (memberRank == 0 && selectedRank != 0)
            {
                levelUp.SetEnabled(true);
                warehouseAdd.SetEnabled(true);
                warehouseGet.SetEnabled(true);
                guildChatListen.SetEnabled(true);
                guildChatSpeak.SetEnabled(true);
                inviteMember.SetEnabled(true);
                removeMember.SetEnabled(true);
                promote.SetEnabled(true);
                demote.SetEnabled(true);
                setMOTD.SetEnabled(true);
                if(editPublicNote!=null) editPublicNote.SetEnabled(true);
                claimAdd.SetEnabled(true);
                claimEdit.SetEnabled(true);
                claimAction.SetEnabled(true);
            }
            else
            {
                levelUp.SetEnabled(false);
                warehouseAdd.SetEnabled(false);
                warehouseGet.SetEnabled(false);
                guildChatListen.SetEnabled(false);
                guildChatSpeak.SetEnabled(false);
                inviteMember.SetEnabled(false);
                removeMember.SetEnabled(false);
                promote.SetEnabled(false);
                demote.SetEnabled(false);
                setMOTD.SetEnabled(false);
                if(editPublicNote!=null)  editPublicNote.SetEnabled(false);
                claimAdd.SetEnabled(false);
                claimEdit.SetEnabled(false);
                claimAction.SetEnabled(false);
            }
        }

        // public void SettingsDropdownClicked()
        // {
        //     settingsDropDownOpen = !settingsDropDownOpen;
        // }

        public void ShowRankSettings(ChangeEvent<int> evt)
        {
            if (settingsDropdown != null)
            {
                if (settingsDropdown.Index == 0)
                {
                    guildRanksPanel.ShowVisualElement();
                    rankPermissionsPanel.HideVisualElement();
                    addRankButton.visible = true;
                }
                else if (settingsDropdown.Index == 1)
                {
                    guildRanksPanel.HideVisualElement();
                    rankPermissionsPanel.ShowVisualElement();
                    addRankButton.visible = false;
                }
                else
                {
                    guildRanksPanel.HideVisualElement();
                    rankPermissionsPanel.HideVisualElement();
                    addRankButton.visible = false;
                }
            }
        }

        // public void ShowGuildRanks(Text buttonText)
        // {
        //     settingsDropDown.gameObject.GetComponentInChildren<Text>().text = buttonText.text;
        //     settingsDropDownOpen = false;
        //     guildRanksPanel.gameObject.SetActive(true);
        //     rankPermissionsPanel.HideVisualElement();
        //     guildSettingsPanel.gameObject.SetActive(false);
        // }
        //
        // public void ShowRankPermissions(Text buttonText)
        // {
        //     settingsDropDown.gameObject.GetComponentInChildren<Text>().text = buttonText.text;
        //     settingsDropDownOpen = false;
        //     guildRanksPanel.gameObject.SetActive(false);
        //     rankPermissionsPanel.ShowVisualElement();
        //     guildSettingsPanel.gameObject.SetActive(false);
        // }
        //
        // public void ShowSettingsOptions(Text buttonText)
        // {
        //     settingsDropDown.gameObject.GetComponentInChildren<Text>().text = buttonText.text;
        //     settingsDropDownOpen = false;
        //     guildRanksPanel.gameObject.SetActive(false);
        //     rankPermissionsPanel.HideVisualElement();
        //     guildSettingsPanel.gameObject.SetActive(true);
        // }
        public void RankDropdownClicked(ChangeEvent<int> evt)
        {
            rankDropDownOpen = !rankDropDownOpen;
            if (rankModDropdown!=null)
            {
                 selectedRank = rankModDropdown.Index;
                UpdateRankDisplay();
            }
        }

        public void RankDropdownButtonClicked(GameObject buttonClicked)
        {
            // for (int i = 0; i < rankButtons.Count; i++)
            // {
            //     if (buttonClicked.name == rankButtons[i].name)
            //     {
            //         selectedRank = i;
            //         break;
            //     }
            // }
            if (rankDropDown!=null)
            {
                // rankDropDown.gameObject.GetComponentInChildren<Text>().text = AtavismGuild.Instance.Ranks[selectedRank].rankName;
                rankDropDownOpen = false;
            }
            UpdateRankDisplay();
        }

        public void LevelUpClick()
        {

            AtavismGuild.Instance.SendGuildCommand("levelUp", null, "");
        }

        public void ToggleLevelUp(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";levelUp;" + (levelUp.value ? "1" : "0"));
        }

        public void ToggleWarehouseAdd(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";whAdd;" + (warehouseAdd.value ? "1" : "0"));
        }

        public void ToggleWarehouseGet(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";whGet;" + (warehouseGet.value ? "1" : "0"));
        }

        public void ToggleGuildChatListen(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";chat;" + (guildChatListen.value ? "1" : "0"));
        }

        public void ToggleGuildChatSpeak(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";chat;" + (guildChatSpeak.value ? "1" : "0"));
        }

        public void ToggleGuildInvite(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";invite;" + (inviteMember.value ? "1" : "0"));
        }

        public void ToggleGuildRemove(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";kick;" + (removeMember.value ? "1" : "0"));
        }

        public void ToggleGuildPromote(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";promote;" + (promote.value ? "1" : "0"));
        }

        public void ToggleGuildDemote(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";demote;" + (demote.value ? "1" : "0"));
        }

        public void ToggleGuildSetMotd(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";setmotd;" + (setMOTD.value ? "1" : "0"));
        }

        public void ToggleGuildEditPublicNote(ChangeEvent<bool> evt)
        {
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";editpublic;" + (editPublicNote.value ? "1" : "0"));
        }

        public void ToggleGuildEditClaim(ChangeEvent<bool> evt)
        {
            if (Time.time < interactionDelay)
                return;
         /*   if (claimEdit.isOn)
            {
                claimAdd.isOn = true;
                claimAction.isOn = true;
            }*/
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";claimEdit;" + (claimEdit.value ? "1" : "0"));
        }
        public void ToggleGuildAddClaim(ChangeEvent<bool> evt)
        {
            if (Time.time < interactionDelay)
                return;
         /*   if (claimAdd.isOn)
            {
                claimAction.isOn = true;
            }
            if (claimEdit.isOn)
                claimAdd.isOn = true;*/
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";claimAdd;" + (claimAdd.value ? "1" : "0"));
        }
        public void ToggleGuildActionClaim(ChangeEvent<bool> evt)
        {
            if (claimAdd.value || claimEdit.value)
                claimAction.value = true;
            if (Time.time > interactionDelay)
                AtavismGuild.Instance.SendGuildCommand("editRank", null, AtavismGuild.Instance.Ranks[selectedRank].rankLevel + ";claimAction;" + (claimAction.value ? "1" : "0"));
        }

        public void AddRankClick()
        {
            Debug.LogError("AddRankClick");
            if (maxNumberOfRanks > AtavismGuild.Instance.Ranks.Count)
                AtavismGuild.Instance.SendGuildCommand("addRank", null, "NewRank");
        }
        
        public void EditMOTDClick()
        {
            if (editMOTDField != null)
                AtavismGuild.Instance.SendGuildCommand("setmotd", null, editMOTDField.value);
            HideEditMOTDPopup();

        }
        public void EditMOTDMenuClicked(MouseUpEvent evt)
        {
          //  Debug.LogError("editMOTDMenuClicked");
            HideMemberPopup();
            if (editMOTD != null)
            {
                
                editMOTD.ShowVisualElement();

                // editMOTD.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                editMOTDField.value = AtavismGuild.Instance.Motd;
                
                // AtavismUIUtility.BringToFront(editMOTD.gameObject);
                // EventSystem.current.SetSelectedGameObject(editMOTDField.gameObject, null);
            }
            else
            {
                AtavismEventSystem.DispatchEvent("GUILD_EDIT_MOTD", new string[]{});
            }
            
            
        }
        public void HideEditMOTDPopup()
        {
            if (editMOTD != null)
            {
                editMOTD.HideVisualElement();
            }
        }

        #endregion Rank Popup
    }
}