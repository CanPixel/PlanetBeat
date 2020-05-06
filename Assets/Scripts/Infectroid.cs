using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public abstract class PickupableObject : MonoBehaviourPun {
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

    public void Init() {
        asteroidColl = GetComponent<Collider2D>();
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
}

public class Infectroid : PickupableObject {
    public Image src, glow;
    public Text increasePopupTxt;
    public GameObject explodeParticles;

    [HideInInspector] public bool inOrbit = false;
    [HideInInspector] public bool giveTag = false;
    [HideInInspector] public float inOrbitTimer;

    public float grabDelay = 0; 
    
    [Header("INFECT")]
    public float infectDelay = 1;
    private float infectTime = 0;
    public int penalty = 2;
    private bool inPlanet = false;
    public float destroyAfter = 10;

   // [Header("PHYSICS")]
    //public float defaultRbDrag = 0.2f;
    //public float maxInOrbitTime = 5;
    //public float outOrbitForce = 40;

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
    private float timeBombTick = 0;

    private Vector3 standardGlowScale;
    private bool destroy = false;

    void Start() {
        dropBoosts = false;
        base.Init();
        network = GetComponent<AsteroidNetwork>();
        rb = GetComponent<Rigidbody2D>();
        
        //rb.drag = defaultRbDrag;// - .15f;

        SetTexture(TextureSwitcher.GetCurrentTexturePack());
        rb.AddForce(transform.up * Thrust);
        LinksOfRechts = Random.Range(0, 2);

        increasePopupBaseSize = increasePopupTxt.transform.localScale.x;
        increasePopupTxt.transform.localScale = Vector3.zero;
        standardGlowScale = glow.transform.localScale;
    }

    void OnEnable() {
        transform.SetParent(GameObject.FindGameObjectWithTag("ASTEROIDBELT").transform, true);
        baseScale = transform.localScale;
    }

    [PunRPC]
    public void SynchTimer(float timer, float timeBombTick) {
        if(spawnTimer > destroyAfter - 5) {
            increasePopupHideTimer = 0;
            increasePopupTxt.color = new Color(0, 1, 0f);
            increasePopupTxt.text = ((int)Mathf.Clamp((destroyAfter - spawnTimer) + 1, 0, 5)).ToString();
            increasePopupTxt.transform.localScale = Vector3.one * increasePopupBaseSize;
        }

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
        float fade = 1;
        src.color = Color.Lerp(src.color, new Color(src.color.r, src.color.g, src.color.b, fade), Time.deltaTime * 5f);

        if(collectTimer > 0) collectTimer -= Time.deltaTime;

        increasePopupTxt.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f) * 10f);
        increasePopupTxt.transform.position = transform.position + new Vector3(0.05f, 0.35f, 0);
        if(increasePopupHideTimer > 1f) increasePopupTxt.transform.localScale = Vector3.Lerp(increasePopupTxt.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
        
        if(spawnTimer > destroyAfter) {
            DestroyDefinite();
            return;
        }

        if(PhotonNetwork.IsMasterClient) {
            spawnTimer += Time.deltaTime;
            photonView.RPC("SynchTimer", RpcTarget.All, spawnTimer, timeBombTick);
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

    void OnTriggerStay2D(Collider2D col) {
        if(col.gameObject.tag == "ORBIT" && col.gameObject != null && PhotonNetwork.IsMasterClient) {
            playerPlanets = col.gameObject.GetComponent<PlayerPlanets>();
            if(playerPlanets != null && playerPlanets.HasPlayer() && !GameManager.GAME_WON) {
                inPlanet = true;
                infectTime += Time.deltaTime;
                if(infectTime > infectDelay && playerPlanets.currentScore > 0) {
                    playerPlanets.Explode(penalty);
                    infectTime = 0;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if(col.gameObject.tag == "ORBIT" && col.gameObject != null && PhotonNetwork.IsMasterClient) {
            inPlanet = false;
            //infectTime = 0;
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
