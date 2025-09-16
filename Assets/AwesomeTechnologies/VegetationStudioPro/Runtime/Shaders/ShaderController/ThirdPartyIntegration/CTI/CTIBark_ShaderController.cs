using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class CTIBark_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "CTI/LOD Bark", "CTI/LOD Bark Array"
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
                heading = "CTI Tree bark shader",
            };

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkColorVariation", "Color variation", "", ShaderUtility.GetColorFromMaterials(_materials, "_HueVariation", ShaderNames));
            Settings.AddFloatProperty("BarkTranslucencyStrength", "Translucency strength", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TranslucencyStrength", ShaderNames), 0, 1);
            Settings.AddFloatProperty("BarkAlphaCutoff", "Alpha cutoff", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Cutoff", ShaderNames), 0, 1);

            Settings.AddLabelProperty("Wind settings");
            Settings.AddFloatProperty("TumbleStrength", "Tumble Strength", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TumbleStrength"), -1, 1);
            Settings.AddFloatProperty("TumbleFrequency", "Tumble Frequency", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TumbleFrequency"), 0, 4);
            Settings.AddFloatProperty("TimeOffset", "Time Offset", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TimeOffset"), 0, 2);
            Settings.AddFloatProperty("LeafTurbulence", "Leaf Turbulence", "", ShaderUtility.GetFloatFromMaterials(_materials, "_LeafTurbulence"), 0, 4);
            Settings.AddFloatProperty("EdgeFlutterInfluence", "Edge Flutter Influence", "", ShaderUtility.GetFloatFromMaterials(_materials, "_EdgeFlutterInfluence"), 0, 1);
        }

        public void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro)
        {
            if (Settings == null)
                return;

            for (int i = 0; i < ShaderNames.Length; i++)
                if (_material.shader.name == ShaderNames[i])
                {
                    Shader.SetGlobalFloat("_Lux_SnowAmount", _environmentSettings.snowAmount);
                    Shader.SetGlobalColor("_Lux_SnowColor", _environmentSettings.snowColor);
                    Shader.SetGlobalColor("_Lux_SnowSpecColor", _environmentSettings.snowSpecularColor);
                    Shader.SetGlobalVector("_Lux_RainfallRainSnowIntensity", new Vector3(_environmentSettings.rainAmount, _environmentSettings.rainAmount, _environmentSettings.snowAmount));
                    Shader.SetGlobalVector("_Lux_WaterFloodlevel", new Vector4(_environmentSettings.rainAmount, _environmentSettings.rainAmount, _environmentSettings.rainAmount, _environmentSettings.rainAmount));

                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial"

                    _material.SetColor("_HueVariation", Settings.GetColorPropertyValue("BarkColorVariation"));
                    _material.SetFloat("_TranslucencyStrength", Settings.GetFloatPropertyValue("BarkTranslucencyStrength"));
                    _material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("BarkAlphaCutoff"));

                    _material.SetFloat("_TumbleStrength", Settings.GetFloatPropertyValue("TumbleStrength"));
                    _material.SetFloat("_TumbleFrequency", Settings.GetFloatPropertyValue("TumbleFrequency"));
                    _material.SetFloat("_TimeOffset", Settings.GetFloatPropertyValue("TimeOffset"));
                    _material.SetFloat("_LeafTurbulence", Settings.GetFloatPropertyValue("LeafTurbulence"));
                    _material.SetFloat("_EdgeFlutterInfluence", Settings.GetFloatPropertyValue("EdgeFlutterInfluence"));
                }
        }
    }
}