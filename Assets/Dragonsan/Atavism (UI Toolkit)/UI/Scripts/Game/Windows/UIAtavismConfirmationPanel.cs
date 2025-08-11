using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    public delegate void UIConfirmationResponse(object confirmObject, bool accepted);
    public delegate void UIConfirmationResponseMulti(object[] confirmObject, bool accepted);
    
    public class UIAtavismConfirmationPanel : MonoBehaviour
    {

        static UIAtavismConfirmationPanel instance;
        [AtavismSeparator("Window Base")] [SerializeField]
        protected UIDocument uiDocument;
        
        private Label confirmationMessage;
        private Button yesButton;
        private Button cancelButton;
        private Label countdownText;
        float countdown = 0f;
        // float count = 0;
        object confirmationObject;
        object[] confirmationObjects;
        UIConfirmationResponse confirmationResponse;
        UIConfirmationResponseMulti confirmationResponseMulti;


        private void OnEnable()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            
            uiDocument.enabled = true;
            confirmationMessage = uiDocument.rootVisualElement.Q<Label>("message");
            yesButton = uiDocument.rootVisualElement.Q<Button>("button-yes");
            cancelButton = uiDocument.rootVisualElement.Q<Button>("button-cancel");
            countdownText = uiDocument.rootVisualElement.Q<Label>("timer");
            yesButton.clicked += YesClicked;
            cancelButton.clicked += CancelClicked;
            Hide();
        }

        void Show()
        {
            AtavismSettings.Instance.OpenWindow(this);   
            uiDocument.rootVisualElement.ShowVisualElement();
            uiDocument.sortingOrder = 510;
            AtavismUIUtility.BringToFront(this.gameObject);
        }

        public void Hide()
        {
            AtavismSettings.Instance.CloseWindow(this);   
            uiDocument.rootVisualElement.HideVisualElement();
        }
        void Update()
        {
            if (countdownText != null)
                if (Time.time < countdown)
                {
                    float sec = countdown - Time.time;
                    countdownText.text = "(" + (int)sec + ")";
                }
                else if (Time.time > countdown && countdown > 0f)
                {
                    countdownText.text = "";
                    countdown = 0f;
                    CancelClicked();
                }
                else
                {
                    countdownText.text = "";
                }
        }
        public void ShowConfirmationBox(string message, object confirmObject, UIConfirmationResponse responseMethod)
        {
            Show();
            if (confirmationMessage != null)
                confirmationMessage.text = message;
            this.confirmationObject = confirmObject;
            this.confirmationResponseMulti = null;
            this.confirmationResponse = responseMethod;
            this.confirmationObjects = null;
        }
        public void ShowConfirmationBox(string message, UIConfirmationResponseMulti responseMethod, params object[] confirmObjects )
        {
            Show();
            if (confirmationMessage != null)
                confirmationMessage.text = message;
            this.confirmationObject = null;
            this.confirmationResponse = null;
            this.confirmationResponseMulti = responseMethod;
            this.confirmationObjects = confirmObjects;
        }
        public void ShowConfirmationBox(string message, object confirmObject, UIConfirmationResponse responseMethod, float c = 0f )
        {
            this.countdown = Time.time + c;
            Show();
            if (confirmationMessage != null)
                confirmationMessage.text = message;
            this.confirmationObject = confirmObject;
            this.confirmationResponse = responseMethod;
            this.confirmationObjects = null;
        }

        public void ShowConfirmationBox(string message, UIConfirmationResponseMulti responseMethod, float c = 0f, params object[] confirmObjects )
        {
            this.countdown = Time.time + c;
            Show();
            if (confirmationMessage != null)
                confirmationMessage.text = message;
            this.confirmationObject = null;
            this.confirmationResponseMulti = responseMethod;
            this.confirmationObjects = confirmObjects;
        }
        
        public void YesClicked()
        {
            this.countdown = 0f;
            if (confirmationResponseMulti != null)
            {
                confirmationResponseMulti(confirmationObjects, true);    
            }
            else
            {
                confirmationResponse(confirmationObject, true);
            }

            Hide();
        }

        public void CancelClicked()
        {
            this.countdown = 0f;
            if (confirmationResponseMulti != null)
            {
                confirmationResponseMulti(confirmationObjects, false);    
            }
            else
            {
                confirmationResponse(confirmationObject, false);
            }
            Hide();
        }

        public static UIAtavismConfirmationPanel Instance
        {
            get
            {
                return instance;
            }
        }
    }
}