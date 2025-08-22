#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using MySqlConnector;

namespace ExcaliburAI.AI
{
    public static class AtavismDbExt
    {
        public static List<Dictionary<string,object>> QueryRows(string sql)
        {
            var res = new List<Dictionary<string,object>>();
            using (var conn = new MySqlConnection(GetConnStr()))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            var row = new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase);
                            for (int i=0;i<rd.FieldCount;i++) row[rd.GetName(i)] = rd.IsDBNull(i)? null : rd.GetValue(i);
                            res.Add(row);
                        }
                    }
                }
            }
            return res;
        }

        public static string SnapshotSummary()
        {
            try
            {
                var classesT = StudioConfig.TableName("classes");
                var itemsT   = StudioConfig.TableName("items");
                var racesT   = StudioConfig.TableName("races");
                var classes = SafeCount($"SELECT COUNT(*) FROM `{classesT}`");
                var items   = SafeCount($"SELECT COUNT(*) FROM `{itemsT}`");
                var races   = SafeCount($"SELECT COUNT(*) FROM `{racesT}`");
                var topClasses = QueryNames($"SELECT name FROM `{classesT}` LIMIT 20");
                var topRaces   = QueryNames($"SELECT name FROM `{racesT}` LIMIT 20");
                return $"Classes: {classes} [{string.Join(", ", topClasses)}]\n" +
                       $"Races: {races} [{string.Join(", ", topRaces)}]\n" +
                       $"Items: {items}";
            } catch (Exception ex) { return "(snapshot failed: " + ex.Message + ")"; }
        }

        static int SafeCount(string sql)
        {
            using (var conn = new MySqlConnection(GetConnStr()))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    var o = cmd.ExecuteScalar();
                    int n=0; try{ n = Convert.ToInt32(o); } catch {}
                    return n;
                }
            }
        }

        static List<string> QueryNames(string sql)
        {
            var list = new List<string>();
            using (var conn = new MySqlConnection(GetConnStr()))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read()) list.Add(Convert.ToString(rd[0]));
                    }
                }
            }
            return list;
        }

        static string GetConnStr()
        {
            // reuse AtavismDb private methods via reflection or duplicate logic
            var t = typeof(AtavismDb);
            var m = t.GetMethod("Exec");
            // Build the same connection string as AtavismDb.ConnStr() indirectly
            // Simpler: call the private 'ConnStr' via reflection if exists
            var mi = t.GetMethod("ConnStr", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            if (mi != null) return (string)mi.Invoke(null, null);
            // Fallback: replicate default
            return "Server=localhost;Port=3306;Database=atavism;User ID=root;Password=;SslMode=None;Allow User Variables=true;Convert Zero Datetime=true;";
        }
    }
}
#endif
