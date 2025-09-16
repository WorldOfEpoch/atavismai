#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AwesomeTechnologies.TouchReact
{
    [InitializeOnLoad]
    public class TouchReactSceneViewDetector : MonoBehaviour
    {
        private static SceneView currentSceneView;
        private static EditorWindow currentEditorWindow;

        public delegate void MultiVegetationCellRefreshDelegate(Camera _sceneviewCamera);
        public static MultiVegetationCellRefreshDelegate OnChangedSceneViewCameraDelegate;

        static TouchReactSceneViewDetector()
        {
            EditorApplication.update += UpdateEditorCallback;
        }

        ~TouchReactSceneViewDetector()
        {
            EditorApplication.update -= UpdateEditorCallback;
        }

        private static void UpdateEditorCallback()
        {
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

        public static Camera GetCurrentSceneViewCamera()
        {
            if (currentSceneView != null)
                return currentSceneView.camera;

            Camera[] sceneviewCameras = SceneView.GetAllSceneCameras();
            return sceneviewCameras.Length > 0 ? sceneviewCameras[0] : null;
        }
    }
}
#endif