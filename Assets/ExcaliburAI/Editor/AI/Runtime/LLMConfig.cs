using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    [System.Serializable]
    public class LocalOpenAIConfig
    {
        public string endpoint = "http://localhost:11434/v1/chat/completions";
        public string model = "llama3.1:8b";
        public string apiKey = "";
    }

    [System.Serializable]
    public class LoraEntry
    {
        public string name = "";
        public float weight = 0.8f;
        public bool enabled = false;
    }

    [System.Serializable]
    public class Automatic1111Config
    {
        public string endpoint = "http://127.0.0.1:7861";
        public string loraPath = "";
        public bool preferApiList = true;
        public LoraEntry[] loras = new LoraEntry[0];

        // Persistent (style) prompts
        [TextArea] public string persistentPrompt = "highly detailed, sharp, game ui icon, centered, clean silhouette, professional";
        [TextArea] public string persistentNegative = "blurry, text, watermark, jpeg artifacts, low contrast, background clutter, cropped";

        // Default quality controls
        public string samplerName = "DPM++ 2M Karras";
        public int steps = 28;
        public float cfgScale = 6.5f;
        public int width = 512;
        public int height = 512;

        // Hi-Res fix
        public bool enableHires = true;
        public float hiresScale = 1.5f; // 512 -> 768
        public float hiresDenoising = 0.35f;
        public string hiresUpscaler = "4x-UltraSharp"; // fallback if missing will be ESRGAN
    }

    [System.Serializable]
    public class ImageConfig
    {
        public string provider = "automatic1111";
        public Automatic1111Config automatic1111 = new Automatic1111Config();
    }

    [System.Serializable]
    public class LLMSettingsModel
    {
        public string provider = "local-openai";
        public LocalOpenAIConfig localOpenAI = new LocalOpenAIConfig();
        public bool stream = true;
        public int timeoutSeconds = 60;
        public ImageConfig image = new ImageConfig();
    }

    public static class LLMConfig
    {
        public static string ConfigPath
        {
            get
            {
#if UNITY_EDITOR
                return System.IO.Path.Combine(Application.dataPath, "ExcaliburAI/Config/llm.json");
#else
                return System.IO.Path.Combine(Application.persistentDataPath, "ExcaliburAI/Config/llm.json");
#endif
            }
        }

        public static LLMSettingsModel Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    var def = new LLMSettingsModel();
                    Save(def);
                    return def;
                }
                var json = File.ReadAllText(ConfigPath);
                return JsonUtility.FromJson<LLMSettingsModel>(json);
            }
            catch
            {
                return new LLMSettingsModel();
            }
        }

        public static void Save(LLMSettingsModel m)
        {
            var json = JsonUtility.ToJson(m, true);
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
