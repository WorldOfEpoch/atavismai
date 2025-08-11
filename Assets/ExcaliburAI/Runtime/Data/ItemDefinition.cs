using UnityEngine;

namespace ExcaliburAI.Data
{
    public enum WeaponType { Sword, Axe, Dagger, Mace, Spear, Bow, Staff, Other }

    [CreateAssetMenu(menuName = "ExcaliburAI/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;              // deterministic if you want
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Gameplay")]
        public string Tier;            // e.g., Common/Rare/Epic/Legendary
        public WeaponType WeaponType;
        public int MinDamage;
        public int MaxDamage;
        public float AttackSpeed = 1f;

        [Header("Visuals")]
        public Sprite Icon;
        public string IconPrompt;      // used to (re)generate icon

        // convenience
        public string SafeName => string.IsNullOrWhiteSpace(DisplayName) ? name : DisplayName;
    }
}
