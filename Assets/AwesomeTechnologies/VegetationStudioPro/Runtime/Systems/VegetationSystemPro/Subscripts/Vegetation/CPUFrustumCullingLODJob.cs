using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Culling
{
    [BurstCompile]
    public struct CPUFrustumCullingLODJob : IJob
    {
        [ReadOnly] public NativeArray<Plane> FrustumPlanes;

        [ReadOnly] public NativeList<MatrixInstance> MergedMatrixInstanceList;
        [WriteOnly] public NativeList<Matrix4x4> MatrixListLOD0;
        [WriteOnly] public NativeList<Matrix4x4> MatrixListLOD1;
        [WriteOnly] public NativeList<Matrix4x4> MatrixListLOD2;
        [WriteOnly] public NativeList<Matrix4x4> MatrixListLOD3;
        [WriteOnly] public NativeList<Vector4> FadeListLOD0;
        [WriteOnly] public NativeList<Vector4> FadeListLOD1;
        [WriteOnly] public NativeList<Vector4> FadeListLOD2;
        [WriteOnly] public NativeList<Vector4> FadeListLOD3;
        [WriteOnly] public NativeList<Matrix4x4> ShadowMatrixListLOD0;
        [WriteOnly] public NativeList<Matrix4x4> ShadowMatrixListLOD1;
        [WriteOnly] public NativeList<Matrix4x4> ShadowMatrixListLOD2;
        [WriteOnly] public NativeList<Matrix4x4> ShadowMatrixListLOD3;
        [WriteOnly] public NativeList<Vector4> ShadowFadeListLOD0;
        [WriteOnly] public NativeList<Vector4> ShadowFadeListLOD1;
        [WriteOnly] public NativeList<Vector4> ShadowFadeListLOD2;
        [WriteOnly] public NativeList<Vector4> ShadowFadeListLOD3;

        [ReadOnly] public float CullDistance;
        [ReadOnly] public float3 CameraPosition;
        [ReadOnly] public float3 FloatingOriginOffset;

        [ReadOnly] public bool NoFrustumCulling;
        [ReadOnly] public bool HasBackShadow;
        [ReadOnly] public float3 LightDirection;

        [ReadOnly] public float3 ItemBoundsCenter;
        [ReadOnly] public float3 ItemBoundsExtents;

        [ReadOnly] public bool UseLODFade;
        [ReadOnly] public int LODCount;
        [ReadOnly] public int MaxLODIndex;
        [ReadOnly] public int MaxLOD0;
        [ReadOnly] public int MaxLOD1;
        [ReadOnly] public int MaxLOD2;
        [ReadOnly] public int MaxLOD3;
        [ReadOnly] public int ShadowLODIndex;
        [ReadOnly] public int CustomShadowLODIndex;
        [ReadOnly] public float LODFactor;
        [ReadOnly] public float LODBias;
        [ReadOnly] public float LODFadeDistance;

        [ReadOnly] public float ItemLod0To1Distance;
        [ReadOnly] public float ItemLod1To2Distance;
        [ReadOnly] public float ItemLod2To3Distance;

        float3 ExtractTranslationFromMatrix(Matrix4x4 _matrix)
        {
            float3 translate;
            translate.x = _matrix.m03;
            translate.y = _matrix.m13;
            translate.z = _matrix.m23;
            return translate;
        }

        float3 ExtractScaleFromMatrix(Matrix4x4 _matrix)
        {
            float3 scale;
            scale.x = _matrix.GetColumn(0).magnitude;
            scale.y = _matrix.GetColumn(1).magnitude;
            scale.z = _matrix.GetColumn(2).magnitude;
            return scale;
        }

        Matrix4x4 TranslateMatrix(Matrix4x4 _matrix, float3 _offset)
        {
            Matrix4x4 translatedMatrix = _matrix;
            translatedMatrix.m03 = _matrix.m03 + _offset.x;
            translatedMatrix.m13 = _matrix.m13 + _offset.y;
            translatedMatrix.m23 = _matrix.m23 + _offset.z;
            return translatedMatrix;
        }

        //bool SphereInFrustum(BoundingSphere _boundingSphere)
        //{
        //    for (int i = 0; i < FrustumPlanes.Length; i++)
        //    {
        //        // get the angle between plane and object
        //        // add the plane's distance (to the world origin) to remove the offset / to localize our position
        //        // compare the angle to the object's radius to detect if it is outside of the view frustum
        //        if (math.dot(FrustumPlanes[i].normal, _boundingSphere.position) + FrustumPlanes[i].distance < -_boundingSphere.radius)  // radius needs to be inversed
        //            return false;   // object not in frustum
        //    }

        //    return true;    // object in frustum
        //}

        bool BoundsInFrustum(Bounds _bounds)
        {
            for (int i = 0; i < FrustumPlanes.Length; i++)
                if (math.dot(FrustumPlanes[i].normal, _bounds.center) + FrustumPlanes[i].distance + math.mul(_bounds.extents, math.abs(FrustumPlanes[i].normal)) < 0)
                    return false;
            return true;
        }

        float GetFadePercentage(float _cameraDistance, float _nextDistance)
        {
            float distance = _nextDistance + LODFadeDistance - _cameraDistance;
            if (distance <= LODFadeDistance)    // "single fade" -- to fade single LODs/end-pieces
                return math.clamp(distance / LODFadeDistance * 2, 0, 1);
            return 1;
        }

        float GetFadePercentage(float _thisDistance, float _cameraDistance, float _nextDistance, bool _isFixed = false)
        {
            float distance = _cameraDistance - _thisDistance;
            if (distance <= LODFadeDistance)    // "dual fade" -- to fade the crossovers for multi LODs/middle segments
                return _isFixed ? 1 : math.clamp(distance / LODFadeDistance * 2, 0, 1);

            distance = _nextDistance + LODFadeDistance - _cameraDistance;
            if (distance <= LODFadeDistance)    // "single fade" -- to fade single LODs/end-pieces
                return math.clamp(distance / LODFadeDistance * 2, 0, 1);

            return 1;
        }

        float3 CalculateShadowLength(Ray _ray)
        {
            float3 worldPlaneNormal = new(0, 1, 0);
            float objectAngle = math.dot(_ray.origin, worldPlaneNormal);
            float sunAngle = math.dot(-_ray.direction, worldPlaneNormal);
            if (sunAngle < 0.00001f)
                sunAngle = 0.00001f;

            float shadowLength = objectAngle / sunAngle;
            return _ray.origin + _ray.direction * shadowLength;
        }

        Bounds GetShadowBounds(Bounds _bounds)
        {
            float3 objectBoundsMin = _bounds.min;   // effectively center - extents -- don't calculate more often than needed
            float3 objectBoundsMax = _bounds.max;   // effectively center + extents -- don't calculate more often than needed
            _bounds.Encapsulate(CalculateShadowLength(new(new float3(objectBoundsMax.x, objectBoundsMax.y, objectBoundsMax.z), LightDirection)));   // top right
            _bounds.Encapsulate(CalculateShadowLength(new(new float3(objectBoundsMax.x, objectBoundsMax.y, objectBoundsMin.z), LightDirection)));   // top left
            _bounds.Encapsulate(CalculateShadowLength(new(new float3(objectBoundsMin.x, objectBoundsMax.y, objectBoundsMax.z), LightDirection)));   // bottom right
            _bounds.Encapsulate(CalculateShadowLength(new(new float3(objectBoundsMin.x, objectBoundsMax.y, objectBoundsMin.z), LightDirection)));   // bottom left
            return _bounds;
        }

        void AppendShadowData(Matrix4x4 _instanceData, int _defaultIndex, float _fadePercentage)
        {
            if (ShadowLODIndex < _defaultIndex)
                return; // apply "shadow LOD distance limitation"

            if (CustomShadowLODIndex > 0)   // apply a custom shadow LOD to each LOD level w/ fallback
            {
                if ((CustomShadowLODIndex == 3 && MaxLODIndex > 2) || _defaultIndex == 3)
                {
                    ShadowMatrixListLOD3.Add(_instanceData);
                    ShadowFadeListLOD3.Add(new(_fadePercentage, 0, 0, 0));
                }
                else if ((CustomShadowLODIndex == 2 && MaxLODIndex > 1) || _defaultIndex == 2)
                {
                    ShadowMatrixListLOD2.Add(_instanceData);
                    ShadowFadeListLOD2.Add(new(_fadePercentage, 0, 0, 0));
                }
                else if ((CustomShadowLODIndex == 1 && MaxLODIndex > 0) || _defaultIndex == 1)
                {
                    ShadowMatrixListLOD1.Add(_instanceData);
                    ShadowFadeListLOD1.Add(new(_fadePercentage, 0, 0, 0));
                }
            }
            else    // no custom shadow chosen -- use normal per LOD shadows
            {
                if (_defaultIndex == 0)
                {
                    ShadowMatrixListLOD0.Add(_instanceData);
                    ShadowFadeListLOD0.Add(new(_fadePercentage, 0, 0, 0));
                }
                else if (_defaultIndex == 1)
                {
                    ShadowMatrixListLOD1.Add(_instanceData);
                    ShadowFadeListLOD1.Add(new(_fadePercentage, 0, 0, 0));
                }
                else if (_defaultIndex == 2)
                {
                    ShadowMatrixListLOD2.Add(_instanceData);
                    ShadowFadeListLOD2.Add(new(_fadePercentage, 0, 0, 0));
                }
                else if (_defaultIndex == 3)
                {
                    ShadowMatrixListLOD3.Add(_instanceData);
                    ShadowFadeListLOD3.Add(new(_fadePercentage, 0, 0, 0));
                }
            }
        }

        void RenderShadows(bool _shouldCull, Bounds _bounds, MatrixInstance _mergeMatrixInstanceList, float _camToItemDistance, float _lod0To1Distance, float _lod1To2Distance, float _lod2To3Distance, float _itemCullDistance)
        {
            if (ShadowLODIndex == -1) return;   // skip since no shadow rendering

            if (_shouldCull)
                if (BoundsInFrustum(GetShadowBounds(_bounds)) == false)
                    return; // return if shadow is not potentially visible anymore

            bool useFixedShadow = CustomShadowLODIndex > 0;

            bool skipLODFade = CustomShadowLODIndex > 0 && LODCount > 0 && ShadowLODIndex > 0;  // whether custom LOD is used -- it's the last LOD -- it's the last shadow LOD
            if (_camToItemDistance <= _lod0To1Distance + (skipLODFade ? 0 : LODFadeDistance))   // within LOD0 range => begin transition to LOD0
                AppendShadowData(_mergeMatrixInstanceList.matrix, MaxLOD0, !UseLODFade || skipLODFade ? 1 : GetFadePercentage(_camToItemDistance, _lod0To1Distance));

            skipLODFade = CustomShadowLODIndex > 1 && LODCount > 1 && ShadowLODIndex > 1;
            if (LODCount > 1 && _camToItemDistance <= _lod1To2Distance + (skipLODFade ? 0 : LODFadeDistance) && _camToItemDistance > _lod0To1Distance)  // out of range of LOD1 => begin transition to LOD2
                AppendShadowData(_mergeMatrixInstanceList.matrix, MaxLOD1, !UseLODFade || skipLODFade ? 1 : GetFadePercentage(_lod0To1Distance, _camToItemDistance, _lod1To2Distance, useFixedShadow));

            skipLODFade = CustomShadowLODIndex > 2 && LODCount > 2 && ShadowLODIndex > 2;
            if (LODCount > 2 && _camToItemDistance <= _lod2To3Distance + (skipLODFade ? 0 : LODFadeDistance) && _camToItemDistance > _lod1To2Distance)  // out of range of LOD1 => begin transition to LOD2
                AppendShadowData(_mergeMatrixInstanceList.matrix, MaxLOD2, !UseLODFade || skipLODFade ? 1 : GetFadePercentage(_lod1To2Distance, _camToItemDistance, _lod2To3Distance, useFixedShadow));

            if (LODCount > 3 && _camToItemDistance > _lod2To3Distance)  // out of range of LOD2 => => begin transition to LOD3 => permanent LOD3 until culled w/ transition
                AppendShadowData(_mergeMatrixInstanceList.matrix, MaxLOD3, !UseLODFade ? 1 : GetFadePercentage(_lod2To3Distance, _camToItemDistance, _itemCullDistance, useFixedShadow));
        }

        void RenderVegetation(MatrixInstance _mergeMatrixInstanceList, float _camToItemDistance, float _lod0To1Distance, float _lod1To2Distance, float _lod2To3Distance, float _itemCullDistance)
        {
            if (_camToItemDistance <= _lod0To1Distance + LODFadeDistance)   // within LOD0 range => begin transition to LOD0
            {
                (MaxLOD0 == 3 ? FadeListLOD3 : MaxLOD0 == 2 ? FadeListLOD2 : MaxLOD0 == 1 ? FadeListLOD1 : FadeListLOD0).Add(new float4(!UseLODFade ? 1 : GetFadePercentage(_camToItemDistance, _lod0To1Distance), 0, 0, 0));
                (MaxLOD0 == 3 ? MatrixListLOD3 : MaxLOD0 == 2 ? MatrixListLOD2 : MaxLOD0 == 1 ? MatrixListLOD1 : MatrixListLOD0).Add(_mergeMatrixInstanceList.matrix);
            }

            if (LODCount > 1 && _camToItemDistance <= _lod1To2Distance + LODFadeDistance && _camToItemDistance > _lod0To1Distance)  // out of range of LOD0 => begin transition to LOD1
            {
                (MaxLOD1 == 3 ? FadeListLOD3 : MaxLOD1 == 2 ? FadeListLOD2 : FadeListLOD1).Add(new float4(!UseLODFade ? 1 : GetFadePercentage(_lod0To1Distance, _camToItemDistance, _lod1To2Distance), 0, 0, 0));
                (MaxLOD1 == 3 ? MatrixListLOD3 : MaxLOD1 == 2 ? MatrixListLOD2 : MatrixListLOD1).Add(_mergeMatrixInstanceList.matrix);
            }

            if (LODCount > 2 && _camToItemDistance <= _lod2To3Distance + LODFadeDistance && _camToItemDistance > _lod1To2Distance)  // out of range of LOD1 => begin transition to LOD2
            {
                (MaxLOD2 == 3 ? FadeListLOD3 : FadeListLOD2).Add(new float4(!UseLODFade ? 1 : GetFadePercentage(_lod1To2Distance, _camToItemDistance, _lod2To3Distance), 0, 0, 0));
                (MaxLOD2 == 3 ? MatrixListLOD3 : MatrixListLOD2).Add(_mergeMatrixInstanceList.matrix);
            }

            if (LODCount > 3 && _camToItemDistance > _lod2To3Distance)  // out of range of LOD2 => => begin transition to LOD3 => permanent LOD3 until culled w/ transition
            {
                FadeListLOD3.Add(new float4(!UseLODFade ? 1 : GetFadePercentage(_lod2To3Distance, _camToItemDistance, _itemCullDistance), 0, 0, 0));
                MatrixListLOD3.Add(_mergeMatrixInstanceList.matrix);
            }
        }

        public void Execute()
        {
            for (int i = 0; i < MergedMatrixInstanceList.Length; i++)
            {
                if (MergedMatrixInstanceList[i].controlData.x <= 0)
                    continue;   // skip rendering certain vegetation instances ex: masked out persistent storage instances

                // get matrix info w/ floating offset
                MatrixInstance mergedMatrixInstanceList = MergedMatrixInstanceList[i];
                mergedMatrixInstanceList.matrix = TranslateMatrix(mergedMatrixInstanceList.matrix, FloatingOriginOffset);
                float3 scale = ExtractScaleFromMatrix(mergedMatrixInstanceList.matrix);

                // calculate camPos vs vegInstancePos
                float3 itemPosition = ExtractTranslationFromMatrix(mergedMatrixInstanceList.matrix);
                float camToItemDistance = math.distance(CameraPosition, itemPosition);

                // calculate effective bounds
                float3 boundsCenter = itemPosition + new float3(0, ItemBoundsCenter.y * scale.y, 0);
                Bounds bounds = new();
                bounds.center = boundsCenter;
                bounds.extents = ItemBoundsExtents * scale;

                // calculate cull distance
                float itemCullDistance = CullDistance * mergedMatrixInstanceList.controlData.x;

                // culling distance filter
                if (camToItemDistance > itemCullDistance + (UseLODFade ? LODFadeDistance : 0))
                    continue;

                // calculate lod distances
                float lod0To1Distance = math.clamp(ItemLod0To1Distance * LODFactor * LODBias * mergedMatrixInstanceList.controlData.x, 0, itemCullDistance);
                float lod1To2Distance = math.clamp(ItemLod1To2Distance * LODFactor * LODBias * mergedMatrixInstanceList.controlData.x, 0, itemCullDistance);
                float lod2To3Distance = math.clamp(ItemLod2To3Distance * LODFactor * LODBias * mergedMatrixInstanceList.controlData.x, 0, itemCullDistance);

                // adjust max distance to match LODCount
                if (LODCount == 1)
                    lod0To1Distance = math.max(lod0To1Distance, itemCullDistance);
                else if (LODCount == 2)
                    lod1To2Distance = math.max(lod1To2Distance, itemCullDistance);
                else if (LODCount == 3)
                    lod2To3Distance = math.max(lod2To3Distance, itemCullDistance);

                if (NoFrustumCulling)
                {   // no frustum culling / render objects and their shadows in 360 around the camera within culling distance
                    RenderVegetation(mergedMatrixInstanceList, camToItemDistance, lod0To1Distance, lod1To2Distance, lod2To3Distance, itemCullDistance);
                    RenderShadows(false, bounds, mergedMatrixInstanceList, camToItemDistance, lod0To1Distance, lod1To2Distance, lod2To3Distance, itemCullDistance);
                }
                else
                {
                    // calculate if the boundingBox of the vegetationInstance is in the view frustum
                    if (BoundsInFrustum(bounds) == false)
                    {   // don't render objects out of the view frustum
                        if (!HasBackShadow) continue;   // whether shadow frustum culling is valid for this item
                        RenderShadows(true, bounds, mergedMatrixInstanceList, camToItemDistance, lod0To1Distance, lod1To2Distance, lod2To3Distance, itemCullDistance);
                    }
                    else
                    {   // frustum culling / render objects and their shadows within the view frustum within culling distance
                        RenderVegetation(mergedMatrixInstanceList, camToItemDistance, lod0To1Distance, lod1To2Distance, lod2To3Distance, itemCullDistance);
                        RenderShadows(true, bounds, mergedMatrixInstanceList, camToItemDistance, lod0To1Distance, lod1To2Distance, lod2To3Distance, itemCullDistance);
                    }
                }
            }
        }
    }
}