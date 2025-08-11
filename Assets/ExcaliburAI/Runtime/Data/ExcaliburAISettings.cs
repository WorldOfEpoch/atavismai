using UnityEngine;

namespace ExcaliburAI.Data
{
    [CreateAssetMenu(menuName="ExcaliburAI/Settings")]
    public class ExcaliburAISettings : ScriptableObject
    {
        [Header("Atavism Mode")]
        public bool atavismMode = true;

        [Tooltip("Where Atavism keeps item icons (sprites) in your Unity project.")]
        public string atavismIconsFolder = "Assets/Atavism/Content/Icons/Generated";

        [Tooltip("Where to drop CSVs you’ll import with the Atavism Editor.")]
        public string atavismCsvFolder   = "Assets/Atavism/Content/CSV";
    }
}
