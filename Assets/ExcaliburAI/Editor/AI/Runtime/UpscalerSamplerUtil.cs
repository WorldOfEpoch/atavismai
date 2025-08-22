#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace ExcaliburAI.AI
{
    public static class UpscalerSamplerUtil
    {
        public static async Task<string[]> GetUpscalersAsync(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return new string[0];
            var url = endpoint.TrimEnd('/') + "/sdapi/v1/upscalers";
            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = 20;
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError ||
                    req.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError)
                    return new string[0];
                try
                {
                    var arr = JArray.Parse(req.downloadHandler.text);
                    var list = new List<string>();
                    foreach (var x in arr)
                    {
                        var name = (string)x["name"];
                        if (!string.IsNullOrEmpty(name)) list.Add(name);
                    }
                    return list.ToArray();
                }
                catch { return new string[0]; }
            }
        }

        public static async Task<string[]> GetSamplersAsync(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return new string[0];
            var url = endpoint.TrimEnd('/') + "/sdapi/v1/samplers";
            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = 20;
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError ||
                    req.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError)
                    return new string[0];
                try
                {
                    var arr = JArray.Parse(req.downloadHandler.text);
                    var list = new List<string>();
                    foreach (var x in arr)
                    {
                        var name = (string)x["name"];
                        if (!string.IsNullOrEmpty(name)) list.Add(name);
                    }
                    return list.ToArray();
                }
                catch { return new string[0]; }
            }
        }
    }
}
#endif
