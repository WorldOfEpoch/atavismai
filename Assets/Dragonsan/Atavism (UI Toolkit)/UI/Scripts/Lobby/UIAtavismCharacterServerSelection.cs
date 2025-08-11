using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
namespace Atavism.UI
{

    public class UIAtavismCharacterServerSelection : UIAtavismWindowBase
    {
        //Instance
        // private static UIAtavismCharacterServerSelection instance;
        // public static UIAtavismCharacterServerSelection Instance => instance;


        //Visual Elements
        [SerializeField] public VisualTreeAsset listElementTemplate;
        private Label serverListlabel;
        private Button cancelButton;
        private Button connectButton;
        private ListView serverList;
        List<WorldServerEntry> serverEntries = new List<WorldServerEntry>();
        WorldServerEntry selectedEntry = null;

        Task<bool> connectTask;
        // int serverID = 0;

        // private void Awake()
        // {
        //     instance = this;
        // }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateData();
        }

        private void serverCancel()
        {
            Hide();
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("SHOW_SERVER_LIST", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("SHOW_SERVER_LIST", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
          //  Debug.LogError("OnEvent "+eData.eventType);
            if (eData.eventType == "SHOW_SERVER_LIST")
            {
                Show();
            }
        }
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            var thisVisualElement = uiDocument.rootVisualElement;

            // connec to the UI items
            serverListlabel = thisVisualElement.Q<Label>("server-list-label");
            cancelButton = thisVisualElement.Q<Button>("cancel-button");
            connectButton = thisVisualElement.Q<Button>("connect-button");
            serverList = thisVisualElement.Q<ListView>("server-list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = serverList.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            // serverList = uiWindow.Query<ListView>("list");
           
            serverList.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismServerListEntry newListEntryLogic = new UIAtavismServerListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = listElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            serverList.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismServerListEntry).SetServerDetails(serverEntries[index],this);
            };

            // Attach button routines
            cancelButton.clicked += serverCancel;
            connectButton.clicked += ConnectToSelectedServer;
            connectButton.SetEnabled(false);
            isRegisteredUI = true;

            return true;
        }

        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            isRegisteredUI = false;

            return true;
        }

        public void SelectEntry(WorldServerEntry selectedEntry)
        {
            this.selectedEntry = selectedEntry;
            connectButton.SetEnabled(true);
        }

        public override void Show()
        {
          //  Debug.LogError("Show");
            base.Show();
            UpdateData();
        }

        public void ConnectToSelectedServer()
        {
            Hide();
            connectTask = AtavismClient.Instance.ConnectToGameServer(selectedEntry.Name);
            connectButton.SetEnabled(false);

        }

        public override void UpdateData()
        {
            base.UpdateData();
            serverList.Clear();
            serverEntries.Clear();
            if(AtavismClient.Instance != null && AtavismClient.Instance.WorldServerMap != null)
            foreach (WorldServerEntry entry in AtavismClient.Instance.WorldServerMap.Values)
            {
                serverEntries.Add(entry);
            }
            serverList.itemsSource = serverEntries;
            serverList.Rebuild();
            serverList.selectedIndex = -1;
        }

        void Update()
        {
            if (connectTask != null && connectTask.IsCompleted)
            {
                if (connectTask.Result && CharacterSelectionCreationManager.Instance != null)
                {
                    CharacterSelectionCreationManager.Instance.StartCharacterSelection();
                   Hide();
                }
                connectTask = null;
            }
        }
    }
}