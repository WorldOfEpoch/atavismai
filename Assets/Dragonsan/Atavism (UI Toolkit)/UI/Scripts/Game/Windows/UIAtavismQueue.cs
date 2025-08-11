using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
   public class UIAtavismQueue : MonoBehaviour
    {
        public VisualElement m_queuePanel;
        public Label m_countText;
        public Label m_messageText;
        [SerializeField] private UIDocument uiDocument;
        private Button serverListButton;
        private VisualElement anim;
        private void Reset()
        {
            uiDocument = gameObject.GetComponent<UIDocument>();
        }

        int animId = 1;
        private void OnEnable()
        {
            uiDocument = gameObject.GetComponent<UIDocument>();
            uiDocument.enabled = true;
            m_queuePanel = uiDocument.rootVisualElement.Q<VisualElement>("queue-panel");
            m_countText = m_queuePanel.Q<Label>("count");
            m_messageText = m_queuePanel.Q<Label>("message");
            serverListButton = m_queuePanel.Q<Button>("server-list-button");
            anim = m_queuePanel.Q<VisualElement>("anim");
            anim.RegisterCallback<TransitionEndEvent>(animTransition);
            serverListButton.clicked += ShowServerList;
            AtavismEventSystem.RegisterEvent("LOGIN_QUEUE", this);
             m_queuePanel.HideVisualElement();
            // anim.AddToClassList("slot-anim-"+animId);
            
        
            
        }

        private void Update()
        {
            // if (Input.GetMouseButtonUp(0))
            // {
            //     animTransition(null);
            // }
        }

        private void animTransition(TransitionEndEvent evt)
        {
            // Debug.LogError("animTransition "+animId+" was called");
            if (anim.ClassListContains("slot-anim-"+animId))
                anim.RemoveFromClassList("slot-anim-"+animId);
            animId++;
            if (animId > 21)
                animId = 1;
            anim.AddToClassList("slot-anim-"+animId);

        }

        private void ShowServerList()
        {
            AtavismEventSystem.DispatchEvent("SHOW_SERVER_LIST", new string[]{});
        }

        // Start is called before the first frame update

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("LOGIN_QUEUE", this);
            
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "LOGIN_QUEUE")
            {
                if (m_queuePanel!=null)
                {
                    m_queuePanel.ShowVisualElement();
                    animTransition(null);
                    String count = eData.eventArgs[0];
                    String message = eData.eventArgs[1];

                    if (m_countText !=null)
                    {
                        if (count.Length > 0)
                        {
#if AT_I2LOC_PRESET
                            m_countText.text = I2.Loc.LocalizationManager.GetTranslation("You are")+" "+count;
#else
                            m_countText.text = "You are " + count;
#endif
                        }
                        else
                        {
                            m_countText.text = "";
                        }
                    }

                    if (m_messageText!=null)
                    {
                        m_messageText.text = message;
                    }
                }
            }
        }
    }
}