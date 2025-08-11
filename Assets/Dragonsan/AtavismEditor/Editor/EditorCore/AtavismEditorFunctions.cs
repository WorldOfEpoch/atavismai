using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Atavism.UIEditor
{
    public class AtavismEditorFunctions : MonoBehaviour
    {

        public static int[] effectIds = new int[] { -1 };
        public static string[] effectOptions = new string[] { "~ none ~" };
        public static GUIContent[] GuiEffectOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] questIds = new int[] { -1 };
        public static string[] questOptions = new string[] { "~ none ~" };
        public static GUIContent[] GuiQuestOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] currencyIds = new int[] { -1 };
        public static string[] currencyOptions = new string[] { "~ none ~" };
        public static GUIContent[] GuiCurrencyOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] interactiveProfileIds = new int[] { -1 };
        public static GUIContent[] interactiveProfileOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] itemIds = new int[] { -1 };
        public static string[] itemsList = null;
        public static GUIContent[] GuiItemsList = new GUIContent[] { new GUIContent("~ none ~") };

        public static GUIContent[] RNProfileList = new GUIContent[] { new GUIContent("~ none ~") };
        public static int[] RNProfileIds = new int[] { -1 };

        public static int[] taskIds = new int[] { -1 };
        public static string[] taskOptions = new string[] { "~ none ~" };
        public static GUIContent[] GuiTaskOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] instanceIds = new int[] { 1 };
        public static string[] instanceList = new string[] { "~ none ~" };
        public static GUIContent[] GuiInstanceList = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] GuiInstanceSpawnIds = new int[] { 1 };
        public static GUIContent[] GuiInstanceSpawnList = new GUIContent[] { new GUIContent("~ none ~") };
        public static Vector3[] GuiInstanceSpawnLoc = new Vector3[] { Vector3.zero };

        public static int[] instanceWeatherProfileIds = new int[] { 1 };
        public static string[] instanceWeatherProfileList = new string[] { "~ none ~" };

        public static int[] claimProfileIds = new int[] { -1 };
        public static GUIContent[] claimProfileOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] mobIds = new int[] { -1 };
        public static string[] mobOptions = new string[] { "~ none ~" };
        public static GUIContent[] GuiMobOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] dialogueIds = new int[] { -1 };
        public static string[] dialogueList = new string[] { "~ none ~" };
        public static GUIContent[] GuiDialogueList = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] factionIds = new int[] { -1 };
        public static string[] factionOptions = new string[] { "~ none ~" };
        public static GUIContent[] GuiFactionOptions = new GUIContent[] { new GUIContent("~ none ~") };

        public static int[] merchantTableIds = new int[] { -1 };
        public static string[] merchantTableList = new string[] { "~ none ~" };
        public static GUIContent[] GuiMerchantTableList = new GUIContent[] { new GUIContent("~ none ~") };

        public static int GetFilteredListSelector(GUIContent content, ref string name, int selected, GUIContent[] list,
            int[] ids)
        {
            string wSearched = name.ToLower();
            List<int> itList = new List<int>();
            List<GUIContent> ItList = new List<GUIContent>();
            int wSelectedinSublist = 0;
            List<GUIContent> wFilteredItems = new List<GUIContent>(list);
            if (name.Length > 1)
            {
                int i = 0;
                foreach (GUIContent c in list)
                {
                    if (c.text.ToLower().Contains(wSearched))
                    {
                        itList.Add(ids[i]);
                        ItList.Add(c);
                    }

                    i++;
                }

                wSelectedinSublist =
                    (int)EditorGUILayout.IntPopup(content, selected, ItList.ToArray(), itList.ToArray());
            }
            else
            {
                wSelectedinSublist = (int)EditorGUILayout.IntPopup(content, selected, list, ids);
            }

            return wSelectedinSublist;
        }

        #region Effects

        public static void LoadEffectOptions()
        {
            // Read all entries from the table
            string query = "SELECT id, name FROM effects where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                effectOptions = new string[rows.Count + 1];
                effectOptions[optionsId] = "~ none ~";
                effectIds = new int[rows.Count + 1];
                effectIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    effectOptions[optionsId] = data["id"] + ":" + data["name"];
                    effectIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadEffectOptions(bool gui)
        {
            if (!gui)
            {
                LoadEffectOptions();
                return;
            }

            // Read all entries from the table
            string query = "SELECT id, name FROM effects where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiEffectOptions = new GUIContent[rows.Count + 1];
                GuiEffectOptions[optionsId] = new GUIContent("~ none ~");
                effectIds = new int[rows.Count + 1];
                effectIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiEffectOptions[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    effectIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Damage Type

        public static GUIContent[] LoadDamageTypeOptions()
        {

            GUIContent[] options = new GUIContent[1];
            // Read all entries from the table
            string query = "SELECT name FROM damage_type where isactive = 1";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                options = new GUIContent[rows.Count];
                foreach (Dictionary<string, string> data in rows)
                {
                    options[optionsId] = new GUIContent(data["name"]);
                    optionsId++;
                }
            }

            return options;
        }

        #endregion

        #region Option Chioce

        /// <summary>
        /// Loads the atavism choice options for the specified OptionType
        /// </summary>
        /// <returns>The atavism choice options.</returns>
        /// <param name="optionType">Option type.</param>
        public static List<string> LoadOptionChoiceList(string optionType, bool allowNone)
        {
            List<string> options = new List<string>();
            if (allowNone)
            {
                options.Add("~ none ~");
            }

            // First need to get the ID for the optionType
            int optionTypeID = -1;

            string query = "SELECT id FROM editor_option where optionType = '" + optionType + "' AND isactive = 1";
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    optionTypeID = int.Parse(data["id"]);
                }
            }

            // If we have an ID, load in the options
            if (optionTypeID != -1)
            {
                // Read all entries from the table
                query = "SELECT optionTypeID, choice FROM editor_option_choice where optionTypeID = " + optionTypeID +
                        " AND isactive = 1";

                rows.Clear();
                // Load data
                rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
                //Debug.Log("#Rows:"+rows.Count);
                // Read all the data
                if ((rows != null) && (rows.Count > 0))
                {
                    foreach (Dictionary<string, string> data in rows)
                    {
                        options.Add(data["choice"]);
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Loads the atavism choice options for the specified OptionType
        /// </summary>
        /// <returns>The atavism choice options.</returns>
        /// <param name="optionType">Option type.</param>
        public static string[] LoadAtavismChoiceOptions(string optionType, bool allowNone)
        {
            string[] options = new string[] { };
            if (allowNone)
            {
                options = new string[1];
                options[0] = "~ none ~";
            }

            // First need to get the ID for the optionType
            int optionTypeID = -1;

            string query = "SELECT id FROM editor_option where optionType = '" + optionType + "' AND isactive = 1";
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    optionTypeID = int.Parse(data["id"]);
                }
            }

            // If we have an ID, load in the options
            if (optionTypeID != -1)
            {
                // Read all entries from the table
                query = "SELECT optionTypeID, choice FROM editor_option_choice where optionTypeID = " + optionTypeID +
                        " AND isactive = 1";

                rows.Clear();
                // Load data
                rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
                //Debug.Log("#Rows:"+rows.Count);
                // Read all the data
                int optionsId = 0;
                if ((rows != null) && (rows.Count > 0))
                {
                    if (allowNone)
                    {
                        options = new string[rows.Count + 1];
                        options[0] = "~ none ~";
                        optionsId++;
                    }
                    else
                    {
                        options = new string[rows.Count];
                    }

                    foreach (Dictionary<string, string> data in rows)
                    {
                        options[optionsId] = data["choice"];
                        optionsId++;
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Loads the atavism choice options for the specified OptionType
        /// </summary>
        /// <returns>The atavism choice options.</returns>
        /// <param name="optionType">Option type.</param>
        public static void LoadAtavismChoiceOptions(string optionType, bool allowNone, out int[] ids,
            out string[] options)
        {
            string query =
                "SELECT id, choice FROM editor_option_choice where optionTypeID = (SELECT id from editor_option where "
                + "optionType = '" + optionType + "') AND isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            options = new string[rows.Count];
            ids = new int[rows.Count];

            int optionsId = 0;
            if (allowNone)
            {
                options = new string[rows.Count + 1];
                options[optionsId] = "~ none ~";
                ids = new int[rows.Count + 1];
                ids[optionsId] = -1;
                optionsId++;
            }

            // Read data
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    options[optionsId] = data["choice"];
                    ids[optionsId] = int.Parse(data["id"]);
                    optionsId++;
                }
            }
        }

        /// <summary>
        /// Loads the atavism choice options for the specified OptionType
        /// </summary>
        /// <param name="optionType"></param>
        /// <param name="allowNone"></param>
        /// <param name="ids"></param>
        /// <param name="options"></param>
        /// <param name="addAll"></param>
        public static void LoadAtavismChoiceOptions(string optionType, bool allowNone, out int[] ids,
            out string[] options, bool addAll)
        {
            string query =
                "SELECT id, choice FROM editor_option_choice where optionTypeID = (SELECT id from editor_option where "
                + "optionType = '" + optionType + "') AND isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            options = new string[rows.Count];
            ids = new int[rows.Count];

            int optionsId = 0;
            if (addAll)
            {
                if (allowNone)
                {
                    options = new string[rows.Count + 2];
                    options[optionsId] = "~ none ~";
                    ids = new int[rows.Count + 2];
                    ids[optionsId] = -1;
                    optionsId++;
                    options[optionsId] = "Any";
                    ids[optionsId] = 0;


                }
                else
                {
                    options = new string[rows.Count + 1];
                    options[optionsId] = "Any";
                    ids = new int[rows.Count + 1];
                    ids[optionsId] = 0;
                    optionsId++;
                }
            }
            else
            {
                if (allowNone)
                {
                    options = new string[rows.Count + 1];
                    options[optionsId] = "~ none ~";
                    ids = new int[rows.Count + 1];
                    ids[optionsId] = -1;
                    optionsId++;
                }
            }

            // Read data
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    options[optionsId] = data["choice"];
                    ids[optionsId] = int.Parse(data["id"]);
                    optionsId++;
                }
            }
        }

        #endregion

        #region Interactive Objects

        public static void LoadInteractiveObjectProfile()
        {
            // Read all entries from the table
            string query = "SELECT id, name FROM interactive_object where isactive = 1 and instance = -1";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            // rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                interactiveProfileOptions = new GUIContent[rows.Count + 1];
                interactiveProfileOptions[optionsId] = new GUIContent("~ none ~");
                interactiveProfileIds = new int[rows.Count + 1];
                interactiveProfileIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    interactiveProfileOptions[optionsId] = new GUIContent(data["name"]);
                    interactiveProfileIds[optionsId] = int.Parse(data["id"]);
                }
            }

        }

        public static Dictionary<string, string> LoadInteractiveObjectProfile(int id)
        {
            // Read all entries from the table
            string query = "SELECT * FROM interactive_object where isactive = 1 and instance = -1 and id = " + id;

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Debug.Log("#Rows:"+rows.Count);
            if (rows.Count > 0)
            {
                LoadInteractiveObjectProfileCoords(id, rows[0]);
                return rows[0];
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, string> LoadInteractiveObjectProfileCoords(int profileId,
            Dictionary<string, string> list)
        {
            // Read all entries from the table
            string query = "SELECT * FROM interactive_object_coordeffects where objId = " + profileId +
                           " order by `order`";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            //  Debug.Log("#Rows:" + rows.Count);
            int c = 0;
            if (rows.Count > 0)
            {
                foreach (var row in rows)
                {
                    // Debug.Log(row["coordEffect"]);

                    list.Add("coord_" + c++, row["coordEffect"]);
                }
            }

            list.Add("coord_num", c.ToString());
            return list;

        }

        #endregion

        #region Items

        public static void LoadItemOptions()
        {
            string query = "SELECT id, name FROM item_templates where isactive = 1";
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);


            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            itemsList = new string[rows.Count + 1];
            itemIds = new int[rows.Count + 1];
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                itemsList[optionsId] = "~ none ~";
                itemIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    itemsList[optionsId] = data["id"] + ":" + data["name"];
                    itemIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }


        public static void LoadItemOptions(bool gui)
        {
            if (!gui)
            {
                LoadItemOptions();
                return;
            }

            string query = "SELECT id, name FROM item_templates where isactive = 1";
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);


            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            GuiItemsList = new GUIContent[rows.Count + 1];
            itemIds = new int[rows.Count + 1];
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiItemsList[optionsId] = new GUIContent("~ none ~");
                itemIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiItemsList[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    itemIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Resource Node Prodile

        public static void LoadResourceNodeProfileOptions(bool gui)
        {
            if (!gui)
            {
                LoadItemOptions();
                return;
            }

            string query = "SELECT id, name FROM resource_node_profile where isactive = 1";
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);


            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            RNProfileList = new GUIContent[rows.Count + 1];
            RNProfileIds = new int[rows.Count + 1];
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                RNProfileList[optionsId] = new GUIContent("~ none ~");
                RNProfileIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    RNProfileList[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    RNProfileIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Quests

        public static void LoadQuestOptions()
        {
            string query = "SELECT id, name FROM quests where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                questOptions = new string[rows.Count + 1];
                questOptions[optionsId] = "~ none ~";
                questIds = new int[rows.Count + 1];
                questIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    questOptions[optionsId] = data["id"] + ":" + data["name"];
                    questIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadQuestOptions(bool gui)
        {
            if (!gui)
            {
                LoadQuestOptions();
                return;
            }

            string query = "SELECT id, name FROM quests where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiQuestOptions = new GUIContent[rows.Count + 1];
                GuiQuestOptions[optionsId] = new GUIContent("~ none ~");
                questIds = new int[rows.Count + 1];
                questIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiQuestOptions[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    questIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Currency

        public static void LoadCurrencyOptions()
        {
            // Read all entries from the table
            string query = "SELECT id, name FROM currencies where isactive = 1";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                currencyOptions = new string[rows.Count + 1];
                currencyOptions[optionsId] = "~ none ~";
                currencyIds = new int[rows.Count + 1];
                currencyIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    currencyOptions[optionsId] = data["name"];
                    currencyIds[optionsId] = int.Parse(data["id"]);
                }
            }

        }

        public static void LoadCurrencyOptions(bool gui)
        {
            if (!gui)
            {
                LoadCurrencyOptions();
                return;
            }

            // Read all entries from the table
            string query = "SELECT id, name FROM currencies where isactive = 1";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiCurrencyOptions = new GUIContent[rows.Count + 1];
                GuiCurrencyOptions[optionsId] = new GUIContent("~ none ~");
                currencyIds = new int[rows.Count + 1];
                currencyIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiCurrencyOptions[optionsId] = new GUIContent(data["name"]);
                    currencyIds[optionsId] = int.Parse(data["id"]);
                }
            }

        }

        #endregion

        #region Tasks

        public static void LoadTaskOptions()
        {
            string query = "SELECT id, name FROM task where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                taskOptions = new string[rows.Count + 1];
                taskOptions[optionsId] = "~ none ~";
                taskIds = new int[rows.Count + 1];
                taskIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    taskOptions[optionsId] = data["id"] + ":" + data["name"];
                    taskIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadTaskOptions(bool gui)
        {
            if (!gui)
            {
                LoadTaskOptions();
                return;
            }

            string query = "SELECT id, name FROM task where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiTaskOptions = new GUIContent[rows.Count + 1];
                GuiTaskOptions[optionsId] = new GUIContent("~ none ~");
                taskIds = new int[rows.Count + 1];
                taskIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiTaskOptions[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    taskIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Instances

        public static int GetInstanceID(string instanceName)
        {
            string query = "SELECT id FROM instance_template where island_name = '" + instanceName + "'";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.adminDatabasePrefix, query);
            // Read data
            //	int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    return int.Parse(data["id"]);
                }
            }

            return -1;
        }

        public static void LoadInstanceOptions()
        {
            string query = "SELECT id, island_name FROM instance_template";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.adminDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                instanceList = new string[rows.Count + 1];
                instanceList[optionsId] = "~ none ~";
                instanceIds = new int[rows.Count + 1];
                instanceIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    instanceList[optionsId] = data["id"] + ":" + data["island_name"];
                    instanceIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadInstanceOptions(bool gui)
        {
            if (!gui)
            {
                LoadInstanceOptions();
                return;
            }

            string query = "SELECT id, island_name FROM instance_template";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.adminDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiInstanceList = new GUIContent[rows.Count + 1];
                GuiInstanceList[optionsId] = new GUIContent("~ none ~");
                instanceIds = new int[rows.Count + 1];
                instanceIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiInstanceList[optionsId] = new GUIContent(data["id"] + ":" + data["island_name"]);
                    instanceIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }


        public static void LoadInstancePortals(int instanceId)
        {
            // Read all entries from the table
            string query = "SELECT * FROM " + "island_portals" + " where island = " + instanceId;

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.adminDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiInstanceSpawnList = new GUIContent[rows.Count + 1];
                GuiInstanceSpawnList[optionsId] = new GUIContent("~ none ~");
                GuiInstanceSpawnIds = new int[rows.Count + 1];
                GuiInstanceSpawnIds[optionsId] = -1;
                GuiInstanceSpawnLoc = new Vector3[rows.Count + 1];
                GuiInstanceSpawnLoc[optionsId] = Vector3.zero;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;

                    GuiInstanceSpawnIds[optionsId] = int.Parse(data["id"]);
                    GuiInstanceSpawnList[optionsId] = new GUIContent(data["name"]);
                    GuiInstanceSpawnLoc[optionsId] = new Vector3(float.Parse(data["locX"]), float.Parse(data["locY"]),
                        float.Parse(data["locZ"]));
                }
            }
        }

        public void LoadInstanceWeatherProfilesOptions()
        {
            // Read all entries from the table
            string query = "SELECT id, name FROM weather_profile ";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //   Debug.LogError("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                instanceWeatherProfileList = new string[rows.Count + 1];
                instanceWeatherProfileList[optionsId] = "~ none ~";
                instanceWeatherProfileIds = new int[rows.Count + 1];
                instanceWeatherProfileIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    instanceWeatherProfileList[optionsId] = data["id"] + ":" + data["name"];
                    instanceWeatherProfileIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Claims

        public static void LoadClaimObjectLimitProfile()
        {
            // Read all entries from the table
            string query = "SELECT id, name FROM claim_profile where isactive = 1";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                claimProfileOptions = new GUIContent[rows.Count + 1];
                claimProfileOptions[optionsId] = new GUIContent("~ none ~");
                claimProfileIds = new int[rows.Count + 1];
                claimProfileIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    claimProfileOptions[optionsId] = new GUIContent(data["name"]);
                    claimProfileIds[optionsId] = int.Parse(data["id"]);
                }
            }

        }

        #endregion

        #region Mobs

        public static void LoadMobOptions()
        {
            string query = "SELECT id, name FROM mob_templates where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                mobOptions = new string[rows.Count + 1];
                mobOptions[optionsId] = "~ none ~";
                mobIds = new int[rows.Count + 1];
                mobIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    mobOptions[optionsId] = data["id"] + ":" + data["name"];
                    mobIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadMobOptions(bool gui)
        {
            if (!gui)
            {
                LoadMobOptions();
                return;
            }

            string query = "SELECT id, name FROM mob_templates where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiMobOptions = new GUIContent[rows.Count + 1];
                GuiMobOptions[optionsId] = new GUIContent("~ none ~");
                mobIds = new int[rows.Count + 1];
                mobIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiMobOptions[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    mobIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static Mob LoadMobTemplateModel(int id)
        {
            // Read all entries from the table
            string query =
                "SELECT display1,aggro_radius,name,displayName FROM mob_templates where isactive = 1 AND id =" + id;

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read all the data
            Mob display = new Mob();
            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    display.display1 = data["display1"];
                    display.aggro_range = int.Parse(data["aggro_radius"]);
                    display.Name = data["name"];
                    display.displayName = data["displayName"];
                }
            }

            return display;
        }

        #endregion

        #region Dialogue

        public static void LoadDialogueList()
        {
            string query = "SELECT id, name FROM dialogue where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                dialogueList = new string[rows.Count + 1];
                dialogueList[optionsId] = "~ none ~";
                dialogueIds = new int[rows.Count + 1];
                dialogueIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    dialogueList[optionsId] = data["id"] + ":" + data["name"];
                    dialogueIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadDialogueList(bool gui)
        {
            if (!gui)
            {
                LoadDialogueList();
                return;
            }

            string query = "SELECT id, name FROM dialogue where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiDialogueList = new GUIContent[rows.Count + 1];
                GuiDialogueList[optionsId] = new GUIContent("~ none ~");
                dialogueIds = new int[rows.Count + 1];
                dialogueIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiDialogueList[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    dialogueIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadOpeningDialogueList(bool gui)
        {
            if (!gui)
            {
                LoadDialogueList();
                return;
            }

            string query = "SELECT id, name FROM dialogue where openingDialogue = 1 and isactive = 1 ";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiDialogueList = new GUIContent[rows.Count + 1];
                GuiDialogueList[optionsId] = new GUIContent("~ none ~");
                dialogueIds = new int[rows.Count + 1];
                dialogueIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiDialogueList[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    dialogueIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        #endregion

        #region Factions

        public static void LoadFactionOptions()
        {
            string query = "SELECT id, name FROM factions where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                factionOptions = new string[rows.Count + 1];
                factionOptions[optionsId] = "~ none ~";
                factionIds = new int[rows.Count + 1];
                factionIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    factionOptions[optionsId] = data["id"] + ":" + data["name"];
                    factionIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadFactionOptions(bool gui)
        {
            if (!gui)
            {
                LoadFactionOptions();
                return;
            }

            string query = "SELECT id, name FROM factions where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiFactionOptions = new GUIContent[rows.Count + 1];
                GuiFactionOptions[optionsId] = new GUIContent("~ none ~");
                factionIds = new int[rows.Count + 1];
                factionIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiFactionOptions[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    factionIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static GUIContent[] LoadStatOptionsForGui()
        {

            GUIContent[] options = new GUIContent[1];
            // Read all entries from the table
            string query = "SELECT name FROM stat where isactive = 1";

            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                options = new GUIContent[rows.Count];
                foreach (Dictionary<string, string> data in rows)
                {
                    options[optionsId] = new GUIContent(data["name"]);
                    optionsId++;
                }
            }

            return options;
        }

        #endregion

        #region Item Slots

        public static GUIContent[] LoadSlotsOptions(bool addRoot = false, bool addNone = false)
        {
            GUIContent[] options = new GUIContent[0];
            // Read all entries from the table
            string query = "SELECT name FROM item_slots where isactive = 1";

            List<Dictionary<string, string>>
                rows = null; //DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);

            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                options = new GUIContent[rows.Count + (addRoot ? 1 : 0) + (addNone ? 1 : 0)];
                if (addNone)
                {

                    options[optionsId] = new GUIContent("None");
                    optionsId++;
                }

                if (addRoot)
                {

                    options[optionsId] = new GUIContent("Root");
                    optionsId++;
                }


                foreach (Dictionary<string, string> data in rows)
                {
                    options[optionsId] = new GUIContent(data["name"]);
                    optionsId++;
                }
            }
            else
            {
                options = new GUIContent[rows.Count + (addRoot ? 1 : 0) + (addNone ? 1 : 0)];
                if (addNone)
                {

                    options[optionsId] = new GUIContent("None");
                    optionsId++;
                }

                if (addRoot)
                {

                    options[optionsId] = new GUIContent("Root");
                    optionsId++;
                }
            }

            return options;
        }

        #endregion

        #region Merchant Tables

        public static void LoadMerchantTableList()
        {
            string query = "SELECT id, name FROM merchant_tables where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                merchantTableList = new string[rows.Count + 1];
                merchantTableList[optionsId] = "~ none ~";
                merchantTableIds = new int[rows.Count + 1];
                merchantTableIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    merchantTableList[optionsId] = data["id"] + ":" + data["name"];
                    merchantTableIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }

        public static void LoadMerchantTableList(bool gui)
        {
            if (!gui)
            {
                LoadMerchantTableList();
                return;
            }

            string query = "SELECT id, name FROM merchant_tables where isactive = 1";

            // Load data
            List<Dictionary<string, string>> rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            // Read data
            int optionsId = 0;
            if ((rows != null) && (rows.Count > 0))
            {
                GuiMerchantTableList = new GUIContent[rows.Count + 1];
                GuiMerchantTableList[optionsId] = new GUIContent("~ none ~");
                merchantTableIds = new int[rows.Count + 1];
                merchantTableIds[optionsId] = -1;
                foreach (Dictionary<string, string> data in rows)
                {
                    optionsId++;
                    GuiMerchantTableList[optionsId] = new GUIContent(data["id"] + ":" + data["name"]);
                    merchantTableIds[optionsId] = int.Parse(data["id"]);
                }
            }
        }


        #endregion

        #region Patrol Path

        public static void SavePatrolPath(PatrolPathMarker marker)
        {
            // Verify the user is logged in and the Content Database is connected

            // If the gameObject already has an id, run an update
            if (marker.id < 1)
            {
                InsertMarker(marker);
            }
            else
            {
                UpdateMarker(marker);
            }
        }

        public static int InsertMarker(PatrolPathMarker marker)
        {
            string query =
                "INSERT INTO patrol_path (name, startingPoint, travelReverse, locX, locY, locZ, lingerTime) VALUES ";
            query += "('" + marker.name + "'," + marker.startingPoint + "," + marker.travelReverse + "," +
                     marker.transform.position.x + ",";
            query += marker.transform.position.y + "," + marker.transform.position.z + "," + marker.lingerTime + ")";

            // Setup the register data		
            List<Register> update = new List<Register>();
            int itemID = -1;
            itemID = DatabasePack.Insert(DatabasePack.contentDatabasePrefix, query, update);
            marker.id = itemID;

            // If there is a next point, insert that and get the id, then save that as the nextPoint in the database
            if (marker.nextPoint != null)
            {
                if (marker.nextPoint.GetComponent<PatrolPathMarker>().id < 1)
                {
                    int nextPointID = InsertMarker(marker.nextPoint.GetComponent<PatrolPathMarker>());
                    query = "UPDATE patrol_path set nextPoint = " + nextPointID + " where id = " + marker.id;
                    DatabasePack.Update(DatabasePack.contentDatabasePrefix, query, update);
                }
                else
                {
                    UpdateMarker(marker.nextPoint.GetComponent<PatrolPathMarker>());
                }
            }

            return itemID;
        }

        public static int UpdateMarker(PatrolPathMarker marker)
        {
            string query = "";
            int nextPointID = -1;
            // If there is a next point, insert that and get the id, then save that as the nextPoint in the database
            if (marker.nextPoint != null)
            {
                if (marker.nextPoint.GetComponent<PatrolPathMarker>().id < 1)
                {
                    nextPointID = InsertMarker(marker.nextPoint.GetComponent<PatrolPathMarker>());
                }
                else
                {
                    UpdateMarker(marker.nextPoint.GetComponent<PatrolPathMarker>());
                }
            }

            query = "UPDATE patrol_path";
            query += " SET travelReverse = " + marker.travelReverse + ", locX = " + marker.transform.position.x +
                     ", locY = " + marker.transform.position.y;
            query += ", locZ = " + marker.transform.position.z + ", lingerTime = " + marker.lingerTime +
                     ", nextPoint = " + nextPointID;
            query += " WHERE id=" + marker.id;

            // Setup the register data		
            List<Register> update = new List<Register>();
            DatabasePack.Update(DatabasePack.contentDatabasePrefix, query, update);

            return marker.id;
        }

        public static void DeleteMarker(PatrolPathMarker marker)
        {
            string query = "UPDATE patrol_path";
            query += " SET isactive = 0";
            query += " WHERE id=" + marker.id;

            // Setup the register data		
            List<Register> update = new List<Register>();
            DatabasePack.Update(DatabasePack.contentDatabasePrefix, query, update);

            // Update any spawns that are using this path
            query = "UPDATE spawn_data";
            query += " SET patrolPath = -1";
            query += " WHERE patrolPath=" + marker.id;
            DatabasePack.Update(DatabasePack.contentDatabasePrefix, query, update);
        }

        #endregion

        #region Mob Spawn Data

        public static List<MobSpawnData> LoadMobSpawnDataEntities(int instanceId)
        {
            List<MobSpawnData> list = new List<MobSpawnData>();
            // Read all entries from the table
            string query = "SELECT * FROM spawn_data where isactive = 1 and instance = " + instanceId;

            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            // Load data
            rows = DatabasePack.LoadData(DatabasePack.contentDatabasePrefix, query);
            //Debug.Log("#Rows:"+rows.Count);
            // Read all the data

            if ((rows != null) && (rows.Count > 0))
            {
                foreach (Dictionary<string, string> data in rows)
                {
                    //foreach(string key in data.Keys)
                    //	Debug.Log("Name[" + key + "]:" + data[key]);
                    //return;
                    MobSpawnData display = new MobSpawnData();
                    display.id = int.Parse(data["id"]);
                    display.Name = data["name"];
                    display.mobTemplate = int.Parse(data["mobTemplate"]);
                    display.mobTemplate2 = int.Parse(data["mobTemplate2"]);
                    display.mobTemplate3 = int.Parse(data["mobTemplate3"]);
                    display.mobTemplate4 = int.Parse(data["mobTemplate4"]);
                    display.mobTemplate5 = int.Parse(data["mobTemplate5"]);
                    // display.numSpawns = int.Parse(data["numSpawns"]);
                    //  display.spawnRadius = int.Parse(data["spawnRadius"]);
                    display.respawnTime = int.Parse(data["respawnTime"]);
                    display.respawnTimeMax = int.Parse(data["respawnTimeMax"]);
                    display.corpseDespawnTime = int.Parse(data["corpseDespawnTime"]);
                    display.spawnActiveStartHour = int.Parse(data["spawnActiveStartHour"]);
                    display.spawnActiveEndHour = int.Parse(data["spawnActiveEndHour"]);
                    display.alternateSpawnMobTemplate = int.Parse(data["alternateSpawnMobTemplate"]);
                    display.alternateSpawnMobTemplate2 = int.Parse(data["alternateSpawnMobTemplate2"]);
                    display.alternateSpawnMobTemplate3 = int.Parse(data["alternateSpawnMobTemplate3"]);
                    display.alternateSpawnMobTemplate4 = int.Parse(data["alternateSpawnMobTemplate4"]);
                    display.alternateSpawnMobTemplate5 = int.Parse(data["alternateSpawnMobTemplate5"]);
                    display.combat = bool.Parse(data["combat"]);
                    display.roamRadius = int.Parse(data["roamRadius"]);
                    display.roamDelayMin = float.Parse(data["roamDelayMin"]);
                    display.roamDelayMax = float.Parse(data["roamDelayMax"]);
                    display.roamRollTimeEachTime = bool.Parse(data["roamRollTimeEachTime"]);
                    display.patrolPath = int.Parse(data["patrolPath"]);
                    display.startsQuests = data["startsQuests"];
                    display.endsQuests = data["endsQuests"];
                    display.startsDialogues = data["startsDialogues"];
                    display.otherActions = data["otherActions"];
                    display.baseAction = data["baseAction"];
                    display.weaponSheathed = bool.Parse(data["weaponSheathed"]);
                    display.merchantTable = int.Parse(data["merchantTable"]);
                    display.questOpenLootTable = int.Parse(data["questOpenLootTable"]);
                    display.isChest = bool.Parse(data["isChest"]);
                    display.pickupItem = int.Parse(data["pickupItem"]);
                    float x = float.Parse(data["locX"]);
                    float y = float.Parse(data["locY"]);
                    float z = float.Parse(data["locZ"]);
                    display.position = new Vector3(x, y, z);
                    float ox = float.Parse(data["orientX"]);
                    float oy = float.Parse(data["orientY"]);
                    float oz = float.Parse(data["orientZ"]);
                    float ow = float.Parse(data["orientW"]);
                    display.rotation = new Quaternion(ox, oy, oz, ow);
                    display.isLoaded = true;
                    list.Add(display);
                    //Debug.Log("Name:" + display.name  + "=[" +  display.id  + "]");
                }
            }

            return list;
        }


        #endregion

    }
}