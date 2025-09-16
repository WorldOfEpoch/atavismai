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
    public class BeaconMaskArea : BaseMaskArea
    {
        public float Radius;
        public float blendFactor;
        public float3 Position;
        public NativeArray<float> FalloutCurveArray;
        public VegetationType vegetationType;

        public void Init()
        {
            MaskBounds = GetMaskBounds();
        }

        public void SetFalloffCurve(float[] _curveArray)
        {
            if (FalloutCurveArray.IsCreated)
                FalloutCurveArray.Dispose();
            FalloutCurveArray = new NativeArray<float>(_curveArray.Length, Allocator.Persistent);
            FalloutCurveArray.CopyFromFast(_curveArray);
        }

        public override JobHandle SampleMask(VegetationInstanceData _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (ExcludeVegetationType(_vegetationType) == false)
                return _dependsOn;

            SampleVegetationMaskBeaconJob sampleVegetationMaskBeaconJob = new()
            {
                Position = _instanceData.position,
                Included = _instanceData.included,
                MaskPosition = Position,
                Radius = Radius,
                BlendFactor = blendFactor
            };
            return sampleVegetationMaskBeaconJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public override JobHandle SampleMaskPersistentStorage(PersistentVegetationInfo _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (ExcludeVegetationType(_vegetationType) == false)
                return _dependsOn;

            SampleVegetationMaskBeaconPersistentStorageJob sampleVegetationMaskBeaconJob = new()
            {
                instanceData = _instanceData.NativeVegetationItemList,
                MaskPosition = Position,
                Radius = Radius,
                BlendFactor = blendFactor
            };
            return sampleVegetationMaskBeaconJob.Schedule(_instanceData.NativeVegetationItemList, 64, _dependsOn);
        }

        public override JobHandle SampleIncludeVegetationMask(VegetationInstanceData _instanceData, VegetationTypeIndex _vegetationTypeIndex, JobHandle _dependsOn)
        {
            VegetationTypeSettings vegetationTypeSettings = GetVegetationTypeSettings(_vegetationTypeIndex);
            if (vegetationTypeSettings == null)
                return _dependsOn;

            IncludeVegetationMaskBeaconJob includeVegetationMaskBeaconJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,
                Included = _instanceData.included,

                FalloffCurveArray = FalloutCurveArray,
                MaskPosition = Position,
                Radius = Radius,
                Scale = vegetationTypeSettings.Size,
                Density = vegetationTypeSettings.Density
            };
            return includeVegetationMaskBeaconJob.Schedule(_instanceData.included, 64, _dependsOn);
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
            Bounds b = new(Position, Radius * 2 * Vector3.one);
            b.Expand(GetMaxAdditionalDistance());
            return b;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (FalloutCurveArray.IsCreated)
                FalloutCurveArray.Dispose();
        }
    }

    #region jobs
    [BurstCompile]
    public struct SampleVegetationMaskBeaconJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [ReadOnly] public float Radius;
        [ReadOnly] public float BlendFactor;
        [ReadOnly] public float3 MaskPosition;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            float distance = math.distance(new float2(Position[index].x, Position[index].z), new float2(MaskPosition.x, MaskPosition.z));
            if (distance <= Radius * BlendFactor)
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct SampleVegetationMaskBeaconPersistentStorageJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<PersistentVegetationItem> instanceData;
        [ReadOnly] public float Radius;
        [ReadOnly] public float BlendFactor;
        [ReadOnly] public float3 MaskPosition;

        public void Execute(int index)
        {
            if (index > instanceData.Length - 1)
                return;

            float distance = math.distance(new float2(instanceData[index].Position.x, instanceData[index].Position.z), new float2(MaskPosition.x, MaskPosition.z));
            if (distance <= Radius * BlendFactor)
                instanceData[index] = new PersistentVegetationItem { Rotation = quaternion.identity, DistanceFalloff = -1 };    // set to "-1" to exclude from rendering
        }
    }

    [BurstCompile]
    public struct IncludeVegetationMaskBeaconJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;    // x = scale -- y = density
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public NativeArray<float> FalloffCurveArray;
        [ReadOnly] public float3 MaskPosition;
        [ReadOnly] public float Radius;
        [ReadOnly] public float Scale;
        [ReadOnly] public float Density;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            float distance = math.distance(new float2(Position[index].x, Position[index].z), new float2(MaskPosition.x, MaskPosition.z));
            if (distance < Radius)
                ControlData[index] = new float2(math.max(ControlData[index].x, Scale), math.max(ControlData[index].y, Density * SampleFalloffCurveArray(distance / Radius)));   // use the highest scale/density of all overlapping masks
        }

        private float SampleFalloffCurveArray(float _value)
        {
            if (FalloffCurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * FalloffCurveArray.Length);
            index = math.clamp(index, 0, FalloffCurveArray.Length - 1);

            return FalloffCurveArray[index];
        }
    }
    #endregion
}