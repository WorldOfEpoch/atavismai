using UnityEngine;
using UnityEngine.UIElements;


namespace Atavism.UI
{

    public class UIAtavismGuildCreatePanel : UIAtavismWindowBase
    {

        public UITextField m_guildName;
        public Button m_createButton;

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            m_guildName = uiWindow.Query<UITextField>("guild-name-input");
            m_createButton = uiWindow.Query<Button>("create-button");
            m_createButton.clicked += CreateGuildClicked;


          //  Debug.LogError("UIAtavismGuildCreatePanel registerUI End");
            return true;
        }

        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("GUILD_CREATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("GUILD_CREATE", this);
        }


        public void OnEvent(AtavismEventData eData)
        {
            // Debug.LogError("OnEvent " + eData.eventType);
            if (eData.eventType == "GUILD_CREATE")
            {
               
                Show();
                MoveToCenter();
            }
        }

        public void CreateGuildClicked()
        {
            if (m_guildName != null && m_guildName.value != "")
            {
                AtavismGuild.Instance.CreateGuild(m_guildName.value);
                m_guildName.value = "";
            }

            Hide();
        }
    }
}