using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Atavism
{
    [CustomEditor(typeof(Crafting))]
    public class CraftingEditor : Editor
    {
        bool help = false;
        string[] currencyGroup;
        int[] currencyGroupIds;

        public override void OnInspectorGUI()
        {
            Crafting obj = target as Crafting;
            //   var indentOffset = EditorGUI.indentLevel * 5f;
            var lineRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            var labelRect = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.currentViewWidth - 60f, lineRect.height);
            var fieldRect = new Rect(labelRect.xMax, lineRect.y, lineRect.width - labelRect.width - 60f, lineRect.height);
            var buttonRect = new Rect(fieldRect.xMax, lineRect.y, 60f, lineRect.height);
            GUIContent content = new GUIContent("Help");
            content.tooltip = "Click to show or hide help informations";
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
            GUILayout.BeginVertical("Crafting Configuration", topStyle);
            GUILayout.Space(20);
          
            
            content = new GUIContent("Grid Size");
            content.tooltip = "Grid Size";
            obj.gridSize = EditorGUILayout.IntField(content, obj.gridSize);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            
            GUILayout.BeginVertical("Crafting Book Configuration", boxStyle);
            GUILayout.Space(20);
            content = new GUIContent("Show All Skills");
            content.tooltip = "Show All Skills In Crafting Book";
            obj.showAllSkills = EditorGUILayout.Toggle(content, obj.showAllSkills);
            if (help)
                EditorGUILayout.HelpBox(content.tooltip, MessageType.None);

            if (obj.showAllSkills)
            {
                content = new GUIContent("Show Only All Known Skills");
                content.tooltip = "Show Only All Known Skills";
                obj.showAllKnownSkills = EditorGUILayout.Toggle(content, obj.showAllKnownSkills);
                if (help)
                    EditorGUILayout.HelpBox(content.tooltip, MessageType.None);
            }
            GUILayout.EndVertical();


            GUILayout.Space(2);

            GUILayout.EndVertical();
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