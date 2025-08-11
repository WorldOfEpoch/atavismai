using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismLogoutPanel : MonoBehaviour
    {

        static UIAtavismLogoutPanel instance;
        public UIDocument uiDocument;
        private VisualElement rootElement;
        public Label logoutTimerText;
        public Label confirmationText;
        // private Button confirmButton;
        private Button cancelButton;
        object confirmationObject;
        ConfirmationResponse confirmationResponse;
        float logoutTime = -1;
        bool showing = false;
        // float startTime;
        float endTime = -1;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            uiDocument = GetComponent<UIDocument>();
            InitializeUI();
            Hide();
            NetworkAPI.RegisterExtensionMessageHandler("logout_timer", HandleLogoutTimer);
        }
        void InitializeUI()
        {
            rootElement = uiDocument.rootVisualElement.Q<VisualElement>("logout-panel");
            cancelButton = rootElement.Q<Button>("button-cancel");
            confirmationText = rootElement.Q<Label>("message");
            logoutTimerText = rootElement.Q<Label>("timer");
            cancelButton.clicked += () => CancelLogout();
        }
        void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("logout_timer", HandleLogoutTimer);
        }

        // Update is called once per frame
        void Update()
        {
            if (showing)
            {
                int timeUntilLogout = (int)(logoutTime - Time.realtimeSinceStartup);
                if (logoutTimerText != null)
                    logoutTimerText.text = timeUntilLogout.ToString() + "s";
            }
            if (endTime != -1 && endTime > Time.time)
            {
                //  float total = endTime - startTime;
                float currentTime = endTime - Time.time;
                if (logoutTimerText != null)
                    logoutTimerText.text = (int)(currentTime) + "s";

            }
            else
            {
                if (showing)
                    Hide();
            }

            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().PropertyExists("combatstate"))
                    if ((bool)ClientAPI.GetPlayerObject().GetProperty("combatstate"))
                    {
                        if (showing)
                            Hide();
                    }
            }
        }

        public void Show()
        {
            AtavismSettings.Instance.OpenWindow(this);   
            uiDocument.sortingOrder = 70;
            rootElement.ShowVisualElement();
            showing = true;
            transform.position = new Vector3((Screen.width / 2), (Screen.height / 2), 0);
        }

        public void Hide()
        {
            rootElement.HideVisualElement();
            AtavismSettings.Instance.CloseWindow(this);   
            showing = false;
        }

        public void CancelLogout()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.CANCEL_LOGOUT_REQUEST", props);
            if (confirmationResponse != null)
                confirmationResponse(confirmationObject, false);
            Hide();
        }

        public void HandleLogoutTimer(Dictionary<string, object> props)
        {
            int timer = (int)props["timer"];
            logoutTime = Time.realtimeSinceStartup + timer;
            Show();
        }
        public static UIAtavismLogoutPanel Instance
        {
            get
            {
                return instance;
            }
        }
        public bool isShow()
        {
            return (showing);
        }
        public void ShowConfirmationBox(string message, object confirmObject, ConfirmationResponse responseMethod)
        {
            Show();
            //  startTime = Time.time;
            endTime = Time.time + 10f;
            if (confirmationText != null)
                confirmationText.text = message;
            this.confirmationObject = confirmObject;
            this.confirmationResponse = responseMethod;
        }
    }
}