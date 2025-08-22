#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace ExcaliburAI.AI
{
    public class LocalOpenAIChatProvider : IChatProvider
    {
        private readonly LLMSettingsModel cfg;
        public LocalOpenAIChatProvider(LLMSettingsModel cfg) { this.cfg = cfg; }

        public async Task<string> ChatAsync(IList<ChatMessage> messages, bool stream, Action<string> onDelta, int maxTokens, float temperature, CancellationToken ct)
        {
            var url = cfg.localOpenAI.endpoint;
            var payload = new JObject
            {
                ["model"] = cfg.localOpenAI.model,
                ["temperature"] = (double)temperature,
                ["max_tokens"] = maxTokens>0 ? maxTokens : 512
            };
            var arr = new JArray();
            foreach (var m in messages)
            {
                var o = new JObject();
                o["role"] = m.role;
                o["content"] = m.content;
                arr.Add(o);
            }
            payload["messages"] = arr;

            using (var req = new UnityWebRequest(url, "POST"))
            {
                var body = Encoding.UTF8.GetBytes(payload.ToString());
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(cfg.localOpenAI.apiKey)) req.SetRequestHeader("Authorization", "Bearer " + cfg.localOpenAI.apiKey);
                req.timeout = cfg.timeoutSeconds>0 ? cfg.timeoutSeconds : 60;

                var op = req.SendWebRequest();
                while (!op.isDone) { await Task.Yield(); if (ct.IsCancellationRequested) break; }

                if (req.result != UnityWebRequest.Result.Success) throw new Exception("Chat error: " + req.error + "\n" + req.downloadHandler.text);

                var j = JObject.Parse(req.downloadHandler.text);
                // Ollama/OpenAI compatible: choices[0].message.content
                string content = (string)j["choices"]?[0]?["message"]?["content"];
                if (string.IsNullOrEmpty(content))
                {
                    // try plain 'message' root
                    content = (string)j["message"];
                }
                return content ?? "";
            }
        }
    }
}
#endif
