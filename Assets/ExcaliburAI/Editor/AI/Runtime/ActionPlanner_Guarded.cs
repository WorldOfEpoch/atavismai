#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public static class GuardedPlanner
    {
        public static async Task<Plan> PlanAsync(LocalOpenAIChatProvider chat, string userText, CancellationToken ct)
        {
            // Command-mode fallback
            var cmdPlan = PlannerUtil.TryParseCommand(userText);
            if (cmdPlan != null) return cmdPlan;

            var profile = ProjectProfile.Load();
            var snapshot = AtavismDbExt.SnapshotSummary();
            string system = ContextManager.BuildSystemPrompt(profile, snapshot) +
                            "\nPlan only actions relevant to building the MMORPG. " +
                            "Use these actions (args in parentheses):\n" +
                            "- ReadClasses()\n- UpsertClass(name,primary_stat,resource,armor_type,weapon_types)\n- DeleteClass(name)\n- CopyClass(from,to)\n" +
                            "- GenerateItems(count,category)\n- GenerateItemsFromCSV(path)\n- AssignEconomy(path)\n- GenerateIcons(source,filter)\n" +
                            "- GenerateLore(scope)\n- SyncLoreToDB()\n" +
                            "Return ONLY valid JSON: {intents:[{action,args}], dryRun:true/false, note:string}.";

            var msgs = new List<ChatMessage> {
                new ChatMessage("system", system),
                new ChatMessage("user", userText)
            };
            string resp = await chat.ChatAsync(msgs, false, null, 800, 0.2f, ct);
            Plan p = null;
            try { p = JsonUtility.FromJson<Plan>(resp); } catch {}

            if (p == null)
            {
                string j = PlannerUtil.ExtractJson(resp);
                if (!string.IsNullOrEmpty(j))
                {
                    try { p = JsonUtility.FromJson<Plan>(j); } catch {}
                }
            }
            if (p == null) p = new Plan();
            if (p.intents == null) p.intents = new List<PlanIntent>();
            if (p.note == null) p.note = "";
            return p;
        }
    }
}
#endif
