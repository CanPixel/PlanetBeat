using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public float SoundLevel = 0.2f, AmbientLevel = 1;
	private float SoundBase, AmbientBase;

	protected float crescendo = 0;
	protected float crescendoTarget = 1;
	public static void SetCrescendo(float target) {
		instance.crescendoTarget = target;
	}

	public float GetMasterSoundLevel {
		get {return SoundLevel;}
	}

	public float GetMasterAmbientLevel {
		get {return AmbientLevel;}
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

		UpdateVolumeLevels();
	}

	private void UpdateVolumeLevels() {
		SoundBase = SoundLevel;
		AmbientBase = AmbientLevel;
	}

	void Update() {
		crescendo = Mathf.Lerp(crescendo, crescendoTarget, Time.deltaTime);
	}

	void LateUpdate() {
		if(SoundBase != SoundLevel || AmbientBase != AmbientLevel) UpdateVolumeLevels();
	}

	public static float GetMasterAmbientVolume() {
		if(instance == null) return 0;
		else return instance.GetMasterAmbientLevel;
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
