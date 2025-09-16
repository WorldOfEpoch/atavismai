using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    public enum EShadowMaskQuality
    {
        Low1024 = 0,
        Normal2048 = 1,
        High4096 = 2,
        Ultra8192 = 3
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/TextureMaskCreators/ShadowMaskCreator")]
    public class ShadowMaskCreator : MonoBehaviour
    {
        public int selectedTerrainIndex;
        public readonly int cullFarStart = Shader.PropertyToID("_CullFarStart");
        public readonly int cullFarDistance = Shader.PropertyToID("_CullFarDistance");

        public EShadowMaskQuality eShadowMaskQuality = EShadowMaskQuality.Normal2048;
        public Rect areaRect = new(Vector2.zero, Vector2.one);

        [Range(0, 30)] public int invisibleLayer = 30;
        [Range(1, 10)] public float outputIntensity = 4;

        public bool includeTrees = true;
        public bool includeLargeObjects = true;

        public int GetShadowMaskResolution(EShadowMaskQuality _shadowMaskQuality)
        {
            return _shadowMaskQuality switch
            {
                EShadowMaskQuality.Low1024 => 1024,
                EShadowMaskQuality.Normal2048 => 2048,
                EShadowMaskQuality.High4096 => 4096,
                EShadowMaskQuality.Ultra8192 => 8192,
                _ => 2048,
            };
        }
    }
}