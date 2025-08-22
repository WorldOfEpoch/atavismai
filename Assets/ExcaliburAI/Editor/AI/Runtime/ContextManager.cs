#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    [Serializable]
    public class ConversationMemory
    {
        public List<string> lastUserUtterances = new List<string>();
        public List<string> lastAIUtterances = new List<string>();
        public int maxPairs = 8;
    }

    public static class ContextManager
    {
        static string MemPath
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "ExcaliburAI/Config/memory.json");
#else
                return Path.Combine(Application.persistentDataPath, "ExcaliburAI/Config/memory.json");
#endif
            }
        }

        public static ConversationMemory LoadMem()
        {
            try
            {
                if (!File.Exists(MemPath)) { var m = new ConversationMemory(); SaveMem(m); return m; }
                return JsonUtility.FromJson<ConversationMemory>(File.ReadAllText(MemPath));
            } catch { return new ConversationMemory(); }
        }

        public static void SaveMem(ConversationMemory m)
        {
            var dir = Path.GetDirectoryName(MemPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(MemPath, JsonUtility.ToJson(m, true));
        }

        public static void PushExchange(string user, string ai)
        {
            var mem = LoadMem();
            mem.lastUserUtterances.Add(user??"");
            mem.lastAIUtterances.Add(ai??"");
            // Trim
            while (mem.lastUserUtterances.Count > mem.maxPairs) mem.lastUserUtterances.RemoveAt(0);
            while (mem.lastAIUtterances.Count > mem.maxPairs) mem.lastAIUtterances.RemoveAt(0);
            SaveMem(mem);
        }

        public static string BuildSystemPrompt(ProjectProfileModel p, string dbSnapshot)
        {
            // Guardrails: ALWAYS Atavism MMORPG assistant, no drifting
            return
                "You are ExcaliburAI, an in-Unity assistant for building an MMORPG on Atavism. " +
                "Follow the user's instructions exactly, but NEVER drift from this purpose. " +
                "Use the project profile for style and constraints. " +
                "Respect the database state: do not duplicate existing names and do not invent forbidden races/classes. " +
                "If the user asks to create or modify content, plan CRUD operations against the Atavism DB. " +
                "Prefer CSV staging, then apply to DB when asked. " +
                "If the user mentions 'generate lore', expand the lore while keeping consistency with existing factions/locations/characters. " +
                "\n\nPROJECT PROFILE\n" +
                "Name: " + (p.projectName ?? "") + "\n" +
                "Genre: " + (p.genre ?? "") + "\n" +
                "Allowed Races: " + string.Join(", ", p.allowedRaces ?? new string[0]) + "\n" +
                "Allowed Classes: " + (p.allowedClasses!=null && p.allowedClasses.Length>0 ? string.Join(", ", p.allowedClasses) : "(open)") + "\n" +
                "Tone: " + (p.tone ?? "") + "\n" +
                "Naming Style: " + (p.namingStyle ?? "") + "\n" +
                "RestrictToAllowedRaces: " + (p.restrictToAllowedRaces ? "true" : "false") + "\n" +
                "World Summary: " + (p.worldSummary ?? "") + "\n\n" +
                "DB SNAPSHOT (read-only summary for planning):\n" + (dbSnapshot ?? "(none)");
        }
    }
}
#endif
