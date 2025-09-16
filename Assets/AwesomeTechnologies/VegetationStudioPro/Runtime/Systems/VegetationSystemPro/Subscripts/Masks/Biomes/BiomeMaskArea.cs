using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationStudio;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    [System.Serializable]
    public class Node
    {
        public bool Selected;
        public float3 Position;
        public bool OverrideWidth;
        public float CustomWidth = 2f;
        public bool Active = true;
        public bool DisableEdge;
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Masks/BiomeMaskArea", 0)]
    [ScriptExecutionOrder(99)]
    [ExecuteInEditMode]
    public class BiomeMaskArea : MonoBehaviour
    {
        public List<Node> Nodes = new();
        public bool ClosedArea = true;
        public bool ShowArea = true;
        public bool ShowHandles = true;
        public bool useEdgeDisableMode;
        public string MaskName = "";
        public bool isDirty;
        public string Id;
        public BiomeType BiomeType;
        public LayerMask GroundLayerMask;
        public bool useNodeAutoSnapping = true;

        public AnimationCurve BlendCurve = new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });
        public AnimationCurve InverseBlendCurve = new(new Keyframe[] { new(0, 1, 0f, 0f), new(1, 0, 0f, 0f) });
        public AnimationCurve TextureBlendCurve = new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });
        public float BlendDistance = 5f;
        public float NoiseScale = 20;
        public bool UseNoise = false;
        public PolygonMaskBiome _currentMaskArea;

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

        void OnDisable()
        {
            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveBiomeMask(_currentMaskArea);
                _currentMaskArea = null;
            }
        }

        public virtual void OnDrawGizmos()
        {
            if (VegetationStudioManager.ShowBiomes)
                DrawGizmos();
        }

        public virtual void OnDrawGizmosSelected()
        {
            if (VegetationStudioManager.ShowBiomes == false)
                DrawGizmos();
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

            UpdateBiomeMask();
        }

        public void UpdateBiomeMask()   // call "PositionNodes" instead if applicable
        {
            if (enabled == false || gameObject.activeSelf == false)
                return;

            // validate "broken" curves > reset
            if (ValidateAnimationCurve(BlendCurve) == false) BlendCurve = ResetAnimationCurve();
            if (ValidateAnimationCurve(InverseBlendCurve) == false) InverseBlendCurve = ResetAnimationCurve();
            if (ValidateAnimationCurve(TextureBlendCurve) == false) TextureBlendCurve = ResetAnimationCurve();

            // setup a new "PolygonBiomeMask" > feed-in UI-set values ..or through code set values
            PolygonMaskBiome maskArea = new() { BiomeType = BiomeType, BlendDistance = BlendDistance, UseNoise = UseNoise, NoiseScale = NoiseScale };
            maskArea.AddPolygon(GetWorldSpaceNodePositions(), GetDisableEdgeList());

            maskArea.SetCurve(BlendCurve.GenerateCurveArray(4096));
            maskArea.SetInverseCurve(InverseBlendCurve.GenerateCurveArray(4096));
            maskArea.SetTextureCurve(TextureBlendCurve.GenerateCurveArray(4096));

            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveBiomeMask(_currentMaskArea);  // remove > clear cache of old area
                _currentMaskArea = null;    // reset
            }

            _currentMaskArea = maskArea;
            VegetationStudioManager.AddBiomeMask(maskArea); // add > clear cache of new area

            RefreshPostProcessVolume(); // update PPv2 volume when installed
        }

        public List<float3> GetWorldSpaceNodePositions()
        {
            List<float3> worldSpaceNodeList = new();
            for (int i = 0; i < Nodes.Count; i++)
                worldSpaceNodeList.Add(transform.TransformPoint(Nodes[i].Position));
            return worldSpaceNodeList;
        }

        private List<bool> GetDisableEdgeList()
        {
            List<bool> disableEdgeList = new();
            for (int i = 0; i < Nodes.Count; i++)
                disableEdgeList.Add(Nodes[i].DisableEdge);
            return disableEdgeList;
        }

        private bool ValidateAnimationCurve(AnimationCurve _curve)
        {
            float sample = _curve.Evaluate(0.5f);
            if (float.IsNaN(sample))
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Curve error", "A corrupted blend curve has been found and it has been reset", "OK");
#endif
                return false;
            }
            return true;
        }

        private AnimationCurve ResetAnimationCurve()
        {
            return new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });
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

        public void DeleteNode(Node _node)
        {
            Nodes.Remove(_node);
        }

        public void AddNodeToEnd(float3 _worldPosition)
        {
            Nodes.Add(new() { Position = transform.InverseTransformPoint(_worldPosition) });
        }

        public void AddNodeToEnd(float3 _worldPosition, bool _disableEdge)
        {
            Nodes.Add(new()
            {
                Position = transform.InverseTransformPoint(_worldPosition),
                DisableEdge = _disableEdge
            });
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

        public void AddNodeToEnd(float3 _worldPosition, float _customWidth, bool _active, bool _disableEdge)
        {
            Nodes.Add(new()
            {
                Position = transform.InverseTransformPoint(_worldPosition),
                CustomWidth = _customWidth,
                OverrideWidth = true,
                Active = _active,
                DisableEdge = _disableEdge
            });
        }

        public void AddNodesToEnd(float3[] _worldPositions)
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i]);
        }

        public void AddNodesToEnd(float3[] _worldPositions, bool[] _disableEdges)
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i], _disableEdges[i]);
        }

        public void AddNodesToEnd(Vector3[] _worldPositions, bool[] _disableEdges)  // fall-back compatibility overloads for ex: "R.A.M" of "NatureManufacture"
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i], _disableEdges[i]);
        }

        public void AddNodesToEnd(float3[] _worldPositions, float[] _customWidth, bool[] _active)
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i], _customWidth[i], _active[i]);
        }

        public void AddNodesToEnd(float3[] _worldPositions, float[] _customWidth, bool[] _active, bool[] _disableEdges)
        {
            for (int i = 0; i < _worldPositions.Length; i++)
                AddNodeToEnd(_worldPositions[i], _customWidth[i], _active[i], _disableEdges[i]);
        }

        private void DrawGizmos()
        {
#if UNITY_EDITOR
            if (ShowArea)
            {
                if (MaskName != "")
                    Handles.Label(GetMaskCenter(), MaskName + "(" + BiomeType + ")", new(EditorStyles.whiteLabel));
                else
                    Handles.Label(GetMaskCenter(), BiomeType.ToString(), new(EditorStyles.whiteLabel));

                Gizmos.color = new Color(1f, 1f, 0, 1f);
                Camera sceneviewCamera = SceneViewDetector.GetCurrentSceneViewCamera();
                if (sceneviewCamera == null)
                    return;

                for (int i = 0; i < Nodes.Count; i++)
                {
                    float distance = math.distance(sceneviewCamera.transform.position, transform.TransformPoint(Nodes[i].Position));
                    if (distance < 1000)
                    {
                        Gizmos.color = !Nodes[i].DisableEdge ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);
                        if (Nodes[i].Selected)
                            Gizmos.color = new Color(1f, 1f, 1f, 1f);
                        Gizmos.DrawSphere(transform.TransformPoint(Nodes[i].Position), 0.02f * distance);
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

        public void RefreshPostProcessVolume()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessProfileInfo postProcessProfileInfo = VegetationStudioManager.GetPostProcessProfileInfo(BiomeType);
            RefreshPostProcessVolume(postProcessProfileInfo, VegetationStudioManager.GetPostProcessingLayer());
#endif
        }

#if UNITY_POST_PROCESSING_STACK_V2
        public void RefreshPostProcessVolume(PostProcessProfileInfo _postProcessProfileInfo, LayerMask _postProcessLayer)
        {
            gameObject.layer = _postProcessLayer;

            if (_postProcessProfileInfo == null)
            {
                PostProcessVolume postProcessVolume = gameObject.GetComponent<PostProcessVolume>();
                if (postProcessVolume)
                    DestroyImmediate(postProcessVolume);

                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if (meshCollider)
                    DestroyImmediate(meshCollider);
            }
            else
            {
                PostProcessVolume postProcessVolume = gameObject.GetComponent<PostProcessVolume>();
                if (!postProcessVolume)
                    postProcessVolume = gameObject.AddComponent<PostProcessVolume>();

                postProcessVolume.blendDistance = _postProcessProfileInfo.BlendDistance;
                postProcessVolume.priority = _postProcessProfileInfo.Priority;
                postProcessVolume.weight = _postProcessProfileInfo.Weight;
                postProcessVolume.profile = _postProcessProfileInfo.PostProcessProfile;
                postProcessVolume.enabled = _postProcessProfileInfo.Enabled;

                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if (!meshCollider)
                    meshCollider = gameObject.AddComponent<MeshCollider>();

                meshCollider.convex = true;
                meshCollider.enabled = _postProcessProfileInfo.Enabled;
                meshCollider.isTrigger = true;

                float3[] polygonPoints = new float3[Nodes.Count];
                for (int i = 0; i <= Nodes.Count - 1; i++)
                    polygonPoints[i] = Nodes[i].Position;
                meshCollider.sharedMesh = MeshUtility.ExtrudeMeshFromPolygon(polygonPoints, _postProcessProfileInfo.VolumeHeight);
            }
        }
#endif
    }
}