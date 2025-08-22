#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace ExcaliburAI.AI
{
    [Serializable]
    public class ProjectProfileModel
    {
        public string projectName = "World of Epoch";
        public string genre = "High Fantasy";
        public string[] allowedRaces = new string[]{"Human","Elf","Orc","Dwarf"};
        public string[] allowedClasses = new string[]{}; // leave empty to allow new ones
        public string tone = "Heroic, cohesive, PG-13, lore-driven";
        public string namingStyle = "Concise, evocative, no memes.";
        public bool restrictToAllowedRaces = true;
        public bool requireDBAwareness = true;
        public string worldSummary = "Ancient forges, frontier realms, factions vying for power.";
    }

    public static class ProjectProfile
    {
        public static string PathFile
        {
            get
            {
#if UNITY_EDITOR
                return System.IO.Path.Combine(Application.dataPath, "ExcaliburAI/Config/project_profile.json");
#else
                return System.IO.Path.Combine(Application.persistentDataPath, "ExcaliburAI/Config/project_profile.json");
#endif
            }
        }

        public static ProjectProfileModel Load()
        {
            try
            {
                if (!File.Exists(PathFile))
                {
                    var def = new ProjectProfileModel();
                    Save(def);
                    return def;
                }
                return JsonUtility.FromJson<ProjectProfileModel>(File.ReadAllText(PathFile));
            }
            catch { return new ProjectProfileModel(); }
        }

        public static void Save(ProjectProfileModel m)
        {
            var dir = System.IO.Path.GetDirectoryName(PathFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(PathFile, JsonUtility.ToJson(m, true));
        }
    }
}
#endif
