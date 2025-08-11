using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismWorldTimeWAPIDisplay : MonoBehaviour
    {

        [SerializeField] UIDocument uiDocument;
        public Label timeText;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        // Use this for initialization
        void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            
            timeText = uiDocument.rootVisualElement.Q<Label>("timeText");

            AtavismEventSystem.RegisterEvent("WORLD_TIME_UPDATE_WAPI", this);
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("WORLD_TIME_UPDATE_WAPI", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "WORLD_TIME_UPDATE_WAPI")
            {
                //  Debug.LogError("Got WORLD_TIME_UPDATE_WAPI");
                if (timeText != null)
                    timeText.text = eData.eventArgs[4] + "-" + eData.eventArgs[3] + "-" + eData.eventArgs[2] + " " + eData.eventArgs[1] + ":" + eData.eventArgs[0] + " " + eData.eventArgs[5];
            }
        }

    }
}