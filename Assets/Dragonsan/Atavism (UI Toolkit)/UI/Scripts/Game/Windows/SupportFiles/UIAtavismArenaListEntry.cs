using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Atavism.UI
{
    public class UIAtavismArenaListEntry 
    {
        public Label arenaTitleText;
        public Label arenaLevelText;
        public Label arenaTimeText;
        public VisualElement backgroud;
        public VisualElement queue;
        // public VisualElement hover;
        // [SerializeField] Color normalColorText = Color.black;
        // [SerializeField] Color reqFailedColorText = Color.red;

        ArenaEntry entry;
        // int arenaPos;
        // Assuming there is a UIAtavismArenaList class
        // UIAtavismArenaList arenaList;

        // UIDocument uiDocument;

       

        public void SetVisualElement(VisualElement root)
        {
            backgroud = root.Q<VisualElement>();

            backgroud.RegisterCallback<MouseOverEvent>(e => OnPointerEnter());
            backgroud.RegisterCallback<MouseOutEvent>(e => OnPointerExit());
            arenaTitleText = root.Q<Label>("name");
            arenaLevelText = root.Q<Label>("level");
            arenaTimeText = root.Q<Label>("time");
            queue = root.Q<VisualElement>("queue");

        }

        public void SetData(ArenaEntry entry)
        {
            this.entry = entry;
            UpdateDisplay();
        }

        void OnPointerEnter()
        {
            if (backgroud != null)
                backgroud.AddToClassList("arena-entry-hover");
        }

        void OnPointerExit()
        {
            if (backgroud != null)
                backgroud.RemoveFromClassList("arena-entry-hover");
        }

        // public void ArenaEntryClicked()
        // {
        //     Arena.Instance.ArenaEntrySelected(arenaPos);
        //     arenaList.SetArenaDetails();
        // }

        // public void SetArenaEntryDetails(ArenaEntry entry, int pos, UIAtavismArenaList arenaList)
        // {
        //     this.entry = entry;
        //     // this.arenaPos = pos;
        //     UpdateDisplay();
        // }

        public void UpdateDisplay()
        {
            if (entry == null)
                return;
            ArenaEntry selectedArena = Arena.Instance.GetSelectedArenaEntry();
            if (selectedArena != null)
            {
                if (selectedArena.ArenaId == entry.ArenaId)
                {
                    backgroud.AddToClassList("arena-entry-selected");
                }
                else
                {
                    backgroud.RemoveFromClassList("arena-entry-selected");
                }
            }

            if (entry.ArenaQueued)
            {
              //  Debug.LogWarning("Arena "+entry.ArenaName+" Queued");
                queue.visible = true;
            }
            else
            {
               // Debug.LogWarning("Arena "+entry.ArenaName+" not Queued");
                queue.visible =false;
            }

            DateTime arenaStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, entry.StartHour, entry.StartMin, 0);
            DateTime arenaEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, entry.EndHour, entry.EndMin, 0);
            DateTime timeNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TimeManager.Instance.Hour, TimeManager.Instance.Minute, 0);

            this.arenaTitleText.text = entry.ArenaName;
            this.arenaTimeText.text = $"{entry.StartHour}:{(entry.StartMin < 10 ? "0" + entry.StartMin : entry.StartMin.ToString())} - {entry.EndHour}:{(entry.EndMin < 10 ? "0" + entry.EndMin : entry.EndMin.ToString())}";
            this.arenaLevelText.text = $"[{entry.ReqLeval}{(entry.MaxLeval - entry.ReqLeval > 0 ? "-" + entry.MaxLeval : "")}]";

            if (ClientAPI.GetPlayerObject().PropertyExists("level"))
            {
                int playerLevel = (int)ClientAPI.GetPlayerObject().GetProperty("level");

                if (playerLevel >= entry.ReqLeval && playerLevel <= entry.MaxLeval)
                {
                    this.arenaLevelText.RemoveFromClassList("arena-entry-require-failed");

                    if (entry.StartHour == 0 && entry.StartMin == 0 && entry.EndHour == 0 && entry.EndMin == 0)
                    {
                        this.arenaTimeText.RemoveFromClassList("arena-entry-require-failed");
                        this.arenaTitleText.RemoveFromClassList("arena-entry-require-failed");
                    }
                    else if (timeNow >= arenaStart && timeNow <= arenaEnd)
                    {
                        this.arenaTimeText.RemoveFromClassList("arena-entry-require-failed");
                        this.arenaTitleText.RemoveFromClassList("arena-entry-require-failed");
                    }
                    else
                    {
                        this.arenaTimeText.AddToClassList("arena-entry-require-failed");
                        this.arenaTitleText.AddToClassList("arena-entry-require-failed");
                    }
                }
                else
                {
                    if (entry.StartHour == 0 && entry.StartMin == 0 && entry.EndHour == 0 && entry.EndMin == 0)
                    {
                        this.arenaTimeText.RemoveFromClassList("arena-entry-require-failed");
                    }
                    else if (timeNow >= arenaStart && timeNow <= arenaEnd)
                    {
                        this.arenaTimeText.RemoveFromClassList("arena-entry-require-failed");
                    }
                    else
                    {
                        this.arenaTimeText.AddToClassList("arena-entry-require-failed");
                    }

                    this.arenaTitleText.AddToClassList("arena-entry-require-failed");
                    this.arenaLevelText.AddToClassList("arena-entry-require-failed");
                }
            }
        }

    }
}
