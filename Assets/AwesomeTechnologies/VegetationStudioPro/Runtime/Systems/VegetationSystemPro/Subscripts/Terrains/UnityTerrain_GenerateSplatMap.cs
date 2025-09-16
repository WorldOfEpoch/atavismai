using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class UnityTerrain
    {
        public bool NeedsSplatmapUpdate(Bounds _updateBounds)
        {
            return RectExtension.CreateRectFromBounds(_updateBounds).Overlaps(RectExtension.CreateRectFromBounds(TerrainBounds));
        }

        public void PrepareSplatmapGeneration(bool _clearExistingTextures, float _heightCurveWorldHeight, float _worldSpaceSeaLevel)
        {
            int heightmapLength = Terrain.terrainData.alphamapResolution * Terrain.terrainData.alphamapResolution;
            int splatmapLength = heightmapLength * Terrain.terrainData.alphamapLayers;

            currentSplatmapData = new NativeArray<float>(splatmapLength, Allocator.TempJob);    // terrain's current splat map
            if (_clearExistingTextures == false && currentSplatmapData.Length > 0)
                currentSplatmapData.CopyFromFast(Terrain.terrainData.GetAlphamaps(0, 0, Terrain.terrainData.alphamapResolution, Terrain.terrainData.alphamapResolution));   // copy over existing splat map to re-write "copied textures" later

            relativeHeightmapData = new NativeArray<RelativeHeightmapData>(heightmapLength, Allocator.TempJob); // terrain's current height map
            RefreshTerrainData(false, false, !heights.IsCreated);   // get terrain's height values from its height map

            SampleHeightmapJob sampleHeightmapJob = new()
            {
                RelativeHeightmapData = relativeHeightmapData,
                Heights = heights,

                TerrainPositionY = transform.position.y,
                WorldSpaceSeaLevel = _worldSpaceSeaLevel,
                WorldHeight = _heightCurveWorldHeight,

                HeightmapScale = Terrain.terrainData.heightmapScale,
                HeightmapResolution = Terrain.terrainData.heightmapResolution,
                AlphamapResolution = Terrain.terrainData.alphamapResolution,
            };
            splatmapHandle = sampleHeightmapJob.ScheduleParallel(heightmapLength, 64, default);
        }

        public void GenerateSplatmapBiome(BiomeType _biomeType, List<PolygonMaskBiome> _polygonBiomeMaskList, List<TerrainTextureSettings> _terrainTextureSettingsList, float _worldSpaceSeaLevel, bool _clearExistingTextures)
        {
            int blendDataLength = Terrain.terrainData.alphamapResolution * Terrain.terrainData.alphamapResolution;
            int splatmapLength = blendDataLength * Terrain.terrainData.alphamapLayers;

            NativeArray<float> blendData = new(blendDataLength, Allocator.TempJob); // terrain's new splat map w/ copied data + new data => based on this biome
            NativeArray<float> localSplatmapData = new(splatmapLength, Allocator.TempJob);  // terrain's new splat map w/ copied data + new data => based on this biome

            #region biome blending preparation
            if (_biomeType == BiomeType.Default)    // for the default biome set blending to "1" -- no blending
            {
                GenerateBlendMaskJobDefaultBiome generateBlendMaskJobDefaultBiome = new() { BlendData = blendData };
                splatmapHandle = generateBlendMaskJobDefaultBiome.ScheduleParallel(blendDataLength, 64, splatmapHandle);
            }
            else
            {
                for (int i = 0; i < _polygonBiomeMaskList.Count; i++)   // for each "BiomeMaskArea" of this "_biomeType" -- create/prepare the "blendMask" using their settings/curves
                {
                    GenerateBlendMaskJob generateBlendMaskJob = new()
                    {
                        BlendData = blendData,
                        PolygonArray = _polygonBiomeMaskList[i].PolygonArray,
                        SegmentArray = _polygonBiomeMaskList[i].SegmentArray,
                        CurveArray = _polygonBiomeMaskList[i].TextureCurveArray,

                        AlphamapResolution = Terrain.terrainData.alphamapResolution,
                        AlphamapScale = alphamapScale,
                        TerrainPosition = transform.position,

                        PolygonRect = RectExtension.CreateRectFromBounds(_polygonBiomeMaskList[i].MaskBounds),

                        UseNoise = _polygonBiomeMaskList[i].UseNoise,
                        NoiseScale = _polygonBiomeMaskList[i].NoiseScale,
                        BlendDistance = _polygonBiomeMaskList[i].BlendDistance
                    };
                    splatmapHandle = generateBlendMaskJob.ScheduleParallel(blendDataLength, 64, splatmapHandle);
                }
            }
            #endregion

            #region biome splat map generation
            for (int i = 0; i < _terrainTextureSettingsList.Count; i++)
            {
                if (i >= Terrain.terrainData.alphamapLayers)
                    continue;   // skip when the vegetation package has more texture than this terrain

                if (_terrainTextureSettingsList[i].Enabled) // generate new splat map data
                {
                    GenerateSplatmapJob generateSplatmapJob = new()
                    {
                        LocalSplatmapData = localSplatmapData,
                        RelativeHeightmapData = relativeHeightmapData,
                        Heights = heights,

                        TerrainPosition = transform.position,
                        AlphamapScale = alphamapScale,
                        WorldSpaceSeaLevel = _worldSpaceSeaLevel,

                        AlphamapResolution = Terrain.terrainData.alphamapResolution,
                        Layers = Terrain.terrainData.alphamapLayers,
                        HeightmapResolution = Terrain.terrainData.heightmapResolution,
                        HeightmapScale = Terrain.terrainData.heightmapScale,
                        AlphaHeightRatio = (float)Terrain.terrainData.alphamapResolution / Terrain.terrainData.heightmapResolution,

                        TextureIndex = i,
                        DensityFactor = _terrainTextureSettingsList[i].densityFactor,
                        HeightCurve = _terrainTextureSettingsList[i].HeightCurveArray,
                        SteepnessCurve = _terrainTextureSettingsList[i].SteepnessCurveArray,

                        TextureUseNoise = _terrainTextureSettingsList[i].UseNoise,
                        TextureNoiseScale = _terrainTextureSettingsList[i].NoiseScale,
                        NoiseBalancing = _terrainTextureSettingsList[i].noiseBalancing,
                        TextureNoiseOffset = _terrainTextureSettingsList[i].NoiseOffset,
                        InverseTextureNoise = _terrainTextureSettingsList[i].InverseNoise,

                        DistancePerSampleX = (int)math.round(_terrainTextureSettingsList[i].DistancePerSample / Terrain.terrainData.heightmapScale.x),
                        DistancePerSampleZ = (int)math.round(_terrainTextureSettingsList[i].DistancePerSample / Terrain.terrainData.heightmapScale.z),
                        ApplyCurves = _terrainTextureSettingsList[i].applyCurves,
                        ApplyNoise = _terrainTextureSettingsList[i].applyNoise && _terrainTextureSettingsList[i].applyCurves == false,

                        ConcaveEnable = _terrainTextureSettingsList[i].ConcaveEnable,
                        ConcaveDensityFactor = _terrainTextureSettingsList[i].concaveDensityFactor,
                        ConcaveMinHeightDifference = _terrainTextureSettingsList[i].ConcaveMinHeightDifference,
                        ConcaveHeightMin = _terrainTextureSettingsList[i].ConcaveMinHeight,
                        ConcaveHeightMax = _terrainTextureSettingsList[i].ConcaveMaxHeight,
                        ConcaveSteepnessMin = _terrainTextureSettingsList[i].concaveMinSteepness / 90,
                        ConcaveSteepnessMax = _terrainTextureSettingsList[i].concaveMaxSteepness / 90,

                        ConvexEnable = _terrainTextureSettingsList[i].ConvexEnable,
                        ConvexDensityFactor = _terrainTextureSettingsList[i].convexDensityFactor,
                        ConvexMinHeightDifference = _terrainTextureSettingsList[i].ConvexMinHeightDifference,
                        ConvexHeightMin = _terrainTextureSettingsList[i].ConvexMinHeight,
                        ConvexHeightMax = _terrainTextureSettingsList[i].ConvexMaxHeight,
                        ConvexSteepnessMin = _terrainTextureSettingsList[i].convexMinSteepness / 90,
                        ConvexSteepnessMax = _terrainTextureSettingsList[i].convexMaxSteepness / 90
                    };
                    splatmapHandle = generateSplatmapJob.ScheduleParallel(splatmapLength, 64, splatmapHandle);
                }
                else if (_clearExistingTextures == false && _terrainTextureSettingsList[i].LockTexture)   // else when "semi-enabled" copy over existing textures only
                {
                    CopyExistingDataJob copyExistingDataJobJob = new()
                    {
                        LocalSplatmapData = localSplatmapData,
                        CurrentSplatmapData = currentSplatmapData,
                        Layers = Terrain.terrainData.alphamapLayers,
                        TextureIndex = i
                    };
                    splatmapHandle = copyExistingDataJobJob.ScheduleParallel(splatmapLength, 64, splatmapHandle);
                }
                else if (_terrainTextureSettingsList[i].Enabled == false && _terrainTextureSettingsList[i].LockTexture == false)    // else when fully disabled clear existing redundant data
                {
                    ClearExistingDataJob clearExistingDataJob = new()
                    {
                        LocalSplatmapData = localSplatmapData,
                        Layers = Terrain.terrainData.alphamapLayers,
                        TextureIndex = i
                    };
                    splatmapHandle = clearExistingDataJob.ScheduleParallel(splatmapLength, 64, splatmapHandle);
                }
            }
            #endregion

            #region normalization + copy logic for existing textures
            int firstEnabledIndex = 0;
            for (int i = 0; i < _terrainTextureSettingsList.Count; i++)
                if (_terrainTextureSettingsList[i].Enabled)
                {
                    firstEnabledIndex = i;
                    break;
                }

            if (_clearExistingTextures == false)
            {
                NativeArray<int> newTextureArray = new(_terrainTextureSettingsList.Count, Allocator.TempJob);
                NativeArray<int> copiedTextureArray = new(_terrainTextureSettingsList.Count, Allocator.TempJob);

                for (int i = 0; i < _terrainTextureSettingsList.Count; i++)
                    if (_terrainTextureSettingsList[i].Enabled)
                        newTextureArray[i] = 1;
                    else if (_terrainTextureSettingsList[i].LockTexture)
                        copiedTextureArray[i] = 1;

                NormalizeSplatmapCopiedDataJob normalizeSplatmapCopiedDataJob = new()
                {
                    FirstEnabledIndex = firstEnabledIndex,
                    LocalSplatmapData = localSplatmapData,
                    NewTextureArray = newTextureArray,
                    CopiedTextureArray = copiedTextureArray
                };
                splatmapHandle = normalizeSplatmapCopiedDataJob.ScheduleBatch(splatmapLength, Terrain.terrainData.alphamapLayers, splatmapHandle);
            }
            else
            {
                NormalizeSplatmapJob normalizeSplatmapJob = new()
                {
                    FirstEnabledIndex = firstEnabledIndex,
                    LocalSplatmapData = localSplatmapData
                };
                splatmapHandle = normalizeSplatmapJob.ScheduleBatch(splatmapLength, Terrain.terrainData.alphamapLayers, splatmapHandle);
            }
            #endregion

            #region late stage merging/blending
            BlendSplatmapJob blendSplatmapJob = new()   // blend current existing splat map against the new "original" splat map
            {
                AlphamapResolution = Terrain.terrainData.alphamapResolution,
                Layers = Terrain.terrainData.alphamapLayers,
                CurrentSplatmapData = currentSplatmapData,
                BlendData = blendData,
                LocalSplatmapData = localSplatmapData
            };
            splatmapHandle = blendSplatmapJob.ScheduleParallel(splatmapLength, 64, splatmapHandle);
            #endregion
        }

        public void CompleteSplatmapGeneration()
        {
            splatmapHandle.Complete();  // finish / synchronize assigned job handles

            if (relativeHeightmapData.IsCreated)
                relativeHeightmapData.Dispose();    // dispose early to free some memory for copying later

            if (currentSplatmapData.Length > 0)
            {
                float[,,] splatmapArray = new float[Terrain.terrainData.alphamapResolution, Terrain.terrainData.alphamapResolution, Terrain.terrainData.alphamapLayers];
                currentSplatmapData.CopyToFast(splatmapArray);
                Terrain.terrainData.SetAlphamaps(0, 0, splatmapArray);  // actually apply the changes to the terrain's splat map
            }

            if (currentSplatmapData.IsCreated)
                currentSplatmapData.Dispose();  // dispose late since needed for the copy
        }

        public void AssignHeatmapMaterial()
        {
#if UNITY_EDITOR
            if (Terrain == false)
                return;

            if (Terrain.materialTemplate != null && Terrain.materialTemplate.shader.name != "AwesomeTechnologies/Development/Release_Internal/Terrain/VSP-B_TerrainHeatmap")
                actualTerrainMaterial = Terrain.materialTemplate;

            Terrain.drawInstanced = false;
            Terrain.materialTemplate = terrainHeatmapMaterial = new(ShaderUtility.GetShader_TerrainHeatmap()) { name = "VSP-B_TerrainHeatmap", enableInstancing = true };
#endif
        }

        public void RestoreTerrainMaterial()
        {
#if UNITY_EDITOR
            if (Terrain == false || actualTerrainMaterial == null)
                return;

            Terrain.materialTemplate = actualTerrainMaterial;
            Terrain.drawInstanced = true;
#endif
        }

        public void UpdateTerrainMaterial(float _worldspaceSeaLevel, float _worldspaceTerrainLevelMax, TerrainTextureSettings _terrainTextureSettings)
        {
#if UNITY_EDITOR
            if (terrainHeatmapMaterial == null)
                return;

            terrainHeatmapMaterial.SetColor("_HDColor", Color.white);
            terrainHeatmapMaterial.SetColor("_LDColor", new(0.0980392157f, 0.0980392157f, 0.0980392157f, 1));   // 25/255

            terrainHeatmapMaterial.SetFloat("_TerrainPositionY", transform.position.y);
            terrainHeatmapMaterial.SetFloat("_WorldSpaceSeaLevel", _worldspaceSeaLevel);
            terrainHeatmapMaterial.SetFloat("_WorldHeight", _worldspaceTerrainLevelMax - _worldspaceSeaLevel);

            terrainHeatmapMaterial.SetFloat("_DensityFactor", _terrainTextureSettings.densityFactor);
            terrainHeatmapMaterial.SetFloatArray("_HeightCurve", _terrainTextureSettings.TextureHeightCurve.GenerateCurveArray(1024));
            terrainHeatmapMaterial.SetFloatArray("_SteepnessCurve", _terrainTextureSettings.TextureSteepnessCurve.GenerateCurveArray(1024));

            terrainHeatmapMaterial.SetFloat("_UseNoise", _terrainTextureSettings.UseNoise ? 1 : 0);
            terrainHeatmapMaterial.SetFloat("_IsInverseInt", _terrainTextureSettings.InverseNoise ? -1 : 1);
            terrainHeatmapMaterial.SetFloat("_NoiseScale", _terrainTextureSettings.NoiseScale);
            terrainHeatmapMaterial.SetFloat("_NoiseBalancing", _terrainTextureSettings.noiseBalancing);
            terrainHeatmapMaterial.SetVector("_NoiseOffset", new float4(_terrainTextureSettings.NoiseOffset.x, _terrainTextureSettings.NoiseOffset.y, 0, 0));

            terrainHeatmapMaterial.SetTexture("_HeightmapTexture", Terrain.terrainData.heightmapTexture);
            terrainHeatmapMaterial.SetFloat("_TerrainSizeY_Scaled", Terrain.terrainData.bounds.max.y / (Terrain.terrainData.bounds.max.y / Terrain.terrainData.heightmapScale.y / 2)); // adjust for "Terrain.terrainData.heightmapTexture" output values
            terrainHeatmapMaterial.SetFloat("_TerrainSizeX", Terrain.terrainData.size.x);
            terrainHeatmapMaterial.SetFloat("_TerrainSizeZ", Terrain.terrainData.size.z);
            terrainHeatmapMaterial.SetFloat("_DistancePerSampleX", _terrainTextureSettings.DistancePerSample / Terrain.terrainData.size.x);
            terrainHeatmapMaterial.SetFloat("_DistancePerSampleZ", _terrainTextureSettings.DistancePerSample / Terrain.terrainData.size.z);
            terrainHeatmapMaterial.SetFloat("_ApplyCurves", _terrainTextureSettings.applyCurves ? 1 : 0);
            terrainHeatmapMaterial.SetFloat("_ApplyNoise", _terrainTextureSettings.applyNoise ? 1 : 0);

            terrainHeatmapMaterial.SetFloat("_ConcaveEnable", _terrainTextureSettings.ConcaveEnable ? 1 : 0);
            terrainHeatmapMaterial.SetFloat("_ConcaveDensityFactor", _terrainTextureSettings.concaveDensityFactor);
            terrainHeatmapMaterial.SetFloat("_ConcaveMinHeightDifference", _terrainTextureSettings.ConcaveMinHeightDifference);
            terrainHeatmapMaterial.SetFloat("_ConcaveHeightMin", _terrainTextureSettings.ConcaveMinHeight);
            terrainHeatmapMaterial.SetFloat("_ConcaveHeightMax", _terrainTextureSettings.ConcaveMaxHeight);
            terrainHeatmapMaterial.SetFloat("_ConcaveSteepnessMin", _terrainTextureSettings.concaveMinSteepness / 90);
            terrainHeatmapMaterial.SetFloat("_ConcaveSteepnessMax", _terrainTextureSettings.concaveMaxSteepness / 90);

            terrainHeatmapMaterial.SetFloat("_ConvexEnable", _terrainTextureSettings.ConvexEnable ? 1 : 0);
            terrainHeatmapMaterial.SetFloat("_ConvexDensityFactor", _terrainTextureSettings.convexDensityFactor);
            terrainHeatmapMaterial.SetFloat("_ConvexMinHeightDifference", _terrainTextureSettings.ConvexMinHeightDifference);
            terrainHeatmapMaterial.SetFloat("_ConvexHeightMin", _terrainTextureSettings.ConvexMinHeight);
            terrainHeatmapMaterial.SetFloat("_ConvexHeightMax", _terrainTextureSettings.ConvexMaxHeight);
            terrainHeatmapMaterial.SetFloat("_ConvexSteepnessMin", _terrainTextureSettings.convexMinSteepness / 90);
            terrainHeatmapMaterial.SetFloat("_ConvexSteepnessMax", _terrainTextureSettings.convexMaxSteepness / 90);
#endif
        }

        public Texture2D GetTerrainTexture(int _index)  // kept for possible existing custom code
        {
            if (Terrain == null || Terrain.terrainData == null || Terrain.terrainData.terrainLayers == null)
                return null;

            if (_index < Terrain.terrainData.terrainLayers.Length && Terrain.terrainData.terrainLayers[_index])
                return Terrain.terrainData.terrainLayers[_index].diffuseTexture;
            else
                return null;
        }

        public TerrainLayer[] GetTerrainLayers()
        {
            if (Terrain == null || Terrain.terrainData == null || Terrain.terrainData.terrainLayers == null)
                return new TerrainLayer[0];
            return Terrain.terrainData.terrainLayers;
        }

        public void SetTerrainLayers(TerrainLayer[] _terrainLayers)
        {
            if (Terrain && Terrain.terrainData)
                Terrain.terrainData.terrainLayers = _terrainLayers;
        }

        #region jobs
        [BurstCompile]
        public struct SampleHeightmapJob : IJobFor
        {
            [WriteOnly] public NativeArray<RelativeHeightmapData> RelativeHeightmapData;
            [ReadOnly] public NativeArray<float> Heights;

            [ReadOnly] public float TerrainPositionY;
            [ReadOnly] public float WorldSpaceSeaLevel;
            [ReadOnly] public float WorldHeight;

            [ReadOnly] public float3 HeightmapScale;
            [ReadOnly] public int HeightmapResolution;
            [ReadOnly] public int AlphamapResolution;

            public void Execute(int index)
            {
                int y = (int)math.floor(index / AlphamapResolution);
                int x = index - (y * AlphamapResolution);

                float interpolatedX = (float)x / (AlphamapResolution - 1);  // shift source by 1 for correct alignment to the terrain's surface
                float interpolatedY = (float)y / (AlphamapResolution - 1);  // shift source by 1 for correct alignment to the terrain's surface

                RelativeHeightmapData relativeHeightmapData = new()
                {
                    Height = ((TerrainPositionY + GetTriangleInterpolatedHeight(interpolatedX, interpolatedY)) - WorldSpaceSeaLevel) / WorldHeight,
                    Steepness = math.degrees(math.acos(math.dot(GetInterpolatedNormal(interpolatedX, interpolatedY), new float3(0, 1, 0)))) / 90
                };
                RelativeHeightmapData[index] = relativeHeightmapData;
            }

            float GetTriangleInterpolatedHeight(float _x, float _y)
            {
                float fx = _x * (HeightmapResolution - 1);
                float fy = _y * (HeightmapResolution - 1);
                int lx = (int)fx;
                int ly = (int)fy;

                float u = fx - lx;
                float v = fy - ly;
                if (u > v)
                {
                    float z00 = GetHeight(lx + 0, ly + 0);
                    float z01 = GetHeight(lx + 1, ly + 0);
                    float z11 = GetHeight(lx + 1, ly + 1);
                    return z00 + (z01 - z00) * u + (z11 - z01) * v;
                }
                else
                {
                    float z00 = GetHeight(lx + 0, ly + 0);
                    float z10 = GetHeight(lx + 0, ly + 1);
                    float z11 = GetHeight(lx + 1, ly + 1);
                    return z00 + (z11 - z10) * u + (z10 - z00) * v;
                }
            }

            float GetHeight(int _x, int _y)
            {
                _x = math.clamp(_x, 0, HeightmapResolution - 1);
                _y = math.clamp(_y, 0, HeightmapResolution - 1);
                return Heights[_x + (_y * HeightmapResolution)] * HeightmapScale.y;
            }

            public float3 GetInterpolatedNormal(float _x, float _y)
            {
                float fx = _x * (HeightmapResolution - 1);
                float fy = _y * (HeightmapResolution - 1);
                int lx = (int)fx;
                int ly = (int)fy;

                float3 n00 = CalculateNormalSobel(lx + 0, ly + 0);
                float3 n10 = CalculateNormalSobel(lx + 1, ly + 0);
                float3 n01 = CalculateNormalSobel(lx + 0, ly + 1);
                float3 n11 = CalculateNormalSobel(lx + 1, ly + 1);

                float u = fx - lx;
                float3 s = math.lerp(n00, n10, u);
                float3 t = math.lerp(n01, n11, u);
                float v = fy - ly;

                return math.normalize(math.lerp(s, t, v));
            }

            float3 CalculateNormalSobel(int _x, int _y)
            {
                float dX = GetHeight(_x - 1, _y - 1) * -1.0f;
                dX += GetHeight(_x - 1, _y) * -2.0f;
                dX += GetHeight(_x - 1, _y + 1) * -1.0f;
                dX += GetHeight(_x + 1, _y - 1) * 1.0f;
                dX += GetHeight(_x + 1, _y) * 2.0f;
                dX += GetHeight(_x + 1, _y + 1) * 1.0f;
                dX /= HeightmapScale.x;

                float dY = GetHeight(_x - 1, _y - 1) * -1.0f;
                dY += GetHeight(_x, _y - 1) * -2.0f;
                dY += GetHeight(_x + 1, _y - 1) * -1.0f;
                dY += GetHeight(_x - 1, _y + 1) * 1.0f;
                dY += GetHeight(_x, _y + 1) * 2.0f;
                dY += GetHeight(_x + 1, _y + 1) * 1.0f;
                dY /= HeightmapScale.z;

                return math.normalize(new float3(-dX, 8, -dY));
            }
        }

        [BurstCompile]
        public struct GenerateBlendMaskJobDefaultBiome : IJobFor
        {
            [WriteOnly] public NativeArray<float> BlendData;

            public void Execute(int index)
            {
                BlendData[index] = 1;
            }
        }

        [BurstCompile]
        public struct GenerateBlendMaskJob : IJobFor
        {
            public NativeArray<float> BlendData;
            [ReadOnly] public NativeArray<float2> PolygonArray;
            [ReadOnly] public NativeArray<LineSegment2D> SegmentArray;
            [ReadOnly] public NativeArray<float> CurveArray;

            [ReadOnly] public int AlphamapResolution;
            [ReadOnly] public float2 AlphamapScale;
            [ReadOnly] public float3 TerrainPosition;

            [ReadOnly] public Rect PolygonRect;

            [ReadOnly] public bool UseNoise;
            [ReadOnly] public float NoiseScale;
            [ReadOnly] public float BlendDistance;

            public void Execute(int index)
            {
                int y = index / AlphamapResolution;
                int x = index - (y * AlphamapResolution);

                float2 point = new(TerrainPosition.x + (AlphamapScale.x * x), TerrainPosition.z + (AlphamapScale.y * y));
                if (PolygonRect.Contains(point) == false || IsInPolygon(point) == false)
                    return; // skip if part of the "BiomeMaskArea" is not on this terrain

                float originalBlend = BlendData[index];
                float currentBlend = math.max(BlendData[index], 1);

                float distanceToEdge = DistanceToEdge(point);
                if (distanceToEdge < BlendDistance)
                {
                    float perlinBlend = math.select(1, noise.cnoise(new float2(TerrainPosition.x + ((AlphamapScale.x * y) / NoiseScale), TerrainPosition.z - ((AlphamapScale.y * -x) / NoiseScale))), UseNoise);
                    currentBlend = math.max(originalBlend, math.min(SampleCurveArray(distanceToEdge / BlendDistance) * perlinBlend, currentBlend));
                }

                BlendData[index] = currentBlend;
            }

            private float SampleCurveArray(float _value)
            {
                if (CurveArray.Length == 0)
                    return 0f;

                int index = math.clamp((int)math.round((_value) * CurveArray.Length), 0, CurveArray.Length - 1);

                if (index == CurveArray.Length - 1)
                    return CurveArray[index];
                return math.lerp(CurveArray[index], CurveArray[index + 1], math.frac(math.clamp(_value, 0, 1) * (CurveArray.Length - 1)));
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

        [BurstCompile]
        public struct GenerateSplatmapJob : IJobFor
        {
            [WriteOnly] public NativeArray<float> LocalSplatmapData;
            [ReadOnly] public NativeArray<RelativeHeightmapData> RelativeHeightmapData;
            [ReadOnly] public NativeArray<float> Heights;

            [ReadOnly] public float3 TerrainPosition;
            [ReadOnly] public float2 AlphamapScale;
            [ReadOnly] public float WorldSpaceSeaLevel;

            [ReadOnly] public int AlphamapResolution;
            [ReadOnly] public int Layers;
            [ReadOnly] public int HeightmapResolution;
            [ReadOnly] public float3 HeightmapScale;
            [ReadOnly] public float AlphaHeightRatio;

            [ReadOnly] public int TextureIndex;
            [ReadOnly] public float DensityFactor;
            [ReadOnly] public NativeArray<float> HeightCurve;
            [ReadOnly] public NativeArray<float> SteepnessCurve;

            [ReadOnly] public bool TextureUseNoise;
            [ReadOnly] public float TextureNoiseScale;
            [ReadOnly] public float NoiseBalancing;
            [ReadOnly] public float2 TextureNoiseOffset;
            [ReadOnly] public bool InverseTextureNoise;

            [ReadOnly] public int DistancePerSampleX;
            [ReadOnly] public int DistancePerSampleZ;
            [ReadOnly] public bool ApplyCurves;
            [ReadOnly] public bool ApplyNoise;

            [ReadOnly] public bool ConcaveEnable;
            [ReadOnly] public float ConcaveDensityFactor;
            [ReadOnly] public float ConcaveMinHeightDifference;
            [ReadOnly] public float ConcaveHeightMin;
            [ReadOnly] public float ConcaveHeightMax;
            [ReadOnly] public float ConcaveSteepnessMin;
            [ReadOnly] public float ConcaveSteepnessMax;

            [ReadOnly] public bool ConvexEnable;
            [ReadOnly] public float ConvexDensityFactor;
            [ReadOnly] public float ConvexMinHeightDifference;
            [ReadOnly] public float ConvexHeightMin;
            [ReadOnly] public float ConvexHeightMax;
            [ReadOnly] public float ConvexSteepnessMin;
            [ReadOnly] public float ConvexSteepnessMax;

            public void Execute(int index) //public void Execute(int startIndex, int count)
            {
                int zQuotient = Math.DivRem(index, Layers, out int z);

                if (z == TextureIndex)
                {
                    int yQuotient = Math.DivRem(zQuotient, AlphamapResolution, out int y);
                    int x = yQuotient % AlphamapResolution;

                    float heightAlpha = SampleCurveArray(HeightCurve, RelativeHeightmapData[y + (x * AlphamapResolution)].Height);
                    float steepnessAlpha = SampleCurveArray(SteepnessCurve, RelativeHeightmapData[y + (x * AlphamapResolution)].Steepness);
                    float perlinAlpha = math.select(math.select(1,  /*noise.cnoise*/ // use custom (shader) noise w/ a matching preview shader
                        Perlin2D(new((TerrainPosition.x + (AlphamapScale.x * y) + TextureNoiseOffset.x) / TextureNoiseScale, (TerrainPosition.z - (AlphamapScale.y * -x) + TextureNoiseOffset.y) / TextureNoiseScale)), TextureUseNoise),
                        -Perlin2D(new((TerrainPosition.x + (AlphamapScale.x * y) + TextureNoiseOffset.x) / TextureNoiseScale, (TerrainPosition.z - (AlphamapScale.y * -x) + TextureNoiseOffset.y) / TextureNoiseScale)), InverseTextureNoise && TextureUseNoise);
                    float curveAlpha = math.clamp(DensityFactor * heightAlpha * steepnessAlpha * perlinAlpha, 0, 10);   // clamp to "10" to match UI limit -- "0" since negative isn't allowed

                    LocalSplatmapData[index] = math.select(curveAlpha, math.max(curveAlpha, // assign "curveAlpha" or bigger value between "curveAlpha" and "cavityAlpha"
                        math.select(1, curveAlpha, ApplyCurves) * math.select(1, perlinAlpha, ApplyNoise) *
                        SampleCavityAlpha(new int2((int)math.round(y / AlphaHeightRatio), (int)math.round(x / AlphaHeightRatio)), RelativeHeightmapData[y + (x * AlphamapResolution)].Steepness)), ConcaveEnable || ConvexEnable);
                }
            }

            float Perlin2D(float2 P)    // https://github.com/BrianSharpe/Wombat/blob/master/Perlin2D.glsl
            {
                // establish our grid cell and unit position
                float2 Pi = math.floor(P);
                float4 Pf_Pfmin1 = P.xyxy - new float4(Pi, Pi + 1.0f);

                // calculate the hash
                float4 Pt = new(Pi.xy, Pi.xy + new float2(1, 1));
                Pt -= math.floor(Pt * (1.0f / 71.0f)) * 71.0f;
                Pt += new float2(26.0f, 161.0f).xyxy;
                Pt *= Pt;
                Pt = Pt.xzxz * Pt.yyww;
                float4 hash_x = math.frac(Pt * (1.0f / 951.135664f));
                float4 hash_y = math.frac(Pt * (1.0f / 642.949883f));

                // calculate the gradient results
                float4 grad_x = hash_x - 0.49999f;
                float4 grad_y = hash_y - 0.49999f;
                float4 grad_results = math.rsqrt(grad_x * grad_x + grad_y * grad_y) * (grad_x * Pf_Pfmin1.xzxz + grad_y * Pf_Pfmin1.yyww);

                // Classic Perlin Interpolation
                grad_results *= 1.4142135623730950488016887242097f;  // scale things to a strict -1.0->1.0 range  *= 1.0/sqrt(0.5)
                float2 blend = Pf_Pfmin1.xy * Pf_Pfmin1.xy * Pf_Pfmin1.xy * (Pf_Pfmin1.xy * (Pf_Pfmin1.xy * 6.0f - 15.0f) + 10.0f);
                float4 blend2 = new(blend, new float2(1.0f - blend));

                float alpha = math.dot(grad_results, blend2.zxzx * blend2.wwyy);
                return math.select(alpha - NoiseBalancing, alpha + -NoiseBalancing, alpha >= 0);    // apply balancing -- clamped later to cut-off negative values
            }

            private float SampleCurveArray(NativeArray<float> _curve, float _value)
            {
                if (_curve.Length == 0)
                    return 0f;

                int index = math.clamp((int)math.round((_value) * _curve.Length), 0, _curve.Length - 1);

                if (index == _curve.Length - 1)
                    return _curve[index];
                return math.lerp(_curve[index], _curve[index + 1], math.frac(math.clamp(_value, 0, 1) * (_curve.Length - 1)));
            }

            float SampleCavityAlpha(int2 _xz, float _steepness) // get center point > compare against average square around it > higher/lower diff = cavity value
            {   // calculate the average height of surrounding pixels -- w/ max offset distance for the "average square" => total "heightmapData" "coverage" per sample point
                float heightLU = GetHeight(_xz.x - DistancePerSampleX, _xz.y + DistancePerSampleZ); // up
                float heightCU = GetHeight(_xz.x, _xz.y + DistancePerSampleZ);
                float heightRU = GetHeight(_xz.x + DistancePerSampleX, _xz.y + DistancePerSampleZ);
                float heightLC = GetHeight(_xz.x - DistancePerSampleX, _xz.y);  // center
                float heightCC = GetHeight(_xz.x, _xz.y);   // center (sample) point => gets compared against surrounding "average square" using eight sample points around the center point
                float heightRC = GetHeight(_xz.x + DistancePerSampleX, _xz.y);
                float heightLD = GetHeight(_xz.x - DistancePerSampleX, _xz.y - DistancePerSampleZ); // down
                float heightCD = GetHeight(_xz.x, _xz.y - DistancePerSampleZ);
                float heightRD = GetHeight(_xz.x + DistancePerSampleX, _xz.y - DistancePerSampleZ);
                float heightAverage = (heightLU + heightCU + heightRU + heightLC + heightRC + heightLD + heightCD + heightRD) / 8;  // "average square"

                float concave = math.select(0, (heightAverage - heightCC) / ConcaveMinHeightDifference * ConcaveDensityFactor,  // get concaveAlpha -- limit by height/steepness
                    ConcaveEnable && heightCC >= ConcaveHeightMin && heightCC <= ConcaveHeightMax && _steepness >= ConcaveSteepnessMin && _steepness <= ConcaveSteepnessMax);

                float convex = math.select(0, (heightCC - heightAverage) / ConvexMinHeightDifference * ConvexDensityFactor, // get convexAlpha -- limit by height/steepness
                    ConvexEnable && heightCC >= ConvexHeightMin && heightCC <= ConvexHeightMax && _steepness >= ConvexSteepnessMin && _steepness <= ConvexSteepnessMax);

                return math.clamp(math.max(concave, convex), 0, 10);    // clamp to "10" to match UI limit -- "0" since negative isn't allowed
            }

            float GetHeight(int _x, int _y)
            {
                _x = math.clamp(_x, 0, HeightmapResolution - 1);
                _y = math.clamp(_y, 0, HeightmapResolution - 1);
                return Heights[_x + (_y * HeightmapResolution)] * HeightmapScale.y;
            }
        }

        [BurstCompile]
        public struct CopyExistingDataJob : IJobFor
        {
            [WriteOnly] public NativeArray<float> LocalSplatmapData;
            [ReadOnly] public NativeArray<float> CurrentSplatmapData;

            [ReadOnly] public int Layers;
            [ReadOnly] public int TextureIndex;

            public void Execute(int index)
            {
                Math.DivRem(index, Layers, out int z);
                if (z == TextureIndex)
                    LocalSplatmapData[index] = CurrentSplatmapData[index];  // get existing data of the texture from the existing splat map data of this terrain
            }
        }

        [BurstCompile]
        public struct ClearExistingDataJob : IJobFor
        {
            [WriteOnly] public NativeArray<float> LocalSplatmapData;

            [ReadOnly] public int Layers;
            [ReadOnly] public int TextureIndex;

            public void Execute(int index)
            {
                Math.DivRem(index, Layers, out int z);
                if (z == TextureIndex)
                    LocalSplatmapData[index] = 0;   // clear existing data for fully disabled textures
            }
        }

        [BurstCompile]
        public struct NormalizeSplatmapCopiedDataJob : IJobParallelForBatch
        {
            [ReadOnly] public int FirstEnabledIndex;
            public NativeArray<float> LocalSplatmapData;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> NewTextureArray;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> CopiedTextureArray;

            public void Execute(int startIndex, int count)  // for each pixel -- texture layer
            {
                float totalCopyValue = 0;
                for (int i = 0; i < count; i++)
                {
                    if (i >= CopiedTextureArray.Length)
                        break;

                    if (CopiedTextureArray[i] == 1)
                        totalCopyValue += LocalSplatmapData[startIndex + i];    // sum all copied layers -- create "merged blend"
                }

                float totalValue = 0;
                for (int i = 0; i < count; i++)
                {
                    if (i >= NewTextureArray.Length)
                        break;

                    if (NewTextureArray[i] == 1)
                        totalValue += LocalSplatmapData[startIndex + i];    // sum all layers -- create "merged blend"
                }

                totalValue /= (1 - totalCopyValue);
                if ((totalValue + totalCopyValue) > 0)  // apply/blend into averaged alpha
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (i >= NewTextureArray.Length)
                            break;

                        if (NewTextureArray[i] == 1)
                            LocalSplatmapData[startIndex + i] /= totalValue;    // divide into "merged blend" to normalize and create the effective "blended" value
                    }
                }
                else
                {   // else fall back to first enabled texture
                    for (int i = 0; i < count; i++)
                        LocalSplatmapData[startIndex + i] = 0;
                    LocalSplatmapData[startIndex + FirstEnabledIndex] = 1;
                }
            }
        }

        [BurstCompile]
        public struct NormalizeSplatmapJob : IJobParallelForBatch
        {
            [ReadOnly] public int FirstEnabledIndex;
            public NativeArray<float> LocalSplatmapData;

            public void Execute(int startIndex, int count)  // for each pixel -- texture layer
            {
                float totalValue = 0;
                for (int i = 0; i < count; i++)
                    totalValue += LocalSplatmapData[startIndex + i];    // sum all layers -- create "merged blend"

                if (totalValue > 0) // apply/blend into averaged alpha
                {
                    for (int i = 0; i < count; i++)
                        LocalSplatmapData[startIndex + i] /= totalValue;    // divide into "merged blend" to normalize and create the effective "blended" value
                }
                else
                {   // else fall back to first enabled texture
                    for (int i = 0; i < count; i++)
                        LocalSplatmapData[startIndex + i] = 0;
                    LocalSplatmapData[startIndex + FirstEnabledIndex] = 1;
                }
            }
        }

        [BurstCompile]
        public struct BlendSplatmapJob : IJobFor
        {
            [ReadOnly] public int AlphamapResolution;
            [ReadOnly] public int Layers;

            public NativeArray<float> CurrentSplatmapData;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float> BlendData;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float> LocalSplatmapData;

            public void Execute(int index)
            {
                int yQuotient = Math.DivRem(index / Layers, AlphamapResolution, out int y);
                int x = yQuotient % AlphamapResolution;
                float pixelBlend = BlendData[y + (x * AlphamapResolution)];
                CurrentSplatmapData[index] = (CurrentSplatmapData[index] * (1 - pixelBlend)) + (LocalSplatmapData[index] * pixelBlend);
            }
        }
        #endregion
    }
}