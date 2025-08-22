#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class AssignEconomyAction : IStudioAction
    {
        public string Name => "AssignEconomy";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string path = (args!=null && args.ContainsKey("path"))? args["path"] : Path.Combine(Application.dataPath, "ExcaliburAI/Staging/CSVs/Items.csv");
            var rows = CsvUtil.Read(path);
            if (rows.Count==0) return "No items to price in: " + path;

            foreach (var r in rows)
            {
                int level = ParseInt(r, "level", 1);
                string rarity = r.ContainsKey("rarity")? r["rarity"] : "common";
                int baseCost = BasePrice(level, rarity);
                r["base_cost"] = baseCost.ToString();
            }
            CsvUtil.Write(path, rows);
            return $"Priced {rows.Count} items → {path}.";
        }

        static int ParseInt(Dictionary<string,string> r,string k,int d){try{ if(r.ContainsKey(k)) return int.Parse(r[k]); }catch{} return d;}
        static int BasePrice(int level, string rarity)
        {
            int mult = 1;
            switch((rarity??"common").ToLower())
            {
                case "uncommon": mult=2; break;
                case "rare": mult=5; break;
                case "epic": mult=10; break;
            }
            return Math.Max(1, level*10*mult);
        }
    }
}
#endif
