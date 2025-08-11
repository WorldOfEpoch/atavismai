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
    public class UIAtavismEffectSlot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIAtavismEffectSlot, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlBoolAttributeDescription showTooltipAttribute = new UxmlBoolAttributeDescription { defaultValue = true, name = "show-Tooltip" };
            private UxmlFloatAttributeDescription tooltipDelayAttribute = new UxmlFloatAttributeDescription { defaultValue = 0, name = "tooltip-Delay" };
            private UxmlEnumAttributeDescription<CooldownDirectionEnum> cooldownDirectionAttribute = new UxmlEnumAttributeDescription<CooldownDirectionEnum> { defaultValue = CooldownDirectionEnum.up, name = "cooldown-Direction" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIAtavismEffectSlot script = (UIAtavismEffectSlot)ve;
                script.showTooltip = showTooltipAttribute.GetValueFromBag(bag, cc);
                script.tooltipDelay = tooltipDelayAttribute.GetValueFromBag(bag, cc);
                script.cooldownDirection = cooldownDirectionAttribute.GetValueFromBag(bag, cc);
            }
        }
        private enum CooldownDirectionEnum { left, right, up, down }

        private bool showTooltip { get; set; }
        private float tooltipDelay { get; set; }
        private CooldownDirectionEnum cooldownDirection { get; set; }

        private Label uiStackLabel, uiCountdownLabel;
        private VisualElement uiIcon;
        private UIFillerElement uiCooldown;
        private AtavismEffect effect;
        private int effectPos;

        private MonoBehaviour controller;
        private Coroutine countownRoutine;

        public UIAtavismEffectSlot() : base()
        {
        }

        public void Initialize(MonoBehaviour controller)
        {
            this.controller = controller;
        }

        private void findReferences()
        {
            uiIcon = this.Query<VisualElement>("Icon");
            uiCooldown = this.Query<UIFillerElement>("Cooldown");
            uiStackLabel = this.Query<Label>("Stack-label");
            uiCountdownLabel = this.Query<Label>("Countdown-label");

            if (showTooltip)
            {
                uiIcon.RegisterCallback<MouseEnterEvent>(onMouseEnter);
                uiIcon.RegisterCallback<MouseLeaveEvent>(onMouseLeave);
            }

            uiIcon.RegisterCallback<MouseDownEvent>(onMouseDown);
        }

        private void onMouseDown(MouseDownEvent evt)
        {
            RemoveEffect();
        }

        private void onMouseEnter(MouseEnterEvent evt)
        {
            if (tooltipDelay > 0f)
                controller.Invoke("ShowTooltip", tooltipDelay);
            else ShowTooltip();
        }

        private void onMouseLeave(MouseLeaveEvent evt)
        {
            if (tooltipDelay > 0f)
                controller.Invoke("HideTooltip", tooltipDelay);
            else HideTooltip();
        }

        public void Show()
        {
            UIToolkit.ShowVisualElement(this);
        }

        public void Hide()
        {
            UIToolkit.HideVisualElement(this);

            if (controller != null)
            {
                if (countownRoutine != null)
                    controller.StopCoroutine(countownRoutine);
                countownRoutine = null;
            }
        }

        public void SetEffect(AtavismEffect effect, int pos, bool textCount)
        {
            if (uiIcon == null)
                findReferences();

            float timeLeft = effect.Expiration - Time.time;
            
            this.effect = effect;
            this.effectPos = pos;

            uiStackLabel.text = effect.StackSize.ToString();
            uiCountdownLabel.text = timeLeft.ToString("N0");

            if (effect.StackSize > 1)
                UIToolkit.ShowVisualElement(uiStackLabel);
            else UIToolkit.HideVisualElement(uiStackLabel);

            UIToolkit.SetBackgroundImage(uiIcon, effect.Icon);

            if (controller != null)
            {
                if (countownRoutine != null)
                    controller.StopCoroutine(countownRoutine);
                countownRoutine = controller.StartCoroutine(runCountdownAsync());
            }
        }

        public void SetEffect(AtavismEffect effect, int pos)
        {
            SetEffect(effect, pos, true);
        }

        public void RemoveEffect()
        {
            Abilities.Instance.RemoveBuff(effect, effectPos);
        }

        public void ShowTooltip()
        {
#if AT_I2LOC_PRESET
        UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Effects/" + effect.name));
        UIAtavismTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Effects/" + effect.tooltip));
#else
            UIAtavismTooltip.Instance.SetTitle(effect.name);
            UIAtavismTooltip.Instance.SetDescription(effect.tooltip);
#endif
            //   UIAtavismTooltip.Instance.SetQuality(1);
            UIAtavismTooltip.Instance.SetQualityColor(AtavismSettings.Instance.effectQualityColor);

            UIAtavismTooltip.Instance.SetType("");
            UIAtavismTooltip.Instance.SetWeight("");
            UIAtavismTooltip.Instance.SetIcon(effect.Icon);
            UIAtavismTooltip.Instance.Show(this);
        }

        public void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator runCountdownAsync()
        {
            WaitForSeconds wait = new WaitForSeconds(0.04f);

            while (effect != null && effect.Expiration > Time.time)
            {
                float timeLeft = effect.Expiration - Time.time;
                float cooldownPercentage = timeLeft / effect.Length;

                string text = "";
                
                if (timeLeft > 86400)
                {
                    int days = (int)timeLeft / 86400;
                    text = "" + (int)days + "d";
                }
                else if (timeLeft > 3600)
                {
                    int hours = (int)timeLeft / 3600;
                    text = "" + (int)hours + "h";
                }
                else if (timeLeft > 60)
                {
                    int minutes = (int)timeLeft / 60;
                    text = "" + (int)minutes + "m";
                }
                else if (timeLeft > 0)
                {
                    text = "" + (int)timeLeft + "s";
                }

                uiCountdownLabel.text = text;
                setCooldown(cooldownPercentage);

                yield return wait;
            }

            uiCountdownLabel.text = "";
            setCooldown(0);
            countownRoutine = null;
        }

        private void setCooldown(float percentage)
        {
            uiCooldown.value = percentage;
            // switch(cooldownDirection)
            // {
            //     case CooldownDirectionEnum.down: uiCooldown.style.top = -(percentage * this.layout.height); break;
            //     case CooldownDirectionEnum.up: uiCooldown.style.top = (percentage * this.layout.height); break;
            //     case CooldownDirectionEnum.left: uiCooldown.style.left = (percentage * this.layout.width); break;
            //     case CooldownDirectionEnum.right: uiCooldown.style.left = -(percentage * this.layout.width); break;
            // }
        }
    }
}