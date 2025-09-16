using AwesomeTechnologies.ColliderSystem;
using AwesomeTechnologies.PrefabSpawner;
using AwesomeTechnologies.TerrainSystem;
#if TOUCH_REACT
using AwesomeTechnologies.TouchReact;
#endif
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
using UnityEngine;
using UnityEditor;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(VegetationStudioManager))]
    public class VegetationStudioManagerEditor : VegetationStudioProBaseEditor
    {
        private VegetationStudioManager vegetationStudioManager;
        private readonly string[] TabNames = { "Settings", "PPv2 volumes" };

        #region "MenuItem" shortcuts
        [MenuItem("Window/Awesome Technologies/Shortcuts/Refresh \"everything\" %#_t", secondaryPriority = 0)]
        static void RecalculateTotalArea()
        {
            VegetationStudioManager t = VegetationStudioManager.Instance;
            for (int i = 0; i < t.VegetationSystemList.Count; i++)
                if (t.VegetationSystemList[i])
                    t.VegetationSystemList[i].RefreshEverything();
        }

        [MenuItem("Window/Awesome Technologies/Shortcuts/Regenerate all splat maps %#_e", secondaryPriority = 1)]
        static void RegenerateSplatmap()
        {
            VegetationStudioManager t = VegetationStudioManager.Instance;
            for (int i = 0; i < t.VegetationSystemList.Count; i++)
            {
                if (t.VegetationSystemList[i] == null) continue;
                TerrainSystemPro tsp = t.VegetationSystemList[i].GetComponent<TerrainSystemPro>();
                if (tsp == null) continue;
                tsp.GenerateSplatMap(t.VegetationSystemList[i].vegetationSystemBounds, false);
                tsp.EnableTerrainHeatmap(false);
            }
        }

        [MenuItem("Window/Awesome Technologies/Systems/Add \"Vegetation Studio Pro\" to the hierarchy", secondaryPriority = 0)]
        public static void AddVegetationStudioManager()
        {
            if (FindAnyObjectByType<VegetationStudioManager>())
            {
                EditorUtility.DisplayDialog("Vegetation Studio Pro Manager", "There already is a Vegetation Studio Pro Manager in the scene, only one should be used", "OK");
            }
            else
            {
                GameObject go = new() { name = "VegetationStudioProManager" };
                go.AddComponent<VegetationStudioManager>();

                GameObject vegetationSystem = new() { name = "VegetationSystemPro" };
                vegetationSystem.transform.SetParent(go.transform);
                VegetationSystemPro vegetationSystemPro = vegetationSystem.AddComponent<VegetationSystemPro>();
                vegetationSystem.AddComponent<TerrainSystemPro>();
                vegetationSystemPro.AddAllUnityTerrains();

                vegetationSystem.AddComponent<ColliderSystemPro>();
                vegetationSystem.AddComponent<PersistentVegetationStorage>();
                RuntimePrefabSpawner runtimePrefabSpawner = vegetationSystem.AddComponent<RuntimePrefabSpawner>();
                runtimePrefabSpawner.enabled = false;

                EditorGUIUtility.PingObject(go);
            }
        }

#if TOUCH_REACT
        [MenuItem("Window/Awesome Technologies/Systems/Add \"Touch React System\" to the hierarchy", secondaryPriority = 1)]
        static void AddTouchReact()
        {
            if (FindAnyObjectByType<TouchReactSystem>())
            {
                EditorUtility.DisplayDialog("Touch React System", "There already is a Touch React System in the scene, only one should be used", "OK");
                return;
            }

            GameObject go = new() { name = "TouchReactSystem" };
            go.AddComponent<TouchReactSystem>();
            if (VegetationStudioManager.Instance)
                go.transform.SetParent(VegetationStudioManager.Instance.transform);
            EditorGUIUtility.PingObject(go);
        }
#endif
        #endregion

        public override void OnInspectorGUI()
        {
            vegetationStudioManager = (VegetationStudioManager)target;
            showLogo = true;
            largeLogo = true;
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical("Box");
            vegetationStudioManager.currentTabIndex = GUILayout.SelectionGrid(vegetationStudioManager.currentTabIndex, TabNames, 2, EditorStyles.toolbarButton);
            EditorGUILayout.EndVertical();

            switch (vegetationStudioManager.currentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawPostProcessInspector();
                    break;
            }
        }

        private void DrawPostProcessInspector()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add profiles", labelStyle);

            EditorGUI.BeginChangeCheck();
            PostProcessProfile newPostProcessProfile = (PostProcessProfile)EditorGUILayout.ObjectField("", null, typeof(PostProcessProfile), false);
            if (EditorGUI.EndChangeCheck())
            {
                vegetationStudioManager.AddPostProcessProfile(newPostProcessProfile);
                EditorUtility.SetDirty(vegetationStudioManager);
            }

            EditorGUILayout.HelpBox("Add \"PPv2\" profiles here to set up \"PostProcessVolumes\" for the biomes", MessageType.Info);
            GUILayout.EndVertical();


            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            vegetationStudioManager.PostProcessingLayer = EditorGUILayout.LayerField("Post process layer", vegetationStudioManager.PostProcessingLayer);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                vegetationStudioManager.RefreshPostProcessVolumes();
                EditorUtility.SetDirty(vegetationStudioManager);
                GUILayout.EndVertical();
                return;
            }

            for (int i = 0; i < vegetationStudioManager.PostProcessProfileInfoList.Count; i++)
            {
                GUILayout.BeginVertical("box");

                if (GUILayout.Button("Remove profile", GUILayout.Width(120)))
                {
                    vegetationStudioManager.RemovePostProcessProfile(i);
                    EditorUtility.SetDirty(vegetationStudioManager);
                    GUILayout.EndVertical();
                    return;
                }

                EditorGUI.BeginChangeCheck();
                PostProcessProfileInfo postProcessProfileInfo = vegetationStudioManager.PostProcessProfileInfoList[i];
                postProcessProfileInfo.Enabled = EditorGUILayout.Toggle("Enabled", postProcessProfileInfo.Enabled);
                postProcessProfileInfo.BiomeType = (BiomeType)EditorGUILayout.EnumPopup("Biome Type", postProcessProfileInfo.BiomeType);
                postProcessProfileInfo.VolumeHeight = EditorGUILayout.FloatField("Volume height", postProcessProfileInfo.VolumeHeight);
                postProcessProfileInfo.Priority = EditorGUILayout.FloatField("Priority", postProcessProfileInfo.Priority);
                postProcessProfileInfo.BlendDistance = EditorGUILayout.Slider("Blend distance", postProcessProfileInfo.BlendDistance, -0.1f, 4f);
                postProcessProfileInfo.Weight = EditorGUILayout.Slider("Weight", postProcessProfileInfo.Weight, 0f, 1f);
                postProcessProfileInfo.PostProcessProfile = (PostProcessProfile)EditorGUILayout.ObjectField("Profile", postProcessProfileInfo.PostProcessProfile, typeof(PostProcessProfile), false);
                if (EditorGUI.EndChangeCheck())
                {
                    vegetationStudioManager.RefreshPostProcessVolumes();
                    EditorUtility.SetDirty(vegetationStudioManager);
                    GUILayout.EndVertical();
                    return;
                }

                GUILayout.EndVertical();
            }
#else
            EditorGUILayout.HelpBox("Install the \"PPv2\" package to enable this section", MessageType.Info);
#endif
        }

        private void DrawSettingsInspector()
        {
            EditorGUILayout.HelpBox("The manager internally handles requests of registered systems and their vegetation, terrains, colliders, masks, billboards, etc\nOnly one manager instance should exist per scene", MessageType.Info);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation systems", labelStyle);

            EditorGUI.BeginDisabledGroup(true);
            for (int i = 0; i < vegetationStudioManager.VegetationSystemList.Count; i++)
                EditorGUILayout.ObjectField("Vegetation System Pro", vegetationStudioManager.VegetationSystemList[i], typeof(VegetationSystemPro), true);
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();
        }
    }
}