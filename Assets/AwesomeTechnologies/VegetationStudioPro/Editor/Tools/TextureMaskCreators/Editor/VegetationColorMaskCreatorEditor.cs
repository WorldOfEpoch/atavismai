using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(VegetationColorMaskCreator))]
    public class VegetationColorMaskCreatorEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty eVegetationColorMaskQuality;
        SerializedProperty areaRect;
        SerializedProperty invisibleLayer;
        SerializedProperty includeGrass;
        SerializedProperty includePlants;
        SerializedProperty includeObjects;
        SerializedProperty includeLargeObjects;
        SerializedProperty includeTrees;

        private void OnEnable()
        {
            eVegetationColorMaskQuality = serializedObject.FindProperty("eVegetationColorMaskQuality");
            areaRect = serializedObject.FindProperty("areaRect");
            invisibleLayer = serializedObject.FindProperty("invisibleLayer");
            includeGrass = serializedObject.FindProperty("includeGrass");
            includePlants = serializedObject.FindProperty("includePlants");
            includeObjects = serializedObject.FindProperty("includeObjects");
            includeLargeObjects = serializedObject.FindProperty("includeLargeObjects");
            includeTrees = serializedObject.FindProperty("includeTrees");
        }

        public override void OnInspectorGUI()
        {
            VegetationColorMaskCreator colorMaskCreator = (VegetationColorMaskCreator)target;
            base.OnInspectorGUI();

            VegetationSystemPro vegetationSystemPro = colorMaskCreator.gameObject.GetComponent<VegetationSystemPro>();
            if (vegetationSystemPro == null)
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                return;
            }

            serializedObject.Update();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Generation area", labelStyle);
            EditorGUILayout.PropertyField(areaRect, new GUIContent("Area"));

            GUILayout.BeginHorizontal();
            string[] terrains = new string[vegetationSystemPro.vegetationStudioTerrainObjectList.Count];
            for (int i = 0; i < vegetationSystemPro.vegetationStudioTerrainObjectList.Count; i++)
            {
                if (vegetationSystemPro.vegetationStudioTerrainObjectList[i] == null)
                    continue;

                if (vegetationSystemPro.vegetationStudioTerrainObjectList[i].transform.parent != null)
                    terrains[i] = vegetationSystemPro.vegetationStudioTerrainObjectList[i].transform.parent.name + " - " + vegetationSystemPro.vegetationStudioTerrainObjectList[i].name;
                else
                    terrains[i] = string.Format("{000}", i) + " - " + vegetationSystemPro.vegetationStudioTerrainObjectList[i].name;
            }

            colorMaskCreator.selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", colorMaskCreator.selectedTerrainIndex, terrains);

            if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
                colorMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationStudioTerrainList[colorMaskCreator.selectedTerrainIndex].TerrainBounds);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to world area"))
                colorMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationSystemBounds);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            EditorGUILayout.PropertyField(eVegetationColorMaskQuality, new GUIContent("Mask resolution"));
            EditorGUILayout.PropertyField(invisibleLayer, new GUIContent("Mask render layer"));
            EditorGUILayout.HelpBox("Select an empty unused layer which is used when rendering the mask", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Included vegetation", labelStyle);
            EditorGUILayout.PropertyField(includeGrass, new GUIContent("Include grass"));
            EditorGUILayout.PropertyField(includePlants, new GUIContent("Include plants"));
            EditorGUILayout.PropertyField(includeObjects, new GUIContent("Include objects"));
            EditorGUILayout.PropertyField(includeLargeObjects, new GUIContent("Include large objects"));
            EditorGUILayout.PropertyField(includeTrees, new GUIContent("Include trees"));
            GUILayout.EndVertical();

            if (colorMaskCreator.areaRect.width < 1 || colorMaskCreator.areaRect.height < 1)
                EditorGUILayout.HelpBox("The selected area needs to be at least one pixel on W and H", MessageType.Error);
            else
            {
                if (GUILayout.Button("Generate mask"))
                    GenerateVegetationColorMask(vegetationSystemPro, colorMaskCreator, colorMaskCreator.GetVegetationColorMaskResolution(colorMaskCreator.eVegetationColorMaskQuality));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateVegetationColorMask(VegetationSystemPro _vegetationSystemPro, VegetationColorMaskCreator _colorMaskCreator, int _textureResolution)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save mask", "", "png", "Enter a file name to save the mask to");
            if (path.Length == 0)
                return;

            // overlapped vegetation cells to render the vegetation instances off => that overlap the set "areaRect"
            List<VegetationCell> cellList = new();
            _vegetationSystemPro.vegetationCellQuadTree.Query(_colorMaskCreator.areaRect, cellList);

            // create new textures to render onto / save onto
            RenderTexture rt = new(_textureResolution, _textureResolution, 24, RenderTextureFormat.ARGB32) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, autoGenerateMips = false, hideFlags = HideFlags.HideAndDontSave };
            Texture2D newTexture = new(_textureResolution, _textureResolution, TextureFormat.RGBA32, false, true, true) { hideFlags = HideFlags.HideAndDontSave };

            // create and position the camera's gameObject
            GameObject cameraGO = new() { name = "Color mask camera", hideFlags = HideFlags.HideAndDontSave };
            cameraGO.transform.SetPositionAndRotation(new float3(_colorMaskCreator.areaRect.position.x, 0, _colorMaskCreator.areaRect.position.y) + new float3(_colorMaskCreator.areaRect.size.x * 0.5f, 1000, _colorMaskCreator.areaRect.size.y * 0.5f),
                Quaternion.Euler(90, 0, 0));    // degrees
            SceneViewDetector.GetCurrentSceneViewCamera().transform.SetPositionAndRotation(cameraGO.transform.position, cameraGO.transform.rotation);   // workaround (possible engine bug)

            // create and define a new camera
            Camera utilityCamera = cameraGO.AddComponent<Camera>();
            utilityCamera.backgroundColor = new(0, 0, 0, 0);
            utilityCamera.clearFlags = CameraClearFlags.Color;
            utilityCamera.cullingMask = 1 << _colorMaskCreator.invisibleLayer;
            utilityCamera.farClipPlane = 10000;
            utilityCamera.orthographic = true;
            utilityCamera.orthographicSize = _colorMaskCreator.areaRect.size.x * 0.5f;
            utilityCamera.targetTexture = rt;

            EditorUtility.DisplayProgressBar("Create color mask", "Render vegetation", 0);
            GL.PushMatrix();
            Graphics.SetRenderTarget(rt);
            GL.Viewport(new Rect(0, 0, rt.width, rt.height));
            GL.Clear(true, true, new Color(0, 0, 0, 0), 1f);
            GL.modelview = utilityCamera.worldToCameraMatrix;
            GL.LoadProjectionMatrix(utilityCamera.projectionMatrix);
            RenderVegetation(_vegetationSystemPro, _colorMaskCreator, cellList, ShaderUtility.GetShader_UtilityVegetationColorMask());  // render vegetation instances into renderTexture
            GL.PopMatrix();

            // texture creation => post processing => saving/importing
            newTexture.ReadPixels(new Rect(0, 0, _textureResolution, _textureResolution), 0, 0);    // read pixels from the global "renderTexture"
            newTexture.Apply(); // apply read pixels onto the texture

            EditorUtility.DisplayProgressBar("Create color mask", "Import texture", 0.75f);
            File.WriteAllBytes(Application.dataPath + path.Replace("Assets", ""), newTexture.EncodeToPNG());    // save texture to the given path as a "PNG"
            TextureExtension.ImportTexture(path, 2, _textureResolution);    // manually import texture into the project

            RenderTexture.active = null;    // reset global "renderTexture"
            DestroyImmediate(cameraGO); // first
            DestroyImmediate(newTexture);
            DestroyImmediate(rt);

            EditorUtility.ClearProgressBar();
        }

        private void RenderVegetation(VegetationSystemPro _vegetationSystemPro, VegetationColorMaskCreator _colorMaskCreator, List<VegetationCell> _cellList, Shader _colorMaskShader)
        {
            for (int i = 0; i < _vegetationSystemPro.vegetationPackageProList.Count; i++)
                for (int j = 0; j < _vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                {
                    VegetationItemInfoPro vegItemInfoPro = _vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j];
                    VegetationItemModelInfo vegItemModelInfo = _vegetationSystemPro.GetVegetationItemModelInfo(vegItemInfoPro.VegetationItemID);
                    if (vegItemModelInfo == null) continue;
                    if (_colorMaskCreator.includeGrass == false && vegItemInfoPro.VegetationType == VegetationType.Grass) continue;
                    if (_colorMaskCreator.includePlants == false && vegItemInfoPro.VegetationType == VegetationType.Plant) continue;
                    if (_colorMaskCreator.includeObjects == false && vegItemInfoPro.VegetationType == VegetationType.Objects) continue;
                    if (_colorMaskCreator.includeLargeObjects == false && vegItemInfoPro.VegetationType == VegetationType.LargeObjects) continue;
                    if (_colorMaskCreator.includeTrees == false && vegItemInfoPro.VegetationType == VegetationType.Tree) continue;

                    for (int k = 0; k < _cellList.Count; k++)
                    {
                        if (j % 10 == 0)
                            EditorUtility.DisplayProgressBar("Render vegetation item: " + vegItemInfoPro.Name, "Render cell " + k + "/" + (_cellList.Count - 1), k / ((float)_cellList.Count - 1));

                        _vegetationSystemPro.SpawnVegetationCellEx(_cellList[k], vegItemInfoPro.VegetationItemID);  // spawn vegetation instances
                        NativeList<MatrixInstance> vegetationInstanceList = _vegetationSystemPro.GetVegetationItemInstances(_cellList[k], vegItemInfoPro.VegetationItemID); // get spawned vegetation instances

                        for (int l = 0; l < vegItemModelInfo.vegetationMeshLod0.subMeshCount; l++)
                        {
                            Material tempMaterial = new(vegItemModelInfo.vegetationMaterialsLOD0[l]);   // the effective material of the vegetation instances
                            tempMaterial.shader = _colorMaskShader;

                            if (tempMaterial.HasProperty("_Color"))
                                if (vegItemInfoPro.useShaderControllerOverrides)    // whether the item model info has been created -- it uses shaderController overrides -- it has a valid shaderController setup
                                {
                                    bool hasTintColor = false;
                                    for (int m = 0; m < vegItemModelInfo.shaderControllers?.Length; m++)
                                        if (vegItemModelInfo.shaderControllers[m] != null && vegItemModelInfo.shaderControllers[m].Settings != null)
                                            if (hasTintColor = vegItemModelInfo.shaderControllers[m].Settings.HasPropertyValue("TintColor1"))
                                                tempMaterial.SetColor("_Color", vegItemModelInfo.shaderControllers[m].Settings.GetColorPropertyValue("TintColor1"));

                                    if (hasTintColor == false)
                                        tempMaterial.SetColor("_Color", ShaderUtility.GetColor_Base(vegItemModelInfo.vegetationMaterialsLOD0[l]));  // get the color of the effective material for the vegetation instances
                                }
                                else
                                    tempMaterial.SetColor("_Color", ShaderUtility.GetColor_Base(vegItemModelInfo.vegetationMaterialsLOD0[l]));  // get the color of the effective material for the vegetation instances

                            tempMaterial.SetPass(0);

                            for (int m = 0; m < vegetationInstanceList.Length; m++)
                            {
                                if (vegetationInstanceList[m].controlData.x <= 0)
                                    continue;   // skip masked out persistent vegetation storage vegetation instances

                                Graphics.DrawMeshNow(vegItemModelInfo.vegetationMeshLod0, vegetationInstanceList[m].matrix, l);
                            }

                            DestroyImmediate(tempMaterial);
                        }

                        _cellList[k].ClearCache();
                    }

                    EditorUtility.ClearProgressBar();
                }
        }
    }
}