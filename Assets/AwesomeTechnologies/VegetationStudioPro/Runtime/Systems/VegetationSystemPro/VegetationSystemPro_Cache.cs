using Unity.Collections;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void CompactVegetationCellCache()
        {
            if (useCompactCache == false)
                return;

            for (int i = 0; i < loadedVegetationCellList.Count; i++)    // for each loaded cell
                loadedVegetationCellList[i].flagForRemoval = loadedVegetationCellList[i].prepared;  // set to be cleared based on whether they have been fully created/initialized

            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // for each camera
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false)
                    continue;

                for (int j = 0; j < vegetationStudioCameraList[i].preloadVegetationCellList.Count; j++) // for all cells within max vegetation distance + pre-load offset
                    vegetationStudioCameraList[i].preloadVegetationCellList[j].flagForRemoval = false;  // re-override to be not cleared for cells still in range -- that are potentially still visible
            }

            predictiveCellLoader.RemoveVegetationCellsFlaggedForRemoval();  // clear predictive pre-loaded cell list to not re-load a cell

            for (int i = loadedVegetationCellList.Count - 1; i >= 0; i--)   // for each loaded cell -- loop backwards
            {
                if (loadedVegetationCellList[i].flagForRemoval == false)    // after both overrides -- safety check whether still potentially visible
                    continue;

                for (int j = 0; j < vegetationPackageProList.Count; j++)    // for each vegetation package
                    for (int k = 0; k < vegetationPackageProList[j].VegetationInfoList.Count; k++)  // for each vegetation item
                    {
                        VegetationItemInfoPro vegItemInfoPro = GetVegetationItemModelInfo(j, k).vegetationItemInfo; // skip clearing when not enabled
                        if (toggleCompactCacheGrass == false && vegItemInfoPro.VegetationType == VegetationType.Grass) continue;
                        if (toggleCompactCachePlants == false && vegItemInfoPro.VegetationType == VegetationType.Plant) continue;
                        if (toggleCompactCacheObjects == false && vegItemInfoPro.VegetationType == VegetationType.Objects) continue;
                        if (toggleCompactCacheLargeObjects == false && vegItemInfoPro.VegetationType == VegetationType.LargeObjects) continue;
                        if (toggleCompactCacheTrees == false && vegItemInfoPro.VegetationType == VegetationType.Tree) continue;
                        loadedVegetationCellList[i].ClearCache(j, k);   // clear the cache of the vegetation item
                    }

                OnClearCacheVegetationCellDelegate?.Invoke(this, loadedVegetationCellList[i]);  // notify the collider system / runtime prefab system
                loadedVegetationCellList.RemoveAtSwapBack(i);   // remove them safely from the list
            }
        }

        private void CompactBillboardCellCache()
        {
            if (useCompactCache == false || toggleCompactCacheBillboards == false)
                return;

            for (int i = 0; i < loadedBillboardCellList.Count; i++) // for each loaded cell
                loadedBillboardCellList[i].flagForRemoval = loadedBillboardCellList[i].prepared;    // set to be cleared based on whether they have been fully created/initialized

            for (int i = 0; i < vegetationStudioCameraList.Count; i++)  // for each camera
            {
                if (vegetationStudioCameraList[i].IsEnabled() == false)
                    continue;

                for (int j = 0; j < vegetationStudioCameraList[i].preloadBillboardCellList.Count; j++)  // for all cells within billboard distance + pre-load offset
                    vegetationStudioCameraList[i].preloadBillboardCellList[j].flagForRemoval = false;   // re-override to be not cleared for cells still in range -- that are potentially still visible
            }

            predictiveCellLoader.RemoveBillboardCellsFlaggedForRemoval();   // clear predictive pre-loaded cell list to not re-load a cell

            for (int i = loadedBillboardCellList.Count - 1; i >= 0; i--)    // for each loaded cell -- loop backwards
            {
                if (loadedBillboardCellList[i].flagForRemoval == false) // after both overrides -- safety check whether still potentially visible
                    continue;

                loadedBillboardCellList[i].ClearCache();    // clear the cache of the billboard cell
                loadedBillboardCellList.RemoveAtSwapBack(i);    // remove them safely from the list
            }
        }
    }
}