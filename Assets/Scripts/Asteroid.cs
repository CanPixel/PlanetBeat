using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Asteroid : MonoBehaviourPun {
    [HideInInspector] public Rigidbody2D rb;
    public Image src, glow;
    public Text scoreText, increasePopupTxt;

    [HideInInspector] public bool held = false;
    [HideInInspector] public bool inOrbit = false;
    [HideInInspector] public bool giveTag = false;
    [HideInInspector] public float inOrbitTimer;

    public float timeToScore = 0.3f;
    public float grabDelay = .5f; 
    
    [Header("VALUE & WORTH")]
    public Vector2Int baseValue = new Vector2Int(8, 12);
    public Vector2 increaseValueDelay = new Vector2(4, 6);
    private float currentIncreaseDelay = 4;
    public Vector2Int increaseRate = new Vector2Int(2, 5);
    private int currentIncrease;
    private float value, increaseValueTimer;

    //EXPLOSION
    public GameObject explodeParticles;
    public Color explosionColor;
    public Vector2 StablePhase = new Vector2(3, 6), UnstablePhase = new Vector2(3, 5);
    private float stablePhaseTime, unstablePhaseTime;
    public float TimeAfterSizzle = 2f;
    private float bombTimer = 0;
    private bool nearExplode = false;
    private ShockwaveScript distortionFX;

    [Header("PHYSICS")]
    public float thrust = 20;
    public float defaultRbDrag = 0.008f;
    public float inPlayerOrbitRbDrag = 0.25f;
    public float maxInOrbitTime = 5;
    public float outOrbitForce = 20;
    
    private bool canConsume = false;
    private float collectTimer, releaseTimer = 0;
    private bool canScore = false;
    private Collider2D asteroidColl;
    [HideInInspector] public PlayerPlanets playerPlanets;
    [HideInInspector] public PlayerShip ownerPlayer;
    [HideInInspector] public PlayerTagsManager playerTagsManager;
    [HideInInspector] public GameObject playerOrbit;

    private AsteroidNetwork network;

    private Vector3 baseScale, explosionExpand = Vector3.zero;
    private float baseTextScale, increasePopupBaseSize, increasePopupHideTimer;
    private bool scaleBack = false;
    private float timeBombTick = 0;

    private const float activateAfterSpawning = 1.25f;
    private Vector3 standardGlowScale;
    private float thrustDelay = 0;

    private bool destroy = false;

    private float spawnTimer = 0;
    public bool IsDoneSpawning {
        get {return spawnTimer > activateAfterSpawning;}
    }

    void Start() {
        stablePhaseTime = Random.Range(StablePhase.x, StablePhase.y);
        unstablePhaseTime = Random.Range(UnstablePhase.x, UnstablePhase.y);
        distortionFX = transform.GetComponentInChildren<ShockwaveScript>();
        distortionFX.gameObject.SetActive(false);
        network = GetComponent<AsteroidNetwork>();
        rb = GetComponent<Rigidbody2D>();
        asteroidColl = GetComponent<Collider2D>();
        playerTagsManager = GetComponent<PlayerTagsManager>();
        rb.drag = defaultRbDrag - .15f;
        SetTexture(TextureSwitcher.GetCurrentTexturePack());
        rb.AddForce(-transform.right * thrust);
        baseTextScale = scoreText.transform.localScale.x;
        scoreText.transform.localScale = Vector3.zero;
        increasePopupBaseSize = increasePopupTxt.transform.localScale.x;
        increasePopupTxt.transform.localScale = Vector3.zero;
        currentIncreaseDelay = Random.Range(increaseValueDelay.x, increaseValueDelay.y);
        currentIncrease = Random.Range(increaseRate.x, increaseRate.y);
        standardGlowScale = glow.transform.localScale;
    }

    void OnEnable() {
        transform.SetParent(GameObject.FindGameObjectWithTag("ASTEROIDBELT").transform, true);
        baseScale = transform.localScale;
        if(PhotonNetwork.IsMasterClient) {
            value = Random.Range(baseValue.x, baseValue.y);
            photonView.RPC("SynchValue", RpcTarget.All, value);
        }
    }

    [PunRPC]
    public void SynchValue(float val) {
        this.value = val;
    }

    void Update() {
        /* 
        thrustDelay += Time.deltaTime;
        if(thrustDelay < 0.2f) {
            rb.AddForce(transform.forward * 4f + transform.right * 8f);
        }*/
        
        increasePopupTxt.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f) * 10f);
        increasePopupTxt.transform.position = transform.position + new Vector3(0.05f, 0.35f, 0);
        if(increasePopupHideTimer > 1f) increasePopupTxt.transform.localScale = Vector3.Lerp(increasePopupTxt.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
        
        scoreText.transform.localScale = Vector3.Lerp(scoreText.transform.localScale, Vector3.one * baseTextScale, Time.deltaTime * 2f);
        scoreText.text = value.ToString();
        scoreText.transform.rotation = Quaternion.identity;
        
        spawnTimer += Time.deltaTime;
        increasePopupHideTimer += Time.deltaTime;
        if(spawnTimer < activateAfterSpawning) return;

        //Explosion phase
        if(spawnTimer > stablePhaseTime) {
            bombTimer += Time.deltaTime;
            timeBombTick += Time.deltaTime;
            var tickBomb = spawnTimer - stablePhaseTime;
            src.transform.localPosition = glow.transform.localPosition = scoreText.transform.localPosition = Vector3.Lerp(src.transform.localPosition, new Vector3(Mathf.Sin(Time.time * tickBomb * 4f) * 10f * tickBomb, Mathf.Sin(Time.time * tickBomb * 4f) * 10f * tickBomb, 0), tickBomb * Time.deltaTime * 4f);

            if(timeBombTick > 1f / tickBomb) {
                AudioManager.PLAY_SOUND("timebombtick", 2f, Random.Range(0.5f, 0.55f) + tickBomb / 3f);
                timeBombTick = 0;
            }

            glow.fillAmount = Mathf.Sin(Time.time * tickBomb);

            src.color = glow.color = Color.Lerp(src.color, explosionColor, tickBomb * Time.deltaTime);

            explosionExpand = Vector2.one * Mathf.Sin(Time.time * tickBomb) / 50f;
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale + explosionExpand, Time.deltaTime * tickBomb);
            distortionFX.SetIntensity(tickBomb / 1000f);

            if(bombTimer > unstablePhaseTime / 2f) distortionFX.gameObject.SetActive(true);

            //Actual explosion
            if(bombTimer > unstablePhaseTime) {
                if(!nearExplode) {
                    AudioManager.PLAY_SOUND("sizzle21", 2f, Random.Range(1f, 1.05f));
                    nearExplode = true;
                }
                if(bombTimer > unstablePhaseTime + TimeAfterSizzle && !destroy) photonView.RPC("ExplodeAsteroid", RpcTarget.All, photonView.ViewID);
            }
        }

        increaseValueTimer += Time.deltaTime;
        if(increaseValueTimer > currentIncreaseDelay) {
            if(PhotonNetwork.IsMasterClient) {
                int inc = Random.Range(increaseRate.x, increaseRate.y);
                float del = Random.Range(increaseValueDelay.x, increaseValueDelay.y);
                photonView.RPC("IncreaseValue", RpcTarget.All, inc, del);
                increaseValueTimer = 0;
            }
        }

        if(scaleBack) transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 2f);

        if (collectTimer > 0) collectTimer -= Time.deltaTime;
        asteroidColl.enabled = collectTimer <= 0f; 

        if(held) ReleaseAsteroid(false);
        else ReleasedTimer();
    }
    
    [PunRPC]
    public void ExplodeAsteroid(int viewID) {
        if(photonView.ViewID == viewID) {
            Camera.main.GetComponent<ScreenShake>().Turn(2f);
            AudioManager.PLAY_SOUND("Explode", 1f, Random.Range(0.95f, 1.05f));
            Instantiate(explodeParticles, transform.position, Quaternion.identity);
            PhotonNetwork.InstantiateSceneObject("Shockwave", transform.position, Quaternion.identity);

            destroy = true;

            photonView.RPC("DestroyAsteroid", RpcTarget.All, photonView.ViewID);
            Destroy(gameObject);
        }
    }

    public void DisableTrails() {
        playerTagsManager.DisableTrails();
    }

    [PunRPC]
    public void IncreaseValue(int increase, float delay) {
        value += currentIncrease;
        increasePopupTxt.text = "+" + currentIncrease.ToString() + "!";
        currentIncrease = increase;
        increasePopupTxt.transform.localScale = Vector3.one * increasePopupBaseSize * 1.5f;
        increasePopupHideTimer = 0;
        currentIncreaseDelay = delay;
    }

    public bool IsOwnedBy(PlayerShip player) {
        if(ownerPlayer == null) return false;
        return ownerPlayer.photonView.ViewID == player.photonView.ViewID;
    }

    public void SetColor(float r, float g, float b) {
        src.color = glow.color = new Color(r, g, b);
    }

    public void Capture(HookShot hookShot) {
        if(!hookShot.canHold()) return;
        if((!held || (held && ownerPlayer != null && ownerPlayer.photonView.ViewID != hookShot.hostPlayer.photonView.ViewID))) {
            scaleBack = false;
            transform.position = hookShot.transform.position;
            ownerPlayer = hookShot.hostPlayer;
            FetchAsteroid(hookShot.hostPlayer);
            hookShot.CatchObject(gameObject);
            collectTimer = grabDelay; 
            playerTagsManager.GiveTag();
            photonView.RPC("SetAsteroidOwner", RpcTarget.AllBufferedViaServer, ownerPlayer.photonView.ViewID, false);
        }
    }

    [PunRPC]
    public void SetAsteroidOwner(int ownerID, bool forceReset) {
        Color col = Color.white;
        var owner = PhotonNetwork.GetPhotonView(ownerID);
        if(owner != null) {
            held = true;
            if(ownerPlayer != null) col = ownerPlayer.playerColor;
            SetColor(col.r, col.g, col.b);
            this.ownerPlayer = owner.GetComponent<PlayerShip>();
        }
        if(forceReset) {
            SetColor(1f, 1f, 1f);
            held = false;
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.gameObject.tag == "PLAYERPLANET" && col.gameObject != null) {
            playerPlanets = col.gameObject.GetComponent<PlayerPlanets>();
            if(playerTagsManager.tagNum == playerPlanets.playerNumber && playerPlanets.HasPlayer() && !playerPlanets.HasReachedMax()) {
                if(canConsume) ConsumeResource();
            }
        }
        if(col.gameObject.tag == "ORBIT") {
            inOrbit = true;
            if(!held) OrbitAroundPlanet();
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "ORBIT") {
            inOrbit = false;
            OrbitAroundPlanet();
        }     
    }

    public void SetTexture(TextureSwitcher.TexturePack elm) {
        src.sprite = elm.asteroid.src;
        if(elm.asteroid.glow == null) {
            glow.enabled = false;
            return;
        }
        glow.enabled = true;
        glow.sprite = elm.asteroid.glow;
        src.SetNativeSize();
        glow.SetNativeSize();
    }

    void OrbitAroundPlanet() {
        if(inOrbit) {
            if(playerTagsManager != null && playerTagsManager.tagNum != 0) {
                if(ownerPlayer.playerNumber == playerTagsManager.tagNum) {
                    inOrbitTimer += Time.deltaTime;
                    rb.drag = inPlayerOrbitRbDrag;

                    if(inOrbitTimer >= maxInOrbitTime) canConsume = true;
                } else {
                    inOrbitTimer = 0;
                    rb.drag = defaultRbDrag;
                }
            } if(playerPlanets != null && playerTagsManager.tagNum != playerPlanets.playerNumber) {
                //Throw the resource out of the orbit
                inOrbitTimer += Time.deltaTime;

                if(inOrbitTimer >= maxInOrbitTime) {
                    rb.AddForce(-transform.right * outOrbitForce);
                    inOrbit = false;
                    inOrbitTimer = 0;
                }
            }
        } else inOrbitTimer = 0;
    }

    [PunRPC]
    public void DestroyAsteroid(int asteroidID) {
        if(photonView != null && photonView.ViewID == asteroidID) {
            GameManager.DESTROY_SERVER_OBJECT(gameObject);
            Destroy(gameObject);
        }
    }

    public void ConsumeResource() {
        playerPlanets.AddingResource(value);
        GameManager.DESTROY_SERVER_OBJECT(gameObject);
        if(photonView != null) photonView.RPC("DestroyAsteroid", RpcTarget.All, photonView.ViewID);
        Destroy(gameObject);
        canConsume = false;
    }

    public void ForceRelease(bool force = false) {
        if(photonView != null && ownerPlayer.photonView != null) {
            if(!force) photonView.RPC("SetAsteroidOwner", RpcTarget.All, 0, false);
            else photonView.RPC("SetAsteroidOwner", RpcTarget.All, 0, true);
        }
    }

    public void ReleaseAsteroid(bool released) {
        if(released) {
            playerTagsManager.TagOn(true);
            playerTagsManager.runTagTimer = true;
            held = false;
            canScore = true;
            scaleBack = true;
            ReleasedTimer();
            ForceRelease();
        } else {
            held = true;
            playerTagsManager.runTagTimer = false;
        }
    }

    public void FetchAsteroid(PlayerShip own) {
        held = true;
    }

    public void ReleasedTimer() {//Gives a small time window in which the player can instantly score
        if (canScore && canConsume == false) {
            releaseTimer += Time.deltaTime;
            canConsume = true; 

            if (releaseTimer >= timeToScore) {
                canScore = false;
                releaseTimer = 0f;
            }
        } else canConsume = false; 
    }
}
