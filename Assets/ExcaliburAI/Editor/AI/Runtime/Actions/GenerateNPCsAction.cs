#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class GenerateNPCsAction : IStudioAction
    {
        public string Name => "GenerateNPCs";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            int count = 10; try{ if(args!=null&&args.ContainsKey("count")) count=int.Parse(args["count"]);}catch{}
            string path = Path.Combine(Application.dataPath, "ExcaliburAI/Staging/CSVs/NPCs.csv");
            var rows = new List<Dictionary<string,string>>();
            for (int i=0;i<count;i++)
            {
                string id = Guid.NewGuid().ToString("N").Substring(0,8);
                rows.Add(new Dictionary<string,string>{
                    {"id",id},{"name","NPC "+(i+1)},{"race","human"},{"class","warrior"},{"faction","neutral"},
                    {"location","Obsidian Reach"},{"role","vendor"},{"backstory","Auto-generated."}
                });
            }
            CsvUtil.Write(path, rows);
            return $"Generated {count} NPCs → {path}";
        }
    }
}
#endif
