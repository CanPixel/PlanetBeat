using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidNudger : MonoBehaviour {
    public PickupableObject asteroid;

    public float defaultRbMass = 0.15f, inOrbitRbMass = 0.1f;

    private Rigidbody2D rb;

    void Start() {
        rb = asteroid.GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "ORBIT") {
            var orb = col.GetComponent<Orbit>();
            if(orb == null) return;
            var orbit = orb.planet;
            if(asteroid != null && orbit != null) rb.mass = inOrbitRbMass;
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "ORBIT") rb.mass = defaultRbMass;
    }
}
