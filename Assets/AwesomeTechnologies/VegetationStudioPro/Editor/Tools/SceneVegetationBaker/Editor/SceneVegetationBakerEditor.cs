using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(SceneVegetationBaker))]
    public class SceneVegetationBakerEditor : VegetationStudioProBaseEditor
    {
        public VegetationSystemPro vegetationSystemPro;
        private SceneVegetationBaker sceneVegetationBaker;
        private int vegIndex;
        private int lastVegIndex;
        private int selectedGridIndex;
        private int selectedVegetationTypeIndex;
        private readonly string[] VegetationTypeNames = { "All", "Trees", "Large Objects", "Objects", "Plants", "Grass" };

        public override void OnInspectorGUI()
        {
            sceneVegetationBaker = (SceneVegetationBaker)target;
            base.OnInspectorGUI();

            vegetationSystemPro = sceneVegetationBaker.GetComponent<VegetationSystemPro>();
            if (vegetationSystemPro == null)
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                return;
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("This component bakes vegetation instances to the hierarchy as GameObjects", MessageType.Info);
            EditorGUILayout.HelpBox("Baking high densities of gameObjects for permanent use is not recommended\n-> The engine can't handle too many gameObjects => colliders", MessageType.Warning);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            if (vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            string[] packageNameList = new string[vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
            {
                if (vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " + vegetationSystemPro.vegetationPackageProList[i].PackageName + " (" + vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";
            }

            EditorGUI.BeginChangeCheck();
            sceneVegetationBaker.vegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", sceneVegetationBaker.vegetationPackageIndex, packageNameList);
            EditorGUILayout.Space();

            if (sceneVegetationBaker.vegetationPackageIndex > vegetationSystemPro.vegetationPackageProList.Count - 1)
                sceneVegetationBaker.vegetationPackageIndex = vegetationSystemPro.vegetationPackageProList.Count - 1;

            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            VegetationPackagePro vegetationPackagePro = vegetationSystemPro.vegetationPackageProList[sceneVegetationBaker.vegetationPackageIndex];
            if (vegetationPackagePro == null) return;
            if (vegetationPackagePro.VegetationInfoList.Count == 0) return;

            EditorGUI.BeginChangeCheck();
            selectedVegetationTypeIndex = GUILayout.SelectionGrid(selectedVegetationTypeIndex, VegetationTypeNames, 3, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
                selectedGridIndex = 0;

            VegetationPackageEditorTools.VegetationItemTypeSelection vegetationItemTypeSelection = VegetationPackageEditorTools.GetVegetationItemTypeSelection(selectedVegetationTypeIndex);

            int selectionCount = 0;
            VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationSystemPro, vegetationPackagePro, ref selectedGridIndex, ref vegIndex, ref selectionCount, vegetationItemTypeSelection, 70);

            if (lastVegIndex != vegIndex)
                GUI.FocusControl(null);
            lastVegIndex = vegIndex;

            GUILayout.EndVertical();

            VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[vegIndex];
            if (vegetationItemInfoPro == null) return;

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Selected item", vegetationItemInfoPro.Name);
            if (GUILayout.Button("Bake the selected vegetation item"))
                if (EditorUtility.DisplayDialog("Bake vegetation", "Selected biome: " + vegetationPackagePro.PackageName + "(" + vegetationPackagePro.BiomeType + ")" +
                    "\nSelected action: Bake the selected vegetation item" +
                    "\nEnsure the scene has been saved before baking\n\nHigh density vegetation items can take hours to bake/convert into gameObjects even on high end setups", "Confirm", "Cancel"))
                    BakeVegetationToScene(vegetationItemInfoPro);

            EditorGUI.BeginChangeCheck();
            sceneVegetationBaker.excludeGrass = EditorGUILayout.Toggle("Exclude grass", sceneVegetationBaker.excludeGrass);
            sceneVegetationBaker.excludePlants = EditorGUILayout.Toggle("Exclude plants", sceneVegetationBaker.excludePlants);
            sceneVegetationBaker.excludeObjects = EditorGUILayout.Toggle("Exclude objects", sceneVegetationBaker.excludeObjects);
            sceneVegetationBaker.excludeLargeObjects = EditorGUILayout.Toggle("Exclude large objects", sceneVegetationBaker.excludeLargeObjects);
            sceneVegetationBaker.excludeTrees = EditorGUILayout.Toggle("Exclude trees", sceneVegetationBaker.excludeTrees);
            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            if (GUILayout.Button("Bake the selected vegetation package / biome"))
                if (EditorUtility.DisplayDialog("Bake vegetation", "Selected biome: " + vegetationPackagePro.PackageName + "(" + vegetationPackagePro.BiomeType + ")" +
                    "\nSelected action: Bake ALL" +
                    "\nEnsure the scene has been saved before baking\n\nHigh density vegetation items can take hours to bake/convert into gameObjects even on high end setups", "Confirm", "Cancel"))
                    for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
                        BakeVegetationToScene(vegetationPackagePro.VegetationInfoList[i]);

            if (GUILayout.Button("Bake ALL vegetation packages / biomes"))
                if (EditorUtility.DisplayDialog("Bake vegetation", "Selected biome: All Biomes\nSelected action: Bake ALL" +
                    "\nEnsure the scene has been saved before baking\n\nHigh density vegetation items can take hours to bake/convert into gameObjects even on high end setups", "Confirm", "Cancel"))
                    for (int i = 0; i < vegetationSystemPro.vegetationPackageProList.Count; i++)
                        for (int j = 0; j < vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                            BakeVegetationToScene(vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j]);

            GUILayout.EndVertical();
        }

        void BakeVegetationToScene(VegetationItemInfoPro _vegetationItemInfo)
        {
            if (vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfo.VegetationItemID) == null) return;
            if (vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfo.VegetationItemID).vegetationModel == null) return;

            if (sceneVegetationBaker.excludeGrass && _vegetationItemInfo.VegetationType == VegetationType.Grass) return;
            if (sceneVegetationBaker.excludePlants && _vegetationItemInfo.VegetationType == VegetationType.Plant) return;
            if (sceneVegetationBaker.excludeTrees && _vegetationItemInfo.VegetationType == VegetationType.Tree) return;
            if (sceneVegetationBaker.excludeObjects && _vegetationItemInfo.VegetationType == VegetationType.Objects) return;
            if (sceneVegetationBaker.excludeLargeObjects && _vegetationItemInfo.VegetationType == VegetationType.LargeObjects) return;

            GameObject root = new() { name = "BakedVegetationItem_" + _vegetationItemInfo.Name };

            vegetationSystemPro.ClearCache(_vegetationItemInfo.VegetationItemID);

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                EditorUtility.DisplayProgressBar("Bake vegetation item: " + _vegetationItemInfo.Name, "Spawn all cells", 0);
#endif

            for (int i = 0; i < vegetationSystemPro.vegetationCellList.Count; i++)
            {
#if UNITY_EDITOR
                if (i % 10 == 0 && Application.isPlaying == false)
                    EditorUtility.DisplayProgressBar("Bake vegetation item: " + _vegetationItemInfo.Name, "Spawn cell " + i + "/" + (vegetationSystemPro.vegetationCellList.Count - 1), i / ((float)vegetationSystemPro.vegetationCellList.Count - 1));
#endif

                vegetationSystemPro.SpawnVegetationCellEx(vegetationSystemPro.vegetationCellList[i], _vegetationItemInfo.VegetationItemID);
                NativeList<MatrixInstance> matrixInstanceList = vegetationSystemPro.GetVegetationItemInstances(vegetationSystemPro.vegetationCellList[i], _vegetationItemInfo.VegetationItemID);

                for (int j = 0; j < matrixInstanceList.Length; j++)
                {
                    if (matrixInstanceList[j].controlData.x <= 0)
                        continue;   // skip masked out persistent vegetation storage vegetation instances

                    GameObject vegetationItem = PrefabUtility.InstantiatePrefab(vegetationSystemPro.GetVegetationItemModelInfo(_vegetationItemInfo.VegetationItemID).vegetationModel, root.transform) as GameObject;
                    vegetationItem.transform.SetPositionAndRotation(MatrixTools.ExtractTranslationFromMatrix(matrixInstanceList[j].matrix), MatrixTools.ExtractRotationFromMatrix(matrixInstanceList[j].matrix));
                    vegetationItem.transform.localScale = MatrixTools.ExtractScaleFromMatrix(matrixInstanceList[j].matrix);
                }

                vegetationSystemPro.vegetationCellList[i].ClearCache();
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                EditorUtility.ClearProgressBar();
            SetSceneDirty();
#endif
        }

        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(sceneVegetationBaker);
        }
    }
}
