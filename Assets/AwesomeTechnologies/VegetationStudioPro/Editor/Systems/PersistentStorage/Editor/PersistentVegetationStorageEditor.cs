using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [CustomEditor(typeof(PersistentVegetationStorage))]
    public class PersistentVegetationStorageEditor : VegetationStudioProBaseEditor
    {
        private PersistentVegetationStorage persistentVegetationStorage;

        private static Texture[] brushTextures;
        private VegetationBrush vegetationBrush;
        private SceneMeshRaycaster sceneMeshRaycaster;
        private int changedCellIndex = -1;  // edit vegetation
        private readonly string[] TabNames = { "Settings", "Stored Vegetation", "Bake Vegetation", "Edit Vegetation", "Paint Vegetation", "Precision Painting" /*, "Import"*/ };
        SerializedProperty layerMask;

        void OnEnable()
        {
            persistentVegetationStorage = (PersistentVegetationStorage)target;
            layerMask = serializedObject.FindProperty("groundLayerMask");
            LoadBrushIcons();
            //LoadImporters();
        }

        void OnSceneGUI()
        {
            if (persistentVegetationStorage == null) return;
            if (persistentVegetationStorage.vegetationSystemPro == null) return;
            if (persistentVegetationStorage.persistentVegetationStoragePackage == null) return;

            if (persistentVegetationStorage.currentTabIndex == 3)
                OnSceneGuiEditVegetation();

            if (persistentVegetationStorage.currentTabIndex == 4)
                OnSceneGUIPaintVegetation();

            if (persistentVegetationStorage.currentTabIndex == 5)
                OnSceneGUIPrecisionPainting();
        }

        public override void OnInspectorGUI()
        {
            persistentVegetationStorage = (PersistentVegetationStorage)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            if (persistentVegetationStorage.vegetationSystemPro == null)
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                return;
            }

            if (persistentVegetationStorage.vegetationSystemPro.isSetupDone == false)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("The VegetationSystemPro component is disabled/paused or has configuration errors\nFix them to re-enable this component\nReset or re-add this component in case of reference losses", MessageType.Error);
                GUILayout.EndVertical();
                return;
            }

            if (persistentVegetationStorage.selectedVegetationPackageIndex >= persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count)
                persistentVegetationStorage.selectedVegetationPackageIndex = 0;

            GUILayout.BeginVertical("box");
            persistentVegetationStorage.currentTabIndex = GUILayout.SelectionGrid(persistentVegetationStorage.currentTabIndex, TabNames, 3, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            switch (persistentVegetationStorage.currentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawStoredVegetationInspector();
                    break;
                case 2:
                    DrawBakeVegetationInspector();
                    break;
                case 3:
                    DrawEditVegetationInspector();
                    break;
                case 4:
                    DrawPaintVegetationInspector();
                    break;
                case 5:
                    DrawPrecisionPaintingInspector();
                    break;
                case 6:
                    DrawImportInspector();
                    break;
            }
        }

        #region utility
        private static void LoadBrushIcons()
        {
            brushTextures = new Texture[20];
            for (int i = 0; i < brushTextures.Length; i++)
                brushTextures[i] = Resources.Load<Texture2D>("Brushes/Brush_" + i);
        }

        private static int AspectSelectionGrid(int _selected, Texture[] _textures, int _approxSize, string _emptyString, out bool _doubleClick)
        {
            GUILayout.BeginVertical("box", GUILayout.MinHeight(10f));
            int result = 0;
            _doubleClick = false;
            if (_textures.Length != 0)
            {
                float num = (EditorGUIUtility.currentViewWidth - 20f) / _approxSize;
                int num2 = Mathf.CeilToInt(_textures.Length / num);
                Rect aspectRect = GUILayoutUtility.GetAspectRect(num / num2);
                Event current = Event.current;
                if (current.type == EventType.MouseDown && current.clickCount == 2 && aspectRect.Contains(current.mousePosition))
                {
                    _doubleClick = true;
                    current.Use();
                }
                int xCount = (int)math.round(EditorGUIUtility.currentViewWidth - 20f) / _approxSize;
                if (xCount > 0)
                    result = GUI.SelectionGrid(aspectRect, Math.Min(_selected, _textures.Length - 1), _textures, xCount, "GridList");
            }
            else
                GUILayout.Label(_emptyString);

            GUILayout.EndVertical();
            return result;
        }

        //private void LoadImporters()
        //{
        //    if (persistentVegetationStorage.vegetationImporterList.Count != 0)
        //        return;

        //    var interfaceType = typeof(IVegetationImporter);
        //    var importerTypes = AppDomain.CurrentDomain.GetAssemblies()
        //        .SelectMany(x => x.GetLoadableTypes())
        //        .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
        //        .Select(Activator.CreateInstance);

        //    foreach (var importer in importerTypes)
        //        if (importer is IVegetationImporter importerInterface)
        //            persistentVegetationStorage.vegetationImporterList.Add(importerInterface);
        //}

        private string[] GetImporterNameArray()
        {
            string[] resultArray = new string[persistentVegetationStorage.vegetationImporterList.Count];
            for (int i = 0; i < persistentVegetationStorage.vegetationImporterList.Count; i++)
                resultArray[i] = persistentVegetationStorage.vegetationImporterList[i].ImporterName;
            return resultArray;
        }

        private bool IsPersistentStoragePackagePresent()
        {
            if (persistentVegetationStorage.persistentVegetationStoragePackage == null)
            {
                EditorGUILayout.HelpBox("A persistent vegetation package needs to be created and assigned", MessageType.Error);
                return false;
            }

            if (persistentVegetationStorage.persistentVegetationStoragePackage.PersistentVegetationCellList.Count != persistentVegetationStorage.vegetationSystemPro.vegetationCellList.Count)
            {
                EditorGUILayout.HelpBox("The persistent storage is not initialized or has been initialized for another world and/or cell size", MessageType.Error);
                return false;
            }

            return true;
        }

        public void CreatePersistentVegetationStorage()
        {
            PersistentVegetationStoragePackage newPackage = CreateInstance<PersistentVegetationStoragePackage>();

            if (AssetDatabase.IsValidFolder("Assets/PersistentVegetationStorageData") == false)
                AssetDatabase.CreateFolder("Assets", "PersistentVegetationStorageData");

            string filename = "PersistentVegetationStorage_" + Guid.NewGuid() + ".asset";
            AssetDatabase.CreateAsset(newPackage, "Assets/PersistentVegetationStorageData/" + filename);

            PersistentVegetationStoragePackage loadedPackage = AssetDatabase.LoadAssetAtPath<PersistentVegetationStoragePackage>("Assets/PersistentVegetationStorageData/" + filename);
            persistentVegetationStorage.persistentVegetationStoragePackage = loadedPackage;
            persistentVegetationStorage.InitializePersistentStorage();
        }

        private void SelectVegetationPackage()
        {
            GUILayout.BeginVertical("box");

            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            string[] packageNameList = new string[persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i < persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " + persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i].PackageName
                        + " (" + persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";
            }

            persistentVegetationStorage.selectedVegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", persistentVegetationStorage.selectedVegetationPackageIndex, packageNameList);
            if (persistentVegetationStorage.selectedVegetationPackageIndex > persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count - 1)
                persistentVegetationStorage.selectedVegetationPackageIndex = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count - 1;

            GUILayout.EndVertical();
        }

        private void SelectGroundLayers()
        {
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(layerMask, new GUIContent("Node snap-layers"));
            EditorGUILayout.HelpBox("Select the layers to snap-on when working on meshes/colliders", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(persistentVegetationStorage);
            GUILayout.EndVertical();
        }
#endregion

        #region Draw inspector logic
        private void DrawSettingsInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Info", labelStyle);
            EditorGUILayout.HelpBox(
                "The PersistentVegetationStorage component is designed to store baked vegetation instances generated from the rules of the VegetationSystemPro component\nThe baked data is stored in a scriptable object" +
                "\nPainting and import/export from 3rd party systems is also possible", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Status", labelStyle);
            if (persistentVegetationStorage.persistentVegetationStoragePackage == null)
                EditorGUILayout.HelpBox("A persistent vegetation storage package has to be assigned", MessageType.Info);
            else
            {
                EditorGUILayout.LabelField("Vegetation cell count: " + persistentVegetationStorage.GetPersistentVegetationCellCount(), labelStyle);

                List<PersistentVegetationInstanceInfo> instanceList = persistentVegetationStorage.persistentVegetationStoragePackage.GetPersistentVegetationInstanceInfoList();
                int totalCount = 0;
                for (int i = 0; i < instanceList.Count; i++)
                    totalCount += instanceList[i].Count;

                long fileSize = AssetUtility.GetAssetSize(persistentVegetationStorage.persistentVegetationStoragePackage);
                float storageSize = (float)fileSize / (1024 * 1024);
                EditorGUILayout.LabelField("Storage size: " + storageSize.ToString("F2") + " mbyte", labelStyle);
                EditorGUILayout.LabelField("Total instance count: " + totalCount.ToString("N0"), labelStyle);
            }

            EditorGUI.BeginChangeCheck();
            persistentVegetationStorage.disablePersistentStorage = EditorGUILayout.Toggle("Disable persistent storage", persistentVegetationStorage.disablePersistentStorage);
            persistentVegetationStorage.useVegetationMasking = EditorGUILayout.Toggle("Enable vegetation mask exclusion", persistentVegetationStorage.useVegetationMasking);
            if (EditorGUI.EndChangeCheck())
            {
                persistentVegetationStorage.vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(persistentVegetationStorage);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Configuration", labelStyle);
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            persistentVegetationStorage.persistentVegetationStoragePackage = EditorGUILayout.ObjectField("Storage data", persistentVegetationStorage.persistentVegetationStoragePackage, typeof(PersistentVegetationStoragePackage), true) as PersistentVegetationStoragePackage;
            if (GUILayout.Button("Create"))
                CreatePersistentVegetationStorage();
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (persistentVegetationStorage.persistentVegetationStoragePackage != null && !persistentVegetationStorage.persistentVegetationStoragePackage.Initialized)
                    if (EditorUtility.DisplayDialog("Initialize persistent storage", "Initialize the persistent storage for the current vegetation system?", "OK", "Cancel"))
                        persistentVegetationStorage.InitializePersistentStorage();

                EditorUtility.SetDirty(target);
                if (persistentVegetationStorage.persistentVegetationStoragePackage)
                    EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                persistentVegetationStorage.vegetationSystemPro.ClearCache();
            }

            EditorGUILayout.HelpBox("Create a new PersistentVegetationStoragePackage object by navigating to Window/Awesome Technologies/Persistent Vegetation Storage Package", MessageType.Info);
            if (persistentVegetationStorage.persistentVegetationStoragePackage == null)
            {
                GUILayout.EndVertical();
                return;
            }

            if (GUILayout.Button("Initialize persistent storage"))
            {
                if (EditorUtility.DisplayDialog("Initialize persistent storage", "This clears existing data in the storage", "OK", "Cancel"))
                {
                    persistentVegetationStorage.InitializePersistentStorage();
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                    persistentVegetationStorage.vegetationSystemPro.ClearCache();
                }
            }

            EditorGUILayout.HelpBox("Initializing the persistent storage clears the assigned storage data and configures it to store vegetation instances for the current configuration of the \"VegetationSystemPro\" component", MessageType.Info);
            GUILayout.EndVertical();
        }

        private void DrawStoredVegetationInspector()
        {
            if (IsPersistentStoragePackagePresent() == false)
                return;

            List<PersistentVegetationInstanceInfo> instanceList = persistentVegetationStorage.persistentVegetationStoragePackage.GetPersistentVegetationInstanceInfoList();
            if (instanceList.Count == 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("There are no vegetation items stored in this storage\nTo add vegetation items from the rules either bake them or paint with the tool", MessageType.Warning);
                EditorGUILayout.HelpBox("Imports/Exports from 3rd party systems are possible using the storage API through custom code", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            SelectVegetationPackage();
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
                return;

            GUILayout.BeginVertical("box");
            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null || vegetationPackagePro.VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            List<string> vegetationItemIdList = new();
            for (int i = 0; i < instanceList.Count; i++)
            {
                VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.GetVegetationInfo(instanceList[i].VegetationItemID);
                if (vegetationItemInfoPro != null)
                    vegetationItemIdList.Add(instanceList[i].VegetationItemID);
            }

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, vegetationItemIdList, 64, ref persistentVegetationStorage.selectedStorageVegetationID);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            int instanceCount = 0;
            PersistentVegetationInstanceInfo selectedPersistentVegetationInstanceInfo = null;
            for (int i = 0; i < instanceList.Count; i++)
                if (instanceList[i].VegetationItemID == persistentVegetationStorage.selectedStorageVegetationID)
                {
                    selectedPersistentVegetationInstanceInfo = instanceList[i];
                    instanceCount = instanceList[i].Count;
                }

            EditorGUILayout.LabelField("Instance count: " + instanceCount.ToString("N0"), labelStyle);

            if (selectedPersistentVegetationInstanceInfo != null)
                for (int i = 0; i < selectedPersistentVegetationInstanceInfo.SourceCountList.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(PersistentVegetationStorageTools.GetSourceName(selectedPersistentVegetationInstanceInfo.SourceCountList[i].VegetationSourceID) + " : " + selectedPersistentVegetationInstanceInfo.SourceCountList[i].Count.ToString("N0"), labelStyle);

                    if (GUILayout.Button("Clear instances", GUILayout.Width(120)))  // instances per source
                    {
                        if (EditorUtility.DisplayDialog("Clear instances", "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" + "\nSelected action: Clear specific instances", "OK", "Cancel"))
                        {
                            persistentVegetationStorage.RemoveVegetationItemInstances(persistentVegetationStorage.selectedStorageVegetationID, selectedPersistentVegetationInstanceInfo.SourceCountList[i].VegetationSourceID);
                            persistentVegetationStorage.vegetationSystemPro.ClearCache(persistentVegetationStorage.selectedStorageVegetationID);
                            EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                        }
                    }

                    GUILayout.EndHorizontal();
                }
            GUILayout.EndVertical();

            if (GUILayout.Button("Clear instances of all sources")) // instances of all sources
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear instances", "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" +
                    "\nSelected action: Clear instances of all sources(Painted/Baked/Imported)", "Clear", "Clear/enable run-time spawn", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        persistentVegetationStorage.RemoveVegetationItemInstances(persistentVegetationStorage.selectedStorageVegetationID);
                        break;
                    case 1:
                        persistentVegetationStorage.RemoveVegetationItemInstances(persistentVegetationStorage.selectedStorageVegetationID);
                        VegetationItemInfoPro tempVegetationItemInfo = persistentVegetationStorage.vegetationSystemPro.GetVegetationItemInfo(persistentVegetationStorage.selectedStorageVegetationID);
                        if (tempVegetationItemInfo != null)
                            tempVegetationItemInfo.EnableRuntimeSpawn = true;
                        break;
                }

                persistentVegetationStorage.vegetationSystemPro.ClearCache(persistentVegetationStorage.selectedStorageVegetationID);
                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Selected vegetation package / biome", labelStyle);
            GUILayout.EndVertical();
            if (GUILayout.Button("Clear ALL vegetation items")) // all instances
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage", "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" +
                    "\nSelected action: Clear ALL", "Clear", "Clear/enable run-time spawn", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        for (int i = 0; i < vegetationItemIdList.Count; i++)
                            persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i]);
                        break;
                    case 1:
                        for (int i = 0; i < vegetationItemIdList.Count; i++)
                        {
                            persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i]);
                            VegetationItemInfoPro tempVegetationItemInfo = persistentVegetationStorage.vegetationSystemPro.GetVegetationItemInfo(vegetationItemIdList[i]);
                            if (tempVegetationItemInfo != null)
                                tempVegetationItemInfo.EnableRuntimeSpawn = true;
                        }
                        break;
                }

                persistentVegetationStorage.vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
            }

            if (GUILayout.Button("Clear all BAKED vegetation items"))   // all instances that are baked
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage", "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" +
                    "\nSelected action: Clear all BAKED", "Clear", "Clear/enable run-time spawn", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        for (int i = 0; i < vegetationItemIdList.Count; i++)
                            persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i], 0);
                        break;
                    case 1:
                        for (int i = 0; i < vegetationItemIdList.Count; i++)
                        {
                            persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i], 0);
                            VegetationItemInfoPro tempVegetationItemInfo = persistentVegetationStorage.vegetationSystemPro.GetVegetationItemInfo(vegetationItemIdList[i]);
                            if (tempVegetationItemInfo != null)
                                tempVegetationItemInfo.EnableRuntimeSpawn = true;
                        }
                        break;
                }

                persistentVegetationStorage.vegetationSystemPro.ClearCache();
                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("ALL vegetation packages / biomes", labelStyle);
            GUILayout.EndVertical();
            if (GUILayout.Button("Clear ALL vegetation items"))   // all instances of all biomes
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage", "Selected biome: All Biomes\nSelected action: Clear ALL", "Clear", "Clear/enable run-time spawn", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        ClearAllItemsFromAllVegetationPackages(false, false);
                        break;
                    case 1:
                        ClearAllItemsFromAllVegetationPackages(false, true);
                        break;
                }
            }

            if (GUILayout.Button("Clear all BAKED vegetation items")) // all instances of all biomes that are baked
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Clear storage", "Selected biome: All Biomes\nSelected action: Clear all BAKED", "Clear", "Clear/enable run-time spawn", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        ClearAllItemsFromAllVegetationPackages(true, false);
                        break;
                    case 1:
                        ClearAllItemsFromAllVegetationPackages(true, true);
                        break;
                }
            }
        }

        private void ClearAllItemsFromAllVegetationPackages(bool _bakedOnly, bool _enableRuntimeSpawn)
        {
            for (int i = 0; i < persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                for (int j = 0; j < persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                {
                    if (_bakedOnly)
                        persistentVegetationStorage.RemoveVegetationItemInstances(persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID, 0);
                    else
                        persistentVegetationStorage.RemoveVegetationItemInstances(persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].VegetationItemID);

                    if (_enableRuntimeSpawn)
                        persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j].EnableRuntimeSpawn = true;
                }
                EditorUtility.SetDirty(persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i]);
            }

            persistentVegetationStorage.vegetationSystemPro.ClearCache();
            EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
        }

        private void DrawBakeVegetationInspector()
        {
            if (IsPersistentStoragePackagePresent() == false)
                return;

            SelectVegetationPackage();
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
                return;

            GUILayout.BeginVertical("box");
            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null || vegetationPackagePro.VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            List<string> vegetationItemIdList = VegetationPackageEditorTools.CreateVegetationInfoIdList(vegetationPackagePro);
            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, vegetationItemIdList, 64, ref persistentVegetationStorage.selectedBakeVegetationID);
            GUILayout.EndVertical();
            if (vegetationItemIdList.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no vegetation items in the vegetation package", MessageType.Info);
                return;
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            EditorGUI.BeginChangeCheck();
            persistentVegetationStorage.forceBaking = EditorGUILayout.Toggle("Include disabled items", persistentVegetationStorage.forceBaking);
            persistentVegetationStorage.excludeGrass = EditorGUILayout.Toggle("Exclude grass", persistentVegetationStorage.excludeGrass);
            persistentVegetationStorage.excludePlants = EditorGUILayout.Toggle("Exclude plants", persistentVegetationStorage.excludePlants);
            persistentVegetationStorage.excludeObjects = EditorGUILayout.Toggle("Exclude objects", persistentVegetationStorage.excludeObjects);
            persistentVegetationStorage.excludeLargeObjects = EditorGUILayout.Toggle("Exclude large objects", persistentVegetationStorage.excludeLargeObjects);
            persistentVegetationStorage.excludeTrees = EditorGUILayout.Toggle("Exclude trees", persistentVegetationStorage.excludeTrees);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(persistentVegetationStorage);
            GUILayout.EndVertical();

            if (GUILayout.Button("Bake the selected vegetation item"))
            {
                bool buttonResult = EditorUtility.DisplayDialog("Bake vegetation", "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" +
                  "\nSelected action: Bake the selected vegetation item", "Bake", "Cancel");

                if (buttonResult)
                {
                    persistentVegetationStorage.BakeVegetationItem(persistentVegetationStorage.selectedBakeVegetationID);
                    persistentVegetationStorage.vegetationSystemPro.ClearCache(persistentVegetationStorage.selectedBakeVegetationID);
                    EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                }
            }

            if (GUILayout.Button("Bake the selected vegetation package / biome"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Bake vegetation", "Selected biome: " + vegetationPackagePro.PackageName + " (" + vegetationPackagePro.BiomeType + ")" +
                   "\nSelected action: Bake ALL\n\"Clear and bake\" clears previously baked data", "Bake", "Clear and bake", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        for (int i = 0; i < vegetationItemIdList.Count; i++)
                            persistentVegetationStorage.BakeVegetationItem(vegetationItemIdList[i]);
                        break;
                    case 1:
                        for (int i = 0; i < vegetationItemIdList.Count; i++)
                        {
                            persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemIdList[i], 0);
                            persistentVegetationStorage.BakeVegetationItem(vegetationItemIdList[i]);
                        }
                        break;
                }

                persistentVegetationStorage.vegetationSystemPro.ClearCache(vegetationPackagePro);
                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
            }

            if (GUILayout.Button("Bake ALL vegetation packages / biomes"))
            {
                int buttonResult = EditorUtility.DisplayDialogComplex("Bake vegetation", "Selected biome: All biomes" +
                   "\nSelected action: Bake ALL\n\"Clear and bake\" clears previously baked data", "Bake", "Clear and bake", "Cancel");

                switch (buttonResult)
                {
                    case 0:
                        BakeAllVegetationItemsOfAllBiomes(false);
                        break;
                    case 1:
                        BakeAllVegetationItemsOfAllBiomes(true);
                        break;
                }
            }

            EditorGUILayout.HelpBox("\"Enable run-time spawning\" is being disabled for each baked item", MessageType.Warning);
        }

        private void BakeAllVegetationItemsOfAllBiomes(bool _clearOld)
        {
            for (int i = 0; i < persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[i];
                for (int j = 0; j < vegetationPackagePro.VegetationInfoList.Count; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];
                    if (_clearOld)
                        persistentVegetationStorage.RemoveVegetationItemInstances(vegetationItemInfoPro.VegetationItemID, 0);
                    persistentVegetationStorage.BakeVegetationItem(vegetationItemInfoPro.VegetationItemID);
                }
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            persistentVegetationStorage.vegetationSystemPro.ClearCache();
            EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
        }

        private void DrawEditVegetationInspector()
        {
            if (IsPersistentStoragePackagePresent() == false)
                return;

            SelectVegetationPackage();
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
                return;

            GUILayout.BeginVertical("box");
            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null || vegetationPackagePro.VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, VegetationPackageEditorTools.CreateVegetationInfoIDList(vegetationPackagePro,
                new[] { VegetationType.Objects, VegetationType.Tree, VegetationType.LargeObjects }), 64, ref persistentVegetationStorage.selectedEditVegetationID);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Edit vegetation instances", labelStyle);
            EditorGUILayout.HelpBox("Select the vegetation item to edit\nMove/Rotate/Scale handles show up in the scene view when close up to related vegetation instances", MessageType.Info);
            EditorGUILayout.LabelField("Move: W / Rotate: E / Scale: R", labelStyle);

            EditorGUILayout.LabelField("Insert vegetation item: Ctrl-click", labelStyle);
            EditorGUILayout.LabelField("Delete vegetation item: Hold Ctrl-Shift", labelStyle);
            SelectGroundLayers();
            GUILayout.EndVertical();
        }

        private void DrawPaintVegetationInspector()
        {
            if (IsPersistentStoragePackagePresent() == false)
                return;

            SelectVegetationPackage();
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
                return;

            GUILayout.BeginVertical("box");
            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null || vegetationPackagePro.VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, VegetationPackageEditorTools.CreateVegetationInfoIDList(vegetationPackagePro,
                new[] { VegetationType.Grass, VegetationType.Plant, VegetationType.Tree, VegetationType.Objects, VegetationType.LargeObjects }), 64, ref persistentVegetationStorage.selectedPaintVegetationID);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings -- Vegetation item", labelStyle);
            persistentVegetationStorage.randomizePosition = EditorGUILayout.Toggle("Randomize paint position", persistentVegetationStorage.randomizePosition);
            persistentVegetationStorage.usePositionOffsetRange = EditorGUILayout.Toggle("Use position offset range", persistentVegetationStorage.usePositionOffsetRange);
            EditorGUI.BeginDisabledGroup(true);
            _ = EditorGUILayout.Toggle("Randomize rotation (inherited)", persistentVegetationStorage.randomizeRotation);
            EditorGUI.EndDisabledGroup();
            persistentVegetationStorage.useScale = EditorGUILayout.Toggle("Use scale", persistentVegetationStorage.useScale);
            EditorGUILayout.Space();
            persistentVegetationStorage.useSteepnessRule = EditorGUILayout.Toggle("Use steepness rule", persistentVegetationStorage.useSteepnessRule);
            persistentVegetationStorage.useConcaveLocationRule = EditorGUILayout.Toggle("Use concave location rule", persistentVegetationStorage.useConcaveLocationRule);
            EditorGUILayout.Space();
            persistentVegetationStorage.useTerrainTextureIncludeRule = EditorGUILayout.Toggle("Use terrain texture include rule", persistentVegetationStorage.useTerrainTextureIncludeRule);    // terr tex
            persistentVegetationStorage.useTerrainTextureExcludeRule = EditorGUILayout.Toggle("Use terrain texture exclude rule", persistentVegetationStorage.useTerrainTextureExcludeRule);
            persistentVegetationStorage.useTerrainTextureScaleRule = EditorGUILayout.Toggle("Use terrain texture scale rule", persistentVegetationStorage.useTerrainTextureScaleRule);
            EditorGUILayout.Space();
            persistentVegetationStorage.useTextureMaskIncludeRule = EditorGUILayout.Toggle("Use texture mask include rule", persistentVegetationStorage.useTextureMaskIncludeRule); // tex mask
            persistentVegetationStorage.useTextureMaskExcludeRule = EditorGUILayout.Toggle("Use texture mask exclude rule", persistentVegetationStorage.useTextureMaskExcludeRule);
            persistentVegetationStorage.useTextureMaskScaleRule = EditorGUILayout.Toggle("Use texture mask scale rule", persistentVegetationStorage.useTextureMaskScaleRule);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings -- Painting", labelStyle);
            persistentVegetationStorage.selectedBrushIndex = AspectSelectionGrid(persistentVegetationStorage.selectedBrushIndex, brushTextures, 32, "No brushes defined", out _);
            persistentVegetationStorage.brushSize = EditorGUILayout.Slider("Brush area", persistentVegetationStorage.brushSize, 0.6f, 128);
            persistentVegetationStorage.sampleDistance = EditorGUILayout.Slider("Sample distance", persistentVegetationStorage.sampleDistance, math.max(0.4f, math.lerp(0.4f, 4, persistentVegetationStorage.brushSize / 128)), persistentVegetationStorage.brushSize);
            SelectGroundLayers();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings -- Delete painting: Hold Ctrl", labelStyle);
            persistentVegetationStorage.ignoreHeight = EditorGUILayout.Toggle("Ignore height on delete", persistentVegetationStorage.ignoreHeight);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(persistentVegetationStorage);
        }

        private void DrawPrecisionPaintingInspector()
        {
            if (IsPersistentStoragePackagePresent() == false)
                return;

            SelectVegetationPackage();
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
                return;

            if (sceneMeshRaycaster == null)
                sceneMeshRaycaster = new SceneMeshRaycaster();

            GUILayout.BeginVertical("box");
            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null || vegetationPackagePro.VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, VegetationPackageEditorTools.CreateVegetationInfoIDList(vegetationPackagePro,
                new[] { VegetationType.Grass, VegetationType.Plant }), 64, ref persistentVegetationStorage.selectedPrecisionPaintingVegetationID);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings -- Vegetation item", labelStyle);
            persistentVegetationStorage.useScale = EditorGUILayout.Toggle("Use scale rules", persistentVegetationStorage.useScale);
            persistentVegetationStorage.useSteepnessRule = EditorGUILayout.Toggle("Use steepness rules", persistentVegetationStorage.useSteepnessRule);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings -- Painting", labelStyle);
            persistentVegetationStorage.precisionPaintingMode = (PrecisionPaintingMode)EditorGUILayout.EnumPopup("Painting mode", persistentVegetationStorage.precisionPaintingMode);
            persistentVegetationStorage.sampleDistance = EditorGUILayout.Slider("Sample distance", persistentVegetationStorage.sampleDistance, 0.2f, 5f);
            SelectGroundLayers();
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(persistentVegetationStorage);
        }

        private void DrawImportInspector()
        {
            if (IsPersistentStoragePackagePresent() == false)
                return;

            SelectVegetationPackage();
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0)
                return;

            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null || vegetationPackagePro.VegetationInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox("The vegetation package is missing or contains no vegetation items", MessageType.Info);
                return;
            }

            for (int i = 0; i < persistentVegetationStorage.vegetationImporterList.Count; i++)
            {
                persistentVegetationStorage.vegetationImporterList[i].PersistentVegetationStoragePackage = persistentVegetationStorage.persistentVegetationStoragePackage;
                persistentVegetationStorage.vegetationImporterList[i].VegetationPackagePro = vegetationPackagePro;
                persistentVegetationStorage.vegetationImporterList[i].PersistentVegetationStorage = persistentVegetationStorage;
            }

            string[] importerNames = GetImporterNameArray();
            GUILayout.BeginVertical("box");
            persistentVegetationStorage.selectedImporterIndex = EditorGUILayout.Popup(persistentVegetationStorage.selectedImporterIndex, importerNames);
            GUILayout.EndVertical();

            if (persistentVegetationStorage.vegetationImporterList.Count == 0)
                return;

            GUILayout.BeginVertical("box");
            IVegetationImporter importer = persistentVegetationStorage.vegetationImporterList[persistentVegetationStorage.selectedImporterIndex];
            EditorGUILayout.LabelField(importer.ImporterName, labelStyle);
            GUILayout.EndVertical();

            importer.OnGUI();
        }
        #endregion

        #region OnSceneGUI logic
        private void OnSceneGuiEditVegetation()
        {
            if (persistentVegetationStorage.selectedEditVegetationID == "") return;
            if (persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList.Count == 0) return;
            VegetationPackagePro vegetationPackagePro = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex];
            if (vegetationPackagePro == null) return;
            if (vegetationPackagePro.VegetationInfoList.Count == 0) return;
            VegetationItemInfoPro vegetationItemInfo = vegetationPackagePro.GetVegetationInfo(persistentVegetationStorage.selectedEditVegetationID);

            if (Event.current.type == EventType.MouseDown)
                changedCellIndex = -1;  // reset before moving an item

            if (Event.current.type == EventType.MouseUp)
                if (changedCellIndex != -1) // whether an item has moved cells ..on release/after moving
                {
                    persistentVegetationStorage.RepositionCellItems(changedCellIndex, persistentVegetationStorage.selectedEditVegetationID);
                    EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                }

            VegetationStudioCamera vegetationStudioCamera = persistentVegetationStorage.vegetationSystemPro.GetSceneViewVegetationStudioCamera();
            if (vegetationStudioCamera == null || vegetationStudioCamera.vegetationCullingGroup == null)
                return;

            List<VegetationCell> closeCellList = new();
            for (int i = 0; i < vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList.Length; i++)
                if (vegetationStudioCamera.vegetationCullingGroup.cellCullingInfoList[vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList[i]].CurrentDistanceBand == 0)
                    closeCellList.Add(vegetationStudioCamera.preloadVegetationCellList[vegetationStudioCamera.vegetationCullingGroup.visibleCellIndexList[i]]);

            Event currentEvent = Event.current;
            if (currentEvent.shift || currentEvent.control)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (currentEvent.shift && currentEvent.control)
            {   // remove item
                for (int i = 0; i < closeCellList.Count; i++)
                {
                    PersistentVegetationCell persistentVegetationCell = persistentVegetationStorage.persistentVegetationStoragePackage.PersistentVegetationCellList[closeCellList[i].index];
                    PersistentVegetationInfo persistentVegetationInfo = persistentVegetationCell.GetPersistentVegetationInfo(persistentVegetationStorage.selectedEditVegetationID);
                    if (persistentVegetationInfo == null)
                        continue;

                    for (int j = persistentVegetationInfo.VegetationItemList.Count - 1; j >= 0; j--)
                    {
                        PersistentVegetationItem persistentVegetationItem = persistentVegetationInfo.VegetationItemList[j];
                        float distance = math.distance(vegetationStudioCamera.selectedCamera.transform.position, persistentVegetationItem.Position + persistentVegetationStorage.vegetationSystemPro.VegetationSystemPosition);

                        Handles.color = Color.red;
                        if (Handles.Button(persistentVegetationItem.Position + persistentVegetationStorage.vegetationSystemPro.VegetationSystemPosition,
                            quaternion.LookRotation(persistentVegetationItem.Position - (float3)vegetationStudioCamera.selectedCamera.transform.position, new float3(0, 1, 0)), 0.025f * distance, 0.025f * distance, Handles.CircleHandleCap))
                        {
                            persistentVegetationInfo.RemovePersistentVegetationItemInstance(ref persistentVegetationItem);
                            persistentVegetationStorage.vegetationSystemPro.ClearCache(closeCellList[i], vegetationItemInfo.VegetationItemID);

                            persistentVegetationStorage.persistentVegetationStoragePackage.SetInstanceInfoDirty();
                            EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < closeCellList.Count; i++)
                {   // edit item
                    PersistentVegetationCell persistentVegetationCell = persistentVegetationStorage.persistentVegetationStoragePackage.PersistentVegetationCellList[closeCellList[i].index];
                    PersistentVegetationInfo persistentVegetationInfo = persistentVegetationCell.GetPersistentVegetationInfo(persistentVegetationStorage.selectedEditVegetationID);
                    if (persistentVegetationInfo == null)
                        continue;

                    for (int j = persistentVegetationInfo.VegetationItemList.Count - 1; j >= 0; j--)
                    {
                        PersistentVegetationItem persistentVegetationItem = persistentVegetationInfo.VegetationItemList[j]; // PVS item to "edit" > write back
                        float3 worldspacePosition = persistentVegetationItem.Position + persistentVegetationStorage.vegetationSystemPro.VegetationSystemPosition;
                        if (math.distance(vegetationStudioCamera.selectedCamera.transform.position, worldspacePosition) > 50)
                            continue;

                        EditorGUI.BeginChangeCheck();
                        if (Tools.current == Tool.Move)
                        {
                            float3 newPosition = Handles.PositionHandle(worldspacePosition, quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {   // change position
                                if (math.abs(worldspacePosition.x - newPosition.x) < 0.01f && math.abs(worldspacePosition.z - newPosition.z) < 0.01f)
                                    persistentVegetationItem.Position = newPosition - persistentVegetationStorage.vegetationSystemPro.VegetationSystemPosition;
                                else
                                    persistentVegetationItem.Position = PositionVegetationItem(newPosition) - persistentVegetationStorage.vegetationSystemPro.VegetationSystemPosition;

                                changedCellIndex = closeCellList[i].index;  // change the cell index in case the item moved cells
                                persistentVegetationInfo.VegetationItemList[j] = persistentVegetationItem;  // write back
                                persistentVegetationInfo.UpdatePersistentVegetationItemInstanceSourceId(ref persistentVegetationItem, 1);

                                persistentVegetationStorage.vegetationSystemPro.ClearCache(closeCellList[i], vegetationItemInfo.VegetationItemID);
                                persistentVegetationStorage.persistentVegetationStoragePackage.SetInstanceInfoDirty();
                                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                            }
                        }

                        if (Tools.current == Tool.Rotate)
                        {
                            Handles.color = Color.red;
                            quaternion newRotation;
                            if (vegetationItemInfo.VegetationType == VegetationType.Tree)
                                newRotation = Handles.Disc(persistentVegetationItem.Rotation, worldspacePosition, new float3(0, 1, 0), HandleUtility.GetHandleSize(worldspacePosition) * 1, true, 0.1f);
                            else
                                newRotation = Handles.RotationHandle(persistentVegetationItem.Rotation, worldspacePosition);

                            if (EditorGUI.EndChangeCheck())
                            {
                                persistentVegetationItem.Rotation = newRotation;    // change rotation
                                persistentVegetationInfo.VegetationItemList[j] = persistentVegetationItem;  // write back
                                persistentVegetationInfo.UpdatePersistentVegetationItemInstanceSourceId(ref persistentVegetationItem, 1);

                                persistentVegetationStorage.vegetationSystemPro.ClearCache(closeCellList[i], vegetationItemInfo.VegetationItemID);
                                persistentVegetationStorage.persistentVegetationStoragePackage.SetInstanceInfoDirty();
                                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                            }
                        }

                        if (Tools.current == Tool.Scale)
                        {
                            Handles.color = Color.red;
                            float scale = Handles.ScaleSlider(persistentVegetationItem.Scale.x, worldspacePosition, new float3(1, 0, 0), persistentVegetationItem.Rotation, HandleUtility.GetHandleSize(worldspacePosition) * 1, 0.1f);

                            if (EditorGUI.EndChangeCheck())
                            {
                                persistentVegetationItem.Scale = new float3(scale); // change scale
                                persistentVegetationInfo.VegetationItemList[j] = persistentVegetationItem;  // write back
                                persistentVegetationInfo.UpdatePersistentVegetationItemInstanceSourceId(ref persistentVegetationItem, 1);
                                persistentVegetationStorage.vegetationSystemPro.ClearCache(closeCellList[i], vegetationItemInfo.VegetationItemID);

                                persistentVegetationStorage.persistentVegetationStoragePackage.SetInstanceInfoDirty();
                                EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                            }
                        }
                    }
                }
            }

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control && !currentEvent.shift && !currentEvent.alt)
            {   // add item
                Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, 10000).OrderBy(h => h.distance).ToArray();
                for (int i = 0; i < hits.Length; i++)
                    if (hits[i].collider is TerrainCollider || persistentVegetationStorage.groundLayerMask.Contains(hits[i].collider.gameObject.layer))
                    {
                        float scale = UnityEngine.Random.Range(vegetationItemInfo.MinScale, vegetationItemInfo.MaxScale);
                        persistentVegetationStorage.AddVegetationItemInstance(persistentVegetationStorage.selectedEditVegetationID, hits[i].point, new float3(scale), quaternion.Euler(0, UnityEngine.Random.Range(0, 6.28318530718f), 0), true, 1, 1, true);
                        EditorUtility.SetDirty(persistentVegetationStorage.persistentVegetationStoragePackage);
                        break;
                    }
            }
        }

        private float3 PositionVegetationItem(float3 _position)
        {
            RaycastHit[] hits = Physics.RaycastAll(new Ray(_position + new float3(0, 10000, 0), new float3(0, -1, 0))).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
                if (hits[i].collider is TerrainCollider || persistentVegetationStorage.groundLayerMask.Contains(hits[i].collider.gameObject.layer))
                    return hits[i].point;
            return _position;
        }

        private void OnSceneGUIPaintVegetation()
        {
            if (Event.current.alt) return;
            if (persistentVegetationStorage.selectedPaintVegetationID == "")
                return;

            float3 hitPosition = float3.zero;
            if (GetPreviewBrushData(ref hitPosition) == false)
                return;

            if (Event.current.type == EventType.Repaint)
                PaintVegetationItems(hitPosition, false);

            if (Event.current.type == EventType.MouseMove)
            {
                HandleUtility.Repaint();    // enforce for smooth painting preview
                PaintVegetationItems(hitPosition, false);
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                HandleUtility.Repaint();    // enforce for smooth painting preview
                PaintVegetationItems(hitPosition, true);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                HandleUtility.Repaint();    // enforce for smooth FPS painting preview
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive); // don't click on > select other objects
                Event.current.Use();    // don't click on > select other objects
                PaintVegetationItems(hitPosition, true);
            }
        }

        private void PaintVegetationItems(float3 _hitPosition, bool _addVegetationItems)
        {
            float3 corner = _hitPosition + new float3(-persistentVegetationStorage.brushSize, 0, -persistentVegetationStorage.brushSize);
            float currentSampleDistance = persistentVegetationStorage.sampleDistance;

            int xCount = (int)math.round(persistentVegetationStorage.brushSize * 2 / currentSampleDistance);
            int zCount = xCount;

            for (int x = 0; x < xCount; x++)
                for (int z = 0; z < zCount; z++)
                {
                    float3 samplePosition = corner + new float3(x * currentSampleDistance, 0, z * currentSampleDistance);
                    float3 randomizedPosition = persistentVegetationStorage.randomizePosition ? RandomizePosition(samplePosition, currentSampleDistance) : samplePosition;
                    samplePosition = AllignToTerrain(samplePosition, out float3 normal, out UnityTerrain unityTerrain);

                    if (_addVegetationItems)
                        randomizedPosition = AllignToTerrain(randomizedPosition, out normal, out unityTerrain);

                    if (SampleBrushPosition(samplePosition, corner) == false)
                        continue;

                    Handles.SphereHandleCap(0, samplePosition, quaternion.identity, HandleUtility.GetHandleSize(samplePosition) * 0.1f, EventType.Repaint);
                    Handles.DrawLine(samplePosition, samplePosition + normal);

                    if (_addVegetationItems == false)
                        continue;

                    if (Event.current.control)
                        EraseVegetationItem(randomizedPosition, persistentVegetationStorage.selectedPaintVegetationID, currentSampleDistance);
                    else
                        AddVegetationItem(unityTerrain, randomizedPosition, normal, persistentVegetationStorage.selectedPaintVegetationID, currentSampleDistance);
                }
        }

        private void OnSceneGUIPrecisionPainting()
        {
            if (persistentVegetationStorage.selectedPrecisionPaintingVegetationID == "")
                return;

            if (Event.current.type == EventType.Repaint)
                PrecisionPaintItem(false);

            if (Event.current.type == EventType.MouseMove)
            {
                HandleUtility.Repaint();    // enforce for smooth FPS painting preview
                PrecisionPaintItem(false);
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                HandleUtility.Repaint();    // enforce for smooth FPS painting preview
                PrecisionPaintItem(true);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                HandleUtility.Repaint();    // enforce for smooth FPS painting preview
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive); // don't click on > select other objects
                Event.current.Use();    // don't click on > select other objects
                PrecisionPaintItem(true);
            }
        }

        private void PrecisionPaintItem(bool _addVegetationItem)
        {
            if (sceneMeshRaycaster == null)
                return;

            bool includeMeshes = true;
            bool includeColliders = false;
            switch (persistentVegetationStorage.precisionPaintingMode)
            {
                case PrecisionPaintingMode.Terrain:
                    includeMeshes = false;
                    includeColliders = false;
                    break;
                case PrecisionPaintingMode.TerrainAndColliders:
                    includeMeshes = false;
                    includeColliders = true;
                    break;
                case PrecisionPaintingMode.TerrainAndMeshes:
                    includeMeshes = true;
                    includeColliders = false;
                    break;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (sceneMeshRaycaster.RaycastSceneMeshes(ray, out RaycastHit raycastHit, true, includeColliders, includeMeshes))
            {
                Gizmos.color = Color.white;
                Handles.SphereHandleCap(0, raycastHit.point, quaternion.identity, HandleUtility.GetHandleSize(raycastHit.point) * 0.1f, EventType.Repaint);

                Gizmos.color = Color.green;
                Vector3 normal = raycastHit.normal.normalized;
                Handles.DrawLine(raycastHit.point, raycastHit.point + normal * 2);

                if (_addVegetationItem == false)
                    return;

                if (Event.current.control)
                    EraseVegetationItem(raycastHit.point, persistentVegetationStorage.selectedPrecisionPaintingVegetationID, persistentVegetationStorage.sampleDistance);
                else
                    AddVegetationItem(null, raycastHit.point, normal, persistentVegetationStorage.selectedPrecisionPaintingVegetationID, persistentVegetationStorage.sampleDistance);
            }
        }

        private void EraseVegetationItem(float3 _worldPosition, string _vegetationItemID, float _sampleDistance)
        {
            if (persistentVegetationStorage.ignoreHeight)
                VegetationStudioManager.RemoveVegetationItemInstance2D(_vegetationItemID, _worldPosition, _sampleDistance);
            else
                VegetationStudioManager.RemoveVegetationItemInstance(_vegetationItemID, _worldPosition, _sampleDistance);
        }

        private void AddVegetationItem(UnityTerrain _unityTerrain, float3 _worldPosition, float3 _terrainNormal, string _vegetationItemID, float _sampleDistance)
        {
            VegetationItemInfoPro vegetationItemInfo = persistentVegetationStorage.vegetationSystemPro.GetVegetationItemInfo(_vegetationItemID);

            // steepness exclude
            float slopeAngle = math.degrees(math.acos(math.dot(_terrainNormal, new float3(0, 1, 0))));
            if (vegetationItemInfo.UseSteepnessRule && persistentVegetationStorage.useSteepnessRule)
                if (vegetationItemInfo.UseAdvancedSteepnessRule == false)
                {
                    if (slopeAngle >= vegetationItemInfo.MaxSteepness || slopeAngle < vegetationItemInfo.MinSteepness)
                        return;
                }
                else
                {
                    if (RandomCutoff(SampleCurveArray(vegetationItemInfo.SteepnessRuleCurve.GenerateCurveArray(4096), slopeAngle, 90)))
                        return;
                }

            // scale
            float3 scale = new(1, 1, 1);
            if (persistentVegetationStorage.useScale)
                scale *= math.select(vegetationItemInfo.ScaleMultiplier * UnityEngine.Random.Range(vegetationItemInfo.MinScale, vegetationItemInfo.MaxScale),
                    vegetationItemInfo.ScaleMultiplier * math.clamp(vegetationItemInfo.MaxScale * SampleCurveArray(vegetationItemInfo.scaleRuleCurve.GenerateCurveArray(4096), UnityEngine.Random.Range(0f, 1f), 1), vegetationItemInfo.MinScale, vegetationItemInfo.MaxScale),
                    vegetationItemInfo.useAdvancedScaleRule);

            // terrain texture include/exclude/scale -- concave/convex
            if (_unityTerrain != null && _unityTerrain.Terrain != null && _unityTerrain.Terrain.terrainData != null)
            {
                int x = (int)((_worldPosition.x - _unityTerrain.transform.position.x) / _unityTerrain.alphamapScale.x); // cast-flooring used to conform with splatmapData "offset"
                int z = (int)((_worldPosition.z - _unityTerrain.transform.position.z) / _unityTerrain.alphamapScale.y); // cast-flooring used to conform with splatmapData "offset"

                if (vegetationItemInfo.UseTerrainTextureIncludeRules && persistentVegetationStorage.useTerrainTextureIncludeRule)   // terr tex include
                {
                    bool includeOnTexture = false;
                    for (int i = 0; i < vegetationItemInfo.TerrainTextureIncludeRuleList.Count; i++)
                    {
                        int splatmapChunkIndex = vegetationItemInfo.TerrainTextureIncludeRuleList[i].TextureIndex / 4;  // floor to int -- get current chunk/pass of the data
                        if (splatmapChunkIndex >= _unityTerrain.splatmapArrayList.Count)
                            continue;   // async filter

                        int splatmapTextureIndex = vegetationItemInfo.TerrainTextureIncludeRuleList[i].TextureIndex - (4 * splatmapChunkIndex); // get actual texture index within current chunk
                        if (_unityTerrain.splatmapFormatList[splatmapChunkIndex] == 1)  // old vs new terrainData compatibility adjustment
                        {
                            splatmapTextureIndex--;
                            if (splatmapTextureIndex == -1)
                                splatmapTextureIndex = 3;
                        }

                        int brightness = math.select(math.select(math.select(math.select(0,
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].A, splatmapTextureIndex == 3),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].B, splatmapTextureIndex == 2),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].G, splatmapTextureIndex == 1),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].R, splatmapTextureIndex == 0);

                        if (vegetationItemInfo.TerrainTextureIncludeRuleList[i].Inverse)
                            brightness = 255 - brightness;

                        if (brightness < (int)math.round(vegetationItemInfo.TerrainTextureIncludeRuleList[i].MinimumValue * 255)
                            || brightness > (int)math.round(vegetationItemInfo.TerrainTextureIncludeRuleList[i].MaximumValue * 255))    // if point is NOT on the terrain texture then exclude
                            includeOnTexture = false;
                        else
                        {
                            includeOnTexture = true;
                            break;
                        }
                    }

                    if (includeOnTexture == false)
                        return;
                }

                if (vegetationItemInfo.UseTerrainTextureExcludeRules && persistentVegetationStorage.useTerrainTextureExcludeRule)   // terr tex exclude
                {
                    for (int i = 0; i < vegetationItemInfo.TerrainTextureExcludeRuleList.Count; i++)
                    {
                        int splatmapChunkIndex = vegetationItemInfo.TerrainTextureExcludeRuleList[i].TextureIndex / 4;  // floor to int -- get current chunk/pass of the data
                        if (splatmapChunkIndex >= _unityTerrain.splatmapArrayList.Count)
                            continue;   // async filter

                        int splatmapTextureIndex = vegetationItemInfo.TerrainTextureExcludeRuleList[i].TextureIndex - (4 * splatmapChunkIndex); // get actual texture index within current chunk
                        if (_unityTerrain.splatmapFormatList[splatmapChunkIndex] == 1)  // old vs new terrainData compatibility adjustment
                        {
                            splatmapTextureIndex--;
                            if (splatmapTextureIndex == -1)
                                splatmapTextureIndex = 3;
                        }

                        int brightness = math.select(math.select(math.select(math.select(0,
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].A, splatmapTextureIndex == 3),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].B, splatmapTextureIndex == 2),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].G, splatmapTextureIndex == 1),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].R, splatmapTextureIndex == 0);

                        if (vegetationItemInfo.TerrainTextureExcludeRuleList[i].Inverse)
                            brightness = 255 - brightness;

                        if (brightness >= (int)math.round(vegetationItemInfo.TerrainTextureExcludeRuleList[i].MinimumValue * 255)
                            && brightness <= (int)math.round(vegetationItemInfo.TerrainTextureExcludeRuleList[i].MaximumValue * 255))   // if point is on the terrain texture then exclude
                            return;
                    }
                }

                if (vegetationItemInfo.UseTerrainTextureScaleRules && persistentVegetationStorage.useTerrainTextureScaleRule)   // terr tex scale
                {
                    for (int i = 0; i < vegetationItemInfo.TerrainTextureScaleRuleList.Count; i++)
                    {
                        int splatmapChunkIndex = vegetationItemInfo.TerrainTextureScaleRuleList[i].TextureIndex / 4;    // floor to int -- get current chunk/pass of the data
                        if (splatmapChunkIndex >= _unityTerrain.splatmapArrayList.Count)
                            continue;   // async filter

                        int splatmapTextureIndex = vegetationItemInfo.TerrainTextureScaleRuleList[i].TextureIndex - (4 * splatmapChunkIndex);   // get actual texture index within current chunk
                        if (_unityTerrain.splatmapFormatList[splatmapChunkIndex] == 1)  // old vs new terrainData compatibility adjustment
                        {
                            splatmapTextureIndex--;
                            if (splatmapTextureIndex == -1)
                                splatmapTextureIndex = 3;
                        }

                        int brightness = math.select(math.select(math.select(math.select(0,
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].A, splatmapTextureIndex == 3),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].B, splatmapTextureIndex == 2),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].G, splatmapTextureIndex == 1),
                            _unityTerrain.splatmapArrayList[splatmapChunkIndex][x + (z * _unityTerrain.Terrain.terrainData.alphamapResolution)].R, splatmapTextureIndex == 0);

                        if (vegetationItemInfo.TerrainTextureScaleRuleList[i].Inverse)
                            brightness = 255 - brightness;

                        if (!(brightness >= (int)math.round(vegetationItemInfo.TerrainTextureScaleRuleList[i].MinBrightness * 255)
                            && brightness <= (int)math.round(vegetationItemInfo.TerrainTextureScaleRuleList[i].MaxBrightness * 255)))   // if point is not on the terrain texture then don't affect scale
                        { }
                        else
                            scale *= math.clamp((brightness / (vegetationItemInfo.TerrainTextureScaleRuleList[i].BrightnessThreshold * 255)) * vegetationItemInfo.TerrainTextureScaleRuleList[i].ScaleMultiplier,
                                vegetationItemInfo.TerrainTextureScaleRuleList[i].MinimumValue, vegetationItemInfo.TerrainTextureScaleRuleList[i].MaximumValue);
                    }
                }

                if (vegetationItemInfo.UseConcaveLocationRule && persistentVegetationStorage.useConcaveLocationRule)    // concave/convex
                {
                    int2 xz = new((int)math.round((_worldPosition.x - _unityTerrain.transform.position.x) / _unityTerrain.Terrain.terrainData.heightmapScale.x),
                        (int)math.round((_worldPosition.z - _unityTerrain.transform.position.z) / _unityTerrain.Terrain.terrainData.heightmapScale.z));

                    int DistancePerSampleX = (int)math.round(vegetationItemInfo.ConcaveLocationDistance / _unityTerrain.Terrain.terrainData.heightmapScale.x);
                    int DistancePerSampleZ = (int)math.round(vegetationItemInfo.ConcaveLocationDistance / _unityTerrain.Terrain.terrainData.heightmapScale.z);

                    // calculate the average height of surrounding pixels -- w/ max offset distance for the "average square" => total "heightmapData" "coverage" per sample point
                    float heightLU = GetHeight(xz.x - DistancePerSampleX, xz.y + DistancePerSampleZ, _unityTerrain);    // up
                    float heightCU = GetHeight(xz.x, xz.y + DistancePerSampleZ, _unityTerrain);
                    float heightRU = GetHeight(xz.x + DistancePerSampleX, xz.y + DistancePerSampleZ, _unityTerrain);
                    float heightLC = GetHeight(xz.x - DistancePerSampleX, xz.y, _unityTerrain); // center
                    float heightCC = GetHeight(xz.x, xz.y, _unityTerrain);  // center (sample) point => gets compared against surrounding "average square" using eight sample points around the center point
                    float heightRC = GetHeight(xz.x + DistancePerSampleX, xz.y, _unityTerrain);
                    float heightLD = GetHeight(xz.x - DistancePerSampleX, xz.y - DistancePerSampleZ, _unityTerrain);    // down
                    float heightCD = GetHeight(xz.x, xz.y - DistancePerSampleZ, _unityTerrain);
                    float heightRD = GetHeight(xz.x + DistancePerSampleX, xz.y - DistancePerSampleZ, _unityTerrain);
                    float heightAverage = (heightLU + heightCU + heightRU + heightLC + heightRC + heightLD + heightCD + heightRD) / 8;  // "average square"

                    bool shouldExclude = heightAverage < (heightCC + vegetationItemInfo.ConcaveLocationMinHeightDifference);    // true if "convex" => exclude as points around the center are lower
                    if (vegetationItemInfo.ConcaveLocationInverse) shouldExclude = !shouldExclude;  // treat base rule as "convex"
                    if (shouldExclude == false) { } // points around the center aren't lower => "concave" so don't exclude
                    else return;
                }
            }

            if (vegetationItemInfo.UseTextureMaskIncludeRules && persistentVegetationStorage.useTextureMaskIncludeRule) // tex mask include
            {
                bool includeOnTextureOuter = false;
                for (int i = 0; i < vegetationItemInfo.TextureMaskIncludeRuleList.Count; i++)
                {
                    TextureMaskGroup maskGroup = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex].GetTextureMaskGroup(vegetationItemInfo.TextureMaskIncludeRuleList[i].TextureMaskGroupID);
                    if (maskGroup == null)
                        continue;

                    bool includeOnTextureInner = false;
                    for (int j = 0; j < maskGroup.TextureMaskList.Count; j++)
                    {
                        float2 texelSize = new(maskGroup.TextureMaskList[j].TextureRect.width / maskGroup.TextureMaskList[j].MaskTexture.width, maskGroup.TextureMaskList[j].TextureRect.height / maskGroup.TextureMaskList[j].MaskTexture.height);
                        int x = (int)math.round(math.frac((_worldPosition.x - maskGroup.TextureMaskList[j].TextureRect.position.x) / texelSize.x / maskGroup.TextureMaskList[j].MaskTexture.width * maskGroup.TextureMaskList[j].Repeat.x) * maskGroup.TextureMaskList[j].MaskTexture.width);
                        int z = (int)math.round(math.frac((_worldPosition.z - maskGroup.TextureMaskList[j].TextureRect.position.y) / texelSize.y / maskGroup.TextureMaskList[j].MaskTexture.height * maskGroup.TextureMaskList[j].Repeat.y) * maskGroup.TextureMaskList[j].MaskTexture.height);

                        if (x < 0 || x >= maskGroup.TextureMaskList[j].MaskTexture.width || z < 0 || z >= maskGroup.TextureMaskList[j].MaskTexture.height)
                            return;

                        int channel = vegetationItemInfo.TextureMaskIncludeRuleList[i].GetIntPropertyValue("ChannelSelector");
                        NativeArray<RGBABytes> RGBAChannelArray = maskGroup.TextureMaskList[j].MaskTexture.GetRawTextureData<RGBABytes>();
                        int brightness = math.select(math.select(math.select(math.select(0,
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].A, channel == 3),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].B, channel == 2),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].G, channel == 1),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].R, channel == 0);

                        bool inverse = vegetationItemInfo.TextureMaskIncludeRuleList[i].GetBooleanPropertyValue("Inverse");
                        if (inverse)
                            brightness = 255 - brightness;

                        if (brightness < (int)math.round(vegetationItemInfo.TextureMaskIncludeRuleList[i].MinDensity * 255)
                            || brightness > (int)math.round(vegetationItemInfo.TextureMaskIncludeRuleList[i].MaxDensity * 255)) // if point is NOT on the terrain texture then exclude
                        {
                            if (inverse)
                            {
                                includeOnTextureInner = false;
                                break;
                            }
                        }
                        else
                        {
                            if (inverse)
                                includeOnTextureInner = true;
                            else
                            {
                                includeOnTextureInner = true;
                                break;
                            }
                        }
                    }

                    if (includeOnTextureInner)
                        includeOnTextureOuter = true;
                    else
                    {
                        includeOnTextureOuter = false;
                        break;
                    }
                }

                if (includeOnTextureOuter == false)
                    return;
            }

            if (vegetationItemInfo.UseTextureMaskExcludeRules && persistentVegetationStorage.useTextureMaskExcludeRule) // tex mask exclude
                for (int i = 0; i < vegetationItemInfo.TextureMaskExcludeRuleList.Count; i++)
                {
                    TextureMaskGroup maskGroup = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex].GetTextureMaskGroup(vegetationItemInfo.TextureMaskExcludeRuleList[i].TextureMaskGroupID);
                    if (maskGroup == null)
                        continue;

                    for (int j = 0; j < maskGroup.TextureMaskList.Count; j++)
                    {
                        float2 texelSize = new(maskGroup.TextureMaskList[j].TextureRect.width / maskGroup.TextureMaskList[j].MaskTexture.width, maskGroup.TextureMaskList[j].TextureRect.height / maskGroup.TextureMaskList[j].MaskTexture.height);
                        int x = (int)math.round(math.frac((_worldPosition.x - maskGroup.TextureMaskList[j].TextureRect.position.x) / texelSize.x / maskGroup.TextureMaskList[j].MaskTexture.width * maskGroup.TextureMaskList[j].Repeat.x) * maskGroup.TextureMaskList[j].MaskTexture.width);
                        int z = (int)math.round(math.frac((_worldPosition.z - maskGroup.TextureMaskList[j].TextureRect.position.y) / texelSize.y / maskGroup.TextureMaskList[j].MaskTexture.height * maskGroup.TextureMaskList[j].Repeat.y) * maskGroup.TextureMaskList[j].MaskTexture.height);

                        if (x < 0 || x >= maskGroup.TextureMaskList[j].MaskTexture.width || z < 0 || z >= maskGroup.TextureMaskList[j].MaskTexture.height)
                            return;

                        int channel = vegetationItemInfo.TextureMaskExcludeRuleList[i].GetIntPropertyValue("ChannelSelector");
                        NativeArray<RGBABytes> RGBAChannelArray = maskGroup.TextureMaskList[j].MaskTexture.GetRawTextureData<RGBABytes>();
                        int brightness = math.select(math.select(math.select(math.select(0,
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].A, channel == 3),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].B, channel == 2),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].G, channel == 1),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].R, channel == 0);

                        if (vegetationItemInfo.TextureMaskExcludeRuleList[i].GetBooleanPropertyValue("Inverse"))
                            brightness = 255 - brightness;

                        if (brightness >= (int)math.round(vegetationItemInfo.TextureMaskExcludeRuleList[i].MinDensity * 255)
                            && brightness <= (int)math.round(vegetationItemInfo.TextureMaskExcludeRuleList[i].MaxDensity * 255))    // if point is on the texture mask then exclude
                            return;
                    }
                }

            if (vegetationItemInfo.UseTextureMaskScaleRules && persistentVegetationStorage.useTextureMaskScaleRule) // tex mask scale
                for (int i = 0; i < vegetationItemInfo.TextureMaskScaleRuleList.Count; i++)
                {
                    TextureMaskGroup maskGroup = persistentVegetationStorage.vegetationSystemPro.vegetationPackageProList[persistentVegetationStorage.selectedVegetationPackageIndex].GetTextureMaskGroup(vegetationItemInfo.TextureMaskScaleRuleList[i].TextureMaskGroupID);
                    if (maskGroup == null)
                        continue;

                    for (int j = 0; j < maskGroup.TextureMaskList.Count; j++)
                    {
                        float2 texelSize = new(maskGroup.TextureMaskList[j].TextureRect.width / maskGroup.TextureMaskList[j].MaskTexture.width, maskGroup.TextureMaskList[j].TextureRect.height / maskGroup.TextureMaskList[j].MaskTexture.height);
                        int x = (int)math.round(math.frac((_worldPosition.x - maskGroup.TextureMaskList[j].TextureRect.position.x) / texelSize.x / maskGroup.TextureMaskList[j].MaskTexture.width * maskGroup.TextureMaskList[j].Repeat.x) * maskGroup.TextureMaskList[j].MaskTexture.width);
                        int z = (int)math.round(math.frac((_worldPosition.z - maskGroup.TextureMaskList[j].TextureRect.position.y) / texelSize.y / maskGroup.TextureMaskList[j].MaskTexture.height * maskGroup.TextureMaskList[j].Repeat.y) * maskGroup.TextureMaskList[j].MaskTexture.height);

                        if (x < 0 || x >= maskGroup.TextureMaskList[j].MaskTexture.width || z < 0 || z >= maskGroup.TextureMaskList[j].MaskTexture.height)
                            return;

                        int channel = vegetationItemInfo.TextureMaskScaleRuleList[i].GetIntPropertyValue("ChannelSelector");
                        NativeArray<RGBABytes> RGBAChannelArray = maskGroup.TextureMaskList[j].MaskTexture.GetRawTextureData<RGBABytes>();
                        int brightness = math.select(math.select(math.select(math.select(0,
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].A, channel == 3),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].B, channel == 2),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].G, channel == 1),
                            RGBAChannelArray[x + (z * maskGroup.TextureMaskList[j].MaskTexture.width)].R, channel == 0);

                        if (vegetationItemInfo.TextureMaskScaleRuleList[i].GetBooleanPropertyValue("Inverse"))
                            brightness = 255 - brightness;

                        if (!(brightness >= (int)math.round(vegetationItemInfo.TextureMaskScaleRuleList[i].MinBrightness * 255)
                            && brightness <= (int)math.round(vegetationItemInfo.TextureMaskScaleRuleList[i].MaxBrightness * 255)))  // if point is not on the terrain texture then don't affect scale
                        { }
                        else
                            scale *= math.clamp((brightness / (vegetationItemInfo.TextureMaskScaleRuleList[i].BrightnessThreshold * 255)) * vegetationItemInfo.TextureMaskScaleRuleList[i].ScaleMultiplier,
                                vegetationItemInfo.TextureMaskScaleRuleList[i].MinDensity, vegetationItemInfo.TextureMaskScaleRuleList[i].MaxDensity);
                    }
                }

            quaternion rotation = quaternion.identity;
            float3 lookAt;
            float3 angleScale = float3.zero;
            switch (vegetationItemInfo.RotationMode)    // rotation (using radians)
            {
                case VegetationRotationType.RotateY:
                    rotation = quaternion.RotateY(UnityEngine.Random.Range(0, 6.28318530718f));
                    break;
                case VegetationRotationType.RotateXYZ:
                    rotation = quaternion.Euler(new float3(UnityEngine.Random.Range(0, 6.28318530718f), UnityEngine.Random.Range(0, 6.28318530718f), UnityEngine.Random.Range(0, 6.28318530718f)));
                    break;
                case VegetationRotationType.FollowTerrain:
                    lookAt = math.cross(-_terrainNormal, new float3(1, 0, 0));
                    lookAt = lookAt.y < 0 ? -lookAt : lookAt;
                    rotation = math.mul(quaternion.LookRotation(lookAt, _terrainNormal), quaternion.AxisAngle(new float3(0, 1, 0), UnityEngine.Random.Range(0, 6.28318530718f)));
                    break;
                case VegetationRotationType.FollowTerrainScale:
                    lookAt = math.cross(-_terrainNormal, new float3(1, 0, 0));
                    lookAt = lookAt.y < 0 ? -lookAt : lookAt;
                    rotation = math.mul(quaternion.LookRotation(lookAt, _terrainNormal), quaternion.AxisAngle(new float3(0, 1, 0), UnityEngine.Random.Range(0, 6.28318530718f)));
                    float newScale = math.clamp(slopeAngle / 45f, 0, 1);
                    angleScale = new float3(newScale, 0, newScale);
                    break;
            }

            if (persistentVegetationStorage.usePositionOffsetRange) // position
                _worldPosition.y += UnityEngine.Random.Range(vegetationItemInfo.MinUpOffset, vegetationItemInfo.MaxUpOffset);

            VegetationStudioManager.AddVegetationItemInstanceEx(_vegetationItemID, _worldPosition, new float3(scale.x + angleScale.x, scale.y + angleScale.y, scale.z + angleScale.z), rotation, 5, _sampleDistance, 1);
        }

        private bool RandomCutoff(float _value)
        {
            return !(_value > UnityEngine.Random.Range(0, 1));
        }

        private float SampleCurveArray(float[] _curveArray, float _value, float _maxValue)
        {
            if (_curveArray.Length == 0)
                return 0f;

            int index = (int)math.round((_value / _maxValue) * _curveArray.Length);
            return _curveArray[math.clamp(index, 0, _curveArray.Length - 1)];
        }

        float GetHeight(int _x, int _y, UnityTerrain _unityTerrain)
        {
            _x = math.clamp(_x, 0, _unityTerrain.Terrain.terrainData.heightmapResolution - 1);
            _y = math.clamp(_y, 0, _unityTerrain.Terrain.terrainData.heightmapResolution - 1);
            return _unityTerrain.heights[_x + (_y * _unityTerrain.Terrain.terrainData.heightmapResolution)] * _unityTerrain.Terrain.terrainData.heightmapScale.y;
        }

        private float3 RandomizePosition(float3 _position, float _sampleDistance)
        {
            float randomDistanceFactor = _sampleDistance * 0.25f;
            return _position + new float3(UnityEngine.Random.Range(-randomDistanceFactor, randomDistanceFactor));
        }

        private bool SampleBrushPosition(float3 _worldPosition, float3 _corner)
        {
            float3 position = _worldPosition - _corner;
            float xNormalized = (position.x / vegetationBrush.size) * 0.5f;
            float zNormalized = (position.z / vegetationBrush.size) * 0.5f;

            Texture2D currentBrushTexture = brushTextures[persistentVegetationStorage.selectedBrushIndex] as Texture2D;
            if (currentBrushTexture == null)
                return false;

            int x = math.clamp((int)math.round(xNormalized * currentBrushTexture.width), 0, currentBrushTexture.width);
            int z = math.clamp((int)math.round(zNormalized * currentBrushTexture.height), 0, currentBrushTexture.height);

            Color color = currentBrushTexture.GetPixel(x, z);
            if (color.a > 0.1f)
                return true;
            else
                return false;
        }

        private float3 AllignToTerrain(float3 _position, out float3 _normal, out UnityTerrain _unityTerrain)
        {
            RaycastHit[] hits = Physics.RaycastAll(new(_position + new float3(0, 10000, 0), new float3(0, -1, 0))).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
                if (hits[i].collider is TerrainCollider || persistentVegetationStorage.groundLayerMask.Contains(hits[i].collider.gameObject.layer))
                {
                    _normal = math.normalize(hits[i].normal);
                    _unityTerrain = hits[i].collider is TerrainCollider ? hits[i].collider.GetComponent<UnityTerrain>() : null;
                    return hits[i].point;
                }

            _unityTerrain = null;
            _normal = new float3(0, 1, 0);
            return _position;
        }

        private void GetActiveBrush(int _size)
        {
            if (vegetationBrush == null)
                vegetationBrush = new VegetationBrush();
            vegetationBrush.Load(brushTextures[persistentVegetationStorage.selectedBrushIndex] as Texture2D, _size);
        }

        private bool GetPreviewBrushData(ref float3 _hitPosition)
        {
            GetActiveBrush((int)math.ceil(persistentVegetationStorage.brushSize));
            return Raycast(ref _hitPosition, out _);
        }

        private bool Raycast(ref float3 _pos, out float2 _uv)
        {
            RaycastHit[] hits = Physics.RaycastAll(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition)).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
                if (hits[i].collider is TerrainCollider || persistentVegetationStorage.groundLayerMask.Contains(hits[i].collider.gameObject.layer))
                {
                    _uv = hits[i].textureCoord;
                    _pos = hits[i].point;
                    return true;
                }

            _uv = float2.zero;
            _pos = float3.zero;
            return false;
        }
        #endregion
    }
}