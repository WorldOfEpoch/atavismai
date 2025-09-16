using AwesomeTechnologies.Utility.BVHTree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using Unity.Burst;
using AwesomeTechnologies.Shaders;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    public enum TraverseState
    {
        FromParent,
        FromSibling,
        FromChild
    }

    public struct BVHRay
    {
        public float3 Origin;
        public float3 Direction;
        public int DoRaycast;
    }

    [System.Serializable]
    public struct MeshTerrainMeshSource
    {
        public MeshRenderer MeshRenderer;
        public TerrainSourceID TerrainSourceID;
        public MaterialPropertyBlock MaterialPropertyBlock;
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Terrains/MeshTerrain", 1)]
    [ScriptExecutionOrder(-99)]
    [ExecuteInEditMode]
    public class MeshTerrain : MonoBehaviour, IVegetationStudioTerrain
    {
        public int CurrentTabIndex;
        public bool MultiLevelRaycast;
        public bool AutoAddToVegegetationSystem;
        public bool Filterlods;
#if UNITY_EDITOR
        public bool ShowDebugInfo;
        private Material debugMaterial;
#endif

        public MeshTerrainData MeshTerrainData;
        public List<MeshTerrainMeshSource> MeshTerrainMeshSourceList = new();
        public bool NeedGeneration;
        private bool initDone;

        private List<ObjectData> objects;
        private List<BVHNode> nodes;
        private List<BVHTriangle> finalPrims;
        private List<BVHTriangle> tris;
        private NativeArray<LBVHNODE> nativeNodes;
        private NativeArray<LBVHTriangle> nativePrims;

        private NativeArray<BVHRay> rays;
        private NativeArray<HitInfo> tempHits;
        private NativeArray<HitInfo> raycastHits;

        public string TerrainType => "Mesh terrain";    // name to display in the terrain list / inspector UI

        public Bounds TerrainBounds { get { return MeshTerrainData ? MeshTerrainData.Bounds : new(); } }    // bounds used for ex: overlap queries, vegetation cell Y-Axis sampling, cell cache clearing

        public void Init()
        {
            CreateNativeArrays();
#if UNITY_EDITOR
            debugMaterial = new(ShaderUtility.GetShader_UtilityAlbedoColor());
#endif
            initDone = true;
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
            Init();

            if (AutoAddToVegegetationSystem)
                VegetationStudioManager.AddTerrain(gameObject, false, null);
            else
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);
        }

#if UNITY_EDITOR
        void Update()
        {
            if (ShowDebugInfo)
                DrawDebuginfo();
        }
#endif

        void OnDisable()
        {
            initDone = false;

            if (AutoAddToVegegetationSystem)
                VegetationStudioManager.RemoveTerrain(gameObject);
            else
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);

            DisposeNativeArrays();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(TerrainBounds.center, TerrainBounds.size);
        }

        private void CreateNativeArrays()
        {
            DisposeNativeArrays();
            if (MeshTerrainData == null) return;
            nativeNodes = new NativeArray<LBVHNODE>(MeshTerrainData.lNodes.ToArray(), Allocator.Persistent);    // persistent as used by several jobs across several (sub-)systems
            nativePrims = new NativeArray<LBVHTriangle>(MeshTerrainData.lPrims.ToArray(), Allocator.Persistent);    // persistent as used by several jobs across several (sub-)systems
        }

        private void DisposeNativeArrays()
        {
            if (nativeNodes.IsCreated) nativeNodes.Dispose();
            if (nativePrims.IsCreated) nativePrims.Dispose();
        }

        public void GenerateMeshTerrain(bool _refreshTerrainArea = true)
        {
            objects = new List<ObjectData>();
            for (int i = 0; i < MeshTerrainMeshSourceList.Count; i++)
            {
                if (MeshTerrainMeshSourceList[i].MeshRenderer.GetComponent<MeshFilter>().sharedMesh == null)
                    continue;   // skip since missing mesh

                objects.Add(new(MeshTerrainMeshSourceList[i].MeshRenderer, (int)MeshTerrainMeshSourceList[i].TerrainSourceID)); // add new object (terrain mesh source)
            }

            BVH.Build(ref objects, out nodes, out tris, out finalPrims);    // build the BVH
            BVH.BuildLbvhData(nodes, finalPrims, out MeshTerrainData.lNodes, out MeshTerrainData.lPrims);   // assign into valid containers for jobs => "nativeArray" later

            Bounds oldBounds = MeshTerrainData.Bounds;
            MeshTerrainData.Bounds = CalculateTerrainBounds();  // enlarge bounds based on the added "mesh terrain sources"

            CreateNativeArrays();   // assign BVH data into jobs friendly "nativeArrays"
            NeedGeneration = false; // disable UI warning box
            if (_refreshTerrainArea)
            {
                VegetationStudioManager.RefreshTerrainArea(oldBounds);  // clear old bounds in case of re-generation
                VegetationStudioManager.RefreshTerrainArea(TerrainBounds);  // update / prepare new area
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(MeshTerrainData);
#endif
        }

        private Bounds CalculateTerrainBounds() // simple enlarging/encapsulating based on all meshes added
        {
            Bounds terrainBounds = new();
            for (int i = 0; i < MeshTerrainMeshSourceList.Count; i++)
                if (i == 0)
                {
                    if (MeshTerrainMeshSourceList[i].MeshRenderer)
                        terrainBounds = MeshTerrainMeshSourceList[i].MeshRenderer.bounds;
                }
                else
                {
                    if (MeshTerrainMeshSourceList[i].MeshRenderer)
                        terrainBounds.Encapsulate(MeshTerrainMeshSourceList[i].MeshRenderer.bounds);
                }
            return terrainBounds;
        }

        public void AddMeshRenderer(GameObject _go, TerrainSourceID _terrainSourceID)   // add new meshes as terrain sources -- filter out LODs when enabled
        {
            MeshRenderer[] meshRenderers = _go.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (Filterlods)
                {
                    if (meshRenderers[i].name.ToUpper().Contains("LOD1")) continue;
                    if (meshRenderers[i].name.ToUpper().Contains("LOD2")) continue;
                    if (meshRenderers[i].name.ToUpper().Contains("LOD3")) continue;
                }

                MeshTerrainMeshSource newMeshTerrainTerrainSource = new()   // create a new terrain source with the given meshRenderers, terrainSourceID => create a new MPB for "Debug" purposes
                {
                    MeshRenderer = meshRenderers[i],
                    TerrainSourceID = _terrainSourceID,
                    MaterialPropertyBlock = new MaterialPropertyBlock()
                };

                MeshTerrainMeshSourceList.Add(newMeshTerrainTerrainSource); // assign to the list for iterations
            }

            NeedGeneration = true;  // flag for generation since entirely new data
        }

#if UNITY_EDITOR
        private void DrawDebuginfo()    // re-draw the added meshes for debugging when enabled -- draw them with a solid color based on their "terrainSourceID"
        {
            for (int i = 0; i < MeshTerrainMeshSourceList.Count; i++)
            {
                if (MeshTerrainMeshSourceList[i].MaterialPropertyBlock == null) // edge case, shouldn't happen
                {
                    MeshTerrainMeshSource meshTerrainMeshSource = MeshTerrainMeshSourceList[i];
                    meshTerrainMeshSource.MaterialPropertyBlock = new MaterialPropertyBlock();
                    MeshTerrainMeshSourceList[i] = meshTerrainMeshSource;
                }

                DrawMeshRenderer(MeshTerrainMeshSourceList[i].MeshRenderer, MeshTerrainMeshSourceList[i].MaterialPropertyBlock, MeshTerrainMeshSourceList[i].TerrainSourceID);  // execute debug draw
            }
        }

        private void DrawMeshRenderer(MeshRenderer _meshRenderer, MaterialPropertyBlock _materialPropertyBlock, TerrainSourceID _terrainSourceID)   // re-draw the added meshes w/ new color -- used for "Debug" purposes
        {
            if (_meshRenderer == null || debugMaterial == null)
                return;

            MeshFilter meshFilter = _meshRenderer.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                return;

            _materialPropertyBlock.SetColor("_Color", GetMeshTerrainSourceTypeColor(_terrainSourceID));

            for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
                Graphics.DrawMesh(meshFilter.sharedMesh, Matrix4x4.TRS(_meshRenderer.transform.position, _meshRenderer.transform.rotation, _meshRenderer.transform.lossyScale), debugMaterial, 0, null, i, _materialPropertyBlock);
        }

        private Color GetMeshTerrainSourceTypeColor(TerrainSourceID _terrainSourceID)   // simple color return base on a given "terrainSourceID" -- used for "Debug" purposes
        {
            return _terrainSourceID switch
            {
                TerrainSourceID.TerrainSourceID1 => Color.green,
                TerrainSourceID.TerrainSourceID2 => Color.red,
                TerrainSourceID.TerrainSourceID3 => Color.blue,
                TerrainSourceID.TerrainSourceID4 => Color.yellow,
                TerrainSourceID.TerrainSourceID5 => Color.grey,
                TerrainSourceID.TerrainSourceID6 => Color.magenta,
                TerrainSourceID.TerrainSourceID7 => Color.cyan,
                TerrainSourceID.TerrainSourceID8 => Color.white,
                _ => Color.green,
            };
        }
#endif

        public JobHandle SampleCellHeight(NativeArray<Bounds> _vegetationCellBoundsList, float _worldspaceHeightCutoff, Rect _updateRect, JobHandle _dependsOn = default)
        {
            if (initDone == false)
                return _dependsOn;

            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (_updateRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            if (nativeNodes.IsCreated == false)
                CreateNativeArrays();

            BVHTerrainCellSampleJob bvhTerrainCellSampleJob = new()
            {
                VegetationCellBoundsList = _vegetationCellBoundsList,
                Nodes = nativeNodes,
                TerrainRect = terrainRect
            };
            return bvhTerrainCellSampleJob.ScheduleParallel(_vegetationCellBoundsList.Length, 64, _dependsOn);
        }

        public JobHandle SampleTerrain(VegetationInstanceData _instanceData, int _sampleCount, Rect _cellRect, JobHandle _dependsOn)
        {
            if (initDone == false)
                return _dependsOn;

            Rect terrainRect = RectExtension.CreateRectFromBounds(TerrainBounds);
            if (_cellRect.Overlaps(terrainRect) == false)
                return _dependsOn;

            if (nativeNodes.IsCreated == false)
                CreateNativeArrays();

            rays = new NativeArray<BVHRay>(_sampleCount, Allocator.TempJob);
            tempHits = new NativeArray<HitInfo>(_sampleCount, Allocator.TempJob);
            raycastHits = new NativeArray<HitInfo>(_sampleCount, Allocator.TempJob);

            CreateBVHRaycastJob createBVHRaysJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,
                TerrainRect = terrainRect,
                Rays = rays
            };
            _dependsOn = createBVHRaysJob.ScheduleParallel(_sampleCount, 64, _dependsOn);

            if (MultiLevelRaycast)
            {
                SampleBVHTreeMultiLevelJob sampleBVHTreeMultiLevelJob = new()
                {
                    Rays = rays,
                    TempHits = tempHits,
                    RaycastHits = raycastHits,
                    NativeNodes = nativeNodes,
                    NativePrims = nativePrims
                };
                _dependsOn = sampleBVHTreeMultiLevelJob.ScheduleParallel(_sampleCount, 64, _dependsOn);
            }
            else
            {
                SampleBVHTreeJob sampleBVHTreeJob = new()
                {
                    Rays = rays,
                    TempHits = tempHits,
                    RaycastHits = raycastHits,
                    NativeNodes = nativeNodes,
                    NativePrims = nativePrims
                };
                _dependsOn = sampleBVHTreeJob.ScheduleParallel(_sampleCount, 64, _dependsOn);
            }

            UpdateBVHInstanceListJob updateInstanceListJob = new()
            {
                Position = _instanceData.position,
                Rotation = _instanceData.rotation,
                Scale = _instanceData.scale,
                TerrainNormal = _instanceData.terrainNormal,
                RandomNumberIndex = _instanceData.randomNumberIndex,
                ControlData = _instanceData.controlData,
                TerrainSourceID = _instanceData.terrainSourceID,
                Included = _instanceData.included,

                raycastHits = raycastHits
            };
            return updateInstanceListJob.Schedule(_sampleCount, _dependsOn);
        }

        public JobHandle SampleConcaveLocation(VegetationInstanceData _instanceData, float _minHeightDifference, float _distancePerSample, bool _inverse, Rect _cellRect, JobHandle _dependsOn)
        {
            if (initDone == false)
                return _dependsOn;
            // TODO implement concave sampling for mesh terrain
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

        public virtual void CompleteSplatmapGeneration()
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

    #region jobs
    [BurstCompile]
    public struct BVHTerrainCellSampleJob : IJobFor
    {
        public NativeArray<Bounds> VegetationCellBoundsList;
        [ReadOnly] public NativeArray<LBVHNODE> Nodes;
        [ReadOnly] public Rect TerrainRect;

        public void Execute(int index)
        {
            if (TerrainRect.Overlaps(RectExtension.CreateRectFromBounds(VegetationCellBoundsList[index])) == false)
                return; // skip when this terrain's (custom) bounds don't overlap with the given cell's bounds

            // store/create temporary (extreme) data to compare against/with
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            CalculateCellSize(0, ref minHeight, ref maxHeight); // get highest/lowest point of this terrain

            if (float.IsNegativeInfinity(minHeight) || float.IsPositiveInfinity(maxHeight)) // safety check -- whether mesh generation had some issue
                return; // skip and keep "disabled"

            bool enabled = VegetationCellBoundsList[index].center.y > -99999;   // whether the cell has been "enabled" (and sampled) already by another terrain

            Bounds t = new( // assign new Y-Axis bounds
                 new float3(VegetationCellBoundsList[index].center.x, (maxHeight + minHeight) * 0.5f, VegetationCellBoundsList[index].center.z),
                 new float3(VegetationCellBoundsList[index].size.x, maxHeight - minHeight, VegetationCellBoundsList[index].size.z));

            if (enabled)
            {   // compare against existing bounds -- reset back to potentially already bigger bounds -- else use new bigger bounds
                t.min = new float3(t.min.x, math.min(t.min.y, VegetationCellBoundsList[index].min.y), t.min.z);
                t.max = new float3(t.max.x, math.max(t.max.y, VegetationCellBoundsList[index].max.y), t.max.z);
            }

            VegetationCellBoundsList[index] = t;
        }

        public void CalculateCellSize(int nodeID, ref float minHeight, ref float maxHeight)
        {
            if (Nodes[nodeID].IsLeaf == 1)
            {
                if (Nodes[nodeID].BMin.y < minHeight)
                    minHeight = Nodes[nodeID].BMin.y;

                if (Nodes[nodeID].BMax.y > maxHeight)
                    maxHeight = Nodes[nodeID].BMax.y;
            }
            else
            {
                CalculateCellSize(Nodes[nodeID].LChildID, ref minHeight, ref maxHeight);
                CalculateCellSize(Nodes[nodeID].RChildID, ref minHeight, ref maxHeight);
            }
        }
    }

    [BurstCompile]
    public struct CreateBVHRaycastJob : IJobFor
    {
        [ReadOnly] public NativeList<float3> Position;
        [ReadOnly] public NativeList<float2> ControlData;
        [ReadOnly] public Rect TerrainRect;
        [WriteOnly] public NativeArray<BVHRay> Rays;

        public void Execute(int index)
        {
            BVHRay ray = new()  // fill with new rays to use for the BVH
            {
                Origin = Position[index] + new float3(0, 10000, 0), // scan based on the given position of the current "node" -- start at max height allowed for terrains in Unity
                Direction = Vector3.down,   // scan downwards
                DoRaycast = math.select(1, 0, TerrainRect.Contains(new float2(Position[index].x, Position[index].z)) == false || ControlData[index].y <= 0) // don't use ray when "node" isn't on this terrain -- when "spawnChance" is zero
            };
            Rays[index] = ray;
        }
    }

    [BurstCompile]
    public struct SampleBVHTreeJob : IJobFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<BVHRay> Rays;
        [DeallocateOnJobCompletion] public NativeArray<HitInfo> TempHits;
        [WriteOnly] public NativeArray<HitInfo> RaycastHits;
        [ReadOnly] public NativeArray<LBVHNODE> NativeNodes;
        [ReadOnly] public NativeArray<LBVHTriangle> NativePrims;

        public void Execute(int index)
        {
            if (Rays[index].DoRaycast == 0)
            {   // skip when "spawnChance" is zero -- when ray shouldn't be used / there has been no hit
                RaycastHits[index] = new HitInfo { HitDistance = -1 };  // set to "-1" to declare as not to be used
                return;
            }

            RayCastStackless(index);
        }

        bool RayCastStackless(int index)
        {
            LBVHNODE current = NativeNodes[0];

            // without using stack
            float3 rayDirection = Rays[index].Direction;

            // ----------------------------------------------------------------------------------------------------            
            float sA = rayDirection[current.SplitAxis];
            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);
            // ----------------------------------------------------------------------------------------------------

            int rootNearID = current.NearNodeID;
            int rootNodeID = current.NodeID;
            current = NativeNodes[rootNearID];

            TraverseState state = TraverseState.FromParent;

            bool intersect = false;
            float bestDist = float.MaxValue;

            while (current.NodeID != rootNodeID)
            {
                switch (state)
                {
                    case TraverseState.FromChild:
                        int cID = current.NodeID;

                        current = NativeNodes[current.ParentID];

                        // ----------------------------------------------------------------------------------------------------                        
                        sA = rayDirection[current.SplitAxis];

                        current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                        current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);

                        // ----------------------------------------------------------------------------------------------------

                        if (cID == current.NearNodeID)
                        {
                            current = NativeNodes[current.FarNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else
                        {
                            state = TraverseState.FromChild;
                        }
                        break;
                    case TraverseState.FromSibling:
                        if (current.IntersectRay(Rays[index]) == false)
                        {
                            current = NativeNodes[current.ParentID];
                            state = TraverseState.FromChild;
                        }
                        else if (current.IsLeaf == 1)
                        {
                            // no need to iterate as we can only have 1 triangle in a leaf node
                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHits, index))
                                if (TempHits[index].HitDistance < bestDist)
                                {
                                    bestDist = TempHits[index].HitDistance;
                                    RaycastHits[index] = TempHits[index];
                                    intersect = true;
                                }
                            current = NativeNodes[current.ParentID];
                            state = TraverseState.FromChild;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------
                            sA = rayDirection[current.SplitAxis];

                            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);

                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[current.NearNodeID];
                            state = TraverseState.FromParent;
                        }
                        break;
                    case TraverseState.FromParent:
                        if (current.IntersectRay(Rays[index]) == false)
                        {
                            cID = current.NodeID;
                            current = NativeNodes[current.ParentID];

                            // ----------------------------------------------------------------------------------------------------                
                            sA = rayDirection[current.SplitAxis];

                            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);

                            current = cID == current.NearNodeID ? NativeNodes[current.FarNodeID] : NativeNodes[current.NearNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else if (current.IsLeaf == 1)
                        {
                            // no need to iterate as we can only have 1 triangle in a leaf node
                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHits, index))
                                if (TempHits[index].HitDistance < bestDist)
                                {
                                    bestDist = TempHits[index].HitDistance;
                                    RaycastHits[index] = TempHits[index];
                                    intersect = true;
                                }

                            // ----------------------------------------------------------------------------------------------------

                            NativeNodes[current.ParentID].GetChildrenIDsAndSplitAxis(out int lChild, out int rChild, out int splitAxis);

                            sA = rayDirection[splitAxis];
                            int farNodeID = math.select(rChild, lChild, sA < 0f);
                            current = NativeNodes[farNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------        

                            sA = rayDirection[current.SplitAxis];

                            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);
                            current = NativeNodes[current.NearNodeID];
                            state = TraverseState.FromParent;
                        }
                        break;
                }
            }

            return intersect;
        }
    }

    [BurstCompile]
    public struct SampleBVHTreeMultiLevelJob : IJobFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<BVHRay> Rays;
        [DeallocateOnJobCompletion] public NativeArray<HitInfo> TempHits;
        [WriteOnly] public NativeArray<HitInfo> RaycastHits;
        [ReadOnly] public NativeArray<LBVHNODE> NativeNodes;
        [ReadOnly] public NativeArray<LBVHTriangle> NativePrims;

        public void Execute(int index)
        {
            if (Rays[index].DoRaycast == 0)
            {   // skip when "spawnChance" is zero -- when ray shouldn't be used / there has been no hit
                RaycastHits[index] = new HitInfo { HitDistance = -1 };  // set to "-1" to declare as not to be used
                return;
            }

            RayCastStackless(index);
        }

        bool RayCastStackless(int index)
        {
            LBVHNODE current = NativeNodes[0];
            float3 rayDirection = Rays[index].Direction;

            // ----------------------------------------------------------------------------------------------------            
            float sA = rayDirection[current.SplitAxis];
            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);
            // ----------------------------------------------------------------------------------------------------

            int rootNearID = current.NearNodeID;
            int rootNodeID = current.NodeID;
            current = NativeNodes[rootNearID];

            TraverseState state = TraverseState.FromParent;
            bool intersect = false;

            while (current.NodeID != rootNodeID)
            {
                switch (state)
                {
                    case TraverseState.FromChild:
                        int cID = current.NodeID;

                        current = NativeNodes[current.ParentID];

                        // ----------------------------------------------------------------------------------------------------                        
                        sA = rayDirection[current.SplitAxis];
                        current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                        current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);

                        // ----------------------------------------------------------------------------------------------------

                        if (cID == current.NearNodeID)
                        {
                            current = NativeNodes[current.FarNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else
                        {
                            state = TraverseState.FromChild;
                        }
                        break;
                    case TraverseState.FromSibling:
                        float dist;
                        if (!BVHBBox.IntersectRay(Rays[index], current.BMin, current.BMax, out dist))
                        {
                            current = NativeNodes[current.ParentID];
                            state = TraverseState.FromChild;
                        }
                        else if (current.IsLeaf == 1)
                        {
                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHits, index))
                            {
                                RaycastHits[index] = (TempHits[index]);
                                intersect = true;
                            }

                            current = NativeNodes[current.ParentID];
                            state = TraverseState.FromChild;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------
                            sA = rayDirection[current.SplitAxis];
                            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);
                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[current.NearNodeID];
                            state = TraverseState.FromParent;
                        }
                        break;
                    case TraverseState.FromParent:
                        if (!BVHBBox.IntersectRay(Rays[index], current.BMin, current.BMax, out dist))
                        {
                            cID = current.NodeID;
                            current = NativeNodes[current.ParentID];

                            // ----------------------------------------------------------------------------------------------------                        
                            sA = rayDirection[current.SplitAxis];
                            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);
                            // ----------------------------------------------------------------------------------------------------

                            if (cID == current.NearNodeID)
                            {
                                current = NativeNodes[current.FarNodeID];
                                state = TraverseState.FromSibling;
                            }
                            else
                            {
                                current = NativeNodes[current.NearNodeID];
                                state = TraverseState.FromSibling;
                            }
                        }
                        else if (current.IsLeaf == 1)
                        {
                            // test triangle for intersection
                            if (NativePrims[current.PrimitivesOffset].IntersectRay(Rays[index], ref TempHits, index))
                            {
                                RaycastHits[index] = (TempHits[index]);
                                intersect = true;
                            }

                            // ----------------------------------------------------------------------------------------------------

                            NativeNodes[current.ParentID].GetChildrenIDsAndSplitAxis(out int lChild, out int rChild, out int splitAxis);
                            sA = rayDirection[splitAxis];

                            //int nearNodeID = math.select(lChild, rChild, sA < 0f);
                            int farNodeID = math.select(rChild, lChild, sA < 0f);

                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[farNodeID];
                            state = TraverseState.FromSibling;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------------------------------                            
                            sA = rayDirection[current.SplitAxis];
                            current.NearNodeID = math.select(current.LChildID, current.RChildID, sA < 0f);
                            current.FarNodeID = math.select(current.RChildID, current.LChildID, sA < 0f);
                            // ----------------------------------------------------------------------------------------------------

                            current = NativeNodes[current.NearNodeID];
                            state = TraverseState.FromParent;
                        }
                        break;
                }
            }

            return intersect;
        }
    }

    [BurstCompile]
    public struct UpdateBVHInstanceListJob : IJobFor
    {
        [WriteOnly] public NativeList<float3> Position;
        [WriteOnly] public NativeList<quaternion> Rotation;
        [WriteOnly] public NativeList<float3> Scale;
        [WriteOnly] public NativeList<float3> TerrainNormal;
        public NativeList<int> RandomNumberIndex;
        public NativeList<float2> ControlData;
        [WriteOnly] public NativeList<byte> TerrainSourceID;
        [WriteOnly] public NativeList<byte> Included;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<HitInfo> raycastHits;

        public void Execute(int index)
        {
            if (raycastHits[index].HitDistance <= 0)
                return; // skip when "spawnChance" is zero -- when ray shouldn't be used / there has been no hit

            // else assign all default/calculated values to use for further spawning rules and matrix building
            Position.Add(raycastHits[index].HitPoint);
            Rotation.Add(quaternion.Euler(0, 0, 0));    // def val
            Scale.Add(new float3(1, 1, 1)); // def val
            TerrainNormal.Add(raycastHits[index].HitNormal);
            RandomNumberIndex.Add(RandomNumberIndex[index]);
            ControlData.Add(new float2(ControlData[index].x, 1));   // "biomeMaskDistance" -- "include mask def value" -- later shared controlData for various things
            TerrainSourceID.Add(raycastHits[index].TerrainSourceID);
            Included.Add(1);    // do spawn (by default nodes don't spawn otherwise)
        }
    }
    #endregion
}