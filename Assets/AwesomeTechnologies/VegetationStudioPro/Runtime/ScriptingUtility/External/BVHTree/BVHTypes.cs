using UnityEngine;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using Unity.Collections;
using Unity.Mathematics;

namespace AwesomeTechnologies.Utility.BVHTree
{
    public struct HitInfo   // 29 bytes
    {
        public float3 HitPoint; // 12 bytes
        public float3 HitNormal;    // 12 bytes
        public float HitDistance;   // 4 bytes
        public byte TerrainSourceID;    // 1 byte

        public HitInfo(HitInfo _hitInfo)
        {
            HitPoint = _hitInfo.HitPoint;
            HitNormal = _hitInfo.HitNormal;
            HitDistance = _hitInfo.HitDistance;
            TerrainSourceID = _hitInfo.TerrainSourceID;
        }

        public void Clear()
        {
            HitDistance = float.MaxValue;
        }
    }

    [System.Serializable]
    public struct LBVHTriangle  // 52 bytes
    {
        public float3 V0;   // 12 bytes
        public float3 V1;   // 12 bytes
        public float3 V2;   // 12 bytes
        public float3 N;    // 12 bytes
        public int TerrainSourceID; // 4 bytes

        public LBVHTriangle(float3 _v0, float3 _v1, float3 _v2, float3 _n, int _terrainSourceID)
        {
            V0 = _v0;
            V1 = _v1;
            V2 = _v2;
            N = _n;
            TerrainSourceID = _terrainSourceID;
        }

        public bool IntersectRay(BVHRay _ray, out HitInfo _hitInfo)
        {
            bool intersect = false;

            _hitInfo.HitPoint = new float3(0f, 0f, 0f);
            _hitInfo.HitNormal = new float3(0f, 1f, 0f);
            _hitInfo.HitDistance = float.MaxValue;
            _hitInfo.TerrainSourceID = (byte)TerrainSourceID;

            float3 rayO = _ray.Origin, rayD = _ray.Direction;

            float3 edge0 = V0 - rayO;
            float3 edge1 = V1 - rayO;
            float3 edge2 = V2 - rayO;

            float3 cross0 = math.normalize(math.cross(edge0, edge1));
            float3 cross1 = math.normalize(math.cross(edge1, edge2));
            float3 cross2 = math.normalize(math.cross(edge2, edge0));

            float angle0 = math.dot(cross0, rayD);
            float angle1 = math.dot(cross1, rayD);
            float angle2 = math.dot(cross2, rayD);

            if (angle0 < 0f && angle1 < 0f && angle2 < 0f)
            {
                float3 w0 = rayO - V0;
                var a = -math.dot(N, w0);
                var b = math.dot(N, rayD);
                var r = a / b;
                float3 I = rayO + rayD * r;
                if (a < 0f)
                {
                    _hitInfo.HitPoint = I;
                    _hitInfo.HitDistance = r;
                    _hitInfo.HitNormal = math.normalize(N);
                    intersect = true;
                }
            }
            return intersect;
        }

        public bool IntersectRay(BVHRay _ray, ref NativeArray<HitInfo> _hitInfos, int _hitInfoID)
        {
            float3 rayO = _ray.Origin, rayD = _ray.Direction;

            float3 edge0 = V0 - rayO;
            float3 edge1 = V1 - rayO;
            float3 edge2 = V2 - rayO;

            float3 cross0 = math.normalize(math.cross(edge0, edge1));
            float3 cross1 = math.normalize(math.cross(edge1, edge2));
            float3 cross2 = math.normalize(math.cross(edge2, edge0));

            float angle0 = math.dot(cross0, rayD);
            float angle1 = math.dot(cross1, rayD);
            float angle2 = math.dot(cross2, rayD);

            if (angle0 < 0f && angle1 < 0f && angle2 < 0f)
            {
                float3 w0 = rayO - V0;
                var a = -math.dot(N, w0);
                var b = math.dot(N, rayD);
                var r = a / b;
                float3 I = rayO + rayD * r;
                if (a < 0f)
                {
                    HitInfo hI = new HitInfo
                    {
                        HitNormal = math.normalize(N),
                        HitPoint = I,
                        HitDistance = r,
                        TerrainSourceID = (byte)TerrainSourceID
                    };
                    _hitInfos[_hitInfoID] = hI;
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    public struct LBVHNODE  // 64 bytes
    {
        public float3 BMin; // 12 bytes
        public float3 BMax; // 12 bytes
        public int NodeID;  // 4 bytes

        public int PrimitivesCount; // 4 bytes
        public int PrimitivesOffset;    // 4 bytes

        public int ParentID;    // 4 bytes

        public int LChildID;    // 4 bytes
        public int RChildID;    // 4 bytes

        public int IsLeaf;  // 4 byte

        public int NearNodeID;  // 4 byte
        public int FarNodeID;   // 4 byte
        public int SplitAxis;   // 4 byte

        public LBVHNODE(BVHNode _node)
        {
            BMin = _node.Min;
            BMax = _node.Max;
            NodeID = _node.NodeID;
            PrimitivesCount = _node.PrimitivesCount;
            PrimitivesOffset = _node.PrimitivesOffset;
            ParentID = _node.ParentID;
            LChildID = _node.LChildID;
            RChildID = _node.RChildID;
            IsLeaf = _node.IsLeaf;
            SplitAxis = _node.SplitAxis;
            NearNodeID = -1;
            FarNodeID = -1;
        }

        public void GetChildrenIDsAndSplitAxis(out int _lChildID, out int _rChildID, out int _splitAxis)
        {
            _lChildID = LChildID;
            _rChildID = RChildID;
            _splitAxis = SplitAxis;
        }

        public bool IntersectRay(BVHRay _r)//, out float _hitDist)
        {
            float tXmin, tXmax, tYmin, tYmax, tZmin, tZmax;
            float xA = 1f / _r.Direction.x, yA = 1f / _r.Direction.y, zA = 1f / _r.Direction.z;
            float xE = _r.Origin.x, yE = _r.Origin.y, zE = _r.Origin.z;

            // calculate t interval in x-axis
            if (xA >= 0)
            {
                tXmin = (BMin.x - xE) * xA;
                tXmax = (BMax.x - xE) * xA;
            }
            else
            {
                tXmin = (BMax.x - xE) * xA;
                tXmax = (BMin.x - xE) * xA;
            }

            // calculate t interval in y-axis
            if (yA >= 0)
            {
                tYmin = (BMin.y - yE) * yA;
                tYmax = (BMax.y - yE) * yA;
            }
            else
            {
                tYmin = (BMax.y - yE) * yA;
                tYmax = (BMin.y - yE) * yA;
            }

            // calculate t interval in z-axis
            if (zA >= 0)
            {
                tZmin = (BMin.z - zE) * zA;
                tZmax = (BMax.z - zE) * zA;
            }
            else
            {
                tZmin = (BMax.z - zE) * zA;
                tZmax = (BMin.z - zE) * zA;
            }

            // find if there an intersection among three t intervals

            // float3
            var tMin = math.max(tXmin, math.max(tYmin, tZmin));
            var tMax = math.min(tXmax, math.min(tYmax, tZmax));

            // Vector3
            //t_min = Mathf.Max(t_xmin, Mathf.Max(t_ymin, t_zmin));
            //t_max = Mathf.Min(t_xmax, Mathf.Min(t_ymax, t_zmax));

            //hitDist = t_min;
            return (tMin <= tMax);
        }
    }

    [System.Serializable]
    public struct ObjectData
    {
        public MeshRenderer Renderer;
        public Mesh Mesh;
        public int SubMeshCount;

        public List<Vector3> VerticeList;
        public List<Vector3> NormalList;
        public int[] Indices;
        public bool HasNormals;

        public BVHNode BVH;
        public bool IsValid;
        public List<BVHNode> Nodes;
        public List<BVHTriangle> Prims;

        public int TerrainSourceID;

        public ObjectData(MeshRenderer _r, int _terrainSourceID)
        {
            Renderer = _r;
            Mesh = _r.GetComponent<MeshFilter>().sharedMesh;
            IsValid = Mesh != null;

            SubMeshCount = 0;
            VerticeList = null;
            NormalList = null;
            Indices = null;
            HasNormals = false;
            Prims = null;
            Nodes = null;
            TerrainSourceID = _terrainSourceID;

            BVH = new BVHNode();

            if (IsValid)
            {
                SubMeshCount = Mesh.subMeshCount;
                VerticeList = new List<Vector3>();
                Mesh.GetVertices(VerticeList);
                NormalList = new List<Vector3>();
                Mesh.GetNormals(NormalList);
                Indices = new int[Mesh.triangles.Length];
                Indices = Mesh.triangles;
                HasNormals = NormalList.Count > 0;

                Matrix4x4 mtrx = Renderer.localToWorldMatrix;
                for (int v = 0; v < VerticeList.Count; v++)
                {
                    VerticeList[v] = mtrx.MultiplyPoint3x4(VerticeList[v]);
                    NormalList[v] = mtrx.MultiplyVector(NormalList[v]);
                }
            }
        }
    }
}