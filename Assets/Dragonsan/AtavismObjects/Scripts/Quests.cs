using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Atavism
{

    public class QuestLogEntry
    {
        public OID QuestId = null;
        public OID NpcId;
        public string Title = "";
        public string Description = "";
        public string Objective = "";
        public int gradeCount = 0;
        public string ProgressText = "";
        public List<QuestGradeEntry> gradeInfo = new List<QuestGradeEntry>();
        public bool Complete = false;
        public int GradeReached = 0;
        public int itemChosen = -1;
        public int reqLeval = 1;
        public string CompleteText = "";

    }

    public class QuestGradeEntry
    {
        public string completionText;
        public List<string> objectives;
        public List<QuestRewardEntry> rewardItems;
        public List<QuestRewardEntry> RewardItemsToChoose;
        public List<QuestRepRewardEntry> rewardRep;
        public int expReward;
        public List<QuestRewardEntry> currencies;
    }

    public class QuestRewardEntry
    {
        public string name = "";
        public int id = -1;
        public int count = 1;
        public AtavismInventoryItem item;
    }

    public class QuestRepRewardEntry
    {
        public string name = "";
        public int count = 1;
    }
    public class Quests : MonoBehaviour
    {

        static Quests instance;

        // info about the last quests we were offered
        //	int questsOfferedSelectedIndex = 0;
        List<QuestLogEntry> questsOffered = new List<QuestLogEntry>();

        // quest log info
        int questLogSelectedIndex = 0;
        List<QuestLogEntry> questLogEntries = new List<QuestLogEntry>();

        // quest progress info
        //	int questProgressSelectedIndex = 0;
        List<QuestLogEntry> questsInProgress = new List<QuestLogEntry>();
        // quest history log info
        int questHistoryLogSelectedIndex = 0;
        List<QuestLogEntry> questHistoryLogEntries = new List<QuestLogEntry>();
        OID npcID;
        public List<long> questListSelected = new List<long>();
        [SerializeField] int maxQuestsSelected = 4;

        private bool questdataloaded = false;
        
        //Queue
        List<Dictionary<string, object>> questMsgQueue = new List<Dictionary<string, object>>();
        List<Dictionary<string, object>> questHistoryMsgQueue = new List<Dictionary<string, object>>();
        
        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_OFFER", _HandleQuestOfferResponse);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_LOG_INFO", _HandleQuestLogInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_HISTORY_LOG_INFO", _HandleQuestHistoryLogInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_STATE_INFO", _HandleQuestStateInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_PROGRESS", _HandleQuestProgressInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.REMOVE_QUEST_RESP", _HandleRemoveQuestResponse);
            NetworkAPI.RegisterExtensionMessageHandler("quest_event", _HandleQuestEvent);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
        }
        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);

        }
        void ClientReady()
        {
            AtavismLogger.LogDebugMessage("Quest ClientReady");
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("Quest", HandleQuestPrefabData);
           
        }

        private void HandleQuestPrefabData(Dictionary<string, object> props)
        {
            //    Debug.LogError("HandleQuestPrefabData "+Time.time);
            int qId = 0;
            try
            {
                int num = (int)props["num"];
                bool sendAll = (bool)props["all"];
                for (int i = 0; i < num; i++)
                {
                    //  Debug.LogError("HandleQuestPrefabData "+i);
                    QuestData qpd = new QuestData();
                    qId = (int)props["i" + i + "id"];
                    qpd.id = qId;
                    qpd.title = (string)props["i" + i + "name"];
                    qpd.description = (string)props["i" + i + "description"];
                    qpd.objective = (string)props["i" + i + "objectiveText"];
                    qpd.progress = (string)props["i" + i + "progressText"];

                    int objectives = (int)props["i" + i + "objectives"];
                    for (int j = 0; j < objectives; j++)
                    {
                        QuestObjectiveData qda = new QuestObjectiveData();
                        qda.type = (string)props["i" + i + "objectiveT" + j];
                        qda.name = (string)props["i" + i + "objectiveTe" + j];
                        qda.count = (int)props["i" + i + "objectiveC" + j];
                        qpd.objectives.AddLast(qda);
                    }

                    int numGrades = (int)props["i" + i + "numGrades"];
                    for (int j = 0; j < numGrades; j++)
                    {
                        QuestRewardGradeData qrgd = new QuestRewardGradeData();
                        qrgd.exp = (int)props["i" + i + "experience" + j];
                        qrgd.completeText = (string)props["i" + i + "completionText" + j];

                        int currencies = (int)props["i" + i + "currencies" + j];
                        for (int k = 0; k < currencies; k++)
                        {
                            int currency = (int)props["i" + i + "currency" + j + "_" + k];
                            int currencyCount = (int)props["i" + i + "currencyC" + j + "_" + k];
                            qrgd.rewardsCurrency.Add(currency, currencyCount);
                        }

                        int rewards = (int)props["i" + i + "rewards" + j];
                        for (int k = 0; k < rewards; k++)
                        {
                            int item = (int)props["i" + i + "reward" + j + "_" + k];
                            int itemCount = (int)props["i" + i + "rewardc" + j + "_" + k];
                            qrgd.rewardsItems.Add(item, itemCount);
                        }

                        int rewardsC = (int)props["i" + i + "rewardsC" + j];
                        for (int k = 0; k < rewardsC; k++)
                        {
                            int item = (int)props["i" + i + "rewardC" + j + "_" + k];
                            int itemCount = (int)props["i" + i + "rewardCc" + j + "_" + k];
                            qrgd.rewardsToChooseItems.Add(item, itemCount);
                        }

                        int factions = (int)props["i" + i + "factions" + j];
                        for (int k = 0; k < factions; k++)
                        {
                            string faction = (string)props["i" + i + "faction" + j + "_" + k];
                            int reputationCount = (int)props["i" + i + "factionR" + j + "_" + k];
                            qrgd.rewardsReputation.Add(faction, reputationCount);
                        }




                        qpd.rewardsGrades.Add(j, qrgd);
                    }

                    qpd.date = (long)props["i" + i + "date"];

                    AtavismPrefabManager.Instance.SaveQuest(qpd);
                }

                if (props.ContainsKey("toRemove"))
                {
                    string keys = (string)props["toRemove"];
                    if (keys.Length > 0)
                    {
                        string[] _keys = keys.Split(';');
                        foreach (string k in _keys)
                        {
                            if (k.Length > 0)
                            {
                                AtavismPrefabManager.Instance.DeleteQuest(int.Parse(k));
                            }
                        }
                    }
                }

                if (sendAll)
                {
                    //  Debug.LogError("Quest prefab loaded");
                    questdataloaded = true;

                    foreach (Dictionary<string, object> qProps in questMsgQueue)
                    {
                        //      Debug.LogError("Running Queued Quest update message");
                        _HandleQuestProgressInfo(qProps);
                    }

                    foreach (Dictionary<string, object> qProps in questHistoryMsgQueue)
                    {
                        //      Debug.LogError("Running Queued Quest update message");
                        _HandleQuestHistoryLogInfo(qProps);
                    }

                    //string[] args = new string[1];
                    //AtavismEventSystem.DispatchEvent("ITEM_RELOAD", args);

                    if (AtavismLogger.logLevel <= LogLevel.Debug)
                        Debug.Log("All data received. Running Queued Quest update message.");
                    AtavismPrefabManager.Instance.reloaded++;
                }
                else
                {
                    AtavismPrefabManager.Instance.LoadQuestPrefabData();
                    Debug.LogWarning("Not Running Queued Quest update message. Not all data sent");
                }
            }
            catch (System.Exception e)
            {
                AtavismLogger.LogError("Exception loading quest prefab data id=" + qId + " Exception:" + e);
            }
            //   Debug.LogError("HandleQuestPrefabData End");
        }

        public void OnEvent(AtavismEventData eData)
        {
         if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                foreach (QuestLogEntry qle in questsOffered)
                {
                    //qle.gradeInfo[0].RewardItemsToChoose[i]
                    foreach (QuestGradeEntry qge in qle.gradeInfo)
                    {
                        // foreach (QuestRewardEntry qre in qge.rewardItems)
                        // {
                        //     if(qre.item!=null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                        // foreach (QuestRewardEntry qre in qge.RewardItemsToChoose)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                    }
                }
                foreach (QuestLogEntry qle in questLogEntries)
                {
                    //qle.gradeInfo[0].RewardItemsToChoose[i]
                    foreach (QuestGradeEntry qge in qle.gradeInfo)
                    {
                        // foreach (QuestRewardEntry qre in qge.rewardItems)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                        // foreach (QuestRewardEntry qre in qge.RewardItemsToChoose)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                    }
                }
                foreach (QuestLogEntry qle in questsInProgress)
                {
                    //qle.gradeInfo[0].RewardItemsToChoose[i]
                    if (qle != null)
                        foreach (QuestGradeEntry qge in qle.gradeInfo)
                    {
                        // foreach (QuestRewardEntry qre in qge.rewardItems)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                        // foreach (QuestRewardEntry qre in qge.RewardItemsToChoose)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                    }
                }
                foreach (QuestLogEntry qle in questHistoryLogEntries)
                {
                    //qle.gradeInfo[0].RewardItemsToChoose[i]
                    if(qle != null)
                    foreach (QuestGradeEntry qge in qle.gradeInfo)
                    {
                        // foreach (QuestRewardEntry qre in qge.rewardItems)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                        // foreach (QuestRewardEntry qre in qge.RewardItemsToChoose)
                        // {
                        //     if (qre.item != null)
                        //         qre.item.icon = AtavismPrefabManager.Instance.GetItemIconByID(qre.item.templateId);
                        // }
                    }
                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("QUEST_ITEM_UPDATE", args);

            }
        }




        public QuestLogEntry GetQuestOfferedInfo(int pos)
        {
            return questsOffered[pos];
        }

        public void AcceptQuest(int questPos)
        {
            NetworkAPI.SendQuestResponseMessage(npcID.ToLong(), questsOffered[questPos].QuestId.ToLong(), true);
        }

        public void DeclineQuest(int questPos)
        {
            NetworkAPI.SendQuestResponseMessage(npcID.ToLong(), questsOffered[questPos].QuestId.ToLong(), false);
        }

        public void QuestLogEntrySelected(int pos)
        {
            questLogSelectedIndex = pos;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);

        }
        public void QuestHistoryLogEntrySelected(int pos)
        {
            questHistoryLogSelectedIndex = pos;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
        }
        public List<QuestLogEntry> GetSelectedListQuestLog()
        {
            List<QuestLogEntry> questsList = new List<QuestLogEntry>();
           
            if (!AtavismSettings.Instance.GetQuestListSelected().ContainsKey(ClientAPI.GetPlayerOid()))
                AtavismSettings.Instance.GetQuestListSelected().Add(ClientAPI.GetPlayerOid(), new List<long>());
            foreach (QuestLogEntry q in questLogEntries)
            {
                if (AtavismSettings.Instance != null  && AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Contains(q.QuestId.ToLong()))
                    questsList.Add(q);
            }
            return questsList;
        }

        public QuestLogEntry GetSelectedQuestLogEntry()
        {
            if (questLogEntries.Count - 1 < questLogSelectedIndex)
                return null;
            if (questLogSelectedIndex == -1)
                return null;
            return questLogEntries[questLogSelectedIndex];
        }
        /// <summary>
        ///  Function return selected historical quest
        /// </summary>
        /// <returns></returns>
        public QuestLogEntry GetSelectedQuestHistoryLogEntry()
        {
            if (questHistoryLogEntries.Count - 1 < questHistoryLogSelectedIndex)
                return null;
            if (questHistoryLogSelectedIndex == -1)
                return null;
            return questHistoryLogEntries[questHistoryLogSelectedIndex];
        }

        public void AbandonQuest()
        {
            if (questLogSelectedIndex == -1 || questLogSelectedIndex > questLogEntries.Count)
                return;
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/abandonQuest " + questLogEntries[questLogSelectedIndex].QuestId);
            UpdateQuestListSelected();
        }

        public QuestLogEntry GetQuestProgressInfo(int pos)
        {
            if (questsInProgress.Count > pos && pos >= 0)
            {
                return questsInProgress[pos];
            }
            else
            {
                return null;
            }
        }

        public bool CompleteQuest()
        {
            QuestLogEntry quest = questsInProgress[0];
            /*Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("senderOid", ClientAPI.GetPlayerOid());
            props.Add("questNPC", quest.NpcId);
            props.Add("questOID", quest.QuestId);
            props.Add("reward", rewardID);
            NetworkAPI.SendExtensionMessage(quest.NpcId.ToLong(), false, "ao.COMPLETE_QUEST", props);*/

            if (quest.gradeInfo[0].RewardItemsToChoose.Count > 0 && quest.itemChosen == -1)
                return false;

            NetworkAPI.SendTargetedCommand(quest.NpcId.ToLong(), "/completeQuest " + quest.QuestId + " " + quest.itemChosen);
            UpdateQuestListSelected();
            return true;
        }

        void _HandleQuestOfferResponse(Dictionary<string, object> props)
        {
            // update the information about the quests on offer from this npc
            questsOffered.Clear();
            int numQuests = (int)props["numQuests"];
            npcID = (OID)props["npcID"];
            for (int i = 0; i < numQuests; i++)
            {
                QuestLogEntry logEntry = new QuestLogEntry();
                questsOffered.Add(logEntry);
                
                int id = (int)props["qId"+i];
                var questDefinition = AtavismPrefabManager.Instance.GetQuestByID(id);

                logEntry.Title = questDefinition.title;
                //logEntry.Title = (string)props["title" + i];
                logEntry.QuestId = (OID)props["questID" + i];
                logEntry.NpcId = npcID;
                logEntry.Description = questDefinition.description;
                //logEntry.Description = (string)props["description" + i];
                logEntry.Objective = questDefinition.objective;
                //logEntry.Objective = (string)props["objective" + i];
                //logEntry.Objectives.Clear ();
                //LinkedList<string> objectives = (LinkedList<string>)props ["objectives"];
                //foreach (string objective in objectives)
                //	logEntry.Objectives.Add (objective);
                  logEntry.gradeCount = questDefinition.rewardsGrades.Count-1;
//                logEntry.gradeCount = (int)props["grades"+i];
                foreach (var grade in questDefinition.rewardsGrades.Values)
                {
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                    foreach (var reward in grade.rewardsItems)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = reward.Value;
                        gradeRewards.Add(entry);
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    foreach (var reward in grade.rewardsToChooseItems)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = reward.Value;
                        gradeRewardsToChoose.Add(entry);
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    foreach (var reward in grade.rewardsReputation)
                    {
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = reward.Key;
                        entry.count = reward.Value;
                        gradeRepReward.Add(entry);
                    }
                    gradeEntry.rewardRep = gradeRepReward;
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    foreach (var reward in grade.rewardsCurrency)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        entry.count = reward.Value;
                        currencies.Add(entry);
                    }

                    gradeEntry.currencies = currencies;
                    
                    gradeEntry.expReward = grade.exp;
                    gradeEntry.completionText = grade.completeText;
                    logEntry.gradeInfo.Add(gradeEntry);
                }

                
              /*
              //  logEntry.gradeInfo = new List<QuestGradeEntry>();
                //ClientAPI.Write("Quest grades: %s" % logEntry.grades)
                for (int j = 0; j < (logEntry.gradeCount + 1); j++)
                {
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();

                    int numRewards = (int)props["rewards" + i + " " + j];
                    //               Debug.LogError("Quest " + logEntry.Title + " rewards count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewards" + i + "_" + j + "_" + k];
                        
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewards" + i + "_" + j + "_" + k + "Count"];
                        gradeRewards.Add(entry);
                        //ClientAPI.Write("Reward: %s" % entry)
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    numRewards = (int)props["rewardsToChoose" + i + " " + j];
                    //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewardsToChoose" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewardsToChoose" + i + "_" + j + "_" + k + "Count"];
                        gradeRewardsToChoose.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;

                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    numRewards = (int)props["rewardsRep" + i + " " + j];
                    //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = (string)props["rewardsRep" + i + "_" + j + "_" + k];
                        entry.count= (int)props["rewardsRep" + i + "_" + j + "_" + k + "Count"];
                        gradeRepReward.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.rewardRep = gradeRepReward;




                    // Quest Exp
                    gradeEntry.expReward = (int)props["xpReward" + i + " " + j];
                    // Currencies
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    numRewards = (int)props["currencies" + i + " " + j];
                    for (int k = 0; k < numRewards; k++)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["currency" + i + "_" + j + "_" + k];
                        entry.count = (int)props["currency" + i + "_" + j + "_" + k + "Count"];
                        currencies.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.currencies = currencies;
                    logEntry.gradeInfo.Add(gradeEntry);
                }
                */
                
            }
            // dispatch a ui event to tell the rest of the system
            if (gameObject.GetComponent<NpcInteraction>().NpcId != npcID)
                gameObject.GetComponent<NpcInteraction>().InteractionOptions.Clear();
            gameObject.GetComponent<NpcInteraction>().NpcId = npcID;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_OFFERED_UPDATE", args);
        }

        /// <summary>
        /// Handles the Quest Log Update message, which has information about the current status 
        /// of Quests that the player is on.
        /// </summary>
        /// <param name="props">Properties.</param>
        void _HandleQuestLogInfo(Dictionary<string, object> props)
        {
            // update our idea of the state
            QuestLogEntry logEntry = null;
            long quest_id = (long)props["ext_msg_subject_oid"];
            OID questID = OID.fromLong(quest_id);
            foreach (QuestLogEntry entry in questLogEntries)
            {
                if (entry.QuestId.Equals(questID))
                {
                    logEntry = entry;
                    break;
                }
            }
            if (logEntry == null)
            {
                logEntry = new QuestLogEntry();
                questLogEntries.Add(logEntry);
            }
            logEntry.QuestId = questID;
            
            int id = (int)props["qId"];
            var questDefinition = AtavismPrefabManager.Instance.GetQuestByID(id);

            logEntry.Title = questDefinition.title;
            //logEntry.Title = (string)props["title"];
            logEntry.Description = questDefinition.description;
            //logEntry.Description = (string)props["description"];
            logEntry.Objective = questDefinition.objective;
            //logEntry.Objective = (string)props["objective"];
            logEntry.Complete = (bool)props["complete"];

            logEntry.gradeCount = questDefinition.rewardsGrades.Count-1;
//                logEntry.gradeCount = (int)props["grades"];
                foreach (var grade in questDefinition.rewardsGrades)
                {
                    
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    
                    List<string> objectives = new List<string>();
                    int numObjectives = (int)props["numObjectives" + grade.Key];
                    for (int k = 0; k < numObjectives; k++)
                    {
                        string objective = (string)props["objective" + 0 + "_" + k];
                        objectives.Add(objective);
                    }
                    gradeEntry.objectives = objectives;
                    
                    
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                    foreach (var reward in grade.Value.rewardsItems)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = reward.Value;
                        gradeRewards.Add(entry);
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    foreach (var reward in grade.Value.rewardsToChooseItems)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = reward.Value;
                        gradeRewardsToChoose.Add(entry);
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    foreach (var reward in grade.Value.rewardsReputation)
                    {
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = reward.Key;
                        entry.count = reward.Value;
                        gradeRepReward.Add(entry);
                    }
                    gradeEntry.rewardRep = gradeRepReward;
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    foreach (var reward in grade.Value.rewardsCurrency)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        entry.count = reward.Value;
                        currencies.Add(entry);
                    }

                    gradeEntry.currencies = currencies;
                    
                    gradeEntry.expReward = grade.Value.exp;
                    gradeEntry.completionText = grade.Value.completeText;
                    logEntry.gradeInfo.Add(gradeEntry);
                }

                
                
           /*
           // logEntry.gradeInfo = new List<QuestGradeEntry>();
            for (int j = 0; j < (logEntry.gradeCount + 1); j++)
            {
                QuestGradeEntry gradeEntry = new QuestGradeEntry();
                // Objectives
                List<string> objectives = new List<string>();
                int numObjectives = (int)props["numObjectives" + j];
                for (int k = 0; k < numObjectives; k++)
                {
                    string objective = (string)props["objective" + j + "_" + k];
                    objectives.Add(objective);
                }
                gradeEntry.objectives = objectives;

                // Rewards
                List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                int numRewards = (int)props["rewards" + j];
                //           Debug.LogError("QuestLog " + logEntry.Title + " rewards count:" + numRewards);
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRewardEntry entry = new QuestRewardEntry();
                    entry.id = (int)props["rewards" + j + "_" + k];
                    AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                    entry.item = item;
                    entry.item.Count = (int)props["rewards" + j + "_" + k + "Count"];
                    gradeRewards.Add(entry);
                    //ClientAPI.Write("Reward: %s" % entry)
                }
                gradeEntry.rewardItems = gradeRewards;
                // Items to choose from
                List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                numRewards = (int)props["rewardsToChoose" + j];
                //          Debug.LogError("QuestLog " + logEntry.Title + " rewards Choose count:" + numRewards);
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRewardEntry entry = new QuestRewardEntry();
                    entry.id = (int)props["rewardsToChoose" + j + "_" + k];
                    AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                    entry.item = item;
                    entry.item.Count = (int)props["rewardsToChoose" + j + "_" + k + "Count"];
                    gradeRewardsToChoose.Add(entry);
                    //ClientAPI.Write("Reward: %s" % entry)
                }
                gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                numRewards = (int)props["rewardsRep" + j];
                //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRepRewardEntry entry = new QuestRepRewardEntry();
                    entry.name = (string)props["rewardsRep" + j + "_" + k];
                    entry.count = (int)props["rewardsRep" + j + "_" + k + "Count"];
                    gradeRepReward.Add(entry);
                    //ClientAPI.Write("Reward to choose: %s" % entry)
                }
                gradeEntry.rewardRep = gradeRepReward;


                gradeEntry.expReward = (int)props["xpReward" + j];
                // Currencies
                List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                numRewards = (int)props["currencies" + j];
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRewardEntry entry = new QuestRewardEntry();
                    entry.id = (int)props["currency" + j + "_" + k];
                    entry.count = (int)props["currency" + j + "_" + k + "Count"];
                    currencies.Add(entry);
                    //ClientAPI.Write("Reward: %s" % entry)
                }
                gradeEntry.currencies = currencies;
                logEntry.gradeInfo.Add(gradeEntry);
            }
            */
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
            UpdateQuestListSelected();

        }

        /// <summary>
        /// Handles the Quest History Log Update message, which has information about the Historicaly Quests.
        /// </summary>
        /// <param name="props">Properties.</param>
        void _HandleQuestHistoryLogInfo(Dictionary<string, object> props)
        {
            if (!questdataloaded)
            {
                questHistoryMsgQueue.Add(props);
                Debug.LogWarning("_HandleQuestHistoryLogInfo quest definition not loaded add queue");
                return;
            }
            
            questHistoryLogEntries.Clear();
            for (int ii = 0; ii < (int)props["numQuests"]; ii++)
            {
                QuestLogEntry logEntry = new QuestLogEntry();
                long qId = (long)props["questId" + ii];
                OID questId = OID.fromLong(qId);
                logEntry.QuestId = questId;
                int id = (int)props["qId" + ii];
                var questDefinition = AtavismPrefabManager.Instance.GetQuestByID(id);
                logEntry.Title = questDefinition.title;
                //logEntry.Title = (string)props["title" + ii];
                logEntry.Description = questDefinition.description;
                //logEntry.Description = (string)props["description" + ii];
                logEntry.Objective = questDefinition.objective;
                //logEntry.Objective = (string)props["objective" + ii];
                logEntry.CompleteText = questDefinition.rewardsGrades[0].completeText;
                //logEntry.CompleteText = (string)props["complete" + ii];
                logEntry.reqLeval = (int)props["level" + ii];
                questHistoryLogEntries.Add(logEntry);
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_HISTORY_LOG_UPDATE", args);
        }



        /// <summary>
        /// Handles the updates of the Quest State and updates the objectives in the players Quest Log
        /// to match.
        /// </summary>
        /// <param name="props">Properties.</param>
        void _HandleQuestStateInfo(Dictionary<string, object> props)
        {
            long quest_id = (long)props["ext_msg_subject_oid"];
            OID questID = OID.fromLong(quest_id);
            // foreach (var prop in props)
            // {
            //     Debug.LogWarning(prop.Key);
            // }
            // update our idea of the state
            foreach (QuestLogEntry entry in questLogEntries)
            {
                if (!entry.QuestId.Equals(questID))
                    continue;
                for (int j = 0; j < (entry.gradeCount + 1); j++)
                {
                    // Objectives
                    List<string> objectives = new List<string>();
                    int numObjectives = (int)props["numObjectives" + j];
                    for (int k = 0; k < numObjectives; k++)
                    {
                        string objective = (string)props["objective" + j + "_" + k];
                        objectives.Add(objective);
                    }
                    entry.gradeInfo[j].objectives = objectives;
                }
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
        }

        void _HandleQuestProgressInfo(Dictionary<string, object> props)
        {
            if (!questdataloaded)
            {
                questMsgQueue.Add(props);
                Debug.LogWarning("_HandleQuestProgressInfo quest definition not loaded add queue");
                return;
            }

           /// update the information about the quests in progress from this npc
           //  Debug.LogError("_HandleQuestProgressInfo ");
            questsInProgress.Clear();
            int numQuests = (int)props["numQuests"];
            npcID = (OID)props["npcID"];
            for (int i = 0; i < numQuests; i++)
            {
                QuestLogEntry logEntry = new QuestLogEntry();
                questsInProgress.Add(logEntry);
                logEntry.Title = (string)props["title" + i];
                logEntry.QuestId = (OID)props["questID" + i];
                logEntry.NpcId = npcID;
                int id = (int)props["qId" + i];
                var questDefinition = AtavismPrefabManager.Instance.GetQuestByID(id);
                //logEntry.Description = (string)props ["description" + i];
                logEntry.ProgressText = questDefinition.progress;
                //logEntry.ProgressText = (string)props["progress" + i];
                logEntry.Complete = (bool)props["complete" + i];
                logEntry.Objective = questDefinition.objective;
                //logEntry.Objective = (string)props["objective" + i];
                logEntry.gradeCount = questDefinition.rewardsGrades.Count-1;
//                logEntry.gradeCount = (int)props["grades" + i];
                foreach (var grade in questDefinition.rewardsGrades.Values)
                {
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                    foreach (var reward in grade.rewardsItems)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = reward.Value;
                        gradeRewards.Add(entry);
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    foreach (var reward in grade.rewardsToChooseItems)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = reward.Value;
                        gradeRewardsToChoose.Add(entry);
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    foreach (var reward in grade.rewardsReputation)
                    {
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = reward.Key;
                        entry.count = reward.Value;
                        gradeRepReward.Add(entry);
                    }
                    gradeEntry.rewardRep = gradeRepReward;
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    foreach (var reward in grade.rewardsCurrency)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = reward.Key;
                        entry.count = reward.Value;
                        currencies.Add(entry);
                    }

                    gradeEntry.currencies = currencies;
                    
                    gradeEntry.expReward = grade.exp;
                    gradeEntry.completionText = grade.completeText;
                    logEntry.gradeInfo.Add(gradeEntry);
                }



                
                /*
                // logEntry.gradeInfo = new List<QuestGradeEntry>();
                //ClientAPI.Write("Quest grades: %s" % logEntry.grades)
                for (int j = 0; j < (logEntry.gradeCount + 1); j++)
                {
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                    int numRewards = (int)props["rewards" + i + " " + j];
                   // Debug.LogError("QuestProgress rewards " + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewards" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewards" + i + "_" + j + "_" + k + "Count"];
                       // Debug.LogError("QuestProgress rewards " + entry.id + " "+ entry.item.Count);

                        gradeRewards.Add(entry);
                        //ClientAPI.Write("Reward: %s" % entry)
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    numRewards = (int)props["rewardsToChoose" + i + " " + j];
                   // Debug.LogError("QuestProgress rewardsToChoose " + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewardsToChoose" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewardsToChoose" + i + "_" + j + "_" + k + "Count"];
                        gradeRewardsToChoose.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    numRewards = (int)props["rewardsRep" + i + " " + j];
                  //  Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = (string)props["rewardsRep" + i + "_" + j + "_" + k];
                        entry.count = (int)props["rewardsRep" + i + "_" + j + "_" + k + "Count"];
                        gradeRepReward.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.rewardRep = gradeRepReward;

                    if (props.ContainsKey("xpReward" + i + " " + j))
                    {
                            //              Debug.LogError("Quest Progress xpReward" + i + " " + j + " ->" + props["xpReward" + i + " " + j]);
                        gradeEntry.expReward = (int)props["xpReward" + i + " " + j];
                    }
                    else
                        Debug.LogWarning("Quest Progress no xpReward");

                    // Currencies
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    numRewards = (int)props["currencies" + i + " " + j];
                  //  Debug.LogError("QuestProgress currencies " + numRewards);

                    for (int k = 0; k < numRewards; k++)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["currency" + i + "_" + j + "_" + k];
                        entry.count = (int)props["currency" + i + "_" + j + "_" + k + "Count"];
                        currencies.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.currencies = currencies;
                    gradeEntry.completionText = (string)props["completion" + i + "_" + j];
                    logEntry.gradeInfo.Add(gradeEntry);
                }
                */
                
            }
            //
            // dispatch a ui event to tell the rest of the system
            //
            if (gameObject.GetComponent<NpcInteraction>().NpcId != npcID)
                gameObject.GetComponent<NpcInteraction>().InteractionOptions.Clear();

            gameObject.GetComponent<NpcInteraction>().NpcId = npcID;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_PROGRESS_UPDATE", args);
            UpdateQuestListSelected();
          //  Debug.LogError("_HandleQuestProgressInfo End");
        }

        void _HandleRemoveQuestResponse(Dictionary<string, object> props)
        {
            int index = 1; // questLogSelectedIndex is 1 based.
            long quest_id = (long)props["ext_msg_subject_oid"];
            OID questID = OID.fromLong(quest_id);
            foreach (QuestLogEntry entry in questLogEntries)
            {
                if (entry.QuestId.Equals(questID))
                {
                    questLogEntries.Remove(entry);
                    break;
                }
                index++;
            }
            if (index == questLogSelectedIndex)
            {
                // we removed the selected entry. reset selection
                questLogSelectedIndex = 0;
            }
            else if (index < questLogSelectedIndex)
            {
                // removed an entry before our selection - decrement our selection
                questLogSelectedIndex--;
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
            UpdateQuestListSelected();
        }

        public void _HandleQuestEvent(Dictionary<string, object> props)
        {
            string eventType = (string)props["event"];
            int val1 = (int)props["val1"];
            int val2 = (int)props["val2"];
            int val3 = (int)props["val3"];
            string data = (string)props["data"];

         //   Debug.LogError("Quest Event " + data + " " + val1 + " " + val2 + " " + val3 + " " + eventType);
           
#if AT_I2LOC_PRESET
        if (data.IndexOf(" killed:") != -1) {
            string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + data.Remove(data.IndexOf(" killed:")));
            //    string objectivesValues = data.Remove(0, data.LastIndexOf(':') < 0 ? 0 : data.LastIndexOf(':')+1);
            data = objectivesNames + " " + I2.Loc.LocalizationManager.GetTranslation("killed") + ": " + val2 + "/" + val3 + (val2 == val3 ? " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")" : "");
        }
        if (data.IndexOf(" collected:") != -1) {
            string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + data.Remove(data.IndexOf(" collected:")));
            //    string objectivesValues = data.Remove(0, data.LastIndexOf(':') < 0 ? 0 : data.LastIndexOf(':')+1);
            data = objectivesNames + " " + I2.Loc.LocalizationManager.GetTranslation("collected") + ": " + val2 + "/" + val3 + (val2 == val3 ? " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")" : "");
        }
#endif
            //string errorMessage = eventType;
            if (eventType == "QuestProgress")
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
                args[0] = data;
                AtavismEventSystem.DispatchEvent("ANNOUNCEMENT", args);
            }
        }

        #region Properties
        public static Quests Instance
        {
            get
            {
                return instance;
            }
        }

        public List<QuestLogEntry> QuestLogEntries
        {
            get
            {
                return questLogEntries;
            }
        }
        public List<QuestLogEntry> QuestHistoryLogEntries
        {
            get
            {
                return questHistoryLogEntries;
            }
        }
        #endregion Properties


        //  List<GameObject> qObjects = new List<GameObject>();
        OID questOnMinimap;

        public void ClickedQuest(QuestLogEntry quest)
        {
            /*   Scene aScene = SceneManager.GetActiveScene();
               List<string> teleports = new List<string>();
               string[] cords;
               GameObject obj;
               string qCordsObjects = "";
               foreach (GameObject qo in qObjects) {
                   Destroy(qo);
               }
               questOnMinimap = quest.QuestId;
               qObjects.Clear();
               for (int i = 0; i < quest.gradeInfo[0].objectives.Count; i++) {
                   qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + quest.Title + i);
                   //   Debug.LogError(qCordsObjects);
                   if (!string.IsNullOrEmpty(qCordsObjects)) {
                       string[] cLoc = qCordsObjects.Split('&');
                       for (int ii = 0; ii < cLoc.Length; ii++) {
                           cords = cLoc[ii].Split('|');
                           if (aScene.name.Equals(cords[0])) {
                               obj = new GameObject(quest.QuestId + "Obj" + i + "_" + ii);
                               obj.transform.localPosition = new Vector3(float.Parse(cords[1]), float.Parse(cords[2]), float.Parse(cords[3]));
                               bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                               mmi.Target = obj.transform;
                               mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestMobArea;
                               mmi.IconColor = Color.blue;
                               mmi.Size = cords.Length > 4 ? float.Parse(cords[4]) : 35;
                               mmi.InfoItem = "";
                               qObjects.Add(obj);
                           } else if (teleports.IndexOf(aScene.name + "_" + cords[0]) == -1) {
                               qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + aScene.name + "_" + cords[0]);
                               if (!string.IsNullOrEmpty(qCordsObjects)) {
                                   string[] cordsTelep = qCordsObjects.Split('|');
                                   obj = new GameObject(quest.QuestId + "Obj" + i + "_" + ii);
                                   obj.transform.localPosition = new Vector3(float.Parse(cordsTelep[0]), float.Parse(cordsTelep[1]), float.Parse(cordsTelep[2]));
                                   bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                                   mmi.Target = obj.transform;
                                   mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestTarget;
                                   mmi.IconColor = Color.blue;
                                   mmi.Size = 20;
                                   mmi.InfoItem = "";
                                   qObjects.Add(obj);
                                   teleports.Add(aScene.name + "_" + cords[0]);
                               } else {
                                   Debug.LogError("No Cords for " + aScene.name + "_" + cords[0]);
                               }
                           }
                       }
                   } else {
                       Debug.LogError("No Cords for " + quest.Title + i);
                   }
               }
               qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + quest.Title);
               if (!string.IsNullOrEmpty(qCordsObjects)) {
                   cords = qCordsObjects.Split('|');
                   obj = new GameObject(quest.QuestId + "Target");
                   if (aScene.name.Equals(cords[0])) {
                       obj.transform.localPosition = new Vector3(float.Parse(cords[1]), float.Parse(cords[2]), float.Parse(cords[3]));
                       bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                       mmi.Target = obj.transform;
                       if (cords.Length > 4 && !string.IsNullOrEmpty(cords[4])) {
                           mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestMobArea;
                           mmi.Size = float.Parse(cords[4]);

                       } else {
                           mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestTarget;
                           mmi.Size = 20;
                       }
                       mmi.IconColor = Color.blue;
                       mmi.InfoItem = "";
                       qObjects.Add(obj);
                   } else if (teleports.IndexOf(aScene.name + "_" + cords[0]) == -1) {
                       qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + aScene.name + "_" + cords[0]);
                       if (!string.IsNullOrEmpty(qCordsObjects)) {
                           string[] cordsTelep = qCordsObjects.Split('|');
                           obj.transform.localPosition = new Vector3(float.Parse(cordsTelep[0]), float.Parse(cordsTelep[1]), float.Parse(cordsTelep[2]));
                           bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                           mmi.Target = obj.transform;
                           mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestTarget;
                           mmi.IconColor = Color.blue;
                           mmi.Size = 20;
                           mmi.InfoItem = "";
                           qObjects.Add(obj);
                           teleports.Add(aScene.name + "_" + cords[0]);
                       } else {
                           Debug.LogError("No Cords for " + aScene.name + "_" + cords[0]);
                       }
                   }
               } else {
                   Debug.LogError("No Cords for " + quest.Title);
               }
               */
        }


        void UpdateQuestListSelected()
        {
            bool jestQ;
            //     bool isQuestOnMinimap = false;
        
            List<long> toRemove = new List<long>();
            if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null )
            {
                if (!AtavismSettings.Instance.GetQuestListSelected().ContainsKey(ClientAPI.GetPlayerOid()))
                    AtavismSettings.Instance.GetQuestListSelected().Add(ClientAPI.GetPlayerOid(), new List<long>());

                foreach (long id in AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()])
                {
                    jestQ = false;
                    foreach (QuestLogEntry q in questLogEntries)
                    {
                        if (q.QuestId.ToLong() == id)
                        {
                            jestQ = true;
                            break;
                        }
                    }
                    if (!jestQ)
                        toRemove.Add(id);
                }
                foreach (long id in toRemove)
                {
                    AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Remove(id);
                }
            }
            /*
                    foreach (QuestLogEntry q in questLogEntries) {
                        if (q.QuestId.Equals(questOnMinimap))
                            isQuestOnMinimap = true;
                    }
                    Debug.LogError("isQuestOnMinimap->" + isQuestOnMinimap);
                    if (!isQuestOnMinimap) {
                        foreach (GameObject qo in qObjects) {
                            Destroy(qo);
                        }
                        qObjects.Clear();
                    }
                    */
        }
        public int GetMaxQuestsSelected
        {
            get
            {
                return maxQuestsSelected;
            }
        }




    }
}