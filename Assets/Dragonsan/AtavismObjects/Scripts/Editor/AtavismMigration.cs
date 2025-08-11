using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Atavism
{
    public class AtavismMigration : EditorWindow
    {

         // [MenuItem("Window/Atavism/Atavism Migration Database from X.8 to X.9")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AtavismMigration));
        }

        Vector2 scrollPos;

        public void OnGUI()
        {
            GUILayout.Label("");
            if (GUILayout.Button("Migrate"))
            {
                if (DatabasePack.TestConnection(DatabasePack.contentDatabasePrefix, true))
                {
                    if (EditorUtility.DisplayDialog("",
                        Lang.GetTranslate("Are you sure you want to migrate data to version Atavism X.9") + " ?",
                        Lang.GetTranslate("OK"), Lang.GetTranslate("Cancel")))
                    {
                        List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
                        string query = "SELECT * FROM `server_version` ";
                        rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
                        if ((rows != null) && (rows.Count > 0))
                        {
                            foreach (Dictionary<string, string> data in rows)
                            {
                                string sql4 = "UPDATE `abilities` SET `damageType`='"+data["name"]+"' WHERE damageType = ''";
                                DatabasePack.ExecuteNonQuery(DatabasePack.contentDatabasePrefix, sql4, 600);
                            }
                        }
                        
                        
                        
                        query = "SELECT * FROM abilities  WHERE abilityType ='AtackAbility'";
                        rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
                        if ((rows != null) && (rows.Count == 0))
                        {
                            Run();
                        }
                        else if ((rows != null) && (rows.Count > 0))
                        {
                            if (EditorUtility.DisplayDialog("",
                                Lang.GetTranslate("Data has been detected in one of the new tables are you sure you want to start data migration") + " ?", Lang.GetTranslate("OK"), Lang.GetTranslate("Cancel")))
                            {
                                Run();
                            }
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("", "Setup the connection to the database in the editor ",
                        Lang.GetTranslate("OK"), "");
                }
            }
        }


       // [MenuItem("Window/Atavism/Atavism Migration to X.5")]
       [MenuItem("Window/Atavism/Atavism Migration Database from X.8 to X.9")]
        public static void Migrate()
        {
            if (DatabasePack.TestConnection(DatabasePack.contentDatabasePrefix, true))
            {
                if (EditorUtility.DisplayDialog("Atavism migration data to version X.9",
                    Lang.GetTranslate("Are you sure you want to migrate data to version Atavism X.9") + " ?",
                    Lang.GetTranslate("OK"), Lang.GetTranslate("Cancel")))
                {
                    List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
                    string query = "SELECT * FROM ability_effects";
                    rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
                    if ((rows != null) && (rows.Count == 0))
                    {
                        Run();
                    }
                    else if ((rows != null) && (rows.Count > 0))
                    {
                        if (EditorUtility.DisplayDialog("Atavism migration data to version X.9",
                            Lang.GetTranslate(
                                "Data has been detected in one of the new tables are you sure you want to start data migration") +
                            " ?", Lang.GetTranslate("OK"), Lang.GetTranslate("Cancel")))
                        {
                            Run();
                        }
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Atavism migration data to version X.9",
                    "Setup the connection to the basebase in the Atavism Editor", Lang.GetTranslate("OK"), "");
            }
        }

        static void Run()
        {
            
            //Abilities
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            string query1 = "SELECT * FROM abilities ";
            if (rows != null)
                rows.Clear();
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query1);
            
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                  
                    
                    string query2 = "SELECT * FROM abilities_powerup_settings where  ability_id = "+ data["id"];
                    List<Dictionary<string, string>> rows2 = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query2);
                    int pu_id = -1;
                    if (rows2.Count > 0)
                    {
                        Dictionary<string, string> purow = rows2[0];
                        pu_id = int.Parse(purow["id"]);
                    }
                    else
                    {
                        string sql1 = "INSERT INTO `abilities_powerup_settings` (`id`, `ability_id`, `thresholdMaxTime`) VALUES(null, " + data["id"] + ", 0)";
                        pu_id = DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql1, new List<Register>());
                    }

                    for (int i = 1; i <= 6; i++)
                    {
                        int effectId = int.Parse(data["activationEffect" + i]);
                        string target = data["activationTarget" + i];
                        if (effectId > 0 && !string.IsNullOrEmpty(target))
                        {
                            string sql2 = "INSERT INTO `ability_effects` (`id`, `ability_power_id`, `target`, `effect`, `delay`, `chance_min`, `chance_max`) VALUES(null, " + pu_id + ", '"+target+"', "+effectId+",0,100,100)";
                            DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql2, new List<Register>());
                        }
                    }
                    for (int i = 1; i <= 5; i++)
                    {
                        string _effect = data["coordEffect" + i];
                        string _event = data["coordEffect" + i+"event"];
                        if (!string.IsNullOrEmpty(_event) && !string.IsNullOrEmpty(_effect) && !_effect.Equals("~ none ~") )
                        {
                            string sql3 = "INSERT INTO `abilities_coordeffects` (`id`, `ability_power_id`, `coordEffectEvent`, `coordEffect`) VALUES(null, " + pu_id + ", '"+_event+"', '"+_effect+"')";
                            DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql3, new List<Register>());
                        }
                    }
                    // string targetType_org = data["TargetType"];
                    // string targetType = "Single Target";
                    // string targetSubType = "Enemy";
                    // switch (targetType_org)
                    // {
                    //     case "Self":
                    //         targetType = "Single Target";
                    //         targetSubType = "Self";
                    //         break;    
                    //     case "Enemy":
                    //         targetType = "Single Target";
                    //         targetSubType = "Enemy";
                    //         break;    
                    //     case "Friendly":
                    //         targetType = "Single Target";
                    //         targetSubType = "Friendly";
                    //         break;    
                    //     case "Friend Not Self":
                    //         targetType = "Single Target";
                    //         targetSubType = "Friend Not Self";
                    //         break;    
                    //     case "AoE Friendly":
                    //         targetType = "AoE";
                    //         targetSubType = "Friendly";
                    //         break;    
                    //     case "AoE Enemy":
                    //         targetType = "AoE";
                    //         targetSubType = "Enemy";
                    //         break;    
                    //     
                    // }
                    
                    
                    
                    // string sql2 = "INSERT INTO `mob_behaviors` (`id`, `profile_id`, `behavior_order`, `type`, `flee_type`, `flee_loc_x`, `flee_loc_y`, `flee_loc_z`, `ability_interval`, `mob_tag`, `ignore_chase_distance`, `weapon`, `creationtimestamp`, `updatetimestamp`) VALUES"+
                    //               " (null, "+bhv_profile_id+", 0, 1, -1, NULL, NULL, NULL, 1000, -1, 0, -1,  NOW(),  NOW())";
                    // int bhv_id = DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql2, new List<Register>());
                    //
                    // int order = 0;
                    // for (int i = 0; i < 3; i++)
                    // {
                    //     if (int.Parse(data["ability"+i]) > 0)
                    //     {
                    //         string sql4 ="INSERT INTO `mob_ability` (`id`, `mob_ability_order`, `behavior_id`, `abilities`, `minAbilityRangePercentage`, `maxAbilityRangePercentage`, `mob_ability_type`, `creationtimestamp`, `updatetimestamp`) VALUES"+
                    //                      "(null, "+order+", "+bhv_id+", '"+data["ability"+i]+";1;', 0, 50, 0,  NOW(),  NOW())";
                    //         int abi_id = DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql4, new List<Register>());
                    //
                    //         string sql5 = "INSERT INTO `mob_ability_conditions_group` (`id`, `group_order`, `mob_ability_id`, `creationtimestamp`, `updatetimestamp`) VALUES"+
                    //                      "(null, 0, "+abi_id+",  NOW(),  NOW())";
                    //         int abi_cond_group_id = DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql5, new List<Register>());
                    //
                    //         string sql6 ="INSERT INTO `mob_ability_conditions` (`id`, `conditions_group_id`, `type`, `distance`, `less`, `stat_name`, `stat_value`, `stat_vitality_percentage`, `target`, `effect_tag_id`, `on_target`, `combat_state`, `death_state`, `trigger_event_Id`, `target_number`, `target_ally`, `creationtimestamp`, `updatetimestamp`) VALUES"+
                    //                      "(null, "+abi_cond_group_id+", 2, -1, 1, '"+data["abilityStatReq"+i]+"', "+data["abilityStatPercent"+i]+", 1, 0, -1, 0, 0, 0, -1, -1, 0,  NOW(),  NOW())";
                    //         DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql6, new List<Register>());
                    //         order++;
                    //     }
                    // }

                    // if (int.Parse(data["autoAttack"]) > 0)
                    // {
                    //     string sql7 ="INSERT INTO `mob_ability` (`id`, `mob_ability_order`, `behavior_id`, `abilities`, `minAbilityRangePercentage`, `maxAbilityRangePercentage`, `mob_ability_type`, `creationtimestamp`, `updatetimestamp`) VALUES"+
                    //                  "(null, "+order+", "+bhv_id+", '"+data["autoAttack"]+";1;', 0, 50, 0,  NOW(),  NOW())";
                    //     DatabasePack.Insert(DatabasePack.contentDatabasePrefix, sql7, new List<Register>());
                    //     order++;
                    //     
                    // }
                    // string sql8 = "UPDATE `mob_templates` SET `behavior_profile_id`="+bhv_profile_id+" WHERE id="+data["id"];
                    // DatabasePack.ExecuteNonQuery(DatabasePack.contentDatabasePrefix, sql8, 600);
                    

                }

                string sqlability = "update world_content.abilities set updatetimestamp = now()";
                DatabasePack.ExecuteNonQuery(DatabasePack.contentDatabasePrefix, sqlability, 600);


            }
                
     
            EditorUtility.DisplayDialog("Atavism migration data to version X.7", "Migration was successful",
                Lang.GetTranslate("OK"), "");
            Debug.Log("Migration was successful");

        }

    }
}