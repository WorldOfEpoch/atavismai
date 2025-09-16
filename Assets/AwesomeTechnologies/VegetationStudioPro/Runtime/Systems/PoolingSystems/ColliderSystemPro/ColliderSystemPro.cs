using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using AwesomeTechnologies.Shaders;

namespace AwesomeTechnologies.ColliderSystem
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Systems/ColliderSystemPro", 2)]
    [ScriptExecutionOrder(105)]
    [ExecuteInEditMode]
    public class ColliderSystemPro : MonoBehaviour
    {
        public VegetationSystemPro vegetationSystemPro;

        [NonSerialized] public VisibleVegetationCellSelector visibleVegetationCellSelector;
        [NonSerialized] public readonly List<VegetationPackageColliderInfo> packageColliderInfoList = new();
        public NativeList<JobHandle> jobHandleList;

        public delegate void MultiCreateColliderDelegate(GameObject _colliderGameObject);
        public MultiCreateColliderDelegate OnCreateColliderDelegate;

        public delegate void MultiBeforeDestroyColliderDelegate(GameObject _colliderGameObject);
        public MultiBeforeDestroyColliderDelegate OnBeforeDestroyColliderDelegate;

        public int currentTabIndex;
        private Transform colliderParent;
        private float3 lastFloatingOriginOffset;

        public bool setBakedCollidersStatic = false;
        public bool convertBakedCollidersToMesh = false;

        public bool excludeTrees;
        public bool excludeObjects;
        public bool excludeLargeObjects;

        public bool showDebugCells;
        public bool showColliders;

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
            SetupColliderSystem();
        }

        private void OnDisable()
        {
            DisposeColliderSystem();
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

        void UpdateFloatingOrigin(float3 _deltaFloatingOriginOffset)
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    packageColliderInfoList[i].colliderManagerList[j]?.runtimePrefabStorage.UpdateFloatingOrigin(_deltaFloatingOriginOffset);
        }

        private void SetupDelegates()
        {
            if (vegetationSystemPro == false)
                return;

            vegetationSystemPro.OnStartVegetationSystemDelegate += OnStartVegetationSystem;
            vegetationSystemPro.OnUnloadVegetationSystemDelegate += OnStartVegetationSystem;
            vegetationSystemPro.OnRefreshColliderSystemDelegate += OnStartVegetationSystem;


            vegetationSystemPro.OnClearCacheDelegate += OnClearCache;
            vegetationSystemPro.OnClearCacheVegetationCellDelegate += OnClearCacheVegetationCell;
            vegetationSystemPro.OnClearCacheVegetationItemDelegate += OnClearCacheVegetationItem;
            vegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate += OnClearCacheVegetationCellVegetationItem;
            vegetationSystemPro.OnRenderCompleteDelegate += OnRenderComplete;
        }

        private void OnCreateCollider(GameObject _colliderObject)
        {
            OnCreateColliderDelegate?.Invoke(_colliderObject);
        }

        private void OnBeforeDestroyCollider(GameObject _colliderObject)
        {
            OnBeforeDestroyColliderDelegate?.Invoke(_colliderObject);
        }

        private void RemoveDelegates()
        {
            if (vegetationSystemPro == false)
                return;

            vegetationSystemPro.OnStartVegetationSystemDelegate -= OnStartVegetationSystem;
            vegetationSystemPro.OnUnloadVegetationSystemDelegate -= OnStartVegetationSystem;
            vegetationSystemPro.OnRefreshColliderSystemDelegate -= OnStartVegetationSystem;

            vegetationSystemPro.OnClearCacheDelegate -= OnClearCache;
            vegetationSystemPro.OnClearCacheVegetationCellDelegate -= OnClearCacheVegetationCell;
            vegetationSystemPro.OnClearCacheVegetationItemDelegate -= OnClearCacheVegetationItem;
            vegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate -= OnClearCacheVegetationCellVegetationItem;
            vegetationSystemPro.OnRenderCompleteDelegate -= OnRenderComplete;
        }

        private void OnStartVegetationSystem(VegetationSystemPro _vegetationSystemPro)
        {
            SetupColliderSystem();
        }


        public void SetupColliderSystem()
        {
            DisposeColliderSystem();

            if (vegetationSystemPro == false || vegetationSystemPro.isSetupDone == false)
                return;

            jobHandleList = new NativeList<JobHandle>(64, Allocator.Persistent);

            GameObject colliderParentObject = new("Run-time colliders") { hideFlags = HideFlags.DontSave };
            colliderParentObject.transform.SetParent(transform);    // parent below main system
            colliderParent = colliderParentObject.transform;    // assign for global access

            visibleVegetationCellSelector = new();

            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = new();

                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderType != ColliderType.Disabled)    // only create colliders for items that use them
                    {
                        ColliderManager tmpColliderManager = new(visibleVegetationCellSelector, vegetationSystemPro, vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j], colliderParent, showColliders);
                        tmpColliderManager.OnCreateColliderDelegate += OnCreateCollider;
                        tmpColliderManager.OnBeforeDestroyColliderDelegate += OnBeforeDestroyCollider;

                        vegetationPackageColliderInfo.colliderManagerList.Add(tmpColliderManager);
                    }
                    else
                    {
                        vegetationPackageColliderInfo.colliderManagerList.Add(null);    // keep list length -- add null specifically to skip certain logic but keep certain other logic
                    }

                packageColliderInfoList.Add(vegetationPackageColliderInfo);
            }

            visibleVegetationCellSelector.Init(vegetationSystemPro);
        }

        private void OnRenderComplete(VegetationSystemPro _vegetationSystemPro)
        {
            if (packageColliderInfoList.Count <= 0)
                return;

            Profiler.BeginSample("Collider system processing");

            TestFloatingOrigin();

            jobHandleList.Clear();
            JobHandle cullingJobHandle = default;

            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                {
                    if (packageColliderInfoList[i].colliderManagerList[j] == null)
                        continue;

                    JobHandle itemCullingHandle = cullingJobHandle;
                    itemCullingHandle = packageColliderInfoList[i].colliderManagerList[j].vegetationItemSelector.RemoveInvisibleCells(itemCullingHandle);
                    itemCullingHandle = packageColliderInfoList[i].colliderManagerList[j].vegetationItemSelector.LoadVisibleCells(itemCullingHandle);
                    itemCullingHandle = packageColliderInfoList[i].colliderManagerList[j].vegetationItemSelector.ProcessInstanceCulling(itemCullingHandle);
                    jobHandleList.Add(itemCullingHandle);
                }

            JobHandle mergedHandle = JobHandle.CombineDependencies(jobHandleList.AsArray());    // combine job handles after cell state handling > culling
            JobHandle.ScheduleBatchedJobs();    // run/prioritize all collider system jobs
            mergedHandle.Complete();    // finish / synchronize assigned job handles

            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    packageColliderInfoList[i].colliderManagerList[j]?.vegetationItemSelector.ProcessEvents();

            Profiler.EndSample();
        }

        private void OnClearCache(VegetationSystemPro _vegetationSystemPro)
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    packageColliderInfoList[i].colliderManagerList[j]?.vegetationItemSelector.RefreshAllVegetationCells();
        }

        private void OnClearCacheVegetationCell(VegetationSystemPro _vegetationSystemPro, VegetationCell _vegetationCell)
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    packageColliderInfoList[i].colliderManagerList[j]?.vegetationItemSelector.RefreshVegetationCell(_vegetationCell);
        }

        private void OnClearCacheVegetationItem(VegetationSystemPro _vegetationSystemPro, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    if (i == _vegetationPackageIndex && j == _vegetationItemIndex)
                        packageColliderInfoList[i].colliderManagerList[j]?.vegetationItemSelector.RefreshAllVegetationCells();
        }

        private void OnClearCacheVegetationCellVegetationItem(VegetationSystemPro _vegetationSystemPro, VegetationCell _vegetationCell, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    if (i == _vegetationPackageIndex && j == _vegetationItemIndex)
                        packageColliderInfoList[i].colliderManagerList[j]?.vegetationItemSelector.RefreshVegetationCell(_vegetationCell);
        }

        void DestroyColliderParent()
        {
            if (colliderParent == null)
                return;

            if (Application.isPlaying)
                Destroy(colliderParent.gameObject);
            else
                DestroyImmediate(colliderParent.gameObject);
        }

        public void DisposeColliderSystem()
        {
            if (jobHandleList.IsCreated)
                jobHandleList.Dispose();

            for (int i = 0; i < packageColliderInfoList.Count; i++)
            {
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    if (packageColliderInfoList[i].colliderManagerList[j] != null)
                    {
                        packageColliderInfoList[i].colliderManagerList[j].OnCreateColliderDelegate -= OnCreateCollider;
                        packageColliderInfoList[i].colliderManagerList[j].OnBeforeDestroyColliderDelegate -= OnBeforeDestroyCollider;
                        packageColliderInfoList[i].colliderManagerList[j].Dispose();
                    }

                packageColliderInfoList[i].colliderManagerList.Clear();
            }

            packageColliderInfoList.Clear();
            visibleVegetationCellSelector?.Dispose();
            visibleVegetationCellSelector = null;
            DestroyColliderParent();
        }

        #region debug stuff
        public void SetColliderVisibility(bool _value)
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    packageColliderInfoList[i].colliderManagerList[j]?.SetColliderVisibility(_value);
        }

        public int GetLoadedInstanceCount()
        {
            int instanceCount = 0;

            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    if (packageColliderInfoList[i].colliderManagerList[j] != null)
                        instanceCount += packageColliderInfoList[i].colliderManagerList[j].vegetationItemSelector.instanceList.Length;
            return instanceCount;
        }

        public int GetVisibleColliders()
        {
            int instanceCount = 0;

            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                    if (packageColliderInfoList[i].colliderManagerList[j] != null)
                        instanceCount += packageColliderInfoList[i].colliderManagerList[j].runtimePrefabStorage.runtimePrefabInfoList.Count;
            return instanceCount;
        }
        #endregion

        public void BakeCollidersToScene()
        {
            for (int i = 0; i < packageColliderInfoList.Count; i++)
                for (int j = 0; j < packageColliderInfoList[i].colliderManagerList.Count; j++)
                {
                    if (vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderUseForBake == false || vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].ColliderType == ColliderType.Disabled)
                        continue;   // skip baking unsupported colliders -- lists get synced w/ "null" dummies in the "SETUP"

                    BakeVegetationItemColliders(packageColliderInfoList[i].colliderManagerList[j], vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j]);
                }
        }

        void BakeVegetationItemColliders(ColliderManager _colliderManager, VegetationItemInfoPro _vegetationItemInfoPro)
        {
            if (excludeTrees && _vegetationItemInfoPro.VegetationType == VegetationType.Tree) return;
            if (excludeObjects && _vegetationItemInfoPro.VegetationType == VegetationType.Objects) return;
            if (excludeLargeObjects && _vegetationItemInfoPro.VegetationType == VegetationType.LargeObjects) return;

            vegetationSystemPro.ClearCache(_vegetationItemInfoPro.VegetationItemID);

            GameObject rootItem = new("Baked colliders_" + _vegetationItemInfoPro.Name + "_" + _vegetationItemInfoPro.VegetationItemID);

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                EditorUtility.DisplayProgressBar("Bake collider: " + _vegetationItemInfoPro.Name, "Spawn all cells", 0);
#endif                       
            int colliderCount = 0;

            for (int i = 0; i < vegetationSystemPro.vegetationCellList.Count; i++)
            {
#if UNITY_EDITOR
                if (i % 10 == 0 && Application.isPlaying == false)
                    EditorUtility.DisplayProgressBar("Bake collider: " + _vegetationItemInfoPro.Name, "Spawn cell " + i + "/" + (vegetationSystemPro.vegetationCellList.Count - 1), i / ((float)vegetationSystemPro.vegetationCellList.Count - 1));
#endif

                vegetationSystemPro.SpawnVegetationCellEx(vegetationSystemPro.vegetationCellList[i], _vegetationItemInfoPro.VegetationItemID);
                NativeList<MatrixInstance> vegetationInstanceList = vegetationSystemPro.GetVegetationItemInstances(vegetationSystemPro.vegetationCellList[i], _vegetationItemInfoPro.VegetationItemID);

                for (int j = 0; j < vegetationInstanceList.Length; j++)
                {
                    if (vegetationInstanceList[j].controlData.x <= 0)
                        continue;   // skip masked out persistent vegetation storage vegetation instances

                    ItemSelectorInstanceInfo itemSelectorInstanceInfo = new()
                    {
                        Position = MatrixTools.ExtractTranslationFromMatrix(vegetationInstanceList[j].matrix),
                        Scale = MatrixTools.ExtractScaleFromMatrix(vegetationInstanceList[j].matrix),
                        Rotation = MatrixTools.ExtractRotationFromMatrix(vegetationInstanceList[j].matrix)
                    };

                    GameObject newCollider = _colliderManager.colliderPool.GetObject(itemSelectorInstanceInfo);
                    newCollider.transform.SetParent(rootItem.transform, true);
                    newCollider.hideFlags = HideFlags.None; // make sure the engine doesn't "cull/optimize" the colliders away

                    Collider[] colls = newCollider.GetComponentsInChildren<Collider>();
                    for (int k = 0; k < colls.Length; k++)
                        colls[k].gameObject.hideFlags = HideFlags.None; // "From Prefab" mode -- make sure the engine doesn't "cull/optimize" the colliders away

                    SetNavmeshArea(newCollider, _vegetationItemInfoPro.NavMeshArea);

                    if (setBakedCollidersStatic)
                        SetStatic(newCollider);

                    if (convertBakedCollidersToMesh)
                        CreateMeshPrimitives(newCollider);

                    colliderCount++;
                }

                vegetationSystemPro.vegetationCellList[i].ClearCache();
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                EditorUtility.ClearProgressBar();
            SetSceneDirty();
#endif

            if (colliderCount == 0)
                DestroyImmediate(rootItem);
        }

        private void SetStatic(GameObject _go)
        {
#if UNITY_EDITOR
            Collider[] colliders = _go.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].gameObject.isStatic = true;
#endif
        }

        static void SetNavmeshArea(GameObject _go, int _navmeshArea)
        {
#if UNITY_EDITOR          
            GameObjectUtility.SetNavMeshArea(_go, _navmeshArea);
#endif
            foreach (Transform child in _go.transform)
                SetNavmeshArea(child.gameObject, _navmeshArea);
        }

#if UNITY_EDITOR
        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(gameObject);
        }
#endif

        private void CreateMeshPrimitives(GameObject _go)
        {
            Material colliderMaterial = new(ShaderUtility.GetShader_EngineDefault());

            Collider[] colliders = _go.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                CapsuleCollider capsuleCollider = colliders[i] as CapsuleCollider;
                if (capsuleCollider != null)
                {
                    capsuleCollider.gameObject.AddComponent<MeshFilter>().sharedMesh = MeshUtility.CreateCapsuleMesh(capsuleCollider.radius, capsuleCollider.height);
                    capsuleCollider.gameObject.AddComponent<MeshRenderer>().sharedMaterial = colliderMaterial;

                    switch (capsuleCollider.direction)
                    {
                        case 0: //  degrees
                            capsuleCollider.transform.rotation = Quaternion.Euler(capsuleCollider.transform.rotation.eulerAngles.x, capsuleCollider.transform.rotation.eulerAngles.y, capsuleCollider.transform.rotation.eulerAngles.z - 90);
                            break;
                        case 2: //  degrees
                            capsuleCollider.transform.rotation = Quaternion.Euler(capsuleCollider.transform.rotation.eulerAngles.x - 90, capsuleCollider.transform.rotation.eulerAngles.y, capsuleCollider.transform.rotation.eulerAngles.z);
                            break;
                    }

                    capsuleCollider.transform.localPosition += new Vector3(
                        capsuleCollider.center.x * capsuleCollider.transform.localScale.x,
                        capsuleCollider.center.y * capsuleCollider.transform.localScale.y,
                        capsuleCollider.center.z * capsuleCollider.transform.localScale.z);
                    DestroyImmediate(capsuleCollider);
                }

                MeshCollider meshCollider = colliders[i] as MeshCollider;
                if (meshCollider != null)
                {
                    meshCollider.gameObject.AddComponent<MeshFilter>().sharedMesh = meshCollider.sharedMesh;
                    meshCollider.gameObject.AddComponent<MeshRenderer>().sharedMaterial = colliderMaterial;
                    DestroyImmediate(meshCollider);
                }

                BoxCollider boxCollider = colliders[i] as BoxCollider;
                if (boxCollider != null)
                {
                    boxCollider.gameObject.AddComponent<MeshFilter>().sharedMesh = MeshUtility.CreateBoxMesh(boxCollider.size.z, boxCollider.size.x, boxCollider.size.y);
                    boxCollider.gameObject.AddComponent<MeshRenderer>().sharedMaterial = colliderMaterial;
                    boxCollider.transform.localPosition += new Vector3(
                        boxCollider.center.x * boxCollider.transform.localScale.x,
                        boxCollider.center.y * boxCollider.transform.localScale.y,
                        boxCollider.center.z * boxCollider.transform.localScale.z);
                    DestroyImmediate(boxCollider);
                }

                SphereCollider sphereCollider = colliders[i] as SphereCollider;
                if (sphereCollider != null)
                {
                    sphereCollider.gameObject.AddComponent<MeshFilter>().sharedMesh = MeshUtility.CreateSphereMesh(sphereCollider.radius);
                    sphereCollider.gameObject.AddComponent<MeshRenderer>().sharedMaterial = colliderMaterial;
                    sphereCollider.transform.localPosition += new Vector3(
                        sphereCollider.center.x * sphereCollider.transform.localScale.x,
                        sphereCollider.center.y * sphereCollider.transform.localScale.y,
                        sphereCollider.center.z * sphereCollider.transform.localScale.z);
                    DestroyImmediate(sphereCollider);
                }
            }
        }
    }
}