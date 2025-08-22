
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using UnityEditor;

/// <summary>
/// Fills missing/null columns in Config/content_map.json by inspecting the live DB schema.
/// Only touches fields that are null/empty. Existing values are preserved.
/// </summary>
public static class ContentMapRefiner
{
    [MenuItem("ExcaliburAI/Atavism/Fill Missing Mapping Fields")]
    public static async void Fill()
    {
        string mapPath = "Config/content_map.json";
        if (!File.Exists(mapPath))
        {
            EditorUtility.DisplayDialog("ExcaliburAI", "Missing " + mapPath, "OK");
            return;
        }

        JObject map = JObject.Parse(File.ReadAllText(mapPath));
        var cfg = Env.LoadDb();
        string serverConn = Env.ConnStrServer(cfg);

        try
        {
            using (var conn = new MySqlConnection(serverConn))
            {
                await conn.OpenAsync();

                // Helper: returns set of column names for "schema.table"
                async System.Threading.Tasks.Task<HashSet<string>> GetCols(string fullTable)
                {
                    if (string.IsNullOrWhiteSpace(fullTable) || !fullTable.Contains(".")) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var parts = fullTable.Split('.');
                    string schema = parts[0];
                    string table = parts[1];
                    var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var cmd = new MySqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@s AND TABLE_NAME=@t", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", schema);
                        cmd.Parameters.AddWithValue("@t", table);
                        using (var r = await cmd.ExecuteReaderAsync())
                        {
                            while (await r.ReadAsync()) cols.Add(r.GetString(0));
                        }
                    }
                    return cols;
                }

                string Pick(HashSet<string> cols, params string[] candidates)
                {
                    foreach (var c in candidates)
                        if (cols.Contains(c)) return c;
                    // relaxed contains
                    foreach (var c in candidates)
                    {
                        var hit = cols.FirstOrDefault(x => x.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (hit != null) return hit;
                    }
                    return null;
                }

                void SetIfNull(JObject columns, string key, string value)
                {
                    if (columns[key] == null || columns[key].Type == JTokenType.Null || string.IsNullOrWhiteSpace(columns[key].ToString()))
                    {
                        if (!string.IsNullOrWhiteSpace(value)) columns[key] = value;
                    }
                }

                // Items
                if (map["items"] is JObject items && items["table"] != null)
                {
                    var cols = await GetCols(items.Value<string>("table"));
                    var colmap = (JObject)items["columns"];
                    SetIfNull(colmap, "displayName", Pick(cols, "display_name", "title", "label", "item_name"));
                    SetIfNull(colmap, "levelReq", Pick(cols, "required_level", "req_level", "level_req", "level"));
                    SetIfNull(colmap, "vendorId", Pick(cols, "vendor_id", "merchant_id"));
                    SetIfNull(colmap, "contentUid", Pick(cols, "content_uid", "uid", "guid", "ext_id"));
                }

                // Drops
                if (map["drops"] is JObject drops && drops["table"] != null)
                {
                    var cols = await GetCols(drops.Value<string>("table"));
                    var colmap = (JObject)drops["columns"];
                    SetIfNull(colmap, "itemId", Pick(cols, "item_id", "loot_item_id", "itm_id", "itemTemplate"));
                    SetIfNull(colmap, "min",    Pick(cols, "count_min", "min_qty", "min"));
                }

                // Vendors
                if (map["vendors"] is JObject vendors && vendors["table"] != null)
                {
                    var cols = await GetCols(vendors.Value<string>("table"));
                    var colmap = (JObject)vendors["columns"];
                    SetIfNull(colmap, "vendorId", Pick(cols, "vendor_id", "player_shop_id", "shop_id", "merchant_id"));
                    SetIfNull(colmap, "itemId",   Pick(cols, "item_id", "itemTemplate", "template_id"));
                }

                // Skills
                if (map["skills"] is JObject skills && skills["table"] != null)
                {
                    var cols = await GetCols(skills.Value<string>("table"));
                    var colmap = (JObject)skills["columns"];
                    SetIfNull(colmap, "displayName", Pick(cols, "display_name", "title", "label"));
                    SetIfNull(colmap, "school",      Pick(cols, "school", "category"));
                    SetIfNull(colmap, "contentUid",  Pick(cols, "content_uid", "uid", "guid", "ext_id"));
                }

                // Quests
                if (map["quests"] is JObject quests)
                {
                    if (quests["quests"] is JObject tQ && tQ["table"] != null)
                    {
                        var cols = await GetCols(tQ.Value<string>("table"));
                        var colmap = (JObject)tQ["columns"];
                        SetIfNull(colmap, "displayName", Pick(cols, "display_name", "title", "label"));
                        SetIfNull(colmap, "giverNpcId",  Pick(cols, "giver_npc_id", "npc_id"));
                        SetIfNull(colmap, "reqLevel",    Pick(cols, "required_level", "req_level"));
                        SetIfNull(colmap, "contentUid",  Pick(cols, "content_uid", "uid", "guid", "ext_id"));
                    }
                    if (quests["objectives"] is JObject tO && tO["table"] != null)
                    {
                        var cols = await GetCols(tO.Value<string>("table"));
                        var colmap = (JObject)tO["columns"];
                        SetIfNull(colmap, "questId", Pick(cols, "quest_id", "q_id"));
                        SetIfNull(colmap, "targetId", Pick(cols, "target_id", "mob_id", "item_id"));
                    }
                    if (quests["rewards"] is JObject tR && tR["table"] != null)
                    {
                        var cols = await GetCols(tR.Value<string>("table"));
                        var colmap = (JObject)tR["columns"];
                        // If rewards table seems wrong, try to point it to a better one (quest_rewards)
                        if (!cols.Any())
                        {
                            // Try to detect an alternative quest reward table in the same schema
                            var schema = tR.Value<string>("table").Split('.')[0];
                            using (var cmd = new MySqlCommand(@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=@s AND (TABLE_NAME LIKE '%reward%' OR TABLE_NAME LIKE '%quest_reward%')", conn))
                            {
                                cmd.Parameters.AddWithValue("@s", schema);
                                using (var r = await cmd.ExecuteReaderAsync())
                                {
                                    if (await r.ReadAsync())
                                    {
                                        var newTable = schema + "." + r.GetString(0);
                                        tR["table"] = newTable;
                                        cols = await GetCols(newTable);
                                    }
                                }
                            }
                        }
                        SetIfNull(colmap, "questId", Pick(cols, "quest_id", "q_id"));
                        SetIfNull(colmap, "itemId",  Pick(cols, "item_id", "reward_item_id"));
                        SetIfNull(colmap, "xp",      Pick(cols, "xp", "experience", "reward_xp"));
                        SetIfNull(colmap, "currency",Pick(cols, "currency", "currency_id"));
                        SetIfNull(colmap, "amount",  Pick(cols, "amount", "value", "reward_amount"));
                    }
                }

                // Mobs: usually OK; no changes unless missing
                if (map["mobs"] is JObject mobs && mobs["table"] != null)
                {
                    var cols = await GetCols(mobs.Value<string>("table"));
                    var colmap = (JObject)mobs["columns"];
                    SetIfNull(colmap, "id",   Pick(cols, "id", "mob_id", "npc_id"));
                    SetIfNull(colmap, "name", Pick(cols, "name", "display_name", "label"));
                }
            }

            // Save if changed
            File.WriteAllText(mapPath, map.ToString());
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("ExcaliburAI", "Filled missing mapping fields.\nCheck Config/content_map.json.", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("ExcaliburAI", "Refiner error: " + ex.Message, "OK");
        }
    }
}
#endif
