using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Atavism.UI.Game;
using HNGamers.Atavism;
//using UMA;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismPetInventoryPanel : UIAtavismWindowBase
    {
        #region variables
        static UIAtavismPetInventoryPanel instance;
        long _id;
       // public  Label targetName;
       [AtavismSeparator("Stats")]
       [SerializeField]  private VisualTreeAsset statsListElementTemplate;
       [SerializeField] private int numberElementsPerPage = 12;
       private VisualElement statGrid;
       public List<UIStat> StatsName = new List<UIStat>();
       List<UIStat> StatsNameOnCharacter = new List<UIStat>();
       List<UIAtavismCharacterStatEntry> _statEntries = new  List<UIAtavismCharacterStatEntry>();
        
        Button buttonPrev;
        Button buttonNext;
        
        
        // [AtavismSeparator("Slots")]
        Dictionary<string,UIAtavismCharacterEquipSlot> slots = new Dictionary<string,UIAtavismCharacterEquipSlot>();
        // public List<string> slotName = new List<string>();

        UIAtavismCharacterEquipSlot slotAmmo;
        // [AtavismSeparator("Panel")]
        // public
        // GameObject panel;
        #endregion variables;

        private int page = 0;

        int petProfile = -1;
        //KeyCode key = KeyCode.C;
        void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(instance);
              
            }
            instance = this;
        }
        
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            
            statGrid = uiDocument.rootVisualElement.Query<VisualElement>("stat-grid");
            statGrid.Clear();
            for (int i = 0; i < numberElementsPerPage; i++)
            {
                UIAtavismCharacterStatEntry row = new UIAtavismCharacterStatEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = statsListElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = row;
                // Initialize the controller script
                row.SetVisualElement(newListEntry);
                statGrid.Add(newListEntry);
                _statEntries.Add(row);
            }
            
            buttonPrev = uiDocument.rootVisualElement.Query<Button>("prev-button");  
            buttonNext = uiDocument.rootVisualElement.Query<Button>("next-button");
            
            if (buttonPrev != null)
                buttonPrev.clicked +=OnPrevClick;
            if (buttonNext != null)
                buttonNext.clicked +=OnNextClick;
            slots.Clear();
            foreach (var slot in Inventory.Instance.itemSlots.Keys)
            {
                string slot_name = slot.Replace(" ", "-");
                VisualElement val = uiDocument.rootVisualElement.Query<VisualElement>("equip-slot-" + slot_name);
                if (val != null)
                {
                    UIAtavismCharacterEquipSlot _slot = new UIAtavismCharacterEquipSlot();
                    // Assign the controller script to the visual element
                    val.userData = _slot;
                    _slot.pet = true;
                    _slot.slotName = slot;
                    // Initialize the controller script
                    _slot.SetVisualElement(val);
                    slots.Add(slot, _slot);
                }
            }
            // foreach (var slot in Inventory.Instance.itemSlots.Keys)
            // {
            //     string slot_name = slot.Replace(" ", "-");
            //     UIAtavismItemDisplay val = uiDocument.rootVisualElement.Query<UIAtavismItemDisplay>("equip-slot-"+slot_name);
            //     if(val != null)
            //         slots.Add(slot, val);
            // }
            VisualElement ammo = uiDocument.rootVisualElement.Query<VisualElement>("equip-slot-Ammo");
            if (ammo != null)
            {
                UIAtavismCharacterEquipSlot _slot = new UIAtavismCharacterEquipSlot();
                // Assign the controller script to the visual element
                ammo.userData = _slot;
                // Initialize the controller script
                _slot.SetVisualElement(ammo);
                _slot.ammo = true;
                slotAmmo = _slot;
            }
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("PET_INVENTORY_UPDATE",  OnEvent);
            AtavismEventSystem.RegisterEvent("PET_LIST_UPDATE",  OnEvent);
        }

        protected override void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "PET_INVENTORY_UPDATE")
            {
                UpdateCharacterData();
            }else if (eData.eventType == "PET_LIST_UPDATE")
            {
                //Debug.Log("PET_LIST_UPDATE "+eData.eventArgs[0]);
                string[] ids = eData.eventArgs[0].Split("|");
                if (!ids.Contains(petProfile.ToString()))
                {
                    Hide();
                }
            }
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("PET_INVENTORY_UPDATE",  OnEvent);
            AtavismEventSystem.UnregisterEvent("PET_LIST_UPDATE",  OnEvent);
        }

        private void OnPrevClick()
        {
            page--;
            if (page < 0)
                page = 0;
            AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
            UpdateStats(node);
        }

        private void OnNextClick()
        {
            page++;
           int maxPage = (int)StatsNameOnCharacter.Count / numberElementsPerPage;
           if ((StatsNameOnCharacter.Count % numberElementsPerPage) != 0)
               maxPage++;
           if (page >= maxPage)
               page = 0;
            AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
            UpdateStats(node);
        }
        
        
        void Awake()
        {
            Hide();
        }

        protected override void Update()
        {
            base.Update();
        }
        public override void Show()
        {
            base.Show();
           
        }

        public override void Hide()
        {
            StopAllCoroutines();
            base.Hide();
            if(AtavismSettings.Instance.GetPetPanelCamera()!=null)
                AtavismSettings.Instance.GetPetPanelCamera().enabled = false;
        }


        void UpdateStats(AtavismObjectNode node)
        {
            StatsNameOnCharacter.Clear();
            foreach (var stat in StatsName)
            {
                if (node.Properties.ContainsKey(stat.name))
                {
                    StatsNameOnCharacter.Add(stat);
                }
            }
            
            
            for (int i = page * numberElementsPerPage; i < (page + 1) * numberElementsPerPage; i++)
            {

                if (node != null && StatsNameOnCharacter.Count > i && StatsNameOnCharacter[i].name.Length > 0)
                {
                    if (node.Properties.ContainsKey(StatsNameOnCharacter[i].name))
                    {
                        _statEntries[i - page * numberElementsPerPage].Show();
                        _statEntries[i - page * numberElementsPerPage].UpdateStat(StatsNameOnCharacter[i].displayName,
                            (string)node.GetPropertyStatWithPrecision(StatsNameOnCharacter[i].name).ToString());
                    }
                    else
                    {
                        _statEntries[i - page * numberElementsPerPage].Hide();
                    }
                }
                else
                {
                    _statEntries[i - page * numberElementsPerPage].Hide();
                }
            }
        }
     
        public void UpdateCharacterData(long id)
        {
             //Debug.LogError("Pet UpdateCharacterData "+ id);
            _id = id;
            AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
            petProfile = -1;
            object pp = node.GetProperty("petProfile");
            if (pp != null)
                petProfile = (int)pp;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("petProfile", petProfile);
            NetworkAPI.SendExtensionMessage(0, false, "inventory.GET_PET_INVENTORY", props);
            // UpdateCharacterData();
        }
        private IEnumerator UpdateSlotsList()
        {
    
            bool done = false;
            while (!done)
            {
                int slotProfile = -1;
                AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
                if (node != null)
                {
                    object op = node.GetProperty("slotsProfile");
                    if (op != null)
                        slotProfile = (int)op;
                    if (slotProfile > 0)
                    {
                        var profile = AtavismPrefabManager.Instance.LoadSlotsProfilesData(slotProfile);
                        if (profile != null)
                        {
                            foreach (var s in slots.Keys)
                            {
                                // Debug.LogError("Pet slot " + s + " profile=" + profile + " petProfile=" + petProfile);
                                slots[s].petProfile = petProfile;
                                if (profile.slots.Contains(s))
                                {
                                    // Debug.LogError("Pet slot " + s + " Show");
                                    slots[s].Show();
                                }
                                else
                                {
                                    // Debug.LogError("Pet slot " + s + " Hide");
                                    slots[s].Hide();
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("Pet Not Found SlotsProfilesData");
                        }
                    }
                    else
                    {
                        foreach (var s in slots.Keys)
                        {
                                slots[s].Hide();
                        }
                    }
                }
                else
                {
                    done = true;
                    Hide();
                }
                yield return new WaitForSeconds (1f);

            }

        }
        
        
        public void UpdateCharacterData()
        {
            AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
            StopAllCoroutines();
            StartCoroutine(UpdateSlotsList());
           //  Debug.LogError("Pet UpdateCharacterData after update " + _id);
            if (uiWindowTitle != null)
            {
                string mName = node.Name;
                
                
                if (node.Properties.ContainsKey("DisplayName"))
                    mName = (string)node.Properties["DisplayName"];
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != "" && I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != null) mName
 = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName);
#endif
                uiWindowTitle.text = mName.ToUpper();

            }

            int slotProfile = -1;
            int petProfile = -1;
            object op = node.GetProperty("slotsProfile");
            if (op != null)
                slotProfile = (int)op;

            object pp = node.GetProperty("petProfile");
            if (pp != null)
                petProfile = (int)pp;

            // Debug.LogError("Pet UpdateCharacterData " + _id + " slotP=" + slotProfile + " petP=" + petProfile);

            UpdateStats(node);

            // foreach (var s in slots.Values)
            // {
            //     s.SetItemData(null);
            // }

            if (slotProfile > 0)
            {
                var profile = AtavismPrefabManager.Instance.LoadSlotsProfilesData(slotProfile);
                if (profile != null)
                {
                    foreach (var s in slots.Keys)
                    {
                        // Debug.LogError("Pet slot " + s + " profile=" + profile + " petProfile=" + petProfile);
                        slots[s].petProfile = petProfile;
                        if (profile.slots.Contains(s))
                        {
                            // Debug.LogError("Pet slot " + s + " Show");
                            slots[s].Show();
                        }
                        else
                        {
                            // Debug.LogError("Pet slot " + s + " Hide");
                            slots[s].Hide();
                        }
                    }
                }
                else
                {
                    Debug.LogError("Pet Not Found SlotsProfilesData");
                }
            }

            foreach (var slotName in slots.Keys)
            {

                AtavismInventoryItem item = GetItemInSlot(slotName);
                // Debug.LogError("Pet slotName=" + slotName + " item=" + item);
                slots[slotName].UpdateEquipItemData(item);
            }

            if (AtavismSettings.Instance.GetPetPanelSpawn() != null)
            {
                if (AtavismSettings.Instance.PetAvatar != null)
                    DestroyImmediate(AtavismSettings.Instance.PetAvatar);
                node.GameObject.GetComponent<AtavismMobAppearance>().ResetAttachObject();

                string prefabName = (string)node.GetProperty("model");
                if (prefabName.Contains(".prefab"))
                {
                    int resourcePathPos = prefabName.IndexOf("Resources/");
                    prefabName = prefabName.Substring(resourcePathPos + 10);
                    prefabName = prefabName.Remove(prefabName.Length - 7);
                }

                GameObject prefab = (GameObject)Resources.Load(prefabName);

                if (prefab == null)
                {
                    Debug.LogWarning("No Model " + prefabName);
                    return;
                }

                AtavismSettings.Instance.PetAvatar = (GameObject)Instantiate(prefab,
                    AtavismSettings.Instance.GetPetPanelSpawn().position,
                    AtavismSettings.Instance.GetPetPanelSpawn().rotation);
                if (node.PropertyExists("umaData"))
                {
                    Dictionary<string, object> umaDictionary = (Dictionary<string, object>)node.GetProperty("umaData");
                    //   AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismNode>().AddLocalProperty("umaData",umaDictionary);
                    var node1 = AtavismSettings.Instance.PetAvatar.GetComponent<AtavismNode>();
                    if (node1 == null)
                    {
                        node1 = AtavismSettings.Instance.PetAvatar.AddComponent<AtavismNode>();
                    }

                    node1.AddLocalProperty("umaData", umaDictionary);
                    node1.AddLocalProperty("genderId", (int)node.GetProperty("genderId"));
                    node1.AddLocalProperty("race", (int)node.GetProperty("race"));
                    node1.AddLocalProperty("aspect", (int)node.GetProperty("aspect"));
                    AtavismSettings.Instance.PetAvatar.SendMessage("GrabRecipe",
                        SendMessageOptions.DontRequireReceiver);
                }

                DestroyImmediate(AtavismSettings.Instance.PetAvatar.GetComponent<MobController3D>());

                var mcm = AtavismSettings.Instance.PetAvatar.GetComponent<ModularCustomizationManager>();
                if (mcm != null)
                {

                    if (node.PropertyExists(mcm.EyeMaterialPropertyName))
                    {
                        mcm.UpdateEyeMaterial((int)node.GetProperty(mcm.EyeMaterialPropertyName));
                    }

                    if (node.PropertyExists(mcm.HairMaterialPropertyName))
                    {
                        mcm.UpdateHairMaterial((int)node.GetProperty(mcm.HairMaterialPropertyName));
                    }

                    if (node.PropertyExists(mcm.SkinMaterialPropertyName))
                    {
                        mcm.UpdateSkinMaterial((int)node.GetProperty(mcm.SkinMaterialPropertyName));
                    }

                    if (node.PropertyExists(mcm.MouthMaterialPropertyName))
                    {
                        mcm.UpdateMouthMaterial((int)node.GetProperty(mcm.MouthMaterialPropertyName));
                    }

                    if (node.PropertyExists(mcm.bodyColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.bodyColorPropertyName).ToString().Split(',');
                        Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBodyColor(test);
                    }

                    if (node.PropertyExists(mcm.scarColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.scarColorPropertyName).ToString().Split(',');
                        Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBodyScarColor(test);
                    }

                    if (node.PropertyExists(mcm.hairColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.hairColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateHairColor(color32);
                    }

                    if (node.PropertyExists(mcm.mouthColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.mouthColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateMouthColor(color32);
                    }

                    if (node.PropertyExists(mcm.beardColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.beardColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBeardColor(color32);
                    }

                    if (node.PropertyExists(mcm.eyeBrowColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.eyeBrowColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateEyebrowColor(color32);
                    }

                    if (node.PropertyExists(mcm.stubbleColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.stubbleColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateStubbleColor(color32);
                    }

                    if (node.PropertyExists(mcm.bodyArtColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.bodyArtColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBodyArtColor(color32);
                    }

                    if (node.PropertyExists(mcm.eyeColorPropertyName))
                    {
                        var item = node.GetProperty(mcm.eyeColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateEyeColor(color32);
                    }

                    if (node.PropertyExists(mcm.helmetColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.helmetColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.Head);
                        }
                    }

                    if (node.PropertyExists(mcm.torsoColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.torsoColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.Torso);
                        }
                    }

                    if (node.PropertyExists(mcm.upperArmsColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.upperArmsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.Upperarms);
                        }
                    }

                    if (node.PropertyExists(mcm.lowerArmsColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.lowerArmsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.LowerArms);
                        }
                    }

                    if (node.PropertyExists(mcm.hipsColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.hipsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.Hips);
                        }
                    }

                    if (node.PropertyExists(mcm.lowerLegsColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.lowerLegsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.LowerLegs);
                        }
                    }

                    if (node.PropertyExists(mcm.feetColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.feetColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.Feet);
                        }
                    }

                    if (node.PropertyExists(mcm.handsColorPropertyName))
                    {
                        var colorProperties = node.GetProperty(mcm.handsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, ModularCustomizationManager.BodyType.Hands);
                        }
                    }

                    if (node.PropertyExists(mcm.hairPropertyName))
                    {
                        mcm.UpdateHairModel((int)node.GetProperty(mcm.hairPropertyName));
                    }



                    if (node.PropertyExists(mcm.beardPropertyName))
                    {
                        mcm.UpdateBeardModel((int)node.GetProperty(mcm.beardPropertyName));
                    }

                    if (node.PropertyExists(mcm.eyebrowPropertyName))
                    {

                        mcm.UpdateEyebrowModel((int)node.GetProperty(mcm.eyebrowPropertyName));
                    }

                    if (node.PropertyExists(mcm.headPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.headPropertyName),
                            ModularCustomizationManager.BodyType.Head);
                    }

                    if (node.PropertyExists(mcm.faceTexPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.faceTexPropertyName),
                            ModularCustomizationManager.BodyType.Face);
                    }

                    if (node.PropertyExists(mcm.handsPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.handsPropertyName),
                            ModularCustomizationManager.BodyType.Hands);
                    }

                    if (node.PropertyExists(mcm.lowerArmsPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.lowerArmsPropertyName),
                            ModularCustomizationManager.BodyType.LowerArms);
                    }

                    if (node.PropertyExists(mcm.upperArmsPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.upperArmsPropertyName),
                            ModularCustomizationManager.BodyType.Upperarms);
                    }

                    if (node.PropertyExists(mcm.torsoPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.torsoPropertyName),
                            ModularCustomizationManager.BodyType.Torso);
                    }

                    if (node.PropertyExists(mcm.hipsPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.hipsPropertyName),
                            ModularCustomizationManager.BodyType.Hips);
                    }

                    if (node.PropertyExists(mcm.lowerLegsPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.lowerLegsPropertyName),
                            ModularCustomizationManager.BodyType.LowerLegs);
                    }

                    if (node.PropertyExists(mcm.feetPropertyName))
                    {
                        mcm.UpdateBodyModel((string)node.GetProperty(mcm.feetPropertyName),
                            ModularCustomizationManager.BodyType.Feet);
                    }

                    if (node.PropertyExists(mcm.earsPropertyName))
                    {
                        mcm.UpdateEarModel((int)node.GetProperty(mcm.earsPropertyName));
                    }

                    if (node.PropertyExists(mcm.eyesPropertyName))
                    {
                        mcm.UpdateEyeModel((int)node.GetProperty(mcm.eyesPropertyName));
                    }

                    if (node.PropertyExists(mcm.tuskPropertyName))
                    {
                        mcm.UpdateTuskModel((int)node.GetProperty(mcm.tuskPropertyName));
                    }

                    if (node.PropertyExists(mcm.mouthPropertyName))
                    {
                        mcm.UpdateMouthModel((int)node.GetProperty(mcm.mouthPropertyName));
                    }

                    if (node.PropertyExists(mcm.faithPropertyName))
                    {
                        mcm.SetFaith((string)node.GetProperty(mcm.faithPropertyName));
                    }
#if IPBRInt
                if (node.PropertyExists(mcm.blendshapePresetValue) && (!mcm.enableSavingInfinityPBRBlendshapes))
                {
                    mcm.UpdateBlendShapePresets((int)node.GetProperty(mcm.blendshapePresetValue));
                }

                if (node.PropertyExists(mcm.infinityBlendShapes))
                {
                    mcm.UpdateBlendShapes((string)node.GetProperty(mcm.infinityBlendShapes));
                }
#endif

                }

                // AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<MobController3D>().enabled = false;

                node.GameObject.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();

                // AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
                //    Debug.LogError("Other "+node.Oid+" "+AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<AtavismNode>().Oid);
                //AtavismSettings.Instance.OtherCharacterAvatar.layer = 24;
                AtavismSettings.Instance.PetAvatar.transform.position =
                    AtavismSettings.Instance.GetPetPanelSpawn().position;
                AtavismSettings.Instance.PetAvatar.transform.rotation =
                    AtavismSettings.Instance.GetPetPanelSpawn().rotation;
                AtavismSettings.Instance.GetPetPanelCamera().enabled = true;
            }


            if (!showing)
                Show();
        }

        AtavismInventoryItem GetItemInSlot(string slotName)
        {
            foreach (AtavismInventoryItem item in Inventory.Instance.EquippedPetItems.Values)
            {
                string orgSlotName = Inventory.Instance.GetItemByTemplateID(item.TemplateId).slot;

                if (Inventory.Instance.itemGroupSlots.ContainsKey(slotName))
                {
                    // Debug.LogError(item.ItemId+" found Group "+slotName+" GS:"+Inventory.Instance.itemGroupSlots[slotName]);
                    try
                    {


                        if (Inventory.Instance.itemGroupSlots[orgSlotName].all)
                        {
                            foreach (var s in Inventory.Instance.itemGroupSlots[orgSlotName].slots)
                            {
                                // Debug.LogError(slotName+" item "+item.ItemId+" "+item.slot+" orgSlotName="+orgSlotName+" set? "+(!(item.slot.StartsWith("Set_") && item.slot.ToLower().Contains(orgSlotName.ToLower())))+" Set_? "+item.slot.StartsWith("Set_")+" orgslot in slot? "+item.slot.ToLower().Contains(orgSlotName.ToLower())+" "+s.name.ToLower().Contains(orgSlotName.ToLower()));
                                if (s.name.ToLower() == slotName.ToLower() && !item.slot.StartsWith("Set_"))
                                {
                                    // Debug.LogError(slotName+" return items "+item.ItemId);
                                    return item;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(item.ItemId + "Exception slotName: " + slotName + " orgSlotName: " + orgSlotName+" "+e);
                    }

                    foreach (var s in Inventory.Instance.itemGroupSlots[slotName].slots)
                    {
                        // Debug.LogError(item.ItemId+" "+slotName+"Group Slot "+s.name+" | "+slotName);

                        if (s.name.ToLower() == item.slot.ToLower())
                        {
                            // Debug.LogError(slotName+" return items "+item.ItemId);
                            return item;
                        }
                    }

                }

            }

            return null;
        }
        public static UIAtavismPetInventoryPanel Instance
        {
            get
            {
                return instance;
            }
        }
        public long Id
        {
            get
            {
                return _id;
            }
        }

    }
}