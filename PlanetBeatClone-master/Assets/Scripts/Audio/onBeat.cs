using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class onBeat : MonoBehaviour {
    public int beatSection;

    public AudioMixer _audioMixer;

    [System.Serializable]
    public enum BeatTime {
        X2, X4, Full,  Every4
    }
    
    [System.Serializable]
    public class BeatEvent {
        public string name = "OnBeat";
        public UnityEvent Event;
        public AudioSource src;
        public BeatTime BeatTime;
        public int afterBeat = 0;
        [HideInInspector] public float delay = 0;
    }
    public BeatEvent[] beatEvents;

    private float[] vol;
    
    void Start() {
        beatSection = 0;
        vol = new float[beatEvents.Length];
        for(int i = 0; i < vol.Length; i++) {
            if(beatEvents[i].src == null) {
                vol[i] = 0;
                continue;
            }
            vol[i] = beatEvents[i].src.volume;
        }
    }

    void Update() {
        int index = 0;
        foreach(var i in beatEvents) {
            if(i.src != null) i.src.volume = AudioManager.GetMasterMusicVolume() * vol[index];
            index++;
            switch(i.BeatTime) {
                case BeatTime.X2:
                    if(i.delay > 0) {
                        i.delay -= Time.deltaTime;
                        continue;
                    }
                    if(BeatPulse._beatCountX2 == 0 && beatSection >= i.afterBeat) {
                        i.Event.Invoke(); 
                        PlaySound(i);
                        i.delay = 1f;
                    }
                    break;
                case BeatTime.X4:
                    if(i.delay > 0) {
                        i.delay -= Time.deltaTime;
                        continue;
                    }
                    if(BeatPulse._beatCountX4 % 4 == 0 && beatSection >= i.afterBeat) {
                        i.Event.Invoke(); 
                        PlaySound(i);
                        i.delay = 0.2f;
                    }
                    break;
                case BeatTime.Every4:
                    if(i.delay > 0) {
                        i.delay -= Time.deltaTime;
                        continue;
                    }
                    if (BeatPulse._beatFull && beatSection >= i.afterBeat) {
                        i.Event.Invoke();
                        PlaySound(i);
                        i.delay = 0.2f;
                    }
                    break;
                case BeatTime.Full:
                    if(i.delay > 0) {
                        i.delay -= Time.deltaTime;
                        continue;
                    }
                     if (BeatPulse._beatCountX8 == 0 && BeatPulse._beatFull && beatSection >= i.afterBeat) {
                        i.Event.Invoke(); 
                        PlaySound(i);
                        i.delay = 0.2f;
                     }
                    break;
                default:
                    break;
            }
        }

        if (BeatPulse._beatCountX8 == 0 && BeatPulse._beatFull) {
            beatSection++;
            //_audioMixer.SetFloat("lowcutoff", Random.Range(500f, 10000f));
            //_audioMixer.SetFloat("delayecho", Random.Range(500f, 10000f));

        }
        //if (BeatPulse._beatFull) src.Play();
    }

    public void PlaySound(BeatEvent evt) {
        if(evt.src == null) return;
        evt.src.Play();
    }
}
