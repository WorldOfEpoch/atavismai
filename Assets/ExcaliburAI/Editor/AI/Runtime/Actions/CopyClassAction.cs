#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class CopyClassAction : IStudioAction
    {
        public string Name => "CopyClass";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string from = Arg(args,"from","");
            string to   = Arg(args,"to","");
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) return "CopyClass requires 'from' and 'to'.";
            string table = StudioConfig.TableName("classes");
            if (dryRun) return $"Would copy class '{from}' → '{to}'.";

            var rows = AtavismDbExt.QueryRows($"SELECT primary_stat,resource,armor_type,weapon_types FROM `{table}` WHERE name='{MySqlConnector.MySqlHelper.EscapeString(from)}' LIMIT 1");
            if (rows.Count==0) return $"Source class '{from}' not found.";
            var r = rows[0];
            AtavismDb.Exec($"INSERT INTO `{table}` (name,primary_stat,resource,armor_type,weapon_types) VALUES (@n,@p,@r,@a,@w)",
                new Dictionary<string,object>{{"@n",to},{"@p",r["primary_stat"]},{"@r",r["resource"]},{"@a",r["armor_type"]},{"@w",r["weapon_types"]}});
            return $"Copied '{from}' to '{to}'.";
        }

        string Arg(Dictionary<string,string> a,string k,string d){ if(a!=null && a.ContainsKey(k) && !string.IsNullOrEmpty(a[k])) return a[k]; return d; }
    }
}
#endif
