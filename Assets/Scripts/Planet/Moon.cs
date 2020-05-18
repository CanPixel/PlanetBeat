using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour {
    public float orbitSpeed = 20f;
    private Transform focalPoint;

    void Start() {
        focalPoint = transform.parent;
    }

    void FixedUpdate() {
        transform.RotateAround(focalPoint.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }
}
