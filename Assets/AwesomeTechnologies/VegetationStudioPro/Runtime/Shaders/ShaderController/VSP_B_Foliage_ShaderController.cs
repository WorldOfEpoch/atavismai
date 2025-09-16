using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class VSP_B_Foliage_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =  // the shader/-s this shaderController should control
        {
            "AwesomeTechnologies/Release/Vegetation/VSP-B_Foliage"
        };

        private static readonly string[] BillboardShaderNames = // same as above => when a custom billboard shader is used then replace it also in "VegetationItemModelInfo.cs"
        {
            "AwesomeTechnologies/Development/Release_Internal/Vegetation/VSP-B_Billboard"
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
            for (int i = 0; i < BillboardShaderNames.Length; i++)   // for all shaders added to the string array
                if (BillboardShaderNames[i] == _material.shader.name)   // only control materials that use a shader of the string array
                    return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "VSP-B Foliage shader",
                supportsInstancedIndirect = true,   // let the system set the rendering mode to "Instanced indirect" by default => for all vegetation items that use this shader controller
            };

            Settings.AddFloatProperty("AlphaClipping", "Alpha cutoff", "", ShaderUtility.GetFloatFromMaterials(_materials, "_AlphaClipping"), 0, 1);
            Settings.AddFloatProperty("BumpMapScale", "Bump map scale", "", ShaderUtility.GetFloatFromMaterials(_materials, "_BumpMapScale"), 0, 2);

            /// "TintColor1" is being automatically passed to the above added "VSP-B_Billboard" shader to write to its own "_HealthyColor"
            /// => important here is the "TintColor1" string
            /// "_HealthyColor" is the property name of the above linked "VSP-B_Foliage" shader of the material of the actual vegetation instance
            /// 
            /// => The property names differ from shader to shader and need to be set accordingly when creating shader controllers
            /// 
            Settings.AddColorProperty("TintColor1", "Healthy color", "", ShaderUtility.GetColorFromMaterials(_materials, "_HealthyColor"));

            // Also automatically passed to the above added "VSP-B_Billboard" shader
            Settings.AddColorProperty("TintColor2", "Dry color", "", ShaderUtility.GetColorFromMaterials(_materials, "_DryColor"));
            Settings.AddTextureProperty("TintAreaTex", "Dry color noise texture", "", ShaderUtility.GetTextureFromMaterials(_materials, "_DryColorNoiseTex"));
            Settings.AddFloatProperty("TintAreaScale", "Dry color noise scale", "", ShaderUtility.GetFloatFromMaterials(_materials, "_DryColorNoiseScale"), 1, 1000);
            // ----- billboard pass end -----

            Settings.AddFloatProperty("RandomDarkening", "Random darkening", "", ShaderUtility.GetFloatFromMaterials(_materials, "_RandomDarkening"), 0, 1);
            Settings.AddFloatProperty("RootAmbient", "Root ambient", "", ShaderUtility.GetFloatFromMaterials(_materials, "_RootAmbient"), 0, 1);

            Settings.AddFloatProperty("Specular", "Specular power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_SpecularPower"), 0, 5);
            Settings.AddFloatProperty("Smoothness", "Smoothness power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_SmoothnessPower"), 0, 2);
            Settings.AddFloatProperty("Occlusion", "Occlusion power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_AmbientOcclusionPower"), 0, 1);
        }

        public void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro)
        {
            if (Settings == null)
                return;

            for (int i = 0; i < ShaderNames.Length; i++)    // for all shaders added to the string array
                if (_material.shader.name == ShaderNames[i])    // only control materials that use a shader of the string array
                {
                    // Also automatically passed to the above added "VSP-B_Billboard" shader
                    Settings.supportsWind = _material.GetFloat("VERTEX_WIND") == 1; // passthrough for billboards
                    Settings.ptInitialBend = _material.GetFloat("_InitialBend");    // WIND
                    Settings.ptStiffness = _material.GetFloat("_Stiffness");
                    Settings.ptDrag = _material.GetFloat("_Drag");
                    Settings.ptShiverDrag = _material.GetFloat("_ShiverDrag");
                    Settings.ptShiverDirectionality = _material.GetFloat("_ShiverDirectionality");

                    Settings.supportsHueVariation = _material.GetFloat("HUE_VARIATION") == 1;   // passthrough for billboards

                    // also here to let the "Weather" tab of the "VegetationSystemPro" override these values dynamically
                    Settings.supportsSnow = _material.GetFloat("FOLIAGE_SNOW") == 1;    // passthrough for billboards
                    _material.SetFloat("_SnowAmount", _environmentSettings.snowAmount); // SNOW
                    _material.SetColor("_SnowColor", _environmentSettings.snowColor);
                    _material.SetFloat("_SnowMinimumVariation", _environmentSettings.snowMinimumVariation);
                    _material.SetFloat("_SnowBlendPower", _environmentSettings.snowBlendPower);
                    _material.SetFloat("_SnowMinimumHeight", _environmentSettings.snowMinHeight);
                    _material.SetFloat("_SnowMinimumHeightVariation", _environmentSettings.snowMinHeightVariation);
                    _material.SetFloat("_SnowMinimumHeightBlendPower", _environmentSettings.snowMinHeightBlendPower);
                    // ----- billboard pass end -----

                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial" -- only apply needed values for billboards and snow

                    _material.SetFloat("_AlphaClipping", Settings.GetFloatPropertyValue("AlphaClipping"));
                    _material.SetFloat("_BumpMapScale", Settings.GetFloatPropertyValue("BumpMapScale"));

                    // "TintColor1" is being written to the material's "_HealthyColor" of the above added "VSP-B_Foliage" shader => same with the other property pairs
                    _material.SetColor("_HealthyColor", Settings.GetColorPropertyValue("TintColor1"));

                    _material.SetColor("_DryColor", Settings.GetColorPropertyValue("TintColor2"));  // HUE VARIATION
                    _material.SetFloat("_DryColorNoiseScale", Settings.GetFloatPropertyValue("TintAreaScale"));

                    _material.SetFloat("_RandomDarkening", Settings.GetFloatPropertyValue("RandomDarkening"));  // DARKENING
                    _material.SetFloat("_RootAmbient", Settings.GetFloatPropertyValue("RootAmbient"));

                    _material.SetFloat("_SpecularPower", Settings.GetFloatPropertyValue("Specular"));
                    _material.SetFloat("_SmoothnessPower", Settings.GetFloatPropertyValue("Smoothness"));
                    _material.SetFloat("_AmbientOcclusionPower", Settings.GetFloatPropertyValue("Occlusion"));
                }
        }
    }
}