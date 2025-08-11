using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIButtonToggleGroup : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIButtonToggleGroup, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription itemsAttribute = new UxmlStringAttributeDescription { defaultValue = "Item1,Item2", name = "Items" };
            private UxmlStringAttributeDescription itemClassNameAttribute = new UxmlStringAttributeDescription { defaultValue = "unity-button", name = "item-Class-Name" };
            private UxmlStringAttributeDescription selectedItemClassNameAttribute = new UxmlStringAttributeDescription { defaultValue = "unity-button__selected", name = "selected-item-Class-Name" };

            private UxmlColorAttributeDescription backgroundColorNormalAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(0, 0, 0, 0), name = "Background-Color-Normal" };
            private UxmlColorAttributeDescription textColorNormalAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(158 / 255f, 154 / 255f, 147 / 255f, 1), name = "Text-Color-Normal" };
            private UxmlColorAttributeDescription backgroundColorCheckedAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(0, 0, 0, 0), name = "Background-Color-Checked" };
            private UxmlColorAttributeDescription textColorCheckedAttribute = new UxmlColorAttributeDescription { defaultValue = new Color(223 / 255f, 205 / 255f, 177 / 255f, 1), name = "Text-Color-Checked" };

            private UxmlBoolAttributeDescription canDeselectAttribute = new UxmlBoolAttributeDescription { defaultValue = false, name = "Can-Deselect" };
            private UxmlIntAttributeDescription defaultItemIndexAttribute = new UxmlIntAttributeDescription { defaultValue = -1, name = "Default-Item-Index" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIButtonToggleGroup script = (UIButtonToggleGroup)ve;

                script.itemClassName = itemClassNameAttribute.GetValueFromBag(bag, cc);
                script.selectedItemClassName = selectedItemClassNameAttribute.GetValueFromBag(bag, cc);

                script.Items = itemsAttribute.GetValueFromBag(bag, cc);

                script.BackgroundColorNormal = backgroundColorNormalAttribute.GetValueFromBag(bag, cc);
                script.TextColorNormal = textColorNormalAttribute.GetValueFromBag(bag, cc);
                script.BackgroundColorChecked = backgroundColorCheckedAttribute.GetValueFromBag(bag, cc);
                script.TextColorChecked = textColorCheckedAttribute.GetValueFromBag(bag, cc);

                script.CanDeselect = canDeselectAttribute.GetValueFromBag(bag, cc);
                script.DefaultItemIndex = defaultItemIndexAttribute.GetValueFromBag(bag, cc);

                script.CreateButtons(script.Items);
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        public Action<int> OnItemIndexChanged;

        protected string itemClassName { get; set; }
        protected string selectedItemClassName { get; set; }
        protected string items;
        public string Items
        {
            get { return items; }
            set
            {
                items = value;
                if (string.IsNullOrEmpty(items))
                    itemsArray = null;
                else itemsArray = items.Split(',');
            }
        }
        
        public Color TextColorNormal { get; set; }
        public Color TextColorChecked { get; set; }
        public Color BackgroundColorNormal { get; set; }
        public Color BackgroundColorChecked { get; set; }

        public int DefaultItemIndex { get; set; }
        public bool CanDeselect { get; set; }

        protected string[] itemsArray;
        public string[] ItemsArray => itemsArray;
        protected List<UIButtonToggle> listofItems;
        protected int itemIndex;
        public int ItemIndex { get { return itemIndex; } }

        public UIButtonToggleGroup(): base()
        {
            focusable = false;
            //style.flexDirection = FlexDirection.Row;
        }

        /// <summary>
        /// More buttons seperate by comma. Example: "Button1,Button2,Button3"
        /// </summary>
        /// <param name="buttonNames"></param>
        public virtual void CreateButtons(string buttonNames)
        {
            Items = buttonNames;

            deleteButtons();
            createButtons(itemClassName);
            addButtons();
        }

        protected virtual void addButtons()
        {
            for (int n = 0; n < listofItems.Count; n++)
                Add(listofItems[n]);
        }

        private void createButtons(string className)
        {
            if (listofItems == null)
                listofItems = new List<UIButtonToggle>();
            else listofItems.Clear();

            if (itemsArray != null)
            {
                for (int n = 0; n < itemsArray.Length; n++)
                {
                    UIButtonToggle button = new UIButtonToggle();
                    button.name = "Item " + (n + 1).ToString();
                    button.text = itemsArray[n];
                    button.AddToClassList(className);
                     button.RemoveFromClassList("unity-button");
                    button.BackgroundColorNormal = BackgroundColorNormal;
                    button.TextColorNormal = TextColorNormal;
                    button.BackgroundColorChecked = BackgroundColorChecked;
                    button.TextColorChecked = TextColorChecked;
                    button.viewDataKey = n.ToString();
                    listofItems.Add(button);

                    button.RegisterCallback<MouseUpEvent>(onItemClicked);

                    if (DefaultItemIndex == n)
                    {
                        button.IsOn = true;
                        button.AddToClassList(selectedItemClassName);
                    }
                }
            }
        }

        private void deleteButtons()
        {
            if (listofItems != null)
            {
                for (int n = 0; n < listofItems.Count; n++)
                {
                    listofItems[n].UnregisterCallback<MouseUpEvent>(onItemClicked);
                    listofItems[n].RemoveFromHierarchy();
                }
            }
        }

        private void onItemClicked(MouseUpEvent evt)
        {
            UIButtonToggle b = (UIButtonToggle)evt.target;
            int index = Convert.ToInt32(b.viewDataKey);
            Set(index);
        }

        public virtual void Set(int index, bool notify = true)
        {
            if (listofItems == null || listofItems.Count == 0)
            {
                itemIndex = -1;
                return;
            }

            if (CanDeselect)
                itemIndex = Mathf.Clamp(index, -1, listofItems.Count);
            else itemIndex = Mathf.Clamp(index, 0, listofItems.Count);

            for (int n = 0; n < listofItems.Count; n++)
            {
                if (n == itemIndex)
                {
                    listofItems[n].IsOn = true;
                    listofItems[n].AddToClassList(selectedItemClassName);
                    if (notify)
                    {
                        if (OnItemIndexChanged != null)
                            OnItemIndexChanged.Invoke(itemIndex);
                    }

                    continue;
                }

                listofItems[n].IsOn = false;
                listofItems[n].RemoveFromClassList(selectedItemClassName);
            }
        }
    }
}