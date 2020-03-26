using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunGravity : MonoBehaviour {
    [Header("PHYSICS")]
    [Range(200, 3000)]
    public float orbitEffectDistance = 2000;
    public float OrbitSpeed = 4;
    public float planetForce = 4000;

    [Range(0, 1)]
    public float Mass = 1;

    private List<DyingObject> destroyingObjects = new List<DyingObject>();
    private class DyingObject {
        public GameObject obj;
        public float lifetime = 0;

        public DyingObject(GameObject gameObject) {
            obj = gameObject;
        }
    }

    //Eating animation
    void Update() {
        for(int i = 0; i < destroyingObjects.Count; i++) {
            var obj = destroyingObjects[i].obj;
            if(obj == null) continue;
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
            destroyingObjects[i].lifetime += Time.deltaTime;
            obj.transform.localPosition = Vector3.Lerp(obj.transform.localPosition, Vector3.zero, destroyingObjects[i].lifetime);

            if(destroyingObjects[i].lifetime > 1) {
                destroyingObjects.RemoveAt(i);
                GameManager.DESTROY_SERVER_OBJECT(obj); 
            }
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.tag == "PLAYERSHIP" || col.tag == "Resource") {
            var dist = Vector3.Distance(col.transform.position, transform.position);

            // Hoe hoger hoe slapper de force
            float totalForce = -(orbitEffectDistance / planetForce * (Mass / 2f));
            var orientation = (col.transform.position - transform.position).normalized;

            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
    }


    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.tag == "Resource") {
            destroyingObjects.Add(new DyingObject(collision.gameObject));
            //GameManager.DESTROY_SERVER_OBJECT(collision.gameObject); 
        }
    }
}
