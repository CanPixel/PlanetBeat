using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ResourceRotate : MonoBehaviour {
    public Vector3 axis;
    public float speed;

    void Update() {
        transform.RotateAround(transform.parent.position, axis, speed);
    }
}
