using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [System.Serializable]
    public class Node
    {
        public bool Selected;
        public float3 Position;
        public bool OverrideWidth;
        public float CustomWidth = 2f;
        public bool Active = true;
    }

    [ExecuteInEditMode]
    public class VegetationMask : MonoBehaviour
    {
        public bool RemoveGrass = true;
        public bool RemovePlants = true;
        public bool RemoveObjects = true;
        public bool RemoveLargeObjects = true;
        public bool RemoveTrees = true;

        public float AdditionalGrassPerimiter;
        public float AdditionalPlantPerimiter;
        public float AdditionalObjectPerimiter;
        public float AdditionalLargeObjectPerimiter;
        public float AdditionalTreePerimiter;

        public float AdditionalGrassPerimiterMax;
        public float AdditionalPlantPerimiterMax;
        public float AdditionalObjectPerimiterMax;
        public float AdditionalLargeObjectPerimiterMax;
        public float AdditionalTreePerimiterMax;

        public float NoiseScaleGrass = 0;
        public float NoiseScalePlant = 0;
        public float NoiseScaleObject = 0;
        public float NoiseScaleLargeObject = 0;
        public float NoiseScaleTree = 0;

        public EMaskRadiusType eMaskRadiusGrass = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusPlant = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusObject = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusLargeObject = EMaskRadiusType.outerRadius;
        public EMaskRadiusType eMaskRadiusTree = EMaskRadiusType.center;

        public string Id;

        public bool IncludeVegetationType;

        public List<Node> Nodes = new();
        public bool ClosedArea = true;
        public bool ShowArea = true;
        public bool ShowHandles = true;
        public string MaskName = "";
        public bool isDirty;
        public LayerMask GroundLayerMask;
        public bool useNodeAutoSnapping = true;

        public List<VegetationTypeSettings> VegetationTypeList = new();

        private float3 lastPosition;
        private quaternion lastRotation;
        private float3 lastLossyScale;

        public virtual void Reset()
        {
            if (Id == "")
                Id = System.Guid.NewGuid().ToString();
        }

        void OnEnable()
        {
            if (Id == "")
                Id = System.Guid.NewGuid().ToString();

            isDirty = true;
        }

        void Update()
        {
#if !UNITY_EDITOR
            if (Application.isPlaying && isDirty == false)
                return;
#endif

            if (isDirty || lastPosition.Equals(transform.position) == false || lastRotation.Equals(transform.rotation) == false || lastLossyScale.Equals(transform.lossyScale) == false)
            {
                PositionNodes();    // set node positions and add mask to the system

                lastPosition = transform.position;
                lastRotation = transform.rotation;
                lastLossyScale = transform.lossyScale;

                isDirty = false;
            }
        }

        public virtual void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        public void AddVegetationTypes(BaseMaskArea _maskArea)
        {
            for (int i = 0; i < VegetationTypeList.Count; i++)
                _maskArea.VegetationTypeList.Add(new VegetationTypeSettings(VegetationTypeList[i]));
        }

        public void ClearNodes()    // kept for third party assets
        {
            Nodes.Clear();
        }

        public void PositionNodes()
        {
            if (useNodeAutoSnapping)
                for (int i = 0; i < Nodes.Count; i++)
                {
                    RaycastHit[] hits = Physics.RaycastAll(new(transform.TransformPoint(Nodes[i].Position) + new Vector3(0, 10000, 0), Vector3.down)).OrderBy(h => h.distance).ToArray();
                    for (int j = 0; j < hits.Length; j++)
                        if (hits[j].collider is TerrainCollider || GroundLayerMask.Contains(hits[j].collider.gameObject.layer))
                        {
                            Nodes[i].Position = transform.InverseTransformPoint(hits[j].point);
                            break;
                        }
                }

            UpdateVegetationMask();
        }

        public virtual void UpdateVegetationMask()  // call "PositionNodes" instead if applicable
        {

        }

        public void AddNode(float3 _worldPosition)
        {
            if (Nodes.Count == 0)
            {
                AddNodeToEnd(_worldPosition);
                return;
            }

            Node closestNode = FindClosestNode(_worldPosition);
            Node nextNode = GetNextNode(closestNode);
            Node previousNode = GetPreviousNode(closestNode);

            LineSegment3D nextSegment = new(transform.TransformPoint(closestNode.Position), transform.TransformPoint(nextNode.Position));
            LineSegment3D previousSegment = new(transform.TransformPoint(closestNode.Position), transform.TransformPoint(previousNode.Position));

            float nextSegmentDistance = nextSegment.DistanceTo(_worldPosition);
            float previousSegmentDistance = previousSegment.DistanceTo(_worldPosition);

            Node node = new() { Position = transform.InverseTransformPoint(_worldPosition) };

            int currentNodeIndex = GetNodeIndex(closestNode);

            if (nextSegmentDistance < previousSegmentDistance)
            {
                if (currentNodeIndex == Nodes.Count - 1)
                    Nodes.Add(node);
                else
                    Nodes.Insert(currentNodeIndex + 1, node);
            }
            else
            {
                Nodes.Insert(currentNodeIndex, node);
            }

            isDirty = true;
        }

        public void AddNodeToEnd(float3 _worldPosition)
        {
            Nodes.Add(new() { Position = transform.InverseTransformPoint(_worldPosition) });
        }

        public void AddNodeToEnd(float3 _worldPosition, float _customWidth, bool _active)
        {
            Nodes.Add(new()
            {
                Position = transform.InverseTransformPoint(_worldPosition),
                CustomWidth = _customWidth,
                OverrideWidth = true,
                Active = _active
            });
        }

        public void AddNodesToEnd(float3[] _worldPositions)
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i]);
        }

        public void AddNodesToEnd(float3[] _worldPositions, float[] _customWidth, bool[] _active)
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i], _customWidth[i], _active[i]);
        }

        public int GetNodeIndex(Node _node)
        {
            int nodeIndex = 0;
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] == _node)
                {
                    nodeIndex = i;
                    break;
                }
            return nodeIndex;
        }
        public Node GetNextNode(Node _node)
        {
            int nodeIndex = 0;
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] == _node)
                {
                    nodeIndex = i;
                    break;
                }

            if (nodeIndex == Nodes.Count - 1)
                return Nodes[0];
            else
                return Nodes[nodeIndex + 1];
        }

        public Node GetPreviousNode(Node _node)
        {
            int nodeIndex = 0;
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] == _node)
                {
                    nodeIndex = i;
                    break;
                }

            if (nodeIndex == 0)
                return Nodes[Nodes.Count - 1];
            else
                return Nodes[nodeIndex - 1];
        }

        public Node FindClosestNode(float3 _worldPosition)
        {
            Node returnNode = null;
            float smallestDistance = float.MaxValue;

            for (int i = 0; i < Nodes.Count; i++)
            {
                float distance = math.distance(_worldPosition, transform.TransformPoint(Nodes[i].Position));
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    returnNode = Nodes[i];
                }
            }
            return returnNode;
        }

        public void DeleteNode(Node _node)
        {
            Nodes.Remove(_node);
        }

        public List<float3> GetWorldSpaceNodePositions()
        {
            List<float3> worldSpaceNodeList = new();
            for (int i = 0; i < Nodes.Count; i++)
                worldSpaceNodeList.Add(transform.TransformPoint(Nodes[i].Position));
            return worldSpaceNodeList;
        }

        void DrawGizmos()
        {
#if UNITY_EDITOR
            if (ShowArea)
            {
                if (MaskName != "")
                    Handles.Label(GetMaskCenter(), MaskName, new(EditorStyles.whiteLabel));

                Gizmos.color = new Color(1f, 1f, 0, 1f);
                Camera sceneviewCamera = SceneViewDetector.GetCurrentSceneViewCamera();
                if (sceneviewCamera == null)
                    return;

                for (int i = 0; i < Nodes.Count; i++)
                {
                    float distance = math.distance(sceneviewCamera.transform.position, transform.TransformPoint(Nodes[i].Position));
                    if (distance < 500)
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 1f);
                        if (Nodes[i].Selected)
                            Gizmos.color = new Color(0, 1f, 0f, 1f);
                        Gizmos.DrawSphere(transform.TransformPoint(Nodes[i].Position), 0.015f * distance);
                    }
                }

                Gizmos.color = new Color(1f, 1f, 1f, 1f);
                if (Nodes.Count > 1)
                    for (int i = 0; i < Nodes.Count; i++)
                        if (i == Nodes.Count - 1)
                        {
                            if (ClosedArea)
                                Gizmos.DrawLine(transform.TransformPoint(Nodes[0].Position), transform.TransformPoint(Nodes[i].Position));
                        }
                        else
                        {
                            Gizmos.DrawLine(transform.TransformPoint(Nodes[i].Position), transform.TransformPoint(Nodes[i + 1].Position));
                        }
            }
#endif
        }

        public float3 GetMaskCenter()
        {
            return GetMeanVector(GetWorldSpaceNodePositions().ToArray());
        }

        private float3 GetMeanVector(float3[] _positions)
        {
            if (_positions.Length == 0)
                return float3.zero;

            float x = 0f;
            float y = 0f;
            float z = 0f;

            foreach (float3 pos in _positions)
            {
                x += pos.x;
                y += pos.y;
                z += pos.z;
            }

            return new float3(x / _positions.Length, y / _positions.Length, z / _positions.Length);
        }
    }
}