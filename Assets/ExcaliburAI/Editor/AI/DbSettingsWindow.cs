#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ExcaliburAI.AI
{
    public class DbSettingsWindow : EditorWindow
    {
        class DbCfg { public string server="localhost"; public int port=3306; public string database="atavism"; public string user="root"; public string password=""; }
        DbCfg cfg;
        string path;

        [MenuItem("ExcaliburAI/DB Settings")]
        public static void Open(){ GetWindow<DbSettingsWindow>("ExcaliburAI DB").minSize=new Vector2(420,220); }

        void OnEnable()
        {
            path = Path.Combine(Application.dataPath, "ExcaliburAI/Config/db.json");
            try{ cfg = JsonUtility.FromJson<DbCfg>(File.ReadAllText(path)); } catch { cfg = new DbCfg(); }
        }

        void OnGUI()
        {
            GUILayout.Label("Database Connection", EditorStyles.boldLabel);
            cfg.server = EditorGUILayout.TextField("Server", cfg.server);
            cfg.port = EditorGUILayout.IntField("Port", cfg.port);
            cfg.database = EditorGUILayout.TextField("Database", cfg.database);
            cfg.user = EditorGUILayout.TextField("User", cfg.user);
            cfg.password = EditorGUILayout.PasswordField("Password", cfg.password);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, JsonUtility.ToJson(cfg, true)); }
            if (GUILayout.Button("Test Connection")) { Test(); }
            if (GUILayout.Button("Snapshot")) { var s = AtavismDbExt.SnapshotSummary(); EditorUtility.DisplayDialog("DB Snapshot", s, "OK"); }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Path: " + path, EditorStyles.miniLabel);
        }

        void Test()
        {
            try
            {
                // cheap test: SELECT 1
                int n = AtavismDb.Exec("SELECT 1");
                EditorUtility.DisplayDialog("DB", "Connected OK.", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("DB Error", ex.Message, "OK");
            }
        }
    }
}
#endif
