using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviourPunCallbacks, IPunObservable {
    public PlayerTutorial playerTutorial;
    public HookShot hookShot;
    public ParticleSystem exhaust;

    public ParticleSystem exhaustDefault;               //Bradley
    public GameObject exhaustDefaultGameObject;         //Bradley

    public Light exhaustLight;
    public GameObject model;

    [HideInInspector] public GameObject playerLabel;
    [HideInInspector] public Collider2D[] colliders;

    public float boostShipRotate = 10f;

    [Header("BOOST")]
    public float boostDuration = 0.12f;
    public float boostVelocity = 23;
    public float boostForce = 230;
    public float boostCooldownDuration = 3.5f;
    private float boostCooldownTimer = 0f, boostTimer = 0f;
    private bool canBoost = true;
    private float fuelMeter;
    public float maxFuel = 0.8f;
    private bool isBoosting;
    private bool onCooldown;
    private float waitForCooldown;
    public float cooldownPenalty = 3.5f;

    // Bradley
    public Animator boostAnimator;
    public imageSwitcher imageSwitcherScript;

    private Vector3 baseScale;

    [Space(10)]
    public Component[] networkIgnore;

    [HideInInspector] public int playerNumber;
    [HideInInspector] public Color playerColor;

    #region MOVEMENT
        private float maxVelocity = 5;
        private float baseVelocity;
        [Header("PHYSICS")]
        public float acceleration = 25f;

        public float defaultVelocity = 4.0f;

        [Range(1, 20)]
        public float turningSpeed = 4.6f;
        [Range(1, 5)]
        public float brakingSpeed = 1.3f;

        public float respawningTime = 3;

        public float defaultDrag = 2;
        public float stopDrag = 1.5f;
        private float baseStopDrag, baseDefaultDrag;
        private float hookDelay;

        [Range(1, 10)]
        public int maxAsteroids = 1;
    #endregion

    //Rigidbody reference voor physics en movement hoeraaa
    private Rigidbody2D rb;
    private float velocity, turn;
    [Header("GRAPPLE")]
    public float trailingSpeed = 15f;
    public float throwForce = 34;

    [Range(0.1f,10)]
    public float throwingReduction = 0.4f; 

    [Space(5)]
    public float heldResourceScaleFactor = 0.09f;

    public static string PLAYERNAME;

    private Vector3 exLastPos;
    private float exLastTime;

    private float animationRotateSpeed;

    [HideInInspector] public PlayerName playerName;
    [HideInInspector] public PlayerPlanets planet;

    private bool dropAsteroid = false;
    [SerializeField] [HideInInspector] private float respawnDelay = 0;
    private float flicker = 0;

    private bool boostable = false;
    public void ActivateBoosting() {
        boostable = true;
    }
    public void ActivateGrapple() {
        hookShot.ActivateGrapple();
    }

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
        PositionToPlanet();
    }
    [PunRPC]
    public void ClearHomePlanet() {
        if(planet != null && photonView != null) planet.ResetPlanet();
        planet = null;
    }

    public void SetTextureByPlanet(Color col) {
        var playerElm = PlanetSwitcher.GetPlayerTexture(col);

        DestroyImmediate(model.gameObject);
        Destroy(model.gameObject);

        model = Instantiate(playerElm.model);
        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one * 8f;
        model.transform.localRotation = Quaternion.Euler(0, 180, -90);

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

            if(playerName != null && photonView.ViewID > 0) playerName.SetHost(gameObject, photonView.Owner.NickName);
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
        var settingsTwo = exhaustDefault.main;

        settings.startColor = new ParticleSystem.MinMaxGradient(col);                                   // COLOR FIRE
        settingsTwo.startColor = new ParticleSystem.MinMaxGradient(col);                                   // COLOR FIRE

        //gameObject.GetComponent<Renderer>().material.SetColor("_EMISSION", playerColor);

        boostAnimator.SetInteger("boostAnimatie", 1);
        if (playerName != null) playerName.SetHost(gameObject, photonView.Owner.NickName);
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
        GameManager.LIVE_PLAYER_COUNT++;
        rb = GetComponent<Rigidbody2D>();
        baseVelocity = maxVelocity;
        baseStopDrag = stopDrag;
        baseDefaultDrag = defaultDrag;
  //      var cont = GameObject.FindGameObjectWithTag("CUSTOM CONTROLLER");
    //    if(cont != null) customController = cont.GetComponent<CustomController>();
      //  if(customController != null && customController.useCustomControls) hookShot.customController = customController;
        maxVelocity = defaultVelocity;
        boostCooldownTimer = boostCooldownDuration;
        SoundManager.PLAY_SOUND("Exhaust");

        if(GameManager.SkipCountdown()) ActivateBoosting();
    }

    void FixedUpdate() {
        if(transform.position.z != -1.1f) transform.position = new Vector3(transform.position.x, transform.position.y, -1.1f);

        maxVelocity = Mathf.Lerp(maxVelocity, defaultVelocity, Time.deltaTime * 2f);

        if(respawnDelay > 0) {
            LerpToPlanet();
            respawnDelay -= Time.deltaTime;

            flicker += Time.deltaTime;
            if(flicker > 0.2f) {
                if(model != null) model.SetActive(!model.activeSelf);
                flicker = 0;
            }
        } else if(model != null) model.SetActive(true);

        //Rotate animation
        if(boostCooldownTimer < (boostCooldownDuration - 2)) {
            animationRotateSpeed = Mathf.LerpAngle(animationRotateSpeed, (boostShipRotate * ((boostCooldownDuration - 1) - boostCooldownTimer)), Time.deltaTime * 6f); 
            model.transform.Rotate(-animationRotateSpeed, 0, 0);
        } 
        else model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, 180, -90), Time.deltaTime * 2.5f);

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
                if(asteroid.tag == "InfectroidTutorial") playerTutorial.CompleteSubTask("infectroid");

                asteroid.rb.constraints = RigidbodyConstraints2D.None;
                asteroid.throwed = true;
                asteroid.ReleaseAsteroid(true, asteroid.photonView.ViewID);
                if((asteroid.tag == "Powerup" && !(asteroid as Infectroid).inOrbit) || asteroid.tag == "ResourceTutorial" || asteroid.tag == "Resource" || asteroid.tag == "InfectroidTutorial" || ((asteroid as Infectroid).playerPlanets != null && (asteroid as Infectroid).playerPlanets.playerNumber == playerNumber)) asteroid.rb.AddForce(transform.up * throwForce);
            }
            SetCollision(asteroid.GetCollider2D(), false);
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            
            if(asteroid.photonView.ViewID > 0) asteroid.photonView.RPC("ReleaseAsteroid", RpcTarget.All, true, asteroid.photonView.ViewID); 
            else asteroid.ReleaseAsteroid(true, asteroid.photonView.ViewID);
            dropAsteroid = false;
        }
    }

    void Update() {
        //RuntimeManager.StudioSystem.setParameterByName("Exhaust", Exaust);
        
        if(planet != null) {
            if (planet.gameObject == GameObject.Find("PLANETRED")) imageSwitcherScript.SetHandRed();
            else if (planet.gameObject == GameObject.Find("PLANETPINK")) imageSwitcherScript.SetHandPink();
            else if (planet.gameObject == GameObject.Find("PLANETBLUE")) imageSwitcherScript.SetHandBlue();
            else if (planet.gameObject == GameObject.Find("PLANETYELLOW")) imageSwitcherScript.SetHandYellow();
            else if (planet.gameObject == GameObject.Find("PLANETCYAN")) imageSwitcherScript.SetHandCyan();
            else if (planet.gameObject == GameObject.Find("PLANETGREEN")) imageSwitcherScript.SetHandGreen();
        }

        #if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.R) && planet != null) planet.AddingResource(5); /////////////////////////////////////////////////////////////////////////////////////////  DEBUG
            if(Input.GetKeyDown(KeyCode.Q) && planet != null) planet.Explode(5);
        #endif

        BoostManager();

        if(hookDelay > 0) hookDelay -= Time.deltaTime;
        if(ReleaseAsteroidKey() && trailingObjects.Count > 0 && respawnDelay <= 0) {
            SoundManager.PLAY_SOUND("ThrowObject");
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
        
        exhaustLight.color = playerColor;
        exhaustLight.enabled = true;

        if(!photonView.IsMine && PhotonNetwork.IsConnected) return;

        //Removes asteroids that got destroyed / eaten by the sun
        for(int i = 0; i < trailingObjects.Count; i++) if(trailingObjects[i] == null) {
            trailingObjects.RemoveAt(i);
            i--;
        }

        //Removes asteroids owned by other players
        for(int i = 0; i < trailingObjects.Count; i++) if(!trailingObjects[i].IsOwnedBy(this)) {
            trailingObjects[i].ReleaseAsteroid(true, trailingObjects[i].photonView.ViewID);
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && photonView.IsMine && boostable) BoostPlayer();

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
            playerTutorial.CompleteSubTask("boost");
            SoundManager.PLAY_SOUND("Boost");

            boostAnimator.SetInteger("boostAnimatie", 3);                                       // BOOSTING ANIMATIE

            if (rb != null) rb.AddForce(transform.up * boostForce);

            if(!isBoosting) {
                baseScale = model.transform.localScale;
                model.transform.localScale *= 1.25f;
            }
            isBoosting = true;
        }
        else {
            isBoosting = false;
            boostAnimator.SetInteger("boostAnimatie", 1);                                       // EMPTY BOOST ANIMATIE (Niet zichtbaar)
        }
    }
    private void BoostCooldown() {
        boostCooldownTimer += Time.deltaTime;
        var settings = exhaust.main;
        var switchcol = Color.clear;
        
        if (boostCooldownTimer >= 0.9f && boostCooldownTimer < boostCooldownDuration)
        {
            settings.startColor = new ParticleSystem.MinMaxGradient(switchcol);                 // COLOR FIRE
        }

        if (boostCooldownTimer >= boostCooldownDuration) {
            canBoost = true;

            //gameObject.GetComponent<Renderer>().material.SetColor("_EMISSION", playerColor);
            settings.startColor = new ParticleSystem.MinMaxGradient(playerColor);               // COLOR FIRE
            boostAnimator.SetInteger("boostAnimatie", 2);                                       // FULL BOOST ANIMATIE
        } else {
            model.transform.localScale = Vector3.Lerp(model.transform.localScale, baseScale, Time.deltaTime * 2f);
        }
    }

    public bool ReleaseAsteroidKey() {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public void SetLean(float x, float y, float z) {
        model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(x, y, z - 90), Time.deltaTime * 3f);
    }

    void ProcessInputs() {
        //naar voren en naar achteren (W & S)
        if(IsThrust()) {
            velocity = Mathf.Lerp(velocity, maxVelocity, Time.deltaTime * acceleration);
            if(rb != null) rb.drag = defaultDrag;

            //Particle enabled
            exhaustDefaultGameObject.SetActive(true);
        }
        else {
            velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * acceleration * 2f);
            if(rb != null) rb.drag = stopDrag;

            // Particle disabled
            exhaustDefaultGameObject.SetActive(false);
        }

        //Spreekt voor zich
        if(IsBrake()) {
            velocity = 0;
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * brakingSpeed);
        }

        if(IsTurningLeft()) turn += turningSpeed * Time.deltaTime * 50f;
        if(IsTurningRight()) turn -= turningSpeed * Time.deltaTime * 50f;

        if(IsThrust()) playerTutorial.CompleteSubTask("move");
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
        return respawnDelay <= 0 && !hookShot.HasObject();
    }
    [PunRPC]
    public void CastHook(int viewID) {
        if(!CanCastHook() && !hookShot.IsDelayingHook()) return;
        if(photonView.ViewID == viewID) hookShot.CastHook();
        hookDelay = 1f;
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
