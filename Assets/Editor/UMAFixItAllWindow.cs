// UMAFixItAllWindow.cs
// One-click UMA repair & diagnostics for Atavism projects.
// - Rebuilds UMA index (reflection, tolerant across UMA versions)
// - Checks Global Library, races, DNA converters, slots/overlays
// - Detects pipeline (Built-in/URP/HDRP) and can upgrade UMA shaders/materials
// - Scans for common Atavism wardrobe overrides
// - Attempts Atavism UMA index rebuild (if integration is present)
// - Optionally creates a temp Test Scene with UMA DCS prefab to validate visuals
//
// Place in: Assets/Editor/UMAFixItAllWindow.cs

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public class UMAFixItAllWindow : EditorWindow
{
    [MenuItem("Tools/UMA/Full Repair (Atavism)")]
    public static void Open() => GetWindow<UMAFixItAllWindow>("UMA Full Repair");

    bool autoFixMaterials = true;
    bool createTestScene = false;
    bool attemptAtavismFix = true;
    bool logVerbose = true;

    string report;
    string pipeline;
    int fixesApplied;

    void OnGUI()
    {
        GUILayout.Label("UMA Full Repair (Atavism)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        autoFixMaterials = EditorGUILayout.ToggleLeft("Auto-fix UMA materials/shaders for current Render Pipeline", autoFixMaterials);
        createTestScene = EditorGUILayout.ToggleLeft("Create Test Scene & spawn UMA DCS prefab", createTestScene);
        attemptAtavismFix = EditorGUILayout.ToggleLeft("Attempt Atavism UMA index rebuild (if present)", attemptAtavismFix);
        logVerbose = EditorGUILayout.ToggleLeft("Verbose console logging", logVerbose);

        EditorGUILayout.Space();
        if (GUILayout.Button("Run Full Repair", GUILayout.Height(32)))
        {
            RunFullRepair();
        }

        EditorGUILayout.Space();
        if (!string.IsNullOrEmpty(report))
        {
            GUILayout.Label("Last summary:", EditorStyles.boldLabel);
            var scroll = new Vector2();
            EditorGUILayout.TextArea(report, GUILayout.MinHeight(140));
            if (GUILayout.Button("Open Report File"))
            {
                EditorUtility.RevealInFinder("Assets/UMA_FullRepairReport.txt");
            }
        }
    }

    void RunFullRepair()
    {
        fixesApplied = 0;
        var lines = new List<string>();
        lines.Add($"=== UMA Full Repair Report ({DateTime.Now}) ===");

        // 1) Detect render pipeline
        pipeline = DetectPipeline();
        Log(lines, $"Render Pipeline: {pipeline}");

        // 2) Clear UMA caches (safe)
        TryClearUMACaches(lines);

        // 3) Rebuild UMA index (reflection)
        TryRebuildUMAIndex(lines);

        // 4) Global Library & core assets sanity
        ValidateGlobalLibraryAndCoreAssets(lines);

        // 5) Wardrobe overrides (Atavism)
        ScanAtavismWardrobeDefaults(lines);

        // 6) Materials & Shaders check and optional fix
        ValidateAndFixUMAMaterials(lines, autoFixMaterials);

        // 7) Force material refresh if UMA API found
        TryRefreshUMAMaterials(lines);

        // 8) Attempt Atavism UMA index rebuild
        if (attemptAtavismFix) TryAtavismUMAIndexRebuild(lines);

        // 9) Optional test scene with UMA DCS prefab
        if (createTestScene) CreateTestSceneWithUMADCS(lines);

        // 10) Save report
        SaveReport(lines);
        report = string.Join("\n", lines);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("UMA Full Repair", $"Completed. Fixes applied: {fixesApplied}\nSee Assets/UMA_FullRepairReport.txt", "OK");
    }

    // ----------------- Helpers -----------------

    string DetectPipeline()
    {
        var rp = GraphicsSettings.currentRenderPipeline;
        if (rp == null) return "Built-in (SRP disabled)";
        var t = rp.GetType().Name;
        if (t.Contains("Universal")) return "URP";
        if (t.Contains("HDRenderPipeline")) return "HDRP";
        return t;
    }

    void TryClearUMACaches(List<string> lines)
    {
        try
        {
            // Some projects store UMA caches under Library/UMA or Temp; we can only safely clear Editor prefs & request reimport.
            Log(lines, "- Clearing UMA-related editor state (requesting reimport of UMA assets).");
            var guids = AssetDatabase.FindAssets("t:ScriptableObject UMA");
            foreach (var g in guids) { /* marker */ }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        catch (Exception e)
        {
            Log(lines, $"! Clear cache step skipped: {e.Message}");
        }
    }

    void TryRebuildUMAIndex(List<string> lines)
    {
        try
        {
            // Try to locate UMA.UMAAssetIndexer and call RebuildIndex()
            var idxType = FindType("UMA.UMAAssetIndexer");
            if (idxType != null)
            {
                var getInstance = idxType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instance = getInstance?.GetValue(null);
                var rebuild = idxType.GetMethod("RebuildIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (instance != null && rebuild != null)
                {
                    rebuild.Invoke(instance, null);
                    fixesApplied++;
                    Log(lines, "+ UMA Asset Index rebuilt.");
                }
                else
                {
                    Log(lines, "! UMAAssetIndexer present but methods not found; skipping.");
                }
            }
            else
            {
                // Older/newer UMA may have a menu command; try to invoke via MenuItem
                if (!EditorApplication.ExecuteMenuItem("UMA/Global Library/Update Index"))
                    Log(lines, "! UMA indexer type not found and menu command unavailable.");
                else
                {
                    fixesApplied++;
                    Log(lines, "+ UMA index rebuilt via menu.");
                }
            }
        }
        catch (Exception e)
        {
            Log(lines, $"! UMA index rebuild failed: {e.Message}");
        }
    }

    void ValidateGlobalLibraryAndCoreAssets(List<string> lines)
    {
        try
        {
            var raceType = FindType("UMA.RaceData");
            var wardrobeType = FindType("UMA.WardrobeRecipe");
            var dnaConvType = FindType("UMA.DynamicDNAConverterController");
            var slotType = FindType("UMA.SlotDataAsset");
            var overlayType = FindType("UMA.OverlayData");
            var umaMaterialType = FindType("UMA.UMAMaterial");

            int races = CountAssetsOfType(raceType);
            int wardrobes = CountAssetsOfType(wardrobeType);
            int converters = CountAssetsOfType(dnaConvType);
            int slots = CountAssetsOfType(slotType);
            int overlays = CountAssetsOfType(overlayType);
            int umaMats = CountAssetsOfType(umaMaterialType);

            Log(lines, $"- Races: {races}, WardrobeRecipes: {wardrobes}, Converters: {converters}, Slots: {slots}, Overlays: {overlays}, UMAMaterials: {umaMats}");

            if (races == 0) Warn(lines, "No UMA Races found. Reimport UMA DCS or ensure Global Library includes Human races.");
            if (converters == 0) Warn(lines, "No DNA Converters found. Head shapes will be wrong without the correct converters.");
            if (slots == 0 || overlays == 0) Warn(lines, "Slots/Overlays missing. Heads can render invisible or generic.");
        }
        catch (Exception e)
        {
            Log(lines, $"! Global Library validation error: {e.Message}");
        }
    }

    void ScanAtavismWardrobeDefaults(List<string> lines)
    {
        try
        {
            // Look for common Atavism UMA avatar/wardrobe components by name to see if default overrides exist.
            // This is heuristic (no hard dependency): we only warn if something obvious is found.
            var avatarTypes = new[] { "Atavism.UMAAvatarSettings", "AtavismUMAAvatar", "Atavism.UMACharacter" };
            bool found = false;

            foreach (var typeName in avatarTypes)
            {
                var t = FindType(typeName);
                if (t == null) continue;

                var objs = GameObject.FindObjectsOfType(typeof(MonoBehaviour));
                foreach (var o in objs)
                {
                    var mb = o as MonoBehaviour;
                    if (mb == null) continue;
                    if (mb.GetType() == t)
                    {
                        found = true;
                        // Try to read a likely field that lists default wardrobe items
                        var f = t.GetField("defaultWardrobe", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (f != null)
                        {
                            var list = f.GetValue(mb) as System.Collections.IEnumerable;
                            int count = 0;
                            if (list != null) foreach (var _ in list) count++;
                            Log(lines, $"- Atavism avatar '{mb.name}' has defaultWardrobe entries: {count}. If head looks wrong, review these for head-slot overrides.");
                        }
                        else
                        {
                            Log(lines, $"- Found Atavism avatar '{mb.name}'. Review its UMA wardrobe or equip profiles for head overrides.");
                        }
                    }
                }
            }

            if (!found)
                Log(lines, "- No Atavism UMA avatar component detected in open scenes (that’s fine).");
        }
        catch (Exception e)
        {
            Log(lines, $"! Atavism wardrobe scan skipped: {e.Message}");
        }
    }

    void ValidateAndFixUMAMaterials(List<string> lines, bool autoFix)
    {
        try
        {
            var umaMaterialType = FindType("UMA.UMAMaterial");
            if (umaMaterialType == null) { Log(lines, "- UMAMaterial type not found; skipping material validation."); return; }

            var guids = AssetDatabase.FindAssets("t:UMAMaterial");
            int checkedCount = 0, upgraded = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset == null) continue;

                // Read 'material' field via reflection
                var matField = umaMaterialType.GetField("material", BindingFlags.Public | BindingFlags.Instance);
                if (matField == null) continue;

                var umaMat = asset;
                var unityMat = matField.GetValue(umaMat) as Material;
                checkedCount++;

                if (unityMat == null)
                {
                    Warn(lines, $"UMAMaterial missing Material: {path}");
                    continue;
                }

                // Check shader vs pipeline
                bool mismatch = false;
                if (pipeline == "URP" && unityMat.shader != null && !unityMat.shader.name.StartsWith("Universal Render Pipeline"))
                    mismatch = true;
                if (pipeline.StartsWith("Built-in") && unityMat.shader != null && unityMat.shader.name.StartsWith("Universal Render Pipeline"))
                    mismatch = true;

                if (mismatch)
                {
                    Warn(lines, $"Pipeline mismatch on {path} -> {unityMat.shader?.name}");
                    if (autoFix)
                    {
                        if (pipeline == "URP")
                        {
                            var lit = Shader.Find("Universal Render Pipeline/Lit");
                            if (lit != null)
                            {
                                unityMat.shader = lit;
                                EditorUtility.SetDirty(unityMat);
                                upgraded++;
                                fixesApplied++;
                            }
                        }
                        else // Built-in fallback
                        {
                            var standard = Shader.Find("Standard");
                            if (standard != null)
                            {
                                unityMat.shader = standard;
                                EditorUtility.SetDirty(unityMat);
                                upgraded++;
                                fixesApplied++;
                            }
                        }
                    }
                }
            }

            Log(lines, $"- UMAMaterials checked: {checkedCount}. Upgraded: {upgraded}.");
        }
        catch (Exception e)
        {
            Log(lines, $"! Material validation failed: {e.Message}");
        }
    }

    void TryRefreshUMAMaterials(List<string> lines)
    {
        try
        {
            // Call UMA Material Refresh if available
            var libType = FindType("UMA.UMAGlobalContext");
            if (libType == null) libType = FindType("UMA.UMAGlobalLibrary");

            if (libType != null)
            {
                // Some UMA versions expose a static method on a utility, try common names:
                bool called = EditorApplication.ExecuteMenuItem("UMA/Global Library/Update All Recipes");
                called |= EditorApplication.ExecuteMenuItem("UMA/Global Library/Rebuild CharacterSystem");

                if (called)
                {
                    fixesApplied++;
                    Log(lines, "+ Invoked UMA recipe/material refresh via menu.");
                }
                else
                {
                    Log(lines, "- UMA refresh menu not found; consider manually rebuilding DCS recipes.");
                }
            }
            else
            {
                Log(lines, "- UMA Global Library type not found; skipping refresh.");
            }
        }
        catch (Exception e)
        {
            Log(lines, $"! UMA material refresh failed: {e.Message}");
        }
    }

    void TryAtavismUMAIndexRebuild(List<string> lines)
    {
        try
        {
            // Try a known Atavism menu first; if not present, search for an integration class
            if (EditorApplication.ExecuteMenuItem("Atavism/UMA/Rebuild UMA Index"))
            {
                fixesApplied++;
                Log(lines, "+ Atavism UMA Index rebuilt via menu.");
                return;
            }

            // Try reflection fallback: look for types that might expose Rebuild/Refresh
            var candidates = new[]
            {
                "AtavismUMAIntegration", "Atavism.UMAIntegration", "Dragonsan.AtavismUMAIntegration", "Atavism.AtavismUMA"
            };
            foreach (var c in candidates)
            {
                var t = FindType(c);
                if (t == null) continue;
                var m = t.GetMethod("RebuildUMAIndex", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (m != null) { m.Invoke(null, null); fixesApplied++; Log(lines, $"+ Atavism UMA Index rebuilt via {c}.RebuildUMAIndex()."); return; }
            }

            Log(lines, "- Atavism UMA rebuild hook not found; continuing.");
        }
        catch (Exception e)
        {
            Log(lines, $"! Atavism UMA index rebuild failed: {e.Message}");
        }
    }

    void CreateTestSceneWithUMADCS(List<string> lines)
    {
        try
        {
            var tempPath = "Assets/Temp_UMA_TestScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Try to find UMA DCS prefab
            var dcsType = FindType("UMA.CharacterSystem.DynamicCharacterSystem");
            GameObject dcsGO = null;

            if (dcsType != null)
            {
                // Search a prefab that has DCS component
                var prefabGuid = AssetDatabase.FindAssets("t:Prefab UMA DCS")
                    .Concat(AssetDatabase.FindAssets("t:Prefab DynamicCharacterSystem"))
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(prefabGuid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        dcsGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        dcsGO.name = "UMA_DCS_Test";
                        Log(lines, $"+ Spawned UMA DCS prefab: {path}");
                    }
                }
            }

            if (dcsGO == null)
            {
                // Fallback: empty GO to at least place a marker
                var go = new GameObject("UMA_TestRoot");
                Log(lines, "- UMA DCS prefab not found; created empty marker object.");
            }

            // Add light & camera tuning a bit
            var light = GameObject.FindObjectOfType<Light>();
            if (light != null) light.intensity = 1.2f;

            EditorSceneManager.SaveScene(scene, tempPath);
            Log(lines, $"+ Test Scene saved at {tempPath}. Use it to validate head visuals without Atavism.");
        }
        catch (Exception e)
        {
            Log(lines, $"! Could not create test scene: {e.Message}");
        }
    }

    // ----------------- Utility -----------------

    static Type FindType(string fullName)
    {
        // Search loaded assemblies
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName, false);
            if (t != null) return t;
        }
        // Try slower search
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
            .FirstOrDefault(t => t.FullName == fullName || t.Name == fullName);
    }

    static int CountAssetsOfType(Type t)
    {
        if (t == null) return 0;
        var guids = AssetDatabase.FindAssets($"t:{t.Name}");
        return guids.Length;
    }

    void SaveReport(List<string> lines)
    {
        var path = "Assets/UMA_FullRepairReport.txt";
        File.WriteAllText(path, string.Join("\n", lines));
        AssetDatabase.ImportAsset(path);
        if (logVerbose) Debug.Log($"UMA Full Repair report saved: {path}");
    }

    void Log(List<string> lines, string msg)
    {
        lines.Add(msg);
        if (logVerbose) Debug.Log(msg);
    }

    void Warn(List<string> lines, string msg)
    {
        lines.Add("WARNING: " + msg);
        if (logVerbose) Debug.LogWarning(msg);
    }
}
