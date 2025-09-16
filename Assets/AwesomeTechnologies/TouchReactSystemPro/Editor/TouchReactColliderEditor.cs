using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [CustomEditor(typeof(TouchReactCollider))]
    public class TouchReactColliderEditor : TouchReactBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TouchReactCollider touchReactCollider = (TouchReactCollider)target;
            EditorGUILayout.HelpBox("Add this to any GameObject with a valid collider setup", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Colliders", LabelStyle);
            touchReactCollider.AddChildColliders = EditorGUILayout.Toggle("Add child colliders", touchReactCollider.AddChildColliders);
            EditorGUILayout.HelpBox("This adds all colliders of child GameObjects", MessageType.Info);
            touchReactCollider.ColliderScale = EditorGUILayout.Slider("Collider scale", touchReactCollider.ColliderScale, 0.1f, 5f);
            EditorGUILayout.HelpBox("Collider scale only affects the touch react area/collider and not the original collider/-s", MessageType.Info);

            if (GUILayout.Button("Refresh colliders"))
                touchReactCollider.RefreshColliders();

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                touchReactCollider.RefreshColliders();
                EditorUtility.SetDirty(touchReactCollider);
            }
        }
    }
}