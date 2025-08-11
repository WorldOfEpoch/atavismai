using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ExcaliburAI.Data;

namespace ExcaliburAI.Editor
{
    public static class RuleBasedGenerator
    {
        const string GenIconDir = "Assets/ExcaliburAI/Generated/Icons";

        [MenuItem("ExcaliburAI/Generate/Weapons (Rule-Based)")]
        public static void GenerateWeaponsMenu() => GenerateWeapons(25, null, null);

        [MenuItem("ExcaliburAI/Generate/Classes (Rule-Based)")]
        public static void GenerateClassesMenu() => GenerateClasses(6, null, null);

        public static void GenerateWeapons(int count, RuleSet rules, int? seed)
        {
            rules ??= LoadRulesOrCreate();
            var rng = new System.Random(seed ?? Environment.TickCount);
            var outDir = "Assets/ExcaliburAI/Generated/Items";
            Directory.CreateDirectory(outDir);
            Directory.CreateDirectory(GenIconDir);

            for (int i = 0; i < count; i++)
            {
                // pick weapon type
                var wt = rules.weaponRules[rng.Next(rules.weaponRules.Length)].type;
                var wr = rules.GetRule(wt);

                // base ranges
                int minMin = wr.minDamageMin, minMax = wr.minDamageMax;
                int maxMin = wr.maxDamageMin, maxMax = wr.maxDamageMax;
                float spdMin = wr.attackSpeedMin, spdMax = wr.attackSpeedMax;

                // pick tier & apply multipliers
                var tier = rules.RollTier(rng);
                float dmgMult = tier.dmgMult, spdMult = tier.spdMult;

                int minDmg = Mathf.RoundToInt(RandomRangeInt(rng, minMin, minMax) * dmgMult);
                int maxDmg = Mathf.Max(minDmg + 1, Mathf.RoundToInt(RandomRangeInt(rng, maxMin, maxMax) * dmgMult));
                float atkSpd = Mathf.Clamp(RandomRangeFloat(rng, spdMin, spdMax) * spdMult, 0.5f, 2.0f);

                var baseName = wr.baseNames.Length > 0 ? wr.baseNames[rng.Next(wr.baseNames.Length)] : wt.ToString();
                var name = rules.MakeWeaponName(rng, baseName, tier.name);

                // create asset
                var asset = ScriptableObject.CreateInstance<ItemDefinition>();
                asset.Id = Guid.NewGuid().ToString("N");
                asset.DisplayName = name;
                asset.Description = $"A {tier.name.ToLower()} {wt} forged for adventurers of {rules.defaultTheme}.";
                asset.Tier = tier.name;
                asset.WeaponType = wt;
                asset.MinDamage = minDmg;
                asset.MaxDamage = maxDmg;
                asset.AttackSpeed = atkSpd;
                asset.IconPrompt = $"{tier.name.ToLower()} {wt} {baseName} fantasy icon";

                // placeholder icon
                asset.Icon = MakePlaceholderIcon(name, rng);

                var settings = AiSettingsUtil.LoadOrCreate();
                var iconDir = settings.atavismMode ? settings.atavismIconsFolder
                                   : "Assets/ExcaliburAI/Generated/Icons";
                Directory.CreateDirectory(iconDir);
                var path = $"{iconDir}/{Sanitize(name)}.png";
                File.WriteAllBytes(path, asset.Icon.texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                asset.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                var path = $"{outDir}/{Sanitize(name)}.asset";
                AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ExcaliburAI: Generated {count} weapons (rule-based).");
        }

        public static void GenerateClasses(int count, RuleSet rules, int? seed)
        {
            rules ??= LoadRulesOrCreate();
            var rng = new System.Random(seed ?? Environment.TickCount);
            var outDir = "Assets/ExcaliburAI/Generated/Classes";
            Directory.CreateDirectory(outDir);
            Directory.CreateDirectory(GenIconDir);

            for (int i = 0; i < count; i++)
            {
                var role = rules.roles.Length > 0 ? rules.roles[rng.Next(rules.roles.Length)] : "DPS";
                var stats = rules.primaryStatCombos.Length > 0 ? rules.primaryStatCombos[rng.Next(rules.primaryStatCombos.Length)] : "STR/DEX";
                var name = rules.MakeClassName(rng);
                var pitch = rules.MakeClassPitch(rng, name, role, stats, rules.defaultTheme);

                var asset = ScriptableObject.CreateInstance<ClassDefinition>();
                asset.Id = Guid.NewGuid().ToString("N");
                asset.ClassName = name;
                asset.Role = role;
                asset.PrimaryStats = stats;
                asset.FantasyPitch = pitch;
                asset.IconPrompt = $"{name} sigil, {role} icon";
                asset.Icon = MakePlaceholderIcon(name, rng);

                var path = $"{outDir}/{Sanitize(name)}.asset";
                AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ExcaliburAI: Generated {count} classes (rule-based).");
        }

        static RuleSet LoadRulesOrCreate()
        {
            var guid = AssetDatabase.FindAssets("t:RuleSet").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid))
                return AssetDatabase.LoadAssetAtPath<RuleSet>(AssetDatabase.GUIDToAssetPath(guid));

            var dir = "Assets/ExcaliburAI/Runtime/Data";
            Directory.CreateDirectory(dir);
            var rules = ScriptableObject.CreateInstance<RuleSet>();
            AssetDatabase.CreateAsset(rules, $"{dir}/DefaultRuleSet.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("ExcaliburAI: Created DefaultRuleSet.asset");
            return rules;
        }

        static int RandomRangeInt(System.Random rng, int min, int maxInclusive) => rng.Next(min, maxInclusive + 1);
        static float RandomRangeFloat(System.Random rng, float min, float max) => (float)(min + rng.NextDouble() * (max - min));

        static string Sanitize(string s)
        {
            var safe = new string((s ?? "Unnamed").Where(ch => char.IsLetterOrDigit(ch) || ch=='_' || ch=='-' || ch==' ').ToArray());
            return string.IsNullOrWhiteSpace(safe) ? "Unnamed" : safe.Trim().Replace(' ', '_');
        }

        static Sprite MakePlaceholderIcon(string label, System.Random rng)
        {
            var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var col = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            var pixels = Enumerable.Repeat(col, 256 * 256).ToArray();
            tex.SetPixels(pixels); tex.Apply();

            Directory.CreateDirectory(GenIconDir);
            var path = $"{GenIconDir}/{Sanitize(label)}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null) { ti.textureType = TextureImporterType.Sprite; ti.mipmapEnabled = false; ti.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
