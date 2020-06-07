using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class Asteroid : PickupableObject {
    public TextMeshProUGUI scoreText, increasePopupTxt;

    public Animator animator;

    public bool canScoreWithoutDropping = false;

    [HideInInspector] public bool inOrbit = false;
    [HideInInspector] public bool giveTag = false;
    [HideInInspector] public float inOrbitTimer;

    public float timeToScore = 3f;
    public float grabDelay = 0; 
    
    [Header("VALUE & WORTH")]
    public Vector2Int baseValue = new Vector2Int(4, 6);
    public Vector2 increaseValueDelay = new Vector2(2, 3);
    public int maxValue = 20;
    private float currentIncreaseDelay = 4;
    public Vector2Int increaseRate = new Vector2Int(3, 5);
    private int currentIncrease;
    [HideInInspector] public float value;
    private float increaseValueTimer;

    //EXPLOSION
    public GameObject explodeParticles;
    public Color explosionColor, UnstableTextColor;
    public Vector2 StablePhase = new Vector2(8, 10);
    private float stablePhaseTime, unstablePhaseTime;
    public float TimeAfterSizzle = 1f;
    private float bombTimer = 0;
    private bool nearExplode = false;
    private ShockwaveScript distortionFX;

    [Header("PHYSICS")]
    public float defaultRbDrag = 0.2f;
    public float maxInOrbitTime = 5;
    public float outOrbitForce = 40;

    [Header("SPAWN")]
    public float beginThrust = 0.4f;
    public float curve = 0;
    public float swirlDuration = 2;
    private bool StartThrustTimer = true;
    private bool CurveThrustTimer = true;
    public float Thrust = 0.1f;
    public float StartThrustTimerAmount = 2.5f;
    public float speedRotate = 7;
    public float timeRotate = -70;
    private int LinksOfRechts = 0;

    private float consumeTimer = 0;
    private bool canConsume = false;
    private float collectTimer, releaseTimer = 0;
    private bool canScore = false;
    [HideInInspector] public PlayerPlanets playerPlanets;
    [HideInInspector] public PlayerTagsManager playerTagsManager;
    [HideInInspector] public GameObject playerOrbit;

    private AsteroidNetwork network;

    private Vector3 baseScale, explosionExpand = Vector3.zero;
    private float baseTextScale, increasePopupBaseSize, increasePopupHideTimer;
    private bool timeBombTick = false;

    private Vector3 standardGlowScale;
    private Vector3 standardScale;

    private bool destroy = false;

    void Start() {
        base.Init();
        dropBoosts = true;
        distortionFX = transform.GetComponentInChildren<ShockwaveScript>();
        distortionFX.gameObject.SetActive(false);
        network = GetComponent<AsteroidNetwork>();
        rb = GetComponent<Rigidbody2D>();
        asteroidColl = GetComponent<Collider2D>();
        playerTagsManager = GetComponent<PlayerTagsManager>();
        rb.drag = defaultRbDrag - .15f;
        standardScale = transform.localScale;
        rb.AddForce(transform.up * Thrust);
        LinksOfRechts = Random.Range(0, 2);

        baseTextScale = scoreText.transform.localScale.x;
        scoreText.transform.localScale = Vector3.zero;
        increasePopupBaseSize = increasePopupTxt.transform.localScale.x;
        increasePopupTxt.transform.localScale = Vector3.zero;
    }

    void OnEnable() {
        transform.SetParent(GameObject.FindGameObjectWithTag("ASTEROIDBELT").transform, true);
        animator.SetBool("res-low", true);

        baseScale = transform.localScale;
        
        if(PhotonNetwork.IsMasterClient && photonView.ViewID > 0) {
            stablePhaseTime = Random.Range(StablePhase.x, StablePhase.y);
            currentIncreaseDelay = Random.Range(increaseValueDelay.x, increaseValueDelay.y);
            currentIncrease = Random.Range(increaseRate.x, increaseRate.y);
            
            value = Random.Range(baseValue.x, baseValue.y);
            photonView.RPC("SynchValues", RpcTarget.All, value, currentIncrease, currentIncreaseDelay, stablePhaseTime, unstablePhaseTime);
        }
    }

    [PunRPC]
    public void SynchValues(float val, int inc, float del, float stablePhaseTime, float unstablePhaseTime) {
        if(PhotonNetwork.IsMasterClient) return;
        this.value = val;
        this.currentIncrease = inc;
        this.currentIncreaseDelay = del;
        this.stablePhaseTime = stablePhaseTime;
        this.unstablePhaseTime = unstablePhaseTime;
    }

    [PunRPC]
    public void SynchTimer(float timer, bool timeBombTick) {
        if(PhotonNetwork.IsMasterClient) return;
        this.spawnTimer = timer;
        this.timeBombTick = timeBombTick;
    }

    void FixedUpdate() {
        thrustDelay += Time.fixedDeltaTime;
        if(thrustDelay > 0.25f && thrustDelay < swirlDuration) rb.AddRelativeForce(transform.right * 0.05f * (swirlDuration - thrustDelay) * curve, ForceMode2D.Impulse);
        
        StartThrustTimerAmount -= Time.deltaTime;
        if (StartThrustTimerAmount < 0) StartThrustTimer = false;
        
        if (StartThrustTimer) {
            int Direction = 0;
            if(LinksOfRechts == 1) Direction = 10;
            else if (LinksOfRechts == 0) Direction = -10;

            rb.AddForce(transform.up * beginThrust); 
            transform.Rotate(0,0, Time.deltaTime * speedRotate * Direction );
        }
        if(StartThrustTimerAmount < timeRotate) CurveThrustTimer = false;
    }

    void Update() {
        unstablePhaseTime = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        if(gameObject.tag == "ResourceTutorial" && held && ownerPlayer != null) ownerPlayer.playerTutorial.CompleteSubTask("GrabResource");

        if(consumeTimer > 0) consumeTimer -= Time.deltaTime;

        float fade = (collectTimer <= 0f) ? 1 : 0.4f;
        if(scoreText != null) scoreText.color = Color.Lerp(scoreText.color, new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, fade), Time.deltaTime * 5f);

        if(collectTimer > 0) collectTimer -= Time.deltaTime;

        if(increasePopupTxt != null) {
            increasePopupTxt.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f) * 10f);
            increasePopupTxt.transform.position = transform.position + new Vector3(0.05f, 0.35f, 0);
            if(increasePopupHideTimer > 1f) increasePopupTxt.transform.localScale = Vector3.Lerp(increasePopupTxt.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
        }

        if (scoreText != null)
        {
            scoreText.transform.localScale = Vector3.Lerp(scoreText.transform.localScale, Vector3.one * baseTextScale, Time.deltaTime * 2f);
            scoreText.text = "+" + value.ToString();

            var basePos = transform.position - new Vector3(0.0f, 0.35f, 1);

            scoreText.transform.rotation = Quaternion.identity;
            scoreText.transform.position = basePos;
        }

        if (PhotonNetwork.IsMasterClient && photonView.ViewID > 0) {
            spawnTimer += Time.deltaTime;
            photonView.RPC("SynchTimer", RpcTarget.All, spawnTimer, timeBombTick);
        }
        increasePopupHideTimer += Time.deltaTime;
        if(spawnTimer < activateAfterSpawning) return;

        //ANIMATION -- MEDIUM VALUE && HIGH VALUE 
        //Explosion phase
        if(spawnTimer > stablePhaseTime) {
            scoreText.CrossFadeColor(UnstableTextColor, 0.5f, false, false);

            bombTimer += Time.deltaTime;
            var tickBomb = spawnTimer - stablePhaseTime;
           // src.transform.localPosition = glow.transform.localPosition = scoreText.transform.localPosition = Vector3.Lerp(src.transform.localPosition, new Vector3(Mathf.Sin(Time.time * tickBomb * 4f) * 10f * tickBomb, Mathf.Sin(Time.time * tickBomb * 4f) * 10f * tickBomb, 0), tickBomb * Time.deltaTime * 4f);

            if(!timeBombTick) {
                SoundManager.PLAY_SOUND("HotPotatoTicking");
                timeBombTick = true;
            }

            if(bombTimer > 1f / tickBomb) {
                explosionExpand = new Vector3(1.2f, 1.2f, 1.2f) * Mathf.Sin(Time.time * tickBomb) / 20f;
                distortionFX.SetIntensity(tickBomb / 1000f);
            }
            if(bombTimer > unstablePhaseTime / 2f) distortionFX.gameObject.SetActive(true); 
            animator.SetBool("res-unstable", true);
            animator.SetBool("res-high", false);

            //Actual explosion
            if(spawnTimer > (stablePhaseTime + unstablePhaseTime) - 1) {
                if(!nearExplode) {
                    SoundManager.PLAY_SOUND("HotPotatoSizzle");
                    nearExplode = true;
                }
                if(spawnTimer > stablePhaseTime + unstablePhaseTime + TimeAfterSizzle && !destroy) photonView.RPC("ExplodeAsteroid", RpcTarget.All, photonView.ViewID);
            } 
        }
        
        if(PhotonNetwork.IsMasterClient) {
            increaseValueTimer += Time.deltaTime;
            if(increaseValueTimer > currentIncreaseDelay) {
                int inc = Random.Range(increaseRate.x, increaseRate.y);
                float del = Random.Range(increaseValueDelay.x, increaseValueDelay.y);
                photonView.RPC("SetValue", RpcTarget.All, value + inc, inc, del);
                increaseValueTimer = 0;
            }
        }

        if(held) ReleaseAsteroid(false, photonView.ViewID);
        else ReleasedTimer();  
    }
    
    [PunRPC]
    public void ExplodeAsteroid(int viewID) {
        if(photonView.ViewID == viewID) {
            Camera.main.GetComponent<ScreenShake>().Turn(0.7f);

            SoundManager.PLAY_SOUND("HotPotatoExplosion");

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
    public void SetValue(float value, int increase, float delay) {
        if(this.value == maxValue) return;
        this.value = value;
        increasePopupTxt.text = "+" + increase.ToString() + "!";
        currentIncrease = increase;
        increasePopupTxt.transform.localScale = Vector3.one * increasePopupBaseSize * 1.5f;
        increasePopupHideTimer = 0;
        currentIncreaseDelay = delay;
        if(this.value > maxValue) this.value = maxValue;
    }

    public override void Capture(HookShot hookShot) {
        if(!hookShot.CanHold() || collectTimer > 0) return;

        if(GameManager.GAME_STARTED || (!GameManager.GAME_STARTED && hookShot.hostPlayer.photonView.IsMine)) SoundManager.PLAY_SOUND("CatchObject");

        if(gameObject.tag == "ResourceTutorial" || (!held || (held && ownerPlayer != null && ownerPlayer.photonView.ViewID != hookShot.hostPlayer.photonView.ViewID))) {
            transform.position = hookShot.transform.position;
            ownerPlayer = hookShot.hostPlayer;
            FetchAsteroid(hookShot.hostPlayer);
            hookShot.CatchObject(gameObject);
            collectTimer = grabDelay; 
            consumeTimer = 1.25f;

            if(photonView.ViewID > 0) {
                photonView.RPC("SynchCollectTimer", RpcTarget.All, collectTimer);
                photonView.RPC("SetAsteroidOwner", RpcTarget.AllBufferedViaServer, ownerPlayer.photonView.ViewID, false);
            } else {
                SynchCollectTimer(collectTimer);
                SetAsteroidOwner(ownerPlayer.photonView.ViewID, false);
            }
        }
    }

    [PunRPC]
    protected void SynchCollectTimer(float del) {
        collectTimer = del;
    }

    [PunRPC]
    public void SetAsteroidOwner(int ownerID, bool forceReset) {
        Color col = Color.white;
        var owner = PhotonNetwork.GetPhotonView(ownerID);
        if(owner != null) {
            held = true;
            this.ownerPlayer = owner.GetComponent<PlayerShip>();
            if(this.ownerPlayer != null) col = ownerPlayer.playerColor;
            if(playerTagsManager != null) playerTagsManager.GiveTag();
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ASTEROID"), LayerMask.NameToLayer("PLAYER"), true);
        }
        if(forceReset) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ASTEROID"), LayerMask.NameToLayer("PLAYER"), false);
            held = false;
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.gameObject.tag == "PLAYERPLANET" && col.gameObject != null) {
            playerPlanets = col.gameObject.GetComponent<PlayerPlanets>();
            if(playerTagsManager.tagNum == playerPlanets.playerNumber && playerPlanets.HasPlayer() && !playerPlanets.HasReachedMax()) {
                if((canConsume || canScoreWithoutDropping) && gameObject.tag != "ResourceTutorial") ConsumeResource();

                if(gameObject.tag == "ResourceTutorial" && throwed) {
                    if(ownerPlayer != null) ownerPlayer.playerTutorial.CompleteSubTask("throwresource");
                    ConsumeResource();
                }
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

    void OrbitAroundPlanet() {
        if(inOrbit) {
            if(playerTagsManager != null && playerTagsManager.tagNum != 0) {
                if(ownerPlayer.playerNumber == playerTagsManager.tagNum) {
                    inOrbitTimer += Time.deltaTime;

                    if(inOrbitTimer >= maxInOrbitTime) canConsume = true;
                } else {
                    inOrbitTimer = 0;
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
        if(gameObject.tag == "ResourceTutorial" && held) value = 10;
        if(consumeTimer > 0) return;
        playerPlanets.AddingResource(value);
        if(photonView != null && photonView.ViewID > 0) photonView.RPC("DestroyAsteroid", RpcTarget.All, photonView.ViewID);
        Destroy(gameObject);
        canConsume = false;
    }

    public new void ForceRelease(bool force = false) {
        if(photonView != null && ownerPlayer.photonView != null) {
            if(!force) photonView.RPC("SetAsteroidOwner", RpcTarget.All, 0, false);
            else photonView.RPC("SetAsteroidOwner", RpcTarget.All, 0, true);
        }
    }

    [PunRPC]
    public new void ReleaseAsteroid(bool released, int viewID) {
        if(photonView.ViewID == viewID) {
            if(released) {
                playerTagsManager.runTagTimer = true;
                held = false;
                transform.localScale = baseScale;
                canScore = true;
                ReleasedTimer();
                ForceRelease();
            } else {
                held = true;
                playerTagsManager.runTagTimer = false;
            }
        }
    }

    public void FetchAsteroid(PlayerShip own) {
        held = true;
    }

    public void ReleasedTimer() { //Gives a small time window in which the player can instantly score
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
