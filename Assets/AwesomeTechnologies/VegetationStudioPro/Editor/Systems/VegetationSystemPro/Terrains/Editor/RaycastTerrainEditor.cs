using AwesomeTechnologies.VegetationStudio;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(RaycastTerrain))]
    public class RaycastTerrainEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty raycastTerrainBounds;
        SerializedProperty layerMask;
        SerializedProperty terrainSourceID;
        SerializedProperty autoAddToVegegetationSystem;

        private void OnEnable()
        {
            raycastTerrainBounds = serializedObject.FindProperty("RaycastTerrainBounds");
            layerMask = serializedObject.FindProperty("RaycastLayerMask");
            terrainSourceID = serializedObject.FindProperty("TerrainSourceID");
            autoAddToVegegetationSystem = serializedObject.FindProperty("AutoAddToVegegetationSystem");
        }

        public override void OnInspectorGUI()
        {
            RaycastTerrain raycastTerrain = (RaycastTerrain)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            serializedObject.Update();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Info", labelStyle);
            EditorGUILayout.HelpBox("The raycast terrain is designed to be in a fixed location\nChanging the position requires a system refresh", MessageType.Warning);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            EditorGUILayout.PropertyField(raycastTerrainBounds, new GUIContent("Area"));

            if (EditorGUI.EndChangeCheck())
                VegetationStudioManager.RefreshTerrainArea(raycastTerrain.TerrainBounds);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(layerMask, new GUIContent("Node snap-layers"));
            EditorGUILayout.HelpBox("Select the layers to snap-on when working with meshes/colliders", MessageType.Info);

            EditorGUILayout.PropertyField(terrainSourceID, new GUIContent("Terrain Source ID"));
            EditorGUILayout.HelpBox("The Terrain Source ID can be set differently for each terrain and is used for spawning rules", MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                VegetationStudioManager.ClearCache(raycastTerrain.TerrainBounds);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Terrain streaming", labelStyle);
            EditorGUILayout.PropertyField(autoAddToVegegetationSystem, new GUIContent("Add/Remove on Enable/Disable"));
            EditorGUILayout.HelpBox("When enabled the terrain adds/removes itself when enabled/disabled\nIn addition the \"Automatic calculation\" must be disabled in the \"Terrains\" tab", MessageType.Info);
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(raycastTerrain);
        }
    }
}