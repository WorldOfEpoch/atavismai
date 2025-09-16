using AwesomeTechnologies.Utility;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationPackageBillboardInstance
    {
        public List<BillboardInstance> billboardInstanceList = new();

        public VegetationPackageBillboardInstance(int _vegetationItemCount)
        {
            for (int i = 0; i < _vegetationItemCount; i++)  // for each vegetation item of all vegetation packages
                billboardInstanceList.Add(new BillboardInstance()); // add a new instance -- first containing temporary merged data to create the actual mesh -- then only the mesh
        }

        public void ClearCache()
        {
            for (int i = 0; i < billboardInstanceList.Count; i++)
                billboardInstanceList[i].ClearCache();
        }

        public void Dispose()
        {
            for (int i = 0; i < billboardInstanceList.Count; i++)
                billboardInstanceList[i].Dispose();
            billboardInstanceList.Clear();
        }
    }

    public class BillboardInstance  // 141 bytes -- 9 bytes(fixed data) -- 132 bytes(temporary data) -- 52(216) bytes(effective instance data)
    {
        public bool loaded; // 1 byte
        public Mesh mesh;   // 8 bytes => the actual resulting mesh used for the rendering
        public NativeList<MatrixInstance> instanceList; // 80 bytes
        public NativeList<Vector3> vertexList;  // 12 bytes
        public NativeList<int> indexList;   // 4 bytes
        public NativeList<Vector2> uv1List; // 8 bytes
        public NativeList<Vector2> uv2List; // 8 bytes
        public NativeList<Vector2> uv3List; // 8 bytes
        public NativeList<Vector3> normalList;  // 12 bytes

        public BillboardInstance()  // persistent as lists w/ compacting instead of disposing -- to have direct control when to dispose -- temporary data => only used to create the actual mesh
        {
            instanceList = new NativeList<MatrixInstance>(Allocator.Persistent);
            vertexList = new NativeList<Vector3>(Allocator.Persistent);
            indexList = new NativeList<int>(Allocator.Persistent);
            uv1List = new NativeList<Vector2>(Allocator.Persistent);
            uv2List = new NativeList<Vector2>(Allocator.Persistent);
            uv3List = new NativeList<Vector2>(Allocator.Persistent);
            normalList = new NativeList<Vector3>(Allocator.Persistent);
        }

        public void ClearCache()
        {
            if (loaded == false)
                return;

            if (mesh)
                Object.DestroyImmediate(mesh);  // immediate since only called for hard refreshes
            CompactMemory();

            loaded = false;
        }

        public void CompactMemory()
        {
            if (instanceList.IsCreated) instanceList.CompactMemory();
            if (vertexList.IsCreated) vertexList.CompactMemory();
            if (indexList.IsCreated) indexList.CompactMemory();
            if (uv1List.IsCreated) uv1List.CompactMemory();
            if (uv2List.IsCreated) uv2List.CompactMemory();
            if (uv3List.IsCreated) uv3List.CompactMemory();
            if (normalList.IsCreated) normalList.CompactMemory();
        }

        public void Dispose()
        {
            if (mesh)
                Object.DestroyImmediate(mesh);  // immediate since only called for hard refreshes

            if (instanceList.IsCreated) instanceList.Dispose();
            if (vertexList.IsCreated) vertexList.Dispose();
            if (indexList.IsCreated) indexList.Dispose();
            if (uv1List.IsCreated) uv1List.Dispose();
            if (uv2List.IsCreated) uv2List.Dispose();
            if (uv3List.IsCreated) uv3List.Dispose();
            if (normalList.IsCreated) normalList.Dispose();
        }
    }
}

