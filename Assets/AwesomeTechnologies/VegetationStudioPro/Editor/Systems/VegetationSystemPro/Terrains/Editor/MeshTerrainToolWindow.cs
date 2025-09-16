using System;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEngine;

public class MeshTerrainToolWindow : EditorWindow
{
    [MenuItem("Window/Awesome Technologies/Tools/Open mesh terrain setup tool")]
    static void Init()
    {
        MeshTerrainToolWindow window = (MeshTerrainToolWindow)GetWindow(typeof(MeshTerrainToolWindow));
        window.Show();
    }

    private readonly List<MeshRenderer> _meshRendererList = new();
    private VegetationSystemPro vspSys;
    bool addAsChild = false;
    bool addToSystem = false;
    bool combineMeshes = false;
    TerrainSourceID terrainSourceID = TerrainSourceID.TerrainSourceID1;

    bool skipLODs = false;
    bool multiLevelSpawning = false;
    bool terrainStreaming = false;

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Add meshes", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        GameObject go = EditorGUILayout.ObjectField("Add mesh", null, typeof(GameObject), true) as GameObject;

        if (EditorGUI.EndChangeCheck())
            if (go != null)
            {
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    if (skipLODs)
                    {
                        if (_meshRendererList[i].name.ToUpper().Contains("LOD1")) continue;
                        if (_meshRendererList[i].name.ToUpper().Contains("LOD2")) continue;
                        if (_meshRendererList[i].name.ToUpper().Contains("LOD3")) continue;
                    }

                    MeshRenderer meshRenderer = Selection.gameObjects[i].GetComponent<MeshRenderer>();
                    if (meshRenderer)
                        _meshRendererList.Add(meshRenderer);
                }

                if (Selection.gameObjects.Length == 0)
                {
                    MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                        _meshRendererList.Add(meshRenderer);
                    else
                    {
                        MeshRenderer[] ms = go.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < ms.Length; i++)
                        {
                            if (ms[i] == null)
                                continue;

                            if (skipLODs)
                            {
                                if (ms[i].name.ToUpper().Contains("LOD1")) continue;
                                if (ms[i].name.ToUpper().Contains("LOD2")) continue;
                                if (ms[i].name.ToUpper().Contains("LOD3")) continue;
                            }

                            _meshRendererList.Add(ms[i]);
                        }
                    }
                }
            }

        ///
        ///
        /// Add settings

        EditorGUILayout.HelpBox("Add any mesh with a valid mesh renderer component\nMultiple separate gameObjects can be added\nSelect multiple gameObjects at the same time and drop them into the slot", MessageType.Info);

        skipLODs = EditorGUILayout.Toggle("Skip LODs on import", skipLODs);
        EditorGUILayout.HelpBox("When enabled the import skips all meshes with names containing LOD1, LOD2 or LOD3", MessageType.Info);

        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Mesh renderers", EditorStyles.boldLabel);
        if (GUILayout.Button("Clear list"))
            _meshRendererList.Clear();

        for (int i = 0; i < _meshRendererList.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            _meshRendererList[i] = EditorGUILayout.ObjectField("Mesh renderer", _meshRendererList[i], typeof(MeshRenderer), true) as MeshRenderer;
            if (EditorGUI.EndChangeCheck())
            {
                if (_meshRendererList[i] == null)
                {
                    _meshRendererList.RemoveAt(i);
                    GUILayout.EndVertical();
                    return;
                }
            }
        }
        GUILayout.EndVertical();

        ///
        ///
        /// Create settings

        if (_meshRendererList.Count > 0)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Create", EditorStyles.boldLabel);

            combineMeshes = EditorGUILayout.Toggle("Merge to single terrain", combineMeshes);

            addToSystem = EditorGUILayout.Toggle("Add to a system", addToSystem);
            if (addToSystem)
                vspSys = EditorGUILayout.ObjectField("Vegetation System Pro", vspSys, typeof(VegetationSystemPro), true) as VegetationSystemPro;

            addAsChild = EditorGUILayout.Toggle("Add as child", addAsChild);

            GUILayout.Label("", EditorStyles.boldLabel);
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            multiLevelSpawning = EditorGUILayout.Toggle("Multi level spawning", multiLevelSpawning);
            EditorGUILayout.HelpBox("Multi level spawning spawns vegetation on all intersecting levels of the added meshes", MessageType.Info);
            terrainStreaming = EditorGUILayout.Toggle("Terrain streaming", terrainStreaming);
            EditorGUILayout.HelpBox("When set the terrain will add and remove itself when enabled/disabled\nThis only works if automatic area calculation is disabled on the vegetation system and is only done in playmode and builds", MessageType.Info);
            terrainSourceID = (TerrainSourceID)EditorGUILayout.EnumPopup("Select terrain source id", terrainSourceID);

            GUILayout.Label("", EditorStyles.boldLabel);
            if (GUILayout.Button("Create mesh terrains"))
            {
                if (combineMeshes)
                    CreateMergedMeshTerrain(addToSystem, terrainSourceID, vspSys);
                else
                    CreateMeshTerrains(addToSystem, addAsChild, terrainSourceID, vspSys);
            }

            GUILayout.EndVertical();
        }
    }

    void CreateMeshTerrains(bool _addToVegetationSystem, bool _addAsChild, TerrainSourceID _terrainSourceID, VegetationSystemPro _vspsys)   // create "separate" mesh terrains
    {
        for (int i = 0; i < _meshRendererList.Count; i++)
        {
            if (_meshRendererList[i] == null)
                continue;

            string filename = "MeshTerrainData_" + _meshRendererList[i].name + "_" + Guid.NewGuid() + ".asset";
            GameObject go = new("MeshTerrain_" + _meshRendererList[i].name);
            if (_addAsChild)
                if (go)
                    go.transform.SetParent(_meshRendererList[i].transform);

            if (AssetDatabase.IsValidFolder("Assets/MeshTerrainData") == false)
                AssetDatabase.CreateFolder("Assets", "MeshTerrainData");

            MeshTerrainData meshTerrainData = CreateInstance<MeshTerrainData>();

            AssetDatabase.CreateAsset(meshTerrainData, "Assets/MeshTerrainData/" + filename);
            MeshTerrainData loadedMeshTerrainData = AssetDatabase.LoadAssetAtPath<MeshTerrainData>("Assets/MeshTerrainData/" + filename);

            MeshTerrain meshTerrain = go.AddComponent<MeshTerrain>();
            meshTerrain.MeshTerrainData = loadedMeshTerrainData;
            meshTerrain.AddMeshRenderer(_meshRendererList[i].gameObject, _terrainSourceID);

            meshTerrain.MultiLevelRaycast = multiLevelSpawning;
            meshTerrain.AutoAddToVegegetationSystem = terrainStreaming;

            meshTerrain.GenerateMeshTerrain();

            if (_addToVegetationSystem)
                VegetationStudioManager.AddTerrain(go, true, _vspsys);
        }
    }

    void CreateMergedMeshTerrain(bool _addToVegetationSystem, TerrainSourceID _terrainSourceID, VegetationSystemPro _vspsys)    // create "merged" mesh terrains
    {
        string filename = "MeshTerrainData_" + _meshRendererList[0].name + "_" + Guid.NewGuid() + ".asset";
        GameObject go = new("MeshTerrain_" + _meshRendererList[0].name);

        if (AssetDatabase.IsValidFolder("Assets/MeshTerrainData") == false)
            AssetDatabase.CreateFolder("Assets", "MeshTerrainData");
        MeshTerrainData meshTerrainData = CreateInstance<MeshTerrainData>();

        AssetDatabase.CreateAsset(meshTerrainData, "Assets/MeshTerrainData/" + filename);
        MeshTerrainData loadedMeshTerrainData = AssetDatabase.LoadAssetAtPath<MeshTerrainData>("Assets/MeshTerrainData/" + filename);

        MeshTerrain meshTerrain = go.AddComponent<MeshTerrain>();
        meshTerrain.MeshTerrainData = loadedMeshTerrainData;

        for (int i = 0; i < _meshRendererList.Count; i++)
        {
            if (_meshRendererList[i] == null)
                continue;

            meshTerrain.AddMeshRenderer(_meshRendererList[i].gameObject, _terrainSourceID);
        }

        if (_addToVegetationSystem)
            VegetationStudioManager.AddTerrain(go, true, _vspsys);

        meshTerrain.MultiLevelRaycast = multiLevelSpawning;
        meshTerrain.AutoAddToVegegetationSystem = terrainStreaming;

        meshTerrain.GenerateMeshTerrain();
    }
}