
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public static class PlannerUtil
    {
        /// <summary>
        /// Extract the largest JSON object from a response. Handles fenced code blocks (```json ... ```).
        /// </summary>
        public static string ExtractJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = s.Trim();

            // Strip code fences if present
            if (s.StartsWith("```", StringComparison.Ordinal))
            {
                int firstNewline = s.IndexOf('\n');
                if (firstNewline >= 0 && firstNewline + 1 < s.Length)
                    s = s.Substring(firstNewline + 1);

                int lastFence = s.LastIndexOf("```", StringComparison.Ordinal);
                if (lastFence >= 0)
                    s = s.Substring(0, lastFence);
            }

            // Greedy JSON slice between first '{' and last '}'
            int a = s.IndexOf('{');
            int b = s.LastIndexOf('}');
            if (a >= 0 && b > a)
                return s.Substring(a, b - a + 1);

            return null;
        }

        /// <summary>
        /// Lightweight "command mode": ActionName(key=value, key2=value2) -> Plan with one intent.
        /// </summary>
        public static Plan TryParseCommand(string user)
        {
            if (string.IsNullOrWhiteSpace(user)) return null;

            var m = Regex.Match(user, @"^\s*([A-Za-z_][A-Za-z_0-9]*)\s*\((.*)\)\s*$");
            if (!m.Success) return null;

            string action = m.Groups[1].Value;
            string argsStr = m.Groups[2].Value;

            var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in argsStr.Split(','))
            {
                var kv = part.Split(new[] { '=' }, 2);
                if (kv.Length == 2)
                {
                    string key = kv[0].Trim();
                    string val = kv[1].Trim();
                    if (key.Length > 0) args[key] = val;
                }
            }

            var p = new Plan { dryRun = true };
            p.intents.Add(new PlanIntent { action = action, args = args });
            return p;
        }
    }
}
#endif
