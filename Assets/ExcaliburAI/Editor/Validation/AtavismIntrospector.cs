
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using UnityEditor;

/// <summary>
/// Auto-tailors content_map.json by reading actual Atavism DB schemas listed in Config/db.env.
/// Respects MYSQL_HOST/PORT/USER/PASS and DB names from MYSQL_DB_* and optional MYSQL_DB_LIST.
/// </summary>
public static class AtavismIntrospector
{
    class Col
    {
        public string Name;
        public string Type;
        public bool IsPk;
    }
    class Tbl
    {
        public string Schema;
        public string Name;
        public List<Col> Cols = new List<Col>();
    }

    static async Task<List<Tbl>> ReadSchemaAsync(string serverConnStr, IEnumerable<string> dbs)
    {
        var list = new List<Tbl>();
        await using (var conn = new MySqlConnection(serverConnStr))
        {
            await conn.OpenAsync();

            // Fetch existing schema names
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmdSchemas = new MySqlCommand("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA", conn))
            using (var r = await cmdSchemas.ExecuteReaderAsync())
            {
                while (await r.ReadAsync()) existing.Add(r.GetString(0));
            }

            var dbList = dbs.Where(s => existing.Contains(s)).ToArray();

            foreach (var db in dbList)
            {
                // Tables
                var tables = new List<string>();
                using (var getTbls = new MySqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=@db", conn))
                {
                    getTbls.Parameters.AddWithValue("@db", db);
                    using (var r = await getTbls.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync()) tables.Add(r.GetString(0));
                    }
                }

                // Columns per table
                foreach (var t in tables)
                {
                    var tbl = new Tbl { Schema = db, Name = t };
                    using (var getCols = new MySqlCommand(
                        "SELECT COLUMN_NAME, DATA_TYPE, COLUMN_KEY " +
                        "FROM INFORMATION_SCHEMA.COLUMNS " +
                        "WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t " +
                        "ORDER BY ORDINAL_POSITION", conn))
                    {
                        getCols.Parameters.AddWithValue("@db", db);
                        getCols.Parameters.AddWithValue("@t", t);
                        using (var rc = await getCols.ExecuteReaderAsync())
                        {
                            while (await rc.ReadAsync())
                            {
                                string colKey = rc.IsDBNull(2) ? string.Empty : rc.GetString(2);
                                bool isPk = colKey.IndexOf("PRI", StringComparison.OrdinalIgnoreCase) >= 0;
                                tbl.Cols.Add(new Col
                                {
                                    Name = rc.GetString(0),
                                    Type = rc.GetString(1),
                                    IsPk = isPk
                                });
                            }
                        }
                    }
                    list.Add(tbl);
                }
            }
        }
        return list;
    }

    static Tbl Best(IEnumerable<Tbl> tbls, Func<Tbl, int> score)
    {
        Tbl best = null;
        int bestScore = int.MinValue;
        foreach (var t in tbls)
        {
            int s = score(t);
            if (s > bestScore) { bestScore = s; best = t; }
        }
        return best;
    }

    static int HasCols(Tbl t, params string[] cols)
    {
        var names = new HashSet<string>(t.Cols.Select(c => c.Name.ToLowerInvariant()));
        int c = 0;
        foreach (var col in cols)
        {
            if (names.Contains(col.ToLowerInvariant())) c++;
        }
        return c;
    }

    static string PickCol(Tbl t, params string[] names)
    {
        foreach (var n in names)
        {
            var c = t.Cols.FirstOrDefault(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase));
            if (c != null) return c.Name;
        }
        foreach (var n in names)
        {
            var c = t.Cols.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(n.ToLowerInvariant()));
            if (c != null) return c.Name;
        }
        return null;
    }

    static JObject MapForItems(Tbl t)
    {
        return new JObject
        {
            ["table"] = string.Format("{0}.{1}", t.Schema, t.Name),
            ["keys"] = new JArray(t.Cols.Where(c => c.IsPk).Select(c => c.Name)),
            ["columns"] = new JObject
            {
                ["id"] = PickCol(t, "id", "item_id"),
                ["name"] = PickCol(t, "name", "internal_name", "item_name"),
                ["displayName"] = PickCol(t, "display_name", "displayname", "label", "title"),
                ["rarity"] = PickCol(t, "rarity", "quality", "item_quality"),
                ["levelReq"] = PickCol(t, "level_req", "level", "req_level", "required_level"),
                ["itemType"] = PickCol(t, "item_type", "type", "category"),
                ["icon"] = PickCol(t, "icon", "icon_name", "icon_path"),
                ["sellable"] = PickCol(t, "sellable", "is_sellable", "can_sell"),
                ["vendorId"] = PickCol(t, "vendor_id", "merchant_id"),
                ["contentUid"] = PickCol(t, "content_uid", "uid", "guid", "ext_id")
            }
        };
    }

    static JObject MapForDrops(Tbl t)
    {
        return new JObject
        {
            ["table"] = string.Format("{0}.{1}", t.Schema, t.Name),
            ["columns"] = new JObject
            {
                ["tableId"] = PickCol(t, "loot_table_id", "table_id", "lt_id"),
                ["itemId"] = PickCol(t, "item_id", "itm_id", "loot_item_id"),
                ["chance"] = PickCol(t, "chance", "probability", "drop_chance", "pct"),
                ["min"] = PickCol(t, "min_qty", "min", "qty_min"),
                ["max"] = PickCol(t, "max_qty", "max", "qty_max")
            }
        };
    }

    static JObject MapForVendors(Tbl t)
    {
        return new JObject
        {
            ["table"] = string.Format("{0}.{1}", t.Schema, t.Name),
            ["columns"] = new JObject
            {
                ["vendorId"] = PickCol(t, "vendor_id", "merchant_id", "shop_id"),
                ["itemId"] = PickCol(t, "item_id"),
                ["price"] = PickCol(t, "price", "cost"),
                ["currency"] = PickCol(t, "currency", "currency_id")
            }
        };
    }

    static JObject MapForSkills(Tbl t)
    {
        return new JObject
        {
            ["table"] = string.Format("{0}.{1}", t.Schema, t.Name),
            ["keys"] = new JArray(t.Cols.Where(c => c.IsPk).Select(c => c.Name)),
            ["columns"] = new JObject
            {
                ["id"] = PickCol(t, "id", "skill_id", "ability_id"),
                ["name"] = PickCol(t, "name", "internal_name"),
                ["displayName"] = PickCol(t, "display_name", "label", "title"),
                ["icon"] = PickCol(t, "icon", "icon_name", "icon_path"),
                ["school"] = PickCol(t, "school", "category"),
                ["contentUid"] = PickCol(t, "content_uid", "uid", "guid", "ext_id")
            }
        };
    }

    static JObject MapForQuests(Tbl tQ, Tbl tObj, Tbl tRew)
    {
        Func<Tbl, string[], string> pick = (tbl, names) =>
        {
            foreach (var n in names)
            {
                var c = tbl.Cols.FirstOrDefault(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase));
                if (c != null) return c.Name;
            }
            foreach (var n in names)
            {
                var c = tbl.Cols.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(n.ToLowerInvariant()));
                if (c != null) return c.Name;
            }
            return null;
        };

        return new JObject
        {
            ["quests"] = new JObject
            {
                ["table"] = string.Format("{0}.{1}", tQ.Schema, tQ.Name),
                ["columns"] = new JObject
                {
                    ["id"] = pick(tQ, new[] { "id", "quest_id" }),
                    ["name"] = pick(tQ, new[] { "name" }),
                    ["displayName"] = pick(tQ, new[] { "display_name", "title" }),
                    ["giverNpcId"] = pick(tQ, new[] { "giver_npc_id", "npc_id" }),
                    ["reqLevel"] = pick(tQ, new[] { "required_level", "req_level" }),
                    ["contentUid"] = pick(tQ, new[] { "content_uid", "uid", "guid", "ext_id" })
                }
            },
            ["objectives"] = new JObject
            {
                ["table"] = string.Format("{0}.{1}", tObj.Schema, tObj.Name),
                ["columns"] = new JObject
                {
                    ["questId"] = pick(tObj, new[] { "quest_id", "q_id" }),
                    ["kind"] = pick(tObj, new[] { "type", "kind" }),
                    ["targetId"] = pick(tObj, new[] { "target_id", "mob_id", "item_id" }),
                    ["count"] = pick(tObj, new[] { "count", "qty" })
                }
            },
            ["rewards"] = new JObject
            {
                ["table"] = string.Format("{0}.{1}", tRew.Schema, tRew.Name),
                ["columns"] = new JObject
                {
                    ["questId"] = pick(tRew, new[] { "quest_id", "q_id" }),
                    ["itemId"] = pick(tRew, new[] { "item_id" }),
                    ["xp"] = pick(tRew, new[] { "xp", "experience" }),
                    ["currency"] = pick(tRew, new[] { "currency", "currency_id" }),
                    ["amount"] = pick(tRew, new[] { "amount", "value" })
                }
            }
        };
    }

    [MenuItem("ExcaliburAI/Atavism/Auto-tailor Mapping")]
    public static async void BuildAndWriteMappingMenu()
    {
        try
        {
            string result = await BuildAndWriteMapping();
            EditorUtility.DisplayDialog("ExcaliburAI", result, "OK");
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("ExcaliburAI", "Error: " + ex.Message, "OK");
        }
    }

    public static async Task<string> BuildAndWriteMapping(string envPath = "Config/db.env")
    {
        if (!File.Exists(envPath)) throw new FileNotFoundException("Missing " + envPath);
        var cfg = Env.LoadDb(envPath);
        var dbs = Env.GetAllDbNames(envPath);
        var serverConnStr = Env.ConnStrServer(cfg, envPath);

        var all = await ReadSchemaAsync(serverConnStr, dbs);

        // Heuristic selection of key tables
        var candidatesItems = all.Where(t => t.Name.ToLowerInvariant().Contains("item") && !t.Name.ToLowerInvariant().Contains("stat")).ToList();
        var itemTbl = Best(candidatesItems, t => HasCols(t, "name") + HasCols(t, "icon") + HasCols(t, "display_name") + HasCols(t, "rarity", "quality") + HasCols(t, "item_type"));

        var dropTbl = Best(all.Where(t => {
            var n = t.Name.ToLowerInvariant();
            return n.Contains("loot") || n.Contains("drop");
        }), t => HasCols(t, "item_id") + HasCols(t, "chance"));

        var vendTbl = Best(all.Where(t => {
            var n = t.Name.ToLowerInvariant();
            return n.Contains("vendor") || n.Contains("merchant") || n.Contains("shop");
        }), t => HasCols(t, "item_id") + HasCols(t, "price", "cost"));

        var skillTbl = Best(all.Where(t => {
            var n = t.Name.ToLowerInvariant();
            return n.Contains("skill") || n.Contains("ability");
        }), t => HasCols(t, "name") + HasCols(t, "icon"));

        // Quests (exclude objective/reward tables)
        var qTbl = Best(all.Where(t => {
            var n = t.Name.ToLowerInvariant();
            return n.Contains("quest") && !n.Contains("objective") && !n.Contains("reward");
        }), t => HasCols(t, "name") + HasCols(t, "display_name", "title"));

        var qObjTbl = Best(all.Where(t => t.Name.ToLowerInvariant().Contains("objective")), t => HasCols(t, "quest_id") + HasCols(t, "type", "kind"));
        var qRewTbl = Best(all.Where(t => t.Name.ToLowerInvariant().Contains("reward")), t => HasCols(t, "quest_id"));

        var mobTbl = Best(all.Where(t => {
            var n = t.Name.ToLowerInvariant();
            return n.Contains("mob") || n.Contains("npc");
        }), t => HasCols(t, "name"));

        var root = new JObject();
        if (itemTbl != null) root["items"] = MapForItems(itemTbl);
        if (dropTbl != null) root["drops"] = MapForDrops(dropTbl);
        if (vendTbl != null) root["vendors"] = MapForVendors(vendTbl);
        if (skillTbl != null) root["skills"] = MapForSkills(skillTbl);
        if (qTbl != null && qObjTbl != null && qRewTbl != null) root["quests"] = MapForQuests(qTbl, qObjTbl, qRewTbl);
        if (mobTbl != null)
        {
            string nameCol = null;
            var nameExact = mobTbl.Cols.FirstOrDefault(c => string.Equals(c.Name, "name", StringComparison.OrdinalIgnoreCase));
            if (nameExact != null) nameCol = nameExact.Name;
            else if (mobTbl.Cols.Count > 0) nameCol = mobTbl.Cols[0].Name;

            root["mobs"] = new JObject
            {
                ["table"] = string.Format("{0}.{1}", mobTbl.Schema, mobTbl.Name),
                ["columns"] = new JObject
                {
                    ["id"] = mobTbl.Cols.FirstOrDefault(c => c.IsPk) != null ? mobTbl.Cols.FirstOrDefault(c => c.IsPk).Name : null,
                    ["name"] = nameCol
                }
            };
        }

        Directory.CreateDirectory("Assets/ExcaliburAI/Staging");
        File.WriteAllText("Config/content_map.json", root.ToString());
        var sb = new StringBuilder();
        sb.AppendLine("# Atavism Schema Report");
        foreach (var g in all.GroupBy(t => t.Schema))
        {
            sb.AppendLine();
            sb.AppendLine("## Schema `" + g.Key + "`");
            foreach (var t in g)
            {
                sb.AppendLine("- **" + t.Name + "**");
                foreach (var c in t.Cols)
                    sb.AppendLine("  - " + c.Name + " : " + c.Type + (c.IsPk ? " (PK)" : ""));
            }
        }
        File.WriteAllText("Assets/ExcaliburAI/Staging/schema_report.md", sb.ToString());
        AssetDatabase.Refresh();

        return "Mapping written to Config/content_map.json. Also wrote schema_report.md for review.";
    }
}
