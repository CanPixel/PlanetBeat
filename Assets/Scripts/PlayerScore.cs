using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class PlayerScore : MonoBehaviour
{

    public float currentScore;


    public float resourceValue; 

    public float maxScore;

    public float minScore;

    public bool resourceAdded;

    public bool resourceInOrbit; 

    public Text scoreText; 

    public GameObject playerPlanet;

    Asteroid _asteroid; 

    void Start()
    {
        currentScore = 0; 
    }

    // Update is called once per frame
    void Update()
    {
        //AddingResource(); 
        scoreText.text = currentScore.ToString("F0");
    }

    
    public void AddingResource(float amount)
    {
        currentScore += amount; 
        //playerPlanet.transform.localScale = new Vector2(2f, 2f );             
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //if the player throws the rock to his planet instead of flying there let it orbit for a bit
            if (col.gameObject.tag == "Resource")
            {
                resourceInOrbit = true;
                _asteroid = col.gameObject.GetComponent<Asteroid>();
                _asteroid.inOrbit = true; 
            }
        }
    }
