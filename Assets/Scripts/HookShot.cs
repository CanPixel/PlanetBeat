using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HookShot : MonoBehaviour {
    public LineRenderer lineAim;
    public PlayerShip hostPlayer;
    private RectTransform rope;
    private CircleCollider2D tip;

    public Collider2D[] playerColliders;

    [Header("PHYSICS")]
    [Range(20, 200)]
    public float hookShotSpeed = 50;
    [Range(1, 10)]
    public float hookShotRange = 5;

    private bool isShootingHook = false, triggerHook = false;

    private float shootTimer = 0;
    private bool hitObject = false, didntCatch = false;
    private GameObject grabbedObj;

    void Start() {
        rope = transform.GetChild(0).GetComponent<RectTransform>();
        tip = rope.transform.GetChild(0).GetComponent<CircleCollider2D>();
        foreach(var i in playerColliders) if(i != null) Physics2D.IgnoreCollision(tip, i, true);
    }

    void Update() {
        if(grabbedObj != null) grabbedObj.transform.position = tip.transform.position;

        float hookScale = Mathf.Lerp(rope.transform.localScale.x, (IsShooting() ? 1 : 0.1f), Time.deltaTime * 2f);
        rope.transform.localScale = new Vector3(hookScale, hookScale, hookScale);
    
        if(Input.GetKey(KeyCode.Space)) {
            triggerHook = true;
            var hit = Physics2D.Raycast(rope.transform.position, rope.transform.TransformDirection(rope.transform.forward) * hookShotRange);
            lineAim.SetPosition(1, hit.point);
        }
        if(Input.GetKeyUp(KeyCode.Space) && triggerHook && shootTimer <= 0 && (hostPlayer.photonView != null && hostPlayer.photonView.IsMine)) {
            isShootingHook = true;
            triggerHook = false;
            shootTimer = 0.1f;
        }

        if(shootTimer > 0) shootTimer += Time.deltaTime;
        if(shootTimer > 1) didntCatch = true;

        if(IsShooting()) {
            if(rope.sizeDelta.y + hookShotSpeed < hookShotRange * 1000f && !didntCatch) {
                if(!hitObject) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y + hookShotSpeed);
                else if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - hookShotSpeed);
                else ResetHook();
            }

            if(didntCatch) {
                if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - hookShotSpeed);
                else ResetHook();
            }
        }
    }

    protected void ResetHook() {
        triggerHook = hitObject = isShootingHook = false;
        rope.sizeDelta = new Vector2(rope.sizeDelta.x, 0);
        shootTimer = 0;
        if(grabbedObj != null) hostPlayer.AddAsteroid(grabbedObj);
        grabbedObj = null; 
        didntCatch = false;
    }

    public void CatchObject(GameObject obj) {
        hitObject = true;
        grabbedObj = obj;
        obj.GetComponent<PhotonView>().TransferOwnership(hostPlayer.photonView.Controller.ActorNumber);
    }

    public bool IsShooting() {
        return isShootingHook;
    }
}
