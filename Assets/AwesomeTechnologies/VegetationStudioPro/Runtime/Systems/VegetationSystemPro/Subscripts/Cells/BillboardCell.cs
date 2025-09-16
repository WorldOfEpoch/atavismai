using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class BillboardCell : IHasRect
    {
        public Bounds cellBounds;   // bounds of the cell -- used for several things ex: cell culling, gathering of overlapping vegetation cells, rendering batching
        public int index;   // assigned index in the list after creation
        public List<VegetationPackageBillboardInstance> vegetationPackageBillboardInstanceList = new(); // stores "instanced" vegetation packages and their "instances"
        public List<VegetationCell> overlapVegetationCells = new(); // stores overlapping vegetation cells temporarily while creating persistent data for rendering all billboards
        public bool prepared;   // whether internal data has been prepared -- whether lists have been created for all vegetation packages and their vegetation items
        public int loadedState; // 0 = (partially) unloaded -- 1 = loaded overlapped vegetationCells -- 2 = loaded mesh-data -- 3 = loaded final "billboard-cell-mesh"
        public bool flagForRemoval = false; // whether to clear the cell's cache when out of (pre-)load range

        public BillboardCell(Rect _rectangle, float _centerY, float _sizeY)
        {
            cellBounds = RectExtension.CreateBoundsFromRect(_rectangle, _centerY, _sizeY);
        }

        public Rect Rectangle
        {
            get { return RectExtension.CreateRectFromBounds(cellBounds); }
            set { cellBounds = RectExtension.CreateBoundsFromRect(value); }
        }

        public void PrepareBillboardCell(List<VegetationPackagePro> _vegetationPackageProList)
        {   // create new instances for each vegetation package which in turn create their internal instances
            Dispose();  // pre-reset for "intermediate" refreshes
            for (int i = 0; i < _vegetationPackageProList.Count; i++)   // for all vegetation packages added to the system
                vegetationPackageBillboardInstanceList.Add(new VegetationPackageBillboardInstance(_vegetationPackageProList[i].VegetationInfoList.Count));
            prepared = true;    // set as finished loading internal data so it can be referenced and/or cleared
        }

        public void ClearCache()
        {
            for (int i = 0; i < vegetationPackageBillboardInstanceList.Count; i++)
                vegetationPackageBillboardInstanceList[i].ClearCache();
            loadedState = 0;    // reset to re-start billboard cell loading => re-generate all billboards
        }

        public void ClearCache(int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (_vegetationPackageIndex < vegetationPackageBillboardInstanceList.Count)
                if (_vegetationItemIndex < vegetationPackageBillboardInstanceList[_vegetationPackageIndex].billboardInstanceList.Count)
                {
                    vegetationPackageBillboardInstanceList[_vegetationPackageIndex].billboardInstanceList[_vegetationItemIndex].ClearCache();
                    loadedState = 0;    // reset to re-start billboard cell loading => re-generate billboard of this vegetation item
                }
        }

        public void Dispose()
        {
            for (int i = 0; i < vegetationPackageBillboardInstanceList.Count; i++)
                vegetationPackageBillboardInstanceList[i].Dispose();
            vegetationPackageBillboardInstanceList.Clear();
        }
    }
}