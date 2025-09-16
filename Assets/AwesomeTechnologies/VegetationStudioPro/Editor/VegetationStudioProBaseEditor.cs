using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies
{
    public class VegetationStudioProBaseEditor : Editor
    {
        private bool initSkin = false;
        internal bool showLogo = false;
        internal bool largeLogo = false;
        private Texture2D logoTexture;
        internal string overrideLogoTextureName;
        private Texture2D overrideLogoTextureSmall;
        internal GUIStyle labelStyle;

        private void InitSkin()
        {
            logoTexture = Resources.Load<Texture2D>("Banners/AWESOME_Vegetation_Studio_Pro_Editor");
            labelStyle = new GUIStyle("Label") { fontStyle = FontStyle.Italic };
            labelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f) : new Color(0f, 0f, 0f);
            initSkin = true;
        }

        public override void OnInspectorGUI()
        {
            if (initSkin == false)
                InitSkin();

            if (showLogo == false)
                return;

            EditorGUIUtility.labelWidth = 200;

            Texture2D selectedLogoTexture = logoTexture;

            if (overrideLogoTextureName != "")
            {
                if (overrideLogoTextureSmall == null)
                    overrideLogoTextureSmall = Resources.Load<Texture2D>(overrideLogoTextureName);
                else
                    selectedLogoTexture = overrideLogoTextureSmall;
            }

            GUILayoutUtility.GetRect(1, 3, GUILayout.ExpandWidth(false));
            Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(selectedLogoTexture.height));
            float width = space.width;

            space.xMin = (width - selectedLogoTexture.width + 18) / 2;
            if (space.xMin < 0)
                space.xMin = 0;

            space.width = selectedLogoTexture.width;
            space.height = selectedLogoTexture.height;
            GUI.DrawTexture(space, selectedLogoTexture, ScaleMode.ScaleToFit, true, 0);

            if (largeLogo)
                EditorGUILayout.LabelField("Version: VSP-Beyond v1.0.9 by VMTB|Klani", labelStyle);

            GUILayoutUtility.GetRect(1, 3, GUILayout.ExpandWidth(false));
        }
    }
}