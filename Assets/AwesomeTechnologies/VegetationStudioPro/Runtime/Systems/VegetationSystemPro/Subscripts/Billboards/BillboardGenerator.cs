using AwesomeTechnologies.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    public class BillboardGenerator
    {
        [BurstCompile]
        public struct PrepareBillboardMeshJob : IJob
        {
            [ReadOnly] public NativeList<MatrixInstance> InstanceList;
            [WriteOnly] public NativeList<Vector3> VertexList;
            [WriteOnly] public NativeList<int> IndexList;
            [WriteOnly] public NativeList<Vector2> UV1List;
            [WriteOnly] public NativeList<Vector2> UV2List;
            [WriteOnly] public NativeList<Vector2> UV3List;
            [WriteOnly] public NativeList<Vector3> NormalList;

            [ReadOnly] public Bounds VegetationItemBounds;

            private float3 srcVertex0;
            private float3 srcVertex1;
            private float3 srcVertex2;
            private float3 srcVertex3;

            private float2 srcUV0;
            private float2 srcUV1;
            private float2 srcUV2;
            private float2 srcUV3;

            private int srcIndex0;
            private int srcIndex1;
            private int srcIndex2;
            private int srcIndex3;
            private int srcIndex4;
            private int srcIndex5;

            public void Execute()
            {
                if (InstanceList.Length <= 0)
                    return; // skip rest function as not needed since no instances exist -- when no data would get merged

                float3 position;
                Quaternion rotation;
                float3 scale;
                float2 uv2Rotation;
                float2 uv3Scale;
                int vertexIndex = 0;

                float vegetationItemSize = math.max(math.max(VegetationItemBounds.size.x, VegetationItemBounds.size.y), VegetationItemBounds.size.z);

                srcVertex0 = new float3(-0.5f, -0.5f, 0);
                srcVertex1 = new float3(0.5f, 0.5f, 0);
                srcVertex2 = new float3(0.5f, -0.5f, 0);
                srcVertex3 = new float3(-0.5f, 0.5f, 0);

                srcUV0 = new float2(0, 0);
                srcUV1 = new float2(1, 1);
                srcUV2 = new float2(1, 0);
                srcUV3 = new float2(0, 1);

                srcIndex0 = 0;
                srcIndex1 = 1;
                srcIndex2 = 2;
                srcIndex3 = 1;
                srcIndex4 = 0;
                srcIndex5 = 3;

                for (int i = 0; i < InstanceList.Length; i++)
                {
                    if (InstanceList[i].controlData.x <= 0)
                        continue;   // skip masked out persistent vegetation storage vegetation instances

                    scale = ExtractScaleFromMatrix(InstanceList[i].matrix);
                    position = ExtractTranslationFromMatrix(InstanceList[i].matrix);
                    position.y += VegetationItemBounds.extents.y * scale.y;
                    rotation = ExtractRotationFromMatrix(InstanceList[i].matrix);

                    VertexList.Add(position);
                    VertexList.Add(position);
                    VertexList.Add(position);
                    VertexList.Add(position);

                    NormalList.Add(srcVertex0);
                    NormalList.Add(srcVertex1);
                    NormalList.Add(srcVertex2);
                    NormalList.Add(srcVertex3);

                    UV1List.Add(srcUV0);
                    UV1List.Add(srcUV1);
                    UV1List.Add(srcUV2);
                    UV1List.Add(srcUV3);

                    uv2Rotation.x = (360f - rotation.eulerAngles.y) / 360f;
                    uv2Rotation.y = 1f;
                    UV2List.Add(uv2Rotation);
                    UV2List.Add(uv2Rotation);
                    UV2List.Add(uv2Rotation);
                    UV2List.Add(uv2Rotation);

                    uv3Scale.x = vegetationItemSize * scale.x;
                    uv3Scale.y = -(VegetationItemBounds.extents.y * scale.y);
                    UV3List.Add(uv3Scale);
                    UV3List.Add(uv3Scale);
                    UV3List.Add(uv3Scale);
                    UV3List.Add(uv3Scale);

                    IndexList.Add(srcIndex0 + vertexIndex);
                    IndexList.Add(srcIndex1 + vertexIndex);
                    IndexList.Add(srcIndex2 + vertexIndex);
                    IndexList.Add(srcIndex3 + vertexIndex);
                    IndexList.Add(srcIndex4 + vertexIndex);
                    IndexList.Add(srcIndex5 + vertexIndex);
                    vertexIndex += 4;
                }
            }

            float3 ExtractScaleFromMatrix(Matrix4x4 _matrix)
            {
                return new float3(_matrix.GetColumn(0).magnitude, _matrix.GetColumn(1).magnitude, _matrix.GetColumn(2).magnitude);
            }

            float3 ExtractTranslationFromMatrix(Matrix4x4 _matrix)
            {
                float3 translation;
                translation.x = _matrix.m03;
                translation.y = _matrix.m13;
                translation.z = _matrix.m23;
                return translation;
            }

            quaternion ExtractRotationFromMatrix(Matrix4x4 _matrix)
            {
                float3 forward;
                forward.x = _matrix.m02;
                forward.y = _matrix.m12;
                forward.z = _matrix.m22;

                if (forward.Equals(float3.zero))
                    return quaternion.identity;

                float3 upward;
                upward.x = _matrix.m01;
                upward.y = _matrix.m11;
                upward.z = _matrix.m21;

                return Quaternion.LookRotation(forward, upward);    // degrees
            }
        }

        public static Mesh CreateMergedBillboardMesh(BillboardInstance _billboardInstance, BillboardCell _cell, string _biomeName, string _itemName)
        {
            NativeArray<Vector3> vertexArray = _billboardInstance.vertexList.AsArray();
            NativeArray<int> indexArray = _billboardInstance.indexList.AsArray();
            NativeArray<Vector2> uv1Array = _billboardInstance.uv1List.AsArray();
            NativeArray<Vector2> uv2Array = _billboardInstance.uv2List.AsArray();
            NativeArray<Vector2> uv3Array = _billboardInstance.uv3List.AsArray();
            NativeArray<Vector3> normalArray = _billboardInstance.normalList.AsArray();

            Vector3[] vertices = new Vector3[_billboardInstance.vertexList.Length];
            int[] indices = new int[_billboardInstance.indexList.Length];
            Vector2[] uv1s = new Vector2[_billboardInstance.uv1List.Length];
            Vector2[] uv2s = new Vector2[_billboardInstance.uv2List.Length];
            Vector2[] uv3s = new Vector2[_billboardInstance.uv3List.Length];
            Vector3[] normals = new Vector3[_billboardInstance.normalList.Length];

            // TODO use nativeArray directly when Unity adds an interface
            vertexArray.CopyToFast(vertices);
            indexArray.CopyToFast(indices);
            uv1Array.CopyToFast(uv1s);
            uv2Array.CopyToFast(uv2s);
            uv3Array.CopyToFast(uv3s);
            normalArray.CopyToFast(normals);

            Mesh mesh = new()
            {
                name = "MergedBillboard --" + " Cell: " + _cell.index + " | Biome: " + _biomeName + " | Item: " + _itemName,
                hideFlags = HideFlags.DontSave,
                indexFormat = IndexFormat.UInt32,
                subMeshCount = 1,
                vertices = vertices
            };

            mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            mesh.SetSubMesh(0, new(0, indices.Length, MeshTopology.Triangles) { bounds = _cell.cellBounds, vertexCount = vertices.Length }, MeshUpdateFlags.DontRecalculateBounds);
            mesh.uv = uv1s;
            mesh.uv2 = uv2s;
            mesh.uv3 = uv3s;
            mesh.normals = normals;
            mesh.bounds = _cell.cellBounds;
            mesh.UploadMeshData(true);  // immediately upload to the GPU => reduce potential loading stutters(not that relevant in this use case) -- remove memory from CPU based RAM => only applies to builds

            return mesh;
        }
    }
}