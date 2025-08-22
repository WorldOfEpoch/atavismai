#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MySqlConnector;
using System.Threading.Tasks;
using System.Text;

/// <summary>
/// Editor window to edit Atavism DB settings stored in Config/db.env and test connectivity.
/// </summary>
public class ExcaliburDbSettingsWindow : EditorWindow
{
    private Env.DbConfig cfg;
    private bool showPass = false;
    private Vector2 scroll;
    private readonly StringBuilder log = new StringBuilder();

    [MenuItem("ExcaliburAI/Atavism/Database Settings")]
    public static void Open()
    {
        var wnd = GetWindow<ExcaliburDbSettingsWindow>("Atavism DB Settings");
        wnd.minSize = new Vector2(520, 420);
    }

    void OnEnable()
    {
        cfg = Env.LoadDb();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Atavism Database (MySQL/MariaDB)", EditorStyles.boldLabel);
        EditorGUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        cfg.Host = EditorGUILayout.TextField("Host", cfg.Host);
        cfg.Port = EditorGUILayout.IntField("Port", cfg.Port);
        EditorGUILayout.EndHorizontal();

        cfg.User = EditorGUILayout.TextField("User", cfg.User);

        EditorGUILayout.BeginHorizontal();
        if (showPass) cfg.Pass = EditorGUILayout.TextField("Password", cfg.Pass);
        else cfg.Pass = EditorGUILayout.PasswordField("Password", cfg.Pass);
        showPass = GUILayout.Toggle(showPass, showPass ? "Hide" : "Show", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Database Names (must match your Atavism install):", EditorStyles.miniBoldLabel);
        cfg.DB_Admin    = EditorGUILayout.TextField("Admin DB", cfg.DB_Admin);
        cfg.DB_Atavism  = EditorGUILayout.TextField("Atavism DB", cfg.DB_Atavism);
        cfg.DB_Master   = EditorGUILayout.TextField("Master DB", cfg.DB_Master);
        cfg.DB_World    = EditorGUILayout.TextField("World Content DB", cfg.DB_World);

        EditorGUILayout.Space(12);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save to Config/db.env", GUILayout.Height(28)))
        {
            Env.SaveDb(cfg);
            Log("Saved Config/db.env");
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Test All", GUILayout.Height(28)))
        {
            _ = TestAllAsync();
        }
        if (GUILayout.Button("Test Atavism", GUILayout.Height(28)))
        {
            _ = TestOneAsync(cfg.DB_Atavism);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(log.ToString(), GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    async Task TestAllAsync()
    {
        await TestOneAsync(cfg.DB_Admin);
        await TestOneAsync(cfg.DB_Atavism);
        await TestOneAsync(cfg.DB_Master);
        await TestOneAsync(cfg.DB_World);
    }

    async Task TestOneAsync(string db)
    {
        try
        {
            var connStr = Env.ConnStr(db, cfg);
            using (var conn = new MySqlConnection(connStr))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand("SELECT DATABASE();", conn))
                {
                    var r = await cmd.ExecuteScalarAsync();
                    Log($"OK: {db} (SELECT DATABASE() -> {r})");
                }
            }
        }
        catch (System.Exception ex)
        {
            Log($"ERROR: {db} -> {ex.Message}");
        }
    }

    void Log(string msg)
    {
        log.AppendLine(System.DateTime.Now.ToString("HH:mm:ss") + "  " + msg);
        Repaint();
    }
}
#endif
