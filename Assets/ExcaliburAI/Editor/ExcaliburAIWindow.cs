using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using ExcaliburAI.Data;

namespace ExcaliburAI.Editor
{
    public class ExcaliburAIWindow : EditorWindow
    {
        private ITextImageProvider _provider;
        private string _theme = "high fantasy";
        private int _weaponCount = 10;
        private int _classCount = 5;
        private Vector2 _scroll;
        private string _lastJson = "";
        private SecretsConfig _secrets;


        [MenuItem("ExcaliburAI/Control Panel")]
        public static void Open() => GetWindow<ExcaliburAIWindow>("ExcaliburAI");

        private void OnEnable()
        {
            // swap this for a real provider when ready
            _provider = new DummyProvider();
            _secrets = SecretsLoader.Load();
        }

        private void OnGUI()
        {
            using var scroll = new EditorGUILayout.ScrollViewScope(_scroll);
            _scroll = scroll.scrollPosition;

            EditorGUILayout.LabelField("Provider", EditorStyles.boldLabel);
            var prov = _provider?.ProviderName ?? "<none>";
            var provCfg = _secrets?.provider ?? "dummy";
            EditorGUILayout.LabelField("Active:", prov);
            EditorGUILayout.LabelField("Configured:", provCfg);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Content Prompts", EditorStyles.boldLabel);
            _theme = EditorGUILayout.TextField(new GUIContent("Theme/Setting"), _theme);
            _classCount = EditorGUILayout.IntSlider(new GUIContent("Classes to Create"), _classCount, 1, 20);
            _weaponCount = EditorGUILayout.IntSlider(new GUIContent("Weapons to Create"), _weaponCount, 1, 200);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Classes (Preview JSON)", GUILayout.Height(28)))
                    _ = GenerateClassesAsync();

                if (GUILayout.Button("Generate Weapons (Preview JSON)", GUILayout.Height(28)))
                    _ = GenerateWeaponsAsync();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Assets From Last JSON", GUILayout.Height(30)))
                CreateAssetsFromJson(_lastJson);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Last JSON", EditorStyles.boldLabel);
            _lastJson = EditorGUILayout.TextArea(_lastJson, GUILayout.MinHeight(160));
        }

        async Task GenerateClassesAsync()
        {
            var system = "You are a game designer that outputs STRICT JSON matching the provided schema.";
            var user = $@"
Create exactly {_classCount} fantasy RPG classes in the theme '{_theme}'.
JSON schema:
{{
  ""classes"": [
    {{ ""id"": ""string"", ""name"": ""string"", ""role"": ""string"", ""primaryStats"": ""string"", ""pitch"": ""string"", ""iconPrompt"": ""string"" }}
  ]
}}";
            _lastJson = await _provider.GenerateTextJson(system, user);
            Repaint();
        }

        async Task GenerateWeaponsAsync()
        {
            var system = "You are a game designer that outputs STRICT JSON matching the provided schema.";
            var user = $@"
Create exactly {_weaponCount} melee weapons for a fantasy RPG in theme '{_theme}'.
Each item must include a short description, tier (Common/Rare/Epic/Legendary), type, minDamage, maxDamage, attackSpeed, and a concise iconPrompt.
JSON schema:
{{
  ""weapons"": [
    {{ ""id"": ""string"", ""name"": ""string"", ""tier"": ""string"", ""type"": ""string"", ""minDamage"": 0, ""maxDamage"": 0, ""attackSpeed"": 1.0, ""description"": ""string"", ""iconPrompt"": ""string"" }}
  ]
}}";
            _lastJson = await _provider.GenerateTextJson(system, user);
            Repaint();
        }

        [Serializable] class WeaponList { public WeaponJson[] weapons; }
        [Serializable] class WeaponJson
        {
            public string id, name, tier, type, description, iconPrompt;
            public int minDamage, maxDamage; public float attackSpeed;
        }

        [Serializable] class ClassList { public ClassJson[] classes; }
        [Serializable] class ClassJson
        {
            public string id, name, role, primaryStats, pitch, iconPrompt;
        }

        void CreateAssetsFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("No JSON to import.");
                return;
            }

            // try weapons
            try
            {
                var wl = JsonUtility.FromJson<WeaponList>(json);
                if (wl?.weapons != null && wl.weapons.Length > 0)
                    foreach (var w in wl.weapons) CreateWeaponAsset(w);
            }
            catch { /* ignore */ }

            // try classes
            try
            {
                var cl = JsonUtility.FromJson<ClassList>(json);
                if (cl?.classes != null && cl.classes.Length > 0)
                    foreach (var c in cl.classes) CreateClassAsset(c);
            }
            catch { /* ignore */ }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ExcaliburAI: Asset creation complete.");
        }

        void CreateWeaponAsset(WeaponJson w)
        {
            var dir = "Assets/ExcaliburAI/Generated/Items";
            Directory.CreateDirectory(dir);

            var asset = ScriptableObject.CreateInstance<ItemDefinition>();
            asset.Id = string.IsNullOrWhiteSpace(w.id) ? Guid.NewGuid().ToString("N") : w.id;
            asset.DisplayName = w.name;
            asset.Description = w.description;
            asset.Tier = w.tier;
            asset.WeaponType = ParseEnumSafe<WeaponType>(w.type);
            asset.MinDamage = w.minDamage;
            asset.MaxDamage = w.maxDamage;
            asset.AttackSpeed = Mathf.Max(0.01f, w.attackSpeed);
            asset.IconPrompt = w.iconPrompt;

            // icon: placeholder for now
            asset.Icon = CreatePlaceholderIcon(w.name);

            var path = $"{dir}/{Sanitize(w.name)}.asset";
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
        }

        void CreateClassAsset(ClassJson c)
        {
            var dir = "Assets/ExcaliburAI/Generated/Classes";
            Directory.CreateDirectory(dir);

            var asset = ScriptableObject.CreateInstance<ClassDefinition>();
            asset.Id = string.IsNullOrWhiteSpace(c.id) ? Guid.NewGuid().ToString("N") : c.id;
            asset.ClassName = c.name;
            asset.FantasyPitch = c.pitch;
            asset.Role = c.role;
            asset.PrimaryStats = c.primaryStats;
            asset.IconPrompt = c.iconPrompt;
            asset.Icon = CreatePlaceholderIcon(c.name);

            var path = $"{dir}/{Sanitize(c.name)}.asset";
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
        }

        static string Sanitize(string s)
        {
            var safe = new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch=='_' || ch=='-' || ch==' ').ToArray());
            return string.IsNullOrWhiteSpace(safe) ? "Unnamed" : safe.Trim().Replace(' ', '_');
        }

        static T ParseEnumSafe<T>(string value) where T : struct
            => Enum.TryParse<T>(value, true, out var r) ? r : default;

        static Sprite CreatePlaceholderIcon(string label)
        {
            var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            var pixels = Enumerable.Repeat(col, 256 * 256).ToArray();
            tex.SetPixels(pixels);
            tex.Apply();

            var path = $"Assets/ExcaliburAI/Generated/Icons/{Sanitize(label)}.png";
            Directory.CreateDirectory("Assets/ExcaliburAI/Generated/Icons");
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
