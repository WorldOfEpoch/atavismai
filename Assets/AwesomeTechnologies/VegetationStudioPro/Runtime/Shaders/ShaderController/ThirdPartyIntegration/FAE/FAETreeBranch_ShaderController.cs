using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class FAETreeBranchShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "FAE/Tree Branch"
        };

        public bool MatchShader(string _shaderName)
        {
            if (string.IsNullOrEmpty(_shaderName))
                return false;

            for (int i = 0; i < ShaderNames.Length; i++)
                if (ShaderNames[i].Contains(_shaderName))
                    return true;

            return false;
        }

        public bool MatchBillboardShader(Material _material)
        {
            if (_material.shader.name == "FAE/Tree Billboard")
                return true;
            return false;
        }

        public ShaderControllerSettings Settings { get; set; }

        public void CreateDefaultSettings(Material[] _materials)
        {
            Settings = new ShaderControllerSettings
            {
                heading = "Fantasy Adventure Environment Tree Branch",
                supportsInstancedIndirect = false
            };

            Settings.AddColorProperty("HueVariation", "Hue Variation", "", ShaderUtility.GetColorFromMaterials(_materials, "_HueVariation"));
            Settings.AddColorProperty("TransmissionColor", "Transmission Color", "", ShaderUtility.GetColorFromMaterials(_materials, "_TransmissionColor"));
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

                    _material.SetColor("_HueVariation", Settings.GetColorPropertyValue("HueVariation"));
                    _material.SetColor("_TransmissionColor", Settings.GetColorPropertyValue("TransmissionColor"));
                    _material.SetFloat("_MaxWindStrength", Settings.GetFloatPropertyValue("WindInfluence"));
                    _material.SetFloat("_WindAmplitudeMultiplier", Settings.GetFloatPropertyValue("WindAmplitude"));
                }
        }
    }
}