using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [CustomEditor(typeof(VegetationMaskLine))]
    public class VegetationMaskLineEditor : VegetationMaskEditor
    {
        private bool showLineSegmentSettings;

        public override void OnInspectorGUI()
        {
            VegetationMaskLine vegetationMaskLine = (VegetationMaskLine)target;
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Line settings", labelStyle);

            vegetationMaskLine.LineWidth = EditorGUILayout.Slider("Line width", vegetationMaskLine.LineWidth, 0.1f, 50);

            if (showLineSegmentSettings = EditorGUILayout.Toggle("Enable per segment overrides", showLineSegmentSettings))
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Nodes", labelStyle);

                for (int i = 0; i < vegetationMaskLine.Nodes.Count - 1; i++)
                {
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Node #" + (i + 1), labelStyle, GUILayout.Width(60));
                    EditorGUIUtility.labelWidth = 50;
                    vegetationMaskLine.Nodes[i].Active = EditorGUILayout.Toggle("Enabled", vegetationMaskLine.Nodes[i].Active, GUILayout.Width(65));
                    EditorGUIUtility.labelWidth = 85;
                    vegetationMaskLine.Nodes[i].OverrideWidth = EditorGUILayout.Toggle("Custom width", vegetationMaskLine.Nodes[i].OverrideWidth, GUILayout.Width(105));
                    EditorGUIUtility.labelWidth = 50;
                    vegetationMaskLine.Nodes[i].CustomWidth = EditorGUILayout.Slider("", vegetationMaskLine.Nodes[i].CustomWidth, 0.1f, 50);

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationMaskLine.UpdateVegetationMask();
                EditorUtility.SetDirty(vegetationMaskLine);
            }
        }

        public override void OnSceneGUI()
        {
            VegetationMaskLine vegetationMaskLine = (VegetationMaskLine)target;
            base.OnSceneGUI();

            if (vegetationMaskLine.ShowArea == false)
                return; // skip when not supposed to draw outlines -- mask line specific

            Vector3[] worldPoints = new Vector3[vegetationMaskLine.Nodes.Count];
            for (int i = 0; i < vegetationMaskLine.Nodes.Count; i++)
                worldPoints[i] = vegetationMaskLine.transform.TransformPoint(vegetationMaskLine.Nodes[i].Position);

            DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, 0, new(1, 1, 1, 1));

            if ((vegetationMask.AdditionalGrassPerimiter > 0.1f || vegetationMask.AdditionalGrassPerimiterMax > 0.1f) && vegetationMask.RemoveGrass)
            {
                DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalGrassPerimiter, new(0, 1, 0, 1));
                if (vegetationMask.AdditionalGrassPerimiterMax > vegetationMask.AdditionalGrassPerimiter)
                    DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalGrassPerimiterMax, new(0, 1, 0, 1));
            }

            if ((vegetationMask.AdditionalPlantPerimiter > 0.1f || vegetationMask.AdditionalPlantPerimiterMax > 0.1f) && vegetationMask.RemovePlants)
            {
                DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalPlantPerimiter, new(0, 0, 1, 1));
                if (vegetationMask.AdditionalPlantPerimiterMax > vegetationMask.AdditionalPlantPerimiter)
                    DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalPlantPerimiterMax, new(0, 0, 1, 1));
            }

            if ((vegetationMask.AdditionalObjectPerimiter > 0.1f || vegetationMask.AdditionalObjectPerimiterMax > 0.1f) && vegetationMask.RemoveObjects)
            {
                DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalObjectPerimiter, new(1, 1, 0, 1));
                if (vegetationMask.AdditionalObjectPerimiterMax > vegetationMask.AdditionalObjectPerimiter)
                    DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalObjectPerimiterMax, new(1, 1, 0, 1));
            }

            if ((vegetationMask.AdditionalLargeObjectPerimiter > 0.1f || vegetationMask.AdditionalLargeObjectPerimiterMax > 0.1f) && vegetationMask.RemoveLargeObjects)
            {
                DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalLargeObjectPerimiter, new(1, 1, 1, 1));
                if (vegetationMask.AdditionalLargeObjectPerimiterMax > vegetationMask.AdditionalLargeObjectPerimiter)
                    DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalLargeObjectPerimiterMax, new(1, 1, 1, 1));
            }

            if ((vegetationMask.AdditionalTreePerimiter > 0.1f || vegetationMask.AdditionalTreePerimiterMax > 0.1f) && vegetationMask.RemoveTrees)
            {
                DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalTreePerimiter, new(1, 0, 0, 1));
                if (vegetationMask.AdditionalTreePerimiterMax > vegetationMask.AdditionalTreePerimiter)
                    DrawLineOutline(worldPoints, vegetationMaskLine.LineWidth, vegetationMask.AdditionalTreePerimiterMax, new(1, 0, 0, 1));
            }
        }
    }
}