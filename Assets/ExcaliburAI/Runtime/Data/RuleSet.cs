using System;
using System.Linq;
using UnityEngine;

namespace ExcaliburAI.Data
{
    [Serializable] public class TierEntry { public string name = "Common"; public float weight = 1f; public float dmgMult = 1f; public float spdMult = 1f; }
    [Serializable] public class WeaponTypeRule
    {
        public WeaponType type = WeaponType.Sword;
        public int minDamageMin = 2, minDamageMax = 5;
        public int maxDamageMin = 6, maxDamageMax = 10;
        public float attackSpeedMin = 0.9f, attackSpeedMax = 1.3f;
        public string[] baseNames = new[] { "Shortsword", "Longsword", "Blade", "Sabre" };
    }

    [CreateAssetMenu(menuName = "ExcaliburAI/Rule Set")]
    public class RuleSet : ScriptableObject
    {
        [Header("General")]
        public string defaultTheme = "high fantasy";
        public int nameMaxLen = 32;

        [Header("Tiers (weighted)")]
        public TierEntry[] tiers = new[]
        {
            new TierEntry { name="Common",    weight=55, dmgMult=1.00f, spdMult=1.00f },
            new TierEntry { name="Uncommon",  weight=25, dmgMult=1.08f, spdMult=1.00f },
            new TierEntry { name="Rare",      weight=12, dmgMult=1.15f, spdMult=1.02f },
            new TierEntry { name="Epic",      weight=6,  dmgMult=1.25f, spdMult=1.04f },
            new TierEntry { name="Legendary", weight=2,  dmgMult=1.35f, spdMult=1.06f },
        };

        [Header("Weapon Type Rules")]
        public WeaponTypeRule[] weaponRules = new[]
        {
            new WeaponTypeRule { type=WeaponType.Sword, baseNames=new[]{"Shortsword","Longsword","Blade","Cutlass"} },
            new WeaponTypeRule { type=WeaponType.Axe,   minDamageMin=3, minDamageMax=6, maxDamageMin=8, maxDamageMax=14, attackSpeedMin=0.8f, attackSpeedMax=1.1f, baseNames=new[]{"Hatchet","Handaxe","War Axe"} },
            new WeaponTypeRule { type=WeaponType.Dagger,minDamageMin=1, minDamageMax=3, maxDamageMin=4, maxDamageMax=8,  attackSpeedMin=1.2f, attackSpeedMax=1.6f, baseNames=new[]{"Shiv","Stiletto","Knife"} },
            new WeaponTypeRule { type=WeaponType.Mace,  minDamageMin=2, minDamageMax=5, maxDamageMin=7, maxDamageMax=12, attackSpeedMin=0.9f, attackSpeedMax=1.2f, baseNames=new[]{"Morningstar","Cudgel","War Mace"} },
            new WeaponTypeRule { type=WeaponType.Spear, minDamageMin=2, minDamageMax=4, maxDamageMin=7, maxDamageMax=12, attackSpeedMin=1.0f, attackSpeedMax=1.3f, baseNames=new[]{"Pike","Lance","Partisan"} },
            new WeaponTypeRule { type=WeaponType.Staff, minDamageMin=1, minDamageMax=3, maxDamageMin=5, maxDamageMax=9,  attackSpeedMin=1.0f, attackSpeedMax=1.3f, baseNames=new[]{"Quarterstaff","Mage Rod","Wand"} },
        };

        [Header("Name Parts")]
        public string[] adjectives = new[] { "Bronze", "Iron", "Steel", "Dark", "Blessed", "Ancient", "Runed", "Gilded" };
        public string[] materials  = new[] { "Bronze", "Iron", "Steel", "Obsidian", "Mithril", "Bone", "Ebony" };
        public string[] epithets   = new[] { "of Dawn", "of the Wolf", "of Ash", "of Storms", "of Night", "of the Vale" };

        [Header("Classes")]
        public string[] roles = new[] { "Tank", "Healer", "DPS", "Support" };
        public string[] primaryStatCombos = new[] { "STR/VIT", "DEX/AGI", "INT/WIS", "STR/DEX", "INT/DEX" };
        public string[] classBases = new[] { "Guardian", "Berserker", "Ranger", "Assassin", "Templar", "Warlock", "Arcanist", "Druid" };
        public string[] classPitches = new[]
        {
            "{NAME} are {ROLE}s who thrive in {THEME}, channeling {STATS} to overcome foes.",
            "As masters of {THEME}, {NAME} wield {STATS} to define the front line as a {ROLE}.",
            "{NAME} bend {THEME} to their will; a {ROLE} who relies on {STATS}."
        };

        public TierEntry RollTier(System.Random rng)
        {
            float total = tiers.Sum(t => t.weight);
            float roll = (float)(rng.NextDouble() * total);
            foreach (var t in tiers)
            {
                roll -= t.weight;
                if (roll <= 0) return t;
            }
            return tiers[0];
        }

        public WeaponTypeRule GetRule(WeaponType type) => weaponRules.FirstOrDefault(r => r.type == type) ?? weaponRules[0];

        public string MakeWeaponName(System.Random rng, string baseName, string tier)
        {
            var adj = Pick(adjectives, rng);
            var mat = Pick(materials, rng);
            var epi = rng.NextDouble() < 0.5 ? " " + Pick(epithets, rng) : "";
            var name = $"{adj} {mat} {baseName}{epi}";
            return name.Length > nameMaxLen ? name.Substring(0, nameMaxLen) : name;
        }

        public string MakeClassName(System.Random rng)
        {
            var baseName = Pick(classBases, rng);
            var adj = rng.NextDouble() < 0.4 ? Pick(adjectives, rng) + " " : "";
            var mat = rng.NextDouble() < 0.25 ? Pick(materials, rng) + " " : "";
            return (adj + mat + baseName).Trim();
        }

        public string MakeClassPitch(System.Random rng, string name, string role, string stats, string theme)
        {
            var t = Pick(classPitches, rng);
            return t.Replace("{NAME}", name).Replace("{ROLE}", role).Replace("{STATS}", stats).Replace("{THEME}", theme);
        }

        static T Pick<T>(T[] arr, System.Random rng) => arr.Length == 0 ? default : arr[rng.Next(arr.Length)];
    }
}
