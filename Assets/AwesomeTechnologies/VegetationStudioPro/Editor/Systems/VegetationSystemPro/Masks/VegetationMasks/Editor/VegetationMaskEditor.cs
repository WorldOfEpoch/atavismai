using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    public class VegetationMaskEditor : VegetationStudioProBaseEditor
    {
        public VegetationMask vegetationMask;
        private int selectedVegetationTypeIndex;

        SerializedProperty useNodeAutoSnapping;
        SerializedProperty layerMask;

        private void OnEnable()
        {
            useNodeAutoSnapping = serializedObject.FindProperty("useNodeAutoSnapping");
            layerMask = serializedObject.FindProperty("GroundLayerMask");
        }

        public override void OnInspectorGUI()
        {
            vegetationMask = (VegetationMask)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            if (vegetationMask.enabled)
            {
                EditorGUILayout.LabelField("Node placement", labelStyle);
                EditorGUILayout.LabelField("Insert node: Ctrl-Click");
                EditorGUILayout.LabelField("Delete node: Ctrl-Shift-Click");
                EditorGUILayout.PropertyField(useNodeAutoSnapping, new GUIContent("Node auto snapping"));
                EditorGUILayout.PropertyField(layerMask, new GUIContent("Node snap-layers"));
                EditorGUILayout.HelpBox("Select the layers to snap-on when working on meshes/colliders", MessageType.Info);
            }
            else
                EditorGUILayout.HelpBox("Enable the mask to edit nodes", MessageType.Warning);
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                SetMaskDirty();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Mask settings", labelStyle);
            vegetationMask.MaskName = EditorGUILayout.TextField("Mask name", vegetationMask.MaskName);
            vegetationMask.ShowArea = EditorGUILayout.Toggle("Show area", vegetationMask.ShowArea);
            vegetationMask.ShowHandles = EditorGUILayout.Toggle("Show handles", vegetationMask.ShowHandles);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(vegetationMask);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("General vegetation removal", labelStyle);
            EditorGUILayout.HelpBox("The radius type sets how accuracte/aggressive the vegetation instances should be removed\n" +
                "The additional perimeter range uses noise to blend between min/max\nDisable the noise by setting it to 1 or set the min/max to the same value each", MessageType.Info);

            if (vegetationMask.RemoveGrass = EditorGUILayout.Toggle("Remove grass", vegetationMask.RemoveGrass))
            {
                vegetationMask.eMaskRadiusGrass = (EMaskRadiusType)EditorGUILayout.EnumPopup("Radius type", vegetationMask.eMaskRadiusGrass);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref vegetationMask.AdditionalGrassPerimiter, ref vegetationMask.AdditionalGrassPerimiterMax, 0, 50);
                vegetationMask.NoiseScaleGrass = EditorGUILayout.Slider("Perimeter noise scale", vegetationMask.NoiseScaleGrass, 0, 50);
            }

            EditorGUILayout.Space();

            if (vegetationMask.RemovePlants = EditorGUILayout.Toggle("Remove plants", vegetationMask.RemovePlants))
            {
                vegetationMask.eMaskRadiusPlant = (EMaskRadiusType)EditorGUILayout.EnumPopup("Radius type", vegetationMask.eMaskRadiusPlant);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref vegetationMask.AdditionalPlantPerimiter, ref vegetationMask.AdditionalPlantPerimiterMax, 0, 50);
                vegetationMask.NoiseScalePlant = EditorGUILayout.Slider("Perimeter noise scale", vegetationMask.NoiseScalePlant, 0, 50);
            }

            EditorGUILayout.Space();

            if (vegetationMask.RemoveObjects = EditorGUILayout.Toggle("Remove objects", vegetationMask.RemoveObjects))
            {
                vegetationMask.eMaskRadiusObject = (EMaskRadiusType)EditorGUILayout.EnumPopup("Radius type", vegetationMask.eMaskRadiusObject);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref vegetationMask.AdditionalObjectPerimiter, ref vegetationMask.AdditionalObjectPerimiterMax, 0, 50);
                vegetationMask.NoiseScaleObject = EditorGUILayout.Slider("Perimeter noise scale", vegetationMask.NoiseScaleObject, 0, 50);
            }

            EditorGUILayout.Space();

            if (vegetationMask.RemoveLargeObjects = EditorGUILayout.Toggle("Remove large objects", vegetationMask.RemoveLargeObjects))
            {
                vegetationMask.eMaskRadiusLargeObject = (EMaskRadiusType)EditorGUILayout.EnumPopup("Radius type", vegetationMask.eMaskRadiusLargeObject);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref vegetationMask.AdditionalLargeObjectPerimiter, ref vegetationMask.AdditionalLargeObjectPerimiterMax, 0, 50);
                vegetationMask.NoiseScaleLargeObject = EditorGUILayout.Slider("Perimeter noise scale", vegetationMask.NoiseScaleLargeObject, 0, 50);
            }

            EditorGUILayout.Space();

            if (vegetationMask.RemoveTrees = EditorGUILayout.Toggle("Remove trees", vegetationMask.RemoveTrees))
            {
                vegetationMask.eMaskRadiusTree = (EMaskRadiusType)EditorGUILayout.EnumPopup("Radius type", vegetationMask.eMaskRadiusTree);
                EditorFunctions.FloatRangeField("Additional perimeter range", ref vegetationMask.AdditionalTreePerimiter, ref vegetationMask.AdditionalTreePerimiterMax, 0, 50);
                vegetationMask.NoiseScaleTree = EditorGUILayout.Slider("Perimeter noise scale", vegetationMask.NoiseScaleTree, 0, 50);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Forced localized vegetation placement", labelStyle);
            if (vegetationMask.IncludeVegetationType = EditorGUILayout.Toggle("Include vegetation", vegetationMask.IncludeVegetationType))
            {
                if (GUILayout.Button("Add vegetation type"))
                {
                    vegetationMask.VegetationTypeList.Add(new());
                    selectedVegetationTypeIndex = vegetationMask.VegetationTypeList.Count - 1;
                }

                string[] packageNameList = new string[vegetationMask.VegetationTypeList.Count];
                for (int i = 0; i < vegetationMask.VegetationTypeList.Count; i++)
                    packageNameList[i] = (i + 1).ToString() + ". Item";

                if (vegetationMask.VegetationTypeList.Count > 0)
                {
                    selectedVegetationTypeIndex = EditorGUILayout.Popup("Selected include item", selectedVegetationTypeIndex, packageNameList);
                    if (selectedVegetationTypeIndex > vegetationMask.VegetationTypeList.Count - 1)
                        selectedVegetationTypeIndex = vegetationMask.VegetationTypeList.Count - 1;

                    GUILayout.BeginVertical("box");
                    vegetationMask.VegetationTypeList[selectedVegetationTypeIndex].Index = (VegetationTypeIndex)EditorGUILayout.EnumPopup("Vegetation type", vegetationMask.VegetationTypeList[selectedVegetationTypeIndex].Index);
                    vegetationMask.VegetationTypeList[selectedVegetationTypeIndex].Density = EditorGUILayout.Slider("Density", vegetationMask.VegetationTypeList[selectedVegetationTypeIndex].Density, 0, 5);
                    vegetationMask.VegetationTypeList[selectedVegetationTypeIndex].Size = EditorGUILayout.Slider("Size", vegetationMask.VegetationTypeList[selectedVegetationTypeIndex].Size, 0.1f, 5);
                    if (GUILayout.Button("Delete selected item"))
                        vegetationMask.VegetationTypeList.RemoveAt(selectedVegetationTypeIndex);
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                SetMaskDirty();
        }

        public virtual void OnSceneGUI()
        {
            vegetationMask = (VegetationMask)target;

            if (vegetationMask.ShowHandles && vegetationMask.enabled)
            {
                Event currentEvent = Event.current;

                if (currentEvent.shift || currentEvent.control)
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                //else
                //HandleUtility.Repaint();

                if (currentEvent.shift && currentEvent.control)
                    for (int i = 0; i < vegetationMask.Nodes.Count; i++)
                    {
                        Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
                        Vector3 worldSpaceNode = vegetationMask.transform.TransformPoint(vegetationMask.Nodes[i].Position);
                        float distance = Vector3.Distance(cameraPosition, worldSpaceNode);

                        Handles.color = Color.red;
                        if (Handles.Button(worldSpaceNode, Quaternion.LookRotation(worldSpaceNode - cameraPosition, Vector3.up), 0.015f * distance, 0.015f * distance, Handles.CircleHandleCap))
                        {
                            vegetationMask.DeleteNode(vegetationMask.Nodes[i]);
                            SetMaskDirty();
                        }
                    }

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control && !currentEvent.shift && !currentEvent.alt)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 10000f);
                    for (int i = 0; i < hits.Length; i++)
                        if (hits[i].collider is TerrainCollider || vegetationMask.GroundLayerMask.Contains(hits[i].collider.gameObject.layer))
                        {
                            vegetationMask.AddNode(hits[i].point);
                            currentEvent.Use();
                            SetMaskDirty();
                            break;
                        }
                }

                if (!currentEvent.shift)
                {
                    bool nodeChanged = false;

                    for (int i = 0; i < vegetationMask.Nodes.Count; i++)
                    {
                        Vector3 worldSpaceNode = vegetationMask.transform.TransformPoint(vegetationMask.Nodes[i].Position);
                        if (Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, worldSpaceNode) > 1000 && vegetationMask.Nodes.Count > 50)
                            continue;

                        Vector3 newWorldSpaceNode = Handles.PositionHandle(worldSpaceNode, Quaternion.identity);
                        vegetationMask.Nodes[i].Position = vegetationMask.transform.InverseTransformPoint(newWorldSpaceNode);
                        if (worldSpaceNode != newWorldSpaceNode)
                            nodeChanged = true;
                    }

                    if (nodeChanged)
                        SetMaskDirty();
                }
            }
        }

        void SetMaskDirty()
        {
            vegetationMask.isDirty = true;
            EditorUtility.SetDirty(vegetationMask);
        }

        internal void DrawAreaOutline(List<Vector3> _worldPointList, float _width, Color _color)
        {
            if (useNodeAutoSnapping.boolValue == false)
                return;

            for (int i = 0; i < _worldPointList.Count - 1; i++)
                if (Vector3.Distance(_worldPointList[i], SceneView.currentDrawingSceneView.camera.transform.position) > 1000)
                    return;

            List<Vector3> newInflatedList = PolygonUtility.InflatePolygon(_worldPointList, _width, true);
            PolygonUtility.AlignPointsWithTerrain(newInflatedList, true, vegetationMask.GroundLayerMask);

            Handles.color = _color;
            Handles.DrawAAPolyLine(2.5f, newInflatedList.ToArray());
        }

        internal void DrawLineOutline(Vector3[] _worldPoints, float _width, float _additionalWidth, Color _color)
        {
            if (useNodeAutoSnapping.boolValue == false)
                return;

            for (int i = 0; i < _worldPoints.Length - 1; i++)
            {
                if (Vector3.Distance(_worldPoints[i], SceneView.currentDrawingSceneView.camera.transform.position) > 1000)
                    continue;

                if (vegetationMask.Nodes[i].Active == false)
                    continue;

                float lineWidth = _width + _additionalWidth * 2;
                if (vegetationMask.Nodes[i].OverrideWidth)
                    lineWidth = vegetationMask.Nodes[i].CustomWidth + _additionalWidth * 2;

                List<Vector3> newInflatedList = PolygonUtility.InflatePolygon(new() { _worldPoints[i], _worldPoints[i + 1] }, lineWidth * 0.5f, false);
                PolygonUtility.AlignPointsWithTerrain(newInflatedList, true, vegetationMask.GroundLayerMask);

                Handles.color = _color;
                Handles.DrawAAPolyLine(2.5f, newInflatedList.ToArray());
            }
        }
    }
}