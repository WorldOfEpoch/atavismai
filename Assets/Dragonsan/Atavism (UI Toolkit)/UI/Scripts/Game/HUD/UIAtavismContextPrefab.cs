using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Atavism;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismContextPrefab 
    {
        public VisualElement contextObject; //Used to hide / set active when nearby.
        [HideInInspector] public ContextInfo contextInfo;

        [Space(10)] // 10 pixels of spacing here.
        public UIFillerElement interactFiller;

        [Space(10)] // 10 pixels of spacing here.
        public Label contextName;

        public VisualElement contextImage;
        //[HideInInspector] public Sprite contextSprite;

        [Space(10)] // 10 pixels of spacing here.
        public Label contectButton;

        [Space(10)] // 10 pixels of spacing here.
        public VisualElement contextInteract; //Displays interaction key and Text    

        [Space(10)] // 10 pixels of spacing here.
        public VisualElement contextInteractRow1;

        public Label contextInteractRow1Text;

        [Space(10)] // 10 pixels of spacing here.
        public VisualElement contextInteractRow2;

        public Label contextInteractRow2Text;

        [Space(10)] // 10 pixels of spacing here.
        public bool playerInRange = false;

        public bool isFocused = false;

        private Vector3 screenPos;

        public VisualElement m_Root { get; set; }
    
        public UIAtavismContextPrefab(VisualElement visualElement)
        {
            m_Root = visualElement.Q<VisualElement>("root");
            contextObject = m_Root.Q<VisualElement>("contextObject");
            contextInteract = m_Root.Q<VisualElement>("InteractObject");
            
            contextName = m_Root.Q<Label>("ContextName");
            
            contextImage = m_Root.Q<VisualElement>("contextObject");//????
            contextInteractRow1 = m_Root.Q<VisualElement>("InteractRow-1");
            
            contectButton = contextInteractRow1.Q<Label>("InteractButton");
            interactFiller = contextInteractRow1.Q<UIFillerElement>("InteractFiller");
            contextInteractRow1Text = contextInteractRow1.Q<Label>("InteractText");
            
            if(contextObject == null)Debug.LogError("contextObject element could not be found");
            if(contextInteract == null)Debug.LogError("contextInteract element could not be found");
            if(contextName == null)Debug.LogError("contextName element could not be found");
            if(interactFiller == null)Debug.LogError("InteractFiller element could not be found");
            if(contextInteractRow1Text == null)Debug.LogError("contextInteractRow1Text element could not be found");
            
            m_Root.schedule.Execute(Update).Every(10);
        }

        public void Delete()
        {
            m_Root.RemoveFromHierarchy();
            
        }
        
     
        void OnEnable()
        {
            Hide();
         //   m_Root.schedule.Execute(Update).Every(100);
        }

        void OnDisable()
        {
            Hide();
        }

        // Update is called once per frame
      public void Update()
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

          
                if (contextInfo != null)
                {
                    screenPos = Camera.main.WorldToViewportPoint(contextInfo.getPointPosition());
                }
              //  Debug.LogError("AtavismGroundItemsContextPrefab: screenPos="+screenPos);
                float canvasWidth = m_Root.parent.resolvedStyle.width;
                float canvasHeight = m_Root.parent.resolvedStyle.height;
                if (contextObject != null && contextInfo != null)
                {
                    contextObject.style.left = screenPos.x * canvasWidth;
                    contextObject.style.top = canvasHeight - screenPos.y * canvasHeight;
                }

                if (contextInteract != null && contextInfo != null)
                {
                    contextInteract.style.left = screenPos.x * canvasWidth + contextInfo.contextInteractXOffset;
                    contextInteract.style.top = canvasHeight - screenPos.y * canvasHeight + contextInfo.contextInteractXOffset;
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
            if (contextInteractRow1Text != null)
            {
                contextInteractRow1Text.text =text;
            }
        }

        public void SetInteractRow2Text(string text)
        {
            if (contextInteractRow2Text != null)
            {
                contextInteractRow2Text.text =text;
            }
        }

        public void SetContextName(string name)
        {
            if (contextInfo != null)
            {
                contextInfo.contextNameString = name;
                if (contextName != null)
                {
                    contextName.text = contextInfo.contextNameString;
                }
            }
        }

        public void Hide()
        {
            if (contextObject != null)
            {
                contextObject.HideVisualElement();
            }

            if (contextInteract != null)
            {
                contextInteract.HideVisualElement();
            }
        }

        public void ShowInteract()
        {
            if (contextInteract != null)
            {
                contextInteract.ShowVisualElement();
            }
        }

        public void ShowContext()
        {
            if (contextObject != null)
            {
                if (contextInfo != null && contextInfo.contextSpriteIcon != null)
                {
                    contextImage.SetBackgroundImage(contextInfo.contextSpriteIcon);
                }

                contextObject.ShowVisualElement();
            }
        }

        public void HideInteract()
        {
            if (contextInteract != null)
            {
                contextInteract.HideVisualElement();
            }
        }

        public void HideContext()
        {
            if (contextObject != null)
            {
                contextObject.HideVisualElement();
            }
        }
    }
}