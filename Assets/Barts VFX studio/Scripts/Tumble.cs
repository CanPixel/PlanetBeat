using UnityEngine;
using System.Collections;

public class Tumble : MonoBehaviour {
    public float tumble;

    void Update() {                                
       transform.Rotate(Vector3.forward * tumble * Time.deltaTime);
    }
}