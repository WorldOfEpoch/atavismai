using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Culling
{
    public struct CellCullingInfo
    {
        public Bounds Bounds;
        public Bounds CellCullingBoundsAddy;
        public int CurrentDistanceBand;
        public int PreviousDistanceBand;
        public int Visibility;
        public int LastVisibility;
        public int Enabled;
    }

    public struct CellCullingEvent
    {
        public int Index;
        public bool IsVisible;
        public int CurrentDistanceBand;
        public int PreviousDistanceBand;
    }

    enum ECellCullingVisibility
    {
        Invisible = -1,
        Visible = 1
    }

    public class CellCullingGroup
    {
        public Camera targetCamera;
        public ECameraCullingMode cameraCullingMode = ECameraCullingMode.ViewFrustum;
        public bool addShadowCells;
        public bool addShadowCells_DayNight;
        public float shadowDistance;
        private float3 floatingOriginOffset = new(0, 0, 0);

        public NativeList<float> distanceBandList;
        public NativeList<CellCullingInfo> cellCullingInfoList;
        public NativeList<int> visibleCellIndexList;
        public NativeArray<Plane> frustumPlanes;
        private NativeList<int> visibilityEventList;
        private NativeList<int> distanceBandEventList;

        public delegate void StateChanged(CellCullingEvent _cellCullingInfo);
        public StateChanged OnStateChanged;
        public StateChanged OnDistanceBandStateChanged;

        public CellCullingGroup()
        {
            distanceBandList = new NativeList<float>(2, Allocator.Persistent);
            cellCullingInfoList = new NativeList<CellCullingInfo>(Allocator.Persistent);
            visibleCellIndexList = new NativeList<int>(Allocator.Persistent);
            frustumPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
            visibilityEventList = new NativeList<int>(Allocator.Persistent);
            distanceBandEventList = new NativeList<int>(Allocator.Persistent);
        }

        ~CellCullingGroup()
        {
            Dispose();
        }

        public void SetFloatingOriginOffset(float3 _floatingOriginOffset)
        {
            floatingOriginOffset = _floatingOriginOffset;
        }

        private float3 GetTargetCameraPosition()
        {
            return targetCamera.transform.position;
        }

        public JobHandle Cull(JobHandle _dependsOn)
        {
            visibleCellIndexList.Clear();
            visibilityEventList.Clear();
            distanceBandEventList.Clear();

            if (targetCamera == null || cellCullingInfoList.Length == 0)
                return _dependsOn;

            CellCullingJob cellCullingJob = new()
            {
                CellCullingInfoList = cellCullingInfoList,
                DistanceReferencePoint = GetTargetCameraPosition(),
                DistancesList = distanceBandList,
                FrustumPlanes = frustumPlanes,
                NoFrustumCulling = cameraCullingMode == ECameraCullingMode.Complete360,
                AddShadowCells = addShadowCells,
                AddShadowCells_DayNight = addShadowCells_DayNight,
                ShadowDistance = shadowDistance,
                FloatingOriginOffset = floatingOriginOffset
            };
            JobHandle handle = cellCullingJob.ScheduleParallel(cellCullingInfoList.Length, 64, _dependsOn); // cell culling

            CellCullingVisibleJob cellCullingVisibleJob = new() { CellCullingInfoList = cellCullingInfoList };  // for each visible "cellCullingInfo"
            handle = cellCullingVisibleJob.ScheduleAppend(visibleCellIndexList, cellCullingInfoList.AsParallelReader().Length, handle); // export original indices of "cellCullingInfoList" into "visibleCellIndexList"

            CellCullingEventJob cellCullingEventJob = new() { CellCullingInfoList = cellCullingInfoList };  // for each "cellCullingInfo" that changed its visibility state
            handle = cellCullingEventJob.ScheduleAppend(visibilityEventList, cellCullingInfoList.AsParallelReader().Length, handle);  // -> export original indices of "cellCullingInfoList" into "eventList" 

            CellCullingDistanceBandEventJob cellCullingDistanceBandEventJob = new() { CellCullingInfoList = cellCullingInfoList };  // for each "cellCullingInfo" that changed its distance state
            return cellCullingDistanceBandEventJob.ScheduleAppend(distanceBandEventList, cellCullingInfoList.AsParallelReader().Length, handle);    // -> export original indices of "cellCullingInfoList" into "distanceBandEventList"
        }

        public void ProcessEvents()
        {
            for (int i = 0; i < visibilityEventList.Length; i++)    // for all cellInfo whose visibility state changed
            {
                OnStateChanged?.Invoke(new()
                {
                    Index = visibilityEventList[i],
                    IsVisible = cellCullingInfoList[visibilityEventList[i]].Visibility == (int)ECellCullingVisibility.Visible,
                    CurrentDistanceBand = cellCullingInfoList[visibilityEventList[i]].CurrentDistanceBand,
                    PreviousDistanceBand = cellCullingInfoList[visibilityEventList[i]].PreviousDistanceBand
                }); // notify registered functions -- used for collider / runtime prefab system

                CellCullingInfo cellCullingInfo = cellCullingInfoList[visibilityEventList[i]];
                cellCullingInfo.LastVisibility = cellCullingInfoList[visibilityEventList[i]].Visibility;    // set new (current) visibility state to compare against next frame
                cellCullingInfoList[visibilityEventList[i]] = cellCullingInfo;
            }
        }

        public void ProcessDistanceBandEvents()
        {
            for (int i = 0; i < distanceBandEventList.Length; i++)  // for all cellInfo whose distanceBand changed
            {
                OnDistanceBandStateChanged?.Invoke(new()
                {
                    Index = distanceBandEventList[i],
                    IsVisible = cellCullingInfoList[distanceBandEventList[i]].Visibility == (int)ECellCullingVisibility.Visible,
                    CurrentDistanceBand = cellCullingInfoList[distanceBandEventList[i]].CurrentDistanceBand,
                    PreviousDistanceBand = cellCullingInfoList[distanceBandEventList[i]].PreviousDistanceBand
                }); // notify registered functions -- used for collider / runtime prefab system

                CellCullingInfo cellCullingInfo = cellCullingInfoList[distanceBandEventList[i]];
                cellCullingInfo.PreviousDistanceBand = cellCullingInfoList[distanceBandEventList[i]].CurrentDistanceBand;   // set new (current) distanceBand state to compare against next frame
                cellCullingInfoList[distanceBandEventList[i]] = cellCullingInfo;
            }
        }

        public void Clear()
        {
            if (distanceBandList.IsCreated) distanceBandList.Clear();
            if (cellCullingInfoList.IsCreated) cellCullingInfoList.Clear();
            if (visibleCellIndexList.IsCreated) visibleCellIndexList.Clear();
            if (visibilityEventList.IsCreated) visibilityEventList.Clear();
            if (distanceBandEventList.IsCreated) distanceBandEventList.Clear();
        }

        public void CompactMemory()
        {
            if (distanceBandList.IsCreated) distanceBandList.CompactMemory();
            if (cellCullingInfoList.IsCreated) cellCullingInfoList.CompactMemory();
            if (visibleCellIndexList.IsCreated) visibleCellIndexList.CompactMemory();
            if (visibilityEventList.IsCreated) visibilityEventList.CompactMemory();
            if (distanceBandEventList.IsCreated) distanceBandEventList.CompactMemory();
        }

        public void Dispose()
        {
            if (distanceBandList.IsCreated) distanceBandList.Dispose();
            if (cellCullingInfoList.IsCreated) cellCullingInfoList.Dispose();
            if (visibleCellIndexList.IsCreated) visibleCellIndexList.Dispose();
            if (frustumPlanes.IsCreated) frustumPlanes.Dispose();
            if (visibilityEventList.IsCreated) visibilityEventList.Dispose();
            if (distanceBandEventList.IsCreated) distanceBandEventList.Dispose();
        }
    }

    #region jobs
    [BurstCompile]
    struct CellCullingJob : IJobFor
    {
        [NativeDisableParallelForRestriction] public NativeList<CellCullingInfo> CellCullingInfoList;
        [ReadOnly] public NativeList<float> DistancesList;
        [ReadOnly] public NativeArray<Plane> FrustumPlanes;
        [ReadOnly] public float3 DistanceReferencePoint;
        [ReadOnly] public bool NoFrustumCulling;
        [ReadOnly] public bool AddShadowCells;
        [ReadOnly] public bool AddShadowCells_DayNight;
        [ReadOnly] public float ShadowDistance;
        [ReadOnly] public Vector3 FloatingOriginOffset;

        public void Execute(int index)
        {
            if (CellCullingInfoList[index].Enabled == 0)
                return; // return if no culling info got generated -- cells are "disabled" since underwater

            CellCullingInfo cellCullingInfo = CellCullingInfoList[index];
            cellCullingInfo.Bounds.center += FloatingOriginOffset;

            // calculate distance and check whether a cell is in (pre-)load range
            float distance = math.distance(cellCullingInfo.Bounds.center, DistanceReferencePoint);
            for (int i = 0; i < DistancesList.Length; i++)
                if (distance <= DistancesList[i])
                {
                    cellCullingInfo.CurrentDistanceBand = i;
                    break;
                }

            if (cellCullingInfo.CurrentDistanceBand != -1)  // when not ouf of range (edge case since preloaded cells are in range generally) -- avoid writing "1" to "Visibility" to avoid false pooling system event writes
            {
                Bounds effectiveBounds = new(cellCullingInfo.Bounds.center + new Vector3(0, cellCullingInfo.CellCullingBoundsAddy.size.y * 0.5f, 0), cellCullingInfo.Bounds.size + cellCullingInfo.CellCullingBoundsAddy.size);
                cellCullingInfo.Visibility = NoFrustumCulling ? 1 : BoundsInFrustum(effectiveBounds);   // set visibility based on frustum culling
            }

            if (AddShadowCells && NoFrustumCulling == false)    // load cells behind a camera for shadow culling when needed
                if (cellCullingInfo.Visibility == -1 && distance <= (AddShadowCells_DayNight ? math.max(DistancesList[^1], ShadowDistance) : math.min(DistancesList[^1], ShadowDistance)))
                {   // default is based on the min dist of shadowDistance vs highest distance -- dayNight mode inverts this behavior => ensure no shadow pop-in at the cost of performance / memory
                    cellCullingInfo.Visibility = 1; // set the cell to be visible to render shadows -- for billboard cells the entire "billboard-cell-mesh" gets rendered in addition
                    cellCullingInfo.CurrentDistanceBand = 1;    // set as "higher distance band" to only load data of "1" type cells
                    cellCullingInfo.Enabled = 2;    // indicate that the cell is enabled behind the camera
                }
                else
                    cellCullingInfo.Enabled = 1;    // reset => the default state only gets set on new data from the "pre-load square" ==> no new data when standing still while rotating the camera

            cellCullingInfo.Bounds.center -= FloatingOriginOffset;
            CellCullingInfoList[index] = cellCullingInfo;
        }

        //int SphereInFrustum(BoundingSphere _boundingSphere)
        //{
        //    for (int i = 0; i < FrustumPlanes.Length; i++)
        //        if (math.dot(FrustumPlanes[i].normal, _boundingSphere.position) + FrustumPlanes[i].distance < -_boundingSphere.radius)
        //            return -1;
        //    return 1;
        //}

        int BoundsInFrustum(Bounds _bounds)
        {
            for (int i = 0; i < FrustumPlanes.Length; i++)
                if (math.dot(FrustumPlanes[i].normal, _bounds.center) + FrustumPlanes[i].distance + math.mul(_bounds.extents, math.abs(FrustumPlanes[i].normal)) < 0)
                    return -1;
            return 1;
        }
    }

    [BurstCompile]
    struct CellCullingVisibleJob : IJobFilter
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<CellCullingInfo> CellCullingInfoList;

        public bool Execute(int index)
        {
            if (CellCullingInfoList[index].Visibility == 1)
                return true;    // export the current index of the loop into the output list
            return false;   // skip
        }
    }

    [BurstCompile]
    struct CellCullingEventJob : IJobFilter
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<CellCullingInfo> CellCullingInfoList;

        public bool Execute(int index)
        {
            if (CellCullingInfoList[index].LastVisibility != CellCullingInfoList[index].Visibility)
                return true;    // export the current index of the loop into the output list
            return false;   // skip
        }
    }

    [BurstCompile]
    struct CellCullingDistanceBandEventJob : IJobFilter
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<CellCullingInfo> CellCullingInfoList;

        public bool Execute(int index)
        {
            if (CellCullingInfoList[index].CurrentDistanceBand != CellCullingInfoList[index].PreviousDistanceBand)
                return true;    // export the current index of the loop into the output list
            return false;   // skip
        }
    }
    #endregion
}