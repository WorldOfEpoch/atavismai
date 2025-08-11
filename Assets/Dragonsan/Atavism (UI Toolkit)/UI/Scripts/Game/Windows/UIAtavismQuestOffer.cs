using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;


namespace Atavism.UI
{

    public class UIAtavismQuestOffer //: MonoBehaviour
    {

     private string questOfferTitle = "quest-offer-title";
     private string questOfferDescription = "quest-offer-description";
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

        private bool hostorical = false;
       // public List<UGUICurrency> currency1;
       //  public List<UGUICurrency> currency2;
        int questPos = -1;

        // Use this for initialization
        public UIAtavismQuestOffer()
        {
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", OnEvent);
            AtavismEventSystem.RegisterEvent("QUEST_ITEM_UPDATE", OnEvent);
        }
        ~UIAtavismQuestOffer()
        {
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", OnEvent);
            AtavismEventSystem.UnregisterEvent("QUEST_ITEM_UPDATE", OnEvent);
        }

        public void SetVisualElement(VisualElement visualElement, VisualTreeAsset rewardTemplate, VisualTreeAsset currencyTewmplate, VisualTreeAsset chooseRewardTemplate, VisualTreeAsset reputationTemplate)
        {
            questRewardTewmplate = rewardTemplate;
            questCurrencyTewmplate = currencyTewmplate;
            questChooreRewardTewmplate = chooseRewardTemplate;
            questRerputationTewmplate = reputationTemplate;
            
            m_questTitle = visualElement.Q<Label>(questOfferTitle);
            m_questDescription = visualElement.Q<Label>(questOfferDescription);
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
            m_questCurrencyList.Add(newListEntry);
            newListEntryLogic.ReverseOrder = true;
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

        public void UpdateQuestOfferDetails()
        {
            hostorical = false;
            UpdateQuestOfferDetails(0);
        }

        public void UpdateQuestOfferDetails(int questPos)
        {
            this.questPos = questPos;
            QuestLogEntry selectedQuest = Quests.Instance.GetQuestOfferedInfo(questPos);
            UpdateQuestOfferDetails(selectedQuest,hostorical);
        }

        public void UpdateQuestOfferDetails(QuestLogEntry selectedQuest, bool hostorical)
        {
            if (selectedQuest == null)
                return;
            this.hostorical = hostorical;
            int i0 = 0;
#if AT_I2LOC_PRESET
            if (m_questTitle != null)
                m_questTitle.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Title);
            if (m_questDescription != null)
                m_questDescription.text =
                    I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Description);
#else
            if (m_questTitle != null)
                m_questTitle.text = selectedQuest.Title;
            if (m_questDescription != null)
                m_questDescription.text = selectedQuest.Description;
#endif

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
#if AT_I2LOC_PRESET
     if (m_questObjective != null)
                m_questObjective.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Objective);
#else
            if (m_questObjective != null)
                m_questObjective.text = selectedQuest.Objective;
#endif
            
               for (int i = 0; i < objectiveTexts.Count; i++)
            {

                if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].objectives != null && i < selectedQuest.gradeInfo[0].objectives.Count)
                {
                    objectiveTexts[i].ShowVisualElement();
#if AT_I2LOC_PRESET
                string objectives = selectedQuest.gradeInfo[0].objectives[i];
                if (objectives != null && objectives != "" && objectives != ": 0/0") {
                    string nameOjective = "";
                    if (objectives.IndexOf(" slain:") != -1) {
                        string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.IndexOf(" slain:")));
                        nameOjective = I2.Loc.LocalizationManager.GetTranslation("slain") + " " + objectivesNames;
                    } else if (objectives.IndexOf(" collect:") != -1) {
                        string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.IndexOf(" collect:")));
                        nameOjective = I2.Loc.LocalizationManager.GetTranslation("collect") + " " + objectivesNames;
                    } else if (objectives.IndexOf(":") != -1) {
                        nameOjective = I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.LastIndexOf(':')));
                    }
                    string valueObjective = objectives.Remove(0, objectives.LastIndexOf(':') < 0 ? 0 : objectives.LastIndexOf(':'));
                    // if (history) valueObjective = "";
                    objectiveTexts[i].text = nameOjective + " " + valueObjective;
                }
                else objectiveTexts[i].text = "";
#else
                    objectiveTexts[i].text = selectedQuest.gradeInfo[0].objectives[i];
#endif
                }
                else
                {
                    objectiveTexts[i].HideVisualElement();
                }
            }
            
            
            
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && !this.hostorical)
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

            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && !this.hostorical)
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
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && !this.hostorical)
            {
                for (int i = 0; i < selectedQuest.gradeInfo[0].RewardItemsToChoose.Count; i++)
                {
                    if (itemChooseRewards.Count == i)
                        QuestChooseRewardSetupNewElement();
                    itemChooseRewards[i].Show();
                    itemChooseRewards[i].SetData(selectedQuest.gradeInfo[0].RewardItemsToChoose[i], null);
                    i0++;
                }
            }

            for (int i = 0; i < itemChooseRewards.Count-i0; i++)
            {
                itemChooseRewards[i0+i].Hide();
            }
            
            if (m_questChooseRewardList != null)
            {
                if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].RewardItemsToChoose.Count == 0 || this.hostorical)
                    m_questChooseRewardList.HideVisualElement();
                else
                    m_questChooseRewardList.ShowVisualElement();
            }

            if (m_questChooseRewardTitle != null)
            {
                if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && selectedQuest.gradeInfo[0].RewardItemsToChoose.Count == 0 || this.hostorical)
                    m_questChooseRewardTitle.HideVisualElement();
                else
                    m_questChooseRewardTitle.ShowVisualElement();
            }

            // Currency Rewards
            
            i0 = 0;
            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 && !this.hostorical)
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

            if (selectedQuest != null && selectedQuest.gradeInfo != null && selectedQuest.gradeInfo.Count > 0 &&
                selectedQuest.gradeInfo[0].rewardRep != null && selectedQuest.gradeInfo[0].rewardRep.Count > 0 && !this.hostorical)
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

        public void AcceptQuest()
        {
            Quests.Instance.AcceptQuest(0);
            this.questPos = -1;
        }

        public void DeclineQuest()
        {
            Quests.Instance.DeclineQuest(0);
            this.questPos = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eData"></param>
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "UPDATE_LANGUAGE" || eData.eventType == "QUEST_ITEM_UPDATE")
            {
                if (this.questPos != -1)
                    UpdateQuestOfferDetails(this.questPos);
            }
        }



    }
}