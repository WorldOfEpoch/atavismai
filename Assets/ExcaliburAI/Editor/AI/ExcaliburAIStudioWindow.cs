#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public partial class ExcaliburAIStudioWindow : EditorWindow
    {
        Vector2 chatScroll, planScroll, logScroll, profileScroll;
        List<(string role,string text)> chat = new List<(string,string)>();
        string input = "";
        Plan currentPlan = new Plan();
        bool autoApply = false;
        string runLog = "";
        ProjectProfileModel profile;

        [MenuItem("ExcaliburAI/Studio")]
        public static void Open() { var w = GetWindow<ExcaliburAIStudioWindow>("ExcaliburAI Studio"); w.minSize = new Vector2(1080,720); }

        void OnEnable(){ chat.Clear(); Log("ExcaliburAI ready. Describe what to build."); profile = ProjectProfile.Load(); }
        void Log(string s){ runLog += s + "\n"; Repaint(); }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // --- Left: Chat ---
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width*0.36f));
            GUILayout.Label("Chat", EditorStyles.boldLabel);
            chatScroll = EditorGUILayout.BeginScrollView(chatScroll, "box");
            foreach (var m in chat){ GUILayout.Label(m.role + ": " + m.text, EditorStyles.wordWrappedLabel); GUILayout.Space(4); }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            input = EditorGUILayout.TextField(input);
            if (GUILayout.Button("Send", GUILayout.Width(80))) _ = SendAsync();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // --- Middle: Plan ---
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width*0.28f));
            GUILayout.Label("Planned Actions", EditorStyles.boldLabel);
            planScroll = EditorGUILayout.BeginScrollView(planScroll, "box");
            if (currentPlan!=null && currentPlan.intents!=null)
            {
                foreach (var it in currentPlan.intents)
                {
                    GUILayout.Label("• " + it.action);
                    if (it.args!=null) foreach (var kv in it.args) GUILayout.Label("   - "+kv.Key+": "+kv.Value, EditorStyles.miniLabel);
                    GUILayout.Space(3);
                }
                GUILayout.Label("Dry Run: " + currentPlan.dryRun);
                if (!string.IsNullOrEmpty(currentPlan.note)) GUILayout.Label("Note: "+ currentPlan.note, EditorStyles.wordWrappedMiniLabel);
            }
            else GUILayout.Label("No plan yet.");
            EditorGUILayout.EndScrollView();
            autoApply = GUILayout.Toggle(autoApply, "Auto-apply changes");
            if (GUILayout.Button("Run Plan", GUILayout.Height(30))) RunPlan();
            EditorGUILayout.EndVertical();

            // --- Right: Profile + Log tabs ---
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Project Profile", EditorStyles.boldLabel);
            profileScroll = EditorGUILayout.BeginScrollView(profileScroll, "box");
            profile.projectName = EditorGUILayout.TextField("Project Name", profile.projectName);
            profile.genre = EditorGUILayout.TextField("Genre", profile.genre);
            profile.tone = EditorGUILayout.TextField("Tone", profile.tone);
            profile.namingStyle = EditorGUILayout.TextField("Naming Style", profile.namingStyle);
            profile.restrictToAllowedRaces = EditorGUILayout.Toggle("Restrict to Allowed Races", profile.restrictToAllowedRaces);
            profile.requireDBAwareness = EditorGUILayout.Toggle("Require DB Awareness", profile.requireDBAwareness);
            EditorGUILayout.LabelField("Allowed Races (comma-separated)");
            string races = string.Join(", ", profile.allowedRaces ?? new string[0]);
            races = EditorGUILayout.TextField(races);
            profile.allowedRaces = SplitCsv(races);
            EditorGUILayout.LabelField("Allowed Classes (optional, comma-separated)");
            string classes = string.Join(", ", profile.allowedClasses ?? new string[0]);
            classes = EditorGUILayout.TextField(classes);
            profile.allowedClasses = SplitCsv(classes);
            profile.worldSummary = EditorGUILayout.TextField("World Summary", profile.worldSummary);
            if (GUILayout.Button("Save Profile")) ProjectProfile.Save(profile);
            EditorGUILayout.EndScrollView();

            GUILayout.Label("Log", EditorStyles.boldLabel);
            logScroll = EditorGUILayout.BeginScrollView(logScroll, "box");
            GUILayout.Label(runLog, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Clear Log")) runLog = "";
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            string user = input;
            chat.Add(("you", user));
            input = ""; Repaint();

            try
            {
                var cfg = LLMConfig.Load();
                var provider = new LocalOpenAIChatProvider(cfg);

                // Build a guarded system prompt and fetch a natural-language reply
                var sys = ContextManager.BuildSystemPrompt(ProjectProfile.Load(), AtavismDbExt.SnapshotSummary());
                var msgs = new List<ChatMessage>{ new ChatMessage("system", sys), new ChatMessage("user", user)};
                string reply = await provider.ChatAsync(msgs, false, null, 512, 0.2f, CancellationToken.None);
                chat.Add(("ai", reply));
                ContextManager.PushExchange(user, reply);
                Repaint();

                // DB-aware planning
                var plan = await GuardedPlanner.PlanAsync(provider, user, CancellationToken.None);
                currentPlan = plan;
                Log("Planner produced " + (plan.intents!=null? plan.intents.Count:0) + " intents.");
            }
            catch (Exception ex)
            {
                chat.Add(("ai", "Error: "+ex.Message));
            }
        }

        void RunPlan()
        {
            if (currentPlan==null || currentPlan.intents==null || currentPlan.intents.Count==0) { Log("No plan to run."); return; }
            bool dry = !autoApply && currentPlan.dryRun;
            foreach (var it in currentPlan.intents)
            {
                if (!ActionRegistry.Map.ContainsKey(it.action)) { Log("Unknown action: "+it.action); continue; }
                try
                {
                    string res = ActionRegistry.Map[it.action].Run(it.args, dry);
                    Log("[" + it.action + "] " + res);
                }
                catch (Exception ex) { Log("[" + it.action + "] ERROR: " + ex.Message); }
            }
        }

        string[] SplitCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return new string[0];
            var parts = s.Split(',');
            for (int i=0;i<parts.Length;i++) parts[i] = parts[i].Trim();
            return parts;
        }
    }
}
#endif
