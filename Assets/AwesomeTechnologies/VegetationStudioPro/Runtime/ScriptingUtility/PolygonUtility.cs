using AwesomeTechnologies.Utility.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public static class LineSegment2Dextention
    {
        public static float DistanceToPoint(this LineSegment2D _lineSegment, float2 _point)
        {
            return math.sqrt(SqrDistanceToPoint(_point, _lineSegment));
        }

        public static float SqrDistanceToPoint(float2 _point, LineSegment2D _segment)
        {
            float2 diff = _point - _segment.Center;
            float param = math.dot(_segment.Direction, diff);
            float2 closestPoint;

            if (-_segment.Extent < param)
            {
                if (param < _segment.Extent)
                    closestPoint = _segment.Center + (param * _segment.Direction);
                else
                    closestPoint = _segment.Point1;
            }
            else
            {
                closestPoint = _segment.Point0;
            }

            return math.lengthsq(closestPoint - _point);
        }
    }

    public struct LineSegment2D
    {
        public float2 Point0;
        public float2 Point1;
        public float2 Center;
        public float2 Direction;
        public readonly float Extent;
        public int DisableEdge;

        public LineSegment2D(float2 _point0, float2 _point1)
        {
            Point0 = _point0;
            Point1 = _point1;
            Center = 0.5f * (Point0 + Point1);
            Direction = Point1 - Point0;
            float directionLength = math.length(Direction);
            float inverseDirectionLength = 1f / directionLength;
            Direction *= inverseDirectionLength;
            Extent = 0.5f * directionLength;
            DisableEdge = 0;
        }
    }

    public struct LineSegment3D
    {
        public float3 Point0;
        public float3 Point1;
        public float3 Center;
        public float3 Direction;
        public float Extent;

        public LineSegment3D(float3 _point0, float3 _point1)
        {
            Point0 = _point0;
            Point1 = _point1;
            Center = Direction = float3.zero;
            Extent = 0f;
            CalcDir();
        }

        public void CalcDir()
        {
            Center = 0.5f * (Point0 + Point1);
            Direction = Point1 - Point0;
            var directionLength = math.length(Direction);
            var invDirectionLength = 1f / directionLength;
            Direction *= invDirectionLength;
            Extent = 0.5f * directionLength;
        }

        public float DistanceTo(float3 _point)
        {
            return math.sqrt(SqrPoint3Segment3(ref _point, ref this));
        }

        public static float SqrPoint3Segment3(ref float3 _point, ref LineSegment3D _segment)
        {
            var diff = _point - _segment.Center;
            var param = math.dot(_segment.Direction, diff);
            float3 closestPoint;

            if (-_segment.Extent < param)
            {
                if (param < _segment.Extent)
                    closestPoint = _segment.Center + param * _segment.Direction;
                else
                    closestPoint = _segment.Point1;
            }
            else
            {
                closestPoint = _segment.Point0;
            }

            return math.lengthsq(closestPoint - _point);
        }
    }

    public class PolygonUtility
    {
        public static void AlignPointsWithTerrain(List<Vector3> _pointList, bool _closePolygon, LayerMask _groundLayerMask)
        {
            for (int i = 0; i < _pointList.Count; i++)
            {
                RaycastHit[] hits = Physics.RaycastAll(new(_pointList[i] + new Vector3(0, 10000f, 0), Vector3.down), 11000f).OrderBy(h => h.distance).ToArray();
                for (int j = 0; j < hits.Length; j++)
                {
                    if (!(hits[j].collider is TerrainCollider || _groundLayerMask.Contains(hits[j].collider.gameObject.layer)))
                        continue;
                    _pointList[i] = hits[j].point;
                    break;
                }
            }

            if (_closePolygon && _pointList.Count > 0)
                _pointList.Add(_pointList[0]);
        }

        public static List<Vector3> InflatePolygon(List<Vector3> _pointList, double _offset, bool _closedPolygon)
        {
            List<Vector3> offsetPointList = new();

            List<External.ClipperLib.IntPoint> polygon = new();
            for (int i = 0; i < _pointList.Count; i++)
                polygon.Add(new External.ClipperLib.IntPoint(_pointList[i].x, _pointList[i].z));

            External.ClipperLib.ClipperOffset co = new();
            co.AddPath(polygon, External.ClipperLib.JoinType.jtRound, _closedPolygon ? External.ClipperLib.EndType.etClosedPolygon : External.ClipperLib.EndType.etOpenRound);

            List<List<External.ClipperLib.IntPoint>> solution = new();
            co.Execute(ref solution, _offset);

            for (int i = 0; i < solution.Count; i++)
                for (int j = 0; j < solution[i].Count; j++)
                    offsetPointList.Add(new float3(Convert.ToInt32(solution[i][j].X), 0, Convert.ToInt32(solution[i][j].Y)));
            return offsetPointList;
        }

        public static List<float2> DouglasPeucker(List<float2> _points, int _startIndex, int _lastIndex, float _epsilon)
        {
            float dmax = 0f;
            int index = _startIndex;

            for (int i = index + 1; i < _lastIndex; ++i)
            {
                float d = PointLineDistance(_points[i], _points[_startIndex], _points[_lastIndex]);
                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            if (dmax > _epsilon)
            {
                var res1 = DouglasPeucker(_points, _startIndex, index, _epsilon);
                var res2 = DouglasPeucker(_points, index, _lastIndex, _epsilon);

                var finalRes = new List<float2>();
                for (int i = 0; i < res1.Count - 1; ++i)
                    finalRes.Add(res1[i]);

                for (int i = 0; i < res2.Count; i++)
                    finalRes.Add(res2[i]);

                return finalRes;
            }
            else
            {
                return new List<float2>(new[] { _points[_startIndex], _points[_lastIndex] });
            }
        }

        public static float PointLineDistance(float2 _point, float2 _start, float2 _end)
        {
            if (_start.Equals(_end))
                return math.distance(_point, _start);

            float n = math.abs((_end.x - _start.x) * (_start.y - _point.y) - (_start.x - _point.x) * (_end.y - _start.y));
            float d = math.sqrt((_end.x - _start.x) * (_end.x - _start.x) + (_end.y - _start.y) * (_end.y - _start.y));

            return n / d;
        }

        public static double Cross(float2 _o, float2 _a, float2 _b)
        {
            return (_a.x - _o.x) * (_b.y - _o.y) - (_a.y - _o.y) * (_b.x - _o.x);
        }

        public static List<float2> GetConvexHull(List<float2> _points)
        {
            if (_points == null)
                return null;

            if (_points.Count <= 1)
                return _points;

            int n = _points.Count, k = 0;
            List<float2> h = new(new float2[2 * n]);

            _points.Sort((a, b) =>
                a.x.Equals(b.x) ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && Cross(h[k - 2], h[k - 1], _points[i]) <= 0)
                    k--;
                h[k++] = _points[i];
            }

            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && Cross(h[k - 2], h[k - 1], _points[i]) <= 0)
                    k--;
                h[k++] = _points[i];
            }

            return h.Take(k - 1).ToList();
        }

        public static List<float2> DouglasPeuckerReduction(List<float2> _pointList, float _tolerance)
        {
            if (_pointList == null || _pointList.Count < 3)
                return _pointList;

            int firstPoint = 0;
            int lastPoint = _pointList.Count - 1;
            List<int> pointIndexsToKeep = new() { firstPoint, lastPoint };

            while (_pointList[firstPoint].Equals(_pointList[lastPoint]))
                lastPoint--;

            DouglasPeuckerReduction(_pointList, firstPoint, lastPoint, _tolerance, ref pointIndexsToKeep);

            pointIndexsToKeep.Sort();

            return pointIndexsToKeep.Select(index => _pointList[index]).ToList();
        }

        private static void DouglasPeuckerReduction(List<float2> _points, int _firstPoint, int _lastPoint, float _tolerance, ref List<int> _pointIndexsToKeep)
        {
            float maxDistance = 0;
            int indexFarthest = 0;

            for (int index = _firstPoint; index < _lastPoint; index++)
            {
                float distance = PerpendicularDistance(_points[_firstPoint], _points[_lastPoint], _points[index]);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (!(maxDistance > _tolerance) || indexFarthest == 0)
                return;
            _pointIndexsToKeep.Add(indexFarthest);

            DouglasPeuckerReduction(_points, _firstPoint, indexFarthest, _tolerance, ref _pointIndexsToKeep);
            DouglasPeuckerReduction(_points, indexFarthest, _lastPoint, _tolerance, ref _pointIndexsToKeep);
        }

        public static float PerpendicularDistance(float2 _p1, float2 _p2, float2 _p)
        {
            float area = math.abs(.5f * (_p1.x * _p2.y + _p2.x * _p.y + _p.x * _p1.y - _p2.x * _p1.y - _p.x * _p2.y - _p1.x * _p.y));
            float bottom = math.sqrt(Mathf.Pow(_p1.x - _p2.x, 2) + math.pow(_p1.y - _p2.y, 2));

            float height = area / bottom * 2;
            return height;
        }
    }
}