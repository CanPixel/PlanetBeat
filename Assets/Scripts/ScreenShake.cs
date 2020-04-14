using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour {
    private Vector3 basePos;
    private Vector3 baseRot;

    private float intensity;

    void Update() {
        if(intensity > 0) {
            transform.localPosition = basePos + new Vector3(Mathf.Sin(Time.time * 10f * intensity) * intensity, Mathf.Cos(Time.time * 10f * intensity) * intensity, 0);
            intensity -= Time.deltaTime;
        }
    }

    public void Shake(float intensity) {
        AudioManager.PLAY_SOUND("Leap", 1.3f, 0.3f);
        basePos = transform.localPosition;
        this.intensity = intensity;
    }
}
