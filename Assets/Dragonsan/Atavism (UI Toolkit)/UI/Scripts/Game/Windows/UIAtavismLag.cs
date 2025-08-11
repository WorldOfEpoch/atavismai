using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismLag : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] float updateInterval = 0.5F;
        [SerializeField] Label textOutput;
        [SerializeField] private int lagLimitGreen = 100;
        [SerializeField] private int lagLimitYellow = 500;
        private float timeleft;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        // Use this for initialization
        void OnEnable()
        {
            timeleft = updateInterval;
            uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            textOutput = uiDocument.rootVisualElement.Q<Label>("lag");
        }

        // Update is called once per frame
        void Update()
        {
            timeleft -= Time.deltaTime;
            if (timeleft <= 0.0)
            {
                int lag = Mathf.RoundToInt(NetworkAPI.GetLag() * 1000);;
                string format = System.String.Format("{0} ms", lag);
                if (textOutput != null)
                    textOutput.text = format;
                if (lag < lagLimitGreen)
                {
                    if (textOutput != null)
                    {
                        textOutput.RemoveFromClassList("lag-high");
                        textOutput.RemoveFromClassList("lag-medium");
                        textOutput.AddToClassList("lag-low");
                    }

                }
                else if (lag < lagLimitYellow)
                {
                    if (textOutput != null)
                    {
                        textOutput.RemoveFromClassList("lag-high");
                        textOutput.RemoveFromClassList("lag-low");
                        textOutput.AddToClassList("lag-medium");
                    }
                }
                else
                {
                    if (textOutput != null)
                    {
                        textOutput.RemoveFromClassList("lag-low");
                        textOutput.RemoveFromClassList("lag-medium");
                        textOutput.AddToClassList("lag-high");
                    }

                }

                timeleft = updateInterval;
            }

          
        }

    }
}