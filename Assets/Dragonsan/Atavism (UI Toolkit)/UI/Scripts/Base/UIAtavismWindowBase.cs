using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class UIAtavismWindowBase : MonoBehaviour
    {
        [Flags]
        public enum WindowShowEnum { OnEnable = 2, Start = 4 }
        public event Action onWindowDragEnd;

        [AtavismSeparator("Window Base")]
        [SerializeField] protected bool isVisible;
        public bool IsVisible => isVisible;
        [SerializeField] protected bool isDraggable;
        public bool IsDraggable => isDraggable;
        [SerializeField] protected bool showOnCenter = false;
        public bool ShowOnCenter => showOnCenter;
        [SerializeField] protected bool showUseFade = false;
        public bool ShowUseFade => showUseFade;
        [SerializeField] protected bool saveWindowPosition = false;
        public bool SaveWindowPosition => saveWindowPosition;
        [SerializeField] protected bool hideCursorOnHide = true;
        public bool HideCursorOnHide => hideCursorOnHide;
        [SerializeField] protected bool hideOnEscKey = false;
        public bool HideOnEscKey => hideOnEscKey;
        
        [SerializeField] public UIDocument uiDocument;
        [SerializeField] protected UnityEvent onShowEvent;
        [SerializeField] protected UnityEvent onHideEvent;
        [SerializeField] protected WindowShowEnum autoShow = WindowShowEnum.OnEnable | WindowShowEnum.Start;
        // private float initialWidth;
        // private float initialHeight;
        protected bool isRegisteredUI, isInitialize;
        protected VisualElement uiScreen, uiWindow, uiWindowDraggableTrigger;
        protected Label uiWindowTitle;
        protected Button uiWindowCloseButton;
        public MouseButton DraggingMouseButton = MouseButton.LeftMouse;
        protected bool isDragging;
        public bool IsDragging => isDragging;
        protected Vector2 draggingMinValues, draggingMaxValues, draggingMouseOffset;
        [SerializeField] private string windowElementName = "Window";
        
         [AtavismSeparator("UI")]
         [SerializeField] protected UIDocument uiThisSceneDocument;
        protected bool showing = false;
        #region Initiate
        protected virtual void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        protected virtual void OnEnable()
        {
            if(uiDocument == null)
            uiDocument = GetComponent<UIDocument>();


            Initialize();

            if (autoShow.HasFlag(WindowShowEnum.OnEnable))
                Show();
            else
                Hide();
        }

        protected virtual void OnDisable()
        {
            Deinitialize();
        }

        protected virtual void Start()
        {
            // initialWidth = Screen.width;
            // initialHeight = Screen.height;
            if (autoShow.HasFlag(WindowShowEnum.Start))
                Show();
            else 
                Hide();
        }

        protected virtual void Destroy()
        {
           
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize()
        {
            if (isInitialize)
                return;

            registerUI();
            registerEvents();
            registerExtensionMessages();

            isInitialize = true;
        }

        public virtual void Deinitialize()
        {
            if (!isInitialize)
                return;

            // Hide();

            unregisterExtensionMessages();
            unregisterEvents();
            unregisterUI();

            isInitialize = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool registerUI()
        {
            if (isRegisteredUI)
                return false;
            if(uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            uiScreen = uiDocument.rootVisualElement.Query<VisualElement>("Screen");
            if (uiScreen == null)
                uiScreen = uiDocument.rootVisualElement;
            uiWindow = uiDocument.rootVisualElement.Query<VisualElement>(windowElementName);
            if (uiWindow != null)
            {
                uiWindowTitle = uiWindow.Query<Label>("Window-title");
                uiWindowCloseButton = uiWindow.Query<Button>("Window-close-button");
                uiWindowDraggableTrigger = uiWindow.Query<VisualElement>("Window-Draggable-Trigger");
            }
            else
            {
                uiWindowTitle = uiDocument.rootVisualElement.Query<Label>("Window-title");
                uiWindowCloseButton = uiDocument.rootVisualElement.Query<Button>("Window-close-button");
                uiWindowDraggableTrigger = uiDocument.rootVisualElement.Query<VisualElement>("Window-Draggable-Trigger");
            }
            //uiScreen.AddManipulator(new DragManipulator());

            // Check if any of the UI elements are null and log an error message if they are
            if (uiScreen == null)
                AtavismLogger.LogError("UI Screen element not found.");
            if (uiWindow == null)
                AtavismLogger.LogDebugMessage("UI Window element not found.");
            if (uiWindowTitle == null)
                AtavismLogger.LogDebugMessage("UI Window Title element not found.");
            if (uiWindowCloseButton == null)
                AtavismLogger.LogDebugMessage("UI Window Close Button element not found.");
            if (uiWindowDraggableTrigger == null && !isDraggable)
                AtavismLogger.LogDebugMessage("UI Window Draggable Trigger element not found.");

            // Events
            if (uiWindowCloseButton != null)
            {
                uiWindowCloseButton.clicked += onWindowCloseButtonClicked;
            }
            else
            {
            }

            if (isDraggable && uiWindowDraggableTrigger == null)
                AtavismLogger.LogDebugMessage("Missing draggable-trigger element.");
            else if (uiWindowDraggableTrigger != null)
                uiWindowDraggableTrigger.RegisterCallback<MouseDownEvent>(onDraggableTriggerMouseDown, TrickleDown.TrickleDown);

            if (UIAtavismAudioManager.Instance != null)
                UIAtavismAudioManager.Instance.RegisterSFX(uiDocument);

            isRegisteredUI = true;

            return true;
        }

        protected virtual bool unregisterUI()
        {
            if (!isRegisteredUI)
                return false;

            if (uiWindowCloseButton != null)
                uiWindowCloseButton.clicked -= onWindowCloseButtonClicked;

            if (uiWindowDraggableTrigger != null)
                uiWindowDraggableTrigger.UnregisterCallback<MouseDownEvent>(onDraggableTriggerMouseDown);

            if (UIAtavismAudioManager.Instance != null)
                UIAtavismAudioManager.Instance.UnregisterSFX(uiDocument);

            isRegisteredUI = false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerEvents()
        {
        }
        protected virtual void unregisterEvents()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerPropertyChangedHandlers()
        {
        }
        protected virtual void unregisterPropertyChangedHandlers()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerExtensionMessages()
        {
        }
        protected virtual void unregisterExtensionMessages()
        {
        }
        #endregion
        #region UI Events
        protected virtual void onWindowCloseButtonClicked()
        {
            Hide();
        }
        #endregion
        #region Dragging
        protected virtual void onDraggableTriggerMouseDown(MouseDownEvent evt)
        {
            uiDocument.sortingOrder = 50;
            AtavismSettings.Instance.WindowOnTop = this;
            if (uiWindow == null)
            {
                Debug.LogWarning("onDraggableTriggerMouseDown Missing Window element."+name);
                return;
            }

            if (evt.button == (int)DraggingMouseButton)
                draggingBegin();
        }
        protected virtual void draggingBegin()
        {
            float width = uiWindow.resolvedStyle.width;
            float height = uiWindow.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            draggingMinValues.x = 0f;
            draggingMinValues.y = 0f;
            draggingMaxValues.x = canvasWidth - width;
            draggingMaxValues.y = canvasHeight - height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            
            draggingMouseOffset.x = scaledMousePosition.x - uiWindow.resolvedStyle.left;
            draggingMouseOffset.y = (canvasHeight - scaledMousePosition.y) - uiWindow.resolvedStyle.top;
            
            
            isDragging = true;
        }

        protected virtual void dragging()
        {
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            Vector2 position = new Vector2(scaledMousePosition.x - draggingMouseOffset.x, (canvasHeight - scaledMousePosition.y) - draggingMouseOffset.y);
            
            uiWindow.style.left = Mathf.Clamp(position.x, draggingMinValues.x, draggingMaxValues.x);
            uiWindow.style.top = Mathf.Clamp(position.y, draggingMinValues.y, draggingMaxValues.y);
        }

        protected virtual void draggingEnd()
        {
         //   Debug.LogError("draggingEnd");
            isDragging = false;
        }

        protected virtual void onDraggingEnd()
        {
           // Debug.LogError("onDraggingEnd");
            onWindowDragEnd?.Invoke();
        }

        public void MoveToCenter()
        {
          //  Debug.LogError("MoveToCenter");
            if (temporaryBlockCenter )
                return;
            float width = uiWindow.resolvedStyle.width;
            float height = uiWindow.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
           // Debug.LogError("MoveToCenter width="+width+"  height="+height);
            if (uiWindow.resolvedStyle.width == 0 && uiWindow.resolvedStyle.height == 0)
            {
                uiWindow.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
                width = uiWindow.style.width.value.value;
                height = uiWindow.style.height.value.value;
            }
          //  Debug.LogError("MoveToCenter | width="+width+"  height="+height);
            uiWindow.style.left = canvasWidth * 0.5f - (width * 0.5f);
            uiWindow.style.top = canvasHeight * 0.5f - (height * 0.5f);
        
        }

        private bool temporaryBlockCenter = false;
        private float leftPosition, topPosition;
        private IVisualElementScheduledItem showScheduler;
        private IVisualElementScheduledItem hideScheduler;


        public void SetPosition(float left, float top)
        {
            temporaryBlockCenter = true;
            
            if (uiWindow.resolvedStyle.width == 0 && uiWindow.resolvedStyle.height == 0)
            {
                uiWindow.RegisterCallback<GeometryChangedEvent>(onGeometryChangedSetPosition);
                leftPosition = left;
                topPosition = top;
            }
            else
            {
                uiWindow.style.left = left;
                uiWindow.style.top = top;
                temporaryBlockCenter = false;

            }
        }
        
        private void onGeometryChangedSetPosition(GeometryChangedEvent evt)
        {
            SetPosition(leftPosition, topPosition);
            uiWindow.UnregisterCallback<GeometryChangedEvent>(onGeometryChangedSetPosition);
        }
        private void onGeometryChanged(GeometryChangedEvent evt)
        {
            MoveToCenter();
            uiWindow.UnregisterCallback<GeometryChangedEvent>(onGeometryChanged);
        }

        #endregion
        #region Atavism Events
        protected virtual void OnEvent(AtavismEventData eData)
        {
        }
        #endregion
        #region Loop Updates
        protected virtual void Update()
        {
            if (isDraggable)
            {
                if (isDragging)
                {
                    if (Input.GetMouseButtonUp((int)DraggingMouseButton))
                    {
                        draggingEnd();
                        onDraggingEnd();
                        if (saveWindowPosition && AtavismSettings.Instance != null)
                        {
                            AtavismSettings.Instance.SetWindowPosition(this.name, new Vector3(uiWindow.resolvedStyle.left, uiWindow.resolvedStyle.top,0));
                        }
                    }
                    else
                    {
                        dragging();
                    }
                }
            }

            if (showUseFade)
            {
                if (uiWindow != null)
                {
                    var op = uiWindow.resolvedStyle.opacity;
                  //  Debug.LogError("Update " + op);
                    if (showing && op < 1f)
                    {
                        uiWindow.FadeInVisualElement();
                        
                    }
                    else if (!showing && op > 0f)
                    {
                        uiWindow.FadeOutVisualElement();
                    }
                }
            }
        }
        #endregion
        #region Public Methods
        public void Toggle()
        {
        //    Debug.LogError("UIAtavismWindowBase Toggle");
            if (showing)
                Hide();
            else
                Show();
        }

        public virtual void Show()
        {
            // if (showing)
                // return;
           // Debug.LogError("UIAtavismWindowBase Show " + name);
           if (AtavismSettings.Instance != null )
           {
               if(isDraggable)
                    AtavismSettings.Instance.OpenUIToolkitWindow(this);
               if(hideOnEscKey)
                    AtavismSettings.Instance.OpenUIToolkitWindowEscKey(this);
               if(hideCursorOnHide) 
                   AtavismSettings.Instance.OpenWindow(this);
           }

           if (!isRegisteredUI)
                registerUI();
            bool changedPosition = false;
            if (AtavismSettings.Instance != null)
            {
                AtavismSettings.Instance.OpenUIToolkitWindow(this);
               Vector3 v =  AtavismSettings.Instance.GetWindowPosition(this.name);
               if (v != Vector3.zero)
               {
                   SetPosition( v.x, v.y );
                   // Vector3 pos = uiWindow.transform.position;
                   // pos.x = v.x;
                   // pos.y = v.y;
                   // uiWindow.transform.position = pos;
                   changedPosition = true;
               }

            }
           // Debug.LogError(name+" set position ? "+changedPosition);
       
            if (!showUseFade)
            {
                if (uiScreen != null)
                {
                    uiScreen.ShowVisualElement();
                }
                else if (uiWindow != null)
                {
                    uiWindow.ShowVisualElement();
                    //uiWindow.style.opacity = 0.01f;
                    // showScheduler = uiWindow.schedule.Execute(FadeIn).Every(20);//.Until(() => showing);

                }
            }
            else
            {
                if (uiWindow != null)
                {
                    uiWindow.ShowVisualElement();
                    uiWindow.style.opacity = 0.01f;
                }
            }

            showing = true;
            isVisible = true;
            onShowEvent.Invoke();
            uiDocument.sortingOrder = 50f;
            AtavismSettings.Instance.WindowOnTop = this;
            if(showOnCenter && (!saveWindowPosition || !changedPosition) )
                MoveToCenter();
        }
        
        public virtual void Hide()
        {
           // Debug.LogError("UIAtavismWindowBase Hide "+name);
           if (AtavismSettings.Instance != null )
           {
               if(isDraggable)
                   AtavismSettings.Instance.CloseUIToolkitWindow(this);
               if(hideOnEscKey)
                   AtavismSettings.Instance.CloseUIToolkitWindowEscKey(this);
               if(hideCursorOnHide) 
                   AtavismSettings.Instance.CloseWindow(this);
           }
            if (!isRegisteredUI)
                registerUI();

            if (!showUseFade)
            {
                if (uiScreen != null)
                {
                    uiScreen.HideVisualElement();
                }else
                if (uiWindow != null)
                {
                    // hideScheduler = uiWindow.schedule.Execute(FadeOut).Every(20);//.Until(() => !showing);
                     uiWindow.HideVisualElement();
                }
            }

            isVisible = false;
            showing = false;
            onHideEvent.Invoke();
            uiDocument.sortingOrder = 20;
            if(UIAtavismTooltip.Instance!=null)
                UIAtavismTooltip.Instance.Hide();
                
          
        }

        
        public void ApplyTexture(VisualElement thisElement, Texture2D thisTexture)
        {
            if (thisElement == null)
            {
                Debug.LogError("ApplyTexture Exception object is null");
                return;
            }
            thisElement.style.backgroundImage = thisTexture;
        }

        public void ApplyText(Label thisLabelElement, string thisText)
        {
            if (thisLabelElement == null)
            {
                Debug.LogError("ApplyText Exception object is null");
                return;
            }
                
            thisLabelElement.text = thisText;
        }

        public void ApplyText(Label thisLabelElement, string thisText, StyleFont thisFont, StyleFontDefinition thisFontDefinition, StyleEnum<FontStyle> thisFontStyleAndWeight, StyleLength thisFontSize)
        {
            thisLabelElement.text = thisText;
            ApplyFont(thisLabelElement, thisFont);
            ApplyFontSize(thisLabelElement, thisFontSize);
            ApplyFontStyleAndWeight(thisLabelElement, thisFontStyleAndWeight);
            ApplyFontDefinition(thisLabelElement, thisFontDefinition);
        }

        public void ApplyText(Label thisLabelElement, string thisText, StyleFont thisFont, StyleLength thisFontSize)
        {
            thisLabelElement.text = thisText;
            ApplyFont(thisLabelElement, thisFont);
            ApplyFontSize(thisLabelElement, thisFontSize);
        }

        public void ApplyText(Label thisLabelElement, string thisText, StyleFont thisFont, StyleFontDefinition thisFontDefinition,  StyleLength thisFontSize)
        {
            thisLabelElement.text = thisText;
            ApplyFont(thisLabelElement, thisFont);
            ApplyFontSize(thisLabelElement, thisFontSize);
            ApplyFontDefinition(thisLabelElement, thisFontDefinition);
        }

        public void ApplyStyle(VisualElement thisElement, StyleFont thisFont, StyleFontDefinition thisFontDefinition, StyleEnum<FontStyle> thisFontStyleAndWeight, StyleLength thisFontSize)
        {
            ApplyFont(thisElement, thisFont);
            ApplyFontSize(thisElement, thisFontSize);
            ApplyFontStyleAndWeight(thisElement, thisFontStyleAndWeight);
            ApplyFontDefinition(thisElement, thisFontDefinition);
        }

        public void ApplyStyle(VisualElement thisElement, StyleFont thisFont, StyleEnum<FontStyle> thisFontStyleAndWeight, StyleLength thisFontSize)
        {
            ApplyFont(thisElement, thisFont);
            ApplyFontSize(thisElement, thisFontSize);
            ApplyFontStyleAndWeight(thisElement, thisFontStyleAndWeight);
        }

        public void ApplyStyle(VisualElement thisElement, StyleFont thisFont, StyleFontDefinition thisFontDefinition, StyleLength thisFontSize)
        {
            ApplyFont(thisElement, thisFont);
            ApplyFontSize(thisElement, thisFontSize);
            ApplyFontDefinition(thisElement, thisFontDefinition);
        }

        public void ApplyStyle(VisualElement thisElement, StyleFont thisFont, StyleLength thisFontSize)
        {
            ApplyFont(thisElement, thisFont);
            ApplyFontSize(thisElement, thisFontSize);
        }

        public void ApplyFont(VisualElement thisElement, StyleFont thisFont)
        {
            thisElement.style.unityFont = thisFont;
        }

        public void ApplyFontDefinition(VisualElement thisElement, StyleFontDefinition thisFontDefinition)
        {
            thisElement.style.unityFontDefinition = thisFontDefinition;
        }

        public void ApplyFontStyleAndWeight(VisualElement thisElement, StyleEnum<FontStyle> thisFontStyleAndWeight)
        {
            thisElement.style.unityFontStyleAndWeight = thisFontStyleAndWeight;
        }

        public void ApplyFontSize(VisualElement thisElement, StyleLength thisFontSize)
        {
            thisElement.style.fontSize = thisFontSize;
        }




        public virtual void UpdateData()
        {
        }
        #endregion
    }
}