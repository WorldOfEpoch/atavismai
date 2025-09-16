using AwesomeTechnologies.Utility;
using UnityEditor;

namespace AwesomeTechnologies.MeshTerrains
{
    [CustomEditor(typeof(MeshTerrainData))]
    public class MeshTerrainDataEditor : VegetationStudioProBaseEditor
    {
        [MenuItem("Window/Awesome Technologies/Create data packages/Mesh Terrain/MeshTerrainData")]
        public static void CreateMeshTerrainDataScriptableObject()
        {
            ScriptableObjectUtility.CreateAndReturnAsset<MeshTerrainData>();
        }
    }
}