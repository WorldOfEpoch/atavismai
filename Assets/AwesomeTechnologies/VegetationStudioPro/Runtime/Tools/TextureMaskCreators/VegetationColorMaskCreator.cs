using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    public enum EVegetationColorMaskQuality
    {
        Low1024 = 0,
        Normal2048 = 1,
        High4096 = 2,
        Ultra8192 = 3
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/TextureMaskCreators/VegetationColorMaskCreator")]
    public class VegetationColorMaskCreator : MonoBehaviour
    {
        public int selectedTerrainIndex;

        public EVegetationColorMaskQuality eVegetationColorMaskQuality = EVegetationColorMaskQuality.Normal2048;
        public Rect areaRect = new(Vector2.zero, Vector2.one);

        [Range(0, 30)] public int invisibleLayer = 30;

        public bool includeGrass = true;
        public bool includePlants = true;
        public bool includeObjects;
        public bool includeLargeObjects;
        public bool includeTrees;

        public int GetVegetationColorMaskResolution(EVegetationColorMaskQuality _vegetationColorMaskQuality)
        {
            return _vegetationColorMaskQuality switch
            {
                EVegetationColorMaskQuality.Low1024 => 1024,
                EVegetationColorMaskQuality.Normal2048 => 2048,
                EVegetationColorMaskQuality.High4096 => 4096,
                EVegetationColorMaskQuality.Ultra8192 => 8192,
                _ => 2048,
            };
        }
    }
}