#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    [Serializable] class LoreGraph { public List<LoreNode> nodes = new List<LoreNode>(); public List<LoreEdge> edges = new List<LoreEdge>(); }
    [Serializable] class LoreNode { public string id; public string type; public string name; public string summary; }
    [Serializable] class LoreEdge { public string from; public string to; public string rel; }

    public class BuildLoreGraphAction : IStudioAction
    {
        public string Name => "BuildLoreGraph";

        public string Run(Dictionary<string,string> args, bool dryRun)
        {
            string path = Path.Combine(Application.dataPath, "ExcaliburAI/Lore/lore.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var g = new LoreGraph();
            // Seed with placeholder factions/places; can be expanded by LLM later
            g.nodes.Add(new LoreNode{ id="f1", type="faction", name="Order of the Ember", summary="A militant order guarding ancient forges."});
            g.nodes.Add(new LoreNode{ id="l1", type="location", name="Obsidian Reach", summary="Volcanic frontier rich with rare ores."});
            g.edges.Add(new LoreEdge{ from="f1", to="l1", rel="controls"});
            File.WriteAllText(path, JsonUtility.ToJson(g, true));
            return "Created lore graph scaffold → " + path;
        }
    }
}
#endif
