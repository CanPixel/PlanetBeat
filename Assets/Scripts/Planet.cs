using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour {
    [Header("REFERENCES")]
    public CircleCollider2D influenceRing;
    //Main Texture + Glow texture
    public Image glow, src; 

    private GameObject trail, orbit;
    [Header("PHYSICS")]
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
    private float glowOffset;

    void OnValidate() {
        if(OrbitSpeed < 0) OrbitSpeed = 0;
    }

    void Start() {
        glowOffset = 1f + Random.Range(0f, 1f);
        if(blackHole) Mass = 100;
        trail = GetComponentInChildren<TrailRenderer>().gameObject;
        orbit = Util.FindChildWithTag(transform, "ORBIT");
        orbitScr = orbit.GetComponent<Orbit>();
    }

    void Update() {
        //Glow fluctuation
        glow.color = Color.Lerp(glow.color, glow.color + new Color(0, 0, 0, Mathf.Sin(Time.time * 3f + glowOffset)), Time.deltaTime * 1f);

        UpdateOrbits();
        orbit.transform.localRotation = Quaternion.Euler(orbit.transform.localEulerAngles.x, orbit.transform.localEulerAngles.y, orbit.transform.localEulerAngles.z + OrbitSpeed);
    }

    private void UpdateOrbits() {
        influenceRing.radius = orbitEffectDistance;
        if(orbitScr == null) {
            orbit = Util.FindChildWithTag(transform, "ORBIT");
            orbitScr = orbit.GetComponent<Orbit>();
        }
        orbitScr.SetOrbitDistance(orbitEffectDistance);
    }

    public void SetTexture(TextureSwitcher.TextureElement element) {
        src.sprite = element.src;
        if(element.glow == null) {
            glow.sprite = null;
            glow.color = new Color(0, 0, 0, 0);
            glow.enabled = false;
            return;
        }
        glow.enabled = true;
        glow.color = new Color(1, 1, 1, 1);
        glow.sprite = element.glow;
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP" || col.tag == "Resource") {
            var dist = Vector3.Distance(col.transform.position, transform.position);

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
