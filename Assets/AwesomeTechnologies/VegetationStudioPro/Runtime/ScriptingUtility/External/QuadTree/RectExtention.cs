using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public static class RectExtension
    {
        public static bool Contains(this Rect _self, Rect _rect)
        {
            return _self.Contains(new float2(_rect.xMin, _rect.yMin)) && _self.Contains(new float2(_rect.xMax, _rect.yMax));
        }

        public static void FromBounds(this Rect _self, Bounds _bounds)
        {
            _self.xMin = _bounds.center.x - _bounds.extents.x;
            _self.yMin = _bounds.center.z - _bounds.extents.z;
            _self.width = _bounds.size.x;
            _self.height = _bounds.size.z;
        }

        public static Rect CreateRectFromBounds(Bounds _bounds)
        {
            return new Rect(_bounds.center.x - _bounds.extents.x, _bounds.center.z - _bounds.extents.z, _bounds.size.x, _bounds.size.z);
        }

        public static Bounds CreateBoundsFromRect(Rect _rect)
        {
            return new Bounds(new float3(_rect.center.x, 0, _rect.center.y), new float3(_rect.size.x, 0, _rect.size.y));
        }

        public static Bounds CreateBoundsFromRect(Rect _rect, float _centerY)
        {
            return new Bounds(new float3(_rect.center.x, _centerY, _rect.center.y), new float3(_rect.size.x, 0, _rect.size.y));
        }

        public static Bounds CreateBoundsFromRect(Rect _rect, float _centerY, float _sizeY)
        {
            return new Bounds(new float3(_rect.center.x, _centerY, _rect.center.y), new float3(_rect.size.x, _sizeY, _rect.size.y));
        }
    }
}