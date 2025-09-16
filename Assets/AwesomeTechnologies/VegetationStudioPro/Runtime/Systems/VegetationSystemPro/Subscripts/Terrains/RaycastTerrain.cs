using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Terrains/RaycastTerrain", 2)]
    [ScriptExecutionOrder(-98)]
    [ExecuteInEditMode]
    public class RaycastTerrain : MonoBehaviour, IVegetationStudioTerrain
    {
        public Bounds RaycastTerrainBounds = new(float3.zero, new float3(200, 20, 200));    // default bounds are 2x as the inspector cuts them in half again
        public LayerMask RaycastLayerMask = 1;  // default to "Default" layer
        public TerrainSourceID TerrainSourceID;
        public bool AutoAddToVegegetationSystem;

        private bool initDone;
        private NativeArray<RaycastCommand> raycastCommands;
        private NativeArray<RaycastHit> raycastHits;

        public string TerrainType => "Raycast terrain"; // name to display in the terrain list / inspector UI

        public Bounds TerrainBounds => new(RaycastTerrainBounds.center + transform.position, RaycastTerrainBounds.size);    // bounds used for ex: overlap queries, vegetation cell Y-Axis sampling, cell cache clearing

        public void Init()
        {
            // only used for "UnityTerrain" and "Mesh Terrain"
        }

        public void RefreshTerrainData(bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            // only used for "UnityTerrain"
        }

        public void RefreshTerrainData(Bounds _bounds, bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            // only used for "UnityTerrain"
        }

        void OnEnable()
        {
            initDone = true;

            if (AutoAddToVegegetationSystem)
                VegetationStudioManager.AddTerrain(gameObject, false, null);
            else
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
        }

        void OnDisable()
        {
            if (AutoAddToVegegetationSystem)
                VegetationStudioManager.RemoveTerrain(gameObject);
            else
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);

            initDone = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(RaycastTerrainBounds.center + transform.position + VegetationStudioManager.GetFloatingOriginOffset(), RaycastTerrainBounds.size);
        }

        public JobHandle SampleCellHeight(NativeArray<Bounds> _vegetationCellBoundsList, float _worldspaceHeightCutoff, Rect _updateRect, JobHandle _dependsOn = default)
        {
            if (initDone == false)
                return _dependsOn;

            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (_updateRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            RaycastTerrainCellSampleJob raycastTerrainCellSampleJob = new()
            {
                VegetationCellBoundsList = _vegetationCellBoundsList,
                TerrainRect = terrainRect,
                TerrainMinHeight = TerrainBounds.min.y,
                TerrainMaxHeight = TerrainBounds.max.y
            };
            return raycastTerrainCellSampleJob.ScheduleParallel(_vegetationCellBoundsList.Length, 64, _dependsOn);
        }

        public JobHandle SampleTerrain(VegetationInstanceData _instanceData, int _sampleCount, Rect _cellRect, JobHandle _dependsOn)
        {
            if (initDone == false)
                return _dependsOn;

            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (_cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            float3 floatingOriginOffset = VegetationStudioManager.GetFloatingOriginOffset();
            raycastCommands = new NativeArray<RaycastCommand>(_sampleCount, Allocator.TempJob);
            raycastHits = new NativeArray<RaycastHit>(_sampleCount, Allocator.TempJob);

            CreateRaycastCommandsJob createRaycastCommandsJob = new()
            {
                Position = _instanceData.position,
                FloatingOriginOffset = floatingOriginOffset + new float3(0, 10000, 0),
                QueryParams = new QueryParameters { layerMask = RaycastLayerMask, hitTriggers = QueryTriggerInteraction.Ignore },
                RaycastCommands = raycastCommands
            };
            _dependsOn = createRaycastCommandsJob.ScheduleParallel(_sampleCount, 64, _dependsOn);
            _dependsOn = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 64, 1, _dependsOn); // maxHits = 1 as > 1 not supported by the cell sampling > culling

            UpdateRaycastInstanceListJob updateRaycastInstanceListJob = new()
            {
                Position = _instanceData.position,
                Rotation = _instanceData.rotation,
                Scale = _instanceData.scale,
                TerrainNormal = _instanceData.terrainNormal,
                RandomNumberIndex = _instanceData.randomNumberIndex,
                ControlData = _instanceData.controlData,
                TerrainSourceID = _instanceData.terrainSourceID,
                Included = _instanceData.included,

                raycastCommands = raycastCommands,
                raycastHits = raycastHits,

                terrainSourceID = (byte)TerrainSourceID,
                terrainRect = terrainRect,
                floatingOriginOffset = floatingOriginOffset
            };
            return updateRaycastInstanceListJob.Schedule(raycastHits.Length, _dependsOn);
        }

        public JobHandle SampleConcaveLocation(VegetationInstanceData _instanceData, float _minHeightDifference, float _distancePerSample, bool _inverse, Rect _cellRect, JobHandle _dependsOn)
        {
            if (initDone == false)
                return _dependsOn;
            // TODO implement concave sampling for raycast terrains
            return _dependsOn;
        }

        public void VerifySplatmapAccess()
        {
            // only used for "UnityTerrain"
        }

        public JobHandle ProcessSplatmapIncludeRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            return _dependsOn;  // only used for "UnityTerrain"
        }

        public JobHandle ProcessSplatmapExcludeRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            return _dependsOn;  // only used for "UnityTerrain"
        }

        public JobHandle ProcessSplatmapScaleRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            return _dependsOn;  // only used for "UnityTerrain"
        }

        public JobHandle ProcessSplatmapDensityRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn)
        {
            return _dependsOn;  // only used for "UnityTerrain"
        }

        public bool NeedsSplatmapUpdate(Bounds _updateBounds)
        {
            return false;   // only used for "UnityTerrain"
        }

        public void PrepareSplatmapGeneration(bool _clearExistingTextures, float _heightCurveWorldHeight, float _worldSpaceSeaLevel)
        {
            // only used for "UnityTerrain"
        }

        public void GenerateSplatmapBiome(BiomeType _biomeType, List<PolygonMaskBiome> _polygonBiomeMaskList, List<TerrainTextureSettings> _terrainTextureSettingsList, float _worldSpaceSeaLevel, bool _clearExistingTextures)
        {
            // only used for "UnityTerrain"
        }

        public void CompleteSplatmapGeneration()
        {
            // only used for "UnityTerrain"
        }

        public void AssignHeatmapMaterial()
        {
            // only used for "UnityTerrain"
        }

        public void UpdateTerrainMaterial(float _worldspaceSeaLevel, float _worldspaceMaxTerrainHeight, TerrainTextureSettings _terrainTextureSettings)
        {
            // only used for "UnityTerrain"
        }

        public void RestoreTerrainMaterial()
        {
            // only used for "UnityTerrain"
        }

        public Texture2D GetTerrainTexture(int _index)
        {
            return null;    // only used for "UnityTerrain"
        }

        public TerrainLayer[] GetTerrainLayers()
        {
            return new TerrainLayer[0]; // only used for "UnityTerrain"
        }

        public void SetTerrainLayers(TerrainLayer[] _terrainLayers)
        {
            // only used for "UnityTerrain"
        }
    }

    [BurstCompile]
    public struct RaycastTerrainCellSampleJob : IJobFor
    {
        public NativeArray<Bounds> VegetationCellBoundsList;
        [ReadOnly] public Rect TerrainRect;
        [ReadOnly] public float TerrainMinHeight;
        [ReadOnly] public float TerrainMaxHeight;

        public void Execute(int index)
        {
            if (TerrainRect.Overlaps(RectExtension.CreateRectFromBounds(VegetationCellBoundsList[index])) == false)
                return; // skip when this terrain's (custom) bounds don't overlap with the given cell's bounds

            bool enabled = VegetationCellBoundsList[index].center.y > -99999;   // whether the cell has been "enabled" (and sampled) already by another terrain

            Bounds t = new( // assign new Y-Axis bounds
                 new float3(VegetationCellBoundsList[index].center.x, (TerrainMaxHeight + TerrainMinHeight) * 0.5f, VegetationCellBoundsList[index].center.z),
                 new float3(VegetationCellBoundsList[index].size.x, TerrainMaxHeight - TerrainMinHeight, VegetationCellBoundsList[index].size.z));

            if (enabled)
            {   // compare against existing bounds -- reset back to potentially already bigger bounds -- else use new bigger bounds
                t.min = new float3(t.min.x, math.min(t.min.y, VegetationCellBoundsList[index].min.y), t.min.z);
                t.max = new float3(t.max.x, math.max(t.max.y, VegetationCellBoundsList[index].max.y), t.max.z);
            }

            VegetationCellBoundsList[index] = t;
        }
    }

    [BurstCompile]
    public struct CreateRaycastCommandsJob : IJobFor
    {
        [ReadOnly] public NativeList<float3> Position;
        [ReadOnly] public float3 FloatingOriginOffset;
        [ReadOnly] public QueryParameters QueryParams;
        [WriteOnly] public NativeArray<RaycastCommand> RaycastCommands;

        public void Execute(int index)
        {
            RaycastCommand raycastCommand = new()   // fill with new raycastCommands to use for the multi threaded ray-casting
            {
                from = Position[index] + FloatingOriginOffset,  // scan based on the given position of the current "node" -- start at max height allowed for terrains in Unity
                distance = 13337,   // set matching range for the start height
                direction = Vector3.down,   // scan downwards
                queryParameters = QueryParams   // only scan needed stuff
            };
            RaycastCommands[index] = raycastCommand;
        }
    }

    [BurstCompile]
    public struct UpdateRaycastInstanceListJob : IJobFor
    {
        [WriteOnly] public NativeList<float3> Position;
        [WriteOnly] public NativeList<quaternion> Rotation;
        [WriteOnly] public NativeList<float3> Scale;
        [WriteOnly] public NativeList<float3> TerrainNormal;
        public NativeList<int> RandomNumberIndex;
        public NativeList<float2> ControlData;
        [WriteOnly] public NativeList<byte> TerrainSourceID;
        [WriteOnly] public NativeList<byte> Included;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<RaycastCommand> raycastCommands;  // only here to dispose it
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<RaycastHit> raycastHits;
        [ReadOnly] public byte terrainSourceID;
        [ReadOnly] public Rect terrainRect;
        [ReadOnly] public float3 floatingOriginOffset;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0 || raycastHits[index].distance <= 0)
                return; // skip when "spawnChance" is zero -- when ray shouldn't be used / there has been no hit

            float3 position = raycastHits[index].point;
            position -= floatingOriginOffset;   // apply floatingOriginOffset

            if (terrainRect.Contains(new float2(position.x, position.z)) == false)
                return; // skip when "node" isn't on this terrain

            // else assign all default/calculated values to use for further spawning rules and matrix building
            Position.Add(position);
            Rotation.Add(quaternion.Euler(0, 0, 0));    // def val
            Scale.Add(new float3(1, 1, 1)); // def val
            TerrainNormal.Add(raycastHits[index].normal);
            RandomNumberIndex.Add(RandomNumberIndex[index]);
            ControlData.Add(new float2(ControlData[index].x, 1));   // "biomeMaskDistance" -- "include mask def value" -- later shared controlData for various things
            TerrainSourceID.Add(terrainSourceID);
            Included.Add(1);    // do spawn (by default nodes don't spawn otherwise)
        }
    }
}