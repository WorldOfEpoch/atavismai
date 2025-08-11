using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    /// <summary>
    /// ToDo: Tooltip (required Tootip designed in UI Toolkit)
    /// </summary>
    public class UIAtavismItemDisplay : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIAtavismItemDisplay, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // private UxmlBoolAttributeDescription showTooltipAttribute = new UxmlBoolAttributeDescription { defaultValue = true, name = "show-Tooltip" };
            // private UxmlFloatAttributeDescription tooltipDelayAttribute = new UxmlFloatAttributeDescription { defaultValue = 0, name = "tooltip-Delay" };
            private UxmlBoolAttributeDescription showTitleAttribute = new UxmlBoolAttributeDescription { name = "Show-Title", defaultValue = true };
            private UxmlBoolAttributeDescription showCountAttribute = new UxmlBoolAttributeDescription { name = "Show-Count", defaultValue = true };
            private UxmlStringAttributeDescription classStyleAttribute = new UxmlStringAttributeDescription { name = "Class-Style", defaultValue = "ItemDisplay" };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIAtavismItemDisplay script = (UIAtavismItemDisplay)ve;
                script.showTitle = showTitleAttribute.GetValueFromBag(bag, cc);
                script.showCount = showCountAttribute.GetValueFromBag(bag, cc);
                script.ussClassName = classStyleAttribute.GetValueFromBag(bag, cc);
                //script.showTooltip = showTooltipAttribute.GetValueFromBag(bag, cc);
                script.generate();
            }
        }

        public string ussClassName { get; set; }

        public string frameUssClassName
        {
            get
            {
                return ussClassName + "__frame";
            }
        }

        public string iconUssClassName
        {
            get { return ussClassName + "__icon"; }
        }

        public string titleUssClassName
        {
            get { return ussClassName + "__title"; }
        }  
        public string countUssClassName
        {
            get { return ussClassName + "__count"; }
        }

        private VisualElement uiIconFrame, uiIcon;
        private Label uiTitle;
        private Label uiCount;
        private bool showTitle;
        private bool showCount;
        private AtavismInventoryItem item;
        public MonoBehaviour MonoController { get; set; }

        public UIAtavismItemDisplay() : base()
        {
            // generate();
        }

        ~UIAtavismItemDisplay()
        {
            AtavismEventSystem.UnregisterEvent("ITEM_ICON_UPDATE", OnEvent);
        }
        
        private void generate(){
        pickingMode = PickingMode.Ignore;

            if (uiIconFrame != null)
                uiIconFrame.RemoveFromHierarchy();
            uiIconFrame = new VisualElement();
            uiIconFrame.name = "Icon-frame";
            uiIconFrame.AddToClassList(frameUssClassName);
            Add(uiIconFrame);

            if (uiIcon != null)
                uiIcon.RemoveFromHierarchy();
            uiIcon = new VisualElement();
            uiIcon.name = "Icon";
            uiIcon.AddToClassList(iconUssClassName);
            uiIconFrame.Add(uiIcon);
            
            if (uiCount != null)
                uiCount.RemoveFromHierarchy();
            if (showCount)
            {
                uiCount = new Label();
                uiCount.name = "Count";
                uiCount.text = "";
                uiCount.pickingMode = PickingMode.Ignore;
                uiCount.RemoveUnityClassStyle();
                uiCount.AddToClassList(countUssClassName);
                uiIcon.Add(uiCount);
            }

            if (uiTitle != null)
                uiTitle.RemoveFromHierarchy();
            if (showTitle)
            {
                uiTitle = new Label();
                uiTitle.name = "Title";
                uiTitle.text = "Item Display";
                uiTitle.pickingMode = PickingMode.Ignore;
                uiTitle.RemoveUnityClassStyle();
                uiTitle.AddToClassList(titleUssClassName);
                Add(uiTitle);
            }

            RegisterCallback<MouseEnterEvent>(onMouseEnter);
            RegisterCallback<MouseLeaveEvent>(onMouseLeave);
            RegisterCallback<MouseUpEvent>(onMouseUp);
            AtavismEventSystem.RegisterEvent("ITEM_ICON_UPDATE", OnEvent);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ITEM_ICON_UPDATE")
            {
                // Update 
                SetItemData(this.item);
            }
        }

        private void onMouseEnter(MouseEnterEvent evt)
        {
            if(item!=null)
                item.ShowUITooltip(uiIcon);
        }

        private void onMouseLeave(MouseLeaveEvent evt)
        {
            UIAtavismTooltip.Instance.Hide();
        }

        private void onMouseUp(MouseUpEvent evt)
        {
        }

        public void Reset()
        {
            if (uiTitle != null)
            {
                uiTitle.text = "";
                uiTitle.style.color = StyleKeyword.Null;
            }
            if (uiCount != null)
            {
                uiCount.text = "";
            }

            uiIconFrame.style.unityBackgroundImageTintColor = StyleKeyword.Null;
            uiIcon.style.backgroundImage = StyleKeyword.Null;
        }
        
        public void SetItemData(AtavismInventoryItem item)
        {
            this.item = item;
            if (item == null)
            {
                Reset();
                return;
            }

            if (uiTitle != null)
            {
                uiTitle.text = item.name;
                uiTitle.style.color = AtavismSettings.Instance.ItemQualityColor(item.Quality);
            }

            if (uiCount != null)
            {
                if (item.Count > 1)
                    uiCount.text = item.Count.ToString();
                else
                    uiCount.text = "";
            }
            
            uiIconFrame.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.Quality);
            uiIcon.SetBackgroundImage(item.Icon);
        }
    }
}