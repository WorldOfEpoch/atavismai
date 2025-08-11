using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Atavism
{
    
    public class UGUIControls : MonoBehaviour
    {
        [AtavismSeparator("Key Settings")] 
        [SerializeField] UGUIKeySettingsEntry uiKeyPrefab;

        [SerializeField] private RectTransform controlsGrid;
        [SerializeField] private RectTransform windowsGrid;
        [SerializeField] List<UGUIKeySettingsEntry> controls = new List<UGUIKeySettingsEntry>();
        [SerializeField] List<UGUIKeySettingsEntry> windows = new List<UGUIKeySettingsEntry>();
        [SerializeField] private TMP_Dropdown dodgeOptionDropdown;
        [SerializeField] GameObject changeInfoPanel;

        string currentKey = "";

        private bool altKey = false;
        // Use this for initialization
        void Start()
        {
            if (dodgeOptionDropdown)
            {
                dodgeOptionDropdown.value = AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap ? 0 : 1;
            }

            foreach (FieldInfo p in typeof(AtavismKeySettings).GetFields())
            {
                AtavismKeyDefinition akd = p.GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
                if (akd != null && akd.type == KeyControlType.Movement && akd.show)
                {
                    GameObject go = Instantiate(uiKeyPrefab.gameObject, controlsGrid);
                    UGUIKeySettingsEntry e = go.GetComponent<UGUIKeySettingsEntry>();
                    go.name = p.Name;
                    e.def = akd;
                    if (akd.canChange)
                    {
                        if (e.button)
                            e.button.onClick.AddListener(() => ChangeKey(p.Name));
                        if (e.altButton)
                            e.altButton.onClick.AddListener(() => ChangeAltKey(p.Name));
                    }
                    else
                    {
                        if (e.button)
                            e.button.interactable = false;
                        if (e.altButton)
                            e.altButton.interactable = false;
                    }
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(p.Name) != null ?I2.Loc.LocalizationManager.GetTranslation(p.Name):p.Name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = I2.Loc.LocalizationManager.GetTranslation(p.Name) != null ?I2.Loc.LocalizationManager.GetTranslation(p.Name):p.Name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = p.Name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = p.Name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif
                    if (p.Name.Equals("dodge") && AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap)
                    {
                        go.SetActive(false);
                    }

                    controls.Add(e);
                }
                else if (akd != null && akd.type == KeyControlType.Window && akd.show)
                {
                    GameObject go = Instantiate(uiKeyPrefab.gameObject, windowsGrid);
                    UGUIKeySettingsEntry e = go.GetComponent<UGUIKeySettingsEntry>();
                    go.name = p.Name;
                    e.def = akd;
                    if (akd.canChange)
                    {
                        if (e.button)
                            e.button.onClick.AddListener(() => ChangeKey(p.Name));
                        if (e.altButton)
                            e.altButton.onClick.AddListener(() => ChangeAltKey(p.Name));
                    }
                    else
                    {
                        if (e.button)
                            e.button.interactable = false;
                        if (e.altButton)
                            e.altButton.interactable = false;
                    }
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(p.Name) != null ?I2.Loc.LocalizationManager.GetTranslation(p.Name):p.Name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = I2.Loc.LocalizationManager.GetTranslation(p.Name) != null ?I2.Loc.LocalizationManager.GetTranslation(p.Name):p.Name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = p.Name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = p.Name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif
                    windows.Add(e);
                }
            }

            foreach (var akd in AtavismSettings.Instance.GetKeySettings().additionalActions)
            {
                
                //AtavismKeyDefinition akd = p.GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
                if (akd != null && akd.type == KeyControlType.Movement && akd.show)
                {
                    GameObject go = Instantiate(uiKeyPrefab.gameObject, controlsGrid);
                    UGUIKeySettingsEntry e = go.GetComponent<UGUIKeySettingsEntry>();
                    go.name = akd.name;
                    e.def = akd;
                    if (akd.canChange)
                    {
                        if (e.button)
                            e.button.onClick.AddListener(() => ChangeKey(akd.name));
                        if (e.altButton)
                            e.altButton.onClick.AddListener(() => ChangeAltKey(akd.name));
                    }
                    else
                    {
                        if (e.button)
                            e.button.interactable = false;
                        if (e.altButton)
                            e.altButton.interactable = false;
                    }
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(akd.name) != null ?I2.Loc.LocalizationManager.GetTranslation(akd.name):akd.name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = I2.Loc.LocalizationManager.GetTranslation(akd.name) != null ?I2.Loc.LocalizationManager.GetTranslation(akd.name):akd.name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = akd.name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = akd.name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif
                  
                    controls.Add(e);
                }



            }

            AtavismEventSystem.RegisterEvent("KEY_UPDATE_VIEW", this);
        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("KEY_UPDATE_VIEW", this);

        }
        // Update is called once per frame
        void Update()
        {

        }
        void OnEnable()
        {
            UpdateViewKeys();
        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "KEY_UPDATE_VIEW")
            {
                UpdateViewKeys();
            }
        }

        public void UpdateViewKeys()
        {
            if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetKeySettings() != null)
            {
                foreach (UGUIKeySettingsEntry e in controls)
                {
                    if (e.name.Equals("dodge"))
                    {
                        if (AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap)
                        {
                            e.gameObject.SetActive(false);
                        }
                        else
                        {
                            e.gameObject.SetActive(true);
                        }
                    }
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(e.name) != null ?I2.Loc.LocalizationManager.GetTranslation(e.name):e.name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = I2.Loc.LocalizationManager.GetTranslation(e.name) != null ?I2.Loc.LocalizationManager.GetTranslation(e.name):e.name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                     if (e.label != null)
                         e.label.text = e.name;
                     if (e.TMPLabel != null)
                         e.TMPLabel.text = e.name;
                     if (e.buttonText != null)
                         e.buttonText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                     if (e.buttonTMPText != null)
                         e.buttonTMPText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                     if (e.altButtonText != null)
                         e.altButtonText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
                     if (e.altButtonTMPText != null)
                         e.altButtonTMPText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
#endif
                }

                foreach (UGUIKeySettingsEntry e in windows)
                {
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(e.name) != null ? I2.Loc.LocalizationManager.GetTranslation(e.name) : e.name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = I2.Loc.LocalizationManager.GetTranslation(e.name) != null ? I2.Loc.LocalizationManager.GetTranslation(e.name) : e.name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = e.name;
                    if (e.TMPLabel != null)
                        e.TMPLabel.text = e.name;
                    if (e.buttonText != null)
                        e.buttonText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.buttonTMPText != null)
                        e.buttonTMPText.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButtonText != null)
                        e.altButtonText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
                    if (e.altButtonTMPText != null)
                        e.altButtonTMPText.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif

                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(controlsGrid);
                LayoutRebuilder.ForceRebuildLayoutImmediate(windowsGrid);
            }
        }

        private bool duplicateMessage = false;
        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(currentKey) && !duplicateMessage)
            {

                Event e = Event.current;
                
                if ((e.isKey && e.keyCode!=KeyCode.None)|| (e.isMouse ))
                {
                    Debug.Log("Detected character:  K="+e.isKey+" M="+e.isMouse+" KC="+e.keyCode+" MB="+ e.button);

                    KeyCode k = KeyCode.None;
                    if (e.isMouse )
                    {
                        switch (e.button)
                        {
                            case 0:
                                k = KeyCode.Mouse0;
                                break;
                            case 1:
                                k = KeyCode.Mouse1;
                                break;
                            case 2:
                                k = KeyCode.Mouse2;
                                break;
                            case 3:
                                k = KeyCode.Mouse3;
                                break;
                            case 4:
                                k = KeyCode.Mouse4;
                                break;
                            case 5:
                                k = KeyCode.Mouse5;
                                break;
                            case 6:
                                k = KeyCode.Mouse6;
                                break;
                        }
                    }
                    
                    bool found = false;
                    foreach (FieldInfo p in typeof(AtavismKeySettings).GetFields())
                    {
                        AtavismKeyDefinition akd = p.GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
                        if (akd != null)
                        {
                            if (!p.Name.Equals(currentKey) && (e.isKey && (akd.key == e.keyCode || akd.altKey == e.keyCode) || (e.isMouse  &&(akd.key == k || akd.altKey == k))))
                            {
                                found = true;
                            } else if (p.Name.Equals(currentKey))
                            {
                                if (( e.isKey  && ((altKey && akd.key == e.keyCode) || (!altKey && akd.altKey == e.keyCode)))|| (e.isMouse&& ((altKey && akd.key == k) || (!altKey && akd.altKey == k))))
                                {
                                    found = true;
                                }
                            }                     
                        }
                    }

                    foreach (var akd in AtavismSettings.Instance.GetKeySettings().additionalActions)
                    {
                        if (akd != null)
                        {
                            if (!akd.name.Equals(currentKey) && (e.isKey && (akd.key == e.keyCode || akd.altKey == e.keyCode) || (e.isMouse  &&(akd.key == k || akd.altKey == k))))
                            {
                                found = true;
                            } else if (akd.name.Equals(currentKey))
                            {
                                if (( e.isKey  && ((altKey && akd.key == e.keyCode) || (!altKey && akd.altKey == e.keyCode)))|| (e.isMouse&& ((altKey && akd.key == k) || (!altKey && akd.altKey == k))))
                                {
                                    found = true;
                                }
                            }                     
                        }
                    }

                    

                    if (found)
                    {
                        duplicateMessage = true;
#if AT_I2LOC_PRESET
                        string confirmationString = I2.Loc.LocalizationManager.GetTranslation("KeyConfirmText") ;
#else
                        string confirmationString = "The selected key {0} is already assigned. Are you sure you want to continue?";
#endif
                        UGUIConfirmationPanel.Instance.ShowConfirmationBox(String.Format(confirmationString, e.keyCode.ToString() ),Confirmed, (e.isKey?e.keyCode:k), e.shift, e.alt, e.control);
                    } else if(e.keyCode == KeyCode.Escape)
                    {
                        
                        setKey(KeyCode.None,false, false, false);
                    }
                    else if((e.isKey &&e.keyCode != KeyCode.None )|| (e.isMouse))
                    {
                        setKey((e.isKey?e.keyCode:k), e.shift, e.alt, e.control);
                    }
                }
            }
        }

        public void Confirmed(object[] obj, bool response)
        {
            duplicateMessage = false;
            if (!response)
                return;

            KeyCode keyCode = (KeyCode)obj[0];
            bool shift = (bool)obj[1];
            bool alt = (bool)obj[2];
            bool control = (bool)obj[3];
            setKey(keyCode, shift, alt, control);
        }

        void setKey(KeyCode keyCode, bool shift, bool alt, bool control)
        {
            foreach (FieldInfo p in typeof(AtavismKeySettings).GetFields())
            {

                AtavismKeyDefinition akd = p.GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
                if (akd != null && akd.key == keyCode)
                {
                    akd.key = KeyCode.None;
                }

                if (akd != null && akd.altKey == keyCode)
                {
                    akd.altKey = KeyCode.None;
                }
            }

            foreach (var akd in AtavismSettings.Instance.GetKeySettings().additionalActions)
            {
                if (akd != null && akd.key == keyCode)
                {
                    akd.key = KeyCode.None;
                }

                if (akd != null && akd.altKey == keyCode)
                {
                    akd.altKey = KeyCode.None;
                }
            }

            AtavismKeyDefinition _akd = AtavismSettings.Instance.GetKeySettings().AdditionalActions(currentKey);
            if (_akd == null)
                _akd = typeof(AtavismKeySettings).GetField(currentKey).GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;

            if (_akd != null)
            {
                if (!altKey)
                {
                    _akd.key = keyCode;
                    if (_akd.defControl) _akd.useKeyControl = control;
                    if (_akd.defAlt) _akd.useKeyAlt = alt;
                    if (_akd.defShift) _akd.useKeyShift = shift;
                }

                else
                {
                    _akd.altKey = keyCode;
                    if (_akd.defControl) _akd.useAltKeyControl = control;
                    if (_akd.defAlt) _akd.useAltKeyAlt = alt;
                    if (_akd.defShift) _akd.useAltKeyShift = shift;
                }
            }

           
            currentKey = "";
            UpdateViewKeys();
            StartCoroutine(restore());
        }

        IEnumerator restore()
        {
            WaitForSeconds delay = new WaitForSeconds(0.1f);
            yield return delay;
            if (changeInfoPanel)
                changeInfoPanel.SetActive(false);
            string[] cArgs = new string[1];
            cArgs[0] = "N";
            AtavismEventSystem.DispatchEvent("CHANGE_KEY",cArgs);

            foreach (var c in controls)
            {
                if (c.button != null)
                    c.button.interactable = true;
                if (c.altButton != null)
                    c.altButton.interactable = true;
            }

            foreach (var w in windows)
            {
                if (w.button != null)
                    w.button.interactable = true;
                if (w.altButton != null)
                    w.altButton.interactable = true;
            }
        }
        

        public void ChangeKey(string s)
        {
            currentKey = s;
            altKey = false;
            string[] cArgs = new string[1];
            cArgs[0] = "T";
            AtavismEventSystem.DispatchEvent("CHANGE_KEY",cArgs);
           // AtavismKeyDefinition akd = typeof(AtavismKeySettings).GetField(s).GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
            if (changeInfoPanel)
                changeInfoPanel.SetActive(true);
            foreach (var c in controls)
            {
                if (c.button != null)
                    c.button.interactable = false;
                if (c.altButton != null)
                    c.altButton.interactable = false;
            }

            foreach (var w in windows)
            {
                if (w.button != null)
                    w.button.interactable = false;
                if (w.altButton != null)
                    w.altButton.interactable = false;
            }
        }
        public void ChangeAltKey(string s)
        {
            currentKey = s;
            altKey = true;
            string[] cArgs = new string[1];
            cArgs[0] = "T";
            AtavismEventSystem.DispatchEvent("CHANGE_KEY",cArgs);
            //AtavismKeyDefinition akd = typeof(AtavismKeySettings).GetField(s).GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
            if (changeInfoPanel)
                changeInfoPanel.SetActive(true);

            foreach (var c in controls)
            {
                if (c.button != null)
                    c.button.interactable = false;
                if (c.altButton != null)
                    c.altButton.interactable = false;
            }

            foreach (var w in windows)
            {
                if (w.button != null)
                    w.button.interactable = false;
                if (w.altButton != null)
                    w.altButton.interactable = false;
            }
        }

        public void ChangeDodgeOption(int value)
        {
            AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap = value == 0;
            UpdateViewKeys();
        }
    }
}