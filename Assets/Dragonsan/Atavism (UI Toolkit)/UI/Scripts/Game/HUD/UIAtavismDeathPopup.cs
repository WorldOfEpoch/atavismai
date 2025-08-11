using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismDeathPopup : MonoBehaviour
    {
        [AtavismSeparator("Window Base")] 
        [SerializeField] protected UIDocument uiDocument;

        [AtavismSeparator("Settings")] [SerializeField]
        private bool showSpiritButton = true;
        private VisualElement uiWindow;
        private Label m_messageText;
        private Button m_buttonSpirit;
        private Button m_buttonRelease;
        bool dead = false;
        string state = "";

        private void OnEnable()
        {
            registerUI();
            Hide();
            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().GameObject != null)
                {
                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>() != null)
                    {
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("deadstate", HandleDeadState);
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("state", HandleState);
                    }
                    else
                    {
                        Debug.LogError("UIAtavismDeathPopup: AtavismNode is null");
                    }
                }
                else
                {
                    Debug.LogError("UIAtavismDeathPopup: GameObject is null");
                }
            }
            else
            {
                Debug.LogError("UIAtavismDeathPopup: PlayerObject is null");
            }
            // The player may have changed scenes, but their stats were not sent back down, so let's take a look
            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().PropertyExists("deadstate"))
                {
                    dead = (bool)ClientAPI.GetPlayerObject().GetProperty("deadstate");
                }
                if (ClientAPI.GetPlayerObject().PropertyExists("state"))
                {
                    state = (string)ClientAPI.GetPlayerObject().GetProperty("state");
                }
            }

            UpdateShowState();
        }

        protected bool registerUI()
        {
            uiWindow =  uiDocument.rootVisualElement.Query<VisualElement>("death-panel");
            
            m_messageText = uiDocument.rootVisualElement.Query<Label>("death-label");
            m_buttonSpirit = uiDocument.rootVisualElement.Query<Button>("button-spirit");
            m_buttonRelease = uiDocument.rootVisualElement.Query<Button>("button-release");

            if (m_buttonSpirit != null)
                m_buttonSpirit.clicked += ReleaseToSpiritClicked;
            if (m_buttonRelease != null)
                m_buttonRelease.clicked += ReleaseClicked;

            return true;
        }

        // Use this for initialization
        private void OnDestroy()
        {
            if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetPlayerObject().GameObject != null && ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>() != null)
            {
                ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("deadstate", HandleDeadState);
                ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("state", HandleState);
            }
        }

        void Show()
        {
            AtavismSettings.Instance.OpenWindow(this);
            uiWindow.ShowVisualElement();
            AtavismUIUtility.BringToFront(this.gameObject);
            if(m_buttonSpirit!=null)
                if(showSpiritButton)
                    m_buttonSpirit.ShowVisualElement();
                else
                    m_buttonSpirit.HideVisualElement();
        }

        public void Hide()
        {
            AtavismSettings.Instance.CloseWindow(this);
            uiWindow.HideVisualElement();
        }

        public void HandleDeadState(object sender, PropertyChangeEventArgs args)
        {
            //  Debug.LogError("UGUIDeathPopup: HandleDeadState");
            dead = (bool)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("deadstate");
            UpdateShowState();
        }

        public void HandleState(object sender, PropertyChangeEventArgs args)
        {
            state = (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("state");
            UpdateShowState();
        }

        public void UpdateShowState()
        {
            //  Debug.LogError("UGUIDeathPopup: UpdateShowState "+dead+" "+state);
            if (dead && state != "spirit")
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void ReleaseClicked()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/release");
        }

        public void ReleaseToSpiritClicked()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/releaseToSpirit");
        }
    }
}