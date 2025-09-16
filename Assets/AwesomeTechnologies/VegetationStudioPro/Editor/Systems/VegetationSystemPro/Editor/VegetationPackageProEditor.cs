using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [CustomEditor(typeof(VegetationPackagePro))]
    public class VegetationPackageProEditor : VegetationStudioProBaseEditor
    {
        private VegetationPackagePro vegetationPackagePro;
        private string selectedVegetationItem = "";
        private int selectedTexture;
        private int newTextureCountIndex;
        private static readonly string[] textureCountNames = { "No textures", "4 textures", "8 textures", "12 textures", "16 textures", "32 textures" };

        [MenuItem("Window/Awesome Technologies/Create data packages/VegetationPackagePro/No Texture Biome")]
        public static void CreateYourScriptableObject()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 0;
        }

        [MenuItem("Window/Awesome Technologies/Create data packages/VegetationPackagePro/4 terrain texture Biome")]
        public static void CreateVegetationPackageObject4Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 4;
            vegetationPackagePro.LoadDefaultTextures();
        }

        [MenuItem("Window/Awesome Technologies/Create data packages/VegetationPackagePro/8 terrain texture Biome")]
        public static void CreateVegetationPackageObject8Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 8;
            vegetationPackagePro.LoadDefaultTextures();
        }

        [MenuItem("Window/Awesome Technologies/Create data packages/VegetationPackagePro/12 terrain texture Biome")]
        public static void CreateVegetationPackageObject12Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 12;
            vegetationPackagePro.LoadDefaultTextures();
        }

        [MenuItem("Window/Awesome Technologies/Create data packages/VegetationPackagePro/16 terrain texture Biome")]
        public static void CreateVegetationPackageObject16Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 16;
            vegetationPackagePro.LoadDefaultTextures();
        }

        [MenuItem("Window/Awesome Technologies/Create data packages/VegetationPackagePro/32 terrain texture Biome")]
        public static void CreateVegetationPackageObject32Textures()
        {
            VegetationPackagePro vegetationPackagePro = ScriptableObjectUtility.CreateAndReturnAsset<VegetationPackagePro>();
            vegetationPackagePro.TerrainTextureCount = 32;
            vegetationPackagePro.LoadDefaultTextures();
        }

        public void OnEnable()
        {
            vegetationPackagePro = (VegetationPackagePro)target;
            newTextureCountIndex = GetTerrainTextureIndex(vegetationPackagePro);
        }

        public override void OnInspectorGUI()
        {
            vegetationPackagePro = (VegetationPackagePro)target;
            base.OnInspectorGUI();

            for (int i = 0; i < vegetationPackagePro.VegetationInfoList.Count; i++)
            {
                bool changed = false;
                if (vegetationPackagePro.VegetationInfoList[i].VegetationItemID == "")
                {
                    vegetationPackagePro.VegetationInfoList[i].VegetationItemID = System.Guid.NewGuid().ToString();
                    changed = true;
                }

                if (changed)
                    EditorUtility.SetDirty(vegetationPackagePro);
            }

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Edit the content of a vegetation package by adding it to a \"VegetationSystemPro\" as a biome", MessageType.Warning);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Biome", labelStyle);
            EditorGUI.BeginChangeCheck();
            vegetationPackagePro.BiomeType = (BiomeType)EditorGUILayout.EnumPopup("Biome type", vegetationPackagePro.BiomeType);
            if (EditorGUI.EndChangeCheck())
            {
                VegetationStudioManager.ClearCache();
                EditorUtility.SetDirty(vegetationPackagePro);
            }

            if (vegetationPackagePro.VegetationInfoList.Count > 0)
            {
                List<string> vegetationItemIdList = VegetationPackageEditorTools.CreateVegetationInfoIdList(vegetationPackagePro);
                VegetationPackageEditorTools.DrawVegetationItemSelector(vegetationPackagePro, vegetationItemIdList, 64, ref selectedVegetationItem);
                EditorGUILayout.LabelField("VegetationItemID", labelStyle);
                EditorGUILayout.TextField(vegetationPackagePro.GetVegetationInfo(selectedVegetationItem).VegetationItemID);
            }
            else
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("This vegetation package has no vegetation items", MessageType.Info);
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Regenerate vegetation item IDs"))
            {
                vegetationPackagePro.RegenerateVegetationItemIDs();
                EditorUtility.SetDirty(vegetationPackagePro);
            }
            EditorGUILayout.HelpBox("Regenerate IDs whenever duplicating a vegetation package to avoid issues", MessageType.Warning);
            GUILayout.EndVertical();

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Terrain textures", labelStyle);

            GUILayout.BeginVertical("box");
            newTextureCountIndex = EditorGUILayout.Popup("Number of terrain textures", newTextureCountIndex, textureCountNames);
            if (newTextureCountIndex > 2)
                EditorGUILayout.HelpBox("The default SRP terrain shaders have a texture amount limit, per terrain\nURP = 4, HDRP = 8" +
                    "\nUse custom terrain shaders like \"GTS, TVE-Terrain or MicroSplat\"\nAnd/Or split your terrain/-s and use multiple ones", MessageType.Warning);

            if (GUILayout.Button("Change the number of textures for the biome"))
                ChangeTerrainTextureCount(vegetationPackagePro, GetTerrainTextureCountFromIndex(newTextureCountIndex));
            GUILayout.EndVertical();

            if (vegetationPackagePro.TerrainTextureCount > 0)
            {
                GUIContent[] textureImageButtons = new GUIContent[vegetationPackagePro.TerrainTextureList.Count];
                for (int i = 0; i < vegetationPackagePro.TerrainTextureList.Count; i++)
                {
                    if (vegetationPackagePro.TerrainTextureList[i] == null)
                        continue;

                    Texture2D textureItemTexture = AssetPreview.GetAssetPreview(vegetationPackagePro.TerrainTextureList[i].Texture);
                    Texture2D convertedTexture = new(2, 2, TextureFormat.RGBA32, false, false);
                    if (textureItemTexture)
                        convertedTexture.LoadImage(textureItemTexture.EncodeToPNG());

                    textureImageButtons[i] = new GUIContent { image = convertedTexture };
                }

                int imageWidth = 60;
                int columns = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - imageWidth / 2f) / imageWidth);
                if (columns > 0)
                {
                    int rows = Mathf.CeilToInt((float)textureImageButtons.Length / columns);
                    int gridHeight = (rows) * imageWidth;
                    selectedTexture = GUILayout.SelectionGrid(selectedTexture, textureImageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));
                }

                GUIStyle variantStyle = new(EditorStyles.helpBox);

                EditorGUI.BeginDisabledGroup(true);
                vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].Enabled = EditorGUILayout.Toggle("Include texture in splat map", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].Enabled);
                vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].LockTexture = EditorGUILayout.Toggle("Copy existing data", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].LockTexture);

                vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].densityFactor = EditorGUILayout.Slider("Texture density factor", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].densityFactor, 0.1f, 10);

                EditorGUILayout.BeginHorizontal(variantStyle);
                EditorGUILayout.LabelField("Texture " + (selectedTexture + 1).ToString() + " Height", labelStyle, GUILayout.Width(150));
                vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].TextureHeightCurve = EditorGUILayout.CurveField(vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].TextureHeightCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(variantStyle);
                EditorGUILayout.LabelField("Texture " + (selectedTexture + 1).ToString() + " Steepness", labelStyle, GUILayout.Width(150));
                vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].TextureSteepnessCurve = EditorGUILayout.CurveField(vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].TextureSteepnessCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Height(75));
                EditorGUILayout.EndHorizontal();

                if (vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].UseNoise = EditorGUILayout.Toggle("Use perlin noise", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].UseNoise))
                {
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].InverseNoise = EditorGUILayout.Toggle("Invert noise", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].InverseNoise);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].NoiseScale = EditorGUILayout.Slider("Noise scale", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].NoiseScale, 1, 500);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].noiseBalancing = EditorGUILayout.Slider("Noise balancing", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].noiseBalancing, -1, 1);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].NoiseOffset = EditorGUILayout.Vector2Field("Noise offset", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].NoiseOffset);
                }

                if (vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveEnable || vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexEnable)
                {
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].DistancePerSample = EditorGUILayout.Slider("Distance per sample", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].DistancePerSample, 0.1f, 20);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].applyCurves = EditorGUILayout.Toggle("Apply curves", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].applyCurves);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].applyNoise = EditorGUILayout.Toggle("Apply noise", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].applyNoise);
                }

                if (vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveEnable = EditorGUILayout.Toggle("Use concave override", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveEnable))
                {
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].concaveDensityFactor = EditorGUILayout.Slider("Texture density factor", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].concaveDensityFactor, 0.1f, 10);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveMinHeightDifference = EditorGUILayout.Slider("Height difference smoothing", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveMinHeightDifference, 0.1f, 20);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveMinHeight = EditorGUILayout.Slider("Min height above sea level", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveMinHeight, 0, 1000);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveMaxHeight = EditorGUILayout.Slider("Max height above sea level", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConcaveMaxHeight, 0, 1000);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].concaveMinSteepness = EditorGUILayout.Slider("Min steepness", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].concaveMinSteepness, 0, 90);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].concaveMaxSteepness = EditorGUILayout.Slider("Max steepness", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].concaveMaxSteepness, 0, 90);
                }

                if (vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexEnable = EditorGUILayout.Toggle("Use convex override", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexEnable))
                {
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].convexDensityFactor = EditorGUILayout.Slider("Texture density factor", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].convexDensityFactor, 0.1f, 10);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexMinHeightDifference = EditorGUILayout.Slider("Height difference smoothing", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexMinHeightDifference, 0.1f, 20);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexMinHeight = EditorGUILayout.Slider("Min height above sea level", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexMinHeight, 0, 1000);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexMaxHeight = EditorGUILayout.Slider("Max height above sea level", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].ConvexMaxHeight, 0, 1000);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].convexMinSteepness = EditorGUILayout.Slider("Min steepness", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].convexMinSteepness, 0, 90);
                    vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].convexMaxSteepness = EditorGUILayout.Slider("Max steepness", vegetationPackagePro.TerrainTextureSettingsList[selectedTexture].convexMaxSteepness, 0, 90);
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("This vegetation package has no textures", MessageType.Info);
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
        }

        private int GetTerrainTextureIndex(VegetationPackagePro _vegetationPackage)
        {
            return _vegetationPackage.TerrainTextureCount switch
            {
                0 => 0,
                4 => 1,
                8 => 2,
                12 => 3,
                16 => 4,
                32 => 5,
                _ => 0,
            };
        }

        private int GetTerrainTextureCountFromIndex(int _index)
        {
            return _index switch
            {
                0 => 0,
                1 => 4,
                2 => 8,
                3 => 12,
                4 => 16,
                5 => 32,
                _ => 0,
            };
        }

        private void ChangeTerrainTextureCount(VegetationPackagePro _vegetationPackagePro, int _newCount)
        {
            if (_vegetationPackagePro.TerrainTextureCount == _newCount)
                return;

            _vegetationPackagePro.TerrainTextureCount = _newCount;

            if (_newCount > _vegetationPackagePro.TerrainTextureCount)
                _vegetationPackagePro.LoadDefaultTextures();
            else
            {
                _vegetationPackagePro.ResizeTerrainTextureList(_newCount);
                _vegetationPackagePro.ResizeTerrainTextureSettingsList(_newCount);
            }
        }
    }
}