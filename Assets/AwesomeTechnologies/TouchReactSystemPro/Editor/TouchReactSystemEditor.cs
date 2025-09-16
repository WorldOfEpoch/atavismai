using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [CustomEditor(typeof(TouchReactSystem))]
    public class TouchReactSystemEditor : TouchReactBaseEditor
    {
        private int _currentTabIndex;
        private readonly string[] TabNames = { "Settings", "Debug" };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginVertical("box");
            _currentTabIndex = GUILayout.SelectionGrid(_currentTabIndex, TabNames, 2, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            switch (_currentTabIndex)
            {
                case 0:
                    DrawSettingsInspector();
                    break;
                case 1:
                    DrawDebugInspector();
                    break;
            }
        }

        void DrawSettingsInspector()
        {
            TouchReactSystem touchReactSystem = (TouchReactSystem)target;

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Info", LabelStyle);
            EditorGUILayout.HelpBox("The \"TouchReactSystem\" bends vegetation using supported shaders within range of the selected camera\nOnly one instance per scene should be used", MessageType.Info);
            EditorGUILayout.HelpBox("Simply add a \"TouchReactCollider\" or \"TouchReactMesh\" to any GameObject to affect vegetation", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Settings", LabelStyle);
            EditorGUI.BeginChangeCheck();
            touchReactSystem.AutoselectCamera = EditorGUILayout.Toggle("Auto select camera", touchReactSystem.AutoselectCamera);
            if (!touchReactSystem.AutoselectCamera)
                touchReactSystem.SelectedCamera = EditorGUILayout.ObjectField("Camera", touchReactSystem.SelectedCamera, typeof(Camera), true) as Camera;
            EditorGUILayout.HelpBox("Select the active camera to follow or let the system automatically decide", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                touchReactSystem.Init();
                EditorUtility.SetDirty(touchReactSystem);
            }

            if (touchReactSystem.SelectedCamera == null)
                EditorGUILayout.HelpBox("A valid camera needs to be selected", MessageType.Error);

            EditorGUI.BeginChangeCheck();
            //touchReactSystem.InvisibleLayer = EditorGUILayout.IntSlider("Touch React layer", touchReactSystem.InvisibleLayer, 0, 31);
            //EditorGUILayout.HelpBox("Select a layer not visible by any other camera. This is used to render a touch buffer for selected colliders and meshes within range.", MessageType.Info);

            touchReactSystem.Speed = EditorGUILayout.Slider("Speed", touchReactSystem.Speed, 0, 10);
            EditorGUILayout.HelpBox("This sets the speed at which the vegetation grows back to its original state", MessageType.Info);

            touchReactSystem.TouchReactQuality = (TouchReactQuality)EditorGUILayout.EnumPopup("Buffer resolution", touchReactSystem.TouchReactQuality);
            EditorGUILayout.HelpBox("Pixel resolution of the touch react buffer\nLow = 512x512, Normal = 1024x1024 and High = 2048x2048", MessageType.Info);

            touchReactSystem.OrthographicSize = EditorGUILayout.IntSlider("Affected area", touchReactSystem.OrthographicSize, 10, 500);
            EditorGUILayout.HelpBox("The area around the camera affected by touch react\nIncreasing the range reduces the accuracy/resolution of the mask", MessageType.Info);

            float resolution = (float)touchReactSystem.OrthographicSize / touchReactSystem.GetTouchReactQualityPixelResolution(touchReactSystem.TouchReactQuality);
            EditorGUILayout.LabelField("Current resolution " + resolution.ToString("F2") + " meter per pixel", LabelStyle);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                touchReactSystem.UpdateCamera();
                EditorUtility.SetDirty(touchReactSystem);
            }
        }

        void DrawDebugInspector()
        {
            TouchReactSystem touchReactSystem = (TouchReactSystem)target;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Debug", LabelStyle);
            touchReactSystem.ShowDebugColliders = EditorGUILayout.Toggle("Show used colliders/meshes", touchReactSystem.ShowDebugColliders);

            EditorGUI.BeginChangeCheck();
            touchReactSystem.HideTouchReactCamera = EditorGUILayout.Toggle("Hide touch react camera", touchReactSystem.HideTouchReactCamera);
            EditorGUILayout.HelpBox("When enabled the touch react camera will be hidden in the hierarchy", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
                touchReactSystem.UpdateTouchReactCamera();

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(touchReactSystem);

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Colliders", LabelStyle);
            EditorGUILayout.LabelField("Collider count: " + touchReactSystem.ColliderList.Count.ToString(), LabelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Meshes", LabelStyle);
            EditorGUILayout.LabelField("Mesh count: " + touchReactSystem.MeshFilterList.Count.ToString(), LabelStyle);
            GUILayout.EndVertical();
        }
    }
}