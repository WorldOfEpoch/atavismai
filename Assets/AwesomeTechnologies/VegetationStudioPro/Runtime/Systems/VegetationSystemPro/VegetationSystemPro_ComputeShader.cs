using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.Utility.Culling;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void SetupComputeShaderIDs()
        {
            if (vegetationRenderSettings.UseInstancedIndirect() == false)
                return;

            dummyGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, 144);    // size in bytes of the "IndirectShaderData" struct -- specific stride for the instanceData
            argsBufferDispatch = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 4, sizeof(uint));  // dynamically set later -- thread group count for the "DispatchIndirect()" call which "runs" the compute shader

            frustumMatrixShader = ShaderUtility.GetComputeShader_GPUFrustumCullingLODJob();
            frustumKernelHandle = frustumMatrixShader.FindKernel("GPUFrustumCullingLODJob");

            _mergeBufferID = Shader.PropertyToID("MergeBuffer");
            mergeBufferShader = ShaderUtility.GetComputeShader_MergeInstancedIndirectBuffers();
            mergeBufferKernelHandle = mergeBufferShader.FindKernel("MergeInstancedIndirectBuffers");

            _mergeSourceBuffer0ID = Shader.PropertyToID("MergeSourceBuffer0");
            _mergeSourceBuffer1ID = Shader.PropertyToID("MergeSourceBuffer1");
            _mergeSourceBuffer2ID = Shader.PropertyToID("MergeSourceBuffer2");
            _mergeSourceBuffer3ID = Shader.PropertyToID("MergeSourceBuffer3");
            _mergeSourceBuffer4ID = Shader.PropertyToID("MergeSourceBuffer4");
            _mergeSourceBuffer5ID = Shader.PropertyToID("MergeSourceBuffer5");
            _mergeSourceBuffer6ID = Shader.PropertyToID("MergeSourceBuffer6");
            _mergeSourceBuffer7ID = Shader.PropertyToID("MergeSourceBuffer7");
            _mergeSourceBuffer8ID = Shader.PropertyToID("MergeSourceBuffer8");
            _mergeSourceBuffer9ID = Shader.PropertyToID("MergeSourceBuffer9");
            _mergeSourceBuffer10ID = Shader.PropertyToID("MergeSourceBuffer10");
            _mergeSourceBuffer11ID = Shader.PropertyToID("MergeSourceBuffer11");
            _mergeSourceBuffer12ID = Shader.PropertyToID("MergeSourceBuffer12");
            _mergeSourceBuffer13ID = Shader.PropertyToID("MergeSourceBuffer13");
            _mergeSourceBuffer14ID = Shader.PropertyToID("MergeSourceBuffer14");
            _mergeSourceBuffer15ID = Shader.PropertyToID("MergeSourceBuffer15");

            _mergeInstanceCount0ID = Shader.PropertyToID("MergeSourceBufferCount0");
            _mergeInstanceCount1ID = Shader.PropertyToID("MergeSourceBufferCount1");
            _mergeInstanceCount2ID = Shader.PropertyToID("MergeSourceBufferCount2");
            _mergeInstanceCount3ID = Shader.PropertyToID("MergeSourceBufferCount3");
            _mergeInstanceCount4ID = Shader.PropertyToID("MergeSourceBufferCount4");
            _mergeInstanceCount5ID = Shader.PropertyToID("MergeSourceBufferCount5");
            _mergeInstanceCount6ID = Shader.PropertyToID("MergeSourceBufferCount6");
            _mergeInstanceCount7ID = Shader.PropertyToID("MergeSourceBufferCount7");
            _mergeInstanceCount8ID = Shader.PropertyToID("MergeSourceBufferCount8");
            _mergeInstanceCount9ID = Shader.PropertyToID("MergeSourceBufferCount9");
            _mergeInstanceCount10ID = Shader.PropertyToID("MergeSourceBufferCount10");
            _mergeInstanceCount11ID = Shader.PropertyToID("MergeSourceBufferCount11");
            _mergeInstanceCount12ID = Shader.PropertyToID("MergeSourceBufferCount12");
            _mergeInstanceCount13ID = Shader.PropertyToID("MergeSourceBufferCount13");
            _mergeInstanceCount14ID = Shader.PropertyToID("MergeSourceBufferCount14");
            _mergeInstanceCount15ID = Shader.PropertyToID("MergeSourceBufferCount15");

            _cameraFrustumPlane0 = Shader.PropertyToID("FrustumPlane0");
            _cameraFrustumPlane1 = Shader.PropertyToID("FrustumPlane1");
            _cameraFrustumPlane2 = Shader.PropertyToID("FrustumPlane2");
            _cameraFrustumPlane3 = Shader.PropertyToID("FrustumPlane3");
            _cameraFrustumPlane4 = Shader.PropertyToID("FrustumPlane4");
            _cameraFrustumPlane5 = Shader.PropertyToID("FrustumPlane5");
            _worldSpaceCameraPos = Shader.PropertyToID("WorldSpaceCameraPos");

            _instanceCountID = Shader.PropertyToID("_InstanceCount");
            _sourceBufferID = Shader.PropertyToID("SourceShaderDataBuffer");

            _objectBufferLod0ID = Shader.PropertyToID("ObjectBufferLOD0");
            _objectBufferLod1ID = Shader.PropertyToID("ObjectBufferLOD1");
            _objectBufferLod2ID = Shader.PropertyToID("ObjectBufferLOD2");
            _objectBufferLod3ID = Shader.PropertyToID("ObjectBufferLOD3");

            _shadowBufferLod0ID = Shader.PropertyToID("ShadowBufferLOD0");
            _shadowBufferLod1ID = Shader.PropertyToID("ShadowBufferLOD1");
            _shadowBufferLod2ID = Shader.PropertyToID("ShadowBufferLOD2");
            _shadowBufferLod3ID = Shader.PropertyToID("ShadowBufferLOD3");

            _cullDistance = Shader.PropertyToID("_CullDistance");
            _floatingOriginOffsetID = Shader.PropertyToID("_FloatingOriginOffset");

            _noFrustumCullingID = Shader.PropertyToID("_NoFrustumCulling");
            _hasBackShadowID = Shader.PropertyToID("_HasBackShadow");
            _lightDirection = Shader.PropertyToID("_LightDirection");

            _itemBoundsCenter = Shader.PropertyToID("_itemBoundsCenter");
            _itemBoundsExtents = Shader.PropertyToID("_itemBoundsExtents");

            _useLodFade = Shader.PropertyToID("_UseLODFade");
            _lodCount = Shader.PropertyToID("_LODCount");
            _maxLodIndex = Shader.PropertyToID("_maxLodIndex");
            _maxLOD0 = Shader.PropertyToID("_maxLOD0");
            _maxLOD1 = Shader.PropertyToID("_maxLOD1");
            _maxLOD2 = Shader.PropertyToID("_maxLOD2");
            _maxLOD3 = Shader.PropertyToID("_maxLOD3");
            _shadowLODIndexID = Shader.PropertyToID("_shadowLODIndex");
            _customShadowLODIndex = Shader.PropertyToID("_customShadowLODIndex");
            _lodFactor = Shader.PropertyToID("_LODFactor");
            _lodBias = Shader.PropertyToID("_LODBias");
            _lodFadeDistance = Shader.PropertyToID("_LODFadeDistance");

            _lod0To1Distance = Shader.PropertyToID("_lod0To1Distance");
            _lod1To2Distance = Shader.PropertyToID("_lod1To2Distance");
            _lod2To3Distance = Shader.PropertyToID("_lod2To3Distance");

            _objectShaderDataBufferID = Shader.PropertyToID("VisibleShaderDataBuffer");
            _indirectShaderDataBufferID = Shader.PropertyToID("IndirectShaderDataBuffer");  // only needed for old indirect integrations
        }

        private void PrepareGraphicsBuffers()
        {
            //if (vegetationRenderSettings.UseInstancedIndirect() == false)
            //    return;

            Profiler.BeginSample("Setup instanced indirect graphics buffers");
            for (int i = 0; i < instancedIndirectCellList.Count; i++)   // create graphics buffers to store matrix data, per cell containing instanced indirect vegetation instances
                for (int j = 0; j < instancedIndirectCellList[i].vegetationPackageInstanceList.Count; j++)
                    for (int k = 0; k < instancedIndirectCellList[i].vegetationPackageInstanceList[j].matrixInstanceList.Count; k++)
                    {
                        if (vegetationPackageProModelsList[j].vegetationItemModelList[k].vegetationItemInfo.VegetationRenderMode != VegetationRenderMode.InstancedIndirect)
                            continue;   // skip non indirect vegetation items

                        if (vegetationPackageProModelsList[j].vegetationItemModelList[k].distanceBand < instancedIndirectCellList[i].loadedDistanceBand)
                            continue;   // skip if distance band not matching

                        if ((instancedIndirectCellList[i].vegetationPackageInstanceList[j].graphicsBufferList[k] != null && instancedIndirectCellList[i].vegetationPackageInstanceList[j].graphicsBufferList[k].IsValid())
                            || instancedIndirectCellList[i].vegetationPackageInstanceList[j].matrixInstanceList[k].Length <= 0)
                            continue;   // skip if already created -- skip if no instances exist of the vegetation item in this cell

                        // create graphics buffer for the current vegetationItem of the current cell -- convert matrix instances of all vegetation instances for the current vegetationItem of the current cell
                        instancedIndirectCellList[i].vegetationPackageInstanceList[j].graphicsBufferList[k] = new(GraphicsBuffer.Target.Append, instancedIndirectCellList[i].vegetationPackageInstanceList[j].matrixInstanceList[k].Length, 80);    // 80 = size in bytes of the "MatrixInstance" struct
                        instancedIndirectCellList[i].vegetationPackageInstanceList[j].graphicsBufferList[k].SetData(instancedIndirectCellList[i].vegetationPackageInstanceList[j].matrixInstanceList[k].AsArray());
                    }

            instancedIndirectCellList.Clear();  // clear as done copying CPU matrix data into GPU buffers -- to stop looping
            Profiler.EndSample();
        }

        private void SetGPUFrustumPlanes(CellCullingGroup _cullingGroup)
        {
            //if (vegetationRenderSettings.UseInstancedIndirect() == false) return;
            frustumMatrixShader.SetVector(_cameraFrustumPlane0, new float4(_cullingGroup.frustumPlanes[0].normal, _cullingGroup.frustumPlanes[0].distance));
            frustumMatrixShader.SetVector(_cameraFrustumPlane1, new float4(_cullingGroup.frustumPlanes[1].normal, _cullingGroup.frustumPlanes[1].distance));
            frustumMatrixShader.SetVector(_cameraFrustumPlane2, new float4(_cullingGroup.frustumPlanes[2].normal, _cullingGroup.frustumPlanes[2].distance));
            frustumMatrixShader.SetVector(_cameraFrustumPlane3, new float4(_cullingGroup.frustumPlanes[3].normal, _cullingGroup.frustumPlanes[3].distance));
            frustumMatrixShader.SetVector(_cameraFrustumPlane4, new float4(_cullingGroup.frustumPlanes[4].normal, _cullingGroup.frustumPlanes[4].distance));
            frustumMatrixShader.SetVector(_cameraFrustumPlane5, new float4(_cullingGroup.frustumPlanes[5].normal, _cullingGroup.frustumPlanes[5].distance));
            frustumMatrixShader.SetVector(_worldSpaceCameraPos, new float4(_cullingGroup.targetCamera.transform.position, 1));
        }

        private void SetGraphicsBuffer(int _bufferID, int _bufferCountID, int _cellIndex, int _vegPackageIndex, int _vegItemIndex)
        {
            if (_cellIndex >= computeShaderCellList.Count)  // when index out of sync / higher than existing valid vegetation cell count
            {
                mergeBufferShader.SetBuffer(mergeBufferKernelHandle, _bufferID, dummyGraphicsBuffer);   // set dummy buffer
                mergeBufferShader.SetInt(_bufferCountID, 0);    // null out instance count
                return;
            }

            mergeBufferShader.SetBuffer(mergeBufferKernelHandle, _bufferID, computeShaderCellList[_cellIndex].vegetationPackageInstanceList[_vegPackageIndex].graphicsBufferList[_vegItemIndex]);   // set graphics buffer
            mergeBufferShader.SetInt(_bufferCountID, computeShaderCellList[_cellIndex].vegetationPackageInstanceList[_vegPackageIndex].graphicsBufferList[_vegItemIndex].count);    // set instance count
        }

        private void PrepareGPURenderList(VegetationItemModelInfo _vegItemModelInfo, int _cameraIndex, int _packageIndex, int _itemIndex, int _shadowLodIndex)
        {
            Profiler.BeginSample("Prepare render list -- GPU");
            computeShaderCellList.Clear();
            // visible cell vs item band filter -- preparation/filter for render list
            for (int i = 0; i < vegetationStudioCameraList[_cameraIndex].vegetationCullingGroup.visibleCellIndexList.Length; i++)
            {
                VegetationCell vegetationCell = vegetationStudioCameraList[_cameraIndex].preloadVegetationCellList[vegetationStudioCameraList[_cameraIndex].vegetationCullingGroup.visibleCellIndexList[i]];
                CellCullingInfo cellCullingInfo = vegetationStudioCameraList[_cameraIndex].GetCellCullingInfo(vegetationStudioCameraList[_cameraIndex].vegetationCullingGroup.visibleCellIndexList[i]);
                if (cellCullingInfo.CurrentDistanceBand > _vegItemModelInfo.distanceBand)   // vegItemModelInfo!!
                    continue;   // only merge same cell distance band types

                if (vegetationCell.vegetationPackageInstanceList[_packageIndex].graphicsBufferList[_itemIndex] == null || vegetationCell.vegetationPackageInstanceList[_packageIndex].graphicsBufferList[_itemIndex].IsValid() == false)
                    continue;   // skip if the vegetation cell is not meant for instanced indirect instances/items

                computeShaderCellList.Add(vegetationCell);  // add valid cells
            }

            if (computeShaderCellList.Count == 0)
            {
                Profiler.EndSample();
                return; // return when no valid vegetation cells exist -- when the cell does not contain indirect instances
            }

            int totalInstanceCount = 0;
            for (int i = 0; i < computeShaderCellList.Count; i++)   // for each visible vegetation cell containing instanced indirect vegetation -- get total instance count to create render list
                totalInstanceCount += computeShaderCellList[i].vegetationPackageInstanceList[_packageIndex].graphicsBufferList[_itemIndex].count;

            if (totalInstanceCount == 0)
            {
                Profiler.EndSample();
                return; // return when total instance count is null
            }

            Profiler.BeginSample("Update buffer sizes");    // update/prepare buffer size + fixed padding offset
            if (vegetationRenderSettings.enableSinglePassInstancedVR)
            {
                if (totalInstanceCount * 2 > _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].mergeBuffer.count)
                    _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].UpdateGraphicsBufferSize((totalInstanceCount + (int)(totalInstanceCount * 0.001f)) * 2);
            }
            else
            {
                if (totalInstanceCount > _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].mergeBuffer.count)
                    _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].UpdateGraphicsBufferSize(totalInstanceCount + (int)(totalInstanceCount * 0.001f));
            }
            Profiler.EndSample();

            Profiler.BeginSample("Merge buffers/matrix data");  // merge buffer data
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].mergeBuffer.SetCounterValue(0);
            mergeBufferShader.SetBuffer(mergeBufferKernelHandle, _mergeBufferID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].mergeBuffer);

            int buffercount = 16;   // use chunks of 16 cells at once
            for (int i = 0; i < computeShaderCellList.Count; i += buffercount)  // merge matrix instance data of all vegetation instances to create a total render list
            {
                int instanceCountMerge = computeShaderCellList[i].vegetationPackageInstanceList[_packageIndex].graphicsBufferList[_itemIndex].count;    // get initial instance count using the first cell
                for (int j = 1; j < buffercount; j++)   // compare all cells of the current chunk to each other -- update the instance count to use the highest count
                    if (i + j < computeShaderCellList.Count)    // safety check for the last (partial) chunk
                    {
                        int tempInstanceCount = computeShaderCellList[i + j].vegetationPackageInstanceList[_packageIndex].graphicsBufferList[_itemIndex].count; // get the instance count of the next cell/-s for comparison
                        if (tempInstanceCount > instanceCountMerge)
                            instanceCountMerge = tempInstanceCount; // update the instance count since the this cell has a higher instance count than the previous once (and thus all before this one)
                    }   // => highest instance count reached of all cells in this chunk

                argsDispatch[0] = (uint)math.ceil(instanceCountMerge / 32f);    // split instance count to use for the separate threads -- "f" to enforce float division
                if (argsDispatch[0] == 0)
                    continue;

                // pass matrix lists as a chunk of 16 cells and merge them
                SetGraphicsBuffer(_mergeSourceBuffer0ID, _mergeInstanceCount0ID, i, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer1ID, _mergeInstanceCount1ID, i + 1, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer2ID, _mergeInstanceCount2ID, i + 2, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer3ID, _mergeInstanceCount3ID, i + 3, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer4ID, _mergeInstanceCount4ID, i + 4, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer5ID, _mergeInstanceCount5ID, i + 5, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer6ID, _mergeInstanceCount6ID, i + 6, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer7ID, _mergeInstanceCount7ID, i + 7, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer8ID, _mergeInstanceCount8ID, i + 8, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer9ID, _mergeInstanceCount9ID, i + 9, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer10ID, _mergeInstanceCount10ID, i + 10, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer11ID, _mergeInstanceCount11ID, i + 11, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer12ID, _mergeInstanceCount12ID, i + 12, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer13ID, _mergeInstanceCount13ID, i + 13, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer14ID, _mergeInstanceCount14ID, i + 14, _packageIndex, _itemIndex);
                SetGraphicsBuffer(_mergeSourceBuffer15ID, _mergeInstanceCount15ID, i + 15, _packageIndex, _itemIndex);

                argsBufferDispatch.SetData(argsDispatch);
                mergeBufferShader.DispatchIndirect(mergeBufferKernelHandle, argsBufferDispatch);    // run merge
            }
            Profiler.EndSample();

            ///

            ///

            // reset LOD buffer counters
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD0?.SetCounterValue(0);
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD1?.SetCounterValue(0);
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD2?.SetCounterValue(0);
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD3?.SetCounterValue(0);

            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD0?.SetCounterValue(0);
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD1?.SetCounterValue(0);
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD2?.SetCounterValue(0);
            _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD3?.SetCounterValue(0);

            // frustum culling / lod calculations
            frustumMatrixShader.SetInt(_instanceCountID, totalInstanceCount);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _sourceBufferID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].mergeBuffer); // pass merged matrix instance data of all vegetation instances

            frustumMatrixShader.SetBuffer(frustumKernelHandle, _objectBufferLod0ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD0);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _objectBufferLod1ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD1 ?? dummyGraphicsBuffer);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _objectBufferLod2ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD2 ?? dummyGraphicsBuffer);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _objectBufferLod3ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].objectBufferLOD3 ?? dummyGraphicsBuffer);

            frustumMatrixShader.SetBuffer(frustumKernelHandle, _shadowBufferLod0ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD0);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _shadowBufferLod1ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD1 ?? dummyGraphicsBuffer);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _shadowBufferLod2ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD2 ?? dummyGraphicsBuffer);
            frustumMatrixShader.SetBuffer(frustumKernelHandle, _shadowBufferLod3ID, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex].shadowBufferLOD3 ?? dummyGraphicsBuffer);

            frustumMatrixShader.SetFloat(_cullDistance, vegItemDistances.y);

            frustumMatrixShader.SetBool(_hasBackShadowID, _vegItemModelInfo.distanceBand == 1); // only largeObjects/trees use shadow frustum culling

            frustumMatrixShader.SetVector(_itemBoundsCenter, _vegItemModelInfo.vegetationItemInfo.Bounds.center);
            frustumMatrixShader.SetVector(_itemBoundsExtents, _vegItemModelInfo.vegetationItemInfo.Bounds.extents);

            frustumMatrixShader.SetBool(_useLodFade, _vegItemModelInfo.vegetationItemInfo.EnableCrossFade && QualitySettings.enableLODCrossFade);
            frustumMatrixShader.SetInt(_lodCount, _vegItemModelInfo.lodCount);
            frustumMatrixShader.SetInt(_maxLodIndex, _vegItemModelInfo.maxLODIndex);
            frustumMatrixShader.SetInt(_maxLOD0, _vegItemModelInfo.maxLOD0Index);
            frustumMatrixShader.SetInt(_maxLOD1, _vegItemModelInfo.maxLOD1Index);
            frustumMatrixShader.SetInt(_maxLOD2, _vegItemModelInfo.maxLOD2Index);
            frustumMatrixShader.SetInt(_maxLOD3, _vegItemModelInfo.maxLOD3Index);
            frustumMatrixShader.SetInt(_shadowLODIndexID, _shadowLodIndex = _vegItemModelInfo.vegetationItemInfo.DisableShadows || !hasSunMoon ? -1 : _shadowLodIndex); // -1 = disabled
            frustumMatrixShader.SetInt(_customShadowLODIndex, math.clamp(_vegItemModelInfo.vegetationSystemPro.vegetationRenderSettings.GetCustomShadowLODIndex(_vegItemModelInfo.vegetationItemInfo.VegetationType), 0, _vegItemModelInfo.maxLODIndex));
            frustumMatrixShader.SetFloat(_lodFactor, _vegItemModelInfo.vegetationItemInfo.LODFactor);
            frustumMatrixShader.SetFloat(_lodFadeDistance, _vegItemModelInfo.vegetationItemInfo.EnableCrossFade && QualitySettings.enableLODCrossFade ? vegetationSettings.crossFadeDistance : 0);

            frustumMatrixShader.SetFloat(_lod0To1Distance, _vegItemModelInfo.lod0To1Distance);
            frustumMatrixShader.SetFloat(_lod1To2Distance, _vegItemModelInfo.lod1To2Distance);
            frustumMatrixShader.SetFloat(_lod2To3Distance, _vegItemModelInfo.lod2To3Distance);

            argsDispatch[0] = (uint)math.ceil(totalInstanceCount / 32f);    // split instance count to use for the separate threads -- "f" to enforce float division
            argsBufferDispatch.SetData(argsDispatch);
            frustumMatrixShader.DispatchIndirect(frustumKernelHandle, argsBufferDispatch);  // run GPU frustum culling / lod caclulations

            if (vegetationRenderSettings.enableSinglePassInstancedVR)
                frustumMatrixShader.DispatchIndirect(frustumKernelHandle, argsBufferDispatch);  // run GPU frustum culling / lod caclulations
            Profiler.EndSample();

            // render vegetation after merge > cull/lod job -- directly called in here since this depends on the "Dispatch" async completion/-s
            RenderGPUVegetation(_vegItemModelInfo, _cameraIndex, _vegItemModelInfo.cameraGraphicsBufferList[_cameraIndex], _shadowLodIndex);
        }

        private void RenderGPUVegetation(VegetationItemModelInfo _vegItemModelInfo, int _cameraIndex, CameraGraphicsBuffers _cameraGraphicsBuffers, int _shadowLodIndex)
        {
            // render visible non shadow objects that are only within the view frustum (by default)
            _cameraGraphicsBuffers.CopyInstanceCounts(false);
            RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD0Index, _cameraIndex, _cameraGraphicsBuffers);
            if (_vegItemModelInfo.lodCount > 1) // has 2 LOD levels
                RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD1Index, _cameraIndex, _cameraGraphicsBuffers);
            if (_vegItemModelInfo.lodCount > 2) // has 3 LOD levels
                RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD2Index, _cameraIndex, _cameraGraphicsBuffers);
            if (_vegItemModelInfo.lodCount > 3) // has 4 LOD levels
                RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD3Index, _cameraIndex, _cameraGraphicsBuffers);

            // since we are doing (separate) shadow culling we render them separately to not draw shadows twice
            if (_shadowLodIndex == -1 || _vegItemModelInfo.vegetationItemInfo.DisableShadows) return;   // skip when not supposed to render shadows
            renderParams.shadowCastingMode = ShadowCastingMode.ShadowsOnly; // set to render shadows only

            _cameraGraphicsBuffers.CopyInstanceCounts(true);
            RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD0Index, _cameraIndex, _cameraGraphicsBuffers);
            if (_vegItemModelInfo.lodCount < 2) return; // has not 2 LOD levels
            RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD1Index, _cameraIndex, _cameraGraphicsBuffers);
            if (_vegItemModelInfo.lodCount < 3) return; // has not 3 LOD levels
            RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD2Index, _cameraIndex, _cameraGraphicsBuffers);
            if (_vegItemModelInfo.lodCount < 4) return; // has not 4 LOD levels
            RenderVegetationItemIndirect(_vegItemModelInfo, _vegItemModelInfo.maxLOD3Index, _cameraIndex, _cameraGraphicsBuffers);
        }

        private void RenderVegetationItemIndirect(VegetationItemModelInfo _vegItemModelInfo, int _lodIndex, int _cameraIndex, CameraGraphicsBuffers _cameraGraphicsBuffers)
        {
            // get buffers
            bool isShadow = renderParams.shadowCastingMode == ShadowCastingMode.ShadowsOnly;
            GraphicsBuffer objectBuffer = _cameraGraphicsBuffers.GetIndirectBufferAtIndex(_lodIndex, isShadow);
            List<GraphicsBuffer> argsBuffers = _cameraGraphicsBuffers.GetArgsBufferAtIndex(_lodIndex, isShadow);

            // get/set base data needed for rendering
            Mesh mesh = _vegItemModelInfo.GetMeshAtIndex(_lodIndex);
            Material[] materials = _vegItemModelInfo.GetMaterialsAtIndex(_lodIndex);
            MaterialPropertyBlock materialPropertyBlock = _vegItemModelInfo.GetMPBAtIndex(_lodIndex);
            materialPropertyBlock.Clear();

            for (int i = 0; i < _vegItemModelInfo.shaderControllers?.Length; i++)   // "SpeedTree WindBridge"
                if (_vegItemModelInfo.shaderControllers[i] != null && _vegItemModelInfo.shaderControllers[i].Settings != null && _vegItemModelInfo.shaderControllers[i].Settings.isSpeedTree)
                {
                    MeshRenderer meshRenderer = _vegItemModelInfo.speedTreeWindBridgeMeshRendererList[_cameraIndex];
                    if (meshRenderer) meshRenderer.GetPropertyBlock(materialPropertyBlock); // pass this vegetation item's "MPB" to the engine so it can write the usual "SpeedTree" wind data
                }

            // updater render params to match current vegetationItem / LOD level
            renderParams.matProps = materialPropertyBlock;
            renderParams.worldBounds = CalculateWorldBounds(_vegItemModelInfo, _lodIndex);

            // pass data to the indirect shader data
            materialPropertyBlock.SetBuffer(_objectShaderDataBufferID, objectBuffer);
            materialPropertyBlock.SetBuffer(_indirectShaderDataBufferID, objectBuffer); // only needed for old indirect integrations

            for (int i = 0; i < math.min(mesh.subMeshCount, materials.Length); i++) // for each sub mesh w/ safety filter based on materials present
            {
                renderParams.material = materials[i];
                Graphics.RenderMeshIndirect(renderParams, mesh, argsBuffers[i]);
            }
        }

        private void DisposeGraphicsBuffers()   // only disposing dummy/util here as others get disposed with their cells / items
        {
            dummyGraphicsBuffer?.Dispose();
            argsBufferDispatch?.Release();
        }
    }
}