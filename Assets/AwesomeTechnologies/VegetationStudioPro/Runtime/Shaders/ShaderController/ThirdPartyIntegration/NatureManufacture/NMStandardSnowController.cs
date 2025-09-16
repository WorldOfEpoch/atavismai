using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class NMStandardSnowController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "NatureManufacture Shaders/Standard Shaders/Standard Metalic Snow", "NatureManufacture Shaders/Standard Shaders/Standard Specular Snow"
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
                heading = "Nature Manufacture Standard Snow",
                supportsInstancedIndirect = true,
            };

            Settings.AddLabelProperty("Snow settings");
            Settings.AddBooleanProperty("GlobalSnow", "Use Global Snow Value", "", true);
            Settings.AddFloatProperty("SnowAmount", "Snow Amount", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Snow_Amount"), 0, 1);
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
                        _material.SetFloat("_Snow_Amount", _environmentSettings.snowAmount * 2f);

                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial"

                    if (globalSnow == false)
                        _material.SetFloat("_Snow_Amount", Settings.GetFloatPropertyValue("SnowAmount") * 2f);
                }
        }
    }
}