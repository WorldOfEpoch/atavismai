using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Atavism.UI.Game;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismWorldBuilder : UIAtavismWindowBase
    {

        static UIAtavismWorldBuilder instance;
        [SerializeField] private VisualTreeAsset limitElementTemplate;
        [SerializeField] private VisualTreeAsset buildingListElementTemplate;
        [SerializeField] private VisualTreeAsset permissionElementTemplate;
         Label claimName;
         Label claimType;
         Label size;
         Label status;
         Button buyButton;
         Button sellButton;
         Button createButton;
         Button deleteButton;
         Button permissionsButton;
         private VisualElement buildObjectPanel;
         UIAtavismBuildObjectsList buildObjectList;
         UIDropdown buildingCategory;
         VisualElement editObjectPanel;
         Label objectName;
         Label editMode;
         VisualElement constructObjectPanel;
         Label constructObjectName;
         Label objectStatus;
         List<UIAtavismBuildMaterialSlot> requiredItems = new List<UIAtavismBuildMaterialSlot>();
         VisualElement createClaimPanel;
         UITextField createClaimName;
         Label claimSizeText;
         UIDropdown newClaimType;
         UIAtavismCurrencyInputPanel currencyInputPanel;
         VisualElement sellClaimPanel;
         Toggle sellClaimForSaleToggle;
         UIAtavismCurrencyInputPanel sellClaimCurrencyPanel;
        
         UIDropdown taxCurrency;
         UITextField taxAmount;
         UITextField taxInterval;
         UITextField timeWindowToPayTax;
         UITextField timeWindowToSellClaim;
         private SliderInt claimSizeSlider;
         private Toggle forSaleToggle;
         private Toggle ownedToggle;
        
         VisualElement permissionsPanel;
         UIDropdown permissionCategory;
         Label permissionLevel;
         UITextField permissionPlayerName;

         private ListView permissionsList;
         // KeyCode toggleKey;
       
         Button upgradeButton;
         VisualElement upgradePanel;
         Label reqItemTitle;
         List<UIAtavismBuildMaterialSlot> UpgradeRequiredItems = new List<UIAtavismBuildMaterialSlot>();
        UIAtavismCurrencyDisplay upgradeCost;
         Label upgradeSize;
        //Taxs
         Button taxPayButton;
         Label taxStatusTitle;
         Label taxStatusText;
         Label taxInfoTitle;
         Label taxInfoText;
        //Limits
         VisualElement limitsPanel;
         VisualElement limitsGrid;
        Button limitsButton;
         List<UIAtavismLimitDisplay> limitsList;
         VisualElement limitsListGrid;
        private bool showLimits = false;

        //public KeyCode toggleKey;
        string newClaimName;
        int newClaimSize = 10;
        bool playerOwned = true;
        bool forSale = true;
        int currencyID = 0;
        long cost = 0;
        string playerPermissionName;
        int permissionLevelGiven = 0;
        string[] levels = new string[] { "Interaction", "Add Objects", "Edit Objects", "Add Users", "Manage Users" };
        List<string> taxCurrencyOptions = new List<string>();
        List<string> buildingCategoryOptions = new List<string>();
        List<string> newClaimTypeOptions = new List<string>();
        // Use this for initialization
       

        protected override void Update()
        {
            base.Update();
            // if (Input.GetKeyDown(toggleKey) && !ClientAPI.UIHasFocus())
            // {
            //     if (showing)
            //         Hide();
            //     else
            //         Show();
            // }
        }

        protected override void OnEnable()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            base.OnEnable();
            claimSizeSlider.value  =newClaimSize;
            forSaleToggle.SetValueWithoutNotify(forSale);
            ownedToggle.SetValueWithoutNotify(playerOwned);
           
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
//Admin Panel
            claimName = uiWindow.Q<Label>("claim-name");
            claimType = uiWindow.Q<Label>("claim-type");
            size = uiWindow.Q<Label>("size");
            status = uiWindow.Q<Label>("status");
            buyButton = uiWindow.Q<Button>("buy-button");
            buyButton.clicked += BuyClaim;
            sellButton = uiWindow.Q<Button>("sell-button");
            sellButton.clicked += ShowSellClaimPanel;
            createButton = uiWindow.Q<Button>("create-button");
            createButton.clicked += ShowCreateClaimPanel;
            deleteButton = uiWindow.Q<Button>("delete-button");
            deleteButton.clicked += DeleteClaim;
            permissionsButton = uiWindow.Q<Button>("permissions-button");
            permissionsButton.clicked += ShowPermissionsPanel;
            //build Panel
            buildObjectPanel = uiWindow.Q<VisualElement>("build-objects-panel");
            buildObjectList = new UIAtavismBuildObjectsList();
            buildObjectList.SetVisualElement(buildObjectPanel,buildingListElementTemplate);
            Button selectButton = uiWindow.Q<Button>("delete-button");
            selectButton.clicked += StartSelectObject;
            
            buildingCategory = uiWindow.Q<UIDropdown>("category-dropdown");
            buildingCategory.RegisterCallback<ChangeEvent<int>>(ChangeCategory);
            buildingCategory.Screen = uiScreen;
            
            //Edit
            editObjectPanel = uiWindow.Q<VisualElement>("edit-object-panel");
            objectName = uiWindow.Q<Label>("selected-object-name");
            Button moveButton = uiWindow.Q<Button>("move-button");
            moveButton.clicked += StartMoveItem;
            editMode = uiWindow.Q<Label>("mode");
            Button changeModeButton = uiWindow.Q<Button>("change-mode-button");
            changeModeButton.clicked += ChangeEditMode;
            
            objectStatus = uiWindow.Q<Label>("selected-object-status");
            
            Button editRemoveButton = uiWindow.Q<Button>("edit-remove-button");
            editRemoveButton.clicked += RemoveItem;
            Button editSaveButton = uiWindow.Q<Button>("edit-save-button");
            editSaveButton.clicked += SaveObjectChanges;
            
            
            // List<UIAtavismBuildMaterialSlot> requiredItems;
            
            //Construct           
            constructObjectPanel = uiWindow.Q<VisualElement>("construct-panel");
            constructObjectName = uiWindow.Q<Label>("construct-object-name");
            for (int i = 1; i <= 6; i++)
            {
                VisualElement item = constructObjectPanel.Q<VisualElement>("build-material-slot-"+i);
                if (item != null)
                {
                    UIAtavismBuildMaterialSlot m = new UIAtavismBuildMaterialSlot();
                    m.SetVisualElement(item);
                    requiredItems.Add(m);
                }
            }
            
            Button constructCancelButton = uiWindow.Q<Button>("construct-cancel-button");
            constructCancelButton.clicked += ShowObjectList;
            Button constructBuildButton = uiWindow.Q<Button>("construct-build-button");
            constructBuildButton.clicked += BuildClicked;
            Button constructRemoveButton = uiWindow.Q<Button>("construct-remove-button");
            constructRemoveButton.clicked += RemoveItem;
            
            
           //Create Panel 
            createClaimPanel = uiWindow.Q<VisualElement>("create-panel");
            createClaimName = createClaimPanel.Q<UITextField>("create-name");
            claimSizeText = createClaimPanel.Q<Label>("create-size-text");
            claimSizeSlider = createClaimPanel.Q<SliderInt>("create-size" );
            claimSizeSlider.RegisterValueChangedCallback(SetClaimSize);
            newClaimType = createClaimPanel.Q<UIDropdown>("create-type-dropdown");
            newClaimType.Screen = uiScreen;
            
            forSaleToggle= createClaimPanel.Q<Toggle>("for-sale-toggle");
            forSaleToggle.RegisterValueChangedCallback(SetForSale);
            ownedToggle = createClaimPanel.Q<Toggle>("owned-toggle");
            ownedToggle.RegisterValueChangedCallback(SetPlayerOwned);
            VisualElement createCurrency = createClaimPanel.Q<VisualElement>("create-currency-input");
            currencyInputPanel = new UIAtavismCurrencyInputPanel();
            currencyInputPanel.SetVisualElement(createCurrency);
            currencyInputPanel.SetCurrencyReverseOrder = true;
            taxCurrency = createClaimPanel.Q<UIDropdown>("currency-dropdown");
            taxCurrency.Screen = uiScreen;
            
            taxAmount = createClaimPanel.Q<UITextField>("tax-amount");
            taxInterval = createClaimPanel.Q<UITextField>("tax-interval");
            timeWindowToPayTax = createClaimPanel.Q<UITextField>("tax-time-pay");
            timeWindowToSellClaim = createClaimPanel.Q<UITextField>("tax-time-sell");
            
            Button createCancelButton = createClaimPanel.Q<Button>("create-cancel-button");
            createCancelButton.clicked += UpdateClaimDetails;
            Button createCreateButton = createClaimPanel.Q<Button>("create-create-button");
            createCreateButton.clicked += CreateClaim;
            
            //Sell Panel
            sellClaimPanel = uiWindow.Q<VisualElement>("sell-claim-panel");
            sellClaimForSaleToggle = uiWindow.Q<Toggle>();
            // sellClaimForSaleToggle.RegisterValueChangedCallback(sel)
             VisualElement sellCurrency = uiWindow.Q<VisualElement>("sell-currency-input");
             sellClaimCurrencyPanel = new UIAtavismCurrencyInputPanel();
             sellClaimCurrencyPanel.SetVisualElement(sellCurrency);
             sellClaimCurrencyPanel.SetCurrencyReverseOrder = true;

            Button sellCancelButton = uiWindow.Q<Button>("sell-cancel-button");
            sellCancelButton.clicked += ShowObjectList;
            Button sellSaveButton = uiWindow.Q<Button>("sell-save-button");
            sellSaveButton.clicked += SaveSellClaimSettings;
            
            
            permissionsPanel = uiWindow.Q<VisualElement>("permissions-panel");
            permissionsList = uiWindow.Q<ListView>("permissions-list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = permissionsList.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            permissionsList.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismClaimPermission newListEntryLogic = new UIAtavismClaimPermission();
                // Instantiate the UXML template for the entry
                var newListEntry = permissionElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // slots.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            permissionsList.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismClaimPermission).SetPermissionDetails(WorldBuilder.Instance.ActiveClaim.permissions[index]);
            };
            permissionLevel = uiWindow.Q<Label>("permission");
            
            permissionCategory = uiWindow.Q<UIDropdown>("permission-category");
            if (permissionCategory != null)
            {
                permissionCategory.Screen = uiScreen;
                permissionCategory.RegisterCallback<ChangeEvent<int>>(ChangePermissionLevel);
            }
                
            permissionPlayerName = uiWindow.Q<UITextField>("player-name");
            
            Button changePermissionButton = uiWindow.Q<Button>("change-permission-button");
            if (changePermissionButton != null)
            {
                changePermissionButton.clicked += ChangePermissionLevel;
            }

            Button addPlayerButton = uiWindow.Q<Button>("add-player-button");
            addPlayerButton.clicked += AddPermission;
         
            Button permissionCancelButton = uiWindow.Q<Button>("permission-cancel-button");
            permissionCancelButton.clicked += ShowObjectList;
            
            // KeyCode toggleKey;
//Upgrade Panel
            upgradeButton = uiWindow.Q<Button>("upgrade-button");
            upgradeButton.clicked += ShowUpgradeClaim;
            upgradePanel = uiWindow.Q<VisualElement>("upgrade-panel");
            reqItemTitle = upgradePanel.Q<Label>("require-items-title");

            VisualElement currencyUpg = upgradePanel.Q<VisualElement>("upgrade-currency");
            upgradeCost = new UIAtavismCurrencyDisplay();
            upgradeCost.SetVisualElement(currencyUpg);
            upgradeCost.ReverseOrder = true;
            
            // List<UIAtavismBuildMaterialSlot> UpgradeRequiredItems;
            upgradeSize = upgradePanel.Q<Label>("new-size");
            Button actionUpgradeButton = upgradePanel.Q<Button>("action-upgrade-button");
            actionUpgradeButton.clicked += SendUpgradeClaim;
            Button upgradeCancelButton = upgradePanel.Q<Button>("upgrade-cancel-button");
            upgradeCancelButton.clicked += ShowObjectList;

            for (int i = 1; i <= 6; i++)
            {
                VisualElement item = upgradePanel.Q<VisualElement>("build-material-slot-"+i);
                if (item != null)
                {
                    UIAtavismBuildMaterialSlot m = new UIAtavismBuildMaterialSlot();
                    m.SetVisualElement(item);
                    UpgradeRequiredItems.Add(m);
                }
            }
            
            
            //Taxs
            taxPayButton = uiWindow.Q<Button>("pay-tax-button");
            taxPayButton.clicked += ClickPayTax;
            taxStatusTitle = uiWindow.Q<Label>("tax-status-title");
            taxStatusText = uiWindow.Q<Label>("tax-status");
            taxInfoTitle = uiWindow.Q<Label>("tax-info-title");
            taxInfoText = uiWindow.Q<Label>("tax-info");
            //Limits
            limitsPanel = uiWindow.Q<Button>("limits-panel" );
            limitsGrid = uiWindow.Q<VisualElement>("limits-grid");
            limitsButton = uiWindow.Q<Button>("limits-button");
            limitsButton.clicked += ShowLimits;
            // List<UIAtavismLimitDisplay> limitsList= uiWindow.Q<Button>();
            // limitsListGrid = uiWindow.Q<VisualElement>();




            return true;
        }

      


        protected override void registerEvents()
        {
            base.registerEvents();
            
            AtavismEventSystem.RegisterEvent("CLAIM_CHANGED", this);
            AtavismEventSystem.RegisterEvent("CLAIM_OBJECT_SELECTED", this);
            AtavismEventSystem.RegisterEvent("CLAIM_OBJECT_UPDATED", this);
            AtavismEventSystem.RegisterEvent("CLAIM_UPGRADE_SHOW", this);
            //   AtavismEventSystem.RegisterEvent("CLAIM_TAX_SHOW", this);
            
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("CLAIM_CHANGED", this);
            AtavismEventSystem.UnregisterEvent("CLAIM_OBJECT_SELECTED", this);
            AtavismEventSystem.UnregisterEvent("CLAIM_OBJECT_UPDATED", this);
            AtavismEventSystem.UnregisterEvent("CLAIM_UPGRADE_SHOW", this);
            //    AtavismEventSystem.UnregisterEvent("CLAIM_TAX_SHOW", this);
        }


        public override void Show()
        {
            base.Show();
            // AtavismSettings.Instance.OpenWindow(this);
            HidePanels();
            UpdateClaimDetails();
            WorldBuilder.Instance.ShowClaims = true;

            // WorldBuilder.Instance.BuildingState = WorldBuildingState.None;
            // BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
          
            AtavismCursor.Instance.SetUIActivatableClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
            // AtavismUIUtility.BringToFront(gameObject);
        }

        public override void Hide()
        {
            base.Hide();

            WorldBuilder.Instance.ShowClaims = false;
            HidePanels();
            // WorldBuilder.Instance.BuildingState = WorldBuildingState.None;
            WorldBuilder.Instance.SelectedObject = null;
            if(BuilderBehaviour.Instance!=null)
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
            
        }

        public void OnEvent(AtavismEventData eData)
        {
        //    Debug.LogError("UGUIWorldBuilder OnEvent "+eData.eventType);
            if (eData.eventType == "CLAIM_UPGRADE_SHOW")
            {
                HidePanels();
           //     Debug.LogError("ClaimUpgradeMessage ");
                int claimId = int.Parse(eData.eventArgs[0]);
                int currency = int.Parse(eData.eventArgs[1]);
                long cost = long.Parse(eData.eventArgs[2]);

                if (upgradePanel != null)
                    upgradePanel.ShowVisualElement();

                List<int> items = new List<int>();
                if (eData.eventArgs[3].Length > 0)
                {
                    string[] itemsList = eData.eventArgs[3].Split(',');
                    foreach (string item in itemsList)
                    {
                        if(item.Length>0)
                            items.Add(int.Parse(item));
                    }
                }
             //   Debug.LogError("ClaimUpgradeMessage "+items);
             if (UpgradeRequiredItems != null)
             {
               //  Debug.LogError("ClaimUpgradeMessage UpgradeRequiredItems count " + UpgradeRequiredItems.Count + " item count " + items.Count);
                 if (items.Count == 0)
                 {
                     if (reqItemTitle!=null)
                         reqItemTitle.HideVisualElement();
                 }
                 else
                 {
                     if (reqItemTitle!=null)
                         reqItemTitle.ShowVisualElement();
                 }

                 int i = 0;
                 foreach (var tempId in items)
                 {
                     AtavismInventoryItem aii = AtavismPrefabManager.Instance.LoadItem(tempId);
                     if (UpgradeRequiredItems.Count > i)
                     {
                         if (UpgradeRequiredItems[i]!=null)
                             UpgradeRequiredItems[i].Show();
                         UpgradeRequiredItems[i].UpdateBuildingSlotData(aii, 0);
                     }

                     i++;
                 }

              //   Debug.LogError("ClaimUpgradeMessage i=" + i);
                 for (int j = i; j < UpgradeRequiredItems.Count; j++)
                 {
                     UpgradeRequiredItems[j].UpdateBuildingSlotData(null, 0);
                     if (UpgradeRequiredItems[j]!=null)
                         UpgradeRequiredItems[j].Hide();
                 }
             }

             if (upgradeCost != null)
                {
                    // List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(currency, cost);
                    upgradeCost.SetData(currency, cost);
                    // for (int i = 0; i < upgradeCost.Count; i++)
                    // {
                    //     if (i < currencyDisplayList.Count)
                    //     {
                    //         upgradeCost[i].gameObject.SetActive(true);
                    //         upgradeCost[i].SetCurrencyDisplayData(currencyDisplayList[i]);
                    //     }
                    //     else
                    //     {
                    //         upgradeCost[i].gameObject.SetActive(false);
                    //     }
                    // }
                }

                if (upgradeSize != null)
                {
                    upgradeSize.text = eData.eventArgs[4] + "x" + eData.eventArgs[5] + "x" + eData.eventArgs[6];
                }
            }
            else if (eData.eventType == "CLAIM_CHANGED")
            {
                if (WorldBuilder.Instance.ActiveClaim == null)
                {
                    Hide();
                }
                else
                {
                    if (showing)
                    {
                       /* if (showLimits)
                        {
                            if (limitsPanel)
                            {
                                limitsPanel.gameObject.SetActive(true);
                                updateDisplayLimits();
                            }
                        }*/

                        UpdateClaimDetails();
                        WorldBuilder.Instance.BuildingState = WorldBuildingState.EditItem;
                        if (BuilderBehaviour.Instance.CurrentMode == BuildMode.None)
                            BuilderBehaviour.Instance.ChangeMode(BuildMode.Edition);

                    }
                }
            }
            else if (eData.eventType == "CLAIM_OBJECT_SELECTED")
            {
                if (WorldBuilder.Instance.SelectedObject == null)
                {
                    HidePanels();
                    return;
                }
                for (int i = 0; i < requiredItems.Count; i++)
                {
                    requiredItems[i].Discarded();
                }
                
                
                
                AtavismBuildObjectTemplate template = WorldBuilder.Instance.GetBuildObjectTemplate(WorldBuilder.Instance.SelectedObject);
                if (template.upgradeItemsReq.Count == 0 || WorldBuilder.Instance.SelectedObject.FinalStage)
                {
                    ShowEditObject();
                }
                else
                {
                    ShowConstructObject();
                }
            }
            else if (eData.eventType == "CLAIM_OBJECT_UPDATED")
            {
            //   Debug.LogError("############### CLAIM_OBJECT_UPDATED "+WorldBuilder.Instance.SelectedObject);
                if (WorldBuilder.Instance.SelectedObject == null)
                {
                    HidePanels();
                    if (WorldBuilder.Instance.ActiveClaim == null || !WorldBuilder.Instance.ActiveClaim.playerOwned)
                        return;
                    buildObjectPanel.ShowVisualElement();
                    ChangeCategory(null);
                    if (showLimits)
                    {
                        if (limitsPanel!=null)
                        {
                            limitsPanel.ShowVisualElement();
                            updateDisplayLimits();
                        }  }

                    if (buildingCategory!=null)
                    {
                        
                        buildingCategoryOptions.Clear();
                        buildingCategoryOptions.Add("All");

                        if (WorldBuilder.Instance.ActiveClaim.limits.Count > 0)
                        {
                            foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                            {
                                string n = WorldBuilder.Instance.GetBuildingCategory(key);
                                buildingCategoryOptions.Add(n);
                            }
                        }
                        else
                        {

                            foreach (var ct in WorldBuilder.Instance.BuildingCategory.Keys)
                            {
                                // Debug.LogError("ShowObjectList "+WorldBuilder.Instance.BuildingCategory[ct].name);
                                buildingCategoryOptions.Add(WorldBuilder.Instance.BuildingCategory[ct].name);
                             
                            }
                        }
                        buildingCategory.Options(buildingCategoryOptions);
                        buildingCategory.Index =0; 
                    }
                    return;
                }
              // Debug.LogError("############### CLAIM_OBJECT_UPDATED ItemReqs count "+WorldBuilder.Instance.SelectedObject.ItemReqs.Count+" FinalStage "+WorldBuilder.Instance.SelectedObject.FinalStage);
                if (WorldBuilder.Instance.SelectedObject.ItemReqs.Count == 0 || WorldBuilder.Instance.SelectedObject.FinalStage)
                {
                    for (int i = 0; i < requiredItems.Count; i++)
                    {
                        requiredItems[i].Discarded();
                    }
                    ShowEditObject();
                }
                else
                {
//                    Debug.LogError("############### ShowConstructObject");
                    ShowConstructObject();
                }
            }
        }

        IEnumerator UpdateTimer()
        {
          //  Debug.LogError("!!!!!!!!!!!!!!!!!! UpdateTimer " + WorldBuilder.Instance.ActiveClaim.taxTime);
            WaitForSeconds delay = new WaitForSeconds(1f);
            while (true)
            {
                if (WorldBuilder.Instance.ActiveClaim !=null && WorldBuilder.Instance.ActiveClaim.taxTime > Time.time)
                {
                    float _time = WorldBuilder.Instance.ActiveClaim.taxTime - Time.time;
                    int days = 0;
                    int hour = 0;
                    int minute = 0;
                    int secound = 0;
                    if (_time > 24 * 3600)
                    {
                        days = (int) (_time / (24F * 3600F));
                    }

                    if (_time - days * 24 * 3600 > 3600)
                        hour = (int) ((_time - days * 24 * 3600) / 3600F);
                    if (_time - days * 24 * 3600 - hour * 3600 > 60)
                        minute = (int) (_time - days * 24 * 3600 - hour * 3600) / 60;
                    //secound = (int) (_time - days * 24 * 3600 - hour * 3600 - minute * 60);
                    //   Debug.LogError( "$$$$$$$$$$$$$$$$            hour="+hour+" minute="+minute+" secound="+secound);
                    string outTime = "";
                    if (days > 0)
                    {
#if AT_I2LOC_PRESET
                    outTime += days + " " + I2.Loc.LocalizationManager.GetTranslation("days");
#else
                        outTime += days + " " + "days";
#endif
                    }

                    if (hour > 0)
                    {
                        if (minute > 9)
                            outTime += " " + hour + ":";
                        else
                            outTime += " 0" + hour + ":";
                        if (minute > 0)
                        {
                            if (minute < 10)
                                outTime += "0" + minute;
                            else
                                outTime += minute;
                        }
                        else
                        {
                            outTime += "00";
                        }
                    }
                    else if (minute > 0)
                    {
                        if (minute > 9)
                            outTime += " " + minute;
                        else
                            outTime += " 0" + minute;
                    }

                    //  outTime = (days > 0 ? days > 1 ? days + " days " : days + " day " : "") + (hour > 0 ? hour + "h " : "")+ (minute > 0 ? minute + "m" : "");
                    if (taxStatusText != null)
                        taxStatusText.text = outTime;

                }
                yield return delay;
            }
        }
        

        public void UpdateClaimDetails()
        {
            if(taxStatusText!=null)
                taxStatusText.HideVisualElement();
            if(taxStatusTitle!=null)
                taxStatusTitle.HideVisualElement();
            if (limitsPanel!=null && limitsPanel.style.display == DisplayStyle.Flex)
                if (WorldBuilder.Instance.ActiveClaim != null)
                {
                    int index = 0;
                    if (limitsGrid != null)
                    {
                        foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                        {
                            string c = (WorldBuilder.Instance.ActiveClaim.limitsCount.ContainsKey(key) ? WorldBuilder.Instance.ActiveClaim.limitsCount[key] + "" : "0") + "/" + WorldBuilder.Instance.ActiveClaim.limits[key];
                            string n = WorldBuilder.Instance.GetBuildingCategory(key);
                            if (limitsList.Count <= index)
                            {
                                VisualElement v = limitElementTemplate.Instantiate();
                                UIAtavismLimitDisplay go = new UIAtavismLimitDisplay(v,limitsListGrid);
                                limitsList.Add(go);
                            }
                            if(limitsList[index]!=null)
                                limitsList[index].Show();
                            limitsList[index].Display(c,n);
                            index++;
                        }

                        for (int i = index; i < limitsList.Count; i++)
                        {
                            if(limitsList[index]!=null)
                                limitsList[i].Hide();
                        }
                    }

                }
            if (permissionsPanel.style.display == DisplayStyle.Flex && WorldBuilder.Instance.ActiveClaim != null)
            {
                PermissionsUpdated();

                if (claimName != null)
                    claimName.text = WorldBuilder.Instance.ActiveClaim.name;
                if (claimType != null)
                {
                    foreach (var ct in WorldBuilder.Instance.ClaimTypes.Values)
                    {
                        if (ct.id == WorldBuilder.Instance.ActiveClaim.claimType)
                        {
                            claimType.text = ct.name;
                        }
                    }
                }

                if (size != null)
                    size.text = WorldBuilder.Instance.ActiveClaim.sizeX + "x" +WorldBuilder.Instance.ActiveClaim.sizeY + "x" + WorldBuilder.Instance.ActiveClaim.sizeZ;
              
                
             //   Debug.LogWarning("UpdateClaimDetails: "+WorldBuilder.Instance.ActiveClaim.taxTime);
                if (WorldBuilder.Instance.ActiveClaim != null && WorldBuilder.Instance.ActiveClaim.taxTime > 0)
                {
                   /* if (WorldBuilder.Instance.ActiveClaim.permissionlevel > 0)
                    {*/
                        if (taxStatusText != null)
                            taxStatusText.ShowVisualElement();
                        if (taxStatusTitle != null)
                            taxStatusTitle.ShowVisualElement();
                        if (taxPayButton != null)
                            taxPayButton.ShowVisualElement();
                        StopAllCoroutines();
                        StartCoroutine(UpdateTimer());
                   /* }
                    else
                    {
                        StopAllCoroutines();
                    }*/
                }
                else
                {
                    if(taxStatusText!=null)
                        taxStatusText.HideVisualElement();
                    if(taxStatusTitle!=null)
                        taxStatusTitle.HideVisualElement();
                    if(taxPayButton!=null)
                        taxPayButton.HideVisualElement();
                    
                }
                
                if (WorldBuilder.Instance.ActiveClaim.forSale)
                {
#if AT_I2LOC_PRESET
             if (status != null)     status.text = I2.Loc.LocalizationManager.GetTranslation("For Sale");
#else
                    if (status != null)
                        status.text = "For Sale";
#endif
                    if (!WorldBuilder.Instance.ActiveClaim.playerOwned)
                        buyButton.ShowVisualElement();
                   
                }
                else
                {
#if AT_I2LOC_PRESET
              if (status != null)    status.text = I2.Loc.LocalizationManager.GetTranslation("Owned");
#else
                    if (status != null)
                        status.text = "Owned";
#endif
                }
                return;
            }
            else
            {
                HidePanels();
                buyButton.HideVisualElement();
                sellButton.HideVisualElement();
                if(taxPayButton!=null)
                    taxPayButton.HideVisualElement();
                if(upgradeButton!=null)
                    upgradeButton.HideVisualElement();
                deleteButton.HideVisualElement();
                createButton.HideVisualElement();
                permissionsButton.HideVisualElement();
                if (WorldBuilder.Instance.ActiveClaim == null)
                {
                    if (claimName != null)
                        claimName.text = "-";
                    if (size != null)
                        size.text = "-";
                    if (status != null)
                        status.text = "-";
                    if (taxInfoText != null)
                        taxInfoText.HideVisualElement();
                    if (taxInfoTitle != null)
                        taxInfoTitle.HideVisualElement();
                    if (claimType != null)
                    {
                        claimType.text = "-";
                    }
                    if (ClientAPI.IsPlayerAdmin())
                    {
                        createButton.ShowVisualElement();
                    }
                    return;
                }
            }

            if (claimName != null)
                claimName.text = WorldBuilder.Instance.ActiveClaim.name;
            if (claimType != null)
            {
                foreach (var ct in WorldBuilder.Instance.ClaimTypes.Values)
                {
                    if (ct.id == WorldBuilder.Instance.ActiveClaim.claimType)
                    {
                        claimType.text = ct.name;
                    }
                }
            }
            if (size != null)
                size.text = WorldBuilder.Instance.ActiveClaim.sizeX + "x" + WorldBuilder.Instance.ActiveClaim.sizeY + "x" + WorldBuilder.Instance.ActiveClaim.sizeZ;
           
            if (WorldBuilder.Instance.ActiveClaim != null && WorldBuilder.Instance.ActiveClaim.taxTime > 0)
            {
                if (WorldBuilder.Instance.ActiveClaim.permissionlevel > 0||WorldBuilder.Instance.ActiveClaim.forSale)
                {
                    if (taxStatusText != null)
                        taxStatusText.ShowVisualElement();
                    if (taxStatusTitle != null)
                        taxStatusTitle.ShowVisualElement();
                    if (WorldBuilder.Instance.ActiveClaim.permissionlevel > 0)
                    {
                        if (taxPayButton != null)
                            taxPayButton.ShowVisualElement();
                    }

                    StopAllCoroutines();
                    StartCoroutine(UpdateTimer());
                }
            }
            else
            {
                if(taxStatusText!=null)
                    taxStatusText.HideVisualElement();
                if(taxStatusTitle!=null)
                    taxStatusTitle.HideVisualElement();
                if(taxPayButton!=null)
                    taxPayButton.HideVisualElement();
                    
            }
            
            
            if (taxInfoTitle != null)
            {
                taxInfoTitle.ShowVisualElement();
            }
                
            if (taxInfoText != null)
            {
                taxInfoText.ShowVisualElement();
                long time = (long) WorldBuilder.Instance.ActiveClaim.taxInterval;
                if (WorldBuilder.Instance.ActiveClaim.taxAmount > 0)
                {
                long days = 0;
                long hour = 0;
                if (time > 24)
                {
                    days = (long) (time / 24F);
                    hour =  (time - (days * 24));
                }
                else
                {
                    hour = time;
                }
                string cost = Inventory.Instance.GetCostString(WorldBuilder.Instance.ActiveClaim.taxCurrency, WorldBuilder.Instance.ActiveClaim.taxAmount);
                
                
                long _days = 0;
                long _hour = 0;
                if (WorldBuilder.Instance.ActiveClaim.taxPeriodPay > 24)
                {
                    _days = (long) (WorldBuilder.Instance.ActiveClaim.taxPeriodPay / 24F);
                    _hour =  (WorldBuilder.Instance.ActiveClaim.taxPeriodPay - (_days * 24));
                }
                else
                {
                    _hour = WorldBuilder.Instance.ActiveClaim.taxPeriodPay;
                }
                
#if AT_I2LOC_PRESET
                     taxInfoText.text = cost + " "+I2.Loc.LocalizationManager.GetTranslation("per")+" " + (days > 0 ? days > 1 ? days + " "+I2.Loc.LocalizationManager.GetTranslation("days")+" " : days + " "+
                        I2.Loc.LocalizationManager.GetTranslation("day")+" " : "") + (hour > 0 ? hour + " "+I2.Loc.LocalizationManager.GetTranslation("hour") : "")+
                                  ". "+I2.Loc.LocalizationManager.GetTranslation("Can be paid")+" "+ (_days > 0 ? _days > 1 ? _days + " days " : _days + " day " : "") + (_hour > 0 ? _hour + " hour" : "")+" "+
                                 I2.Loc.LocalizationManager.GetTranslation("before tax expire");
#else
                taxInfoText.text = cost + " per " + (days > 0 ? days > 1 ? days + " days " : days + " day " : "") + (hour > 0 ? hour + " hour" : "")+
                                   ". Can be paid "+ (_days > 0 ? _days > 1 ? _days + " days " : _days + " day " : "") + (_hour > 0 ? _hour + " hour" : "")+" before tax expires";
#endif
                    if(taxPayButton!=null)
                        taxPayButton.SetEnabled(true);

                }
                else
                {
#if AT_I2LOC_PRESET
                taxInfoText.text = I2.Loc.LocalizationManager.GetTranslation("No Tax");
#else
                    taxInfoText.text = "No tax";
#endif
                    if(taxPayButton!=null)
                        taxPayButton.SetEnabled(false);
                }
            }

            
            if (WorldBuilder.Instance.ActiveClaim.playerOwned)
            {
                buildObjectPanel.ShowVisualElement();
                if (WorldBuilder.Instance.BuildingState == WorldBuildingState.None)
                    WorldBuilder.Instance.BuildingState = WorldBuildingState.Standard;
                ChangeCategory(null);
                if (showLimits)
                {
                    if (limitsPanel!=null)
                    {
                        limitsPanel.ShowVisualElement();
                        updateDisplayLimits();
                    }      }

                sellButton.ShowVisualElement();
                deleteButton.ShowVisualElement();
                permissionsButton.ShowVisualElement();
                if(upgradeButton!=null)
                    upgradeButton.ShowVisualElement();
                if(taxPayButton!=null)
                    taxPayButton.ShowVisualElement();
                if (buildingCategory!=null)
                {
                    buildingCategoryOptions.Clear();
#if AT_I2LOC_PRESET
                   buildingCategoryOptions.Add(I2.Loc.LocalizationManager.GetTranslation("All"));
#else
                    buildingCategoryOptions.Add("All");
#endif   
                    if (WorldBuilder.Instance.ActiveClaim.limits.Count > 0)
                    {
                        foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                        {
                            string n = WorldBuilder.Instance.GetBuildingCategory(key);
#if AT_I2LOC_PRESET
                                n = I2.Loc.LocalizationManager.GetTranslation(n);
#endif
                            buildingCategoryOptions.Add(n);
                        }
                    }
                    else
                    {

                        foreach (var ct in WorldBuilder.Instance.BuildingCategory.Keys)
                        {
                            // Debug.LogError("ShowObjectList "+WorldBuilder.Instance.BuildingCategory[ct].name);
#if AT_I2LOC_PRESET
                            buildingCategoryOptions.Add(I2.Loc.LocalizationManager.GetTranslation(WorldBuilder.Instance.BuildingCategory[ct].name));
#else
                            
                            buildingCategoryOptions.Add(WorldBuilder.Instance.BuildingCategory[ct].name);
#endif
                        }
                    }
                    buildingCategory.Options(buildingCategoryOptions);
                    buildingCategory.Index=0; 

                }

            }
            else if (WorldBuilder.Instance.ActiveClaim.permissionlevel > 0)
            {
                buildObjectPanel.ShowVisualElement();
                if (WorldBuilder.Instance.BuildingState == WorldBuildingState.None)
                    WorldBuilder.Instance.BuildingState = WorldBuildingState.Standard;
                ChangeCategory(null);
                if (showLimits)
                {
                    if (limitsPanel!=null)
                    {
                        limitsPanel.ShowVisualElement();
                        updateDisplayLimits();
                    }
                }
                if (buildingCategory!=null)
                {
                  
                    buildingCategoryOptions.Clear();
#if AT_I2LOC_PRESET
                   buildingCategoryOptions.Add(I2.Loc.LocalizationManager.GetTranslation("All"));
#else
                    buildingCategoryOptions.Add("All");
#endif                    
                    
                    if (WorldBuilder.Instance.ActiveClaim.limits.Count > 0)
                    {
                        foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                        {
  
                            string n = WorldBuilder.Instance.GetBuildingCategory(key);
#if AT_I2LOC_PRESET
                                n = I2.Loc.LocalizationManager.GetTranslation(n);
#endif
                            buildingCategoryOptions.Add(n);
                        }
                    }
                    else
                    {

                        foreach (var ct in WorldBuilder.Instance.BuildingCategory.Keys)
                        {
                            // Debug.LogError("ShowObjectList "+WorldBuilder.Instance.BuildingCategory[ct].name);
#if AT_I2LOC_PRESET
                            buildingCategoryOptions.Add(I2.Loc.LocalizationManager.GetTranslation(WorldBuilder.Instance.BuildingCategory[ct].name));
#else
                            
                            buildingCategoryOptions.Add(WorldBuilder.Instance.BuildingCategory[ct].name);
#endif
                        }
                    }
                    buildingCategory.Options(buildingCategoryOptions);
                    buildingCategory.Index=0; 

                }
                
                
                
            }
            else
            {
                buildObjectPanel.HideVisualElement();
            }

            if (WorldBuilder.Instance.ActiveClaim.forSale)
            {
#if AT_I2LOC_PRESET
             if (status != null) status.text = I2.Loc.LocalizationManager.GetTranslation("For Sale");
#else
                if (status != null)
                    status.text = "For Sale";
#endif
                if (!WorldBuilder.Instance.ActiveClaim.playerOwned)
                    buyButton.ShowVisualElement();
            }
            else
            {
#if AT_I2LOC_PRESET
           if (status != null)   status.text = I2.Loc.LocalizationManager.GetTranslation("Owned");
#else
                if (status != null)
                    status.text = "Owned";
#endif
            }
        }

        void HidePanels()
        {
            buildObjectPanel.HideVisualElement();
            editObjectPanel.HideVisualElement();
            createClaimPanel.HideVisualElement();
            sellClaimPanel.HideVisualElement();
            permissionsPanel.HideVisualElement();
            if (constructObjectPanel != null)
                constructObjectPanel.HideVisualElement();
            if (upgradePanel != null)
                upgradePanel.HideVisualElement();
            if(limitsPanel!=null)
                limitsPanel.HideVisualElement();
        }

        public void ShowLimits()
        {
            if (limitsPanel != null)
            {
                if (limitsPanel.style.display == DisplayStyle.Flex)
                {
                    limitsPanel.HideVisualElement();
                    showLimits = false;
                }
                else
                {
                    limitsPanel.ShowVisualElement();
                    showLimits = true;
                }

                updateDisplayLimits();
            }
        }

        void updateDisplayLimits()
        {
            if (limitsPanel.style.display == DisplayStyle.Flex)
                if (WorldBuilder.Instance.ActiveClaim != null)
                {
                    int index = 0;
                    if (limitsGrid != null)
                    {
                        foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                        {
                            string c = (WorldBuilder.Instance.ActiveClaim.limitsCount.ContainsKey(key) ? WorldBuilder.Instance.ActiveClaim.limitsCount[key] + "" : "0") + "/" + WorldBuilder.Instance.ActiveClaim.limits[key];
                            string n = WorldBuilder.Instance.GetBuildingCategory(key);
                            if (limitsList.Count <= index)
                            {
                                VisualElement v = limitElementTemplate.Instantiate();
                                UIAtavismLimitDisplay go = new UIAtavismLimitDisplay(v,limitsListGrid);
                                limitsList.Add(go);
                            }

                            if(limitsList[index]!=null)
                                limitsList[index].Show();
                            limitsList[index].Display(c, n);
                            index++;
                        }

                        for (int i = index; i < limitsList.Count; i++)
                        {
                            if (limitsList[index]!=null)
                                limitsList[i].Hide();
                        }
                    }

                }
        }

        public void DeleteClaim()
        {
#if AT_I2LOC_PRESET
       string message = I2.Loc.LocalizationManager.GetTranslation("Are you sure you want to delete your claim") + ": " + WorldBuilder.Instance.ActiveClaim.name;
#else
            string message = "Are you sure you want to delete your claim: " + WorldBuilder.Instance.ActiveClaim.name;
#endif
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(message, null, ConfirmedDeleteClaim);
        }

        public void ConfirmedDeleteClaim(object obj, bool accepted)
        {
            if (accepted)
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("claimID", WorldBuilder.Instance.ActiveClaim.id);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.DELETE_CLAIM", props);
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
            }
        }

        public void BuyClaim()
        {
            string costString = Inventory.Instance.GetCostString(WorldBuilder.Instance.ActiveClaim.currency, WorldBuilder.Instance.ActiveClaim.cost);
            string costitem = "";
            if (WorldBuilder.Instance.ActiveClaim.purchaseItemReq > 0)
            {
               AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(WorldBuilder.Instance.ActiveClaim.purchaseItemReq);
                costitem = " and " + aii.BaseName;
            }
#if AT_I2LOC_PRESET
         if (WorldBuilder.Instance.ActiveClaim.purchaseItemReq > 0)
            {
               AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(WorldBuilder.Instance.ActiveClaim.purchaseItemReq);
                costitem = " "+I2.Loc.LocalizationManager.GetTranslation("and")+ " " +  I2.Loc.LocalizationManager.GetTranslation("Items/"+aii.BaseName);
            }  
            string message = I2.Loc.LocalizationManager.GetTranslation("Are you sure you want to buy claim") + ": " + WorldBuilder.Instance.ActiveClaim.name
    + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + costitem+ "?";
           
#else
            string message = "Are you sure you want to buy claim: " + WorldBuilder.Instance.ActiveClaim.name
                + " for " + costString + costitem+"?";
#endif
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(message, null, ConfirmedBuyClaim);
        }

        public void ConfirmedBuyClaim(object obj, bool accepted)
        {
            if (accepted)
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("claimID", WorldBuilder.Instance.ActiveClaim.id);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.PURCHASE_CLAIM", props);
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
            }
        }

        #region Create Claim

        public void ShowCreateClaimPanel()
        {
            HidePanels();
            createButton.HideVisualElement();
            createClaimPanel.ShowVisualElement();
            if (newClaimType!=null)
            {
              
                newClaimTypeOptions.Clear();
                // newClaimTypeOptions.Add("All");
                // newClaimType.options.Clear();
                foreach (var ct in WorldBuilder.Instance.ClaimTypes.Keys)
                {
                    
                    newClaimTypeOptions.Add(WorldBuilder.Instance.ClaimTypes[ct].name);
                }

                newClaimType.Options(newClaimTypeOptions);
                newClaimType.Index = 0;
            }

            if (taxCurrency!=null)
            {
               
                taxCurrencyOptions.Clear();
                // taxCurrencyOptions.Add("All");
                foreach (var cur in Inventory.Instance.Currencies.Values)
                {
                    taxCurrencyOptions.Add( cur.name);
                }
                taxCurrency.Options(taxCurrencyOptions);
                taxCurrency.Index = 0;
            }

            currencyInputPanel.SetCurrencies(Inventory.Instance.GetMainCurrencies());
            currencyInputPanel.ClearCurrencyAmounts();
            
        }

        public void SetClaimName(string name)
        {
            newClaimName = name;
        }

        public void SetClaimSize(ChangeEvent<int> evt)
        {
            newClaimSize = evt.newValue;
            if (claimSizeText != null)
                claimSizeText.text = newClaimSize.ToString();
        }

        public void SetPlayerOwned(ChangeEvent<bool> evt)
        {
            playerOwned = evt.newValue;
        }

        public void SetForSale(ChangeEvent<bool> evt)
        {
            this.forSale = evt.newValue;
        }

        public void CreateClaim()
        {
            newClaimName = createClaimName.text;
            currencyInputPanel.GetCurrencyAmount(out currencyID, out cost);
            int ct = newClaimType.Index;
            string ctn = newClaimTypeOptions[newClaimType.Index];
            foreach (var ctid in WorldBuilder.Instance.ClaimTypes.Keys)
            {
                if (WorldBuilder.Instance.ClaimTypes[ctid].name.Equals(newClaimTypeOptions[newClaimType.Index]))
                    ct = WorldBuilder.Instance.ClaimTypes[ctid].id;
            }
            int tCurrency = -1;
            if (taxCurrency!=null)
            {
                int index = 0;
                foreach (var cur in Inventory.Instance.Currencies.Values)
                {
                 //   Debug.LogError("Create Claim cur "+cur.name+" "+cur.id);
                    if (index == taxCurrency.Index)
                    {
                        tCurrency = cur.id;
                      //  Debug.LogError("Create Claim set cur "+cur.name+" "+cur.id);
                    }
                    index++;
                }
            }

            long tAmount = 0L;
            if (taxAmount!=null)
            {
                if(taxAmount.value.Length>0)
                    tAmount = long.Parse(taxAmount.value);
            }

            long tInterval = 0L;
            if (taxInterval!=null)
            {
                if (taxInterval.value.Length > 0)
                    tInterval = long.Parse(taxInterval.value);
            }

            long tTimeWindowToPayTax = 0L;
            if (timeWindowToPayTax!=null)
            {
                if (timeWindowToPayTax.value.Length > 0)
                    tTimeWindowToPayTax = long.Parse(timeWindowToPayTax.value);
            }

            long tTimeWindowToSellClaim = 0L;
            if (timeWindowToSellClaim!=null)
            {
                if (timeWindowToSellClaim.value.Length > 0)
                    tTimeWindowToSellClaim = long.Parse(timeWindowToSellClaim.value);
            }

            WorldBuilder.Instance.CreateClaim(newClaimName, newClaimSize, playerOwned, forSale, currencyID, cost, ct, ctn, tCurrency, tAmount, tInterval, tTimeWindowToPayTax, tTimeWindowToSellClaim);
            UpdateClaimDetails();
            //	ShowObjectList();
        }

        #endregion Create Claim

        #region Edit Object
        public void ShowObjectList()
        {
            HidePanels();
            buildObjectPanel.ShowVisualElement();
            ChangeCategory(null);
            if (showLimits)
            {
                if (limitsPanel!=null)
                {
                    limitsPanel.ShowVisualElement();
                    updateDisplayLimits();
                }
            }
         //   Debug.LogError("ShowObjectList");
            if (buildingCategory!=null)
            {
                // buildingCategory.options.Clear();
                buildingCategoryOptions.Add("All");
               
                if (WorldBuilder.Instance.ActiveClaim.limits.Count > 0)
                {
                    foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                    {
                        string n = WorldBuilder.Instance.GetBuildingCategory(key);
                        buildingCategoryOptions.Add(n);
                    }
                }
                else
                {

                    foreach (var ct in WorldBuilder.Instance.BuildingCategory.Keys)
                    {
                        // Debug.LogError("ShowObjectList "+WorldBuilder.Instance.BuildingCategory[ct].name);
                        buildingCategoryOptions.Add(WorldBuilder.Instance.BuildingCategory[ct].name);
                    }
                }
                buildingCategory.Options(buildingCategoryOptions);
                buildingCategory.Index = 0;
                // buildingCategory.SetValueWithoutNotify(0); 

            }

        }

        public void ChangeCategory(ChangeEvent<int> evt)
        {
            if (WorldBuilder.Instance.ActiveClaim == null)
                return;
            
            if (buildObjectList != null && buildingCategory != null)
            {
                if (buildingCategory.Index == -1)
                    return;
                if (buildingCategory.Index == 0)
                {
                        buildObjectList.changeCategory(-2);
                }
                else
                {
                    if (WorldBuilder.Instance.ActiveClaim.limits.Count > 0)
                    {
                        int index = 1;
                        foreach (var key in WorldBuilder.Instance.ActiveClaim.limits.Keys)
                        {
                            if (index == buildingCategory.Index)
                            {
                                buildObjectList.changeCategory(key);
                            }
                            index++;
                        }
                    }
                    else
                    {
                        buildObjectList.changeCategory(WorldBuilder.Instance.BuildingCategory[buildingCategory.Index - 1].id);
                    }
                }
            }
        }

        public void ShowEditObjectPanel()
        {
            HidePanels();
            editObjectPanel.ShowVisualElement();
        }

        public void StartSelectObject()
        {
            WorldBuilder.Instance.BuildingState = WorldBuildingState.SelectItem;
            AtavismCursor.Instance.ClearUIActivatableClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
           // WorldBuilderInterface.Instance.StartSelectObject();
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Edition);
            
        }

        void ShowEditObject()
        {
          //  Debug.LogError("!!!!!!!!!!!!!!!!!   ShowEditObject");
            AtavismCursor.Instance.SetUIActivatableClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
            HidePanels();
            editObjectPanel.ShowVisualElement();
            AtavismBuildObjectTemplate template = WorldBuilder.Instance.GetBuildObjectTemplate(WorldBuilder.Instance.SelectedObject);
#if AT_I2LOC_PRESET
           if (objectName != null)
                objectName.text =  I2.Loc.LocalizationManager.GetTranslation(template.buildObjectName);
#else
            if (objectName != null)
                objectName.text = template.buildObjectName;
#endif
            if (WorldBuilderInterface.Instance.MouseWheelBuildMode == MouseWheelBuildMode.MoveVertical)
            {
#if AT_I2LOC_PRESET
           if (editMode != null)  editMode.text = I2.Loc.LocalizationManager.GetTranslation("Vertical");
#else
                if (editMode != null)
                    editMode.text = "Vertical";
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
           if (editMode != null)  editMode.text = I2.Loc.LocalizationManager.GetTranslation("Rotate");
#else
                if (editMode != null)
                    editMode.text = "Rotate";
#endif
            }
        }

        public void StartMoveItem()
        {
            if (WorldBuilder.Instance.SelectedObject == null)
            {
                return;
            }
            if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement)
            {
                return;
            }

            if (!WorldBuilder.Instance.SelectedObject.canBeMoved)
            {
                string[] args = new string[1];
#if AT_I2LOC_PRESET
            args[0] = WorldBuilder.Instance.SelectedObject.TemplateName + " " + I2.Loc.LocalizationManager.GetTranslation("cannot be moved.");
#else
                args[0] = WorldBuilder.Instance.SelectedObject.TemplateName + " cannot be moved.";
#endif
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                return;
            }
            if (WorldBuilder.Instance.BuildingState != WorldBuildingState.MoveItem)
            {
                WorldBuilder.Instance.BuildingState = WorldBuildingState.MoveItem;
                WorldBuilder.Instance.SelectedObject.ObjectPlaced = false;

                WorldBuilderInterface.Instance.SetCurrentReticle(WorldBuilder.Instance.SelectedObject.gameObject);
            }
        }

        public void ChangeEditMode()
        {
            if (WorldBuilderInterface.Instance.MouseWheelBuildMode == MouseWheelBuildMode.MoveVertical)
            {
                WorldBuilderInterface.Instance.MouseWheelBuildMode = MouseWheelBuildMode.Rotate;
#if AT_I2LOC_PRESET
             if (editMode != null)  editMode.text = I2.Loc.LocalizationManager.GetTranslation("Rotate");
#else
                if (editMode != null)
                    editMode.text = "Rotate";
#endif
            }
            else
            {
                WorldBuilderInterface.Instance.MouseWheelBuildMode = MouseWheelBuildMode.MoveVertical;
#if AT_I2LOC_PRESET
             if (editMode != null)  editMode.text = I2.Loc.LocalizationManager.GetTranslation("Vertical");
#else
                if (editMode != null)
                    editMode.text = "Vertical";
#endif
            }
        }

        public void RemoveItem()
        {
            WorldBuilder.Instance.PickupClaimObject();
            HidePanels();
            WorldBuilder.Instance.BuildingState = WorldBuildingState.SelectItem;
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Edition);
            buildObjectPanel.ShowVisualElement();
            ChangeCategory(null);
        }

        public void SaveObjectChanges()
        {
            HidePanels();
            WorldBuilder.Instance.BuildingState = WorldBuildingState.SelectItem;
            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            buildObjectPanel.ShowVisualElement();
            ChangeCategory(null);
            WorldBuilder.Instance.SelectedObject.ObjectPlaced = false;
            WorldBuilder.Instance.SelectedObject = null;

        }

        #endregion Edit Object

        #region Construct Building
        void ShowConstructObject()
        {
            // Debug.LogError("ShowConstructObject");
            HidePanels();
            constructObjectPanel.ShowVisualElement();

            if (constructObjectName != null)
                constructObjectName.text = WorldBuilder.Instance.SelectedObject.TemplateName;
            if (WorldBuilder.Instance.SelectedObject.Complete)
            {
                if (WorldBuilder.Instance.SelectedObject.Health < WorldBuilder.Instance.SelectedObject.MaxHealth)
                {
#if AT_I2LOC_PRESET
                string statusText = I2.Loc.LocalizationManager.GetTranslation("Damaged") + ": " + WorldBuilder.Instance.SelectedObject.Health + "/" + WorldBuilder.Instance.SelectedObject.MaxHealth;
#else
                    string statusText = "Damaged: " + WorldBuilder.Instance.SelectedObject.Health + "/" + WorldBuilder.Instance.SelectedObject.MaxHealth;
#endif
                    if (objectStatus != null)
                        objectStatus.text = statusText;
                }
                else
                {
#if AT_I2LOC_PRESET
                if (objectStatus != null) objectStatus.text = I2.Loc.LocalizationManager.GetTranslation("Complete");
#else
                    if (objectStatus != null)
                        objectStatus.text = "Complete";
#endif
                }
            }
            else
            {
#if AT_I2LOC_PRESET
            string statusText = I2.Loc.LocalizationManager.GetTranslation("In Construction") + ": " + WorldBuilder.Instance.SelectedObject.Health + "/" + WorldBuilder.Instance.SelectedObject.MaxHealth;
#else
                string statusText = "In Construction: " + WorldBuilder.Instance.SelectedObject.Health + "/" + WorldBuilder.Instance.SelectedObject.MaxHealth;
#endif
                if (objectStatus != null)
                    objectStatus.text = statusText;
            }

            for (int i = 0; i < requiredItems.Count; i++)
            {
                requiredItems[i].Hide();
            }

            int itemNum = 0;
            foreach (int itemID in WorldBuilder.Instance.SelectedObject.ItemReqs.Keys)
            {

                requiredItems[itemNum].Show();
                AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(itemID);
                
                requiredItems[itemNum].UpdateBuildingSlotData(item, WorldBuilder.Instance.SelectedObject.ItemReqs[itemID]);
                itemNum++;
            }
        }

        public void BuildClicked()
        {
            WorldBuilder.Instance.ImproveBuildObject();
        }

        #endregion Construct Building

        #region Sell Claim
        public void ShowSellClaimPanel()
        {
            HidePanels();
            sellClaimPanel.ShowVisualElement();
            sellClaimCurrencyPanel.SetCurrencies(Inventory.Instance.GetMainCurrencies());
            sellClaimCurrencyPanel.SetCurrencyAmounts(WorldBuilder.Instance.ActiveClaim.currency, WorldBuilder.Instance.ActiveClaim.cost);
            sellClaimForSaleToggle.value = WorldBuilder.Instance.ActiveClaim.forSale;
        }

        public void SaveSellClaimSettings()
        {
            WorldBuilder.Instance.ActiveClaim.forSale = sellClaimForSaleToggle.value;
            sellClaimCurrencyPanel.GetCurrencyAmount(out currencyID, out cost);
            WorldBuilder.Instance.ActiveClaim.currency = currencyID;
            WorldBuilder.Instance.ActiveClaim.cost = cost;
            WorldBuilder.Instance.SendEditClaim();
            ShowObjectList();
        }
        

        #endregion Sell Claim

        #region Permissions
        public void ShowPermissionsPanel()
        {
            HidePanels();
            permissionsPanel.ShowVisualElement();
            playerPermissionName = "";
            permissionLevelGiven = 0;
            if (permissionCategory != null)
            {
                permissionCategory.Options(levels.ToList());
                permissionCategory.Index = permissionLevelGiven;
            }
            
#if AT_I2LOC_PRESET
       if (permissionLevel!=null)  permissionLevel.text =
 I2.Loc.LocalizationManager.GetTranslation(levels[permissionLevelGiven].ToString());
#else
                if (permissionLevel != null)
                    permissionLevel.text = levels[permissionLevelGiven].ToString();
#endif

            
            Refresh();
        }

        void PermissionsUpdated()
        {
            Refresh();
        }

        private void Refresh()
        {
            // throw new System.NotImplementedException();
        }

        public void SetPermissionPlayerName(string name)
        {
            if (permissionPlayerName != null)
                playerPermissionName = permissionPlayerName.text;
            else
                playerPermissionName = name;
        }

        private void ChangePermissionLevel(ChangeEvent<int> evt)
        {
            permissionLevelGiven = evt.newValue;
#if AT_I2LOC_PRESET
        if (permissionLevel != null)  permissionLevel.text = I2.Loc.LocalizationManager.GetTranslation(levels[permissionLevelGiven].ToString());
#else
            if (permissionLevel != null)
                permissionLevel.text = levels[permissionLevelGiven].ToString();
#endif

        }
        
        public void ChangePermissionLevel()
        {
            permissionLevelGiven++;
            if (permissionLevelGiven >= levels.Length)
            {
                permissionLevelGiven = 0;
            }
#if AT_I2LOC_PRESET
        if (permissionLevel != null)  permissionLevel.text = I2.Loc.LocalizationManager.GetTranslation(levels[permissionLevelGiven].ToString());
#else
            if (permissionLevel != null)
                permissionLevel.text = levels[permissionLevelGiven].ToString();
#endif
        }

        public void AddPermission()
        {
            // Add 1 to permission level because I'm an idiot and set it to index 1 on the server
            WorldBuilder.Instance.AddPermission(playerPermissionName, permissionLevelGiven + 1);
            if (permissionPlayerName!=null)
                 permissionPlayerName.value="";
        }


        public void ShowUpgradeClaim()
        {
            
            WorldBuilder.Instance.SendGetUpgradeClaim();
        }
        
        public void SendUpgradeClaim()
        {
            
            WorldBuilder.Instance.SendUpgradeClaim();
            UpdateClaimDetails();
        }

        public void ClickPayTax()
        {
            if(WorldBuilder.Instance.ActiveClaim!=null)
                WorldBuilder.Instance.SendPayTaxForClaim(WorldBuilder.Instance.ActiveClaim.id, false);
        }
        
        // #region implemented abstract members of AtList
        //
        // public override int NumberOfCells()
        // {
        //     int numCells = WorldBuilder.Instance.ActiveClaim.permissions.Count;
        //     return numCells;
        // }
        //
        // public override void UpdateCell(int index, UIClaimPermission cell)
        // {
        //     cell.SetPermissionDetails(WorldBuilder.Instance.ActiveClaim.permissions[index]);
        // }
        //
        // #endregion

        #endregion Permissions

        public static UIAtavismWorldBuilder Instance
        {
            get
            {
                return instance;
            }
        }

        public bool Showing
        {
            get
            {
                return showing;
            }
        }
    }
}