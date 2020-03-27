using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOnCollider : MonoBehaviour {
    public PlayerShip host;
    public LockOnAim lockAim;

    public GameObject reticle;
    public Image left, right;

    private GameObject target;

    void Start() {
        reticle.SetActive(false);
    }

    void Update() {
        if(target != null) SetReticle(target.transform.position);

        reticle.transform.localScale = Vector3.Lerp(reticle.transform.localScale, Vector3.one * 1.5f + Vector3.one * (Mathf.Sin(Time.time * 15f) + 0.5f), Time.deltaTime * 4f);
        reticle.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(reticle.transform.eulerAngles.z, 0, Time.deltaTime * 1.5f));
        left.color = right.color = Color.Lerp(left.color, lockAim.selectColor + new Color(0, 0, 0, 1), Time.deltaTime * 4f);

        if(Input.GetKeyUp(KeyCode.Space) && target != null) host.hookShot.FireLockOn(target);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.tag == "Resource") {
            if(target != col.gameObject) {
                reticle.transform.localScale = Vector3.one * 3.5f;
                target = col.gameObject;
                reticle.SetActive(true);
                reticle.transform.rotation = Quaternion.Euler(0, 0, 270);
                left.color = right.color = Color.white;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.tag == "Resource" && col.gameObject == target) ClearTarget();
    }

    protected void SetReticle(Vector2 pos) {
        reticle.transform.position = pos;
    }

    protected void ClearTarget() {
        left.color = right.color = Color.white;
        target = null;
        reticle.SetActive(false);
        reticle.transform.localScale = Vector3.one * 2f;
    }
}
