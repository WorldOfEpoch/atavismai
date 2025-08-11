using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismVip : MonoBehaviour
    {
        [SerializeField] public UIDocument uiDocument;
        public Sprite[] backgrounds;
        private VisualElement background;
        private Label level;
        public bool showZeroLevel;
        bool mouseEntered = false;
        bool started = false;
        private VisualElement m_Root;
        // Start is called before the first frame update
        protected virtual void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();

            uiDocument.enabled = true;
            m_Root = uiDocument.rootVisualElement.Q<VisualElement>("vip-panel");
            background = uiDocument.rootVisualElement.Q<VisualElement>("background");
            level= uiDocument.rootVisualElement.Q<Label>("level");
            
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseUpEvent>(OnPointerClick);
            AtavismEventSystem.RegisterEvent("VIP_UPDATE", this);
            AtavismEventSystem.RegisterEvent("LOADING_SCENE_END", this);
            
        }

        private void OnPointerClick(MouseUpEvent evt)
        {
           AtavismEventSystem.DispatchEvent("VIP_SHOW",new string[]{});
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("VIP_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("LOADING_SCENE_END", this);
        }
        // Update is called once per frame
        void Update()
        {
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            int lev = AtavismVip.Instance.GetLevel;
            if (level!=null)
            {
                if (started)
                {
                    if (!level.text.Equals(lev.ToString()))
                    {
                        string[] event_args = new string[1];
                        AtavismEventSystem.DispatchEvent("VIP_LEVELUP", event_args);
                    }
                }
                level.text = lev.ToString();
                if (background!=null)
                {
                    if (backgrounds.Length > lev)
                        background.SetBackgroundImage(backgrounds[lev]);
                    if (lev == 0 && !showZeroLevel)
                    {
                        m_Root.HideVisualElement();
                        // level.enabled = false;
                        // background.enabled = false;
                    }
                    else
                    {
                        m_Root.ShowVisualElement();
                        // level.enabled = true;
                        // background.enabled = true;
                    }
                }
                started = true;
            }
            if (AtavismVip.Instance.GetTime != 0)
            {

                float timeLeft = AtavismVip.Instance.GetTimeElapsed - Time.time;
                if (background!=null)
                {
                    if (timeLeft > 0)
                        background.RemoveFromClassList("vip-expired-icon");// = Color.white;
                    else
                    background.AddToClassList("vip-expired-icon");//color = Color.gray);

                }
            }
            else
            {
                background.RemoveFromClassList("vip-expired-icon");
            }
        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "VIP_UPDATE")
            {
                UpdateDisplay();
            }  else if (eData.eventType == "LOADING_SCENE_END")
            {
                AtavismVip.Instance.GetVipStatus();
                UpdateDisplay();
            }
        }

        public void OnPointerEnter(MouseEnterEvent evt)
        {
            MouseEntered = true;
        }

        public void OnPointerExit(MouseLeaveEvent evt)
        {
            MouseEntered = false;
        }
        void ShowTooltip()
        {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation(AtavismVip.Instance.GetName));
#else
            UIAtavismTooltip.Instance.SetTitle(AtavismVip.Instance.GetName);
#endif
            UIAtavismTooltip.Instance.HideType(true);
            UIAtavismTooltip.Instance.EnableIcon(false);
            if(AtavismVip.Instance.GetMaxPoints > 0)
#if AT_I2LOC_PRESET
               UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Points"),AtavismVip.Instance.GetPoints + " / " + AtavismVip.Instance.GetMaxPoints, true);
#else
                UIAtavismTooltip.Instance.AddAttribute("Points ",AtavismVip.Instance.GetPoints + " / " + AtavismVip.Instance.GetMaxPoints, true);
#endif
            foreach (Bonus b in AtavismVip.Instance.GetBonuses)
            {
                string bonusName = b.name;
#if AT_I2LOC_PRESET
                bonusName = I2.Loc.LocalizationManager.GetTranslation(bonusName);
#endif
                if (b.value != 0)
                    UIAtavismTooltip.Instance.AddAttribute(bonusName, b.value.ToString(), false);
                if (b.percentage != 0)
                    UIAtavismTooltip.Instance.AddAttribute(bonusName, b.percentage + " %", false);
            }
          //  UIAtavismTooltip.Instance.AddAttribute(AtavismVip.Instance.GetPoints+" / "+ AtavismVip.Instance.GetMaxPoints , "", true);
            if (AtavismVip.Instance.GetTime != 0){

                float timeLeft = AtavismVip.Instance.GetTimeElapsed - Time.time;
                int days = 0;
                int hours = 0;
                int minutes = 0;
                if (timeLeft > 86400)
                {
                    days = (int)timeLeft / 86400;
                    timeLeft -= days * 86400;
                }
                if (timeLeft > 3600)
                {
                    hours = (int)timeLeft / 3600;
                    timeLeft -= hours * 3600;
                }
                if (timeLeft > 60)
                {
                    minutes = (int)timeLeft / 60;
                    timeLeft = minutes * 60;
                }
                if (days > 0)
                {
#if AT_I2LOC_PRESET
                       UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Expires in :")+" " + days + " "+I2.Loc.LocalizationManager.GetTranslation("days"), "", true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Expires in : " + days + " days", "", true);
#endif
                }
                else if (hours > 0)
                {
#if AT_I2LOC_PRESET
                       UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Expires in :")+" " + hours + " "+I2.Loc.LocalizationManager.GetTranslation("hours"), "", true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Expires in : " + hours + " hour", "", true);
#endif
                }
                else if (minutes > 0 && timeLeft > 0)
                {
#if AT_I2LOC_PRESET
                       UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Expires in :")+" " + minutes + " "+I2.Loc.LocalizationManager.GetTranslation("minutes"), "", true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Expires in : " + minutes + " minutes", "", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                       UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Expired"), "", true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Expired", "", true,Color.red);
#endif

                }
            }
#if AT_I2LOC_PRESET
               UIAtavismTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation(AtavismVip.Instance.GetDescription));
#else
            UIAtavismTooltip.Instance.SetDescription( AtavismVip.Instance.GetDescription );
#endif

            UIAtavismTooltip.Instance.Show(m_Root);
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