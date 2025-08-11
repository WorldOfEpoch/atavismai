using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismCrosshair : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnDestroy()
        {
            unregisterEvents();
        }

        private void Reset()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            _uiDocument.enabled = true;
            
            registerEvents();
        }

        protected void registerEvents()
        {

            AtavismEventSystem.RegisterEvent("CROSSHAIR", this);
        }

        protected void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("CROSSHAIR", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CROSSHAIR")
            {
                if (eData.eventArgs[0].Equals("Show"))
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        void Show()
        {
            _uiDocument.rootVisualElement.ShowVisualElement();
        }
        void Hide()
        {
            _uiDocument.rootVisualElement.HideVisualElement();
        }
    }
}