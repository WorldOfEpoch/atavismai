#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public static class ActionPlanner
    {
        private static string systemPrompt = 
            "You are ExcaliburAI Planner. Convert the user's request into a JSON plan that matches this C# schema:\n" +
            "{\"intents\":[{\"action\":string, \"args\":{key:string}}], \"dryRun\":bool, \"note\":string}.\n" +
            "Actions: GenerateClasses(args: count:int, race:string|optional), GenerateRaces(args: list:string), GenerateItemsFromCSV(args: path:string), GenerateItems(args: count:int, category:string|optional), AssignEconomy(args: path:string|optional), GenerateIcons(args: source:string=csv|db, filter:string|optional), BuildLoreGraph(args:{}), GenerateNPCs(args: count:int|optional), GenerateDialogues(args: npc_source:string=csv|db).\n" +
            "Default to dryRun:true. Always produce VALID JSON only.";

        public static async Task<Plan> PlanAsync(LocalOpenAIChatProvider chat, string userText, CancellationToken ct)
        {
            try
            {
                var msgs = new List<ChatMessage> {
                    new ChatMessage("system", systemPrompt),
                    new ChatMessage("user", userText)
                };
                string resp = await chat.ChatAsync(msgs, false, null, 512, 0.2f, ct);
                var plan = JsonUtility.FromJson<Plan>(resp);
                if (plan == null) plan = new Plan();
                if (plan.intents == null) plan.intents = new List<PlanIntent>();
                return plan;
            }
            catch (Exception ex)
            {
                Debug.LogError("Planner error: " + ex.Message);
                var fallback = new Plan();
                fallback.note = "Planner failed; no actions generated.";
                return fallback;
            }
        }
    }
}
#endif
