using UnityEngine;
using System.Collections.Generic;
using Atavism.UI.Game;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMailRead : UIAtavismWindowBase
    {

        public Label nameText;
        public Label subjectText;
        public Label messageText;
        public List<UIAtavismMailAttachment> itemSlots = new List<UIAtavismMailAttachment>();
        public UIAtavismCurrencyDisplay currencyDisplays;
        public Button takeCurrencyButton;
        MailEntry mailBeingRead;
        [SerializeField] bool hideOnShowMailList = true;
        [SerializeField] bool hideMailListOnStartRead = true;

        private Button replayButton;
        private Button deleteButton;
        private Button takeButton;
        // Use this for initialization

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            nameText = uiWindow.Q<Label>("sender");
            subjectText = uiWindow.Q<Label>("subject");
            messageText = uiWindow.Q<Label>("message");
            
            VisualElement currency = uiWindow.Q<VisualElement>("currency");
            currencyDisplays = new UIAtavismCurrencyDisplay();
            currencyDisplays.SetVisualElement(currency);
            currencyDisplays.ReverseOrder = true;

            for (int i = 1; i <= 10; i++)
            {
                VisualElement item = uiWindow.Q<VisualElement>("item-"+i);
                if (item != null)
                {
                    UIAtavismMailAttachment att = new UIAtavismMailAttachment();
                    att.SetVisualElement(item);
                    itemSlots.Add(att);
                }
            }
           
            
            
            replayButton = uiWindow.Q<Button>("replay-button");
            replayButton.clicked += Reply;
            deleteButton = uiWindow.Q<Button>("delete-button");
            deleteButton.clicked += DeleteMail;
            takeCurrencyButton = uiWindow.Q<Button>("take-button");
            takeCurrencyButton.clicked += TakeCurrency;
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
     
            // if (titleBar != null)
                // titleBar.SetOnPanelClose(Hide);
            AtavismEventSystem.RegisterEvent("MAIL_UPDATE", OnEvent);
            AtavismEventSystem.RegisterEvent("CLOSE_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.RegisterEvent("CLOSE_READ_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.RegisterEvent("MAIL_SELECTED", OnEvent);
            AtavismEventSystem.RegisterEvent("START_READ_MAIL_WINDOW", OnEvent);
            
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("MAIL_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("CLOSE_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.UnregisterEvent("CLOSE_READ_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.UnregisterEvent("MAIL_SELECTED", OnEvent);
            AtavismEventSystem.UnregisterEvent("START_READ_MAIL_WINDOW", OnEvent);
        }

        public void OnEvent(AtavismEventData eData)
        {
            // if (!enabled)
            //     return;

            if (eData.eventType == "MAIL_UPDATE")
            {
                UpdateAttachmentsDisplay();
            }
            else if (eData.eventType == "CLOSE_MAIL_WINDOW")
            {
                Hide();
            }
            else if (eData.eventType == "CLOSE_READ_MAIL_WINDOW")
            {
                if (hideOnShowMailList)
                    Hide();
            }
            else if (eData.eventType == "MAIL_SELECTED")
            {
                StartReadingMail();
            }
        }

        public void StartReadingMail()
        {
            mailBeingRead = Mailing.Instance.SelectedMail;
            if (mailBeingRead != null)
            {
                Mailing.Instance.SetMailRead(mailBeingRead);
                //gameObject.SetActive(true);
                if(!showing)
                    Show();
                if (nameText != null)
                    nameText.text = mailBeingRead.senderName;
                if (subjectText != null)
                    subjectText.text = mailBeingRead.subject;
                if (messageText != null)
                    messageText.text = mailBeingRead.message;
                UpdateAttachmentsDisplay();
                if (hideMailListOnStartRead)
                {
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_MAIL_LIST_WINDOW", args);
                }
                   
            }
            else
            {
                Hide();
            }
        }

        void UpdateAttachmentsDisplay()
        {
            mailBeingRead = Mailing.Instance.SelectedMail;
            if (mailBeingRead == null)
                return;
            // Items
            bool isAttachment = false;
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (mailBeingRead.items.Count > i && mailBeingRead.items[i].item != null)
                {
                    itemSlots[i].Show();
                    itemSlots[i].SetMailAttachmentData(mailBeingRead.items[i].item, mailBeingRead.items[i].count, i);
                    isAttachment = true;

                }
                else
                {
                    itemSlots[i].Hide();
                }
            }
            // Currency
            Currency c = mailBeingRead.GetMainCurrency();
            if (c != null && mailBeingRead.currencies[c] > 0)
            {
                takeCurrencyButton.HideVisualElement();
            }
            else
            { 
                if (isAttachment)
                    takeCurrencyButton.ShowVisualElement();
                else
                    takeCurrencyButton.HideVisualElement();
            }
            if(c==null)
                currencyDisplays.Hide();
            else
                currencyDisplays.SetData(c.id, mailBeingRead.currencies[c]);
       
        }

        public void TakeCurrency()
        {
            Mailing.Instance.TakeMailCurrency(mailBeingRead);
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i].IsVisible())
                    Mailing.Instance.TakeMailItem(i);
            }
        }
        public void DeleteMail()
        {
#if AT_I2LOC_PRESET
       if (subjectText != null)   UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("DeleteMailPopup") + " " + subjectText.text + "?", null, DeleteMail);
#else
            if (subjectText != null)
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Delete Mail " + subjectText.text + "?", null, DeleteMail);
#endif
        }

        public void DeleteMail(object item, bool accepted)
        {
            if (accepted)
            {
                Mailing.Instance.DeleteMail(mailBeingRead);
                Hide();
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("MAILBOX_OPEN", args);
             
            }
        }

        public void Reply()
        {
            Hide();
            string[] args = new string[2];
            args[0] = mailBeingRead.senderName;
            args[1] = mailBeingRead.subject;
            AtavismEventSystem.DispatchEvent("MAILBOX_OPEN_COMPOSE", args);
        }

        public void Return()
        {
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MAILBOX_OPEN", args);
            Hide();
        }
        protected override void onWindowCloseButtonClicked()
        {
            Hide();
            Mailing.Instance.SelectedMail = null;
        }

    }
}