using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Atavism;
using TMPro;
using UnityEngine.UI;

namespace Atavism
{
    public class ContextPrefab : MonoBehaviour
    {
        public GameObject contextObject; //Used to hide / set active when nearby.
        [HideInInspector] public ContextInfo contextInfo;

        [Space(10)] // 10 pixels of spacing here.
        public Image interactFiller;

        [Space(10)] // 10 pixels of spacing here.
        public TextMeshProUGUI contextNameTMP;

        public Image contextImage;
        //[HideInInspector] public Sprite contextSprite;

        [Space(10)] // 10 pixels of spacing here.
        public TextMeshProUGUI buttonText;

        [Space(10)] // 10 pixels of spacing here.
        public GameObject contextInteract; //Displays interaction key and Text    

        [Space(10)] // 10 pixels of spacing here.
        public GameObject contextInteractRow1;

        public TextMeshProUGUI contextInteractRow1TMP;

        [Space(10)] // 10 pixels of spacing here.
        public GameObject contextInteractRow2;

        public TextMeshProUGUI contextInteractRow2TMP;

        [Space(10)] // 10 pixels of spacing here.
        public bool playerInRange = false;

        public bool isFocused = false;

        private Vector3 screenPos;

        void OnEnable()
        {
            Hide();
        }

        void OnDisable()
        {
            Hide();
        }

        // Update is called once per frame
        void Update()
        {
            if (playerInRange && !AtavismSettings.Instance.isWindowOpened())
            {
                if (!isFocused)
                {
                    if (contextInfo != null && !contextInfo.hideContext)
                    {
                        ShowContext();
                    }
                    else
                    {
                        HideContext();
                    }

                    HideInteract();
                }
                else if (isFocused)
                {
                    HideContext();

                    if (contextInfo != null && !contextInfo.hideInteract)
                    {
                        ShowInteract();
                    }
                    else
                    {
                        HideInteract();
                    }
                }

                // if (contextInfo.targetPoint != null)
                // {
                if (contextInfo != null)
                {
                    screenPos = Camera.main.WorldToScreenPoint(contextInfo.getPointPosition());
                }
                // }
                // else
                // {
                //     screenPos = Camera.main.WorldToScreenPoint(contextInfo.centerPointCoords);
                // }


                if (contextObject != null && contextInfo != null)
                {
                    contextObject.transform.position = new Vector3(screenPos.x + contextInfo.contextObjectXOffset, screenPos.y + contextInfo.contextObjectYOffset);
                }

                if (contextInteract != null && contextInfo != null)
                {
                    contextInteract.transform.position = new Vector3(screenPos.x + contextInfo.contextInteractXOffset, screenPos.y + contextInfo.contextInteractYOffset);
                }

                if (contextInfo != null)
                {
                    SetContextName(contextInfo.contextNameString);
                }
            }
            else
            {
                Hide();
            }
        }

        public void SetInteractRow1Text(string text)
        {
            if (contextInteractRow1TMP != null)
            {
                contextInteractRow1TMP.SetText(text);
            }
        }

        public void SetInteractRow2Text(string text)
        {
            if (contextInteractRow2TMP != null)
            {
                contextInteractRow2TMP.SetText(text);
            }
        }

        public void SetContextName(string name)
        {
            if (contextInfo != null)
            {
                contextInfo.contextNameString = name;
                if (contextNameTMP != null)
                {
                    contextNameTMP.SetText(contextInfo.contextNameString);
                }
            }
        }

        public void Hide()
        {
            if (contextObject != null)
            {
                contextObject.SetActive(false);
            }

            if (contextInteract != null)
            {
                contextInteract.SetActive(false);
            }
        }

        public void ShowInteract()
        {
            if (contextInteract != null)
            {
                contextInteract.SetActive(true);
            }
        }

        public void ShowContext()
        {
            if (contextObject != null)
            {
                if (contextInfo != null && contextInfo.contextSpriteIcon != null)
                {
                    contextImage.sprite = contextInfo.contextSpriteIcon;
                }

                contextObject.SetActive(true);
            }
        }

        public void HideInteract()
        {
            if (contextInteract != null)
            {
                contextInteract.SetActive(false);
            }
        }

        public void HideContext()
        {
            if (contextObject != null)
            {
                contextObject.SetActive(false);
            }
        }
    }
}