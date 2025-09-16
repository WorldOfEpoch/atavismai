using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Masks/VegetationMaskLine", 3)]
    [ScriptExecutionOrder(99)]
    //[ExecuteInEditMode]   // not needed as derived
    public class VegetationMaskLine : VegetationMask
    {
        private readonly List<LineMaskArea> lineMaskList = new();
        public float LineWidth = 5;

        public override void Reset()
        {
            ClosedArea = false;
            LineWidth = 5;
            base.Reset();
        }

        void OnDisable()
        {
            if (lineMaskList.Count > 0)
            {
                for (int i = 0; i < lineMaskList.Count; i++)
                    VegetationStudioManager.RemoveVegetationMask(lineMaskList[i]);
                lineMaskList.Clear();
            }
        }

        public override void UpdateVegetationMask() // call "PositionNodes" instead if applicable
        {
            if (enabled == false || gameObject.activeSelf == false)
                return;

            List<float3> worldSpaceNodeList = GetWorldSpaceNodePositions();

            if (lineMaskList.Count > 0)
            {
                for (int i = 0; i < lineMaskList.Count; i++)
                    VegetationStudioManager.RemoveVegetationMask(lineMaskList[i]);

                lineMaskList.Clear();
            }

            if (worldSpaceNodeList.Count > 1)
            {
                for (int i = 0; i < worldSpaceNodeList.Count - 1; i++)
                {
                    if (Nodes[i].Active == false)
                        continue;

                    float width = LineWidth;

                    if (Nodes[i].OverrideWidth)
                        width = Nodes[i].CustomWidth;

                    LineMaskArea maskArea = new()
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

                    maskArea.SetLineData(worldSpaceNodeList[i], worldSpaceNodeList[i + 1], width);

                    lineMaskList.Add(maskArea);
                    VegetationStudioManager.AddVegetationMask(maskArea);
                }
            }
        }
    }
}