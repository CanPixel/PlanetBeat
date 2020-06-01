﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Orbit : MonoBehaviour {
    public CircleCollider2D coll;
    [SerializeField] private GameObject gravityRing;
    public PlayerPlanets planet;

    [Header("PHYSICS - ORBIT TRAILS")]
    public int orbitTrailOffset = 700;
    
    [Range(0, 10)]
    public float planetForcePlayer = 1, planetForceResource = 1, planetForcePowerup = 1;
    [Range(0, 1)]
    public float Mass = 1;

    [Space(10)]
    public float PlayerExitVelocityReduction = 2;
    public float ResourceExitVelocityReduction = 2;
    public float PowerupExitVelocityReduction = 2;

    private float totalForce;
    private bool flip = false;

    void OnValidate() {
        if(orbitTrailOffset < 0) orbitTrailOffset = 0;
    }

    void Start() {
        if(Random.Range(0, 2) == 0) flip = true;
    }

    void Update() {
        coll.radius = orbitTrailOffset;
        gravityRing.transform.localPosition = new Vector3(orbitTrailOffset, 0, 0);
    }

    void FixedUpdate() {
        transform.rotation = Quaternion.Euler(0, 0, ((flip) ? -1f : 1f) * 300f * Time.time);
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP") {
            var dist = Vector3.Distance(col.transform.position, transform.position);
            float totalForce = -(planetForcePlayer * (Mass / 2f)); 
            var orientation = (col.transform.position - transform.position).normalized;
            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);

            var rot = Quaternion.LookRotation(orientation * 100f);
            col.GetComponent<PlayerShip>().SetLean(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);

        } else if(col.tag == "Resource" || col.tag == "Powerup" || col.tag == "InfectroidTutorial") {
            if(col.GetComponent<PickupableObject>().held) return; //Influence of gravity bij trailingObjects 
            var dist = Vector3.Distance(col.transform.position, transform.position);
            float totalForce = -(planetForceResource * (Mass / 2f)); 
            if(col.tag == "Powerup" || col.tag == "InfectroidTutorial") totalForce = -(planetForcePowerup * (Mass / 2f)); 
            var orientation = (col.transform.position - transform.position).normalized;
            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        } 
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP") {
            var ship = col.GetComponent<PlayerShip>();
            if(ship != null) ship.NeutralizeForce(PlayerExitVelocityReduction);

            col.GetComponent<PlayerShip>().SetLean(0, 0, 0);

        } else if(col.tag == "Resource" || col.tag == "Powerup" || col.tag == "InfectroidTutorial") {
            var rb = col.GetComponent<Rigidbody2D>();
            if(rb != null) {
                if(col.tag == "Resource") rb.velocity /= ResourceExitVelocityReduction;
                else if(col.tag == "Powerup" || col.tag == "InfectroidTutorial") rb.velocity /= PowerupExitVelocityReduction;
            }
        }
    } 
}
