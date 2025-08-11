using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//using Ceto;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Linq;
using System.Reflection;
//using VolumetricFogAndMist;
#if AT_PPS2_PRESET
using UnityEngine.Rendering.PostProcessing;
#endif
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using Atavism.UI;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;


namespace Atavism
{
    
    [Serializable]
    public class AtavismQualitySetingsDefault
    {
        public string name;
        public int pixelLightCount = 3;
        public int masterTextureLimit = 0;
        public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        public bool softParticles;
        public bool realtimeReflectionProbes;
        public bool billboardsFaceCameraPosition;
        public ShadowQuality shadows;
        public ShadowResolution shadowResolution;
        public float shadowDistance;
        public int shadowCascades;
        public SkinWeights blendWeights;
        public int verticalSync = 1;//false=Dont sync; True =Every V Bank
        public float lodBias = 2f;
        public int particleRaycastBudget = 4096;
    }





    [Serializable]
    public class AtCred
    {
        private string _l = "";
        private string _p = "";
        public string l
        {
            get
            {
                byte[] decodedBytes = Convert.FromBase64String(_l);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            set
            {
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(value);
                _l = Convert.ToBase64String(bytesToEncode);
            }
        }
        public string p
        {
            get
            {
                byte[] decodedBytes = Convert.FromBase64String(_p);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            set
            {
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(value);
                _p = Convert.ToBase64String(bytesToEncode);
            }
        }
    }

    [Serializable]
    public class AtavismGeneralSettings
    {
        public bool freeCamera = false;
        public string language;
        public float sensitivityMouse = 0.035f;
        public float sensitivityWheelMouse = -1f;
        public bool showHelmet = true;
        public bool saveCredential = false;
        public bool showTitle = true;
        public bool invertMouse = false;
        public DodgeDirectionEnum dodgeDirection = DodgeDirectionEnum.CharacterDirection; //0 - Character Direction; 1 - Camera forward; 2 - Camera Backward
        public int autoLootGroundMinQuality = 5;
    }

    public enum DodgeDirectionEnum
    {
        CharacterDirection,
        CameraForward,
        CameraBackward,
    }
    
    [Serializable]
    public class AtavismAudioSettings
    {
        public float masterLevel = -20f;
        public float sfxLevel = -20f;
        public float musicLevel = -15f;
        public float uiLevel = -20f;
        public float ambientLevel = -20f;
        public float footstepsLevel = -20f;

    }

    [Serializable]
    public class AtavismVideoSettings
    {
        public bool fps = false;
        public int quality = 5;
        public bool customSettings = false;
        //Quality Custom Settings
        public int shadows = 2;//0=Disable; 1=Hard only; 2=All(Soft&Hard)
        public int shadowDistance = 150;
        public int shadowResolution = 1;//0=Low; 1=Medium; 2=High; 3=VeryHigh
        public int verticalSync = 1;//false=Dont sync; True =Every V Bank
        [Range(0, 3)]
        public int lodBias = 2;
        public int particleRaycastBudget = 4096;
        public int masterTextureLimit = 0;
        public bool softParticles = true;
        //Camera Effects
        public bool amplifyOcclusionEffect = true;
        public bool seScreenSpaceShadows = true;
        public bool volumetricFog = true;
        public bool hxVolumetricCamera = true;
        // PostProcessingBehaviour Start
        public int antialiasing = 0;
        public bool depthOfField = true;
        public bool vignette = true;
        public bool chromaticAberration = true;
        public bool motionBlur = true;
        public bool bloom = true;
        public bool screenSpaceReflections = true;
        public bool dithering = true;
        public bool colorGrading = true;
        public bool autoExposure = true;
        // PostProcessingBehaviour End
        public bool ambientOcclusion = true;
        public bool depthBlur = true;
        //trawa
        [Range(0, 1)]
        public float detailObjectDensity = 1f;



    }


    [Serializable]
    public class AtavismAllSettings
    {
        public AtavismVideoSettings videoSettings = new AtavismVideoSettings();
        //    public ZbWaterSettings waterSettings = new ZbWaterSettings();
        public AtavismAudioSettings audioSettings = new AtavismAudioSettings();
        public AtavismGeneralSettings generalSettings = new AtavismGeneralSettings();
        public Dictionary<string, AtavismWindowsSettings> windowsSettings = new Dictionary<string, AtavismWindowsSettings>();
        public Dictionary<long, List<long>> questListSelected = new Dictionary<long, List<long>>();
      //  public List<long> questListSelected = new List<long>();
        public AtavismKeySettings keySettings = new AtavismKeySettings();
        [HideInInspector]
        public AtCred credential = new AtCred();
    }

    public enum KeyControlType
    {
        Movement,
        Window
    }
    [Serializable]
    public class AtavismKeyDefinition
    {
       [HideInInspector] public string name = "";       
        public KeyCode key = KeyCode.None;
        public KeyCode altKey = KeyCode.None;
       [HideInInspector] public bool useKeyAlt = false;
       [HideInInspector] public bool useKeyShift = false;
       [HideInInspector] public bool useKeyControl = false;
       [HideInInspector] public bool useAltKeyAlt = false;
       [HideInInspector] public bool useAltKeyShift = false;
       [HideInInspector] public bool useAltKeyControl = false;
       [HideInInspector] public bool useCommand = false;
       [HideInInspector] public bool defAlt = false;
       [HideInInspector] public bool defControl = false;
       [HideInInspector] public bool defShift = false;
       [HideInInspector] public bool defCommand = false;
        public bool canChange = true;
        public bool show = true;
        public KeyControlType type = KeyControlType.Movement;
    }

    [Serializable]
    public class AtavismKeySettings
    {
       
        public AtavismKeyDefinition moveForward = new AtavismKeyDefinition();
        public AtavismKeyDefinition moveBackward= new AtavismKeyDefinition();
        public AtavismKeyDefinition turnLeft= new AtavismKeyDefinition();
        public AtavismKeyDefinition turnRight= new AtavismKeyDefinition();
        public AtavismKeyDefinition strafeLeft= new AtavismKeyDefinition();
        public AtavismKeyDefinition strafeRight= new AtavismKeyDefinition();
        public AtavismKeyDefinition autoRun= new AtavismKeyDefinition();
        public AtavismKeyDefinition walkRun= new AtavismKeyDefinition();
        public AtavismKeyDefinition jump= new AtavismKeyDefinition();
        public AtavismKeyDefinition showHideWeapon= new AtavismKeyDefinition();
        public AtavismKeyDefinition inventory= new AtavismKeyDefinition();
        public AtavismKeyDefinition character= new AtavismKeyDefinition();
        public AtavismKeyDefinition mail= new AtavismKeyDefinition();
        public AtavismKeyDefinition guild= new AtavismKeyDefinition();
        public AtavismKeyDefinition quest= new AtavismKeyDefinition();
        public AtavismKeyDefinition skills= new AtavismKeyDefinition();
        public AtavismKeyDefinition map= new AtavismKeyDefinition();
        public AtavismKeyDefinition arena= new AtavismKeyDefinition();
        public AtavismKeyDefinition social= new AtavismKeyDefinition();
        public AtavismKeyDefinition sprint= new AtavismKeyDefinition();
        public AtavismKeyDefinition loot= new AtavismKeyDefinition();
        public AtavismKeyDefinition dodge= new AtavismKeyDefinition();
        public AtavismKeyDefinition switchWeapon = new AtavismKeyDefinition();
        public AtavismKeyDefinition action= new AtavismKeyDefinition();
        //public List<String> additionalActionsKey = new List<string>();
        public List<AtavismKeyDefinition> additionalActions = new List<AtavismKeyDefinition>();

        public AtavismKeyDefinition AdditionalActions(string key)
        {
            
            foreach (var aa in additionalActions)
            {
                if (aa.name.Equals(key))
                    return aa;
            }

            return null;
        }

        public bool dodgeDoubleTap = false;
    }

    [Serializable]
    public class AtavismWindowsSettings
    {
        public string windowName = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
    }
    [Serializable]
    public class DsMiniMapSettings
    {
        public GameObject minimapItemPrefab;
        public VisualTreeAsset minimapUiItemPrefab;
        public Sprite minimapIcon;
        public float minimapIconSize = 30;
        public Color minimapIconColor = Color.white;
        public Sprite minimapQuestConcludableIcon;
        public float minimapQuestConcludableIconSize = 30;
        public Color minimapQuestConcludableIconColor = Color.white;
        public Sprite minimapQuestProgressIcon;
        public float minimapQuestProgressIconSize = 30;
        public Color minimapQuestProgressIconColor = Color.white;
        public Sprite minimapQuestAvailableIcon;
        public float minimapQuestAvailableIconSize = 30;
        public Color minimapQuestAvailableIconColor = Color.white;
        public Sprite minimapQuestMobArea;
        public float minimapQuestMobAreaSize = 30;
        public Color minimapQuestMobAreaColor = Color.white;
        public Sprite minimapQuestTarget;
        public float minimapQuestTargetSize = 30;
        public Color minimapQuestTargetColor = Color.white;
        public Sprite minimapShopIcon;
        public float minimapShopIconSize = 30;
        public Color minimapShopIconColor = Color.white;
        public Sprite minimapBankIcon;
        public float minimapBankIconSize = 30;
        public Color minimapBankIconColor = Color.white;
        public Sprite minimapBossIcon;
        public float minimapBossIconSize = 30;
        public Color minimapBossIconColor = Color.white;
    }
    [Serializable]
    public class DsAdminLocation
    {
        [SerializeField]
        string _name;
        [SerializeField]
        Vector3 _loc;
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public Vector3 Loc
        {
            get
            {
                return _loc;
            }
        }
    }

    [Serializable]
    public class ClassAvatar
    {
        public string name = "ExampleClass";
        public Sprite[] avatars;
    }

    [Serializable]
    public class GenderAvatar
    {
        public string name = "ExampleGender";
        public List<ClassAvatar> classes = new List<ClassAvatar>();
    }

    [Serializable]
    public class RaceAvatar
    {
        public string name= "ExampleRace";
        public List<GenderAvatar> genders =new List<GenderAvatar>();
    }

    [Serializable]
    public class DsAdminPanelSettings
    {
        [SerializeField]
        string _instanceName;
        [SerializeField]
        List<DsAdminLocation> _locs = new List<DsAdminLocation>();
        public string InstanceName
        {
            get
            {
                return _instanceName;
            }
        }
        public List<DsAdminLocation> Loc
        {
            get
            {
                return _locs;
            }
        }
    }

    [Serializable]
    public class DsAdminRestart
    {
        [SerializeField]
        string _name;
        [SerializeField]
        string _message;
        [SerializeField]
        string _schedule;
        [SerializeField]
        int  _CountdownTime;
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public string Message
        {
            get
            {
                return _message;
            }
        }
        public string Schedule
        {
            get
            {
                return _schedule;
            }
        }
        public int CountdownTime
        {
            get
            {
                return _CountdownTime;
            }
        }
    }

    public class AtavismSettings : MonoBehaviour
    {
        static AtavismSettings instance;

        #region Variable Settings
        [AtavismSeparator("General Settings")]
        [HideInInspector]public bool autoPlayCharacter = false;
        [SerializeField] AtavismAllSettings settings = new AtavismAllSettings();
        public double maxTimeBetweenDoubleTap = 0.2;
        public KeyCode openGameSettingsKey = KeyCode.Escape;
        public bool useSameKeyForBarMenuFromGameSetting = false;
        public KeyCode openToolBarMenuKey = KeyCode.None; 
        [SerializeField] int questPrevLimit = 4;
        public Sprite expIcon;
        


        public bool saveInFile = true;
        public bool dragJoysticStopAutoAttack = false;
        List<MonoBehaviour> openedWindow = new List<MonoBehaviour>();
        List<UIAtavismWindowBase> openedUIToolkitWindow = new List<UIAtavismWindowBase>();
        List<UIAtavismWindowBase> openedUIToolkitWindowEscKey = new List<UIAtavismWindowBase>();
        private bool _isMenuBarOpened = false;
        [AtavismSeparator("Admin Panel Settings")]
        [SerializeField] List<DsAdminPanelSettings> _adminLocationSettings = new List<DsAdminPanelSettings>();
        [SerializeField] List<DsAdminRestart> _adminRestartSettings = new List<DsAdminRestart>();

        
        
        [AtavismSeparator("Prefabs")]
        //new GUI
        public UnityEngine.UIElements.VisualTreeAsset UISlotInventoryItemUXML;
        public UnityEngine.UIElements.VisualTreeAsset UISlotActionBarItemUXML;
        public UnityEngine.UIElements.VisualTreeAsset UISlotBagItemUXML;
        public UnityEngine.UIElements.VisualTreeAsset UIActivatableUXML;
        //old GUI
        public UGUIAtavismActivatable actionBarPrefab;
        public UGUIAtavismActivatable inventoryItemPrefab;
        public UGUIAtavismActivatable abilitySlotPrefab;
        [AtavismSeparator("Inventory ")]
        public Color effectQualityColor = Color.white;
        public Color abilityQualityColor = Color.white;
        public List<Color> itemQualityColor;
        [HideInInspector]public Dictionary<int,String> qualityNames = new Dictionary<int,String>();
        public Sprite defaultBagIcon;
        public Sprite defaultItemIcon;
        public Color itemDropColorTrue = Color.blue;
        public Color itemDropColorFalse = Color.red;
        /*  [AtavismSeparator("Quest Minimap Icons")]
           public Sprite questAvailableIcon;
           public Sprite questInProgressIcon;
           public Sprite questConcludableIcon;
           public Sprite dialogAvailableIcon;
           public Sprite itemToSellIcon;*/
        [AtavismSeparator("Player/Mob Names Settings")]
        [SerializeField] bool visibleMobsName = true;
        [SerializeField] bool visibleShopMessageOnSelf = true;
        [SerializeField] bool visibleOid = false;
        [SerializeField] bool showLevelWithName = false;

        [SerializeField] TMP_FontAsset mobNameFont;
        [SerializeField] Vector3 mobNamePosition = new Vector3(0f, 0.5f, 0f);
        [SerializeField] int mobNameFontSize = 2;
        [SerializeField] Color mobNameDefaultColor = Color.white;
        [SerializeField] TextAlignmentOptions mobNameAlignment = TextAlignmentOptions.Midline;
        [SerializeField] Vector4 mobNameMargin = new Vector4(7f, 2f, 7f, 2f);
        [SerializeField] float mobNameOutlineWidth = 0.2f;
        [SerializeField] string questNewText = "!";
        [SerializeField] string questConcludableText = "*";
        [SerializeField] string questProgressText = "?";
        [SerializeField] string shopText = "";
        [SerializeField] string bankText = "bank";
        [SerializeField] int npcInfoTextSize = 10;
        [SerializeField] Color npcInfoTextColor = Color.white;
        [SerializeField] Vector3 npcInfoTextPosition = new Vector3(0f, 0.5f, 0f);
        [SerializeField] TMP_SpriteAsset npcInfoSpriteAsset;

        [AtavismSeparator("Avatar Icons")]
       // public Sprite[] meleAvatars;
       // public Sprite[] femaleAvatars;

        [SerializeField] List<RaceAvatar> races = new List<RaceAvatar>();
        [AtavismSeparator("Audio Settings")]
        public AudioMixer masterMixer;
        public AudioMixerSnapshot mixerFocus;
        public AudioMixerSnapshot mixerNoFocus;
        //private bool gameFocus = true;
        GameObject mainCamera;
        GameObject effectCamera;
        Camera characterPanelCamera;
        Camera otherCharacterPanelCamera;
        Camera petPanelCamera;

        Transform characterPanelSpawn;
        Transform otherCharacterPanelSpawn;
        Transform petPanelSpawn;
        GameObject characterAvatar;
        GameObject otherCharacterAvatar;
        GameObject petAvatar;
        [AtavismSeparator("Graphic Quality")]
        [SerializeField]
        List<AtavismQualitySetingsDefault> _defaultQualitySettings = new List<AtavismQualitySetingsDefault>();
#if AT_PPS2_PRESET
    List<PostProcessProfile> postProcessProfiles = new List<PostProcessProfile>();
    List<PostProcessVolume> postProcessVolumes = new List<PostProcessVolume>();
#endif
        string createURL_PL = "https://www.atavismonline.com";
        string forgotURL_PL = "https://www.atavismonline.com";
        string webURL_PL = "https://www.atavismonline.com";
        string shopURL_PL = "https://www.atavismonline.com";
        string createURL_EN = "https://www.atavismonline.com";
        string forgotURL_EN = "https://www.atavismonline.com";
        string webURL_EN = "https://www.atavismonline.com";
        string shopURL_EN = "https://www.atavismonline.com";

        [AtavismSeparator("Uma Settings")]
        [SerializeField] GameObject UMAGameObject;
        [SerializeField] GameObject wardrobeGameObject;
        [AtavismSeparator("Loading Screen Settings")]

        [SerializeField] GameObject sceneLoaderPrefab;
        private AssetBundle m_assetsMob;
        private AssetBundle m_assetsNpc;
        //    public vAudioSurface defaultSurface;
        //    public List<vAudioSurface> customSurfaces;

        [AtavismSeparator("UGUI Mini Map Settings")]
        [SerializeField] DsMiniMapSettings minimapSettings = new DsMiniMapSettings();

       // [HideInInspector]
       // [AtavismSeparator("LevelUp Effect Setting")]
        [HideInInspector]
         [SerializeField] GameObject levelUpPrefab;
        // [MinMax(1f, 99f), DisplayName("Filtering (%)"), Tooltip("")]
        [HideInInspector]
        [SerializeField] float levelUpPrefabDuration = 2f;
       //  Dictionary<string, DsWindowsSettings> windowsSettings;

        [AtavismSeparator("Instance settings")]
        [SerializeField] List<string> gameInstances = new List<string>();
        [SerializeField] List<string> arenaInstances = new List<string>();
        [SerializeField] List<string> dungeonInstances = new List<string>();
        
        
        UIDropdown openedUIDropdown = null;
        #endregion
        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            mainCamera = GameObject.Find("MainCamera");
            effectCamera = GameObject.Find("EffectCamera");
            
            // AtavismEventSystem.RegisterEvent("OPEN_WINDOW", this);
            // AtavismEventSystem.RegisterEvent("CLOSE_WINDOW", this);
            
            Load();
            LoadUmaWardrobe();
            LoadLoader();
        }

        void ClientReady()
        {
            AtavismLogger.LogDebugMessage("GameSettings ClientReady");
         //   Debug.LogError("GameSettings ClientReady");
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("Race", HandleRacePrefabData);
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("ActionSettings", HandleActionsPrefabData);
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("GameSettings", HandleGamePrefabData);
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("Stats", HandleStatsData);
            Dictionary<string, object> props = new Dictionary<string, object>();
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "ActionSettings");
            AtavismClient.Instance.NetworkHelper.GetPrefabs(props, "GameSettings");
        }

        private void HandleStatsData(Dictionary<string, object> props)
        {
            //  Debug.LogError("HandleStatsData Start");
            try
            {
                // Debug.LogError("HandleStatsData | qualityNames Count "+qualityNames.Count);
                int num = (int) props["num"];
                bool sendAll = (bool)props["all"];
                for (int i = 0; i < num; i++)
                {
                    int id = (int) props["s" + i+"id"];
                    string sname = (string) props["s" + i + "name"];
                    int prec = (int) props["s" + i + "prec"];
                    long date = (long) props["s" + i + "date"];
                    StatProfileData spd = new StatProfileData();
                    spd.id = id;
                    spd.name = sname;
                    spd.precision = prec;
                    spd.division = (int)Math.Pow(10,prec);
                    spd.date = date;
                   AtavismPrefabManager.Instance.SaveStat(spd);
                }
                if (props.ContainsKey("toRemove"))
                {
                    string keys = (string)props["toRemove"];
                    if (keys.Length > 0)
                    {
                        string[] _keys = keys.Split(';');
                        foreach (string k in _keys)
                        {
                            if (k.Length > 0)
                            {
                                AtavismPrefabManager.Instance.DeleteStat(int.Parse(k));
                            }
                        }
                    }
                }
                if (sendAll)
                {
                    AtavismPrefabManager.Instance.reloaded++;
                    if(AtavismLogger.logLevel <= LogLevel.Debug) 
                        Debug.Log("All data received for stats.");
                }
                else
                {
                    AtavismPrefabManager.Instance.LoadStatPrefabData();
                    AtavismLogger.LogWarning("Not all skills data was sent by Prefab server");
                }
            } catch (System.Exception e)
            {
                AtavismLogger.LogError("Exception loading Stats data Exception:" + e);
            }
            //  Debug.LogError("HandleStatsData End");
        }
         private void HandleGamePrefabData(Dictionary<string, object> props)
        {
             //  Debug.LogError("HandleGamePrefabData Start");
            try
            {
               // Debug.LogError("HandleGamePrefabData | qualityNames Count "+qualityNames.Count);
                int num = (int) props["num"];
                qualityNames.Clear();
                for (int i = 0; i < num; i++)
                {
                    int actionId = (int) props["i" + i];
                    string action = (string) props["c" + i];
                    qualityNames.Add(i+1,action);
                }
               
            } catch (System.Exception e)
            {
                AtavismLogger.LogError("Exception loading game settings prefab data Exception:" + e);
            }
            //  Debug.LogError("HandleGamePrefabData End");
        }
         
        private void HandleActionsPrefabData(Dictionary<string, object> props)
        {
             //  Debug.LogError("HandleActionsPrefabData Start");
            try
            {
               // Debug.LogError("HandleActionsPrefabData | Count "+settings.keySettings.additionalActions.Count);
                List<AtavismKeyDefinition> newList = new List<AtavismKeyDefinition>();
                int num = (int) props["aNum"];
                for (int i = 0; i < num; i++)
                {
                    
                    int actionId = (int) props["k" + i];
                    string action = (string) props["a" + i];
                    //Debug.LogError("action "+actionId+" "+action);
                    if (settings.keySettings.AdditionalActions(action) == null)
                    {
                        AtavismKeyDefinition adk = new AtavismKeyDefinition();
                        adk.name = action;
                        newList.Add(adk);
                    }
                    else
                    {
                        newList.Add(settings.keySettings.AdditionalActions(action));
                    }
                }

                settings.keySettings.additionalActions = newList;
             //   Debug.LogError("HandleActionsPrefabData || Count "+settings.keySettings.additionalActions.Count);
                // foreach (var action in list)
                // {
                //     settings.keySettings.additionalActions.Remove(action);
                // }
              //  Debug.LogError("HandleActionsPrefabData ||| Count "+settings.keySettings.additionalActions.Count);
            } catch (System.Exception e)
            {
                AtavismLogger.LogError("Exception loading actions prefab data Exception:" + e);
            }
            //  Debug.LogError("HandleActionsPrefabData End");
        }

        private void HandleRacePrefabData(Dictionary<string, object> props)
        {
           //   Debug.LogError("HandleRacePrefabData ");
            try
            {
                var races = AtavismPrefabManager.Instance.GetRaceData();
                races.Clear();
                int num = (int) props["num"];
                for (int i = 0; i < num; i++)
                {
                  //  Debug.LogError("HandleRacePrefabData i="+i);

                  
                    int raceId = (int) props["raceId" + i];
                 //   Debug.LogError("HandleRacePrefabData raceId="+raceId);
                    RaceData race = new RaceData();
                    if (races.ContainsKey(raceId))
                    {
                        race = races[raceId];
                    }

                    race.id = raceId;
                    race.name = (string) props["raceName" + i];
                    race.description = (string) props["raceDesc" + i];
                    race.iconPath = (string) props["raceIconP" + i];

                    string iconData = (string) props["raceIcon" + i];
                    
                    Texture2D tex = new Texture2D(2, 2);
                    bool wyn = tex.LoadImage(System.Convert.FromBase64String(iconData));
                    Sprite icon = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    if (AtavismPrefabManager.Instance.useOldStoreIconsInDefinitions)
                    {
                        race.iconData = iconData;
                        race.icon = icon;
                    }
                    AtavismPrefabManager.Instance.SaveIcon(icon, iconData, race.iconPath);
                    int classId = (int) props["classId" + i];
                  //  Debug.LogError("HandleRacePrefabData classId="+classId);

                    ClassData c = new ClassData();
                    if (race.classList.ContainsKey(classId))
                    {
                        c = race.classList[classId];
                    }

                    c.id = classId;
                    c.name = (string) props["className" + i];
                    c.description = (string) props["classDesc" + i];
                    c.iconPath = (string) props["classIconP" + i];
                    iconData = (string) props["classIcon" + i];
                     tex = new Texture2D(2, 2);
                     wyn = tex.LoadImage(System.Convert.FromBase64String(iconData));
                    icon = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    if (AtavismPrefabManager.Instance.useOldStoreIconsInDefinitions)
                    {
                        c.iconData = iconData;
                        c.icon = icon;
                    }

                    AtavismPrefabManager.Instance.SaveIcon(icon, iconData, c.iconPath);

                     int gnum = (int) props["gennum" + i];
                 //   Debug.LogError("HandleRacePrefabData gnum=" + gnum);
                    if(gnum>0)
                    for (int k = 0; k < gnum; k++)
                    {
                        int genderId = (int) props["genId" + i+"_"+k];
                       // Debug.LogError("HandleRacePrefabData genderId="+genderId+" c.genderList.count="+c.genderList.Count);

                        GenderData gender = new GenderData();
                        if (c.genderList.ContainsKey(genderId))
                        {
                            gender = c.genderList[genderId];
                        }

                        gender.id = genderId;
                        gender.name = (string) props["genName" + i + "_" + k];
                        gender.prefab = (string) props["genPrefab" + i + "_" + k];
                        gender.iconPath = (string) props["genIconP" + i + "_" + k];
                        iconData = (string) props["genIcon" + i + "_" + k];
                         tex = new Texture2D(2, 2);
                         wyn = tex.LoadImage(System.Convert.FromBase64String(iconData));
                        icon = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        if (AtavismPrefabManager.Instance.useOldStoreIconsInDefinitions)
                        {
                            gender.iconData = iconData;
                            gender.icon = icon;
                        }
                        AtavismPrefabManager.Instance.SaveIcon(icon, iconData, gender.iconPath);
                        c.genderList[genderId] = gender;

                    }
                //    Debug.LogError("HandleRacePrefabData race="+race.id+" classId="+classId+" c.genderList.count="+c.genderList.Count);

                    race.classList[classId]= c;
                   // race.date = (long) props["date" + i];

                    races[raceId] = race;
                }
              //  Debug.LogError("HandleRacePrefabData |");

                if (props.ContainsKey("RaceToRemove"))
                {
                    string keys = (string) props["RaceToRemove"];
                    if (keys.Length > 0)
                    {
                        string[] _keys = keys.Split(';');
                        foreach (string k in _keys)
                        {
                            if (k.Length > 0)
                            {
                                AtavismPrefabManager.Instance.GetRaceData().Remove(int.Parse(k));
                            }
                        }
                    }
                }
                AtavismPrefabManager.Instance.reloaded++;

            }
            catch (System.Exception e)
            {
                AtavismLogger.LogError("Exception loading race prefab data Exception:" + e);
            }
          //  Debug.LogError("HandleRacePrefabData End");

        }

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            //     if (GameObject.Find("UMA") == null && UMAGameObject) {
            //          Instantiate(UMAGameObject);
            //      }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if ((scene.name == "Login" || scene.name == ClientAPI.Instance.characterSceneName) && Quests.Instance != null)
            {
                Quests.Instance.QuestLogEntries.Clear();
                Quests.Instance.QuestHistoryLogEntries.Clear();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (GameObject.Find("/UMA") == null && UMAGameObject)
                {
                    Instantiate(UMAGameObject);
                }
            }
            if(AtavismClient.Instance.LoadLevelName.Equals(scene.name))
            {
                openedWindow.Clear();
                if (!scene.name.Equals("Login") && !scene.name.Equals(ClientAPI.Instance.characterSceneName))
                {
#if !AT_MOBILE
                    if (openedWindow.Count == 0 && !isMenuBarOpened)
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.lockState = CursorLockMode.Locked;
                    }
#endif
                    
                }
                openedUIToolkitWindow.Clear();
                openedUIToolkitWindowEscKey.Clear();
            } 
        }
        
        // public void OnEvent(AtavismEventData eData)
        // {
        //     if (eData.eventType == "OPEN_WINDOW")
        //     {
        //        
        //     } else if (eData.eventType == "CLOSE_WINDOW")
        //     {
        //         
        //     }
        // }
        //
        
        void Update()
        {
            if (ClientAPI.Instance == null && !SceneManager.GetActiveScene().name.Equals("Login"))
            {
                SceneManager.LoadScene("Login");
            }

            if (!ClientAPI.UIHasFocus() && Input.GetKeyUp(KeyCode.Escape))
            {
                if (openedUIToolkitWindowEscKey.Count > 0)
                {
                    openedUIToolkitWindowEscKey[openedUIToolkitWindowEscKey.Count-1].Hide();
                }
            }
            
        }

        void OnEnabled()
        {

        }

        void Save()
        {

            //   Debug.LogError("Save settings");
            if (saveInFile)
            {
                //    Debug.LogError("Save settings start");
#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA_10_0 || UNITY_WSA
                if (!Directory.Exists(Application.persistentDataPath+"/" + Application.productName))
                    Directory.CreateDirectory(Application.persistentDataPath+"/" + Application.productName);
                FileStream file = File.Create( Application.persistentDataPath+"/" + Application.productName + "/user.settings");
#elif UNITY_XBOXONE
                if (File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/user.settings"))
                {
                    FileStream file = File.Open(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/user.settings", FileMode.Open);                
#else
                if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName))
                    Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName);
                FileStream file = File.Create(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/user.settings");
#endif
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(file, settings);
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Failed to serialize. Reason: " + e.Message+"\n\n"+e.StackTrace);

                    throw;
                }
                finally
                {
                    file.Close();
                }
                //      Debug.LogError("Save settings end");

            }
            else
            {
                PlayerPrefs.SetFloat("masterLevel", settings.audioSettings.masterLevel);
                PlayerPrefs.SetFloat("sfxLevel", settings.audioSettings.sfxLevel);
                PlayerPrefs.SetFloat("musicLevel", settings.audioSettings.musicLevel);
                PlayerPrefs.SetFloat("uiLevel", settings.audioSettings.uiLevel);
                PlayerPrefs.SetFloat("ambientLavel", settings.audioSettings.ambientLevel);
                PlayerPrefs.SetFloat("footstepsLevel", settings.audioSettings.footstepsLevel);

                PlayerPrefs.SetInt("Video fps", settings.videoSettings.fps == true ? 1 : 0);
                PlayerPrefs.SetInt("Video texture limit", settings.videoSettings.masterTextureLimit);
                PlayerPrefs.SetInt("Video quality", settings.videoSettings.quality);
                PlayerPrefs.SetInt("Video ambient occlusion", settings.videoSettings.ambientOcclusion == true ? 1 : 0);
                PlayerPrefs.SetInt("Video deep blur", settings.videoSettings.depthBlur == true ? 1 : 0);
                PlayerPrefs.SetInt("Video bloom", settings.videoSettings.bloom == true ? 1 : 0);
                PlayerPrefs.SetInt("Video Vertical sync", settings.videoSettings.verticalSync);
                PlayerPrefs.SetInt("Video shadows", settings.videoSettings.shadows);
                PlayerPrefs.SetFloat("Video lod bias", settings.videoSettings.lodBias);
                PlayerPrefs.SetInt("Video antialiasing", settings.videoSettings.antialiasing);
                //General Settings
                PlayerPrefs.SetString("Language", settings.generalSettings.language);
                PlayerPrefs.SetInt("freeCamera", settings.generalSettings.freeCamera == true ? 1 : 0);
                PlayerPrefs.SetFloat("mSens", settings.generalSettings.sensitivityMouse);
                PlayerPrefs.SetFloat("mWSens", settings.generalSettings.sensitivityWheelMouse);
                PlayerPrefs.SetInt("showHelm", settings.generalSettings.showHelmet == true ? 1 : 0);
                PlayerPrefs.SetInt("savec", settings.generalSettings.saveCredential == true ? 1 : 0);
                PlayerPrefs.SetInt("alq", settings.generalSettings.autoLootGroundMinQuality );

                //Keys
                PlayerPrefs.SetInt("ksl", (int)settings.keySettings.strafeLeft.key);
                PlayerPrefs.SetInt("aksl", (int)settings.keySettings.strafeLeft.altKey);
                PlayerPrefs.SetInt("ksr", (int)settings.keySettings.strafeRight.key);
                PlayerPrefs.SetInt("aksr", (int)settings.keySettings.strafeRight.altKey);
                PlayerPrefs.SetInt("kmf", (int)settings.keySettings.moveForward.key);
                PlayerPrefs.SetInt("akmf", (int)settings.keySettings.moveForward.altKey);
                PlayerPrefs.SetInt("kmb", (int)settings.keySettings.moveBackward.key);
                PlayerPrefs.SetInt("akmb", (int)settings.keySettings.moveBackward.altKey);
                PlayerPrefs.SetInt("ktl", (int)settings.keySettings.turnLeft.key);
                PlayerPrefs.SetInt("aktl", (int)settings.keySettings.turnLeft.altKey);
                PlayerPrefs.SetInt("ktr", (int)settings.keySettings.turnRight.key);
                PlayerPrefs.SetInt("aktr", (int)settings.keySettings.turnRight.altKey);
                PlayerPrefs.SetInt("kar", (int)settings.keySettings.autoRun.key);
                PlayerPrefs.SetInt("akar", (int)settings.keySettings.autoRun.altKey);
                PlayerPrefs.SetInt("kwr", (int)settings.keySettings.walkRun.key);
                PlayerPrefs.SetInt("akwr", (int)settings.keySettings.walkRun.altKey);
                PlayerPrefs.SetInt("kju", (int)settings.keySettings.jump.key);
                PlayerPrefs.SetInt("akju", (int)settings.keySettings.jump.altKey);
                PlayerPrefs.SetInt("ksw", (int)settings.keySettings.showHideWeapon.key);
                PlayerPrefs.SetInt("aksw", (int)settings.keySettings.showHideWeapon.altKey);
                PlayerPrefs.SetInt("kdo", (int)settings.keySettings.dodge.key);
                PlayerPrefs.SetInt("akdo", (int)settings.keySettings.dodge.altKey);
                PlayerPrefs.SetInt("ksp", (int)settings.keySettings.sprint.key);
                PlayerPrefs.SetInt("aksp", (int)settings.keySettings.sprint.altKey);
                PlayerPrefs.SetInt("klo", (int)settings.keySettings.loot.key);
                PlayerPrefs.SetInt("aklo", (int)settings.keySettings.loot.altKey);

                PlayerPrefs.SetInt("kinv", (int)settings.keySettings.inventory.key);
                PlayerPrefs.SetInt("akinv", (int)settings.keySettings.inventory.altKey);
                PlayerPrefs.SetInt("kchar", (int)settings.keySettings.character.key);
                PlayerPrefs.SetInt("akchar", (int)settings.keySettings.character.altKey);
                PlayerPrefs.SetInt("kmail", (int)settings.keySettings.mail.key);
                PlayerPrefs.SetInt("akmail", (int)settings.keySettings.mail.altKey);
                PlayerPrefs.SetInt("kquild", (int)settings.keySettings.guild.key);
                PlayerPrefs.SetInt("akquild", (int)settings.keySettings.guild.altKey);
                PlayerPrefs.SetInt("kquest", (int)settings.keySettings.quest.key);
                PlayerPrefs.SetInt("akquest", (int)settings.keySettings.quest.altKey);
                PlayerPrefs.SetInt("kskill", (int)settings.keySettings.skills.key);
                PlayerPrefs.SetInt("akskill", (int)settings.keySettings.skills.altKey);
                PlayerPrefs.SetInt("kmap", (int)settings.keySettings.map.key);
                PlayerPrefs.SetInt("akmap", (int)settings.keySettings.map.altKey);
                PlayerPrefs.SetInt("karena", (int)settings.keySettings.arena.key);
                PlayerPrefs.SetInt("akarena", (int)settings.keySettings.arena.altKey);
                PlayerPrefs.SetInt("ksoc", (int)settings.keySettings.social.key);
                PlayerPrefs.SetInt("aksoc", (int)settings.keySettings.social.altKey);
                PlayerPrefs.SetInt("ddt", settings.keySettings.dodgeDoubleTap == true ? 1 : 0);
                //Quests
               // PlayerPrefs.SetString("qs", string.Join(";", settings.questListSelected.Select(x => x.ToString()).ToArray()));
                PlayerPrefs.SetString("qs", string.Join("!", settings.questListSelected.Select(x => x.Key + "=" + string.Join(";", x.Value.Select(y => y.ToString()).ToArray())).ToArray()));
               // string.Join(";", x.Value.Select(y => y.ToString()).ToArray()));
                //Credential
                PlayerPrefs.SetString("cl", settings.credential.l);
                PlayerPrefs.SetString("cp", settings.credential.p);
                // PlayerPrefs.SetString("Windows",)
                PlayerPrefs.Save();
            }
        }

        void Load()
        {
            //  Debug.LogError("Load settings");
            if (saveInFile)
            {
#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA_10_0 || UNITY_WSA
                if (File.Exists( Application.persistentDataPath+"/" + Application.productName + "/user.settings"))
                {
                    FileStream file = File.Open( Application.persistentDataPath+"/" + Application.productName + "/user.settings", FileMode.Open);
#elif UNITY_XBOXONE
                if (File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/user.settings"))
                {
                    FileStream file = File.Open(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/" + Application.productName + "/user.settings", FileMode.Open);
#else
                if (File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/user.settings"))
                {
                    FileStream file = File.Open(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/user.settings", FileMode.Open);
#endif
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        AtavismAllSettings _settings = (AtavismAllSettings)bf.Deserialize(file);
                        if (_settings.keySettings == null || _settings.keySettings.switchWeapon == null || _settings.keySettings.switchWeapon.key == null || _settings.keySettings.switchWeapon.altKey == null)
                        {
                            _settings.keySettings = settings.keySettings;
                            Debug.LogWarning("Replaced key settings by default settings");
                        }
                        if(_settings.keySettings.additionalActions is IDictionary )
                        {
                            AtavismLogger.LogWarning("isDictionary");
                            _settings.keySettings.additionalActions = settings.keySettings.additionalActions;
                        }
                        if (_settings.generalSettings.autoLootGroundMinQuality == null)
                            _settings.generalSettings.autoLootGroundMinQuality = 5;
                        
                        foreach (FieldInfo p in typeof(AtavismKeySettings).GetFields())
                        {
    
                            AtavismKeyDefinition akdOld = p.GetValue(_settings.keySettings) as AtavismKeyDefinition;
                            AtavismKeyDefinition akdNew = p.GetValue(settings.keySettings) as AtavismKeyDefinition;
                            if (akdNew != null )
                            {
                                if (akdOld != null)
                                {
                                    akdOld.canChange = akdNew.canChange;
                                    akdOld.show = akdNew.show;
                                }
                                else
                                {
                                    p.SetValue(_settings.keySettings,akdNew);
                                }
                            }
                        }
                        
                        foreach (var akdOld in _settings.keySettings.additionalActions)
                        {
                            foreach (var akdNew in settings.keySettings.additionalActions)
                            {
                                if (akdNew != null && akdOld != null)
                                {
                                    if (akdNew.name.Equals(akdOld.name))
                                    {
                                        akdOld.canChange = akdNew.canChange;
                                        akdOld.show = akdNew.show;
                                    }
                                }
                            }
                        }
                        settings = _settings;
                        
                     //   Debug.LogError("Windows Settings settings.windowsSettings="+(settings.windowsSettings!=null?settings.windowsSettings.Count:"null") );
                        
                    }
                    catch (SerializationException e)
                    {
                        Debug.LogError("Failed to deserialize. Reason: " + e.Message + "\n\n" + e.StackTrace);
                        throw;
                    }
                    finally
                    {
                        file.Close();
                    }

                    applySettings();
                }
                else
                {
                    Save();
                }
            }
            else
            {
                if (settings == null)
                    settings = new AtavismAllSettings();
                settings.credential.p = PlayerPrefs.GetString("cp");
                settings.credential.l = PlayerPrefs.GetString("cl");
                settings.audioSettings.masterLevel = PlayerPrefs.GetFloat("masterLevel", settings.audioSettings.masterLevel);
                settings.audioSettings.sfxLevel =  PlayerPrefs.GetFloat("sfxLevel", settings.audioSettings.sfxLevel);
                settings.audioSettings.musicLevel = PlayerPrefs.GetFloat("musicLevel", settings.audioSettings.musicLevel);
                settings.audioSettings.uiLevel = PlayerPrefs.GetFloat("uiLevel", settings.audioSettings.uiLevel);
                settings.audioSettings.ambientLevel = PlayerPrefs.GetFloat("ambientLavel", settings.audioSettings.ambientLevel);
                settings.audioSettings.footstepsLevel = PlayerPrefs.GetFloat("footstepsLevel", settings.audioSettings.footstepsLevel);

                settings.videoSettings.fps = PlayerPrefs.GetInt("Video fps")==1;//, settings.videoSettings.fps == true ? 1 : 0);
                settings.videoSettings.masterTextureLimit = PlayerPrefs.GetInt("Video texture limit");//, settings.videoSettings.masterTextureLimit);
                settings.videoSettings.quality = PlayerPrefs.GetInt("Video quality");//, settings.videoSettings.quality);
                settings.videoSettings.ambientOcclusion = PlayerPrefs.GetInt("Video ambient occlusion") == 1;//, settings.videoSettings.ambientOcclusion == true ? 1 : 0);
                settings.videoSettings.depthBlur = PlayerPrefs.GetInt("Video deep blur") == 1;//, settings.videoSettings.depthBlur == true ? 1 : 0);
                settings.videoSettings.bloom = PlayerPrefs.GetInt("Video bloom") == 1;//, settings.videoSettings.bloom == true ? 1 : 0);
                settings.videoSettings.verticalSync = PlayerPrefs.GetInt("Video Vertical sync");//, settings.videoSettings.verticalSync);
                settings.videoSettings.shadows = PlayerPrefs.GetInt("Video shadows");//, settings.videoSettings.shadows);
                settings.videoSettings.lodBias = PlayerPrefs.GetInt("Video lod bias");//, settings.videoSettings.lodBias);
                settings.videoSettings.antialiasing = PlayerPrefs.GetInt("Video antialiasing");//, settings.videoSettings.antialiasing);
                //General Settings
                settings.generalSettings.language = PlayerPrefs.GetString("Language");//, settings.generalSettings.language);
                settings.generalSettings.freeCamera = PlayerPrefs.GetInt("freeCamera") == 1;//, settings.generalSettings.freeCamera == true ? 1 : 0);
                settings.generalSettings.sensitivityMouse = PlayerPrefs.GetFloat("mSens");//, settings.generalSettings.sensitivityMouse);
                settings.generalSettings.sensitivityWheelMouse = PlayerPrefs.GetFloat("mWSens");//, settings.generalSettings.sensitivityWheelMouse);
                settings.generalSettings.showHelmet = PlayerPrefs.GetInt("showHelm") == 1;//, settings.generalSettings.showHelmet == true ? 1 : 0);
                settings.generalSettings.saveCredential = PlayerPrefs.GetInt("savec") == 1;//, settings.generalSettings.saveCredential == true ? 1 : 0);
                //Quests
                // settings.questListSelected = (PlayerPrefs.GetString("qs").Split(';').Select(n => Convert.ToInt32(n)).ToArray()).OfType<long>().ToList();
                settings.generalSettings.autoLootGroundMinQuality = PlayerPrefs.GetInt("alq");                                                                                           
                Dictionary<long, List<long>> qs = new Dictionary<long, List<long>>();
//                settings.questListSelected = (PlayerPrefs.GetString("qs").Split(';').Select(n => Convert.ToInt32(n)).ToArray()).OfType<long>().ToList();
                settings.questListSelected = PlayerPrefs.GetString("qs").Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(part => part.Split('='))
               .ToDictionary(s => (long)Convert.ToInt32(s[0]), s => (s[1].Split(';').Select(n => Convert.ToInt32(n)).ToArray()).OfType<long>().ToList());
                ;
                //Keys
                settings.keySettings.strafeLeft.key = (KeyCode)PlayerPrefs.GetInt("ksl");
                settings.keySettings.strafeLeft.altKey = (KeyCode)PlayerPrefs.GetInt("aksl");
                settings.keySettings.strafeRight.key = (KeyCode)PlayerPrefs.GetInt("ksr");
                settings.keySettings.strafeRight.altKey = (KeyCode)PlayerPrefs.GetInt("aksr");
                settings.keySettings.moveForward.key = (KeyCode)PlayerPrefs.GetInt("kmf");
                settings.keySettings.moveForward.altKey = (KeyCode)PlayerPrefs.GetInt("akmf");
                settings.keySettings.moveBackward.key = (KeyCode)PlayerPrefs.GetInt("kmb");
                settings.keySettings.moveBackward.altKey = (KeyCode)PlayerPrefs.GetInt("akmb");
                settings.keySettings.turnLeft.key = (KeyCode)PlayerPrefs.GetInt("ktl");
                settings.keySettings.turnLeft.altKey = (KeyCode)PlayerPrefs.GetInt("aktl");
                settings.keySettings.turnRight.key = (KeyCode)PlayerPrefs.GetInt("ktr");
                settings.keySettings.turnRight.altKey = (KeyCode)PlayerPrefs.GetInt("aktr");
                settings.keySettings.autoRun.key = (KeyCode)PlayerPrefs.GetInt("kar");
                settings.keySettings.autoRun.altKey = (KeyCode)PlayerPrefs.GetInt("akar");
                settings.keySettings.walkRun.key = (KeyCode)PlayerPrefs.GetInt("kwr");
                settings.keySettings.walkRun.altKey = (KeyCode)PlayerPrefs.GetInt("akwr");
                settings.keySettings.jump.key = (KeyCode)PlayerPrefs.GetInt("kju");
                settings.keySettings.jump.altKey = (KeyCode)PlayerPrefs.GetInt("akju");
                settings.keySettings.showHideWeapon.key = (KeyCode)PlayerPrefs.GetInt("ksw");
                settings.keySettings.showHideWeapon.altKey = (KeyCode)PlayerPrefs.GetInt("aksw");
                settings.keySettings.dodge.key = (KeyCode)PlayerPrefs.GetInt("kdo");
                settings.keySettings.dodge.altKey = (KeyCode)PlayerPrefs.GetInt("akdo");
                settings.keySettings.sprint.key = (KeyCode)PlayerPrefs.GetInt("ksp");
                settings.keySettings.sprint.altKey = (KeyCode)PlayerPrefs.GetInt("aksp");
                settings.keySettings.loot.key = (KeyCode)PlayerPrefs.GetInt("klo");
                settings.keySettings.loot.altKey = (KeyCode)PlayerPrefs.GetInt("aklo");

                settings.keySettings.inventory.key = (KeyCode)PlayerPrefs.GetInt("kinv");
                settings.keySettings.inventory.altKey = (KeyCode)PlayerPrefs.GetInt("akinv");
                settings.keySettings.character.key = (KeyCode)PlayerPrefs.GetInt("kchar");
                settings.keySettings.character.altKey = (KeyCode)PlayerPrefs.GetInt("akchar");
                settings.keySettings.mail.key = (KeyCode)PlayerPrefs.GetInt("kmail");
                settings.keySettings.mail.altKey = (KeyCode)PlayerPrefs.GetInt("akmail");
                settings.keySettings.guild.key = (KeyCode)PlayerPrefs.GetInt("kquild");
                settings.keySettings.guild.altKey = (KeyCode)PlayerPrefs.GetInt("akquild");
                settings.keySettings.quest.key = (KeyCode)PlayerPrefs.GetInt("kquest");
                settings.keySettings.quest.altKey = (KeyCode)PlayerPrefs.GetInt("akquest");
                settings.keySettings.skills.key = (KeyCode)PlayerPrefs.GetInt("kskill");
                settings.keySettings.skills.altKey = (KeyCode)PlayerPrefs.GetInt("akskill");
                settings.keySettings.map.key = (KeyCode)PlayerPrefs.GetInt("kmap");
                settings.keySettings.map.altKey = (KeyCode)PlayerPrefs.GetInt("akmap");
                settings.keySettings.arena.key = (KeyCode)PlayerPrefs.GetInt("karena");
                settings.keySettings.arena.altKey = (KeyCode)PlayerPrefs.GetInt("akarena");
                settings.keySettings.social.key = (KeyCode)PlayerPrefs.GetInt("ksoc");
                settings.keySettings.social.altKey = (KeyCode)PlayerPrefs.GetInt("aksoc");
                settings.keySettings.dodgeDoubleTap = PlayerPrefs.GetInt("ddt")==1;
            }
            //  Debug.LogError("Load settings apply");
            applySettings();
            //    Debug.LogError("Load settings seng message");

            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("SETTINGS_LOADED", args);
            //   Debug.LogError("Load settings send message " +settings.credential.p+" "+settings.credential.l+ "  "+settings.generalSettings.saveCredential);
        }

        private void applySettings()
        {
            //Applay Audio Settings
            if (masterMixer != null)
            {
                masterMixer.SetFloat("masterVol", settings.audioSettings.masterLevel);
                masterMixer.SetFloat("musicVol", settings.audioSettings.musicLevel);
                masterMixer.SetFloat("sfxVol", settings.audioSettings.sfxLevel);
                masterMixer.SetFloat("uiVol", settings.audioSettings.uiLevel);
                masterMixer.SetFloat("AmbientVol", settings.audioSettings.ambientLevel);
                masterMixer.SetFloat("FootstepsVol", settings.audioSettings.footstepsLevel);
            }

            //Applay Video Settings
            if (!mainCamera)
                mainCamera = GameObject.Find("MainCamera");
            if (!effectCamera)
                effectCamera = GameObject.Find("EffectCamera");
            //      Screen.SetResolution(gSettings.videoSettings.resolutionWidth, gSettings.videoSettings.resolutionHeight, gSettings.videoSettings.fullscreen);
            QualitySettings.SetQualityLevel(settings.videoSettings.quality, false);
            if (settings.videoSettings.customSettings)
            {
                switch (settings.videoSettings.shadows)
                {
                    case 0:
                        QualitySettings.shadows = ShadowQuality.Disable;
                        break;
                    case 1:
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                        break;
                    case 2:
                        QualitySettings.shadows = ShadowQuality.All;
                        break;
                }
                //   QualitySettings.shadowDistance = settings.videoSettings.shadowDistance;
                switch (settings.videoSettings.shadowDistance)
                {
                    case 0:
                        QualitySettings.shadowDistance = 50;
                        break;
                    case 1:
                        QualitySettings.shadowDistance = 100;
                        break;
                    case 2:
                        QualitySettings.shadowDistance = 150;
                        break;
                    case 3:
                        QualitySettings.shadowDistance = 300;
                        break;
                    case 4:
                        QualitySettings.shadowDistance = 500;
                        break;
                }

                switch (settings.videoSettings.shadowResolution)
                {
                    case 0:
                        QualitySettings.shadowResolution = ShadowResolution.Low;
                        break;
                    case 1:
                        QualitySettings.shadowResolution = ShadowResolution.Medium;
                        break;
                    case 2:
                        QualitySettings.shadowResolution = ShadowResolution.High;
                        break;
                    case 3:
                        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                        break;
                }
                QualitySettings.vSyncCount = settings.videoSettings.verticalSync;
                //  QualitySettings.lodBias = settings.videoSettings.lodBias;
                switch (settings.videoSettings.lodBias)
                {
                    case 0:
                        QualitySettings.lodBias = 0.3f;
                        break;
                    case 1:
                        QualitySettings.lodBias = 0.4f;
                        break;
                    case 2:
                        QualitySettings.lodBias = 0.7f;
                        break;
                    case 3:
                        QualitySettings.lodBias = 1f;
                        break;
                    case 4:
                        QualitySettings.lodBias = 1.5f;
                        break;
                    case 5:
                        QualitySettings.lodBias = 2f;
                        break;
                }     //   QualitySettings.particleRaycastBudget = settings.videoSettings.particleRaycastBudget;

                switch (settings.videoSettings.shadowResolution)
                {
                    case 0:
                        QualitySettings.particleRaycastBudget = 4;
                        break;
                    case 1:
                        QualitySettings.particleRaycastBudget = 16;
                        break;
                    case 2:
                        QualitySettings.particleRaycastBudget = 64;
                        break;
                    case 3:
                        QualitySettings.particleRaycastBudget = 256;
                        break;
                    case 4:
                        QualitySettings.particleRaycastBudget = 1024;
                        break;
                    case 5:
                        QualitySettings.particleRaycastBudget = 2048;
                        break;
                }
                QualitySettings.globalTextureMipmapLimit = 3 - settings.videoSettings.masterTextureLimit;
                QualitySettings.softParticles = settings.videoSettings.softParticles;
            }
            ApplyCamEffect();
            if (settings.keySettings.additionalActions == null)
            {
                settings.keySettings.additionalActions = new List<AtavismKeyDefinition>();
            }

            if (settings.keySettings.action == null)
            {
                settings.keySettings.action = new AtavismKeyDefinition();
            }
        }

        public void ApplyCamEffect()
        {
#if AT_PPS2_PRESET
        if (postProcessVolumes.Count > 0) {
            foreach(PostProcessVolume ppv in postProcessVolumes) {
                if(ppv ==null)
                    continue;
                foreach (PostProcessEffectSettings ppes in ppv.sharedProfile.settings){
                    if (ppes.name.Equals("Bloom")) {
                        ppes.enabled.Override(settings.videoSettings.bloom);
                    }
                    if (ppes.name.Equals("DepthOfField")) {
                        ppes.enabled.Override(settings.videoSettings.depthOfField);
                    }
                    if (ppes.name.Equals("Vignette")) {
                        ppes.enabled.Override(settings.videoSettings.vignette);
                    }
                    if (ppes.name.Equals("AmbientOcclusion")) {
                        ppes.enabled.Override(settings.videoSettings.ambientOcclusion);
                    }
                    if (ppes.name.Equals("AutoExposure")) {
                        ppes.enabled.Override(settings.videoSettings.autoExposure);
                    }
                    if (ppes.name.Equals("ChromaticAberration")) {
                        ppes.enabled.Override(settings.videoSettings.chromaticAberration);
                    }
                    if (ppes.name.Equals("ColorGrading")) {
                        ppes.enabled.Override(settings.videoSettings.colorGrading);
                    }
                    if (ppes.name.Equals("Dithering")) {
                        ppes.enabled.Override(settings.videoSettings.dithering);
                    }
                    if (ppes.name.Equals("MotionBlur")) {
                        ppes.enabled.Override(settings.videoSettings.motionBlur);
                    }
                    if (ppes.name.Equals("ScreenSpaceReflections")) {
                        ppes.enabled.Override(settings.videoSettings.screenSpaceReflections);
                    }
                }
            }
        }

        Camera cam = Camera.main;
        if (cam != null) {
            PostProcessLayer ppl = cam.GetComponent<PostProcessLayer>();
            if (ppl != null) {
                    switch (settings.videoSettings.antialiasing) {
                        case 0:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            break;
                        case 1:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                            break;
                        case 2:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                            break;
                        case 3:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                            break;
                    }
            }
/*
            SEScreenSpaceShadows sesss = cam.GetComponent<SEScreenSpaceShadows>();
            if (sesss != null) {
                sesss.enabled = settings.videoSettings.seScreenSpaceShadows;
            }

            VolumetricFog vf = cam.GetComponent<VolumetricFog>();
            if (vf != null) {
                vf.enabled = settings.videoSettings.volumetricFog;
            }
            HxVolumetricCamera hvc = cam.GetComponent<HxVolumetricCamera>();
            if (hvc != null) {
                hvc.enabled = settings.videoSettings.hxVolumetricCamera;
            }
            HxVolumetricImageEffectOpaque hvieo = cam.GetComponent<HxVolumetricImageEffectOpaque>();
            if (hvieo != null) {
                hvieo.enabled = settings.videoSettings.hxVolumetricCamera;
            }*/
        }
#else
            //Debug.Log("");
#endif


#if AT_I2LOC_PRESET
        //  QualitySettings.shadowQuality=
        if (settings.generalSettings.language != null)
            I2.Loc.LocalizationManager.CurrentLanguage = settings.generalSettings.language;
#endif

            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
            if (terrains != null)
                for (int i = 0; i < terrains.Length; i++)
                {
                    terrains[i].detailObjectDensity = settings.videoSettings.detailObjectDensity;
                }
        }
        //  void
        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            Save();
        }
        void OnDestroy()
        {
            // AtavismEventSystem.UnregisterEvent("OPEN_WINDOW", this);
            // AtavismEventSystem.UnregisterEvent("CLOSE_WINDOW", this);
            Save();
        }



        void OnApplicationFocus(bool state)
        {
            //  gameFocus = state;
            if (masterMixer)
            {
                if (state)
                {
                    mixerFocus.TransitionTo(.1f);
                    // masterMixer.FindSnapshot("Fokus").TransitionTo(.01f);
                }
                else
                {
                    mixerNoFocus.TransitionTo(.2f);
                    //masterMixer.FindSnapshot("noFokus").TransitionTo(.01f);
                }
            }

            if (state)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
#if !AT_MOBILE
            if (state)
            {
                if (Camera.main != null)
                {
                    Atavism3rdPersonInput a3pi = Camera.main.transform.GetComponent<Atavism3rdPersonInput>();
                    if (a3pi != null)
                    {
                        if (a3pi.mouseLookLocked)
                        {
                            if (openedWindow.Count == 0 && !isMenuBarOpened)
                            {
                                Cursor.visible = false;
                                Cursor.lockState = CursorLockMode.None;
                                Cursor.lockState = CursorLockMode.Locked;
                            }
                            else
                            {
                                Cursor.visible = true;
                                Cursor.lockState = CursorLockMode.None;
                                Cursor.lockState = CursorLockMode.Confined;
                            }
                        }
                    }
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Confined;
                }
            }
            else
            { 
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
#endif    
        }
//Windows Methode
        public bool isWindowOpened()
        {
            return openedWindow.Count > 0;
        }
        
        public void ResetWindows()
        {
            settings.windowsSettings.Clear();
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("WINDOWS_RESET", args);
        }

        public Vector3 GetWindowPosition(string winName)
        {
            if (settings.windowsSettings.ContainsKey(winName))
            {
                var v = new Vector3(settings.windowsSettings[winName].x, settings.windowsSettings[winName].y, settings.windowsSettings[winName].z);
               // Debug.LogError("GetWindowPosition ? "+winName+" v="+v);
                return v;
            }
            return Vector3.zero;
        }

        public void SetWindowPosition(string winName, Vector3 winPosition)
        {
           // Debug.LogError("SetWindowPosition ? "+winName+" v="+winPosition);
            if (settings.windowsSettings.ContainsKey(winName))
            {
                settings.windowsSettings[winName].x = winPosition.x;
                settings.windowsSettings[winName].y = winPosition.y;
                settings.windowsSettings[winName].z = winPosition.z;
            }
            else
            {
                AtavismWindowsSettings winSet = new AtavismWindowsSettings();
                winSet.windowName = winName;
                winSet.x = winPosition.x;
                winSet.y = winPosition.y;
                winSet.z = winPosition.z;
                settings.windowsSettings.Add(winName, winSet);
            }
        }

        public List<VisualElement> getAllElement(Vector2 point)
        {
            List<VisualElement> list = new List<VisualElement>();
            foreach (var window in openedWindow)
            {
                if (window is UIAtavismWindowBase)
                {
                    List<VisualElement> sublist = new List<VisualElement>();
                    UIAtavismWindowBase uawb = (UIAtavismWindowBase)window;
                    uawb.uiDocument.rootVisualElement.panel.PickAll(point, sublist);
                    list.AddRange(sublist);
                }
            }

            return list;
        }

        
        public void OpenWindow(MonoBehaviour window )
        {
            AtavismLogger.LogDebugMessage("OpenWindow "+window.name);
            openedWindow.Add(window);
#if !AT_MOBILE              
            AtavismLogger.LogDebugMessage("CloseWindow Show cursor");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
#endif 
        }
        public void CloseWindow(MonoBehaviour window)
        {
            AtavismLogger.LogDebugMessage("CloseWindow "+window.name);
            openedWindow.Remove(window);
#if !AT_MOBILE
            if (openedWindow.Count == 0 && !isMenuBarOpened)
            {
                AtavismLogger.LogDebugMessage("CloseWindow Hide cursor");
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.lockState = CursorLockMode.Locked;
            }
#endif             
        }

        public void OpenUIToolkitWindow(UIAtavismWindowBase window)
        {
            openedUIToolkitWindow.Add(window);
        } 
        public void CloseUIToolkitWindow(UIAtavismWindowBase window)
        {
            openedUIToolkitWindow.Remove(window);
        } 
        
        public void OpenUIToolkitWindowEscKey(UIAtavismWindowBase window)
        {
            openedUIToolkitWindowEscKey.Add(window);
        } 
        public void CloseUIToolkitWindowEscKey(UIAtavismWindowBase window)
        {
            openedUIToolkitWindowEscKey.Remove(window);
        }

        public bool IsAnyOpenedWindowEscKey => openedUIToolkitWindowEscKey.Count > 0;


        public bool isMenuBarOpened
        {
            set
            {
                _isMenuBarOpened = value;
#if !AT_MOBILE
                if (openedWindow.Count == 0 && !isMenuBarOpened)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.lockState = CursorLockMode.Confined;
                    ClientAPI.mouseLook = false;
                }
#endif        
            }
            get
            {
                return _isMenuBarOpened;
            }
        }
        
        public static AtavismSettings Instance
        {
            get
            {
                return instance;
            }
        }

        public static bool UIHasFocus()
        {
            if (EventSystem.current.currentSelectedGameObject != null
                && (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null))
                return true;
            return false;
        }

          void LoadLoader() {
            if (sceneLoaderPrefab != null)
            {
                GameObject goLoader = GameObject.Find("/"+ sceneLoaderPrefab.name);
                if (goLoader == null)
                    goLoader = Instantiate(sceneLoaderPrefab);
              //  goLoader.name = "CanvasLoader";
                goLoader.transform.SetAsLastSibling();
            }
        }
          
        void LoadUmaWardrobe()
        {
            GameObject goUma = GameObject.Find("/UMA");
            GameObject goPrzeb = GameObject.Find("/Wardrobe");
            if (UMAGameObject != null)
                if (goUma == null)
                    goUma = Instantiate(UMAGameObject);
            if (wardrobeGameObject != null)
                if (goPrzeb == null)
                    goPrzeb = Instantiate(wardrobeGameObject);

            if (goUma != null)
            {
                goUma.name = "UMA";
                DontDestroyOnLoad(goUma);
            }
            if (goPrzeb != null)
            {
                goPrzeb.name = "Wardrobe";
                DontDestroyOnLoad(goPrzeb);
                if (characterPanelCamera == null)
                {
                    Camera[] cams = goPrzeb.GetComponentsInChildren<Camera>(true);
                    foreach (Camera c in cams)
                    {
                        if (c.name == "CameraCharacterPanel")
                        {
                            characterPanelCamera = c;
                            c.enabled = false;
                        }
                        else if (c.name == "PetCameraCharacterPanel")
                        {
                            petPanelCamera = c;
                            c.enabled = false;
                        }
                        else
                        {
                            otherCharacterPanelCamera = c;
                            c.enabled = false;
                        }
                    }
                    CapsuleCollider[] ccol = goPrzeb.GetComponentsInChildren<CapsuleCollider>(true);
                    foreach (CapsuleCollider c in ccol)
                    {
                        if (c.name == "CharacterCapsulePoint")
                        {
                            characterPanelSpawn = c.transform;
                        }
                        else if (c.name == "PetCharacterCapsulePoint")
                        {
                            petPanelSpawn = c.transform;
                        }
                        else
                        {
                            otherCharacterPanelSpawn = c.transform;
                        }
                    }
                }
            }
        }



        public void ClickCreateUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(createURL_EN);
            } else {
                Application.OpenURL(createURL_PL);
            }
#else
            Application.OpenURL(createURL_EN);
#endif
        }
        public void ClickForgotUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(forgotURL_EN);
            } else {
                Application.OpenURL(forgotURL_PL);
            }
#else
            Application.OpenURL(forgotURL_EN);
#endif
        }

        public void ClickWebPageUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(webURL_EN);
            } else {
                Application.OpenURL(webURL_PL);
            }
#else
            Application.OpenURL(webURL_EN);
#endif
        }
        public void ClickShopWebPageUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(webURL_EN);
            } else {
                Application.OpenURL(webURL_PL);
            }
#else
            Application.OpenURL(webURL_EN);
#endif
        }


        public Dictionary<long, List<long>> GetQuestListSelected()
        {
            if (settings.questListSelected == null)
                settings.questListSelected = new Dictionary<long, List<long>>();
            return settings.questListSelected;
        }
        public AtCred GetCredentials()
        {
            return settings.credential;
        }

        public AtavismGeneralSettings GetGeneralSettings()
        {
            return settings.generalSettings;
        }
        public AtavismAudioSettings GetAudioSettings()
        {
            return settings.audioSettings;
        }
        public AtavismVideoSettings GetVideoSettings()
        {
            return settings.videoSettings;
        }

        public AtavismKeySettings GetKeySettings()
        {
            return settings.keySettings;
        }
        public Camera GetCharacterPanelCamera()
        {
            return characterPanelCamera;
        }
        public Camera GetOtherCharacterPanelCamera()
        {
            return otherCharacterPanelCamera;
        }
        public Camera GetPetPanelCamera()
        {
            return petPanelCamera;
        }
        public Transform GetCharacterPanelSpawn()
        {
            return characterPanelSpawn;
        }
        public Transform GetOtherCharacterPanelSpawn()
        {
            return otherCharacterPanelSpawn;
        }
        public Transform GetPetPanelSpawn()
        {
            return petPanelSpawn;
        }

        public int GetQuestPrevLimit
        {
            get
            {
                return questPrevLimit;
            }
        }
        private UIAtavismMiniMap _uiminimap = null;

        public UIAtavismMiniMap UiMiniMap
        {
            set {
                _uiminimap = value;
            }
            get {
                return _uiminimap;
            }
        }

            private bl_MiniMap _minimap = null;

            public bl_MiniMap MiniMap {
                set {
                    _minimap = value;
                }
                get {
                    return _minimap;
                }
            }
            private bl_MaskHelper _maskHelper = null;

            public bl_MaskHelper MaskHelper {
                set { _maskHelper = value; }
                get { return _maskHelper; }
            }
        
        public List<string> GameInstances
        {
            get
            {
                return gameInstances;
            }
        }
        public List<string> ArenaInstances
        {
            get
            {
                return arenaInstances;
            }
        }
        public List<string> DungeonInstances
        {
            get
            {
                return dungeonInstances;
            }
        }

           public DsMiniMapSettings MinimapSettings {
               get { return minimapSettings; }
           }
        public GameObject LevelUpPrefab
        {
            get
            {
                return levelUpPrefab;
            }
        }
        public float LevelUpPrefabDuration
        {
            get
            {
                return levelUpPrefabDuration;
            }
        }
        public AtavismQualitySetingsDefault GetDefaultQuality(int id)
        {
            return _defaultQualitySettings[id];
        }

        public bool NameVisable
        {
            get
            {
                return visibleMobsName;
            }
        }
        public bool VisableOid
        {
            get
            {
                return visibleOid;
            }
        }
        public bool ShowLevelWithName
        {
            get
            {
                return showLevelWithName;
            }
        }
        
        public bool VisibleShopMessageOnSelf
        {
            get
            {
                return visibleShopMessageOnSelf;
            }
        }
        public TMP_FontAsset MobNameFont
        {
            get
            {
                return mobNameFont;
            }
        }
        public Vector3 MobNamePosition
        {
            get
            {
                return mobNamePosition;
            }
        }
        public int MobNameFontSize
        {
            get
            {
                return mobNameFontSize;
            }
        }
        public Color MobNameDefaultColor
        {
            get
            {
                return mobNameDefaultColor;
            }
        }
        public TextAlignmentOptions MobNameAlignment
        {
            get
            {
                return mobNameAlignment;
            }
        }
        public Vector4 MobNameMargin
        {
            get
            {
                return mobNameMargin;
            }
        }
        public float MobNameOutlineWidth
        {
            get
            {
                return mobNameOutlineWidth;
            }
        }
         public string QuestNewText
        {
            get
            {
                return questNewText;
            }
        }
        public string QuestConcludableText
        {
            get
            {
                return questConcludableText;
            }
        }
        public string QuestProgressText
        {
            get
            {
                return questProgressText;
            }
        }
        public string ShopText
        {
            get
            {
                return shopText;
            }
        }
        public string BankText
        {
            get
            {
                return bankText;
            }
        }
        public TMP_SpriteAsset GetSpriteAsset
        {
            get
            {
                return npcInfoSpriteAsset;
            }
        }
        public int GetNpcInfoTextSize
        {
            get
            {
                return npcInfoTextSize;
            }
        }
        public Color GetNpcInfoTextColor
        {
            get
            {
                return npcInfoTextColor;
            }
        }
        public Vector3 GetNpcInfoTextPosition
        {
            get
            {
                return npcInfoTextPosition;
            }
        }
        
       GameObject _contextMenu;
        /// <summary>
        /// Przechowuje otwarte menu kontextowe wcelu zamkniecia w przypadku otwarciz innego
        /// </summary>
        /// <param name="obj"></param>
        public void DsContextMenu(GameObject obj)
        {
            if (obj != null)
            {
                if (_contextMenu != null && _contextMenu != obj)
                    _contextMenu.SetActive(false);
                _contextMenu = obj;
            }
            else
            {
                if (_contextMenu != null)
                {
                    _contextMenu.SetActive(false);
                    _contextMenu = null;
                }
            }
        }
        
        public DsAdminPanelSettings GetAdminLocations()
        {
            foreach (DsAdminPanelSettings s in _adminLocationSettings)
            {
                if (s.InstanceName == SceneManager.GetActiveScene().name)
                    return s;
            }
            return null;
        }

        public List<DsAdminRestart> GetAdminRestarts
        {
            get
            {
                return _adminRestartSettings;
            }
        }
        
        
        public Color ItemQualityColor(int index)
        {
            index -= 1;
            return itemQualityColor[index < 0 ? 0 : index >= itemQualityColor.Count ? itemQualityColor.Count - 1 : index];
            if (itemQualityColor.Count - 1 < index || index == 0) 
            {
                //    Debug.LogError("Add Color to Item Quality Color in AtavismSettings");
                return Color.white;
            }

            return itemQualityColor[index-1];

        }
        public GameObject CharacterAvatar
        {
            get
            {
                return characterAvatar;
            }
            set
            {
                characterAvatar = value;
            }
        }
        public GameObject OtherCharacterAvatar
        {
            get
            {
                return otherCharacterAvatar;
            }
            set
            {
                otherCharacterAvatar = value;
            }
        }
        public GameObject PetAvatar
        {
            get
            {
                return petAvatar;
            }
            set
            {
                petAvatar = value;
            }
        }
        public Sprite[] Avatars(string Race,string Gender, string Class)
        {
            foreach(RaceAvatar ra in races)
            {
                if (ra.name.Equals(Race))
                {
                    foreach (GenderAvatar ga in ra.genders)
                    {
                        if (ga.name.Equals(Gender))
                        {
                            foreach (ClassAvatar ca in ga.classes)
                            {
                                if (ca.name.Equals(Class))
                                {
                                    return ca.avatars;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public Sprite Avatar(string path)
        {
            foreach (RaceAvatar ra in races)
            {
                foreach (GenderAvatar ga in ra.genders)
                {
                    foreach (ClassAvatar ca in ga.classes)
                    {
                        foreach (Sprite s in ca.avatars)
                        {
                            if(s!=null)
                            if (s.name.Equals(path))
                            {
                                return s;
                            }
                        }
                    }
                }
            }
            return null;
        }
        private UIAtavismWindowBase lastWindow;

        public UIAtavismWindowBase WindowOnTop
        {
            get
            {
                return lastWindow;
            }
            set
            {
                if (lastWindow != null)
                {
                    if (lastWindow.IsVisible)
                    {
                        if (lastWindow != value)
                            lastWindow.uiDocument.sortingOrder = 49;
                    }

                    int orderId = 48;
                    if(openedUIToolkitWindow.Count>0)
                    for (int i = openedUIToolkitWindow.Count - 1; i >= 0; i--)
                    {
                        var window = openedUIToolkitWindow[i];
                        if (window!=null && window != lastWindow && window != value)
                            if (orderId > 20)
                            {
                                if(window.uiDocument!=null)
                                window.uiDocument.sortingOrder = orderId--;
                            }
                            else
                            {
                                if(window.uiDocument!=null)
                                window.uiDocument.sortingOrder = orderId;
                            }

                    }
                }
                lastWindow = value;
            }
        }

        public void ShowUIDropdown(UIDropdown dropdown)
        {
            if(openedUIDropdown!=null &&  openedUIDropdown != dropdown)
                openedUIDropdown.HidePopup();
            openedUIDropdown = dropdown;
        } 

#if AT_PPS2_PRESET
    public List<PostProcessVolume> PostProcessVolumes
    {
        set
        {
            postProcessVolumes = value;
            ApplyCamEffect();
        }
        get
        {
            return postProcessVolumes;
        }
    }
#endif
    }
}