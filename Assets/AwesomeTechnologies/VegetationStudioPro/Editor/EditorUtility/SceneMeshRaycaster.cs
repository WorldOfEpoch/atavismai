using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeTechnologies.External.Octree;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public class MeshRendererRaycastInfo
    {
        public MeshRenderer MeshRenderer;
        public Mesh Mesh;
        public Matrix4x4 LocalToWorldMatrix4X4;
        public Bounds Bounds;
    }

    public class SceneMeshRaycaster
    {
#if UNITY_EDITOR
        private readonly MethodInfo _methIntersectRayMesh;
#endif
        public List<MeshRendererRaycastInfo> MeshRendererRaycastInfoList = new();
        public List<TerrainCollider> SceneTerrainColliderList = new();

        public BoundsOctree<MeshRendererRaycastInfo> BoundsOctree;

        public SceneMeshRaycaster()
        {
#if UNITY_EDITOR
            var editorTypes = typeof(Editor).Assembly.GetTypes();

            var typeHandleUtility = editorTypes.FirstOrDefault(t => t.Name == "HandleUtility");
            if (typeHandleUtility != null)
                _methIntersectRayMesh = typeHandleUtility.GetMethod("IntersectRayMesh", (BindingFlags.Static | BindingFlags.NonPublic));
#endif
            FindMeshRenderers();
            SetupOctree();
        }

        private Bounds GetSceneBounds()
        {
            Bounds sceneBounds = MeshRendererRaycastInfoList.Count > 0 ? MeshRendererRaycastInfoList[0].Bounds : new Bounds();
            for (int i = 0; i < MeshRendererRaycastInfoList.Count; i++)
                sceneBounds.Encapsulate(MeshRendererRaycastInfoList[i].Bounds);
            return sceneBounds;
        }

        private void SetupOctree()
        {
            Bounds sceneBounds = GetSceneBounds();
            BoundsOctree = new BoundsOctree<MeshRendererRaycastInfo>(sceneBounds.size.magnitude, sceneBounds.center, 1, 1.2f);
            for (int i = 0; i < MeshRendererRaycastInfoList.Count; i++)
                BoundsOctree.Add(MeshRendererRaycastInfoList[i], MeshRendererRaycastInfoList[i].Bounds);
        }

        private void FindMeshRenderers()
        {
            MeshRendererRaycastInfoList.Clear();

            MeshRenderer[] meshRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshRendererRaycastInfo meshRendererRaycastInfo = new()
                {
                    MeshRenderer = meshRenderers[i],
                    Bounds = meshRenderers[i].bounds,
                    LocalToWorldMatrix4X4 = meshRenderers[i].localToWorldMatrix,
                };

                MeshFilter meshFilter = meshRenderers[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter)
                    meshRendererRaycastInfo.Mesh = meshFilter.sharedMesh;

                if (meshRendererRaycastInfo.Mesh)
                    MeshRendererRaycastInfoList.Add(meshRendererRaycastInfo);
            }

            SceneTerrainColliderList.Clear();
            TerrainCollider[] terrainColliders = Object.FindObjectsByType<TerrainCollider>(FindObjectsSortMode.None);
            SceneTerrainColliderList.AddRange(terrainColliders);
        }

        private bool IntersectRayMesh(Ray _ray, MeshFilter _meshFilter, out RaycastHit _hit)
        {
            return IntersectRayMesh(_ray, _meshFilter.mesh, _meshFilter.transform.localToWorldMatrix, out _hit);
        }

        private bool IntersectRayMesh(Ray _ray, Mesh _mesh, Matrix4x4 _matrix, out RaycastHit _hit)
        {
#if UNITY_EDITOR
            var parameters = new object[] { _ray, _mesh, _matrix, null };
            bool result = (bool)_methIntersectRayMesh.Invoke(null, parameters);
            _hit = (RaycastHit)parameters[3];
            return result;
#else
            _hit = new RaycastHit();
            return false;
#endif
        }

        public bool RaycastSceneMeshes(Ray _ray, out RaycastHit _hit, bool _includeTerrain, bool _includeColliders, bool _includeMeshes)
        {
            _hit = new RaycastHit();
            RaycastHit tempRaycastHit;
            bool hitSomething = false;

            float minDistance = float.PositiveInfinity;

            if (_includeTerrain && !_includeColliders)
                for (int i = 0; i < SceneTerrainColliderList.Count; i++)
                {
                    if (!SceneTerrainColliderList[i].Raycast(_ray, out tempRaycastHit, float.PositiveInfinity)) continue;
                    float distance = Vector3.Distance(_ray.origin, tempRaycastHit.point);
                    if (!(distance < minDistance)) continue;
                    minDistance = distance;
                    hitSomething = true;
                    _hit = tempRaycastHit;
                }

            if (_includeColliders && !_includeTerrain)
            {
                RaycastHit[] hits = Physics.RaycastAll(_ray, float.PositiveInfinity);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (!(hits[i].collider is TerrainCollider))
                    {
                        float distance = Vector3.Distance(_ray.origin, hits[i].point);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            hitSomething = true;
                            _hit = hits[i];
                        }
                    }
                }
            }

            if (_includeTerrain && _includeColliders)
                if (Physics.Raycast(_ray, out tempRaycastHit, float.PositiveInfinity))
                {
                    float distance = Vector3.Distance(_ray.origin, tempRaycastHit.point);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        hitSomething = true;
                        _hit = tempRaycastHit;
                    }
                }

            if (_includeMeshes)
            {
                List<MeshRendererRaycastInfo> collidingWith = new();
                BoundsOctree.GetColliding(collidingWith, _ray);

                for (int i = 0; i < collidingWith.Count; i++)
                {
                    // enable for debugging
                    //Gizmos.color = Color.white;
                    //Handles.DrawWireCube(collidingWith[i].Bounds.center, collidingWith[i].Bounds.size);

                    if (!IntersectRayMesh(_ray, collidingWith[i].Mesh,
                        collidingWith[i].LocalToWorldMatrix4X4, out tempRaycastHit))
                        continue;

                    float distance = Vector3.Distance(_ray.origin, tempRaycastHit.point);
                    if (!(distance < minDistance))
                        continue;

                    minDistance = distance;
                    hitSomething = true;
                    _hit = tempRaycastHit;
                }
            }

            return hitSomething;
        }
    }
}