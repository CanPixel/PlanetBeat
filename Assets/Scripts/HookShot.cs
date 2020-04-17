using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HookShot : MonoBehaviour {
    public PlayerShip hostPlayer;
    private RectTransform rope;
    private CircleCollider2D tip;

    public Collider2D[] playerColliders;

//    [HideInInspector] public CustomController customController;
    private int hengelData;

    [Header("PHYSICS")]
    [Range(20, 400)]
    public float hookShotSpeed = 50;
    [Range(1, 10)]
    public float hookShotRange = 5;

    private bool isShootingHook = false, triggerHook = false;

    private float shootTimer = 0;
    private bool hitObject = false, didntCatch = false;
    private GameObject grabbedObj;

    private bool reelback = false;

    private GameObject lockOnAimTarget;

    void Start() {
        rope = transform.GetChild(0).GetComponent<RectTransform>();
        tip = rope.transform.GetChild(0).GetComponent<CircleCollider2D>();
    }

    void Update() {
        if(grabbedObj != null) grabbedObj.transform.position = tip.transform.position;

        float hookScale = Mathf.Lerp(rope.transform.localScale.x, (IsShooting() ? 1 : 0.1f), Time.deltaTime * 2f);
        rope.transform.localScale = new Vector3(hookScale, hookScale, hookScale);

        //Spelen op custom controls
    /*     if (customController != null) {
            var newData = customController.GetData();
            if (hengelData < newData) ReelOut(newData);
            else if (hengelData > newData) ReelIn(newData);

            if(IsShooting()) {
                if(rope.sizeDelta.y + hookShotSpeed < hookShotRange * 1000f && !didntCatch) {
                    if(!hitObject) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y + hookShotSpeed);
                    else if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - hookShotSpeed);
                }

                if(didntCatch) {
                }
            }
        }*/

        FreeAim();
        if(shootTimer > 0) shootTimer += Time.deltaTime;
        if(shootTimer > 1) didntCatch = true;
    }

    #region AIMING_TYPES_LOGIC
        public void FireLockOn(GameObject target) {
            lockOnAimTarget = target;
            if(hostPlayer.IsThisClient()) hostPlayer.photonView.RPC("CastHook", RpcTarget.All, hostPlayer.photonView.ViewID);
        }

        protected void FreeAim() {
            if(Input.GetKey(KeyCode.Space)) triggerHook = true;
            if(Input.GetKeyUp(KeyCode.Space) && triggerHook && shootTimer <= 0) {
                if(hostPlayer.IsThisClient()) hostPlayer.photonView.RPC("CastHook", RpcTarget.All, hostPlayer.photonView.ViewID);
            }

            if(IsShooting()) {
                if(rope.sizeDelta.y + hookShotSpeed < hookShotRange * 1000f && !didntCatch) {
                    if(!hitObject) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y + hookShotSpeed);
                    else if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - hookShotSpeed);
                    else ResetHook();
                }

                if(didntCatch) {
                    if(!reelback) {
                        AudioManager.PLAY_SOUND("reel");
                        reelback = true;
                    }

                    if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - hookShotSpeed);
                    else ResetHook();
                }
            }
        }
    #endregion

    #region CUSTOM_INTERACTION_CONTROLLER
      /*   protected void ReelIn(int newData) {
            hengelData = newData;
        }

        protected void ReelOut(int newData) {
            if (Mathf.Abs(newData - hengelData) > customController.sensitivity && shootTimer <= 0) {
                triggerHook = true;
                if (hostPlayer.IsThisClient()) hostPlayer.photonView.RPC("CastHook", RpcTarget.All, hostPlayer.photonView.ViewID);
                else if (hostPlayer.isSinglePlayer) CastHook();
            }
            hengelData = newData;
        } */
    #endregion

    public void CastHook() {
        if(!GameManager.GAME_STARTED) return;
        AudioManager.PLAY_SOUND("Kick", 1f, 0.9f);
        AudioManager.PLAY_SOUND("CastHook", 0.8f, Random.Range(0.9f, 1f));
        isShootingHook = true;
        triggerHook = false;
        shootTimer = 0.1f;
    }

    protected void ResetHook() {
        reelback = false;
        lockOnAimTarget = null;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
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
        var photon = obj.GetComponent<PhotonView>();
        if(photon != null && hostPlayer.photonView != null) photon.TransferOwnership(hostPlayer.photonView.Controller.ActorNumber);
    }

    public bool canHold() {
        return grabbedObj == null;
    }

    public bool IsShooting() {
        return isShootingHook;
    }
}
