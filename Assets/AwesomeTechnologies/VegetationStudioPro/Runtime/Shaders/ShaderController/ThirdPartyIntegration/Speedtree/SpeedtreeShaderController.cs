using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class SpeedtreeShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =  // the shader/-s this shaderController should control
        {
            "Nature/SpeedTree", "Nature/SpeedTree8", "URP/Nature/SpeedTree8", "HDRP/Nature/SpeedTree8"
        };

        public bool MatchShader(string _shaderName) // used to match this shaderController to a vegetationItem and its materials/shaders
        {
            if (string.IsNullOrEmpty(_shaderName))
                return false;

            for (int i = 0; i < ShaderNames.Length; i++)
                if (ShaderNames[i] == _shaderName)
                    return true;

            return false;
        }

        public bool MatchBillboardShader(Material _material)    // used to match this shaderController to a vegetationItem and its "billboardMaterial" / "billboardShader"
        {
            return false;   // no billboard shader used here so just return "false"
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "SpeedTree settings",
                isSpeedTree = true, // make sure a "SpeedTree WindBridge" gets created (~workaround) -- make sure the engine applies CPU based "SpeedTree" wind data
            };

            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("FoliageTintColor", "Foliage tint color", "", ShaderUtility.GetColorFromMaterials(_materials, "_Color"));
            Settings.AddColorProperty("FoliageHue", "Foliage HUE variation", "", ShaderUtility.GetColorFromMaterials(_materials, "_HueVariation"));

            Settings.AddLabelProperty("Bark settings");
            Settings.AddColorProperty("BarkTintColor", "Bark tint color", "", ShaderUtility.GetColorFromMaterials(_materials, "_Color"));
            Settings.AddColorProperty("BarkHue", "Bark HUE variation", "", ShaderUtility.GetColorFromMaterials(_materials, "_HueVariation"));
        }

        public void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro)
        {
            if (Settings == null)
                return;

            for (int i = 0; i < ShaderNames.Length; i++)    // for all shaders added to the string array
                if (_material.shader.name == ShaderNames[i])    // only control materials that use a shader of the string array
                {
                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial"

                    Color foliageHueVariation = Settings.GetColorPropertyValue("FoliageHue");
                    Color barkHueVariation = Settings.GetColorPropertyValue("BarkHue");
                    Color foliageTintColor = Settings.GetColorPropertyValue("FoliageTintColor");
                    Color barkTintColor = Settings.GetColorPropertyValue("BarkTintColor");

                    if (_material.HasProperty("_Cutoff"))
                        _material.SetFloat("_Cutoff", _material.GetFloat("_Cutoff"));

                    if (ShaderUtility.HasKeyword(_material, "GEOM_TYPE_BRANCH"))
                    {
                        _material.SetColor("_HueVariation", barkHueVariation);
                        _material.SetColor("_Color", barkTintColor);
                    }
                    else
                    {
                        _material.SetColor("_HueVariation", foliageHueVariation);
                        _material.SetColor("_Color", foliageTintColor);
                    }
                }
        }
    }
}