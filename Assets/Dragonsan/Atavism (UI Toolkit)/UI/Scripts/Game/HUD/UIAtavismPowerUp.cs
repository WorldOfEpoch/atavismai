using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
   public class UIAtavismPowerUp : MonoBehaviour
    {
        UIDocument uiDocument;
        private VisualElement m_progressImage;
        private VisualElement m_backgroundImage;
        [SerializeField] bool progressImageVertical;
        [SerializeField] bool progressImageDirection;
        private UIProgressBar progressBar;
      //  public GameObject gameObject;
        private float m_startTime = 0f;
        private float m_endTime = 0f;
        private float m_time = 0f;
        private float value = 0;    
        private float lowValue = 0;    
        private float highValue = 100;    
        // public bool testImg = false;
            bool showing = false;
        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            RegisterUI();
            if (m_backgroundImage != null)
            {
                m_backgroundImage.HideVisualElement();
            }
            if (progressBar!=null)
            {
                progressBar.HideVisualElement();
            }
            AtavismEventSystem.RegisterEvent("START_POWER_UP", this);
            AtavismEventSystem.RegisterEvent("CANCEL_POWER_UP", this);
            
        }

        private void RegisterUI()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            progressBar = uiDocument.rootVisualElement.Q<UIProgressBar>("progressBar");
            m_backgroundImage = uiDocument.rootVisualElement.Q<VisualElement>("backgroundImage");
            m_progressImage = uiDocument.rootVisualElement.Q<VisualElement>("progressImage");
           if(m_progressImage != null) m_progressImage.RegisterCallback<GeometryChangedEvent>(new EventCallback<GeometryChangedEvent>(this.OnGeometryChanged));
        }
        private void OnGeometryChanged(GeometryChangedEvent e) => this.SetProgress(value);
        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("START_POWER_UP", this);
            AtavismEventSystem.UnregisterEvent("CANCEL_POWER_UP", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
//            Debug.LogError("UGUIPowerUp "+eData.eventType);
            if (eData.eventType == "START_POWER_UP")
            {
                m_startTime = Time.time;
                long t = long.Parse(eData.eventArgs[1]);
                m_endTime = m_startTime + (t - 100) / 1000F;

              
            }
            else if (eData.eventType == "CANCEL_POWER_UP")
            {
                Hide();
            }

        }

        // Update is called once per frame
        void Update()
        {
            // if (testImg)
            // {
            //     testImg = false;
            //     m_startTime = Time.time;
            //     long t = 10000;
            //     m_endTime = m_startTime + t / 1000F;
            //     if (m_backgroundImage != null)
            //     {
            //         m_backgroundImage.ShowVisualElement();
            //     }
            //
            //     if (progressBar != null)
            //     {
            //         progressBar.ShowVisualElement();
            //     }
            //     
            // }

            if (showing)
            {
                highValue = m_endTime - m_startTime;
                value = Time.time - m_startTime;
                if (progressBar != null)
                {
                    progressBar.lowValue = 0;
                    progressBar.highValue = highValue;
                    progressBar.value = value;
                }

                SetProgress(value);
            }

            if (Time.time > m_endTime && showing)
            {
                Hide();
            }
        }

        void Hide()
        {
            showing = false;
            if (m_backgroundImage != null)
            {
                m_backgroundImage.HideVisualElement();
            }
            if (progressBar!=null)
            {
                progressBar.HideVisualElement();
            }
        }

        void Show()
        {
            showing = true;
            if (m_backgroundImage != null)
            {
                m_backgroundImage.ShowVisualElement();
            }

            if (progressBar != null)
            {
                progressBar.ShowVisualElement();
            }
        }
        
        private void SetProgress(float p)
        {
            if (this.m_backgroundImage == null || this.m_progressImage == null)
            {
                Debug.LogError("Please assign the background image and progress image first!");
                return;
            }

            if (progressImageVertical)
            {
                float progresHeight = this.CalculateProgressHeight((double)p >= (double)this.lowValue
                    ? ((double)p <= (double)this.highValue ? p : this.highValue)
                    : this.lowValue);
                if ((double)progresHeight < 0.0)
                    return;
                if(progressImageDirection)
                    this.m_progressImage.style.top = (StyleLength)progresHeight;
                else
                    this.m_progressImage.style.bottom = (StyleLength)progresHeight;
            }
            else
            {
                float progressWidth = this.CalculateProgressWidth((double)p >= (double)this.lowValue
                    ? ((double)p <= (double)this.highValue ? p : this.highValue)
                    : this.lowValue);
                if ((double)progressWidth < 0.0)
                    return;
                if(progressImageDirection)
                    this.m_progressImage.style.right = (StyleLength)progressWidth;
                else
                    this.m_progressImage.style.left = (StyleLength)progressWidth;
            }
        }

        private float CalculateProgressWidth(float width)
        {
            if (this.m_backgroundImage == null || this.m_progressImage == null)
                return 0.0f;
            Rect layout = this.m_backgroundImage.layout;
            if (float.IsNaN(layout.width))
                return 0.0f;
            layout = this.m_backgroundImage.layout;
            float num = layout.width - 2f;
            return num - Mathf.Max(num * width / this.highValue, 1f);
        }
        private float CalculateProgressHeight(float height)
        {
            if (this.m_backgroundImage == null || this.m_progressImage == null)
                return 0.0f;
            Rect layout = this.m_backgroundImage.layout;
            if (float.IsNaN(layout.height))
                return 0.0f;
            layout = this.m_backgroundImage.layout;
            float num = layout.height - 2f;
            return num - Mathf.Max(num * height / this.highValue, 1f);
        }
        
    }
}