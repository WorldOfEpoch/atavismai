using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class AngryMeshRockShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "ANGRYMESH/PBR BlendTopDetail", "ANGRYMESH/VS BlendTopDetail"
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
                heading = "ANGRYMESH Rocks",
                supportsInstancedIndirect = false,
            };

            Settings.AddLabelProperty("Base Rock settings");
            Settings.AddColorProperty("BaseColor", "Base color", "", ShaderUtility.GetColorFromMaterials(_materials, "_BaseColor"));
            Settings.AddFloatProperty("BaseSmoothness", "Base smoothness", "", ShaderUtility.GetFloatFromMaterials(_materials, "_BaseSmoothness"), 0, 1);
            Settings.AddFloatProperty("BaseAOIntensity", "Base AO intensity", "", ShaderUtility.GetFloatFromMaterials(_materials, "_BaseAOIntensity"), 0, 1);

            Settings.AddLabelProperty("Top settings");
            Settings.AddColorProperty("TopColor", "Top color", "", ShaderUtility.GetColorFromMaterials(_materials, "_TopColor"));
            Settings.AddFloatProperty("TopOffset", "Top offset", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TopOffset"), 0, 1);
            Settings.AddFloatProperty("TopContrast", "Top contrast", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TopContrast"), 0, 1);
            Settings.AddFloatProperty("TopIntensity", "Top intensity", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TopIntensity"), 0, 1);
            Settings.AddFloatProperty("TopNormalIntensity", "Top normal intensity", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TopNormalIntensity"), 0, 1);
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

                    _material.SetColor("_BaseColor", Settings.GetColorPropertyValue("BaseColor"));
                    _material.SetFloat("_BaseSmoothness", Settings.GetFloatPropertyValue("BaseSmoothness"));
                    _material.SetFloat("_BaseAOIntensity", Settings.GetFloatPropertyValue("BaseAOIntensity"));

                    _material.SetColor("_TopColor", Settings.GetColorPropertyValue("TopColor"));
                    _material.SetFloat("_TopOffset", Settings.GetFloatPropertyValue("TopOffset"));
                    _material.SetFloat("_TopContrast", Settings.GetFloatPropertyValue("TopContrast"));
                    _material.SetFloat("_TopIntensity", Settings.GetFloatPropertyValue("TopIntensity"));
                    _material.SetFloat("_TopNormalIntensity", Settings.GetFloatPropertyValue("TopNormalIntensity"));
                }
        }
    }
}