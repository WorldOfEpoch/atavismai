// ExcaliburAI – AiSettingsUtil
// Loads or creates the Settings asset that controls Atavism integration paths.

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ExcaliburAI.Data;

namespace ExcaliburAI.Editor
{
    public static class AiSettingsUtil
    {
        private const string DefaultAssetPath = "Assets/ExcaliburAI/Resources/ExcaliburAISettings.asset";

        /// <summary>Find existing settings or create a default one.</summary>
        public static ExcaliburAISettings LoadOrCreate()
        {
            var guid = AssetDatabase.FindAssets("t:ExcaliburAISettings").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return AssetDatabase.LoadAssetAtPath<ExcaliburAISettings>(path);
            }

            Directory.CreateDirectory("Assets/ExcaliburAI/Resources");
            var inst = ScriptableObject.CreateInstance<ExcaliburAISettings>();
            AssetDatabase.CreateAsset(inst, DefaultAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("ExcaliburAI: Created default ExcaliburAISettings asset.");
            return inst;
        }

        /// <summary>Opens/frames the settings asset in the Inspector.</summary>
        [MenuItem("ExcaliburAI/Open Settings")]
        public static void OpenSettings()
        {
            var settings = LoadOrCreate();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        /// <summary>Ensure the Atavism target folders exist (icons/CSV).</summary>
        [MenuItem("ExcaliburAI/Utilities/Ensure Atavism Folders")]
        public static void EnsureAtavismFolders()
        {
            var s = LoadOrCreate();
            var icons = SanitizeFolder(s.atavismIconsFolder);
            var csv   = SanitizeFolder(s.atavismCsvFolder);

            if (!Directory.Exists(icons)) Directory.CreateDirectory(icons);
            if (!Directory.Exists(csv))   Directory.CreateDirectory(csv);

            AssetDatabase.Refresh();
            Debug.Log($"ExcaliburAI: Ensured folders:\n  {icons}\n  {csv}");
        }

        /// <summary>Returns a normalized Unity project-relative folder path.</summary>
        public static string SanitizeFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "Assets";
            var p = path.Replace('\\', '/').Trim();
            if (!p.StartsWith("Assets")) p = "Assets/" + p.TrimStart('/');
            return p.TrimEnd('/');
        }
    }
}
