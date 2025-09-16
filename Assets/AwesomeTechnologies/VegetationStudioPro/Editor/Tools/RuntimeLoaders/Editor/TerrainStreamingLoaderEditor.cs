using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(TerrainStreamingLoader))]
    public class TerrainStreamingLoaderEditor : Editor
    {
        SerializedProperty removeTerrains;
        SerializedProperty floatingOriginAnchor;

        private void OnEnable()
        {
            removeTerrains = serializedObject.FindProperty("removeTerrains");
            floatingOriginAnchor = serializedObject.FindProperty("floatingOriginAnchor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("This component helps automate terrain streaming to improve editor workflows\n" +
                "It automates:\n- Terrains get removed from the list on scene start (this is sometimes needed)\n- Automatic calculation gets disabled\n- The chosen origin override gets assigned\nAll actions are temporary and get reset on scene stop", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(removeTerrains, new GUIContent("Remove terrains"));
            EditorGUILayout.PropertyField(floatingOriginAnchor, new GUIContent("Origin transform override"));
            EditorGUILayout.HelpBox("If no transform is set the already assigned transform gets used", MessageType.Info);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}