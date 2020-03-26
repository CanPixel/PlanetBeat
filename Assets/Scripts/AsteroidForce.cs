// For short bursts of force (such as an explosion) use ForceMode2D.Impulse
// This adds an instant force impulse to the Rigidbody2D, using mass

using System.Collections;
using UnityEngine;

public class AsteroidForce : MonoBehaviour {
    private Rigidbody2D rb2D;
    public float thrust = 10.0f;
    private bool startForce = true;

    private void Start() {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
    }

    void OnEnable() {
        transform.SetParent(GameObject.FindGameObjectWithTag("ASTEROIDBELT").transform, true);
    }

    private void Update() {
        if (startForce) {
            rb2D.AddForce(-transform.right * thrust);
            startForce = false;
        }
    }
}