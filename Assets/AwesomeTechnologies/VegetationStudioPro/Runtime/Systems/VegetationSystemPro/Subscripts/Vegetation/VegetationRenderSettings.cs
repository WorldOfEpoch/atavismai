using UnityEngine;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    [System.Serializable]
    public class VegetationRenderSettings
    {
        public bool showLODDebug;

        public int grassShadows = -1;
        public int plantShadows = -1;
        public int objectShadows = -1;
        public int largeObjectShadows = 3;
        public int treeShadows = 3;
        public bool billboardShadows;

        public int grassCustomShadowLODIndex = 0;
        public int plantCustomShadowLODIndex = 0;
        public int objectCustomShadowLODIndex = 0;
        public int largeObjectCustomShadowLODIndex = 0;
        public int treeCustomShadowLODIndex = 0;

        public bool dayNightSupport;

        public LayerMask grassLayer = 0;
        public LayerMask plantLayer = 0;
        public LayerMask objectLayer = 0;
        public LayerMask largeObjectLayer = 0;
        public LayerMask treeLayer = 0;
        public LayerMask billboardLayer = 0;

        public int renderingLayerMask = 257;

        public bool grassBlendProbes = true;
        public bool plantBlendProbes = true;
        public bool objectBlendProbes = true;
        public bool largeObjectBlendProbes = true;
        public bool treeBlendProbes = true;
        public bool billboardBlendProbes = true;

        public ReflectionProbeUsage grassRPU = ReflectionProbeUsage.Simple;
        public ReflectionProbeUsage plantRPU = ReflectionProbeUsage.Simple;
        public ReflectionProbeUsage objectRPU = ReflectionProbeUsage.Simple;
        public ReflectionProbeUsage largeObjectRPU = ReflectionProbeUsage.Simple;
        public ReflectionProbeUsage treeRPU = ReflectionProbeUsage.Simple;
        public ReflectionProbeUsage billboardRPU = ReflectionProbeUsage.Simple;

        public bool disableInstancedIndirectWindows;
        public bool disableInstancedIndirectOsx;
        public bool disableInstancedIndirectLinux;
        public bool disableInstancedIndirectIos;
        public bool disableInstancedIndirectAndroid;
        public bool disableInstancedIndirectTvOs;
        public bool disableInstancedIndirectXboxOne;
        public bool disableInstancedIndirectPs4;
        public bool disableInstancedIndirectWsa;
        public bool disableInstancedIndirectWebGL;

        public bool enableSinglePassInstancedVR;

        #region shadows
        public int GetShadowCastingMode(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => grassShadows,
                VegetationType.Plant => plantShadows,
                VegetationType.Objects => objectShadows,
                VegetationType.LargeObjects => largeObjectShadows,
                VegetationType.Tree => treeShadows,
                _ => -1,
            };
        }

        public ShadowCastingMode GetBillboardShadowCastingMode()
        {
            return billboardShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        public int GetCustomShadowLODIndex(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => grassCustomShadowLODIndex,
                VegetationType.Plant => plantCustomShadowLODIndex,
                VegetationType.Objects => objectCustomShadowLODIndex,
                VegetationType.LargeObjects => largeObjectCustomShadowLODIndex,
                VegetationType.Tree => treeCustomShadowLODIndex,
                _ => -1,
            };
        }
        #endregion

        #region layers
        public LayerMask GetLayer(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => grassLayer,
                VegetationType.Plant => plantLayer,
                VegetationType.Objects => objectLayer,
                VegetationType.LargeObjects => largeObjectLayer,
                VegetationType.Tree => treeLayer,
                _ => 0,
            };
        }

        public LayerMask GetBillboardLayer()
        {
            return billboardLayer;
        }
        #endregion

        #region probes
        public LightProbeUsage GetBlendProbeUsage(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => grassBlendProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off,
                VegetationType.Plant => plantBlendProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off,
                VegetationType.Objects => objectBlendProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off,
                VegetationType.LargeObjects => largeObjectBlendProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off,
                VegetationType.Tree => treeBlendProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off,
                _ => LightProbeUsage.BlendProbes,
            };
        }

        public LightProbeUsage GetBillboardBlendProbeUsage()
        {
            return billboardBlendProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off;
        }

        public ReflectionProbeUsage GetReflectionProbeUsage(VegetationType _vegetationType)
        {
            return _vegetationType switch
            {
                VegetationType.Grass => grassRPU,
                VegetationType.Plant => plantRPU,
                VegetationType.Objects => objectRPU,
                VegetationType.LargeObjects => largeObjectRPU,
                VegetationType.Tree => treeRPU,
                _ => ReflectionProbeUsage.Simple,
            };
        }

        public ReflectionProbeUsage GetBillboardReflectionProbeUsage()
        {
            return billboardRPU;
        }
        #endregion

        #region platform support
        public bool UseInstancedIndirect()
        {
            if (SystemInfo.supportsComputeShaders == false)
                return false;

            return Application.platform switch
            {
                RuntimePlatform.WindowsEditor => !disableInstancedIndirectWindows,
                RuntimePlatform.WindowsPlayer => !disableInstancedIndirectWindows,
                RuntimePlatform.OSXEditor => !disableInstancedIndirectOsx,
                RuntimePlatform.OSXPlayer => !disableInstancedIndirectOsx,
                RuntimePlatform.LinuxEditor => !disableInstancedIndirectLinux,
                RuntimePlatform.LinuxPlayer => !disableInstancedIndirectLinux,
                RuntimePlatform.IPhonePlayer => !disableInstancedIndirectIos,
                RuntimePlatform.Android => !disableInstancedIndirectAndroid,
                RuntimePlatform.tvOS => !disableInstancedIndirectTvOs,
                RuntimePlatform.XboxOne => !disableInstancedIndirectXboxOne,
                RuntimePlatform.PS4 => !disableInstancedIndirectPs4,
                RuntimePlatform.WSAPlayerX64 => !disableInstancedIndirectWsa,
                RuntimePlatform.WSAPlayerX86 => !disableInstancedIndirectWsa,
                RuntimePlatform.WSAPlayerARM => !disableInstancedIndirectWsa,
                RuntimePlatform.WebGLPlayer => !disableInstancedIndirectWebGL,
                _ => false,
            };
        }
        #endregion
    }
}