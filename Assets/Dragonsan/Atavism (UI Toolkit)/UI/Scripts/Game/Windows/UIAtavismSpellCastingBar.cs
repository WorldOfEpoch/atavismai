using System;
using System.Collections;
using System.Collections.Generic;
using Atavism.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismSpellCastingBar : MonoBehaviour
    {

        new static UIAtavismSpellCastingBar instance;
        
        [SerializeField] public UIDocument uiDocument;
        private VisualElement m_icon;
        private VisualElement m_iconGameObject;
        private Label m_castTimeLabel;
        private Label m_SpellName;
        private UIProgressBar m_bar;
        private VisualElement m_Root;

        float startTime;
        float endTime = -1;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if(uiDocument==null)
                uiDocument = GetComponent<UIDocument>();
            registerUI();
        }

        protected bool registerUI()
        {
            uiDocument.enabled = true;
            m_Root = uiDocument.rootVisualElement;
            m_bar = uiDocument.rootVisualElement.Query<UIProgressBar>("castbar-progress");
            m_icon = uiDocument.rootVisualElement.Query<VisualElement>("icon");
            m_SpellName = uiDocument.rootVisualElement.Query<Label>("spell-name");
            m_castTimeLabel = uiDocument.rootVisualElement.Query<Label>("duration-label");
            m_iconGameObject = uiDocument.rootVisualElement.Query<VisualElement>("icon-frame");
            return true;
        }


        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;

            Hide();
            AtavismEventSystem.RegisterEvent("CASTING_STARTED", this);
            AtavismEventSystem.RegisterEvent("CASTING_CANCELLED", this);
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CASTING_STARTED", this);
            AtavismEventSystem.UnregisterEvent("CASTING_CANCELLED", this);
        }

        // Update is called once per frame
        void Update()
        {
            if (endTime != -1 && endTime > Time.time)
            {
                float total = endTime - startTime;
                float currentTime = endTime - Time.time;
                if (m_bar != null)
                {
                    m_bar.highValue = total;
                    m_bar.value = currentTime ;
                }

                if (m_castTimeLabel != null)
                    m_castTimeLabel.text = string.Format("{0:0.0}", currentTime) + "s";
            }
            else
            {
                Hide();
            }
        }

        void Show()
        {
            m_Root.ShowVisualElement();
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CASTING_STARTED")
            {
                if (eData.eventArgs.Length > 1 && OID.fromString(eData.eventArgs[1]).ToLong() != ClientAPI.GetPlayerOid())
                    return;
                Show();
                startTime = Time.time;
                endTime = Time.time + float.Parse(eData.eventArgs[0]);
                int abiltyId = 0;
                if (eData.eventArgs.Length > 2)
                    abiltyId = int.Parse(eData.eventArgs[2]);
                if (m_iconGameObject != null)
                {
                    if (abiltyId > 0)
                    {
                        m_iconGameObject.ShowVisualElement();
                    }
                    else
                    {
                        m_iconGameObject.HideVisualElement();
                    }
                }
                if (m_icon != null)
                {
                    if (abiltyId > 0)
                    {
                        AtavismAbility aa = Abilities.Instance.GetAbility(abiltyId);
                        m_icon.style.backgroundImage = new StyleBackground(aa.Icon);
                        m_icon.ShowVisualElement();

                        if (m_SpellName != null)
                        {
                            m_SpellName.text = aa.name;
                        }
                    }
                    else
                    {
                        m_icon.HideVisualElement();
                    }
                }

            }
            else if (eData.eventType == "CASTING_CANCELLED")
            {
                if (eData.eventArgs.Length > 1 && OID.fromString(eData.eventArgs[1]).ToLong() != ClientAPI.GetPlayerOid())
                    return;
                Hide();
                endTime = -1;
            }
        }

        public static UIAtavismSpellCastingBar Instance
        {
            get
            {
                return instance;
            }
        }
    }
}