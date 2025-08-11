using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismGroundItemDisplay //: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public VisualElement background;
        // public Color32 backgroundDefaultColor;
        // public Color32 backgroundSelectedColor;
        public Label text;

        private Vector3 screenPos;

       // [HideInInspector] public RectTransform rect;
        public GroundItemDisplay groundItemDisplay;

        public float displayTime = 5f;
        private float showTime = 0;
        // Start is called before the first frame update
        public UIAtavismGroundItemDisplay(VisualElement visualElement, GroundItemDisplay groundItemDisplay)
        {
            this.groundItemDisplay = groundItemDisplay;
            m_Root = visualElement.Q<VisualElement>("background");;
            background = m_Root;
            text = visualElement.Q<Label>("label");
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            m_Root.RegisterCallback<ClickEvent>(OnPointerClick);
            showTime = Time.time;
            m_Root.schedule.Execute(Update).Every(10);
            // rect = transform.GetComponent<RectTransform>();
            Show();     
        }

        public void Delete()
        {
            m_Root.RemoveFromHierarchy();
        }
        
        public VisualElement m_Root { get; set; }

        public void Setup()
        {
                 
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
                background.HideVisualElement();
            }

            if (text!=null)
                text.HideVisualElement();
        }
        
        public void Show()
        {
            if (background != null)
            {
                background.ShowVisualElement();
            }
            if (text!=null)
                text.ShowVisualElement();

        }
       
        public void OnPointerClick(ClickEvent evt)
        {
            groundItemDisplay.Loot();
        }

        public void OnPointerEnter(MouseEnterEvent mouseEnterEvent)
        {
            
            // Debug.LogError("OnPointerEnter");
            //
            // if (background != null)
            // {
            //     background.AddToClassList("ground-item-selected");
            // }
        }

        public void OnPointerExit(MouseLeaveEvent mouseLeaveEvent)
        {
            // Debug.LogError("OnPointerExit");
            // if (background != null)
            // {
            //     background.RemoveFromClassList("ground-item-selected");
            // }
        }
    }
}