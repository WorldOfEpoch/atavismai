using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RuntimePrefabPool : VegetationItemPool
{
    private readonly List<GameObject> prefabPoolList = new();
    private int prefabCounter;

    private readonly VegetationSystemPro vegetationSystemPro;
    private readonly VegetationItemInfoPro vegetationItemInfoPro;

    private readonly Transform prefabParent;
    private readonly RuntimePrefabRule runtimePrefabRule;

    private bool showPrefabsInHierarchy;

    public RuntimePrefabPool(RuntimePrefabRule _runtimePrefabRule, VegetationItemInfoPro _vegetationItemInfoPro, Transform _prefabParent, bool _showPrefabsInHierarchy, VegetationSystemPro _vegetationSystemPro)
    {
        vegetationSystemPro = _vegetationSystemPro;
        vegetationItemInfoPro = _vegetationItemInfoPro;
        prefabParent = _prefabParent;
        runtimePrefabRule = _runtimePrefabRule;
        showPrefabsInHierarchy = _showPrefabsInHierarchy;
    }

    private void AddVegetationItemInstanceInfo(GameObject _runtimePrefab)
    {
        VegetationItemInstanceInfo vegetationItemInstanceInfo = _runtimePrefab.AddComponent<VegetationItemInstanceInfo>();
        vegetationItemInstanceInfo.VegetationType = vegetationItemInfoPro.VegetationType;
        vegetationItemInstanceInfo.VegetationItemID = vegetationItemInfoPro.VegetationItemID;
        vegetationItemInstanceInfo.runtimePrefabSource = runtimePrefabRule.RuntimePrefab;

        RuntimeObjectInfo runtimeObjectInfo = _runtimePrefab.AddComponent<RuntimeObjectInfo>();
        runtimeObjectInfo.VegetationItemInfo = vegetationItemInfoPro;
    }

    private void UpdateVegetationItemInstanceInfo(GameObject _runtimePrefab, ItemSelectorInstanceInfo _info)
    {
        VegetationItemInstanceInfo vegetationItemInstanceInfo = _runtimePrefab.GetComponent<VegetationItemInstanceInfo>();
        if (vegetationItemInstanceInfo)
        {
            vegetationItemInstanceInfo.Position = _info.Position;
            vegetationItemInstanceInfo.VegetationItemInstanceID = ((int)math.round(vegetationItemInstanceInfo.Position.x * 100f)).ToString() + "_" +
                                                                  ((int)math.round(vegetationItemInstanceInfo.Position.y * 100f)).ToString() + "_" +
                                                                  ((int)math.round(vegetationItemInstanceInfo.Position.z * 100f)).ToString();
            vegetationItemInstanceInfo.Rotation = _info.Rotation;
            vegetationItemInstanceInfo.Scale = _info.Scale;
        }
    }

    public void SetPrefabVisibility(bool _value)    // not used currently -- used differently through "RuntimePrefabInfo.cs"
    {
        showPrefabsInHierarchy = _value;
        for (int i = 0; i < prefabPoolList.Count; i++)
            if (_value)
                prefabPoolList[i].hideFlags = HideFlags.DontSave;
            else
                prefabPoolList[i].hideFlags = HideFlags.HideAndDontSave;
    }

    private HideFlags GetVisibilityHideFlags()
    {
        return showPrefabsInHierarchy ? HideFlags.DontSave : HideFlags.HideAndDontSave;
    }

    public override GameObject GetObject(ItemSelectorInstanceInfo _info)
    {
        if (prefabPoolList.Count <= 0)
            return CreateRuntimePrefabObject(_info);

        GameObject prefabObject = prefabPoolList[prefabPoolList.Count - 1];
        prefabPoolList.RemoveAtSwapBack(prefabPoolList.Count - 1);
        PositionPrefabObject(prefabObject, _info);
        prefabObject.SetActive(true);
        return prefabObject;
    }

    public override void ReturnObject(GameObject _prefabObject)
    {
        if (_prefabObject == null)
            return;

        if (runtimePrefabRule.UsePool)
        {
            _prefabObject.SetActive(false);
            prefabPoolList.Add(_prefabObject);
        }
        else
        {
            DestroyObject(_prefabObject);
        }
    }

    private void PositionPrefabObject(GameObject _prefabObject, ItemSelectorInstanceInfo _info)
    {
        float3 scale = runtimePrefabRule.UseVegetationItemScale ? _info.Scale : Vector3.one;

        _prefabObject.transform.SetPositionAndRotation(_info.Position + vegetationSystemPro.floatingOriginOffset + math.mul(_info.Rotation, _info.Scale * runtimePrefabRule.PrefabOffset),
            _info.Rotation * Quaternion.Euler(runtimePrefabRule.PrefabRotation));   // degrees
        _prefabObject.transform.localScale = new float3(scale.x * runtimePrefabRule.PrefabScale.x, scale.y * runtimePrefabRule.PrefabScale.y, scale.z * runtimePrefabRule.PrefabScale.z);

        UpdateVegetationItemInstanceInfo(_prefabObject, _info);
    }

    public GameObject CreateRuntimePrefabObject(ItemSelectorInstanceInfo _info)
    {
        GameObject newRuntimePrefab;
        if (runtimePrefabRule.RuntimePrefab)
        {
            newRuntimePrefab = Object.Instantiate(runtimePrefabRule.RuntimePrefab);
            newRuntimePrefab.name = runtimePrefabRule.RuntimePrefab.name;
            newRuntimePrefab.SetActive(true);   // ensure spawned gameObject is enabled => prefabs could be in a pre-disabled state
        }
        else
        {   // this here gets skipped now as the UI already loads a primitive cube on missing prefabs
            newRuntimePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newRuntimePrefab.name = "Run-time prefab" + prefabCounter.ToString() + "_" + newRuntimePrefab.name;
        }

        newRuntimePrefab.transform.SetParent(prefabParent);
        newRuntimePrefab.hideFlags = GetVisibilityHideFlags();
        newRuntimePrefab.layer = runtimePrefabRule.PrefabLayer;

        AddVegetationItemInstanceInfo(newRuntimePrefab);
        PositionPrefabObject(newRuntimePrefab, _info);  // last

        prefabCounter++;
        return newRuntimePrefab;
    }

    private static void DestroyObject(GameObject _go)
    {
        if (Application.isPlaying)
            Object.Destroy(_go);
        else
            Object.DestroyImmediate(_go);
    }

    public void Dispose()
    {
        for (int i = 0; i < prefabPoolList.Count; i++)
            DestroyObject(prefabPoolList[i]);
        prefabPoolList.Clear();
    }
}