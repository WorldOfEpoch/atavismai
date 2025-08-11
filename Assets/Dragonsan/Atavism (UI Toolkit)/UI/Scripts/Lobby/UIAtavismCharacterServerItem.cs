using UnityEngine;
using UnityEngine.UIElements;
namespace Atavism.UI
{

    public class UIAtavismCharacterServerItem : MonoBehaviour
    {
        public string serverName;
        public string serverType;
        public string serverPopulation;

        private Button serverbutton;
        private Label servernameLable;
        private Label servertypeLabel;
        private Label serverpopulationLabel;

        WorldServerEntry entry;
         // UIAtavismServerList serverList;

        void OnEnable()
        {
            var thisVisualElement = GetComponent<UIDocument>().rootVisualElement;

            // connec to the UI items
            servernameLable = thisVisualElement.Q<Label>("server-name");
            servertypeLabel = thisVisualElement.Q<Label>("server-type");
            serverpopulationLabel = thisVisualElement.Q<Label>("server-population");
            serverbutton = thisVisualElement.Q<Button>("server-item");

        }

        public void SetServerDetails(WorldServerEntry entry, UIAtavismCharacterServerSelection serverList)
        {
            this.entry = entry;

            if (entry.Name == AtavismClient.Instance.WorldId)
            {
                servernameLable.text = entry.Name + "(current)";

            }
            else
            {
                string status = (string)entry["status"];
                if (status != "Online")
                {
                        this.servernameLable.text = entry.Name + " (" + status + ")";
                }
                else
                {
                        this.servernameLable.text = entry.Name;
                }
            }
        }

        public void ServerSelected()
        {
            // serverList.SelectEntry(entry);
        }
    }
}