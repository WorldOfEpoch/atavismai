using AwesomeTechnologies.VegetationSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.PrefabSpawner
{
    [CustomEditor(typeof(RuntimePrefabSpawner))]
    public class RuntimePrefabSpawnerEditor : VegetationStudioProBaseEditor
    {
        private RuntimePrefabSpawner runtimePrefabSpawner;
        private int vegIndex;
        private int lastVegIndex;
        private int selectedGridIndex;
        private int selectedVegetationTypeIndex;
        private readonly string[] TabNames = { "Settings", "Debug" };
        private readonly string[] VegetationTypeNames = { "All", "Trees", "Large Objects", "Objects", "Plants", "Grass" };

        public override void OnInspectorGUI()
        {
            runtimePrefabSpawner = (RuntimePrefabSpawner)target;
            base.OnInspectorGUI();

            if (runtimePrefabSpawner.vegetationSystemPro == null)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                GUILayout.EndVertical();
                return;
            }

            if (runtimePrefabSpawner.enabled == false)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Component is disabled\nEnable it to use run-time prefab spawning", MessageType.Warning);
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical("Box");
            EditorGUILayout.HelpBox("The runtime prefab spawner allows for adding details to vegetation instances like sounds, particle effects or any other prefab\nThis system is only active while the scene is running", MessageType.Info);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("Box");
            runtimePrefabSpawner.currentTabIndex = GUILayout.SelectionGrid(runtimePrefabSpawner.currentTabIndex, TabNames, 2, EditorStyles.toolbarButton);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            switch (runtimePrefabSpawner.currentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawDebugInspector();
                    break;
            }
        }

        private void DrawSettingsInspector()
        {
            GUILayout.BeginVertical("box");

            if (runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            string[] packageNameList = new string[runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i < runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                if (runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " + runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList[i].PackageName + " (" + runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";
            }

            EditorGUI.BeginChangeCheck();
            runtimePrefabSpawner.vegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", runtimePrefabSpawner.vegetationPackageIndex, packageNameList);
            EditorGUILayout.Space();

            if (runtimePrefabSpawner.vegetationPackageIndex > runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList.Count - 1)
                runtimePrefabSpawner.vegetationPackageIndex = runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList.Count - 1;

            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            VegetationPackagePro vegetationPackagePro = runtimePrefabSpawner.vegetationSystemPro.vegetationPackageProList[runtimePrefabSpawner.vegetationPackageIndex];
            if (vegetationPackagePro == null) return;
            if (vegetationPackagePro.VegetationInfoList.Count == 0) return;

            EditorGUI.BeginChangeCheck();
            selectedVegetationTypeIndex = GUILayout.SelectionGrid(selectedVegetationTypeIndex, VegetationTypeNames, 3, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
                selectedGridIndex = 0;

            VegetationPackageEditorTools.VegetationItemTypeSelection vegetationItemTypeSelection = VegetationPackageEditorTools.GetVegetationItemTypeSelection(selectedVegetationTypeIndex);

            int selectionCount = 0;
            VegetationPackageEditorTools.DrawVegetationItemSelector(runtimePrefabSpawner.vegetationSystemPro, vegetationPackagePro, ref selectedGridIndex, ref vegIndex, ref selectionCount, vegetationItemTypeSelection, 70);

            if (lastVegIndex != vegIndex)
                GUI.FocusControl(null);
            lastVegIndex = vegIndex;

            VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[vegIndex];
            if (vegetationItemInfoPro == null)
                return;

            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Selected item", vegetationItemInfoPro.Name);
            if (GUILayout.Button("Add run-time prefab rule"))
            {
                RuntimePrefabRule newRuntimePrefabRule = new();
                newRuntimePrefabRule.SetSeed();
                if (vegetationItemInfoPro.VegetationType == VegetationType.Grass || vegetationItemInfoPro.VegetationType == VegetationType.Plant || vegetationItemInfoPro.VegetationType == VegetationType.Objects)
                    newRuntimePrefabRule.SpawnFrequency = 0.1f; // safety check to not overload the scene

                vegetationItemInfoPro.RuntimePrefabRuleList.Add(newRuntimePrefabRule);
                runtimePrefabSpawner.RefreshRuntimePrefabs();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }
            GUILayout.EndVertical();

            for (int i = 0; i < vegetationItemInfoPro.RuntimePrefabRuleList.Count; i++)
            {
                RuntimePrefabRule runtimePrefabRule = vegetationItemInfoPro.RuntimePrefabRuleList[i];
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginVertical("box");
                runtimePrefabRule.RuntimePrefab = EditorGUILayout.ObjectField("Run-time prefab", runtimePrefabRule.RuntimePrefab, typeof(GameObject), true) as GameObject;
                if (runtimePrefabRule.RuntimePrefab == null)
                {
                    GameObject fallbackCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fallbackCube.hideFlags = HideFlags.HideAndDontSave;
                    runtimePrefabRule.RuntimePrefab = fallbackCube;
                }

                Texture2D prefabTexture = AssetPreview.GetAssetPreview(runtimePrefabRule.RuntimePrefab);
                Texture2D convertedPrefabTexture = new(2, 2, TextureFormat.RGBA32, false, false);

                if (Application.isPlaying)
                    convertedPrefabTexture = prefabTexture;
                else
                {
                    if (prefabTexture)
                        convertedPrefabTexture.LoadImage(prefabTexture.EncodeToPNG());
                }

                if (convertedPrefabTexture)
                {
                    Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(convertedPrefabTexture.height));
                    float width = space.width;
                    space.xMin = (width - convertedPrefabTexture.width);
                    if (space.xMin < 0)
                        space.xMin = 0;
                    space.width = convertedPrefabTexture.width;
                    space.height = convertedPrefabTexture.height;
                    EditorGUI.DrawPreviewTexture(space, convertedPrefabTexture);
                }

                runtimePrefabRule.SpawnFrequency = EditorGUILayout.Slider("Spawn frequency", runtimePrefabRule.SpawnFrequency, 0, 1f);
                runtimePrefabRule.PrefabScale = math.max(EditorGUILayout.Vector3Field("Scale", runtimePrefabRule.PrefabScale), float3.zero);
                runtimePrefabRule.UseVegetationItemScale = EditorGUILayout.Toggle("Apply vegetation item scale", runtimePrefabRule.UseVegetationItemScale);
                runtimePrefabRule.PrefabRotation = EditorGUILayout.Vector3Field("Rotation", runtimePrefabRule.PrefabRotation);
                runtimePrefabRule.PrefabOffset = EditorGUILayout.Vector3Field("Offset", runtimePrefabRule.PrefabOffset);
                runtimePrefabRule.PrefabLayer = EditorGUILayout.LayerField("Prefab layer", runtimePrefabRule.PrefabLayer);
                runtimePrefabRule.Seed = EditorGUILayout.IntSlider("Seed", runtimePrefabRule.Seed, 0, 99);
                runtimePrefabRule.UsePool = EditorGUILayout.Toggle("Use pooling system", runtimePrefabRule.UsePool);

                runtimePrefabRule.DistanceFactor = EditorGUILayout.Slider("Distance factor", runtimePrefabRule.DistanceFactor, 0, 1f);
                float currentDistance = runtimePrefabSpawner.vegetationSystemPro.vegetationSettings.GetGrassDistance() * runtimePrefabRule.DistanceFactor;  // based on grass distance
                EditorGUILayout.HelpBox("The distance from the camera in which prefabs get instantiated\nThis is based on the global grass render distance" + "\nGeneration distance: " + currentDistance.ToString("F2"), MessageType.Info);

                if (EditorGUI.EndChangeCheck())
                {
                    runtimePrefabSpawner.RefreshRuntimePrefabs();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                }

                if (GUILayout.Button("Remove run-time prefab rule"))
                {
                    vegetationItemInfoPro.RuntimePrefabRuleList.Remove(runtimePrefabRule);
                    runtimePrefabSpawner.RefreshRuntimePrefabs();
                    EditorUtility.SetDirty(vegetationPackagePro);
                    SetSceneDirty();
                    GUILayout.EndVertical();
                    return;
                }

                GUILayout.EndVertical();
            }
        }

        private void DrawDebugInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Debug info", labelStyle);

            EditorGUI.BeginChangeCheck();
            runtimePrefabSpawner.showDebugCells = EditorGUILayout.Toggle("Show affected cells", runtimePrefabSpawner.showDebugCells);
            EditorGUILayout.HelpBox("Show the affected vegetation cells in the scene", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            EditorGUI.BeginChangeCheck();
            runtimePrefabSpawner.showRuntimePrefabs = EditorGUILayout.Toggle("Show run-time prefabs", runtimePrefabSpawner.showRuntimePrefabs);
            EditorGUILayout.HelpBox("Show the run-time spawned prefabs in the hierarchy", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                runtimePrefabSpawner.SetRuntimePrefabVisibility(runtimePrefabSpawner.showRuntimePrefabs);
                EditorApplication.RepaintHierarchyWindow();
                SetSceneDirty();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime info", labelStyle);
            if (runtimePrefabSpawner.visibleVegetationCellSelector != null)
            {
                if (runtimePrefabSpawner.vegetationSystemPro.vegetationSettings.grassDistance <= 0)
                    EditorGUILayout.HelpBox("GameObjects are disabled from spawning since the grass distance is set to zero", MessageType.Warning);

                EditorGUILayout.LabelField("Visible cells: " + runtimePrefabSpawner.visibleVegetationCellSelector.visibleSelectorVegetationCellList.Count.ToString());
                EditorGUILayout.LabelField("Loaded instances: " + runtimePrefabSpawner.GetLoadedInstanceCount());
                EditorGUILayout.LabelField("Visible instances: " + runtimePrefabSpawner.GetVisibleColliders());
            }
            else
            {
                EditorGUILayout.HelpBox("Prefab run-time info only shows while the scene is running", MessageType.Info);
            }
            GUILayout.EndVertical();
        }

        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(runtimePrefabSpawner);
        }
    }
}