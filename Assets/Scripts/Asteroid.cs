using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Asteroid : MonoBehaviourPun {
    [HideInInspector]
    public Rigidbody2D rb;

    public bool held = false;

    public bool inOrbit = false;

    public bool isOvertimeBomb = false;

    public bool isInstantBomb = false; 

    public float orbitSpeed;
    public float value;
    public float weight;
    float throwAirTime;
    public float grabDelay = .5f; 


    public float orbitDuration;
     public float orbitTimer;
    public float forceApplied;
    private float collectTimer;
    private PolygonCollider2D asteroidColl;
    public ParticleSystem infectedAstroid;
    GameObject infection;
    public Transform movePoint; 

    public GameObject tempPlanet;
    PlayerScore _playerscore;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        asteroidColl = GetComponent<PolygonCollider2D>();
        //infectedAstroid = Instantiate(infectedAstroid, rb.transform.position, Quaternion.identity);
        //infectedAstroid.Stop(); 
    }

    void Update() {
        //OrbitAroundPlanet(); //Function orbits an astroid around a player planet 
        
        
        if (collectTimer > 0) collectTimer -= Time.deltaTime;
        asteroidColl.enabled = collectTimer <= 0f; 
    }

    void OnCollisionEnter2D(Collision2D col) {
        ///CAN CODE
        if(col.gameObject.tag == "PLAYERSHIP") {
            if(photonView != null) {
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        //Hook touches a object
        if (col.gameObject.tag == "HOOKSHOT" && !held) {
            transform.position = col.transform.position;
            //rb.simulated = false;
            col.gameObject.GetComponent<HookTip>().hookShot.CatchObject(gameObject);
            //Debug.Log("grabbed");
            held = true;
            collectTimer = grabDelay; 
        }

        //Grabbed object touches the home planet 
        if (!isInstantBomb || !isOvertimeBomb) {
            if (col.gameObject.tag == "PLAYERPLANET" && col.gameObject != null && held) {
                _playerscore = col.gameObject.GetComponent<PlayerScore>();
                _playerscore.AddingResource(value);
                gameObject.SetActive(false);
                held = false;
            }
        }
    }


    void OnTriggerEnter2D(Collider2D col) {
        //Thrown object enters the orbit of a player planet        
        if (col.gameObject.tag == "PLAYERPLANET") {
            inOrbit = true; 
            _playerscore = col.gameObject.GetComponent<PlayerScore>();
        }

        tempPlanet = col.gameObject;
        //movePoint = col.gameObject.GetComponentInChildren<GameObject>.

        if (isOvertimeBomb) {
            //infectedAstroid.Play(); 
           //infectedAstroid = Instantiate(infectedAstroid, rb.transform.position, Quaternion.identity); 
           
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "PLAYERPLANET") {
            inOrbit = false;

            if (isOvertimeBomb) {
               // infectedAstroid.Stop(); 
            }
        }     
        

    }

    void OrbitAroundPlanet() {
        if (!inOrbit) orbitTimer = 0;      

        if (!held) {
            if (inOrbit) {
                transform.RotateAround(tempPlanet.transform.position, Vector3.forward, orbitSpeed * Time.deltaTime);  //How to find the planet has to be reworked! but it rotates the astroid around the players planet                                                                                                                       //start a timer 
                orbitTimer += Time.deltaTime;
                rb.velocity = new Vector2(0,0);

             
                //float step = .1f * Time.deltaTime;
               // transform.position = Vector3.MoveTowards(transform.position, movePoint.position, step);

                if (!isOvertimeBomb)
                {
                    if (orbitTimer >= orbitDuration)  //if the astroid has finished his time in the orbit collect the points 
                    {
                        _playerscore.AddingResource(value);
                        gameObject.SetActive(false);
                        inOrbit = false;
                        held = false;
                        //orbitTimer = 0;
                    }
                }

                if (isOvertimeBomb)
                {
                    //infectedAstroid.transform.position = rb.transform.position; 

                    if (orbitTimer >= orbitDuration)
                    {
                        _playerscore.AddingResource(value);
                        orbitTimer = 0;
                    }
                }
                    //lerp resource slowly closer to the planet?
                 //   Debug.Log("rotate around planet");                           
            }                      
        }
    }

    public void ReleaseAsteroid() {
        held = false; 
    }
}
