﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviourPunCallbacks {
    public bool isSinglePlayer {
        get {return GameManager.instance != null && GameManager.instance.isSinglePlayer;}
    } 
    public HookShot hookShot;
    public ParticleSystem exhaust;

    private GameObject homePlanet;
    [HideInInspector] public GameObject playerLabel;

    [System.Serializable]
    public enum HookMethod {
        FreeAim, LockOn
    }
    [Space(5)]
    public HookMethod hookMethod;

    [HideInInspector] public Collider2D[] colliders;

    [Header("PLAYER VALUES")]
    public LockOnAim lockOnAim;
    public Sprite emit;
    public Sprite noEmit;
    public Image ship;
    //Player Identity
    public int playerNumber;
    public Color playerColor;
    [Space(10)]
    private CustomController customController;
    public Component[] networkIgnore;

    #region MOVEMENT
    [Header("PHYSICS")]
        public float maxVelocity = 5;
        public float acceleration = 0.1f;

        [Range(1, 20)]
        public float turningSpeed = 2.5f;
        [Range(1, 5)]
        public float brakingSpeed = 1;

        public float defaultDrag;
        public float stopDrag;
    #endregion

    //Rigidbody reference voor physics en movement hoeraaa
    private Rigidbody2D rb;
    private float velocity, turn;
    [Header("GRAPPLE")]
    public float trailingSpeed = 8f;

    [Range(0.1f,10)]
    public float throwingReduction = 1f; 

    //Voor het herkennen van de local player (die jij speelt) ivm network
    public static GameObject LocalPlayerInstance;
    public static string PLAYERNAME;

    private Vector3 exLastPos;
    private float exLastTime;

    private AudioSource exhaustSound;

    private PlayerName playerName;
    private PlayerPlanets planet;

    public GameObject GetHomePlanet() {
        return homePlanet;
    }

    public void SetHomePlanet(GameObject planet) {
        homePlanet = planet;
        this.planet = homePlanet.GetComponent<PlayerPlanets>();
    }

    [PunRPC]
    public void SetAim(int i) {
         hookMethod = (i == 0) ? HookMethod.FreeAim : HookMethod.LockOn;
    }

    #region IPunObservable implementation
        public override void OnEnable() {
            base.OnEnable();

            colliders = GetComponentsInChildren<Collider2D>();
            exhaustSound = GetComponent<AudioSource>();
            transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);

            if(!isSinglePlayer && photonView != null) playerNumber = photonView.ViewID;

            if(!isSinglePlayer && photonView != null && !photonView.IsMine) {
                Random.InitState(photonView.ViewID);
                playerColor = Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0.6f, 1f));
                exLastPos = transform.position;
                var playerNameTag = Instantiate(Resources.Load("PlayerName"), transform.position, Quaternion.identity) as GameObject;
                playerName = playerNameTag.GetComponent<PlayerName>();
                if(playerName != null) {
                    playerName.SetColor(playerColor);
                    if(photonView.Owner != null) playerName.SetHost(gameObject, photonView.Owner.NickName);
                }
                if(photonView.Owner != null) PLAYERNAME = photonView.Owner.NickName;
                playerLabel = playerNameTag;
                if(playerNameTag != null) GameManager.playerLabels.Add(PLAYERNAME, playerNameTag);
                foreach(var i in networkIgnore) if(i != null) DestroyImmediate(i);
            }
            if(playerLabel != null) playerLabel.GetComponent<Text>().color = playerColor;
            if(PhotonNetwork.IsMasterClient && photonView != null && !isSinglePlayer) photonView.RPC("SetAim", RpcTarget.All, PlayerPrefs.GetInt("AIM_MODE"));
            lockOnAim.gameObject.SetActive(hookMethod == HookMethod.LockOn);
            GameManager.ClaimPlanet(this);
        }
    #endregion

    public void SetPlayerNameColor(Color col) {
        if(playerName == null) return;
        playerName.SetColor(col);
    }

    public void SetColor(float r, float g, float b) {
        var col = new Color(r, g, b);
        SetPlayerNameColor(col);
        playerColor = col;
        if(planet != null) planet.AssignPlayer(this);
    }

    public void SetCollision(Collider2D asteroid, bool state) {
        foreach(var i in colliders) if(!i.isTrigger) Physics2D.IgnoreCollision(i, asteroid, !state);
    }

    //Lijn aan asteroids/objecten achter de speler
    [HideInInspector] public List<Asteroid> trailingObjects = new List<Asteroid>();

   public static void SetName(string name) {
       PLAYERNAME = name;
   }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        var cont = GameObject.FindGameObjectWithTag("CUSTOM CONTROLLER");
        if(cont != null) customController = cont.GetComponent<CustomController>();
        if(customController != null && customController.useCustomControls) hookShot.customController = customController;
    }

    void Awake() {
        if(IsThisClient()) PlayerShip.LocalPlayerInstance = this.gameObject;       
    }

    void FixedUpdate() {
        if(IsThisClient() || isSinglePlayer) {
            ProcessInputs();
            if(rb != null) {
                rb.AddForce(transform.up * velocity);
                rb.rotation = turn;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && trailingObjects.Count > 0) {
            AudioManager.PLAY_SOUND("collect");
            var asteroid = trailingObjects[0];
            trailingObjects.RemoveAt(0);
            asteroid.rb.constraints = RigidbodyConstraints2D.None;
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            asteroid.rb.velocity = rb.velocity / throwingReduction; 
            asteroid.ReleaseAsteroid(true); 
        }
    }

    void Update() {
        exhaustSound.volume = Mathf.Lerp(exhaustSound.volume, IsThrust() ? 0.05f : 0, Time.deltaTime * 10f);

        lockOnAim.selectColor = playerColor;
        //Particles emitten wanneer movement
        if(IsThisClient() || isSinglePlayer) {
            var emitting = exhaust.emission;
            emitting.enabled = IsThrust();
            ship.sprite = IsThrust() ? emit : noEmit;
        } else {
            if(exLastTime > 0) exLastTime -= Time.deltaTime;
            else {
                exLastPos = transform.position;
                exLastTime = 0.25f;
            }
            var emitting = exhaust.emission;
            bool shouldEmit = Mathf.Abs(Vector3.Distance(exLastPos, transform.position)) > 0.025f;
            emitting.enabled = shouldEmit;
            ship.sprite = shouldEmit ? emit : noEmit;
        }

        if (!isSinglePlayer) {
            if (photonView == null) return;
            if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
        }

        //Removes asteroids that got destroyed / eaten by the sun
        for(int i = 0; i < trailingObjects.Count; i++) if(trailingObjects[i] == null) {
            trailingObjects.RemoveAt(i);
            i--;
        }

        //Removes asteroids owned by other players
        for(int i = 0; i < trailingObjects.Count; i++) if(!trailingObjects[i].IsOwnedBy(this)) {
            trailingObjects.RemoveAt(i);
            i--;
        }

        //Trailing object positions & (stiekem) een kleinere scaling, anders waren ze wel fk bulky
        for (int i = 0; i < trailingObjects.Count; i++)
            if(trailingObjects[i].held) {
                trailingObjects[i].transform.localScale = Vector3.Lerp(trailingObjects[i].transform.localScale, Vector3.one * 0.06f, Time.deltaTime * 2f);
                trailingObjects[i].transform.position = Vector3.Lerp(trailingObjects[i].transform.position, (transform.position - (transform.up * (i + 1) * 0.5f)), Time.deltaTime * trailingSpeed);
            }

        //ignore this, this is for later
        // if(Health < 0) GameManager.instance.LeaveRoom();
    }

    void ProcessInputs() {
        //naar voren en naar achteren (W & S)
        if(IsThrust()) {
            velocity = Mathf.Lerp(velocity, maxVelocity, Time.deltaTime * acceleration);
            rb.drag = defaultDrag;
        }
        else {
            velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * acceleration * 2f);
            rb.drag = stopDrag;
        }

        //Spreekt voor zich
        if(IsBrake()) {
            velocity = 0;
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * brakingSpeed);
        }

        if(IsTurningLeft()) turn += turningSpeed * Time.deltaTime * 50f;
        if(IsTurningRight()) turn -= turningSpeed * Time.deltaTime * 50f;
    }

    //Wanneer je orbits exit, de speed dampening.
    public void NeutralizeForce(float exitVelocityReduction) {
        if(rb != null) rb.velocity /= exitVelocityReduction;
    }

    //Voegt object toe aan trail achter player
    public void AddAsteroid(GameObject obj) {
        obj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        var script = obj.GetComponent<Asteroid>();
        if(script != null) trailingObjects.Add(script);
    }

    public void RemoveAsteroid(GameObject obj) {
        for(int i = 0; i < trailingObjects.Count; i++) {
            if(trailingObjects[i] == obj) {
                trailingObjects.RemoveAt(i);
                return;
            }
        }
    }

    public bool IsThisClient() {
        return photonView != null && photonView.IsMine;
    }

    [PunRPC]
    public void CastHook(int viewID) {
        if(photonView.ViewID == viewID) hookShot.CastHook();
    }

    #region MOVEMENT_INPUTS
    public bool IsThrust() {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        public bool IsBrake() {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }

        public bool IsTurningRight() {
            return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        }

        public bool IsTurningLeft() {
            return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        }
    #endregion
}
