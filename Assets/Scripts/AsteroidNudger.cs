using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidNudger : MonoBehaviour {
    public PickupableObject asteroid;
    public float nudgeForce = 2;

    [HideInInspector] public bool isInOrbit = false;

    private Rigidbody2D rb;

    void Start() {
        rb = asteroid.GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "ORBIT") isInOrbit = true;
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.tag == "ORBIT") rb.velocity /= nudgeForce;
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "ORBIT") isInOrbit = false;
    }
}
