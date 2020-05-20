using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;

public class PlayerTutorial : MonoBehaviour {
    public GameObject resourcePrefab, infectroidPrefab;

    private LineRenderer line;
    private Text text;
    public Image icon;

    [Range(0, 50)]
    public int segments = 50;
    private float radius = 0;

    [Header("Tutorial Vars")]
    public float resourceOrbitSpeed = 0.5f;

    public PlayerShip host;
    public Image[] images;

    [System.Serializable]
    public class TutorialPiece {
        public string tutorialName;
        [Header("VISUALS")]
        public string text;
        public Sprite image;
        public Vector3 imagePos, textPos;

        [Header("TIMING")]
        public float duration = 3;
        public bool completeAfterDuration = false;
        [HideInInspector] public bool completed = false;
    
        [Header("EVENTS")]
        public UnityEvent tutorialEvent;
    }
    [Space(10)]
    public TutorialPiece[] tutorialSteps;
    public int tutorialProgress = 0;
    private float tutorialTimer = 0;

    private float resourceTick = 0, infectroidTick = 0;

    private Asteroid tutorialResource;
    private Infectroid tutorialInfectroid;
    private Vector3 textPos;

    public static Dictionary<string, TutorialPiece> tutorialStepsByName = new Dictionary<string, TutorialPiece>();

    void Start() {
        text = GetComponentInChildren<Text>();
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        text.color = new Color(1, 1, 1, 1);

        foreach(var i in tutorialSteps) tutorialStepsByName.Add(i.tutorialName, i);
    }

    void Update() {
        TutorialTick();

        var sine = Mathf.Sin(Time.time * 7f);
        text.transform.rotation = Quaternion.identity;
        text.transform.position = new Vector3(transform.position.x + textPos.x + 2.75f + sine / 7f, transform.position.y + textPos.y + 0.5f, 0);

        var targetCol = new Color(1, 1, 1, radius / 100f);
        foreach(var i in images) i.color = targetCol;
        text.color = targetCol;

        if(!GameManager.GAME_STARTED && host.photonView.IsMine) {
            line.enabled = text.enabled = true;
            radius = Mathf.Lerp(radius, 6500 + sine * 1000f, Time.deltaTime * 3f);
            CreatePoints(radius);
        } else {
            radius = Mathf.Lerp(radius, 0, Time.deltaTime * 3f);
            if(radius <= 45) line.enabled = text.enabled = false;
            else CreatePoints(radius);
        }
    }

    private void TutorialTick() {
        if(tutorialProgress < tutorialSteps.Length) {
            var curTutorial = tutorialSteps[tutorialProgress];
            text.text = "<size=110>" + curTutorial.text.Replace("<br>", "\n") + "</size>";
            if(curTutorial.image != null) {
                icon.sprite = curTutorial.image;
                icon.transform.localPosition = curTutorial.imagePos;
                icon.enabled = true;
            } else icon.enabled = false;

            tutorialTimer += Time.deltaTime;
            if(tutorialTimer > curTutorial.duration && (curTutorial.completeAfterDuration || curTutorial.completed)) {
                tutorialProgress++;
                if(tutorialProgress < tutorialSteps.Length) tutorialSteps[tutorialProgress].tutorialEvent.Invoke();
                tutorialTimer = 0;
            }
        }

        //Tuturial Resource
        if(tutorialResource != null && !tutorialResource.held) {
            resourceTick += Time.deltaTime;    
            tutorialResource.transform.position = GetOrbit(host.planet.transform.position, Mathf.Clamp(resourceTick, 0, 1), resourceTick * resourceOrbitSpeed);
        }
        if(tutorialInfectroid != null && !tutorialInfectroid.held) {
            infectroidTick += Time.deltaTime;    
            tutorialInfectroid.transform.position = GetOrbit(host.planet.transform.position, Mathf.Clamp(infectroidTick, 0, 0.9f), infectroidTick * resourceOrbitSpeed);
        }
    }

     void CreatePoints(float radius) {
        float x;
        float y;
        float angle = 20f;
        for(int i = 0; i < (segments + 1); i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / segments);
        }
    }

    public void SpawnTutorialResource() {
        if(GameManager.SkipCountdown()) return;

        var obj = Instantiate(resourcePrefab);
        tutorialResource = obj.GetComponent<Asteroid>();
        tutorialResource.transform.position = transform.position;
        tutorialResource.tag = "ResourceTutorial";
        tutorialResource.value = 10;
    }

    public void SpawnTutorialInfectroid() {
        if(GameManager.SkipCountdown()) return;

        var obj = Instantiate(infectroidPrefab);
        tutorialInfectroid = obj.GetComponent<Infectroid>();
        tutorialInfectroid.transform.position = transform.position;
        tutorialInfectroid.tag = "InfectroidTutorial";
        host.planet.currentScore = 10;
        line.startColor = line.endColor = host.playerColor;
    }

    public void ResetOrbitColor() {
        line.startColor = line.endColor = Color.white;
        host.photonView.RPC("ReadyPlayer", RpcTarget.MasterClient, host.photonView.ViewID);
    }

    private Vector3 GetOrbit(Vector3 center, float radius, float angle) {
        var ang = angle * 360f;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }
}