using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

[RequireComponent (typeof (AudioSource))]
public class BeatPulse : MonoBehaviour {
	public AudioSource _audioSource;

    //Beat Detection
    public float _bpm;
    float _beatTime, _beatTimeD8;
    int _tap;
    public static bool _beatFull, _beatD8;
    float[] _tapTime = new float[4];
    float _beatTimer, _beatTimerD8;
    int _beatCount, _beatCountD;
    public static int _beatCountX2, _beatCountX4, _beatCountX8, _beatCountX16, _beatCountD2, _beatCountD4;
    bool _customBeat;

    void BeatDetection() {
        if (Input.GetKeyUp(KeyCode.F1)) {
            _customBeat = true;
            _tap = 0;
        }

        if (_customBeat) {
            if (Input.GetKeyDown(KeyCode.F2)) {
                if (_tap < 4) {
                    _tapTime[_tap] = Time.realtimeSinceStartup;
                    _tap++;
                }
                if (_tap == 4) {
                    _bpm = 60 / ((_tapTime[3] - _tapTime[0]) * 0.25f);
                    _tap = 0;
                    _beatTimer = 0;
                    _beatTimeD8 = 0;
                    _beatCount = 0;
                    _beatCountD = 0;
                    _customBeat = false;
                }
            }
        }
        _beatFull = false;
        _beatTime = 60 / _bpm;
        _beatTimer += Time.deltaTime;
       
        if (_beatTimer >= _beatTime) {
            _beatTimer -= _beatTime;
            _beatFull = true;
            _beatCount++;
        }
        _beatCountX2 = _beatCount % 2;
        _beatCountX4 = _beatCount % 4;
        _beatCountX8 = _beatCount % 8;
        _beatCountX16 = _beatCount % 16;

        _beatD8 = false;
        _beatTimeD8 = _beatTime / 8;
        _beatTimerD8 += Time.deltaTime;

        if (_beatTimerD8 >= _beatTimeD8) {
            _beatTimerD8 -= _beatTimeD8;
            _beatD8 = true;
            _beatCountD++;
        }
        _beatCountD2 = _beatCountD % 4;
        _beatCountD4 = _beatCountD % 2;
    }

	void Update () {
        BeatDetection();
    }
}