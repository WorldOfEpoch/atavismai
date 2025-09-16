using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(ScreenshotUtility))]
    public class ScreenshotUtilityEditor : Editor
    {
        SerializedProperty screenshotKey;

        private void OnEnable()
        {
            screenshotKey = serializedObject.FindProperty("screenshotKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(screenshotKey, new GUIContent("Screenshot hotkey"));
            if (GUILayout.Button("Take screenshot"))
                ((ScreenshotUtility)target).TakeScreenshot();

            serializedObject.ApplyModifiedProperties();
        }
    }
}