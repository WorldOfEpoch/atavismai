using AwesomeTechnologies.VegetationStudio;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/RuntimeLoaders/CameraLoader")]
    [ExecuteInEditMode]
    public class CameraLoader : MonoBehaviour
    {
        public Camera Camera;
        public bool noFrustumCulling;
        public bool renderDirectToCamera = true;
        public bool renderBillboardsOnly;

        private void Reset()
        {
            if (Camera == null)
                Camera = GetComponent<Camera>();
        }

        void OnEnable()
        {
            if (Camera == null)
                return;

            VegetationStudioManager.AddCamera(Camera, noFrustumCulling, renderDirectToCamera, renderBillboardsOnly);
        }

        void OnDisable()
        {
            if (Camera == null)
                return;

            VegetationStudioManager.RemoveCamera(Camera);
        }
    }
}