using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientNoise : MonoBehaviour {
	[HideInInspector]
	public float masterVolume = 1f;

	public float volumeBoost = 0f;
	
	private bool startFade = true;

	//WASH NOISE GEN
	private AudioSource wash;
	[System.Serializable]
	public class ProceduralAudioNoise {
		[HideInInspector]
		public float sampling_freq = 48000;

		[Range(0f, 1f)]
		public float noiseRatio = 0.5f;

		[Range(-1f, 1f)]
		public float offset;

		public float cutoffOn = 800;
		public float cutoffOff = 100;
		public bool cutOff, cutoffSweep;

		public float toneFrequency = 440f;
		public Vector2 frequencySweep = new Vector2(440, 440);
		[Range(0f, 1f)]
		public float sweepSpeed = 1;
		public float gain = 0.05f;

		[HideInInspector]
		public float increment, phase;
		private float sweep, cutoffTick;

		private System.Random rand = new System.Random();
		[HideInInspector]
		public AudioLowPassFilter lowPassFilter;

		public void Start(AudioLowPassFilter fil) {
			lowPassFilter = fil;
			cutoffTick = offset * sweepSpeed;
		}

		public void OnAudioFilterRead(float[] data, int channels) {
			float tonalPart = 0;
			float noisePart = 0;
			increment = toneFrequency * 2f * Mathf.PI / sampling_freq;

			for(int i = 0; i < data.Length; i++) {
				noisePart = noiseRatio * (float)(rand.NextDouble() * 2.0 - 1.0 + offset);
				phase = phase + increment;
				sweep++;
				if(phase > 2 * Mathf.PI) phase = 0;

				tonalPart = (1f - noiseRatio) * (float)(gain * Mathf.Sin(phase));

				data[i] = noisePart + tonalPart;
				if(channels == 2) {
					data[i + 1] = data[i];
					i++;
				}
			}
		}

		public void Update() {
			cutoffTick += Time.deltaTime;

			if(!cutoffSweep) lowPassFilter.cutoffFrequency = cutOff ? cutoffOn : cutoffOff;
			else {
				float frequency = (frequencySweep.y / 2f) * Mathf.Sin(cutoffTick * sweepSpeed) + ((frequencySweep.y / 2f) + (frequencySweep.x / 2f));
				lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, frequency, Time.deltaTime * sweepSpeed);
			}
		}
	}

	[Space(10)]
	public ProceduralAudioNoise washNoise;

	void Start() {
		washNoise.sampling_freq = AudioSettings.outputSampleRate;
		washNoise.Start(GetComponent<AudioLowPassFilter>());
		wash = GetComponent<AudioSource>();
		washNoise.Update();

		masterVolume = 0;
		startFade = true;
	}

	void OnAudioFilterRead(float[] data, int channels) {
		washNoise.OnAudioFilterRead(data, channels);
	}

	void Update () {
		if(startFade) {
			masterVolume = Mathf.Lerp(masterVolume, 1f, Time.deltaTime * 2f);
			if(Util.Approximately(masterVolume, 1f, 0.1f)) {
				masterVolume = 1f;
				startFade = false;
			}
		}

		washNoise.Update();
		wash.volume = (masterVolume * Mathf.Lerp(wash.volume, AudioManager.GetMasterAmbientVolume(), Time.deltaTime * 1f) + volumeBoost);
	}

	public float GetFrequency() {
		return washNoise.lowPassFilter.cutoffFrequency;
	}
}