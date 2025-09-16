using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace AwesomeTechnologies.PrefabSpawner
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Systems/RuntimePrefabSpawner", 4)]
    [ScriptExecutionOrder(105)]
    [ExecuteInEditMode]
    public class RuntimePrefabSpawner : MonoBehaviour
    {
        public VegetationSystemPro vegetationSystemPro;

        [NonSerialized] public VisibleVegetationCellSelector visibleVegetationCellSelector;
        [NonSerialized] public readonly List<VegetationPackageRuntimePrefabInfo> packageRuntimePrefabInfoList = new();
        public NativeList<JobHandle> jobHandleList;

        public int currentTabIndex;
        public int vegetationPackageIndex;
        private Transform runtimePrefabParent;
        private float3 lastFloatingOriginOffset;

        public bool showDebugCells;
        public bool showRuntimePrefabs;

        private void Reset()
        {
            FindVegetationSystemPro();
        }

        private void FindVegetationSystemPro()
        {
            if (vegetationSystemPro == false)
                vegetationSystemPro = GetComponent<VegetationSystemPro>();
        }

        private void OnEnable()
        {
            FindVegetationSystemPro();
            SetFloatingOrigin();
            SetupDelegates();
            SetupRuntimePrefabSystem();
        }

        private void OnDisable()
        {
            DisposeRuntimePrefabSystem();
            RemoveDelegates();
        }

        private void OnDrawGizmosSelected()
        {
            if (showDebugCells)
                visibleVegetationCellSelector?.DrawDebugGizmos();
        }

        void SetFloatingOrigin()
        {
            if (vegetationSystemPro == false)
                return;

            lastFloatingOriginOffset = vegetationSystemPro.floatingOriginOffset;
        }

        void TestFloatingOrigin()
        {
            if (vegetationSystemPro == false)
                return;

            if (lastFloatingOriginOffset.Equals(vegetationSystemPro.floatingOriginOffset) == false)
                UpdateFloatingOrigin(vegetationSystemPro.floatingOriginOffset - lastFloatingOriginOffset);

            lastFloatingOriginOffset = vegetationSystemPro.floatingOriginOffset;
        }

        void UpdateFloatingOrigin(Vector3 _deltaFloatingOriginOffset)
        {
            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k <= packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.runtimePrefabStorage.UpdateFloatingOrigin(_deltaFloatingOriginOffset);
        }

        public void RefreshRuntimePrefabs()
        {
            SetupRuntimePrefabSystem();
        }

        private void SetupDelegates()
        {
            if (vegetationSystemPro == false)
                return;

            vegetationSystemPro.OnStartVegetationSystemDelegate += OnStartVegetationSystem;
            vegetationSystemPro.OnUnloadVegetationSystemDelegate += OnStartVegetationSystem;
            vegetationSystemPro.OnRefreshRuntimePrefabSpawnerDelegate += OnStartVegetationSystem;

            vegetationSystemPro.OnClearCacheDelegate += OnClearCache;
            vegetationSystemPro.OnClearCacheVegetationCellDelegate += OnClearCacheVegetationCell;
            vegetationSystemPro.OnClearCacheVegetationItemDelegate += OnClearCacheVegetationItem;
            vegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate += OnClearCacheVegetationCellVegetationItem;
            vegetationSystemPro.OnRenderCompleteDelegate += OnRenderComplete;
        }

        private void RemoveDelegates()
        {
            if (vegetationSystemPro == false)
                return;

            vegetationSystemPro.OnStartVegetationSystemDelegate -= OnStartVegetationSystem;
            vegetationSystemPro.OnUnloadVegetationSystemDelegate -= OnStartVegetationSystem;
            vegetationSystemPro.OnRefreshRuntimePrefabSpawnerDelegate -= OnStartVegetationSystem;

            vegetationSystemPro.OnClearCacheDelegate -= OnClearCache;
            vegetationSystemPro.OnClearCacheVegetationCellDelegate -= OnClearCacheVegetationCell;
            vegetationSystemPro.OnClearCacheVegetationItemDelegate -= OnClearCacheVegetationItem;
            vegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate -= OnClearCacheVegetationCellVegetationItem;
            vegetationSystemPro.OnRenderCompleteDelegate -= OnRenderComplete;
        }

        private void OnStartVegetationSystem(VegetationSystemPro _vegetationSystemPro)
        {
            SetupRuntimePrefabSystem();
        }

        private void OnClearCache(VegetationSystemPro _vegetationSystemPro)
        {
            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.vegetationItemSelector.RefreshAllVegetationCells();
        }

        private void OnClearCacheVegetationCell(VegetationSystemPro _vegetationSystemPro, VegetationCell _vegetationCell)
        {
            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.vegetationItemSelector.RefreshVegetationCell(_vegetationCell);
        }

        private void OnClearCacheVegetationItem(VegetationSystemPro _vegetationSystemPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    if (i == _vegetationPackageIndex && j == _vegetationItemIndex)  // refresh all vegetation cells -- only if it's the matching vegetation item
                        for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                            packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.vegetationItemSelector.RefreshAllVegetationCells();
        }

        private void OnClearCacheVegetationCellVegetationItem(VegetationSystemPro _vegetationSystemPro, VegetationCell _vegetationCell, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    if (i == _vegetationPackageIndex && j == _vegetationItemIndex)  // refresh the specified vegetation cell -- only if it's the matching vegetation item
                        for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                            packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.vegetationItemSelector.RefreshVegetationCell(_vegetationCell);
        }

        public void SetupRuntimePrefabSystem()
        {
            DisposeRuntimePrefabSystem();

            if (vegetationSystemPro == false || vegetationSystemPro.isSetupDone == false || Application.isPlaying == false)
                return; // only run in play mode to not clutter the scene view with details/particles/sounds -- also not with gameObjects and their colliders since they render slower

            jobHandleList = new NativeList<JobHandle>(64, Allocator.Persistent);

            GameObject runtimePrefabParentObject = new("Run-time prefabs") { hideFlags = HideFlags.DontSave };
            runtimePrefabParentObject.transform.SetParent(transform);   // parent below main system
            runtimePrefabParent = runtimePrefabParentObject.transform;  // assign for global access

            visibleVegetationCellSelector = new();

            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)    // for each vegetation package
            {
                VegetationPackageRuntimePrefabInfo vegetationPackageRuntimePrefabInfo = new();

                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)  // for each vegetation item
                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RuntimePrefabRuleList.Count > 0)
                    {
                        VegetationItemRuntimePrefabInfo vegetationItemRuntimePrefabInfo = new();

                        for (int k = 0; k < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RuntimePrefabRuleList.Count; k++) // for each "runtime prefab rule" of a vegetation item
                            vegetationItemRuntimePrefabInfo.runtimePrefabManagerList.Add(new RuntimePrefabManager(visibleVegetationCellSelector, vegetationSystemPro, vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j],
                                vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].RuntimePrefabRuleList[k], runtimePrefabParent, showRuntimePrefabs));

                        vegetationPackageRuntimePrefabInfo.runtimePrefabManagerList_Ex.Add(vegetationItemRuntimePrefabInfo);
                    }

                packageRuntimePrefabInfoList.Add(vegetationPackageRuntimePrefabInfo);
            }

            visibleVegetationCellSelector.Init(vegetationSystemPro);
        }

        void DestroyRuntimePrefabParent()
        {
            if (runtimePrefabParent == false)
                return;

            if (Application.isPlaying)
                Destroy(runtimePrefabParent.gameObject);
            else
                DestroyImmediate(runtimePrefabParent.gameObject);
        }

        private void OnRenderComplete(VegetationSystemPro _vegetationSystemPro)
        {
            if (packageRuntimePrefabInfoList.Count == 0)
                return;

            Profiler.BeginSample("Runtime prefab system processing");

            TestFloatingOrigin();

            jobHandleList.Clear();
            JobHandle cullingJobHandle = default;

            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                    {
                        if (packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k] == null)
                            continue;

                        JobHandle itemCullingHandle = cullingJobHandle;
                        itemCullingHandle = packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k].vegetationItemSelector.RemoveInvisibleCells(itemCullingHandle);
                        itemCullingHandle = packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k].vegetationItemSelector.LoadVisibleCells(itemCullingHandle);
                        itemCullingHandle = packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k].vegetationItemSelector.ProcessInstanceCulling(itemCullingHandle);
                        jobHandleList.Add(itemCullingHandle);
                    }

            JobHandle mergedHandle = JobHandle.CombineDependencies(jobHandleList.AsArray());    // combine job handles after cell state handling > culling
            JobHandle.ScheduleBatchedJobs();    //  run/prioritize all runtime prefab system jobs
            mergedHandle.Complete();    // finish / synchronize assigned job handles

            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.vegetationItemSelector.ProcessEvents();

            Profiler.EndSample();
        }

        public void DisposeRuntimePrefabSystem()
        {
            if (jobHandleList.IsCreated)
                jobHandleList.Dispose();

            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
            {
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                {
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.Dispose();
                    packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Clear();
                }
                packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Clear();
            }

            packageRuntimePrefabInfoList.Clear();
            visibleVegetationCellSelector?.Dispose();
            visibleVegetationCellSelector = null;
            DestroyRuntimePrefabParent();
        }

        #region debug stuff
        public void SetRuntimePrefabVisibility(bool _value)
        {
            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k]?.SetRuntimePrefabVisibility(_value);
        }

        public int GetLoadedInstanceCount()
        {
            int instanceCount = 0;

            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        instanceCount += packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k].vegetationItemSelector.instanceList.Length;
            return instanceCount;
        }

        public int GetVisibleColliders()
        {
            int instanceCount = 0;

            for (int i = 0; i < packageRuntimePrefabInfoList.Count; i++)
                for (int j = 0; j < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex.Count; j++)
                    for (int k = 0; k < packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList.Count; k++)
                        instanceCount += packageRuntimePrefabInfoList[i].runtimePrefabManagerList_Ex[j].runtimePrefabManagerList[k].runtimePrefabStorage.runtimePrefabInfoList.Count;
            return instanceCount;
        }
        #endregion
    }
}