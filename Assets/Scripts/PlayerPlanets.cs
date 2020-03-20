using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanets : MonoBehaviour
{
    //a check for all player planets 
    //Getcomponent their playerscore script
    //In this script check the score of the player
    // With this information we can select the weakest player at the end of the doomsday event 

    //public List playerPlanets[]; 

    public GameObject playerPlanet1;
    public GameObject playerPlanet2;

    public GameObject lowestPlayer; 

    public bool blackHole = false;
    // public GameObject playerPlanet3;

    public List<GameObject> playerPlanets = new List<GameObject>();

    GameObject planet1;

    PlayerScore playerScore1, playerScore2, playerScore3, playerScore4;
    


    public float lowestValue; 

    void Start()
    {

      
        playerScore1 = playerPlanet1.GetComponent<PlayerScore>();
        playerScore2 = playerPlanet2.GetComponent<PlayerScore>(); 

 

        //collect all the planets in a list 
        //lowestValue = Mathf.Min(planet3.currentScore, 2, 3);
    }

    void Update()
    {
        LowestPlanetCheck();
        lowestPlayer = playerScore1.GetComponent<GameObject>();



    }

    public void LowestPlanetCheck()
    {
        if (blackHole == true)
        {
            lowestValue = Mathf.Min(playerScore1.currentScore, playerScore2.currentScore);       
        }
    }

    //we'll make a function that checks this which gets enabled in the sun script
}
