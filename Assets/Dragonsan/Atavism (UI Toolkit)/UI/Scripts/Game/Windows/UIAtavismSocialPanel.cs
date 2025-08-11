using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismSocialPanel : UIAtavismWindowBase
    {

        [SerializeField] VisualTreeAsset prefab;
        [SerializeField] Button whisperButton;
        [SerializeField] Button groupButton;
        [SerializeField] Button guildButton;
        [SerializeField] Button privetInstanceButton;
        [SerializeField] Button deleteButton;
        public VisualElement memberPopup;
        AtavismSocialMember selectedMember = null;
        float interactionDelay;
        [AtavismSeparator("Friends List")]
        [SerializeField] VisualElement friendsPanel;
        [SerializeField] List<UIAtavismSocialMemberEntry> friendList = new List<UIAtavismSocialMemberEntry>();
        public VisualElement subWindowAddFriend;
        public ListView friendListGrid;
        public UITextField inviteNameField;
        bool _showFriendList = false;

        [AtavismSeparator("Block List")]
        [SerializeField] List<UIAtavismSocialMemberEntry> blockList = new List<UIAtavismSocialMemberEntry>();
        [SerializeField] VisualElement blockListPanel;
        public VisualElement subWindowAddBlock;
        public ListView blockListGrid;
        public UITextField blockListNameField;
        private Button addFrienButton;
        private Button addBlockButton;
        private Button addFrienButton2;
        private Button addBlockButton2;

        // VisualElement addFriendWindow;
        // VisualElement addBlockWindow;
        private Button blockCloseButton;
        private Button friendCloseButton;

        private UIButtonToggleGroup menu;

        private bool showSubWindowAddFriend;

        private bool showSubWindowAddBlock;
        // Use this for initialization

        protected override bool registerUI()
        { 
            if (!base.registerUI())
                return false;
            
            menu= uiWindow.Query<UIButtonToggleGroup>("menu");
            menu.OnItemIndexChanged += TopMenuChange;
            whisperButton= uiWindow.Query<Button>("whisper-button");
            whisperButton.clicked += WhisperOptionClicked;
            groupButton= uiWindow.Query<Button>("group-button");;
            groupButton.clicked += InviteGroupOptionClicked;
             guildButton= uiWindow.Query<Button>("guild-button");;
             guildButton.clicked += InviteGuildOptionClicked;
             privetInstanceButton= uiWindow.Query<Button>("priv-button");;
             privetInstanceButton.clicked += InvitePrivateInstane;
             deleteButton= uiWindow.Query<Button>("delete-button");;
             deleteButton.clicked += DeleteOptionClicked;
            memberPopup= uiWindow.Query<VisualElement>("menu-popup");
            
            friendsPanel= uiWindow.Query<VisualElement>("friend-panel");
            friendListGrid= uiWindow.Query<ListView>("friend-list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = friendListGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            friendListGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismSocialMemberEntry newListEntryLogic = new UIAtavismSocialMemberEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = prefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // slots.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            friendListGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismSocialMemberEntry).SetSocialMemberDetails(AtavismSocial.Instance.Friends[index],this);
            };
            
            blockListPanel = uiWindow.Query<VisualElement>("block-panel");
            blockListGrid= uiWindow.Query<ListView>("block-list");
#if UNITY_6000_0_OR_NEWER    
            scrollView = blockListGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            blockListGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismSocialMemberEntry newListEntryLogic = new UIAtavismSocialMemberEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = prefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // slots.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            blockListGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismSocialMemberEntry).SetBlockSocialMemberDetails(AtavismSocial.Instance.Banneds[index],this);
            };
            
            
            
            
            
            addFrienButton= uiWindow.Query<Button>("add-friend-button");
            addFrienButton.clicked += AddMemberMenuClicked;
            addBlockButton= uiWindow.Query<Button>("add-block-button");;
            addBlockButton.clicked += AddBlockListMenuClicked;
            
            subWindowAddFriend= uiScreen.Query<VisualElement>("add-friend-panel");
            inviteNameField = subWindowAddFriend.Query<UITextField>("player-name");
            addFrienButton2= subWindowAddFriend.Query<Button>("add-friend-button");
            addFrienButton2.clicked += AddMemberByNameClicked;
            friendCloseButton = subWindowAddFriend.Query<Button>("Window-close-button");
            friendCloseButton.clicked += HideInvitePopup;

            subWindowAddBlock= uiScreen.Query<VisualElement>("add-block-panel");
            blockListNameField= subWindowAddBlock.Query<UITextField>("player-name");
            addBlockButton2= subWindowAddBlock.Query<Button>("add-block-button");;
            addBlockButton2.clicked += AddBlockListMemberByNameClicked;
            blockCloseButton = subWindowAddBlock.Query<Button>("Window-close-button");
            blockCloseButton.clicked += HideAddBlockPopup;
            return true;
        }

        private void TopMenuChange(int obj)
        {
            switch (obj)
            {
                case 0:
                    showFriensList();
                    break;
                case 1:
                    showBlockList();
                    break;
            }
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("SOCIAL_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("SOCIAL_UPDATE", this);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().social.key) ||Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().social.altKey) )&& !ClientAPI.UIHasFocus())
            {
                Toggle();
            }

            // if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && memberPopup != null &&
            //     memberPopup.IsVisibleElement())
            // {
            //     HideMemberPopup();
            // }
            
            if (showUseFade)
            {
                if (subWindowAddFriend != null)
                {
                    var op = subWindowAddFriend.resolvedStyle.opacity;
                    //  Debug.LogError("Update " + op);
                    if (showSubWindowAddFriend && op < 1f)
                    {
                        subWindowAddFriend.FadeInVisualElement();
                        
                    }
                    else if (!showSubWindowAddFriend && op > 0f)
                    {
                        subWindowAddFriend.FadeOutVisualElement();
                    }
                }
                if (subWindowAddBlock != null)
                {
                    var op = subWindowAddBlock.resolvedStyle.opacity;
                    //  Debug.LogError("Update " + op);
                    if (showSubWindowAddBlock && op < 1f)
                    {
                        subWindowAddBlock.FadeInVisualElement();
                        
                    }
                    else if (!showSubWindowAddBlock && op > 0f)
                    {
                        subWindowAddBlock.FadeOutVisualElement();
                    }
                }
            }

            
            
        }

        public override void Show()
        {
            base.Show();
            AtavismSocial.Instance.SendGetFriends();
            UpdateSocialDetails();
            HideSubWindowAddFriend();
            HideSubWindowAddBlock();
            HideMemberPopup();
            showFriensList();

        }

        public override void Hide()
        {
            base.Hide();
           HideSubWindowAddFriend();
           HideSubWindowAddBlock();
        }

        void ShowSubWindowAddFriend()
        {
            showSubWindowAddFriend = true;
            SubWindowMoveToCenter(subWindowAddFriend);
        }

        void HideSubWindowAddFriend()
        {
            showSubWindowAddFriend = false;
        }
        
        void ShowSubWindowAddBlock()
        {
            showSubWindowAddBlock = true;

            SubWindowMoveToCenter(subWindowAddBlock);
        }

        
        private void onSubWindowGeometryChanged(GeometryChangedEvent evt)
        {
            
            SubWindowMoveToCenter((VisualElement)evt.target);
            uiWindow.UnregisterCallback<GeometryChangedEvent>(onSubWindowGeometryChanged);
        }
        public void SubWindowMoveToCenter(VisualElement visualElement )
        {
            float width = visualElement.resolvedStyle.width;
            float height = visualElement.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            // Debug.LogError("MoveToCenter width="+width+"  height="+height);
            if (visualElement.resolvedStyle.width == 0 && visualElement.resolvedStyle.height == 0)
            {
                visualElement.RegisterCallback<GeometryChangedEvent>(onSubWindowGeometryChanged);
                width = visualElement.style.width.value.value;
                height = visualElement.style.height.value.value;
            }
            //  Debug.LogError("MoveToCenter | width="+width+"  height="+height);
            visualElement.style.left = canvasWidth * 0.5f - (width * 0.5f);
            visualElement.style.top = canvasHeight * 0.5f - (height * 0.5f);
        
        }
        void HideSubWindowAddBlock()
        {
            showSubWindowAddBlock = false;
        }

        public void showFriensList()
        {
            if (friendsPanel != null)
                friendsPanel.ShowVisualElement();
            if (blockListPanel != null)
                blockListPanel.HideVisualElement();
            _showFriendList = true;
          //  _showBlockList = false;
          HideSubWindowAddBlock();
            HideSubWindowAddFriend();
            HideMemberPopup();

        }

        public void showBlockList()
        {
            if (friendsPanel != null)
                friendsPanel.HideVisualElement();
            if (blockListPanel != null)
                    blockListPanel.ShowVisualElement();
            _showFriendList = false;
          //  _showBlockList = true;
          HideSubWindowAddBlock();
            HideSubWindowAddFriend();
          
            HideMemberPopup();


        }


        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "SOCIAL_UPDATE")
            {
                //if (showing) {
                UpdateSocialDetails();
                //   }
            }
        }

        public void UpdateSocialDetails()
        {
            friendListGrid.itemsSource = AtavismSocial.Instance.Friends;
            friendListGrid.Rebuild();
            friendListGrid.selectedIndex = -1;

            blockListGrid.itemsSource = AtavismSocial.Instance.Banneds;
            blockListGrid.Rebuild();
            blockListGrid.selectedIndex = -1;
            

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HideMemberPopup();
        }

        #region BlockList
        public void HideAddBlockPopup()
        {
            HideSubWindowAddBlock();
        }

        public void AddBlockListMenuClicked()
        {
            HideMemberPopup();
            if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetTargetObject().CheckBooleanProperty("combat.userflag"))
            {
                AtavismSocial.Instance.SendAddBlock(OID.fromLong(ClientAPI.GetTargetOid()), null);
                return;
            }
            else
            {
                ShowSubWindowAddBlock();

                if (blockListNameField != null)
                {
                    blockListNameField.value = "";
                    // EventSystem.current.SetSelectedGameObject(blockListNameField.gameObject, null);
                }
            }
        }
        public void AddBlockListMemberClicked()
        {
            HideMemberPopup();
            if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetTargetObject().CheckBooleanProperty("combat.userflag"))
            {
                AtavismSocial.Instance.SendAddBlock(OID.fromLong(ClientAPI.GetTargetOid()), null);
                return;
            }
            else
            {
                ShowSubWindowAddBlock();

                if (blockListNameField != null)
                {
                    blockListNameField.value = "";
                    // EventSystem.current.SetSelectedGameObject(TMPBlockListNameField.gameObject, null);
                }
            }
        }
        public void AddBlockListMemberByNameClicked()
        {

            if (blockListNameField != null)
                if (blockListNameField.value != "")
                {
                    AtavismSocial.Instance.SendAddBlock(null, blockListNameField.value);
                    HideSubWindowAddBlock();
                }
        }

        #endregion BlockList
        #region FriendList
        public void HideInvitePopup()
        {
            HideSubWindowAddFriend();
        }

        public void AddMemberClicked()
        {
            HideMemberPopup();
            if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetTargetObject().CheckBooleanProperty("combat.userflag"))
            {
                AtavismSocial.Instance.SendInvitation(OID.fromLong(ClientAPI.GetTargetOid()), null);
                return;
            }
            else
            {
                ShowSubWindowAddFriend();
                if (inviteNameField != null)
                {
                    inviteNameField.value = "";
                    // EventSystem.current.SetSelectedGameObject(inviteNameField.gameObject, null);
                }
            }
        }

        public void AddMemberMenuClicked()
        {
            HideMemberPopup();
            if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetTargetObject().CheckBooleanProperty("combat.userflag"))
            {
                AtavismSocial.Instance.SendInvitation(OID.fromLong(ClientAPI.GetTargetOid()), null);
                return;
            }
            else
            {
                ShowSubWindowAddFriend();
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
                    AtavismSocial.Instance.SendInvitation(null, inviteNameField.value);
                    HideSubWindowAddFriend();
                }
        }
        #endregion FriendList


        #region Member Popup
        public void ShowMemberPopup(UIAtavismSocialMemberEntry selectedMemberEntry, AtavismSocialMember member)
        {
            selectedMember = member;
            memberPopup.ShowVisualElement();
            // float width = visualElement.resolvedStyle.width;
            // float height = visualElement.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            
            
            Vector3 popupPosition = Input.mousePosition;
            memberPopup.style.left = scaledMousePosition.x - uiWindow.resolvedStyle.left; 
            memberPopup.style.top  = canvasHeight - scaledMousePosition.y - uiWindow.resolvedStyle.top; 
            if (_showFriendList)
            {
                if (AtavismGuild.Instance.GuildName == null || AtavismGuild.Instance.GuildName == "")
                {
                    if (guildButton != null)
                        guildButton.HideVisualElement();
                }
                else
                {
                    AtavismGuildMember gMember = AtavismGuild.Instance.GetGuildMemberByOid(OID.fromLong(ClientAPI.GetPlayerOid()));
                    if (gMember != null)
                    {
                        AtavismGuildRank rank = AtavismGuild.Instance.Ranks[gMember.rank];
                        if (rank.permissions.Contains(AtavismGuildRankPermission.invite))
                        {
                            if (guildButton != null)
                                guildButton.ShowVisualElement();
                        }
                    }

                }
                if (groupButton != null)
                    groupButton.ShowVisualElement();
                if (whisperButton != null)
                    whisperButton.ShowVisualElement();
                if(privetInstanceButton != null)
                    privetInstanceButton.ShowVisualElement();

            }
            else
            {
                if (guildButton != null)
                    guildButton.HideVisualElement();
                if (groupButton != null)
                    groupButton.HideVisualElement();
                if (whisperButton != null)
                    whisperButton.HideVisualElement();
                if(privetInstanceButton != null)
                    privetInstanceButton.HideVisualElement();

            }
        }

        public void HideMemberPopup()
        {
         //   Debug.LogError("HideMemberPopup");
            memberPopup.HideVisualElement();
        }

        public void InvitePrivateInstane()
        {
           AtavismSocial.Instance.SendInvitationToPtivate(selectedMember.oid);
           HideMemberPopup();
           AtavismSettings.Instance.DsContextMenu(null);
        }
       
        public void WhisperOptionClicked()
        {

            UIAtavismChatManager.Instance.StartWhisper(AtavismSocial.Instance.GetSocialMemberByOid(selectedMember.oid).name);
            HideMemberPopup();
            AtavismSettings.Instance.DsContextMenu(null);
        }

        public void InviteGroupOptionClicked()
        {
            AtavismGroup.Instance.SendInviteRequestMessage(selectedMember.oid);
            HideMemberPopup();
            AtavismSettings.Instance.DsContextMenu(null);
        }

        public void InviteGuildOptionClicked()
        {
            AtavismGuild.Instance.SendGuildCommand("invite", null, AtavismSocial.Instance.GetSocialMemberByOid(selectedMember.oid).name);
            memberPopup.HideVisualElement();
            AtavismSettings.Instance.DsContextMenu(null);
        }

        public void DeleteOptionClicked()
        {
            //    Debug.LogError("DeleteOptionClicked");
            if (_showFriendList)
            {
                AtavismSocial.Instance.SendDelFriend(selectedMember.oid);
            }
            else
            {
                AtavismSocial.Instance.SendDelBlock(selectedMember.oid);
            }
            HideMemberPopup();
            AtavismSettings.Instance.DsContextMenu(null);
        }



        #endregion Member Popup


    }
}