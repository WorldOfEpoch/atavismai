#if UNITY_SPLINES_EDITOR
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    [CustomEditor(typeof(BiomeMaskSpline))]
    public class BiomeMaskSplineEditor : VegetationStudioProBaseEditor
    {
        BiomeMaskSpline biomeMaskSpline;
        SerializedProperty splineContainer;
        SerializedProperty eSplineSampleResolution;
        SerializedProperty tangentMode;
        //SerializedProperty useKnotAutoSnapping;
        SerializedProperty layerMask;
        SerializedProperty biomeType;
        SerializedProperty pathWidthList;
        SerializedProperty blendBalance;

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
            biomeType = serializedObject.FindProperty("biomeType");
            pathWidthList = serializedObject.FindProperty("pathWidthList");
            blendBalance = serializedObject.FindProperty("blendBalance");
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
            biomeMaskSpline = (BiomeMaskSpline)target;
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("Box");

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(splineContainer, new GUIContent("Spline container"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(biomeMaskSpline.tangentMode == TangentMode.Linear);
            EditorGUILayout.PropertyField(eSplineSampleResolution, new GUIContent("Spline sampling resolution"));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Clear spline container"))
            {
                Object[] undoObjects = { biomeMaskSpline, biomeMaskSpline.splineContainer };
                Undo.RecordObjects(undoObjects, "BiomeMaskSpline -- Clear spline container");
                biomeMaskSpline.ClearSplineContainer();
            }
            EditorGUILayout.PropertyField(tangentMode, new GUIContent("Tangent mode"));

            EditorGUI.BeginChangeCheck();
            biomeMaskSpline.useKnotAutoSnapping = EditorGUILayout.Toggle(new GUIContent("Knot auto snapping"), biomeMaskSpline.useKnotAutoSnapping);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(biomeMaskSpline.splineContainer, "BiomeMaskSpline -- Spline container changed (autoSnapping)");   // auto snapping (non-destructive) compatibility due to custom "offset" logic
                biomeMaskSpline.shallSnapKnotsOnce = true;
            }
            EditorGUILayout.PropertyField(layerMask, new GUIContent("Knot snap-layers"));
            EditorGUILayout.HelpBox("Select the layers to snap-on when working on meshes/colliders", MessageType.Info);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Settings -- Path spline", labelStyle);
            if (biomeMaskSpline.splineContainer.Splines.Count == 0)
                EditorGUILayout.HelpBox("No splines have been added to the spline container", MessageType.Warning);

            if (biomeMaskSpline.splineContainer.Splines.Count > 5)
                biomeMaskSpline.showSplinePathWidthMenu = EditorGUILayout.Toggle("", biomeMaskSpline.showSplinePathWidthMenu);
            EditorGUILayout.EndHorizontal();

            if (biomeMaskSpline.showSplinePathWidthMenu || biomeMaskSpline.splineContainer.Splines.Count <= 5)
                for (int i = 0; i < math.min(biomeMaskSpline.splineContainer.Splines.Count, biomeMaskSpline.pathWidthList.Count); i++)
                {
                    bool isClosed = biomeMaskSpline.splineContainer.Splines[i].Closed;
                    EditorGUI.BeginDisabledGroup(isClosed);
                    EditorGUILayout.PropertyField(pathWidthList.GetArrayElementAtIndex(i), new GUIContent(isClosed ? "Closed" : "Path width " + i));
                    EditorGUI.EndDisabledGroup();
                }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings -- Biome", labelStyle);
            EditorGUILayout.PropertyField(biomeType, new GUIContent("Biome type"));
            EditorGUILayout.PropertyField(blendBalance, new GUIContent("Blend balance"));

            if (GUILayout.Button("Generate per spline splatmap"))
                biomeMaskSpline.GenerateSplatmap(false);

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                biomeMaskSpline.isDirty = true; // set to update the internals of the spline mask > underlying mask/-s
                EditorUtility.SetDirty(biomeMaskSpline);    // notify to allow on-disk saving -- force full UI/SceneView refresh
            }
        }

        public void OnSceneGUI()
        {
            biomeMaskSpline = (BiomeMaskSpline)target;
            if (Event.current.button == 0 && (Event.current.type == EventType.MouseUp) && biomeMaskSpline.hasSplineContainerChanged)
            {
                biomeMaskSpline.GenerateSplatmap(true);
                biomeMaskSpline.hasSplineContainerChanged = false;
            }
        }

        void OnSplineContainerChanged(SplineContainer _splineContainer, int _index)
        {
            biomeMaskSpline = (BiomeMaskSpline)target;

            if (_index == 5)
                biomeMaskSpline.showSplinePathWidthMenu = true;

            biomeMaskSpline.isDirty = true; // set to update the internals of the spline mask > underlying mask/-s
            EditorUtility.SetDirty(biomeMaskSpline);    // notify to allow on-disk saving -- force full UI/SceneView refresh
        }

        void OnSplineContainerChanged(SplineContainer _splineContainer, int _previousIndex, int _newIndex)
        {
            biomeMaskSpline = (BiomeMaskSpline)target;
            biomeMaskSpline.isDirty = true; // set to update the internals of the spline mask > underlying mask/-s
            EditorUtility.SetDirty(biomeMaskSpline);    // notify to allow on-disk saving -- force full UI/SceneView refresh
        }

        void OnSplineChanged(Spline _spline, int _intData, SplineModification _modification)
        {
            biomeMaskSpline = (BiomeMaskSpline)target;

            bool isValidSpline = false; // only update the correct spline
            for (int i = 0; i < biomeMaskSpline.splineContainer.Splines.Count; i++)
                if (isValidSpline = _spline == biomeMaskSpline.splineContainer.Splines[i])
                    break;
            if (isValidSpline == false)
                return;

            if (_modification == SplineModification.KnotModified)
            {
                Undo.RecordObject(biomeMaskSpline.splineContainer, "BiomeMaskSpline -- Spline container changed (knotModified)");   // "Bezier" mode non-destructive save
                if (biomeMaskSpline.haveKnotsBeenSnappedOnce == false)  // skip running the event multiple times due to implicit event "re-running"
                    biomeMaskSpline.shallSnapKnotsOnce = true;  // allow auto snap logic to run once (done implicitly due to compatibility reasons)
                else
                    biomeMaskSpline.haveKnotsBeenSnappedOnce = false;
            }

            biomeMaskSpline.isDirty = true; // set to update the internals of the spline mask > underlying mask/-s
            biomeMaskSpline.hasSplineContainerChanged = true;   // set to notify "splat map generation" "on mouse release" UI check
            EditorUtility.SetDirty(biomeMaskSpline);    // notify to allow on-disk saving -- force full UI/SceneView refresh
        }
    }
}
#endif