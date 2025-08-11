using Atavism.UI.Game;
using HNGamers.Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using Atavism.UI;
using UMA;
using UMA.CharacterSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    /// <summary>
    /// soft bug: last color is saved as Color and when change the race, race can have different color set but is applied last saved Color, but race may not have that Color in the color set
    /// Fix is simple, instead of the Color save the Color index - ToDo sometimes
    /// </summary>
    public class UIAtavismCharacterCreationManagerUMA : UIAtavismCharacterCreationManager
    {
        [System.Serializable]
        public class DNADisplay
        {
            public string DnaName;
            public string DisplayedName;
            public List<string> CompatibleRaces;
        }
        [Serializable]
        public class DNADisplayColors
        {
            public string ColorType;
            public string DisplayedName;
            public List<Color> colors = new List<Color>();
        }
        [Serializable]
        public class UmaCustomizerPanel
        {
            public string PanelName;
            public List<DNADisplay> dnaList;
            public List<DNADisplayColors> dnaColorsList;
            public List<DNADisplaySlots> dnaSlotsList;
        }

        [System.Serializable]
        public class DNADisplaySlots
        {
            public string slotType;
            public string slotName;
            public List<RacialAlternatives> alternatives = new List<RacialAlternatives>();
        }

        
        [AtavismSeparator("Customizer")]
        [SerializeField] List<RaceRecipe> noviceRecipe = new List<RaceRecipe>();
        [SerializeField] List<RaceRecipe> LeatherRecipe = new List<RaceRecipe>();
        [SerializeField] List<RaceRecipe> ClothRecipe = new List<RaceRecipe>();
        [SerializeField] List<RaceRecipe> PlateRecipe = new List<RaceRecipe>();
        [SerializeField] private VisualTreeAsset dnaSliderPrefab;
        [SerializeField] private VisualTreeAsset dnaColorPrefab;
        [SerializeField] private VisualTreeAsset colorPickerPrefab;
        [SerializeField] private UmaCustomizerPanel[] customizer;
        // [SerializeField] private CustomizerPanel[] customizer;

        private List<UICustomizerSliderPropertyController> listofCustomizerDnaSliders;
        private List<UICustomizerSliderPropertyController> listofCustomizerSliders;
        private List<UICustomizerColorPropertyController> listofCustomizerColors;
        // private Color lastHairColor, lastStubbleColor, lastSkinColor, lastEyeColor, lastScarColor, lastBodyArtColor, lastBeardColor, lastEyebrowColor, lastMouthColor;
        // private int lastHairId, lastBeardId, lastFaceId, lastEyebrowId, lastHandId, lastLowerArmId, lastUpperArmId, lastTorsoId, lastHipId, lastLowerLegId, lastEyeId, lastMouthId, lastHeadId, lastFeetId;

        private Button uiNoviceButton, uiLeatherButton, uiPlateButton, uiNoGearButton, uiChampionButton, uiClothButton, uiRandomButton;
        string gearClicked = "NoviceGear";
        
        Dictionary<string, VisualElement> customizerUmaPanel = new Dictionary<string, VisualElement>();
        [SerializeField] private bool autoZoomBodyTab = true;
        [SerializeField] private bool autoZoomHeadTab = true;
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            uiNoviceButton = uiDocument.rootVisualElement.Query<Button>("novice-button");
            uiChampionButton = uiDocument.rootVisualElement.Query<Button>("champion-button");
            uiNoGearButton = uiDocument.rootVisualElement.Query<Button>("no-gear-button");
            uiClothButton = uiDocument.rootVisualElement.Query<Button>("cloth-button");
            uiLeatherButton = uiDocument.rootVisualElement.Query<Button>("leather-button");
            uiPlateButton = uiDocument.rootVisualElement.Query<Button>("plate-button");
            uiRandomButton = uiDocument.rootVisualElement.Query<Button>("random-button");

            uiNoviceButton.clicked += ClickNoviceGear;
            uiChampionButton.clicked += ClickChampionGear;
            uiNoGearButton.clicked += ClickNoGear;
            uiClothButton.clicked += ClickClothGear;
            uiLeatherButton.clicked += ClickLeatherGear;
            uiPlateButton.clicked += ClickPlateGear;
            uiRandomButton.clicked += ClickRandom;

            string items = "";
            for (int n = 0; n < customizer.Length; n++)
            {
                if (n == 0)
                    items += customizer[n].PanelName;
                else items += "," + customizer[n].PanelName;
            }
            listofCustomizerDnaSliders = new List<UICustomizerSliderPropertyController>();
            listofCustomizerSliders = new List<UICustomizerSliderPropertyController>();
            listofCustomizerColors = new List<UICustomizerColorPropertyController>();
            uiCustomizerPanel.ContentWithScroll = true;
            uiCustomizerPanel.CreateButtons(items);
            for (int n = 0; n < customizer.Length; n++)
            {
                 if (customizer[n].dnaList != null)
                    for (int i = 0; i < customizer[n].dnaList.Count; i++)
                    {
                        VisualElement e = dnaSliderPrefab.CloneTree().contentContainer;
                        UICustomizerSliderPropertyController sliderItem =
                            e.Query<UICustomizerSliderPropertyController>();
                        sliderItem.SetDisplayName(customizer[n].dnaList[i].DisplayedName);
                        sliderItem.SetPropertyName(customizer[n].dnaList[i].DnaName);
                        sliderItem.userData = customizer[n].dnaList[i];
                        sliderItem.SetRange(0, 1000);
                        sliderItem.SetValue(500,false);
                        sliderItem.UpdateData();

                        uiCustomizerPanel.AddItem(e, n);
                        listofCustomizerDnaSliders.Add(sliderItem);

                        sliderItem.OnValueChanged += OnSliderDNAValueChanged;
                    }
                 if (customizer[n].dnaColorsList != null)
                for (int i = 0; i < customizer[n].dnaColorsList.Count; i++)
                {
                    VisualElement e = dnaColorPrefab.CloneTree().contentContainer;
                    UICustomizerColorPropertyController colorItem = e.Query<UICustomizerColorPropertyController>();
                    colorItem.SetDisplayName(customizer[n].dnaColorsList[i].DisplayedName);
                    colorItem.SetPropertyName(customizer[n].dnaColorsList[i].ColorType);
                    colorItem.userData = customizer[n].dnaColorsList[i];
                    colorItem.SetColorsList(customizer[n].dnaColorsList[i].colors.ToArray());
                    colorItem.UpdateData();
                    uiCustomizerPanel.AddItem(e, n);
                    listofCustomizerColors.Add(colorItem);
                    colorItem.OnSelectedColorChanged += OnSelectedColorChanged;
                }
                 
                 if (customizer[n].dnaSlotsList != null)
                     for (int i = 0; i < customizer[n].dnaSlotsList.Count; i++)
                     {
                         VisualElement e = dnaSliderPrefab.CloneTree().contentContainer;
                         UICustomizerSliderPropertyController sliderItem =
                             e.Query<UICustomizerSliderPropertyController>();
                         sliderItem.SetDisplayName(customizer[n].dnaSlotsList[i].slotName);
                         sliderItem.SetPropertyName(customizer[n].dnaSlotsList[i].slotType);
                         sliderItem.userData = customizer[n].dnaSlotsList[i];
                         sliderItem.SetRange(0, customizer[n].dnaSlotsList[i].alternatives.Count - 1);
                         sliderItem.UpdateData();

                         uiCustomizerPanel.AddItem(e, n);
                         listofCustomizerSliders.Add(sliderItem);

                         sliderItem.OnValueChanged += OnSliderValueChanged;
                     }
                 
            }

            uiCustomizerPanel.OnItemIndexChanged += OnPanelChanged;

            isRegisteredUI = true;

            return true;
        }

   
        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            if (listofCustomizerDnaSliders != null)
            {
                for (int n = 0; n < listofCustomizerDnaSliders.Count; n++)
                {
                    listofCustomizerDnaSliders[n].OnValueChanged -= OnSliderDNAValueChanged;
                    listofCustomizerDnaSliders[n].RemoveFromHierarchy();
                }

                listofCustomizerDnaSliders.Clear();
            }

            if (listofCustomizerSliders != null)
            {
                for (int n = 0; n < listofCustomizerSliders.Count; n++)
                {
                    listofCustomizerSliders[n].OnValueChanged -= OnSliderValueChanged;
                    listofCustomizerSliders[n].RemoveFromHierarchy();
                }

                listofCustomizerSliders.Clear();
            }

            if (listofCustomizerColors != null)
            {
                for (int n = 0; n < listofCustomizerColors.Count; n++)
                {
                    listofCustomizerColors[n].OnSelectedColorChanged -= OnSelectedColorChanged;
                    listofCustomizerColors[n].RemoveFromHierarchy();
                }

                listofCustomizerColors.Clear();
            }
            uiCustomizerPanel.OnItemIndexChanged -= OnPanelChanged;

            isRegisteredUI = false;

            return true;
        }

        public override void Show()
        {
            base.Show();
            if (uiCustomizerPanel.ItemsArray != null && uiCustomizerPanel.ItemsArray.Length > 0)
                uiCustomizerPanel.SelectPanel(0);
        }

        public void UpdateDataCharacterControllers()
        {
            
        }

        
        void UpdateEquipDisplay(List<RaceRecipe> recipeList)
        {
            CharacterManager cm = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel.GetComponent<CharacterManager>();
            if (cm != null)
            {
                if (recipeList == null)
                {
                    cm.RequestSlot("FullOutfit", "");
                    return;
                }
                UMA.CharacterSystem.DynamicCharacterAvatar avatar = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
                if (avatar != null)
                    foreach (RaceRecipe raceRecipe in recipeList)
                    {
                        if (cm.avatar.activeRace.name == raceRecipe.race)
                        {
            
                            cm.RequestSlot("FullOutfit", raceRecipe.recipe);
                   
                        }
                    }
                //  cm.RequestSlot("FullOutfit", recipe);
            }
        }
#region Button Functions
        private void ClickRandom()
        {
            for (int n = 0; n < listofCustomizerDnaSliders.Count; n++)
            {
                listofCustomizerDnaSliders[n].SetRandomValue();
            }
            for (int n = 0; n < listofCustomizerSliders.Count; n++)
            {
                listofCustomizerSliders[n].SetRandomValue();
            }

            for (int n = 0; n < listofCustomizerColors.Count; n++)
            {
                listofCustomizerColors[n].SetRandomColor();
            }
        }

        private void ClickPlateGear()
        {
            gearClicked = "PlateGear";
            UpdateEquipDisplay(PlateRecipe);
            uiNoviceButton.RemoveFromClassList("selected-button");
            uiLeatherButton.RemoveFromClassList("selected-button");
            uiPlateButton.AddToClassList("selected-button");
            uiNoGearButton.RemoveFromClassList("selected-button");
            uiChampionButton.AddToClassList("selected-button");
            uiClothButton.RemoveFromClassList("selected-button");

        }

        private void ClickLeatherGear()
        {
            gearClicked = "LeatherGear";
            UpdateEquipDisplay(LeatherRecipe);
            uiNoviceButton.RemoveFromClassList("selected-button");
            uiLeatherButton.AddToClassList("selected-button");
            uiPlateButton.RemoveFromClassList("selected-button");
            uiNoGearButton.RemoveFromClassList("selected-button");
            uiChampionButton.AddToClassList("selected-button");
            uiClothButton.RemoveFromClassList("selected-button");

        }

        private void ClickClothGear()
        {
            gearClicked = "ClothGear";
            UpdateEquipDisplay(ClothRecipe);
            uiNoviceButton.RemoveFromClassList("selected-button");
            uiLeatherButton.RemoveFromClassList("selected-button");
            uiPlateButton.RemoveFromClassList("selected-button");
            uiNoGearButton.RemoveFromClassList("selected-button");
            uiChampionButton.AddToClassList("selected-button");
            uiClothButton.AddToClassList("selected-button");

        }

        private void ClickNoGear()
        {
            gearClicked = "NoGear";
            UpdateEquipDisplay(null);
            uiNoviceButton.RemoveFromClassList("selected-button");
            uiLeatherButton.RemoveFromClassList("selected-button");
            uiPlateButton.RemoveFromClassList("selected-button");
            uiNoGearButton.AddToClassList("selected-button");
            uiChampionButton.RemoveFromClassList("selected-button");
            uiClothButton.RemoveFromClassList("selected-button");
            uiClothButton.HideVisualElement();
            uiLeatherButton.HideVisualElement();
            uiPlateButton.HideVisualElement();
        }

        private void ClickChampionGear()
        {
            ClickClothGear();
            uiNoviceButton.RemoveFromClassList("selected-button");
            uiLeatherButton.RemoveFromClassList("selected-button");
            uiPlateButton.RemoveFromClassList("selected-button");
            uiNoGearButton.RemoveFromClassList("selected-button");
            uiChampionButton.AddToClassList("selected-button");
            uiClothButton.AddToClassList("selected-button");

            uiClothButton.ShowVisualElement();
            uiLeatherButton.ShowVisualElement();
            uiPlateButton.ShowVisualElement();
        }

        private void ClickNoviceGear()
        {
            gearClicked = "NoviceGear";
            UpdateEquipDisplay(noviceRecipe);
            uiNoviceButton.AddToClassList("selected-button");
            uiLeatherButton.RemoveFromClassList("selected-button");
            uiPlateButton.RemoveFromClassList("selected-button");
            uiNoGearButton.RemoveFromClassList("selected-button");
            uiChampionButton.RemoveFromClassList("selected-button");
            uiClothButton.RemoveFromClassList("selected-button");
           
            uiClothButton.HideVisualElement();
            uiLeatherButton.HideVisualElement();
            uiPlateButton.HideVisualElement();
        }
        #endregion

        public override void SelectGenderSlot(int slot)
        {
            base.SelectGenderSlot(slot);
            switch (gearClicked)
            {
                case "NoviceGear":
                    ClickNoviceGear();
                    break;
                case "ClothGear":
                    ClickClothGear();
                    break;
                case "PlateGear":
                    ClickPlateGear();
                    break;
                case "LeatherGear":
                    ClickLeatherGear();
                    break;
                case "NoGear":
                    ClickNoGear();
                    break;

            }

            UMA.CharacterSystem.DynamicCharacterAvatar avatar = UIAtavismCharacterSelectionSceneManager.Instance
                .CharacterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();

            if (avatar)
            {
                for (int n = 0; n < customizer.Length; n++)
                {
                    if (customizer[n].dnaList != null)
                        foreach (DNADisplay display in customizer[n].dnaList)
                        {
                         if(   display.CompatibleRaces.Contains(avatar.activeRace.name))
                         
                                {
                                    foreach (var slider in listofCustomizerDnaSliders)
                                    {
                                        if (slider.PropertyName.Equals(display.DnaName))
                                        {
                                            slider.ShowVisualElement();
                                            slider.SetValue(500,false);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var slider in listofCustomizerDnaSliders)
                                    {
                                        if (slider.PropertyName.Equals(display.DnaName))
                                        {
                                            slider.HideVisualElement();
                                        }
                                    }

                                }
                        }
                }
                for (int n = 0; n < listofCustomizerSliders.Count; n++)
                    listofCustomizerSliders[n].SetValue(0, false);
                for (int n = 0; n < listofCustomizerColors.Count; n++)
                    listofCustomizerColors[n].SetRandomColor();
            }
            
        }

        public override void SelectGenderSlot(UICreateCharacterSlot slot)
        {
            base.SelectGenderSlot(slot);
            switch (gearClicked)
            {
                case "NoviceGear":
                    ClickNoviceGear();
                    break;
                case "ClothGear":
                    ClickClothGear();
                    break;
                case "PlateGear":
                    ClickPlateGear();
                    break;
                case "LeatherGear":
                    ClickLeatherGear();
                    break;
                case "NoGear":
                    ClickNoGear();
                    break;

            }
            UMA.CharacterSystem.DynamicCharacterAvatar avatar = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
            
            
            if (avatar)
            {
                for (int n = 0; n < customizer.Length; n++)
                {
                    if (customizer[n].dnaList != null)
                        foreach (DNADisplay display in customizer[n].dnaList)
                        {
                            if(   display.CompatibleRaces.Contains(avatar.activeRace.name))
                         
                            {
                                foreach (var slider in listofCustomizerDnaSliders)
                                {
                                    if (slider.PropertyName.Equals(display.DnaName))
                                    {
                                        slider.ShowVisualElement();
                                        slider.SetValue(500,false);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var slider in listofCustomizerDnaSliders)
                                {
                                    if (slider.PropertyName.Equals(display.DnaName))
                                    {
                                        slider.HideVisualElement();
                                    }
                                }

                            }
                        }
                }
                for (int n = 0; n < listofCustomizerSliders.Count; n++)
                    listofCustomizerSliders[n].SetValue(0,false);
                for (int n = 0; n < listofCustomizerColors.Count; n++)
                    listofCustomizerColors[n].SetRandomColor();
            }
            
        
        }


        #region Panels
        protected virtual void OnPanelChanged(int index)
        {
           // Debug.LogError("OnPanelChanged "+index);
            if (index == 0)
            {
                
                if(autoZoomBodyTab)
                    UIAtavismCharacterSelectionSceneManager.Instance.ZoomBody();
            }
            else
            {
                if (autoZoomHeadTab)
                    UIAtavismCharacterSelectionSceneManager.Instance.ZoomHead();
            }
        }
        #endregion
        #region Sliders
    
        private void OnSliderDNAValueChanged(int value, string propertyName)
        {
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }
            float val = value / 1000f;
            UMA.CharacterSystem.DynamicCharacterAvatar avatar = characterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
            if (avatar != null)
            {
              Dictionary<string, DnaSetter>  dna = avatar.GetDNA();
                if(dna.ContainsKey(propertyName))
                    dna[propertyName].Set(val);
                else
                    Debug.LogWarning("Cant find " + name + " in dna of the race " + avatar.activeRace.name );
                avatar.BuildCharacter();
            }
            
        }

        protected virtual void OnSliderValueChanged(int selection, string propertyName)
        {
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }

            
            UMA.CharacterSystem.DynamicCharacterAvatar avatar =
                characterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
            if (avatar != null)
            {
                for (int n = 0; n < customizer.Length; n++)
                {
                    if (customizer[n].dnaSlotsList != null)
                        foreach (var display in customizer[n].dnaSlotsList)
                        {
                            if (display.slotType.Equals(propertyName))
                            {
                                if (display.alternatives.Count > selection)
                                    foreach (RaceRecipe raceRecipe in display.alternatives[selection].raceRecipes)
                                    {
                                        if (raceRecipe.race == avatar.activeRace.name)
                                        {

                                            Debug.Log(display.slotType + " " + raceRecipe.recipe);
                                            if (raceRecipe.recipe == "")
                                            {
                                                avatar.ClearSlot(display.slotType);
                                                avatar.BuildCharacter();
                                            }
                                            else
                                            {
                                                avatar.SetSlot(display.slotType, raceRecipe.recipe);
                                                avatar.BuildCharacter();
                                            }

                                        }
                                    }
                            }
                        }
                }
            }
        }

        #endregion
        #region Colors
     
        protected virtual void OnSelectedColorChanged(Color color, string propertyName)
        {
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }
            
            
            UMA.CharacterSystem.DynamicCharacterAvatar avatar = characterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();

            if (avatar != null)
            {
                for (int n = 0; n < customizer.Length; n++)
                {
                    if (customizer[n].dnaColorsList != null)
                        foreach (var display in customizer[n].dnaColorsList)
                        {
                            if (display.ColorType.Equals(propertyName))
                            {
                                avatar.SetColor(propertyName, color);
                                avatar.UpdateColors(true);
                            }
                        }
                }
            }
        }
        #endregion
 #region Create Character
       
        public override void GetCharacterParamsForCreation(Dictionary<string, object> properties, GameObject character)
        {
           
            // If the character has the customisable hair, save the property
            // if (character.GetComponent<CustomisedHair>() != null)
            // {
            //     CustomisedHair customHair = character.GetComponent<CustomisedHair>();
            //     properties.Add("custom:" + customHair.hairPropertyName, customHair.ActiveHair.name);
            // }

            //UMA Custominzation
            UMA.CharacterSystem.DynamicCharacterAvatar avatar = character.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
            if(avatar != null)
            {
                avatar.ClearSlot("FullOutfit");
                avatar.BuildCharacter();

                string recipe = avatar.GetCurrentRecipe();
                properties.Add("custom:umaData:CurrentRecipe", recipe);
            }
        }

        #endregion
        
        
        
        
        public override void SelectAvatarSlot(int index)
        {
            base.SelectAvatarSlot(index);

            UpdateDataCharacterControllers();
        }

        public override void SelectAvatarSlot(UICreateCharacterSlot slot)
        {
            base.SelectAvatarSlot(slot);

            UpdateDataCharacterControllers();
        }
        
        public static string ToRGBHex(Color c)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}