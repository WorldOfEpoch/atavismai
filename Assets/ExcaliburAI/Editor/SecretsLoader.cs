// ExcaliburAI – SecretsLoader
// Reads Secrets/excalibur.secrets.json from the project root (outside Assets)
// and exposes a strongly-typed config for editor code.

using System;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.Editor
{
    #region Secrets DTOs (match your JSON keys exactly)

    [Serializable] public class OpenAISecrets   { public string apiKey; public string textModel; public string imageModel; }
    [Serializable] public class StabilitySecrets{ public string apiKey; }
    [Serializable] public class ReplicateSecrets{ public string apiToken; }

    [Serializable] public class GeminiSecrets
    {
        // either "model" or "textModel" is fine; we’ll read whichever you fill
        public string apiKey;
        public string model;
        public string textModel;
    }

    [Serializable] public class LocalOpenAISecrets
    {
        // For Ollama / LM Studio / any OpenAI-compatible local server
        // Example: endpoint = "http://localhost:11434/v1/chat/completions"
        public string endpoint;
        public string model;
        public string apiKey; // optional for local
    }

    [Serializable] public class MySqlSecrets
    {
        public string host;
        public int    port = 3306;
        public string user;
        public string password;
        public string database = "world_content";
        public bool   ssl = false;
    }

    [Serializable]
    public class SecretsConfig
    {
        // Which provider the tool should use: "dummy", "openai", "gemini", "local-openai"
        public string provider = "dummy";

        public OpenAISecrets      openai;
        public StabilitySecrets   stability;
        public ReplicateSecrets   replicate;
        public GeminiSecrets      gemini;
        public LocalOpenAISecrets localOpenAI;
        public MySqlSecrets       mysql;
    }

    #endregion

    public static class SecretsLoader
    {
        public static string ProjectRoot =>
            Directory.GetParent(Application.dataPath).FullName;

        public static string SecretsPath =>
            Path.Combine(ProjectRoot, "Secrets", "excalibur.secrets.json");

        /// <summary>Load secrets. Returns null if missing or invalid JSON.</summary>
        public static SecretsConfig Load()
        {
            try
            {
                if (!File.Exists(SecretsPath))
                {
                    Debug.LogWarning($"ExcaliburAI: Secrets file not found at: {SecretsPath}");
                    return null;
                }

                var json = File.ReadAllText(SecretsPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning("ExcaliburAI: Secrets file is empty.");
                    return null;
                }

                var cfg = JsonUtility.FromJson<SecretsConfig>(json);
                if (cfg == null)
                {
                    Debug.LogError("ExcaliburAI: Failed to parse secrets JSON.");
                    return null;
                }

                // Normalize a couple optional fields for convenience
                if (cfg.gemini != null && string.IsNullOrEmpty(cfg.gemini.textModel) && !string.IsNullOrEmpty(cfg.gemini.model))
                    cfg.gemini.textModel = cfg.gemini.model;

                return cfg;
            }
            catch (Exception e)
            {
                Debug.LogError($"ExcaliburAI: Error loading secrets: {e}");
                return null;
            }
        }

        /// <summary>Quick provider name for UI.</summary>
        public static string GetConfiguredProvider()
        {
            try
            {
                if (!File.Exists(SecretsPath)) return "missing";
                var json = File.ReadAllText(SecretsPath);
                var cfg  = JsonUtility.FromJson<SecretsConfig>(json);
                return cfg?.provider ?? "missing";
            }
            catch { return "error"; }
        }

        /// <summary>True if mysql section is present and minimally filled.</summary>
        public static bool HasMySql(SecretsConfig cfg)
        {
            return cfg != null && cfg.mysql != null &&
                   !string.IsNullOrWhiteSpace(cfg.mysql.host) &&
                   !string.IsNullOrWhiteSpace(cfg.mysql.user) &&
                   !string.IsNullOrWhiteSpace(cfg.mysql.database);
        }
    }
}
