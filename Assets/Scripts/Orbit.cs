using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {
    [SerializeField] private GameObject gravityRing;

    [Header("PHYSICS")]
    [Range(200, 3000)]
    public float orbitEffectDistance = 2000;

    public float planetForce = 2000f;
    private float totalForce;
    public bool isDefault = true; 

    private float Mass;

    void Start() {
        var Planet = GetComponentInParent<Planet>();

        if (isDefault) {
            planetForce = 1450f;
            Mass = 1f; 
        }
    }

    public void SetOrbitDistance(float ringOffs) {
        gravityRing.transform.localPosition = new Vector3(ringOffs, 0, 0);
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "PLAYERSHIP" || col.tag == "Resource") {
            var dist = Vector3.Distance(col.transform.position, transform.position);

            float totalForce = -(orbitEffectDistance / planetForce * (Mass / 2f)); 
            var orientation = (col.transform.position - transform.position).normalized;

            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
    }
}
