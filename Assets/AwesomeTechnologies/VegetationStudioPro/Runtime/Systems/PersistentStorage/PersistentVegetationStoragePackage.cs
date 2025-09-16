using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AwesomeTechnologies.Utility;
using Unity.Collections;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [Serializable]
    public class SourceCount
    {
        public byte VegetationSourceID;
        public int Count;
    }

    [Serializable]
    public class PersistentVegetationInstanceInfo
    {
        public string VegetationItemID;
        public int Count;
        public List<SourceCount> SourceCountList = new();

        public void AddSourceCountList(List<SourceCount> _sourceCountList)
        {
            for (int i = 0; i < _sourceCountList.Count; i++)
                AddSourceCount(_sourceCountList[i]);
        }

        public void AddSourceCount(SourceCount _sourceCount)
        {
            SourceCount tempSourceCount = GetSourceCount(_sourceCount.VegetationSourceID);
            if (tempSourceCount == null)
            {
                tempSourceCount = new SourceCount { VegetationSourceID = _sourceCount.VegetationSourceID };
                SourceCountList.Add(tempSourceCount);
            }
            tempSourceCount.Count += _sourceCount.Count;
        }

        SourceCount GetSourceCount(byte _vegetationSourceID)
        {
            for (int i = 0; i < SourceCountList.Count; i++)
                if (SourceCountList[i].VegetationSourceID == _vegetationSourceID)
                    return SourceCountList[i];
            return null;
        }
    }

    [Serializable]
    public struct PersistentVegetationItem  // 45 bytes
    {
        public float3 Position; // 12 bytes
        public float3 Scale;    // 12 bytes
        public Quaternion Rotation; // 16 bytes
        public byte VegetationSourceID; // 1 byte
        public float DistanceFalloff;   // 4 bytes
    }

    [Serializable]
    public class PersistentVegetationInfo   // vegetation instance info per PVS cell
    {
        public string VegetationItemID;
        public List<PersistentVegetationItem> VegetationItemList = new();
        [NonSerialized] public NativeList<PersistentVegetationItem> NativeVegetationItemList;
        [NonSerialized] public List<NativeList<PersistentVegetationItem>> DisposeList = new();
        public List<SourceCount> SourceCountList = new();

        public void CopyInstancesToNativeList()
        {
            NativeVegetationItemList = new(VegetationItemList.Count, Allocator.Persistent) { Length = VegetationItemList.Count };
            NativeVegetationItemList.CopyFromFast(VegetationItemList);
            DisposeList.Add(NativeVegetationItemList);
        }

        public void ClearCell()
        {
            VegetationItemList.Clear();
            SourceCountList.Clear();
        }

        public void AddPersistentVegetationItemInstance(ref PersistentVegetationItem _persistentVegetationItem)
        {
            IncreaseSourceCount(_persistentVegetationItem.VegetationSourceID);
            VegetationItemList.Add(_persistentVegetationItem);
        }

        public void RemovePersistentVegetationItemInstance(ref PersistentVegetationItem _persistentVegetationItem)
        {
            DecreaseSourceCount(_persistentVegetationItem.VegetationSourceID);
            VegetationItemList.Remove(_persistentVegetationItem);
        }

        public void RemovePersistentVegetationInstanceAtIndex(int _index)
        {
            if (_index >= VegetationItemList.Count)
                return;

            DecreaseSourceCount(VegetationItemList[_index].VegetationSourceID);
            VegetationItemList.RemoveAt(_index);
        }

        public void UpdatePersistentVegetationItemInstanceSourceId(ref PersistentVegetationItem _persistentVegetationItem, byte _newSourceID)
        {
            if (_persistentVegetationItem.VegetationSourceID != _newSourceID)
            {
                DecreaseSourceCount(_persistentVegetationItem.VegetationSourceID);
                _persistentVegetationItem.VegetationSourceID = _newSourceID;
                IncreaseSourceCount(_persistentVegetationItem.VegetationSourceID);
            }
        }

        void IncreaseSourceCount(byte _vegetationSourceID)
        {
            SourceCount sourceCount = GetSourceCount(_vegetationSourceID);
            if (sourceCount == null)
            {
                sourceCount = new SourceCount { VegetationSourceID = _vegetationSourceID };
                SourceCountList.Add(sourceCount);
            }
            sourceCount.Count++;
        }

        SourceCount GetSourceCount(byte _vegetationSourceID)
        {
            for (int i = 0; i < SourceCountList.Count; i++)
                if (SourceCountList[i].VegetationSourceID == _vegetationSourceID)
                    return SourceCountList[i];
            return null;
        }

        void DecreaseSourceCount(byte _vegetationSourceID)
        {
            SourceCount sourceCount = GetSourceCount(_vegetationSourceID);
            if (sourceCount == null)
                return;

            sourceCount.Count--;

            if (sourceCount.Count == 0)
                SourceCountList.Remove(sourceCount);
        }

        public void Dispose()
        {
            for (int i = 0; i < DisposeList.Count; i++)
            {
                if (DisposeList[i].IsCreated)
                    DisposeList[i].Dispose();
            }
            DisposeList.Clear();
        }
    }

    [Serializable]
    public class PersistentVegetationCell
    {
        public List<PersistentVegetationInfo> PersistentVegetationInfoList = new();

        public void Dispose()
        {
            for (int i = 0; i < PersistentVegetationInfoList.Count; i++)
                PersistentVegetationInfoList[i].Dispose();
        }

        public void AddVegetationItemInstance(string _vegetationItemID, float3 _position, float3 _scale, quaternion _rotation, byte _vegetationSourceID, float _distanceFalloff)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo == null)
            {
                persistentVegetationInfo = new PersistentVegetationInfo { VegetationItemID = _vegetationItemID };
                PersistentVegetationInfoList.Add(persistentVegetationInfo);
            }

            PersistentVegetationItem persistentVegetationItem = new()
            {
                Position = _position,
                Rotation = _rotation,
                Scale = _scale,
                VegetationSourceID = _vegetationSourceID,
                DistanceFalloff = _distanceFalloff
            };

            persistentVegetationInfo.AddPersistentVegetationItemInstance(ref persistentVegetationItem);
        }

        public void RemoveVegetationItemInstance(string _vegetationItemID, float3 _position, float _minimumDistance)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo == null)
                return;

            for (int i = persistentVegetationInfo.VegetationItemList.Count - 1; i >= 0; i--)
                if (math.distance(persistentVegetationInfo.VegetationItemList[i].Position, _position) < _minimumDistance)
                    persistentVegetationInfo.VegetationItemList.RemoveAt(i);
        }

        public void RemoveVegetationItemInstance2D(string _vegetationItemID, float3 _position, float _minimumDistance)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo == null)
                return;

            for (int i = persistentVegetationInfo.VegetationItemList.Count - 1; i >= 0; i--)
                if (math.distance(new float2(persistentVegetationInfo.VegetationItemList[i].Position.x, persistentVegetationInfo.VegetationItemList[i].Position.z), new float2(_position.x, _position.z)) < _minimumDistance)
                    persistentVegetationInfo.VegetationItemList.RemoveAt(i);
        }

        public void AddVegetationItemInstanceEx(string _vegetationItemID, float3 _position, float3 _scale, quaternion _rotation, byte _vegetationSourceID, float _minimumDistance, float _distanceFalloff)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo == null)
            {
                persistentVegetationInfo = new PersistentVegetationInfo { VegetationItemID = _vegetationItemID };
                PersistentVegetationInfoList.Add(persistentVegetationInfo);
            }

            float closestDistance = CalculateClosestItemDistance(_position, persistentVegetationInfo.VegetationItemList);
            if (closestDistance < _minimumDistance)
                return;

            PersistentVegetationItem persistentVegetationItem = new()
            {
                Position = _position,
                Rotation = _rotation,
                Scale = _scale,
                VegetationSourceID = _vegetationSourceID,
                DistanceFalloff = _distanceFalloff
            };

            persistentVegetationInfo.AddPersistentVegetationItemInstance(ref persistentVegetationItem);
        }

        private float CalculateClosestItemDistance(float3 _position, List<PersistentVegetationItem> _instanceList)
        {
            float nearestSqrMag = float.PositiveInfinity;
            float3 nearestVector3 = Vector3.zero;

            for (int i = 0; i < _instanceList.Count; i++)
            {
                float sqrMag = math.lengthsq(_instanceList[i].Position - _position);
                if (sqrMag < nearestSqrMag)
                {
                    nearestSqrMag = sqrMag;
                    nearestVector3 = _instanceList[i].Position;
                }
            }

            return math.distance(nearestVector3, _position);
        }

        public void ClearCell()
        {
            PersistentVegetationInfoList.Clear();
        }

        public PersistentVegetationInfo GetPersistentVegetationInfo(string _vegetationItemID)
        {
            for (int i = 0; i < PersistentVegetationInfoList.Count; i++)
                if (PersistentVegetationInfoList[i].VegetationItemID == _vegetationItemID)
                    return PersistentVegetationInfoList[i];
            return null;
        }

        public void RemoveVegetationItemInstances(string _vegetationItemID)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo != null)
                PersistentVegetationInfoList.Remove(persistentVegetationInfo);
        }

        public void RemoveVegetationItemInstances(string _vegetationItemID, byte _vegetationSourceID)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(_vegetationItemID);
            if (persistentVegetationInfo != null)
            {
                for (int i = persistentVegetationInfo.VegetationItemList.Count - 1; i >= 0; i--)
                    if (persistentVegetationInfo.VegetationItemList[i].VegetationSourceID == _vegetationSourceID)
                        persistentVegetationInfo.RemovePersistentVegetationInstanceAtIndex(i);

                if (persistentVegetationInfo.VegetationItemList.Count == 0)
                    PersistentVegetationInfoList.Remove(persistentVegetationInfo);
            }
        }
    }

    [Serializable]
    public class ExportData
    {
        public List<PersistentVegetationCell> PersistentVegetationCellList;
        public List<PersistentVegetationInstanceInfo> PersistentVegetationInstanceInfoList;
        public List<byte> PersistentVegetationInstanceSourceList;
    }

    [PreferBinarySerialization]
    [Serializable]
    public class PersistentVegetationStoragePackage : ScriptableObject
    {
        public List<PersistentVegetationCell> PersistentVegetationCellList = new();
        public List<PersistentVegetationInstanceInfo> PersistentVegetationInstanceInfoList = new();
        public List<byte> PersistentVegetationInstanceSourceList = new();
        [SerializeField] private bool _instanceInfoDirty;

        public void Dispose()
        {
            for (int i = 0; i < PersistentVegetationCellList.Count; i++)
                PersistentVegetationCellList[i].Dispose();
        }

        public void ExportToFile(string _filename)
        {
            ExportData exportData = new()
            {
                PersistentVegetationCellList = PersistentVegetationCellList,
                PersistentVegetationInstanceInfoList = PersistentVegetationInstanceInfoList,
                PersistentVegetationInstanceSourceList = PersistentVegetationInstanceSourceList
            };

            BinaryFormatter bf = SerializationSurrogateUtility.GetBinaryFormatter();
            FileStream file = File.Create(_filename);
            bf.Serialize(file, exportData);
            file.Close();
        }

        public void ExportToStream(Stream _outputStream)
        {
            ExportData exportData = new()
            {
                PersistentVegetationCellList = PersistentVegetationCellList,
                PersistentVegetationInstanceInfoList = PersistentVegetationInstanceInfoList,
                PersistentVegetationInstanceSourceList = PersistentVegetationInstanceSourceList
            };

            BinaryFormatter bf = SerializationSurrogateUtility.GetBinaryFormatter();
            bf.Serialize(_outputStream, exportData);
            _outputStream.Position = 0;
        }

        public void ImportFromStream(Stream _inputStream)
        {
            BinaryFormatter bf = SerializationSurrogateUtility.GetBinaryFormatter();
            var exportData = (ExportData)bf.Deserialize(_inputStream);
            PersistentVegetationCellList = exportData.PersistentVegetationCellList;
            PersistentVegetationInstanceInfoList = exportData.PersistentVegetationInstanceInfoList;
            PersistentVegetationInstanceSourceList = exportData.PersistentVegetationInstanceSourceList;
            _inputStream.Position = 0;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void ImportFromFile(string _filename)
        {
            BinaryFormatter bf = SerializationSurrogateUtility.GetBinaryFormatter();
            FileStream file = File.Open(_filename, FileMode.Open);
            var exportData = (ExportData)bf.Deserialize(file);

            PersistentVegetationCellList = exportData.PersistentVegetationCellList;
            PersistentVegetationInstanceInfoList = exportData.PersistentVegetationInstanceInfoList;
            PersistentVegetationInstanceSourceList = exportData.PersistentVegetationInstanceSourceList;

            file.Close();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public bool Initialized
        {
            get { return PersistentVegetationCellList.Count > 0; }
        }

        public void ClearPersistentVegetationCells()
        {
            PersistentVegetationCellList.Clear();
        }

        public void SetInstanceInfoDirty()
        {
            _instanceInfoDirty = true;
        }

        public void RemoveVegetationItemInstances(string _vegetationItemID)
        {
            for (int i = 0; i < PersistentVegetationCellList.Count; i++)
                PersistentVegetationCellList[i].RemoveVegetationItemInstances(_vegetationItemID);
            _instanceInfoDirty = true;
        }

        public void RemoveVegetationItemInstances(string _vegetationItemID, byte _vegetationSourceID)
        {
            for (int i = 0; i < PersistentVegetationCellList.Count; i++)
                PersistentVegetationCellList[i].RemoveVegetationItemInstances(_vegetationItemID, _vegetationSourceID);
            _instanceInfoDirty = true;
        }

        public void AddVegetationCell()
        {
            PersistentVegetationCellList.Add(new());
            _instanceInfoDirty = true;
        }

        public void AddVegetationItemInstance(int _cellIndex, string _vegetationItemID, float3 _position, float3 _scale, quaternion _rotation, byte _vegetationSourceID, float _distanceFalloff)
        {
            if (PersistentVegetationCellList.Count > _cellIndex)
                PersistentVegetationCellList[_cellIndex].AddVegetationItemInstance(_vegetationItemID, _position, _scale, _rotation, _vegetationSourceID, _distanceFalloff);
            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void AddVegetationItemInstanceEx(int _cellIndex, string _vegetationItemID, float3 _position, float3 _scale, quaternion _rotation, byte _vegetationSourceID, float _minimumDistance, float _distanceFalloff)
        {
            if (PersistentVegetationCellList.Count > _cellIndex)
                PersistentVegetationCellList[_cellIndex].AddVegetationItemInstanceEx(_vegetationItemID, _position, _scale, _rotation, _vegetationSourceID, _minimumDistance, _distanceFalloff);
            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveVegetationItemInstance(int _cellIndex, string _vegetationItemID, float3 _position, float _minimumDistance)
        {
            if (PersistentVegetationCellList.Count > _cellIndex)
                PersistentVegetationCellList[_cellIndex].RemoveVegetationItemInstance(_vegetationItemID, _position, _minimumDistance);
            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveVegetationItemInstance2D(int _cellIndex, string _vegetationItemID, float3 _position, float _minimumDistance)
        {
            if (PersistentVegetationCellList.Count > _cellIndex)
                PersistentVegetationCellList[_cellIndex].RemoveVegetationItemInstance2D(_vegetationItemID, _position, _minimumDistance);
            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public List<PersistentVegetationInstanceInfo> GetPersistentVegetationInstanceInfoList()
        {
            if (_instanceInfoDirty)
            {
                UpdatePersistentVegetationInstanceInfo();
                _instanceInfoDirty = false;
            }

            return PersistentVegetationInstanceInfoList;
        }

        private void UpdatePersistentVegetationInstanceInfo()
        {
            PersistentVegetationInstanceInfoList.Clear();

            for (int i = 0; i < PersistentVegetationCellList.Count; i++)
            {
                PersistentVegetationCell cell = PersistentVegetationCellList[i];
                for (int j = 0; j < cell.PersistentVegetationInfoList.Count; j++)
                {
                    PersistentVegetationInstanceInfo instanceInfo = GetPersistentVegetationInstanceInfo(cell.PersistentVegetationInfoList[j].VegetationItemID);
                    if (instanceInfo == null)
                    {
                        instanceInfo = new PersistentVegetationInstanceInfo { VegetationItemID = cell.PersistentVegetationInfoList[j].VegetationItemID };
                        PersistentVegetationInstanceInfoList.Add(instanceInfo);
                    }

                    instanceInfo.Count += cell.PersistentVegetationInfoList[j].VegetationItemList.Count;
                    instanceInfo.AddSourceCountList(cell.PersistentVegetationInfoList[j].SourceCountList);
                }
            }
        }

        private PersistentVegetationInstanceInfo GetPersistentVegetationInstanceInfo(string _vegetationItemID)
        {
            for (int i = 0; i < PersistentVegetationInstanceInfoList.Count; i++)
                if (PersistentVegetationInstanceInfoList[i].VegetationItemID == _vegetationItemID)
                    return PersistentVegetationInstanceInfoList[i];
            return null;
        }
    }
}