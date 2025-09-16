#if UNITY_EDITOR
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
// TerrainAPI has moved out of the experimental namespace in newer Unity versions.
// Use UnityEditor.TerrainTools instead of the experimental namespace when available.
using UnityEditor.TerrainTools;
#endif

namespace Gaia
{
    public class NoiseComponent : MonoBehaviour
    {
        public Material mat;
        public NoiseSettings noiseSettings;

        void Update()
        {
            if (mat != null)
            {
                noiseSettings.SetupMaterial(mat);
            }
        }
    }
}
#endif