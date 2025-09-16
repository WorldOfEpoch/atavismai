using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Culling;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
#if USING_HDRP && UNITY_2023_1_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif

namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public enum EVegetationStudioCameraType
    {
        Normal,
        SceneView
    }

    [Serializable]
    public enum ECameraCullingMode
    {
        ViewFrustum = 0,
        Complete360 = 1
    }

    [Serializable]
    public class VegetationStudioCamera
    {
        public VegetationSystemPro vegetationSystemPro;

        [SerializeField] public Camera selectedCamera;
#if USING_HDRP && UNITY_2023_1_OR_NEWER
        public HDCamera hdCamera;
#endif
        public EVegetationStudioCameraType eVegetationStudioCameraType = EVegetationStudioCameraType.Normal;
        public ECameraCullingMode eCameraCullingMode = ECameraCullingMode.ViewFrustum;
        public bool renderDirectToCamera;
        public bool renderBillboardsOnly;

        private Vector3 floatingOriginOffset;
        public GameObject speedTreeWindBridgeGO;
        [NonSerialized] public List<CameraMatrixLists> cameraMatrixLists;

        private JobHandle currentJobHandle;
        public CellCullingGroup vegetationCullingGroup;
        public CellCullingGroup billboardCullingGroup;
        [NonSerialized] public List<VegetationCell> preloadVegetationCellList;
        [NonSerialized] public List<BillboardCell> preloadBillboardCellList;
        private float3 lastVegetationCameraPosition = new(0, -10000, 0);
        private float3 lastBillboardCameraPosition = new(0, -10000, 0);
        private float lastVegetationDistance;
        private float lastBillboardDistance;
        private bool isDirtyVegetation = true;
        private bool isDirtyBillboard = true;

        public delegate void MultiOnVegetationCellVisibityChangedDelegate(VegetationStudioCamera _vegetationStudioCamera, VegetationCell _vegetationCell);
        public MultiOnVegetationCellVisibityChangedDelegate onVegetationCellVisibleDelegate;
        public MultiOnVegetationCellVisibityChangedDelegate onVegetationCellInvisibleDelegate;
        public MultiOnVegetationCellVisibityChangedDelegate onPreloadCellInvisibleDelegate;

        public delegate void MultiOnVegetationDistanceBandChangedDelegate(VegetationStudioCamera _vegetationStudioCamera, VegetationCell _vegetationCell, int _distanceBand, int _previousDistanceBand);
        public MultiOnVegetationDistanceBandChangedDelegate onVegetationCellDistanceBandChangedDelegate;

        public bool IsEnabled(bool _logError = false)
        {
            if (_logError && selectedCamera == null)
            {
                if (eVegetationStudioCameraType != EVegetationStudioCameraType.SceneView) Debug.LogError("VSP internal error log: Camera: A missing camera has been detected");
                return false;
            }
            if (vegetationCullingGroup == null || billboardCullingGroup == null) return false;
            if (cameraMatrixLists == null) return false;

            if (Application.isPlaying == false && eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView)
                return true;    // enable scene view camera for in-editor work

            if (Application.isPlaying == false && eVegetationStudioCameraType == EVegetationStudioCameraType.Normal)
                return false;   // disable any other camera while doing in-editor work

            // else enable camera for play mode if valid
            return (selectedCamera && selectedCamera.enabled && selectedCamera.gameObject.activeInHierarchy);
        }

        public VegetationStudioCamera(Camera _selectedCamera)
        {
            selectedCamera = _selectedCamera;
        }

        public VegetationStudioCamera(EVegetationStudioCameraType _vegetationStudioCameraType)
        {
#if UNITY_EDITOR
            if (_vegetationStudioCameraType != EVegetationStudioCameraType.SceneView)
                return;

            selectedCamera = SceneViewDetector.GetCurrentSceneViewCamera();
            eVegetationStudioCameraType = _vegetationStudioCameraType;
            SceneViewDetector.OnChangedSceneViewCameraDelegate += OnChangedSceneViewCameraDelegate;
#endif
        }

        void OnChangedSceneViewCameraDelegate(Camera _camera)
        {
#if UNITY_EDITOR
            selectedCamera = _camera;
            Dispose();
#endif
        }

        ~VegetationStudioCamera()
        {
            RemoveDelegates();
        }

        private Vector3 GetCameraPosition()
        {
            return selectedCamera.transform.position - floatingOriginOffset;
        }

        public void SetFloatingOriginOffset(Vector3 _newOffset)
        {
            floatingOriginOffset = _newOffset;

            vegetationCullingGroup?.SetFloatingOriginOffset(_newOffset);
            billboardCullingGroup?.SetFloatingOriginOffset(_newOffset);
        }

        public void PrepareRenderLists(List<VegetationPackagePro> _vegetationPackageProList)
        {
            if (selectedCamera == null)
                return;

            if (eVegetationStudioCameraType != EVegetationStudioCameraType.SceneView && selectedCamera.enabled == false || selectedCamera.transform.gameObject.activeInHierarchy == false)
                return;

            if (ValidateCameraMatrixLists(_vegetationPackageProList) == false)
                DisposeCameraMatrixLists();  // dispose if internal render lists don't match

            if (cameraMatrixLists == null)
            {
                cameraMatrixLists = new List<CameraMatrixLists>(_vegetationPackageProList.Count);
                for (int i = 0; i < _vegetationPackageProList.Count; i++)
                    cameraMatrixLists.Add(new CameraMatrixLists(_vegetationPackageProList[i].VegetationInfoList, vegetationSystemPro));
            }
        }

        private bool ValidateCameraMatrixLists(List<VegetationPackagePro> _vegetationPackageProList)
        {
            if (cameraMatrixLists?.Count != _vegetationPackageProList.Count)
                return false;

            for (int i = 0; i < cameraMatrixLists.Count; i++)
                if (cameraMatrixLists[i].mergedMatrixInstanceList.Count != _vegetationPackageProList[i].VegetationInfoList.Count)
                    return false;

            return true;
        }

        private void CreateCullingGroup()
        {
            vegetationCullingGroup?.Dispose();
            vegetationCullingGroup = new CellCullingGroup { targetCamera = selectedCamera };
            vegetationCullingGroup.OnStateChanged += OnStateChanged;
            vegetationCullingGroup.OnDistanceBandStateChanged += OnDistanceBandStateChanged;
        }

        private void CreateBillboardCullingGroup()
        {
            billboardCullingGroup?.Dispose();
            billboardCullingGroup = new CellCullingGroup { targetCamera = selectedCamera };
        }

        public void PreCullCalculations(Vector3 _floatingOriginOffset, bool _forceUpdate = false)
        {
            if (vegetationSystemPro == null || selectedCamera == null)
                return;

            if (eVegetationStudioCameraType != EVegetationStudioCameraType.SceneView && selectedCamera.enabled == false || selectedCamera.transform.gameObject.activeInHierarchy == false)
                return;

            if (vegetationCullingGroup == null) CreateCullingGroup();
            if (billboardCullingGroup == null) CreateBillboardCullingGroup();

            vegetationCullingGroup.cameraCullingMode = eCameraCullingMode;
            billboardCullingGroup.cameraCullingMode = eCameraCullingMode;

            GeometryUtilityAllocFree.CalculateFrustumPlanes(selectedCamera);
            for (int i = 0; i < 6; i++)
            {
                vegetationCullingGroup.frustumPlanes[i] = GeometryUtilityAllocFree.FrustumPlanes[i];
                billboardCullingGroup.frustumPlanes[i] = GeometryUtilityAllocFree.FrustumPlanes[i];
            }

            if (vegetationSystemPro.hasSunMoon)
            {
                vegetationCullingGroup.addShadowCells = vegetationSystemPro.vegetationRenderSettings.largeObjectShadows > -1 || vegetationSystemPro.vegetationRenderSettings.treeShadows > -1;
                vegetationCullingGroup.addShadowCells_DayNight = vegetationSystemPro.vegetationRenderSettings.dayNightSupport;
                vegetationCullingGroup.shadowDistance = QualitySettings.shadowDistance + vegetationSystemPro.vegetationCellSize;

                billboardCullingGroup.addShadowCells = vegetationSystemPro.vegetationRenderSettings.billboardShadows;
                billboardCullingGroup.addShadowCells_DayNight = vegetationSystemPro.vegetationRenderSettings.dayNightSupport;
                billboardCullingGroup.shadowDistance = QualitySettings.shadowDistance + vegetationSystemPro.billboardCellSize;
            }

            SetFloatingOriginOffset(_floatingOriginOffset);
            UpdateCellPreloading(_forceUpdate);
        }

        private void UpdateCellPreloading(bool _forceUpdate)
        {
            float3 selectedCameraPosition = GetCameraPosition();
            UpdateVegetationCellPreloading(_forceUpdate, selectedCameraPosition, math.distance(lastVegetationCameraPosition, selectedCameraPosition));
            UpdateBillboardCellPreloading(_forceUpdate, selectedCameraPosition, math.distance(lastBillboardCameraPosition, selectedCameraPosition));
        }

        private void UpdateVegetationCellPreloading(bool _forceUpdate, float3 _cameraPosition, float _cellToCamDistance)
        {
            if (vegetationCullingGroup == null)
                return;

            isDirtyVegetation = _forceUpdate;

            if (preloadVegetationCellList == null)
            {
                preloadVegetationCellList = new List<VegetationCell>();
                isDirtyVegetation = true;
            }

            float furthestVegetationDistance = vegetationSystemPro.vegetationSettings.GetFurthestVegetationDistance();
            if (furthestVegetationDistance <= 0 || renderBillboardsOnly)    // when distance is zero -- when in billboard only mode
            {
                preloadVegetationCellList.Clear();
                vegetationCullingGroup.CompactMemory();
                return; // disable vegetation rendering / cell loading when all vegetation distances are zero
            }

            if (isDirtyVegetation || _cellToCamDistance >= vegetationSystemPro.vegetationCellSize || lastVegetationDistance != furthestVegetationDistance)
            {   // dirty = at start/reset -- distance = when cam moved "cellSize" meters since its last pos then generate new ones through updating the "preloadAreaSize" -- update cells when "vegDistances" get modified
                lastVegetationCameraPosition = _cameraPosition;
                lastVegetationDistance = furthestVegetationDistance;
                isDirtyVegetation = true;
            }

            if (isDirtyVegetation)
            {
                isDirtyVegetation = false;

                float preloadAreaSize = (furthestVegetationDistance + vegetationSystemPro.vegetationCellSize) * 2;  // x2 since extents vs size -- preload within the needed distance + "cellSize" meters (matching above update limit)
                Rect selectedAreaRect = new(new float2(_cameraPosition.xz - (preloadAreaSize * 0.5f)), new float2(preloadAreaSize));

                if (onPreloadCellInvisibleDelegate != null)
                    for (int i = 0; i < preloadVegetationCellList.Count; i++)
                        if (preloadVegetationCellList[i]?.Rectangle.Overlaps(selectedAreaRect) == false)    // if not in range/visible anymore
                            onPreloadCellInvisibleDelegate(this, preloadVegetationCellList[i]); // notify collider system / runtime prefab spawner

                preloadVegetationCellList.Clear();
                vegetationSystemPro.vegetationCellQuadTree.Query(new(selectedAreaRect.position, selectedAreaRect.size), preloadVegetationCellList);

                vegetationCullingGroup.visibleCellIndexList.Clear();
                UpdateCullingGroup();   // update values for vegetation cell culling

                if (vegetationSystemPro.loadPredictiveCells)    // whether predictive preloading is enabled -- clear old data -- add (new) cells to preload list
                {
                    //vegetationSystemPro.predictiveCellLoader.ClearNonImportant();   // clear non important cells (all cells that got used for preloading previously)
                    vegetationSystemPro.predictiveCellLoader.AddPreloadAreaVegetation(preloadVegetationCellList, false);    // add cells to the list for actual preload later
                }
            }
        }

        private void UpdateBillboardCellPreloading(bool _forceUpdate, float3 _cameraPosition, float _cellToCamDistance)
        {
            if (billboardCullingGroup == null)
                return;

            isDirtyBillboard = _forceUpdate;

            if (preloadBillboardCellList == null)
            {
                preloadBillboardCellList = new List<BillboardCell>();
                isDirtyBillboard = true;
            }

            float billboardDistance = vegetationSystemPro.vegetationSettings.GetBillboardDistance(billboardCullingGroup.targetCamera.farClipPlane);
            if (billboardDistance <= 0) // when distance is zero
            {
                preloadBillboardCellList.Clear();
                billboardCullingGroup.CompactMemory();
                return; // disable billboard rendering / cell loading when the distance is zero
            }

            if (isDirtyBillboard || _cellToCamDistance >= vegetationSystemPro.billboardCellSize || lastBillboardDistance != billboardDistance)
            {   // dirty = at start/reset -- distance = when cam moved "cellSize" meters since its last pos then generate new ones through updating the "preloadAreaSize" -- update when "billboardDistance" gets modified
                lastBillboardCameraPosition = _cameraPosition;
                lastBillboardDistance = billboardDistance;
                isDirtyBillboard = true;
            }

            if (isDirtyBillboard)
            {
                isDirtyBillboard = false;

                float preloadAreaSize = (billboardDistance + vegetationSystemPro.billboardCellSize) * 2;    // x2 since extents vs size -- preload within the needed distance + "cellSize" meters (matching above update limit)
                Rect selectedAreaRect = new(new float2(_cameraPosition.xz - (preloadAreaSize * 0.5f)), new float2(preloadAreaSize));

                preloadBillboardCellList.Clear();
                vegetationSystemPro.billboardCellQuadTree.Query(new(selectedAreaRect.position, selectedAreaRect.size), preloadBillboardCellList);

                billboardCullingGroup.visibleCellIndexList.Clear();
                UpdateBillboardCullingGroup();  // update values for billboard cell culling

                if (vegetationSystemPro.loadPredictiveCells)    // whether predictive preloading is enabled -- clear old data -- add (new) cells to preload list
                {
                    //vegetationSystemPro.predictiveCellLoader.ClearNonImportant();   // clear non important cells (all cells that got used for preloading previously)
                    vegetationSystemPro.predictiveCellLoader.AddPreloadAreaBillboard(preloadBillboardCellList, false);  // add cells to the list for actual preload later
                }
            }
        }

        void UpdateCullingGroup()
        {
            if (vegetationSystemPro == null || vegetationCullingGroup == null)
                return;

            vegetationCullingGroup.distanceBandList.Clear();
            vegetationCullingGroup.cellCullingInfoList.Clear();

            // add highest distance of each distanceBandType to the list -- add vegetationCellSize as padding to pre-load cells just in time
            vegetationCullingGroup.distanceBandList.Add(vegetationSystemPro.vegetationSettings.GetLowerDistanceBandDistance() + vegetationSystemPro.vegetationCellSize);    // type 0 cells
            vegetationCullingGroup.distanceBandList.Add(vegetationSystemPro.vegetationSettings.GetHigherDistanceBandDistance() + vegetationSystemPro.vegetationCellSize);   // type 1 cells

            // update capacity of the list storing all the vegetation cells' culling info
            if (vegetationCullingGroup.cellCullingInfoList.Capacity < preloadVegetationCellList.Count)
                vegetationCullingGroup.cellCullingInfoList.Capacity = preloadVegetationCellList.Count;

            for (int i = 0; i < preloadVegetationCellList.Count; i++)   // only for vegetation cells within the preload range to reduce loop time when cell culling
            {
                CellCullingInfo cellCullingInfo = new() // create new culling info used for cell culling -- enlarge culling bounds for ~max possible item sizes
                {
                    Bounds = preloadVegetationCellList[i].cellBounds,
                    CellCullingBoundsAddy = vegetationSystemPro.cellCullingBoundsAddy,
                    CurrentDistanceBand = -1,
                    PreviousDistanceBand = -1,
                    Visibility = (int)ECellCullingVisibility.Invisible,
                    LastVisibility = (int)ECellCullingVisibility.Invisible,
                    Enabled = preloadVegetationCellList[i].EnabledInt   // vegetation cells can have a disabled state when below the "sea level"
                };
                vegetationCullingGroup.cellCullingInfoList.Add(cellCullingInfo);
            }
        }

        public void UpdateBillboardCullingGroup()
        {
            if (vegetationSystemPro == null || billboardCullingGroup == null)
                return;

            billboardCullingGroup.distanceBandList.Clear();
            billboardCullingGroup.cellCullingInfoList.Clear();

            // add current distance of billboards based on the selected camera's far clip plane and the "BillboardDistanceFactor" -- add billboardCellSize as padding to pre-load cells just in time
            billboardCullingGroup.distanceBandList.Add(vegetationSystemPro.vegetationSettings.GetBillboardDistance(selectedCamera.farClipPlane) + vegetationSystemPro.billboardCellSize);

            // update capacity of the list storing all the billboard cells' culling info
            if (billboardCullingGroup.cellCullingInfoList.Capacity < preloadBillboardCellList.Count)
                billboardCullingGroup.cellCullingInfoList.Capacity = preloadBillboardCellList.Count;

            for (int i = 0; i < preloadBillboardCellList.Count; i++)    // only for billboard cells within the preload range to reduce loop time when cell culling
            {
                CellCullingInfo cellCullingInfo = new() // create new culling info used for cell culling
                {
                    Bounds = preloadBillboardCellList[i].cellBounds,
                    CellCullingBoundsAddy = vegetationSystemPro.cellCullingBoundsAddy,
                    CurrentDistanceBand = -1,
                    PreviousDistanceBand = -1,
                    Visibility = (int)ECellCullingVisibility.Invisible,
                    LastVisibility = (int)ECellCullingVisibility.Invisible,
                    Enabled = 1 // not used for billboard cells -- vegetation cells can have a disabled state when below the "sea level"
                };
                billboardCullingGroup.cellCullingInfoList.Add(cellCullingInfo);
            }
        }

        public JobHandle ScheduleCellCulling(JobHandle _dependsOn)
        {
            if (vegetationCullingGroup == null)
                return _dependsOn;

            Profiler.BeginSample("Vegetation cell culling");
            currentJobHandle = vegetationCullingGroup.Cull(_dependsOn);
            Profiler.EndSample();

            if (billboardCullingGroup == null)
                return currentJobHandle;

            Profiler.BeginSample("Billboard cell culling");
            currentJobHandle = billboardCullingGroup.Cull(currentJobHandle);
            Profiler.EndSample();

            return currentJobHandle;
        }

        public CellCullingInfo GetCellCullingInfo(int _potentialVisibleVegetationCellIndex)
        {
            return vegetationCullingGroup.cellCullingInfoList[_potentialVisibleVegetationCellIndex];
        }

        public void ProcessEvents()
        {
            vegetationCullingGroup?.ProcessEvents();
            vegetationCullingGroup?.ProcessDistanceBandEvents();
        }

        void OnStateChanged(CellCullingEvent _cellCullingInfo)
        {
            if (_cellCullingInfo.IsVisible)
                onVegetationCellVisibleDelegate?.Invoke(this, preloadVegetationCellList[_cellCullingInfo.Index]);
            else
                onVegetationCellInvisibleDelegate?.Invoke(this, preloadVegetationCellList[_cellCullingInfo.Index]);
        }

        void OnDistanceBandStateChanged(CellCullingEvent _cellCullingInfo)
        {
            onVegetationCellDistanceBandChangedDelegate?.Invoke(this, preloadVegetationCellList[_cellCullingInfo.Index], _cellCullingInfo.CurrentDistanceBand, _cellCullingInfo.PreviousDistanceBand);
        }

        public void RemoveDelegates()
        {
#if UNITY_EDITOR
            if (eVegetationStudioCameraType != EVegetationStudioCameraType.SceneView) return;
            SceneViewDetector.OnChangedSceneViewCameraDelegate -= OnChangedSceneViewCameraDelegate;
#endif
        }

        public void DisposeCameraMatrixLists()
        {
            if (cameraMatrixLists != null)
            {
                for (int i = 0; i < cameraMatrixLists.Count; i++)
                    cameraMatrixLists[i].Dispose();
                cameraMatrixLists.Clear();
            }

            cameraMatrixLists = null;   // set to null to declare "recreationable"
        }

        public void Clear()
        {
            vegetationCullingGroup?.Clear();
            preloadVegetationCellList?.Clear();
            preloadVegetationCellList = null;   // declare "recreationable"
            lastVegetationCameraPosition = new(0, -10000, 0);
            lastVegetationDistance = 0;
            isDirtyVegetation = true;

            billboardCullingGroup?.Clear();
            preloadBillboardCellList?.Clear();
            preloadBillboardCellList = null;    // declare "recreationable"
            lastBillboardCameraPosition = new(0, -10000, 0);
            lastBillboardDistance = 0;
            isDirtyBillboard = true;
        }

        public void Dispose()
        {
            Clear();

            vegetationCullingGroup?.Dispose();
            vegetationCullingGroup = null;

            billboardCullingGroup?.Dispose();
            billboardCullingGroup = null;

            DisposeCameraMatrixLists();

            if (speedTreeWindBridgeGO)
                if (Application.isPlaying)
                    GameObject.Destroy(speedTreeWindBridgeGO);
                else
                    GameObject.DestroyImmediate(speedTreeWindBridgeGO);
        }

        #region Cell gizmos
#if UNITY_EDITOR
        public void DrawPredictiveVegetationCellGizmos()
        {
            if (preloadVegetationCellList == null)
                return;

            for (int i = 0; i < preloadVegetationCellList.Count; i++)   // for all cells within max vegetation distance + pre-load offset
                if (preloadVegetationCellList[i].Enabled)
                {
                    Gizmos.color = vegetationSystemPro.GetCellGizmoColor(true, preloadVegetationCellList[i].loadedDistanceBand);    // get color based on the cell's distance band type
                    Gizmos.DrawWireCube(preloadVegetationCellList[i].cellBounds.center, preloadVegetationCellList[i].cellBounds.size);
                }
        }

        public void DrawVisibleVegetationCellGizmos(VegetationSystemPro.ECellCullingDebugMode _cellCullingMode)
        {
            if (vegetationCullingGroup == null)
                return;

            for (int i = 0; i < vegetationCullingGroup.visibleCellIndexList.Length; i++)    // for all visible cells
            {
                CellCullingInfo cellCullingInfo = vegetationCullingGroup.cellCullingInfoList[vegetationCullingGroup.visibleCellIndexList[i]];
                if (cellCullingInfo.Enabled > 0)    // only for visible cells within preload cell list -- within max vegetation distance + pre-load offset
                {
                    Gizmos.color = vegetationSystemPro.GetCellGizmoColor(false, cellCullingInfo.Enabled == 2 ? cellCullingInfo.Enabled : cellCullingInfo.CurrentDistanceBand);  // get color based on the cell's distance band type
                    if (_cellCullingMode == VegetationSystemPro.ECellCullingDebugMode.CellSampling)
                        Gizmos.DrawWireCube(preloadVegetationCellList[vegetationCullingGroup.visibleCellIndexList[i]].cellBounds.center, preloadVegetationCellList[vegetationCullingGroup.visibleCellIndexList[i]].cellBounds.size);
                    else if (_cellCullingMode == VegetationSystemPro.ECellCullingDebugMode.CellCulling)
                        Gizmos.DrawWireCube(cellCullingInfo.Bounds.center + new Vector3(0, cellCullingInfo.CellCullingBoundsAddy.size.y * 0.5f, 0), cellCullingInfo.Bounds.size + cellCullingInfo.CellCullingBoundsAddy.size);
                }
            }
        }

        public void DrawPredictiveBillboardCellGizmos()
        {
            if (preloadBillboardCellList == null)
                return;

            for (int i = 0; i < preloadBillboardCellList.Count; i++)   // for all cells within billboard distance + pre-load offset
            {
                Gizmos.color = vegetationSystemPro.GetCellGizmoColor(true, preloadBillboardCellList[i].loadedState == 3 ? 1 : 99);  // get color based on the cell's loaded state
                Gizmos.DrawWireCube(preloadBillboardCellList[i].cellBounds.center, preloadBillboardCellList[i].cellBounds.size);
            }
        }

        public void DrawVisibleBillboardCellGizmos()
        {
            if (billboardCullingGroup == null)
                return;

            for (int i = 0; i < billboardCullingGroup.visibleCellIndexList.Length; i++) // for all visible billboard cells
            {
                CellCullingInfo cellCullingInfo = billboardCullingGroup.cellCullingInfoList[billboardCullingGroup.visibleCellIndexList[i]];
                Gizmos.color = vegetationSystemPro.GetCellGizmoColor(false, cellCullingInfo.Enabled == 2 ? cellCullingInfo.Enabled : cellCullingInfo.CurrentDistanceBand);  // get color based on the cell's distance band type
                Gizmos.DrawWireCube(cellCullingInfo.Bounds.center, cellCullingInfo.Bounds.size);
            }
        }
#endif
        #endregion
    }

    public static class GeometryUtilityAllocFree
    {
        public static Plane[] FrustumPlanes = new Plane[6];
        private static readonly Action<Plane[], Matrix4x4> InternalExtractPlanes = (Action<Plane[], Matrix4x4>)Delegate.CreateDelegate(typeof(Action<Plane[], Matrix4x4>), typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic));

        public static void CalculateFrustumPlanes(Camera _camera)
        {
            InternalExtractPlanes(FrustumPlanes, _camera.projectionMatrix * _camera.worldToCameraMatrix);
        }
    }

    public class CameraMatrixLists  // lists for CPU vegetation rendering
    {
        [NonSerialized] public readonly List<NativeList<MatrixInstance>> mergedMatrixInstanceList = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> matrixListLOD0 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> matrixListLOD1 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> matrixListLOD2 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> matrixListLOD3 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> fadeListLOD0 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> fadeListLOD1 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> fadeListLOD2 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> fadeListLOD3 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> shadowMatrixListLOD0 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> shadowMatrixListLOD1 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> shadowMatrixListLOD2 = new();
        [NonSerialized] public readonly List<NativeList<Matrix4x4>> shadowMatrixListLOD3 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> shadowFadeListLOD0 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> shadowFadeListLOD1 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> shadowFadeListLOD2 = new();
        [NonSerialized] public readonly List<NativeList<Vector4>> shadowFadeListLOD3 = new();

        public CameraMatrixLists(List<VegetationItemInfoPro> _vegItemInfoPro, VegetationSystemPro _vegetationSystemPro)
        {
            int capacity = 0;   // initial capacity -- increases permanently at run-time as needed -- later adjusts to power of two anyway
            for (int i = 0; i < _vegItemInfoPro.Count; i++)
            {
                mergedMatrixInstanceList.Add(new NativeList<MatrixInstance>(capacity, Allocator.Persistent));
                matrixListLOD0.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                matrixListLOD1.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                matrixListLOD2.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                matrixListLOD3.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                fadeListLOD0.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                fadeListLOD1.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                fadeListLOD2.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                fadeListLOD3.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                shadowMatrixListLOD0.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                shadowMatrixListLOD1.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                shadowMatrixListLOD2.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                shadowMatrixListLOD3.Add(new NativeList<Matrix4x4>(capacity, Allocator.Persistent));
                shadowFadeListLOD0.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                shadowFadeListLOD1.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                shadowFadeListLOD2.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
                shadowFadeListLOD3.Add(new NativeList<Vector4>(capacity, Allocator.Persistent));
            }
        }

        ~CameraMatrixLists()
        {
            Dispose();
        }

        public void Dispose()
        {
            DisposeMatrixInstanceList(mergedMatrixInstanceList);
            DisposeMatrixList(matrixListLOD0);
            DisposeMatrixList(matrixListLOD1);
            DisposeMatrixList(matrixListLOD2);
            DisposeMatrixList(matrixListLOD3);
            DisposeVector4List(fadeListLOD0);
            DisposeVector4List(fadeListLOD1);
            DisposeVector4List(fadeListLOD2);
            DisposeVector4List(fadeListLOD3);
            DisposeMatrixList(shadowMatrixListLOD0);
            DisposeMatrixList(shadowMatrixListLOD1);
            DisposeMatrixList(shadowMatrixListLOD2);
            DisposeMatrixList(shadowMatrixListLOD3);
            DisposeVector4List(shadowFadeListLOD0);
            DisposeVector4List(shadowFadeListLOD1);
            DisposeVector4List(shadowFadeListLOD2);
            DisposeVector4List(shadowFadeListLOD3);
            GC.SuppressFinalize(this);  // avoid running the "finalizer / destructor" on ex: scene exit
        }

        void DisposeMatrixInstanceList(List<NativeList<MatrixInstance>> _list)
        {
            for (int i = 0; i < _list?.Count; i++)
                if (_list[i].IsCreated) _list[i].Dispose();
        }

        void DisposeMatrixList(List<NativeList<Matrix4x4>> _list)
        {
            for (int i = 0; i < _list?.Count; i++)
                if (_list[i].IsCreated) _list[i].Dispose();
        }

        void DisposeVector4List(List<NativeList<Vector4>> _list)
        {
            for (int i = 0; i < _list?.Count; i++)
                if (_list[i].IsCreated) _list[i].Dispose();
        }
    }

    public class CameraGraphicsBuffers  // buffers for GPU vegetation rendering
    {
        private readonly int appendStride = 144;    // size in bytes of the "IndirectShaderData" struct -- specific stride for the instanceData

        // fields for storing data while/after frustum culling / LOD calculation
        public GraphicsBuffer mergeBuffer;  // stores merged matrix data of instances of all cells
        public GraphicsBuffer objectBufferLOD0;
        public GraphicsBuffer objectBufferLOD1;
        public GraphicsBuffer objectBufferLOD2;
        public GraphicsBuffer objectBufferLOD3;
        public GraphicsBuffer shadowBufferLOD0;
        public GraphicsBuffer shadowBufferLOD1;
        public GraphicsBuffer shadowBufferLOD2;
        public GraphicsBuffer shadowBufferLOD3;

        // fields for storing copied extracts for the rendering -- arguments for the indirect rendering
        private readonly GraphicsBuffer.IndirectDrawIndexedArgs[] indirectArguments = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        public readonly List<GraphicsBuffer> argsBufferListLOD0 = new();
        public readonly List<GraphicsBuffer> argsBufferListLOD1 = new();
        public readonly List<GraphicsBuffer> argsBufferListLOD2 = new();
        public readonly List<GraphicsBuffer> argsBufferListLOD3 = new();
        public readonly List<GraphicsBuffer> shadowArgsBufferListLOD0 = new();
        public readonly List<GraphicsBuffer> shadowArgsBufferListLOD1 = new();
        public readonly List<GraphicsBuffer> shadowArgsBufferListLOD2 = new();
        public readonly List<GraphicsBuffer> shadowArgsBufferListLOD3 = new();

        public CameraGraphicsBuffers(Mesh _vegetationMeshLod0, Mesh _vegetationMeshLod1, Mesh _vegetationMeshLod2, Mesh _vegetationMeshLod3)
        {
            int instanceCount = 1;  // initial capacity -- increases permanently at run-time just as needed -- increases by "0.1%"
            int commandCount = 1;

            mergeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride);
            objectBufferLOD0 = new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride);
            objectBufferLOD1 = _vegetationMeshLod1 ? new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride) : null;
            objectBufferLOD2 = _vegetationMeshLod2 ? new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride) : null;
            objectBufferLOD3 = _vegetationMeshLod3 ? new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride) : null;
            shadowBufferLOD0 = new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride);
            shadowBufferLOD1 = _vegetationMeshLod1 ? new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride) : null;
            shadowBufferLOD2 = _vegetationMeshLod2 ? new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride) : null;
            shadowBufferLOD3 = _vegetationMeshLod3 ? new GraphicsBuffer(GraphicsBuffer.Target.Append, instanceCount, appendStride) : null;

            if (_vegetationMeshLod0)
                for (int i = 0; i < _vegetationMeshLod0.subMeshCount; i++)
                {
                    GraphicsBuffer argsBufferMergedLod0 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                    GraphicsBuffer shadowArgsBufferMergedLod0 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);

                    indirectArguments[0].baseVertexIndex = _vegetationMeshLod0.GetBaseVertex(i);
                    indirectArguments[0].indexCountPerInstance = _vegetationMeshLod0.GetIndexCount(i);
                    indirectArguments[0].startIndex = _vegetationMeshLod0.GetIndexStart(i);

                    argsBufferMergedLod0.SetData(indirectArguments);
                    shadowArgsBufferMergedLod0.SetData(indirectArguments);

                    argsBufferListLOD0.Add(argsBufferMergedLod0);
                    shadowArgsBufferListLOD0.Add(shadowArgsBufferMergedLod0);
                }

            if (_vegetationMeshLod1)
                for (int i = 0; i < _vegetationMeshLod1.subMeshCount; i++)
                {
                    GraphicsBuffer argsBufferMergedLod1 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                    GraphicsBuffer shadowArgsBufferMergedLod1 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);

                    indirectArguments[0].baseVertexIndex = _vegetationMeshLod1.GetBaseVertex(i);
                    indirectArguments[0].indexCountPerInstance = _vegetationMeshLod1.GetIndexCount(i);
                    indirectArguments[0].startIndex = _vegetationMeshLod1.GetIndexStart(i);

                    argsBufferMergedLod1.SetData(indirectArguments);
                    shadowArgsBufferMergedLod1.SetData(indirectArguments);

                    argsBufferListLOD1.Add(argsBufferMergedLod1);
                    shadowArgsBufferListLOD1.Add(shadowArgsBufferMergedLod1);
                }

            if (_vegetationMeshLod2)
                for (int i = 0; i < _vegetationMeshLod2.subMeshCount; i++)
                {
                    GraphicsBuffer argsBufferMergedLod2 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                    GraphicsBuffer shadowArgsBufferMergedLod2 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);

                    indirectArguments[0].baseVertexIndex = _vegetationMeshLod2.GetBaseVertex(i);
                    indirectArguments[0].indexCountPerInstance = _vegetationMeshLod2.GetIndexCount(i);
                    indirectArguments[0].startIndex = _vegetationMeshLod2.GetIndexStart(i);

                    argsBufferMergedLod2.SetData(indirectArguments);
                    shadowArgsBufferMergedLod2.SetData(indirectArguments);

                    argsBufferListLOD2.Add(argsBufferMergedLod2);
                    shadowArgsBufferListLOD2.Add(shadowArgsBufferMergedLod2);
                }

            if (_vegetationMeshLod3)
                for (int i = 0; i < _vegetationMeshLod3.subMeshCount; i++)
                {
                    GraphicsBuffer argsBufferMergedLod3 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                    GraphicsBuffer shadowArgsBufferMergedLod3 = new(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);

                    indirectArguments[0].baseVertexIndex = _vegetationMeshLod3.GetBaseVertex(i);
                    indirectArguments[0].indexCountPerInstance = _vegetationMeshLod3.GetIndexCount(i);
                    indirectArguments[0].startIndex = _vegetationMeshLod3.GetIndexStart(i);

                    argsBufferMergedLod3.SetData(indirectArguments);
                    shadowArgsBufferMergedLod3.SetData(indirectArguments);

                    argsBufferListLOD3.Add(argsBufferMergedLod3);
                    shadowArgsBufferListLOD3.Add(shadowArgsBufferMergedLod3);
                }
        }

        ~CameraGraphicsBuffers()    // workaround for "async" states caused by third parties
        {
            ReleaseGraphicsBuffers();
        }

        public void UpdateGraphicsBufferSize(int _instanceCount)
        {
            mergeBuffer?.Release();
            objectBufferLOD0?.Release();
            objectBufferLOD1?.Release();
            objectBufferLOD2?.Release();
            objectBufferLOD3?.Release();
            shadowBufferLOD0?.Release();
            shadowBufferLOD1?.Release();
            shadowBufferLOD2?.Release();
            shadowBufferLOD3?.Release();

            mergeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride);
            objectBufferLOD0 = new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride);
            objectBufferLOD1 = objectBufferLOD1 != null ? new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride) : null;
            objectBufferLOD2 = objectBufferLOD2 != null ? new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride) : null;
            objectBufferLOD3 = objectBufferLOD3 != null ? new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride) : null;
            shadowBufferLOD0 = new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride);
            shadowBufferLOD1 = shadowBufferLOD1 != null ? new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride) : null;
            shadowBufferLOD2 = shadowBufferLOD2 != null ? new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride) : null;
            shadowBufferLOD3 = shadowBufferLOD3 != null ? new GraphicsBuffer(GraphicsBuffer.Target.Append, _instanceCount, appendStride) : null;
        }

        public void CopyInstanceCounts(bool _isShadow)
        {
            if (_isShadow)
            {
                for (int i = 0; i < shadowArgsBufferListLOD0.Count; i++)    // LOD 0
                    GraphicsBuffer.CopyCount(shadowBufferLOD0, shadowArgsBufferListLOD0[i], sizeof(uint));
                for (int i = 0; i < shadowArgsBufferListLOD1.Count; i++)    // LOD 1
                    GraphicsBuffer.CopyCount(shadowBufferLOD1, shadowArgsBufferListLOD1[i], sizeof(uint));
                for (int i = 0; i < shadowArgsBufferListLOD2.Count; i++)    // LOD 2
                    GraphicsBuffer.CopyCount(shadowBufferLOD2, shadowArgsBufferListLOD2[i], sizeof(uint));
                for (int i = 0; i < shadowArgsBufferListLOD3.Count; i++)    // LOD 3
                    GraphicsBuffer.CopyCount(shadowBufferLOD3, shadowArgsBufferListLOD3[i], sizeof(uint));
            }
            else
            {
                for (int i = 0; i < argsBufferListLOD0.Count; i++)  // LOD 0
                    GraphicsBuffer.CopyCount(objectBufferLOD0, argsBufferListLOD0[i], sizeof(uint));
                for (int i = 0; i < argsBufferListLOD1.Count; i++)  // LOD 1
                    GraphicsBuffer.CopyCount(objectBufferLOD1, argsBufferListLOD1[i], sizeof(uint));
                for (int i = 0; i < argsBufferListLOD2.Count; i++)  // LOD 2
                    GraphicsBuffer.CopyCount(objectBufferLOD2, argsBufferListLOD2[i], sizeof(uint));
                for (int i = 0; i < argsBufferListLOD3.Count; i++)  // LOD 3
                    GraphicsBuffer.CopyCount(objectBufferLOD3, argsBufferListLOD3[i], sizeof(uint));
            }
        }

        public GraphicsBuffer GetIndirectBufferAtIndex(int _lodIndex, bool _isShadow)
        {
            if (_isShadow)
            {
                return _lodIndex switch
                {
                    0 => shadowBufferLOD0,
                    1 => shadowBufferLOD1,
                    2 => shadowBufferLOD2,
                    3 => shadowBufferLOD3,
                    _ => null,
                };
            }
            else
            {
                return _lodIndex switch
                {
                    0 => objectBufferLOD0,
                    1 => objectBufferLOD1,
                    2 => objectBufferLOD2,
                    3 => objectBufferLOD3,
                    _ => null,
                };
            }
        }

        public List<GraphicsBuffer> GetArgsBufferAtIndex(int _lodIndex, bool _isShadow)
        {
            if (_isShadow)
            {
                return _lodIndex switch
                {
                    0 => shadowArgsBufferListLOD0,
                    1 => shadowArgsBufferListLOD1,
                    2 => shadowArgsBufferListLOD2,
                    3 => shadowArgsBufferListLOD3,
                    _ => null,
                };
            }
            else
            {
                return _lodIndex switch
                {
                    0 => argsBufferListLOD0,
                    1 => argsBufferListLOD1,
                    2 => argsBufferListLOD2,
                    3 => argsBufferListLOD3,
                    _ => null,
                };
            }
        }

        public void ReleaseGraphicsBuffers()
        {
            mergeBuffer?.Release();
            objectBufferLOD0?.Release();
            objectBufferLOD1?.Release();
            objectBufferLOD2?.Release();
            objectBufferLOD3?.Release();
            shadowBufferLOD0?.Release();
            shadowBufferLOD1?.Release();
            shadowBufferLOD2?.Release();
            shadowBufferLOD3?.Release();

            mergeBuffer = null;
            objectBufferLOD0 = null;
            objectBufferLOD1 = null;
            objectBufferLOD2 = null;
            objectBufferLOD3 = null;
            shadowBufferLOD0 = null;
            shadowBufferLOD1 = null;
            shadowBufferLOD2 = null;
            shadowBufferLOD3 = null;

            ReleaseArgsBuffers();
            GC.SuppressFinalize(this);  // avoid running the "finalizer / destructor" on ex: scene exit
        }

        void ReleaseArgsBuffers()
        {
            for (int i = 0; i < argsBufferListLOD0.Count; i++)
                if (argsBufferListLOD0[i] != null)
                {
                    argsBufferListLOD0[i]?.Release();
                    argsBufferListLOD0[i] = null;
                }
            argsBufferListLOD0.Clear();

            for (int i = 0; i < argsBufferListLOD1.Count; i++)
                if (argsBufferListLOD1[i] != null)
                {
                    argsBufferListLOD1[i]?.Release();
                    argsBufferListLOD1[i] = null;
                }
            argsBufferListLOD1.Clear();

            for (int i = 0; i < argsBufferListLOD2.Count; i++)
                if (argsBufferListLOD2[i] != null)
                {
                    argsBufferListLOD2[i]?.Release();
                    argsBufferListLOD2[i] = null;
                }
            argsBufferListLOD2.Clear();

            for (int i = 0; i < argsBufferListLOD3.Count; i++)
                if (argsBufferListLOD3[i] != null)
                {
                    argsBufferListLOD3[i]?.Release();
                    argsBufferListLOD3[i] = null;
                }
            argsBufferListLOD3.Clear();

            for (int i = 0; i < shadowArgsBufferListLOD0.Count; i++)
                if (shadowArgsBufferListLOD0[i] != null)
                {
                    shadowArgsBufferListLOD0[i]?.Release();
                    shadowArgsBufferListLOD0[i] = null;
                }
            shadowArgsBufferListLOD0.Clear();

            for (int i = 0; i < shadowArgsBufferListLOD1.Count; i++)
                if (shadowArgsBufferListLOD1[i] != null)
                {
                    shadowArgsBufferListLOD1[i]?.Release();
                    shadowArgsBufferListLOD1[i] = null;
                }
            shadowArgsBufferListLOD1.Clear();

            for (int i = 0; i < shadowArgsBufferListLOD2.Count; i++)
                if (shadowArgsBufferListLOD2[i] != null)
                {
                    shadowArgsBufferListLOD2[i]?.Release();
                    shadowArgsBufferListLOD2[i] = null;
                }
            shadowArgsBufferListLOD2.Clear();

            for (int i = 0; i < shadowArgsBufferListLOD3.Count; i++)
                if (shadowArgsBufferListLOD3[i] != null)
                {
                    shadowArgsBufferListLOD3[i]?.Release();
                    shadowArgsBufferListLOD3[i] = null;
                }
            shadowArgsBufferListLOD3.Clear();
        }
    }
}