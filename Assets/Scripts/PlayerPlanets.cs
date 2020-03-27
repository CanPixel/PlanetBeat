﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPlanets : MonoBehaviour {
    public GameObject player;
    [HideInInspector] public float playerNumber;
    [HideInInspector] public float currentScore;
    public float maxScore = 100f;
    [HideInInspector] public float minScore;
    public Text scoreText;
    private Color orbitColor;
    public TrailRenderer orbitTrail; 
    PlayerPlanets _playerPlanets;

    void Start() {
        if(player == null) {
            scoreText.enabled = false;
            return;
        }
        playerNumber = player.GetComponent<PlayerShip>().playerNumber;
        scoreText = GetComponentInChildren<Text>();
        var _player = player.GetComponent<PlayerShip>();
        orbitColor = _player.playerColor;
        var col = Color.white - _player.playerColor;
        scoreText.color = new Color(col.r, col.g, col.b, 1); 
        orbitTrail.material.color = orbitColor; 
        currentScore = minScore = 0;
    }

    void Update() {
        if(scoreText != null) scoreText.text = currentScore.ToString("F0");
    }

    public void AddingResource(float amount) {
        if (currentScore < maxScore) currentScore += amount;
        if (currentScore <= minScore) currentScore = minScore;
    }
}