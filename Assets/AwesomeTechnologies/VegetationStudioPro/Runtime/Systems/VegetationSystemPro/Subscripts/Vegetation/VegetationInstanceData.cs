using AwesomeTechnologies.Utility;
using Unity.Collections;
using Unity.Mathematics;

namespace AwesomeTechnologies.Vegetation
{
    public class VegetationInstanceData // 66 bytes
    {
        public NativeList<float3> position; // 12 bytes
        public NativeList<quaternion> rotation; // 16 bytes
        public NativeList<float3> scale;    // 12 bytes
        public NativeList<float3> terrainNormal;    // 12 bytes
        public NativeList<int> randomNumberIndex;   // 4 bytes
        public NativeList<float2> controlData;  // 8 bytes
        public NativeList<byte> terrainSourceID;    // 1 byte
        public NativeList<byte> included;   // 1 byte

        public VegetationInstanceData() // persistent as pooled lists w/ compacting instead of disposing
        {
            position = new NativeList<float3>(Allocator.Persistent);
            rotation = new NativeList<quaternion>(Allocator.Persistent);
            scale = new NativeList<float3>(Allocator.Persistent);
            terrainNormal = new NativeList<float3>(Allocator.Persistent);
            randomNumberIndex = new NativeList<int>(Allocator.Persistent);
            controlData = new NativeList<float2>(Allocator.Persistent);
            terrainSourceID = new NativeList<byte>(Allocator.Persistent);
            included = new NativeList<byte>(Allocator.Persistent);
        }

        public void SetCapacity(int _capacity)
        {
            position.Capacity = _capacity;
            rotation.Capacity = _capacity;
            scale.Capacity = _capacity;
            terrainNormal.Capacity = _capacity;
            randomNumberIndex.Capacity = _capacity;
            controlData.Capacity = _capacity;
            terrainSourceID.Capacity = _capacity;
            included.Capacity = _capacity;
        }

        public void ResizeUninitialized(int _length)
        {
            position.ResizeUninitialized(_length);
            rotation.ResizeUninitialized(_length);
            scale.ResizeUninitialized(_length);
            terrainNormal.ResizeUninitialized(_length);
            randomNumberIndex.ResizeUninitialized(_length);
            controlData.ResizeUninitialized(_length);
            terrainSourceID.ResizeUninitialized(_length);
            included.ResizeUninitialized(_length);
        }

        public void Clear()
        {
            position.Clear();
            rotation.Clear();
            scale.Clear();
            terrainNormal.Clear();
            randomNumberIndex.Clear();
            controlData.Clear();
            terrainSourceID.Clear();
            included.Clear();
        }

        public void CompactMemory()
        {
            position.CompactMemory();
            rotation.CompactMemory();
            scale.CompactMemory();
            terrainNormal.CompactMemory();
            randomNumberIndex.CompactMemory();
            controlData.CompactMemory();
            terrainSourceID.CompactMemory();
            included.CompactMemory();
        }

        public void Dispose()
        {
            position.Dispose();
            rotation.Dispose();
            scale.Dispose();
            terrainNormal.Dispose();
            randomNumberIndex.Dispose();
            controlData.Dispose();
            terrainSourceID.Dispose();
            included.Dispose();
        }
    }
}