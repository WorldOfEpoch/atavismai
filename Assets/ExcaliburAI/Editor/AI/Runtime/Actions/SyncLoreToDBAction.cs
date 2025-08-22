#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace ExcaliburAI.AI
{
    public class SyncLoreToDBAction : IStudioAction
    {
        public string Name => "SyncLoreToDB";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string path = Path.Combine(Application.dataPath, "ExcaliburAI/Lore/lore.json");
            if (!File.Exists(path)) return "No lore.json found.";
            var j = JObject.Parse(File.ReadAllText(path));
            var nodes = j["nodes"] as JArray;
            if (nodes == null) return "No nodes in lore.json";
            string table = StudioConfig.TableName("lore");

            if (dryRun) return $"Would sync {nodes.Count} lore nodes to `{table}`.";

            var rows = new List<Dictionary<string,object>>();
            foreach (var n in nodes)
            {
                rows.Add(new Dictionary<string,object>{
                    {"ext_id", (string)n["id"]},
                    {"type", (string)n["type"]},
                    {"name", (string)n["name"]},
                    {"summary", (string)n["summary"]}
                });
            }
            AtavismDb.BulkInsert(table, rows);
            return $"Inserted {rows.Count} lore rows into `{table}`.";
        }
    }
}
#endif
