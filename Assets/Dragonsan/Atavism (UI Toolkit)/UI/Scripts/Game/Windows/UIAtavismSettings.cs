using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismSettings : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] VisualTreeAsset controlElementTemplate;

        [SerializeField] private AudioMixer masterMixer;
        
        [SerializeField] Label buttonSoundsText;
        [SerializeField] VisualElement buttonSoundsImage;
        [SerializeField] Label buttonGraphicsText;
        [SerializeField] VisualElement buttonGraphicsImage;
        [SerializeField] Label buttonGeneralText;
        [SerializeField] VisualElement buttonGeneralImage;
        [SerializeField] Label buttonControllText;
        [SerializeField] VisualElement buttonControllImage;
        private UIButtonToggleGroup menuTab;
        private Button returnButton;
        [SerializeField] VisualElement tabSounds;
        [SerializeField] VisualElement tabGraphics;
        [SerializeField] VisualElement tabGeneral;
        [SerializeField] VisualElement tabControll;
        private VisualElement screen;
        // private UIGeneralSettings generalSettings;
        // [SerializeField] Color selectedColor = new Color(0f, 1f, 0f, 1f);
        // [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 1f);
        // [SerializeField] Color selectedTextColor = new Color(0f, 1f, 0f, 1f);
        // [SerializeField] Color normalTextColor = new Color(1f, 1f, 1f, 1f);


        private UIAtavismGeneralSettings generalSettings;
        private UIAtavismGraphicSettings graphicSettings;
        private UIAtavismMusicSettings musicSettings;
        private UIAtavismControls controlSettings;
        // Use this for initialization
        // void Start()
        // {
        //     registerUI();
        // }
        public bool isShowing = false;
        void registerUI()
        {
            uiDocument.enabled = true;
            screen = uiDocument.rootVisualElement.Q<VisualElement>("Screen");
            returnButton = screen.Q<Button>("return-button");
            returnButton.clicked += Hide;
            menuTab = screen.Q<UIButtonToggleGroup>("tab-menu");
            menuTab.OnItemIndexChanged += TopMenuChange;
            tabSounds = screen.Q<VisualElement>("tab-sounds");
            tabGraphics = screen.Q<VisualElement>("tab-graphic");
            tabGeneral = screen.Q<VisualElement>("tab-general");
            tabControll = screen.Q<VisualElement>("tab-control");

            generalSettings = new UIAtavismGeneralSettings();
            generalSettings.Setup(tabGeneral,screen,this);
            graphicSettings = new UIAtavismGraphicSettings();
            graphicSettings.Setup(tabGraphics,screen, this);
            musicSettings = new UIAtavismMusicSettings();
            musicSettings.Setup(tabSounds,screen,this,masterMixer);
            controlSettings = new UIAtavismControls();
            controlSettings.Setup(tabControll,controlElementTemplate,screen,this);
            ClickGeneral();
            Hide();
        }

        public void Hide()
        {
            AtavismSettings.Instance.CloseWindow(this);
            isShowing = false;
            screen.HideVisualElement();
        }

        public void Show()
        {
            AtavismSettings.Instance.OpenWindow(this);   
            isShowing = true;
            screen.ShowVisualElement();
            menuTab.Set(0);
        }
        private void TopMenuChange(int obj)
        {
          //  Debug.LogError("TopMenuChange "+obj);
            switch (obj)
            {
                case 0:
                    ClickGeneral();
                    break;
                case 1:
                    ClickSound();
                    break;
                case 2:
                    ClickGraphic();
                    break;
                case 3:
                    ClickControl();
                    break;
            }
        }

        private void OnEnable()
        {
            registerUI();
        }

        private void OnDisable()
        {
            // AtavismSettings.Instance.CloseWindow(this);   
        }

        public void ClickSound()
        {
            if (tabControll != null)
                tabControll.HideVisualElement();
            if (tabGeneral != null)
                tabGeneral.HideVisualElement();
            if (tabGraphics != null)
                tabGraphics.HideVisualElement();
            if (tabSounds != null)
                tabSounds.ShowVisualElement();
            if (buttonControllText != null)
                buttonControllText.RemoveFromClassList("menu-gear-selected");
            if (buttonControllImage != null)
                buttonControllImage.RemoveFromClassList("menu-gear-selected");
            if (buttonGeneralText != null)
                buttonGeneralText.RemoveFromClassList("menu-gear-selected");
            if (buttonGeneralImage != null)
                buttonGeneralImage.RemoveFromClassList("menu-gear-selected");
            if (buttonGraphicsText != null)
                buttonGraphicsText.RemoveFromClassList("menu-gear-selected");
            if (buttonGraphicsImage != null)
                buttonGraphicsImage.RemoveFromClassList("menu-gear-selected");
            if (buttonSoundsText != null)
            {
                buttonSoundsText.AddToClassList("menu-gear-selected");
            }
            if (buttonSoundsImage != null)
                buttonSoundsImage.AddToClassList("menu-gear-selected");
        }
        public void ClickGeneral()
        {
            if (tabControll != null)
                tabControll.HideVisualElement();
            if (tabGeneral != null)
            {
                tabGeneral.ShowVisualElement();
                generalSettings.UpdateParam();
            }
            if (tabGraphics != null)
                tabGraphics.HideVisualElement();
            if (tabSounds != null)
                tabSounds.HideVisualElement();
            if (buttonControllText != null)
                buttonControllText.RemoveFromClassList("menu-gear-selected");
            if (buttonControllImage != null)
                buttonControllImage.RemoveFromClassList("menu-gear-selected");
            if (buttonGeneralText != null)
                buttonGeneralText.AddToClassList("menu-gear-selected");
            if (buttonGeneralImage != null)
                buttonGeneralImage.AddToClassList("menu-gear-selected");
            if (buttonGraphicsText != null)
                buttonGraphicsText.RemoveFromClassList("menu-gear-selected");
            if (buttonGraphicsImage != null)
                buttonGraphicsImage.RemoveFromClassList("menu-gear-selected");
            if (buttonSoundsText != null)
            {
                buttonSoundsText.RemoveFromClassList("menu-gear-selected");
            }
            if (buttonSoundsImage != null)
                buttonSoundsImage.RemoveFromClassList("menu-gear-selected");
            
        }
        public void ClickGraphic()
        {
            if (tabControll != null)
                tabControll.HideVisualElement();
            if (tabGeneral != null)
                tabGeneral.HideVisualElement();
            if (tabGraphics != null)
                tabGraphics.ShowVisualElement();
            if (tabSounds != null)
                tabSounds.HideVisualElement();
            if (buttonControllText != null)
                buttonControllText.RemoveFromClassList("menu-gear-selected");
            if (buttonControllImage != null)
                buttonControllImage.RemoveFromClassList("menu-gear-selected");
            if (buttonGeneralText != null)
                buttonGeneralText.RemoveFromClassList("menu-gear-selected");
            if (buttonGeneralImage != null)
                buttonGeneralImage.RemoveFromClassList("menu-gear-selected");
            if (buttonGraphicsText != null)
                buttonGraphicsText.AddToClassList("menu-gear-selected");
            if (buttonGraphicsImage != null)
                buttonGraphicsImage.AddToClassList("menu-gear-selected");
            if (buttonSoundsText != null)
            {
                buttonSoundsText.RemoveFromClassList("menu-gear-selected");
            }
            if (buttonSoundsImage != null)
                buttonSoundsImage.RemoveFromClassList("menu-gear-selected");
        }
        public void ClickControl()
        {
            if (tabControll != null)
                tabControll.ShowVisualElement();
            if (tabGeneral != null)
                tabGeneral.HideVisualElement();
            if (tabGraphics != null)
                tabGraphics.HideVisualElement();
            if (tabSounds != null)
                tabSounds.HideVisualElement();
            if (buttonControllText != null)
                buttonControllText.AddToClassList("menu-gear-selected");
            if (buttonControllImage != null)
                buttonControllImage.AddToClassList("menu-gear-selected");
            if (buttonGeneralText != null)
                buttonGeneralText.RemoveFromClassList("menu-gear-selected");
            if (buttonGeneralImage != null)
                buttonGeneralImage.RemoveFromClassList("menu-gear-selected");
            if (buttonGraphicsText != null)
                buttonGraphicsText.RemoveFromClassList("menu-gear-selected");
            if (buttonGraphicsImage != null)
                buttonGraphicsImage.RemoveFromClassList("menu-gear-selected");
            if (buttonSoundsText != null)
            {
                buttonSoundsText.RemoveFromClassList("menu-gear-selected");
            }
            if (buttonSoundsImage != null)
                buttonSoundsImage.RemoveFromClassList("menu-gear-selected");
        }

        private void OnGUI()
        {
            Event e = Event.current;    
            if(controlSettings != null)
             controlSettings.KeyCheck(e.isMouse, e.button, e.shift, e.control, e.alt,e.isKey, e.keyCode);
        }

        private void Update()
        {
            // if(controlSettings != null)
                // controlSettings.update();
        }
    }
}