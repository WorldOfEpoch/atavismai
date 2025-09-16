using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.Utility;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationItemModelInfo
    {
        public VegetationSystemPro vegetationSystemPro;
        public VegetationItemInfoPro vegetationItemInfo;

        public GameObject vegetationModel;

        [NonSerialized] public IShaderController[] shaderControllers;

        public int lodCount;
        public int maxLODIndex;
        public int maxLOD0Index;
        public int maxLOD1Index;
        public int maxLOD2Index;
        public int maxLOD3Index;
        public float lod0To1Distance;
        public float lod1To2Distance;
        public float lod2To3Distance;

        public Mesh vegetationMeshLod0;
        public Mesh vegetationMeshLod1;
        public Mesh vegetationMeshLod2;
        public Mesh vegetationMeshLod3;
        public Material[] vegetationMaterialsLOD0;
        public Material[] vegetationMaterialsLOD1;
        public Material[] vegetationMaterialsLOD2;
        public Material[] vegetationMaterialsLOD3;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockLOD0;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockLOD1;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockLOD2;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockLOD3;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockShadowsLOD0;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockShadowsLOD1;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockShadowsLOD2;
        public MaterialPropertyBlock vegetationMaterialPropertyBlockShadowsLOD3;

        public readonly List<CameraGraphicsBuffers> cameraGraphicsBufferList = new();
        public readonly List<MaterialPropertyBlock> cameraBillboardMaterialPropertyBlockList = new();
        public readonly List<MeshRenderer> speedTreeWindBridgeMeshRendererList = new();
        public Bounds cullingBoundsAddy;
        public int distanceBand;

        public NativeArray<float> scaleRuleCurveArray;
        public NativeArray<float> heightRuleCurveArray;
        public NativeArray<float> steepnessRuleCurveArray;
        public NativeArray<float> distanceFalloffCurveArray;

        public Material billboardMaterial;

        public VegetationItemModelInfo(VegetationSystemPro _vegetationSystemPro, VegetationItemInfoPro _vegItemInfoPro)
        {
            vegetationSystemPro = _vegetationSystemPro;
            vegetationItemInfo = _vegItemInfoPro;

            SetVegetationModel(_vegItemInfoPro);    // model
            SetShaderControllers(_vegItemInfoPro);  // shadercontroller/-s

            UpdateLODCount();   // max lod count -- minus max level set by engine
            lod0To1Distance = GetLODDistance(vegetationModel, (int)LODLevel.LOD0);
            lod1To2Distance = GetLODDistance(vegetationModel, (int)LODLevel.LOD1);
            lod2To3Distance = GetLODDistance(vegetationModel, (int)LODLevel.LOD2);

            SetRenderData(_vegItemInfoPro); // set needed data for rendering -- mesh/materials/mpb's -- 2D item shader/texture -- material to use gpu instancing -- check for missing materials/shaders
            PrepareRenderLists();   // prepare buffers for indirect rendering -- MPBs to alternate between mesh and billboard-mesh -- prepare per item GOs, for per camera parent GO for the "SpeedTree WindBridge"

            // create needed arrays -- copy the data setup through the UI into the native container
            GenerateNativeScaleRuleCurve();
            GenerateNativeHeightRuleCurve();
            GenerateNativeSteepnessRuleCurve();
            GenerateNativeDistanceFalloffCurve();

            CalculateCellCullingBoundsAddy();   // get item bounds -- raw for vegetation instance culling -- scaled by scale rules for cell culling
            distanceBand = _vegItemInfoPro.GetDistanceBand();   // distance type for vegetation cell culling / shadow culling

            switch (_vegItemInfoPro.VegetationType) // per type adjustments -- safety switch -- for external modifications => ex: debug inspector
            {
                case VegetationType.Grass:
                    _vegItemInfoPro.UseBillboards = false;  // not supported
                    _vegItemInfoPro.ColliderType = ColliderType.Disabled;   // not supported
                    break;
                case VegetationType.Plant:
                    _vegItemInfoPro.UseBillboards = false;  // not supported
                    _vegItemInfoPro.ColliderType = ColliderType.Disabled;   // not supported
                    break;
                case VegetationType.Tree:
                    if (_vegItemInfoPro.UseBillboards) CreateBillboardMaterial();    // supported
                    _vegItemInfoPro.UseDistanceFalloff = false; // not supported
                    _vegItemInfoPro.DistanceFalloffStartDistance = 1;   // safety def value
                    break;
                case VegetationType.Objects:
                    _vegItemInfoPro.UseBillboards = false;  // not supported
                    break;
                case VegetationType.LargeObjects:
                    _vegItemInfoPro.UseBillboards = false;  // not supported
                    _vegItemInfoPro.UseDistanceFalloff = false; // not supported
                    _vegItemInfoPro.DistanceFalloffStartDistance = 1;   // safety def value
                    break;
            }

            RefreshMaterials(); // set required material data -- enabled instancing -- apply shader controller values
        }

        private void SetVegetationModel(VegetationItemInfoPro _vegItemInfoPro)
        {
            if (_vegItemInfoPro.PrefabType == VegetationPrefabType.Texture)
                vegetationModel = Resources.Load<GameObject>("Models/DefaultGrassPatch");   // load default combined 2D plane grass
            else
                vegetationModel = _vegItemInfoPro.VegetationPrefab; // assign specific model

            if (vegetationModel == null)    // null check -- error log -- default model loading
            {
                Debug.LogError("VSP internal error log: Prefabs: The prefab of vegetation item: \"" + _vegItemInfoPro.Name + "\" is missing");
                vegetationModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vegetationModel.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void SetShaderControllers(VegetationItemInfoPro _vegItemInfoPro)
        {
            bool treatAsNew = _vegItemInfoPro.ShaderControllerSettings == null; // new or material-reset vegetation item

            // setup shader controller arrays
            Material[] vegetationItemMaterials = ShaderUtility.GetMaterials(vegetationModel);   // get LOD0 materials / count
            if (vegetationItemMaterials != null)
            {
                shaderControllers = new IShaderController[vegetationItemMaterials.Length];  // runtime storage for material settings -- for internal functions of this class
                if (treatAsNew || shaderControllers.Length != _vegItemInfoPro.ShaderControllerSettings.Length)  // on new or reset item
                    _vegItemInfoPro.ShaderControllerSettings = new ShaderControllerSettings[vegetationItemMaterials.Length];    // material settings stored on disk in the vegetation package
            }
            else
            {
                shaderControllers = new IShaderController[0];   // runtime storage for material settings -- for internal functions of this class
                if (treatAsNew) // on new or reset item
                    _vegItemInfoPro.ShaderControllerSettings = new ShaderControllerSettings[0]; // material settings stored on disk in the vegetation package
            }

            // get valid shader controllers if any -- create default settings -- write back existing settings
            for (int i = 0; i < shaderControllers.Length; i++)
            {
                bool hasValidShaderController = false;
                shaderControllers[i] = ShaderUtility.GetShaderController(ShaderUtility.GetShaderName(vegetationModel, i));  // get shader controller using the shader name of LOD0's material
                if (shaderControllers[i] != null)   // if a shader controller exists for this vegetation item/material/shader
                {
                    shaderControllers[i].CreateDefaultSettings(vegetationItemMaterials);    // create needed default (runtime) data for shader controllers

                    if (_vegItemInfoPro.ShaderControllerSettings[i] == null)    // on new or reset item
                        _vegItemInfoPro.ShaderControllerSettings[i] = shaderControllers[i].Settings;    // write (new) default values
                    else if (_vegItemInfoPro.ShaderControllerSettings[i].heading != shaderControllers[i].Settings.heading)
                        _vegItemInfoPro.ShaderControllerSettings[i] = shaderControllers[i].Settings;    // write (new) default values -- new (unmatching) shader controller found
                    else
                        shaderControllers[i].Settings = _vegItemInfoPro.ShaderControllerSettings[i];    // else write back custom values

                    hasValidShaderController = true;
                }

                if (vegetationItemInfo.useShaderControllerOverrides && hasValidShaderController == false && treatAsNew)
                    vegetationItemInfo.useShaderControllerOverrides = false;    // force disable to not use multiple materials for no reason

                if (vegetationItemInfo.PrefabType == VegetationPrefabType.Texture)
                    vegetationItemInfo.useShaderControllerOverrides = true; // always enable for "2D" items to match other internal logic -- each new "2D" item is it's very own item
            }

            // set default rendering mode value for newly added or material-reset vegetation items
            if (treatAsNew)
            {
                if (SystemInfo.supportsComputeShaders == false) // don't use indirect when not supported by the system
                {
                    _vegItemInfoPro.VegetationRenderMode = VegetationRenderMode.Instanced;
                    return;
                }

                for (int i = 0; i < shaderControllers?.Length; i++) // don't use indirect when not supported by ALL the shaders on the vegetation item
                    if (shaderControllers[i] != null)
                    {
                        if (shaderControllers[i].Settings.supportsInstancedIndirect == false)
                        {
                            _vegItemInfoPro.VegetationRenderMode = VegetationRenderMode.Instanced;
                            break;
                        }

                        _vegItemInfoPro.VegetationRenderMode = VegetationRenderMode.InstancedIndirect;
                    }
            }
        }

        public void UpdateLODCount()
        {
            lodCount = MeshUtility.GetLODCount(vegetationModel, shaderControllers); // dynamic
            maxLODIndex = lodCount - 1; // static
            maxLOD0Index = math.min(0 + QualitySettings.maximumLODLevel, maxLODIndex);
            maxLOD1Index = math.min(1 + QualitySettings.maximumLODLevel, maxLODIndex);
            maxLOD2Index = math.min(2 + QualitySettings.maximumLODLevel, maxLODIndex);
            maxLOD3Index = math.min(3 + QualitySettings.maximumLODLevel, maxLODIndex);
            if ((4 - QualitySettings.maximumLODLevel) < lodCount)
                lodCount = math.clamp((4 - QualitySettings.maximumLODLevel), 1, 4);
        }

        private float GetLODDistance(GameObject _rootVegetationModel, int _lodIndex)
        {
            LODGroup lodGroup = _rootVegetationModel.GetComponentInChildren<LODGroup>();
            if (lodGroup)
            {
                LOD[] lods = lodGroup.GetLODs();
                if (_lodIndex >= 0 && _lodIndex < lods.Length)
                    return (lodGroup.size / lods[_lodIndex].screenRelativeTransitionHeight);
            }

            return -1;
        }

        private void SetRenderData(VegetationItemInfoPro _vegItemInfoPro)
        {
            GameObject goTemp;
            MeshRenderer mrTemp;

            goTemp = MeshUtility.GetSourceLOD(vegetationModel, LODLevel.LOD0);  // go
            if (goTemp)
            {
                vegetationMeshLod0 = MeshUtility.GetMeshFromGameObject(goTemp); // mesh
                mrTemp = goTemp.GetComponentInChildren<MeshRenderer>(); // mesh renderer
                if (mrTemp)
                {
                    vegetationMaterialsLOD0 = CreateMaterials(mrTemp, LODLevel.LOD0);  // materials
                    mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockLOD0 = new());   // object MPB
                    mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockShadowsLOD0 = new());    // shadow MPB
                }
            }

            if (maxLODIndex > 0)
            {
                goTemp = MeshUtility.GetSourceLOD(vegetationModel, LODLevel.LOD1);  // go
                if (goTemp)
                {
                    vegetationMeshLod1 = MeshUtility.GetMeshFromGameObject(goTemp); // mesh
                    mrTemp = goTemp.GetComponentInChildren<MeshRenderer>(); // mesh renderer
                    if (mrTemp)
                    {
                        vegetationMaterialsLOD1 = CreateMaterials(mrTemp, LODLevel.LOD1);  // materials
                        mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockLOD1 = new());   // object MPB
                        mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockShadowsLOD1 = new());    // shadow MPB
                    }
                }
            }

            if (maxLODIndex > 1)
            {
                goTemp = MeshUtility.GetSourceLOD(vegetationModel, LODLevel.LOD2);  // go
                if (goTemp)
                {
                    vegetationMeshLod2 = MeshUtility.GetMeshFromGameObject(goTemp); // mesh
                    mrTemp = goTemp.GetComponentInChildren<MeshRenderer>(); // mesh renderer
                    if (mrTemp)
                    {
                        vegetationMaterialsLOD2 = CreateMaterials(mrTemp, LODLevel.LOD2);  // materials
                        mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockLOD2 = new());   // object MPB
                        mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockShadowsLOD2 = new());    // shadow MPB
                    }
                }
            }

            if (maxLODIndex > 2)
            {
                goTemp = MeshUtility.GetSourceLOD(vegetationModel, LODLevel.LOD3);  // go
                if (goTemp)
                {
                    vegetationMeshLod3 = MeshUtility.GetMeshFromGameObject(goTemp); // mesh
                    mrTemp = goTemp.GetComponentInChildren<MeshRenderer>(); // mesh renderer
                    if (mrTemp)
                    {
                        vegetationMaterialsLOD3 = CreateMaterials(mrTemp, LODLevel.LOD3);  // materials
                        mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockLOD3 = new());   // object MPB
                        mrTemp.GetPropertyBlock(vegetationMaterialPropertyBlockShadowsLOD3 = new());    // shadow MPB
                    }
                }
            }

            if (_vegItemInfoPro.PrefabType == VegetationPrefabType.Texture) // additional post changes for 2D vegetation items
            {
                if (_vegItemInfoPro.VegetationTexture == null)
                    Debug.LogError("VSP internal error log: Textures: The texture of vegetation item: \"" + _vegItemInfoPro.Name + "\" is missing");
                Shader grassShader = ShaderUtility.GetShader_Foliage();
                Setup2DItems(grassShader, vegetationMaterialsLOD0, _vegItemInfoPro.VegetationTexture);
                Setup2DItems(grassShader, vegetationMaterialsLOD1, _vegItemInfoPro.VegetationTexture);
                Setup2DItems(grassShader, vegetationMaterialsLOD2, _vegItemInfoPro.VegetationTexture);
                Setup2DItems(grassShader, vegetationMaterialsLOD3, _vegItemInfoPro.VegetationTexture);
            }
        }

        private Material[] CreateMaterials(MeshRenderer _meshRenderer, LODLevel _lodIndex)
        {
            Material[] materials = new Material[_meshRenderer.sharedMaterials.Length];
            for (int i = 0; i < _meshRenderer.sharedMaterials.Length; i++)
            {
                if (_meshRenderer.sharedMaterials[i])
                {
                    if (vegetationItemInfo.useShaderControllerOverrides)
                    {
                        materials[i] = new(_meshRenderer.sharedMaterials[i]);
                        vegetationSystemPro.renderingMaterials.Add(materials[i], materials[i]);
                    }
                    else
                    {
                        if (vegetationSystemPro.renderingMaterials.ContainsKey(_meshRenderer.sharedMaterials[i]) == false)
                            vegetationSystemPro.renderingMaterials.Add(_meshRenderer.sharedMaterials[i], new(_meshRenderer.sharedMaterials[i]));
                        vegetationSystemPro.renderingMaterials.TryGetValue(_meshRenderer.sharedMaterials[i], out materials[i]);
                    }
                }
                else
                {
                    Debug.LogError("VSP internal error log: Materials: A material of \"" + vegetationModel.name + "\" is missing -- Material index: " + i + " LOD index: " + _lodIndex);
                    materials[i] = new Material(vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.InstancedIndirect ? ShaderUtility.GetShader_Standard() : ShaderUtility.GetShader_EngineDefault());
                }

                RefreshMaterial(materials[i], i, (int)_lodIndex);
            }

            return materials;
        }

        private void Setup2DItems(Shader _shader, Material[] _materials, Texture2D _texture)
        {
            for (int i = 0; i < _materials?.Length; i++)
                if (_materials[i])
                {
                    _materials[i].shader = _shader;
                    _materials[i].SetTexture("_MainTex", _texture);
                }
        }

        public void PrepareRenderLists()    // GPU and billboards
        {
            ReleaseCameraBuffers();
            cameraBillboardMaterialPropertyBlockList.Clear();
            speedTreeWindBridgeMeshRendererList.Clear();

            for (int i = 0; i < vegetationSystemPro.vegetationStudioCameraList.Count; i++)
            {
                if (vegetationItemInfo.VegetationRenderMode == VegetationRenderMode.InstancedIndirect && vegetationSystemPro.vegetationRenderSettings.UseInstancedIndirect())
                    cameraGraphicsBufferList.Add(new CameraGraphicsBuffers(vegetationMeshLod0, vegetationMeshLod1, vegetationMeshLod2, vegetationMeshLod3));    // containers/buffers for the GPU based rendering

                if (vegetationItemInfo.UseBillboards)   // MPBs for passing needed data into the billboard shader
                    cameraBillboardMaterialPropertyBlockList.Add(new MaterialPropertyBlock());

                // "SpeedTree WindBridge"
                if (vegetationSystemPro.vegetationStudioCameraList[i].speedTreeWindBridgeGO == null)
                {   // skip "SceneCamera" and add dummy to keep list in sync
                    speedTreeWindBridgeMeshRendererList.Add(null);
                    continue;   // skip when not prepared / issue somewhere else
                }
                else
                    for (int j = 0; j < shaderControllers?.Length; j++)
                        if (shaderControllers[j] != null && shaderControllers[j].Settings != null && shaderControllers[j].Settings.isSpeedTree)
                        {   // create a clean copy of the "SpeedTree" LOD0 to copy needed data like material and wind data
                            GameObject stwbGO = Object.Instantiate(MeshUtility.GetSourceLOD(vegetationModel, LODLevel.LOD0), vegetationSystemPro.vegetationStudioCameraList[i].speedTreeWindBridgeGO.transform);
                            stwbGO.hideFlags = HideFlags.HideAndDontSave;
                            stwbGO.name = "stwb_" + vegetationItemInfo.Name;
                            stwbGO.GetComponent<MeshFilter>().sharedMesh = new() { bounds = new Bounds(float3.zero, Vector3.one) }; // bounds creation needed for engine "culling"
                            MeshRenderer meshRenderer = stwbGO.GetComponent<MeshRenderer>();    // meshRenderer gets "MPB" assigned when rendering so the engine can write the wind data as usual w/ gameObjects
                            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                            meshRenderer.receiveShadows = false;
                            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                            meshRenderer.allowOcclusionWhenDynamic = false;

                            Component[] components = stwbGO.GetComponents<Component>();
                            for (int k = 0; k < components.Length; k++) // remove all redundant components => only keep needed data for the "WindBridge"
                            {
                                if (components[k] is Transform) continue;
                                if (components[k] is Tree) continue;
                                if (components[k] is MeshRenderer) continue;
                                if (components[k] is MeshFilter) continue;
                                if (Application.isPlaying) Object.Destroy(components[k]);
                                else Object.DestroyImmediate(components[k]);
                            }

                            speedTreeWindBridgeMeshRendererList.Add(meshRenderer);  // add clean meshRenderer to assign is "MPB" later
                        }
            }
        }

        public void CalculateCellCullingBoundsAddy()    // used for cell culling and defining rendering bounds
        {
            cullingBoundsAddy = vegetationItemInfo.Bounds = MeshUtility.CalculateBounds(vegetationModel, maxLODIndex);  // get base bounds => scaled up bounds for cell culling -- for rendering frustum culling

            if (vegetationItemInfo.Bounds == null || vegetationItemInfo.Bounds.extents.magnitude <= 0)
                Debug.LogError("VSP internal error log: Bounds: The bounds of \"" + vegetationItemInfo.Name + "\" have not been generated correctly -- a mesh might be missing");

            float maxScaleRange = math.max(vegetationItemInfo.MinScale, vegetationItemInfo.MaxScale);
            if (vegetationItemInfo.useAdvancedScaleRule)
            {
                maxScaleRange = vegetationItemInfo.MinScale;
                for (int i = 0; i < vegetationItemInfo.scaleRuleCurve.keys.Length; i++)
                    maxScaleRange = math.max(maxScaleRange, vegetationItemInfo.scaleRuleCurve.keys[i].value * vegetationItemInfo.MaxScale);
            }

            float maxScaleNoise = vegetationItemInfo.UseNoiseScaleRule ? math.max(vegetationItemInfo.NoiseScaleMinScale, vegetationItemInfo.NoiseScaleMaxScale) : 1;

            #region unused due to too much approximation / false positives
            //float maxScaleTextureMask = 1;
            //if (vegetationItemInfo.UseTextureMaskScaleRules)
            //    for (int i = 0; i < vegetationItemInfo.TextureMaskScaleRuleList.Count; i++)
            //    {   // min/max limit approximated since tex masks mostly have 255 brightness
            //        float brightness = (1 / vegetationItemInfo.TextureMaskScaleRuleList[i].BrightnessThreshold);
            //        float brightnessLimit = (1 / vegetationItemInfo.TextureMaskScaleRuleList[i].BrightnessThreshold) * vegetationItemInfo.TextureMaskScaleRuleList[i].BrightnessThreshold;
            //        if (!(brightnessLimit >= vegetationItemInfo.TextureMaskScaleRuleList[i].MinBrightness && brightnessLimit <= vegetationItemInfo.TextureMaskScaleRuleList[i].MaxBrightness))
            //            continue;
            //        maxScaleTextureMask = math.max(maxScaleTextureMask, math.clamp(brightness * vegetationItemInfo.TextureMaskScaleRuleList[i].ScaleMultiplier, // "ScaleMultiplier" ineffective for stacked texture masks in a group
            //            vegetationItemInfo.TextureMaskScaleRuleList[i].MinDensity, vegetationItemInfo.TextureMaskScaleRuleList[i].MaxDensity));
            //    }

            //float maxScaleBiomeEdge = vegetationItemInfo.UseBiomeEdgeScaleRule ? math.max(vegetationItemInfo.BiomeEdgeScaleMinScale, vegetationItemInfo.BiomeEdgeScaleMaxScale) : 1;

            //float maxScaleTerrainTexture = 1;
            //if (vegetationItemInfo.UseTerrainTextureScaleRules)
            //    for (int i = 0; i < vegetationItemInfo.TerrainTextureScaleRuleList.Count; i++)
            //    {   // no min/max limit as approximation too ineffective w/ splat map density variation
            //        maxScaleTerrainTexture = math.max(maxScaleTerrainTexture, math.clamp((1 / vegetationItemInfo.TerrainTextureScaleRuleList[i].BrightnessThreshold) * vegetationItemInfo.TerrainTextureScaleRuleList[i].ScaleMultiplier,
            //            vegetationItemInfo.TerrainTextureScaleRuleList[i].MinimumValue, vegetationItemInfo.TerrainTextureScaleRuleList[i].MaximumValue));
            //    }
            #endregion

            cullingBoundsAddy.extents *= vegetationItemInfo.ScaleMultiplier * maxScaleRange * maxScaleNoise;    //* maxScaleTextureMask * maxScaleBiomeEdge * maxScaleTerrainTexture;
            if (vegetationItemInfo.Offset.y > 0)
                cullingBoundsAddy.extents += new Vector3(0, vegetationItemInfo.Offset.y, 0);
            if (vegetationItemInfo.MinUpOffset > 0 || vegetationItemInfo.MaxUpOffset > 0)
                cullingBoundsAddy.extents += new Vector3(0, math.max(vegetationItemInfo.MinUpOffset, vegetationItemInfo.MaxUpOffset), 0);
            vegetationSystemPro.shouldForceUpdateCellCulling = true;
        }

        public void GenerateNativeScaleRuleCurve()  // called in editor for user changes -- called once for each vegetation item at system start
        {
            if (scaleRuleCurveArray.IsCreated == false || scaleRuleCurveArray == null)
                scaleRuleCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
            scaleRuleCurveArray.CopyFrom(vegetationItemInfo.scaleRuleCurve.GenerateCurveArray(4096));
        }

        public void GenerateNativeHeightRuleCurve() // called in editor for user changes -- called once for each vegetation item at system start
        {
            if (heightRuleCurveArray.IsCreated == false || heightRuleCurveArray == null)
                heightRuleCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
            heightRuleCurveArray.CopyFrom(vegetationItemInfo.HeightRuleCurve.GenerateCurveArray(4096));
        }

        public void GenerateNativeSteepnessRuleCurve()  // called in editor for user changes -- called once for each vegetation item at system start
        {
            if (steepnessRuleCurveArray.IsCreated == false || steepnessRuleCurveArray == null)
                steepnessRuleCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
            steepnessRuleCurveArray.CopyFrom(vegetationItemInfo.SteepnessRuleCurve.GenerateCurveArray(4096));
        }

        public void GenerateNativeDistanceFalloffCurve()    // called in editor for user changes -- called once for each vegetation item at system start
        {
            if (distanceFalloffCurveArray.IsCreated == false || distanceFalloffCurveArray == null)
                distanceFalloffCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
            distanceFalloffCurveArray.CopyFrom(vegetationItemInfo.distanceFalloffCurve.GenerateCurveArray(4096));
        }

        public void CreateBillboardMaterial()
        {
            billboardMaterial = new Material(ShaderUtility.GetShader_Billboard());

            if (billboardMaterial == null)
                return;

            billboardMaterial.enableInstancing = true;
#if !USING_URP
            billboardMaterial.EnableKeyword("LOD_FADE_CROSSFADE");  // enable shader internal state -- custom crossfade used -- align w/ (redundant) engine BiRP requirements
#endif
            billboardMaterial.SetTexture("_MainTex", vegetationItemInfo.BillboardTexture);
            billboardMaterial.SetTexture("_BumpMap", vegetationItemInfo.BillboardNormalTexture);
            billboardMaterial.SetFloat("_RowCount", vegetationItemInfo.lastBillboardAtlasTilingRow);
            billboardMaterial.SetFloat("_ColumnCount", vegetationItemInfo.lastBillboardAtlasTilingColumn);

            UpdateBillboardMaterial();
        }

        public void UpdateBillboardMaterial()
        {
            if (billboardMaterial == false)
                return;

            billboardMaterial.SetFloat("_ZShadowBias", vegetationItemInfo.BillboardShadowOffset);
            billboardMaterial.SetFloat("_FarFadeDistance", vegetationItemInfo.BillboardFadeOutDistance);

            billboardMaterial.SetFloat("_AlphaClipping", vegetationItemInfo.BillboardCutoff);
            billboardMaterial.SetFloat("_Brightness", vegetationItemInfo.BillboardBrightness);
            billboardMaterial.SetFloat("_BumpMapScale", vegetationItemInfo.BillboardNormalStrength);
            billboardMaterial.SetFloat("_SpecularPower", vegetationItemInfo.BillboardSpecular);
            billboardMaterial.SetFloat("_OcclusionPower", vegetationItemInfo.BillboardOcclusion);

            billboardMaterial.SetColor("_SnowColor", vegetationSystemPro.environmentSettings.snowColor);
            billboardMaterial.SetFloat("_SnowAmount", vegetationSystemPro.environmentSettings.snowAmount);
            billboardMaterial.SetFloat("_SnowMinimumVariation", vegetationSystemPro.environmentSettings.snowMinimumVariation);
            billboardMaterial.SetFloat("_SnowBlendPower", vegetationSystemPro.environmentSettings.billboardSnowBlendPower);
            billboardMaterial.SetFloat("_SnowMinimumHeight", vegetationSystemPro.environmentSettings.snowMinHeight);
            billboardMaterial.SetFloat("_SnowMinimumHeightVariation", vegetationSystemPro.environmentSettings.snowMinHeightVariation);
            billboardMaterial.SetFloat("_SnowMinimumHeightBlendPower", vegetationSystemPro.environmentSettings.snowMinHeightBlendPower);

            for (int i = 0; i < shaderControllers?.Length; i++) // sync billboard w/ mesh where applicable
                if (shaderControllers[i] != null && shaderControllers[i].MatchBillboardShader(billboardMaterial) && shaderControllers[i].Settings != null)
                {
                    if (shaderControllers[i].Settings.supportsWind)
                    {
                        billboardMaterial.SetFloat("VERTEX_WIND", 1);
                        billboardMaterial.SetFloat("_InitialBend", shaderControllers[i].Settings.ptInitialBend);
                        billboardMaterial.SetFloat("_Stiffness", shaderControllers[i].Settings.ptStiffness);
                        billboardMaterial.SetFloat("_Drag", shaderControllers[i].Settings.ptDrag);
                        billboardMaterial.SetFloat("_ShiverDrag", shaderControllers[i].Settings.ptShiverDrag);
                        billboardMaterial.SetFloat("_ShiverDirectionality", shaderControllers[i].Settings.ptShiverDirectionality);
                    }
                    else
                        billboardMaterial.SetFloat("VERTEX_WIND", 0);

                    if (shaderControllers[i].Settings.supportsSnow)
                        billboardMaterial.SetFloat("BILLBOARD_SNOW", 1);
                    else
                        billboardMaterial.SetFloat("BILLBOARD_SNOW", 0);

                    if (vegetationItemInfo.lastBillboardAtlasColorSource == EBillboardAtlasColorSource.TextureColorBake)
                        return; // skip applying settings when not an "isolatedMaterial" -- only apply needed values for billboards and snow

                    billboardMaterial.SetColor("_HealthyColor", shaderControllers[i].Settings.GetColorPropertyValue("TintColor1"));

                    if (shaderControllers[i].Settings.supportsHueVariation)
                    {
                        billboardMaterial.SetFloat("HUE_VARIATION", 1);
                        billboardMaterial.SetColor("_DryColor", shaderControllers[i].Settings.GetColorPropertyValue("TintColor2"));
                        billboardMaterial.SetTexture("_DryColorNoiseTex", shaderControllers[i].Settings.GetTexturePropertyValue("TintAreaTex"));
                        billboardMaterial.SetFloat("_DryColorNoiseScale", shaderControllers[i].Settings.GetFloatPropertyValue("TintAreaScale"));
                    }
                    else
                        billboardMaterial.SetFloat("HUE_VARIATION", 0);
                }

            if (vegetationSystemPro.vegetationRenderSettings.showLODDebug)
                billboardMaterial.SetColor("_LODDebugColor", GetLODColor(4));
            else
                billboardMaterial.SetColor("_LODDebugColor", Color.white);
        }

        private Color GetLODColor(int _lodIndex)    // lod debug colors
        {
            return _lodIndex switch
            {
                0 => Color.green,
                1 => Color.blue,
                2 => Color.red,
                3 => Color.cyan,
                4 => Color.yellow,  // billboards
                _ => Color.magenta,
            };
        }

        private void RefreshMaterial(Material _material, int _matIndex, int _lodIndex)
        {
            if (_material.shader == null || _material.shader.name == "Hidden/InternalErrorShader")
            {
                Debug.LogError("VSP internal error log: Shaders: The shader of material \"" + _material.name + "\" is missing -- Material index: " + _matIndex + " LOD index: " + _lodIndex);
                return;
            }

            _material.enableInstancing = true;  // enfore needed instancing support

#if USING_URP
            if (vegetationItemInfo.VegetationRenderMode != VegetationRenderMode.Normal)
#endif
            _material.EnableKeyword("LOD_FADE_CROSSFADE");  // enforce (needed) crossfade support => the system always writes "correct" values in the "LOD-Culling" logic to avoid incorrect states

            if (_material.HasProperty("_CullFarStart"))
                _material.SetFloat("_CullFarStart", 100000);    // safety set for certain shaders -- left here for compatibility => else set through a "shader controller"

            if (_material.HasProperty("_LODDebugColor") && vegetationItemInfo.useShaderControllerOverrides)
                if (vegetationSystemPro.vegetationRenderSettings.showLODDebug)
                    _material.SetColor("_LODDebugColor", GetLODColor(_lodIndex));   // simple color pass for  LOD debugging and visualization
                else
                    _material.SetColor("_LODDebugColor", Color.white);

            if (_matIndex < shaderControllers?.Length)
                shaderControllers[_matIndex]?.UpdateMaterial(_material, vegetationSystemPro.environmentSettings, vegetationItemInfo);   // update shader controller material
        }

        public void RefreshMaterials()
        {
            for (int i = 0; i < vegetationMaterialsLOD0?.Length; i++)
                RefreshMaterial(vegetationMaterialsLOD0[i], i, 0);

            for (int i = 0; i < vegetationMaterialsLOD1?.Length; i++)
                RefreshMaterial(vegetationMaterialsLOD1[i], i, 1);

            for (int i = 0; i < vegetationMaterialsLOD2?.Length; i++)
                RefreshMaterial(vegetationMaterialsLOD2[i], i, 2);

            for (int i = 0; i < vegetationMaterialsLOD3?.Length; i++)
                RefreshMaterial(vegetationMaterialsLOD3[i], i, 3);

            if (vegetationItemInfo.UseBillboards)
                UpdateBillboardMaterial();
        }

        public Mesh GetMeshAtIndex(int _lodIndex)   // for late stage rendering setup
        {
            return _lodIndex switch
            {
                0 => vegetationMeshLod0,
                1 => vegetationMeshLod1,
                2 => vegetationMeshLod2,
                3 => vegetationMeshLod3,
                _ => null,
            };
        }

        public Material[] GetMaterialsAtIndex(int _lodIndex)    // for late stage rendering setup
        {
            return _lodIndex switch
            {
                0 => vegetationMaterialsLOD0,
                1 => vegetationMaterialsLOD1,
                2 => vegetationMaterialsLOD2,
                3 => vegetationMaterialsLOD3,
                _ => null,
            };
        }

        public MaterialPropertyBlock GetMPBAtIndex(int _lodIndex)   // for late stage rendering setup
        {
            return _lodIndex switch
            {
                0 => vegetationMaterialPropertyBlockLOD0,
                1 => vegetationMaterialPropertyBlockLOD1,
                2 => vegetationMaterialPropertyBlockLOD2,
                3 => vegetationMaterialPropertyBlockLOD3,
                _ => null,
            };
        }

        public void ClearMaterials()    // existing materials for the shader controllers get disposed through GC
        {
            for (int i = 0; i < vegetationMaterialsLOD0?.Length; i++)
                vegetationMaterialsLOD0[i] = null;

            for (int i = 0; i < vegetationMaterialsLOD1?.Length; i++)
                vegetationMaterialsLOD1[i] = null;

            for (int i = 0; i < vegetationMaterialsLOD2?.Length; i++)
                vegetationMaterialsLOD2[i] = null;

            for (int i = 0; i < vegetationMaterialsLOD3?.Length; i++)
                vegetationMaterialsLOD3[i] = null;
        }

        private static void DestroyMaterials(Material[] _materials)
        {
            for (int i = 0; i < _materials?.Length; i++)
                if (Application.isPlaying)
                    Object.Destroy(_materials[i]);
                else
                    Object.DestroyImmediate(_materials[i]);
        }

        private void ReleaseCameraBuffers()
        {
            for (int i = 0; i < cameraGraphicsBufferList.Count; i++)
                cameraGraphicsBufferList[i].ReleaseGraphicsBuffers();
            cameraGraphicsBufferList.Clear();
        }

        public void Dispose()
        {
            if (vegetationItemInfo.useShaderControllerOverrides)    // whether the item model info has been created -- it uses shaderController overrides -- it has a valid shaderController setup
            {
                DestroyMaterials(vegetationMaterialsLOD0);
                DestroyMaterials(vegetationMaterialsLOD1);
                DestroyMaterials(vegetationMaterialsLOD2);
                DestroyMaterials(vegetationMaterialsLOD3);
            }

            if (Application.isPlaying)
                Object.Destroy(billboardMaterial);
            else
                Object.DestroyImmediate(billboardMaterial);

            ReleaseCameraBuffers(); // release != dispose -- only release for runtime refresh -- indirect dispose using GC and "= null"

            for (int i = 0; i < speedTreeWindBridgeMeshRendererList.Count; i++)
            {
                if (speedTreeWindBridgeMeshRendererList[i] == null) continue;
                if (Application.isPlaying)
                    Object.Destroy(speedTreeWindBridgeMeshRendererList[i].gameObject);
                else
                    Object.DestroyImmediate(speedTreeWindBridgeMeshRendererList[i].gameObject);
            }
            speedTreeWindBridgeMeshRendererList.Clear();

            if (scaleRuleCurveArray.IsCreated)
                scaleRuleCurveArray.Dispose();

            if (heightRuleCurveArray.IsCreated)
                heightRuleCurveArray.Dispose();

            if (steepnessRuleCurveArray.IsCreated)
                steepnessRuleCurveArray.Dispose();

            if (distanceFalloffCurveArray.IsCreated)
                distanceFalloffCurveArray.Dispose();
        }
    }
}