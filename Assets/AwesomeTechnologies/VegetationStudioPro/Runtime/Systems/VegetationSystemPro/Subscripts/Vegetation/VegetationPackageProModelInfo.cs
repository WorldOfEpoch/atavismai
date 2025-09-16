using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationPackageProModelInfo
    {
        public readonly List<VegetationItemModelInfo> vegetationItemModelList = new();

        public VegetationPackageProModelInfo(VegetationSystemPro _vegetationSystemPro, VegetationPackagePro _vegetationPackagePro)
        {
            for (int i = 0; i < _vegetationPackagePro.VegetationInfoList.Count; i++)
                vegetationItemModelList.Add(new(_vegetationSystemPro, _vegetationPackagePro.VegetationInfoList[i]));
        }

        public Bounds GetMaxCullingBoundsAddy()
        {
            /// return biggest "cullingBoundsAddy" in the list of the current package -- used for accurate cell culling
            /// -> to further compare with potentially other existing packages
            /// 

            float3 widestBoundsAddy = new();
            Bounds maxCullingBoundsAddy = new();
            for (int i = 0; i < vegetationItemModelList.Count; i++)
            {
                widestBoundsAddy.x = vegetationItemModelList[i].cullingBoundsAddy.size.x > widestBoundsAddy.x ? vegetationItemModelList[i].cullingBoundsAddy.size.x : widestBoundsAddy.x;
                widestBoundsAddy.y = vegetationItemModelList[i].cullingBoundsAddy.size.y > widestBoundsAddy.y ? vegetationItemModelList[i].cullingBoundsAddy.size.y : widestBoundsAddy.y;
                widestBoundsAddy.z = vegetationItemModelList[i].cullingBoundsAddy.size.z > widestBoundsAddy.z ? vegetationItemModelList[i].cullingBoundsAddy.size.z : widestBoundsAddy.z;
                maxCullingBoundsAddy.size = new(math.max(widestBoundsAddy.x, widestBoundsAddy.z), widestBoundsAddy.y, math.max(widestBoundsAddy.x, widestBoundsAddy.z));
            }
            return maxCullingBoundsAddy;
        }

        public void PrepareRenderListsPerModel()
        {
            for (int i = 0; i < vegetationItemModelList.Count; i++)
                vegetationItemModelList[i].PrepareRenderLists();
        }

        public void Dispose()
        {
            for (int i = 0; i < vegetationItemModelList.Count; i++)
                vegetationItemModelList[i].Dispose();
            vegetationItemModelList.Clear();
        }
    }
}