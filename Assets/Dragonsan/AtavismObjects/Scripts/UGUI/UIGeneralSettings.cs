using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Atavism
{
    [Serializable]
    public class LanguageFlag
    {
        public string language = "";
        public Image flagSelectedImage;
    }
    public class UIGeneralSettings : MonoBehaviour
    {
        [SerializeField]
        Toggle freeCamera;

        [SerializeField] List<LanguageFlag> flags = new List<LanguageFlag>();
        [SerializeField]
        Image polishFlag;
        [SerializeField]
        Image englishFlag;
        [SerializeField] Slider sensitivityMouse;
        [SerializeField] Slider sensitivityWheelMouse;
        [SerializeField]
        Toggle showTitle;
        [SerializeField] Toggle invertMouse;
        [SerializeField] TMP_Dropdown autoLootQualitySelect;
        void Start()
        {
            if (invertMouse)
            {
                invertMouse.onValueChanged.AddListener(v =>
                {
                    ChangeIvnertMouse(v);
                });
            }

            if (freeCamera)
            {
                freeCamera.onValueChanged.AddListener(v =>
                {
                    ChangeFreeCamera();
                });
            }
            if (showTitle)
            {
                showTitle.onValueChanged.AddListener(v =>
                {
                    ChangeShowTitle();
                });
            }
            
        }
        void OnEnable()
        {
            updParam();
        }
        
        public void updParam()
        {
            if (freeCamera)
                freeCamera.isOn = AtavismSettings.Instance.GetGeneralSettings().freeCamera;
            if (showTitle) showTitle.isOn = AtavismSettings.Instance.GetGeneralSettings().showTitle;
            updateFlags();
            if (sensitivityMouse != null)
                sensitivityMouse.value = AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse;
            if (sensitivityWheelMouse != null)
                sensitivityWheelMouse.value = AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse;
            if (invertMouse)
                invertMouse.isOn = AtavismSettings.Instance.GetGeneralSettings().invertMouse;
            
            
            if (autoLootQualitySelect != null)
            {
                List<TMP_Dropdown.OptionData> Options2 = new List<TMP_Dropdown.OptionData>();
                autoLootQualitySelect.ClearOptions();
                foreach (var quality in AtavismSettings.Instance.qualityNames)
                {


#if AT_I2LOC_PRESET
            Options2.Add(new TMP_Dropdown.OptionData(I2.Loc.LocalizationManager.GetTranslation(quality.Value)));
#else
                    Options2.Add(new TMP_Dropdown.OptionData(quality.Value));
#endif
                }

                autoLootQualitySelect.AddOptions(Options2);
                autoLootQualitySelect.value = AtavismSettings.Instance.GetGeneralSettings().autoLootGroundMinQuality-1;
                autoLootQualitySelect.onValueChanged.AddListener( ChangeAutoLootQuality);
            }

        }

        public void ChangeAutoLootQuality(int val)
        {
            AtavismSettings.Instance.GetGeneralSettings().autoLootGroundMinQuality = val + 1;
        }

        // Use this for initialization
        public void ChangeFreeCamera()
        {
            AtavismSettings.Instance.GetGeneralSettings().freeCamera = freeCamera.isOn;
        }

        public void ChangeShowTitle()
        {
            AtavismSettings.Instance.GetGeneralSettings().showTitle = showTitle.isOn;
        }

        void ChangeIvnertMouse(bool v)
        {
            AtavismSettings.Instance.GetGeneralSettings().invertMouse = v;
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
            foreach (var flag in flags)
            {
                if (I2.Loc.LocalizationManager.CurrentLanguage == flag.language) { if (flag.flagSelectedImage!=null) flag.flagSelectedImage.enabled = true; } else { if (flag.flagSelectedImage != null) flag.flagSelectedImage.enabled = false; }
            }


      //  if (I2.Loc.LocalizationManager.CurrentLanguage == "Polish") { if (polishFlag!=null) polishFlag.enabled = true; } else { if (polishFlag != null) polishFlag.enabled = false; }
      //  if (I2.Loc.LocalizationManager.CurrentLanguage == "English") { if (englishFlag != null) englishFlag.enabled = true; } else { if (englishFlag != null) englishFlag.enabled = false; }
#endif
        }
        public void ResetWindows()
        {
            AtavismSettings.Instance.ResetWindows();

        }
        public void SetSensitivityMouse(float v)
        {
            AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse = v;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MOUSE_SENSITIVE", args);
        }
        public void SetSensitivityWheelMouse(float v)
        {
            AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse = v;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MOUSE_SENSITIVE", args);
        }
    }
}