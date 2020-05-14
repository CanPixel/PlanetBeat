using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidNudger : MonoBehaviour {
    public PickupableObject asteroid;
    public float nudgeForce = 2;

    public bool isInfectroid = false;

    [HideInInspector] public bool isInOrbit = false;

    private Rigidbody2D rb;

    void Start() {
        rb = asteroid.GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "ORBIT") {
            // var planet = col.transform.parent.GetComponent<PlayerPlanets>();
            //if(isInfectroid || (!isInfectroid && planet != null && planet.HasPlayer())) isInOrbit = true;
            isInOrbit = true;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.tag == "ORBIT") {
            //var planet = col.transform.parent.GetComponent<PlayerPlanets>();
            //if(isInfectroid || (!isInfectroid && planet != null && planet.HasPlayer())) rb.velocity /= nudgeForce;
            rb.velocity /= nudgeForce;
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "ORBIT") {
          //  var planet = col.transform.parent.GetComponent<PlayerPlanets>();
           //  if(isInfectroid || (!isInfectroid && planet != null && planet.HasPlayer())) isInOrbit = false;
            isInOrbit = false;
        }
    }
}
