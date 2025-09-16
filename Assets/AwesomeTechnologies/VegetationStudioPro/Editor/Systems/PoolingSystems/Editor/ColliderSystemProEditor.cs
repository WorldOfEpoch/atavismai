using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.ColliderSystem
{
    [CustomEditor(typeof(ColliderSystemPro))]
    public class ColliderSystemProEditor : VegetationStudioProBaseEditor
    {
        private ColliderSystemPro colliderSystemPro;
        private readonly string[] TabNames = { "Info", "Baking", "Debug" };

        public override void OnInspectorGUI()
        {
            colliderSystemPro = (ColliderSystemPro)target;
            base.OnInspectorGUI();

            if (colliderSystemPro.vegetationSystemPro == null)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                GUILayout.EndVertical();
                return;
            }

            if (colliderSystemPro.enabled == false)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Component is disabled\nEnable it to spawn (pooled) colliders", MessageType.Warning);
                GUILayout.EndVertical();
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");
            colliderSystemPro.currentTabIndex = GUILayout.SelectionGrid(colliderSystemPro.currentTabIndex, TabNames, 3, EditorStyles.toolbarButton);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            switch (colliderSystemPro.currentTabIndex)
            {
                case 0:
                    DrawInfoInspector();
                    break;
                case 1:
                    DrawNavmeshInspector();
                    break;
                case 2:
                    DrawDebugInspector();
                    break;
            }
        }

        private void DrawInfoInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("When enabled the collider system creates colliders for vegetation instances around cameras\nCollider settings need to be configured for each vegetation item", MessageType.Info);
            EditorGUILayout.HelpBox("By default the collider system uses object pooling for better performance within the set distances on each vegetation item\n" +
                "The colliders can also be baked for other uses like\n- creating a NavMesh\n- server sided checks in a multiplayer environment\n- light baking with mesh primitives", MessageType.Info);
            GUILayout.EndVertical();
        }

        private void DrawNavmeshInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Baking info", labelStyle);
            EditorGUILayout.HelpBox("Bake the colliders of configured vegetation items converted as GameObjects\nAfter creating the NavMesh/LightMap/Other the GameObjects can be removed", MessageType.Info);

            EditorGUILayout.HelpBox("This system can/should be turned off when keeping baked colliders but needs to be re-enabled to bake the/new colliders", MessageType.Warning);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Baking toggles", labelStyle);
            EditorGUILayout.HelpBox("Enable both toggles when baking colliders for a NavMesh/LightMap\nMore settings can be set per vegetation item in the \"Colliders\" section", MessageType.Info);
            colliderSystemPro.setBakedCollidersStatic = EditorGUILayout.Toggle("Set static", colliderSystemPro.setBakedCollidersStatic);
            colliderSystemPro.convertBakedCollidersToMesh = EditorGUILayout.Toggle("Convert to primitive meshes", colliderSystemPro.convertBakedCollidersToMesh);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Exclude toggles", labelStyle);
            colliderSystemPro.excludeTrees = EditorGUILayout.Toggle("Exclude trees", colliderSystemPro.excludeTrees);
            colliderSystemPro.excludeObjects = EditorGUILayout.Toggle("Exclude objects", colliderSystemPro.excludeObjects);
            colliderSystemPro.excludeLargeObjects = EditorGUILayout.Toggle("Exclude large objects", colliderSystemPro.excludeLargeObjects);
            if (GUILayout.Button("Bake colliders to the hierarchy"))
                if (EditorUtility.DisplayDialog("Bake colliders", "Selected action: Bake colliders\nEnsure the scene has been saved before baking" +
                    "\n\nHigh density colliders can take hours to bake/convert into gameObjects even on high end setups", "Confirm", "Cancel"))
                    colliderSystemPro.BakeCollidersToScene();
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            EditorGUILayout.HelpBox("Baking high densities of colliders for permanent use is not recommended\n-> The engine can't handle too many gameObjects => colliders", MessageType.Warning);
        }

        private void DrawDebugInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Debug info", labelStyle);

            EditorGUI.BeginChangeCheck();
            colliderSystemPro.showDebugCells = EditorGUILayout.Toggle("Show affected cells", colliderSystemPro.showDebugCells);
            EditorGUILayout.HelpBox("Show the affected vegetation cells in the scene", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            EditorGUI.BeginChangeCheck();
            colliderSystemPro.showColliders = EditorGUILayout.Toggle("Show colliders", colliderSystemPro.showColliders);
            EditorGUILayout.HelpBox("Show the run-time spawned colliders in the hierarchy", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                colliderSystemPro.SetColliderVisibility(colliderSystemPro.showColliders);
                EditorApplication.RepaintHierarchyWindow();
                SetSceneDirty();
            }

            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime info", labelStyle);

            if (colliderSystemPro.visibleVegetationCellSelector != null)
            {
                if (colliderSystemPro.vegetationSystemPro.vegetationSettings.grassDistance <= 0)
                    EditorGUILayout.HelpBox("Colliders are disabled from spawning since the grass distance is set to zero", MessageType.Warning);

                EditorGUILayout.LabelField("Visible cells: " + colliderSystemPro.visibleVegetationCellSelector.visibleSelectorVegetationCellList.Count.ToString());
                EditorGUILayout.LabelField("Loaded instances: " + colliderSystemPro.GetLoadedInstanceCount());
                EditorGUILayout.LabelField("Visible instances: " + colliderSystemPro.GetVisibleColliders());
            }
            else
            {
                EditorGUILayout.HelpBox("Collider run-time info only shows while the scene is running", MessageType.Info);
            }
            GUILayout.EndVertical();
        }

        private void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(colliderSystemPro);
        }
    }
}