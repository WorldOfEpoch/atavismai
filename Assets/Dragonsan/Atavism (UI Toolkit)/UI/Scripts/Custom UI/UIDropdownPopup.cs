using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIDropdownPopup : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIDropdownPopup, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription dropdownAttribute = new UxmlStringAttributeDescription { name = "Dropdown-Name", defaultValue = "UIDropdown" };
            private UxmlIntAttributeDescription indexAttribute = new UxmlIntAttributeDescription { name = "Index" };
            private UxmlStringAttributeDescription optionsAttribute = new UxmlStringAttributeDescription { name = "Options", defaultValue = "Option 1,Option 2" };
           
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIDropdownPopup dropdownPopup = (UIDropdownPopup)ve;
                dropdownPopup.DropdownName = dropdownAttribute.GetValueFromBag(bag, cc);
                dropdownPopup.Index = indexAttribute.GetValueFromBag(bag, cc);
                dropdownPopup.Options = optionsAttribute.GetValueFromBag(bag, cc);
                dropdownPopup.generateLayout();
            }
        }

        public string ussClassName = "UIDropdown";

        public string popupUssClassName
        {
            get { return ussClassName + "__popup"; }
        }
        public string scrollUssClassName
        {
            get { return ussClassName + "__scroll"; }
        }

        public string popupSelectedUssClassName  {
            get { return  ussClassName + "__popup:show";}
        }
        public string itemUssClassName  {
            get { return  ussClassName + "__item";
            }
        }
        public string selectedUssClassName  {
            get { return  ussClassName + "__selected";
            }
        }
        public string DropdownName { get; set; }
        public string Options { get; set; }
        public Dictionary<int,string> OptionDictionary { get; set; }
        public int Index { get; private set; }
        public bool IsPopupOpen { get; private set; }

        private List<string> listofOptions;
        private List<int> listofOptionsIds;
        public VisualElement uiPopupPanel;
        private ScrollView uiScrollPanel;
        private UIDropdown uiDropdown;
        private List<Label> listofItems;

        public void SetDropdown(UIDropdown dropdown)
        {
            uiDropdown = dropdown;
        }
        public UIDropdownPopup() : base()
        {
            style.position = Position.Absolute;
            // Debug.LogError("UIDropdownPopup()");
            this.RegisterCallback<AttachToPanelEvent>(onAttachToPanel);
            this.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
        }

        private void onAttachToPanel(AttachToPanelEvent evt)
        {
            generateItems();

            if (uiDropdown == null)
            {
                uiDropdown = this.parent.Query<UIDropdown>(DropdownName);
                if (uiDropdown != null)
                    uiDropdown.Initialize(this);
            }
            if(listofItems.Count > 0)
            Select(Index);
        }

        private void onGeometryChanged(GeometryChangedEvent evt)
        {
            generateItems();
            if(listofItems.Count > 0)
            Select(Index);
        }

        public void RegisterValueChangedCallback()
        {
            
        }


        public void Select(int index)
        {
            if (listofOptions == null)
            {
                Debug.LogWarning("Dropdown options not set");
                return;
            }
            if (listofOptions.Count == 0 && index >= 0)
            {
                Debug.LogWarning("Dropdown options not set");
                return;
            }

            int previousValue = this.Index;
            if (index >= 0 && index < listofOptions.Count)
                this.Index = index;
            else
            {
                Debug.LogWarning("Dropdown option not found "+index+" in "+Options);
                this.Index = -1;
            }
            using (ChangeEvent<int> pooled = ChangeEvent<int>.GetPooled(previousValue, this.Index))
            {
                pooled.target = (IEventHandler)uiDropdown;
                this.SendEvent((EventBase)pooled);
            }
            using (ChangeEvent<string> pooled = ChangeEvent<string>.GetPooled(previousValue>=0?listofOptions.Count>previousValue?listofOptions[previousValue]:"":"", this.Index>=0?listofOptions.Count>this.Index?listofOptions[this.Index]:"":""))
            {
                pooled.target = (IEventHandler) uiDropdown;
                this.SendEvent((EventBase) pooled);
            }

            if (listofItems.Count > previousValue && previousValue >= 0)
            {
                listofItems[previousValue].RemoveFromClassList(selectedUssClassName);
            }

            if (listofItems.Count > this.Index && this.Index >= 0)
            {
                listofItems[this.Index].AddToClassList(selectedUssClassName);
            }

            if (uiDropdown != null)
                uiDropdown.UpdateData();
        }
        public void ClearSelection() => Select(-1);
        public string GetSelectedOption()
        {
            if(listofOptions != null)
            {
                if (Index >= 0 && Index < listofOptions.Count)
                    return listofOptions[Index];
            }

            return "";
        }
        public int GetSelectedOptionId()
        {
            if(listofOptionsIds != null)
            {
                if (Index >= 0 && Index < listofOptionsIds.Count)
                    return listofOptionsIds[Index];
            }

            return -1;
        }

        public void ShowPopup(VisualElement positionToElement)
        {
            IsPopupOpen = true;
            if (uiDropdown == null)
            {
                uiDropdown = this.parent.Query<UIDropdown>(DropdownName);
                if (uiDropdown != null)
                    uiDropdown.Initialize(this);
            }
            uiPopupPanel.style.width = uiDropdown.layout.width;
            uiPopupPanel.RegisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
            uiDropdown.Screen.Add(uiPopupPanel);
            uiPopupPanel.ShowVisualElement();
            
          
            // uiPopupPanel.
        }

        private void onPopupMenuGeometryChanged(GeometryChangedEvent evt)
        {
            if (!(uiDropdown.Screen is UIDropdown))
            {
                uiPopupPanel.style.top = uiDropdown.worldBound.y + uiDropdown.resolvedStyle.height;
                uiPopupPanel.style.left = uiDropdown.worldBound.x;
            }
            else
            {
                 Debug.LogWarning("uiScreen is not assigned to UIDropdown popup can be shown under other ui elements");
            }
            uiPopupPanel.SetEnabled(true);
            uiPopupPanel.UnregisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
        }
        public void HidePopup()
        {
            uiPopupPanel.UnregisterCallback<GeometryChangedEvent>(onPopupMenuGeometryChanged);
            IsPopupOpen = false;
            uiPopupPanel.SetEnabled(false);

            uiPopupPanel.HideVisualElement();
        }

        private void onItemClicked(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                int index = Convert.ToInt32(((Label)evt.target).viewDataKey);
                Select(index);

                HidePopup();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void generateLayout()
        {
            uiPopupPanel = this.Q<VisualElement>("popup-panel");
            if (uiPopupPanel == null)
                uiPopupPanel = new VisualElement();
            uiPopupPanel.name = "popup-panel";
            uiPopupPanel.style.position = Position.Absolute;
            uiPopupPanel.style.flexGrow = 1f;
            uiPopupPanel.style.flexShrink = 0f;
            uiPopupPanel.focusable = true;
            uiPopupPanel.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            uiPopupPanel.AddToClassList(popupUssClassName);
            uiScrollPanel = this.Q<ScrollView>("popup-scroll");
            if (uiScrollPanel == null)
               uiScrollPanel = new ScrollView();
#if UNITY_6000_0_OR_NEWER    
                uiScrollPanel.mouseWheelScrollSize = 19;
#endif
            uiScrollPanel.name = "popup-scroll";
            uiScrollPanel.AddToClassList(scrollUssClassName);
            uiPopupPanel.Add(uiScrollPanel);
            uiPopupPanel.RegisterCallback<FocusInEvent>(onFocusIn);
            uiPopupPanel.RegisterCallback<FocusOutEvent>(onFocusOut);
            Add(uiPopupPanel);

            generateItems();
            HidePopup();
        }
        
        private void onFocusIn(FocusInEvent evt)
        {
        }

        private void onFocusOut(FocusOutEvent evt)
        {
        }
        public void generateItems()
        {
            if (string.IsNullOrEmpty(Options))
                listofOptions = new List<string>();
            else
                listofOptions = UIToolkit.ParseChoiceList(Options);
            if (OptionDictionary != null)
            {
                foreach (var key in OptionDictionary.Keys)
                {
                    listofOptions.Add(OptionDictionary[key]);
                    listofOptionsIds.Add(key);
                }
            }

            clearItems();
            if (listofOptions != null)
            {
                for (int n = 0; n < listofOptions.Count; n++)
                {
                    Label label = addItem(listofOptions[n]);
                    label.viewDataKey = n.ToString();
                    listofItems.Add(label);
                }
            }

            if(listofItems.Count > 0 && Index<0)
                Select(0);
            else if(listofItems.Count > 0)
                Select(Index);
        }
        private void clearItems()
        {
            if (listofItems != null)
            {
                for (int n = 0; n < listofItems.Count; n++)
                    listofItems[n].RemoveFromHierarchy();
                listofItems.Clear();
            }
            else listofItems = new List<Label>();
        }
        private Label addItem(string title)
        {
            Label label = new Label();
            label.name = title;
            label.text = title;
            label.RegisterCallback<MouseUpEvent>(onItemClicked);
            label.AddToClassList(itemUssClassName);
            uiScrollPanel.Add(label);

            return label;
        }
    }
}