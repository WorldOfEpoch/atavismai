using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Wind;
#if UNITY_EDITOR
using AwesomeTechnologies.Vegetation.Masks;
using UnityEditor;
#endif
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
#if USING_HDRP && UNITY_2023_1_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace AwesomeTechnologies.VegetationSystem
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Systems/VegetationSystemPro", 0)]
    [ScriptExecutionOrder(100)]
    [ExecuteInEditMode]
    public partial class VegetationSystemPro : MonoBehaviour
    {   // create cell storage
        [NonSerialized] public QuadTree<VegetationCell> vegetationCellQuadTree = new(new Rect());   // "storage" for querying and filling lists based on area comparisons for vegetation cells
        [NonSerialized] public QuadTree<BillboardCell> billboardCellQuadTree = new(new Rect()); // "storage" for querying and filling lists based on area comparisons for billboard cells
        [NonSerialized] public readonly List<VegetationCell> vegetationCellList = new();    // all created (unloaded) vegetation cells
        [NonSerialized] public readonly List<BillboardCell> billboardCellList = new();  // all created (unloaded) billboard cells

        [NonSerialized] public readonly List<VegetationCell> loadedVegetationCellList = new();  // stores all loaded vegetation cells
        [NonSerialized] private readonly List<BillboardCell> loadedBillboardCellList = new();   // stores all loaded billboard cells (temporarily while loading them => billboard cells get cleared right after, only keep the resulting mesh)

        [NonSerialized] private readonly List<VegetationCell> predictiveVegetationCellLoaderList = new();   // stores additional vegetation cells for preloading and offloading to more workers per frame
        [NonSerialized] private readonly List<BillboardCell> predictiveBillboardCellLoaderList = new(); // stores additional billboard cells for preloading and offloading to more workers per frame
        [NonSerialized] private readonly List<VegetationCell> instancedIndirectCellList = new();    // stores loaded vegetation cells that need an additional setup for compute shaders
        [NonSerialized] public readonly List<VegetationCell> compactMemoryCellList = new(); // stores vegetation cells that get their temporary rule data cleared after they got loaded

        // create base cell system data/logic
        public float systemRelativeSeaLevel = 0;    // set per frame using the engine's "Update" loop -- used for the "VegetationCellSpawner" -- height rules -- cell size sampling
        public PredictiveCellLoader predictiveCellLoader;   //  stores data and logic for the "position based pre-loading"
        public VegetationCellSpawner vegetationCellSpawner; // the spawner for cells => vegetation item rules ==> their matrices
        public PersistentVegetationStorage persistentVegetationStorage; // persistent storage component on the gameObject -- data of it gets loaded in the "vegetationCellSpawner"
        private JobHandle prepareVegetationHandle;  // batched default job handle -- main handle for vegetation cell loading > CPU render list merge > cull/lods job
        private JobHandle prepareBillboardsHandle;  // batched default job handle -- main handle for billboard cell loading > billboard mesh data generation > billboard mesh creation
        [NonSerialized] public bool isSetupDone;    // "safety" toggle/limiter

        // create other needed lists
        public List<VegetationPackagePro> vegetationPackageProList = new();
        public List<VegetationPackageProModelInfo> vegetationPackageProModelsList = new();
        [NonSerialized] public Bounds cellCullingBoundsAddy;    // cell culling additional bounds -- biggest possible value of all items
        [NonSerialized] public bool shouldForceUpdateCellCulling = true;    // whether to force updating cell culling -- whether the scale of an item changed thus possibly the "boundsAddy"
        public List<VegetationStudioCamera> vegetationStudioCameraList = new();

        public List<IVegetationStudioTerrain> vegetationStudioTerrainList = new();
        public List<GameObject> vegetationStudioTerrainObjectList = new();

        [NonSerialized] private readonly List<IWindController> windControllerList = new();
        public List<WindControllerSettings> windControllerSettingsList = new();

        // create separate UI tab objects => for internal non-UI use
        public VegetationSettings vegetationSettings = new();
        public VegetationRenderSettings vegetationRenderSettings = new();
        public EnvironmentSettings environmentSettings = new();

        #region inspector ui tab data => for internal non-UI use
        // settings tab
        public float SeaLevel;  // absolute sea level => for relative use "systemRelativeSeaLevel"
        public bool excludeSeaLevelCells = true;
        public float vegetationCellSize = 128;
        public float billboardCellSize = 1024;

        public bool enableAutoSystemRefresh = true;
        public Transform floatingOriginAnchor;
        public float3 floatingOriginOffset;
        private float3 floatingOriginStart;
        public bool loadPredictiveCells = true;
        public int predictiveCellsPerFrame = 1;
        public bool togglePredictiveGrass;
        public bool togglePredictivePlants;
        public bool togglePredictiveObjects;
        public bool togglePredictiveLargeObjects = true;
        public bool togglePredictiveTrees = true;
        public bool togglePredictiveBillboards = true;
        public bool useCompactCache = true;
        public bool toggleCompactCacheGrass = true;
        public bool toggleCompactCachePlants = true;
        public bool toggleCompactCacheObjects = true;
        public bool toggleCompactCacheLargeObjects = true;
        public bool toggleCompactCacheTrees = true;
        public bool toggleCompactCacheBillboards = true;

        // rendering tab
        public Light sunDirectionalLight;
        public bool hasSunMoon;
        private float3 sunMoonDirection;
        private int lastEngineLODMax;

        // cameras tab
        public bool useCameraAutoSelection = true;

        // terrains tab
        public Bounds vegetationSystemBounds;   // "total area" -- has the issue of using "terrainData.bounds" which can create "false bottoms" => false "systemRelativeSeaLevel" values ..since even the engine handles it wrong itself kinda
        public bool automaticBoundsCalculation = true;
        public float3 VegetationSystemPosition
        {
            get
            {
                float3 position = vegetationSystemBounds.center - vegetationSystemBounds.extents;
                position.y = 0;
                return position;
            }
        }

        // weather tab
        public WindZone selectedWindZone;
        public float windSpeedFactor = 1;

        #endregion

        #region inspector ui tab data => for external UI-only use
#if UNITY_EDITOR
        // inspector selection fields
        public int selectedTabIndex;
        public int selectedBiomeTabIndex = -1;
        public int selectedVegetationPackageIndex;

        // settings tab
        public int selectedSettingsSubTabIndex;
        public bool showSeaLevelMenu;
        public bool showCellSizeMenu;
        public bool showGlobalDensityMenu;

        public bool showSystemBehaviorMenu;
        public bool showTerrainStreamingMenu;
        public bool showPredictiveCellMenu;
        public bool showCompactCacheMenu;

        // rendering tab
        public bool showGlobalDistanceMenu;
        public bool showRenderShadowMenu;
        public bool showRenderLayerMenu;
        public bool showProbeMenu;
        public bool showRenderPlatformMenu;

        // terrains tab
        public bool showTerrainBatchMenu;
        public bool showTerrainListMenu;
        public bool showTerrainAreaMenu;

        // weather tab
        public bool showWeatherSnowMenu;
        public bool showWeatherRainMenu;
        public bool showWeatherWindMenu;
        public bool showWeatherCustomWindMenu;

        // edit biome / vegetation item tab
        [NonSerialized] public VegetationItemInfoPro vegetationItemInfoProEditor;
        public int selectedVegetationTypeIndex; // sub-tab type category
        public int selectedGridIndex;   // grid "icon"
        public int selectedVegetationItemSubTab;    // sub-tab settings/rules
        public bool showAddVegetationItemMenu;
        public bool showSelectedVegetationItemMenu;
        public bool showVegetationItemSettingsMenu;
        public bool showPositionRotationScaleMenu;
        public bool showHeightSteepnessMenu;
        public bool showVegetationPackageNoiseMenu;
        public bool showLODMenu;
        public bool showDistanceFalloffMenu;
        public bool showShaderSettingsMenu;
        public bool showShaderSettingsMaterials;
        public bool showShaderSettingsControllers;
        public bool showBillboardsMenu;
        public bool showColliderRulesMenu;
        public bool showTextureMaskRulesMenu;
        public bool showBiomeRulesMenu;
        public bool showVegetationMaskRulesMenu;
        public bool showTerrainTextureRulesMenu;
        public bool showConcaveLocationRulesMenu;
        public bool showTerrainSourceSettingsMenu;

        // texture masks tab
        public int selectedTextureMaskGroupIndex;
        public int selectedTextureMaskGroupTextureIndex;

        // batch edit tab
        // general
        public bool showGeneralBatchMenu;
        public bool toggleTypeFilterSelection;
        public VegetationType overrideSelectionType;
        public bool toggleRuntimeState;
        public VegetationRenderMode overrideRenderMode;
        // pos rot scale
        public bool toggleRandomizePosition;
        public Vector3 overridePositionOffset;
        public float overrideMinUpOffsetRange;
        public float overrideMaxUpOffsetRange;
        public bool showPositionRotationScaleBatchMenu;
        public VegetationRotationType overrideRotationMode;
        public float overrideMinScale;
        public float overrideMaxScale;
        // height steepness
        public bool showHeightSteepnessBatchMenu;
        public bool toggleHeightRule;
        public float overrideMinHeight;
        public float overrideMaxHeight;
        public bool toggleSteepnessRule;
        public float overrideMinSteepness;
        public float overrideMaxSteepness;
        // lod
        public bool showLODBatchMenu;
        public bool toggleLODCrossfade;
        // dist falloff
        public bool showDistanceFalloffBatchMenu;
        public bool toggleDistanceFalloff;
        public float overrideDistanceFalloffFactor;
        // mat settings
        public bool showShaderSettingsBatchMenu;
        public bool toggleUseBillboardSync;
        public bool toggleUseShaderControllerOverrides;
        // billboard
        public bool showBillboardBatchMenu;
        public bool toggleBillboardState;
        public bool toggleBillboardCrossfadeState;
        public float overrideBillboardCrossfadeDistance;
        public bool toggleBillboardWind;
        public bool toggleBillboardHue;
        public bool toggleBillboardSnow;
        // collider
        public bool showColliderBatchMenu;
        public ColliderType overrideColliderType;
        public float overrideColliderDistance;
        // biome edge rules
        public bool showBiomeMaskRulesBatchMenu;
        public bool toggleBiomeEdgeScaleRule;   // scale
        public float overrideBiomeEdgeScaleDistance;
        public float overrideBiomeEdgeScaleMinScale;
        public float overrideBiomeEdgeScaleMaxScale;
        public bool toggleBiomeEdgeScaleInvert;
        public bool toggleBiomeEdgeIncludeRule; // include
        public float overrideBiomeEdgeIncludeDistance;
        public bool toggleBiomeEdgeIncludeInvert;
        // terrain source rules
        public bool showTerrainSourceRulesBatchMenu;
        public bool toggleTerrainSourceIncludeRule;
        public bool toggleTerrainSourceExcludeRule;
        public TerrainSourceRule overrideTerrainSourceIncludeRule;
        public TerrainSourceRule overrideTerrainSourceExcludeRule;

        // debug fields
        public bool showDebugSettingsMenu;
        public bool showSystemTotalArea;
        public bool showSeaLevel;
        public bool showAllMeshTerrainAreas;
        public bool showAllRaycastTerrainAreas;
        public bool showTextureMaskAreas;
        public TextureMask debugTextureMask;
        public bool showBiomeMasks;
        public bool showVegetationMaskCells;
        public bool showBiomeMaskCells;
        public bool showVegetationCells;
        public bool showPredictiveVegetationCells;
        public ECellCullingDebugMode showVisibleVegetationCells = ECellCullingDebugMode.Disabled;
        public bool showBillboardCells;
        public bool showPredictiveBillboardCells;
        public bool showVisibleBillboardCells;
        public bool onSceneCamChangeDirty;
        public bool showDebugToolsMenu;
#endif
        #endregion

        #region delegates
        public delegate void MultiOnAddCameraDelegate(VegetationStudioCamera _vegetationStudioCamera);
        public MultiOnAddCameraDelegate OnAddCameraDelegate;    // on add camera   

        public delegate void MultiOnRemoveCameraDelegate(VegetationStudioCamera _vegetationStudioCamera);
        public MultiOnRemoveCameraDelegate OnRemoveCameraDelegate;  // on remove camera

        public delegate void MultiOnVegetationStudioRefreshDelegate(VegetationSystemPro _vegetationSystemPro);
        public MultiOnVegetationStudioRefreshDelegate OnStartVegetationSystemDelegate;  // on vegetation system start
        public MultiOnVegetationStudioRefreshDelegate OnUnloadVegetationSystemDelegate; // on vegetation system unload
        public MultiOnVegetationStudioRefreshDelegate OnRefreshColliderSystemDelegate;  // on collider system refresh
        public MultiOnVegetationStudioRefreshDelegate OnRefreshRuntimePrefabSpawnerDelegate;    // on runtime prefab system refresh

        public delegate void MultiOnVegetationCellSpawnedDelegate(VegetationCell _vegetationCell, bool _preloaded = false);
        public MultiOnVegetationCellSpawnedDelegate OnVegetationCellLoaded; // on vegetation cell spawn

        public delegate void MultiOnClearCacheDelegate(VegetationSystemPro _vegetationSystemPro);
        public MultiOnClearCacheDelegate OnClearCacheDelegate;  // on clear cache

        public delegate void MultiOnClearCacheVegetationItemDelegate(VegetationSystemPro _vegetationSystemPro, int _vegetationPackageIndex, int _vegetationItemIndex);
        public MultiOnClearCacheVegetationItemDelegate OnClearCacheVegetationItemDelegate;  // on clear cache per item


        public delegate void MultiOnClearCacheVegetationCellDelegate(VegetationSystemPro _vegetationSystemPro, VegetationCell _vegetationCell);
        public MultiOnClearCacheVegetationCellDelegate OnClearCacheVegetationCellDelegate;  // on clear cache per cell

        public delegate void MultiOnClearCacheVegetationCellVegetationItemDelegate(VegetationSystemPro _vegetationSystemPro, VegetationCell _vegetationCell, int _vegetationPackageIndex, int _vegetationItemIndex);
        public MultiOnClearCacheVegetationCellVegetationItemDelegate OnClearCacheVegetationCellVegetatonItemDelegate;   // on clear cache per item and cell

        public delegate void MultiOnRenderCompleteDelegate(VegetationSystemPro _vegetationSystemPro);
        public MultiOnRenderCompleteDelegate OnRenderCompleteDelegate;  // on (render) loop completion
        #endregion

        #region Rendering data
        public readonly Dictionary<Material, Material> renderingMaterials = new();  // stores a pool of materials for shared use
        private readonly Matrix4x4[] renderingArray = new Matrix4x4[1023];  // stores chunks w/ 1023 instances to render
        private readonly Vector4[] renderingLodFadeArray = new Vector4[1023];   // stores chunks w/ 1023 instances of their fade data
        private readonly float[] renderingLayerArray = new float[1023]; // stored chunks w/ 1023 instances for rendering layers for SRPs
        private readonly int _unityLODFadeID = Shader.PropertyToID("unity_LODFade");    // engine ID for LOD fading
        private readonly int _unityRenderingLayerID = Shader.PropertyToID("unity_RenderingLayer");  // engine ID for rendering layers for SRPs
        float2 vegItemDistances = new();    // x = base distance -- y = culling distance
        Bounds renderBounds = new();    // center = camera position -- extents = per LOD or culling distance
        private RenderParams renderParams = new() { receiveShadows = true, renderingLayerMask = 1 };    // base render params -- renderingLayerMask hardcoded as not yet fully implemented by Unity either
#if USING_HDRP && UNITY_2023_1_OR_NEWER
        private RayTracingInstanceCullingConfig rtCullingConfig;    // RTAS culling config -- cull existing hierarchy gameObjects
        private RayTracingMeshInstanceConfig rtInstanceConfig;  // base RTAS render params
#endif

        // billboard shader IDs
        private readonly int _fadeDistanceID = Shader.PropertyToID("_FadeDistance");
        private readonly int _cullDistanceID = Shader.PropertyToID("_CullDistance");
        private readonly int _nearCullDistanceID = Shader.PropertyToID("_NearCullDistance");
        private readonly int _farCullDistanceID = Shader.PropertyToID("_FarCullDistance");

        #region ComputeShader data
        private readonly List<VegetationCell> computeShaderCellList = new();
        private GraphicsBuffer dummyGraphicsBuffer; // dummy for "null-ing"
        private GraphicsBuffer argsBufferDispatch;  // buffer to store the "argsDispatch"
        private readonly uint[] argsDispatch = new uint[4] { 1, 1, 1, 1 };  // dispatch parameters (threadGroupsX, threadGroupsY, threadGroupsZ, padding)

        private ComputeShader frustumMatrixShader;
        private int frustumKernelHandle;

        private int _mergeBufferID = -1;
        private ComputeShader mergeBufferShader;
        private int mergeBufferKernelHandle;

        private int _mergeSourceBuffer0ID = -1;
        private int _mergeSourceBuffer1ID = -1;
        private int _mergeSourceBuffer2ID = -1;
        private int _mergeSourceBuffer3ID = -1;
        private int _mergeSourceBuffer4ID = -1;
        private int _mergeSourceBuffer5ID = -1;
        private int _mergeSourceBuffer6ID = -1;
        private int _mergeSourceBuffer7ID = -1;
        private int _mergeSourceBuffer8ID = -1;
        private int _mergeSourceBuffer9ID = -1;
        private int _mergeSourceBuffer10ID = -1;
        private int _mergeSourceBuffer11ID = -1;
        private int _mergeSourceBuffer12ID = -1;
        private int _mergeSourceBuffer13ID = -1;
        private int _mergeSourceBuffer14ID = -1;
        private int _mergeSourceBuffer15ID = -1;

        private int _mergeInstanceCount0ID = -1;
        private int _mergeInstanceCount1ID = -1;
        private int _mergeInstanceCount2ID = -1;
        private int _mergeInstanceCount3ID = -1;
        private int _mergeInstanceCount4ID = -1;
        private int _mergeInstanceCount5ID = -1;
        private int _mergeInstanceCount6ID = -1;
        private int _mergeInstanceCount7ID = -1;
        private int _mergeInstanceCount8ID = -1;
        private int _mergeInstanceCount9ID = -1;
        private int _mergeInstanceCount10ID = -1;
        private int _mergeInstanceCount11ID = -1;
        private int _mergeInstanceCount12ID = -1;
        private int _mergeInstanceCount13ID = -1;
        private int _mergeInstanceCount14ID = -1;
        private int _mergeInstanceCount15ID = -1;

        private int _cameraFrustumPlane0 = -1;
        private int _cameraFrustumPlane1 = -1;
        private int _cameraFrustumPlane2 = -1;
        private int _cameraFrustumPlane3 = -1;
        private int _cameraFrustumPlane4 = -1;
        private int _cameraFrustumPlane5 = -1;
        private int _worldSpaceCameraPos = -1;

        private int _instanceCountID = -1;
        private int _sourceBufferID = -1;

        private int _objectBufferLod0ID = -1;
        private int _objectBufferLod1ID = -1;
        private int _objectBufferLod2ID = -1;
        private int _objectBufferLod3ID = -1;

        private int _shadowBufferLod0ID = -1;
        private int _shadowBufferLod1ID = -1;
        private int _shadowBufferLod2ID = -1;
        private int _shadowBufferLod3ID = -1;

        private int _cullDistance = -1;
        private int _floatingOriginOffsetID = -1;

        private int _noFrustumCullingID = -1;
        private int _hasBackShadowID = -1;
        private int _lightDirection = -1;

        private int _itemBoundsCenter = -1;
        private int _itemBoundsExtents = -1;

        private int _useLodFade = -1;
        private int _lodCount = -1;
        private int _maxLodIndex = -1;
        private int _maxLOD0 = -1;
        private int _maxLOD1 = -1;
        private int _maxLOD2 = -1;
        private int _maxLOD3 = -1;
        private int _shadowLODIndexID = -1;
        private int _customShadowLODIndex = -1;
        private int _lodFactor = -1;
        private int _lodBias = -1;
        private int _lodFadeDistance = -1;

        private int _lod0To1Distance = -1;
        private int _lod1To2Distance = -1;
        private int _lod2To3Distance = -1;

        private int _objectShaderDataBufferID = -1;
        private int _indirectShaderDataBufferID = -1;   // only needed for old indirect integrations
        #endregion
        #endregion

        void Reset()
        {
            RestartVegetationSystem();
        }

        void OnEnable()
        {
            SetupVegetationSystem();
        }

        void Update()
        {
            if (isSetupDone == false || vegetationCellList.Count <= 0)
                return; // skip if the system is not ready yet -- if the system is frozen

            // prepare data for cell/vegetation culling/loading
            VerifySplatmapAccess(); // verify/clear -> prepare splat map data => through the "IVegetationStudioTerrain" interface
            floatingOriginOffset = Application.isPlaying ? (float3)(floatingOriginAnchor ? floatingOriginAnchor : transform).position - floatingOriginStart : float3.zero;  // used for cell/vegetation culling, rendering, masks, etc
            if (shouldForceUpdateCellCulling)
                UpdateCellCullingBoundsAddy();

            // vegetation cell logic / jobs
            PrepareCameraData();    // prepare data for all cameras -- cell culling -- vegetation/billboard rendering/culling -- events (also) for collider/runtime prefab system
            LoadVegetationCells();  // predictive pre-load > (pre-)load

            // update data for rendering => (shadow) culling and LOD job/compute shader
            sunDirectionalLight = RenderSettings.sun;
            hasSunMoon = sunDirectionalLight != null && sunDirectionalLight.enabled && sunDirectionalLight.gameObject.activeInHierarchy && sunDirectionalLight.shadows != LightShadows.None && QualitySettings.shadows != ShadowQuality.Disable;
            sunMoonDirection = sunDirectionalLight ? sunDirectionalLight.transform.forward : float3.zero;
            if (lastEngineLODMax != QualitySettings.maximumLODLevel)
            {
                UpdateLODCounts();
                lastEngineLODMax = QualitySettings.maximumLODLevel;
            }

            // CPU render list preparation
            PrepareCPURenderList(); // cell matrix list merging > vegetation instance (frustum) culling / lod calculation
        }

        void LateUpdate()
        {
            if (isSetupDone == false || vegetationCellList.Count <= 0)
                return; // skip if the system is not ready yet -- if the system is frozen

            // complete spawning rules jobs -- CPU render list merge > cull/lod job
            CompleteVegetationCellLoading();    // finish / synchronize all jobs related to vegetation cell loading
            LoadBillboardCells();   // load billboard cells -- create persistent billboard data if not done yet (per camera > visible cell)

            // rendering
            RenderVegetation(); // render vegetation instances
            RenderBillboards(); // render billboard instances

            // clear memory
            ReturnTemporaryVegetationCellMemory();  // base clear -- clear the temporary data of vegetation instances used for the spawning rules
            CompactVegetationCellCache();   // extended clear -- clear vegetation cell caches out of range(persistent data storing "matrix instances" / "matrix buffers") -- declare vegetation cells "recreationable"
            CompactBillboardCellCache();    // extended clear -- clear billboard cell caches out of range(persistent data storing the "billboard-cell-mesh") -- declare billboard cells "recreationable"

            // end of entire loop notification
            OnRenderCompleteDelegate?.Invoke(this); // notify collider system / runtime prefab spawner
        }

        void OnDisable()
        {
            UnloadVegetationSystem();
        }

        #region Custom functions -- System
        private void SetupVegetationSystem()
        {
            EnableEditorApi();  // subscribe to editor delegates
            VegetationStudioManager.RegisterVegetationSystem(this);

            SetupCameras();
            FindWindZone();
            RefreshTerrainInterfaceList();
            persistentVegetationStorage = GetComponent<PersistentVegetationStorage>();

            SetupWindControllers();
            SetupSpeedTreeWindBridge();
            SetupVegetationItemModelData();
            SetupRenderingLayerData();
            SetupComputeShaderIDs();

            vegetationCellSpawner = new(this);
            predictiveCellLoader = new(this);
            floatingOriginStart = (floatingOriginAnchor ? floatingOriginAnchor : this.transform).position;  // set floating origin position
            CreateVegetationCells();
            CreateBillboardCells();

            isSetupDone = true; // enable/resume system
            OnStartVegetationSystemDelegate?.Invoke(this);  // notify collider system / runtime prefab spawner
        }

        private void UnloadVegetationSystem()
        {
            DisableEditorApi(); // unsubscribe from editor delegates
            VegetationStudioManager.UnregisterVegetationSystem(this);

            // rendering focused data
            DisposeVegetationStudioCameraData();
            DisposeVegetationItemModelData();
            DisposeGraphicsBuffers();

            // cell focused data
            DisposeVegetationCells();
            DisposeBillboardCells();
            vegetationCellSpawner?.Dispose();
            predictiveCellLoader?.Clear();

            isSetupDone = false;    // disable/freeze the system
            OnUnloadVegetationSystemDelegate?.Invoke(this); // notify collider system / runtime prefab spawner
        }
        #endregion

        #region Custom functions -- Utility
        public void RestartVegetationSystem()
        {
            UnloadVegetationSystem();
            SetupVegetationSystem();
        }

        public void RefreshItemSystem()
        {
            DisposeVegetationStudioCameraMatrixLists();
            DisposeVegetationItemModelData();
            SetupVegetationItemModelData();
            PrepareAllVegetationCells();
            PrepareAllBillboardCells();
            ClearCache();
            RefreshColliderSystem();    // notify collider system
            RefreshRuntimePrefabSpawner();  // notify runtime prefab spawner
        }

        public void RefreshCellSystem()
        {
            DisposeVegetationCells();
            DisposeBillboardCells();
            ClearVegetationStudioCameraData();
            floatingOriginStart = (floatingOriginAnchor ? floatingOriginAnchor : this.transform).position;  // set floating origin position
            CreateVegetationCells();
            CreateBillboardCells();
        }

        public void RefreshEverything() // used to update "everything" related to the base system -- used with the "CTRL+SHIFT+T" keybind to refresh "everything"
        {
            CalculateVegetationSystemBounds(true, false);   // update all internal data of the added terrains -- recalculate the "totalArea"
            RestartVegetationSystem();  // restart the entire vegetation system
        }

        public void RefreshColliderSystem()
        {
            OnRefreshColliderSystemDelegate?.Invoke(this);
        }

        public void RefreshRuntimePrefabSpawner()
        {
            OnRefreshRuntimePrefabSpawnerDelegate?.Invoke(this);
        }

        public void CompleteVegetationCellLoading()
        {
            Profiler.BeginSample("Complete vegetation cell loading");
            JobHandle.ScheduleBatchedJobs();    // run/prioritize jobs related to vegetation cell loading
            prepareVegetationHandle.Complete(); // finish / synchronize assigned job handles
            Profiler.EndSample();
        }

        private void EnableEditorApi()
        {
            TerrainCallbacks.heightmapChanged += OnHeightChanged;
            TerrainCallbacks.textureChanged += OnTextureChanged;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                SceneViewDetector.OnSceneViewTransformChangeDelegate += OnSceneviewTransformChanged;
#endif
        }

        private void DisableEditorApi()
        {
            TerrainCallbacks.heightmapChanged -= OnHeightChanged;
            TerrainCallbacks.textureChanged -= OnTextureChanged;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                SceneViewDetector.OnSceneViewTransformChangeDelegate -= OnSceneviewTransformChanged;
#endif
        }

        private void OnSceneviewTransformChanged(Camera _currentCamera)
        {
#if UNITY_EDITOR
            if (onSceneCamChangeDirty)
                return; // skip when set so through UI in the "Debug" tab

            EditorUtility.SetDirty(this);   // set as dirty to enforce a full refresh -- force the engine to stay up to date with the rendering in edit-mode
#endif
        }

        private void FindCamera()   // get first found "MainCamera"
        {
            if (useCameraAutoSelection == false)
                return;

            Camera selectedCamera = Camera.main;

            if (selectedCamera == null)
            {
                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                for (int i = 0; i < cameras.Length; i++)
                    if (cameras[i].gameObject.name.Contains("Main Camera") || cameras[i].gameObject.name.Contains("MainCamera"))
                    {
                        selectedCamera = cameras[i];
                        break;
                    }
            }

            if (selectedCamera != null)
                AddCamera(selectedCamera);
        }

        private void SetupCameras()
        {
            for (int i = vegetationStudioCameraList.Count - 1; i > -1; i--)
                if (vegetationStudioCameraList[i].eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView || vegetationStudioCameraList[i].selectedCamera == null)
                    RemoveVegetationStudioCamera(vegetationStudioCameraList[i]);    // remove old scene view camera -- any null cameras

            if (Application.isPlaying == false) // only use the scene camera when not in play mode
                AddVegetationStudioCamera(new(EVegetationStudioCameraType.SceneView) { vegetationSystemPro = this });

            if (Application.isPlaying && vegetationStudioCameraList.Count == 0)
                FindCamera();   // make sure the camera list is not empty
        }

        public void SetupRenderingLayerData()
        {
            float flagsFloat = BitConverter.ToSingle(BitConverter.GetBytes(vegetationRenderSettings.renderingLayerMask), 0);
            for (int i = 0; i < renderingLayerArray.Length; i++)
                renderingLayerArray[i] = flagsFloat;
            Shader.SetGlobalFloat("VSPRenderingLayerMask", flagsFloat);
        }
        #endregion
    }
}