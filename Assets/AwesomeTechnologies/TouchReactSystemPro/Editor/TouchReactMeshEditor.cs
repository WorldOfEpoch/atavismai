using UnityEditor;

namespace AwesomeTechnologies.TouchReact
{
    [CustomEditor(typeof(TouchReactMesh))]
    public class TouchReactMeshEditor : TouchReactBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("Add this to any GameObject with a valid mesh renderer setup", MessageType.Info);
        }
    }
}