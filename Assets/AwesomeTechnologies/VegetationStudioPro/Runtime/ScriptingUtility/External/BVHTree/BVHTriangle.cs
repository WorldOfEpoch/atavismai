using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility.BVHTree
{
    public class BVHTriangle
    {
        public int PrimID;
        public Vector3 V0, V1, V2, N0, N1, N2, N;
        public int TerrainSourceID;

        public Vector3 Center;
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Centroid;

        public BVHTriangle(Vector3 _v0, Vector3 _v1, Vector3 _v2, Vector3 _n0, Vector3 _n1, Vector3 _n2, int _primID, int _terrainSourceID)
        {
            V0 = _v0; V1 = _v1; V2 = _v2;
            N0 = _n0; N1 = _n1; N2 = _n2;

            N = Vector3.Cross(Vector3.Normalize(V1 - V0), Vector3.Normalize(V2 - V0));

            PrimID = _primID;
            TerrainSourceID = _terrainSourceID;

            Min = Vector3.zero;
            Max = Vector3.zero;

            Center = (Min + Max) * 0.5f;
            Centroid = Center;

            CalculateBBox();
        }

        public void CalculateBBox()
        {
            Min = new Vector3(math.min(Min.x, V0.x), math.min(Min.y, V0.y), math.min(Min.z, V0.z));
            Max = new Vector3(math.max(Max.x, V0.x), math.max(Max.y, V0.y), math.max(Max.z, V0.z));

            Min = new Vector3(math.min(Min.x, V1.x), math.min(Min.y, V1.y), math.min(Min.z, V1.z));
            Max = new Vector3(math.max(Max.x, V1.x), math.max(Max.y, V1.y), math.max(Max.z, V1.z));

            Min = new Vector3(math.min(Min.x, V2.x), math.min(Min.y, V2.y), math.min(Min.z, V2.z));
            Max = new Vector3(math.max(Max.x, V2.x), math.max(Max.y, V2.y), math.max(Max.z, V2.z));

            Centroid = (Min + Max) * 0.5f;
        }
    }
}