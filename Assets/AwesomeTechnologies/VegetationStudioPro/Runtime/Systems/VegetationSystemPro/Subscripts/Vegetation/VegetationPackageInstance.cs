using AwesomeTechnologies.Utility;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public struct MatrixInstance    // 80 bytes
    {
        public Matrix4x4 matrix;    // 64 bytes
        public float4 controlData;  // 16 bytes -- used initially for storing the distance falloff -- later used for storing crossfade data (only the fade percent value)
    }

    public class VegetationPackageInstance
    {
        public readonly List<NativeList<MatrixInstance>> matrixInstanceList = new();
        public NativeList<int> loadStateList;
        public readonly List<GraphicsBuffer> graphicsBufferList = new();

        public VegetationPackageInstance(int _vegetationItemCount)
        {
            matrixInstanceList.Capacity = _vegetationItemCount; // set/prepare capacity based on total item count possible
            loadStateList = new NativeList<int>(_vegetationItemCount, Allocator.Persistent);    // prepare list based on total item count possible

            for (int i = 0; i < _vegetationItemCount; i++)  // for each vegetation item of all vegetation packages
            {
                // prepare lists for rendering / cell rule generation
                matrixInstanceList.Add(new NativeList<MatrixInstance>(Allocator.Persistent));   // data to store each vegetation instance -- pos, rot, scale, distance falloff
                loadStateList.Add(0);   // mark each vegetation item as not loaded
                graphicsBufferList.Add(null);   // mark as not loaded/ready -- dummy fill as gets actually assigned later
            }
        }

        public void ClearInstanceMemory(int _index)
        {
            if (matrixInstanceList[_index].IsCreated)
                matrixInstanceList[_index].CompactMemory(); // clear and free memory(as long as the OS plays along)

            if (loadStateList.IsCreated)
                loadStateList[_index] = 0;  // reset count to declare "recreationable" -- allow creation of a new set of "MatrixInstance" data

            graphicsBufferList[_index]?.Release();  // release != dispose -- only release for run-time refresh -- allow to refresh graphicsBuffers with a new set of "MatrixInstance" data
            graphicsBufferList[_index] = null;  // reset to declare "recreationable"
        }

        public void Dispose()
        {
            for (int i = 0; i < matrixInstanceList.Count; i++)
                if (matrixInstanceList[i].IsCreated)
                    matrixInstanceList[i].Dispose();
            matrixInstanceList.Clear();

            if (loadStateList.IsCreated)
                loadStateList.Dispose();

            for (int i = 0; i < graphicsBufferList.Count; i++)
                graphicsBufferList[i]?.Dispose();
        }
    }
}