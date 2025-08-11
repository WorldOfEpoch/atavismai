using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Atavism
{
    [InitializeOnLoad]
    class InitialSetupUma
    {
        static InitialSetupUma()
        {
         /*   if (PlayerPrefs.GetInt("AtavismSetupUma", 0) == 0)
            {
                EditorApplication.update += Update;
            }*/
            if (!File.Exists(Path.GetFullPath("Assets/..") + "/InitialSetupUma.txt"))
            {
                EditorApplication.update += Update;

            }
        }

        private static List<Object> AddedDuringGui = new List<Object>();

        [MenuItem("Window/Atavism Uma Setup")]
        public static void SetupAtavismUnity()
        {
            Setup();
        }

        static void Update()
        {
            PlayerPrefs.SetInt("AtavismSetupUma", 1);
            PlayerPrefs.Save();
            TextWriter tw = new StreamWriter(Path.GetFullPath("Assets/..") + "/InitialSetupUma.txt", true);
            tw.Close();

            EditorApplication.update -= Update;
            Setup();
        }

        private static void AddObject(Object draggedObject)
        {
            System.Type type = draggedObject.GetType();
            if (UMA.UMAAssetIndexer.Instance.IsIndexedType(type))
            {
                UMA.UMAAssetIndexer.Instance.EvilAddAsset(type, draggedObject);
            }
        }

        private static void RecursiveScanFoldersForAssets(string path)
        {
            var assetFiles = System.IO.Directory.GetFiles(path);

            foreach (var assetFile in assetFiles)
            {
                string Extension = System.IO.Path.GetExtension(assetFile).ToLower();
                if (Extension == ".asset" || Extension == ".controller" || Extension == ".txt")
                {
                    Object o = AssetDatabase.LoadMainAssetAtPath(assetFile);
                    if (o)
                    {
                        AddedDuringGui.Add(o);
                    }
                }
            }
            foreach (var subFolder in System.IO.Directory.GetDirectories(path))
            {
                RecursiveScanFoldersForAssets(subFolder.Replace('\\', '/'));
            }
        }

        static void Setup()
        {
            AddedDuringGui.Clear();
            var path = "Assets/Dragonsan/AtavismObjects/UMA";
            //var path = "Assets/Dragonsan/AtavismObjects/UMAContent";
            if (System.IO.Directory.Exists(path))
            {
                RecursiveScanFoldersForAssets(path);
            }
            //var path = "Assets/Dragonsan/AtavismObjects/UMA";
            path = "Assets/Dragonsan/AtavismObjects/UMAContent";
            if (System.IO.Directory.Exists(path))
            {
                RecursiveScanFoldersForAssets(path);
            }


            if (AddedDuringGui.Count > 0)
            {
                for (int i = 0; i < AddedDuringGui.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Adding Items to Global Library.", AddedDuringGui[i].name, ((float)i / (float)AddedDuringGui.Count));
                    AddObject(AddedDuringGui[i]);
                }
                EditorUtility.ClearProgressBar();
                UMA.UMAAssetIndexer.Instance.ForceSave();
            }
            SetupScene();
        }

        static void SetupScene()
        {
            string MainFolder = "Assets/Dragonsan/Scenes/UMA/";
            string SceneType = ".unity";
            string ScenesName = "CharacterSelection";
            string scene = "";
           // string sceneCSNoUma = "Assets/Dragonsan/Scenes/CharacterSelection.unity";
            if (Directory.Exists(Path.GetFullPath("Assets/..") + "/Assets/Atavism demo"))
            {
                scene = MainFolder + "Demo/" + ScenesName + SceneType;
            }
            else
            {
                scene = MainFolder + "Core/" + ScenesName + SceneType;
            }
            if (!File.Exists(scene))
            {
                return;
            }

            EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            int i = 0;
            bool exist = false;
            for (i = 0; i < original.Length; i++)
            {
                if (original[i].path.Contains(ScenesName))
                {
                    original[i].enabled = false;
                }
                if (original[i].path.Equals(scene))
                {
                    original[i].enabled = true;
                    exist = true;
                }
            }
            EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[original.Length + (exist ? 0 : 1)];
            System.Array.Copy(original, newSettings, original.Length);
            int index = original.Length;
            if (!exist)
            {
                EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(scene, true);
                newSettings[index] = sceneToAdd;
            }
            EditorBuildSettings.scenes = newSettings;
        }

    }
}
