using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    public class BiomeMaskSortOrderComparer : IComparer<PolygonMaskBiome>
    {
        public int Compare(PolygonMaskBiome _x, PolygonMaskBiome _y)
        {
            if (_x != null && _y != null)
                return _x.BiomeSortOrder.CompareTo(_y.BiomeSortOrder);
            else
                return 0;
        }
    }

    public class PolygonMaskBiome
    {
        public Bounds MaskBounds;
        public BiomeType BiomeType;
        public float BlendDistance;
        public bool UseNoise;
        public float NoiseScale;
        public int BiomeSortOrder;
        private Rect polygonRect;

        public delegate void MultionMaskDeleteDelegate(PolygonMaskBiome _maskArea);
        public MultionMaskDeleteDelegate OnMaskDeleteDelegate;

        public void CallDeleteEvent()
        {
            OnMaskDeleteDelegate?.Invoke(this);
        }

        private LineSegment2D[] segments;
        private float2[] points2D;
        private float3[] points3D;
        public NativeArray<float2> PolygonArray;
        public NativeArray<LineSegment2D> SegmentArray;
        public NativeArray<float> CurveArray;
        public NativeArray<float> InverseCurveArray;
        public NativeArray<float> TextureCurveArray;

        private bool[] disableEdges;

        public void AddPolygon(List<float3> _pointList, List<bool> _disableEdgeList)
        {
            if (_pointList.Count == 0)
                return;

            disableEdges = _disableEdgeList.ToArray();

            points2D = new float2[_pointList.Count];
            points3D = new float3[_pointList.Count];
            for (int i = 0; i < _pointList.Count; i++)
            {
                points2D[i] = new float2(_pointList[i].x, _pointList[i].z);
                points3D[i] = _pointList[i];
            }
            MaskBounds = GetMaskBounds();

            if (PolygonArray.IsCreated)
                PolygonArray.Dispose();
            PolygonArray = new NativeArray<float2>(points2D.Length, Allocator.Persistent);  // persistent as used by several jobs across several (sub-)systems
            PolygonArray.CopyFromFast(points2D);

            CreateSegments();

            polygonRect = RectExtension.CreateRectFromBounds(MaskBounds);
        }

        public void SetCurve(float[] _curveArray)
        {
            if (CurveArray.IsCreated)
                CurveArray.Dispose();
            CurveArray = new NativeArray<float>(_curveArray.Length, Allocator.Persistent);  // persistent as used by several jobs across several (sub-)systems
            CurveArray.CopyFromFast(_curveArray);
        }

        public void SetInverseCurve(float[] _curveArray)
        {
            if (InverseCurveArray.IsCreated)
                InverseCurveArray.Dispose();
            InverseCurveArray = new NativeArray<float>(_curveArray.Length, Allocator.Persistent);   // persistent as used by several jobs across several (sub-)systems
            InverseCurveArray.CopyFromFast(_curveArray);
        }

        public void SetTextureCurve(float[] _curveArray)
        {
            if (TextureCurveArray.IsCreated)
                TextureCurveArray.Dispose();
            TextureCurveArray = new NativeArray<float>(_curveArray.Length, Allocator.Persistent);   // persistent as used by several jobs across several (sub-)systems
            TextureCurveArray.CopyFromFast(_curveArray);
        }

        void CreateSegments()
        {
            segments = new LineSegment2D[points2D.Length];

            for (int i = 0; i < points2D.Length - 1; i++)
            {
                LineSegment2D lineSegment2D = new(points2D[i], points2D[i + 1]);
                segments[i] = lineSegment2D;

                if (disableEdges[i] && disableEdges[i + 1])
                    segments[i].DisableEdge = 1;
            }

            if (points2D.Length > 0)
            {
                LineSegment2D lineSegment2D = new(points2D[0], points2D[points2D.Length - 1]);
                segments[points2D.Length - 1] = lineSegment2D;

                if (disableEdges[0] && disableEdges[points2D.Length - 1])
                    segments[points2D.Length - 1].DisableEdge = 1;
            }

            if (SegmentArray.IsCreated)
                SegmentArray.Dispose();
            SegmentArray = new NativeArray<LineSegment2D>(segments.Length, Allocator.Persistent);   // persistent as used by several jobs across several (sub-)systems
            SegmentArray.CopyFromFast(segments);
        }

        public bool Contains(float3 _point)
        {
            if (PolygonArray.IsCreated == false)
                return false;
            return IsInPolygon(new float2(_point.x, _point.z));
        }

        public JobHandle BiomeMaskIncludeJob(VegetationInstanceData _instanceData, BiomeType _currentBiomeType, int _sampleCount, JobHandle _dependsOn)
        {
            BiomeMaskIncludeJob biomeMaskIncludeJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,

                PolygonArray = PolygonArray,
                SegmentArray = SegmentArray,
                Include = _currentBiomeType == BiomeType,
                BlendDistance = BlendDistance,
                UseNoise = UseNoise,
                NoiseScale = NoiseScale,
                CurveArray = CurveArray,
                InverseCurveArray = InverseCurveArray,
                PolygonRect = polygonRect
            };
            return biomeMaskIncludeJob.ScheduleParallel(_sampleCount, 64, _dependsOn);
        }

        private Bounds GetMaskBounds()
        {
            Bounds expandedBounds = points3D.Length > 0 ? new Bounds(points3D[0], new float3(1, 1, 1)) : new Bounds(float3.zero, new float3(1, 1, 1));
            for (int i = 0; i < points3D.Length; i++)
                expandedBounds.Encapsulate(points3D[i]);
            return expandedBounds;
        }

        public void Dispose()
        {
            if (PolygonArray.IsCreated) PolygonArray.Dispose();
            if (SegmentArray.IsCreated) SegmentArray.Dispose();
            if (CurveArray.IsCreated) CurveArray.Dispose();
            if (InverseCurveArray.IsCreated) InverseCurveArray.Dispose();
            if (TextureCurveArray.IsCreated) TextureCurveArray.Dispose();
        }

        private bool IsInPolygon(float2 _point)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
                return false;

            float2 oldPoint = new(PolygonArray[^1].x, PolygonArray[^1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                if (PolygonArray[i].x > oldPoint.x)
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - oldPoint.y) * (PolygonArray[i].x - oldPoint.x) < (PolygonArray[i].y - oldPoint.y) * (_point.x - oldPoint.x))
                        inside = !inside;
                }
                else
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - PolygonArray[i].y) * (oldPoint.x - PolygonArray[i].x) < (oldPoint.y - PolygonArray[i].y) * (_point.x - PolygonArray[i].x))
                        inside = !inside;
                }

                oldPoint = PolygonArray[i];
            }

            return inside;
        }
    }

    [BurstCompile]
    public struct BiomeMaskIncludeJob : IJobFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;

        [ReadOnly] public NativeArray<float> CurveArray;
        [ReadOnly] public NativeArray<float> InverseCurveArray;
        [ReadOnly] public NativeArray<float2> PolygonArray;
        [ReadOnly] public NativeArray<LineSegment2D> SegmentArray;

        [ReadOnly] public bool Include;
        [ReadOnly] public bool UseNoise;
        [ReadOnly] public float NoiseScale;
        [ReadOnly] public float BlendDistance;
        [ReadOnly] public Rect PolygonRect;

        public void Execute(int index)
        {
            float2 point = new(Position[index].x, Position[index].z);
            if (PolygonRect.Contains(point) == false || IsInPolygon(point) == false)
                return;

            float2 controlData = ControlData[index];

            float originalSpawnChance = controlData.y;
            controlData.y = math.select(controlData.y = math.min(0, controlData.y), controlData.y = math.max(1, controlData.y), Include);

            float distanceToEdge = DistanceToEdge(point);
            controlData.x = math.select(controlData.x, math.min(distanceToEdge, controlData.x), Include);

            if (distanceToEdge < BlendDistance)
            {
                float perlinNoise = math.select(1, noise.cnoise(new float2(point.x / NoiseScale, point.y / NoiseScale)), UseNoise);
                perlinNoise = math.select(perlinNoise, 0, !Include && !UseNoise);
                controlData.y = math.select(math.max((SampleInverseCurveArray(distanceToEdge / BlendDistance)) * (1 - perlinNoise), controlData.y), math.min(SampleCurveArray(distanceToEdge / BlendDistance) * perlinNoise, controlData.y), Include);
                controlData.y = math.select(math.min(controlData.y, originalSpawnChance), math.max(controlData.y, originalSpawnChance), Include);
            }

            ControlData[index] = controlData;
        }

        private float SampleCurveArray(float _value)
        {
            if (CurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * CurveArray.Length);
            return CurveArray[math.clamp(index, 0, CurveArray.Length - 1)];
        }

        private float SampleInverseCurveArray(float _value)
        {
            if (InverseCurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * InverseCurveArray.Length);
            return InverseCurveArray[math.clamp(index, 0, InverseCurveArray.Length - 1)];
        }

        private float DistanceToEdge(float2 _point)
        {
            float distance = float.MaxValue;
            for (int i = 0; i < SegmentArray.Length; i++)
                if (SegmentArray[i].DisableEdge == 0)
                    distance = math.min(distance, SegmentArray[i].DistanceToPoint(_point));
            return distance;
        }

        private bool IsInPolygon(float2 _point)
        {
            bool inside = false;

            if (PolygonArray.Length < 3)
                return false;

            float2 oldPoint = new(PolygonArray[^1].x, PolygonArray[^1].y);

            for (int i = 0; i < PolygonArray.Length; i++)
            {
                if (PolygonArray[i].x > oldPoint.x)
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - oldPoint.y) * (PolygonArray[i].x - oldPoint.x) < (PolygonArray[i].y - oldPoint.y) * (_point.x - oldPoint.x))
                        inside = !inside;
                }
                else
                {
                    if ((PolygonArray[i].x < _point.x) == (_point.x <= oldPoint.x) && (_point.y - PolygonArray[i].y) * (oldPoint.x - PolygonArray[i].x) < (oldPoint.y - PolygonArray[i].y) * (_point.x - PolygonArray[i].x))
                        inside = !inside;
                }

                oldPoint = PolygonArray[i];
            }

            return inside;
        }
    }
}