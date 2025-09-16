using UnityEditor;

namespace AwesomeTechnologies.Vegetation
{
    [CustomEditor(typeof(VegetationMask))]
    public class VegetationMaskErrorEditor : VegetationStudioProBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("This component is a base class used by other mask components", MessageType.Error);
        }
    }
}