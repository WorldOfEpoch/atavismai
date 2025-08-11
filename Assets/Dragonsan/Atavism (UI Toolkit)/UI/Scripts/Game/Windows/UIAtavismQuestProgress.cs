using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismQuestProgress 
    {

        private string questTitle = "quest-progress-title";
        private string questDescription = "quest-progress-description";
        public Label m_questTitle;
        public Label m_questProgress;
        int m_questPos = -1;

        public void SetVisualElement(VisualElement visualElement)
        {
            m_questTitle = visualElement.Q<Label>(questTitle);
            m_questProgress = visualElement.Q<Label>(questDescription);
            
            if(m_questTitle == null)
                Debug.LogError("no tittle element for quest progress");
            if(m_questProgress == null)
                Debug.LogError("no description element for quest progress");
        }
        // Use this for initialization
        public UIAtavismQuestProgress()
        {
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", OnEvent);
            AtavismEventSystem.RegisterEvent("QUEST_ITEM_UPDATE", OnEvent);
        }
        ~UIAtavismQuestProgress()
        {
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", OnEvent);
            AtavismEventSystem.UnregisterEvent("QUEST_ITEM_UPDATE", OnEvent);
        }

        public void UpdateQuestProgressDetails()
        {
            UpdateQuestProgressDetails(0);
        }

        public void UpdateQuestProgressDetails(int questPos)
        {
            this.m_questPos = questPos;
            QuestLogEntry selectedQuest = Quests.Instance.GetQuestProgressInfo(questPos);
#if AT_I2LOC_PRESET
        if (m_questTitle!=null) m_questTitle.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.Title);
        if (m_questProgress!=null)  m_questProgress.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + selectedQuest.ProgressText);
#else
            if (m_questTitle != null)
                m_questTitle.text = selectedQuest.Title;
            if (m_questProgress != null)
                m_questProgress.text = selectedQuest.ProgressText;
#endif
        }

        public void Continue()
        {
            this.m_questPos = -1;
        }

        public void Cancel()
        {
            this.m_questPos = -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eData"></param>
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "UPDATE_LANGUAGE" || eData.eventType == "QUEST_ITEM_UPDATE")
            {
                if (this.m_questPos != -1)
                    UpdateQuestProgressDetails(this.m_questPos);
            }
        }
    }
}