using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class FAETreeTrunk_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "FAE/Tree Trunk"
        };

        public bool MatchShader(string _shaderName)
        {
            if (string.IsNullOrEmpty(_shaderName))
                return false;

            for (int i = 0; i < ShaderNames.Length; i++)
                if (ShaderNames[i].Contains(_shaderName))
                    return true;

            return false;
        }

        public bool MatchBillboardShader(Material _material)
        {
            if (_material.shader.name == "FAE/Tree Billboard")
                return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "Fantasy Adventure Environment Tree Trunk",
                supportsInstancedIndirect = false
            };

            Settings.AddFloatProperty("GradientBrightness", "Gradient Brightness", "", ShaderUtility.GetFloatFromMaterials(_materials, "_GradientBrightness"), 0, 2);
            Settings.AddFloatProperty("AmbientOcclusion", "Ambient Occlusion", "", ShaderUtility.GetFloatFromMaterials(_materials, "_AmbientOcclusion"), 0, 1);
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

                    float ambientOcclusion = Settings.GetFloatPropertyValue("AmbientOcclusion");
                    float gradientBrightness = Settings.GetFloatPropertyValue("GradientBrightness");

                    _material.SetFloat("_AmbientOcclusion", ambientOcclusion);
                    _material.SetFloat("_GradientBrightness", gradientBrightness);
                }
        }
    }
}