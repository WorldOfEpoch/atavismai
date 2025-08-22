#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class GenerateClassesAction : IStudioAction
    {
        public string Name => "GenerateClasses";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            int count = ParseInt(args, "count", 5);
            string race = args!=null && args.ContainsKey("race")? args["race"] : "";
            string staging = Path.Combine(Application.dataPath, "ExcaliburAI/Staging/CSVs/Classes.csv");

            var rows = new List<Dictionary<string,string>>();
            for (int i=0;i<count;i++)
            {
                var id = Guid.NewGuid().ToString("N").Substring(0,8);
                var cls = new Dictionary<string,string> {
                    {"id", id},
                    {"name", (string.IsNullOrEmpty(race)? "": (Upper(race)+" ")) + "Class"+(i+1)},
                    {"primary_stat","STR"},
                    {"resource","Rage"},
                    {"armor_type","Heavy"},
                    {"weapon_types","Sword,Shield"},
                    {"description","Auto-generated class."}
                };
                rows.Add(cls);
            }
            CsvUtil.Write(staging, rows);
            return $"Generated {rows.Count} classes → {staging} (review then import).";
        }

        static int ParseInt(Dictionary<string,string> a,string k,int d){try{if(a!=null&&a.ContainsKey(k)) return int.Parse(a[k]);}catch{}return d;}
        static string Upper(string s){ if(string.IsNullOrEmpty(s)) return s; return char.ToUpper(s[0])+ (s.Length>1?s.Substring(1):""); }
    }
}
#endif
