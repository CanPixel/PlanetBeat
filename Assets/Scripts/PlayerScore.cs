using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class PlayerScore : MonoBehaviour
{

    public float currentScore;

    public float maxScore = 100f;

    public float minScore;

    public Text scoreText;

    PlayerPlanets _playerPlanets;

    public bool weakestPlanet; 

    //public GameObject playerPlanet;

    //Asteroid _asteroid; 

    void Start()
    {
        currentScore = minScore = 0;
        var go = GameObject.Find("GAME FIELD");
        _playerPlanets = go.GetComponent<PlayerPlanets>(); 
    }

    // Update is called once per frame
    void Update()
    {
        //AddingResource(); 
        scoreText.text = currentScore.ToString("F0");
        CheckForRank(); 
    }

    
    public void AddingResource(float amount)
    {
        if(currentScore < maxScore)
            currentScore += amount;  
        
        if(currentScore <= minScore)
        {
            currentScore = minScore; 
        }
    }

    void CheckForRank()
    {
        if(currentScore == _playerPlanets.lowestValue)
        {
            //you are the weakest planet 
            weakestPlanet = true; 

        }
    }
}


//a check for all player planets 
//Getcomponent their playerscore script
//In this script check the score of the player
// With this information we can select the weakest player at the end of the doomsday event 