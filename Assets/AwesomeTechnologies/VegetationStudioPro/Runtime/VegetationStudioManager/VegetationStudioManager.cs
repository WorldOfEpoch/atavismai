using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/VegetationStudioManager", 0)]
    [ExecuteInEditMode]
    public partial class VegetationStudioManager : MonoBehaviour
    {
        public static VegetationStudioManager Instance;

        public int currentTabIndex = 0;
        private static bool showBiomes; // used w/ property
        [NonSerialized] private VegetationItemInfoPro clipboardVegetationItemInfo;
        [NonSerialized] private AnimationCurve clipboardAnimationCurve;

        public List<VegetationSystemPro> VegetationSystemList = new();
        private readonly List<PolygonMaskBiome> biomeMaskList = new();
        private readonly List<BaseMaskArea> vegetationMaskList = new();

        public delegate void MultiAddVegetationSystemDelegate(VegetationSystemPro _vegetationSystem);
        public MultiAddVegetationSystemDelegate OnAddVegetationSystemDelegate;

        public delegate void MultiRemoveVegetationSystemDelegate(VegetationSystemPro _vegetationSystem);
        public MultiRemoveVegetationSystemDelegate OnRemoveVegetationSystemDelegate;

#if UNITY_POST_PROCESSING_STACK_V2
        public List<PostProcessProfileInfo> PostProcessProfileInfoList = new();
        public LayerMask PostProcessingLayer = 0;
#endif

        private void OnEnable()
        {
            VegetationSystemListFilter();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (BurstCompiler.Options.IsEnabled == false)
                Debug.LogWarning("VSP internal error log: The burst compiler is disabled!! Performance will be decreased!");
#endif
        }

        void OnDisable()
        {
            DisposeBiomeMasks();
            DisposeVegetationMasks();
        }

        void VegetationSystemListFilter()
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] == null)
                    VegetationSystemList.RemoveAt(i);
        }

        protected static void FindInstance() // static function to find the singleton instance
        {
            Instance = (VegetationStudioManager)FindAnyObjectByType(typeof(VegetationStudioManager));
        }

        #region vegetation system
        public static void RegisterVegetationSystem(VegetationSystemPro _vegetationSystem)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RegisterVegetationSystem(_vegetationSystem);
        }

        private void Instance_RegisterVegetationSystem(VegetationSystemPro _vegetationSystem)
        {
            if (_vegetationSystem == null)
                return;

            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] == null)
                    VegetationSystemList.RemoveAt(i);

            if (VegetationSystemList.Contains(_vegetationSystem) == false)
            {
                VegetationSystemList.Add(_vegetationSystem);
                OnAddVegetationSystem(_vegetationSystem);
                OnAddVegetationSystemDelegate?.Invoke(_vegetationSystem);
            }
        }

        public void OnAddVegetationSystem(VegetationSystemPro _vegetationSystem)
        {

        }

        public static void UnregisterVegetationSystem(VegetationSystemPro _vegetationSystem)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_UnregisterVegetationSystem(_vegetationSystem);
        }

        private void Instance_UnregisterVegetationSystem(VegetationSystemPro _vegetationSystem)
        {
            VegetationSystemList.Remove(_vegetationSystem);
            OnRemoveVegetationSystem(_vegetationSystem);
            OnRemoveVegetationSystemDelegate?.Invoke(_vegetationSystem);
        }

        public void OnRemoveVegetationSystem(VegetationSystemPro _vegetationSystem)
        {

        }
        #endregion

        #region vegetation cells
        public static void OnVegetationCellRefresh(VegetationSystemPro _vegetationSystem)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Internal_OnVegetationCellRefresh(_vegetationSystem);
        }

        private void Internal_OnVegetationCellRefresh(VegetationSystemPro _vegetationSystem)
        {
            for (int i = 0; i < biomeMaskList.Count; i++)
                AddBiomeMaskToVegetationSystem(_vegetationSystem, biomeMaskList[i]);

            for (int i = 0; i < vegetationMaskList.Count; i++)
                AddVegetationMaskToVegetationSystem(_vegetationSystem, vegetationMaskList[i]);
        }

        public static void ClearCache()
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Internal_ClearCache();
        }

        private void Internal_ClearCache()
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                    VegetationSystemList[i].ClearCache();
        }

        public static void ClearCache(Bounds _bounds)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Internal_ClearCache(_bounds);
        }

        private void Internal_ClearCache(Bounds _bounds)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                    VegetationSystemList[i].ClearCache(_bounds);
        }
        #endregion

        #region vegetation item clipboard
        /// <summary>
        /// Adds a new VegetationItemInfo to the clippboard -- Used for copy paste in the VegetationSystem inspector
        /// </summary>
        /// <param name="vegetationItemInfo"></param>
        public static void AddVegetationItemToClipboard(VegetationItemInfoPro vegetationItemInfo)   // vegetation item
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Internal_AddVegetationItemToClipboard(vegetationItemInfo);
        }

        private void Internal_AddVegetationItemToClipboard(VegetationItemInfoPro _vegetationItemInfo) // vegetation item
        {
            clipboardVegetationItemInfo = new VegetationItemInfoPro(_vegetationItemInfo);
        }

        public static VegetationItemInfoPro GetVegetationItemFromClipboard()    // vegetation item
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                return Instance.Internal_GetVegetationItemFromClipboard();

            return null;
        }

        private VegetationItemInfoPro Internal_GetVegetationItemFromClipboard() // vegetation item
        {
            return clipboardVegetationItemInfo;
        }
        #endregion

        #region animation curve clipboard
        public static void AddAnimationCurveToClipboard(AnimationCurve _animationCurve) // animation curve
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Internal_AddAnimationCurveToClipboard(_animationCurve);
        }

        private void Internal_AddAnimationCurveToClipboard(AnimationCurve _animationCurve)  // animation curve
        {
            clipboardAnimationCurve = _animationCurve;
        }

        public static AnimationCurve GetAnimationCurveFromClippboard()  // animation curve
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                return Instance.Internal_GetAnimationCurveFromClippboard();

            return null;
        }

        private AnimationCurve Internal_GetAnimationCurveFromClippboard()    // animation curve
        {
            return clipboardAnimationCurve;
        }
        #endregion

        #region terrains
        public static void AddTerrain(GameObject _go, bool _forceAdd, VegetationSystemPro _vspSys)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_AddTerrain(_go, _forceAdd, _vspSys);
        }

        private void Instance_AddTerrain(GameObject _go, bool _forceAdd, VegetationSystemPro _vspSys)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                {
                    if (VegetationSystemList[i].automaticBoundsCalculation && _forceAdd == false)
                    {
                        RefreshTerrainArea(_go.GetComponent<IVegetationStudioTerrain>().TerrainBounds);
                        continue;
                    }

                    if (_vspSys != null)
                    {
                        if (VegetationSystemList[i] == _vspSys)
                            VegetationSystemList[i].AddTerrain(_go);
                    }
                    else
                        VegetationSystemList[i].AddTerrain(_go);
                }
        }

        public static void RemoveTerrain(GameObject _go)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RemoveTerrain(_go);
        }

        private void Instance_RemoveTerrain(GameObject _go)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                {
                    if (VegetationSystemList[i].automaticBoundsCalculation)
                    {
                        RefreshTerrainArea(_go.GetComponent<IVegetationStudioTerrain>().TerrainBounds);
                        continue;
                    }

                    VegetationSystemList[i].RemoveTerrain(_go);
                }
        }

        public static void RefreshTerrainArea(Bounds _bounds)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RefreshTerrainArea(_bounds);
        }

        private void Instance_RefreshTerrainArea(Bounds _bounds)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                {
                    if (VegetationSystemList[i].isSetupDone == false || RectExtension.CreateRectFromBounds(VegetationSystemList[i].vegetationSystemBounds).Overlaps(RectExtension.CreateRectFromBounds(_bounds)) == false)
                        continue;

                    VegetationSystemList[i].RefreshTerrainArea(_bounds);
                }
        }

        public static void RefreshTerrainArea()
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RefreshTerrainArea();
        }

        private void Instance_RefreshTerrainArea()
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                {
                    if (VegetationSystemList[i].isSetupDone == false)
                        continue;

                    VegetationSystemList[i].RefreshTerrainArea();
                }
        }

        public static void RefreshTerrainHeightMap(bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RefreshTerrainHeightmap(_splatData, _holesData, _heightsData);

            RefreshTerrainArea();
        }

        private void Instance_RefreshTerrainHeightmap(bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                    VegetationSystemList[i].RefreshTerrainHeightmap(_splatData, _holesData, _heightsData);
        }

        public static void RefreshTerrainHeightMap(Bounds _bounds, bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RefreshTerrainHeightmap(_bounds, _splatData, _holesData, _heightsData);

            RefreshTerrainArea(_bounds);
        }

        private void Instance_RefreshTerrainHeightmap(Bounds _bounds, bool _splatData = true, bool _holesData = true, bool _heightsData = true)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                    VegetationSystemList[i].RefreshTerrainHeightmap(_bounds, _splatData, _holesData, _heightsData);
        }

        public static void PrepareTerrainStreaming(bool _removeTerrains, Transform _floatingOriginAnchor)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_PrepareTerrainStreaming(_removeTerrains, _floatingOriginAnchor);
        }

        private void Instance_PrepareTerrainStreaming(bool _removeTerrains, Transform _floatingOriginAnchor)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                {
                    VegetationSystemList[i].automaticBoundsCalculation = false;
                    if (_removeTerrains) VegetationSystemList[i].RemoveAllTerrains();
                    VegetationSystemList[i].floatingOriginAnchor = _floatingOriginAnchor == null ? VegetationSystemList[i].floatingOriginAnchor : _floatingOriginAnchor;
                }
        }
        #endregion

        #region cameras
        public static void AddCamera(Camera _camera, bool _noFrustumCulling = false, bool _renderDirectToCamera = false, bool _renderBillboardsOnly = false)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_AddCamera(_camera, _noFrustumCulling, _renderDirectToCamera, _renderBillboardsOnly);
        }

        private void Instance_AddCamera(Camera _camera, bool _noFrustumCulling = false, bool _renderDirectToCamera = false, bool _renderBillboardsOnly = false)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                    VegetationSystemList[i].AddCamera(_camera, _noFrustumCulling, _renderDirectToCamera, _renderBillboardsOnly);
        }

        public static void RemoveCamera(Camera _camera)
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                Instance.Instance_RemoveCamera(_camera);
        }

        private void Instance_RemoveCamera(Camera _camera)
        {
            for (int i = 0; i < VegetationSystemList.Count; i++)
                if (VegetationSystemList[i] != null)
                    VegetationSystemList[i].RemoveCamera(_camera);
        }
        #endregion

        public static Vector3 GetFloatingOriginOffset()
        {
            if (!Instance)
                FindInstance();

            if (Instance)
                return Instance.Instance_GetFloatingOriginOffset();

            return float3.zero;
        }

        private float3 Instance_GetFloatingOriginOffset()
        {
            if (VegetationSystemList.Count > 0)
                return VegetationSystemList[0].floatingOriginOffset;
            return float3.zero;
        }
    }
}