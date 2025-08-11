using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Atavism
{
    public delegate void ConfirmationResponse(object confirmObject, bool accepted);
    public delegate void ConfirmationResponseMulti(object[] confirmObject, bool accepted);
    
    public class UGUIConfirmationPanel : MonoBehaviour
    {

        static UGUIConfirmationPanel instance;

        public UGUIPanelTitleBar titleBar;
        public Text confirmationText;
        public TextMeshProUGUI TMPConfirmationText;
        public Button yesButton;
        public Button cancelButton;
        [SerializeField] GameObject messagepanel;
        [SerializeField] Text countdownText;
        [SerializeField] TextMeshProUGUI TMPCountdownText;
        float countdown = 0f;
        // float count = 0;
        object confirmationObject;
        object[] confirmationObjects;
        ConfirmationResponse confirmationResponse;
        ConfirmationResponseMulti confirmationResponseMulti;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;

            GetComponent<CanvasGroup>().alpha = 0f;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            if (titleBar != null)
                titleBar.SetOnPanelClose(CancelClicked);
        }

        void Show()
        {
            GetComponent<CanvasGroup>().alpha = 1f;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<CanvasGroup>().interactable = true;
            AtavismUIUtility.BringToFront(this.gameObject);
            if (messagepanel != null)
                messagepanel.transform.position = new Vector3((Screen.width / 2), (Screen.height / 2), 0);
            else
                transform.position = new Vector3((Screen.width / 2), (Screen.height / 2), 0);
        }

        public void Hide()
        {
            GetComponent<CanvasGroup>().alpha = 0f;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            GetComponent<CanvasGroup>().interactable = false;
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
            if (TMPCountdownText != null)
                if (Time.time < countdown)
                {
                    float sec = countdown - Time.time;
                    TMPCountdownText.text = "(" + (int)sec + ")";
                }
                else if (Time.time > countdown && countdown > 0f)
                {
                    TMPCountdownText.text = "";
                    countdown = 0f;
                    CancelClicked();
                }
                else
                {
                    TMPCountdownText.text = "";
                }
        }
        public void ShowConfirmationBox(string message, object confirmObject, ConfirmationResponse responseMethod)
        {
            Show();
            if (confirmationText != null)
                confirmationText.text = message;
            if (TMPConfirmationText != null)
                TMPConfirmationText.text = message;
            this.confirmationObject = confirmObject;
            this.confirmationResponseMulti = null;
            this.confirmationResponse = responseMethod;
            this.confirmationObjects = null;
        }
        public void ShowConfirmationBox(string message, ConfirmationResponseMulti responseMethod, params object[] confirmObjects )
        {
            Show();
            if (confirmationText != null)
                confirmationText.text = message;
            if (TMPConfirmationText != null)
                TMPConfirmationText.text = message;
            this.confirmationObject = null;
            this.confirmationResponse = null;
            this.confirmationResponseMulti = responseMethod;
            this.confirmationObjects = confirmObjects;
        }
        public void ShowConfirmationBox(string message, object confirmObject, ConfirmationResponse responseMethod, float c = 0f )
        {
            this.countdown = Time.time + c;
            Show();
            if (confirmationText != null)
                confirmationText.text = message;
            if (TMPConfirmationText != null)
                TMPConfirmationText.text = message;
            this.confirmationObject = confirmObject;
            this.confirmationResponse = responseMethod;
            this.confirmationObjects = null;
        }

        public void ShowConfirmationBox(string message, ConfirmationResponseMulti responseMethod, float c = 0f, params object[] confirmObjects )
        {
            this.countdown = Time.time + c;
            Show();
            if (confirmationText != null)
                confirmationText.text = message;
            if (TMPConfirmationText != null)
                TMPConfirmationText.text = message;
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

        public static UGUIConfirmationPanel Instance
        {
            get
            {
                return instance;
            }
        }
    }
}