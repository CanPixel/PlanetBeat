using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTagsManager : MonoBehaviour { 
    [HideInInspector] public float tagNum;
    [HideInInspector] public float tagTimer = 0;
    public float tagDuration;
    [HideInInspector] public bool runTagTimer = false ; 
    private TrailRenderer asteroidTrailRenderer;
    public Color ogTrailColor; 

    private Asteroid _asteroid; 
    private Collider2D asteroidColl;

    private PlayerShip tagPlayer;

    private Image src, glow;

    void Start() {
        asteroidColl = GetComponent<Collider2D>();
        _asteroid = GetComponent<Asteroid>();
        //src = _asteroid.src;
        //glow = _asteroid.glow;
        asteroidTrailRenderer = GetComponent<TrailRenderer>();
        asteroidTrailRenderer.material.color = ogTrailColor; 
        
        DisableTrails();
    }

    public void DisableTrails() {
        asteroidTrailRenderer.enabled = false;
    }

    void Update() {
        StartTagTimer(); 

       if (tagTimer >= tagDuration / 2f && tagPlayer != null) tagPlayer.SetCollision(asteroidColl, true);
    }

    public void GiveTag() {
        tagNum = _asteroid.ownerPlayer.playerNumber;
        asteroidTrailRenderer.material.color = _asteroid.ownerPlayer.playerColor;
      //  TagOn(false);
        if(_asteroid != null && _asteroid.ownerPlayer != null && src != null && glow != null) src.color = glow.color = _asteroid.ownerPlayer.playerColor * 1.7f;
    }

    public void RemoveTag() {
        if(tagPlayer != null) {
            tagPlayer.SetCollision(asteroidColl, true);
            tagPlayer = null;
        }
        tagNum = 0;
        asteroidTrailRenderer.material.color = ogTrailColor;
        
        src.color = glow.color = Color.white;
        if(_asteroid != null) _asteroid.ForceRelease(true);
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
}