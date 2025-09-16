using System;
using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.Vegetation.Masks;
using Unity.Collections;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public enum VegetationType
    {
        Grass = 0,
        Plant = 1,
        Tree = 2,
        Objects = 3,
        LargeObjects = 4
    }

    [Serializable]
    public enum VegetationPrefabType
    {
        Mesh = 0,
        Texture = 1
    }

    [Serializable]
    public enum VegetationRenderMode
    {
        Instanced = 0,
        Normal = 1,
        InstancedIndirect = 2,
#if USING_HDRP && UNITY_2023_1_OR_NEWER
        RayTraced = 3
#endif
    }

    [Serializable]
    public enum VegetationRotationType
    {
        RotateY = 0,
        RotateXYZ = 1,
        FollowTerrain = 2,
        FollowTerrainScale = 3,
        NoRotation = 4
    }

    [Serializable]
    public enum ColliderType
    {
        Disabled = 0,
        Capsule = 1,
        Sphere = 2,
        Box = 3,
        Mesh = 4,
        CustomMesh = 5,
        FromPrefab = 6
    }

    [Serializable]
    public enum NavMeshObstacleType
    {
        Disabled = 0,
        Capsule = 1,
        Box = 2
    }

    [Serializable]
    public enum LODLevel
    {
        LOD3 = 3,
        LOD2 = 2,
        LOD1 = 1,
        LOD0 = 0
    }

    [Serializable]
    public enum EBillboardAtlasColorSource
    {
        TextureColorBake,
        ShaderController
    }

    [Serializable]
    public enum BillboardQuality
    {
        Mono_128x = 9,  // 1x1
        Mono_256x = 10,
        Mono_512x = 11,
        Quad_128x = 12, // 1x4
        Quad_256x = 13,
        Quad_512x = 14,
        Octa_128x = 0,  // 1x8
        Octa_256x = 1,
        Octa_512x = 2,
        HexaDeca_256x = 8,  // 1x16 
        Octa3D_128x = 4,    // 8x8
        Octa3D_256x = 5,
        Octa3D_512x = 6,
        HexaDeca3D_256x = 7,    // 16x16
    }

    [Serializable]
    public enum VegetationTypeIndex
    {
        VegetationType1 = 1,
        VegetationType2 = 2,
        VegetationType3 = 3,
        VegetationType4 = 4,
        VegetationType5 = 5,
        VegetationType6 = 6,
        VegetationType7 = 7,
        VegetationType8 = 8,
        VegetationType9 = 9,
        VegetationType10 = 10,
        VegetationType11 = 11,
        VegetationType12 = 12,
        VegetationType13 = 13,
        VegetationType14 = 14,
        VegetationType15 = 15,
        VegetationType16 = 16,
        VegetationType17 = 17,
        VegetationType18 = 18,
        VegetationType19 = 19,
        VegetationType20 = 20,
        VegetationType21 = 21,
        VegetationType22 = 22,
        VegetationType23 = 23,
        VegetationType24 = 24,
        VegetationType25 = 25,
        VegetationType26 = 26,
        VegetationType27 = 27,
        VegetationType28 = 28,
        VegetationType29 = 29,
        VegetationType30 = 30,
        VegetationType31 = 31,
        VegetationType32 = 32
    }

    [Serializable]
    public enum TerrainSourceID
    {
        TerrainSourceID1 = 0,
        TerrainSourceID2 = 1,
        TerrainSourceID3 = 2,
        TerrainSourceID4 = 3,
        TerrainSourceID5 = 4,
        TerrainSourceID6 = 5,
        TerrainSourceID7 = 6,
        TerrainSourceID8 = 7
    }

    [Serializable]
    public enum BiomeType
    {
        Default = 0,
        BorealForest = 1,
        TemperateDeciduousForest = 2,
        TropicalRainForest = 3,
        TemperateRainForest = 4,
        ScrubForest = 5,
        DeadForest = 6,
        FantasyForest = 7,
        Grassland = 8,
        Desert = 9,
        Swamp = 10,
        Tundra = 11,
        Oasis = 12,
        Underwater = 13,
        FrozenForest = 14,
        Volcano = 15,
        River = 16,
        Seaside = 17,
        Meadow = 18,
        Lake = 19,
        Road = 20,
        Biome1 = 21,
        Biome2,
        Biome3,
        Biome4,
        Biome5,
        Biome6,
        Biome7,
        Biome8,
        Biome9,
        Biome10,
        Biome11,
        Biome12,
        Biome13,
        Biome14,
        Biome15,
        Biome16,
        Biome17,
        Biome18,
        Biome19,
        Biome20,
        Biome21,
        Biome22,
        Biome23,
        Biome24,
        Biome25,
        Biome26,
        Biome27,
        Biome28,
        Biome29,
        Biome30,
        Biome31,
        Biome32,
        Biome33,
        Biome34,
        Biome35,
        Biome36,
        Biome37,
        Biome38,
        Biome39,
        Biome40,
        Biome41,
        Biome42,
        Biome43,
        Biome44
    }

    [Serializable]
    public enum TextureMaskRuleType
    {
        Include = 1,
        Exclude = 2,
        Density = 3,
        Scale = 4
    }

    [Serializable]
    public enum TerrainTextureType
    {
        Texture1 = 0,
        Texture2 = 1,
        Texture3 = 2,
        Texture4 = 3,
        Texture5 = 4,
        Texture6 = 5,
        Texture7 = 6,
        Texture8 = 7,
        Texture9 = 8,
        Texture10 = 9,
        Texture11 = 10,
        Texture12 = 11,
        Texture13 = 12,
        Texture14 = 13,
        Texture15 = 14,
        Texture16 = 15,
        Texture17 = 16,
        Texture18 = 17,
        Texture19 = 18,
        Texture20 = 19,
        Texture21 = 20,
        Texture22 = 21,
        Texture23 = 22,
        Texture24 = 23,
        Texture25 = 24,
        Texture26 = 25,
        Texture27 = 26,
        Texture28 = 27,
        Texture29 = 28,
        Texture30 = 29,
        Texture31 = 30,
        Texture32 = 31
    }

    [Serializable]
    public struct VegetationItemIndices
    {
        public int vegetationPackageIndex;
        public int vegetationItemIndex;
    }

    public class VegetationInfoComparer : IComparer<int>
    {
        public List<VegetationItemInfoPro> vegetationInfoList;

        public int Compare(int _a, int _b)
        {
            int aTypeValue = (int)vegetationInfoList[_a].VegetationType;
            int bTypeValue = (int)vegetationInfoList[_b].VegetationType;
            return bTypeValue.CompareTo(aTypeValue);
        }
    }

    public class VegetationInfoIDComparer : IComparer<string>
    {
        public List<VegetationItemInfoPro> vegetationInfoList;

        public int Compare(string _a, string _b)
        {
            int indexA = GetIndexFromID(_a);
            int indexB = GetIndexFromID(_b);
            if (indexA < 0 || indexB < 0)
                return -1;

            int aTypeValue = (int)vegetationInfoList[indexA].VegetationType;
            int bTypeValue = (int)vegetationInfoList[indexB].VegetationType;
            return bTypeValue.CompareTo(aTypeValue);
        }

        private int GetIndexFromID(string _id)
        {
            for (int i = 0; i < vegetationInfoList.Count; i++)
                if (vegetationInfoList[i].VegetationItemID == _id)
                    return i;
            return -1;
        }
    }

    public class BiomeSortOrderComparer : IComparer<VegetationPackagePro>
    {
        public int Compare(VegetationPackagePro _x, VegetationPackagePro _y)
        {
            if (_x != null && _y != null)
                return _x.BiomeSortOrder.CompareTo(_y.BiomeSortOrder);
            else
                return 0;
        }
    }

    [Serializable]
    public class TextureMaskRule
    {
        public String TextureMaskGroupID;
        public float MinDensity = 0;
        public float MaxDensity = 1;
        public float ScaleMultiplier = 1;
        public float DensityMultiplier = 1;
        public float BrightnessThreshold = 1;
        public float MinBrightness = 0;
        public float MaxBrightness = 1;
        public List<SerializedControllerProperty> TextureMaskPropertiesList = new();

        public TextureMaskRule(TextureMaskRuleType _type)
        {
            switch (_type)
            {
                case TextureMaskRuleType.Include:
                    break;
                case TextureMaskRuleType.Exclude:
                    break;
                case TextureMaskRuleType.Density:
                    break;
                case TextureMaskRuleType.Scale:
                    MinDensity = 1;
                    MaxDensity = 5;
                    break;
            }
        }

        public TextureMaskRule(TextureMaskRule _sourceItem)
        {
            TextureMaskGroupID = _sourceItem.TextureMaskGroupID;
            MinDensity = _sourceItem.MinDensity;
            MaxDensity = _sourceItem.MaxDensity;
            ScaleMultiplier = _sourceItem.ScaleMultiplier;
            DensityMultiplier = _sourceItem.DensityMultiplier;
            BrightnessThreshold = _sourceItem.BrightnessThreshold;

            for (int i = 0; i < _sourceItem.TextureMaskPropertiesList.Count; i++)
                TextureMaskPropertiesList.Add(new SerializedControllerProperty(_sourceItem.TextureMaskPropertiesList[i]));
        }

        public TextureMaskRule(TextureMaskSettings _textureMaskSettings)
        {
            for (int i = 0; i < _textureMaskSettings.controllerPropertyList.Count; i++)
                TextureMaskPropertiesList.Add(new SerializedControllerProperty(_textureMaskSettings.controllerPropertyList[i]));
        }

        public TextureMaskRule(TextureMaskSettings _textureMaskSettings, TextureMaskRuleType _type)
        {
            for (int i = 0; i < _textureMaskSettings.controllerPropertyList.Count; i++)
                TextureMaskPropertiesList.Add(new SerializedControllerProperty(_textureMaskSettings.controllerPropertyList[i]));

            switch (_type)
            {
                case TextureMaskRuleType.Include:
                    break;
                case TextureMaskRuleType.Exclude:
                    break;
                case TextureMaskRuleType.Density:
                    break;
                case TextureMaskRuleType.Scale:
                    MinDensity = 1;
                    MaxDensity = 5;
                    break;
            }
        }

        public bool GetBooleanPropertyValue(string _propertyName)
        {
            for (int i = 0; i < TextureMaskPropertiesList.Count; i++)
                if (TextureMaskPropertiesList[i].PropertyName == _propertyName)
                    return TextureMaskPropertiesList[i].BoolValue;
            return false;
        }

        public int GetIntPropertyValue(string _propertyName)
        {
            for (int i = 0; i < TextureMaskPropertiesList.Count; i++)
                if (TextureMaskPropertiesList[i].PropertyName == _propertyName)
                    return TextureMaskPropertiesList[i].IntValue;
            return 0;
        }
    }

    [Serializable]
    public class TerrainTextureRule
    {
        public int TextureIndex;
        public float MinimumValue = 0;
        public float MaximumValue = 1;
        public float DensityMultiplier = 1;
        public float ScaleMultiplier = 1;
        public float BrightnessThreshold = 1;
        public float MinBrightness = 0;
        public float MaxBrightness = 1;
        public bool Inverse;

        public TerrainTextureRule()
        {

        }

        public TerrainTextureRule(TextureMaskRuleType _type)
        {
            switch (_type)
            {
                case TextureMaskRuleType.Include:
                    break;
                case TextureMaskRuleType.Exclude:
                    break;
                case TextureMaskRuleType.Density:
                    break;
                case TextureMaskRuleType.Scale:
                    MinimumValue = 1;
                    MaximumValue = 5;
                    break;
            }
        }

        public TerrainTextureRule(TerrainTextureRule _sourceItem)
        {
            TextureIndex = _sourceItem.TextureIndex;
            MinimumValue = _sourceItem.MinimumValue;
            MaximumValue = _sourceItem.MaximumValue;
            ScaleMultiplier = _sourceItem.ScaleMultiplier;
            DensityMultiplier = _sourceItem.DensityMultiplier;
            BrightnessThreshold = _sourceItem.BrightnessThreshold;
            Inverse = _sourceItem.Inverse;
        }
    }

    [Serializable]
    public struct TerrainSourceRule
    {
        public bool UseTerrainSourceID1;
        public bool UseTerrainSourceID2;
        public bool UseTerrainSourceID3;
        public bool UseTerrainSourceID4;
        public bool UseTerrainSourceID5;
        public bool UseTerrainSourceID6;
        public bool UseTerrainSourceID7;
        public bool UseTerrainSourceID8;

        public bool this[int _index]
        {
            get { return UseTerrainSource(_index); }
            set { SetUseTerrainSource(_index, value); }
        }

        public void SetUseTerrainSource(int _index, bool _value)
        {
            switch (_index)
            {
                case 0:
                    UseTerrainSourceID1 = _value;
                    break;
                case 1:
                    UseTerrainSourceID2 = _value;
                    break;
                case 2:
                    UseTerrainSourceID3 = _value;
                    break;
                case 3:
                    UseTerrainSourceID4 = _value;
                    break;
                case 4:
                    UseTerrainSourceID5 = _value;
                    break;
                case 5:
                    UseTerrainSourceID6 = _value;
                    break;
                case 6:
                    UseTerrainSourceID7 = _value;
                    break;
                case 7:
                    UseTerrainSourceID8 = _value;
                    break;
            }
        }

        public bool UseTerrainSource(int _index)
        {
            return math.select(math.select(math.select(math.select(math.select(math.select(math.select(math.select(
                0,
                UseTerrainSourceID8 ? 1 : 0, _index == 7),
                UseTerrainSourceID7 ? 1 : 0, _index == 6),
                UseTerrainSourceID6 ? 1 : 0, _index == 5),
                UseTerrainSourceID5 ? 1 : 0, _index == 4),
                UseTerrainSourceID4 ? 1 : 0, _index == 3),
                UseTerrainSourceID3 ? 1 : 0, _index == 2),
                UseTerrainSourceID2 ? 1 : 0, _index == 1),
                UseTerrainSourceID1 ? 1 : 0, _index == 0)
            == 1;
        }
    }

    [Serializable]
    public class TerrainTextureSettings
    {
        public bool Enabled = true;
        public bool LockTexture;

        public float densityFactor = 1;
        public AnimationCurve TextureHeightCurve = new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });
        public AnimationCurve TextureSteepnessCurve = new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });
        public NativeArray<float> HeightCurveArray;
        public NativeArray<float> SteepnessCurveArray;

        public bool UseNoise = false;
        public float NoiseScale = 15;
        public float noiseBalancing = 0;
        public float2 NoiseOffset = float2.zero;
        public bool InverseNoise;

        public float DistancePerSample = 1;
        public bool applyCurves;
        public bool applyNoise;

        public bool ConcaveEnable;
        public float concaveDensityFactor = 1;
        public float ConcaveMinHeightDifference = 0.1f;
        public float ConcaveMinHeight = 0;
        public float ConcaveMaxHeight = 1000;
        public float concaveMinSteepness = 0;
        public float concaveMaxSteepness = 90;

        public bool ConvexEnable;
        public float convexDensityFactor = 1;
        public float ConvexMinHeightDifference = 0.1f;
        public float ConvexMinHeight = 0;
        public float ConvexMaxHeight = 1000;
        public float convexMinSteepness = 0;
        public float convexMaxSteepness = 90;
    }

    [Serializable]
    public class TerrainTextureInfo
    {
        public Texture2D Texture;
        public Texture2D TextureNormals;
        public Texture2D TextureOcclusion;
        public Texture2D TextureHeightMap;
        public float2 TileSize = new(8, 8);
        public float2 Offset = new(0, 0);
        public TerrainLayer TerrainLayer;
    }

    [Serializable]
    public class RuntimePrefabRule
    {
        public GameObject RuntimePrefab;
        public float DistanceFactor = 0.2f;
        public float SpawnFrequency = 1f;
        public int Seed;
        public bool UsePool = true;
        public float3 PrefabOffset = new(0, 0, 0);
        public float3 PrefabRotation = new(0, 0, 0);
        public float3 PrefabScale = new(1, 1, 1);
        public LayerMask PrefabLayer = 0;
        public bool UseVegetationItemScale;

        public void SetSeed()
        {
            Seed = UnityEngine.Random.Range(0, 101);
        }

        public RuntimePrefabRule()
        {

        }

        public RuntimePrefabRule(RuntimePrefabRule _sourceItem)
        {
            RuntimePrefab = _sourceItem.RuntimePrefab;
            SpawnFrequency = _sourceItem.SpawnFrequency;
            Seed = _sourceItem.Seed;
            PrefabOffset = _sourceItem.PrefabOffset;
            PrefabRotation = _sourceItem.PrefabRotation;
            PrefabScale = _sourceItem.PrefabScale;
            PrefabLayer = _sourceItem.PrefabLayer;
            UseVegetationItemScale = _sourceItem.UseVegetationItemScale;
            UsePool = _sourceItem.UsePool;
        }
    }

    [Serializable]
    public class VegetationItemInfoPro
    {
        public string VegetationItemID;
        public string VegetationGuid = "";  // same as the used prefab/texture

        public VegetationType VegetationType = VegetationType.Tree;
        public VegetationPrefabType PrefabType = VegetationPrefabType.Mesh;
        public GameObject VegetationPrefab;
        public Texture2D VegetationTexture;
        public string Name;
        public bool EnableRuntimeSpawn = true;
        public bool DisableShadows;
        public int Seed;
        public VegetationRenderMode VegetationRenderMode = VegetationRenderMode.Instanced;
        public float RenderDistanceFactor = 1;

        public float Density = 1;
        public float SampleDistance = 1;
        public bool UseSamplePointOffset;
        public float SamplePointMinOffset = 2;
        public float SamplePointMaxOffset = 6;

        public bool RandomizePosition = true;
        public float3 Offset = new(0, 0, 0);    // absolute
        public float MinUpOffset = 0;   // range
        public float MaxUpOffset = 0;   // range
        public float3 RotationOffset = new(0, 0, 0);
        public VegetationRotationType RotationMode = VegetationRotationType.RotateY;

        public Bounds Bounds;
        public float3 ScaleMultiplier = new(1, 1, 1);
        public float MinScale = 0.8f;
        public float MaxScale = 1.2f;
        public bool useAdvancedScaleRule;
        public AnimationCurve scaleRuleCurve = new(new Keyframe[] { new(0, 0, 0, 0), new(1, 1, 0, 0) });

        public bool UseHeightRule = true;
        public float MinHeight;
        public float MaxHeight = 1500;

        public bool UseAdvancedHeightRule;
        public float MaxCurveHeight = 500;
        public AnimationCurve HeightRuleCurve = new(new Keyframe[] { new(0, 1, 0, 0), new(1, 1, 0, 0) });

        public bool UseSteepnessRule = true;
        public float MinSteepness;
        public float MaxSteepness = 45;

        public bool UseAdvancedSteepnessRule;
        public AnimationCurve SteepnessRuleCurve = new(new Keyframe[] { new(0, 0, 0, 0), new(0.5f, 1, 0, 0) });

        public bool UseNoiseCutoff;
        public float NoiseCutoffValue = 0.33f;
        public float NoiseCutoffScale = 15;
        public bool NoiseCutoffInverse;
        public float2 NoiseCutoffOffset = new(0, 0);

        public bool UseNoiseDensity = true;
        public float NoiseDensityScale = 15;
        public float NoiseDensityBalancing = 0;
        public bool NoiseDensityInverse;
        public float2 NoiseDensityOffset = new(0, 0);

        public bool UseNoiseScaleRule = true;
        public float NoiseScaleMinScale = 0.8f;
        public float NoiseScaleMaxScale = 1.2f;
        public float NoiseScaleScale = 15;
        public float NoiseScaleBalancing = 0;
        public bool NoiseScaleInverse;
        public float2 NoiseScaleOffset = new(0, 0);

        public bool EnableCrossFade = true;
        public float LODFactor = 1;

        public bool UseDistanceFalloff = false;
        public float DistanceFalloffStartDistance = 0.75f;
        public bool UseAdvancedDistanceFalloff;
        public AnimationCurve distanceFalloffCurve = new(new Keyframe[] { new(0, 0.75f, 0, 0), new(1, 1, 0, 0) });

        public ShaderControllerSettings[] ShaderControllerSettings;
        public bool useShaderControllerOverrides = false;

        public bool UseBillboards = true;
        public float BillboardFadeOutDistance = 20;
        public float BillboardShadowOffset = 1;
        public float BillboardCutoff = 0.25f;
        public float BillboardBrightness = 1;
        public float BillboardNormalStrength = 1;
#if USING_HDRP
        public float BillboardSpecular = 0.5f;
#else
        public float BillboardSpecular = 0;
#endif
        public float BillboardOcclusion = 0.25f;

        public LODLevel BillboardSourceLODLevel = LODLevel.LOD0;
        public EBillboardAtlasColorSource eBillboardAtlasColorSource = EBillboardAtlasColorSource.TextureColorBake;
        public BillboardQuality BillboardQuality = BillboardQuality.Octa_128x;
        public bool BillboardRecalculateNormals;
        public float BillboardNormalBlendFactor = 1;
        public Texture2D BillboardTexture;
        public Texture2D BillboardNormalTexture;
        public float lastBillboardAtlasTilingRow = 1;
        public float lastBillboardAtlasTilingColumn = 8;
        public EBillboardAtlasColorSource lastBillboardAtlasColorSource = EBillboardAtlasColorSource.TextureColorBake;

        public ColliderType ColliderType = ColliderType.FromPrefab;
        public float ColliderRadius = 0.2f;
        public float ColliderHeight = 2;
        public float3 ColliderSize = Vector3.one;
        public float3 ColliderOffset = float3.zero;
        public Mesh ColliderMesh;
        public bool ColliderConvex;
        public bool ColliderTrigger;
        public string ColliderTag = "Untagged";
        public float ColliderDistanceFactor = 0.2f;
        public bool ColliderUseForBake = true;

        public NavMeshObstacleType NavMeshObstacleType = NavMeshObstacleType.Disabled;
        public int NavMeshArea;
        public float3 NavMeshObstacleCenter;
        public float3 NavMeshObstacleSize = Vector3.one;
        public float NavMeshObstacleRadius = 0.5f;
        public float NavMeshObstacleHeight = 2;
        public bool NavMeshObstacleCarve = true;

        public bool UseBiomeEdgeScaleRule;
        public float BiomeEdgeScaleDistance = 10f;
        public float BiomeEdgeScaleMinScale = 0.4f;
        public float BiomeEdgeScaleMaxScale = 1.2f;
        public bool BiomeEdgeScaleInverse;

        public bool UseBiomeEdgeIncludeRule;
        public float BiomeEdgeIncludeDistance = 10f;
        public bool BiomeEdgeIncludeInverse;

        public bool UseVegetationMask;
        public VegetationTypeIndex VegetationTypeIndex = VegetationTypeIndex.VegetationType1;

        public bool UseTextureMaskIncludeRules; // texture
        public List<TextureMaskRule> TextureMaskIncludeRuleList = new();

        public bool UseTextureMaskExcludeRules; // texture
        public List<TextureMaskRule> TextureMaskExcludeRuleList = new();

        public bool UseTextureMaskDensityRules; // texture
        public List<TextureMaskRule> TextureMaskDensityRuleList = new();

        public bool UseTextureMaskScaleRules;   // texture
        public List<TextureMaskRule> TextureMaskScaleRuleList = new();

        public bool UseTerrainTextureIncludeRules;  // terrain
        public List<TerrainTextureRule> TerrainTextureIncludeRuleList = new();

        public bool UseTerrainTextureExcludeRules;  // terrain
        public List<TerrainTextureRule> TerrainTextureExcludeRuleList = new();

        public bool UseTerrainTextureDensityRules;  // terrain
        public List<TerrainTextureRule> TerrainTextureDensityRuleList = new();

        public bool UseTerrainTextureScaleRules;    // terrain
        public List<TerrainTextureRule> TerrainTextureScaleRuleList = new();

        public bool UseConcaveLocationRule;
        public float ConcaveLocationDistance = 5;
        public float ConcaveLocationMinHeightDifference = 0.1f;
        public bool ConcaveLocationInverse;

        public bool UseTerrainSourceIncludeRule;
        public TerrainSourceRule TerrainSourceIncludeRule;
        public bool UseTerrainSourceExcludeRule;
        public TerrainSourceRule TerrainSourceExcludeRule;

        public List<RuntimePrefabRule> RuntimePrefabRuleList = new();

        public VegetationItemInfoPro()
        {

        }

        public VegetationItemInfoPro(VegetationItemInfoPro _sourceItem)
        {
            VegetationItemID = Guid.NewGuid().ToString();
            CopySettingValues(_sourceItem);
            Seed = UnityEngine.Random.Range(0, 101);
        }

        public void Init()  // set some randomized default values
        {
            Seed = UnityEngine.Random.Range(0, 101);

            switch (VegetationType)
            {
                case VegetationType.Grass:
                    SampleDistance = math.round(UnityEngine.Random.Range(0.4f, 1f) * 100) / 100;
                    RotationMode = VegetationRotationType.FollowTerrain;
                    break;
                case VegetationType.Plant:
                    SampleDistance = math.round(UnityEngine.Random.Range(1f, 1.6f) * 100) / 100;
                    RotationMode = VegetationRotationType.FollowTerrain;
                    break;
                case VegetationType.Objects:
                    SampleDistance = math.round(UnityEngine.Random.Range(1.6f, 3.2f) * 100) / 100;
                    RotationMode = VegetationRotationType.FollowTerrain;
                    break;
                case VegetationType.Tree:
                    SampleDistance = math.round(UnityEngine.Random.Range(5, 10) * 100) / 100;
                    RotationMode = VegetationRotationType.RotateY;
                    break;
                case VegetationType.LargeObjects:
                    SampleDistance = math.round(UnityEngine.Random.Range(5, 15) * 100) / 100;
                    RotationMode = VegetationRotationType.FollowTerrain;
                    break;
            }

            MinScale = math.round(UnityEngine.Random.Range(0.6f, 1f) * 100) / 100;
            MaxScale = math.round(UnityEngine.Random.Range(1.2f, 1.6f) * 100) / 100;

            int noiseScale = UnityEngine.Random.Range(5, 50);
            bool noiseInvert = UnityEngine.Random.Range(0, 1) > 0.5f;
            NoiseCutoffValue = math.round(UnityEngine.Random.Range(0.33f, 0.66f) * 100) / 100;
            NoiseCutoffScale = noiseScale;
            NoiseCutoffInverse = noiseInvert;

            NoiseDensityScale = noiseScale;
            NoiseDensityInverse = noiseInvert;

            NoiseScaleMinScale = math.round(UnityEngine.Random.Range(0.6f, 1) * 100) / 100;
            NoiseScaleMaxScale = math.round(UnityEngine.Random.Range(1.2f, 1.6f) * 100) / 100;
            NoiseScaleScale = noiseScale;
            NoiseScaleBalancing = math.round((noiseInvert ? UnityEngine.Random.Range(-0.25f, 0) : UnityEngine.Random.Range(0, 0.25f)) * 100) / 100;
            NoiseScaleInverse = noiseInvert;
        }

        public int GetDistanceBand()    // distance type for vegetation cell culling / shadow culling
        {
            if (VegetationType == VegetationType.Tree || VegetationType == VegetationType.LargeObjects)
                return 1;
            return 0;
        }

        public void CopySettingValues(VegetationItemInfoPro _sourceItem)
        {
            VegetationGuid = _sourceItem.VegetationGuid;

            VegetationType = _sourceItem.VegetationType;
            PrefabType = _sourceItem.PrefabType;
            Name = _sourceItem.Name;
            if (_sourceItem.PrefabType == VegetationPrefabType.Mesh) VegetationPrefab = _sourceItem.VegetationPrefab;
            if (_sourceItem.PrefabType == VegetationPrefabType.Texture) VegetationTexture = _sourceItem.VegetationTexture;
            VegetationRenderMode = _sourceItem.VegetationRenderMode;
            EnableRuntimeSpawn = _sourceItem.EnableRuntimeSpawn;
            DisableShadows = _sourceItem.DisableShadows;
            Seed = _sourceItem.Seed;
            RenderDistanceFactor = _sourceItem.RenderDistanceFactor;

            Density = _sourceItem.Density;
            SampleDistance = _sourceItem.SampleDistance;
            UseSamplePointOffset = _sourceItem.UseSamplePointOffset;
            SamplePointMinOffset = _sourceItem.SamplePointMinOffset;
            SamplePointMaxOffset = _sourceItem.SamplePointMaxOffset;

            RandomizePosition = _sourceItem.RandomizePosition;
            Offset = _sourceItem.Offset;
            MinUpOffset = _sourceItem.MinUpOffset;
            MaxUpOffset = _sourceItem.MaxUpOffset;
            RotationOffset = _sourceItem.RotationOffset;
            RotationMode = _sourceItem.RotationMode;

            Bounds = _sourceItem.Bounds;
            ScaleMultiplier = _sourceItem.ScaleMultiplier;
            MinScale = _sourceItem.MinScale;
            MaxScale = _sourceItem.MaxScale;
            useAdvancedScaleRule = _sourceItem.useAdvancedScaleRule;
            scaleRuleCurve = new AnimationCurve(_sourceItem.scaleRuleCurve.keys);

            UseHeightRule = _sourceItem.UseHeightRule;
            MinHeight = _sourceItem.MinHeight;
            MaxHeight = _sourceItem.MaxHeight;

            UseAdvancedHeightRule = _sourceItem.UseAdvancedHeightRule;
            MaxCurveHeight = _sourceItem.MaxCurveHeight;
            HeightRuleCurve = new AnimationCurve(_sourceItem.HeightRuleCurve.keys);

            UseSteepnessRule = _sourceItem.UseSteepnessRule;
            MinSteepness = _sourceItem.MinSteepness;
            MaxSteepness = _sourceItem.MaxSteepness;

            UseAdvancedSteepnessRule = _sourceItem.UseAdvancedSteepnessRule;
            SteepnessRuleCurve = new AnimationCurve(_sourceItem.SteepnessRuleCurve.keys);

            UseNoiseCutoff = _sourceItem.UseNoiseCutoff;
            NoiseCutoffValue = _sourceItem.NoiseCutoffValue;
            NoiseCutoffScale = _sourceItem.NoiseCutoffScale;
            NoiseCutoffInverse = _sourceItem.NoiseCutoffInverse;
            NoiseCutoffOffset = _sourceItem.NoiseCutoffOffset;

            UseNoiseDensity = _sourceItem.UseNoiseDensity;
            NoiseDensityScale = _sourceItem.NoiseDensityScale;
            NoiseDensityBalancing = _sourceItem.NoiseDensityBalancing;
            NoiseDensityInverse = _sourceItem.NoiseDensityInverse;
            NoiseDensityOffset = _sourceItem.NoiseDensityOffset;

            UseNoiseScaleRule = _sourceItem.UseNoiseScaleRule;
            NoiseScaleMinScale = _sourceItem.NoiseScaleMinScale;
            NoiseScaleMaxScale = _sourceItem.NoiseScaleMaxScale;
            NoiseScaleScale = _sourceItem.NoiseScaleScale;
            NoiseScaleBalancing = _sourceItem.NoiseScaleBalancing;
            NoiseScaleInverse = _sourceItem.NoiseScaleInverse;
            NoiseScaleOffset = _sourceItem.NoiseScaleOffset;

            EnableCrossFade = _sourceItem.EnableCrossFade;
            LODFactor = _sourceItem.LODFactor;

            UseDistanceFalloff = _sourceItem.UseDistanceFalloff;
            DistanceFalloffStartDistance = _sourceItem.DistanceFalloffStartDistance;
            UseAdvancedDistanceFalloff = _sourceItem.UseAdvancedDistanceFalloff;
            distanceFalloffCurve = new AnimationCurve(_sourceItem.distanceFalloffCurve.keys);

            ShaderControllerSettings = null;    // null to re-create -- don't copy material settings
            useShaderControllerOverrides = _sourceItem.useShaderControllerOverrides;

            UseBillboards = _sourceItem.UseBillboards;
            BillboardFadeOutDistance = _sourceItem.BillboardFadeOutDistance;
            BillboardShadowOffset = _sourceItem.BillboardShadowOffset;
            BillboardCutoff = _sourceItem.BillboardCutoff;
            BillboardBrightness = _sourceItem.BillboardBrightness;
            BillboardNormalStrength = _sourceItem.BillboardNormalStrength;
            BillboardSpecular = _sourceItem.BillboardSpecular;
            BillboardOcclusion = _sourceItem.BillboardOcclusion;

            BillboardQuality = _sourceItem.BillboardQuality;
            BillboardSourceLODLevel = _sourceItem.BillboardSourceLODLevel;
            eBillboardAtlasColorSource = _sourceItem.eBillboardAtlasColorSource;
            BillboardRecalculateNormals = _sourceItem.BillboardRecalculateNormals;
            BillboardNormalBlendFactor = _sourceItem.BillboardNormalBlendFactor;
            BillboardTexture = _sourceItem.BillboardTexture;
            BillboardNormalTexture = _sourceItem.BillboardNormalTexture;
            lastBillboardAtlasTilingRow = _sourceItem.lastBillboardAtlasTilingRow;
            lastBillboardAtlasTilingColumn = _sourceItem.lastBillboardAtlasTilingColumn;
            lastBillboardAtlasColorSource = _sourceItem.lastBillboardAtlasColorSource;

            ColliderType = _sourceItem.ColliderType;
            ColliderRadius = _sourceItem.ColliderRadius;
            ColliderHeight = _sourceItem.ColliderHeight;
            ColliderSize = _sourceItem.ColliderSize;
            ColliderOffset = _sourceItem.ColliderOffset;
            ColliderMesh = _sourceItem.ColliderMesh;
            ColliderTrigger = _sourceItem.ColliderTrigger;
            ColliderConvex = _sourceItem.ColliderConvex;
            ColliderDistanceFactor = _sourceItem.ColliderDistanceFactor;
            ColliderUseForBake = _sourceItem.ColliderUseForBake;

            NavMeshObstacleType = _sourceItem.NavMeshObstacleType;
            NavMeshArea = _sourceItem.NavMeshArea;
            NavMeshObstacleCenter = _sourceItem.NavMeshObstacleCenter;
            NavMeshObstacleSize = _sourceItem.NavMeshObstacleSize;
            NavMeshObstacleRadius = _sourceItem.NavMeshObstacleRadius;
            NavMeshObstacleHeight = _sourceItem.NavMeshObstacleHeight;
            NavMeshObstacleCarve = _sourceItem.NavMeshObstacleCarve;

            UseBiomeEdgeScaleRule = _sourceItem.UseBiomeEdgeScaleRule;
            BiomeEdgeScaleDistance = _sourceItem.BiomeEdgeScaleDistance;
            BiomeEdgeScaleMinScale = _sourceItem.BiomeEdgeScaleMinScale;
            BiomeEdgeScaleMaxScale = _sourceItem.BiomeEdgeScaleMaxScale;
            BiomeEdgeScaleInverse = _sourceItem.BiomeEdgeScaleInverse;

            UseBiomeEdgeIncludeRule = _sourceItem.UseBiomeEdgeIncludeRule;
            BiomeEdgeIncludeDistance = _sourceItem.BiomeEdgeIncludeDistance;
            BiomeEdgeIncludeInverse = _sourceItem.BiomeEdgeIncludeInverse;

            UseVegetationMask = _sourceItem.UseVegetationMask;
            VegetationTypeIndex = _sourceItem.VegetationTypeIndex;

            UseTextureMaskIncludeRules = _sourceItem.UseTextureMaskIncludeRules;    // texture
            for (int i = 0; i < _sourceItem.TextureMaskIncludeRuleList.Count; i++)
                TextureMaskIncludeRuleList.Add(new TextureMaskRule(_sourceItem.TextureMaskIncludeRuleList[i]));

            UseTextureMaskExcludeRules = _sourceItem.UseTextureMaskExcludeRules;    // texture
            for (int i = 0; i < _sourceItem.TextureMaskExcludeRuleList.Count; i++)
                TextureMaskExcludeRuleList.Add(new TextureMaskRule(_sourceItem.TextureMaskExcludeRuleList[i]));

            UseTextureMaskScaleRules = _sourceItem.UseTextureMaskScaleRules;    // texture
            for (int i = 0; i < _sourceItem.TextureMaskScaleRuleList.Count; i++)
                TextureMaskScaleRuleList.Add(new TextureMaskRule(_sourceItem.TextureMaskScaleRuleList[i]));

            UseTextureMaskDensityRules = _sourceItem.UseTextureMaskDensityRules;    // texture
            for (int i = 0; i < _sourceItem.TextureMaskDensityRuleList.Count; i++)
                TextureMaskDensityRuleList.Add(new TextureMaskRule(_sourceItem.TextureMaskDensityRuleList[i]));

            UseTerrainTextureIncludeRules = _sourceItem.UseTerrainTextureIncludeRules;
            for (int i = 0; i < _sourceItem.TerrainTextureIncludeRuleList.Count; i++)   // terrain
                TerrainTextureIncludeRuleList.Add(new TerrainTextureRule(_sourceItem.TerrainTextureIncludeRuleList[i]));

            UseTerrainTextureExcludeRules = _sourceItem.UseTerrainTextureExcludeRules;
            for (int i = 0; i < _sourceItem.TerrainTextureExcludeRuleList.Count; i++)   // terrain
                TerrainTextureExcludeRuleList.Add(new TerrainTextureRule(_sourceItem.TerrainTextureExcludeRuleList[i]));

            UseTerrainTextureDensityRules = _sourceItem.UseTerrainTextureDensityRules;
            for (int i = 0; i < _sourceItem.TerrainTextureDensityRuleList.Count; i++)   // terrain
                TerrainTextureDensityRuleList.Add(new TerrainTextureRule(_sourceItem.TerrainTextureDensityRuleList[i]));

            UseTerrainTextureScaleRules = _sourceItem.UseTerrainTextureScaleRules;
            for (int i = 0; i < _sourceItem.TerrainTextureScaleRuleList.Count; i++)   // terrain
                TerrainTextureScaleRuleList.Add(new TerrainTextureRule(_sourceItem.TerrainTextureScaleRuleList[i]));

            UseConcaveLocationRule = _sourceItem.UseConcaveLocationRule;
            ConcaveLocationDistance = _sourceItem.ConcaveLocationDistance;
            ConcaveLocationMinHeightDifference = _sourceItem.ConcaveLocationMinHeightDifference;

            UseTerrainSourceIncludeRule = _sourceItem.UseTerrainSourceIncludeRule;
            TerrainSourceIncludeRule = _sourceItem.TerrainSourceIncludeRule;

            UseTerrainSourceExcludeRule = _sourceItem.UseTerrainSourceExcludeRule;
            TerrainSourceExcludeRule = _sourceItem.TerrainSourceExcludeRule;

            for (int i = 0; i < _sourceItem.RuntimePrefabRuleList.Count; i++)
                RuntimePrefabRuleList.Add(new RuntimePrefabRule(_sourceItem.RuntimePrefabRuleList[i]));
        }
    }

    [Serializable]
    public class VegetationPackagePro : ScriptableObject
    {
        public string PackageName = "No name";
        public List<VegetationItemInfoPro> VegetationInfoList = new();
        public List<TextureMaskGroup> TextureMaskGroupList = new();
        public List<TerrainTextureInfo> TerrainTextureList = new();
        public List<TerrainTextureSettings> TerrainTextureSettingsList = new();
        public BiomeType BiomeType = BiomeType.Default;
        public int BiomeSortOrder = 1;
        public int TerrainTextureCount;
        public bool GenerateBiomeSplatmap = true;

        public void RegenerateVegetationItemIDs()
        {
            for (int i = 0; i < VegetationInfoList.Count; i++)
                VegetationInfoList[i].VegetationItemID = Guid.NewGuid().ToString();
        }

        public string GetVegetationItemID(string _assetGuid)    // get the first matching vegItemID using its GUID
        {
            for (int i = 0; i < VegetationInfoList.Count; i++)
                if (VegetationInfoList[i].VegetationGuid == _assetGuid)
                    return VegetationInfoList[i].VegetationItemID;
            return "";
        }

        public VegetationItemInfoPro GetVegetationInfo(string _vegetationItemID)
        {
            for (int i = 0; i < VegetationInfoList.Count; i++)
                if (VegetationInfoList[i].VegetationItemID == _vegetationItemID)
                    return VegetationInfoList[i];
            return null;
        }

        public TextureMaskGroup GetTextureMaskGroup(string _textureMaskGroupID)
        {
            for (int i = 0; i < TextureMaskGroupList.Count; i++)
                if (TextureMaskGroupList[i].TextureMaskGroupID == _textureMaskGroupID)
                    return TextureMaskGroupList[i];
            return null;
        }

        public void DeleteTextureMaskGroup(TextureMaskGroup _textureMaskGroup)
        {
            TextureMaskGroupList.Remove(_textureMaskGroup);
            for (int i = 0; i < VegetationInfoList.Count; i++)
            {
                for (int j = VegetationInfoList[i].TextureMaskIncludeRuleList.Count - 1; j >= 0; j--)
                    if (VegetationInfoList[i].TextureMaskIncludeRuleList[j].TextureMaskGroupID == _textureMaskGroup.TextureMaskGroupID)
                        VegetationInfoList[i].TextureMaskIncludeRuleList.RemoveAt(j);

                if (VegetationInfoList[i].TextureMaskIncludeRuleList.Count == 0)
                    VegetationInfoList[i].UseTextureMaskIncludeRules = false;
            }
        }

        public void ResizeTerrainTextureList(int _newCount)
        {
            if (_newCount <= 0)
                TerrainTextureList.Clear();
            else
                while (TerrainTextureList.Count > _newCount)
                    TerrainTextureList.RemoveAt(TerrainTextureList.Count - 1);
        }

        public void ResizeTerrainTextureSettingsList(int _newCount)
        {
            if (_newCount <= 0)
                TerrainTextureSettingsList.Clear();
            else
                while (TerrainTextureSettingsList.Count > _newCount)
                    TerrainTextureSettingsList.RemoveAt(TerrainTextureSettingsList.Count - 1);
        }

        public void PrepareTextureArrays()  // create native arrays for terrain texture splat map generation -- check whether everything is valid
        {
            for (int i = 0; i < TerrainTextureSettingsList.Count; i++)  // for all terrain textures added to the vegetation package
            {
                if (TerrainTextureSettingsList[i].HeightCurveArray.IsCreated)   // check existing array
                    TerrainTextureSettingsList[i].HeightCurveArray.Dispose();   // dispose since a new one gets assigned
                TerrainTextureSettingsList[i].HeightCurveArray = new NativeArray<float>(4096, Allocator.Persistent);    // create a new one

                if (ValidateAnimationCurve(TerrainTextureSettingsList[i].TextureHeightCurve) == false)  // check whether the animation curve has errors
                    TerrainTextureSettingsList[i].TextureHeightCurve = ResetAnimationCurve(); // reset it in such a case
                TerrainTextureSettingsList[i].HeightCurveArray.CopyFromFast(TerrainTextureSettingsList[i].TextureHeightCurve.GenerateCurveArray(4096)); // copy data from the animation curve into the array

                ///

                if (TerrainTextureSettingsList[i].SteepnessCurveArray.IsCreated)    // check existing array
                    TerrainTextureSettingsList[i].SteepnessCurveArray.Dispose();    // dispose since a new one gets assigned
                TerrainTextureSettingsList[i].SteepnessCurveArray = new NativeArray<float>(4096, Allocator.Persistent); // create a new one

                if (ValidateAnimationCurve(TerrainTextureSettingsList[i].TextureSteepnessCurve) == false)   // check whether the animation curve has errors
                    TerrainTextureSettingsList[i].TextureSteepnessCurve = ResetAnimationCurve();  // reset it in such a case
                TerrainTextureSettingsList[i].SteepnessCurveArray.CopyFromFast(TerrainTextureSettingsList[i].TextureSteepnessCurve.GenerateCurveArray(4096));   // copy data from the animation curve into the array
            }
        }

        private bool ValidateAnimationCurve(AnimationCurve _curve)
        {
            float sample = _curve.Evaluate(0.5f);
            if (float.IsNaN(sample))
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Curve error", "A problem with a splat map curve has been found and it has been reset", "OK");
#endif
                return false;
            }
            return true;
        }

        private AnimationCurve ResetAnimationCurve()
        {
            return new(new Keyframe[] { new(0, 0, 0, 0), new(1, 1, 0, 0) });
        }

        public void DisposeTextureArrays()
        {
            for (int i = 0; i < TerrainTextureSettingsList.Count; i++)  // for all terrain textures added to the vegetation package
            {
                if (TerrainTextureSettingsList[i].HeightCurveArray.IsCreated)
                    TerrainTextureSettingsList[i].HeightCurveArray.Dispose();

                if (TerrainTextureSettingsList[i].SteepnessCurveArray.IsCreated)
                    TerrainTextureSettingsList[i].SteepnessCurveArray.Dispose();
            }
        }

        public void LoadDefaultTextures()
        {
            if (TerrainTextureCount == 0) return;

            if (TerrainTextureSettingsList.Count < TerrainTextureCount)
                for (int i = TerrainTextureSettingsList.Count; i < TerrainTextureCount; i++)
                {
                    TerrainTextureSettings terrainTextureSettings = new();
                    terrainTextureSettings.Enabled = i < 4;
                    TerrainTextureSettingsList.Add(terrainTextureSettings);
                }

            if (TerrainTextureList.Count == 0)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture1", "TerrainTextures/TerrainTexture1_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture2", "TerrainTextures/TerrainTexture2_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture3", "TerrainTextures/TerrainTexture3_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture4", "TerrainTextures/TerrainTexture4_n", new float2(8, 8)));
            }

            if (TerrainTextureCount == 4) return;
            if (TerrainTextureList.Count == 4)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture5", "TerrainTextures/TerrainTexture5_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture6", "TerrainTextures/TerrainTexture6_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture7", "TerrainTextures/TerrainTexture7_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture8", "TerrainTextures/TerrainTexture8_n", new float2(8, 8)));
            }

            if (TerrainTextureCount == 8) return;
            if (TerrainTextureList.Count == 8)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture9", "TerrainTextures/TerrainTexture9_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture10", "TerrainTextures/TerrainTexture10_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture11", "TerrainTextures/TerrainTexture11_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture12", "TerrainTextures/TerrainTexture12_n", new float2(8, 8)));
            }

            if (TerrainTextureCount == 12) return;
            if (TerrainTextureList.Count == 12)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture13", "TerrainTextures/TerrainTexture13_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture14", "TerrainTextures/TerrainTexture14_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture15", "TerrainTextures/TerrainTexture15_n", new float2(8, 8)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture16", "TerrainTextures/TerrainTexture16_n", new float2(8, 8)));
            }

            if (TerrainTextureCount == 16) return;
            if (TerrainTextureList.Count == 16)
            {
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);

                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);

                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);

                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
                TerrainTextureList.Add(null);
            }
        }

        private TerrainTextureInfo LoadTexture(string _textureName, string _normalTextureName, float2 _uv)
        {
            TerrainTextureInfo newInfo = new() { TileSize = _uv };
            if (_textureName != "") newInfo.Texture = Resources.Load<Texture2D>(_textureName);
            if (_normalTextureName != "") newInfo.TextureNormals = Resources.Load<Texture2D>(_normalTextureName);
            return newInfo;
        }

        public void AddVegetationItem(Texture2D _texture, VegetationType _vegetationType, bool _enableRuntimeSpawn = true, string _newVegetationItemID = "")
        {
            VegetationItemInfoPro vegetationItemInfoPro = new()
            {
                VegetationPrefab = null,
                VegetationTexture = _texture,
                PrefabType = VegetationPrefabType.Texture,
                VegetationType = _vegetationType
            };

            vegetationItemInfoPro.VegetationItemID = _newVegetationItemID == "" ? Guid.NewGuid().ToString() : _newVegetationItemID;
#if UNITY_EDITOR
            vegetationItemInfoPro.VegetationGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_texture));
#endif

            vegetationItemInfoPro.Name = _texture ? _texture.name : "Missing texture";
            vegetationItemInfoPro.EnableRuntimeSpawn = _enableRuntimeSpawn;
            vegetationItemInfoPro.Init();

            VegetationInfoList.Add(vegetationItemInfoPro);  // add to the list for further processing/searching/validation 

            //if (vegetationItemInfoPro.VegetationType == VegetationType.Grass || vegetationItemInfoPro.VegetationType == VegetationType.Plant)
            //    GenerateBillboard(vegetationItemInfoPro.VegetationItemID);
        }

        public void AddVegetationItem(GameObject _vegetationPrefab, VegetationType _vegetationType, bool _enableRuntimeSpawn = true, string _newVegetationItemID = "")
        {
            VegetationItemInfoPro vegetationItemInfoPro = new()
            {
                VegetationPrefab = _vegetationPrefab,
                VegetationTexture = null,
                PrefabType = VegetationPrefabType.Mesh,
                VegetationType = _vegetationType
            };

            vegetationItemInfoPro.VegetationItemID = _newVegetationItemID == "" ? Guid.NewGuid().ToString() : _newVegetationItemID;
#if UNITY_EDITOR
            vegetationItemInfoPro.VegetationGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_vegetationPrefab));
#endif

            vegetationItemInfoPro.Name = _vegetationPrefab ? _vegetationPrefab.name : "Missing prefab";
            vegetationItemInfoPro.EnableRuntimeSpawn = _enableRuntimeSpawn;
            vegetationItemInfoPro.Init();

            VegetationInfoList.Add(vegetationItemInfoPro);  // add to the list for further processing/searching/validation 

            if (vegetationItemInfoPro.VegetationType == VegetationType.Tree) //|| vegetationItemInfoPro.VegetationType == VegetationType.Grass || vegetationItemInfoPro.VegetationType == VegetationType.Plant)
                GenerateBillboard(vegetationItemInfoPro.VegetationItemID);
        }

        public void DuplicateVegetationItem(VegetationItemInfoPro _vegetationItemInfo)
        {
            VegetationItemInfoPro newVegetationItemInfo = new(_vegetationItemInfo);
            newVegetationItemInfo.Name += "_Copy";
            VegetationInfoList.Add(newVegetationItemInfo);
        }

        public void GenerateBillboard(string _vegetationItemID)
        {
            GenerateBillboard(GetVegetationItemIndexFromID(_vegetationItemID));
        }

        public int GetVegetationItemIndexFromID(string _id)
        {
            for (int i = 0; i < VegetationInfoList.Count; i++)
                if (VegetationInfoList[i].VegetationItemID == _id)
                    return i;
            return -1;
        }

        public void GenerateBillboard(int _vegetationItemIndex)
        {
#if UNITY_EDITOR
            if (VegetationInfoList.Count <= 0 || _vegetationItemIndex <= -1)
                return; // skip empty vegetation package when batch editing

            VegetationItemInfoPro vegItemInfoPro = VegetationInfoList[_vegetationItemIndex];    // get items' info
            if (vegItemInfoPro.UseBillboards == false || vegItemInfoPro.VegetationItemID == null)
                return; // skip vegetation items that shouldn't use billboards

            // prepare paths and folders for the billboard textures
            string assetPath = AssetDatabase.GetAssetPath(this);
            string directory = Path.GetDirectoryName(assetPath);
            string folderName = Path.GetFileNameWithoutExtension(assetPath) + "_BillboardTextures";

            if (AssetDatabase.IsValidFolder(directory + "/" + folderName) == false) // if no existing folder found then create one
                AssetDatabase.CreateFolder(directory, folderName);

            string billboardPath = directory + "/" + folderName + "/" + vegItemInfoPro.Name + "_billboard";
            string billboardPathNormal = billboardPath + "Normal";
            string billboardPathTexture = billboardPath + ".png";
            string billboardPathNormalTexture = billboardPathNormal + ".png";

            // prepare texture fields -- check for existing billboard textures and delete them
            Texture2D billboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardPathTexture);
            if (billboardTexture) AssetDatabase.DeleteAsset(billboardPathTexture);

            Texture2D billboardNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardPathNormalTexture);
            if (billboardNormalTexture) AssetDatabase.DeleteAsset(billboardPathNormalTexture);

            // get the shaders for rendering later -- get an override billboard shader if any using the item's "IShaderController"
            string overrideBillboardAtlasShader = "";
            string overrideBillboardAtlasNormalShader = "";

            for (int i = 0; i < vegItemInfoPro.ShaderControllerSettings?.Length; i++)
                if (vegItemInfoPro.ShaderControllerSettings[i] != null)
                {
                    overrideBillboardAtlasShader = vegItemInfoPro.ShaderControllerSettings[i].overrideBillboardAtlasShader;
                    overrideBillboardAtlasNormalShader = vegItemInfoPro.ShaderControllerSettings[i].overrideBillboardAtlasNormalShader;
                }

            Shader diffuseShader = overrideBillboardAtlasShader != "" ? Shader.Find(overrideBillboardAtlasShader) : ShaderUtility.GetShader_UtilityVegetationColorMask();
            Shader normalShader = overrideBillboardAtlasNormalShader != "" ? Shader.Find(overrideBillboardAtlasNormalShader) : ShaderUtility.GetShader_UtilityVegetationNormalMask();

            // generate diffuse texture
            EditorUtility.DisplayProgressBar("Generate billboard atlas", "Diffuse -- " + vegItemInfoPro.Name, 0);   // display progress bar -- set to zero
            billboardTexture = BillboardAtlasRenderer.GenerateBillboardTexture(vegItemInfoPro, diffuseShader, false);
            BillboardAtlasRenderer.RemoveBillboardAtlasPixelBleed(billboardTexture, vegItemInfoPro.BillboardQuality);
            TextureExtension.SaveTextureToFile(billboardTexture, billboardPath);

            // generate normal texture
            EditorUtility.DisplayProgressBar("Generate billboard atlas", "Normal -- " + vegItemInfoPro.Name, 0.33f);    // set progress to 33
            billboardNormalTexture = BillboardAtlasRenderer.GenerateBillboardTexture(vegItemInfoPro, normalShader, true);
            BillboardAtlasRenderer.RemoveBillboardAtlasPixelBleed(billboardNormalTexture, vegItemInfoPro.BillboardQuality);
            TextureExtension.SaveTextureToFile(billboardNormalTexture, billboardPathNormal);

            // save textures into the project -- manual texture import
            EditorUtility.DisplayProgressBar("Generate billboard atlas", "Importing textures -- " + vegItemInfoPro.Name, 0.66f);    // set progress to 66
            TextureExtension.ImportTexture(billboardPathTexture, 0, 4096);
            TextureExtension.ImportTexture(billboardPathNormalTexture, 1, 4096);

            // setup vegetation item with newly generated files -- save textures for internal logic when creating the billboard material
            vegItemInfoPro.BillboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardPathTexture);
            vegItemInfoPro.BillboardNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardPathNormalTexture);

            // save last generation settings for internal logic when creating/updating the billboard material
            vegItemInfoPro.lastBillboardAtlasTilingRow = BillboardAtlasRenderer.GetBillboardQualityRowCount(vegItemInfoPro.BillboardQuality);
            vegItemInfoPro.lastBillboardAtlasTilingColumn = BillboardAtlasRenderer.GetBillboardQualityColumnCount(vegItemInfoPro.BillboardQuality);
            vegItemInfoPro.lastBillboardAtlasColorSource = vegItemInfoPro.eBillboardAtlasColorSource;

            EditorUtility.ClearProgressBar();
#endif
        }
    }
}