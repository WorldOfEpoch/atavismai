using Atavism.UI.Game;
using HNGamers.Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static HNGamers.Atavism.ModularCustomizationManager;

namespace Atavism.UI
{
    
    public class UIAtavismCharacterSelectionSceneManager : UIAtavismWindowBase
    {
        private static UIAtavismCharacterSelectionSceneManager instance;
        public static UIAtavismCharacterSelectionSceneManager Instance => instance;

        [AtavismSeparator("Runtime")]
        [SerializeField]  protected LoginState loginState;
        public LoginState State => loginState;

        [AtavismSeparator("Setup")]
        [SerializeField] private Transform spawnPosition;
        [SerializeField][Range(1,6)] private int maxCharacterSlots = 5;
        [SerializeField] private MouseButton characterModelRotateInput;
        [SerializeField] private float characterRotationSpeed = 250.0f;
        [SerializeField] private Vector3 cameraZoomHeadLoc = new Vector3(0.7f, 0.05f, -0.3f);
        [SerializeField] private Vector3 cameraZoomBodyLoc = new Vector3(2.5f, 1f, -1f);
        [SerializeField] private PortraitType portraitType;

        [AtavismSeparator("UI")]
        [SerializeField] private string uiSelectServerButtonName = "selectServerButton";
        [SerializeField] private string uiCreateCharacterButtonName = "createCharacterButton";
        [SerializeField] private string uiDeleteCharacterButtonName = "deleteCharacterButton";
        [Space(10)]
        [SerializeField] private string uiCharacterItemSelectedClassName = "characterItemSelected";
        [SerializeField] private string uiContentCharactersName = "content-characters";
        [Space(10)]
        [SerializeField] private string uiCharacterItemIconName = "itemCharacterIcon";
        [SerializeField] private string uiCharacterItemTitleName = "itemCharacterName";
        [SerializeField] private string uiCharacterItemClassName = "itemCharacterClass";
        [SerializeField] private string uiCharacterItemRaceName = "itemCharacterRace";
        [SerializeField] private string uiCharacterItemLevelName = "itemCharacterLevel";
        [Space(10)]
        [SerializeField] private string uiCharacterNameLabelName = "characterNameLabel";
        [SerializeField] private string uiWorldEnterButtonName = "worldEnterButton";
        [SerializeField] private string uiQuitButtonName = "quitButton";
        [Space(10)]
        [SerializeField] private string uiMessageFiledLabelName = "message-label";
        [AtavismSeparator("Initialize")]
        [SerializeField] private GameObject characterCamera;
        [SerializeField] private UIAtavismCharacterCreationManager creationManager;

        protected Button uiSelectServerButton;
        protected Button uiCreateCharacterButton;
        protected Button uiDeleteCharacterButton;
        protected List<UICharacterSelectionSlot> uiCharacterItems;
        protected Label uiCharacterNameLabel;
        protected Button uiWorldEnterButton;
        protected Button uiQuitButton;

        protected Label uiMessageFieldLabel;
        // Character fields
        protected int selectedCharacterIndex;
        protected GameObject characterModel;
        public GameObject CharacterModel => characterModel;
        protected GameObject characterDCS;
        public GameObject CharacterDCS => characterDCS;
        protected List<CharacterEntry> characterEntries;
        protected CharacterEntry selectedCharacterEntry = null;

        // Camera fields
        protected float x = 180;
        protected float y = 0;
        private float scrollpos = 1;
        [SerializeField]  float cameraMoveRate = 1.0f;
        // Other
        private float enterWorldClickLimit = 0;

        #region Initiate
        private void Awake()
        {
            instance = this;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            registerEvents();
        }

        protected override void Start()
        {
            base.Start();
#if HSVPicker
            if (bodyartpicker)
            {
                bodyartpicker.onValueChanged.AddListener(color =>
            {
                SwitchBodyArtColortoColor(color);
            });
            }

            if (eyepicker)
            {
                eyepicker.onValueChanged.AddListener(color =>
            {
                SwitchEyeColortoColor(color);
            });
            }

            if (hairpicker)
            {
                hairpicker.onValueChanged.AddListener(color =>
            {
                if (allowDifferentHairColors)
                {
                    SwitchHairColortoColor(color);
                }
                else
                {
                    SwitchHairColortoColor(color);
                    SwitchBeardColortoColor(color);
                    SwitchEyebrowColortoColor(color);
                }
            });
            }

            if (stubblepicker)
            {
                stubblepicker.onValueChanged.AddListener(color =>
            {
                SwitchStubbleColortoColor(color);
            });
            }

            if (scarpicker)
            {
                scarpicker.onValueChanged.AddListener(color =>
            {
                SwitchSkinScarColortoColor(color);
            });
            }

            if (mouthpicker)
            {
                mouthpicker.onValueChanged.AddListener(color =>
                {
                    SwitchMouthColortoColor(color);
                });
            }

            if (primarypicker && atavismColorPicker)
            {
                primarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchPrimaryColortoColor(color, atavismColorPicker);
                });
            }

            if (secondarypicker && atavismColorPicker)
            {
                secondarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchSecondaryColortoColor(color, atavismColorPicker);
                });
            }

            if (metalprimarypicker && atavismColorPicker)
            {
                metalprimarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchMetalPrimaryColortoColor(color, atavismColorPicker);
                });
            }

            if (metalsecondarypicker && atavismColorPicker)
            {
                metalsecondarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchMetalSecondaryColortoColor(color, atavismColorPicker);
                });
            }

            if (metaldarkpicker && atavismColorPicker)
            {
                metaldarkpicker.onValueChanged.AddListener(color =>
                {
                    SwitchMetalDarkColortoColor(color, atavismColorPicker);
                });
            }

            if (leatherprimarypicker && atavismColorPicker)
            {
                leatherprimarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchLeatherPrimaryColortoColor(color, atavismColorPicker);
                });
            }

            if (leathertertiarypicker && atavismColorPicker)
            {
                leathertertiarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchLeatherTertiaryColortoColor(color, atavismColorPicker);
                });
            }

            if (tertiarypicker && atavismColorPicker)
            {
                tertiarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchTertiaryColortoColor(color, atavismColorPicker);
                });
            }

            if (leathersecondarypicker && atavismColorPicker)
            {
                leathersecondarypicker.onValueChanged.AddListener(color =>
                {
                    SwitchLeatherSecondaryColortoColor(color, atavismColorPicker);
                });
            }

            if (allowDifferentHairColors)
            {
                if (beardpicker)
                {
                    beardpicker.onValueChanged.AddListener(color =>
                {
                    SwitchBeardColortoColor(color);
                });
                }
                if (eyebrowpicker)
                {
                    eyebrowpicker.onValueChanged.AddListener(color =>
                {
                    SwitchEyebrowColortoColor(color);
                });
                }
            }

            if (skinpicker)
            {
                skinpicker.onValueChanged.AddListener(color =>
                {
                    SwitchSkinColortoColor(color);
                });
            }
#endif

            Show();

            if (characterEntries != null && characterEntries.Count == 0)
            {
                //StartCharacterCreation();
            }
            if (characterCamera != null)
                characterCamera.SetActive(true);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            unregisterEvents();
        }
        #endregion
        #region UI
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            
            uiSelectServerButton = uiDocument.rootVisualElement.Query<Button>(uiSelectServerButtonName);
            uiCreateCharacterButton = uiDocument.rootVisualElement.Query<Button>(uiCreateCharacterButtonName);
            uiDeleteCharacterButton = uiDocument.rootVisualElement.Query<Button>(uiDeleteCharacterButtonName);
            uiMessageFieldLabel = uiDocument.rootVisualElement.Query<Label>(uiMessageFiledLabelName);
            uiCharacterItems = new List<UICharacterSelectionSlot>();
            VisualElement content = uiDocument.rootVisualElement.Query<VisualElement>(uiContentCharactersName);
            content.Query().Children<UICharacterSelectionSlot>().ForEach(e => { uiCharacterItems.Add(e); });
            for (int n = 0; n < uiCharacterItems.Count; n++)
            {
                uiCharacterItems[n].viewDataKey = n.ToString();
                uiCharacterItems[n].clicked += onCharacterItemSlotClicked;
            }

            uiCharacterNameLabel = uiDocument.rootVisualElement.Query<Label>(uiCharacterNameLabelName);
            uiWorldEnterButton = uiDocument.rootVisualElement.Query<Button>(uiWorldEnterButtonName);
            uiQuitButton = uiDocument.rootVisualElement.Query<Button>(uiQuitButtonName);

            uiSelectServerButton.clicked += SelectServer;
            uiWorldEnterButton.clicked += EnterWorld;
            uiCreateCharacterButton.clicked += onCreateCharacterButtonClicked;
            uiDeleteCharacterButton.clicked += onDeleteCharacterButtonClicked;
            uiQuitButton.clicked += onQuitButtonClicked;

            isRegisteredUI = true;

            return true;
        }

        private void SelectServer()
        {
            AtavismEventSystem.DispatchEvent("SHOW_SERVER_LIST", new string[]{});
        }

        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            if (uiCharacterItems != null)
                for (int n = 0; n < uiCharacterItems.Count; n++)
                    uiCharacterItems[n].clicked -= onCharacterItemSlotClicked;

            uiWorldEnterButton.clicked -= EnterWorld;
            uiCreateCharacterButton.clicked -= onCreateCharacterButtonClicked;
            uiDeleteCharacterButton.clicked -= onDeleteCharacterButtonClicked;
            uiQuitButton.clicked -= onQuitButtonClicked;

            isRegisteredUI = false;

            return true;
        }

        public void ZoomHead()
        {
            scrollpos = 0;
        }
        public void ZoomBody()
        {
            scrollpos = 1;
        }

        protected virtual void onCharacterItemSlotClicked(VisualElement e)
        {
            int index = Convert.ToInt32(e.viewDataKey);
            SelectCharacterItemSlot(index);
        }

        protected void onDeleteCharacterButtonClicked()
        {
            string message = "Do you want to delete character: " + selectedCharacterEntry[PROPS.CHARACTER_NAME];
#if AT_I2LOC_PRESET
        message = I2.Loc.LocalizationManager.GetTranslation("Do you want to delete character") + ": " + selectedCharacterEntry[PROPS.CHARACTER_NAME];
#endif

            UIAtavismDialogPopupManager.Instance.ShowDialogPopup(message, DeleteCharacterConfirmed, "Yes", () => UIAtavismDialogPopupManager.Instance.HideDialogPopup(), "No");
        }

        protected void onCreateCharacterButtonClicked() => StartCharacterCreation();
        protected void onQuitButtonClicked() => ClientAPI.Instance.Quit();
        #endregion
        #region Atavism Events
        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent(EVENTS.WORLD_ERROR, this);
        }
        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent(EVENTS.WORLD_ERROR, this);
        }

        protected override void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == EVENTS.WORLD_ERROR)
            {
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup(eData.eventArgs[0]);
            }
        }
        #endregion
        #region Loop Updates
        protected override void Update()
        {
            base.Update();

            if(characterCamera != null && characterModel != null)
            {
                Vector3 destPosIn = characterModel.GetComponent<AtavismMobAppearance>().GetSocketTransform("Head").position + cameraZoomHeadLoc;
                Vector3 destPosOut = characterModel.transform.position + cameraZoomBodyLoc;
                Vector3 diff = destPosOut - destPosIn;
                Vector3 destPos = destPosIn + diff * scrollpos;
                characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, destPos, Time.deltaTime * cameraMoveRate);
            }
            
        }

        /// <summary>
        /// Handles character rotation if the mouse button is down and the player is dragging it.
        /// </summary>
        void LateUpdate()
        {
            if (characterModel && Input.GetMouseButton((int)characterModelRotateInput) && !AtavismCursor.Instance.IsMouseOverUI())
            {
                x -= Input.GetAxis("Mouse X") * characterRotationSpeed * 0.02f;

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                //position.y = height;
                characterModel.transform.rotation = rotation;
            }
            scrollpos -= Input.GetAxisRaw("Mouse ScrollWheel");
            if(scrollpos > 1)
                scrollpos = 1;
            else if (scrollpos < 0)
                scrollpos = 0;
            
        }
        #endregion
        #region Private/Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entry"></param>
        protected virtual void updateDataCharacterItemSlot(UICharacterSelectionSlot item, CharacterEntry entry)
        {
            item.SetCharacterName((string)entry[PROPS.CHARACTER_NAME]);
            item.SetCharacterRace((string)entry[PROPS.CHARACTER_RACE]);
            item.SetCharacterClass((string)entry[PROPS.CHARACTER_CLASS]);
            item.SetCharacterLevel(entry.ContainsKey(PROPS.LEVEL) ? "Level " + (int)entry[PROPS.LEVEL] : "Level 1");

            string gender = (string)entry[PROPS.CHARACTER_GENDER];
            int raceId = (int)entry[PROPS.CHARACTER_RACE_ID];
            int aspectId = (int)entry[PROPS.CHARACTER_CLASS_ID];
            int genderId = -1;

            if (entry.ContainsKey(PROPS.CHARACTER_GENDER_ID))
                genderId = (int)entry[PROPS.CHARACTER_GENDER_ID];
            else
            {
                foreach (var gen in AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].genderList.Values)
                {
                    if (gen.name.Equals(gender))
                    {
                        genderId = gen.id;
                    }
                }
            }

            Sprite portraitSprite = null;
            if (entry.ContainsKey(PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT)))
                portraitSprite = PortraitManager.Instance.LoadPortrait((string)entry[PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT)]);
            if (portraitSprite == null && entry.ContainsKey(PROPS.PORTRAIT))
                portraitSprite = PortraitManager.Instance.LoadPortrait((string)entry[PROPS.PORTRAIT]);
            if (portraitSprite == null)
                portraitSprite = PortraitManager.Instance.GetCharacterSelectionPortrait(genderId, raceId, aspectId, portraitType);

            if (portraitSprite == null)
            {
                item.SetCharacterIcon(null);
             //   Debug.LogError("Portrait sprite is null.");
            }
            else
                item.SetCharacterIcon(portraitSprite);

            item.Unselect();
            item.Show();
        }

        #endregion
        #region Public Methods
        public override void Show()
        {
            base.Show();

            loginState = LoginState.CharacterSelect;
            UpdateData();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UpdateData()
        {
            if (AtavismClient.Instance != null)
                uiWindowTitle.text = AtavismClient.Instance.WorldId;

            if (ClientAPI.Instance != null)
                characterEntries = ClientAPI.GetCharacterEntries();

            // Update data
            for (int n = 0; n < uiCharacterItems.Count; n++)
            {
                if (n < characterEntries.Count)
                {
                    updateDataCharacterItemSlot(uiCharacterItems[n], characterEntries[n]);

                    continue;
                }

                uiCharacterItems[n].Hide();
            }

            SelectCharacterItemSlot(PlayerPrefs.GetInt("LAST_SELECTED_CHARACTER_INDEX", 0));
            uiCreateCharacterButton.style.display = characterEntries.Count < maxCharacterSlots ? DisplayStyle.Flex : DisplayStyle.None;

            loginState = LoginState.CharacterSelect;
            if (characterEntries.Count == 0)
            {
                StartCharacterCreation();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        public virtual void ShowCharacterModel(CharacterEntry entry)
        {
          //  Debug.LogError("Delete");
            if (characterModel != null)
                Destroy(characterModel);

            if (selectedCharacterEntry != null)
            {
                int raceId = (int)selectedCharacterEntry[PROPS.CHARACTER_RACE_ID];
                int aspectId = (int)selectedCharacterEntry[PROPS.CHARACTER_CLASS_ID];
                string gender = (string)selectedCharacterEntry[PROPS.CHARACTER_GENDER];
                int genderId = -1;
                if (selectedCharacterEntry.ContainsKey(PROPS.CHARACTER_GENDER_ID))
                    genderId = (int)selectedCharacterEntry[PROPS.CHARACTER_GENDER_ID];
                else
                    foreach (var gen in AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].genderList.Values)
                        if (gen.name.Equals(gender))
                            genderId = gen.id;

                ShowCharacterModel(raceId, aspectId, genderId);
            }
        }
        public virtual void ShowCharacterModel(int raceId, int aspectId, int genderId)
        {
          //  Debug.LogError("Delete");
            if (characterModel != null)
                Destroy(characterModel);

          //  Debug.LogError("ShowCharacterModel "+raceId+" "+aspectId+" "+ genderId);
            
            string genderPrefab = AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].genderList[genderId].prefab;
            int resourcePathPosDamage = genderPrefab.IndexOf("Resources/");
            if (genderPrefab.Length > 10)
            {
                genderPrefab = genderPrefab.Substring(resourcePathPosDamage + 10);
                genderPrefab = genderPrefab.Remove(genderPrefab.Length - 7);
            }

            GameObject prefab = (GameObject)Resources.Load(genderPrefab);
            if (prefab == null)
            {
                AtavismLogger.LogWarning("prefab = null model: " + genderPrefab + " Loading ExampleCharacter");
                prefab = (GameObject)Resources.Load("ExampleCharacter");
            }

          //  Debug.LogError("Instantiate "+genderPrefab );
            characterModel = (GameObject)Instantiate(prefab, spawnPosition);
            x = 180;

            if (characterModel != null)
            {
                characterModel.transform.localPosition = new Vector3(0, 0, 0);
                characterModel.transform.localRotation = Quaternion.identity;
                CustomizationManagerCheck();

                AtavismMobAppearance mobAppearance = characterModel.GetComponent<AtavismMobAppearance>();
                mobAppearance.aspect = aspectId;
                mobAppearance.race = raceId;
                mobAppearance.gender = genderId;
                
                if (selectedCharacterEntry != null && isVisible)
                {
                    foreach (var key in selectedCharacterEntry.Keys)
                    {
                        if (key.EndsWith("DisplayID"))
                        {
                             string keyv = key.Substring(0, key.IndexOf("DisplayID"));

                            if (selectedCharacterEntry.ContainsKey(keyv + "DisplayVAL"))
                                mobAppearance.displayVal = (string)selectedCharacterEntry[keyv + "DisplayVAL"];

                            mobAppearance.UpdateEquipDisplay(key, (string)selectedCharacterEntry[key]);
                        }
                    }
                }
            }

            // Temp
            /*if (characterModel != null)
            {
                if (characterModel != null && characterModel.GetComponent<CustomisedHair>() != null)
                {
                    CustomisedHair customHair = characterModel.GetComponent<CustomisedHair>();
                    if (selectedCharacterEntry.ContainsKey(customHair.hairPropertyName))
                    {
                        customHair.UpdateHairModel((string)selectedCharacterEntry[customHair.hairPropertyName]);
                    }
                }
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CustomizationManagerCheck()
        {
            if (selectedCharacterEntry == null)
                return;
            if (characterModel == null)
                return;

            ModularCustomizationManager modularCustomizationManager = characterModel.GetComponent<ModularCustomizationManager>();

            if (modularCustomizationManager == null)
                return;

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.EyeMaterialPropertyName))
            {
                modularCustomizationManager.UpdateEyeMaterial((int)selectedCharacterEntry[modularCustomizationManager.EyeMaterialPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.HairMaterialPropertyName))
            {
                modularCustomizationManager.UpdateHairMaterial((int)selectedCharacterEntry[modularCustomizationManager.HairMaterialPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.SkinMaterialPropertyName))
            {
                modularCustomizationManager.UpdateSkinMaterial((int)selectedCharacterEntry[modularCustomizationManager.SkinMaterialPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.MouthMaterialPropertyName))
            {
                modularCustomizationManager.UpdateMouthMaterial((int)selectedCharacterEntry[modularCustomizationManager.MouthMaterialPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.bodyColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.bodyColorPropertyName].ToString().Split(',');
                Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateBodyColor(test);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.scarColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.scarColorPropertyName].ToString().Split(',');
                Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateBodyScarColor(test);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.hairColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.hairColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateHairColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.mouthColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.mouthColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateMouthColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.beardColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.beardColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateBeardColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.eyeBrowColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.eyeBrowColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateEyebrowColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.stubbleColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.stubbleColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateStubbleColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.bodyArtColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.bodyArtColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateBodyArtColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.eyeColorPropertyName))
            {
                var item = selectedCharacterEntry[modularCustomizationManager.eyeColorPropertyName].ToString().Split(',');
                Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                modularCustomizationManager.UpdateEyeColor(color32);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.helmetColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.helmetColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.Head);
                }
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.torsoColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.torsoColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.Torso);
                }
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.upperArmsColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.upperArmsColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.Upperarms);
                }
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.lowerArmsColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.lowerArmsColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.LowerArms);
                }
            }
            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.hipsColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.hipsColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.Hips);
                }
            }
            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.lowerLegsColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.lowerLegsColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.LowerLegs);
                }
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.feetColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.feetColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.Feet);
                }
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.handsColorPropertyName))
            {
                var colorProperties = selectedCharacterEntry[modularCustomizationManager.handsColorPropertyName].ToString().Split('@');
                foreach (var colorProperty in colorProperties)
                {
                    var colorPropertyItem = colorProperty.Split(':');
                    var colorslot = colorPropertyItem[0];
                    var coloritem = colorPropertyItem[1].Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                    modularCustomizationManager.UpdateShaderColor(color32, colorslot, BodyType.Hands);
                }
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.hairPropertyName))
            {
                modularCustomizationManager.UpdateHairModel((int)selectedCharacterEntry[modularCustomizationManager.hairPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.beardPropertyName))
            {
                modularCustomizationManager.UpdateBeardModel((int)selectedCharacterEntry[modularCustomizationManager.beardPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.eyebrowPropertyName))
            {

                modularCustomizationManager.UpdateEyebrowModel((int)selectedCharacterEntry[modularCustomizationManager.eyebrowPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.headPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.headPropertyName], BodyType.Head);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.faceTexPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.faceTexPropertyName], BodyType.Face);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.handsPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.handsPropertyName], BodyType.Hands);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.lowerArmsPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.lowerArmsPropertyName], BodyType.LowerArms);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.upperArmsPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.upperArmsPropertyName], BodyType.Upperarms);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.torsoPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.torsoPropertyName], BodyType.Torso);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.hipsPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.hipsPropertyName], BodyType.Hips);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.lowerLegsPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.lowerLegsPropertyName], BodyType.LowerLegs);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.feetPropertyName))
            {
                modularCustomizationManager.UpdateBodyModel((string)selectedCharacterEntry[modularCustomizationManager.feetPropertyName], BodyType.Feet);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.earsPropertyName))
            {
                modularCustomizationManager.UpdateEarModel((int)selectedCharacterEntry[modularCustomizationManager.earsPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.eyesPropertyName))
            {
                modularCustomizationManager.UpdateEyeModel((int)selectedCharacterEntry[modularCustomizationManager.eyesPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.tuskPropertyName))
            {
                modularCustomizationManager.UpdateTuskModel((int)selectedCharacterEntry[modularCustomizationManager.tuskPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.mouthPropertyName))
            {
                modularCustomizationManager.UpdateMouthModel((int)selectedCharacterEntry[modularCustomizationManager.mouthPropertyName]);
            }

            if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.faithPropertyName))
            {
                modularCustomizationManager.SetFaith((string)selectedCharacterEntry[modularCustomizationManager.faithPropertyName]);
            }
#if IPBRInt
                if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.blendshapePresetValue) && (!modularCustomizationManager.enableSavingInfinityPBRBlendshapes))
                {
                    modularCustomizationManager.UpdateBlendShapePresets((int)selectedCharacterEntry[modularCustomizationManager.blendshapePresetValue]);
                }

                if (selectedCharacterEntry.ContainsKey(modularCustomizationManager.infinityBlendShapes))
                {
                    modularCustomizationManager.UpdateBlendShapes((string)selectedCharacterEntry[modularCustomizationManager.infinityBlendShapes]);
                }
#endif

        }

        public virtual void SelectCharacterItemSlot(CharacterEntry characterentry)
        {
            UpdateData();
            int index = characterEntries.IndexOf(characterentry);
            if (index != -1)
            {
                PlayerPrefs.SetInt("LAST_SELECTED_CHARACTER_INDEX", index);
            }

            SelectCharacterItemSlot(index);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public virtual void SelectCharacterItemSlot(int index)
        {
            if (index >= 0 && index >= characterEntries.Count)
            {
                index = characterEntries.Count-1;
            }
            PlayerPrefs.SetInt("LAST_SELECTED_CHARACTER_INDEX", index);

            for (int n = 0; n < uiCharacterItems.Count; n++)
                uiCharacterItems[n].Unselect();

            if (index >= 0 && index < uiCharacterItems.Count)
                uiCharacterItems[index].Select();
            else if (characterEntries.Count == 0)
            {
                StartCharacterCreation(); 
            }
            else
            {
                SelectCharacterItemSlot(0);
                return;
            }

            if (index >= 0 && index < characterEntries.Count)
            {
                selectedCharacterIndex = index;
                selectedCharacterEntry = characterEntries[selectedCharacterIndex];

                uiCharacterNameLabel.text = (string)characterEntries[selectedCharacterIndex][PROPS.CHARACTER_NAME];
            }
            else
            {
                selectedCharacterIndex = -1;
                selectedCharacterEntry = null;
                uiCharacterNameLabel.text = "";
            }

            ShowCharacterModel(selectedCharacterEntry);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void DeleteCharacterConfirmed()
        {
            UIAtavismDialogPopupManager.Instance.HideDialogPopup();

            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add("characterId", selectedCharacterEntry.CharacterId);
            NetworkAPI.DeleteCharacter(attrs);

            UpdateData();
            if (characterEntries.Count > 0)
            SelectCharacterItemSlot(0);
            
        }
        #endregion

        public void StartCharacterCreation()
        {
            Hide();
            creationManager.Show();
            selectedCharacterEntry = null;
        }

        public void EnterWorld()
        {
            if (selectedCharacterEntry == null)
                return;

            if (enterWorldClickLimit > Time.time)
                return;

            enterWorldClickLimit = Time.time + 1f;
            string dialogMessage = "Entering World...";
#if AT_I2LOC_PRESET
            dialogMessage = I2.Loc.LocalizationManager.GetTranslation("Entering World...");
#endif

            if (uiMessageFieldLabel != null)
            {
                uiMessageFieldLabel.text = dialogMessage;
            }
            else
            {
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup(dialogMessage);
            }

            AtavismClient.Instance.EnterGameWorld(selectedCharacterEntry.CharacterId);
        }
    }
}