using System.Collections.Generic;
using Atavism.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismGeneralSettings //: MonoBehaviour
    {
        public UIDocument uiDocument;
        private VisualElement rootElement;
        private VisualElement screen;
        private Toggle freeCamera;
        private Toggle showTitle;
        private Image polishFlagImage;
        private Image englishFlagImage;
        private Slider sensitivityMouseSlider;
        private Slider sensitivityWheelMouseSlider;
        [SerializeField] Toggle invertMouse;
        [SerializeField] UIDropdown autoLootQualitySelect;
        private MonoBehaviour monoBehaviour;
        public void Setup(VisualElement tabGeneral, VisualElement screen, MonoBehaviour monoBehaviour)
        {
            this.screen = screen;
            this.monoBehaviour = monoBehaviour;
            rootElement = tabGeneral;
            InitializeUI();
        }
        void OnEnable()
        {
            rootElement = uiDocument.rootVisualElement;
            InitializeUI();
            UpdateParam();
        }

        void InitializeUI()
        {
            freeCamera = rootElement.Q<Toggle>("free-camera-toggle");
            freeCamera.RegisterValueChangedCallback(ChangeFreeCamera);
            invertMouse = rootElement.Q<Toggle>("invert-mouse-toggle");
            invertMouse.RegisterValueChangedCallback(ChangeInvertMouse);
            showTitle = rootElement.Q<Toggle>("show-title-toggle");
            showTitle.RegisterValueChangedCallback(ChangeShowTitle);
            polishFlagImage = rootElement.Q<Image>("PolishFlagImage");
            englishFlagImage = rootElement.Q<Image>("EnglishFlagImage");
            sensitivityMouseSlider = rootElement.Q<Slider>("sensitivity-mouse-slider");
            sensitivityMouseSlider.RegisterValueChangedCallback(SetSensitivityMouse);
            sensitivityWheelMouseSlider = rootElement.Q<Slider>("sensitivity-wheel-mouse-slider");
            sensitivityWheelMouseSlider.RegisterValueChangedCallback(SetSensitivityWheelMouse);
            autoLootQualitySelect = rootElement.Q<UIDropdown>("item-quality");
            autoLootQualitySelect.Screen = screen;
            autoLootQualitySelect.RegisterCallback<ChangeEvent<int>>(ChangeAutoLootQuality);
            
        }

        public void UpdateParam()
        {
            if (freeCamera !=null )
                freeCamera.value = AtavismSettings.Instance.GetGeneralSettings().freeCamera;
            if (showTitle !=null ) showTitle.value = AtavismSettings.Instance.GetGeneralSettings().showTitle;
            updateFlags();
            if (sensitivityMouseSlider != null)
                sensitivityMouseSlider.value = AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse;
            if (sensitivityWheelMouseSlider != null)
                sensitivityWheelMouseSlider.value = AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse;
            if (invertMouse != null)
                invertMouse.value = AtavismSettings.Instance.GetGeneralSettings().invertMouse;
            
            if (autoLootQualitySelect != null)
            {
                List<string> Options2 = new List<string>();
                foreach (var quality in AtavismSettings.Instance.qualityNames)
                {


#if AT_I2LOC_PRESET
            Options2.Add(I2.Loc.LocalizationManager.GetTranslation(quality.Value));
#else
                    Options2.Add(quality.Value);
#endif
                }

                autoLootQualitySelect.Options(Options2);
                autoLootQualitySelect.Index = AtavismSettings.Instance.GetGeneralSettings().autoLootGroundMinQuality-1;
             
                //autoLootQualitySelect.onValueChanged.AddListener( ChangeAutoLootQuality);
            }
            //      I2.Loc.LocalizationManager.CurrentLanguage
        }
        // Use this for initialization
        public void ChangeFreeCamera(ChangeEvent<bool> evt)
        {
            AtavismSettings.Instance.GetGeneralSettings().freeCamera = freeCamera.value;
        }

        void ChangeInvertMouse(ChangeEvent<bool> evt)
        {
            AtavismSettings.Instance.GetGeneralSettings().invertMouse = evt.newValue;
        }
        public void ChangeShowTitle(ChangeEvent<bool> evt)
        {
            AtavismSettings.Instance.GetGeneralSettings().showTitle = showTitle.value;
        }
        public void ChangeAutoLootQuality(ChangeEvent<int> evt)
        {
            AtavismSettings.Instance.GetGeneralSettings().autoLootGroundMinQuality = evt.newValue + 1;
        }


        public void SetLanguage(string _lang)
        {
#if AT_I2LOC_PRESET
        if (I2.Loc.LocalizationManager.HasLanguage(_lang)) {
            I2.Loc.LocalizationManager.CurrentLanguage = _lang;
            AtavismSettings.Instance.GetGeneralSettings().language = _lang;
        }
        string[] args = new string[1];
        AtavismEventSystem.DispatchEvent("UPDATE_LANGUAGE", args);
        AtavismSettings.Instance.GetGeneralSettings().language = I2.Loc.LocalizationManager.CurrentLanguage;
        updateFlags();
#endif
        }
        // Update is called once per frame
        void updateFlags()
        {
#if AT_I2LOC_PRESET

        if (I2.Loc.LocalizationManager.CurrentLanguage == "Polish") { if (polishFlagImage!=null) polishFlagImage.visible = true; } else { if (polishFlagImage != null) polishFlagImage.visible = false; }
        if (I2.Loc.LocalizationManager.CurrentLanguage == "English") { if (englishFlagImage != null) englishFlagImage.visible = true; } else { if (englishFlagImage != null) englishFlagImage.visible = false; }
#endif
        }
        public void ResetWindows()
        {
            AtavismSettings.Instance.ResetWindows();

        }
        public void SetSensitivityMouse(ChangeEvent<float> evt)
        {
            AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse = evt.newValue;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MOUSE_SENSITIVE", args);
        }
        public void SetSensitivityWheelMouse(ChangeEvent<float> evt)
        {
            AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse = evt.newValue;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MOUSE_SENSITIVE", args);
        }

       
    }
}