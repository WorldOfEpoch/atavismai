using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismAchievementSlot 
    {
        private Label m_name;
        private Label m_status;
        private UIProgressBar m_progress;
       
        bool mouseEntered = false;
        AtavismAchievement achievement;
        private VisualElement root;
        public void SetVisualElement(VisualElement visualElement)
        {
            root = visualElement;
            root.RegisterCallback<MouseDownEvent>(Click);
            root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);

            m_name = visualElement.Q<Label>("name");
            m_status = visualElement.Q<Label>("status");
            m_progress = visualElement.Q<UIProgressBar>("progress");
        }

        
        
        // Start is called before the first frame update
        public void SetData(AtavismAchievement achievement)
        {
            this.achievement = achievement;

            if (m_name != null)
            {
#if AT_I2LOC_PRESET
                m_name.text = I2.Loc.LocalizationManager.GetTranslation(achievement.name);
#else
                m_name.text = achievement.name;
#endif
                m_name.RemoveFromClassList("achievement-active-title");
                m_name.RemoveFromClassList("achievement-active");
                if (achievement.active)
                {
                    if (ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).PropertyExists("title") && ((string)ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).GetProperty("title")).Equals(achievement.name))
                    {
                        m_name.AddToClassList("achievement-active-title");
                    }
                    else
                    {
                        m_name.AddToClassList("achievement-active");
                    }
                }
               
            }
            else
            {
                // Debug.LogError("Text componet is not assigned");
            }

            if(m_status != null)
                m_status.text = achievement.value + "/" + achievement.max;    
            // else
                // Debug.LogError("Text component is not assigned");

            if (m_progress != null)
            {
                m_progress.lowValue = 0;
                m_progress.highValue = achievement.max;
                m_progress.value = achievement.value;
            }

        }
    
        public void Click(MouseDownEvent evt)
        {
            // Debug.LogError("Click");
            if (achievement != null && achievement.id > 0)
            {
                ClientAPI.GetPlayerObject();
                Dictionary<string, object> props = new Dictionary<string, object>();
                if (ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).PropertyExists("title") && ((string)ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).GetProperty("title")).Equals(achievement.name))
                    props.Add("id", 0);
                else
                    props.Add("id", achievement.id);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.SET_ACHIEVEMENTS_TITLE", props);
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
        void ShowTooltip()
        {
            if (achievement == null)
                return;
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation(achievement.name));
#else
            UIAtavismTooltip.Instance.SetTitle(achievement.name);
#endif
            UIAtavismTooltip.Instance.HideType(true);
            UIAtavismTooltip.Instance.HideWeight(true);
            UIAtavismTooltip.Instance.EnableIcon(false);
            if (AtavismVip.Instance.GetMaxPoints > 0)
#if AT_I2LOC_PRESET
               UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Progress"),achievement.value + " / " + achievement.max, true);
#else
                UIAtavismTooltip.Instance.AddAttribute("Progress ", achievement.value + " / " + achievement.max, true);
#endif
            //  UGUITooltip.Instance.AddAttribute(AtavismVip.Instance.GetPoints+" / "+ AtavismVip.Instance.GetMaxPoints , "", true);
            UIAtavismTooltip.Instance.SetDescription(achievement.desc);
            UIAtavismTooltip.Instance.Show(root);
        }
        
        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }
        public bool MouseEntered
        {
            get
            {
                return mouseEntered;
            }
            set
            {
                mouseEntered = value;
                if (mouseEntered)
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