using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using Unity.Jobs;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.Masks
{
    [Serializable]
    public class TextureMaskGroup
    {
        public List<TextureMask> TextureMaskList = new();
        public string TextureMaskName;
        public string TextureMaskGroupID;
        public TextureMaskSettings Settings = new();

        public TextureMaskGroup()
        {
            TextureMaskName = "New texture mask group";
            TextureMaskGroupID = Guid.NewGuid().ToString();
            Settings.AddRgbaSelectorProperty("ChannelSelector", "Select channel", "", 0);
            Settings.AddBooleanProperty("Inverse", "Inverse", "", false);
        }

        public Texture2D GetPreviewTexture()
        {
            for (int i = 0; i < TextureMaskList.Count; i++)
                if (TextureMaskList[i].MaskTexture)
                    return TextureMaskList[i].MaskTexture;
            return null;
        }

        public JobHandle SampleIncludeMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            for (int i = 0; i < TextureMaskList.Count; i++)
                _dependsOn = TextureMaskList[i].SampleIncludeMask(_instanceData, _cellRect, _textureMaskRule, _dependsOn);
            return _dependsOn;
        }

        public JobHandle SampleExcludeMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            for (int i = 0; i < TextureMaskList.Count; i++)
                _dependsOn = TextureMaskList[i].SampleExcludeMask(_instanceData, _cellRect, _textureMaskRule, _dependsOn);
            return _dependsOn;
        }

        public JobHandle SampleDensityMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            for (int i = 0; i < TextureMaskList.Count; i++)
                _dependsOn = TextureMaskList[i].SampleDensityMask(_instanceData, _cellRect, _textureMaskRule, _dependsOn);
            return _dependsOn;
        }

        public JobHandle SampleScaleMask(VegetationInstanceData _instanceData, Rect _cellRect, TextureMaskRule _textureMaskRule, JobHandle _dependsOn)
        {
            for (int i = 0; i < TextureMaskList.Count; i++)
                _dependsOn = TextureMaskList[i].SampleScaleMask(_instanceData, _cellRect, _textureMaskRule, _dependsOn);
            return _dependsOn;
        }
    }
}