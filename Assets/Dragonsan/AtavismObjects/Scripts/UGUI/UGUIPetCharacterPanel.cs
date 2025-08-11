using System;
using System.Collections;
using System.Linq;

using System.Collections.Generic;
using HNGamers.Atavism;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HNGamers.Atavism.ModularCustomizationManager;

namespace Atavism
{

    public class UGUIPetCharacterPanel : MonoBehaviour
    {
        bool showing = false;
        static UGUIPetCharacterPanel instance;
        public  TextMeshProUGUI targetName;
        long _id =-1;
        int petProfile = -1;
        [AtavismSeparator("Stats")]
        public List<TextMeshProUGUI> StatsText = new List<TextMeshProUGUI>();
        public List<string> StatsName = new List<string>();
        List<string> StatsNameOnCharacter = new List<string>();
        [AtavismSeparator("Slots")]
        public List<UGUICharacterEquipSlot> slots = new List<UGUICharacterEquipSlot>();
        // public List<string> slotName = new List<string>();

        private void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(instance);
              
            }
            instance = this;
        }

        // public List<SetsSlots> setsSlots;
        //KeyCode key = KeyCode.C;
        void Awake()
        {
            
            AtavismEventSystem.RegisterEvent("PET_INVENTORY_UPDATE",  this);
            AtavismEventSystem.RegisterEvent("PET_LIST_UPDATE",  OnEvent);
            Hide();
        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("PET_INVENTORY_UPDATE",  this);
            AtavismEventSystem.UnregisterEvent("PET_LIST_UPDATE",  OnEvent);
        }

        void Update()
        {
            // if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().character.key) || Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().character.altKey))&& !ClientAPI.UIHasFocus())
                // Toggle();
        }
        // public void Toggle()
        // {
        //     if (showing)
        //         Hide();
        //     else
        //         Show();
        // }

        
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "PET_INVENTORY_UPDATE" || eData.eventType == "ITEM_ICON_UPDATE" || eData.eventType == "SLOTS_UPDATE")
            {
              //  if(showing)
                    UpdateCharacterData();
          
            }else if (eData.eventType == "PET_LIST_UPDATE")
            {
                // Debug.Log("PET_LIST_UPDATE "+eData.eventArgs[0]);
                string[] ids = eData.eventArgs[0].Split("|");
                if (!ids.Contains(petProfile.ToString()))
                {
                    Hide();
                }
            }

        }
        
        
        public void Show()
        {

            AtavismSettings.Instance.OpenWindow(this);
            showing = true;
            GetComponent<CanvasGroup>().alpha = 1f;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<CanvasGroup>().interactable = true;
            AtavismUIUtility.BringToFront(gameObject);
        }


        public void UpdateCharacterData(long id)
        {
            // Debug.LogError("Pet UpdateCharacterData "+ id);
            _id = id;
            AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
            petProfile = -1;
            object pp = node.GetProperty("petProfile");
            if (pp != null)
                petProfile = (int)pp;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("petProfile", petProfile);
            NetworkAPI.SendExtensionMessage(0, false, "inventory.GET_PET_INVENTORY", props);

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
                            foreach (var slot in slots)
                            {
                                if(slot ==null)
                                    continue;
                                // Debug.LogError("Pet slot " + slot.slotName + " profile=" + profile + " petProfile=" + petProfile);
                                slot.petProfile = petProfile;
                                slot.pet = true;
                                if (profile.slots.Contains(slot.slotName))
                                {
                                    // Debug.LogError("Pet slot " + slot + " Show");
                                    if(!slot.gameObject.activeSelf)
                                        slot.gameObject.SetActive(true);
                                }
                                else
                                {
                                    // Debug.LogError("Pet slot " + slot + " Hide");
                                    if(slot.gameObject.activeSelf)
                                        slot.gameObject.SetActive(false);
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
                        foreach (var slot in slots)
                        {
                            if(slot ==null)
                                continue;

                            if (slot.gameObject.activeSelf)
                                slot.gameObject.SetActive(false);
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


        public void UpdateCharacterData(){      
            
            if(_id == -1)
                return;
            
            AtavismObjectNode node = ClientAPI.GetObjectNode(_id);
            StopAllCoroutines();
            StartCoroutine(UpdateSlotsList());
            // Debug.LogError("Pet UpdateCharacterData " + _id);
            if (targetName != null)
            {
                string mName = node.Name;
                if (node.Properties.ContainsKey("DisplayName"))
                    mName = (string)node.Properties["DisplayName"];

#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != "" && I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != null) mName
 = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName);
#endif
                targetName.text = mName.ToUpper();

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

            if (node != null)
            {
                StatsNameOnCharacter.Clear();
                foreach (var stat in StatsName)
                {
                    if (node.Properties.ContainsKey(stat))
                    {
                        StatsNameOnCharacter.Add(stat);
                    }
                }
                for (int i = 0; i < StatsText.Count; i++)
                {
                    if (StatsText[i] != null && StatsNameOnCharacter.Count > i && StatsNameOnCharacter[i].Length > 0)
                    {
                        if (node.Properties.ContainsKey(StatsNameOnCharacter[i]))
                        {
                            if(!StatsText[i].gameObject.transform.parent.gameObject.activeSelf)
                                StatsText[i].gameObject.transform.parent.gameObject.SetActive(true);
                            StatsText[i].text = (string)node.GetProperty(StatsNameOnCharacter[i]).ToString();
                        } else
                        {
                            if(StatsText[i].gameObject.transform.parent.gameObject.activeSelf)
                            StatsText[i].gameObject.transform.parent.gameObject.SetActive(false);
                        }
                    } else
                    {
                        if(StatsText[i] != null && StatsText[i].gameObject.transform.parent.gameObject.activeSelf)
                            StatsText[i].gameObject.transform.parent.gameObject.SetActive(false);
                    }
                }
                
            }
            
            // UpdateStats(node);
            
            if (slotProfile > 0)
            {
                var profile = AtavismPrefabManager.Instance.LoadSlotsProfilesData(slotProfile);
                if (profile != null)
                {
                    foreach (var slot in slots)
                    {
                        if(slot ==null)
                            continue;
                        // Debug.LogError("Pet slot " + slot.slotName + " profile=" + profile + " petProfile=" + petProfile);
                        slot.petProfile = petProfile;
                        slot.pet = true;
                        if (profile.slots.Contains(slot.slotName))
                        {
                            // Debug.LogError("Pet slot " + slot + " Show");
                            if(!slot.gameObject.activeSelf)
                                slot.gameObject.SetActive(true);
                        }
                        else
                        {
                            // Debug.LogError("Pet slot " + slot + " Hide");
                            if(slot.gameObject.activeSelf)
                                slot.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Pet Not Found SlotsProfilesData");
                }
            }

            foreach (var slot in slots)
            {

                AtavismInventoryItem item = GetItemInSlot(slot.slotName);
                // Debug.LogError("Pet slotName=" + slot.slotName + " item=" + item);
                slot.UpdateEquipItemData(item);
            }
            
            
            
        if (AtavismSettings.Instance.GetPetPanelSpawn() != null)
            {
                if(AtavismSettings.Instance.PetAvatar!=null)
                    DestroyImmediate(AtavismSettings.Instance.PetAvatar);
                
                node.GameObject.GetComponent<AtavismMobAppearance>().ResetAttachObject();
           
            
            string prefabName = (string) node.GetProperty("model");
            if (prefabName.Contains(".prefab"))
            {
                int resourcePathPos = prefabName.IndexOf("Resources/");
                prefabName = prefabName.Substring(resourcePathPos + 10);
                prefabName = prefabName.Remove(prefabName.Length - 7);
            }
            GameObject prefab = (GameObject)Resources.Load(prefabName);
            AtavismSettings.Instance.PetAvatar = (GameObject) Instantiate(prefab, AtavismSettings.Instance.GetPetPanelSpawn().position, AtavismSettings.Instance.GetPetPanelSpawn().rotation);
            if(node.PropertyExists("umaData"))
            {	
                Dictionary<string, object> umaDictionary = (Dictionary<string, object>)node.GetProperty("umaData");
                //   AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismNode>().AddLocalProperty("umaData",umaDictionary);
                var node1 = AtavismSettings.Instance.PetAvatar.GetComponent<AtavismNode>();
                if (node1 == null)
                {
                    node1 = AtavismSettings.Instance.PetAvatar.AddComponent<AtavismNode>();
                }
                node1.AddLocalProperty("umaData",umaDictionary);
                node1.AddLocalProperty("genderId",(int)node.GetProperty("genderId"));
                node1.AddLocalProperty("race",(int)node.GetProperty("race"));
                node1.AddLocalProperty("aspect",(int)node.GetProperty("aspect"));
                AtavismSettings.Instance.PetAvatar.SendMessage("GrabRecipe", SendMessageOptions.DontRequireReceiver);
            }
                 var mcm = AtavismSettings.Instance.PetAvatar.GetComponent<ModularCustomizationManager>();
            if ( mcm != null)
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
                    Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateBodyColor(test);
                }

                if (node.PropertyExists(mcm.scarColorPropertyName))
                {
                    var item = node.GetProperty(mcm.scarColorPropertyName).ToString().Split(',');
                    Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateBodyScarColor(test);
                }

                if (node.PropertyExists(mcm.hairColorPropertyName))
                {
                    var item = node.GetProperty(mcm.hairColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateHairColor(color32);
                }

                if (node.PropertyExists(mcm.mouthColorPropertyName))
                {
                    var item = node.GetProperty(mcm.mouthColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateMouthColor(color32);
                }

                if (node.PropertyExists(mcm.beardColorPropertyName))
                {
                    var item = node.GetProperty(mcm.beardColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateBeardColor(color32);
                }

                if (node.PropertyExists(mcm.eyeBrowColorPropertyName))
                {
                    var item = node.GetProperty(mcm.eyeBrowColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateEyebrowColor(color32);
                }

                if (node.PropertyExists(mcm.stubbleColorPropertyName))
                {
                    var item = node.GetProperty(mcm.stubbleColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateStubbleColor(color32);
                }

                if (node.PropertyExists(mcm.bodyArtColorPropertyName))
                {
                    var item = node.GetProperty(mcm.bodyArtColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                    mcm.UpdateBodyArtColor(color32);
                }

                if (node.PropertyExists(mcm.eyeColorPropertyName))
                {
                    var item = node.GetProperty(mcm.eyeColorPropertyName).ToString().Split(',');
                    Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]), Convert.ToByte(item[2]), Convert.ToByte(item[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                        Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]), Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
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
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.headPropertyName), ModularCustomizationManager.BodyType.Head);
                }

                if (node.PropertyExists(mcm.faceTexPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.faceTexPropertyName), ModularCustomizationManager.BodyType.Face);
                }

                if (node.PropertyExists(mcm.handsPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.handsPropertyName), ModularCustomizationManager.BodyType.Hands);
                }

                if (node.PropertyExists(mcm.lowerArmsPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.lowerArmsPropertyName), ModularCustomizationManager.BodyType.LowerArms);
                }

                if (node.PropertyExists(mcm.upperArmsPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.upperArmsPropertyName), ModularCustomizationManager.BodyType.Upperarms);
                }

                if (node.PropertyExists(mcm.torsoPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.torsoPropertyName), ModularCustomizationManager.BodyType.Torso);
                }

                if (node.PropertyExists(mcm.hipsPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.hipsPropertyName), ModularCustomizationManager.BodyType.Hips);
                }

                if (node.PropertyExists(mcm.lowerLegsPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.lowerLegsPropertyName), ModularCustomizationManager.BodyType.LowerLegs);
                }

                if (node.PropertyExists(mcm.feetPropertyName))
                {
                    mcm.UpdateBodyModel((string)node.GetProperty(mcm.feetPropertyName), ModularCustomizationManager.BodyType.Feet);
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
             
            
            DestroyImmediate(AtavismSettings.Instance.PetAvatar.GetComponent <MobController3D>());
            node.GameObject.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
         //     AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
    //AtavismSettings.Instance.OtherCharacterAvatar.layer = 24;
                AtavismSettings.Instance.PetAvatar.transform.position = AtavismSettings.Instance.GetPetPanelSpawn().position;
                AtavismSettings.Instance.PetAvatar.transform.rotation = AtavismSettings.Instance.GetPetPanelSpawn().rotation;
                AtavismSettings.Instance.GetPetPanelCamera().enabled = true;
            }
            if (!showing)  Show();
            //    gameObject.SetActive(true);
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
        
        
        public void Hide()
        {
            StopAllCoroutines();
            AtavismSettings.Instance.CloseWindow(this);
            //    gameObject.SetActive(false);
            showing = false;
            GetComponent<CanvasGroup>().alpha = 0f;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            if (AtavismSettings.Instance.GetPetPanelCamera() != null)
                AtavismSettings.Instance.GetPetPanelCamera().enabled = false;
            _id =-1;
        }

        public static UGUIPetCharacterPanel Instance
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