using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBounce : MonoBehaviour {
    private float delay = 0;

    private Vector3 baseScale;

    void Start() {
        baseScale = transform.localScale;
    }

    public void Bounce() {
        if(delay > 0) return;
        transform.localScale = baseScale * 1.25f;
        delay = 0.5f;
    }

    void Update() {
        if(delay > 0) delay -= Time.deltaTime;

        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 2f);
    }
}
