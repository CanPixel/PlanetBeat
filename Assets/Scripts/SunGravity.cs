using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunGravity : MonoBehaviour
{
    //[Header("REFERENCES")]
    //public CircleCollider2D influenceRing;

    [Header("PHYSICS")]
    [Range(200, 1000)]
    public float orbitEffectDistance = 2000;
    public float OrbitSpeed = 4;

    [Range(0, 1)]
    public float Mass = 1;


    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "PLAYERSHIP" || col.tag == "Resource")
        {
            var dist = Vector3.Distance(col.transform.position, transform.position);
            //float totalForce = -(stableOrbitDistance / 100f * (Mass / 2f)) / (dist * dist);

            // Hoe hoger hoe slapper de force
            float PlanetForce = 2000f;
            float totalForce = -(orbitEffectDistance / PlanetForce * (Mass / 2f));
            var orientation = (col.transform.position - transform.position).normalized;

            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        //Check to see if the Collider's name is "Chest"
        if (collision.collider.tag == "Resource")
        {
            Debug.Log("Hit");
            Destroy(collision.gameObject);
        }
    }
}
