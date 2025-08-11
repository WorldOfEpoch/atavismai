using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Atavism.UIEditor;

namespace Atavism
{
    [CustomEditor(typeof(InteractiveObject))]
    [CanEditMultipleObjects]
    public class InteractiveObjectEditor : Editor
    {

        private bool profilesLoaded = false;
        private bool effectsLoaded = false;
        private bool questsLoaded = false;
        private bool tasksLoaded = false;
        private bool instancesLoaded = false;
        private bool currencyLoaded = false;
        private bool itemsLoaded = false;
        string[] interactionTypes;
        bool help = false;

        private string searchProfile = "";
        private string searchItem = "";
        private string searchCurrency = "";
        private string searchQuest = "";
        private string searchQuestReq = "";
        private string searchEffect = "";
        private string searchInstance = "";
        private string searchTask = "";

        
        private SerializedProperty idProp;
        private SerializedProperty prefabProp;
        private SerializedProperty profileIdProp;
        private SerializedProperty interactionTypeProp;
        private SerializedProperty interactionIDProp;
        private SerializedProperty interactionData1Prop;
        private SerializedProperty interactionData2Prop;
        private SerializedProperty interactionData3Prop;
        private SerializedProperty questReqIDProp;
        private SerializedProperty interactTimeReqProp;
        private SerializedProperty refreshDurationProp;
        private SerializedProperty cursorIconProp;
        private SerializedProperty highlightProp;
        private SerializedProperty highlightColourProp;
        private SerializedProperty minLevelProp;

        
        private SerializedProperty maxLevelProp;
        private SerializedProperty itemRequirementProp;
        private SerializedProperty itemCountRequirementProp;
        private SerializedProperty itemRequirementGetProp;
        private SerializedProperty currencyRequirementProp;
        private SerializedProperty currencyCountRequirementProp;
        private SerializedProperty currencyRequirementGetProp;
        private SerializedProperty makeBusyProp;
        private SerializedProperty despawnTimeProp;
        private SerializedProperty despawnDelayProp;
        private SerializedProperty useLimitProp;
        private SerializedProperty interactDistanceProp;
        private SerializedProperty interactCoordEffectProp;
        private SerializedProperty activateCoordEffectsProp;

        private void OnEnable()
        {
             idProp = serializedObject.FindProperty("id");
             prefabProp = serializedObject.FindProperty("prefab");
             profileIdProp = serializedObject.FindProperty("profileId");
             interactionTypeProp = serializedObject.FindProperty("interactionType");
             interactionIDProp = serializedObject.FindProperty("interactionID");
             interactionData1Prop = serializedObject.FindProperty("interactionData1");
             interactionData2Prop = serializedObject.FindProperty("interactionData2");
             interactionData3Prop = serializedObject.FindProperty("interactionData3");
             questReqIDProp = serializedObject.FindProperty("questReqID");
             interactTimeReqProp = serializedObject.FindProperty("interactTimeReq");
             refreshDurationProp = serializedObject.FindProperty("refreshDuration");
             cursorIconProp = serializedObject.FindProperty("cursorIcon");
             highlightProp = serializedObject.FindProperty("highlight");
             highlightColourProp = serializedObject.FindProperty("highlightColour");
             minLevelProp = serializedObject.FindProperty("minLevel");
             maxLevelProp = serializedObject.FindProperty("maxLevel");
             itemRequirementProp = serializedObject.FindProperty("itemRequirement");
             itemCountRequirementProp = serializedObject.FindProperty("itemCountRequirement");
             itemRequirementGetProp = serializedObject.FindProperty("itemRequirementGet");
             currencyRequirementProp = serializedObject.FindProperty("currencyRequirement");
             currencyCountRequirementProp = serializedObject.FindProperty("currencyCountRequirement");
             currencyRequirementGetProp = serializedObject.FindProperty("currencyRequirementGet");
             makeBusyProp = serializedObject.FindProperty("makeBusy");
             despawnTimeProp = serializedObject.FindProperty("despawnTime");
             despawnDelayProp = serializedObject.FindProperty("despawnDelay");
             useLimitProp = serializedObject.FindProperty("useLimit");
             interactDistanceProp = serializedObject.FindProperty("interactDistance");
             interactCoordEffectProp = serializedObject.FindProperty("interactCoordEffect");
             activateCoordEffectsProp = serializedObject.FindProperty("activateCoordEffects");
            
            
        }

        public override void OnInspectorGUI()
        {
         //   var indentOffset = EditorGUI.indentLevel * 5f;
         serializedObject.Update();
            var lineRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            var labelRect = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.currentViewWidth - 60f, lineRect.height);
            var fieldRect = new Rect(labelRect.xMax, lineRect.y, lineRect.width - labelRect.width - 60f, lineRect.height);
            var buttonRect = new Rect(fieldRect.xMax, lineRect.y, 60f, lineRect.height);

            InteractiveObject obj = target as InteractiveObject;
            GUIContent content = new GUIContent("Help");
            content.tooltip = "Click to show or hide help information's";
            if (GUI.Button(buttonRect, content, EditorStyles.miniButton))
                help = !help;
            GUIStyle topStyle = new GUIStyle(GUI.skin.box);
            topStyle.normal.textColor = Color.white;
            topStyle.hover.textColor = Color.white;
            topStyle.fontStyle = FontStyle.Bold;
            topStyle.alignment = TextAnchor.UpperLeft;
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.cyan;
            boxStyle.hover.textColor = Color.cyan;
            boxStyle.fontStyle = FontStyle.Bold;
            boxStyle.alignment = TextAnchor.UpperLeft;
            GUILayout.BeginVertical("Atavism Interactive Object Configuration", topStyle);
            GUILayout.Space(20);

             var lineResetRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            var buttonResetRect = new Rect(fieldRect.xMax, lineResetRect.y, 60f, lineResetRect.height);
            GUIContent reset = new GUIContent("Reset ID");
            reset.tooltip = "Click to reset ID ";
            if (GUI.Button(buttonResetRect, reset, EditorStyles.miniButton))
                idProp.intValue = -1;
            content = new GUIContent("ID");
            content.tooltip = "Id";
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(idProp, content);
            EditorGUI.EndDisabledGroup();
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            // Read in Interaction options from Database and display a drop down list
            
            if (interactionTypes == null)
            {
                interactionTypes = new string[] { "~ none ~" };
                interactionTypes = AtavismEditorFunctions.LoadAtavismChoiceOptions("Interaction Type", false);
            }
            if (interactionTypes.Length == 0)
            {
                EditorGUILayout.LabelField("!! Interaction Type is not loaded check database configuration in Atavism Editor !!");
                GUILayout.EndVertical();
                return;
            }
            
            if (obj.activateCoordEffects == null)
                obj.activateCoordEffects = new System.Collections.Generic.List<GameObject>();
            if (!profilesLoaded)
                AtavismEditorFunctions.LoadInteractiveObjectProfile();
            profilesLoaded = true;


            
            content = new GUIContent("Search Profile:");
            content.tooltip = "Search Profile ";
            searchProfile = EditorGUILayout.TextField(content, searchProfile);
            content = new GUIContent("Profile:");
            content.tooltip = "Profile";

            var pid = profileIdProp.intValue;
            profileIdProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchProfile, profileIdProp.intValue,  AtavismEditorFunctions.interactiveProfileOptions, AtavismEditorFunctions.interactiveProfileIds);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            

            if (pid != profileIdProp.intValue && profileIdProp.intValue > 0)
            {
                    var profile = AtavismEditorFunctions.LoadInteractiveObjectProfile(profileIdProp.intValue);
                    foreach (var key in profile.Keys)
                    {
                    //    Debug.Log(key);
                    }
                    interactionTypeProp.stringValue = profile["interactionType"];
                    interactionIDProp.intValue = int.Parse(profile["interactionID"]);
                    interactionData1Prop.stringValue = profile["interactionData1"];
                    interactionData2Prop.stringValue = profile["interactionData2"];
                    interactionData3Prop.stringValue = profile["interactionData3"];
                    questReqIDProp.intValue = int.Parse(profile["questReqID"]);
                    interactTimeReqProp.floatValue = float.Parse(profile["interactTimeReq"]);
                    refreshDurationProp.intValue = int.Parse(profile["respawnTime"]);
                    interactTimeReqProp.floatValue = float.Parse(profile["interactTimeReq"]);
                    makeBusyProp.boolValue = int.Parse(profile["makeBusy"])==1;
                    despawnTimeProp.floatValue = float.Parse(profile["despawnTime"]);
                    despawnDelayProp.floatValue = float.Parse(profile["despawnDelay"]);
                    useLimitProp.intValue= int.Parse(profile["useLimit"]);
                    interactDistanceProp.floatValue= float.Parse(profile["interactDistance"]);

                    string ce = profile["coordEffect"];
                    if (!string.IsNullOrEmpty(ce))
                    {
                        
                        if (!ce.Contains("Resources"))
                            ce = "Assets/Resources/Content/CoordinatedEffects/" + ce+".prefab";
                        GameObject _go = (GameObject)AssetDatabase.LoadAssetAtPath(ce, typeof(GameObject));
                        interactCoordEffectProp.objectReferenceValue = _go;

                    }
                    else
                    {
                        interactCoordEffectProp.objectReferenceValue = null;
                    }
                    maxLevelProp.intValue= int.Parse(profile["maxLevel"]);
                    minLevelProp.intValue= int.Parse(profile["minLevel"]);
                    itemRequirementProp.intValue= int.Parse(profile["itemReq"]);
                    itemRequirementGetProp.boolValue= int.Parse(profile["itemReqGet"])==1;
                    itemCountRequirementProp.intValue= int.Parse(profile["itemCountReq"]);
                    currencyRequirementProp.intValue= int.Parse(profile["currencyReq"]);
                    currencyRequirementGetProp.boolValue= int.Parse(profile["currencyReqGet"])==1;
                    currencyCountRequirementProp.intValue= int.Parse(profile["currencyCountReq"]);
                    string go = profile["gameObject"];
                    if (!string.IsNullOrEmpty(go))
                    {
                        prefabProp.objectReferenceValue =
                            (GameObject)AssetDatabase.LoadAssetAtPath(go, typeof(GameObject));
                    }
                    else
                    {
                        prefabProp.objectReferenceValue = null;
                    }

                    int num = int.Parse(profile["coord_num"]);
                    activateCoordEffectsProp.ClearArray();
                    for (int i = 0; i < num; i++)
                    {
                        Debug.Log(profile["coord_" + i]);
                        GameObject _go = (GameObject)AssetDatabase.LoadAssetAtPath(profile["coord_" + i], typeof(GameObject));
                        activateCoordEffectsProp.InsertArrayElementAtIndex(activateCoordEffectsProp.arraySize);
                        activateCoordEffectsProp.GetArrayElementAtIndex(activateCoordEffectsProp.arraySize - 1).objectReferenceValue = _go;
                    }
                    
                    string icon = profile["icon"];
                    if (!string.IsNullOrEmpty(icon))
                    {
                        Texture2D _go = (Texture2D)AssetDatabase.LoadAssetAtPath(icon, typeof(Texture2D));
                        cursorIconProp.objectReferenceValue = _go;

                    }else
                        cursorIconProp.objectReferenceValue = null;
            }
            
                GUILayout.BeginVertical("Requirements",boxStyle);
            GUILayout.Space(20);
            // Quest Required ID

            
            content = new GUIContent("Search Quest:");
            content.tooltip = "Search Quest ";
            searchQuestReq = EditorGUILayout.TextField(content, searchQuestReq);
            content = new GUIContent("Required Quest:");
            content.tooltip = "Defines quest which the player must have started to interact with that Interactive Object";
            bool preBoolValue = false; 
            float preFloatValue = 0f; 
            int preIntValue = questReqIDProp.intValue;
            questReqIDProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchQuestReq, questReqIDProp.intValue, AtavismEditorFunctions.GuiQuestOptions, AtavismEditorFunctions.questIds);
            if (preIntValue != questReqIDProp.intValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            content = new GUIContent("Min Level");
            content.tooltip = "Defines minimum level that the player must have for interaction";
            preIntValue = minLevelProp.intValue;
                minLevelProp.intValue = EditorGUILayout.IntField(content, minLevelProp.intValue);
                if (preIntValue != minLevelProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }

                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Max Level");
            content.tooltip = "Defines maximum level that the player must have for interaction";
            preIntValue = maxLevelProp.intValue;
            maxLevelProp.intValue = EditorGUILayout.IntField(content, maxLevelProp.intValue);
            if (preIntValue != maxLevelProp.intValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
         
            // Item Required 

            content = new GUIContent("Search Item:");
            content.tooltip = "Search Item ";
            searchItem = EditorGUILayout.TextField(content, searchItem);
            content = new GUIContent("Required Item:");
            content.tooltip = "Defines items that the player must have for interaction.";
            preIntValue = itemRequirementProp.intValue;
            itemRequirementProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchItem, itemRequirementProp.intValue,  AtavismEditorFunctions.GuiItemsList, AtavismEditorFunctions.itemIds);
            if (preIntValue != itemRequirementProp.intValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            if (itemRequirementProp.intValue > 0)
            {
                content = new GUIContent("Item Count");
                content.tooltip = "Defines the number of items that the player must have for interaction.";
                preIntValue = itemCountRequirementProp.intValue;
                itemCountRequirementProp.intValue = EditorGUILayout.IntField(content, itemCountRequirementProp.intValue);
                if (preIntValue != itemCountRequirementProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }  
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
                content = new GUIContent("Take Item");
                content.tooltip = "Defines if these items will be taken upon interaction.";
                preBoolValue = itemRequirementGetProp.boolValue;
                itemRequirementGetProp.boolValue = EditorGUILayout.Toggle(content, itemRequirementGetProp.boolValue);
                if (preBoolValue != itemRequirementGetProp.boolValue)
                {
                    profileIdProp.intValue = -1;
                } 
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            }

            // Currecny Required 

            content = new GUIContent("Search Currency:");
            content.tooltip = "Search Currency ";
            searchCurrency = EditorGUILayout.TextField(content, searchCurrency);
            content = new GUIContent("Required Currency:");
            content.tooltip = "Defines currency that the player must have for interaction.";
            preIntValue = currencyRequirementProp.intValue;
            currencyRequirementProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchCurrency, currencyRequirementProp.intValue, AtavismEditorFunctions.GuiCurrencyOptions, AtavismEditorFunctions.currencyIds);
            if (preIntValue != currencyRequirementProp.intValue)
            {
                profileIdProp.intValue = -1;
            }  
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            if (currencyRequirementProp.intValue > 0)
            {
                content = new GUIContent("Currency Amount");
                content.tooltip = "Defines the amount of currency that the player must have for interaction.";
                preIntValue = currencyCountRequirementProp.intValue;
                currencyCountRequirementProp.intValue = EditorGUILayout.IntField(content, currencyCountRequirementProp.intValue);
                if (preIntValue != currencyCountRequirementProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }  
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
                content = new GUIContent("Take Currency");
                content.tooltip = "Defines if amount of currency will be taken upon interaction.";
                preBoolValue = currencyRequirementGetProp.boolValue;
                currencyRequirementGetProp.boolValue = EditorGUILayout.Toggle(content, currencyRequirementGetProp.boolValue);
                if (preBoolValue != currencyRequirementGetProp.boolValue)
                {
                    profileIdProp.intValue = -1;
                }  
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            }
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Settings",boxStyle);
            GUILayout.Space(20);

            //Game Object / Prefab
            
            content = new GUIContent("Game Object");
            content.tooltip = "It’s a Game Object that will be spawned. For Interactive object saved in scene leave this option empty. (Dynamic Interactive Objects)";
            var prop = prefabProp.objectReferenceValue as GameObject;
            EditorGUILayout.PropertyField(prefabProp,content);
            if (prop != prefabProp.objectReferenceValue as GameObject)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            // Coord Effect

            content = new GUIContent("Interact Coord Effect");
            content.tooltip = "It’s a Coordinated Effect that will be played while interacting with the Interactive Object";
             prop = interactCoordEffectProp.objectReferenceValue as GameObject;
            EditorGUILayout.PropertyField(interactCoordEffectProp,content);
            if (prop != interactCoordEffectProp.objectReferenceValue as GameObject)
            {
            profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
           
            // Cursor Icon
            content = new GUIContent("Cursor Icon");
            content.tooltip = "Defines cursor while hovering over Interactive Object";
            EditorGUILayout.PropertyField(cursorIconProp,content);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            // Interaction Make busy

            content = new GUIContent("Use by One person at a time");
            content.tooltip = "Defines if the Interactive Objects should be usable by more than one concurrent player.";
            preBoolValue = makeBusyProp.boolValue;
            makeBusyProp.boolValue = EditorGUILayout.Toggle(content, makeBusyProp.boolValue);
            if (preBoolValue != makeBusyProp.boolValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            // Interaction Time

            content = new GUIContent("Interaction Time (s)");
            content.tooltip = "Defines the number of seconds for interacting with the Interactive Object";
            preFloatValue = interactTimeReqProp.floatValue;
            interactTimeReqProp.floatValue = EditorGUILayout.FloatField(content, interactTimeReqProp.floatValue);
            if (preFloatValue != interactTimeReqProp.floatValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            // Interaction Distance 

            content = new GUIContent("Interaction Distance (m)");
            content.tooltip = "Defines distance from which the Interactive Object can be interacted with";
          
            preFloatValue = interactDistanceProp.floatValue;
            interactDistanceProp.floatValue = EditorGUILayout.FloatField(content, interactDistanceProp.floatValue);
            if (preFloatValue != interactDistanceProp.floatValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);


            // Respawn Time

            content = new GUIContent("Respawn Time (s)");
            content.tooltip = "Defines the number of seconds after which the Interactive Object will be respawned after being used";
            preIntValue = refreshDurationProp.intValue;
            refreshDurationProp.intValue = EditorGUILayout.IntField(content, refreshDurationProp.intValue);
            if (preIntValue != refreshDurationProp.intValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            // Despawn Delay

            content = new GUIContent("Despawn Delay (s)");
            content.tooltip = "Defines delay of despawn after the Interactive Object will be used";
            preFloatValue = despawnDelayProp.floatValue;
            despawnDelayProp.floatValue = EditorGUILayout.FloatField(content, despawnDelayProp.floatValue);
            if (preFloatValue != despawnDelayProp.floatValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            // Despawn Time

            content = new GUIContent("Despawn Time (s)");
            content.tooltip = "Defines for how long the Interactive Object will disappear after was spawned. For Interactive object saved in scene leave this with value -1 or 0. (Dynamic Interactive Objects)";
            preFloatValue = despawnTimeProp.floatValue;
            despawnTimeProp.floatValue = EditorGUILayout.FloatField(content, despawnTimeProp.floatValue);
            if (preFloatValue != despawnTimeProp.floatValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

         
            // Use Limit

            content = new GUIContent("Use Limit");
            content.tooltip = "Defines the total number of usage. Set it to 0 or -1 for unlimited";
            preIntValue = useLimitProp.intValue;
            useLimitProp.intValue = EditorGUILayout.IntField(content, useLimitProp.intValue);
            if (preIntValue != useLimitProp.intValue)
            {
                profileIdProp.intValue = -1;
            }
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            
            
            int selectedOption = GetPositionOfInteraction(interactionTypeProp.stringValue);
            preIntValue = selectedOption;
            content = new GUIContent("Interaction Type");
            content.tooltip = "Defines the type of Interactive Object";
            selectedOption = EditorGUILayout.Popup(content, selectedOption, interactionTypes);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            interactionTypeProp.stringValue = interactionTypes[selectedOption];
            if (selectedOption != preIntValue)
            {
                profileIdProp.intValue = -1;
            }
            if (!questsLoaded)
                AtavismEditorFunctions.LoadQuestOptions(true);
            questsLoaded = true;

            if (!itemsLoaded)
                AtavismEditorFunctions.LoadItemOptions(true);
            itemsLoaded = true;
            if(!currencyLoaded)
                AtavismEditorFunctions.LoadCurrencyOptions(true);
            currencyLoaded = true;
            
            if (interactionTypeProp.stringValue.Contains("ApplyEffect"))
            {
                if (!effectsLoaded)
                    AtavismEditorFunctions.LoadEffectOptions(true);
                effectsLoaded = true;

                content = new GUIContent("Search Effect:");
                content.tooltip = "Search Effect ";
                searchEffect = EditorGUILayout.TextField(content, searchEffect);
                content = new GUIContent("Effect:");
                content.tooltip = "Define the Effect that will be applied once the interactive object is activated";
                preIntValue = interactionIDProp.intValue;
                interactionIDProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchEffect, interactionIDProp.intValue,   AtavismEditorFunctions.GuiEffectOptions, AtavismEditorFunctions.effectIds);
                if (preIntValue != interactionIDProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            }
            else if (interactionTypeProp.stringValue.Contains("Quest"))
            {

                content = new GUIContent("Search Quest:");
                content.tooltip = "Search Quest ";
                searchQuest = EditorGUILayout.TextField(content, searchQuest);
                content = new GUIContent("Quest:");
                content.tooltip = "Define the Quest that will be start once the interactive object is activated";
                preIntValue = interactionIDProp.intValue;
                interactionIDProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchQuest, interactionIDProp.intValue,   AtavismEditorFunctions.GuiQuestOptions, AtavismEditorFunctions.questIds);
                if (preIntValue != interactionIDProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            }
            else if (interactionTypeProp.stringValue.Contains("Task"))
            {
                if (!tasksLoaded)
                    AtavismEditorFunctions.LoadTaskOptions(true);
                tasksLoaded = true;

                content = new GUIContent("Search Task:");
                content.tooltip = "Search Task ";
                searchTask = EditorGUILayout.TextField(content, searchTask);
                content = new GUIContent("Task:");
                content.tooltip = "Define the Tack that will be complete once the interactive object is activated";
                preIntValue = interactionIDProp.intValue;
                interactionIDProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchTask, interactionIDProp.intValue,    AtavismEditorFunctions.GuiTaskOptions, AtavismEditorFunctions.taskIds);
                if (preIntValue != interactionIDProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
                
            }
            else if (interactionTypeProp.stringValue.Equals("InstancePortal"))
            {
                if (!instancesLoaded)
                    AtavismEditorFunctions.LoadInstanceOptions(true);
                instancesLoaded = true;

                content = new GUIContent("Search Instance:");
                content.tooltip = "Search Instance ";
                searchInstance = EditorGUILayout.TextField(content, searchInstance);
                content = new GUIContent("Instance:");
                content.tooltip = "Define the Instance to which the player will be moved once the interactive object is activated";
                preIntValue = interactionIDProp.intValue;
                interactionIDProp.intValue = AtavismEditorFunctions.GetFilteredListSelector(content, ref searchInstance, interactionIDProp.intValue,  AtavismEditorFunctions.GuiInstanceList, AtavismEditorFunctions.instanceIds);
                if (preIntValue != interactionIDProp.intValue)
                {
                    profileIdProp.intValue = -1;
                }
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
                
              
                // Need to get a location to teleport to
                Vector3 position = new Vector3();
                
                float.TryParse(interactionData1Prop.stringValue, out position.x);
                float.TryParse(interactionData2Prop.stringValue, out position.y);
                float.TryParse(interactionData3Prop.stringValue, out position.z);
                Vector3 positionValue = position;
                content = new GUIContent("Position");
                content.tooltip = "Define position in the Instance to which the player will be moved once the interactive object is activated";
                position = EditorGUILayout.Vector3Field(content, position);
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
                interactionData1Prop.stringValue = position.x.ToString();
                interactionData2Prop.stringValue = position.y.ToString();
                interactionData3Prop.stringValue = position.z.ToString();

                if (positionValue != position)
                {
                    profileIdProp.intValue = -1;
                }
                
            }
            else if (interactionTypeProp.stringValue.Contains("CoordEffect"))
            {

                content = new GUIContent("Coord Effects");
                content.tooltip = "Define Coordinated Effect which will be played one after another once the Interactive Object is activated";
                EditorGUILayout.LabelField(content);
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(activateCoordEffectsProp);
                if (EditorGUI.EndChangeCheck())
                {
                    profileIdProp.intValue = -1;
                }
            }

            // highlight colour
            content = new GUIContent("Highlight");
            content.tooltip = "Turn On/Off Highlight ";
            highlightProp.boolValue = EditorGUILayout.Toggle(content, highlightProp.boolValue);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            if (highlightProp.boolValue)
            {
                content = new GUIContent("Highlight Colour");
                content.tooltip = "Highlight Colour";
                highlightColourProp.colorValue = EditorGUILayout.ColorField(content, highlightColourProp.colorValue);
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            }
            GUILayout.EndVertical();
        
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(obj);
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
            
        }

        private int GetPositionOfInteraction(string interactionType)
        {
            for (int i = 0; i < interactionTypes.Length; i++)
            {
                if (interactionTypes[i] == interactionType)
                    return i;
            }
            return 0;
        }
    }
}