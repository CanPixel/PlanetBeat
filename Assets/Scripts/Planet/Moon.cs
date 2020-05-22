using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour {
    public float orbitSpeed = 20f;
    private Transform focalPoint;

    [HideInInspector] public bool flip = false;

    private Vector3 targetScale;

    void Start() {
        focalPoint = transform.parent;
    }

    void Update() {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 2f);
    }

    void FixedUpdate() {
        transform.RotateAround(focalPoint.position, (flip)? Vector3.down : Vector3.up, orbitSpeed * Time.deltaTime);
    }

    public void Init(float orbitSpeed, bool flip, float scale) {
        this.orbitSpeed = orbitSpeed;
        this.flip = flip;
        targetScale = Vector3.one * scale;
        transform.localScale = Vector3.zero;
    }
}
