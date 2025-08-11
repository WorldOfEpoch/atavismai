using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;


namespace Atavism.UI
{

    public class UIAtavismDialoguePanel : UIAtavismWindowBase
        // , IPointerDownHandler
    {

       
        //public Label windowTitle;
        [AtavismSeparator("Dialogue Settings")]
        public List<UIAtavismDialogueOptionEntry> interactionOptions =new List<UIAtavismDialogueOptionEntry>();
        [SerializeField] private VisualTreeAsset  interactionOptionTewmplate;
        public Sprite newQuestSprite;
        public Sprite progressQuestSprite;
        public Sprite dialogueSprite;
        public Sprite merchantSprite;
        public Sprite bankSprite;
        public Sprite repairSprite;
        public Sprite abilitySprite;
        public Sprite auctionSprite;
        public Sprite mailSprite;
        public Sprite gearModificationSprite;
        public Sprite guildWarehouseSprite;
        
        private VisualElement m_dialogueContentPanel;
        private Label m_dialogueText;
        private ListView m_dialogueOptionList;

        [AtavismSeparator("Quest Settings")]
        public UIAtavismQuestOffer s_questOfferPanel;
        [SerializeField] private VisualTreeAsset questRewardTemplate;
        [SerializeField] private VisualTreeAsset questCurrencyTemplate;
        [SerializeField] private VisualTreeAsset questChooseRewardTemplate;
        [SerializeField] private VisualTreeAsset questRerputationTemplate;

        private VisualElement m_buttonPanel;
        private Button m_button1;
        private Button m_button2;

       // public List<UGUIDialogueBarButton> bottomBarButtons;
        public VisualElement questOfferPanel;
        public VisualElement questProgressPanel;
        public VisualElement questConcludePanel;
        public UIAtavismQuestProgress s_questProgressPanel;
        public UIAtavismQuestConclude s_questConcludePanel;
        
        
      

        // Use this for initialization
        void Start()
        {
            base.Start();

          //  Hide();
            // Register for 
        }

        protected override void OnEnable()
        {
            base.OnEnable();
         //   Debug.LogError("UIAtavismDialoguePanel OnEnable End");
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("NPC_INTERACTIONS_UPDATE", this);
            AtavismEventSystem.RegisterEvent("DIALOGUE_UPDATE", this);
            AtavismEventSystem.RegisterEvent("QUEST_OFFERED_UPDATE", this);
            AtavismEventSystem.RegisterEvent("QUEST_PROGRESS_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CLOSE_NPC_DIALOGUE", this);
            AtavismEventSystem.RegisterEvent("MERCHANT_UI_OPENED", this);
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("NPC_INTERACTIONS_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("DIALOGUE_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("QUEST_OFFERED_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("QUEST_PROGRESS_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_NPC_DIALOGUE", this);
            AtavismEventSystem.UnregisterEvent("MERCHANT_UI_OPENED", this);
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
            base.unregisterEvents();
        }

     
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            
            //QuestOffer
            questOfferPanel = uiDocument.rootVisualElement.Query<VisualElement>("quest-offer-panel");
            s_questOfferPanel = new UIAtavismQuestOffer();
            s_questOfferPanel.SetVisualElement(questOfferPanel,questRewardTemplate, questCurrencyTemplate, questChooseRewardTemplate, questRerputationTemplate);
          
          //Dialog panel
            m_dialogueContentPanel = uiDocument.rootVisualElement.Query<VisualElement>("dialogue-panel");
            m_dialogueOptionList = uiDocument.rootVisualElement.Q<ListView>("dialogue-option-list");
            m_dialogueText = m_dialogueContentPanel.Q<Label>("dialogue-text");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = m_dialogueOptionList.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            m_dialogueOptionList.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismDialogueOptionEntry newListEntryLogic = new UIAtavismDialogueOptionEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = interactionOptionTewmplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry, this);
                interactionOptions.Add(newListEntryLogic);
                return newListEntry;
            };
            
            //Quest Progress
            questProgressPanel = uiDocument.rootVisualElement.Query<VisualElement>("quest-progress-panel");
            s_questProgressPanel = new UIAtavismQuestProgress();
            s_questProgressPanel.SetVisualElement(questProgressPanel);
            questConcludePanel = uiDocument.rootVisualElement.Query<VisualElement>("quest-conclude-panel");
            s_questConcludePanel = new UIAtavismQuestConclude();
            s_questConcludePanel.SetVisualElement(questConcludePanel,questRewardTemplate, questCurrencyTemplate, questChooseRewardTemplate, questRerputationTemplate);
            //bottom buttons
            m_buttonPanel = uiDocument.rootVisualElement.Query<VisualElement>("button-panel");
            m_button1 = m_buttonPanel.Q<Button>("button-1");
            m_button2 = m_buttonPanel.Q<Button>("button-2");

            return true;
        }

        void OnDestroy()
        {
           base.OnDisable();
        }

        // public virtual void OnPointerDown(PointerEventData eventData)
        // {
        //     // Focus the window
        //     AtavismUIUtility.BringToFront(this.gameObject);
        // }
        public void Show()
        {
            //AtavismSettings.Instance.OpenWindow(this);
            base.Show();
          
            string mName = "";
            if (NpcInteraction.Instance.NpcId != null && ClientAPI.GetObjectNode(NpcInteraction.Instance.NpcId.ToLong()) != null)
            {
                mName = (string)ClientAPI.GetObjectNode(NpcInteraction.Instance.NpcId.ToLong()).GetProperty("displayName");
#if AT_I2LOC_PRESET
                if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName))) mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName);
#endif
                if (uiWindowTitle != null)
                    uiWindowTitle.text = mName;
            }
            else
            {
                if (uiWindowTitle != null)
                    uiWindowTitle.text = "";
            }
            //TODO UI Elements NPC panle BringToFront when show
            // AtavismUIUtility.BringToFront(this.gameObject);
        }

        public void Hide()
        {
           // AtavismSettings.Instance.CloseWindow(this);
           

            m_dialogueContentPanel.HideVisualElement();
            questOfferPanel.HideVisualElement();
            questProgressPanel.HideVisualElement();
            questConcludePanel.HideVisualElement();
            base.Hide();
        }

        public void OnEvent(AtavismEventData eData)
        {
            m_dialogueContentPanel.HideVisualElement();
            questOfferPanel.HideVisualElement();
            questProgressPanel.HideVisualElement();
            questConcludePanel.HideVisualElement();
            if (eData.eventType == "NPC_INTERACTIONS_UPDATE")
            {
                m_dialogueContentPanel.ShowVisualElement();
                ShowOptions();
            }
            else if (eData.eventType == "DIALOGUE_UPDATE")
            {
                m_dialogueContentPanel.ShowVisualElement();
                ShowChat();
            }
            else if (eData.eventType == "QUEST_OFFERED_UPDATE")
            {
                Show();
                m_dialogueContentPanel.HideVisualElement();
                questOfferPanel.ShowVisualElement();
                s_questOfferPanel.UpdateQuestOfferDetails();
                HideButtonBars();
                if (m_button1 != null)
                {
                    m_button1.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button1.text = I2.Loc.LocalizationManager.GetTranslation("Accept");
#else
                    m_button1.text = "Accept";
#endif
                    m_button1.clicked += AcceptQuest;
                }
                
                if (m_button2 != null)
                {
                    m_button2.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button2.text = I2.Loc.LocalizationManager.GetTranslation("Decline");
#else
                    m_button2.text = "Decline";
#endif
                    m_button2.clicked += DeclineQuest;
                }
//                 if (bottomBarButtons.Count > 0)
//                 {
//                     bottomBarButtons[0].gameObject.SetActive(true);
//                     
// #if AT_I2LOC_PRESET
//                 bottomBarButtons[0].SetButtonFunction(questOfferPanel.AcceptQuest, I2.Loc.LocalizationManager.GetTranslation("Accept"));
// #else
//                     bottomBarButtons[0].SetButtonFunction(s_questOfferPanel.AcceptQuest, "Accept");
// #endif
//                      bottomBarButtons[0].GetComponent<Button>().interactable = true;
//                 }
//                 if (bottomBarButtons.Count > 1)
//                 {
//                     bottomBarButtons[1].gameObject.SetActive(true);
// #if AT_I2LOC_PRESET
//                 bottomBarButtons[1].SetButtonFunction(questOfferPanel.DeclineQuest, I2.Loc.LocalizationManager.GetTranslation("Decline"));
// #else
//                     bottomBarButtons[1].SetButtonFunction(s_questOfferPanel.DeclineQuest, "Decline");
// #endif
//                      bottomBarButtons[1].GetComponent<Button>().interactable = true;
//                 }
            }
            else if (eData.eventType == "QUEST_PROGRESS_UPDATE")
            {
                Show();
                m_dialogueContentPanel.HideVisualElement();
                questProgressPanel.ShowVisualElement();
               if(s_questProgressPanel!=null) s_questProgressPanel.UpdateQuestProgressDetails();
                HideButtonBars();
                if (m_button1 != null)
                {
                    m_button1.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button1.text = I2.Loc.LocalizationManager.GetTranslation("Continue");
#else
                    m_button1.text = "Continue";
#endif
                    if (Quests.Instance.GetQuestProgressInfo(0).Complete)
                    m_button1.clicked += ShowConcludeQuestPanel;
                }
                
                if (m_button2 != null)
                {
                    m_button2.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button2.text = I2.Loc.LocalizationManager.GetTranslation("Cancel");
#else
                    m_button2.text = "Cancel";
#endif
                    m_button2.clicked += Hide;
                }
//                 if (bottomBarButtons.Count > 0)
//                 {
//                     bottomBarButtons[0].gameObject.SetActive(true);
// #if AT_I2LOC_PRESET
//                 bottomBarButtons[0].SetButtonFunction(questProgressPanel.Continue, I2.Loc.LocalizationManager.GetTranslation("Continue"));
// #else
//                     bottomBarButtons[0].SetButtonFunction(s_questProgressPanel.Continue, "Continue");
// #endif
//                     if (Quests.Instance.GetQuestProgressInfo(0).Complete)
//                     {
//                          bottomBarButtons[0].GetComponent<Button>().interactable = true;
//                     }
//                     else
//                     {
//                          bottomBarButtons[0].GetComponent<Button>().interactable = false;
//                     }
//                 }
//                 if (bottomBarButtons.Count > 1)
//                 {
//                     bottomBarButtons[1].gameObject.SetActive(true);
// #if AT_I2LOC_PRESET
//                 bottomBarButtons[1].SetButtonFunction(questProgressPanel.Cancel, I2.Loc.LocalizationManager.GetTranslation("Cancel"));
// #else
//                     bottomBarButtons[1].SetButtonFunction(s_questProgressPanel.Cancel, "Cancel");
// #endif
//                      bottomBarButtons[1].GetComponent<Button>().interactable = true;
//                 }
            }
            else if (eData.eventType == "CLOSE_NPC_DIALOGUE")
            {
                Hide();
            }
            else if (eData.eventType == "MERCHANT_UI_OPENED")
            {
                Hide();
            }
            else if (eData.eventType == "UPDATE_LANGUAGE")
            {
                if (NpcInteraction.Instance.NpcId != null && ClientAPI.GetObjectNode(NpcInteraction.Instance.NpcId.ToLong()) != null)
                {
                    string mName = ClientAPI.GetObjectNode(NpcInteraction.Instance.NpcId.ToLong()).Name;
#if AT_I2LOC_PRESET
                if (I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != "") mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName);
#endif
                    if (uiWindowTitle != null)
                        uiWindowTitle.text = mName.ToUpper();
                }
                else
                {
                    if (uiWindowTitle != null)
                        uiWindowTitle.text = "";
                }

            }
        }

        public void ShowQuestList()
        {
            questProgressPanel.HideVisualElement();
            questConcludePanel.HideVisualElement();
            questOfferPanel.HideVisualElement();
            m_dialogueContentPanel.ShowVisualElement();
            if (NpcInteraction.Instance.InteractionOptions.Count == 0)
            {
                Hide();
                return;
            }
            ShowOptions();

        }

        void ShowOptions()
        {
            Show();

            Dialogue d = NpcInteraction.Instance.Dialogue;
            if (d != null)
            {
                string dialText = d.text;
#if AT_I2LOC_PRESET
                dialText = I2.Loc.LocalizationManager.GetTranslation("Quests/" +dialText);
#endif
                if (m_dialogueText != null)
                {
                    m_dialogueText.text = dialText;
                    m_dialogueText.ShowVisualElement();
                }
            }
            else
            {
                if (m_dialogueText != null)
                {
                    m_dialogueText.text = "";
                    m_dialogueText.HideVisualElement();
                }
            }

            m_dialogueOptionList.Clear();
            m_dialogueOptionList.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismDialogueOptionEntry).SetData(
                    NpcInteraction.Instance.InteractionOptions[index]);
            };

            m_dialogueOptionList.itemsSource = NpcInteraction.Instance.InteractionOptions;
            m_dialogueOptionList.Rebuild();
            m_dialogueOptionList.selectedIndex = -1;
            // Only show one bottom bar, close
            HideButtonBars();
            if (m_button1 != null)
            {
                m_button1.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button1.text = I2.Loc.LocalizationManager.GetTranslation("Close");
#else
                m_button1.text = "Close";
#endif
                m_button1.clicked += Hide;
            }
        }

        // void DialogueNewElement()
        // {
        //     Debug.LogError("dialogueNewElement");
        //     UIAtavismDialogueOptionEntry newListEntryLogic = new UIAtavismDialogueOptionEntry();
        //     // Instantiate the UXML template for the entry
        //     var newListEntry = interactionOptionTewmplate.Instantiate();
        //     // Assign the controller script to the visual element
        //     newListEntry.userData = newListEntryLogic;
        //     // Initialize the controller script
        //     newListEntryLogic.SetVisualElement(newListEntry);
        //     m_dialogueOptionList.Add(newListEntry);
        //     interactionOptions.Add(newListEntryLogic);
        // }
        
        void ShowChat()
        {
            Show();

            Dialogue d = NpcInteraction.Instance.Dialogue;
            if (d != null)
            {
              //  Debug.LogError("Dialogue show "+d+" "+d.audioClip);
                if (d.audioClip.Length > 0)
                {
                    AtavismNpcAudioManager.Instance.PlayAudio(d.audioClip, ClientAPI.GetObjectNode(NpcInteraction.Instance.NpcId.ToLong()).GameObject, NpcInteraction.Instance.NpcId.ToLong());
                }
                string dialText = d.text;
#if AT_I2LOC_PRESET
                dialText =  I2.Loc.LocalizationManager.GetTranslation("Quests/" +dialText);
#endif
                if (m_dialogueText != null)
                {
                    m_dialogueText.text = dialText;
                    m_dialogueText.ShowVisualElement();
                }
            }
            m_dialogueOptionList.Clear();
            m_dialogueOptionList.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismDialogueOptionEntry).SetData(NpcInteraction.Instance.Dialogue.actions[index]);
            };
                
            m_dialogueOptionList.itemsSource = NpcInteraction.Instance.Dialogue.actions;
            m_dialogueOptionList.Rebuild();
            m_dialogueOptionList.selectedIndex = -1;
            
            
            // Only show one bottom bar, close
            HideButtonBars();
            if (m_button1 != null)
            {
                m_button1.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button1.text = I2.Loc.LocalizationManager.GetTranslation("Close");
#else
                m_button1.text = "Close";
#endif
                m_button1.clicked += Hide;
            }
        }

        public void ShowConcludeQuestPanel()
        {
            questConcludePanel.ShowVisualElement();
            s_questConcludePanel.UpdateQuestConcludeDetails();
            questProgressPanel.HideVisualElement();

            HideButtonBars();
            if (m_button1 != null)
            {
                m_button1.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button1.text = I2.Loc.LocalizationManager.GetTranslation("Complete");
#else
                m_button1.text = "Complete";
#endif
                m_button1.clicked += CompleteQuest;
            }

            if (m_button2 != null)
            {
                m_button2.ShowVisualElement();
#if AT_I2LOC_PRESET
                m_button2.text = I2.Loc.LocalizationManager.GetTranslation("Cancel");
#else
                m_button2.text = "Cancel";
#endif
                m_button2.clicked += Hide;
            }

//             if (bottomBarButtons.Count > 0)
//             {
//                 bottomBarButtons[0].gameObject.SetActive(true);
// #if AT_I2LOC_PRESET
//             bottomBarButtons[0].SetButtonFunction(questConcludePanel.CompleteQuest, I2.Loc.LocalizationManager.GetTranslation("Complete"));
// #else
//                 bottomBarButtons[0].SetButtonFunction(s_questConcludePanel.CompleteQuest, "Complete");
// #endif
//             }
//             if (bottomBarButtons.Count > 1)
//             {
//                 bottomBarButtons[1].gameObject.SetActive(true);
// #if AT_I2LOC_PRESET
//             bottomBarButtons[1].SetButtonFunction(questProgressPanel.Cancel, I2.Loc.LocalizationManager.GetTranslation("Cancel"));
// #else
//                 bottomBarButtons[1].SetButtonFunction(s_questProgressPanel.Cancel, "Cancel");
// #endif
//             }
        }
        public void CompleteQuest()
        {
            if (!Quests.Instance.CompleteQuest())
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
#if AT_I2LOC_PRESET
            args[0] = I2.Loc.LocalizationManager.GetTranslation("You must select an item reward before completing this Quest.");
#else
                args[0] = "You must select an item reward before completing this Quest.";
#endif
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                return;
            }

           Hide();
          
           // this.questPos = -1;
        }
        public void AcceptQuest()
        {
            Quests.Instance.AcceptQuest(0);
            Hide();
           
           // this.questPos = -1;
        }
        
        public void DeclineQuest()
        {
            Quests.Instance.DeclineQuest(0);
            Hide();
            
       //     this.questPos = -1;
        }

        private void ResetButtons()
        {
            if (m_button1 != null)
            {
                m_button1.clicked -= Hide;
                m_button1.clicked -= CompleteQuest;
                m_button1.clicked -= ShowConcludeQuestPanel;
                m_button1.clicked -= AcceptQuest;
            }

            if (m_button2 != null)
            {
                m_button2.clicked -= Hide;
                m_button2.clicked -= DeclineQuest;
            }

        }
        
        void HideButtonBars()
        {
            ResetButtons();
            if (m_button1 != null)
            {
                m_button1.HideVisualElement();
            }

            if (m_button2 != null)
            {
                m_button2.HideVisualElement();
            }
        }
    }
}