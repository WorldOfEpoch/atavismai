using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismMiniTooltip : MonoBehaviour
    {
        [AtavismSeparator("Window Base")] [SerializeField]
        protected UIDocument uiDocument;
        
        static UIAtavismMiniTooltip instance;

        private Label description;

        private VisualElement uiroot;
        private float initialWidth;
        private float initialHeight;
        GameObject target;
        bool showing = false;
        protected Vector2 draggingMinValues, draggingMaxValues, draggingMouseOffset;
        void OnEnable()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            uiDocument.enabled = true;
            // initialWidth = Screen.width;
            uiroot = uiDocument.rootVisualElement.Q<VisualElement>("mini-tooltip-panel");
            description = uiroot.Q<Label>("message");
            Hide();
        }

        // Use this for initialization
        void OnDestroy()
        {
            instance = null;
        }

        void Update()
        {
            if (!showing)
                return;
            CalculatePosition();
        }

        void CalculatePosition(){
            
            float width = uiroot.resolvedStyle.width;
            float height = uiroot.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            draggingMinValues.x = 0f;
            draggingMinValues.y = 0f;
            draggingMaxValues.x = canvasWidth - width;
            draggingMaxValues.y = canvasHeight - height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 position = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            position += new Vector2(5, -5);
       //     Debug.LogError("scaleFactor "+scaleFactor+" scaledMousePosition="+scaledMousePosition+" width="+width+" height="+height+" position="+position+" draggingMinValues="+draggingMinValues+" draggingMaxValues="+draggingMaxValues );
            uiroot.style.left = Mathf.Clamp(position.x, draggingMinValues.x, draggingMaxValues.x);
            uiroot.style.top = Mathf.Clamp(canvasHeight-position.y-height, draggingMinValues.y, draggingMaxValues.y);
        }


        public void SetDescription(string descriptionText)
        {
            if (description != null)
            {
#if AT_I2LOC_PRESET
                description.text = I2.Loc.LocalizationManager.GetTranslation(descriptionText);
#else
                description.text = descriptionText;
#endif
            }
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public void Show(VisualElement target)
        {
            uiroot.RegisterCallback<GeometryChangedEvent>(onGeometryShow);
            uiroot.style.opacity = 0.1f;
            uiroot.ShowVisualElement();

            // showing = true;
        }
        private void onGeometryShow(GeometryChangedEvent evt)
        {

            CalculatePosition();
            uiroot.style.opacity = 1;
            uiroot.UnregisterCallback<GeometryChangedEvent>(onGeometryShow);
            showing = true;
        }
        public void Hide()
        {
            uiroot.HideVisualElement();
            showing = false;
        }

        public static UIAtavismMiniTooltip Instance
        {
            get
            {
                return instance;
            }

        }
    }
}