// Assets/Editor/SwapURPToBuiltinAndFixUMA.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UMA_AVAILABLE
using UMA;
using UMA.CharacterSystem;
#endif

public static class SwapURPToBuiltinAndFixUMA
{
    [MenuItem("Tools/Render Pipeline/Fix: Back To Built-in + UMA Visible")]
    public static void Run()
    {
        int changed = 0;
        var map = new Dictionary<string, string>
        {
            // URP -> Built-in
            { "Universal Render Pipeline/Lit", "Standard" },
            { "Universal Render Pipeline/Simple Lit", "Standard" },
            { "Universal Render Pipeline/Unlit", "Unlit/Texture" },
            { "Universal Render Pipeline/Particles/Unlit", "Particles/Standard Unlit" },
            { "Universal Render Pipeline/Particles/Lit", "Particles/Standard Surface" },
            { "Universal Render Pipeline/Terrain/Lit", "Nature/Terrain/Standard" },

            // Common UMA URP-ish fallbacks (some packs relabel to URP Lit)
            { "UMA/URP/Lit", "UMA/Standard" },
            { "UMA/URP/Transparent", "UMA/Transparent/Standard" },
        };

        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat || mat.shader == null) continue;

            // Swap known URP shaders
            if (map.TryGetValue(mat.shader.name, out string builtin))
            {
                var newShader = Shader.Find(builtin);
                if (newShader != null)
                {
                    Undo.RecordObject(mat, "Swap URP Shader");
                    mat.shader = newShader;
                    EditorUtility.SetDirty(mat);
                    changed++;
                }
            }

            // If using Built-in Standard or UMA Standard, force to opaque & visible
            var shName = mat.shader != null ? mat.shader.name : "";
            if (shName == "Standard" || shName.StartsWith("UMA/"))
            {
                // Restore alpha to 1 if it drifted to 0 (invisible)
                if (mat.HasProperty("_Color"))
                {
                    var c = mat.color;
                    if (c.a < 0.99f) { c.a = 1f; mat.color = c; }
                }

                // Force Opaque mode if it’s not a hair/transparent type
                bool isLikelyTransparent = shName.Contains("Transparent")
                                           || mat.name.ToLower().Contains("hair")
                                           || mat.name.ToLower().Contains("lash");

                if (!isLikelyTransparent)
                {
                    SetStandardOpaque(mat);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Rebuild UMA avatars in open scenes (if UMA present)
        int rebuilt = 0;
#if UMA_AVAILABLE
        var avatars = Object.FindObjectsOfType<UMA.CharacterSystem.DynamicCharacterAvatar>();
        foreach (var av in avatars)
        {
            try
            {
                av.BuildCharacter(false);
                rebuilt++;
            }
            catch { /* ignore */ }
        }
#endif

        EditorUtility.DisplayDialog("Done",
            $"Updated {changed} materials.\n" +
            $"{(rebuilt > 0 ? $"Rebuilt {rebuilt} UMA avatar(s)." : "Open scene with UMA avatars and run again to rebuild.")}",
            "OK");
    }

    // Built-in Standard: reliably set to Opaque
    static void SetStandardOpaque(Material mat)
    {
        // _Mode 0 = Opaque
        if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 0f);

        mat.renderQueue = -1; // use default
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0f);

        // Standard surface settings
        if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 1);
    }
}
