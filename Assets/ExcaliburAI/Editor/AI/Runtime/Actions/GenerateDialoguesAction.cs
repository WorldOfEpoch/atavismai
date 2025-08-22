#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public class GenerateDialoguesAction : IStudioAction
    {
        public string Name => "GenerateDialogues";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string path = Path.Combine(Application.dataPath, "ExcaliburAI/Staging/CSVs/Dialogue.csv");
            var rows = new List<Dictionary<string,string>>();
            // Minimal seed nodes
            rows.Add(new Dictionary<string,string>{{"id","d1"},{"npc_id","npc1"},{"node_type","line"},{"text","Welcome to the Obsidian Reach."},{"choices",""},{"next_ids",""},{"conditions",""}});
            rows.Add(new Dictionary<string,string>{{"id","d2"},{"npc_id","npc1"},{"node_type","choice"},{"text","How can I help you?"},{"choices","Trade|d3; Lore|d4"},{"next_ids","d3|d4"},{"conditions",""}});
            CsvUtil.Write(path, rows);
            return "Scaffolded dialogue → " + path;
        }
    }
}
#endif
