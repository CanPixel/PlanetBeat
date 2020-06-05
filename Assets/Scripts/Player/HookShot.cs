﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class HookShot : MonoBehaviour {
    public PlayerShip hostPlayer;
    private RectTransform rope;
    private CircleCollider2D tip;
    public GameObject aimPos;
    public Animator animateHand;
    public Animator animateHandUI;
    public SkinnedMeshRenderer handRenderer;
    public ParticleSystem handParticles;

    public Collider2D[] playerColliders;

    private int hengelData;

    [Header("PHYSICS")]
    public float hookShotCastSpeed = 50;
    public float HookShotReelSpeed = 50;

    public float hookShotRange = 5;

    private bool isShootingHook = false, triggerHook = false;
    private float shootTimer = 0;
    private bool hitObject = false, didntCatch = false;
    private GameObject grabbedObj;

    private bool reelback = false;

    private bool HookCooldown = false;
    public float HookCooldownTime = 10.0f;
    private float HookCooldownDelay;

    public Color on, off;
    public Image HookCooldownParent;
    public Image HookCooldownIcon;

    private Vector3 baseHandScale;
    private bool scaleTo = false, scaleBack = false;

    private float shootDelay = 0;
    private bool grabbable = false;

    void Start() {
        baseHandScale = animateHand.transform.localScale;
        rope = transform.GetChild(0).GetComponent<RectTransform>();
        tip = rope.transform.GetChild(0).GetComponent<CircleCollider2D>();
        if(!hostPlayer.photonView.IsMine) HookCooldownParent.gameObject.SetActive(false);

        var em = handParticles.emission;
        em.enabled = false;

        if(GameManager.SkipCountdown()) grabbable = true;
    }

    void Update() {
        //Playercolor Rayman hand + particles 
        handRenderer.material.SetColor("_EmissionColor", hostPlayer.playerColor * 2f);
        var set = handParticles.main;
        set.startColor = new ParticleSystem.MinMaxGradient(hostPlayer.playerColor * 2f);

        if(shootDelay > 0) shootDelay -= Time.deltaTime;
        if(grabbedObj != null) grabbedObj.transform.position = tip.transform.position;

        float hookScale = Mathf.Lerp(rope.transform.localScale.x, (IsShooting() ? 1 : 0.1f), Time.deltaTime * 8f);
        rope.transform.localScale = new Vector3(hookScale, hookScale, hookScale);

        HookCooldownParent.transform.position = Vector3.Lerp(HookCooldownParent.transform.position, aimPos.transform.position, Time.deltaTime * 2f);

        FreeAim();
        if(shootTimer > 0) shootTimer += Time.deltaTime;
        if(shootTimer > 1) didntCatch = true;
        
        //Cooldown
        if (HookCooldown) {
            HookCooldownDelay -= Time.deltaTime;
            if(HookCooldownDelay < 0) HookCooldown = false;
        }

        // animation
        if(grabbable) {
            if (HookCooldown) animateHandUI.SetInteger("handAnimation", 2);
            else animateHandUI.SetInteger("handAnimation", 1);
        } else animateHandUI.SetInteger("handAnimation", 2);
    }

    public void ActivateGrapple() {
        grabbable = true;
    }

    protected void FreeAim() {
        if(Input.GetKey(KeyCode.Space) && !HookCooldown && !HasObject()) {
            triggerHook = true;
            HookCooldown = true;
            HookCooldownDelay = HookCooldownTime;
        }
        if(Input.GetKeyUp(KeyCode.Space) && triggerHook && shootTimer <= 0 && grabbable) {
            if(hostPlayer.IsThisClient()) hostPlayer.photonView.RPC("CastHook", RpcTarget.All, hostPlayer.photonView.ViewID);
        }

        if(IsShooting()) {
            if(rope.sizeDelta.y < hookShotRange * 10f && !didntCatch) {
                if(!hitObject) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y + hookShotCastSpeed);
                else if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - HookShotReelSpeed);
            }

            if(didntCatch) {
                if(!reelback) {
                    SoundManager.PLAY_SOUND("ReelIn");
                    reelback = true;
                }

                if(rope.sizeDelta.y > 0) rope.sizeDelta = new Vector2(rope.sizeDelta.x, rope.sizeDelta.y - HookShotReelSpeed);
                else ResetHook();
            }
        }
    }

    public void DelayHook() {
        shootTimer = 0.1f;
    }

    public void CastHook() {
        if(!hostPlayer.CanCastHook() || IsDelayingHook()) return;

        if(GameManager.GAME_STARTED || (!GameManager.GAME_STARTED && hostPlayer.photonView.IsMine)) SoundManager.PLAY_SOUND("ReelOut");

        isShootingHook = true;
        triggerHook = false;
        animateHand.SetBool("Travel", true);
        DelayHook();
        scaleTo = true;
        var em = handParticles.emission;
        em.enabled = true;
    }

    protected void ResetHook() {
        reelback = false;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        triggerHook = hitObject = isShootingHook = false;
        rope.sizeDelta = new Vector2(rope.sizeDelta.x, 0);
        shootTimer = 0;
        if(grabbedObj != null) hostPlayer.AddAsteroid(grabbedObj);
        grabbedObj = null; 
        didntCatch = false;
        animateHand.SetBool("Travel", false);
        animateHand.SetBool("Fetch", false);
        var em = handParticles.emission;
        em.enabled = false;
    }

    public void CatchObject(GameObject obj) {
        hitObject = true;
        grabbedObj = obj;
        var photon = obj.GetComponent<PhotonView>();
        if(photon != null && hostPlayer.photonView != null) photon.TransferOwnership(hostPlayer.photonView.Controller.ActorNumber);
        animateHand.SetBool("Fetch", true);

        //if(obj.tag == "InfectroidTutorial") hostPlayer.playerTutorial.tutorialStepsByName["Infectroid"].completed = true;

        scaleBack = true;
    }

    public bool HasObject() {
        return grabbedObj != null;
    }

    public bool CanHold() {
        return grabbedObj == null && hostPlayer.CanHold();
    }

    public bool IsShooting() {
        return isShootingHook;
    }

    public bool IsDelayingHook() {
        return shootDelay > 0;
    }

    public void DelayShoot() {
        shootDelay = 0.5f;
    }
}
