using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    public class LineMaskArea : BaseMaskArea
    {
        private LineSegment2D line2D;

        private float width;
        private float3 point1;
        private float3 point2;
        private float3 centerPoint;

        public void SetLineData(float3 _point1, float3 _point2, float _width)
        {
            width = _width;
            point1 = _point1;
            point2 = _point2;
            centerPoint = math.lerp(_point1, _point2, 0.5f);
            line2D = new LineSegment2D(new float2(_point1.x, _point1.z), new float2(_point2.x, _point2.z));

            MaskBounds = GetMaskBounds();
        }

        public override bool HasVegetationTypeIndex(VegetationTypeIndex _vegetationTypeIndex)
        {
            for (int i = 0; i < VegetationTypeList.Count; i++)
                if (VegetationTypeList[i].Index == _vegetationTypeIndex)
                    return true;
            return false;
        }

        public override JobHandle SampleMask(VegetationInstanceData _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (ExcludeVegetationType(_vegetationType) == false)
                return _dependsOn;

            SampleVegetationMaskLineJob sampleVegetationMaskLineJob = new()
            {
                Position = _instanceData.position,
                Scale = _instanceData.scale,
                Included = _instanceData.included,

                LineSegment2D = line2D,
                Width = width,
                AdditionalWidth = GetAdditionalWidth(_vegetationType),
                AdditionalWidthMax = GetAdditionalWidthMax(_vegetationType),
                NoiseScale = GetPerlinScale(_vegetationType),
                Bounds = _bounds,
                eMaskRadiusType = GetSafetyRadiusType(_vegetationType)
            };
            return sampleVegetationMaskLineJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public override JobHandle SampleMaskPersistentStorage(PersistentVegetationInfo _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (ExcludeVegetationType(_vegetationType) == false)
                return _dependsOn;

            SampleVegetationMaskLinePersistentStorageJob sampleVegetationMaskLinePersistentStorageJob = new()
            {
                instanceData = _instanceData.NativeVegetationItemList,

                LineSegment2D = line2D,
                Width = width,
                AdditionalWidth = GetAdditionalWidth(_vegetationType),
                AdditionalWidthMax = GetAdditionalWidthMax(_vegetationType),
                NoiseScale = GetPerlinScale(_vegetationType),
                Bounds = _bounds,
                eMaskRadiusType = GetSafetyRadiusType(_vegetationType)
            };
            return sampleVegetationMaskLinePersistentStorageJob.Schedule(_instanceData.NativeVegetationItemList, 64, _dependsOn);
        }

        public override JobHandle SampleIncludeVegetationMask(VegetationInstanceData _instanceData, VegetationTypeIndex _vegetationTypeIndex, JobHandle _dependsOn)
        {
            VegetationTypeSettings vegetationTypeSettings = GetVegetationTypeSettings(_vegetationTypeIndex);
            if (vegetationTypeSettings == null)
                return _dependsOn;

            IncludeVegetationMaskLineJob includeVegetationMaskLineJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,
                Included = _instanceData.included,

                LineSegment2D = line2D,
                Scale = vegetationTypeSettings.Size,
                Density = vegetationTypeSettings.Density,
                Width = width
            };
            return includeVegetationMaskLineJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public Bounds GetMaskBounds()
        {
            Bounds expandedBounds = new(centerPoint, new float3(1, 1, 1));
            expandedBounds.Encapsulate(point1);
            expandedBounds.Encapsulate(point2);
            expandedBounds.Expand(width);
            expandedBounds.Expand(GetMaxAdditionalDistance());
            return expandedBounds;
        }
    }

    #region jobs
    [BurstCompile]
    public struct SampleVegetationMaskLineJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [ReadOnly] public LineSegment2D LineSegment2D;
        [ReadOnly] public float AdditionalWidth;
        [ReadOnly] public float AdditionalWidthMax;
        [ReadOnly] public float NoiseScale;
        [ReadOnly] public float Width;
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
            if (LineSegment2D.DistanceToPoint(position) < (additionalWidth + (Width * 0.5f) + boundsRadius))
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct SampleVegetationMaskLinePersistentStorageJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<PersistentVegetationItem> instanceData;

        [ReadOnly] public LineSegment2D LineSegment2D;
        [ReadOnly] public float AdditionalWidth;
        [ReadOnly] public float AdditionalWidthMax;
        [ReadOnly] public float NoiseScale;
        [ReadOnly] public float Width;
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
            if (LineSegment2D.DistanceToPoint(position) < (additionalWidth + (Width * 0.5f) + boundsRadius))
                instanceData[index] = new PersistentVegetationItem { Rotation = quaternion.identity, DistanceFalloff = -1 };    // set to "-1" to exclude from rendering
        }
    }

    [BurstCompile]
    public struct IncludeVegetationMaskLineJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;    // x = scale -- y = density
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public LineSegment2D LineSegment2D;
        [ReadOnly] public float Scale;
        [ReadOnly] public float Density;
        [ReadOnly] public float Width;

        public void Execute(int index)
        {
            if (Included[index] == 0 || (LineSegment2D.DistanceToPoint(new float2(Position[index].x, Position[index].z)) >= Width * 0.5f))
                return;

            ControlData[index] = new float2(math.max(ControlData[index].x, Scale), math.max(ControlData[index].y, Density));    // use the highest scale/density of all overlapping masks
        }
    }
    #endregion
}