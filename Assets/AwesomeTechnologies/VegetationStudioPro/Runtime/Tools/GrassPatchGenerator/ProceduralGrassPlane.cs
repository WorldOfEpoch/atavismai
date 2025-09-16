using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [System.Serializable]
    public class ProceduralGrassPlane : MonoBehaviour
    {
        public int index = 0;
        public int lodLevel = 0;
        public Material material;

        public float width = 0.33f;
        public float height = 0.33f;
        public int widthSegments = 2;
        public int heightSegments = 2;

        private float2 pivot = float2.zero; // centered pivot point

        public float minimumBendHeight = 0f;
        public float bendDistanceOffset_first = 0.33f;
        public float bendDistanceOffset_second = 0.33f;
        public float curveOffset = 0.33f;

        public bool bakePhase;
        public bool bakeBend;
        public bool bakeAO;
        public float phase;
        public AnimationCurve aoCurve;
        public AnimationCurve bendCurve;

        public void CreateGrassPlane()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
                mesh = new Mesh();
            mesh.Clear();

            int hCount = widthSegments + 1;
            int vCount = heightSegments + 1;

            float scaleX = width / widthSegments;
            float scaleY = height / heightSegments;
            float uvFactorX = 1f / widthSegments;
            float uvFactorY = 1f / heightSegments;

            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount * vCount;
            int[] triangles = new int[numTriangles];
            Vector3[] vertices = new Vector3[numVertices];
            float4 tangent = new(1f, 0f, 0f, -1f);
            Vector4[] tangents = new Vector4[numVertices];
            Vector2[] uvs = new Vector2[numVertices];

            int index = 0;
            for (float y = 0f; y < vCount; y++)
                for (float x = 0f; x < hCount; x++)
                {
                    float currentOffset = math.lerp(bendDistanceOffset_first, bendDistanceOffset_second, x * uvFactorX);
                    float zOffset = math.lerp(0, currentOffset, y * uvFactorY);
                    if ((y * scaleY) <= minimumBendHeight)
                        zOffset = 0;

                    float normalizedX = x * uvFactorX;
                    float zCurveoffset = math.lerp(0, curveOffset, (normalizedX * 2) - 0.5f);
                    if (normalizedX <= 0.5f)
                        zCurveoffset = math.lerp(curveOffset, 0, normalizedX * 2);

                    vertices[index] = new float3(x * scaleX - width * 0.5f - pivot.x, y * scaleY - height * 0.5f - pivot.y, zOffset + zCurveoffset);
                    tangents[index] = tangent;
                    uvs[index++] = new float2(x * uvFactorX, y * uvFactorY);
                }

            index = 0;
            for (int y = 0; y < heightSegments; y++)
                for (int x = 0; x < widthSegments; x++)
                {
                    triangles[index] = (y * hCount) + x;
                    triangles[index + 1] = ((y + 1) * hCount) + x;
                    triangles[index + 2] = (y * hCount) + x + 1;

                    triangles[index + 3] = ((y + 1) * hCount) + x;
                    triangles[index + 4] = ((y + 1) * hCount) + x + 1;
                    triangles[index + 5] = (y * hCount) + x + 1;
                    index += 6;
                }

            mesh.vertices = vertices;
            mesh.triangles = triangles; // assign vertices first else it errors out
            mesh.tangents = tangents;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
            mesh.RecalculateBounds();

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
                meshRenderer.sharedMaterial = material;

            if (bakePhase || bakeBend)
                ApplyPhaseAndBend(mesh);
        }

        private void ApplyPhaseAndBend(Mesh _mesh)
        {
            Color[] meshColors;
            Vector3[] vertex = _mesh.vertices;

            if (_mesh.colors.Length == 0)
                meshColors = new Color[_mesh.vertexCount];
            else
                meshColors = _mesh.colors;

            byte phaseByte = 255;
            byte bendByte = 255;
            byte ambientByte = 255;
            if (bakePhase)
                phaseByte = (byte)(phase * 255);

            for (int i = 0; i < meshColors.Length; i++)
            {
                float vertexHeight = (vertex[i].y + height * 0.5f) / height;
                vertexHeight = math.clamp(vertexHeight, 0, 1);

                if (bakeBend)
                {
                    float bendCurveOutput = bendCurve.Evaluate(vertexHeight);
                    bendCurveOutput = math.clamp(bendCurveOutput, 0f, 1f);
                    bendByte = (byte)(bendCurveOutput * 255);
                }

                if (bakeAO)
                {
                    float ambientOcculsionCurveOutput = aoCurve.Evaluate(vertexHeight);
                    ambientOcculsionCurveOutput = math.clamp(ambientOcculsionCurveOutput, 0f, 1f);
                    ambientByte = (byte)(ambientOcculsionCurveOutput * 255);
                }

                meshColors[i] = new Color32(ambientByte, phaseByte, bendByte, bendByte);
            }

            _mesh.colors = meshColors;
        }
    }
}