using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismGlobalEventEntry 
    {
        private bool mouseEntered = false;

        public VisualElement m_icon;
        public VisualElement m_root;
        private string name = "";
        private string description = "";
        private int id = -1;

        public void SetVisualElement(VisualElement visualElement)
        {
            m_root = visualElement;
            m_icon = m_root.Q<VisualElement>("icon");
            m_root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            
        }
        private void OnEnable()
        {
            AtavismEventSystem.RegisterEvent("GLOABL_EVENTS_ICON", OnEvent);
            
        }

        private void OnDisable()
        {
            AtavismEventSystem.UnregisterEvent("GLOABL_EVENTS_ICON", OnEvent);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "GLOABL_EVENTS_ICON")
            {
                loadIcon();
            }
        }
        public void UpdateDisplay(GlobalEvent ge)
        {
            this.name = ge.name;
            this.description = ge.description;
            this.id = ge.id;
            loadIcon();
        }
       
      void loadIcon()
        {
            if (id > 0)
            {
                if (m_icon != null)
                {
                    m_icon.style.backgroundImage = AtavismPrefabManager.Instance.GetGlobalEventIconByID(id).texture;
                }
            }
        }

        public void OnPointerEnter(MouseEnterEvent evt)
        {
#if !AT_MOBILE
            MouseEntered = true;
#endif
        }

        public void OnPointerExit(MouseLeaveEvent evt)
        {
#if !AT_MOBILE
            MouseEntered = false;
#endif
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();

        }
        void ShowTooltip()
        {
#if AT_I2LOC_PRESET
        UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation(name));
        UIAtavismTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation(description));
#else
            UIAtavismTooltip.Instance.SetTitle(name);
            UIAtavismTooltip.Instance.SetDescription(description);
#endif
            UIAtavismTooltip.Instance.SetQualityColor(AtavismSettings.Instance.effectQualityColor);

            UIAtavismTooltip.Instance.SetType("");
            UIAtavismTooltip.Instance.SetWeight("");
            if(m_icon!=null)
                UIAtavismTooltip.Instance.SetIcon(AtavismPrefabManager.Instance.GetGlobalEventIconByID(id));
            UIAtavismTooltip.Instance.Show(m_root);
        }
        
        public bool MouseEntered
        {
            get { return mouseEntered; }
            set
            {
                mouseEntered = value;
                if (mouseEntered )
                {
                    ShowTooltip();
                }
                else
                {
                    HideTooltip();
                }
            }
        }

       
    }
}