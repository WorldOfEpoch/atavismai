using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismActionCombo : MonoBehaviour
    {

        [SerializeField] private UIDocument uiDocument;

        private VisualElement slot;

        public UIAtavismActionBarSlot comboSlot;

        // private bool presed = false;
        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;

            slot = uiDocument.rootVisualElement.Q<VisualElement>("slot");
            comboSlot = new UIAtavismActionBarSlot();
            // Assign the controller script to the visual element
            slot.userData = comboSlot;
            // Initialize the controller script
            comboSlot.SetVisualElement(slot);

            registerEvents();
        }

        // Use this for initialization
        void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("ACTION_UPDATE", this);
            AtavismEventSystem.RegisterEvent("COOLDOWN_UPDATE", this);
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("ABILITY_UPDATE", this);
            UpdateActions();
        }

        ~UIAtavismActionCombo()
        {
            AtavismEventSystem.UnregisterEvent("ACTION_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("COOLDOWN_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("ABILITY_UPDATE", this);
        }


        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ACTION_UPDATE")
            {
                UpdateActions();
            }
            else if (eData.eventType == "ABILITY_UPDATE")
            {
                UpdateActions();
            }
            else if (eData.eventType == "COOLDOWN_UPDATE")
            {
                UpdateActions();
            }
            else if (eData.eventType == "INVENTORY_UPDATE")
            {
                UpdateActions();
            }
        }

        public void UpdateActions()
        {
           
            if (comboSlot != null)
            {
                if (Actions.Instance.ComboAction != null && Actions.Instance.ComboShowCenter)
                {
                    slot.ShowVisualElement();
                    comboSlot.UpdateActionData(Actions.Instance.ComboAction, -1);
                }
                else
                {
                    slot.HideVisualElement();
                }
            }
        }

    }
}