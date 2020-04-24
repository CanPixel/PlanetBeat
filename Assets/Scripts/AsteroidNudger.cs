using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidNudger : MonoBehaviour {
    public PickupableObject asteroid;

    public float nudgeForce = 10f;

    void OnTriggerStay2D(Collider2D col) {
        if(col.tag == "ORBIT") {
            var orbit = col.GetComponent<Orbit>().planet;
            if(asteroid != null && asteroid.ownerPlayer != null && orbit.playerNumber == asteroid.ownerPlayer.playerNumber) {
                var dir = (asteroid.transform.position - orbit.transform.position).normalized;
                if(Mathf.Abs(dir.x) < 0.5f && Mathf.Abs(dir.y) < 0.5f) return;
                asteroid.rb.AddForce(-dir * nudgeForce);
            }
        }
    }
}
