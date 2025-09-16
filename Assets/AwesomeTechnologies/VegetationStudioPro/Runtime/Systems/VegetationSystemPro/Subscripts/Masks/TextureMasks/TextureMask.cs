using AwesomeTechnologies.VegetationSystem;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.Masks
{
    public struct RGBABytes // bytes for texture mask vegetation rules
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }

    [Serializable]
    public class TextureMask
    {
        public Texture2D MaskTexture;
        public Rect TextureRect;
        public Vector2 Repeat = Vector2.one;
        //private NativeArray<RGBABytes> rgbaChannelArray;

        public TextureMask(Texture2D _maskTexture, Rect _textureRect)
        {
            MaskTexture = _maskTexture;
            TextureRect = _textureRect;
            //rgbaChannelArray = _maskTexture.GetRawTextureData<RGBABytes>();
        }

        //public NativeArray<RGBABytes> VerifyTextureMaskAccess()
        //{
        //    return rgbaChannelArray = MaskTexture.GetRawTextureData<RGBABytes>();
        //}

        public JobHandle SampleIncludeMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            if (MaskTexture == null || _cellRect.Overlaps(TextureRect) == false)
                return _dependsOn;

            RGBAMaskIncludeJob rgbaMaskIncludeJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,
                Included = _instanceData.included,

                MinBrightness = (int)math.round(_textureMaskRule.MinDensity * 255),
                MaxBrightness = (int)math.round(_textureMaskRule.MaxDensity * 255),
                Inverse = _textureMaskRule.GetBooleanPropertyValue("Inverse"),
                SelectedChannel = _textureMaskRule.GetIntPropertyValue("ChannelSelector"),

                RGBAChannelArray = MaskTexture.GetRawTextureData<RGBABytes>(),
                TextureWidth = MaskTexture.width,
                TextureHeight = MaskTexture.height,
                TexelSize = new(TextureRect.width / MaskTexture.width, TextureRect.height / MaskTexture.height),
                TextureRect = TextureRect,
                Repeat = Repeat
            };
            return rgbaMaskIncludeJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public JobHandle SampleExcludeMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            if (MaskTexture == null || _cellRect.Overlaps(TextureRect) == false)
                return _dependsOn;

            RGBAMaskExcludeJob rgbaMaskExcludeJob = new()
            {
                Position = _instanceData.position,
                Included = _instanceData.included,

                MinBrightness = (int)math.round(_textureMaskRule.MinDensity * 255),
                MaxBrightness = (int)math.round(_textureMaskRule.MaxDensity * 255),
                Inverse = _textureMaskRule.GetBooleanPropertyValue("Inverse"),
                SelectedChannel = _textureMaskRule.GetIntPropertyValue("ChannelSelector"),

                RGBAChannelArray = MaskTexture.GetRawTextureData<RGBABytes>(),
                TextureWidth = MaskTexture.width,
                TextureHeight = MaskTexture.height,
                TexelSize = new(TextureRect.width / MaskTexture.width, TextureRect.height / MaskTexture.height),
                TextureRect = TextureRect,
                Repeat = Repeat
            };
            return rgbaMaskExcludeJob.Schedule(_instanceData.included, 64, _dependsOn);
        }

        public JobHandle SampleDensityMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            if (MaskTexture == null || _cellRect.Overlaps(TextureRect) == false)
                return _dependsOn;

            RGBAMaskDensityJob rgbaMaskDensityJob = new()
            {
                Position = _instanceData.position,
                ControlData = _instanceData.controlData,

                DensityMultiplier = _textureMaskRule.DensityMultiplier,
                MinDensity = _textureMaskRule.MinDensity,
                MaxDensity = _textureMaskRule.MaxDensity,
                BrightnessThreshold = (_textureMaskRule.BrightnessThreshold * 255),
                MinBrightness = (int)math.round(_textureMaskRule.MinBrightness * 255),
                MaxBrightness = (int)math.round(_textureMaskRule.MaxBrightness * 255),
                Inverse = _textureMaskRule.GetBooleanPropertyValue("Inverse"),
                SelectedChannel = _textureMaskRule.GetIntPropertyValue("ChannelSelector"),

                RGBAChannelArray = MaskTexture.GetRawTextureData<RGBABytes>(),
                TextureWidth = MaskTexture.width,
                TextureHeight = MaskTexture.height,
                TexelSize = new(TextureRect.width / MaskTexture.width, TextureRect.height / MaskTexture.height),
                TextureRect = TextureRect,
                Repeat = Repeat
            };
            return rgbaMaskDensityJob.Schedule(_instanceData.controlData, 64, _dependsOn);
        }

        public JobHandle SampleScaleMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            if (MaskTexture == null || _cellRect.Overlaps(TextureRect) == false)
                return _dependsOn;

            RGBAMaskScaleJob rgbaMaskScaleJob = new()
            {
                Position = _instanceData.position,
                Scale = _instanceData.scale,
                Included = _instanceData.included,

                ScaleMultiplier = _textureMaskRule.ScaleMultiplier,
                MinScale = _textureMaskRule.MinDensity,
                MaxScale = _textureMaskRule.MaxDensity,
                BrightnessThreshold = (_textureMaskRule.BrightnessThreshold * 255),
                MinBrightness = (int)math.round(_textureMaskRule.MinBrightness * 255),
                MaxBrightness = (int)math.round(_textureMaskRule.MaxBrightness * 255),
                Inverse = _textureMaskRule.GetBooleanPropertyValue("Inverse"),
                SelectedChannel = _textureMaskRule.GetIntPropertyValue("ChannelSelector"),

                RGBAChannelArray = MaskTexture.GetRawTextureData<RGBABytes>(),
                TextureWidth = MaskTexture.width,
                TextureHeight = MaskTexture.height,
                TexelSize = new(TextureRect.width / MaskTexture.width, TextureRect.height / MaskTexture.height),
                TextureRect = TextureRect,
                Repeat = Repeat
            };
            return rgbaMaskScaleJob.Schedule(_instanceData.included, 64, _dependsOn);
        }
    }

    #region jobs
    [BurstCompile]
    public struct RGBAMaskIncludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;
        [ReadOnly] public int SelectedChannel;

        [ReadOnly] public NativeArray<RGBABytes> RGBAChannelArray;
        [ReadOnly] public int TextureWidth;
        [ReadOnly] public int TextureHeight;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public Rect TextureRect;
        [ReadOnly] public float2 Repeat;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            int x = (int)math.round(math.frac((Position[index].x - TextureRect.position.x) / TexelSize.x / TextureWidth * Repeat.x) * TextureWidth);    // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"
            int z = (int)math.round(math.frac((Position[index].z - TextureRect.position.y) / TexelSize.y / TextureHeight * Repeat.y) * TextureHeight);  // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"

            if (x < 0 || x >= TextureWidth || z < 0 || z >= TextureHeight)
                return;

            float2 controlData = ControlData[index];

            int brightness = math.select(math.select(math.select(math.select(0,
                RGBAChannelArray[x + (z * TextureWidth)].A, SelectedChannel == 3),
                RGBAChannelArray[x + (z * TextureWidth)].B, SelectedChannel == 2),
                RGBAChannelArray[x + (z * TextureWidth)].G, SelectedChannel == 1),
                RGBAChannelArray[x + (z * TextureWidth)].R, SelectedChannel == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (brightness >= MinBrightness && brightness <= MaxBrightness) // if point is on the texture mask then "flag" as affected => (if point is not on the texture mask then exclude)
                controlData.y = 0;  // flag as on the texture to not exclude (which happens by default otherwise)

            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct RGBAMaskExcludeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<byte> Included;

        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;
        [ReadOnly] public int SelectedChannel;

        [ReadOnly] public NativeArray<RGBABytes> RGBAChannelArray;
        [ReadOnly] public int TextureWidth;
        [ReadOnly] public int TextureHeight;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public Rect TextureRect;
        [ReadOnly] public float2 Repeat;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            int x = (int)math.round(math.frac((Position[index].x - TextureRect.position.x) / TexelSize.x / TextureWidth * Repeat.x) * TextureWidth);    // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"
            int z = (int)math.round(math.frac((Position[index].z - TextureRect.position.y) / TexelSize.y / TextureHeight * Repeat.y) * TextureHeight);  // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"

            if (x < 0 || x >= TextureWidth || z < 0 || z >= TextureHeight)
                return;

            int brightness = math.select(math.select(math.select(math.select(0,
                RGBAChannelArray[x + (z * TextureWidth)].A, SelectedChannel == 3),
                RGBAChannelArray[x + (z * TextureWidth)].B, SelectedChannel == 2),
                RGBAChannelArray[x + (z * TextureWidth)].G, SelectedChannel == 1),
                RGBAChannelArray[x + (z * TextureWidth)].R, SelectedChannel == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (brightness >= MinBrightness && brightness <= MaxBrightness) // if point is on the texture mask then exclude
                Included[index] = 0;
        }
    }

    [BurstCompile]
    public struct RGBAMaskDensityJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float2> ControlData;

        [ReadOnly] public float DensityMultiplier;
        [ReadOnly] public float MinDensity;
        [ReadOnly] public float MaxDensity;
        [ReadOnly] public float BrightnessThreshold;
        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;
        [ReadOnly] public int SelectedChannel;

        [ReadOnly] public NativeArray<RGBABytes> RGBAChannelArray;
        [ReadOnly] public int TextureWidth;
        [ReadOnly] public int TextureHeight;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public Rect TextureRect;
        [ReadOnly] public float2 Repeat;

        public void Execute(int index)
        {
            if (ControlData[index].y <= 0)
                return;

            int x = (int)math.round(math.frac((Position[index].x - TextureRect.position.x) / TexelSize.x / TextureWidth * Repeat.x) * TextureWidth);    // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"
            int z = (int)math.round(math.frac((Position[index].z - TextureRect.position.y) / TexelSize.y / TextureHeight * Repeat.y) * TextureHeight);  // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"

            if (x < 0 || x >= TextureWidth || z < 0 || z >= TextureHeight)
                return;

            int brightness = math.select(math.select(math.select(math.select(0,
                RGBAChannelArray[x + (z * TextureWidth)].A, SelectedChannel == 3),
                RGBAChannelArray[x + (z * TextureWidth)].B, SelectedChannel == 2),
                RGBAChannelArray[x + (z * TextureWidth)].G, SelectedChannel == 1),
                RGBAChannelArray[x + (z * TextureWidth)].R, SelectedChannel == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (!(brightness >= MinBrightness && brightness <= MaxBrightness))  // if point is not on the texture mask then don't affect density
                return;

            float2 controlData = ControlData[index];
            controlData.y *= math.clamp((brightness / BrightnessThreshold) * DensityMultiplier, MinDensity, MaxDensity);
            ControlData[index] = controlData;
        }
    }

    [BurstCompile]
    public struct RGBAMaskScaleJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<float3> Position;
        [NativeDisableParallelForRestriction] public NativeList<float3> Scale;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<byte> Included;

        [ReadOnly] public float ScaleMultiplier;
        [ReadOnly] public float MinScale;
        [ReadOnly] public float MaxScale;
        [ReadOnly] public float BrightnessThreshold;
        [ReadOnly] public float MinBrightness;
        [ReadOnly] public float MaxBrightness;
        [ReadOnly] public bool Inverse;
        [ReadOnly] public int SelectedChannel;

        [ReadOnly] public NativeArray<RGBABytes> RGBAChannelArray;
        [ReadOnly] public int TextureWidth;
        [ReadOnly] public int TextureHeight;
        [ReadOnly] public float2 TexelSize;
        [ReadOnly] public Rect TextureRect;
        [ReadOnly] public float2 Repeat;

        public void Execute(int index)
        {
            if (Included[index] == 0)
                return;

            int x = (int)math.round(math.frac((Position[index].x - TextureRect.position.x) / TexelSize.x / TextureWidth * Repeat.x) * TextureWidth);    // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"
            int z = (int)math.round(math.frac((Position[index].z - TextureRect.position.y) / TexelSize.y / TextureHeight * Repeat.y) * TextureHeight);  // "frac space" used for "repeat" logic > "shift" data out of max bounds => "rebuild"

            if (x < 0 || x >= TextureWidth || z < 0 || z >= TextureHeight)
                return;

            int brightness = math.select(math.select(math.select(math.select(0,
                RGBAChannelArray[x + (z * TextureWidth)].A, SelectedChannel == 3),
                RGBAChannelArray[x + (z * TextureWidth)].B, SelectedChannel == 2),
                RGBAChannelArray[x + (z * TextureWidth)].G, SelectedChannel == 1),
                RGBAChannelArray[x + (z * TextureWidth)].R, SelectedChannel == 0);

            if (Inverse)
                brightness = 255 - brightness;

            if (!(brightness >= MinBrightness && brightness <= MaxBrightness))  // if point is not on the texture mask then don't affect scale
                return;

            Scale[index] *= math.clamp((brightness / BrightnessThreshold) * ScaleMultiplier, MinScale, MaxScale);
        }
    }
    #endregion
}