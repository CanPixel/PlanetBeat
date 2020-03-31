using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerPlanets : MonoBehaviourPun {
    private PlayerShip player;
    public int playerNumber;
    [HideInInspector] public float currentScore;
    public float maxScore = 100f;
    [HideInInspector] public float minScore;
    public Text scoreText;
    private Color orbitColor;
    public TrailRenderer orbitTrail; 

    public bool HasPlayer() {
        return player != null;
    }

    void Start() {
        currentScore = minScore = 0;
        if(player == null) {
            scoreText.enabled = false;
            return;
        }
    }

    public void SetColor(Color col) {
        orbitColor = orbitTrail.material.color = scoreText.color = col;
    }

    [PunRPC]
    public void ClaimPlayer(int playerNumbe, float r, float g, float b) {
        playerNumber = playerNumbe;
        player = PhotonNetwork.GetPhotonView(playerNumber).GetComponent<PlayerShip>();
        var col = new Color(r, g, b);
        player.playerColor = col;
        player.SetPlayerNameColor(col);
        orbitColor = player.playerColor;
        scoreText.color = player.playerColor;
        scoreText.enabled = true;
        orbitTrail.material.color = orbitColor; 
    }

    public void AssignPlayer(PlayerShip player) {
        this.player = player;
        playerNumber = player.playerNumber;
        scoreText = GetComponentInChildren<Text>();
        photonView.RPC("ClaimPlayer", RpcTarget.AllBuffered, playerNumber, player.playerColor.r, player.playerColor.g, player.playerColor.b);
        scoreText.enabled = true;
    }

    void Update() {
        if(scoreText != null) {
            scoreText.text = currentScore.ToString("F0");
            if(player != null) {
                orbitColor = player.playerColor;
                scoreText.color = orbitColor;
                orbitTrail.material.color = orbitColor; 
            }
        }
    }

    [PunRPC]
    public void SetResource(float i) {
        float amount = Mathf.Clamp(i, 0, maxScore);
        currentScore = amount;
        var newScale = transform.localScale + new Vector3(amount, amount, 0) / 50f;
        GetComponent<UIFloat>().SetBaseScale(newScale);
    }

    public void AddingResource(float amount) {
        if (currentScore < maxScore) {
            currentScore += amount;
            var newScale = transform.localScale + new Vector3(amount, amount, 0) / 50f;
            GetComponent<UIFloat>().SetBaseScale(newScale);
            if(photonView != null) photonView.RPC("SetResource", RpcTarget.AllBufferedViaServer, currentScore + amount);
        }
        if (currentScore <= minScore) currentScore = minScore;

        AudioManager.PLAY_SOUND("collect", 1, 1.2f);
    }
}