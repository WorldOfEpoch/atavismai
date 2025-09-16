using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace AwesomeTechnologies.VegetationSystem
{
    public struct ARGBBytes // bytes for splat map vegetation rules
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Terrains/UnityTerrain", 0)]
    [ScriptExecutionOrder(-100)]
    [ExecuteInEditMode]
    public partial class UnityTerrain : MonoBehaviour, IVegetationStudioTerrain
    {
        public struct RelativeHeightmapData // relative data for splat map generation
        {
            public float Height;
            public float Steepness;
        }

        public Terrain Terrain;
        public TerrainSourceID TerrainSourceID;
        public bool DisableTerrainTreesAndDetails = true;
        [FormerlySerializedAs("AutoAddToVegegetationSystem")] public bool AutoAddToVegetationSystem;

        private bool initDone;
        public Rect terrainRect;    // area to compare against -- whether to include in calculations or skip
        public float2 holemapScale; // holemapScale => engine's "heightmapScale" ~equivalent since Unity doesn't provide this
        public float2 alphamapScale;    // alphamapScale => engine's "heightmapScale" ~equivalent since Unity doesn't provide this
        public NativeArray<bool> holes; // stores native data of all existing holes ..if any
        public NativeArray<float> heights;  // stores native data of all height info
        public readonly List<NativeArray<ARGBBytes>> splatmapArrayList = new(); // stores native data of all existing splat info
        public readonly List<int> splatmapFormatList = new();   // stores additional info about what format/index order to use -- whether ancient vs new terrain data

        private JobHandle splatmapHandle;   // temporary handle for splat map generation
        private NativeArray<float> currentSplatmapData; // temporary native storage for copying existing textures
        private NativeArray<RelativeHeightmapData> relativeHeightmapData;   // temporary relative native data

#if UNITY_EDITOR
        [SerializeField] private Material actualTerrainMaterial;    // the actual terrain material to use in the scene -- serialized to not null => cause issues when heatmap is active while entering play mode
        private Material terrainHeatmapMaterial;    // temporary heatmap material for previewing terrain texture rules
#endif

        ~UnityTerrain()
        {
            Dispose();
        }

        public string TerrainType => "Unity terrain";   // name to display in the terrain list / inspector UI

        public Bounds TerrainBounds // bounds used for ex: overlap queries, vegetation cell Y-Axis sampling, cell cache clearing
        {
            get
            {
                if (Terrain && Terrain.terrainData)
                    return new Bounds(Terrain.terrainData.bounds.center + transform.position, Terrain.terrainData.bounds.size);
                return new Bounds(float3.zero, float3.zero);
            }
        }

        void FindTerrain()  // safety find
        {
            if (Terrain == false)
                Terrain = gameObject.GetComponent<Terrain>();
        }

        void Reset()
        {
            FindTerrain();
        }

        void Awake()
        {
            FindTerrain();
        }

        void OnEnable()
        {
            Init();

            if (AutoAddToVegetationSystem)
                VegetationStudioManager.AddTerrain(gameObject, false, null);
            else
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
        }

        void Start()
        {
            if (Terrain && DisableTerrainTreesAndDetails)
                Terrain.drawTreesAndFoliage = false;
        }

        void OnDisable()
        {
            initDone = false;

            if (AutoAddToVegetationSystem)
                VegetationStudioManager.RemoveTerrain(gameObject);
            else
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);

            Dispose();
        }

        public void Dispose()
        {
            if (heights.IsCreated)
                heights.Dispose();

            if (holes.IsCreated)
                holes.Dispose();

            GC.SuppressFinalize(this);  // avoid running the "finalizer / destructor" on ex: scene exit
        }

        public void OnHeightChanged(VegetationSystemPro _vspSys, RectInt _heightRegion)
        {
            RefreshTerrainData(false, false, true); // get terrain's height values from its height map

            float2 updatedPosition = new((_heightRegion.position.x * Terrain.terrainData.heightmapScale.x) + transform.position.x, (_heightRegion.position.y * Terrain.terrainData.heightmapScale.z) + transform.position.z);
            _vspSys.RefreshTerrainArea(RectExtension.CreateBoundsFromRect(new(updatedPosition, new int2(_heightRegion.size.x, _heightRegion.size.y) * ((float3)Terrain.terrainData.heightmapScale).xz)));   // reload spawning rules -- update cell heights/culling
        }

        public void OnTextureChanged(VegetationSystemPro _vspSys, string _textureName, RectInt _texelRegion)
        {
            bool isHole = _textureName == "holes";
            RefreshTerrainData(!isHole, isHole, false); // get terrain's splat/hole values from its splat/hole map

            float2 scale = isHole ? holemapScale : alphamapScale;
            float2 updatedPosition = new((_texelRegion.position.x * scale.x) + transform.position.x, (_texelRegion.position.y * scale.y) + transform.position.z);
            _vspSys.ClearCache(RectExtension.CreateBoundsFromRect(new(updatedPosition, new int2(_texelRegion.size.x, _texelRegion.size.y) * alphamapScale)));   // reload spawning rules
        }

        public void Init()
        {
#if UNITY_EDITOR
            if (actualTerrainMaterial)  // safety filter to ensure clean rendering -- for when the engine gets closed/crashes/exports a builds while the heat map is still active -- the engine runs this once when exporting a build too
                Terrain.materialTemplate = actualTerrainMaterial;
#endif
            Terrain.drawInstanced = true;   // enforce internal related terrain behavior -- instancing for rendering
            RefreshTerrainData(splatmapArrayList.Count < 1, !holes.IsCreated, !heights.IsCreated);  // ensure data exist to read from it for certain rules ex: steepness rule, terrain texture rules
            initDone = true;
        }

        public void RefreshTerrainData(bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            if (Terrain.terrainData == false)
                return;

            terrainRect = new Rect(new float2(transform.position.x, transform.position.z), new float2(Terrain.terrainData.size.x, Terrain.terrainData.size.z));

            if (_splatData)
            {
                alphamapScale = new(Terrain.terrainData.size.x / Terrain.terrainData.alphamapResolution, Terrain.terrainData.size.z / Terrain.terrainData.alphamapResolution);
                VerifySplatmapAccess(); // get updated splat data ..w/ correct compatibility format
            }

            if (_holesData)
            {   // get updated holes data
                holemapScale = new(Terrain.terrainData.size.x / Terrain.terrainData.holesResolution, Terrain.terrainData.size.z / Terrain.terrainData.holesResolution);
                if (holes.IsCreated) holes.Dispose();
                else
                {
                    holes = new(Terrain.terrainData.holesResolution * Terrain.terrainData.holesResolution, Allocator.Persistent);   // persistent as used by several jobs across several (sub-)systems
                    holes.CopyFromFast(Terrain.terrainData.GetHoles(0, 0, Terrain.terrainData.holesResolution, Terrain.terrainData.holesResolution));
                }
            }

            if (_heightsData)
            {   // get updated height data
                if (heights.IsCreated) heights.Dispose();
                else
                {
                    heights = new(Terrain.terrainData.heightmapResolution * Terrain.terrainData.heightmapResolution, Allocator.Persistent); // persistent as used by several jobs across several (sub-)systems
                    heights.CopyFromFast(Terrain.terrainData.GetHeights(0, 0, Terrain.terrainData.heightmapResolution, Terrain.terrainData.heightmapResolution));
                }
            }
        }

        public void RefreshTerrainData(Bounds _bounds, bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            if (RectExtension.CreateRectFromBounds(_bounds).Overlaps(RectExtension.CreateRectFromBounds(TerrainBounds)))
                RefreshTerrainData(_splatData, _holesData, _heightsData);
        }

        public JobHandle SampleCellHeight(NativeArray<Bounds> _vegetationCellBoundsList, float _worldspaceHeightCutoff, Rect _updateRect, JobHandle _dependsOn = default)
        {
            if (initDone == false)
                return _dependsOn;

            RefreshTerrainData(false, false, !heights.IsCreated);   // get terrain's height values from its height map -- update "terrainRect"
            if (_updateRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            UnityTerrainCellSampleJob unityTerrainCellSampleJob = new()
            {
                VegetationCellBoundsList = _vegetationCellBoundsList,
                WorldspaceHeightCutoff = _worldspaceHeightCutoff,

                TerrainRect = terrainRect,
                TerrainPositionY = transform.position.y,

                InputHeights = heights,
                HeightmapResolution = Terrain.terrainData.heightmapResolution,
                HeightmapScale = Terrain.terrainData.heightmapScale
            };
            return unityTerrainCellSampleJob.ScheduleParallel(_vegetationCellBoundsList.Length, 64, _dependsOn);
        }

        public JobHandle SampleTerrain(VegetationInstanceData _instanceData, int _sampleCount, Rect _cellRect, JobHandle _dependsOn)
        {
            if (initDone == false)
                return _dependsOn;

            RefreshTerrainData(false, !holes.IsCreated, !heights.IsCreated);    // get terrain's height values from its height map -- update "terrainRect"
            if (_cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            UnityTerrainNodeSampleJob unityTerrainNodeSampleJob = new()
            {
                Position = _instanceData.position,
                Rotation = _instanceData.rotation,
                Scale = _instanceData.scale,
                TerrainNormal = _instanceData.terrainNormal,
                ControlData = _instanceData.controlData,
                TerrainSourceID = _instanceData.terrainSourceID,
                Included = _instanceData.included,

                TerrainSourceIDThis = (byte)TerrainSourceID,
                TerrainPosition = transform.position,
                TerrainSize = Terrain.terrainData.size,

                InputHoles = holes,
                HolesResolution = Terrain.terrainData.holesResolution,

                InputHeights = heights,
                HeightmapResolution = Terrain.terrainData.heightmapResolution,
                HeightmapScale = Terrain.terrainData.heightmapScale
            };
            return unityTerrainNodeSampleJob.ScheduleParallel(_sampleCount, 64, _dependsOn);
        }

        public JobHandle SampleConcaveLocation(VegetationInstanceData _instanceData, float _minHeightDifference, float _distancePerSample, bool _inverse, Rect _cellRect, JobHandle _dependsOn)
        {
            if (initDone == false)
                return _dependsOn;

            RefreshTerrainData(false, false, !heights.IsCreated);   // get terrain's height values from its height map -- update "terrainRect"
            if (_cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            UnityTerrainConcaveIncludeJob unityTerrainConcaveIncludeJob = new()
            {
                Included = _instanceData.included,
                Position = _instanceData.position,

                InputHeights = heights,
                HeightmapResolution = Terrain.terrainData.heightmapResolution,
                HeightmapScale = Terrain.terrainData.heightmapScale,
                TerrainPositionXZ = new float2(transform.position.x, transform.position.z),

                DistancePerSampleX = (int)math.round(_distancePerSample / Terrain.terrainData.heightmapScale.x),
                DistancePerSampleZ = (int)math.round(_distancePerSample / Terrain.terrainData.heightmapScale.z),
                MinHeightDifference = _minHeightDifference,
                Inverse = _inverse
            };
            return unityTerrainConcaveIncludeJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public void VerifySplatmapAccess()
        {
            splatmapArrayList.Clear();
            splatmapFormatList.Clear();

            if (Terrain == null || Terrain.terrainData == null)
                return;

            for (int i = 0; i < Terrain.terrainData.alphamapTextures.Length; i++)
            {   // done to avoid things like GC at run-time ..and also due to the format list especially
                splatmapArrayList.Add(Terrain.terrainData.alphamapTextures[i].GetRawTextureData<ARGBBytes>());  // get new updated "splatmap" data from the terrain => after hand/scripted painting

                if (Terrain.terrainData.alphamapTextures[i].format == TextureFormat.RGBA32) // compatibility filter
                    splatmapFormatList.Add(1);
                else
                    splatmapFormatList.Add(0);
            }
        }

        public JobHandle ProcessSplatmapIncludeRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            if (Terrain == null || _cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            for (int i = 0; i < _terrainTextureRuleList.Count; i++)
            {
                int splatmapChunkIndex = _terrainTextureRuleList[i].TextureIndex / 4;   // floor to int -- get current chunk/pass of the data
                if (splatmapChunkIndex >= splatmapArrayList.Count)
                    return _dependsOn;  // async filter

                int splatmapTextureIndex = _terrainTextureRuleList[i].TextureIndex - (4 * splatmapChunkIndex);  // get actual texture index within current chunk
                if (splatmapFormatList[splatmapChunkIndex] == 1)    // old vs new terrainData compatibility adjustment
                {
                    splatmapTextureIndex--;
                    if (splatmapTextureIndex == -1)
                        splatmapTextureIndex = 3;
                }

                SplatmapIncludeJob splatmapIncludeJob = new()
                {
                    Position = _instanceData.position,
                    ControlData = _instanceData.controlData,
                    Included = _instanceData.included,

                    MinBrightness = (int)math.round(_terrainTextureRuleList[i].MinimumValue * 255),
                    MaxBrightness = (int)math.round(_terrainTextureRuleList[i].MaximumValue * 255),
                    Inverse = _terrainTextureRuleList[i].Inverse,

                    SplatmapChunkArray = splatmapArrayList[splatmapChunkIndex],
                    SplatmapTextureIndex = splatmapTextureIndex,
                    AlphamapResolution = Terrain.terrainData.alphamapResolution,
                    TexelSize = alphamapScale,
                    TerrainPosition = transform.position
                };
                _dependsOn = splatmapIncludeJob.Schedule(_instanceData.included, 64, _dependsOn);
            }

            IncludeEvaluationJob includeEvaluationJob = new() { Included = _instanceData.included, ControlData = _instanceData.controlData };
            return includeEvaluationJob.Schedule(_instanceData.included, 64, _dependsOn);   // ilbc of the cell spawner
        }

        public JobHandle ProcessSplatmapExcludeRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            if (Terrain == null || _cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            for (int i = 0; i < _terrainTextureRuleList.Count; i++)
            {
                int splatmapChunkIndex = _terrainTextureRuleList[i].TextureIndex / 4;   // floor to int -- get current chunk/pass of the data
                if (splatmapChunkIndex >= splatmapArrayList.Count)
                    return _dependsOn;  // async filter

                int splatmapTextureIndex = _terrainTextureRuleList[i].TextureIndex - (4 * splatmapChunkIndex);  // get actual texture index within current chunk
                if (splatmapFormatList[splatmapChunkIndex] == 1)    // old vs new terrainData compatibility adjustment
                {
                    splatmapTextureIndex--;
                    if (splatmapTextureIndex == -1)
                        splatmapTextureIndex = 3;
                }

                SplatmapExcludeJob splatmapExcludeJob = new()
                {
                    Position = _instanceData.position,
                    Included = _instanceData.included,

                    MinBrightness = (int)math.round(_terrainTextureRuleList[i].MinimumValue * 255),
                    MaxBrightness = (int)math.round(_terrainTextureRuleList[i].MaximumValue * 255),
                    Inverse = _terrainTextureRuleList[i].Inverse,

                    SplatmapChunkArray = splatmapArrayList[splatmapChunkIndex],
                    SplatmapTextureIndex = splatmapTextureIndex,
                    AlphamapResolution = Terrain.terrainData.alphamapResolution,
                    TexelSize = alphamapScale,
                    TerrainPosition = transform.position
                };
                _dependsOn = splatmapExcludeJob.Schedule(_instanceData.included, 64, _dependsOn);
            }

            return _dependsOn;
        }

        public JobHandle ProcessSplatmapDensityRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            if (Terrain == null || _cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            for (int i = 0; i < _terrainTextureRuleList.Count; i++)
            {
                int splatmapChunkIndex = _terrainTextureRuleList[i].TextureIndex / 4;   // floor to int -- get current chunk/pass of the data
                if (splatmapChunkIndex >= splatmapArrayList.Count)
                    return _dependsOn;  // async filter

                int splatmapTextureIndex = _terrainTextureRuleList[i].TextureIndex - (4 * splatmapChunkIndex);  // get actual texture index within current chunk
                if (splatmapFormatList[splatmapChunkIndex] == 1)    // old vs new terrainData compatibility adjustment
                {
                    splatmapTextureIndex--;
                    if (splatmapTextureIndex == -1)
                        splatmapTextureIndex = 3;
                }

                SplatmapDensityJob splatmapDensityJob = new()
                {
                    Position = _instanceData.position,
                    ControlData = _instanceData.controlData,

                    DensityMultiplier = _terrainTextureRuleList[i].DensityMultiplier,
                    MinDensity = _terrainTextureRuleList[i].MinimumValue,
                    MaxDensity = _terrainTextureRuleList[i].MaximumValue,
                    BrightnessThreshold = (_terrainTextureRuleList[i].BrightnessThreshold * 255),
                    MinBrightness = (int)math.round(_terrainTextureRuleList[i].MinBrightness * 255),
                    MaxBrightness = (int)math.round(_terrainTextureRuleList[i].MaxBrightness * 255),
                    Inverse = _terrainTextureRuleList[i].Inverse,

                    SplatmapChunkArray = splatmapArrayList[splatmapChunkIndex],
                    SplatmapTextureIndex = splatmapTextureIndex,
                    AlphamapResolution = Terrain.terrainData.alphamapResolution,
                    TexelSize = alphamapScale,
                    TerrainPosition = transform.position
                };
                _dependsOn = splatmapDensityJob.Schedule(_instanceData.controlData, 64, _dependsOn);
            }

            return _dependsOn;
        }

        public JobHandle ProcessSplatmapScaleRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            if (Terrain == null || _cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            for (int i = 0; i < _terrainTextureRuleList.Count; i++)
            {
                int splatmapChunkIndex = _terrainTextureRuleList[i].TextureIndex / 4;   // floor to int -- get current chunk/pass of the data
                if (splatmapChunkIndex >= splatmapArrayList.Count)
                    return _dependsOn;  // async filter

                int splatmapTextureIndex = _terrainTextureRuleList[i].TextureIndex - (4 * splatmapChunkIndex);  // get actual texture index within current chunk
                if (splatmapFormatList[splatmapChunkIndex] == 1)    // old vs new terrainData compatibility adjustment
                {
                    splatmapTextureIndex--;
                    if (splatmapTextureIndex == -1)
                        splatmapTextureIndex = 3;
                }

                SplatmapScaleJob splatmapScaleJob = new()
                {
                    Position = _instanceData.position,
                    Scale = _instanceData.scale,
                    Included = _instanceData.included,

                    ScaleMultiplier = _terrainTextureRuleList[i].ScaleMultiplier,
                    MinScale = _terrainTextureRuleList[i].MinimumValue,
                    MaxScale = _terrainTextureRuleList[i].MaximumValue,
                    BrightnessThreshold = (_terrainTextureRuleList[i].BrightnessThreshold * 255),
                    MinBrightness = (int)math.round(_terrainTextureRuleList[i].MinBrightness * 255),
                    MaxBrightness = (int)math.round(_terrainTextureRuleList[i].MaxBrightness * 255),
                    Inverse = _terrainTextureRuleList[i].Inverse,

                    SplatmapChunkArray = splatmapArrayList[splatmapChunkIndex],
                    SplatmapTextureIndex = splatmapTextureIndex,
                    AlphamapResolution = Terrain.terrainData.alphamapResolution,
                    TexelSize = alphamapScale,
                    TerrainPosition = transform.position
                };
                _dependsOn = splatmapScaleJob.Schedule(_instanceData.included, 64, _dependsOn);
            }

            return _dependsOn;
        }
    }

    #region jobs
    [BurstCompile]
    public struct UnityTerrainCellSampleJob : IJobFor
    {
        public NativeArray<Bounds> VegetationCellBoundsList;
        [ReadOnly] public float WorldspaceHeightCutoff;

        [ReadOnly] public Rect TerrainRect;
        [ReadOnly] public float TerrainPositionY;

        [ReadOnly] public NativeArray<float> InputHeights;
        [ReadOnly] public int HeightmapResolution;
        [ReadOnly] public float3 HeightmapScale;

        public void Execute(int index)
        {
            Rect cellRect = RectExtension.CreateRectFromBounds(VegetationCellBoundsList[index]);
            if (TerrainRect.Overlaps(cellRect) == false)
                return; // skip when this terrain's (custom) bounds don't overlap with the given cell's bounds

            // store/create temporary (extreme) data to compare against/with -- amount of cells -- anchor position bottom left
            int xCount = (int)math.ceil(cellRect.width / HeightmapScale.x);
            int zCount = (int)math.ceil(cellRect.height / HeightmapScale.z);

            int xStart = (int)math.floor((VegetationCellBoundsList[index].min.x - TerrainRect.position.x) / HeightmapScale.x);
            int zStart = (int)math.floor((VegetationCellBoundsList[index].min.z - TerrainRect.position.y) / HeightmapScale.z);

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = xStart; x <= xStart + xCount; x++)
                for (int z = zStart; z <= zStart + zCount; z++)
                {   // get highest/lowest point of this terrain
                    float heightSample = GetHeight(x, z);
                    if (heightSample < minHeight)
                        minHeight = heightSample;

                    if (heightSample > maxHeight)
                        maxHeight = heightSample;
                }

            if (maxHeight + TerrainPositionY < WorldspaceHeightCutoff)
                return; // skip when below set "seaLevel" -- don't "enable" the cell

            bool enabled = VegetationCellBoundsList[index].center.y > -99999;   // whether the cell has been "enabled" (and sampled) already by another terrain

            Bounds t = new( // assign new Y-Axis bounds
                 new float3(VegetationCellBoundsList[index].center.x, (maxHeight + minHeight) * 0.5f + TerrainPositionY, VegetationCellBoundsList[index].center.z),
                 new float3(VegetationCellBoundsList[index].size.x, maxHeight - minHeight, VegetationCellBoundsList[index].size.z));

            if (enabled)
            {   // compare against existing bounds -- reset back to potentially already bigger bounds -- else use new bigger bounds
                t.min = new float3(t.min.x, math.min(t.min.y, VegetationCellBoundsList[index].min.y), t.min.z);
                t.max = new float3(t.max.x, math.max(t.max.y, VegetationCellBoundsList[index].max.y), t.max.z);
            }

            VegetationCellBoundsList[index] = t;
        }

        float GetHeight(int _x, int _y)
        {
            _x = math.clamp(_x, 0, HeightmapResolution - 1);
            _y = math.clamp(_y, 0, HeightmapResolution - 1);
            return InputHeights[_x + (_y * HeightmapResolution)] * HeightmapScale.y;
        }
    }

    [BurstCompile]
    public struct UnityTerrainNodeSampleJob : IJobFor
    {
        [NativeDisableParallelForRestriction] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<quaternion> Rotation;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<float3> TerrainNormal;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<byte> TerrainSourceID;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<byte> Included;

        [ReadOnly] public byte TerrainSourceIDThis;
        [ReadOnly] public float3 TerrainPosition;
        [ReadOnly] public float3 TerrainSize;

        [ReadOnly] public NativeArray<bool> InputHoles;
        [ReadOnly] public int HolesResolution;

        [ReadOnly] public NativeArray<float> InputHeights;
        [ReadOnly] public int HeightmapResolution;
        [ReadOnly] public float3 HeightmapScale;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0)
                return; // skip nodes that don't spawn anyway -- this comes from "density/cutoff/biome mask" rules > late stage randomize filter

            float2 interpolatedPosition = new((Position[index].x - TerrainPosition.x) / TerrainSize.x, (Position[index].z - TerrainPosition.z) / TerrainSize.z);
            if (interpolatedPosition.x < 0 || interpolatedPosition.x > 1 || interpolatedPosition.y < 0 || interpolatedPosition.y > 1)
                return; // skip nodes outside of the terrain's area -- don't (re-)write to cell nodes not on this terrain when sharing cells with other terrains

            if (IsHole(new float2(interpolatedPosition.x, interpolatedPosition.y)))
                return; // skip nodes that are on terrain holes

            // else assign all default/calculated values to use for further spawning rules and matrix building
            Position[index] = new float3(Position[index].x, GetTriangleInterpolatedHeight(interpolatedPosition.x, interpolatedPosition.y) + TerrainPosition.y, Position[index].z);
            Rotation[index] = quaternion.Euler(0, 0, 0);    // def val
            Scale[index] = new float3(1, 1, 1);    // def val
            TerrainNormal[index] = GetInterpolatedNormal(interpolatedPosition.x, interpolatedPosition.y);
            ControlData[index] = new float2(ControlData[index].x, 1);   // "biomeMaskDistance" -- "include mask def value" -- later shared controlData for various things
            TerrainSourceID[index] = TerrainSourceIDThis;
            Included[index] = 1;    // do spawn (by default nodes don't spawn otherwise)
        }

        float GetTriangleInterpolatedHeight(float _x, float _y)
        {
            float fx = _x * (HeightmapResolution - 1);
            float fy = _y * (HeightmapResolution - 1);
            int lx = (int)fx;
            int ly = (int)fy;

            float u = fx - lx;
            float v = fy - ly;
            if (u > v)
            {
                float z00 = GetHeight(lx + 0, ly + 0);
                float z01 = GetHeight(lx + 1, ly + 0);
                float z11 = GetHeight(lx + 1, ly + 1);
                return z00 + (z01 - z00) * u + (z11 - z01) * v;
            }
            else
            {
                float z00 = GetHeight(lx + 0, ly + 0);
                float z10 = GetHeight(lx + 0, ly + 1);
                float z11 = GetHeight(lx + 1, ly + 1);
                return z00 + (z11 - z10) * u + (z10 - z00) * v;
            }
        }

        float GetHeight(int _x, int _y)
        {
            _x = math.clamp(_x, 0, HeightmapResolution - 1);
            _y = math.clamp(_y, 0, HeightmapResolution - 1);
            return InputHeights[_x + (_y * HeightmapResolution)] * HeightmapScale.y;
        }

        bool IsHole(float2 _position)
        {
            int x = math.clamp((int)(_position.x * HolesResolution), 0, HolesResolution - 1);
            int y = math.clamp((int)(_position.y * HolesResolution), 0, HolesResolution - 1);
            return !InputHoles[y * HolesResolution + x];
        }

        public float3 GetInterpolatedNormal(float _x, float _y)
        {
            float fx = _x * (HeightmapResolution - 1);
            float fy = _y * (HeightmapResolution - 1);
            int lx = (int)fx;
            int ly = (int)fy;

            float3 n00 = CalculateNormalSobel(lx + 0, ly + 0);
            float3 n10 = CalculateNormalSobel(lx + 1, ly + 0);
            float3 n01 = CalculateNormalSobel(lx + 0, ly + 1);
            float3 n11 = CalculateNormalSobel(lx + 1, ly + 1);

            float u = fx - lx;
            float3 s = math.lerp(n00, n10, u);
            float3 t = math.lerp(n01, n11, u);
            float v = fy - ly;

            return math.normalize(math.lerp(s, t, v));
        }

        float3 CalculateNormalSobel(int _x, int _y)
        {
            float dX = GetHeight(_x - 1, _y - 1) * -1.0f;
            dX += GetHeight(_x - 1, _y) * -2.0f;
            dX += GetHeight(_x - 1, _y + 1) * -1.0f;
            dX += GetHeight(_x + 1, _y - 1) * 1.0f;
            dX += GetHeight(_x + 1, _y) * 2.0f;
            dX += GetHeight(_x + 1, _y + 1) * 1.0f;
            dX /= HeightmapScale.x;

            float dY = GetHeight(_x - 1, _y - 1) * -1.0f;
            dY += GetHeight(_x, _y - 1) * -2.0f;
            dY += GetHeight(_x + 1, _y - 1) * -1.0f;
            dY += GetHeight(_x - 1, _y + 1) * 1.0f;
            dY += GetHeight(_x, _y + 1) * 2.0f;
            dY += GetHeight(_x + 1, _y + 1) * 1.0f;
            dY /= HeightmapScale.z;

            return math.normalize(new float3(-dX, 8, -dY));
        }
    }

    [BurstCompile]
    public struct UnityTerrainConcaveIncludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;

        [ReadOnly] public NativeArray<float> InputHeights;
        [ReadOnly] public int HeightmapResolution;
        [ReadOnly] public float3 HeightmapScale;
        [ReadOnly] public float2 TerrainPositionXZ;

        [ReadOnly] public int DistancePerSampleX;
        [ReadOnly] public int DistancePerSampleZ;
        [ReadOnly] public float MinHeightDifference;
        [ReadOnly] public bool Inverse;

        public void Execute(int index)  // get center point > compare against average square around it > higher/lower diff = cavity value
        {
            if (Included[index] == 0)
                return;

            int2 xz = new((int)math.round((Position[index].x - TerrainPositionXZ.x) / HeightmapScale.x), (int)math.round((Position[index].z - TerrainPositionXZ.y) / HeightmapScale.z));    // xz of the center point (be aware of item Z vs terrain Y)

            // calculate the average height of surrounding pixels -- w/ max offset distance for the "average square" => total "heightmapData" "coverage" per sample point
            float heightLU = GetHeight(xz.x - DistancePerSampleX, xz.y + DistancePerSampleZ);   // up
            float heightCU = GetHeight(xz.x, xz.y + DistancePerSampleZ);
            float heightRU = GetHeight(xz.x + DistancePerSampleX, xz.y + DistancePerSampleZ);
            float heightLC = GetHeight(xz.x - DistancePerSampleX, xz.y);    // center
            float heightCC = GetHeight(xz.x, xz.y); // center (sample) point => gets compared against surrounding "average square" using eight sample points around the center point
            float heightRC = GetHeight(xz.x + DistancePerSampleX, xz.y);
            float heightLD = GetHeight(xz.x - DistancePerSampleX, xz.y - DistancePerSampleZ);   // down
            float heightCD = GetHeight(xz.x, xz.y - DistancePerSampleZ);
            float heightRD = GetHeight(xz.x + DistancePerSampleX, xz.y - DistancePerSampleZ);
            float heightAverage = (heightLU + heightCU + heightRU + heightLC + heightRC + heightLD + heightCD + heightRD) / 8;  // "average square"

            bool shouldExclude = heightAverage < (heightCC + MinHeightDifference);  // true if "convex" => exclude as points around the center are lower
            if (Inverse) shouldExclude = !shouldExclude;    // treat base rule as "convex"
            if (shouldExclude == false) return; // points around the center aren't lower => "concave" so don't exclude
            Included[index] = 0;
        }

        float GetHeight(int _x, int _y)
        {
            _x = math.clamp(_x, 0, HeightmapResolution - 1);
            _y = math.clamp(_y, 0, HeightmapResolution - 1);
            return InputHeights[_x + (_y * HeightmapResolution)] * HeightmapScale.y;
        }
    }

    [BurstCompile]
    public struct SplatmapIncludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;

        [ReadOnly] public NativeArray<ARGBBytes> SplatmapChunkArray;
        [ReadOnly] public int SplatmapTextureIndex;
        [ReadOnly] public int AlphamapResolution;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public float3 TerrainPosition;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            int x = (int)((Position[index].x - TerrainPosition.x) / TexelSize.x);   // cast-flooring used to conform with splatmapData "offset"
            int z = (int)((Position[index].z - TerrainPosition.z) / TexelSize.y);   // cast-flooring used to conform with splatmapData "offset"

            float2 controlData = ControlData[index];

            if (x < 0 || x >= AlphamapResolution || z < 0 || z >= AlphamapResolution)   // out of terrain / splatmap range
            {   // safety logic flow check to not skip further iterations of this job when multiple terrains share the same vegetation cells
                controlData.y = 0;  // flag as "included" for the late stage "Evaluation" => reset state in late stage "Evaluation" for re-processing of another terrain
                ControlData[index] = controlData;
                return;
            }

            int brightness = math.select(math.select(math.select(math.select(0,
                SplatmapChunkArray[x + (z * AlphamapResolution)].A, SplatmapTextureIndex == 3),
                SplatmapChunkArray[x + (z * AlphamapResolution)].B, SplatmapTextureIndex == 2),
                SplatmapChunkArray[x + (z * AlphamapResolution)].G, SplatmapTextureIndex == 1),
                SplatmapChunkArray[x + (z * AlphamapResolution)].R, SplatmapTextureIndex == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (brightness >= MinBrightness && brightness <= MaxBrightness) // if point is on the terrain texture then "flag" as affected => (if point is not on the terrain texture then exclude)
                controlData.y = 0;  // flag as on the texture to not exclude (which happens by default otherwise)

            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct SplatmapExcludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;

        [ReadOnly] public NativeArray<ARGBBytes> SplatmapChunkArray;
        [ReadOnly] public int SplatmapTextureIndex;
        [ReadOnly] public int AlphamapResolution;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public float3 TerrainPosition;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            int x = (int)((Position[index].x - TerrainPosition.x) / TexelSize.x);   // cast-flooring used to conform with splatmapData "offset"
            int z = (int)((Position[index].z - TerrainPosition.z) / TexelSize.y);   // cast-flooring used to conform with splatmapData "offset"

            if (x < 0 || x >= AlphamapResolution || z < 0 || z >= AlphamapResolution)
                return;

            int brightness = math.select(math.select(math.select(math.select(0,
                SplatmapChunkArray[x + (z * AlphamapResolution)].A, SplatmapTextureIndex == 3),
                SplatmapChunkArray[x + (z * AlphamapResolution)].B, SplatmapTextureIndex == 2),
                SplatmapChunkArray[x + (z * AlphamapResolution)].G, SplatmapTextureIndex == 1),
                SplatmapChunkArray[x + (z * AlphamapResolution)].R, SplatmapTextureIndex == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (brightness >= MinBrightness && brightness <= MaxBrightness) // if point is on the terrain texture then exclude
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct SplatmapDensityJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;

        [ReadOnly] public float DensityMultiplier;
        [ReadOnly] public float MinDensity;
        [ReadOnly] public float MaxDensity;
        [ReadOnly] public float BrightnessThreshold;
        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;

        [ReadOnly] public NativeArray<ARGBBytes> SplatmapChunkArray;
        [ReadOnly] public int SplatmapTextureIndex;
        [ReadOnly] public int AlphamapResolution;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public float3 TerrainPosition;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0)
                return;

            int x = (int)((Position[index].x - TerrainPosition.x) / TexelSize.x);   // cast-flooring used to conform with splatmapData "offset"
            int z = (int)((Position[index].z - TerrainPosition.z) / TexelSize.y);   // cast-flooring used to conform with splatmapData "offset"

            if (x < 0 || x >= AlphamapResolution || z < 0 || z >= AlphamapResolution)
                return;

            int brightness = math.select(math.select(math.select(math.select(0,
                SplatmapChunkArray[x + (z * AlphamapResolution)].A, SplatmapTextureIndex == 3),
                SplatmapChunkArray[x + (z * AlphamapResolution)].B, SplatmapTextureIndex == 2),
                SplatmapChunkArray[x + (z * AlphamapResolution)].G, SplatmapTextureIndex == 1),
                SplatmapChunkArray[x + (z * AlphamapResolution)].R, SplatmapTextureIndex == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (!(brightness >= MinBrightness && brightness <= MaxBrightness))  // if point is not on the terrain texture then don't affect density
                return;

            float2 controlData = ControlData[index];
            controlData.y *= math.clamp((brightness / BrightnessThreshold) * DensityMultiplier, MinDensity, MaxDensity);
            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct SplatmapScaleJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public float ScaleMultiplier;
        [ReadOnly] public float MinScale;
        [ReadOnly] public float MaxScale;
        [ReadOnly] public float BrightnessThreshold;
        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;

        [ReadOnly] public NativeArray<ARGBBytes> SplatmapChunkArray;
        [ReadOnly] public int SplatmapTextureIndex;
        [ReadOnly] public int AlphamapResolution;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public float3 TerrainPosition;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            int x = (int)((Position[index].x - TerrainPosition.x) / TexelSize.x);   // cast-flooring used to conform with splatmapData "offset"
            int z = (int)((Position[index].z - TerrainPosition.z) / TexelSize.y);   // cast-flooring used to conform with splatmapData "offset"

            if (x < 0 || x >= AlphamapResolution || z < 0 || z >= AlphamapResolution)
                return;

            int brightness = math.select(math.select(math.select(math.select(0,
                SplatmapChunkArray[x + (z * AlphamapResolution)].A, SplatmapTextureIndex == 3),
                SplatmapChunkArray[x + (z * AlphamapResolution)].B, SplatmapTextureIndex == 2),
                SplatmapChunkArray[x + (z * AlphamapResolution)].G, SplatmapTextureIndex == 1),
                SplatmapChunkArray[x + (z * AlphamapResolution)].R, SplatmapTextureIndex == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (!(brightness >= MinBrightness && brightness <= MaxBrightness))  // if point is not on the terrain texture then don't affect scale
                return;

            Scale[index] *= math.clamp((brightness / BrightnessThreshold) * ScaleMultiplier, MinScale, MaxScale);
        }
    }
    #endregion
}