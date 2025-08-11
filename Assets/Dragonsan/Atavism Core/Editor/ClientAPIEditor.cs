using UnityEditor;
using UnityEngine;


namespace Atavism
{
    [CustomEditor(typeof(ClientAPI))]
    public class ClientAPIEditor : Editor
    {
        bool help = false;
        public int[] classIds = new int[] {-1};
        public string[] classOptions = new string[] {"~ none ~"};

        public int[] raceIds = new int[] {-1};
        public string[] raceOptions = new string[] {"~ none ~"};
        public int[] genderIds = new int[] {-1};
        public string[] genderOptions = new string[] {"~ none ~"};
        GUIContent[] slots;
        GUIContent[] slots2;

        private bool loaded = false;

        public override void OnInspectorGUI()
        {
            ClientAPI obj = target as ClientAPI;
            //   var indentOffset = EditorGUI.indentLevel * 5f;
            var lineRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            var labelRect = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.currentViewWidth - 60f, lineRect.height);
            var fieldRect = new Rect(labelRect.xMax, lineRect.y, lineRect.width - labelRect.width - 60f, lineRect.height);
            var buttonRect = new Rect(fieldRect.xMax, lineRect.y, 60f, lineRect.height);
            GUIContent content = new GUIContent("Help");
            content.tooltip = "Click to show or hide help information's";
            if (GUI.Button(buttonRect, content, EditorStyles.miniButton))
                help = !help;

            GUIStyle topStyle = new GUIStyle(GUI.skin.box);
            topStyle.normal.textColor = Color.white;
            topStyle.fontStyle = FontStyle.Bold;
            topStyle.alignment = TextAnchor.UpperLeft;
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.cyan;
            boxStyle.fontStyle = FontStyle.Bold;
            boxStyle.alignment = TextAnchor.UpperLeft;
            GUILayout.BeginVertical("Atavism Client Api", topStyle);
            GUILayout.Space(20);
            
            
            GUI.backgroundColor = Color.blue;
            GUILayout.BeginVertical("Server Connection Settings", topStyle);
            GUI.backgroundColor = Color.white;
            GUILayout.Space(20);
            
            content = new GUIContent("Master Server");
            content.tooltip = "Master Server";
            obj.masterServer = EditorGUILayout.TextField(content, obj.masterServer);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Master Server Rdp Port");
            content.tooltip = "Master Server Rdp Port";
            obj.masterServerRdpPort = (ushort)EditorGUILayout.IntField(content, obj.masterServerRdpPort);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Master Server Tcp Port");
            content.tooltip = "Master Server Tcp Port";
            obj.masterServerTcpPort = (ushort)EditorGUILayout.IntField(content, obj.masterServerTcpPort);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Prefab Server");
            content.tooltip = "Prefab Server";
            obj.prefabServer = EditorGUILayout.TextField(content, obj.prefabServer);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Prefab Server Port");
            content.tooltip = "Prefab Server Port";
            obj.prefabServerPort = (ushort)EditorGUILayout.IntField(content, obj.prefabServerPort);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            GUILayout.EndVertical();
            
            GUILayout.Space(5);
            
            content = new GUIContent("Character Scene Name");
            content.tooltip = "Character Scene Name";
            obj.characterSceneName = EditorGUILayout.TextField(content, obj.characterSceneName);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            
            content = new GUIContent("Script Object");
            content.tooltip = "Select object in Login scene with Atavism Scripts assigned to it";
            obj.scriptObject = (GameObject) EditorGUILayout.ObjectField(content, obj.scriptObject, typeof(GameObject),true);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            //log level
            content = new GUIContent("Log level");
            content.tooltip = "Log Level";
            obj.logLevel = (LogLevel)EditorGUILayout.EnumPopup(content, obj.logLevel);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            AtavismLogger.logLevel = obj.logLevel;
            
            GUILayout.Space(5);
            
            content = new GUIContent("Ignore Server Props Objects");
            content.tooltip = "Select to not create in hierarchy props object";
            obj.ignoreServerPropObjects = EditorGUILayout.Toggle(content, obj.ignoreServerPropObjects);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            content = new GUIContent("Max Time Between Messages");
            content.tooltip = "The maximum time that can elapse since the last message received from the server before disconnecting";
            obj.maxTimeBetweenMessages = EditorGUILayout.FloatField(content, obj.maxTimeBetweenMessages);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Number Samples For Calculate Lag");
            content.tooltip = "Number Samples For Calculate Lag";
            obj.numberSamplesForCalculateLag = EditorGUILayout.IntField(content, obj.numberSamplesForCalculateLag);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            GUI.backgroundColor = Color.blue;
            GUILayout.BeginVertical("Player sync Params", topStyle);
            GUI.backgroundColor = Color.white;
            GUILayout.Space(20);
            
            content = new GUIContent("Moving Time Between Updates");
            content.tooltip = "Time between sending a message with the current position if the player is moving";
            obj.movingTimeBetweenUpdates = EditorGUILayout.FloatField(content, obj.movingTimeBetweenUpdates);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            content = new GUIContent("Static Time Between Updates");
            content.tooltip = "Time between sending a message with the current position if the player is not moving";
            obj.staticTimeBetweenUpdates = EditorGUILayout.FloatField(content, obj.staticTimeBetweenUpdates);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            content = new GUIContent("Moving Sync Frame Multiply");
            content.tooltip = "How long as the character is to be synchronized within one frame if player moving ";
            obj.movingSyncFrameMultiply = EditorGUILayout.FloatField(content, obj.movingSyncFrameMultiply);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Static Sync Frame Multiply");
            content.tooltip = "How long as the character is to be synchronized within one frame if player not moving";
            obj.staticSyncFrameMultiply = EditorGUILayout.FloatField(content, obj.staticSyncFrameMultiply);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Static Sync Position Diff Tolerance");
            content.tooltip = "The minimum distance at which the position will be synchronized if player is not moving";
            obj.staticSyncPositionDiffTolerance = EditorGUILayout.FloatField(content, obj.staticSyncPositionDiffTolerance);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            GUILayout.EndVertical();
            
            GUI.backgroundColor = Color.blue;
            GUILayout.BeginVertical("Mob/Npc Params", topStyle);
            GUI.backgroundColor = Color.white;
            GUILayout.Space(20);

            content = new GUIContent("Resync Mobs");
            content.tooltip = "If the mob is sufficiently away (15) from the first point, update the position";
            obj.resyncMobs = EditorGUILayout.Toggle(content, obj.resyncMobs);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Smooth Rotation");
            content.tooltip = "How fast the character should rotate to achieve the target rotation (must be greater than 0)";
            obj.smoothRotation = EditorGUILayout.FloatField(content, obj.smoothRotation);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
  
            content = new GUIContent("Spawn Mob Additional Y");
            content.tooltip = "Spawn Mob Additional Y";
            obj.spawnMobAdditionalY = EditorGUILayout.FloatField(content, obj.spawnMobAdditionalY);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            content = new GUIContent("Mob Load Frame Skip");
            content.tooltip = "The number of frames in between will spawn a new mob";
            obj.mobLoadFrameSkip = EditorGUILayout.IntField(content, obj.mobLoadFrameSkip);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            content = new GUIContent("Despawn Delay");
            content.tooltip = "Additional time before the object is destroyed on the client side";
            obj.despawnDelay = EditorGUILayout.FloatField(content, obj.despawnDelay);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            GUILayout.EndVertical();
            
        


            GUILayout.Space(2);

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(obj);
                EditorUtility.SetDirty(target);
            }
        }



        public int GetOptionPosition(int id, int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == id)
                    return i;
            }

            return 0;
        }
    }
}