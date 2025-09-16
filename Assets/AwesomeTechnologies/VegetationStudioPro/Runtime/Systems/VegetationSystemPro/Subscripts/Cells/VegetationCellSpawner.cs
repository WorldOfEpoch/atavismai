using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationCellSpawner
    {
        public VegetationSystemPro vegetationSystemPro;
        public NativeList<JobHandle> jobHandleList; // stores combined spawning rule handles => (lower)
        public NativeList<JobHandle> cellJobHandleList; // stores cell spawning related handles => (higher)
        public VegetationInstanceDataPool vegetationInstanceDataPool;
        public NativeArray<float> randomNumbers;

        public VegetationCellSpawner(VegetationSystemPro _vegetationSystemPro)
        {
            vegetationSystemPro = _vegetationSystemPro;
            jobHandleList = new NativeList<JobHandle>(64, Allocator.Persistent);
            cellJobHandleList = new NativeList<JobHandle>(64, Allocator.Persistent);
            vegetationInstanceDataPool = new VegetationInstanceDataPool();
            GeneratePersistentRandomNumberArray();
        }

        void GeneratePersistentRandomNumberArray()
        {
            UnityEngine.Random.InitState(0);
            randomNumbers = new NativeArray<float>(10000, Allocator.Persistent);
            for (int i = 0; i < randomNumbers.Length; i++)
                randomNumbers[i] = UnityEngine.Random.Range(0f, 1f);
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);  // reset random state to not mess with third party logic
        }

        #region vegetation cell spawn functions
        public JobHandle SpawnVegetationCell(VegetationCell _vegetationCell, string _vegetationItemID, bool _skipPersistentVegetation = false)
        {   // utility spawn cell function internal -- use external version
            // get indices of this specific vegetation item -- return its calculated data(rules)
            VegetationItemIndices vegetationItemIndices = vegetationSystemPro.GetVegetationItemIndices(_vegetationItemID);
            if (vegetationItemIndices.vegetationPackageIndex == -1 || vegetationItemIndices.vegetationItemIndex == -1)
                return default(JobHandle);
            return ScheduleVegetationItemRules(_vegetationCell, _vegetationCell.Rectangle, vegetationItemIndices.vegetationPackageIndex, vegetationItemIndices.vegetationItemIndex, _skipPersistentVegetation);
        }

        public JobHandle SpawnVegetationCell(VegetationCell _vegetationCell, int _currentDistanceBand, out bool _hasInstancedIndirect, bool _billboardCell, bool _predictive)
        {   // main spawn cell function
            _hasInstancedIndirect = false;
            if (_billboardCell == false)
                _vegetationCell.loadedDistanceBand = _currentDistanceBand;  // set the distance band type to determine whether to upgrade/skip the next time -- render(or not) vegetation items based on their distance band type
            jobHandleList.Clear();

            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)    // for all vegetation packages
                for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)  // for all vegetation items
                {
                    VegetationItemInfoPro vegItemInfoPro = vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j];
                    if (_billboardCell && vegItemInfoPro.UseBillboards == false)
                        continue;   // skip generating data(rules) in billboard only mode when the vegetation item has billboards turned off(is not supported)

                    if (_currentDistanceBand > vegItemInfoPro.GetDistanceBand())
                        continue;   // skip generating data(rules) if the cell is not meant for the vegetation item (yet)

                    if (_predictive && vegetationSystemPro.predictiveCellLoader.ValidatePredictiveVegetationType(vegItemInfoPro.VegetationType) == false)
                        continue;   // skip if the vegetation type shouldn't be "predictive preloaded"

                    jobHandleList.Add(ScheduleVegetationItemRules(_vegetationCell, _vegetationCell.Rectangle, i, j));   // calculate data(rules) of all valid items of all packages

                    if (_billboardCell == false && vegItemInfoPro.VegetationRenderMode == VegetationRenderMode.InstancedIndirect)
                        _hasInstancedIndirect = true;   // mark cell as containing instanced indirect items so it gets compute shader data prepared later
                }

            if (jobHandleList.Length <= 0)
                return default(JobHandle);

            vegetationSystemPro.compactMemoryCellList.Add(_vegetationCell); // add cell to temp list so it's temporary rule data gets cleared at the end of the frame/loop
            return JobHandle.CombineDependencies(jobHandleList.AsArray());  // return calculated data(rules) of all items of all packages
        }
        #endregion

        JobHandle ScheduleVegetationItemRules(VegetationCell _vegetationCell, Rect _vegetationCellRect, int _vegetationPackageIndex, int _vegetationItemIndex, bool _skipPersistentVegetation = false)
        {
            if (vegetationSystemPro.vegetationStudioTerrainList.Count <= 0)
                return default(JobHandle);  // return if no terrains are in the list -- safety async check

            if (_vegetationCell.vegetationPackageInstanceList.Count <= _vegetationPackageIndex)
                return default(JobHandle);  // return if vegetationPackageInstanceList is async

            if (_vegetationCell.vegetationPackageInstanceList[_vegetationPackageIndex].loadStateList[_vegetationItemIndex] == 1)
                return default(JobHandle);  // return if the vegetation item already got loaded
            _vegetationCell.vegetationPackageInstanceList[_vegetationPackageIndex].loadStateList[_vegetationItemIndex] = 1; // set the vegetation item as loaded

            // get relevant data related to the vegetation item
            VegetationPackagePro vegetationPackagePro = vegetationSystemPro.vegetationPackageProList[_vegetationPackageIndex];
            VegetationItemInfoPro vegetationItemInfoPro = vegetationSystemPro.vegetationPackageProList[_vegetationPackageIndex].VegetationInfoList[_vegetationItemIndex];
            VegetationItemModelInfo vegetationItemModelInfo = vegetationSystemPro.vegetationPackageProModelsList[_vegetationPackageIndex].vegetationItemModelList[_vegetationItemIndex];
            NativeList<MatrixInstance> matrixList = _vegetationCell.vegetationPackageInstanceList[_vegetationPackageIndex].matrixInstanceList[_vegetationItemIndex];
            matrixList.Clear(); // reset the matrix list as it gets filled later with data based on the (newly updated)rules

            JobHandle vegetationItemHandle = default(JobHandle);    // handle for all the jobs/rules
            int ilbatchCount = 64;  // ~16 to ~128

            #region general run-time state filtering
            // always spawn items in the default biome -- if the vegetation cell is not covered by the specified biome then disable spawning
            BiomeType currentBiome = vegetationSystemPro.vegetationPackageProList[_vegetationPackageIndex].BiomeType;
            bool doRuntimeSpawn = !(currentBiome != BiomeType.Default && !_vegetationCell.HasBiome(currentBiome));

            float globalDensity = vegetationSystemPro.vegetationSettings.GetVegetationItemDensity(vegetationItemInfoPro.VegetationType);
            if (globalDensity < 0.01f)  // safety divide by zero filter
                doRuntimeSpawn = false;

            if (vegetationItemInfoPro.EnableRuntimeSpawn == false)  // default run-time toggle
                doRuntimeSpawn = false;

            if (vegetationItemInfoPro.UseVegetationMask)    // initial vegetation mask include mode filter
            {
                bool hasVegetationTypeIndex = false;
                if (_vegetationCell.vegetationMaskList != null)
                    for (int i = 0; i < _vegetationCell.vegetationMaskList.Count; i++)
                        if (_vegetationCell.vegetationMaskList[i].HasVegetationTypeIndex(vegetationItemInfoPro.VegetationTypeIndex))
                        {
                            hasVegetationTypeIndex = true;  // at least one mask registered for this cell contains an ID for this vegetation item
                            break;  // skip rest as only one needs to be found
                        }

                if (hasVegetationTypeIndex == false)
                    doRuntimeSpawn = false; // else since no matching ID got found disable runtime spawning
            }
            #endregion

            if (doRuntimeSpawn)
            {
                #region Initialization of the pool, sampleCount, "included", etc
                // amount of nodes for the vegetation item in this cell -- actual sample distance for each vegetation item based on the global density
                float finalSampleDistance = math.clamp(vegetationItemInfoPro.SampleDistance / globalDensity, 0.1f, _vegetationCell.cellBounds.size.x * 0.5f);   // /2 since extents vs size
                int xSamples = (int)math.ceil(_vegetationCell.cellBounds.size.x / finalSampleDistance);
                int zSamples = (int)math.ceil(_vegetationCell.cellBounds.size.z / finalSampleDistance);
                int sampleCount = xSamples * zSamples;  // max possible samples/nodes in this vegetation cell for this vegetation item

                matrixList.Capacity = sampleCount;  // setup general max size

                VegetationInstanceData instanceData = vegetationInstanceDataPool.GetObject();   // get a new object from the pool
                _vegetationCell.vegetationInstanceDataList.Add(instanceData);   // assign the object to the vegetation cell -- used as a reference to dispose/clear data later, per cell

                instanceData.ResizeUninitialized(sampleCount);  // resize lists to match max possible node count -- uninitialized to save perf => get written to later
                InitializeInstanceData initializeInstanceData = new() { Included = instanceData.included };
                vegetationItemHandle = initializeInstanceData.ScheduleParallel(sampleCount, ilbatchCount, vegetationItemHandle);    // init "include" list since we use that to loop through jobs => all other lists have the same length

                float defaultSpawnChance = 0;   // never directly spawn for specific biome vegetation packages as they calculate/increase their own spawn chance only inside of them
                if (currentBiome == BiomeType.Default)
                    defaultSpawnChance = 1; // always spawn for default biome vegetation packages
                #endregion

                #region initial cell node creation and filtering + density/cutoff rules
                GenerateCellNodesJob generateCellNodesJob = new()
                {
                    Position = instanceData.position,
                    ControlData = instanceData.controlData,
                    RandomNumberIndex = instanceData.randomNumberIndex,

                    CellCorner = _vegetationCell.cellBounds.min,
                    RandomizePosition = vegetationItemInfoPro.RandomizePosition,
                    DefaultSpawnChance = defaultSpawnChance,
                    RandomNumbers = randomNumbers,
                    CellRect = _vegetationCellRect,
                    CellIndex = _vegetationCell.index,
                    Seed = vegetationItemInfoPro.Seed + vegetationSystemPro.vegetationSettings.seed,
                    UseSamplePointOffset = vegetationItemInfoPro.UseSamplePointOffset,
                    SamplePointMinOffset = vegetationItemInfoPro.SamplePointMinOffset,
                    SamplePointMaxOffset = vegetationItemInfoPro.SamplePointMaxOffset,
                    CalculatedSampleDistance = finalSampleDistance,
                    XSamples = xSamples,
                    ZSamples = zSamples
                };
                vegetationItemHandle = generateCellNodesJob.ScheduleParallel(sampleCount, ilbatchCount, vegetationItemHandle);

                if (_vegetationCell.biomeMaskList != null)  // biome mask area
                    for (int k = 0; k < _vegetationCell.biomeMaskList.Count; k++)
                    {
                        if (_vegetationCell.biomeMaskList[k].BiomeSortOrder < vegetationSystemPro.vegetationPackageProList[_vegetationPackageIndex].BiomeSortOrder)
                            continue;

                        vegetationItemHandle = _vegetationCell.biomeMaskList[k].BiomeMaskIncludeJob(instanceData, currentBiome, sampleCount, vegetationItemHandle);
                    }

                if (vegetationItemInfoPro.UseNoiseCutoff)   // noise cutoff
                {
                    PerlinNoiseCutoffJob perlinNoiseCutoffJob = new()
                    {
                        Position = instanceData.position,
                        ControlData = instanceData.controlData,
                        InversePerlinMask = vegetationItemInfoPro.NoiseCutoffInverse,
                        PerlinCutoff = vegetationItemInfoPro.NoiseCutoffValue,
                        PerlinScale = vegetationItemInfoPro.NoiseCutoffScale,
                        Offset = vegetationItemInfoPro.NoiseCutoffOffset,
                    };
                    vegetationItemHandle = perlinNoiseCutoffJob.ScheduleParallel(sampleCount, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseNoiseDensity)  // noise density
                {
                    PerlinNoiseDensityJob perlinNoiseDensityJob = new()
                    {
                        Position = instanceData.position,
                        ControlData = instanceData.controlData,
                        InversePerlinMask = vegetationItemInfoPro.NoiseDensityInverse,
                        PerlinScale = vegetationItemInfoPro.NoiseDensityScale,
                        PerlinBalancing = vegetationItemInfoPro.NoiseDensityBalancing,
                        Offset = vegetationItemInfoPro.NoiseDensityOffset
                    };
                    vegetationItemHandle = perlinNoiseDensityJob.ScheduleParallel(sampleCount, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTextureMaskDensityRules)   // texture mask density
                    for (int k = 0; k < vegetationItemInfoPro.TextureMaskDensityRuleList.Count; k++)
                    {
                        TextureMaskGroup textureMaskGroup = vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro.TextureMaskDensityRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                            vegetationItemHandle = textureMaskGroup.SampleDensityMask(instanceData, _vegetationCellRect, vegetationItemInfoPro.TextureMaskDensityRuleList[k], vegetationItemHandle);
                    }

                if (vegetationItemInfoPro.UseTerrainTextureDensityRules)  // terrain texture density
                    for (int k = 0; k < vegetationSystemPro.vegetationStudioTerrainList.Count; k++)
                        vegetationItemHandle = vegetationSystemPro.vegetationStudioTerrainList[k].ProcessSplatmapDensityRule(vegetationItemInfoPro.TerrainTextureDensityRuleList, instanceData, _vegetationCellRect, vegetationItemHandle);

                SpawnChanceFilterJob spawnChanceFilterJob = new()   // late stage randomized filter based on local density
                {
                    ControlData = instanceData.controlData,
                    RandomNumberIndex = instanceData.randomNumberIndex,

                    RandomNumbers = randomNumbers,
                    Density = vegetationItemInfoPro.Density
                };
                vegetationItemHandle = spawnChanceFilterJob.ScheduleParallel(sampleCount, ilbatchCount, vegetationItemHandle);

                for (int k = 0; k < vegetationSystemPro.vegetationStudioTerrainList.Count; k++) // terrains
                    vegetationItemHandle = vegetationSystemPro.vegetationStudioTerrainList[k].SampleTerrain(instanceData, sampleCount, _vegetationCellRect, vegetationItemHandle);
                #endregion

                #region include/exclude + height/steepness/concave rule
                if (vegetationItemInfoPro.UseTerrainSourceExcludeRule)  // terrain exclude
                {
                    TerrainSourceExcludeJob terrainSourceExcludeJob = new()
                    {
                        Included = instanceData.included,
                        TerrainSourceID = instanceData.terrainSourceID,

                        TerrainSourceRule = vegetationItemInfoPro.TerrainSourceExcludeRule
                    };
                    vegetationItemHandle = terrainSourceExcludeJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTerrainSourceIncludeRule)  // terrain include
                {
                    TerrainSourceIncludeJob terrainSourceIncludeJob = new()
                    {
                        Included = instanceData.included,
                        TerrainSourceID = instanceData.terrainSourceID,

                        TerrainSourceRule = vegetationItemInfoPro.TerrainSourceIncludeRule
                    };
                    vegetationItemHandle = terrainSourceIncludeJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseHeightRule)    // height
                {
                    HeightFilterJob heightFilterJob = new()
                    {
                        Included = instanceData.included,
                        Position = instanceData.position,
                        RandomNumberIndex = instanceData.randomNumberIndex,

                        MinHeight = vegetationItemInfoPro.MinHeight + vegetationSystemPro.systemRelativeSeaLevel,
                        MaxHeight = vegetationItemInfoPro.MaxHeight + vegetationSystemPro.systemRelativeSeaLevel,
                        Advanced = vegetationItemInfoPro.UseAdvancedHeightRule,
                        HeightRuleCurveArray = vegetationItemModelInfo.heightRuleCurveArray,
                        RandomNumbers = randomNumbers,
                        MaxCurveHeight = vegetationItemInfoPro.MaxCurveHeight
                    };
                    vegetationItemHandle = heightFilterJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseConcaveLocationRule)   // concave (include and exclude via invert)
                    for (int k = 0; k < vegetationSystemPro.vegetationStudioTerrainList.Count; k++)
                        vegetationItemHandle = vegetationSystemPro.vegetationStudioTerrainList[k].SampleConcaveLocation(instanceData, vegetationItemInfoPro.ConcaveLocationMinHeightDifference, vegetationItemInfoPro.ConcaveLocationDistance, vegetationItemInfoPro.ConcaveLocationInverse, _vegetationCellRect, vegetationItemHandle);

                if (vegetationItemInfoPro.UseSteepnessRule) // steepness
                {
                    SteepnessFilterJob steepnessFilterJob = new()
                    {
                        TerrainNormal = instanceData.terrainNormal,
                        RandomNumberIndex = instanceData.randomNumberIndex,
                        Included = instanceData.included,

                        RandomNumbers = randomNumbers,

                        SteepnessRuleCurveArray = vegetationItemModelInfo.steepnessRuleCurveArray,
                        Advanced = vegetationItemInfoPro.UseAdvancedSteepnessRule,
                        MinSteepness = vegetationItemInfoPro.MinSteepness,
                        MaxSteepness = vegetationItemInfoPro.MaxSteepness
                    };
                    vegetationItemHandle = steepnessFilterJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTextureMaskIncludeRules)   // texture mask include
                {
                    for (int k = 0; k < vegetationItemInfoPro.TextureMaskIncludeRuleList.Count; k++)
                    {
                        TextureMaskGroup textureMaskGroup = vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro.TextureMaskIncludeRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                            vegetationItemHandle = textureMaskGroup.SampleIncludeMask(instanceData, _vegetationCellRect, vegetationItemInfoPro.TextureMaskIncludeRuleList[k], vegetationItemHandle);
                    }

                    IncludeEvaluationJob includeEvaluationJob = new() { Included = instanceData.included, ControlData = instanceData.controlData };
                    vegetationItemHandle = includeEvaluationJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTextureMaskExcludeRules)   // texture mask exclude
                    for (int k = 0; k < vegetationItemInfoPro.TextureMaskExcludeRuleList.Count; k++)
                    {
                        TextureMaskGroup textureMaskGroup = vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro.TextureMaskExcludeRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                            vegetationItemHandle = textureMaskGroup.SampleExcludeMask(instanceData, _vegetationCellRect, vegetationItemInfoPro.TextureMaskExcludeRuleList[k], vegetationItemHandle);
                    }

                if (vegetationItemInfoPro.UseTerrainTextureIncludeRules)    // terrain texture include
                    for (int k = 0; k < vegetationSystemPro.vegetationStudioTerrainList.Count; k++)
                        vegetationItemHandle = vegetationSystemPro.vegetationStudioTerrainList[k].ProcessSplatmapIncludeRule(vegetationItemInfoPro.TerrainTextureIncludeRuleList, instanceData, _vegetationCellRect, vegetationItemHandle);

                if (vegetationItemInfoPro.UseTerrainTextureExcludeRules)    // terrain texture exclude
                    for (int k = 0; k < vegetationSystemPro.vegetationStudioTerrainList.Count; k++)
                        vegetationItemHandle = vegetationSystemPro.vegetationStudioTerrainList[k].ProcessSplatmapExcludeRule(vegetationItemInfoPro.TerrainTextureExcludeRuleList, instanceData, _vegetationCellRect, vegetationItemHandle);

                if (vegetationItemInfoPro.UseBiomeEdgeIncludeRule && currentBiome != BiomeType.Default) // biome edge include
                {
                    BiomeEdgeDistanceIncludeJob biomeEdgeDistanceIncludeJob = new()
                    {
                        ControlData = instanceData.controlData,
                        Included = instanceData.included,

                        MaxDistance = vegetationItemInfoPro.BiomeEdgeIncludeDistance,
                        Inverse = vegetationItemInfoPro.BiomeEdgeIncludeInverse
                    };
                    vegetationItemHandle = biomeEdgeDistanceIncludeJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }
                #endregion

                #region pos, rot, scale + end stage rules w/ scale (vegetation mask include, etc)
                if (vegetationItemInfoPro.UseBiomeEdgeScaleRule && currentBiome != BiomeType.Default)
                {
                    BiomeEdgeDistanceScaleJob biomeEdgeDistanceScaleJob = new() // biome edge scale
                    {
                        Scale = instanceData.scale,
                        ControlData = instanceData.controlData,
                        Included = instanceData.included,

                        MinScale = vegetationItemInfoPro.BiomeEdgeScaleMinScale,
                        MaxScale = vegetationItemInfoPro.BiomeEdgeScaleMaxScale,
                        MaxDistance = vegetationItemInfoPro.BiomeEdgeScaleDistance,
                        InverseScale = vegetationItemInfoPro.BiomeEdgeScaleInverse
                    };
                    vegetationItemHandle = biomeEdgeDistanceScaleJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                PosRotScaleJob posRotScaleJob = new()   // base randomized pos(offset only), rot(automated rotation w/ mode + offset), scale(just min/max)
                {
                    Position = instanceData.position,
                    Rotation = instanceData.rotation,
                    Scale = instanceData.scale,
                    TerrainNormal = instanceData.terrainNormal,
                    RandomNumberIndex = instanceData.randomNumberIndex,
                    ControlData = instanceData.controlData,
                    Included = instanceData.included,

                    RandomNumbers = randomNumbers,
                    VegetationRotationType = vegetationItemInfoPro.RotationMode,

                    ScaleCurveArray = vegetationItemModelInfo.scaleRuleCurveArray,
                    Advanced = vegetationItemInfoPro.useAdvancedScaleRule,

                    MinScale = vegetationItemInfoPro.MinScale,
                    MaxScale = vegetationItemInfoPro.MaxScale,
                    ScaleMultiplier = vegetationItemInfoPro.ScaleMultiplier,
                    RotationOffset = vegetationItemInfoPro.RotationOffset,
                    PositionOffset = vegetationItemInfoPro.Offset,
                    MinUpOffset = vegetationItemInfoPro.MinUpOffset,
                    MaxUpOffset = vegetationItemInfoPro.MaxUpOffset
                };
                vegetationItemHandle = posRotScaleJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);

                if (_vegetationCell.vegetationMaskList != null && vegetationItemInfoPro.UseVegetationMask == true)  // vegetation mask include mode -- scale and density -- after biomed edge > pos/rot/scale since "ControlData"
                {
                    for (int k = 0; k < _vegetationCell.vegetationMaskList.Count; k++)  // for each include vegetation mask -- get (compare) scale/density, per (overlapped) mask (w/ highest value)
                        vegetationItemHandle = _vegetationCell.vegetationMaskList[k].SampleIncludeVegetationMask(instanceData, vegetationItemInfoPro.VegetationTypeIndex, vegetationItemHandle);

                    if (_vegetationCell.vegetationMaskList.Count > 0)   // late stage apply -- actually apply scale/density values to each overlapped node (after each (overlapped) mask got filtered for the highest value)
                    {
                        VegetationMaskIncludeJob vegetationMaskIncludeJob = new()
                        {
                            Scale = instanceData.scale,
                            RandomNumberIndex = instanceData.randomNumberIndex,
                            ControlData = instanceData.controlData,
                            Included = instanceData.included,

                            RandomNumbers = randomNumbers
                        };
                        vegetationItemHandle = vegetationMaskIncludeJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                    }
                }

                if (vegetationItemInfoPro.UseNoiseScaleRule)    // noise scale
                {
                    PerlinNoiseScaleJob perlinNoiseScaleJob = new()
                    {
                        Included = instanceData.included,
                        Position = instanceData.position,
                        Scale = instanceData.scale,

                        PerlinScale = vegetationItemInfoPro.NoiseScaleScale,
                        MinScale = vegetationItemInfoPro.NoiseScaleMinScale,
                        MaxScale = vegetationItemInfoPro.NoiseScaleMaxScale,
                        PerlinBalancing = vegetationItemInfoPro.NoiseScaleBalancing,
                        InversePerlinMask = vegetationItemInfoPro.NoiseScaleInverse,
                        Offset = vegetationItemInfoPro.NoiseScaleOffset
                    };
                    vegetationItemHandle = perlinNoiseScaleJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                if (vegetationItemInfoPro.UseTextureMaskScaleRules) // texture mask scale
                    for (int k = 0; k < vegetationItemInfoPro.TextureMaskScaleRuleList.Count; k++)
                    {
                        TextureMaskGroup textureMaskGroup = vegetationPackagePro.GetTextureMaskGroup(vegetationItemInfoPro.TextureMaskScaleRuleList[k].TextureMaskGroupID);
                        if (textureMaskGroup != null)
                            vegetationItemHandle = textureMaskGroup.SampleScaleMask(instanceData, _vegetationCellRect, vegetationItemInfoPro.TextureMaskScaleRuleList[k], vegetationItemHandle);
                    }

                if (vegetationItemInfoPro.UseTerrainTextureScaleRules)  // terrain texture scale
                    for (int k = 0; k < vegetationSystemPro.vegetationStudioTerrainList.Count; k++)
                        vegetationItemHandle = vegetationSystemPro.vegetationStudioTerrainList[k].ProcessSplatmapScaleRule(vegetationItemInfoPro.TerrainTextureScaleRuleList, instanceData, _vegetationCellRect, vegetationItemHandle);

                if (_vegetationCell.vegetationMaskList != null && vegetationItemInfoPro.UseVegetationMask == false) // vegetation mask exclude mode -- after all scale rules to respect new bounds
                    for (int k = 0; k < _vegetationCell.vegetationMaskList.Count; k++)  // for each exclude vegetation mask -- set overlapped nodes to be excluded
                        vegetationItemHandle = _vegetationCell.vegetationMaskList[k].SampleMask(instanceData, vegetationItemInfoPro.VegetationType, vegetationItemHandle, vegetationItemInfoPro.Bounds);
                #endregion

                #region endstage filtering / distance fall off + creating the effective instances
                if (vegetationItemInfoPro.UseDistanceFalloff)
                {
                    DistanceFalloffJob distanceFalloffJob = new()
                    {   // filter out further nodes "temporarily" -- randomized distance falloff w/ minimum distance -- either linear or w/ curve
                        RandomNumberIndex = instanceData.randomNumberIndex,
                        ControlData = instanceData.controlData, // each node gets a fixed value which multiplies with the render distance of a vegetation item thus "temporarily" reducing the effective end distance
                        Included = instanceData.included,

                        RandomNumbers = randomNumbers,

                        DistanceFalloffCurveArray = vegetationItemModelInfo.distanceFalloffCurveArray,
                        Advanced = vegetationItemInfoPro.UseAdvancedDistanceFalloff,
                        DistanceFalloffStartDistance = vegetationItemInfoPro.DistanceFalloffStartDistance
                    };
                    vegetationItemHandle = distanceFalloffJob.Schedule(instanceData.included, ilbatchCount, vegetationItemHandle);
                }

                CreateMatrixInstancesJob createInstanceMatrixJob = new()    // for each included node
                {   // convert final pos, rot, scale into a matrix for the rendering and distance/frustum culling -- include distanceFalloff for internal "MatrixInstance" for distance culling
                    Position = instanceData.position,
                    Scale = instanceData.scale,
                    Rotation = instanceData.rotation,
                    ControlData = instanceData.controlData,
                    Included = instanceData.included,

                    useDistanceFalloff = vegetationItemModelInfo.vegetationItemInfo.UseDistanceFalloff,

                    VegetationInstanceMatrixList = matrixList   // add result to the local matrix list, which gets merged later per vegetation item for each cell => the merged list gets further split into LOD lists for the rendering
                };
                vegetationItemHandle = createInstanceMatrixJob.Schedule(vegetationItemHandle);
                #endregion
            }

            #region persistent storage logic
            if (vegetationSystemPro.persistentVegetationStorage && vegetationSystemPro.persistentVegetationStorage.disablePersistentStorage == false && _skipPersistentVegetation == false) // if the persistent storage exists and is not turned off/skipped
            {   // load instances from the storage for rendering them later
                PersistentVegetationCell persistentVegetationCell = vegetationSystemPro.persistentVegetationStorage.GetPersistentVegetationCell(_vegetationCell.index); // get stored / previously generated cell
                PersistentVegetationInfo persistentVegetationInfo = persistentVegetationCell?.GetPersistentVegetationInfo(vegetationItemInfoPro.VegetationItemID);  // get data(rules) of stored / previously generated vegetation item

                if (persistentVegetationInfo != null && persistentVegetationInfo.VegetationItemList.Count > 0)  // if the persistent storage has been setup correctly and has instances stored of the item
                {
                    persistentVegetationInfo.CopyInstancesToNativeList();   // prepare internal native list for job passing (-> masking) -> rendering vegetation instances

                    if (vegetationItemInfoPro.UseVegetationMask == false && _vegetationCell.vegetationMaskList != null && vegetationSystemPro.persistentVegetationStorage.useVegetationMasking) // vegetation mask exlusions
                        for (int k = 0; k < _vegetationCell.vegetationMaskList.Count; k++)
                            vegetationItemHandle = _vegetationCell.vegetationMaskList[k].SampleMaskPersistentStorage(persistentVegetationInfo, vegetationItemInfoPro.VegetationType, vegetationItemHandle, vegetationItemInfoPro.Bounds);

                    if (doRuntimeSpawn == false)
                    {
                        matrixList.ResizeUninitialized(persistentVegetationInfo.VegetationItemList.Count);  // prepare matrixList capacity for when exclusively using painted/baked items
                        LoadPersistentStorageToMatrixWideJob loadPersistentStorageToMatrixJob = new()
                        {
                            InstanceList = persistentVegetationInfo.NativeVegetationItemList,
                            VegetationInstanceMatrixList = matrixList,
                            VegetationSystemPosition = vegetationSystemPro.VegetationSystemPosition
                        };
                        vegetationItemHandle = loadPersistentStorageToMatrixJob.Schedule(matrixList, ilbatchCount, vegetationItemHandle);   // loop based on the static instanceList / adjusted matrixList
                    }
                    else
                    {
                        LoadPersistentStorageToMatrixJob loadPersistentStorageToMatrixJob = new()
                        {
                            InstanceList = persistentVegetationInfo.NativeVegetationItemList,
                            VegetationInstanceMatrixList = matrixList,
                            VegetationSystemPosition = vegetationSystemPro.VegetationSystemPosition
                        };
                        vegetationItemHandle = loadPersistentStorageToMatrixJob.Schedule(vegetationItemHandle); // loop based on the dynamic run-time instanceList
                    }
                }
            }
            #endregion

            JobHandle.ScheduleBatchedJobs();    // run/prioritize all rule jobs(persistent storage loading)

            return vegetationItemHandle;
        }

        public void Dispose()
        {
            if (jobHandleList.IsCreated)
                jobHandleList.Dispose();

            if (cellJobHandleList.IsCreated)
                cellJobHandleList.Dispose();

            if (randomNumbers.IsCreated)
                randomNumbers.Dispose();

            vegetationInstanceDataPool.Dispose();
        }
    }
}