using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace ExcaliburAI
{
    // Unified studio window for ExcaliburAI.  This single window replaces the
    // previous scattering of settings and database configuration across
    // multiple editor windows.  All configuration values are persisted to
    // Assets/ExcaliburAI/Config/studio_config.json.  The UI is divided
    // into three logical sections: conversation and planner, project profile,
    // and settings (database, LLM, image generation).
    public class ExcaliburAIStudio : EditorWindow
    {
        // Config wrapper classes to persist settings.  They mirror the
        // structure of the previous Env, LLMConfig and other classes but
        // consolidated into a single document for ease of management.
        [Serializable] private class ProjectProfile
        {
            public string projectName = "Unnamed Project";
            public string genre = "High Fantasy";
            public string tone = "Heroic, cohesive, PG-13, lore-driven";
            public string namingStyle = "Concise, evocative";
            public bool restrictToAllowedRaces = true;
            public List<string> allowedRaces = new List<string>();
            public List<string> allowedClasses = new List<string>();
            public string worldSummary = string.Empty;
        }

        [Serializable] private class DatabaseSettings
        {
            public string host = "127.0.0.1";
            public int port = 3306;
            public string user = "";
            public string password = "";
            public string adminDb = "admin";
            public string atavismDb = "atavism";
            public string masterDb = "master";
            public string worldContentDb = "world_content";
        }

        [Serializable] private class LlmSettings
        {
            public string provider = "local-openai";
            public string endpoint = "http://localhost:11434/v1/chat/completions";
            public string model = "llama3.1:8b";
            public string apiKey = "";
            public bool stream = true;
        }

        [Serializable] private class ImageSettings
        {
            public string provider = "automatic1111";
            public string endpoint = "http://localhost:7860";
            public string loraFolder = string.Empty;
            public bool useReferenceImg = false;
            public float img2imgDenoise = 0.35f;
            public string persistentPrompt = string.Empty;
            public string persistentNegative = string.Empty;
            public List<LoraEntry> loras = new List<LoraEntry>();
        }

        [Serializable] private class LoraEntry
        {
            public string name;
            public bool enabled;
            public float weight;
        }

        [Serializable] private class StudioConfig
        {
            public ProjectProfile project = new ProjectProfile();
            public DatabaseSettings database = new DatabaseSettings();
            public LlmSettings llm = new LlmSettings();
            public ImageSettings image = new ImageSettings();
        }

        private StudioConfig config;
        private Vector2 conversationScroll;
        private Vector2 settingsScroll;
        private string chatInput = string.Empty;
        private readonly List<string> log = new List<string>();
        private bool showProject = true;
        private bool showDatabase = true;
        private bool showLlm = true;
        private bool showImage = true;

        private const string CONFIG_PATH = "Assets/ExcaliburAI/Config/studio_config.json";

        [MenuItem("ExcaliburAI/Studio (Redesign)")]
        public static void Open()
        {
            GetWindow<ExcaliburAIStudio>("ExcaliburAI Studio");
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            var path = CONFIG_PATH;
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    config = JsonUtility.FromJson<StudioConfig>(json);
                }
                catch
                {
                    config = new StudioConfig();
                }
            }
            else
            {
                config = new StudioConfig();
            }
        }

        private void SaveConfig()
        {
            var dir = Path.GetDirectoryName(CONFIG_PATH);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var json = JsonUtility.ToJson(config, prettyPrint: true);
            File.WriteAllText(CONFIG_PATH, json);
            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawConversationArea();
                DrawSettingsArea();
            }
        }

        private void DrawConversationArea()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.6f)))
            {
                GUILayout.Label("Assistant Chat", EditorStyles.boldLabel);
                conversationScroll = EditorGUILayout.BeginScrollView(conversationScroll);
                foreach (var line in log)
                {
                    EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Enter command:");
                chatInput = EditorGUILayout.TextArea(chatInput, GUILayout.Height(60));
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Plan (dry run)", GUILayout.Height(24)))
                    {
                        Plan(chatInput, dryRun: true);
                    }
                    if (GUILayout.Button("Apply", GUILayout.Height(24)))
                    {
                        Plan(chatInput, dryRun: false);
                    }
                }
            }
        }

        private void DrawSettingsArea()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.4f)))
            {
                settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll);

                DrawProjectProfile();
                DrawDatabaseSettings();
                DrawLlmSettings();
                DrawImageSettings();

                if (GUILayout.Button("Save Settings"))
                {
                    SaveConfig();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawProjectProfile()
        {
            showProject = EditorGUILayout.Foldout(showProject, "Project Profile", true, EditorStyles.foldoutHeader);
            if (showProject)
            {
                var p = config.project;
                p.projectName = EditorGUILayout.TextField("Project Name", p.projectName);
                p.genre = EditorGUILayout.TextField("Genre", p.genre);
                p.tone = EditorGUILayout.TextField("Tone", p.tone);
                p.namingStyle = EditorGUILayout.TextField("Naming Style", p.namingStyle);
                p.restrictToAllowedRaces = EditorGUILayout.Toggle("Restrict To Allowed Races", p.restrictToAllowedRaces);
                string raceCsv = string.Join(", ", p.allowedRaces.ToArray());
                string newRaceCsv = EditorGUILayout.TextField("Allowed Races (comma)", raceCsv);
                if (newRaceCsv != raceCsv)
                {
                    p.allowedRaces = new List<string>(newRaceCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    for (int i = 0; i < p.allowedRaces.Count; i++)
                    {
                        p.allowedRaces[i] = p.allowedRaces[i].Trim();
                    }
                }
                string classCsv = string.Join(", ", p.allowedClasses.ToArray());
                string newClassCsv = EditorGUILayout.TextField("Allowed Classes (comma)", classCsv);
                if (newClassCsv != classCsv)
                {
                    p.allowedClasses = new List<string>(newClassCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    for (int i = 0; i < p.allowedClasses.Count; i++)
                    {
                        p.allowedClasses[i] = p.allowedClasses[i].Trim();
                    }
                }
                GUILayout.Label("World Summary");
                p.worldSummary = EditorGUILayout.TextArea(p.worldSummary, GUILayout.Height(60));
                GUILayout.Space(4);
            }
        }

        private void DrawDatabaseSettings()
        {
            showDatabase = EditorGUILayout.Foldout(showDatabase, "Database Settings", true, EditorStyles.foldoutHeader);
            if (showDatabase)
            {
                var d = config.database;
                d.host = EditorGUILayout.TextField("Host", d.host);
                d.port = EditorGUILayout.IntField("Port", d.port);
                d.user = EditorGUILayout.TextField("User", d.user);
                d.password = EditorGUILayout.PasswordField("Password", d.password);
                d.adminDb = EditorGUILayout.TextField("Admin DB", d.adminDb);
                d.atavismDb = EditorGUILayout.TextField("Atavism DB", d.atavismDb);
                d.masterDb = EditorGUILayout.TextField("Master DB", d.masterDb);
                d.worldContentDb = EditorGUILayout.TextField("World Content DB", d.worldContentDb);
                if (GUILayout.Button("Test Connection"))
                {
                    TestDatabaseConnection();
                }
            }
        }

        private void DrawLlmSettings()
        {
            showLlm = EditorGUILayout.Foldout(showLlm, "LLM Settings", true, EditorStyles.foldoutHeader);
            if (showLlm)
            {
                var l = config.llm;
                l.provider = EditorGUILayout.TextField("Provider", l.provider);
                l.endpoint = EditorGUILayout.TextField("Endpoint", l.endpoint);
                l.model = EditorGUILayout.TextField("Model", l.model);
                l.apiKey = EditorGUILayout.PasswordField("API Key", l.apiKey);
                l.stream = EditorGUILayout.Toggle("Stream", l.stream);
            }
        }

        private void DrawImageSettings()
        {
            showImage = EditorGUILayout.Foldout(showImage, "Image Generation Settings", true, EditorStyles.foldoutHeader);
            if (showImage)
            {
                var im = config.image;
                im.provider = EditorGUILayout.TextField("Provider", im.provider);
                im.endpoint = EditorGUILayout.TextField("Endpoint", im.endpoint);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("LoRA Folder");
                im.loraFolder = EditorGUILayout.TextField(im.loraFolder);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string selected = EditorUtility.OpenFolderPanel("Select LoRA Folder", im.loraFolder, string.Empty);
                    if (!string.IsNullOrEmpty(selected))
                    {
                        im.loraFolder = selected;
                        RefreshLoraList();
                    }
                }
                EditorGUILayout.EndHorizontal();
                im.useReferenceImg = EditorGUILayout.Toggle("Use Reference Image", im.useReferenceImg);
                im.img2imgDenoise = EditorGUILayout.Slider("Img2Img Denoise", im.img2imgDenoise, 0f, 1f);
                GUILayout.Label("Persistent Prompt");
                im.persistentPrompt = EditorGUILayout.TextArea(im.persistentPrompt, GUILayout.Height(40));
                GUILayout.Label("Persistent Negative");
                im.persistentNegative = EditorGUILayout.TextArea(im.persistentNegative, GUILayout.Height(40));
                GUILayout.Space(4);
                GUILayout.Label("LoRAs");
                if (im.loras != null)
                {
                    for (int i = 0; i < im.loras.Count; i++)
                    {
                        var entry = im.loras[i];
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            entry.enabled = EditorGUILayout.Toggle(entry.enabled, GUILayout.Width(16));
                            EditorGUILayout.LabelField(entry.name);
                            entry.weight = EditorGUILayout.Slider(entry.weight, 0f, 2f);
                        }
                    }
                }
                if (GUILayout.Button("Refresh LoRA List"))
                {
                    RefreshLoraList();
                }
            }
        }

        private void TestDatabaseConnection()
        {
            // Dummy test: append a log line. In a full implementation this would
            // attempt to open a connection using MySqlConnector.
            log.Add($"[Database] Would test connection to {config.database.host}:{config.database.port}");
            conversationScroll.y = float.MaxValue;
        }

        private void Plan(string text, bool dryRun)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                log.Add("Please enter a command.");
                conversationScroll.y = float.MaxValue;
                return;
            }
            log.Add($"You: {text}");
            // In a full implementation this would call into the planner, passing
            // config (project profile, DB snapshot, allowed actions) and the
            // input text.  Here we append a stub response.
            log.Add("Planner: (stub) No actions executed. Redesign skeleton in effect.");
            conversationScroll.y = float.MaxValue;
        }

        private void RefreshLoraList()
        {
            var im = config.image;
            var list = new List<LoraEntry>();
            try
            {
                if (!string.IsNullOrEmpty(im.loraFolder) && Directory.Exists(im.loraFolder))
                {
                    var files = Directory.GetFiles(im.loraFolder, "*.safetensors", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        var name = Path.GetFileNameWithoutExtension(file);
                        var existing = im.loras.Find(e => e.name == name);
                        var entry = existing ?? new LoraEntry { name = name, enabled = false, weight = 0.8f };
                        list.Add(entry);
                    }
                }
            }
            catch
            {
                // ignore
            }
            im.loras = list;
        }
    }
}