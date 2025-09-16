using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace AwesomeTechnologies.Utility.BVHTree
{
    public struct BVHNode
    {
        public int IsLeaf;  // 0 = false, 1 = true        
        public int NodeType;    // o is root, 1 is left , 2 is right
        public Vector3 Centroid;    //  = new Vector3(0f);
        public Vector3 Min; //  = new Vector3(1f) *  float.MaxValue;
        public Vector3 Max; //  = new Vector3(1f) * -float.MaxValue;

        // move from a single primitive in a leaf to multiple
        public int PrimID;  //  = -1;
        public int PrimitivesCount; //  How many primitives there are in the leaf
        public int PrimitivesOffset;    //  Where in the flatten buffer the primitives are starting from
        public int NodeID;  //  = 0;
        public int ParentID;    //  = 0;
        public int LChildID;    //  = 0;
        public int RChildID;    //  = 0;

        // used in the intersection methods
        public int SplitAxis;
        public float SplitValue;
        public int NearNodeID;
        public int FarNodeID;
        public int SplitMethod;

        public static BVHNode CreateBVHNode()
        {
            BVHNode b = new BVHNode
            {
                NodeType = 0,
                IsLeaf = 0,
                Centroid = Vector3.zero,
                Min = Vector3.one * float.MaxValue,
                Max = Vector3.one * -float.MaxValue,
                PrimID = -1,
                PrimitivesCount = 0,
                PrimitivesOffset = 0,
                NodeID = 0,
                ParentID = -1,
                LChildID = 0,
                RChildID = 0,
                NearNodeID = 0,
                FarNodeID = 0,
                SplitAxis = 0,
                SplitMethod = 1
            };

            return b;
        }

        public BVHNode(List<BVHTriangle> _tris, List<BVHNode> _nodes, ref List<BVHTriangle> _finalPrims)
        {
            this = CreateBVHNode();

            Centroid = Vector3.zero;
            Min = Vector3.zero;
            Max = Vector3.zero;
            NodeID = 0;
            CalculateBBox(_tris);
            _nodes.Add(this);

            if (_tris.Count > 0)
                Build(0, _tris, ref _nodes, ref _finalPrims);
        }

        public void Build(int _nodeID, List<BVHTriangle> _tris, ref List<BVHNode> _nodes, ref List<BVHTriangle> _finalPrims)
        {
            BVHNode node = _nodes[_nodeID];

            // Leaf
            if (_tris.Count <= BVH.MaxPrimsCountPerNode)
            {
                // NOTE: the prims offset is calculated in the "FlatteningBVH" method
                node.IsLeaf = 1;

                node.PrimitivesCount = 1;
                node.PrimitivesOffset = _finalPrims.Count;

                _finalPrims.Add(_tris[0]);
                _nodes[_nodeID] = node;
            }
            else
            {
                List<BVHTriangle> lTris = new List<BVHTriangle>();
                List<BVHTriangle> rTris = new List<BVHTriangle>();

                switch (node.SplitMethod)
                {
                    // -------------------- Equal count split ! --------------------
                    case 0: //  (int)BVHSplitMethod.SPLIT_EQUAL_COUNTS:

                        node.GetLongestAxisAndValue();
                        int splitAxis = node.SplitAxis;

                        // Equal split
                        if (lTris.Count == 0 || rTris.Count == 0)
                        {
                            // in this case the incomming tris list should be sorted
                            switch (splitAxis)
                            {
                                case 0: _tris.Sort(CompareX); break;
                                case 1: _tris.Sort(CompareY); break;
                                case 2: _tris.Sort(CompareZ); break;
                            }
                            int trisHalf = _tris.Count / 2;

                            lTris = _tris.GetRange(0, trisHalf);
                            rTris = _tris.GetRange(trisHalf, _tris.Count - trisHalf);
                        }

                        break;

                    // -------------------- Median axis split ! --------------------
                    case 1: // (int)BVHSplitMethod.SPLIT_MIDDLE:

                        node.GetLongestAxisAndValue();

                        float splitValue = node.SplitValue;
                        splitAxis = node.SplitAxis;

                        // median split triangle buffer
                        switch (splitAxis)
                        {
                            case 0:
                                lTris = _tris.FindAll(n => n.Centroid.x < splitValue);
                                rTris = _tris.FindAll(n => n.Centroid.x >= splitValue);
                                break;
                            case 1:
                                lTris = _tris.FindAll(n => n.Centroid.y < splitValue);
                                rTris = _tris.FindAll(n => n.Centroid.y >= splitValue);
                                break;
                            case 2:
                                lTris = _tris.FindAll(n => n.Centroid.z < splitValue);
                                rTris = _tris.FindAll(n => n.Centroid.z >= splitValue);
                                break;
                        }

                        // if median split was not good enough -- switch to equal split
                        if (lTris.Count == 0 || rTris.Count == 0)
                        {
                            // in this case the incomming tris list should be sorted
                            switch (splitAxis)
                            {
                                case 0: _tris.Sort(CompareX); break;
                                case 1: _tris.Sort(CompareY); break;
                                case 2: _tris.Sort(CompareZ); break;
                            }

                            int trisHalf = _tris.Count / 2;
                            lTris = _tris.GetRange(0, trisHalf);
                            rTris = _tris.GetRange(trisHalf, _tris.Count - trisHalf);
                        }
                        //Debug.Log("Sliptting primitives using median split");
                        break;

                    // -------------------- Split using surface area heuristic ! --------------------
                    case 2: // BVHSplitMethod.SPLIT_SAH:

                        break;
                }

                BVHNode lChild = CreateBVHNode();
                BVHNode rChild = CreateBVHNode();

                lChild.NodeID = _nodes.Count + 0;
                rChild.NodeID = _nodes.Count + 1;

                lChild.ParentID = node.NodeID;
                rChild.ParentID = node.NodeID;

                lChild.NodeType = 1;
                rChild.NodeType = 2;

                node.LChildID = lChild.NodeID;
                node.RChildID = rChild.NodeID;

                lChild.CalculateBBox(lTris);
                rChild.CalculateBBox(rTris);

                // ----------------------------------------------------------------------------
                // adding both children into the nodes list
                // ----------------------------------------------------------------------------

                _nodes.Add(lChild);
                _nodes.Add(rChild);

                // ----------------------------------------------------------------------------
                // building the children nodes
                // ----------------------------------------------------------------------------

                // use only when dealing with structs
                _nodes[_nodeID] = node;

                Build(lChild.NodeID, lTris, ref _nodes, ref _finalPrims);
                Build(rChild.NodeID, rTris, ref _nodes, ref _finalPrims);
            }
        }

        public void GetLongestAxisAndValue()
        {
            float xLength = Mathf.Abs(Min.x - Max.x);
            if (xLength < 0.000001f)
                xLength = 0f;

            float yLength = Mathf.Abs(Min.y - Max.y);
            if (yLength < 0.000001f)
                yLength = 0f;

            float zLength = Mathf.Abs(Min.z - Max.z);
            if (zLength < 0.000001f)
                zLength = 0f;

            float[] sides = new float[] { xLength, yLength, zLength };
            float maxLength = math.max(math.max(xLength, yLength), zLength);

            for (int i = 0; i < sides.Length; i++)
                if (maxLength == sides[i])
                {
                    // return a new array with the first element as the axis ID
                    // -> the second element is the value which is half of the length of the longest axis

                    SplitAxis = i;
                    SplitValue = Centroid[i];
                    return;
                }

            SplitAxis = 0;
            SplitValue = 0f;

            Debug.LogError("NOTE: BBox longest side is not calculated properly!");
        }

        public GameObject CalculateBBox(List<BVHTriangle> _tris)
        {
            for (int tr = 0; tr < _tris.Count; tr++)
            {
                Min = new Vector3(math.min(Min.x, _tris[tr].V0.x), math.min(Min.y, _tris[tr].V0.y), math.min(Min.z, _tris[tr].V0.z));
                Max = new Vector3(math.max(Max.x, _tris[tr].V0.x), math.max(Max.y, _tris[tr].V0.y), math.max(Max.z, _tris[tr].V0.z));

                Min = new Vector3(math.min(Min.x, _tris[tr].V1.x), math.min(Min.y, _tris[tr].V1.y), math.min(Min.z, _tris[tr].V1.z));
                Max = new Vector3(math.max(Max.x, _tris[tr].V1.x), math.max(Max.y, _tris[tr].V1.y), math.max(Max.z, _tris[tr].V1.z));

                Min = new Vector3(math.min(Min.x, _tris[tr].V2.x), math.min(Min.y, _tris[tr].V2.y), math.min(Min.z, _tris[tr].V2.z));
                Max = new Vector3(math.max(Max.x, _tris[tr].V2.x), math.max(Max.y, _tris[tr].V2.y), math.max(Max.z, _tris[tr].V2.z));
            }

            Centroid = (Min + Max) / 2f;

            return null;
        }

        #region Compare utility
        private static int CompareX(BVHTriangle h1, BVHTriangle h2)
        {
            if (h1.Centroid.x - h2.Centroid.x < 0f)
                return -1;
            return 1;
        }

        private static int CompareY(BVHTriangle h1, BVHTriangle h2)
        {
            if (h1.Centroid.y - h2.Centroid.y < 0f)
                return -1;
            return 1;
        }

        private static int CompareZ(BVHTriangle h1, BVHTriangle h2)
        {
            if (h1.Centroid.z - h2.Centroid.z < 0f)
                return -1;
            return 1;
        }
        #endregion
    }
}