using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public static class UIToolkit
    {
        /// <summary>
        /// Change display style to display normally
        /// </summary>
        /// <param name="ve"></param>
        public static  void ShowVisualElement(this VisualElement ve)
        {
            if (ve == null)
                return;

            ve.style.display = DisplayStyle.Flex;
        }
        /// <summary>
        /// Change display style to hide
        /// </summary>
        /// <param name="ve"></param>
        public static void HideVisualElement(this VisualElement ve)
        {
            if (ve == null)
                return;

            ve.style.display = DisplayStyle.None;
        }
        /// <summary>
        /// Check if Visual Element is displayed
        /// </summary>
        /// <param name="ve"></param>
        /// <returns></returns>
        public static bool IsVisibleElement(this VisualElement ve)
        {
            if (ve == null)
                return false;

            return ve.style.display != DisplayStyle.None;
            
        }
        /// <summary>
        /// Change display style to display normally for list of elements
        /// </summary>
        /// <param name="ve"></param>
        public static void ShowVisualElements(VisualElement[] ve)
        {
            for (int n = 0; n < ve.Length; n++)
                ve[n].ShowVisualElement();
        }
        /// <summary>
        /// Change display style to hide for list of elements
        /// </summary>
        /// <param name="ve"></param>
        public static void HideVisualElements(VisualElement[] ve)
        {
            for (int n = 0; n < ve.Length; n++)
                ve[n].HideVisualElement();
        }
        /// <summary>
        /// Set Background Image from Texture 2D
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="texture"></param>
        public static void SetBackgroundImage(this VisualElement ve, Texture2D texture) => ve.style.backgroundImage = new StyleBackground(texture);
        /// <summary>
        /// Set Background Image from Sprite
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="sprite"></param>
        public static void SetBackgroundImage(this VisualElement ve, Sprite sprite) => ve.style.backgroundImage = new StyleBackground(sprite);

        public static void SetToMousePosition(this VisualElement ve, VisualElement parent, Vector2 pivot)
        {
            pivot.x = Mathf.Clamp01(pivot.x);
            pivot.y = Mathf.Clamp01(pivot.y);
            
            Vector2 offset;
            offset.x = ve.layout.width * pivot.x;
            offset.y = ve.layout.height * pivot.y;
            
            float canvasWidth = parent.GetDocumentRoot().resolvedStyle.width;
            float canvasHeight = parent.GetDocumentRoot().resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
          
            Vector3 pos = ve.transform.position;
            
            pos.x = scaledMousePosition.x - offset.x;
            pos.y = canvasHeight - scaledMousePosition.y - offset.y;

            ve.transform.position = parent.ChangeCoordinatesTo(ve.parent, pos);
        }

        public static List<string> ParseChoiceList(string choices)
        {
            if (string.IsNullOrEmpty(choices.Trim()))
            {
                return null;
            }

            string[] array = choices.Split(new char[1] { ',' });
            if (array.Length != 0)
            {
                List<string> list = new List<string>();
                string[] array2 = array;
                foreach (string text in array2)
                {
                    list.Add(text.Trim());
                }

                return list;
            }

            return null;
        }

        public static VisualElement GetDocumentRoot(this VisualElement ve)
        {
            return (ve.parent == null ? ve : ve.parent.GetDocumentRoot());
        }

        public static VisualElement GetDocumentScreen(this VisualElement ve)
        {
            VisualElement root = GetDocumentRoot(ve);
            VisualElement screen = root.Q<VisualElement>("Screen");
            return screen;
        }

        public static void RemoveUnityClassStyle(this VisualElement ve)
        {
            if (ve is Label)
            {
                ve.RemoveFromClassList("unity-label");
                ve.RemoveFromClassList("unity-text-element");
            }

            if(ve is ProgressBar)
            {
                ve.RemoveFromClassList("unity-progress-bar");

                VisualElement container = ve.ElementAt(0);
                VisualElement background = container.ElementAt(0);
                VisualElement progressBar = background.ElementAt(0);
                VisualElement titleContainer = background.ElementAt(1);
                VisualElement title = titleContainer.ElementAt(0);

                container.RemoveFromClassList(AbstractProgressBar.containerUssClassName);
                background.RemoveFromClassList(AbstractProgressBar.backgroundUssClassName);
                //progressBar.RemoveFromClassList(AbstractProgressBar.progressUssClassName);
                titleContainer.RemoveFromClassList(AbstractProgressBar.titleContainerUssClassName);
                title.RemoveFromClassList(AbstractProgressBar.titleUssClassName);
                title.RemoveUnityClassStyle();
            }
        }

        public static void FadeScaleVisualElement(this VisualElement ve, Vector2 to, float step)
        {
            Vector2 scale = ve.resolvedStyle.scale.value;
            if ((to-scale).magnitude >= 0.001f)
            {
                scale = Vector3.Lerp(scale, to, Time.deltaTime * step);
                ve.style.scale = new StyleScale(scale);
            }
            else
            {
                ve.style.scale = new StyleScale(to);
            }
        }
        
        
        
        public static void FadeInVisualElement(this VisualElement visualElement)
        {
            var op = visualElement.resolvedStyle.opacity;
            if (op >= 0.001f)
            {
                visualElement.ShowVisualElement();
            }
            op += 0.2f;
            visualElement.style.opacity = op;
            if (op >= 0.98f)
            {
                visualElement.style.opacity = 1;
            }
        }

        public static void FadeOutVisualElement(this VisualElement visualElement)
        {
           
            var op = visualElement.resolvedStyle.opacity;
            op -= 0.2f;
            visualElement.style.opacity = op;
            if (op <= 0.1f)
            {
                visualElement.HideVisualElement();
                visualElement.style.opacity = 0;
            }
        }

        public static void VisibleVisualElement(this VisualElement visualElement)
        {
            visualElement.style.visibility = Visibility.Visible;
        }
        public static void HiddenVisualElement(this VisualElement visualElement)
        {
            visualElement.style.visibility = Visibility.Hidden;
        }
        
    }
}
