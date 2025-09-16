using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public class SelectedVegetationCell
    {
        public readonly VegetationCell vegetationCell;  // used for gizmo drawing -- used for collider / runtime prefab system
        public int cameraCount; // used for gizmo coloring per camera

        private readonly List<VegetationStudioCamera> vegetationStudioCameraList = new();   // safety check list more or less

        public SelectedVegetationCell(VegetationCell _vegetationCell, VegetationStudioCamera _vegetationStudioCamera)
        {
            vegetationCell = _vegetationCell;
            cameraCount = 0;
            AddCameraReference(_vegetationStudioCamera);
        }

        public void AddCameraReference(VegetationStudioCamera _vegetationStudioCamera)
        {
            if (vegetationStudioCameraList.Contains(_vegetationStudioCamera))
                return;

            cameraCount++;
            vegetationStudioCameraList.Add(_vegetationStudioCamera);
        }

        public void RemoveCameraReference(VegetationStudioCamera vegetationStudioCamera)
        {
            if (vegetationStudioCameraList.Contains(vegetationStudioCamera) == false)
                return;

            vegetationStudioCameraList.Remove(vegetationStudioCamera);
            cameraCount--;
        }
    }

    public class VisibleVegetationCellSelector  // used for collider / runtime prefab system
    {
        private VegetationSystemPro vegetationSystemPro;
        [NonSerialized] public readonly List<SelectedVegetationCell> visibleSelectorVegetationCellList = new();

        public delegate void MultiOnVegetationCellVisibleDelegate(VegetationCell _vegetationCell);
        public MultiOnVegetationCellVisibleDelegate OnVegetationCellVisibleDelegate;

        public delegate void MultiOnVegetationCellInvisibleDelegate(VegetationCell _vegetationCell);
        public MultiOnVegetationCellInvisibleDelegate OnVegetationCellInvisibleDelegate;

        public void Init(VegetationSystemPro _vegetationSystemPro)
        {
            vegetationSystemPro = _vegetationSystemPro;
            vegetationSystemPro.OnAddCameraDelegate += OnAddCamera;
            vegetationSystemPro.OnRemoveCameraDelegate += OnRemoveCamera;

            AddVisibleVegetationCells();
        }

        private void AddVisibleVegetationCells()
        {
            for (int i = 0; i < vegetationSystemPro.vegetationStudioCameraList.Count; i++)
                OnAddCamera(vegetationSystemPro.vegetationStudioCameraList[i]);
        }

        private SelectedVegetationCell GetSelectorVegetationCell(VegetationCell _vegetationCell)
        {
            for (int i = 0; i < visibleSelectorVegetationCellList.Count; i++)
                if (visibleSelectorVegetationCellList[i].vegetationCell == _vegetationCell)
                    return visibleSelectorVegetationCellList[i];
            return null;
        }

        private void AddVisibleCellsFromCamera(VegetationStudioCamera _vegetationStudioCamera)
        {
            if (_vegetationStudioCamera.vegetationCullingGroup == null)
                return;

            for (int i = 0; i < _vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList.Length; i++)
            {
                SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(_vegetationStudioCamera.preloadVegetationCellList[_vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList[i]]);

                if (selectedVegetationCell != null)
                    selectedVegetationCell.AddCameraReference(_vegetationStudioCamera);
                else
                {
                    selectedVegetationCell = new SelectedVegetationCell(_vegetationStudioCamera.preloadVegetationCellList[_vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList[i]], _vegetationStudioCamera);

                    visibleSelectorVegetationCellList.Add(selectedVegetationCell);
                    OnVegetationCellVisibleDelegate?.Invoke(selectedVegetationCell.vegetationCell);
                }
            }
        }

        private void RemoveVisibleCellsFromCamera(VegetationStudioCamera _vegetationStudioCamera)
        {
            if (_vegetationStudioCamera.vegetationCullingGroup == null)
                return;

            for (int j = 0; j < _vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList.Length; j++)
            {
                SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(_vegetationStudioCamera.preloadVegetationCellList[_vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList[j]]);

                if (selectedVegetationCell == null)
                    continue;

                selectedVegetationCell.RemoveCameraReference(_vegetationStudioCamera);
                if (selectedVegetationCell.cameraCount == 0)
                {
                    visibleSelectorVegetationCellList.Remove(selectedVegetationCell);
                    OnVegetationCellInvisibleDelegate?.Invoke(selectedVegetationCell.vegetationCell);
                }
            }
        }

        private void OnAddCamera(VegetationStudioCamera _vegetationStudioCamera)
        {
            _vegetationStudioCamera.onPreloadCellInvisibleDelegate += OnVegetationCellInvisible;
            _vegetationStudioCamera.onVegetationCellDistanceBandChangedDelegate += OnVegetationCellDistanceBandChanged;
            AddVisibleCellsFromCamera(_vegetationStudioCamera);
        }

        private void OnRemoveCamera(VegetationStudioCamera _vegetationStudioCamera)
        {
            _vegetationStudioCamera.onPreloadCellInvisibleDelegate -= OnVegetationCellInvisible;
            _vegetationStudioCamera.onVegetationCellDistanceBandChangedDelegate -= OnVegetationCellDistanceBandChanged;
            RemoveVisibleCellsFromCamera(_vegetationStudioCamera);
        }

        public void DrawDebugGizmos()
        {
            for (int i = 0; i < visibleSelectorVegetationCellList.Count; i++)
            {
                Gizmos.color = SelectVegetationCellGizmoColor(visibleSelectorVegetationCellList[i].cameraCount);
                Gizmos.DrawWireCube(visibleSelectorVegetationCellList[i].vegetationCell.cellBounds.center, visibleSelectorVegetationCellList[i].vegetationCell.cellBounds.size);
            }
        }

        private static Color SelectVegetationCellGizmoColor(int _count)
        {
            return _count switch
            {
                0 => Color.black,
                1 => Color.white,
                2 => Color.yellow,
                3 => Color.red,
                _ => Color.green,
            };
        }

        private void OnVegetationCellDistanceBandChanged(VegetationStudioCamera _vegetationStudioCamera, VegetationCell _vegetationCell, int _currentDistanceBand, int _previousDistanceBand)
        {
            if (_currentDistanceBand == 0)
                OnVegetationCellVisible(_vegetationStudioCamera, _vegetationCell);
            else if (_previousDistanceBand == 0)
                OnVegetationCellInvisible(_vegetationStudioCamera, _vegetationCell);
        }

        private void OnVegetationCellVisible(VegetationStudioCamera _vegetationStudioCamera, VegetationCell _vegetationCell)
        {
            SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(_vegetationCell);
            if (selectedVegetationCell != null)
                selectedVegetationCell.AddCameraReference(_vegetationStudioCamera);
            else
            {
                selectedVegetationCell = new SelectedVegetationCell(_vegetationCell, _vegetationStudioCamera);
                visibleSelectorVegetationCellList.Add(selectedVegetationCell);
                OnVegetationCellVisibleDelegate?.Invoke(selectedVegetationCell.vegetationCell);
            }
        }

        private void OnVegetationCellInvisible(VegetationStudioCamera _vegetationStudioCamera, VegetationCell _vegetationCell)
        {
            SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(_vegetationCell);
            if (selectedVegetationCell == null)
                return;

            selectedVegetationCell.RemoveCameraReference(_vegetationStudioCamera);
            if (selectedVegetationCell.cameraCount == 0)
            {
                visibleSelectorVegetationCellList.Remove(selectedVegetationCell);
                OnVegetationCellInvisibleDelegate?.Invoke(selectedVegetationCell.vegetationCell);
            }
        }

        public void Dispose()
        {
            vegetationSystemPro.OnAddCameraDelegate -= OnAddCamera;
            vegetationSystemPro.OnRemoveCameraDelegate -= OnRemoveCamera;

            for (int i = 0; i < vegetationSystemPro.vegetationStudioCameraList.Count; i++)
                OnRemoveCamera(vegetationSystemPro.vegetationStudioCameraList[i]);
        }
    }
}