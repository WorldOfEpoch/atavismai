using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismArenaScoreListEntity
    {
        public Label PlyScore;
        public Label PlyName;
        public Label PlyKills;
        public Label PlyDeaths;
        public Label PlyDamageTaken;
        public Label PlyDamageDealt;
        private VisualElement uiRoot;

        public void SetVisualElement(VisualElement visualElement)
        {
            uiRoot = visualElement;

            PlyName = visualElement.Q<Label>("name");
            PlyScore = visualElement.Q<Label>("score");
            PlyKills = visualElement.Q<Label>("kills");
            PlyDeaths = visualElement.Q<Label>("deaths");
            PlyDamageTaken = visualElement.Q<Label>("damage-taken");
            PlyDamageDealt = visualElement.Q<Label>("damage-dealth");
        }
    }
}