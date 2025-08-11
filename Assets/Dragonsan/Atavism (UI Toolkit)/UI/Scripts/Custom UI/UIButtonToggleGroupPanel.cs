using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIButtonToggleGroupPanel : UIButtonToggleGroup
    {
        public new class UxmlFactory : UxmlFactory<UIButtonToggleGroupPanel, UxmlTraits> { }
        public new class UxmlTraits : UIButtonToggleGroup.UxmlTraits
        {
            private UxmlIntAttributeDescription headerButtonPanelHeightAttribute = new UxmlIntAttributeDescription { defaultValue = 80, name = "Header-Button-Height" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                UIButtonToggleGroupPanel script = (UIButtonToggleGroupPanel)ve;

                script.headerButtonHeight = headerButtonPanelHeightAttribute.GetValueFromBag(bag, cc);

                base.Init(ve, bag, cc);
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        private int headerButtonHeight { get; set; }
        private VisualElement headerButtonPanel;
        private VisualElement contentPanelContainer;
        private List<ScrollView> contentScrollPanels;
        private List<VisualElement> contentPanels;

        public bool ContentWithScroll { get; set; }
        public UIButtonToggleGroupPanel() : base()
        {
            focusable = false;
            style.flexDirection = FlexDirection.Column;
            style.alignItems = Align.Center;
        }

        public override void CreateButtons(string buttonNames)
        {
            if (headerButtonPanel != null)
                headerButtonPanel.RemoveFromHierarchy();
            headerButtonPanel = new VisualElement();
            headerButtonPanel.name = "header-button-panel";
            headerButtonPanel.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            headerButtonPanel.style.height = headerButtonHeight;
            headerButtonPanel.style.flexDirection = FlexDirection.Row;
            headerButtonPanel.style.flexShrink = 0f;
            headerButtonPanel.AddToClassList("UI-BTGP__header");
            Add(headerButtonPanel);

            base.CreateButtons(buttonNames);

            // Container
            if (contentPanelContainer != null)
                contentPanelContainer.RemoveFromHierarchy();
            contentPanelContainer = new VisualElement();
            contentPanelContainer.name = "container";
            contentPanelContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            contentPanelContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            contentPanelContainer.style.paddingLeft = 10;
            contentPanelContainer.style.paddingRight = 10;
            contentPanelContainer.style.paddingBottom = 10;
            contentPanelContainer.style.paddingTop = 10;
            contentPanelContainer.AddToClassList("UI-BTGP__container");
            Add(contentPanelContainer);

            // Content panels
            if (contentPanels != null && contentPanels.Count > 0)
                for (int n = 0; n < contentPanels.Count; n++)
                    contentPanels[n].RemoveFromHierarchy();
            if (contentScrollPanels != null && contentScrollPanels.Count > 0)
                for (int n = 0; n < contentScrollPanels.Count; n++)
                    contentScrollPanels[n].RemoveFromHierarchy();

            if (ContentWithScroll)
            {
                if (contentScrollPanels == null)
                    contentScrollPanels = new List<ScrollView>();
                else contentScrollPanels.Clear();

                for (int n = 0; n < listofItems.Count; n++)
                {
                    ScrollView content = new ScrollView();
                    content.name = "content-" + listofItems[n].name;
                    content.style.display = DisplayStyle.None;
                    content.AddToClassList("UI-BTGP__panel");
                    contentScrollPanels.Add(content);
                    contentPanelContainer.Add(content);
                }
            }
            else
            {
                if (contentPanels == null)
                    contentPanels = new List<VisualElement>();
                else contentPanels.Clear();

                for (int n = 0; n < listofItems.Count; n++)
                {
                    VisualElement content = new VisualElement();
                    content.name = "content-" + listofItems[n].name;
                    content.style.display = DisplayStyle.None;
                    content.AddToClassList("UI-BTGP__panel");
                    contentPanels.Add(content);
                    contentPanelContainer.Add(content);
                }
            }
        }

        public void SelectPanel(int index)
        {
            Set(index, true);
        }

        public override void Set(int index, bool notify = true)
        {
            base.Set(index, notify);
            if (ContentWithScroll)
            {
                if (contentScrollPanels != null)
                {
                    for (int n = 0; n < contentScrollPanels.Count; n++)
                    {
                        if (n == index)
                            contentScrollPanels[n].style.display = DisplayStyle.Flex;
                        else contentScrollPanels[n].style.display = DisplayStyle.None;
                    }
                }
            }
            else
            {

                if (contentPanels != null)
                {
                    for (int n = 0; n < contentPanels.Count; n++)
                    {
                        if (n == index)
                            contentPanels[n].style.display = DisplayStyle.Flex;
                        else contentPanels[n].style.display = DisplayStyle.None;
                    }
                }
            }
        }

        protected override void addButtons()
        {
            if (listofItems != null && headerButtonPanel != null)
                for (int n = 0; n < listofItems.Count; n++)
                    headerButtonPanel.Add(listofItems[n]);
        }

        public void AddItem(VisualElement e, int containerIndex)
        {
            if (ContentWithScroll)
            {
                contentScrollPanels[containerIndex].Add(e);
            }
            else
            {
                contentPanels[containerIndex].Add(e);
            }
        }
    }
}