using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.VegetationSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [CustomEditor(typeof(VegetationBeacon))]
    public class VegetationBeaconEditor : VegetationStudioProBaseEditor
    {
        private bool displayRadiusGizmo;
        private InspectorCurveEditor distanceCurveEditor;
        private int vegetationTypeIndex;

        SerializedProperty removeGrass;
        SerializedProperty removePlants;
        SerializedProperty removeObjects;
        SerializedProperty removeLargeObjects;
        SerializedProperty removeTrees;
        SerializedProperty radius;
        SerializedProperty blendFactor;

        public void OnEnable()
        {
            InspectorCurveEditor.Settings settings = InspectorCurveEditor.Settings.DefaultSettings;
            distanceCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Falloff };

            removeGrass = serializedObject.FindProperty("RemoveGrass");
            removePlants = serializedObject.FindProperty("RemovePlants");
            removeObjects = serializedObject.FindProperty("RemoveObjects");
            removeLargeObjects = serializedObject.FindProperty("RemoveLargeObjects");
            removeTrees = serializedObject.FindProperty("RemoveTrees");
            radius = serializedObject.FindProperty("Radius");
            blendFactor = serializedObject.FindProperty("blendFactor");
        }

        public void OnDisable()
        {
            distanceCurveEditor.RemoveAll();
        }

        public override void OnInspectorGUI()
        {
            VegetationBeacon vegetationBeacon = (VegetationBeacon)target;
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            displayRadiusGizmo = EditorGUILayout.Toggle("Show radius/handles", displayRadiusGizmo);
            EditorGUILayout.PropertyField(radius, new GUIContent("Radius"));
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Removal settings", labelStyle);
            EditorGUILayout.PropertyField(blendFactor, new GUIContent("Radius"));
            EditorGUILayout.PropertyField(removeGrass, new GUIContent("Remove grass"));
            EditorGUILayout.PropertyField(removePlants, new GUIContent("Remove plant"));
            EditorGUILayout.PropertyField(removeObjects, new GUIContent("Remove objects"));
            EditorGUILayout.PropertyField(removeLargeObjects, new GUIContent("Remove large objects"));
            EditorGUILayout.PropertyField(removeTrees, new GUIContent("Remove trees"));
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationBeacon.UpdateVegetationBeacon();
                EditorUtility.SetDirty(vegetationBeacon);
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Generation settings", labelStyle);
            EditorGUILayout.LabelField("Falloff curve", labelStyle);
            if (distanceCurveEditor.EditCurve(vegetationBeacon.FalloffCurve, this))
            {
                vegetationBeacon.UpdateVegetationBeacon();
                EditorUtility.SetDirty(vegetationBeacon);
            }

            EditorGUI.BeginChangeCheck();

            Keyframe selectedKeyDistanceFalloff = distanceCurveEditor.GetSelection().Keyframe ?? new();
            if (distanceCurveEditor.GetSelection().Keyframe != null)
            {
                int index = distanceCurveEditor.GetSelection().KeyframeIndex;
                float time = math.round(EditorGUILayout.Slider("Distance from center", selectedKeyDistanceFalloff.time, 0, 1) * 100) / 100;
                float value = math.round(EditorGUILayout.Slider("Density", selectedKeyDistanceFalloff.value, 0, 1) * 100) / 100;
                float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeyDistanceFalloff.inTangent, -5, 5) * 100) / 100;
                float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeyDistanceFalloff.outTangent, -5, 5) * 100) / 100;
                if (index > 0)
                    time = math.max(vegetationBeacon.FalloffCurve.keys[index - 1].time + 0.0001f, time);    // safety "clamp" else keys delete themselves when having the same value
                if (index < vegetationBeacon.FalloffCurve.keys.Length - 1)
                    time = math.min(vegetationBeacon.FalloffCurve.keys[index + 1].time - 0.0001f, time);    // safety "clamp" else keys delete themselves when having the same value
                vegetationBeacon.FalloffCurve.MoveKey(index, new Keyframe(time, value, inTangent, outTangent));
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Forced localized vegetation placement", labelStyle);

            if (GUILayout.Button("Add vegetation type"))
            {
                vegetationBeacon.VegetationTypeList.Add(new());
                vegetationTypeIndex = vegetationBeacon.VegetationTypeList.Count - 1;
            }

            string[] packageNameList = new string[vegetationBeacon.VegetationTypeList.Count];
            for (int i = 0; i < vegetationBeacon.VegetationTypeList.Count; i++)
                packageNameList[i] = (i + 1).ToString() + ". Item";

            if (vegetationBeacon.VegetationTypeList.Count > 0)
            {
                if (vegetationTypeIndex > vegetationBeacon.VegetationTypeList.Count - 1)
                    vegetationTypeIndex = vegetationBeacon.VegetationTypeList.Count - 1;
                vegetationTypeIndex = EditorGUILayout.Popup("Selected item", vegetationTypeIndex, packageNameList);

                GUILayout.BeginVertical("box");
                vegetationBeacon.VegetationTypeList[vegetationTypeIndex].Index = (VegetationTypeIndex)EditorGUILayout.EnumPopup("Vegetation type", vegetationBeacon.VegetationTypeList[vegetationTypeIndex].Index);
                vegetationBeacon.VegetationTypeList[vegetationTypeIndex].Density = EditorGUILayout.Slider("Density", vegetationBeacon.VegetationTypeList[vegetationTypeIndex].Density, 0, 5);
                vegetationBeacon.VegetationTypeList[vegetationTypeIndex].Size = EditorGUILayout.Slider("Size", vegetationBeacon.VegetationTypeList[vegetationTypeIndex].Size, 0.1f, 5);
                if (GUILayout.Button("Delete selected item"))
                    vegetationBeacon.VegetationTypeList.RemoveAt(vegetationTypeIndex);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationBeacon.UpdateVegetationBeacon();
                EditorUtility.SetDirty(vegetationBeacon);
            }
        }

        public void OnSceneGUI()
        {
            if (displayRadiusGizmo == false)
                return;

            VegetationBeacon vegetationBeacon = (VegetationBeacon)target;

            EditorGUI.BeginChangeCheck();

            Vector3 terrainPosition = GetTerrainPosition(vegetationBeacon.transform.position);
            vegetationBeacon.Radius = math.clamp(Handles.RadiusHandle(Quaternion.identity, terrainPosition, vegetationBeacon.Radius, true), 0, 150);
            Handles.color = new Color(1, 0, 0, 0.1f);
            Handles.DrawSolidDisc(terrainPosition, Vector3.up, vegetationBeacon.Radius);

            if (EditorGUI.EndChangeCheck())
            {
                vegetationBeacon.UpdateVegetationBeacon();
                EditorUtility.SetDirty(vegetationBeacon);
            }
        }

        private Vector3 GetTerrainPosition(Vector3 _position)
        {
            RaycastHit[] hits = Physics.RaycastAll(new(_position + new Vector3(0, 10000, 0), Vector3.down));
            for (int j = 0; j < hits.Length; j++)
                if (hits[j].collider is TerrainCollider)
                    return hits[j].point;
            return _position;
        }
    }
}
