using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class SoundManager : MonoBehaviour {
	private static SoundManager instance;

	/* OLD SOUNDSYSTEM */
	private Dictionary<string, string> soundBank = new Dictionary<string, string>();

	/*NEW SOUNDSYSTEM (FMOD) */
	[System.Serializable]
	public class SoundEvent {
		public string labelName;
		public string fmodPath;
	}
	public SoundEvent[] soundEvents;

	void Start() {
		if(instance != null) {
			instance.soundBank.Clear();
		    for(int i = 0; i < soundEvents.Length; i++) instance.soundBank.Add(soundEvents[i].labelName.ToLower(), soundEvents[i].fmodPath);
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
		for(int i = 0; i < soundEvents.Length; i++) soundBank.Add(soundEvents[i].labelName.ToLower(), soundEvents[i].fmodPath);
	}

	public void PlaySound(string name) {
		if(soundBank.Count <= 0) return;
		if(soundBank.ContainsKey(name.ToLower())) PlayClipAtPoint(name.ToLower());
		else Debug.LogWarning("Could not find '" + name.ToLower() + "' sound file!");
	}

	public static void PLAY_SOUND(string name) {
		if(instance == null) return;
		instance.PlaySound(name);
	}

	private static void PlayClipAtPoint(string path) {
		FMODUnity.RuntimeManager.PlayOneShot(instance.soundBank[path], new Vector3(0, 0, 0));
	}
}
