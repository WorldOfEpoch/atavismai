#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class GenerateIconsAction : IStudioAction
    {
        public string Name => "GenerateIcons";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string source = (args!=null && args.ContainsKey("source"))? args["source"] : "csv";
            string filter = (args!=null && args.ContainsKey("filter"))? args["filter"] : "";
            if (dryRun) return $"Would generate icons from {source} (filter: {filter}).";

            string csv = Path.Combine(Application.dataPath, "ExcaliburAI/Staging/CSVs/Items.csv");
            var rows = CsvUtil.Read(csv);
            if (rows.Count==0) return "No items in CSV staging. Run GenerateItems / load CSV first.";
            int done=0;
            foreach (var r in rows)
            {
                if (!string.IsNullOrEmpty(filter) && !(r.ContainsKey("category") && r["category"].Contains(filter))) continue;
                string prompt = r.ContainsKey("icon_prompt")? r["icon_prompt"] : (r.ContainsKey("name")? r["name"] : "game icon");
                try
                {
                    var gen = new Automatic1111ImageGenerator(LLMConfig.Load());
                    var task = gen.Txt2ImgAsync(prompt, "blurry,text,watermark", 512,512, 24, 6.5f, CancellationToken.None);
                    task.Wait();
                    string path = task.Result;
                    done++;
                } catch (Exception ex)
                {
                    Debug.LogError("Icon gen failed: "+ex.Message);
                }
            }
            AssetDatabase.Refresh();
            return $"Generated {done} icons to {StudioConfig.PathImageStaging()}";
        }
    }
}
#endif
