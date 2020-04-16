﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class AsteroidSpawner : MonoBehaviour {
    public int asteroidAmount = 4;

    public GameObject blackHole;

    private GameObject[] AsteroidsList;
    public GameObject prefab;

    public Vector2 asteroidSpawnDelay = new Vector2(2, 5);
    private float currentSpawnDelay, asteroidSpawnTimer = 0;
    
    //Delay after black hole opens, for when asteroid actually spawns
    private float spawnAnimationDelay = 1.5f;

    private BlackHoleEffect blackHoleEffect;
    private float baseRadius;
    private bool openBlackHole = false, shake = false;

    private ScreenShake mainCamScreenShake;

    private bool kickOrSnare = false;

    void Start() {
        mainCamScreenShake = Camera.main.GetComponent<ScreenShake>();
        currentSpawnDelay = Random.Range(asteroidSpawnDelay.x, asteroidSpawnDelay.y);
        blackHoleEffect = Camera.main.GetComponent<BlackHoleEffect>();
        baseRadius = blackHoleEffect.radius;
        blackHoleEffect.radius = 0;
    }

    void Update() {
        if(!GameManager.GAME_STARTED) return;

        if(openBlackHole) blackHoleEffect.radius = Mathf.Lerp(blackHoleEffect.radius, baseRadius * 1.5f + Mathf.Sin(Time.time * 15f) * 1f, Time.deltaTime * 2f);
        else blackHoleEffect.radius = Mathf.Lerp(blackHoleEffect.radius, 0, Time.deltaTime * 1f);

        AsteroidsList = GameObject.FindGameObjectsWithTag("Resource");
        if (AsteroidsList.Length < asteroidAmount) {
            asteroidSpawnTimer += Time.deltaTime;
            if(asteroidSpawnTimer > currentSpawnDelay) openBlackHole = true;

            /* if(asteroidSpawnTimer > currentSpawnDelay + spawnAnimationDelay) {
                SpawnAsteroid();
                currentSpawnDelay = Random.Range(asteroidSpawnDelay.x, asteroidSpawnDelay.y);
                asteroidSpawnTimer = 0;
                openBlackHole = shake = false;
            }*/
        }
    }

    public void SpitAsteroidOnBeat() {
        AsteroidsList = GameObject.FindGameObjectsWithTag("Resource");
        if(asteroidSpawnTimer > currentSpawnDelay + (spawnAnimationDelay / 2f) && !shake) {
            mainCamScreenShake.Shake(1f);
            shake = true;
        }

        if (AsteroidsList.Length < asteroidAmount && openBlackHole) {
            if(asteroidSpawnTimer > currentSpawnDelay + spawnAnimationDelay) {
                SpawnAsteroid();
                currentSpawnDelay = Random.Range(asteroidSpawnDelay.x, asteroidSpawnDelay.y);
                asteroidSpawnTimer = 0;
                openBlackHole = shake = false;
            }
        }
    }

    protected void SpawnAsteroid() {
        Vector3 center = transform.position;
        Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);
        GameObject InstancedPrefab = GameManager.SPAWN_SERVER_OBJECT(prefab, blackHole.transform.position, rot);

        AudioManager.PLAY_SOUND("actualBeat", 2f, Random.Range(0.9f, 1.1f));
        if(!kickOrSnare) AudioManager.PLAY_SOUND("DRUMS_KICK", 2f, 1f);
        else AudioManager.PLAY_SOUND("DRUMS_SNARE", 2f, 1f);
        kickOrSnare = !kickOrSnare;
    }

    private Vector3 RandomCircle(Vector3 center, float radius, int a) {
        float ang = a;
        return new Vector3(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad), center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad), center.z);
    }
}
