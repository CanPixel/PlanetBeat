using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BlackHoleGravity : MonoBehaviour {
    [Header("PHYSICS")]
    [Range(0, 10)]
    public float planetForceResource = 1;
    [Range(0, 10)]
    public float planetForcePlayer = 1;

    [Range(0, 1)]
    public float Mass = 1;

    [Space(10)]
    public bool killAsteroids = true;

    private List<DyingObject> destroyingObjects = new List<DyingObject>();
    private class DyingObject {
        public GameObject obj;
        public float lifetime = 0;

        public DyingObject(GameObject gameObject) {
            obj = gameObject;
        }
    }

    private AsteroidSpawner spawner;

    public void SpawnOnBeat() {
       // if(spawner == null) spawner = GameObject.FindGameObjectWithTag("CENTER").GetComponent<AsteroidSpawner>();
       // spawner.SpitAsteroidOnBeat();
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
                if(obj != null) {
                    if(GameManager.LOCAL_PLAYER != null && GameManager.LOCAL_PLAYER.photonView.IsMine) {
                        obj.GetPhotonView().TransferOwnership(0);
                        GameManager.DESTROY_SERVER_OBJECT(obj); 
                    }
                    else Destroy(obj);
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.tag == "PLAYERSHIP") {
            float totalForce = -(planetForcePlayer * (Mass / 2f));
            var orientation = (col.transform.position - transform.position).normalized;

            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
        else if (col.tag == "Resource" || col.tag == "Powerup") {
            var ast = col.GetComponent<PickupableObject>();
            var nudge = col.GetComponent<AsteroidNudger>();

            if(ast.held || !ast.IsDoneSpawning || (nudge != null && col.tag == "Powerup" && nudge.isInOrbit)) return; //Influence of sun gravity bij trailingObjects 

            float totalForce = -(planetForceResource * (Mass / 2f));
            var orientation = (col.transform.position - transform.position).normalized;
            col.GetComponent<Rigidbody2D>().AddForce(orientation * totalForce);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if ((collision.collider.tag == "Resource" || collision.collider.tag == "Powerup") && killAsteroids) {
            if(!collision.gameObject.GetComponent<PickupableObject>().IsDoneSpawning) return; 
            if(collision.gameObject.GetComponent<Asteroid>() != null) collision.gameObject.GetComponent<Asteroid>().DisableTrails();
            destroyingObjects.Add(new DyingObject(collision.gameObject));
            AudioManager.PLAY_SOUND("Burn", 0.8f, 1.3f);
        }
    }
}
