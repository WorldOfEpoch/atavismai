using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [AddComponentMenu("AwesomeTechnologies/TouchReact/TouchReactMesh", 2)]
    [ExecuteInEditMode]
    public class TouchReactMesh : MonoBehaviour
    {
        public List<MeshFilter> MeshFilterList = new();

        private void Awake()
        {
            MeshFilterList.Clear();
        }

        private void Start()
        {
            AddMeshToManager();
        } 

        void OnEnable()
        {
            AddMeshToManager();
        }

        private void OnDisable()
        {
            RemoveMeshFromManager();
        }

        private void AddMeshToManager()
        {
            MeshFilter[] meshes = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mesh in meshes)
            {
                MeshFilterList.Add(mesh);
                TouchReactSystem.AddMeshFilter(mesh);
            }
        }

        private void RemoveMeshFromManager()
        {
            for (int i = 0; i < MeshFilterList.Count; i++)
                TouchReactSystem.RemoveMeshFilter(MeshFilterList[i]);
            MeshFilterList.Clear();
        }
    }
}