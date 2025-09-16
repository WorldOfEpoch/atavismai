using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Culling;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationStudio;
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
        private void CreateVegetationCells()
        {
            //DisposeVegetationCells();

            // cell creation -- quad tree setup
            Rect vegetationSystemRect = RectExtension.CreateRectFromBounds(new(vegetationSystemBounds.center, vegetationSystemBounds.size + 2 * vegetationCellSize * Vector3.one)); // total area + safety "edge case" offset
            vegetationCellQuadTree = new QuadTree<VegetationCell>(vegetationSystemRect);    // setup the quadTree to use for "query" operations later ex: "ClearCache(Bounds _bounds)"
            float2 cellCorner = new(vegetationSystemBounds.min.x, vegetationSystemBounds.min.z);    // use each (bottom left) cellCorner as the "startPosition" anchor

            for (int x = 0; x < (int)math.ceil(vegetationSystemBounds.size.x / vegetationCellSize); x++)    // for each cell on the X-Axis
                for (int z = 0; z < (int)math.ceil(vegetationSystemBounds.size.z / vegetationCellSize); z++)    // for each cell on the Z-Axis
                {   // create a new vegetationCell -- position it based on the current XZ count times the size + cellCorner offset to anchor it -- give it it's XZ bounds => Y bounds sampled later
                    VegetationCell vegetationCell = new(new Rect(new float2(vegetationCellSize * x + cellCorner.x, vegetationCellSize * z + cellCorner.y), new float2(vegetationCellSize, vegetationCellSize)));
                    vegetationCellList.Add(vegetationCell); // add to the list for indexing
                    vegetationCell.index = vegetationCellList.Count - 1;    // assign an index for specific access ex: persistent storage
                    vegetationCellQuadTree.Insert(vegetationCell);  // insert into the quadTree for querying
                }

            loadedVegetationCellList.Clear();   // safety clear
            loadedVegetationCellList.Capacity = vegetationCellList.Count;   // set capacity directly to max possible -- avoid increasing it in single steps
            //  --

            // height sampling -- sea level cutoff
            NativeArray<Bounds> vegetationCellBounds = new(vegetationCellList.Count, Allocator.TempJob);
            for (int i = 0; i < vegetationCellList.Count; i++)
                vegetationCellBounds[i] = vegetationCellList[i].cellBounds; // store bounds into a temporary "nativeArray" to use it in a job

            JobHandle handle = default;
            for (int i = 0; i < vegetationStudioTerrainList.Count; i++) // per terrain, get actual Y-Axis bounds of the cells (for cell culling) -- enable cells that are within the bounds ..and (partially) above the "seaLevel"
                handle = vegetationStudioTerrainList[i].SampleCellHeight(vegetationCellBounds, excludeSeaLevelCells ? systemRelativeSeaLevel = vegetationSystemBounds.min.y + SeaLevel : vegetationSystemBounds.min.y, vegetationSystemRect, handle);
            JobHandle.ScheduleBatchedJobs();    // run/prioritize all terrains' sample height jobs
            handle.Complete();  // finish / synchronize assigned job handles  

            for (int i = 0; i < vegetationCellList.Count; i++)
                vegetationCellList[i].cellBounds = vegetationCellBounds[i]; // write back from the temporary "nativeArray"
            vegetationCellBounds.Dispose(); // dispose the temporary "nativeArray"
            //  --

            PrepareAllVegetationCells();    // prepare all vegetationCells -- create and enlarge lists to store matrices and states => get filled later in the actual "cellSpawning"

            VegetationStudioManager.OnVegetationCellRefresh(this);  // notify vegetationManager -- let masks register into this vegetationSystem
        }

        private void PrepareAllVegetationCells()
        {
            for (int i = 0; i < vegetationCellList.Count; i++)
                vegetationCellList[i].PrepareVegetationCell(vegetationPackageProList);
        }

        private void LoadVegetationCells()
        {
            Profiler.BeginSample("Vegetation cell loading jobs");

            systemRelativeSeaLevel = vegetationSystemBounds.min.y + SeaLevel;   // set sea level for cell loading(exclusion)
            vegetationCellSpawner.cellJobHandleList.Clear();

            Profiler.BeginSample("Vegetation cell (pre-)loading");
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // vegetation cell loading per camera w/ frustum culling
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false || vegetationStudioCameraList[i].renderBillboardsOnly)
                    continue;

                for (int j = 0; j < vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList.Length; j++)
                {
                    VegetationCell vegetationCell = vegetationStudioCameraList[i].preloadVegetationCellList[vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList[j]];
                    CellCullingInfo cellCullingInfo = vegetationStudioCameraList[i].GetCellCullingInfo(vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList[j]);
                    if (vegetationCell.loadedDistanceBand <= cellCullingInfo.CurrentDistanceBand)   // is the cell not empty -- has the cell a valid distance band vs what's visible -- far cell -> close cell re-generation
                        continue;   // don't load an already correctly loaded cell again

                    // run rules -- prepare matrix instances -- load from persistent storage
                    vegetationCellSpawner.cellJobHandleList.Add(vegetationCellSpawner.SpawnVegetationCell(vegetationCell, cellCullingInfo.CurrentDistanceBand, out bool hasInstancedIndirect, false, false));
                    OnVegetationCellLoaded?.Invoke(vegetationCell); // notify collider system / runtime prefab spawner

                    if (loadedVegetationCellList.Contains(vegetationCell) == false) // add to loaded vegetation cell list to clear their cache later
                        loadedVegetationCellList.Add(vegetationCell);

                    // if indirect cell add to the list to only for those cells prepare further data for indirect rendering
                    if (hasInstancedIndirect && instancedIndirectCellList.Contains(vegetationCell) == false)
                        instancedIndirectCellList.Add(vegetationCell);

                    if (predictiveCellLoader.preloadVegetationCellList.Count > 0 && vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList[j] < predictiveCellLoader.preloadVegetationCellList.Count)    // empty when "PredictivePreloading" is disabled
                        predictiveCellLoader.preloadVegetationCellList.RemoveAtSwapBack(vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList[j]);  // remove early to avoid trying to re-load a cell thus avoid "load misses"
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Vegetation cell predictive preloading");
            predictiveVegetationCellLoaderList.Clear();
            predictiveCellLoader.GetAllVegetationCellsForThisFrame(predictiveVegetationCellLoaderList); // check if "PredictivePreloading" is enabled and get available cells
            for (int i = 0; i < predictiveVegetationCellLoaderList.Count; i++)  // "predictive pre-load" all additional cells available if any
            {
                VegetationCell vegetationCell = predictiveVegetationCellLoaderList[i];
                //if (vegetationCell.loadedDistanceBand != 99)    // is the cell not empty -- has the cell's cache been cleared
                //continue;   // don't load an already correctly loaded cell again

                if (predictiveCellLoader.GetPredictiveDistanceBandType(out int distanceBand) == false)  // get distanceBand type that should be "predictive preloaded"
                    continue;   // skip if no vegetation/distanceBand type should be "predictive preloaded"

                // run rules -- prepare matrix instances -- load from persistent storage
                vegetationCellSpawner.cellJobHandleList.Add(vegetationCellSpawner.SpawnVegetationCell(vegetationCell, distanceBand, out bool hasInstancedIndirect, false, true));
                OnVegetationCellLoaded?.Invoke(vegetationCell, true);   // notify custom registered functions

                if (loadedVegetationCellList.Contains(vegetationCell) == false) // add to loaded vegetation cell list to clear their cache later
                    loadedVegetationCellList.Add(vegetationCell);

                // if indirect cell add to the list to only for those cells prepare further data for indirect rendering
                if (hasInstancedIndirect && instancedIndirectCellList.Contains(vegetationCell) == false)
                    instancedIndirectCellList.Add(vegetationCell);
            }
            Profiler.EndSample();

            prepareVegetationHandle = JobHandle.CombineDependencies(vegetationCellSpawner.cellJobHandleList.AsArray()); // combine job handles after "predictive pre-load" -> load
            //JobHandle.ScheduleBatchedJobs();    // run all (combined) job handles
            Profiler.EndSample();
        }

        private void PrepareCPURenderList()
        {
            Profiler.BeginSample("Prepare render list -- CPU");
            vegetationCellSpawner.cellJobHandleList.Clear();
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // CPU frustum culling per camera
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false || vegetationStudioCameraList[i].renderBillboardsOnly)
                    continue;

                for (int j = 0; j < vegetationPackageProList.Count; j++)
                    for (int k = 0; k < vegetationPackageProList[j].VegetationInfoList.Count; k++)
                    {
                        VegetationItemModelInfo vegItemModelInfo = vegetationPackageProModelsList[j].vegetationItemModelList[k];
                        if (vegItemModelInfo.vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.InstancedIndirect && vegetationRenderSettings.UseInstancedIndirect())
                            continue;   // fall back to instanced when not in play mode and indirect rendering is selected -- when platform is marked as not supported

                        NativeList<MatrixInstance> mergedMatrixInstanceList = vegetationStudioCameraList[i].cameraMatrixLists[j].mergedMatrixInstanceList[k];
                        mergedMatrixInstanceList.Clear();

                        Profiler.BeginSample("Merge cell job");
                        JobHandle vegetationItemMergeJobHandle = prepareVegetationHandle;
                        for (int l = 0; l < vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList.Length; l++)  // merge vegetation cells per camera for visible cells
                        {
                            VegetationCell vegetationCell = vegetationStudioCameraList[i].preloadVegetationCellList[vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList[l]];
                            CellCullingInfo cellCullingInfo = vegetationStudioCameraList[i].GetCellCullingInfo(vegetationStudioCameraList[i].vegetationCullingGroup.visibleCellIndexList[l]);
                            if (cellCullingInfo.CurrentDistanceBand > vegItemModelInfo.distanceBand)    // vegItemModelInfo!!
                                continue;   // only merge same cell distance band types

                            MergeCellInstancesJob mergeCellInstancesJob = new() // merge matrix instance data of all vegetation instances to create total render list
                            {
                                OutputNativeList = mergedMatrixInstanceList,
                                InputNativeList = vegetationCell.vegetationPackageInstanceList[j].matrixInstanceList[k]
                            };

                            Profiler.BeginSample("Schedule merge cell job");
                            vegetationItemMergeJobHandle = mergeCellInstancesJob.Schedule(vegetationItemMergeJobHandle);
                            Profiler.EndSample();
                        }
                        Profiler.EndSample();

                        Profiler.BeginSample("Prepare vegetation cull/lod job");
                        // prepare/set data for rendering / (frustum) culling and LOD calculation
                        vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD0[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD1[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD2[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD3[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD0[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD1[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD2[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD3[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD0[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD1[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD2[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD3[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD0[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD1[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD2[k].Clear();
                        vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD3[k].Clear();

                        CPUFrustumCullingLODJob frustumLODJob = new()
                        {
                            FrustumPlanes = vegetationStudioCameraList[i].vegetationCullingGroup.frustumPlanes,
                            MergedMatrixInstanceList = mergedMatrixInstanceList,    // pass merged matrix instance data of all vegetation instances
                            MatrixListLOD0 = vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD0[k],
                            MatrixListLOD1 = vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD1[k],
                            MatrixListLOD2 = vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD2[k],
                            MatrixListLOD3 = vegetationStudioCameraList[i].cameraMatrixLists[j].matrixListLOD3[k],
                            FadeListLOD0 = vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD0[k],
                            FadeListLOD1 = vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD1[k],
                            FadeListLOD2 = vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD2[k],
                            FadeListLOD3 = vegetationStudioCameraList[i].cameraMatrixLists[j].fadeListLOD3[k],
                            ShadowMatrixListLOD0 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD0[k],
                            ShadowMatrixListLOD1 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD1[k],
                            ShadowMatrixListLOD2 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD2[k],
                            ShadowMatrixListLOD3 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowMatrixListLOD3[k],
                            ShadowFadeListLOD0 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD0[k],
                            ShadowFadeListLOD1 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD1[k],
                            ShadowFadeListLOD2 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD2[k],
                            ShadowFadeListLOD3 = vegetationStudioCameraList[i].cameraMatrixLists[j].shadowFadeListLOD3[k],
                            CullDistance = vegetationSettings.GetVegetationItemCullDistance(vegItemModelInfo.vegetationItemInfo),
                            CameraPosition = vegetationStudioCameraList[i].selectedCamera.transform.position,
                            FloatingOriginOffset = floatingOriginOffset,
                            NoFrustumCulling = vegetationStudioCameraList[i].eCameraCullingMode == ECameraCullingMode.Complete360,
                            HasBackShadow = vegItemModelInfo.distanceBand == 1, // only largeObjects/trees use shadow frustum culling
                            LightDirection = sunMoonDirection,
                            ItemBoundsCenter = vegItemModelInfo.vegetationItemInfo.Bounds.center,
                            ItemBoundsExtents = vegItemModelInfo.vegetationItemInfo.Bounds.extents,
                            UseLODFade = vegItemModelInfo.vegetationItemInfo.EnableCrossFade && QualitySettings.enableLODCrossFade,
                            LODCount = vegItemModelInfo.lodCount,
                            MaxLODIndex = vegItemModelInfo.maxLODIndex,
                            MaxLOD0 = vegItemModelInfo.maxLOD0Index,
                            MaxLOD1 = vegItemModelInfo.maxLOD1Index,
                            MaxLOD2 = vegItemModelInfo.maxLOD2Index,
                            MaxLOD3 = vegItemModelInfo.maxLOD3Index,
                            ShadowLODIndex = vegItemModelInfo.vegetationItemInfo.DisableShadows || !hasSunMoon ? -1 : vegetationRenderSettings.GetShadowCastingMode(vegItemModelInfo.vegetationItemInfo.VegetationType),    // -1 = disabled
                            CustomShadowLODIndex = math.clamp(vegItemModelInfo.vegetationSystemPro.vegetationRenderSettings.GetCustomShadowLODIndex(vegItemModelInfo.vegetationItemInfo.VegetationType), 0, vegItemModelInfo.maxLODIndex),
                            LODFactor = vegItemModelInfo.vegetationItemInfo.LODFactor,
                            LODBias = QualitySettings.lodBias,
                            LODFadeDistance = vegItemModelInfo.vegetationItemInfo.EnableCrossFade && QualitySettings.enableLODCrossFade ? vegetationSettings.crossFadeDistance : 0,
                            ItemLod0To1Distance = vegItemModelInfo.lod0To1Distance,
                            ItemLod1To2Distance = vegItemModelInfo.lod1To2Distance,
                            ItemLod2To3Distance = vegItemModelInfo.lod2To3Distance
                        };

                        Profiler.BeginSample("Schedule vegetation cull/lod job");
                        vegetationItemMergeJobHandle = frustumLODJob.Schedule(vegetationItemMergeJobHandle);
                        Profiler.EndSample();
                        vegetationCellSpawner.cellJobHandleList.Add(vegetationItemMergeJobHandle);
                        Profiler.EndSample();   // end of "Prepare vegetation cull/lod job"
                    }
            }

            if (vegetationCellSpawner.cellJobHandleList.Length > 0) // safety filter when using "Instanced indirect" -- avoid overwriting the "prepareVegetationHandle" with "nothing" => avoid not completing the correct jobs
                prepareVegetationHandle = JobHandle.CombineDependencies(vegetationCellSpawner.cellJobHandleList.AsArray()); // combine job handles after cell merge -> cull/lod job
            JobHandle.ScheduleBatchedJobs();    // run/prioritize all (combined) CPU render list jobs
            Profiler.EndSample();   // end of "Prepare CPU render list -- Instanced/Normal"
        }

        private void RenderVegetation()
        {
            UpdateWindControllers();   // update/apply wind controllers/settings => through the "IWindController" interface

            if (vegetationRenderSettings.UseInstancedIndirect())
            {   // set compute shader render values
                PrepareGraphicsBuffers();   // load indirect cells -- create graphics buffers if not done yet (per indirect cell > package > item) -- clear redundant non GPU based data
                frustumMatrixShader.SetFloat(_lodBias, QualitySettings.lodBias);
                frustumMatrixShader.SetVector(_lightDirection, (Vector3)sunMoonDirection);
                frustumMatrixShader.SetVector(_floatingOriginOffsetID, (Vector3)floatingOriginOffset);
            }

            Profiler.BeginSample("Render vegetation instances");
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // render vegetation instances per camera
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false || vegetationStudioCameraList[i].renderBillboardsOnly)
                    continue;

                if (vegetationRenderSettings.UseInstancedIndirect())
                {
                    // set compute shader render values, per camera
                    SetGPUFrustumPlanes(vegetationStudioCameraList[i].vegetationCullingGroup);
                    frustumMatrixShader.SetBool(_noFrustumCullingID, vegetationStudioCameraList[i].eCameraCullingMode == ECameraCullingMode.Complete360);
                }

#if USING_HDRP && UNITY_2023_1_OR_NEWER
                bool isRTASRequired = false;    // whether at least one vegetation item exists with the "rayTraced" rendering mode
                vegetationStudioCameraList[i].hdCamera?.rayTracingAccelerationStructure?.ClearInstances();  // clear existing data from last frame
#endif
                // set rendering properties, per camera
                UpdateSpeedTreeWindBridge(i);
                renderParams.camera = vegetationStudioCameraList[i].renderDirectToCamera ? vegetationStudioCameraList[i].selectedCamera : null;
                renderBounds.center = vegetationStudioCameraList[i].selectedCamera.transform.position;

                for (int j = 0; j < vegetationPackageProList.Count; j++)    // per vegetation package
                    for (int k = 0; k < vegetationPackageProList[j].VegetationInfoList.Count; k++)  // per vegetation item
                    {
                        VegetationItemModelInfo vegItemModelInfo = vegetationPackageProModelsList[j].vegetationItemModelList[k];

                        // set rendering properties, per vegetation item
                        vegItemDistances.x = vegetationSettings.crossFadeDistance + math.length(vegItemModelInfo.cullingBoundsAddy.extents);    // base distance
                        vegItemDistances.y = vegetationSettings.GetVegetationItemCullDistance(vegItemModelInfo.vegetationItemInfo); // per vegetation item type (culling) distance
                        int shadowLodIndex = vegetationRenderSettings.GetShadowCastingMode(vegItemModelInfo.vegetationItemInfo.VegetationType); // get/set shadow LOD distance limitation
                        renderParams.shadowCastingMode = ShadowCastingMode.Off; // don't render shadows for objects -- render shadows later separately
                        renderParams.layer = vegetationRenderSettings.GetLayer(vegItemModelInfo.vegetationItemInfo.VegetationType); // set layer
                        renderParams.lightProbeUsage = vegetationRenderSettings.GetBlendProbeUsage(vegItemModelInfo.vegetationItemInfo.VegetationType); // set light probe usage(blend probes)
                        renderParams.reflectionProbeUsage = vegetationRenderSettings.GetReflectionProbeUsage(vegItemModelInfo.vegetationItemInfo.VegetationType);   // set reflection probe usage
#if USING_HDRP && UNITY_2023_1_OR_NEWER
                        isRTASRequired = (vegItemModelInfo.vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.RayTraced && isRTASRequired == false) || isRTASRequired == true; // at least one item requires the RTAS to be setup
                        rtInstanceConfig.layer = vegetationRenderSettings.GetLayer(vegItemModelInfo.vegetationItemInfo.VegetationType); // set rt layer
#endif

                        if (vegItemModelInfo.vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.InstancedIndirect && vegetationRenderSettings.UseInstancedIndirect())
                        {
                            Profiler.BeginSample("Render vegetation -- GPU");   // render vegetation instances using the GPU indirectly
                            PrepareGPURenderList(vegItemModelInfo, i, j, k, shadowLodIndex);    // prepare(here) > render(internally)
                            Profiler.EndSample();
                        }
                        else
                        {
                            Profiler.BeginSample("Render vegetation -- CPU");   // render vegetation instances using the CPU directly
                            RenderCPUVegetation(vegItemModelInfo, i, j, k, shadowLodIndex); // prepare(done earlier in update) > render(here)
                            Profiler.EndSample();
                        }
                    }

#if USING_HDRP && UNITY_2023_1_OR_NEWER
                Profiler.BeginSample("Setup RTAS -- CPU DXR");
                if (Application.isPlaying && isRTASRequired && SystemInfo.supportsRayTracing)
                    SetupRTAS(i);   // setup RTAS -- cull scene gameObjects for building/rendering
                else
                    DisposeRTAS(i); // dispose of redundant data
                Profiler.EndSample();

                Profiler.BeginSample("Build RTAS -- CPU DXR");
                vegetationStudioCameraList[i].hdCamera?.rayTracingAccelerationStructure?.Build(vegetationStudioCameraList[i].selectedCamera.transform.position); // build the RTAS -- combine/sort/prepare data from the "rendering" pass
                Profiler.EndSample();
#endif
            }
            Profiler.EndSample();
        }

        private void RenderCPUVegetation(VegetationItemModelInfo _vegItemModelInfo, int _cameraIndex, int _packageIndex, int _itemIndex, int _shadowLodIndex)
        {
            // render visible non shadow objects that are only within the view frustum (by default)
            RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD0Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
            if (_vegItemModelInfo.lodCount > 1)  // has 2 LOD levels
                RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD1Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
            if (_vegItemModelInfo.lodCount > 2)  // has 3 LOD levels
                RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD2Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
            if (_vegItemModelInfo.lodCount > 3)  // has 4 LOD levels
                RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD3Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);

            // since we are doing (separate) shadow culling we render them separately to not draw shadows twice
            if (_shadowLodIndex == -1 || _vegItemModelInfo.vegetationItemInfo.DisableShadows) return;   // skip when not supposed to render shadows
            renderParams.shadowCastingMode = ShadowCastingMode.ShadowsOnly; // set to render shadows only

            RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD0Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
            if (_vegItemModelInfo.lodCount < 2) return; // has not 2 LOD levels
            RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD1Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
            if (_vegItemModelInfo.lodCount < 3) return; // has not 3 LOD levels
            RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD2Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
            if (_vegItemModelInfo.lodCount < 4) return; // has not 4 LOD levels
            RenderVegetationItemDirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD3Index, _cameraIndex, vegetationStudioCameraList[_cameraIndex].cameraMatrixLists[_packageIndex], _itemIndex);
        }

        private Bounds CalculateWorldBounds(VegetationItemModelInfo _vegItemModelInfo, int _lodIndex)
        {
            if (_lodIndex < 3)  // calculate/adjust per LOD distances for "worldBounds"
            {   // get lodDistance per LOD level
                float lodDistance = _lodIndex == 0 ? _vegItemModelInfo.lod0To1Distance : _lodIndex == 1 ? _vegItemModelInfo.lod1To2Distance : _vegItemModelInfo.lod2To3Distance;
                if (lodDistance == -1)  // if no LODs
                    lodDistance = vegItemDistances.y;   // set to culling distance
                lodDistance = math.clamp(lodDistance * _vegItemModelInfo.vegetationItemInfo.LODFactor * QualitySettings.lodBias, 0, vegItemDistances.y);    // clamp lodDistance and calculate the end result w/ biases

                if ((_lodIndex == 0 && _vegItemModelInfo.lodCount == 1) || (_lodIndex == 1 && _vegItemModelInfo.lodCount == 2) || (_lodIndex == 2 && _vegItemModelInfo.lodCount == 3))
                    lodDistance = math.max(lodDistance, vegItemDistances.y);    // "last" lodDistance -- extend to be the culling distance for objects with less than four LODs

                renderBounds.extents = (vegItemDistances.x + lodDistance) * Vector3.one;    // extents used directly here so no x2
            }
            else
                renderBounds.extents = (vegItemDistances.x + vegItemDistances.y) * Vector3.one; // extents used directly here so no x2

            return renderBounds;
        }

        private void RenderVegetationItemDirect(VegetationItemModelInfo _vegItemModelInfo, int _lodIndex, int _cameraIndex, CameraMatrixLists _matrixLists, int _itemIndex)
        {
            // get matrixLists
            bool isShadow = renderParams.shadowCastingMode == ShadowCastingMode.ShadowsOnly;
            NativeList<Matrix4x4> _matrixList =
                _lodIndex == 0 ? isShadow ? _matrixLists.shadowMatrixListLOD0[_itemIndex] : _matrixLists.matrixListLOD0[_itemIndex] :
                _lodIndex == 1 ? isShadow ? _matrixLists.shadowMatrixListLOD1[_itemIndex] : _matrixLists.matrixListLOD1[_itemIndex] :
                _lodIndex == 2 ? isShadow ? _matrixLists.shadowMatrixListLOD2[_itemIndex] : _matrixLists.matrixListLOD2[_itemIndex] :
                isShadow ? _matrixLists.shadowMatrixListLOD3[_itemIndex] : _matrixLists.matrixListLOD3[_itemIndex];
            if (_matrixList.Length == 0) return; // skip when the vegetation item isn't used at run-time or in a persistent storage
            NativeList<Vector4> _lodFadeList =
                _lodIndex == 0 ? isShadow ? _matrixLists.shadowFadeListLOD0[_itemIndex] : _matrixLists.fadeListLOD0[_itemIndex] :
                _lodIndex == 1 ? isShadow ? _matrixLists.shadowFadeListLOD1[_itemIndex] : _matrixLists.fadeListLOD1[_itemIndex] :
                _lodIndex == 2 ? isShadow ? _matrixLists.shadowFadeListLOD2[_itemIndex] : _matrixLists.fadeListLOD2[_itemIndex] :
                isShadow ? _matrixLists.shadowFadeListLOD3[_itemIndex] : _matrixLists.fadeListLOD3[_itemIndex];

            // get/set base data needed for rendering
            Mesh mesh = _vegItemModelInfo.GetMeshAtIndex(_lodIndex);
            Material[] materials = _vegItemModelInfo.GetMaterialsAtIndex(_lodIndex);
            MaterialPropertyBlock materialPropertyBlock = _vegItemModelInfo.GetMPBAtIndex(_lodIndex);
            materialPropertyBlock.Clear();

            for (int i = 0; i < _vegItemModelInfo.shaderControllers?.Length; i++)   // "SpeedTree WindBridge"
                if (_vegItemModelInfo.shaderControllers[i] != null && _vegItemModelInfo.shaderControllers[i].Settings != null && _vegItemModelInfo.shaderControllers[i].Settings.isSpeedTree)
                {
                    MeshRenderer meshRenderer = _vegItemModelInfo.speedTreeWindBridgeMeshRendererList[_cameraIndex];
                    if (meshRenderer) meshRenderer.GetPropertyBlock(materialPropertyBlock); // pass this vegetation item's "MPB" to the engine so it can write the usual "SpeedTree" wind data
                }

#if USING_HDRP && UNITY_2023_1_OR_NEWER
            if (Application.isPlaying && _vegItemModelInfo.vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.RayTraced && SystemInfo.supportsRayTracing)
            {
                if (_vegItemModelInfo.vegetationItemInfo.VegetationType == VegetationType.Grass || _vegItemModelInfo.vegetationItemInfo.VegetationType == VegetationType.Plant || _vegItemModelInfo.vegetationItemInfo.VegetationType == VegetationType.Tree)
                {
                    rtInstanceConfig.enableTriangleCulling = false;
                    rtInstanceConfig.subMeshFlags = RayTracingSubMeshFlags.Enabled | RayTracingSubMeshFlags.UniqueAnyHitCalls;
                }
                else
                {
                    rtInstanceConfig.enableTriangleCulling = true;
                    rtInstanceConfig.subMeshFlags = RayTracingSubMeshFlags.Enabled | RayTracingSubMeshFlags.ClosestHitOnly;
                }

                rtInstanceConfig.mesh = mesh;
                rtInstanceConfig.materialProperties = materialPropertyBlock;
            }
            else
#endif
            {
                // updater render params to match current vegetationItem / LOD level
                renderParams.matProps = materialPropertyBlock;
                renderParams.worldBounds = CalculateWorldBounds(_vegItemModelInfo, _lodIndex);
            }

            // render vegetation instances using the correct mode, per vegetation item
            if (_vegItemModelInfo.vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.Normal)
                for (int i = 0; i < _matrixList.Length; i++)    // render for totalInstanceCount, per entry only
                    for (int j = 0; j < math.min(mesh.subMeshCount, materials.Length); j++) // render per (sub) mesh w/ correct material/-s, RPs, matrices -- w/ safety filter based on materials present
                    {
                        renderParams.material = materials[j];
                        Graphics.RenderMesh(renderParams, mesh, j, _matrixList[i]);
                    }
            else
            {
                int maxSize = 1023; // max size to render per call
                int totalInstanceCount = _matrixList.Length;    // copy over totalInstanceCount
                for (int i = 0; i < (int)math.ceil(_matrixList.Length / (float)maxSize); i++)   // split rendering over only X calls based on totalInstanceCount / maxSize
                {
                    int copyCount = maxSize;    // initialize w/ maxSize
                    if (totalInstanceCount < maxSize)   // adjust to fit
                        copyCount = totalInstanceCount;

                    NativeSlice<Matrix4x4> matrixSlice = new(_matrixList.AsArray(), i * maxSize, copyCount);
                    matrixSlice.CopyToFast(renderingArray); // extract data needed for this call only

                    if (_lodFadeList.Length == _matrixList.Length)
                    {
                        NativeSlice<Vector4> lodFadeSlice = new(_lodFadeList.AsArray(), i * maxSize, copyCount);
                        lodFadeSlice.CopyToFast(renderingLodFadeArray); // extract data needed for this call only
                        materialPropertyBlock.SetVectorArray(_unityLODFadeID, renderingLodFadeArray);   // pass to engine's fade array
                    }

                    materialPropertyBlock.SetFloatArray(_unityRenderingLayerID, renderingLayerArray);   // pass to engine's rendering layer

                    for (int j = 0; j < math.min(mesh.subMeshCount, materials.Length); j++) // render per (sub) mesh w/ correct material/-s, RPs, matrices, instanceCount -- w/ safety filter based on materials present
                    {
#if USING_HDRP && UNITY_2023_1_OR_NEWER
                        if (Application.isPlaying && _vegItemModelInfo.vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.RayTraced && SystemInfo.supportsRayTracing)
                        {
                            rtInstanceConfig.material = materials[j];
                            rtInstanceConfig.subMeshIndex = (uint)j;
                            vegetationStudioCameraList[_cameraIndex].hdCamera?.rayTracingAccelerationStructure?.AddInstances(rtInstanceConfig, renderingArray, copyCount);
                        }
                        else
#endif
                        {
                            //renderParams.material = materials[j];
                            //Graphics.RenderMeshInstanced(renderParams, mesh, j, renderingArray, copyCount);

                            // fallback for now as new method has bugs and worse performance
                            Graphics.DrawMeshInstanced(mesh, j, materials[j], renderingArray, copyCount, renderParams.matProps, renderParams.shadowCastingMode, renderParams.receiveShadows, renderParams.layer, renderParams.camera, renderParams.lightProbeUsage);
                        }
                    }

                    totalInstanceCount -= maxSize;  // substract used data -- only use rest for the next call
                }
            }
        }

        public void ClearCache()
        {
            CompleteVegetationCellLoading();

            for (int i = 0; i < loadedVegetationCellList.Count; i++)
                loadedVegetationCellList[i].ClearCache();
            loadedVegetationCellList.Clear();

            ClearBillboardCellCache();

            OnClearCacheDelegate?.Invoke(this);
        }

        public void ClearCache(Bounds _bounds)
        {
            CompleteVegetationCellLoading();

            Rect clearRect = RectExtension.CreateRectFromBounds(_bounds);
            List<VegetationCell> overlapBillboardCellList = new();
            vegetationCellQuadTree.Query(clearRect, overlapBillboardCellList);
            for (int i = 0; i < overlapBillboardCellList.Count; i++)
            {
                overlapBillboardCellList[i].ClearCache();
                loadedVegetationCellList.RemoveSwapBack(overlapBillboardCellList[i]);
                OnClearCacheVegetationCellDelegate?.Invoke(this, overlapBillboardCellList[i]);
            }

            ClearBillboardCellCache(_bounds);
        }

        public void ClearCache(VegetationCell _vegetationCell)
        {
            CompleteVegetationCellLoading();
            _vegetationCell.ClearCache();
            loadedVegetationCellList.RemoveSwapBack(_vegetationCell);
            ClearBillboardCellCache(_vegetationCell.cellBounds);
            OnClearCacheVegetationCellDelegate?.Invoke(this, _vegetationCell);  // notify collider system / runtime prefab spawner
        }

        public void ClearCache(VegetationCell _vegetationCell, string _vegetationItemID)
        {
            VegetationItemIndices indices = GetVegetationItemIndices(_vegetationItemID);
            ClearCache(_vegetationCell, indices.vegetationPackageIndex, indices.vegetationItemIndex);
        }

        public void ClearCache(VegetationCell _vegetationCell, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (_vegetationPackageIndex == -1 || _vegetationItemIndex == -1)
                return;

            CompleteVegetationCellLoading();

            _vegetationCell.ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            ClearBillboardCellCache(_vegetationCell.cellBounds, _vegetationPackageIndex, _vegetationItemIndex);

            OnClearCacheVegetationCellVegetatonItemDelegate?.Invoke(this, _vegetationCell, _vegetationPackageIndex, _vegetationItemIndex);  // notify collider system / runtime prefab spawner
        }

        public void ClearCache(VegetationPackagePro _vegetationPackage)
        {
            for (int i = 0; i < _vegetationPackage.VegetationInfoList.Count; i++)
                ClearCache(_vegetationPackage.VegetationInfoList[i].VegetationItemID);
        }

        public void ClearCache(string _vegetationItemID)
        {
            VegetationItemIndices indices = GetVegetationItemIndices(_vegetationItemID);
            ClearCache(indices.vegetationPackageIndex, indices.vegetationItemIndex);
        }

        public void ClearCache(int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (_vegetationPackageIndex == -1 || _vegetationItemIndex == -1)
                return;

            CompleteVegetationCellLoading();

            for (int i = 0; i < loadedVegetationCellList.Count; i++)
                loadedVegetationCellList[i].ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
            ClearBillboardCellCache(_vegetationPackageIndex, _vegetationItemIndex);

            OnClearCacheVegetationItemDelegate?.Invoke(this, _vegetationPackageIndex, _vegetationItemIndex);    // notify collider system / runtime prefab spawner
        }

        private void DisposeVegetationCells()
        {
            CompleteVegetationCellLoading();

            for (int i = 0; i < vegetationCellList.Count; i++)
                vegetationCellList[i].Dispose();
            vegetationCellList.Clear();

            predictiveVegetationCellLoaderList.Clear();
            loadedVegetationCellList.Clear();
            instancedIndirectCellList.Clear();
            compactMemoryCellList.Clear();

            if (persistentVegetationStorage)
                persistentVegetationStorage.Dispose();
        }

        private void ReturnTemporaryVegetationCellMemory()  // general object pooling return for vegetation cells after they got loaded for the usual rendering
        {
            for (int i = 0; i < compactMemoryCellList.Count; i++)   // for each vegetation cell that finished loading all the rules -- that finished turning the rules into matrix instances for the rendering
            {
                for (int j = 0; j < compactMemoryCellList[i].vegetationInstanceDataList.Count; j++) // for each instance we got from the instance pool
                    vegetationCellSpawner.vegetationInstanceDataPool.ReturnObject(compactMemoryCellList[i].vegetationInstanceDataList[j]);  // return the object, make it ready for pooled re-use logic  
                compactMemoryCellList[i].vegetationInstanceDataList.Clear();    // clear the list to not iterate again

                for (int j = 0; j < compactMemoryCellList[i].vegetationPackageInstanceList.Count; j++)
                    for (int k = 0; k < compactMemoryCellList[i].vegetationPackageInstanceList[j].matrixInstanceList.Count; k++)
                    {
                        if (vegetationPackageProModelsList[j].vegetationItemModelList[k].vegetationItemInfo.UseBillboards && predictiveCellLoader.ValidatePredictiveVegetationType(vegetationPackageProModelsList[j].vegetationItemModelList[k].vegetationItemInfo.VegetationType)
                            || vegetationPackageProModelsList[j].vegetationItemModelList[k].vegetationItemInfo.ColliderType != ColliderType.Disabled
                            || vegetationPackageProModelsList[j].vegetationItemModelList[k].vegetationItemInfo.RuntimePrefabRuleList.Count > 0
                            || compactMemoryCellList[i].vegetationPackageInstanceList[j].graphicsBufferList[k] == null) // skip items that didn't get their data copied into GPU based buffers -- that still need it as such
                            continue;   // keep needed data for certain systems -- skip items with colliders -- skip items with run-time prefabs -- skip items with billboards when using predictive preloading

                        if (compactMemoryCellList[i].vegetationPackageInstanceList[j].matrixInstanceList[k].IsCreated)
                            compactMemoryCellList[i].vegetationPackageInstanceList[j].matrixInstanceList[k].CompactMemory();    // clear redundant memory since we copied this data into GPU based buffers
                    }

                if (persistentVegetationStorage)
                    persistentVegetationStorage.GetPersistentVegetationCell(compactMemoryCellList[i].index)?.Dispose();
            }

            compactMemoryCellList.Clear();  // clear the list to not iterate again
        }

        private void ReturnTemporaryVegetationCellMemory(VegetationCell _vegetationCell)    // specific object pooling return for vegetation cells that got used for utility logic
        {
            for (int i = 0; i < _vegetationCell.vegetationInstanceDataList.Count; i++)  // for each instance we got from the instance pool
                vegetationCellSpawner.vegetationInstanceDataPool.ReturnObject(_vegetationCell.vegetationInstanceDataList[i]);   // return the object, make it ready for pooled re-use logic
            _vegetationCell.vegetationInstanceDataList.Clear(); // clear the list to not iterate again

            if (persistentVegetationStorage)
                persistentVegetationStorage.GetPersistentVegetationCell(_vegetationCell.index)?.Dispose();
        }

        public void SpawnVegetationCellEx(VegetationCell _vegetationCell, string _vegetationItemID, bool _skipPersistentVegetation = false)
        {   // utility spawn cell function external
            CompleteVegetationCellLoading();

            JobHandle cellSpawnHandle = vegetationCellSpawner.SpawnVegetationCell(_vegetationCell, _vegetationItemID, _skipPersistentVegetation);
            cellSpawnHandle.Complete(); // finish / synchronize assigned job handles

            ReturnTemporaryVegetationCellMemory(_vegetationCell);
        }

        public NativeList<MatrixInstance> GetVegetationItemInstances(VegetationCell _vegetationCell, string _vegetationItemID)
        {
            CompleteVegetationCellLoading();
            VegetationItemIndices vegetationItemIndices = GetVegetationItemIndices(_vegetationItemID);
            if (_vegetationCell.prepared && vegetationItemIndices.vegetationItemIndex > -1 && vegetationItemIndices.vegetationItemIndex > -1)
                return _vegetationCell.vegetationPackageInstanceList[vegetationItemIndices.vegetationPackageIndex].matrixInstanceList[vegetationItemIndices.vegetationItemIndex];
            return new NativeList<MatrixInstance>();
        }

        public VegetationItemIndices GetVegetationItemIndices(string _vegetationItemID)
        {
            VegetationItemIndices indices = new()
            {   // -1 as "false" flag to skip using the indices => further logic
                vegetationItemIndex = -1,
                vegetationPackageIndex = -1,
            };

            for (int i = 0; i < vegetationPackageProList.Count; i++)
                for (int j = 0; j < vegetationPackageProList[i].VegetationInfoList.Count; j++)
                    if (vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID == _vegetationItemID)
                    {
                        indices.vegetationPackageIndex = i;
                        indices.vegetationItemIndex = j;
                        return indices;
                    }

            return indices; // return "false"
        }

        public void RefreshTerrainArea()
        {
            RefreshTerrainArea(vegetationSystemBounds);
        }

        public void RefreshTerrainArea(Bounds _bounds)  // called to refresh/re-adjust entire system to terrain changes ex: modified matrices, heightMap changes, meshTerrain content changes
        {
            if (isSetupDone == false || vegetationCellQuadTree == null)
                return;

            ClearVegetationStudioCameraData();  // refresh entire cell culling as we modified the cell Y-Axis bounds potentially => and potentially even the XZ bounds, total system area, etc
            ClearCache(_bounds);    // clear the cache of all cells to let them refresh/re-generate all the vegetation item rules => thus resulting vegetation instances

            List<VegetationCell> overlapVegetationCellList = new();
            vegetationCellQuadTree.Query(RectExtension.CreateRectFromBounds(_bounds), overlapVegetationCellList);   // get all vegetationCells that overlap with the given bounds

            Bounds updateBounds = _bounds;
            NativeArray<Bounds> vegetationCellBounds = new(overlapVegetationCellList.Count, Allocator.TempJob);
            for (int i = 0; i < overlapVegetationCellList.Count; i++)   // for each overlapped vegetation cell
            {   // create cellBounds based on the existing XZ bounds => set Y-Axis to "-100000" to "disable" each cell by default
                Bounds cellBounds = RectExtension.CreateBoundsFromRect(overlapVegetationCellList[i].Rectangle, -100000);
                vegetationCellBounds[i] = cellBounds;   // write bounds into a temporary "nativeArray" to use them in a job
                updateBounds.Encapsulate(cellBounds);   // enlarge "updateBounds" to include all overlapped vegetation cells perfectly => ex: for when two terrains share the same vegetation cell
            }

            JobHandle handle = default;
            for (int i = 0; i < vegetationStudioTerrainList.Count; i++) // per terrain, get actual Y-Axis bounds of the cells (for cell culling) -- enable cells that are within the bounds ..and (partially) above the "seaLevel"
                handle = vegetationStudioTerrainList[i].SampleCellHeight(vegetationCellBounds, excludeSeaLevelCells ? systemRelativeSeaLevel = vegetationSystemBounds.min.y + SeaLevel : vegetationSystemBounds.min.y, RectExtension.CreateRectFromBounds(updateBounds), handle);
            JobHandle.ScheduleBatchedJobs();    // run/prioritize all terrains' sample height jobs
            handle.Complete();  // finish / synchronize assigned job handles

            for (int i = 0; i < overlapVegetationCellList.Count; i++)
                overlapVegetationCellList[i].cellBounds = vegetationCellBounds[i];  // write back from the temporary "nativeArray"
            vegetationCellBounds.Dispose(); // dispose the temporary "nativeArray"
        }
    }
}