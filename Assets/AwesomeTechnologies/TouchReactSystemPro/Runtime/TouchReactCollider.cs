using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [AddComponentMenu("AwesomeTechnologies/TouchReact/TouchReactCollider", 1)]
    [ExecuteInEditMode]
    public class TouchReactCollider : MonoBehaviour
    {
        public List<TouchColliderInfo> ColliderList = new();
        public bool AddChildColliders = true;
        public float ColliderScale = 1f;

        private void Awake()
        {
            ColliderList.Clear();
        }

        private void Start()
        {
            AddCollidersToManager();
        }

        void OnEnable()
        {
            AddCollidersToManager();
        }

        private void OnDisable()
        {
            RemoveCollidersFromManager();
        }

        public void RefreshColliders()
        {
            RemoveCollidersFromManager();
            AddCollidersToManager();
        }

        private void AddCollidersToManager()
        {
            Collider[] colliders = AddChildColliders ? gameObject.GetComponentsInChildren<Collider>() : gameObject.GetComponents<Collider>();
            foreach (Collider thisCollider in colliders)
            {
                if (thisCollider is TerrainCollider)
                    continue;

                TouchColliderInfo touchColliderInfo = new TouchColliderInfo
                {
                    Collider = thisCollider,
                    Scale = ColliderScale
                };
                ColliderList.Add(touchColliderInfo);
                TouchReactSystem.AddCollider(touchColliderInfo);
            }
        }

        private void RemoveCollidersFromManager()
        {
            for (int i = 0; i < ColliderList.Count; i++)
                TouchReactSystem.RemoveCollider(ColliderList[i]);
            ColliderList.Clear();
        }
    }
}