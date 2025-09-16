using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public interface IShaderController
    {
        // called to detect whether the shader controller has been made for the passed shader/material
        bool MatchShader(string _shaderName);

        // called to detect whether the last LOD is using a billboard shader or for when the billboard shader should sync values with its mesh version
        bool MatchBillboardShader(Material _material);

        ShaderControllerSettings Settings { get; set; }

        // called once when a new vegetation item is added or when refreshing it
        void CreateDefaultSettings(Material[] _materials);

        // called through various system refreshes or when material settings change on a vegetation item
        void UpdateMaterial(Material _material, EnvironmentSettings _environmentSettings, VegetationItemInfoPro _vegItemInfoPro);
    }

    [Serializable]
    public class ShaderControllerSettings : BaseControllerSettings
    {
        public string heading;  // name of the shader controller in the material settings of a vegetation item
        public bool supportsInstancedIndirect;  // whether newly added items should automatically have "Instanced indirect" chosen as the render mode
        public bool isSpeedTree;    // whether made with "SpeedTree" -- whether to prepare a "SpeedTree WindBridge" to get wind working (~workaround) => make sure the engine applies CPU based "SpeedTree" wind data
        public bool supportsWind;   // whether billboards should use wind
        public bool supportsHueVariation;   // whether billboards should use hue variaton
        public bool supportsSnow;   // whether billboards should use snow

        // pass wind data to align tree meshes with their billboards -- only done internally to keep global wind synchronized
        public float ptInitialBend;
        public float ptStiffness;
        public float ptDrag;
        public float ptShiverDrag;
        public float ptShiverDirectionality;

        // assign custom shaders used for the billboard texture atlas generation
        public string overrideBillboardAtlasShader = "";
        public string overrideBillboardAtlasNormalShader = "";

        public ShaderControllerSettings()
        {

        }
    }
}