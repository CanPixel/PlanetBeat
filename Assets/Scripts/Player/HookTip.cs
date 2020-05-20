using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTip : MonoBehaviour {
    public HookShot hookShot;

    void OnEnable() {
        if(hookShot.hostPlayer.photonView != null && !hookShot.hostPlayer.photonView.IsMine) {
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(GetComponent<CircleCollider2D>());
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if((col.gameObject.tag == "ResourceTutorial" || col.gameObject.tag == "Resource") && hookShot.IsShooting()) col.gameObject.GetComponent<Asteroid>().Capture(hookShot);

        if((col.gameObject.tag == "InfectroidTutorial" || col.gameObject.tag == "Powerup") && hookShot.IsShooting()) col.gameObject.GetComponent<PickupableObject>().Capture(hookShot);
    }
}
