using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [CustomEditor(typeof(VegetationItemMask))]
    public class VegetationItemMaskEditor : VegetationStudioProBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("This type of mask is designed to be used through custom code", MessageType.Warning);
            EditorGUILayout.HelpBox("Set the position, type and ID of the vegetation instance that should be masked out" +
                "\nThis can be used for specific masking ex: harvesting" +
                "\nLook at the \"HarvestingDemo.cs\" file for an example", MessageType.Info);

            GUILayout.EndVertical();
        }
    }
}