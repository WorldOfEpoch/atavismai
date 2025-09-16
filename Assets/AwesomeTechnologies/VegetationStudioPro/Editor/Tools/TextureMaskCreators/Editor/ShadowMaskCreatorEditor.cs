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
    [CustomEditor(typeof(ShadowMaskCreator))]
    public class ShadowMaskCreatorEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty eShadowMaskQuality;
        SerializedProperty areaRect;

        SerializedProperty invisibleLayer;
        SerializedProperty outputIntensity;

        SerializedProperty includeTrees;
        SerializedProperty includeLargeObjects;

        private void OnEnable()
        {
            eShadowMaskQuality = serializedObject.FindProperty("eShadowMaskQuality");
            areaRect = serializedObject.FindProperty("areaRect");

            invisibleLayer = serializedObject.FindProperty("invisibleLayer");
            outputIntensity = serializedObject.FindProperty("outputIntensity");

            includeTrees = serializedObject.FindProperty("includeTrees");
            includeLargeObjects = serializedObject.FindProperty("includeLargeObjects");
        }

        public override void OnInspectorGUI()
        {
            ShadowMaskCreator shadowMaskCreator = (ShadowMaskCreator)target;
            base.OnInspectorGUI();

            VegetationSystemPro vegetationSystemPro = shadowMaskCreator.gameObject.GetComponent<VegetationSystemPro>();
            if (vegetationSystemPro == null)
            {
                EditorGUILayout.HelpBox("Add this component to a GameObject with a VegetationSystemPro component" +
                    "\n\nConsider simply re-adding it in case of the engine having lost the internal reference\nEx: When updating versions, clearing the \"Library\" folder", MessageType.Error);
                return;
            }

            serializedObject.Update();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Generation area", labelStyle);
            EditorGUILayout.HelpBox("Only \"Unity Terrains\" are supported, ensure them being the lowest part of your vegetation system's \"total area\"", MessageType.Warning);
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

            shadowMaskCreator.selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", shadowMaskCreator.selectedTerrainIndex, terrains);

            if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
                shadowMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationStudioTerrainList[shadowMaskCreator.selectedTerrainIndex].TerrainBounds);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to world area"))
                shadowMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationSystemBounds);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            EditorGUILayout.PropertyField(eShadowMaskQuality, new GUIContent("Mask resolution"));
            EditorGUILayout.PropertyField(invisibleLayer, new GUIContent("Mask render layer"));
            EditorGUILayout.HelpBox("Select an empty unused layer which is used when rendering the mask", MessageType.Info);

            EditorGUILayout.PropertyField(outputIntensity, new GUIContent("Output intensity"));
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Included vegetation", labelStyle);
            EditorGUILayout.PropertyField(includeTrees, new GUIContent("Include trees"));
            EditorGUILayout.PropertyField(includeLargeObjects, new GUIContent("Include large objects"));
            GUILayout.EndVertical();

            if (shadowMaskCreator.areaRect.width < 1 || shadowMaskCreator.areaRect.height < 1)
                EditorGUILayout.HelpBox("The selected area needs to be at least one pixel on W and H", MessageType.Error);
            else
            {
                if (GUILayout.Button("Generate mask"))
                    GenerateShadowMask(vegetationSystemPro, shadowMaskCreator, shadowMaskCreator.GetShadowMaskResolution(shadowMaskCreator.eShadowMaskQuality));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateShadowMask(VegetationSystemPro _vegetationSystemPro, ShadowMaskCreator _shadowMaskCreator, int _textureResolution)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save mask", "", "png", "Enter a file name to save the mask to");
            if (path.Length == 0)
                return;

            // overlapped vegetation cells to render the vegetation instances off => that overlap the set "areaRect"
            List<VegetationCell> cellList = new();
            _vegetationSystemPro.vegetationCellQuadTree.Query(_shadowMaskCreator.areaRect, cellList);

            // create new textures to render onto / save onto
            RenderTexture rt = new(_textureResolution, _textureResolution, 24, RenderTextureFormat.RFloat) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, autoGenerateMips = false, hideFlags = HideFlags.HideAndDontSave };
            Texture2D newTexture = new(_textureResolution, _textureResolution, TextureFormat.RGBA32, false, true, true) { hideFlags = HideFlags.HideAndDontSave };

            // create and position the camera's gameObject
            GameObject cameraGO = new() { name = "Shadow mask camera", hideFlags = HideFlags.HideAndDontSave };
            cameraGO.transform.SetPositionAndRotation(new float3(_shadowMaskCreator.areaRect.x, 0, _shadowMaskCreator.areaRect.y) + new float3(_shadowMaskCreator.areaRect.size.x * 0.5f, 1000, _shadowMaskCreator.areaRect.size.y * 0.5f),
                Quaternion.Euler(90, 0, 0));    // degrees
            SceneViewDetector.GetCurrentSceneViewCamera().transform.SetPositionAndRotation(cameraGO.transform.position, cameraGO.transform.rotation);   // workaround (possible engine bug)

            // create and define a new camera
            Camera utilityCamera = cameraGO.AddComponent<Camera>();
            utilityCamera.backgroundColor = Color.black;
            utilityCamera.clearFlags = CameraClearFlags.Color;
            utilityCamera.cullingMask = 1 << _shadowMaskCreator.invisibleLayer;
            utilityCamera.farClipPlane = 10000;
            utilityCamera.orthographic = true;
            utilityCamera.orthographicSize = _shadowMaskCreator.areaRect.size.x * 0.5f;
            utilityCamera.targetTexture = rt;

            EditorUtility.DisplayProgressBar("Create shadow mask", "Render vegetation", 0);
            GL.PushMatrix();
            Graphics.SetRenderTarget(rt);
            GL.Viewport(new Rect(0, 0, rt.width, rt.height));
            GL.Clear(true, true, new Color(0, 0, 0, 0), 1f);
            GL.modelview = utilityCamera.worldToCameraMatrix;
            GL.LoadProjectionMatrix(utilityCamera.projectionMatrix);
            RenderVegetation(_vegetationSystemPro, _shadowMaskCreator, cellList, ShaderUtility.GetShader_UtilityVertexHeight()); // render vegetation instances into renderTexture
            GL.PopMatrix();

            // texture creation => post processing => saving/importing
            EditorUtility.DisplayProgressBar("Create shadow mask", "Compute heights", 0.25f);
            float[] outputHeights = new float[_textureResolution * _textureResolution];
            GraphicsBuffer outputHeightBuffer = new(GraphicsBuffer.Target.Append, _textureResolution * _textureResolution, 4);
            ComputeShader decodeShader = ShaderUtility.GetComputeShader_UtilityVertexHeight_Decode();
            int decodeKernelHandle = decodeShader.FindKernel("UtilityVertexHeight_Decode");
            decodeShader.SetInt("textureResolution", _textureResolution);
            decodeShader.SetTexture(decodeKernelHandle, "textureDown", rt);
            decodeShader.SetBuffer(decodeKernelHandle, "outputHeightBuffer", outputHeightBuffer);
            decodeShader.Dispatch(decodeKernelHandle, _textureResolution / 8, _textureResolution / 8, 1);
            outputHeightBuffer.GetData(outputHeights);
            outputHeightBuffer.Dispose();

            EditorUtility.DisplayProgressBar("Create shadow mask", "Evaluate height output", 0.5f);
            Color32[] outputColors = new Color32[outputHeights.Length];
            float minTerrainHeight = _vegetationSystemPro.vegetationSystemBounds.min.y; // effectively center - extents -- don't calculate more often than needed

            for (int x = 0; x < _textureResolution; x++)
                for (int y = 0; y < _textureResolution; y++)
                {
                    int i = x + (y * _textureResolution);
                    float relativeHeightDown = math.clamp(((outputHeights[i] - minTerrainHeight) - SampleTerrainHeight((float)x / _textureResolution, (float)y / _textureResolution, _shadowMaskCreator.areaRect)) * _shadowMaskCreator.outputIntensity, 0, 255);

                    outputColors[i].a = 255;
                    outputColors[i].r = (byte)relativeHeightDown;
                    outputColors[i].g = 0;
                    outputColors[i].b = 0;
                }

            newTexture.SetPixels32(outputColors);   // read pixels from the global "renderTexture" => set pixels from the evaluated height "Color32" data
            newTexture.Apply(); // apply read pixels onto the texture
            // --

            EditorUtility.DisplayProgressBar("Create shadow mask", "Import texture", 0.75f);
            File.WriteAllBytes(Application.dataPath + path.Replace("Assets", ""), newTexture.EncodeToPNG());    // save texture to the given path as a "PNG"
            TextureExtension.ImportTexture(path, 2, _textureResolution);    // manually import texture into the project

            RenderTexture.active = null;    // reset global "renderTexture"
            DestroyImmediate(cameraGO); // first
            DestroyImmediate(newTexture);
            DestroyImmediate(rt);

            EditorUtility.ClearProgressBar();
        }

        private void RenderVegetation(VegetationSystemPro _vegetationSystemPro, ShadowMaskCreator _shadowMaskCreator, List<VegetationCell> _cellList, Shader _shadowMaskShader)
        {
            for (int i = 0; i < _vegetationSystemPro.vegetationPackageProList.Count; i++)
                for (int j = 0; j < _vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList.Count; j++)
                {
                    VegetationItemInfoPro vegItemInfoPro = _vegetationSystemPro.vegetationPackageProList[i].VegetationInfoList[j];
                    VegetationItemModelInfo vegItemModelInfo = _vegetationSystemPro.GetVegetationItemModelInfo(vegItemInfoPro.VegetationItemID);
                    if (vegItemModelInfo == null) continue;
                    if (vegItemInfoPro.VegetationType == VegetationType.Grass) continue;
                    if (vegItemInfoPro.VegetationType == VegetationType.Plant) continue;
                    if (vegItemInfoPro.VegetationType == VegetationType.Objects) continue;
                    if (_shadowMaskCreator.includeLargeObjects == false && vegItemInfoPro.VegetationType == VegetationType.LargeObjects) continue;
                    if (_shadowMaskCreator.includeTrees == false && vegItemInfoPro.VegetationType == VegetationType.Tree) continue;

                    for (int k = 0; k < _cellList.Count; k++)
                    {
                        if (j % 10 == 0)
                            EditorUtility.DisplayProgressBar("Render vegetation item: " + vegItemInfoPro.Name, "Render cell " + k + "/" + (_cellList.Count - 1), k / ((float)_cellList.Count - 1));

                        _vegetationSystemPro.SpawnVegetationCellEx(_cellList[k], vegItemInfoPro.VegetationItemID);  // spawn vegetation instances
                        NativeList<MatrixInstance> vegetationInstanceList = _vegetationSystemPro.GetVegetationItemInstances(_cellList[k], vegItemInfoPro.VegetationItemID); // get spawned vegetation instances

                        for (int l = 0; l < vegItemModelInfo.vegetationMeshLod0.subMeshCount; l++)
                        {
                            vegItemModelInfo.vegetationMaterialsLOD0[l].SetFloat(_shadowMaskCreator.cullFarStart, 50000);   // safety set for certain shaders -- left here for compatibility
                            vegItemModelInfo.vegetationMaterialsLOD0[l].SetFloat(_shadowMaskCreator.cullFarDistance, 20);   // safety set for certain shaders -- left here for compatibility
                            Material tempMaterial = new(vegItemModelInfo.vegetationMaterialsLOD0[l]);
                            tempMaterial.shader = _shadowMaskShader;
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

        private float SampleTerrainHeight(float _xNormalized, float _yNormalized, Rect _area)
        {
            float3 position = new float3(_area.position.x, 10000, _area.position.y) + new float3(_xNormalized * _area.size.x, 0, _yNormalized * _area.size.y);
            RaycastHit[] hits = Physics.RaycastAll(new(position, Vector3.down), 13337);
            for (int i = 0; i < hits.Length; i++)
                if (hits[i].collider is TerrainCollider)
                    return hits[i].point.y;
            return 0;
        }
    }
}