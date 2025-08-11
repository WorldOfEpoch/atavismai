using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Atavism
{
   public class UGUIQueue : MonoBehaviour
    {
        public GameObject queuePanel;
        public TextMeshProUGUI countText;
        public TextMeshProUGUI messageText;

        // Start is called before the first frame update
        void Start()
        {
            AtavismEventSystem.RegisterEvent("LOGIN_QUEUE", this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("LOGIN_QUEUE", this);
            
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "LOGIN_QUEUE")
            {
                if (queuePanel)
                {
                    queuePanel.SetActive(true);
                    String count = eData.eventArgs[0];
                    String message = eData.eventArgs[1];

                    if (countText )
                    {
                        if (count.Length > 0)
                        {
#if AT_I2LOC_PRESET
                            countText.text = I2.Loc.LocalizationManager.GetTranslation("You are")+" "+count;
#else
                            countText.text = "You are " + count;
#endif
                        }
                        else
                        {
                            countText.text = "";
                        }
                    }

                    if (messageText)
                    {
                        messageText.text = message;
                    }
                }
            }
        }
    }
}