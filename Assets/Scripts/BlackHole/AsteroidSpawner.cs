using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class AsteroidSpawner : MonoBehaviourPun {
    public Animator animator;
    public GameObject asteroid;
    public GameObject powerup;
    public int asteroidAmount = 4, powerupAmount = 2;

    public GameObject blackHole;
    private GameObject[] AsteroidsList, PowerupsList;

    public Vector2Int objectSpawnDelay = new Vector2Int(2, 7), powerupSpawnDelays = new Vector2Int(6, 8);
    private float asteroidSpawnDelay, powerupSpawnDelay, asteroidSpawnTimer = 0, powerupSpawnTimer = 0;
    
    //Delay after black hole opens, for when asteroid actually spawns
    private float spawnAnimationDelay = 1.5f;
    private bool shake = false, enableInfectroid = false;

    private ScreenShake mainCamScreenShake;
    private int sample = 0;

    private int blackHoleInt = 0;

    [FMODUnity.ParamRef]
    private float cutoff = 0;
    private float targetCutoff = 0;
    private float cutoffSpeed = 2f;

    void Start() {
        Random.InitState(System.DateTime.Now.Millisecond);
        mainCamScreenShake = Camera.main.GetComponent<ScreenShake>();
        asteroidSpawnDelay = Random.Range(objectSpawnDelay.x, objectSpawnDelay.y);
        powerupSpawnDelay = Random.Range(powerupSpawnDelays.x, powerupSpawnDelays.y);
    }

    [PunRPC]
    public void SynchBlackHole(int blackHoleAnim, float cutoff) {
        animator.SetInteger("blackHoleAnimation", blackHoleAnim);
        this.cutoff = cutoff;
    }

    void Update() {
        Random.InitState(System.DateTime.Now.Millisecond);
        //FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BlackHole", cutoff);

        cutoff = Mathf.Lerp(cutoff, targetCutoff, Time.deltaTime * cutoffSpeed);
        if(!GameManager.GAME_STARTED) return;

        if(shake) {
            photonView.RPC("ShakeScreenNetwork", RpcTarget.All);
            shake = false;
        }

        if(PhotonNetwork.IsMasterClient) {
            PowerupsList = GameObject.FindGameObjectsWithTag("Powerup");
            if(PowerupsList.Length < powerupAmount && enableInfectroid) powerupSpawnTimer += Time.deltaTime;

            AsteroidsList = GameObject.FindGameObjectsWithTag("Resource");
            if(AsteroidsList.Length < asteroidAmount) asteroidSpawnTimer += Time.deltaTime;
            photonView.RPC("SynchBlackHole", RpcTarget.Others, blackHoleInt, cutoff);
        }
    }

    [PunRPC]
    public void OpenBlackHoleSound() {
        SoundManager.PLAY_SOUND("BlackHoleOpen");
    }

    [PunRPC]
    public void SpawnSound(string obj) {
        SoundManager.PLAY_SOUND(obj + "Spawn");
    }

    public void AnticipateSpawn() {
        if(asteroidSpawnTimer > asteroidSpawnDelay) {
            photonView.RPC("OpenBlackHoleSound", RpcTarget.All);
            targetCutoff = 1;
            animator.SetInteger("blackHoleAnimation", 1);
            blackHoleInt = 1;
        }

        if(powerupSpawnTimer > powerupSpawnDelay) {
            photonView.RPC("OpenBlackHoleSound", RpcTarget.All);
            targetCutoff = 1;
            animator.SetInteger("blackHoleAnimation", 1);
            blackHoleInt = 1;
        }
    }

    public void SetSpawnOnBeat() {
        if(asteroidSpawnTimer > asteroidSpawnDelay + spawnAnimationDelay) SpawnResource();
        if(powerupSpawnTimer > powerupSpawnDelay + spawnAnimationDelay) SpawnPowerup();
    }

    [PunRPC]
    protected void ShakeScreenNetwork() {
        mainCamScreenShake.Shake(0.6f);
        mainCamScreenShake.Turn(0.75f);
    }

    public void EnableInfectroidPowerup() {
        enableInfectroid = true;
    }

    protected void SpawnPowerup() {
        Vector3 center = transform.position;
        Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);
        GameManager.SPAWN_SERVER_OBJECT(powerup, new Vector3(blackHole.transform.position.x, blackHole.transform.position.y, -1.1f), rot);
        //SoundManager.PLAY_SOUND("InfectroidSpawn");
        photonView.RPC("SpawnSound", RpcTarget.All, "Infectroid");

        cutoff = targetCutoff = 0;
        blackHoleInt = 2;
        shake = true;
        animator.SetInteger("blackHoleAnimation", 2);

        powerupSpawnDelay = Random.Range(powerupSpawnDelays.x, powerupSpawnDelays.y);
        powerupSpawnTimer = 0;
    }
    protected void SpawnResource() {
        Vector3 center = transform.position;
        Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);
        GameManager.SPAWN_SERVER_OBJECT(asteroid, new Vector3(blackHole.transform.position.x, blackHole.transform.position.y, -1.1f), rot);
        //SoundManager.PLAY_SOUND("ResourceSpawn");
        photonView.RPC("SpawnSound", RpcTarget.All, "Resource");

        cutoff = targetCutoff = 0;
        blackHoleInt = 2;
        shake = true;
        animator.SetInteger("blackHoleAnimation", 2);

        asteroidSpawnDelay = Random.Range(objectSpawnDelay.x, objectSpawnDelay.y);
        asteroidSpawnTimer = 0;
    }

    private Vector3 RandomCircle(Vector3 center, float radius, int ang) {
        return new Vector3(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad), center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad), center.z);
    }
}
