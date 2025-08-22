#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class ReadClassesAction : IStudioAction
    {
        public string Name => "ReadClasses";
        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string table = StudioConfig.TableName("classes");
            var rows = AtavismDbExt.QueryRows($"SELECT id,name,primary_stat,resource,armor_type,weapon_types FROM `{table}` LIMIT 200");
            return $"Read {rows.Count} classes from `{table}`.";
        }
    }
}
#endif
