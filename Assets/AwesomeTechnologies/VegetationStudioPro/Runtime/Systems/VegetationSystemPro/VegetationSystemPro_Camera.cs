using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void AddCamera(Camera _camera, bool _noFrustumCulling = false, bool _renderDirectToCamera = false, bool _renderBillboardsOnly = false)
        {
            CompleteVegetationCellLoading();

            VegetationStudioCamera vegetationStudioCamera = GetVegetationStudioCamera(_camera);
            if (vegetationStudioCamera == null)
            {
                vegetationStudioCamera = new VegetationStudioCamera(_camera)
                {
                    eCameraCullingMode = _noFrustumCulling ? ECameraCullingMode.Complete360 : ECameraCullingMode.ViewFrustum,
                    renderDirectToCamera = _renderDirectToCamera,
                    renderBillboardsOnly = _renderBillboardsOnly,
                    vegetationSystemPro = this
                };

                AddVegetationStudioCamera(vegetationStudioCamera);
            }

            SetupSpeedTreeWindBridge(); // setup "SpeedTree WindBridge"
            SetupCameraDataPerModel();  // setup new render lists (GPU and billboards)
        }

        private void AddVegetationStudioCamera(VegetationStudioCamera _vegetationStudioCamera)
        {
            vegetationStudioCameraList.Add(_vegetationStudioCamera);
            OnAddCameraDelegate?.Invoke(_vegetationStudioCamera);

            RefreshColliderSystem();
            RefreshRuntimePrefabSpawner();
        }

        public void RemoveCamera(Camera _camera)
        {
            CompleteVegetationCellLoading();

            VegetationStudioCamera vegetationStudioCamera = GetVegetationStudioCamera(_camera);
            if (vegetationStudioCamera != null)
                RemoveVegetationStudioCamera(vegetationStudioCamera);

            SetupCameraDataPerModel();  // dispose/clear render lists (GPU and billboards)
            RefreshColliderSystem();
            RefreshRuntimePrefabSpawner();
        }

        private void RemoveVegetationStudioCamera(VegetationStudioCamera _vegetationStudioCamera)
        {
            _vegetationStudioCamera.Dispose();
            vegetationStudioCameraList.Remove(_vegetationStudioCamera);
            OnRemoveCameraDelegate?.Invoke(_vegetationStudioCamera);
        }

        public VegetationStudioCamera GetVegetationStudioCamera(Camera _camera)
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                if (vegetationStudioCameraList[i].selectedCamera == _camera)
                    return vegetationStudioCameraList[i];
            return null;
        }

        public VegetationStudioCamera GetSceneViewVegetationStudioCamera()
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                if (vegetationStudioCameraList[i].eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView)
                    return vegetationStudioCameraList[i];
            return null;
        }

        private void PrepareCameraData()
        {
            Profiler.BeginSample("Prepare needed camera data(per camera)");
            JobHandle cullingHandle = default;
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // for each camera prepare own data
            {
                Profiler.BeginSample("Pre-cull calculations for vegetation/billboard cells and instances");
                vegetationStudioCameraList[i].PreCullCalculations(floatingOriginOffset, shouldForceUpdateCellCulling);  // set floating origin offset -- get camera frustum planes -- create and set cullingGroups for cell culling/(pre-)loading
                Profiler.EndSample();

                Profiler.BeginSample("Prepare camera internal render lists");
                vegetationStudioCameraList[i].PrepareRenderLists(vegetationPackageProList); // prepare(if needed) render lists for CPU vegetation rendering
                Profiler.EndSample();

                if (vegetationStudioCameraList[i].IsEnabled(true) == false)
                    continue;   // skip if previous calculations failed

                Profiler.BeginSample("Schedule vegetation/billboard cell culling");
                cullingHandle = vegetationStudioCameraList[i].ScheduleCellCulling(cullingHandle);   // run cell culling -- set visibility states -- fill lists based on current/changed states
                Profiler.EndSample();

                Profiler.BeginSample("Complete vegetation/billboard cell culling");
                JobHandle.ScheduleBatchedJobs();    // run/prioritize all cell culling jobs
                cullingHandle.Complete();   // complete cell culling before processing events
                Profiler.EndSample();

                Profiler.BeginSample("Process vegetation cell culling events");
                vegetationStudioCameraList[i].ProcessEvents();  // handle events -- on change callbacks -- used for collider / runtime prefab system
                Profiler.EndSample();
            }
            shouldForceUpdateCellCulling = false;
            Profiler.EndSample();
        }

        public void ForceCellCullingRefresh()
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                vegetationStudioCameraList[i].PreCullCalculations(floatingOriginOffset, true);
        }

        public void ClearVegetationStudioCameraData()
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                vegetationStudioCameraList[i].Clear();
            shouldForceUpdateCellCulling = true;    // don't re-override camera internal states -- also updates "cellCullingBoundsAddy"
        }

        public void DisposeVegetationStudioCameraMatrixLists()
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                vegetationStudioCameraList[i].DisposeCameraMatrixLists();
        }

        public void DisposeVegetationStudioCameraData()
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
            {
                vegetationStudioCameraList[i].Dispose();
#if USING_HDRP && UNITY_2023_1_OR_NEWER
                DisposeRTAS(i);
#endif
            }
        }
    }
}