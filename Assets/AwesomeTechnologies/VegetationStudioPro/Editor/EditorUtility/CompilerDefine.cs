using UnityEditor;
using UnityEditor.Build;

namespace AwesomeTechnologies.Utility
{
    [InitializeOnLoad]
    public class CompilerDefine : Editor
    {
        private const string VSP_DEFINE = "VEGETATION_STUDIO_PRO";

        static CompilerDefine()
        {
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            if (symbols.Contains(VSP_DEFINE))
                return;

            symbols += ";" + VSP_DEFINE;
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols);
        }
    }
}