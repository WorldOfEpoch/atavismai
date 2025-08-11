using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismQuestListEntry //: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        //	public Text questTitleText;
        public Label questTitleText;
        public Toggle questSelectedToggle;
        public Label questLevelText;
        public VisualElement select;
        QuestLogEntry entry;
        int questPos;
        UIAtavismQuestList questList;
        bool selected = false;

        public void SetVisualElement(VisualElement visualElement, UIAtavismQuestList uiAtavismQuestList)
        {
            questTitleText = visualElement.Q<Label>("label");
            questSelectedToggle = visualElement.Q<Toggle>("toggle");
            questSelectedToggle.RegisterValueChangedCallback(ClickQuestListToggle);
            questLevelText = visualElement.Q<Label>("level");
            select = visualElement.Q<VisualElement>("background");
            select.RegisterCallback<ClickEvent>(QuestEntryClicked);
        }

        
        // Use this for initialization
        // void Start()
        // {
        //     AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
        //
        // }
        // private void OnDestroy()
        // {
        //     AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
        //
        // }
        // public void OnPointerEnter(PointerEventData eventData)
        // {
        //     MouseEntered = true;
        //     /*   if (!selected)
        //            select.enabled = true;*/
        // }
        //
        // public void OnPointerExit(PointerEventData eventData)
        // {
        //     MouseEntered = false;
        //     /*  if (!selected)
        //           select.enabled = false;*/
        // }

        public void QuestEntryClicked(ClickEvent evt)
        {
            if (questList.History)
                Quests.Instance.QuestHistoryLogEntrySelected(questPos);
            else
                Quests.Instance.QuestLogEntrySelected(questPos);
            questList.SetQuestDetails();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "UPDATE_LANGUAGE")
            {
                if (this.entry != null)
                    UpdateDisplay();
            }
        }


        public void SetQuestEntryDetails(QuestLogEntry entry, int pos, UIAtavismQuestList questList)
        {
            this.entry = entry;
            this.questPos = pos;
            this.questList = questList;
            UpdateDisplay();

        }

        private void UpdateDisplay()
        {
            if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null &&   (!AtavismSettings.Instance.GetQuestListSelected().ContainsKey(ClientAPI.GetPlayerOid())))
                AtavismSettings.Instance.GetQuestListSelected().Add(ClientAPI.GetPlayerOid(), new List<long>());

            if (select != null)
            {
                QuestLogEntry selectedQuest;
                if (questList.History)
                    selectedQuest = Quests.Instance.GetSelectedQuestHistoryLogEntry();
                else
                    selectedQuest = Quests.Instance.GetSelectedQuestLogEntry();

                if (selectedQuest != null && entry != null)
                    if (selectedQuest.QuestId == entry.QuestId)
                    {
                        select.AddToClassList("quest-selected");
                        selected = true;
                    }
                    else
                    {
                        select.RemoveFromClassList("quest-selected");
                        selected = false;
                    }
            }


            if (questSelectedToggle != null)
            {
                if (questList.History)
                    questSelectedToggle.HideVisualElement();
                else
                    questSelectedToggle.ShowVisualElement();

                if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null &&
                   
                    AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Contains(entry.QuestId.ToLong()))
                    questSelectedToggle.value = true;
                else
                    questSelectedToggle.value = false;
            }
            if (entry.Complete)
            {
#if AT_I2LOC_PRESET
           if (questTitleText!=null)
            this.questTitleText.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + entry.Title) + " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")";
#else
                if (questTitleText != null)
                    this.questTitleText.text = entry.Title + " (Complete)";
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
           if (questTitleText!=null)
            this.questTitleText.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + entry.Title);
#else
                if (questTitleText != null)
                    this.questTitleText.text = entry.Title;
#endif
            }

            if (questLevelText != null)
                this.questLevelText.text = "1";
            // this.questSelectedToggle.isOn = true;
        }

        public void ClickQuestListToggle(ChangeEvent<bool> evt)
        {
            if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null && (!AtavismSettings.Instance.GetQuestListSelected().ContainsKey(ClientAPI.GetPlayerOid())))
                AtavismSettings.Instance.GetQuestListSelected().Add(ClientAPI.GetPlayerOid(), new List<long>());

            if (questSelectedToggle.value)
            {
                if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null && !AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Contains(entry.QuestId.ToLong()))
                {
                    if (AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Count < AtavismSettings.Instance.GetQuestPrevLimit)
                        AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Add(entry.QuestId.ToLong());
                    else
                    {
                        questSelectedToggle.value = false;
                        string[] arg = new string[1];
#if AT_I2LOC_PRESET
                    arg[0] = I2.Loc.LocalizationManager.GetTranslation("QuestPrevLimit") + " " + AtavismSettings.Instance.GetQuestPrevLimit;
#else
                        arg[0] = "Limit selected quests is " + AtavismSettings.Instance.GetQuestPrevLimit;
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", arg);
                        return;
                    }
                }
            }
            else
            {
                if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null && AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Contains(entry.QuestId.ToLong()))
                    AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Remove(entry.QuestId.ToLong());
            }
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_LIST_UPDATE", args);
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        public bool MouseEntered
        {
            set
            {
            }
        }

    }
}