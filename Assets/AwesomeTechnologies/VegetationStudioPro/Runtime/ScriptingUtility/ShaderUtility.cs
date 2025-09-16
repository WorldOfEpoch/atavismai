using AwesomeTechnologies.VegetationSystem;
using System;
using System.Linq;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class ShaderUtility  // utility class for setting up shaderControllers for vegetationItems
    {
        public static IShaderController GetShaderController(string _shaderName)
        {
            var shaderControllerTypes = typeof(IShaderController).Assembly.GetTypes()
                .Where(x => typeof(IShaderController).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .ToList();

            if (_shaderName == "")
                return null;

            for (int i = 0; i < shaderControllerTypes.Count; i++)
            {
                IShaderController shaderController = (IShaderController)shaderControllerTypes[i];
                if (shaderController != null)
                    if (shaderController.MatchShader(_shaderName))
                        return shaderController;
            }

            return null;
        }

        public static bool HasShader(Material _material, string[] _shaderNames)
        {
            string shaderName = _material.shader.name;
            for (int i = 0; i < _shaderNames.Length; i++)
                if (shaderName.Equals(shaderName[i]))
                    return true;
            return false;
        }

        public static bool HasKeyword(Material _material, string _keyword)
        {
            for (int i = 0; i < _material.shaderKeywords.Length; i++)
                if (_material.shaderKeywords[i].Contains(_keyword))
                    return true;
            return false;
        }

        public static float GetFloatFromMaterials(Material[] _materials, string _propertyName)
        {
            for (int i = 0; i < _materials.Length; i++)
                if (_materials[i].HasProperty(_propertyName))
                    return _materials[i].GetFloat(_propertyName);
            return 1;
        }

        public static float GetFloatFromMaterials(Material[] _materials, string _propertyName, string[] _shaderNames)
        {
            for (int i = 0; i < _materials.Length; i++)
                if (_materials[i].HasProperty(_propertyName) && HasShader(_materials[i], _shaderNames))
                    return _materials[i].GetFloat(_propertyName);
            return 1;
        }

        public static Vector4 GetVector4FromMaterials(Material[] _materials, string _propertyName)
        {
            for (int i = 0; i < _materials.Length; i++)
                if (_materials[i].HasProperty(_propertyName))
                    return _materials[i].GetVector(_propertyName);
            return Vector4.zero;
        }

        public static Color GetColorFromMaterials(Material[] _materials, string _propertyName)
        {
            for (int i = 0; i < _materials.Length; i++)
                if (_materials[i].HasProperty(_propertyName))
                    return _materials[i].GetColor(_propertyName);
            return Color.white;
        }

        public static Color GetColorFromMaterials(Material[] _materials, string _propertyName, string[] _shaderNames)
        {
            for (int i = 0; i < _materials.Length; i++)
                if (_materials[i].HasProperty(_propertyName) && HasShader(_materials[i], _shaderNames))
                    return _materials[i].GetColor(_propertyName);
            return Color.white;
        }

        public static Texture GetTextureFromMaterials(Material[] _materials, string _propertyName)
        {
            for (int i = 0; i < _materials.Length; i++)
                if (_materials[i].HasProperty(_propertyName))
                    return _materials[i].GetTexture(_propertyName);
            return null;
        }

        public static string GetShaderName(GameObject _prefab, int _materialIndex)
        {
            GameObject go = MeshUtility.GetSourceLOD(_prefab, LODLevel.LOD0, true);
            if (go == null) return "";
            MeshRenderer meshRenderer = go.GetComponentInChildren<MeshRenderer>();
            if (!meshRenderer || !meshRenderer.sharedMaterial || !meshRenderer.sharedMaterials[_materialIndex] || !meshRenderer.sharedMaterials[_materialIndex].shader) return "";
            return meshRenderer.sharedMaterials[_materialIndex].shader.name;
        }

        public static Material GetMaterial(GameObject _prefab)
        {
            GameObject go = MeshUtility.GetSourceLOD(_prefab, LODLevel.LOD0, true);
            if (go == null) return null;
            MeshRenderer meshRenderer = go.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.sharedMaterial == null) return null;
            return meshRenderer.sharedMaterial;
        }

        public static Material[] GetMaterials(GameObject _prefab)
        {
            GameObject go = MeshUtility.GetSourceLOD(_prefab, LODLevel.LOD0, true);
            if (go == null) return null;
            MeshRenderer meshRenderer = go.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.sharedMaterials == null) return null;
            return meshRenderer.sharedMaterials;
        }

        public static Texture GetTexture_Diffuse(Material _material)
        {
            if (_material.HasProperty("_MainTex")) return _material.GetTexture("_MainTex");
            if (_material.HasProperty("_MainTexture")) return _material.GetTexture("_MainTexture");
            if (_material.HasProperty("_MainAlbedoTex")) return _material.GetTexture("_MainAlbedoTex");
            if (_material.HasProperty("_MainAlbedoTexture")) return _material.GetTexture("_MainAlbedoTexture");
            if (_material.HasProperty("_BaseMap")) return _material.GetTexture("_BaseMap");
            if (_material.HasProperty("_BaseColorMap")) return _material.GetTexture("_BaseColorMap");
            if (_material.HasProperty("_TrunkBaseColorMap")) return _material.GetTexture("_TrunkBaseColorMap");
            return null;
        }

        public static Color GetColor_Base(Material _material)
        {
            if (_material.HasProperty("_HealthyColor")) return _material.GetColor("_HealthyColor");
            if (_material.HasProperty("_TintColor")) return _material.GetColor("_TintColor");
            if (_material.HasProperty("_ColorTint")) return _material.GetColor("_ColorTint");
            if (_material.HasProperty("_Color")) return _material.GetColor("_Color");   // late since default value of the engine
            if (_material.HasProperty("_BaseColor")) return _material.GetColor("_BaseColor");   // late since default value of the engine
            if (_material.HasProperty("_HueVariation")) return _material.GetColor("_HueVariation"); // last as "fallback" since usually the "alternative" color
            if (_material.HasProperty("_DryColor")) return _material.GetColor("_DryColor"); // last as "fallback" since usually the "alternative" color
            return Color.white;
        }

        public static Shader GetShader_EngineDefault()
        {
#if USING_HDRP
            return Shader.Find("HDRP/Lit");
#elif USING_URP
            return Shader.Find("Universal Render Pipeline/Lit");
#else
            return Shader.Find("Standard");
#endif
        }

        public static Shader GetShader_Standard()
        {
            return Shader.Find("AwesomeTechnologies/Release/Vegetation/VSP-B_Standard");
        }

        public static Shader GetShader_Foliage()
        {
            return Shader.Find("AwesomeTechnologies/Release/Vegetation/VSP-B_Foliage");
        }

        public static Shader GetShader_Billboard()
        {
            return Shader.Find("AwesomeTechnologies/Development/Release_Internal/Vegetation/VSP-B_Billboard");
        }

        public static ComputeShader GetComputeShader_GPUFrustumCullingLODJob()
        {
            return Resources.Load<ComputeShader>("ComputeShaders/GPUFrustumCullingLODJob");
        }

        public static ComputeShader GetComputeShader_MergeInstancedIndirectBuffers()
        {
            return Resources.Load<ComputeShader>("ComputeShaders/MergeInstancedIndirectBuffers");
        }

        public static Shader GetShader_TerrainHeatmap()
        {
            return Shader.Find("AwesomeTechnologies/Development/Release_Internal/Terrain/VSP-B_TerrainHeatmap");
        }

        public static Shader GetShader_UtilityVegetationColorMask()
        {
            return Shader.Find("AwesomeTechnologies/Development/Utility/UtilityVegetationColorMask");
        }

        public static Shader GetShader_UtilityVegetationNormalMask()
        {
            return Shader.Find("AwesomeTechnologies/Development/Utility/UtilityVegetationNormalMask");
        }

        public static Shader GetShader_UtilityAlbedoColor()
        {
            return Shader.Find("AwesomeTechnologies/Development/Utility/UtilityAlbedoColor");
        }

        public static Shader GetShader_UtilityVertexColor()
        {
            return Shader.Find("AwesomeTechnologies/Development/Utility/UtilityVertexColor");
        }

        public static Shader GetShader_UtilityVertexHeight()
        {
            return Shader.Find("AwesomeTechnologies/Development/Utility/UtilityVertexHeight");
        }

        public static ComputeShader GetComputeShader_UtilityVertexHeight_Decode()
        {
            return Resources.Load<ComputeShader>("ComputeShaders/UtilityVertexHeight_Decode");
        }
    }
}