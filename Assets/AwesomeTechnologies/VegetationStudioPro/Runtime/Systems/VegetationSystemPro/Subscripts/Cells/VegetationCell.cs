using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationCell : IHasRect
    {
        public Bounds cellBounds;   // bounds of the cell -- used for several things ex: cell culling, cell spawning, rendering batching
        public int index;   // assigned index in the list after creation
        public readonly List<VegetationPackageInstance> vegetationPackageInstanceList = new();  // stores "instanced" vegetation packages and their "instances"
        public readonly List<VegetationInstanceData> vegetationInstanceDataList = new();    // stores data (end result of all rules) of all instances

        public bool prepared;   // whether internal data has been prepared -- whether lists have been created for all vegetation packages and their vegetation items
        public int loadedDistanceBand = 99; // 99 = "unloaded" -- 0 = "grass/plants/objects -- 1 = "large objects, trees"
        //public bool important = false;  // not used / set to true internally currently
        public bool flagForRemoval = false; // whether to clear the cell's cache when out of (pre-)load range

        public List<PolygonMaskBiome> biomeMaskList;    // stores references to overlapping biome masks ..assigned from the "VegetationStudioManager"
        public List<BaseMaskArea> vegetationMaskList;   // stores references to overlapping dynamic masks ..assigned from the "VegetationStudioManager"

        public bool Enabled => cellBounds.center.y > -100000;   // evaluate whether to use this cell -- usually true unless below sea level

        public int EnabledInt => cellBounds.center.y > -100000 ? 1 : 0; // evaluate whether to use this cell -- usually true unless below sea level

        public VegetationCell(Rect _rectangle)
        {
            cellBounds = RectExtension.CreateBoundsFromRect(_rectangle, -100000);   // set to "-100000" to "disable" the cell => gets assigned a real value when actually creating and positioning cells ..unless below sea level
        }

        public Rect Rectangle
        {
            get { return RectExtension.CreateRectFromBounds(cellBounds); }
            set { cellBounds = RectExtension.CreateBoundsFromRect(value); }
        }

        public void PrepareVegetationCell(List<VegetationPackagePro> _vegetationPackageProList)
        {   // create new instances for each vegetation package which in turn create their internal instances
            Dispose(false); // pre-reset for "intermediate" refreshes
            int itemCount = 0;
            for (int i = 0; i < _vegetationPackageProList.Count; i++)   // for all vegetation packages added to the system
            {
                itemCount += _vegetationPackageProList[i].VegetationInfoList.Count; // get total count of vegetation items across all vegetation packages
                vegetationPackageInstanceList.Add(new VegetationPackageInstance(_vegetationPackageProList[i].VegetationInfoList.Count));
            }

            vegetationInstanceDataList.Capacity = itemCount;    // set/prepare capacity based on total item count possible
            prepared = true;    // set as finished loading internal data so it can be referenced and/or cleared
        }

        public NativeList<MatrixInstance> GetVegetationPackageInstancesList(int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (_vegetationPackageIndex == -1 || _vegetationItemIndex == -1)
                return new NativeList<MatrixInstance>();

            if (vegetationPackageInstanceList.Count > _vegetationPackageIndex)
                if (vegetationPackageInstanceList[_vegetationPackageIndex].matrixInstanceList.Count > _vegetationItemIndex)
                    return vegetationPackageInstanceList[_vegetationPackageIndex].matrixInstanceList[_vegetationItemIndex];
            return new NativeList<MatrixInstance>();
        }

        public void ClearCache()    // clear cache for all vegetation items -- free memory -- declare as "recreationable"
        {
            for (int i = 0; i < vegetationPackageInstanceList.Count; i++)
                for (int j = 0; j < vegetationPackageInstanceList[i].matrixInstanceList.Count; j++)
                    vegetationPackageInstanceList[i].ClearInstanceMemory(j);
            loadedDistanceBand = 99;    // set cell as "unloaded" / "recreationable"
        }

        public void ClearCache(int _vegetationPackageIndex, int _vegetationItemIndex)   // clear cache for a specific vegetation item -- free memory -- declare as "recreationable"
        {
            if (vegetationPackageInstanceList.Count > _vegetationPackageIndex && vegetationPackageInstanceList[_vegetationPackageIndex].loadStateList.Length > _vegetationItemIndex)
                vegetationPackageInstanceList[_vegetationPackageIndex].ClearInstanceMemory(_vegetationItemIndex);
            loadedDistanceBand = 99;    // set cell as "unloaded" / "recreationable"
        }

        public void Dispose(bool _clearMasks = true)
        {
            for (int i = 0; i < vegetationPackageInstanceList.Count; i++)   // "persistent" vegetation item data
                vegetationPackageInstanceList[i].Dispose();
            vegetationPackageInstanceList.Clear();

            for (int i = 0; i < vegetationInstanceDataList.Count; i++)  // "temporary" vegetation item data
                vegetationInstanceDataList[i].Dispose();
            vegetationInstanceDataList.Clear();

            if (_clearMasks)
                ClearMasks();
        }

        public void ClearMasks()
        {
            if (biomeMaskList != null)  // biome mask clear
            {
                for (int i = 0; i < biomeMaskList.Count; i++)
                    biomeMaskList[i].OnMaskDeleteDelegate -= OnBiomeMaskDelete;
                biomeMaskList.Clear();
            }

            if (vegetationMaskList != null) // dynamic mask clear
            {
                for (int i = 0; i < vegetationMaskList.Count; i++)
                    vegetationMaskList[i].OnMaskDeleteDelegate -= OnVegetationMaskDelete;
                vegetationMaskList.Clear();
            }
        }

        public void AddBiomeMask(PolygonMaskBiome _maskArea)
        {
            if (biomeMaskList == null)
                biomeMaskList = new List<PolygonMaskBiome>();
            else if (biomeMaskList.Contains(_maskArea))
                return;

            biomeMaskList.Add(_maskArea);

            if (biomeMaskList.Count > 1)
                biomeMaskList.Sort(new BiomeMaskSortOrderComparer());

            _maskArea.OnMaskDeleteDelegate += OnBiomeMaskDelete;
            ClearCache();
        }

        public bool HasBiome(BiomeType _biomeType)
        {
            if (biomeMaskList == null)
                return false;

            for (int i = 0; i < biomeMaskList.Count; i++)
                if (biomeMaskList[i].BiomeType == _biomeType)
                    return true;
            return false;
        }

        private void OnBiomeMaskDelete(PolygonMaskBiome _maskArea)
        {
            _maskArea.OnMaskDeleteDelegate -= OnBiomeMaskDelete;
            biomeMaskList?.Remove(_maskArea);
            ClearCache();
        }

        public void AddVegetationMask(BaseMaskArea _maskArea)
        {
            if (vegetationMaskList == null)
                vegetationMaskList = new List<BaseMaskArea>();
            else if (vegetationMaskList.Contains(_maskArea))
                return;

            vegetationMaskList.Add(_maskArea);

            _maskArea.OnMaskDeleteDelegate += OnVegetationMaskDelete;
            ClearCache();
        }

        public void AddVegetationMask(BaseMaskArea _maskArea, int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (vegetationMaskList == null)
                vegetationMaskList = new List<BaseMaskArea>();
            else if (vegetationMaskList.Contains(_maskArea))
                return;

            vegetationMaskList.Add(_maskArea);

            _maskArea.OnMaskDeleteDelegate += OnVegetationMaskDelete;
            ClearCache(_vegetationPackageIndex, _vegetationItemIndex);
        }

        private void OnVegetationMaskDelete(BaseMaskArea _maskArea)
        {
            _maskArea.OnMaskDeleteDelegate -= OnVegetationMaskDelete;
            vegetationMaskList?.Remove(_maskArea);
            ClearCache();
        }
    }
}