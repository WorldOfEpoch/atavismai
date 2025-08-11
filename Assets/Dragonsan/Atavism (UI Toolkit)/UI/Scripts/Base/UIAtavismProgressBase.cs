using Atavism;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIAtavismProgressBase : MonoBehaviour
{
    public static UIAtavismProgressBase instance;
    /*
        public static UIAtavismProgressBase Instance => instance;

    private void Awake()
    {
        instance = this;
    }
    */

    [AtavismSeparator("Runtime")]
    [SerializeField] private bool isVisible;
    public bool IsVisible => isVisible;

    [AtavismSeparator("UI")]
    [SerializeField] private UIDocument uiDialogProgressDocument;
    [Space(10)]
    [SerializeField] private string uiProgressPanelName = "panel_progress";
    [SerializeField] private string uiMessageLabelName = "messageLabel";

    [SerializeField] private string uiProgressBarName = "progress";
    [SerializeField] private string uiProgressBarLabelName = "progress-name";

    [SerializeField] private string uiOverlayIconName = "icon-overlay";
    [SerializeField] private string uiInternalIconName = "internal-icon";

    private VisualElement uiProgressScreen, uiProgressPanel, uiOverlayIcon, uiInternalIcon;
    private Label uiMessageLabel, uiDurationLabel, uiProgressBarLabel;

    private int countdown;


    private void OnEnable()
    {
        registerUI();
    }

    private void Start()
    {
    }
    private void Update()
    {
    }

    private void registerUI()
    {
        uiProgressScreen = uiDialogProgressDocument.rootVisualElement.Query<VisualElement>("Screen");
        uiProgressPanel = uiDialogProgressDocument.rootVisualElement.Query<VisualElement>(uiProgressPanelName);
        uiOverlayIcon = uiDialogProgressDocument.rootVisualElement.Query<VisualElement>(uiOverlayIconName);
        uiInternalIcon = uiDialogProgressDocument.rootVisualElement.Query<VisualElement>(uiInternalIconName);

        uiMessageLabel = uiDialogProgressDocument.rootVisualElement.Query<Label>(uiMessageLabelName);
        uiProgressBarLabel = uiDialogProgressDocument.rootVisualElement.Query<Label>(uiProgressBarLabelName);

    }


    public void ShowMessage(string message)
    {
        ShowProgressProgress(message);
    }


    public void ApplyTexture(VisualElement thisElement, Texture2D thisTexture)
    {
        thisElement.style.backgroundImage = thisTexture;
    }

    public bool ShowProgressProgress(string message)
    {
        if (isVisible)
        {
            HideProgressProgress();
        }

        uiMessageLabel.text = message;

        uiProgressScreen.style.display = DisplayStyle.Flex;

        isVisible = true;

        return true;
    }

    public bool HideProgressProgress()
    {
        StopCoroutine("timeoutCounterAsync");

        if (!isVisible)
        {
            Debug.LogError("Cannot hide Progress dialog because it is already hidden!");
            return false;
        }

        uiProgressScreen.style.display = DisplayStyle.None;

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

        while (countdown > 0)
        {
            countdown--;
            UpdateCountdown(uiProgressBarLabelName);
            yield return wait;
        }

        HideProgressProgress();
    }

    public virtual void UpdateCountdown(string defaultLabel)
    {
        if (countdown > 0)
            uiDurationLabel.text = "(" + countdown.ToString() + ") " + defaultLabel;
    }
}
