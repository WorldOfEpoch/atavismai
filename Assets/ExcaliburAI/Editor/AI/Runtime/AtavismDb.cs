#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using MySqlConnector;

namespace ExcaliburAI.AI
{
    public static class AtavismDb
    {
        static string cfgPath => Path.Combine(Application.dataPath, "ExcaliburAI/Config/db.json");

        [Serializable] class DbCfg { public string server="localhost"; public int port=3306; public string database="atavism"; public string user="root"; public string password=""; }

        static DbCfg LoadCfg()
        {
            try
            {
                if (!File.Exists(cfgPath)) File.WriteAllText(cfgPath, JsonUtility.ToJson(new DbCfg(), true));
                return JsonUtility.FromJson<DbCfg>(File.ReadAllText(cfgPath));
            }
            catch { return new DbCfg(); }
        }

        static string ConnStr()
        {
            var c = LoadCfg();
            return $"Server={c.server};Port={c.port};Database={c.database};User ID={c.user};Password={c.password};SslMode=None;Allow User Variables=true;Convert Zero Datetime=true;";
        }

        public static int Exec(string sql, Dictionary<string, object> p = null)
        {
            using (var conn = new MySqlConnection(ConnStr()))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (p != null) foreach (var kv in p) cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static void BulkInsert(string table, List<Dictionary<string, object>> rows)
        {
            if (rows == null || rows.Count == 0) return;
            // naive multi-value insert for small batches
            var cols = new List<string>(rows[0].Keys);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("INSERT INTO `" + table + "` (`" + string.Join("`,`", cols.ToArray()) + "`) VALUES ");
            for (int i=0;i<rows.Count;i++)
            {
                sb.Append("(");
                for (int j=0;j<cols.Count;j++)
                {
                    var v = rows[i][cols[j]];
                    if (v == null) sb.Append("NULL");
                    else sb.Append("'" + MySqlHelper.EscapeString(v.ToString()) + "'");
                    if (j<cols.Count-1) sb.Append(",");
                }
                sb.Append(")");
                if (i<rows.Count-1) sb.Append(",");
            }
            Exec(sb.ToString(), null);
        }
    }
}
#endif
