﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviourPunCallbacks, IPunObservable {
    public PlayerTutorial playerTutorial;
    public HookShot hookShot;
    public ParticleSystem exhaust;
    public Light exhaustLight;

    [HideInInspector] public GameObject playerLabel;
    [HideInInspector] public Collider2D[] colliders;

    [Header("BOOST")]
    public float boostDuration = 0.2f;
    public float boostVelocity = 23;
    public float boostForce = 230;
    public float boostCooldownDuration = 3;
    private float boostCooldownTimer = 0f, boostTimer = 0f;
    private bool canBoost = true;
    private float fuelMeter;
    public float maxFuel = 0.8f;
    private bool isBoosting;
    private bool onCooldown;
    private float waitForCooldown;
    public float cooldownPenalty; 

    [Header("PLAYER VALUES")]
    public GameObject model;
    public int playerNumber;
    public Color playerColor;
    [Space(10)]
    public Component[] networkIgnore;

    #region MOVEMENT
    [Header("PHYSICS")]
        private float maxVelocity = 5;
        private float baseVelocity;
        public float acceleration = 0.1f;

        public float defaultVelocity = 4.0f;

        [Range(1, 20)]
        public float turningSpeed = 2.5f;
        [Range(1, 5)]
        public float brakingSpeed = 1;

        public float respawningTime = 2;

        public float defaultDrag;
        public float stopDrag;
        private float baseStopDrag, baseDefaultDrag;
        private float hookDelay;

        [Range(1, 10)]
        public int maxAsteroids = 2;
    #endregion

    //Rigidbody reference voor physics en movement hoeraaa
    private Rigidbody2D rb;
    private float velocity, turn;
    [Header("GRAPPLE")]
    public float trailingSpeed = 8f;
    public float throwForce = 34;

    [Range(0.1f,10)]
    public float throwingReduction = 1f; 

    [Space(5)]
    public float heldResourceScaleFactor = 0.09f;

    public static string PLAYERNAME;

    private Vector3 exLastPos;
    private float exLastTime;

    private AudioSource exhaustSound;

    [HideInInspector] public PlayerName playerName;
    [HideInInspector] public PlayerPlanets planet;

    private bool dropAsteroid = false;
    [SerializeField] [HideInInspector] private float respawnDelay = 0;
    private float flicker = 0;

    public bool CanExplode() {
        return respawnDelay <= 0;
    }

    public GameObject GetHomePlanet() {
        if(planet == null) return null;
        return planet.gameObject;
    }
    public void SetHomePlanet(GameObject planet) {
        this.planet = planet.GetComponent<PlayerPlanets>();
        playerColor = planet.GetComponent<PlanetGlow>().planetColor;
        ForceColor(playerColor.r, playerColor.g, playerColor.b);
    }
    [PunRPC]
    public void ClearHomePlanet() {
        if(planet != null && photonView != null) planet.ResetPlanet();
        planet = null;
    }

    public void SetTextureByPlanet(Color col) {
        var playerElm = PlanetSwitcher.GetPlayerTexture(col);

        Destroy(model);
        model = Instantiate(playerElm.model);
        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one * 8f;
        model.transform.localRotation = Quaternion.Euler(0, 0, -90);

        if(playerLabel != null) playerLabel.GetComponent<Text>().color = col;
    }

    #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if(stream.IsWriting) stream.SendNext(respawnDelay);
            else respawnDelay = (float)stream.ReceiveNext();
        }

        public override void OnEnable() {
            base.OnEnable();

            fuelMeter = maxFuel;

            colliders = GetComponentsInChildren<Collider2D>();
            exhaustSound = GetComponent<AudioSource>();
            transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);

            var playerNameTag = Instantiate(Resources.Load("PlayerName"), transform.position, Quaternion.identity) as GameObject;
            playerName = playerNameTag.GetComponent<PlayerName>();
            if(playerName != null && photonView.Owner != null) playerName.SetHost(gameObject, photonView.Owner.NickName);
            if(photonView.Owner != null) PLAYERNAME = photonView.Owner.NickName;
            playerLabel = playerNameTag;

            //REMOTE PLAYERS (Non-controllable from client)
            if(!photonView.IsMine) {
                exLastPos = transform.position; //For movement/exhaust particles
                foreach(var i in networkIgnore) if(i != null) DestroyImmediate(i);
            }

            playerNumber = photonView.ViewID;
            GameManager.ClaimPlanet(this);

            if(playerName != null) playerName.SetHost(gameObject, photonView.Owner.NickName);
        }

        public override void OnDisable() {
            if(playerName != null) Destroy(playerName.gameObject);
            if(planet != null) ClearHomePlanet();
            base.OnDisable();
        }
    #endregion

    public void PositionToPlanet() {
        if(planet != null) transform.position = photonView.transform.position = new Vector3(planet.transform.position.x, planet.transform.position.y, -1.1f);
    }
    public void LerpToPlanet() {
        if(planet == null) return;
        transform.position = photonView.transform.position = Vector3.Lerp(transform.position, new Vector3(planet.transform.position.x, planet.transform.position.y, -1.1f), Time.deltaTime);
    }

    [PunRPC]
    public void ReadyPlayer(int viewID) {
        if(viewID == photonView.ViewID) GameManager.ReadyNewPlayer();
    }

    public bool CanHold() {
        return trailingObjects.Count < maxAsteroids;
    }

    public void Destroy() {
        if(trailingObjects.Count > 0) {
            var asteroid = trailingObjects[0];
            trailingObjects.RemoveAt(0);
            if(asteroid.rb != null) {
                asteroid.rb.constraints = RigidbodyConstraints2D.None;
                asteroid.rb.velocity = rb.velocity / throwingReduction; 
            }
            SetCollision(asteroid.GetCollider2D(), false);
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            asteroid.photonView.RPC("ReleaseAsteroid", RpcTarget.All, true, asteroid.photonView.ViewID); 
        }
        Destroy(model);
        Destroy(rb);
        foreach(var i in colliders) Destroy(i);
        Destroy(exhaustSound);
        Destroy(exhaust);
        foreach(Transform t in transform) if(t == transform) Destroy(t.gameObject);
        Destroy(this);
    }

    public void Explode(float exp) {
        if(planet != null) planet.Explode(exp);
        flicker = 1;
        respawnDelay = respawningTime;

        for(int i = 0; i < trailingObjects.Count; i++) {
            trailingObjects[i].ForceRelease(true);
            trailingObjects.RemoveAt(i);
            i--;
        }
        
        ///////////////KATALYSATOR CODE HERE
    }

    public void SetLabel(PlayerName name, string nameTag) {
        this.playerName = name;
        this.playerName.SetHost(gameObject, nameTag);
    }

    public void ForceColor(float r, float g, float b) {
        var col = new Color(r, g, b);
        playerColor = col;
        SetTextureByPlanet(playerColor);
        var settings = exhaust.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(col);
        if(playerName != null) playerName.SetHost(gameObject, photonView.Owner.NickName);
    }

    public void SetCollision(Collider2D asteroid, bool state) {
        foreach(var i in colliders) if(!i.isTrigger) Physics2D.IgnoreCollision(i, asteroid, !state);
    }

    //Lijn aan asteroids/objecten achter de speler
    [HideInInspector] public List<PickupableObject> trailingObjects = new List<PickupableObject>();

   public static void SetName(string name) {
       PLAYERNAME = name;
   }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        baseVelocity = maxVelocity;
        baseStopDrag = stopDrag;
        baseDefaultDrag = defaultDrag;
  //      var cont = GameObject.FindGameObjectWithTag("CUSTOM CONTROLLER");
    //    if(cont != null) customController = cont.GetComponent<CustomController>();
      //  if(customController != null && customController.useCustomControls) hookShot.customController = customController;
        maxVelocity = defaultVelocity;
        boostCooldownTimer = boostCooldownDuration;
    }

    void FixedUpdate() {
        maxVelocity = Mathf.Lerp(maxVelocity, defaultVelocity, Time.deltaTime * 2f);

        if(respawnDelay > 0) {
            LerpToPlanet();
            respawnDelay -= Time.deltaTime;

            flicker += Time.deltaTime;
            if(flicker > 0.2f) {
                model.SetActive(!model.activeSelf);
                flicker = 0;
            }
        } else model.SetActive(true);

        if(IsThisClient() && respawnDelay <= 0) {
            ProcessInputs();
            if(rb != null) {
                rb.AddForce(transform.up * velocity);
                rb.rotation = turn;
            }
        }

        if(dropAsteroid && trailingObjects.Count > 0 && respawnDelay <= 0 && hookDelay <= 0) {
            var asteroid = trailingObjects[0];
            trailingObjects.RemoveAt(0);
            if(asteroid.rb != null) {
                if(asteroid.tag == "InfectroidTutorial") playerTutorial.tutorialStepsByName["Infectroid"].completed = true;
                asteroid.rb.constraints = RigidbodyConstraints2D.None;
                asteroid.throwed = true;
                asteroid.rb.AddForce(transform.up * throwForce);
            }
            SetCollision(asteroid.GetCollider2D(), false);
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            if(asteroid.photonView.ViewID > 0) asteroid.photonView.RPC("ReleaseAsteroid", RpcTarget.All, true, asteroid.photonView.ViewID); 
            else asteroid.ReleaseAsteroid(true, asteroid.photonView.ViewID);
            dropAsteroid = false;
        }
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.R) && planet != null) planet.AddingResource(5); /////////////////////////////////////////////////////////////////////////////////////////  DEBUG

        BoostManager();

        if(!GameManager.GAME_STARTED) PositionToPlanet();
        exhaustSound.volume = Mathf.Lerp(exhaustSound.volume, IsThrust() ? 0.05f : 0, Time.deltaTime * 10f);

        if(hookDelay > 0) hookDelay -= Time.deltaTime;
        if(ReleaseAsteroidKey() && trailingObjects.Count > 0 && respawnDelay <= 0) {
            AudioManager.PLAY_SOUND("collect");
            dropAsteroid = true;
            hookShot.DelayShoot();
        }

        //Particles emitten wanneer movement   ||   Exhaust
        if(IsThisClient()) {
            var emitting = exhaust.emission;
            emitting.enabled = IsThrust();
        } else {
            if(exLastTime > 0) exLastTime -= Time.deltaTime;
            else {
                exLastPos = transform.position;
                exLastTime = 0.25f;
            }
            var emitting = exhaust.emission;
            bool shouldEmit = Mathf.Abs(Vector3.Distance(exLastPos, transform.position)) > 0.04f;
            emitting.enabled = shouldEmit;
        }
        exhaustLight.color = exhaust.main.startColor.color;
        exhaustLight.enabled = exhaust.emission.enabled;

        if(!photonView.IsMine && PhotonNetwork.IsConnected) return;

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
                trailingObjects[i].transform.localScale = Vector3.Lerp(trailingObjects[i].transform.localScale, Vector3.one * heldResourceScaleFactor, Time.deltaTime * 2f);
                trailingObjects[i].transform.position = Vector3.Lerp(trailingObjects[i].transform.position, (transform.position - (transform.up * (i + 1) * 0.5f)), Time.deltaTime * trailingSpeed);
            }
    }

    protected void BoostManager() {
        if (Input.GetKeyDown(KeyCode.LeftShift)) BoostPlayer();

        if (isBoosting) {
            boostTimer += Time.deltaTime;

            if (boostTimer >= boostDuration) {
                canBoost = false;
                BoostPlayer();

                boostCooldownTimer = 0f;
                boostTimer = 0f;
            }
        } 
        else if (!canBoost) BoostCooldown();
    }
    private void BoostPlayer() {
        if (canBoost) {
            if(rb != null) rb.AddForce(transform.up * boostForce);
            isBoosting = true;
        }
        else if (!canBoost) isBoosting = false;
    }

    private void BoostCooldown() {
        boostCooldownTimer += Time.deltaTime;
        var settings = exhaust.main;
        var switchcol = Color.white;
        settings.startColor = new ParticleSystem.MinMaxGradient(switchcol);

        if (boostCooldownTimer >= boostCooldownDuration)
        {
            canBoost = true;
            settings.startColor = new ParticleSystem.MinMaxGradient(playerColor);
        }
    }

    public bool ReleaseAsteroidKey() {
        return Input.GetKeyDown(KeyCode.Space);
    }

    void ProcessInputs() {
        //naar voren en naar achteren (W & S)
        if(IsThrust()) {
            velocity = Mathf.Lerp(velocity, maxVelocity, Time.deltaTime * acceleration);
            if(rb != null) rb.drag = defaultDrag;
        }
        else {
            velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * acceleration * 2f);
            if(rb != null) rb.drag = stopDrag;
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
        var script = obj.GetComponent<PickupableObject>();
        if(script != null) trailingObjects.Add(script);
    }
    public void RemoveAsteroid(GameObject obj) {
        for(int i = 0; i < trailingObjects.Count; i++) {
            if(trailingObjects[i] == obj) {
                trailingObjects[i].photonView.RPC("ReleaseAsteroid", RpcTarget.All, true, trailingObjects[i].photonView.ViewID);
                trailingObjects.RemoveAt(i);
                return;
            }
        }
    }

    public bool IsThisClient() {
        return photonView != null && photonView.IsMine;
    }

    public bool CanCastHook() {
        return respawnDelay <= 0 /* && GameManager.GAME_STARTED*/ && !hookShot.HasObject();
    }
    [PunRPC]
    public void CastHook(int viewID) {
        if(!CanCastHook() && !hookShot.IsDelayingHook()) return;
        if(photonView.ViewID == viewID) hookShot.CastHook();
        hookDelay = 0.5f;
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
