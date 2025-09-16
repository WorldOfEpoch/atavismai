using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationSystem;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(BackgroundMaskCreator))]
    public class BackgroundMaskCreatorEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty eBackgroundMaskQuality;
        SerializedProperty areaRect;

        private void OnEnable()
        {
            eBackgroundMaskQuality = serializedObject.FindProperty("eBackgroundMaskQuality");
            areaRect = serializedObject.FindProperty("areaRect");
        }

        public override void OnInspectorGUI()
        {
            BackgroundMaskCreator backgroundMaskCreator = (BackgroundMaskCreator)target;
            base.OnInspectorGUI();

            VegetationSystemPro vegetationSystemPro = backgroundMaskCreator.gameObject.GetComponent<VegetationSystemPro>();
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

            backgroundMaskCreator.selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", backgroundMaskCreator.selectedTerrainIndex, terrains);

            if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
                backgroundMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationStudioTerrainList[backgroundMaskCreator.selectedTerrainIndex].TerrainBounds);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to world area"))
                backgroundMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationSystemBounds);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            EditorGUILayout.PropertyField(eBackgroundMaskQuality, new GUIContent("Mask resolution"));
            GUILayout.EndVertical();

            if (backgroundMaskCreator.areaRect.width < 1 || backgroundMaskCreator.areaRect.height < 1)
                EditorGUILayout.HelpBox("The selected area needs to be at least one pixel on W and H", MessageType.Error);
            else
            {
                if (GUILayout.Button("Generate mask"))
                    GenerateBackgroundMask(backgroundMaskCreator.areaRect, backgroundMaskCreator.GetBackgroundMaskResolution(backgroundMaskCreator.eBackgroundMaskQuality));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateBackgroundMask(Rect _area, int _textureResolution)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save mask", "", "png", "Enter a file name to save the mask to");
            if (path.Length == 0)
                return;

            // render parameters
            float ratio = _area.width / _area.height;
            int textureWidth = _textureResolution;
            int textureHeight = (int)math.round(_textureResolution / ratio);

            // create new textures to render onto / save onto
            RenderTexture rt = new(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, autoGenerateMips = false, hideFlags = HideFlags.HideAndDontSave };
            Texture2D newTexture = new(textureWidth, textureHeight, TextureFormat.RGBA32, false, true, true) { hideFlags = HideFlags.HideAndDontSave };

            // create and position the camera's gameObject
            GameObject cameraGO = new() { name = "Background mask camera", hideFlags = HideFlags.HideAndDontSave };
            cameraGO.transform.SetPositionAndRotation(new float3(_area.center.x, 0, _area.center.y) + new float3(0, 1000, 0), Quaternion.Euler(90, 0, 0));  // degrees

            // create and define a new camera
            Camera utilityCamera = cameraGO.AddComponent<Camera>();
            utilityCamera.farClipPlane = 10000;
            utilityCamera.orthographic = true;
            utilityCamera.orthographicSize = _area.size.x * 0.5f / ratio;
            utilityCamera.targetTexture = rt;
            utilityCamera.Render(); // render into the render texture

            Graphics.SetRenderTarget(rt);   // assign to the global render texture to read from it
            newTexture.ReadPixels(new(0, 0, textureWidth, textureHeight), 0, 0);    // read pixels from the global "renderTexture"
            newTexture.Apply(); // apply read pixels onto the texture

            File.WriteAllBytes(Application.dataPath + path.Replace("Assets", ""), newTexture.EncodeToPNG());    // save texture to the given path as a "PNG"
            TextureExtension.ImportTexture(path, 2, _textureResolution);    // manually import texture into the project

            RenderTexture.active = null;    // reset global "renderTexture"
            DestroyImmediate(cameraGO); // first
            DestroyImmediate(newTexture);
            DestroyImmediate(rt);
        }
    }
}