#if UNITY_POST_PROCESSING_STACK_V2
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace AwesomeTechnologies.VegetationStudio
{
    [System.Serializable]
    public class PostProcessProfileInfo
    {
        public bool Enabled = true;
        public PostProcessProfile PostProcessProfile;
        public BiomeType BiomeType = BiomeType.Biome1;
        public float BlendDistance = 0;
        public float Weight = 1;
        public float VolumeHeight = 20;
        public float Priority;
    }

    public partial class VegetationStudioManager
    {
        public void RefreshPostProcessVolumes()
        {
            BiomeMaskArea[] biomeMaskAreas = Object.FindObjectsByType<BiomeMaskArea>(FindObjectsSortMode.None);
            for (int i = 0; i < biomeMaskAreas.Length; i++)
                biomeMaskAreas[i].RefreshPostProcessVolume(Instance_GetPostProcessProfileInfo(biomeMaskAreas[i].BiomeType), PostProcessingLayer);
        }

        public static PostProcessProfileInfo GetPostProcessProfileInfo(BiomeType _biomeType)
        {
            if (!Instance) FindInstance();
            if (Instance)
                return Instance.Instance_GetPostProcessProfileInfo(_biomeType);
            return null;
        }

        public PostProcessProfileInfo Instance_GetPostProcessProfileInfo(BiomeType _biomeType)
        {
            for (int i = 0; i < PostProcessProfileInfoList.Count; i++)
                if (PostProcessProfileInfoList[i].BiomeType == _biomeType)
                    return PostProcessProfileInfoList[i];
            return null;
        }

        public void AddPostProcessProfile(PostProcessProfile _postProcessProfile)
        {
            PostProcessProfileInfoList.Add(new() { PostProcessProfile = _postProcessProfile });
            RefreshPostProcessVolumes();
        }

        public void RemovePostProcessProfile(int _index)
        {
            PostProcessProfileInfoList.RemoveAt(_index);
            RefreshPostProcessVolumes();
        }

        public static LayerMask GetPostProcessingLayer()
        {
            if (!Instance) FindInstance();
            if (Instance)
                return Instance.PostProcessingLayer;
            return 0;
        }
    }
}
#endif