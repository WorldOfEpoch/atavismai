using Unity.Mathematics;

namespace AwesomeTechnologies.VegetationSystem
{
    [System.Serializable]
    public class VegetationSettings
    {
        public float crossFadeDistance = 10;
        public float grassDistance = 175;
        public float plantDistance = 225;
        public float objectDistance = 225;
        public float largeObjectDistance = 1000;
        public float treeDistance = 340;
        public float billboardDistanceFactor = 1;
        public bool disableLocalDistanceFactor = false;

        public int seed = 42;
        public float grassDensity = 1;
        public float plantDensity = 1;
        public float objectDensity = 1;
        public float largeObjectDensity = 1;
        public float treeDensity = 1;

        #region culling distances
        public float GetGrassDistance()
        {
            return grassDistance;
        }

        public float GetPlantDistance()
        {
            return plantDistance;
        }

        public float GetObjectDistance()
        {
            return objectDistance;
        }

        public float GetLargeObjectDistance()
        {
            return largeObjectDistance;
        }

        public float GetTreeDistance()
        {
            return treeDistance;
        }

        public float GetVegetationItemCullDistance(VegetationItemInfoPro _vegItemInfoPro, bool _useLocalFactor = true)
        {
            return _vegItemInfoPro.VegetationType switch
            {
                VegetationType.Grass => GetGrassDistance() * (_useLocalFactor ? (disableLocalDistanceFactor ? 1 : _vegItemInfoPro.RenderDistanceFactor) : 1),
                VegetationType.Plant => GetPlantDistance() * (_useLocalFactor ? (disableLocalDistanceFactor ? 1 : _vegItemInfoPro.RenderDistanceFactor) : 1),
                VegetationType.Tree => GetTreeDistance() * (_useLocalFactor ? (disableLocalDistanceFactor ? 1 : _vegItemInfoPro.RenderDistanceFactor) : 1),
                VegetationType.Objects => GetObjectDistance() * (_useLocalFactor ? (disableLocalDistanceFactor ? 1 : _vegItemInfoPro.RenderDistanceFactor) : 1),
                VegetationType.LargeObjects => GetLargeObjectDistance() * (_useLocalFactor ? (disableLocalDistanceFactor ? 1 : _vegItemInfoPro.RenderDistanceFactor) : 1),
                _ => 0,
            };
        }

        public float GetBillboardDistance(float _camFarClipDistance)
        {
            return billboardDistanceFactor * _camFarClipDistance;
        }

        public float GetLowerDistanceBandDistance()
        {
            float max = math.max(grassDistance, plantDistance);
            return math.max(max, objectDistance) + crossFadeDistance;
        }

        public float GetHigherDistanceBandDistance()
        {
            return math.max(largeObjectDistance, treeDistance) + crossFadeDistance;
        }

        public float GetFurthestVegetationDistance()
        {
            float max = math.max(grassDistance, plantDistance);
            max = math.max(max, objectDistance);
            max = math.max(max, largeObjectDistance);
            return math.max(max, treeDistance) + crossFadeDistance;
        }

        public float GetClosestVegetationDistance()
        {
            float min = math.min(grassDistance, plantDistance);
            min = math.min(min, objectDistance);
            min = math.min(min, largeObjectDistance);
            return math.min(min, treeDistance) + crossFadeDistance;
        }
        #endregion

        #region densities
        public float GetVegetationItemDensity(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => grassDensity,
                VegetationType.Plant => plantDensity,
                VegetationType.Objects => objectDensity,
                VegetationType.LargeObjects => largeObjectDensity,
                VegetationType.Tree => treeDensity,
                _ => 1,
            };
        }
        #endregion
    }
}