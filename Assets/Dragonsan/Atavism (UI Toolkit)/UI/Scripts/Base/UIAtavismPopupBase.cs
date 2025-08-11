using Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIAtavismPopupBase : MonoBehaviour
{
    public static UIAtavismPopupBase instance;
    /*
        public static UIAtavismPopupBase Instance => instance;

    private void Awake()
    {
        instance = this;
    }
    */

    [AtavismSeparator("Runtime")]
        [SerializeField] private bool isVisible;
        public bool IsVisible => isVisible;

        [AtavismSeparator("UI")]
        [SerializeField] private UIDocument uiDialogPopupDocument;
        [Space(10)]
        [SerializeField] private string uiPopupPanelName = "panel_popup";
        [SerializeField] private string uiMessageLabelName = "messageLabel";
        [SerializeField] private string uiConfirmButtonName = "confirmButton";
        [SerializeField] private string uiCancelButtonName = "cancelButton";

        private VisualElement uiPopupScreen;
        private VisualElement uiPopupPanel;
        private Label uiMessageLabel;
        private Button uiConfirmButton;
        private Button uiCancelButton;

        private Action confirmAction, cancelAction;
        private int countdown;


        private void OnEnable()
        {
            registerUI();
        }

        private void registerUI()
        {
            uiPopupScreen = uiDialogPopupDocument.rootVisualElement.Query<VisualElement>("Screen");
            uiPopupPanel = uiDialogPopupDocument.rootVisualElement.Query<VisualElement>(uiPopupPanelName);
            uiMessageLabel = uiDialogPopupDocument.rootVisualElement.Query<Label>(uiMessageLabelName);
            uiConfirmButton = uiDialogPopupDocument.rootVisualElement.Query<Button>(uiConfirmButtonName);
            uiCancelButton = uiDialogPopupDocument.rootVisualElement.Query<Button>(uiCancelButtonName);
        }

        public void ShowMessage(string message)
        {
            ShowDialogPopup(message, confirm: null);
        }

        public void ShowDialogPopup(string message)
        {
            ShowDialogPopup(message, () => HideDialogPopup(), "CLOSE");
        }

        public void ShowDialogPopup(string message, string confirmButton)
        {
            ShowDialogPopup(message, () => HideDialogPopup(), confirmButton);
        }

        public bool ShowDialogPopup(string message, Action confirm, string confirmButton = "OK", Action cancel = null, string cancelButton = "CLOSE")
        {
            if (isVisible)
            {
                HideDialogPopup();
                //return false;
            }

            uiMessageLabel.text = message;

            if (confirm == null)
                uiConfirmButton.style.display = DisplayStyle.None;
            else
            {
                uiConfirmButton.style.display = DisplayStyle.Flex;
                uiConfirmButton.text = confirmButton;
                uiConfirmButton.clicked += confirm;
            }
            confirmAction = confirm;

            if (cancel == null)
                uiCancelButton.style.display = DisplayStyle.None;
            else
            {
                uiCancelButton.style.display = DisplayStyle.Flex;
                uiCancelButton.text = cancelButton;
                uiCancelButton.clicked += cancel;
            }
            cancelAction = cancel;

            uiPopupScreen.style.display = DisplayStyle.Flex;

            isVisible = true;

            return true;
        }

        public bool HideDialogPopup()
        {
            StopCoroutine("timeoutCounterAsync");

            if (!isVisible)
            {
                Debug.LogError("Cannot hide popup dialog because it is already hidden!");
                return false;
            }

            if (confirmAction != null)
                uiConfirmButton.clicked -= confirmAction;
            if (cancelAction != null)
                uiCancelButton.clicked -= cancelAction;

            uiPopupScreen.style.display = DisplayStyle.None;

            isVisible = false;

            return true;
        }

        public void SetTimeout(int seconds)
        {
            StopCoroutine("timeoutCounterAsync");
            StartCoroutine("timeoutCounterAsync", seconds);
        }
        protected virtual IEnumerator timeoutCounterAsync(int seconds)
        {
            WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
            countdown = seconds;
            string defaultLabel = uiConfirmButton.text;

            while (countdown > 0)
            {
                countdown--;
                UpdateCountdown(defaultLabel);
                yield return wait;
            }

            HideDialogPopup();
        }

        public virtual void UpdateCountdown(string defaultLabel)
        {
            if (countdown > 0)
                uiConfirmButton.text = "(" + countdown.ToString() + ") " + defaultLabel;
        }
    }
