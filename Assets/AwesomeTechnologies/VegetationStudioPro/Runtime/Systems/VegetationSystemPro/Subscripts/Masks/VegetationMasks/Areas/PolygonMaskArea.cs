using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    public class PolygonMaskArea : BaseMaskArea
    {
        private float2[] points2D;
        private float3[] points3D;
        private LineSegment2D[] segments;

        private NativeArray<float2> polygonArray;
        private NativeArray<LineSegment2D> segmentArray;

        public void AddPolygon(List<float3> _pointList)
        {
            if (_pointList.Count <= 0)
                return;

            points2D = new float2[_pointList.Count];
            points3D = new float3[_pointList.Count];
            for (int i = 0; i < _pointList.Count; i++)
            {
                points2D[i] = new float2(_pointList[i].x, _pointList[i].z);
                points3D[i] = _pointList[i];
            }

            MaskBounds = GetMaskBounds();

            if (polygonArray.IsCreated)
                polygonArray.Dispose();
            polygonArray = new NativeArray<float2>(points2D.Length, Allocator.Persistent);  // persistent as used by several jobs across several (sub-)systems
            polygonArray.CopyFromFast(points2D);

            CreateSegments();
        }

        void CreateSegments()
        {
            segments = new LineSegment2D[points2D.Length];

            for (int i = 0; i < points2D.Length - 1; i++)
                segments[i] = new LineSegment2D(points2D[i], points2D[i + 1]);

            if (points2D.Length > 0)
                segments[points2D.Length - 1] = new LineSegment2D(points2D[0], points2D[points2D.Length - 1]);

            if (segmentArray.IsCreated)
                segmentArray.Dispose();
            segmentArray = new NativeArray<LineSegment2D>(segments.Length, Allocator.Persistent);   // persistent as used by several jobs across several (sub-)systems
            segmentArray.CopyFromFast(segments);
        }

        public override JobHandle SampleMask(VegetationInstanceData _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (ExcludeVegetationType(_vegetationType) == false)
                return _dependsOn;

            SampleVegetationMaskPolygonJob sampleVegetationMaskPolygonJob = new()
            {
                Position = _instanceData.position,
                Scale = _instanceData.scale,
                Included = _instanceData.included,

                PolygonArray = polygonArray,
                SegmentArray = segmentArray,
                AdditionalWidth = GetAdditionalWidth(_vegetationType),
                AdditionalWidthMax = GetAdditionalWidthMax(_vegetationType),
                NoiseScale = GetPerlinScale(_vegetationType),
                Bounds = _bounds,
                eMaskRadiusType = GetSafetyRadiusType(_vegetationType)
            };
            return sampleVegetationMaskPolygonJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public override JobHandle SampleMaskPersistentStorage(PersistentVegetationInfo _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (ExcludeVegetationType(_vegetationType) == false)
                return _dependsOn;

            SampleVegetationMaskPersistentStoragePolygonJob sampleVegetationMaskPersistentStoragePolygonJob = new()
            {
                instanceData = _instanceData.NativeVegetationItemList,

                PolygonArray = polygonArray,
                SegmentArray = segmentArray,
                AdditionalWidth = GetAdditionalWidth(_vegetationType),
                AdditionalWidthMax = GetAdditionalWidthMax(_vegetationType),
                NoiseScale = GetPerlinScale(_vegetationType),
                Bounds = _bounds,
                eMaskRadiusType = GetSafetyRadiusType(_vegetationType)
            };
            return sampleVegetationMaskPersistentStoragePolygonJob.Schedule(_instanceData.NativeVegetationItemList, 64, _dependsOn);
        }

        public override JobHandle SampleIncludeVegetationMask(VegetationInstanceData _instanceData, VegetationTypeIndex _vegetationTypeIndex, JobHandle _dependsOn)
        {
            VegetationTypeSettings vegetationTypeSettings = GetVegetationTypeSettings(_vegetationTypeIndex);
            if (vegetationTypeSettings == null)
                return _dependsOn;

            IncludeVegetationMaskPolygonJob includeVegetatiomMaskPolygonJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,
                Included = _instanceData.included,

                PolygonArray = polygonArray,
                Scale = vegetationTypeSettings.Size,
                Density = vegetationTypeSettings.Density
            };
            return includeVegetatiomMaskPolygonJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public override bool HasVegetationTypeIndex(VegetationTypeIndex _vegetationTypeIndex)
        {
            for (int i = 0; i < VegetationTypeList.Count; i++)
                if (VegetationTypeList[i].Index == _vegetationTypeIndex)
                    return true;
            return false;
        }

        private Bounds GetMaskBounds()
        {
            Bounds expandedBounds = points3D.Length > 0 ? new Bounds(points3D[0], new float3(1, 1, 1)) : new Bounds(new float3(0, 0, 0), new float3(1, 1, 1));
            for (int i = 0; i < points3D.Length; i++)
                expandedBounds.Encapsulate(points3D[i]);
            expandedBounds.Expand(GetMaxAdditionalDistance());
            return expandedBounds;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (polygonArray.IsCreated) polygonArray.Dispose();
            if (segmentArray.IsCreated) segmentArray.Dispose();
        }
    }

    #region jobs
    [BurstCompile]
    public struct SampleVegetationMaskPolygonJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [ReadOnly] public NativeArray<float2> PolygonArray;
        [ReadOnly] public NativeArray<LineSegment2D> SegmentArray;
        [ReadOnly] public float AdditionalWidth;
        [ReadOnly] public float AdditionalWidthMax;
        [ReadOnly] public float NoiseScale;
        [ReadOnly] public Bounds Bounds;
        [ReadOnly] public EMaskRadiusType eMaskRadiusType;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            float2 position = new(Position[index].x, Position[index].z);

            float perlin = noise.cnoise(new float2(position.x / NoiseScale, position.y / NoiseScale));
            perlin += 1f;
            perlin /= 2f;
            perlin = math.clamp(perlin, 0, 1);

            float boundsRadius = 0;
            if (eMaskRadiusType != EMaskRadiusType.center)
                boundsRadius = math.select(math.max(Bounds.extents.x * Scale[index].x, Bounds.extents.z * Scale[index].z), math.length(new float2(Bounds.extents.x * Scale[index].x, Bounds.extents.z * Scale[index].z)),
                    eMaskRadiusType == EMaskRadiusType.outerRadius);

            float additionalWidth = math.lerp(AdditionalWidth, AdditionalWidthMax, perlin);
            if (IsInPolygon(position) || (DistanceToEdge(position) < additionalWidth + boundsRadius))
                Included[index] = 0;
        }

        private float DistanceToEdge(float2 _point)
        {
            float distance = float.MaxValue;
            for (int i = 0; i < SegmentArray.Length; i++)
                distance = math.min(distance, SegmentArray[i].DistanceToPoint(_point));
            return distance;
        }

        private bool IsInPolygon(float2 _point)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
                return false;

            float2 oldPoint = new(PolygonArray[^1].x, PolygonArray[^1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                if (PolygonArray[i].x > oldPoint.x)
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - oldPoint.y) * (PolygonArray[i].x - oldPoint.x) < (PolygonArray[i].y - oldPoint.y) * (_point.x - oldPoint.x))
                        inside = !inside;
                }
                else
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - PolygonArray[i].y) * (oldPoint.x - PolygonArray[i].x) < (oldPoint.y - PolygonArray[i].y) * (_point.x - PolygonArray[i].x))
                        inside = !inside;
                }

                oldPoint = PolygonArray[i];
            }

            return inside;
        }
    }

    [BurstCompile]
    public struct SampleVegetationMaskPersistentStoragePolygonJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<PersistentVegetationItem> instanceData;

        [ReadOnly] public NativeArray<float2> PolygonArray;
        [ReadOnly] public NativeArray<LineSegment2D> SegmentArray;
        [ReadOnly] public float AdditionalWidth;
        [ReadOnly] public float AdditionalWidthMax;
        [ReadOnly] public float NoiseScale;
        [ReadOnly] public Bounds Bounds;
        [ReadOnly] public EMaskRadiusType eMaskRadiusType;

        public void Execute(int index)
        {
            if (index > instanceData.Length - 1)
                return;

            float2 position = new(instanceData[index].Position.x, instanceData[index].Position.z);

            float perlin = noise.cnoise(new float2(position.x / NoiseScale, position.y / NoiseScale));
            perlin += 1f;
            perlin /= 2f;
            perlin = math.clamp(perlin, 0, 1);

            float boundsRadius = 0;
            if (eMaskRadiusType != EMaskRadiusType.center)
                boundsRadius = math.select(math.max(Bounds.extents.x * instanceData[index].Scale.x, Bounds.extents.z * instanceData[index].Scale.z), math.length(new float2(Bounds.extents.x * instanceData[index].Scale.x, Bounds.extents.z * instanceData[index].Scale.z)),
                    eMaskRadiusType == EMaskRadiusType.outerRadius);

            float additionalWidth = math.lerp(AdditionalWidth, AdditionalWidthMax, perlin);
            if (IsInPolygon(position) || (DistanceToEdge(position) < additionalWidth + boundsRadius))
                instanceData[index] = new PersistentVegetationItem { Rotation = quaternion.identity, DistanceFalloff = -1 };    // set to "-1" to exclude from rendering
        }

        private float DistanceToEdge(float2 _point)
        {
            float distance = float.MaxValue;
            for (int i = 0; i < SegmentArray.Length; i++)
                distance = math.min(distance, SegmentArray[i].DistanceToPoint(_point));
            return distance;
        }

        private bool IsInPolygon(float2 _point)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
                return false;

            float2 oldPoint = new(PolygonArray[^1].x, PolygonArray[^1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                if (PolygonArray[i].x > oldPoint.x)
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - oldPoint.y) * (PolygonArray[i].x - oldPoint.x) < (PolygonArray[i].y - oldPoint.y) * (_point.x - oldPoint.x))
                        inside = !inside;
                }
                else
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - PolygonArray[i].y) * (oldPoint.x - PolygonArray[i].x) < (oldPoint.y - PolygonArray[i].y) * (_point.x - PolygonArray[i].x))
                        inside = !inside;
                }

                oldPoint = PolygonArray[i];
            }

            return inside;
        }
    }

    [BurstCompile]
    public struct IncludeVegetationMaskPolygonJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;    // x = scale -- y = density
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public NativeArray<float2> PolygonArray;
        [ReadOnly] public float Scale;
        [ReadOnly] public float Density;

        public void Execute(int index)
        {
            if (Included[index] == 0 || IsInPolygon(new float2(Position[index].x, Position[index].z)) == false)
                return;

            ControlData[index] = new float2(math.max(ControlData[index].x, Scale), math.max(ControlData[index].y, Density));    // use the highest scale/density of all overlapping masks
        }

        private bool IsInPolygon(float2 _point)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
                return false;

            float2 oldPoint = new(PolygonArray[^1].x, PolygonArray[^1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                if (PolygonArray[i].x > oldPoint.x)
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - oldPoint.y) * (PolygonArray[i].x - oldPoint.x) < (PolygonArray[i].y - oldPoint.y) * (_point.x - oldPoint.x))
                        inside = !inside;
                }
                else
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - PolygonArray[i].y) * (oldPoint.x - PolygonArray[i].x) < (oldPoint.y - PolygonArray[i].y) * (_point.x - PolygonArray[i].x))
                        inside = !inside;
                }

                oldPoint = PolygonArray[i];
            }

            return inside;
        }
    }
    #endregion
}