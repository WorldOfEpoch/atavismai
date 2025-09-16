using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void AddTerrain(GameObject _go)
        {
            _go.TryGetComponent(out IVegetationStudioTerrain _vst);
            if (_vst == null)
                return;

            if (vegetationStudioTerrainObjectList.Contains(_go) || (RectExtension.CreateRectFromBounds(_vst.TerrainBounds).Overlaps(RectExtension.CreateRectFromBounds(vegetationSystemBounds)) == false && automaticBoundsCalculation == false))
                return; // skip when already added -- when not overlapping with the system's totalArea while "automaticBoundsCalculation" is disabled

            vegetationStudioTerrainObjectList.Add(_go);
            RefreshTerrainInterfaceList();

            if (automaticBoundsCalculation)
                CalculateVegetationSystemBounds();
            else
                RefreshTerrainArea(_vst.TerrainBounds);
        }

        public void AddTerrains(GameObject[] _terrains)
        {
            Bounds combinedBounds = new();
            for (int i = 0; i < _terrains.Length; i++)
            {
                _terrains[i].TryGetComponent(out IVegetationStudioTerrain _vst);
                if (_vst == null)
                    continue;

                if (vegetationStudioTerrainObjectList.Contains(_terrains[i]) || (RectExtension.CreateRectFromBounds(_vst.TerrainBounds).Overlaps(RectExtension.CreateRectFromBounds(vegetationSystemBounds)) == false && automaticBoundsCalculation == false))
                    continue;   // skip when already added -- when not overlapping with the system's totalArea while "automaticBoundsCalculation" is disabled

                vegetationStudioTerrainObjectList.Add(_terrains[i]);

                if (i == 0)
                    combinedBounds = _vst.TerrainBounds;
                else
                    combinedBounds.Encapsulate(_vst.TerrainBounds);
            }

            RefreshTerrainInterfaceList();

            if (automaticBoundsCalculation)
                CalculateVegetationSystemBounds();
            else
                RefreshTerrainArea(combinedBounds);
        }

        public void AddAllUnityTerrains()
        {
            Terrain[] terrains = FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            GameObject[] terrainGOs = new GameObject[terrains.Length];
            for (int i = 0; i < terrains.Length; i++)
            {
                terrains[i].TryGetComponent(out UnityTerrain _unityTerrain);
                if (_unityTerrain == null)
                    terrains[i].gameObject.AddComponent<UnityTerrain>();    // force-add "UnityTerrain" components
                terrainGOs[i] = terrains[i].gameObject;
            }
            AddTerrains(terrainGOs);
        }

        public void AddAllMeshTerrains()
        {
            MeshTerrain[] terrains = FindObjectsByType<MeshTerrain>(FindObjectsSortMode.None);
            GameObject[] terrainGOs = new GameObject[terrains.Length];
            for (int i = 0; i < terrains.Length; i++)
                terrainGOs[i] = terrains[i].gameObject;
            AddTerrains(terrainGOs);
        }

        public void AddAllRaycastTerrains()
        {
            RaycastTerrain[] terrains = FindObjectsByType<RaycastTerrain>(FindObjectsSortMode.None);
            GameObject[] terrainGOs = new GameObject[terrains.Length];
            for (int i = 0; i < terrains.Length; i++)
                terrainGOs[i] = terrains[i].gameObject;
            AddTerrains(terrainGOs);
        }

        public void RemoveAllUnityTerrains()
        {
            RefreshTerrainInterfaceList();  // safety null removal
            List<GameObject> terrainGOs = new();
            for (int i = vegetationStudioTerrainObjectList.Count - 1; i >= 0; i--)
                if (vegetationStudioTerrainObjectList[i].TryGetComponent(out UnityTerrain _terrain))
                    terrainGOs.Add(_terrain.gameObject);
            RemoveTerrains(terrainGOs.ToArray());
        }

        public void RemoveAllMeshTerrains()
        {
            RefreshTerrainInterfaceList();  // safety null removal
            List<GameObject> terrainGOs = new();
            for (int i = vegetationStudioTerrainObjectList.Count - 1; i >= 0; i--)
                if (vegetationStudioTerrainObjectList[i].TryGetComponent(out MeshTerrain _terrain))
                    terrainGOs.Add(_terrain.gameObject);
            RemoveTerrains(terrainGOs.ToArray());
        }

        public void RemoveAllRaycastTerrains()
        {
            RefreshTerrainInterfaceList();  // safety null removal
            List<GameObject> terrainGOs = new();
            for (int i = vegetationStudioTerrainObjectList.Count - 1; i >= 0; i--)
                if (vegetationStudioTerrainObjectList[i].TryGetComponent(out RaycastTerrain _terrain))
                    terrainGOs.Add(_terrain.gameObject);
            RemoveTerrains(terrainGOs.ToArray());
        }

        public void RemoveTerrain(GameObject _go)
        {
            if (vegetationStudioTerrainObjectList.Count <= 0)
                return;

            if (vegetationStudioTerrainObjectList.Contains(_go))
                vegetationStudioTerrainObjectList.Remove(_go);
            RefreshTerrainInterfaceList();

            if (automaticBoundsCalculation)
                CalculateVegetationSystemBounds();
            else
            {
                if (_go != null)
                {
                    _go.TryGetComponent(out IVegetationStudioTerrain _vst);
                    if (_vst != null)
                        RefreshTerrainArea(_vst.TerrainBounds);
                }
            }
        }

        public void RemoveTerrains(GameObject[] _terrains)
        {
            if (vegetationStudioTerrainObjectList.Count <= 0)
                return;

            for (int i = 0; i < _terrains.Length; i++)
                if (vegetationStudioTerrainObjectList.Contains(_terrains[i]))
                    vegetationStudioTerrainObjectList.Remove(_terrains[i]);
            RefreshTerrainInterfaceList();

            if (automaticBoundsCalculation)
                CalculateVegetationSystemBounds();
            else
                RefreshTerrainArea();
        }

        public void RemoveAllTerrains(bool _removeEmtpy = false)
        {
            if (vegetationStudioTerrainObjectList.Count <= 0)
                return;

            if (_removeEmtpy)
                RefreshTerrainInterfaceList();
            else
            {
                for (int i = vegetationStudioTerrainObjectList.Count - 1; i >= 0; i--)
                    vegetationStudioTerrainObjectList.RemoveAt(i);
                RefreshTerrainInterfaceList();
            }

            if (automaticBoundsCalculation)
                CalculateVegetationSystemBounds();
            else
                RefreshTerrainArea();
        }

        public void RefreshTerrainInterfaceList()
        {
            for (int i = vegetationStudioTerrainObjectList.Count - 1; i >= 0; i--)  // remove all terrains that have a "null" gameObject 
                if (vegetationStudioTerrainObjectList[i] == null)
                    vegetationStudioTerrainObjectList.RemoveAt(i);

            vegetationStudioTerrainList.Clear();    // clear current interface list
            for (int i = 0; i < vegetationStudioTerrainObjectList.Count; i++)   // re-fill the interface list based on the current gameObject list > which just got filtered
            {
                vegetationStudioTerrainObjectList[i].TryGetComponent(out IVegetationStudioTerrain _vst);
                if (_vst != null)
                    vegetationStudioTerrainList.Add(_vst);
            }
        }

        public void CalculateVegetationSystemBounds(bool _refreshTerrainData = true, bool _refreshCellSystem = true)
        {
            Bounds newSystemBounds = new();
            for (int i = 0; i < vegetationStudioTerrainObjectList.Count; i++)
            {
                vegetationStudioTerrainObjectList[i].TryGetComponent(out IVegetationStudioTerrain _vst);
                if (_vst == null)
                    continue;

                if (_refreshTerrainData)
                {
                    if (_vst is MeshTerrain _meshTerrain)
                        _meshTerrain.GenerateMeshTerrain(false);
                    _vst.RefreshTerrainData();
                }

                if (automaticBoundsCalculation)
                {
                    if (i == 0)
                        newSystemBounds = _vst.TerrainBounds;
                    else
                        newSystemBounds.Encapsulate(_vst.TerrainBounds);

                    vegetationSystemBounds = newSystemBounds;
                }
            }

            if (vegetationStudioTerrainObjectList.Count <= 0)
                vegetationSystemBounds = newSystemBounds;

            if (_refreshCellSystem)
                RefreshCellSystem();
        }

        public void RefreshTerrainHeightmap(bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            for (int i = 0; i < vegetationStudioTerrainList.Count; i++)
                vegetationStudioTerrainList[i].RefreshTerrainData(_splatData, _holesData, _heightsData);
        }

        public void RefreshTerrainHeightmap(Bounds _bounds, bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            for (int i = 0; i < vegetationStudioTerrainList.Count; i++)
                vegetationStudioTerrainList[i].RefreshTerrainData(_bounds, _splatData, _holesData, _heightsData);
        }

        public void VerifySplatmapAccess()   // called before the vegetation cell loading -- utility function to avoid "out of sync" splat map / texture mask data
        {
            /// when doing at run-time splat map painting / texture mask generation ..also in builds
            /// -> move the for loop out of of all IF statements
            /// ==> or rather call the for loop manually for performance/function flow reasons
            /// 

#if UNITY_EDITOR
            if (Application.isPlaying == false) // not called at run-time to not mess with "Profiler" stats and other performance related things ==> does result in errors depending on what's going on ex: CTRL+Z operations especially
            {
                for (int i = 0; i < vegetationStudioTerrainList.Count; i++)
                    vegetationStudioTerrainList[i].VerifySplatmapAccess();  // verify and refresh underlying splat map data ==> to keep compatibility with hand/scripted painting
            }
#endif
        }

        public void OnHeightChanged(Terrain _terrain, RectInt _heightRegion, bool _synched) // even function subscribed to engine event
        {
            if (_synched == false || enableAutoSystemRefresh == false)
                return;

            _terrain.TryGetComponent(out UnityTerrain _ut);
            if (_ut == null) return;

            CalculateVegetationSystemBounds(false, false);  // only update "total area" without refreshing other data
            _ut.OnHeightChanged(this, _heightRegion);   // then update based on the the terrain's area ..terrain's data > clear cells > clear cell culling > resize cell Y-Axis
        }

        public void OnTextureChanged(Terrain _terrain, string _textureName, RectInt _texelRegion, bool _synched)    // even function subscribed to engine event
        {
            if (_synched == false || enableAutoSystemRefresh == false)
                return;

            _terrain.TryGetComponent(out UnityTerrain _ut);
            if (_ut == null) return;

            _ut.OnTextureChanged(this, _textureName, _texelRegion);
        }
    }
}