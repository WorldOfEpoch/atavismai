using Atavism;
using Atavism.UI;
using HNGamers.Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static HNGamers.Atavism.ModularCustomizationManager;
using UnityEngine.Scripting;
using System.Linq;

namespace Atavism.UI
{

    [Serializable]
    public class UIStat
    {
        public String name="";
        public String displayName="";
        // public Image selected;
        // public List<UGUICharacterEquipSlot> slots;

    }
    public class UIAtavismCharacterProfileManager : UIAtavismWindowBase
    {
        #region variables

        // [AtavismSeparator("Base Screen")] public string uiScreenstring = "Screen";
        long _id;


        // [AtavismSeparator("Other Character Screen")]
        // public string targetName;

        [AtavismSeparator("Stats")] [SerializeField]
        private VisualTreeAsset statsListElementTemplate;

        [SerializeField] private int numberElementsPerPage = 12;
        private VisualElement statGrid;
        public List<UIStat> StatsName = new List<UIStat>();
        List<UIAtavismCharacterStatEntry> _statEntries = new List<UIAtavismCharacterStatEntry>();
        private int page = 0;
        Button buttonPrev;
        Button buttonNext;


        [AtavismSeparator("Slots")]
        // public List<UIAtavismCharacterEquipSlot> slots = new List<UIAtavismCharacterEquipSlot>();
        Dictionary<string, UIAtavismCharacterEquipSlot> slots = new Dictionary<string, UIAtavismCharacterEquipSlot>();

        // Dictionary<string,UIAtavismCharacterEquipSlot> slotsAmmo = new Dictionary<string,UIAtavismCharacterEquipSlot>();
        // public List<string> slotName = new List<string>();
        UIAtavismCharacterEquipSlot slotAmmo;
        // public static UIAtavismCharacterProfileManager Instance => (UIAtavismCharacterProfileManager)instance;
        // static UIAtavismCharacterProfileManager instance;


        public List<string> sets;
        private Dictionary<string, Button> setSlots = new Dictionary<string, Button>();
        float clicklimit =-1;
        #endregion variables;


        new void Start()
        {
            // UIAtavismCharacterProfileManager wnd = GetWindow<UIAtavismCharacterProfileManager>();
            // wnd.titleContent = new GUIContent("Drag And Drop");
            base.Start();
            // if (instance != null)
            // {
            //     GameObject.DestroyImmediate(gameObject);
            //     return;
            // }
            //
            // instance = this;

        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("EQUIPPED_UPDATE", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SLOTS_UPDATE", this);
            
            if (ClientAPI.GetPlayerObject() != null)
            {
                foreach (var stat in StatsName)
                {
                    ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(stat.name, PropHandler);
                }
              
            }
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("EQUIPPED_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SLOTS_UPDATE", this);
            if (ClientAPI.GetPlayerObject() != null)
            {
                foreach (var stat in StatsName)
                {
                    ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(stat.name, PropHandler);
                }
              
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "EQUIPPED_UPDATE" || eData.eventType == "ITEM_ICON_UPDATE" ||
                eData.eventType == "SLOTS_UPDATE")
            {
                // Update 
                UpdateEquipSlots();

            }
            if (eData.eventType == "EQUIPPED_UPDATE")
            {
                clicklimit = 0;
            }
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
                buttonPrev.clicked += OnPrevClick;
            if (buttonNext != null)
                buttonNext.clicked += OnNextClick;
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
                    _slot.slotName = slot;
                    // Initialize the controller script
                    _slot.SetVisualElement(val);
                    slots.Add(slot, _slot);
                }
            }

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

            setSlots.Clear();
            foreach (var set in sets)
            {
                string setName = set.Replace(" ", "-");
                Button setButton = uiDocument.rootVisualElement.Query<Button>("button-"+setName);
                if (setButton != null)
                {
                    setButton.clicked+=()=>
                    {
                        ClickSelectSet(set);
                    };
                    setSlots.Add(set, setButton);
                }
            }
            
            
            return true;
        }
        private void PropHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateStats(ClientAPI.GetPlayerObject());
        }
        private void OnPrevClick()
        {
            page--;
            int maxPage = (int)StatsName.Count / numberElementsPerPage;
            
            // if ((StatsName.Count % numberElementsPerPage) != 0)
            //     maxPage++;
            if (page < 0)
                page = maxPage;
            UpdateStats(ClientAPI.GetPlayerObject());
        }

        private void OnNextClick()
        {
            page++;
            int maxPage = (int)StatsName.Count / numberElementsPerPage;
            
            if ((StatsName.Count % numberElementsPerPage) != 0)
                maxPage++;
            if (page >= maxPage)
                page = 0;
            UpdateStats(ClientAPI.GetPlayerObject());
        }



        public override void Show()
        {
            base.Show();
            // UpdateOtherCharacterData(ClientAPI.GetPlayerOid());
            updateCharacterData();
            UpdateEquipSlots();
        }

        public void Hide()
        {
            base.Hide();

        }

        private void UpdateEquipItemData(AtavismInventoryItem item)
        {

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

        public long Id
        {
            get { return _id; }
        }

   public void UpdateEquipSlots()
        {
            UpdateStats(ClientAPI.GetPlayerObject());

            foreach (var slotName in slots.Keys)
            {
                AtavismInventoryItem item = GetItemInSlot(slotName);
                slots[slotName].UpdateEquipItemData(item);
            }
            
                //	ammoSlot.gameObject.SetActive(true);
                if (slotAmmo != null)
                    slotAmmo.UpdateEquipItemData(Inventory.Instance.EquippedAmmo);
            if (setSlots != null)
                foreach (var set in setSlots.Keys)
                {
                    if (Inventory.Instance.sets.Contains(set))
                    {
                        setSlots[set].ShowVisualElement();
                        if (set.Equals(Inventory.Instance.GetSetSelected))
                        {
                            setSlots[set].AddToClassList("equip-set-selected");
                        }
                        else
                        {
                            setSlots[set].RemoveFromClassList("equip-set-selected");
                        }
                    }
                    else
                    {
                        setSlots[set].HideVisualElement();
                    }
                }
        }

      AtavismInventoryItem GetItemInSlot(string slotName)
        {
            foreach (AtavismInventoryItem item in Inventory.Instance.EquippedItems.Values)
            {
                string orgSlotName = Inventory.Instance.GetItemByTemplateID(item.TemplateId).slot;

                if (Inventory.Instance.itemGroupSlots.ContainsKey(slotName))
                {
                    //Debug.LogError(item.ItemId+" found Group "+slotName+" GS:"+Inventory.Instance.itemGroupSlots[slotName]);
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


        public void updateCharacterData()
        {
            //
          
            if (AtavismSettings.Instance.GetCharacterPanelSpawn() != null)
            {
                if (AtavismSettings.Instance.CharacterAvatar != null)
                    DestroyImmediate(AtavismSettings.Instance.CharacterAvatar);
                ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobAppearance>().ResetAttachObject();
                /*
                  AtavismSettings.Instance.CharacterAvatar = Instantiate(ClientAPI.GetPlayerObject().GameObject);

                  DestroyImmediate(AtavismSettings.Instance.CharacterAvatar.GetComponent <MobController3D>());
                  DestroyImmediate(AtavismSettings.Instance.CharacterAvatar.GetComponent <AtavismNode>());
    */

                string prefabName = (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                    .GetProperty("model");
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

                AtavismSettings.Instance.CharacterAvatar = (GameObject)Instantiate(prefab,
                    AtavismSettings.Instance.GetCharacterPanelSpawn().position,
                    AtavismSettings.Instance.GetCharacterPanelSpawn().rotation);
                if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists("umaData"))
                {
                    Dictionary<string, object> umaDictionary = (Dictionary<string, object>)ClientAPI.GetPlayerObject()
                        .GameObject.GetComponent<AtavismNode>().GetProperty("umaData");
                    //   AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismNode>().AddLocalProperty("umaData",umaDictionary);
                    var node = AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismNode>();
                    if (node == null)
                    {
                        node = AtavismSettings.Instance.CharacterAvatar.AddComponent<AtavismNode>();
                    }

                    node.AddLocalProperty("umaData", umaDictionary);
                    node.AddLocalProperty("genderId",
                        (int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty("genderId"));
                    node.AddLocalProperty("race",
                        (int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("race"));
                    node.AddLocalProperty("aspect",
                        (int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("aspect"));
                    AtavismSettings.Instance.CharacterAvatar.SendMessage("GrabRecipe",
                        SendMessageOptions.DontRequireReceiver);
                }

                var mcm = AtavismSettings.Instance.CharacterAvatar.GetComponent<ModularCustomizationManager>();
                if (mcm != null)
                {

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.EyeMaterialPropertyName))
                    {
                        mcm.UpdateEyeMaterial((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.EyeMaterialPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.HairMaterialPropertyName))
                    {
                        mcm.UpdateHairMaterial((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.HairMaterialPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.SkinMaterialPropertyName))
                    {
                        mcm.UpdateSkinMaterial((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.SkinMaterialPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.MouthMaterialPropertyName))
                    {
                        mcm.UpdateMouthMaterial((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.MouthMaterialPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.bodyColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.bodyColorPropertyName).ToString().Split(',');
                        Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBodyColor(test);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.scarColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.scarColorPropertyName).ToString().Split(',');
                        Color32 test = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBodyScarColor(test);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.hairColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.hairColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateHairColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.mouthColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.mouthColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateMouthColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.beardColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.beardColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBeardColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.eyeBrowColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.eyeBrowColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateEyebrowColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.stubbleColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.stubbleColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateStubbleColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.bodyArtColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.bodyArtColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateBodyArtColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.eyeColorPropertyName))
                    {
                        var item = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.eyeColorPropertyName).ToString().Split(',');
                        Color32 color32 = new Color32(Convert.ToByte(item[0]), Convert.ToByte(item[1]),
                            Convert.ToByte(item[2]), Convert.ToByte(item[3]));
                        mcm.UpdateEyeColor(color32);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.helmetColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.helmetColorPropertyName).ToString().Split('@');
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

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.torsoColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.torsoColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.Torso);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.upperArmsColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.upperArmsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.Upperarms);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.lowerArmsColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.lowerArmsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.LowerArms);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.hipsColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.hipsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.Hips);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.lowerLegsColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.lowerLegsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.LowerLegs);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.feetColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.feetColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.Feet);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.handsColorPropertyName))
                    {
                        var colorProperties = ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.handsColorPropertyName).ToString().Split('@');
                        foreach (var colorProperty in colorProperties)
                        {
                            var colorPropertyItem = colorProperty.Split(':');
                            var colorslot = colorPropertyItem[0];
                            var coloritem = colorPropertyItem[1].Split(',');
                            Color32 color32 = new Color32(Convert.ToByte(coloritem[0]), Convert.ToByte(coloritem[1]),
                                Convert.ToByte(coloritem[2]), Convert.ToByte(coloritem[3]));
                            mcm.UpdateShaderColor(color32, colorslot, BodyType.Hands);
                        }
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.hairPropertyName))
                    {
                        mcm.UpdateHairModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.hairPropertyName));
                    }



                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.beardPropertyName))
                    {
                        mcm.UpdateBeardModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.beardPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.eyebrowPropertyName))
                    {

                        mcm.UpdateEyebrowModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.eyebrowPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.headPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.headPropertyName), BodyType.Head);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.faceTexPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.faceTexPropertyName), BodyType.Face);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.handsPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.handsPropertyName), BodyType.Hands);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.lowerArmsPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.lowerArmsPropertyName), BodyType.LowerArms);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.upperArmsPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.upperArmsPropertyName), BodyType.Upperarms);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.torsoPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.torsoPropertyName), BodyType.Torso);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.hipsPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.hipsPropertyName), BodyType.Hips);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.lowerLegsPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.lowerLegsPropertyName), BodyType.LowerLegs);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.feetPropertyName))
                    {
                        mcm.UpdateBodyModel(
                            (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                                .GetProperty(mcm.feetPropertyName), BodyType.Feet);
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.earsPropertyName))
                    {
                        mcm.UpdateEarModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.earsPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.eyesPropertyName))
                    {
                        mcm.UpdateEyeModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.eyesPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.tuskPropertyName))
                    {
                        mcm.UpdateTuskModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.tuskPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.mouthPropertyName))
                    {
                        mcm.UpdateMouthModel((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.mouthPropertyName));
                    }

                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                        .PropertyExists(mcm.faithPropertyName))
                    {
                        mcm.SetFaith((string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>()
                            .GetProperty(mcm.faithPropertyName));
                    }
#if IPBRInt
                if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists(mcm.blendshapePresetValue) && (!mcm.enableSavingInfinityPBRBlendshapes))
                {
                    mcm.UpdateBlendShapePresets((int)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty(mcm.blendshapePresetValue));
                }

                if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists(mcm.infinityBlendShapes))
                {
                    mcm.UpdateBlendShapes((string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty(mcm.infinityBlendShapes));
                }
#endif

                }




                DestroyImmediate(AtavismSettings.Instance.CharacterAvatar.GetComponent<MobController3D>());


                ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
                //     AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismMobAppearance>().ReApplyEquipDisplay();
                //AtavismSettings.Instance.OtherCharacterAvatar.layer = 24;
                AtavismSettings.Instance.CharacterAvatar.transform.position =
                    AtavismSettings.Instance.GetCharacterPanelSpawn().position;
                AtavismSettings.Instance.CharacterAvatar.transform.rotation =
                    AtavismSettings.Instance.GetCharacterPanelSpawn().rotation;
                AtavismSettings.Instance.GetCharacterPanelCamera().enabled = true;
            }
            //    gameObject.SetActive(true);
        }


        // Update is called once per frame
        new void Update()
        {
            base.Update();
            foreach (var slotName in slots.Keys)
            {
                slots[slotName].Update();
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            // Show();
        }

        public void ClickSelectSet(String set)
        {
            
            if (clicklimit > Time.time)
                return;
            clicklimit = Time.time + 1f;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("set", set);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.SWITCH_WEAPON", props);
        }

    }
}