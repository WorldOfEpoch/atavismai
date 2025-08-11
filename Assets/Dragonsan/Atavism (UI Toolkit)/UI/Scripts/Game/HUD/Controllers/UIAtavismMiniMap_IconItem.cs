using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;


namespace Atavism.UI
{

    public class UIAtavismMiniMap_IconItem 
    {

        [Separator("SETTINGS")]
        public float DestroyIn = 5f;

        [Separator("REFERENCES")] 
        public VisualElement uiRoot = null;
        public VisualElement TargetGraphic;
        public VisualElement CircleAreaRect = null;
        public Sprite DeathIcon = null;
        public Label InfoText = null;

        // private CanvasGroup m_CanvasGroup;
        private Animator Anim;
        public float delay = 0.1f;
        // private bl_MaskHelper MaskHelper = null;

        public void SetVisualElement(VisualElement visualElement)
        {
            uiRoot = visualElement;
            InfoText = visualElement.Q<Label>("Text");
            TargetGraphic = visualElement.Q<VisualElement>("Target");
            CircleAreaRect = visualElement.Q<VisualElement>("Circle-Area");
            Init();
        }
        /// <summary>
        /// 
        /// </summary>
        void Init()
        {
          
            // if (GetComponent<Animator>() != null)
            // {
            //     Anim = GetComponent<Animator>();
            // }
            // if (Anim != null)
            // {
            //     Anim.enabled = false;
            // }
            uiRoot.style.opacity = 0;
            if (CircleAreaRect != null)
            {
                CircleAreaRect.HideVisualElement();
            }
        }

        public void Show()
        {
            uiRoot.ShowVisualElement();
        }
        public void Hide()
        {
            uiRoot.HideVisualElement();
        }
        
        /// <summary>
        /// When player or the target die,desactive,remove,etc..
        /// call this for remove the item UI from Map
        /// for change to other icon and desactive in certain time
        /// or destroy immediate
        /// </summary>
        /// <param name="inmediate"></param>
        public void DestroyIcon(bool inmediate)
        {
            TargetGraphic.RemoveFromHierarchy();
            if (inmediate)
            {
                // Destroy(gameObject);
            }
            else
            {
                //Change the sprite to icon death
                TargetGraphic.style.backgroundImage = DeathIcon.texture;
                //destroy in 5 seconds
                // Destroy(gameObject, DestroyIn);
            }
        }
        /// <summary>
        /// When player or the target die,desactive,remove,etc..
        /// call this for remove the item UI from Map
        /// for change to other icon and desactive in certain time
        /// or destroy immediate
        /// </summary>
        /// <param name="inmediate"></param>
        /// <param name="death"></param>
        public void DestroyIcon(bool inmediate, Sprite death)
        {
            TargetGraphic.RemoveFromHierarchy();
            if (inmediate)
            {
                // Destroy(gameObject);
            }
            else
            {
                //Change the sprite to icon death
                TargetGraphic.SetBackgroundImage(death);
                //destroy in 5 seconds
                // Destroy(gameObject, DestroyIn);
            }
        }
        /// <summary>
        /// Get info to display
        /// </summary>
        /// <param name="info"></param>
        public void GetInfoItem(string info)
        {
            if (InfoText == null)
                return;

            InfoText.text = info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ico"></param>
        public void SetIcon(Sprite ico)
        {
            TargetGraphic.SetBackgroundImage(ico);
        }

        /// <summary>
        /// Show a visible circle area in the minimap with this
        /// item as center
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="AreaColor"></param>
        public VisualElement SetCircleArea(float radius, Color AreaColor)
        {
            if (CircleAreaRect == null)
            {
                return null;
            }

            // MaskHelper = transform.root.GetComponentInChildren<bl_MaskHelper>();
            // MaskHelper.SetMaskedIcon(CircleAreaRect);
            radius = radius * 10;
            radius = radius * bl_MiniMapUtils.GetMiniMap().IconMultiplier;
            CircleAreaRect.style.width = radius;
            CircleAreaRect.style.height = radius;
            CircleAreaRect.style.unityBackgroundImageTintColor = AreaColor;
            CircleAreaRect.ShowVisualElement();

            return CircleAreaRect;
        }

        /// <summary>
        /// 
        /// </summary>
        public void HideCircleArea()
        {
            // CircleAreaRect.SetParent(transform);
            CircleAreaRect.HideVisualElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator FadeIcon()
        {
            yield return new WaitForSeconds(delay);
            while (uiRoot.style.opacity.value < 1)
            {
                var op = uiRoot.resolvedStyle.opacity;
                op += Time.deltaTime * 2;
                uiRoot.style.opacity = op;
                yield return null;
            }
            if (Anim != null)
            {
                Anim.enabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetVisibleAlpha()
        {
            uiRoot.style.opacity = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnClickMark()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private bool open = false;
        // public void InfoItem()
        // {
        //     open = !open;
        //     Animator a = GetComponent<Animator>();
        //     if (open)
        //     {
        //         a.SetBool("Open", true);
        //     }
        //     else
        //     {
        //         a.SetBool("Open", false);
        //     }
        // }

        // public void DelayStart(float v)
        // {
        //     delay = v;
        //     StartCoroutine(FadeIcon());
        // }
    }
}