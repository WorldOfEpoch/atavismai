using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismMobCreator : UIAtavismWindowBase
    {
        static UIAtavismMobCreator instance;
        [SerializeField] GameObject spawnMarkerTemplate;
        [SerializeField] GameObject patrolMarkerTemplate;
        [SerializeField] GameObject spawnRoamTemplate;
        [SerializeField] GameObject spawnAggroTemplate;

         Label templateName;
         Label templateName2;
         Label templateName3;
         Label templateName4;
         Label templateName5;
         private Button templateButton;
         private Button templateButton2;
         private Button templateButton3;
         private Button templateButton4;
         private Button templateButton5;
         UITextField despawnTimeInput;
         UITextField respawnTimeInput;
         UITextField respawnTimeMaxInput;
         UITextField startTimeInput;
         UITextField endTimeInput;
         Label alternateTemplateName;
         Label alternateTemplateName2;
         Label alternateTemplateName3;
         Label alternateTemplateName4;
         Label alternateTemplateName5;
         private Button alternateTemplateButton;
         private Button alternateTemplateButton2;
         private Button alternateTemplateButton3;
         private Button alternateTemplateButton4;
         private Button alternateTemplateButton5;
         UITextField roamRatiusInput;
         UITextField roamDelayMinInput;
         UITextField roamDelayMaxInput;
         Toggle roamRollTimeEachTime;
         Label selectedPatrolPathName;
         Button selectedPatrolPathButton;
         Button selectedPatrolPathClearButton;
         UITextField pickupItemInput;
         Toggle offersBank;
         Label merchantTableValue;
         private Button merchantTableButton;
         private Button merchantTableClearButton;
         private Button startQuestsButton;
         private Button endQuestsButton;
         private Button dialogueButton;
         private Button editPositionButton;
         private Button spawnUpdateButton;
         private Button spawnDeleteButton;
         private Button spawnBackButton;

         private Button mobClearButton;
         private Button mobBackButton;

         private Button pathCreateButton;
         private Button pathClearButton;
         private Button pathBackButton;

         private Button startQuestBackButton;
         private Button endQuestBackButton;

         private Button dialogueBackButton;

         private Button merchantClearButton;
         private Button merchantBackButton;

         private Button pathAddPointButton;
         private Button createPathBackButton;

         private Button playerPositionButton;
         private Button editPositionBackButton;

         private Button spawnNewButton;
         private Button selectSpawnButton;

         UITextField positionXInput;
         UITextField positionYInput;
         UITextField positionZInput;
        MobSpawn spawnInCreation;
        [SerializeField] VisualTreeAsset rowPrefab;
        [SerializeField] VisualTreeAsset rowPathPrefab;
         VisualElement spawnPanel;
         VisualElement mobListPanel;
         VisualElement startQuestPanel;
         VisualElement endQuestPanel;
         VisualElement dialogPanel;
         VisualElement merchantPanel;
         VisualElement selectSpawnPanel;
         VisualElement patrolPathPanel;
         VisualElement patrolPathCreatePanel;
         VisualElement editPositionPanel;

         ListView mobTemplateGrid;
         ListView startQuestAvailableGrid;
         ListView startQuestSelectedGrid;
         ListView endQuestAvailableGrid;
         ListView endQuestSelectedGrid;
         ListView dialogAvailableGrid;
         ListView dialogSelectedGrid;
         ListView merchantGrid;
         ListView patrolPathGrid;
         ListView patrolPathCreateGrid;
         Button spawnButton;
         VisualElement startPanel;
        
         UITextField lingerTimeInput;
         Toggle travelReverse;
         UITextField PathNameInput;
         UITextField mobSearchInput;
         Button markersButton;
         Button roamButton;
         Button aggroButton;

         List<MobTemplate> mobTemplates = new List<MobTemplate>();
         List<MobTemplate> mobTemplatesSearched = new List<MobTemplate>();
         List<QuestTemplate> questTemplates = new List<QuestTemplate>();
         List<QuestTemplate> questTemplatesAvailable = new List<QuestTemplate>();
        List<DialogueTemplate> dialogueTemplates = new List<DialogueTemplate>();
        List<DialogueTemplate> dialogueTemplatesAvailable = new List<DialogueTemplate>();
        List<MerchantTableTemplate> merchantTables = new List<MerchantTableTemplate>();
        Dictionary<int, PatrolPath> patrolPaths = new Dictionary<int, PatrolPath>();
        bool hasAccess = false;
        bool accessChecked = false;
        bool mobselectState = false;
        Dictionary<int, MobSpawn> mobSpawns = new Dictionary<int, MobSpawn>();
        string lingerTime = "0";
        bool showingMarker = false;
        bool showingRoam = false;
        bool showingAggro = false;


         protected override void OnEnable()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            
            base.OnEnable();
           
            // Debug.LogError("UIAtavismMobCreator OnEnable End");
        }
        protected override void registerEvents()
        {
            NetworkAPI.RegisterExtensionMessageHandler("world_developer_response", WorldDeveloperHandler);
            NetworkAPI.RegisterExtensionMessageHandler("mobTemplates", HandleMobTemplateUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("questTemplates", HandleQuestTemplateUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("dialogueTemplates", HandleDialogueTemplateUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("merchantTables", HandleMerchantTableUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("patrolPoints", HandlePatrolPathUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("add_visible_spawn_marker", HandleSpawnList);
            NetworkAPI.RegisterExtensionMessageHandler("spawn_data", HandleSpawnData);
            NetworkAPI.RegisterExtensionMessageHandler("spawn_marker_added", HandleSpawnAdded);
            NetworkAPI.RegisterExtensionMessageHandler("spawn_marker_deleted", HandleSpawnDeleted);
        }

        protected override void unregisterEvents()
        {
            NetworkAPI.RemoveExtensionMessageHandler("world_developer_response", WorldDeveloperHandler);
            NetworkAPI.RemoveExtensionMessageHandler("mobTemplates", HandleMobTemplateUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("questTemplates", HandleQuestTemplateUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("dialogueTemplates", HandleDialogueTemplateUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("merchantTables", HandleMerchantTableUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("patrolPoints", HandlePatrolPathUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("add_visible_spawn_marker", HandleSpawnList);
            NetworkAPI.RemoveExtensionMessageHandler("spawn_data", HandleSpawnData);
            NetworkAPI.RemoveExtensionMessageHandler("spawn_marker_added", HandleSpawnAdded);
            NetworkAPI.RemoveExtensionMessageHandler("spawn_marker_deleted", HandleSpawnDeleted);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            startPanel = uiWindow.Query<VisualElement>("start-panel");
            spawnPanel = uiWindow.Query<VisualElement>("spawner-panel");
            mobListPanel = uiWindow.Query<VisualElement>("mob-panel");
            startQuestPanel = uiWindow.Query<VisualElement>("start-quest-panel");
            endQuestPanel = uiWindow.Query<VisualElement>("end-quest-panel");
            dialogPanel = uiWindow.Query<VisualElement>("dialogue-panel");
            merchantPanel = uiWindow.Query<VisualElement>("merchant-table-panel");
            selectSpawnPanel = uiWindow.Query<VisualElement>("select-spawner-panel");
            patrolPathPanel = uiWindow.Query<VisualElement>("select-path-panel");
            patrolPathCreatePanel = uiWindow.Query<VisualElement>("create-path-panel");
            editPositionPanel = uiWindow.Query<VisualElement>("edit-position-panel");

            spawnNewButton = startPanel.Query<Button>("spawn-new-button");
            spawnNewButton.clicked += ShowSpawnMobPanel;
            selectSpawnButton = startPanel.Query<Button>("select-spawn-button");
            selectSpawnButton.clicked += ShowSelectSpawnPanel;

//Spawn Panel
            templateName = spawnPanel.Query<Label>("template1-name");
            templateName2 = spawnPanel.Query<Label>("template2-name");
            templateName3 = spawnPanel.Query<Label>("template3-name");
            templateName4 = spawnPanel.Query<Label>("template4-name");
            templateName5 = spawnPanel.Query<Label>("template5-name");

           // Debug.LogError("Spawner " + templateName);
            templateButton = spawnPanel.Query<Button>("template1-button");
            templateButton2 = spawnPanel.Query<Button>("template2-button");
            templateButton3 = spawnPanel.Query<Button>("template3-button");
            templateButton4 = spawnPanel.Query<Button>("template4-button");
            templateButton5 = spawnPanel.Query<Button>("template5-button");
            templateButton.clicked += ShowMobTemplates1;
            templateButton2.clicked += ShowMobTemplates2;
            templateButton3.clicked += ShowMobTemplates3;
            templateButton4.clicked += ShowMobTemplates4;
            templateButton5.clicked += ShowMobTemplates5;
            despawnTimeInput = spawnPanel.Query<UITextField>("despawn-time");
            respawnTimeInput = spawnPanel.Query<UITextField>("respawn-time-min");
            respawnTimeMaxInput = spawnPanel.Query<UITextField>("respawn-time-max");
            startTimeInput = spawnPanel.Query<UITextField>("start-time");
            endTimeInput = spawnPanel.Query<UITextField>("end-time");
            alternateTemplateName = spawnPanel.Query<Label>("alt-template1-name");
            alternateTemplateName2 = spawnPanel.Query<Label>("alt-template2-name");
            alternateTemplateName3 = spawnPanel.Query<Label>("alt-template3-name");
            alternateTemplateName4 = spawnPanel.Query<Label>("alt-template4-name");
            alternateTemplateName5 = spawnPanel.Query<Label>("alt-template5-name");
            alternateTemplateButton = spawnPanel.Query<Button>("alt-template1-button");
            alternateTemplateButton2 = spawnPanel.Query<Button>("alt-template2-button");
            alternateTemplateButton3 = spawnPanel.Query<Button>("alt-template3-button");
            alternateTemplateButton4 = spawnPanel.Query<Button>("alt-template4-button");
            alternateTemplateButton5 = spawnPanel.Query<Button>("alt-template5-button");
            alternateTemplateButton.clicked += ShowMobTemplatesAlter1;
            alternateTemplateButton2.clicked += ShowMobTemplatesAlter2;
            alternateTemplateButton3.clicked += ShowMobTemplatesAlter3;
            alternateTemplateButton4.clicked += ShowMobTemplatesAlter4;
            alternateTemplateButton5.clicked += ShowMobTemplatesAlter5;
            roamRatiusInput = spawnPanel.Query<UITextField>("roam-radius");
            roamDelayMinInput = spawnPanel.Query<UITextField>("roam-delay-min");
            roamDelayMaxInput = spawnPanel.Query<UITextField>("roam-delay-max");
            roamRollTimeEachTime = spawnPanel.Query<Toggle>("roam-roll-time-each-time");
            
            selectedPatrolPathName = spawnPanel.Query<Label>("patrol-path-name");
            selectedPatrolPathButton = spawnPanel.Query<Button>("patrol-path-button");
            selectedPatrolPathButton.clicked += ShowPatrolPath;
            selectedPatrolPathClearButton = spawnPanel.Query<Button>("clear-path-button");
            selectedPatrolPathClearButton.clicked += PatrolPathClear;
            merchantTableValue = spawnPanel.Query<Label>("merchant-table-name");
            merchantTableButton = spawnPanel.Query<Button>("merchant-table-button");
            merchantTableButton.clicked += ShowMerchandTables;
            merchantTableClearButton = spawnPanel.Query<Button>("merchant-table-clear-button");
            merchantTableClearButton.clicked += MerchandTableClear;
//        pickupItemInput = uiWindow.Query<UITextField>("faction");//?
            offersBank = spawnPanel.Query<Toggle>("bank-toggle");
            offersBank.RegisterValueChangedCallback(SetBank);
            
            startQuestsButton = spawnPanel.Query<Button>("start-quests-button");
            startQuestsButton.clicked += ShowStartQuest;
            endQuestsButton = spawnPanel.Query<Button>("end-quests-button");
            endQuestsButton.clicked += ShowEndQuest;
            dialogueButton = spawnPanel.Query<Button>("dialogue-button");
            dialogueButton.clicked += ShowDialogues;
            editPositionButton = spawnPanel.Query<Button>("spawner-edit-position-button");
            editPositionButton.clicked += ShowEditPosition;

            spawnButton = spawnPanel.Query<Button>("spawner-spawn-button");
            spawnUpdateButton = spawnPanel.Query<Button>("spawner-save-button");
            spawnDeleteButton = spawnPanel.Query<Button>("spawner-delete-button");
            spawnBackButton = spawnPanel.Query<Button>("spawner-back-button");
            spawnButton.clicked += SpawnMobHere;
            spawnUpdateButton.clicked += UpdateSpawn;
            spawnDeleteButton.clicked += DeleteSpawn;
            spawnBackButton.clicked += CancelSpawn;
//Mob Panel
            mobSearchInput = mobListPanel.Query<UITextField>("mob-template-search");
            mobSearchInput.RegisterValueChangedCallback<string>(ShowMobTemplatesSearch);
            mobTemplateGrid = mobListPanel.Query<ListView>("mob-list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = mobTemplateGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            mobTemplateGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            mobTemplateGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var tmpl = mobTemplatesSearched[index];
                entry.SetEntryDetails(tmpl.name, tmpl.ID, true, false, false, false, false, false, index, this);
            };


            mobClearButton = mobListPanel.Query<Button>("mob-clear-button");
            mobBackButton = mobListPanel.Query<Button>("mob-back-button");
            mobClearButton.clicked += ClearTemplate;
            mobBackButton.clicked += HideMobTemplate;
//Select Path 
            patrolPathGrid = patrolPathPanel.Query<ListView>("path-list");
#if UNITY_6000_0_OR_NEWER    
             scrollView = patrolPathGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            patrolPathGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            patrolPathGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var tmpl = patrolPaths.Values.ToList()[index];
                entry.SetEntryDetails(tmpl.name, tmpl.pathID, false, false, false, false, false, true, index, this);
            };
            pathCreateButton = patrolPathPanel.Query<Button>("path-create-button");
            pathClearButton = patrolPathPanel.Query<Button>("path-clear-button");
            pathBackButton = patrolPathPanel.Query<Button>("path-back-button");
            pathCreateButton.clicked += ShowPatrolPathCreate;
            pathClearButton.clicked += PatrolPathClear;
            pathBackButton.clicked += HidePatrolPath;
//Create Path            
            PathNameInput = patrolPathCreatePanel.Query<UITextField>("path-name-input");
            patrolPathCreateGrid = patrolPathCreatePanel.Query<ListView>("path-points-list");
#if UNITY_6000_0_OR_NEWER    
            scrollView = patrolPathCreateGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            patrolPathCreateGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorPathEntry newListEntryLogic = new UIAtavismMobCreatorPathEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPathPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            patrolPathCreateGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorPathEntry);
                var point = this.spawnInCreation.patrolPoints[index];
                entry.SetEntryDetails(
                    point.marker.transform.position.x + "," + point.marker.transform.position.y + "," +
                    point.marker.transform.position.z, point, this);
            };
            lingerTimeInput = patrolPathCreatePanel.Query<UITextField>("path-linger-time");
            travelReverse = patrolPathCreatePanel.Query<Toggle>("path-travel-reverse");
            travelReverse.RegisterValueChangedCallback(SetTravelReverse);
            pathAddPointButton = patrolPathCreatePanel.Query<Button>("path-add-point-button");
            pathAddPointButton.clicked += PatrolPathAddPointClicked;
            createPathBackButton = patrolPathCreatePanel.Query<Button>("create-path-back-button");
            createPathBackButton.clicked += HidePatrolPathCreate;

//Start Quests
            startQuestAvailableGrid = startQuestPanel.Query<ListView>("start-quest-list");
#if UNITY_6000_0_OR_NEWER    
            scrollView = startQuestAvailableGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            startQuestAvailableGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            startQuestAvailableGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var tmpl = questTemplatesAvailable[index];
                entry.SetEntryDetails(tmpl.title, tmpl.questID, false, true, false, false, false, false, index, this);
            };
            startQuestSelectedGrid = startQuestPanel.Query<ListView>("selected-start-quest-list");
#if UNITY_6000_0_OR_NEWER    
             scrollView = startQuestSelectedGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            startQuestSelectedGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            startQuestSelectedGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var qid = this.spawnInCreation.startsQuests[index];
                var tmpl = GetQuestTemplate(qid);
                entry.SetEntryDetails(tmpl.title, tmpl.questID, false, true, false, false, false, false, index, this);
            };
            startQuestBackButton = startQuestPanel.Query<Button>("start-quests-back-button");
            startQuestBackButton.clicked += HideStartQuest;
//End Quests
            endQuestAvailableGrid = endQuestPanel.Query<ListView>("end-quest-list");
#if UNITY_6000_0_OR_NEWER    
            scrollView = endQuestAvailableGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            endQuestAvailableGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            endQuestAvailableGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var tmpl = questTemplatesAvailable[index];
                entry.SetEntryDetails(tmpl.title, tmpl.questID, false, false, true, false, false, false, index, this);
            };
            endQuestSelectedGrid = endQuestPanel.Query<ListView>("selected-end-quest-list");
#if UNITY_6000_0_OR_NEWER    
            scrollView = endQuestSelectedGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            endQuestSelectedGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            endQuestSelectedGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var qid = this.spawnInCreation.endsQuests[index];
                var tmpl = GetQuestTemplate(qid);
                entry.SetEntryDetails(tmpl.title, tmpl.questID, false, false, true, false, false, false, index, this);
            };
            endQuestBackButton = endQuestPanel.Query<Button>("end-quests-back-button");
            endQuestBackButton.clicked += HideEndQuest;
//Dialogues
            dialogAvailableGrid = dialogPanel.Query<ListView>("dialogue-list");
#if UNITY_6000_0_OR_NEWER    
             scrollView = dialogAvailableGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            dialogAvailableGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            dialogAvailableGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var tmpl = dialogueTemplatesAvailable[index];
                entry.SetEntryDetails(tmpl.title, tmpl.dialogueID, false, false, false, false, true, false, index,
                    this);
            };
            dialogSelectedGrid = dialogPanel.Query<ListView>("selected-dialogue-list");
#if UNITY_6000_0_OR_NEWER    
            scrollView = dialogSelectedGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            dialogSelectedGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            dialogSelectedGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var dial = this.spawnInCreation.startsDialogues[index];
                DialogueTemplate tmpl = GetDialogueTemplate(dial);
                entry.SetEntryDetails(tmpl.title, tmpl.dialogueID, false, false, false, false, true, false, index,
                    this);
            };
            dialogueBackButton = dialogPanel.Query<Button>("dialogue-back-button");
            dialogueBackButton.clicked += HideDialogues;
//Merchant
            merchantGrid = merchantPanel.Query<ListView>("merchant-table-list");
#if UNITY_6000_0_OR_NEWER    
             scrollView = merchantGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            merchantGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismMobCreatorListEntry newListEntryLogic = new UIAtavismMobCreatorListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = rowPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            merchantGrid.bindItem = (item, index) =>
            {
                var entry = (item.userData as UIAtavismMobCreatorListEntry);
                var tmpl = merchantTables[index];
                entry.SetEntryDetails(tmpl.title, tmpl.tableID, false, false, false, true, false, false, index, this);
            };
            merchantClearButton = merchantPanel.Query<Button>("merchant-table-clear-button");
            merchantBackButton = merchantPanel.Query<Button>("merchant-table-back-button");
            merchantClearButton.clicked += MerchandTableClear;
            merchantBackButton.clicked += HideMerchantTable;
//Edit position

            positionXInput = editPositionPanel.Query<UITextField>("position-x");
            positionYInput = editPositionPanel.Query<UITextField>("position-y");
            positionZInput = editPositionPanel.Query<UITextField>("position-z");
            playerPositionButton = editPositionPanel.Query<Button>("player-position-button");
            playerPositionButton.clicked += SetPlayerPosition;
            editPositionBackButton = editPositionPanel.Query<Button>("position-back-button");
            editPositionBackButton.clicked += HideEditPosition;

          
            markersButton = uiWindow.Query<Button>("markers-button");
            markersButton.clicked += ToggleSpawns;
            roamButton = uiWindow.Query<Button>("roam-button");
            roamButton.clicked += ToggleRoam;
            aggroButton = uiWindow.Query<Button>("aggro-button");
            aggroButton.clicked += ToggleAggro;

            spawnPanel.HideVisualElement();
            mobListPanel.HideVisualElement();
            startQuestPanel.HideVisualElement();
            endQuestPanel.HideVisualElement();
            dialogPanel.HideVisualElement();
            merchantPanel.HideVisualElement();
            selectSpawnPanel.HideVisualElement();
            patrolPathPanel.HideVisualElement();
            startPanel.ShowVisualElement();
            patrolPathCreatePanel.HideVisualElement();
            editPositionPanel.HideVisualElement();


        //    Debug.LogError("UIAtavismMobCreator registerUI End");
            return true;
        }




        protected override bool unregisterUI()
        {
            base.unregisterUI();


            return true;
        }

        
        
        
        // Update is called once per frame
        void Update()
        {
            base.Update();
            if (mobselectState && Input.GetMouseButtonDown(0))
            {
                // Do raycast with layer mask of the spawn makers layer
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                // Casts the ray and get the first game object hit
                LayerMask layerMask = 1 << spawnMarkerTemplate.layer;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.transform.gameObject.GetComponent<SpawnMarker>() != null)
                    {
                        SpawnSelected(hit.transform.gameObject.GetComponent<SpawnMarker>().MarkerID);
                    }
                }
            }
        }
        
        private void ShowMobTemplates1()
        {
            ShowMobTemplates(1);
        }
        private void ShowMobTemplates2()
        {
            ShowMobTemplates(2);
        }
        private void ShowMobTemplates3()
        {
            ShowMobTemplates(3);
        }
        private void ShowMobTemplates4()
        {
            ShowMobTemplates(4);
        }
        private void ShowMobTemplates5()
        {
            ShowMobTemplates(5);
        }
        private void ShowMobTemplatesAlter1()
        {
            ShowMobTemplatesAlter(1);
        }
        private void ShowMobTemplatesAlter2()
        {
            ShowMobTemplatesAlter(2);
        }
        private void ShowMobTemplatesAlter3()
        {
            ShowMobTemplatesAlter(3);
        }
        private void ShowMobTemplatesAlter4()
        {
            ShowMobTemplatesAlter(4);
        }
        private void ShowMobTemplatesAlter5()
        {
            ShowMobTemplatesAlter(5);
        }

        private void PatrolPathClear()
        {
            PatrolPathClicked(-1);
        }
        private void MerchandTableClear()
        {
           MerchandTableClicked(-1);
        }
        public void SetPlayerPosition()
        {
            spawnInCreation.marker.transform.position = ClientAPI.GetPlayerObject().Position;
            spawnInCreation.marker.transform.rotation = ClientAPI.GetPlayerObject().Orientation;
            spawnInCreation.position = ClientAPI.GetPlayerObject().Position;
            spawnInCreation.orientation = ClientAPI.GetPlayerObject().Orientation;
            ShowEditPosition();
        }
        public void ShowEditPosition()
        {
            editPositionPanel.ShowVisualElement();
            Vector3 position = spawnInCreation.marker.transform.position;
            positionXInput.value = position.x.ToString("0.00");
            //   position.x = float.Parse(posX);
            positionYInput.value = position.y.ToString("0.00");
            //  position.y = float.Parse(posY);
            positionZInput.value = position.z.ToString("0.00");
            // position.z = float.Parse(posZ);
        }

        public void HideEditPosition()
        {
            Vector3 position = new Vector3(float.Parse(positionXInput.text), float.Parse(positionYInput.text), float.Parse(positionZInput.text));

            spawnInCreation.marker.transform.position = position;
            spawnInCreation.position = position;
            editPositionPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();

        }



        public void ToggleBuildingModeEnabled()
        {
            if (!accessChecked)
            {
                CheckAccess();
            }
            if (!showing && hasAccess)
            {
                Show();
                startPanel.ShowVisualElement();
                spawnPanel.HideVisualElement();
                mobListPanel.HideVisualElement();
                startQuestPanel.HideVisualElement();
                endQuestPanel.HideVisualElement();
                dialogPanel.HideVisualElement();
                merchantPanel.HideVisualElement();
                selectSpawnPanel.HideVisualElement();
                patrolPathPanel.HideVisualElement();
                patrolPathCreatePanel.HideVisualElement();
                editPositionPanel.HideVisualElement();
                GetMobTemplates();
                AtavismUIUtility.BringToFront(this.gameObject);
            }
            else
            {
                Hide();
                ClearSpawns();
                ClearPatrolPath();
            }

        }

        void CheckAccess()
        {
            if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetPlayerObject().PropertyExists("adminLevel"))
            {
                int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
                if (adminLevel == 5)
                {
                    hasAccess = true;
                }
                else
                {
                    int currentWorld = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "world");
                    if (currentWorld > 0)
                    {
                        Dictionary<string, object> props = new Dictionary<string, object>();
                        props.Add("senderOid", ClientAPI.GetPlayerOid());
                        props.Add("world", currentWorld);
                        NetworkAPI.SendExtensionMessage(0, false, "ao.REQUEST_DEVELOPER_ACCESS", props);
                    }
                }
                accessChecked = true;
            }
        }


        public void ShowSelectSpawnPanel()
        {
            startPanel.HideVisualElement();
            selectSpawnPanel.ShowVisualElement();
            mobselectState = true;
        }

        public void HideSelectSpawnPanel()
        {
            startPanel.ShowVisualElement();
            selectSpawnPanel.HideVisualElement();
            mobselectState = false;
        }

        public void ShowSpawnMobPanel()
        {
            //   spawnPanel.ShowVisualElement();
            //  GetMobTemplates();
            spawnInCreation = new MobSpawn();
            spawnPanel.ShowVisualElement();
            spawnDeleteButton.HideVisualElement();
            spawnButton.ShowVisualElement();
            spawnUpdateButton.HideVisualElement();
            editPositionButton.HideVisualElement();
            ShowSpawnMob();
        }




        void ShowSpawnMob()
        {
            startPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();
            templateName.text = spawnInCreation.GetMobTemplateName(1);
            templateName2.text = spawnInCreation.GetMobTemplateName(2);
            templateName3.text = spawnInCreation.GetMobTemplateName(3);
            templateName4.text = spawnInCreation.GetMobTemplateName(4);
            templateName5.text = spawnInCreation.GetMobTemplateName(5);
            despawnTimeInput.value = spawnInCreation.despawnTime.ToString();
            respawnTimeInput.value = spawnInCreation.respawnTime.ToString();
            respawnTimeMaxInput.value = spawnInCreation.respawnTimeMax.ToString();
            startTimeInput.value = spawnInCreation.spawnActiveStartHour;
            endTimeInput.value = spawnInCreation.spawnActiveEndHour;
            alternateTemplateName.text = spawnInCreation.GetAlternateMobTemplateName(1);
            alternateTemplateName2.text = spawnInCreation.GetAlternateMobTemplateName(2);
            alternateTemplateName3.text = spawnInCreation.GetAlternateMobTemplateName(3);
            alternateTemplateName4.text = spawnInCreation.GetAlternateMobTemplateName(4);
            alternateTemplateName5.text = spawnInCreation.GetAlternateMobTemplateName(5);
            roamRatiusInput.value = spawnInCreation.roamRadius.ToString("N1");
            if(roamDelayMinInput!=null) roamDelayMinInput.value = spawnInCreation.roamDelayMin.ToString("N1");
            if(roamDelayMaxInput!=null) roamDelayMaxInput.value = spawnInCreation.roamDelayMax.ToString("N1");
            if(roamRollTimeEachTime!=null) roamRollTimeEachTime.value = spawnInCreation.roamRollTimeEachTime;
            //  pickupItemInput.text = spawnInCreation.pickupItemID.ToString();
            offersBank.value = spawnInCreation.otherActions.Contains("Bank");
            merchantTableValue.text = spawnInCreation.merchantTable.ToString();
            string pname = "";
            foreach (PatrolPath tmpl in patrolPaths.Values)
            {
                if (tmpl.pathID.Equals(spawnInCreation.patrolPath))
                    pname = tmpl.name;
            }
            if (selectedPatrolPathName != null)
                selectedPatrolPathName.text = spawnInCreation.patrolPath + " " + pname;
            /*     if (spawnInCreation.mobTemplate != null && mobCreationState == MobCreationState.EditSpawn) {
                    if (GUILayout.Button("Edit Position")) {
                   //     propertySelectState = MobPropertySelectState.SpawnPositioning;
                    }

               }*/
        }

        public void SelectTemplate(int id)
        {
            if (alternateTemplate)
            {
                foreach (MobTemplate mt in mobTemplates)
                {
                    if (mt.ID.Equals(id))
                    {
                        switch (templSelectId)
                        {
                            case 1:
                                spawnInCreation.alternateMobTemplate = mt;
                                spawnInCreation.alternateMobTemplateID = mt.ID;
                                break;
                            case 2:
                                spawnInCreation.alternateMobTemplate2 = mt;
                                spawnInCreation.alternateMobTemplateID2 = mt.ID;
                                break;
                            case 3:
                                spawnInCreation.alternateMobTemplate3 = mt;
                                spawnInCreation.alternateMobTemplateID3 = mt.ID;
                                break;
                            case 4:
                                spawnInCreation.alternateMobTemplate4 = mt;
                                spawnInCreation.alternateMobTemplateID4 = mt.ID;
                                break;
                            case 5:
                                spawnInCreation.alternateMobTemplate5 = mt;
                                spawnInCreation.alternateMobTemplateID5 = mt.ID;
                                break;
                        }
                        break;
                    }
                }
            }
            else
            {
                foreach (MobTemplate mt in mobTemplates)
                {
                    if (mt.ID.Equals(id))
                    {
                        switch (templSelectId)
                        {
                            case 1:
                                spawnInCreation.mobTemplate = mt;
                                spawnInCreation.mobTemplateID = mt.ID;
                                break;
                            case 2:
                                spawnInCreation.mobTemplate2 = mt;
                                spawnInCreation.mobTemplateID2 = mt.ID;
                                break;
                            case 3:
                                spawnInCreation.mobTemplate3 = mt;
                                spawnInCreation.mobTemplateID3 = mt.ID;
                                break;
                            case 4:
                                spawnInCreation.mobTemplate4 = mt;
                                spawnInCreation.mobTemplateID4 = mt.ID;
                                break;
                            case 5:
                                spawnInCreation.mobTemplate5 = mt;
                                spawnInCreation.mobTemplateID5 = mt.ID;
                                break;
                        }
                    }
                }
            }
            mobListPanel.HideVisualElement();
            ShowSpawnMob();
        }
        public void ClearTemplate()
        {
            if (alternateTemplate)
            {
                switch (templSelectId)
                {
                    case 1:
                        spawnInCreation.alternateMobTemplate = null;
                        spawnInCreation.alternateMobTemplateID = -1;
                        break;
                    case 2:
                        spawnInCreation.alternateMobTemplate2 = null;
                        spawnInCreation.alternateMobTemplateID2 = -1;
                        break;
                    case 3:
                        spawnInCreation.alternateMobTemplate3 = null;
                        spawnInCreation.alternateMobTemplateID3 = -1;
                        break;
                    case 4:
                        spawnInCreation.alternateMobTemplate4 = null;
                        spawnInCreation.alternateMobTemplateID4 = -1;
                        break;
                    case 5:
                        spawnInCreation.alternateMobTemplate5 = null;
                        spawnInCreation.alternateMobTemplateID5 = -1;
                        break;
                }
            }
            else
            {
                switch (templSelectId)
                {
                    case 1:
                        spawnInCreation.mobTemplate = null;
                        spawnInCreation.mobTemplateID = -1;
                        break;
                    case 2:
                        spawnInCreation.mobTemplate2 = null;
                        spawnInCreation.mobTemplateID2 = -1;
                        break;
                    case 3:
                        spawnInCreation.mobTemplate3 = null;
                        spawnInCreation.mobTemplateID3 = -1;
                        break;
                    case 4:
                        spawnInCreation.mobTemplate4 = null;
                        spawnInCreation.mobTemplateID4 = -1;
                        break;
                    case 5:
                        spawnInCreation.mobTemplate5 = null;
                        spawnInCreation.mobTemplateID5 = -1;
                        break;
                }
            }
            mobListPanel.HideVisualElement();
            ShowSpawnMob();
        }

        bool alternateTemplate = false;
        int templSelectId = 1;
 
        public void HideMobTemplate()
        {
            spawnPanel.ShowVisualElement();
            mobListPanel.HideVisualElement();
        }
        public void ShowMobTemplates(int template)
        {
            spawnPanel.HideVisualElement();
            this.alternateTemplate = false;
            this.templSelectId = template;
            mobListPanel.ShowVisualElement();
            mobTemplatesSearched.Clear();
            foreach (MobTemplate tmpl in mobTemplates)
            {
                if (tmpl.name.ToLower().Contains(mobSearchInput.text.ToLower()))
                {
                    mobTemplatesSearched.Add(tmpl);
                }
            }

            mobTemplateGrid.itemsSource = mobTemplatesSearched;
            mobTemplateGrid.Rebuild();
            mobTemplateGrid.selectedIndex = -1;
        }

        public void ShowMobTemplatesAlter(int template)
        {
            spawnPanel.HideVisualElement();
            this.alternateTemplate = true;
            this.templSelectId = template;
            mobListPanel.ShowVisualElement();
            mobTemplatesSearched.Clear();
            foreach (MobTemplate tmpl in mobTemplates)
            {
                if (tmpl.name.ToLower().Contains(mobSearchInput.text.ToLower()))
                {
                    mobTemplatesSearched.Add(tmpl);
                }
            }

            mobTemplateGrid.itemsSource = mobTemplatesSearched;
            mobTemplateGrid.Rebuild();
            mobTemplateGrid.selectedIndex = -1;
        }

        public void ShowMobTemplatesSearch(ChangeEvent<string> evt)
        {
            mobListPanel.ShowVisualElement();
            mobTemplatesSearched.Clear();
            foreach (MobTemplate tmpl in mobTemplates)
            {
                if (tmpl.name.ToLower().Contains(mobSearchInput.text.ToLower()))
                {
                    mobTemplatesSearched.Add(tmpl);
                }
            }

            mobTemplateGrid.itemsSource = mobTemplatesSearched;
            mobTemplateGrid.Rebuild();
            mobTemplateGrid.selectedIndex = -1;
            int pos = 0;
        }

        public void StartQuestClicked(int id)
        {
            foreach (QuestTemplate tmpl in questTemplates)
            {
                if (tmpl.questID.Equals(id))
                {
                    if (!spawnInCreation.startsQuests.Contains(tmpl.questID))
                    {
                        spawnInCreation.startsQuests.Add(tmpl.questID);
                    }
                    else
                    {
                        spawnInCreation.startsQuests.Remove(tmpl.questID);
                    }
                    break;
                }
            }
            ShowStartQuest();
        }

        public void EndQuestClicked(int id)
        {
            foreach (QuestTemplate tmpl in questTemplates)
            {
                if (tmpl.questID.Equals(id))
                {
                    if (!spawnInCreation.endsQuests.Contains(tmpl.questID))
                    {
                        spawnInCreation.endsQuests.Add(tmpl.questID);
                    }
                    else
                    {
                        spawnInCreation.endsQuests.Remove(tmpl.questID);
                    }
                    break;
                }
            }
            ShowEndQuest();
        }

        public void ShowStartQuest()
        {
            spawnPanel.HideVisualElement();
            startQuestPanel.ShowVisualElement();

            // Draw Display names
            startQuestSelectedGrid.itemsSource = spawnInCreation.startsQuests;
            startQuestSelectedGrid.Rebuild();
            startQuestSelectedGrid.selectedIndex = -1;
            // Draw Display names
            questTemplatesAvailable.Clear();
            foreach (QuestTemplate tmpl in questTemplates)
            {
                if (!spawnInCreation.startsQuests.Contains(tmpl.questID))
                {
                    questTemplatesAvailable.Add(tmpl);
                }
            }
            startQuestAvailableGrid.itemsSource = questTemplatesAvailable;
            startQuestAvailableGrid.Rebuild();
            startQuestAvailableGrid.selectedIndex = -1;
        }

        public void ShowEndQuest()
        {
            spawnPanel.HideVisualElement();
            endQuestPanel.ShowVisualElement();
            // Draw Display names
            endQuestSelectedGrid.itemsSource = spawnInCreation.endsQuests;
            endQuestSelectedGrid.Rebuild();
            endQuestSelectedGrid.selectedIndex = -1;
            // Draw Display names
            questTemplatesAvailable.Clear();
            foreach (QuestTemplate tmpl in questTemplates)
            {
                if (!spawnInCreation.endsQuests.Contains(tmpl.questID))
                {
                    questTemplatesAvailable.Add(tmpl);
                }
            }
            endQuestAvailableGrid.itemsSource = questTemplatesAvailable;
            endQuestAvailableGrid.Rebuild();
            endQuestAvailableGrid.selectedIndex = -1;

        }

        public void HideStartQuest()
        {
            startQuestPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();
        }

        public void HideEndQuest()
        {
            endQuestPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();
        }

        public void HideMerchantTable()
        {
            merchantPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();
        }

        public void MerchandTableClicked(int id)
        {
            spawnInCreation.merchantTable = id;
            //  if (id != -1)
            merchantPanel.HideVisualElement();
            ShowSpawnMob();
        }

        public void ShowMerchandTables()
        {
            spawnPanel.HideVisualElement();
            merchantPanel.ShowVisualElement();

            merchantGrid.itemsSource = merchantTables;
            merchantGrid.Rebuild();
            merchantGrid.selectedIndex = -1;
        }
        public void HideDialogues()
        {
            dialogPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();

        }
        public void DialoguesClicked(int id)
        {
            foreach (DialogueTemplate tmpl in dialogueTemplates)
            {
                if (tmpl.dialogueID.Equals(id))
                {
                    if (!spawnInCreation.startsDialogues.Contains(tmpl.dialogueID))
                    {
                        spawnInCreation.startsDialogues.Add(tmpl.dialogueID);
                    }
                    else
                    {
                        spawnInCreation.startsDialogues.Remove(tmpl.dialogueID);
                    }
                    break;
                }
            }
            ShowDialogues();
        }

        public void ShowDialogues()
        {
            spawnPanel.HideVisualElement();
            dialogPanel.ShowVisualElement();
            // Draw Display names
            dialogSelectedGrid.itemsSource = spawnInCreation.startsDialogues;
            dialogSelectedGrid.Rebuild();
            dialogSelectedGrid.selectedIndex = -1;
            int pos = 0;
            // Draw Display names
            dialogueTemplatesAvailable.Clear();
            foreach (DialogueTemplate tmpl in dialogueTemplates)
            {
                if (!spawnInCreation.startsDialogues.Contains(tmpl.dialogueID))
                {
                    dialogueTemplatesAvailable.Add(tmpl);
                }
            }
            dialogAvailableGrid.itemsSource = dialogueTemplatesAvailable;
            dialogAvailableGrid.Rebuild();
            dialogAvailableGrid.selectedIndex = -1;

        }
        public void HidePatrolPath()
        {
            patrolPathPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();

        }


        public void HidePatrolPathCreate()
        {
            //  ClearPatrolPath();
            // spawnInCreation.patrolPoints.Clear();
            patrolPathCreatePanel.HideVisualElement();
            if (selectedPatrolPathName != null)
                selectedPatrolPathName.text = "New " + PathNameInput.text;
            spawnInCreation.patrolPath = -1;
            spawnPanel.ShowVisualElement();

        }
        public void SetLinger()
        {

        }

        public void PatrolPathAddPointClicked()
        {
            PatrolPoint pp = new PatrolPoint();
            pp.marker = Instantiate<GameObject>(patrolMarkerTemplate);
            pp.marker.transform.position = ClientAPI.GetPlayerObject().Position;
            pp.marker.transform.rotation = ClientAPI.GetPlayerObject().Orientation;
            pp.lingerTime = int.Parse(lingerTimeInput.text);
            spawnInCreation.patrolPoints.Add(pp);
            ShowPatrolPathCreate();
        }

        public void PatrolPathDeletePointClicked(PatrolPoint pp)
        {
            spawnInCreation.patrolPoints.Remove(pp);
            DestroyImmediate(pp.marker);
            ShowPatrolPathCreate();
        }
        /// <summary>
        /// Show Window of Create of the new patrol path
        /// </summary>
        public void ShowPatrolPathCreate()
        {
            // Draw Display names
            patrolPathCreateGrid.itemsSource = spawnInCreation.patrolPoints;
            patrolPathCreateGrid.Rebuild();
            patrolPathCreateGrid.selectedIndex = -1;
            lingerTimeInput.value = lingerTime;
            travelReverse.value = spawnInCreation.travelReverse;
            patrolPathCreatePanel.ShowVisualElement();
            patrolPathPanel.HideVisualElement();
        }
        /// <summary>
        /// set travel path to be  Reverse
        /// </summary>
        public void SetTravelReverse(ChangeEvent<bool> evt)
        {
            spawnInCreation.travelReverse = travelReverse.value;
        }

        public void PatrolPathClicked(int id)
        {
            // Debug.LogError("PatrolPathClicked "+id);
            spawnInCreation.patrolPoints.Clear();
            spawnInCreation.patrolPath = id;
            string pname = "";
            foreach (PatrolPath tmpl in patrolPaths.Values)
            {
                if (tmpl.pathID.Equals(id))
                    pname = tmpl.name;
            }
            if (selectedPatrolPathName != null)
                selectedPatrolPathName.text = id + " " + pname;
            patrolPathPanel.HideVisualElement();
            spawnPanel.ShowVisualElement();
        }

        /// <summary>
        /// Show window with list of partrol path
        /// </summary>
        public void ShowPatrolPath()
        {
            spawnPanel.HideVisualElement();
            patrolPathPanel.ShowVisualElement();
            patrolPathGrid.itemsSource = patrolPaths.Values.ToList();
            patrolPathGrid.Rebuild();
            patrolPathGrid.selectedIndex = -1;
        }

        /// <summary>
        /// Set Bank option to spawner
        /// </summary>

        public void SetBank(ChangeEvent<bool> evt)
        {
            bool _offersBank = spawnInCreation.otherActions.Contains("Bank");
            bool _nowOffersBank = offersBank.value;
            if (_nowOffersBank)
            {
                if (!_offersBank)
                    spawnInCreation.otherActions.Add("Bank");
            }
            else
            {
                spawnInCreation.otherActions.Remove("Bank");
            }

        }
        /// <summary>
        /// Prepare Dictionary of spawn properties to sent 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        Dictionary<string, object> SetMobSpawnMessageProps(Vector3 position, Quaternion orientation)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("playerOid", ClientAPI.GetPlayerOid());
            props.Add("markerID", spawnInCreation.ID);
            props.Add("loc", position);
            props.Add("orient", orientation);
            props.Add("mobTemplate", spawnInCreation.mobTemplateID);
            props.Add("mobTemplate2", spawnInCreation.mobTemplateID2);
            props.Add("mobTemplate3", spawnInCreation.mobTemplateID3);
            props.Add("mobTemplate4", spawnInCreation.mobTemplateID4);
            props.Add("mobTemplate5", spawnInCreation.mobTemplateID5);
            props.Add("respawnTime", spawnInCreation.respawnTime);
            props.Add("respawnTimeMax", spawnInCreation.respawnTimeMax);
            props.Add("despawnTime", spawnInCreation.despawnTime);
            props.Add("numSpawns", 1);
            props.Add("spawnRadius", 0);
            props.Add("spawnActiveStartHour", int.Parse(spawnInCreation.spawnActiveStartHour));
            props.Add("spawnActiveEndHour", int.Parse(spawnInCreation.spawnActiveEndHour));
            props.Add("alternateMobTemplate", spawnInCreation.alternateMobTemplateID);
            props.Add("alternateMobTemplate2", spawnInCreation.alternateMobTemplateID2);
            props.Add("alternateMobTemplate3", spawnInCreation.alternateMobTemplateID3);
            props.Add("alternateMobTemplate4", spawnInCreation.alternateMobTemplateID4);
            props.Add("alternateMobTemplate5", spawnInCreation.alternateMobTemplateID5);
            props.Add("roamRadius", (int)spawnInCreation.roamRadius);
            props.Add("merchantTable", spawnInCreation.merchantTable);
            props.Add("patrolPath", spawnInCreation.patrolPath);
            props.Add("patrolPointsCount", spawnInCreation.patrolPoints.Count);
            props.Add("patrolPointName", PathNameInput.text);
            for (int i = 0; i < spawnInCreation.patrolPoints.Count; i++)
            {
                props.Add("patrolPoint" + i + "x", spawnInCreation.patrolPoints[i].marker.transform.position.x);
                props.Add("patrolPoint" + i + "y", spawnInCreation.patrolPoints[i].marker.transform.position.y);
                props.Add("patrolPoint" + i + "z", spawnInCreation.patrolPoints[i].marker.transform.position.z);
                props.Add("patrolPoint" + i + "linger", spawnInCreation.patrolPoints[i].lingerTime);
            }
            props.Add("patrolPointsTravelReverse", spawnInCreation.travelReverse);
            props.Add("pickupItem", spawnInCreation.pickupItemID);
            props.Add("isChest", spawnInCreation.isChest);
            props.Add("domeID", -1);
            props.Add("startsQuestsCount", spawnInCreation.startsQuests.Count);
            props.Add("endsQuestsCount", spawnInCreation.endsQuests.Count);
            for (int i = 0; i < spawnInCreation.startsQuests.Count; i++)
            {
                props.Add("startsQuest" + i + "ID", spawnInCreation.startsQuests[i]);
            }
            for (int i = 0; i < spawnInCreation.endsQuests.Count; i++)
            {
                props.Add("endsQuest" + i + "ID", spawnInCreation.endsQuests[i]);
            }
            props.Add("startsDialoguesCount", spawnInCreation.startsDialogues.Count);
            for (int i = 0; i < spawnInCreation.startsDialogues.Count; i++)
            {
                props.Add("startsDialogue" + i + "ID", spawnInCreation.startsDialogues[i]);
            }
            props.Add("otherActionsCount", spawnInCreation.otherActions.Count);
            for (int i = 0; i < spawnInCreation.otherActions.Count; i++)
            {
                props.Add("otherAction" + i, spawnInCreation.otherActions[i]);
            }
            return props;
        }

        /// <summary>
        /// Send Spawn new mob to the server
        /// </summary>
        public void SpawnMobHere()
        {

            if (spawnInCreation.mobTemplateID == -1 && spawnInCreation.mobTemplateID2 == -1 &&
                spawnInCreation.mobTemplateID3 == -1
                && spawnInCreation.mobTemplateID4 == -1 && spawnInCreation.mobTemplateID5 == -1
               )
            {
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", new []{"The mob template can not be empty"});
                return;
            }
            
            spawnInCreation.despawnTime = int.Parse(despawnTimeInput.text);
            spawnInCreation.respawnTime = int.Parse(respawnTimeInput.text);
            spawnInCreation.respawnTimeMax = int.Parse(respawnTimeMaxInput.text);
            spawnInCreation.spawnActiveStartHour = startTimeInput.text;
            spawnInCreation.spawnActiveEndHour = endTimeInput.text;
            spawnInCreation.roamRadius = float.Parse(roamRatiusInput.text);
            if(roamDelayMinInput!=null) spawnInCreation.roamDelayMin = float.Parse(roamDelayMinInput.text);
            if(roamDelayMaxInput!=null) spawnInCreation.roamDelayMax = float.Parse(roamDelayMaxInput.text);
            if(roamRollTimeEachTime!=null) spawnInCreation.roamRollTimeEachTime = roamRollTimeEachTime.value;
            
            Vector3 position = ClientAPI.GetPlayerObject().Position;
            position.y = Mathf.Ceil(position.y*100f)/100f;
            Dictionary<string, object> props = SetMobSpawnMessageProps(position, ClientAPI.GetPlayerObject().Orientation);
            NetworkAPI.SendExtensionMessage(0, false, "mob.CREATE_MOB_SPAWN", props);
            ClientAPI.Write("Sending create mob spawn");
            ClearPatrolPath();
            Dictionary<string, object> sProps = new Dictionary<string, object>();
            sProps.Add("senderOid", ClientAPI.GetPlayerOid());
            sProps.Add("type", "patrol");
            NetworkAPI.SendExtensionMessage(0, false, "mob.GET_TEMPLATES", sProps);
        }
        /// <summary>
        /// Send Delete Spawn Marker to the server
        /// </summary>
        public void DeleteSpawn()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("senderOid", ClientAPI.GetPlayerOid());
            props.Add("markerID", spawnInCreation.ID);
            NetworkAPI.SendExtensionMessage(0, false, "ao.DELETE_SPAWN_MARKER", props);
            spawnInCreation.DeleteMarker();
            spawnInCreation = null;
            spawnPanel.HideVisualElement();
            startPanel.ShowVisualElement();
        }

        /// <summary>
        /// Cancel Spawn
        /// </summary>
        public void CancelSpawn()
        {
            if (spawnInCreation.ID < 0)
            {
                spawnInCreation.DeleteMarker();
            }
            spawnInCreation = null;
            spawnPanel.HideVisualElement();
            startPanel.ShowVisualElement();
            ClearPatrolPath();
        }

        /// <summary>
        /// Apply Changes in inputs of the spawn
        /// </summary>
        public void ApplyChangespawn()
        {
            spawnInCreation.despawnTime = int.Parse(despawnTimeInput.text);
            spawnInCreation.respawnTime = int.Parse(respawnTimeInput.text);
            spawnInCreation.respawnTimeMax = int.Parse(respawnTimeMaxInput.text);
            spawnInCreation.spawnActiveStartHour = startTimeInput.text;
            spawnInCreation.spawnActiveEndHour = endTimeInput.text;
            spawnInCreation.roamRadius = float.Parse(roamRatiusInput.text);
            if(roamDelayMinInput!=null) spawnInCreation.roamDelayMin = float.Parse(roamDelayMinInput.text);
            if(roamDelayMaxInput!=null) spawnInCreation.roamDelayMax = float.Parse(roamDelayMaxInput.text);
            if(roamRollTimeEachTime!=null) spawnInCreation.roamRollTimeEachTime = roamRollTimeEachTime.value;
        }
        /// <summary>
        /// Send Update Sprawn to server
        /// </summary>
        public void UpdateSpawn()
        {
            spawnInCreation.despawnTime = int.Parse(despawnTimeInput.text);
            spawnInCreation.respawnTime = int.Parse(respawnTimeInput.text);
            spawnInCreation.respawnTimeMax = int.Parse(respawnTimeMaxInput.text);
            spawnInCreation.spawnActiveStartHour = startTimeInput.text;
            spawnInCreation.spawnActiveEndHour = endTimeInput.text;
            spawnInCreation.roamRadius = float.Parse(roamRatiusInput.text);
            if(roamDelayMinInput!=null) spawnInCreation.roamDelayMin = float.Parse(roamDelayMinInput.text);
            if(roamDelayMaxInput!=null) spawnInCreation.roamDelayMax = float.Parse(roamDelayMaxInput.text);
            if(roamRollTimeEachTime!=null) spawnInCreation.roamRollTimeEachTime = roamRollTimeEachTime.value;
            
            Dictionary<string, object> props = SetMobSpawnMessageProps(spawnInCreation.position, spawnInCreation.orientation);
            NetworkAPI.SendExtensionMessage(0, false, "ao.EDIT_SPAWN_MARKER", props);
            ClearPatrolPath();
            spawnPanel.HideVisualElement();
            startPanel.ShowVisualElement();
        }

        /// <summary>
        /// Select Spawner
        /// </summary>
        /// <param name="spawnID"></param>
        public void SpawnSelected(int spawnID)
        {
            if (!mobselectState)
                return;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("senderOid", ClientAPI.GetPlayerOid());
            props.Add("markerID", spawnID);
            NetworkAPI.SendExtensionMessage(0, false, "ao.REQUEST_SPAWN_DATA", props);
        }
        /// <summary>
        /// Geting Quest Template from the list of templates
        /// </summary>
        /// <param name="questID"></param>
        /// <returns></returns>
        QuestTemplate GetQuestTemplate(int questID)
        {
            foreach (QuestTemplate tmpl in questTemplates)
            {
                if (tmpl.questID == questID)
                    return tmpl;
            }
            return null;
        }
        /// <summary>
        /// Geting dialogue from the list od dialoges
        /// </summary>
        /// <param name="dialogueID"></param>
        /// <returns></returns>
        DialogueTemplate GetDialogueTemplate(int dialogueID)
        {
            foreach (DialogueTemplate tmpl in dialogueTemplates)
            {
                if (tmpl.dialogueID == dialogueID)
                    return tmpl;
            }
            return null;
        }

        /// <summary>
        /// Delete all path markers and clear paths
        /// </summary>
        void ClearPatrolPath()
        {
            if (spawnInCreation != null)
            {
                foreach (PatrolPoint pp in spawnInCreation.patrolPoints)
                {
                    DestroyImmediate(pp.marker);
                }
                spawnInCreation.patrolPoints.Clear();
            }
            if (PathNameInput != null)
                PathNameInput.value = "";
            if (selectedPatrolPathName != null)
                selectedPatrolPathName.text = "";
        }

        /// <summary>
        /// Geting Mob Template from the list of templates
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MobTemplate GetMobTemplateByID(int id)
        {
            MobTemplate template = null;
            foreach (MobTemplate tmpl in mobTemplates)
            {
                if (tmpl.ID == id)
                    return tmpl;
            }
            return template;
        }

        void ClearSpawns()
        {
            if (spawnInCreation != null)
                spawnInCreation.DeleteMarker();
            foreach (MobSpawn spawn in mobSpawns.Values)
            {
                spawn.DeleteMarker();
            }
            mobSpawns.Clear();
        }

        public void ToggleSpawns()
        {
            if (showingMarker)
            {
                foreach (MobSpawn spawn in mobSpawns.Values)
                {
                    showingMarker = false;
                    spawn.HideMarker();
                }
                if (markersButton != null)
                    markersButton.style.color = Color.red;
            }
            else
            {
                foreach (MobSpawn spawn in mobSpawns.Values)
                {
                    spawn.ShowMarker();
                    showingMarker = true;
                }
                if (markersButton != null)
                    markersButton.style.color = Color.green;
            }
        }

        public void ToggleAggro()
        {
            if (showingAggro)
            {
                foreach (MobSpawn spawn in mobSpawns.Values)
                {
                    showingAggro = false;
                    spawn.HideAggro();
                }
                if (aggroButton != null)
                    aggroButton.style.color = Color.red;
            }
            else
            {
                if (!showingMarker)
                    ToggleSpawns();
                foreach (MobSpawn spawn in mobSpawns.Values)
                {
                    spawn.ShowAggro();
                    showingAggro = true;
                }
                if (aggroButton != null)
                    aggroButton.style.color = Color.green;
            }
        }

        public void ToggleRoam()
        {
            if (showingRoam)
            {
                foreach (MobSpawn spawn in mobSpawns.Values)
                {
                    spawn.HideRoam();
                    showingRoam = false;
                }
                if (roamButton != null)
                    roamButton.style.color = Color.red;
            }
            else
            {
                if (!showingMarker)
                    ToggleSpawns();
                foreach (MobSpawn spawn in mobSpawns.Values)
                {
                    spawn.ShowRoam();
                    showingRoam = true;
                }
                if (roamButton != null)
                    roamButton.style.color = Color.green;
            }

        }

        public override void Hide()
        {
            ClearSpawns();
            base.Hide();
        }

        public void GetMobTemplates()
        {
            // get mob templates
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("senderOid", ClientAPI.GetPlayerOid());
            props.Add("type", "mob,quests,dialogues,merchantTables,patrol");
            NetworkAPI.SendExtensionMessage(0, false, "mob.GET_TEMPLATES", props);

            // Get spawn markers
            props = new Dictionary<string, object>();
            props.Add("senderOid", ClientAPI.GetPlayerOid());
            NetworkAPI.SendExtensionMessage(0, false, "ao.VIEW_MARKERS", props);
        }
        #region Message Handlers

        public void WorldDeveloperHandler(Dictionary<string, object> props)
        {
            bool isAdmin = (bool)props["isAdmin"];
            bool isDeveloper = (bool)props["isDeveloper"];
            if (isAdmin || isDeveloper)
            {
                hasAccess = true;
            }
            else
            {
                hasAccess = false;
            }
        }

        public void HandleMobTemplateUpdate(Dictionary<string, object> props)
        {
            mobTemplates.Clear();
            int numTemplates = (int)props["numTemplates"];
            for (int i = 0; i < numTemplates; i++)
            {
                MobTemplate template = new MobTemplate();
                template.name = (string)props["mob_" + i + "Name"];
                template.ID = (int)props["mob_" + i + "ID"];
                template.subtitle = (string)props["mob_" + i + "SubTitle"];
                template.species = (string)props["mob_" + i + "Species"];
                template.subspecies = (string)props["mob_" + i + "Subspecies"];
                template.level = (int)props["mob_" + i + "Level"];
                template.attackable = (bool)props["mob_" + i + "Attackable"];
                template.faction = (int)props["mob_" + i + "Faction"];
                template.mobType = (int)props["mob_" + i + "MobType"];
                //template.gender = (string)props["mob_" + i + "Gender"];
                template.scale = (float)props["mob_" + i + "Scale"];
                List<string> displays = new List<string>();
                int numDisplays = (int)props["mob_" + i + "NumDisplays"];
                for (int j = 0; j < numDisplays; j++)
                {
                    string display = (string)props["mob_" + i + "Display" + j];
                    displays.Add(display);
                }
                template.displays = displays;
                /*for itemID in props["mob_%dEquipment" % i]:
                    template.equippedItems.append(GetItemTemplateByID(itemID))
                numLootTables = int(props["mob_%dNumLootTables" % i])
                for j in range(0, numLootTables):
                    lootTableID = int(props["mob_%dLootTable%d" % (i, j)])
                    lootTableChance = int(props["mob_%dLootTable%dChance" % (i, j)])
                    lootTable = LootTableDropEntry()
                    lootTable.itemID = lootTableID
                    lootTable.dropChance = lootTableChance
                    template.lootTables.append(lootTable)
                    //ClientAPI.Write("Loot table %s for mob %s has chance: %s" % (lootTableID, template.name, lootTableChance))*/
                mobTemplates.Add(template);
            }
            ClientAPI.Write("Number of mob templates added: " + mobTemplates.Count);
            //mobCreationState = MobCreationState.SelectTemplate;
            //selectedTemplate = -1;
        }

        public void HandleQuestTemplateUpdate(Dictionary<string, object> props)
        {
            questTemplates.Clear();
            int numTemplates = (int)props["numTemplates"];
            for (int i = 0; i < numTemplates; i++)
            {
                QuestTemplate template = new QuestTemplate();
                template.title = (string)props["quest_" + i + "Title"];
                template.questID = (int)props["quest_" + i + "Id"];
                questTemplates.Add(template);
            }
            ClientAPI.Write("Number of quest templates added: " + questTemplates.Count);
        }

        public void HandleDialogueTemplateUpdate(Dictionary<string, object> props)
        {
            dialogueTemplates.Clear();
            int numTemplates = (int)props["numTemplates"];
            for (int i = 0; i < numTemplates; i++)
            {
                DialogueTemplate template = new DialogueTemplate();
                template.title = (string)props["dialogue_" + i + "Title"];
                template.dialogueID = (int)props["dialogue_" + i + "Id"];
                dialogueTemplates.Add(template);
            }
            ClientAPI.Write("Number of dialogue templates added: " + dialogueTemplates.Count);
        }

        public void HandleMerchantTableUpdate(Dictionary<string, object> props)
        {
            merchantTables.Clear();
            int numTemplates = (int)props["numTemplates"];
            for (int i = 0; i < numTemplates; i++)
            {
                MerchantTableTemplate template = new MerchantTableTemplate();
                template.title = (string)props["merchant_" + i + "Title"];
                template.tableID = (int)props["merchant_" + i + "Id"];
                merchantTables.Add(template);
            }
            ClientAPI.Write("Number of merchant tables added: " + merchantTables.Count);
        }

        public void HandlePatrolPathUpdate(Dictionary<string, object> props)
        {
            patrolPaths.Clear();
            int numTemplates = (int)props["numPatrols"];
            for (int i = 0; i < numTemplates; i++)
            {
                PatrolPath template = new PatrolPath();
                template.name = (string)props["patrol_" + i + "Title"];
                template.pathID = (int)props["patrol_" + i + "Id"];
                patrolPaths[template.pathID] = template;
            }
            ClientAPI.Write("Number of patrol paths added: " + patrolPaths.Count);
        }

        private void HandleSpawnDeleted(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("Got spawn marker delete");
            int id = (int)props["spawnID"];
            if (mobSpawns.ContainsKey(id))
            {
                mobSpawns[id].DeleteMarker();
                mobSpawns.Remove(id);
            }
            AtavismLogger.LogDebugMessage("Removed spawn: " + id);
        }

        public void HandleSpawnList(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("Got spawn list");
            ClearSpawns();
            int numMarkers = (int)props["numMarkers"];
            for (int i = 0; i < numMarkers; i++)
            {
                MobSpawn spawn = new MobSpawn();
                spawn.ID = (int)props["markerID_" + i];
                spawn.position = (Vector3)props["markerLoc_" + i];
                spawn.orientation = (Quaternion)props["markerOrient_" + i];
                spawn.roamRadius = (int)props["markerRoamRadius_" + i];
                spawn.aggroRadius = (int)props["markerAggroRadius_" + i];
                spawn.CreateMarkerObject(spawnMarkerTemplate);
                spawn.CreateRoamObject(spawnRoamTemplate);
                spawn.CreateAggroObject(spawnAggroTemplate);
                if (!showingMarker)
                    spawn.HideMarker();
                if (!showingRoam)
                    spawn.HideRoam();
                if (!showingAggro)
                    spawn.HideAggro();
                mobSpawns.Add(spawn.ID, spawn);
                AtavismLogger.LogDebugMessage("Added spawn: " + spawn.ID);
            }

            if (markersButton != null)
                if (showingMarker)
                {
                    markersButton.style.color = Color.green;
                }
                else
                {
                    markersButton.style.color = Color.red;
                }
            if (roamButton != null)
                if (showingRoam)
                {
                    roamButton.style.color = Color.green;
                }
                else
                {
                    roamButton.style.color = Color.red;
                }
            if (aggroButton != null)
                if (showingAggro)
                {
                    aggroButton.style.color = Color.green;
                }
                else
                {
                    aggroButton.style.color = Color.red;
                }

            if (!showing)
            {
                ClearSpawns();
                ClearPatrolPath();
            }
        }

        public void HandleSpawnAdded(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("Got spawn added");
            MobSpawn spawn = new MobSpawn();
            spawn.ID = (int)props["markerID"];
            spawn.position = (Vector3)props["markerLoc"];
            spawn.orientation = (Quaternion)props["markerOrient"];
            spawn.roamRadius = (int)props["markerRoamRadius"];
            spawn.aggroRadius = (int)props["markerAggroRadius"];
            spawn.CreateMarkerObject(spawnMarkerTemplate);
            spawn.CreateRoamObject(spawnRoamTemplate);
            spawn.CreateAggroObject(spawnAggroTemplate);
            if (!showingMarker)
                spawn.HideMarker();
            if (!showingRoam)
                spawn.HideRoam();
            if (!showingAggro)
                spawn.HideAggro();
            if (mobSpawns.ContainsKey(spawn.ID))
            {
                mobSpawns[spawn.ID].DeleteMarker();
                mobSpawns.Remove(spawn.ID);
            }


            mobSpawns.Add(spawn.ID, spawn);
            AtavismLogger.LogDebugMessage("Added spawn: " + spawn.ID);
        }


        public void HandleSpawnData(Dictionary<string, object> props)
        {
            int spawnID = (int)props["spawnID"];
            spawnInCreation = mobSpawns[spawnID];
            //spawnInCreation.ID = 
            spawnInCreation.numSpawns = (int)props["numSpawns"];
            spawnInCreation.despawnTime = (int)props["despawnTime"];
            spawnInCreation.respawnTime = (int)props["respawnTime"];
            spawnInCreation.respawnTimeMax = (int)props["respawnTimeMax"];
            spawnInCreation.spawnRadius = (int)props["spawnRadius"];
            spawnInCreation.mobTemplateID = (int)props["mobTemplate"];
            spawnInCreation.mobTemplateID2 = (int)props["mobTemplate2"];
            spawnInCreation.mobTemplateID3 = (int)props["mobTemplate3"];
            spawnInCreation.mobTemplateID4 = (int)props["mobTemplate4"];
            spawnInCreation.mobTemplateID5 = (int)props["mobTemplate5"];
            spawnInCreation.mobTemplate = GetMobTemplateByID(spawnInCreation.mobTemplateID);
            spawnInCreation.mobTemplate2 = GetMobTemplateByID(spawnInCreation.mobTemplateID2);
            spawnInCreation.mobTemplate3 = GetMobTemplateByID(spawnInCreation.mobTemplateID3);
            spawnInCreation.mobTemplate4 = GetMobTemplateByID(spawnInCreation.mobTemplateID4);
            spawnInCreation.mobTemplate5 = GetMobTemplateByID(spawnInCreation.mobTemplateID5);
            spawnInCreation.roamRadius = (int)props["roamRadius"];
            spawnInCreation.patrolPath = (int)props["patrolPath"];
            spawnInCreation.spawnActiveStartHour = "" + (int)props["spawnActiveStartHour"];
            spawnInCreation.spawnActiveEndHour = "" + (int)props["spawnActiveEndHour"];
            spawnInCreation.alternateMobTemplateID = (int)props["alternateMobTemplate"];
            spawnInCreation.alternateMobTemplateID2 = (int)props["alternateMobTemplate2"];
            spawnInCreation.alternateMobTemplateID3 = (int)props["alternateMobTemplate3"];
            spawnInCreation.alternateMobTemplateID4 = (int)props["alternateMobTemplate4"];
            spawnInCreation.alternateMobTemplateID5 = (int)props["alternateMobTemplate5"];
            spawnInCreation.alternateMobTemplate = GetMobTemplateByID(spawnInCreation.alternateMobTemplateID);
            spawnInCreation.alternateMobTemplate2 = GetMobTemplateByID(spawnInCreation.alternateMobTemplateID2);
            spawnInCreation.alternateMobTemplate3 = GetMobTemplateByID(spawnInCreation.alternateMobTemplateID3);
            spawnInCreation.alternateMobTemplate4 = GetMobTemplateByID(spawnInCreation.alternateMobTemplateID4);
            spawnInCreation.alternateMobTemplate5 = GetMobTemplateByID(spawnInCreation.alternateMobTemplateID5);
            //spawnInCreation.hasCombat = (bool)props["hasCombat"];
            //spawnInCreation.startsQuests = (List<object>)props["startsQuests"];
            spawnInCreation.merchantTable = (int)props["merchantTable"];
            List<object> questList = (List<object>)props["startsQuests"];
            foreach (object quest in questList)
            {
                spawnInCreation.startsQuests.Add((int)quest);
            }
            //spawnInCreation.endsQuests = (List<object>)props["endsQuests"];
            questList = (List<object>)props["endsQuests"];
            foreach (object quest in questList)
            {
                spawnInCreation.endsQuests.Add((int)quest);
            }
            List<object> dialogueList = (List<object>)props["startsDialogues"];
            foreach (object dialogue in dialogueList)
            {
                spawnInCreation.startsDialogues.Add((int)dialogue);
            }
            spawnInCreation.pickupItemID = (int)props["pickupItem"];
            spawnInCreation.isChest = (bool)props["isChest"];
            List<object> otherActions = (List<object>)props["otherActions"];
            foreach (object action in otherActions)
            {
                spawnInCreation.otherActions.Add((string)action);
            }
            //  spawnInCreation.otherActions
            // mobCreationState = MobCreationState.EditSpawn;
            HideSelectSpawnPanel();
            spawnDeleteButton.ShowVisualElement();
            spawnButton.HideVisualElement();
            spawnUpdateButton.ShowVisualElement();
            editPositionButton.ShowVisualElement();

            ShowSpawnMob();
        }

        #endregion Message Handlers

        public static UIAtavismMobCreator Instance
        {
            get
            {
                return instance;
            }
        }
    }
}