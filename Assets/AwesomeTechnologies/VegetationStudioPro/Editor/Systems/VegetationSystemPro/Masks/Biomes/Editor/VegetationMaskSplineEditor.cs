#if UNITY_SPLINES_EDITOR
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace AwesomeTechnologies.Vegetation.Masks
{
    [CustomEditor(typeof(VegetationMaskSpline))]
    public class VegetationMaskSplineEditor : VegetationStudioProBaseEditor
    {
        VegetationMaskSpline vegetationMaskSpline;
        SerializedProperty splineContainer;
        SerializedProperty eSplineSampleResolution;
        SerializedProperty tangentMode;
        //SerializedProperty useKnotAutoSnapping;
        SerializedProperty layerMask;
        SerializedProperty pathWidthList;
        SerializedProperty eMaskRadiusGrass;
        SerializedProperty eMaskRadiusPlants;
        SerializedProperty eMaskRadiusObjects;
        SerializedProperty eMaskRadiusLargeObjects;
        SerializedProperty eMaskRadiusTrees;
        SerializedProperty includeVegetationItems;
        SerializedProperty includeVegetationItemList;
        public int selectedIncludeVegetationItemIndex;

        private void OnEnable()
        {
            SplineContainer.SplineAdded += OnSplineContainerChanged;
            SplineContainer.SplineRemoved += OnSplineContainerChanged;
            SplineContainer.SplineReordered += OnSplineContainerChanged;
            Spline.Changed += OnSplineChanged;
            splineContainer = serializedObject.FindProperty("splineContainer");
            eSplineSampleResolution = serializedObject.FindProperty("eSplineSampleResolution");
            tangentMode = serializedObject.FindProperty("tangentMode");
            //useKnotAutoSnapping = serializedObject.FindProperty("useKnotAutoSnapping");
            layerMask = serializedObject.FindProperty("layerMask");
            pathWidthList = serializedObject.FindProperty("pathWidthList");
            eMaskRadiusGrass = serializedObject.FindProperty("eMaskRadiusGrass");
            eMaskRadiusPlants = serializedObject.FindProperty("eMaskRadiusPlants");
            eMaskRadiusObjects = serializedObject.FindProperty("eMaskRadiusObjects");
            eMaskRadiusLargeObjects = serializedObject.FindProperty("eMaskRadiusLargeObjects");
            eMaskRadiusTrees = serializedObject.FindProperty("eMaskRadiusTrees");
            includeVegetationItems = serializedObject.FindProperty("includeVegetationItems");
            includeVegetationItemList = serializedObject.FindProperty("includeVegetationItemList");
        }

        private void OnDisable()
        {
            SplineContainer.SplineAdded -= OnSplineContainerChanged;
            SplineContainer.SplineRemoved -= OnSplineContainerChanged;
            SplineContainer.SplineReordered -= OnSplineContainerChanged;
            Spline.Changed -= OnSplineChanged;
        }

        public override void OnInspectorGUI()
        {
            vegetationMaskSpline = (VegetationMaskSpline)target;
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("Box");

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(splineContainer, new GUIContent("Spline container"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(vegetationMaskSpline.tangentMode == TangentMode.Linear);
            EditorGUILayout.PropertyField(eSplineSampleResolution, new GUIContent("Spline sampling resolution"));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Clear spline container"))
            {
                Object[] undoObjects = { vegetationMaskSpline, vegetationMaskSpline.splineContainer };
                Undo.RecordObjects(undoObjects, "VegetationMaskSpline -- Clear spline container");
                vegetationMaskSpline.ClearSplineContainer();
            }
            EditorGUILayout.PropertyField(tangentMode, new GUIContent("Tangent mode"));

            EditorGUI.BeginChangeCheck();
            vegetationMaskSpline.useKnotAutoSnapping = EditorGUILayout.Toggle(new GUIContent("Knot auto snapping"), vegetationMaskSpline.useKnotAutoSnapping);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(vegetationMaskSpline.splineContainer, "VegetationMaskSpline -- Spline container changed (autoSnapping)"); // auto snapping (non-destructive) compatibility due to custom "offset" logic
                vegetationMaskSpline.shallSnapKnotsOnce = true;
            }
            EditorGUILayout.PropertyField(layerMask, new GUIContent("Knot snap-layers"));
            EditorGUILayout.HelpBox("Select the layers to snap-on when working on meshes/colliders", MessageType.Info);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Settings -- Path spline", labelStyle);
            if (vegetationMaskSpline.splineContainer.Splines.Count == 0)
                EditorGUILayout.HelpBox("No splines have been added to the spline container", MessageType.Warning);

            if (vegetationMaskSpline.splineContainer.Splines.Count > 5)
                vegetationMaskSpline.showSplinePathWidthMenu = EditorGUILayout.Toggle("", vegetationMaskSpline.showSplinePathWidthMenu);
            EditorGUILayout.EndHorizontal();

            if (vegetationMaskSpline.showSplinePathWidthMenu || vegetationMaskSpline.splineContainer.Splines.Count <= 5)
                for (int i = 0; i < math.min(vegetationMaskSpline.splineContainer.Splines.Count, vegetationMaskSpline.pathWidthList.Count); i++)
                {
                    bool isClosed = vegetationMaskSpline.splineContainer.Splines[i].Closed;
                    EditorGUI.BeginDisabledGroup(isClosed);
                    EditorGUILayout.PropertyField(pathWidthList.GetArrayElementAtIndex(i), new GUIContent(isClosed ? "Closed" : "Path width " + i));
                    EditorGUI.EndDisabledGroup();
                }

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!(vegetationMaskSpline.splineContainer != null && vegetationMaskSpline.splineContainer.Spline != null && vegetationMaskSpline.splineContainer.Spline.Closed));
            EditorGUILayout.LabelField("Settings -- Closed spline", labelStyle);
            if (GUILayout.Button("Align first spline to child-meshes"))
                vegetationMaskSpline.AlignSplineToMeshes();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings -- Exclude mask", labelStyle);
            EditorGUILayout.LabelField("Removal accuracy type", labelStyle);
            EditorGUILayout.PropertyField(eMaskRadiusGrass, new GUIContent("Grass"));
            EditorGUILayout.PropertyField(eMaskRadiusPlants, new GUIContent("Plants"));
            EditorGUILayout.PropertyField(eMaskRadiusObjects, new GUIContent("Objects"));
            EditorGUILayout.PropertyField(eMaskRadiusLargeObjects, new GUIContent("Large objects"));
            EditorGUILayout.PropertyField(eMaskRadiusTrees, new GUIContent("Trees"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings -- Include mask", labelStyle);
            EditorGUILayout.PropertyField(includeVegetationItems, new GUIContent("Force-Include vegetation"));
            if (includeVegetationItems.boolValue)
            {
                if (GUILayout.Button("Add vegetation type"))
                {
                    Undo.RecordObject(vegetationMaskSpline, "VegetationMaskSpline -- Added include mode item");
                    vegetationMaskSpline.includeVegetationItemList.Add(new());
                    selectedIncludeVegetationItemIndex = vegetationMaskSpline.includeVegetationItemList.Count - 1;
                }

                string[] packageNameList = new string[vegetationMaskSpline.includeVegetationItemList.Count];
                for (int i = 0; i < vegetationMaskSpline.includeVegetationItemList.Count; i++)
                    packageNameList[i] = (i + 1).ToString() + ". Item";

                if (vegetationMaskSpline.includeVegetationItemList.Count > 0 && vegetationMaskSpline.includeVegetationItemList.Count == includeVegetationItemList.arraySize)
                {
                    selectedIncludeVegetationItemIndex = EditorGUILayout.Popup("Selected include item", selectedIncludeVegetationItemIndex, packageNameList);
                    if (selectedIncludeVegetationItemIndex > vegetationMaskSpline.includeVegetationItemList.Count - 1)
                        selectedIncludeVegetationItemIndex = vegetationMaskSpline.includeVegetationItemList.Count - 1;

                    GUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(includeVegetationItemList.GetArrayElementAtIndex(selectedIncludeVegetationItemIndex).FindPropertyRelative("Index"), new GUIContent("Vegetation type"));
                    EditorGUILayout.PropertyField(includeVegetationItemList.GetArrayElementAtIndex(selectedIncludeVegetationItemIndex).FindPropertyRelative("Density"), new GUIContent("Density"));
                    EditorGUILayout.PropertyField(includeVegetationItemList.GetArrayElementAtIndex(selectedIncludeVegetationItemIndex).FindPropertyRelative("Size"), new GUIContent("Size"));
                    if (GUILayout.Button("Delete selected item"))
                    {
                        Undo.RecordObject(vegetationMaskSpline, "VegetationMaskSpline -- Removed include mode item");
                        vegetationMaskSpline.includeVegetationItemList.RemoveAt(selectedIncludeVegetationItemIndex);
                    }
                    GUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationMaskSpline.isDirty = true;    // set to update the internals of the spline mask > underlying mask/-s
                EditorUtility.SetDirty(vegetationMaskSpline);   // notify to allow on-disk saving -- force full UI/SceneView refresh
            }
        }

        void OnSplineContainerChanged(SplineContainer _splineContainer, int _index)
        {
            vegetationMaskSpline = (VegetationMaskSpline)target;

            if (_index == 5)
                vegetationMaskSpline.showSplinePathWidthMenu = true;

            vegetationMaskSpline.isDirty = true;    // set to update the internals of the spline mask > underlying mask/-s
            EditorUtility.SetDirty(vegetationMaskSpline);   // notify to allow on-disk saving -- force full UI/SceneView refresh
        }

        void OnSplineContainerChanged(SplineContainer _splineContainer, int _previousIndex, int _newIndex)
        {
            vegetationMaskSpline = (VegetationMaskSpline)target;
            vegetationMaskSpline.isDirty = true;    // set to update the internals of the spline mask > underlying mask/-s
            EditorUtility.SetDirty(vegetationMaskSpline);   // notify to allow on-disk saving -- force full UI/SceneView refresh
        }

        void OnSplineChanged(Spline _spline, int _intData, SplineModification _modification)
        {
            vegetationMaskSpline = (VegetationMaskSpline)target;

            bool isValidSpline = false; // only update the correct spline
            for (int i = 0; i < vegetationMaskSpline.splineContainer.Splines.Count; i++)
                if (isValidSpline = _spline == vegetationMaskSpline.splineContainer.Splines[i])
                    break;
            if (isValidSpline == false)
                return;

            if (_modification == SplineModification.KnotModified)
            {
                Undo.RecordObject(vegetationMaskSpline.splineContainer, "VegetationMaskSpline -- Spline container changed (knotModified)");  // "Bezier" mode non-destructive save
                if (vegetationMaskSpline.haveKnotsBeenSnappedOnce == false) // skip running the event multiple times due to implicit event "re-running"
                    vegetationMaskSpline.shallSnapKnotsOnce = true; // allow auto snap logic to run once (done implicitly due to compatibility reasons)
                else
                    vegetationMaskSpline.haveKnotsBeenSnappedOnce = false;
            }

            vegetationMaskSpline.isDirty = true;    // set to update the internals of the spline mask > underlying mask/-s
            EditorUtility.SetDirty(vegetationMaskSpline);   // notify to allow on-disk saving -- force full UI/SceneView refresh
        }
    }
}
#endif