using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public float SoundLevel = 0.1f, AmbientLevel = 0.1f, MusicLevel = 0.8f;
	private float SoundBase, AmbientBase, MusicBase;

	private float OldMusicLevel;

	public float GetMasterSoundLevel {
		get {return SoundLevel;}
	}

	public float GetMasterAmbientLevel {
		get {return AmbientLevel;}
	}

	public float GetMasterMusicLevel {
		get {return MusicBase;}
	}

	private static AudioManager instance;
	public AmbientNoise ambientDrone;

	[Header("Sounds")]
	public AudioClip[] sounds;

	private Dictionary<string, AudioClip> soundBank = new Dictionary<string, AudioClip>();
	private int OSTFocus = 0;

	void Start() {
		if(instance != null) {
			instance.soundBank.Clear();
			instance.ambientDrone = ambientDrone;
			instance.sounds = sounds;
		    for(int i = 0; i < sounds.Length; i++) instance.soundBank.Add(sounds[i].name.ToLower(), sounds[i]);
			instance.UpdateVolumeLevels();
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
		for(int i = 0; i < sounds.Length; i++) soundBank.Add(sounds[i].name.ToLower(), sounds[i]);

		OldMusicLevel = MusicLevel;
		UpdateVolumeLevels();
	}

	private void UpdateVolumeLevels() {
		SoundBase = SoundLevel;
		AmbientBase = AmbientLevel;
		MusicBase = MusicLevel;
	}

	void LateUpdate() {
		if(SoundBase != SoundLevel || AmbientBase != AmbientLevel || MusicBase != MusicLevel) UpdateVolumeLevels();
	}

	public static void MuteMusic(bool i) {
		if(instance == null) return;
		if(i) instance.MusicLevel = 0;
		else instance.MusicLevel = instance.OldMusicLevel;
	}

	public static float GetMasterAmbientVolume() {
		if(instance == null) return 0;
		else return instance.GetMasterAmbientLevel;
	}

	public static float GetMasterMusicVolume() {
		if(instance == null) return 0;
		else return instance.GetMasterMusicLevel;
	}

	public void PlaySound(string name) {
		if(soundBank.Count <= 0) return;
		if(soundBank.ContainsKey(name.ToLower())) PlayClipAtPoint(soundBank[name.ToLower()], Camera.main.transform.position, 0.5f, 1, 0);
		else Debug.LogError("Could not find '" + name.ToLower() + "' sound file!");
	}

	public static void PLAY_UNIQUE_SOUND(string name, float volume = 1.0f, float range = 0.3f, float basepitch = 0) {
		float pitch = Random.Range(1f - range, 1f + range) + basepitch;
		PLAY_SOUND(name.ToLower(), Camera.main.transform.position - new Vector3(0, 20, 0), volume, pitch);
	}

	public static void PLAY_UNIQUE_SOUND_AT(string name, Vector3 pos, float volume = 1.0f, float range = 0.3f, float basepitch = 0, float spatialBlend = 0.75f) {
		float pitch = Random.Range(1f - range, 1f + range) + basepitch;
		PLAY_SOUND(name.ToLower(), pos, volume, pitch, spatialBlend);
	}

	public static void PLAY_SOUND(string name, float volume = 1.0f, float pitch = 1.0f) {
		if(Camera.main == null) return;
		PLAY_SOUND(name.ToLower(), Camera.main.transform.position - new Vector3(0, 20, 0), volume, pitch);
	}

	public static void PLAY_SOUND(string name, Vector3 pos, float volume, float pitch, float spatial = 0.75f) {
		if(instance == null) return;
		
		if(instance.soundBank.ContainsKey(name.ToLower())) PlayClipAtPoint(instance.soundBank[name.ToLower()], new Vector3(pos.x, 1, pos.z), volume * instance.GetMasterSoundLevel, pitch, spatial);
		else Debug.LogError("Could not find '" + name.ToLower() + "' sound file!");
	}

	private static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 pos, float volume, float pitch, float spatial) {
		var tempGO = new GameObject("Temp Audio");
		tempGO.transform.position = pos;
		var aSource = tempGO.AddComponent<AudioSource>();
		aSource.clip = clip;
		aSource.volume = volume;
		aSource.pitch = pitch;
		aSource.spatialBlend = spatial;
		aSource.dopplerLevel = 0;
		aSource.Play();
		Destroy(tempGO, clip.length);
		return aSource;
	}
}
