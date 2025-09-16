using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    [CustomEditor(typeof(BiomeMaskArea))]
    public class VegetationMaskAreaEditor : VegetationStudioProBaseEditor
    {
        BiomeMaskArea biomeMaskArea;
        InspectorCurveEditor distanceCurveEditor;
        bool nodeChanged = false;

        SerializedProperty useNodeAutoSnapping;
        SerializedProperty layerMask;

        public void OnEnable()
        {
            InspectorCurveEditor.Settings settings = InspectorCurveEditor.Settings.DefaultSettings;
            distanceCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Distance };

            useNodeAutoSnapping = serializedObject.FindProperty("useNodeAutoSnapping");
            layerMask = serializedObject.FindProperty("GroundLayerMask");
        }

        public void OnDisable()
        {
            distanceCurveEditor.RemoveAll();
        }

        public override void OnInspectorGUI()
        {
            biomeMaskArea = (BiomeMaskArea)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            if (biomeMaskArea.enabled)
            {
                EditorGUILayout.LabelField("Node placement", labelStyle);
                EditorGUILayout.LabelField("Insert node: Ctrl-Click");
                EditorGUILayout.LabelField("Delete node: Ctrl-Shift-Click");
                biomeMaskArea.useEdgeDisableMode = EditorGUILayout.Toggle("Enable edge disable mode", biomeMaskArea.useEdgeDisableMode);
                EditorGUILayout.HelpBox("When enabled the delete function changes to the edge disable function\nEdge-lines between two disabled edge-nodes will not be included when calculating edge distances in rules and blending", MessageType.Info);
                EditorGUILayout.PropertyField(useNodeAutoSnapping, new GUIContent("Node auto snapping"));
                EditorGUILayout.PropertyField(layerMask, new GUIContent("Node snap-layers"));
                EditorGUILayout.HelpBox("Select the layers to be used when working on meshes/colliders", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Enable the mask to edit nodes", MessageType.Warning);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mask settings", labelStyle);
            biomeMaskArea.MaskName = EditorGUILayout.TextField("Mask name", biomeMaskArea.MaskName);
            biomeMaskArea.ShowArea = EditorGUILayout.Toggle("Show area", biomeMaskArea.ShowArea);
            biomeMaskArea.ShowHandles = EditorGUILayout.Toggle("Show handles", biomeMaskArea.ShowHandles);
            biomeMaskArea.BiomeType = (BiomeType)EditorGUILayout.EnumPopup("Select biome type", biomeMaskArea.BiomeType);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Generation settings", labelStyle);
            EditorGUILayout.HelpBox("Set the distance in which biomes can blend between each other\nThe area outside of the red box is used to blend the biomes", MessageType.Info);
            biomeMaskArea.BlendDistance = EditorGUILayout.Slider("Biome blend distance", biomeMaskArea.BlendDistance, 0, 500);
            biomeMaskArea.UseNoise = EditorGUILayout.Toggle("Use noise", biomeMaskArea.UseNoise);
            biomeMaskArea.NoiseScale = EditorGUILayout.Slider("Noise scale", biomeMaskArea.NoiseScale, 1, 500);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                SetMaskDirty();

            EditorGUILayout.HelpBox("The curves define the blend area in more detail\nGreen = selected biome mask / Red = other biomes\nY-Axis = Spawn chance / X-Axis = Distance from center", MessageType.Info);

            EditorGUILayout.LabelField("Vegetation blend curves", labelStyle);
            if (distanceCurveEditor.EditCurves(biomeMaskArea.BlendCurve, biomeMaskArea.InverseBlendCurve, this))
                SetMaskDirty();

            EditorGUILayout.LabelField("Terrain texture blend curve", labelStyle);
            if (GUILayout.Button("Generate mask's splatmap") && biomeMaskArea.gameObject.activeInHierarchy && biomeMaskArea.enabled)
                VegetationStudioManager.GenerateSplatMap(biomeMaskArea._currentMaskArea.MaskBounds, false);
            EditorGUILayout.HelpBox("This only updates the splat map based on this mask's area\nConsider using the \"TerrainSystemPro\" component or keybind-shortcut for a full refresh", MessageType.Info);

            if (distanceCurveEditor.EditCurve(biomeMaskArea.TextureBlendCurve, this))
                SetMaskDirty();

            GUILayout.EndVertical();
        }

        void SetMaskDirty()
        {
            biomeMaskArea.isDirty = true;
            EditorUtility.SetDirty(biomeMaskArea);
        }

        public virtual void OnSceneGUI()
        {
            biomeMaskArea = (BiomeMaskArea)target;

            if (biomeMaskArea.ShowHandles && biomeMaskArea.enabled)
            {
                Event currentEvent = Event.current;

                if (currentEvent.shift || currentEvent.control)
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                if (currentEvent.type == EventType.MouseUp && nodeChanged)
                {
                    VegetationStudioManager.GenerateSplatMap(biomeMaskArea._currentMaskArea.MaskBounds, true);
                    nodeChanged = false;
                }

                if (currentEvent.shift && currentEvent.control)
                    if (biomeMaskArea.useEdgeDisableMode == false)
                        for (int i = 0; i < biomeMaskArea.Nodes.Count; i++)
                        {
                            Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                            Vector3 worldSpaceNode = biomeMaskArea.transform.TransformPoint(biomeMaskArea.Nodes[i].Position);
                            float distance = Vector3.Distance(cameraPosition, worldSpaceNode);

                            Handles.color = Color.red;
                            if (Handles.Button(worldSpaceNode, Quaternion.LookRotation(worldSpaceNode - cameraPosition, Vector3.up), 0.030f * distance, 0.015f * distance, Handles.CircleHandleCap))
                            {
                                biomeMaskArea.DeleteNode(biomeMaskArea.Nodes[i]);
                                SetMaskDirty();
                            }
                        }
                    else
                        for (int i = 0; i < biomeMaskArea.Nodes.Count; i++)
                        {
                            Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                            Vector3 worldSpaceNode = biomeMaskArea.transform.TransformPoint(biomeMaskArea.Nodes[i].Position);
                            float distance = Vector3.Distance(cameraPosition, worldSpaceNode);

                            Handles.color = Color.yellow;
                            if (Handles.Button(worldSpaceNode, Quaternion.LookRotation(worldSpaceNode - cameraPosition, Vector3.up), 0.030f * distance, 0.015f * distance, Handles.CircleHandleCap))
                            {
                                biomeMaskArea.Nodes[i].DisableEdge = !biomeMaskArea.Nodes[i].DisableEdge;
                                SetMaskDirty();
                            }
                        }

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control && !currentEvent.shift && !currentEvent.alt)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 10000f);
                    for (int i = 0; i < hits.Length; i++)
                        if (hits[i].collider is TerrainCollider || biomeMaskArea.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                        {
                            biomeMaskArea.AddNode(hits[i].point);
                            currentEvent.Use();
                            SetMaskDirty();
                            break;
                        }
                }

                if (!currentEvent.shift && !currentEvent.alt)
                {
                    bool nodeChangeTemp = false;
                    Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;

                    for (int i = 0; i < biomeMaskArea.Nodes.Count; i++)
                    {
                        Vector3 worldSpaceNode = biomeMaskArea.transform.TransformPoint(biomeMaskArea.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);
                        if (distance > 1000 && biomeMaskArea.Nodes.Count > 50)
                            continue;

                        Vector3 newWorldSpaceNode = Handles.PositionHandle(worldSpaceNode, Quaternion.identity);
                        biomeMaskArea.Nodes[i].Position = biomeMaskArea.transform.InverseTransformPoint(newWorldSpaceNode);
                        if (worldSpaceNode != newWorldSpaceNode)
                            nodeChangeTemp = true;
                    }

                    if (nodeChangeTemp)
                    {
                        SetMaskDirty();
                        nodeChanged = true;
                    }
                }
            }

            if (biomeMaskArea.ShowArea && biomeMaskArea.Nodes.Count > 0)
            {
                if (useNodeAutoSnapping.boolValue == false)
                    return;

                Vector3[] worldPoints = new Vector3[biomeMaskArea.Nodes.Count];
                for (int i = 0; i < biomeMaskArea.Nodes.Count; i++)
                {
                    worldPoints[i] = biomeMaskArea.transform.TransformPoint(biomeMaskArea.Nodes[i].Position);
                    if (Vector3.Distance(worldPoints[i], SceneView.currentDrawingSceneView.camera.transform.position) > 1000)
                        return;
                }

                List<Vector3> worldPointsClosedList = new(worldPoints);
                worldPointsClosedList.Add(worldPointsClosedList[0]);

                if (biomeMaskArea.BlendDistance > 0.01f)
                {
                    List<Vector3> inflatedTreeListMax = PolygonUtility.InflatePolygon(worldPointsClosedList, -biomeMaskArea.BlendDistance, true);
                    PolygonUtility.AlignPointsWithTerrain(inflatedTreeListMax, true, biomeMaskArea.GroundLayerMask);

                    Handles.color = new Color(1, 0, 0, 1);
                    Handles.DrawAAPolyLine(2.5f, inflatedTreeListMax.ToArray());
                }
            }
        }
    }
}