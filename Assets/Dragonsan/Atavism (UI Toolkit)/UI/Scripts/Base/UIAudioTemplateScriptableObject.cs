using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism.UI
{
    [CreateAssetMenu(fileName = "UIAudioTemplate", menuName = "Atavism/AudioTemplate", order = 1)]
    public class UIAudioTemplateScriptableObject : ScriptableObject
    {
        [System.Serializable]
        public class CAudioEvent
        {
            public string StyleClass;
            public AudioClip MouseDownClip;
            public AudioClip MouseUpClip;
            public AudioClip MouseEnterClip;
        }

        [AtavismSeparator("Global SFX")]
        [SerializeField] private AudioClip sfxButtonMouseEnter;
        public AudioClip ButtonMouseEnter => sfxButtonMouseEnter;

        [SerializeField] private AudioClip sfxButtonMouseDown;
        public AudioClip ButtonMouseDown => sfxButtonMouseDown;

        [SerializeField] private AudioClip sfxButtonMouseUp;
        public AudioClip ButtonMouseUp => sfxButtonMouseUp;

        [SerializeField] private AudioClip sfxToggleChanged;
        public AudioClip ToggleChanged => sfxToggleChanged;

        [SerializeField] private AudioClip sfxSliderChanged;
        public AudioClip SliderChanged => sfxSliderChanged;

        [SerializeField] private CAudioEvent[] audioEvents;
        public CAudioEvent[] AudioEvents => audioEvents;
    }
}