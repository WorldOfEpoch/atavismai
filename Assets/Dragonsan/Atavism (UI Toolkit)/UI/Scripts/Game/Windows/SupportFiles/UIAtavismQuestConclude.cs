using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismQuestConclude 
    {

        
        private string questOfferTitle = "quest-offer-title";
        private string questOfferDescription = "quest-offer-description";
        private string questOfferObjectiveTitleText = "quest-offer-objective-title";
        private string questOfferObjectiveText = "quest-offer-objective-text";
        private string questOfferObjectives = "quest-offer-objective-";

     
        private string questOfferRewardTitle = "quest-offer-reward-title";
        private string questOfferRewardList = "quest-offer-reward-list";
     
        private string questOfferCurrencyList = "quest-offer-currency-list";
     
        private string questOfferChooseRewardTitle = "quest-offer-choose-reward-title";
        private string questOfferChooseRewardList = "quest-offer-choose-reward-list";
     
        private string questOfferReputationTitle = "quest-offer-reputation-title";
        private string questOfferReputationList = "quest-offer-reputation-list";
     
        private Label m_questTitle;
        private Label m_questDescription;
        private Label m_questObjectiveTitle;
        private Label m_questObjective;
        List<Label> objectiveTexts = new List<Label>();
        private VisualElement m_questRewardList;
        private VisualElement m_questCurrencyList;
        private VisualElement m_questChooseRewardList;
        private VisualElement m_questReputationList;
     
        private Label m_questRewardTitle;
        private Label m_questChooseRewardTitle;
        private Label m_questReputationTitle;


        private VisualTreeAsset questRewardTewmplate;
        private VisualTreeAsset questCurrencyTewmplate;
        private VisualTreeAsset questChooreRewardTewmplate;
        private VisualTreeAsset questRerputationTewmplate;

        public List<UIAtavismQuestRewardEntry> itemRewards = new List<UIAtavismQuestRewardEntry>();
        public List<UIAtavismQuestRewardEntry> itemChooseRewards = new List<UIAtavismQuestRewardEntry>();
        public List<UIAtavismCurrencyDisplay> currencyRewards = new List<UIAtavismCurrencyDisplay>();
        public List<UIAtavismReputationDisplay> m_reputationRewards = new List<UIAtavismReputationDisplay>();
        
        int questPos = -1;

        // Use this for initialization
         public UIAtavismQuestConclude()
        {
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", OnEvent);
        }
        ~UIAtavismQuestConclude()
        {
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", OnEvent);
        }

       

        public void SetVisualElement(VisualElement visualElement, VisualTreeAsset rewardTemplate, VisualTreeAsset currencyTewmplate, VisualTreeAsset chooseRewardTemplate, VisualTreeAsset reputationTemplate)
        {
            questRewardTewmplate = rewardTemplate;
            questCurrencyTewmplate = currencyTewmplate;
            questChooreRewardTewmplate = chooseRewardTemplate;
            questRerputationTewmplate = reputationTemplate;
            
            m_questTitle = visualElement.Q<Label>(questOfferTitle);
            m_questDescription = visualElement.Q<Label>(questOfferDescription);
            m_questObjectiveTitle = visualElement.Q<Label>(questOfferObjectiveTitleText);
            m_questObjective = visualElement.Q<Label>(questOfferObjectiveText);
            m_questRewardTitle = visualElement.Q<Label>(questOfferRewardTitle);
            m_questChooseRewardTitle = visualElement.Q<Label>(questOfferChooseRewardTitle);
            m_questReputationTitle = visualElement.Q<Label>(questOfferReputationTitle);
            
            m_questRewardList = visualElement.Q<VisualElement>(questOfferRewardList);
            m_questCurrencyList = visualElement.Q<VisualElement>(questOfferCurrencyList);
            m_questChooseRewardList = visualElement.Q<VisualElement>(questOfferChooseRewardList);
            m_questReputationList = visualElement.Q<VisualElement>(questOfferReputationList);

            for (int i = 1; i < 10; i++)
            {
                Label label = visualElement.Q<Label>(questOfferObjectives+i);
                if(label!=null)
                    objectiveTexts.Add(label);
            }
            if(m_questObjective!=null)
                m_questObjective.HideVisualElement();
            if(m_questObjectiveTitle!=null)
                m_questObjectiveTitle.HideVisualElement();

        }
        
            void QuestRewardSetupNewElement()
        {
            UIAtavismQuestRewardEntry newListEntryLogic = new UIAtavismQuestRewardEntry();
            // Instantiate the UXML template for the entry
            var newListEntry = questRewardTewmplate.Instantiate();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
            m_questRewardList.Add(newListEntry);
            itemRewards.Add(newListEntryLogic);
        }
        void QuestChooseRewardSetupNewElement()
        {
            UIAtavismQuestRewardEntry newListEntryLogic = new UIAtavismQuestRewardEntry();
            // Instantiate the UXML template for the entry
            var newListEntry = questChooreRewardTewmplate.Instantiate();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
            m_questChooseRewardList.Add(newListEntry);
            itemChooseRewards.Add(newListEntryLogic);
        }
        
        void QuestCurrencyRewardSetupNewElement()
        {
            UIAtavismCurrencyDisplay newListEntryLogic = new UIAtavismCurrencyDisplay();
            // Instantiate the UXML template for the entry
            var newListEntry = questCurrencyTewmplate.Instantiate();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
            newListEntryLogic.ReverseOrder = true;
            m_questCurrencyList.Add(newListEntry);
            currencyRewards.Add(newListEntryLogic);
        }

        void QuestReputationRewardSetupNewElement()
        {
            UIAtavismReputationDisplay newListEntryLogic = new UIAtavismReputationDisplay();
            // Instantiate the UXML template for the entry
            var newListEntry = questRerputationTewmplate.Instantiate();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
            m_questReputationList.Add(newListEntry);
            m_reputationRewards.Add(newListEntryLogic);
        }

        public void UpdateQuestConcludeDetails()
        {
            UpdateQuestConcludeDetails(0);
        }
        public void UpdateQuestConcludeDetails(int questPos)
        {
            this.questPos = questPos;
            QuestLogEntry selectedQuest = Quests.Instance.GetQuestProgressInfo(questPos);
            UpdateQuestConcludeDetails(selectedQuest);
        }

        public void UpdateQuestConcludeDetails(QuestLogEntry selectedQuest)
        {
#if AT_I2LOC_PRESET
        if (m_questTitle != null) m_questTitle.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Title);
       if (m_questDescription != null)  m_questDescription.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.gradeInfo[0].completionText);
#else
            if (m_questTitle != null)
                m_questTitle.text = selectedQuest.Title;
            if (m_questDescription != null)
                m_questDescription.text = selectedQuest.gradeInfo[0].completionText;
#endif
            int i0 = 0;
            
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 &&
                selectedQuest.gradeInfo[0].expReward > 0)
            {
                if (itemRewards.Count == 0)
                    QuestRewardSetupNewElement();
                QuestRewardEntry qer = new QuestRewardEntry();
                itemRewards[i0].SetExpData(selectedQuest.gradeInfo[0].expReward);
                itemRewards[i0].Show();
                i0++;
            }
            
    
            // Item Rewards
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0)
            {

                // Item Rewards
                for (int i = 0; i < selectedQuest.gradeInfo[0].rewardItems.Count; i++)
                {
                    if (itemRewards.Count == i0)
                        QuestRewardSetupNewElement();
                    itemRewards[i0].Show();
                    itemRewards[i0].SetData(selectedQuest.gradeInfo[0].rewardItems[i], null);

                    i0++;
                }
            }

            for (int i = 0; i < itemRewards.Count-i0; i++)
            {
                itemRewards[i0+i].Hide();
            }
            
         
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0)
            {
                if (m_questRewardList != null)
                {
                    if (selectedQuest.gradeInfo[0].rewardItems.Count == 0 && i0 == 0)
                        m_questRewardList.HideVisualElement();
                    else
                        m_questRewardList.ShowVisualElement();
                }

                if (m_questRewardTitle != null)
                {
                    if (selectedQuest.gradeInfo[0].rewardItems.Count == 0 &&
                        selectedQuest.gradeInfo[0].currencies.Count == 0)
                        m_questRewardTitle.HideVisualElement();
                    else
                        m_questRewardTitle.ShowVisualElement();
                }

            }
            else
            {
                if (m_questRewardTitle != null)m_questRewardTitle.HideVisualElement();
                if (m_questChooseRewardTitle != null)  m_questChooseRewardTitle.HideVisualElement();
            }

         

            // Item Choose Rewards
            i0 = 0;
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0)
            {
                for (int i = 0; i < selectedQuest.gradeInfo[0].RewardItemsToChoose.Count; i++)
                {
                    if (itemChooseRewards.Count == i)
                        QuestChooseRewardSetupNewElement();
                    itemChooseRewards[i].Show();
                    itemChooseRewards[i].SetData(selectedQuest.gradeInfo[0].RewardItemsToChoose[i], ItemChosen);
                    i0++;
                }
            }

            for (int i = 0; i < itemChooseRewards.Count-i0; i++)
            {
                itemChooseRewards[i0+i].Hide();
            }
            
         
          

            // Currency Rewards
            i0 = 0;
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0)
            {
                for (int i = 0; i < selectedQuest.gradeInfo[0].currencies.Count; i++)
                {
                    if (currencyRewards.Count == i)
                        QuestCurrencyRewardSetupNewElement();
                    currencyRewards[i].Show();
                    currencyRewards[i].SetData(selectedQuest.gradeInfo[0].currencies[i].id,
                        selectedQuest.gradeInfo[0].currencies[i].count);
                    i0++;
                }
            }

            for (int i = 0; i < currencyRewards.Count-i0; i++)
            {
                currencyRewards[i0+i].Hide();
            }

            //Repoutation
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 &&
                selectedQuest.gradeInfo[0].rewardRep != null && selectedQuest.gradeInfo[0].rewardRep.Count > 0)
            {
                if (m_questReputationTitle != null)
                    m_questReputationTitle.ShowVisualElement();
                if (m_questReputationList != null)
                    m_questReputationList.ShowVisualElement();


                
                i0 = 0;
                for (int i = 0; i < selectedQuest.gradeInfo[0].rewardRep.Count; i++)
                {
                    if (m_reputationRewards.Count == i)
                        QuestReputationRewardSetupNewElement();
                    m_reputationRewards[i].Show();
                    m_reputationRewards[i].SetData(selectedQuest.gradeInfo[0].rewardRep[i]);
                    i0++;
                }
                
                for (int i = 0; i < m_reputationRewards.Count-i0; i++)
                {
                    m_reputationRewards[i0+i].Hide();
                }
             
            }
            else
            {
                if (m_questReputationTitle != null)
                    m_questReputationTitle.HideVisualElement();
                if (m_questReputationList != null)
                    m_questReputationList.HideVisualElement();

              
            }

        }

        public void ItemChosen(AtavismInventoryItem item)
        {
            QuestLogEntry quest = Quests.Instance.GetQuestProgressInfo(0);
            quest.itemChosen = item.templateId;
            for (int i = 0; i < itemChooseRewards.Count; i++)
            {
                if (i < quest.gradeInfo[0].RewardItemsToChoose.Count)
                {
                    if (quest.gradeInfo[0].RewardItemsToChoose[i].item == item)
                        itemChooseRewards[i].Selected(true);
                    else
                        itemChooseRewards[i].Selected(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eData"></param>
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "UPDATE_LANGUAGE")
            {
                if (this.questPos != -1)
                    UpdateQuestConcludeDetails(this.questPos);
            }
        }

    }
}