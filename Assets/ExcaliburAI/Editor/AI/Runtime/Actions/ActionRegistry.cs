#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public interface IStudioAction
    {
        string Name { get; }
        string Run(Dictionary<string,string> args, bool dryRun);
    }

    public static class ActionRegistry
    {
        static Dictionary<string, IStudioAction> map = null;
        public static Dictionary<string, IStudioAction> Map
        {
            get
            {
                if (map != null) return map;
                map = new Dictionary<string, IStudioAction>(StringComparer.OrdinalIgnoreCase);
                Register(new GenerateClassesAction());
                Register(new GenerateItemsAction());
                Register(new GenerateItemsFromCSVAction());
                Register(new AssignEconomyAction());
                Register(new GenerateIconsAction());
                Register(new BuildLoreGraphAction());
                Register(new GenerateNPCsAction());
                Register(new GenerateDialoguesAction());
                return map;
            }
        }
        static void Register(IStudioAction a) { Map[a.Name]=a; }
    }
}
#endif
