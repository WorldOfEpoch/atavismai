using AwesomeTechnologies.Vegetation.PersistentStorage;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    #region utility logic
    [BurstCompile]
    public struct InitializeInstanceData : IJobFor
    {
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<byte> Included;

        public void Execute(int index)
        {
            Included[index] = 0;    // by default no node should spawn vegetation instances -- init with zero to not have undefined values of previous memory cells
        }
    }

    [BurstCompile]
    public struct IncludeEvaluationJob : IJobParallelForDefer   // needed job for "split" behavior -- allows for multiple, separate, textures instead of all being "combined" as one
    {
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            float2 controlData = ControlData[index];

            if (ControlData[index].y == 1)  // don't spawn unless flagged otherwise -- (by default => as set/initialized in the "terrain sampling")
                Included[index] = 0;
            else
                controlData.y = 1;  // reset state for re-processing of another terrain -- for cases where multiple terrains share the same vegetation cells

            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct MergeCellInstancesJob : IJob
    {
        [WriteOnly] public NativeList<MatrixInstance> OutputNativeList;
        [ReadOnly] public NativeList<MatrixInstance> InputNativeList;

        public void Execute()
        {
            if (InputNativeList.Length > 0)
                OutputNativeList.AddRange(InputNativeList.AsArray());
        }
    }
    #endregion

    #region initial cell node creation and filtering + density/cutoff rules
    [BurstCompile]
    public struct GenerateCellNodesJob : IJobFor
    {
        [NativeDisableParallelForRestriction] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] public NativeList<int> RandomNumberIndex;

        [ReadOnly] public NativeArray<float> RandomNumbers;
        [ReadOnly] public float3 CellCorner;
        [ReadOnly] public Rect CellRect;
        [ReadOnly] public int CellIndex;
        [ReadOnly] public float DefaultSpawnChance;
        [ReadOnly] public int Seed;
        [ReadOnly] public bool UseSamplePointOffset;
        [ReadOnly] public float SamplePointMinOffset;
        [ReadOnly] public float SamplePointMaxOffset;
        [ReadOnly] public bool RandomizePosition;
        [ReadOnly] public float CalculatedSampleDistance;
        [ReadOnly] public int XSamples;
        [ReadOnly] public int ZSamples;

        public void Execute(int index)
        {
            int z = (int)math.floor((float)index / XSamples);
            int x = index - (z * XSamples);

            int randomNumberIndex = x + (z * ZSamples) + CellIndex + Seed;
            while (randomNumberIndex > 9999)
                randomNumberIndex -= 10000;
            RandomNumberIndex[index] = randomNumberIndex;

            // define a new node w/ def vals -- apply random offset -- nodes after that receive initial filtering using the "spawnChance" through "density/cutoff/biomeMask" rules > late stage spawnChance filter
            Position[index] = new(CellCorner.x + (x * CalculatedSampleDistance), 0, CellCorner.z + (z * CalculatedSampleDistance));

            if (RandomizePosition)
            {
                Position[index] += GetRandomOffset(CalculatedSampleDistance * 0.5f, RandomNumberIndex[index]);
                RandomNumberIndex[index] += 2;
            }

            if (UseSamplePointOffset)
            {   // "random rotation" times the random offset -- "quaternion.Euler" accepts radians so convert from degrees (365)
                Position[index] += math.mul(quaternion.Euler(new float3(0, math.frac(SamplePointMinOffset) * 6.37045176978f, 0)), new float3(RandomRange(RandomNumberIndex[index], SamplePointMinOffset, SamplePointMaxOffset), 0, 0));
                RandomNumberIndex[index]++;
            }

            // "controlData" is used for passing/storing additional node info between jobs -- x = biomeMaskDistance -- y = spawnChance -- disable spawning for nodes outside of the cell due to randomization
            ControlData[index] = new(1000000f, CellRect.Contains(new float2(Position[index].x, Position[index].z)) ? DefaultSpawnChance : 0);
        }

        private float3 GetRandomOffset(float _distance, int _randomNumberIndex)
        {
            return new float3(RandomRange(_randomNumberIndex, -_distance, _distance), 0, RandomRange(_randomNumberIndex + 1, -_distance, _distance));
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }
    }

    [BurstCompile]
    public struct PerlinNoiseCutoffJob : IJobFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [ReadOnly] public float PerlinCutoff;
        [ReadOnly] public float PerlinScale;
        [ReadOnly] public bool InversePerlinMask;
        [ReadOnly] public float2 Offset;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0)
                return;

            float2 controlData = ControlData[index];

            float perlin = noise.cnoise(new float2((Position[index].x + Offset.x) / PerlinScale, (Position[index].z + Offset.y) / PerlinScale));
            perlin += 1f;
            perlin *= 0.5f;
            perlin = math.clamp(perlin, 0, 1);
            perlin = math.select(perlin, 1 - perlin, InversePerlinMask);
            if (perlin <= PerlinCutoff)
                controlData.y = 0;

            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct PerlinNoiseDensityJob : IJobFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [ReadOnly] public float PerlinScale;
        [ReadOnly] public float PerlinBalancing;
        [ReadOnly] public bool InversePerlinMask;
        [ReadOnly] public float2 Offset;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0)
                return;

            float2 controlData = ControlData[index];

            float perlin = noise.cnoise(new float2((Position[index].x + Offset.x) / PerlinScale, (Position[index].z + Offset.y) / PerlinScale));
            perlin += 1f;
            perlin *= 0.5f;
            perlin = math.clamp(math.select(perlin - PerlinBalancing, perlin + -PerlinBalancing, perlin >= 0), 0, 1);
            perlin = math.select(perlin, 1 - perlin, InversePerlinMask);
            controlData.y *= perlin;

            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct SpawnChanceFilterJob : IJobFor
    {
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] public NativeList<int> RandomNumberIndex;

        [ReadOnly] public NativeArray<float> RandomNumbers;
        [ReadOnly] public float Density;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0)
                return;

            float2 controlData = ControlData[index];

            if (RandomCutoff(controlData.y * Density, RandomNumberIndex[index]))
                controlData.y = 0;
            RandomNumberIndex[index]++;

            ControlData[index] = controlData;
        }

        private bool RandomCutoff(float _value, int _randomNumberIndex)
        {
            return !(_value > RandomRange(_randomNumberIndex, 0, 1));
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }
    }
    #endregion

    #region include/exclude + height/steepness/concave rule
    [BurstCompile]
    public struct TerrainSourceExcludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> TerrainSourceID;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public TerrainSourceRule TerrainSourceRule;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (TerrainSourceRule[TerrainSourceID[index]])
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct TerrainSourceIncludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> TerrainSourceID;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [ReadOnly] public TerrainSourceRule TerrainSourceRule;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (TerrainSourceRule[TerrainSourceID[index]] == false)
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct HeightFilterJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<int> RandomNumberIndex;

        [ReadOnly] public float MinHeight;
        [ReadOnly] public float MaxHeight;
        [ReadOnly] public NativeArray<float> HeightRuleCurveArray;
        [ReadOnly] public NativeArray<float> RandomNumbers;
        [ReadOnly] public bool Advanced;
        [ReadOnly] public float MaxCurveHeight;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (Advanced)
            {
                if (RandomCutoff(SampleCurveArray((Position[index].y - MinHeight) / MaxCurveHeight), RandomNumberIndex[index]))
                    Included[index] = 0;
                RandomNumberIndex[index]++; // purposely in here to not affect existing projects
            }
            else
            {
                if (Position[index].y < MinHeight || Position[index].y > MaxHeight)
                    Included[index] = 0;
            }
        }

        private bool RandomCutoff(float _value, int _randomNumberIndex)
        {
            return !(_value > RandomRange(_randomNumberIndex, 0, 1));
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }

        private float SampleCurveArray(float _value)
        {
            if (HeightRuleCurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * HeightRuleCurveArray.Length);
            return HeightRuleCurveArray[math.clamp(index, 0, HeightRuleCurveArray.Length - 1)];
        }
    }

    [BurstCompile]
    public struct SteepnessFilterJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> TerrainNormal;
        [NativeDisableParallelForRestriction] public NativeList<int> RandomNumberIndex;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public NativeArray<float> RandomNumbers;

        [ReadOnly] public NativeArray<float> SteepnessRuleCurveArray;
        [ReadOnly] public bool Advanced;
        [ReadOnly] public float MinSteepness;
        [ReadOnly] public float MaxSteepness;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            float slopeAngle = math.degrees(math.acos(math.dot(TerrainNormal[index], new float3(0, 1, 0))));

            if (Advanced)
            {
                if (RandomCutoff(SampleCurveArray(slopeAngle / 90), RandomNumberIndex[index]))
                    Included[index] = 0;
                RandomNumberIndex[index]++; // purposely in here to not affect existing projects
            }
            else
            {
                if (slopeAngle < MinSteepness || slopeAngle > MaxSteepness)
                    Included[index] = 0;
            }
        }

        private bool RandomCutoff(float _value, int _randomNumberIndex)
        {
            return !(_value > RandomRange(_randomNumberIndex, 0, 1));
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }

        private float SampleCurveArray(float _value)
        {
            if (SteepnessRuleCurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * SteepnessRuleCurveArray.Length);
            return SteepnessRuleCurveArray[math.clamp(index, 0, SteepnessRuleCurveArray.Length - 1)];
        }
    }

    [BurstCompile]
    public struct BiomeEdgeDistanceIncludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public float MaxDistance;
        [ReadOnly] public bool Inverse;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (Inverse)
            {
                if (ControlData[index].x < MaxDistance)
                    Included[index] = 0;
            }
            else
            {
                if (ControlData[index].x > MaxDistance)
                    Included[index] = 0;
            }
        }
    }
    #endregion

    #region pos, rot, scale + end stage rules w/ scale (vegetation mask include, etc)
    [BurstCompile]
    public struct BiomeEdgeDistanceScaleJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public float MaxDistance;
        [ReadOnly] public float MinScale;
        [ReadOnly] public float MaxScale;
        [ReadOnly] public bool InverseScale;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            if (ControlData[index].x < MaxDistance)
                Scale[index] *= math.select(math.lerp(MinScale, MaxScale, ControlData[index].x / MaxDistance), math.lerp(MaxScale, MinScale, ControlData[index].x / MaxDistance), InverseScale);
        }
    }

    [BurstCompile]
    public struct PerlinNoiseScaleJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float3> Scale;
        [ReadOnly] public float PerlinScale;
        [ReadOnly] public float PerlinBalancing;
        [ReadOnly] public bool InversePerlinMask;
        [ReadOnly] public float2 Offset;
        [ReadOnly] public float MinScale;
        [ReadOnly] public float MaxScale;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            float perlin = noise.cnoise(new float2((Position[index].x + Offset.x) / PerlinScale, (Position[index].z + Offset.y) / PerlinScale));
            perlin += 1f;
            perlin *= 0.5f;
            perlin = math.clamp(math.select(perlin - PerlinBalancing, perlin + -PerlinBalancing, perlin >= 0), 0, 1);
            perlin = math.select(perlin, 1 - perlin, InversePerlinMask);
            Scale[index] *= math.lerp(MinScale, MaxScale, perlin);
        }
    }

    [BurstCompile]
    public struct PosRotScaleJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<quaternion> Rotation;
        [NativeDisableParallelForRestriction] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> TerrainNormal;
        [NativeDisableParallelForRestriction] public NativeList<int> RandomNumberIndex;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public NativeArray<float> RandomNumbers;
        [ReadOnly] public VegetationRotationType VegetationRotationType;

        [ReadOnly] public NativeArray<float> ScaleCurveArray;
        [ReadOnly] public bool Advanced;

        [ReadOnly] public float MinScale;
        [ReadOnly] public float MaxScale;
        [ReadOnly] public float3 ScaleMultiplier;
        [ReadOnly] public float3 RotationOffset;
        [ReadOnly] public float3 PositionOffset;
        [ReadOnly] public float MinUpOffset;
        [ReadOnly] public float MaxUpOffset;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            ControlData[index] = new(); // reset control data for late stage stuff -- x = distance falloff/vegetation mask include scale -- y = vegetation mask include density

            float3 angle;   // terrain normal angle delta
            float3 up = new(0, 1, 0);   // terrain normal direction -- up direction for the "position offset range"

            switch (VegetationRotationType) // rotation mode (using radians)
            {
                case VegetationRotationType.RotateY:
                    Rotation[index] = quaternion.RotateY(RandomRange(RandomNumberIndex[index], 0, 6.28318530718f));
                    RandomNumberIndex[index]++;
                    break;
                case VegetationRotationType.RotateXYZ:
                    Rotation[index] = quaternion.Euler(new float3(RandomRange(RandomNumberIndex[index], 0, 6.28318530718f), RandomRange(RandomNumberIndex[index] + 1, 0, 6.28318530718f), RandomRange(RandomNumberIndex[index] + 2, 0, 6.28318530718f)));
                    RandomNumberIndex[index] += 3;
                    up = TerrainNormal[index];
                    break;
                case VegetationRotationType.FollowTerrain:
                    angle = math.cross(-TerrainNormal[index], new float3(1, 0, 0));
                    angle = angle.y < 0 ? -angle : angle;
                    Rotation[index] = math.mul(quaternion.LookRotation(angle, TerrainNormal[index]), quaternion.AxisAngle(new float3(0, 1, 0), RandomRange(RandomNumberIndex[index], 0, 6.28318530718f)));
                    RandomNumberIndex[index]++;
                    up = TerrainNormal[index];
                    break;
                case VegetationRotationType.FollowTerrainScale:
                    angle = math.cross(-TerrainNormal[index], new float3(1, 0, 0));
                    angle = angle.y < 0 ? -angle : angle;
                    Rotation[index] = math.mul(quaternion.LookRotation(angle, TerrainNormal[index]), quaternion.AxisAngle(new float3(0, 1, 0), RandomRange(RandomNumberIndex[index], 0, 6.28318530718f)));
                    RandomNumberIndex[index]++;
                    up = TerrainNormal[index];
                    float angleScale = math.clamp(math.degrees(math.acos(math.dot(TerrainNormal[index], new float3(0, 1, 0)))) / 45f, 0, 1);
                    Scale[index] += new float3(angleScale, 0, angleScale);
                    break;
            }

            // scale
            Scale[index] *= math.select(ScaleMultiplier * RandomRange(RandomNumberIndex[index], MinScale, MaxScale), ScaleMultiplier * math.clamp(MaxScale * SampleScaleCurveArray(RandomRange(RandomNumberIndex[index], 0, 1)), MinScale, MaxScale), Advanced);
            RandomNumberIndex[index]++;

            // rot (using radians)
            Rotation[index] = math.mul(Rotation[index], quaternion.Euler(math.radians(RotationOffset)));

            // pos
            Position[index] += math.mul(Rotation[index], PositionOffset * Scale[index]);
            Position[index] += up * RandomRange(RandomNumberIndex[index], MinUpOffset * Scale[index].y, MaxUpOffset * Scale[index].y);
            RandomNumberIndex[index]++;
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }

        private float SampleScaleCurveArray(float _value)
        {
            if (ScaleCurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * ScaleCurveArray.Length);
            return ScaleCurveArray[math.clamp(index, 0, ScaleCurveArray.Length - 1)];
        }
    }
    #endregion

    #region endstage filtering / distance fall off + creating the effective instances
    [BurstCompile]
    public struct DistanceFalloffJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<int> RandomNumberIndex;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public NativeArray<float> RandomNumbers;

        [ReadOnly] public NativeArray<float> DistanceFalloffCurveArray;
        [ReadOnly] public bool Advanced;
        [ReadOnly] public float DistanceFalloffStartDistance;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            ControlData[index] = math.select(new float2(DistanceFalloffStartDistance + RandomRange(RandomNumberIndex[index], 0, 1 - DistanceFalloffStartDistance), 0),
                new float2(math.clamp(SampleCurveArray(RandomRange(RandomNumberIndex[index], 0, 1)), 0.01f, 1), 0), Advanced);
            //RandomNumberIndex[index]++;   // not needed anymore as called after relevant code
        }

        public float RandomRange(int _randomNumberIndex, float _min, float _max)
        {
            while (_randomNumberIndex > 9999)
                _randomNumberIndex -= 10000;
            return math.lerp(_min, _max, RandomNumbers[_randomNumberIndex]);
        }

        private float SampleCurveArray(float _value)
        {
            if (DistanceFalloffCurveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value) * DistanceFalloffCurveArray.Length);
            return DistanceFalloffCurveArray[math.clamp(index, 0, DistanceFalloffCurveArray.Length - 1)];
        }
    }

    [BurstCompile]
    public struct CreateMatrixInstancesJob : IJob
    {
        [ReadOnly] public NativeList<float3> Position;
        [ReadOnly] public NativeList<quaternion> Rotation;
        [ReadOnly] public NativeList<float3> Scale;
        [ReadOnly] public NativeList<float2> ControlData;
        [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public bool useDistanceFalloff;

        [WriteOnly] public NativeList<MatrixInstance> VegetationInstanceMatrixList;

        public void Execute()
        {
            for (int i = 0; i < Included.Length; i++)
            {
                if (Included[i] == 0)
                    continue;

                MatrixInstance matrixInstance = new()
                {
                    matrix = Matrix4x4.TRS(Position[i], Rotation[i], Scale[i]),
                    controlData = new float4(math.select(1, ControlData[i].x, useDistanceFalloff), 0, 0, 0)
                };
                VegetationInstanceMatrixList.Add(matrixInstance);
            }
        }
    }
    #endregion

    #region persistent storage logic
    [BurstCompile]
    public struct LoadPersistentStorageToMatrixWideJob : IJobParallelForDefer
    {
        [ReadOnly] public NativeList<PersistentVegetationItem> InstanceList;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeList<MatrixInstance> VegetationInstanceMatrixList;
        [ReadOnly] public float3 VegetationSystemPosition;

        public void Execute(int index)
        {
            MatrixInstance matrixInstance = new()
            {
                matrix = Matrix4x4.TRS(InstanceList[index].Position + VegetationSystemPosition, InstanceList[index].Rotation, InstanceList[index].Scale),
                controlData = new float4(InstanceList[index].DistanceFalloff, 0, 0, 0)
            };
            VegetationInstanceMatrixList[index] = matrixInstance;   // replace since list is static -- non run-time mode
        }
    }

    [BurstCompile]
    public struct LoadPersistentStorageToMatrixJob : IJob
    {
        [ReadOnly] public NativeList<PersistentVegetationItem> InstanceList;
        [WriteOnly] public NativeList<MatrixInstance> VegetationInstanceMatrixList;
        [ReadOnly] public float3 VegetationSystemPosition;

        public void Execute()
        {
            for (int i = 0; i < InstanceList.Length; i++)
            {
                if (InstanceList[i].DistanceFalloff <= 0)
                    continue;   // skip masked out persistent vegetation storage vegetation instances

                MatrixInstance matrixInstance = new()
                {
                    matrix = Matrix4x4.TRS(InstanceList[i].Position + VegetationSystemPosition, InstanceList[i].Rotation, InstanceList[i].Scale),
                    controlData = new float4(InstanceList[i].DistanceFalloff, 0, 0, 0)
                };
                VegetationInstanceMatrixList.Add(matrixInstance);   // add since list is dynamic -- run-time mode
            }
        }
    }
    #endregion
}