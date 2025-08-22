#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class GenerateItemsFromCSVAction : IStudioAction
    {
        public string Name => "GenerateItemsFromCSV";

        public string Run(System.Collections.Generic.Dictionary<string,string> args, bool dryRun)
        {
            string path = (args!=null && args.ContainsKey("path"))? args["path"] : Path.Combine(Application.dataPath, "ExcaliburAI/Templates/Items.csv");
            if (!File.Exists(path)) return "CSV not found: " + path;

            var rows = CsvUtil.Read(path);
            if (rows.Count==0) return "CSV is empty: " + path;

            // Apply to DB or preview
            string table = StudioConfig.TableName("items");
            if (dryRun) return $"Loaded {rows.Count} items from CSV. Ready to insert into `{table}`.";

            // Convert rows to DB-friendly types
            var dbRows = new List<System.Collections.Generic.Dictionary<string,object>>();
            foreach (var r in rows)
            {
                var d = new System.Collections.Generic.Dictionary<string,object>();
                foreach (var kv in r) d[kv.Key]=kv.Value;
                dbRows.Add(d);
            }
            AtavismDb.BulkInsert(table, dbRows);
            return $"Inserted {rows.Count} rows into `{table}`.";
        }
    }
}
#endif
