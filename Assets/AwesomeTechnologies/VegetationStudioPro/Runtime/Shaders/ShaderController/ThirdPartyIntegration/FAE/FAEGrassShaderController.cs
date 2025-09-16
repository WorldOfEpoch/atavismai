using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class FAEGrassShaderController : IShaderController
    {
        private static readonly string[] ShaderNames =
        {
            "FAE/Grass"
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
                heading = "Fantasy Adventure Environment Grass",
                supportsInstancedIndirect = true
            };

            bool hasPigmentMap = Shader.GetGlobalTexture("_PigmentMap");

            Settings.AddLabelProperty("Color");
            Settings.AddBooleanProperty("EnablePigmentMap", "Use pigment map", "", hasPigmentMap);
            Settings.AddColorProperty("TopColor", "Top", "", ShaderUtility.GetColorFromMaterials(_materials, "_ColorTop"));
            Settings.AddColorProperty("BottomColor", "Bottom", "", ShaderUtility.GetColorFromMaterials(_materials, "_ColorBottom"));
            Settings.AddFloatProperty("WindTint", "Wind tint", "", ShaderUtility.GetFloatFromMaterials(_materials, "_ColorVariation"), 0, 1);
            Settings.AddFloatProperty("AmbientOcclusion", "Ambient Occlusion", "", ShaderUtility.GetFloatFromMaterials(_materials, "_AmbientOcclusion"), 0, 1);

            Settings.AddLabelProperty("Translucency");
            Settings.AddFloatProperty("TranslucencyAmount", "Amount", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TransmissionAmount"), 0, 10);
            Settings.AddFloatProperty("TranslucencySize", "Size", "", ShaderUtility.GetFloatFromMaterials(_materials, "_TransmissionSize"), 1, 20);

            Settings.AddLabelProperty("Wind");
            Settings.AddFloatProperty("WindInfluence", "Influence", "", ShaderUtility.GetFloatFromMaterials(_materials, "_MaxWindStrength"), 0, 1);
            Settings.AddFloatProperty("WindSwinging", "Swinging", "", ShaderUtility.GetFloatFromMaterials(_materials, "_WindSwinging"), 0, 1);
            Settings.AddFloatProperty("WindAmplitude", "Amplitude Multiplier", "", ShaderUtility.GetFloatFromMaterials(_materials, "_WindAmplitudeMultiplier"), 0, 10);

#if TOUCH_REACT
            Settings.AddLabelProperty("Touch React");
#else
            Settings.AddLabelProperty("Player bending");
#endif
            Settings.AddFloatProperty("BendingInfluence", "Influence", "", ShaderUtility.GetFloatFromMaterials(_materials, "_BendingInfluence"), 0, 1);
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

                    // force enable touch react usage
#if TOUCH_REACT
                    _material.SetFloat("_VS_TOUCHBEND", 0);
#endif
                    _material.SetFloat("_PigmentMapInfluence", Settings.GetBooleanPropertyValue("EnablePigmentMap") ? 1 : 0);

                    // allow VS heightmaps to control the height
                    _material.SetFloat("_MaxHeight", 0.5f);

                    _material.SetColor("_ColorTop", Settings.GetColorPropertyValue("TopColor"));
                    _material.SetColor("_ColorBottom", Settings.GetColorPropertyValue("BottomColor"));
                    _material.SetFloat("_ColorVariation", Settings.GetFloatPropertyValue("WindTint"));
                    _material.SetFloat("_AmbientOcclusion", Settings.GetFloatPropertyValue("AmbientOcclusion"));

                    _material.SetFloat("_TransmissionAmount", Settings.GetFloatPropertyValue("TranslucencyAmount"));
                    _material.SetFloat("_TransmissionSize", Settings.GetFloatPropertyValue("TranslucencySize"));

                    _material.SetFloat("_MaxWindStrength", Settings.GetFloatPropertyValue("WindInfluence"));
                    _material.SetFloat("_WindSwinging", Settings.GetFloatPropertyValue("WindSwinging"));
                    _material.SetFloat("_WindAmplitudeMultiplier", Settings.GetFloatPropertyValue("WindAmplitude"));

                    _material.SetFloat("_BendingInfluence", Settings.GetFloatPropertyValue("BendingInfluence"));
                }
        }
    }
}