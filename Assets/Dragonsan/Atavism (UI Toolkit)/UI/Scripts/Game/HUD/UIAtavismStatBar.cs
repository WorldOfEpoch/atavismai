using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismStatBar : MonoBehaviour
    {
        UIDocument uiDocument;
        public string prop;
        public string propMax;
        public string description;
        public bool forTarget = false;
        protected Label _label;
        protected UIProgressBar _progressBar;
        // [Range(0,1)]
        // public float delaySpeed = 0.01f;
        private int _value = 1;
        private int _valueMax = 1;
        private bool _mouseEntered;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        // Use this for initialization
        void OnEnable()
        {
            if(uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            RegisterUI();
            if (forTarget)
            {
                AtavismEventSystem.RegisterEvent("PROPERTY_" + prop, this);
                AtavismEventSystem.RegisterEvent("PROPERTY_" + propMax, this);
                AtavismEventSystem.RegisterEvent("PLAYER_TARGET_CHANGED", this);
                AtavismEventSystem.RegisterEvent("OBJECT_TARGET_CHANGED", this);

                if (ClientAPI.GetTargetObject() != null)
                {
                    _value = (int)ClientAPI.GetTargetObject().GetProperty(prop);
                    _valueMax = (int)ClientAPI.GetTargetObject().GetProperty(propMax);
                    UpdateProgressBar();
                }
            }
            else
            {
                if (ClientAPI.GetPlayerObject() != null)
                {
                    ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(prop, PropHandler);
                    ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler(propMax, PropMaxHandler);

                    if (ClientAPI.GetPlayerObject().PropertyExists(prop))
                    {
                        _value = (int)ClientAPI.GetPlayerObject().GetProperty(prop);
                    }
                    if (ClientAPI.GetPlayerObject().PropertyExists(propMax))
                    {
                        _valueMax = (int)ClientAPI.GetPlayerObject().GetProperty(propMax);
                    }
                    UpdateProgressBar();
                }
            }
        }

        void RegisterUI()
        {
            uiDocument.enabled = true;
            _label = uiDocument.rootVisualElement.Q<Label>("label");
            _progressBar = uiDocument.rootVisualElement.Q<UIProgressBar>("progressBar");
            _progressBar.RegisterCallback<PointerEnterEvent>((e) =>
            {
                UIAtavismMiniTooltip.Instance.SetDescription(description);
                UIAtavismMiniTooltip.Instance.Show(_progressBar);

            });
            _progressBar.RegisterCallback<PointerLeaveEvent>((e) => { UIAtavismMiniTooltip.Instance.Hide(); });

        }
        
        
        void OnDestroy()
        {
            if (forTarget)
            {
                AtavismEventSystem.UnregisterEvent("PROPERTY_" + prop, this);
                AtavismEventSystem.UnregisterEvent("PROPERTY_" + propMax, this);
                AtavismEventSystem.UnregisterEvent("PLAYER_TARGET_CHANGED", this);
                AtavismEventSystem.UnregisterEvent("OBJECT_TARGET_CHANGED", this);
            }
            else
            {
                if (ClientAPI.GetPlayerObject() != null)
                {
                    ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(prop, PropHandler);
                    ClientAPI.GetPlayerObject().RemovePropertyChangeHandler(propMax, PropMaxHandler);
                }
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "PROPERTY_" + prop)
            {
                if (eData.eventArgs[0] == "target")
                {
                    if(ClientAPI.GetTargetObject()!=null)
                    _value = (int)ClientAPI.GetTargetObject().GetProperty(prop);
                }
                //Debug.Log("Got health property: " + eData.eventArgs.Length + " with unit: " + eData.eventArgs[0]);
            }
            else if (eData.eventType == "PROPERTY_" + propMax)
            {
                if (eData.eventArgs[0] == "target")
                {
                    if(ClientAPI.GetTargetObject()!=null)
                    _valueMax = (int)ClientAPI.GetTargetObject().GetProperty(propMax);
                }
            }
            else if (eData.eventType == "PLAYER_TARGET_CHANGED")
            {
                if (ClientAPI.GetTargetObject() != null)
                {
                    _value = (int)ClientAPI.GetTargetObject().GetProperty(prop);
                    _valueMax = (int)ClientAPI.GetTargetObject().GetProperty(propMax);
                    UpdateProgressBar();
                }
                else
                {
                    _value = 100;
                    _valueMax = 100;
                    UpdateProgressBar();
                }
            }else if (eData.eventType == "OBJECT_TARGET_CHANGED")
            {
              //  int id = int.Parse(eData.eventArgs[0]);                
               // int claimId = int.Parse(eData.eventArgs[1]);           
             //   Debug.Log("UGUI StatBar val=" + value + " max=" + valueMax);
                if(WorldBuilder.Instance.SelectedClaimObject!=null){
                    _value = WorldBuilder.Instance.SelectedClaimObject.Health;
                    _valueMax = WorldBuilder.Instance.SelectedClaimObject.MaxHealth;
                   // Debug.Log("UGUI StatBar val=" + value + " max=" + valueMax);
                    UpdateProgressBar();
                }
                else
                {
                    _value = 100;
                    _valueMax = 100;
                    UpdateProgressBar();
                }
            }
            UpdateProgressBar();
        }

        public void PropHandler(object sender, PropertyChangeEventArgs args)
        {
            _value = (int)ClientAPI.GetPlayerObject().GetProperty(prop);
            UpdateProgressBar();
        }

        public void PropMaxHandler(object sender, PropertyChangeEventArgs args)
        {
            _valueMax = (int)ClientAPI.GetPlayerObject().GetProperty(propMax);
            UpdateProgressBar();
        }

        void UpdateProgressBar()
        {
            if (_progressBar != null)
            {
                _progressBar.highValue = _valueMax;
                _progressBar.value = _value;
            }
            if (_label != null)
            {
                _label.text = _value + " / " + _valueMax;
            }

        }

        private void Update()
        {
        
        }

    }

}