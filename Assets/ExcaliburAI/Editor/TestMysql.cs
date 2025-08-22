#if UNITY_EDITOR
using UnityEditor;
using System.Threading.Tasks;
using MySqlConnector;

public static class ExcaliburDiagnostics
{
    [MenuItem("ExcaliburAI/Diagnostics/Test MySQL Connection")]
    public static async void TestMysqlConnection()
    {
        try
        {
            var connStr = Env.GetMySqlConnString("Config/db.env");
            await using var conn = new MySqlConnection(connStr);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand("SELECT 1", conn);
            var result = await cmd.ExecuteScalarAsync();
            EditorUtility.DisplayDialog("ExcaliburAI", "MySQL OK. SELECT 1 -> " + result, "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("ExcaliburAI", "MySQL ERROR:\n" + ex.Message, "OK");
        }
    }
}
#endif
