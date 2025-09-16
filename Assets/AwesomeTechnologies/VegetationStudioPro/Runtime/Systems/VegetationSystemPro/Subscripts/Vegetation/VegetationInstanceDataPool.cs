using System.Collections.Generic;
using Unity.Collections;

namespace AwesomeTechnologies.Vegetation
{
    public class VegetationInstanceDataPool
    {
        private readonly List<VegetationInstanceData> vegetationInstanceDataList = new();   // stores pooled objects available for the cell spawner -- pooled items store information from the rules which turn into matrix instances

        public VegetationInstanceData GetObject()
        {
            if (vegetationInstanceDataList.Count <= 0)  // if none exists right now (due to not returned to the pool yet -- due to first call ever)
                return new VegetationInstanceData();    // return a new instance to fill the "gap"

            VegetationInstanceData vegetationInstanceData = vegetationInstanceDataList[vegetationInstanceDataList.Count - 1];   // get ref
            vegetationInstanceDataList.RemoveAtSwapBack(vegetationInstanceDataList.Count - 1);  // then safely remove

            vegetationInstanceData.Clear(); // clear old data -- safety clear

            return vegetationInstanceData;  // else return one in the pool
        }

        public void ReturnObject(VegetationInstanceData _vegetationInstanceData)    // return objects to the pool after the entire loop ran once -- also return after using the utility cell spawning function
        {
            _vegetationInstanceData.CompactMemory();    // free/compact memory
            vegetationInstanceDataList.Add(_vegetationInstanceData);    // re-add to the pool list
        }

        public void Dispose()
        {
            for (int i = 0; i < vegetationInstanceDataList.Count; i++)
                vegetationInstanceDataList[i].Dispose();
            vegetationInstanceDataList.Clear();
        }
    }
}