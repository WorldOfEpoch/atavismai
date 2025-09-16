using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Masks/VegetationMaskArea", 2)]
    [ScriptExecutionOrder(99)]
    //[ExecuteInEditMode]   // not needed as derived
    public class VegetationMaskArea : VegetationMask
    {
        private PolygonMaskArea polygonMaskArea;
        public float tolerance = 0.2f;

        void OnDisable()
        {
            if (polygonMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(polygonMaskArea);
                polygonMaskArea = null;
            }
        }

        public override void UpdateVegetationMask() // call "PositionNodes" instead if applicable
        {
            if (enabled == false || gameObject.activeSelf == false)
                return;

            List<float3> worldSpaceNodeList = GetWorldSpaceNodePositions();

            PolygonMaskArea maskArea = new()
            {
                RemoveGrass = RemoveGrass && eMaskRadiusGrass != EMaskRadiusType.off,
                RemovePlants = RemovePlants && eMaskRadiusPlant != EMaskRadiusType.off,
                RemoveObjects = RemoveObjects && eMaskRadiusObject != EMaskRadiusType.off,
                RemoveLargeObjects = RemoveLargeObjects && eMaskRadiusLargeObject != EMaskRadiusType.off,
                RemoveTrees = RemoveTrees && eMaskRadiusTree != EMaskRadiusType.off,

                AdditionalGrassWidth = AdditionalGrassPerimiter,
                AdditionalPlantWidth = AdditionalPlantPerimiter,
                AdditionalObjectWidth = AdditionalObjectPerimiter,
                AdditionalLargeObjectWidth = AdditionalLargeObjectPerimiter,
                AdditionalTreeWidth = AdditionalTreePerimiter,

                AdditionalGrassWidthMax = AdditionalGrassPerimiterMax,
                AdditionalPlantWidthMax = AdditionalPlantPerimiterMax,
                AdditionalObjectWidthMax = AdditionalObjectPerimiterMax,
                AdditionalLargeObjectWidthMax = AdditionalLargeObjectPerimiterMax,
                AdditionalTreeWidthMax = AdditionalTreePerimiterMax,

                NoiseScaleGrass = NoiseScaleGrass,
                NoiseScalePlant = NoiseScalePlant,
                NoiseScaleObject = NoiseScaleObject,
                NoiseScaleLargeObject = NoiseScaleLargeObject,
                NoiseScaleTree = NoiseScaleTree,

                eMaskRadiusGrass = eMaskRadiusGrass,
                eMaskRadiusPlant = eMaskRadiusPlant,
                eMaskRadiusObject = eMaskRadiusObject,
                eMaskRadiusLargeObject = eMaskRadiusLargeObject,
                eMaskRadiusTree = eMaskRadiusTree,
            };

            if (maskArea.AdditionalGrassWidthMax < maskArea.AdditionalGrassWidth)
                maskArea.AdditionalGrassWidthMax = maskArea.AdditionalGrassWidth;

            if (maskArea.AdditionalPlantWidthMax < maskArea.AdditionalPlantWidth)
                maskArea.AdditionalPlantWidthMax = maskArea.AdditionalPlantWidth;

            if (maskArea.AdditionalObjectWidthMax < maskArea.AdditionalObjectWidth)
                maskArea.AdditionalObjectWidthMax = maskArea.AdditionalObjectWidth;

            if (maskArea.AdditionalLargeObjectWidthMax < maskArea.AdditionalLargeObjectWidth)
                maskArea.AdditionalLargeObjectWidthMax = maskArea.AdditionalLargeObjectWidth;

            if (maskArea.AdditionalTreeWidthMax < maskArea.AdditionalTreeWidth)
                maskArea.AdditionalTreeWidthMax = maskArea.AdditionalTreeWidth;

            if (IncludeVegetationType)
                AddVegetationTypes(maskArea);

            maskArea.AddPolygon(worldSpaceNodeList);

            if (polygonMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(polygonMaskArea);
                polygonMaskArea = null;
            }

            polygonMaskArea = maskArea;
            VegetationStudioManager.AddVegetationMask(maskArea);
        }

        public void GenerateHullNodes(float _tolerance, MeshFilter[] _meshFilters = null)
        {
            List<float2> worldSpacePointList = new();
            float minPos = float.PositiveInfinity;

            if (_meshFilters == null)
                _meshFilters = GetComponentsInChildren<MeshFilter>();

            for (int i = 0; i < _meshFilters?.Length; i++)
                if (_meshFilters[i] && _meshFilters[i].sharedMesh)
                {
                    List<Vector3> verticeList = new();
                    _meshFilters[i].sharedMesh.GetVertices(verticeList);
                    for (int j = 0; j < verticeList.Count; j++)
                    {
                        float3 worldSpaceVertex = _meshFilters[i].transform.TransformPoint(verticeList[j]);
                        worldSpacePointList.Add(worldSpaceVertex.xz);
                        if (worldSpaceVertex.y < minPos)
                            minPos = worldSpaceVertex.y;    // base ground alignment => get lowest point of all meshes that get outlined
                    }
                }

            List<float2> hullPointList = PolygonUtility.GetConvexHull(worldSpacePointList);
            List<float2> reducedPointList = PolygonUtility.DouglasPeuckerReduction(hullPointList, _tolerance);

            if (reducedPointList.Count >= 3)
            {
                Nodes.Clear();
                for (int i = 0; i < reducedPointList.Count; i++)
                    AddNode(new float3(reducedPointList[i].x, minPos, reducedPointList[i].y));
            }

            PositionNodes();    // internal update call of the VMA/VML -- additional terrain ground alignment => raycast the terrain/-s and align with the nearest world ground
        }
    }
}