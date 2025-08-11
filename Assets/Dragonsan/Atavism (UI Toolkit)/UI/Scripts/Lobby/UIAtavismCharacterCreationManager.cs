using Atavism.UI;
using Atavism.UI.Game;
using HNGamers.Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static HNGamers.Atavism.ModularCustomizationManager;

namespace Atavism.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class UIAtavismCharacterCreationManager : UIAtavismWindowBase
    {
        [AtavismSeparator("UI")]
        [SerializeField] private string uiContainerRacesName = "container-races";
        [SerializeField] private string uiContainerClassessName = "container-classes";
        [SerializeField] private string uiContainerGendersName = "container-genders";
        [SerializeField] private string uiContainerAvatarsName = "container-avatars";
        [SerializeField] private string uiCreatePanelName = "create-panel";
        [SerializeField] private string uiAvatarsPanelName = "avatars-panel";
        [SerializeField] private string uiAvatarsCloseButtonName = "avatar-close";
        [SerializeField] private string uiAvatarName = "avatar";
        [SerializeField] private string uiCreateButtonName = "createCharacterButton";
        [SerializeField] private string uiCharacterNameTextFieldName = "characterNameTextField";
        [SerializeField] private string uiCancelButtonName = "cancelButton";
        [SerializeField] private string uiCustomizerPanelName = "customizer-panel";
        [SerializeField] private string uiRaceDesriptionIconName = "race-description-icon";
        [SerializeField] private string uiRaceDesriptionLabelName = "race-description-label";
        [SerializeField] private string uiClassDesriptionIconName = "class-description-icon";
        [SerializeField] private string uiClassDesriptionLabelName = "class-description-label";
        [SerializeField] private string uiMessageFiledLabelName = "message-label";

        protected VisualElement uiRaceDescriptionIcon, uiClassDescriptionIcon;
        protected VisualElement uiAvatarsPanel, uiCreatePanel;
        protected Label uiRaceDescriptionLabel, uiClassDescriptionLabel, uiMessageFieldLabel;
        protected TextField uiCharacterNameTextField;
        protected List<UICreateCharacterSlot> uiRaceSlots;
        protected List<UICreateCharacterSlot> uiClassSlots;
        protected List<UICreateCharacterSlot> uiGenderSlots;
        protected List<UICreateCharacterSlot> uiAvatarSlots;
        protected Button uiCreateButton, uiCancelButton, uiAvatarPanelCloseButton ;
        protected UIButtonToggleGroupPanel uiCustomizerPanel;
        UICreateCharacterSlot uiSelectedAvatarSlot;
        protected int selectedRaceId, selectedClassId, selectedGenderId, selectedAvatarIndex;
        protected RaceData selectedRaceData;
        protected ClassData selectedClassData;
        protected GenderData selectedGenderData;

        private int lastSelectedClass, lastSelectedGender, lastSelectedAvatar;

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            // Get references
            uiCreateButton = uiDocument.rootVisualElement.Query<Button>(uiCreateButtonName);
            uiCancelButton = uiDocument.rootVisualElement.Query<Button>(uiCancelButtonName);
            uiCharacterNameTextField = uiDocument.rootVisualElement.Query<TextField>(uiCharacterNameTextFieldName);
            uiCustomizerPanel = uiDocument.rootVisualElement.Query<UIButtonToggleGroupPanel>(uiCustomizerPanelName);

            
            uiCreatePanel = uiDocument.rootVisualElement.Query<VisualElement>(uiCreatePanelName);
            uiAvatarsPanel = uiDocument.rootVisualElement.Query<VisualElement>(uiAvatarsPanelName);
            uiAvatarPanelCloseButton = uiDocument.rootVisualElement.Query<Button>(uiAvatarsCloseButtonName);
            uiSelectedAvatarSlot = uiDocument.rootVisualElement.Query<UICreateCharacterSlot>(uiAvatarName);
            uiSelectedAvatarSlot.clicked += ShowAvatarList;
            
            // races
            VisualElement container = uiDocument.rootVisualElement.Query<VisualElement>(uiContainerRacesName);
            uiRaceSlots = new List<UICreateCharacterSlot>();
            container.Query().Children<UICreateCharacterSlot>().ForEach(e => { uiRaceSlots.Add(e); e.clicked += onRaceSlotClicked; });

            // classes
            container = uiDocument.rootVisualElement.Query<VisualElement>(uiContainerClassessName);
            uiClassSlots = new List<UICreateCharacterSlot>();
            container.Query().Children<UICreateCharacterSlot>().ForEach(e => { uiClassSlots.Add(e); e.clicked += onClassSlotClicked; });

            // genders
            container = uiDocument.rootVisualElement.Query<VisualElement>(uiContainerGendersName);
            uiGenderSlots = new List<UICreateCharacterSlot>();
            container.Query().Children<UICreateCharacterSlot>().ForEach(e => { uiGenderSlots.Add(e); e.clicked += onGenderSlotClicked; });

            // avatars
            container = uiDocument.rootVisualElement.Query<VisualElement>(uiContainerAvatarsName);
            uiAvatarSlots = new List<UICreateCharacterSlot>();
            container.Query().Children<UICreateCharacterSlot>().ForEach(e => { uiAvatarSlots.Add(e); e.clicked += onAvatarSlotClicked; });

            // others
            uiRaceDescriptionIcon = uiDocument.rootVisualElement.Query<VisualElement>(uiRaceDesriptionIconName);
            uiClassDescriptionIcon = uiDocument.rootVisualElement.Query<VisualElement>(uiClassDesriptionIconName);
            uiRaceDescriptionLabel = uiDocument.rootVisualElement.Query<Label>(uiRaceDesriptionLabelName);
            uiClassDescriptionLabel = uiDocument.rootVisualElement.Query<Label>(uiClassDesriptionLabelName);
            uiMessageFieldLabel = uiDocument.rootVisualElement.Query<Label>(uiMessageFiledLabelName);

            // Register controls
            uiCreateButton.clicked += onCreateButtonClick;
            uiCancelButton.clicked += onCancelButtonClick;
            uiAvatarPanelCloseButton.clicked += onCloseAvatarPanel;
            
            // Final
            isRegisteredUI = true;
            
            return true;
        }

      
        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            // Unregister controls
            for (int n = 0; n < uiRaceSlots.Count; n++)
                uiRaceSlots[n].clicked -= onRaceSlotClicked;
            for (int n = 0; n < uiClassSlots.Count; n++)
                uiClassSlots[n].clicked -= onClassSlotClicked;
            for (int n = 0; n < uiGenderSlots.Count; n++)
                uiGenderSlots[n].clicked -= onGenderSlotClicked;
            for (int n = 0; n < uiAvatarSlots.Count; n++)
                uiAvatarSlots[n].clicked -= onAvatarSlotClicked;

            uiCreateButton.clicked -= onCreateButtonClick;
            uiCancelButton.clicked -= onCancelButtonClick;

            // Final
            isRegisteredUI = false;

            return true;
        }

        private void onRaceSlotClicked(VisualElement e)
        {
            SelectRaceSlot((UICreateCharacterSlot)e);
        }

        private void onClassSlotClicked(VisualElement e)
        {
            SelectClassSlot((UICreateCharacterSlot)e);
        }

        private void onGenderSlotClicked(VisualElement e)
        {
            SelectGenderSlot((UICreateCharacterSlot)e);
        }

        private void onAvatarSlotClicked(VisualElement e)
        {
            SelectAvatarSlot((UICreateCharacterSlot)e);
            CloseAvatarList(e);
        }
        private void ShowAvatarList(VisualElement obj)
        {
            uiAvatarsPanel.ShowVisualElement();
            uiCreatePanel.HideVisualElement();
        }

        private void CloseAvatarList(VisualElement obj)
        {
            uiAvatarsPanel.HideVisualElement();
            uiCreatePanel.ShowVisualElement();
        }
        protected virtual void onCreateButtonClick()
        {
            CreateCharacter();
        }

        protected virtual void onCancelButtonClick()
        {
            
            UIAtavismCharacterSelectionSceneManager.Instance.ZoomBody();
            CloseAvatarList(null);
            Hide();
        }

        protected virtual void onCloseAvatarPanel()
        {
            CloseAvatarList(null);
        }
        public override void Show()
        {
            base.Show();

            lastSelectedClass = lastSelectedGender = lastSelectedGender = -1;

            uiCharacterNameTextField.value = "";
            UpdateData_RaceSlots();
            UpdateData_AvatarSlots();
        }

        public override void UpdateData()
        {
            UpdateData_RaceSlots();
            UpdateData_AvatarSlots();
        }

        #region Race
        public void UpdateData_RaceSlots()
        {
            // Races
            int g = 0;
            var races = AtavismPrefabManager.Instance.GetRaceData().Keys.ToList();
            races.Sort();
            foreach (var raceId in races)
            {
                
                if (uiRaceSlots.Count > g)
                {
                    var raceData = AtavismPrefabManager.Instance.GetRaceData()[raceId];
                    uiRaceSlots[g].viewDataKey = raceData.id.ToString();
                    uiRaceSlots[g].SetIcon( AtavismPrefabManager.Instance.GetRaceIconByID(raceData.id));
                    uiRaceSlots[g].Unselect();
                    uiRaceSlots[g].Show();
                }

                g++;
            }
            for (int gg = g; gg < uiRaceSlots.Count; gg++)
            {
                uiRaceSlots[gg].Hide();
            }
            
            SelectRaceSlot(UnityEngine.Random.Range(0, races.Count));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        public void SelectRaceSlot(UICreateCharacterSlot slot)
        {
            for (int n = 0; n < uiRaceSlots.Count; n++)
                uiRaceSlots[n].Unselect();

            slot.Select();
            selectedRaceId = Convert.ToInt32(slot.viewDataKey);
            selectedRaceData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId];

            UpdateData_RaceDescriptions();
            UpdateData_ClassSlots();
        }
        public void SelectRaceSlot(int index)
        {
            for (int n = 0; n < uiRaceSlots.Count; n++)
                uiRaceSlots[n].Unselect();

            uiRaceSlots[index].Select();
            selectedRaceId = Convert.ToInt32(uiRaceSlots[index].viewDataKey);
            selectedRaceData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId];

            UpdateData_RaceDescriptions();
            UpdateData_ClassSlots();
        }
        public void UpdateData_RaceDescriptions()
        {
            uiRaceDescriptionIcon.style.backgroundImage = new StyleBackground(selectedRaceData.icon);
            uiRaceDescriptionLabel.text = selectedRaceData.description;
        }
        #endregion
        #region Class
        public void UpdateData_ClassSlots()
        { 
            // Classes
            int g = 0;
            var classes = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList.Keys.ToList();
            classes.Sort();
           bool found = false;
           int lastAvaiableClass = -1;
            foreach (var classId in classes)
            {
                
                if (uiClassSlots.Count > g)
                {
                    var classData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[classId];
                    lastAvaiableClass++;
                    uiClassSlots[g].viewDataKey = classData.id.ToString();
                    uiClassSlots[g].SetIcon( AtavismPrefabManager.Instance.GetClassIconByID(selectedRaceId, classData.id));
                    uiClassSlots[g].Unselect();
                    uiClassSlots[g].Show();
                    if(classData.id == lastSelectedClass)
                        found = true;
                }

                g++;
            }
            for (int gg = g; gg < uiClassSlots.Count; gg++)
            {
                uiClassSlots[gg].Hide();
            }

            if (!found)
            {
                lastSelectedClass = UnityEngine.Random.Range(0, lastAvaiableClass);
            }
            if (lastSelectedClass != -1 )
                SelectClassSlot(lastSelectedClass);
            else
                SelectClassSlot(UnityEngine.Random.Range(0, classes.Count));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        public void SelectClassSlot(UICreateCharacterSlot slot)
        {
            int index = -1;

            for (int n = 0; n < uiClassSlots.Count; n++)
            {
                uiClassSlots[n].Unselect();

                if (uiClassSlots[n] == slot)
                    index = n;
            }

            slot.Select();
            selectedClassId = Convert.ToInt32(slot.viewDataKey);
            selectedClassData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[selectedClassId];

            UpdateData_ClassDescriptions();
            UpdateData_GenderSlots();

            lastSelectedClass = index;
        }
        public void SelectClassSlot(int index)
        {
            for (int n = 0; n < uiClassSlots.Count; n++)
                uiClassSlots[n].Unselect();

            uiClassSlots[index].Select();
            selectedClassId = Convert.ToInt32(uiClassSlots[index].viewDataKey);
            selectedClassData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[selectedClassId];

            UpdateData_ClassDescriptions();
            UpdateData_GenderSlots();

            lastSelectedClass = index;
        }
        public void UpdateData_ClassDescriptions()
        {
            uiClassDescriptionIcon.style.backgroundImage = new StyleBackground(selectedClassData.icon);
            uiClassDescriptionLabel.text = selectedClassData.description;
        }
        #endregion
        #region Gender
        public void UpdateData_GenderSlots()
        {
            // Genders
            
            int g = 0;
            var genders = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[selectedClassId].genderList.Keys.ToList();
            genders.Sort();
            foreach (var genderId in genders)
            {
                if (uiGenderSlots.Count > g)
                {
                    var genderData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[selectedClassId].genderList[genderId];
                    uiGenderSlots[g].viewDataKey = genderData.id.ToString();
                    uiGenderSlots[g].SetIcon( AtavismPrefabManager.Instance.GetGenderIconByID(selectedRaceId, selectedClassId, genderData.id));
                    uiGenderSlots[g].Unselect();
                    uiGenderSlots[g].Show();
                }

                g++;
            }
           

            for (int gg = g; gg < uiGenderSlots.Count; gg++)
            {
                uiGenderSlots[gg].Hide();
            }

            if (lastSelectedGender != -1)
                SelectGenderSlot(lastSelectedGender);
            else
                SelectGenderSlot(UnityEngine.Random.Range(0, genders.Count));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        public virtual void SelectGenderSlot(UICreateCharacterSlot slot)
        {
            int index = -1;
            for (int n = 0; n < uiGenderSlots.Count; n++)
            {
                if (uiGenderSlots[n] == slot)
                    index = n;
                uiGenderSlots[n].Unselect();
            }

            slot.Select();
            selectedGenderId = Convert.ToInt32(slot.viewDataKey);
            selectedGenderData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[selectedClassId].genderList[selectedGenderId];

            UpdateData_AvatarSlots();

            lastSelectedGender = index;
        }
        public virtual void SelectGenderSlot(int index)
        {
            for (int n = 0; n < uiGenderSlots.Count; n++)
                uiGenderSlots[n].Unselect();

            uiGenderSlots[index].Select();
            selectedGenderId = Convert.ToInt32(uiGenderSlots[index].viewDataKey);
            selectedGenderData = AtavismPrefabManager.Instance.GetRaceData()[selectedRaceId].classList[selectedClassId].genderList[selectedGenderId];

            UpdateData_AvatarSlots();

            lastSelectedGender = index;
        }
        #endregion
        #region Avatar
        public void UpdateData_AvatarSlots()
        {
            Sprite[] avatars = AtavismSettings.Instance.Avatars(selectedRaceData.name, selectedGenderData.name, selectedClassData.name);
            for (int n = 0; n < uiAvatarSlots.Count; n++)
            {
                if (avatars != null && n < avatars.Length)
                {
                    uiAvatarSlots[n].viewDataKey = n.ToString();
                    uiAvatarSlots[n].SetIcon(avatars[n]);
                    uiAvatarSlots[n].Unselect();
                    uiAvatarSlots[n].Show();

                    continue;
                }

                uiAvatarSlots[n].Hide();
            }

            if (avatars != null && avatars.Length > 0)
            {
                if (lastSelectedAvatar != -1)
                    SelectAvatarSlot(lastSelectedAvatar);
                else
                    SelectAvatarSlot(UnityEngine.Random.Range(0, avatars.Length));
            }
            else SelectAvatarSlot(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        public virtual void SelectAvatarSlot(UICreateCharacterSlot slot)
        {
            for (int n = 0; n < uiAvatarSlots.Count; n++)
                uiAvatarSlots[n].Unselect();

            slot.Select();    
            uiSelectedAvatarSlot.Select();
            if (!string.IsNullOrEmpty(slot.viewDataKey))
                selectedAvatarIndex = Convert.ToInt32(slot.viewDataKey);
            else selectedAvatarIndex = -1;
            Sprite[] avatars = AtavismSettings.Instance.Avatars(selectedRaceData.name, selectedGenderData.name,
                selectedClassData.name);
            uiSelectedAvatarSlot.SetIcon(avatars[selectedAvatarIndex]);
            lastSelectedAvatar = selectedAvatarIndex;

            if(UIAtavismCharacterSelectionSceneManager.Instance!=null)
                UIAtavismCharacterSelectionSceneManager.Instance.ShowCharacterModel(selectedRaceId, selectedClassId,
                selectedGenderId);
        }

        public virtual void SelectAvatarSlot(int index)
        {
            for (int n = 0; n < uiAvatarSlots.Count; n++)
                uiAvatarSlots[n].Unselect();

            uiSelectedAvatarSlot.Select();
            if (index != -1)
            {
                uiAvatarSlots[index].Select();
                if (!string.IsNullOrEmpty(uiAvatarSlots[index].viewDataKey))
                    selectedAvatarIndex = Convert.ToInt32(uiAvatarSlots[index].viewDataKey);
                else selectedAvatarIndex = -1;
                if (selectedAvatarIndex > -1)
                {
                    Sprite[] avatars = AtavismSettings.Instance.Avatars(selectedRaceData.name, selectedGenderData.name,
                        selectedClassData.name);
                    uiSelectedAvatarSlot.SetIcon(avatars[selectedAvatarIndex]);
                }
            }

            lastSelectedAvatar = selectedAvatarIndex;
            if(UIAtavismCharacterSelectionSceneManager.Instance!=null)
            UIAtavismCharacterSelectionSceneManager.Instance.ShowCharacterModel(selectedRaceId, selectedClassId, selectedGenderId);
        }
        #endregion
        #region Create Character
        /// <summary>
        /// Sends the Create Character message to the server with a collection of properties
        /// to save to the new character.
        /// </summary>
        public virtual void CreateCharacter()
        {
            string errorMessage, dialogMessage;
            string characterName = uiCharacterNameTextField.text;
            string raceName = selectedRaceData.name;
            string className = selectedClassData.name;
            string genderName = selectedGenderData.name;
            int raceId = selectedRaceId;
            int aspectId = selectedClassId;
            int genderId = selectedGenderData.id;

            if (string.IsNullOrEmpty(characterName))
            {
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("You must set character name.");
                return;
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PROPS.CHARACTER_NAME, characterName);
            properties.Add(PROPS.CHARACTER_RACE, raceName);
            //properties.Add(PROPS.CHARACTER_CUSTOM(PROPS.CHARACTER_RACE_ID), raceId);
            properties.Add(PROPS.CHARACTER_CLASS, className);
            //properties.Add(PROPS.CHARACTER_CUSTOM(PROPS.CHARACTER_CLASS_ID), aspectId);
            properties.Add(PROPS.CHARACTER_GENDER, genderName);
            properties.Add(PROPS.CHARACTER_GENDER_ID, genderId);
            properties.Add("prefab", AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].genderList[genderId].prefab);

            if (PortraitManager.Instance.portraitType == PortraitType.Custom)
            {
                Sprite[] icons = AtavismSettings.Instance.Avatars(raceName, genderName, className);

                if (icons != null && icons.Length > 0)
                {
                    if (selectedAvatarIndex != -1)
                    {
                        Sprite avatarIcon = icons[selectedAvatarIndex];
                        if (avatarIcon != null)
                            properties.Add(PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT), avatarIcon.name);
                        else
                        {
                            Debug.LogError("CharacterSelectionCreationManager icons for " + raceName + " " + genderName + " " + className + " is null.");
                            //return;
                        }
                    }
                }
                else
                {
                    // Custom sprite from AtavismMobAppearance will be used
                }
            }
            else if (PortraitManager.Instance.portraitType == PortraitType.Class)
            {
                Sprite portraitSprite = PortraitManager.Instance.GetCharacterSelectionPortrait(genderId, raceId, aspectId, PortraitType.Class);
                properties.Add(PROPS.CHARACTER_CUSTOM(PROPS.PORTRAIT), portraitSprite.name);
            }

            GameObject character = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;

            
            GetCharacterParamsForCreation(properties, character);

            dialogMessage = "Please wait...";
#if AT_I2LOC_PRESET
            dialogMessage = I2.Loc.LocalizationManager.GetTranslation("Please wait...");
#endif
            if (uiMessageFieldLabel != null)
            {
                uiMessageFieldLabel.text = dialogMessage;
            }
            else
            {
              //  UIAtavismDialogPopupManager.Instance.ShowMessage(dialogMessage);
            }

            errorMessage = "";
            CharacterEntry characterSelected = AtavismClient.Instance.NetworkHelper.CreateCharacter(properties);
            if (characterSelected == null)
            {
                errorMessage = "Unknown Error";
            }
            else
            {
                if (!characterSelected.Status)
                {
                    if (characterSelected.ContainsKey("errorMessage"))
                    {
                        errorMessage = (string)characterSelected["errorMessage"];
                    }
                }
            }

            dialogMessage = "";

            if (errorMessage == "")
            {               
                // Have to rename all the properties. This seems kind of pointless.
                Dictionary<string, object> newProps = new Dictionary<string, object>();
                foreach (string prop in properties.Keys)
                {
                    if (prop.Contains(":"))
                    {
                        string[] newPropParts = prop.Split(':');
                        if (newPropParts.Length > 2 && newPropParts[2] != null)
                        {
                            string newProp = "uma" + newPropParts[2];
                            newProps.Add(newProp, properties[prop]);
                        }
                    }
                }

                foreach (string prop in newProps.Keys)
                {
                    if (!characterSelected.ContainsKey(prop))
                        characterSelected.Add(prop, newProps[prop]);
                }

                UIAtavismCharacterSelectionSceneManager.Instance.SelectCharacterItemSlot(characterSelected);
                dialogMessage = "Character has been succesfully created.";
                if (uiMessageFieldLabel != null)
                {
                    uiMessageFieldLabel.text = dialogMessage;
                }

                Hide();
                // UIAtavismDialogPopupManager.Instance.ShowDialogPopup(dialogMessage);
            }
            else
            {
                dialogMessage = errorMessage;
#if AT_I2LOC_PRESET
                dialogMessage = I2.Loc.LocalizationManager.GetTranslation(errorMessage);         
#endif
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup(dialogMessage);
            }
        }
        
        public virtual void GetCharacterParamsForCreation(Dictionary<string, object> properties, GameObject character)
        {
            // If the character has the customisable hair, save the property
            if (character.GetComponent<CustomisedHair>() != null)
            {
                CustomisedHair customHair = character.GetComponent<CustomisedHair>();
                properties.Add("custom:" + customHair.hairPropertyName, customHair.ActiveHair.name);
            }
            
        }

        
        #endregion
    }
}