using AwesomeTechnologies.VegetationStudio;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/RuntimeLoaders/TerrainStreamingLoader")]
    [ScriptExecutionOrder(-101)]
    public class TerrainStreamingLoader : MonoBehaviour
    {
        public bool removeTerrains;
        public Transform floatingOriginAnchor;

        void OnEnable()
        {
            VegetationStudioManager.PrepareTerrainStreaming(removeTerrains, floatingOriginAnchor);
        }
    }
}