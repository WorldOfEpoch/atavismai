using UnityEngine;
using UnityEngine.UIElements;


namespace Atavism.UI
{

    public class UIAtavismGuildEditMotdPanel : UIAtavismWindowBase
    {

        public UITextField m_motd;
        public Button m_saveButton;

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            m_motd = uiWindow.Query<UITextField>("guild-motd-field");
            m_saveButton = uiWindow.Query<Button>("save-button");
            m_saveButton.clicked += SaveClicked;


         //   Debug.LogError("UIAtavismGuildCreatePanel registerUI End");
            return true;
        }

        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("GUILD_EDIT_MOTD", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("GUILD_EDIT_MOTD", this);
        }


        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "GUILD_EDIT_MOTD")
            {
               
                Show();
                MoveToCenter();
                if (m_motd != null)
                    m_motd.value = AtavismGuild.Instance.Motd;
            }
        }

        public void SaveClicked()
        {

            if (m_motd != null && m_motd.value != "")
            {
                AtavismGuild.Instance.SendGuildCommand("setmotd", null, m_motd.value);
            }

            Hide();
        }
    }
}