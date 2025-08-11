using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Atavism.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Atavism
{
    [Serializable]
    public class GlobalEventData
    {
        public int id = -1;
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public long date = 0L;
    }

    [Serializable]
    public class RecourceNodeProfileSettingData
    {
        public int id = -1;
        [NonSerialized] public Texture2D cursorIcon;
        public string cursorIconData = "";
        public string cursorIconPath = "";
        [NonSerialized] public Sprite selectedIcon;
        public string selectedIconData = "";
        public string selectedIconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public long date = 0L;
    }

    [Serializable]
    public class RecourceNodeProfileData
    {
        public int id = -1;
        public long date = 0L;
        public Dictionary<int, RecourceNodeProfileSettingData> settingList = new Dictionary<int, RecourceNodeProfileSettingData>();
    }

    [Serializable]
    public class StatProfileData
    {
        public int id = -1;
        public string name = "";
        public int precision = 0;
        public float division = 1;
        public long date = 0L;
    }

    [Serializable]
    public class RaceData
    {
        public int id = -1;
        public string name = "";
        public string description = "";
        [NonSerialized] public Sprite icon;
        [NonSerialized] public bool iconloaded = false;
        public string iconData = "";
        public string iconPath = "";
        public Dictionary<int, ClassData> classList = new Dictionary<int, ClassData>();
        public long date = 0L;
    }
    
    
    

    [Serializable]
    public class ClassData
    {
        public int id = -1;
        public string name = "";
        public string description = "";
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public Dictionary<int, GenderData> genderList = new Dictionary<int, GenderData>();
    }

    [Serializable]
    public class GenderData
    {
        public int id = -1;
        public string name = "";
        public string prefab = "";
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
    }

    [Serializable]
    public class ItemPrefabData
    {
        public int templateId = -1;
        public string name;
        [NonSerialized] public Sprite icon;
        [NonSerialized] public bool iconloaded = false;
        public string iconData = "";
        public string iconPath = "";
        public string tooltip;
        public string itemType = "";
        public string subType = "";
        public string slot = "";
        public int quality = 0;
        public int currencyType = 0;
        public long cost = 0;
        public int binding = 0;
        public bool sellable = true;
        public int damageValue = 0;
        public int damageMaxValue = 0;
        public int setId = 0;
        public int enchantId = 0;
        public int weaponSpeed = 2000;
        public int stackLimit = 1;
        public bool auctionHouse = false;
        public bool unique = false;
        public int gear_score = 0;
        public int weaponProfile = -1;
        public string sockettype = "";
        public int durability = 0;
        public int weight = 0;
        public int autoattack = -1;
        public int ammoType = -1;
        public int deathLoss = 0;
        public bool parry = false;
        public bool repairable = true;
        public string groundPrefab = "";
        public int audioProfile = -1;
        public List<string> itemEffectTypes = new List<string>();
        public List<string> itemEffectNames = new List<string>();
        public List<string> itemEffectValues = new List<string>();
        public List<string> itemReqTypes = new List<string>();
        public List<string> itemReqNames = new List<string>();
        public List<string> itemReqValues = new List<string>();
        public long date = 0L;
    }

    [Serializable]
    public class ItemSetPrefabData
    {
        public int Setid = 0;
        public string Name = "name"; // The enchant profile name
        public List<int> itemList = new List<int>();
        public List<AtavismInventoryItemSetLevel> levelList = new List<AtavismInventoryItemSetLevel>();
        public long date = 0L;
    }

    [Serializable]
    public class AbilityPrefabData
    {
        public int id = 0;
        public string name;
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;

        public string tooltip;

        //public string rank = "";
        public int cost = 0;
        public int pulseCost = 0;
        public float costPercentage = 0;
        public float pulseCostPercentage = 0;
        public string costProperty = "mana";
        public string pulseCostProperty = "mana";
        public bool globalcd = false;
        public bool weaponcd = false;
        public string cooldownType = "";
        public float cooldownLength = 0;
        public string weaponReq = "";
        public int reagentReq = -1;
        public int distance = 0;
        public bool castingInRun = false;
        public TargetType targetType;
        public TargetSubType targetSubType;
        public float castTime = 0;
        public float speed = 0;
        public bool passive = false;
        public string aoeType = "";
        public int maxRange = 4; // How far away the target can be (in metres)
        public int minRange = 0; // How close the target can be (in metres)
        public int aoeRadius = 1;

        public string aoePrefab = "";

        //public int reqLevel = 1;
        public bool toggle = false;
        public LinkedList<long> powerup = new LinkedList<long>();
        public long date = 0L;
    }

    [Serializable]
    public class EffectsPrefabData
    {
        public int id;
        public string name;
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public string tooltip;
        public long date = 0L;
        public bool show = true;

        public bool isBuff;
        public int stackLimit = 1;
        public bool stackTime = false;

        public bool allowMultiple = false;
        /*   float length;
           float expiration = -1;
           bool active = false;
           bool passive = false;
           */
    }

    [Serializable]
    public class CurrencyPrefabData
    {
        public int id = -1;
        public string name = "";
        public string description = "";
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public int group = 1;
        public int position = 1;
        public int convertsTo = -1;
        public int conversionAmountReq = 1;
        public long max = 999999;
        public long date = 0L;
    }

    [Serializable]
    public class SkillPrefabData
    {
        public int id = 0;
        public string skillname = "";
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public int mainAspect = -1;
        public int type = -1;
        public int oppositeAspect = -1;
        public bool mainAspectOnly = false;
        public int parentSkill = -1;
        public int parentSkillLevelReq = -1;
        public int playerLevelReq = 1;
        public int pcost = 0;
        public bool talent = false;
        public List<int> abilities = new List<int>();
        public List<int> abilityLevelReqs = new List<int>();
        public long date = 0L;
    }


    [Serializable]
    public class BuildObjPrefabData
    {
        public int id = 0;
        public string buildObjectName = "";
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public int category = 0;
        public List<int> objectCategory = new List<int>();
        public int skill = -1;
        public int skillLevelReq = 0;
        public float distanceReq = 1f;
        public float buildTimeReq = 1f;

        public bool buildTaskReqPlayer = true;

//        public ClaimType validClaimTypes = ClaimType.Any;
        public List<int> validClaimTypes = new List<int>();
        public bool onlyAvailableFromItem = false;
        public string reqWeapon = "";
        public string gameObject = "";

        public List<int> itemsReq = new List<int>();
        public List<int> itemsReqCount = new List<int>();

        public List<int> upgradeItemsReq = new List<int>();

        public long date = 0L;
        //   public Dictionary<int, int> itemReqs = new Dictionary<int, int>();
    }

    [Serializable]
    public class CraftingRecipePrefabData
    {
        public int recipeID;
        public string recipeName;
        [NonSerialized] public Sprite icon;
        public string iconData = "";
        public string iconPath = "";
        [NonSerialized] public bool iconloaded = false;
        public int skillID = -1;
        public int skillLevelReq = -1;
        public string stationReq = "";
        public int creationTime = 0;

        public List<int> createsItems = new List<int>();
        public List<int> createsItemsCounts = new List<int>();
        public List<int> createsItems2 = new List<int>();
        public List<int> createsItemsCounts2 = new List<int>();
        public List<int> createsItems3 = new List<int>();
        public List<int> createsItemsCounts3 = new List<int>();
        public List<int> createsItems4 = new List<int>();
        public List<int> createsItemsCounts4 = new List<int>();
        public List<int> itemsReq = new List<int>();
        public List<int> itemsReqCounts = new List<int>();
        public long date = 0L;
    }

    [Serializable]
    public class QuestData
    {
        public int id;
        public string title;
        public string description;
        public string objective;
        public string progress;
        public LinkedList<QuestObjectiveData> objectives = new LinkedList<QuestObjectiveData>();
        public Dictionary<int, QuestRewardGradeData> rewardsGrades = new Dictionary<int, QuestRewardGradeData>();
        public long date = 0L;
    }

    [Serializable]
    public class QuestObjectiveData
    {
        public int count;
        public string name;
        public string type;
    }

    [Serializable]
    public class QuestRewardGradeData
    {
        public int exp;
        public string completeText;
        public Dictionary<int, int> rewardsItems = new Dictionary<int, int>();
        public Dictionary<int, int> rewardsToChooseItems = new Dictionary<int, int>();
        public Dictionary<string, int> rewardsReputation = new Dictionary<string, int>();
        public Dictionary<int, int> rewardsCurrency = new Dictionary<int, int>();
    }

    [Serializable]
    public class AtavismPrefabData
    {
        public Dictionary<int, ItemPrefabData> items = new Dictionary<int, ItemPrefabData>();
        public Dictionary<int, ItemSetPrefabData> itemSets = new Dictionary<int, ItemSetPrefabData>();
        public Dictionary<int, CraftingRecipePrefabData> craftRecipes = new Dictionary<int, CraftingRecipePrefabData>();
        public Dictionary<int, CurrencyPrefabData> currencies = new Dictionary<int, CurrencyPrefabData>();

        public Dictionary<int, BuildObjPrefabData> buildObjects = new Dictionary<int, BuildObjPrefabData>();

        public Dictionary<int, SkillPrefabData> skills = new Dictionary<int, SkillPrefabData>();
        public Dictionary<int, AbilityPrefabData> abilities = new Dictionary<int, AbilityPrefabData>();
        public Dictionary<int, EffectsPrefabData> effects = new Dictionary<int, EffectsPrefabData>();
        public Dictionary<int, RaceData> races = new Dictionary<int, RaceData>();

        public Dictionary<int, RecourceNodeProfileData> recourceNode = new Dictionary<int, RecourceNodeProfileData>();
        public Dictionary<int, GlobalEventData> globalEvents = new Dictionary<int, GlobalEventData>();
        public Dictionary<int, QuestData> quests = new Dictionary<int, QuestData>();
        public Dictionary<int, WeaponProfileData> weaponProfiles = new Dictionary<int, WeaponProfileData>();
        public Dictionary<int, ItemAudioProfileData> itemAudioProfiles = new Dictionary<int, ItemAudioProfileData>();
        public Dictionary<int, StatProfileData> statProfiles = new Dictionary<int, StatProfileData>();
        public Dictionary<int, InteractiveObjectProfileData> interactiveObjectProfiles = new Dictionary<int, InteractiveObjectProfileData>();
        public Dictionary<int , SlotsProfileData> slotsProfiles = new Dictionary<int , SlotsProfileData>();
    }

    [Serializable]
    public class AtavismIconData
    {
        [NonSerialized] public Sprite icon;
        [NonSerialized] public Texture2D icon2d;
        public string iconData = "";
    }

    [Serializable]
    public class WeaponProfileActionData
    {
        public string action;
        public int abilityId;
        public bool zoom;
        public string slot;
        public string coordEffect;
        
    }

    [Serializable]
    public class WeaponProfileData
    {
        public int id;
        public string name;
        public long date = 0L;
        public List<WeaponProfileActionData> actions = new List<WeaponProfileActionData>();
        
    }
    
    [Serializable]
    public class ItemAudioProfileData
    {
        public int id;
        public string name;
        public string use;
        public string drag_begin;
        public string drag_end;
        public string delete;
        public string broke;
        public string pick_up;
        public string fall;
        public string drop;
        public long date = 0L;
    }
    
    [Serializable]
    public class InteractiveObjectProfileData
    {
        public int id;
        public string name;
        public string interactionType;
        public int useLimit;

        public int minLevel;
        public int maxLevel;
        public int questRequirement;
        public int itemRequirement;
        public int itemCountRequirement;
        public bool itemRequirementGet = true;
        public int currencyRequirement;
        public int currencyCountRequirement;
        public bool currencyRequirementGet = true;
        public string iconData = "";
        public string iconPath = "";
        // public int pick_up;
        // public int fall;
        // public int drop;
        public long date = 0L;
    }
    
    [Serializable]
    public class SlotsProfileData
    {
        public int id;
        public string name;
        public long date = 0L;
        public List<string> slots = new List<string>();
        
    }
    public class AtavismPrefabManager : MonoBehaviour
    {
        static AtavismPrefabManager instance;
        // Start is called before the first frame update

        [HideInInspector] public AtavismPrefabData prefabsdatadata = new AtavismPrefabData();
        Dictionary<int, float> itemIconGet = new Dictionary<int, float>();
        Dictionary<int, float> abilityIconGet = new Dictionary<int, float>();
        Dictionary<int, float> skillIconGet = new Dictionary<int, float>();
        Dictionary<int, float> currencyIconGet = new Dictionary<int, float>();
        Dictionary<int, float> effectsIconGet = new Dictionary<int, float>();
        private Dictionary<int, float> globalEventsIconGet = new Dictionary<int, float>();
        Dictionary<string, float> resourceIconGet = new Dictionary<string, float>();
        Dictionary<int, float> craftRecipesIconGet = new Dictionary<int, float>();

        Dictionary<int, float> buildObjectsIconGet = new Dictionary<int, float>();

        //  Dictionary<int, float> effectsIconGet = new Dictionary<int, float>();
        protected bool prefabReloading = false;
        protected int toReload = 11;
        [HideInInspector] public int reloaded = 0;
        [SerializeField] private bool loadOnStartup = false;
        [SerializeField] private bool forceLoadAllPrefabsFromServer = false;
        public bool useOldStoreIconsInDefinitions = false;

        public Dictionary<string, AtavismIconData> prefabIconData = new Dictionary<string, AtavismIconData>();

        public bool PrefabReloading
        {
            get { return prefabReloading; }
        }

        public int ToReload
        {
            get { return toReload; }
        }

        void Start()
        {

            if (instance != null)
            {
                return;
            }

            instance = this;
            SceneManager.sceneLoaded += sceneLoaded;

#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA_10_0 || UNITY_WSA
            if (!Directory.Exists(Application.persistentDataPath+"/" + Application.productName))
                Directory.CreateDirectory(Application.persistentDataPath+"/" + Application.productName);

            
            if (!forceLoadAllPrefabsFromServer && File.Exists( Application.persistentDataPath+"/" + Application.productName + "/game_icon.data"))
            {
                try
                {
                FileStream file = File.Open( Application.persistentDataPath+"/" + Application.productName + "/game_icon.data", FileMode.Open);
#else
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName);
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName);
            if (!forceLoadAllPrefabsFromServer && File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/game_icon.data"))
            {
                try
                {
                    FileStream file = File.Open(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/game_icon.data", FileMode.Open);

#endif
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        prefabIconData = (Dictionary<string, AtavismIconData>)bf.Deserialize(file);
                        // if (prefabsdatadata.races == null)
                        //     prefabsdatadata.races = new Dictionary<int, RaceData>();


                    }
                    catch (SerializationException e)
                    {
                        Debug.LogError("Failed to deserialize. Reason: " + e.Message + "\n\n" + e.StackTrace);
                        prefabIconData = new Dictionary<string, AtavismIconData>();
                        // throw;
                    }
                    finally
                    {
                        file.Close();
                    }
                    if (prefabsdatadata.statProfiles == null)
                        prefabsdatadata.statProfiles = new Dictionary<int, StatProfileData>();
                    if (prefabsdatadata.weaponProfiles == null)
                        prefabsdatadata.weaponProfiles = new Dictionary<int, WeaponProfileData>();
                    if (prefabsdatadata.itemAudioProfiles == null)
                        prefabsdatadata.itemAudioProfiles = new Dictionary<int, ItemAudioProfileData>();
                    if (prefabsdatadata.interactiveObjectProfiles == null)
                        prefabsdatadata.interactiveObjectProfiles = new Dictionary<int, InteractiveObjectProfileData>();
                    if (prefabsdatadata.slotsProfiles == null)
                        prefabsdatadata.slotsProfiles = new Dictionary<int, SlotsProfileData>();
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Failed open file. Reason: " + e.Message + "\n\n" + e.StackTrace);
                    if(prefabIconData==null)
                        prefabIconData = new Dictionary<string, AtavismIconData>();
                    // throw;
                }
            }
            else
            {
                prefabIconData = new Dictionary<string, AtavismIconData>();
            }


#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA_10_0 || UNITY_WSA
            if (!forceLoadAllPrefabsFromServer && File.Exists( Application.persistentDataPath+"/" + Application.productName + "/game.data"))
            {
                try
                {
                FileStream file = File.Open( Application.persistentDataPath+"/" + Application.productName + "/game.data", FileMode.Open);
#else
            if (!forceLoadAllPrefabsFromServer && File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/game.data"))
            {
                try
                {
                    FileStream file = File.Open(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/game.data", FileMode.Open);

#endif
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        prefabsdatadata = (AtavismPrefabData)bf.Deserialize(file);
                        if (prefabsdatadata.races == null)
                            prefabsdatadata.races = new Dictionary<int, RaceData>();

                        if (prefabsdatadata.recourceNode == null)
                            prefabsdatadata.recourceNode = new Dictionary<int, RecourceNodeProfileData>();
                        if (prefabsdatadata.globalEvents == null)
                            prefabsdatadata.globalEvents = new Dictionary<int, GlobalEventData>();
                        if (prefabsdatadata.statProfiles == null)
                            prefabsdatadata.statProfiles = new Dictionary<int, StatProfileData>();
                        if (prefabsdatadata.interactiveObjectProfiles == null)
                            prefabsdatadata.interactiveObjectProfiles = new Dictionary<int, InteractiveObjectProfileData>();
                        if(prefabsdatadata.slotsProfiles == null)
                            prefabsdatadata.slotsProfiles = new Dictionary<int, SlotsProfileData>();
                        
                List<int> todatete = new List<int>();
                foreach (BuildObjPrefabData item in prefabsdatadata.buildObjects.Values)
                {
                    if(item.iconPath==null)
                        todatete.Add(item.id);
                   
                }

                foreach (int id in todatete)
                    prefabsdatadata.buildObjects.Remove(id);
                todatete.Clear();
                foreach (ItemPrefabData item in prefabsdatadata.items.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.templateId);
                }

                foreach (int id in todatete)
                    prefabsdatadata.items.Remove(id);
                todatete.Clear();
                foreach (CraftingRecipePrefabData item in prefabsdatadata.craftRecipes.Values)
                {
                    if(item.iconPath==null)
                        todatete.Add(item.recipeID);

                }

                foreach (int id in todatete)
                    prefabsdatadata.craftRecipes.Remove(id);
                todatete.Clear();


                foreach (CurrencyPrefabData item in prefabsdatadata.currencies.Values)
                {
                    if(item.iconPath==null)
                           todatete.Add(item.id);
                }

                foreach (int id in todatete)
                    prefabsdatadata.currencies.Remove(id);
                todatete.Clear();

                foreach (AbilityPrefabData item in prefabsdatadata.abilities.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);
                }

                foreach (int id in todatete)
                    prefabsdatadata.abilities.Remove(id);
                todatete.Clear();

                foreach (SkillPrefabData item in prefabsdatadata.skills.Values)
                {
                    if(item.iconPath==null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.skills.Remove(id);
                todatete.Clear();
                foreach (EffectsPrefabData item in prefabsdatadata.effects.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.effects.Remove(id);
                
                todatete.Clear();
                foreach (GlobalEventData item in prefabsdatadata.globalEvents.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.globalEvents.Remove(id);

                todatete.Clear();
                foreach (RecourceNodeProfileData item in prefabsdatadata.recourceNode.Values)
                {
                    foreach (var s in item.settingList.Values)
                    {
                        if (s.selectedIconPath == null || s.cursorIconPath == null)
                        {
                            todatete.Add(item.id);
                        }
                    }

                }

                foreach (int id in todatete)
                    prefabsdatadata.recourceNode.Remove(id);

                
                todatete.Clear();
                foreach (RaceData item in prefabsdatadata.races.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.races.Remove(id);

                
                        
                        
                        
                        foreach (RaceData race in prefabsdatadata.races.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading Race " + race.id);

                            // if (race.iconData.Length > 0)
                            // {
                            //     Texture2D tex = new Texture2D(2, 2);
                            //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(race.iconData));
                            //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            //     race.icon = sprite;
                            // }

                            foreach (ClassData _class in race.classList.Values)
                            {
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Loading Class " + _class.id);
                                // if (_class.iconData.Length > 0)
                                // {
                                //     Texture2D tex = new Texture2D(2, 2);
                                //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(_class.iconData));
                                //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                //     _class.icon = sprite;
                                // }

                                foreach (GenderData gender in _class.genderList.Values)
                                {
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Loading Gender " + gender.id);
                                    // if (gender.iconData.Length > 0)
                                    // {
                                    //     Texture2D tex = new Texture2D(2, 2);
                                    //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(gender.iconData));
                                    //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                    //     gender.icon = sprite;
                                    // }


                                }

                            }

                        }



                        foreach (BuildObjPrefabData item in prefabsdatadata.buildObjects.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading Build Object " + item.buildObjectName);
                            // if (item.iconData.Length > 0)
                            // {
                            //     Texture2D tex = new Texture2D(2, 2);
                            //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                            //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            //     item.icon = sprite;
                            // }

                        }

                        // foreach (ItemPrefabData item in prefabsdatadata.items.Values)
                        // {
                        //     if (AtavismLogger.logLevel <= LogLevel.Debug)
                        //         Debug.Log("Loading Item " + item.name);
                        //     if (item.iconData.Length > 0)
                        //     {
                        //         Texture2D tex = new Texture2D(2, 2);
                        //         bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                        //         Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        //         item.icon = sprite;
                        //     }
                        // }

                        foreach (CraftingRecipePrefabData item in prefabsdatadata.craftRecipes.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading crafting recipe  " + item.recipeName);
                            // if (item.iconData.Length > 0)
                            // {
                            //     Texture2D tex = new Texture2D(2, 2);
                            //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                            //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            //     item.icon = sprite;
                            // }
                        }


                        foreach (CurrencyPrefabData item in prefabsdatadata.currencies.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading currency " + item.name);
                            // if (item.iconData.Length > 0)
                            // {
                            //     Texture2D tex = new Texture2D(2, 2);
                            //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                            //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            //     item.icon = sprite;
                            // }
                        }

                        // foreach (AbilityPrefabData item in prefabsdatadata.abilities.Values)
                        // {
                        //     if (AtavismLogger.logLevel <= LogLevel.Debug)
                        //         Debug.Log("Loading ability " + item.name);
                        //     if (item.iconData.Length > 0)
                        //     {
                        //         Texture2D tex = new Texture2D(2, 2);
                        //         bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                        //         Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        //         item.icon = sprite;
                        //     }
                        // }

                        foreach (SkillPrefabData item in prefabsdatadata.skills.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading Skill " + item.skillname);
                            // if (item.iconData.Length > 0)
                            // {
                            //     Texture2D tex = new Texture2D(2, 2);
                            //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                            //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            //     item.icon = sprite;
                            // }
                        }

                        foreach (EffectsPrefabData item in prefabsdatadata.effects.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading effect " + item.name);
                            // if (item.iconData.Length > 0)
                            // {
                            //     Texture2D tex = new Texture2D(2, 2);
                            //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(item.iconData));
                            //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            //     item.icon = sprite;
                            // }
                        }

                        foreach (ItemSetPrefabData item in prefabsdatadata.itemSets.Values)
                        {
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Loading item set " + item.Name);

                        }

                        foreach (RecourceNodeProfileData node in prefabsdatadata.recourceNode.Values)
                        {
                            foreach (RecourceNodeProfileSettingData set in node.settingList.Values)
                            {
                                // if (set.cursorIconData.Length > 0)
                                // {
                                //     Texture2D tex = new Texture2D(2, 2);
                                //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(set.cursorIconData));
                                //     //Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                //     set.cursorIcon = tex;
                                // }
                                //
                                // if (set.selectedIconData.Length > 0)
                                // {
                                //     Texture2D tex = new Texture2D(2, 2);
                                //     bool wyn = tex.LoadImage(System.Convert.FromBase64String(set.selectedIconData));
                                //     Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                //     set.selectedIcon = sprite;
                                // }
                            }
                        }

                        foreach (GlobalEventData gv in prefabsdatadata.globalEvents.Values)
                        {
                            /* if (AtavismLogger.logLevel <= LogLevel.Debug)
                                 Debug.Log("Loading Global Event " + gv.id);
                             if (gv.iconData.Length > 0)
                             {
                                 Texture2D tex = new Texture2D(2, 2);
                                 bool wyn = tex.LoadImage(System.Convert.FromBase64String(gv.iconData));
                                 Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                 gv.icon = sprite;
                             }*/
                        }

                    }
                    catch (SerializationException e)
                    {
                        Debug.LogError("Failed to deserialize. Reason: " + e.Message + "\n\n" + e.StackTrace);
                        prefabsdatadata = new AtavismPrefabData();
                        // throw;
                    }
                    finally
                    {
                        file.Close();
                    }
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Failed open file. Reason: " + e.Message + "\n\n" + e.StackTrace);
                    prefabsdatadata = new AtavismPrefabData();
                    // throw;
                }
            }
            else
            {
                prefabsdatadata = new AtavismPrefabData();
            }

            if (prefabsdatadata.quests == null)
            {
                prefabsdatadata.quests = new Dictionary<int, QuestData>();
            }

            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("GetCountData", HandleGetCountData);
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("ServerVersion", HandleServerVersion);

            AtavismEventSystem.RegisterEvent("ITEM_RELOAD", this);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SKILL_ICON_UPDATE", this);
            AtavismEventSystem.RegisterEvent("EFFECT_ICON_UPDATE", this);
            
            Dictionary<string, object> prop = new Dictionary<string, object>();
            AtavismClient.Instance.NetworkHelper.GetPrefabs(prop, "GetVersion");
            
            if (loadOnStartup)
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "GetCountData");
                string[] arg = new string[1];
                AtavismEventSystem.DispatchEvent("LOADING_PREFAB_SHOW", arg);

            }

            LoadStatPrefabData();
            LoadRaceData();
            LoadItemPrefabData();
            LoadItemSetPrefabData();
            LoadWeaponProfilesPrefabData();
            LoadItemAudioProfilesPrefabData();
            LoadQuestPrefabData();
            LoadCurrencyPrefabData();
            LoadSkillsPrefabData();
            LoadAbilitiesPrefabData();
            LoadEffectsPrefabData();
            LoadResourceNodePrefabData();
            LoadBuldingObjectsPrefabData();
            LoadCraftingRecipePrefabData();

            LoadGlobalEventsData();
            LoadInteractiveObjectProfilesData();
            LoadSlotsProfilesData();
            
            NetworkAPI.RegisterExtensionMessageHandler("ao.reloaded", HandleReloaded);
            NetworkAPI.RegisterExtensionMessageHandler("combatSettings", HandleCombatSettings);

            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("LOAD_PREFAB", args);
            toReload = 11;


        }

        void HandleServerVersion(Dictionary<string, object> props)
        {
            string version = (string)props["v"];
            string date = (string)props["d"];
            UnityEngine.Debug.Log("Atavism Server version <color=green>" + version + "</color>"+" "+date);

            if (!version.Equals(AtavismClient.Version))
            {
#if AT_I2LOC_PRESET
				string errorMessage = I2.Loc.LocalizationManager.GetTranslation("Server version mismatch");
#else
                string errorMessage = "Server version mismatch";
#endif
                AtavismLogger.LogError(errorMessage);
               if(UIAtavismDialogPopupManager.Instance != null) UIAtavismDialogPopupManager.Instance.ShowDialogPopup(errorMessage);
               if(UGUIDialogPopup.Instance != null) UGUIDialogPopup.Instance.ShowDialogPopup(errorMessage,true);
            }
            
        }
        
        private int items, craft, curr, itset, skill, ability, effects, building, resource, race, global, quest , wProfile, iaProfile, ioProfile, sProfile= 0;

        public void HandleGetCountData(Dictionary<string, object> props)
        {
            items = (int)props["Item"];
            craft = (int)props["CraftingRecipe"];
            curr = (int)props["Currency"];
            itset = (int)props["ItemSet"];
            skill = (int)props["Skill"];
            ability = (int)props["Ability"];
            effects = (int)props["Effect"];
            building = (int)props["BuildingObject"];
            resource = (int)props["ResourceNode"];
            race = (int)props["Race"];
            global = (int)props["GlobalEvents"];
            if (props.ContainsKey("Quests"))
                quest = (int)props["Quests"];
            if (props.ContainsKey("WeaponProfile"))
                wProfile = (int)props["WeaponProfile"];
            if (props.ContainsKey("ItemAudioProfile"))
                iaProfile = (int)props["ItemAudioProfile"];
            if (props.ContainsKey("InteractiveObjects"))
                ioProfile = (int)props["InteractiveObjects"];
            if (props.ContainsKey("SlotsProfile"))
                sProfile = (int)props["SlotsProfile"];
            prefabCount = items + craft + curr + itset + skill + ability + effects + building + resource + race + global + quest + wProfile + iaProfile + ioProfile + sProfile;
            iconsCount = items + craft + curr + skill + ability + effects + building + resource;
            StartCoroutine(PrefabLoading());
        }

        private int prefabCount = 0;
        private int iconsCount = 0;

        private IEnumerator PrefabLoading()
        {
            WaitForSeconds delay = new WaitForSeconds(0.5f);
            int loadedprefab = 0;
            int loadedicons = 0;
            while (reloaded < toReload || loadedprefab < prefabCount || loadedicons < iconsCount)
            {
                loadedprefab = GetCountItems() + GetCountRecipe() + GetCountCurrencies() + GetCountItemSets() + GetCountSkills() + GetCountAbilities() + GetCountEffects() + GetCountBuildings()
                               + GetCountResourceNodes() + GetCountRaces() + GetCountGlobalEvents() + GetCountQuestNodes() + GetCountWeaponProfiles()+ GetCountItemAudioProfiles()
                               + GetCountInteractiveObjectProfiles() + GetCountSlotsProfiles();
                
                loadedicons = GetCountLoadedItemIcons() + GetCountLoadedRecipeIcons() + GetCountLoadedCurrencyIcons() + GetCountLoadedSkillIcons() + GetCountLoadedAbilityIcons()
                              + GetCountLoadedEffectIcons() + GetCountLoadedBuildingIcons() + GetCountLoadedResourceNodeIcons();

                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("AtavismPrefabManager: Loaded prefabs " + loadedprefab + " / " + prefabCount + " Loaded icons " + loadedicons + " / " + iconsCount + "\n" +
                                                  "items " + GetCountItems() + "/" + items + "\n" +
                                                  "craft " + GetCountRecipe() + "/" + craft + "\n" +
                                                  "curr " + GetCountCurrencies() + "/" + curr + "\n" +
                                                  "itemSet " + GetCountItemSets() + "/" + itset + "\n" +
                                                  "skills " + GetCountSkills() + "/" + skill + "\n" +
                                                  "ability " + GetCountAbilities() + "/" + ability + "\n" +
                                                  "effects " + GetCountEffects() + "/" + effects + "\n" +
                                                  "building " + GetCountBuildings() + "/" + building + "\n" +
                                                  "resource " + GetCountResourceNodes() + "/" + resource + "\n" +
                                                  "race " + GetCountRaces() + "/" + race + "\n" +
                                                  "global " + GetCountGlobalEvents() + "/" + global + "\n\n" +
                                                  "quests " + GetCountQuestNodes() + "/" + quest + "\n" +
                                                  "weaponProfile " + GetCountWeaponProfiles() + "/" + wProfile + "\n\n" +
                                                  "itemAudioProfile " + GetCountItemAudioProfiles() + "/" + iaProfile + "\n\n" +
                                                  "InteractiveObjects " + GetCountInteractiveObjectProfiles() + "/" + ioProfile + "\n\n" +
                                                  "SlotsProfile " + GetCountSlotsProfiles() + "/" + sProfile + "\n\n" +
                                                  "icons:\n" +
                                                  "items " + GetCountLoadedItemIcons() + "/" + items + "\n" +
                                                  "craft " + GetCountLoadedRecipeIcons() + "/" + craft + "\n" +
                                                  "curr " + GetCountLoadedCurrencyIcons() + "/" + curr + "\n" +
                                                  "skill " + GetCountLoadedSkillIcons() + "/" + skill + "\n" +
                                                  "ability " + GetCountLoadedAbilityIcons() + "/" + ability + "\n" +
                                                  "effects " + GetCountLoadedEffectIcons() + "/" + effects + "\n" +
                                                  "building " + GetCountLoadedBuildingIcons() + "/" + building + "\n" +
                                                  "resource " + GetCountLoadedResourceNodeIcons() + "/" + resource);
       /**/
                string[] args = new string[2];
                args[0] = (loadedicons + loadedprefab).ToString();
                args[1] = (prefabCount + iconsCount).ToString();
                AtavismEventSystem.DispatchEvent("LOADING_PREFAB_UPDATE", args);

                yield return delay;
            }
            Debug.Log("AtavismPrefabManager Loaded all data "+Time.time);
            string[] arg = new string[1];
            AtavismEventSystem.DispatchEvent("LOADING_PREFAB_HIDE", arg);
        }

        public void HandleReloaded(Dictionary<string, object> props)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("HandleReloaded");
            //  reloaded = 0;
            LoadStatPrefabData();
            LoadRaceData();
            LoadItemPrefabData();
            LoadItemSetPrefabData();
            LoadItemAudioProfilesPrefabData();
            LoadCurrencyPrefabData();
            LoadSkillsPrefabData();
            LoadAbilitiesPrefabData();
            LoadEffectsPrefabData();
            LoadResourceNodePrefabData();
            LoadBuldingObjectsPrefabData();
            LoadCraftingRecipePrefabData();
            LoadGlobalEventsData();
            LoadQuestPrefabData();
            LoadWeaponProfilesPrefabData();
            LoadInteractiveObjectProfilesData();
            LoadSlotsProfilesData();
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("HandleReloaded End");
        }

        public void HandleCombatSettings(Dictionary<string, object> props)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("HandleCombatSettings");
            prefabReloading = (bool)props["DevMode"];

            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("SETTINGS", args);

            if(props.ContainsKey("dodge"))
                Abilities.Instance.dodgeAbility = (int)props["dodge"];
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("HandleCombatSettings En prefabReloading="+prefabReloading);
        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("ITEM_RELOAD", this);
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SKILL_ICON_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("EFFECT_ICON_UPDATE", this);

            NetworkAPI.RemoveExtensionMessageHandler("ao.reloaded", HandleReloaded);
            
            
            
#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA_10_0 || UNITY_WSA
            if (!Directory.Exists(Application.persistentDataPath+"/" + Application.productName))
                Directory.CreateDirectory(Application.persistentDataPath+"/" + Application.productName);
            FileStream fileIcon = File.Create( Application.persistentDataPath+"/" + Application.productName + "/game_icon.data");
#else
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName);

            FileStream fileIcon = File.Create(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/game_icon.data");
/*#else
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName);

            FileStream fileIcon = File.Create(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/game.data");*/
#endif
            try
            {
              
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fileIcon, prefabIconData);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize icon Data. Reason: " + e.Message + "\n\n" + e.StackTrace);
            }
            finally
            {
                fileIcon.Close();
            }
            
#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA_10_0 || UNITY_WSA
            if (!Directory.Exists(Application.persistentDataPath+"/" + Application.productName))
                Directory.CreateDirectory(Application.persistentDataPath+"/" + Application.productName);
            FileStream file = File.Create( Application.persistentDataPath+"/" + Application.productName + "/game.data");
#else
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName);

            FileStream file = File.Create(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/game.data");
/*#else
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName);

            FileStream file = File.Create(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/game.data");*/
#endif
            try
            {
                List<int> todatete = new List<int>();
                foreach (BuildObjPrefabData item in prefabsdatadata.buildObjects.Values)
                {
                    if(item.iconPath==null)
                        todatete.Add(item.id);
                   
                }

                foreach (int id in todatete)
                    prefabsdatadata.buildObjects.Remove(id);
                todatete.Clear();
                foreach (ItemPrefabData item in prefabsdatadata.items.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.templateId);
                }

                foreach (int id in todatete)
                    prefabsdatadata.items.Remove(id);
                todatete.Clear();
                foreach (CraftingRecipePrefabData item in prefabsdatadata.craftRecipes.Values)
                {
                    if(item.iconPath==null)
                        todatete.Add(item.recipeID);

                }

                foreach (int id in todatete)
                    prefabsdatadata.craftRecipes.Remove(id);
                todatete.Clear();


                foreach (CurrencyPrefabData item in prefabsdatadata.currencies.Values)
                {
                    if(item.iconPath==null)
                           todatete.Add(item.id);
                }

                foreach (int id in todatete)
                    prefabsdatadata.currencies.Remove(id);
                todatete.Clear();

                foreach (AbilityPrefabData item in prefabsdatadata.abilities.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);
                }

                foreach (int id in todatete)
                    prefabsdatadata.abilities.Remove(id);
                todatete.Clear();

                foreach (SkillPrefabData item in prefabsdatadata.skills.Values)
                {
                    if(item.iconPath==null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.skills.Remove(id);
                todatete.Clear();
                foreach (EffectsPrefabData item in prefabsdatadata.effects.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.effects.Remove(id);
                
                todatete.Clear();
                foreach (GlobalEventData item in prefabsdatadata.globalEvents.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.globalEvents.Remove(id);

                todatete.Clear();
                foreach (RecourceNodeProfileData item in prefabsdatadata.recourceNode.Values)
                {
                    foreach (var s in item.settingList.Values)
                    {
                        if (s.selectedIconPath == null || s.cursorIconPath == null)
                        {
                            todatete.Add(item.id);
                        }
                    }

                }

                foreach (int id in todatete)
                    prefabsdatadata.recourceNode.Remove(id);

                
                todatete.Clear();
                foreach (RaceData item in prefabsdatadata.races.Values)
                {
                    if (item.iconPath == null)
                        todatete.Add(item.id);

                }

                foreach (int id in todatete)
                    prefabsdatadata.races.Remove(id);

                
                
                
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, prefabsdatadata);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize. Reason: " + e.Message + "\n\n" + e.StackTrace);
            }
            finally
            {
                file.Close();
            }
        }


        void ClientReady()
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Connect Prefab Server ClientReady");
        }

        public void OnEvent(AtavismEventData eData)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("AtavismPrefabManager: "+eData.eventType);
            if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                if (loadOnStartup)
                {
                    if (GetCountItems() - GetCountLoadedItemIcons() > 0)
                    {
                        LoadItemIcons(10);
                    }
                }
            }
            else if (eData.eventType == "Skill_ICON_UPDATE")
            {
                if (loadOnStartup)
                {
                    if (GetCountSkills() - GetCountLoadedSkillIcons() > 0)
                    {
                        LoadSkillIcons(10);
                    }
                }
            }
            else if (eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                if (loadOnStartup)
                {
                    if (GetCountCurrencies() - GetCountLoadedCurrencyIcons() > 0)
                    {
                        LoadCurrencyIcons(10);
                    }
                }
            }
            else if (eData.eventType == "EFFECT_ICON_UPDATE")
            {
                if (loadOnStartup)
                {
                    if (GetCountEffects() - GetCountLoadedEffectIcons() > 0)
                    {
                        LoadEffectsIcons();
                    }
                }
            }
            else if (eData.eventType == "ITEM_RELOAD")
            {
                if (loadOnStartup)
                {
                    if (GetCountItems() - GetCountLoadedItemIcons() > 0)
                    {
                        LoadItemIcons();
                    }

                    if (GetCountSkills() - GetCountLoadedSkillIcons() > 0)
                    {
                        LoadSkillIcons();
                    }

                    if (GetCountCurrencies() - GetCountLoadedCurrencyIcons() > 0)
                    {
                        LoadCurrencyIcons();
                    }

                    if (GetCountEffects() - GetCountLoadedEffectIcons() > 0)
                    {
                        LoadEffectsIcons();
                    }
                }
            }
        }


        private void sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (!arg0.name.Equals("Login") && !arg0.name.Equals(ClientAPI.Instance.characterSceneName))
            {

            }
            else if (arg0.name.Equals("Login"))
            {
                LoadStatPrefabData();
                LoadRaceData();
                LoadItemPrefabData();
                LoadItemSetPrefabData();
                LoadWeaponProfilesPrefabData();
                LoadItemAudioProfilesPrefabData();
                LoadCurrencyPrefabData();
                LoadSkillsPrefabData();
                LoadAbilitiesPrefabData();
                LoadEffectsPrefabData();
                LoadResourceNodePrefabData();
                LoadBuldingObjectsPrefabData();
                LoadCraftingRecipePrefabData();

                LoadGlobalEventsData();
                LoadInteractiveObjectProfilesData();
                LoadSlotsProfilesData();
            }
        }

        public string GetStat(int id)
        {
            return prefabsdatadata.statProfiles[id].name;
        }
        
        public int GetStatPrecision(string stat)
        {
            foreach (var e in prefabsdatadata.statProfiles)
            {
                if (e.Value.name.Equals(stat))
                {
                    return e.Value.precision;
                }
            }
            Debug.LogWarning("Not Found Stat definition for "+stat);
            return 1;
        }

        public StatProfileData GetStatProfile(string stat)
        {
            foreach (var e in prefabsdatadata.statProfiles)
            {
                if (e.Value.name.Equals(stat))
                {
                    return e.Value;
                }
            }

            return null;
        }
        public float GetStatValueWithPrecision(string stat, int value)
        {
            var def = GetStatProfile(stat);
            if (def != null)
            {
                return (float)System.Math.Round(value / def.division, def.precision);
            }
            Debug.LogWarning("No Stat Definition for "+stat);
            return value;
        }
        
        public void SaveStat(StatProfileData spd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save StatProfileData id=" + spd.id + " name=" + spd.name);
            if (prefabsdatadata.statProfiles.ContainsKey(spd.id))
            {
                prefabsdatadata.statProfiles.Remove(spd.id);
            }
            prefabsdatadata.statProfiles.Add(spd.id, spd);
        }
        
        public void DeleteStat(int id)
        {
            if (prefabsdatadata.statProfiles.ContainsKey(id))
            {
                prefabsdatadata.statProfiles.Remove(id);
            }
        }
        
        public void SaveCurrency(CurrencyPrefabData cpd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save CurrencyPrefabData id=" + cpd.id + " name=" + cpd.name);
            if (prefabsdatadata.currencies.ContainsKey(cpd.id))
            {
                prefabIconData.Remove(prefabsdatadata.currencies[cpd.id].iconPath);
                prefabsdatadata.currencies.Remove(cpd.id);
            }

            prefabsdatadata.currencies.Add(cpd.id, cpd);
        }

        public void SaveCurrencyIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveCurrencyIcon id=" + id);
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.currencies.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.currencies[id].icon = icon;
                    prefabsdatadata.currencies[id].iconData = iconData;
                }

                prefabsdatadata.currencies[id].iconloaded = true;
            }
        }

        public void DeleteCurrency(int id)
        {
            if (prefabsdatadata.currencies.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.currencies[id].iconPath);
                prefabsdatadata.currencies.Remove(id);
            }
        }

        public void SaveSkill(SkillPrefabData cpd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SkillPrefabData id=" + cpd.id + " name=" + cpd.skillname);
            if (prefabsdatadata.skills.ContainsKey(cpd.id))
            {
                prefabIconData.Remove(prefabsdatadata.skills[cpd.id].iconPath);
                prefabsdatadata.skills.Remove(cpd.id);
            }

            prefabsdatadata.skills.Add(cpd.id, cpd);
        }

        public void SaveSkillIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveSkillIcon id=" + id);
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.skills.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.skills[id].icon = icon;
                    prefabsdatadata.skills[id].iconData = iconData;
                    prefabsdatadata.skills[id].iconloaded = true;
                }
            }
        }

        public void DeleteSkill(int id)
        {
            if (prefabsdatadata.skills.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.skills[id].iconPath);
                prefabsdatadata.skills.Remove(id);
            }
        }

        public void SaveAbility(AbilityPrefabData cpd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save AbilityPrefabData id=" + cpd.id + " name=" + cpd.name);
            if (prefabsdatadata.abilities.ContainsKey(cpd.id))
            {
                prefabIconData.Remove(prefabsdatadata.abilities[cpd.id].iconPath);
                prefabsdatadata.abilities.Remove(cpd.id);
            }

            prefabsdatadata.abilities.Add(cpd.id, cpd);
        }

        public void SaveAbilityIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveAbilityIcon id=" + id);
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.abilities.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.abilities[id].icon = icon;
                    prefabsdatadata.abilities[id].iconData = iconData;
                }

                prefabsdatadata.abilities[id].iconloaded = true;
            }
        }

        public void DeleteAbility(int id)
        {
            if (prefabsdatadata.abilities.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.abilities[id].iconPath);
                prefabsdatadata.abilities.Remove(id);
            }
        }

        public void SaveEffects(EffectsPrefabData cpd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save EffectsPrefabData id=" + cpd.id + " name=" + cpd.name);
            if (prefabsdatadata.effects.ContainsKey(cpd.id))
            {
                prefabIconData.Remove(prefabsdatadata.effects[cpd.id].iconPath);
                prefabsdatadata.effects.Remove(cpd.id);
            }

            prefabsdatadata.effects.Add(cpd.id, cpd);
        }

        public void SaveEffectsIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveEffectsIcon id=" + id);
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.effects.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.effects[id].icon = icon;
                    prefabsdatadata.effects[id].iconData = iconData;
                }

                prefabsdatadata.effects[id].iconloaded = true;
            }
        }


        public void DeleteEffect(int id)
        {
            if (prefabsdatadata.effects.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.effects[id].iconPath);
                prefabsdatadata.effects.Remove(id);
            }
        }
        public void SaveWeaponProfile(WeaponProfileData wpd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save WeaponProfileData id=" + wpd.id + " name=" + wpd.name);
            if (prefabsdatadata.weaponProfiles.ContainsKey(wpd.id))
            {
                prefabsdatadata.weaponProfiles.Remove(wpd.id);
            }
            prefabsdatadata.weaponProfiles.Add(wpd.id, wpd);
        }

        public void SaveItemAudioProfile(ItemAudioProfileData iapd)
        {
            if(AtavismLogger.isLogDebug())
            AtavismLogger.LogDebugMessage("Save ItemAudioProfileData id=" + iapd.id + " name=" + iapd.name);
            if (prefabsdatadata.itemAudioProfiles.ContainsKey(iapd.id))
            {
                prefabsdatadata.itemAudioProfiles.Remove(iapd.id);
            }
            prefabsdatadata.itemAudioProfiles.Add(iapd.id, iapd);
        }

        public void SaveItem(ItemPrefabData ipd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save ItemPrefabData id=" + ipd.templateId + " name=" + ipd.name);
            if (prefabsdatadata.items.ContainsKey(ipd.templateId))
            {
                prefabIconData.Remove(prefabsdatadata.items[ipd.templateId].iconPath);
                prefabsdatadata.items.Remove(ipd.templateId);
                
            }

            prefabsdatadata.items.Add(ipd.templateId, ipd);
        }

        public void SaveIcon(Sprite icon, string iconData, string iconPath)
        {
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }
        }

        public Sprite GetIcon(string path)
        {
            // if(AtavismLogger.isLogDebug())
            //     AtavismLogger.LogDebugMessage("GetIcon "+path+" prefabIconData count "+prefabIconData.Count);
            if (path != null && prefabIconData.ContainsKey(path))
            {
                if (prefabIconData[path].icon == null && prefabIconData[path].iconData.Length > 0)
                {
                    Texture2D tex = new Texture2D(2, 2);
                    bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabIconData[path].iconData));
                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    prefabIconData[path].icon = sprite;
                }

                return prefabIconData[path].icon;
            }

            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("AtavismPrefabManager.GetIcon: not found icon " + path);
            return null;
        }

        public Texture2D GetIcon2d(string path)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("GetIcon "+path+" prefabIconData count "+prefabIconData.Count);
            if (path != null && prefabIconData.ContainsKey(path))
            {
                if (prefabIconData[path].icon2d == null && prefabIconData[path].iconData.Length > 0)
                {
                    Texture2D tex = new Texture2D(2, 2);
                    bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabIconData[path].iconData));
                    prefabIconData[path].icon2d = tex;
                }
                
                return prefabIconData[path].icon2d;
            }

            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("AtavismPrefabManager.GetIcon2d: not found icon " + path);
            return null;
        }

        public void SaveItemsIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveItemsIcon id=" + id);
            if (prefabsdatadata.items.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.items[id].icon = icon;
                    prefabsdatadata.items[id].iconData = iconData;
                }

                prefabsdatadata.items[id].iconloaded = true;
            }
        }

        public void DeleteItem(int id)
        {
            if (prefabsdatadata.items.ContainsKey(id)){
                prefabIconData.Remove(prefabsdatadata.items[id].iconPath);
                prefabsdatadata.items.Remove(id);
            }
        }

        public void DeleteWeaponProfile(int id)
        {
            if (prefabsdatadata.weaponProfiles.ContainsKey(id)){
                prefabsdatadata.weaponProfiles.Remove(id);
            }
        }

        public void DeleteItemAudioProfile(int id)
        {
            if (prefabsdatadata.itemAudioProfiles.ContainsKey(id)){
                prefabsdatadata.itemAudioProfiles.Remove(id);
            }
        }

        public void SaveItemSet(ItemSetPrefabData ipd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save ItemSetPrefabData id=" + ipd.Setid + " name=" + ipd.Name);
            if (prefabsdatadata.itemSets.ContainsKey(ipd.Setid))
                prefabsdatadata.itemSets.Remove(ipd.Setid);
            prefabsdatadata.itemSets.Add(ipd.Setid, ipd);

            foreach (var itemId in ipd.itemList)
            {
                if (prefabsdatadata.items.ContainsKey(itemId))
                {
                    prefabsdatadata.items[itemId].setId = ipd.Setid;
                }
            }
        }


        public void DeleteItemSet(int id)
        {
            if (prefabsdatadata.itemSets.ContainsKey(id))
            {
                foreach (var itemId in prefabsdatadata.itemSets[id].itemList)
                {
                    if (prefabsdatadata.items.ContainsKey(itemId))
                    {
                        prefabsdatadata.items[itemId].setId = 0;
                    }
                }

                prefabsdatadata.itemSets.Remove(id);
            }
        }

        public void SaveCrafringRecipe(CraftingRecipePrefabData ipd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveCrafringRecipe id=" + ipd.recipeID + " name=" + ipd.recipeName);
            if (prefabsdatadata.craftRecipes.ContainsKey(ipd.recipeID))
            {
                if(prefabIconData.ContainsKey(prefabsdatadata.craftRecipes[ipd.recipeID].iconPath))
                    prefabIconData.Remove(prefabsdatadata.craftRecipes[ipd.recipeID].iconPath);
                prefabsdatadata.craftRecipes.Remove(ipd.recipeID);
            }

            prefabsdatadata.craftRecipes.Add(ipd.recipeID, ipd);
        }

        public void SaveCrafringRecipeIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveCrafringRecipeIcon id=" + id);
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.craftRecipes.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.craftRecipes[id].icon = icon;
                    prefabsdatadata.craftRecipes[id].iconData = iconData;
                }

                prefabsdatadata.craftRecipes[id].iconloaded = true;
            }
        }

        public void DeleteCrafringRecipe(int id)
        {
            if (prefabsdatadata.craftRecipes.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.craftRecipes[id].iconPath);
                prefabsdatadata.craftRecipes.Remove(id);
            }
        }

        public void SaveBuildObject(BuildObjPrefabData ipd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save BuildObjPrefabData id=" + ipd.id + " name=" + ipd.buildObjectName);
            if (prefabsdatadata.buildObjects.ContainsKey(ipd.id))
            {
                prefabIconData.Remove(prefabsdatadata.buildObjects[ipd.id].iconPath);
                prefabsdatadata.buildObjects.Remove(ipd.id);
            }

            prefabsdatadata.buildObjects.Add(ipd.id, ipd);
        }

        public void SaveBuildObjectIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save SaveItemsIcon id=" + id);
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.buildObjects.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.buildObjects[id].icon = icon;
                    prefabsdatadata.buildObjects[id].iconData = iconData;
                }

                prefabsdatadata.buildObjects[id].iconloaded = true;
            }
        }

        public void DeleteBuildObject(int id)
        {
            if (prefabsdatadata.buildObjects.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.buildObjects[id].iconPath);
                prefabsdatadata.buildObjects.Remove(id);
            }
        }

        public void DeleteGlobalEvent(int id)
        {
            if (prefabsdatadata.globalEvents.ContainsKey(id))
            {
                prefabIconData.Remove(prefabsdatadata.globalEvents[id].iconPath);
                prefabsdatadata.globalEvents.Remove(id);
            }
        }

        public ItemPrefabData GetItemTemplateByID(int id)
        {
            if (prefabsdatadata.items.ContainsKey(id))
                return prefabsdatadata.items[id];
            return null;
        }
        
        public WeaponProfileData GetWeaponProfileByID(int id)
        {
            if (prefabsdatadata.weaponProfiles.ContainsKey(id))
                return prefabsdatadata.weaponProfiles[id];
            return null;
        }

        public ItemAudioProfileData GetItemAudioProfileByID(int id)
        {
            if (prefabsdatadata.itemAudioProfiles.ContainsKey(id))
                return prefabsdatadata.itemAudioProfiles[id];
            return null;
        }


        public Sprite GetItemIconByID(int id)
        {

            if (prefabsdatadata.items.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.items[id].iconPath);
                    if (icon == null && prefabsdatadata.items[id].iconPath.Length > 0)
                    {
                        if (!itemIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get  Item Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "ItemIcon");
                            itemIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (itemIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get  Item Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "ItemIcon");
                                itemIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.items[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.items[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.items[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.items[id].iconPath.Length > 0)
                        {
                            if (!itemIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get  Item Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "ItemIcon");
                                itemIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (itemIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get  Item Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "ItemIcon");
                                    itemIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetAbilityIconByID(int id)
        {
            if (prefabsdatadata.abilities.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    
                    Sprite icon = GetIcon(prefabsdatadata.abilities[id].iconPath);
                    if (icon == null && prefabsdatadata.abilities[id].iconPath.Length > 0)
                    {
                        Debug.LogError("GetAbilityIconByID "+id+" "+prefabsdatadata.abilities[id].iconPath);
                        if (!abilityIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get  Ability Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "AbilityIcon");
                            abilityIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (abilityIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get  Ability Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "AbilityIcon");
                                abilityIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.abilities[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.abilities[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.abilities[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.abilities[id].iconPath.Length > 0)
                        {
                            if (!abilityIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get  Ability Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "AbilityIcon");
                                abilityIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (abilityIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get  Ability Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "AbilityIcon");
                                    abilityIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetEffectIconByID(int id)
        {
            if (prefabsdatadata.effects.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.effects[id].iconPath);
                    if (icon == null && prefabsdatadata.effects[id].iconPath.Length > 0)
                    {
                        if (!effectsIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get Effect Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
                            effectsIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (effectsIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Effect Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
                                effectsIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.effects[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.effects[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.effects[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.effects[id].iconPath.Length > 0)
                        {
                            if (!effectsIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Effect Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
                                effectsIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (effectsIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get Effect Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
                                    effectsIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetSkillIconByID(int id)
        {
            if (prefabsdatadata.skills.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.skills[id].iconPath);
                    if (icon == null && prefabsdatadata.skills[id].iconPath.Length > 0)
                    {
                        if (!skillIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get Skill Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "SkillIcon");
                            skillIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (skillIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Skill Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "SkillIcon");
                                skillIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.skills[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.skills[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.skills[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.skills[id].iconPath.Length > 0)
                        {
                            if (!skillIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Skill Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "SkillIcon");
                                skillIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (skillIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get Skill Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "SkillIcon");
                                    skillIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetCraftingRecipeIconByID(int id)
        {
            if (prefabsdatadata.craftRecipes.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.craftRecipes[id].iconPath);
                    if (icon == null && prefabsdatadata.craftRecipes[id].iconPath.Length > 0)
                    {
                        if (!craftRecipesIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get Crafting Recipe Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CraftingRecipeIcon");
                            craftRecipesIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (craftRecipesIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Crafting Recipe Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CraftingRecipeIcon");
                                craftRecipesIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.craftRecipes[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.craftRecipes[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.craftRecipes[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.craftRecipes[id].iconPath.Length > 0)
                        {
                            if (!craftRecipesIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Crafting Recipe Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CraftingRecipeIcon");
                                craftRecipesIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (craftRecipesIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get Crafting Recipe Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CraftingRecipeIcon");
                                    craftRecipesIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetBuildingObjectIconByID(int id)
        {
            if (prefabsdatadata.buildObjects.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.buildObjects[id].iconPath);
                    if (icon == null && prefabsdatadata.buildObjects[id].iconPath.Length > 0)
                    {
                        if (!buildObjectsIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get Building Object Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "BuildingObjectIcon");
                            buildObjectsIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (buildObjectsIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Building Object Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "BuildingObjectIcon");
                                buildObjectsIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.buildObjects[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.buildObjects[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.buildObjects[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.buildObjects[id].iconPath.Length > 0)
                        {
                            if (!buildObjectsIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Building Object Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "BuildingObjectIcon");
                                buildObjectsIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (buildObjectsIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get Building Object Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "BuildingObjectIcon");
                                    buildObjectsIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetCurrencyIconByID(int id)
        {
            if (prefabsdatadata.currencies.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.currencies[id].iconPath);
                    if (icon == null && prefabsdatadata.currencies[id].iconPath.Length > 0)
                    {
                        if (!currencyIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get Currency Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CurrencyIcon");
                            currencyIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (currencyIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Currency Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CurrencyIcon");
                                currencyIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.currencies[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.currencies[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.currencies[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                        else if (prefabsdatadata.currencies[id].iconPath.Length > 0)
                        {
                            if (!currencyIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Currency Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CurrencyIcon");
                                currencyIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (currencyIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get Currency Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CurrencyIcon");
                                    currencyIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetRaceIconByID(int raceid)
        {
            if (prefabsdatadata.races.ContainsKey(raceid))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.races[raceid].iconPath);

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.races[raceid].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.races[raceid].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.races[raceid].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        public Sprite GetClassIconByID(int raceid, int classId)
        {
            if (prefabsdatadata.races.ContainsKey(raceid))
            {
                if (prefabsdatadata.races[raceid].classList.ContainsKey(classId))
                {
                    if (!useOldStoreIconsInDefinitions)
                    {
                        Sprite icon = GetIcon(prefabsdatadata.races[raceid].classList[classId].iconPath);

                        return icon;
                    }
                    else
                    {
                        Sprite icon = prefabsdatadata.races[raceid].classList[classId].icon;
                        if (icon == null)
                        {
                            if (prefabsdatadata.races[raceid].classList[classId].iconData.Length > 0)
                            {
                                Texture2D tex = new Texture2D(2, 2);
                                bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.races[raceid].classList[classId].iconData));
                                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                icon = sprite;
                            }
                        }

                        return icon;
                    }
                }
            }

            return null;
        }

        public Sprite GetGenderIconByID(int raceid, int classId, int genderId)
        {
            if (prefabsdatadata.races.ContainsKey(raceid))
            {
                if (prefabsdatadata.races[raceid].classList.ContainsKey(classId))
                {
                    if (prefabsdatadata.races[raceid].classList[classId].genderList.ContainsKey(genderId))
                    {
                        if (!useOldStoreIconsInDefinitions)
                        {
                            Sprite icon = GetIcon(prefabsdatadata.races[raceid].classList[classId].genderList[genderId].iconPath);

                            return icon;
                        }
                        else
                        {
                            Sprite icon = prefabsdatadata.races[raceid].classList[classId].genderList[genderId].icon;
                            if (icon == null)
                            {
                                if (prefabsdatadata.races[raceid].classList[classId].genderList[genderId].iconData.Length > 0)
                                {
                                    Texture2D tex = new Texture2D(2, 2);
                                    bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.races[raceid].classList[classId].genderList[genderId].iconData));
                                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                    icon = sprite;
                                }
                            }

                            return icon;
                        }
                    }
                }
            }

            return null;
        }

        public Sprite GetResourceNodeIconById(int profileId, int settingId)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("GetResourceNodeIconById icon "+profileId+" "+settingId);
            if (prefabsdatadata.recourceNode.ContainsKey(profileId))
            {
                if (prefabsdatadata.recourceNode[profileId].settingList.ContainsKey(settingId))
                {
                    if (!useOldStoreIconsInDefinitions)
                    {
                    Sprite icon = GetIcon(prefabsdatadata.recourceNode[profileId].settingList[settingId].selectedIconPath);
                    if(AtavismLogger.isLogDebug())
                        AtavismLogger.LogDebugMessage("recourceNode icon "+profileId+" "+settingId+" "+icon+" "+prefabsdatadata.recourceNode[profileId].settingList[settingId].selectedIconPath);
                    return icon;
                    }
                    else
                    {
                        Sprite icon = prefabsdatadata.recourceNode[profileId].settingList[settingId].selectedIcon;
                        if (icon == null)
                        {
                            if (prefabsdatadata.recourceNode[profileId].settingList[settingId].selectedIconData.Length > 0)
                            {
                                Texture2D tex = new Texture2D(2, 2);
                                bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.recourceNode[profileId].settingList[settingId].selectedIconData));
                                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                icon = sprite;
                            }
                        }
                        return icon;
                    }
                }

            }

            return null;
        }

        public Texture2D GetResourceNodeCursorIconById(int profileId, int settingId)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("GetResourceNodeCursorIconById icon " + profileId + " " + settingId + " setting ");
            if (prefabsdatadata.recourceNode.ContainsKey(profileId))
            {
                if (prefabsdatadata.recourceNode[profileId].settingList.ContainsKey(settingId))
                {
                    if (!useOldStoreIconsInDefinitions)
                    {
                        Texture2D icon = GetIcon2d(prefabsdatadata.recourceNode[profileId].settingList[settingId].cursorIconPath);
                        if(AtavismLogger.isLogDebug())
                            AtavismLogger.LogDebugMessage("GetResourceNodeCursorIconById icon "+profileId+" "+settingId+" "+icon+" path "+prefabsdatadata.recourceNode[profileId].settingList[settingId].cursorIconData);

                        if (icon != null)
                            return icon;
                    }
                    else
                    {
                        Texture2D icon = prefabsdatadata.recourceNode[profileId].settingList[settingId].cursorIcon;
                        if (icon == null)
                        {
                            if (prefabsdatadata.recourceNode[profileId].settingList[settingId].cursorIconData.Length > 0)
                            {
                                Texture2D tex = new Texture2D(2, 2);
                                bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.recourceNode[profileId].settingList[settingId].cursorIconData));
                                icon =  tex;
                            }
                        }

                        return icon;
                    }
                }
                else
                {
                    if(AtavismLogger.isLogDebug())
                        AtavismLogger.LogDebugMessage("GetResourceNodeCursorIconById icon " + profileId + " " + settingId + " no setting");
                }

            }
            else
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("GetResourceNodeCursorIconById icon " + profileId + " " + settingId + " no resource");
            }

            return null;
        }

        public Sprite GetGlobalEventIconByID(int id)
        {
            if (prefabsdatadata.globalEvents.ContainsKey(id))
            {
                if (!useOldStoreIconsInDefinitions)
                {
                    Sprite icon = GetIcon(prefabsdatadata.globalEvents[id].iconPath);
                    if (icon == null && prefabsdatadata.globalEvents[id].iconPath.Length > 0)
                    {
                        if (!globalEventsIconGet.ContainsKey(id))
                        {
                            Dictionary<string, object> ps = new Dictionary<string, object>();
                            ps.Add("objs", id + ";");
                            if(AtavismLogger.isLogDebug())
                                AtavismLogger.LogDebugMessage("Get Global Event Icon for id " + id);
                            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "GlobalEvents");
                            globalEventsIconGet.Add(id, Time.time);
                        }
                        else
                        {
                            if (globalEventsIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Global Event Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "GlobalEvents");
                                globalEventsIconGet[id] = Time.time;
                            }
                        }
                    }

                    return icon;
                }
                else
                {
                    Sprite icon = prefabsdatadata.globalEvents[id].icon;
                    if (icon == null)
                    {
                        if (prefabsdatadata.globalEvents[id].iconData.Length > 0)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            bool wyn = tex.LoadImage(System.Convert.FromBase64String(prefabsdatadata.globalEvents[id].iconData));
                            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                            icon = sprite;
                            prefabsdatadata.globalEvents[id].icon = sprite;
                        }
                        else if (prefabsdatadata.globalEvents[id].iconPath.Length > 0)
                        {
                            if (!globalEventsIconGet.ContainsKey(id))
                            {
                                Dictionary<string, object> ps = new Dictionary<string, object>();
                                ps.Add("objs", id + ";");
                                if(AtavismLogger.isLogDebug())
                                    AtavismLogger.LogDebugMessage("Get Global Event Icon for id " + id);
                                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "GlobalEvents");
                                globalEventsIconGet.Add(id, Time.time);
                            }
                            else
                            {
                                if (globalEventsIconGet[id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                {
                                    Dictionary<string, object> ps = new Dictionary<string, object>();
                                    ps.Add("objs", id + ";");
                                    if(AtavismLogger.isLogDebug())
                                        AtavismLogger.LogDebugMessage("Get Global Event Icon for id " + id);
                                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "GlobalEvents");
                                    globalEventsIconGet[id] = Time.time;
                                }
                            }
                        }
                    }

                    return icon;
                }
            }

            return null;
        }

        /**
         * Get Quest Definition by quest id
         */
        public QuestData GetQuestByID(int id)
        {
            if (prefabsdatadata.quests.ContainsKey(id))
                return prefabsdatadata.quests[id];
            return null;
        }

        /**
         * Save Quest data  
         */
        public void SaveQuest(QuestData qpd)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("Save QuestData id=" + qpd.id + " name=" + qpd.title);
            if (prefabsdatadata.quests.ContainsKey(qpd.id))
                prefabsdatadata.quests.Remove(qpd.id);
            prefabsdatadata.quests.Add(qpd.id, qpd);
        }

        /**
         * Delete Quest data for specific id 
         */
        public void DeleteQuest(int id)
        {
            if (prefabsdatadata.quests.ContainsKey(id))
                prefabsdatadata.quests.Remove(id);
        }

        /// <summary>
        /// Function to get count of the list with item definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedItemIcons()
        {
            int count = 0;
            foreach (var item in prefabsdatadata.items.Values)
            {
                if (GetIcon(item.iconPath) != null || item.iconPath.Length == 0 || item.iconloaded)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with item definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountItems()
        {
            return prefabsdatadata.items.Count;
        }
        
        /// <summary>
        /// Function to get count of the list with weapon profile definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountWeaponProfiles()
        {
            return prefabsdatadata.weaponProfiles.Count;
        }

        /// <summary>
        /// Function to get count of the list with item audio profile definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountItemAudioProfiles()
        {
            return prefabsdatadata.itemAudioProfiles.Count;
        }

        /// <summary>
        /// Function to get count of the list with interactive object profile definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountInteractiveObjectProfiles()
        {
            return prefabsdatadata.interactiveObjectProfiles.Count;
        }

        /// <summary>
        /// Function to get count of the list with slots profile definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountSlotsProfiles()
        {
            return prefabsdatadata.slotsProfiles.Count;
        }

        /// <summary>
        /// Function to get count of the list with crafting recipe definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedRecipeIcons()
        {
            int count = 0;
            foreach (var item in prefabsdatadata.craftRecipes.Values)
            {
                if (GetIcon(item.iconPath) != null || item.iconPath.Length == 0 || item.iconloaded)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with crafting recipe definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountRecipe()
        {
            return prefabsdatadata.craftRecipes.Count;
        }

        /// <summary>
        /// Function to get count of the list with skill definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedSkillIcons()
        {
            int count = 0;
            foreach (var skill in prefabsdatadata.skills.Values)
            {
                if (GetIcon(skill.iconPath) != null || skill.iconPath.Length == 0 || skill.iconloaded)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with skill definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountSkills()
        {
            return prefabsdatadata.skills.Count;
        }

        /// <summary>
        /// Function to get count of the list with currency definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedCurrencyIcons()
        {
            int count = 0;
            foreach (var currency in prefabsdatadata.currencies.Values)
            {
                if (GetIcon(currency.iconPath) != null || currency.iconPath.Length == 0 || currency.iconloaded)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with currency definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountCurrencies()
        {
            return prefabsdatadata.currencies.Count;
        }

        /// <summary>
        /// Function to get count of the list with ability definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedAbilityIcons()
        {
            int count = 0;
            foreach (var ability in prefabsdatadata.abilities.Values)
            {
                if (GetIcon(ability.iconPath) != null || ability.iconPath.Length == 0 || ability.iconloaded)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with ability definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountAbilities()
        {
            return prefabsdatadata.abilities.Count;
        }

        /// <summary>
        /// Function to get count of the list with effect definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedEffectIcons()
        {
            int count = 0;
            List<int> iconLack = new List<int>();
            foreach (var effect in prefabsdatadata.effects.Values)
            {
                if (GetIcon(effect.iconPath) != null || effect.iconPath.Length == 0 || effect.iconloaded)
                {
                    count++;
                }
                else if(effect.iconPath.Length>0)
                {
                    if (effectsIconGet.ContainsKey(effect.id))
                    {
                        if (effectsIconGet[effect.id] + 5f < Time.time)
                            iconLack.Add(effect.id);
                    }
                    else
                    {
                        iconLack.Add(effect.id);
                    }
                    if(AtavismLogger.isLogDebug())
                        AtavismLogger.LogDebugMessage("Effect Icon Not loaded for "+effect.name+":"+effect.id+" "+effect.iconPath);
                }
            }
            if (iconLack.Count > 0 && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                    effectsIconGet[id] = Time.time;

                }
                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
            }
            return count;
        }

        /// <summary>
        /// Function to get count of the list with effect definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountEffects()
        {
            return prefabsdatadata.effects.Count;
        }

        /// <summary>
        /// Function to get count of the list with building object definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedBuildingIcons()
        {
            int count = 0;
            foreach (var Building in prefabsdatadata.buildObjects.Values)
            {
                if (GetIcon(Building.iconPath) != null || Building.iconPath.Length == 0 || Building.iconloaded)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with building object definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountBuildings()
        {
            return prefabsdatadata.buildObjects.Count;
        }

        /// <summary>
        /// Function to get count of the list with resource node definitions where icons are defined
        /// </summary>
        /// <returns></returns>
        public int GetCountLoadedResourceNodeIcons()
        {
            int count = 0;
            foreach (var rn in prefabsdatadata.recourceNode.Values)
            {
                bool ok = true;
                foreach (var rnsl in rn.settingList.Values)
                {
                    if (rnsl.cursorIconPath.Length == 0 || rnsl.selectedIconPath.Length == 0)
                    {
                        ok = false;
                    }
                }

                if (ok)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with race definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountRaces()
        {
            int count = 0;
            foreach (var race in prefabsdatadata.races.Values)
            {
                count += race.classList.Count;
            }

            return count;
        }

        /// <summary>
        /// Function to get count of the list with item set definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountItemSets()
        {
            return prefabsdatadata.itemSets.Count;
        }

        /// <summary>
        /// Function to get count of the list with global event definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountGlobalEvents()
        {
            return prefabsdatadata.globalEvents.Count;
        }

        /// <summary>
        /// Function to get count of the list with resource node definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountResourceNodes()
        {
            return prefabsdatadata.recourceNode.Count;
        }

        /// <summary>
        /// Function to get count of the list with quest definitions
        /// </summary>
        /// <returns></returns>
        public int GetCountQuestNodes()
        {
            return prefabsdatadata.quests.Count;
        }

        /// <summary>
        /// Function to get list of the race definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, RaceData> GetRaceData()
        {
            return prefabsdatadata.races;
        }

        /// <summary>
        /// Function to get list of the stats definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, StatProfileData>.ValueCollection GetStatsData()
        {
            return prefabsdatadata.statProfiles.Values;
        }
        
        /// <summary>
        /// Function to get list of the item set definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, ItemSetPrefabData>.ValueCollection GetItemSetPrefabData()
        {
            return prefabsdatadata.itemSets.Values;
        }

        /// <summary>
        /// Function to get list of the item definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, ItemPrefabData>.ValueCollection GetItemPrefabData()
        {
            return prefabsdatadata.items.Values;
        }
        
        /// <summary>
        /// Function to get list of the weapon profile definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, WeaponProfileData>.ValueCollection GetWeaponProfilePrefabData()
        {
            if (prefabsdatadata.weaponProfiles == null)
                prefabsdatadata.weaponProfiles = new Dictionary<int, WeaponProfileData>();
            return prefabsdatadata.weaponProfiles.Values;
        }
        
        /// <summary>
        /// Function to get list of the item audio profile definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, ItemAudioProfileData>.ValueCollection GetItemAudioProfilePrefabData()
        {
            if (prefabsdatadata.itemAudioProfiles == null)
                prefabsdatadata.itemAudioProfiles = new Dictionary<int, ItemAudioProfileData>();
            return prefabsdatadata.itemAudioProfiles.Values;
        }

        /// <summary>
        /// Function to get list of the crafting recipe definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, CraftingRecipePrefabData>.ValueCollection GetCraftingRecipesPrefabData()
        {
            return prefabsdatadata.craftRecipes.Values;
        }

        /// <summary>
        /// Function to get list of the currency definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, CurrencyPrefabData>.ValueCollection GetCurrencyPrefabData()
        {
            return prefabsdatadata.currencies.Values;
        }

        /// <summary>
        /// Function to get list of the skill definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, SkillPrefabData>.ValueCollection GetSkillPrefabData()
        {
            return prefabsdatadata.skills.Values;
        }

        /// <summary>
        /// Function to get list of the ability definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, AbilityPrefabData>.ValueCollection GetAbilityPrefabData()
        {
            return prefabsdatadata.abilities.Values;
        }

        /// <summary>
        /// Function to get list of the effect definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, EffectsPrefabData>.ValueCollection GetEffectPrefabData()
        {
            return prefabsdatadata.effects.Values;
        }

        /// <summary>
        /// Function to get list of the building objects definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, BuildObjPrefabData>.ValueCollection GetBuildObjPrefabData()
        {
            return prefabsdatadata.buildObjects.Values;
        }

        /// <summary>
        /// Function to get list of the resource node definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, RecourceNodeProfileData>.ValueCollection GetResourceNodePrefabData()
        {
            return prefabsdatadata.recourceNode.Values;
        }

        /// <summary>
        /// Function to get list of the global event definitions 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, GlobalEventData>.ValueCollection GetGlobalEventPrefabData()
        {
            return prefabsdatadata.globalEvents.Values;
        }

        /// <summary>
        /// Function to get list of the race definitions
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, RaceData>.ValueCollection GetRacesPrefabData()
        {
            return prefabsdatadata.races.Values;
        }

        /// <summary>
        /// Function to get list of the quest definitions 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, QuestData>.ValueCollection GetQuestsPrefabData()
        {
            return prefabsdatadata.quests.Values;
        }

        public Dictionary<string, AtavismIconData> GetIconsPrefabData()
        {
            return prefabIconData;
        }

        /// <summary>
        /// Function to get list of the interactive object profile definitions 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, InteractiveObjectProfileData>.ValueCollection GetInteractiveObjectProfilePrefabData()
        {
            return prefabsdatadata.interactiveObjectProfiles.Values;
        }

        
        /// <summary>
        /// Function to get list of the slots profile definitions 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, SlotsProfileData>.ValueCollection GetSlotsProfilePrefabData()
        {
            return prefabsdatadata.slotsProfiles.Values;
        }

        
        /// <summary>
        /// Function to get AtavismInventoryItem definition for specific item name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public AtavismInventoryItem LoadItem(string Name)
        {
            foreach (ItemPrefabData ipd in prefabsdatadata.items.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("LoadItem by name "+ Name+" prefName="+ipd.name);
                if (ipd != null && ipd.name.Equals(Name))
                {
                    AtavismInventoryItem item = new AtavismInventoryItem();
                    item.templateId = ipd.templateId;
                    item.name = ipd.name;
                    //  item.icon = ipd.icon;
                    item.tooltip = ipd.tooltip;
                    item.itemType = ipd.itemType;
                    item.subType = ipd.subType;
                    item.slot = ipd.slot;
                    item.quality = ipd.quality;
                    item.currencyType = ipd.currencyType;
                    item.cost = ipd.cost;
                    item.binding = ipd.binding;
                    item.sellable = ipd.sellable;
                    item.DamageValue = ipd.damageValue;
                    item.DamageMaxValue = ipd.damageMaxValue;
                    item.SetId = ipd.setId;
                    item.EnchantId = ipd.enchantId;
                    item.WeaponSpeed = ipd.weaponSpeed;
                    item.StackLimit = ipd.stackLimit;
                    item.auctionHouse = ipd.auctionHouse;
                    item.Unique = ipd.unique;
                    item.gear_score = ipd.gear_score;
                    item.weaponProfile = ipd.weaponProfile;

                    item.sockettype = ipd.sockettype;
                    item.Durability = ipd.durability;
                    item.MaxDurability = ipd.durability;
                    item.weight = ipd.weight;
                    item.autoattack = ipd.autoattack;
                    item.ammoType = ipd.ammoType;
                    item.deathLoss = ipd.deathLoss;
                    item.parry = ipd.parry;
                    item.repairable = ipd.repairable;

                    item.itemEffectTypes = ipd.itemEffectTypes;
                    item.itemEffectNames = ipd.itemEffectNames;
                    item.itemEffectValues = ipd.itemEffectValues;

                    item.itemReqTypes = ipd.itemReqTypes;
                    item.itemReqNames = ipd.itemReqNames;
                    item.itemReqValues = ipd.itemReqValues;
                    return item;
                }
            }

            return null;
        }

        public AtavismInventoryItem LoadItem(string Name, AtavismInventoryItem item)
        {
            foreach (ItemPrefabData ipd in prefabsdatadata.items.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("LoadItem by name "+ Name+" prefName="+ipd.name);
                if (ipd != null && ipd.name.Equals(Name))
                {
                    item.templateId = ipd.templateId;
                    item.name = ipd.name;
                    //  item.icon = ipd.icon;
                    item.tooltip = ipd.tooltip;
                    item.itemType = ipd.itemType;
                    item.subType = ipd.subType;
                    item.slot = ipd.slot;
                    item.quality = ipd.quality;
                    item.currencyType = ipd.currencyType;
                    item.cost = ipd.cost;
                    item.binding = ipd.binding;
                    item.sellable = ipd.sellable;
                    item.DamageValue = ipd.damageValue;
                    item.DamageMaxValue = ipd.damageMaxValue;
                    item.SetId = ipd.setId;
                    item.EnchantId = ipd.enchantId;
                    item.WeaponSpeed = ipd.weaponSpeed;
                    item.StackLimit = ipd.stackLimit;
                    item.auctionHouse = ipd.auctionHouse;
                    item.Unique = ipd.unique;
                    item.gear_score = ipd.gear_score;
                    item.weaponProfile = ipd.weaponProfile;
                    item.sockettype = ipd.sockettype;
                    item.Durability = ipd.durability;
                    item.MaxDurability = ipd.durability;
                    item.weight = ipd.weight;
                    item.autoattack = ipd.autoattack;
                    item.ammoType = ipd.ammoType;
                    item.deathLoss = ipd.deathLoss;
                    item.parry = ipd.parry;
                    item.repairable = ipd.repairable;

                    item.itemEffectTypes = ipd.itemEffectTypes;
                    item.itemEffectNames = ipd.itemEffectNames;
                    item.itemEffectValues = ipd.itemEffectValues;

                    item.itemReqTypes = ipd.itemReqTypes;
                    item.itemReqNames = ipd.itemReqNames;
                    item.itemReqValues = ipd.itemReqValues;
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to get AtavismInventoryItem definition for specific item id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AtavismInventoryItem LoadItem(int id)
        {
            if (prefabsdatadata.items.ContainsKey(id))
            {
                AtavismInventoryItem item = new AtavismInventoryItem();

                ItemPrefabData ipd = prefabsdatadata.items[id];
                if (ipd != null)
                {

                    item.templateId = ipd.templateId;
                    item.name = ipd.name;
                    // item.icon = ipd.icon;
                    item.tooltip = ipd.tooltip;
                    item.itemType = ipd.itemType;
                    item.subType = ipd.subType;
                    item.slot = ipd.slot;
                    item.quality = ipd.quality;
                    item.currencyType = ipd.currencyType;
                    item.cost = ipd.cost;
                    item.binding = ipd.binding;
                    item.sellable = ipd.sellable;
                    item.DamageValue = ipd.damageValue;
                    item.DamageMaxValue = ipd.damageMaxValue;
                    item.SetId = ipd.setId;
                    item.EnchantId = ipd.enchantId;
                    item.WeaponSpeed = ipd.weaponSpeed;
                    item.StackLimit = ipd.stackLimit;
                    item.auctionHouse = ipd.auctionHouse;
                    item.Unique = ipd.unique;
                    item.gear_score = ipd.gear_score;
                    item.weaponProfile = ipd.weaponProfile;
                    item.Durability = ipd.durability;
                    item.MaxDurability = ipd.durability;
                    item.sockettype = ipd.sockettype;
                    item.weight = ipd.weight;
                    item.autoattack = ipd.autoattack;
                    item.ammoType = ipd.ammoType;
                    item.deathLoss = ipd.deathLoss;
                    item.parry = ipd.parry;
                    item.repairable = ipd.repairable;

                    item.itemEffectTypes = ipd.itemEffectTypes;
                    item.itemEffectNames = ipd.itemEffectNames;
                    item.itemEffectValues = ipd.itemEffectValues;

                    item.itemReqTypes = ipd.itemReqTypes;
                    item.itemReqNames = ipd.itemReqNames;
                    item.itemReqValues = ipd.itemReqValues;

                }
                else
                {
                    AtavismLogger.LogError("Item definition id=" + id + " is null");
                    return null;
                }

                return item;
            }
            else
            {
                if (id > 0)
                {

                    AtavismLogger.LogError("Storage doesn't contain item definition id=" + id);
                    //  LoadItemPrefabData();
                }

            }

            return null;
        }

        public AtavismInventoryItem LoadItem(AtavismInventoryItem item)
        {
            if(AtavismLogger.isLogDebug())
                AtavismLogger.LogDebugMessage("LoadItem  "+item);
            if (item != null)
                if (prefabsdatadata.items.ContainsKey(item.templateId))
                {
                    ItemPrefabData ipd = prefabsdatadata.items[item.templateId];
                    if (ipd != null)
                    {

                        item.templateId = ipd.templateId;
                        item.name = ipd.name;
                        //item.icon = ipd.icon;
                        item.tooltip = ipd.tooltip;
                        item.itemType = ipd.itemType;
                        item.subType = ipd.subType;
                        item.slot = ipd.slot;
                        item.quality = ipd.quality;
                        item.currencyType = ipd.currencyType;
                        item.cost = ipd.cost;
                        item.binding = ipd.binding;
                        item.sellable = ipd.sellable;
                        item.DamageValue = ipd.damageValue;
                        item.DamageMaxValue = ipd.damageMaxValue;
                        item.SetId = ipd.setId;
                        item.EnchantId = ipd.enchantId;
                        item.WeaponSpeed = ipd.weaponSpeed;
                        item.StackLimit = ipd.stackLimit;
                        item.auctionHouse = ipd.auctionHouse;
                        item.Unique = ipd.unique;
                        item.gear_score = ipd.gear_score;
                        item.weaponProfile = ipd.weaponProfile;
                        // item.Durability = ipd.durability;
                        item.MaxDurability = ipd.durability;
                        item.sockettype = ipd.sockettype;
                        item.weight = ipd.weight;
                        item.autoattack = ipd.autoattack;
                        item.ammoType = ipd.ammoType;
                        item.deathLoss = ipd.deathLoss;
                        item.parry = ipd.parry;
                        item.repairable = ipd.repairable;

                        item.itemEffectTypes = ipd.itemEffectTypes;
                        item.itemEffectNames = ipd.itemEffectNames;
                        item.itemEffectValues = ipd.itemEffectValues;

                        item.itemReqTypes = ipd.itemReqTypes;
                        item.itemReqNames = ipd.itemReqNames;
                        item.itemReqValues = ipd.itemReqValues;

                    }
                    else
                    {
                        AtavismLogger.LogError("Item definition id=" + item.templateId + " is null");
                        return null;
                    }

                    return item;
                }

            return null;
        }

        /// <summary>
        /// Function to request item icons with limit from prefab server 
        /// </summary>
        /// <param name="limit"></param>
        public void LoadItemIcons(int limit = 5)
        {
            List<int> iconLack = new List<int>();
            foreach (var item in prefabsdatadata.items.Values)
            {
                if (item.iconPath.Length > 0) 
                {
                    Sprite icon = GetIcon(item.iconPath);
                    if (icon == null )
                    {
                        if (itemIconGet.ContainsKey(item.templateId))
                        {
                            if (itemIconGet[item.templateId] + 2f < Time.time &&
                                AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                                iconLack.Add(item.templateId);
                        }
                        else
                        {
                            iconLack.Add(item.templateId);
                        }
                    }
                }

                if (iconLack.Count >= limit)
                    break;
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                    itemIconGet[id] = Time.time;

                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "ItemIcon");
            }
        }

        /// <summary>
        /// Function to request skill icons with limit from prefab server
        /// </summary>
        /// <param name="limit"></param>
        public void LoadSkillIcons(int limit = 5)
        {
            List<int> iconLack = new List<int>();
            foreach (var skill in prefabsdatadata.skills.Values)
            {
                if (skill.iconPath.Length > 0 && GetIcon(skill.iconPath) == null)
                {
                    if (skillIconGet.ContainsKey(skill.id))
                    {
                        if (skillIconGet[skill.id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            iconLack.Add(skill.id);
                    }
                    else
                    {
                        iconLack.Add(skill.id);
                    }
                }

                if (iconLack.Count >= limit)
                    break;
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                    skillIconGet[id] = Time.time;

                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "SkillIcon");
            }
        }

        /// <summary>
        /// Function to request currency icons with limit from prefab server   
        /// </summary>
        /// <param name="limit"></param>
        public void LoadCurrencyIcons(int limit = 5)
        {
            List<int> iconLack = new List<int>();
            foreach (var currency in prefabsdatadata.currencies.Values)
            {
                if (currency.iconPath.Length > 0 && GetIcon(currency.iconPath) == null)
                {
                    if (currencyIconGet.ContainsKey(currency.id))
                    {
                        if (currencyIconGet[currency.id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            iconLack.Add(currency.id);
                    }
                    else
                    {
                        iconLack.Add(currency.id);
                    }
                }

                if (iconLack.Count >= limit)
                    break;
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                    currencyIconGet[id] = Time.time;

                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CurrencyIcon");
            }
        }

        /// <summary>
        /// Function to request effect icons with limit from prefab server
        /// </summary>
        /// <param name="limit"></param>
        public void LoadEffectsIcons(int limit = 5)
        {
            List<int> iconLack = new List<int>();
            foreach (var eff in prefabsdatadata.effects.Values)
            {
                if (eff.iconPath.Length > 0 && GetIcon(eff.iconPath) == null)
                {
                    if (effectsIconGet.ContainsKey(eff.id))
                    {
                        if (effectsIconGet[eff.id] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                            iconLack.Add(eff.id);
                    }
                    else
                    {
                        iconLack.Add(eff.id);
                    }
                }

                if (iconLack.Count >= limit)
                    break;
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                    effectsIconGet[id] = Time.time;

                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "EffectIcon");
            }
        }

        /// <summary>
        /// Function to get AtavismInventoryItemSet definition for specific item set id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AtavismInventoryItemSet LoadItemSet(int id)
        {
            AtavismInventoryItemSet aiis = new AtavismInventoryItemSet();
            if (prefabsdatadata.itemSets.ContainsKey(id))
            {
                ItemSetPrefabData ipd = prefabsdatadata.itemSets[id];
                if (ipd != null)
                {
                    aiis.Setid = ipd.Setid;
                    aiis.Name = ipd.Name;
                    aiis.itemList = ipd.itemList;
                    aiis.levelList = ipd.levelList;
                }
                else
                {
                    AtavismLogger.LogError("Item set definiction id=" + id + " is null");
                    return null;
                }
            }
            else
            {
                if (id > 0)
                {
                    AtavismLogger.LogError("Storage doesn't contain item set definition id=" + id);
                    // LoadItemSetPrefabData();
                }

                return null;
            }

            return aiis;
        }

        /// <summary>
        /// Function to get all items set definitions
        /// </summary>
        /// <returns></returns>
        public List<AtavismInventoryItemSet> LoadAllItemSet()
        {
            List<AtavismInventoryItemSet> list = new List<AtavismInventoryItemSet>();
            foreach (ItemSetPrefabData ipd in prefabsdatadata.itemSets.Values)
            {
                AtavismInventoryItemSet aiis = new AtavismInventoryItemSet();
                if (ipd != null)
                {
                    aiis.Setid = ipd.Setid;
                    aiis.Name = ipd.Name;
                    aiis.itemList = ipd.itemList;
                    aiis.levelList = ipd.levelList;
                    list.Add(aiis);
                }
            }

            return list;
        }

        /// <summary>
        /// Function to get AtavismBuildObjectTemplate definition for specific building object id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AtavismBuildObjectTemplate LoadBuildObject(int id)
        {
            AtavismBuildObjectTemplate aiis = new AtavismBuildObjectTemplate();
            if (prefabsdatadata.buildObjects.ContainsKey(id))
            {
                BuildObjPrefabData ipd = prefabsdatadata.buildObjects[id];
                if (ipd != null)
                {
                    aiis.id = ipd.id;
                    aiis.buildObjectName = ipd.buildObjectName;
                    // aiis.icon = ipd.icon;
                    aiis.category = ipd.category;
                    aiis.objectCategory = ipd.objectCategory;
                    aiis.skill = ipd.skill;
                    aiis.skillLevelReq = ipd.skillLevelReq;
                    aiis.distanceReq = ipd.distanceReq;
                    aiis.buildTaskReqPlayer = ipd.buildTaskReqPlayer;
                    aiis.validClaimTypes = ipd.validClaimTypes;
                    aiis.onlyAvailableFromItem = ipd.onlyAvailableFromItem;
                    aiis.reqWeapon = ipd.reqWeapon;
                    aiis.gameObject = ipd.gameObject;
                    aiis.itemsReq = ipd.itemsReq;
                    aiis.itemsReqCount = ipd.itemsReqCount;
                    aiis.upgradeItemsReq = ipd.upgradeItemsReq;
                    aiis.buildTimeReq = ipd.buildTimeReq;
                }
                else
                {
                    AtavismLogger.LogError("BuildObject definiction id=" + id + " is null");
                    return null;
                }
            }
            else
            {
                if (id > 0)
                {

                    AtavismLogger.LogError("Storage doesn't contain BuildObject definition id=" + id);
                    //   LoadBuldingObjectsPrefabData();
                }

                return null;
            }

            return aiis;
        }

        /// <summary>
        /// Function to get all building object definitions
        /// </summary>
        /// <returns></returns>
        public List<AtavismBuildObjectTemplate> LoadAllBuildObject()
        {
            List<AtavismBuildObjectTemplate> list = new List<AtavismBuildObjectTemplate>();
            List<int> iconLack = new List<int>();
            foreach (BuildObjPrefabData ipd in prefabsdatadata.buildObjects.Values)
            {
                AtavismBuildObjectTemplate aiis = new AtavismBuildObjectTemplate();
                //go.AddComponent<AtavismBuildObjectTemplate>();
                if (ipd != null)
                {

                    aiis.id = ipd.id;
                    aiis.buildObjectName = ipd.buildObjectName;
                    // aiis.icon = ipd.icon;
                    aiis.category = ipd.category;
                    aiis.objectCategory = ipd.objectCategory;
                    aiis.skill = ipd.skill;
                    aiis.skillLevelReq = ipd.skillLevelReq;
                    aiis.distanceReq = ipd.distanceReq;
                    aiis.buildTaskReqPlayer = ipd.buildTaskReqPlayer;
                    aiis.validClaimTypes = ipd.validClaimTypes;
                    aiis.onlyAvailableFromItem = ipd.onlyAvailableFromItem;
                    aiis.reqWeapon = ipd.reqWeapon;
                    aiis.gameObject = ipd.gameObject;
                    aiis.itemsReq = ipd.itemsReq;
                    aiis.itemsReqCount = ipd.itemsReqCount;
                    aiis.upgradeItemsReq = ipd.upgradeItemsReq;
                    aiis.buildTimeReq = ipd.buildTimeReq;
                    list.Add(aiis);
                    Sprite icon = GetIcon(ipd.iconPath);
                    if (icon == null && ipd.iconPath.Length > 0)                    
                    {
                        iconLack.Add(aiis.id);
                    }
                }
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "BuildingObjectIcon");

            }

            return list;
        }

        /// <summary>
        /// Function to get AtavismCraftingRecipe definition for specific crafting recipe id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AtavismCraftingRecipe LoadCraftingRecipe(int id)
        {
            AtavismCraftingRecipe aiis = new AtavismCraftingRecipe();
            if (prefabsdatadata.craftRecipes.ContainsKey(id))
            {
                CraftingRecipePrefabData ipd = prefabsdatadata.craftRecipes[id];
                if (ipd != null)
                {
                    aiis.recipeID = ipd.recipeID;
                    aiis.recipeName = ipd.recipeName;
                    // aiis.icon = ipd.icon;
                    aiis.skillID = ipd.skillID;
                    aiis.skillLevelReq = ipd.skillLevelReq;
                    aiis.stationReq = ipd.stationReq;
                    aiis.creationTime = ipd.creationTime;

                    aiis.createsItems = ipd.createsItems;
                    aiis.createsItemsCounts = ipd.createsItemsCounts;
                    aiis.createsItems2 = ipd.createsItems2;
                    aiis.createsItemsCounts2 = ipd.createsItemsCounts2;
                    aiis.createsItems3 = ipd.createsItems3;
                    aiis.createsItemsCounts3 = ipd.createsItemsCounts3;
                    aiis.createsItems4 = ipd.createsItems4;
                    aiis.createsItemsCounts4 = ipd.createsItemsCounts4;
                    aiis.itemsReq = ipd.itemsReq;
                    aiis.itemsReqCounts = ipd.itemsReqCounts;
                }
                else
                {
                    AtavismLogger.LogError("CraftingRecipe definiction id=" + id + " is null");
                    return null;
                }
            }
            else
            {
                if (id > 0)
                {
                    AtavismLogger.LogError("Storage doesn't contain CraftingRecipe definition id=" + id);
                    //  LoadCraftingRecipePrefabData();
                }

                return null;
            }

            return aiis;
        }

        /// <summary>
        /// Function to get all Crafting recipe definitions
        /// </summary>
        /// <returns></returns>
        public List<AtavismCraftingRecipe> LoadAllCraftingRecipe()
        {
            List<AtavismCraftingRecipe> list = new List<AtavismCraftingRecipe>();
            List<int> iconLack = new List<int>();
            foreach (CraftingRecipePrefabData ipd in prefabsdatadata.craftRecipes.Values)
            {
                AtavismCraftingRecipe aiis = new AtavismCraftingRecipe();
                if (ipd != null)
                {
                    aiis.recipeID = ipd.recipeID;
                    aiis.recipeName = ipd.recipeName;
                    // aiis.icon = ipd.icon;
                    aiis.skillID = ipd.skillID;
                    aiis.skillLevelReq = ipd.skillLevelReq;
                    aiis.stationReq = ipd.stationReq;
                    aiis.creationTime = ipd.creationTime;

                    aiis.createsItems = ipd.createsItems;
                    aiis.createsItemsCounts = ipd.createsItemsCounts;
                    aiis.createsItems2 = ipd.createsItems2;
                    aiis.createsItemsCounts2 = ipd.createsItemsCounts2;
                    aiis.createsItems3 = ipd.createsItems3;
                    aiis.createsItemsCounts3 = ipd.createsItemsCounts3;
                    aiis.createsItems4 = ipd.createsItems4;
                    aiis.createsItemsCounts4 = ipd.createsItemsCounts4;
                    aiis.itemsReq = ipd.itemsReq;
                    aiis.itemsReqCounts = ipd.itemsReqCounts;
                    list.Add(aiis);
                    Sprite icon = GetIcon(ipd.iconPath);
                    if (icon == null && ipd.iconPath.Length > 0)                    
                    {
                        iconLack.Add(aiis.recipeID);
                    }
                }
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CraftingRecipeIcon");
            }

            return list;
        }


        /// <summary>
        /// Function to get Currency definition for specific currency id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Currency LoadCurrency(int id)
        {
            if (prefabsdatadata.currencies.ContainsKey(id))
            {
                Currency aiis = new Currency();

                CurrencyPrefabData ipd = prefabsdatadata.currencies[id];
                if (ipd != null)
                {
                    aiis.id = ipd.id;
                    aiis.name = ipd.name;
                    //  aiis.icon = ipd.icon;
                    aiis.convertsTo = ipd.convertsTo;
                    aiis.conversionAmountReq = ipd.conversionAmountReq;
                    //  aiis.description = ipd.description;
                    aiis.max = ipd.max;
                    aiis.position = ipd.position;
                    //  aiis.group = ipd.;
                    aiis.group = ipd.group;
                }
                else
                {
                    AtavismLogger.LogError("Currency definiction id=" + id + " is null");
                    return null;
                }

                return aiis;
            }
            else
            {
                if (id > 0)
                {

                    AtavismLogger.LogError("Storage doesn't contain Currency definition id=" + id);
                    //    LoadCurrencyPrefabData();
                }

            }

            return null;
        }

        /// <summary>
        /// Function to get all currency definitions
        /// </summary>
        /// <returns></returns>
        public List<Currency> LoadAllCurrency()
        {
            List<Currency> list = new List<Currency>();
            List<int> iconLack = new List<int>();
            foreach (CurrencyPrefabData ipd in prefabsdatadata.currencies.Values)
            {
                Currency aiis = new Currency();
                if (ipd != null)
                {
                    aiis.id = ipd.id;
                    aiis.name = ipd.name;
                    //  aiis.icon = ipd.icon;
                    aiis.convertsTo = ipd.convertsTo;
                    aiis.conversionAmountReq = ipd.conversionAmountReq;
                    //  aiis.description = ipd.description;
                    aiis.max = ipd.max;
                    aiis.position = ipd.position;
                    //  aiis.group = ipd.;
                    aiis.group = ipd.group;
                    list.Add(aiis);
                    Sprite icon = GetIcon(ipd.iconPath);
                    if (icon == null && ipd.iconPath.Length > 0)                    
                    {
                        iconLack.Add(aiis.id);
                    }
                }
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "CurrencyIcon");
            }

            return list;
        }


        /// <summary>
        /// Function to get AtavismEffect definition for specific effect id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AtavismEffect LoadEffect(int id)
        {

            if (prefabsdatadata.effects.ContainsKey(id))
            {
                AtavismEffect item = new AtavismEffect();
                EffectsPrefabData ipd = prefabsdatadata.effects[id];
                if (ipd != null)
                {
                    item.id = ipd.id;
                    item.name = ipd.name;
                    //  item.icon = ipd.icon;
                    item.tooltip = ipd.tooltip;
                    item.isBuff = ipd.isBuff;
                    item.show = ipd.show;
                    item.stackLimit = ipd.stackLimit;
                    item.stackTime = ipd.stackTime;
                    item.allowMultiple = ipd.allowMultiple;
                }
                else
                {
                    AtavismLogger.LogError("effect definiction id=" + id + " is null");
                    return null;
                }

                return item;
            }
            else
            {
                if (id > 0)
                {

                    AtavismLogger.LogError("Storage doesn't contain effect definition id=" + id);
                    //  LoadEffectsPrefabData();
                }

            }

            return null;
        }

        /// <summary>
        /// Function to get effect prefab definition for specific effect id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EffectsPrefabData GetEffectPrefab(int id)
        {
            return prefabsdatadata.effects[id];
        }

        /// <summary>
        /// Function to get ability prefab definition for specific ability id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AbilityPrefabData GetAbilityPrefab(int id)
        {
            return prefabsdatadata.abilities[id];
        }

        /// <summary>
        /// Function to get AtavismAbility definition for specific ability id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AtavismAbility LoadAbility(int id)
        {

            if (prefabsdatadata.abilities.ContainsKey(id))
            {
                AtavismAbility aa = new AtavismAbility();
                AbilityPrefabData ipd = prefabsdatadata.abilities[id];
                if (ipd != null)
                {
                    aa.id = ipd.id;
                    aa.name = ipd.name;
                    // aa.icon = ipd.icon;
                    aa.tooltip = ipd.tooltip;
                    aa.cost = ipd.cost;
                    aa.costPercentage = ipd.costPercentage;
                    aa.costProperty = ipd.costProperty;
                    aa.pulseCost = ipd.pulseCost;
                    aa.pulseCostPercentage = ipd.pulseCostPercentage;
                    aa.pulseCostProperty = ipd.pulseCostProperty;
                    aa.globalcd = ipd.globalcd;
                    aa.weaponcd = ipd.weaponcd;
                    aa.cooldownType = ipd.cooldownType;
                    aa.cooldownLength = ipd.cooldownLength;
                    aa.reagentReq = ipd.reagentReq;
                    aa.weaponReq = ipd.weaponReq;
                    aa.distance = ipd.distance;
                    aa.castingInRun = ipd.castingInRun;
                    aa.targetType = ipd.targetType;
                    aa.targetSubType = ipd.targetSubType;
                    aa.castTime = ipd.castTime;
                    aa.speed = ipd.speed;
                    aa.passive = ipd.passive;
                    aa.toggle = ipd.toggle;
                    aa.aoeType = ipd.aoeType;
                    aa.aoeRadius = ipd.aoeRadius;
                    aa.minRange = ipd.minRange;
                    aa.maxRange = ipd.maxRange;
                }
                else
                {
                    AtavismLogger.LogError("ability definiction id=" + id + " is null");
                    return null;
                }

                return aa;

            }
            else
            {
                if (id > 0)
                {

                    AtavismLogger.LogError("Storage doesn't contain ability definition id=" + id);
                    //  LoadAbilitiesPrefabData();
                }

            }

            return null;
        }

        /// <summary>
        /// Function to get Ability from ability definition
        /// </summary>
        /// <param name="aa"></param>
        /// <returns></returns>
        public AtavismAbility LoadAbility(AtavismAbility aa)
        {
            if (aa != null)
                if (prefabsdatadata.abilities.ContainsKey(aa.id))
                {
                    AbilityPrefabData ipd = prefabsdatadata.abilities[aa.id];
                    if (ipd != null)
                    {
                        aa.id = ipd.id;
                        aa.name = ipd.name;
                        // aa.icon = ipd.icon;
                        aa.tooltip = ipd.tooltip;
                        aa.cost = ipd.cost;
                        aa.costPercentage = ipd.costPercentage;
                        aa.costProperty = ipd.costProperty;
                        aa.pulseCost = ipd.pulseCost;
                        aa.pulseCostPercentage = ipd.pulseCostPercentage;
                        aa.pulseCostProperty = ipd.pulseCostProperty;
                        aa.globalcd = ipd.globalcd;
                        aa.weaponcd = ipd.weaponcd;
                        aa.cooldownType = ipd.cooldownType;
                        aa.cooldownLength = ipd.cooldownLength;
                        aa.reagentReq = ipd.reagentReq;
                        aa.weaponReq = ipd.weaponReq;
                        aa.distance = ipd.distance;
                        aa.castingInRun = ipd.castingInRun;
                        aa.targetType = ipd.targetType;
                        aa.targetSubType = ipd.targetSubType;
                        aa.castTime = ipd.castTime;
                        aa.speed = ipd.speed;
                        aa.passive = ipd.passive;
                        aa.toggle = ipd.toggle;
                        aa.aoeType = ipd.aoeType;
                        aa.aoeRadius = ipd.aoeRadius;
                        aa.minRange = ipd.minRange;
                        aa.maxRange = ipd.maxRange;
                    }
                    else
                    {
                        AtavismLogger.LogError("ability definiction id=" + aa.id + " is null");
                        return null;
                    }

                    return aa;

                }

            return null;
        }

        /// <summary>
        /// Function to get all ability from definition
        /// </summary>
        /// <returns></returns>
        public List<AtavismAbility> LoadAllAbilities()
        {
            List<AtavismAbility> list = new List<AtavismAbility>();
            List<int> iconLack = new List<int>();
            foreach (AbilityPrefabData ipd in prefabsdatadata.abilities.Values)
            {
                AtavismAbility aa = new AtavismAbility();
                if (ipd != null)
                {
                    aa.id = ipd.id;
                    aa.name = ipd.name;
                    // aa.icon = ipd.icon;
                    aa.tooltip = ipd.tooltip;
                    aa.cost = ipd.cost;
                    aa.costPercentage = ipd.costPercentage;
                    aa.costProperty = ipd.costProperty;
                    aa.pulseCost = ipd.pulseCost;
                    aa.pulseCostPercentage = ipd.pulseCostPercentage;
                    aa.pulseCostProperty = ipd.pulseCostProperty;
                    aa.globalcd = ipd.globalcd;
                    aa.weaponcd = ipd.weaponcd;
                    aa.cooldownType = ipd.cooldownType;
                    aa.cooldownLength = ipd.cooldownLength;
                    aa.reagentReq = ipd.reagentReq;
                    aa.weaponReq = ipd.weaponReq;
                    aa.distance = ipd.distance;
                    aa.castingInRun = ipd.castingInRun;
                    aa.targetType = ipd.targetType;
                    aa.targetSubType = ipd.targetSubType;
                    aa.castTime = ipd.castTime;
                    aa.speed = ipd.speed;
                    aa.passive = ipd.passive;
                    aa.toggle = ipd.toggle;
                    aa.aoeType = ipd.aoeType;
                    aa.aoeRadius = ipd.aoeRadius;
                    aa.minRange = ipd.minRange;
                    aa.maxRange = ipd.maxRange;
                    list.Add(aa);
                    Sprite icon = GetIcon(ipd.iconPath);
                    if (icon == null && ipd.iconPath.Length > 0)                    
                    {
                        iconLack.Add(aa.id);
                    }
                }
            }

            if (iconLack.Count > 0)
            {

                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "AbilityIcon");
            }

            return list;
        }

        /// <summary>
        /// Function to get skill definition
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Skill LoadSkill(int id)
        {
            Skill aa = new Skill();
            if (prefabsdatadata.skills.ContainsKey(id))
            {
                SkillPrefabData ipd = prefabsdatadata.skills[id];
                if (ipd != null)
                {
                    aa.id = ipd.id;
                    aa.skillname = ipd.skillname;
                    // aa.icon = ipd.icon;
                    aa.mainAspect = ipd.mainAspect;
                    aa.oppositeAspect = ipd.oppositeAspect;
                    aa.mainAspectOnly = ipd.mainAspectOnly;
                    aa.parentSkill = ipd.parentSkill;
                    aa.parentSkillLevelReq = ipd.parentSkillLevelReq;
                    aa.playerLevelReq = ipd.playerLevelReq;
                    aa.pcost = ipd.pcost;
                    aa.talent = ipd.talent;
                    aa.abilities = ipd.abilities;
                    aa.abilityLevelReqs = ipd.abilityLevelReqs;
                    aa.type = ipd.type;
                }
                else
                {
                    AtavismLogger.LogError("skill definition id=" + id + " is null");
                    return null;
                }
            }
            else
            {
                if (id > 0)
                {

                    AtavismLogger.LogError("Storage doesn't contain skill definition id=" + id);
                    //    LoadSkillsPrefabData();
                }

                return null;
            }

            return aa;
        }

        /// <summary>
        /// Function to get all skill definitions and request missing icons form prefab server
        /// </summary>
        /// <returns></returns>
        public List<Skill> LoadAllSkill()
        {
            List<Skill> list = new List<Skill>();
            List<int> iconLack = new List<int>();
            foreach (SkillPrefabData ipd in prefabsdatadata.skills.Values)
            {
                Skill aa = new Skill();
                if (ipd != null)
                {
                    aa.id = ipd.id;
                    aa.skillname = ipd.skillname;
                    //aa.icon = ipd.icon;
                    aa.mainAspect = ipd.mainAspect;
                    aa.oppositeAspect = ipd.oppositeAspect;
                    aa.mainAspectOnly = ipd.mainAspectOnly;
                    aa.parentSkill = ipd.parentSkill;
                    aa.parentSkillLevelReq = ipd.parentSkillLevelReq;
                    aa.playerLevelReq = ipd.playerLevelReq;
                    aa.pcost = ipd.pcost;
                    aa.talent = ipd.talent;
                    aa.abilities = ipd.abilities;
                    aa.abilityLevelReqs = ipd.abilityLevelReqs;
                    aa.type = ipd.type;
                    list.Add(aa);
                    Sprite icon = GetIcon(ipd.iconPath);
                    if (icon == null && ipd.iconPath.Length > 0)                    
                    {
                        iconLack.Add(aa.id);
                    }
                }
            }

            if (iconLack.Count > 0)
            {
                string s = "";
                foreach (int id in iconLack)
                {
                    s += id + ";";
                }

                Dictionary<string, object> ps = new Dictionary<string, object>();
                ps.Add("objs", s);
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(ps, "SkillIcon");
            }

            return list;
        }
        
        /// <summary>
        /// Function sending list saved Stats to server to get update data
        /// </summary>
        public void LoadStatPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (StatProfileData ipd in prefabsdatadata.statProfiles.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("StatProfileData "+ipd.id+" "+ipd.name);
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Stats");
        }

        /// <summary>
        /// Function sending list saved Races to server to get update data
        /// </summary>
        public void LoadRaceData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (RaceData ipd in prefabsdatadata.races.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("LodRaceData "+ipd.id+" "+ipd.name);
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Race");
        }

        /// <summary>
        /// Function sending list saved Items to server to get update data
        /// </summary>
        public void LoadItemPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (ItemPrefabData ipd in prefabsdatadata.items.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("LoadItemPrefabData "+ipd.templateId+" "+ipd.name);
                props.Add("iId" + i, ipd.templateId);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Item");
        }
        
        /// <summary>
        /// Function sending list saved Weapon Profiles to server to get update data
        /// </summary>
        public void LoadWeaponProfilesPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (WeaponProfileData ipd in prefabsdatadata.weaponProfiles.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("LoadWeaponProfilesPrefabData "+ipd.id+" "+ipd.name);
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "WeaponProfile");
        }
        
        /// <summary>
        /// Function sending list saved Item Audio Profiles to server to get update data
        /// </summary>
        public void LoadItemAudioProfilesPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (ItemAudioProfileData ipd in prefabsdatadata.itemAudioProfiles.Values)
            {
                if(AtavismLogger.isLogDebug())
                    AtavismLogger.LogDebugMessage("LoadItemAudioProfilesPrefabData "+ipd.id+" "+ipd.name);
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "ItemAudioProfile");
        }

        /// <summary>
        /// Function sending list saved Crafting recipes to server to get update data
        /// </summary>
        public void LoadCraftingRecipePrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (CraftingRecipePrefabData ipd in prefabsdatadata.craftRecipes.Values)
            {
                props.Add("iId" + i, ipd.recipeID);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "CraftingRecipe");
        }

        /// <summary>
        /// Function sending list saved Crafting recipes to server to get update data
        /// </summary>
        public void LoadBuldingObjectsPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (BuildObjPrefabData ipd in prefabsdatadata.buildObjects.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "BuildingObject");
        }

        /// <summary>
        /// Function sending list saved currency to server to get update data
        /// </summary>

        public void LoadCurrencyPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (CurrencyPrefabData ipd in prefabsdatadata.currencies.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Currency");
        }

        /// <summary>
        /// Function sending list saved Items set to server to get update data
        /// </summary>

        public void LoadItemSetPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (ItemSetPrefabData ipd in prefabsdatadata.itemSets.Values)
            {
                props.Add("iId" + i, ipd.Setid);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "ItemSet");
        }

        /// <summary>
        /// Function sending list saved skills to server to get update data
        /// </summary>

        public void LoadSkillsPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (SkillPrefabData ipd in prefabsdatadata.skills.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Skill");
        }

        /// <summary>
        /// Function sending list saved abilities to server to get update data
        /// </summary>

        public void LoadAbilitiesPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (AbilityPrefabData ipd in prefabsdatadata.abilities.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Ability");
        }

        /// <summary>
        /// Function sending list saved effects to server to get update data
        /// </summary>

        public void LoadEffectsPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (EffectsPrefabData ipd in prefabsdatadata.effects.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Effect");
        }

        public void LoadResourceNodePrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (RecourceNodeProfileData ipd in prefabsdatadata.recourceNode.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "ResourceNode");
        }

        /// <summary>
        /// Send request update stored quests and get new  
        /// </summary>
        public void LoadQuestPrefabData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;

            foreach (QuestData ipd in prefabsdatadata.quests.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "Quest");
        }

        /// <summary>
        /// Function to get resource node definition
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="settingId"></param>
        /// <returns></returns>
        public RecourceNodeProfileSettingData LoadResourceNodeData(int profileId, int settingId)
        {
            if (profileId < 1 || settingId < 0)
                return null;
            if (prefabsdatadata.recourceNode.ContainsKey(profileId))
            {
                if (prefabsdatadata.recourceNode[profileId].settingList.ContainsKey(settingId))
                {
                    return prefabsdatadata.recourceNode[profileId].settingList[settingId];
                }
            }

            if (!resourceIconGet.ContainsKey(profileId + "|" + settingId))
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("objs", profileId + "|" + settingId + ";");
                AtavismClient.Instance.NetworkHelper.GetIconPrefabs(props, "ResourceNodeIcon");
                resourceIconGet.Add(profileId + "|" + settingId, Time.time);
            }
            else
            {
                if (resourceIconGet[profileId + "|" + settingId] + 2f < Time.time && AtavismClient.Instance.NetworkHelper.GetPrefabQueueSize() == 0)
                {
                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("objs", profileId + "|" + settingId + ";");
                    AtavismClient.Instance.NetworkHelper.GetIconPrefabs(props, "ResourceNodeIcon");
                    resourceIconGet[profileId + "|" + settingId] = Time.time;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to get definition for Glbal Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public GlobalEventData LoadGlobalEventsData(int eventId)
        {
            if (prefabsdatadata.globalEvents.ContainsKey(eventId))
            {
                return prefabsdatadata.globalEvents[eventId];
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("iId" + 0, eventId);
            props.Add("iDate" + 0, 0L);
            props.Add("c", 1);
            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(props, "GlobalEvents");
            return null;

        }

        /// <summary>
        /// Send request update for stored global events and get new
        /// </summary>
        public void LoadGlobalEventsData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (GlobalEventData ipd in prefabsdatadata.globalEvents.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetIconPrefabs(props, "GlobalEvents");
        }

        /// <summary>
        /// Function to get definition for interactive object profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public InteractiveObjectProfileData LoadInteractiveObjectProfilesData(int id)
        {
            if (prefabsdatadata.interactiveObjectProfiles.ContainsKey(id))
            {
                return prefabsdatadata.interactiveObjectProfiles[id];
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("iId" + 0, id);
            props.Add("iDate" + 0, 0L);
            props.Add("c", 1);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "InteractiveObjects");
            return null;

        }
        
        /// <summary>
        /// Send request update for stored interactive object profiles and get new
        /// </summary>
        public void LoadInteractiveObjectProfilesData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (InteractiveObjectProfileData ipd in prefabsdatadata.interactiveObjectProfiles.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "InteractiveObjects");
        }
        /// <summary>
        /// Function to get definition for slots profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SlotsProfileData LoadSlotsProfilesData(int id)
        {
            if (prefabsdatadata.slotsProfiles.ContainsKey(id))
            {
                return prefabsdatadata.slotsProfiles[id];
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("iId" + 0, id);
            props.Add("iDate" + 0, 0L);
            props.Add("c", 1);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "SlotsProfile");
            return null;

        }
        
        /// <summary>
        /// Send request update for stored interactive object profiles and get new
        /// </summary>
        public void LoadSlotsProfilesData()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            int i = 0;
            foreach (SlotsProfileData ipd in prefabsdatadata.slotsProfiles.Values)
            {
                props.Add("iId" + i, ipd.id);
                props.Add("iDate" + i, ipd.date);
                i++;
            }

            props.Add("c", i);
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "SlotsProfile");
        }

        public static AtavismPrefabManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Function to save icons to resource node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="settingId"></param>
        /// <param name="icons"></param>
        /// <param name="icons2"></param>
        /// <param name="iconc"></param>
        /// <param name="iconc2"></param>
        /// <param name="date"></param>
        public void SaveResourceNodeIcon(int id, int settingId, Sprite selectedIcon, string selectedIconData, string selectedIconPath, Texture2D cursorIcon, string cursorIconData, string cursorIconPath, long date)
        {
            if (prefabsdatadata.recourceNode.ContainsKey(id))
            {
                
                if (!prefabsdatadata.recourceNode[id].settingList.ContainsKey(settingId))
                {
                    prefabsdatadata.recourceNode[id].settingList[settingId] = new RecourceNodeProfileSettingData();
                    prefabsdatadata.recourceNode[id].settingList[settingId].id = settingId;
                }
                if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(selectedIconPath))
                {
                    prefabIconData[selectedIconPath].iconData = selectedIconData;
                    prefabIconData[selectedIconPath].icon = selectedIcon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = selectedIcon;
                    aid.iconData = selectedIconData;
                    prefabIconData.Add(selectedIconPath, aid);
                }
                if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(cursorIconPath))
                {
                    prefabIconData[cursorIconPath].iconData = cursorIconData;
                    prefabIconData[cursorIconPath].icon2d = cursorIcon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon2d = cursorIcon;
                    aid.iconData = cursorIconData;
                    prefabIconData.Add(cursorIconPath, aid);
                }

                prefabsdatadata.recourceNode[id].settingList[settingId].selectedIconPath = selectedIconPath;
                prefabsdatadata.recourceNode[id].settingList[settingId].cursorIconPath = cursorIconPath;
                prefabsdatadata.recourceNode[id].date = date;
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.recourceNode[id].settingList[settingId].selectedIcon = selectedIcon;
                    prefabsdatadata.recourceNode[id].settingList[settingId].selectedIconData = selectedIconData;
                    prefabsdatadata.recourceNode[id].settingList[settingId].cursorIcon = cursorIcon;
                    prefabsdatadata.recourceNode[id].settingList[settingId].cursorIconData = cursorIconData;
                }
            }
            else
            {
                prefabsdatadata.recourceNode[id] = new RecourceNodeProfileData();
                prefabsdatadata.recourceNode[id].id = id;
                prefabsdatadata.recourceNode[id].date = date;
                prefabsdatadata.recourceNode[id].settingList[settingId] = new RecourceNodeProfileSettingData();
                prefabsdatadata.recourceNode[id].settingList[settingId].id = settingId;
                prefabsdatadata.recourceNode[id].settingList[settingId].selectedIconPath = selectedIconPath;
                prefabsdatadata.recourceNode[id].settingList[settingId].cursorIconPath = cursorIconPath;
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.recourceNode[id].settingList[settingId].selectedIcon = selectedIcon;
                    prefabsdatadata.recourceNode[id].settingList[settingId].selectedIconData = selectedIconData;
                    prefabsdatadata.recourceNode[id].settingList[settingId].cursorIcon = cursorIcon;
                    prefabsdatadata.recourceNode[id].settingList[settingId].cursorIconData = cursorIconData;
                }
            }
        }



        /// <summary>
        /// Function to save for global event
        /// </summary>
        /// <param name="gvd"></param>
        public void SaveGlobalEvent(GlobalEventData gvd)
        {
            if (prefabsdatadata.globalEvents.ContainsKey(gvd.id))
                prefabsdatadata.globalEvents.Remove(gvd.id);
            prefabsdatadata.globalEvents.Add(gvd.id, gvd);
        }

        /// <summary>
        /// Function to save icon for global event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sprite"></param>
        /// <param name="icon2"></param>
        /// <param name="date"></param>
        public void SaveGlobalEventIcon(int id, Sprite icon, string iconData, string iconPath)
        {
            if (!useOldStoreIconsInDefinitions)
                if (prefabIconData.ContainsKey(iconPath))
                {
                    prefabIconData[iconPath].iconData = iconData;
                    prefabIconData[iconPath].icon = icon;
                }
                else
                {
                    AtavismIconData aid = new AtavismIconData();
                    aid.icon = icon;
                    aid.iconData = iconData;
                    prefabIconData.Add(iconPath, aid);
                }

            if (prefabsdatadata.globalEvents.ContainsKey(id))
            {
                if (useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.globalEvents[id].iconData = iconData;
                    prefabsdatadata.globalEvents[id].icon = icon;
                }

                prefabsdatadata.globalEvents[id].id = id;
            }
            else

            {
                prefabsdatadata.globalEvents[id] = new GlobalEventData();
                if (!useOldStoreIconsInDefinitions)
                {
                    prefabsdatadata.globalEvents[id].iconData = iconData;
                    prefabsdatadata.globalEvents[id].icon = icon;
                }
            }

            prefabsdatadata.globalEvents[id].id = id;
        }
        
        /// <summary>
        /// Function to save for interactive object profile
        /// </summary>
        /// <param name="iop"></param>
        public void SaveInteractiveObjectProfile(InteractiveObjectProfileData iop)
        {
            if (prefabsdatadata.interactiveObjectProfiles.ContainsKey(iop.id))
                prefabsdatadata.interactiveObjectProfiles.Remove(iop.id);
            prefabsdatadata.interactiveObjectProfiles.Add(iop.id, iop);
        }

        public void DeleteInteractiveObjectProfile(int id)
        {
            if (prefabsdatadata.interactiveObjectProfiles.ContainsKey(id))
                prefabsdatadata.interactiveObjectProfiles.Remove(id);
        }
        
        /// <summary>
        /// Function to save for slots profile
        /// </summary>
        /// <param name="iop"></param>
        public void SaveSlotsProfile(SlotsProfileData iop)
        {
            if (prefabsdatadata.slotsProfiles.ContainsKey(iop.id))
                prefabsdatadata.slotsProfiles.Remove(iop.id);
            prefabsdatadata.slotsProfiles.Add(iop.id, iop);
        }
        /// <summary>
        /// Finction to delete definition of slots profile
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void DeleteSlotsProfile(int id)
        {
            if (prefabsdatadata.slotsProfiles.ContainsKey(id))
                prefabsdatadata.slotsProfiles.Remove(id);
        }
    }
    
}


