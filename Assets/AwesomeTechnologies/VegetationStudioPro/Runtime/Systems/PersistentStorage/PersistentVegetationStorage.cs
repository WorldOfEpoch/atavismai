using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [Serializable]
    public enum PrecisionPaintingMode
    {
        Terrain,
        TerrainAndColliders,
        TerrainAndMeshes
    }

    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Systems/PersistentVegetationStorage", 3)]
    [ExecuteInEditMode]
    public class PersistentVegetationStorage : MonoBehaviour
    {
        public VegetationSystemPro vegetationSystemPro;
        public PersistentVegetationStoragePackage persistentVegetationStoragePackage;
        public int currentTabIndex;
        public int selectedVegetationPackageIndex;

        public int selectedBrushIndex;
        public float brushSize = 5;
        public float sampleDistance = 1;
        public LayerMask groundLayerMask;

        public bool randomizePosition = true;
        public bool randomizeRotation = true;
        public bool usePositionOffsetRange = true;
        public bool useScale = true;
        public bool useSteepnessRule;
        public bool useConcaveLocationRule;
        public bool useTerrainTextureIncludeRule;   // terr tex
        public bool useTerrainTextureExcludeRule;
        public bool useTerrainTextureScaleRule;
        public bool useTextureMaskIncludeRule;  // tex mask
        public bool useTextureMaskExcludeRule;
        public bool useTextureMaskScaleRule;

        public bool ignoreHeight = true;

        public bool disablePersistentStorage;
        public bool useVegetationMasking;
        public bool forceBaking;
        public bool excludeGrass = true;
        public bool excludePlants;
        public bool excludeTrees;
        public bool excludeObjects;
        public bool excludeLargeObjects;

        public string selectedEditVegetationID;
        public string selectedPaintVegetationID;
        public string selectedBakeVegetationID;
        public string selectedStorageVegetationID;
        public string selectedPrecisionPaintingVegetationID;
        public PrecisionPaintingMode precisionPaintingMode = PrecisionPaintingMode.TerrainAndMeshes;

        public List<IVegetationImporter> vegetationImporterList = new();
        public int selectedImporterIndex;

        void Reset()
        {
            vegetationSystemPro = GetComponent<VegetationSystemPro>();
            vegetationSystemPro.persistentVegetationStorage = this;
        }

        private void OnEnable()
        {
            vegetationSystemPro = GetComponent<VegetationSystemPro>();
        }

        public bool HasValidPersistentStorage(int _cellCount)
        {
            if (persistentVegetationStoragePackage == null) return false;
            if (persistentVegetationStoragePackage.PersistentVegetationCellList.Count != _cellCount) return false;

            return true;
        }

        public void SetPersistentVegetationStoragePackage(PersistentVegetationStoragePackage _persistentVegetationStoragePackage)
        {
            persistentVegetationStoragePackage = _persistentVegetationStoragePackage;
            if (vegetationSystemPro)
                vegetationSystemPro.ClearCache();
        }

        public void InitializePersistentStorage()
        {
            if (persistentVegetationStoragePackage == null)
                return;

            persistentVegetationStoragePackage.ClearPersistentVegetationCells();
            for (int i = 0; i < vegetationSystemPro.vegetationCellList.Count; i++)
                persistentVegetationStoragePackage.AddVegetationCell();
        }

        public void InitializePersistentStorage(int _cellCount)
        {
            if (persistentVegetationStoragePackage == null)
                return;

            persistentVegetationStoragePackage.ClearPersistentVegetationCells();
            for (int i = 0; i < _cellCount; i++)
                persistentVegetationStoragePackage.AddVegetationCell();
        }

        public void AddVegetationItemInstance(string _vegetationItemID, float3 _worldPosition, float3 _scale, quaternion _rotation, bool _applyMeshRotation, byte _vegetationSourceID, float _distanceFalloff, bool _clearCellCache = false)
        {
            if (vegetationSystemPro == null || persistentVegetationStoragePackage == null)
                return;

            VegetationItemInfoPro vegetationItemInfo = vegetationSystemPro.GetVegetationItemInfo(_vegetationItemID);
            if (_applyMeshRotation && vegetationItemInfo != null)
                _rotation *= Quaternion.Euler(vegetationItemInfo.RotationOffset);   // degrees

            Rect positionRect = new(new float2(_worldPosition.x, _worldPosition.z), float2.zero);
            List<VegetationCell> overlapCellList = new();
            vegetationSystemPro.vegetationCellQuadTree.Query(positionRect, overlapCellList);

            for (int i = 0; i < overlapCellList.Count; i++)
            {
                if (_clearCellCache)
                    vegetationSystemPro.ClearCache(overlapCellList[i], _vegetationItemID);
                persistentVegetationStoragePackage.AddVegetationItemInstance(overlapCellList[i].index, _vegetationItemID, _worldPosition - vegetationSystemPro.VegetationSystemPosition, _scale, _rotation, _vegetationSourceID, _distanceFalloff);
            }
        }

        public void AddVegetationItemInstanceEx(string _vegetationItemID, float3 _worldPosition, float3 _scale, quaternion _rotation, byte _vegetationSourceID, float _minimumDistance, float _distanceFalloff, bool _clearCellCache = false)
        {
            if (vegetationSystemPro == null || persistentVegetationStoragePackage == null || vegetationSystemPro.vegetationCellQuadTree == null)
                return;

            Rect positionRect = new(new float2(_worldPosition.x, _worldPosition.z), float2.zero);
            List<VegetationCell> overlapCellList = new();
            vegetationSystemPro.vegetationCellQuadTree.Query(positionRect, overlapCellList);

            for (int i = 0; i < overlapCellList.Count; i++)
            {
                if (_clearCellCache)
                    vegetationSystemPro.ClearCache(overlapCellList[i], _vegetationItemID);
                persistentVegetationStoragePackage.AddVegetationItemInstanceEx(overlapCellList[i].index, _vegetationItemID, _worldPosition - vegetationSystemPro.VegetationSystemPosition, _scale, _rotation, _vegetationSourceID, _minimumDistance, _distanceFalloff);
            }
        }

        public void RemoveVegetationItemInstance(string _vegetationItemID, float3 _worldPosition, float _minimumDistance, bool _clearCellCache = false)
        {
            if (vegetationSystemPro == null || persistentVegetationStoragePackage == null)
                return;

            Rect positionRect = new(new float2(_worldPosition.x, _worldPosition.z), float2.zero);
            List<VegetationCell> overlapCellList = new();
            vegetationSystemPro.vegetationCellQuadTree.Query(positionRect, overlapCellList);

            for (int i = 0; i < overlapCellList.Count; i++)
            {
                if (_clearCellCache)
                    vegetationSystemPro.ClearCache(overlapCellList[i], _vegetationItemID);
                persistentVegetationStoragePackage.RemoveVegetationItemInstance(overlapCellList[i].index, _vegetationItemID, _worldPosition - vegetationSystemPro.VegetationSystemPosition, _minimumDistance);
            }
        }

        public void RemoveVegetationItemInstance2D(string _vegetationItemID, float3 _worldPosition, float _minimumDistance, bool _clearCellCache = false)
        {
            if (vegetationSystemPro == null || persistentVegetationStoragePackage == null)
                return;

            Rect positionRect = new(new float2(_worldPosition.x, _worldPosition.z), float2.zero);
            List<VegetationCell> overlapCellList = new();
            vegetationSystemPro.vegetationCellQuadTree.Query(positionRect, overlapCellList);

            for (int i = 0; i < overlapCellList.Count; i++)
            {
                if (_clearCellCache)
                    vegetationSystemPro.ClearCache(overlapCellList[i], _vegetationItemID);
                persistentVegetationStoragePackage.RemoveVegetationItemInstance2D(overlapCellList[i].index, _vegetationItemID, _worldPosition - vegetationSystemPro.VegetationSystemPosition, _minimumDistance);
            }
        }

        public void RepositionCellItems(int _cellIndex, string _vegetationItemID)
        {
            PersistentVegetationInfo persistentVegetationInfo = persistentVegetationStoragePackage.PersistentVegetationCellList[_cellIndex].GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo == null)
                return;

            List<PersistentVegetationItem> originalItemList = new();
            originalItemList.AddRange(persistentVegetationInfo.VegetationItemList);
            persistentVegetationInfo.ClearCell();

            for (int i = 0; i < originalItemList.Count; i++)
                AddVegetationItemInstance(_vegetationItemID, originalItemList[i].Position + vegetationSystemPro.VegetationSystemPosition, originalItemList[i].Scale, originalItemList[i].Rotation, false, originalItemList[i].VegetationSourceID, originalItemList[i].DistanceFalloff, true);

            vegetationSystemPro.ClearCache(vegetationSystemPro.vegetationCellList[_cellIndex], _vegetationItemID);
        }

        public int GetPersistentVegetationCellCount()
        {
            if (persistentVegetationStoragePackage && persistentVegetationStoragePackage.PersistentVegetationCellList != null)
                return persistentVegetationStoragePackage.PersistentVegetationCellList.Count;
            return 0;
        }

        public PersistentVegetationCell GetPersistentVegetationCell(int _index)
        {
            if (persistentVegetationStoragePackage && persistentVegetationStoragePackage.PersistentVegetationCellList != null)
                if (_index < persistentVegetationStoragePackage.PersistentVegetationCellList.Count)
                    return persistentVegetationStoragePackage.PersistentVegetationCellList[_index];
            return null;
        }

        public void RemoveVegetationItemInstances(string _vegetationItemID, byte _vegetationSourceID)
        {
            if (persistentVegetationStoragePackage == null)
                return;
            persistentVegetationStoragePackage.RemoveVegetationItemInstances(_vegetationItemID, _vegetationSourceID);
        }


        public void RemoveVegetationItemInstances(string _vegetationItemID)
        {
            if (persistentVegetationStoragePackage == null)
                return;
            persistentVegetationStoragePackage.RemoveVegetationItemInstances(_vegetationItemID);
        }

        public void Dispose()
        {
            if (persistentVegetationStoragePackage)
                persistentVegetationStoragePackage.Dispose();
        }

        public void BakeVegetationItem(string _vegetationItemID)
        {
            if (vegetationSystemPro == null)
                return;

            if (_vegetationItemID == "")
            {
                Debug.LogError("VSP internal error log: Baking: The given vegetationItemID is empty");
                return;
            }

            VegetationItemInfoPro vegetationItemInfo = vegetationSystemPro.GetVegetationItemInfo(_vegetationItemID);

            if (excludeGrass && vegetationItemInfo.VegetationType == VegetationType.Grass) return;
            if (excludePlants && vegetationItemInfo.VegetationType == VegetationType.Plant) return;
            if (excludeTrees && vegetationItemInfo.VegetationType == VegetationType.Tree) return;
            if (excludeObjects && vegetationItemInfo.VegetationType == VegetationType.Objects) return;
            if (excludeLargeObjects && vegetationItemInfo.VegetationType == VegetationType.LargeObjects) return;

            if (forceBaking)
                vegetationItemInfo.EnableRuntimeSpawn = true;

            vegetationSystemPro.ClearCache(_vegetationItemID);

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                EditorUtility.DisplayProgressBar("Bake vegetation item: " + vegetationItemInfo.Name, "Spawn all cells", 0);
#endif

            for (int i = 0; i < vegetationSystemPro.vegetationCellList.Count; i++)
            {
#if UNITY_EDITOR
                if (i % 10 == 0 && Application.isPlaying == false)
                    EditorUtility.DisplayProgressBar("Bake vegetation item: " + vegetationItemInfo.Name, "Spawn cell " + i + "/" + (vegetationSystemPro.vegetationCellList.Count - 1), i / ((float)vegetationSystemPro.vegetationCellList.Count - 1));
#endif

                vegetationSystemPro.SpawnVegetationCellEx(vegetationSystemPro.vegetationCellList[i], _vegetationItemID, true); // skip re-baking vegetation already in the persistent storage
                NativeList<MatrixInstance> vegetationInstanceList = vegetationSystemPro.GetVegetationItemInstances(vegetationSystemPro.vegetationCellList[i], _vegetationItemID);

                for (int j = 0; j < vegetationInstanceList.Length; j++)
                {
                    if (vegetationInstanceList[j].controlData.x <= 0)
                        continue;   // skip masked out persistent vegetation storage vegetation instances

                    Matrix4x4 vegetationItemMatrix = vegetationInstanceList[j].matrix;
                    persistentVegetationStoragePackage.AddVegetationItemInstance(vegetationSystemPro.vegetationCellList[i].index, _vegetationItemID,
                        MatrixTools.ExtractTranslationFromMatrix(vegetationItemMatrix) - vegetationSystemPro.VegetationSystemPosition, MatrixTools.ExtractScaleFromMatrix(vegetationItemMatrix), MatrixTools.ExtractRotationFromMatrix(vegetationItemMatrix),
                        0, vegetationInstanceList[j].controlData.x);
                }

                vegetationSystemPro.vegetationCellList[i].ClearCache();
            }

            vegetationItemInfo.EnableRuntimeSpawn = false;
            vegetationSystemPro.ClearCache(_vegetationItemID);

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                EditorUtility.ClearProgressBar();
#endif
        }
    }
}