#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AwesomeTechnologies.PackageInstaller
{
    public static class PackageInstaller
    {
        private static ListRequest listRequest;
        private const string VSP_PACKAGES_DEFINE = "VSP_PACKAGES";
        private const string COLLECTIONS_PACKAGE = "com.unity.collections";
        private const string SPLINES_PACKAGE = "com.unity.splines";
        private const string SHADER_PATH = "Assets/AwesomeTechnologies/VegetationStudioPro/Runtime/Shaders/Resources";

        [UnityEditor.Callbacks.DidReloadScripts]
        static void VerifyPackages()
        {
#if !VSP_PACKAGES
        if (Application.isPlaying == false)
        {
            listRequest = Client.List(false, true);
            EditorApplication.update += ListProgress;
        }
#endif
        }

        static void ListProgress()
        {
            if (listRequest.IsCompleted)
            {
                bool hasCollections = false;
                bool hasSplines = false;
                if (listRequest.Status == StatusCode.Success)
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name.Contains(COLLECTIONS_PACKAGE))
                            hasCollections = true;

                        if (package.name.Contains(SPLINES_PACKAGE))
                            hasSplines = true;
                    }
                else if (listRequest.Status >= StatusCode.Failure)
                    Debug.Log(listRequest.Error.message);

                EditorApplication.update -= ListProgress;

                if (hasCollections == false || hasSplines == false)
                {
                    if (hasCollections == true && hasSplines == false)
                    {
                        Client.Add(SPLINES_PACKAGE);
                        return;
                    }

                    if (EditorUtility.DisplayDialog("Vegetation Studio Pro - Beyond -- Install needed packages",
                            "The \"Collections\" / \"Splines\" package and their dependencies are missing\n\nConfirm to install the missing packages\nRestart the editor after installing", "Confirm", "Cancel"))
                    {
                        if (hasCollections == false) Client.Add(COLLECTIONS_PACKAGE);   // double check since "Client.Add()" jumps out the code flow too early entirely
                        if (hasSplines == false) Client.Add(SPLINES_PACKAGE);   // double check since "Client.Add()" jumps out the code flow too early entirely
                    }
                }
                else
                {
#if !VSP_PACKAGES
                    ReimportShaders();
                    AddCompilerDefine();
#endif
                }
            }
        }

        static void AddCompilerDefine()
        {
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            if (symbols.Contains(VSP_PACKAGES_DEFINE))
                return;

            symbols += ";" + VSP_PACKAGES_DEFINE;
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols);
        }

        static void ReimportShaders()
        {
            foreach (string _guid in AssetDatabase.FindAssets("t:shader", new[] { SHADER_PATH }))
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(_guid));
        }
    }
}
#endif