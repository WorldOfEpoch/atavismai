#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    [Serializable]
    public class ReferenceConfig
    {
        public string referenceFolder = "";
        public string activeReference = ""; // full path of selected image
    }

    public static class ReferenceLibrary
    {
        public static string ConfigPath
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "ExcaliburAI/Config/references.json");
#else
                return Path.Combine(Application.persistentDataPath, "ExcaliburAI/Config/references.json");
#endif
            }
        }

        public static ReferenceConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    var def = new ReferenceConfig();
                    Save(def);
                    return def;
                }
                var json = File.ReadAllText(ConfigPath);
                return JsonUtility.FromJson<ReferenceConfig>(json);
            }
            catch
            {
                return new ReferenceConfig();
            }
        }

        public static void Save(ReferenceConfig cfg)
        {
            var json = JsonUtility.ToJson(cfg, true);
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(ConfigPath, json);
        }

        public static List<string> ScanImages(string folder)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return list;
            var exts = new string[] { "*.png", "*.jpg", "*.jpeg", "*.webp" };
            foreach (var ext in exts)
            {
                foreach (var f in Directory.GetFiles(folder, ext, SearchOption.AllDirectories))
                {
                    // Use string replace to avoid char literal escaping issues on some compilers
                    list.Add(f.Replace("\\", "/"));
                }
            }
            return list;
        }

        public static string ToBase64(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);
            return Convert.ToBase64String(bytes);
        }
    }
}
#endif
