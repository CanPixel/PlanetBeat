using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    [Header("REFERENCES")]
    public CircleCollider2D influenceRing;
    public CircleCollider2D stableOrbitRing;

    private GameObject trail, orbit;
    [Header("PHYSICS")]
    [Range(100, 800)]
    public float stableOrbitDistance = 200;
    [Range(200, 1000)]
    public float orbitEffectDistance = 700;
    public float OrbitSpeed = 4;
    [Range(0, 1)]
    public float Mass = 1;

    [HideInInspector]
    private List<GameObject> orbitOBJ = new List<GameObject>();
    public void AddToOrbit(GameObject obj) {
        orbitOBJ.Add(obj);
    }

    public bool blackHole = false;

    private Orbit orbitScr;

    void OnValidate() {
        if(OrbitSpeed < 0) OrbitSpeed = 0;
    //    if(orbitEffectDistance < stableOrbitDistance) orbitEffectDistance = stableOrbitDistance + 1;
      //  UpdateOrbits();
    }

    void Start() {
        if(blackHole) Mass = 100;
        trail = GetComponentInChildren<TrailRenderer>().gameObject;
        orbit = Util.FindChildWithTag(transform, "ORBIT");
        orbitScr = orbit.GetComponent<Orbit>();
        orbitScr.SetStableOrbit(stableOrbitDistance);
    }

    void Update() {
        UpdateOrbits();

    //    trail.transform.localPosition = new Vector3(stableOrbitDistance, 0, 0);
        orbit.transform.localRotation = Quaternion.Euler(orbit.transform.localEulerAngles.x, orbit.transform.localEulerAngles.y, orbit.transform.localEulerAngles.z + OrbitSpeed);
    }

    private void UpdateOrbits() {
        //stableOrbitRing.radius = stableOrbitDistance - 50;
        influenceRing.radius = orbitEffectDistance;
        if(orbitScr == null) {
            orbit = Util.FindChildWithTag(transform, "ORBIT");
            orbitScr = orbit.GetComponent<Orbit>();
        }
        orbitScr.SetOrbitDistance(orbitEffectDistance);
    }

    /* 
    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP") {
            var dist = Vector3.Distance(col.transform.position, transform.position);
            float totalForce = -(stableOrbitDistance / 100f * (Mass / 2f)) / (dist * dist); 
            var orientation = (col.transform.position - transform.position).normalized;

            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP") {
            var ship = col.GetComponent<PlayerShip>();
            if(ship != null) ship.NeutralizeForce();
        }
    } */

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP" || col.tag == "Resource") {
            var dist = Vector3.Distance(col.transform.position, transform.position);
            //float totalForce = -(stableOrbitDistance / 100f * (Mass / 2f)) / (dist * dist);

            float PlanetForce = 50f;
            float totalForce = -(orbitEffectDistance / PlanetForce * (Mass / 2f)); 
            var orientation = (col.transform.position - transform.position).normalized;

            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP") {
            var ship = col.GetComponent<PlayerShip>();
            if(ship != null) ship.NeutralizeForce();
        }
    }
}
