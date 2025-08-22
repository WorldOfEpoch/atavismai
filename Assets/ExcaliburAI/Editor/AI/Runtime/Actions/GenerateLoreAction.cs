#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ExcaliburAI.AI
{
    public class GenerateLoreAction : IStudioAction
    {
        public string Name => "GenerateLore";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            // Kick off synchronous generation for simplicity
            string scope = (args!=null && args.ContainsKey("scope"))? args["scope"] : "factions, locations, artifacts";
            string outPath = Path.Combine(Application.dataPath, "ExcaliburAI/Lore/lore.json");
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

            var profile = ProjectProfile.Load();
            var cfg = LLMConfig.Load();
            var provider = new LocalOpenAIChatProvider(cfg);

            string dbSnap = AtavismDbExt.SnapshotSummary();
            string system = ContextManager.BuildSystemPrompt(profile, dbSnap);
            string user = "Generate new lore entries for: " + scope + ". Keep consistency with existing lore; avoid duplicates. Return JSON with {nodes:[{id,type,name,summary}], edges:[{from,to,rel}]}.";
            var msgs = new List<ChatMessage> { new ChatMessage("system", system), new ChatMessage("user", user) };

            string resp = provider.ChatAsync(msgs, false, null, 900, 0.2f, CancellationToken.None).Result;
            try
            {
                var j = JObject.Parse(resp);
                File.WriteAllText(outPath, j.ToString());
                return "Lore updated → " + outPath;
            }
            catch
            {
                // fallback: write as plain text so nothing is lost
                File.WriteAllText(outPath, resp);
                return "Lore text saved (not valid JSON) → " + outPath;
            }
        }
    }
}
#endif
