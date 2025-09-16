using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public struct ItemSelectorInstanceInfo
    {
        public int VegetationCellIndex;
        public int VegetationCellItemIndex;
        public float3 Position;
        public quaternion Rotation;
        public float3 Scale;
        public int Visible;
        public int LastVisible;
        public int Remove;
    }

    public class VegetationItemSelector
    {
        private readonly VegetationSystemPro vegetationSystemPro;

        private readonly VisibleVegetationCellSelector visibleVegetationCellSelector;
        [NonSerialized] public readonly List<VegetationCell> readyToLoadVegetationCellList = new();
        [NonSerialized] public readonly List<VegetationCell> readyToUnloadVegetationCellList = new();
        [NonSerialized] public readonly List<VegetationCell> loadedVegetationCellList = new();
        public readonly NativeList<ItemSelectorInstanceInfo> instanceList;

        private readonly NativeList<int> visibilityChangedIndexList;
        private readonly NativeList<int> removeVegetationCellIndexList;
        public readonly string vegetationItemID;
        private readonly VegetationItemIndices vegetationItemIndices;

        public float cullingDistance = 50f; // based on grass distance -- gets dynamically set later

        // runtime prefab spawner
        private readonly bool useSpawnChance;
        private readonly float spawnChance;
        private readonly int spawnSeed;

        public delegate void MultiOnVegetationItemVisibilityChangeDelegate(ItemSelectorInstanceInfo _itemSelectorInstanceInfo, VegetationItemIndices _vegetationItemIndices, string _vegetationItemID);
        public MultiOnVegetationItemVisibilityChangeDelegate OnVegetationItemVisibleDelegate;
        public MultiOnVegetationItemVisibilityChangeDelegate OnVegetationItemInvisibleDelegate;

        public delegate void MultiOnVegetationCellVisibilityChangeDelegate(int _vegetationCellIndex);
        public MultiOnVegetationCellVisibilityChangeDelegate OnVegetationCellInvisibleDelegate;

        public VegetationItemSelector(VisibleVegetationCellSelector _visibleVegetationCellSelector, VegetationSystemPro _vegetationSystemPro, VegetationItemInfoPro _vegetationItemInfoPro, bool _useSpawnChance, float _spawnChance, int _spawnSeed)
        {
            vegetationSystemPro = _vegetationSystemPro;
            vegetationSystemPro.OnVegetationCellLoaded += OnVegetationCellLoaded;

            visibleVegetationCellSelector = _visibleVegetationCellSelector;
            visibleVegetationCellSelector.OnVegetationCellVisibleDelegate += OnVegetationCellVisible;
            visibleVegetationCellSelector.OnVegetationCellInvisibleDelegate += OnVegetationCellInvisible;

            vegetationItemID = _vegetationItemInfoPro.VegetationItemID;
            vegetationItemIndices = _vegetationSystemPro.GetVegetationItemIndices(vegetationItemID);

            // runtime prefab spawner
            useSpawnChance = _useSpawnChance;
            spawnChance = _spawnChance;
            spawnSeed = _spawnSeed;

            visibilityChangedIndexList = new NativeList<int>(512, Allocator.Persistent);    // persistent as perma needed -- ~512 instance count preparation
            removeVegetationCellIndexList = new NativeList<int>(64, Allocator.Persistent);  // persistent as perma needed -- ~512 cell count preparation
            instanceList = new NativeList<ItemSelectorInstanceInfo>(512, Allocator.Persistent); // persistent as perma needed -- ~512 instance count preparation
        }

        public void OnVegetationCellLoaded(VegetationCell _vegetationCell, bool _preloaded)
        {
            if (_preloaded) return;

            if (loadedVegetationCellList.Contains(_vegetationCell) == false)
                return;

            if (readyToUnloadVegetationCellList.Contains(_vegetationCell) == false)
                readyToUnloadVegetationCellList.Add(_vegetationCell);

            if (readyToLoadVegetationCellList.Contains(_vegetationCell) == false)
                readyToLoadVegetationCellList.Add(_vegetationCell);
        }

        public void OnVegetationCellVisible(VegetationCell _vegetationCell)
        {
            readyToLoadVegetationCellList.Add(_vegetationCell);
        }

        public void OnVegetationCellInvisible(VegetationCell _vegetationCell)
        {
            readyToUnloadVegetationCellList.Add(_vegetationCell);
        }

        public void RefreshVegetationCell(VegetationCell _vegetationCell)
        {
            if (loadedVegetationCellList.Contains(_vegetationCell) == false)
                return;

            if (readyToUnloadVegetationCellList.Contains(_vegetationCell) == false)
                readyToUnloadVegetationCellList.Add(_vegetationCell);

            if (readyToLoadVegetationCellList.Contains(_vegetationCell) == false)
                readyToLoadVegetationCellList.Add(_vegetationCell);
        }

        public void RefreshAllVegetationCells()
        {
            for (int i = 0; i < loadedVegetationCellList.Count; i++)
            {
                if (readyToUnloadVegetationCellList.Contains(loadedVegetationCellList[i]) == false)
                    readyToUnloadVegetationCellList.Add(loadedVegetationCellList[i]);

                if (readyToLoadVegetationCellList.Contains(loadedVegetationCellList[i]) == false)
                    readyToLoadVegetationCellList.Add(loadedVegetationCellList[i]);
            }
        }

        public void Dispose()
        {
            vegetationSystemPro.OnVegetationCellLoaded -= OnVegetationCellLoaded;
            visibleVegetationCellSelector.OnVegetationCellVisibleDelegate -= OnVegetationCellVisible;
            visibleVegetationCellSelector.OnVegetationCellInvisibleDelegate -= OnVegetationCellInvisible;

            if (visibilityChangedIndexList.IsCreated) visibilityChangedIndexList.Dispose();
            if (removeVegetationCellIndexList.IsCreated) removeVegetationCellIndexList.Dispose();
            if (instanceList.IsCreated) instanceList.Dispose();
        }

        public JobHandle RemoveInvisibleCells(JobHandle _processCullingHandle)
        {
            bool needsRemoval = false;
            removeVegetationCellIndexList.Clear();

            for (int i = 0; i < readyToUnloadVegetationCellList.Count; i++)
            {
                removeVegetationCellIndexList.Add(readyToUnloadVegetationCellList[i].index);
                loadedVegetationCellList.RemoveSwapBack(readyToUnloadVegetationCellList[i]);
                OnVegetationCellInvisibleDelegate?.Invoke(readyToUnloadVegetationCellList[i].index);
                needsRemoval = true;
            }
            readyToUnloadVegetationCellList.Clear();

            if (needsRemoval)
            {
                FlagInstancesForRemovalJob flagInstancesForRemovalJob = new()
                {
                    InstanceList = instanceList,
                    RemoveCellIndexList = removeVegetationCellIndexList
                };
                _processCullingHandle = flagInstancesForRemovalJob.ScheduleParallel(instanceList.Length, 64, _processCullingHandle);

                RemoveInstancesJob removeInstancesJob = new() { InstanceList = instanceList };
                _processCullingHandle = removeInstancesJob.Schedule(_processCullingHandle);
            }

            return _processCullingHandle;
        }

        public JobHandle LoadVisibleCells(JobHandle _processCullingHandle) // used in collider / runtime prefab system 
        {
            for (int i = 0; i < readyToLoadVegetationCellList.Count; i++)
            {
                NativeList<MatrixInstance> matrixInstanceList = readyToLoadVegetationCellList[i].GetVegetationPackageInstancesList(vegetationItemIndices.vegetationPackageIndex, vegetationItemIndices.vegetationItemIndex);
                if (matrixInstanceList.IsCreated == false)
                    continue;   // safety check for "out of sync" situations -- when the burst compiler is disabled

                if (useSpawnChance)
                {   // runtime prefab system
                    AddInstancesSpawnChanceJob addInstancesSpawnChanceJob = new()
                    {
                        InstanceList = instanceList,
                        MatrixInstanceList = matrixInstanceList,
                        RandomNumbers = vegetationSystemPro.vegetationCellSpawner.randomNumbers,
                        RandomNumberIndex = readyToLoadVegetationCellList[i].index + spawnSeed,
                        SpawnChance = spawnChance,
                        VegetationCellIndex = readyToLoadVegetationCellList[i].index
                    };
                    _processCullingHandle = addInstancesSpawnChanceJob.Schedule(matrixInstanceList.Length, _processCullingHandle);
                }
                else
                {   // collider system
                    AddInstancesJob addInstancesJob = new()
                    {
                        InstanceList = instanceList,
                        MatrixInstanceList = matrixInstanceList,
                        VegetationCellIndex = readyToLoadVegetationCellList[i].index
                    };
                    _processCullingHandle = addInstancesJob.Schedule(matrixInstanceList.Length, _processCullingHandle);
                }

                if (loadedVegetationCellList.Contains(readyToLoadVegetationCellList[i]) == false)
                    loadedVegetationCellList.Add(readyToLoadVegetationCellList[i]);
            }

            readyToLoadVegetationCellList.Clear();

            return _processCullingHandle;
        }

        public JobHandle ProcessInstanceCulling(JobHandle _processCullingHandle)
        {
            ResetVisibilityJob resetVisibilityJob = new() { InstanceList = instanceList };
            _processCullingHandle = resetVisibilityJob.Schedule(_processCullingHandle);

            for (int i = 0; i < vegetationSystemPro.vegetationStudioCameraList.Count; i++)
            {
                if (vegetationSystemPro.vegetationStudioCameraList[i].IsEnabled() == false)
                    continue;

                DistanceCullingJob distanceCullingJob = new()
                {
                    InstanceList = instanceList,
                    CameraPosition = (float3)vegetationSystemPro.vegetationStudioCameraList[i].selectedCamera.transform.position - vegetationSystemPro.floatingOriginOffset,
                    CullingDistance = cullingDistance,
                };
                _processCullingHandle = distanceCullingJob.Schedule(_processCullingHandle);
            }

            visibilityChangedIndexList.Clear();
            VisibilityChangedFilterManualJob visibilityChangedFilterManualJob = new()
            {
                InstanceList = instanceList,
                VisibilityChangedIndexList = visibilityChangedIndexList
            };
            return visibilityChangedFilterManualJob.Schedule(_processCullingHandle);
        }

        public void ProcessEvents()
        {
            for (int i = 0; i < visibilityChangedIndexList.Length; i++) // for each vegetation instance that recently changed its visibility state
                if (instanceList[visibilityChangedIndexList[i]].Visible == 1)
                    OnVegetationItemVisibleDelegate?.Invoke(instanceList[visibilityChangedIndexList[i]], vegetationItemIndices, vegetationItemID);
                else
                    OnVegetationItemInvisibleDelegate?.Invoke(instanceList[visibilityChangedIndexList[i]], vegetationItemIndices, vegetationItemID);
        }

        [BurstCompile]
        public struct FlagInstancesForRemovalJob : IJobFor
        {
            [NativeDisableParallelForRestriction] public NativeList<ItemSelectorInstanceInfo> InstanceList;
            [ReadOnly] public NativeList<int> RemoveCellIndexList;

            public void Execute(int index)
            {
                ItemSelectorInstanceInfo instanceInfo = InstanceList[index];
                for (int i = 0; i < RemoveCellIndexList.Length; i++)
                    if (instanceInfo.VegetationCellIndex == RemoveCellIndexList[i])
                    {
                        instanceInfo.Remove = 1;
                        InstanceList[index] = instanceInfo;
                        break;
                    }
            }
        }

        [BurstCompile]
        public struct RemoveInstancesJob : IJob
        {
            public NativeList<ItemSelectorInstanceInfo> InstanceList;

            public void Execute()
            {
                for (int i = 0; i < InstanceList.Length; i++)
                {
                    if (InstanceList[i].Remove == 1)
                        InstanceList.RemoveAtSwapBack(i);
                }
            }
        }

        [BurstCompile]
        public struct AddInstancesSpawnChanceJob : IJobFor  // runtime prefab spawner
        {
            [WriteOnly] public NativeList<ItemSelectorInstanceInfo> InstanceList;
            [ReadOnly] public NativeList<MatrixInstance> MatrixInstanceList;
            [ReadOnly] public NativeArray<float> RandomNumbers;

            public int RandomNumberIndex;
            public float SpawnChance;
            public int VegetationCellIndex;

            public void Execute(int index)
            {
                if (MatrixInstanceList[index].controlData.x <= 0)
                    return; // skip masked out persistent vegetation storage vegetation instances

                if (RandomCutoff(SpawnChance, RandomNumberIndex) == false)
                {
                    ItemSelectorInstanceInfo itemSelectorInstanceInfo = new()
                    {
                        VegetationCellIndex = VegetationCellIndex,
                        VegetationCellItemIndex = index,
                        Position = ExtractTranslationFromMatrix(MatrixInstanceList[index].matrix),
                        Scale = ExtractScaleFromMatrix(MatrixInstanceList[index].matrix),
                        Rotation = ExtractRotationFromMatrix(MatrixInstanceList[index].matrix),
                        Visible = -1,
                        LastVisible = -1
                    };
                    InstanceList.Add(itemSelectorInstanceInfo);
                }

                RandomNumberIndex++;
            }

            private bool RandomCutoff(float _value, int _randomNumberIndex)
            {
                return !(_value > RandomRange(_randomNumberIndex, 0, 1));
            }

            public float RandomRange(int _randomNumberIndex, float _min, float _max)
            {
                while (_randomNumberIndex > 9999)
                    _randomNumberIndex -= 10000;
                return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
            }

            private float3 ExtractTranslationFromMatrix(Matrix4x4 _matrix)
            {
                float3 translate;
                translate.x = _matrix.m03;
                translate.y = _matrix.m13;
                translate.z = _matrix.m23;
                return translate;
            }

            private quaternion ExtractRotationFromMatrix(Matrix4x4 _matrix)
            {
                float3 forward;
                forward.x = _matrix.m02;
                forward.y = _matrix.m12;
                forward.z = _matrix.m22;

                if (forward.Equals(float3.zero))
                    return quaternion.identity;

                float3 upward;
                upward.x = _matrix.m01;
                upward.y = _matrix.m11;
                upward.z = _matrix.m21;

                return Quaternion.LookRotation(forward, upward);    // degrees
            }

            private float3 ExtractScaleFromMatrix(Matrix4x4 _matrix)
            {
                return new float3(_matrix.GetColumn(0).magnitude, _matrix.GetColumn(1).magnitude, _matrix.GetColumn(2).magnitude);
            }
        }

        [BurstCompile]
        public struct AddInstancesJob : IJobFor // collider system
        {
            [WriteOnly] public NativeList<ItemSelectorInstanceInfo> InstanceList;
            [ReadOnly] public NativeList<MatrixInstance> MatrixInstanceList;
            [ReadOnly] public int VegetationCellIndex;

            public void Execute(int index)
            {
                if (MatrixInstanceList[index].controlData.x <= 0)
                    return; // skip masked out persistent vegetation storage vegetation instances

                ItemSelectorInstanceInfo itemSelectorInstanceInfo = new()
                {
                    VegetationCellIndex = VegetationCellIndex,
                    VegetationCellItemIndex = index,
                    Position = ExtractTranslationFromMatrix(MatrixInstanceList[index].matrix),
                    Scale = ExtractScaleFromMatrix(MatrixInstanceList[index].matrix),
                    Rotation = ExtractRotationFromMatrix(MatrixInstanceList[index].matrix),
                    Visible = -1,
                    LastVisible = -1
                };
                InstanceList.Add(itemSelectorInstanceInfo);
            }

            private float3 ExtractTranslationFromMatrix(Matrix4x4 _matrix)
            {
                float3 translate;
                translate.x = _matrix.m03;
                translate.y = _matrix.m13;
                translate.z = _matrix.m23;
                return translate;
            }

            private quaternion ExtractRotationFromMatrix(Matrix4x4 _matrix)
            {
                float3 forward;
                forward.x = _matrix.m02;
                forward.y = _matrix.m12;
                forward.z = _matrix.m22;

                if (forward.Equals(float3.zero))
                    return quaternion.identity;

                float3 upward;
                upward.x = _matrix.m01;
                upward.y = _matrix.m11;
                upward.z = _matrix.m21;

                return Quaternion.LookRotation(forward, upward);    // degrees
            }

            private float3 ExtractScaleFromMatrix(Matrix4x4 _matrix)
            {
                return new float3(_matrix.GetColumn(0).magnitude, _matrix.GetColumn(1).magnitude, _matrix.GetColumn(2).magnitude);
            }
        }

        [BurstCompile]
        public struct ResetVisibilityJob : IJob
        {
            public NativeList<ItemSelectorInstanceInfo> InstanceList;

            public void Execute()
            {
                for (int i = 0; i < InstanceList.Length; i++)
                {
                    ItemSelectorInstanceInfo itemSelectorInstanceInfo = InstanceList[i];
                    itemSelectorInstanceInfo.LastVisible = itemSelectorInstanceInfo.Visible;
                    itemSelectorInstanceInfo.Visible = 0;
                    InstanceList[i] = itemSelectorInstanceInfo;
                }
            }
        }

        [BurstCompile]
        public struct DistanceCullingJob : IJob
        {
            public NativeList<ItemSelectorInstanceInfo> InstanceList;
            [ReadOnly] public float3 CameraPosition;
            [ReadOnly] public float CullingDistance;

            public void Execute()
            {
                for (int i = 0; i < InstanceList.Length; i++)
                {
                    ItemSelectorInstanceInfo itemSelectorInstanceInfo = InstanceList[i];
                    if (math.distance(itemSelectorInstanceInfo.Position, CameraPosition) <= CullingDistance)
                        itemSelectorInstanceInfo.Visible = 1;
                    //else
                    //    itemSelectorInstanceInfo.Visible = -1;
                    //itemSelectorInstanceInfo.Visible = math.select(0, 1, math.distance(itemSelectorInstanceInfo.Position, CameraPosition) <= CullingDistance);    // doesn't work for multi camera setups
                    InstanceList[i] = itemSelectorInstanceInfo;
                }
            }
        }

        [BurstCompile]
        public struct VisibilityChangedFilterManualJob : IJob
        {
            [ReadOnly] public NativeList<ItemSelectorInstanceInfo> InstanceList;
            [WriteOnly] public NativeList<int> VisibilityChangedIndexList;

            public void Execute()
            {
                for (int i = 0; i < InstanceList.Length; i++)
                {
                    if (InstanceList[i].Visible != InstanceList[i].LastVisible)
                        VisibilityChangedIndexList.Add(i);
                }
            }
        }
    }
}