using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Atavism.UI.UIAudioTemplateScriptableObject;

namespace Atavism.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class UIAtavismAudioManager : MonoBehaviour
    {
        private class CAudioSubscription
        {
            public VisualElement Element;
            public EventCallback<MouseEnterEvent> CallbackMouseEnter;
            public EventCallback<MouseDownEvent> CallbackMouseDown;
            public EventCallback<MouseUpEvent> CallbackMouseUp;

            public CAudioSubscription(VisualElement element, 
                EventCallback<MouseEnterEvent> callbackMouseEnter, EventCallback<MouseDownEvent> callbackMouseDown, EventCallback<MouseUpEvent> callbackMouseUp)
            {
                this.Element = element;
                this.CallbackMouseEnter = callbackMouseEnter;
                this.CallbackMouseDown = callbackMouseDown;
                this.CallbackMouseUp = callbackMouseUp;
            }
        }

        private static UIAtavismAudioManager instance;
        public static UIAtavismAudioManager Instance => instance;

        [AtavismSeparator("Runtime")]
        [SerializeField] private bool isInitialized;

        [AtavismSeparator("Initialize")]
        [SerializeField] private UIAudioTemplateScriptableObject sfxTemplate;

        private AudioSource audioSource;
        private Dictionary<string, CAudioEvent> listofAudioEvents;
        private Dictionary<int, CAudioSubscription> subscriptions;

        #region Initiate
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();

            listofAudioEvents = new Dictionary<string, CAudioEvent>();
            subscriptions = new Dictionary<int, CAudioSubscription>();

            if (sfxTemplate == null)
            {
                Debug.LogError("Missing audio template reference!");
                return;
            }

            for (int n = 0; n < sfxTemplate.AudioEvents.Length; n++)
                listofAudioEvents[sfxTemplate.AudioEvents[n].StyleClass] = sfxTemplate.AudioEvents[n];

            isInitialized = true;
        }
        #endregion
        #region Public Methods
        public void RegisterSFX(UIDocument document)
        {
            if (document != null)
                document.rootVisualElement.Query().Children<VisualElement>().ForEach(e => RegisterSFX(e));
        }

        public void RegisterSFX(VisualElement element)
        {
            if (!isInitialized)
                return;

            if (element == null)
            {
                Debug.LogError("RegisterSFX failed! Element is null.");
                return;
            }

            UnregisterSFX(element);

            IEnumerable<string> styleSheets = element.GetClasses();
            foreach (string style in styleSheets)
            {
                if (listofAudioEvents.ContainsKey(style))
                {
                    EventCallback<MouseEnterEvent> callbackMouseEnter = null;
                    EventCallback<MouseDownEvent> callbackMouseDown = null;
                    EventCallback<MouseUpEvent> callbackMouseUp = null;

                    CAudioEvent audioEvent = listofAudioEvents[style];
                    if (audioEvent.MouseEnterClip != null)
                    {
                        callbackMouseEnter = (MouseEnterEvent evt) => { audioSource.PlayOneShot(audioEvent.MouseEnterClip); };
                        element.RegisterCallback(callbackMouseEnter);
                    }
                    if (audioEvent.MouseDownClip != null)
                    {
                        callbackMouseDown = (MouseDownEvent evt) => { audioSource.PlayOneShot(audioEvent.MouseDownClip); };
                        element.RegisterCallback(callbackMouseDown, TrickleDown.TrickleDown);
                    }
                    if (audioEvent.MouseUpClip != null)
                    {
                        callbackMouseUp = (MouseUpEvent evt) => { audioSource.PlayOneShot(audioEvent.MouseUpClip); };
                        element.RegisterCallback(callbackMouseUp);
                    }

                    subscriptions[element.GetHashCode()] = new CAudioSubscription(element, callbackMouseEnter, callbackMouseDown, callbackMouseUp);

                    return;
                }
            }

            if (sfxTemplate.ButtonMouseEnter != null)
                element.RegisterCallback<MouseEnterEvent>(onButtonMouseEnter);
            if (sfxTemplate.ButtonMouseDown != null)
                element.RegisterCallback<MouseDownEvent>(onButtonMouseDown, TrickleDown.TrickleDown);
            if (sfxTemplate.ButtonMouseUp != null)
                element.RegisterCallback<MouseUpEvent>(onButtonMouseUp);
        }

        public void UnregisterSFX(UIDocument document)
        {
            try {
                if (document != null)
                    document.rootVisualElement.Query().Children<VisualElement>().ForEach(e => UnregisterSFX(e));
            }
            catch { }
        }

        public void UnregisterSFX(VisualElement element)
        {
            if (!isInitialized)
                return;

            if (element == null)
            {
                Debug.LogError("UnregisterSFX failed! Element is null.");
                return;
            }

            if (subscriptions.ContainsKey(element.GetHashCode()))
            {
                element.UnregisterCallback<MouseEnterEvent>(subscriptions[element.GetHashCode()].CallbackMouseEnter);
                element.UnregisterCallback<MouseDownEvent>(subscriptions[element.GetHashCode()].CallbackMouseDown);
                element.UnregisterCallback<MouseUpEvent>(subscriptions[element.GetHashCode()].CallbackMouseUp);

                subscriptions.Remove(element.GetHashCode());
            }
            else
            {
                element.UnregisterCallback<MouseEnterEvent>(onButtonMouseEnter);
                element.UnregisterCallback<MouseDownEvent>(onButtonMouseDown);
                element.UnregisterCallback<MouseUpEvent>(onButtonMouseUp);
            }
        }

        public void RegisterSFX(Toggle toggle)
        {
            if (!isInitialized)
                return;

            if (toggle == null)
            {
                Debug.LogError("RegisterSFX failed! Toggle is null.");
                return;
            }

            if (sfxTemplate.ToggleChanged != null)
                toggle.RegisterValueChangedCallback(onToggleValueChanged);
        }

        public void UnregisterSFX(Toggle toggle)
        {
            if (!isInitialized)
                return;

            if (toggle == null)
            {
                Debug.LogError("UnregisterSFX failed! Toggle is null.");
                return;
            }

            if (sfxTemplate.ToggleChanged != null)
                toggle.UnregisterValueChangedCallback(onToggleValueChanged);
        }
        #endregion
        #region Generic - Event Handlers
        private void onButtonMouseEnter(MouseEnterEvent evt)
        {
            audioSource.PlayOneShot(sfxTemplate.ButtonMouseEnter);
        }

        private void onButtonMouseDown(MouseDownEvent evt)
        {
            audioSource.PlayOneShot(sfxTemplate.ButtonMouseDown);
        }

        private void onButtonMouseUp(MouseUpEvent evt)
        {
            audioSource.PlayOneShot(sfxTemplate.ButtonMouseUp);
        }

        private void onToggleValueChanged(ChangeEvent<bool> evt)
        {
            audioSource.PlayOneShot(sfxTemplate.ToggleChanged);
        }
        #endregion
    }
}