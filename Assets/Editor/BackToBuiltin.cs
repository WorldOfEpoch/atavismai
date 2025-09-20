// Assets/Editor/BackToBuiltin.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class BackToBuiltin
{
    [MenuItem("Tools/Render Pipeline/Swap URP Materials To Built-in")]
    public static void SwapMaterials()
    {
        var map = new Dictionary<string, string>
        {
            { "Universal Render Pipeline/Lit", "Standard" },
            { "Universal Render Pipeline/Simple Lit", "Standard" },
            { "Universal Render Pipeline/Unlit", "Unlit/Texture" },
            { "Universal Render Pipeline/Particles/Unlit", "Particles/Standard Unlit" },
            { "Universal Render Pipeline/Particles/Lit", "Particles/Standard Surface" },
            { "Universal Render Pipeline/Terrain/Lit", "Nature/Terrain/Standard" }
            // add any custom URP shaders -> built-in equivalents here
        };

        string[] guids = AssetDatabase.FindAssets("t:Material");
        int changed = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat || mat.shader == null) continue;

            string urpName = mat.shader.name;
            if (map.TryGetValue(urpName, out string builtinName))
            {
                var newShader = Shader.Find(builtinName);
                if (newShader != null)
                {
                    Undo.RecordObject(mat, "Swap URP Shader");
                    mat.shader = newShader;
                    EditorUtility.SetDirty(mat);
                    changed++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Swap Complete", $"Updated {changed} materials.", "OK");
    }
}
