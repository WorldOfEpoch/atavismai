using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using ExcaliburAI.Data;

namespace ExcaliburAI.Export
{
    public static class AtavismCsvExporter
    {
        [MenuItem("ExcaliburAI/Export/Items to CSV")]
        public static void ExportItemsCsv()
        {
            var items = AssetDatabase.FindAssets("t:ItemDefinition");
            var sb = new StringBuilder();
            sb.AppendLine("id,name,description,tier,type,minDamage,maxDamage,attackSpeed,iconPath");
            foreach (var guid in items)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                var iconPath = item.Icon ? AssetDatabase.GetAssetPath(item.Icon.texture) : "";
                sb.AppendLine($"{item.Id},{Escape(item.SafeName)},{Escape(item.Description)},{item.Tier},{item.WeaponType},{item.MinDamage},{item.MaxDamage},{item.AttackSpeed},{iconPath}");
            }
            var dest = EditorUtility.SaveFilePanel("Save Items CSV", Application.dataPath, "items", "csv");
            if (!string.IsNullOrEmpty(dest)) File.WriteAllText(dest, sb.ToString());
            Debug.Log("Exported items CSV.");
        }

        static string Escape(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
    }
}
