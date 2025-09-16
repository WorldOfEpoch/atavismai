using AwesomeTechnologies.Utility;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [CustomEditor(typeof(PersistentVegetationStoragePackage))]
    public class PersistentVegetationStoragePackageEditor : VegetationStudioProBaseEditor
    {

        [MenuItem("Window/Awesome Technologies/Create data packages/Persistent Vegetation/PersistentVegetationStorage Package")]
        public static void CreatePersistentVegetationStoragePackage()
        {
            ScriptableObjectUtility.CreateAndReturnAsset<PersistentVegetationStoragePackage>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("All vegetation painted with the Vegetation Studio painting tools, baked from rules or added using the API are stored in this scriptable object\n" +
                "Which uses serialized forced binary to save space and increase loading speed", MessageType.Info);
            GUILayout.EndVertical();
        }
    }
}
