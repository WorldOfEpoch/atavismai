using System;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Tools
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Tools/Camera/ScreenshotUtility")]
    [ScriptExecutionOrder(200)]
    public class ScreenshotUtility : MonoBehaviour
    {
        public KeyCode screenshotKey = KeyCode.Alpha0;

        void LateUpdate()
        {
            if (Input.GetKeyDown(screenshotKey))
                TakeScreenshot();
        }

        public void TakeScreenshot()
        {
            ScreenCapture.CaptureScreenshot("Assets/Screenshot_" + Guid.NewGuid() + ".png", 1);
            Debug.Log("Screenshot taken!");
        }
    }
}


