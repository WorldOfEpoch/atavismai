using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    public partial class VegetationStudioManager
    {
        public static void AddVegetationMask(BaseMaskArea _maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_AddVegetationMask(_maskArea);
        }

        private void Instance_AddVegetationMask(BaseMaskArea _maskArea)
        {
            if (vegetationMaskList.Contains(_maskArea) == false)
                vegetationMaskList.Add(_maskArea);

            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i])
                    AddVegetationMaskToVegetationSystem(VegetationSystemList[i], _maskArea);
        }

        private static void AddVegetationMaskToVegetationSystem(VegetationSystemPro _vegetationSystem, BaseMaskArea _maskArea)
        {
            if (_vegetationSystem.vegetationCellQuadTree == null || _vegetationSystem.billboardCellQuadTree == null)
                return;

            VegetationItemIndices vegetationItemIndices = _vegetationSystem.GetVegetationItemIndices(_maskArea.VegetationItemID);

            Rect maskRect = RectExtension.CreateRectFromBounds(_maskArea.MaskBounds);
            List<VegetationCell> selectedCellList = new();
            _vegetationSystem.vegetationCellQuadTree.Query(maskRect, selectedCellList);

            List<BillboardCell> selectedBillboardCellList = new();
            _vegetationSystem.billboardCellQuadTree.Query(maskRect, selectedBillboardCellList);

            if (vegetationItemIndices.vegetationPackageIndex > -1 && vegetationItemIndices.vegetationItemIndex > -1)    // for specific masks ex: "VegetationItemMask" -- clear cache of the given item
            {
                for (int i = 0; i < selectedCellList.Count; i++)
                    selectedCellList[i].AddVegetationMask(_maskArea, vegetationItemIndices.vegetationPackageIndex, vegetationItemIndices.vegetationItemIndex);

                for (int i = 0; i < selectedBillboardCellList.Count; i++)
                    selectedBillboardCellList[i].ClearCache(vegetationItemIndices.vegetationPackageIndex, vegetationItemIndices.vegetationItemIndex);
            }
            else
            {
                for (int i = 0; i < selectedCellList.Count; i++)
                    selectedCellList[i].AddVegetationMask(_maskArea);

                for (int i = 0; i < selectedBillboardCellList.Count; i++)
                    selectedBillboardCellList[i].ClearCache();
            }
        }

        public static void RemoveVegetationMask(BaseMaskArea _maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_RemoveVegetationMask(_maskArea);
        }

        private void Instance_RemoveVegetationMask(BaseMaskArea _maskArea)
        {
            vegetationMaskList.Remove(_maskArea);

            Rect maskRect = RectExtension.CreateRectFromBounds(_maskArea.MaskBounds);
            List<BillboardCell> selectedBillboardCellList = new();  // only needed for billboard cells here since vegetation cells are subscribed to the vegetation masks
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i])
                {
                    VegetationSystemList[i].billboardCellQuadTree.Query(maskRect, selectedBillboardCellList);
                    for (int j = 0; j < selectedBillboardCellList.Count; j++)
                        selectedBillboardCellList[j].ClearCache();
                }

            _maskArea.CallDeleteEvent();    // remove from vegetation cell > clear cache
            _maskArea.Dispose();
        }

        private void DisposeVegetationMasks()
        {
            for (int i = 0; i < vegetationMaskList.Count; i++)
            {
                vegetationMaskList[i].CallDeleteEvent();    // remove from vegetation cell > clear cache
                vegetationMaskList[i].Dispose();
            }

            vegetationMaskList.Clear();
        }
    }
}