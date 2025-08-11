using System;
using Atavism;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismFPS : MonoBehaviour
    {
        public static UIAtavismFPS instance;

        [AtavismSeparator("UI")] [SerializeField]
        private UIDocument uiDocument;

        public static UIAtavismFPS Instance => instance;
        private VisualElement uiProgressScreen;
        private float fps;
        private float timeleft;
        private int frames;
        private float accum;
        private float updateInterval;
        private Label UIFPSNameLabel, UIFPSLabel;

        public string fpsnamelabel;
        public string fpsvaluelabel;

        private void OnEnable()
        {
            registerUI();
        }

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }


        private void Update()
        {
            if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetVideoSettings().fps)
            {
                uiProgressScreen.ShowVisualElement();
                timeleft -= Time.deltaTime;
                accum += Time.timeScale / Time.deltaTime;
                ++frames;

                // Interval ended - update GUI text and start new interval
                if (timeleft <= 0.0)
                {
                    // display two fractional digits (f2 format)
                    fps = (accum / frames);
                    UIFPSLabel.text = fps.ToString("F0", CultureInfo.InvariantCulture);
                    timeleft = updateInterval;
                    accum = 0.0f;
                    frames = 0;
                }
            }
            else
            {
                uiProgressScreen.HideVisualElement();
            }
        }

        private void registerUI()
        {
            if(uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            uiProgressScreen = uiDocument.rootVisualElement.Query<VisualElement>("Screen");
            UIFPSNameLabel = uiDocument.rootVisualElement.Query<Label>(fpsnamelabel);
            UIFPSLabel = uiDocument.rootVisualElement.Query<Label>(fpsvaluelabel);


        }

        private void Awake()
        {
            instance = this;
        }
    }
}