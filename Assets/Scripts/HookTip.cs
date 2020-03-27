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

    void OnCollisionEnter2D(Collision2D col) {
        if(col.gameObject.tag == "Resource") {
            var ast = col.gameObject.GetComponent<Asteroid>();
            ast.Capture(hookShot);
        }
    }
}
