using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class VSP_B_Standard_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =  // the shader/-s this shaderController should control
        {
            "AwesomeTechnologies/Release/Vegetation/VSP-B_Standard"
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
                heading = "VSP-B Standard shader",
                supportsInstancedIndirect = true,   // let the system set the rendering mode to "Instanced indirect" by default => for all vegetation items that use this shader controller
            };

            Settings.AddColorProperty("Color", "Color", "", ShaderUtility.GetColorFromMaterials(_materials, "_Color"));
            Settings.AddFloatProperty("BumpMapScale", "Bump map scale", "", ShaderUtility.GetFloatFromMaterials(_materials, "_BumpMapScale"), 0, 2);
            Settings.AddFloatProperty("Metallic", "Metallic power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_MetallicPower"), 0, 5);
            Settings.AddFloatProperty("Smoothness", "Smoothness power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_SmoothnessPower"), 0, 2);
            Settings.AddFloatProperty("Occlusion", "Occlusion power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_OcclusionPower"), 0, 1);
        }

        public void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro)
        {
            if (Settings == null)
                return;

            for (int i = 0; i < ShaderNames.Length; i++)    // for all shaders added to the string array
                if (_material.shader.name == ShaderNames[i])    // only control materials that use a shader of the string array
                {
                    // let the "Weather" tab of the "VegetationSystemPro" override these values dynamically
                    _material.SetFloat("_SnowAmount", _environmentSettings.snowAmount); // SNOW
                    _material.SetColor("_SnowColor", _environmentSettings.snowColor);
                    _material.SetFloat("_SnowMinimumVariation", _environmentSettings.snowMinimumVariation);
                    _material.SetFloat("_SnowBlendPower", _environmentSettings.snowBlendPower);
                    _material.SetFloat("_SnowMinimumHeight", _environmentSettings.snowMinHeight);
                    _material.SetFloat("_SnowMinimumHeightVariation", _environmentSettings.snowMinHeightVariation);
                    _material.SetFloat("_SnowMinimumHeightBlendPower", _environmentSettings.snowMinHeightBlendPower);

                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial" -- only apply needed values for billboards and snow

                    // "Color" is being written to the material's "_Color" of the above added "VSP-B_Standard" shader => same with the other property pairs
                    _material.SetColor("_Color", Settings.GetColorPropertyValue("Color"));
                    _material.SetFloat("_BumpMapScale", Settings.GetFloatPropertyValue("BumpMapScale"));
                    _material.SetFloat("_MetallicPower", Settings.GetFloatPropertyValue("Metallic"));
                    _material.SetFloat("_SmoothnessPower", Settings.GetFloatPropertyValue("Smoothness"));
                    _material.SetFloat("_OcclusionPower", Settings.GetFloatPropertyValue("Occlusion"));
                }
        }
    }
}