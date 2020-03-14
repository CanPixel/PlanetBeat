using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour {
    private Rigidbody2D rb;

    public bool held = false;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D col) {
        if(col.gameObject.tag == "HOOKSHOT") {
            transform.position = col.transform.position;
            rb.simulated = false;
            col.gameObject.GetComponent<HookTip>().hookShot.CatchObject(gameObject);
        }
        else if(col.gameObject.tag == "PlayerPlanet") {
            if(held) ;
        }
    }
}
