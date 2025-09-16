using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [System.Serializable]
    public class VegetationTypeSettings
    {
        public VegetationTypeIndex Index = VegetationTypeIndex.VegetationType1;
        [Range(0, 5)] public float Density = 1;
        [Range(0.1f, 5)] public float Size = 1;

        public VegetationTypeSettings()
        {

        }

        public VegetationTypeSettings(VegetationTypeSettings _vegetationTypeSettings)
        {
            Index = _vegetationTypeSettings.Index;
            Density = _vegetationTypeSettings.Density;
            Size = _vegetationTypeSettings.Size;
        }
    }
}