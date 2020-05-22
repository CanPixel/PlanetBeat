using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Remoteroid : PickupableObject {
    public Image src, glow;
    public GameObject explodeParticles;
    public Text scoreText;

    [HideInInspector] public bool inOrbit = false;
    [HideInInspector] public bool giveTag = false;
    [HideInInspector] public float inOrbitTimer;

    public float grabDelay = 0; 
    
    //EXPLOSION
    public Color explosionColor;

    [Header("PHYSICS")]
    public float defaultRbDrag = 0.2f;
    public float inPlayerOrbitRbDrag = 0.6f;
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

    private bool canConsume = false;
    private float collectTimer, releaseTimer = 0;
    private bool canScore = false;
    [HideInInspector] public PlayerPlanets playerPlanets;
    [HideInInspector] public GameObject playerOrbit;

    private AsteroidNetwork network;

    private Vector3 baseScale, explosionExpand = Vector3.zero;
    private float baseTextScale;
    private bool scaleBack = false;
    private float timeBombTick = 0;

    private Vector3 standardGlowScale;
    private bool destroy = false;

    private int pressDetonate = 0;

    void Start() {
        dropBoosts = false;
        base.Init();
        network = GetComponent<AsteroidNetwork>();
        rb = GetComponent<Rigidbody2D>();
        rb.drag = defaultRbDrag - .15f;
        SetTexture(PlanetSwitcher.GetCurrentTexturePack());
        rb.AddForce(transform.up * Thrust);
        LinksOfRechts = Random.Range(0, 2);
        scoreText.gameObject.SetActive(false);

        baseTextScale = scoreText.transform.localScale.x;
        scoreText.transform.localScale = Vector3.zero;
        standardGlowScale = glow.transform.localScale;
    }

    void OnEnable() {
        transform.SetParent(GameObject.FindGameObjectWithTag("ASTEROIDBELT").transform, true);
        baseScale = transform.localScale;
    }

    [PunRPC]
    public void SynchTimer(float timer, float timeBombTick) {
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
            transform.Rotate(0,0, Time.deltaTime * speedRotate * Direction);
        }
        if(StartThrustTimerAmount < timeRotate) CurveThrustTimer = false;
    }

    void Update() {
        if(collectTimer > 0) collectTimer -= Time.deltaTime;

        scoreText.transform.localScale = Vector3.Lerp(scoreText.transform.localScale, Vector3.one * baseTextScale, Time.deltaTime * 2f);
        scoreText.transform.rotation = Quaternion.identity;

        if(PhotonNetwork.IsMasterClient) {
            spawnTimer += Time.deltaTime;
            photonView.RPC("SynchTimer", RpcTarget.All, spawnTimer, timeBombTick);
        }
        if(spawnTimer < activateAfterSpawning) return;

        if(ownerPlayer != null && ownerPlayer.photonView.IsMine) {
            scoreText.gameObject.SetActive(true);
            scoreText.text = "Space";
            if(ownerPlayer.ReleaseAsteroidKey()) {
                pressDetonate++;
                if(pressDetonate > 1) photonView.RPC("ExplodeAsteroid", RpcTarget.All, photonView.ViewID);
            }
        }
    }

    public override void Capture(HookShot hookShot) {
        if(!hookShot.CanHold() || collectTimer > 0) return;
        AudioManager.PLAY_SOUND("kickVerb", 1, Random.Range(1f, 1.1f));
        AudioManager.PLAY_SOUND("Reel");
        if((!held || (held && ownerPlayer != null && ownerPlayer.photonView.ViewID != hookShot.hostPlayer.photonView.ViewID))) {
            scaleBack = false;
            transform.position = hookShot.transform.position;
            ownerPlayer = hookShot.hostPlayer;
            FetchAsteroid(hookShot.hostPlayer);
            hookShot.CatchObject(gameObject);
            collectTimer = grabDelay; 
            photonView.RPC("SynchCollectTimer", RpcTarget.All, collectTimer);
            photonView.RPC("SetAsteroidOwner", RpcTarget.AllBufferedViaServer, ownerPlayer.photonView.ViewID, false);
        }
    }

    [PunRPC]
    public void ExplodeAsteroid(int viewID) {
        if(photonView.ViewID == viewID) {
            Camera.main.GetComponent<ScreenShake>().Turn(1.5f);
            AudioManager.PLAY_SOUND("Explode", 1f, Random.Range(0.95f, 1.05f));
            Instantiate(explodeParticles, transform.position, Quaternion.identity);
            PhotonNetwork.InstantiateSceneObject("Shockwave", transform.position, Quaternion.identity);

            destroy = true;
            photonView.RPC("DestroyAsteroid", RpcTarget.All, photonView.ViewID);
            Destroy(gameObject);
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
            col = ownerPlayer.playerColor;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ASTEROID"), LayerMask.NameToLayer("PLAYER"), true);
        }
        if(forceReset) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ASTEROID"), LayerMask.NameToLayer("PLAYER"), false);
            held = false;
        }
    }

    public void SetTexture(PlanetSwitcher.TexturePack elm) {
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

    [PunRPC]
    public void DestroyAsteroid(int asteroidID) {
        if(photonView != null && photonView.ViewID == asteroidID) {
            GameManager.DESTROY_SERVER_OBJECT(gameObject);
            Destroy(gameObject);
        }
    }

    public void DestroyDefinite() {
        AudioManager.PLAY_SOUND("Explode", 1f, Random.Range(0.95f, 1.05f));
        Instantiate(explodeParticles, transform.position, Quaternion.identity);
        GameManager.DESTROY_SERVER_OBJECT(gameObject);
        if(photonView != null) photonView.RPC("DestroyAsteroid", RpcTarget.All, photonView.ViewID);
        Destroy(gameObject);
        canConsume = false;
    }

    [PunRPC]
    public void ReleaseAsteroid(bool released, int viewID) {
        if(photonView.ViewID == viewID) {
            if(released) {
                held = false;
                canScore = true;
                scaleBack = true;
                ReleasedTimer();
                ForceRelease();
            } else held = true;
        }
    }

    public void FetchAsteroid(PlayerShip own) {
        held = true;
    }

    public void ReleasedTimer() { //Gives a small time window in which the player can instantly score
        if (canScore && canConsume == false) {
            releaseTimer += Time.deltaTime;
            canConsume = true; 

            if (releaseTimer >= 1) {
                canScore = false;
                releaseTimer = 0f;
            }
        } else canConsume = false; 
    }
}
