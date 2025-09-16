using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/VegetationMeshCombiner")]
    public class VegetationMeshCombiner : MonoBehaviour
    {
        public GameObject targetGameObject;
        public bool shallCombineSubmeshes = true;

        void Reset()
        {
            targetGameObject = gameObject;
        }

        public GameObject CombineMeshes()
        {
            // save original transform data
            Vector3 originalPosition = targetGameObject.transform.position;
            Quaternion originalRotation = targetGameObject.transform.rotation;
            Vector3 originalScale = targetGameObject.transform.localScale; // use local scale

            // reset transform data for correct pivot point
            targetGameObject.transform.position = new Vector3(0, 0, 0);
            targetGameObject.transform.rotation = Quaternion.identity;
            targetGameObject.transform.localScale = Vector3.one;   // use local scale

            // get all sharedMeshes from the target
            MeshFilter[] meshFilters = targetGameObject.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combinedData = new CombineInstance[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)    // for all mesh filters -- assign data into a struct used for combining
            {
                combinedData[i].mesh = meshFilters[i].sharedMesh;
                combinedData[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            // get all sharedMaterials from the target
            MeshRenderer[] meshRenderers = targetGameObject.GetComponentsInChildren<MeshRenderer>();
            List<Material> tempMaterialList = new List<Material>();
            for (int i = 0; i < meshRenderers.Length; i++)  // for all mesh renderers -- assign data into a (dynamically extending) list
                tempMaterialList.AddRange(meshRenderers[i].sharedMaterials);

            // create a new gameObject to combine the meshes/sub-meshes(duplicate materials) into
            GameObject combinedGameObject = new GameObject(targetGameObject.name + "_Combined");

            // create a new mesh(filter) and assign the "combinedData" into it
            MeshFilter combinedMeshFilter = combinedGameObject.AddComponent<MeshFilter>();
            combinedMeshFilter.mesh = new Mesh();
            combinedMeshFilter.sharedMesh.CombineMeshes(combinedData, false, true); // combine only the super-meshes

            // create a new mesh renderer and pass all the (uncombined) sharedMaterials into it
            MeshRenderer combinedMeshRenderer = combinedGameObject.AddComponent<MeshRenderer>();
            combinedMeshRenderer.sharedMaterials = tempMaterialList.ToArray();  // convert to an array since it's needed for passing it

            if (shallCombineSubmeshes)  // additionally combine sub-meshes(duplicate materials) if enabled
            {
                SubmeshCombiner submeshCombiner = new SubmeshCombiner();
                for (int i = 0; i < combinedMeshFilter.sharedMesh.subMeshCount; i++)    // for each sub-mesh within the combined mesh
                    submeshCombiner.AddSubmesh(combinedMeshFilter.sharedMesh.GetIndices(i), combinedMeshRenderer.sharedMaterials[i]);

                submeshCombiner.UpdateMesh(combinedMeshFilter.sharedMesh);  // combine sub-meshes
                combinedMeshRenderer.sharedMaterials = submeshCombiner.GetMaterialsFromSubMeshes(); // assign matching materials
            }

            // write back original transform data
            targetGameObject.transform.position = originalPosition;
            targetGameObject.transform.rotation = originalRotation;
            targetGameObject.transform.localScale = originalScale;  // use local scale

            // apply target's transform data
            combinedGameObject.transform.position = originalPosition;
            combinedGameObject.transform.rotation = originalRotation;
            combinedGameObject.transform.localScale = originalScale;    // use local scale

            return combinedGameObject;  // return combined result
        }
    }

    public class SubmeshInfo
    {
        public Material material;
        public readonly List<int> indices = new List<int>();
    }

    public class SubmeshCombiner
    {
        public readonly List<SubmeshInfo> submeshInfoList = new List<SubmeshInfo>();

        public void AddSubmesh(int[] _indices, Material _material)
        {
            SubmeshInfo submeshInfo = GetExistingSubmeshInfo(_material);    // check for duplicate material
            if (submeshInfo == null)    // when no duplicate found
            {
                submeshInfo = new SubmeshInfo { material = _material };
                submeshInfoList.Add(submeshInfo);   // create a new "sub-mesh" and add it to the list
            }

            submeshInfo.indices.AddRange(_indices); // add indices to combine sub-mesh
        }

        private SubmeshInfo GetExistingSubmeshInfo(Material _material)
        {
            for (int i = 0; i < submeshInfoList.Count; i++) // for already existing "sub-meshes"
                if (submeshInfoList[i].material == _material)
                    return submeshInfoList[i];  // return existing "sub-mesh"

            return null;    // else return null to create a new "sub-mesh"
        }

        public void UpdateMesh(Mesh _mesh)
        {
            _mesh.subMeshCount = submeshInfoList.Count; // update meshes subMeshCount
            for (int i = 0; i < submeshInfoList.Count; i++) // for each sub mesh
                _mesh.SetIndices(submeshInfoList[i].indices.ToArray(), _mesh.GetTopology(i), i);    // set new indice data to combine the sub meshes
        }

        public Material[] GetMaterialsFromSubMeshes()
        {
            Material[] materials = new Material[submeshInfoList.Count];
            for (int i = 0; i < submeshInfoList.Count; i++) // for each sub mesh
                materials[i] = submeshInfoList[i].material;

            return materials;   // return each material of all sub meshes
        }
    }
}