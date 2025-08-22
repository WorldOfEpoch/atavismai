#if UNITY_EDITOR
using System.Collections.Generic;

namespace ExcaliburAI.AI
{
    public class DeleteClassAction : IStudioAction
    {
        public string Name => "DeleteClass";

        public string Run(System.Collections.Generic.Dictionary<string,string> args, bool dryRun)
        {
            string name = (args!=null && args.ContainsKey("name"))? args["name"] : "";
            if (string.IsNullOrEmpty(name)) return "DeleteClass requires 'name'.";
            string table = StudioConfig.TableName("classes");
            if (dryRun) return $"Would delete class '{name}' from `{table}`.";
            int n = AtavismDb.Exec($"DELETE FROM `{table}` WHERE name=@n", new System.Collections.Generic.Dictionary<string,object>{{"@n",name}});
            return n>0? $"Deleted class '{name}'." : $"Class '{name}' not found.";
        }
    }
}
#endif
