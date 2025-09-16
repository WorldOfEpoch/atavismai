using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void SetupVegetationItemModelData()
        {
            for (int i = 0; i < vegetationPackageProList.Count; i++)
                vegetationPackageProModelsList.Add(new(this, vegetationPackageProList[i]));
        }

        public void SetupCameraDataPerModel()   // called when adding/removing cameras
        {
            for (int i = 0; i < vegetationPackageProModelsList.Count; i++)
                vegetationPackageProModelsList[i].PrepareRenderListsPerModel();
        }

        public void UpdateCellCullingBoundsAddy()   // called when an item changed its scale
        {
            float3 widestBoundsAddy = new();
            Bounds maxCullingBoundsAddy = new();
            for (int i = 0; i < vegetationPackageProModelsList.Count; i++)
            {
                Bounds packageMaxAddy = vegetationPackageProModelsList[i].GetMaxCullingBoundsAddy();
                widestBoundsAddy.x = packageMaxAddy.size.x > widestBoundsAddy.x ? packageMaxAddy.size.x : widestBoundsAddy.x;
                widestBoundsAddy.y = packageMaxAddy.size.y > widestBoundsAddy.y ? packageMaxAddy.size.y : widestBoundsAddy.y;
                widestBoundsAddy.z = packageMaxAddy.size.z > widestBoundsAddy.z ? packageMaxAddy.size.z : widestBoundsAddy.z;
                maxCullingBoundsAddy.size = new(math.max(widestBoundsAddy.x, widestBoundsAddy.z), widestBoundsAddy.y, math.max(widestBoundsAddy.x, widestBoundsAddy.z));
            }

            // biggest "cullingBoundsAddy" of all packages and items -- used for accurate cell culling
            cellCullingBoundsAddy = maxCullingBoundsAddy;
        }

        public void UpdateLODCounts()
        {
            for (int i = 0; i < vegetationPackageProModelsList.Count; i++)
                for (int j = 0; j < vegetationPackageProModelsList[i].vegetationItemModelList.Count; j++)
                    vegetationPackageProModelsList[i].vegetationItemModelList[j].UpdateLODCount();
        }

        public void RefreshMaterials()
        {
            for (int i = 0; i < vegetationPackageProModelsList.Count; i++)
                for (int j = 0; j < vegetationPackageProModelsList[i].vegetationItemModelList.Count; j++)
                    vegetationPackageProModelsList[i].vegetationItemModelList[j].RefreshMaterials();
        }

        public void ClearMaterials()
        {
            for (int i = 0; i < vegetationPackageProModelsList.Count; i++)
                for (int j = 0; j < vegetationPackageProModelsList[i].vegetationItemModelList.Count; j++)
                    vegetationPackageProModelsList[i].vegetationItemModelList[j].ClearMaterials();
        }

        public VegetationItemModelInfo GetVegetationItemModelInfo(string _vegetationItemID)
        {
            VegetationItemIndices vegetationItemIndices = GetVegetationItemIndices(_vegetationItemID);
            return GetVegetationItemModelInfo(vegetationItemIndices.vegetationPackageIndex, vegetationItemIndices.vegetationItemIndex);
        }

        public VegetationItemModelInfo GetVegetationItemModelInfo(int _vegetationPackageIndex, int _vegetationItemIndex)
        {
            if (_vegetationPackageIndex == -1 || _vegetationItemIndex == -1)
                return null;

            if (_vegetationPackageIndex < vegetationPackageProModelsList.Count)
                if (_vegetationItemIndex < vegetationPackageProModelsList[_vegetationPackageIndex].vegetationItemModelList.Count)
                    return vegetationPackageProModelsList[_vegetationPackageIndex].vegetationItemModelList[_vegetationItemIndex];
            return null;
        }

        private void DisposeVegetationItemModelData()
        {
            for (int i = 0; i < vegetationPackageProModelsList.Count; i++)
                vegetationPackageProModelsList[i].Dispose();
            vegetationPackageProModelsList.Clear();

            renderingMaterials.Clear();
            cellCullingBoundsAddy = new();
            shouldForceUpdateCellCulling = true;
        }
    }
}