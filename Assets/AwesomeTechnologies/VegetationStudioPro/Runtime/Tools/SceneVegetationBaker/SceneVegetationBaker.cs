using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/SceneVegetationBaker")]
    public class SceneVegetationBaker : MonoBehaviour
    {
        public int vegetationPackageIndex;
        public bool excludeGrass = true;
        public bool excludePlants;
        public bool excludeObjects;
        public bool excludeLargeObjects;
        public bool excludeTrees;
    }
}