using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;


namespace Atavism.UI
{

    public class UIAtavismMiniMapItem : MonoBehaviour
    {

        [Separator("TARGET")]
        [Tooltip("UI Prefab")]
        public VisualTreeAsset MiniMapItemEntryTemplate = null;
        [Tooltip("Transform to UI Icon will be follow")]
        public Transform Target = null;
        [Tooltip("Custom Position from target position")]
        public Vector3 OffSet = Vector3.zero;

        [Separator("ICON")]
        public StyleBackground Icon = null;
        public Sprite DeathIcon = null;
        public Color IconColor = new Color(1, 1, 1, 0.9f);
        [Range(1, 100)] public float Size = 20;

        [Separator("CIRCLE AREA")]
        public bool ShowCircleArea = false;
        [Range(1, 100)] public float CircleAreaRadius = 10;
        public Color CircleAreaColor = new Color(1, 1, 1, 0.9f);

        [Separator("ICON BUTTON")]
        [Tooltip("UI can interact when press it?")]
        [CustomToggle("is Interact-able")]
        public bool isInteractable = true;
        [TextArea(2, 2)] public string InfoItem = "";

        [Separator("SETTINGS")]
        [Tooltip("Can Icon show when is off screen?")]
        [CustomToggle("Off Screen")]
        public bool OffScreen = true;
        [CustomToggle("Destroy Icon with object")] public bool DestroyWithObject = false;
        [Range(0, 5)] public float BorderOffScreen = 0.01f;
        [Range(1, 50)] public float OffScreenSize = 10;
        [CustomToggle("isHoofdPunt")]
        public bool isHoofdPunt = false;
        [Tooltip("Time before render/show item in minimap after instance")]
        [Range(0, 3)] public float RenderDelay = 0.3f;
        public ItemEffect m_Effect = ItemEffect.None;
        //Privates
        // private Image Graphic = null;
        // private RectTransform GraphicRect;
        private VisualElement RectRoot;
        private UIAtavismMiniMap_IconItem cacheItem = null;
        private VisualElement CircleAreaRect = null;
        private Vector3 position;
        private UIAtavismMiniMap MiniMap;

        AtavismNode node;

        /// <summary>
        /// Get all required component in start
        /// </summary>
        void Start()
        {
            node = GetComponent<AtavismNode>();
            MiniMap = UIAtavismMiniMapUtils.GetMiniMap();
           // InfoItem = gameObject.name;
            if (MiniMap != null)
            {
                CreateIcon();
            }
            else
            {
                Debug.Log("You need a MiniMap in scene for use MiniMap Items.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CreateIcon()
        {
            if (MiniMap.hasError)
                return;
            
            //Instantiate UI in canvas
            if (MiniMapItemEntryTemplate == null)
                MiniMapItemEntryTemplate = AtavismSettings.Instance.MinimapSettings.minimapUiItemPrefab;

            RectRoot = UIAtavismMiniMapUtils.GetMiniMap().MMUIRoot;

            
            cacheItem = new UIAtavismMiniMap_IconItem();
            // Instantiate the UXML template for the entry
            var newEntry = MiniMapItemEntryTemplate.Instantiate();
            // Assign the controller script to the visual element
            newEntry.userData = cacheItem;
            // Initialize the controller script
            cacheItem.SetVisualElement(newEntry);
            // Return the root of the instantiated visual tree
           // return newListEntry;
           cacheItem.TargetGraphic.name = node.name;
           RectRoot.Add(cacheItem.TargetGraphic);
          
            // cacheItem = Instantiate(GraphicPrefab) as GameObject;
            //SetUp Icon UI
            // Graphic = cacheItem.GetComponent<Image>();
            // GraphicRect = Graphic.GetComponent<RectTransform>();
            if (Icon != null)
            {
                cacheItem.TargetGraphic.style.backgroundImage = Icon;
                cacheItem.TargetGraphic.style.unityBackgroundImageTintColor = IconColor;
                if (node != null)
                    if (node.PropertyExists("reaction"))
                    {
                        int targetType = (int)node.GetProperty("reaction");
                        if (targetType < 0)
                        {
                            cacheItem.TargetGraphic.style.unityBackgroundImageTintColor = Color.red;
                        }
                        else if (targetType > 0)
                        {
                            cacheItem.TargetGraphic.style.unityBackgroundImageTintColor = Color.green;
                        }
                        else
                            cacheItem.TargetGraphic.style.unityBackgroundImageTintColor = Color.yellow;
                    }

            }
            else
            {
                cacheItem.TargetGraphic.style.backgroundImage = AtavismSettings.Instance.MinimapSettings.minimapIcon.texture;
                cacheItem.TargetGraphic.style.unityBackgroundImageTintColor = Color.blue;

            }
            // cacheItem.transform.SetParent(RectRoot.transform, false);
            // GraphicRect.anchoredPosition = Vector2.zero;
            // cacheItem.GetComponent<CanvasGroup>().interactable = isInteractable;
            if (Target == null)
            {
                Target = this.GetComponent<Transform>();
            }
            StartEffect();
            DelayStart(RenderDelay);
            cacheItem.GetInfoItem(InfoItem);
            if (ShowCircleArea)
            {
                 CircleAreaRect = cacheItem.SetCircleArea(CircleAreaRadius, CircleAreaColor);
            }
        }
        public void DelayStart(float v)
        {
            cacheItem.delay = v;
            StartCoroutine(cacheItem.FadeIcon());
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {

            //If a component missing, return for avoid bugs.
            if (Target == null)
                return;
            if (cacheItem == null)
                return;

            if (isHoofdPunt)
            {
                if (MiniMap.Target != null)
                {
                    transform.position = MiniMap.Target.TransformPoint((MiniMap.Target.forward) * 100);
                }
            }

            if (MiniMap.MMCamera == null)
                return;
            cacheItem.GetInfoItem(InfoItem);
            cacheItem.TargetGraphic.style.position = Position.Absolute;
            //Setting the modify position
            Vector3 CorrectPosition = TargetPosition + OffSet;
            CorrectPosition.y = MiniMap.MMCamera.transform.position.y;
            Vector2 size = new Vector2(RectRoot.resolvedStyle.width, RectRoot.resolvedStyle.height) * 0.5f;
            //Convert the position of target in ViewPortPoint
            //Vector2 vp2 = UIAtavismMiniMap.MiniMapCamera.WorldToViewportPoint(CorrectPosition);
            Vector3 vp2 = MiniMap.MMCamera.WorldToViewportPoint(CorrectPosition);
            //Calculate the position of target and convert into position of screen

            position = new Vector3((vp2.x * RectRoot.resolvedStyle.width),
                (vp2.y * RectRoot.resolvedStyle.height), 0f);

            Vector2 UnClampPosition = position;
            //if show off screen
            if (OffScreen)
            {
                //Calculate the max and min distance to move the UI
                //this clamp in the RectRoot sizeDela for border
                position.x =
                    Mathf.Clamp(position.x, -BorderOffScreen, (2 * size.x - BorderOffScreen)); //+RectRoot.resolvedStyle.width*0.5f;
                position.y =
                    Mathf.Clamp(position.y,  -BorderOffScreen, (2 * size.y - BorderOffScreen)); //-RectRoot.resolvedStyle.height*0.5f;
            }

            //calculate the position of UI again, determine if off screen
            //if off screen reduce the size
            float Iconsize = Size;
            //Use this (useCompassRotation when have a circle miniMap)
            if (m_miniMap.useCompassRotation)
            {
                //Compass Rotation
                Vector3 screenPos = Vector3.zero;
                //Calculate difference
                Vector3 forward = Target.position - m_miniMap.TargetPosition;
                //Position of target from camera
                Vector3 cameraRelativeDir = UIAtavismMiniMap.MiniMapCamera.transform.InverseTransformDirection(forward);
                //normalize values for screen fix
                cameraRelativeDir.z = 0;
                cameraRelativeDir = cameraRelativeDir.normalized * 0.5f;
                //Convert values to positive for calculate area OnScreen and OffScreen.
                float posPositiveX = Mathf.Abs(position.x);
                float relativePositiveX = Mathf.Abs((0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize)));
                //when target if offScreen clamp position in circle area.
                if (posPositiveX >= relativePositiveX)
                {
                    screenPos.x = 0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize) /*/ Camera.main.aspect*/;
                    screenPos.y = 0.5f + (cameraRelativeDir.y * m_miniMap.CompassSize);
                    position = screenPos;
                    Iconsize = OffScreenSize;
                }
                else
                {
                    Iconsize = Size;
                }
            }
            else
            {
                if (position.x ==  - BorderOffScreen || position.y ==  - BorderOffScreen ||
                    position.x == 2*size.x - BorderOffScreen || position.y == 2*size.y - BorderOffScreen)
                {
                    Iconsize = OffScreenSize;
                }
                else
                {
                    Iconsize = Size;
                }
            }

            // Debug.LogError(
            //     "Update position " + name + ":" + node.Oid + " size=" + size + " UnClampPosition=" + UnClampPosition +
            //     " position=" + position + " vp2=" + vp2 + " CorrectPosition=" + CorrectPosition, gameObject);
            //Apply position to the UI (for follow)
            cacheItem.TargetGraphic.style.bottom = position.y - cacheItem.TargetGraphic.resolvedStyle.height * 0.5f;
            cacheItem.TargetGraphic.style.left = position.x - cacheItem.TargetGraphic.resolvedStyle.width * 0.5f;
            if (CircleAreaRect != null)
            {
                CircleAreaRect.style.bottom = UnClampPosition.y - CircleAreaRect.resolvedStyle.height * 0.5f;
                CircleAreaRect.style.left = UnClampPosition.x - CircleAreaRect.resolvedStyle.width * 0.5f;
            }

            //Change size with smooth transition
            float CorrectSize = Iconsize * MiniMap.IconMultiplier;

            var sizeDelta =
                Vector2.Lerp(
                    new Vector2(cacheItem.TargetGraphic.resolvedStyle.width,
                        cacheItem.TargetGraphic.resolvedStyle.height), new Vector2(CorrectSize, CorrectSize),
                    Time.deltaTime * 8);
//Debug.LogWarning("Icon size "+Size+" OffScreenSize="+OffScreenSize+" CorrectSize="+CorrectSize+" => "+sizeDelta.x+" "+sizeDelta.y+" "+name);
            cacheItem.TargetGraphic.style.width = sizeDelta.x;
            cacheItem.TargetGraphic.style.height = sizeDelta.y;

            if (MiniMap.RotationAlwaysFront)
            {
                //with this the icon rotation always will be the same (for front)
                Quaternion r = Quaternion.identity;
                r.x = Target.rotation.x;
                cacheItem.TargetGraphic.style.rotate = new Rotate(r.eulerAngles.y);
                // GraphicRect.localRotation = r;
            }
            else
            {
                //with this the rotation icon will depend of target
                Vector3 vre = MiniMap.transform.eulerAngles;
                Vector3 re = Vector3.zero;
                //Fix player rotation for apply to el icon.
                re.z = ((-this.Target.rotation.eulerAngles.y) + vre.y);

                Quaternion q = Quaternion.Euler(re);
                cacheItem.TargetGraphic.style.rotate = new Rotate(q.eulerAngles.y);
                // GraphicRect.rotation = q;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void StartEffect()
        {
            //TODO
            // Animator a = Graphic.GetComponent<Animator>();
            // if (m_Effect == ItemEffect.Pulsing)
            // {
            //     a.SetInteger("Type", 2);
            // }
            // else if (m_Effect == ItemEffect.Fade)
            // {
            //     a.SetInteger("Type", 1);
            // }
        }

        /// <summary>
        /// When player or the target die,desactive,remove,etc..
        /// call this for remove the item UI from Map
        /// for change to other icon and desactive in certain time
        /// or destroy immediate
        /// </summary>
        /// <param name="inmediate"></param>
        public void DestroyItem(bool inmediate)
        {
           // Debug.LogError("Graphic Item of " + this.name + " DestroyItem");
            if (cacheItem == null)
            {
                Debug.Log("Graphic Item of " + this.name + " not exist in scene");
                return;
            }

            if (DeathIcon == null || inmediate)
            {
                cacheItem.DestroyIcon(inmediate);
                
            }
            else
            {
                cacheItem.DestroyIcon(inmediate, DeathIcon);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ico"></param>
        public void SetIcon(Sprite ico)
        {
            if (cacheItem == null)
            {
                if(ico != null)
                Icon = ico.texture;
                //Debug.LogWarning("You can't set a icon before create the item.",this);
                return;
            }

            cacheItem.SetIcon(ico);
        }

        public void SetColor(Color col)
        {
            if (cacheItem == null)
            {
                IconColor = col;
                return;
            }
            if(cacheItem.TargetGraphic.style.unityBackgroundImageTintColor != col)
                cacheItem.TargetGraphic.style.unityBackgroundImageTintColor = col;
        }


        /// <summary>
        /// Show a visible circle area in the minimap with this
        /// item as center
        /// </summary>
        public void SetCircleArea(float radius, Color AreaColor)
        {
            // CircleAreaRect = cacheItem.SetCircleArea(radius, AreaColor);
        }

        /// <summary>
        /// 
        /// </summary>
        public void HideCircleArea()
        {
            cacheItem.HideCircleArea();
            // CircleAreaRect = null;
        }

        /// <summary>
        /// Call this for hide item in miniMap
        /// For show again just call "ShowItem()"
        /// NOTE: For destroy item call "DestroyItem(bool immediate)" instant this.
        /// </summary>
        public void HideItem()
        {
            if (cacheItem != null)
            {
                cacheItem.Hide();
            }
            else
            {
                Debug.Log("There is no item to disable.");
            }
        }

        /// <summary>
        /// Call this for show again the item in miniMap when is hide
        /// </summary>
        public void ShowItem()
        {
            if (cacheItem != null)
            {
                cacheItem.Show();
                cacheItem.SetVisibleAlpha();
            }
            else
            {
                Debug.Log("There is no item to active.");
            }
        }

        /// <summary>
        /// If you need destroy icon when this gameObject is destroy.
        /// </summary>
        void OnDestroy()
        {
             DestroyItem(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 TargetPosition
        {
            get
            {
                if (Target == null)
                {
                    return Vector3.zero;
                }

                return new Vector3(Target.position.x, -1f, Target.position.z);
                //return Target.position;
            }
        }

        private UIAtavismMiniMap _minimap = null;
        private UIAtavismMiniMap m_miniMap
        {
            get
            {
                if (_minimap == null)
                {
                    _minimap = AtavismSettings.Instance.UiMiniMap;

                }
                return _minimap;
            }
        }
    }
}