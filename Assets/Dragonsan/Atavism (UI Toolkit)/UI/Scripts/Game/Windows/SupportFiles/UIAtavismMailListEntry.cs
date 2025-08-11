using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UAtavismMailListEntry //: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public Label m_senderText;
        public Label m_subjectText;
        public VisualElement m_itemIcon;
        public VisualElement m_selectedImage;
        MailEntry entry;
        bool selected = false;
        private VisualElement m_Root;
        
        // Use this for initialization
       public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseUpEvent>(MailEntryClicked);
            m_senderText = visualElement.Q<Label>("sender");
            m_subjectText = visualElement.Q<Label>("subject");
            m_itemIcon = visualElement.Q<VisualElement>("icon");
            m_selectedImage = visualElement.Q<VisualElement>("selected");
        }

        public void OnPointerEnter(MouseEnterEvent evt)
        {
            MouseEntered = true;
            if (!selected)
            {
                if (m_selectedImage != null)
                    m_selectedImage.visible = true;
            }
        }

        public void OnPointerExit(MouseLeaveEvent evt)
        {
            MouseEntered = false;
            if (!selected)
            {
                if (m_selectedImage != null)
                    m_selectedImage.visible = false;
            }
        }

        public void MailEntryClicked(MouseUpEvent evt)
        {
            Mailing.Instance.SelectedMail = entry;
        }

        public void SetMailEntryDetails(MailEntry entry)
        {
            this.entry = entry;
            if (m_senderText != null)
                this.m_senderText.text = entry.senderName;
            if (entry.subject == "")
            {
#if AT_I2LOC_PRESET
             if (m_subjectText != null) this.m_subjectText.text = I2.Loc.LocalizationManager.GetTranslation("No topic");
#else
                if (m_subjectText != null)
                    this.m_subjectText.text = "No topic";
#endif
            }
            else
            {
                if (m_subjectText != null)
                    this.m_subjectText.text = entry.subject;
            }
            //this.itemIcon.sprite = entry.;
            if (entry == Mailing.Instance.SelectedMail)
            {
                selected = true;
                if (m_selectedImage != null)
                    m_selectedImage.visible = true;
                /*  Color col = GetComponent<Image>().color;
                   col.a = 1f;
                   GetComponent<Image>().color = col;*/
            }
            else
            {
                selected = false;
                if (m_selectedImage != null)
                    m_selectedImage.visible = false;
                /*  Color col = GetComponent<Image>().color;
                   col.a = 0f;
                   GetComponent<Image>().color = col;*/
            }
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        public bool MouseEntered
        {
            set
            {
            }
        }
    }
}