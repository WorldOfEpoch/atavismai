using UnityEngine;
using Unity.Mathematics;
using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.VegetationStudio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AwesomeTechnologies.VegetationSystem
{
    public class BillboardAtlasRenderer
    {
        public static Texture2D GenerateBillboardTexture(VegetationItemInfoPro _vegItemInfoPro, Shader _replacementShader, bool _generateNormals)
        {
#if UNITY_EDITOR
            bool originalFogState = RenderSettings.fog;  // store original fog value
            Unsupported.SetRenderSettingsUseFogNoDirty(false);  // disable fog
#endif
            // atlas size/tiling/resolution -- based on the set quality level => setup containers and iteration depth
            int resolution = GetBillboardQualityTileResolution(_vegItemInfoPro.BillboardQuality);
            int rowCount = GetBillboardQualityRowCount(_vegItemInfoPro.BillboardQuality);
            int columnCount = GetBillboardQualityColumnCount(_vegItemInfoPro.BillboardQuality);
            Texture2D newTexture = new(resolution * columnCount, resolution * rowCount, TextureFormat.RGBA32, false, true, true) { hideFlags = HideFlags.HideAndDontSave }; // output texture
            RenderTexture frameBuffer = new(resolution, resolution, 32, RenderTextureFormat.ARGB32) { hideFlags = HideFlags.HideAndDontSave };  // temp render data
            // --

            // create and setup the source's gameObject and render data -- create a temp go for later modifications and specific rendering graphics
            GameObject sourceLodGO = MeshUtility.InstantiateSourceLOD(_vegItemInfoPro.VegetationPrefab, _vegItemInfoPro.BillboardSourceLODLevel, float3.zero, Quaternion.Euler(_vegItemInfoPro.RotationOffset), out MeshFilter meshFilter, out MeshRenderer meshRenderer);
            sourceLodGO.hideFlags = HideFlags.HideAndDontSave;
            if (sourceLodGO == null || meshFilter == null || meshFilter.sharedMesh == null || meshRenderer == null)
            {
                Debug.LogError("VSP internal error log: Billboards: Data needed for the texture atlas generation is missing => Check the Prefab > MeshFilter > Mesh > MeshRenderer");
                goto skipGeneration;
            }
            Bounds bounds = MeshUtility.CalculateBounds(_vegItemInfoPro.VegetationPrefab, (int)_vegItemInfoPro.BillboardSourceLODLevel);    // get/enlarge the effective "physical bounds" ..ensure "from the ground up" logic
            float maxBoundsSize = math.max(math.max(bounds.size.x, bounds.size.y), bounds.size.z);
            SetupBillboardAtlasShader(_vegItemInfoPro, meshRenderer, _replacementShader, maxBoundsSize);

            if (_vegItemInfoPro.BillboardRecalculateNormals)
                MeshUtility.RecalculateMeshNormals(sourceLodGO, _vegItemInfoPro.BillboardNormalBlendFactor);
            // --

            // create and define a new camera -- create and position the camera's gameObject
            GameObject cameraGO = new("Billboard texture atlas camera") { hideFlags = HideFlags.HideAndDontSave };
            cameraGO.transform.position = new float3(0, bounds.extents.y - (MeshUtility.GetMinVertexPosition(_vegItemInfoPro.VegetationPrefab) * 0.5f), 0); // align the camera to the bounds / object to render
            Camera utilityCamera = cameraGO.AddComponent<Camera>();
            utilityCamera.backgroundColor = new(0, 0, 0, 0);    // black/transparent => remove background entirely as not needed -- ensure best "outlining" and "backlit-coloring" -- ensure best "clip" support performance thus quality
            utilityCamera.clearFlags = CameraClearFlags.Color;
            utilityCamera.orthographic = true;
            utilityCamera.orthographicSize = maxBoundsSize * 0.5f;  // "orthographicExtents"
            utilityCamera.farClipPlane = maxBoundsSize;
            utilityCamera.nearClipPlane = -maxBoundsSize;
            utilityCamera.targetTexture = frameBuffer;
            // --

            float xAngleStep = 360f / rowCount / 4;
            float yAngleStep = 360f / columnCount;
            Graphics.SetRenderTarget(frameBuffer);  // set the global "renderTexture"
            GL.Viewport(new(0, 0, frameBuffer.width, frameBuffer.height));

            for (int i = 0; i < columnCount; i++)   // for each column needed for the atlas -- base (2D) rotation
                for (int j = 0; j < rowCount; j++)  // for each row needed for the atlas -- extra (3D) rotation
                {   // rotate the camera in steps for each needed perspective => based on the chosen quality level
                    utilityCamera.transform.rotation = math.mul(quaternion.AxisAngle(math.up(), math.radians(yAngleStep * i)), quaternion.AxisAngle(math.right(), math.radians(xAngleStep * j)));

                    GL.PushMatrix();
                    GL.Clear(true, true, utilityCamera.backgroundColor, 1);
                    GL.LoadProjectionMatrix(utilityCamera.projectionMatrix);
                    GL.modelview = utilityCamera.worldToCameraMatrix;

                    for (int k = 0; k < meshRenderer.sharedMaterials.Length; k++)   // render the source gameObject into the render texture -- multiple angles => as set through the chosen quality mode
                    {
                        if (meshRenderer.sharedMaterials[k] == null) continue;
                        Material material = meshRenderer.sharedMaterials[k];
                        material.SetPass(0);
                        Graphics.DrawMeshNow(meshFilter.sharedMesh, Matrix4x4.TRS(meshRenderer.transform.position, meshRenderer.transform.rotation, meshRenderer.transform.lossyScale), k);
                    }

                    GL.PopMatrix();

                    newTexture.ReadPixels(new(0, 0, frameBuffer.width, frameBuffer.height), i * resolution, j * resolution);    // read pixels from the "frameBuffer" at the correct pixels through 
                }

            newTexture.Apply(); // apply read pixels onto the texture

            RenderTexture.active = null;    // reset the global "renderTexture"
            Object.DestroyImmediate(cameraGO);
        skipGeneration: // safety check
            Object.DestroyImmediate(sourceLodGO);
            Object.DestroyImmediate(frameBuffer);

#if UNITY_EDITOR
            Unsupported.SetRenderSettingsUseFogNoDirty(originalFogState);   // restore original fog value
#endif

            return newTexture;
        }

        public static int GetBillboardQualityTileResolution(BillboardQuality _billboardQuality)
        {
            return _billboardQuality switch
            {
                BillboardQuality.Mono_256x or BillboardQuality.Quad_256x or BillboardQuality.Octa_256x or BillboardQuality.HexaDeca_256x or BillboardQuality.Octa3D_256x or BillboardQuality.HexaDeca3D_256x => 256,
                BillboardQuality.Mono_512x or BillboardQuality.Quad_512x or BillboardQuality.Octa_512x or BillboardQuality.Octa3D_512x => 512,
                _ => 128,
            };
        }

        public static int GetBillboardQualityRowCount(BillboardQuality _billboardQuality)
        {
            return _billboardQuality switch
            {
                BillboardQuality.Octa3D_128x or BillboardQuality.Octa3D_256x or BillboardQuality.Octa3D_512x => 8,
                BillboardQuality.HexaDeca3D_256x => 16,
                _ => 1,
            };
        }

        public static int GetBillboardQualityColumnCount(BillboardQuality _billboardQuality)
        {
            return _billboardQuality switch
            {
                BillboardQuality.Mono_128x or BillboardQuality.Mono_256x or BillboardQuality.Mono_512x => 1,
                BillboardQuality.Quad_128x or BillboardQuality.Quad_256x or BillboardQuality.Quad_512x => 4,
                BillboardQuality.HexaDeca_256x or BillboardQuality.HexaDeca3D_256x => 16,
                _ => 8,
            };
        }

        private static void SetupBillboardAtlasShader(VegetationItemInfoPro _vegItemInfoPro, MeshRenderer _meshRenderer, Shader _replacementShader, float _maxBoundsSize)
        {
            VegetationItemModelInfo vegItemModelInfo = VegetationStudioManager.Instance.VegetationSystemList[0].GetVegetationItemModelInfo(_vegItemInfoPro.VegetationItemID);

            Material[] materials = new Material[_meshRenderer.sharedMaterials.Length];
            for (int j = 0; j < _meshRenderer.sharedMaterials.Length; j++)
                if (_meshRenderer.sharedMaterials[j] != null)
                {
                    materials[j] = new(_meshRenderer.sharedMaterials[j]);
                    materials[j].shader = _replacementShader;   // set the replacement "billboardTexture" shader -- do this first before writing to new properties

                    if (materials[j].HasProperty("_DepthBoundsSize"))
                        materials[j].SetFloat("_DepthBoundsSize", _maxBoundsSize);  // pass depthInfo to bake alpha depth into the texture for a render shader later

                    Texture diffuseTexture = ShaderUtility.GetTexture_Diffuse(_meshRenderer.sharedMaterials[j]);
                    if (diffuseTexture)
                        materials[j].SetTexture("_MainTex", diffuseTexture);

                    if (materials[j].HasProperty("_Color") && _vegItemInfoPro.eBillboardAtlasColorSource == EBillboardAtlasColorSource.TextureColorBake)
                        if (_vegItemInfoPro.useShaderControllerOverrides && vegItemModelInfo != null)   // whether the item model info has been created -- it uses shaderController overrides -- it has a valid shaderController setup
                        {
                            bool hasTintColor = false;
                            for (int k = 0; k < vegItemModelInfo.shaderControllers?.Length; k++)
                                if (vegItemModelInfo.shaderControllers[k] != null && vegItemModelInfo.shaderControllers[k].Settings != null)
                                    if (hasTintColor = vegItemModelInfo.shaderControllers[k].Settings.HasPropertyValue("TintColor1"))
                                        materials[j].SetColor("_Color", vegItemModelInfo.shaderControllers[k].Settings.GetColorPropertyValue("TintColor1"));

                            if (hasTintColor == false)
                                materials[j].SetColor("_Color", ShaderUtility.GetColor_Base(vegItemModelInfo.vegetationMaterialsLOD0[j]));  // get the color of the effective material for the vegetation instances
                        }
                        else
                            materials[j].SetColor("_Color", ShaderUtility.GetColor_Base(_meshRenderer.sharedMaterials[j])); // get the color from the initial prefab
                }

            _meshRenderer.sharedMaterials = materials;
        }

        public static void RemoveBillboardAtlasPixelBleed(Texture2D _texture, BillboardQuality _billboardQuality)
        {
            int tileResolution = GetBillboardQualityTileResolution(_billboardQuality);
            int columnCount = GetBillboardQualityColumnCount(_billboardQuality);

            for (int i = 0; i < columnCount; i++)
                for (int j = 0; j < tileResolution / 64; j++)   // remove "x" amount of lines of pixels -- avoid bleeding into the top/bottom of the same tile
                    for (int k = 0; k < _texture.width; k++)
                    {
                        Color pixelColor = _texture.GetPixel(k, i * tileResolution + j);
                        pixelColor.a = 0;   // "invisible" the potentially bleeding lines of pixels
                        _texture.SetPixel(k, i * tileResolution + j, pixelColor);
                    }

            _texture.Apply();
        }
    }
}