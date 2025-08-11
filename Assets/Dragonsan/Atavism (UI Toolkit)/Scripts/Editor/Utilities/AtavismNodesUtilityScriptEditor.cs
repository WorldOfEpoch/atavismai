using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Atavism
{
    [CustomEditor(typeof(AtavismNodesUtilityScript))]
    public class AtavismNodesDebugScriptEditor : UnityEditor.Editor
    {
        AtavismNodesUtilityScript script;

        bool showNodeProperties;
        bool showPropertiesWithT;

        Dictionary<string, object> m_customProperties;
        List<string> m_customList;

        /// <summary>
        /// OnEnable
        /// </summary>
        public void OnEnable()
        {
            script = (AtavismNodesUtilityScript)target;
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10f);

            if (script.ObjectNode != null)
            {
                GUILayout.BeginHorizontal();
                showNodeProperties = EditorGUILayout.Toggle("Node " + script.ObjectNode.Oid + " (" + script.ObjectNode.Properties.Count + ")", showNodeProperties);
                showPropertiesWithT = EditorGUILayout.Toggle("Show _t", showPropertiesWithT);
                GUILayout.EndHorizontal();

                if (showNodeProperties)
                    DrawDictionary(script.ObjectNode.Properties, "", showPropertiesWithT ? "" : "_t");
            }
        }

        #region Static Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="list"></param>
        public static void SaveObjectReferenceToSerializedProperty(SerializedProperty property, List<Object> list)
        {
            if (property == null)
            {
                Debug.LogError("Missing property.");
                return;
            }
            if (list == null || (list != null && list.Count == 0))
            {
                Debug.LogError("List is null or empty.");
                return;
            }

            property.ClearArray();
            property.Next(true); // skip generic field
            property.Next(true); // advance to array size field
            property.arraySize = list.Count;
            property.Next(true); // advance to first array index

            for (int n = 0; n < list.Count; n++)
            {
                property.objectReferenceValue = list[n];
                if (n < list.Count - 1)
                    property.Next(false);
            }
        }

        /// <summary>
        /// Example SaveStringArrayToSerializedProperty("sfxDeath", FindAssetsInAssetDatabase(script.PathSFXDeath, "AudioClip", FindAssetReturnType.AssetPath, "Assets/Resources/", "."));
        /// </summary>
        /// <param name="property"></param>
        /// <param name="stringArray"></param>
        public static void SaveStringArrayToSerializedProperty(SerializedProperty property, string[] stringArray)
        {
            if (property == null)
            {
                Debug.Log("Missing property.");
                return;
            }

            if (!property.isArray)
            {
                Debug.Log("Property is not an Array.");
                return;
            }

            property.ClearArray();
            property.Next(true); // skip generic field
            property.Next(true); // advance to array size field
            property.arraySize = stringArray.Length;
            property.Next(true); // advance to first array index

            for (int n = 0; n < stringArray.Length; n++)
            {
                if (string.IsNullOrEmpty(stringArray[n]))
                    property.stringValue = "";
                else
                    property.stringValue = stringArray[n];
                if (n < stringArray.Length - 1)
                    property.Next(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        public static void DrawList(List<string> list)
        {
            GUILayout.Space(2f);
            GUILayout.BeginVertical();
            for (int n = 0; n < list.Count; n++)
                DrawListItem(list[n]);
            GUILayout.EndVertical();
        }
        public static void DrawList(List<int> list)
        {
            List<string> newStringList = new List<string>();
            foreach (int i in list)
                newStringList.Add(i.ToString());
            DrawList(newStringList);
        }
        public static void DrawList(List<long> list)
        {
            List<string> newStringList = new List<string>();
            foreach (int i in list)
                newStringList.Add(i.ToString());
            DrawList(newStringList);
        }

        public static void DrawListItem(string item)
        {
            GUILayout.BeginHorizontal();
            GUILayout.TextField(item, GUILayout.MinWidth(160), GUILayout.MaxWidth(160));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        public static void DrawDictionary(Dictionary<string, object> dictionary, string skipIfKeyContains = "", string skipIfLastCharsAre = "")
        {
            GUILayout.Space(2f);
            GUILayout.BeginVertical();

            List<string> listOfKeys = dictionary.Keys.ToList();
            listOfKeys.Sort();
            for (int n = 0; n < listOfKeys.Count; n++)
            {
                if (skipIfKeyContains != "" && listOfKeys[n].Contains(skipIfKeyContains))
                    continue;
                if (skipIfLastCharsAre != "")
                    if (listOfKeys[n].Length >= skipIfLastCharsAre.Length)
                        if (listOfKeys[n].Substring(listOfKeys[n].Length - skipIfLastCharsAre.Length, skipIfLastCharsAre.Length) == skipIfLastCharsAre)
                            continue;

                DrawDictionaryItem(listOfKeys[n], dictionary[listOfKeys[n]]);
            }
            GUILayout.EndVertical();
        }
        public static void DrawDictionary(Dictionary<int, object> dictionary, string skipIfKeyContains = "", string skipIfLastCharsAre = "")
        {
            Dictionary<string, object> newDictionaryWithString = new Dictionary<string, object>();
            List<int> listOfKeys = dictionary.Keys.ToList();
            foreach (int i in listOfKeys)
                newDictionaryWithString[i.ToString()] = dictionary[i];

            DrawDictionary(newDictionaryWithString);
        }

        public static void DrawDictionaryItem(string key, object value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.TextField(key, GUILayout.MinWidth(160), GUILayout.MaxWidth(160));
            if (value != null)
                GUILayout.TextField(value.ToString(), GUILayout.MinWidth(160), GUILayout.MaxWidth(160));
            GUILayout.EndHorizontal();
        }
        #endregion
    }
}