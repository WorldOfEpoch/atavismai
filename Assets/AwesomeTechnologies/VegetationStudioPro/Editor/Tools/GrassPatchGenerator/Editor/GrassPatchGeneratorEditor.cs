using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    [CustomEditor(typeof(GrassPatchGenerator))]
    public class GrassPatchGeneratorEditor : VegetationStudioProBaseEditor
    {
        [MenuItem("Window/Awesome Technologies/Tools/Add Grass Patch Generator to the hierarchy")]
        static void AddPatchGenerator()
        {
            GameObject go = new() { name = "GrassPatchGenerator" };
            go.AddComponent<GrassPatchGenerator>();
            if (VegetationStudioManager.Instance)
                go.transform.SetParent(VegetationStudioManager.Instance.transform);
            EditorGUIUtility.PingObject(go);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GrassPatchGenerator grassPatchGenerator = (GrassPatchGenerator)target;

            EditorGUI.BeginChangeCheck();

            if (grassPatchGenerator.showGenerationSection = VegetationPackageEditorTools.DrawHeader("Generation", grassPatchGenerator.showGenerationSection))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("General", labelStyle);
                grassPatchGenerator.randomSeed = EditorGUILayout.IntSlider("Seed", grassPatchGenerator.randomSeed, 1, 100);
                grassPatchGenerator.planeCount = EditorGUILayout.IntSlider("Plane count", grassPatchGenerator.planeCount, 1, 32);
                grassPatchGenerator.planeGap = EditorGUILayout.Slider("Plane gap", grassPatchGenerator.planeGap, 0.1f, 2f);
                grassPatchGenerator.planeWidth = EditorGUILayout.Slider("Plane width", grassPatchGenerator.planeWidth, 0f, 1f);
                grassPatchGenerator.planeHeight = EditorGUILayout.Slider("Plane height", grassPatchGenerator.planeHeight, 0f, 1f);
                EditorFunctions.FloatRangeField("Min/Max Scale", ref grassPatchGenerator.minScale, ref grassPatchGenerator.maxScale, 0.1f, 2f);
                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Resolution", labelStyle);
                grassPatchGenerator.planeWidthSegments = EditorGUILayout.IntSlider("Width segment amount", grassPatchGenerator.planeWidthSegments, 2, 8);
                grassPatchGenerator.planeHeightSegments = EditorGUILayout.IntSlider("Height segment amount", grassPatchGenerator.planeHeightSegments, 2, 8);
                EditorGUILayout.LabelField("Tris: " + grassPatchGenerator.GetMeshTriangleCount().ToString(), labelStyle);
                EditorGUILayout.LabelField("Verts: " + grassPatchGenerator.GetMeshVertexCount().ToString(), labelStyle);
                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Bending", labelStyle);
                grassPatchGenerator.minBendHeight = EditorGUILayout.Slider("Minimum bend height", grassPatchGenerator.minBendHeight, 0f, 1f);
                grassPatchGenerator.maxBendDistance = EditorGUILayout.Slider("Bend", grassPatchGenerator.maxBendDistance, 0f, 0.5f);
                grassPatchGenerator.curveOffset = EditorGUILayout.Slider("Curve", grassPatchGenerator.curveOffset, 0f, 0.5f);
                EditorGUILayout.EndVertical();
            }

            if (grassPatchGenerator.showMaterialSection = VegetationPackageEditorTools.DrawHeader("Material settings", grassPatchGenerator.showMaterialSection))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Textures -- Left = Main -- Right = Dry color noise", labelStyle);
                GUILayout.BeginHorizontal();
                grassPatchGenerator.mainTexture = (Texture2D)EditorGUILayout.ObjectField(grassPatchGenerator.mainTexture, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
                grassPatchGenerator.dryColorNoiseTexture = (Texture2D)EditorGUILayout.ObjectField(grassPatchGenerator.dryColorNoiseTexture, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Material settings", labelStyle);
                EditorGUILayout.HelpBox("Save the scene to apply the material toggle changes below\nTemporary materials don't apply state changes immediately", MessageType.Info);
                EditorGUILayout.HelpBox("More material settings can be found on the exported material\nCustom materials can be assigned after exporting as well", MessageType.Warning);
                grassPatchGenerator.textureCutoff = EditorGUILayout.Slider("Alpha cutoff", grassPatchGenerator.textureCutoff, 0f, 1f);
                grassPatchGenerator.healthyColor = EditorGUILayout.ColorField("Healthy color", grassPatchGenerator.healthyColor);
                grassPatchGenerator.toggleHueVariation = EditorGUILayout.Toggle("Use hue variation", grassPatchGenerator.toggleHueVariation);
                grassPatchGenerator.dryColor = EditorGUILayout.ColorField("Dry color", grassPatchGenerator.dryColor);
                grassPatchGenerator.dryColorNoiseScale = EditorGUILayout.Slider("Dry color noise scale", grassPatchGenerator.dryColorNoiseScale, 1, 1000);
                grassPatchGenerator.toggleDarkening = EditorGUILayout.Toggle("Use random darkening", grassPatchGenerator.toggleDarkening);
                grassPatchGenerator.randomDarkening = EditorGUILayout.Slider("Random darkening", grassPatchGenerator.randomDarkening, 0f, 1f);
                grassPatchGenerator.rootAmbient = EditorGUILayout.Slider("Root ambient", grassPatchGenerator.rootAmbient, 0f, 1f);
                EditorGUILayout.EndVertical();
            }

            if (grassPatchGenerator.showVertexColorSection = VegetationPackageEditorTools.DrawHeader("Vertex coloring -- Advanced", grassPatchGenerator.showVertexColorSection))
            {
                GUILayout.BeginVertical("box");
                grassPatchGenerator.bakePhase = EditorGUILayout.Toggle("Include phase", grassPatchGenerator.bakePhase);
                grassPatchGenerator.bakeBend = EditorGUILayout.Toggle("Include bending", grassPatchGenerator.bakeBend);
                grassPatchGenerator.bakeAo = EditorGUILayout.Toggle("Include occlusion", grassPatchGenerator.bakeAo);
                grassPatchGenerator.showVertexColors = EditorGUILayout.Toggle("Show vertex colors", grassPatchGenerator.showVertexColors);
                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Occlusion", labelStyle);
                grassPatchGenerator.aoCurve = EditorGUILayout.CurveField(grassPatchGenerator.aoCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
                EditorGUILayout.HelpBox("Horizontal: min/max height -- Vertical: bottom no ambient => top max ambient\nEnable \"Show vertex colors\" to visualize this", MessageType.Info);
                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Wind bending", labelStyle);
                grassPatchGenerator.bendCurve = EditorGUILayout.CurveField(grassPatchGenerator.bendCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
                EditorGUILayout.HelpBox("Horizontal: min/max height -- Vertical: bottom do not bend => top max bend\nEnable \"Show vertex colors\" to visualize this", MessageType.Info);
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                grassPatchGenerator.UpdateGrassPatchGenerator();
                EditorUtility.SetDirty(grassPatchGenerator);
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Export", labelStyle);
            if (GUILayout.Button("Single prefab"))
                grassPatchGenerator.ExportPrefab(false);

            if (GUILayout.Button("Prefab w/ LODs"))
                grassPatchGenerator.ExportPrefab(true);
            EditorGUILayout.EndVertical();
        }
    }
}