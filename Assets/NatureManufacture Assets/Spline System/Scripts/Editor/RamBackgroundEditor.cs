using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

[InitializeOnLoad]
public class RamBackgroundEditor : MonoBehaviour
{
    static RamBackgroundEditor()
    {
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        //if (Event.current.type.ToString().Contains("Drag"))
        //    Debug.Log(Event.current.type);

        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            if (DragAndDrop.objectReferences.Length < 1)
                return;

            // --- SplineProfile -> RamSpline ---
            SplineProfile splineProfile = null;
            if (DragAndDrop.objectReferences[0] is SplineProfile)
            {
                splineProfile = (SplineProfile)DragAndDrop.objectReferences[0];

                GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false);
                if (go != null)
                {
                    RamSpline ramSpline = go.GetComponent<RamSpline>();

                    if (ramSpline != null)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor

                        if (Event.current.type == EventType.DragPerform)
                        {
                            Undo.RecordObject(ramSpline, "River changed");

                            // Create the editor and invoke its non-public ResetToProfile via reflection (avoids CS0122)
                            Editor ramSplineEditor = Editor.CreateEditor(ramSpline);

                            ramSpline.currentProfile = splineProfile;

                            try
                            {
                                MethodInfo mi = ramSplineEditor.GetType().GetMethod(
                                    "ResetToProfile",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                );
                                if (mi != null)
                                {
                                    mi.Invoke(ramSplineEditor, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"RamBackgroundEditor: Could not invoke ResetToProfile on RamSplineEditor. {ex.Message}");
                            }

                            ramSpline.GenerateSpline();
                            EditorUtility.SetDirty(ramSpline);
                            Editor.DestroyImmediate(ramSplineEditor);
                            DragAndDrop.AcceptDrag();
                            Event.current.Use();
                            return;
                        }
                    }
                }

                Event.current.Use();
            }

            // --- LakePolygonProfile -> LakePolygon ---
            LakePolygonProfile lakePolygonProfile = null;
            if (DragAndDrop.objectReferences[0] is LakePolygonProfile)
            {
                lakePolygonProfile = (LakePolygonProfile)DragAndDrop.objectReferences[0];

                GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

                if (go != null)
                {
                    LakePolygon lakePolygon = go.GetComponent<LakePolygon>();

                    if (lakePolygon != null)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor

                        if (Event.current.type == EventType.DragPerform)
                        {
                            Undo.RecordObject(lakePolygon, "Lake changed");

                            // Create the editor and invoke its non-public ResetToProfile via reflection (avoids CS0122)
                            Editor lakePolygonEditor = Editor.CreateEditor(lakePolygon);

                            lakePolygon.currentProfile = lakePolygonProfile;

                            try
                            {
                                MethodInfo mi = lakePolygonEditor.GetType().GetMethod(
                                    "ResetToProfile",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                );
                                if (mi != null)
                                {
                                    mi.Invoke(lakePolygonEditor, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"RamBackgroundEditor: Could not invoke ResetToProfile on LakePolygonEditor. {ex.Message}");
                            }

                            lakePolygon.GeneratePolygon();
                            EditorUtility.SetDirty(lakePolygon);
                            Editor.DestroyImmediate(lakePolygonEditor);

                            DragAndDrop.AcceptDrag();
                            Event.current.Use();
                            return;
                        }
                    }
                }

                Event.current.Use();
            }
        }
    }
}
