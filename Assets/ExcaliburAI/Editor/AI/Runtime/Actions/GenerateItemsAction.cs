#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class GenerateItemsAction : IStudioAction
    {
        public string Name => "GenerateItems";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            int count = ParseInt(args, "count", 20);
            string category = args!=null && args.ContainsKey("category")? args["category"] : "misc";
            string staging = Path.Combine(Application.dataPath, "ExcaliburAI/Staging/CSVs/Items.csv");

            var rows = new List<Dictionary<string,string>>();
            var rng = new System.Random();
            for (int i=0;i<count;i++)
            {
                var id = Guid.NewGuid().ToString("N").Substring(0,8);
                string name = $"{Upper(category)} Item {i+1}";
                string rarity = Pick(rng, new[]{"common","uncommon","rare","epic"});
                string level = rng.Next(1, 60).ToString();
                string material = Pick(rng, new[]{"iron","steel","mithril","obsidian"});
                string element = Pick(rng, new[]{"none","fire","ice","lightning"});
                string prompt = $"{name}, {material}, {element}, centered, icon";
                rows.Add(new Dictionary<string,string>{
                    {"id", id}, {"name",name}, {"slug", Slug(name)}, {"category",category}, {"rarity",rarity},
                    {"level", level}, {"material",material}, {"element",element}, {"description","Auto-generated item."},
                    {"base_cost","0"}, {"icon_prompt", prompt}
                });
            }
            CsvUtil.Write(staging, rows);
            return $"Scaffolded {rows.Count} items → {staging}. Edit then AssignEconomy/GenerateIcons.";
        }

        static string Slug(string s){ return s.ToLower().Replace(" ","_"); }
        static string Pick(System.Random r, string[] arr){ return arr[r.Next(arr.Length)]; }
        static int ParseInt(Dictionary<string,string> a,string k,int d){try{if(a!=null&&a.ContainsKey(k)) return int.Parse(a[k]);}catch{}return d;}
        static string Upper(string s){ if(string.IsNullOrEmpty(s)) return s; return char.ToUpper(s[0])+ (s.Length>1?s.Substring(1):""); }
    }
}
#endif
