#if UNITY_EDITOR
using AwesomeTechnologies.VegetationSystem;

namespace AwesomeTechnologies.TerrainSystem
{
    public partial class TerrainSystemPro
    {
        public void EnableTerrainHeatmap(bool _value)
        {
            if (vegetationSystemPro == false)
                return;

            if (_value && enableHeatmap == false)
                return;

            if (_value == false)
            {
                for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++)
                    vegetationSystemPro.vegetationStudioTerrainList[i].RestoreTerrainMaterial();

                enableHeatmap = _value;
                return;
            }

            if (_value)
                for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++)
                    vegetationSystemPro.vegetationStudioTerrainList[i].AssignHeatmapMaterial();

            UpdateTerrainHeatmap();
        }

        public void UpdateTerrainHeatmap()
        {
            if (vegetationSystemPro == false)
                return;

            VegetationPackagePro vegetationPackagePro = vegetationSystemPro.vegetationPackageProList[vegetationPackageIndex];
            if (vegetationPackagePro.TerrainTextureSettingsList.Count <= 0)
                return;

            TerrainTextureSettings terrainTextureSettings = vegetationPackagePro.TerrainTextureSettingsList[vegetationPackageTextureIndex];
            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainList.Count; i++)
                vegetationSystemPro.vegetationStudioTerrainList[i].UpdateTerrainMaterial(vegetationSystemPro.systemRelativeSeaLevel, vegetationSystemPro.vegetationSystemBounds.max.y, terrainTextureSettings);
        }
    }
}
#endif