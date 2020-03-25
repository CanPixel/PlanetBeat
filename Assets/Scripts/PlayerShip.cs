using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviourPunCallbacks {
    public bool isSinglePlayer {
        get {return GameManager.instance != null && GameManager.instance.isSinglePlayer;}
    } 

    public Sprite emit, noEmit;
    public Image ship;

    private CustomController customController;

    [Space(10)]
    #region MOVEMENT
        public float maxVelocity = 5;
        public float acceleration = 0.1f;

        [Range(1, 20)]
        public float turningSpeed = 2.5f;
        [Range(1, 10)]
        public float exitVelocityReduction = 2;
        [Range(1, 5)]
        public float brakingSpeed = 1;
    #endregion

    //Rigidbody reference voor physics en movement hoeraaa
    private Rigidbody2D rb;
    private float velocity, turn;

    public Component[] networkIgnore;

    public float trailingSpeed = 8f;

    [Range(0.1f,10)]
    public float throwingReduction = 1f; 

    //Voor het herkennen van de local player (die jij speelt) ivm network
    public static GameObject LocalPlayerInstance;
    public static string PLAYERNAME;

    private Vector3 exLastPos;
    private float exLastTime;

    public HookShot hookShot;

    #region IPunObservable implementation
        public override void OnEnable() {
            base.OnEnable();
            transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);

            if(!isSinglePlayer && photonView != null && !photonView.IsMine) {
                exLastPos = transform.position;
                var playerNameTag = Instantiate(Resources.Load("PlayerName"), transform.position, Quaternion.identity) as GameObject;
                var pN = playerNameTag.GetComponent<PlayerName>();
                pN.SetHost(gameObject, photonView.Owner.NickName);
                PLAYERNAME = photonView.Owner.NickName;
                GameManager.playerLabels.Add(PLAYERNAME, playerNameTag);
                foreach(var i in networkIgnore) if(i != null) DestroyImmediate(i);
            } 
        }
    #endregion

    //Lijn aan asteroids/objecten achter de speler
    [HideInInspector] public List<Asteroid> trailingObjects = new List<Asteroid>();
    private ParticleSystem exhaust;

    public override void OnDisable() {
        base.OnDisable();
    }

   public static void SetName(string name) {
       PLAYERNAME = name;
   }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        exhaust = GetComponentInChildren<ParticleSystem>();
        var cont = GameObject.FindGameObjectWithTag("CUSTOM CONTROLLER");
        if(cont != null) customController = cont.GetComponent<CustomController>();
        if(customController != null && customController.useCustomControls) hookShot.customController = customController;
    }

    void Awake() {
        if(IsThisClient()) PlayerShip.LocalPlayerInstance = this.gameObject;       
    }

    void Update() {
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

        if(IsThisClient() || isSinglePlayer) {
            ProcessInputs();
            if(rb != null) {
                rb.AddForce(transform.up * velocity);
                rb.rotation = turn;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && trailingObjects.Count > 0) {
            var asteroid = trailingObjects[0];
            trailingObjects.RemoveAt(0);
            
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            asteroid.rb.velocity = rb.velocity / throwingReduction; 
            asteroid.ReleaseAsteroid(); 
        }

        //Removes asteroids that got destroyed / eaten by the sun
        for(int i = 0; i < trailingObjects.Count; i++) if(trailingObjects[i] == null) {
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
        if(IsThrust()) velocity = Mathf.Lerp(velocity, maxVelocity, Time.deltaTime * acceleration);
        else velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * acceleration * 2f);

        //Spreekt voor zich
        if(IsBrake()) {
            velocity = 0;
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * brakingSpeed);
        }

        if(IsTurningLeft()) turn += turningSpeed * Time.deltaTime * 50f;
        if(IsTurningRight()) turn -= turningSpeed * Time.deltaTime * 50f;
    }

    //Wanneer je orbits exit, de speed dampening.
    public void NeutralizeForce() {
        if(rb != null) rb.velocity /= exitVelocityReduction;
    }

    //Voegt object toe aan trail achter player
    public void AddAsteroid(GameObject obj) {
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
