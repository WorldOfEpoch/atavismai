using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismFactionStancesListEntry 
    {

        [SerializeField] Label name;
        [SerializeField] Label stance;
        [SerializeField] Label stanceNext;
        [SerializeField] Label stancePrev;
        [SerializeField] UIProgressBar slider;
        private VisualElement uiRoot;
        
        public void SetVisualElement(VisualElement visualElement)
        {
            uiRoot = visualElement;
            name = visualElement.Q<Label>("name");
            stance = visualElement.Q<Label>("stance");
            stanceNext = visualElement.Q<Label>("stance-next");
            stancePrev = visualElement.Q<Label>("stance-prev");
            slider = visualElement.Q<UIProgressBar>("slider");
        }
        
        
        public void UpdateDisplay(string name, int value)
        {
            if (this.name != null)
#if AT_I2LOC_PRESET
            this.name.text = I2.Loc.LocalizationManager.GetTranslation(name);
#else
                this.name.text = name;
#endif

            if (slider != null)
            {
                slider.value = value;
                // slider.targetGraphic.color = Color.red;
                if (value < -1500)
                {
                    slider.lowValue = -3000;
                    slider.highValue = -1500;
                    if (stanceNext != null)
#if AT_I2LOC_PRESET
                    stanceNext.text = I2.Loc.LocalizationManager.GetTranslation("Disliked");
#else
                        stanceNext.text = "Disliked";
#endif
                    if (stancePrev != null)
                        stancePrev.text = "";
                    if (stance != null)
#if AT_I2LOC_PRESET
                    stance.text = I2.Loc.LocalizationManager.GetTranslation("Hated");
#else
                        stance.text = "Hated";
#endif
                }
                else if (value < 0)
                {
                    slider.lowValue = -1500;
                    slider.highValue = 0;

                    if (stanceNext != null)
#if AT_I2LOC_PRESET
                    stanceNext.text = I2.Loc.LocalizationManager.GetTranslation("Neutral");
#else
                        stanceNext.text = "Neutral";
#endif
                    if (stancePrev != null)
#if AT_I2LOC_PRESET
                    stancePrev.text = I2.Loc.LocalizationManager.GetTranslation("Hated");
#else
                        stancePrev.text = "Hated";
#endif
                    if (stance != null)
#if AT_I2LOC_PRESET
                    stance.text = I2.Loc.LocalizationManager.GetTranslation("Disliked");
#else
                        stance.text = "Disliked";
#endif
                }
                else if (value < 500)
                {
                    slider.lowValue = 0;
                    slider.highValue = 500;
                    if (stanceNext != null)
#if AT_I2LOC_PRESET
                    stanceNext.text = I2.Loc.LocalizationManager.GetTranslation("Friendly");
#else
                        stanceNext.text = "Friendly";
#endif
                    if (stancePrev != null)
#if AT_I2LOC_PRESET
                    stancePrev.text = I2.Loc.LocalizationManager.GetTranslation("Disliked");
#else
                        stancePrev.text = "Disliked";
#endif
                    if (stance != null)
#if AT_I2LOC_PRESET
                    stance.text = I2.Loc.LocalizationManager.GetTranslation("Neutral");
#else
                        stance.text = "Neutral";
#endif
                }
                else if (value < 1500)
                {
                    slider.lowValue = 500;
                    slider.highValue = 1500;
                    if (stanceNext != null)
#if AT_I2LOC_PRESET
                    stanceNext.text = I2.Loc.LocalizationManager.GetTranslation("Honoured");
#else
                        stanceNext.text = "Honoured";
#endif
                    if (stancePrev != null)
#if AT_I2LOC_PRESET
                    stancePrev.text = I2.Loc.LocalizationManager.GetTranslation("Neutral");
#else
                        stancePrev.text = "Neutral";
#endif
                    if (stance != null)
#if AT_I2LOC_PRESET
                    stance.text = I2.Loc.LocalizationManager.GetTranslation("Friendly");
#else
                        stance.text = "Friendly";
#endif
                }
                else if (value < 3000)
                {
                    slider.lowValue = 1500;
                    slider.highValue = 3000;
                    if (stanceNext != null)
#if AT_I2LOC_PRESET
                    stanceNext.text = I2.Loc.LocalizationManager.GetTranslation("Exalted");
#else
                        stanceNext.text = "Exalted";
#endif
                    if (stancePrev != null)
#if AT_I2LOC_PRESET
                    stancePrev.text = I2.Loc.LocalizationManager.GetTranslation("Friendly");
#else
                        stancePrev.text = "Friendly";
#endif
                    if (stance != null)
#if AT_I2LOC_PRESET
                    stance.text = I2.Loc.LocalizationManager.GetTranslation("Honoured");
#else
                        stance.text = "Honoured";
#endif
                }
                else
                {
                    slider.lowValue = 3000;
                    slider.highValue = 3100;
                    if (stanceNext != null)
                        stanceNext.text = "";
                    if (stancePrev != null)
#if AT_I2LOC_PRESET
                    stancePrev.text = I2.Loc.LocalizationManager.GetTranslation("Honoured");
#else
                        stancePrev.text = "Honoured";
#endif
                    if (stance != null)
#if AT_I2LOC_PRESET
                    stance.text = I2.Loc.LocalizationManager.GetTranslation("Exalted");
#else
                        stance.text = "Exalted";
#endif
                }
                /*public static final int HatedRep = -3000;
                public static final int DislikedRep = -1500;
                public static final int NeutralRep = 0;
                public static final int FriendlyRep = 500;
                public static final int HonouredRep = 1500;
                public static final int ExaltedRep = 3000;
            */
            }

        }
    }
}