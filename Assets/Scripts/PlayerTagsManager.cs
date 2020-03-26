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

    Asteroid _asteroid; 

    void Start() {
        _asteroid = GetComponent<Asteroid>();
        asteroidTrailRenderer = GetComponent<TrailRenderer>();
        asteroidTrailRenderer.material.color = ogTrailColor; 
    }

    void Update() {
        StartTagTimer(); 
    }

    public void GiveTag() {
        tagNum = _asteroid.playerShip.playerNumber;
        asteroidTrailRenderer.material.color = _asteroid.playerShip.playerColor;
        TagOn(false);
    }

    public void RemoveTag() {
        tagNum = 0;
        asteroidTrailRenderer.material.color = ogTrailColor;
    }

    public void TagOn(bool state) {
        if(asteroidTrailRenderer != null)
            asteroidTrailRenderer.enabled = state;
    }

    public void StartTagTimer() {
        if (runTagTimer) {
            if (!_asteroid.inOrbit) {
                if (tagTimer < tagDuration) tagTimer += Time.deltaTime;

                if (tagTimer >= tagDuration) {
                    runTagTimer = false;
                    RemoveTag();
                }
            }
        }
        else tagTimer = 0f;
    }
}