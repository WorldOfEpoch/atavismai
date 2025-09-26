// Assets/Editor/UMAItemFactoryUltra.cs
// Unity 2020+ / UMA 2 (DCS) / Atavism 10.11+ helper
// Tabs: Settings • Scan/Group/Build • Drag&Drop • CSV • LLM Generate • Export • Repair
//
// This version avoids hard references to UMA packed recipe types.
// It builds Wardrobe Recipes via reflection so it works across UMA DCS variants.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

using UMA;
using UMA.CharacterSystem;

public class UMAItemFactoryUltra : EditorWindow
{
    // ========================== Settings ScriptableObjects ==========================
    [Serializable]
    public class ThemeSettings : ScriptableObject
    {
        public string themeName = "World of Epoch";
        [TextArea(3, 10)] public string loreSeed = "Ancient crystals, neon steel, frontier guilds.";
        public string[] allowedWardrobeSlots = new[] { "Chest", "Legs", "Helmet", "Hands", "Feet", "Shoulders" };
        public string[] races = new[] { "HumanMale", "HumanFemale" };
        public UMAMaterial defaultUMAMaterial;
        public string defaultWardrobeSlot = "Chest";
        public string[] namePrefixes = new[] { "Iron", "Bronze", "Warden's", "Epoch", "Starlit", "Rogue's" };
        public string[] nameCores = new[] { "Breastplate", "Greaves", "Helm", "Gauntlets", "Sabatons", "Spaulders" };
        public string[] nameSuffixes = new[] { "of Embers", "Mk II", "Prototype", "of the Rift", "Δ-9" };
        public string[] qualities = new[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        public string[] itemTypes = new[] { "Armor" };
        public string[] subTypes = new[] { "Plate", "Mail", "Leather", "Cloth" };
        public int minLevel = 1;
        public int maxLevel = 50;
    }

    [Serializable]
    public class LLMSettings : ScriptableObject
    {
        public bool useLLM = false;
        public string openAICompatibleEndpoint = "http://localhost:11434/v1/chat/completions";
        public string apiKey = "";
        public string model = "llama3.1:8b";

        [TextArea(3, 10)]
        public string systemPrompt =
@"You create MMORPG item rows for bulk import.
OUTPUT STRICTLY AS CSV with header:
itemName,wardrobeSlot,races,quality,itemType,subType,level,description
No markdown, no code fences—CSV only.";

        [TextArea(3, 10)]
        public string userPromptTemplate =
@"Theme: {THEME}
Lore seed: {LORE}
Allowed wardrobe slots: {SLOTS}
Races: {RACES}
Generate {COUNT} items with varied qualities and levels. Keep names short, evocative.
CSV only.";
    }

    [Serializable]
    public class ExportProfile : ScriptableObject
    {
        public string exportFolder = "Assets/UMA_Auto_Central/Exports";
        public bool writeCsv = true;
        public bool writeSql = false;

        public string[] csvHeader = new[] {
            "templateId","name","displayName","equipSlot","quality","itemType","subType","requiredLevel",
            "iconPath","prefabPath","stackLimit","bindType","sellPrice","statsJson","wardrobeRecipePath"
        };

        public int defaultStackLimit = 1;
        public string defaultBindType = "None";
        public int defaultSellPrice = 0;
    }

    [Serializable]
    public class BuildItem
    {
        public string itemName;
        public string wardrobeSlot;
        public string[] races;
        public UMAMaterial umaMaterial;     // optional override

        public string fbxPath;              // Assets/...
        public string diffusePath;          // Assets/...
        public string normalPath;           // Assets/...
        public string specPath;             // Assets/...

        public string quality;              // Common/Uncommon/...
        public string itemType;             // Armor/Weapon/...
        public string subType;              // Plate/Mail/...
        public int level = 1;
        public string description;

        public string slotAssetPath;
        public string overlayAssetPath;
        public string recipeAssetPath;
        public string prefabAssetPath;
    }

    [Serializable]
    public class ItemCatalog : ScriptableObject
    {
        public List<BuildItem> items = new List<BuildItem>();
    }

    // =============================== In-memory state ===============================
    ThemeSettings theme;
    LLMSettings llm;
    ExportProfile exportProfile;
    ItemCatalog catalog;

    string centralRoot = "Assets/UMA_Auto_Central";
    string SlotsDir => $"{centralRoot}/UMA/Slots";
    string OverlaysDir => $"{centralRoot}/UMA/Overlays";
    string RecipesDir => $"{centralRoot}/UMA/Recipes";
    string ItemsDir => $"{centralRoot}/Items";
    string AtavismDir => $"{centralRoot}/Atavism/Items";

    bool dryRun = false;
    bool skipExisting = true;
    bool onlyCreateMissing = true;
    bool forceOverwrite = false;

    string diffuseTag = "diff";
    string normalTag = "norm";
    string specTag = "spec";

    DefaultAsset scanSourceFolder;
    bool recursiveScan = true;

    TextAsset csvFile;
    char csvDelimiter = ',';

    Rect dropArea;

    enum Tab { Settings, ScanBuild, DragDrop, CSV, LLMGenerate, Export, Repair }
    Tab tab = Tab.Settings;

    [MenuItem("Tools/UMA/UMA Item Factory Ultra")]
    public static void Open() => GetWindow<UMAItemFactoryUltra>("UMA Item Factory Ultra");

    void OnEnable()
    {
        theme = LoadOrCreate<ThemeSettings>("Assets/UMA_Auto_Central/Settings/ThemeSettings.asset");
        llm = LoadOrCreate<LLMSettings>("Assets/UMA_Auto_Central/Settings/LLMSettings.asset");
        exportProfile = LoadOrCreate<ExportProfile>("Assets/UMA_Auto_Central/Settings/ExportProfile.asset");
        catalog = LoadOrCreate<ItemCatalog>("Assets/UMA_Auto_Central/Settings/ItemCatalog.asset");
    }

    T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var obj = AssetDatabase.LoadAssetAtPath<T>(path);
        if (!obj)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            obj = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(obj, path);
            AssetDatabase.SaveAssets();
        }
        return obj;
    }

    void OnGUI()
    {
        tab = (Tab)GUILayout.Toolbar((int)tab, Enum.GetNames(typeof(Tab)));
        EditorGUILayout.Space();

        switch (tab)
        {
            case Tab.Settings: DrawSettings(); break;
            case Tab.ScanBuild: DrawScanBuild(); break;
            case Tab.DragDrop: DrawDragDrop(); break;
            case Tab.CSV: DrawCSV(); break;
            case Tab.LLMGenerate: DrawLLM(); break;
            case Tab.Export: DrawExport(); break;
            case Tab.Repair: DrawRepair(); break;
        }
    }

    // ================================ UI: Settings ================================
    void DrawSettings()
    {
        EditorGUILayout.LabelField("Central & Behavior", EditorStyles.boldLabel);
        centralRoot = EditorGUILayout.TextField("Central Root", centralRoot);
        dryRun = EditorGUILayout.Toggle("Dry Run (no writes)", dryRun);
        skipExisting = EditorGUILayout.Toggle("Skip Existing", skipExisting);
        onlyCreateMissing = EditorGUILayout.Toggle("Only Create Missing", onlyCreateMissing);
        forceOverwrite = EditorGUILayout.Toggle("Force Overwrite", forceOverwrite);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detection Tags", EditorStyles.boldLabel);
        diffuseTag = EditorGUILayout.TextField("Diffuse Tag", diffuseTag);
        normalTag = EditorGUILayout.TextField("Normal Tag", normalTag);
        specTag = EditorGUILayout.TextField("Spec/Metal Tag", specTag);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Theme Settings", EditorStyles.boldLabel);
        Editor.CreateEditor(theme).OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("LLM Settings (optional)", EditorStyles.boldLabel);
        Editor.CreateEditor(llm).OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Export Profile", EditorStyles.boldLabel);
        Editor.CreateEditor(exportProfile).OnInspectorGUI();

        if (GUILayout.Button("Save Settings")) { AssetDatabase.SaveAssets(); }
    }

    // ============================ UI: Scan / Group / Build ============================
    void DrawScanBuild()
    {
        EditorGUILayout.LabelField("Scan & Build", EditorStyles.boldLabel);
        scanSourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Source Folder", scanSourceFolder, typeof(DefaultAsset), false);
        recursiveScan = EditorGUILayout.Toggle("Recursive Scan", recursiveScan);

        if (GUILayout.Button("Scan → Group → Build"))
        {
            EnsureDirs();

            var items = FindItemsByFolder(scanSourceFolder, recursiveScan);
            if (items.Count == 0) { Log("No FBX found under source."); return; }

            foreach (var it in items)
            {
                try { GroupAndBuild(it); CatalogUpsert(it); }
                catch (Exception ex) { Debug.LogError($"[{it.itemName}] {ex.Message}\n{ex.StackTrace}"); }
            }
            if (!dryRun) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            Log("Scan complete.");
        }
    }

    // ================================= UI: Drag & Drop =================================
    void DrawDragDrop()
    {
        EditorGUILayout.HelpBox("Drop an FBX or folder here → a small dialog will confirm name, slot, races, material.", MessageType.Info);
        dropArea = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Here");

        var e = Event.current;
        if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && dropArea.Contains(e.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    var it = BuildItemFromAnyPath(path);
                    if (it != null) UMAItemWizard.Open(this, it, theme);
                }
            }
            Event.current.Use();
        }
    }

    // =================================== UI: CSV ===================================
    void DrawCSV()
    {
        EditorGUILayout.LabelField("CSV Import", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        csvDelimiter = EditorGUILayout.TextField("Delimiter", csvDelimiter.ToString())[0];

        EditorGUILayout.HelpBox(
@"Required headers:
itemName,wardrobeSlot,races,quality,itemType,subType,level,description,fbxPath,diffusePath,normalPath,specPath,umaMaterialPath
- races: semicolon separated, e.g. HumanMale;HumanFemale
- any path can be blank to auto-detect by tags", MessageType.Info);

        if (GUILayout.Button("Run CSV"))
        {
            if (!csvFile) { Log("No CSV provided."); return; }
            EnsureDirs();

            var rows = ParseCsv(csvFile.text, csvDelimiter);
            foreach (var row in rows)
            {
                try { var it = ItemFromCsvRow(row); GroupAndBuild(it); CatalogUpsert(it); }
                catch (Exception ex) { Debug.LogError($"[CSV] {ex.Message}\n{ex.StackTrace}"); }
            }

            if (!dryRun) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            Log("CSV complete.");
        }
    }

    // ================================ UI: LLM Generate ================================
    int llmCount = 10;
    void DrawLLM()
    {
        EditorGUILayout.LabelField("LLM Generator", EditorStyles.boldLabel);
        llm.useLLM = EditorGUILayout.Toggle("Use LLM", llm.useLLM);
        llmCount = EditorGUILayout.IntSlider("How many items", llmCount, 1, 100);

        if (GUILayout.Button("Generate → Build"))
        {
            EnsureDirs();
            List<Dictionary<string, string>> rows;

            if (llm.useLLM)
            {
                var csv = CallLLMSynchronously(llm, theme, llmCount);
                if (string.IsNullOrWhiteSpace(csv))
                {
                    Debug.LogWarning("LLM returned empty or failed; using rule-based generator.");
                    rows = RuleBasedCSV(theme, llmCount);
                }
                else rows = ParseCsv(csv, ','); // returns List<Dictionary<string,string>>
            }
            else
            {
                rows = RuleBasedCSV(theme, llmCount);
            }

            foreach (var row in rows)
            {
                try { var it = ItemFromCsvRow(row, fallbackWardrobeSlotFromTheme: true); GroupAndBuild(it); CatalogUpsert(it); }
                catch (Exception ex) { Debug.LogError($"[LLM] {ex.Message}\n{ex.StackTrace}"); }
            }

            if (!dryRun) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            Log("LLM/Rule generate complete.");
        }

        EditorGUILayout.HelpBox("Tip: Start your local model with an OpenAI-compatible endpoint (e.g., LM Studio, Ollama w/ openai-compat). No API key needed for local unless configured.", MessageType.Info);
    }

    // ================================== UI: Export ==================================
    void DrawExport()
    {
        EditorGUILayout.LabelField("Export Atavism Bulk Templates", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Exports a CSV of your catalog (and optional SQL). Adjust mapping later if needed in Atavism Editor.", MessageType.Info);

        if (GUILayout.Button("Export Now"))
        {
            var exportDir = exportProfile.exportFolder;
            Directory.CreateDirectory(exportDir);

            var csvPath = Path.Combine(exportDir, "atavism_items.csv").Replace("\\", "/");
            var sqlPath = Path.Combine(exportDir, "atavism_items.sql").Replace("\\", "/");

            if (exportProfile.writeCsv)
                File.WriteAllText(csvPath, BuildAtavismCSV(catalog.items, exportProfile));
            if (exportProfile.writeSql)
                File.WriteAllText(sqlPath, BuildAtavismSQL(catalog.items));

            AssetDatabase.Refresh();
            Log($"Exported: {(exportProfile.writeCsv ? csvPath : "")} {(exportProfile.writeSql ? sqlPath : "")}");
        }
    }

    // ================================== UI: Repair ==================================
    void DrawRepair()
    {
        EditorGUILayout.HelpBox("Repair checks catalog entries and (re)creates any missing UMA assets or Atavism prefabs.", MessageType.Info);
        if (GUILayout.Button("Repair Missing Assets"))
        {
            EnsureDirs();
            foreach (var it in catalog.items.ToList())
            {
                try { GroupAndBuild(it, repairMode: true); }
                catch (Exception ex) { Debug.LogError($"[Repair] {it.itemName}: {ex.Message}"); }
            }
            if (!dryRun) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            Log("Repair complete.");
        }
    }

    // ================================ Core pipeline ================================
    void EnsureDirs()
    {
        if (dryRun) return;
        Directory.CreateDirectory(SlotsDir);
        Directory.CreateDirectory(OverlaysDir);
        Directory.CreateDirectory(RecipesDir);
        Directory.CreateDirectory(ItemsDir);
        Directory.CreateDirectory(AtavismDir);
        Directory.CreateDirectory("Assets/UMA_Auto_Central/Settings");
    }

    List<BuildItem> FindItemsByFolder(DefaultAsset root, bool recursive)
    {
        var list = new List<BuildItem>();
        if (!root) return list;

        string rootPath = AssetDatabase.GetAssetPath(root);
        string fullRoot = Path.GetFullPath(rootPath);
        var folders = Directory.GetDirectories(fullRoot, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        foreach (var f in folders)
        {
            var fbx = Directory.GetFiles(f, "*.fbx", SearchOption.AllDirectories).FirstOrDefault();
            if (fbx == null) continue;
            var itemName = new DirectoryInfo(f).Name;
            list.Add(new BuildItem
            {
                itemName = itemName,
                wardrobeSlot = theme.defaultWardrobeSlot,
                races = theme.races,
                umaMaterial = theme.defaultUMAMaterial
            });
        }
        return list;
    }

    BuildItem BuildItemFromAnyPath(string anyPath)
    {
        if (string.IsNullOrEmpty(anyPath)) return null;
        if (AssetDatabase.IsValidFolder(anyPath))
        {
            return new BuildItem
            {
                itemName = new DirectoryInfo(anyPath).Name,
                wardrobeSlot = theme.defaultWardrobeSlot,
                races = theme.races,
                umaMaterial = theme.defaultUMAMaterial
            };
        }
        else
        {
            return new BuildItem
            {
                itemName = Path.GetFileNameWithoutExtension(anyPath),
                wardrobeSlot = theme.defaultWardrobeSlot,
                races = theme.races,
                umaMaterial = theme.defaultUMAMaterial
            };
        }
    }

    void GroupAndBuild(BuildItem it, bool repairMode = false)
    {
        if (string.IsNullOrEmpty(it.itemName)) throw new Exception("Empty itemName.");
        var wslot = string.IsNullOrEmpty(it.wardrobeSlot) ? theme.defaultWardrobeSlot : it.wardrobeSlot;
        var races = (it.races != null && it.races.Length > 0) ? it.races : theme.races;
        var mat = it.umaMaterial ? it.umaMaterial : theme.defaultUMAMaterial;

        // 1) discover FBX + textures; group under ItemsDir
        var itemRoot = $"{ItemsDir}/{it.itemName}";
        var meshDir = $"{itemRoot}/Meshes";
        var texDir = $"{itemRoot}/Textures";
        if (!dryRun) { Directory.CreateDirectory(meshDir); Directory.CreateDirectory(texDir); }

        if (string.IsNullOrEmpty(it.fbxPath))
        {
            it.fbxPath = FindByName($"*.fbx", it.itemName);
            if (string.IsNullOrEmpty(it.fbxPath)) throw new Exception("FBX not found for " + it.itemName);
        }
        it.fbxPath = EnsureInFolder(it.fbxPath, $"{meshDir}/{Path.GetFileName(it.fbxPath)}");

        if (string.IsNullOrEmpty(it.diffusePath)) it.diffusePath = FindClosestByTags(it.itemName, diffuseTag);
        if (!string.IsNullOrEmpty(it.diffusePath))
            it.diffusePath = EnsureInFolder(it.diffusePath, $"{texDir}/{Path.GetFileName(it.diffusePath)}");

        if (string.IsNullOrEmpty(it.normalPath)) it.normalPath = FindClosestByTags(it.itemName, normalTag);
        if (!string.IsNullOrEmpty(it.normalPath))
            it.normalPath = EnsureInFolder(it.normalPath, $"{texDir}/{Path.GetFileName(it.normalPath)}");

        if (string.IsNullOrEmpty(it.specPath)) it.specPath = FindClosestByTags(it.itemName, specTag);
        if (!string.IsNullOrEmpty(it.specPath))
            it.specPath = EnsureInFolder(it.specPath, $"{texDir}/{Path.GetFileName(it.specPath)}");

        // 2) UMA assets
        it.slotAssetPath = $"{SlotsDir}/{it.itemName}_Slot.asset";
        it.overlayAssetPath = $"{OverlaysDir}/{it.itemName}_Overlay.asset";
        it.recipeAssetPath = $"{RecipesDir}/{it.itemName}_WardrobeRecipe.asset";
        it.prefabAssetPath = $"{AtavismDir}/UMAItem_{it.itemName}.prefab";

        var slot = AssetDatabase.LoadAssetAtPath<SlotDataAsset>(it.slotAssetPath);
        var overlay = AssetDatabase.LoadAssetAtPath<OverlayDataAsset>(it.overlayAssetPath);
        var recipe = AssetDatabase.LoadAssetAtPath<UMAWardrobeRecipe>(it.recipeAssetPath);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(it.prefabAssetPath);

        if (!(slot && skipExisting && !forceOverwrite) || repairMode)
        {
            slot = BuildSlot(it.itemName, it.fbxPath, it.slotAssetPath);
        }
        if (!(overlay && skipExisting && !forceOverwrite) || repairMode)
        {
            overlay = BuildOverlay(it.itemName, it.overlayAssetPath, mat, it.diffusePath, it.normalPath, it.specPath);
        }
        if (!(recipe && skipExisting && !forceOverwrite) || repairMode)
        {
            recipe = BuildRecipe_AnyUMA(it.itemName, it.recipeAssetPath, slot, overlay, wslot, races);
        }
        if (!(prefab && skipExisting && !forceOverwrite) || repairMode)
        {
            CreateAtavismItemPrefab(it.prefabAssetPath, it.itemName, recipe, wslot);
        }
    }

    void CatalogUpsert(BuildItem it)
    {
        var x = catalog.items.FirstOrDefault(a => a.itemName == it.itemName);
        if (x == null) catalog.items.Add(it);
        else
        {
            x.wardrobeSlot = it.wardrobeSlot;
            x.races = it.races;
            x.umaMaterial = it.umaMaterial;
            x.fbxPath = it.fbxPath; x.diffusePath = it.diffusePath; x.normalPath = it.normalPath; x.specPath = it.specPath;
            x.slotAssetPath = it.slotAssetPath; x.overlayAssetPath = it.overlayAssetPath; x.recipeAssetPath = it.recipeAssetPath; x.prefabAssetPath = it.prefabAssetPath;
            x.quality = it.quality; x.itemType = it.itemType; x.subType = it.subType; x.level = it.level; x.description = it.description;
        }
        EditorUtility.SetDirty(catalog);
    }

    // ============================== UMA build helpers ==============================
    // Uses reflection to invoke UMA's SlotDataAssetBuilder if available; else creates a minimal placeholder Slot.
    SlotDataAsset BuildSlot(string itemName, string fbxAssetPath, string slotAssetPath)
    {
        var fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxAssetPath);
        if (!fbxObj) throw new Exception("FBX load failed: " + fbxAssetPath);

        var smr = fbxObj.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault();
        if (!smr) throw new Exception("No SkinnedMeshRenderer in FBX.");

        var builderType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
            .FirstOrDefault(t => t.Name == "SlotDataAssetBuilder");

        if (builderType != null)
        {
            var mi = builderType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                                .FirstOrDefault(m =>
                                {
                                    var ps = m.GetParameters();
                                    return m.Name.Contains("CreateSlotDataAsset") && ps.Length >= 3 &&
                                           ps[0].ParameterType == typeof(SkinnedMeshRenderer) &&
                                           ps[1].ParameterType == typeof(string) &&
                                           ps[2].ParameterType == typeof(string);
                                });

            if (mi != null && !dryRun)
            {
                var outDir = Path.GetDirectoryName(slotAssetPath).Replace("\\", "/");
                var assetName = Path.GetFileNameWithoutExtension(slotAssetPath);
                Directory.CreateDirectory(outDir);
                var result = mi.Invoke(null, new object[] { smr, outDir, assetName }) as SlotDataAsset;
                if (result != null) return result;
            }
        }

        // Fallback: minimal Slot (name only). Good enough for overlay-recipe linkage; you can bake later.
        var slot = ScriptableObject.CreateInstance<SlotDataAsset>();
        slot.slotName = itemName + "_Slot";
        if (!dryRun) SaveNewOrReplace(slotAssetPath, slot);
        return AssetDatabase.LoadAssetAtPath<SlotDataAsset>(slotAssetPath);
    }

    OverlayDataAsset BuildOverlay(string itemName, string overlayAssetPath, UMAMaterial mat,
                                  string diffPath, string normPath, string specPath)
    {
        var overlay = ScriptableObject.CreateInstance<OverlayDataAsset>();
        overlay.overlayName = itemName + "_Overlay";
        overlay.material = mat;

        var list = new List<Texture>();
        list.Add(LoadTex(diffPath));
        if (!string.IsNullOrEmpty(normPath)) list.Add(LoadTex(normPath));
        if (!string.IsNullOrEmpty(specPath)) list.Add(LoadTex(specPath));
        overlay.textureList = list.ToArray();

        if (!dryRun) SaveNewOrReplace(overlayAssetPath, overlay);
        return AssetDatabase.LoadAssetAtPath<OverlayDataAsset>(overlayAssetPath);
    }

    // ********** Version-agnostic recipe builder (reflection) **********
    UMAWardrobeRecipe BuildRecipe_AnyUMA(string itemName, string recipeAssetPath, SlotDataAsset slot,
                                         OverlayDataAsset overlay, string wslot, string[] races)
    {
        var recipe = ScriptableObject.CreateInstance<UMAWardrobeRecipe>();
        recipe.recipeType = "Wardrobe";
        recipe.wardrobeSlot = wslot;
        recipe.DisplayValue = itemName;
        try { recipe.compatibleRaces = races?.ToList() ?? new List<string>(); } catch { }

        // Try both modern & older UMA type layouts
        // 1) Resolve packed types
        var packedSlotType = FindAnyType(
            "UMA.UMAPackedRecipeBase+UMAPackedSlot",
            "UMA.UMATextRecipe+UMAPackedSlot",
            "UMATextRecipe+UMAPackedSlot",
            "UMA.UMAPackedSlot"
        );

        var packedOverlayType = FindAnyType(
            "UMA.UMAPackedRecipeBase+UMAPackedOverlay",
            "UMA.UMATextRecipe+UMAPackedOverlay",
            "UMATextRecipe+UMAPackedOverlay",
            "UMA.UMAPackedOverlay"
        );

        // Container recipe (DCS)
        var dcsRecipeType = FindAnyType(
            "UMA.UMAPackedRecipeBase+UMAPackedDCSRecipe",
            "UMA.UMATextRecipe+DCSPackRecipe",
            "UMATextRecipe+DCSPackRecipe"
        );

        // If we can't find packed types, just save an empty recipe with metadata so you can fill in later.
        if (packedSlotType == null || packedOverlayType == null)
        {
            Debug.LogWarning("[UMA Item Factory] Packed types not found; creating metadata-only recipe.");
            if (!dryRun) { SaveNewOrReplace(recipeAssetPath, recipe); TryAddToIndexer(recipe); }
            return AssetDatabase.LoadAssetAtPath<UMAWardrobeRecipe>(recipeAssetPath);
        }

        // Build PackedOverlay
        var packedOverlay = Activator.CreateInstance(packedOverlayType);
        SetMember(packedOverlay, new[] { "overlay" }, overlay);
        // colorData (OverlayColorData)
        try { SetMember(packedOverlay, new[] { "colorData" }, new OverlayColorData()); } catch { }

        // Build overlay list
        var overlayList = CreateListOf(packedOverlayType);
        overlayList.Add(packedOverlay);

        // Build PackedSlot
        var packedSlot = Activator.CreateInstance(packedSlotType);
        SetMember(packedSlot, new[] { "slotID" }, slot);
        SetMember(packedSlot, new[] { "overlays" }, overlayList);

        // Build slot list
        var slotList = CreateListOf(packedSlotType);
        slotList.Add(packedSlot);

        // (A) Try to assign into a container recipe (baseRecipe/DCSPack)
        bool assigned = false;
        if (dcsRecipeType != null)
        {
            var dcs = Activator.CreateInstance(dcsRecipeType);
            // Some UMA use 'slots', others 'slotsV2'
            if (SetMember(dcs, new[] { "slots", "slotsV2" }, slotList))
            {
                if (SetMember(recipe, new[] { "baseRecipe", "packedRecipe", "dcsRecipe" }, dcs))
                {
                    assigned = true;
                }
            }
        }

        // (B) Fallback: some UMA versions expose slotDataList directly on the recipe
        if (!assigned)
        {
            if (!SetMember(recipe, new[] { "slotDataList", "slots", "slotsV2" }, slotList))
            {
                Debug.LogWarning("[UMA Item Factory] Could not bind packed slots to recipe (unknown UMA layout). Saving metadata-only recipe.");
            }
        }

        if (!dryRun)
        {
            SaveNewOrReplace(recipeAssetPath, recipe);
            TryAddToIndexer(recipe);
        }
        return AssetDatabase.LoadAssetAtPath<UMAWardrobeRecipe>(recipeAssetPath);
    }

    // Add to UMA indexer with whichever EvilAddAsset signature exists.
    void TryAddToIndexer(UMAWardrobeRecipe recipe)
    {
        try
        {
            var idx = UMAAssetIndexer.Instance;
            var mi = idx.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                        .FirstOrDefault(m =>
                        {
                            if (m.Name != "EvilAddAsset") return false;
                            var ps = m.GetParameters();
                            return (ps.Length == 2 && ps[0].ParameterType == typeof(Type)) ||
                                   (ps.Length == 1);
                        });
            if (mi != null)
            {
                var ps = mi.GetParameters();
                if (ps.Length == 2) mi.Invoke(idx, new object[] { typeof(UMAWardrobeRecipe), recipe });
                else mi.Invoke(idx, new object[] { recipe });
            }
        }
        catch { }
    }

    // Reflection helpers
    Type FindAnyType(params string[] fullNames)
    {
        var asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var name in fullNames)
        {
            foreach (var a in asms)
            {
                Type t = null;
                try { t = a.GetType(name, false, false); } catch { }
                if (t != null) return t;
            }
        }
        // Fallback: search by short name
        foreach (var name in fullNames.Select(n => n.Split('+').Last().Split('.').Last()))
        {
            foreach (var a in asms)
            {
                Type t = null;
                try { t = a.GetTypes().FirstOrDefault(tt => tt.Name == name); } catch { continue; }
                if (t != null) return t;
            }
        }
        return null;
    }

    IList CreateListOf(Type elementType)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        return (IList)Activator.CreateInstance(listType);
    }

    bool SetMember(object target, string[] candidateNames, object value)
    {
        if (target == null) return false;
        var t = target.GetType();
        foreach (var name in candidateNames)
        {
            // property first
            var pi = t.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (pi != null && pi.CanWrite)
            {
                if (pi.PropertyType.IsAssignableFrom(value?.GetType() ?? typeof(object)) || IsListAssignable(pi.PropertyType, value))
                {
                    pi.SetValue(target, value, null);
                    return true;
                }
            }
            // field
            var fi = t.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (fi != null)
            {
                if (fi.FieldType.IsAssignableFrom(value?.GetType() ?? typeof(object)) || IsListAssignable(fi.FieldType, value))
                {
                    fi.SetValue(target, value);
                    return true;
                }
            }
        }
        return false;
    }

    bool IsListAssignable(Type targetType, object value)
    {
        if (value == null) return false;
        if (!typeof(IList).IsAssignableFrom(targetType)) return false;
        return typeof(IList).IsAssignableFrom(value.GetType());
    }

    void CreateAtavismItemPrefab(string prefabPath, string itemName, UMAWardrobeRecipe recipe, string wardrobeSlot)
    {
        if (dryRun) { Log($"[DRY] Create Prefab: {prefabPath}"); return; }

        var go = new GameObject($"UMAItem_{itemName}");

        // Prefer your real AtavismUMAItem if present; otherwise fallback bridge
        var atavismType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
            .FirstOrDefault(t => t.Name == "AtavismUMAItem" && typeof(MonoBehaviour).IsAssignableFrom(t));

        MonoBehaviour comp = atavismType != null
            ? (MonoBehaviour)go.AddComponent(atavismType)
            : go.AddComponent<AtavismUMAItemBridge>();

        var t = comp.GetType();
        var f1 = t.GetField("wardrobeRecipe", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        var f2 = t.GetField("wardrobeSlot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (f1 != null) f1.SetValue(comp, recipe);
        if (f2 != null) f2.SetValue(comp, wardrobeSlot);

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go);
    }

    // =============================== Search & IO helpers ===============================
    string EnsureInFolder(string srcAssetPath, string dstAssetPath)
    {
        if (!srcAssetPath.StartsWith("Assets/")) srcAssetPath = ToAssetPath(srcAssetPath);
        if (!dstAssetPath.StartsWith("Assets/")) dstAssetPath = ToAssetPath(dstAssetPath);
        if (srcAssetPath == dstAssetPath) return dstAssetPath;

        if (dryRun) { Log($"[DRY] Copy {srcAssetPath} -> {dstAssetPath}"); return dstAssetPath; }
        Directory.CreateDirectory(Path.GetDirectoryName(dstAssetPath));
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dstAssetPath) != null) return dstAssetPath; // already there
        var ok = AssetDatabase.CopyAsset(srcAssetPath, dstAssetPath);
        if (!ok) throw new Exception($"Copy failed: {srcAssetPath} -> {dstAssetPath}");
        return dstAssetPath;
    }

    string FindByName(string glob, string containsNameLower)
    {
        containsNameLower = containsNameLower.ToLower();
        var guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(containsNameLower));
        if (guids.Length == 0) guids = AssetDatabase.FindAssets("");
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (!p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
            if (Path.GetFileNameWithoutExtension(p).ToLower().Contains(containsNameLower))
                return p;
        }
        return null;
    }

    string FindClosestByTags(string baseName, string tag)
    {
        baseName = baseName.ToLower();
        tag = tag.ToLower();
        var guids = AssetDatabase.FindAssets("");
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var ext = Path.GetExtension(p).ToLower();
            if (ext != ".png" && ext != ".tga" && ext != ".jpg") continue;
            var fn = Path.GetFileNameWithoutExtension(p).ToLower();
            if (fn.Contains(baseName) && fn.Contains(tag)) return p;
        }
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var ext = Path.GetExtension(p).ToLower();
            if (ext != ".png" && ext != ".tga" && ext != ".jpg") continue;
            var fn = Path.GetFileNameWithoutExtension(p).ToLower();
            if (fn.Contains(tag)) return p;
        }
        return null;
    }

    void SaveNewOrReplace(string path, UnityEngine.Object obj)
    {
        var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (existing != null)
        {
            if (!forceOverwrite && skipExisting) return;
            AssetDatabase.DeleteAsset(path);
        }
        AssetDatabase.CreateAsset(obj, path);
        EditorUtility.SetDirty(obj);
    }

    Texture2D LoadTex(string assetPath)
        => string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

    static string ToAssetPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return null;
        string proj = Path.GetFullPath(Application.dataPath + "/..");
        return "Assets" + fullPath.Replace(proj, "").Replace("\\", "/");
    }

    void Log(string msg) => Debug.Log("[UMA Item Factory] " + msg);

    // ============================== CSV parse / generate ==============================
    List<Dictionary<string, string>> ParseCsv(string text, char delim)
    {
        var rows = new List<Dictionary<string, string>>();
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return rows;

        var headers = SplitCsvLine(lines[0], delim).Select(h => h.Trim()).ToArray();
        for (int i = 1; i < lines.Length; i++)
        {
            var cells = SplitCsvLine(lines[i], delim);
            var row = new Dictionary<string, string>();
            for (int c = 0; c < headers.Length && c < cells.Count; c++) row[headers[c]] = cells[c];
            rows.Add(row);
        }
        return rows;
    }

    List<Dictionary<string, string>> RuleBasedCSV(ThemeSettings t, int count)
    {
        var rows = new List<Dictionary<string, string>>();
        var rnd = new System.Random();
        for (int i = 0; i < count; i++)
        {
            var name = $"{Pick(t.namePrefixes, rnd)} {Pick(t.nameCores, rnd)} {Pick(t.nameSuffixes, rnd)}".Replace("  ", " ").Trim();
            var slot = Pick(t.allowedWardrobeSlots, rnd);
            var quality = Pick(t.qualities, rnd);
            var subtype = Pick(t.subTypes, rnd);
            var lvl = rnd.Next(t.minLevel, t.maxLevel + 1);
            rows.Add(new Dictionary<string, string> {
                {"itemName", name},
                {"wardrobeSlot", slot},
                {"races", string.Join(";", t.races)},
                {"quality", quality},
                {"itemType", Pick(t.itemTypes, rnd)},
                {"subType", subtype},
                {"level", lvl.ToString()},
                {"description", $"{quality} {subtype} {slot.ToLower()} forged in the {t.themeName} style."},
                {"fbxPath",""},
                {"diffusePath",""},
                {"normalPath",""},
                {"specPath",""},
                {"umaMaterialPath",""}
            });
        }
        return rows;
    }

    string Pick(string[] arr, System.Random r) => arr[Mathf.Clamp(r.Next(arr.Length), 0, arr.Length - 1)];

    List<string> SplitCsvLine(string line, char delim)
    {
        var res = new List<string>(); bool q = false; var cur = "";
        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"') { q = !q; continue; }
            if (!q && ch == delim) { res.Add(cur); cur = ""; }
            else cur += ch;
        }
        res.Add(cur); return res;
    }

    BuildItem ItemFromCsvRow(Dictionary<string, string> row, bool fallbackWardrobeSlotFromTheme = false)
    {
        string Get(string k, string d = "") => row.ContainsKey(k) ? row[k] : d;

        var races = Get("races", string.Join(";", theme.races))
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();

        UMAMaterial mat = theme.defaultUMAMaterial;
        var matPath = Get("umaMaterialPath", "");
        if (!string.IsNullOrWhiteSpace(matPath))
        {
            var m = AssetDatabase.LoadAssetAtPath<UMAMaterial>(matPath);
            if (m) mat = m;
        }

        var slot = Get("wardrobeSlot", fallbackWardrobeSlotFromTheme ? theme.defaultWardrobeSlot : "");

        int lvl = 1; int.TryParse(Get("level", "1"), out lvl);

        return new BuildItem
        {
            itemName = Get("itemName", "").Trim(),
            wardrobeSlot = slot,
            races = races,
            umaMaterial = mat,
            fbxPath = Get("fbxPath", "").Trim(),
            diffusePath = Get("diffusePath", "").Trim(),
            normalPath = Get("normalPath", "").Trim(),
            specPath = Get("specPath", "").Trim(),
            quality = Get("quality", "Common"),
            itemType = Get("itemType", "Armor"),
            subType = Get("subType", "Plate"),
            level = lvl,
            description = Get("description", "")
        };
    }

    string BuildAtavismCSV(List<BuildItem> items, ExportProfile xp)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", xp.csvHeader));
        int id = 10000; // starting ID; adjust as needed
        foreach (var it in items)
        {
            var cells = new List<string> {
                id.ToString(),
                Csv(it.itemName),
                Csv(it.itemName),
                Csv(it.wardrobeSlot),
                Csv(string.IsNullOrEmpty(it.quality) ? "Common" : it.quality),
                Csv(string.IsNullOrEmpty(it.itemType) ? "Armor" : it.itemType),
                Csv(string.IsNullOrEmpty(it.subType) ? "Plate" : it.subType),
                it.level.ToString(),
                Csv(""), // iconPath
                Csv(it.prefabAssetPath),
                xp.defaultStackLimit.ToString(),
                Csv(xp.defaultBindType),
                xp.defaultSellPrice.ToString(),
                Csv($"{{\"armor\": {ArmorByQuality(it.quality)}, \"stam\": {StatByLevel(it.level)} }}"),
                Csv(it.recipeAssetPath)
            };
            sb.AppendLine(string.Join(",", cells));
            id++;
        }
        return sb.ToString();
    }

    string BuildAtavismSQL(List<BuildItem> items)
    {
        var sb = new StringBuilder();
        int id = 10000;
        foreach (var it in items)
        {
            sb.AppendLine(
$"INSERT INTO item_templates (id,name,displayname,slot,quality,itemtype,subtype,requiredlevel,prefabpath,stacklimit,bindtype,sellprice,statsjson,wardroberecipe) VALUES " +
$"({id},'{Sql(it.itemName)}','{Sql(it.itemName)}','{Sql(it.wardrobeSlot)}','{Sql(it.quality)}','{Sql(it.itemType)}','{Sql(it.subType)}',{it.level},'{Sql(it.prefabAssetPath)}',1,'None',0,'{{\"armor\":{ArmorByQuality(it.quality)},\"stam\":{StatByLevel(it.level)}}}','{Sql(it.recipeAssetPath)}');");
            id++;
        }
        return sb.ToString();
    }

    int ArmorByQuality(string q)
    {
        switch ((q ?? "").ToLower())
        {
            case "uncommon": return 6;
            case "rare": return 10;
            case "epic": return 16;
            case "legendary": return 24;
            default: return 4;
        }
    }
    int StatByLevel(int lvl) => Mathf.RoundToInt(Mathf.Lerp(1, 50, Mathf.InverseLerp(1, 50, lvl)));

    string Csv(string s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";
    string Sql(string s) => (s ?? "").Replace("'", "''");

    // ================================== LLM call ==================================
    string CallLLMSynchronously(LLMSettings l, ThemeSettings t, int count)
    {
        try
        {
            var sys = l.systemPrompt;
            var user = l.userPromptTemplate
                .Replace("{THEME}", t.themeName)
                .Replace("{LORE}", t.loreSeed)
                .Replace("{SLOTS}", string.Join(", ", t.allowedWardrobeSlots))
                .Replace("{RACES}", string.Join(";", t.races))
                .Replace("{COUNT}", count.ToString());

            var payload = "{\"model\":\"" + l.model + "\",\"messages\":["
                        + "{\"role\":\"system\",\"content\":\"" + EscapeJson(sys) + "\"},"
                        + "{\"role\":\"user\",\"content\":\"" + EscapeJson(user) + "\"}"
                        + "],\"temperature\":0.8}";

            using (var req = new UnityWebRequest(l.openAICompatibleEndpoint, "POST"))
            {
                var body = new System.Text.UTF8Encoding().GetBytes(payload);
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(l.apiKey)) req.SetRequestHeader("Authorization", "Bearer " + l.apiKey);

                var op = req.SendWebRequest();
                while (!op.isDone) { }

#if UNITY_2020_3_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success) { Debug.LogWarning(req.error); return null; }
#else
                if (req.isHttpError || req.isNetworkError) { Debug.LogWarning(req.error); return null; }
#endif
                var json = req.downloadHandler.text;
                // naive extraction of first message content
                var marker = "\"content\":\"";
                var idx = json.IndexOf(marker, StringComparison.Ordinal);
                if (idx < 0) return null;
                idx += marker.Length;
                var end = json.IndexOf("\"", idx, StringComparison.Ordinal);
                if (end < 0) end = json.Length;
                var content = json.Substring(idx, end - idx);
                content = content.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"");
                return content.Trim();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("LLM call failed: " + ex.Message);
            return null;
        }
    }

    string EscapeJson(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");

    // ================================== Wizard ==================================
    public class UMAItemWizard : EditorWindow
    {
        UMAItemFactoryUltra owner;
        BuildItem it; ThemeSettings themeRef;
        string racesStr;

        public static void Open(UMAItemFactoryUltra owner, BuildItem it, ThemeSettings t)
        {
            var w = CreateInstance<UMAItemWizard>();
            w.owner = owner; w.it = it; w.themeRef = t;
            w.titleContent = new GUIContent("UMA Item");
            w.racesStr = string.Join(";", (it.races != null && it.races.Length > 0) ? it.races : t.races);
            w.position = new Rect(Screen.width / 2, Screen.height / 2, 420, 260);
            w.ShowUtility();
        }
        void OnGUI()
        {
            it.itemName = EditorGUILayout.TextField("Item Name", it.itemName);
            it.wardrobeSlot = EditorGUILayout.TextField("Wardrobe Slot", string.IsNullOrEmpty(it.wardrobeSlot) ? themeRef.defaultWardrobeSlot : it.wardrobeSlot);
            racesStr = EditorGUILayout.TextField("Races (;)", racesStr);
            it.umaMaterial = (UMAMaterial)EditorGUILayout.ObjectField("UMA Material", it.umaMaterial ? it.umaMaterial : themeRef.defaultUMAMaterial, typeof(UMAMaterial), false);
            it.quality = EditorGUILayout.TextField("Quality", string.IsNullOrEmpty(it.quality) ? "Common" : it.quality);
            it.itemType = EditorGUILayout.TextField("Item Type", string.IsNullOrEmpty(it.itemType) ? "Armor" : it.itemType);
            it.subType = EditorGUILayout.TextField("SubType", string.IsNullOrEmpty(it.subType) ? "Plate" : it.subType);
            it.level = EditorGUILayout.IntField("Level", it.level <= 0 ? 1 : it.level);
            it.description = EditorGUILayout.TextField("Description", it.description);

            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel")) Close();
                if (GUILayout.Button("Create"))
                {
                    it.races = racesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                    owner.EnsureDirs();
                    try { owner.GroupAndBuild(it); owner.CatalogUpsert(it); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Close(); }
                    catch (Exception ex) { Debug.LogError(ex.Message); }
                }
            }
        }
    }
}

// ===================== Fallback bridge if AtavismUMAItem not present =====================
public class AtavismUMAItemBridge : MonoBehaviour
{
    public UMAWardrobeRecipe wardrobeRecipe;
    public string wardrobeSlot = "Chest";
}
