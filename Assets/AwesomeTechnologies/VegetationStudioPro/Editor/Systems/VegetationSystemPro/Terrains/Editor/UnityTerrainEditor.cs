using AwesomeTechnologies.VegetationStudio;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(UnityTerrain))]
    public class UnityTerrainEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty terrainSourceID;
        SerializedProperty disableUnityTreesID;
        SerializedProperty autoAddToVegetationSystemID;

        void OnEnable()
        {
            terrainSourceID = serializedObject.FindProperty("TerrainSourceID");
            disableUnityTreesID = serializedObject.FindProperty("DisableTerrainTreesAndDetails");
            autoAddToVegetationSystemID = serializedObject.FindProperty("AutoAddToVegetationSystem");
        }

        void OnSceneGUI()
        {
            if (Event.current.type == EventType.ValidateCommand) // refresh vegetation instances after painting undo/redo operations on the terrain
                VegetationStudioManager.RefreshTerrainHeightMap();  // refresh all terrains of all vegetation systems
        }

        public override void OnInspectorGUI()
        {
            UnityTerrain unityTerrain = (UnityTerrain)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(terrainSourceID, new GUIContent("Terrain Source ID"));
            EditorGUILayout.HelpBox("The Terrain Source ID can be set differently for each terrain and is used for spawning rules", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                VegetationStudioManager.ClearCache(unityTerrain.TerrainBounds);

            EditorGUILayout.PropertyField(disableUnityTreesID, new GUIContent("Disable Unity's trees and details"));
            EditorGUILayout.HelpBox("When enabled Unity's trees and details get disabled on scene start", MessageType.Info);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Terrain streaming", labelStyle);
            EditorGUILayout.PropertyField(autoAddToVegetationSystemID, new GUIContent("Add/Remove on Enable/Disable"));
            EditorGUILayout.HelpBox("When enabled the terrain adds/removes itself when enabled/disabled\nIn addition the \"Automatic calculation\" must be disabled in the \"Terrains\" tab", MessageType.Info);
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(unityTerrain);
        }
    }
}