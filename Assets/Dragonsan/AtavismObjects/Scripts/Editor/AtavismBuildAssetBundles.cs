using System.IO;
using UnityEditor;
using UnityEngine;

namespace Atavism
{
    public class AtavismBuildAssetBundles
    {

        [MenuItem("Assets/Atavism Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}