using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.VegetationSystem;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [CustomEditor(typeof(ObstacleMaskCreator))]
    public class ObstacleMaskCreatorEditor : VegetationStudioProBaseEditor
    {
        SerializedProperty eObstacleMaskQuality;
        SerializedProperty areaRect;

        SerializedProperty disableOverdraw;

        SerializedProperty firstLayer;
        SerializedProperty allowTerrainCollider_first;
        SerializedProperty minRadius_first;

        SerializedProperty secondLayer;
        SerializedProperty allowTerrainCollider_second;
        SerializedProperty minRadius_second;

        SerializedProperty thirdLayer;
        SerializedProperty allowTerrainCollider_third;
        SerializedProperty minRadius_third;

        private void OnEnable()
        {
            eObstacleMaskQuality = serializedObject.FindProperty("eObstacleMaskQuality");
            areaRect = serializedObject.FindProperty("areaRect");

            disableOverdraw = serializedObject.FindProperty("disableOverdraw");

            firstLayer = serializedObject.FindProperty("firstLayer");
            allowTerrainCollider_first = serializedObject.FindProperty("allowTerrainCollider_first");
            minRadius_first = serializedObject.FindProperty("minRadius_first");

            secondLayer = serializedObject.FindProperty("secondLayer");
            allowTerrainCollider_second = serializedObject.FindProperty("allowTerrainCollider_second");
            minRadius_second = serializedObject.FindProperty("minRadius_second");

            thirdLayer = serializedObject.FindProperty("thirdLayer");
            allowTerrainCollider_third = serializedObject.FindProperty("allowTerrainCollider_third");
            minRadius_third = serializedObject.FindProperty("minRadius_third");
        }

        public override void OnInspectorGUI()
        {
            ObstacleMaskCreator obstacleMaskCreator = (ObstacleMaskCreator)target;
            base.OnInspectorGUI();

            VegetationSystemPro vegetationSystemPro = obstacleMaskCreator.gameObject.GetComponent<VegetationSystemPro>();
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

            obstacleMaskCreator.selectedTerrainIndex = EditorGUILayout.Popup("Select terrain", obstacleMaskCreator.selectedTerrainIndex, terrains);

            if (GUILayout.Button("Snap to terrain", GUILayout.Width(120)))
                obstacleMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationStudioTerrainList[obstacleMaskCreator.selectedTerrainIndex].TerrainBounds);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to world area"))
                obstacleMaskCreator.areaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.vegetationSystemBounds);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", labelStyle);
            EditorGUILayout.PropertyField(eObstacleMaskQuality, new GUIContent("Mask resolution"));

            EditorGUILayout.PropertyField(disableOverdraw, new GUIContent("Disable overdraw"));
            EditorGUILayout.HelpBox("Disable overdraw to improve performance, each pixel gets only written to once\nMind that the draw order gets affected", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Included obstacles", labelStyle);
            EditorGUILayout.HelpBox("Draw order is R > G > B -- Latter overwrites former\nUse the terrain collider toggle to block rays but not write terrain data", MessageType.Info);

            EditorGUILayout.PropertyField(firstLayer, new GUIContent("Obstacle layer R"));
            EditorGUILayout.PropertyField(allowTerrainCollider_first, new GUIContent("Allow terrain collider"));
            EditorGUILayout.PropertyField(minRadius_first, new GUIContent("Minimum radius"));

            EditorGUILayout.PropertyField(secondLayer, new GUIContent("Obstacle layer G"));
            EditorGUILayout.PropertyField(allowTerrainCollider_second, new GUIContent("Allow terrain collider"));
            EditorGUILayout.PropertyField(minRadius_second, new GUIContent("Minimum radius"));

            EditorGUILayout.PropertyField(thirdLayer, new GUIContent("Obstacle layer B"));
            EditorGUILayout.PropertyField(allowTerrainCollider_third, new GUIContent("Allow terrain collider"));
            EditorGUILayout.PropertyField(minRadius_third, new GUIContent("Minimum radius"));
            GUILayout.EndVertical();

            if (obstacleMaskCreator.areaRect.width < 1 || obstacleMaskCreator.areaRect.height < 1)
                EditorGUILayout.HelpBox("The selected area needs to be at least one pixel on W and H", MessageType.Error);
            else
            {
                if (GUILayout.Button("Generate mask"))
                    GenerateObstacleMask(obstacleMaskCreator, obstacleMaskCreator.GetObstacleMaskResolution(obstacleMaskCreator.eObstacleMaskQuality));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateObstacleMask(ObstacleMaskCreator _obstacleMaskCreator, int _textureResolution)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save mask", "", "png", "Enter the file name to save the mask to");
            if (path.Length == 0)
                return;

            float pixelSize = _obstacleMaskCreator.areaRect.size.x / _textureResolution;    // size to scan, of a pixel, based on the selected resolution quality
            Vector3 maskCorner = new(_obstacleMaskCreator.areaRect.position.x, 0, _obstacleMaskCreator.areaRect.position.y); // position-offset to scan from, if set at all
            Texture2D obstacleMaskTexture = new(_textureResolution, _textureResolution, TextureFormat.RGBA32, false, true, true) { hideFlags = HideFlags.HideAndDontSave }; // texture that gets written to -- output texture

            for (int x = 0; x < _textureResolution; x++)    // for each X pixel based on the resolution
            {
                if (x % 100 == 0)
                    EditorUtility.DisplayProgressBar("Generate mask ", "Raycast for obstacles", (float)x / _textureResolution);

                for (int z = 0; z < _textureResolution; z++)    // for each Z pixel based on the resolution
                {
                    RaycastHit hit;
                    Ray ray = new(new Vector3(x * pixelSize, 10000, z * pixelSize) + maskCorner, Vector3.down); // create a new ray for each pixel specifically w/ an offset if set

                    /// skip scanning if the layer is zero -- no layer has been enabled for the layer mask
                    /// scan each pixel by its exact pixel size w/ an additional minimum radius offset to define more detailed rules
                    /// -> the "areaRect" to "_textureResolution" ratio determines the total area to scan ..thus more/less ex: "terrains"
                    /// color the pixel when all criteria meet -- R G B used to have the ability to define more detailed rules
                    /// skip overdrawing/re-drawing pixels if set so to save performance
                    /// color the rest black to exclude the pixel from being used in a rule -- to get perfect exclusions
                    /// 

                    if (_obstacleMaskCreator.disableOverdraw == false)  // when not disabling overdraw => fill entire texture with black first => then override each pixel afterwards
                        obstacleMaskTexture.SetPixel(x, z, Color.black);    // fill rest with black to fully exclude from any rule

                    if (_obstacleMaskCreator.firstLayer.value != 0)
                        if (Physics.SphereCast(ray, (pixelSize * 0.5f) + _obstacleMaskCreator.minRadius_first, out hit, float.MaxValue, _obstacleMaskCreator.firstLayer))
                        {
                            if (_obstacleMaskCreator.allowTerrainCollider_first == false && (hit.collider is TerrainCollider))
                            {
                            }
                            else
                            {
                                obstacleMaskTexture.SetPixel(x, z, Color.red);
                                if (_obstacleMaskCreator.disableOverdraw) continue;
                            }
                        }

                    if (_obstacleMaskCreator.secondLayer.value != 0)
                        if (Physics.SphereCast(ray, (pixelSize * 0.5f) + _obstacleMaskCreator.minRadius_second, out hit, float.MaxValue, _obstacleMaskCreator.secondLayer))
                        {
                            if (_obstacleMaskCreator.allowTerrainCollider_second == false && (hit.collider is TerrainCollider))
                            {
                            }
                            else
                            {
                                obstacleMaskTexture.SetPixel(x, z, Color.green);
                                if (_obstacleMaskCreator.disableOverdraw) continue;
                            }
                        }

                    if (_obstacleMaskCreator.thirdLayer.value != 0)
                        if (Physics.SphereCast(ray, (pixelSize * 0.5f) + _obstacleMaskCreator.minRadius_third, out hit, float.MaxValue, _obstacleMaskCreator.thirdLayer))
                        {
                            if (_obstacleMaskCreator.allowTerrainCollider_third == false && (hit.collider is TerrainCollider))
                            {
                            }
                            else
                            {
                                obstacleMaskTexture.SetPixel(x, z, Color.blue);
                                if (_obstacleMaskCreator.disableOverdraw) continue;
                            }
                        }

                    if (_obstacleMaskCreator.disableOverdraw)   // when overdraw is disabled only paint black the rest that didn't get drawn to yet
                        obstacleMaskTexture.SetPixel(x, z, Color.black);    // fill rest with black to fully exclude from any rule
                }
            }

            obstacleMaskTexture.Apply();    // apply read pixels onto the texture

            File.WriteAllBytes(Application.dataPath + path.Replace("Assets", ""), obstacleMaskTexture.EncodeToPNG());   // save texture to the given path as a "PNG"
            TextureExtension.ImportTexture(path, 2, _textureResolution);    // manually import texture into the project

            EditorUtility.ClearProgressBar();
        }
    }
}