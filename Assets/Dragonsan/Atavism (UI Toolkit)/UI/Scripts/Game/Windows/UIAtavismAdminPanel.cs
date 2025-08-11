using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public enum AdminChooseType
    {
        Currency,
        Item,
        Skill,
        Weather
    }
    public class AdminChooseData //: ScriptableObject
    {
        public string m_Name;
        public Sprite m_Image;
        public Color m_Quality;
        public int m_id;
        public AdminChooseType m_type;
    }
    public class UIAtavismAdminPanel : UIAtavismWindowBase
    {

        protected override void OnEnable()
        {
            base.OnEnable();
           // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("AdminWindow");
           // Hide();
            
        }


       

        public static UIAtavismAdminPanel Instance => (UIAtavismAdminPanel)instance;
        static UIAtavismAdminPanel instance;

        [AtavismSeparator("Status")]
        public string ui_gmStatusText_string = "GMStatus-label";
        public string ui_instanceText_string = "Instance-label";
        public string ui_positionText_string = "Position-label";

        public string uiGmStatusButton_string = "GMStatus-button";
        // public string ui_gmStatusLabel_string = "gmStatus-label";
        // public string ui_instanceLabel_string = "instance-label";
        // public string ui_positionLabel_string = "position-label";
        
        [AtavismSeparator("Section Buttons")]
        public string uiTeleportButton_string = "TeleportOptions-button";
        public string uiGainCommandsButton_string = "GainCommands-button";
        public string uiMobSpawnerButton_string = "MobSpawner-button";
        public string uiWeatherButton_string = "Weather-button";
        public string uiServerCommanButton_string = "ServerCommand-button";

        [AtavismSeparator("Teleport")]
        public string ui_teleportPanel_string = "Teleport-panel";
        public string ui_teleportToPlayerField_string = "Teleport-field";
        public string uiTeleportToPlayerButton_string = "Button-teleport-player";
        public string ui_summonPlayerField_string = "Summon-field";
        public string uiSummonPlayerButton_string = "Button-summon-player";
        public string ui_changeInstanceField_string = "Instance-field";
        public string uiChangeInstanceButton_string = "Button-change-instance";
        public string ui_gotoXField_string = "GoToPositionX-field";
        public string ui_gotoYField_string = "GoToPositionY-field";
        public string ui_gotoZField_string = "GoToPositionZ-field";
        public string uiGoToPositionButton_string = "Button-goto-position";
        public string ui_instanceLocs_string = "Location-dropdown";
        public string uiGoToLocationButton_string = "Button-goto-location";
        
        [AtavismSeparator("Gain")]
        public string ui_gainCommandsPanel_string = "GainCommands-panel";
        public string ui_expField_string = "Text-Experience";
        public string ui_gainExpButton_string = "Button-Experience";

        public string ui_chooseItemButton_string = "button-item-select";
        public string ui_itemCountField_string = "field-item";
        public string ui_gainItemButton_string = "button-item";
        
        public string ui_chooseCurrencyButton_string = "button-currency-select";
        public string ui_currencyCountField_string = "field-currency";
        public string ui_gainCurrencyButton_string = "button-currency";
        
        public string ui_chooseSkillButton_string = "button-skill-select";
        public string ui_skillCountField_string = "field-skill";
        public string ui_gainSkillButton_string = "button-skill";
        
        public string ui_choosePanel_string = "Choose-panel";
        public VisualTreeAsset m_chooseListEntryTemplate;
        
        public string ui_itemIconCountText_string = "item-icon-loaded";
        public string ui_skillIconCountText_string = "skill-icon-loaded";
        public string ui_currencyIconCountText_string = "currency-icon-loaded";
        
        public string ui_iconLoadPartCountField_string = "number-icon";
        
        public string ui_itemIconLoadButton_string = "button-load-icon-item";
        public string ui_skillIconLoadButton_string = "button-load-icon-skill";
        public string ui_currencyIconLoadButton_string = "button-load-icon-currency";
        [AtavismSeparator("Weather")]
        public string ui_weatherCommandsPanel_string = "Weather-panel";
        public string ui_yearField_string = "time-year-text";
        public string ui_monthField_string = "time-month-text";
        public string ui_dayField_string = "time-day-text";
        public string ui_hourField_string = "time-hour-text";
        public string ui_minuteField_string = "time-minute-text";
        public string ui_setTimeButton_string = "button-timeset";
        public string ui_getTimeButton_string = "button-timeget";
        
        public string ui_chooseWeatherButton_string = "button-weather-profile";
        public string ui_chooseWeatherButtonSet_string = "button-weather-profile-set";
        
        public string ui_chooseTitle_string = "";
        public string ui_filterTextField_string = "";
        [AtavismSeparator("Server")]
        public string ui_serverCommandsPanel_string = "ServerCommands-panel";
        public string ui_serverMessageField_string = "text-message-input";
        public string ui_serverCountdowanField_string = "countdown-time";
        public string ui_serverScheduleField_string = "schedule-field";
        public string ui_serverCommandProfile_string = "profile-dropdown" ;
        public string ui_serverRestartToggle_string = "close-server-toggle";
        public string ui_serverShutdownButton_string = "send-shutdown-button";
        public string ui_serverReloadButton_string = "reload-button";
        public string ui_serverReloadProgressPanel_string = "reloading-panel";
        public string ui_serverReloadProgressSlider_string = "reloading-progress";


       // public VisualElement titleBar;
       
        // public Label gmStatusLabel;
        // public Label instanceLabel;
        // public Label positionLabel;

        public Label gmStatusText;
        public Label instanceText;
        public Label positionText;
        
        public Button statusToggleButton;
        public Button teleportButton;
        public Button gainCommandsButton;
        public Button mobSpawnerButton;
        public Button weatherButton;
        public Button serverCommansButton;


        [AtavismSeparator("Teleport Commands")]
        public VisualElement teleportPanel;
        public TextField teleportToPlayerField;
        public TextField summonPlayerField;
        public TextField changeInstanceField;
        public TextField gotoXField;
        public TextField gotoYField;
        public TextField gotoZField;
        public UIDropdown instanceLocs;
        public Button teleportToPlayerButton ;
        public Button summonPlayerButton ;
        public Button changeInstancePlayerButton ;
        public Button goToPositionButton;
        public Button goToLocationButton ;
        
        [AtavismSeparator("Gain Commands")]
        public VisualElement gainCommandsPanel;
        public TextField expField;
        public Button gainExpButton ;
        
        public Button chooseItemButton;
        public TextField itemCountField;
        public Button gainItemButton ;
        
        public Button chooseCurrencyButton;
        public TextField currencyCountField;
        public Button gainCurrencyButton;
        
        public Button chooseSkillButton;
        public TextField skillCountField;
        public Button gainSkillButton;
        
        public VisualElement choosePanel;
        private ListView choosePanelList;
        private TextField choosePanelSearcheField;
        private Button choosePanelCloseButton;
        
        public Label itemIconCountText;
        public Label skillIconCountText;
        public Label currencyIconCountText;
        
        public TextField iconLoadPartCountField;//?
        
        public Button itemIconLoadButton;
        public Button skillIconLoadButton;
        public Button currencyIconLoadButton;

      
           
          
           
        
        
        [AtavismSeparator("Weather Commands")]
        public VisualElement weatherCommandsPanel;
        public TextField yearField;
        public TextField monthField;
        public TextField dayField;
        public TextField hourField;
        public TextField minuteField;
        public Button setTimeButton;
        public Button getTimeButton;
        public Button chooseWeatherButton;
        public Button setWeatherButton;
        [AtavismSeparator("Choose Window")]
        public TextField chooseTitle;
        public TextField filterTextField;
        [AtavismSeparator("Server Commands")]
        public VisualElement serverCommandsPanel;
        public TextField serverMessageField;
        public TextField serverCountdowanField;
        public TextField serverScheduleField;
        public UIDropdown serverCommandProfile;
        public Toggle serverRestartToggle;


        public Button serverShutdownButton;
        public Button serverReloadButton;
        public VisualElement serverReloadProgressPanel;
        public UIProgressBar serverReloadProgressSlider;




        bool gmActive = false;
        AdminChooseType chooseType = AdminChooseType.Currency;
        int currencyID = -1;
        int itemID = -1;
        int skillID = -1;
        int profileId = -1;

        private List<AdminChooseData> m_chooseList = new List<AdminChooseData>();
        string actLoc = "";

        private bool autoItemIconGet = false;
        private bool autoSkillIconGet = false;
        private bool autoCurrencyIconGet = false;
        #region Initiate

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            gmStatusText = uiDocument.rootVisualElement.Query<Label>(ui_gmStatusText_string);
            instanceText = uiDocument.rootVisualElement.Query<Label>(ui_instanceText_string);
            positionText = uiDocument.rootVisualElement.Query<Label>(ui_positionText_string);

            statusToggleButton = uiDocument.rootVisualElement.Query<Button>(uiGmStatusButton_string);
            if (statusToggleButton != null)
                statusToggleButton.clicked += ToggleGMMode;
            teleportButton = uiDocument.rootVisualElement.Query<Button>(uiTeleportButton_string);
            gainCommandsButton = uiDocument.rootVisualElement.Query<Button>(uiGainCommandsButton_string);
            mobSpawnerButton = uiDocument.rootVisualElement.Query<Button>(uiMobSpawnerButton_string);
            weatherButton = uiDocument.rootVisualElement.Query<Button>(uiWeatherButton_string);
            serverCommansButton = uiDocument.rootVisualElement.Query<Button>(uiServerCommanButton_string);

            if (teleportButton != null)
                teleportButton.clicked += ShowTeleportOptions;
            if (gainCommandsButton != null)
                gainCommandsButton.clicked += ShowGainCommands;
            if (mobSpawnerButton != null)
                mobSpawnerButton.clicked += ShowSpawner;
            if (weatherButton != null)
                weatherButton.clicked += ShowWeatherCommands;
            if (serverCommansButton != null)
                serverCommansButton.clicked += ShowServerCommands;

            teleportPanel = uiDocument.rootVisualElement.Query<VisualElement>(ui_teleportPanel_string);
            teleportToPlayerField = uiDocument.rootVisualElement.Query<TextField>(ui_teleportToPlayerField_string);
            teleportToPlayerButton = uiDocument.rootVisualElement.Query<Button>(uiTeleportToPlayerButton_string);
            summonPlayerField = uiDocument.rootVisualElement.Query<TextField>(ui_summonPlayerField_string);
            summonPlayerButton = uiDocument.rootVisualElement.Query<Button>(uiSummonPlayerButton_string);
            changeInstanceField = uiDocument.rootVisualElement.Query<TextField>(ui_changeInstanceField_string);
            changeInstancePlayerButton = uiDocument.rootVisualElement.Query<Button>(uiChangeInstanceButton_string);
            gotoXField = uiDocument.rootVisualElement.Query<TextField>(ui_gotoXField_string);
            gotoYField = uiDocument.rootVisualElement.Query<TextField>(ui_gotoYField_string);
            gotoZField = uiDocument.rootVisualElement.Query<TextField>(ui_gotoZField_string);
            goToPositionButton = uiDocument.rootVisualElement.Query<Button>(uiGoToPositionButton_string);
            instanceLocs = uiDocument.rootVisualElement.Query<UIDropdown>(ui_instanceLocs_string);
            instanceLocs.Screen = uiScreen;
            goToLocationButton = uiDocument.rootVisualElement.Query<Button>(uiGoToLocationButton_string);

            if (teleportToPlayerButton != null)
                teleportToPlayerButton.clicked += TeleportToPlayer;
            if (summonPlayerButton != null)
                summonPlayerButton.clicked += SummonPlayer;
            if (changeInstancePlayerButton != null)
                changeInstancePlayerButton.clicked += ChangeInstance;
            if (goToPositionButton != null)
                goToPositionButton.clicked += GotoPosition;
            if (goToLocationButton != null)
                goToLocationButton.clicked += GoToInstanceLoc;



            gainCommandsPanel = uiDocument.rootVisualElement.Query<VisualElement>(ui_gainCommandsPanel_string);
            expField = uiDocument.rootVisualElement.Query<TextField>(ui_expField_string);
            gainExpButton = uiDocument.rootVisualElement.Query<Button>(ui_gainExpButton_string);

            chooseItemButton = uiDocument.rootVisualElement.Query<Button>(ui_chooseItemButton_string);
            itemCountField = uiDocument.rootVisualElement.Query<TextField>(ui_itemCountField_string);
            gainItemButton = uiDocument.rootVisualElement.Query<Button>(ui_gainItemButton_string);

            chooseCurrencyButton = uiDocument.rootVisualElement.Query<Button>(ui_chooseCurrencyButton_string);
            currencyCountField = uiDocument.rootVisualElement.Query<TextField>(ui_currencyCountField_string);
            gainCurrencyButton = uiDocument.rootVisualElement.Query<Button>(ui_gainCurrencyButton_string);
            chooseSkillButton = uiDocument.rootVisualElement.Query<Button>(ui_chooseSkillButton_string);
            skillCountField = uiDocument.rootVisualElement.Query<TextField>(ui_skillCountField_string);
            gainSkillButton = uiDocument.rootVisualElement.Query<Button>(ui_gainSkillButton_string);

            if (gainExpButton != null) gainExpButton.clicked += GetExperience;
            if (gainItemButton != null) gainItemButton.clicked += GenerateItem;
            if (gainCurrencyButton != null) gainCurrencyButton.clicked += GetCurrency;
            if (gainSkillButton != null) gainSkillButton.clicked += GainSkill;

            if (chooseItemButton != null) chooseItemButton.clicked += ChooseItem;
            if (chooseCurrencyButton != null) chooseCurrencyButton.clicked += ChooseCurrency;
            if (chooseSkillButton != null) chooseSkillButton.clicked += ChooseSkill;

            choosePanel = uiDocument.rootVisualElement.Query<VisualElement>(ui_choosePanel_string);
            choosePanelList = choosePanel.Q<ListView>("choose-list-view");
#if UNITY_6000_0_OR_NEWER               
            ScrollView scrollView = choosePanelList.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            choosePanelSearcheField = choosePanel.Q<TextField>("choose-search-field");
            choosePanelCloseButton = choosePanel.Q<Button>("Window-close-button");
            choosePanelCloseButton.clicked += ChoosePanelClose;
            //choosePanelSearcheField.RegisterCallback<ChangeEvent<string>>(UpdateChooseFilter);
            choosePanelSearcheField.RegisterCallback<KeyUpEvent>(UpdateChooseFilter);
            choosePanelList.selectionChanged += OnChooseElementSelected;
            choosePanelList.fixedItemHeight = 65;



            itemIconCountText = uiDocument.rootVisualElement.Query<Label>(ui_itemIconCountText_string);
            skillIconCountText = uiDocument.rootVisualElement.Query<Label>(ui_skillIconCountText_string);
            currencyIconCountText = uiDocument.rootVisualElement.Query<Label>(ui_currencyIconCountText_string);
            iconLoadPartCountField = uiDocument.rootVisualElement.Query<TextField>(ui_iconLoadPartCountField_string);

            itemIconLoadButton = uiDocument.rootVisualElement.Query<Button>(ui_itemIconLoadButton_string);
            skillIconLoadButton = uiDocument.rootVisualElement.Query<Button>(ui_skillIconLoadButton_string);
            currencyIconLoadButton = uiDocument.rootVisualElement.Query<Button>(ui_currencyIconLoadButton_string);

            if (itemIconLoadButton != null) itemIconLoadButton.clicked += GetItemIcons;
            if (skillIconLoadButton != null) skillIconLoadButton.clicked += GetSkillIcons;
            if (currencyIconLoadButton != null) currencyIconLoadButton.clicked += GetCurrencyIcons;

            weatherCommandsPanel = uiDocument.rootVisualElement.Query<VisualElement>(ui_weatherCommandsPanel_string);
            yearField = uiDocument.rootVisualElement.Query<TextField>(ui_yearField_string);
            monthField = uiDocument.rootVisualElement.Query<TextField>(ui_monthField_string);
            dayField = uiDocument.rootVisualElement.Query<TextField>(ui_dayField_string);
            hourField = uiDocument.rootVisualElement.Query<TextField>(ui_hourField_string);
            minuteField = uiDocument.rootVisualElement.Query<TextField>(ui_minuteField_string);
            setTimeButton = uiDocument.rootVisualElement.Query<Button>(ui_setTimeButton_string);
            getTimeButton = uiDocument.rootVisualElement.Query<Button>(ui_getTimeButton_string);
            if (setTimeButton != null) setTimeButton.clicked += SetWorldTime;
            if (getTimeButton != null) getTimeButton.clicked += GetWorldTime;
            chooseWeatherButton = uiDocument.rootVisualElement.Query<Button>(ui_chooseWeatherButton_string);
            if (chooseWeatherButton != null) chooseWeatherButton.clicked += ChooseWeatherProfile;
            setWeatherButton = uiDocument.rootVisualElement.Query<Button>(ui_chooseWeatherButtonSet_string);
            if (setWeatherButton != null) setWeatherButton.clicked += SetWeatherProfile;
            
            chooseTitle = uiDocument.rootVisualElement.Query<TextField>(ui_chooseTitle_string);
            filterTextField = uiDocument.rootVisualElement.Query<TextField>(ui_filterTextField_string);

            serverCommandsPanel = uiDocument.rootVisualElement.Query<VisualElement>(ui_serverCommandsPanel_string);
            serverMessageField = uiDocument.rootVisualElement.Query<TextField>(ui_serverMessageField_string);
            serverCountdowanField = uiDocument.rootVisualElement.Query<TextField>(ui_serverCountdowanField_string);
            serverScheduleField = uiDocument.rootVisualElement.Query<TextField>(ui_serverScheduleField_string);
            serverCommandProfile = uiDocument.rootVisualElement.Query<UIDropdown>(ui_serverCommandProfile_string);
            serverCommandProfile.Screen = uiScreen;
            serverCommandProfile.RegisterCallback<ChangeEvent<int>>(SetCommandServerProfile);
            serverRestartToggle = uiDocument.rootVisualElement.Query<Toggle>(ui_serverRestartToggle_string);

            serverShutdownButton = uiDocument.rootVisualElement.Query<Button>(ui_serverShutdownButton_string);
            serverReloadButton = uiDocument.rootVisualElement.Query<Button>(ui_serverReloadButton_string);
            if (serverShutdownButton != null) serverShutdownButton.clicked += SendShutdown;
            if (serverReloadButton != null) serverReloadButton.clicked += SendReload;
          //  Debug.LogError("Admin register");
            serverReloadProgressPanel =
                uiDocument.rootVisualElement.Query<VisualElement>(ui_serverReloadProgressPanel_string);
            serverReloadProgressSlider =
                uiDocument.rootVisualElement.Query<UIProgressBar>(ui_serverReloadProgressSlider_string);
           // Debug.LogError("Admin register2 "+serverReloadProgressPanel);

            serverReloadProgressPanel.HideVisualElement();
          //  Debug.LogError("Admin register3");

            return true;

        }


        #endregion

        // Use this for initialization
        void Start()
        {
            base.Start();


            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
           // Debug.LogError("admin start");
            instance = this;
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SKILL_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SETTINGS", this);

            //if (titleBar != null)
            //      titleBar.SetOnPanelClose(Hide);
            Hide();
            if (choosePanel != null)
                choosePanel.HideVisualElement();
            if (ClientAPI.GetPlayerObject() != null)
            {
                ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler("gm", GMHandler);
                if (ClientAPI.GetPlayerObject().PropertyExists("gm"))
                {
                    gmActive = (bool)ClientAPI.GetPlayerObject().GetProperty("gm");
                }
            }

            if (gmActive)
            {
                if (gmStatusText != null)
                    gmStatusText.text = "Active";
            }
            else
            {
                if (gmStatusText != null)
                    gmStatusText.text = "Inactive";
            }

            if (AtavismPrefabManager.Instance.PrefabReloading)
            {
                if(serverReloadButton != null)
                    serverReloadButton.SetEnabled(true);
            }
            else
            {
                if(serverReloadButton != null)
                    serverReloadButton.SetEnabled(false);
            }
            if(serverReloadProgressPanel != null)
                serverReloadProgressPanel.HideVisualElement();
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SKILL_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SETTINGS", this);
            if (statusToggleButton != null)
                statusToggleButton.clicked -= ToggleGMMode;

            if (teleportButton != null) teleportButton.clicked -= ShowTeleportOptions;
            if (gainCommandsButton != null) gainCommandsButton.clicked -= ShowGainCommands;
            if (mobSpawnerButton != null) mobSpawnerButton.clicked -= ShowSpawner;
            if (weatherButton != null) weatherButton.clicked -= ShowWeatherCommands;
            if (serverCommansButton != null) serverCommansButton.clicked -= ShowServerCommands;

            if (teleportToPlayerButton != null) teleportToPlayerButton.clicked -= TeleportToPlayer;
            if (summonPlayerButton != null) summonPlayerButton.clicked -= SummonPlayer;
            if (changeInstancePlayerButton != null) changeInstancePlayerButton.clicked -= ChangeInstance;
            if (goToPositionButton != null) goToPositionButton.clicked -= GotoPosition;
            if (goToLocationButton != null) goToLocationButton.clicked -= GoToInstanceLoc;

            if (gainExpButton != null) gainExpButton.clicked -= GetExperience;
            if (gainItemButton != null) gainItemButton.clicked -= GenerateItem;
            if (gainCurrencyButton != null) gainCurrencyButton.clicked -= GetCurrency;
            if (gainSkillButton != null) gainSkillButton.clicked -= GainSkill;

            if (chooseItemButton != null) chooseItemButton.clicked -= ChooseItem;
            if (chooseCurrencyButton != null) chooseCurrencyButton.clicked -= ChooseCurrency;
            if (chooseSkillButton != null) chooseSkillButton.clicked -= ChooseSkill;

            if (choosePanelCloseButton != null) choosePanelCloseButton.clicked -= ChoosePanelClose;

            if (itemIconLoadButton != null) itemIconLoadButton.clicked -= GetItemIcons;
            if (skillIconLoadButton != null) skillIconLoadButton.clicked -= GetSkillIcons;
            if (currencyIconLoadButton != null) currencyIconLoadButton.clicked -= GetCurrencyIcons;
        }

        new public void OnEvent(AtavismEventData eData)
        {
            base.OnEvent(eData);
            if (eData.eventType == "ITEM_ICON_UPDATE")
            {

                if(itemIconCountText != null)
                    itemIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedItemIcons() + " / " + AtavismPrefabManager.Instance.GetCountItems();
                

                if (autoItemIconGet)
                {
                    if (AtavismPrefabManager.Instance.GetCountItems() - AtavismPrefabManager.Instance.GetCountLoadedItemIcons() > 0)
                    {
                        int count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;
                        if (count == -1)
                            count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;

                        if (count < 1)
                        {
                            AtavismPrefabManager.Instance.LoadItemIcons();
                        }
                        else
                        {
                            AtavismPrefabManager.Instance.LoadItemIcons(count);
                        }
                    }
                    else
                    {
                        autoItemIconGet = !autoItemIconGet;


                                itemIconLoadButton.text = "LOAD";
                                itemIconLoadButton.text = "LOAD";
                        
                    }
                }

                if (chooseType == AdminChooseType.Item)
                {
                    UpdateChooseList();
                }
            }
            else
            if (eData.eventType == "Skill_ICON_UPDATE")
            {

                    skillIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedSkillIcons() + " / " + AtavismPrefabManager.Instance.GetCountSkills();
                

                if (autoSkillIconGet)
                {
                    if (AtavismPrefabManager.Instance.GetCountSkills() - AtavismPrefabManager.Instance.GetCountLoadedSkillIcons() > 0)
                    {
                        int count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;
                        if (count == -1)
                            count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;

                        if (count < 1)
                        {
                            AtavismPrefabManager.Instance.LoadSkillIcons();
                        }
                        else
                        {
                            AtavismPrefabManager.Instance.LoadSkillIcons(count);
                        }
                    }
                    else
                    {
                        autoSkillIconGet = !autoSkillIconGet;


                                skillIconLoadButton.text = "LOAD";
                        
                    }
                }

                if (chooseType == AdminChooseType.Skill)
                {
                    UpdateChooseList();
                }
            }
            else
            if (eData.eventType == "CURRENCY_ICON_UPDATE")
            {

                    currencyIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedCurrencyIcons() + " / " + AtavismPrefabManager.Instance.GetCountCurrencies();
                

                if (autoCurrencyIconGet)
                {
                    if (AtavismPrefabManager.Instance.GetCountCurrencies() - AtavismPrefabManager.Instance.GetCountLoadedCurrencyIcons() > 0)
                    {
                        int count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;
                        if (count == -1)
                            count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;

                        if (count < 1)
                        {
                            AtavismPrefabManager.Instance.LoadCurrencyIcons();
                        }
                        else
                        {
                            AtavismPrefabManager.Instance.LoadCurrencyIcons(count);
                        }
                    }
                    else
                    {
                        autoCurrencyIconGet = !autoCurrencyIconGet;


                                currencyIconLoadButton.text = "LOAD";
                        
                    }
                }

                if (chooseType == AdminChooseType.Currency)
                {
                    UpdateChooseList();
                }
            }
            else if (eData.eventType == "SETTINGS")
            {
                //  Debug.LogError("SETTINGS");
                    if (AtavismPrefabManager.Instance.PrefabReloading)
                    {
                        if(serverReloadButton != null)   serverReloadButton.SetEnabled(true);
                }
                else
                {
                    if(serverReloadButton != null)  serverReloadButton.SetEnabled(false);
                }
            }

        }


     

        public override void Show()
        {
            base.Show();
            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 3)
            {
                // GetComponent<CanvasGroup>().alpha = 1f;
                // GetComponent<CanvasGroup>().blocksRaycasts = true;
              
              //  gameObject.SetActive(true);
                AtavismUIUtility.BringToFront(gameObject);
                if (AtavismPrefabManager.Instance.PrefabReloading)
                {
                    if(serverReloadButton != null)  serverReloadButton.SetEnabled(true);
                }
                else
                {
                    if(serverReloadButton != null)  serverReloadButton.SetEnabled(false);
                }
                if(serverReloadProgressPanel != null)   serverReloadProgressPanel.HideVisualElement();
                if (itemIconCountText != null)
                    itemIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedItemIcons() + " / " +
                                             AtavismPrefabManager.Instance.GetCountItems();
                if (skillIconCountText != null)
                    skillIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedSkillIcons() + " / " +
                                              AtavismPrefabManager.Instance.GetCountSkills();
                if (currencyIconCountText != null)
                    currencyIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedCurrencyIcons() + " / " +
                                                 AtavismPrefabManager.Instance.GetCountCurrencies();
            }
            else
            {
                Hide();
            }
        }

        public void Hide()
        {
            base.Hide();
            //gameObject.SetActive(false);
            // GetComponent<CanvasGroup>().alpha = 0f;
            // GetComponent<CanvasGroup>().blocksRaycasts = false;
            serverReloadProgressPanel.HideVisualElement();
          
        }

        // Update is called once per frame
        new void Update()
        {
            base.Update();

            if (showing)
            {
                //			instanceText.text = Application.loadedLevelName;
                if (instanceText != null)
                    instanceText.text = SceneManager.GetActiveScene().name;
                if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetPlayerObject().Position != null)
                {
                    if (positionText != null)
                        positionText.text = ClientAPI.GetPlayerObject().Position.x.ToString("n2") + "," + ClientAPI.GetPlayerObject().Position.y.ToString("n2") + "," + ClientAPI.GetPlayerObject().Position.z.ToString("n2");
                }
                if (!actLoc.Equals(SceneManager.GetActiveScene().name))
                {
                    actLoc = SceneManager.GetActiveScene().name;
                    UpdateLocList();
                }
            }
        }

        public void GMHandler(object sender, PropertyChangeEventArgs args)
        {
            gmActive = (bool)ClientAPI.GetPlayerObject().GetProperty("gm");
            if (gmActive)
            {
                if (gmStatusText != null)
                    gmStatusText.text = "Active";

            }
            else
            {
                if (gmStatusText != null)
                    gmStatusText.text = "Inactive";

            }
        }

        public void ToggleGMMode()
        {
            if (gmActive)
            {
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gm 0");
            }
            else
            {
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gm 1");
            }
        }

        public void ShowTeleportOptions()
        {
           teleportPanel.ShowVisualElement();
             gainCommandsPanel.HideVisualElement();
             weatherCommandsPanel.HideVisualElement();
             serverCommandsPanel.HideVisualElement();
             
             // teleportPanel.style.display = DisplayStyle.Flex;
             // gainCommandsPanel.style.display = DisplayStyle.None;
             // weatherCommandsPanel.style.display = DisplayStyle.None;
             // serverCommandsPanel.style.display = DisplayStyle.None;

        }

        public void ShowGainCommands()
        {
            teleportPanel.HideVisualElement();
            gainCommandsPanel.ShowVisualElement();
            weatherCommandsPanel.HideVisualElement();
            serverCommandsPanel.HideVisualElement();

             if (itemIconCountText != null)
                itemIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedItemIcons() + " / " +
                                          AtavismPrefabManager.Instance.GetCountItems();
             if (skillIconCountText != null)
                skillIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedSkillIcons() + " / " +
                                          AtavismPrefabManager.Instance.GetCountSkills();
             if (currencyIconCountText != null)
                currencyIconCountText.text = AtavismPrefabManager.Instance.GetCountLoadedCurrencyIcons() + " / " +
                                             AtavismPrefabManager.Instance.GetCountCurrencies();

        }

        public void ShowWeatherCommands()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(0, false, "weather.GET_WEATHER_PROFILE", props);
            teleportPanel.HideVisualElement();
            gainCommandsPanel.HideVisualElement();
            weatherCommandsPanel.ShowVisualElement();
            serverCommandsPanel.HideVisualElement();

        }

        public void ShowServerCommands()
        {
            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 5)
            {
                teleportPanel.HideVisualElement();
                gainCommandsPanel.HideVisualElement();
                 weatherCommandsPanel.HideVisualElement();
                serverCommandsPanel.ShowVisualElement();
                UpdateServerCommandProfileList();
            }
        }

        public void ShowSpawner()
        {
            //	Camera.main.GetComponentInChildren<MobCreator>().ToggleBuildingModeEnabled();
            UIAtavismMobCreator.Instance.ToggleBuildingModeEnabled();

        }


        void UpdateServerCommandProfileList()
        {
            if (serverCommandProfile != null)
            {

                List<DsAdminRestart> _profiles = AtavismSettings.Instance.GetAdminRestarts;
                // serverCommandProfile.Clear();
                List<string> list = new List<string>();
                list.Add("None");
                foreach (DsAdminRestart l in _profiles)
                {
                    list.Add(l.Name);
                    // VisualElement thisElement = new UIDropdownPopup() { name = l.Name, viewDataKey = l.Message };
                    // serverCommandProfile.Add(thisElement);
                }
                serverCommandProfile.Options(list);
                if(list.Count>0)
                serverCommandProfile.Index = 0;
            }
        }

        public void SetCommandServerProfile(ChangeEvent<int> evt)
        {
            int id = evt.newValue;
            // Debug.LogError("SetCommandServerProfile: id:"+id);
            List<DsAdminRestart> _profiles = AtavismSettings.Instance.GetAdminRestarts;
            if (id > 0)
            {
                 //  Debug.LogError("SetCommandServerProfile: selected Profile " + _profiles[id - 1].Name);
                if (serverMessageField != null)
                    serverMessageField.value = _profiles[id - 1].Message;
                if (serverScheduleField != null)
                    serverScheduleField.value = _profiles[id - 1].Schedule;
                if (serverCountdowanField != null)
                    serverCountdowanField.value = _profiles[id - 1].CountdownTime.ToString();
            }
             // else Debug.LogError("SetCommandServerProfile: selected Profile -> no profile");

        }

        public void SendShutdown()
        {
            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 5)
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("message", serverMessageField.text);
                props.Add("time", serverCountdowanField.text.Length > 0 ? int.Parse(serverCountdowanField.text) : 10);
                props.Add("schedule", serverScheduleField.text);
                props.Add("restart", serverRestartToggle != null ? serverRestartToggle.value : true);
                NetworkAPI.SendExtensionMessage(0, false, "server.Shutdown", props);

            }

        }


        IEnumerator checkReload()
        {
            //   Debug.LogError("checkReload "+AtavismPrefabManager.Instance.reloaded+" < "+AtavismPrefabManager.Instance.ToReload);
            while (AtavismPrefabManager.Instance.reloaded < AtavismPrefabManager.Instance.ToReload)
            {

                if (serverReloadProgressSlider != null)
                {
                    serverReloadProgressSlider.highValue = AtavismPrefabManager.Instance.ToReload;
                    serverReloadProgressSlider.value = AtavismPrefabManager.Instance.reloaded;
                }

                yield return new WaitForSeconds(0.1f);

                if (serverReloadProgressSlider != null)
                {
                    serverReloadProgressSlider.highValue = AtavismPrefabManager.Instance.ToReload;
                    serverReloadProgressSlider.value = AtavismPrefabManager.Instance.reloaded;
                }
                //      Debug.LogError("checkReload| "+AtavismPrefabManager.Instance.reloaded+" < "+AtavismPrefabManager.Instance.ToReload);

            }
                serverReloadProgressPanel.HideVisualElement();
            string[] args = new string[1];
            args[0] = "Definitions reloaded";
            AtavismEventSystem.DispatchEvent("ANNOUNCEMENT", args);
        }


        public void SendReload()
        {
         //   Debug.LogError("Admin SendReload");

            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 5)
            {
                if (AtavismPrefabManager.Instance.PrefabReloading)
                {
                    Dictionary<string, object> props = new Dictionary<string, object>();
                    NetworkAPI.SendExtensionMessage(0, false, "server.Reload", props);
                    AtavismPrefabManager.Instance.reloaded = 0;
                    //AtavismPrefabManager.Instance.refabReloading = true;
                        serverReloadProgressPanel.ShowVisualElement();
                    StartCoroutine(checkReload());
                }
            }
        }


        public void TeleportToPlayer()
        {
            if (teleportToPlayerField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gotoplayer " + teleportToPlayerField.text);
            if (teleportToPlayerField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gotoplayer " + teleportToPlayerField.text);
        }

        public void SummonPlayer()
        {
            if (summonPlayerField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/summon " + summonPlayerField.text);
            if (summonPlayerField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/summon " + summonPlayerField.text);
        }

        public void ChangeInstance()
        {
            if (changeInstanceField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ci " + changeInstanceField.text);
            if (changeInstanceField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ci " + changeInstanceField.text);
        }

        public void GotoPosition()
        {
            if (gotoXField != null && gotoYField != null && gotoZField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gotopos " + gotoXField.text + " " + gotoYField.text + " " + gotoZField.text);
            if (gotoXField != null && gotoYField != null && gotoZField != null)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gotopos " + gotoXField.text + " " + gotoYField.text + " " + gotoZField.text);
        }

        public void GetExperience()
        {
            int expAmount = 0;
            if (expField != null)
                expAmount = int.Parse(expField.text);
            if (expField != null)
                expAmount = int.Parse(expField.text);
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/getExp " + expAmount);
        }

        void ChoosePanelClose()
        {
            choosePanel.HideVisualElement();
        }
        public void ChooseWeatherProfile()
        {
         //   Debug.LogError("ChooseWeatherProfile");
            if (chooseType != AdminChooseType.Weather)
            {
                choosePanelSearcheField.value = "";
            }
            choosePanelSearcheField.Focus();
            choosePanel.ShowVisualElement();
            chooseType = AdminChooseType.Weather;
            if(chooseTitle != null) chooseTitle.value = "Weather Profile";
            UpdateChooseList();

        }

        public void ChooseCurrency()
        {
            if (chooseType != AdminChooseType.Currency)
            {
                choosePanelSearcheField.value = "";

            }
            choosePanelSearcheField.Focus();
            chooseType = AdminChooseType.Currency;
            choosePanel.ShowVisualElement();
            if (chooseTitle != null)
                chooseTitle.value = "Choose Currency";
            UpdateChooseList();

        }

        public void ChooseItem()
        {
            if (chooseType != AdminChooseType.Item)
            {
                choosePanelSearcheField.value = "";

            }
            choosePanelSearcheField.Focus();
            chooseType = AdminChooseType.Item;
            choosePanel.ShowVisualElement();
            if(chooseTitle != null)  chooseTitle.value = "Choose Item";
            UpdateChooseList();

        }

        public void ChooseSkill()
        {
            if (chooseType != AdminChooseType.Skill)
            {
                choosePanelSearcheField.value = "";

            }
            choosePanelSearcheField.Focus();
            chooseType = AdminChooseType.Skill;
            choosePanel.ShowVisualElement();

            if (chooseTitle != null)
                chooseTitle.value = "Choose Skill";
            UpdateChooseList();

        }

        public void UpdateChooseFilter(KeyUpEvent evt)
        {
            UpdateChooseList();
        }

        public void CurrencySelected(int id)
        {
            currencyID = id;
            choosePanel.HideVisualElement();
            chooseCurrencyButton.text = currencyID.ToString();
        }

        public void ItemSelected(int id)
        {
            itemID = id;
            choosePanel.HideVisualElement();
            chooseItemButton.text = itemID.ToString();
        }

        public void SkillSelected(int id)
        {
            skillID = id;
            choosePanel.HideVisualElement();
            chooseSkillButton.text = skillID.ToString();
        }

        public void WeatherProfileSelected(int id)
        {
            profileId = id;
            choosePanel.HideVisualElement();
            chooseWeatherButton.text = AtavismWeatherManager.Instance.Profiles[profileId].name;
        }

        // public void ItemIconUpdate(int id)
        // {
        //     Dictionary<string, object> ps = new Dictionary<string, object>();
        //     ps.Add("objs", id + ";");
        //     AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "ItemIcon");
        // }
        //
        // public void SkillIconUpdate(int id)
        // {
        //
        // }

        public void GetItemIcons()
        {
            Debug.LogError("GetItemIcons");
            if (AtavismPrefabManager.Instance.GetCountItems() -
                AtavismPrefabManager.Instance.GetCountLoadedItemIcons() == 0)
                return;
            Debug.LogError("GetItemIcons |");
            autoItemIconGet = !autoItemIconGet;
            if (autoItemIconGet)
            {
                int count = iconLoadPartCountField != null
                    ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1
                    : -1;
                if (count == -1)
                    count = iconLoadPartCountField != null
                        ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1
                        : -1;

                if (count < 1)
                {
                    AtavismPrefabManager.Instance.LoadItemIcons();
                }
                else
                {
                    AtavismPrefabManager.Instance.LoadItemIcons(count);
                }

                itemIconLoadButton.text = "STOP";
               
            }
            else
            {
                itemIconLoadButton.text = "LOAD";
            }
        }

        public void GetSkillIcons()
        {
            Debug.LogError("GetSkillIcons");
            if (AtavismPrefabManager.Instance.GetCountSkills() -
                AtavismPrefabManager.Instance.GetCountLoadedSkillIcons() == 0)
            {


                return;
            }

            Debug.LogError("GetSkillIcons |");
            autoSkillIconGet = !autoSkillIconGet;
            if (autoSkillIconGet)
            {
                int count = iconLoadPartCountField != null
                    ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1
                    : -1;
                if (count == -1)
                    count = iconLoadPartCountField != null
                        ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1
                        : -1;

                if (count < 1)
                {
                    AtavismPrefabManager.Instance.LoadSkillIcons();
                }
                else
                {
                    AtavismPrefabManager.Instance.LoadSkillIcons(count);
                }
                skillIconLoadButton.text = "STOP";
            }
            else
            {
               skillIconLoadButton.text = "LOAD";
            }
        }

        public void GetCurrencyIcons()
        {
            Debug.LogError("GetCurrencyIcons");
            if (AtavismPrefabManager.Instance.GetCountCurrencies() - AtavismPrefabManager.Instance.GetCountLoadedCurrencyIcons() == 0)
                return;
            Debug.LogError("GetCurrencyIcons |");
            autoCurrencyIconGet = !autoCurrencyIconGet;
            if (autoCurrencyIconGet)
            {
                int count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;
                if (count == -1)
                    count = iconLoadPartCountField != null ? iconLoadPartCountField.text.Length > 0 ? int.Parse(iconLoadPartCountField.text) : -1 : -1;

                if (count < 1)
                {
                    AtavismPrefabManager.Instance.LoadCurrencyIcons();
                }
                else
                {
                    AtavismPrefabManager.Instance.LoadCurrencyIcons(count);
                }
                currencyIconLoadButton.text = "STOP";


            }
            else
            {

                currencyIconLoadButton.text = "LOAD";
            }

        }


        public void GenerateItem()
        {
            int count = 0;
            if (itemCountField != null && itemCountField.text.Length > 0)
                count = int.Parse(itemCountField.text);
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gi " + itemID + " " + count);
        }

        public void GetCurrency()
        {
            int count = 1;
            if (currencyCountField != null && currencyCountField.text.Length > 0)
                count = int.Parse(currencyCountField.text);
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/getCurrency " + currencyID + " " + count);
        }

        public void GainSkill()
        {
            int count = 0;
            if (skillCountField != null && skillCountField.text.Length > 0)
                count = int.Parse(skillCountField.text);

            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/getSkillCurrent " + skillID + " " + count);
        }

        void UpdateLocList()
        {
            DsAdminPanelSettings _loc = AtavismSettings.Instance.GetAdminLocations();
            if (_loc != null)
            {
                if (instanceLocs != null)
                {
                    
                   
                    // List<VisualElement> _options = new List<VisualElement>();
                    // instanceLocs.Clear();
                    List<string> options = new List<string>();
                    foreach (DsAdminLocation l in _loc.Loc)
                    {
                        // VisualElement thisElement = new UIDropdownPopup() { name = l.Name, viewDataKey = l.Loc.ToString() };
                        // _options.Add(thisElement);
                        options.Add(l.Name);
                        // instanceLocs.Add(thisElement);
                    }
                    instanceLocs.Options(options);
                    //instanceLocs.Add(_options);
                }
            }
            else
            {
                Debug.LogWarning("No Admin Settings for instace " + actLoc);
            }
        }



        public void GoToInstanceLoc()
        {
            if (instanceLocs != null)
            {
                DsAdminPanelSettings _loc = AtavismSettings.Instance.GetAdminLocations();
                if (_loc != null)
                {
                    Vector3 l =  _loc.Loc[instanceLocs.Index].Loc; 
                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/gotopos " + l.x + " " + l.y + " " + l.z);
                }
                else
                {
                    Debug.LogWarning("No Admin Settings for instace " + actLoc);
                }
            }
        }

        public void SetWorldTime()
        {
            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            if (yearField != null)
                year = int.Parse(yearField.text);
            if (monthField != null)
                month = int.Parse(monthField.text);
            if (dayField != null)
                day = int.Parse(dayField.text);
            if (hourField != null)
                hour = int.Parse(hourField.text);
            if (minuteField != null)
                minute = int.Parse(minuteField.text);

            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/setWorldTime " + year + " " + month + " " + day + " " + hour + " " + minute);
        }

        public void GetWorldTime()
        {

            if (yearField != null)
                yearField.value = AtavismWeatherManager.Instance.Year.ToString();
            if (monthField != null)
                monthField.value = AtavismWeatherManager.Instance.Month.ToString();
            if (dayField != null)
                dayField.value = AtavismWeatherManager.Instance.Day.ToString();
            if (hourField != null)
                hourField.value = AtavismWeatherManager.Instance.Hour.ToString();
            if (minuteField != null)
                minuteField.value = AtavismWeatherManager.Instance.Minute.ToString();

        }


        public void SetWeatherProfile()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/setWeatherProfile " + profileId);
        }

        private void OnChooseElementSelected(IEnumerable<object> obj)
        {
            var selected = choosePanelList.selectedItem as AdminChooseData;

            // Handle none-selection (Escape to deselect everything)
            if (selected != null)
            {
                switch (selected.m_type)
                {
                    case AdminChooseType.Currency:
                        CurrencySelected(selected.m_id);
                        break;
                    case AdminChooseType.Skill:
                        SkillSelected(selected.m_id);
                        break;
                    case AdminChooseType.Item:
                        ItemSelected(selected.m_id);
                        break;
                    case AdminChooseType.Weather:
                        WeatherProfileSelected(selected.m_id);
                        break;
                }
            }
        }

        public void UpdateChooseList()
        {
            choosePanelList.Clear();
            m_chooseList.Clear();
            choosePanelList.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAdminChooseListEntry newListEntryLogic = new UIAtavismAdminChooseListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = m_chooseListEntryTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            if (chooseType == AdminChooseType.Currency)
            {
                foreach (Currency currency in Inventory.Instance.Currencies.Values)
                {
                    if (choosePanelSearcheField != null)
                    {
                        if (currency.name.ToLower().Contains(choosePanelSearcheField.text.ToLower()))
                        {
                            AdminChooseData acd = new AdminChooseData();
                            acd.m_Name =  currency.name;
                            acd.m_Image = currency.Icon;
                            acd.m_Quality = new Color(1, 1, 1);
                            acd.m_type = AdminChooseType.Currency;
                            acd.m_id = currency.id;
                            m_chooseList.Add(acd);
                        }
                    }
                    else
                    {
                        AdminChooseData acd = new AdminChooseData();
                        acd.m_Name =  currency.name;
                        acd.m_Image = currency.Icon;
                        acd.m_Quality = new Color(1, 1, 1);
                        acd.m_type = AdminChooseType.Currency;
                        acd.m_id = currency.id;
                        m_chooseList.Add(acd);
                    }
                }
            }
            else if (chooseType == AdminChooseType.Item)
            {
                List<ItemPrefabData> activeItems = new List<ItemPrefabData>();
                foreach (ItemPrefabData item in AtavismPrefabManager.Instance.GetItemPrefabData())
                {
                    if (choosePanelSearcheField != null)
                    {
                        if (item.name.ToLower().Contains(choosePanelSearcheField.text.ToLower()))
                        {
                            AdminChooseData acd = new AdminChooseData();
                            acd.m_Name =  item.name;
                            Sprite s =  AtavismPrefabManager.Instance.GetItemIconByID(item.templateId);
                            if(s == null)
                                s =  AtavismSettings.Instance.defaultItemIcon;
                            acd.m_Image = s; 
                            acd.m_Quality = new Color(1, 1, 1);
                            acd.m_type = AdminChooseType.Item;
                            acd.m_id = item.templateId;
                            m_chooseList.Add(acd);
                        }
                    }
                    else
                    {
                        AdminChooseData acd = new AdminChooseData();
                        acd.m_Name =  item.name;
                        Sprite s =  AtavismPrefabManager.Instance.GetItemIconByID(item.templateId);
                        if(s == null)
                            s =  AtavismSettings.Instance.defaultItemIcon;
                        acd.m_Image = s; 
                        acd.m_Quality = new Color(1, 1, 1);
                        acd.m_type = AdminChooseType.Item;
                        acd.m_id = item.templateId;
                        m_chooseList.Add(acd);
                    }
                }

            }
            else if (chooseType == AdminChooseType.Skill)
            {
                List<Skill> activeSkills = new List<Skill>();
                foreach (Skill skill in Skills.Instance.SkillsList.Values)
                {
                    if (choosePanelSearcheField != null)
                    {
                        if (skill.skillname.ToLower().Contains(choosePanelSearcheField.text.ToLower()))
                        {
                            AdminChooseData acd = new AdminChooseData();
                            acd.m_Name =  skill.skillname;
                            acd.m_Image = skill.Icon;
                            acd.m_Quality = new Color(1, 1, 1);
                            acd.m_type = AdminChooseType.Skill;
                            acd.m_id = skill.id;
                            m_chooseList.Add(acd);
                        }
                    }
                    else
                    {
                        AdminChooseData acd = new AdminChooseData();
                        acd.m_Name =  skill.skillname;
                        acd.m_Image = skill.Icon;
                        acd.m_Quality = new Color(1, 1, 1);
                        acd.m_type = AdminChooseType.Skill;
                        acd.m_id = skill.id;
                        m_chooseList.Add(acd);
                    }
                }
            }
            else if (chooseType == AdminChooseType.Weather)
            {
                List<WeatherProfile> activeProfiles = new List<WeatherProfile>();
                foreach (WeatherProfile profile in AtavismWeatherManager.Instance.Profiles.Values)
                {
                    if (choosePanelSearcheField != null)
                    {
                        if (profile.name.ToLower().Contains(choosePanelSearcheField.text.ToLower()))
                        {
                            AdminChooseData acd = new AdminChooseData();
                            acd.m_Name =  profile.name;
                            acd.m_Image = null;
                            acd.m_Quality = new Color(1, 1, 1);
                            acd.m_type = AdminChooseType.Weather;
                            acd.m_id = profile.id;
                            m_chooseList.Add(acd);
                        }
                    }
                    else
                    {
                        AdminChooseData acd = new AdminChooseData();
                        acd.m_Name =  profile.name;
                        acd.m_Image = null;
                        acd.m_Quality = new Color(1, 1, 1);
                        acd.m_type = AdminChooseType.Weather;
                        acd.m_id = profile.id;
                        m_chooseList.Add(acd);
                    }
                }

            }
            choosePanelList.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAdminChooseListEntry).SetData(m_chooseList[index]);
            };
                
            choosePanelList.itemsSource = m_chooseList;
            choosePanelList.Rebuild();
            choosePanelList.selectedIndex = -1;
            
        }

    }
}