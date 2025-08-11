using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ExcaliburAI.Editor
{
    [Serializable] public class OpenAISecrets { public string apiKey; public string textModel; public string imageModel; }
    [Serializable] public class StabilitySecrets { public string apiKey; }
    [Serializable] public class ReplicateSecrets { public string apiToken; }

    [Serializable]
    public class SecretsConfig
    {
        public string provider = "dummy";
        public OpenAISecrets openai;
        public StabilitySecrets stability;
        public ReplicateSecrets replicate;
    }

    public static class SecretsLoader
    {
        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        public static string SecretsPath => Path.Combine(ProjectRoot, "Secrets", "excalibur.secrets.json");

        public static SecretsConfig Load()
        {
            try
            {
                if (!File.Exists(SecretsPath))
                {
                    Debug.LogWarning($"ExcaliburAI: secrets file not found at {SecretsPath}");
                    return null;
                }
                var json = File.ReadAllText(SecretsPath);
                var cfg = JsonUtility.FromJson<SecretsConfig>(json);
                return cfg;
            }
            catch (Exception e)
            {
                Debug.LogError("ExcaliburAI: failed to load secrets: " + e);
                return null;
            }
        }
    }
}
