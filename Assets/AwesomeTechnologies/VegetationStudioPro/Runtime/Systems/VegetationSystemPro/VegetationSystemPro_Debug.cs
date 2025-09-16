#if UNITY_EDITOR
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationStudio;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public enum ECellCullingDebugMode
        {
            Disabled,
            CellSampling,
            CellCulling
        }

        void DrawSeaLevel()
        {
            Gizmos.color = new Color(0, 0, 0.8f, 0.4f);
            Gizmos.DrawCube(new float3(vegetationSystemBounds.center.x, systemRelativeSeaLevel, vegetationSystemBounds.center.z), new float3(vegetationSystemBounds.size.x, 0, vegetationSystemBounds.size.z));
            Gizmos.DrawWireCube(vegetationSystemBounds.center, vegetationSystemBounds.size);
        }

        void DrawTextureMaskAreas()
        {
            if (debugTextureMask == null)
                return;

            float3 center = new(debugTextureMask.TextureRect.center.x, vegetationSystemBounds.center.y, debugTextureMask.TextureRect.center.y);
            float3 size = new(debugTextureMask.TextureRect.width, vegetationSystemBounds.size.y, debugTextureMask.TextureRect.height);
            Gizmos.color = new Color(0, 0.8f, 0.8f, 0.4f);
            Gizmos.DrawCube(center, size);
        }

        void DrawMeshTerrainAreas()
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < vegetationStudioTerrainObjectList.Count; i++)
                if (vegetationStudioTerrainObjectList[i] != null && vegetationStudioTerrainObjectList[i].TryGetComponent(out MeshTerrain _meshTerrain))
                    Gizmos.DrawWireCube(_meshTerrain.TerrainBounds.center, _meshTerrain.TerrainBounds.size);
        }

        void DrawRaycastTerrainAreas()
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < vegetationStudioTerrainObjectList.Count; i++)
                if (vegetationStudioTerrainObjectList[i] != null && vegetationStudioTerrainObjectList[i].TryGetComponent(out RaycastTerrain _raycastTerrain))
                    Gizmos.DrawWireCube(_raycastTerrain.RaycastTerrainBounds.center + _raycastTerrain.transform.position + VegetationStudioManager.GetFloatingOriginOffset(), _raycastTerrain.RaycastTerrainBounds.size);
        }

        public Color GetCellGizmoColor(bool _isPreload, int _distanceBand)
        {
            return _distanceBand switch
            {
                0 => _isPreload ? Color.cyan : Color.green, // type "0" cells
                1 => _isPreload ? Color.white : Color.black, // type "1" cells
                2 => Color.yellow,  // shadow culling
                _ => Color.red, // unloaded/cleared/undefined cells
            };
        }

        void OnDrawGizmos()
        {
            if (enabled == false)
                return;

            if (showSeaLevel)
                DrawSeaLevel();

            if (showSystemTotalArea)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(vegetationSystemBounds.center, vegetationSystemBounds.size);
            }

            if (showAllMeshTerrainAreas)
                DrawMeshTerrainAreas();

            if (showAllRaycastTerrainAreas)
                DrawRaycastTerrainAreas();

            VegetationStudioManager.ShowBiomes = showBiomeMasks;

            if (showTextureMaskAreas)
                DrawTextureMaskAreas();

            if (showBiomeMaskCells)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < vegetationCellList.Count; i++)
                    if (vegetationCellList[i].Enabled && vegetationCellList[i].biomeMaskList != null && vegetationCellList[i].biomeMaskList.Count > 0)
                        Gizmos.DrawWireCube(vegetationCellList[i].cellBounds.center, vegetationCellList[i].cellBounds.size + Vector3.up * 5);
            }

            if (showVegetationMaskCells)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i < vegetationCellList.Count; i++)
                    if (vegetationCellList[i].Enabled && vegetationCellList[i].vegetationMaskList != null && vegetationCellList[i].vegetationMaskList.Count > 0)
                        Gizmos.DrawWireCube(vegetationCellList[i].cellBounds.center, vegetationCellList[i].cellBounds.size + Vector3.up * 10);
            }

            if (showVegetationCells)
                for (int i = 0; i < vegetationCellList.Count; i++)
                    if (vegetationCellList[i].Enabled)
                    {
                        Gizmos.color = GetCellGizmoColor(true, vegetationCellList[i].loadedDistanceBand);
                        Gizmos.DrawWireCube(vegetationCellList[i].cellBounds.center, vegetationCellList[i].cellBounds.size);
                    }

            if (showPredictiveVegetationCells)
                for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                    vegetationStudioCameraList[i].DrawPredictiveVegetationCellGizmos();

            if (showVisibleVegetationCells != ECellCullingDebugMode.Disabled)
                for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                    vegetationStudioCameraList[i].DrawVisibleVegetationCellGizmos(showVisibleVegetationCells);

            if (showBillboardCells)
            {
                for (int i = 0; i < billboardCellList.Count; i++)
                {
                    Gizmos.color = GetCellGizmoColor(true, billboardCellList[i].loadedState == 3 ? 1 : 99);
                    Gizmos.DrawWireCube(billboardCellList[i].cellBounds.center, billboardCellList[i].cellBounds.size);
                }
            }

            if (showPredictiveBillboardCells)
                for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                    vegetationStudioCameraList[i].DrawPredictiveBillboardCellGizmos();

            if (showVisibleBillboardCells)
                for (int i = 0; i < vegetationStudioCameraList.Count; i++)
                    vegetationStudioCameraList[i].DrawVisibleBillboardCellGizmos();
        }
    }
}
#endif