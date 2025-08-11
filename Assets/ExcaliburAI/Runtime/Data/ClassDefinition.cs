using UnityEngine;

namespace ExcaliburAI.Data
{
    [CreateAssetMenu(menuName = "ExcaliburAI/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        public string Id;
        public string ClassName;
        [TextArea] public string FantasyPitch;   // flavor text
        public string Role;                      // e.g., Tank/Healer/DPS
        public string PrimaryStats;              // e.g., STR/DEX/INT
        public Sprite Icon;
        public string IconPrompt;
    }
}
