using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidNudger : MonoBehaviour
{
    public PickupableObject asteroid;
    public float nudgeForce = 2;

    public bool isInfectroid = false;

    [HideInInspector] public bool isInOrbit = false;

    private Rigidbody2D rb;

    void Start()
    {
        rb = asteroid.GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "ORBIT")
        {
            if (isInfectroid) isInOrbit = true;
            var orbit = col.transform.GetComponent<Orbit>();
            if (orbit != null)
            {
                var planet = orbit.planet;
                if (!isInfectroid && planet != null && asteroid.ownerPlayer != null && asteroid.ownerPlayer.playerNumber == planet.playerNumber)
                {
                    isInOrbit = true;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "ORBIT")
        {
            if (isInfectroid) rb.velocity /= nudgeForce;
            var orbit = col.transform.GetComponent<Orbit>();
            if (orbit != null)
            {
                var planet = orbit.planet;
                if (!isInfectroid && planet != null && asteroid.ownerPlayer != null && asteroid.ownerPlayer.playerNumber == planet.playerNumber) rb.velocity /= nudgeForce;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "ORBIT")
        {
            if (isInfectroid) isInOrbit = false;
            var orbit = col.transform.GetComponent<Orbit>();
            if (orbit != null)
            {
                var planet = orbit.planet;
                if (!isInfectroid && planet != null && asteroid.ownerPlayer != null && asteroid.ownerPlayer.playerNumber == planet.playerNumber) isInOrbit = false;
            }
        }
    }
}