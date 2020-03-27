using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Asteroid : MonoBehaviour {
    [HideInInspector] public Rigidbody2D rb;
    public Image src, glow;

    [HideInInspector] public bool held = false;
    public bool inOrbit = false;
    [HideInInspector] public bool giveTag = false;
    [HideInInspector] public float inOrbitTimer;

    private bool canConsume = false;
    private float defaultRbDrag;
    public float inPlayerOrbitRbDrag = 0.25f;

    public float value = 5;
    public float grabDelay = .5f; 
    public float maxInOrbitTime = 5;
    public float outOrbitForce = 20;
    private float collectTimer;
    private Collider2D asteroidColl;
    [HideInInspector] public PlayerPlanets playerPlanets;

    [HideInInspector] public PlayerShip playerShip;
    [HideInInspector] public PlayerTagsManager playerTagsManager;

    [HideInInspector] public GameObject playerOrbit;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        asteroidColl = GetComponent<Collider2D>();
        playerTagsManager = GetComponent<PlayerTagsManager>();
        rb.drag = defaultRbDrag - .15f;
        SetTexture(TextureSwitcher.GetCurrentTexturePack());
    }

    void Update() {
        if (collectTimer > 0) collectTimer -= Time.deltaTime;
        asteroidColl.enabled = collectTimer <= 0f; 

        if(held) ReleaseAsteroid(false);
    }

    void OnCollisionEnter2D(Collision2D col) {
        //Hook touches a object
        if (col.gameObject.tag == "HOOKSHOT" && !held && col.gameObject.GetComponent<HookTip>().hookShot.canHold()) {
            transform.position = col.transform.position;
            var hookShot = col.gameObject.GetComponent<HookTip>().hookShot;
            playerShip = hookShot.hostPlayer;
            FetchAsteroid(hookShot.hostPlayer);
            hookShot.CatchObject(gameObject);
            collectTimer = grabDelay; 
            playerTagsManager.GiveTag();
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.gameObject.tag == "PLAYERPLANET" && col.gameObject != null) {
            playerPlanets = col.gameObject.GetComponent<PlayerPlanets>();
            if(playerTagsManager.tagNum == playerPlanets.playerNumber) {
                if(canConsume || held) ConsumeResource();
            }
        }
        if(col.gameObject.tag == "ORBIT") {
            inOrbit = true;
            if(!held) OrbitAroundPlanet();
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "ORBIT") {
            inOrbit = false;
            OrbitAroundPlanet();
        }     
    }

    public void SetTexture(TextureSwitcher.TexturePack elm) {
        src.sprite = elm.asteroid.src;
        if(elm.asteroid.glow == null) {
            glow.enabled = false;
            return;
        }
        glow.enabled = true;
        glow.sprite = elm.asteroid.glow;
    }

    void OrbitAroundPlanet() {
        if(inOrbit) {
            if(playerTagsManager != null && playerTagsManager.tagNum != 0) {
                if(playerShip.playerNumber == playerTagsManager.tagNum) {
                    inOrbitTimer += Time.deltaTime;
                    rb.drag = inPlayerOrbitRbDrag;

                    if(inOrbitTimer >= maxInOrbitTime) canConsume = true;
                } else {
                    inOrbitTimer = 0;
                    rb.drag = defaultRbDrag;
                }
            } if(playerPlanets != null && playerTagsManager.tagNum != playerPlanets.playerNumber) {
                //Throw the resource out of the orbit
                inOrbitTimer += Time.deltaTime;

                if(inOrbitTimer >= maxInOrbitTime) {
                    rb.AddForce(-transform.right * outOrbitForce);
                    inOrbit = false;
                    inOrbitTimer = 0;
                }
            }
        } else inOrbitTimer = 0;
    }

    public void ConsumeResource() {
        playerPlanets.AddingResource(value);
        gameObject.SetActive(false);
    }

    public void ReleaseAsteroid(bool released) {
        if(released) {
            playerTagsManager.TagOn(true);
            playerTagsManager.runTagTimer = true;
            held = false;
        } else {
            held = true;
            playerTagsManager.runTagTimer = false;
        }
        //owner
    }

    public void FetchAsteroid(PlayerShip own) {
        //if(owner != null) owner.RemoveAsteroid(gameObject);
        held = true;
      //  owner = own;
    }
}
