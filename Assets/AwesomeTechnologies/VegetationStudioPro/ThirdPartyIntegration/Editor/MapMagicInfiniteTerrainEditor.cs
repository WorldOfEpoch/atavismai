#if VEGETATION_STUDIO_PRO && VSP_PACKAGES
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.External.MapMagicInterface
{
    [CustomEditor(typeof(MapMagicInfiniteTerrain))]
    public class MapMagicInfiniteTerrainEditor : VegetationStudioProBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if MAPMAGIC
            EditorGUILayout.HelpBox("Map Magic 1 installed", MessageType.Info);
#else
            EditorGUILayout.HelpBox("Map Magic 1 not detected", MessageType.Error);
#endif
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("This component adds the \"UnityTerrain\" component to all run-time created terrains of MapMagic 1\nAdd it to any GameObject in the same scene as \"VegetationStudioPro\"", MessageType.Info);
            GUILayout.EndVertical();
        }
    }
}
#endif