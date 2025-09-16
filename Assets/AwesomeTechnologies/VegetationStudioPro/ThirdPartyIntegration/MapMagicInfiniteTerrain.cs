#if VEGETATION_STUDIO_PRO && VSP_PACKAGES
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.External.MapMagicInterface
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/ThirdPartyIntegration/MapMagicInfiniteTerrain")]
    public class MapMagicInfiniteTerrain : MonoBehaviour
    {
#if MAPMAGIC
        void OnEnable()
        {
            MapMagic.MapMagic.OnApplyCompleted += OnGenerationCompleted;
        }

        void OnDisable()
        {
            MapMagic.MapMagic.OnApplyCompleted -= OnGenerationCompleted;
        }
#endif

        void OnGenerationCompleted(Terrain _terrain)
        {
            UnityTerrain unityTerrain = _terrain.gameObject.GetComponent<UnityTerrain>();
            if (unityTerrain == null)
                unityTerrain = _terrain.gameObject.AddComponent<UnityTerrain>();
            unityTerrain.AutoAddToVegetationSystem = true;
            VegetationStudioManager.AddTerrain(unityTerrain.gameObject, false, null);
        }
    }
}
#endif