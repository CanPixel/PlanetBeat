using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTagsManager : MonoBehaviour { 
    [HideInInspector] public float tagNum;
    [HideInInspector] public float tagTimer = 0;
    public float tagDuration;
    [HideInInspector] public bool runTagTimer = false ; 
    private TrailRenderer asteroidTrailRenderer;
    public Color ogTrailColor; 

    private Asteroid _asteroid; 
    private Collider2D asteroidColl;

    void Start() {
        asteroidColl = GetComponent<Collider2D>();
        _asteroid = GetComponent<Asteroid>();
        asteroidTrailRenderer = GetComponent<TrailRenderer>();
        asteroidTrailRenderer.material.color = ogTrailColor; 
    }

    void Update() {
        StartTagTimer(); 
    }

    public void GiveTag() {
        tagNum = _asteroid.ownerPlayer.playerNumber;
        asteroidTrailRenderer.material.color = _asteroid.ownerPlayer.playerColor;
        TagOn(false);
    }

    public void RemoveTag() {
        tagNum = 0;
        asteroidTrailRenderer.material.color = ogTrailColor;
    }

    public void TagOn(bool state) {
        if(asteroidTrailRenderer != null) asteroidTrailRenderer.enabled = state;
    }

    public void StartTagTimer() {
        if (runTagTimer) {
            if (!_asteroid.inOrbit || (_asteroid.inOrbit && _asteroid.ownerPlayer != null && _asteroid.playerPlanets != null && _asteroid.ownerPlayer.playerNumber != _asteroid.playerPlanets.playerNumber)) {
                if (tagTimer < tagDuration) tagTimer += Time.deltaTime;

                if (tagTimer >= tagDuration) {
                    runTagTimer = false;
                    RemoveTag();
                }
            }
        }
        else tagTimer = 0f;
    }

    public void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "PLAYERSHIP") {
            var _playerShip = col.gameObject.GetComponent<PlayerShip>();
            //var hook = col.gameObject.GetComponentsInChildren<PlayerShip>(); 

            if (_playerShip.playerNumber != tagNum) return;
            else Physics2D.IgnoreCollision(col.collider, asteroidColl);
        }
        if(col.gameObject.tag == "HOOKSHOT") {
            var _playerShip = col.gameObject.GetComponentInParent<PlayerShip>();

            if (_playerShip.playerNumber != tagNum) return;
            else Physics2D.IgnoreCollision(col.collider, asteroidColl);
        }
    }
}