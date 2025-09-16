using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.ColliderSystem
{
    public class VegetationPackageColliderInfo
    {
        [NonSerialized] public readonly List<ColliderManager> colliderManagerList = new();
    }

    public class ColliderManager
    {
        [NonSerialized] public readonly VegetationItemSelector vegetationItemSelector;

        [NonSerialized] public readonly ColliderPool colliderPool;
        [NonSerialized] public readonly RuntimePrefabStorage runtimePrefabStorage;
        private bool showColliders;

        public delegate void MultiCreateColliderDelegate(GameObject _colliderGameObject);
        public MultiCreateColliderDelegate OnCreateColliderDelegate;

        public delegate void MultiBeforeDestroyColliderDelegate(GameObject _colliderGameObject);
        public MultiBeforeDestroyColliderDelegate OnBeforeDestroyColliderDelegate;

        public ColliderManager(VisibleVegetationCellSelector _visibleVegetationCellSelector, VegetationSystemPro _vegetationSystemPro, VegetationItemInfoPro _vegetationItemInfoPro, Transform _colliderParent, bool _showColliders)
        {
            vegetationItemSelector = new VegetationItemSelector(_visibleVegetationCellSelector, _vegetationSystemPro, _vegetationItemInfoPro, false, 1, 0)
            {
                cullingDistance = _vegetationSystemPro.vegetationSettings.GetGrassDistance() * _vegetationItemInfoPro.ColliderDistanceFactor    // based on grass distance
            };

            vegetationItemSelector.OnVegetationItemVisibleDelegate += OnVegetationItemVisible;
            vegetationItemSelector.OnVegetationItemInvisibleDelegate += OnVegetationItemInvisible;
            vegetationItemSelector.OnVegetationCellInvisibleDelegate += OnVegetationCellInvisible;

            showColliders = _showColliders;

            colliderPool = new ColliderPool(_vegetationItemInfoPro, _vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfoPro.VegetationItemID), _vegetationSystemPro, _colliderParent, showColliders);
            runtimePrefabStorage = new RuntimePrefabStorage(colliderPool);
        }

        public void SetColliderVisibility(bool _value)
        {
            showColliders = _value;
            colliderPool.SetColliderVisibility(_value);
        }

        private void OnVegetationItemVisible(ItemSelectorInstanceInfo _itemSelectorInstanceInfo, VegetationItemIndices _vegetationItemIndices, string _vegetationItemID)
        {
            GameObject colliderObject = colliderPool.GetObject(_itemSelectorInstanceInfo);  // get an object from the pool => set pos, rot, scale, etc
            runtimePrefabStorage.AddRuntimePrefab(colliderObject, _itemSelectorInstanceInfo.VegetationCellIndex, _itemSelectorInstanceInfo.VegetationCellItemIndex);
            OnCreateColliderDelegate?.Invoke(colliderObject);
        }

        private void OnVegetationItemInvisible(ItemSelectorInstanceInfo _itemSelectorInstanceInfo, VegetationItemIndices _vegetationItemIndices, string _vegetationItemID)
        {
            OnBeforeDestroyColliderDelegate?.Invoke(runtimePrefabStorage.GetRuntimePrefab(_itemSelectorInstanceInfo.VegetationCellIndex, _itemSelectorInstanceInfo.VegetationCellItemIndex));
            runtimePrefabStorage.RemoveRuntimePrefab(_itemSelectorInstanceInfo.VegetationCellIndex, _itemSelectorInstanceInfo.VegetationCellItemIndex, colliderPool);
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
            colliderPool?.Dispose();
        }
    }
}