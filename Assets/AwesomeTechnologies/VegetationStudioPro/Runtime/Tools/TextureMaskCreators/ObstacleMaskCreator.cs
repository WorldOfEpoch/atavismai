using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    public enum EObstacleMaskQuality
    {
        Low1024 = 0,
        Normal2048 = 1,
        High4096 = 2,
        Ultra8192 = 3
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/TextureMaskCreators/ObstacleMaskCreator")]
    public class ObstacleMaskCreator : MonoBehaviour
    {
        public int selectedTerrainIndex;

        public EObstacleMaskQuality eObstacleMaskQuality = EObstacleMaskQuality.Normal2048;
        public Rect areaRect = new(Vector2.zero, Vector2.one);

        public bool disableOverdraw = true;

        public LayerMask firstLayer = 0;
        public bool allowTerrainCollider_first;
        [Range(0, 10)] public float minRadius_first = 0;

        public LayerMask secondLayer = 0;
        public bool allowTerrainCollider_second;
        [Range(0, 10)] public float minRadius_second = 0;

        public LayerMask thirdLayer = 0;
        public bool allowTerrainCollider_third;
        [Range(0, 10)] public float minRadius_third = 0;

        public int GetObstacleMaskResolution(EObstacleMaskQuality _obstacleMaskQuality)
        {
            return _obstacleMaskQuality switch
            {
                EObstacleMaskQuality.Low1024 => 1024,
                EObstacleMaskQuality.Normal2048 => 2048,
                EObstacleMaskQuality.High4096 => 4096,
                EObstacleMaskQuality.Ultra8192 => 8192,
                _ => 2048,
            };
        }
    }
}