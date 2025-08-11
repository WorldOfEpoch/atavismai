using System;
using System.Collections;
using System.Collections.Generic;
using Atavism;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

[RequireComponent(typeof(UIDocument))]
    public class UIAtavismGroundItemCanvas : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;

        // Start is called before the first frame update
        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            GroundLootManager.Instance.uiItemCanvas = uiDocument.rootVisualElement;
        }

        private void OnDestroy()
        {
            GroundLootManager.Instance.uiItemCanvas = null;
        }
    }
}