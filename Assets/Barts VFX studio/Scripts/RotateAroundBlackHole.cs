using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundBlackHole : MonoBehaviour {
    public Transform Target;
    public float Speed = 10f;

    protected void Update() {
        transform.RotateAround(Target.position,Vector3.forward,Time.deltaTime*Speed);
    }
}
