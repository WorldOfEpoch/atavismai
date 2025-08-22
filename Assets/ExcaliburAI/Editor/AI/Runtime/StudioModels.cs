using System;
using System.Collections.Generic;

namespace ExcaliburAI.AI
{
    [Serializable] public class PlanIntent
    {
        public string action;
        public Dictionary<string, string> args;
    }

    [Serializable] public class Plan
    {
        public List<PlanIntent> intents = new List<PlanIntent>();
        public bool dryRun = true;
        public string note = "";
    }

    [Serializable] public class ItemRow
    {
        public string id, name, slug, category, rarity, level, material, element, description, base_cost, icon_prompt;
    }

    [Serializable] public class ClassRow { public string id, name, primary_stat, resource, armor_type, weapon_types, description; }
    [Serializable] public class RaceRow  { public string id, name, description, home_region, traits; }
    [Serializable] public class NPCRow   { public string id, name, race, @class, faction, location, role, backstory; }
    [Serializable] public class DialogueRow { public string id, npc_id, node_type, text, choices, next_ids, conditions; }
}
