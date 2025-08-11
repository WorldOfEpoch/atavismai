using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

//using VolumetricFogAndMist;

namespace Atavism.UI
{

    public class UIAtavismGraphicSettings //: MonoBehaviour
    {
        [AtavismSeparator("Resolution")]
        // public Dropdown monitorSelect;
        [SerializeField] UIDropdown resolutionSelect;
        [SerializeField] Toggle fullscreenToggle;
        [SerializeField] Toggle fpsToggle;
        [AtavismSeparator("Quality")]
        [SerializeField] UIDropdown qualitySelect;
        [SerializeField] UIDropdown shadowsDropdown;
        [SerializeField] UIDropdown shadowResolutionDropdown;
        [SerializeField] UIDropdown shadowDistanceDropdown;
        [SerializeField] Toggle verticalSyncToggle;
        [SerializeField] UIDropdown lodBiasDropdown;
        [SerializeField] UIDropdown particleRaycastBudgetDropdown;
        [SerializeField] UIDropdown masterTextureLimitDropdown;
        [SerializeField] Toggle softParticlesToggle;
        [AtavismSeparator("Effects")]
        [SerializeField] UIDropdown antialiasingDropdown;
        [SerializeField] Toggle ambiertOcclusionsToggle;
        [SerializeField] Toggle screenSpaceToggle;
        [SerializeField] Toggle volumetricFogToggle;
        [SerializeField] Toggle volumetricCameraToggle;
        [SerializeField] Toggle depthOfFieldToggle;
        [SerializeField] Toggle bloomToggle;
        [SerializeField] Toggle vignetteToggle;
        [SerializeField] Toggle chromaticAberrationToggle;
        [SerializeField] Toggle motionBlurToggle;
        [SerializeField] Toggle autoExposureToggle;
        [SerializeField] Toggle colorGradingToggle;
        [SerializeField] Toggle ditheringToggle;
        [SerializeField] Toggle screenSpaceReflectionsToggle;
        [AtavismSeparator("Terrain")]
        [SerializeField] Slider grassSlider;
        [AtavismSeparator("Wather")]
        // public Dropdown warterQualitySelect;
        // public Toggle ReflectionsToggle;
        //  public Dropdown waterResolutionSelect;
        //  public Dropdown waterRenderingSelect;
        //   private Display[] displays;
        private Resolution[] resolutionsAll;
        private List<Resolution> resolutions = new List<Resolution>();
        private bool fulscr;
        private float startTimer = 0f;
        private MonoBehaviour monoBehaviour;

        public void Setup(VisualElement visualElement, VisualElement Screen, MonoBehaviour monoBehaviour)
        {
            this.monoBehaviour = monoBehaviour;
            
            resolutionSelect= visualElement.Q<UIDropdown>("Resolution");
            resolutionSelect.Screen = Screen;
            resolutionSelect.RegisterCallback<ChangeEvent<int>>(ChangeResolution);
            fullscreenToggle= visualElement.Q<Toggle>("FullscreenToggle");
            fullscreenToggle.RegisterValueChangedCallback(ChangeFullScreen);
            fpsToggle= visualElement.Q<Toggle>("FPSToggle");
            fpsToggle.RegisterValueChangedCallback(ChangeFps);
            // [AtavismSeparator("Quality")]
            qualitySelect= visualElement.Q<UIDropdown>("GraphicQuality");
            qualitySelect.Screen = Screen;
            qualitySelect.RegisterCallback<ChangeEvent<int>>(ChangeQuality);
            shadowsDropdown= visualElement.Q<UIDropdown>("Shadows");
            shadowsDropdown.Screen = Screen;
            shadowsDropdown.RegisterCallback<ChangeEvent<int>>(ChangeShadows);
            shadowResolutionDropdown= visualElement.Q<UIDropdown>("ShadowsResolutions");
            shadowResolutionDropdown.Screen = Screen;
            shadowResolutionDropdown.RegisterCallback<ChangeEvent<int>>(ChangeShadowsRez);
            shadowDistanceDropdown= visualElement.Q<UIDropdown>("ShadowsDistance");
            shadowDistanceDropdown.Screen = Screen;
            shadowDistanceDropdown.RegisterCallback<ChangeEvent<int>>(ChangeShadowsDist);
            verticalSyncToggle= visualElement.Q<Toggle>("VSyncToggle");
            verticalSyncToggle.RegisterValueChangedCallback(ChangeSync);
            lodBiasDropdown= visualElement.Q<UIDropdown>("LodObjects");
            lodBiasDropdown.Screen = Screen;
            lodBiasDropdown.RegisterCallback<ChangeEvent<int>>(ChangeLodbias);
            particleRaycastBudgetDropdown= visualElement.Q<UIDropdown>("Badget");
            particleRaycastBudgetDropdown.Screen = Screen;
            particleRaycastBudgetDropdown.RegisterCallback<ChangeEvent<int>>(ChangeBudget);
            masterTextureLimitDropdown= visualElement.Q<UIDropdown>("TextureQualityPopup");
            masterTextureLimitDropdown.Screen = Screen;
            masterTextureLimitDropdown.RegisterCallback<ChangeEvent<int>>(ChangeTexture);
            softParticlesToggle= visualElement.Q<Toggle>("SoftParticlesToggle");
            softParticlesToggle.RegisterValueChangedCallback(ChangeSoftParticels);
            // [AtavismSeparator("Effects")]
            antialiasingDropdown= visualElement.Q<UIDropdown>("Antialiasing");
            antialiasingDropdown.Screen = Screen;
            antialiasingDropdown.RegisterCallback<ChangeEvent<int>>(ChangeAntialiasing);
            ambiertOcclusionsToggle= visualElement.Q<Toggle>("AmbientOcclusionToggle");
            ambiertOcclusionsToggle.RegisterValueChangedCallback(ChangeEffects);
            
            // screenSpaceToggle= visualElement.Q<Toggle>("windows");
            // screenSpaceToggle.RegisterValueChangedCallback(chan);
            // volumetricFogToggle= visualElement.Q<Toggle>("windows");
            // volumetricFogToggle.RegisterValueChangedCallback(chan);
            // volumetricCameraToggle= visualElement.Q<Toggle>("");
            // volumetricCameraToggle.RegisterValueChangedCallback(chan);
            
            depthOfFieldToggle= visualElement.Q<Toggle>("DepthOfFieldToggle");
            depthOfFieldToggle.RegisterValueChangedCallback(ChangeEffects);
            bloomToggle= visualElement.Q<Toggle>("BloomToggle");
            bloomToggle.RegisterValueChangedCallback(ChangeEffects);
            vignetteToggle= visualElement.Q<Toggle>("VignetteToggle");
            vignetteToggle.RegisterValueChangedCallback(ChangeEffects);
            chromaticAberrationToggle= visualElement.Q<Toggle>("ChromaticAberrationToggle");
            chromaticAberrationToggle.RegisterValueChangedCallback(ChangeEffects);
            motionBlurToggle= visualElement.Q<Toggle>("MotionBlurToggle");
            motionBlurToggle.RegisterValueChangedCallback(ChangeEffects);
            autoExposureToggle= visualElement.Q<Toggle>("AutoExposeToggle");
            autoExposureToggle.RegisterValueChangedCallback(ChangeEffects);
            colorGradingToggle= visualElement.Q<Toggle>("ColorGradingToggle");
            colorGradingToggle.RegisterValueChangedCallback(ChangeEffects);
            ditheringToggle= visualElement.Q<Toggle>("DitheringToggle");
            ditheringToggle.RegisterValueChangedCallback(ChangeEffects);
            screenSpaceReflectionsToggle= visualElement.Q<Toggle>("ScreenSpaceReflectionToggle");
            screenSpaceReflectionsToggle.RegisterValueChangedCallback(ChangeEffects);
            
            startTimer = Time.time + 0.5f;
            updateResolutions();
            updParam();
        }

        private void ChangeFullScreen(ChangeEvent<bool> evt)
        {
            ChangeResolution(null);
        }

        void OnEnable()
        {
            startTimer = Time.time + 0.5f;
            updateResolutions();
            updParam();
        }
        void updateResolutions()
        {
            List<string> Options2 = new List<string>();

            resolutionsAll = Screen.resolutions;
          //  Debug.LogError("Grpthic updParam resolutionsAll=" + resolutionsAll + " " + resolutionsAll.Length);
            // resolutionSelect.ClearOptions();
            resolutions.Clear();
            var m = 0;
            int k = 0;
            List<string> resol = new List<string>();
            foreach (Resolution res in resolutionsAll)
            {
                if (!resol.Contains(res.width + "x" + res.height))
                {
                    resol.Add(res.width + "x" + res.height);
                    resolutions.Add(res);
                    Options2.Add(res.width + "x" + res.height);
                    if (Screen.width == res.width && Screen.height == res.height)
                    {
                        k = m;
                    }
                    m++;
                }
            }
            resol.Clear();
            resolutionSelect.Options(Options2);
            resolutionSelect.Index = k;
        }
        public void updParam()
        {
            startTimer = Time.time + 0.5f;
            fpsToggle.value = AtavismSettings.Instance.GetVideoSettings().fps;
            /*     displays = Display.displays;
              //   monitorSelect.options.Clear();
                 int i = 0;
                 foreach (Display disp in displays)
                 {
                     //   Debug.Log(disp);
                     monitorSelect.options.Add(new Dropdown.OptionData("Display " + i++));
                     //monitorSelect.SelectOption();
                 }
         #if UNITY_EDITOR
                 monitorSelect.value = 0;
         #else
                 monitorSelect.value = 0;
         #endif
         */
            fullscreenToggle.value = Screen.fullScreen;
            List<string> Options2 = new List<string>();

            resolutionsAll = Screen.resolutions;
           // Debug.LogError("Grpthic updParam resolutionsAll="+ resolutionsAll+" "+ resolutionsAll.Length);
            //resolutionSelect.ClearOptions();
         //   resolutions.Clear();
            var m = 0;
            int k = 0;
            List<string> resol = new List<string>();
            foreach (Resolution res in resolutionsAll)
            {
                if (!resol.Contains(res.width + "x" + res.height))
                {
                    resol.Add(res.width + "x" + res.height);
                   // resolutions.Add(res);
                //    Options2.Add(res.width + "x" + res.height));
                    if (Screen.width == res.width && Screen.height == res.height)
                    {
                        k = m;
                    }
                    m++;
                }
            }
            resol.Clear();
           // resolutionSelect.AddOptions(Options2);
            resolutionSelect.Index = k;
            Options2.Clear();
            // qualitySelect.ClearOptions();
            for (int ii = 0; ii < QualitySettings.names.Length; ii++)
            {
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation(QualitySettings.names[ii]));
#else
                Options2.Add(QualitySettings.names[ii]);
#endif
            }
#if AT_I2LOC_PRESET
       Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Custom"));
#else
            Options2.Add("Custom");
#endif
            qualitySelect.Options(Options2);
            if (AtavismSettings.Instance.GetVideoSettings().customSettings)
                qualitySelect.Index = QualitySettings.names.Length;
            else
                qualitySelect.Index = QualitySettings.GetQualityLevel();
            //  List<string> Options = new List<string>();

            if (!AtavismSettings.Instance.GetVideoSettings().customSettings)
            {
                switch (QualitySettings.shadows)
                {
                    case ShadowQuality.Disable:
                        AtavismSettings.Instance.GetVideoSettings().shadows = 0;
                        break;
                    case ShadowQuality.HardOnly:
                        AtavismSettings.Instance.GetVideoSettings().shadows = 1;
                        break;
                    case ShadowQuality.All:
                        AtavismSettings.Instance.GetVideoSettings().shadows = 2;
                        break;
                }

                switch ((int)QualitySettings.shadowDistance)
                {
                    case 50:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 0;
                        break;
                    case 100:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 1;
                        break;
                    case 150:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 2;
                        break;
                    case 300:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 3;
                        break;
                    case 500:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 4;
                        break;
                    default:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 4;
                        break;

                }
                
                switch (QualitySettings.shadowResolution)
                {
                    case ShadowResolution.Low:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 0;
                        break;
                    case ShadowResolution.Medium:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 1;
                        break;
                    case ShadowResolution.High:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 2;
                        break;
                    case ShadowResolution.VeryHigh:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 3;
                        break;
                }
                AtavismSettings.Instance.GetVideoSettings().verticalSync = QualitySettings.vSyncCount;
                if (QualitySettings.lodBias == 2f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 5;
                if (QualitySettings.lodBias == 1.5f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 4;
                if (QualitySettings.lodBias == 1f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 3;
                if (QualitySettings.lodBias == 0.7f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 2;
                if (QualitySettings.lodBias == 0.4f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 1;
                if (QualitySettings.lodBias == 0.3f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 0;


                switch (QualitySettings.particleRaycastBudget)
                {
                    case 4096:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 5;
                        break;
                    case 1024:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 4;
                        break;
                    case 256:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 3;
                        break;
                    case 64:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 2;
                        break;
                    case 16:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 1;
                        break;
                    case 4:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 0;
                        break;
                }

                AtavismSettings.Instance.GetVideoSettings().masterTextureLimit = 3 - QualitySettings.globalTextureMipmapLimit;
                AtavismSettings.Instance.GetVideoSettings().softParticles = QualitySettings.softParticles;
            }


            if (shadowsDropdown != null)
            {
                Options2.Clear();
                // shadowsDropdown.ClearOptions();
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Disable"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("HardOnly"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("All"));
#else
                Options2.Add("Disable");
                Options2.Add("HardOnly");
                Options2.Add("All");
#endif
                shadowsDropdown.Options(Options2);
                shadowsDropdown.Index = AtavismSettings.Instance.GetVideoSettings().shadows;
            }else
            Debug.LogWarning("AtavismSettings.Instance.GetVideoSettings().shadows not found");
            if (shadowDistanceDropdown != null)
            {
                Options2.Clear();
                // shadowDistanceDropdown.ClearOptions();
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryLow"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Low"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Medium"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("High"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryHigh"));
#else
                Options2.Add("VeryLow");
                Options2.Add("Low");
                Options2.Add("Medium");
                Options2.Add("High");
                Options2.Add("VeryHigh");
#endif
                shadowDistanceDropdown.Options(Options2);
                shadowDistanceDropdown.Index = AtavismSettings.Instance.GetVideoSettings().shadowDistance;
            }else
            Debug.LogWarning("AtavismSettings.Instance.GetVideoSettings().shadowDistance not found");

            if (shadowResolutionDropdown != null)
            {
                Options2.Clear();
                // shadowResolutionDropdown.ClearOptions();
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Low"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Medium"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("High"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryHigh"));
#else
                Options2.Add("Low");
                Options2.Add("Medium");
                Options2.Add("High");
                Options2.Add("VeryHigh");
#endif
                shadowResolutionDropdown.Options(Options2);
                shadowResolutionDropdown.Index = AtavismSettings.Instance.GetVideoSettings().shadowResolution;
            }

            else
            {
                Debug.LogWarning("AtavismSettings.Instance.GetVideoSettings().shadowResolution not found");
            }
            verticalSyncToggle.value = AtavismSettings.Instance.GetVideoSettings().verticalSync == 1 ? true : false;

            if (lodBiasDropdown != null)
            {
                Options2.Clear();
                // lodBiasDropdown.ClearOptions();
#if AT_I2LOC_PRESET
           Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryLow"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Low"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Medium"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("High"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryHigh"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("UltraHigh"));
#else
                Options2.Add("VeryLow");
                Options2.Add("Low");
                Options2.Add("Medium");
                Options2.Add("High");
                Options2.Add("VeryHigh");
                Options2.Add("UltraHigh");
#endif
                lodBiasDropdown.Options(Options2);
                lodBiasDropdown.Index = AtavismSettings.Instance.GetVideoSettings().lodBias;
            }
            else
            {
                Debug.LogWarning("AtavismSettings.Instance.GetVideoSettings().lodBias not found");
            }
            if (particleRaycastBudgetDropdown != null)
            {
                Options2.Clear();
                // particleRaycastBudgetDropdown.ClearOptions();
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryLow"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Low"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Medium"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("High"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryHigh"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("UltraHigh"));
#else
                Options2.Add("VeryLow");
                Options2.Add("Low");
                Options2.Add("Medium");
                Options2.Add("High");
                Options2.Add("VeryHigh");
                Options2.Add("UltraHigh");

#endif
                particleRaycastBudgetDropdown.Options(Options2);
                particleRaycastBudgetDropdown.Index = AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget;
            }else
            Debug.LogWarning("AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget not found");


            if (masterTextureLimitDropdown != null)
            {
                Options2.Clear();
                // masterTextureLimitDropdown.ClearOptions();
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Low"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Medium"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("High"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("VeryHigh"));
#else
                Options2.Add("Low");
                Options2.Add("Medium");
                Options2.Add("High");
                Options2.Add("VeryHigh");
#endif
                masterTextureLimitDropdown.Options(Options2);
                masterTextureLimitDropdown.Index = AtavismSettings.Instance.GetVideoSettings().masterTextureLimit;
            }

            if (shadowDistanceDropdown != null)
            {
                Options2.Clear();
                // antialiasingDropdown.ClearOptions();
#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("None"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Fast Approximate"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Subpixel Morphological"));
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation("Temporal"));
#else
                Options2.Add("None");
                Options2.Add("Fast Approximate");
                Options2.Add("Subpixel Morphological");
                Options2.Add("Temporal");

#endif
                antialiasingDropdown.Options(Options2);
                antialiasingDropdown.Index = AtavismSettings.Instance.GetVideoSettings().antialiasing;
            }

            if (ambiertOcclusionsToggle != null)
                ambiertOcclusionsToggle.value = AtavismSettings.Instance.GetVideoSettings().ambientOcclusion;
            if (softParticlesToggle != null)
                softParticlesToggle.value = AtavismSettings.Instance.GetVideoSettings().softParticles;
            // antialiasingToggle.value = AtavismSettings.Instance.GetVideoSettings().antialiasing;
            if (depthOfFieldToggle != null)
                depthOfFieldToggle.value = AtavismSettings.Instance.GetVideoSettings().depthOfField;
            if (vignetteToggle != null)
                vignetteToggle.value = AtavismSettings.Instance.GetVideoSettings().vignette;
            if (bloomToggle != null)
                bloomToggle.value = AtavismSettings.Instance.GetVideoSettings().bloom;
            if (chromaticAberrationToggle != null)
                chromaticAberrationToggle.value = AtavismSettings.Instance.GetVideoSettings().chromaticAberration;
            if (motionBlurToggle != null)
                motionBlurToggle.value = AtavismSettings.Instance.GetVideoSettings().motionBlur;
            if (autoExposureToggle != null)
                autoExposureToggle.value = AtavismSettings.Instance.GetVideoSettings().autoExposure;
            if (colorGradingToggle != null)
                colorGradingToggle.value = AtavismSettings.Instance.GetVideoSettings().colorGrading;
            if (ditheringToggle != null)
                ditheringToggle.value = AtavismSettings.Instance.GetVideoSettings().dithering;
            if (screenSpaceReflectionsToggle != null)
                screenSpaceReflectionsToggle.value = AtavismSettings.Instance.GetVideoSettings().screenSpaceReflections;
            // amplifyOcclusionEffectToggle.value = AtavismSettings.Instance.GetVideoSettings().amplifyOcclusionEffect;
            if (screenSpaceToggle != null)
                screenSpaceToggle.value = AtavismSettings.Instance.GetVideoSettings().seScreenSpaceShadows;
            if (volumetricCameraToggle != null)
                volumetricCameraToggle.value = AtavismSettings.Instance.GetVideoSettings().hxVolumetricCamera;
            if (volumetricFogToggle != null)
                volumetricFogToggle.value = AtavismSettings.Instance.GetVideoSettings().volumetricFog;


        }

        public void ChangeAntialiasing(ChangeEvent<int> evt)
        {
            /*switch(antialiasingSelect.value)
             {
                 case 0:
                     QualitySettings.antiAliasing = 0;
                     break;
                 case 1:
                     QualitySettings.antiAliasing = 2;
                     break;
                 case 2:
                     QualitySettings.antiAliasing = 4;
                     break;
                 case 3:
                     QualitySettings.antiAliasing = 8;
                     break;
             }*/
        }

        public void ChangeFps(ChangeEvent<bool> evt)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetVideoSettings().fps = fpsToggle.value;

        }
        /*   public void ChangeMonitor() {
               if (startTimer > Time.time) return;
               displays[monitorSelect.value].Activate();
           }*/
        float setQualityTime = 0;

        public void ChangeQuality(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (qualitySelect.Index == QualitySettings.names.Length)
            {
                AtavismSettings.Instance.GetVideoSettings().customSettings = true;
            }
            else
            {
                AtavismSettings.Instance.GetVideoSettings().customSettings = false;
                AtavismSettings.Instance.GetVideoSettings().quality = qualitySelect.Index;
                QualitySettings.SetQualityLevel(qualitySelect.Index, true);
                //Apply default values
                AtavismQualitySetingsDefault q = AtavismSettings.Instance.GetDefaultQuality(qualitySelect.Index);
                QualitySettings.pixelLightCount = q.pixelLightCount;
                QualitySettings.globalTextureMipmapLimit = q.masterTextureLimit;
                QualitySettings.anisotropicFiltering = q.anisotropicFiltering;
                QualitySettings.softParticles = q.softParticles;
                QualitySettings.realtimeReflectionProbes = q.realtimeReflectionProbes;
                QualitySettings.billboardsFaceCameraPosition = q.billboardsFaceCameraPosition;
                QualitySettings.shadows = q.shadows;
                QualitySettings.shadowResolution = q.shadowResolution;
                QualitySettings.shadowDistance = q.shadowDistance;
                QualitySettings.shadowCascades = q.shadowCascades;
                QualitySettings.skinWeights = q.blendWeights;
                QualitySettings.vSyncCount = q.verticalSync;
                QualitySettings.lodBias = q.lodBias;
                QualitySettings.particleRaycastBudget = q.particleRaycastBudget;

                setQualityTime = Time.time + .1f;
                switch (QualitySettings.shadows)
                {
                    case ShadowQuality.Disable:
                        AtavismSettings.Instance.GetVideoSettings().shadows = 0;
                        break;
                    case ShadowQuality.HardOnly:
                        AtavismSettings.Instance.GetVideoSettings().shadows = 1;
                        break;
                    case ShadowQuality.All:
                        AtavismSettings.Instance.GetVideoSettings().shadows = 2;
                        break;
                }

                // AtavismSettings.Instance.GetVideoSettings().shadowDistance = (int)QualitySettings.shadowDistance;
                switch ((int)QualitySettings.shadowDistance)
                {
                    case 50:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 0;
                        break;
                    case 100:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 1;
                        break;
                    case 150:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 2;
                        break;
                    case 300:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 3;
                        break;
                    case 500:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 4;
                        break;
                    default:
                        AtavismSettings.Instance.GetVideoSettings().shadowDistance = 4;
                        break;
                }
                
                
                
                switch (QualitySettings.shadowResolution)
                {
                    case ShadowResolution.Low:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 0;
                        break;
                    case ShadowResolution.Medium:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 1;
                        break;
                    case ShadowResolution.High:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 2;
                        break;
                    case ShadowResolution.VeryHigh:
                        AtavismSettings.Instance.GetVideoSettings().shadowResolution = 3;
                        break;
                }
                AtavismSettings.Instance.GetVideoSettings().verticalSync = QualitySettings.vSyncCount;
                if (QualitySettings.lodBias == 2f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 5;
                if (QualitySettings.lodBias == 1.5f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 4;
                if (QualitySettings.lodBias == 1f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 3;
                if (QualitySettings.lodBias == 0.7f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 2;
                if (QualitySettings.lodBias == 0.4f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 1;
                if (QualitySettings.lodBias == 0.3f)
                    AtavismSettings.Instance.GetVideoSettings().lodBias = 0;


                switch (QualitySettings.particleRaycastBudget)
                {
                    case 4096:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 5;
                        break;
                    case 1024:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 4;
                        break;
                    case 256:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 3;
                        break;
                    case 64:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 2;
                        break;
                    case 16:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 1;
                        break;
                    case 4:
                        AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = 0;
                        break;
                }

                AtavismSettings.Instance.GetVideoSettings().masterTextureLimit = 3 - QualitySettings.globalTextureMipmapLimit;
                AtavismSettings.Instance.GetVideoSettings().softParticles = QualitySettings.softParticles;
                updParam();
            }
        }
        public void ChangeShadows(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;

            // AtavismSettings.Instance.GetVideoSettings().customSettings = true;
            if (setQualityTime < Time.time)
            {
                if (qualitySelect != null)
                    qualitySelect.Index = QualitySettings.names.Length;

                AtavismSettings.Instance.GetVideoSettings().shadows = (int)shadowsDropdown.Index;
                switch ((int)shadowsDropdown.Index)
                {
                    case 0:
                        QualitySettings.shadows = ShadowQuality.Disable;
                        break;
                    case 1:
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                        break;
                    case 2:
                        QualitySettings.shadows = ShadowQuality.All;
                        break;
                }
            }
          //  updParam();
        }
        public void ChangeShadowsDist(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().shadowDistance = (int)shadowDistanceDropdown.Index;
                switch ((int)shadowDistanceDropdown.Index)
                {
                    case 0:
                        QualitySettings.shadowDistance = 50;
                        break;
                    case 1:
                        QualitySettings.shadowDistance = 100;
                        break;
                    case 2:
                        QualitySettings.shadowDistance = 150;
                        break;
                    case 3:
                        QualitySettings.shadowDistance = 300;
                        break;
                    case 4:
                        QualitySettings.shadowDistance = 500;
                        break;
                }
            }
           // updParam();
        }
        public void ChangeShadowsRez(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().shadowResolution = (int)shadowResolutionDropdown.Index;
                switch ((int)shadowResolutionDropdown.Index)
                {
                    case 0:
                        QualitySettings.shadowResolution = ShadowResolution.Low;
                        break;
                    case 1:
                        QualitySettings.shadowResolution = ShadowResolution.Medium;
                        break;
                    case 2:
                        QualitySettings.shadowResolution = ShadowResolution.High;
                        break;
                    case 3:
                        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                        break;
                }
            }
          //  updParam();
        }

        public void ChangeSync(ChangeEvent<bool> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().verticalSync = verticalSyncToggle.value ? 1 : 0;
                QualitySettings.vSyncCount = verticalSyncToggle.value ? 1 : 0;
            }
           // updParam();
        }

        public void ChangeLodbias(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().lodBias = (int)lodBiasDropdown.Index;
                switch ((int)lodBiasDropdown.Index)
                {
                    case 0:
                        QualitySettings.lodBias = 0.3f;
                        break;
                    case 1:
                        QualitySettings.lodBias = 0.4f;
                        break;
                    case 2:
                        QualitySettings.lodBias = 0.7f;
                        break;
                    case 3:
                        QualitySettings.lodBias = 1f;
                        break;
                    case 4:
                        QualitySettings.lodBias = 1.5f;
                        break;
                    case 5:
                        QualitySettings.lodBias = 2f;
                        break;
                }
            }
          //  updParam();
        }
        public void ChangeBudget(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().particleRaycastBudget = (int)particleRaycastBudgetDropdown.Index;
                switch ((int)particleRaycastBudgetDropdown.Index)
                {
                    case 0:
                        QualitySettings.particleRaycastBudget = 4;
                        break;
                    case 1:
                        QualitySettings.particleRaycastBudget = 16;
                        break;
                    case 2:
                        QualitySettings.particleRaycastBudget = 64;
                        break;
                    case 3:
                        QualitySettings.particleRaycastBudget = 256;
                        break;
                    case 4:
                        QualitySettings.particleRaycastBudget = 1024;
                        break;
                    case 5:
                        QualitySettings.particleRaycastBudget = 4096;
                        break;
                }
            }
         //   updParam();
        }

        public void ChangeTexture(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().masterTextureLimit = (int)masterTextureLimitDropdown.Index;
                QualitySettings.globalTextureMipmapLimit = 3 - (int)masterTextureLimitDropdown.Index;
            }
          //  updParam();
        }
        public void ChangeSoftParticels(ChangeEvent<bool> evt)
        {
            if (startTimer > Time.time)
                return;
            if (setQualityTime < Time.time)
            {
                qualitySelect.Index = QualitySettings.names.Length;
                AtavismSettings.Instance.GetVideoSettings().softParticles = softParticlesToggle.value;
                QualitySettings.softParticles = softParticlesToggle.value;
            }
           // updParam();
        }

        public void ChangeEffects(ChangeEvent<bool> evt)
        {
            if (startTimer > Time.time)
                return;
            if (depthOfFieldToggle != null)
                AtavismSettings.Instance.GetVideoSettings().depthOfField = depthOfFieldToggle.value;
            if (bloomToggle != null)
                AtavismSettings.Instance.GetVideoSettings().bloom = bloomToggle.value;
            if (vignetteToggle != null)
                AtavismSettings.Instance.GetVideoSettings().vignette = vignetteToggle.value;
            //    AtavismSettings.Instance.GetVideoSettings().amplifyOcclusionEffect = amplifyOcclusionEffectToggle.value;
            if (screenSpaceToggle != null)
                AtavismSettings.Instance.GetVideoSettings().seScreenSpaceShadows = screenSpaceToggle.value;
            if (volumetricFogToggle != null)
                AtavismSettings.Instance.GetVideoSettings().volumetricFog = volumetricFogToggle.value;
            if (volumetricCameraToggle != null)
                AtavismSettings.Instance.GetVideoSettings().hxVolumetricCamera = volumetricCameraToggle.value;
            if (motionBlurToggle != null)
                AtavismSettings.Instance.GetVideoSettings().motionBlur = motionBlurToggle.value;
            if (chromaticAberrationToggle != null)
                AtavismSettings.Instance.GetVideoSettings().chromaticAberration = chromaticAberrationToggle.value;

            if (ambiertOcclusionsToggle != null)
                AtavismSettings.Instance.GetVideoSettings().ambientOcclusion = ambiertOcclusionsToggle.value;
            if (softParticlesToggle != null)
                AtavismSettings.Instance.GetVideoSettings().softParticles = softParticlesToggle.value;
            if (autoExposureToggle != null)
                AtavismSettings.Instance.GetVideoSettings().autoExposure = autoExposureToggle.value;
            if (colorGradingToggle != null)
                AtavismSettings.Instance.GetVideoSettings().colorGrading = colorGradingToggle.value;
            if (ditheringToggle != null)
                AtavismSettings.Instance.GetVideoSettings().dithering = ditheringToggle.value;
            if (screenSpaceReflectionsToggle != null)
                AtavismSettings.Instance.GetVideoSettings().screenSpaceReflections = screenSpaceReflectionsToggle.value;

            if (antialiasingDropdown != null)
                AtavismSettings.Instance.GetVideoSettings().antialiasing = antialiasingDropdown.Index;
            AtavismSettings.Instance.ApplyCamEffect();

        }


        public void ChangeResolution(ChangeEvent<int> evt)
        {
            if (startTimer > Time.time)
                return;
            if (resolutions != null && resolutions.Count >= resolutionSelect.Index)
            {
                Screen.SetResolution(resolutions[resolutionSelect.Index].width, resolutions[resolutionSelect.Index].height, fullscreenToggle.value);
            }
          //  updParam();
        }

        public void ChangeRenderingSampling()
        {
            if (startTimer > Time.time)
                return;

        }

        // Use this for initialization
        void Start()
        {
            updateResolutions();
            updParam();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}