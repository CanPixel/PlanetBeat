using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public abstract class PickupableObject : MonoBehaviourPun {
    public float bounceMultiplier = 0.35f;

    public bool dropBoosts = false;

    [HideInInspector] public PlayerShip ownerPlayer;
    [HideInInspector] public Rigidbody2D rb;
    protected Collider2D asteroidColl;
    [HideInInspector] public bool held = false;
    protected float thrustDelay = 0, spawnTimer = 0;
    protected const float activateAfterSpawning = 1.25f;
    public bool IsDoneSpawning {
        get {return spawnTimer > activateAfterSpawning;}
    }

    [HideInInspector] public bool throwed = false;

    public void ReleaseAsteroid(bool released, int viewID) {}

    public void Init() {
        asteroidColl = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
   
    public abstract void Capture(HookShot hookShot);

    public bool IsOwnedBy(PlayerShip player) {
        if(ownerPlayer == null) return false;
        return ownerPlayer.photonView.ViewID == player.photonView.ViewID;
    }

    public Collider2D GetCollider2D() {
        return asteroidColl;
    }

    public void ForceRelease(bool force = false) {
        if(photonView != null && ownerPlayer.photonView != null) {
            if(!force) photonView.RPC("SetAsteroidOwner", RpcTarget.All, 0, false);
            else photonView.RPC("SetAsteroidOwner", RpcTarget.All, 0, true);
        }
    }

    void OnCollisionEnter2D(Collision2D col) {
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if(rb != null && (col.gameObject.tag == "Resource" || col.gameObject.tag == "Powerup")) {
            rb.velocity = new Vector2(-col.relativeVelocity.x * bounceMultiplier, col.relativeVelocity.y * bounceMultiplier);
        }
    }
}

public class Infectroid : PickupableObject {
    public TextMeshProUGUI increasePopupTxt;
    public GameObject explodeParticles;

    [HideInInspector] public bool giveTag = false;
    [HideInInspector] public float inOrbitTimer;

    public bool inOrbit = false;

    public float grabDelay = 0; 
    public Animator infectroidAnimator;
    
    [Header("INFECT")]
    public float infectDelay = 1;
    private float infectTime = 0;
    public int penalty = 2;
    public float destroyAfter = 10;

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

    private Vector3 baseScale;
    private float baseTextScale, increasePopupBaseSize, increasePopupHideTimer;
    private bool scaleBack = false;
    private int spawnTimerChange = 0;

    void Start() {
        dropBoosts = false;
        base.Init();
        network = GetComponent<AsteroidNetwork>();
        rb = GetComponent<Rigidbody2D>();
        
        rb.AddForce(transform.up * Thrust);
        LinksOfRechts = Random.Range(0, 2);

        increasePopupBaseSize = increasePopupTxt.transform.localScale.x;
        increasePopupTxt.transform.localScale = Vector3.zero;
    }

    void OnEnable() {
        transform.SetParent(GameObject.FindGameObjectWithTag("ASTEROIDBELT").transform, true);
        baseScale = transform.localScale;
    }

    [PunRPC]
    public void SynchTimer(float timer) {
        if(spawnTimer > destroyAfter - 5) infectroidAnimator.SetInteger("Infectroid_Animation", 1);
        if(PhotonNetwork.IsMasterClient) return;
        this.spawnTimer = timer;
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

        increasePopupTxt.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f) * 10f);
        increasePopupTxt.transform.position = transform.position + new Vector3(0.05f, 0.35f, 0);
        if(increasePopupHideTimer > 1f) increasePopupTxt.transform.localScale = Vector3.Lerp(increasePopupTxt.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
        
        if(spawnTimerChange > destroyAfter && gameObject.tag != "InfectroidTutorial") {
            DestroyDefinite();
            return;
        }

        if(PhotonNetwork.IsMasterClient || gameObject.tag == "InfectroidTutorial") {
            spawnTimer += Time.deltaTime;

            if(infectTime > infectDelay && playerPlanets != null && playerPlanets.currentScore > 0) {
                playerPlanets.Explode(penalty);
                infectTime = 0;
            }
            if(photonView.ViewID > 0 && (Mathf.CeilToInt(spawnTimer) != spawnTimerChange)) {
                spawnTimerChange = Mathf.CeilToInt(spawnTimer);
                photonView.RPC("SynchTimer", RpcTarget.All, spawnTimer);
            }
        }
        increasePopupHideTimer += Time.deltaTime;
        if(spawnTimer < activateAfterSpawning) return;

        if(scaleBack) transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 2f);

        if(held) ReleaseAsteroid(false, photonView.ViewID);
        else ReleasedTimer();  

        float maxScale = 0.8f;
        transform.localScale = new Vector3(Mathf.Clamp(transform.localScale.x, 0, maxScale), Mathf.Clamp(transform.localScale.y, 0, maxScale), Mathf.Clamp(transform.localScale.z, 0, maxScale));
    }

    public override void Capture(HookShot hookShot) {
        if(!hookShot.CanHold() || collectTimer > 0) return;
        
        SoundManager.PLAY_SOUND("CatchObject");

        if((!held || (held && ownerPlayer != null && ownerPlayer.photonView.ViewID != hookShot.hostPlayer.photonView.ViewID))) {
            scaleBack = false;
            transform.position = hookShot.transform.position;
            ownerPlayer = hookShot.hostPlayer;
            FetchAsteroid(hookShot.hostPlayer);
            hookShot.CatchObject(gameObject);
            collectTimer = grabDelay; 

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
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ASTEROID"), LayerMask.NameToLayer("PLAYER"), true);
        }
        if(forceReset) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ASTEROID"), LayerMask.NameToLayer("PLAYER"), false);
            held = false;
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if(col.gameObject.tag == "ORBIT") {
            var par = col.transform.parent;
            if(par == null) return;
            var planet = par.GetComponent<PlayerPlanets>();
            if(planet != null) {
                playerPlanets = planet;
                inOrbit = true;
            }
            if(playerPlanets != null && playerPlanets.HasPlayer() && !GameManager.GAME_WON) {
                if(playerPlanets.currentScore > 0) playerPlanets.infected = true;
                infectTime += Time.deltaTime;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.gameObject.tag == "ORBIT") {
            var par = col.transform.parent;
            if(par == null) return;
            var planet = par.GetComponent<PlayerPlanets>();
            if(planet != null) {
                planet.infected = false;
                inOrbit = false;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col) {
        if(col.gameObject.tag == "TutorialWall" && throwed) Destroy(gameObject);
    }

    [PunRPC]
    public void DestroyAsteroid(int asteroidID) {
        if(photonView != null && photonView.ViewID == asteroidID) {
            GameManager.DESTROY_SERVER_OBJECT(gameObject);
            Destroy(gameObject);
        }
    }

    public void DestroyDefinite() {
        SoundManager.PLAY_SOUND("InfectroidExplosion");
        GameManager.DESTROY_SERVER_OBJECT(gameObject);
        if(photonView != null) photonView.RPC("DestroyAsteroid", RpcTarget.All, photonView.ViewID);
        Destroy(gameObject);
        canConsume = false;
    }

    [PunRPC]
    public new void ReleaseAsteroid(bool released, int viewID) {
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

    public void ReleasedTimer() {
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
