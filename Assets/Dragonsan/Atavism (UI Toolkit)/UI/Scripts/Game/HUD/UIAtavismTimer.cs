using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismTimer : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        UIProgressBar _progress;
        Label _label;
        float _stopDisplay;
        bool showing = false;
        [SerializeField] float countdown = 0f;
        [SerializeField] float total = 0f;
        // Use this for initialization
        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            RegisterUI();
            Hide();
            AtavismEventSystem.RegisterEvent("TimerStart", OnEvent);
            AtavismEventSystem.RegisterEvent("TimerStop", OnEvent);
            NetworkAPI.RegisterExtensionMessageHandler("arena_ready", HandleArenaReady);
            NetworkAPI.RegisterExtensionMessageHandler("arena_setup", HandleArenaSetup);
            NetworkAPI.RegisterExtensionMessageHandler("arena_countdown", HandleArenaCouldown);
            NetworkAPI.RegisterExtensionMessageHandler("arena_started", HandleArenaStart);
            NetworkAPI.RegisterExtensionMessageHandler("arena_end", HandleArenaEnd);
            NetworkAPI.RegisterExtensionMessageHandler("arena_abilities", HandleArenaAbilities);

        }

        void RegisterUI()
        {
            uiDocument.enabled = true;
            _label = uiDocument.rootVisualElement.Q<Label>("label");
            _progress = uiDocument.rootVisualElement.Q<UIProgressBar>("progressBar");
        }
        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("TimerStart", OnEvent);
            AtavismEventSystem.UnregisterEvent("TimerStop", OnEvent);
            NetworkAPI.RemoveExtensionMessageHandler("arena_ready", HandleArenaReady);
            NetworkAPI.RemoveExtensionMessageHandler("arena_setup", HandleArenaSetup);
            NetworkAPI.RemoveExtensionMessageHandler("arena_countdown", HandleArenaCouldown);
            NetworkAPI.RemoveExtensionMessageHandler("arena_started", HandleArenaStart);
            NetworkAPI.RemoveExtensionMessageHandler("arena_end", HandleArenaEnd);
            NetworkAPI.RemoveExtensionMessageHandler("arena_abilities", HandleArenaAbilities);
        }

        private void HandleArenaAbilities(Dictionary<string, object> props)
        {
          //  throw new NotImplementedException();
        }

        private void HandleArenaEnd(Dictionary<string, object> props)
        {
            Hide();
        }

        private void HandleArenaStart(Dictionary<string, object> props)
        {
            int len = (int)props["timeLeft"];
            total = len / 1000f;
            countdown = Time.time + total;
            Show();
        }

        private void HandleArenaCouldown(Dictionary<string, object> props)
        {
            int len = (int)props["setupLength"];
            total = len / 1000f;
            countdown = Time.time + total;
            Show();
        }

        private void HandleArenaSetup(Dictionary<string, object> props)
        {
        }

        private void HandleArenaReady(Dictionary<string, object> props)
        {
            Show();
        }

        // Update is called once per frame
        void Update()
        {
            if (showing && Time.time > countdown)
            {
                Hide();
            }

            // if (_label != null)
                if (Time.time < countdown)
                {
                    float sec = countdown - Time.time;
                    int min = 0;
                    if (sec > 60)
                    {
                        min = (int)sec / 60;
                    }

                    if (_label != null)
                        _label.text = (min > 0 ? min + ":" : "") + ((((int)sec - min * 60) < 10 && min > 0)
                            ? "0" + ((int)sec - min * 60)
                            : "" + ((int)sec - min * 60));
                    if (_progress != null)
                    {
                        _progress.highValue = total;
                        _progress.value = sec;
                        Show();
                    }
                }
                else
                {
                    if (_label != null) _label.text = "";
                    Hide();
                }
        }

        void Hide()
        {

            // Debug.LogError("Hide " );
            uiDocument.rootVisualElement.HideVisualElement();
            showing = false;
        }
        void Show()
        { 
            // Debug.LogError("Show " );
            uiDocument.rootVisualElement.ShowVisualElement();
            showing = true;
        }


        public void OnEvent(AtavismEventData eData)
        {
               // Debug.LogError("Timer ");
            if (eData.eventType == "TimerStart")
            {
                // Debug.LogError("TimerStart " + eData.eventArgs[0]);
                total = float.Parse(eData.eventArgs[0]);
                countdown = Time.time + total;
                Show();
            }
            else if (eData.eventType == "TimerStop")
            {
                countdown = 0f;
                   // Debug.LogError("TimerStop " );
                Hide();
            }
        }
    }
}