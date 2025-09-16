using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [CustomEditor(typeof(VegetationMaskArea))]
    public class VegetationMaskAreaEditor : VegetationMaskEditor
    {
        public override void OnInspectorGUI()
        {
            VegetationMaskArea vegetationMaskArea = (VegetationMaskArea)target;
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Automatic outline calculation", labelStyle);

            EditorGUILayout.HelpBox("Analyzes the meshes of this gameObject and its children to calculate an outline", MessageType.Info);
            vegetationMaskArea.tolerance = EditorGUILayout.Slider("Accuracy", vegetationMaskArea.tolerance, 0.1f, 2f);
            if (GUILayout.Button("Calculate outline"))
            {
                vegetationMaskArea.GenerateHullNodes(vegetationMaskArea.tolerance);
                EditorUtility.SetDirty(vegetationMaskArea);
            }

            GUILayout.EndVertical();
        }

        public override void OnSceneGUI()
        {
            VegetationMaskArea vegetationMaskArea = (VegetationMaskArea)target;
            base.OnSceneGUI();

            if (vegetationMaskArea.ShowArea == false)
                return; // skip when supposed not to draw outlines -- mask area specific

            Vector3[] worldPoints = new Vector3[vegetationMask.Nodes.Count];
            for (int i = 0; i < vegetationMask.Nodes.Count; i++)
                worldPoints[i] = vegetationMask.transform.TransformPoint(vegetationMask.Nodes[i].Position);
            List<Vector3> worldPointsList = new(worldPoints);

            if ((vegetationMask.AdditionalGrassPerimiter > 0.1f || vegetationMask.AdditionalGrassPerimiterMax > 0.1f) && vegetationMask.RemoveGrass)
            {
                DrawAreaOutline(worldPointsList, vegetationMask.AdditionalGrassPerimiter, new(0, 1, 0, 1));
                if (vegetationMask.AdditionalGrassPerimiterMax > vegetationMask.AdditionalGrassPerimiter)
                    DrawAreaOutline(worldPointsList, vegetationMask.AdditionalGrassPerimiterMax, new(0, 1, 0, 1));
            }

            if ((vegetationMask.AdditionalPlantPerimiter > 0.1f || vegetationMask.AdditionalPlantPerimiterMax > 0.1f) && vegetationMask.RemovePlants)
            {
                DrawAreaOutline(worldPointsList, vegetationMask.AdditionalPlantPerimiter, new(0, 0, 1, 1));
                if (vegetationMask.AdditionalPlantPerimiterMax > vegetationMask.AdditionalPlantPerimiter)
                    DrawAreaOutline(worldPointsList, vegetationMask.AdditionalPlantPerimiterMax, new(0, 0, 1, 1));
            }

            if ((vegetationMask.AdditionalObjectPerimiter > 0.1f || vegetationMask.AdditionalObjectPerimiterMax > 0.1f) && vegetationMask.RemoveObjects)
            {
                DrawAreaOutline(worldPointsList, vegetationMask.AdditionalObjectPerimiter, new(1, 1, 0, 1));
                if (vegetationMask.AdditionalObjectPerimiterMax > vegetationMask.AdditionalObjectPerimiter)
                    DrawAreaOutline(worldPointsList, vegetationMask.AdditionalObjectPerimiterMax, new(1, 1, 0, 1));
            }

            if ((vegetationMask.AdditionalLargeObjectPerimiter > 0.1f || vegetationMask.AdditionalLargeObjectPerimiterMax > 0.1f) && vegetationMask.RemoveLargeObjects)
            {
                DrawAreaOutline(worldPointsList, vegetationMask.AdditionalLargeObjectPerimiter, new(1, 1, 1, 1));
                if (vegetationMask.AdditionalLargeObjectPerimiterMax > vegetationMask.AdditionalLargeObjectPerimiter)
                    DrawAreaOutline(worldPointsList, vegetationMask.AdditionalLargeObjectPerimiterMax, new(1, 1, 1, 1));
            }

            if ((vegetationMask.AdditionalTreePerimiter > 0.1f || vegetationMask.AdditionalTreePerimiterMax > 0.1f) && vegetationMask.RemoveTrees)
            {
                DrawAreaOutline(worldPointsList, vegetationMask.AdditionalTreePerimiter, new(1, 0, 0, 1));
                if (vegetationMask.AdditionalTreePerimiterMax > vegetationMask.AdditionalTreePerimiter)
                    DrawAreaOutline(worldPointsList, vegetationMask.AdditionalTreePerimiterMax, new(1, 0, 0, 1));
            }
        }
    }
}