using UnityEngine;
using UnityEngine.UIElements;


namespace Atavism.UI
{

    public class UIAtavismGuildAddPlayerPanel : UIAtavismWindowBase
    {

        public UITextField m_playerName;
        public Button m_createButton;

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            m_playerName = uiWindow.Query<UITextField>("guild-name-input");
            m_createButton = uiWindow.Query<Button>("create-button");
            m_createButton.clicked += AddPlayerClicked;


          //  Debug.LogError("UIAtavismGuildCreatePanel registerUI End");
            return true;
        }

        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("GUILD_ADD_PLAYER", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("GUILD_ADD_PLAYER", this);
        }


        public void OnEvent(AtavismEventData eData)
        {
            // Debug.LogError("OnEvent " + eData.eventType);
            if (eData.eventType == "GUILD_ADD_PLAYER")
            {
               
                Show();
                MoveToCenter();
            }
        }

        public void AddPlayerClicked()
        {

            if (m_playerName != null && m_playerName.value != "")
            {
                AtavismGuild.Instance.SendGuildCommand("invite", null, m_playerName.value);
            }

            Hide();
        }
    }
}