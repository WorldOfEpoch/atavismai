// URPMaterialAutoFixer.cs (v4)
// Adds NatureManufacture mappings, GUI/Text → URP/Unlit alpha,
// treats Shader Graphs as already URP, and skips modifying Packages/* (read-only).

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class URPMaterialAutoFixer : EditorWindow
{
    [Serializable]
    private class ShaderMapping
    {
        public string SourceShaderName;        // exact or prefix
        public string TargetShaderName;        // URP shader
        public Func<Material, bool> Converter; // optional converter after swap
        public bool StartsWithMatch;
    }

    private static readonly List<ShaderMapping> ShaderMap = new()
    {
        // ----- Unity Standard workflows -----
        MapExact("Standard", "Universal Render Pipeline/Lit", ConvertFromStandardMetallic),
        MapExact("Standard (Specular setup)", "Universal Render Pipeline/Lit", ConvertFromStandardSpecular),

        // ----- Legacy common -----
        MapExact("Legacy Shaders/Diffuse", "Universal Render Pipeline/Simple Lit", ConvertFromLegacyDiffuse),
        MapExact("Legacy Shaders/Transparent/Cutout/Diffuse", "Universal Render Pipeline/Simple Lit", ConvertFromLegacyCutout),
        MapExact("Unlit/Color", "Universal Render Pipeline/Unlit", ConvertBasicUnlit),
        MapExact("Unlit/Texture", "Universal Render Pipeline/Unlit", ConvertBasicUnlit),

        // ----- Unity Particles -----
        MapExact("Particles/Standard Surface", "Universal Render Pipeline/Particles/Simple Lit", ConvertParticlesTransparent),
        MapExact("Particles/Standard Unlit",   "Universal Render Pipeline/Particles/Unlit",      ConvertParticlesTransparent),

        // ----- Legacy Particles variants -----
        MapExact("Legacy Shaders/Particles/Alpha Blended",              "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAlpha),
        MapExact("Legacy Shaders/Particles/Alpha Blended Premultiply",  "Universal Render Pipeline/Particles/Unlit", ConvertParticlesPremultiply),
        MapExact("Legacy Shaders/Particles/Additive",                   "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAdditive),
        MapExact("Legacy Shaders/Particles/Multiply",                   "Universal Render Pipeline/Particles/Unlit", ConvertParticlesMultiply),
        MapExact("Legacy Shaders/Particles/VertexLit Blended",          "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAlpha),
        MapExact("Legacy Shaders/Particles/Alpha Premultiply",          "Universal Render Pipeline/Particles/Unlit", ConvertParticlesPremultiply),
        MapExact("Legacy Shaders/Particles/VertexLit",                  "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAlpha),
        MapExact("Legacy Shaders/Particles/Cutout",                     "Universal Render Pipeline/Particles/Unlit", ConvertParticlesCutout),

        // ----- Mobile fallbacks -----
        MapStartsWith("Mobile/", "Universal Render Pipeline/Simple Lit", ConvertMobileToSimpleLit),

        // ===== AQUAS =====
        MapStartsWith("AQUAS/Desktop/Front",        "Universal Render Pipeline/Lit",   ConvertAquasFrontWater),
        MapStartsWith("AQUAS/Desktop/Front Opaque", "Universal Render Pipeline/Lit",   ConvertAquasFrontWater),
        MapStartsWith("AQUAS/Misc/Caustic",         "Universal Render Pipeline/Unlit", ConvertAquasCaustics),
        MapStartsWith("AQUAS/Misc",                 "Universal Render Pipeline/Unlit", ConvertTransparentUnlit),
        MapStartsWith("Hidden/AQUAS",               "Universal Render Pipeline/Unlit", ConvertHiddenFx), // gated by toggle

        // ===== NatureManufacture =====
        // Standard (incl. Snow, UV Free Faces) → URP/Lit
        MapStartsWith("NatureManufacture Shaders/Standard Shaders/Standard Metallic", "Universal Render Pipeline/Lit", ConvertNMLit),
        MapStartsWith("NatureManufacture Shaders/Standard Shaders/Standard",          "Universal Render Pipeline/Lit", ConvertNMLit),

        // Water particles
        MapStartsWith("NatureManufacture Shaders/Water/Water Particles Foam", "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAdditive),
        MapStartsWith("NatureManufacture Shaders/Water/Water Particles",      "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAlpha),

        // GUI/Text (fonts etc.)
        MapExact("GUI/Text Shader", "Universal Render Pipeline/Unlit", ConvertGuiTextAlpha),
    };

    // ---------- UI ----------
    private bool _analyzeOnly = true;
    private bool _backupMaterials = true;
    private string _backupFolder = "Assets/URP_Backups_Materials";
    private bool _enableHiddenFallbacks = false; // Hidden/AQUAS → Unlit placeholder
    private bool _friendlyAlreadyUrp = true;     // nicer log for already-URP
    private Vector2 _scroll;
    private readonly List<string> _log = new();

    [MenuItem("Tools/URP/Auto-Fix Materials")]
    private static void OpenWindow()
    {
        var w = GetWindow<URPMaterialAutoFixer>("URP Material Auto-Fixer");
        w.minSize = new Vector2(720, 520);
        w.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("URP Material Auto-Fixer", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _analyzeOnly = EditorGUILayout.ToggleLeft("Dry-run (Analyze only)", _analyzeOnly);

            EditorGUILayout.Space(2);
            _backupMaterials = EditorGUILayout.ToggleLeft("Backup modified materials", _backupMaterials);
            using (new EditorGUI.DisabledScope(!_backupMaterials))
                _backupFolder = EditorGUILayout.TextField("Backup Folder", _backupFolder);

            EditorGUILayout.Space(2);
            _enableHiddenFallbacks = EditorGUILayout.ToggleLeft("Enable Hidden/* fallbacks (e.g., AQUAS Underwater → URP/Unlit)", _enableHiddenFallbacks);
            _friendlyAlreadyUrp = EditorGUILayout.ToggleLeft("Treat 'already URP' as OK (nicer log)", _friendlyAlreadyUrp);
        }

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(_analyzeOnly ? "Analyze Project" : "Convert Project", GUILayout.Height(28)))
                Run(_analyzeOnly);

            if (GUILayout.Button("Clear Log", GUILayout.Height(28)))
                _log.Clear();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var line in _log) EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();
    }

    private void Run(bool analyzeOnly)
    {
        _log.Clear();

        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        if (rp == null || rp.GetType().Name.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) < 0)
            Log("⚠️ Current Render Pipeline asset is not URP. You can still analyze/convert, but results may vary.");
        else
            Log($"✅ Detected URP Render Pipeline Asset: {rp.name}");

        if (!analyzeOnly && _backupMaterials) EnsureFolder(_backupFolder);

        var guids = AssetDatabase.FindAssets("t:Material");
        int total = guids.Length, changed = 0, skipped = 0, already = 0, hiddenSkipped = 0, pkgSkipped = 0, shaderGraphOK = 0;

        Log($"Scanning {total} materials…");
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { skipped++; continue; }

                var result = ProcessMaterial(mat, analyzeOnly, path, ref shaderGraphOK, ref pkgSkipped, ref hiddenSkipped);
                switch (result)
                {
                    case MatResult.Changed: changed++; break;
                    case MatResult.AlreadyUrp: already++; break;
                    case MatResult.Skipped: skipped++; break;
                    case MatResult.HiddenSkipped: hiddenSkipped++; break;
                    case MatResult.PackageSkip: pkgSkipped++; break;
                    case MatResult.ShaderGraphOk: shaderGraphOK++; break;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
        Log($"Done. Changed: {changed}, Already URP: {already}, ShaderGraph OK: {shaderGraphOK}, Hidden-skipped: {hiddenSkipped}, Packages-skipped: {pkgSkipped}, Other Skipped: {skipped}, Total: {total}");
    }

    private enum MatResult { Skipped, Changed, AlreadyUrp, HiddenSkipped, PackageSkip, ShaderGraphOk }

    private MatResult ProcessMaterial(Material mat, bool analyzeOnly, string path, ref int shaderGraphOK, ref int pkgSkipped, ref int hiddenSkipped)
    {
        string src = mat.shader ? mat.shader.name : "(no shader)";

        // 0) Package assets are read-only — don’t touch
        if (path.StartsWith("Packages/", StringComparison.Ordinal))
        {
            Log($"• SKIP (Packages/ read-only): {path} — '{src}'");
            return MatResult.PackageSkip;
        }

        // 1) Already URP?
        if (src.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal))
        {
            if (_friendlyAlreadyUrp) Log($"OK : {path} — already URP (‘{src}’)");
            else Log($"• SKIP: {path} — already URP (‘{src}’)");
            return MatResult.AlreadyUrp;
        }

        // 1b) Shader Graphs are URP/SRP content by design — mark OK
        if (src.StartsWith("Shader Graphs/", StringComparison.Ordinal))
        {
            Log($"OK : {path} — Shader Graph (‘{src}’) — treated as URP-ready");
            return MatResult.ShaderGraphOk;
        }

        // 2) Hidden/Internal — do not change unless enabled (AQUAS only)
        if (src.StartsWith("Hidden/Internal", StringComparison.Ordinal))
        {
            Log($"• SKIP (Hidden/Internal): {path} — '{src}' (post-effect/internal)");
            return MatResult.HiddenSkipped;
        }

        // 3) Find mapping
        var mapping = FindMappingForShader(src) ?? TryCustomPackMappings(mat, src);

        // Hidden/AQUAS fallback off?
        if (mapping != null && mapping.SourceShaderName.StartsWith("Hidden/AQUAS", StringComparison.Ordinal) && !_enableHiddenFallbacks)
        {
            Log($"• SKIP (Hidden disabled): {path} — '{src}' (toggle fallback to convert)");
            return MatResult.HiddenSkipped;
        }

        // Heuristics
        if (mapping == null)
        {
            if (src.StartsWith("Legacy Shaders/Particles/", StringComparison.Ordinal))
                mapping = MapExact(src, "Universal Render Pipeline/Particles/Unlit", ConvertParticlesAlpha);
            else if (src.StartsWith("AQUAS/", StringComparison.Ordinal))
                mapping = MapExact(src, "Universal Render Pipeline/Lit", ConvertAquasFrontWater);
        }

        if (mapping == null)
        {
            Log($"• SKIP: {path} — no mapping for '{src}'");
            return MatResult.Skipped;
        }

        var targetShader = Shader.Find(mapping.TargetShaderName);
        if (targetShader == null)
        {
            Log($"• SKIP: {path} — target shader missing: '{mapping.TargetShaderName}'");
            return MatResult.Skipped;
        }

        if (_analyzeOnly)
        {
            Log($"• {path}  '{src}'  →  '{mapping.TargetShaderName}'");
            return MatResult.Skipped;
        }

        if (_backupMaterials)
        {
            var newPath = MakeBackup(path, _backupFolder);
            Log($"  ↳ backup: {newPath}");
        }

        try
        {
            ConvertCommonPropertiesBeforeSwap(mat);
            mat.shader = targetShader;
            ConvertCommonPropertiesAfterSwap(mat);
            mapping.Converter?.Invoke(mat);

            EditorUtility.SetDirty(mat);
            Log($"• FIXED: {path}  '{src}'  →  '{mapping.TargetShaderName}'");
            return MatResult.Changed;
        }
        catch (Exception ex)
        {
            Log($"• ERROR: {path} — {ex.Message}");
            return MatResult.Skipped;
        }
    }

    // ---------- Common copy helpers ----------
    private class CommonProps
    {
        public Color Color = Color.white;
        public float Cutoff = 0.5f;
        public Texture MainTex;
        public Vector2 MainTexScale = Vector2.one;
        public Vector2 MainTexOffset = Vector2.zero;
        public Texture MetallicGlossMap;
        public float Metallic = 0f;
        public float Smoothness = 0.5f;
        public Texture BumpMap;
        public float BumpScale = 1f;
        public Texture OcclusionMap;
        public float OcclusionStrength = 1f;
        public Texture EmissionMap;
        public Color EmissionColor = Color.black;
        public int SrcBlend = (int)UnityEngine.Rendering.BlendMode.One;
        public int DstBlend = (int)UnityEngine.Rendering.BlendMode.Zero;
        public int ZWrite = 1;
        public float Surface = 0f; // 0 Opaque, 1 Transparent
        public float AlphaClip = 0f;
    }
    private static CommonProps _stash;

    private static void ConvertCommonPropertiesBeforeSwap(Material m)
    {
        _stash = new CommonProps();
        if (m.HasProperty("_Color")) _stash.Color = m.GetColor("_Color");
        if (m.HasProperty("_Cutoff")) _stash.Cutoff = m.GetFloat("_Cutoff");

        if (m.HasProperty("_MainTex"))
        {
            _stash.MainTex = m.GetTexture("_MainTex");
            _stash.MainTexScale = m.GetTextureScale("_MainTex");
            _stash.MainTexOffset = m.GetTextureOffset("_MainTex");
        }

        if (m.HasProperty("_MetallicGlossMap")) _stash.MetallicGlossMap = m.GetTexture("_MetallicGlossMap");
        if (m.HasProperty("_Metallic")) _stash.Metallic = m.GetFloat("_Metallic");
        if (m.HasProperty("_Glossiness")) _stash.Smoothness = m.GetFloat("_Glossiness");
        else if (m.HasProperty("_Smoothness")) _stash.Smoothness = m.GetFloat("_Smoothness");

        if (m.HasProperty("_BumpMap")) _stash.BumpMap = m.GetTexture("_BumpMap");
        if (m.HasProperty("_BumpScale")) _stash.BumpScale = m.GetFloat("_BumpScale");

        if (m.HasProperty("_OcclusionMap")) _stash.OcclusionMap = m.GetTexture("_OcclusionMap");
        if (m.HasProperty("_OcclusionStrength")) _stash.OcclusionStrength = m.GetFloat("_OcclusionStrength");

        if (m.IsKeywordEnabled("_EMISSION") || m.HasProperty("_EmissionMap") || m.HasProperty("_EmissionColor"))
        {
            if (m.HasProperty("_EmissionMap")) _stash.EmissionMap = m.GetTexture("_EmissionMap");
            if (m.HasProperty("_EmissionColor")) _stash.EmissionColor = m.GetColor("_EmissionColor");
        }

        int mode = m.HasProperty("_Mode") ? (int)m.GetFloat("_Mode") : 0;
        _stash.AlphaClip = mode == 1 ? 1f : 0f;
        _stash.Surface = (mode >= 2) ? 1f : 0f;

        if (m.HasProperty("_SrcBlend")) _stash.SrcBlend = (int)m.GetFloat("_SrcBlend");
        if (m.HasProperty("_DstBlend")) _stash.DstBlend = (int)m.GetFloat("_DstBlend");
        if (m.HasProperty("_ZWrite")) _stash.ZWrite = (int)m.GetFloat("_ZWrite");
    }

    private static void ConvertCommonPropertiesAfterSwap(Material m)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", _stash.Color);
        if (m.HasProperty("_BaseMap"))
        {
            m.SetTexture("_BaseMap", _stash.MainTex);
            m.SetTextureScale("_BaseMap", _stash.MainTexScale);
            m.SetTextureOffset("_BaseMap", _stash.MainTexOffset);
        }
        else if (m.HasProperty("_MainTex"))
        {
            m.SetTexture("_MainTex", _stash.MainTex);
            m.SetTextureScale("_MainTex", _stash.MainTexScale);
            m.SetTextureOffset("_MainTex", _stash.MainTexOffset);
        }

        if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", _stash.AlphaClip);
        if (_stash.AlphaClip > 0 && m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff", _stash.Cutoff);
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", _stash.Surface);

        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", _stash.Metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", _stash.Smoothness);
        if (m.HasProperty("_MetallicGlossMap")) m.SetTexture("_MetallicGlossMap", _stash.MetallicGlossMap);

        if (m.HasProperty("_BumpMap")) m.SetTexture("_BumpMap", _stash.BumpMap);
        if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", _stash.BumpScale);

        if (m.HasProperty("_OcclusionMap")) m.SetTexture("_OcclusionMap", _stash.OcclusionMap);
        if (m.HasProperty("_OcclusionStrength")) m.SetFloat("_OcclusionStrength", _stash.OcclusionStrength);

        if (_stash.EmissionMap || _stash.EmissionColor.maxColorComponent > 0.0001f)
        {
            if (m.HasProperty("_EmissionMap")) m.SetTexture("_EmissionMap", _stash.EmissionMap);
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", _stash.EmissionColor);
            m.EnableKeyword("_EMISSION");
        }
    }

    // ---------- Converters ----------
    private static bool ConvertFromStandardMetallic(Material m) { if (m.HasProperty("_WorkflowMode")) m.SetFloat("_WorkflowMode", 1f); return true; }
    private static bool ConvertFromStandardSpecular(Material m) { if (m.HasProperty("_WorkflowMode")) m.SetFloat("_WorkflowMode", 0f); return true; }
    private static bool ConvertFromLegacyDiffuse(Material m) { if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f); if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", Mathf.Min(m.GetFloat("_Smoothness"), 0.2f)); return true; }
    private static bool ConvertFromLegacyCutout(Material m) { if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 1f); return ConvertFromLegacyDiffuse(m); }
    private static bool ConvertBasicUnlit(Material m) => true;

    private static void MakeTransparent(Material m) { if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f); m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; }
    private static void SetBlend(Material m, UnityEngine.Rendering.BlendMode src, UnityEngine.Rendering.BlendMode dst)
    { if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)src); if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)dst); }

    private static bool ConvertParticlesTransparent(Material m) { MakeTransparent(m); return true; }
    private static bool ConvertParticlesAlpha(Material m) { MakeTransparent(m); SetBlend(m, UnityEngine.Rendering.BlendMode.SrcAlpha, UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); return true; }
    private static bool ConvertParticlesPremultiply(Material m) { MakeTransparent(m); SetBlend(m, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); return true; }
    private static bool ConvertParticlesAdditive(Material m) { MakeTransparent(m); SetBlend(m, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.One); return true; }
    private static bool ConvertParticlesMultiply(Material m) { MakeTransparent(m); SetBlend(m, UnityEngine.Rendering.BlendMode.DstColor, UnityEngine.Rendering.BlendMode.Zero); return true; }
    private static bool ConvertParticlesCutout(Material m) { if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 1f); return true; }
    private static bool ConvertMobileToSimpleLit(Material m) => true;

    private static bool ConvertAquasFrontWater(Material m) { MakeTransparent(m); if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f); if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", Mathf.Max(0.6f, m.GetFloat("_Smoothness"))); return true; }
    private static bool ConvertAquasCaustics(Material m) { MakeTransparent(m); SetBlend(m, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.One); return true; }
    private static bool ConvertTransparentUnlit(Material m) { MakeTransparent(m); return true; }
    private static bool ConvertHiddenFx(Material m) { MakeTransparent(m); return true; }

    // NatureManufacture "Standard" family (incl. Snow / UV Free Faces)
    private static bool ConvertNMLit(Material m)
    {
        // Sensible defaults; let artists tweak.
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", Mathf.Clamp01(m.GetFloat("_Metallic")));
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", Mathf.Clamp01(m.GetFloat("_Smoothness")));
        return true;
    }

    // GUI/Text → URP/Unlit alpha blending
    private static bool ConvertGuiTextAlpha(Material m)
    {
        MakeTransparent(m);
        SetBlend(m, UnityEngine.Rendering.BlendMode.SrcAlpha, UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        return true;
    }

    // ---------- Mapping helpers ----------
    private static ShaderMapping MapExact(string src, string dst, Func<Material, bool> fn = null)
        => new ShaderMapping { SourceShaderName = src, TargetShaderName = dst, Converter = fn, StartsWithMatch = false };
    private static ShaderMapping MapStartsWith(string srcPrefix, string dst, Func<Material, bool> fn = null)
        => new ShaderMapping { SourceShaderName = srcPrefix, TargetShaderName = dst, Converter = fn, StartsWithMatch = true };

    private static ShaderMapping FindMappingForShader(string srcName)
    {
        foreach (var m in ShaderMap)
        {
            if (!m.StartsWithMatch && string.Equals(m.SourceShaderName, srcName, StringComparison.Ordinal)) return m;
            if (m.StartsWithMatch && srcName.StartsWith(m.SourceShaderName, StringComparison.Ordinal)) return m;
        }
        return null;
    }

    // Hook for more packs (CTS, MicroSplat, etc.)
    private static ShaderMapping TryCustomPackMappings(Material mat, string srcName)
    {
        // Add project-specific redirects here if needed
        return null;
    }

    // ---------- IO & logging ----------
    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        var parts = folder.Replace("\\", "/").Split('/');
        string acc = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            var next = acc + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(acc, parts[i]);
            acc = next;
        }
    }
    private static string MakeBackup(string path, string rootFolder)
    {
        var fileName = Path.GetFileName(path);
        var dest = Path.Combine(rootFolder, fileName).Replace("\\", "/");
        dest = AssetDatabase.GenerateUniqueAssetPath(dest);
        AssetDatabase.CopyAsset(path, dest);
        return dest;
    }
    private void Log(string s)
    {
        _log.Add(s);
        if (s.StartsWith("• ") || s.StartsWith("OK") || s.StartsWith("  ↳ ") || s.StartsWith("• ERROR")) Debug.Log(s);
    }
}
#endif
