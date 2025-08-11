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
    public class UIAtavismGroupLootSettings : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIAtavismGroupLootSettings, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlBoolAttributeDescription showTooltipAttribute = new UxmlBoolAttributeDescription { defaultValue = true, name = "show-Tooltip" };
            private UxmlFloatAttributeDescription tooltipDelayAttribute = new UxmlFloatAttributeDescription { defaultValue = 0, name = "tooltip-Delay" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                UIAtavismGroupLootSettings script = (UIAtavismGroupLootSettings)ve;
                //script.showTooltip = showTooltipAttribute.GetValueFromBag(bag, cc);
            }
        }

        private bool isVisible, isUI;
        private RadioButtonGroup uiDistributton, uiGrade;
        private UIDropdown uiRarity;
        private Button uiOK, uiCancel;
        public int RarityIndex => uiRarity.Index;
        public bool IsVisible => isVisible;

        public VisualElement Screen;
        public UIAtavismGroupLootSettings() : base()
        {
        }

        private void registerUI()
        {
            if (isUI)
                return;

            uiDistributton = this.Q<RadioButtonGroup>("Distribution");
            uiGrade = this.Q<RadioButtonGroup>("Grade");
            uiRarity = this.Q<UIDropdown>("Rarity");
            uiRarity.Screen = Screen;//this.parent.panel.visualTree.Q<VisualElement>("Screen");
            
            uiOK = this.Q<Button>("OK");
            uiCancel = this.Q<Button>("Cancel");

            if (AtavismGroup.Instance != null)
            {
                if (AtavismGroup.Instance.LeaderOid.Equals(OID.fromLong(ClientAPI.GetPlayerOid())))
                {
                    uiOK.clicked -= onClickConfirm;
                    uiOK.clicked += onClickConfirm;
                    uiOK.SetEnabled(true);
                    uiDistributton.SetEnabled(true);
                    uiGrade.SetEnabled(true);
                    uiRarity.SetEnabled(true);

                }
                else
                {
                    uiOK.SetEnabled(false);
                    uiDistributton.SetEnabled(false);
                    uiGrade.SetEnabled(false);
                    uiRarity.SetEnabled(false);
                }
            }

            uiCancel.clicked += onClickCancel;

            isUI = true;
        }

        private void onClickConfirm()
        {
            bool distributtonFreeForAll = uiDistributton.value == 0 ? true : false;
            bool distributtonRandom = uiDistributton.value == 1 ? true : false;
            bool distributtonLeader = uiDistributton.value == 2 ? true : false;
            bool gradeNormal = uiGrade.value == 0 ? true : false;
            bool gradeDice = uiGrade.value == 1 ? true : false; ;
            int rarityIndex = uiRarity.Index;

            AtavismGroup.Instance.SetLootGroup(distributtonFreeForAll, distributtonRandom, distributtonLeader,
                gradeNormal, gradeDice, rarityIndex);

            Hide();
        }

        private void onClickCancel()
        {
            Hide();
        }

        public void Show()
        {
            isVisible = true;

            registerUI();
            UpdateData();
         
            this.ShowVisualElement();
        }

        public void Hide()
        {
            isVisible = false;
            if (uiRarity != null) uiRarity.HidePopup();
            this.HideVisualElement();
        }

        public void UpdateData()
        {

            if (AtavismGroup.Instance != null)
            {
                if (uiDistributton != null) uiDistributton.value = AtavismGroup.Instance.GetRoll;
                if (uiGrade != null) uiGrade.value = AtavismGroup.Instance.GetDice;
                if (uiRarity != null)
                {
                    uiRarity.Index = AtavismGroup.Instance.GetGrade;
                    uiRarity.Screen = Screen;
                }

                // Debug.LogError("Roll=" + AtavismGroup.Instance.GetRoll + " Grade= " + AtavismGroup.Instance.GetDice +
                               // " Rarity=" + AtavismGroup.Instance.GetGrade);
                
                if (AtavismGroup.Instance.LeaderOid!=null && AtavismGroup.Instance.LeaderOid.Equals(OID.fromLong(ClientAPI.GetPlayerOid())))
                {
                    if (uiOK != null)
                    {
                        uiOK.clicked -= onClickConfirm;
                        uiOK.clicked += onClickConfirm;
                        uiOK.SetEnabled(true);
                    }

                    if (uiDistributton != null)  uiDistributton.SetEnabled(true);
                    if (uiGrade != null) uiGrade.SetEnabled(true);
                    if (uiRarity != null)  uiRarity.SetEnabled(true);

                }
                else
                {
                    if (uiOK != null) uiOK.SetEnabled(false);
                    if (uiDistributton != null)  uiDistributton.SetEnabled(false);
                    if (uiGrade != null)  uiGrade.SetEnabled(false);
                    if (uiRarity != null)   uiRarity.SetEnabled(false);
                }

            }
        }
    }
}