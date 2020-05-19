using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BeatPulse : MonoBehaviour {
    public float _bpm;
    float _beatTime, _beatTimeD8;
    public static bool _beatFull, _beatD8;
    float _beatTimer, _beatTimerD8;
    int _beatCount, _beatCountD;
    public static int _beatCountX2, _beatCountX4, _beatCountX8, _beatCountX16, _beatCountD2, _beatCountD4;

    public static bool BEGIN = false;

    public static bool AudioPhaseSwitcher = false;

    private void BeatDetection() {
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
        if(BEGIN) BeatDetection();
    }
}