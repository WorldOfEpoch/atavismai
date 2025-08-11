using HNGamers.Atavism;
using System.Collections;
using System.Collections.Generic;
using Atavism.UIEditor;
using UnityEditor;
using UnityEngine;
namespace Atavism
{
    [CustomEditor(typeof(AtavismSettings))]
     class AtavismSettingsEditor : Editor
    {
        AtavismSettings script;
        SerializedObject thisScriptObject;
        List<string> actions;

        private SerializedProperty settings;
        private SerializedProperty maxTimeBetweenDoubleTap ;
        private SerializedProperty openGameSettingsKey ;
        private SerializedProperty useSameKeyForBarMenuFromGameSetting ;
        private SerializedProperty openToolBarMenuKey ; 
        private SerializedProperty questPrevLimit ;
        private SerializedProperty expIcon;
        private SerializedProperty saveInFile;
        private SerializedProperty dragJoysticStopAutoAttack;
        private SerializedProperty adminLocationSettings;
        private SerializedProperty adminRestartSettings;
        
        private SerializedProperty UISlotInventoryItemUXML;
        private SerializedProperty UISlotActionBarItemUXML;
        private SerializedProperty UISlotBagItemUXML;
        private SerializedProperty UIActivatableUXML;


        private SerializedProperty actionBarPrefab;
        private SerializedProperty inventoryItemPrefab;
        private SerializedProperty abilitySlotPrefab;
        private SerializedProperty effectQualityColor;
        private SerializedProperty abilityQualityColor;
        private SerializedProperty itemQualityColor;
        private SerializedProperty defaultBagIcon;
        private SerializedProperty defaultItemIcon;
        private SerializedProperty itemDropColorTrue;
        private SerializedProperty itemDropColorFalse;
        private SerializedProperty visibleMobsName;
        private SerializedProperty visibleShopMessageOnSelf;
        private SerializedProperty visibleOid;
        private SerializedProperty showLevelWithName;
        private SerializedProperty mobNameFont;
        private SerializedProperty mobNamePosition;
        private SerializedProperty mobNameFontSize;
        private SerializedProperty mobNameDefaultColor;
        private SerializedProperty mobNameAlignment;
        private SerializedProperty mobNameMargin;
        private SerializedProperty mobNameOutlineWidth;
        private SerializedProperty questNewText;
        private SerializedProperty questConcludableText;
        private SerializedProperty questProgressText;
        private SerializedProperty shopText;
        private SerializedProperty bankText;
        private SerializedProperty npcInfoTextSize;
        private SerializedProperty npcInfoTextColor;
        private SerializedProperty npcInfoTextPosition;
        private SerializedProperty npcInfoSpriteAsset;
        private SerializedProperty races;
        private SerializedProperty masterMixer;
        private SerializedProperty mixerFocus;
        private SerializedProperty mixerNoFocus;
        private SerializedProperty _defaultQualitySettings;

        private SerializedProperty UMAGameObject;
        private SerializedProperty wardrobeGameObject;

        private SerializedProperty sceneLoaderPrefab;

        private SerializedProperty minimapSettings;

        private SerializedProperty gameInstances;
        private SerializedProperty arenaInstances;
        private SerializedProperty dungeonInstances;

        
        SerializedProperty myGUID;

        private void OnEnable()
        {
            script = (AtavismSettings)target;
            thisScriptObject = new SerializedObject(target);


           

            settings = thisScriptObject.FindProperty("settings");
            maxTimeBetweenDoubleTap= thisScriptObject.FindProperty("maxTimeBetweenDoubleTap");
            openGameSettingsKey= thisScriptObject.FindProperty("openGameSettingsKey");
            useSameKeyForBarMenuFromGameSetting= thisScriptObject.FindProperty("useSameKeyForBarMenuFromGameSetting");
            openToolBarMenuKey= thisScriptObject.FindProperty("openToolBarMenuKey");
            questPrevLimit= thisScriptObject.FindProperty("questPrevLimit");
            expIcon= thisScriptObject.FindProperty("expIcon");
            saveInFile= thisScriptObject.FindProperty("saveInFile");
            dragJoysticStopAutoAttack= thisScriptObject.FindProperty("dragJoysticStopAutoAttack");
            adminLocationSettings= thisScriptObject.FindProperty("_adminLocationSettings");
            adminRestartSettings= thisScriptObject.FindProperty("_adminRestartSettings");
            
            UISlotInventoryItemUXML = thisScriptObject.FindProperty("UISlotInventoryItemUXML");
            UISlotActionBarItemUXML = thisScriptObject.FindProperty("UISlotActionBarItemUXML");
            UISlotBagItemUXML = thisScriptObject.FindProperty("UISlotBagItemUXML");
            UIActivatableUXML = thisScriptObject.FindProperty("UIActivatableUXML");

            actionBarPrefab = thisScriptObject.FindProperty("actionBarPrefab");
            inventoryItemPrefab= thisScriptObject.FindProperty("inventoryItemPrefab");
            abilitySlotPrefab= thisScriptObject.FindProperty("abilitySlotPrefab");
            effectQualityColor= thisScriptObject.FindProperty("effectQualityColor");
            abilityQualityColor= thisScriptObject.FindProperty("abilityQualityColor");
            itemQualityColor= thisScriptObject.FindProperty("itemQualityColor");
            defaultBagIcon= thisScriptObject.FindProperty("defaultBagIcon");
            defaultItemIcon= thisScriptObject.FindProperty("defaultItemIcon");
            itemDropColorTrue= thisScriptObject.FindProperty("itemDropColorTrue");
            itemDropColorFalse= thisScriptObject.FindProperty("itemDropColorFalse");
            visibleMobsName= thisScriptObject.FindProperty("visibleMobsName");
            visibleShopMessageOnSelf= thisScriptObject.FindProperty("visibleShopMessageOnSelf");
            visibleOid= thisScriptObject.FindProperty("visibleOid");
            showLevelWithName= thisScriptObject.FindProperty("showLevelWithName");
            mobNameFont= thisScriptObject.FindProperty("mobNameFont");
            mobNamePosition= thisScriptObject.FindProperty("mobNamePosition");
            mobNameFontSize= thisScriptObject.FindProperty("mobNameFontSize");
            mobNameDefaultColor= thisScriptObject.FindProperty("mobNameDefaultColor");
            mobNameAlignment= thisScriptObject.FindProperty("mobNameAlignment");
            mobNameMargin= thisScriptObject.FindProperty("mobNameMargin");
            mobNameOutlineWidth= thisScriptObject.FindProperty("mobNameOutlineWidth");
            questNewText= thisScriptObject.FindProperty("questNewText");
            questConcludableText= thisScriptObject.FindProperty("questConcludableText");
            questProgressText= thisScriptObject.FindProperty("questProgressText");
            shopText= thisScriptObject.FindProperty("shopText");
            bankText= thisScriptObject.FindProperty("bankText");
            npcInfoTextSize= thisScriptObject.FindProperty("npcInfoTextSize");
            npcInfoTextColor= thisScriptObject.FindProperty("npcInfoTextColor");
            npcInfoTextPosition= thisScriptObject.FindProperty("npcInfoTextPosition");
            npcInfoSpriteAsset= thisScriptObject.FindProperty("npcInfoSpriteAsset");
            races= thisScriptObject.FindProperty("races");
            masterMixer= thisScriptObject.FindProperty("masterMixer");
            mixerFocus= thisScriptObject.FindProperty("mixerFocus");
            mixerNoFocus= thisScriptObject.FindProperty("mixerNoFocus");
            _defaultQualitySettings= thisScriptObject.FindProperty("_defaultQualitySettings");
            UMAGameObject= thisScriptObject.FindProperty("UMAGameObject");
            wardrobeGameObject= thisScriptObject.FindProperty("wardrobeGameObject");
            sceneLoaderPrefab= thisScriptObject.FindProperty("sceneLoaderPrefab");
            minimapSettings= thisScriptObject.FindProperty("minimapSettings");
            gameInstances= thisScriptObject.FindProperty("gameInstances");
            arenaInstances= thisScriptObject.FindProperty("arenaInstances");
            dungeonInstances= thisScriptObject.FindProperty("dungeonInstances");
        }

        public override void OnInspectorGUI()
        {
            thisScriptObject.Update();
            EditorGUI.BeginChangeCheck();

            if (actions == null)
            {
                actions = AtavismEditorFunctions.LoadOptionChoiceList("Weapon Actions", false);
            }

            List<AtavismKeyDefinition> newList = new List<AtavismKeyDefinition>();
            foreach (var action in actions)
            {
            //    Debug.LogError(action);
                if (script.GetKeySettings().AdditionalActions(action) == null)
                {
                    AtavismKeyDefinition adk = new AtavismKeyDefinition();
                    adk.name = action;
                    newList.Add(adk);
                    
                }
                else
                {
                    newList.Add(script.GetKeySettings().AdditionalActions(action));
                }    
            }

            script.GetKeySettings().additionalActions = newList;
            EditorGUILayout.PropertyField(settings);
            // EditorGUILayout.LabelField("Weapon Actions Keys Settings");
            //
            //  foreach (var action in script.GetKeySettings().additionalActions)
            //  {
            //    //  EditorGUILayout.LabelField(action.name);
            //      action.key = (KeyCode)EditorGUILayout.EnumPopup("Key",action.key);
            //      action.altKey = (KeyCode)EditorGUILayout.EnumPopup("Alt Key",action.altKey);
            //      action.canChange = EditorGUILayout.Toggle("Key",action.canChange);
            //      action.show = EditorGUILayout.Toggle("Show",action.show);
            //      EditorGUILayout.Space();
            //  }
            //
            EditorGUILayout.PropertyField(maxTimeBetweenDoubleTap);
            EditorGUILayout.PropertyField(openGameSettingsKey);
            EditorGUILayout.PropertyField(useSameKeyForBarMenuFromGameSetting);
            EditorGUILayout.PropertyField(openToolBarMenuKey);
            EditorGUILayout.PropertyField(questPrevLimit);
            EditorGUILayout.PropertyField(expIcon);
            EditorGUILayout.PropertyField(saveInFile);
            EditorGUILayout.PropertyField(dragJoysticStopAutoAttack);

            
            EditorGUILayout.PropertyField(adminLocationSettings);
            EditorGUILayout.PropertyField(adminRestartSettings);
                 
            EditorGUILayout.PropertyField(UISlotInventoryItemUXML);
            EditorGUILayout.PropertyField(UISlotActionBarItemUXML);
            EditorGUILayout.PropertyField(UISlotBagItemUXML);
            EditorGUILayout.PropertyField(UIActivatableUXML);


            EditorGUILayout.PropertyField(actionBarPrefab);
            EditorGUILayout.PropertyField(inventoryItemPrefab);
            EditorGUILayout.PropertyField(abilitySlotPrefab);
            EditorGUILayout.PropertyField(effectQualityColor);
            EditorGUILayout.PropertyField(abilityQualityColor);
            EditorGUILayout.PropertyField(itemQualityColor);
            // EditorGUILayout.PropertyField(qualityNames);
            EditorGUILayout.PropertyField(defaultBagIcon);
            EditorGUILayout.PropertyField(defaultItemIcon);
            EditorGUILayout.PropertyField(itemDropColorTrue);
            EditorGUILayout.PropertyField(itemDropColorFalse);
            EditorGUILayout.PropertyField(visibleMobsName);
            EditorGUILayout.PropertyField(visibleShopMessageOnSelf);
            EditorGUILayout.PropertyField(visibleOid);
            EditorGUILayout.PropertyField(showLevelWithName);
            EditorGUILayout.PropertyField(mobNameFont);
            EditorGUILayout.PropertyField(mobNamePosition);
            EditorGUILayout.PropertyField(mobNameFontSize);
            EditorGUILayout.PropertyField(mobNameDefaultColor);
            EditorGUILayout.PropertyField(mobNameAlignment);
            EditorGUILayout.PropertyField(mobNameMargin);
            EditorGUILayout.PropertyField(mobNameOutlineWidth);
            EditorGUILayout.PropertyField(questNewText);
            EditorGUILayout.PropertyField(questConcludableText);
            EditorGUILayout.PropertyField(questProgressText);
            EditorGUILayout.PropertyField(shopText);
            EditorGUILayout.PropertyField(bankText);
            EditorGUILayout.PropertyField(npcInfoTextSize);
            EditorGUILayout.PropertyField(npcInfoTextColor);
            EditorGUILayout.PropertyField(npcInfoTextPosition);
            EditorGUILayout.PropertyField(npcInfoSpriteAsset);
            EditorGUILayout.PropertyField(races);
            
            EditorGUILayout.PropertyField(masterMixer);
            EditorGUILayout.PropertyField(mixerFocus);
            EditorGUILayout.PropertyField(mixerNoFocus);
            
            EditorGUILayout.PropertyField(_defaultQualitySettings);

            EditorGUILayout.PropertyField(UMAGameObject);
            EditorGUILayout.PropertyField(wardrobeGameObject);

            EditorGUILayout.PropertyField(sceneLoaderPrefab);

            EditorGUILayout.PropertyField(minimapSettings);

            EditorGUILayout.PropertyField(gameInstances);
            EditorGUILayout.PropertyField(arenaInstances);
            EditorGUILayout.PropertyField(dungeonInstances);
           


            if (EditorGUI.EndChangeCheck())
            {
                thisScriptObject.ApplyModifiedProperties();
                GUI.FocusControl(null);
            }
            EditorGUI.BeginChangeCheck();

            thisScriptObject.ApplyModifiedProperties();  // resolve issue with not detecting a change. (Errol)
            if (EditorGUI.EndChangeCheck())
            {
                // switch (script.currentTab)
                // {
                //     case "Parts":
                //     case "Replacement":
                //         GUI.FocusControl(null);
                //         break;
                // }
             
            }

        }

    }
}