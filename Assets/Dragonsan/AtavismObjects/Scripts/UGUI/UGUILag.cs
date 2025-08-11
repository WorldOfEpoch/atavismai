using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Atavism
{

    public class UGUILag : MonoBehaviour
    {

        [SerializeField] float updateInterval = 0.5F;
        [SerializeField] Text textOutput;
        [SerializeField] TextMeshProUGUI TMPTextOutput;
        [SerializeField] private int lagLimitGreen = 100;
        [SerializeField] private int lagLimitYellow = 500;
        private float timeleft;

        // Use this for initialization
        void Start()
        {
            timeleft = updateInterval;
            if (textOutput != null)
                textOutput.material = new Material(textOutput.material);
            if (TMPTextOutput != null)
                TMPTextOutput.material = new Material(TMPTextOutput.material);
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
                if (TMPTextOutput != null)
                    TMPTextOutput.text = format;
                if (lag < lagLimitGreen)
                {
                    if (textOutput != null)
                        textOutput.material.color = Color.green;
                    if (TMPTextOutput != null)
                        TMPTextOutput.color = Color.green;

                }
                else if (lag < lagLimitYellow)
                {
                    if (textOutput != null)
                        textOutput.material.color = Color.yellow;
                    if (TMPTextOutput != null)
                        TMPTextOutput.color = Color.yellow;
                }
                else
                {
                    if (textOutput != null)
                        textOutput.material.color = Color.red;
                    if (TMPTextOutput != null)
                        TMPTextOutput.color = Color.red;

                }

                timeleft = updateInterval;
            }

          
        }

    }
}