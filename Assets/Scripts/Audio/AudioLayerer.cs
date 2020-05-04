using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioLayerer : MonoBehaviour {
    public AudioLayer[] mixLayers;

    private List<AudioSource> audioSources = new List<AudioSource>(); 
    private bool playing = false;
    [Space(10)]
    public bool masterFadeIn = true;
    private float fadeVolume = 0;

    [System.Serializable]
    public class AudioLayer {
        public string name = "Audio Layer";
        public AudioMixerGroup mixGroup;
        public AudioClip sourceClip;
    }

    void Start() {
        CreateAudioLayers();
        PlayAll();
    }

    protected void CreateAudioLayers() {
        for(int i = 0; i < mixLayers.Length; i++) {
            var obj = new GameObject(mixLayers[i].name + " [Audio Layer]");
            obj.transform.SetParent(transform);
            var src = obj.AddComponent<AudioSource>();
            src.clip = mixLayers[i].sourceClip;
            src.outputAudioMixerGroup = mixLayers[i].mixGroup;
            src.loop = true;
            src.playOnAwake = false;
            audioSources.Add(src);
        }
    }

    protected void PlayAll() {
        for(int i = 0; i < audioSources.Count; i++) audioSources[i].Play();
        playing = true;
    }

    void LateUpdate() {
        if(!playing) return;

        if(masterFadeIn) fadeVolume = Mathf.Lerp(fadeVolume, 1f, Time.deltaTime);
        else fadeVolume = 1f;

        var stem = audioSources[0];
        for(int i = 1; i < audioSources.Count; i++) {
            audioSources[i].timeSamples = stem.timeSamples;
            audioSources[i].volume = fadeVolume;
        }
    }
}
