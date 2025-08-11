using Atavism.UI.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
   
    public class UIAtavismPortraitsManager : UIAtavismWindowBase
    {
        internal class GroupMemberOrganizer
        {
            internal VisualElement uiContainer;

            internal Label uiTitle;
            internal Label uiLevel;
            internal VisualElement uiIcon;
            internal VisualElement uiLeaderIcon;
            internal VisualElement uiPopupPanel;

            internal UIAtavismVitalityStatProgressBar uiHealth;
            internal UIAtavismVitalityStatProgressBar uiMana;

            internal VisualElement uiEffectsContainer;
            internal UIAtavismEffectSlot[] uiEffectSlots;

            internal Button uiPopupMenu_Promote;
            internal Button uiPopupMenu_Kick;

           
            //internal Button uiPopupMenu_Whisper; // not used ATM, optional, it could be useful
            //internal Button uiPopupMenu_Info; // not used ATM, optional, it could be useful
        }

        public enum PortraitPopupMenuType { none, Player, GroupMember, GuildMember, Pet, HelpBuilding, RepairBuilding }
        public string LEADER_PREFIX = "";

        [AtavismSeparator("UI")]
        [SerializeField] private string playerContainerName = "Player";
        [SerializeField] private string targetContainerName = "Target";
        [SerializeField] private string groupPanelName = "Group-panel";
        [SerializeField] private string groupSettingsButtonName = "Group-settings-button";
        [SerializeField] private string groupMembersContainerName = "Group-members-container";
        [SerializeField] private string groupLeaderIconName = "leader-icon";
        [Tooltip("Name + number is automatic")]
        [SerializeField] private string groupMemberName = "Group-member-";
        [Space(10)]
        [SerializeField] private string vipContainerName = "VIP";

        [Space(10)]
        [SerializeField] private string titleName = "Title";
        [SerializeField] private string subTitleName = "SubTitle";
        [SerializeField] private string speciesName = "Species";
        [SerializeField] private string iconName = "Icon";
        [SerializeField] private string levelName = "Level";
        [SerializeField] private string targetLightName = "light";
        [SerializeField] private string targetMobTypeName = "mob-type";
        [SerializeField] private string healthVitalityBarName = "Health";
        [SerializeField] private string manaVitalityBarName = "Mana";
        [SerializeField] private string weightVitalityBarName = "Weight";
        [SerializeField] private string staminaVitalityBarName = "Stamina";
        [SerializeField] private string[] shieldVitalityBarName = {"shield1"};
        [Space(10)]
        [SerializeField] private string popupPanelName = "Popup-panel";
        [SerializeField] private string effectsContainerName = "Effects-container";
        [Tooltip("Name + number is automatic")]
        [SerializeField] private string effectName = "Effect-";

        [Space(10)]
        [SerializeField] private string popupMenuLeaveGroupName = "LeaveGroup";
        [SerializeField] private string popupMenuWhisperName = "Whisper";
        [SerializeField] private string popupMenuInviteToGroupName = "InviteToGroup";
        [SerializeField] private string popupMenuTradeName = "Trade";
        [SerializeField] private string popupMenuDuelName = "Duel";
        [SerializeField] private string popupMenuInfoName = "Info";
        [SerializeField] private string popupMenuPetInfoName = "PetInfo";
        [SerializeField] private string popupMenuDespawnName = "Despawn";
        [SerializeField] private string popupMenuInviteToFriendsName = "InviteToFriends";
        [SerializeField] private string popupMenuInviteToGuildName = "InviteToGuild";
        [SerializeField] private string popupMenuHelpBuildingName = "HelpBuilding";
        [SerializeField] private string popupMenuRepairBuildingName = "RepairBuilding";
        [SerializeField] private string popupMenuPromoteName = "Promote";
        [SerializeField] private string popupMenuKickName = "Kick";

        [Space(10)]
        [SerializeField] private string targetMobTypeClass = "target-mob-type-";
        [SerializeField] private string targetTitleEnemyStanceClass = "title-enemy";
        [SerializeField] private string targetTitleFriendlyStanceClass = "title-friendly";
        [SerializeField] private string targetTitleNeutralStanceClass = "title-neutral";
        [SerializeField] private string targetLightEnemyStanceClass = "target-light-enemy";
        [SerializeField] private string targetLightFriendlyStanceClass = "target-light-friendly";
        [SerializeField] private string targetLightNeutralStanceClass = "target-light-neutral";
        
        private VisualElement uiPlayerContainer, uiTargetContainer, uiGroupPanel, uiGroupMembersContainer;
        private VisualElement uiPlayerIcon, uiPlayerLeaderIcon, uiPlayerVIP, uiTargetIcon;
        private VisualElement uiPlayerPopupPanel, uiTargetPopupPanel;
        private VisualElement uiPlayerEffectsContainer, uiTargetEffectsContainer;
        private VisualElement uiTargetMobTypeImage,uiTargetLightImage;
        private UIAtavismVitalityStatProgressBar uiPlayerHealth, uiPlayerMana, uiPlayerWeight, uiPlayerStamina;
        private Dictionary<string,UIAtavismVitalityStatProgressBar> uiPlayerShields = new Dictionary<string, UIAtavismVitalityStatProgressBar>();
        private UIAtavismVitalityStatProgressBar uiTargetHealth;
        private UIAtavismEffectSlot[] uiPlayerEffectSlots, uiTargetEffectSlots;
        private Label uiPlayerTitle, uiPlayerLevel, uiTargetTitle, uiTargetSubTitle, uiTargetSpecies, uiTargetLevel;
        private Button uiPopupMenu_Player_LeaveGroup;
        private Button uiPopupMenu_Target_Trade, uiPopupMenu_Target_Duel, uiPopupMenu_Target_Info, uiPopupMenu_Target_Pet_Info, uiPopupMenu_Target_Despawn, uiPopupMenu_Target_InviteToGroup, 
            uiPopupMenu_Target_InviteToFriends, uiPopupMenu_Target_InviteToGuild, uiPopupMenu_Target_HelpBuilding, uiPopupMenu_Target_RepairBuilding, 
            uiPopupMenu_Target_Whisper;
        private Button uiGroupSettingsButton;
        private UIAtavismGroupLootSettings uiGroupLootSettings;
        private UIAtavismGroupDice uiGroupDice;
        private GroupMemberOrganizer[] uiGroupMembers;

        private AtavismObjectNode lastTargetNode;
        private AtavismEffect[] lastTargetEffects;
        private VisualElement lastActivePopupMenu;

        #region Initiate
        protected override void OnEnable()
        {
            base.OnEnable();

            Show();
        }

        protected override void Start()
        {
            base.Start();

            Show();
        }

        public override void Show()
        {
            base.Show();

            HideLastPopupMenu();
            HidePlayerPopupMenu();
            HideTargetPopupMenu();
            HideVIP();
            HideGroupPanel();

            UpdateData();
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            uiPlayerContainer = uiDocument.rootVisualElement.Q<VisualElement>(playerContainerName);
            uiTargetContainer = uiDocument.rootVisualElement.Q<VisualElement>(targetContainerName);
            uiGroupPanel = uiDocument.rootVisualElement.Q<VisualElement>(groupPanelName);
            uiGroupSettingsButton = uiGroupPanel.Q<Button>(groupSettingsButtonName);
            uiGroupMembersContainer = uiDocument.rootVisualElement.Q<VisualElement>(groupMembersContainerName);

            uiPlayerIcon = uiPlayerContainer.Q<VisualElement>(iconName);
            uiPlayerLeaderIcon = uiPlayerContainer.Q<VisualElement>(groupLeaderIconName);
            uiPlayerVIP = uiPlayerContainer.Q<VisualElement>(vipContainerName);
            uiPlayerPopupPanel = uiPlayerContainer.Q<VisualElement>(popupPanelName);
            uiPlayerEffectsContainer = uiPlayerContainer.Q<VisualElement>(effectsContainerName);
            addEffectSlots(uiPlayerEffectsContainer.Children().ToArray(), ref uiPlayerEffectSlots);
            uiPlayerHealth = uiPlayerContainer.Q<UIAtavismVitalityStatProgressBar>(healthVitalityBarName);
            uiPlayerHealth.RegisterCallback<PointerEnterEvent>((e) =>
            {
#if AT_I2LOC_PRESET
                UIAtavismMiniTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Health"));
#else
                        UIAtavismMiniTooltip.Instance.SetDescription("Health");
#endif                        
                UIAtavismMiniTooltip.Instance.Show(uiPlayerHealth);

            });
            uiPlayerHealth.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });

            uiPlayerMana = uiPlayerContainer.Q<UIAtavismVitalityStatProgressBar>(manaVitalityBarName);
            uiPlayerMana.RegisterCallback<PointerEnterEvent>((e) =>
            {
#if AT_I2LOC_PRESET
                UIAtavismMiniTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Mana"));
#else
                        UIAtavismMiniTooltip.Instance.SetDescription("Mana");
#endif                        
                UIAtavismMiniTooltip.Instance.Show(uiPlayerMana);

            });
            uiPlayerMana.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });

            uiPlayerWeight = uiPlayerContainer.Q<UIAtavismVitalityStatProgressBar>(weightVitalityBarName);
            uiPlayerWeight.RegisterCallback<PointerEnterEvent>((e) =>
            {
#if AT_I2LOC_PRESET
                UIAtavismMiniTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Weight"));
#else
                        UIAtavismMiniTooltip.Instance.SetDescription("Weight");
#endif                        
                UIAtavismMiniTooltip.Instance.Show(uiPlayerWeight);

            });
            uiPlayerWeight.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });

            uiPlayerStamina = uiPlayerContainer.Q<UIAtavismVitalityStatProgressBar>(staminaVitalityBarName);
            uiPlayerStamina.RegisterCallback<PointerEnterEvent>((e) =>
            {
#if AT_I2LOC_PRESET
                UIAtavismMiniTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Stamina"));
#else
                        UIAtavismMiniTooltip.Instance.SetDescription("Stamina");
#endif                        
                UIAtavismMiniTooltip.Instance.Show(uiPlayerStamina);

            });
            uiPlayerStamina.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });
            uiPlayerShields.Clear();
            foreach (var shieldName in shieldVitalityBarName)
            {
                UIAtavismVitalityStatProgressBar  uiPlayerShield = uiPlayerContainer.Q<UIAtavismVitalityStatProgressBar>(shieldName);
                if (uiPlayerShield != null)
                {
                    uiPlayerShields.Add(shieldName,uiPlayerShield);
                    uiPlayerShield.RegisterCallback<PointerEnterEvent>((e) =>
                    {
#if AT_I2LOC_PRESET
                        UIAtavismMiniTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Shield"));
#else
                        UIAtavismMiniTooltip.Instance.SetDescription("Shield");
#endif                        
                        UIAtavismMiniTooltip.Instance.Show(uiPlayerShield);

                    });
                    uiPlayerShield.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });

                    uiPlayerShield.HideVisualElement();
                }
            }
          
            uiPlayerTitle = uiPlayerContainer.Q<Label>(titleName);
            uiPlayerLevel = uiPlayerContainer.Q<Label>(levelName);
            uiPopupMenu_Player_LeaveGroup = uiPlayerContainer.Q<Button>(popupMenuLeaveGroupName);

            uiTargetIcon = uiTargetContainer.Q<VisualElement>(iconName);
            uiTargetPopupPanel = uiTargetContainer.Q<VisualElement>(popupPanelName);
            uiTargetEffectsContainer = uiTargetContainer.Q<VisualElement>(effectsContainerName);
            addEffectSlots(uiTargetEffectsContainer.Children().ToArray(), ref uiTargetEffectSlots);
            uiTargetHealth = uiTargetContainer.Q<UIAtavismVitalityStatProgressBar>(healthVitalityBarName);
            uiTargetTitle = uiTargetContainer.Q<Label>(titleName);
            uiTargetSubTitle = uiTargetContainer.Q<Label>(subTitleName);
            uiTargetSpecies = uiTargetContainer.Q<Label>(speciesName);
            uiTargetLevel = uiTargetContainer.Q<Label>(levelName);
            uiTargetLightImage = uiTargetContainer.Q<VisualElement>(targetLightName);
            uiTargetMobTypeImage = uiTargetContainer.Q<VisualElement>(targetMobTypeName);
            uiPopupMenu_Target_Trade = uiTargetContainer.Q<Button>(popupMenuTradeName);
            uiPopupMenu_Target_Duel = uiTargetContainer.Q<Button>(popupMenuDuelName);
            uiPopupMenu_Target_Info = uiTargetContainer.Q<Button>(popupMenuInfoName);
            uiPopupMenu_Target_Pet_Info = uiTargetContainer.Q<Button>(popupMenuPetInfoName);
            uiPopupMenu_Target_Despawn = uiTargetContainer.Q<Button>(popupMenuDespawnName);
            uiPopupMenu_Target_InviteToGroup = uiTargetContainer.Q<Button>(popupMenuInviteToGroupName);
            uiPopupMenu_Target_InviteToFriends = uiTargetContainer.Q<Button>(popupMenuInviteToFriendsName);
            uiPopupMenu_Target_InviteToGuild = uiTargetContainer.Q<Button>(popupMenuInviteToGuildName);
            uiPopupMenu_Target_HelpBuilding = uiTargetContainer.Q<Button>(popupMenuHelpBuildingName);
            uiPopupMenu_Target_RepairBuilding = uiTargetContainer.Q<Button>(popupMenuRepairBuildingName);
            uiPopupMenu_Target_Whisper = uiTargetContainer.Q<Button>(popupMenuWhisperName);

            uiGroupLootSettings = uiScreen.Q<UIAtavismGroupLootSettings>();
            uiGroupLootSettings.Screen = uiScreen;
            uiGroupLootSettings.Hide();
            uiGroupDice = uiScreen.Q<UIAtavismGroupDice>();
            uiGroupDice.Initialize(this);
            uiGroupDice.Hide();

            VisualElement[] groupMembers = uiGroupMembersContainer.Children().ToArray();
            uiGroupMembers = new GroupMemberOrganizer[groupMembers.Length];
            for (int n = 0; n < groupMembers.Length; n++)
            {
                uiGroupMembers[n] = new GroupMemberOrganizer();
                uiGroupMembers[n].uiContainer = groupMembers[n];
                uiGroupMembers[n].uiIcon = groupMembers[n].Q<VisualElement>(iconName);
                uiGroupMembers[n].uiLeaderIcon = groupMembers[n].Q<VisualElement>(groupLeaderIconName);
                uiGroupMembers[n].uiIcon.viewDataKey = n.ToString();
                uiGroupMembers[n].uiPopupPanel = groupMembers[n].Query<VisualElement>(popupPanelName);
                uiGroupMembers[n].uiEffectsContainer = groupMembers[n].Query<VisualElement>(effectsContainerName);
                addEffectSlots(uiGroupMembers[n].uiEffectsContainer.Children().ToArray(), ref uiGroupMembers[n].uiEffectSlots);
                uiGroupMembers[n].uiHealth = groupMembers[n].Q<UIAtavismVitalityStatProgressBar>(healthVitalityBarName);
                uiGroupMembers[n].uiMana = groupMembers[n].Q<UIAtavismVitalityStatProgressBar>(manaVitalityBarName);
                uiGroupMembers[n].uiTitle = groupMembers[n].Q<Label>(titleName);
                uiGroupMembers[n].uiLevel = groupMembers[n].Q<Label>(levelName);
                uiGroupMembers[n].uiPopupMenu_Promote = uiGroupMembers[n].uiPopupPanel.Q<Button>(popupMenuPromoteName);
                uiGroupMembers[n].uiPopupMenu_Promote.viewDataKey = n.ToString();
                uiGroupMembers[n].uiPopupMenu_Kick = uiGroupMembers[n].uiPopupPanel.Q<Button>(popupMenuKickName);
                uiGroupMembers[n].uiPopupMenu_Kick.viewDataKey = n.ToString();
            }

            // Events
            uiPlayerIcon.RegisterCallback<MouseDownEvent>(onPlayerPortraitMouseDownEvent, TrickleDown.TrickleDown);
            uiPopupMenu_Player_LeaveGroup.clicked += LeaveGroupCommand;

            uiTargetIcon.RegisterCallback<MouseDownEvent>(onTargetPortraitMouseDownEvent, TrickleDown.TrickleDown);
            uiPopupMenu_Target_Trade.clicked += TradeCommand;
            uiPopupMenu_Target_Duel.clicked += DuelCommand;
            uiPopupMenu_Target_Info.clicked += InfoCommand;
            uiPopupMenu_Target_Pet_Info.clicked += PetInfoCommand;
            uiPopupMenu_Target_Despawn.clicked += PetDespawnCommand;
            uiPopupMenu_Target_InviteToGroup.clicked += InviteToGroupCommand;
            uiPopupMenu_Target_InviteToFriends.clicked += InviteToFriendCommand;
            uiPopupMenu_Target_InviteToGuild.clicked += InviteToGuildCommand;
            uiPopupMenu_Target_HelpBuilding.clicked += BuildHelpCommand;
            uiPopupMenu_Target_RepairBuilding.clicked += BuildRepairCommand;
            uiPopupMenu_Target_Whisper.clicked += WhisperCommand;

            uiGroupSettingsButton.clicked += onGroupLootSettingsClicked;

            for (int n = 0; n < uiGroupMembers.Length; n++)
            {
                uiGroupMembers[n].uiIcon.RegisterCallback<MouseDownEvent>(onGroupMemberMouseDownEvent, TrickleDown.TrickleDown);
                uiGroupMembers[n].uiPopupMenu_Kick.RegisterCallback<ClickEvent>(onGroupMemberKickEvent);
                uiGroupMembers[n].uiPopupMenu_Promote.RegisterCallback<ClickEvent>(onGroupMemberPromoteToLeaderEvent);
            }

            return true;
        }

        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            // Events
            uiPlayerIcon.UnregisterCallback<MouseDownEvent>(onPlayerPortraitMouseDownEvent, TrickleDown.TrickleDown);
            uiPopupMenu_Player_LeaveGroup.clicked -= LeaveGroupCommand;

            uiTargetIcon.UnregisterCallback<MouseDownEvent>(onTargetPortraitMouseDownEvent, TrickleDown.TrickleDown);
            uiPopupMenu_Target_Trade.clicked -= TradeCommand;
            uiPopupMenu_Target_Duel.clicked -= DuelCommand;
            uiPopupMenu_Target_Info.clicked -= InfoCommand;
            uiPopupMenu_Target_Pet_Info.clicked -= PetInfoCommand;
            
            
            uiPopupMenu_Target_Despawn.clicked -= PetDespawnCommand;
            uiPopupMenu_Target_InviteToGroup.clicked -= InviteToGroupCommand;
            uiPopupMenu_Target_InviteToFriends.clicked -= InviteToFriendCommand;
            uiPopupMenu_Target_InviteToGuild.clicked -= InviteToGuildCommand;
            uiPopupMenu_Target_HelpBuilding.clicked -= BuildHelpCommand;
            uiPopupMenu_Target_RepairBuilding.clicked -= BuildRepairCommand;
            uiPopupMenu_Target_Whisper.clicked -= WhisperCommand;

            uiGroupSettingsButton.clicked -= onGroupLootSettingsClicked;

            for (int n = 0; n < uiGroupMembers.Length; n++)
            {
                uiGroupMembers[n].uiIcon.UnregisterCallback<MouseDownEvent>(onGroupMemberMouseDownEvent, TrickleDown.TrickleDown);
                uiGroupMembers[n].uiPopupMenu_Kick.UnregisterCallback<ClickEvent>(onGroupMemberKickEvent);
                uiGroupMembers[n].uiPopupMenu_Promote.UnregisterCallback<ClickEvent>(onGroupMemberPromoteToLeaderEvent);
            }

            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();

            AtavismEventSystem.RegisterEvent(EVENTS.PLAYER_TARGET_CHANGED, this);
            AtavismEventSystem.RegisterEvent(EVENTS.PLAYER_TARGET_CLEARED, this);
            AtavismEventSystem.RegisterEvent(EVENTS.OBJECT_TARGET_CHANGED, this);
            AtavismEventSystem.RegisterEvent(EVENTS.EFFECT_UPDATE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.EFFECT_ICON_UPDATE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.GROUP_UPDATE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.GROUP_DICE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.GROUP_UPDATE_SETTINGS, this);
            AtavismEventSystem.RegisterEvent(EVENTS.GROUP_INVITE_REQUEST, this);
            AtavismEventSystem.RegisterEvent(EVENTS.GROUP_INVITE_REQUEST, this);
            NetworkAPI.RegisterExtensionMessageHandler("shieldUpdate", HandleShieldListUpdate);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();

            NetworkAPI.RemoveExtensionMessageHandler("shieldUpdate", HandleShieldListUpdate);
            AtavismEventSystem.UnregisterEvent(EVENTS.PLAYER_TARGET_CHANGED, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.PLAYER_TARGET_CLEARED, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.OBJECT_TARGET_CHANGED, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.EFFECT_UPDATE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.EFFECT_ICON_UPDATE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.GROUP_UPDATE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.GROUP_DICE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.GROUP_UPDATE_SETTINGS, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.GROUP_INVITE_REQUEST, this);
        }

        protected override void registerPropertyChangedHandlers()
        {
            base.registerPropertyChangedHandlers();

            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.HEALTH, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.HEALTH_MAX, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.MANA, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.MANA_MAX, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.WEIGHT, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.WEIGHT_MAX, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.STAMINA, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(PROPS.STAMINA_MAX, updateDataPlayerHandler);
        }

        protected override void unregisterPropertyChangedHandlers()
        {
            base.unregisterPropertyChangedHandlers();

            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.HEALTH, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.HEALTH_MAX, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.MANA, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.MANA_MAX, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.WEIGHT, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.WEIGHT_MAX, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.STAMINA, updateDataPlayerHandler);
            ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(PROPS.STAMINA_MAX, updateDataPlayerHandler);
        }
        #endregion
        #region UI Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void onPlayerPortraitMouseDownEvent(MouseDownEvent evt)
        {
           // Debug.LogError("onPlayerPortraitMouseDownEvent");
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                ClientAPI.SetTarget(ClientAPI.GetPlayerOid());
            }
            else if (evt.button == (int)MouseButton.RightMouse)
            {
                if (AtavismGroup.Instance.Members != null && AtavismGroup.Instance.Members.Count > 0)
                    ShowPlayerPopupMenu();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void onTargetPortraitMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                ClientAPI.SetTarget(ClientAPI.GetTargetOid());
            }
            else if (evt.button == (int)MouseButton.RightMouse)
            {
                if (lastTargetNode == null)
                {
                    HideTargetPopupMenu();
                    return;
                }
                // Verify the target is a player and is friendly
                
                if (!lastTargetNode.PropertyExists("targetType"))
                    return;
                int targetType = (int)lastTargetNode.GetProperty("targetType");
                if (targetType < 0)
                    return;
                bool isPet = false;
                if (lastTargetNode.PropertyExists(PROPS.PET))
                    isPet = (bool)lastTargetNode.GetProperty(PROPS.PET);
               // Debug.LogError("onTargetPortraitMouseDownEvent ||");
                // Show popup menu
                if (!isPet)
                {
                    bool isGuildMember = false;
                    if (!lastTargetNode.CheckBooleanProperty("combat.userflag"))
                        return;
                    AtavismGuildMember member = AtavismGuild.Instance.GetGuildMemberByOid(OID.fromLong(ClientAPI.GetPlayerOid()));
                    if (member != null)
                    {
                        if (AtavismGuild.Instance.Ranks.Count > member.rank)
                        {
                            AtavismGuildRank rank = AtavismGuild.Instance.Ranks[member.rank];
                            if (rank.permissions.Contains(AtavismGuildRankPermission.invite))
                                isGuildMember = true;
                        }
                    }

                    if (AtavismGroup.Instance.Members != null && AtavismGroup.Instance.Members.Count > 0)
                        ShowTargetPopupMenu(PortraitPopupMenuType.GroupMember);
                    else ShowTargetPopupMenu(PortraitPopupMenuType.Player);

                    if (isGuildMember)
                        ShowTargetPopupMenu(PortraitPopupMenuType.GuildMember, true);
                }
                else if (lastTargetNode.PropertyExists(PROPS.PET_OWNER) && ((OID)lastTargetNode.GetProperty(PROPS.PET_OWNER)).Equals(OID.fromLong(ClientAPI.GetPlayerOid())))
                {
                    ShowTargetPopupMenu(PortraitPopupMenuType.Pet, false);
                }
                else if (WorldBuilder.Instance.SelectedClaimObject != null)
                {
                    ShowTargetPopupMenu(PortraitPopupMenuType.none);

                    if (!WorldBuilder.Instance.SelectedClaimObject.Solo)
                        if (WorldBuilder.Instance.SelectedClaimObject.totalTime > 0 || WorldBuilder.Instance.SelectedClaimObject.currentTime < WorldBuilder.Instance.SelectedClaimObject.totalTime)
                            if (!WorldBuilder.Instance.GetClaim(WorldBuilder.Instance.SelectedClaimObject.ClaimID).playerOwned)
                                ShowTargetPopupMenu(PortraitPopupMenuType.HelpBuilding, true);

                    if (WorldBuilder.Instance.SelectedClaimObject.Health < WorldBuilder.Instance.SelectedClaimObject.MaxHealth && WorldBuilder.Instance.SelectedClaimObject.Repairable)
                        ShowTargetPopupMenu(PortraitPopupMenuType.RepairBuilding, true);
                }
            }
          //  Debug.LogError("onTargetPortraitMouseDownEvent End");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void onGroupMemberMouseDownEvent(MouseDownEvent evt)
        {
            int index = Convert.ToInt32(((VisualElement)evt.target).viewDataKey);

            if (evt.button == (int)MouseButton.LeftMouse)
            {
                SelectGroupMember(index);
            }
            else if (evt.button == (int)MouseButton.RightMouse)
            {
                if (AtavismGroup.Instance.LeaderOid.ToLong() == ClientAPI.GetPlayerOid())
                    ShowGroupMemberPopupMenu(index);
            }
        }

        protected virtual void onGroupMemberKickEvent(ClickEvent evt)
        {
            int index = Convert.ToInt32(((VisualElement)evt.target).viewDataKey);

            KickCommand(index);
        }

        protected virtual void onGroupMemberPromoteToLeaderEvent(ClickEvent evt)
        {
            int index = Convert.ToInt32(((VisualElement)evt.target).viewDataKey);

            PromoteToLeaderCommand(index);
        }

        protected virtual void onGroupLootSettingsClicked()
        {
            if (uiGroupLootSettings.IsVisible)
                uiGroupLootSettings.Hide();
            else uiGroupLootSettings.Show();
        }
        #endregion
        #region Atavism Events
        protected override void OnEvent(AtavismEventData eData)
        {
           // Debug.LogError("Group OnEvent "+eData.eventType);
            base.OnEvent(eData);

            if (eData.eventType == EVENTS.GROUP_UPDATE)
            {
                updateData_Player(true);
                updateData_Target(true);
                updateData_GroupMembers(true);
                uiGroupLootSettings.UpdateData();
            }

            if (eData.eventType == EVENTS.PLAYER_TARGET_CHANGED || eData.eventType == EVENTS.PLAYER_TARGET_CLEARED || eData.eventType == EVENTS.OBJECT_TARGET_CHANGED)
                updateData_Target(true);

            if (eData.eventType == EVENTS.GROUP_INVITE_REQUEST)
            {
                long groupLeaderoid = Convert.ToInt64(eData.eventArgs[0]);
                string groupLeaderName = eData.eventArgs[1];
                int timeout = Convert.ToInt32(eData.eventArgs[2]);

                string message = groupLeaderName + " " + "has invited you to join their group";
#if AT_I2LOC_PRESET
        message = groupLeaderName + " " + I2.Loc.LocalizationManager.GetTranslation("has invited you to join their group");
#endif

                UIAtavismDialogPopupManager.Instance.ShowDialogPopup(message, confirmGroupRequestInvitation, "Yes", declineGroupRequestInvitation, "No");
                UIAtavismDialogPopupManager.Instance.SetTimeout(timeout);
            }

            if (eData.eventType == EVENTS.EFFECT_UPDATE || eData.eventType == EVENTS.EFFECT_ICON_UPDATE)
            {
                updatePlayerEffectSlots();

                if (eData.eventType == EVENTS.EFFECT_ICON_UPDATE)
                {
                    updateTargetEffectSlots();
                    updateGroupMembersEffects();
                }
            }

            if (eData.eventType == EVENTS.GROUP_DICE)
            {
              //  Debug.LogError("Group Dice");
                
                
                float length = int.Parse(eData.eventArgs[0]);
                float expiration = Time.time + length;
                int diceItemId = int.Parse(eData.eventArgs[1]);
              //  Debug.LogError("Group Dice diceItemId="+diceItemId+" lenght="+length+" expire="+expiration+" "+(uiGroupDice!=null?uiGroupDice.IsVisible+"":"bd"));

              
                if (uiGroupDice!=null && !uiGroupDice.IsVisible)
                {
                    AtavismInventoryItem diceItem = Inventory.Instance.GetItemByTemplateID(diceItemId);
                    uiGroupDice.Show(diceItem, expiration, length);
                }
                else
                {
                 //   Debug.LogError("Group Dice vis");
                }

               // Debug.LogError("Group Dice END");
            }
        }
        #endregion
        #region Property Changed Handlers
        private void updateDataPlayerHandler(object sender, PropertyChangeEventArgs args)
        {
            updateData_Player();
        }
        private void updateDataTargetHandler(object sender, PropertyChangeEventArgs args)
        {
            updateData_Target();
        }
        #endregion
        #region Loop Updates
        protected override void Update()
        {
            base.Update();

            if (Input.GetMouseButtonUp(0))
                HideLastPopupMenu();
            UpdateData();
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        public override void UpdateData()
        {
            base.UpdateData();

            updateData_Player(true);
            updateData_Target(true);
            updateData_GroupMembers(true);
        }

        public void ShowPopupMenu(VisualElement ve)
        {
            HideLastPopupMenu();
            lastActivePopupMenu = ve;
            UIToolkit.ShowVisualElement(ve);
            UIToolkit.SetToMousePosition(ve, uiScreen, new Vector2(0.5f, 0.1f)); // This situation when element is already visible
            // Need to register callback for situations when element is not created (Display element off UI Toolkit will destroy the element)
            ve.RegisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged); 
        }
        public void HideLastPopupMenu()
        {
            if (lastActivePopupMenu != null)
            {
                lastActivePopupMenu.UnregisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
                UIToolkit.HideVisualElement(lastActivePopupMenu);
            }

            lastActivePopupMenu = null;
        }

        private void onPopupMenuGeometryChanged(GeometryChangedEvent evt)
        {
            UIToolkit.SetToMousePosition((VisualElement)evt.target, uiScreen, new Vector2(0.5f, 0.1f)); 
        }
        #region Player
        public void ShowPlayerPopupMenu() => ShowPopupMenu(uiPlayerPopupPanel);
        public void HidePlayerPopupMenu()
        {
            if (uiPlayerPopupPanel != null)
                uiPlayerPopupPanel.UnregisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
            UIToolkit.HideVisualElement(uiPlayerPopupPanel);
        }
        public void ShowVIP() => UIToolkit.ShowVisualElement(uiPlayerVIP);
        public void HideVIP() => UIToolkit.HideVisualElement(uiPlayerVIP);

        public void LeaveGroupCommand()
        {
            AtavismGroup.Instance.LeaveGroup();
            HideLastPopupMenu();
        }
        #endregion
        #region Target
        public void ShowTargetPortrait() => UIToolkit.ShowVisualElement(uiTargetContainer);
        public void HideTargetPortrait()
        {
            HideTargetPopupMenu();
            UIToolkit.HideVisualElement(uiTargetContainer);
        }
        public void ShowTargetPopupMenu(PortraitPopupMenuType type, bool add = false)
        {
        //    Debug.LogError("onTargetPortraitMouseDownEvent ShowTargetPopupMenu |");

            if (!add)
            {
                UIToolkit.HideVisualElement(uiPopupMenu_Target_Trade);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_Duel);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_Info);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_Pet_Info);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_Despawn);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_InviteToGroup);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_InviteToFriends);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_InviteToGuild);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_HelpBuilding);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_RepairBuilding);
                UIToolkit.HideVisualElement(uiPopupMenu_Target_Whisper);
            }

            switch (type)
            {
                case PortraitPopupMenuType.Player:
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Whisper);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Trade);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Duel);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Info);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_InviteToGroup);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_InviteToFriends);
                    break;
                case PortraitPopupMenuType.GroupMember:
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Whisper);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Trade);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Duel);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Info);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_InviteToFriends);
                    break;
                case PortraitPopupMenuType.Pet:
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Despawn);
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_Pet_Info);
                    break;
                case PortraitPopupMenuType.GuildMember:
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_InviteToGuild);
                    break;
                case PortraitPopupMenuType.HelpBuilding:
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_HelpBuilding);
                    break;
                case PortraitPopupMenuType.RepairBuilding:
                    UIToolkit.ShowVisualElement(uiPopupMenu_Target_RepairBuilding);
                    break;
            }

            if (type != PortraitPopupMenuType.none)
                ShowPopupMenu(uiTargetPopupPanel);
        }
        public void HideTargetPopupMenu()
        {
            if (uiTargetPopupPanel != null)
                uiTargetPopupPanel.UnregisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
            UIToolkit.HideVisualElement(uiTargetPopupPanel);
        }

        /// <summary>
        /// 
        /// </summary>
        public void TradeCommand()
        {
            if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetObjectNode(ClientAPI.GetTargetOid()) != null)
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, ClientAPI.GetObjectNode(ClientAPI.GetTargetOid()).Position) > AtavismTrade.Instance.InteractionDistance)
                {
                    string[] args = new string[1];
#if AT_I2LOC_PRESET
                args[0] = I2.Loc.LocalizationManager.GetTranslation("Target is too far away");
#else
                    args[0] = "Target is too far away";
#endif
                    AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                }
                else
                {
                    NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/trade");
                }

            HideLastPopupMenu();
        }

        public void WhisperCommand()
        {
            
            if (UIAtavismChatManager.Instance != null)
                UIAtavismChatManager.Instance.StartWhisper(ClientAPI.GetTargetObject().Name);

            HideLastPopupMenu();
        }

        public void InviteToGroupCommand()
        {
            AtavismGroup.Instance.SendInviteRequestMessage(OID.fromLong(ClientAPI.GetTargetOid()));

            HideLastPopupMenu();
        }

        public void DuelCommand()
        {
            
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/duel");

            HideLastPopupMenu();
        }
        public void InfoCommand()
        {
            UIAtavismOtherCharacterPanel.Instance.UpdateCharacterData(ClientAPI.GetTargetOid());

            HideLastPopupMenu();
        }

        public void PetInfoCommand()
        {
            // Debug.LogError("Pet PetInfoCommand ");
            UIAtavismPetInventoryPanel.Instance.UpdateCharacterData(ClientAPI.GetTargetOid());

            HideLastPopupMenu();
        }

        public void PetDespawnCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand despawn");

            HideLastPopupMenu();
        }

        public void InviteToFriendCommand()
        {
            AtavismSocial.Instance.SendInvitation(OID.fromLong(ClientAPI.GetTargetOid()), null);

            HideLastPopupMenu();
        }
        /// <summary>
        /// Send Invite to guild command 
        /// </summary>
        public void InviteToGuildCommand()
        {
            AtavismGuild.Instance.SendGuildCommand("invite", OID.fromLong(ClientAPI.GetTargetOid()), null);

            HideLastPopupMenu();
        }

        /// <summary>
        /// Send Building Help command 
        /// </summary>
        public void BuildHelpCommand()
        {
            //   Debug.LogError("BuildHelpCommand");
            /* if (WorldBuilder.Instance.SelectedClaimObject == null)
                 return;*/
            int id = WorldBuilder.Instance.SelectedClaimObject.ID;
            int claimID = WorldBuilder.Instance.SelectedClaimObject.ClaimID;
            //   Debug.LogError("BuildHelpCommand objId="+id+" claimId="+claimID);

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("action", "useHelp");
            props.Add("claimID", claimID);
            props.Add("objectID", id);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM_OBJECT", props);

            HideLastPopupMenu();

        }
        /// <summary>
        /// Send Building Repiar command
        /// </summary>
        public void BuildRepairCommand()
        {
            /* if (WorldBuilder.Instance.SelectedClaimObject == null)
                 return;*/
            int id = WorldBuilder.Instance.SelectedClaimObject.ID;
            int claimID = WorldBuilder.Instance.SelectedClaimObject.ClaimID;
            if (WorldBuilder.Instance.GetClaim(claimID).permissionlevel < 1 && !WorldBuilder.Instance.GetClaim(claimID).playerOwned)
            {
              //  Debug.LogError("BuildHelpCommand no permition");
                return;
            }

            if (!WorldBuilder.Instance.GetClaim(claimID).claimObjects[id].Repairable)
            {
              //  Debug.LogError("BuildHelpCommand no Repairable");
                return;
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("action", "useRepair");
            props.Add("claimID", claimID);
            props.Add("objectID", id);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM_OBJECT", props);

            HideLastPopupMenu();
        }
        #endregion
        #region Group
        public void SelectGroupMember(int index) => ClientAPI.SetTarget(AtavismGroup.Instance.Members[index].oid.ToLong());
        public void ShowGroupPanel() => UIToolkit.ShowVisualElement(uiGroupPanel);
        public void HideGroupPanel()
        {
            HideGroupMembers();
            UIToolkit.HideVisualElement(uiGroupPanel);
        }
        public void ShowGroupMember(int index) => UIToolkit.ShowVisualElement(uiGroupMembers[index].uiContainer);
        public void HideGroupMember(int index)
        {
            HideGroupMemberPopupMenu(index);
            UIToolkit.HideVisualElement(uiGroupMembers[index].uiContainer);
        }
        public void HideGroupMembers()
        {
            for (int n = 0; n < uiGroupMembers.Length; n++)
                HideGroupMember(n);
        }
        public void ShowGroupMemberPopupMenu(int index) => ShowPopupMenu(uiGroupMembers[index].uiPopupPanel);

        public void HideGroupMemberPopupMenu(int index)
        {
            if (uiGroupMembers[index].uiPopupPanel != null)
                uiGroupMembers[index].uiPopupPanel.UnregisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
            UIToolkit.HideVisualElement(uiGroupMembers[index].uiPopupPanel);
        }
        public void HideGroupMembersPopupMenu()
        {
            for (int n = 0; n < uiGroupMembers.Length; n++)
                HideGroupMemberPopupMenu(n);
        }

        
        public void PromoteToLeaderCommand(int index)
        {
            AtavismGroup.Instance.PromoteToLeader(AtavismGroup.Instance.Members[index].oid);
            HideGroupMemberPopupMenu(index);
        }

        public void KickCommand(int index)
        {
            AtavismGroup.Instance.RemoveGroupMember(AtavismGroup.Instance.Members[index].oid);
            HideGroupMemberPopupMenu(index);
        }
        #endregion
        #endregion
        #region Private Methods
        #region Player
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatePortrait"></param>
        protected virtual void updateData_Player(bool updatePortrait = false)
        {
            if (ClientAPI.GetPlayerObject() == null)
                return;

            uiPlayerTitle.text = getMobTitle(ClientAPI.GetPlayerObject());
            if (AtavismGroup.Instance.LeaderOid!=null && AtavismGroup.Instance.LeaderOid.ToLong() == ClientAPI.GetPlayerOid())
                uiPlayerLeaderIcon.ShowVisualElement();
            else
                uiPlayerLeaderIcon.HideVisualElement();
            
            object health = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.HEALTH);
            object health_max = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.HEALTH_MAX);
            object mana = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.MANA);
            object mana_max = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.MANA_MAX);
            object weight = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.WEIGHT);
            object weight_max = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.WEIGHT_MAX);
            object stamina = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.STAMINA);
            object stamina_max = ClientAPI.GetPlayerObject().GetPropertyStatWithPrecision(PROPS.STAMINA_MAX);

            updateData_VitalityStat(uiPlayerHealth, health, health_max);
            updateData_VitalityStat(uiPlayerMana, mana, mana_max);
            updateData_VitalityStat(uiPlayerWeight, weight, weight_max);
            updateData_VitalityStat(uiPlayerStamina, stamina, stamina_max);

            object level = ClientAPI.GetPlayerObject().GetProperty(PROPS.LEVEL);
            if (level != null)
                uiPlayerLevel.text = ((int)level).ToString();

          //  if (updatePortrait)
                UIToolkit.SetBackgroundImage(uiPlayerIcon, UTILS.GetPortrait(ClientAPI.GetPlayerObject()));
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void updatePlayerEffectSlots() => updateEffects(ref uiPlayerEffectSlots, Abilities.Instance.PlayerEffects);
        #endregion
        #region Target
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatePortrait"></param>
        protected virtual void updateData_Target(bool updatePortrait = false)
        {
            if (ClientAPI.GetTargetObject() == null)
            {
                unregisterTarget();
                HideTargetPortrait();
                return;
            }

            if (WorldBuilder.Instance.SelectedClaimObject != null)
            {
                ClaimObject co = WorldBuilder.Instance.SelectedClaimObject;
                AtavismBuildObjectTemplate template = WorldBuilder.Instance.GetBuildObjectTemplate(co.TemplateID);
                
                uiTargetTitle.text = template.buildObjectName;
                uiTargetSubTitle.text = "";
                uiTargetSpecies.text = "";

                UIToolkit.HideVisualElement(uiTargetLevel.parent);
                updateData_VitalityStat(uiTargetHealth, co.Health, co.MaxHealth);

                if (updatePortrait)
                    UIToolkit.SetBackgroundImage(uiTargetIcon, template.Icon);
                
                lastTargetNode = ClientAPI.WorldManager.GetObjectNode(co.gameObject.GetComponent<AtavismNode>().Oid);
            }
            else
            {
                if (uiTargetMobTypeImage != null)
                {
                    for (int i = 0; i < 200; i++)
                    {
                        uiTargetMobTypeImage.RemoveFromClassList(targetMobTypeClass+i);
                    }
                    if (ClientAPI.GetTargetObject().PropertyExists("mobType"))
                    {
                        int mobType = (int)ClientAPI.GetTargetObject().GetProperty("mobType");
                        uiTargetMobTypeImage.ShowVisualElement();
                        uiTargetMobTypeImage.AddToClassList(targetMobTypeClass+mobType);
                    }
                    else
                    {
                        uiTargetMobTypeImage.HideVisualElement();
                    }
                }

                uiTargetTitle.RemoveFromClassList(targetTitleEnemyStanceClass);
                uiTargetTitle.RemoveFromClassList(targetTitleFriendlyStanceClass);
                uiTargetTitle.RemoveFromClassList(targetTitleNeutralStanceClass);
                uiTargetTitle.text = getMobTitle(ClientAPI.GetTargetObject());
                
                int targetType = 0;
                if (ClientAPI.GetTargetObject().PropertyExists("reaction"))
                {
                    targetType = (int)ClientAPI.GetTargetObject().GetProperty("reaction");
                    if (ClientAPI.GetTargetObject().PropertyExists("aggressive"))
                    {
                        if ((bool)ClientAPI.GetTargetObject().GetProperty("aggressive"))
                        {
                            targetType = -1;
                        }
                    }
                }

                if (uiTargetLightImage != null)
                {
                    uiTargetLightImage.RemoveFromClassList(targetLightEnemyStanceClass);
                    uiTargetLightImage.RemoveFromClassList(targetLightFriendlyStanceClass);
                    uiTargetLightImage.RemoveFromClassList(targetLightNeutralStanceClass);
                }

                if (targetType < 0)
                {
                    uiTargetTitle.AddToClassList(targetTitleEnemyStanceClass);
                    if (uiTargetLightImage != null) uiTargetLightImage.AddToClassList(targetLightEnemyStanceClass);
                }
                else if (targetType > 0)
                {
                    uiTargetTitle.AddToClassList(targetTitleFriendlyStanceClass);
                    if (uiTargetLightImage != null) uiTargetLightImage.AddToClassList(targetLightFriendlyStanceClass);
                }
                else
                {
                    uiTargetTitle.AddToClassList(targetTitleNeutralStanceClass);
                    if (uiTargetLightImage != null) uiTargetLightImage.AddToClassList(targetLightNeutralStanceClass);
                }

                uiTargetSubTitle.text = getMobSubTitle(ClientAPI.GetTargetObject());
                uiTargetSpecies.text = getMobSpecies(ClientAPI.GetTargetObject());

                object health = ClientAPI.GetTargetObject().GetPropertyStatWithPrecision(PROPS.HEALTH);
                object health_max = ClientAPI.GetTargetObject().GetPropertyStatWithPrecision(PROPS.HEALTH_MAX);
                object level = ClientAPI.GetTargetObject().GetProperty(PROPS.LEVEL);
                bool isPet = false;
                if (ClientAPI.GetTargetObject().PropertyExists(PROPS.PET))
                    isPet = (bool)ClientAPI.GetTargetObject().GetProperty(PROPS.PET);
                if (isPet)
                {
                    level = ClientAPI.GetTargetObject().GetProperty(PROPS.PET_LEVEL);
                }
                
                updateData_VitalityStat(uiTargetHealth, health, health_max);

                if (level != null)
                {
                    uiTargetLevel.text = ((int)level).ToString();
                    UIToolkit.ShowVisualElement(uiTargetLevel.parent);
                }

               // if (updatePortrait)
                    UIToolkit.SetBackgroundImage(uiTargetIcon, UTILS.GetPortrait(ClientAPI.GetTargetObject()));

                registerTarget();
            }

            updateTargetEffectSlots();
            ShowTargetPortrait();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void updateTargetEffectSlots()
        {
            if (lastTargetNode != null)
                lastTargetEffects = getEffects(lastTargetNode);

            updateEffects(ref uiTargetEffectSlots, lastTargetEffects);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerTarget()
        {
            AtavismMobNode mobNode = ClientAPI.GetTargetObject();

            if (lastTargetNode != null)
            {
                if (mobNode != null && lastTargetNode.Oid == mobNode.Oid)
                    return;
            }

            unregisterTarget();

            lastTargetNode = mobNode;

            if (lastTargetNode != null)
            {
                lastTargetNode.RegisterPropertyChangeHandler(PROPS.HEALTH, updateDataTargetHandler);
                lastTargetNode.RegisterPropertyChangeHandler(PROPS.HEALTH_MAX, updateDataTargetHandler);
                lastTargetNode.RegisterPropertyChangeHandler(PROPS.EFFECTS, updateDataTargetHandler);
                lastTargetNode.RegisterPropertyChangeHandler(PROPS.REACTION, updateDataTargetHandler);
                lastTargetNode.RegisterPropertyChangeHandler(PROPS.AGGRESSIVE, updateDataTargetHandler);
            }
        }

        protected virtual void unregisterTarget()
        {
            if (lastTargetNode != null)
            {
                lastTargetNode.RemovePropertyChangeHandler(PROPS.HEALTH, updateDataTargetHandler);
                lastTargetNode.RemovePropertyChangeHandler(PROPS.HEALTH_MAX, updateDataTargetHandler);
                lastTargetNode.RemovePropertyChangeHandler(PROPS.EFFECTS, updateDataTargetHandler);
                lastTargetNode.RemovePropertyChangeHandler(PROPS.REACTION, updateDataTargetHandler);
                lastTargetNode.RemovePropertyChangeHandler(PROPS.AGGRESSIVE, updateDataTargetHandler);

                lastTargetNode = null;
            }
        }

        private void confirmGroupRequestInvitation()
        {
            AtavismGroup.Instance.ConfirmGroupRequestInvitation();
            UIAtavismDialogPopupManager.Instance.HideDialogPopup();
        }

        private void declineGroupRequestInvitation()
        {
            AtavismGroup.Instance.DeclineGroupRequestInvitation();
            UIAtavismDialogPopupManager.Instance.HideDialogPopup();
        }
        #endregion
        #region Group
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        protected virtual void updateData_GroupMembers(bool updatePortrait = false)
        {
            //Debug.LogError("updateData_GroupMembers");

            if (AtavismGroup.Instance.Members.Count == 0)
            {
                HideGroupPanel();
            }
            else
            {
                ShowGroupPanel();

                for (int n = 0; n < uiGroupMembers.Length; n++)
                {
                    if (n < AtavismGroup.Instance.Members.Count)
                    {
                        ShowGroupMember(n);
                        updateData_GroupMember(n, updatePortrait);
                    }
                    else HideGroupMember(n);
                }
            }
        }

        protected virtual void updateData_GroupMember(int index, bool updatePortrait = false)
        {
            if (AtavismGroup.Instance.Members == null)
                return;
            if (index < 0 || index >= AtavismGroup.Instance.Members.Count)
                return;

            GroupMember member = AtavismGroup.Instance.Members[index];

            if (AtavismGroup.Instance.LeaderOid == member.oid)
            {
                uiGroupMembers[index].uiLeaderIcon.ShowVisualElement();
                uiGroupMembers[index].uiTitle.text = LEADER_PREFIX + member.name;
            }
            else
            {
                uiGroupMembers[index].uiLeaderIcon.HideVisualElement();
                uiGroupMembers[index].uiTitle.text = member.name;

            }

            object health = member.GetPropertyStatWithPrecision(PROPS.HEALTH);
            object health_max = member.GetPropertyStatWithPrecision(PROPS.HEALTH_MAX);
        object mana = member.GetPropertyStatWithPrecision(PROPS.MANA);
            object mana_max = member.GetPropertyStatWithPrecision(PROPS.MANA_MAX);
            object level = member.properties[PROPS.LEVEL];
            bool offline = member.status == 0 ? true : false;

            updateData_VitalityStat(uiGroupMembers[index].uiHealth, health, health_max);
            updateData_VitalityStat(uiGroupMembers[index].uiMana, mana, mana_max);

            if (level != null)
                uiGroupMembers[index].uiLevel.text = ((int)level).ToString();

            if (updatePortrait)
            {
                AtavismObjectNode objNode = ClientAPI.GetObjectNode(member.oid.ToLong());

                if (objNode != null && objNode.GameObject!=null)
                {
                    Sprite portraitSprite = PortraitManager.Instance.LoadPortrait(objNode.GameObject.GetComponent<AtavismNode>());

                    if (member.properties.ContainsKey(PROPS.PORTRAIT) && member.properties[PROPS.PORTRAIT] != null && ((string)member.properties[PROPS.PORTRAIT]).Length > 0)
                        portraitSprite = PortraitManager.Instance.LoadPortrait((string)member.properties[PROPS.PORTRAIT]);
                    else if (member.properties.ContainsKey(PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT)) && member.properties[PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT)] != null &&
                            ((string)member.properties[PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT)]).Length > 0)
                        portraitSprite = PortraitManager.Instance.LoadPortrait((string)member.properties[PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT)]);
                    else if (ClientAPI.GetObjectNode(member.oid.ToLong()) != null && ClientAPI.GetObjectNode(member.oid.ToLong()).GameObject != null)
                        portraitSprite = PortraitManager.Instance.LoadPortrait(ClientAPI.GetObjectNode(member.oid.ToLong()).GameObject.GetComponent<AtavismNode>());

                    if (portraitSprite == null)
                        StartCoroutine("reloadGroupMemeberPotraitAsync", index);

                    UIToolkit.SetBackgroundImage(uiGroupMembers[index].uiIcon, portraitSprite);
                }
            }

            if (offline)
                uiGroupMembers[index].uiIcon.style.unityBackgroundImageTintColor = UTILS.ChangeColorAlpha(uiGroupMembers[index].uiIcon.resolvedStyle.unityBackgroundImageTintColor, 0.25f);
            else uiGroupMembers[index].uiIcon.style.unityBackgroundImageTintColor = UTILS.ChangeColorAlpha(uiGroupMembers[index].uiIcon.resolvedStyle.unityBackgroundImageTintColor, 1f);

            updateGroupMemberEffects(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void updateGroupMembersEffects()
        {
            for (int n = 0; n < AtavismGroup.Instance.Members.Count; n++)
                updateGroupMemberEffects(n);
        }
        private void updateGroupMemberEffects(int index)
        {
            if (index < AtavismGroup.Instance.Members.Count)
            {
                AtavismObjectNode objNode = ClientAPI.WorldManager.GetObjectNode(AtavismGroup.Instance.Members[index].oid);
                updateEffects(ref uiGroupMembers[index].uiEffectSlots, getEffects(objNode));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private IEnumerator reloadGroupMemeberPotraitAsync(int index)
        {
            yield return new WaitForSecondsRealtime(1f);

            updateData_GroupMember(index, true);

           // Debug.LogError("Reloading portrait!");
        }
        #endregion
        #region Effects
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="effects"></param>
        protected void updateEffects(ref UIAtavismEffectSlot[] slots, AtavismEffect[] effects)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (effects!=null && i < effects.Length)
                {
                    bool passiveEffect = effects[i].Passive;

                    if (!passiveEffect && effects[i].Active == false)
                    {
                        slots[i].Hide();
                    }
                    else
                    {
                        slots[i].SetEffect(effects[i], i);
                        slots[i].Show();
                    }
                }
                else
                {
                    slots[i].Hide();
                }
            }
        }
        protected void updateEffects(ref UIAtavismEffectSlot[] slots, List<AtavismEffect> effects)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < effects.Count)
                {
                    bool passiveEffect = effects[i].Passive;

                    if (!passiveEffect && effects[i].Active == false)
                    {
                        slots[i].Hide();
                    }
                    else
                    {
                        slots[i].SetEffect(effects[i], i);
                        slots[i].Show();
                    }
                }
                else
                {
                    slots[i].Hide();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiEffects"></param>
        /// <param name="uiEffectSlots"></param>
        protected void addEffectSlots(VisualElement[] uiEffects, ref UIAtavismEffectSlot[] uiEffectSlots)
        {
            uiEffectSlots = new UIAtavismEffectSlot[uiEffects.Length];
            for (int n = 0; n < uiEffects.Length; n++)
            {
                uiEffectSlots[n] = uiEffects[n].Q<UIAtavismEffectSlot>();
                uiEffectSlots[n].Initialize(this);
                uiEffectSlots[n].Hide();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected AtavismEffect[] getEffects(AtavismObjectNode node)
        {
            LinkedList<object> effects_prop = new LinkedList<object>();
            float effects_prop_time = 0f;

            if (node !=null && node.PropertyExists("effects"))
            {
                effects_prop = (LinkedList<object>)node.GetProperty("effects");
                if (node.PropertyExists("effects_t"))
                    effects_prop_time = (float)node.GetProperty("effects_t");
            }

            List<AtavismEffect> effects = new List<AtavismEffect>();
            List<int> iconLack = new List<int>();

            foreach (string effectsProp in effects_prop)
            {
                string[] effectData = effectsProp.Split(',');
                int effectID = int.Parse(effectData[0]);
                long duration = long.Parse(effectData[4]);
                bool active = bool.Parse(effectData[5]);
                float secondsLeft = (float)duration / 1000f;
                long length = long.Parse(effectData[6]);

                AtavismEffect effect = null;
                if (effect == null)
                    if (Abilities.Instance.GetEffect(effectID) != null)
                        effect = Abilities.Instance.GetEffect(effectID).Clone();
                if (effect == null)
                {
                    UnityEngine.Debug.LogWarning("Effect " + effectID + " does not exist");
                    continue;
                }

                effect.StackSize = int.Parse(effectData[1]);
                effect.isBuff = bool.Parse(effectData[2]);
                effect.Expiration = Time.time + secondsLeft - (Time.time - effects_prop_time);
                effect.Active = active;
                effect.Length = (float)length / 1000f;
                effect.startTime = long.Parse(effectData[9]);

                effects.Add(effect);
                if (effect.Icon == null)
                {
                    iconLack.Add(effect.id);
                }
            }

            effects = effects.OrderBy(x => x.startTime).ToList();

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
            }

            return effects.ToArray();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual string getMobTitle(AtavismObjectNode node)
        {
            string title = "";
            if (node == null)
                return title;

            object objTitle = node.GetProperty(PROPS.TITLE);
            if (objTitle != null)
                title = (string)objTitle;
            else title = node.Name;
            
   
            if (AtavismGroup.Instance == null)
                return title;
            if (AtavismGroup.Instance.LeaderOid == null)
                return title;

            if (AtavismGroup.Instance.LeaderOid.ToLong() == node.Oid)
                title = LEADER_PREFIX + title;

            return title;
        }
        protected virtual string getMobSubTitle(AtavismObjectNode node)
        {
            string subTitle = "";
            if (node == null)
                return subTitle;

            object objSubTitle = node.GetProperty(PROPS.SUB_TITLE);
            if (objSubTitle != null)
                subTitle = (string)objSubTitle;

            return subTitle;
        }
        protected virtual string getMobSpecies(AtavismObjectNode node)
        {
            string species = "";
            if (node == null)
                return species;

            object objSpecies = node.GetProperty(PROPS.SPECIES);
            if (objSpecies != null)
                species = (string)objSpecies;

            return species;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="value"></param>
        /// <param name="max"></param>
        protected virtual void updateData_VitalityStat(UIAtavismVitalityStatProgressBar ui, object value, object max = null)
        {
            //if (value == null)
               // return;

               if (value != null)
                ui.value = (float)value;
            if (max != null)
                ui.highValue = (float)max;

            //ui.UpdateColors(); if required
        }
        #endregion

        #region Shields

        private void HandleShieldListUpdate(Dictionary<string, object> props)
        {
            string shieldName = (string)props["name"];
            if (uiPlayerShields.ContainsKey(shieldName))
            {
                int sMax = (int)(props["sMax"]);
                int sCur = (int)(props["sCur"]);
                int cMax = (int)(props["cMax"]);
                int cCur = (int)(props["cCur"]);
                if (AtavismLogger.isLogDebug())
                {
                    AtavismLogger.LogDebugMessage("Shield " + shieldName + " sMax=" + sMax + " sCur=" + sCur +
                                                  " cMax=" + cMax + " cCur=" + cCur);
                }

                if (sMax > 0)
                {
                    uiPlayerShields[shieldName].highValue = sMax;
                    uiPlayerShields[shieldName].value = sCur;
                }

                if (cMax > 0)
                {
                    uiPlayerShields[shieldName].highValue = cMax;
                    uiPlayerShields[shieldName].value = cCur;
                }

                if ((sCur == 0 && sMax > 0) || (cCur == 0 && cMax > 0))
                    uiPlayerShields[shieldName].HideVisualElement();
                else
                    uiPlayerShields[shieldName].ShowVisualElement();

            }

        }


        #endregion
        
        #region Dice
        private void Pass()
        {
            /*AtavismGroup.Instance.Pass();
            StopCoroutine(cr);
            if (dicePanel && dicePanel.activeSelf)
            {
                dicePanel.SetActive(false);
            }*/
        }

        private void Roll()
        {
            /*AtavismGroup.Instance.Roll();
            StopCoroutine(cr);
            if (dicePanel && dicePanel.activeSelf)
            {
                dicePanel.SetActive(false);
            }*/
        }

        /*IEnumerator UpdateTimer()
        {
            //  corRuning = true;
            while (Expiration > Time.time)
            {
                float timeLeft = Expiration - Time.time;
                if (timeLeft > 60)
                {
                    int minutes = (int)timeLeft / 60;
                    if (TMPTimeRemaning != null)
                    {
#if AT_I2LOC_PRESET
            TMPTimeRemaning.text =  I2.Loc.LocalizationManager.GetTranslation(timeRemaningPrefix)+" " + (int)minutes + "m";
#else
                        TMPTimeRemaning.text = timeRemaningPrefix + " " + (int)minutes + "m";
#endif
                    }
                }
                else
                {
                    if (TMPTimeRemaning != null)
                    {
#if AT_I2LOC_PRESET
            TMPTimeRemaning.text = I2.Loc.LocalizationManager.GetTranslation(timeRemaningPrefix)+" " + (int)timeLeft + "s";
#else
                        TMPTimeRemaning.text = timeRemaningPrefix + " " + (int)timeLeft + "s";
#endif
                    }
                }
                if (timeRemaning != null)
                    timeRemaning.fillAmount = timeLeft / Length;
                yield return new WaitForSeconds(0.04f);
            }

            if (timeRemaning != null)
                timeRemaning.fillAmount = 1f;
            if (dicePanel && dicePanel.activeSelf)
            {
                dicePanel.SetActive(false);
            }
        }*/
        #endregion
        #region Group Settings
        #endregion
    }
}