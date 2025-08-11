using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMailCompose 
    {
        public UIAtavismMailBox mailBox;
        public UITextField toText;
        public UITextField subjectText;
        public UITextField messageText;
        UIAtavismCurrencyInputPanel uiAtavismCurrencyInputPanel;
        public List<UIAtavismMailAttachmentSlot> attachmentSlots = new List<UIAtavismMailAttachmentSlot>();
        [SerializeField] Toggle codToggle;
        [SerializeField] Toggle moneyToggle;
        [SerializeField] private int numberOfSlots = 5;

        private VisualElement m_Root;

        private Button sendButton;
        private Button cancelButton;
        private Button newMessageButton;
        private float clicklimit;


        // Use this for initialization
        void Start()
        {
            AtavismEventSystem.RegisterEvent("CLOSE_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.RegisterEvent("MAIL_SENT", OnEvent);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", OnEvent);
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CLOSE_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.UnregisterEvent("MAIL_SENT", OnEvent);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", OnEvent);
        }

        void OnEnable()
        {
            StartNewMail();
        }
        
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            toText = m_Root.Q<UITextField>("recipient");
            subjectText = m_Root.Q<UITextField>("subject");
            messageText = m_Root.Q<UITextField>("message");

            for (int i = 1; i <= numberOfSlots; i++)
            {
                VisualElement item = m_Root.Q<VisualElement>("item-"+i);
                if (item != null)
                {
                    UIAtavismMailAttachmentSlot slot = new UIAtavismMailAttachmentSlot();
                    slot.SetVisualElement(item);
                    slot.slotNum = i - 1;
                    attachmentSlots.Add(slot);
                }
            }

            codToggle = m_Root.Q<Toggle>("COD");
            codToggle.RegisterValueChangedCallback(SetCOD);
            moneyToggle = m_Root.Q<Toggle>("money");
            moneyToggle.RegisterValueChangedCallback(SetSendMoney);

            sendButton = m_Root.Q<Button>("send-button");
            sendButton.clicked += Send;
            cancelButton = m_Root.Q<Button>("cancel-button");
            cancelButton.clicked += Cancel;
            // newMessageButton = m_Root.Q<Button>("new-message-button");
            VisualElement currency = m_Root.Q<VisualElement>("input-currency");
            uiAtavismCurrencyInputPanel = new UIAtavismCurrencyInputPanel();
            uiAtavismCurrencyInputPanel.SetVisualElement(currency);
            uiAtavismCurrencyInputPanel.SetOnChange(checkCurrency);
            uiAtavismCurrencyInputPanel.SetCurrencyReverseOrder = true;
            uiAtavismCurrencyInputPanel.SetCurrencies(Inventory.Instance.GetMainCurrencies());
        }

        public void Setup(UIAtavismMailBox uiAtavismMailBox)
        {
            mailBox = uiAtavismMailBox;
        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CLOSE_MAIL_WINDOW")
            {
                this.mailBox.Hide();
            }
            else if (eData.eventType == "MAIL_SENT")
            {
                this.mailBox.ShowList();
            }
            else if (eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                // if (uiAtavismCurrencyInputPanel != null)
                // {
                //     uiAtavismCurrencyInputPanel.SetCurrencies(Inventory.Instance.GetMainCurrencies());
                // }
            }
        }

        public void StartNewMail()
        {
            Mailing.Instance.StartComposingMail();
            if (toText != null)
                toText.value = "";
            if (subjectText != null)
                subjectText.value = "";
            if (messageText != null)
                messageText.value = "";

            foreach (UIAtavismMailAttachmentSlot attachmentSlot in attachmentSlots)
            {
                attachmentSlot.Discarded();
            }

            if (moneyToggle != null)
                if (moneyToggle.value != !Mailing.Instance.MailBeingComposed.cashOnDelivery)
                    moneyToggle.value = !Mailing.Instance.MailBeingComposed.cashOnDelivery;
            if (codToggle != null)
                if (codToggle.value != Mailing.Instance.MailBeingComposed.cashOnDelivery)
                    codToggle.value = Mailing.Instance.MailBeingComposed.cashOnDelivery;

        }

        public void StartReplyMail(string to, string subject)
        {
            StartNewMail();
            if (toText != null)
                toText.value = to;
            if (subjectText != null)
                subjectText.value = "Re: " + subject;
        }

        public void SetMailTo(string to)
        {
            Mailing.Instance.MailBeingComposed.senderName = to;
        }

        public void SetSubject(string subject)
        {
            Mailing.Instance.MailBeingComposed.subject = subject;
        }

        public void SetMessage(string message)
        {
            Mailing.Instance.MailBeingComposed.message = message;
        }

      
        public void SetSendMoney(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                Mailing.Instance.MailBeingComposed.cashOnDelivery = !evt.newValue;
                if (codToggle != null)
                    if (codToggle.value != !evt.newValue)
                        codToggle.SetValueWithoutNotify(!evt.newValue);
            }
            else
            {
                moneyToggle.SetValueWithoutNotify(true);
            }

        }

        public void SetCOD(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                Mailing.Instance.MailBeingComposed.cashOnDelivery = evt.newValue;
                if (moneyToggle != null)
                    if (moneyToggle.value != !evt.newValue)
                        moneyToggle.SetValueWithoutNotify(!evt.newValue);
            }
            else
            {
                codToggle.SetValueWithoutNotify(true);
            }
        }

        public void Send()
        {
            if (toText != null)
                Mailing.Instance.MailBeingComposed.senderName = toText.value;
            if (messageText != null)
                Mailing.Instance.MailBeingComposed.message = messageText.value;
            if (subjectText != null)
                Mailing.Instance.MailBeingComposed.subject = subjectText.value;

            if (clicklimit > Time.time)
                return;
            clicklimit = Time.time + 1f;
            
            Mailing.Instance.SendMail();
            // Clear all attachment slots
            foreach (UIAtavismMailAttachmentSlot attachmentSlot in attachmentSlots)
            {
                if (attachmentSlot.UiActivatable != null)
                    attachmentSlot.Discarded();
            }
            uiAtavismCurrencyInputPanel.ClearCurrencyAmounts();
            //gameObject.SetActive(false);
            mailBox.ShowList();
        }

        public void Cancel()
        {
            mailBox.ShowList();
            foreach (UIAtavismMailAttachmentSlot attachmentSlot in attachmentSlots)
            {
                if (attachmentSlot.UiActivatable != null)
                    attachmentSlot.Discarded();
            }
            Hide();
        }

        public void Close()
        {
            foreach (UIAtavismMailAttachmentSlot attachmentSlot in attachmentSlots)
            {
                if (attachmentSlot.UiActivatable != null)
                    attachmentSlot.Discarded();
            }
            Hide();
        }

        void checkCurrency()
        {
            int currencyId = 0;
            long cost = 0;
            uiAtavismCurrencyInputPanel.GetCurrencyAmount(out currencyId, out cost);
            Mailing.Instance.SetMailCurrencyAmountConvert(currencyId, cost);
            List<Vector2> currencies = new List<Vector2>();
            currencies.Add(new Vector2(currencyId, cost));
            // foreach (Currency currency in Mailing.Instance.MailBeingComposed.currencies.Keys)
            // {
            //     currencies.Add(new Vector2(currency.id, Mailing.Instance.MailBeingComposed.currencies[currency]));
            // }

            if (Inventory.Instance.DoesPlayerHaveEnoughCurrency(currencies))
            {
                Debug.Log("Player does have enough currency");
                return ;
            }
            Debug.Log("Player does not have enough currency");
            return ;
        }

     
        public void Hide()
        {
            m_Root.HideVisualElement();
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }

     
    }
}