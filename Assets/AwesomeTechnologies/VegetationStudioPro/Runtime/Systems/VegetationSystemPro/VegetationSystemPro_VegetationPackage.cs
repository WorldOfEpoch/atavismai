using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void AddVegetationPackage(VegetationPackagePro _vegetationPackagePro)    // validate and add the given vegetation package
        {
            if (vegetationPackageProList.Contains(_vegetationPackagePro) == false)
                vegetationPackageProList.Add(_vegetationPackagePro);
        }

        public void RemoveVegetationPackage(VegetationPackagePro _vegetationPackagePro) // remove the given vegetation package
        {
            vegetationPackageProList.Remove(_vegetationPackagePro);
        }

        public VegetationPackagePro GetVegetationPackageFromBiome(BiomeType _biomeType) // return the first found vegetation package matching the given biome type
        {
            for (int i = 0; i < vegetationPackageProList.Count; i++)
                if (vegetationPackageProList[i].BiomeType == _biomeType)
                    return vegetationPackageProList[i];
            return null;
        }

        public int GetMaxVegetationPackageItemCount()   // return highest count of vegetation items possible of the registered vegetation packages
        {
            int itemCount = 0;
            for (int i = 0; i < vegetationPackageProList.Count; i++)
                itemCount = math.max(vegetationPackageProList[i].VegetationInfoList.Count, itemCount);
            return itemCount;
        }

        public List<BiomeType> GetAdditionalBiomeList() // return a list of all registered biomes that aren't the default biome
        {
            List<BiomeType> additionalBiomeList = new();
            for (int i = 0; i < vegetationPackageProList.Count; i++)
                if (vegetationPackageProList[i].BiomeType != BiomeType.Default)
                    additionalBiomeList.Add(vegetationPackageProList[i].BiomeType);
            return additionalBiomeList.Distinct().ToList(); // remove duplicates
        }

        public int GetBiomeSortOrder(BiomeType _biomeType)  // return the sort order of the first found vegetation package matching the given biome type
        {
            for (int i = 0; i < vegetationPackageProList.Count; i++)
                if (vegetationPackageProList[i].BiomeType == _biomeType)
                    return vegetationPackageProList[i].BiomeSortOrder;
            return 1;
        }

        public VegetationItemInfoPro GetVegetationItemInfo(string _vegetationItemID)    // get a certain vegetation item using its ID -> package ID + item ID
        {
            VegetationItemIndices indices = GetVegetationItemIndices(_vegetationItemID);
            if (indices.vegetationPackageIndex > -1 && indices.vegetationItemIndex > -1)
                return vegetationPackageProList[indices.vegetationPackageIndex].VegetationInfoList[indices.vegetationItemIndex];
            return null;
        }
    }
}