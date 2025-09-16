#if UNITY_SPLINES
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Splines;

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    public enum ESplineSampleResolution
    {
        Low128 = 1024,
        Normal256 = 512,
        High512 = 256,
        Ultra1024 = 128
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Masks/BiomeMaskSpline", 0)]
    [ScriptExecutionOrder(99)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(SplineContainer))]
    public class BiomeMaskSpline : MonoBehaviour
    {
        public bool showSplinePathWidthMenu = true;

        private const string SPLINE_NAME = "BiomeMaskSpline";
        private const string AREA_NAME = "BiomeMaskArea";

        public bool isDirty;
        public SplineContainer splineContainer;
        public ESplineSampleResolution eSplineSampleResolution = ESplineSampleResolution.Normal256;
        public TangentMode tangentMode = TangentMode.AutoSmooth;
        public bool useKnotAutoSnapping;
        public bool shallSnapKnotsOnce;
        public bool haveKnotsBeenSnappedOnce;
        public LayerMask layerMask;

        public List<BiomeMaskArea> maskAreas = new();
        public BiomeType biomeType;
        [Range(0, 50)] readonly float pathWidth = 5;    // base value for new paths
        [Range(0, 50)] public List<float> pathWidthList = new();
        [Range(-1, 1)] public float blendBalance = 0.5f;

        public bool hasSplineContainerChanged;  // splat map generation flag
        private float3 lastPosition;
        private quaternion lastRotation;
        private float3 lastLossyScale;

        private void Reset()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            splineContainer = GetComponent<SplineContainer>();

            for (int i = 0; i < maskAreas.Count; i++)
                if (maskAreas[i] != null && maskAreas[i].gameObject != null)
                    maskAreas[i].gameObject.SetActive(true);

            isDirty = true;
        }

        private void Update()
        {
            if (splineContainer == null || splineContainer.Spline == null)
                return;

#if !UNITY_EDITOR
            if (Application.isPlaying && isDirty == false)
                return;
#endif

            if (lastPosition.Equals(transform.position) == false || lastRotation.Equals(transform.rotation) == false || lastLossyScale.Equals(transform.lossyScale) == false)
            {
                if (splineContainer.Spline.Knots.Count() > 0)
                    splineContainer.Spline.SetKnot(0, splineContainer.Spline.ElementAt(0)); // trigger Unity spline events => run UI-based auto-snapping ==> run "OnIsDirty" implicitly

                if (lastLossyScale.Equals(transform.lossyScale) == false)   // adjust scale for correct 1:1 behavior -- "mesh align" logic
                    for (int i = 0; i < maskAreas.Count; i++)
                        if (maskAreas[i] != null)
                            maskAreas[i].transform.localScale = new float3(1, 1, 1) / transform.lossyScale;

                lastPosition = transform.position;
                lastRotation = transform.rotation;
                lastLossyScale = transform.lossyScale;
            }

            if (isDirty)
            {
                //if (splineContainer.Spline.Knots.Count() == 1)
                //{
                //    BezierKnot knot = splineContainer.Spline.ElementAt(0);
                //    transform.position = transform.TransformPoint(knot.Position);
                //    knot.Position = float3.zero;
                //    splineContainer.Spline.SetKnot(0, knot);
                //}

                OnIsDirty();
            }
        }

        private void OnIsDirty()
        {
            VerifyMasks();
            PositionSplineKnots();
            AlignMasksToSplines();
            isDirty = false;
        }

        private void OnDisable()
        {
            for (int i = 0; i < maskAreas.Count; i++)
                if (maskAreas[i] != null && maskAreas[i].gameObject != null)
                    maskAreas[i].gameObject.SetActive(false);
        }

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (VegetationStudioManager.ShowBiomes)
                DrawAreaGizmo();
        }

        private void OnDrawGizmosSelected()
        {
            if (VegetationStudioManager.ShowBiomes == false)
                DrawAreaGizmo();
        }

        private void DrawAreaGizmo()
        {
#if UNITY_EDITOR
            for (int i = 0; i < maskAreas.Count; i++)
                if (maskAreas[i] != null)
                {
                    if (i >= splineContainer.Splines.Count)
                        break;

                    Handles.Label(maskAreas[i].GetMaskCenter(), maskAreas[i].BiomeType.ToString());
                    for (int j = 0; j < maskAreas[i].Nodes.Count; j++)
                        if (j == maskAreas[i].Nodes.Count - 1)
                        {
                            if (splineContainer.Splines[i].Closed)
                                Gizmos.DrawLine(transform.TransformPoint(maskAreas[i].Nodes[0].Position), transform.TransformPoint(maskAreas[i].Nodes[j].Position));
                        }
                        else
                            Gizmos.DrawLine(transform.TransformPoint(maskAreas[i].Nodes[j].Position), transform.TransformPoint(maskAreas[i].Nodes[j + 1].Position));
                }
#endif
        }
        #endregion

        private void VerifyMasks()
        {
            if (enabled == false || gameObject.activeSelf == false || splineContainer == null)
                return;

            if (maskAreas.Count != splineContainer.Splines.Count || pathWidthList.Count != splineContainer.Splines.Count || pathWidthList.Count != maskAreas.Count) // async state
            {
                RegenerateMasks();
                return;
            }

            for (int i = 0; i < maskAreas.Count; i++)
                if (maskAreas[i] == null)   // safety check
                {
                    RegenerateMasks();
                    return;
                }

            UpdateMasks();
        }

        private void RegenerateMasks()
        {
            if (enabled == false || gameObject.activeSelf == false || splineContainer == null)
                return;

            for (int i = 0; i < maskAreas.Count; i++)
                if (maskAreas[i] != null)
                    if (Application.isPlaying)
                        Destroy(maskAreas[i].gameObject);
                    else
                        DestroyImmediate(maskAreas[i].gameObject);
            maskAreas.Clear();

#if UNITY_EDITOR
            BiomeMaskArea[] children = transform.GetComponentsInChildren<BiomeMaskArea>(true);  // editor undo/redo workaround since it re-triggers the splineContainer events -- disables the gameObjects
            for (int i = 0; i < children.Length; i++)
                if (Application.isPlaying)
                    Destroy(children[i].gameObject);
                else
                    DestroyImmediate(children[i].gameObject);
#endif

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                GameObject go = new() { name = SPLINE_NAME + "_" + i, hideFlags = HideFlags.NotEditable };
                go.transform.SetParent(transform);

                BiomeMaskArea bma = go.AddComponent<BiomeMaskArea>();
                bma.transform.position = splineContainer.transform.position;
                bma.ShowArea = false;
                bma.ShowHandles = false;
                bma.useNodeAutoSnapping = false;
                maskAreas.Add(bma);
            }

            if (pathWidthList.Count > maskAreas.Count)
            {
                for (int i = pathWidthList.Count - 1; i > -1; i--)
                    if (i == maskAreas.Count - 1)
                        break;
                    else
                        pathWidthList.RemoveAt(i);
            }
            else if (pathWidthList.Count < maskAreas.Count)
                pathWidthList.Add(pathWidth);

            UpdateMasks();
        }

        public void UpdateMasks()
        {
            if (enabled == false || gameObject.activeSelf == false || splineContainer == null)
                return;

            for (int i = 0; i < maskAreas.Count; i++)
            {
                maskAreas[i].BiomeType = biomeType;
                maskAreas[i].BlendDistance = pathWidthList[i] * (1 - blendBalance);
            }
        }

        public void ClearSplineContainer()
        {
            for (int i = splineContainer.Splines.Count - 1; i > 0; i--) // remove all splines(knots) except (of) the first spline
                splineContainer.RemoveSplineAt(i);  // remove spline -- triger splineContainer event
            splineContainer.Spline?.Clear();    // clear first spline knots seperately -- trigger spline event

            if (splineContainer.Splines.Count <= 1)
                RegenerateMasks();  // manual call since splineContainer event not triggered
        }

        public void PositionSplineKnots()
        {
            if (enabled == false || gameObject.activeSelf == false || splineContainer == null)
                return;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                if (splineContainer.Splines[i] == null)
                    continue;

                for (int j = 0; j < splineContainer.Splines[i].Knots.Count(); j++)
                {
                    splineContainer.Splines[i].SetTangentMode(j, tangentMode);

                    if (useKnotAutoSnapping == false || (useKnotAutoSnapping && shallSnapKnotsOnce == false))
                        continue;

                    RaycastHit[] hits = Physics.RaycastAll(new(transform.TransformPoint(splineContainer.Splines[i].Knots.ElementAt(j).Position) + new Vector3(0, 10000, 0), Vector3.down)).OrderBy(h => h.distance).ToArray();
                    for (int k = 0; k < hits.Length; k++)
                        if (hits[k].collider is TerrainCollider || layerMask.Contains(hits[k].collider.gameObject.layer))
                        {
                            BezierKnot knot = splineContainer.Splines[i].ElementAt(j);
                            knot.Position = transform.InverseTransformPoint(hits[k].point);
                            knot.Rotation = tangentMode == TangentMode.AutoSmooth ? quaternion.identity : knot.Rotation;
                            splineContainer.Splines[i].SetKnot(j, knot);
                            haveKnotsBeenSnappedOnce = true;
                            break;
                        }
                }
            }

            shallSnapKnotsOnce = false;
        }

        public void AlignMasksToSplines()
        {
            if (enabled == false || gameObject.activeSelf == false || splineContainer == null)
                return;

            for (int i = 0; i < maskAreas.Count; i++)
            {
                maskAreas[i].Nodes.Clear();
                if (splineContainer.Splines[i].Knots.Count() <= 0)
                    continue;

                if (splineContainer.Splines[i].Closed)
                    CreateArea(maskAreas[i], i);
                else
                    CreatePath(maskAreas[i], i);
            }
        }

        private void CreateArea(BiomeMaskArea _bma, int _splineIndex)
        {
            _bma.name = AREA_NAME + "_" + _splineIndex;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                if (i != _splineIndex)
                    continue;

                if (tangentMode == TangentMode.Linear)
                {
                    for (int j = 0; j < splineContainer.Splines[i].Knots.Count(); j++)
                        _bma.Nodes.Add(new() { Position = splineContainer.Splines[i].ElementAt(j).Position });
                }
                else
                {
                    float length = splineContainer.Splines[i].GetLength();
                    int sampleCount = Mathf.ClosestPowerOfTwo((int)math.floor(length * (1 / math.max(math.lerp(1, (int)eSplineSampleResolution * 4, math.min(length / 70710.6781185f, 1)), 1))));   // 70710.6781185 = half of max possible terrain diagonal
                    float spacePerSample = 1f / sampleCount;    // "f" to enforce float division

                    for (int j = 0; j < sampleCount; j++)
                    {
                        SplineUtility.Evaluate(splineContainer.Splines[i], spacePerSample * j, out float3 _position, out _, out _);
                        _bma.Nodes.Add(new() { Position = _position });
                    }
                }
            }

            _bma.PositionNodes();   // internal update call of the BMA
        }

        private void CreatePath(BiomeMaskArea _bma, int _splineIndex)
        {
            _bma.name = SPLINE_NAME + "_" + _splineIndex;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                if (i != _splineIndex)
                    continue;

                if (tangentMode == TangentMode.Linear)
                {
                    for (int j = 0; j < splineContainer.Splines[i].Knots.Count(); j++)  // up
                        _bma.Nodes.Add(new() { Position = splineContainer.Splines[i].ElementAt(j).Position + math.mul(splineContainer.Splines[i].ElementAt(j).Rotation, new float3(pathWidthList[i], 0, 0)) }); // positive width

                    for (int j = splineContainer.Splines[i].Knots.Count() - 1; j >= 0; j--) // down
                        _bma.Nodes.Add(new() { Position = splineContainer.Splines[i].ElementAt(j).Position + math.mul(splineContainer.Splines[i].ElementAt(j).Rotation, new float3(-pathWidthList[i], 0, 0)) });    // negative width
                }
                else
                {
                    float length = splineContainer.Splines[i].GetLength();
                    int sampleCount = Mathf.ClosestPowerOfTwo((int)math.floor(length * (1 / math.max(math.lerp(1, (int)eSplineSampleResolution, math.min(length / 70710.6781185f, 1)), 1))));   // 70710.6781185 = half of max possible terrain diagonal
                    float spacePerSample = 1f / sampleCount;    // "f" to enforce float division

                    for (int j = 0; j < sampleCount + 1; j++)   // up
                    {
                        SplineUtility.Evaluate(splineContainer.Splines[i], spacePerSample * j, out float3 _position, out float3 _tangent, out float3 _up);
                        _bma.Nodes.Add(new() { Position = _position + math.mul(quaternion.LookRotation(_tangent, _up), new float3(pathWidthList[i], 0, 0)) });  // positive width
                    }

                    for (int j = sampleCount; j > -1; j--)  // down
                    {
                        SplineUtility.Evaluate(splineContainer.Splines[i], spacePerSample * j, out float3 _position, out float3 _tangent, out float3 _up);
                        _bma.Nodes.Add(new() { Position = _position + math.mul(quaternion.LookRotation(_tangent, _up), new float3(-pathWidthList[i], 0, 0)) }); // negative width
                    }
                }
            }

            _bma.PositionNodes();   // internal update call of the BMA
        }

        public void GenerateSplatmap(bool _isDynamic)
        {
            if (enabled == false || gameObject.activeSelf == false || splineContainer == null)
                return;

            for (int i = 0; i < maskAreas.Count; i++)
                if (maskAreas[i] != null && maskAreas[i]._currentMaskArea != null)
                    VegetationStudioManager.GenerateSplatMap(maskAreas[i]._currentMaskArea.MaskBounds, _isDynamic);
        }
    }
}
#endif