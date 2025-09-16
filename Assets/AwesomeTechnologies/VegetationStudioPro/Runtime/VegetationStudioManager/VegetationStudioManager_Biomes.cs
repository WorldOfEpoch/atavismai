using AwesomeTechnologies.TerrainSystem;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    public partial class VegetationStudioManager
    {
        public static bool ShowBiomes
        {
            get { return showBiomes; }
            set
            {
                showBiomes = value;
            }
        }

        public static void AddBiomeMask(PolygonMaskBiome _maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_AddBiomeMask(_maskArea);
        }

        private void Instance_AddBiomeMask(PolygonMaskBiome _maskArea)
        {
            if (biomeMaskList.Contains(_maskArea) == false)
                biomeMaskList.Add(_maskArea);

            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i])
                    AddBiomeMaskToVegetationSystem(VegetationSystemList[i], _maskArea);
        }

        private static void AddBiomeMaskToVegetationSystem(VegetationSystemPro _vegetationSystem, PolygonMaskBiome _maskArea)
        {
            if (_vegetationSystem.vegetationCellQuadTree == null && _vegetationSystem.billboardCellQuadTree == null)
                return;

            _maskArea.BiomeSortOrder = _vegetationSystem.GetBiomeSortOrder(_maskArea.BiomeType);

            Rect maskRect = RectExtension.CreateRectFromBounds(_maskArea.MaskBounds);
            List<VegetationCell> selectedCellList = new();
            _vegetationSystem.vegetationCellQuadTree.Query(maskRect, selectedCellList);
            for (int i = 0; i < selectedCellList.Count; i++)
                selectedCellList[i].AddBiomeMask(_maskArea);

            List<BillboardCell> selectedBillboardCellList = new();
            _vegetationSystem.billboardCellQuadTree.Query(maskRect, selectedBillboardCellList);
            for (int i = 0; i < selectedBillboardCellList.Count; i++)
                selectedBillboardCellList[i].ClearCache();
        }

        public static void GenerateSplatMap(Bounds _bounds, bool _isDynamic)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                for (int i = 0; i < Instance.VegetationSystemList.Count; i++)
                {
                    TerrainSystemPro terrainSystem = Instance.VegetationSystemList[i].gameObject.GetComponent<TerrainSystemPro>();
                    if (terrainSystem == false)
                        continue;

                    if (terrainSystem.enableAutoSplatMapGeneration && _isDynamic)   // generate on node move ..if enabled
                        terrainSystem.GenerateSplatMap(_bounds, false);
                    else if (_isDynamic == false)
                        terrainSystem.GenerateSplatMap(_bounds, false); // generate on button press ..but not on node move if disabled

#if UNITY_EDITOR
                    terrainSystem.EnableTerrainHeatmap(false);
#endif
                }
            }
        }

        public static List<PolygonMaskBiome> GetBiomeMasks(BiomeType _biomeType)
        {
            if (!Instance) FindInstance();
            if (Instance)
                return Instance.Instance_GetBiomeMasks(_biomeType);
            else
                return new List<PolygonMaskBiome>();
        }

        public List<PolygonMaskBiome> Instance_GetBiomeMasks(BiomeType _biomeType)
        {
            List<PolygonMaskBiome> biomeList = new();
            for (int i = 0; i < biomeMaskList.Count; i++)
                if (biomeMaskList[i].BiomeType == _biomeType)
                    biomeList.Add(biomeMaskList[i]);
            return biomeList;
        }

        public static BiomeType GetBiomeType(Vector3 _position)
        {
            if (!Instance) FindInstance();
            if (Instance)
                Instance.Instance_GetBiomeType(_position);
            return BiomeType.Default;
        }

        public BiomeType Instance_GetBiomeType(Vector3 _position)
        {
            int currentSortOrder = -1;
            BiomeType currentBiomeType = BiomeType.Default;
            for (int i = 0; i < biomeMaskList.Count; i++)
                if (biomeMaskList[i].Contains(_position))
                    if (biomeMaskList[i].BiomeSortOrder > currentSortOrder)
                    {
                        currentSortOrder = biomeMaskList[i].BiomeSortOrder;
                        currentBiomeType = biomeMaskList[i].BiomeType;
                    }
            return currentBiomeType;
        }

        public static void RemoveBiomeMask(PolygonMaskBiome _maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_RemoveBiomeMask(_maskArea);
        }

        private void Instance_RemoveBiomeMask(PolygonMaskBiome _maskArea)
        {
            biomeMaskList.Remove(_maskArea);

            Rect maskRect = RectExtension.CreateRectFromBounds(_maskArea.MaskBounds);
            List<BillboardCell> selectedBillboardCellList = new();
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

        private void DisposeBiomeMasks()
        {
            for (int i = 0; i < biomeMaskList.Count; i++)
            {
                biomeMaskList[i].CallDeleteEvent(); // remove from vegetation cell > clear cache
                biomeMaskList[i].Dispose();
            }

            biomeMaskList.Clear();
        }
    }
}