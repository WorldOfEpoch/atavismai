using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class UnityStandard_ShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =  // the shader/-s this shaderController should control
        {
            "Standard"
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
                heading = "Unity Standard BIRP shader",
                supportsInstancedIndirect = false,
            };

            Settings.AddFloatProperty("AlphaClipping", "Alpha cutoff", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Cutoff"), 0, 1);
            Settings.AddFloatProperty("BumpMapScale", "Bump map scale", "", ShaderUtility.GetFloatFromMaterials(_materials, "_BumpScale"), 0, 2);

            Settings.AddColorProperty("TintColor1", "Healthy color", "", ShaderUtility.GetColorFromMaterials(_materials, "_Color"));    // "TintColor1" to sync color to billboards => Read more in "VSP_B_Foliage_ShaderController.cs"
            Settings.AddFloatProperty("Metallic", "Metallic power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Metallic"), 0, 1);
            Settings.AddFloatProperty("Smoothness", "Smoothness power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Glossiness"), 0, 1);

            Settings.AddFloatProperty("Parallax", "Height parallax power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_Parallax"), 0.005f, 0.08f);
            Settings.AddFloatProperty("Occlusion", "Occlusion power", "", ShaderUtility.GetFloatFromMaterials(_materials, "_OcclusionStrength"), 0, 1);

            Settings.AddColorProperty("EmissionColor", "Emission color", "", ShaderUtility.GetColorFromMaterials(_materials, "_EmissionColor"));

            Settings.AddFloatProperty("DetailNormalMapScale", "Detail normal map scale", "", ShaderUtility.GetFloatFromMaterials(_materials, "_DetailNormalMapScale"), 0, 2);
        }

        public void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro)
        {
            if (Settings == null)
                return;

            for (int i = 0; i < ShaderNames.Length; i++)    // for all shaders added to the string array
                if (_material.shader.name == ShaderNames[i])    // only control materials that use a shader of the string array
                {
                    if (_vegItemInfoPro.useShaderControllerOverrides == false)
                        continue;   // skip applying settings to the "sharedMaterial" -- only apply needed values for billboards and snow

                    _material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("AlphaClipping"));
                    _material.SetFloat("_BumpScale", Settings.GetFloatPropertyValue("BumpMapScale"));

                    _material.SetColor("_Color", Settings.GetColorPropertyValue("TintColor1")); // "TintColor1" to sync color to billboards => Read more in "VSP_B_Foliage_ShaderController.cs"
                    _material.SetFloat("_Metallic", Settings.GetFloatPropertyValue("Metallic"));
                    _material.SetFloat("_Glossiness", Settings.GetFloatPropertyValue("Smoothness"));

                    _material.SetFloat("_Parallax", Settings.GetFloatPropertyValue("Parallax"));
                    _material.SetFloat("_OcclusionStrength", Settings.GetFloatPropertyValue("Occlusion"));

                    _material.SetColor("_EmissionColor", Settings.GetColorPropertyValue("EmissionColor"));

                    _material.SetFloat("_DetailNormalMapScale", Settings.GetFloatPropertyValue("DetailNormalMapScale"));
                }
        }
    }
}