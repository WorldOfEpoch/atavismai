using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismGuildMemberEntry 
    {

        public Label nameText;
        public Label rankText;
        public Label levelText;
        public Label statusText;
        // public Color onlineColor = Color.green;
        // public Color offlineColor = Color.red;
        // public Color awayColor = Color.gray;
        AtavismGuildMember guildMember;
        UIAtavismGuildPanel guildPanel;
        private VisualElement m_Root;

        public void SetVisualElement(VisualElement root)
        {
            m_Root = root;
            m_Root.pickingMode = PickingMode.Position; 
            Clickable thisClickable = new Clickable((v)=>{});
            // thisClickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            // m_Root.AddManipulator(thisClickable);     
            // Register UI Toolkit events
            // m_Root.RegisterCallback<ClickEvent>(GuildMemberClicked);
            m_Root.RegisterCallback<MouseUpEvent>(GuildMemberClicked);
            rankText = root.Q<Label>("rank");
            nameText = root.Q<Label>("name");
            levelText = root.Q<Label>("level");
            statusText= root.Q<Label>("status");
        }
        
        
        public void SetGuildMemberDetails(AtavismGuildMember guildMember, UIAtavismGuildPanel guildPanel)
        {
            this.guildMember = guildMember;
            this.guildPanel = guildPanel;
            if (nameText != null)
                nameText.text = guildMember.name;
            string rankName = "";//AtavismGuild.Instance.Ranks.Count > 0 ? AtavismGuild.Instance.Ranks[guildMember.rank].rankName : "";
            foreach (var r in AtavismGuild.Instance.Ranks)
            {
                if (r.rankLevel == guildMember.rank)
                {
                    rankName = r.rankName;
                }
            }

            if (rankText != null)
                rankText.text = rankName;
            if (levelText != null)
                levelText.text = guildMember.level.ToString();
#if AT_I2LOC_PRESET
       if (statusText != null)  statusText.text = guildMember.status > 0 ? (guildMember.status > 1 ? I2.Loc.LocalizationManager.GetTranslation("Away") : I2.Loc.LocalizationManager.GetTranslation("Online")) : I2.Loc.LocalizationManager.GetTranslation("Offline");
#else
            if (statusText != null)
                statusText.text = guildMember.status > 0 ? (guildMember.status > 1 ? "Away" : "Online") : "Offline";
#endif


            if (statusText != null)
            {
                statusText.RemoveFromClassList("guild-member-away");
                statusText.RemoveFromClassList("guild-member-online");
                statusText.RemoveFromClassList("guild-member-offline");
                    
            }
            
            if (guildMember.status > 0)
            {
                if (guildMember.status > 1)
                {
                    if (statusText != null)
                        statusText.AddToClassList("guild-member-away");
                }
                else
                {
                    if (statusText != null)
                        statusText.AddToClassList("guild-member-online");

                }
            }
            else
            {
                if (statusText != null)
                    statusText.AddToClassList("guild-member-offline");
            }
        }

        public void GuildMemberClicked(MouseUpEvent mouseUpEvent)
        {
            AtavismGuild.Instance.SelectedMember = guildMember;
            if (mouseUpEvent.button == (int)MouseButton.RightMouse)
            {
                OnPointerClick();
            }
        }

        public void OnPointerClick()
        {
                guildPanel.ShowMemberPopup(this, guildMember);
                // GetComponent<Button>().Select();
        }

      
    }
}