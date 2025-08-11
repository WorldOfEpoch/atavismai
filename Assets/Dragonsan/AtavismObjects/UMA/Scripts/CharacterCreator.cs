using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Atavism;
using UMA;
using UMA.CharacterSystem;
using UnityEngine.UI;
using TMPro;

// This expansion of the "CharacterSelectionCreationManager" overrides and adds a few lines of code to the CreateCharacter()  
// and CharacterSelected() methods. Not a lot of code was needed to get UMA up and running. 
// The 2 areas of additional code have been surrounded by asterisks if you're interested.

public class CharacterCreator : CharacterSelectionCreationManager 
{
    [AtavismSeparator("Camera Settings")]

    [SerializeField] Vector3 cameraInRot = new Vector3(0f, 0f, 0f);
    [SerializeField] Vector3 cameraOutRot = new Vector3(0f, 0f, 0f);
    [SerializeField] float yawPerSec = 10f;
    [SerializeField] float pitchPerSec = 10f;
    [AtavismSeparator("Recipe Settings")]
    [SerializeField] List<RaceRecipe> noviceRecipe = new List<RaceRecipe>();
    [SerializeField] List<RaceRecipe> LeatherRecipe = new List<RaceRecipe>();
    [SerializeField] List<RaceRecipe> ClothRecipe = new List<RaceRecipe>();
    [SerializeField] List<RaceRecipe> PlateRecipe = new List<RaceRecipe>();

    [SerializeField] Button noGearButton;
    [SerializeField] Text noGearButtonText;
    [SerializeField] TextMeshProUGUI TMPNoGearButtonText;
    [SerializeField] Image NoGearButtonImage;
    [SerializeField] Button noviceGearButton;
    [SerializeField] Text noviceGearButtonText;
    [SerializeField] TextMeshProUGUI TMPNoviceGearButtonText;
    [SerializeField] Image NoviceGearButtonImage;
    [SerializeField] Button championGearButton;
    [SerializeField] Text championGearButtonText;
    [SerializeField] TextMeshProUGUI TMPChampionGearButtonText;
    [SerializeField] Image ChampionGearButtonImage;
    [SerializeField] Button leatherButton;
    [SerializeField] Text leatherButtonText;
    [SerializeField] TextMeshProUGUI TMPLeatherButtonText;
    [SerializeField] Image LeatherButtonImage;
    [SerializeField] Button clothButton;
    [SerializeField] Text clothButtonText;
    [SerializeField] TextMeshProUGUI TMPClothButtonText;
    [SerializeField] Image ClothButtonImage;
    [SerializeField] Button plateButton;
    [SerializeField] Text plateButtonText;
    [SerializeField] TextMeshProUGUI TMPPlateButtonText;
    [SerializeField] Image PlateButtonImage;
    string gearClicked = "NoviceGear";
    [AtavismSeparator("Dna Panels")]
    [SerializeField] List<DNASliderPanel> dnaPanels = new List<DNASliderPanel>();

    void Update()
    {
        if (character != null)
            character.transform.position = spawnPosition.transform.position;
        float moveRate = 2.0f;
        if (character != null || characterDCS != null)
            if (zoomingIn)
            {
                if (character != null)
                    characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, character.GetComponent<AtavismMobAppearance>().GetSocketTransform("Head").position + cameraInLoc, Time.deltaTime * moveRate);
                if (characterDCS != null)
                    characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, characterDCS.GetComponent<AtavismMobAppearance>().GetSocketTransform("Head").position + cameraInLoc, Time.deltaTime * moveRate);

                if (Mathf.Abs(this.CameraYaw - cameraInRot.y) > 1)
                {
                    float difference = this.CameraYaw - cameraInRot.y;
                    if (difference > 360)
                        difference -= 360;
                    float alteration = Time.deltaTime * yawPerSec; // 120 degrees per second
                    if (Mathf.Abs(difference) > alteration)
                    {
                        if ((difference > 0 && difference < 180) || difference < -180)
                            this.CameraYaw = this.CameraYaw - alteration;
                        else
                            this.CameraYaw = this.CameraYaw + alteration;
                    }
                    else
                    {
                        this.CameraYaw = cameraInRot.y;
                    }
                }
                if (Mathf.Abs(this.CameraPitch - cameraInRot.x) > 1)
                {
                    float difference = this.CameraPitch - cameraInRot.x;
                    if (difference > 360)
                        difference -= 360;
                    float alteration = Time.deltaTime * pitchPerSec; // 120 degrees per second
                    if (Mathf.Abs(difference) > alteration)
                    {
                        if ((difference > 0 && difference < 180) || difference < -180)
                            this.CameraPitch = this.CameraPitch - alteration;
                        else
                            this.CameraPitch = this.CameraPitch + alteration;
                    }
                    else
                    {
                        this.CameraPitch = cameraInRot.x;
                    }
                }
            }
            else if (zoomingOut)
            {
                if (characterDCS != null)
                    characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, characterDCS.transform.position + cameraOutLoc, Time.deltaTime * moveRate);
                if (character != null)
                    characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, character.transform.position + cameraOutLoc, Time.deltaTime * moveRate);
                if (Mathf.Abs(this.CameraYaw - cameraOutRot.y) > 1)
                {
                    float difference = this.CameraYaw - cameraOutRot.y;
                    if (difference > 360)
                        difference -= 360;
                    float alteration = Time.deltaTime * yawPerSec; // 120 degrees per second
                    if (Mathf.Abs(difference) > alteration)
                    {
                        if ((difference > 0 && difference < 180) || difference < -180)
                            this.CameraYaw = this.CameraYaw - alteration;
                        else
                            this.CameraYaw = this.CameraYaw + alteration;
                    }
                    else
                    {
                        this.CameraYaw = cameraOutRot.y;
                    }
                }
                if (Mathf.Abs(this.CameraPitch - cameraOutRot.x) > 1)
                {
                    float difference = this.CameraPitch - cameraOutRot.x;
                    if (difference > 360)
                        difference -= 360;
                    float alteration = Time.deltaTime * pitchPerSec; // 120 degrees per second
                    if (Mathf.Abs(difference) > alteration)
                    {
                        if ((difference > 0 && difference < 180) || difference < -180)
                            this.CameraPitch = this.CameraPitch - alteration;
                        else
                            this.CameraPitch = this.CameraPitch + alteration;
                    }
                    else
                    {
                        this.CameraPitch = cameraOutRot.x;
                    }
                }
            }
    }

    #region Variable Settings
    public void ZoomCameraIn()
    {
        zoomingIn = true;
        zoomingOut = false;
    }

    public void ZoomCameraOut()
    {
        zoomingOut = true;
        zoomingIn = false;
    }

    public float CameraPitch
    {
        get
        {
            float pitch;
            Camera camera = Camera.main;
            pitch = characterCamera.transform.rotation.eulerAngles.x;
            return pitch;
        }
        set
        {
            Camera camera = Camera.main;
            Vector3 pitchYawRoll = characterCamera.transform.eulerAngles;
            pitchYawRoll.x = value;
            characterCamera.transform.eulerAngles = pitchYawRoll;
        }
    }

    public float CameraYaw
    {
        get
        {
            float yaw;
            Camera camera = Camera.main;
            yaw = characterCamera.transform.rotation.eulerAngles.y;
            return yaw;
        }
        set
        {
            Camera camera = Camera.main;
            Vector3 pitchYawRoll = characterCamera.transform.eulerAngles;
            pitchYawRoll.y = value;
            characterCamera.transform.eulerAngles = pitchYawRoll;
        }
    }
    #endregion Variable Settings

    public override void CreateCharacter()
	{
		if (characterName == "")
			return;
        ZoomCameraOut();
      /*  CharacterManager cm = character.GetComponent<CharacterManager>();
        if (cm != null)
        {
            cm.RequestSlot("FullOutfit", "");
        }*/
        Dictionary<string, object> properties = new Dictionary<string, object>();
		properties.Add("characterName", characterName);
        string raceName = AtavismPrefabManager.Instance.GetRaceData()[raceId].name;
        string className = AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].name;
        string ganderName = AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].genderList[genderId].name;
           
        properties.Add("prefab", AtavismPrefabManager.Instance.GetRaceData()[raceId].classList[aspectId].genderList[genderId].prefab);
           
        properties.Add("race", raceName);
        properties.Add("aspect", className);
        properties.Add("gender", ganderName);
        properties.Add("genderId", genderId);
		
		UMA.CharacterSystem.DynamicCharacterAvatar avatar = character.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
		if(avatar != null)
		{
            avatar.ClearSlot("FullOutfit");
            avatar.BuildCharacter();

            string recipe = avatar.GetCurrentRecipe();
			properties.Add("custom:umaData:CurrentRecipe", recipe);
		}
		
		
		if (PortraitManager.Instance.portraitType == PortraitType.Custom)
        {
            Sprite[] icons =  AtavismSettings.Instance.Avatars(raceName, ganderName, className);
            if (icons != null && icons.Length > 0)
			{
				if (avatarList == null)
				{
					Debug.LogError("CharacterSelectionCreationManager avatarList is null", gameObject);
				}
				else
				{
					if (icons.Length > avatarList.Selected())
						if (icons[avatarList.Selected()] == null)
						{
							Debug.LogError("CharacterSelectionCreationManager icons for " + ganderName + " is null ; avatarList selected " + avatarList.Selected(), gameObject);
						}
						else
						{
							properties.Add("custom:portrait", icons[avatarList.Selected()].name);
						
						}
				}
			}
		}

		if (PortraitManager.Instance.portraitType == PortraitType.Class)
		{
            Sprite portraitSprite = PortraitManager.Instance.GetCharacterSelectionPortrait(genderId, raceId, aspectId, PortraitType.Class);
        	properties.Add("custom:portrait", portraitSprite.name);
		}

#if AT_I2LOC_PRESET
		dialogMessage = I2.Loc.LocalizationManager.GetTranslation("Please wait...");
#else
		dialogMessage = "Please wait...";
#endif
		errorMessage = "";
		characterSelected = AtavismClient.Instance.NetworkHelper.CreateCharacter(properties);
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
			StartCharacterSelection();
			//nameUI.text = characterName;
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
		}
		else
		{
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation(errorMessage), true);
#else
			ShowDialog(errorMessage, true);
#endif
		}
	}
	
	public override void CharacterSelected(CharacterEntry entry)
	{
		characterSelected = entry;
		foreach (UGUICharacterSelectSlot charSlot in characterSlots)
		{
			charSlot.CharacterSelected(characterSelected);
		}
		if (character != null)
			Destroy(character);
  	//	race = GetRaceDataByName((string)characterSelected["race"]);
	//	gender = (string)characterSelected["gender"];
		Dictionary<string, object> appearanceProps = new Dictionary<string, object>();
		foreach (string key in entry.Keys)
		{
			
			if (key.StartsWith("custom:appearanceData:"))
			{
				appearanceProps.Add(key.Substring(23), entry[key]);
			}
		}
		// Dna settings
		string prefabName = (string)characterSelected["model"];
		if (prefabName.Contains(".prefab"))
		{
			int resourcePathPos = prefabName.IndexOf("Resources/");
			prefabName = prefabName.Substring(resourcePathPos + 10);
			prefabName = prefabName.Remove(prefabName.Length - 7);
		}
		GameObject prefab = (GameObject)Resources.Load(prefabName);
		if (prefab != null)
		{
			character = (GameObject)Instantiate(prefab, spawnPosition.position, spawnPosition.rotation);
			
			//****************************************************************************************************************************************
		        
			UMA.CharacterSystem.DynamicCharacterAvatar avatar = character.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
			if(avatar != null)
			{
				if (characterSelected.ContainsKey("custom:umaData:CurrentRecipe"))
				{
					string recipe = (string)characterSelected["custom:umaData:CurrentRecipe"];
					avatar.LoadFromRecipeString(recipe);
				}
			}
		        
			//****************************************************************************************************************************************
		}	
		else
			Debug.LogError("prefab = null model: " + prefabName);
        character.GetComponent<AtavismMobAppearance>().aspect = (int)characterSelected["aspectId"];
        character.GetComponent<AtavismMobAppearance>().gender = (int)characterSelected["genderId"];
        character.GetComponent<AtavismMobAppearance>().race = (int)characterSelected["raceId"];
        ZoomCameraOut();
		//SetCharacter (prefab);
        
        
        foreach (var key in characterSelected.Keys)
        {
            if (key.EndsWith("DisplayID"))
            {
                // Debug.LogError("Key="+key);
                string keyv = key.Substring(0,key.IndexOf("DisplayID"));
                       
                //    Debug.LogError("Keyv="+keyv+" "+characterSelected[keyv+"DisplayVAL"]);
                character.GetComponent<AtavismMobAppearance>().displayVal = (string) characterSelected[keyv+"DisplayVAL"];
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay(key, (string)characterSelected[key]);
            }
        }

        // Name
        if (nameUI != null)
			nameUI.text = (string)entry["characterName"];
		if (TMPNameUI != null)
			TMPNameUI.text = (string)entry["characterName"];

		// Temp
		if (character.GetComponent<CustomisedHair>() != null)
		{
			CustomisedHair customHair = character.GetComponent<CustomisedHair>();
			if (characterSelected.ContainsKey(customHair.hairPropertyName))
			{
				customHair.UpdateHairModel((string)characterSelected[customHair.hairPropertyName]);
			}
		}
	}

    public override void SetCharacterGender(int genderId)
    {
        base.SetCharacterGender(genderId);
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
        UMA.CharacterSystem.DynamicCharacterAvatar avatar = character.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
        foreach (DNASliderPanel panel in dnaPanels)
        {
            if (panel != null)
            {
                panel.Avatar = avatar;
                panel.UpdateAvatar();
            }
        }
    }

    public override void StartCharacterCreation()
    {
        //gearClicked = "NoviceGear";
        base.StartCharacterCreation();
        UMA.CharacterSystem.DynamicCharacterAvatar avatar = character.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
        foreach (DNASliderPanel panel in dnaPanels)
        {
            if (panel != null)
            {
                panel.Avatar = avatar;
                panel.UpdateAvatar();
            }
        }
        ClickNoviceGear();

    }
    void UpdateEquipDisplay(List<RaceRecipe> recipeList)
    {
        CharacterManager cm = character.GetComponent<CharacterManager>();
        if (cm != null)
        {
            if (recipeList == null)
            {
                cm.RequestSlot("FullOutfit", "");
                return;
            }
            UMA.CharacterSystem.DynamicCharacterAvatar avatar = character.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
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

    public void ClickNoGear()
    {
        gearClicked = "NoGear";
        UpdateEquipDisplay(null);
        if (noGearButtonText != null)
            noGearButtonText.color = selectedButtonTextColor;
        else if (TMPNoGearButtonText != null)
            TMPNoGearButtonText.color = selectedButtonTextColor;
        else if (NoGearButtonImage != null)
            NoGearButtonImage.color = selectedButtonTextColor;
        else if (noGearButton != null)
        {
            ColorBlock colF = noGearButton.colors;
            colF.normalColor = selectedButtonTextColor;
            noGearButton.colors = colF;
        }
        if (noviceGearButtonText != null)
            noviceGearButtonText.color = defaultButtonTextColor;
        else if (TMPNoviceGearButtonText != null)
            TMPNoviceGearButtonText.color = defaultButtonTextColor;
        else if (NoviceGearButtonImage != null)
            NoviceGearButtonImage.color = defaultButtonTextColor;
        else if (noviceGearButton != null)
        {
            ColorBlock colM = noviceGearButton.colors;
            colM.normalColor = defaultButtonTextColor;
            noviceGearButton.colors = colM;
        }
        if (championGearButtonText != null)
            championGearButtonText.color = defaultButtonTextColor;
        else if (TMPChampionGearButtonText != null)
            TMPChampionGearButtonText.color = defaultButtonTextColor;
        else if (ChampionGearButtonImage != null)
            ChampionGearButtonImage.color = defaultButtonTextColor;
        else if (championGearButton != null)
        {
            ColorBlock colM = championGearButton.colors;
            colM.normalColor = defaultButtonTextColor;
            championGearButton.colors = colM;
        }
        hideChampion();
    }
    public void ClickNoviceGear()
    {
        gearClicked = "NoviceGear";
        UpdateEquipDisplay(noviceRecipe);
        hideChampion();
        if (noGearButtonText != null)
            noGearButtonText.color = defaultButtonTextColor;
        else if (TMPNoGearButtonText != null)
            TMPNoGearButtonText.color = defaultButtonTextColor;
        else if (NoGearButtonImage != null)
            NoGearButtonImage.color = defaultButtonTextColor;
        else if (noGearButton != null)
        {
            ColorBlock colF = noGearButton.colors;
            colF.normalColor = defaultButtonTextColor;
            noGearButton.colors = colF;
        }
        if (noviceGearButtonText != null)
            noviceGearButtonText.color = selectedButtonTextColor;
        else if (TMPNoviceGearButtonText != null)
            TMPNoviceGearButtonText.color = selectedButtonTextColor;
        else if (NoviceGearButtonImage != null)
            NoviceGearButtonImage.color = selectedButtonTextColor;
        else if (noviceGearButton != null)
        {
            ColorBlock colM = noviceGearButton.colors;
            colM.normalColor = selectedButtonTextColor;
            noviceGearButton.colors = colM;
        }
        if (championGearButtonText != null)
            championGearButtonText.color = defaultButtonTextColor;
        else if (TMPChampionGearButtonText != null)
            TMPChampionGearButtonText.color = defaultButtonTextColor;
        else if (ChampionGearButtonImage != null)
            ChampionGearButtonImage.color = defaultButtonTextColor;
        else if (championGearButton != null)
        {
            ColorBlock colM = championGearButton.colors;
            colM.normalColor = defaultButtonTextColor;
            championGearButton.colors = colM;
        }
    }
    public void ClickChampionGear()
    {
        showChampion();
        if (noGearButtonText != null)
            noGearButtonText.color = defaultButtonTextColor;
        else if (TMPNoGearButtonText != null)
            TMPNoGearButtonText.color = defaultButtonTextColor;
        else if (NoGearButtonImage != null)
            NoGearButtonImage.color = defaultButtonTextColor;
        else if (noGearButton != null)
        {
            ColorBlock colF = noGearButton.colors;
            colF.normalColor = defaultButtonTextColor;
            noGearButton.colors = colF;
        }
        if (noviceGearButtonText != null)
            noviceGearButtonText.color = defaultButtonTextColor;
        else if (TMPNoviceGearButtonText != null)
            TMPNoviceGearButtonText.color = defaultButtonTextColor;
        else if (NoviceGearButtonImage != null)
            NoviceGearButtonImage.color = defaultButtonTextColor;
        else if (noviceGearButton != null)
        {
            ColorBlock colM = noviceGearButton.colors;
            colM.normalColor = defaultButtonTextColor;
            noviceGearButton.colors = colM;
        }
        if (championGearButtonText != null)
            championGearButtonText.color = selectedButtonTextColor;
        else if (TMPChampionGearButtonText != null)
            TMPChampionGearButtonText.color = selectedButtonTextColor;
        else if (ChampionGearButtonImage != null)
            ChampionGearButtonImage.color = selectedButtonTextColor;
        else if (championGearButton != null)
        {
            ColorBlock colM = championGearButton.colors;
            colM.normalColor = selectedButtonTextColor;
            championGearButton.colors = colM;
        }
    }
    public void ClickLeatherGear()
    {
        gearClicked = "LeatherGear";
        UpdateEquipDisplay(LeatherRecipe);
        if (leatherButtonText != null)
            leatherButtonText.color = selectedButtonTextColor;
        else if (TMPLeatherButtonText != null)
            TMPLeatherButtonText.color = selectedButtonTextColor;
        else if (LeatherButtonImage != null)
            LeatherButtonImage.color = selectedButtonTextColor;
        else if (leatherButton != null)
        {
            ColorBlock colF = leatherButton.colors;
            colF.normalColor = selectedButtonTextColor;
            leatherButton.colors = colF;
        }
        if (clothButtonText != null)
            clothButtonText.color = defaultButtonTextColor;
        else if (TMPClothButtonText != null)
            TMPClothButtonText.color = defaultButtonTextColor;
        else if (ClothButtonImage != null)
            ClothButtonImage.color = defaultButtonTextColor;
        else if (clothButton != null)
        {
            ColorBlock colM = clothButton.colors;
            colM.normalColor = defaultButtonTextColor;
            clothButton.colors = colM;
        }
        if (plateButtonText != null)
            plateButtonText.color = defaultButtonTextColor;
        else if (TMPPlateButtonText != null)
            TMPPlateButtonText.color = defaultButtonTextColor;
        else if (PlateButtonImage != null)
            PlateButtonImage.color = defaultButtonTextColor;
        else if (plateButton != null)
        {
            ColorBlock colM = plateButton.colors;
            colM.normalColor = defaultButtonTextColor;
            plateButton.colors = colM;
        }
    }
    public void ClickClothGear()
    {
        gearClicked = "ClothGear";
        UpdateEquipDisplay(ClothRecipe);
        if (leatherButtonText != null)
            leatherButtonText.color = defaultButtonTextColor;
        else if (TMPLeatherButtonText != null)
            TMPLeatherButtonText.color = defaultButtonTextColor;
        else if (LeatherButtonImage != null)
            LeatherButtonImage.color = defaultButtonTextColor;
        else if (leatherButton != null)
        {
            ColorBlock colF = leatherButton.colors;
            colF.normalColor = defaultButtonTextColor;
            leatherButton.colors = colF;
        }
        if (clothButtonText != null)
            clothButtonText.color = selectedButtonTextColor;
        else if (TMPClothButtonText != null)
            TMPClothButtonText.color = selectedButtonTextColor;
        else if (ClothButtonImage != null)
            ClothButtonImage.color = selectedButtonTextColor;
        else if (clothButton != null)
        {
            ColorBlock colM = clothButton.colors;
            colM.normalColor = selectedButtonTextColor;
            clothButton.colors = colM;
        }
        if (plateButtonText != null)
            plateButtonText.color = defaultButtonTextColor;
        else if (TMPPlateButtonText != null)
            TMPPlateButtonText.color = defaultButtonTextColor;
        else if (PlateButtonImage != null)
            PlateButtonImage.color = defaultButtonTextColor;
        else if (plateButton != null)
        {
            ColorBlock colM = plateButton.colors;
            colM.normalColor = defaultButtonTextColor;
            plateButton.colors = colM;
        }

    }
    public void ClickPlateGear()
    {
        gearClicked = "PlateGear";
        UpdateEquipDisplay(PlateRecipe);
        if (leatherButtonText != null)
            leatherButtonText.color = defaultButtonTextColor;
        else if (TMPLeatherButtonText != null)
            TMPLeatherButtonText.color = defaultButtonTextColor;
        else if (LeatherButtonImage != null)
            LeatherButtonImage.color = defaultButtonTextColor;
        else if (leatherButton != null)
        {
            ColorBlock colF = leatherButton.colors;
            colF.normalColor = defaultButtonTextColor;
            leatherButton.colors = colF;
        }
        if (clothButtonText != null)
            clothButtonText.color = defaultButtonTextColor;
        else if (TMPClothButtonText != null)
            TMPClothButtonText.color = defaultButtonTextColor;
        else if (ClothButtonImage != null)
            ClothButtonImage.color = defaultButtonTextColor;
        else if (clothButton != null)
        {
            ColorBlock colM = clothButton.colors;
            colM.normalColor = defaultButtonTextColor;
            clothButton.colors = colM;
        }
        if (plateButtonText != null)
            plateButtonText.color = selectedButtonTextColor;
        else if (TMPPlateButtonText != null)
            TMPPlateButtonText.color = selectedButtonTextColor;
        else if (PlateButtonImage != null)
            PlateButtonImage.color = selectedButtonTextColor;
        else if (plateButton != null)
        {
            ColorBlock colM = plateButton.colors;
            colM.normalColor = selectedButtonTextColor;
            plateButton.colors = colM;
        }

    }
    void showChampion()
    {
        if (leatherButton != null)
        {
            leatherButton.gameObject.SetActive(true);
        }
        if (clothButton != null)
        {
            clothButton.gameObject.SetActive(true);
        }
        if (plateButton != null)
        {
            plateButton.gameObject.SetActive(true);
        }
        if (LeatherButtonImage != null)
        {
            LeatherButtonImage.enabled = true;
        }
        if (ClothButtonImage != null)
        {
            ClothButtonImage.enabled = true;
        }
        if (PlateButtonImage != null)
        {
            PlateButtonImage.enabled = true;
        }
        ClickClothGear();
    }
    void hideChampion()
    {
        if (leatherButton != null)
        {
            leatherButton.gameObject.SetActive(false);
        }
        if (clothButton != null)
        {
            clothButton.gameObject.SetActive(false);
        }
        if (plateButton != null)
        {
            plateButton.gameObject.SetActive(false);
        }
        if (LeatherButtonImage != null)
        {
            LeatherButtonImage.enabled = false;
        }
        if (ClothButtonImage != null)
        {
            ClothButtonImage.enabled = false;
        }
        if (PlateButtonImage != null)
        {
            PlateButtonImage.enabled = false;
        }

    }

    public void Randomize()
    {
        foreach (DNASliderPanel panel in dnaPanels)
        {
            if (panel != null)
            {
                panel.Randomize();
            }
        }
    }
}
