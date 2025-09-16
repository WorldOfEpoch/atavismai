using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class NMAdvancedGrassController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "NatureManufacture Shaders/Grass/Advanced Grass Light", "NatureManufacture Shaders/Grass/Advanced Grass Specular", "NatureManufacture Shaders/Grass/Advanced Grass Standard"
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
                heading = "Nature Manufacture Advanced Grass",
                supportsInstancedIndirect = true
            };

            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("HealthyColorTint", "Healthy color tint", "", ShaderUtility.GetColorFromMaterials(_materials, "_HealthyColor"));
            Settings.AddColorProperty("DryColorTint", "Dry color tint", "", ShaderUtility.GetColorFromMaterials(_materials, "_DryColor"));
            Settings.AddFloatProperty("ColorNoiseSpread", "Color noise spread", "", ShaderUtility.GetFloatFromMaterials(_materials, "_ColorNoiseSpread"), 1, 150);
            Settings.AddFloatProperty("AlphaCutoff", "Alpha cutoff", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Cutoff"), 0, 1);

            Settings.AddLabelProperty("Wind settings");
            Settings.AddFloatProperty("InitialBend", "Initial Bend", "", ShaderUtility.GetFloatFromMaterials(_materials, "_InitialBend"), 0, 10);
            Settings.AddFloatProperty("Stiffness", "Stiffness", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Stiffness"), 0, 10);
            Settings.AddFloatProperty("Drag", "Drag", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Drag"), 0, 10);
            Settings.AddFloatProperty("ShiverDrag", "Shiver Drag", "", ShaderUtility.GetFloatFromMaterials(_materials, "_ShiverDrag"), 0, 10);
            Settings.AddFloatProperty("ShiverDirectionality", "Shiver Directionality", "", ShaderUtility.GetFloatFromMaterials(_materials, "_ShiverDirectionality"), 0, 1);
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

                    _material.SetFloat("_CullFarStart", 10000);
                    _material.SetColor("_HealthyColor", Settings.GetColorPropertyValue("HealthyColorTint"));
                    _material.SetColor("_DryColor", Settings.GetColorPropertyValue("DryColorTint"));

                    _material.SetFloat("_ColorNoiseSpread", Settings.GetFloatPropertyValue("ColorNoiseSpread"));
                    _material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("AlphaCutoff"));

                    _material.SetFloat("_InitialBend", Settings.GetFloatPropertyValue("InitialBend"));
                    _material.SetFloat("_Stiffness", Settings.GetFloatPropertyValue("Stiffness"));
                    _material.SetFloat("_Drag", Settings.GetFloatPropertyValue("Drag"));
                    _material.SetFloat("_ShiverDrag", Settings.GetFloatPropertyValue("ShiverDrag"));
                    _material.SetFloat("_ShiverDirectionality", Settings.GetFloatPropertyValue("ShiverDirectionality"));
                }
        }
    }
}