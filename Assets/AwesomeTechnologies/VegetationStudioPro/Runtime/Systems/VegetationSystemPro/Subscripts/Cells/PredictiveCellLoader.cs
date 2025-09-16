using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public class PredictiveCellLoader
    {
        private readonly VegetationSystemPro vegetationSystemPro;
        public readonly List<VegetationCell> preloadVegetationCellList = new();
        public readonly List<BillboardCell> preloadBillboardCellList = new();

        public PredictiveCellLoader(VegetationSystemPro _vegetationSystemPro)
        {
            vegetationSystemPro = _vegetationSystemPro;
        }

        public void Clear()
        {
            preloadVegetationCellList?.Clear();
            preloadBillboardCellList?.Clear();
        }

        //public void ClearNonImportant() // used in each camera when gathering vegetation cells
        //{
        //    for (int i = preloadVegetationCellList.Count - 1; i >= 0; i--)  // remove all non important vegetation cells safely
        //        if (preloadVegetationCellList[i].important == false)
        //            preloadVegetationCellList.RemoveAtSwapBack(i);
        //}

        public void RemoveVegetationCellsFlaggedForRemoval()  // used in "CompactCache"
        {
            for (int i = preloadVegetationCellList.Count - 1; i > -1; i--)  // remove all flagged vegetation cells safely
                if (preloadVegetationCellList[i].flagForRemoval)
                    preloadVegetationCellList.RemoveAtSwapBack(i);
        }

        public void RemoveBillboardCellsFlaggedForRemoval() // used in "CompactCache"
        {
            for (int i = preloadBillboardCellList.Count - 1; i > -1; i--)   // remove all flagged billboard cells safely
                if (preloadBillboardCellList[i].flagForRemoval)
                    preloadBillboardCellList.RemoveAtSwapBack(i);
        }

        public bool ValidatePredictiveVegetationType(VegetationType _vegetationType)
        {
            if (vegetationSystemPro.loadPredictiveCells == false)
                return false;

            return _vegetationType switch
            {   // return whether the vegetation type should be "predictive preloaded"
                VegetationType.Grass => vegetationSystemPro.togglePredictiveGrass,
                VegetationType.Plant => vegetationSystemPro.togglePredictivePlants,
                VegetationType.Objects => vegetationSystemPro.togglePredictiveObjects,
                VegetationType.LargeObjects => vegetationSystemPro.togglePredictiveLargeObjects,
                VegetationType.Tree => vegetationSystemPro.togglePredictiveTrees,
                _ => false
            };
        }

        public bool GetPredictiveDistanceBandType(out int _type)
        {
            _type = 0;
            if (vegetationSystemPro.togglePredictiveGrass || vegetationSystemPro.togglePredictivePlants || vegetationSystemPro.togglePredictiveObjects)
                _type = 0;  // gather "zero type" vegetation cells which can include all types of vegetation types
            else if (vegetationSystemPro.togglePredictiveLargeObjects || vegetationSystemPro.togglePredictiveTrees)
                _type = 1;  // gather "one type" vegetation cells which only include largeObjects and/or trees
            else
                return false;   // else don't "predictive pre-load" at all

            return true;
        }

        public void GetAllVegetationCellsForThisFrame(List<VegetationCell> _preloadList)
        {
            if (preloadVegetationCellList.Count == 0)   // empty when "PredictivePreloading" is disabled
                return;

            // else fill the predictiveCellLoaderList of the vegetation system to use them to "predictive pre-load" the vegetation cells
            for (int i = 0; i < vegetationSystemPro.predictiveCellsPerFrame; i++)   // gather for set amount per frame
            {
                VegetationCell vegetationCell = GetLastPreloadableVegetationCell();
                if (vegetationCell != null)
                    _preloadList.Add(vegetationCell);
            }
        }

        public void GetAllBillboardCellsForThisFrame(List<BillboardCell> _preloadList)
        {
            if (preloadBillboardCellList.Count == 0)    // empty when "PredictivePreloading" is disabled
                return;

            // else fill the predictiveCellLoaderList of the vegetation system to use them to "predictive pre-load" the billboard cells
            for (int i = 0; i < vegetationSystemPro.predictiveCellsPerFrame; i++)   // gather for set amount per frame
            {
                BillboardCell billboardCell = GetLastPreloadableBillboardCell();
                if (billboardCell != null)
                    _preloadList.Add(billboardCell);
            }
        }

        private VegetationCell GetLastPreloadableVegetationCell()
        {
            while (preloadVegetationCellList.Count > 0) // empty when "PredictivePreloading" is disabled
            {
                VegetationCell vegetationCell = preloadVegetationCellList[preloadVegetationCellList.Count - 1]; // get last vegetation cell in the list
                preloadVegetationCellList.RemoveAt(preloadVegetationCellList.Count - 1);    // remove safely
                return vegetationCell;
            }

            return null;
        }

        private BillboardCell GetLastPreloadableBillboardCell()
        {
            while (preloadBillboardCellList.Count > 0)  // empty when "PredictivePreloading" is disabled
            {
                BillboardCell billboardCell = preloadBillboardCellList[preloadBillboardCellList.Count - 1]; // get last billboard cell in the list
                preloadBillboardCellList.RemoveAt(preloadBillboardCellList.Count - 1);  // remove safely
                return billboardCell;
            }

            return null;
        }

        public void AddPreloadAreaVegetation(List<VegetationCell> _overlapVegetationCellList, bool _isImportant)
        {   // actually used
            preloadVegetationCellList.Clear();

            if (GetPredictiveDistanceBandType(out int distanceBand) == false)   // get distanceBand type that should be made available to be "predictive preloaded"
                return; // return if no vegetation/distanceBand type should be "predictive preloaded"

            for (int i = 0; i < _overlapVegetationCellList.Count; i++)  // for all vegetation cells within max vegetation distance + pre-load offset
                if (_overlapVegetationCellList[i].Enabled && _overlapVegetationCellList[i].loadedDistanceBand > distanceBand)   // skip vegetation cells that aren't (enabled) of the desired distanceBand -- ">" else list overload thus cell skipping
                {
                    //if (_isImportant)
                    //    _overlapVegetationCellList[i].important = true; // important vegetation cells don't get compacted

                    preloadVegetationCellList.Add(_overlapVegetationCellList[i]);   // add to internal pre-load list > passed into the system's pre-load list later
                }
        }

        public void AddPreloadAreaBillboard(List<BillboardCell> _overlapBillboardCellList, bool _isImportant)
        {   // actually used
            preloadBillboardCellList.Clear();

            if (vegetationSystemPro.togglePredictiveBillboards == false)
                return;

            for (int i = 0; i < _overlapBillboardCellList.Count; i++)   // for all vegetation cells within max vegetation distance + pre-load offset
            {
                //if (_isImportant)
                //    _overlapBillboardCellList[i].important = true;  // important billboard cells don't get compacted

                preloadBillboardCellList.AddRange(_overlapBillboardCellList);   // addRange for index-synched loop access -- to internal pre-load list > passed into the system's pre-load list later
            }
        }

        public void AddPreloadAreaVegetation(Rect _rect, int _distanceBand, bool _isImportant)
        {
            List<VegetationCell> temp = new();
            vegetationSystemPro.vegetationCellQuadTree.Query(_rect, temp);
            for (int i = 0; i < temp.Count; i++)
                if (temp[i].loadedDistanceBand >= _distanceBand)
                {
                    //if (_isImportant)
                    //    tempPreloadList[i].important = true;

                    if (preloadVegetationCellList.Contains(temp[i]) == false)
                        preloadVegetationCellList.Add(temp[i]);
                }
        }

        public void AddPreloadAreaVegetation(Rect _rect, List<VegetationCell> _overlapVegetationCellList, int _distanceBand, bool _isImportant)
        {
            vegetationSystemPro.vegetationCellQuadTree.Query(_rect, _overlapVegetationCellList);
            for (int i = 0; i < _overlapVegetationCellList.Count; i++)
                if (_overlapVegetationCellList[i].loadedDistanceBand >= _distanceBand)
                {
                    //if (_isImportant)
                    //    _overlapVegetationCellList[i].important = true;

                    if (preloadVegetationCellList.Contains(_overlapVegetationCellList[i]) == false)
                        preloadVegetationCellList.Add(_overlapVegetationCellList[i]);
                }
        }

        public void AddPreloadAreaVegetation(float3 _position, float _radius, int _distanceBand, bool _isImportant)
        {
            Rect rect = new(new float2(_position.xz - _radius), new float2(_radius * 2));
            AddPreloadAreaVegetation(rect, _distanceBand, _isImportant);
        }

        public void PreloadAllVegetationCells(int _distanceBand)
        {
            for (int i = 0; i < vegetationSystemPro.vegetationCellList.Count; i++)
                if (vegetationSystemPro.vegetationCellList[i].loadedDistanceBand >= _distanceBand)
                    preloadVegetationCellList.Add(vegetationSystemPro.vegetationCellList[i]);
        }

        public void PreloadAllBillboardCells()
        {
            for (int i = 0; i < vegetationSystemPro.billboardCellList.Count; i++)
                preloadBillboardCellList.Add(vegetationSystemPro.billboardCellList[i]);
        }
    }
}