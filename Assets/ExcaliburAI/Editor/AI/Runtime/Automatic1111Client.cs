#if UNITY_EDITOR
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ExcaliburAI.AI
{
    public class Automatic1111ImageGenerator : IImageGenerator
    {
        private readonly LLMSettingsModel cfg;

        public Automatic1111ImageGenerator(LLMSettingsModel cfg)
        {
            this.cfg = cfg;
        }

        string Endpoint => cfg?.image?.automatic1111?.endpoint?.TrimEnd('/') ?? "http://127.0.0.1:7861";

        // Original txt2img pathway
        public async Task<string> Txt2ImgAsync(string entryPrompt, string entryNegative = "", int width = -1, int height = -1, int steps = -1, float cfgScale = -1f, CancellationToken ct = default(CancellationToken))
        {
            return await GenerateAsync(entryPrompt, entryNegative, useImg2Img:false, denoise:0.35f, width:width, height:height, steps:steps, cfgScale:cfgScale, ct:ct);
        }

        // New: one method that can do txt2img or img2img depending on 'useImg2Img' and presence of a reference
        public async Task<string> GenerateAsync(string entryPrompt, string entryNegative, bool useImg2Img, float denoise, int width, int height, int steps, float cfgScale, CancellationToken ct)
        {
            var a = cfg.image.automatic1111;
            var refCfg = ReferenceLibrary.Load();

            // Compose prompt
            var loraPrefix = LoraUtil.BuildPromptPrefix(cfg);
            var positive = (loraPrefix + " " + a.persistentPrompt + " " + (entryPrompt ?? "")).Trim();
            var negative = (a.persistentNegative + " " + (entryNegative ?? "")).Trim();

            int W = (width  > 0 ? width  : a.width);
            int H = (height > 0 ? height : a.height);
            int S = (steps  > 0 ? steps  : a.steps);
            double CFG = (cfgScale > 0 ? cfgScale : a.cfgScale);

            if (!useImg2Img || string.IsNullOrEmpty(refCfg.activeReference) || !File.Exists(refCfg.activeReference))
            {
                // txt2img
                var url = Endpoint + "/sdapi/v1/txt2img";
                var payload = new JObject
                {
                    ["prompt"] = positive,
                    ["negative_prompt"] = negative,
                    ["width"] = W,
                    ["height"] = H,
                    ["steps"] = S,
                    ["cfg_scale"] = CFG,
                    ["sampler_name"] = a.samplerName
                };
                if (a.enableHires)
                {
                    payload["enable_hr"] = true;
                    payload["denoising_strength"] = a.hiresDenoising;
                    payload["hr_scale"] = a.hiresScale;
                    payload["hr_upscaler"] = a.hiresUpscaler;
                }
                return await PostAndSaveAsync(url, payload, ct);
            }
            else
            {
                // img2img with selected reference
                var url = Endpoint + "/sdapi/v1/img2img";
                string b64 = ReferenceLibrary.ToBase64(refCfg.activeReference);
                var payload = new JObject
                {
                    ["prompt"] = positive,
                    ["negative_prompt"] = negative,
                    ["steps"] = S,
                    ["cfg_scale"] = CFG,
                    ["sampler_name"] = a.samplerName,
                    ["denoising_strength"] = (double)denoise,
                    ["resize_mode"] = 1, // Crop and Resize
                    ["width"] = W,
                    ["height"] = H,
                    ["init_images"] = new JArray(b64)
                };
                return await PostAndSaveAsync(url, payload, ct);
            }
        }

        private async Task<string> PostAndSaveAsync(string url, JObject payload, CancellationToken ct)
        {
            var json = payload.ToString();
            using (var req = new UnityWebRequest(url, "POST"))
            {
                var body = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 180;

                var op = req.SendWebRequest();
                while (!op.isDone)
                {
                    await Task.Yield();
                    if (ct.IsCancellationRequested) break;
                }

                if (req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError)
                    throw new Exception("A1111 request failed: " + req.error + "\n" + req.downloadHandler.text);

                var respText = req.downloadHandler.text;
                var resp = JObject.Parse(respText);
                var images = resp["images"] as JArray;
                if (images == null || images.Count == 0)
                    throw new Exception("Automatic1111 returned no images");

                var b64 = (string)images[0];
                var bytes = Convert.FromBase64String(b64);

                var dir = "Assets/ExcaliburAI/Staging/Images";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var assetPath = dir + "/gen_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                File.WriteAllBytes(assetPath, bytes);
                AssetDatabase.ImportAsset(assetPath);
                return assetPath;
            }
        }
    }
}
#endif
