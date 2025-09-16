#if USING_HDRP && UNITY_2023_1_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void SetupRTAS(int _cameraIndex)
        {
            // setup currently rendering camera for the RTAS
            vegetationStudioCameraList[_cameraIndex].hdCamera = HDCamera.GetOrCreate(vegetationStudioCameraList[_cameraIndex].selectedCamera);

            if (vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure == null)
            {
                // create RTAS
                vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure = new(new(RayTracingAccelerationStructure.ManagementMode.Manual, RayTracingAccelerationStructure.RayTracingModeMask.Everything, 1));
                rtCullingConfig = new();
                rtInstanceConfig = new() { dynamicGeometry = false, lightProbeUsage = LightProbeUsage.Off, renderingLayerMask = 1 };    // renderingLayerMask hardcoded as not yet fully implemented by Unity either

                // general culling parameters
                RayTracingInstanceCullingTest rtCullingTest = new();
                rtCullingTest.allowAlphaTestedMaterials = true;
                rtCullingTest.allowOpaqueMaterials = true;
                rtCullingTest.allowTransparentMaterials = true;
                rtCullingTest.allowVisualEffects = true;
                rtCullingTest.instanceMask = 255;
                rtCullingTest.layerMask = -1; // 0 = reflections -- 1 = shadows
                rtCullingTest.shadowCastingModeMask = -1; //(1 << (int)ShadowCastingMode.Off) | (1 << (int)ShadowCastingMode.On) | (1 << (int)ShadowCastingMode.TwoSided);

                // assign (general) culling parameters
                rtCullingConfig.instanceTests = new RayTracingInstanceCullingTest[1];
                rtCullingConfig.instanceTests[0] = rtCullingTest;

                // general flags
                rtCullingConfig.flags = RayTracingInstanceCullingFlags.EnableSphereCulling | RayTracingInstanceCullingFlags.EnableLODCulling;

                // sub mesh flags
                rtCullingConfig.subMeshFlagsConfig.alphaTestedMaterials = RayTracingSubMeshFlags.Enabled;
                rtCullingConfig.subMeshFlagsConfig.opaqueMaterials = RayTracingSubMeshFlags.Enabled /*| RayTracingSubMeshFlags.ClosestHitOnly*/ | RayTracingSubMeshFlags.UniqueAnyHitCalls;
                rtCullingConfig.subMeshFlagsConfig.transparentMaterials = RayTracingSubMeshFlags.Enabled;

                // triangle culling
                rtCullingConfig.triangleCullingConfig.checkDoubleSidedGIMaterial = true;
            }

            // object culling -- dynamic values
            rtCullingConfig.sphereCenter = vegetationStudioCameraList[_cameraIndex].selectedCamera.transform.position;
            rtCullingConfig.sphereRadius = vegetationStudioCameraList[_cameraIndex].selectedCamera.farClipPlane;

            // lod culling -- dynamic values
            rtCullingConfig.lodParameters.cameraPixelHeight = vegetationStudioCameraList[_cameraIndex].selectedCamera.pixelHeight;
            rtCullingConfig.lodParameters.cameraPosition = vegetationStudioCameraList[_cameraIndex].selectedCamera.transform.position;
            rtCullingConfig.lodParameters.fieldOfView = vegetationStudioCameraList[_cameraIndex].selectedCamera.fieldOfView;

            // perform culling
            //vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure.ClearInstances(); // clear existing data from last frame
            vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure.CullInstances(ref rtCullingConfig);   // cull scene gameObjects and insert them for building/rendering them later
        }

        private void DisposeRTAS(int _cameraIndex)
        {
            if (vegetationStudioCameraList[_cameraIndex].hdCamera == null)
                return;

            vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure?.ClearInstances();
            vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure?.Dispose();
            vegetationStudioCameraList[_cameraIndex].hdCamera.rayTracingAccelerationStructure = null;
            vegetationStudioCameraList[_cameraIndex].hdCamera = null;
        }
    }
}
#endif