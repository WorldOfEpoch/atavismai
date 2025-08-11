using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using TMPro;
 using System;
 using Atavism.UI.Game;
 using UnityEngine.SceneManagement;
 using UnityEngine.UIElements;

 namespace Atavism.UI
{

    public class UIAtavismArenaScore : MonoBehaviour
    {
        [SerializeField] protected UIDocument uiDocument;
        [SerializeField] VisualTreeAsset listElementTemplate;
        List<VisualElement> TeamsGrids = new List<VisualElement>();
        Label Score;
        List<Label> teamName = new List<Label>();
        List<List<UIAtavismArenaScoreListEntity>> _teams = new  List<List<UIAtavismArenaScoreListEntity>>();
        bool showing = false;
        protected bool isRegisteredUI, isInitialize;
        protected VisualElement uiScreen, uiWindow;
        public int numberOfTeams = 2;
        private Button leaveButton;
        public KeyCode toggleKey = KeyCode.F2;

        // Use this for initialization
        protected void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            Initialize();
            ArenaSetup();
            Hide();
        }

        protected virtual void OnDisable()
        {
          //  Debug.LogError("OnDisable");
            Deinitialize();
        }

        public virtual void Initialize()
        {
            if (isInitialize)
                return;

            registerUI();
            registerEvents();

            isInitialize = true;
        }

        public virtual void Deinitialize()
        {
            if (!isInitialize)
                return;

            Hide();

            unregisterEvents();
            unregisterUI();

            isInitialize = false;
        }


        protected void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("ARENA_SCORE_SETUP", this);
            AtavismEventSystem.RegisterEvent("ARENA_SCORE_UPDATE", this);
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
            NetworkAPI.RegisterExtensionMessageHandler("arena_end", HandleArenaEndMessage);
        }

        protected void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("ARENA_SCORE_SETUP", this);
            AtavismEventSystem.UnregisterEvent("ARENA_SCORE_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
            NetworkAPI.RemoveExtensionMessageHandler("arena_end", HandleArenaEndMessage);
        }

        protected bool registerUI()
        {
            uiDocument.enabled = true;
            uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("ArenaScoreWindow");
            uiScreen = uiDocument.rootVisualElement.Query<VisualElement>("Screen");
            Button uiWindowCloseButton = uiDocument.rootVisualElement.Query<Button>("Window-close-button");
            if (uiWindowCloseButton != null)
                uiWindowCloseButton.HideVisualElement();
            Score = uiDocument.rootVisualElement.Query<Label>("score");
            for (int i = 1; i <= numberOfTeams; i++)
            {
                var team = uiDocument.rootVisualElement.Query<Label>("team-" + i);
                if (team != null)
                {
                    teamName.Add(team);
                }
                
                var list = uiDocument.rootVisualElement.Query<VisualElement>("list-" + i);
                if (list != null)
                {
                    TeamsGrids.Add(list);
                }
                _teams.Add(new List<UIAtavismArenaScoreListEntity>());
            }

            leaveButton = uiDocument.rootVisualElement.Query<Button>("leave-button");
            if (leaveButton != null)
              leaveButton.clicked += LeaveInstance;
           

            Hide();
            return true;
        }

        protected virtual bool unregisterUI()
        {
            if (!isRegisteredUI)
                return false;
            if (leaveButton != null)
                leaveButton.clicked -= LeaveInstance;
          

            if (UIAtavismAudioManager.Instance != null)
                UIAtavismAudioManager.Instance.UnregisterSFX(uiDocument);

            isRegisteredUI = false;

            return true;
        }

        private void HandleArenaEndMessage(Dictionary<string, object> props)
        {
            Show();
            UpdateDisplay();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(toggleKey) && !AtavismSettings.UIHasFocus() && Arena.Instance.InArena)
            {
                if (showing)
                    Hide();
                else
                    Show();
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ARENA_SCORE_UPDATE" || eData.eventType == "UPDATE_LANGUAGE")
            {
                UpdateDisplay();
            }
            else if (eData.eventType == "ARENA_SCORE_SETUP")
            {
                ArenaSetup();
            }
        }




        void ArenaSetup()
        {
            List<ArenaTeamEntry> teams = Arena.Instance.ArenaTeamEntries;
            if (teams != null)
            {
                if (teams.Count > 0)
                {
                    for (int i = 0; i < teams.Count; i++)
                    {
                        TeamsGrids[i].Clear();

                        if (_teams[i] == null)
                            _teams[i] = new List<UIAtavismArenaScoreListEntity>();
                        for (int j = 0; j < teams[i].players.Count; j++)
                        {

                            UIAtavismArenaScoreListEntity row = new UIAtavismArenaScoreListEntity();
                            // Instantiate the UXML template for the entry
                            var newListEntry = listElementTemplate.Instantiate();
                            // Assign the controller script to the visual element
                            newListEntry.userData = row;
                            // Initialize the controller script
                            row.SetVisualElement(newListEntry);
                            TeamsGrids[i].Add(newListEntry);
                            row.PlyName.text = teams[i].players[j].playerName;
                            row.PlyScore.text = teams[i].players[j].score.ToString();
                            row.PlyKills.text = teams[i].players[j].kills.ToString();
                            row.PlyDeaths.text = teams[i].players[j].deaths.ToString();
                            row.PlyDamageTaken.text = teams[i].players[j].damageTaken.ToString();
                            row.PlyDamageDealt.text = teams[i].players[j].damageDealt.ToString();
                            _teams[i].Add(row);
                        }
                    }
                }
                else
                {
                    //  Debug.LogError("ArenaSetup teams = 0");
                }
            }
            else
            {
                //   Debug.LogError("ArenaSetup teams null");
            }
        }

        void UpdateDisplay()
        {
            List<ArenaTeamEntry> teams = Arena.Instance.ArenaTeamEntries;
            if (Score != null)
                Score.text = "";
            if (teams != null)
            {
                if (teams.Count > 0)
                {
                    for (int i = 0; i < teams.Count; i++)
                    {
                        if (teamName[i] != null)
                            teamName[i].text = teams[i].teamName;
                        if (Score != null)
                        {
                            Score.text = Score.text + teams[i].score.ToString();
                            if (teams.Count - 1 > i)
                                Score.text = Score.text + " : ";
                        }

                        if (_teams[i] == null)
                            _teams[i] = new List<UIAtavismArenaScoreListEntity>();
                        for (int j = 0; j < teams[i].players.Count; j++)
                        {
                            if (_teams[i].Count - 1 < j)
                            {
                                UIAtavismArenaScoreListEntity row = new UIAtavismArenaScoreListEntity();
                                // Instantiate the UXML template for the entry
                                var newListEntry = listElementTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = row;
                                // Initialize the controller script
                                row.SetVisualElement(newListEntry);
                                TeamsGrids[i].Add(newListEntry);
                                row.PlyName.text = teams[i].players[j].playerName;
                                row.PlyScore.text = teams[i].players[j].score.ToString();
                                row.PlyKills.text = teams[i].players[j].kills.ToString();
                                row.PlyDeaths.text = teams[i].players[j].deaths.ToString();
                                row.PlyDamageTaken.text = teams[i].players[j].damageTaken.ToString();
                                row.PlyDamageDealt.text = teams[i].players[j].damageDealt.ToString();
                                _teams[i].Add(row);
                            }
                            else
                            {
                                UIAtavismArenaScoreListEntity ply = _teams[i][j];
                                ply.PlyName.text = teams[i].players[j].playerName;
                                ply.PlyScore.text = teams[i].players[j].score.ToString();
                                ply.PlyKills.text = teams[i].players[j].kills.ToString();
                                ply.PlyDeaths.text = teams[i].players[j].deaths.ToString();
                                ply.PlyDamageTaken.text = teams[i].players[j].damageTaken.ToString();
                                ply.PlyDamageDealt.text = teams[i].players[j].damageDealt.ToString();
                                _teams[i][j] = ply;
                            }
                        }

                    }
                }
                else
                {
                    Debug.LogError("UpdateDisplay teams = 0");
                }
            }
            else
            {
                Debug.LogError("UpdateDisplay teams null");
            }
        }

        public void Show()
        {
            AtavismSettings.Instance.OpenWindow(this);
            showing = true;
            // GetComponent<CanvasGroup>().alpha = 1f;
            // GetComponent<CanvasGroup>().blocksRaycasts = true;
            // GetComponent<CanvasGroup>().interactable = true;
            AtavismUIUtility.BringToFront(gameObject);
            //   transform.position = new Vector3((Screen.width / 2) - GetComponent<RectTransform>().sizeDelta.x / 2 * GetComponent<RectTransform>().localScale.x, (Screen.height / 2) - GetComponent<RectTransform>().sizeDelta.y / 2 * GetComponent<RectTransform>().localScale.y, 0);
            if (uiScreen != null)
            {
                uiScreen.ShowVisualElement();
            }else if (uiWindow != null)
            {
                uiWindow.ShowVisualElement();
            }
        }

        public void Hide()
        {
            AtavismSettings.Instance.CloseWindow(this);
            showing = false;
            if (uiScreen != null)
            {
                uiScreen.HideVisualElement();
            }else if (uiWindow != null)
            {
                uiWindow.HideVisualElement();
            }
            // GetComponent<CanvasGroup>().alpha = 0f;
            // GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        public void LeaveInstance()
        {
            if (AtavismSettings.Instance.ArenaInstances.Contains(SceneManager.GetActiveScene().name))
            {
#if AT_I2LOC_PRESET
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want exit arena") + " " + "?", null, SendLeaveArena);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want exit arena ?", null,
                    SendLeaveArena);
#endif
            }

            if (AtavismSettings.Instance.DungeonInstances.Contains(SceneManager.GetActiveScene().name))
            {
#if AT_I2LOC_PRESET
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want exit instance") + " " + "?", null, SendLeaveInstance);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want exit instance ?", null,
                    SendLeaveInstance);
#endif
                void SendLeaveArena(object item, bool accepted)
                {
                    if (accepted)
                    {
                        //    Debug.LogError("Leave Arena");
                        Dictionary<string, object> props = new Dictionary<string, object>();
                        NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.leaveArena", props);
                    }
                }

                void SendLeaveInstance(object item, bool accepted)
                {
                    if (accepted)
                    {
                        //    Debug.LogError("Leave Instance");
                        Dictionary<string, object> props = new Dictionary<string, object>();
                        NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.leaveInstance", props);
                    }
                }
            }
        }

        void SendLeaveArena(object item, bool accepted)
        {
            if (accepted)
            {
                //    Debug.LogError("Leave Arena");
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.leaveArena", props);
            }
        }

        void SendLeaveInstance(object item, bool accepted)
        {
            if (accepted)
            {
                //    Debug.LogError("Leave Instance");
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.leaveInstance", props);
            }
        }

    }
}