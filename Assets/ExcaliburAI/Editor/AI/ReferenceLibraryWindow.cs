#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace ExcaliburAI.AI
{
    public class ReferenceLibraryWindow : EditorWindow
    {
        ReferenceConfig cfg;
        List<string> images = new List<string>();
        Vector2 scroll;
        int columns = 4;
        Texture2D[] thumbs = new Texture2D[0];

        [MenuItem("ExcaliburAI/Reference Library")]
        public static void Open()
        {
            var w = GetWindow<ReferenceLibraryWindow>("Reference Library");
            w.minSize = new Vector2(680, 520);
        }

        void OnEnable()
        {
            cfg = ReferenceLibrary.Load();
            Refresh();
        }

        void Refresh()
        {
            images = ReferenceLibrary.ScanImages(cfg.referenceFolder);
            thumbs = new Texture2D[images.Count];
            for (int i=0;i<images.Count;i++)
            {
                var tex = new Texture2D(2,2);
                try { tex.LoadImage(System.IO.File.ReadAllBytes(images[i])); }
                catch {}
                thumbs[i] = tex;
            }
            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            cfg.referenceFolder = EditorGUILayout.TextField("Reference Folder", cfg.referenceFolder);
            if (GUILayout.Button("Browse...", GUILayout.Width(100)))
            {
                var p = EditorUtility.OpenFolderPanel("Select reference folder", cfg.referenceFolder, "");
                if (!string.IsNullOrEmpty(p)) cfg.referenceFolder = p;
                ReferenceLibrary.Save(cfg);
                Refresh();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Rescan"))
            {
                Refresh();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Active Reference: " + (string.IsNullOrEmpty(cfg.activeReference) ? "(none)" : Path.GetFileName(cfg.activeReference)));

            scroll = EditorGUILayout.BeginScrollView(scroll);
            int idx = 0;
            int rows = Mathf.CeilToInt((float)images.Count / columns);
            float size = Mathf.Floor((position.width - 40) / columns) - 12;
            for (int r=0; r<rows; r++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c=0; c<columns; c++)
                {
                    if (idx >= images.Count) { GUILayout.FlexibleSpace(); continue; }
                    var tex = thumbs[idx];
                    var path = images[idx];
                    GUILayout.BeginVertical("box", GUILayout.Width(size), GUILayout.Height(size+36));
                    if (tex != null) GUILayout.Label(tex, GUILayout.Width(size), GUILayout.Height(size));
                    else GUILayout.Label("(image)", GUILayout.Width(size), GUILayout.Height(size));
                    if (GUILayout.Button(Path.GetFileName(path), GUILayout.Height(20)))
                    {
                        cfg.activeReference = path;
                        ReferenceLibrary.Save(cfg);
                    }
                    GUILayout.EndVertical();
                    idx++;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Active Reference"))
            {
                cfg.activeReference = "";
                ReferenceLibrary.Save(cfg);
            }
        }
    }
}
#endif
