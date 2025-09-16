using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    public class CircleMaskArea : BaseMaskArea
    {
        public float3 Position;
        public float Radius = 0.1f;
        public VegetationType VegetationType;

        public void Init()
        {
            MaskBounds = GetMaskBounds();
        }

        public override JobHandle SampleMask(VegetationInstanceData _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (VegetationType != _vegetationType)
                return _dependsOn;

            SampleVegetationMaskCircleJob sampleVegetationMaskCircleJob = new()
            {
                MaskPosition = Position,
                Radius = Radius,
                Position = _instanceData.position,
                Included = _instanceData.included
            };
            return sampleVegetationMaskCircleJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public override JobHandle SampleMaskPersistentStorage(PersistentVegetationInfo _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            if (VegetationType != _vegetationType)
                return _dependsOn;

            SampleVegetationMaskCirclePersistentStorageJob sampleVegetatiomMaskCirclePersistentStorageJob = new()
            {
                instanceData = _instanceData.NativeVegetationItemList,
                MaskPosition = Position,
                Radius = Radius,
            };
            return sampleVegetatiomMaskCirclePersistentStorageJob.Schedule(_instanceData.NativeVegetationItemList, 64, _dependsOn);
        }

        private Bounds GetMaskBounds()
        {
            Bounds b = new(Position, Vector3.one);
            b.Expand(GetMaxAdditionalDistance());
            return b;
        }
    }

    #region jobs
    [BurstCompile]
    public struct SampleVegetationMaskCircleJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [ReadOnly] public float Radius;
        [ReadOnly] public float3 MaskPosition;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (math.distance(new float2(Position[index].x, Position[index].z), new float2(MaskPosition.x, MaskPosition.z)) <= Radius)
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct SampleVegetationMaskCirclePersistentStorageJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<PersistentVegetationItem> instanceData;
        [ReadOnly] public float Radius;
        [ReadOnly] public float3 MaskPosition;

        public void Execute(int index)
        {
            if (index > instanceData.Length - 1)
                return;

            if (math.distance(new float2(instanceData[index].Position.x, instanceData[index].Position.z), new float2(MaskPosition.x, MaskPosition.z)) <= Radius)
                instanceData[index] = new PersistentVegetationItem { Rotation = quaternion.identity, DistanceFalloff = -1 };    // set to "-1" to exclude from rendering
        }
    }
    #endregion
}