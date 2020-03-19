using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerShip : MonoBehaviourPunCallbacks, IPunObservable {
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

    public float trailingSpeed = 8f;

    public bool isSingePlayer = false; 

    [Range(0.1f,10)]
    public float throwingReduction = 1f; 

    //Voor het herkennen van de local player (die jij speelt) ivm network
    public static GameObject LocalPlayerInstance;

    //private string PlayerName = "";

    #region IPunObservable implementation
        public override void OnEnable() {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            //if(stream.IsWriting) stream.SendNext(PlayerName);
            //else if(stream.IsReading) PlayerName = stream.ReceiveNext().ToString();
        }
    #endregion

    //Lijn aan asteroids/objecten achter de speler
    [HideInInspector] public List<Asteroid> trailingObjects = new List<Asteroid>();

    private ParticleSystem exhaust;

    #region Network player spawning
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode) {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }

    void OnLevelWasLoaded(int level) {
        this.CalledOnLevelWasLoaded(level);
    }

    void CalledOnLevelWasLoaded(int level) {
        //if(!Physics.Raycast(transform.position, -Vector3.up, 5f)) {
        //    transform.position = new Vector3(0, 5, 0);
        //}
    }

    public override void OnDisable() {
        base.OnDisable();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    #endregion

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        exhaust = GetComponentInChildren<ParticleSystem>();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);
    }

    void Awake() {


        if(photonView == null) return;
        if(photonView.IsMine) PlayerShip.LocalPlayerInstance = this.gameObject;       
    }

    void Update() {

        if (!isSingePlayer)
        {
            if (photonView == null) return;
            if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
        }

        if(( photonView != null && photonView.IsMine) || isSingePlayer) {
            ProcessInputs();
            rb.AddForce(transform.up * velocity);
            rb.rotation = turn;
        }

        if (Input.GetKeyDown(KeyCode.F) && trailingObjects.Count > 0)
        {
            var asteroid = trailingObjects[0];
            trailingObjects.RemoveAt(0);
            
            asteroid.transform.TransformDirection(new Vector2(transform.forward.x * asteroid.transform.forward.x, transform.forward.y * asteroid.transform.forward.y));
            asteroid.rb.velocity = rb.velocity / throwingReduction; 
            asteroid.ReleaseAsteroid(); 
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
        //Particles emitten wanneer movement
        var emit = exhaust.emission;
        emit.enabled = IsThrust();

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
        rb.velocity /= exitVelocityReduction;
    }

    //Voegt object toe aan trail achter player
    public void AddAsteroid(GameObject obj) {
        var script = obj.GetComponent<Asteroid>();
        if(script != null) trailingObjects.Add(script);
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
