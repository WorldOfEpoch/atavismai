#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

namespace AwesomeTechnologies.TouchReact
{
    [InitializeOnLoad]
    public class CompilerDefineTouchReact : Editor
    {
        private const string TOUCH_REACT_DEFINE = "TOUCH_REACT";

        static CompilerDefineTouchReact()
        {
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            if (symbols.Contains(TOUCH_REACT_DEFINE))
                return;

            symbols += ";" + TOUCH_REACT_DEFINE;
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols);
        }
    }
}
#endif