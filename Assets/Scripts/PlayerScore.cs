using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour {
    public List<GameObject> playerPlanets = new List<GameObject>();

    public GameObject background; 

    void Update() {
        CheckScore();
    }

    public void CheckScore() {
        if (Input.GetKeyDown(KeyCode.L)) {
            foreach (GameObject item in playerPlanets) {
                var ok = item.GetComponent<PlayerPlanets>();
                Debug.Log(ok.currentScore);
                //lowestValue = Mathf.Min(playerScore1.currentScore, playerScore2.currentScore);
            }
        }
    }
}