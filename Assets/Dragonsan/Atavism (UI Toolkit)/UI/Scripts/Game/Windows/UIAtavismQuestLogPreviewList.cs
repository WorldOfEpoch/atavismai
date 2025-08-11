using System.Collections;
using System.Collections.Generic;
using Atavism;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismQuestLogPreviewList : MonoBehaviour
    {
        [AtavismSeparator("Window Base")] [SerializeField]
        protected UIDocument uiDocument;

        [AtavismSeparator("UI")] [SerializeField]
        private string listElementName = "quest-preview-container";
        [SerializeField] private VisualTreeAsset questPrevEntryTemplate;
        [SerializeField] private VisualTreeAsset questPrevEntryObjectiveTemplate;
        [SerializeField] private int maxNumberDisplayElements = 5;
        private VisualElement m_list;
        private List<UIAtavismQuestLogPreviewEntry> m_questPrevList = new List<UIAtavismQuestLogPreviewEntry>();

        void QuestPreviewEntrySetupNewElement()
        {
            UIAtavismQuestLogPreviewEntry newListEntryLogic = new UIAtavismQuestLogPreviewEntry();
            // Instantiate the UXML template for the entry
            var newListEntry = questPrevEntryTemplate.Instantiate();
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry, questPrevEntryObjectiveTemplate);
            m_list.Add(newListEntry);
            m_questPrevList.Add(newListEntryLogic);
        }

        // Use this for initialization
        void OnEnable()
        {
            uiDocument.enabled = true;
            m_list = uiDocument.rootVisualElement.Q<VisualElement>(listElementName);
            AtavismEventSystem.RegisterEvent("QUEST_LOG_LIST_UPDATE", this);
            AtavismEventSystem.RegisterEvent("QUEST_LOG_UPDATE", this);
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
            //Quests.Instance
            Refresh();
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("QUEST_LOG_LIST_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("QUEST_LOG_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
        }

        void Refresh()
        {
            //int numCells = Quests.Instance.GetSelectedListQuestLog().Count;
            
            //uiDocument.rootVisualElement.childCount
            int k = 0;
            for (int i = 0; i < (Quests.Instance.GetSelectedListQuestLog().Count > maxNumberDisplayElements ? maxNumberDisplayElements : Quests.Instance.GetSelectedListQuestLog().Count); i++)
            {
                if (m_questPrevList.Count <= i)
                {
                   QuestPreviewEntrySetupNewElement();
                }
                
                if (i < Quests.Instance.GetSelectedListQuestLog().Count)
                {
                    m_questPrevList[i].Show();
                    m_questPrevList[i].SetQuestPrev(Quests.Instance.GetSelectedListQuestLog()[i]);
                }
                else
                {
                    m_questPrevList[i].Hide();
                    k++;
                }
            }

            if (m_questPrevList.Count > Quests.Instance.GetSelectedListQuestLog().Count)
            {
                for (int i = Quests.Instance.GetSelectedListQuestLog().Count; i < m_questPrevList.Count; i++)
                {
                    m_questPrevList[i].Hide();
                    k++;
                }
            }
            
            if (k == m_questPrevList.Count)
            {
              uiDocument.rootVisualElement.HideVisualElement();
            }
            else
            {
                uiDocument.rootVisualElement.ShowVisualElement();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "QUEST_LOG_UPDATE" || eData.eventType == "QUEST_LOG_LIST_UPDATE" ||
                eData.eventType == "UPDATE_LANGUAGE")
            {
                Refresh();
            }
        }

        // private void OnEnable()
        // {
        //     AtavismEventSystem.RegisterEvent("QUEST_LOG_UPDATE", this);
        //     AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
        // }
        //
        // private void OnDisable()
        // {
        //     AtavismEventSystem.UnregisterEvent("QUEST_LOG_UPDATE", this);
        //     AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
        // }


    }
}