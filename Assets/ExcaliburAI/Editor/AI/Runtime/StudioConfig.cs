#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace ExcaliburAI.AI
{
    public static class StudioConfig
    {
        static string mapPath => Path.Combine(Application.dataPath, "ExcaliburAI/Config/atavism_mapping.json");
        [System.Serializable] class Map { public Tables tables = new Tables(); public Paths paths = new Paths(); }
        [System.Serializable] class Tables { public string items="atavism_item_templates"; public string classes="atavism_classes"; public string races="atavism_races"; public string npc="atavism_npc_templates"; public string dialogue="atavism_dialogue_nodes"; public string lore="atavism_lore"; }
        [System.Serializable] class Paths { public string iconsOutput="Assets/Atavism/Icons/"; public string csvStaging="Assets/ExcaliburAI/Staging/CSVs/"; public string imageStaging="Assets/ExcaliburAI/Staging/Images/"; }

        static Map _m;
        static Map M { get { if (_m==null){ try{ _m = JsonUtility.FromJson<Map>(File.ReadAllText(mapPath)); } catch { _m=new Map(); } } return _m; } }
        public static string TableName(string key) { var t=M.tables; switch(key){case"items":return t.items;case"classes":return t.classes;case"races":return t.races;case"npc":return t.npc;case"dialogue":return t.dialogue;case"lore":return t.lore;default:return key;} }
        public static string PathCsvStaging() => M.paths.csvStaging;
        public static string PathImageStaging() => M.paths.imageStaging;
        public static string PathIconsOutput() => M.paths.iconsOutput;
    }
}
#endif
