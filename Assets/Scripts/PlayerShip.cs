using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviourPunCallbacks, IPunObservable {
    public PlayerHighlighter playerHighlighter;
    public HookShot hookShot;
    public ParticleSystem exhaust;

    [HideInInspector] public GameObject playerLabel;
    [HideInInspector] public Collider2D[] colliders;

    [Header("BOOST")]
    public float boostDuration = 0.2f;
    public float boostVelocity = 22f;
    public float boostVelocity2 = 22f; 
    public float boostCooldownDuration = 3;
    private float boostCooldownTimer = 0f, boostTimer = 0f;
    public bool canBoost = true;
    public bool isBoosting;
    private bool onCooldown;
  
    
    public float fuelMeter;
    public float maxFuel;
    public float minFuel;
    public float waitForCooldown;
    public float cooldownPenalty; 

    public bool hitBottem;

    public Image fuelBar;
    public Image fuelBarBackground;

    public bool boost1;
    public bool boost2; 

        


    [Header("PLAYER VALUES")]
    public Image ship;
    public int playerNumber;
    public Color playerColor;
    [Space(10)]
//    private CustomController customController;
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

        [Range(1, 10)]
        public int maxAsteroids = 2;
    #endregion

    //Rigidbody reference voor physics en movement hoeraaa
    private Rigidbody2D rb;
    private float velocity, turn;
    [Header("GRAPPLE")]
    public float trailingSpeed = 8f;

    [Range(0.1f,10)]
    public float throwingReduction = 1f; 

    public Vector3 SpeedShardOffset;
    public GameObject SpawnStarShard;
    public float ShardTimeInterval = 1.1f;
    public bool canShard = false;

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
    }

    [PunRPC]
    public void ClearHomePlanet() {
        if(planet != null && photonView != null) planet.ResetPlanet();
        planet = null;
    }

    public void SetTexture(TextureSwitcher.TexturePack pack) {
        ship.sprite = pack.Ship[TextureSwitcher.GetPlayerTintIndex(photonView.ViewID)].src;
        ship.SetNativeSize();
    }

    #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if(stream.IsWriting) {
                stream.SendNext(respawnDelay);
            } else {
                respawnDelay = (float)stream.ReceiveNext();
            }
        }

        public override void OnEnable() {
            base.OnEnable();

            colliders = GetComponentsInChildren<Collider2D>();
            exhaustSound = GetComponent<AudioSource>();
            transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);

            //REMOTE PLAYERS (Non-controllable from client)
            if(!photonView.IsMine) {
                exLastPos = transform.position; //For movement/exhaust particles
                var playerNameTag = Instantiate(Resources.Load("PlayerName"), transform.position, Quaternion.identity) as GameObject;
                playerName = playerNameTag.GetComponent<PlayerName>();
                if(playerName != null && photonView.Owner != null) playerName.SetHost(gameObject, photonView.Owner.NickName);
                if(photonView.Owner != null) PLAYERNAME = photonView.Owner.NickName;
                playerLabel = playerNameTag;

                //Name tag replacement
                /* if(playerNameTag != null) {
                    if(!GameManager.playerLabels.ContainsKey(PLAYERNAME)) GameManager.playerLabels.Add(PLAYERNAME, playerNameTag);
                    else {
                        string replacementName = PLAYERNAME + Random.Range(0, 1000);
                        photonView.Owner.NickName = replacementName;
                        playerName.Rename(replacementName);
                        GameManager.playerLabels.Add(replacementName, playerNameTag);
                    }
                }*/

                foreach(var i in networkIgnore) if(i != null) DestroyImmediate(i);
            }

            playerColor = TextureSwitcher.GetPlayerTint(photonView.ViewID);
            playerNumber = photonView.ViewID;
            if(playerLabel != null) playerLabel.GetComponent<Text>().color = playerColor;
            ForceColor(playerColor);
            GameManager.ClaimPlanet(this);
        }

        public override void OnDisable() {
            if(playerName != null) Destroy(playerName.gameObject);
            if(planet != null) ClearHomePlanet();
            base.OnDisable();
        }
    #endregion

    public void PositionToPlanet() {
        if(planet != null) transform.position = photonView.transform.position = planet.transform.position;
    }
    public void LerpToPlanet() {
        if(planet == null) return;
        transform.position = photonView.transform.position = Vector3.Lerp(transform.position, planet.transform.position, Time.deltaTime);
    }

    public bool CanHold() {
        return trailingObjects.Count < maxAsteroids;
    }

    public void Destroy() {
        Destroy(ship);
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

    public void ForceColor(Color col) {
        ForceColor(col.r, col.g, col.b);
    }
    public void ForceColor(float r, float g, float b) {
        var col = new Color(r, g, b);
        playerColor = col;
        if(planet != null) planet.AssignPlayer(this);
        SetTexture(TextureSwitcher.GetCurrentTexturePack());
        var settings = exhaust.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(col);
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

        fuelBar = GameObject.Find("FuelBar").GetComponent<Image>(); 
        fuelBarBackground = GameObject.Find("FuelBarBackground").GetComponent<Image>(); 
      

  //      var cont = GameObject.FindGameObjectWithTag("CUSTOM CONTROLLER");
    //    if(cont != null) customController = cont.GetComponent<CustomController>();
      //  if(customController != null && customController.useCustomControls) hookShot.customController = customController;
        maxVelocity = defaultVelocity;
        boostCooldownTimer = boostCooldownDuration;

        fuelBar.color = playerColor; 
    }

    void FixedUpdate() {
        maxVelocity = Mathf.Lerp(maxVelocity, defaultVelocity, Time.deltaTime * 2f);

        if(respawnDelay > 0) {
            ship.color = Color.Lerp(ship.color, new Color(ship.color.r, ship.color.g, ship.color.b, 0.25f), Time.deltaTime * 8f);

            LerpToPlanet();
            respawnDelay -= Time.deltaTime;

            flicker += Time.deltaTime;
            if(flicker > 0.2f) {
                ship.enabled = !ship.enabled;
                flicker = 0;
            }
        } else {
            ship.color = Color.Lerp(ship.color, new Color(ship.color.r, ship.color.g, ship.color.b, 1), Time.deltaTime * 4f);
            ship.enabled = true;
        }

        if(IsThisClient() && respawnDelay <= 0) {
            ProcessInputs();
            if(rb != null) {
                rb.AddForce(transform.up * velocity);
                rb.rotation = turn;
            }
        }

        if(dropAsteroid && trailingObjects.Count > 0 && respawnDelay <= 0) {
            var asteroid = trailingObjects[0];
            trailingObjects.RemoveAt(0);
            if(asteroid.rb != null) {
                asteroid.rb.constraints = RigidbodyConstraints2D.None;
                asteroid.rb.velocity = rb.velocity / throwingReduction; 
            }
            SetCollision(asteroid.GetCollider2D(), false);
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            asteroid.photonView.RPC("ReleaseAsteroid", RpcTarget.All, true, asteroid.photonView.ViewID); 
            dropAsteroid = false;
        }
    }

    void Update() {
        if(boost1) BoostManager();
        else if(boost2) BoostManager2(); 

        if(!GameManager.GAME_STARTED) PositionToPlanet();
        exhaustSound.volume = Mathf.Lerp(exhaustSound.volume, IsThrust() ? 0.05f : 0, Time.deltaTime * 10f);

        //Soft borders
        var DistFromCenter = Vector2.Distance(Camera.main.WorldToScreenPoint(transform.position), new Vector2(Screen.width, Screen.height) / 2f);
        DistFromCenter /= 200f;
        //maxVelocity = Mathf.Lerp(maxVelocity, baseVelocity * Mathf.Clamp(1.75f - DistFromCenter, 0.2f, 1), Time.deltaTime * 4f);
        //stopDrag = Mathf.Lerp(stopDrag, baseStopDrag * Mathf.Clamp(4f - DistFromCenter, 0.1f, 1), Time.deltaTime * 4f);
        //defaultDrag = Mathf.Lerp(defaultDrag, baseDefaultDrag * Mathf.Clamp(4f - DistFromCenter, 0.1f, 1), Time.deltaTime * 4f);

        if(ReleaseAsteroidKey() && trailingObjects.Count > 0 && respawnDelay <= 0) {
            AudioManager.PLAY_SOUND("collect");
            dropAsteroid = true;
        }

        //Particles emitten wanneer movement
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

        if(!photonView.IsMine && PhotonNetwork.IsConnected) return;

        //Removes asteroids that got destroyed / eaten by the sun
        for(int i = 0; i < trailingObjects.Count; i++) if(trailingObjects[i] == null) {
            trailingObjects.RemoveAt(i);
            i--;
        }

        //Removes asteroids owned by other players
        for(int i = 0; i < trailingObjects.Count; i++) if(!trailingObjects[i].IsOwnedBy(this)) {
            //trailingObjects[i].ReleaseAsteroid(true);
            trailingObjects.RemoveAt(i);
            i--;
        }

        //Trailing object positions & (stiekem) een kleinere scaling, anders waren ze wel fk bulky
        for (int i = 0; i < trailingObjects.Count; i++)
            if(trailingObjects[i].held) {
                trailingObjects[i].transform.localScale = Vector3.Lerp(trailingObjects[i].transform.localScale, Vector3.one * 0.09f, Time.deltaTime * 2f);
                trailingObjects[i].transform.position = Vector3.Lerp(trailingObjects[i].transform.position, (transform.position - (transform.up * (i + 1) * 0.5f)), Time.deltaTime * trailingSpeed);
            }
    
        //Grapple Cooldown Code
        if(trailingObjects.Count > 0 && trailingObjects[0].dropBoosts && canShard) {
            ShardTimeInterval += Time.deltaTime;
            if(ShardTimeInterval >= 0.5f) {
                ShardTimeInterval = 0;
                PhotonNetwork.Instantiate("SpeedShard", SpawnStarShard.transform.position, transform.rotation); 
            }    
        }
    }

    [PunRPC]
    public void SynchBoost(int viewID)
    {
        fuelBar.enabled = fuelBarBackground.enabled != (photonView.IsMine || viewID == photonView.ViewID); 
    }
    protected void BoostManager()
    {
        photonView.RPC("SynchBoost", RpcTarget.All, photonView.ViewID); 
        //fuelBar.enabled = fuelBarBackground.enabled = (photonView.IsMine); 

        // fuelBar.transform.localScale = new Vector3(fuelMeter / maxFuel, .15f);    
        fuelBar.fillAmount = fuelMeter / maxFuel;


        if (Input.GetKey(KeyCode.LeftShift) && canBoost) BoostPlayer(true); // Start boost
        else StartCoroutine("BoostRecharge");

        if ((Input.GetKeyUp(KeyCode.LeftShift) || fuelMeter < 0) && isBoosting)
        {
            if (fuelMeter <= 0) //Fuel meter gets completely drained 
            {
                fuelBarBackground.GetComponent<Image>().color = new Color(255, 0, 0);
                waitForCooldown = cooldownPenalty;
            }
            BoostPlayer(false);
        }
    }

    
    private void BoostPlayer(bool boosting)
    {
        fuelBar.fillAmount = fuelMeter / maxFuel;
        if (boosting)
        {
            maxVelocity = boostVelocity;
            fuelMeter -= Time.deltaTime; //take fuel away       
            isBoosting = true;

        }
        else
        {
            canBoost = false;
            maxVelocity = defaultVelocity;
            StartCoroutine("BoostRecharge");
        }
    }

    IEnumerator BoostRecharge()
    {
        isBoosting = false;
        maxVelocity = defaultVelocity;

        if (fuelMeter < maxFuel)
        {
            yield return new WaitForSeconds(waitForCooldown);

            fuelBarBackground.GetComponent<Image>().color = new Color(255, 255, 255);
            fuelMeter += Time.deltaTime;
            canBoost = true;
            waitForCooldown = 1f;
        }
        else StopCoroutine("BoostRecharge");
    }



    protected void BoostManager2()
    {
        //fuelBar.fillAmount = boostDuration / boostTimer; 
       
        if (Input.GetKeyDown(KeyCode.LeftShift)) BoostPlayer2();

        if (isBoosting)
        {
            boostTimer += Time.deltaTime;

            if (boostTimer >= boostDuration)
            {
                canBoost = false;
                BoostPlayer2();
                boostTimer = 0f;
                boostCooldownTimer = 0f;
                onCooldown = true;
            }
        }

        if (onCooldown)
        {
            boostCooldownTimer += Time.deltaTime;
            // fuelBar.color = Color.white;
            var settings = exhaust.main;
            var switchcol = Color.white; 
            settings.startColor = new ParticleSystem.MinMaxGradient(switchcol);

            if (boostCooldownTimer >= boostCooldownDuration)
            {
                canBoost = true;
                onCooldown = false;
                // fuelBar.color = playerColor; 
                settings.startColor = new ParticleSystem.MinMaxGradient(playerColor);
            }
        }
    }

    private void BoostPlayer2()
    {
        if (canBoost)
        {
            maxVelocity = boostVelocity2;
            isBoosting = true;
            //boostcode
        }
        else if (!canBoost)
        {
            isBoosting = false;
            maxVelocity = defaultVelocity;
        }
    }


    void OnTriggerStay2D(Collider2D collision) {
        if(collision.gameObject.tag == "SPEEDSHARD") {
            maxVelocity = boostVelocity;
            GameManager.DESTROY_SERVER_OBJECT(collision.gameObject);
            Destroy(collision.gameObject);
        }
    }

public bool ReleaseAsteroidKey() {
        return Input.GetKeyDown(KeyCode.F) | Input.GetKeyDown(KeyCode.E) | Input.GetKeyDown(KeyCode.R) | Input.GetKeyDown(KeyCode.C);
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
 
        return respawnDelay <= 0 && GameManager.GAME_STARTED;
        
    }

    [PunRPC]
    public void CastHook(int viewID) {
        if(!CanCastHook()) return;
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
