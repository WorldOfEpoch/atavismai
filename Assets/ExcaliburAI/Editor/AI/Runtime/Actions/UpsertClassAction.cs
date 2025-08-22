#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class UpsertClassAction : IStudioAction
    {
        public string Name => "UpsertClass";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string name = Arg(args,"name","");
            if (string.IsNullOrEmpty(name)) return "UpsertClass requires 'name'.";
            string primary = Arg(args,"primary_stat","STR");
            string resource = Arg(args,"resource","Mana");
            string armor = Arg(args,"armor_type","Medium");
            string weapons = Arg(args,"weapon_types","Sword,Bow");

            string table = StudioConfig.TableName("classes");

            if (dryRun) return $"Would upsert class '{name}' into `{table}`.";

            // Try update by name; if none updated, insert
            int n = AtavismDb.Exec($"UPDATE `{table}` SET primary_stat=@p,resource=@r,armor_type=@a,weapon_types=@w WHERE name=@n",
                new Dictionary<string,object>{{"@p",primary},{"@r",resource},{"@a",armor},{"@w",weapons},{"@n",name}});
            if (n==0)
            {
                AtavismDb.Exec($"INSERT INTO `{table}` (name,primary_stat,resource,armor_type,weapon_types) VALUES (@n,@p,@r,@a,@w)",
                    new Dictionary<string,object>{{"@n",name},{"@p",primary},{"@r",resource},{"@a",armor},{"@w",weapons}});
                return $"Inserted class '{name}'.";
            }
            return $"Updated class '{name}'.";
        }

        string Arg(Dictionary<string,string> a,string k,string d){ if(a!=null && a.ContainsKey(k) && !string.IsNullOrEmpty(a[k])) return a[k]; return d; }
    }
}
#endif
