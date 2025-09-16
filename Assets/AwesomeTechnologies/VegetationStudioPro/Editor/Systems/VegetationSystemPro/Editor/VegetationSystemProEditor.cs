using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Wind;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(VegetationSystemPro))]
    public class VegetationSystemProEditor : VegetationStudioProBaseEditor
    {
        private VegetationSystemPro vegetationSystemPro;
        private int selectedVegetationItemIndex;
        private int lastVegegetationItemIndex;
        private int selectedTerrainIndex;   // terrain for texture mask area

        private InspectorCurveEditor scaleCurveEditor;
        private InspectorCurveEditor heightCurveEditor;
        private InspectorCurveEditor steepnessCurveEditor;
        private InspectorCurveEditor distanceFalloffCurveEditor;

        private Texture2D dummyPreviewTexture;
        private int includeTerrainTextureIndex; // terrain texture
        private int excludeTerrainTextureIndex;
        private int densityTerrainTextureIndex;
        private int scaleTerrainTextureIndex;
        private int includeTextureMaskIndex;    // texture mask
        private int excludeTextureMaskIndex;
        private int densityTextureMaskIndex;
        private int scaleTextureMaskIndex;
        private int includeTextureMaskGroupIndex;   // texture mask group
        private int excludeTextureMaskGroupIndex;
        private int densityTextureMaskGroupIndex;
        private int scaleTextureMaskGroupIndex;

        private readonly string[] tabNames = { "Settings", "Terrains", "Cameras", "Rendering", "Weather", "Debug" };
        private readonly string[] settingsSubTabNames = { "Base", "Extra" };
        private readonly string[] tabNamesBiomes = { "Biomes", "Texture masks", "Edit items", "Batch edit" };
        private readonly string[] vegetationTypeNames = { "All", "Trees", "Large Objects", "Objects", "Plants", "Grass" };
        private readonly string[] vegetationSubTabNames = { "Rendering", "Spawning" };
        private string[] navAreas;
        private readonly string[] rgbaChannelNames = { "R Channel", "G Channel", "B Channel", "A Channel" };
        private readonly string[] renderingLayerMaskNames =
        {
            "0: Light Layer default", "1: Light Layer 1", "2: Light Layer 2", "3: Light Layer 3", "4: Light Layer 4", "5: Light Layer 5", "6: Light Layer 6", "7: Light Layer 7",
            "8: Decal Layer default", "9: Decal Layer 1", "10: Decal Layer 2", "11: Decal Layer 3", "12: Decal Layer 4", "13: Decal Layer 5", "14: Decal Layer 6", "15: Decal Layer 7"
        };

        void OnEnable()
        {
            vegetationSystemPro = (VegetationSystemPro)target;
            navAreas = UnityEngine.AI.NavMesh.GetAreaNames();
            dummyPreviewTexture = new Texture2D(80, 80);

            InspectorCurveEditor.Settings curveSettings = InspectorCurveEditor.Settings.DefaultSettings;
            scaleCurveEditor = new InspectorCurveEditor(curveSettings) { CurveType = InspectorCurveEditor.InspectorCurveType.Scale };
            heightCurveEditor = new InspectorCurveEditor(curveSettings) { CurveType = InspectorCurveEditor.InspectorCurveType.Height };
            steepnessCurveEditor = new InspectorCurveEditor(curveSettings) { CurveType = InspectorCurveEditor.InspectorCurveType.Steepness };
            distanceFalloffCurveEditor = new InspectorCurveEditor(curveSettings) { CurveType = InspectorCurveEditor.InspectorCurveType.FalloffInverted };
        }

        void OnDisable()
        {
            scaleCurveEditor.RemoveAll();
            heightCurveEditor.RemoveAll();
            steepnessCurveEditor.RemoveAll();
            distanceFalloffCurveEditor.RemoveAll();
            DestroyImmediate(dummyPreviewTexture);
            VegetationStudioManager.ShowBiomes = false;
        }

        public override void OnInspectorGUI()
        {
            vegetationSystemPro = (VegetationSystemPro)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            vegetationSystemPro.selectedTabIndex = GUILayout.SelectionGrid(vegetationSystemPro.selectedTabIndex, tabNames, 6, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.selectedBiomeTabIndex = -1;

            EditorGUI.BeginChangeCheck();
            vegetationSystemPro.selectedBiomeTabIndex = GUILayout.SelectionGrid(vegetationSystemPro.selectedBiomeTabIndex, tabNamesBiomes, 4, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.selectedTabIndex = vegetationSystemPro.selectedBiomeTabIndex + tabNames.Length;
            GUILayout.EndVertical();

            switch (vegetationSystemPro.selectedTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawTerrainsInspector();
                    break;
                case 2:
                    DrawCamerasInspector();
                    break;
                case 3:
                    DrawRenderingInspector();
                    break;
                case 4:
                    DrawWeatherInspector();
                    break;
                case 5:
                    DrawDebugInspector();
                    break;
                case 6:
                    DrawBiomesInspector();
                    break;
                case 7:
                    DrawTextureMasksInspector();
                    break;
                case 8:
                    DrawEditItemsInspector();
                    break;
                case 9:
                    DrawBatchEditInspector();
                    break;
            }
        }

        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(vegetationSystemPro);
        }

        void DrawSettingsInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("The base settings for the vegetation system", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            vegetationSystemPro.selectedSettingsSubTabIndex = GUILayout.SelectionGrid(vegetationSystemPro.selectedSettingsSubTabIndex, settingsSubTabNames, 2, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            if (vegetationSystemPro.selectedSettingsSubTabIndex == 0)
            {
                if (vegetationSystemPro.showSeaLevelMenu = VegetationPackageEditorTools.DrawHeader("Sea level", vegetationSystemPro.showSeaLevelMenu))
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.HelpBox("The sea level is relative to the minimum height of the total area in the \"Terrains\" tab\nExclude underwater cells for less memory usage and better performance", MessageType.Warning);
                    vegetationSystemPro.SeaLevel = EditorGUILayout.DelayedFloatField("Sea level", vegetationSystemPro.SeaLevel);
                    vegetationSystemPro.excludeSeaLevelCells = EditorGUILayout.Toggle("Exclude underwater cells", vegetationSystemPro.excludeSeaLevelCells);
                    EditorGUILayout.HelpBox("Use the \"Debug\" tab to visualize and understand this behavior", MessageType.Info);
                    GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        vegetationSystemPro.RefreshCellSystem();
                        SetSceneDirty();
                    }
                }

                if (vegetationSystemPro.showCellSizeMenu = VegetationPackageEditorTools.DrawHeader("Cell sizes", vegetationSystemPro.showCellSizeMenu))
                {
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.HelpBox("For best performance choose terrain and cell sizes that are \"power of two\"\nex: 256, 512, 1024, 2048 for terrain sizes\nex: 32, 64, 128, 256 for cell sizes" +
                        "\nEnsure the cells perfectly align with the terrain by using the \"Debug\" tab\nThis can be done by clean division/modulo of \"TotalArea\" extents XZ by the cell size" +
                        "\nFor cell sizes higher than the total area extents XZ do the reverse\nex: A % B = 0 -- C % A = 0", MessageType.Info);
                    EditorGUILayout.HelpBox("Changing cell sizes also changes vegetation instance data and requires re-initialization of the used persistent storage", MessageType.Warning);
                    vegetationSystemPro.vegetationCellSize = EditorGUILayout.Slider("Vegetation cell size", vegetationSystemPro.vegetationCellSize, 25, 256);
                    vegetationSystemPro.billboardCellSize = EditorGUILayout.Slider("Billboard cell size", vegetationSystemPro.billboardCellSize, 500, 8192);
                    if (GUILayout.Button("Update"))
                    {
                        vegetationSystemPro.RefreshCellSystem();
                        SetSceneDirty();
                    }
                    GUILayout.EndVertical();
                }

                if (vegetationSystemPro.showGlobalDensityMenu = VegetationPackageEditorTools.DrawHeader("Global densities", vegetationSystemPro.showGlobalDensityMenu))
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical("box");
                    vegetationSystemPro.vegetationSettings.seed = EditorGUILayout.IntSlider("Seed", vegetationSystemPro.vegetationSettings.seed, 0, 100);
                    EditorGUILayout.Space();
                    vegetationSystemPro.vegetationSettings.grassDensity = EditorGUILayout.Slider("Grass density", vegetationSystemPro.vegetationSettings.grassDensity, 0, 2);
                    vegetationSystemPro.vegetationSettings.plantDensity = EditorGUILayout.Slider("Plant density", vegetationSystemPro.vegetationSettings.plantDensity, 0, 2);
                    vegetationSystemPro.vegetationSettings.objectDensity = EditorGUILayout.Slider("Object density", vegetationSystemPro.vegetationSettings.objectDensity, 0, 2);
                    vegetationSystemPro.vegetationSettings.largeObjectDensity = EditorGUILayout.Slider("Large object density", vegetationSystemPro.vegetationSettings.largeObjectDensity, 0, 2);
                    vegetationSystemPro.vegetationSettings.treeDensity = EditorGUILayout.Slider("Tree density", vegetationSystemPro.vegetationSettings.treeDensity, 0, 2);
                    GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        vegetationSystemPro.ClearCache();
                        SetSceneDirty();
                    }
                }

                return;
            }

            EditorGUI.BeginChangeCheck();

            if (vegetationSystemPro.showSystemBehaviorMenu = VegetationPackageEditorTools.DrawHeader("System behavior", vegetationSystemPro.showSystemBehaviorMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Refresh shortcuts can be found in \"Window/AwesomeTechnologies/Shortcuts\"", MessageType.Info);
                EditorGUILayout.HelpBox("Undo/Redo operations always depend on whether the engine does UI updates\n- scene gizmos are enabled\n- the terrain is selected and its \"UnityTerrain\" script is not hidden/closed", MessageType.Warning);
                vegetationSystemPro.enableAutoSystemRefresh = EditorGUILayout.Toggle("Refresh on terrain changes", vegetationSystemPro.enableAutoSystemRefresh);
                EditorGUILayout.HelpBox("Automatically refresh after the engine detects changes to terrains\nThe engine only detects certain changes on saving the scene", MessageType.Info);
                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showTerrainStreamingMenu = VegetationPackageEditorTools.DrawHeader("Terrain streaming support", vegetationSystemPro.showTerrainStreamingMenu))
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("For terrain streaming setups few steps are required, more info in the \"Terrains\" tab\nTo improve the workflow consider using the \"TerrainStreamingLoader\" component", MessageType.Info);
                vegetationSystemPro.floatingOriginAnchor = (Transform)EditorGUILayout.ObjectField("Origin transform", vegetationSystemPro.floatingOriginAnchor, typeof(Transform), true);
                EditorGUILayout.HelpBox("Assign a transform to use as the world anchor for floating origin offsets\nIf no transform is set this gameObject gets used", MessageType.Info);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.RestartVegetationSystem();
            }

            if (vegetationSystemPro.showPredictiveCellMenu = VegetationPackageEditorTools.DrawHeader("Predictive preloading", vegetationSystemPro.showPredictiveCellMenu))
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");

                vegetationSystemPro.loadPredictiveCells = EditorGUILayout.Toggle("Load additional cells", vegetationSystemPro.loadPredictiveCells);
                EditorGUILayout.HelpBox("Preload additional cells, per frame, within the \"pre-load square\" around a camera\n" +
                    "The \"pre-load square\" is based on the highest rendering distance set in the \"Rendering\" tab\n" +
                    "This enables further use of worker threads, per frame, to pre-load cells over time which smooths out the cell loading and camera rotation movements\n" +
                    "By default a camera only loads visible cells, per frame, which can cause hitches in more extreme situations", MessageType.Info);
                EditorGUILayout.HelpBox("Memory usage increases since more cells are loaded at the same time\nThis also depends on the included vegetation types and their density", MessageType.Warning);

                vegetationSystemPro.predictiveCellsPerFrame = EditorGUILayout.IntSlider("Cells per frame", vegetationSystemPro.predictiveCellsPerFrame, 1, 5);
                EditorGUILayout.HelpBox("The max allowed cells per frame that should be offloaded to worker threads\n 1 = smooth / 5 = aggressive loading", MessageType.Info);
                EditorGUILayout.HelpBox("Quick cell loading with high cells per frame can cause performance drops\n" +
                    "This is especially the case for very fast moving cameras\nReduce hitches by using less cells per frame / include less dense vegetation types", MessageType.Warning);

                vegetationSystemPro.togglePredictiveGrass = EditorGUILayout.Toggle("Preload grass", vegetationSystemPro.togglePredictiveGrass);
                vegetationSystemPro.togglePredictivePlants = EditorGUILayout.Toggle("Preload plants", vegetationSystemPro.togglePredictivePlants);
                vegetationSystemPro.togglePredictiveObjects = EditorGUILayout.Toggle("Preload objects", vegetationSystemPro.togglePredictiveObjects);
                vegetationSystemPro.togglePredictiveLargeObjects = EditorGUILayout.Toggle("Preload large objects", vegetationSystemPro.togglePredictiveLargeObjects);
                vegetationSystemPro.togglePredictiveTrees = EditorGUILayout.Toggle("Preload trees", vegetationSystemPro.togglePredictiveTrees);

                vegetationSystemPro.togglePredictiveBillboards = EditorGUILayout.Toggle("Preload billboards", vegetationSystemPro.togglePredictiveBillboards);
                EditorGUILayout.HelpBox("Billboard cells load seperately after vegetation cells and have their own \"pre-load square\"", MessageType.Info);

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.ClearVegetationStudioCameraData();
            }

            if (vegetationSystemPro.showCompactCacheMenu = VegetationPackageEditorTools.DrawHeader("Memory management", vegetationSystemPro.showCompactCacheMenu))
            {
                GUILayout.BeginVertical("box");

                vegetationSystemPro.useCompactCache = EditorGUILayout.Toggle("Enable cell cache compacting", vegetationSystemPro.useCompactCache);
                EditorGUILayout.HelpBox("Loaded cells that leave the \"pre-load square\", of all cameras combined, clear their cache at the end of the next available frame\n" +
                    "The \"pre-load square\" is based on the highest rendering distance set in the \"Rendering\" tab", MessageType.Info);
                EditorGUILayout.HelpBox("This reduces memory usage and is especially useful for very large terrains\n" +
                    "But can be undesired for certain use cases as cells need to be loaded again", MessageType.Warning);

                vegetationSystemPro.toggleCompactCacheGrass = EditorGUILayout.Toggle("Compact grass", vegetationSystemPro.toggleCompactCacheGrass);
                vegetationSystemPro.toggleCompactCachePlants = EditorGUILayout.Toggle("Compact plants", vegetationSystemPro.toggleCompactCachePlants);
                vegetationSystemPro.toggleCompactCacheObjects = EditorGUILayout.Toggle("Compact objects", vegetationSystemPro.toggleCompactCacheObjects);
                vegetationSystemPro.toggleCompactCacheLargeObjects = EditorGUILayout.Toggle("Compact large objects", vegetationSystemPro.toggleCompactCacheLargeObjects);
                vegetationSystemPro.toggleCompactCacheTrees = EditorGUILayout.Toggle("Compact trees", vegetationSystemPro.toggleCompactCacheTrees);

                vegetationSystemPro.toggleCompactCacheBillboards = EditorGUILayout.Toggle("Compact billboards", vegetationSystemPro.toggleCompactCacheBillboards);
                EditorGUILayout.HelpBox("Billboard cells clear seperately after vegetation cells and have their own \"pre-load square\"", MessageType.Info);

                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();
        }

        void DrawRenderingInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Set base and additional rendering settings\nOptimize performance and toggle platform support", MessageType.Info);
            GUILayout.EndVertical();

            if (vegetationSystemPro.showGlobalDistanceMenu = VegetationPackageEditorTools.DrawHeader("Global rendering distances", vegetationSystemPro.showGlobalDistanceMenu))
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");

                vegetationSystemPro.vegetationSettings.crossFadeDistance = EditorGUILayout.Slider("Crossfade distance", vegetationSystemPro.vegetationSettings.crossFadeDistance, 0, 20);
                EditorGUILayout.HelpBox("A higher crossfade distance uses up more performance, keep it as low as possible\nThe crossfade distance works additively to the vegetation distances", MessageType.Warning);

                vegetationSystemPro.vegetationSettings.disableLocalDistanceFactor = EditorGUILayout.Toggle("Disable local distance factor", vegetationSystemPro.vegetationSettings.disableLocalDistanceFactor);
                EditorGUILayout.HelpBox("Disable the local distance factor, set seperately on vegetation items, to enforce pure usage of the global vegetation distances", MessageType.Info);

                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.vegetationSettings.grassDistance = EditorGUILayout.Slider("Grass distance", vegetationSystemPro.vegetationSettings.grassDistance, 0, 250);
                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.RefreshColliderSystem();
                    vegetationSystemPro.RefreshRuntimePrefabSpawner();
                }
                vegetationSystemPro.vegetationSettings.plantDistance = EditorGUILayout.Slider("Plant distance", vegetationSystemPro.vegetationSettings.plantDistance, 0, 350);
                vegetationSystemPro.vegetationSettings.objectDistance = EditorGUILayout.Slider("Object distance", vegetationSystemPro.vegetationSettings.objectDistance, 0, 350);
                vegetationSystemPro.vegetationSettings.largeObjectDistance = EditorGUILayout.Slider("Large object distance", vegetationSystemPro.vegetationSettings.largeObjectDistance, 0, 2000);
                vegetationSystemPro.vegetationSettings.treeDistance = EditorGUILayout.Slider("Tree distance", vegetationSystemPro.vegetationSettings.treeDistance, 0, 2000);

                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.vegetationSettings.billboardDistanceFactor = EditorGUILayout.Slider("Billboard distance factor", vegetationSystemPro.vegetationSettings.billboardDistanceFactor, 0, 1);
                EditorGUILayout.HelpBox("Billboards render between tree-meshes and a camera's far clip plane", MessageType.Info);
                if (EditorGUI.EndChangeCheck())
                    if (vegetationSystemPro.vegetationSettings.billboardDistanceFactor <= 0)
                        vegetationSystemPro.ClearBillboardCellCache();

                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                    SetSceneDirty();
            }

            if (vegetationSystemPro.showRenderShadowMenu = VegetationPackageEditorTools.DrawHeader("Shadows", vegetationSystemPro.showRenderShadowMenu))
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Per LOD shadow limitation (distance): -1 = disabled shadows", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.grassShadows = (int)EditorGUILayout.Slider("Grass shadows", vegetationSystemPro.vegetationRenderSettings.grassShadows, -1, 3);
                vegetationSystemPro.vegetationRenderSettings.plantShadows = (int)EditorGUILayout.Slider("Plant shadows", vegetationSystemPro.vegetationRenderSettings.plantShadows, -1, 3);
                vegetationSystemPro.vegetationRenderSettings.objectShadows = (int)EditorGUILayout.Slider("Object shadows", vegetationSystemPro.vegetationRenderSettings.objectShadows, -1, 3);
                vegetationSystemPro.vegetationRenderSettings.largeObjectShadows = (int)EditorGUILayout.Slider("Large object shadows", vegetationSystemPro.vegetationRenderSettings.largeObjectShadows, -1, 3);
                vegetationSystemPro.vegetationRenderSettings.treeShadows = (int)EditorGUILayout.Slider("Tree shadows", vegetationSystemPro.vegetationRenderSettings.treeShadows, -1, 3);
                vegetationSystemPro.vegetationRenderSettings.billboardShadows = EditorGUILayout.Toggle("Billboard shadows", vegetationSystemPro.vegetationRenderSettings.billboardShadows);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Custom LOD shadow limitation (quality): 0 = disabled", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.grassCustomShadowLODIndex = (int)EditorGUILayout.Slider("Grass custom index", vegetationSystemPro.vegetationRenderSettings.grassCustomShadowLODIndex, 0, 3);
                vegetationSystemPro.vegetationRenderSettings.plantCustomShadowLODIndex = (int)EditorGUILayout.Slider("Plant custom index", vegetationSystemPro.vegetationRenderSettings.plantCustomShadowLODIndex, 0, 3);
                vegetationSystemPro.vegetationRenderSettings.objectCustomShadowLODIndex = (int)EditorGUILayout.Slider("Object custom index", vegetationSystemPro.vegetationRenderSettings.objectCustomShadowLODIndex, 0, 3);
                vegetationSystemPro.vegetationRenderSettings.largeObjectCustomShadowLODIndex = (int)EditorGUILayout.Slider("Large object custom index", vegetationSystemPro.vegetationRenderSettings.largeObjectCustomShadowLODIndex, 0, 3);
                vegetationSystemPro.vegetationRenderSettings.treeCustomShadowLODIndex = (int)EditorGUILayout.Slider("Tree custom index", vegetationSystemPro.vegetationRenderSettings.treeCustomShadowLODIndex, 0, 3);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Day/Night cycle support", labelStyle);
                EditorGUILayout.HelpBox("Cells behind a camera stay active for shadows of \"LargeObjects/Trees/Billboards\"\n" +
                    "The used distance is the smaller value between the engine's \"ShadowDistance\" and the higher distance of \"LargeObjects vs Trees\" or of \"Billboards\" separately", MessageType.Info);
                vegetationSystemPro.vegetationRenderSettings.dayNightSupport = EditorGUILayout.Toggle("Enhanced day/night cycle support", vegetationSystemPro.vegetationRenderSettings.dayNightSupport);
                EditorGUILayout.HelpBox("Enabling this inverts to use the maximum distance instead of minimum which can come with a higher performance and memory cost in favor of accuracy\n" +
                    "Use the \"Debug\" tab to visualize this behavior", MessageType.Warning);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                    SetSceneDirty();
            }

            if (vegetationSystemPro.showRenderLayerMenu = VegetationPackageEditorTools.DrawHeader("Layers", vegetationSystemPro.showRenderLayerMenu))
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Layers -- Culling Mask / Colliders", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.grassLayer = EditorGUILayout.LayerField("Grass layer", vegetationSystemPro.vegetationRenderSettings.grassLayer);
                vegetationSystemPro.vegetationRenderSettings.plantLayer = EditorGUILayout.LayerField("Plant layer", vegetationSystemPro.vegetationRenderSettings.plantLayer);
                vegetationSystemPro.vegetationRenderSettings.objectLayer = EditorGUILayout.LayerField("Object layer", vegetationSystemPro.vegetationRenderSettings.objectLayer);
                vegetationSystemPro.vegetationRenderSettings.largeObjectLayer = EditorGUILayout.LayerField("Large object layer", vegetationSystemPro.vegetationRenderSettings.largeObjectLayer);
                vegetationSystemPro.vegetationRenderSettings.treeLayer = EditorGUILayout.LayerField("Tree layer", vegetationSystemPro.vegetationRenderSettings.treeLayer);
                vegetationSystemPro.vegetationRenderSettings.billboardLayer = EditorGUILayout.LayerField("Billboard layer", vegetationSystemPro.vegetationRenderSettings.billboardLayer);
                EditorGUILayout.HelpBox("Select the basic layers for vegetation instance rendering and their colliders", MessageType.Info);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.RefreshColliderSystem();
                    SetSceneDirty();
                }

                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Rendering layers", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.renderingLayerMask = EditorGUILayout.MaskField(vegetationSystemPro.vegetationRenderSettings.renderingLayerMask, renderingLayerMaskNames);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.SetupRenderingLayerData();
                    SetSceneDirty();
                }
            }

            if (vegetationSystemPro.showProbeMenu = VegetationPackageEditorTools.DrawHeader("Probes", vegetationSystemPro.showProbeMenu))
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Light probes", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.grassBlendProbes = EditorGUILayout.Toggle("Grass blend probes", vegetationSystemPro.vegetationRenderSettings.grassBlendProbes);
                vegetationSystemPro.vegetationRenderSettings.plantBlendProbes = EditorGUILayout.Toggle("Plant blend probes", vegetationSystemPro.vegetationRenderSettings.plantBlendProbes);
                vegetationSystemPro.vegetationRenderSettings.objectBlendProbes = EditorGUILayout.Toggle("Object blend probes", vegetationSystemPro.vegetationRenderSettings.objectBlendProbes);
                vegetationSystemPro.vegetationRenderSettings.largeObjectBlendProbes = EditorGUILayout.Toggle("Large object blend probes", vegetationSystemPro.vegetationRenderSettings.largeObjectBlendProbes);
                vegetationSystemPro.vegetationRenderSettings.treeBlendProbes = EditorGUILayout.Toggle("Tree blend probes", vegetationSystemPro.vegetationRenderSettings.treeBlendProbes);
                vegetationSystemPro.vegetationRenderSettings.billboardBlendProbes = EditorGUILayout.Toggle("Billboard blend probes", vegetationSystemPro.vegetationRenderSettings.billboardBlendProbes);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Reflection probes", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.grassRPU = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Grass reflection probes", vegetationSystemPro.vegetationRenderSettings.grassRPU);
                vegetationSystemPro.vegetationRenderSettings.plantRPU = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Plant reflection probes", vegetationSystemPro.vegetationRenderSettings.plantRPU);
                vegetationSystemPro.vegetationRenderSettings.objectRPU = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Object reflection probes", vegetationSystemPro.vegetationRenderSettings.objectRPU);
                vegetationSystemPro.vegetationRenderSettings.largeObjectRPU = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Large reflection blend probes", vegetationSystemPro.vegetationRenderSettings.largeObjectRPU);
                vegetationSystemPro.vegetationRenderSettings.treeRPU = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Tree reflection probes", vegetationSystemPro.vegetationRenderSettings.treeRPU);
                vegetationSystemPro.vegetationRenderSettings.billboardRPU = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Billboard reflection probes", vegetationSystemPro.vegetationRenderSettings.billboardRPU);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                    SetSceneDirty();
            }

            if (vegetationSystemPro.showRenderPlatformMenu = VegetationPackageEditorTools.DrawHeader("Platform support", vegetationSystemPro.showRenderPlatformMenu))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Disable instanced indirect per platform, fallback to instanced", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectWindows = EditorGUILayout.Toggle("Windows", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectWindows);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectOsx = EditorGUILayout.Toggle("OSX", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectOsx);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectLinux = EditorGUILayout.Toggle("Linux", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectLinux);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectIos = EditorGUILayout.Toggle("iOS", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectIos);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectAndroid = EditorGUILayout.Toggle("Android", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectAndroid);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectTvOs = EditorGUILayout.Toggle("Apple TvOS", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectTvOs);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectXboxOne = EditorGUILayout.Toggle("Xbox One", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectXboxOne);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectPs4 = EditorGUILayout.Toggle("PS4", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectPs4);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectWsa = EditorGUILayout.Toggle("Windows store app", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectWsa);
                vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectWebGL = EditorGUILayout.Toggle("WebGL", vegetationSystemPro.vegetationRenderSettings.disableInstancedIndirectWebGL);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.RestartVegetationSystem();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("VR rendering", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.enableSinglePassInstancedVR = EditorGUILayout.Toggle("Single pass instanced support", vegetationSystemPro.vegetationRenderSettings.enableSinglePassInstancedVR);
                EditorGUILayout.HelpBox("Enable this when using \"Instanced indirect\" as the render mode", MessageType.Info);
                EditorGUILayout.HelpBox("Performance is going to be reduced, only enable this when needed!", MessageType.Warning);
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                    SetSceneDirty();
            }
        }

        void DrawWeatherInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Weather settings can be easily integrated into any shader using the \"IShaderController\" interface and with other third party systems", MessageType.Info);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();

            if (vegetationSystemPro.showWeatherSnowMenu = VegetationPackageEditorTools.DrawHeader("Snow", vegetationSystemPro.showWeatherSnowMenu))
            {
                GUILayout.BeginVertical("box");
                vegetationSystemPro.environmentSettings.snowAmount = EditorGUILayout.Slider("Snow amount", vegetationSystemPro.environmentSettings.snowAmount, 0, 1);
                vegetationSystemPro.environmentSettings.snowMinimumVariation = EditorGUILayout.Slider("Snow minimum variation", vegetationSystemPro.environmentSettings.snowMinimumVariation, 0, 1);
                vegetationSystemPro.environmentSettings.snowBlendPower = EditorGUILayout.Slider("Snow blend power", vegetationSystemPro.environmentSettings.snowBlendPower, 0, 1);
                vegetationSystemPro.environmentSettings.billboardSnowBlendPower = EditorGUILayout.Slider("Billboard snow blend power", vegetationSystemPro.environmentSettings.billboardSnowBlendPower, 0, 1);
                vegetationSystemPro.environmentSettings.snowMinHeight = EditorGUILayout.Slider("Snow minimum height", vegetationSystemPro.environmentSettings.snowMinHeight, 0, 1000);
                vegetationSystemPro.environmentSettings.snowMinHeightVariation = EditorGUILayout.Slider("Snow minimum height variation", vegetationSystemPro.environmentSettings.snowMinHeightVariation, 0, 50);
                vegetationSystemPro.environmentSettings.snowMinHeightBlendPower = EditorGUILayout.Slider("Snow minimum height blend power", vegetationSystemPro.environmentSettings.snowMinHeightBlendPower, 0.01f, 1);

                GUIContent snowLabel = new("Snow color");
                GUIContent snowSpecularLabel = new("Snow specular color");
                vegetationSystemPro.environmentSettings.snowColor = EditorGUILayout.ColorField(snowLabel, vegetationSystemPro.environmentSettings.snowColor, true, true, true);
                vegetationSystemPro.environmentSettings.snowSpecularColor = EditorGUILayout.ColorField(snowSpecularLabel, vegetationSystemPro.environmentSettings.snowSpecularColor, true, true, true);
                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showWeatherRainMenu = VegetationPackageEditorTools.DrawHeader("Rain", vegetationSystemPro.showWeatherRainMenu))
            {
                GUILayout.BeginVertical("box");
                vegetationSystemPro.environmentSettings.rainAmount = EditorGUILayout.Slider("Rain amount", vegetationSystemPro.environmentSettings.rainAmount, 0, 1);
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.RefreshMaterials();

            if (vegetationSystemPro.showWeatherWindMenu = VegetationPackageEditorTools.DrawHeader("Wind", vegetationSystemPro.showWeatherWindMenu))
            {
                GUILayout.BeginVertical("box");
                vegetationSystemPro.selectedWindZone = (WindZone)EditorGUILayout.ObjectField("Wind zone", vegetationSystemPro.selectedWindZone, typeof(WindZone), true);
                vegetationSystemPro.windSpeedFactor = EditorGUILayout.Slider("Wind speed factor", vegetationSystemPro.windSpeedFactor, 0f, 5f);
                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showWeatherCustomWindMenu = VegetationPackageEditorTools.DrawHeader("Custom wind controllers", vegetationSystemPro.showWeatherCustomWindMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("In here are custom wind settings using \"IWindController\" interfaces\n\"HDWind\" is used by VSP-B's default shaders\nUse the inspector's debug mode to access underlying info", MessageType.Info);
                for (int i = 0; i < vegetationSystemPro.windControllerSettingsList.Count; i++)
                    DrawWindControllerInspector(vegetationSystemPro.windControllerSettingsList[i]);
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();
        }

        void DrawWindControllerInspector(WindControllerSettings _windControllerSettings)
        {
            if (_windControllerSettings == null)
                return;

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(_windControllerSettings.heading, labelStyle);
            for (int i = 0; i < _windControllerSettings.controllerPropertyList.Count; i++)
                DrawSerializedProperty(_windControllerSettings.controllerPropertyList[i]);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.RefreshWindControllerSettings();
                vegetationSystemPro.UpdateWindControllers();
                SetSceneDirty();
            }
        }

        void DrawCamerasInspector()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Added cameras spawn vegetation instances using their selected culling mode\nWhen using multiple cameras enable \"Render direct to camera\"" +
                "\nCameras not added are able to render vegetation but don't spawn any", MessageType.Info);
            vegetationSystemPro.useCameraAutoSelection = EditorGUILayout.Toggle("Automatically add main camera", vegetationSystemPro.useCameraAutoSelection);
            EditorGUILayout.HelpBox("When the scene is run this adds the first found \"MainCamera\"\nFor semi-auto behavior a \"CameraLoader\" component can be used", MessageType.Info);
            Camera newCamera = (Camera)EditorGUILayout.ObjectField("Add Camera", null, typeof(Camera), true);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                if (newCamera)
                    vegetationSystemPro.AddCamera(newCamera);
                SetSceneDirty();
            }

            bool multipleCameras;
            if (Application.isPlaying)
                multipleCameras = vegetationSystemPro.vegetationStudioCameraList.Count > 1;
            else
                multipleCameras = vegetationSystemPro.vegetationStudioCameraList.Count > 2;

            for (int i = 0; i < vegetationSystemPro.vegetationStudioCameraList.Count; i++)
            {
                GUILayout.BeginVertical("box");
                if (vegetationSystemPro.vegetationStudioCameraList[i].eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView == false)
                    if (GUILayout.Button("Remove camera", GUILayout.Width(120)))
                    {
                        vegetationSystemPro.RemoveCamera(vegetationSystemPro.vegetationStudioCameraList[i].selectedCamera);
                        SetSceneDirty();
                        return;
                    }

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(vegetationSystemPro.vegetationStudioCameraList[i].eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView);
                vegetationSystemPro.vegetationStudioCameraList[i].selectedCamera = (Camera)EditorGUILayout.ObjectField("Camera", vegetationSystemPro.vegetationStudioCameraList[i].selectedCamera, typeof(Camera), true);
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                {
                    if (vegetationSystemPro.vegetationStudioCameraList[i].selectedCamera == null)
                        vegetationSystemPro.RemoveCamera(vegetationSystemPro.vegetationStudioCameraList[i].selectedCamera);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

                if (vegetationSystemPro.vegetationStudioCameraList[i].eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView == false)
                {
                    EditorGUI.BeginChangeCheck();
                    vegetationSystemPro.vegetationStudioCameraList[i].renderDirectToCamera = EditorGUILayout.Toggle("Render direct to camera", vegetationSystemPro.vegetationStudioCameraList[i].renderDirectToCamera);

                    if (vegetationSystemPro.vegetationStudioCameraList[i].renderDirectToCamera == false && multipleCameras)
                        EditorGUILayout.HelpBox("Multiple cameras detected, enable \"Render direct to camera\" to avoid rendering vegetation twice", MessageType.Warning);

                    EditorGUI.BeginChangeCheck();
                    vegetationSystemPro.vegetationStudioCameraList[i].renderBillboardsOnly = EditorGUILayout.Toggle("Render billboards only", vegetationSystemPro.vegetationStudioCameraList[i].renderBillboardsOnly);
                    if (EditorGUI.EndChangeCheck())
                        vegetationSystemPro.vegetationStudioCameraList[i].PreCullCalculations(vegetationSystemPro.floatingOriginOffset, true);

                    vegetationSystemPro.vegetationStudioCameraList[i].eCameraCullingMode = (ECameraCullingMode)EditorGUILayout.EnumPopup("Culling mode", vegetationSystemPro.vegetationStudioCameraList[i].eCameraCullingMode);
                    if (EditorGUI.EndChangeCheck())
                        SetSceneDirty();
                }
                else
                {
                    EditorGUILayout.HelpBox("While in edit mode the engine's \"SceneCamera\" is used to spawn the vegetation", MessageType.Info);
                }
                GUILayout.EndVertical();
            }
        }

        void DrawTerrainsInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Add any \"UnityTerrain\" / \"MeshTerrain\" / \"RaycastTerrain\"\nOr any custom terrain implementing the \"IVegetationStudioTerrain\" interface", MessageType.Info);
            if (vegetationSystemPro.automaticBoundsCalculation == false)
                EditorGUILayout.HelpBox("\"Automatic calculation\" is disabled!!\n=> Only terrains overlapping with the currently set area can be added", MessageType.Warning);
            EditorGUI.BeginChangeCheck();
            GameObject newTerrain = (GameObject)EditorGUILayout.ObjectField("Add terrain", null, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (newTerrain != null)
                {
                    bool hasInterface = false;
                    MonoBehaviour[] list = newTerrain.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour mb in list)
                        if (mb is IVegetationStudioTerrain)
                        {
                            vegetationSystemPro.AddTerrain(mb.gameObject);
                            hasInterface = true;
                        }

                    if (!hasInterface)
                        EditorUtility.DisplayDialog("Add terrain", "No valid terrain type script found on the GameObject\n\n" +
                            "Add one of the following components:\nUnityTerrain / MeshTerrain / RaycastTerrain\nOr a script with a valid \"IVegetationStudioTerrain\" interface implementation", "OK");
                    SetSceneDirty();
                }
            }
            GUILayout.EndVertical();

            if (vegetationSystemPro.showTerrainBatchMenu = VegetationPackageEditorTools.DrawHeader("Terrain batch tools", vegetationSystemPro.showTerrainBatchMenu))
            {
                GUILayout.BeginVertical("box");
                if (vegetationSystemPro.automaticBoundsCalculation == false)
                    EditorGUILayout.HelpBox("\"Automatic calculation\" is disabled!!\n=> Only terrains overlapping with the currently set area can be added", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Add unity terrains"))
                {
                    vegetationSystemPro.AddAllUnityTerrains();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Add mesh terrains"))
                {
                    vegetationSystemPro.AddAllMeshTerrains();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Add raycast terrains"))
                {
                    vegetationSystemPro.AddAllRaycastTerrains();
                    SetSceneDirty();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Remove unity terrains"))
                {
                    vegetationSystemPro.RemoveAllUnityTerrains();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Remove mesh terrains"))
                {
                    vegetationSystemPro.RemoveAllMeshTerrains();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Remove raycast terrains"))
                {
                    vegetationSystemPro.RemoveAllRaycastTerrains();
                    SetSceneDirty();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Remove all terrains"))
                {
                    vegetationSystemPro.RemoveAllTerrains(false);
                    SetSceneDirty();
                }

                if (GUILayout.Button("Remove all empty terrains"))
                {
                    vegetationSystemPro.RemoveAllTerrains(true);
                    SetSceneDirty();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Some terrain streaming setups require to remove all terrains before entering play mode / exporting a build\nTo automate this use the \"TerrainStreamingLoader\" component", MessageType.Info);

                EditorGUILayout.Space();
                if (GUILayout.Button("Refresh data of ALL added mesh terrains"))
                {
                    for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainObjectList.Count; i++)
                        if (vegetationSystemPro.vegetationStudioTerrainObjectList[i] != null && vegetationSystemPro.vegetationStudioTerrainObjectList[i].TryGetComponent(out MeshTerrain _meshTerrain))
                        {
                            _meshTerrain.GenerateMeshTerrain();
                            _meshTerrain.NeedGeneration = false;
                            vegetationSystemPro.ClearCache(_meshTerrain.TerrainBounds);
                        }

                    SetSceneDirty();
                }

                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showTerrainListMenu = VegetationPackageEditorTools.DrawHeader("Terrain list", vegetationSystemPro.showTerrainListMenu))
            {
                GUILayout.BeginVertical("Box");
                if (vegetationSystemPro.vegetationStudioTerrainObjectList.Count == 0)
                    EditorGUILayout.HelpBox("The list is empty", MessageType.Info);
                else
                {
                    vegetationSystemPro.RefreshTerrainInterfaceList();
                    for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainObjectList.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        GameObject terrainGO = (GameObject)EditorGUILayout.ObjectField((vegetationSystemPro.vegetationStudioTerrainList[i]?.TerrainType ?? "Unkown") + ":", vegetationSystemPro.vegetationStudioTerrainObjectList[i], typeof(GameObject), true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (terrainGO == null)  // on "delete" UI interaction
                                vegetationSystemPro.RemoveTerrain(vegetationSystemPro.vegetationStudioTerrainObjectList[i]);
                            SetSceneDirty();
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showTerrainAreaMenu = VegetationPackageEditorTools.DrawHeader("Vegetation system total area", vegetationSystemPro.showTerrainAreaMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("For terrain streaming setups disable the automatic calculation after all desired terrains got added\nTo automate this use the \"TerrainStreamingLoader\" component", MessageType.Info);
                EditorGUILayout.HelpBox("Changing the total area moves vegetation instance positions and requires re-initialization of the used persistent storage", MessageType.Warning);

                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.automaticBoundsCalculation = EditorGUILayout.Toggle("Automatic calculation", vegetationSystemPro.automaticBoundsCalculation);
                if (EditorGUI.EndChangeCheck())
                {
                    if (vegetationSystemPro.automaticBoundsCalculation)
                        vegetationSystemPro.CalculateVegetationSystemBounds();
                    SetSceneDirty();
                }

                if (vegetationSystemPro.automaticBoundsCalculation)
                    if (GUILayout.Button("Recalculate"))
                    {
                        vegetationSystemPro.CalculateVegetationSystemBounds();
                        SetSceneDirty();
                    }

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(vegetationSystemPro.automaticBoundsCalculation);
                vegetationSystemPro.vegetationSystemBounds = EditorGUILayout.BoundsField("Total area", vegetationSystemPro.vegetationSystemBounds);
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                    SetSceneDirty();

                if (vegetationSystemPro.automaticBoundsCalculation == false)
                    if (GUILayout.Button("Refresh"))
                    {
                        vegetationSystemPro.RefreshCellSystem();
                        SetSceneDirty();
                    }

                GUILayout.EndVertical();
            }
        }

        void DrawBiomesInspector()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            VegetationPackagePro newVegetationPackagePro = (VegetationPackagePro)EditorGUILayout.ObjectField("Add vegetation package", null, typeof(VegetationPackagePro), true);
            EditorGUILayout.HelpBox("Added vegetation packages act as biomes\nChoose from a list of biome type presets to use them with biome masks\nSet up names to identify biomes and a sort order to layer biomes", MessageType.Info);
            EditorGUILayout.HelpBox("Biome types other than \"Default\" need to be used with a \"BiomeMask\" component", MessageType.Warning);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                if (newVegetationPackagePro != null)
                {
                    if (newVegetationPackagePro.PackageName == "" || newVegetationPackagePro.PackageName == " " || newVegetationPackagePro.PackageName == "No name"
                    || newVegetationPackagePro.PackageName == null || newVegetationPackagePro.PackageName.Length == 0)
                    {
                        newVegetationPackagePro.PackageName = newVegetationPackagePro.name;
                        EditorUtility.SetDirty(newVegetationPackagePro);
                    }

                    vegetationSystemPro.AddVegetationPackage(newVegetationPackagePro);
                    vegetationSystemPro.RefreshItemSystem();
                    SetSceneDirty();
                    return;
                }

            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                GUILayout.BeginVertical("box");

                if (GUILayout.Button("Remove biome", GUILayout.Width(120)))
                {
                    vegetationSystemPro.RemoveVegetationPackage(vegetationSystemPro.vegetationPackageProList[i]);
                    vegetationSystemPro.RefreshItemSystem();
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.vegetationPackageProList[i] = (VegetationPackagePro)EditorGUILayout.ObjectField("Vegetation package", vegetationSystemPro.vegetationPackageProList[i], typeof(VegetationPackagePro), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (vegetationSystemPro.vegetationPackageProList[i] == null)
                        vegetationSystemPro.RemoveVegetationPackage(vegetationSystemPro.vegetationPackageProList[i]);

                    if (vegetationSystemPro.vegetationPackageProList[i].PackageName == "" || vegetationSystemPro.vegetationPackageProList[i].PackageName == " " || vegetationSystemPro.vegetationPackageProList[i].PackageName == "No name"
                        || vegetationSystemPro.vegetationPackageProList[i].PackageName == null || vegetationSystemPro.vegetationPackageProList[i].PackageName.Length == 0)
                    {
                        vegetationSystemPro.vegetationPackageProList[i].PackageName = vegetationSystemPro.vegetationPackageProList[i].name;
                        EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                    }

                    vegetationSystemPro.RefreshItemSystem();
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.vegetationPackageProList[i].BiomeType = (BiomeType)EditorGUILayout.EnumPopup("Biome type", vegetationSystemPro.vegetationPackageProList[i].BiomeType);
                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.ClearCache();
                    EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                    SetSceneDirty();
                }

                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.vegetationPackageProList[i].PackageName = EditorGUILayout.TextField("Biome name", vegetationSystemPro.vegetationPackageProList[i].PackageName);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                    SetSceneDirty();
                }

                if (vegetationSystemPro.vegetationPackageProList[i].BiomeType != BiomeType.Default)
                {
                    EditorGUI.BeginChangeCheck();
                    vegetationSystemPro.vegetationPackageProList[i].BiomeSortOrder = EditorGUILayout.IntSlider("Biome sort order", vegetationSystemPro.vegetationPackageProList[i].BiomeSortOrder, 1, 30);
                    if (EditorGUI.EndChangeCheck())
                    {
                        vegetationSystemPro.ClearCache();
                        EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        SetSceneDirty();
                    }
                }

                GUILayout.EndVertical();
            }
        }

        void DrawTextureMasksInspector()
        {
            GUILayout.BeginVertical("box");
            string[] packageNameList = new string[vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                if (vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " + vegetationSystemPro.vegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";

            EditorGUI.BeginChangeCheck();
            vegetationSystemPro.selectedVegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", vegetationSystemPro.selectedVegetationPackageIndex, packageNameList);

            EditorGUILayout.HelpBox("Select the biome to edit texture masks for\nAny kind of image can be used as a texture mask" +
                "\nUse the \"Mask creator\" tools to create advanced masks\nImport existing masks from third part tools or that are hand drawn", MessageType.Info);
            GUILayout.EndVertical();

            if (vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            if (vegetationSystemPro.selectedVegetationPackageIndex > vegetationSystemPro.vegetationPackageProList.Count - 1)
                vegetationSystemPro.selectedVegetationPackageIndex = vegetationSystemPro.vegetationPackageProList.Count - 1;

            VegetationPackagePro vegetationPackagePro = vegetationSystemPro.vegetationPackageProList[vegetationSystemPro.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null)
            {
                vegetationSystemPro.debugTextureMask = null;
                return;
            }

            EditorGUIUtility.labelWidth = 150;
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Create mask group"))
            {
                vegetationPackagePro.TextureMaskGroupList.Add(new());
                vegetationSystemPro.selectedTextureMaskGroupIndex = vegetationPackagePro.TextureMaskGroupList.Count - 1;
            }

            EditorGUILayout.HelpBox("Multiple mask groups can be created\nEach mask group has its own set of texture masks\nEvery mask within a group works additively!", MessageType.Info);

            if (vegetationPackagePro.TextureMaskGroupList.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no texture masks stored in this vegetation package", MessageType.Info);
                vegetationSystemPro.debugTextureMask = null;
                GUILayout.EndVertical();
                return;
            }

            List<string> maskList = new();
            for (int i = 0; i < vegetationPackagePro.TextureMaskGroupList.Count; i++)
                maskList.Add((i + 1) + ". " + vegetationPackagePro.TextureMaskGroupList[i].TextureMaskName);

            if (vegetationSystemPro.selectedTextureMaskGroupIndex > vegetationPackagePro.TextureMaskGroupList.Count - 1)
                vegetationSystemPro.selectedTextureMaskGroupIndex = vegetationPackagePro.TextureMaskGroupList.Count - 1;

            vegetationSystemPro.selectedTextureMaskGroupIndex = EditorGUILayout.Popup("Selected mask group", vegetationSystemPro.selectedTextureMaskGroupIndex, maskList.ToArray());
            TextureMaskGroup textureMaskGroup = vegetationPackagePro.TextureMaskGroupList[vegetationSystemPro.selectedTextureMaskGroupIndex];
            if (textureMaskGroup == null)
            {
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            textureMaskGroup.TextureMaskName = EditorGUILayout.TextField("Name", textureMaskGroup.TextureMaskName);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            if (GUILayout.Button("Delete mask group", GUILayout.Width(150)))
            {
                if (EditorUtility.DisplayDialog("Confirm action", "Delete texture mask group?", "Confirm", "Cancel"))
                {
                    vegetationPackagePro.DeleteTextureMaskGroup(textureMaskGroup);
                    vegetationSystemPro.selectedTextureMaskGroupIndex = 0;
                    vegetationSystemPro.ClearCache(vegetationPackagePro);
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            Texture2D newTexture = (Texture2D)EditorGUILayout.ObjectField("Add texture mask", null, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (newTexture != null)
                    if (VerifyTextureMask(newTexture))
                    {
                        Rect rect = new(0, 0, newTexture.width, newTexture.height);
                        textureMaskGroup.TextureMaskList.Add(new(newTexture, rect));
                        vegetationSystemPro.ClearCache(RectExtension.CreateBoundsFromRect(rect));
                    }

                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            if (textureMaskGroup.TextureMaskList.Count == 0)
            {
                GUILayout.EndVertical();
                vegetationSystemPro.debugTextureMask = null;
                return;
            }

            if (vegetationSystemPro.selectedTextureMaskGroupTextureIndex > textureMaskGroup.TextureMaskList.Count - 1)
                vegetationSystemPro.selectedTextureMaskGroupTextureIndex = 0;

            GUIContent[] textureImageButtons = new GUIContent[textureMaskGroup.TextureMaskList.Count];
            for (int i = 0; i < textureMaskGroup.TextureMaskList.Count; i++)
                textureImageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(textureMaskGroup.TextureMaskList[i].MaskTexture) };

            int imageWidth = 70;
            int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
            int rows = (int)math.ceil((float)textureImageButtons.Length / columns);
            int gridHeight = (rows) * imageWidth;

            EditorGUI.BeginChangeCheck();
            if (columns > 0)
                vegetationSystemPro.selectedTextureMaskGroupTextureIndex = GUILayout.SelectionGrid(vegetationSystemPro.selectedTextureMaskGroupTextureIndex, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(vegetationPackagePro);

            TextureMask textureMask = textureMaskGroup.TextureMaskList[vegetationSystemPro.selectedTextureMaskGroupTextureIndex];
            vegetationSystemPro.debugTextureMask = textureMask;

            if (textureMask.MaskTexture != null)
                EditorGUILayout.LabelField("Selected mask", textureMask.MaskTexture.name);
            else
                EditorGUILayout.HelpBox("Texture is null and has likely been deleted", MessageType.Warning);

            if (GUILayout.Button("Remove texture", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Confirm action", "Remove texture mask from texture mask group?", "Confirm", "Cancel"))
                {
                    textureMaskGroup.TextureMaskList.Remove(textureMask);
                    vegetationSystemPro.selectedTextureMaskGroupTextureIndex = 0;
                    vegetationSystemPro.ClearCache(RectExtension.CreateBoundsFromRect(textureMask.TextureRect));
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

            EditorGUI.BeginChangeCheck();
            Rect oldRect = textureMask.TextureRect;
            textureMask.TextureRect = EditorGUILayout.RectField("Texture area", textureMask.TextureRect);
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(RectExtension.CreateBoundsFromRect(oldRect));
                vegetationSystemPro.ClearCache(RectExtension.CreateBoundsFromRect(textureMask.TextureRect));
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();
            textureMask.Repeat = EditorGUILayout.Vector2Field("Repeat in area", textureMask.Repeat);
            if (EditorGUI.EndChangeCheck())
            {
                textureMask.Repeat = new float2(math.max(textureMask.Repeat.x, 1), math.max(textureMask.Repeat.y, 1));
                vegetationSystemPro.ClearCache(RectExtension.CreateBoundsFromRect(textureMask.TextureRect));
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }

            GUILayout.BeginHorizontal();
            string[] terrains = new string[vegetationSystemPro.vegetationStudioTerrainObjectList.Count];
            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainObjectList.Count; i++)
            {
                if (vegetationSystemPro.vegetationStudioTerrainObjectList[i] == null)
                    continue;

                if (vegetationSystemPro.vegetationStudioTerrainObjectList[i].transform.parent != null)
                    terrains[i] = vegetationSystemPro.vegetationStudioTerrainObjectList[i].transform.parent.name + " - " + vegetationSystemPro.vegetationStudioTerrainObjectList[i].name;
                else
                    terrains[i] = string.Format("{000}", i) + " - " + vegetationSystemPro.vegetationStudioTerrainObjectList[i].name;
            }

            selectedTerrainIndex = EditorGUILayout.Popup("Selected terrain", selectedTerrainIndex, terrains);
            if (GUILayout.Button("Snap to terrain", GUILayout.Width(100)))
            {
                Bounds bounds = vegetationSystemPro.vegetationStudioTerrainList[selectedTerrainIndex].TerrainBounds;
                textureMask.TextureRect = new Rect(new float2(bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z), new float2(bounds.size.z, bounds.size.z));
                vegetationSystemPro.ClearCache(RectExtension.CreateBoundsFromRect(oldRect));
                vegetationSystemPro.ClearCache(bounds);
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to world area"))
            {
                textureMask.TextureRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationSystemBounds);
                vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }
            EditorGUILayout.HelpBox("Set the world area the texture mask is affecting\nThis should be set 1:1 to what the mask got created/painted for", MessageType.Info);
            EditorGUILayout.HelpBox("Use the \"Debug\" tab to visualize the area of the selected texture mask", MessageType.Warning);
            GUILayout.EndVertical();
            EditorGUIUtility.labelWidth = 200;
        }

        bool VerifyTextureMask(Texture2D _texture)
        {
            if (_texture.format != TextureFormat.RGBA32)
            {
                EditorUtility.DisplayDialog("Texture format error", "Texture format is not set to " + TextureFormat.RGBA32, "OK");
                return false;
            }

            AssetUtility.SetTextureReadable(_texture);
            return true;
        }

        #region Edit biomes section
        void DrawEditItemsInspector()
        {
            GUILayout.BeginVertical("box");
            string[] packageNameList = new string[vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                if (vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " + vegetationSystemPro.vegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";

            vegetationSystemPro.selectedVegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", vegetationSystemPro.selectedVegetationPackageIndex, packageNameList);
            EditorGUILayout.HelpBox("Select the biome to configure items for\nChanges apply dynamically to the scene\nBiome settings get saved on disk into the vegetation package file", MessageType.Info);
            GUILayout.EndVertical();

            if (vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            if (vegetationSystemPro.selectedVegetationPackageIndex > vegetationSystemPro.vegetationPackageProList.Count - 1)
                vegetationSystemPro.selectedVegetationPackageIndex = vegetationSystemPro.vegetationPackageProList.Count - 1;

            VegetationPackagePro vegetationPackagePro = vegetationSystemPro.vegetationPackageProList[vegetationSystemPro.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null)
                return;

            if (DrawVegetationItemDropZone(vegetationPackagePro))
                return;

            if (vegetationPackagePro.VegetationInfoList.Count == 0)
                return;

            if (vegetationSystemPro.showSelectedVegetationItemMenu = VegetationPackageEditorTools.DrawHeader("Select vegetation item", vegetationSystemPro.showSelectedVegetationItemMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.selectedVegetationTypeIndex = GUILayout.SelectionGrid(vegetationSystemPro.selectedVegetationTypeIndex, vegetationTypeNames, 3, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.selectedGridIndex = 0;
                int selectionCount = 0;
                VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationSystemPro, vegetationPackagePro, ref vegetationSystemPro.selectedGridIndex, ref selectedVegetationItemIndex, ref selectionCount,
                    VegetationPackageEditorTools.GetVegetationItemTypeSelection(vegetationSystemPro.selectedVegetationTypeIndex), 70);
                GUILayout.EndVertical();

                if (lastVegegetationItemIndex != selectedVegetationItemIndex)
                {
                    GUI.FocusControl(null);
                    scaleCurveEditor.selectedCurve = null;
                    scaleCurveEditor.selectedKeyframeIndex = -1;
                    heightCurveEditor.selectedCurve = null;
                    heightCurveEditor.selectedKeyframeIndex = -1;
                    steepnessCurveEditor.selectedCurve = null;
                    steepnessCurveEditor.selectedKeyframeIndex = -1;
                    distanceFalloffCurveEditor.selectedCurve = null;
                    distanceFalloffCurveEditor.selectedKeyframeIndex = -1;
                }
                lastVegegetationItemIndex = selectedVegetationItemIndex;

                if (vegetationPackagePro.VegetationInfoList.Count == 0 || selectedVegetationItemIndex >= vegetationPackagePro.VegetationInfoList.Count || selectionCount == 0)
                    return;

                vegetationSystemPro.vegetationItemInfoProEditor = vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex];

                DrawActionMenu(vegetationSystemPro, vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor); // delete/copy buttons
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Selected item", vegetationSystemPro.vegetationItemInfoProEditor != null ? vegetationSystemPro.vegetationItemInfoProEditor.Name : "None");
            vegetationSystemPro.selectedVegetationItemSubTab = GUILayout.SelectionGrid(vegetationSystemPro.selectedVegetationItemSubTab, vegetationSubTabNames, 2, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            if (vegetationPackagePro.VegetationInfoList.Count == 0 || vegetationSystemPro.vegetationItemInfoProEditor == null)
                return; // return when no items in the vegetation package -- when the selected item is null

            if (vegetationSystemPro.selectedVegetationItemSubTab == 0)
            {
                DrawVegetationItemSettingsMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
                DrawLODMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
                DrawShaderSettingsMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor);
                DrawDistanceFalloffMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
                DrawBillboardSettings(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor);
                DrawColliderSettingsMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor);
                return;
            }

            DrawPositionRotationScaleMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawHeightSteepnessMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawNoiseSettingMenu(vegetationPackagePro, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawTextureMaskRulesMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawBiomeMaskRulesMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawVegetationMaskRulesMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawTerrainTextureRulesMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawConcaveLocationRuleMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
            DrawTerrainSourceSettingsMenu(vegetationPackagePro, vegetationSystemPro.vegetationItemInfoProEditor, vegetationSystemPro.selectedVegetationPackageIndex, selectedVegetationItemIndex);
        }

        bool DrawVegetationItemDropZone(VegetationPackagePro _vegetationPackagePro)
        {
            vegetationSystemPro.showAddVegetationItemMenu = VegetationPackageEditorTools.DrawHeader("Add vegetation item", vegetationSystemPro.showAddVegetationItemMenu);
            if (vegetationSystemPro.showAddVegetationItemMenu == false)
                return false;

            bool addedItem = false;
            GUILayout.BeginVertical("box");
            if (VegetationStudioManager.GetVegetationItemFromClipboard() != null)
                if (GUILayout.Button("Paste vegetation item"))
                {
                    _vegetationPackagePro.DuplicateVegetationItem(VegetationStudioManager.GetVegetationItemFromClipboard());
                    addedItem = true;
                }

            EditorGUILayout.HelpBox("Drop a prefab/texture here to create a new vegetation item\n2D vegetation items use an optimized \"2D-Plane-Mesh\"\nEach category has their own preset values", MessageType.Info);

            GUILayout.BeginHorizontal();
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.GrassPrefab, _vegetationPackagePro, ref addedItem);
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.PlantPrefab, _vegetationPackagePro, ref addedItem);
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.ObjectPrefab, _vegetationPackagePro, ref addedItem);
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.LargeObjectPrefab, _vegetationPackagePro, ref addedItem);
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.TreePrefab, _vegetationPackagePro, ref addedItem);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.GrassTexture, _vegetationPackagePro, ref addedItem);
            DropZoneTools.DrawVegetationItemDropZone(DropZoneType.PlantTexture, _vegetationPackagePro, ref addedItem);
            GUILayout.EndHorizontal();

            if (addedItem)
            {
                selectedVegetationItemIndex = _vegetationPackagePro.VegetationInfoList.Count - 1;
                vegetationSystemPro.selectedGridIndex = GetSelectedGridIndex(selectedVegetationItemIndex, _vegetationPackagePro);
                vegetationSystemPro.RefreshItemSystem();
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
            GUILayout.EndVertical();
            return addedItem;
        }

        int GetSelectedGridIndex(int _vegIndex, VegetationPackagePro _vegetationPackagePro)
        {
            List<int> vegetationItemIndexList = new();
            for (int i = 0; i < _vegetationPackagePro.VegetationInfoList.Count; i++)
                vegetationItemIndexList.Add(i);

            VegetationInfoComparer vIc = new() { vegetationInfoList = _vegetationPackagePro.VegetationInfoList };
            vegetationItemIndexList.Sort(vIc.Compare);

            for (int i = 0; i < vegetationItemIndexList.Count; i++)
                if (_vegetationPackagePro.VegetationInfoList[vegetationItemIndexList[i]].VegetationItemID == _vegetationPackagePro.VegetationInfoList[_vegIndex].VegetationItemID)
                    return i;
            return 0;
        }

        void DrawActionMenu(VegetationSystemPro _vegetationSystemPro, VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro)
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete selected item"))
                if (EditorUtility.DisplayDialog("Delete vegetation item?", "Delete the selected vegetation item?", "Delete", "Cancel"))
                {
                    _vegetationPackagePro.VegetationInfoList.RemoveAt(selectedVegetationItemIndex);
                    EditorUtility.SetDirty(_vegetationPackagePro);
                    if (_vegetationSystemPro.persistentVegetationStorage && _vegetationSystemPro.persistentVegetationStorage.persistentVegetationStoragePackage)
                    {
                        _vegetationSystemPro.persistentVegetationStorage.RemoveVegetationItemInstances(_vegetationItemInfoPro.VegetationItemID);
                        EditorUtility.SetDirty(_vegetationSystemPro.persistentVegetationStorage.persistentVegetationStoragePackage);
                    }
                    _vegetationSystemPro.RefreshItemSystem();
                    SetSceneDirty();
                    selectedVegetationItemIndex = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    return;
                }

            if (GUILayout.Button("Copy selected item"))
            {
                VegetationStudioManager.AddVegetationItemToClipboard(_vegetationItemInfoPro);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void DrawVegetationItemSettingsMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showVegetationItemSettingsMenu = VegetationPackageEditorTools.DrawHeader("General settings", vegetationSystemPro.showVegetationItemSettingsMenu);
            if (vegetationSystemPro.showVegetationItemSettingsMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation type: " + _vegetationItemInfoPro.VegetationType, labelStyle);

            if (_vegetationItemInfoPro.PrefabType == VegetationPrefabType.Mesh)
            {
                if (_vegetationItemInfoPro.VegetationPrefab == null)
                    EditorGUILayout.HelpBox("The vegetation prefab is missing and got probably deleted after it has been added", MessageType.Error);

                _vegetationItemInfoPro.VegetationPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _vegetationItemInfoPro.VegetationPrefab, typeof(GameObject), false);
            }
            else
            {
                if (_vegetationItemInfoPro.VegetationTexture == null)
                    EditorGUILayout.HelpBox("The vegetation texture is missing and got probably deleted after it has been added", MessageType.Error);

                _vegetationItemInfoPro.VegetationTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", _vegetationItemInfoPro.VegetationTexture, typeof(Texture2D), false);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _vegetationItemInfoPro.ShaderControllerSettings = null;  // reset material settings

                // reset prefab by type / reset name
                if (_vegetationItemInfoPro.PrefabType == VegetationPrefabType.Mesh && _vegetationItemInfoPro.VegetationPrefab != null)
                {
                    _vegetationItemInfoPro.Name = _vegetationItemInfoPro.VegetationPrefab.name;
                    _vegetationItemInfoPro.VegetationTexture = null; // reset texture since using a prefab
                }
                if (_vegetationItemInfoPro.PrefabType == VegetationPrefabType.Texture && _vegetationItemInfoPro.VegetationTexture != null)
                {
                    _vegetationItemInfoPro.Name = _vegetationItemInfoPro.VegetationTexture.name;
                    _vegetationItemInfoPro.VegetationPrefab = null;  // reset prefab since using a texture
                }

#if UNITY_EDITOR    // set GUID to match its prefab
                _vegetationItemInfoPro.VegetationGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_vegetationItemInfoPro.VegetationPrefab ? _vegetationItemInfoPro.VegetationPrefab : _vegetationItemInfoPro.VegetationTexture));
#endif

                vegetationSystemPro.RefreshItemSystem();
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.Name = EditorGUILayout.TextField("Name", _vegetationItemInfoPro.Name);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            if (GUILayout.Button("Refresh vegetation item"))
            {
                vegetationSystemPro.RefreshItemSystem();
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUILayout.HelpBox("Always refresh the vegetation item whenever changes have been made to the prefab\nCustom material settings are reset to the original ones!", MessageType.Info);

            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.EnableRuntimeSpawn = EditorGUILayout.Toggle("Enable run-time spawning", _vegetationItemInfoPro.EnableRuntimeSpawn);
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.DisableShadows = EditorGUILayout.Toggle("Disable shadows", _vegetationItemInfoPro.DisableShadows);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();
            switch (_vegetationItemInfoPro.VegetationRenderMode = (VegetationRenderMode)EditorGUILayout.EnumPopup("Render mode", _vegetationItemInfoPro.VegetationRenderMode))
            {
                case VegetationRenderMode.Instanced:
                    EditorGUILayout.HelpBox("Instanced is the default render mode balancing speed vs compatibility", MessageType.Info);
                    break;
                case VegetationRenderMode.Normal:
                    EditorGUILayout.HelpBox("Use normal as a fallback mode for limited hardware and shaders", MessageType.Info);
                    break;
                case VegetationRenderMode.InstancedIndirect:
                    EditorGUILayout.HelpBox("Instanced indirect is the fastest mode but needs additional integration into custom shaders\n" +
                        "This mode is only active while in play mode or in a build", MessageType.Info);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.RestartVegetationSystem();
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.RenderDistanceFactor = EditorGUILayout.Slider("Local distance factor", _vegetationItemInfoPro.RenderDistanceFactor, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            EditorGUILayout.HelpBox("The global render distance is set in the \"Rendering\" tab\nEffective render distance: "
                + (vegetationSystemPro.vegetationSettings.GetVegetationItemCullDistance(_vegetationItemInfoPro) + vegetationSystemPro.vegetationSettings.crossFadeDistance) +
                "\nThe min/max render distance is affected by the \"Crossfade distance\"", MessageType.Info);

            GUILayout.EndVertical();
        }

        void DrawPositionRotationScaleMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showPositionRotationScaleMenu = VegetationPackageEditorTools.DrawHeader("Density/Position/Rotation/Scale", vegetationSystemPro.showPositionRotationScaleMenu);
            if (vegetationSystemPro.showPositionRotationScaleMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box"); // density, sample distance/offset
            _vegetationItemInfoPro.Seed = EditorGUILayout.IntSlider("Local seed", _vegetationItemInfoPro.Seed, 0, 100);
            _vegetationItemInfoPro.Density = EditorGUILayout.Slider("Local density", _vegetationItemInfoPro.Density, 0, 1);
            _vegetationItemInfoPro.SampleDistance = EditorGUILayout.Slider("Sample distance", _vegetationItemInfoPro.SampleDistance, _vegetationItemInfoPro.VegetationType == VegetationType.Tree ? 2.4f : 0.4f, 50);
            if (_vegetationItemInfoPro.UseSamplePointOffset = EditorGUILayout.Toggle("Use sample point offset", _vegetationItemInfoPro.UseSamplePointOffset))
                EditorFunctions.FloatRangeField("Sample point offset", ref _vegetationItemInfoPro.SamplePointMinOffset, ref _vegetationItemInfoPro.SamplePointMaxOffset, 0.1f, 20);
            EditorGUILayout.HelpBox("Use the sample point offset to create grouped instances\nThe different items need to use the same local seed and sample distance\nDecimal values of the minimum add rotation", MessageType.Info);
            GUILayout.EndVertical();    // end of density, sample distance/offset

            GUILayout.BeginVertical("box"); // pos, rot, scale
            _vegetationItemInfoPro.RandomizePosition = EditorGUILayout.Toggle("Randomize position", _vegetationItemInfoPro.RandomizePosition);
            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.Offset = EditorGUILayout.Vector3Field("Position offset", _vegetationItemInfoPro.Offset);
            EditorFunctions.FloatRangeField("Position offset range", ref _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].MinUpOffset, ref _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].MaxUpOffset, -10, 10);
            EditorGUILayout.HelpBox("Position offsets are affected by scale\nThe XYZ-Rotation gets handled differently by absolute vs range", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling

            _vegetationItemInfoPro.RotationMode = (VegetationRotationType)EditorGUILayout.EnumPopup("Rotation mode", _vegetationItemInfoPro.RotationMode);
            _vegetationItemInfoPro.RotationOffset = EditorGUILayout.Vector3Field("Rotation offset", _vegetationItemInfoPro.RotationOffset);

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.ScaleMultiplier = math.max(EditorGUILayout.Vector3Field("Scale multiplier", _vegetationItemInfoPro.ScaleMultiplier), float3.zero);
            EditorFunctions.FloatRangeField("Min/Max scale", ref _vegetationPackagePro.VegetationInfoList[_vegetationItemIndex].MinScale, ref _vegetationPackagePro.VegetationInfoList[_vegetationItemIndex].MaxScale, 0.1f, 10);
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.useAdvancedScaleRule = EditorGUILayout.Toggle("Advanced scale", _vegetationItemInfoPro.useAdvancedScaleRule);
            if (EditorGUI.EndChangeCheck())
            {
                scaleCurveEditor.selectedCurve = null;
                scaleCurveEditor.selectedKeyframeIndex = -1;
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeScaleRuleCurve();
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
            }

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            if (_vegetationItemInfoPro.useAdvancedScaleRule)
                if (scaleCurveEditor.EditCurve(_vegetationItemInfoPro.scaleRuleCurve, this))
                {
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeScaleRuleCurve();
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                    EditorUtility.SetDirty(_vegetationPackagePro);
                    SetSceneDirty();
                }

            EditorGUI.BeginChangeCheck();

            Keyframe selectedKeyScale = scaleCurveEditor.GetSelection().Keyframe ?? new();
            if (scaleCurveEditor.GetSelection().Keyframe != null)
            {
                int index = scaleCurveEditor.GetSelection().KeyframeIndex;
                float time = math.round(EditorGUILayout.Slider("Scale min", selectedKeyScale.time, 0, 1) * 100) / 100;
                float value = math.round(EditorGUILayout.Slider("Scale max", selectedKeyScale.value, 0, 1) * 100) / 100;
                float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeyScale.inTangent, -5, 5) * 100) / 100;
                float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeyScale.outTangent, -5, 5) * 100) / 100;
                if (index > 0)
                    time = math.max(_vegetationItemInfoPro.scaleRuleCurve.keys[index - 1].time + 0.0001f, time);    // safety "clamp" else keys delete themselves when having the same value
                if (index < _vegetationItemInfoPro.scaleRuleCurve.keys.Length - 1)
                    time = math.min(_vegetationItemInfoPro.scaleRuleCurve.keys[index + 1].time - 0.0001f, time);    // safety "clamp" else keys delete themselves when having the same value
                _vegetationItemInfoPro.scaleRuleCurve.MoveKey(index, new Keyframe(time, value, inTangent, outTangent));
            }

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeScaleRuleCurve();
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            GUILayout.EndVertical();    // end of pos, rot, scale
        }

        void DrawHeightSteepnessMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showHeightSteepnessMenu = VegetationPackageEditorTools.DrawHeader("Height/Steepness rules", vegetationSystemPro.showHeightSteepnessMenu);
            if (vegetationSystemPro.showHeightSteepnessMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box"); // height rule

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.UseHeightRule = EditorGUILayout.Toggle("Use height rule", _vegetationItemInfoPro.UseHeightRule);
            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
                heightCurveEditor.selectedCurve = null;
                heightCurveEditor.selectedKeyframeIndex = -1;
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            }

            if (_vegetationItemInfoPro.UseHeightRule)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(_vegetationItemInfoPro.UseAdvancedHeightRule);
                EditorFunctions.FloatRangeField("Min/Max height above sea level", ref _vegetationItemInfoPro.MinHeight, ref _vegetationItemInfoPro.MaxHeight, -500f, 10000);
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);

                EditorGUI.BeginChangeCheck();
                _vegetationItemInfoPro.UseAdvancedHeightRule = EditorGUILayout.Toggle("Advanced", _vegetationItemInfoPro.UseAdvancedHeightRule);
                if (EditorGUI.EndChangeCheck())
                {
                    GUI.FocusControl(null);
                    heightCurveEditor.selectedCurve = null;
                    heightCurveEditor.selectedKeyframeIndex = -1;
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                }

                if (_vegetationItemInfoPro.UseAdvancedHeightRule)
                {
                    EditorGUI.BeginChangeCheck();
                    _vegetationItemInfoPro.MaxCurveHeight = EditorGUILayout.Slider("Max curve height", _vegetationItemInfoPro.MaxCurveHeight, 1, 2000);
                    heightCurveEditor.SeaLevel = vegetationSystemPro.SeaLevel;
                    heightCurveEditor.MaxValue = _vegetationItemInfoPro.MaxCurveHeight;
                    if (EditorGUI.EndChangeCheck())
                        vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);

                    if (heightCurveEditor.EditCurve(_vegetationItemInfoPro.HeightRuleCurve, this))
                    {
                        vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeHeightRuleCurve();
                        vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();

            Keyframe selectedKeyHeight = heightCurveEditor.GetSelection().Keyframe ?? new();
            if (heightCurveEditor.GetSelection().Keyframe != null)
            {
                int index = heightCurveEditor.GetSelection().KeyframeIndex;
                float time = math.round(EditorGUILayout.Slider("Meter above sea level", selectedKeyHeight.time * _vegetationItemInfoPro.MaxCurveHeight, 0, _vegetationItemInfoPro.MaxCurveHeight) * 100) / 100;
                float value = math.round(EditorGUILayout.Slider("Density", selectedKeyHeight.value, 0, 1) * 100) / 100;
                float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeyHeight.inTangent, -5, 5) * 100) / 100;
                float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeyHeight.outTangent, -5, 5) * 100) / 100;
                if (index > 0)
                    time = math.max((_vegetationItemInfoPro.HeightRuleCurve.keys[index - 1].time + 0.0001f) * _vegetationItemInfoPro.MaxCurveHeight, time); // safety "clamp" else keys delete themselves when having the same value
                if (index < _vegetationItemInfoPro.HeightRuleCurve.keys.Length - 1)
                    time = math.min((_vegetationItemInfoPro.HeightRuleCurve.keys[index + 1].time - 0.0001f) * _vegetationItemInfoPro.MaxCurveHeight, time); // safety "clamp" else keys delete themselves when having the same value
                _vegetationItemInfoPro.HeightRuleCurve.MoveKey(index, new Keyframe(time / _vegetationItemInfoPro.MaxCurveHeight, value, inTangent, outTangent));
            }

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeHeightRuleCurve();
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box"); // steepness rule

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.UseSteepnessRule = EditorGUILayout.Toggle("Use steepness rule", _vegetationItemInfoPro.UseSteepnessRule);
            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
                steepnessCurveEditor.selectedCurve = null;
                steepnessCurveEditor.selectedKeyframeIndex = -1;
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            }

            if (_vegetationItemInfoPro.UseSteepnessRule)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(_vegetationItemInfoPro.UseAdvancedSteepnessRule);
                EditorFunctions.FloatRangeField("Min/Max steepness", ref _vegetationItemInfoPro.MinSteepness, ref _vegetationItemInfoPro.MaxSteepness, 0f, 90);
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);

                EditorGUI.BeginChangeCheck();
                _vegetationItemInfoPro.UseAdvancedSteepnessRule = EditorGUILayout.Toggle("Advanced", _vegetationItemInfoPro.UseAdvancedSteepnessRule);
                if (EditorGUI.EndChangeCheck())
                {
                    GUI.FocusControl(null);
                    steepnessCurveEditor.selectedCurve = null;
                    steepnessCurveEditor.selectedKeyframeIndex = -1;
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                }

                if (_vegetationItemInfoPro.UseAdvancedSteepnessRule)
                    if (steepnessCurveEditor.EditCurve(_vegetationItemInfoPro.SteepnessRuleCurve, this))
                    {
                        vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeSteepnessRuleCurve();
                        vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                    }
            }

            EditorGUI.BeginChangeCheck();

            Keyframe selectedKeySteepness = steepnessCurveEditor.GetSelection().Keyframe ?? new();
            if (steepnessCurveEditor.GetSelection().Keyframe != null)
            {
                int index = steepnessCurveEditor.GetSelection().KeyframeIndex;
                float time = math.round(EditorGUILayout.Slider("Steepness min", selectedKeySteepness.time * 90, 0, 90) * 100) / 100;
                float value = math.round(EditorGUILayout.Slider("Steepness max", selectedKeySteepness.value, 0, 1) * 100) / 100;
                float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeySteepness.inTangent, -5, 5) * 100) / 100;
                float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeySteepness.outTangent, -5, 5) * 100) / 100;
                if (index > 0)
                    time = math.max((_vegetationItemInfoPro.SteepnessRuleCurve.keys[index - 1].time + 0.0001f) * 90, time); // safety "clamp" else keys delete themselves when having the same value
                if (index < _vegetationItemInfoPro.SteepnessRuleCurve.keys.Length - 1)
                    time = math.min((_vegetationItemInfoPro.SteepnessRuleCurve.keys[index + 1].time - 0.0001f) * 90, time); // safety "clamp" else keys delete themselves when having the same value
                _vegetationItemInfoPro.SteepnessRuleCurve.MoveKey(index, new Keyframe(time / 90, value, inTangent, outTangent));
            }

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeSteepnessRuleCurve();
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawNoiseSettingMenu(VegetationPackagePro _vegetationPackagePro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showVegetationPackageNoiseMenu = VegetationPackageEditorTools.DrawHeader("Noise rules", vegetationSystemPro.showVegetationPackageNoiseMenu);
            if (vegetationSystemPro.showVegetationPackageNoiseMenu == false)
                return;

            EditorGUILayout.HelpBox("For custom/other noise types consider using the \"Texture mask rules\"", MessageType.Info);
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseNoiseCutoff = EditorGUILayout.Toggle("Use perlin noise cutoff", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseNoiseCutoff))
            {
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffValue = EditorGUILayout.Slider("Perlin noise cutoff", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffValue, 0, 1);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffScale = EditorGUILayout.Slider("Perlin noise scale", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffScale, 1, 500);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffOffset = EditorGUILayout.Vector2Field("Perlin noise offset", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffOffset);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffInverse = EditorGUILayout.Toggle("Invert perlin noise", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseCutoffInverse);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseNoiseDensity = EditorGUILayout.Toggle("Use perlin noise density", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseNoiseDensity))
            {
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityScale = EditorGUILayout.Slider("Perlin noise scale", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityScale, 1, 500);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityBalancing = EditorGUILayout.Slider("Perlin noise balancing", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityBalancing, -1, 1);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityOffset = EditorGUILayout.Vector2Field("Perlin noise offset", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityOffset);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityInverse = EditorGUILayout.Toggle("Invert perlin noise", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseDensityInverse);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseNoiseScaleRule = EditorGUILayout.Toggle("Use perlin noise scale", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseNoiseScaleRule))
            {
                EditorFunctions.FloatRangeField("Min/Max scale", ref _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleMinScale, ref _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleMaxScale, 0.1f, 5);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleScale = EditorGUILayout.Slider("Perlin noise scale", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleScale, 1, 500);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleBalancing = EditorGUILayout.Slider("Perlin noise balancing", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleBalancing, -1, 1);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleOffset = EditorGUILayout.Vector2Field("Perlin noise offset", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleOffset);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleInverse = EditorGUILayout.Toggle("Invert perlin noise", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].NoiseScaleInverse);
            }
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawLODMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showLODMenu = VegetationPackageEditorTools.DrawHeader("LOD settings", vegetationSystemPro.showLODMenu);
            if (vegetationSystemPro.showLODMenu == false)
                return;

            VegetationItemModelInfo vegItemModelInfo = vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex);
            if (vegItemModelInfo == null)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("No run-time model info available\nEither the system is not enabled or it has not been correctly setup", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Number of LODs: " + vegItemModelInfo.lodCount + "/4", labelStyle);
            _vegetationItemInfoPro.EnableCrossFade = EditorGUILayout.Toggle("Enable LOD crossfade", _vegetationItemInfoPro.EnableCrossFade);

            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID)?.RefreshMaterials();

            if (vegItemModelInfo.lodCount < 2)
            {
                EditorGUILayout.HelpBox("Prefab has no \"LOD Group\" component or too few LOD levels added", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            _vegetationItemInfoPro.LODFactor = EditorGUILayout.Slider("Local LOD distance factor", _vegetationItemInfoPro.LODFactor, 0f, 2f);
            float currentLOD1Distance = vegItemModelInfo.lod0To1Distance * QualitySettings.lodBias * _vegetationItemInfoPro.LODFactor;
            float currentLOD2Distance = vegItemModelInfo.lod1To2Distance * QualitySettings.lodBias * _vegetationItemInfoPro.LODFactor;
            float currentLOD3Distance = vegItemModelInfo.lod2To3Distance * QualitySettings.lodBias * _vegetationItemInfoPro.LODFactor;
            VegetationPackageEditorTools.DrawLODRanges(vegItemModelInfo.lodCount, currentLOD1Distance, currentLOD2Distance, currentLOD3Distance, vegetationSystemPro.vegetationSettings.GetVegetationItemCullDistance(_vegetationItemInfoPro, false));
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawShaderSettingsMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro)
        {
            vegetationSystemPro.showShaderSettingsMenu = VegetationPackageEditorTools.DrawHeader("Material settings", vegetationSystemPro.showShaderSettingsMenu);
            if (vegetationSystemPro.showShaderSettingsMenu == false)
                return;

            if (vegetationSystemPro.showShaderSettingsMaterials = EditorGUILayout.Toggle("Show materials", vegetationSystemPro.showShaderSettingsMaterials))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Mode: " + (_vegetationItemInfoPro.useShaderControllerOverrides ? "Isolated" : "Shared"), labelStyle);
                VegetationItemModelInfo vegItemModelInfo = vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID);
                if (vegItemModelInfo == null)
                    EditorGUILayout.HelpBox("No run-time model info available\nEither the system is not enabled or it has not been correctly setup", MessageType.Warning);
                else
                {
                    EditorGUI.BeginDisabledGroup(true); // show the user the materials are "system internal"
                    for (int i = 0; i < vegItemModelInfo.vegetationMaterialsLOD0.Length; i++)
                        _ = (Material)EditorGUILayout.ObjectField("LOD: 0 -- Material: " + i, vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID).vegetationMaterialsLOD0[i], typeof(Material), false);
                    if (vegItemModelInfo.lodCount < 2) goto endOfMatPreview;
                    EditorGUILayout.Space();
                    for (int i = 0; i < vegItemModelInfo.vegetationMaterialsLOD1.Length; i++)
                        _ = (Material)EditorGUILayout.ObjectField("LOD: 1 -- Material: " + i, vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID).vegetationMaterialsLOD1[i], typeof(Material), false);
                    if (vegItemModelInfo.lodCount < 3) goto endOfMatPreview;
                    EditorGUILayout.Space();
                    for (int i = 0; i < vegItemModelInfo.vegetationMaterialsLOD2.Length; i++)
                        _ = (Material)EditorGUILayout.ObjectField("LOD: 2 -- Material: " + i, vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID).vegetationMaterialsLOD2[i], typeof(Material), false);
                    if (vegItemModelInfo.lodCount < 4) goto endOfMatPreview;
                    EditorGUILayout.Space();
                    for (int i = 0; i < vegItemModelInfo.vegetationMaterialsLOD3.Length; i++)
                        _ = (Material)EditorGUILayout.ObjectField("LOD: 3 -- Material: " + i, vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID).vegetationMaterialsLOD3[i], typeof(Material), false);
                    endOfMatPreview:    // goto label to skip UI that isn't needed => effectively only the "Space()" UI
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.HelpBox("Edit: Right click > \"Properties\"\nSave: Right click top-bar of material preview > \"Copy Material Properties\"", MessageType.Info);
                }
                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showShaderSettingsControllers = EditorGUILayout.Toggle("Show shader controller/-s", vegetationSystemPro.showShaderSettingsControllers))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Create shader controllers using the \"IShaderController\" interface\nThis is optional to control materials isolated, per vegetation item\n" +
                    "It is recommended to setup a basic shader controller to synchronize billboards", MessageType.Info);

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(_vegetationItemInfoPro.PrefabType == VegetationPrefabType.Texture);
                _vegetationItemInfoPro.useShaderControllerOverrides = EditorGUILayout.Toggle("Use shader controller overrides", _vegetationItemInfoPro.useShaderControllerOverrides);
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID)?.ClearMaterials();
                    vegetationSystemPro.RefreshItemSystem();
                    EditorUtility.SetDirty(_vegetationPackagePro);
                    SetSceneDirty();
                }

                if (GUILayout.Button("Reset material settings"))
                {
                    if (EditorUtility.DisplayDialog("Confirm action", "Reset settings of all materials on this vegetation item?", "Confirm", "Cancel"))
                    {
                        _vegetationItemInfoPro.ShaderControllerSettings = null;
                        vegetationSystemPro.RefreshItemSystem();
                        EditorUtility.SetDirty(_vegetationPackagePro);
                        SetSceneDirty();
                    }
                }
                GUILayout.EndVertical();

                for (int i = 0; i < _vegetationItemInfoPro.ShaderControllerSettings?.Length; i++)
                {
                    if (_vegetationItemInfoPro.ShaderControllerSettings[i] == null || _vegetationItemInfoPro.ShaderControllerSettings[i].controllerPropertyList.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No valid shader controller found for the material at index: " + i, MessageType.Info);
                        continue;
                    }

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(_vegetationItemInfoPro.ShaderControllerSettings[i].heading, labelStyle);
                    for (int j = 0; j < _vegetationItemInfoPro.ShaderControllerSettings[i].controllerPropertyList.Count; j++)
                        DrawSerializedProperty(_vegetationItemInfoPro.ShaderControllerSettings[i].controllerPropertyList[j]);
                    GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID)?.RefreshMaterials();
                        EditorUtility.SetDirty(_vegetationPackagePro);
                        SetSceneDirty();
                    }
                }
            }
        }

        void DrawSerializedProperty(SerializedControllerProperty serializedControllerProperty)
        {
            switch (serializedControllerProperty.SerializedControlerPropertyType)
            {
                case SerializedControlerPropertyType.Integer:
                    serializedControllerProperty.IntValue = EditorGUILayout.IntSlider(serializedControllerProperty.PropertyDescription, serializedControllerProperty.IntValue, serializedControllerProperty.IntMinValue, serializedControllerProperty.IntMaxValue);
                    break;
                case SerializedControlerPropertyType.Float:
                    serializedControllerProperty.FloatValue = EditorGUILayout.Slider(serializedControllerProperty.PropertyDescription, serializedControllerProperty.FloatValue, serializedControllerProperty.FloatMinValue, serializedControllerProperty.FloatMaxValue);
                    break;
                case SerializedControlerPropertyType.RgbaSelector:
                    DrawRgbaChannelSelector(serializedControllerProperty);
                    break;
                case SerializedControlerPropertyType.DropDownStringList:
                    DropdownStringListSelector(serializedControllerProperty);
                    break;
                case SerializedControlerPropertyType.Boolean:
                    serializedControllerProperty.BoolValue = EditorGUILayout.Toggle(serializedControllerProperty.PropertyDescription, serializedControllerProperty.BoolValue);
                    break;
                case SerializedControlerPropertyType.ColorSelector:
                    serializedControllerProperty.ColorValue = EditorGUILayout.ColorField(serializedControllerProperty.PropertyDescription, serializedControllerProperty.ColorValue);
                    break;
                case SerializedControlerPropertyType.Label:
                    EditorGUILayout.LabelField(serializedControllerProperty.PropertyDescription, labelStyle);
                    break;
                case SerializedControlerPropertyType.Texture2D:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(serializedControllerProperty.PropertyDescription, labelStyle);
                    serializedControllerProperty.Texture2DValue = (Texture2D)EditorGUILayout.ObjectField(serializedControllerProperty.Texture2DValue, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            if (string.IsNullOrEmpty(serializedControllerProperty.PropertyInfo) == false)
                EditorGUILayout.HelpBox(serializedControllerProperty.PropertyInfo, MessageType.Info);
        }

        void DropdownStringListSelector(SerializedControllerProperty serializedControllerProperty)
        {
            serializedControllerProperty.IntValue = EditorGUILayout.Popup(serializedControllerProperty.PropertyDescription, serializedControllerProperty.IntValue, serializedControllerProperty.StringList.ToArray());
        }

        void DrawRgbaChannelSelector(SerializedControllerProperty serializedControllerProperty)
        {
            serializedControllerProperty.IntValue = EditorGUILayout.Popup(serializedControllerProperty.PropertyDescription, serializedControllerProperty.IntValue, rgbaChannelNames);
        }

        void DrawDistanceFalloffMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (_vegetationItemInfoPro.VegetationType == VegetationType.Tree || _vegetationItemInfoPro.VegetationType == VegetationType.LargeObjects)
                return;

            vegetationSystemPro.showDistanceFalloffMenu = VegetationPackageEditorTools.DrawHeader("Distance falloff", vegetationSystemPro.showDistanceFalloffMenu);
            if (vegetationSystemPro.showDistanceFalloffMenu == false)
                return;

            GUILayout.BeginVertical("box");

            EditorGUI.BeginChangeCheck();
            _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseDistanceFalloff = EditorGUILayout.Toggle("Use distance falloff", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseDistanceFalloff);
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseDistanceFalloff)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginDisabledGroup(_vegetationItemInfoPro.UseAdvancedDistanceFalloff);
                _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].DistanceFalloffStartDistance = EditorGUILayout.Slider("Distance falloff factor", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].DistanceFalloffStartDistance, 0.01f, 1);
                EditorGUILayout.HelpBox("Starts at max render distance declining linearly using the distance factor in a randomized pattern" +
                    "\nStart distance: " + (vegetationSystemPro.vegetationSettings.GetVegetationItemCullDistance(_vegetationItemInfoPro) + vegetationSystemPro.vegetationSettings.crossFadeDistance) +
                    "\nEnd distance: " + (vegetationSystemPro.vegetationSettings.GetVegetationItemCullDistance(_vegetationItemInfoPro) * _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].DistanceFalloffStartDistance
                    + vegetationSystemPro.vegetationSettings.crossFadeDistance) + "\nThe min/max render distance is affected by the \"Crossfade distance\"", MessageType.Info);
                EditorGUI.EndDisabledGroup();

                _vegetationItemInfoPro.UseAdvancedDistanceFalloff = EditorGUILayout.Toggle("Advanced", _vegetationItemInfoPro.UseAdvancedDistanceFalloff);

                if (EditorGUI.EndChangeCheck())
                {
                    distanceFalloffCurveEditor.selectedCurve = null;
                    distanceFalloffCurveEditor.selectedKeyframeIndex = -1;
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                    EditorUtility.SetDirty(_vegetationPackagePro);
                    SetSceneDirty();
                }

                if (_vegetationItemInfoPro.UseAdvancedDistanceFalloff)
                    if (distanceFalloffCurveEditor.EditCurve(_vegetationItemInfoPro.distanceFalloffCurve, this))
                    {
                        vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeDistanceFalloffCurve();
                        vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                        EditorUtility.SetDirty(_vegetationPackagePro);
                        SetSceneDirty();
                    }

                EditorGUI.BeginChangeCheck();

                Keyframe selectedKeyDistanceFalloff = distanceFalloffCurveEditor.GetSelection().Keyframe ?? new();
                if (distanceFalloffCurveEditor.GetSelection().Keyframe != null)
                {
                    int index = distanceFalloffCurveEditor.GetSelection().KeyframeIndex;
                    float time = math.round(EditorGUILayout.Slider("Distance", selectedKeyDistanceFalloff.time, 0, 1) * 100) / 100;
                    float value = math.round(EditorGUILayout.Slider("Density", selectedKeyDistanceFalloff.value, 0, 1) * 100) / 100;
                    float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeyDistanceFalloff.inTangent, -5, 5) * 100) / 100;
                    float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeyDistanceFalloff.outTangent, -5, 5) * 100) / 100;
                    if (index > 0)
                        time = math.max(_vegetationItemInfoPro.distanceFalloffCurve.keys[index - 1].time + 0.0001f, time);  // safety "clamp" else keys delete themselves when having the same value
                    if (index < _vegetationItemInfoPro.distanceFalloffCurve.keys.Length - 1)
                        time = math.min(_vegetationItemInfoPro.distanceFalloffCurve.keys[index + 1].time - 0.0001f, time);  // safety "clamp" else keys delete themselves when having the same value
                    _vegetationItemInfoPro.distanceFalloffCurve.MoveKey(index, new Keyframe(time, value, inTangent, outTangent));
                }

                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.GenerateNativeDistanceFalloffCurve();
                    vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                    EditorUtility.SetDirty(_vegetationPackagePro);
                    SetSceneDirty();
                }
            }

            GUILayout.EndVertical();
        }

        void DrawBillboardSettings(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro)
        {
            if (_vegetationItemInfoPro.VegetationType != VegetationType.Tree)   //&& vegetationItemInfoPro.VegetationType != VegetationType.Grass && vegetationItemInfoPro.VegetationType != VegetationType.Plant)
                return;

            vegetationSystemPro.showBillboardsMenu = VegetationPackageEditorTools.DrawHeader("Billboards", vegetationSystemPro.showBillboardsMenu);
            if (vegetationSystemPro.showBillboardsMenu == false)
                return;

            GUILayout.BeginVertical("box");

            EditorGUI.BeginChangeCheck();
            _vegetationItemInfoPro.UseBillboards = EditorGUILayout.Toggle("Enable billboards", _vegetationItemInfoPro.UseBillboards);
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationItemInfoPro.VegetationItemID);
                vegetationSystemPro.RefreshItemSystem();
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            if (_vegetationItemInfoPro.UseBillboards == false)
            {
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();

            if (_vegetationItemInfoPro.UseBillboards)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("General settings", labelStyle);
                _vegetationItemInfoPro.BillboardShadowOffset = EditorGUILayout.Slider("3D-Mode shadow bias", _vegetationItemInfoPro.BillboardShadowOffset, 0, 10);
                _vegetationItemInfoPro.BillboardFadeOutDistance = EditorGUILayout.Slider("Crossfade cull distance", _vegetationItemInfoPro.BillboardFadeOutDistance, 0, 20);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Material settings", labelStyle);
                _vegetationItemInfoPro.BillboardCutoff = EditorGUILayout.Slider("Alpha clipping", _vegetationItemInfoPro.BillboardCutoff, 0, 1);
                _vegetationItemInfoPro.BillboardBrightness = EditorGUILayout.Slider("Brightness", _vegetationItemInfoPro.BillboardBrightness, 0, 5);
                _vegetationItemInfoPro.BillboardNormalStrength = EditorGUILayout.Slider("Normal scale", _vegetationItemInfoPro.BillboardNormalStrength, 0, 2);
                _vegetationItemInfoPro.BillboardSpecular = EditorGUILayout.Slider("Specular power", _vegetationItemInfoPro.BillboardSpecular, 0, 1);
                _vegetationItemInfoPro.BillboardOcclusion = EditorGUILayout.Slider("Occlusion power", _vegetationItemInfoPro.BillboardOcclusion, 0, 1);
                EditorGUILayout.HelpBox("Custom shaders with features like \"wind, hue variation, snow\" can sync mesh and billboards using the \"IShaderController\" interface\nDetailed settings can be found in the \"Weather\" tab", MessageType.Info);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID)?.UpdateBillboardMaterial();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Texture atlas settings", labelStyle);
                _vegetationItemInfoPro.BillboardSourceLODLevel = (LODLevel)EditorGUILayout.EnumPopup("Atlas LOD source", _vegetationItemInfoPro.BillboardSourceLODLevel);
                _vegetationItemInfoPro.BillboardRecalculateNormals = EditorGUILayout.Toggle("Recalculate LOD source normals", _vegetationItemInfoPro.BillboardRecalculateNormals);
                if (_vegetationItemInfoPro.BillboardRecalculateNormals)
                    _vegetationItemInfoPro.BillboardNormalBlendFactor = EditorGUILayout.Slider("Mesh normal blend factor", _vegetationItemInfoPro.BillboardNormalBlendFactor, 0, 1);
                _vegetationItemInfoPro.eBillboardAtlasColorSource = (EBillboardAtlasColorSource)EditorGUILayout.EnumPopup("Atlas color source", _vegetationItemInfoPro.eBillboardAtlasColorSource);
                _vegetationItemInfoPro.BillboardQuality = (BillboardQuality)EditorGUILayout.EnumPopup("Atlas tiling_resolution", _vegetationItemInfoPro.BillboardQuality);

                if (GUILayout.Button("Regenerate billboard"))
                {
                    _vegetationPackagePro.GenerateBillboard(_vegetationItemInfoPro.VegetationItemID);
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID)?.CreateBillboardMaterial();
                }

                EditorGUI.BeginChangeCheck();
                _vegetationItemInfoPro.BillboardTexture = EditorGUILayout.ObjectField("Billboard atlas texture", _vegetationItemInfoPro.BillboardTexture, typeof(Texture2D), true) as Texture2D;
                _vegetationItemInfoPro.BillboardNormalTexture = EditorGUILayout.ObjectField("Billboard atlas normal texture", _vegetationItemInfoPro.BillboardNormalTexture, typeof(Texture2D), true) as Texture2D;
                if (EditorGUI.EndChangeCheck())
                    vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID)?.CreateBillboardMaterial();

                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }

            GUILayout.EndVertical();
        }

        void DrawColliderSettingsMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro)
        {
            if (_vegetationItemInfoPro.VegetationType == VegetationType.Grass || _vegetationItemInfoPro.VegetationType == VegetationType.Plant)
                return;

            vegetationSystemPro.showColliderRulesMenu = VegetationPackageEditorTools.DrawHeader("Colliders", vegetationSystemPro.showColliderRulesMenu);
            if (vegetationSystemPro.showColliderRulesMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            _vegetationItemInfoPro.ColliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider type", _vegetationItemInfoPro.ColliderType);
            if (_vegetationItemInfoPro.ColliderType == ColliderType.Disabled)
            {
                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.RefreshColliderSystem();
                    EditorUtility.SetDirty(_vegetationPackagePro);
                    SetSceneDirty();
                }
                GUILayout.EndVertical();
                return;
            }

            switch (_vegetationItemInfoPro.ColliderType) // collider type specific fields
            {
                case ColliderType.Capsule:
                    _vegetationItemInfoPro.ColliderRadius = math.max(EditorGUILayout.FloatField("Radius", _vegetationItemInfoPro.ColliderRadius), 0);
                    _vegetationItemInfoPro.ColliderHeight = math.max(EditorGUILayout.FloatField("Height", _vegetationItemInfoPro.ColliderHeight), 0);
                    _vegetationItemInfoPro.ColliderOffset = EditorGUILayout.Vector3Field("Offset", _vegetationItemInfoPro.ColliderOffset);
                    break;
                case ColliderType.Sphere:
                    _vegetationItemInfoPro.ColliderRadius = math.max(EditorGUILayout.FloatField("Radius", _vegetationItemInfoPro.ColliderRadius), 0);
                    _vegetationItemInfoPro.ColliderOffset = EditorGUILayout.Vector3Field("Offset", _vegetationItemInfoPro.ColliderOffset);
                    break;
                case ColliderType.Box:
                    _vegetationItemInfoPro.ColliderSize = math.max(EditorGUILayout.Vector3Field("Size", _vegetationItemInfoPro.ColliderSize), float3.zero);
                    _vegetationItemInfoPro.ColliderOffset = EditorGUILayout.Vector3Field("Offset", _vegetationItemInfoPro.ColliderOffset);
                    break;
                case ColliderType.CustomMesh:
                    _vegetationItemInfoPro.ColliderMesh = (Mesh)EditorGUILayout.ObjectField("Custom mesh", _vegetationItemInfoPro.ColliderMesh, typeof(Mesh), false);
                    _vegetationItemInfoPro.ColliderConvex = EditorGUILayout.Toggle("Convex", _vegetationItemInfoPro.ColliderConvex);
                    break;
                case ColliderType.Mesh:
                    _vegetationItemInfoPro.ColliderConvex = EditorGUILayout.Toggle("Convex", _vegetationItemInfoPro.ColliderConvex);
                    break;
            }

            if (_vegetationItemInfoPro.ColliderType != ColliderType.FromPrefab)  // from prefab only use "tag" and "distance factor"
            {
                EditorGUI.BeginDisabledGroup(_vegetationItemInfoPro.ColliderConvex == false && (_vegetationItemInfoPro.ColliderType == ColliderType.Mesh || _vegetationItemInfoPro.ColliderType == ColliderType.CustomMesh));
                _vegetationItemInfoPro.ColliderTrigger = EditorGUILayout.Toggle("Trigger", _vegetationItemInfoPro.ColliderTrigger);
                EditorGUI.EndDisabledGroup();
            }

            _vegetationItemInfoPro.ColliderTag = EditorGUILayout.TagField("Tag", _vegetationItemInfoPro.ColliderTag);

            _vegetationItemInfoPro.ColliderDistanceFactor = EditorGUILayout.Slider("Local distance factor", _vegetationItemInfoPro.ColliderDistanceFactor, 0, 1);
            float currentDistance = vegetationSystemPro.vegetationSettings.GetGrassDistance() * _vegetationItemInfoPro.ColliderDistanceFactor;  // based on grass distance
            EditorGUILayout.HelpBox("The distance from the camera in which colliders get generated\nThis is based on the global grass render distance" + "\nGeneration distance: " + currentDistance.ToString("F2"), MessageType.Info);
            GUILayout.EndVertical();    // end of colliders

            GUILayout.BeginVertical("box"); // start of nav mesh
            _vegetationItemInfoPro.ColliderUseForBake = EditorGUILayout.Toggle("Include in NavMesh bake", _vegetationItemInfoPro.ColliderUseForBake);
            EditorGUI.BeginDisabledGroup(_vegetationItemInfoPro.ColliderUseForBake == false);
            _vegetationItemInfoPro.NavMeshArea = EditorGUILayout.Popup("Navigation Area", _vegetationItemInfoPro.NavMeshArea, navAreas);
            EditorGUI.EndDisabledGroup();

            _vegetationItemInfoPro.NavMeshObstacleType = (NavMeshObstacleType)EditorGUILayout.EnumPopup("NavMesh Obstacle Type", _vegetationItemInfoPro.NavMeshObstacleType);
            switch (_vegetationItemInfoPro.NavMeshObstacleType)
            {
                case NavMeshObstacleType.Box:
                    _vegetationItemInfoPro.NavMeshObstacleCenter = EditorGUILayout.Vector3Field("Center", _vegetationItemInfoPro.NavMeshObstacleCenter);
                    _vegetationItemInfoPro.NavMeshObstacleSize = EditorGUILayout.Vector3Field("Size", _vegetationItemInfoPro.NavMeshObstacleSize);
                    _vegetationItemInfoPro.NavMeshObstacleCarve = EditorGUILayout.Toggle("Carve", _vegetationItemInfoPro.NavMeshObstacleCarve);
                    break;
                case NavMeshObstacleType.Capsule:
                    _vegetationItemInfoPro.NavMeshObstacleCenter = EditorGUILayout.Vector3Field("Center", _vegetationItemInfoPro.NavMeshObstacleCenter);
                    _vegetationItemInfoPro.NavMeshObstacleRadius = EditorGUILayout.FloatField("Radius", _vegetationItemInfoPro.NavMeshObstacleRadius);
                    _vegetationItemInfoPro.NavMeshObstacleHeight = EditorGUILayout.FloatField("Height", _vegetationItemInfoPro.NavMeshObstacleHeight);
                    _vegetationItemInfoPro.NavMeshObstacleCarve = EditorGUILayout.Toggle("Carve", _vegetationItemInfoPro.NavMeshObstacleCarve);
                    break;
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.RefreshColliderSystem();
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawBiomeMaskRulesMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showBiomeRulesMenu = VegetationPackageEditorTools.DrawHeader("Biome mask rules", vegetationSystemPro.showBiomeRulesMenu);
            if (vegetationSystemPro.showBiomeRulesMenu == false)
                return;

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            if (_vegetationItemInfoPro.UseBiomeEdgeIncludeRule = EditorGUILayout.Toggle("Use biome edge include rule", _vegetationItemInfoPro.UseBiomeEdgeIncludeRule))
            {
                _vegetationItemInfoPro.BiomeEdgeIncludeDistance = EditorGUILayout.Slider("Distance from edge", _vegetationItemInfoPro.BiomeEdgeIncludeDistance, 0, 500);
                _vegetationItemInfoPro.BiomeEdgeIncludeInverse = EditorGUILayout.Toggle("Invert", _vegetationItemInfoPro.BiomeEdgeIncludeInverse);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            if (_vegetationItemInfoPro.UseBiomeEdgeScaleRule = EditorGUILayout.Toggle("Use biome edge scale rule", _vegetationItemInfoPro.UseBiomeEdgeScaleRule))
            {
                _vegetationItemInfoPro.BiomeEdgeScaleDistance = EditorGUILayout.Slider("Distance from edge", _vegetationItemInfoPro.BiomeEdgeScaleDistance, 0, 500);
                EditorFunctions.FloatRangeField("Min/Max scale", ref _vegetationItemInfoPro.BiomeEdgeScaleMinScale, ref _vegetationItemInfoPro.BiomeEdgeScaleMaxScale, 0.1f, 5);
                _vegetationItemInfoPro.BiomeEdgeScaleInverse = EditorGUILayout.Toggle("Invert", _vegetationItemInfoPro.BiomeEdgeScaleInverse);
            }
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawVegetationMaskRulesMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showVegetationMaskRulesMenu = VegetationPackageEditorTools.DrawHeader("Vegetation mask rules", vegetationSystemPro.showVegetationMaskRulesMenu);
            if (vegetationSystemPro.showVegetationMaskRulesMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("This limits the vegetation item to only spawn within vegetation masks!", MessageType.Warning);
            if (_vegetationItemInfoPro.UseVegetationMask = EditorGUILayout.Toggle("Use with vegetation masks", _vegetationItemInfoPro.UseVegetationMask))
                _vegetationItemInfoPro.VegetationTypeIndex = (VegetationTypeIndex)EditorGUILayout.EnumPopup("Vegetation type", _vegetationItemInfoPro.VegetationTypeIndex);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawTextureMaskRulesMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showTextureMaskRulesMenu = VegetationPackageEditorTools.DrawHeader("Texture mask rules", vegetationSystemPro.showTextureMaskRulesMenu);
            if (vegetationSystemPro.showTextureMaskRulesMenu == false)
                return;

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskIncludeRules = EditorGUILayout.Toggle("Use texture mask include rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskIncludeRules))
                DrawTextureMaskRules(_vegetationPackagePro, TextureMaskRuleType.Include, _vegetationItemInfoPro.TextureMaskIncludeRuleList, ref includeTextureMaskIndex, ref includeTextureMaskGroupIndex);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskExcludeRules = EditorGUILayout.Toggle("Use texture mask exclude rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskExcludeRules))
                DrawTextureMaskRules(_vegetationPackagePro, TextureMaskRuleType.Exclude, _vegetationItemInfoPro.TextureMaskExcludeRuleList, ref excludeTextureMaskIndex, ref excludeTextureMaskGroupIndex);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskDensityRules = EditorGUILayout.Toggle("Use texture mask density rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskDensityRules))
                DrawTextureMaskRules(_vegetationPackagePro, TextureMaskRuleType.Density, _vegetationItemInfoPro.TextureMaskDensityRuleList, ref densityTextureMaskIndex, ref densityTextureMaskGroupIndex);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskScaleRules = EditorGUILayout.Toggle("Use texture mask scale rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTextureMaskScaleRules))
                DrawTextureMaskRules(_vegetationPackagePro, TextureMaskRuleType.Scale, _vegetationItemInfoPro.TextureMaskScaleRuleList, ref scaleTextureMaskIndex, ref scaleTextureMaskGroupIndex);
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawTextureMaskRules(VegetationPackagePro _vegetationPackagePro, TextureMaskRuleType _textureMaskRuleType, List<TextureMaskRule> _textureMaskRuleList, ref int _maskIndex, ref int _maskGroupIndex)
        {
            if (_vegetationPackagePro.TextureMaskGroupList.Count > 0)
            {
                GUILayout.BeginVertical("box");
                List<string> textureMaskGroupStringList = new();
                for (int i = 0; i < _vegetationPackagePro.TextureMaskGroupList.Count; i++)
                    textureMaskGroupStringList.Add((i + 1) + ". " + _vegetationPackagePro.TextureMaskGroupList[i].TextureMaskName);

                if (_maskGroupIndex >= textureMaskGroupStringList.Count)
                    _maskGroupIndex = 0;
                _maskGroupIndex = EditorGUILayout.Popup("Select texture mask group", _maskGroupIndex, textureMaskGroupStringList.ToArray());

                if (GUILayout.Button("Add mask group"))
                {
                    TextureMaskGroup textureMaskGroup = _vegetationPackagePro.TextureMaskGroupList[_maskGroupIndex];
                    TextureMaskRule textureMaskRule = new(textureMaskGroup.Settings, _textureMaskRuleType) { TextureMaskGroupID = textureMaskGroup.TextureMaskGroupID };
                    _textureMaskRuleList.Add(textureMaskRule);
                    _maskIndex = _textureMaskRuleList.Count - 1;
                }
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("A \"TextureMaskGroup\" needs to be setup in the vegetation package before setting up a rule", MessageType.Warning);
            }

            DrawTextureMaskRuleIconSelector(_textureMaskRuleList, _vegetationPackagePro, ref _maskIndex);

            if (_maskIndex < _textureMaskRuleList.Count)
            {
                GUILayout.BeginVertical("box");

                TextureMaskRule textureMaskRule = _textureMaskRuleList[_maskIndex];
                TextureMaskGroup textureMaskGroup = _vegetationPackagePro.GetTextureMaskGroup(textureMaskRule.TextureMaskGroupID);
                if (textureMaskGroup != null)
                    EditorGUILayout.LabelField("Selected mask group", textureMaskGroup.TextureMaskName);
                else
                    EditorGUILayout.HelpBox("The selected texture mask group has been deleted from the vegetation package", MessageType.Warning);

                if (GUILayout.Button("Remove mask group", GUILayout.Width(150)))
                {
                    _textureMaskRuleList.RemoveAt(_maskIndex);
                    GUILayout.EndVertical();
                    return;
                }

                for (int i = 0; i < textureMaskRule.TextureMaskPropertiesList.Count; i++)
                    DrawSerializedProperty(textureMaskRule.TextureMaskPropertiesList[i]);

                if (_textureMaskRuleType == TextureMaskRuleType.Include || _textureMaskRuleType == TextureMaskRuleType.Exclude)
                    EditorFunctions.FloatRangeField("Min/Max mask density", ref textureMaskRule.MinDensity, ref textureMaskRule.MaxDensity, 0.01f, 1);

                if (_textureMaskRuleType == TextureMaskRuleType.Density)
                {
                    textureMaskRule.DensityMultiplier = EditorGUILayout.Slider("Density multiplier", textureMaskRule.DensityMultiplier, 0, 5);
                    textureMaskRule.MinDensity = EditorGUILayout.Slider("Min result (off-texture)", textureMaskRule.MinDensity, 0.1f, 1);
                    textureMaskRule.MaxDensity = EditorGUILayout.Slider("Max result (on-texture)", textureMaskRule.MaxDensity, 0.1f, 1);
                }

                if (_textureMaskRuleType == TextureMaskRuleType.Scale)
                {
                    textureMaskRule.ScaleMultiplier = EditorGUILayout.Slider("Scale multiplier", textureMaskRule.ScaleMultiplier, 0.1f, 5);
                    textureMaskRule.MinDensity = EditorGUILayout.Slider("Min result (off-texture)", textureMaskRule.MinDensity, 0.1f, 5);
                    textureMaskRule.MaxDensity = EditorGUILayout.Slider("Max result (on-texture)", textureMaskRule.MaxDensity, 0.1f, 5);
                }

                if (_textureMaskRuleType == TextureMaskRuleType.Scale || _textureMaskRuleType == TextureMaskRuleType.Density)
                {
                    textureMaskRule.BrightnessThreshold = EditorGUILayout.Slider("Density threshold", textureMaskRule.BrightnessThreshold, 0.01f, 2);
                    EditorFunctions.FloatRangeField("Min/Max mask density", ref textureMaskRule.MinBrightness, ref textureMaskRule.MaxBrightness, 0.01f, 1);
                }

                GUILayout.EndVertical();
            }
        }

        void DrawTextureMaskRuleIconSelector(List<TextureMaskRule> _terrainTextureRuleList, VegetationPackagePro _vegetationPackagePro, ref int _index)
        {
            GUIContent[] textureImageButtons = new GUIContent[_terrainTextureRuleList.Count];

            for (int i = 0; i < _terrainTextureRuleList.Count; i++)
            {
                TextureMaskGroup textureMaskGroup = _vegetationPackagePro.GetTextureMaskGroup(_terrainTextureRuleList[i].TextureMaskGroupID);
                if (textureMaskGroup != null)
                {
                    Texture2D previewTexture = textureMaskGroup.GetPreviewTexture();
                    if (previewTexture == null)
                        textureImageButtons[i] = new GUIContent { image = dummyPreviewTexture };
                    else
                        textureImageButtons[i] = new GUIContent { image = AssetPreview.GetAssetPreview(previewTexture) };
                }
                else
                    textureImageButtons[i] = new GUIContent { image = dummyPreviewTexture };
            }

            if (textureImageButtons.Length > 0)
            {
                int imageWidth = 70;
                int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
                int rows = (int)math.ceil((float)textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;

                if (_index > textureImageButtons.Length - 1)
                    _index = 0;

                if (columns > 0)
                    _index = GUILayout.SelectionGrid(_index, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
            }
        }

        void DrawTerrainTextureRulesMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showTerrainTextureRulesMenu = VegetationPackageEditorTools.DrawHeader("Terrain texture rules", vegetationSystemPro.showTerrainTextureRulesMenu);
            if (vegetationSystemPro.showTerrainTextureRulesMenu == false)
                return;

            EditorGUILayout.HelpBox("The preview textures shown are from the first terrain in the terrain list", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureIncludeRules = EditorGUILayout.Toggle("Use terrain texture include rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureIncludeRules))
                DrawTerrainTextureRule(TextureMaskRuleType.Include, _vegetationItemInfoPro.TerrainTextureIncludeRuleList, ref includeTerrainTextureIndex);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureExcludeRules = EditorGUILayout.Toggle("Use terrain texture exclude rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureExcludeRules))
                DrawTerrainTextureRule(TextureMaskRuleType.Exclude, _vegetationItemInfoPro.TerrainTextureExcludeRuleList, ref excludeTerrainTextureIndex);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureDensityRules = EditorGUILayout.Toggle("Use terrain texture density rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureDensityRules))
                DrawTerrainTextureRule(TextureMaskRuleType.Density, _vegetationItemInfoPro.TerrainTextureDensityRuleList, ref densityTerrainTextureIndex);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureScaleRules = EditorGUILayout.Toggle("Use terrain texture scale rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainTextureScaleRules))
                DrawTerrainTextureRule(TextureMaskRuleType.Scale, _vegetationItemInfoPro.TerrainTextureScaleRuleList, ref scaleTerrainTextureIndex);
            if (EditorGUI.EndChangeCheck())
                vegetationSystemPro.GetVegetationItemModelInfo(_vegetationPackageIndex, _vegetationItemIndex)?.CalculateCellCullingBoundsAddy();    // update due to scale changes -- rendering/cellSampling
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawTerrainTextureRule(TextureMaskRuleType _textureMaskRuleType, List<TerrainTextureRule> _terrainTextureRuleList, ref int _index)
        {
            if (GUILayout.Button("Add new item"))
            {
                _terrainTextureRuleList.Add(new(_textureMaskRuleType));
                _index = _terrainTextureRuleList.Count - 1;
            }

            GUIContent[] textureImageButtons = new GUIContent[_terrainTextureRuleList.Count];
            for (int i = 0; i < _terrainTextureRuleList.Count; i++)
                textureImageButtons[i] = new GUIContent { image = GetTerrainPreviewTexture(_terrainTextureRuleList[i].TextureIndex) };

            if (textureImageButtons.Length > 0)
            {
                int imageWidth = 70;
                int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - 50f) / imageWidth);
                int rows = (int)math.ceil((float)textureImageButtons.Length / columns);
                int gridHeight = (rows) * imageWidth;

                if (_index > textureImageButtons.Length - 1)
                    _index = 0;

                if (columns > 0)
                    _index = GUILayout.SelectionGrid(_index, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

                GUILayout.BeginVertical("box");
                if (GUILayout.Button("Delete selected item"))
                {
                    _terrainTextureRuleList.RemoveAt(_index);
                    GUILayout.EndVertical();
                    return;
                }

                TerrainTextureRule ruleInfo = _terrainTextureRuleList[_index];
                ruleInfo.TextureIndex = (int)(TerrainTextureType)EditorGUILayout.EnumPopup("Selected texture", (TerrainTextureType)ruleInfo.TextureIndex);

                if (_textureMaskRuleType == TextureMaskRuleType.Include || _textureMaskRuleType == TextureMaskRuleType.Exclude)
                    EditorFunctions.FloatRangeField("Min/Max texture density", ref ruleInfo.MinimumValue, ref ruleInfo.MaximumValue, 0.1f, 1);

                if (_textureMaskRuleType == TextureMaskRuleType.Scale || _textureMaskRuleType == TextureMaskRuleType.Density)
                    ruleInfo.Inverse = EditorGUILayout.Toggle("Invert", ruleInfo.Inverse);

                if (_textureMaskRuleType == TextureMaskRuleType.Density)
                {
                    ruleInfo.DensityMultiplier = EditorGUILayout.Slider("Density multiplier", ruleInfo.DensityMultiplier, 0, 5);
                    ruleInfo.MinimumValue = EditorGUILayout.Slider("Min result (off-texture)", ruleInfo.MinimumValue, 0f, 1);
                    ruleInfo.MaximumValue = EditorGUILayout.Slider("Max result (on-texture)", ruleInfo.MaximumValue, 0f, 1);
                }

                if (_textureMaskRuleType == TextureMaskRuleType.Scale)
                {
                    ruleInfo.ScaleMultiplier = EditorGUILayout.Slider("Scale multiplier", ruleInfo.ScaleMultiplier, 0.1f, 5);
                    ruleInfo.MinimumValue = EditorGUILayout.Slider("Min result (off-texture)", ruleInfo.MinimumValue, 0.1f, 5);
                    ruleInfo.MaximumValue = EditorGUILayout.Slider("Max result (on-texture)", ruleInfo.MaximumValue, 0.1f, 5);
                }

                if (_textureMaskRuleType == TextureMaskRuleType.Scale || _textureMaskRuleType == TextureMaskRuleType.Density)
                {
                    ruleInfo.BrightnessThreshold = EditorGUILayout.Slider("Density threshold", ruleInfo.BrightnessThreshold, 0.1f, 2);
                    EditorFunctions.FloatRangeField("Min/Max texture density", ref ruleInfo.MinBrightness, ref ruleInfo.MaxBrightness, 0.1f, 1);
                }

                GUILayout.EndVertical();
            }
        }

        Texture2D GetTerrainPreviewTexture(int _textureIndex)   // get the terrain textures from the first found "UnityTerrain" => fallback to the next one on ex: exceeding layer count
        {
            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++)
            {
                UnityTerrain unityTerrain = vegetationSystemPro.vegetationStudioTerrainList[i] as UnityTerrain;
                if (unityTerrain == null || unityTerrain.Terrain == null || unityTerrain.Terrain.terrainData == null || unityTerrain.Terrain.terrainData.terrainLayers == null
                    || _textureIndex >= unityTerrain.Terrain.terrainData.terrainLayers.Length || unityTerrain.Terrain.terrainData.terrainLayers[_textureIndex] == null)
                    continue;   // skip on not matching terrain type -- on missing data => try next "UnityTerrain"

                Texture2D previewTexture = AssetPreview.GetAssetPreview(unityTerrain.Terrain.terrainData.terrainLayers[_textureIndex].diffuseTexture);
                if (previewTexture) return previewTexture;  // return if engine is able to get a preview -- on missing data => try next "UnityTerrain"
            }

            return dummyPreviewTexture;
        }

        void DrawConcaveLocationRuleMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showConcaveLocationRulesMenu = VegetationPackageEditorTools.DrawHeader("Concave location rule", vegetationSystemPro.showConcaveLocationRulesMenu);
            if (vegetationSystemPro.showConcaveLocationRulesMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            if (_vegetationItemInfoPro.UseConcaveLocationRule = EditorGUILayout.Toggle("Use concave location rule", _vegetationItemInfoPro.UseConcaveLocationRule))
            {
                _vegetationItemInfoPro.ConcaveLocationDistance = EditorGUILayout.Slider("Distance per sample", _vegetationItemInfoPro.ConcaveLocationDistance, 1f, 20);
                _vegetationItemInfoPro.ConcaveLocationMinHeightDifference = EditorGUILayout.Slider("Min height difference", _vegetationItemInfoPro.ConcaveLocationMinHeightDifference, 0.1f, 20);
                _vegetationItemInfoPro.ConcaveLocationInverse = EditorGUILayout.Toggle("Invert", _vegetationItemInfoPro.ConcaveLocationInverse);
            }
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }

        void DrawTerrainSourceSettingsMenu(VegetationPackagePro _vegetationPackagePro, VegetationItemInfoPro _vegetationItemInfoPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            vegetationSystemPro.showTerrainSourceSettingsMenu = VegetationPackageEditorTools.DrawHeader("Terrain source rules", vegetationSystemPro.showTerrainSourceSettingsMenu);
            if (vegetationSystemPro.showTerrainSourceSettingsMenu == false)
                return;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainSourceIncludeRule = EditorGUILayout.Toggle("Use terrain source include rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainSourceIncludeRule))
            {
                EditorGUILayout.LabelField("Select include IDs", labelStyle);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID1 = EditorGUILayout.Toggle("Terrain source ID 1", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID1);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID2 = EditorGUILayout.Toggle("Terrain source ID 2", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID2);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID3 = EditorGUILayout.Toggle("Terrain source ID 3", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID3);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID4 = EditorGUILayout.Toggle("Terrain source ID 4", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID4);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID5 = EditorGUILayout.Toggle("Terrain source ID 5", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID5);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID6 = EditorGUILayout.Toggle("Terrain source ID 6", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID6);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID7 = EditorGUILayout.Toggle("Terrain source ID 7", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID7);
                _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID8 = EditorGUILayout.Toggle("Terrain source ID 8", _vegetationItemInfoPro.TerrainSourceIncludeRule.UseTerrainSourceID8);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (_vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainSourceExcludeRule = EditorGUILayout.Toggle("Use terrain source exclude rule", _vegetationPackagePro.VegetationInfoList[selectedVegetationItemIndex].UseTerrainSourceExcludeRule))
            {
                EditorGUILayout.LabelField("Select exclude IDs", labelStyle);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID1 = EditorGUILayout.Toggle("Terrain source ID 1", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID1);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID2 = EditorGUILayout.Toggle("Terrain source ID 2", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID2);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID3 = EditorGUILayout.Toggle("Terrain source ID 3", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID3);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID4 = EditorGUILayout.Toggle("Terrain source ID 4", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID4);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID5 = EditorGUILayout.Toggle("Terrain source ID 5", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID5);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID6 = EditorGUILayout.Toggle("Terrain source ID 6", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID6);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID7 = EditorGUILayout.Toggle("Terrain source ID 7", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID7);
                _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID8 = EditorGUILayout.Toggle("Terrain source ID 8", _vegetationItemInfoPro.TerrainSourceExcludeRule.UseTerrainSourceID8);
            }
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationSystemPro.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                EditorUtility.SetDirty(_vegetationPackagePro);
                SetSceneDirty();
            }
        }
        #endregion

        void DrawBatchEditInspector()
        {
            #region selectionMenu
            GUILayout.BeginVertical("box");
            // vegetation package list
            string[] packageNameList = new string[vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i <= vegetationSystemPro.vegetationPackageProList.Count - 1; i++)
                if (vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " +
                                         vegetationSystemPro.vegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";

            // vegetation package selection
            vegetationSystemPro.selectedVegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", vegetationSystemPro.selectedVegetationPackageIndex, packageNameList);
            EditorGUILayout.HelpBox("Made changes are irreversible, mind your actions!\nBiome settings get saved on disk into the vegetation package file", MessageType.Warning);

            if (vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            if (vegetationSystemPro.selectedVegetationPackageIndex > vegetationSystemPro.vegetationPackageProList.Count - 1)
                vegetationSystemPro.selectedVegetationPackageIndex = vegetationSystemPro.vegetationPackageProList.Count - 1;

            VegetationPackagePro vegetationPackagePro = vegetationSystemPro.vegetationPackageProList[vegetationSystemPro.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();   // begin checking for changes
                                            // type filter
            vegetationSystemPro.toggleTypeFilterSelection = EditorGUILayout.Toggle("Enable type filter", vegetationSystemPro.toggleTypeFilterSelection);
            if (vegetationSystemPro.toggleTypeFilterSelection)
                vegetationSystemPro.overrideSelectionType = (VegetationType)EditorGUILayout.EnumPopup("Vegetation type to override", vegetationSystemPro.overrideSelectionType);
            else
                EditorGUILayout.HelpBox("The type filter is disabled, changes apply to ALL vegetation types!", MessageType.Warning);
            if (EditorGUI.EndChangeCheck()) // when changes got made
                SetSceneDirty();    // write made changes also to disk not just memory
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            vegetationSystemPro.selectedVegetationItemSubTab = GUILayout.SelectionGrid(vegetationSystemPro.selectedVegetationItemSubTab, vegetationSubTabNames, 2, EditorStyles.toolbarButton);
            GUILayout.EndVertical();
            #endregion

            if (vegetationSystemPro.selectedVegetationItemSubTab == 0)
            {
                #region batchGeneralSettings
                if (vegetationSystemPro.showGeneralBatchMenu = VegetationPackageEditorTools.DrawHeader("General settings", vegetationSystemPro.showGeneralBatchMenu))
                {
                    GUILayout.BeginVertical("box");
                    EditorGUI.BeginChangeCheck();   // begin checking for changes

                    // runtime state
                    vegetationSystemPro.toggleRuntimeState = EditorGUILayout.Toggle("Enable run-time spawning", vegetationSystemPro.toggleRuntimeState);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].EnableRuntimeSpawn = vegetationSystemPro.toggleRuntimeState;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].EnableRuntimeSpawn = vegetationSystemPro.toggleRuntimeState;
                            vegetationSystemPro.ClearCache(vegetationPackagePro);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].EnableRuntimeSpawn = vegetationSystemPro.toggleRuntimeState;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].EnableRuntimeSpawn = vegetationSystemPro.toggleRuntimeState;
                                vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            SetSceneDirty();
                        }

                    // render mode
                    EditorGUILayout.LabelField("", labelStyle);
                    vegetationSystemPro.overrideRenderMode = (VegetationRenderMode)EditorGUILayout.EnumPopup("Render mode", vegetationSystemPro.overrideRenderMode);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].VegetationRenderMode = vegetationSystemPro.overrideRenderMode;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].VegetationRenderMode = vegetationSystemPro.overrideRenderMode;
                            vegetationSystemPro.RestartVegetationSystem();
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationRenderMode = vegetationSystemPro.overrideRenderMode;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationRenderMode = vegetationSystemPro.overrideRenderMode;
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            vegetationSystemPro.RestartVegetationSystem();
                            SetSceneDirty();
                        }

                    if (EditorGUI.EndChangeCheck()) // when changes got made
                        SetSceneDirty();    // write made changes also to disk not just memory

                    GUILayout.EndVertical();
                }
                #endregion
                #region batchLODSettings
                if (vegetationSystemPro.showLODBatchMenu = VegetationPackageEditorTools.DrawHeader("LOD settings", vegetationSystemPro.showLODBatchMenu))
                {
                    GUILayout.BeginVertical("box");
                    EditorGUI.BeginChangeCheck();   // begin checking for changes

                    // LOD crossfade
                    vegetationSystemPro.toggleLODCrossfade = EditorGUILayout.Toggle("Enable LOD crossfade", vegetationSystemPro.toggleLODCrossfade);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationPackagePro.VegetationInfoList[i].EnableCrossFade = vegetationSystemPro.toggleLODCrossfade;
                                        vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i].VegetationItemID)?.RefreshMaterials();
                                    }
                                }
                                else
                                {
                                    vegetationPackagePro.VegetationInfoList[i].EnableCrossFade = vegetationSystemPro.toggleLODCrossfade;
                                    vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i].VegetationItemID)?.RefreshMaterials();
                                }
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        {
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].EnableCrossFade = vegetationSystemPro.toggleLODCrossfade;
                                            vegetationSystemPro.GetVegetationItemModelInfo(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID)?.RefreshMaterials();
                                        }
                                    }
                                    else
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].EnableCrossFade = vegetationSystemPro.toggleLODCrossfade;
                                        vegetationSystemPro.GetVegetationItemModelInfo(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID)?.RefreshMaterials();
                                    }
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            SetSceneDirty();
                        }

                    if (EditorGUI.EndChangeCheck()) // when changes got made
                        SetSceneDirty();    // write made changes also to disk not just memory

                    GUILayout.EndVertical();
                }
                #endregion
                #region batchMaterialSettings
                if (vegetationSystemPro.showShaderSettingsBatchMenu = VegetationPackageEditorTools.DrawHeader("Material settings", vegetationSystemPro.showShaderSettingsBatchMenu))
                {
                    GUILayout.BeginVertical("box");
                    EditorGUI.BeginChangeCheck();   // begin checking for changes

                    // shader controller override state
                    vegetationSystemPro.toggleUseShaderControllerOverrides = EditorGUILayout.Toggle("Use shader controller overrides", vegetationSystemPro.toggleUseShaderControllerOverrides);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].useShaderControllerOverrides = vegetationSystemPro.toggleUseShaderControllerOverrides;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].useShaderControllerOverrides = vegetationSystemPro.toggleUseShaderControllerOverrides;
                            vegetationSystemPro.ClearMaterials();
                            vegetationSystemPro.RefreshItemSystem();
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].useShaderControllerOverrides = vegetationSystemPro.toggleUseShaderControllerOverrides;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].useShaderControllerOverrides = vegetationSystemPro.toggleUseShaderControllerOverrides;
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            vegetationSystemPro.ClearMaterials();
                            vegetationSystemPro.RefreshItemSystem();
                            SetSceneDirty();
                        }

                    // material settings reset buttons
                    EditorGUILayout.LabelField("");
                    if (GUILayout.Button("Reset ALL materials of the selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].ShaderControllerSettings = null;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].ShaderControllerSettings = null;
                            vegetationSystemPro.RefreshItemSystem();
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Reset ALL materials of ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ShaderControllerSettings = null;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ShaderControllerSettings = null;
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            vegetationSystemPro.RefreshItemSystem();
                            SetSceneDirty();
                        }

                    if (EditorGUI.EndChangeCheck()) // when changes got made
                        SetSceneDirty();    // write made changes also to disk not just memory

                    GUILayout.EndVertical();
                }
                #endregion
                #region batchBillboardSettings
                if (vegetationSystemPro.showBillboardBatchMenu = VegetationPackageEditorTools.DrawHeader("Billboards", vegetationSystemPro.showBillboardBatchMenu))
                {
                    if (vegetationSystemPro.toggleTypeFilterSelection == false ||
                        (vegetationSystemPro.overrideSelectionType != VegetationType.Tree)) //&& _vegetationSystemPro.overrideSelectionType != VegetationType.Grass && _vegetationSystemPro.overrideSelectionType != VegetationType.Plant))
                    {
                        EditorGUILayout.HelpBox("Selected vegetation type filter not supported\nSpecific \"Tree\" selection needed", MessageType.Warning);
                        goto exitBillboardSettings;
                    }

                    GUILayout.BeginVertical("box");
                    EditorGUI.BeginChangeCheck();   // begin checking for changes

                    // billboard state
                    vegetationSystemPro.toggleBillboardState = EditorGUILayout.Toggle("Enable billboards", vegetationSystemPro.toggleBillboardState);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].UseBillboards = vegetationSystemPro.toggleBillboardState;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].UseBillboards = vegetationSystemPro.toggleBillboardState;
                            vegetationSystemPro.ClearCache(vegetationPackagePro);
                            vegetationSystemPro.RefreshItemSystem();
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseBillboards = vegetationSystemPro.toggleBillboardState;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseBillboards = vegetationSystemPro.toggleBillboardState;
                                vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            vegetationSystemPro.RefreshItemSystem();
                            SetSceneDirty();
                        }

                    // billboard crossfade distance
                    EditorGUILayout.LabelField("", labelStyle);
                    vegetationSystemPro.overrideBillboardCrossfadeDistance = EditorGUILayout.Slider("Crossfade-out distance", vegetationSystemPro.overrideBillboardCrossfadeDistance, 0, 20);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationPackagePro.VegetationInfoList[i].BillboardFadeOutDistance = vegetationSystemPro.overrideBillboardCrossfadeDistance;
                                        vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i].VegetationItemID)?.UpdateBillboardMaterial();
                                    }
                                }
                                else
                                {
                                    vegetationPackagePro.VegetationInfoList[i].BillboardFadeOutDistance = vegetationSystemPro.overrideBillboardCrossfadeDistance;
                                    vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i].VegetationItemID)?.UpdateBillboardMaterial();
                                }
                            vegetationSystemPro.ClearCache(vegetationPackagePro);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        {
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BillboardFadeOutDistance = vegetationSystemPro.overrideBillboardCrossfadeDistance;
                                            vegetationSystemPro.GetVegetationItemModelInfo(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID)?.UpdateBillboardMaterial();
                                        }
                                    }
                                    else
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BillboardFadeOutDistance = vegetationSystemPro.overrideBillboardCrossfadeDistance;
                                        vegetationSystemPro.GetVegetationItemModelInfo(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID)?.UpdateBillboardMaterial();
                                    }
                                vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            SetSceneDirty();
                        }

                    // billboard re-generation
                    EditorGUILayout.LabelField("", labelStyle);
                    EditorGUILayout.LabelField("Regenerate billboards");

                    if (GUILayout.Button("Regenerate ALL billboards of the selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationPackagePro.GenerateBillboard(vegetationPackagePro.VegetationInfoList[i].VegetationItemID);
                                        vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i].VegetationItemID)?.CreateBillboardMaterial();
                                    }
                                }
                                else
                                {
                                    vegetationPackagePro.GenerateBillboard(vegetationPackagePro.VegetationInfoList[i].VegetationItemID);
                                    vegetationSystemPro.GetVegetationItemModelInfo(vegetationPackagePro.VegetationInfoList[i].VegetationItemID)?.CreateBillboardMaterial();
                                }
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Regenerate ALL billboards of ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        {
                                            vegetationSystemPro.vegetationPackageProList[i].GenerateBillboard(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID);
                                            vegetationSystemPro.GetVegetationItemModelInfo(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID)?.CreateBillboardMaterial();
                                        }
                                    }
                                    else
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].GenerateBillboard(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID);
                                        vegetationSystemPro.GetVegetationItemModelInfo(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID)?.CreateBillboardMaterial();
                                    }
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            SetSceneDirty();
                        }

                    if (EditorGUI.EndChangeCheck()) // when changes got made
                        SetSceneDirty();    // write made changes also to disk not just memory

                    GUILayout.EndVertical();
                exitBillboardSettings:;
                }
                #endregion
                #region batchColliderSettings
                if (vegetationSystemPro.showColliderBatchMenu = VegetationPackageEditorTools.DrawHeader("Colliders", vegetationSystemPro.showColliderBatchMenu))
                {
                    if (vegetationSystemPro.toggleTypeFilterSelection == false || vegetationSystemPro.overrideSelectionType == VegetationType.Grass || vegetationSystemPro.overrideSelectionType == VegetationType.Plant)
                    {
                        EditorGUILayout.HelpBox("Selected vegetation type filter not supported\nSpecific \"Object, Large Object, Tree\" selection needed", MessageType.Warning);
                        goto exitBatchColliderSettings;
                    }

                    GUILayout.BeginVertical("box");
                    EditorGUI.BeginChangeCheck();   // begin checking for changes

                    // collider type
                    vegetationSystemPro.overrideColliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider type", vegetationSystemPro.overrideColliderType);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].ColliderType = vegetationSystemPro.overrideColliderType;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].ColliderType = vegetationSystemPro.overrideColliderType;
                            vegetationSystemPro.RefreshColliderSystem();
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderType = vegetationSystemPro.overrideColliderType;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderType = vegetationSystemPro.overrideColliderType;
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            vegetationSystemPro.RefreshColliderSystem();
                            SetSceneDirty();
                        }

                    // local distance factor
                    EditorGUILayout.LabelField("", labelStyle);
                    vegetationSystemPro.overrideColliderDistance = EditorGUILayout.Slider("Local distance factor", vegetationSystemPro.overrideColliderDistance, 0, 1);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].ColliderDistanceFactor = vegetationSystemPro.overrideColliderDistance;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].ColliderDistanceFactor = vegetationSystemPro.overrideColliderDistance;
                            vegetationSystemPro.RefreshColliderSystem();
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderDistanceFactor = vegetationSystemPro.overrideColliderDistance;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderDistanceFactor = vegetationSystemPro.overrideColliderDistance;
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            vegetationSystemPro.RefreshColliderSystem();
                            SetSceneDirty();
                        }

                    if (EditorGUI.EndChangeCheck()) // when changes got made
                        SetSceneDirty();    // write made changes also to disk not just memory

                    GUILayout.EndVertical();
                exitBatchColliderSettings:;
                }
                #endregion
                #region batchDistanceFalloffSettings
                if (vegetationSystemPro.showDistanceFalloffBatchMenu = VegetationPackageEditorTools.DrawHeader("Distance falloff", vegetationSystemPro.showDistanceFalloffBatchMenu))
                {
                    if (vegetationSystemPro.toggleTypeFilterSelection == false || vegetationSystemPro.overrideSelectionType == VegetationType.Tree || vegetationSystemPro.overrideSelectionType == VegetationType.LargeObjects)
                    {
                        EditorGUILayout.HelpBox("Selected vegetation type filter not supported\nSpecific \"Grass, Plant, Object\" selection needed", MessageType.Warning);
                        goto exitBatchDistanceFalloff;
                    }

                    GUILayout.BeginVertical("box");
                    EditorGUI.BeginChangeCheck();   // begin checking for changes

                    // distance falloff state
                    vegetationSystemPro.toggleDistanceFalloff = EditorGUILayout.Toggle("Enable distance falloff", vegetationSystemPro.toggleDistanceFalloff);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].UseDistanceFalloff = vegetationSystemPro.toggleDistanceFalloff;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].UseDistanceFalloff = vegetationSystemPro.toggleDistanceFalloff;
                            vegetationSystemPro.ClearCache(vegetationPackagePro);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseDistanceFalloff = vegetationSystemPro.toggleDistanceFalloff;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseDistanceFalloff = vegetationSystemPro.toggleDistanceFalloff;
                                vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            SetSceneDirty();
                        }

                    // distance falloff factor
                    EditorGUILayout.LabelField("", labelStyle);
                    vegetationSystemPro.overrideDistanceFalloffFactor = EditorGUILayout.Slider("Distance falloff factor", vegetationSystemPro.overrideDistanceFalloffFactor, 0.01f, 1);

                    if (GUILayout.Button("Override selected biome"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                    "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                    "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationPackagePro.VegetationInfoList[i].DistanceFalloffStartDistance = vegetationSystemPro.overrideDistanceFalloffFactor;
                                }
                                else vegetationPackagePro.VegetationInfoList[i].DistanceFalloffStartDistance = vegetationSystemPro.overrideDistanceFalloffFactor;
                            vegetationSystemPro.ClearCache(vegetationPackagePro);
                            EditorUtility.SetDirty(vegetationPackagePro);
                            SetSceneDirty();
                        }

                    if (GUILayout.Button("Override ALL biomes"))
                        if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                        {
                            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                            {
                                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                    if (vegetationSystemPro.toggleTypeFilterSelection)
                                    {
                                        if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                            vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].DistanceFalloffStartDistance = vegetationSystemPro.overrideDistanceFalloffFactor;
                                    }
                                    else
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].DistanceFalloffStartDistance = vegetationSystemPro.overrideDistanceFalloffFactor;
                                vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                                EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                            }
                            SetSceneDirty();
                        }

                    if (EditorGUI.EndChangeCheck()) // when changes got made
                        SetSceneDirty();    // write made changes also to disk not just memory

                    GUILayout.EndVertical();
                exitBatchDistanceFalloff:;
                }
                #endregion
                return;
            }

            #region batchDensityPosRotScaleSettings
            if (vegetationSystemPro.showPositionRotationScaleBatchMenu = VegetationPackageEditorTools.DrawHeader("Density/Position/Rotation/Scale", vegetationSystemPro.showPositionRotationScaleBatchMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();   // begin checking for changes

                // randomize position state
                vegetationSystemPro.toggleRandomizePosition = EditorGUILayout.Toggle("Randomize position", vegetationSystemPro.toggleRandomizePosition);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].RandomizePosition = vegetationSystemPro.toggleRandomizePosition;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].RandomizePosition = vegetationSystemPro.toggleRandomizePosition;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RandomizePosition = vegetationSystemPro.toggleRandomizePosition;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RandomizePosition = vegetationSystemPro.toggleRandomizePosition;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // position offset
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.overridePositionOffset = EditorGUILayout.Vector3Field("Position offset", vegetationSystemPro.overridePositionOffset);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].Offset = vegetationSystemPro.overridePositionOffset;
                            }
                            else
                                vegetationPackagePro.VegetationInfoList[i].Offset = vegetationSystemPro.overridePositionOffset;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].Offset = vegetationSystemPro.overridePositionOffset;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].Offset = vegetationSystemPro.overridePositionOffset;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // position offset range
                EditorGUILayout.LabelField("", labelStyle);
                EditorFunctions.FloatRangeField("Position offset range", ref vegetationSystemPro.overrideMinUpOffsetRange, ref vegetationSystemPro.overrideMaxUpOffsetRange, -10, 10);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                {
                                    vegetationPackagePro.VegetationInfoList[i].MinUpOffset = vegetationSystemPro.overrideMinUpOffsetRange;
                                    vegetationPackagePro.VegetationInfoList[i].MaxUpOffset = vegetationSystemPro.overrideMaxUpOffsetRange;
                                }
                            }
                            else
                            {
                                vegetationPackagePro.VegetationInfoList[i].MinUpOffset = vegetationSystemPro.overrideMinUpOffsetRange;
                                vegetationPackagePro.VegetationInfoList[i].MaxUpOffset = vegetationSystemPro.overrideMaxUpOffsetRange;
                            }
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinUpOffset = vegetationSystemPro.overrideMinUpOffsetRange;
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxUpOffset = vegetationSystemPro.overrideMaxUpOffsetRange;
                                    }
                                }
                                else
                                {
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinUpOffset = vegetationSystemPro.overrideMinUpOffsetRange;
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxUpOffset = vegetationSystemPro.overrideMaxUpOffsetRange;
                                }
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // rotation mode
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.overrideRotationMode = (VegetationRotationType)EditorGUILayout.EnumPopup("Rotation mode", vegetationSystemPro.overrideRotationMode);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].RotationMode = vegetationSystemPro.overrideRotationMode;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].RotationMode = vegetationSystemPro.overrideRotationMode;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RotationMode = vegetationSystemPro.overrideRotationMode;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RotationMode = vegetationSystemPro.overrideRotationMode;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // min/max scale
                EditorGUILayout.LabelField("", labelStyle);
                EditorFunctions.FloatRangeField("Min/Max scale", ref vegetationSystemPro.overrideMinScale, ref vegetationSystemPro.overrideMaxScale, 0.1f, 10f);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                {
                                    vegetationPackagePro.VegetationInfoList[i].MinScale = vegetationSystemPro.overrideMinScale;
                                    vegetationPackagePro.VegetationInfoList[i].MaxScale = vegetationSystemPro.overrideMaxScale;
                                }
                            }
                            else
                            {
                                vegetationPackagePro.VegetationInfoList[i].MinScale = vegetationSystemPro.overrideMinScale;
                                vegetationPackagePro.VegetationInfoList[i].MaxScale = vegetationSystemPro.overrideMaxScale;
                            }
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {

                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinScale = vegetationSystemPro.overrideMinScale;
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxScale = vegetationSystemPro.overrideMaxScale;
                                    }
                                }
                                else
                                {
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinScale = vegetationSystemPro.overrideMinScale;
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxScale = vegetationSystemPro.overrideMaxScale;
                                }
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                if (EditorGUI.EndChangeCheck()) // when changes got made
                    SetSceneDirty();    // write made changes also to disk not just memory

                GUILayout.EndVertical();
            }
            #endregion
            #region batchHeightSteepnessSettings
            if (vegetationSystemPro.showHeightSteepnessBatchMenu = VegetationPackageEditorTools.DrawHeader("Height/Steepness rules", vegetationSystemPro.showHeightSteepnessBatchMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();   // begin checking for changes

                // height rule state
                vegetationSystemPro.toggleHeightRule = EditorGUILayout.Toggle("Use height rule", vegetationSystemPro.toggleHeightRule);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].UseHeightRule = vegetationSystemPro.toggleHeightRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].UseHeightRule = vegetationSystemPro.toggleHeightRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseHeightRule = vegetationSystemPro.toggleHeightRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseHeightRule = vegetationSystemPro.toggleHeightRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // min/max height
                EditorGUILayout.LabelField("", labelStyle);
                EditorFunctions.FloatRangeField("Min/Max height above sea level", ref vegetationSystemPro.overrideMinHeight, ref vegetationSystemPro.overrideMaxHeight, -500, 10000);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                {
                                    vegetationPackagePro.VegetationInfoList[i].MinHeight = vegetationSystemPro.overrideMinHeight;
                                    vegetationPackagePro.VegetationInfoList[i].MaxHeight = vegetationSystemPro.overrideMaxHeight;
                                }
                            }
                            else
                            {
                                vegetationPackagePro.VegetationInfoList[i].MinHeight = vegetationSystemPro.overrideMinHeight;
                                vegetationPackagePro.VegetationInfoList[i].MaxHeight = vegetationSystemPro.overrideMaxHeight;
                            }
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinHeight = vegetationSystemPro.overrideMinHeight;
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxHeight = vegetationSystemPro.overrideMaxHeight;
                                    }
                                }
                                else
                                {
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinHeight = vegetationSystemPro.overrideMinHeight;
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxHeight = vegetationSystemPro.overrideMaxHeight;
                                }
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // steepness rule state
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.toggleSteepnessRule = EditorGUILayout.Toggle("Use steepness rule", vegetationSystemPro.toggleSteepnessRule);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].UseSteepnessRule = vegetationSystemPro.toggleSteepnessRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].UseSteepnessRule = vegetationSystemPro.toggleSteepnessRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseSteepnessRule = vegetationSystemPro.toggleSteepnessRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseSteepnessRule = vegetationSystemPro.toggleSteepnessRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // min/max steepness
                EditorGUILayout.LabelField("", labelStyle);
                EditorFunctions.FloatRangeField("Min/Max steepness", ref vegetationSystemPro.overrideMinSteepness, ref vegetationSystemPro.overrideMaxSteepness, 0, 90);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                {
                                    vegetationPackagePro.VegetationInfoList[i].MinSteepness = vegetationSystemPro.overrideMinSteepness;
                                    vegetationPackagePro.VegetationInfoList[i].MaxSteepness = vegetationSystemPro.overrideMaxSteepness;
                                }
                            }
                            else
                            {
                                vegetationPackagePro.VegetationInfoList[i].MinSteepness = vegetationSystemPro.overrideMinSteepness;
                                vegetationPackagePro.VegetationInfoList[i].MaxSteepness = vegetationSystemPro.overrideMaxSteepness;
                            }
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinSteepness = vegetationSystemPro.overrideMinSteepness;
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxSteepness = vegetationSystemPro.overrideMaxSteepness;
                                    }
                                }
                                else
                                {
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MinSteepness = vegetationSystemPro.overrideMinSteepness;
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].MaxSteepness = vegetationSystemPro.overrideMaxSteepness;
                                }
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                if (EditorGUI.EndChangeCheck()) // when changes got made
                    SetSceneDirty();    // write made changes also to disk not just memory

                GUILayout.EndVertical();
            }
            #endregion
            #region batchBiomeMaskRules
            if (vegetationSystemPro.showBiomeMaskRulesBatchMenu = VegetationPackageEditorTools.DrawHeader("Biome mask rules", vegetationSystemPro.showBiomeMaskRulesBatchMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();   // begin checking for changes

                // biome edge include state
                vegetationSystemPro.toggleBiomeEdgeIncludeRule = EditorGUILayout.Toggle("Use biome edge include rule", vegetationSystemPro.toggleBiomeEdgeIncludeRule);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].UseBiomeEdgeIncludeRule = vegetationSystemPro.toggleBiomeEdgeIncludeRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].UseBiomeEdgeIncludeRule = vegetationSystemPro.toggleBiomeEdgeIncludeRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseBiomeEdgeIncludeRule = vegetationSystemPro.toggleBiomeEdgeIncludeRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseBiomeEdgeIncludeRule = vegetationSystemPro.toggleBiomeEdgeIncludeRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // include - distance from edge
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.overrideBiomeEdgeIncludeDistance = EditorGUILayout.Slider("Distance from edge", vegetationSystemPro.overrideBiomeEdgeIncludeDistance, 0, 500);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].BiomeEdgeIncludeDistance = vegetationSystemPro.overrideBiomeEdgeIncludeDistance;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].BiomeEdgeIncludeDistance = vegetationSystemPro.overrideBiomeEdgeIncludeDistance;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeIncludeDistance = vegetationSystemPro.overrideBiomeEdgeIncludeDistance;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeIncludeDistance = vegetationSystemPro.overrideBiomeEdgeIncludeDistance;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // biome edge include invert state
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.toggleBiomeEdgeIncludeInvert = EditorGUILayout.Toggle("Invert", vegetationSystemPro.toggleBiomeEdgeIncludeInvert);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].BiomeEdgeIncludeInverse = vegetationSystemPro.toggleBiomeEdgeIncludeInvert;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].BiomeEdgeIncludeInverse = vegetationSystemPro.toggleBiomeEdgeIncludeInvert;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeIncludeInverse = vegetationSystemPro.toggleBiomeEdgeIncludeInvert;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeIncludeInverse = vegetationSystemPro.toggleBiomeEdgeIncludeInvert;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // biome edge scale state
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.toggleBiomeEdgeScaleRule = EditorGUILayout.Toggle("Use biome edge scale rule", vegetationSystemPro.toggleBiomeEdgeScaleRule);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].UseBiomeEdgeScaleRule = vegetationSystemPro.toggleBiomeEdgeScaleRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].UseBiomeEdgeScaleRule = vegetationSystemPro.toggleBiomeEdgeScaleRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseBiomeEdgeScaleRule = vegetationSystemPro.toggleBiomeEdgeScaleRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseBiomeEdgeScaleRule = vegetationSystemPro.toggleBiomeEdgeScaleRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // scale - distance from edge
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.overrideBiomeEdgeScaleDistance = EditorGUILayout.Slider("Distance from edge", vegetationSystemPro.overrideBiomeEdgeScaleDistance, 0, 500);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleDistance = vegetationSystemPro.overrideBiomeEdgeScaleDistance;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleDistance = vegetationSystemPro.overrideBiomeEdgeScaleDistance;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleDistance = vegetationSystemPro.overrideBiomeEdgeScaleDistance;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleDistance = vegetationSystemPro.overrideBiomeEdgeScaleDistance;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // scale - min/max scale
                EditorGUILayout.LabelField("", labelStyle);
                EditorFunctions.FloatRangeField("Min/Max scale", ref vegetationSystemPro.overrideBiomeEdgeScaleMinScale, ref vegetationSystemPro.overrideBiomeEdgeScaleMaxScale, 0.1f, 5);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                {
                                    vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleMinScale = vegetationSystemPro.overrideBiomeEdgeScaleMinScale;
                                    vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleMaxScale = vegetationSystemPro.overrideBiomeEdgeScaleMaxScale;
                                }
                            }
                            else
                            {
                                vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleMinScale = vegetationSystemPro.overrideBiomeEdgeScaleMinScale;
                                vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleMaxScale = vegetationSystemPro.overrideBiomeEdgeScaleMaxScale;
                            }
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    {
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleMinScale = vegetationSystemPro.overrideBiomeEdgeScaleMinScale;
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleMaxScale = vegetationSystemPro.overrideBiomeEdgeScaleMaxScale;
                                    }
                                }
                                else
                                {
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleMinScale = vegetationSystemPro.overrideBiomeEdgeScaleMinScale;
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleMaxScale = vegetationSystemPro.overrideBiomeEdgeScaleMaxScale;
                                }
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // biome edge scale invert state
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.toggleBiomeEdgeScaleInvert = EditorGUILayout.Toggle("Invert", vegetationSystemPro.toggleBiomeEdgeScaleInvert);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleInverse = vegetationSystemPro.toggleBiomeEdgeScaleInvert;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].BiomeEdgeScaleInverse = vegetationSystemPro.toggleBiomeEdgeScaleInvert;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleInverse = vegetationSystemPro.toggleBiomeEdgeScaleInvert;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].BiomeEdgeScaleInverse = vegetationSystemPro.toggleBiomeEdgeScaleInvert;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                if (EditorGUI.EndChangeCheck()) // when changes got made
                    SetSceneDirty();    // write made changes also to disk not just memory

                GUILayout.EndVertical();
            }
            #endregion
            #region batchTerrainSourceRules
            if (vegetationSystemPro.showTerrainSourceRulesBatchMenu = VegetationPackageEditorTools.DrawHeader("Terrain source rules", vegetationSystemPro.showTerrainSourceRulesBatchMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();   // begin checking for changes

                // Terrain source include rule
                vegetationSystemPro.toggleTerrainSourceIncludeRule = EditorGUILayout.Toggle("Use terrain source include rule", vegetationSystemPro.toggleTerrainSourceIncludeRule);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].UseTerrainSourceIncludeRule = vegetationSystemPro.toggleTerrainSourceIncludeRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].UseTerrainSourceIncludeRule = vegetationSystemPro.toggleTerrainSourceIncludeRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseTerrainSourceIncludeRule = vegetationSystemPro.toggleTerrainSourceIncludeRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseTerrainSourceIncludeRule = vegetationSystemPro.toggleTerrainSourceIncludeRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // Terrain source include rule toggles
                EditorGUILayout.LabelField("Select include IDs", labelStyle);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID1 = EditorGUILayout.Toggle("Terrain source ID 1", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID1);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID2 = EditorGUILayout.Toggle("Terrain source ID 2", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID2);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID3 = EditorGUILayout.Toggle("Terrain source ID 3", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID3);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID4 = EditorGUILayout.Toggle("Terrain source ID 4", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID4);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID5 = EditorGUILayout.Toggle("Terrain source ID 5", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID5);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID6 = EditorGUILayout.Toggle("Terrain source ID 6", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID6);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID7 = EditorGUILayout.Toggle("Terrain source ID 7", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID7);
                vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID8 = EditorGUILayout.Toggle("Terrain source ID 8", vegetationSystemPro.overrideTerrainSourceIncludeRule.UseTerrainSourceID8);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].TerrainSourceIncludeRule = vegetationSystemPro.overrideTerrainSourceIncludeRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].TerrainSourceIncludeRule = vegetationSystemPro.overrideTerrainSourceIncludeRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].TerrainSourceIncludeRule = vegetationSystemPro.overrideTerrainSourceIncludeRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].TerrainSourceIncludeRule = vegetationSystemPro.overrideTerrainSourceIncludeRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // Terrain source exclude rule
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.toggleTerrainSourceExcludeRule = EditorGUILayout.Toggle("Use terrain source exclude rule", vegetationSystemPro.toggleTerrainSourceExcludeRule);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].UseTerrainSourceExcludeRule = vegetationSystemPro.toggleTerrainSourceExcludeRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].UseTerrainSourceExcludeRule = vegetationSystemPro.toggleTerrainSourceExcludeRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseTerrainSourceExcludeRule = vegetationSystemPro.toggleTerrainSourceExcludeRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].UseTerrainSourceExcludeRule = vegetationSystemPro.toggleTerrainSourceExcludeRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                // Terrain source exclude rule toggles
                EditorGUILayout.LabelField("Select exclude IDs", labelStyle);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID1 = EditorGUILayout.Toggle("Terrain source ID 1", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID1);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID2 = EditorGUILayout.Toggle("Terrain source ID 2", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID2);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID3 = EditorGUILayout.Toggle("Terrain source ID 3", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID3);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID4 = EditorGUILayout.Toggle("Terrain source ID 4", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID4);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID5 = EditorGUILayout.Toggle("Terrain source ID 5", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID5);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID6 = EditorGUILayout.Toggle("Terrain source ID 6", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID6);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID7 = EditorGUILayout.Toggle("Terrain source ID 7", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID7);
                vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID8 = EditorGUILayout.Toggle("Terrain source ID 8", vegetationSystemPro.overrideTerrainSourceExcludeRule.UseTerrainSourceID8);

                if (GUILayout.Button("Override selected biome"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                                "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()),
                                "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                            if (vegetationSystemPro.toggleTypeFilterSelection)
                            {
                                if (vegetationPackagePro.VegetationInfoList[i].VegetationType == vegetationSystemPro.overrideSelectionType)
                                    vegetationPackagePro.VegetationInfoList[i].TerrainSourceExcludeRule = vegetationSystemPro.overrideTerrainSourceExcludeRule;
                            }
                            else vegetationPackagePro.VegetationInfoList[i].TerrainSourceExcludeRule = vegetationSystemPro.overrideTerrainSourceExcludeRule;
                        vegetationSystemPro.ClearCache(vegetationPackagePro);
                        EditorUtility.SetDirty(vegetationPackagePro);
                        SetSceneDirty();
                    }

                if (GUILayout.Button("Override ALL biomes"))
                    if (EditorUtility.DisplayDialog("Confirm selection",
                            "Selected biome: All biomes\nSelected type filter: " + (vegetationSystemPro.toggleTypeFilterSelection == false ? "All items" : vegetationSystemPro.overrideSelectionType.ToString()), "Confirm", "Cancel"))
                    {
                        for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        {
                            for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                                if (vegetationSystemPro.toggleTypeFilterSelection)
                                {
                                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationType == vegetationSystemPro.overrideSelectionType)
                                        vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].TerrainSourceExcludeRule = vegetationSystemPro.overrideTerrainSourceExcludeRule;
                                }
                                else
                                    vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].TerrainSourceExcludeRule = vegetationSystemPro.overrideTerrainSourceExcludeRule;
                            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationPackageProList[i]);
                            EditorUtility.SetDirty(vegetationSystemPro.vegetationPackageProList[i]);
                        }
                        SetSceneDirty();
                    }

                if (EditorGUI.EndChangeCheck()) // when changes got made
                    SetSceneDirty();    // write made changes also to disk not just memory

                GUILayout.EndVertical();
            }
            #endregion
        }

        void DrawDebugInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Use the debug section to identify/solve issues\nBut also to help with understanding the system and how to increase performance", MessageType.Info);
            GUILayout.EndVertical();

            if (vegetationSystemPro.showDebugSettingsMenu = VegetationPackageEditorTools.DrawHeader("Debug settings", vegetationSystemPro.showDebugSettingsMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginChangeCheck();
                vegetationSystemPro.showSeaLevel = EditorGUILayout.Toggle("Show sea level", vegetationSystemPro.showSeaLevel);
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.showSystemTotalArea = EditorGUILayout.Toggle("Show total area", vegetationSystemPro.showSystemTotalArea);
                vegetationSystemPro.showAllMeshTerrainAreas = EditorGUILayout.Toggle("Show all mesh terrain areas", vegetationSystemPro.showAllMeshTerrainAreas);
                vegetationSystemPro.showAllRaycastTerrainAreas = EditorGUILayout.Toggle("Show all raycast terrain areas", vegetationSystemPro.showAllRaycastTerrainAreas);
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.showBiomeMasks = EditorGUILayout.Toggle("Show all biome mask areas", vegetationSystemPro.showBiomeMasks);
                vegetationSystemPro.showTextureMaskAreas = EditorGUILayout.Toggle("Show selected texture mask area", vegetationSystemPro.showTextureMaskAreas);
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.showBiomeMaskCells = EditorGUILayout.Toggle("Show biome mask cells", vegetationSystemPro.showBiomeMaskCells);
                vegetationSystemPro.showVegetationMaskCells = EditorGUILayout.Toggle("Show vegetation mask cells", vegetationSystemPro.showVegetationMaskCells);
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.showVegetationCells = EditorGUILayout.Toggle("Show vegetation cells", vegetationSystemPro.showVegetationCells);
                vegetationSystemPro.showPredictiveVegetationCells = EditorGUILayout.Toggle("Show predictive vegetation cells", vegetationSystemPro.showPredictiveVegetationCells);
                vegetationSystemPro.showVisibleVegetationCells = (VegetationSystemPro.ECellCullingDebugMode)EditorGUILayout.EnumPopup("Show visible vegetation cells", vegetationSystemPro.showVisibleVegetationCells);
                EditorGUILayout.HelpBox("Unloaded = RED\n" + "Loaded(full) = CYAN / Loaded (only Large objects, Trees) = WHITE\n" +
                    "Visible(full) = GREEN / Visible (only Large objects, Trees) = BLACK\n" + "Visible(Shadows) = YELLOW", MessageType.Info);
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.showBillboardCells = EditorGUILayout.Toggle("Show billboard cells", vegetationSystemPro.showBillboardCells);
                vegetationSystemPro.showPredictiveBillboardCells = EditorGUILayout.Toggle("Show predictive billboard cells", vegetationSystemPro.showPredictiveBillboardCells);
                vegetationSystemPro.showVisibleBillboardCells = EditorGUILayout.Toggle("Show visible billboard cells", vegetationSystemPro.showVisibleBillboardCells);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("", labelStyle);
                vegetationSystemPro.vegetationRenderSettings.showLODDebug = EditorGUILayout.Toggle("Show LODs by color tint", vegetationSystemPro.vegetationRenderSettings.showLODDebug);
                EditorGUILayout.HelpBox("Custom shaders need to integrate \"_LODDebugColor\" to their base color\nVegetation items need to use the \"Isolated\" material mode", MessageType.Info);
                if (EditorGUI.EndChangeCheck())
                {
                    vegetationSystemPro.RefreshMaterials();
                    SetSceneDirty();
                }

                vegetationSystemPro.onSceneCamChangeDirty = EditorGUILayout.Toggle("Disable scene camera dirty", vegetationSystemPro.onSceneCamChangeDirty);
                EditorGUILayout.HelpBox("Enable this only when needed\nBy default any change to the scene camera marks the scene as unsaved aka dirty\nDirty ensures the engine actually applies changes immediately" +
                    "\nThis helps the scene view camera to display the vegetation instances accurately\nIt can also help with bugs since on saving the engine does some background refreshes", MessageType.Info);

                if (EditorGUI.EndChangeCheck())
                    SetSceneDirty();

                GUILayout.EndVertical();
            }

            if (vegetationSystemPro.showDebugToolsMenu = VegetationPackageEditorTools.DrawHeader("Debug tools", vegetationSystemPro.showDebugToolsMenu))
            {
                GUILayout.BeginVertical("box");
                vegetationSystemPro.isSetupDone = EditorGUILayout.Toggle("Vegetation system state", vegetationSystemPro.isSetupDone);
                EditorGUILayout.HelpBox("This toggle can be used for soft-disabling the system without clearing any memory\n" +
                    "This only pauses the system so certain data is still loaded ex: Colliders/Run-time prefabs", MessageType.Info);

                if (GUILayout.Button("Refresh vegetation instances"))
                {
                    vegetationSystemPro.RefreshTerrainArea();
                    vegetationSystemPro.RefreshTerrainHeightmap();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Refresh item system"))
                {
                    vegetationSystemPro.RefreshItemSystem();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Refresh cell system"))
                {
                    vegetationSystemPro.RefreshCellSystem();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Refresh pooling system"))
                {
                    vegetationSystemPro.RefreshColliderSystem();
                    vegetationSystemPro.RefreshRuntimePrefabSpawner();
                    SetSceneDirty();
                }

                if (GUILayout.Button("Restart entire vegetation system"))
                {
                    vegetationSystemPro.RestartVegetationSystem();
                    SetSceneDirty();
                }
                GUILayout.EndVertical();
            }
        }
    }
}