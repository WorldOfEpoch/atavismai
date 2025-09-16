using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    public enum EBackgroundMaskQuality
    {
        Low1024 = 0,
        Normal2048 = 1,
        High4096 = 2,
        Ultra8096 = 3
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/TextureMaskCreators/BackgroundMaskCreator")]
    public class BackgroundMaskCreator : MonoBehaviour
    {
        public int selectedTerrainIndex;

        public EBackgroundMaskQuality eBackgroundMaskQuality = EBackgroundMaskQuality.Normal2048;
        public Rect areaRect = new(Vector2.zero, Vector2.one);

        public int GetBackgroundMaskResolution(EBackgroundMaskQuality _backgroundMaskQuality)
        {
            return _backgroundMaskQuality switch
            {
                EBackgroundMaskQuality.Low1024 => 1024,
                EBackgroundMaskQuality.Normal2048 => 2048,
                EBackgroundMaskQuality.High4096 => 4096,
                EBackgroundMaskQuality.Ultra8096 => 8096,
                _ => 2048,
            };
        }
    }
}