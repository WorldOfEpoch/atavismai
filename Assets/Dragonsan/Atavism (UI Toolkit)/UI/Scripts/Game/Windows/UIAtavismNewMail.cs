using System;
using System.Collections;
using System.Collections.Generic;
#if AT_MASTERAUDIO_PRESET
using DarkTonic.MasterAudio;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Atavism.UI
{
[RequireComponent(typeof(UIDocument))]
    public class UIAtavismNewMail : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        private VisualElement image;
        public string audioName = "";

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            uiDocument.enabled = true;
            image = uiDocument.rootVisualElement.Q<VisualElement>("MailButton");
            AtavismEventSystem.RegisterEvent("NO_NEW_MAIL", this);
            AtavismEventSystem.RegisterEvent("NEW_MAIL", this);
            AtavismEventSystem.RegisterEvent("MAILBOX_OPENED", this);
            image.RegisterCallback<ClickEvent>(Click);
            Hide();
        }

        void Awake()
        {
            Hide();
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("NO_NEW_MAIL", this);
            AtavismEventSystem.UnregisterEvent("NEW_MAIL", this);
            AtavismEventSystem.UnregisterEvent("MAILBOX_OPENED", this);
        }

        void Show()
        {
            if (image!=null)
            {
                image.ShowVisualElement();
            }
#if AT_MASTERAUDIO_PRESET
            MasterAudio.PlaySoundAndForget(audioName);
#endif
        }

        void Hide()
        {
            if (image!=null)
            {
                image.HideVisualElement();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
          //  Debug.LogError(eData.eventType);
            if (eData.eventType == "NEW_MAIL")
            {
                Show();
            }
            else if (eData.eventType == "MAILBOX_OPENED")
            {
                Hide();
            }
            else if (eData.eventType == "NO_NEW_MAIL")
            {
                Hide();
            }
        }

        public void Click(ClickEvent evt)
        {
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MAILBOX_OPEN", args);
        }
    }
}