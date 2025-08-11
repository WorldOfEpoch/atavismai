using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using Google.Protobuf.WellKnownTypes;
using UnityEditor.SceneManagement;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Atavism
{
    [InitializeOnLoad]
    class InitialSetup
    {
        static InitialSetup()
        {
            try
            {
                if (Directory.Exists(Path.GetFullPath("Assets/..") + "/Assets/Standard Assets/Atavism Core"))
                {

                    if (!Directory.Exists(Path.GetFullPath("Assets/..") + "/Assets/Dragonsan/Atavism Core"))
                    {
                        Directory.Move(Path.GetFullPath("Assets/..") + "/Assets/Standard Assets/Atavism Core",Path.GetFullPath("Assets/..") + "/Assets/Dragonsan/Atavism Core");
                    }
                }
            }
            catch (Exception e)
            {
                
            }
//#if !UNITY_6000_0_OR_NEWER   
            if (!Directory.Exists(Path.GetFullPath("Assets/..") + "/Assets/TextMesh Pro/Resources"))
                InstallTMPEsenc();
//#endif
            if (!File.Exists(Path.GetFullPath("Assets/..") + "/InitialSetup.txt"))
            {
                EditorApplication.update += Update;

            }
            else
            {
                 CheckRequirements();
            }
        }
        [MenuItem("Window/Atavism/Atavism Check Requirements")]
        private static void CheckRequirements()
        {
         
            checkRequest = Client.List();    // List packages installed for the Project
            EditorApplication.update += ProgressRequest;
        }

        static void ProgressRequest()
        {
          //  Debug.Log("Progress " + checkRequest.IsCompleted);
            if (checkRequest.IsCompleted)
            {
                if (checkRequest.Status == StatusCode.Success)
                    foreach (var package in checkRequest.Result)
                    {
                        //Debug.Log("Progress "+package.name);
                        if (package.name.Equals("com.unity.textmeshpro"))
                        {
                            tmpro = package;
                            Debug.Log("Progress found TextMeshPro");
                        }
        
                        if (package.name.Equals("com.unity.addressables"))
                        {
                            Debug.Log("Progress found Addressables");
                            addr = package;
                        }
        
                        if (package.name.Equals("com.unity.shadergraph"))
                        {
                            Debug.Log("Progress found ShaderGraph");
                            shadergraph = package;
                        }
                        
                        if (package.name.Equals("com.unity.postprocessing"))
                        {
                            Debug.Log("Progress found postprocessing");
                            postprocessing = package;
                        }
                    }
                
                  if (!File.Exists(Path.GetFullPath("Assets/..") + "/AtPackage.txt"))
                {
                  //  Debug.LogError("AtPackage.txt not found");
                   FileStream f =  File.Create(Path.GetFullPath("Assets/..") + "/AtPackage.txt");
                   // f.Write("Installed Packages\n");
                   f.Close();
                } 
                
                // Read the old file.  
                string[] lines = File.ReadAllLines(Path.GetFullPath("Assets/..") + "/AtPackage.txt");
               // Debug.LogError("AtPackage.txt lines "+lines);
                // Write the new file over the old file.
                using (StreamWriter writer = new StreamWriter(Path.GetFullPath("Assets/..") + "/AtPackage.txt"))
                {
                  //  Debug.LogError("AtPackage.txt write");
                    bool tmpf = false;
                    bool addrf = false;
                    bool shgf = false;
                    bool pp2f = false;
                    for (int currentLine = 1; currentLine <= lines.Length; ++currentLine)
                    {

                        if (lines[currentLine - 1].StartsWith("com.unity.textmeshpro"))
                        {
                            tmpf = true;
                            if(tmpro!=null)
                                writer.WriteLine(tmpro.name+"@"+tmpro.version);
                        }
                        else if (lines[currentLine - 1].StartsWith("com.unity.addressables"))
                        {
                            addrf = true;
                            if(addr!=null)
                                writer.WriteLine(addr.name+"@"+addr.version);
                        }
                        else if (lines[currentLine - 1].StartsWith("com.unity.shadergraph"))
                        {
                            shgf = true;
                            if(shadergraph!=null)
                                writer.WriteLine(shadergraph.name+"@"+shadergraph.version);
                        }
                        else if (lines[currentLine - 1].StartsWith("com.unity.postprocessing"))
                        {
                            pp2f = true;
                            if(postprocessing!=null)
                                writer.WriteLine(postprocessing.name+"@"+postprocessing.version);
                        }
                        else
                        {
                            writer.WriteLine(lines[currentLine - 1]);
                        }
                    }
                    if (!tmpf)
                    {
                        if(tmpro!=null)
                            writer.WriteLine(tmpro.name+"@"+tmpro.version);
                    }
                    if (!addrf)
                    {
                        if(addr!=null)
                            writer.WriteLine(addr.name+"@"+addr.version);
                    }
                    if (!shgf)
                    {
                        if(shadergraph!=null)
                            writer.WriteLine(shadergraph.name+"@"+shadergraph.version);
                    }
                    if (!pp2f)
                    {
                        if(postprocessing!=null)
                            writer.WriteLine(postprocessing.name+"@"+postprocessing.version);
                    }
                    writer.Close();
                }
                
#if !UNITY_6000_0_OR_NEWER        
                if (tmpro == null)
                {
                    Debug.Log("Progress Install TextMeshPro");
                    EditorApplication.update += CheckRequest;
                    _addRequest = Client.Add("com.unity.textmeshpro");
                } else 
#endif                
                if (addr == null)
                {
                    Debug.Log("Progress Install Addressables");
                    EditorApplication.update += CheckRequest;
                    _addRequest = Client.Add("com.unity.addressables");
                }else if (shadergraph == null)
                {
                    Debug.Log("Progress Install ShaderGraph");
                    EditorApplication.update += CheckRequest;
                    _addRequest = Client.Add("com.unity.shadergraph");
                }else if (postprocessing == null)
                {
                    Debug.Log("Progress Install postprocessing");
                    EditorApplication.update += CheckRequest;
                    _addRequest = Client.Add("com.unity.postprocessing");
                }
                else
                {
                    AddSympols();
                    Debug.Log("All Package Requirements is OK");
                }
                EditorApplication.update -= ProgressRequest;
            }
        }
        
        static void CheckRequest()
        {
            if (_addRequest.IsCompleted)
            {
                Debug.Log("CheckRequest IsCompleted");
                EditorApplication.update -= ProgressShadergraph;
                if (_addRequest.Status == StatusCode.Success)
                {
                    AssetDatabase.Refresh();

                }
                else if (_addRequest.Status >= StatusCode.Failure)
                    Debug.Log("_addRequest Failure " + _addRequest.Error.message);

                checkRequest = Client.List(); // List packages installed for the Project
                EditorApplication.update += ProgressRequest;

                EditorApplication.update -= CheckRequest;
            }
        }
        
    static AddRequest _addRequest;


        [MenuItem("Window/Atavism/Atavism Unity Setup")]
        public static void SetupAtavismUnity()
        {
            if (EditorUtility.DisplayDialog("Atavism Setup", "Are you sure you want to process Atavism Setup for Unity?\n" +
                                                             "We will set your project settings as following:\n" +
                                                             "- Add necessary layers\n" +
                                                             "- Add scenes to the build settings\n" +
                                                             "- Add TextMesh Pro with installed Essentials\n" +
                                                             "- Set Player settings(Color space to Linear, and.NET to 4.x)", "Setup", "Do Not Setup"))
            {
                Setup();
            }
        }
        
        [MenuItem("Window/Atavism/Rebuild Assets Bundles")]
        public static void RebuildAssets()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        }

        static void Update()
        {
            PlayerPrefs.SetInt("AtavismSetup", 1);
            PlayerPrefs.Save();
            EditorApplication.update -= Update;
            TextWriter tw = new StreamWriter(Path.GetFullPath("Assets/..") + "/InitialSetup.txt", true);
            tw.Close();
            if (EditorUtility.DisplayDialog("Atavism Setup", "Are you sure you want to process Atavism Setup for Unity?\n" +
                                                             "We will set your project settings as following:\n" +
                                                             "- Add necessary layers\n" +
                                                             "- Add scenes to the build settings\n" +
                                                             "- Add TextMesh Pro with installed Essentials\n" +
                                                             "- Set Player settings(Color space to Linear, and.NET to 4.x)", "Setup", "Do Not Setup"))
            {
                Setup();
            }
        }

        static void InstallTMPEsenc()
        {
#if UNITY_6000_0_OR_NEWER
            AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);   
#else
            AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
#endif
            PlayerPrefs.SetInt("AtavismSetupTMP", 0);
            PlayerPrefs.Save();
            //   EditorSceneManager.OpenScene("Assets/Dragonsan/Scenes/Login.unity"); 

        }

        static void Setup()
        {
            
            
            
            string MainFolder = "Assets/Dragonsan/Scenes/";
            
            if (EditorUtility.DisplayDialog("Atavism Setup", "Do you want to load UIToolkit Scenes?", "Yes, load UI Toolkit Scenes", "No, load UGUI Scenes"))
            {
                MainFolder += "UI Toolkit/";
            }
            string SceneType = ".unity";
            string[] ScenesList = new string[] { "Login", "CharacterSelection", "MainWorld", "Arena1v1", "Arena2v2", "Deathmatch 1v1", "Deathmatch 2v2", "SinglePlayerPrivate", "GuildPrivate", };
            int ii = 0;
            int notexist = 0;
            for (ii = 0; ii < ScenesList.Length; ii++)
            {
                if (!File.Exists(MainFolder + ScenesList[ii] + SceneType))
                {
                    notexist++;
                }
            }
            // EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[/*original.Length + */ScenesList.Length - notexist];
            //  System.Array.Copy(original, newSettings, original.Length);
            int i = 0;
            int index = 0;/* original.Length;*/
            for (i = 0; i < ScenesList.Length; i++)
            {
                if (File.Exists(MainFolder + ScenesList[i] + SceneType))
                {
                    EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(MainFolder + ScenesList[i] + SceneType, true);
                    newSettings[index] = sceneToAdd;
                    index++;
                }
            }
            EditorBuildSettings.scenes = newSettings;
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var layerProp8 = layerProps.GetArrayElementAtIndex(8);
            layerProp8.stringValue = "Targetable";
            var layerProp9 = layerProps.GetArrayElementAtIndex(9);
            layerProp9.stringValue = "MiniMap";
            var layerProp10 = layerProps.GetArrayElementAtIndex(10);
            layerProp10.stringValue = "AtavismText";
            var layerProp11 = layerProps.GetArrayElementAtIndex(11);
            layerProp11.stringValue = "Socket";
            tagManager.ApplyModifiedProperties();
            // Client.Remove("TextMesh Pro");
            if (Directory.Exists(Path.GetFullPath("Assets/..") + "/Assets/TextMesh Pro"))
                Directory.Delete(Path.GetFullPath("Assets/..") + "/Assets/TextMesh Pro", true);
#if UNITY_2018_2
            // Client.Add("com.unity.textmeshpro@1.2.4");
            AssetDatabase.ImportPackage("Assets/Dragonsan/AtavismObjects/External/SystemThreading.unitypackage", false);
#endif
#if UNITY_2018_3
            AssetDatabase.ImportPackage("Assets/Dragonsan/AtavismObjects/External/SystemThreading.unitypackage", false);
#endif
#if UNITY_2018_2_OR_NEWER
            //   Client.Add("com.unity.textmeshpro");
#endif
            PlayerPrefs.SetInt("AtavismSetupTMP", 1);
            PlayerPrefs.Save();
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.graphicsJobs = true;
            
            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            
            //    AssetDatabase.ImportPackage("Assets/Dragonsan/AtavismEditor/Editor/Interfaces/system35.unitypackage", false);
            if (Directory.Exists(Path.GetFullPath("Assets/..") + "/Assets/TextMesh Pro"))
                Directory.Delete(Path.GetFullPath("Assets/..") + "/Assets/TextMesh Pro", true);

            Request = Client.List();    // List packages installed for the Project
            EditorApplication.update += Progress;
            //PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0_Subset);
         //  PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);

        }
       static void Progress()
        {
          //  Debug.Log("Progress "+Request.IsCompleted);
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    foreach (var package in Request.Result)
                    {
                        //Debug.Log("Progress "+package.name);
                        if (package.name.Equals("com.unity.textmeshpro"))
                        {
                            tmpro = package;
                            Debug.Log("Progress founf tmpro");
                        }

                        if (package.name.Equals("com.unity.addressables"))
                        {
                            Debug.Log("Progress found addressables");
                            addr = package;
                        }

                        if (package.name.Equals("com.unity.shadergraph"))
                        {
                            Debug.Log("Progress found shadergraph");
                            shadergraph = package;
                        }

                        if (package.name.Equals("com.unity.postprocessing"))
                        {
                            Debug.Log("Progress found postprocessing");
                            postprocessing = package;
                        }
                    }

                if (!File.Exists(Path.GetFullPath("Assets/..") + "/AtPackage.txt"))
                {
                    FileStream f =  File.Create(Path.GetFullPath("Assets/..") + "/AtPackage.txt");
                    // f.Write("Installed Packages\n");
                    f.Close();
                }
                
                // Read the old file.  
                string[] lines = File.ReadAllLines(Path.GetFullPath("Assets/..") + "/AtPackage.txt");
                //Debug.LogError("AtPackage.txt lines "+lines);
              //  // Write the new file over the old file.
                using (StreamWriter writer = new StreamWriter(Path.GetFullPath("Assets/..") + "/AtPackage.txt"))
                {
                    // Debug.LogError("AtPackage.txt write");
                    bool tmpf = false;
                    bool addrf = false;
                    bool shgf = false;
                    bool pp2f = false;
                    for (int currentLine = 1; currentLine <= lines.Length; ++currentLine)
                    {

                        if (lines[currentLine - 1].StartsWith("com.unity.textmeshpro"))
                        {
                            tmpf = true;
                            if(tmpro!=null)
                                writer.WriteLine(tmpro.name+"@"+tmpro.version);
                        }
                        else if (lines[currentLine - 1].StartsWith("com.unity.addressables"))
                        {
                            addrf = true;
                            if(addr!=null)
                                writer.WriteLine(addr.name+"@"+addr.version);
                        }
                        else if (lines[currentLine - 1].StartsWith("com.unity.shadergraph"))
                        {
                            shgf = true;
                            if(shadergraph!=null)
                                writer.WriteLine(shadergraph.name+"@"+shadergraph.version);
                        }
                        else if (lines[currentLine - 1].StartsWith("com.unity.postprocessing"))
                        {
                            pp2f = true;
                            if(postprocessing!=null)
                                writer.WriteLine(postprocessing.name+"@"+postprocessing.version);
                        }
                        else
                        {
                            writer.WriteLine(lines[currentLine - 1]);
                        }
                    }
                    if (!tmpf)
                    {
                        if(tmpro!=null)
                            writer.WriteLine(tmpro.name+"@"+tmpro.version);
                    }
                    if (!addrf)
                    {
                        if(addr!=null)
                            writer.WriteLine(addr.name+"@"+addr.version);
                    }
                    if (!shgf)
                    {
                        if(shadergraph!=null)
                            writer.WriteLine(shadergraph.name+"@"+shadergraph.version);
                    }
                    if (!pp2f)
                    {
                        if(postprocessing!=null)
                            writer.WriteLine(postprocessing.name+"@"+postprocessing.version);
                    }
                    writer.Close();
                }
#if !UNITY_6000_0_OR_NEWER                
                if (tmpro != null)
                {
                    if (!tmpro.version.Equals(tmpro.versions.recommended))
                    {
                        Request2 = Client.Add("com.unity.textmeshpro" + (tmpro.versions.recommended != "" ? '@' + tmpro.versions.recommended : ""));
                        EditorApplication.update += ProgressTmpro;
                    }
                    else
                    {
                        AssetDatabase.Refresh();
                        
                        
                        try
                        {
                            AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
#endif                        
                        if (addr != null)
                        {
                            if (!addr.version.Equals(addr.versions.recommended))
                            {
                                Debug.Log("ProgressTmpro install addressables");
                                EditorApplication.update += ProgressAddressables;
                                Request3 = Client.Add("com.unity.addressables" + (addr.versions.recommended != "" ? '@' + addr.versions.recommended : ""));
                            }
                            else
                            {
                                if (shadergraph != null)
                                {
                                    if (!shadergraph.version.Equals(shadergraph.versions.recommended))
                                    {
                                        Debug.Log("ProgressTmpro install shadergraph");
                                        EditorApplication.update += ProgressShadergraph;
                                        Request3 = Client.Add("com.unity.shadergraph" + (addr.versions.recommended != "" ? '@' + addr.versions.recommended : ""));
                                    }
                                    else
                                    {
                                        if (postprocessing != null)
                                        {
                                            if (!postprocessing.version.Equals(postprocessing.versions.recommended))
                                            {
                                                Debug.Log("ProgressTmpro install postprocessing");
                                                EditorApplication.update += ProgressPostprocessing;
                                                Request4 = Client.Add("com.unity.postprocessing" + (addr.versions.recommended != "" ? '@' + addr.versions.recommended : ""));
                                            }
                                            else
                                            {
                                                AddSympols();
                                                EditorUtility.DisplayDialog("Atavism Setup",
                                                    "Atavism setup was successful",
                                                    "OK", "");
                                            }
                                        }
                                        else
                                        {
                                            Debug.Log("ProgressTmpro search postprocessing");
                                            EditorApplication.update += ProgressSearchPostprocessing;
                                            RequestSearch = Client.Search("com.unity.postprocessing");
                                           
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log("ProgressTmpro search shadergraph");
                                    EditorApplication.update += ProgressSearchShadergraph;
                                    RequestSearch = Client.Search("com.unity.shadergraph");
                                }
                            }
                            
                        }
                        else
                        {
                            Debug.Log("ProgressTmpro search addressables");
                            EditorApplication.update += ProgressSearchAddressables;
                            RequestSearch = Client.Search("com.unity.addressables");
                        }
#if !UNITY_6000_0_OR_NEWER
                    }
                }
                else
                {
                    Debug.Log("ProgressTmpro search textmeshpro");
                    EditorApplication.update += ProgressSearchTmpro;
                    RequestSearchTmp = Client.Search("com.unity.textmeshpro");
                }
#endif                
                EditorApplication.update -= Progress;
            }
        }
        static void ProgressSearchPostprocessing()
        {
            //  Debug.Log("ProgressSearchShadergraph "+RequestSearch.IsCompleted);
            if (RequestSearch.IsCompleted)
            {
                EditorApplication.update -= ProgressSearchPostprocessing;
                if (RequestSearch.Status == StatusCode.Success)
                {
                    Debug.Log("ProgressSearchPostprocessing success ");
                    foreach (var package in RequestSearch.Result)
                    {
                        Debug.Log("ProgressSearchPostprocessing Package name: " + package.name + " " + package.version + " " + package.versions.latestCompatible + " " + package.versions.latest + " " + package.versions.recommended);

                        if (package.name.Equals("com.unity.postprocessing"))
                        {
                            Debug.Log("ProgressSearchPostprocessing install postprocessing");
                            EditorApplication.update += ProgressPostprocessing;
                            Request3 = Client.Add("com.unity.postprocessing" + (package.versions.recommended != "" ? '@' + package.versions.recommended : ""));
                        }
                    }
                }
                else
                {
                    Debug.Log("ProgressSearchPostprocessing no success ");
                }
            }
        }
       static void ProgressSearchShadergraph()
       {
         //  Debug.Log("ProgressSearchShadergraph "+RequestSearch.IsCompleted);
           if (RequestSearch.IsCompleted)
           {
               EditorApplication.update -= ProgressSearchShadergraph;
               if (RequestSearch.Status == StatusCode.Success)
               {
                   Debug.Log("ProgressSearchShadergraph success ");
                   foreach (var package in RequestSearch.Result)
                   {
                       Debug.Log("ProgressSearchShadergraph Package name: " + package.name + " " + package.version + " " + package.versions.latestCompatible + " " + package.versions.latest + " " + package.versions.recommended);

                       if (package.name.Equals("com.unity.shadergraph"))
                       {
                           Debug.Log("ProgressSearchShadergraph install shadergraph");
                           EditorApplication.update += ProgressShadergraph;
                           Request3 = Client.Add("com.unity.shadergraph" + (package.versions.recommended != "" ? '@' + package.versions.recommended : ""));
                       }
                   }
               }
               else
               {
                   Debug.Log("ProgressSearchShadergraph no success ");
               }
           }
       }
       
        static void ProgressSearchAddressables()
        {
            Debug.Log("ProgressSearchAddressables "+RequestSearch.IsCompleted);
            if (RequestSearch.IsCompleted)
            {
                EditorApplication.update -= ProgressSearchAddressables;
                if (RequestSearch.Status == StatusCode.Success)
                {
                    Debug.Log("ProgressSearchAddressables success ");
                    foreach (var package in RequestSearch.Result)
                    {
                        Debug.Log("ProgressSearchAddressables Package name: " + package.name + " " + package.version + " " + package.versions.latestCompatible + " " + package.versions.latest + " " + package.versions.recommended);

                        if (package.name.Equals("com.unity.addressables"))
                        {
                            Debug.Log("ProgressSearchAddressables install addressables");
                            EditorApplication.update += ProgressAddressables;
                            Request3 = Client.Add("com.unity.addressables" + (package.versions.recommended != "" ? '@' + package.versions.recommended : ""));
                        }
                    }
                }
                else
                {
                    Debug.Log("ProgressSearchAddressables no success ");
                }
            }
        }

        static void ProgressSearchTmpro()
        {
           // Debug.Log("ProgressSearchTmpro "+RequestSearchTmp.IsCompleted);
            if (RequestSearchTmp.IsCompleted)
            {
                EditorApplication.update -= ProgressSearchTmpro;
                if (RequestSearchTmp.Status == StatusCode.Success)
                {
                    Debug.Log("ProgressSearchTmpro success ");
                    foreach (var package in RequestSearchTmp.Result)
                    {
                        Debug.Log("ProgressSearchTmpro Package name: " + package.name + " " + package.version + " " + package.versions.latestCompatible + " " + package.versions.latest + " " + package.versions.recommended);
                        if (package.name.Equals("com.unity.textmeshpro"))
                        {
                            Request2 = Client.Add("com.unity.textmeshpro" + (package.versions.recommended != "" ? '@' + package.versions.recommended : ""));
                            EditorApplication.update += ProgressTmpro;
                        }
                    }
                }
                else
                {
                    Debug.Log("ProgressSearchTmpro no success ");
                }
            }
        }


        static void ProgressTmpro()
        {
         //   Debug.Log("ProgressTmpro");

            if (Request2.IsCompleted)
            {
                if (Request2.Status == StatusCode.Success)
                {
                    EditorApplication.update -= ProgressTmpro;
                    //   AssetDatabase.Refresh();
                    try
                    {
                        //  AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (addr != null)
                    {
                        Debug.Log("ProgressTmpro addr not null");
                        if (!addr.version.Equals(addr.versions.recommended))
                        {
                            Debug.Log("ProgressTmpro install addressables");
                            EditorApplication.update += ProgressAddressables;
                            Request3 = Client.Add("com.unity.addressables" + (addr.versions.recommended != "" ? '@' + addr.versions.recommended : ""));
                        }
                        else
                        {
                            if (shadergraph != null)
                            {
                                if (!shadergraph.version.Equals(shadergraph.versions.recommended))
                                {
                                    Debug.Log("ProgressTmpro install shadergraph");
                                    EditorApplication.update += ProgressShadergraph;
                                    Request3 = Client.Add("com.unity.shadergraph" + (addr.versions.recommended != "" ? '@' + addr.versions.recommended : ""));
                                }else
                                {
                                    if (postprocessing != null)
                                    {
                                        if (!postprocessing.version.Equals(postprocessing.versions.recommended))
                                        {
                                            Debug.Log("ProgressTmpro install postprocessing");
                                            EditorApplication.update += ProgressPostprocessing;
                                            Request3 = Client.Add("com.unity.postprocessing" + (addr.versions.recommended != "" ? '@' + addr.versions.recommended : ""));
                                        }
                                        else
                                        {
                                            AddSympols();
                                            EditorUtility.DisplayDialog("Atavism Setup",
                                                "Atavism setup was successful",
                                                "OK", "");
                                    
                                            try
                                            {
#if UNITY_6000_0_OR_NEWER
                            AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);
#else
                                                AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
#endif                  
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("ProgressTmpro search postprocessing");
                                        EditorApplication.update += ProgressSearchPostprocessing;
                                        RequestSearch = Client.Search("com.unity.postprocessing");
                                           
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("ProgressTmpro search shadergraph");
                                EditorApplication.update += ProgressSearchShadergraph;
                                RequestSearch = Client.Search("com.unity.shadergraph");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("ProgressTmpro search addr");
                        RequestSearch = Client.Search("com.unity.addressables");
                        EditorApplication.update += ProgressSearchAddressables;
                    }
                }
            }
        }

     
        
        
        
        
        
        static void ProgressAddressables()
        {
         //   Debug.Log("ProgressAddressables");

            if (Request3.IsCompleted)
            {
                Debug.Log("ProgressAddressables IsCompleted");
                EditorApplication.update -= ProgressAddressables;
                if (Request3.Status == StatusCode.Success)
                {
                    Debug.Log("ProgressAddressables Success");
                    if (shadergraph != null)
                    {
                        if (!shadergraph.version.Equals(shadergraph.versions.recommended))
                        {
                            Debug.Log("ProgressAddressables install shadergraph");
                            EditorApplication.update += ProgressShadergraph;
                            Request3 = Client.Add("com.unity.shadergraph" +
                                                  (addr.versions.recommended != ""
                                                      ? '@' + addr.versions.recommended
                                                      : ""));
                        }
                        else
                        {
                            if (postprocessing != null)
                            {
                                if (!postprocessing.version.Equals(postprocessing.versions.recommended))
                                {
                                    Debug.Log("ProgressTmpro install postprocessing");
                                    EditorApplication.update += ProgressPostprocessing;
                                    Request3 = Client.Add("com.unity.postprocessing" + (addr.versions.recommended != ""
                                        ? '@' + addr.versions.recommended
                                        : ""));
                                }
                                else
                                {
                                    AddSympols();
                                    EditorUtility.DisplayDialog("Atavism Setup",
                                        "Atavism setup was successful",
                                        "OK", "");
                                    try
                                    {
#if UNITY_6000_0_OR_NEWER
                            AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);
#else
                                        AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
#endif                  
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("ProgressAddressables search shadergraph");
                        EditorApplication.update += ProgressSearchShadergraph;
                        RequestSearch = Client.Search("com.unity.shadergraph");
                    }

                    }
                else if (Request3.Status >= StatusCode.Failure)
                    Debug.Log("Failure "+Request3.Error.message);

              
            }
        }
        
        static void ProgressShadergraph()
        {
        //    Debug.Log("ProgressShadergraph");

            if (Request3.IsCompleted)
            {
                Debug.Log("ProgressShadergraph IsCompleted");
                EditorApplication.update -= ProgressShadergraph;
                if (Request3.Status == StatusCode.Success)
                {
                    Debug.Log("ProgressShadergraph Success");
                    if (postprocessing != null)
                    {
                        if (!postprocessing.version.Equals(postprocessing.versions.recommended))
                        {
                            Debug.Log("ProgressTmpro install postprocessing");
                            EditorApplication.update += ProgressPostprocessing;
                            Request3 = Client.Add("com.unity.postprocessing" + (addr.versions.recommended != ""
                                ? '@' + addr.versions.recommended
                                : ""));
                        }
                        else 
                        {
                            AddSympols();
                            EditorUtility.DisplayDialog("Atavism Setup",
                                "Atavism setup was successful",
                                "OK", "");
                            try
                            {
#if UNITY_6000_0_OR_NEWER
                            AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);
#else
                                AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
#endif                  
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
                else if (Request3.Status >= StatusCode.Failure)
                    Debug.Log("Failure "+Request3.Error.message);

              
            }
        }

        static void ProgressPostprocessing()
        {
              //  Debug.Log("ProgressPostprocessing");

            if (Request4.IsCompleted)
            {
              //  Debug.Log("ProgressPostprocessing IsCompleted");
                EditorApplication.update -= ProgressPostprocessing;
                if (Request4.Status == StatusCode.Success)
                {
                  //  Debug.Log("ProgressPostprocessing Success");
                    AddSympols();
                    AssetDatabase.Refresh();
                    try
                    {
#if UNITY_6000_0_OR_NEWER
                            AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);
#else
                        AssetDatabase.ImportPackage("Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage", false);
#endif                  
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    EditorUtility.DisplayDialog("Atavism Setup", "Atavism setup was successful", "OK", "");

                }
                else if (Request4.Status >= StatusCode.Failure)
                    Debug.Log("Failure "+Request4.Error.message);

              
            }
        }

        static void AddSympols()
        {
          //  Debug.Log("AddAddressablesSympol");
            if (startTime > DateTime.UtcNow)
            {
               // Debug.Log("AddAddressablesSympol skipped");
                return;
            }

            startTime = DateTime.UtcNow.AddSeconds(100);
          //  Debug.Log("AddAddressablesSympol | ");
            AssetDatabase.Refresh();
             string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
           //  Debug.Log("AddAddressablesSympol symbols "+symbols);
            bool added = false;
            if (!symbols.Contains("AT_ADDRESSABLES"))
            {
                symbols += ";" + "AT_ADDRESSABLES";
                added=true;
             //   Debug.Log("AddAddressablesSympol  addr");
            }
            if (!symbols.Contains("AT_PPS2_PRESET"))
            {
                symbols += ";" + "AT_PPS2_PRESET";
                added = true;
            //    Debug.Log("AddAddressablesSympol  PPS2");
            }
            
            if (added)
            {
             //   Debug.Log("AddAddressablesSympol || ");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }

        private static PackageInfo tmpro;
        private static PackageInfo addr;
        private static PackageInfo shadergraph;
        private static PackageInfo postprocessing;
        static ListRequest Request;
        static ListRequest checkRequest;
        static SearchRequest RequestSearch;
        static SearchRequest RequestSearchTmp;
        static AddRequest Request2;
        static AddRequest Request3;
        static AddRequest Request4;
        static AddRequest Request5;
        static DateTime startTime = DateTime.UtcNow;
    }
}
