#if VEGETATION_SYSTEM_PRO && VSP_PACKAGES
using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.VegetationSystem;
using System.Collections;
using UnityEngine;

public class VSPDemo_Scripting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] VegetationSystemPro vspSys;        // reference to the VSP system
    [SerializeField] VegetationPackagePro vegPackage;   // reference to a vegetationPackage

    [Header("Dynamic values for a graphic settings menu")]
    [SerializeField] int grassDistance = 42;
    [SerializeField] int objectDistance = 69;
    [SerializeField] int treeDistance = 420;
    [SerializeField] int billboardDistanceFactor = 1;

    [Header("System values for changing generation of vegetation instances")]
    [SerializeField] int grassDensity = 1;
    [SerializeField] int treeDensity = 1;

    [Header("Other settings")]
    [SerializeField] bool toggleRuntimeState = false;
    [SerializeField] int[] vegetationItemIndices = { 3, 1 };    // 3 = trees, 1 = grass2

    void Start()
    {
        // get reference dynamically if needed

        //vspSys = GetComponent<VegetationSystemPro>();
        //vspSys = GetComponentInChildren<VegetationSystemPro>();
        //vspSys = FindAnyObjectByType<VegetationSystemPro>();
        //vspSys = [...]

        StartCoroutine(RandomizeVegetationItemSettings());
    }

    void Update()
    {
        // global system vegetation distances -- no refresh needed as the rendering handles culling of vegetation instances dynamically
        vspSys.vegetationSettings.grassDistance = grassDistance;
        vspSys.vegetationSettings.objectDistance = objectDistance;
        vspSys.vegetationSettings.treeDensity = treeDistance;

        vspSys.vegetationSettings.billboardDistanceFactor = billboardDistanceFactor;    // refresh needed for billboard cells
        ///To ensure clean operation at runtime look into the "VegetationSystemProEditor.cs" script and what refreshes get called after making changes
        ///-> be aware that some refreshes within "Editor" scripts might use unoptimized mass refreshes
        ///--> instead more abstract/lower level refreshes should be used when real time changes are needed
        ///

        /// global system density -- changing this requires a "hard refresh" as the vegetation cells need their data regenerated since the internal system grid/-s needs to be restructured
        /// -> each vegetation cell or rather each vegetation item has its own grid based on its "sample distance" and the "global vegetation density"
        /// --> the local density per vegetation item is only used for exclusion
        vspSys.vegetationSettings.grassDensity = grassDensity;
        vspSys.vegetationSettings.treeDensity = treeDensity;

        for (int i = 0; i < vegetationItemIndices.Length; i++)  // set new item runtime states using the vegetationItemIndices in the array
            vegPackage.VegetationInfoList[vegetationItemIndices[i]].EnableRuntimeSpawn = toggleRuntimeState;

        if (Input.GetKeyDown(KeyCode.O))
            RefreshVSPSystem(); // manual "hard refresh"
    }

    /// <summary>
    ///  needed refreshes when changing things like global system density and most other rules of vegetation items
    ///  -> hard refreshes are mostly needed when changes get made that are internally tied to vegetation cells
    ///  --> so that they regenerate their data / the rules of the vegetation items
    ///  -> and/or when changes to terrains get made like ex: unity terrain painting/sculpting
    ///  --> which passively affect the rule of vegetation items
    /// </summary>
    void RefreshVSPSystem()
    {
        ///Use any "Refresh" or "Clear" function of the main system to refresh related data all at once
        ///

        vspSys.ClearCache();
        //vspSys.ClearCache(new Bounds(Vector3.zero, Vector3.one));    // there are several overloads which can improve performance instead of clearing all cells at once
        vspSys.RefreshTerrainHeightmap();
    }

    IEnumerator RandomizeVegetationItemSettings()
    {
        while (true)    // behave like an update loop
        {
            float FPS = 1f / 20f;
            yield return new WaitForSeconds(FPS);   // run loop at "20 fps"

            if (Time.frameCount % 10 == 0)  // update/refresh only every 10 frames
                for (int i = 0; i < vspSys.vegetationPackageProList.Count; i++)
                    for (int j = 0; j < vspSys.vegetationPackageProList[i].VegetationInfoList.Count; j++)   // for all vegetationItems in all vegetationPackages added to the accessed vspSys
                    {
                        ///Below are examples of accessing the ITEM info of a vegetation item
                        ///
                        ///The data is stored in a vegetationPackage which is a scriptable object of the engine
                        ///-> it comes with a read-only limitation at runtime(in a build) and made changes are lost
                        ///==> create and save own lists/deltas to the disk/a database to save changes and write them back later
                        ///

                        //VegetationPackagePro vegetationPackage = vspSys.vegetationPackageProList[i];  // vegetation package containing all vegetation items and their rules
                        //VegetationItemInfoPro vegetationItemInfo = vspSys.vegetationPackageProList[i].VegetationInfoList[j];  // vegetaton item info containing all its rules
                        vspSys.vegetationPackageProList[i].VegetationInfoList[j].ScaleMultiplier = Vector3.one * Random.Range(0.33f, 1f);   // randomize the scale of this vegetation item => affects all of its instances in the scene
                        RefreshVSPSystem(); // refresh to apply changes -- a "hard refresh" gets used (as an example) thus the loop gets rate limited to reduce performance usage

                        ///

                        ///Below are examples of accessing the MODEL info of a vegetation item
                        ///-> the model info can be used to call certain lower level refreshes instead of refreshing the entire system
                        ///--> get info related to the mesh/material/billboard at runtime
                        ///--> refresh materials/billoards at runtime
                        ///The info gets (re-)created at runtime so modifications are not permanent
                        ///
                        ///==> For general usage accessing the MODEL info is not needed and should rather be avoided
                        ///

                        //VegetationPackageProModelInfo vegetationModelPackage = vspSys.vegetationPackageProModelsList[i];  // vegetation package containing all vegetation items and their at runtime created model data
                        //VegetationItemModelInfo vegetationModelInfo = vspSys.vegetationPackageProModelsList[i].vegetationItemModelList[j];    // vegetation item model info of a vegetation item containing all its model related data
                        ShaderControllerSettings[] shaderControllerSettings = vspSys.vegetationPackageProModelsList[i].vegetationItemModelList[j].vegetationItemInfo.ShaderControllerSettings;  // material settings of the vegetation item
                        vspSys.vegetationPackageProModelsList[i].vegetationItemModelList[j].RefreshMaterials(); // direct call of a lower level refresh using the model info -- lighter refresh thus no frameLimit used

                        ///The example above would be the only "more common" use case of accessing the MODEL info at runtime
                        ///-> the ITEM info can be accessed through the MODEL info which in turn gives access to the shader/material settings of a vegetation item
                        ///--> then call the RefreshMaterials() function only for that one vegetation item using its MODEL info
                        ///
                    }
        }
    }
}
#endif