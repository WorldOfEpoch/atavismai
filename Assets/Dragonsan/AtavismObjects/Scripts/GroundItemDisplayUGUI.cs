using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Atavism
{


    public class GroundItemDisplayUGUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image background;
        public Color32 backgroundDefaultColor;
        public Color32 backgroundSelectedColor;
        public TextMeshProUGUI text;

        private Vector3 screenPos;

       [HideInInspector] public RectTransform rect;
        public GroundItemDisplay groundItemDisplay;

        public float displayTime = 5f;
        private float showTime = 0;
        // Start is called before the first frame update
        void Start()
        {
            if (background)
                background.color = backgroundDefaultColor;
            showTime = Time.time;
            rect = transform.GetComponent<RectTransform>();
            Show();
        }

        private bool keydown = false;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt)||Input.GetKeyDown(KeyCode.RightAlt))
            {
                showTime = Time.time;
                keydown = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftAlt)||Input.GetKeyUp(KeyCode.RightAlt))
            {
                showTime = Time.time;
                keydown = false;
            }

            if (keydown)
            {
                showTime = Time.time;
            }

            if (showTime + displayTime > Time.time)
            {

                if (groundItemDisplay != null)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
            else
            {
                Hide();
            }
        }
        
        public void Hide()
        {
            if (background != null)
            {
                background.enabled = false;
            }

            if (text)
                text.enabled = false;
        }
        
        public void Show()
        {
            if (background != null)
            {
                background.enabled = true;
            }
            if (text)
                text.enabled = true;

        }
       
        public void OnPointerClick(PointerEventData eventData)
        {
            groundItemDisplay.Loot();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (background != null)
            {
                background.color = backgroundSelectedColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (background != null)
            {
                background.color = backgroundDefaultColor;
            }
        }
    }
}