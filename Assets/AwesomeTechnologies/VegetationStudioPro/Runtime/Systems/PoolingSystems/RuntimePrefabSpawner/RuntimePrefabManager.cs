using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.PrefabSpawner
{
    public class VegetationPackageRuntimePrefabInfo
    {
        [NonSerialized] public readonly List<VegetationItemRuntimePrefabInfo> runtimePrefabManagerList_Ex = new();
    }

    public class VegetationItemRuntimePrefabInfo
    {
        [NonSerialized] public readonly List<RuntimePrefabManager> runtimePrefabManagerList = new();
    }

    public class RuntimePrefabManager
    {
        [NonSerialized] public readonly VegetationItemSelector vegetationItemSelector;

        [NonSerialized] public readonly RuntimePrefabPool runtimePrefabPool;
        [NonSerialized] public readonly RuntimePrefabStorage runtimePrefabStorage;
        private readonly VegetationSystemPro vegetationSystemPro;
        private readonly RuntimePrefabRule runtimePrefabRule;
        private bool showPrefabsInHierarchy;

        public RuntimePrefabManager(VisibleVegetationCellSelector _visibleVegetationCellSelector, VegetationSystemPro _vegetationSystemPro, VegetationItemInfoPro _vegetationItemInfoPro, RuntimePrefabRule _runtimePrefabRule, Transform _prefabParent, bool _showPrefabsInHierarchy)
        {
            vegetationItemSelector = new VegetationItemSelector(_visibleVegetationCellSelector, _vegetationSystemPro, _vegetationItemInfoPro, true, _runtimePrefabRule.SpawnFrequency, _runtimePrefabRule.Seed)
            {
                cullingDistance = _vegetationSystemPro.vegetationSettings.GetGrassDistance() * _runtimePrefabRule.DistanceFactor    // based on grass distance
            };

            vegetationItemSelector.OnVegetationItemVisibleDelegate += OnVegetationItemVisible;
            vegetationItemSelector.OnVegetationItemInvisibleDelegate += OnVegetationItemInvisible;
            vegetationItemSelector.OnVegetationCellInvisibleDelegate += OnVegetationCellInvisible;

            vegetationSystemPro = _vegetationSystemPro;
            runtimePrefabRule = _runtimePrefabRule;
            showPrefabsInHierarchy = _showPrefabsInHierarchy;

            runtimePrefabPool = new RuntimePrefabPool(runtimePrefabRule, _vegetationItemInfoPro, _prefabParent, showPrefabsInHierarchy, vegetationSystemPro);
            runtimePrefabStorage = new RuntimePrefabStorage(runtimePrefabPool);
        }

        public void SetRuntimePrefabVisibility(bool _value)
        {
            showPrefabsInHierarchy = _value;
            runtimePrefabStorage.SetPrefabVisibility(_value);
        }

        private void OnVegetationItemVisible(ItemSelectorInstanceInfo _itemSelectorInstanceInfo, VegetationItemIndices _vegetationItemIndices, string _vegetationItemID)
        {
            runtimePrefabStorage.AddRuntimePrefab(runtimePrefabPool.GetObject(_itemSelectorInstanceInfo), _itemSelectorInstanceInfo.VegetationCellIndex, _itemSelectorInstanceInfo.VegetationCellItemIndex);
        }

        private void OnVegetationItemInvisible(ItemSelectorInstanceInfo _itemSelectorInstanceInfo, VegetationItemIndices _vegetationItemIndices, string _vegetationItemID)
        {
            runtimePrefabStorage.RemoveRuntimePrefab(_itemSelectorInstanceInfo.VegetationCellIndex, _itemSelectorInstanceInfo.VegetationCellItemIndex, runtimePrefabPool);
        }

        private void OnVegetationCellInvisible(int _vegetationCellIndex)
        {
            runtimePrefabStorage.RemoveRuntimePrefab(_vegetationCellIndex);
        }

        public void Dispose()
        {
            vegetationItemSelector.OnVegetationItemVisibleDelegate -= OnVegetationItemVisible;
            vegetationItemSelector.OnVegetationItemInvisibleDelegate -= OnVegetationItemInvisible;
            vegetationItemSelector.OnVegetationCellInvisibleDelegate -= OnVegetationCellInvisible;

            vegetationItemSelector?.Dispose();
            runtimePrefabStorage?.Dispose();
            runtimePrefabPool?.Dispose();
        }
    }
}