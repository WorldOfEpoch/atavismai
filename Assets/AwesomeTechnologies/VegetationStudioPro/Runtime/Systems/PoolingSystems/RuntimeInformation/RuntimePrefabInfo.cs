using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class RuntimePrefabInfo
{
    public GameObject RuntimeObject;
    public int VegetationCellIndex;
    public int VegetationCellItemIndex;
}

public class RuntimePrefabStorage
{
    [NonSerialized] public readonly List<RuntimePrefabInfo> runtimePrefabInfoList = new();
    private readonly VegetationItemPool vegetationItemPool;

    public RuntimePrefabStorage(VegetationItemPool _vegetationItemPool)
    {
        vegetationItemPool = _vegetationItemPool;
    }

    public void SetPrefabVisibility(bool _value)
    {
        for (int i = 0; i < runtimePrefabInfoList.Count; i++)
            if (_value)
                runtimePrefabInfoList[i].RuntimeObject.hideFlags = HideFlags.DontSave;
            else
                runtimePrefabInfoList[i].RuntimeObject.hideFlags = HideFlags.HideAndDontSave;
    }

    public void UpdateFloatingOrigin(Vector3 _deltaFloatingOriginOffset)
    {
        for (int i = runtimePrefabInfoList.Count - 1; i >= 0; i--)
            runtimePrefabInfoList[i].RuntimeObject.transform.position += _deltaFloatingOriginOffset;
    }

    public void RemoveRuntimePrefab(int _vegetationCellIndex)
    {
        for (int i = runtimePrefabInfoList.Count - 1; i >= 0; i--)
            if (runtimePrefabInfoList[i].VegetationCellIndex == _vegetationCellIndex)
            {
                if (vegetationItemPool != null)
                    vegetationItemPool.ReturnObject(runtimePrefabInfoList[i].RuntimeObject);
                else
                    DestroyRuntimePrefab(runtimePrefabInfoList[i]);

                runtimePrefabInfoList.RemoveAtSwapBack(i);
            }
    }

    public void AddRuntimePrefab(GameObject _runtimeObject, int _vegetationCellIndex, int _vegetationCellItemIndex)
    {
        RuntimePrefabInfo runtimePrefabInfo = new RuntimePrefabInfo
        {
            RuntimeObject = _runtimeObject,
            VegetationCellIndex = _vegetationCellIndex,
            VegetationCellItemIndex = _vegetationCellItemIndex
        };
        runtimePrefabInfoList.Add(runtimePrefabInfo);
    }

    public void RemoveRuntimePrefab(int _vegetationCellIndex, int _vegetationCellItemIndex, VegetationItemPool _vegetationItemPool)
    {
        for (int i = runtimePrefabInfoList.Count - 1; i >= 0; i--)
            if (runtimePrefabInfoList[i].VegetationCellIndex == _vegetationCellIndex && runtimePrefabInfoList[i].VegetationCellItemIndex == _vegetationCellItemIndex)
            {
                if (_vegetationItemPool != null)
                    _vegetationItemPool.ReturnObject(runtimePrefabInfoList[i].RuntimeObject);
                else
                    DestroyRuntimePrefab(runtimePrefabInfoList[i]);

                runtimePrefabInfoList.RemoveAtSwapBack(i);
            }
    }

    public GameObject GetRuntimePrefab(int _vegetationCellIndex, int _vegetationCellItemIndex)
    {
        for (int i = runtimePrefabInfoList.Count - 1; i >= 0; i--)
        {
            if (runtimePrefabInfoList[i].VegetationCellIndex == _vegetationCellIndex && runtimePrefabInfoList[i].VegetationCellItemIndex == _vegetationCellItemIndex)
                return runtimePrefabInfoList[i].RuntimeObject;
        }

        return null;
    }

    private void DestroyRuntimePrefab(RuntimePrefabInfo _runtimePrefabInfo)
    {
        if (Application.isPlaying)
            Object.Destroy(_runtimePrefabInfo.RuntimeObject);
        else
            Object.DestroyImmediate(_runtimePrefabInfo.RuntimeObject);
    }

    public void Dispose()
    {
        for (int i = runtimePrefabInfoList.Count - 1; i >= 0; i--)
        {
            if (vegetationItemPool != null)
                vegetationItemPool.ReturnObject(runtimePrefabInfoList[i].RuntimeObject);
            else
                DestroyRuntimePrefab(runtimePrefabInfoList[i]);
        }

        runtimePrefabInfoList.Clear();
    }
}