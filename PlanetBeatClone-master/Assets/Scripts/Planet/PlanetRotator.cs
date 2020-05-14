using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotator : MonoBehaviour {
    public Vector3 rotateSpeed = new Vector3(0, 1, 0);
    
    void FixedUpdate() {
        var speed = rotateSpeed * Time.deltaTime;
        transform.Rotate(speed);
    }
}
