using AwesomeTechnologies.Shaders;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/GrassPatchGenerator")]
    public class GrassPatchGenerator : MonoBehaviour
    {
        public bool showGenerationSection;
        public bool showMaterialSection;
        public bool showVertexColorSection;

        private List<ProceduralGrassPlane> planeList;
        private Material objectMaterial;
        private Material vertexColorMaterial;
        public bool showVertexColors = false;

        public int randomSeed = 69;
        public int planeCount = 16;
        public float planeGap = 0.33f;
        public float planeWidth = 0.33f;
        public float planeHeight = 0.33f;
        public float minScale = 0.8f;
        public float maxScale = 1.2f;

        public int planeWidthSegments = 2;
        public int planeHeightSegments = 2;

        public Texture2D mainTexture;
        public float textureCutoff = 0.33f;
        public Color healthyColor = Color.white;
        public bool toggleHueVariation = true;
        public Color dryColor = Color.white;
        public Texture2D dryColorNoiseTexture;
        public float dryColorNoiseScale = 100;
        public bool toggleDarkening = true;
        public float randomDarkening = 0.5f;
        public float rootAmbient = 0.5f;

        public float minBendHeight = 0;
        public float maxBendDistance = 0.25f;
        public float curveOffset = 0.25f;

        public bool bakePhase = true;
        public bool bakeBend = true;
        public bool bakeAo = true;
        public AnimationCurve aoCurve = new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });
        public AnimationCurve bendCurve = new(new Keyframe[] { new(0, 0, 0f, 0f), new(1, 1, 0f, 0f) });

        private void Reset()
        {
            if (mainTexture == null)   // don't reset already custom assigned textures
                mainTexture = Resources.Load<Texture2D>("ShaderTextures/Grass/GrassFrond01");

            if (dryColorNoiseTexture == null)   // don't reset already custom assigned textures
                dryColorNoiseTexture = Resources.Load<Texture2D>("ShaderTextures/HDWind/GustNoise");

            if (vertexColorMaterial == null)
                vertexColorMaterial = new(ShaderUtility.GetShader_UtilityVertexColor());

            UpdateGrassPatchGenerator();
        }

        public void UpdateGrassPatchGenerator()
        {
            if (objectMaterial == null) objectMaterial = new(ShaderUtility.GetShader_Foliage());
            objectMaterial.shader = ShaderUtility.GetShader_Foliage();
            objectMaterial.enableInstancing = true;
            objectMaterial.SetTexture("_MainTex", mainTexture);
            objectMaterial.SetFloat("_AlphaClipping", textureCutoff);
            objectMaterial.SetColor("_HealthyColor", healthyColor);
            objectMaterial.SetFloat("HUE_VARIATION", toggleHueVariation ? 1 : 0);
            objectMaterial.SetColor("_DryColor", dryColor);
            objectMaterial.SetTexture("_DryColorNoiseTex", dryColorNoiseTexture);
            objectMaterial.SetFloat("_DryColorNoiseScale", dryColorNoiseScale);
            objectMaterial.SetFloat("RANDOM_DARKENING", toggleDarkening ? 1 : 0);
            objectMaterial.SetFloat("_RandomDarkening", randomDarkening);
            objectMaterial.SetFloat("_RootAmbient", rootAmbient);
            GenerateGrassPatch();
        }

        public void ClearGrassPlanes()
        {
            if (planeList == null) // safety check
                planeList = new List<ProceduralGrassPlane>();  // new list for showing stats of tris/vert to the user
            else
                planeList.Clear(); // clear existing entries from previous operations

            foreach (Transform _transform in transform.GetComponentsInChildren<Transform>())
                if (_transform && _transform.gameObject.name.StartsWith("Plane_"))
                    DestroyImmediate(_transform.gameObject);    // destroy any planes left from previous operations -- safety delete
        }

        public void GenerateGrassPatch()
        {
            ClearGrassPlanes();
            Random.InitState(randomSeed);

            for (int i = 0; i < planeCount; i++)
            {
                float scale = Random.Range(minScale, maxScale);
                float selectedPlaneWidth = planeWidth * scale;
                float selectedPlaneHeight = planeHeight * scale;

                // create a gameObject for previewing the planes -- not editable for users to not mess with values -- viewable in the hierarchy so they are deletable for certain scenarios
                GameObject go = new() { hideFlags = HideFlags.DontSave | HideFlags.NotEditable, name = "Plane_" + i.ToString() };
                go.transform.SetParent(transform);
                go.transform.localRotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360f), 0)); // degrees
                go.transform.localPosition = new Vector3(Random.Range(-planeGap * 0.5f, planeGap * 0.5f), selectedPlaneHeight * scale * 0.5f, Random.Range(-planeGap * 0.5f, planeGap * 0.5f));

                // prepare all parameters for previewing the planes => merging them into a mesh later
                ProceduralGrassPlane proceduralGrassPlane = go.AddComponent<ProceduralGrassPlane>();
                proceduralGrassPlane.index = i;
                if (i % 4 == 1) proceduralGrassPlane.lodLevel = 2;
                else if (i % 2 == 1) proceduralGrassPlane.lodLevel = 1;
                else proceduralGrassPlane.lodLevel = 0;
                proceduralGrassPlane.material = showVertexColors ? vertexColorMaterial : objectMaterial;

                proceduralGrassPlane.width = selectedPlaneWidth;
                proceduralGrassPlane.height = selectedPlaneHeight;
                proceduralGrassPlane.widthSegments = planeWidthSegments;
                proceduralGrassPlane.heightSegments = planeHeightSegments;

                proceduralGrassPlane.minimumBendHeight = minBendHeight;
                proceduralGrassPlane.bendDistanceOffset_first = Random.Range(-maxBendDistance, maxBendDistance);
                proceduralGrassPlane.bendDistanceOffset_second = Random.Range(-maxBendDistance, maxBendDistance);
                proceduralGrassPlane.curveOffset = Random.Range(-curveOffset, curveOffset);

                proceduralGrassPlane.bakeBend = bakeBend;
                proceduralGrassPlane.bakePhase = bakePhase;
                proceduralGrassPlane.bakeAO = bakeAo;
                proceduralGrassPlane.phase = i * (1f / planeCount);
                proceduralGrassPlane.aoCurve = aoCurve;
                proceduralGrassPlane.bendCurve = bendCurve;

                planeList.Add(proceduralGrassPlane);   // add to the list for showing stats of tris/vert to the user
                proceduralGrassPlane.CreateGrassPlane();    // actually generate the grass planes for previewing them => before merging them into one single mesh w/ LODs if chosen
            }
        }

        public void ExportPrefab(bool _createLODs)
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanelInProject("Save prefab", "", "prefab", "Please enter a file path/name to save the prefab to");
            if (path.Length == 0)
                return;

            // set paths and names to save to
            string prefabName = Path.GetFileNameWithoutExtension(path);
            string directory = Path.GetDirectoryName(path);

            // store old pos/rot for the preview -- set new pos to zero to save the pivot point to world zero
            Vector3 oldPosition = transform.position;
            Quaternion oldRotation = transform.rotation;
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            // create GO/Prefab -- Mesh -- Material -- LODs => save them to the project as actual files
            GameObject go = new() { name = prefabName };
            Material temp = new(objectMaterial);
            AssetDatabase.CreateAsset(temp, Path.ChangeExtension(path, ".mat"));

            Mesh meshLod0 = GetCombinedMesh(0);
            AssetDatabase.CreateAsset(meshLod0, directory + "/" + prefabName + "_LOD0.asset");
            if (_createLODs == false)
            {
                go.AddComponent<MeshFilter>().sharedMesh = meshLod0;
                go.AddComponent<MeshRenderer>().sharedMaterial = temp;
            }
            else
            {
                GameObject goLod0 = new() { name = prefabName + "_LOD0" };
                goLod0.transform.SetParent(go.transform, false);
                goLod0.AddComponent<MeshFilter>().sharedMesh = meshLod0;
                goLod0.AddComponent<MeshRenderer>().sharedMaterial = temp;

                Mesh meshLod1 = GetCombinedMesh(1);
                AssetDatabase.CreateAsset(meshLod1, directory + "/" + prefabName + "_LOD1.asset");
                GameObject goLod1 = new() { name = prefabName + "_LOD1" };
                goLod1.transform.SetParent(go.transform, false);
                goLod1.AddComponent<MeshFilter>().sharedMesh = meshLod1;
                goLod1.AddComponent<MeshRenderer>().sharedMaterial = temp;

                Mesh meshLod2 = GetCombinedMesh(2);
                AssetDatabase.CreateAsset(meshLod2, directory + "/" + prefabName + "_LOD2.asset");
                GameObject goLod2 = new() { name = prefabName + "_LOD2" };
                goLod2.transform.SetParent(go.transform, false);
                goLod2.AddComponent<MeshFilter>().sharedMesh = meshLod2;
                goLod2.AddComponent<MeshRenderer>().sharedMaterial = temp;

                LODGroup lodGroup = go.AddComponent<LODGroup>();
                LOD[] lods = new LOD[3];
                lods[0] = AssignRendererLODData(goLod0, 0.7f);
                lods[1] = AssignRendererLODData(goLod1, 0.05f);
                lods[2] = AssignRendererLODData(goLod2, 0.0025f);
                lodGroup.SetLODs(lods);
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);  // save the end result
            AssetDatabase.Refresh();    // refresh

            DestroyImmediate(go);   // destroy the temporary gameObject (and its children)
            transform.SetPositionAndRotation(oldPosition, oldRotation); // restore the preview pos/rot
#endif
        }

        private Mesh GetCombinedMesh(int _LODIndex)
        {
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();

            List<MeshFilter> filterList = new();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                ProceduralGrassPlane proceduralPlane = meshFilters[i].gameObject.GetComponent<ProceduralGrassPlane>();
                proceduralPlane.CreateGrassPlane();
                if (proceduralPlane.lodLevel >= _LODIndex)
                    filterList.Add(meshFilters[i]);
            }

            CombineInstance[] combine = new CombineInstance[filterList.Count];
            for (int i = 0; i < filterList.Count; i++)
            {
                combine[i].mesh = filterList[i].sharedMesh;
                combine[i].transform = filterList[i].transform.localToWorldMatrix;
            }

            Mesh mesh = new();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        private LOD AssignRendererLODData(GameObject _go, float _screenRelativeTransitionHeight)
        {
            Renderer[] renderers = new Renderer[1];
            renderers[0] = _go.GetComponent<Renderer>();
            return new LOD(_screenRelativeTransitionHeight, renderers);
        }

        public int GetMeshTriangleCount()
        {
            int triangleCount = 0;

            for (int i = 0; i < planeList?.Count; i++)
            {
                if (planeList[i] == null)
                    continue;

                MeshFilter meshFilter = planeList[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter)
                    triangleCount += meshFilter.sharedMesh.triangles.Length / 3;
            }

            return triangleCount;
        }

        public int GetMeshVertexCount()
        {
            int vertexCount = 0;

            for (int i = 0; i < planeList?.Count; i++)
            {
                if (planeList[i] == null)
                    continue;

                MeshFilter meshFilter = planeList[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter)
                    vertexCount += meshFilter.sharedMesh.vertexCount;
            }

            return vertexCount;
        }
    }
}