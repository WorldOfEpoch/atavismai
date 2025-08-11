using UnityEngine;
//using UnityEngine.UI.Tweens;
using UnityEngine.UIElements;
#if AT_MASTERAUDIO_PRESET
using DarkTonic.MasterAudio;
#endif

namespace Atavism.UI
{

    public class UIAtavismMailBox : UIAtavismWindowBase
    {
        public UIAtavismMailList mailList;
        public UIAtavismMailCompose mailCompose;
        [SerializeField] VisualTreeAsset mailListElementTemplate;
        [SerializeField] Label headerText;
        [SerializeField] bool hideMailListOnStartRead = false;
       // Use this for initialization

        void Awake()
        {
            AtavismEventSystem.RegisterEvent("MAILBOX_OPEN", this);
            this.Hide();
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            VisualElement _mailList = uiWindow.Q<VisualElement>("inbox");
            VisualElement _mailCompose = uiWindow.Q<VisualElement>("compose");

            mailList = new UIAtavismMailList();
            mailList.SetVisualElement(_mailList, mailListElementTemplate,this);
            mailList.Setup(hideMailListOnStartRead);

            mailCompose = new UIAtavismMailCompose();
            mailCompose.SetVisualElement(_mailCompose);
            mailCompose.Setup(this);
        //        Debug.LogError("registerUI End");
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("MAILBOX_OPEN", this);
            AtavismEventSystem.RegisterEvent("MAILBOX_OPEN_COMPOSE", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("MAILBOX_OPEN", this);
            AtavismEventSystem.UnregisterEvent("MAILBOX_OPEN_COMPOSE", this);

        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "MAILBOX_OPEN")
            {
                Mailing.Instance.RequestMailList();
                Show();
            } 
            if (eData.eventType == "MAILBOX_OPEN_COMPOSE")
            {
                Show();
                ShowCompose();
                mailCompose.StartReplyMail(eData.eventArgs[0],eData.eventArgs[1]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            base.Update();
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().mail.key)||Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().mail.altKey)) && !ClientAPI.UIHasFocus())
            {
                Toggle();
            }
        }
        public override void Show()
        {
            base.Show();
           // AtavismSettings.Instance.OpenWindow(this);
            //     gameObject.SetActive(true);
            AtavismUIUtility.BringToFront(this.gameObject);
            this.ShowList();
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MAILBOX_OPENED", args);
        }
        public override void Hide()
        {
            base.Hide();
           // AtavismSettings.Instance.CloseWindow(this);
            this.mailCompose.Close();
        }
   
        public void ShowCompose()
        {
            this.mailList.Hide();
            this.mailCompose.StartNewMail();
#if AT_I2LOC_PRESET
        if (this.headerText != null) this.headerText.text = I2.Loc.LocalizationManager.GetTranslation("New Message").ToUpper();
#else
            if (this.headerText != null)
                this.headerText.text = ("New Message").ToUpper();
#endif
            //   this.mailList.gameObject.SetActive(false);
            this.mailCompose.Show();
        }
        public void ShowList()
        {
            //   this.mailList.gameObject.SetActive(true);
            this.mailCompose.Hide();
            this.mailList.Show();
#if AT_I2LOC_PRESET
        if (this.headerText != null) this.headerText.text = I2.Loc.LocalizationManager.GetTranslation("MailBox").ToUpper();
#else
            if (this.headerText != null)
                this.headerText.text = ("MailBox").ToUpper();
#endif

        }



    }
}