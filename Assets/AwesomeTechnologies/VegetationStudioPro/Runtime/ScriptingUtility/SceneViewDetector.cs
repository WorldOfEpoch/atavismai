#if UNITY_EDITOR
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    [InitializeOnLoad]
    public class SceneViewDetector
    {
        private static float3 position;
        private static quaternion rotation;
        private static SceneView currentSceneView;
        private static EditorWindow currentEditorWindow;

        public delegate void MultihangedSceneViewCameraDelegate(Camera _sceneviewCamera);
        public static MultihangedSceneViewCameraDelegate OnChangedSceneViewCameraDelegate;

        public delegate void MultiSceneViewTransformChangeDelegate(Camera _sceneviewCamera);
        public static MultiSceneViewTransformChangeDelegate OnSceneViewTransformChangeDelegate;

        static SceneViewDetector()
        {
            EditorApplication.update += UpdateEditorCallback;
        }

        ~SceneViewDetector()
        {
            EditorApplication.update -= UpdateEditorCallback;
        }

        public static Camera GetCurrentSceneViewCamera()
        {
            if (currentSceneView != null)
                return currentSceneView.camera;

            Camera[] sceneViewCameras = SceneView.GetAllSceneCameras();
            return sceneViewCameras.Length > 0 ? sceneViewCameras[0] : null;
        }

        private static void UpdateEditorCallback()
        {
            if (currentSceneView && currentSceneView.camera)
                if (position.Equals(currentSceneView.camera.transform.position) == false || rotation.Equals(currentSceneView.camera.transform.rotation) == false)
                {
                    rotation = currentSceneView.camera.transform.rotation;
                    position = currentSceneView.camera.transform.position;
                    OnSceneViewTransformChangeDelegate?.Invoke(currentSceneView.camera);
                }

            if (currentEditorWindow == EditorWindow.focusedWindow)
                return;

            currentEditorWindow = EditorWindow.focusedWindow;
            SceneView view = currentEditorWindow as SceneView;

            if (view != null)
                if (currentSceneView != view)
                {
                    currentSceneView = view;
                    OnChangedSceneViewCameraDelegate?.Invoke(currentSceneView.camera);
                }
        }
    }
}
#endif