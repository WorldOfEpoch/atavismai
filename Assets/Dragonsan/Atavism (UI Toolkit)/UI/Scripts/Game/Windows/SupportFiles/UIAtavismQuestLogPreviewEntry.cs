using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismQuestLogPreviewEntry
    {

        private Label m_questTitle;
        private Label m_questObjectivesText;
        private VisualTreeAsset m_questObjectiveTextTemplate;
        private VisualElement m_questObjectives;
        private List<Label> m_objectiveTexts = new List<Label>();
        private QuestLogEntry quest;
        private VisualElement m_root;

        private void OnEnable()
        {
            AtavismEventSystem.RegisterEvent("QUEST_LOG_UPDATE", OnEvent);
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", OnEvent);
        }

        public void SetVisualElement(VisualElement visualElement, VisualTreeAsset template)
        {
            m_root = visualElement;
            m_questTitle = m_root.Q<Label>("quest-preview-title-text");
            m_questTitle.RegisterCallback<PointerUpEvent>(ClickTitle);
            m_questObjectivesText = m_root.Q<Label>("quest-preview-objectives-text");
            m_questObjectiveTextTemplate = template;
            m_questObjectives = m_root.Q<VisualElement>("quest-preview-objectives");
        }


        private void OnDisable()
        {
            AtavismEventSystem.UnregisterEvent("QUEST_LOG_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", OnEvent);
        }

        public void SetQuestPrev(QuestLogEntry selectedQuest)
        {
            this.quest = selectedQuest;
            if (this.quest != null)
                UpdateQuestPrev();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "QUEST_LOG_UPDATE" || eData.eventType == "UPDATE_LANGUAGE")
            {
                if (this.quest != null)
                    UpdateQuestPrev();
            }
        }

        private void UpdateQuestPrev()
        {


            if (m_questTitle != null)
                if (quest.Complete)
                {
#if AT_I2LOC_PRESET

                m_questTitle.text =
 I2.Loc.LocalizationManager.GetTranslation("Quests/" + quest.Title) + " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")";
#else
                    m_questTitle.text = quest.Title + " (Complete)";
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                m_questTitle.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + quest.Title);
#else
                    m_questTitle.text = quest.Title;
#endif
                }

            if (m_questObjectivesText != null)
#if AT_I2LOC_PRESET
            m_questObjectivesText.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + quest.Objective);
#else
                m_questObjectivesText.text = quest.Objective;
#endif

            for (int i = 0; i < quest.gradeInfo[0].objectives.Count; i++)
            {
                if (m_objectiveTexts.Count <= i)
                {
                    TemplateContainer objective = m_questObjectiveTextTemplate.Instantiate();
                    m_questObjectives.Add(objective);
                    m_objectiveTexts.Add(objective.Q<Label>("quest-preview-objective-text"));

                }

                if (i < quest.gradeInfo[0].objectives.Count)
                {
                    m_objectiveTexts[i].visible = true;
                    string objectives = quest.gradeInfo[0].objectives[i];
#if AT_I2LOC_PRESET
                if (objectives != null && objectives != "" && objectives != ": 0/0")
                {
                    string nameOjective = "";
                    if (objectives.IndexOf(" slain:") != -1)
                    {
                        string objectivesNames =
 I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.IndexOf(" slain:")));
                        //    string objectivesValues = data.Remove(0, data.LastIndexOf(':') < 0 ? 0 : data.LastIndexOf(':')+1);
                        nameOjective = I2.Loc.LocalizationManager.GetTranslation("slain") + " " + objectivesNames;
                    }
                    else if (objectives.IndexOf(" collect:") != -1)
                    {
                        string objectivesNames =
 I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.IndexOf(" collect:")));
                        //    string objectivesValues = data.Remove(0, data.LastIndexOf(':') < 0 ? 0 : data.LastIndexOf(':')+1);
                        nameOjective = I2.Loc.LocalizationManager.GetTranslation("collect") + " " + objectivesNames;
                    }
                    else if (objectives.IndexOf(":") != -1)
                    {
                        nameOjective =
 I2.Loc.LocalizationManager.GetTranslation("Quests/" + objectives.Remove(objectives.LastIndexOf(':')));
                    }
                        string valueObjective =
 objectives.Remove(0, objectives.LastIndexOf(':') < 0 ? 0 : objectives.LastIndexOf(':'));
                        m_objectiveTexts[i].text = nameOjective + " " + valueObjective;
                }
                else m_objectiveTexts[i].text = "";

#else
                    m_objectiveTexts[i].text = objectives;
#endif
                }
                else
                {
                    m_objectiveTexts[i].visible = false;
                }
            }

        }

        public void Show()
        {
            m_root.ShowVisualElement();
        }

        public void Hide()
        {
            m_root.HideVisualElement();
        }

        public void ClickTitle(PointerUpEvent evt)
        {
            Quests.Instance.ClickedQuest(quest);
        }
    }
}