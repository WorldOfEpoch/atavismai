using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismSocialMemberEntry //: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {

        public Label nameText;
        public Label levelText;
        public Label statusText;
        public VisualElement select;
        public VisualElement m_Root;
        AtavismSocialMember socialMember;
        UIAtavismSocialPanel socialPanel;
        bool selected = false;
        // Use this for initialization
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);
            // select = m_Root.Q<VisualElement>("select");
            nameText = m_Root.Q<Label>("name");
            levelText = m_Root.Q<Label>("level");
            statusText = m_Root.Q<Label>("status");

        }

        public void SetSocialMemberDetails(AtavismSocialMember socialMember, UIAtavismSocialPanel socialPanel)
        {
            this.socialMember = socialMember;
            this.socialPanel = socialPanel;
            if (nameText!=null)
                nameText.text = socialMember.name;
            if (levelText!=null)
                levelText.text = socialMember.level.ToString();
#if AT_I2LOC_PRESET
        if(statusText!=null)statusText.text = socialMember.status  ? I2.Loc.LocalizationManager.GetTranslation("Online") : I2.Loc.LocalizationManager.GetTranslation("Online");
#else
            if (statusText!=null)
                statusText.text = socialMember.status ? "Online" : "Offline";
#endif
            if (statusText!=null)
            {
                statusText.RemoveFromClassList("guild-member-online");
                statusText.RemoveFromClassList("guild-member-offline");
                if (socialMember.status)
                    statusText.AddToClassList("guild-member-online");
                else
                    statusText.AddToClassList("guild-member-offline");

            }
        }
        public void SetBlockSocialMemberDetails(AtavismSocialMember socialMember, UIAtavismSocialPanel socialPanel)
        {
            this.socialMember = socialMember;
            this.socialPanel = socialPanel;
            if (nameText!=null)
                nameText.text = socialMember.name;
            if (levelText!=null)
                levelText.text = "";
#if AT_I2LOC_PRESET
        if(statusText!=null)statusText.text = "";
#else
            if (statusText!=null)
                statusText.text = "";
#endif
        }



        public void SocialMemberClicked()
        {
            AtavismSocial.Instance.SelectedMember = socialMember;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseEntered = true;
            // if (!selected)
            //     if (select!=null)
            //         select.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseEntered = false;
            // if (!selected)
            //     if (select!=null)
            //         select.enabled = false;
        }

        public void OnPointerClick(MouseUpEvent evt)
        {
            Debug.LogError("OnPointerClick");
            if (evt.button == 1/*Right*/)
            {
                socialPanel.ShowMemberPopup(this, socialMember);
                // GetComponent<Button>().Select();
            }
            else
            {
                socialPanel.HideMemberPopup();
            }
        }

        public bool MouseEntered
        {
            set
            {
            }
        }
    }
}