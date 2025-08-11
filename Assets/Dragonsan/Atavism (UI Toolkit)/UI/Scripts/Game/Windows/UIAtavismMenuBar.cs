using Atavism;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
   public class UIAtavismMenuBar : UIAtavismWindowBase
    {
        [AtavismSeparator("Base Screen")] public string uiScreenstring = "Screen";

        [AtavismSeparator("Button Strings")] public string uiSlotAdminButtonstring = "toolbar-admin";
        public string uiSlotWorldBuilderButtonstring = "toolbar-worldbuilder";
        public string uiSlotCharacterButtonstring = "toolbar-character";
        public string uiSlotInventoryButtonstring = "toolbar-inventory";
        public string uiSlotSkillsButtonstring = "toolbar-skills";
        public string uiSlotQuestsButtonstring = "toolbar-quests";
        public string uiSlotMailboxButtonstring = "toolbar-mailbox";
        public string uiSlotGuildButtonstring = "toolbar-guild";
        public string uiSlotArenaButtonstring = "toolbar-arena";
        public string uiSlotSocialButtonstring = "toolbar-social";
        public string uiSlotFactionsButtonstring = "toolbar-factions";
        public string uiSlotGearModificationButtonstring = "toolbar-gearmodification";
        public string uiSlotAuctionHouseButtonstring = "toolbar-auctionhouse";
        public string uiSlotCraftingButtonstring = "toolbar-crafting";
        public string uiSlotSkillsv2Buttonstring = "toolbar-skillsv2";
        public string uiSlotAchievementButtonstring = "toolbar-achievements";
        public string uiSlotRankingsButtonstring = "toolbar-rankings";
        public string uiSlotTalentButtonstring = "toolbar-talent";
        public string uiSlotShopListButtonstring = "toolbar-shoplist";
        public string uiSlotClaimListButtonstring = "toolbar-claimlist";
        public string uiSlotCraftingBookButtonstring = "toolbar-crafting-book";
        [AtavismSeparator("Hotkey Strings")] public string uiSlotAdminHotkeystring = "toolbar-hotkey-admin";
        public string uiSlotWorldBuilderHotkeystring = "toolbar-hotkey-worldbuilder";
        public string uiSlotCharacterHotkeystring = "toolbar-hotkey-character";
        public string uiSlotInventoryHotkeystring = "toolbar-hotkey-inventory";
        public string uiSlotSkillsHotkeystring = "toolbar-hotkey-skills";
        public string uiSlotQuestsHotkeystring = "toolbar-hotkey-quests";
        public string uiSlotMailboxHotkeystring = "toolbar-hotkey-mailbox";
        public string uiSlotGuildHotkeystring = "toolbar-hotkey-guild";
        public string uiSlotArenaHotkeystring = "toolbar-hotkey-arena";
        public string uiSlotSocialHotkeystring = "toolbar-hotkey-social";
        public string uiSlotFactionsHotkeystring = "toolbar-hotkey-factions";
        public string uiSlotGearModificationHotkeystring = "toolbar-hotkey-gearmodification";
        public string uiSlotAuctionHouseHotkeystring = "toolbar-hotkey-auctionhouse";
        public string uiSlotCraftingHotkeystring = "toolbar-hotkey-crafting";
        public string uiSlotSkillsv2Hotkeystring = "toolbar-hotkey-skillsv2";
        public string uiSlotAchievementHotkeystring = "toolbar-hotkey-achievements";
        public string uiSlotRankingsHotkeystring = "toolbar-hotkey-rankings";
        public string uiSlotTalentHotkeystring = "toolbar-hotkey-talent";
        public string uiSlotShopListHotkeystring = "toolbar-hotkey-shoplist";
        public string uiSlotClaimListHotkeystring = "toolbar-hotkey-claimlist";
        public string uiSlotCraftingBookHotkeystring = "toolbar-hotkey-crafting-book";

        [AtavismSeparator("Button Image Strings")]
        public string uiSlotAdminImagestring = "toolbar-button-image-admin";

        public string uiSlotWorldBuilderImagestring = "toolbar-button-image-worldbuilder";
        public string uiSlotCharacterImagestring = "toolbar-button-image-character";
        public string uiSlotInventoryImagestring = "toolbar-button-image-inventory";
        public string uiSlotSkillsImagestring = "toolbar-button-image-skills";
        public string uiSlotQuestsImagestring = "toolbar-button-image-quests";
        public string uiSlotMailboxImagestring = "toolbar-button-image-mailbox";
        public string uiSlotGuildImagestring = "toolbar-button-image-guild";
        public string uiSlotArenaImagestring = "toolbar-button-image-arena";
        public string uiSlotSocialImagestring = "toolbar-button-image-social";
        public string uiSlotFactionsImagestring = "toolbar-button-image-factions";
        public string uiSlotGearModificationImagestring = "toolbar-button-image-gearmodification";
        public string uiSlotAuctionHouseImagestring = "toolbar-button-image-auctionhouse";
        public string uiSlotCraftingImagestring = "toolbar-button-image-crafting";
        public string uiSlotSkillsv2Imagestring = "toolbar-button-image-skillsv2";
        public string uiSlotAchievementImagestring = "toolbar-button-image-achievements";
        public string uiSlotRankingsImagestring = "toolbar-button-image-rankings";
        public string uiSlotTalentImagestring = "toolbar-button-image-talent";
        public string uiSlotShopListImagestring = "toolbar-button-image-shoplist";
        public string uiSlotClaimListImagestring = "toolbar-button-image-claimlist";
        public string uiSlotCraftingBookImagestring = "toolbar-button-image-crafting-book";

        [AtavismSeparator("Atavism Screens")] [SerializeField]
        UIAtavismAdminPanel uiScreenAdmin;

        [SerializeField] UIAtavismWindowBase uiScreenWorldBuilder;
        [SerializeField] UIAtavismWindowBase uiScreenCharacter;
        [SerializeField] UIAtavismInventory UIScreenInventory;
        [SerializeField] UIAtavismWindowBase uiScreenSkills;
        [SerializeField] UIAtavismWindowBase uiScreenQuests;
        [SerializeField] UIAtavismWindowBase uiScreenMailbox;
        [SerializeField] UIAtavismWindowBase uiScreenGuild;
        [SerializeField] UIAtavismArenaList uiScreenArena;
        [SerializeField] UIAtavismWindowBase uiScreenSocial;
        [SerializeField] UIAtavismWindowBase uiScreenFactions;
        [SerializeField] UIAtavismWindowBase uiScreenGearModification;
        [SerializeField] UIAtavismAuction uiScreenAuction;
        [SerializeField] UIAtavismWindowBase uiScreenCrafting;
        [SerializeField] UIAtavismWindowBase uiScreenSkillsv2;
        [SerializeField] UIAtavismWindowBase uiScreenAchievement;
        [SerializeField] UIAtavismWindowBase uiScreenRankings;
        [SerializeField] UIAtavismWindowBase uiScreenTalent;
        [SerializeField] UIAtavismWindowBase uiScreenShopList;
        [SerializeField] UIAtavismWindowBase uiScreenClaimList;
        [SerializeField] UIAtavismWindowBase uiScreenCraftingBook;
        // [SerializeField] public UIAtavismQuestLogPreview uiScreenQuestLogPreview;

        // [AtavismSeparator("Slot Holder Images")]
        // public Texture2D uiSlotTextureAdmin;
        private VisualElement uiSlotTextureHolderAdmin;
        private Label uiSlotLabelAdmin;
        private Button uiSlotButtonAdmin;

        // public Texture2D uiSlotTextureWorldBuilder;
        private VisualElement uiSlotTextureHolderWorldBuilder;
        private Label uiSlotLabelWorldBuilder;
        private Button uiSlotButtonWorldBuilder;

        // public Texture2D uiSlotTextureCharacter;
        private VisualElement uiSlotTextureHolderCharacter;
        private Label uiSlotLabelCharacter;
        private Button uiSlotButtonCharacter;

        // public Texture2D uiSlotTextureInventory;
        private VisualElement uiSlotTextureHolderInventory;
        private Label uiSlotLabelInventory;
        private Button uiSlotButtonInventory;

        // public Texture2D uiSlotTextureSkills;
        private VisualElement uiSlotTextureHolderSkills;
        private Label uiSlotLabelSkills;
        private Button uiSlotButtonSkills;

        // public Texture2D uiSlotTextureQuests;
        private VisualElement uiSlotTextureHolderQuests;
        private Label uiSlotLabelQuests;
        private Button uiSlotButtonQuests;

        // public Texture2D uiSlotTextureMailbox;
        private VisualElement uiSlotTextureHolderMailbox;
        private Label uiSlotLabelMailbox;
        private Button uiSlotButtonMailbox;

        // public Texture2D uiSlotTextureGuild;
        private VisualElement uiSlotTextureHolderGuild;
        private Label uiSlotLabelGuild;
        private Button uiSlotButtonGuild;

        // public Texture2D uiSlotTextureArena;
        private VisualElement uiSlotTextureHolderArena;
        private Label uiSlotLabelArena;
        private Button uiSlotButtonArena;

        // public Texture2D uiSlotTextureSocial;
        private VisualElement uiSlotTextureHolderSocial;
        private Label uiSlotLabelSocial;
        private Button uiSlotButtonSocial;

        // public Texture2D uiSlotTextureFactions;
        private VisualElement uiSlotTextureHolderFactions;
        private Label uiSlotLabelFactions;
        private Button uiSlotButtonFactions;

        // public Texture2D uiSlotTextureGearModification;
        private VisualElement uiSlotTextureHolderGearModification;
        private Label uiSlotLabelGearModification;
        private Button uiSlotButtonGearModification;

        // public Texture2D uiSlotTextureAuctionHouse;
        private VisualElement uiSlotTextureHolderAuctionHouse;
        private Label uiSlotLabelAuctionHouse;
        private Button uiSlotButtonAuctionHouse;

        // public Texture2D uiSlotTextureCrafting;
        private VisualElement uiSlotTextureHolderCrafting;
        private Label uiSlotLabelCrafting;
        private Button uiSlotButtonCrafting;

        // public Texture2D uiSlotTextureSkillsv2;
        private VisualElement uiSlotTextureHolderSkillsv2;
        private Label uiSlotLabelSkillsv2;
        private Button uiSlotButtonSkillsv2;

        // public Texture2D uiSlotTextureAchievement;
        private VisualElement uiSlotTextureHolderAchievement;
        private Label uiSlotLabelAchievement;
        private Button uiSlotButtonAchievement;

        // public Texture2D uiSlotTextureRankings;
        private VisualElement uiSlotTextureHolderRankings;
        private Label uiSlotLabelRankings;
        private Button uiSlotButtonRankings;

        // public Texture2D uiSlotTextureTalent;
        private VisualElement uiSlotTextureHolderTalent;
        private Label uiSlotLabelTalent;
        private Button uiSlotButtonTalent;

        // public Texture2D uiSlotTextureShopList;
        private VisualElement uiSlotTextureHolderShopList;
        private Label uiSlotLabelShopList;
        private Button uiSlotButtonShopList;

        // public Texture2D uiSlotTextureClaimList;
        private VisualElement uiSlotTextureHolderClaimList;
        private Label uiSlotLabelClaimList;
        private Button uiSlotButtonClaimList;

        // public Texture2D uiSlotTextureCraftingBook;
        private VisualElement uiSlotTextureHolderCraftingBook;
        private Label uiSlotLabelCraftingBook;
        private Button uiSlotButtonCraftingBook;

        [AtavismSeparator("Button Images")] public Texture2D uiButtonTextureAdmin;
        public Texture2D uiButtonTextureWorldBuilder;
        public Texture2D uiButtonTextureCharacter;
        public Texture2D uiButtonTextureInventory;
        public Texture2D uiButtonTextureSkills;
        public Texture2D uiButtonTextureQuests;
        public Texture2D uiButtonTextureMailbox;
        public Texture2D uiButtonTextureGuild;
        public Texture2D uiButtonTextureArena;
        public Texture2D uiButtonTextureSocial;
        public Texture2D uiButtonTextureFactions;
        public Texture2D uiButtonTextureGearModification;
        public Texture2D uiButtonTextureAuctionHouse;
        public Texture2D uiButtonTextureCrafting;
        public Texture2D uiButtonTextureSkillsv2;
        public Texture2D uiButtonTextureAchievement;
        public Texture2D uiButtonTextureRankings;
        public Texture2D uiButtonTextureTalent;
        public Texture2D uiButtonTextureShopList;
        public Texture2D uiButtonTextureClaimList;
        public Texture2D uiButtonTextureCraftingBook;

        [AtavismSeparator("Button Description")]
        public string uiButtonDescriptionAdmin = "Admin";

        public string uiButtonDescriptionWorldBuilder = "World Builder";
        public string uiButtonDescriptionCharacter = "Character";
        public string uiButtonDescriptionInventory = "Inventory";
        public string uiButtonDescriptionSkills = "Skills";
        public string uiButtonDescriptionQuests = "Quests";
        public string uiButtonDescriptionMailbox = "Mailbox";
        public string uiButtonDescriptionGuild = "Guild";
        public string uiButtonDescriptionArena = "Arena";
        public string uiButtonDescriptionSocial = "Social";
        public string uiButtonDescriptionFactions = "Factions";
        public string uiButtonDescriptionGearModification = "Gear Modification";
        public string uiButtonDescriptionAuctionHouse = "Auction House";
        public string uiButtonDescriptionCrafting = "Crafting";
        public string uiButtonDescriptionSkillsV2 = "Skills v2";
        public string uiButtonDescriptionAchievement = "Achievement";
        public string uiButtonDescriptionRankings = "Rankings";
        public string uiButtonDescriptionTalent = "Talent";
        public string uiButtonDescriptionShopList = "Shop List";
        public string uiButtonDescriptionClaimList = "Claim List";
        public string uiButtonDescriptionCraftingBook = "Crafting Book";

        [AtavismSeparator("Hide Element Menu")] [SerializeField]
        bool uiHideMenuAdmin = false;

        [SerializeField] bool uiHideMenuWorldBuilder = false;
        [SerializeField] bool uiHideMenuCharacter = false;
        [SerializeField] bool uiHideMenuInventory = false;
        [SerializeField] bool uiHideMenuSkills = false;
        [SerializeField] bool uiHideMenuQuests = false;
        [SerializeField] bool uiHideMenuMailbox = false;
        [SerializeField] bool uiHideMenuGuild = false;
        [SerializeField] bool uiHideMenuArena = false;
        [SerializeField] bool uiHideMenuSocial = false;
        [SerializeField] bool uiHideMenuFactions = false;
        [SerializeField] bool uiHideMenuGearModification = false;
        [SerializeField] bool uiHideMenuAuctionHouse = false;
        [SerializeField] bool uiHideMenuCrafting = false;
        [SerializeField] bool uiHideMenuSkillsV2 = false;
        [SerializeField] bool uiHideMenuAchievement = false;
        [SerializeField] bool uiHideMenuRankings = false;
        [SerializeField] bool uiHideMenuTalent = false;
        [SerializeField] bool uiHideMenuShopList = false;
        [SerializeField] bool uiHideMenuClaimList = false;
        [SerializeField] bool uiHideMenuCraftingBook = false;



        // Start is called before the first frame update
        new void Start()
        {
            //
            base.Start();
            // Show();
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
            {
                // Debug.LogError("UIAtavismAdminActionBar false");
                return false;
            }

            // Debug.LogError("UIAtavismAdminActionBar registerUI Start");
            var thisVisualElement = GetComponent<UIDocument>().rootVisualElement;


            //Admin Button
            uiSlotTextureHolderAdmin = thisVisualElement.Q<VisualElement>(uiSlotAdminImagestring);
            uiSlotLabelAdmin = thisVisualElement.Q<Label>(uiSlotAdminHotkeystring);
            uiSlotButtonAdmin = thisVisualElement.Q<Button>(uiSlotAdminButtonstring);
            if (uiButtonTextureAdmin)
                ApplyTexture(uiSlotButtonAdmin, uiButtonTextureAdmin);
            if (uiSlotButtonAdmin != null && uiHideMenuAdmin)
            {
                uiSlotButtonAdmin.parent.HideVisualElement();
            }

            //World Builder Button
            uiSlotTextureHolderWorldBuilder = thisVisualElement.Q<VisualElement>(uiSlotWorldBuilderImagestring);
            uiSlotLabelWorldBuilder = thisVisualElement.Q<Label>(uiSlotWorldBuilderHotkeystring);
            uiSlotButtonWorldBuilder = thisVisualElement.Q<Button>(uiSlotWorldBuilderButtonstring);
            if (uiButtonTextureWorldBuilder)
                ApplyTexture(uiSlotButtonWorldBuilder, uiButtonTextureWorldBuilder);
            if (uiSlotButtonWorldBuilder != null && uiHideMenuWorldBuilder)
            {
                uiSlotButtonWorldBuilder.parent.HideVisualElement();
            }

            //Character Button
            uiSlotTextureHolderCharacter = thisVisualElement.Q<VisualElement>(uiSlotCharacterImagestring);
            uiSlotLabelCharacter = thisVisualElement.Q<Label>(uiSlotCharacterHotkeystring);
            uiSlotButtonCharacter = thisVisualElement.Q<Button>(uiSlotCharacterButtonstring);
            if (uiButtonTextureCharacter)
                ApplyTexture(uiSlotButtonCharacter, uiButtonTextureCharacter);
            if (uiSlotButtonCharacter != null && uiHideMenuCharacter)
            {
                uiSlotButtonCharacter.parent.HideVisualElement();
            }

            //Inventory Button
            uiSlotTextureHolderInventory = thisVisualElement.Q<VisualElement>(uiSlotInventoryImagestring);
            uiSlotLabelInventory = thisVisualElement.Q<Label>(uiSlotInventoryHotkeystring);
            uiSlotButtonInventory = thisVisualElement.Q<Button>(uiSlotInventoryButtonstring);
            // if (uiButtonKeyInventory != "")
            ApplyText(uiSlotLabelInventory, AtavismSettings.Instance.GetKeySettings().inventory.key.ToString());
            if (uiButtonTextureInventory)
                ApplyTexture(uiSlotButtonInventory, uiButtonTextureInventory);
            if (uiSlotButtonInventory != null && uiHideMenuInventory)
            {
                uiSlotButtonInventory.parent.HideVisualElement();
            }

            //Skills Button
            uiSlotTextureHolderSkills = thisVisualElement.Q<VisualElement>(uiSlotSkillsImagestring);
            uiSlotLabelSkills = thisVisualElement.Q<Label>(uiSlotSkillsHotkeystring);
            uiSlotButtonSkills = thisVisualElement.Q<Button>(uiSlotSkillsButtonstring);
            if (uiButtonTextureSkills)
                ApplyTexture(uiSlotButtonSkills, uiButtonTextureSkills);
            if (uiSlotButtonSkills != null && uiHideMenuSkills)
            {
                uiSlotButtonSkills.parent.HideVisualElement();
            }

            //Quests Button
            uiSlotTextureHolderQuests = thisVisualElement.Q<VisualElement>(uiSlotQuestsImagestring);
            uiSlotLabelQuests = thisVisualElement.Q<Label>(uiSlotQuestsHotkeystring);
            uiSlotButtonQuests = thisVisualElement.Q<Button>(uiSlotQuestsButtonstring);
            if (uiButtonTextureQuests)
                ApplyTexture(uiSlotButtonQuests, uiButtonTextureQuests);
            if (uiSlotButtonQuests != null && uiHideMenuAdmin)
            {
                uiSlotButtonQuests.parent.HideVisualElement();
            }

            //Mailbox
            uiSlotTextureHolderMailbox = thisVisualElement.Q<VisualElement>(uiSlotMailboxImagestring);
            uiSlotLabelMailbox = thisVisualElement.Q<Label>(uiSlotMailboxHotkeystring);
            uiSlotButtonMailbox = thisVisualElement.Q<Button>(uiSlotMailboxButtonstring);
            if (uiButtonTextureMailbox)
                ApplyTexture(uiSlotButtonMailbox, uiButtonTextureMailbox);
            if (uiSlotButtonMailbox != null && uiHideMenuMailbox)
            {
                uiSlotButtonMailbox.parent.HideVisualElement();
            }

            //Guild
            uiSlotTextureHolderGuild = thisVisualElement.Q<VisualElement>(uiSlotGuildImagestring);
            uiSlotLabelGuild = thisVisualElement.Q<Label>(uiSlotGuildHotkeystring);
            uiSlotButtonGuild = thisVisualElement.Q<Button>(uiSlotGuildButtonstring);
            if (uiButtonTextureGuild)
                ApplyTexture(uiSlotButtonGuild, uiButtonTextureGuild);
            if (uiSlotButtonGuild != null && uiHideMenuGuild)
            {
                uiSlotButtonGuild.parent.HideVisualElement();
            }

            //Arena
            uiSlotTextureHolderArena = thisVisualElement.Q<VisualElement>(uiSlotArenaImagestring);
            uiSlotLabelArena = thisVisualElement.Q<Label>(uiSlotArenaHotkeystring);
            uiSlotButtonArena = thisVisualElement.Q<Button>(uiSlotArenaButtonstring);
            if (uiButtonTextureArena)
                ApplyTexture(uiSlotButtonArena, uiButtonTextureArena);
            if (uiSlotButtonArena != null && uiHideMenuArena)
            {
                uiSlotButtonArena.parent.HideVisualElement();
            }

            //Social
            uiSlotTextureHolderSocial = thisVisualElement.Q<VisualElement>(uiSlotSocialImagestring);
            uiSlotLabelSocial = thisVisualElement.Q<Label>(uiSlotSocialHotkeystring);
            uiSlotButtonSocial = thisVisualElement.Q<Button>(uiSlotSocialButtonstring);
            if (uiButtonTextureSocial)
                ApplyTexture(uiSlotButtonSocial, uiButtonTextureSocial);
            if (uiSlotButtonSocial != null && uiHideMenuSocial)
            {
                uiSlotButtonSocial.parent.HideVisualElement();
            }

            //Factions
            uiSlotTextureHolderFactions = thisVisualElement.Q<VisualElement>(uiSlotFactionsImagestring);
            uiSlotLabelFactions = thisVisualElement.Q<Label>(uiSlotFactionsHotkeystring);
            uiSlotButtonFactions = thisVisualElement.Q<Button>(uiSlotFactionsButtonstring);
            if (uiButtonTextureFactions)
                ApplyTexture(uiSlotButtonFactions, uiButtonTextureFactions);
            if (uiSlotButtonFactions != null && uiHideMenuFactions)
            {
                uiSlotButtonFactions.parent.HideVisualElement();
            }

            //Gear Mod
            uiSlotTextureHolderGearModification = thisVisualElement.Q<VisualElement>(uiSlotGearModificationImagestring);
            uiSlotLabelGearModification = thisVisualElement.Q<Label>(uiSlotGearModificationHotkeystring);
            uiSlotButtonGearModification = thisVisualElement.Q<Button>(uiSlotGearModificationButtonstring);
            if (uiButtonTextureGearModification)
                ApplyTexture(uiSlotButtonGearModification, uiButtonTextureGearModification);
            if (uiSlotButtonGearModification != null && uiHideMenuGearModification)
            {
                uiSlotButtonGearModification.parent.HideVisualElement();
            }

            //Auction House
            uiSlotTextureHolderAuctionHouse = thisVisualElement.Q<VisualElement>(uiSlotAuctionHouseImagestring);
            uiSlotLabelAuctionHouse = thisVisualElement.Q<Label>(uiSlotAuctionHouseHotkeystring);
            uiSlotButtonAuctionHouse = thisVisualElement.Q<Button>(uiSlotAuctionHouseButtonstring);
            if (uiButtonTextureAuctionHouse)
                ApplyTexture(uiSlotButtonAuctionHouse, uiButtonTextureAuctionHouse);
            if (uiSlotButtonAuctionHouse != null && uiHideMenuAuctionHouse)
            {
                uiSlotButtonAuctionHouse.parent.HideVisualElement();
            }

            //Crafting
            uiSlotTextureHolderCrafting = thisVisualElement.Q<VisualElement>(uiSlotCraftingImagestring);
            uiSlotLabelCrafting = thisVisualElement.Q<Label>(uiSlotCraftingHotkeystring);
            uiSlotButtonCrafting = thisVisualElement.Q<Button>(uiSlotCraftingButtonstring);
            if (uiButtonTextureCrafting)
                ApplyTexture(uiSlotButtonCrafting, uiButtonTextureCrafting);
            if (uiSlotButtonCrafting != null && uiHideMenuCrafting)
            {
                uiSlotButtonCrafting.parent.HideVisualElement();
            }

            //Skills V2
            uiSlotTextureHolderSkillsv2 = thisVisualElement.Q<VisualElement>(uiSlotSkillsv2Imagestring);
            uiSlotLabelSkillsv2 = thisVisualElement.Q<Label>(uiSlotSkillsv2Hotkeystring);
            uiSlotButtonSkillsv2 = thisVisualElement.Q<Button>(uiSlotSkillsv2Buttonstring);
            if (uiButtonTextureSkillsv2)
                ApplyTexture(uiSlotButtonSkillsv2, uiButtonTextureSkillsv2);
            if (uiSlotButtonSkillsv2 != null && uiHideMenuSkillsV2)
            {
                uiSlotButtonSkillsv2.parent.HideVisualElement();
            }

            //Achievement
            uiSlotTextureHolderAchievement = thisVisualElement.Q<VisualElement>(uiSlotAchievementImagestring);
            uiSlotLabelAchievement = thisVisualElement.Q<Label>(uiSlotAchievementHotkeystring);
            uiSlotButtonAchievement = thisVisualElement.Q<Button>(uiSlotAchievementButtonstring);
            if (uiButtonTextureAchievement)
                ApplyTexture(uiSlotButtonAchievement, uiButtonTextureAchievement);
            if (uiSlotButtonAchievement != null && uiHideMenuAchievement)
            {
                uiSlotButtonAchievement.parent.HideVisualElement();
            }

            //Rankings
            uiSlotTextureHolderRankings = thisVisualElement.Q<VisualElement>(uiSlotRankingsImagestring);
            uiSlotLabelRankings = thisVisualElement.Q<Label>(uiSlotRankingsHotkeystring);
            uiSlotButtonRankings = thisVisualElement.Q<Button>(uiSlotRankingsButtonstring);
            if (uiButtonTextureRankings)
                ApplyTexture(uiSlotButtonRankings, uiButtonTextureRankings);
            if (uiSlotButtonRankings != null && uiHideMenuRankings)
            {
                uiSlotButtonRankings.parent.HideVisualElement();
            }

            // Talent
            uiSlotTextureHolderTalent = thisVisualElement.Q<VisualElement>(uiSlotTalentImagestring);
            uiSlotLabelTalent = thisVisualElement.Q<Label>(uiSlotTalentHotkeystring);
            uiSlotButtonTalent = thisVisualElement.Q<Button>(uiSlotTalentButtonstring);
            if (uiButtonTextureTalent)
                ApplyTexture(uiSlotButtonTalent, uiButtonTextureTalent);
            if (uiSlotButtonTalent != null && uiHideMenuTalent)
            {
                uiSlotButtonTalent.parent.HideVisualElement();
            }

            //Shopping List
            uiSlotTextureHolderShopList = thisVisualElement.Q<VisualElement>(uiSlotShopListImagestring);
            uiSlotLabelShopList = thisVisualElement.Q<Label>(uiSlotShopListHotkeystring);
            uiSlotButtonShopList = thisVisualElement.Q<Button>(uiSlotShopListButtonstring);
            if (uiButtonTextureShopList)
                ApplyTexture(uiSlotButtonShopList, uiButtonTextureShopList);
            if (uiSlotButtonShopList != null && uiHideMenuShopList)
            {
                uiSlotButtonShopList.parent.HideVisualElement();
            }

            // Claim List
            uiSlotTextureHolderClaimList = thisVisualElement.Q<VisualElement>(uiSlotClaimListImagestring);
            uiSlotLabelClaimList = thisVisualElement.Q<Label>(uiSlotClaimListHotkeystring);
            uiSlotButtonClaimList = thisVisualElement.Q<Button>(uiSlotClaimListButtonstring);
            if (uiButtonTextureClaimList)
                ApplyTexture(uiSlotButtonClaimList, uiButtonTextureClaimList);
            if (uiSlotButtonClaimList != null && uiHideMenuClaimList)
            {
                uiSlotButtonClaimList.parent.HideVisualElement();
            }

            // Crafting Book
            uiSlotTextureHolderCraftingBook = thisVisualElement.Q<VisualElement>(uiSlotCraftingBookImagestring);
            uiSlotLabelCraftingBook = thisVisualElement.Q<Label>(uiSlotCraftingBookHotkeystring);
            uiSlotButtonCraftingBook = thisVisualElement.Q<Button>(uiSlotCraftingBookButtonstring);
            if (uiButtonTextureCraftingBook)
                ApplyTexture(uiSlotButtonCraftingBook, uiButtonTextureCraftingBook);
            if (uiSlotButtonCraftingBook != null && uiHideMenuCraftingBook)
            {
                uiSlotButtonCraftingBook.parent.HideVisualElement();
            }

            //   Debug.LogError("UIAtavismAdminActionBar registerUI End");
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            if (uiSlotButtonAdmin != null)
            {
                uiSlotButtonAdmin.clicked += AdminPanel;
                uiSlotButtonAdmin.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionAdmin);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonAdmin);

                });
                uiSlotButtonAdmin.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });
            }

            if (uiSlotButtonWorldBuilder != null)
            {
                uiSlotButtonWorldBuilder.clicked += WorldBuilderPanel;
                uiSlotButtonWorldBuilder.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionWorldBuilder);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonWorldBuilder);

                });
                uiSlotButtonWorldBuilder.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonCharacter != null)
            {
                uiSlotButtonCharacter.clicked += CharacterPanel;
                uiSlotButtonCharacter.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionCharacter);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonCharacter);

                });
                uiSlotButtonCharacter.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonInventory != null)
            {
                uiSlotButtonInventory.clicked += InventoryPanel;
                uiSlotButtonInventory.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionInventory);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonInventory);

                });
                uiSlotButtonInventory.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonSkills != null)
            {
                uiSlotButtonSkills.clicked += SkillPanel;
                uiSlotButtonSkills.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionSkills);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonSkills);

                });
                uiSlotButtonSkills.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonQuests != null)
            {
                uiSlotButtonQuests.clicked += QuestPanel;
                uiSlotButtonQuests.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionQuests);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonQuests);

                });
                uiSlotButtonQuests.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonMailbox != null)
            {
                uiSlotButtonMailbox.clicked += MailboxPanel;
                uiSlotButtonMailbox.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionMailbox);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonMailbox);

                });
                uiSlotButtonMailbox.RegisterCallback<PointerLeaveEvent>(
                    (e) => { UIAtavismMiniTooltip.Instance.Hide(); });
            }

            if (uiSlotButtonGuild != null)
            {
                uiSlotButtonGuild.clicked += GuildPanel;
                uiSlotButtonGuild.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionGuild);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonGuild);

                });
                uiSlotButtonGuild.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });
            }

            if (uiSlotButtonArena != null)
            {
                uiSlotButtonArena.clicked += ArenaPanel;
                uiSlotButtonArena.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionArena);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonArena);

                });
                uiSlotButtonArena.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });
            }

            if (uiSlotButtonSocial != null)
            {
                uiSlotButtonSocial.clicked += SocialPanel;
                uiSlotButtonSocial.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionSocial);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonSocial);

                });
                uiSlotButtonSocial.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonFactions != null)
            {


                uiSlotButtonFactions.clicked += FactionPanel;
                uiSlotButtonFactions.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionFactions);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonFactions);

                });
                uiSlotButtonFactions.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonGearModification != null)
            {
                uiSlotButtonGearModification.clicked += GearModificationPanel;
                uiSlotButtonGearModification.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionGearModification);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonGearModification);

                });
                uiSlotButtonGearModification.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonAuctionHouse != null)
            {
                uiSlotButtonAuctionHouse.clicked += AuctionPanel;
                uiSlotButtonAuctionHouse.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionAuctionHouse);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonAuctionHouse);

                });
                uiSlotButtonAuctionHouse.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonCrafting != null)
            {
                uiSlotButtonCrafting.clicked += CraftingPanel;
                uiSlotButtonCrafting.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionCrafting);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonCrafting);

                });
                uiSlotButtonCrafting.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonSkillsv2 != null)
            {
                uiSlotButtonSkillsv2.clicked += Skillsv2Panel;
                uiSlotButtonSkillsv2.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionSkillsV2);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonSkillsv2);

                });
                uiSlotButtonSkillsv2.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonAchievement != null)
            {
                uiSlotButtonAchievement.clicked += AchievementPanel;
                uiSlotButtonAchievement.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionAchievement);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonAchievement);

                });
                uiSlotButtonAchievement.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonRankings != null)
            {
                uiSlotButtonRankings.clicked += RankingsPanel;
                uiSlotButtonRankings.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionRankings);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonRankings);

                });
                uiSlotButtonRankings.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonTalent != null)
            {
                uiSlotButtonTalent.clicked += TalentPanel;
                uiSlotButtonTalent.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionTalent);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonTalent);

                });
                uiSlotButtonTalent.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonShopList != null)
            {
                uiSlotButtonShopList.clicked += ShopListPanel;
                uiSlotButtonShopList.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionShopList);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonShopList);

                });
                uiSlotButtonShopList.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonClaimList != null)
            {
                uiSlotButtonClaimList.clicked += ClaimListPanel;
                uiSlotButtonClaimList.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionClaimList);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonClaimList);

                });
                uiSlotButtonClaimList.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }

            if (uiSlotButtonCraftingBook != null)
            {
                uiSlotButtonCraftingBook.clicked += CraftingBookPanel;
                uiSlotButtonCraftingBook.RegisterCallback<PointerEnterEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.SetDescription(uiButtonDescriptionCraftingBook);
                    UIAtavismMiniTooltip.Instance.Show(uiSlotButtonCraftingBook);

                });
                uiSlotButtonCraftingBook.RegisterCallback<PointerLeaveEvent>((e) =>
                {
                    UIAtavismMiniTooltip.Instance.Hide();
                });
            }
            //    Debug.LogError("UIAtavismAdminActionBar registerEvents End");
        }

        private void AuctionPanel()
        {
            // if(uiScreenAuction!=null)
            uiScreenAuction.Toggle();
        }

        private void ArenaPanel()
        {
            //   Debug.LogError("MenuBar Arena panel" );
            // if(uiScreenArena!=null)
            uiScreenArena.Toggle();
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            if (uiSlotButtonAdmin != null) uiSlotButtonAdmin.clicked -= AdminPanel;
            if (uiSlotButtonWorldBuilder != null) uiSlotButtonWorldBuilder.clicked -= WorldBuilderPanel;
            if (uiSlotButtonCharacter != null) uiSlotButtonCharacter.clicked -= CharacterPanel;
            if (uiSlotButtonInventory != null) uiSlotButtonInventory.clicked -= InventoryPanel;
            if (uiSlotButtonSkills != null) uiSlotButtonSkills.clicked -= SkillPanel;
            if (uiSlotButtonQuests != null) uiSlotButtonQuests.clicked -= QuestPanel;
            if (uiSlotButtonMailbox != null) uiSlotButtonMailbox.clicked -= MailboxPanel;
            if (uiSlotButtonGuild != null) uiSlotButtonGuild.clicked -= GuildPanel;
            if (uiSlotButtonArena != null) uiSlotButtonArena.clicked -= ArenaPanel;
            if (uiSlotButtonSocial != null) uiSlotButtonSocial.clicked -= SocialPanel;
            if (uiSlotButtonFactions != null) uiSlotButtonFactions.clicked -= FactionPanel;
            if (uiSlotButtonGearModification != null) uiSlotButtonGearModification.clicked -= GearModificationPanel;
            if (uiSlotButtonAuctionHouse != null) uiSlotButtonAuctionHouse.clicked -= AuctionPanel;
            if (uiSlotButtonCrafting != null) uiSlotButtonCrafting.clicked -= CraftingPanel;
            if (uiSlotButtonSkillsv2 != null) uiSlotButtonSkillsv2.clicked -= Skillsv2Panel;
            if (uiSlotButtonAchievement != null) uiSlotButtonAchievement.clicked -= AchievementPanel;
            if (uiSlotButtonRankings != null) uiSlotButtonRankings.clicked -= RankingsPanel;
            if (uiSlotButtonTalent != null) uiSlotButtonTalent.clicked -= TalentPanel;
            if (uiSlotButtonShopList != null) uiSlotButtonShopList.clicked -= ShopListPanel;
            if (uiSlotButtonClaimList != null) uiSlotButtonClaimList.clicked -= ClaimListPanel;
            if (uiSlotButtonCraftingBook != null) uiSlotButtonCraftingBook.clicked -= CraftingBookPanel;

            //   Debug.LogError("UIAtavismAdminActionBar unregisterEvents End");
        }

        public void CraftingPanel()
        {
            uiScreenCrafting.Toggle();
        }

        private void CraftingBookPanel()
        {
            uiScreenCraftingBook.Toggle();
        }

        private void ShopListPanel()
        {
            uiScreenShopList.Toggle();
        }

        private void ClaimListPanel()
        {
            uiScreenClaimList.Toggle();
        }

        private void TalentPanel()
        {
            uiScreenTalent.Toggle();
        }

        private void RankingsPanel()
        {
            uiScreenRankings.Toggle();
        }

        private void AchievementPanel()
        {
            uiScreenAchievement.Toggle();
        }

        private void Skillsv2Panel()
        {
            uiScreenSkillsv2.Toggle();
        }

        private void GearModificationPanel()
        {
            uiScreenGearModification.Toggle();
        }

        private void FactionPanel()
        {
            uiScreenFactions.Toggle();
        }

        private void SocialPanel()
        {
            uiScreenSocial.Toggle();
        }

        private void GuildPanel()
        {
            uiScreenGuild.Toggle();
        }

        private void MailboxPanel()
        {
            uiScreenMailbox.Toggle();
        }

        private void QuestPanel()
        {
            uiScreenQuests.Toggle();
        }

        private void SkillPanel()
        {
            uiScreenSkills.Toggle();
        }

        private void WorldBuilderPanel()
        {
            uiScreenWorldBuilder.Toggle();
        }

        private void InventoryPanel()
        {
            UIScreenInventory.Toggle();
        }


        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            return true;
        }

        protected void AdminPanel()
        {
            uiScreenAdmin.Toggle();

        }

        protected void CharacterPanel()
        {
            uiScreenCharacter.Toggle();
            // UIAtavismCharacterProfileManager.Instance.Toggle();
        }


        // Update is called once per frame
        new void Update()
        {
            base.Update();
            int adminLevel = 0;
            try
            {
                adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            }
            catch (Exception e)
            {
            }

            if (adminLevel >= 3 && uiSlotButtonAdmin != null)
            {
                uiSlotButtonAdmin.parent.ShowVisualElement();
            }
            else if (uiSlotButtonAdmin != null)
                uiSlotButtonAdmin.parent.HideVisualElement();

            if ((Input.GetKeyDown(AtavismSettings.Instance.openToolBarMenuKey) &&
                 !AtavismSettings.Instance.useSameKeyForBarMenuFromGameSetting) ||
                (Input.GetKeyDown(AtavismSettings.Instance.openGameSettingsKey) &&
                 AtavismSettings.Instance.useSameKeyForBarMenuFromGameSetting))
            {
                Toggle();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

        }

    }
}