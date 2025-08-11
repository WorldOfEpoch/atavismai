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

    public class UIAtavismOtherCharacterPanel : UIAtavismWindowBase
    {
        #region variables
        static UIAtavismOtherCharacterPanel instance;
        long _id;
       // public  Label targetName;
       [AtavismSeparator("Stats")]
       [SerializeField]  private VisualTreeAsset statsListElementTemplate;
       [SerializeField] private int numberElementsPerPage = 12;
       private VisualElement statGrid;
       public List<UIStat> StatsName = new List<UIStat>();
        List<UIAtavismCharacterStatEntry> _statEntries = new  List<UIAtavismCharacterStatEntry>();
        
        Button buttonPrev;
        Button buttonNext;
        
        
        // [AtavismSeparator("Slots")]
        Dictionary<string,UIAtavismItemDisplay> slots = new Dictionary<string,UIAtavismItemDisplay>();
        // public List<string> slotName = new List<string>();

        UIAtavismCharacterEquipSlot slotAmmo;
        // [AtavismSeparator("Panel")]
        // public
        // GameObject panel;
        #endregion variables;

        private int page = 0;
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
                UIAtavismItemDisplay val = uiDocument.rootVisualElement.Query<UIAtavismItemDisplay>("equip-slot-"+slot_name);
                if(val != null)
                    slots.Add(slot, val);
            }
            VisualElement ammo = uiDocument.rootVisualElement.Query<VisualElement>("equip-slot-Ammo");
            if (ammo != null)
            { UIAtavismCharacterEquipSlot _slot = new UIAtavismCharacterEquipSlot();
                // Assign the controller script to the visual element
                ammo.userData = _slot;
                // Initialize the controller script
                _slot.SetVisualElement(ammo);
                _slot.ammo = true;
                slotAmmo = _slot;
            }
            return true;
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
           int maxPage = (int)StatsName.Count / numberElementsPerPage;
           if ((StatsName.Count % numberElementsPerPage) != 0)
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
        void Update()
        {
            
            base.Update();
            //   if (Input.GetKeyDown(GameSettings.Instance.settings.keySettings.character) && !GameSettings.UIHasFocus() )
            //        Toggle();
        }
        // public void Toggle()
        // {
        //     if (showing)
        //         Hide();
        //     else
        //         Show();
        // }
        public override void Show()
        {
            base.Show();
           
        }

        public override void Hide()
        {
            base.Hide();
            if(AtavismSettings.Instance.GetOtherCharacterPanelCamera()!=null)
                AtavismSettings.Instance.GetOtherCharacterPanelCamera().enabled = false;
        }


        void UpdateStats(AtavismObjectNode node)
        {
            
            for (int i = page * numberElementsPerPage; i < (page + 1) * numberElementsPerPage; i++)
            {

                if (node != null && StatsName.Count > i && StatsName[i].name.Length > 0)
                {
                    if (node.Properties.ContainsKey(StatsName[i].name))
                    {
                        _statEntries[i - page * numberElementsPerPage].UpdateStat(StatsName[i].displayName,
                            (string)node.GetPropertyStatWithPrecision(StatsName[i].name).ToString());
                    }
                    else
                    {
                        _statEntries[i - page * numberElementsPerPage].UpdateStat("", "");
                    }
                }
                else
                {
                    _statEntries[i - page * numberElementsPerPage].UpdateStat("", "");
                }
            }
        }

        public void UpdateCharacterData(long id)
        {
            _id = id;
            AtavismObjectNode node = ClientAPI.GetObjectNode(id);
            if (uiWindowTitle != null)
            {
                string mName = node.Name;
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != "" && I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName) != null) mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + mName);
#endif
                uiWindowTitle.text = mName.ToUpper();

            }

            UpdateStats(node);

            foreach (var s in slots.Values)
            {
                s.SetItemData(null);
            }
            
            foreach (var slot in Inventory.Instance.itemSlots.Keys)
            {
                string slotData = (string)node.GetProperty(slot+"DisplayVAL");
               // Debug.LogError("slotData in "+slot+" = "+slotData);
                if (slotData != null && slotData.Length > 0)
                {
                    string[] data = slotData.Split(';');
                    int itemId = int.Parse(data[0]);
                    AtavismInventoryItem aii = AtavismPrefabManager.Instance.LoadItem(itemId);
                    if (data.Length > 1)
                    {
                        for (int i = 1; i < data.Length; i++)
                        {
                            if (data[i].StartsWith("E"))
                            {
                                aii.enchantLeval = int.Parse(data[i].Substring(1));
                            }
                            else if (data[i].StartsWith("S"))
                            {
                                string socket = data[i].Substring(1);
                                string[] socketData = socket.Split('|');
                               // Debug.LogError("slotData in "+slot+" = "+slotData+" -> "+socket+" -> "+String.Join(";",socketData));
                                if (!aii.SocketSlots.ContainsKey(socketData[0]))
                                {
                                    aii.SocketSlots.Add(socketData[0],new Dictionary<int, int>());
                                }
                                aii.SocketSlots[socketData[0]].Add(int.Parse(socketData[1]), int.Parse(socketData[2]));
                            }else if (data[i].StartsWith("B"))
                            {
                                string stat = data[i].Substring(1);
                                string[] stats = stat.Split('|');
                                aii.Stats[stats[0]] = int.Parse(stats[1]);
                                
                            }else if (data[i].StartsWith("T"))
                            {
                                string stat = data[i].Substring(1);
                                string[] stats = stat.Split('|');
                                aii.EnchantStats[stats[0]] = int.Parse(stats[1]);
                            }
                        }
                    }

                   // int index = slotName.IndexOf(slot);
                   // if (index > -1)
                   if(slots.ContainsKey(slot))
                       slots[slot].SetItemData(aii);
                }
            }

            if (AtavismSettings.Instance.GetOtherCharacterPanelSpawn() != null)
            {
                if(AtavismSettings.Instance.OtherCharacterAvatar!=null)
                    DestroyImmediate(AtavismSettings.Instance.OtherCharacterAvatar);
                node.GameObject.GetComponent<AtavismMobAppearance>().ResetAttachObject();
              /*  AtavismSettings.Instance.OtherCharacterAvatar = Instantiate(node.GameObject);
                DestroyImmediate(AtavismSettings.Instance.OtherCharacterAvatar.GetComponent <MobController3D>());
                DestroyImmediate(AtavismSettings.Instance.OtherCharacterAvatar.GetComponent <AtavismNode>());
*/
              
              string prefabName = (string) node.GetProperty("model");
              if (prefabName.Contains(".prefab"))
              {
                  int resourcePathPos = prefabName.IndexOf("Resources/");
                  prefabName = prefabName.Substring(resourcePathPos + 10);
                  prefabName = prefabName.Remove(prefabName.Length - 7);
              }
              GameObject prefab = (GameObject)Resources.Load(prefabName);
              
              if (prefab == null)
              {
                  Debug.LogWarning("No Model "+prefabName);
                  return;
              }

              AtavismSettings.Instance.OtherCharacterAvatar = (GameObject) Instantiate(prefab, AtavismSettings.Instance.GetOtherCharacterPanelSpawn().position, AtavismSettings.Instance.GetOtherCharacterPanelSpawn().rotation);
              if(node.PropertyExists("umaData"))
              {	
                  Dictionary<string, object> umaDictionary = (Dictionary<string, object>)node.GetProperty("umaData");
                  //   AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismNode>().AddLocalProperty("umaData",umaDictionary);
                  var node1 = AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<AtavismNode>();
                  if (node1 == null)
                  {
                      node1 = AtavismSettings.Instance.OtherCharacterAvatar.AddComponent<AtavismNode>();
                  }
                  node1.AddLocalProperty("umaData",umaDictionary);
                  node1.AddLocalProperty("genderId",(int)node.GetProperty("genderId"));
                  node1.AddLocalProperty("race",(int)node.GetProperty("race"));
                  node1.AddLocalProperty("aspect",(int)node.GetProperty("aspect"));
                  AtavismSettings.Instance.OtherCharacterAvatar.SendMessage("GrabRecipe", SendMessageOptions.DontRequireReceiver);
              }
              DestroyImmediate(AtavismSettings.Instance.OtherCharacterAvatar.GetComponent <MobController3D>());
              
                var mcm = AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<ModularCustomizationManager>();
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
              
               // AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<MobController3D>().enabled = false;

                node.GameObject.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
                
               // AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
            //    Debug.LogError("Other "+node.Oid+" "+AtavismSettings.Instance.OtherCharacterAvatar.GetComponent<AtavismNode>().Oid);
                //AtavismSettings.Instance.OtherCharacterAvatar.layer = 24;
                AtavismSettings.Instance.OtherCharacterAvatar.transform.position = AtavismSettings.Instance.GetOtherCharacterPanelSpawn().position;
                AtavismSettings.Instance.OtherCharacterAvatar.transform.rotation = AtavismSettings.Instance.GetOtherCharacterPanelSpawn().rotation;
                AtavismSettings.Instance.GetOtherCharacterPanelCamera().enabled = true;
            }


           
            Show();
        }
        public static UIAtavismOtherCharacterPanel Instance
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