using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private Rigidbody2D rb;

    public bool held = false;

    public bool inOrbit = false;


    public float orbitSpeed;

    public float value;

    public float weight;

    float throwAirTime; 


    public float orbitDuration;

     public float orbitTimer;

    public float forceApplied;

    

    public GameObject tempPlanet;

    PlayerScore _playerscore;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        OrbitAroundPlanet(); //Function orbits an astroid around a player planet 
        AsteroidIsThrown(); 
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "HOOKSHOT")
        {
            transform.position = col.transform.position;
            //rb.simulated = false;
            col.gameObject.GetComponent<HookTip>().hookShot.CatchObject(gameObject);
            //Debug.Log("grabbed");
            held = true;
        }

        //if the player just flies the resource straight in the planet instantly give him the points 
        if (col.gameObject.tag == "PLAYERPLANET" && col.gameObject != null)
        {
            _playerscore = col.gameObject.GetComponent<PlayerScore>();
            _playerscore.AddingResource(value);
            gameObject.SetActive(false);
            held = false;
        }
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        //if the player throws the rock to his planet instead of flying there let it orbit for a bit          
            if (col.gameObject.tag == "PLAYERPLANET")          
                _playerscore = col.gameObject.GetComponent<PlayerScore>();
                tempPlanet = col.gameObject; 
    }

    void OrbitAroundPlanet()
    {
        if (!inOrbit)
            orbitTimer = 0;      

        if (!held)
        {
            if (inOrbit)
            {
                transform.RotateAround(tempPlanet.transform.position, Vector3.forward, orbitSpeed * Time.deltaTime);  //How to find the planet has to be reworked! but it rotates the astroid around the players planet                                                                                                                       //start a timer 
                orbitTimer += Time.deltaTime;

                if (orbitTimer >= orbitDuration)  //if the astroid has finished his time in the orbit collect the points 
                {
                    _playerscore.AddingResource(value);
                    gameObject.SetActive(false);
                    inOrbit = false;
                    held = false; 
                    //orbitTimer = 0;
                }
                //lerp resource slowly closer to the planet?
                Debug.Log("rotate around planet");
            }                      
        }
    }

    void AsteroidIsThrown() 
    {
        if (held)
            if (Input.GetKeyDown(KeyCode.F))
            {
                //var normilazedVelocity = rb.velocity.normalized;
                //rb.AddForce(normilazedVelocity * 20f);
                //rb.AddForce(transform.right * 60);

                rb.AddForce(rb.velocity.normalized * Time.deltaTime * forceApplied);
                held = false;
            }
    }
}
