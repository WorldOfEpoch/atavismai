using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMailList
    {

        UIAtavismMailBox mailBox;
        UIAtavismMailRead mailReadPanel;
        bool showing = false;
        [SerializeField] bool hideMailListOnStartRead = false;
        private VisualElement m_Root;
        private ListView mailList;
        private Button newMessageButton;
        public void SetVisualElement(VisualElement visualElement , VisualTreeAsset mailListElementTemplate,UIAtavismMailBox mailBox)
        {
            m_Root = visualElement;
            mailList = m_Root.Q<ListView>("mail-list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = mailList.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            mailList.makeItem = () =>
            {
                // Instantiate a controller for the data
                UAtavismMailListEntry newListEntryLogic = new UAtavismMailListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = mailListElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // craftSlots.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            mailList.bindItem = (item, index) =>
            {
                var entry = (item.userData as UAtavismMailListEntry);
                MailEntry mail = Mailing.Instance.GetMailEntry(index);
                entry.SetMailEntryDetails(mail);
            };

            newMessageButton = m_Root.Q<Button>("new-message-button");
            newMessageButton.clicked += mailBox.ShowCompose;
            registerEvent();
            Refresh();
        }

        ~UIAtavismMailList()
        {
            unregisterEvent();
        }
        public void  Setup(bool hideMailListOnStartRead)
        {
            this.hideMailListOnStartRead = hideMailListOnStartRead;
            this.mailBox = mailBox;
            Refresh();
            Mailing.Instance.RequestMailList();
        }
        
        void registerEvent()
        {
            
            
            AtavismEventSystem.RegisterEvent("SHOW_MAIL", OnEvent);
            AtavismEventSystem.RegisterEvent("MAIL_UPDATE", OnEvent);
            AtavismEventSystem.RegisterEvent("MAIL_SENT", OnEvent);
            AtavismEventSystem.RegisterEvent("CLOSE_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.RegisterEvent("CLOSE_MAIL_LIST_WINDOW", OnEvent);
            AtavismEventSystem.RegisterEvent("MAIL_SELECTED", OnEvent);
        }

     
        void unregisterEvent()
        {
            AtavismEventSystem.UnregisterEvent("SHOW_MAIL", OnEvent);
            AtavismEventSystem.UnregisterEvent("MAIL_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("MAIL_SENT", OnEvent);
            AtavismEventSystem.UnregisterEvent("CLOSE_MAIL_WINDOW", OnEvent);
            AtavismEventSystem.UnregisterEvent("CLOSE_MAIL_LIST_WINDOW", OnEvent);
            AtavismEventSystem.UnregisterEvent("MAIL_SELECTED", OnEvent);
        }


        private void Refresh()
        {
            mailList.itemsSource = Mailing.Instance.MailList.ToArray();
            mailList.Rebuild();
            mailList.selectedIndex = -1;
        }

        void OnDisable()
        {
            unregisterEvent();
        }
        public void Show()
        {
            showing = true;
            m_Root.ShowVisualElement();
            Mailing.Instance.RequestMailList();
            Refresh();
        }

        public void Hide()
        {
            showing = false;
           m_Root.HideVisualElement();
        }

        // void Update()
        // {
        //     if (Input.GetKeyDown(toggleKey) && !ClientAPI.UIHasFocus())
        //     {
        //         if (showing)
        //         {
        //             Hide();
        //         }
        //         else
        //         {
        //             Mailing.Instance.RequestMailList();
        //             Show();
        //         }
        //     }
        // }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "SHOW_MAIL")
            {
                mailBox.Show();
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("CLOSE_READ_MAIL_WINDOW", args);
            }
            else if (eData.eventType == "MAIL_UPDATE")
            {
                if (!showing)
                    return;
                Refresh();
            }
            else if (eData.eventType == "CLOSE_MAIL_WINDOW")
            {
                mailBox.Hide();
            } else if (eData.eventType == "CLOSE_MAIL_LIST_WINDOW")
            {
                mailBox.Hide();
            }
            else if (eData.eventType == "MAIL_SELECTED")
            {
                if (hideMailListOnStartRead)
                    mailBox.Hide();
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("START_READ_MAIL_WINDOW", args);
                Refresh();
            }
        }

      
      
    }
}