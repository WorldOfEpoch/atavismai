using UnityEditor;

namespace AwesomeTechnologies.Grass
{
    [CustomEditor(typeof(ProceduralGrassPlane))]
    public class ProceduralGrassPlaneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is a utility class used by the \"Grass Patch Generator\" component", MessageType.Error);
        }
    }
}