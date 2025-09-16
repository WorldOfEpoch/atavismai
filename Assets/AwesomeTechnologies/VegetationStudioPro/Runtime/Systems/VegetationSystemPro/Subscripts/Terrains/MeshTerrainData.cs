using AwesomeTechnologies.Utility.BVHTree;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.MeshTerrains
{
    [PreferBinarySerialization]
    [System.Serializable]
    public class MeshTerrainData : ScriptableObject
    {
        public Bounds Bounds;
        public int TriangleCount;   // unused

        public List<LBVHNODE> lNodes = new();
        public List<LBVHTriangle> lPrims = new();
        public List<byte> CoverageList = new(); // unused
    }
}