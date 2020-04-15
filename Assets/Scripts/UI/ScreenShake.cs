using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour {
    private Vector3 basePos;
    private Vector3 baseRot;

    private float intensity;
    private bool turn = false;

    void Update() {
        if(intensity > 0) {
            if(!turn) transform.localPosition = basePos + new Vector3(Mathf.Sin(Time.time * 10f * intensity) * intensity, Mathf.Cos(Time.time * 10f * intensity) * intensity, 0);
            else transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 10f * intensity) * intensity * 10f), Time.deltaTime * 2f);
            intensity -= Time.deltaTime;
        } else transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 2f);
    }

    public void Shake(float intensity) {
        AudioManager.PLAY_SOUND("Leap", 1.3f, 0.3f);
        basePos = transform.localPosition;
        this.intensity = intensity;
        turn = false;
    }

    public void Turn(float intens) {
        turn = true;
        this.intensity = intens;
        baseRot = transform.localEulerAngles;
    }  
}
