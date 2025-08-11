using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismCircularButton 
    {
        #region Fields

        public VisualElement Icon;
        public string Text;
        public string Description;
        public UnityEvent Action;
        public VisualElement m_Root;
        #endregion

        
        
        #region Methods

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement.Q<VisualElement>("button");
            Icon = visualElement.Q<VisualElement>("icon");
        }
        
        public void Init(string name, string description, Sprite sprite, UnityEvent action)
        {
            Text = name;
            Description = description;

            if (sprite != null)
                Icon.SetBackgroundImage(sprite);

            Action = action;
        }

        #endregion
    }

    [Serializable]
    public class UIAtavismCircularButtonData
    {
        #region Fields

        public Sprite Icon;
        public int Order;
        public string Text;
        public string Description;
        public UnityEvent Action;

        #endregion
    }
}