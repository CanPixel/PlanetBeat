using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour {
    public float liveTime = 5.0f;
    
    void OnEnable() {
        transform.SetParent(GameObject.Find("Canvas").transform, true);
    }

    void Update() {
        liveTime -= Time.deltaTime;
        if(liveTime <= 0) {
            GameManager.DESTROY_SERVER_OBJECT(this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
