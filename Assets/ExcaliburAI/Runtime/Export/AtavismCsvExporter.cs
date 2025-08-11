using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ExcaliburAI.Data;

namespace ExcaliburAI.Export
{
    public static class AtavismCsvExporter
    {
        // ⚠️ Columns here are a sane default; if your Atavism editor expects different headers,
        // tell me your version and I’ll match them exactly.
        private static readonly string[] Headers = new[]
        {
            // core identity
            "name","description","icon",
            // classification
            "item_type","sub_type","quality","tier",
            // equip/weapon-ish
            "weapon_type","min_damage","max_damage","attack_speed",
            // stacks & misc
            "stack_size"
        };

        [MenuItem("ExcaliburAI/Export/Items → Atavism CSV")]
        public static void ExportItemsCsv()
        {
            var settings = Editor.AiSettingsUtil.LoadOrCreate();
            var outDir = settings.atavismCsvFolder;
            Directory.CreateDirectory(outDir);
            var path = Path.Combine(outDir, "items_excalibur.csv");

            var guids = AssetDatabase.FindAssets("t:ItemDefinition");
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", Headers));

            foreach (var guid in guids)
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                var iconPath = item.Icon ? AssetDatabase.GetAssetPath(item.Icon.texture) : "";
                // turn Unity path into a short icon name if your Atavism editor wants just the filename:
                var iconName = string.IsNullOrEmpty(iconPath) ? "" : Path.GetFileName(iconPath);

                var row = new string[]
                {
                    Escape(item.SafeName),
                    Escape(item.Description),
                    Escape(iconName),
                    "Weapon",                         // item_type (tweak as needed per your schema)
                    item.WeaponType.ToString(),       // sub_type
                    item.Tier,                        // quality or rarity label
                    item.Tier,                        // tier again if you separate quality/tier
                    item.WeaponType.ToString(),
                    item.MinDamage.ToString(),
                    item.MaxDamage.ToString(),
                    item.AttackSpeed.ToString("0.00"),
                    "1"                               // stack_size
                };
                sb.AppendLine(string.Join(",", row));
            }

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
            AssetDatabase.Refresh();
            Debug.Log($"Atavism CSV written: {path}");
        }

        static string Escape(string s) => "\"" + (s ?? "").Replace("\"","\"\"") + "\"";
    }
}
