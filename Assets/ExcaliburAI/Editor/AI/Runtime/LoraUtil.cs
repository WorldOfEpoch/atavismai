#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace ExcaliburAI.AI
{
    public static class LoraUtil
    {
        public static string[] ScanFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return new string[0];
            var names = new List<string>();
            foreach (var f in Directory.GetFiles(folder, "*.safetensors"))
                names.Add(Path.GetFileNameWithoutExtension(f));
            foreach (var f in Directory.GetFiles(folder, "*.pt"))
            {
                var n = Path.GetFileNameWithoutExtension(f);
                if (!names.Contains(n)) names.Add(n);
            }
            return names.ToArray();
        }

        public static async Task<string[]> FetchFromApiAsync(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return new string[0];
            var url = endpoint.TrimEnd('/') + "/sdapi/v1/loras";
            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = 30;
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
                if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
                    return new string[0];
                try
                {
                    var arr = JArray.Parse(req.downloadHandler.text);
                    var list = new List<string>();
                    foreach (var x in arr)
                    {
                        var n = (string)x["name"];
                        if (!string.IsNullOrEmpty(n)) list.Add(n);
                    }
                    return list.ToArray();
                }
                catch { return new string[0]; }
            }
        }

        public static string BuildPromptPrefix(LLMSettingsModel cfg)
        {
            var a = cfg?.image?.automatic1111;
            if (a == null || a.loras == null) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var l in a.loras)
            {
                if (l != null && l.enabled && !string.IsNullOrEmpty(l.name))
                    sb.Append("<lora:" + l.name + ":" + l.weight.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "> ");
            }
            return sb.ToString();
        }
    }
}
#endif
