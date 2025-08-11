using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AT_MASTERAUDIO_PRESET
using DarkTonic.MasterAudio;
#endif
namespace Atavism
{
    [System.Serializable]
    public class AtavismInventoryAudioEntry
    {
        public string name = "";
        public AudioClip[] audioClips;
#if AT_MASTERAUDIO_PRESET
        public string[] audioClipsName;
#endif
        public float volume = 1.0f;
    
    }

    public class AtavismInventoryAudioManager : MonoBehaviour
    {
        static AtavismInventoryAudioManager instance; 
        [SerializeField]
        string mixerAudioGroup = "SFX";
        [SerializeField]
        List<AtavismInventoryAudioEntry> audio = new List<AtavismInventoryAudioEntry>();
       

        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
                return;
            instance = this;
        }

        public static AtavismInventoryAudioManager Instance
        {
            get
            {
                return instance;
            }
        }

         AudioSource playingAudio ;
        public void PlayAudio(string name, GameObject go)
        {
        //    Debug.LogError("AtavismInventoryAudioManager: PlayAudio " + name + " " + go );
                if (playingAudio != null)
                {
                    playingAudio.Stop();
                    Destroy(playingAudio.gameObject);
                }
                playingAudio =null;
            int index = AudioClipIndex(name);

            if (index > -1)
            {
#if AT_MASTERAUDIO_PRESET
               MasterAudio.StopAllSoundsOfTransform(go.transform);
               if (audio[index].audioClipsName.Length > 0)
               {
                   string audioClip = audio[index].audioClipsName[Random.Range(0, audio[index].audioClipsName.Length)];
                   if (!string.IsNullOrEmpty(audioClip))
                       MasterAudio.PlaySound3DAtTransform(audioClip, go.transform, audio[index].volume);
               }

#endif
                if (audio[index].audioClips.Length > 0)
                {
                    //float volume = audio[index].volume;
                    AudioClip audioClip = audio[index].audioClips[Random.Range(0, audio[index].audioClips.Length)];
                    if (audioClip != null)
                    {
                        GameObject goTemp = new GameObject("Audio");
                        goTemp.transform.position = go.transform.position;
                        AudioSource audios = goTemp.AddComponent<AudioSource>();
                        audios.outputAudioMixerGroup =
                            AtavismSettings.Instance.masterMixer.FindMatchingGroups(mixerAudioGroup)[0];
                        audios.clip = audioClip;
                        audios.volume = audio[index].volume;
                        audios.spatialBlend = 1f;
                        audios.maxDistance = 30f;
                        audios.rolloffMode = AudioRolloffMode.Linear;
                        audios.Play();
                        Destroy(goTemp, audios.clip.length);
                        playingAudio = audios;
                    }
                }

            }
            else
            {
                Debug.LogWarning("Audi Clip Not Found "+name);
            }
        }


        private int AudioClipIndex(string name)
        {
            for (int i = 0; i < audio.Count; i++)
            {   
                if (audio[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}