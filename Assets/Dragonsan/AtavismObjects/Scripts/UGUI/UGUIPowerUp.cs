using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Atavism
{
   public class UGUIPowerUp : MonoBehaviour
    {
        public Image image;
        public GameObject gameObject;
        private float startTime = 0f;
        private float endTime = 0f;
        private float time = 0f;

        // Start is called before the first frame update
        void Start()
        {
            if (image)
            {
                image.fillAmount = 0;
                image.enabled = false;
            }

            AtavismEventSystem.RegisterEvent("START_POWER_UP", this);
            AtavismEventSystem.RegisterEvent("CANCEL_POWER_UP", this);
        }

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
                if (image)
                {
                    image.enabled = true;
                    startTime = Time.time;
                    long t = long.Parse(eData.eventArgs[1]);
                    endTime = startTime + (t-100 )/ 1000F;
                }
                if (gameObject != null && !gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }

            }
            else if (eData.eventType == "CANCEL_POWER_UP")
            {
                if (image)
                {
                    image.fillAmount = 0;
                    image.enabled = false;
                }
            }

            if (gameObject != null && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (image && image.enabled)
            {
                image.fillAmount = (Time.time - startTime) / (endTime - startTime);
            }
        }
    }
}