using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atavism
{

    public class UGUIServerList : AtList<UGUIServerListEntry>
    {

        public Button connectButton;
        public RectTransform panel;
        Task<bool> connectTask;
        List<WorldServerEntry> serverEntries = new List<WorldServerEntry>();
        WorldServerEntry selectedEntry = null;

        void Start()
        {
            AtavismEventSystem.RegisterEvent("SHOW_SERVER_LIST", this);
            connectButton.interactable = false;
            //AtavismClient.Instance.GetGameServerList();

            // Delete the old list
            ClearAllCells();
            
            Refresh();
            if(panel)
                panel.gameObject.SetActive(false); 
            
        }
        
        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("SHOW_SERVER_LIST", this);
            
        }

        public void Show()
        {
            if(panel)
                panel.gameObject.SetActive(true);
            // Delete the old list
            ClearAllCells();

            Refresh();
        }

        public void Hide()
        {
            if(panel)
                panel.gameObject.SetActive(false);

        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "SHOW_SERVER_LIST")
            {
               Show();
            }
        }

        public void SelectEntry(WorldServerEntry selectedEntry)
        {
            this.selectedEntry = selectedEntry;
            connectButton.interactable = true;
        }

        public void ConnectToSelectedServer()
        {
            Hide();
            connectTask = AtavismClient.Instance.ConnectToGameServer(selectedEntry.Name);
            connectButton.interactable = false;
        }

        void Update()
        {
            if (connectTask != null && connectTask.IsCompleted)
            {
                if (connectTask.Result && CharacterSelectionCreationManager.Instance != null)
                {
                    CharacterSelectionCreationManager.Instance.StartCharacterSelection();
                    gameObject.SetActive(false);
                }
                connectTask = null;
            }
        }

        #region implemented abstract members of AtList

        public override int NumberOfCells()
        {
            serverEntries.Clear();
            foreach (WorldServerEntry entry in AtavismClient.Instance.WorldServerMap.Values)
            {
                serverEntries.Add(entry);
            }
            return serverEntries.Count;
        }

        public override void UpdateCell(int index, UGUIServerListEntry cell)
        {
            cell.SetServerDetails(serverEntries[index], this);
        }

        #endregion
    }
}
