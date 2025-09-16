using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Culling;
using AwesomeTechnologies.Utility.Quadtree;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void CreateBillboardCells()
        {
            //DisposeBillboardCells();

            // cell creation -- quad tree setup
            Rect vegetationSystemRect = RectExtension.CreateRectFromBounds(new(vegetationSystemBounds.center, vegetationSystemBounds.size + 2 * billboardCellSize * Vector3.one));  // total area + safety "edge case" offset
            billboardCellQuadTree = new QuadTree<BillboardCell>(vegetationSystemRect);  // setup the quadTree to use for "query" operations later ex: "ClearCache(Bounds _bounds)"
            float2 cellCorner = new(vegetationSystemBounds.min.x, vegetationSystemBounds.min.z);    // use each (bottom left) cellCorner as the "startPosition" anchor

            for (int x = 0; x < (int)math.ceil(vegetationSystemBounds.size.x / billboardCellSize); x++) // for each cell on the X-Axis
                for (int z = 0; z < (int)math.ceil(vegetationSystemBounds.size.z / billboardCellSize); z++) // for each cell on the Z-Axis
                {   // create a new billboardCell -- position it based on the current XZ count times the size + cellCorner offset to anchor it -- give it it's entire bounds
                    BillboardCell billboardCell = new(new(new(billboardCellSize * x + cellCorner.x, billboardCellSize * z + cellCorner.y), new(billboardCellSize, billboardCellSize)), vegetationSystemBounds.center.y, vegetationSystemBounds.size.y);
                    billboardCellList.Add(billboardCell);   // add to the list for indexing
                    billboardCell.index = billboardCellList.Count - 1;  // assign an index for specific access ex: persistent storage
                    billboardCellQuadTree.Insert(billboardCell);    // insert into the quadTree for querying
                }

            loadedBillboardCellList.Clear();    // safety clear
            loadedBillboardCellList.Capacity = billboardCellList.Count; // set capacity directly to max possible -- avoid increasing it in single steps

            PrepareAllBillboardCells(); // prepare all billboardCells -- create billoardInstances to store (temporary merged) mesh data => get filled later when loading billboards
        }

        private void PrepareAllBillboardCells()
        {
            for (int i = 0; i < billboardCellList.Count; i++)
                billboardCellList[i].PrepareBillboardCell(vegetationPackageProList);
        }

        private void LoadBillboardCells()   // the whole loop runs only once for a needed billboard cell ..unless billboard cell caches get cleared
        {
            Profiler.BeginSample("Prepare billboard render list");
            Profiler.BeginSample("Billboard cell loading jobs");
            vegetationCellSpawner.cellJobHandleList.Clear();

            Profiler.BeginSample("Billboard cell (pre-)loading");
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false)
                    continue;

                for (int j = 0; j < vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList.Length; j++)
                {
                    BillboardCell billboardCell = vegetationStudioCameraList[i].preloadBillboardCellList[vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j]];
                    if (billboardCell.loadedState != 0)
                        continue;   // skip if (fully) loaded w/ mesh-data / vegetationCells

                    vegetationCellQuadTree.Query(billboardCell.Rectangle, billboardCell.overlapVegetationCells);    // compare the area of veg cells vs bb cells -- fill the bb cell's overlap list with matching veg cells
                    for (int k = 0; k < billboardCell.overlapVegetationCells.Count; k++)    // for all vegetation cells within the billboard cell
                    {
                        /// all vegetation cells overlapping with a billboard cell's area get loaded which usually is a big chunk of vegetation cells
                        /// -> those vegetation cells all get cleared using the compact cache after the billboards got fully loaded and created
                        /// ..meaning this "hard load" of vegetation cells usually only happens once ..per loaded billboard cell batch / per hard refresh
                        /// => so performance / RAM usage is ok
                        /// ..only billboard internal data remains and gets re-used so long nothing changes with the generation rules
                        /// 

                        if (billboardCell.overlapVegetationCells[k].loadedDistanceBand <= 1)
                            continue;   // skip already loaded vegetation cells that have type 0/1

                        // spawn vegetation cells only for billboards to get their mesh-tree positions based on all the rules
                        vegetationCellSpawner.cellJobHandleList.Add(vegetationCellSpawner.SpawnVegetationCell(billboardCell.overlapVegetationCells[k], 1, out _, true, false));

                        if (loadedVegetationCellList.Contains(billboardCell.overlapVegetationCells[k]) == false)    // add to loaded vegetation cell list to clear their cache later
                            loadedVegetationCellList.Add(billboardCell.overlapVegetationCells[k]);
                    }

                    loadedBillboardCellList.Add(billboardCell);
                    billboardCell.loadedState = 1;  // set as loaded vegetationCells

                    if (predictiveCellLoader.preloadBillboardCellList.Count > 0 && vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j] < predictiveCellLoader.preloadBillboardCellList.Count)   // empty when "PredictivePreloading" is disabled
                        predictiveCellLoader.preloadBillboardCellList.RemoveAtSwapBack(vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j]);    // remove early to avoid trying to re-load a cell thus avoid "load misses"
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Billboard cell predictive preloading");
            predictiveBillboardCellLoaderList.Clear();
            predictiveCellLoader.GetAllBillboardCellsForThisFrame(predictiveBillboardCellLoaderList);   // check if "PredictivePreloading" is enabled and get available cells
            for (int i = 0; i < predictiveBillboardCellLoaderList.Count; i++)   // "predictive pre-load" all additional cells available if any
            {
                BillboardCell billboardCell = predictiveBillboardCellLoaderList[i];
                if (billboardCell.loadedState != 0)
                    continue;   // skip if (fully) loaded w/ mesh-data / vegetationCells

                vegetationCellQuadTree.Query(billboardCell.Rectangle, billboardCell.overlapVegetationCells);    // compare the area of veg cells vs bb cells -- fill the bb cell's overlap list with matching veg cells
                for (int j = 0; j < billboardCell.overlapVegetationCells.Count; j++)    // for all vegetation cells within the billboard cell
                {
                    /// all vegetation cells overlapping with a billboard cell's area get loaded which usually is a big chunk of vegetation cells
                    /// -> those vegetation cells all get cleared using the compact cache after the billboards got fully loaded and created
                    /// ..meaning this "hard load" of vegetation cells usually only happens once ..per loaded billboard cell batch / per hard refresh
                    /// => so performance / RAM usage is ok
                    /// ..only billboard internal data remains and gets re-used so long nothing changes with the generation rules
                    /// 

                    if (billboardCell.overlapVegetationCells[j].loadedDistanceBand <= 1)
                        continue;   // skip already loaded vegetation cells that have type 0/1

                    // spawn vegetation cells only for billboards to get their mesh-tree positions based on all the rules
                    vegetationCellSpawner.cellJobHandleList.Add(vegetationCellSpawner.SpawnVegetationCell(billboardCell.overlapVegetationCells[j], 1, out _, true, false));

                    if (loadedVegetationCellList.Contains(billboardCell.overlapVegetationCells[j]) == false)    // add to loaded vegetation cell list to clear their cache later
                        loadedVegetationCellList.Add(billboardCell.overlapVegetationCells[j]);
                }

                loadedBillboardCellList.Add(billboardCell);
                billboardCell.loadedState = 1;  // set as loaded w/ vegetationCells
            }
            Profiler.EndSample();

            prepareVegetationHandle = JobHandle.CombineDependencies(vegetationCellSpawner.cellJobHandleList.AsArray()); // combine job handles after billboard(vegetation) cell loading
            Profiler.EndSample();

            Profiler.BeginSample("Merge cells and prepare billboard mesh data");
            vegetationCellSpawner.cellJobHandleList.Clear();
            for (int i = 0; i < loadedBillboardCellList.Count; i++)
            {
                BillboardCell billboardCell = loadedBillboardCellList[i];
                if (billboardCell.loadedState != 1)
                    continue;   // skip if not ready w/ needed vegetationCells

                for (int j = 0; j < billboardCell.vegetationPackageBillboardInstanceList.Count; j++)
                    for (int k = 0; k < billboardCell.vegetationPackageBillboardInstanceList[j].billboardInstanceList.Count; k++)
                    {
                        VegetationItemInfoPro vegItemInfoPro = vegetationPackageProList[j].VegetationInfoList[k];
                        BillboardInstance billboardInstance = billboardCell.vegetationPackageBillboardInstanceList[j].billboardInstanceList[k];
                        if (vegItemInfoPro.UseBillboards == false || billboardInstance.loaded)
                            continue;   // skip if not enabled/supported => dispose temporary data later -- skip if already loaded

                        JobHandle prepareBillboardMeshDataHandle = prepareVegetationHandle;
                        for (int l = 0; l < billboardCell.overlapVegetationCells.Count; l++)    // for each vegetation cell that got overlapped -- merge them into one list to create one big merged "billboard-mesh" later
                        {
                            MergeCellInstancesJob mergeCellInstancesJob = new() // merge matrix instance data of all vegetation instances to create total render list
                            {
                                OutputNativeList = billboardInstance.instanceList,
                                InputNativeList = billboardCell.overlapVegetationCells[l].vegetationPackageInstanceList[j].matrixInstanceList[k]
                            };
                            prepareBillboardMeshDataHandle = mergeCellInstancesJob.Schedule(prepareBillboardMeshDataHandle);
                        }

                        BillboardGenerator.PrepareBillboardMeshJob prepareBillboardMeshJob = new()
                        {   // prepare/merge data into one block of data => gets merged into a mesh later
                            InstanceList = billboardInstance.instanceList,  // pass merged matrix instance data of all vegetation instances
                            VertexList = billboardInstance.vertexList,
                            NormalList = billboardInstance.normalList,
                            UV1List = billboardInstance.uv1List,
                            UV2List = billboardInstance.uv2List,
                            UV3List = billboardInstance.uv3List,
                            IndexList = billboardInstance.indexList,
                            VegetationItemBounds = MeshUtility.CalculateBounds(vegItemInfoPro.VegetationPrefab, (int)vegItemInfoPro.BillboardSourceLODLevel)
                        };
                        prepareBillboardMeshDataHandle = prepareBillboardMeshJob.Schedule(prepareBillboardMeshDataHandle);

                        vegetationCellSpawner.cellJobHandleList.Add(prepareBillboardMeshDataHandle);
                    }

                billboardCell.loadedState = 2;  // set as loaded w/ mesh-data
            }

            prepareBillboardsHandle = JobHandle.CombineDependencies(vegetationCellSpawner.cellJobHandleList.AsArray());    // combine job handles after cell load > merge > mesh preparation/creation
            JobHandle.ScheduleBatchedJobs();    // run/prioritize all billboard jobs
            Profiler.EndSample();
            Profiler.EndSample();   // end of "Prepare billboard render list"
        }

        private void RenderBillboards()
        {
            Profiler.BeginSample("Complete billboard cell loading");
            prepareVegetationHandle.Complete(); // finish / synchronize assigned job handles -- wait for matrix data for billboard mesh data generation
            prepareBillboardsHandle.Complete();  // finish / synchronize assigned job handles -- wait for billboard mesh data for billboard mesh creation
            Profiler.EndSample();

            Profiler.BeginSample("Create billboard mesh instances");
            for (int i = 0; i < loadedBillboardCellList.Count; i++)
            {
                BillboardCell billboardCell = loadedBillboardCellList[i];
                if (billboardCell.loadedState != 2)
                    continue;   // skip if not ready w/ needed mesh-data

                for (int j = 0; j < billboardCell.vegetationPackageBillboardInstanceList.Count; j++)
                    for (int k = 0; k < billboardCell.vegetationPackageBillboardInstanceList[j].billboardInstanceList.Count; k++)
                    {
                        VegetationItemInfoPro vegItemInfoPro = vegetationPackageProList[j].VegetationInfoList[k];
                        BillboardInstance billboardInstance = billboardCell.vegetationPackageBillboardInstanceList[j].billboardInstanceList[k];
                        if (billboardInstance.vertexList.Length > 0 && billboardInstance.loaded == false)  // only create meshes when not loaded yet -- when a valid vegetation item/instance
                            billboardInstance.loaded = billboardInstance.mesh = BillboardGenerator.CreateMergedBillboardMesh(billboardInstance, billboardCell, vegetationPackageProList[j].name, vegItemInfoPro.Name);  // set as loaded if mesh got successfully created
                        billboardInstance.CompactMemory();  // dispose of temporary data as not needed anymore -- balance memory -- potential speed up since better memory management
                    }

                billboardCell.loadedState = 3;  // set as loaded w/ final "billboard-cell-mesh"
                billboardCell.overlapVegetationCells.Clear();   // clear list as finished loading
            }
            Profiler.EndSample();

            Profiler.BeginSample("Render billboards");
            bool hasShadow = vegetationRenderSettings.GetBillboardShadowCastingMode() != ShadowCastingMode.Off;
            renderParams.layer = vegetationRenderSettings.GetBillboardLayer();
            renderParams.lightProbeUsage = vegetationRenderSettings.GetBillboardBlendProbeUsage();
            renderParams.reflectionProbeUsage = vegetationRenderSettings.GetBillboardReflectionProbeUsage();

            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // render billboard instances per camera
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false) continue;
                renderParams.camera = Application.isPlaying ? (vegetationStudioCameraList[i].renderDirectToCamera ? vegetationStudioCameraList[i].selectedCamera : null) : null;
                renderBounds.center = vegetationStudioCameraList[i].selectedCamera.transform.position;

                for (int j = 0; j < vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList.Length; j++)   // per visible billboard cell
                    for (int k = 0; k < vegetationStudioCameraList[i].preloadBillboardCellList[vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j]].vegetationPackageBillboardInstanceList.Count; k++)   // per vegetation package
                        for (int l = 0; l < vegetationStudioCameraList[i].preloadBillboardCellList[vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j]].vegetationPackageBillboardInstanceList[k].billboardInstanceList.Count; l++)  // per vegetation instance
                        {
                            BillboardInstance billboardInstance = vegetationStudioCameraList[i].preloadBillboardCellList[vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j]].vegetationPackageBillboardInstanceList[k].billboardInstanceList[l];
                            if (billboardInstance.loaded == false)
                                continue;   // skip if the billboardInstance isn't ready -- when there aren't any / not a tree item type

                            // get/set base data needed for rendering
                            float fadeDistance = vegetationPackageProModelsList[k].vegetationItemModelList[l].vegetationItemInfo.EnableCrossFade && QualitySettings.enableLODCrossFade ? vegetationSettings.crossFadeDistance : 0;
                            float cullDistance = vegetationStudioCameraList[i].renderBillboardsOnly ? 0 : vegetationSettings.GetVegetationItemCullDistance(vegetationPackageProModelsList[k].vegetationItemModelList[l].vegetationItemInfo);
                            float nearCullDistance = math.clamp(cullDistance, 0, cullDistance - (fadeDistance * 0.5f));
                            MaterialPropertyBlock materialPropertyBlock = vegetationPackageProModelsList[k].vegetationItemModelList[l].cameraBillboardMaterialPropertyBlockList[i];
                            materialPropertyBlock.SetFloat(_fadeDistanceID, fadeDistance);
                            materialPropertyBlock.SetFloat(_cullDistanceID, cullDistance);  // based on the tree distance effectively
                            materialPropertyBlock.SetFloat(_nearCullDistanceID, nearCullDistance);
                            materialPropertyBlock.SetFloat(_farCullDistanceID, vegetationSettings.GetBillboardDistance(vegetationStudioCameraList[i].selectedCamera.farClipPlane));

                            // updater render params to match current vegetationItem / LOD level
                            CellCullingInfo cellCullingInfo = vegetationStudioCameraList[i].billboardCullingGroup.cellCullingInfoList[vegetationStudioCameraList[i].billboardCullingGroup.visibleCellIndexList[j]];
                            renderParams.shadowCastingMode = hasShadow && cellCullingInfo.Enabled == 2 ? ShadowCastingMode.ShadowsOnly : vegetationRenderSettings.GetBillboardShadowCastingMode();  // shadow-only mode when it's a "shadow cell"
                            renderParams.matProps = materialPropertyBlock;
                            renderParams.material = vegetationPackageProModelsList[k].vegetationItemModelList[l].billboardMaterial;
                            renderBounds.extents = vegetationSettings.GetBillboardDistance(vegetationStudioCameraList[i].selectedCamera.farClipPlane) * Vector3.one;    // extents used directly here so no x2
                            renderParams.worldBounds = renderBounds;

                            // render billboards
                            Graphics.RenderMesh(renderParams, billboardInstance.mesh, 0, Matrix4x4.TRS(floatingOriginOffset, quaternion.identity, Vector3.one));
                        }
            }
            Profiler.EndSample();
        }

        public void ClearBillboardCellCache()
        {
            for (int i = 0; i < loadedBillboardCellList.Count; i++)
                loadedBillboardCellList[i].ClearCache();
            loadedBillboardCellList.Clear();
        }

        private void ClearBillboardCellCache(int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            for (int i = 0; i < loadedBillboardCellList.Count; i++)
                loadedBillboardCellList[i].ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            loadedBillboardCellList.Clear();
        }

        private void ClearBillboardCellCache(Bounds _bounds)
        {
            if (billboardCellQuadTree == null)
                return;

            Rect clearRect = RectExtension.CreateRectFromBounds(_bounds);
            List<BillboardCell> overlapBillboardCellList = new();

            billboardCellQuadTree.Query(clearRect, overlapBillboardCellList);
            for (int i = 0; i < overlapBillboardCellList.Count; i++)
            {
                overlapBillboardCellList[i].ClearCache();
                loadedBillboardCellList.RemoveSwapBack(overlapBillboardCellList[i]);
            }
        }

        private void ClearBillboardCellCache(Bounds _bounds, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (billboardCellQuadTree == null)
                return;

            Rect clearRect = RectExtension.CreateRectFromBounds(_bounds);
            List<BillboardCell> overlapBillboardCellList = new();

            billboardCellQuadTree.Query(clearRect, overlapBillboardCellList);
            for (int i = 0; i < overlapBillboardCellList.Count; i++)
            {
                overlapBillboardCellList[i].ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
                loadedBillboardCellList.RemoveSwapBack(overlapBillboardCellList[i]);
            }
        }

        private void DisposeBillboardCells()
        {
            for (int i = 0; i < billboardCellList.Count; i++)
                billboardCellList[i].Dispose();
            billboardCellList.Clear();

            predictiveBillboardCellLoaderList.Clear();
            loadedBillboardCellList.Clear();
        }
    }
}