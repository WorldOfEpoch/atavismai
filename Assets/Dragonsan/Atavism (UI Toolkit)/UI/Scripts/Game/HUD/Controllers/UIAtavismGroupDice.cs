using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI.Game
{
    /// <summary>
    /// In this version UI Toolkit has no mask => just rectangular Cooldown design is implemented as UI Toolkit has rectangular culling only.
    /// ToDo: Tooltip (required Tootip designed in UI Toolkit)
    /// </summary>
    public class UIAtavismGroupDice : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIAtavismGroupDice, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlBoolAttributeDescription showTooltipAttribute = new UxmlBoolAttributeDescription { defaultValue = true, name = "show-Tooltip" };
            private UxmlFloatAttributeDescription tooltipDelayAttribute = new UxmlFloatAttributeDescription { defaultValue = 0, name = "tooltip-Delay" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIAtavismGroupDice script = (UIAtavismGroupDice)ve;
                //script.showTooltip = showTooltipAttribute.GetValueFromBag(bag, cc);
            }
        }

        private UIAtavismItemDisplay uiItemDisplay;
        private Label uiRemainingTime;
        private UIProgressBar uiProgressBar;
        private Button uiRoll, uiPass;

        private MonoBehaviour monoController;
        private Coroutine remainingTimeRoutine;
        private bool isUI, userConfirm;
            private bool isVisible = false;

        private AtavismInventoryItem atavismItem;

        public string TimeRemaningPrefix = "Remaining Time";
        public bool IsVisible => isVisible;

        public UIAtavismGroupDice() : base()
        {
        }

        public void Initialize(MonoBehaviour monoController)
        {
            this.monoController = monoController;
        }

        private void registerUI()
        {
           // Debug.Log("Group Dice registerUI");
            if (isUI)
                return;

            uiItemDisplay = this.Q<UIAtavismItemDisplay>();
            uiRemainingTime = this.Q<Label>("time-label");
            uiProgressBar = this.Q<UIProgressBar>();
            uiRoll = this.Q<Button>("Roll");
            uiPass = this.Q<Button>("Pass");

            uiRoll.clicked += Roll;
            uiPass.clicked += Pass;

            isUI = true;
         //   Debug.Log("Group Dice registerUI End");
        }

        public void Show(AtavismInventoryItem atavismItem, float expiration, float length)
        {
          //  Debug.Log("Group Dice Show");
            isVisible = true;
            this.atavismItem = atavismItem;

            registerUI();

            if (atavismItem != null)
                uiItemDisplay.SetItemData(atavismItem);
            else Debug.LogError("Atavism item reference is null!");

            this.ShowVisualElement();

            if (monoController != null)
            {
                if (remainingTimeRoutine != null)
                    monoController.StopCoroutine(remainingTimeRoutine);

                remainingTimeRoutine = monoController.StartCoroutine(remainingTimeAsync(expiration, length));
            }
          //  Debug.Log("Group Dice Show End");
        }

        public void Hide()
        {
          //  Debug.Log("Group Dice Hide");
            isVisible = false;

            if (!userConfirm)
                Roll();
            else
            {
                userConfirm = false;

                this.HideVisualElement();
            }

            if (monoController != null)
                if (remainingTimeRoutine != null)
                    monoController.StopCoroutine(remainingTimeRoutine);
         //   Debug.Log("Group Dice Hide End");
        }

        public void Pass()
        {
          //  Debug.Log("Group Dice Pass");
            userConfirm = true;

            if (AtavismGroup.Instance != null)
                AtavismGroup.Instance.Pass();

            Hide();
        }

        public void Roll()
        {
            //Debug.Log("Group Dice Roll");
            userConfirm = true;

            if (AtavismGroup.Instance != null)
                AtavismGroup.Instance.Roll();

            Hide();
        }

        private IEnumerator remainingTimeAsync(float expiration, float length)
        {
            uiProgressBar.lowValue = 0;
            uiProgressBar.highValue = length;

            while (expiration > Time.time)
            {
                float timeLeft = expiration - Time.time;

                if (timeLeft > 60)
                {
                    int minutes = (int)timeLeft / 60;
#if AT_I2LOC_PRESET
                    uiRemainingTime.text =  I2.Loc.LocalizationManager.GetTranslation(TimeRemaningPrefix)+" " + (int)minutes + "m";
#else
                    uiRemainingTime.text = TimeRemaningPrefix + " " + (int)minutes + "m";
#endif
                }

                uiProgressBar.value = timeLeft ;

                yield return new WaitForSeconds(0.04f);
            }

            Hide();
        }
    }
}