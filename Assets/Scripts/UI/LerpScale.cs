using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpScale : MonoBehaviour {
    private Vector3 baseScale;
    public float recover = 3f;
    
    void Start() {
        baseScale = transform.localScale;
    }

    void Update() {
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * recover);
    }

    public void Scale(float magn) {
        transform.localScale = Vector3.one * magn;
    }
}
