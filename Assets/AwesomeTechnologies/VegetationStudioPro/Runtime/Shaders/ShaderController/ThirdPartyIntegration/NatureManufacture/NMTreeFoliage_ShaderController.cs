using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class NMTreeFoliage_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "NatureManufacture Shaders/Trees/Tree Leaves Metalic", "NatureManufacture Shaders/Trees/Tree_Leaves_Specular"
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
            if (_material.shader.name == "NatureManufacture Shaders/Trees/Cross Model Shader")
                return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "Nature Manufacture Tree Leaves",
                supportsInstancedIndirect = true
            };

            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("HealtyColor", "Healthy Color", "", ShaderUtility.GetColorFromMaterials(_materials, "_HealthyColor"));
            Settings.AddColorProperty("DryColor", "DryColor", "", ShaderUtility.GetColorFromMaterials(_materials, "_DryColor"));

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

                    Color healtyColor = Settings.GetColorPropertyValue("HealtyColor");
                    Color dryColor = Settings.GetColorPropertyValue("DryColor");

                    _material.SetColor("_HealthyColor", healtyColor);
                    _material.SetColor("_DryColor", dryColor);

                    float shiverDrag = Settings.GetFloatPropertyValue("ShiverDrag");
                    float shiverDirectionality = Settings.GetFloatPropertyValue("ShiverDirectionality");

                    _material.SetFloat("_ShiverDrag", shiverDrag);
                    _material.SetFloat("_ShiverDirectionality", shiverDirectionality);

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