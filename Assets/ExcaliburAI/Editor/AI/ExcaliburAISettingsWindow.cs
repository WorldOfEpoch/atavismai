#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace ExcaliburAI.AI
{
    public class ExcaliburAISettingsWindow : EditorWindow
    {
        LLMSettingsModel cfg;
        Vector2 scroll;
        string subjectPrompt = "isometric obsidian longsword on fire, centered";
        string subjectNegative = "";
        bool useImg2Img = false;
        float denoise = 0.35f;

        [MenuItem("ExcaliburAI/AI Settings")]
        public static void Open()
        {
            var w = GetWindow<ExcaliburAISettingsWindow>("ExcaliburAI Settings");
            w.minSize = new Vector2(760, 700);
        }

        void OnEnable() { cfg = LLMConfig.Load(); }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Endpoint
            EditorGUILayout.LabelField("Automatic1111 Endpoint", EditorStyles.boldLabel);
            cfg.image.automatic1111.endpoint = EditorGUILayout.TextField("Endpoint", cfg.image.automatic1111.endpoint);

            // Persistent prompts
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Persistent Style Prompts", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Positive");
            cfg.image.automatic1111.persistentPrompt = EditorGUILayout.TextArea(cfg.image.automatic1111.persistentPrompt, GUILayout.MinHeight(50));
            EditorGUILayout.LabelField("Negative");
            cfg.image.automatic1111.persistentNegative = EditorGUILayout.TextArea(cfg.image.automatic1111.persistentNegative, GUILayout.MinHeight(50));

            // Quality
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Quality Controls", EditorStyles.miniBoldLabel);
            cfg.image.automatic1111.samplerName = EditorGUILayout.TextField("Sampler", cfg.image.automatic1111.samplerName);
            cfg.image.automatic1111.steps = EditorGUILayout.IntSlider("Steps", cfg.image.automatic1111.steps, 8, 80);
            cfg.image.automatic1111.cfgScale = EditorGUILayout.Slider("CFG Scale", cfg.image.automatic1111.cfgScale, 1.0f, 14.0f);
            EditorGUILayout.BeginHorizontal();
            cfg.image.automatic1111.width = EditorGUILayout.IntField("Width", cfg.image.automatic1111.width);
            cfg.image.automatic1111.height = EditorGUILayout.IntField("Height", cfg.image.automatic1111.height);
            EditorGUILayout.EndHorizontal();

            // Hi-Res
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Hi-Res Fix", EditorStyles.miniBoldLabel);
            cfg.image.automatic1111.enableHires = EditorGUILayout.Toggle("Enable Hi-Res", cfg.image.automatic1111.enableHires);
            if (cfg.image.automatic1111.enableHires)
            {
                cfg.image.automatic1111.hiresUpscaler = EditorGUILayout.TextField("Upscaler", cfg.image.automatic1111.hiresUpscaler);
                cfg.image.automatic1111.hiresScale = EditorGUILayout.Slider("HR Scale", cfg.image.automatic1111.hiresScale, 1.1f, 2.0f);
                cfg.image.automatic1111.hiresDenoising = EditorGUILayout.Slider("HR Denoising", cfg.image.automatic1111.hiresDenoising, 0.1f, 0.8f);
            }

            // Reference / Img2Img
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Reference (Img2Img)", EditorStyles.boldLabel);
            var rcfg = ReferenceLibrary.Load();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active Reference", GUILayout.Width(110));
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(rcfg.activeReference) ? "(none)" : rcfg.activeReference, GUILayout.Height(16));
            if (GUILayout.Button("Open Library", GUILayout.Width(120)))
                ReferenceLibraryWindow.Open();
            EditorGUILayout.EndHorizontal();
            useImg2Img = EditorGUILayout.ToggleLeft("Use Active Reference for Img2Img", useImg2Img);
            if (useImg2Img)
                denoise = EditorGUILayout.Slider("Denoising Strength (img2img)", denoise, 0.2f, 0.8f);

            if (GUILayout.Button("Save Settings", GUILayout.Height(26))) { LLMConfig.Save(cfg); }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Quick Subject Test", EditorStyles.boldLabel);
            subjectPrompt = EditorGUILayout.TextField("Subject Prompt", subjectPrompt);
            subjectNegative = EditorGUILayout.TextField("Subject Negative", subjectNegative);
            if (GUILayout.Button("Generate", GUILayout.Height(28))) { _ = TestImageAsync(useImg2Img, denoise); }

            EditorGUILayout.EndScrollView();
        }

        async Task TestImageAsync(bool useImg2Img, float denoise)
        {
            try
            {
                var gen = new Automatic1111ImageGenerator(cfg);
                var path = await gen.GenerateAsync(subjectPrompt, subjectNegative, useImg2Img, denoise, -1, -1, -1, -1f, CancellationToken.None);
                EditorUtility.DisplayDialog("ExcaliburAI", "Image saved to:\n" + path, "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("ExcaliburAI", "Image Error:\n" + ex.Message, "OK");
            }
        }
    }
}
#endif
