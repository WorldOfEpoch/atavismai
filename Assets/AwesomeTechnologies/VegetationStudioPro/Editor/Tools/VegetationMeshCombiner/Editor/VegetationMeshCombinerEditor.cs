using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(VegetationMeshCombiner))]
    public class VegetationMeshCombinerEditor : VegetationStudioProBaseEditor
    {
        public override void OnInspectorGUI()
        {
            VegetationMeshCombiner vegetationMeshCombiner = (VegetationMeshCombiner)target;
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("This tool combines multiple meshes into a single mesh\nAnd multiple submeshes into a single submesh based on duplicate materials", MessageType.Info);
            EditorGUILayout.HelpBox("This is only intended for simple tasks for meshes with only one initial submesh\nFor advanced mesh combining use other advanced tools like \"Mesh Baker\"", MessageType.Warning);

            vegetationMeshCombiner.targetGameObject = EditorGUILayout.ObjectField("Root GameObject", vegetationMeshCombiner.targetGameObject, typeof(GameObject), true) as GameObject;
            EditorGUILayout.HelpBox("Add all meshes as children to a gameObject and assign it as the root gameObject" +
                "\nA new gameObject will be created in the hierarchy with the combined result\nThe new mesh gets saved to the project files", MessageType.Info);

            if (vegetationMeshCombiner.targetGameObject == false)
                EditorGUILayout.HelpBox("Assign a root GameObject", MessageType.Error);

            vegetationMeshCombiner.shallCombineSubmeshes = EditorGUILayout.Toggle("Combine duplicate materials", vegetationMeshCombiner.shallCombineSubmeshes);
            EditorGUILayout.HelpBox("When enabled submeshes get combined that are using the same material", MessageType.Info);

            if (GUILayout.Button("Generate combined mesh"))
                CombineMeshes(vegetationMeshCombiner);
        }

        void CombineMeshes(VegetationMeshCombiner _vegetationMeshCombiner)
        {
            if (_vegetationMeshCombiner.targetGameObject == null)
                return;

            string path = EditorUtility.SaveFilePanelInProject("Save combined mesh", "", "asset", "Enter a file name to save the combined mesh to");
            if (path.Length == 0)
                return;

            // combine meshes and submeshes(materials)
            GameObject combinedGameObject = _vegetationMeshCombiner.CombineMeshes();

            // additionally save the combined mesh to disk
            MeshFilter combinedMeshFilter = combinedGameObject.GetComponentInChildren<MeshFilter>();
            AssetDatabase.CreateAsset(combinedMeshFilter.sharedMesh, path);
        }
    }
}

