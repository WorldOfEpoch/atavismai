using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationStudio;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Jobs;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

namespace AwesomeTechnologies.TerrainSystem
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Systems/TerrainSystemPro", 1)]
    public partial class TerrainSystemPro : MonoBehaviour
    {
        public VegetationSystemPro vegetationSystemPro;
        public bool enableAutoSplatMapGeneration = false;

#if UNITY_EDITOR
        public int currentTabIndex; // currently select tab
        public int vegetationPackageIndex;  // currently selected vegetation package
        public int vegetationPackageTextureIndex;   // currently selected texture
        public int lastVegetationPackageTextureIndex;   // previously selected texture

        public bool enableHeatmap;

        public bool showCurvesMenu = true;
        public bool showNoiseMenu = true;
        public bool showConcaveConvexMenu = false;
#endif

        void Reset()
        {
            vegetationSystemPro = GetComponent<VegetationSystemPro>();
        }

        public void GetSplatPrototypes(VegetationPackagePro _vegetationPackage) // read terrain textures from a terrain into a vegetation package
        {
            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++) // for all "UnityTerrain" terrains in the terrain list
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = vegetationSystemPro.vegetationStudioTerrainList[i];
                if (iVegetationStudioTerrain is UnityTerrain)   // check whether it is an "UnityTerrain" -- whether it can have terrain textures
                {
                    TerrainLayer[] terrainLayers = iVegetationStudioTerrain.GetTerrainLayers();
                    for (int j = 0; j < _vegetationPackage.TerrainTextureList.Count; j++)   // for all terrain textures in the vegetation package
                        if (j < terrainLayers.Length)   // limit vegetation package index to how many layers the current terrain has
                        {   // write terrain textures into vegetation package
                            _vegetationPackage.TerrainTextureList[j].Texture = terrainLayers[j].diffuseTexture;
                            _vegetationPackage.TerrainTextureList[j].TextureNormals = terrainLayers[j].normalMapTexture;
                            _vegetationPackage.TerrainTextureList[j].Offset = terrainLayers[j].tileOffset;
                            _vegetationPackage.TerrainTextureList[j].TileSize = terrainLayers[j].tileSize;
                        }
                    break;  // only get the textures from the first found "UnityTerrain"
                }
            }
        }

        public void SetSplatPrototypes(VegetationPackagePro _vegetationPackage)  // write terrain textures from a vegetation package into a terrain
        {
            TerrainLayer[] terrainLayers = new TerrainLayer[_vegetationPackage.TerrainTextureList.Count];
            for (int i = 0; i < _vegetationPackage.TerrainTextureList.Count; i++)   // for all terrain textures in the vegetation package
            {
                TerrainTextureInfo terrainTextureInfo = _vegetationPackage.TerrainTextureList[i];
                TerrainLayer terrainLayer = terrainTextureInfo.TerrainLayer;

                if (terrainLayer == null)   // if the texture of the vegetation package doens't have a on-disk terrain layer
                {
                    terrainLayer = new TerrainLayer // create a new terrain layer and assign needed values
                    {
                        diffuseTexture = terrainTextureInfo.Texture,
                        normalMapTexture = terrainTextureInfo.TextureNormals,
                        tileSize = terrainTextureInfo.TileSize,
                        tileOffset = terrainTextureInfo.Offset
                    };
#if UNITY_EDITOR
                    terrainLayer = SaveTerrainLayer(terrainLayer, _vegetationPackage);  // save the terrain layer on-disk
                    EditorUtility.SetDirty(_vegetationPackage); // mark the vegetation package as dirty so it actually gets written to on-disk
#endif
                    terrainTextureInfo.TerrainLayer = terrainLayer; // assign the terrain layer to the texture of the vegetation package
                }
                else
                {   // else write new data into the existing terrain layer
                    terrainLayer.diffuseTexture = terrainTextureInfo.Texture;
                    terrainLayer.normalMapTexture = terrainTextureInfo.TextureNormals;
                    terrainLayer.tileSize = terrainTextureInfo.TileSize;
                    terrainLayer.tileOffset = terrainTextureInfo.Offset;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(terrainLayer);   // then set it dirty so it gets written to on-disk
#endif
                }
                terrainLayers[i] = terrainLayer;    // assign the terrain layer to the temp array to later assign it into the terrain
            }

            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++) // for all "UnityTerrain" terrains in the terrain list
            {
                IVegetationStudioTerrain iVegetationStudioTerrain = vegetationSystemPro.vegetationStudioTerrainList[i];
                if (iVegetationStudioTerrain is UnityTerrain)   // check whether it is an "UnityTerrain" -- whether it can have terrain textures
                    iVegetationStudioTerrain.SetTerrainLayers(terrainLayers);   // write the terrain layers into the terrain
            }
        }

        private TerrainLayer SaveTerrainLayer(TerrainLayer _terrainLayer, VegetationPackagePro _vegetationPackagePro)   // save terrain layers on-disk into the project files in a folder next to the related vegetation package
        {
#if UNITY_EDITOR
            if (_vegetationPackagePro == null)
                return _terrainLayer;   // edge case -- return the terrain layer and don't save -- stay within memory only and don't do on-disk writes

            string terrainDataPath = AssetDatabase.GetAssetPath(_vegetationPackagePro);
            string directory = Path.GetDirectoryName(terrainDataPath);
            string fileName = Path.GetFileNameWithoutExtension(terrainDataPath) + "_TerrainLayers";

            if (AssetDatabase.IsValidFolder(directory + "/" + fileName) == false)
                AssetDatabase.CreateFolder(directory, fileName);

            terrainDataPath = terrainDataPath.Replace(".asset", "");
            string newTerrainLayerDataPath = directory + "/" + fileName + "/_TerrainLayer_" + Guid.NewGuid().ToString() + ".asset";
            AssetDatabase.CreateAsset(_terrainLayer, newTerrainLayerDataPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<TerrainLayer>(newTerrainLayerDataPath);    // return the terrain layer at the new path to have the correct reference on-disk within the project files
#else
            return _terrainLayer;   // while in a build -- return the terrain layer and don't save -- stay within memory only and don't do on-disk writes
#endif
        }

        List<IVegetationStudioTerrain> GetOverlapTerrainList(Bounds _updateBounds)  // return a list with all "UnityTerrain" terrains within given bounds
        {
            List<IVegetationStudioTerrain> overlapTerrainList = new();
            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++) // for all "UnityTerrain" terrains in the terrain list
                if (vegetationSystemPro.vegetationStudioTerrainList[i].NeedsSplatmapUpdate(_updateBounds))  // check whether the bounds of the terrain overlaps with "_updateBounds" -- whether it's an "UnityTerrain"
                    overlapTerrainList.Add(vegetationSystemPro.vegetationStudioTerrainList[i]); // add to the overlap list (this always happens basically as long as it's an "UnityTerrain" since "_updateBounds" is the totalArea of the system)
            return overlapTerrainList;
        }

        void PrepareAllTextureArrays()  // creates and validates native arrays and animation curves of all vegetation packages
        {
            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                vegetationSystemPro.vegetationPackageProList[i].PrepareTextureArrays();
        }

        void DisposeAllTextureArrays()  // disposes native arrays of all vegetation packages
        {
            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                vegetationSystemPro.vegetationPackageProList[i].DisposeTextureArrays();
        }

        public void GenerateSplatMap(Bounds _bounds, bool _clearExistingTextures)
        {
            if (vegetationSystemPro == false)
                return;

            PrepareAllTextureArrays();  // create/validate texture curves and arrays
            float heightCurveWorldHeight = vegetationSystemPro.vegetationSystemBounds.max.y - vegetationSystemPro.systemRelativeSeaLevel;   // calculate needed world data for the texture rules
            List<IVegetationStudioTerrain> overlapTerrainList = GetOverlapTerrainList(_bounds); // get all "UnityTerrain" terrains within the given bounds

            for (int i = 0; i < overlapTerrainList.Count; i++)  // for all overlapped "UnityTerrain" terrains
            {
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Prepare generation", "Terrain " + (i + 1) + "/" + overlapTerrainList.Count, (i + 1) / (float)overlapTerrainList.Count);
#endif                
                vegetationSystemPro.ClearCache(overlapTerrainList[i].TerrainBounds);    // clear cells of the affected terrains -- let the system apply terrain texture vegetation item rules afterwards
                overlapTerrainList[i].PrepareSplatmapGeneration(_clearExistingTextures, heightCurveWorldHeight, vegetationSystemPro.systemRelativeSeaLevel);    // get needed height/steepness data -- create/validate a new splat map array -- copy over old data of existing textures
            }

            VegetationPackagePro defaultVegetationPackage = vegetationSystemPro.GetVegetationPackageFromBiome(BiomeType.Default);   // get first found default biome
            if (defaultVegetationPackage != null && defaultVegetationPackage.GenerateBiomeSplatmap != false && defaultVegetationPackage.TerrainTextureSettingsList.Count > 0)   // skip when the default biome shouldn't affect the splat map
                for (int i = 0; i < overlapTerrainList.Count; i++)  // for all overlapped "UnityTerrain" terrains
                {   // generate the splat map using the default biome -- base splat map -- use terrain texture splat map rules -- clear terrain textures if set or else copy over the old existing ones
#if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Generating default biome", "Terrain " + (i + 1) + "/" + overlapTerrainList.Count, (i + 1) / (float)overlapTerrainList.Count);
#endif
                    overlapTerrainList[i].GenerateSplatmapBiome(BiomeType.Default, null, defaultVegetationPackage.TerrainTextureSettingsList, vegetationSystemPro.systemRelativeSeaLevel, _clearExistingTextures);
                }

            List<BiomeType> additionalBiomeList = vegetationSystemPro.GetAdditionalBiomeList(); // get a list of all (additional) non-default biomes
            List<VegetationPackagePro> additionalVegetationPackageList = new();
            for (int i = 0; i < additionalBiomeList.Count; i++) // for all non-default biomes
                additionalVegetationPackageList.Add(vegetationSystemPro.GetVegetationPackageFromBiome(additionalBiomeList[i])); // get and store all non-default biomes

            BiomeSortOrderComparer biomeSortOrderComparer = new();
            additionalVegetationPackageList.Sort(biomeSortOrderComparer);   // sort non-default biomes based on their set sort orders

            for (int i = 0; i < additionalVegetationPackageList.Count; i++) // for all non-default biomes
            {
                if (additionalVegetationPackageList[i].GenerateBiomeSplatmap == false || additionalVegetationPackageList[i].TerrainTextureSettingsList.Count <= 0)
                    continue;   // skip when the current biome shouldn't affect a part of the splat map

                List<PolygonMaskBiome> biomeMaskList = VegetationStudioManager.GetBiomeMasks(additionalVegetationPackageList[i].BiomeType); // get all biome masks that match the current biome

                // re-generate part of the splat map based on the current biome and matching/overlapping biome masks vs the current terrain -- use terrain texture splat map rules -- clear terrain textures if set or else copy over the old existing ones
                for (int j = 0; j < overlapTerrainList.Count; j++)  // for all overlapped "UnityTerrain" terrains
                    overlapTerrainList[j].GenerateSplatmapBiome(additionalVegetationPackageList[i].BiomeType, biomeMaskList, additionalVegetationPackageList[i].TerrainTextureSettingsList, vegetationSystemPro.systemRelativeSeaLevel, _clearExistingTextures);
            }

            JobHandle.ScheduleBatchedJobs();    // run/prioritize all splat map jobs

            for (int i = 0; i < overlapTerrainList.Count; i++)  // for all overlapped "UnityTerrain" terrains
            {   // complete/synchronize splat map generation jobs -- actually assign the generated data to the current terrain -- dispose/clean up used data
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Generating specific biomes", "Terrain " + (i + 1) + "/" + overlapTerrainList.Count, (i + 1) / (float)overlapTerrainList.Count);
#endif
                overlapTerrainList[i].CompleteSplatmapGeneration();
            }

#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            DisposeAllTextureArrays();  // dispose texture arrays
        }
    }
}