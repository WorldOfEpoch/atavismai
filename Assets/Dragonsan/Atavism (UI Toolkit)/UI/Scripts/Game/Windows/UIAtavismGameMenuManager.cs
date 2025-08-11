using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Atavism.UI;

namespace Atavism.UI
{
    public class UIAtavismGameMenuManager : MonoBehaviour
    {
        public UIDocument uiDocument;
        private VisualElement rootElement;
        public  UIAtavismSettings settingsElement;

        private Button resumeButton;
        private Button logoutButton;
        private Button settingsButton;
        private Button quitButton;
        bool showing = false;
        private bool _keyChange = false;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        void Start()
        {
            if(uiDocument==null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            rootElement = uiDocument.rootVisualElement;
            InitializeUI();
            Hide();
            NetworkAPI.RegisterExtensionMessageHandler("LOGOUT_STARTED", ClaimIDMessage);
            AtavismEventSystem.RegisterEvent("CHANGE_KEY", this);
        }
        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CHANGE_KEY", this);
        }
        void InitializeUI()
        {
            // settingsElement = rootElement.Q<VisualElement>("Settings");
            resumeButton = rootElement.Q<Button>("resume-button");
            resumeButton.clicked += Hide;
            logoutButton = rootElement.Q<Button>("logout-button");
            logoutButton.clicked += Logout;
            
            settingsButton = rootElement.Q<Button>("settings-button");
            settingsButton.clicked += ShowSettings;
            quitButton = rootElement.Q<Button>("quit-button");
            quitButton.clicked += Quit;

            // ... Register event listeners for the UI elements ...
        }

        private void ShowSettings()
        {
            Hide();
            settingsElement.Show();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CHANGE_KEY")
            {
                if (eData.eventArgs[0].Equals("T"))
                {
                    _keyChange = true;
                }
                else
                {
                    _keyChange = false;
                }
            }
        }
        public void ClaimIDMessage(Dictionary<string, object> props)
        {
            int status = (int)props["status"];
            Debug.Log("Got Logout: " + status);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && ClientAPI.GetTargetObject() != null && !_keyChange && !AtavismSettings.Instance.IsAnyOpenedWindowEscKey)
            {
                ClientAPI.ClearTarget();
            } else
            if (Input.GetKeyDown(AtavismSettings.Instance.openGameSettingsKey) && ClientAPI.GetTargetObject() == null && !_keyChange && !AtavismSettings.Instance.IsAnyOpenedWindowEscKey)
            {
                if (settingsElement != null && settingsElement.isShowing == true)
                {
                    settingsElement.Hide();
                }
                else
                {
                    if (UIAtavismLogoutPanel.Instance.isShow())
                    {
                        UIAtavismLogoutPanel.Instance.Hide();
                        Dictionary<string, object> props = new Dictionary<string, object>();
                        NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.CANCEL_LOGOUT_REQUEST", props);
                    }
                    else
                    {
                        if (showing)
                        {
                            Hide();
                        }
                        else
                        {
                            Show();
                        }
                    }
                }
            }
            
        }

        public void Show()
        {
            AtavismSettings.Instance.OpenWindow(this);   
            rootElement.ShowVisualElement();
            rootElement.pickingMode = PickingMode.Position;
            showing = true;
            if (Camera.main != null)
                Camera.main.gameObject.SendMessage("NoMove", true);
        }

        public void Hide()
        {
            AtavismSettings.Instance.CloseWindow(this);
            rootElement.HideVisualElement();
            rootElement.pickingMode = PickingMode.Ignore;
            showing = false;
            if (Camera.main != null)
                Camera.main.gameObject.SendMessage("NoMove", false);
        }

        public void Logout()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.LOGOUT_REQUEST", props);
#if AT_I2LOC_PRESET
        UIAtavismLogoutPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Logging out will occur in"), null, CancelLogout);
#else
            UIAtavismLogoutPanel.Instance.ShowConfirmationBox("Logging out will occur in", null, CancelLogout);
#endif
            Hide();
        }
        public void CancelLogout(object obj, bool accepted)
        {
            if (!accepted)
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.CANCEL_LOGOUT_REQUEST", props);
                Hide();
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}