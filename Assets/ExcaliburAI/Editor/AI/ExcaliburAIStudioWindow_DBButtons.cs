#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ExcaliburAI.AI
{
    public partial class ExcaliburAIStudioWindow
    {
        void OnGUI_DBButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("DB Settings", GUILayout.Width(110))) DbSettingsWindow.Open();
            if (GUILayout.Button("Refresh Snapshot", GUILayout.Width(140)))
            {
                string s = AtavismDbExt.SnapshotSummary();
                Log("DB Snapshot: " + s);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
