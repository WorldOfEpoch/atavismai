using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using System;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    [CustomEditor(typeof(MeshTerrain))]
    public class MeshTerrainEditor : VegetationStudioProBaseEditor
    {
        private MeshTerrain meshTerrain;
        private static readonly string[] TabNames = { "Settings", "Mesh terrain sources" };

        public override void OnInspectorGUI()
        {
            meshTerrain = (MeshTerrain)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            meshTerrain.CurrentTabIndex = GUILayout.SelectionGrid(meshTerrain.CurrentTabIndex, TabNames, 2, EditorStyles.toolbarButton);
            switch (meshTerrain.CurrentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawSourceInspector();
                    break;
            }
        }

        void DrawSettingsInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Info", labelStyle);
            EditorGUILayout.HelpBox("The mesh terrain is designed to be in a fixed location\nChanges to added meshes requires regeneration of the mesh terrain data", MessageType.Warning);
            MeshTerrainData meshTerrainData = meshTerrain.MeshTerrainData;
            if (meshTerrainData != null)
            {
                EditorGUILayout.LabelField("Nodes: " + meshTerrainData.lNodes.Count, labelStyle);
                EditorGUILayout.LabelField("Triangles: " + meshTerrainData.lPrims.Count, labelStyle);
            }
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Configuration", labelStyle);
            GUILayout.BeginHorizontal();
            meshTerrain.MeshTerrainData = EditorGUILayout.ObjectField("Mesh terrain data", meshTerrain.MeshTerrainData, typeof(MeshTerrainData), true) as MeshTerrainData;
            if (GUILayout.Button("Create"))
                CreateMeshTerrainDataObject();
            GUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Create and drop a \"MeshTerrainData\" object here\nThe file stores the needed data to position vegetation", MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                if (meshTerrain.MeshTerrainData)
                {
                    meshTerrain.Init(); // light refresh only => won't work for "un-matching" data
                    VegetationStudioManager.ClearCache(meshTerrain.TerrainBounds);
                }

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            meshTerrain.MultiLevelRaycast = EditorGUILayout.Toggle("Multi level spawning", meshTerrain.MultiLevelRaycast);
            EditorGUILayout.HelpBox("Multi level spawning enables vegetation to spawn on all levels of \"depth\" of the added meshes", MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                VegetationStudioManager.ClearCache(meshTerrain.TerrainBounds);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Terrain streaming", labelStyle);
            meshTerrain.AutoAddToVegegetationSystem = EditorGUILayout.Toggle("Add/Remove on Enable/Disable", meshTerrain.AutoAddToVegegetationSystem);
            EditorGUILayout.HelpBox("When enabled the terrain adds/removes itself when enabled/disabled\nIn addition the \"Automatic calculation\" must be disabled in the \"Terrains\" tab", MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(meshTerrain);
        }

        void DrawSourceInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add terrain sources", labelStyle);
            bool hasAddedMesh = false;
            GUILayout.BeginHorizontal();
            DropZoneTools.DrawMeshTerrainDropZone(DropZoneType.MeshRenderer, meshTerrain, ref hasAddedMesh);
            GUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Drop any GameObject with a functional mesh renderer component", MessageType.Info);
            meshTerrain.Filterlods = EditorGUILayout.Toggle("Skip LODs on import", meshTerrain.Filterlods);
            EditorGUILayout.HelpBox("When enabled the import skips all meshes with names containing LOD1, LOD2 or LOD3", MessageType.Info);
            GUILayout.EndVertical();
            if (hasAddedMesh)
            {
                EditorUtility.SetDirty(target);
                return;
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Included sources", labelStyle);
            meshTerrain.ShowDebugInfo = EditorGUILayout.Toggle("Show debug info", meshTerrain.ShowDebugInfo);
            EditorGUILayout.HelpBox("Debug info colors each terrain source based on the selected \"TerrainSourceID\"", MessageType.Info);
            EditorGUILayout.Space();

            int removeIndex = -1;

            if (meshTerrain.MeshTerrainMeshSourceList.Count > 0)
            {
                EditorGUILayout.LabelField("Meshes", labelStyle);

                for (int i = 0; i < meshTerrain.MeshTerrainMeshSourceList.Count; i++)
                {
                    MeshTerrainMeshSource meshTerrainMeshSource = meshTerrain.MeshTerrainMeshSourceList[i];

                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Mesh: ", labelStyle, GUILayout.Width(50));
                    meshTerrainMeshSource.MeshRenderer = EditorGUILayout.ObjectField("", meshTerrainMeshSource.MeshRenderer, typeof(MeshRenderer), true, GUILayout.Width(150)) as MeshRenderer;
                    meshTerrainMeshSource.TerrainSourceID = (TerrainSourceID)EditorGUILayout.EnumPopup("", meshTerrainMeshSource.TerrainSourceID, GUILayout.Width(150));
                    if (GUILayout.Button("Remove"))
                        removeIndex = i;
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        meshTerrain.MeshTerrainMeshSourceList[i] = meshTerrainMeshSource;
                        meshTerrain.NeedGeneration = true;
                        EditorUtility.SetDirty(target);
                    }
                }
            }

            if (removeIndex > -1)
            {
                meshTerrain.MeshTerrainMeshSourceList.RemoveAt(removeIndex);
                meshTerrain.NeedGeneration = true;
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndVertical();

            if (meshTerrain.MeshTerrainData)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Generate mesh terrain data", labelStyle);
                if (meshTerrain.NeedGeneration)
                    EditorGUILayout.HelpBox("The mesh sources have changed\nMesh terrain data needs to be regenerated", MessageType.Warning);
                if (GUILayout.Button("Generate mesh terrain data"))
                    meshTerrain.GenerateMeshTerrain();
                EditorGUILayout.HelpBox("The generated data will be stored in the assigned MeshTerrainData file", MessageType.Info);
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("A mesh terrain data file needs to created and assigned", MessageType.Warning);
            }
        }

        void CreateMeshTerrainDataObject()
        {
            MeshTerrainData meshTerrainData = CreateInstance<MeshTerrainData>();

            if (AssetDatabase.IsValidFolder("Assets/MeshTerrainData") == false)
                AssetDatabase.CreateFolder("Assets", "MeshTerrainData");

            string filename = meshTerrain.name + "_MeshTerrainData_" + Guid.NewGuid() + ".asset";
            AssetDatabase.CreateAsset(meshTerrainData, "Assets/MeshTerrainData/" + filename);

            meshTerrain.MeshTerrainData = AssetDatabase.LoadAssetAtPath<MeshTerrainData>("Assets/MeshTerrainData/" + filename);

            EditorUtility.SetDirty(meshTerrain);
        }
    }
}