using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.TerrainSystem
{
    [CustomEditor(typeof(TerrainSystemPro))]
    public class TerrainSystemProEditor : VegetationStudioProBaseEditor
    {
        private TerrainSystemPro terrainSystemPro;
        private InspectorCurveEditor heightCurveEditor;
        private InspectorCurveEditor steepnessCurveEditor;
        private readonly string[] tabNames = { "Info/Settings", "Edit biome splat map", "Edit terrain textures" };

        public void OnEnable()
        {
            InspectorCurveEditor.Settings settings = InspectorCurveEditor.Settings.DefaultSettings;
            heightCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Height };
            steepnessCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Steepness };
        }

        public void OnDisable()
        {
            heightCurveEditor.RemoveAll();
            steepnessCurveEditor.RemoveAll();

            terrainSystemPro = (TerrainSystemPro)target;
            if (terrainSystemPro)
                terrainSystemPro.EnableTerrainHeatmap(false);
        }

        public override void OnInspectorGUI()
        {
            terrainSystemPro = (TerrainSystemPro)target;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = 200;

            terrainSystemPro.vegetationSystemPro = terrainSystemPro.gameObject.GetComponent<VegetationSystemPro>();
            if (terrainSystemPro.vegetationSystemPro == null)
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                return;
            }

            if (terrainSystemPro.vegetationPackageIndex >= terrainSystemPro.vegetationSystemPro.vegetationPackageProList.Count)
                terrainSystemPro.vegetationPackageIndex = 0;

            GUILayout.BeginVertical("box");
            terrainSystemPro.currentTabIndex = GUILayout.SelectionGrid(terrainSystemPro.currentTabIndex, tabNames, 3, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            switch (terrainSystemPro.currentTabIndex)
            {
                case 0:
                    DrawInfoInspector();
                    break;
                case 1:
                    DrawEditBiomeSplatmapInspector();
                    break;
                case 2:
                    DrawEditTerrainTexturesInspector();
                    break;
            }

            if (terrainSystemPro.currentTabIndex != 1)
                terrainSystemPro.EnableTerrainHeatmap(false);
            else
                terrainSystemPro.EnableTerrainHeatmap(true);    // perma write if enabled further internally -- workaround since engine resets "temporary material data" on saving
        }

        private void DrawInfoInspector()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Terrain system pro", labelStyle);
            EditorGUILayout.HelpBox("This component helps with generating splat maps for terrains\nParts of the splat map can be split to each \"Biome mask\" to generate based on their specific area\n" +
                "Set up rules for each terrain texture, per biome, on how they should be generated", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Terrain textures", labelStyle);
            EditorGUILayout.HelpBox("All vegetation packages/biomes that are used within the same scene/vegetation system should use the same terrain textures in the same order\n" +
                "The terrain textures can then be defined in more detail using the rules and toggled on/off per biome", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Biome mask editing", labelStyle);
            EditorGUILayout.HelpBox("Refresh shortcuts can be found in \"Window/AwesomeTechnologies/Shortcuts\"", MessageType.Info);
            terrainSystemPro.enableAutoSplatMapGeneration = EditorGUILayout.Toggle("Post edit splat map regeneration", terrainSystemPro.enableAutoSplatMapGeneration);
            EditorGUILayout.HelpBox("Automatically regenerate the splat map after a node of a biome mask got moved", MessageType.Info);
            GUILayout.EndVertical();
        }

        private void DrawEditBiomeSplatmapInspector()
        {
            VegetationSystemPro vegetationSystemPro = terrainSystemPro.vegetationSystemPro;

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

            terrainSystemPro.vegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", terrainSystemPro.vegetationPackageIndex, packageNameList);
            if (terrainSystemPro.vegetationPackageIndex > vegetationSystemPro.vegetationPackageProList.Count - 1)
                terrainSystemPro.vegetationPackageIndex = vegetationSystemPro.vegetationPackageProList.Count - 1;

            VegetationPackagePro vegetationPackagePro = terrainSystemPro.vegetationSystemPro.vegetationPackageProList[terrainSystemPro.vegetationPackageIndex];
            if (vegetationPackagePro == null)
            {
                GUILayout.EndVertical();
                return;
            }

            if (vegetationPackagePro.TerrainTextureCount <= 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("The selected vegetation package/biome has no texture slots assigned\nIncrease the size by selecting the vegetation package in the \"Project\" window", MessageType.Warning);
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            vegetationPackagePro.GenerateBiomeSplatmap = EditorGUILayout.Toggle("Include biome in splat map", vegetationPackagePro.GenerateBiomeSplatmap);
            if (EditorGUI.EndChangeCheck())
                SetSceneDirty();

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Generate splat map"))
            {
                terrainSystemPro.GenerateSplatMap(terrainSystemPro.vegetationSystemPro.vegetationSystemBounds, false);
                terrainSystemPro.EnableTerrainHeatmap(false);
                SetSceneDirty();
            }

            if (GUILayout.Button("Generate splat map -- clear existing textures"))
            {
                if (EditorUtility.DisplayDialog("Clear existing textures", "Warning: \"Clear existing textures\" has been pressed!\n\nThis clears and regenerates the splat map including all existing textures!", "Confirm", "Cancel"))
                {
                    terrainSystemPro.GenerateSplatMap(terrainSystemPro.vegetationSystemPro.vegetationSystemBounds, true);
                    terrainSystemPro.EnableTerrainHeatmap(false);
                    SetSceneDirty();
                }
            }

            EditorGUILayout.Space();

            if (terrainSystemPro.vegetationPackageTextureIndex > vegetationPackagePro.TerrainTextureCount)
                terrainSystemPro.vegetationPackageTextureIndex = 0;

            EditorGUI.BeginChangeCheck();
            GUIContent[] textureImageButtons = new GUIContent[vegetationPackagePro.TerrainTextureSettingsList.Count];
            for (int i = 0; i < vegetationPackagePro.TerrainTextureSettingsList.Count; i++)
                textureImageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(vegetationPackagePro.TerrainTextureList[i].Texture) };

            int imageWidth = 70;
            int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
            int rows = (int)math.ceil((float)textureImageButtons.Length / columns);
            int gridHeight = (rows) * imageWidth;
            if (columns > 0)
                terrainSystemPro.vegetationPackageTextureIndex = GUILayout.SelectionGrid(terrainSystemPro.vegetationPackageTextureIndex, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

            if (terrainSystemPro.lastVegetationPackageTextureIndex != terrainSystemPro.vegetationPackageTextureIndex)
            {
                GUI.FocusControl(null);
                heightCurveEditor.selectedCurve = null;
                heightCurveEditor.selectedKeyframeIndex = -1;
                steepnessCurveEditor.selectedCurve = null;
                steepnessCurveEditor.selectedKeyframeIndex = -1;
            }
            terrainSystemPro.lastVegetationPackageTextureIndex = terrainSystemPro.vegetationPackageTextureIndex;

            if (EditorGUI.EndChangeCheck())
                terrainSystemPro.UpdateTerrainHeatmap();

            TerrainTextureSettings terrainTextureSettings = vegetationPackagePro.TerrainTextureSettingsList[terrainSystemPro.vegetationPackageTextureIndex];

            EditorGUI.BeginChangeCheck();
            terrainTextureSettings.Enabled = EditorGUILayout.Toggle("Include texture in splat map", terrainTextureSettings.Enabled);

            EditorGUI.BeginDisabledGroup(terrainTextureSettings.Enabled);
            terrainTextureSettings.LockTexture = EditorGUILayout.Toggle("Copy existing data", terrainTextureSettings.LockTexture);
            EditorGUILayout.HelpBox("Copy existing data to keep a previous generation / hand painted textures\nThe default biome needs to enable this first before per biome copying is possible", MessageType.Info);
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            terrainSystemPro.enableHeatmap = EditorGUILayout.Toggle("Enable heat map", terrainSystemPro.enableHeatmap);
            EditorGUILayout.HelpBox("Preview the local density per texture -- White = more / Gray = less\nActual results depend on the combinated blend of all biome mask + texture rules", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                terrainSystemPro.EnableTerrainHeatmap(terrainSystemPro.enableHeatmap);

            GUILayout.EndVertical();

            if (terrainSystemPro.showCurvesMenu = VegetationPackageEditorTools.DrawHeader("Height/Steepness", terrainSystemPro.showCurvesMenu))
            {
                GUILayout.BeginVertical("box");
                terrainTextureSettings.densityFactor = EditorGUILayout.Slider("Texture density factor", terrainTextureSettings.densityFactor, 0.1f, 10);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Texture " + (terrainSystemPro.vegetationPackageTextureIndex + 1).ToString() + " Height", labelStyle, GUILayout.Width(150));
                heightCurveEditor.SeaLevel = vegetationSystemPro.SeaLevel;
                heightCurveEditor.MaxValue = vegetationSystemPro.vegetationSystemBounds.max.y - vegetationSystemPro.SeaLevel;
                if (heightCurveEditor.EditCurve(terrainTextureSettings.TextureHeightCurve, this))
                {
                    //terrainSystemPro.UpdateTerrainHeatmap();
                    //EditorUtility.SetDirty(vegetationPackagePro);
                }

                Keyframe selectedKeyHeight = heightCurveEditor.GetSelection().Keyframe ?? new();
                if (heightCurveEditor.GetSelection().Keyframe != null)
                {
                    int index = heightCurveEditor.GetSelection().KeyframeIndex;
                    float time = math.round(EditorGUILayout.Slider("Height", selectedKeyHeight.time * heightCurveEditor.MaxValue, 0, heightCurveEditor.MaxValue) * 100) / 100;
                    float value = math.round(EditorGUILayout.Slider("Density", selectedKeyHeight.value, 0, 1) * 100) / 100;
                    float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeyHeight.inTangent, -5, 5) * 100) / 100;
                    float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeyHeight.outTangent, -5, 5) * 100) / 100;
                    if (index > 0)
                        time = math.max((terrainTextureSettings.TextureHeightCurve.keys[index - 1].time + 0.0001f) * heightCurveEditor.MaxValue, time); // safety "clamp" else keys delete themselves when having the same value
                    if (index < terrainTextureSettings.TextureHeightCurve.keys.Length - 1)
                        time = math.min((terrainTextureSettings.TextureHeightCurve.keys[index + 1].time - 0.0001f) * heightCurveEditor.MaxValue, time); // safety "clamp" else keys delete themselves when having the same value
                    terrainTextureSettings.TextureHeightCurve.MoveKey(index, new Keyframe(time / heightCurveEditor.MaxValue, value, inTangent, outTangent));
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Texture " + (terrainSystemPro.vegetationPackageTextureIndex + 1).ToString() + " Steepness", labelStyle, GUILayout.Width(150));
                if (steepnessCurveEditor.EditCurve(terrainTextureSettings.TextureSteepnessCurve, this))
                {
                    //terrainSystemPro.UpdateTerrainHeatmap();
                    //EditorUtility.SetDirty(vegetationPackagePro);
                }

                Keyframe selectedKeySteepness = steepnessCurveEditor.GetSelection().Keyframe ?? new();
                if (steepnessCurveEditor.GetSelection().Keyframe != null)
                {
                    int index = steepnessCurveEditor.GetSelection().KeyframeIndex;
                    float time = math.round(EditorGUILayout.Slider("Steepness", selectedKeySteepness.time * 90, 0, 90) * 100) / 100;
                    float value = math.round(EditorGUILayout.Slider("Density", selectedKeySteepness.value, 0, 1) * 100) / 100;
                    float inTangent = math.round(EditorGUILayout.Slider("InTangent", selectedKeySteepness.inTangent, -5, 5) * 100) / 100;
                    float outTangent = math.round(EditorGUILayout.Slider("OutTangent", selectedKeySteepness.outTangent, -5, 5) * 100) / 100;
                    if (index > 0)
                        time = math.max((terrainTextureSettings.TextureSteepnessCurve.keys[index - 1].time + 0.0001f) * 90, time);  // safety "clamp" else keys delete themselves when having the same value
                    if (index < terrainTextureSettings.TextureSteepnessCurve.keys.Length - 1)
                        time = math.min((terrainTextureSettings.TextureSteepnessCurve.keys[index + 1].time - 0.0001f) * 90, time);  // safety "clamp" else keys delete themselves when having the same value
                    terrainTextureSettings.TextureSteepnessCurve.MoveKey(index, new Keyframe(time / 90, value, inTangent, outTangent));
                }
                GUILayout.EndVertical();
            };

            if (terrainSystemPro.showNoiseMenu = VegetationPackageEditorTools.DrawHeader("Noise", terrainSystemPro.showNoiseMenu))
            {
                GUILayout.BeginVertical("box");
                if (terrainTextureSettings.UseNoise = EditorGUILayout.Toggle("Use perlin noise", terrainTextureSettings.UseNoise))
                {
                    terrainTextureSettings.InverseNoise = EditorGUILayout.Toggle("Invert noise", terrainTextureSettings.InverseNoise);
                    terrainTextureSettings.NoiseScale = EditorGUILayout.Slider("Noise scale", terrainTextureSettings.NoiseScale, 1, 500);
                    terrainTextureSettings.noiseBalancing = EditorGUILayout.Slider("Noise balancing", terrainTextureSettings.noiseBalancing, -1, 1);
                    terrainTextureSettings.NoiseOffset = EditorGUILayout.Vector2Field("Noise offset", terrainTextureSettings.NoiseOffset);
                }
                GUILayout.EndVertical();
            }

            if (terrainSystemPro.showConcaveConvexMenu = VegetationPackageEditorTools.DrawHeader("Concave/Convex overrides", terrainSystemPro.showConcaveConvexMenu))
            {
                GUILayout.BeginVertical("box");
                EditorGUI.BeginDisabledGroup(terrainTextureSettings.ConcaveEnable == false && terrainTextureSettings.ConvexEnable == false);
                terrainTextureSettings.DistancePerSample = EditorGUILayout.Slider("Distance per sample", terrainTextureSettings.DistancePerSample, 0.1f, 20);
                terrainTextureSettings.applyCurves = EditorGUILayout.Toggle("Apply curves", terrainTextureSettings.applyCurves);
                EditorGUI.BeginDisabledGroup(terrainTextureSettings.applyCurves || terrainTextureSettings.UseNoise == false);
                terrainTextureSettings.applyNoise = EditorGUILayout.Toggle("Apply noise", terrainTextureSettings.applyNoise);
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
                if (terrainTextureSettings.ConcaveEnable = EditorGUILayout.Toggle("Use concave override", terrainTextureSettings.ConcaveEnable))
                {
                    terrainTextureSettings.concaveDensityFactor = EditorGUILayout.Slider("Texture density factor", terrainTextureSettings.concaveDensityFactor, 0.1f, 10);
                    terrainTextureSettings.ConcaveMinHeightDifference = EditorGUILayout.Slider("Height difference smoothing", terrainTextureSettings.ConcaveMinHeightDifference, 0.1f, 20);
                    terrainTextureSettings.ConcaveMinHeight = EditorGUILayout.Slider("Min height", terrainTextureSettings.ConcaveMinHeight, 0, 1000);
                    terrainTextureSettings.ConcaveMaxHeight = EditorGUILayout.Slider("Max height", terrainTextureSettings.ConcaveMaxHeight, 0, 1000);
                    terrainTextureSettings.concaveMinSteepness = EditorGUILayout.Slider("Min steepness", terrainTextureSettings.concaveMinSteepness, 0, 90);
                    terrainTextureSettings.concaveMaxSteepness = EditorGUILayout.Slider("Max steepness", terrainTextureSettings.concaveMaxSteepness, 0, 90);
                }

                EditorGUILayout.Space();
                if (terrainTextureSettings.ConvexEnable = EditorGUILayout.Toggle("Use convex override", terrainTextureSettings.ConvexEnable))
                {
                    terrainTextureSettings.convexDensityFactor = EditorGUILayout.Slider("Texture density factor", terrainTextureSettings.convexDensityFactor, 0.1f, 10);
                    terrainTextureSettings.ConvexMinHeightDifference = EditorGUILayout.Slider("Height difference smoothing", terrainTextureSettings.ConvexMinHeightDifference, 0.1f, 20);
                    terrainTextureSettings.ConvexMinHeight = EditorGUILayout.Slider("Min height", terrainTextureSettings.ConvexMinHeight, 0, 1000);
                    terrainTextureSettings.ConvexMaxHeight = EditorGUILayout.Slider("Max height", terrainTextureSettings.ConvexMaxHeight, 0, 1000);
                    terrainTextureSettings.convexMinSteepness = EditorGUILayout.Slider("Min steepness", terrainTextureSettings.convexMinSteepness, 0, 90);
                    terrainTextureSettings.convexMaxSteepness = EditorGUILayout.Slider("Max steepness", terrainTextureSettings.convexMaxSteepness, 0, 90);
                }
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                terrainSystemPro.UpdateTerrainHeatmap();
                EditorUtility.SetDirty(vegetationPackagePro);
                SetSceneDirty();
            }
        }

        private void DrawEditTerrainTexturesInspector()
        {
            GUILayout.BeginVertical("box");

            if (terrainSystemPro.vegetationSystemPro.vegetationPackageProList.Count == 0)
            {
                EditorGUILayout.HelpBox("No vegetation package available", MessageType.Warning);
                GUILayout.EndVertical();
                return;
            }

            string[] packageNameList = new string[terrainSystemPro.vegetationSystemPro.vegetationPackageProList.Count];
            for (int i = 0; i < terrainSystemPro.vegetationSystemPro.vegetationPackageProList.Count; i++)
                if (terrainSystemPro.vegetationSystemPro.vegetationPackageProList[i])
                    packageNameList[i] = (i + 1).ToString() + " " + terrainSystemPro.vegetationSystemPro.vegetationPackageProList[i].PackageName + " (" + terrainSystemPro.vegetationSystemPro.vegetationPackageProList[i].BiomeType.ToString() + ")";
                else
                    packageNameList[i] = "Not found";

            terrainSystemPro.vegetationPackageIndex = EditorGUILayout.Popup("Selected vegetation package", terrainSystemPro.vegetationPackageIndex, packageNameList);
            if (terrainSystemPro.vegetationPackageIndex > terrainSystemPro.vegetationSystemPro.vegetationPackageProList.Count - 1)
                terrainSystemPro.vegetationPackageIndex = terrainSystemPro.vegetationSystemPro.vegetationPackageProList.Count - 1;

            VegetationPackagePro vegetationPackagePro = terrainSystemPro.vegetationSystemPro.vegetationPackageProList[terrainSystemPro.vegetationPackageIndex];
            if (vegetationPackagePro == null)
            {
                GUILayout.EndVertical();
                return;
            }

            if (vegetationPackagePro.TerrainTextureCount <= 0)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("The selected vegetation package/biome has no texture slots assigned\nIncrease the size by selecting the vegetation package in the \"Project\" window", MessageType.Warning);
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                return;
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Read textures from first terrain"))
            {
                if (EditorUtility.DisplayDialog("Read terrain textures", "Selected vegetation package/biome: " + vegetationPackagePro.PackageName + "\nSelected operation: Read" +
                    "\n\nWarning: This operation replaces all terrain textures of the selected vegetation package/biome", "Confirm", "Cancel"))
                {
                    terrainSystemPro.GetSplatPrototypes(vegetationPackagePro);
                }
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            if (GUILayout.Button("Write textures to all terrains"))
                if (EditorUtility.DisplayDialog("Write terrain textures", "Selected vegetation package/biome: " + vegetationPackagePro.PackageName + "\nSelected operation: Write" +
                    "\n\nWarning: This operation replaces all terrain textures of all added terrains ", "Confirm", "Cancel"))
                {
                    terrainSystemPro.SetSplatPrototypes(vegetationPackagePro);
                }

            EditorGUILayout.Space();

            if (terrainSystemPro.vegetationPackageTextureIndex > vegetationPackagePro.TerrainTextureCount)
                terrainSystemPro.vegetationPackageTextureIndex = 0;

            GUIContent[] textureImageButtons = new GUIContent[vegetationPackagePro.TerrainTextureSettingsList.Count];
            for (int i = 0; i < vegetationPackagePro.TerrainTextureSettingsList.Count; i++)
                textureImageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(vegetationPackagePro.TerrainTextureList[i].Texture) };

            int imageWidth = 70;
            int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
            int rows = (int)math.ceil((float)textureImageButtons.Length / columns);
            int gridHeight = (rows) * imageWidth;

            if (columns > 0)
                terrainSystemPro.vegetationPackageTextureIndex = GUILayout.SelectionGrid(terrainSystemPro.vegetationPackageTextureIndex, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

            EditorGUILayout.Space();

            TerrainTextureInfo terrainTextureInfo = vegetationPackagePro.TerrainTextureList[terrainSystemPro.vegetationPackageTextureIndex];

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Main", ""), labelStyle, GUILayout.Width(64));
            EditorGUILayout.LabelField(new GUIContent("Normal", ""), labelStyle, GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            terrainTextureInfo.Texture = (Texture2D)EditorGUILayout.ObjectField(terrainTextureInfo.Texture, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
            terrainTextureInfo.TextureNormals = (Texture2D)EditorGUILayout.ObjectField(terrainTextureInfo.TextureNormals, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();

            Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18));
            terrainTextureInfo.TileSize = EditorGUI.Vector2Field(space, "Texture tile size", terrainTextureInfo.TileSize);
            if (terrainTextureInfo.TileSize.x < 1 || terrainTextureInfo.TileSize.y < 1)
                EditorGUILayout.HelpBox("The texture tile size should be set to a higher value, like ~1-4, depending on the used terrain shader", MessageType.Warning);
            if (EditorGUI.EndChangeCheck())
            {
                AssetUtility.SetTextureReadable(terrainTextureInfo.Texture, false);
                AssetUtility.SetTextureReadable(terrainTextureInfo.TextureNormals, true);
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            GUILayout.EndVertical();
        }

        void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(terrainSystemPro);
        }
    }
}