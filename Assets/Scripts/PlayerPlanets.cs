using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPlanets : MonoBehaviour {
    public PlayerShip player;
    [HideInInspector] public float playerNumber;
    [HideInInspector] public float currentScore;
    public float maxScore = 100f;
    [HideInInspector] public float minScore;
    public Text scoreText;
    private Color orbitColor;
    public TrailRenderer orbitTrail; 
    PlayerPlanets _playerPlanets;

    void Start() {
        currentScore = minScore = 0;
        if(player == null) {
            scoreText.enabled = false;
            return;
        }
       CheckForPlayer();
    }

    private void CheckForPlayer() {
        if(player != null) return;

        foreach(var i in GameManager.GetPlayerList()) {
            if(i.homePlanet == null) {
                player = i;
                i.homePlanet = gameObject;
                Debug.Log("Player assigned to planet " + i.homePlanet.name);
                break;
            }
        }
        if(player == null) return;
        player.homePlanet = gameObject;
        playerNumber = player.playerNumber;
        scoreText = GetComponentInChildren<Text>();
        orbitColor = player.playerColor;
        //var col = Color.white - player.playerColor;
        scoreText.color = player.playerColor;//new Color(col.r, col.g, col.b, 1); 
        scoreText.enabled = true;
        orbitTrail.material.color = orbitColor; 
    }

    void Update() {
        if(player == null) CheckForPlayer();
        if(scoreText != null) scoreText.text = currentScore.ToString("F0");
    }

    public void AddingResource(float amount) {
        if (currentScore < maxScore) currentScore += amount;
        if (currentScore <= minScore) currentScore = minScore;

        var newScale = transform.localScale + new Vector3(amount, amount, 0) / 50f;
        GetComponent<UIFloat>().SetBaseScale(newScale);
    }
}