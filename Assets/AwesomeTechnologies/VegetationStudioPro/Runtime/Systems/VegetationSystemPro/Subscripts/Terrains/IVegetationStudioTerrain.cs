using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public interface IVegetationStudioTerrain
    {
        string TerrainType { get; }

        Bounds TerrainBounds { get; }

        void Init();

        void RefreshTerrainData(bool _splatData = true, bool _holesData = true, bool _heightsData = true);

        void RefreshTerrainData(Bounds _bounds, bool _splatData = true, bool _holesData = true, bool _heightsData = true);

        JobHandle SampleCellHeight(NativeArray<Bounds> _vegetationCellBoundsList, float _worldspaceHeightCutoff, Rect _updateRect, JobHandle _dependsOn = default);

        JobHandle SampleTerrain(VegetationInstanceData _instanceData, int _sampleCount, Rect _cellRect, JobHandle _dependsOn);

        JobHandle SampleConcaveLocation(VegetationInstanceData _instanceData, float _minHeightDifference, float _distancePerSample, bool _inverse, Rect _cellRect, JobHandle _dependsOn);

        void VerifySplatmapAccess();

        JobHandle ProcessSplatmapIncludeRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn);

        JobHandle ProcessSplatmapExcludeRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn);

        JobHandle ProcessSplatmapScaleRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn);

        JobHandle ProcessSplatmapDensityRule(List<TerrainTextureRule> _terrainTextureRuleList, VegetationInstanceData _instanceData, Rect _cellRect, JobHandle _dependsOn);

        bool NeedsSplatmapUpdate(Bounds _updateBounds);

        void PrepareSplatmapGeneration(bool _clearExistingTextures, float _heightCurveWorldHeight, float _worldSpaceSeaLevel);

        void GenerateSplatmapBiome(BiomeType _biomeType, List<PolygonMaskBiome> _polygonBiomeMaskList, List<TerrainTextureSettings> _terrainTextureSettingsList, float _worldSpaceSeaLevel, bool _clearExistingTextures);

        void CompleteSplatmapGeneration();

        void AssignHeatmapMaterial();

        void UpdateTerrainMaterial(float _worldspaceSeaLevel, float _worldspaceMaxTerrainHeight, TerrainTextureSettings _terrainTextureSettings);

        void RestoreTerrainMaterial();

        Texture2D GetTerrainTexture(int _index);

        TerrainLayer[] GetTerrainLayers();

        void SetTerrainLayers(TerrainLayer[] _terrainLayers);
    }
}