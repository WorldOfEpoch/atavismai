using AwesomeTechnologies.External;
using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies
{
    public class MeshUtility
    {
        public static GameObject GetSourceLOD(GameObject _go, LODLevel _LODLevel, bool _disableFallback = false)
        {
            LODGroup lodGroup = _go.GetComponentInChildren<LODGroup>();
            if (lodGroup == false)
                return _go;

            LOD[] lods = lodGroup.GetLODs();

            int lodIndex = (int)_LODLevel;
            lodIndex = math.clamp(lodIndex, 0, lods.Length - 1);

            LOD lod = lods[lodIndex];
            if (lod.renderers.Length > 0 && lod.renderers[0] != null)
            {
                if (lod.renderers[0].gameObject.GetComponent<BillboardRenderer>())
                {
                    if (lodIndex > 0)
                        lod = lods[lodIndex - 1];
                    else
                        return null;
                }

                if (lod.renderers.Length > 1)
                    Debug.LogError("VSP internal error log: Prefabs: A LOD of prefab: \"" + _go.transform.name + "\" has too many renderers, only one is supported and recommended -- LOD index: " + (int)_LODLevel);
                return lod.renderers[0].gameObject;
            }
            else
            {
                if (_disableFallback) return null;
                Debug.LogError("VSP internal error log: Prefabs: A renderer of prefab: \"" + _go.transform.name + "\" is missing -- Renderer index: 0 -- LOD index: " + (int)_LODLevel);
                GameObject fallbackCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fallbackCube.hideFlags = HideFlags.HideAndDontSave;
                return fallbackCube;
            }
        }

        public static GameObject InstantiateSourceLOD(GameObject _go, LODLevel _LODLevel, float3 _positionOffset, quaternion _rotationOffset, out MeshFilter _meshFilter, out MeshRenderer _meshRenderer)
        {
            GameObject lodGO = _go;

            LODGroup lodGroup = _go.GetComponent<LODGroup>();
            if (lodGroup && lodGroup.lodCount > 0)
            {
                int lodIndex = (int)_LODLevel;
                if (lodIndex >= lodGroup.lodCount)
                    lodIndex = lodGroup.lodCount - 1;

                LOD lod = lodGroup.GetLODs()[lodIndex];
                if (!(lod.renderers.Length <= 0 || lod.renderers[0] == null))
                    lodGO = lod.renderers[0].gameObject;
            }

            lodGO = Object.Instantiate(lodGO, _positionOffset, _rotationOffset);
            lodGO.transform.localPosition = float3.zero;
            lodGO.transform.localRotation = quaternion.identity;
            lodGO.transform.localScale = new float3(1);
            _meshFilter = lodGO.GetComponent<MeshFilter>();
            _meshRenderer = lodGO.GetComponent<MeshRenderer>();

            return lodGO;
        }

        public static Mesh GetMeshFromGameObject(GameObject _go)
        {
            if (_go == null)
                return new Mesh();

            MeshFilter meshFilter = _go.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh)
                return meshFilter.sharedMesh;
            else
            {
                Debug.LogError("VSP internal error log: Meshes: The mesh in the mesh filter component of the prefab: \"" + _go.transform.name + "\" is missing");
                return new Mesh();
            }
        }

        public static int GetLODCount(GameObject _go, IShaderController[] _shaderControllers)
        {
            LODGroup lodGroup = _go.GetComponentInChildren<LODGroup>();
            if (lodGroup)
            {
                LOD[] lods = lodGroup.GetLODs();
                int lodCount = lods.Length; // get LOD count

                LOD lastLOD = lods[^1]; // check for existence of a custom billboard
                foreach (Renderer renderer in lastLOD.renderers)
                    if (renderer is BillboardRenderer)
                    {
                        lodCount -= 1;
                        break;
                    }
                    else if (renderer is MeshRenderer)
                    {
                        if (_shaderControllers == null)
                            continue;

                        MeshRenderer meshRenderer = renderer as MeshRenderer;
                        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                            if (meshRenderer.sharedMaterials[i] != null)
                                for (int j = 0; j < _shaderControllers.Length; j++)
                                    if (_shaderControllers[j] != null)
                                        if (_shaderControllers[j].MatchBillboardShader(meshRenderer.sharedMaterials[i]))
                                        {
                                            lodCount -= 1;  // reduce the lodCount by one to adjust
                                            goto foundBillboardLOD; // skip the rest logic
                                        }
                    }

                foundBillboardLOD:;
                return lodCount;
            }

            return 1;
        }

        public static Bounds CalculateBounds(GameObject _go, int _maxLODLevel)
        {
            MeshFilter[] meshFilters = _go.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length == 1)
                return meshFilters[0].sharedMesh.bounds;    // get bounds => bounds get offset + scaled later in the culling jobs

            Bounds combinedbounds = new();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combinedbounds.Encapsulate(meshFilters[i].sharedMesh.bounds);   // grow into total bounds to avoid potential early culling => bounds get offset + scaled later in the culling jobs

                if (i >= _maxLODLevel)
                    break;  // skip enlarging beyond certain LOD levels -- skip ex: "Impostors" => for billboard texture generation
            }

            return combinedbounds;
        }

        public static float GetMinVertexPosition(GameObject _go)
        {
            float minPos = float.PositiveInfinity;

            MeshRenderer[] meshRenderers = _go.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshFilter meshFilter = meshRenderers[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter && meshFilter.sharedMesh)
                {
                    Vector3[] vertices = meshFilter.sharedMesh.vertices;
                    for (int j = 0; j < vertices.Length; j++)
                        if (vertices[j].y < minPos)
                            minPos = vertices[j].y;
                }
            }

            return minPos;
        }

        public static void RecalculateMeshNormals(GameObject _go, float _lerpFactor)
        {
            if (_lerpFactor <= 0) return;
            MeshFilter[] meshFilters = _go.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh mesh = Object.Instantiate(meshFilters[i].sharedMesh);
                mesh.RecalculateNormals();

                Vector3[] originalNormals = meshFilters[i].sharedMesh.normals;
                Vector3[] newNormals = mesh.normals;

                for (int j = 0; j < newNormals.Length; j++)
                    newNormals[j] = math.lerp(originalNormals[j], newNormals[j], _lerpFactor);
                mesh.normals = newNormals;

                mesh.UploadMeshData(false);
                meshFilters[i].mesh = mesh;
            }
        }

        #region CreateMesh functions
        public static Mesh CreateBoxMesh(float length = 1f, float width = 1f, float height = 1f)
        {
            Mesh mesh = new();
            mesh.Clear();

            //const float length = 1f;
            //const float width = 1f;
            //const float height = 1f;

            #region Vertices

            float3 p0 = new(-length * .5f, -width * .5f, height * .5f);
            float3 p1 = new(length * .5f, -width * .5f, height * .5f);
            float3 p2 = new(length * .5f, -width * .5f, -height * .5f);
            float3 p3 = new(-length * .5f, -width * .5f, -height * .5f);

            float3 p4 = new(-length * .5f, width * .5f, height * .5f);
            float3 p5 = new(length * .5f, width * .5f, height * .5f);
            float3 p6 = new(length * .5f, width * .5f, -height * .5f);
            float3 p7 = new(-length * .5f, width * .5f, -height * .5f);

            Vector3[] vertices =
            {
                // Bottom
                p0, p1, p2, p3,

                // Left
                p7, p4, p0, p3,

                // Front
                p4, p5, p1, p0,

                // Back
                p6, p7, p3, p2,

                // Right
                p5, p6, p2, p1,

                // Top
                p7, p6, p5, p4
            };

            #endregion

            #region Normals

            float3 up = Vector3.up;
            float3 down = Vector3.down;
            float3 front = Vector3.forward;
            float3 back = Vector3.back;
            float3 left = Vector3.left;
            float3 right = Vector3.right;

            Vector3[] normals =
            {
                // Bottom
                down, down, down, down,

                // Left
                left, left, left, left,

                // Front
                front, front, front, front,

                // Back
                back, back, back, back,

                // Right
                right, right, right, right,

                // Top
                up, up, up, up
            };

            #endregion

            #region UVs

            float2 _00 = new(0f, 0f);
            float2 _10 = new(1f, 0f);
            float2 _01 = new(0f, 1f);
            float2 _11 = new(1f, 1f);

            Vector2[] uvs =
            {
                // Bottom
                _11, _01, _00, _10,

                // Left
                _11, _01, _00, _10,

                // Front
                _11, _01, _00, _10,

                // Back
                _11, _01, _00, _10,

                // Right
                _11, _01, _00, _10,

                // Top
                _11, _01, _00, _10
            };

            #endregion

            #region Triangles

            int[] triangles =
            {
                // Bottom
                3, 1, 0,
                3, 2, 1,

                // Left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

                // Front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

                // Back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

                // Right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

                // Top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5
            };

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateCapsuleMesh(float radius, float height)
        {
            int segments = 24;

            if (segments % 2 != 0)
                segments++;

            // extra vertex on the seam
            int points = segments + 1;

            // calculate points around a circle
            float[] pX = new float[points];
            float[] pZ = new float[points];
            float[] pY = new float[points];
            float[] pR = new float[points];

            float calcH = 0f;
            float calcV = 0f;

            for (int i = 0; i < points; i++)
            {
                pX[i] = math.sin(math.radians(calcH));
                pZ[i] = math.cos(math.radians(calcH));
                pY[i] = math.cos(math.radians(calcV));
                pR[i] = math.sin(math.radians(calcV));

                calcH += 360f / segments;
                calcV += 180f / segments;
            }

            // - Vertices and UVs -

            Vector3[] vertices = new Vector3[points * (points + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int ind = 0;

            // Y-offset is half the height minus the diameter
            float yOff = (height - radius * 2f) * 0.5f;
            if (yOff < 0)
                yOff = 0;

            // uv calculations
            float stepX = 1f / (points - 1);
            float uvX, uvY;

            // Top Hemisphere
            int top = (int)math.ceil(points * 0.5f);

            for (int y = 0; y < top; y++)
                for (int x = 0; x < points; x++)
                {
                    vertices[ind] = new float3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                    vertices[ind].y = yOff + vertices[ind].y;

                    uvX = 1f - stepX * x;
                    uvY = (vertices[ind].y + height * 0.5f) / height;
                    uvs[ind] = new float2(uvX, uvY);

                    ind++;
                }

            // Bottom Hemisphere
            int btm = (int)math.floor(points * 0.5f);

            for (int y = btm; y < points; y++)
                for (int x = 0; x < points; x++)
                {
                    vertices[ind] = new float3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                    vertices[ind].y = -yOff + vertices[ind].y;

                    uvX = 1f - stepX * x;
                    uvY = (vertices[ind].y + height * 0.5f) / height;
                    uvs[ind] = new float2(uvX, uvY);

                    ind++;
                }

            // - Triangles -

            int[] triangles = new int[segments * (segments + 1) * 2 * 3];

            for (int y = 0, t = 0; y < segments + 1; y++)
                for (int x = 0; x < segments; x++, t += 6)
                {
                    triangles[t + 0] = (y + 0) * (segments + 1) + x + 0;
                    triangles[t + 1] = (y + 1) * (segments + 1) + x + 0;
                    triangles[t + 2] = (y + 1) * (segments + 1) + x + 1;

                    triangles[t + 3] = (y + 0) * (segments + 1) + x + 1;
                    triangles[t + 4] = (y + 0) * (segments + 1) + x + 0;
                    triangles[t + 5] = (y + 1) * (segments + 1) + x + 1;
                }

            Mesh mesh = new();
            mesh.Clear();

            mesh.name = "ProceduralCapsule";

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
            //_capsuleColliderMesh = mesh;
        }

        public static Mesh CreateSphereMesh(float _radius = 1f)
        {
            Mesh mesh = new();
            mesh.Clear();

            //float radius = 1f;
            // Longitude |||
            int nbLong = 24;
            // Latitude ---
            int nbLat = 16;

            #region Vertices

            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = math.PI;
            var _2Pi = _pi * 2f;

            vertices[0] = Vector3.up * _radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (lat + 1) / (nbLat + 1);
                float sin1 = math.sin(a1);
                float cos1 = math.cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2Pi * (lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = math.sin(a2);
                    float cos2 = math.cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new float3(sin1 * cos2, cos1, sin1 * sin2) * _radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -_radius;

            #endregion

            #region Normals

            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normals[n] = vertices[n].normalized;

            #endregion

            #region UVs

            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
                for (int lon = 0; lon <= nbLong; lon++)
                    uvs[lon + lat * (nbLong + 1) + 1] =
                        new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));

            #endregion

            #region Triangles

            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh ExtrudeMeshFromPolygon(float3[] _polygonPoints, float _yExtent)
        {
            Vector2[] polygonPoints2D = new Vector2[_polygonPoints.Length];
            for (int i = 0; i < _polygonPoints.Length; i++)
                polygonPoints2D[i] = new float2(_polygonPoints[i].x, _polygonPoints[i].z);

            Triangulator tr = new(polygonPoints2D);
            int[] indices = tr.Triangulate();

            List<int> indexList = new();

            for (int i = 0; i < indices.Length; i += 3)
            {
                indexList.Add(indices[i + 2]);
                indexList.Add(indices[i + 1]);
                indexList.Add(indices[i]);
            }

            int polygonCount = _polygonPoints.Length;
            for (int i = 0; i < indices.Length; i += 3)
            {
                indexList.Add(indices[i] + polygonCount);
                indexList.Add(indices[i + 1] + polygonCount);
                indexList.Add(indices[i + 2] + polygonCount);
            }

            for (int i = 0; i < _polygonPoints.Length - 1; i++)
            {
                indexList.Add(i);
                indexList.Add(i + polygonCount);
                indexList.Add(i + 1);

                indexList.Add(i + polygonCount);
                indexList.Add(i + 1 + polygonCount);
                indexList.Add(i + 1);
            }

            indexList.Add(polygonCount - 1);
            indexList.Add(polygonCount - 1 + polygonCount);
            indexList.Add(0);

            indexList.Add(polygonCount - 1 + polygonCount);
            indexList.Add(polygonCount);
            indexList.Add(0);

            List<Vector3> verticeList = new();

            for (int i = 0; i < _polygonPoints.Length; i++)
                verticeList.Add(new float3(_polygonPoints[i].x, _polygonPoints[i].y - _yExtent, _polygonPoints[i].z));

            for (int i = 0; i < _polygonPoints.Length; i++)
                verticeList.Add(new float3(_polygonPoints[i].x, _polygonPoints[i].y + _yExtent, _polygonPoints[i].z));

            Mesh mesh = new() { vertices = verticeList.ToArray(), triangles = indexList.ToArray() };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
        #endregion
    }
}