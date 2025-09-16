using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class FAEFoliageShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "FAE/Foliage"
        };

        public bool MatchShader(string _shaderName)
        {
            if (string.IsNullOrEmpty(_shaderName))
                return false;

            for (int i = 0; i < ShaderNames.Length; i++)
                if (ShaderNames[i] == _shaderName)
                    return true;

            return false;
        }

        public bool MatchBillboardShader(Material _material)
        {
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "Fantasy Adventure Environment Foliage",
                supportsInstancedIndirect = true
            };

            Settings.AddLabelProperty("Color");
            Settings.AddFloatProperty("AmbientOcclusion", "Ambient Occlusion", "", ShaderUtility.GetFloatFromMaterials(_materials, "_AmbientOcclusion"), 0, 1);

            Settings.AddLabelProperty("Translucency");
            Settings.AddFloatProperty("TranslucencyAmount", "Amount", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TransmissionAmount"), 0, 10);
            Settings.AddFloatProperty("TranslucencySize", "Size", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TransmissionSize"), 1, 20);

            Settings.AddLabelProperty("Wind");
            Settings.AddFloatProperty("WindInfluence", "Influence", "", ShaderUtility.GetFloatFromMaterials(_materials, "_MaxWindStrength"), 0, 1);
            Settings.AddFloatProperty("GlobalWindMotion", "Global motion", "", ShaderUtility.GetFloatFromMaterials(_materials, "_GlobalWindMotion"), 0, 1);
            Settings.AddFloatProperty("LeafFlutter", "Leaf flutter", "", ShaderUtility.GetFloatFromMaterials(_materials, "_LeafFlutter"), 0, 1);
            Settings.AddFloatProperty("WindSwinging", "Swinging", "", ShaderUtility.GetFloatFromMaterials(_materials, "_WindSwinging"), 0, 1);
            Settings.AddFloatProperty("WindAmplitude", "Amplitude Multiplier", "", ShaderUtility.GetFloatFromMaterials(_materials, "_WindAmplitudeMultiplier"), 0, 10);
        }

        public void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro)
        {
            if (Settings == null)
                return;

            for (int i = 0; i < ShaderNames.Length; i++)
                if (_material.shader.name == ShaderNames[i])
                {
                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial"

                    _material.SetFloat("_AmbientOcclusion", Settings.GetFloatPropertyValue("AmbientOcclusion"));

                    _material.SetFloat("_TransmissionAmount", Settings.GetFloatPropertyValue("TranslucencyAmount"));
                    _material.SetFloat("_TransmissionSize", Settings.GetFloatPropertyValue("TranslucencySize"));

                    _material.SetFloat("_MaxWindStrength", Settings.GetFloatPropertyValue("WindInfluence"));
                    _material.SetFloat("_GlobalWindMotion", Settings.GetFloatPropertyValue("GlobalWindMotion"));
                    _material.SetFloat("_LeafFlutter", Settings.GetFloatPropertyValue("LeafFlutter"));
                    _material.SetFloat("_WindSwinging", Settings.GetFloatPropertyValue("WindSwinging"));
                    _material.SetFloat("_WindAmplitudeMultiplier", Settings.GetFloatPropertyValue("WindAmplitude"));
                }
        }
    }
}