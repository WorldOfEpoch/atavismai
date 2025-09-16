using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class NMTreeFoliageSnow_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "NatureManufacture Shaders/Trees/Tree Bark Metalic Snow","NatureManufacture Shaders/Trees/Tree Bark Specular Snow"
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
            if (_material.shader.name == "NatureManufacture Shaders/Trees/Cross Model Shader Snow")
                return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "Nature Manufacture Tree Snow Bark",
                supportsInstancedIndirect = true,
            };

            Settings.AddLabelProperty("Snow settings");
            Settings.AddBooleanProperty("GlobalSnow", "Use Global Snow Value", "", true);
            Settings.AddFloatProperty("SnowAmount", "Snow Amount", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Snow_Amount"), 0, 1);
            Settings.AddFloatProperty("SnowBrightnessReduction", "Brightness Reduction", "", ShaderUtility.GetFloatFromMaterials(_materials, "_SnowBrightnessReduction"), 0, 1);

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkColor", "Bark Color", "", ShaderUtility.GetColorFromMaterials(_materials, "_Color"));

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
                    bool globalSnow = Settings.GetBooleanPropertyValue("GlobalSnow");
                    if (globalSnow)
                        _material.SetFloat("_Snow_Amount", _environmentSettings.snowAmount);

                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial"

                    if (globalSnow == false)
                        _material.SetFloat("_Snow_Amount", Settings.GetFloatPropertyValue("SnowAmount"));

                    Color barkColor = Settings.GetColorPropertyValue("BarkColor");
                    _material.SetColor("_Color", barkColor);

                    float initialBend = Settings.GetFloatPropertyValue("InitialBend");
                    float stiffness = Settings.GetFloatPropertyValue("Stiffness");
                    float drag = Settings.GetFloatPropertyValue("Drag");

                    _material.SetFloat("_InitialBend", initialBend);
                    _material.SetFloat("_Stiffness", stiffness);
                    _material.SetFloat("_Drag", drag);
                }
        }
    }
}