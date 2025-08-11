using Atavism.UI.Game;
using HNGamers.Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    /// <summary>
    /// soft bug: last color is saved as Color and when change the race, race can have different color set but is applied last saved Color, but race may not have that Color in the color set
    /// Fix is simple, instead of the Color save the Color index - ToDo sometimes
    /// </summary>
    public class UIAtavismCharacterCreationManagerNonUMA : UIAtavismCharacterCreationManager
    {
        public enum CustomizerItemTypeEnum { Slider, Color/*, ColorPicker */}
        public enum CustomizerBodyPartEnum { none, Hair, Beard, Face, Eyebrow, Hands, LowerArms, UpperArms, Torso, Hips, LowerLegs, Eyes, Mouth, Head, Feet };
        public enum CustomizerColorTypeEnum { none, HairColor, StubbleColor, SkinColor, EyeColor, ScarColor, BodyArtColor, BeardColor, EyebrowColor, MouthColor };


        [System.Serializable]
        public class CustomizerItem
        {
            public string DisplayName;
            public CustomizerItemTypeEnum ItemType;
            public CustomizerColorTypeEnum ColorType;
            public CustomizerBodyPartEnum BodyPart;
        }

        [System.Serializable]
        public class CustomizerPanel
        {
            public string PanelName;
            public CustomizerItem[] items;
        }

        [AtavismSeparator("Customizer")]
        [SerializeField] private VisualTreeAsset dnaSliderPrefab;
        [SerializeField] private VisualTreeAsset dnaColorPrefab;
        [SerializeField] private VisualTreeAsset colorPickerPrefab;
        [SerializeField] private CustomizerPanel[] customizer;

        private List<UICustomizerSliderPropertyController> listofCustomizerSliders;
        private List<UICustomizerColorPropertyController> listofCustomizerColors;
        private Color lastHairColor, lastStubbleColor, lastSkinColor, lastEyeColor, lastScarColor, lastBodyArtColor, lastBeardColor, lastEyebrowColor, lastMouthColor;
        private int lastHairId, lastBeardId, lastFaceId, lastEyebrowId, lastHandId, lastLowerArmId, lastUpperArmId, lastTorsoId, lastHipId, lastLowerLegId, lastEyeId, lastMouthId, lastHeadId, lastFeetId;

        [SerializeField] private bool autoZoomBodyTab = true;
        [SerializeField] private bool autoZoomHeadTab = true;
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            string items = "";
            for (int n = 0; n < customizer.Length; n++)
            {
                if (n == 0)
                    items += customizer[n].PanelName;
                else items += "," + customizer[n].PanelName;
            }

            listofCustomizerSliders = new List<UICustomizerSliderPropertyController>();
            listofCustomizerColors = new List<UICustomizerColorPropertyController>();
            uiCustomizerPanel.ContentWithScroll = true;
            uiCustomizerPanel.CreateButtons(items);
            for (int n = 0; n < customizer.Length; n++)
            {
                if (customizer[n].items != null)
                    for (int i = 0; i < customizer[n].items.Length; i++)
                    {
                        if (customizer[n].items[i].ItemType == CustomizerItemTypeEnum.Slider)
                        {
                            VisualElement e = dnaSliderPrefab.CloneTree().contentContainer;
                            UICustomizerSliderPropertyController sliderItem = e.Query<UICustomizerSliderPropertyController>();
                            sliderItem.SetDisplayName(customizer[n].items[i].DisplayName);
                            sliderItem.SetPropertyName(customizer[n].items[i].BodyPart.ToString());
                            sliderItem.UpdateData();

                            uiCustomizerPanel.AddItem(e, n);
                            listofCustomizerSliders.Add(sliderItem);

                            sliderItem.OnValueChanged += OnSliderValueChanged;
                        }

                        if (customizer[n].items[i].ItemType == CustomizerItemTypeEnum.Color)
                        {
                            VisualElement e = dnaColorPrefab.CloneTree().contentContainer;
                            UICustomizerColorPropertyController colorItem = e.Query<UICustomizerColorPropertyController>();
                            colorItem.SetDisplayName(customizer[n].items[i].DisplayName);
                            colorItem.SetPropertyName(customizer[n].items[i].ColorType.ToString());
                            colorItem.UpdateData();

                            uiCustomizerPanel.AddItem(e, n);
                            listofCustomizerColors.Add(colorItem);

                            colorItem.OnSelectedColorChanged += OnSelectedColorChanged;
                        }
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

            lastHairId = lastBeardId = lastFaceId = lastEyebrowId = lastHandId = lastLowerArmId = lastUpperArmId = lastTorsoId = lastHipId = lastLowerLegId = lastEyeId = lastMouthId = lastHeadId = lastFeetId = -1;

            if (uiCustomizerPanel.ItemsArray != null && uiCustomizerPanel.ItemsArray.Length > 0)
                uiCustomizerPanel.SelectPanel(0);
        }

        public void UpdateDataModularCharacterControllers()
        {
            for (int n = 0; n < listofCustomizerSliders.Count; n++)
                updateDataModularCharacterSlider(listofCustomizerSliders[n]);
            for (int n = 0; n < listofCustomizerColors.Count; n++)
                updateDataModularCharacterColor(listofCustomizerColors[n]);
        }

        #region Panels
        protected virtual void OnPanelChanged(int index)
        {
            if (UIAtavismCharacterSelectionSceneManager.Instance == null)
                return;
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

        public override void GetCharacterParamsForCreation(Dictionary<string, object> properties, GameObject character)
        {
            // If the character has the customisable hair, save the property
            if (character.GetComponent<CustomisedHair>() != null)
            {
                CustomisedHair customHair = character.GetComponent<CustomisedHair>();
                properties.Add("custom:" + customHair.hairPropertyName, customHair.ActiveHair.name);
            }

            // Modular customization
            ModularCustomizationManager modularCustomizationManager =
                character.GetComponent<ModularCustomizationManager>();
            if (modularCustomizationManager != null)
            {
                properties.Add("custom:EyeMaterial", modularCustomizationManager.ActiveEyeMaterialId);
                properties.Add("custom:HairMaterial", modularCustomizationManager.ActiveHairMaterialId);
                properties.Add("custom:SkinMaterial", modularCustomizationManager.ActiveSkinMaterialId);
                properties.Add("custom:MouthMaterial", modularCustomizationManager.ActiveMouthMaterialId);

                if (modularCustomizationManager.ActiveHair)
                {
                    properties.Add("custom:HairModel", modularCustomizationManager.ActiveHairId);
                    properties.Add("custom:HairColor",
                        String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveHairColor.r,
                            modularCustomizationManager.ActiveHairColor.g,
                            modularCustomizationManager.ActiveHairColor.b,
                            modularCustomizationManager.ActiveHairColor
                                .a)); //modularCustomizationManager.ActiveHairColor.ToString());
                }

                if (modularCustomizationManager.allowDifferentHairColors)
                {
                    properties.Add("custom:BeardColor",
                        String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveBeardColor.r,
                            modularCustomizationManager.ActiveBeardColor.g,
                            modularCustomizationManager.ActiveBeardColor.b,
                            modularCustomizationManager.ActiveBeardColor
                                .a)); //modularCustomizationManager.ActiveHairColor.ToString());
                    properties.Add("custom:EyeBrowColor",
                        String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveEyebrowColor.r,
                            modularCustomizationManager.ActiveEyebrowColor.g,
                            modularCustomizationManager.ActiveEyebrowColor.b,
                            modularCustomizationManager.ActiveEyebrowColor
                                .a)); //modularCustomizationManager.ActiveHairColor.ToString());
                }

                properties.Add("custom:BodyColor",
                    String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveBodyColor.r,
                        modularCustomizationManager.ActiveBodyColor.g, modularCustomizationManager.ActiveBodyColor.b,
                        modularCustomizationManager.ActiveBodyColor.a));
                properties.Add("custom:ScarColor",
                    String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveScarColor.r,
                        modularCustomizationManager.ActiveScarColor.g, modularCustomizationManager.ActiveScarColor.b,
                        modularCustomizationManager.ActiveScarColor
                            .a)); //modularCustomizationManager.ActiveScarColor.ToString());
                properties.Add("custom:StubbleColor",
                    String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveStubbleColor.r,
                        modularCustomizationManager.ActiveStubbleColor.g,
                        modularCustomizationManager.ActiveStubbleColor.b,
                        modularCustomizationManager.ActiveStubbleColor
                            .a)); //modularCustomizationManager.ActiveBeardColor.ToString());
                properties.Add("custom:BodyArtColor",
                    String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveBodyArtColor.r,
                        modularCustomizationManager.ActiveBodyArtColor.g,
                        modularCustomizationManager.ActiveBodyArtColor.b,
                        modularCustomizationManager.ActiveBodyArtColor
                            .a)); //modularCustomizationManager.ActiveBeardColor.ToString());
                properties.Add("custom:EyeColor",
                    String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveEyeColor.r,
                        modularCustomizationManager.ActiveEyeColor.g, modularCustomizationManager.ActiveEyeColor.b,
                        modularCustomizationManager.ActiveEyeColor
                            .a)); //modularCustomizationManager.ActiveEyeColor.ToString());
                properties.Add("custom:MouthColor",
                    String.Format("{0},{1},{2},{3}", modularCustomizationManager.ActiveMouthColor.r,
                        modularCustomizationManager.ActiveMouthColor.g, modularCustomizationManager.ActiveMouthColor.b,
                        modularCustomizationManager.ActiveMouthColor
                            .a)); //modularCustomizationManager.ActiveEyeColor.ToString());

                if (modularCustomizationManager.ActiveHelmetColor != null)
                {
                    properties.Add("custom:HelmetColor", modularCustomizationManager.ActiveHelmetColor);
                }

                if (modularCustomizationManager.ActiveHeadColor != null)
                {
                    properties.Add("custom:HeadColor", modularCustomizationManager.ActiveHeadColor);
                }

                if (modularCustomizationManager.ActiveTorsoColor != null)
                {
                    properties.Add("custom:TorsoColor", modularCustomizationManager.ActiveTorsoColor);
                }

                if (modularCustomizationManager.ActiveUpperArmsColor != null)
                {
                    properties.Add("custom:UpperArmsColor", modularCustomizationManager.ActiveUpperArmsColor);
                }

                if (modularCustomizationManager.ActiveLowerArmsColor != null)
                {
                    properties.Add("custom:LowerArmsColor", modularCustomizationManager.ActiveLowerArmsColor);
                }

                if (modularCustomizationManager.ActiveHipsColor != null)
                {
                    properties.Add("custom:HipsColor", modularCustomizationManager.ActiveHipsColor);
                }

                if (modularCustomizationManager.ActiveLowerLegsColor != null)
                {
                    properties.Add("custom:LowerLegsColor", modularCustomizationManager.ActiveLowerLegsColor);
                }

                if (modularCustomizationManager.ActiveHandsColor != null)
                {
                    properties.Add("custom:HandsColor", modularCustomizationManager.ActiveHandsColor);
                }

                if (modularCustomizationManager.ActiveHands.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveHands)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:HandsModel", data);
                }

                if (modularCustomizationManager.ActiveLowerArms.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveLowerArms)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:LowerArmsModel", data);
                    //   properties.Add("custom:LowerArmsModel", modularCustomizationManager.ActiveLowerArms.name);
                }

                if (modularCustomizationManager.ActiveUpperArms.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveUpperArms)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:UpperArmsModel", data);
                    //  properties.Add("custom:UpperArmsModel", modularCustomizationManager.ActiveUpperArms.name);
                }

                if (modularCustomizationManager.ActiveTorso.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveTorso)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:TorsoModel", data);
                    // properties.Add("custom:TorsoModel", modularCustomizationManager.ActiveTorso.name);
                }

                if (modularCustomizationManager.ActiveHips.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveHips)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:HipsModel", data);
                    //  properties.Add("custom:HipsModel", modularCustomizationManager.ActiveHips.name);
                }

                if (modularCustomizationManager.ActiveLowerLegs.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveLowerLegs)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:LowerLegsModel", data);
                    //   properties.Add("custom:LowerLegsModel", modularCustomizationManager.ActiveLowerLegs.name);
                }

                if (modularCustomizationManager.ActiveFeet.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveFeet)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:FeetModel", data);
                    //   properties.Add("custom:FeetModel", modularCustomizationManager.ActiveFeet.name);
                }

                if (modularCustomizationManager.ActiveHead.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveHead)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:HeadModel", data);
                    //   properties.Add("custom:HeadModel", modularCustomizationManager.ActiveHead.name);
                }

                if (modularCustomizationManager.ActiveBeard)
                {
                    properties.Add("custom:BeardModel", modularCustomizationManager.ActiveBeardId);
                }

                if (modularCustomizationManager.ActiveEyebrow)
                {
                    properties.Add("custom:EyebrowModel", modularCustomizationManager.ActiveEyebrowId);
                }

                if (modularCustomizationManager.ActiveMouth)
                {
                    properties.Add("custom:MouthModel", modularCustomizationManager.ActiveMouthId);
                }

                if (modularCustomizationManager.ActiveEye)
                {
                    properties.Add("custom:EyeModel", modularCustomizationManager.ActiveEyeId);
                }

                if (modularCustomizationManager.ActiveTusk.Count > 0)
                {
                    string data = "";
                    foreach (var go in modularCustomizationManager.ActiveTusk)
                    {
                        data += go.name + "|";
                    }

                    if (data.Length > 0)
                        data = data.Remove(data.Length - 1);
                    properties.Add("custom:TuskModel", data);
                    properties.Add("custom:TuskModel", modularCustomizationManager.ActiveTuskId);
                }

                if (modularCustomizationManager.ActiveEar)
                {
                    properties.Add("custom:EarModel", modularCustomizationManager.ActiveEarId);
                }

                if (modularCustomizationManager.ActiveFaith != null)
                {
                    properties.Add("custom:FaithValue", modularCustomizationManager.ActiveFaith);
                }
#if IPBRInt
                if (modularCustomizationManager.ActiveBlendshapePreset != -1)
                {
                    properties.Add("custom:BlendshapePresetValue", modularCustomizationManager.ActiveBlendshapePreset);
                }

                if (modularCustomizationManager.ActiveBlendshapes != null)
                {
                    properties.Add("custom:BlendshapesValue", modularCustomizationManager.ActiveBlendshapes);
                }
#endif
            }

        }

        protected virtual void updateDataModularCharacterSlider(UICustomizerSliderPropertyController slider)
        {
            if (UIAtavismCharacterSelectionSceneManager.Instance == null)
                return;
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }
            ModularCustomizationManager modularCustomizationManager = characterModel.GetComponent<ModularCustomizationManager>();
            if (modularCustomizationManager == null)
            {
                Debug.LogWarning("Character model does not have ModularCustomizationManager script");
                return;
            }

            CustomizerBodyPartEnum bodyType = Enum.Parse<CustomizerBodyPartEnum>(slider.PropertyName);

            switch (bodyType)
            {
                case CustomizerBodyPartEnum.Beard: 
                    slider.SetRange(0, modularCustomizationManager.beardModels.Count - 1);
                    slider.UpdateData();

                    if (lastBeardId != -1)
                        slider.SetValue(lastBeardId);
                    else slider.SetRandomValue();
                    
                    break;
                case CustomizerBodyPartEnum.Eyebrow: 
                    slider.SetRange(0, modularCustomizationManager.eyebrowModels.Count - 1);
                    slider.UpdateData();

                    if (lastEyebrowId != -1)
                        slider.SetValue(lastEyebrowId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Eyes: 
                    slider.SetRange(0, modularCustomizationManager.eyeModels.Count - 1);
                    slider.UpdateData();

                    if (lastEyeId != -1)
                        slider.SetValue(lastEyeId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Face: 
                    slider.SetRange(0, modularCustomizationManager.faceModels.Count - 1);
                    slider.UpdateData();

                    if (lastFaceId != -1)
                        slider.SetValue(lastFaceId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Feet: 
                    slider.SetRange(0, modularCustomizationManager.feetModels.Count - 1);
                    slider.UpdateData();

                    if (lastFeetId != -1)
                        slider.SetValue(lastFeetId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Hair: 
                    slider.SetRange(0, modularCustomizationManager.hairModels.Count - 1);
                    slider.UpdateData();

                    if (lastHairId != -1)
                        slider.SetValue(lastHairId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Hands: 
                    slider.SetRange(0, modularCustomizationManager.handModels.Count - 1);
                    slider.UpdateData();

                    if (lastHandId != -1)
                        slider.SetValue(lastHandId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Head: 
                    slider.SetRange(0, modularCustomizationManager.headModels.Count - 1);
                    slider.UpdateData();

                    if (lastHeadId != -1)
                        slider.SetValue(lastHeadId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Hips: 
                    slider.SetRange(0, modularCustomizationManager.hipModels.Count - 1);
                    slider.UpdateData();

                    if (lastHipId != -1)
                        slider.SetValue(lastHipId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.LowerArms: 
                    slider.SetRange(0, modularCustomizationManager.lowerArmModels.Count - 1);
                    slider.UpdateData();

                    if (lastLowerArmId != -1)
                        slider.SetValue(lastLowerArmId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.LowerLegs: 
                    slider.SetRange(0, modularCustomizationManager.lowerLegModels.Count - 1);
                    slider.UpdateData();

                    if (lastLowerLegId != -1)
                        slider.SetValue(lastLowerLegId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Mouth: 
                    slider.SetRange(0, modularCustomizationManager.mouthModels.Count - 1);
                    slider.UpdateData();

                    if (lastMouthId != -1)
                        slider.SetValue(lastMouthId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.Torso: 
                    slider.SetRange(0, modularCustomizationManager.torsoModels.Count - 1);
                    slider.UpdateData();

                    if (lastTorsoId != -1)
                        slider.SetValue(lastTorsoId);
                    else slider.SetRandomValue();

                    break;
                case CustomizerBodyPartEnum.UpperArms: 
                    slider.SetRange(0, modularCustomizationManager.upperArmModels.Count - 1);
                    slider.UpdateData();

                    if (lastUpperArmId != -1)
                        slider.SetValue(lastUpperArmId);
                    else slider.SetRandomValue();

                    break;
            }
        }

        protected virtual void OnSliderValueChanged(int value, string propertyName)
        {
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }
            ModularCustomizationManager modularCustomizationManager = characterModel.GetComponent<ModularCustomizationManager>();
            if (modularCustomizationManager == null)
            {
                Debug.LogError("Character model does not have ModularCustomizationManager script");
                return;
            }

            CustomizerBodyPartEnum bodyType = Enum.Parse<CustomizerBodyPartEnum>(propertyName);

            switch (bodyType)
            {
                case CustomizerBodyPartEnum.Beard: lastBeardId = value; modularCustomizationManager.UpdateBeardModel(value); break;
                case CustomizerBodyPartEnum.Eyebrow: lastEyebrowId = value; modularCustomizationManager.UpdateEyebrowModel(value); break;
                case CustomizerBodyPartEnum.Eyes: lastEyeId = value; modularCustomizationManager.UpdateEyeModel(value); break;
                case CustomizerBodyPartEnum.Face: lastFaceId = value; modularCustomizationManager.SwitchFace(value); break;
                case CustomizerBodyPartEnum.Feet: lastFeetId = value; modularCustomizationManager.SwitchFeet(value); break;
                case CustomizerBodyPartEnum.Hair: lastHairId = value; modularCustomizationManager.UpdateHairModel(value); break;
                case CustomizerBodyPartEnum.Hands: lastHandId = value; modularCustomizationManager.SwitchHands(value); break;
                case CustomizerBodyPartEnum.Head: lastHeadId = value; modularCustomizationManager.SwitchHead(value); break;
                case CustomizerBodyPartEnum.Hips: lastHipId = value; modularCustomizationManager.SwitchHips(value); break;
                case CustomizerBodyPartEnum.LowerArms: lastLowerArmId = value; modularCustomizationManager.SwitchLowerArm(value); break;
                case CustomizerBodyPartEnum.LowerLegs: lastLowerLegId = value; modularCustomizationManager.SwitchLowerLegs(value); break;
                case CustomizerBodyPartEnum.Mouth: lastMouthId = value; modularCustomizationManager.UpdateMouthModel(value); break;
                case CustomizerBodyPartEnum.Torso: lastTorsoId = value; modularCustomizationManager.SwitchTorso(value); break;
                case CustomizerBodyPartEnum.UpperArms: lastUpperArmId = value; modularCustomizationManager.SwitchUpperArm(value); break;
            }
        }
        #endregion
        #region Colors
        protected virtual void updateDataModularCharacterColor(UICustomizerColorPropertyController colorController)
        { 
            if (UIAtavismCharacterSelectionSceneManager.Instance == null)
                return;
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }
            ModularCustomizationManager modularCustomizationManager = characterModel.GetComponent<ModularCustomizationManager>();
            if (modularCustomizationManager == null)
            {
                Debug.LogWarning("Character model does not have ModularCustomizationManager script");
                return;
            }

            CustomizerColorTypeEnum colorType = Enum.Parse<CustomizerColorTypeEnum>(colorController.PropertyName);

            switch (colorType)
            {
                case CustomizerColorTypeEnum.HairColor: 
                    colorController.SetColorsList(modularCustomizationManager.hairColors);
                    colorController.UpdateData();
                    
                    if (lastHairColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastHairColor);

                    break;
                case CustomizerColorTypeEnum.StubbleColor:
                    colorController.SetColorsList(modularCustomizationManager.stubbleColors);
                    colorController.UpdateData();

                    if (lastStubbleColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastStubbleColor);

                    break;
                case CustomizerColorTypeEnum.SkinColor:
                    colorController.SetColorsList(modularCustomizationManager.skinColors);
                    colorController.UpdateData();

                    if (lastSkinColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastSkinColor);

                    break;
                case CustomizerColorTypeEnum.EyeColor:
                    colorController.SetColorsList(modularCustomizationManager.eyeColors);
                    colorController.UpdateData();
                    
                    if (lastEyeColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastEyeColor);

                    break;
                case CustomizerColorTypeEnum.ScarColor:
                    colorController.SetColorsList(modularCustomizationManager.scarColors);
                    colorController.UpdateData();

                    if (lastScarColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastScarColor);

                    break;
                case CustomizerColorTypeEnum.BodyArtColor:
                    colorController.SetColorsList(modularCustomizationManager.bodyArtColors);
                    colorController.UpdateData();
                    
                    if (lastBodyArtColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastBodyArtColor);

                    break;
                case CustomizerColorTypeEnum.BeardColor:
                    if (lastBeardColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastBeardColor);

                    break;
                case CustomizerColorTypeEnum.EyebrowColor:
                    if (lastEyebrowColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastEyebrowColor);

                    break;
                case CustomizerColorTypeEnum.MouthColor:
                    if (lastMouthColor.a == 0f)
                        colorController.SetRandomColor();
                    else colorController.SetColor(lastMouthColor);

                    break;
            }
        }
        protected virtual void OnSelectedColorChanged(Color color, string propertyName)
        {
            GameObject characterModel = UIAtavismCharacterSelectionSceneManager.Instance.CharacterModel;
            if (characterModel == null)
            {
                Debug.LogError("Character model is null");
                return;
            }
            ModularCustomizationManager modularCustomizationManager = characterModel.GetComponent<ModularCustomizationManager>();
            if (modularCustomizationManager == null)
            {
                Debug.LogError("Character model does not have ModularCustomizationManager script");
                return;
            }

            CustomizerColorTypeEnum colorType = Enum.Parse<CustomizerColorTypeEnum>(propertyName);

            switch (colorType)
            {
                case CustomizerColorTypeEnum.HairColor: modularCustomizationManager.UpdateHairColor(color); lastHairColor = color; break;
                case CustomizerColorTypeEnum.StubbleColor: modularCustomizationManager.UpdateStubbleColor(color); lastStubbleColor = color; break;
                case CustomizerColorTypeEnum.SkinColor: modularCustomizationManager.UpdateBodyColor(color); lastSkinColor = color; break;
                case CustomizerColorTypeEnum.EyeColor: modularCustomizationManager.UpdateEyeColor(color); lastEyeColor = color; break;
                case CustomizerColorTypeEnum.ScarColor: modularCustomizationManager.UpdateBodyScarColor(color); lastScarColor = color; break;
                case CustomizerColorTypeEnum.BodyArtColor: modularCustomizationManager.UpdateBodyArtColor(color); lastBodyArtColor = color; break;
                case CustomizerColorTypeEnum.BeardColor: modularCustomizationManager.UpdateBeardColor(color); lastBeardColor = color; break;
                case CustomizerColorTypeEnum.EyebrowColor: modularCustomizationManager.UpdateEyebrowColor(color); lastEyebrowColor = color; break;
                case CustomizerColorTypeEnum.MouthColor: modularCustomizationManager.UpdateMouthColor(color); lastMouthColor = color; break;
            }
        }
        #endregion

        public override void SelectAvatarSlot(int index)
        {
            base.SelectAvatarSlot(index);

            UpdateDataModularCharacterControllers();
        }

        public override void SelectAvatarSlot(UICreateCharacterSlot slot)
        {
            base.SelectAvatarSlot(slot);

            UpdateDataModularCharacterControllers();
        }
    }
}