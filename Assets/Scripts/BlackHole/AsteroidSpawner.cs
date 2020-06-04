using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class AsteroidSpawner : MonoBehaviourPun {
    public Animator animator;
    public GameObject asteroid;
    public GameObject powerup;
    public GameObject remoteroid;
    public int asteroidAmount = 4, powerupAmount = 2;

    public GameObject blackHole;
    private GameObject[] AsteroidsList, PowerupsList;

    private List<GameObject> spawnQueue = new List<GameObject>();

    public Vector2Int objectSpawnDelay = new Vector2Int(2, 7), powerupSpawnDelays = new Vector2Int(6, 8);
    private float asteroidSpawnDelay, powerupSpawnDelay, asteroidSpawnTimer = 0, powerupSpawnTimer = 0;
    
    //Delay after black hole opens, for when asteroid actually spawns
    private float spawnAnimationDelay = 1.5f;

    private BlackHoleEffect blackHoleEffect;
    private float baseRadius;
    private bool openBlackHole = false, shake = false, enableRemoteroid = false, enableInfectroid = false;

    private ScreenShake mainCamScreenShake;
    private int sample = 0;

    [FMODUnity.ParamRef]
    private float cutoff = 0;
    private float cutoffSpeed = 2f;

    void Start() {
        Random.InitState(System.DateTime.Now.Millisecond);
        mainCamScreenShake = Camera.main.GetComponent<ScreenShake>();
        asteroidSpawnDelay = Random.Range(objectSpawnDelay.x, objectSpawnDelay.y);
        powerupSpawnDelay = Random.Range(powerupSpawnDelays.x, powerupSpawnDelays.y);
        blackHoleEffect = Camera.main.GetComponent<BlackHoleEffect>();
        baseRadius = blackHoleEffect.radius;
        blackHoleEffect.radius = 0;
    }

    [PunRPC]
    public void SynchRadius(float radius, float asteroidSpawnTimer, float powerupSpawnTimer) {
        if(PhotonNetwork.IsMasterClient) return;
        blackHoleEffect.radius = radius;
        this.asteroidSpawnTimer = asteroidSpawnTimer;
        this.powerupSpawnTimer = powerupSpawnTimer;
    }

    void Update() {
        Random.InitState(System.DateTime.Now.Millisecond);

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BlackHole", cutoff);

        if(!GameManager.GAME_STARTED) return;
        if(PhotonNetwork.IsMasterClient) {
            if(openBlackHole) blackHoleEffect.radius = Mathf.Lerp(blackHoleEffect.radius, baseRadius * 1.5f + Mathf.Sin(Time.time * 15f) * 1f, Time.deltaTime * 2f);
            else blackHoleEffect.radius = Mathf.Lerp(blackHoleEffect.radius, 0, Time.deltaTime * 1f);
            photonView.RPC("SynchRadius", RpcTarget.All, blackHoleEffect.radius, asteroidSpawnTimer, powerupSpawnTimer);
        }
        if(asteroidSpawnTimer > asteroidSpawnDelay + (spawnAnimationDelay / 2f) && !shake) {
            mainCamScreenShake.Shake(0.6f);
            photonView.RPC("ShakeScreenNetwork", RpcTarget.All);
            shake = true;
        }

        if(PhotonNetwork.IsMasterClient) {
            if(!animator.GetBool("Closing")) {
                PowerupsList = GameObject.FindGameObjectsWithTag("Powerup");
                if(PowerupsList.Length < powerupAmount && enableInfectroid) {
                    powerupSpawnTimer += Time.deltaTime;
                    if(powerupSpawnTimer > powerupSpawnDelay) {
                        openBlackHole = true;
                        //animator.SetBool("Opening", true);
                        cutoff = Mathf.Lerp(cutoff, 1, Time.deltaTime * cutoffSpeed);
                    }
                    if(powerupSpawnTimer > powerupSpawnDelay + spawnAnimationDelay) {
                        SpawnPowerup();
                        powerupSpawnDelay = Random.Range(powerupSpawnDelays.x, powerupSpawnDelays.y);
                        powerupSpawnTimer = 0;
                        openBlackHole = shake = false;
                        //animator.SetBool("Closing", true);
                        //animator.SetBool("Opening", false);
                        cutoff = 0;
                    }
                }

                AsteroidsList = GameObject.FindGameObjectsWithTag("Resource");
                if(AsteroidsList.Length < asteroidAmount) {
                    asteroidSpawnTimer += Time.deltaTime;
                    if(asteroidSpawnTimer > asteroidSpawnDelay) {
                        openBlackHole = true;
                        //animator.SetBool("Opening", true);
                        cutoff = Mathf.Lerp(cutoff, 1, Time.deltaTime * cutoffSpeed);
                    }
                    if(asteroidSpawnTimer > asteroidSpawnDelay + spawnAnimationDelay) {
                        SpawnResource();
                        asteroidSpawnDelay = Random.Range(objectSpawnDelay.x, objectSpawnDelay.y);
                        asteroidSpawnTimer = 0;
                        openBlackHole = shake = false;
                        //animator.SetBool("Closing", true);
                        //animator.SetBool("Opening", false);
                        cutoff = 0;
                    }
                }
            }
        }
    }

    [PunRPC]
    protected void ShakeScreenNetwork() {
        if(PhotonNetwork.IsMasterClient) return;
        mainCamScreenShake.Shake(1f);
        shake = true;
    }

    public void EnableInfectroidPowerup() {
        enableInfectroid = true;
    }
    public void EnableRemoteroid() {
        enableRemoteroid = true;
    }

    protected void SpawnPowerup() {
        Vector3 center = transform.position;
        Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);

        var power = powerup;
        if(enableRemoteroid) power = Random.Range(0, 2) == 1? remoteroid : powerup;
        GameObject InstancedPrefab = GameManager.SPAWN_SERVER_OBJECT(power, new Vector3(blackHole.transform.position.x, blackHole.transform.position.y, -1.1f), rot);

        SoundManager.PLAY_SOUND("InfectroidSpawn");
    }
    protected void SpawnResource() {
        Vector3 center = transform.position;
        Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);
        GameObject InstancedPrefab = GameManager.SPAWN_SERVER_OBJECT(asteroid, new Vector3(blackHole.transform.position.x, blackHole.transform.position.y, -1.1f), rot);

        SoundManager.PLAY_SOUND("ResourceSpawn");
    }

    private Vector3 RandomCircle(Vector3 center, float radius, int ang) {
        return new Vector3(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad), center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad), center.z);
    }
}
