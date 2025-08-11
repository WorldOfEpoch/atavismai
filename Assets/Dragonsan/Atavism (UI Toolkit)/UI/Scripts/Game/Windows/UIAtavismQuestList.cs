using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Atavism.UI.Game;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismQuestList : UIAtavismWindowBase
    {
        [SerializeField] private VisualTreeAsset questListTemplate;
        private List<UIAtavismQuestListEntry> questListOptions = new List<UIAtavismQuestListEntry>();
        private ListView questList;
         VisualElement questDetailsPanel;
         
        //  Label questTitle;
        //  Label questObjective;
        //  List<Label> objectiveTexts;
        //  Label questDescription;
        //  Label rewardTitle;
        //  VisualElement rewardPanel;
        // public List<UGUIItemDisplay> itemRewards;
        //  Label chooseTitle;
        //  VisualElement choosePanel;
        // public List<UGUIItemDisplay> chooseRewards;
        // public List<UGUICurrency> currency1;
        // public List<UGUICurrency> currency2;
        //  Label reputationTitle;
        //  VisualElement reputation1Panel;
        //  Label reputation1Name;
        //  Label reputation1Value;
        //  VisualElement reputation2Panel;
        //  Label reputation2Name;
        //  Label reputation2Value;
        
        Button abandonButton;
        
        // public KeyCode toggleKey;
        // public Button localizeButton;
        bool history = false;

        [AtavismSeparator("Menu Settings")]
        // public bool hideNormaleMenuImage = true;
        private UIButtonToggleGroup menu;
       
        [AtavismSeparator("Quest Settings")]
        public UIAtavismQuestOffer s_questOfferPanel;

        private Button questDetaleWindowCloseButton;
        [SerializeField] private VisualTreeAsset questRewardTemplate;
        [SerializeField] private VisualTreeAsset questCurrencyTemplate;
        [SerializeField] private VisualTreeAsset questChooseRewardTemplate;
        [SerializeField] private VisualTreeAsset questRerputationTemplate;

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            VisualElement inner_panel1 = uiWindow.Q<VisualElement>("inner-panel");
            menu = inner_panel1.Query<UIButtonToggleGroup>("menu");
            menu.OnItemIndexChanged += TopMenuChange;
            questList = inner_panel1.Q<ListView>("list");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = questList.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            questDetailsPanel = inner_panel1.Q<VisualElement>("quest-preview");
            questDetaleWindowCloseButton = questDetailsPanel.Query<Button>("Window-close-button");
            questDetaleWindowCloseButton.clicked += QuestExitClicked;
           // VisualElement  questOfferPanel = uiDocument.rootVisualElement.Query<VisualElement>("quest-offer-panel");
            s_questOfferPanel = new UIAtavismQuestOffer();
            s_questOfferPanel.SetVisualElement(questDetailsPanel,questRewardTemplate, questCurrencyTemplate, questChooseRewardTemplate, questRerputationTemplate);
            abandonButton = questDetailsPanel.Query<Button>("abandon-button");
            abandonButton.clicked += AbandonQuest;
            
            questList.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismQuestListEntry newListEntryLogic = new UIAtavismQuestListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = questListTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry, this);
                questListOptions.Add(newListEntryLogic);
                return newListEntry;
            };
            questList.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismQuestListEntry).SetQuestEntryDetails(
                    history ? Quests.Instance.QuestHistoryLogEntries[index] : Quests.Instance.QuestLogEntries[index],
                    index, this);
            };

            return true;
        }

        private void TopMenuChange(int obj)
        {
         //   Debug.LogError("TopMenuChange "+obj);
            switch (obj)
            {
                case 0:
                    ActiveQuests();
                    break;
                case 1:
                    HistoryQuests();
                    break;
            }
        }

        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            isRegisteredUI = false;
            abandonButton.clicked -= AbandonQuest;
            menu.OnItemIndexChanged -= TopMenuChange;
            questDetaleWindowCloseButton.clicked -= QuestExitClicked;
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("QUEST_LOG_UPDATE", this);
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
            AtavismEventSystem.RegisterEvent("QUEST_ITEM_UPDATE", this);
        }


        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("QUEST_LOG_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
            AtavismEventSystem.UnregisterEvent("QUEST_ITEM_UPDATE", this);
        }
    
        protected override void Start()
        {
            base.Start();
            // Delete the old list
            // ActiveQuests();
         
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ActiveQuests();
            questDetailsPanel.HideVisualElement();
        //    Debug.LogError("OnEnable |");
            // SetQuestDetails();
        }



        public override void Show()
        {
            // AtavismSettings.Instance.OpenWindow(this);
            // Delete the old list
            // ClearAllCells();
            //
            // Refresh();
            // AtavismUIUtility.BringToFront(this.gameObject);
            base.Show();
                
        }

        public override void Hide()
        {
            base.Hide();
           // Debug.LogError("Hide |");
          //  AtavismSettings.Instance.CloseWindow(this);
            // gameObject.SetActive(false);
            QuestExitClicked();
        }
        public void QuestExitClicked()
        {
           // Debug.LogError("QuestExitClicked |");
            Quests.Instance.QuestLogEntrySelected(-1);
            Quests.Instance.QuestHistoryLogEntrySelected(-1);
            questDetailsPanel.HideVisualElement();
        }

        protected override void Update()
        {
            base.Update();
            //if (Input.GetKeyDown(toggleKey) && !ClientAPI.UIHasFocus()) {
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().quest.key) ||Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().quest.altKey) ) && !ClientAPI.UIHasFocus())
            {
                if (showing)
                    Hide();
                else
                    Show();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "QUEST_LOG_UPDATE" || eData.eventType == "UPDATE_LANGUAGE" || eData.eventType == "QUEST_ITEM_UPDATE")
            {
                // Delete the old list
                 Refresh();

                QuestLogEntry selectedQuest;
                if (history)
                    selectedQuest = Quests.Instance.GetSelectedQuestHistoryLogEntry();
                else
                    selectedQuest = Quests.Instance.GetSelectedQuestLogEntry();
                if (selectedQuest == null)
                {
              //      Debug.LogError("OnEvent |");
                    // questDetailsPanel.HideVisualElement();
                    abandonButton.SetEnabled(false);
                    SetQuestDetails();
                    
                }
                else
                {
               //     Debug.LogError("OnEvent ||");
                    abandonButton.SetEnabled(true);
                    SetQuestDetails();
                }
            }
        }

        private void Refresh()
        {
            questList.Clear();
            questList.itemsSource = history ? Quests.Instance.QuestHistoryLogEntries : Quests.Instance.QuestLogEntries;
            questList.Rebuild();
            questList.selectedIndex = -1;
        }

        public void SetQuestDetails()
        {
            // Refresh();
            // if (questDetailScroll != null)
            //     questDetailScroll.value = 1;
            QuestLogEntry selectedQuest;
            if (history)
                selectedQuest = Quests.Instance.GetSelectedQuestHistoryLogEntry();
            else
                selectedQuest = Quests.Instance.GetSelectedQuestLogEntry();

            
            s_questOfferPanel.UpdateQuestOfferDetails(selectedQuest,history);
            if (selectedQuest == null)
            {
                questDetailsPanel.HideVisualElement(); 
            }
            else
            {
                questDetailsPanel.ShowVisualElement();
            }

         
            if (history)
            {
                abandonButton.visible = false;
            }else
            {
                abandonButton.visible = true;
            }
          //  Debug.LogError("SetQuestDetails");
            return;
            
//             if (selectedQuest == null)
//             {
//                
//                     questDetailsPanel.HideVisualElement();
//                 
//                 abandonButton.SetEnabled(false);
//                 return;
//             }
//             abandonButton.SetEnabled( true);
//             if (history)
//             {
//                 abandonButton.HideVisualElement();
//                 if (localizeButton != null)
//                     localizeButton.HideVisualElement();
//             }
//             else
//             {
//                 abandonButton.ShowVisualElement();
//                 if (localizeButton != null)
//                     localizeButton.ShowVisualElement();
//             }
//
//                
// #if AT_I2LOC_PRESET
//         if (questTitle != null)  questTitle.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Title);
//         if (questObjective!=null) questObjective.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Objective);
// #else
//             if (questTitle != null)
//                 questTitle.text = selectedQuest.Title;
//
//             if (questObjective != null)
//                 questObjective.text = selectedQuest.Objective;
// #endif
//             for (int i = 0; i < objectiveTexts.Count; i++)
//             {
//
//                 if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].objectives != null && i < selectedQuest.gradeInfo[0].objectives.Count)
//                 {
//                     objectiveTexts[i].ShowVisualElement();
// #if AT_I2LOC_PRESET
//                 string objectives = selectedQuest.gradeInfo[0].objectives[i];
//                 if (objectives != null && objectives != "" && objectives != ": 0/0") {
//                     string nameOjective = "";
//                     if (objectives.IndexOf(" slain:") != -1) {
//                         string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.IndexOf(" slain:")));
//                         nameOjective = I2.Loc.LocalizationManager.GetTranslation("slain") + " " + objectivesNames;
//                     } else if (objectives.IndexOf(" collect:") != -1) {
//                         string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.IndexOf(" collect:")));
//                         nameOjective = I2.Loc.LocalizationManager.GetTranslation("collect") + " " + objectivesNames;
//                     } else if (objectives.IndexOf(":") != -1) {
//                         nameOjective = I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.LastIndexOf(':')));
//                     }
//                     string valueObjective = objectives.Remove(0, objectives.LastIndexOf(':') < 0 ? 0 : objectives.LastIndexOf(':'));
//                     if (history) valueObjective = "";
//                     objectiveTexts[i].text = nameOjective + " " + valueObjective;
//                 }
//                 else objectiveTexts[i].text = "";
// #else
//                     objectiveTexts[i].text = selectedQuest.gradeInfo[0].objectives[i];
// #endif
//                 }
//                 else
//                 {
//                     objectiveTexts[i].HideVisualElement();
//                 }
//             }
//
// #if AT_I2LOC_PRESET
//        if (questDescription != null) questDescription.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Description);
// #else
//             if (questDescription != null)
//                 questDescription.text = selectedQuest.Description;
// #endif
//             int i0 = 0;
//             if (!history && selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].expReward != null && selectedQuest.gradeInfo[0].expReward > 0)
//             {
//                 itemRewards[i0].gameObject.SetActive(true);
// #if AT_I2LOC_PRESET
//              if (itemRewards[i0].itemName!=null)   itemRewards[i0].itemName.text = selectedQuest.gradeInfo[0].expReward.ToString() + " " + I2.Loc.LocalizationManager.GetTranslation("EXP");
// #else
//                 if (itemRewards[i0].itemName != null)
//                     itemRewards[i0].itemName.text = selectedQuest.gradeInfo[0].expReward.ToString() + " EXP";
// #endif
//                 itemRewards[i0].itemIcon.sprite = AtavismSettings.Instance.expIcon;
//                 if (itemRewards[i0].countText != null)
//                     itemRewards[i0].countText.text = selectedQuest.gradeInfo[0].expReward.ToString();
//                 i0++;
//             }
//             else
//             {
//                 AtavismLogger.LogWarning("No Exp reward");
//             }
//
//             // Item Rewards
//             for (int i = i0; i < itemRewards.Count; i++)
//             {
//                 if (!history && selectedQuest != null && selectedQuest.gradeInfo != null && i - i0 < selectedQuest.gradeInfo[0].rewardItems.Count)
//                 {
//                     itemRewards[i].gameObject.SetActive(true);
//                     itemRewards[i].SetItemData(selectedQuest.gradeInfo[0].rewardItems[i - i0].item, null);
//                 }
//                 else
//                 {
//                     itemRewards[i].gameObject.SetActive(false);
//                 }
//             }
//             if (rewardPanel != null)
//             {
//                 if ((selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].rewardItems.Count == 0 && selectedQuest.gradeInfo[0].expReward == 0) || history)
//                     rewardPanel.HideVisualElement();
//                 else
//                     rewardPanel.ShowVisualElement();
//             }
//             if (rewardTitle != null)
//             {
//                 if ((selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].rewardItems.Count == 0 && selectedQuest.gradeInfo[0].currencies.Count == 0 )|| history)
//                     rewardTitle.HideVisualElement();
//                 else
//                     rewardTitle.ShowVisualElement();
//             }
//
//             // Item Choose Rewards
//             for (int i = 0; i < chooseRewards.Count; i++)
//             {
//                 if (!history && selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && i < selectedQuest.gradeInfo[0].RewardItemsToChoose.Count)
//                 {
//                     chooseRewards[i].gameObject.SetActive(true);
//                     chooseRewards[i].SetItemData(selectedQuest.gradeInfo[0].RewardItemsToChoose[i].item, null);
//                 }
//                 else
//                 {
//                     chooseRewards[i].gameObject.SetActive(false);
//                 }
//             }
//             if (choosePanel != null)
//             {
//                 if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].RewardItemsToChoose.Count == 0 || history)
//                     choosePanel.HideVisualElement();
//                 else
//                     choosePanel.ShowVisualElement();
//             }
//             if (chooseTitle != null)
//             {
//                 if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].RewardItemsToChoose.Count == 0 || history)
//                     chooseTitle.HideVisualElement();
//                 else
//                     chooseTitle.ShowVisualElement();
//             }
//
//             // Currency Rewards
//             if (!history && selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].currencies != null && selectedQuest.gradeInfo[0].currencies.Count > 0)
//             {
//                 List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(selectedQuest.gradeInfo[0].currencies[0].id,
//                                                                                                               selectedQuest.gradeInfo[0].currencies[0].count);
//                 for (int i = 0; i < currency1.Count; i++)
//                 {
//                     if (i < currencyDisplayList.Count)
//                     {
//                         currency1[i].gameObject.SetActive(true);
//                         currency1[i].SetCurrencyDisplayData(currencyDisplayList[i]);
//                     }
//                     else
//                     {
//                         currency1[i].gameObject.SetActive(false);
//                     }
//                 }
//             }
//             else
//             {
//                 for (int i = 0; i < currency1.Count; i++)
//                 {
//                     currency1[i].gameObject.SetActive(false);
//                 }
//             }
//             if (!history && selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].currencies != null && selectedQuest.gradeInfo[0].currencies.Count > 1)
//             {
//
//                 List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(selectedQuest.gradeInfo[0].currencies[1].id,
//                                                                                                               selectedQuest.gradeInfo[0].currencies[1].count);
//                 for (int i = 0; i < currency2.Count; i++)
//                 {
//                     if (i < currencyDisplayList.Count)
//                     {
//                         currency2[i].gameObject.SetActive(true);
//                         currency2[i].SetCurrencyDisplayData(currencyDisplayList[i]);
//                     }
//                     else
//                     {
//                         currency2[i].gameObject.SetActive(false);
//                     }
//                 }
//             }
//             else
//             {
//                 for (int i = 0; i < currency2.Count; i++)
//                 {
//                     currency2[i].gameObject.SetActive(false);
//                 }
//             }
//
//             if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].rewardRep != null && selectedQuest.gradeInfo[0].rewardRep.Count > 0)
//             {
//                 if (reputationTitle != null)
//                     reputationTitle.ShowVisualElement();
//                 if (reputation1Panel != null)
//                     reputation1Panel.ShowVisualElement();
//                 if (reputation1Name != null)
//                 {
//                     reputation1Name.ShowVisualElement();
// #if AT_I2LOC_PRESET
//                     reputation1Name.text = I2.Loc.LocalizationManager.GetTranslation(selectedQuest.gradeInfo[0].rewardRep[0].name);
// #else
//                     reputation1Name.text = selectedQuest.gradeInfo[0].rewardRep[0].name;
// #endif
//                 }
//                 if (reputation1Value != null)
//                 {
//                     reputation1Value.ShowVisualElement();
//                     reputation1Value.text = selectedQuest.gradeInfo[0].rewardRep[0].count.ToString();
//                     if (selectedQuest.gradeInfo[0].rewardRep[0].count > 0)
//                         reputation1Value.style.color = Color.green;
//                     else
//                         reputation1Value.style.color = Color.red;
//                 }
//                 if (selectedQuest.gradeInfo[0].rewardRep.Count > 1)
//                 {
//                     if (reputation2Panel != null)
//                         reputation2Panel.ShowVisualElement();
//                     if (reputation2Name != null)
//                     {
//                         reputation2Name.ShowVisualElement();
// #if AT_I2LOC_PRESET
//                         reputation2Name.text =  I2.Loc.LocalizationManager.GetTranslation(selectedQuest.gradeInfo[0].rewardRep[1].name);
// #else
//                         reputation2Name.text = selectedQuest.gradeInfo[0].rewardRep[1].name;
// #endif
//                     }
//                     if (reputation2Value != null)
//                     {
//                         reputation2Value.ShowVisualElement();
//                         reputation2Value.text = selectedQuest.gradeInfo[0].rewardRep[1].count.ToString();
//                         if (selectedQuest.gradeInfo[0].rewardRep[1].count > 0)
//                             reputation2Value.style.color = Color.green;
//                         else
//                             reputation2Value.style.color = Color.red;
//                     }
//                 }
//                 else
//                 {
//                     if (reputation2Panel != null)
//                         reputation2Panel.HideVisualElement();
//                     if (reputation2Name != null)
//                         reputation2Name.HideVisualElement();
//                     if (reputation2Value != null)
//                         reputation2Value.HideVisualElement();
//                 }
//             }
//             else
//             {
//                 if (reputationTitle != null)
//                     reputationTitle.HideVisualElement();
//                 if (reputation1Panel != null)
//                     reputation1Panel.HideVisualElement();
//                 if (reputation1Name != null)
//                     reputation1Name.HideVisualElement();
//                 if (reputation1Value != null)
//                     reputation1Value.HideVisualElement();
//                 if (reputation2Panel != null)
//                     reputation2Panel.HideVisualElement();
//                 if (reputation2Name != null)
//                     reputation2Name.HideVisualElement();
//                 if (reputation2Value != null)
//                     reputation2Value.HideVisualElement();
//             }

        }

        public void LocalizeTargets()
        {
            Quests.Instance.ClickedQuest(Quests.Instance.GetSelectedQuestLogEntry());
        }

        public void HistoryQuests()
        {
           // Debug.LogError("HistoryQuests");
            Quests.Instance.QuestLogEntrySelected(-1);
            history = true;
            Refresh();
            questDetailsPanel.HideVisualElement();
        }

        public void ActiveQuests()
        {
           // Debug.LogError("ActiveQuests");
            history = false;
          
            Quests.Instance.QuestHistoryLogEntrySelected(-1);
            Refresh();
            questDetailsPanel.HideVisualElement();
        }
        public bool History
        {
            get
            {
                return history;
            }
        }

        public void AbandonQuest()
        {
            QuestLogEntry selectedQuest;
            if (history)
                selectedQuest = Quests.Instance.GetSelectedQuestHistoryLogEntry();
            else
                selectedQuest = Quests.Instance.GetSelectedQuestLogEntry();
#if AT_I2LOC_PRESET
        if (selectedQuest != null)  UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want abandon") + " " + I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Title) + "?", null, AbandonQuest);
#else
            if (selectedQuest != null)
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want abandon " + selectedQuest.Title + "?", null, AbandonQuest);
#endif

        }

        public void AbandonQuest(object item, bool accepted)
        {
            if (accepted)
            {
                Quests.Instance.AbandonQuest();
                questDetailsPanel.HideVisualElement();
                Refresh();
            }
        }

    }
}