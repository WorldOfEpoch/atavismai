using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismRankingListEntry 
    {
        public Label idText;
        public Label playerNameText;
        public Label valueFieldText;

        
        public void SetVisualElement(VisualElement root)
        {
            idText = root.Q<Label>("id");
            playerNameText = root.Q<Label>("name");
            valueFieldText = root.Q<Label>("level");
        }
        public void UpdateInfo(int id, string playerName, string value)
        {
            if (idText != null)
            {
                idText.text = id.ToString();
            }
            if (playerNameText != null)
            {
                if (idText == null)
                playerNameText.text = id+". "+playerName;
                else
                {
                    playerNameText.text = playerName;
                }
            }
            else
            {
                Debug.LogError("Text componet is not assigned");
            }
            if (valueFieldText != null)
            {
                valueFieldText.text = value;
            }
            else
            {
                Debug.LogError("Text componet is not assigned");
            }
        }
    }
}