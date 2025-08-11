using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text.RegularExpressions;
using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    
    public class UIAtavismControls 
    {
        [SerializeField] VisualTreeAsset uiKeyTemplate;

        [SerializeField] private VisualElement controlsGrid;
        [SerializeField] private VisualElement windowsGrid;
        [SerializeField] List<UIAtavismKeySettingsEntry> controls = new List<UIAtavismKeySettingsEntry>();
        [SerializeField] List<UIAtavismKeySettingsEntry> windows = new List<UIAtavismKeySettingsEntry>();
        [SerializeField] private UIDropdown dodgeOptionDropdown;
        [SerializeField] VisualElement changeInfoPanel;
        [SerializeField] VisualElement dodgeElement;
        private VisualElement screen;

        string currentKey = "";

        private bool altKey = false;

        private MonoBehaviour monoBehaviour;
        // Use this for initialization

        
       public  void Setup(VisualElement visualElement, VisualTreeAsset template, VisualElement Screen, MonoBehaviour monoBehaviour)
       {
           this.screen = Screen;
            this.monoBehaviour = monoBehaviour;
            this.uiKeyTemplate = template;
            controlsGrid = visualElement.Q<VisualElement>("controls-grid");
            controlsGrid.Clear();
            windowsGrid = visualElement.Q<VisualElement>("windows-grid");
            windowsGrid.Clear();
            changeInfoPanel = visualElement.Q<VisualElement>("change-info-panel");
            dodgeElement = visualElement.Q<VisualElement>("dodge-element");
            dodgeOptionDropdown = visualElement.Q<UIDropdown>("dodge-type");
            dodgeOptionDropdown.RegisterCallback<ChangeEvent<int>>(ChangeDodgeOption);
            dodgeOptionDropdown.Screen = Screen;
          
            Start();
            
        }

      

        void Start()
        {
            changeInfoPanel.HideVisualElement();
            if (dodgeOptionDropdown!=null)
            {
                dodgeOptionDropdown.Index = AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap ? 0 : 1;
            }

            foreach (FieldInfo p in typeof(AtavismKeySettings).GetFields())
            {
                AtavismKeyDefinition akd = p.GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
                if (akd != null && akd.type == KeyControlType.Movement && akd.show)
                {
                    // GameObject go = Instantiate(uiKeyPrefab.gameObject, controlsGrid);
                    // UIAtavismKeySettingsEntry e = go.GetComponent<UIAtavismKeySettingsEntry>();
                    UIAtavismKeySettingsEntry  e = new UIAtavismKeySettingsEntry();
                    // Instantiate the UXML template for the entry
                    var newListEntry = uiKeyTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = e;
                    // Initialize the controller script
                    e.SetVisualElement(newListEntry);
                    controls.Add(e);
                    controlsGrid.Add(newListEntry);
                    e.name = p.Name;
                    e.def = akd;
                    if (akd.canChange)
                    {
                        if (e.button!=null)
                            e.button.clicked +=() => ChangeKey(p.Name);
                        if (e.altButton!=null)
                            e.altButton.clicked +=() => ChangeAltKey(p.Name);
                    }
                    else
                    {
                        if (e.button!=null)
                            e.button.SetEnabled(false);
                        if (e.altButton!=null)
                            e.altButton.SetEnabled(false);
                    }
                    var newName = Regex.Replace(p.Name, "([a-z])([A-Z])", "$1 $2");
                    newName = newName[0].ToString().ToUpper() + newName.Substring(1);

#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(newName) != null ?I2.Loc.LocalizationManager.GetTranslation(newName):newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif
                    if (p.Name.Equals("dodge") && AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap)
                    {
                        e.Hide();
                    }

                    controls.Add(e);
                }
                else if (akd != null && akd.type == KeyControlType.Window && akd.show)
                {
                    UIAtavismKeySettingsEntry  e = new UIAtavismKeySettingsEntry();
                    // Instantiate the UXML template for the entry
                    var newListEntry = uiKeyTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = e;
                    // Initialize the controller script
                    e.SetVisualElement(newListEntry);
                    windows.Add(e);
                    windowsGrid.Add(newListEntry);
                    e.name = p.Name;
                    e.def = akd;
                    if (akd.canChange)
                    {
                        if (e.button!= null)
                            e.button.clicked +=() => ChangeKey(p.Name);
                        if (e.altButton!= null)
                            e.altButton.clicked +=() => ChangeAltKey(p.Name);
                    }
                    else
                    {
                        if (e.button!= null)
                            e.button.SetEnabled(false);
                        if (e.altButton!= null)
                            e.altButton.SetEnabled(false);
                    }
                    var newName = Regex.Replace(p.Name, "([a-z])([A-Z])", "$1 $2");
                    newName = newName[0].ToString().ToUpper() + newName.Substring(1);

#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(newName) != null ?I2.Loc.LocalizationManager.GetTranslation(newName):newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif
                }
            }

            foreach (var akd in AtavismSettings.Instance.GetKeySettings().additionalActions)
            {
                
                //AtavismKeyDefinition akd = p.GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
                if (akd != null && akd.type == KeyControlType.Movement && akd.show)
                {
                    UIAtavismKeySettingsEntry  e = new UIAtavismKeySettingsEntry();
                    // Instantiate the UXML template for the entry
                    var newListEntry = uiKeyTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = e;
                    // Initialize the controller script
                    e.SetVisualElement(newListEntry);
                    controls.Add(e);
                    controlsGrid.Add(newListEntry);
                    e.name = akd.name;
                    e.def = akd;
                    if (akd.canChange)
                    {
                        if (e.button!= null)
                            e.button.clicked +=() => ChangeKey(akd.name);
                        if (e.altButton!= null)
                            e.altButton.clicked +=() => ChangeAltKey(akd.name);
                    }
                    else
                    {
                        if (e.button!= null)
                            e.button.SetEnabled(false);
                        if (e.altButton!= null)
                            e.altButton.SetEnabled(false);
                    }
                    var newName = Regex.Replace(akd.name, "([a-z])([A-Z])", "$1 $2");
                    newName = newName[0].ToString().ToUpper() + newName.Substring(1);
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(newName) != null ?I2.Loc.LocalizationManager.GetTranslation(newName):newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif
                  
                    controls.Add(e);
                }



            }

            AtavismEventSystem.RegisterEvent("KEY_UPDATE_VIEW", OnEvent);
        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("KEY_UPDATE_VIEW", OnEvent);

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
                foreach (UIAtavismKeySettingsEntry e in controls)
                {
                    if (e.name.Equals("dodge"))
                    {
                        if (AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap)
                        {
                            e.Hide();
                        }
                        else
                        {
                            e.Show();
                        }
                    }
                    var newName = Regex.Replace(e.name, "([a-z])([A-Z])", "$1 $2");
                    newName = newName[0].ToString().ToUpper() + newName.Substring(1);
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(newName) != null ?I2.Loc.LocalizationManager.GetTranslation(newName):newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                     if (e.label != null)
                         e.label.text = newName;
                     if (e.button != null)
                         e.button.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                     if (e.altButton != null)
                         e.altButton.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();
#endif
                }

                foreach (UIAtavismKeySettingsEntry e in windows)
                {
                    var newName = Regex.Replace(e.name, "([a-z])([A-Z])", "$1 $2");
                    newName = newName[0].ToString().ToUpper() + newName.Substring(1);
#if AT_I2LOC_PRESET
                    if (e.label != null)
                        e.label.text = I2.Loc.LocalizationManager.GetTranslation(newName) != null ? I2.Loc.LocalizationManager.GetTranslation(newName) : newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.key.ToString().ToUpper())
                                : e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? I2.Loc.LocalizationManager.GetTranslation("CTRL") + " " : "") + (e.def.useAltKeyShift ? I2.Loc.LocalizationManager.GetTranslation("SHIFT") + " " : "") +
                            (e.def.useAltKeyAlt ? I2.Loc.LocalizationManager.GetTranslation("ALT") + " " : "") +
                            I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper()) != null
                                ? I2.Loc.LocalizationManager.GetTranslation(e.def.altKey.ToString().ToUpper())
                                : e.def.altKey.ToString().ToUpper();
#else
                    if (e.label != null)
                        e.label.text = newName;
                    if (e.button != null)
                        e.button.text = (e.def.useKeyControl ? "CTRL" + " " : "") + (e.def.useKeyShift ? "SHIFT" + " " : "") + (e.def.useKeyAlt ? "ALT" + " " : "") + e.def.key.ToString().ToUpper();
                    if (e.altButton != null)
                        e.altButton.text = (e.def.useAltKeyControl ? "CTRL" + " " : "") + (e.def.useAltKeyShift ? "SHIFT" + " " : "") + (e.def.useAltKeyAlt ? "ALT" + " " : "") + e.def.altKey.ToString().ToUpper();

#endif

                }
                // LayoutRebuilder.ForceRebuildLayoutImmediate(controlsGrid);
                // LayoutRebuilder.ForceRebuildLayoutImmediate(windowsGrid);
            }
        }

        private bool duplicateMessage = false;
        
        private void onMouseDownEvent(PointerDownEvent evt)
        {
            KeyCheck(true, evt.button, false, false,false,false, KeyCode.None);
        }

        public void onKeyDownEvent(KeyDownEvent evt)
        {
         //   Debug.Log("UITK event: type=" + evt.GetType() + ", keyCode=" + evt.keyCode + ", character=" +
            //          (evt.character == 0 ? "0" : "" + evt.character) + ", modifiers=" + evt.modifiers);
            if(evt.keyCode != KeyCode.None)
                KeyCheck(false, 0, evt.shiftKey, evt.ctrlKey, evt.altKey, true, evt.keyCode);
        }
       public void KeyCheck(bool isMouse,int button, bool isShift, bool isCtrl, bool isAlt,bool isKey, KeyCode keyCode){

    
            
            if (!string.IsNullOrEmpty(currentKey) && !duplicateMessage)
            {

                // Event e = Event.current; 
                    
                
                if ((isKey && keyCode!=KeyCode.None)|| (isMouse ))
                {
                  //  Debug.Log("Detected character:  K="+isKey+" M="+isMouse+" KC="+keyCode+" MB="+ button);

                    KeyCode k = KeyCode.None;
                    if (isMouse )
                    {
                        switch (button)
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
                            if (!p.Name.Equals(currentKey) && (isKey && (akd.key == keyCode || akd.altKey == keyCode) || (isMouse  &&(akd.key == k || akd.altKey == k))))
                            {
                                found = true;
                            } else if (p.Name.Equals(currentKey))
                            {
                                if (( isKey  && ((altKey && akd.key == keyCode) || (!altKey && akd.altKey == keyCode)))|| (isMouse&& ((altKey && akd.key == k) || (!altKey && akd.altKey == k))))
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
                            if (!akd.name.Equals(currentKey) && (isKey && (akd.key == keyCode || akd.altKey == keyCode) || (isMouse  &&(akd.key == k || akd.altKey == k))))
                            {
                                found = true;
                            } else if (akd.name.Equals(currentKey))
                            {
                                if (( isKey  && ((altKey && akd.key == keyCode) || (!altKey && akd.altKey == keyCode)))|| (isMouse&& ((altKey && akd.key == k) || (!altKey && akd.altKey == k))))
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
                        UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(String.Format(confirmationString, keyCode.ToString() ),Confirmed, (isKey?keyCode:k), isShift, isAlt, isCtrl);
                    } else if(keyCode == KeyCode.Escape)
                    {
                        
                        setKey(KeyCode.None,false, false, false);
                    }
                    else if((isKey &&keyCode != KeyCode.None )|| (isMouse))
                    {
                        setKey((isKey?keyCode:k), isShift, isAlt, isCtrl);
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
            monoBehaviour.StartCoroutine(restore());
        }

        IEnumerator restore()
        {
            WaitForSeconds delay = new WaitForSeconds(0.1f);
            yield return delay;
            if (changeInfoPanel!=null)
                changeInfoPanel.HideVisualElement();
            string[] cArgs = new string[1];
            cArgs[0] = "N";
            AtavismEventSystem.DispatchEvent("CHANGE_KEY",cArgs);

            foreach (var c in controls)
            {
                if (c.button != null)
                    c.button.SetEnabled(true);
                if (c.altButton != null)
                    c.altButton.SetEnabled(true);
            }

            foreach (var w in windows)
            {
                if (w.button != null)
                    w.button.SetEnabled(true);
                if (w.altButton != null)
                    w.altButton.SetEnabled(true);
            }

            // screen.pickingMode = PickingMode.Ignore;
            // screen.UnregisterCallback<KeyDownEvent>(onKeyDownEvent, TrickleDown.TrickleDown);
            // screen.UnregisterCallback<PointerDownEvent>(onMouseDownEvent, TrickleDown.TrickleDown);
            // changeInfoPanel.UnregisterCallback<KeyDownEvent>(onKeyDownEvent, TrickleDown.TrickleDown);
            // changeInfoPanel.UnregisterCallback<PointerDownEvent>(onMouseDownEvent, TrickleDown.TrickleDown);
        }
        

        public void ChangeKey(string s)
        {
            currentKey = s;
            altKey = false;
            string[] cArgs = new string[1];
            cArgs[0] = "T";
            AtavismEventSystem.DispatchEvent("CHANGE_KEY",cArgs);
           // AtavismKeyDefinition akd = typeof(AtavismKeySettings).GetField(s).GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
            if (changeInfoPanel!=null)
                changeInfoPanel.ShowVisualElement();
            foreach (var c in controls)
            {
                if (c.button != null)
                    c.button.SetEnabled(false); 
                if (c.altButton != null)
                    c.altButton.SetEnabled(false);
            }

            foreach (var w in windows)
            {
                if (w.button != null)
                    w.button.SetEnabled(false);
                if (w.altButton != null)
                    w.altButton.SetEnabled(false);
            }
            
            // screen.pickingMode = PickingMode.Position;
            // screen.RegisterCallback<KeyDownEvent>(onKeyDownEvent, TrickleDown.TrickleDown);
            // screen.RegisterCallback<PointerDownEvent>(onMouseDownEvent, TrickleDown.TrickleDown);
            // changeInfoPanel.pickingMode = PickingMode.Position;
            // changeInfoPanel.RegisterCallback<KeyDownEvent>(onKeyDownEvent, TrickleDown.TrickleDown);
            // changeInfoPanel.RegisterCallback<PointerDownEvent>(onMouseDownEvent, TrickleDown.TrickleDown);
            
        }
        public void ChangeAltKey(string s)
        {
            currentKey = s;
            altKey = true;
            string[] cArgs = new string[1];
            cArgs[0] = "T";
            AtavismEventSystem.DispatchEvent("CHANGE_KEY",cArgs);
            //AtavismKeyDefinition akd = typeof(AtavismKeySettings).GetField(s).GetValue(AtavismSettings.Instance.GetKeySettings()) as AtavismKeyDefinition;
            if (changeInfoPanel!=null)
                changeInfoPanel.ShowVisualElement();

            foreach (var c in controls)
            {
                if (c.button != null)
                    c.button.SetEnabled(false);
                if (c.altButton != null)
                    c.altButton.SetEnabled(false);
            }

            foreach (var w in windows)
            {
                if (w.button != null)
                    w.button.SetEnabled(false);
                if (w.altButton != null)
                    w.altButton.SetEnabled(false);
            }
            // screen.pickingMode = PickingMode.Position;
            // screen.RegisterCallback<KeyDownEvent>(onKeyDownEvent, TrickleDown.TrickleDown);
            // screen.RegisterCallback<PointerDownEvent>(onMouseDownEvent, TrickleDown.TrickleDown);
            // changeInfoPanel.pickingMode = PickingMode.Position;
            // changeInfoPanel.RegisterCallback<KeyDownEvent>(onKeyDownEvent, TrickleDown.TrickleDown);
            // changeInfoPanel.RegisterCallback<PointerDownEvent>(onMouseDownEvent, TrickleDown.TrickleDown);
        }

        public void ChangeDodgeOption(ChangeEvent<int> evt)
        {
            AtavismSettings.Instance.GetKeySettings().dodgeDoubleTap = evt.newValue == 0;
            UpdateViewKeys();
        }
    }
}