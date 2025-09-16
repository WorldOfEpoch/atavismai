using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [BurstCompile]
    public struct VegetationMaskIncludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] public NativeList<int> RandomNumberIndex;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float2> ControlData; // x = scale -- y = density
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public NativeArray<float> RandomNumbers;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (RandomCutoff(ControlData[index].y, RandomNumberIndex[index]))
                Included[index] = 0;    // only spawn inside of include masks -- randomize what is included based on the density, per (overlapped) mask (w/ highest value)
            else
                Scale[index] *= ControlData[index].x;   // scale up items inside of the include mask, per (overlapped) mask (w/ highest value)
            RandomNumberIndex[index]++; // last increase to not mess/align with "DistanceFalloff" -- avoid fall-off behavior when changing "density"
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }

        private bool RandomCutoff(float _value, int _randomNumberIndex)
        {
            return !(_value > RandomRange(_randomNumberIndex, 0, 1));
        }
    }

    public enum EMaskRadiusType
    {
        off = -1,
        center,
        innerRadius,
        outerRadius
    }

    public class BaseMaskArea
    {
        public string VegetationItemID = "";
        public Bounds MaskBounds;

        public bool RemoveGrass = true;
        public bool RemovePlants = true;
        public bool RemoveTrees = true;
        public bool RemoveObjects = true;
        public bool RemoveLargeObjects = true;

        public float AdditionalGrassWidth = 0;
        public float AdditionalPlantWidth = 0;
        public float AdditionalTreeWidth = 0;
        public float AdditionalObjectWidth = 0;
        public float AdditionalLargeObjectWidth = 0;

        public float AdditionalGrassWidthMax = 0;
        public float AdditionalPlantWidthMax = 0;
        public float AdditionalTreeWidthMax = 0;
        public float AdditionalObjectWidthMax = 0;
        public float AdditionalLargeObjectWidthMax = 0;

        public float NoiseScaleGrass = 1;
        public float NoiseScalePlant = 1;
        public float NoiseScaleTree = 1;
        public float NoiseScaleObject = 1;
        public float NoiseScaleLargeObject = 1;

        public EMaskRadiusType eMaskRadiusGrass = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusPlant = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusTree = EMaskRadiusType.center;
        public EMaskRadiusType eMaskRadiusObject = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusLargeObject = EMaskRadiusType.outerRadius;

        public List<VegetationTypeSettings> VegetationTypeList = new();

        public delegate void MultionMaskDeleteDelegate(BaseMaskArea _baseMaskArea);
        public MultionMaskDeleteDelegate OnMaskDeleteDelegate;

        public virtual JobHandle SampleMask(VegetationInstanceData _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            return _dependsOn;
        }

        public virtual JobHandle SampleMaskPersistentStorage(PersistentVegetationInfo _instanceData, VegetationType _vegetationType, JobHandle _dependsOn, Bounds _bounds)
        {
            return _dependsOn;
        }

        public virtual JobHandle SampleIncludeVegetationMask(VegetationInstanceData _instanceData, VegetationTypeIndex _vegetationTypeIndex, JobHandle _dependsOn)
        {
            return _dependsOn;
        }

        public virtual bool HasVegetationTypeIndex(VegetationTypeIndex _vegetationTypeIndex)
        {
            return false;
        }

        public float GetAdditionalWidth(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => AdditionalGrassWidth,
                VegetationType.Plant => AdditionalPlantWidth,
                VegetationType.Tree => AdditionalTreeWidth,
                VegetationType.Objects => AdditionalObjectWidth,
                VegetationType.LargeObjects => AdditionalLargeObjectWidth,
                _ => 0,
            };
        }

        public VegetationTypeSettings GetVegetationTypeSettings(VegetationTypeIndex _vegetationTypeIndex)
        {
            for (int i = 0; i < VegetationTypeList.Count; i++)
                if (VegetationTypeList[i].Index == _vegetationTypeIndex)
                    return VegetationTypeList[i];
            return null;
        }

        public bool ExcludeVegetationType(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => RemoveGrass,
                VegetationType.Plant => RemovePlants,
                VegetationType.Tree => RemoveTrees,
                VegetationType.Objects => RemoveObjects,
                VegetationType.LargeObjects => RemoveLargeObjects,
                _ => false,
            };
        }

        public float GetAdditionalWidthMax(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => AdditionalGrassWidthMax,
                VegetationType.Plant => AdditionalPlantWidthMax,
                VegetationType.Tree => AdditionalTreeWidthMax,
                VegetationType.Objects => AdditionalObjectWidthMax,
                VegetationType.LargeObjects => AdditionalLargeObjectWidthMax,
                _ => 0,
            };
        }

        public float GetPerlinScale(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => NoiseScaleGrass,
                VegetationType.Plant => NoiseScalePlant,
                VegetationType.Tree => NoiseScaleTree,
                VegetationType.Objects => NoiseScaleObject,
                VegetationType.LargeObjects => NoiseScaleLargeObject,
                _ => 0,
            };
        }

        public EMaskRadiusType GetSafetyRadiusType(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => eMaskRadiusGrass,
                VegetationType.Plant => eMaskRadiusPlant,
                VegetationType.Tree => eMaskRadiusTree,
                VegetationType.Objects => eMaskRadiusObject,
                VegetationType.LargeObjects => eMaskRadiusLargeObject,
                _ => EMaskRadiusType.center,
            };
        }

        //public virtual bool Contains(Vector3 _point, VegetationType _vegetationType, bool _useAdditionalDistance, bool _useExcludeFilter)
        //{
        //    return false;
        //}

        //public virtual bool ContainsMask(Vector3 _point, VegetationType _vegetationType, VegetationTypeIndex _vegetationTypeIndex, ref float _size, ref float _density)
        //{
        //    bool hasVegetationType = HasVegetationType(_vegetationTypeIndex, ref _size, ref _density);
        //    if (hasVegetationType == false)
        //      return false;
        //    return Contains(_point, _vegetationType, false, false);
        //}

        //public bool HasVegetationType(VegetationTypeIndex _vegetationTypeIndex, ref float _size, ref float _density)
        //{
        //    for (int i = 0; i < VegetationTypeList.Count; i++)
        //        if (VegetationTypeList[i].Index == vegetationTypeIndex)
        //        {
        //            size = VegetationTypeList[i].Size;
        //            density = VegetationTypeList[i].Density;
        //            return true;
        //        }
        //    return false;
        //}

        public float GetMaxAdditionalDistance() // "~workaround"
        {
            // get dynamic value for accurately masking out vegetation instances that are offset out of their origin cell due to randomization -- calculate the max possible random offset using the first vegetation system in the list
            if (VegetationStudioManager.Instance != null)
                if (VegetationStudioManager.Instance.VegetationSystemList.Count > 0)
                    return math.length(new float2(VegetationStudioManager.Instance.VegetationSystemList[0].vegetationCellSize / 4, VegetationStudioManager.Instance.VegetationSystemList[0].vegetationCellSize / 4));
            return 0;   // editor mode workaround
        }

        public float SamplePerlinNoise(Vector3 _point, float _perlinNoiseScale)
        {
            return noise.cnoise(new float2(_point.x / _perlinNoiseScale, _point.z / _perlinNoiseScale));
        }

        public void CallDeleteEvent()
        {
            OnMaskDeleteDelegate?.Invoke(this);
        }

        public virtual void Dispose()
        {

        }
    }
}