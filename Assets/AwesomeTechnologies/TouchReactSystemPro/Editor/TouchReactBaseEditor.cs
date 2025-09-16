using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    public class TouchReactBaseEditor : Editor
    {
        public GUIStyle LabelStyle;

        public virtual void Awake()
        {
            LabelStyle = new GUIStyle("Label") { fontStyle = FontStyle.Italic };

            if (EditorGUIUtility.isProSkin)
                LabelStyle.normal.textColor = new Color(1f, 1f, 1f);
            else
                LabelStyle.normal.textColor = new Color(0f, 0f, 0f);
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.LabelField("v1.1", LabelStyle);
        }
    }
}