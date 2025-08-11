using Atavism.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

namespace Atavism
{
    public class UIAtavismMusicSettings //: MonoBehaviour
    {
        private AudioMixer masterMixer;
        private VisualElement rootElement;
        private Slider masterVol;
        private Slider musicVol;
        private Slider sfxVol;
        private Slider uiVol;
        private Slider ambientVol;
        private Slider footstepsVol;
        private float startTimer = 0f;
        private MonoBehaviour _monoBehaviour;
        public void Setup(VisualElement tabSounds, VisualElement screen, MonoBehaviour monoBehaviour,AudioMixer masterMixer)
        {
            rootElement = tabSounds;
            _monoBehaviour = monoBehaviour;
            this.masterMixer = masterMixer;
            InitializeUI();

            startTimer = Time.time + 0.3f;

            masterVol.value = AtavismSettings.Instance.GetAudioSettings().masterLevel;
            musicVol.value = AtavismSettings.Instance.GetAudioSettings().musicLevel;
            sfxVol.value = AtavismSettings.Instance.GetAudioSettings().sfxLevel;
            uiVol.value = AtavismSettings.Instance.GetAudioSettings().uiLevel;
            ambientVol.value = AtavismSettings.Instance.GetAudioSettings().ambientLevel;
            footstepsVol.value = AtavismSettings.Instance.GetAudioSettings().footstepsLevel;
        }
        // void OnEnable()
        // {
        //     rootElement = uiDocument.rootVisualElement;
        //     InitializeUI();
        //
        //     startTimer = Time.time + 0.3f;
        //
        //     masterVol.value = AtavismSettings.Instance.GetAudioSettings().masterLevel;
        //     musicVol.value = AtavismSettings.Instance.GetAudioSettings().musicLevel;
        //     sfxVol.value = AtavismSettings.Instance.GetAudioSettings().sfxLevel;
        //     uiVol.value = AtavismSettings.Instance.GetAudioSettings().uiLevel;
        //     ambientVol.value = AtavismSettings.Instance.GetAudioSettings().ambientLevel;
        //     footstepsVol.value = AtavismSettings.Instance.GetAudioSettings().footstepsLevel;
        // }

        void InitializeUI()
        {
            masterVol = rootElement.Q<Slider>("MasterVolume");
            musicVol = rootElement.Q<Slider>("MusicVolume");
            sfxVol = rootElement.Q<Slider>("SFXVolume");
            uiVol = rootElement.Q<Slider>("UIVolume");
            ambientVol = rootElement.Q<Slider>("AmbientVolume");
            footstepsVol = rootElement.Q<Slider>("FootstepsVolume");

            masterVol.RegisterValueChangedCallback(e => SetMasterLev(e.newValue));
            musicVol.RegisterValueChangedCallback(e => SetMusicLev(e.newValue));
            sfxVol.RegisterValueChangedCallback(e => SetSfxLev(e.newValue));
            uiVol.RegisterValueChangedCallback(e => SetUiLev(e.newValue));
            ambientVol.RegisterValueChangedCallback(e => SetAmbientLev(e.newValue));
            footstepsVol.RegisterValueChangedCallback(e => SetFootstepsLev(e.newValue));
        }

        public void SetSfxLev(float sfxLev)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetAudioSettings().sfxLevel = sfxLev;
            masterMixer.SetFloat("sfxVol", sfxLev);
        }
        public void SetMusicLev(float musicLev)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetAudioSettings().musicLevel = musicLev;
            masterMixer.SetFloat("musicVol", musicLev);
        }
        public void SetMasterLev(float masterLev)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetAudioSettings().masterLevel = masterLev;
            masterMixer.SetFloat("masterVol", masterLev);
        }
        public void SetUiLev(float uiLev)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetAudioSettings().uiLevel = uiLev;
            masterMixer.SetFloat("uiVol", uiLev);
        }
        public void SetAmbientLev(float ambientLev)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetAudioSettings().ambientLevel = ambientLev;
            masterMixer.SetFloat("AmbientVol", ambientLev);
        }
        public void SetFootstepsLev(float footstepsLev)
        {
            if (startTimer > Time.time)
                return;
            AtavismSettings.Instance.GetAudioSettings().footstepsLevel = footstepsLev;
            masterMixer.SetFloat("FootstepsVol", footstepsLev);
        }

      
    }
}
